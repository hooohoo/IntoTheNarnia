from pathlib import Path
import bpy
import bmesh
import math
import json
import base64
import struct
from mathutils import Vector

BASE=Path(__file__).resolve().parent
OUT=BASE/'Review'
if not OUT.exists(): OUT=BASE.parent
SPEC=json.loads((OUT/'Data/palette.json').read_text())
bpy.ops.wm.read_factory_settings(use_empty=True)
materials={}
for name,h in SPEC['palette'].items():
    rgb=[int(h[i:i+2],16)/255 for i in (0,2,4)]
    m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*rgb,1)
    node=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    linear=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in rgb]
    node.inputs['Base Color'].default_value=(*linear,1)
    node.inputs['Roughness'].default_value=.42 if name.startswith('wardrobe') else .5 if name.startswith('wood') else .38 if name in ['metal','brass'] else .72
    if name in ['metal','brass']:node.inputs['Metallic'].default_value=.5
    materials[name]=m

def mesh_obj(name,verts,faces,mat):
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.data.materials.append(materials[mat]);return o

def cube(s,name):
    x,y,z=s['pos'];w,d,h=s['size']
    bpy.ops.mesh.primitive_cube_add(size=1,location=(x+w/2,y+d/2,z+h/2))
    o=bpy.context.object;o.name=name;o.dimensions=(w,d,h);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(materials[s['mat']])
    if s.get('bevel',0):
        mod=o.modifiers.new('Edge rounding','BEVEL');mod.width=min(s['bevel'],min(w,d,h)*.3);mod.segments=2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return o

# Stage 02 extensions are below the original primitive implementation.

def make_shape(s,name):
    typ=s['type']
    if typ=='box':return cube(s,name)
    if typ=='sphere':
        bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=s['r'],location=s['pos'])
        o=bpy.context.object;o.name=name;o.data.materials.append(materials[s['mat']]);return o
    if typ=='beam':
        a,b=Vector(s['a']),Vector(s['b']);mid=(a+b)*.5;direction=b-a
        if s.get('round'):
            bpy.ops.mesh.primitive_cylinder_add(vertices=10,radius=s['r'],depth=direction.length,location=mid)
        else:
            bpy.ops.mesh.primitive_cube_add(size=1,location=mid)
            bpy.context.object.dimensions=(s['r']*2,s['r']*2,direction.length)
            bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
        o=bpy.context.object;o.name=name;o.rotation_euler=direction.to_track_quat('Z','Y').to_euler();o.data.materials.append(materials[s['mat']])
        if not s.get('round'):
            mod=o.modifiers.new('Beam edge rounding','BEVEL');mod.width=min(.018,s['r']*.22);mod.segments=2;bpy.ops.object.modifier_apply(modifier=mod.name)
        return o
    if typ=='stair':
        n=s['n'];t=s['tread'];r=s['rise'];w=s['width'];dx,dy=s['direction'];origin=s['origin']
        profile=[(0,-.26),(0,0)]
        for i in range(n):profile.extend([(i*t,(i+1)*r),((i+1)*t,(i+1)*r)])
        profile.append((n*t,n*r-.26))
        verts=[]
        for v in [0,w]:
            verts.extend([(origin[0]+dx*u+dy*v,origin[1]+dy*u-dx*v,origin[2]+z) for u,z in profile])
        k=len(profile);faces=[tuple(range(k-1,-1,-1)),tuple(range(k,2*k))]
        faces.extend([(i,(i+1)%k,(i+1)%k+k,i+k) for i in range(k)])
        return mesh_obj(name,verts,faces,s['mat'])
    if typ=='roof':
        x,y,w,d=s['rect'];z=s['base'];peak=s['ridge'];verts=[];faces=[]
        for a,b,za,zb in [(x,x+w/2,z,peak),(x+w/2,x+w,peak,z)]:
            base=len(verts)
            verts.extend([(a,y,za),(b,y,zb),(b,y+d,zb),(a,y+d,za),(a,y,za-.15),(b,y,zb-.15),(b,y+d,zb-.15),(a,y+d,za-.15)])
            faces.extend([tuple(base+i for i in f) for f in [(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]])
        return mesh_obj(name,verts,faces,s['mat'])
    if typ=='arch':
        axis=s['axis'];c=s['center'];line=s['line'];half=s['width']/2;th=s['thickness'];dep=s['depth'];spring=s['spring'];rise=s['rise'];verts=[];faces=[]
        for j in range(13):
            angle=math.pi*j/12
            for radial in [0,1]:
                u=c+(half-radial*th)*math.cos(angle)
                z=spring+(rise-radial*th*.6)*math.sin(angle)
                for v in [-dep/2,dep/2]:verts.append((u,line+v,z) if axis=='h' else (line+v,u,z))
        for j in range(12):
            a=j*4;b=(j+1)*4
            faces.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),(a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
        faces.extend([(0,1,3,2),(48,50,51,49)])
        return mesh_obj(name,verts,faces,s['mat'])
    raise ValueError(typ)

def join_part(p):
    objects=[make_shape(s,p['id']+'_'+str(i)) for i,s in enumerate(p['shapes'])]
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    if len(objects)>1:bpy.ops.object.join()
    o=bpy.context.object;o.name=p['id']
    # Set a stable architectural origin, and real hinge origins on moving parts.
    bpy.context.scene.cursor.location=p.get('pivot') or (0,0,0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    o['part_id']=p['id'];o['title_ko']=p['title'];o['role']=p['role'];o['level']=p['level'];o['review_only']=True
    o['explode_vector']=p['explode'];o['default_hidden']=p['defaultHidden']
    return o

primitive_shape=make_shape
def make_shape(s,name):
    typ=s['type']
    if typ=='lathe':
        profile=s['profile'];n=s['sides'];x,y,z=s['pos'];verts=[];faces=[]
        for h,r in profile:
            verts.extend((x+r*math.cos(2*math.pi*j/n),y+r*math.sin(2*math.pi*j/n),z+h) for j in range(n))
        for k in range(len(profile)-1):
            for j in range(n):faces.append((k*n+j,k*n+(j+1)%n,(k+1)*n+(j+1)%n,(k+1)*n+j))
        faces.extend([tuple(reversed(range(n))),tuple((len(profile)-1)*n+j for j in range(n))])
        o=mesh_obj(name,verts,faces,s['mat'])
        for polygon in o.data.polygons:polygon.use_smooth=len(polygon.vertices)==4
        return o
    if typ in ['poly','stairStringer']:
        points=s['points'];verts=[];faces=[];n=len(points)
        if typ=='poly':
            for d in [-s['depth']/2,s['depth']/2]:
                for u,z in points:verts.append((u,s['line']+d,z) if s['axis']=='h' else (s['line']+d,u,z))
        else:
            dx,dy=s['direction'];x,y,z=s['origin']
            for v in [s['v']-s['depth']/2,s['v']+s['depth']/2]:
                for u,h in points:verts.append((x+dx*u+dy*v,y+dy*u-dx*v,z+h))
        faces=[tuple(reversed(range(n))),tuple(n+i for i in range(n))]
        faces.extend((i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n))
        o=mesh_obj(name,verts,faces,s['mat'])
        if s.get('bevel',.015)>0:
            bpy.context.view_layer.objects.active=o;o.select_set(True)
            mod=o.modifiers.new('Soft edge','BEVEL');mod.width=s.get('bevel',.015);mod.segments=2
            bpy.ops.object.modifier_apply(modifier=mod.name)
        return o
    o=primitive_shape(s,name)
    if typ=='sphere':
        if s.get('scale'):o.scale=s['scale'];bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
        for polygon in o.data.polygons:polygon.use_smooth=True
    return o

for name,m in materials.items():
    node=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    if name.startswith('wardrobe') and 'Coat Weight' in node.inputs:
        node.inputs['Coat Weight'].default_value=.12
        node.inputs['Coat Roughness'].default_value=.4
    if name=='flame':
        node.inputs['Emission Color'].default_value=(1,.5,.15,1)
        node.inputs['Emission Strength'].default_value=2
