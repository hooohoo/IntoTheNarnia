from pathlib import Path
import sys
import json
import math
import base64
import struct
import copy
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
import bmesh
from mathutils import Vector
from mathutils.bvhtree import BVHTree
import mesh_helpers as M

OUT=Path(__file__).resolve().parent.parent
SPEC=json.loads((OUT/'Data/model-spec.json').read_text())
scene=bpy.context.scene;scene.name='ProfessorHouse_ConnectedLayout_01'
scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
records=[]
collections={}
for level,name in [(-1,'00_Gardens'),(0,'01_Ground'),(1,'02_Upper'),(2,'03_Attic')]:
    c=bpy.data.collections.new(name);scene.collection.children.link(c);collections[level]=c

def register(o,p):
    for c in list(o.users_collection): c.objects.unlink(o)
    collections[p['level']].objects.link(o)
    o['part_id']=p['id'];o['zone']=p.get('zone') or '';o['level']=p['level'];o['role']=p['role']
    records.append((o,p))

def clip_mesh(o,lo,hi,name):
    bm=bmesh.new();bm.from_mesh(o.data)
    for z,low,high in [(lo,True,False),(hi,False,True)]:
        result=bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),dist=.00001,plane_co=(0,0,z),plane_no=(0,0,1),clear_inner=low,clear_outer=high)
        edges=[e for e in result['geom_cut'] if isinstance(e,bmesh.types.BMEdge) and e.is_boundary]
        if edges:
            bmesh.ops.holes_fill(bm,edges=edges,sides=0)
    if not bm.faces: bm.free();return None
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    mesh=bpy.data.meshes.new(name);bm.to_mesh(mesh);bm.free();mesh.update()
    obj=bpy.data.objects.new(name,mesh)
    for mat in o.data.materials:obj.data.materials.append(mat)
    return obj

for i,p in enumerate(SPEC['parts']):
    o=M.join_part(p)
    if p.get('translation'):o.location+=Vector(p['translation'])
    if p.get('sliceArchitectural'):
        # Hall surfaces remain one authored source; slices are derived for floor cutaways.
        for level,lo,hi in [(0,-.5,4.32),(1,4.32,9.01)]:
            for tier,a,b in [('low',lo,min(hi,SPEC['levels'][level]+.95)),('high',max(lo,SPEC['levels'][level]+.95),hi)]:
                if b<=a:continue
                pid=p['id']+'_'+str(level)+'_'+tier
                obj=clip_mesh(o,a,b,pid)
                if obj:
                    meta=copy.deepcopy(p);meta.update(id=pid,level=level,tier=tier)
                    register(obj,meta)
        bpy.data.objects.remove(o,do_unlink=True)
    else:register(o,p)
    if i%100==0:print('PARTS '+str(i),flush=True)

# An explicit 1.20 m comparison mannequin. It is not the final Lucy model or height.
for proxy in SPEC['proxies']:
    p=dict(id=proxy['id'],title='1.20 m 비교 인형',role='proxy',level=proxy['level'],zone='scale',shapes=[],explode=[0,0,0],pivot=None,defaultHidden=False)
    def box(x,y,z,w,d,h,mat,bevel=.025):p['shapes'].append(dict(type='box',pos=[x,y,z],size=[w,d,h],mat=mat,bevel=bevel))
    for x in [-.125,.025]:
        box(x,-.07,.08,.10,.12,.28,'linen');box(x,-.13,0,.10,.22,.09,'brown')
    box(-.19,-.12,.34,.38,.24,.26,'brown',.035)
    box(-.15,-.105,.60,.30,.21,.25,'softGreen',.04)
    for x in [-.21,.15]:box(x,-.06,.46,.06,.12,.30,'softGreen')
    p['shapes'].append(dict(type='sphere',pos=[0,0,1.0],r=.2,mat='childSkin'))
    p['shapes'].append(dict(type='sphere',pos=[0,.025,1.07],r=.2,scale=[1,1,.65],mat='brown'))
    o=M.join_part(p);o.location+=Vector(proxy['position']);register(o,p)

font_path=Path('/System/Library/Fonts/AppleSDGothicNeo.ttc')
font=bpy.data.fonts.load(str(font_path)) if font_path.exists() else None
for item in SPEC['labels']:
    curve=bpy.data.curves.new('Label','FONT');curve.body=item['text']+'\n'+item['name'].split(' · ')[0]
    curve.align_x='CENTER';curve.align_y='CENTER';curve.size=item['size'];curve.space_line=1.15
    if font:curve.font=font
    curve.materials.append(M.materials['ink'])
    o=bpy.data.objects.new('Label_'+item['id'],curve);scene.collection.objects.link(o);o.location=item['position']
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH')
    o=bpy.context.object;p=dict(id=o.name,title=item['name'],role='label',level=item['level'],zone=item['id'])
    register(o,p)

glass=M.materials['glass'];nodes=glass.node_tree.nodes;nodes.clear();output=nodes.new('ShaderNodeOutputMaterial');tr=nodes.new('ShaderNodeBsdfTransparent');tr.inputs[0].default_value=(.93,.97,.98,1);glass.node_tree.links.new(tr.outputs[0],output.inputs[0])
bpy.context.view_layer.update()

def triangles_for(objs):
    verts=[];faces=[];owners=[]
    for o,p in objs:
        o.data.calc_loop_triangles();offset=len(verts)
        verts.extend(o.matrix_world@v.co for v in o.data.vertices)
        for t in o.data.loop_triangles:
            faces.append(tuple(offset+i for i in t.vertices));owners.append(p['id'])
    return BVHTree.FromPolygons(verts,faces,all_triangles=True),owners

physical=[(o,p) for o,p in records if p['role'] not in ['label','route','proxy','glass','decor','surface']]
support=[(o,p) for o,p in records if p['role'] in ['floor','gallery','stairs','outdoor']]
solid,owners=triangles_for(physical);floors,floor_owners=triangles_for(support)
failures=[];samples=0;minimum_headroom=10;max_floor_delta=0
validation_routes={k:v['points'] for k,v in SPEC['routes'].items()}|SPEC['extraValidationRoutes']
for key,points in validation_routes.items():
    for a,b in zip(points,points[1:]):
        n=max(1,math.ceil(math.dist(a,b)/.16))
        for i in range(n+1):
            t=i/n;point=Vector([a[j]*(1-t)+b[j]*t for j in range(3)])
            hit,normal,index,distance=floors.ray_cast(point+Vector((0,0,.3)),Vector((0,0,-1)),1.0)
            samples+=1
            if hit is None or abs(hit.z-point.z)>.235:
                failures.append(dict(route=key,kind='support',point=list(point),hit=list(hit) if hit else None));continue
            max_floor_delta=max(max_floor_delta,abs(hit.z-point.z));foot=hit.z
            origin=Vector((point.x,point.y,foot+.24))
            ceiling,norm,idx,dist=solid.ray_cast(origin,Vector((0,0,1)),5)
            if ceiling:
                minimum_headroom=min(minimum_headroom,ceiling.z-foot)
                if ceiling.z-foot<1.30:failures.append(dict(route=key,kind='headroom',part=owners[idx],point=list(point),clearance=ceiling.z-foot))
            for height in [.35,.72,1.10]:
                for j in range(8):
                    angle=j*math.pi/4;direction=Vector((math.cos(angle),math.sin(angle),0))
                    hit2,norm,idx,dist=solid.ray_cast(Vector((point.x,point.y,foot+height)),direction,.22)
                    if hit2:failures.append(dict(route=key,kind='body-clearance',part=owners[idx],point=list(point),height=height,distance=dist))
report=dict(scope='Actual Blender mesh ray samples, not Unity capsule/NavMesh/physics verification.',routes=list(validation_routes),samples=samples,proxyHeight=1.20,proxyRadius=.22,minimumHeadroom=round(minimum_headroom,3),maxFootPathToTreadDifference=round(max_floor_delta,3),failures=failures,passed=not failures)
(OUT/'Data/mesh-route-checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
print('MESH_CHECK '+json.dumps({k:v for k,v in report.items() if k!='failures'})+' failureCount='+str(len(failures)),flush=True)

viewer=[];triangles=0
for o,p in records:
    mesh=o.data;mesh.calc_loop_triangles();buf=[];triangles+=len(mesh.loop_triangles)
    normal_matrix=o.matrix_world.to_3x3().inverted().transposed()
    for tri in mesh.loop_triangles:
        normal=(normal_matrix@tri.normal).normalized();color=o.data.materials[tri.material_index].diffuse_color[:3]
        for vi in tri.vertices:
            co=o.matrix_world@mesh.vertices[vi].co;buf.extend([*co,*normal,*color])
    entry={k:p.get(k) for k in ['id','title','role','level','zone','tier','route','transparent','spans']}
    entry.update(vertices=len(buf)//9,data=base64.b64encode(struct.pack('<'+'f'*len(buf),*buf)).decode())
    viewer.append(entry)
(OUT/'Data/viewer-model.js').write_text('window.HOUSE_MODEL='+json.dumps(dict(parts=viewer,routes=SPEC['routes'],levels=SPEC['levels'],rooms=SPEC['labels'],proxy=SPEC['reviewCharacter']),ensure_ascii=False,separators=(',',':'))+';')
(OUT/'Data/model-stats.json').write_text(json.dumps(dict(parts=len(records),triangles=triangles,units='metres',sourceUpAxis='Z',glbUpAxis='Y',rooms=50,doors=54,notRuntimeOptimized=True),indent=2))

bpy.ops.object.select_all(action='DESELECT')
for o,p in records:
    if p['role'] not in ['route','label','proxy']:o.select_set(True)
bpy.context.view_layer.objects.active=records[0][0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'Models/ProfessorHouse_ConnectedLayout_01.glb'),export_format='GLB',use_selection=True,use_active_scene=True,export_yup=True)

from fix_glb_materials import fix_glass
fix_glass(OUT/'Models/ProfessorHouse_ConnectedLayout_01.glb')

scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=20;scene.cycles.use_denoising=True
scene.render.resolution_x=1680;scene.render.resolution_y=1120;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
scene.world=bpy.data.worlds.new('ReviewDay');scene.world.use_nodes=True
bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs[0].default_value=(.80,.84,.86,1);bg.inputs[1].default_value=.7
scene.view_settings.view_transform='AgX'
cam_data=bpy.data.cameras.new('ReviewCamera');cam=bpy.data.objects.new('ReviewCamera',cam_data);scene.collection.objects.link(cam);scene.camera=cam
def camera(location,target,ortho=None,lens=24):
    cam.location=location;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
    cam.data.type='ORTHO' if ortho else 'PERSP';cam.data.lens=lens
    if ortho:cam.data.ortho_scale=ortho
def area(name,position,target,power,size):
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=size
    o=bpy.data.objects.new(name,data);scene.collection.objects.link(o);o.location=position;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
area('Large soft key',(15,-10,45),(20,17,0),18000,30)
area('Rear fill',(12,43,30),(20,17,0),15000,25)
area('Hall',(17,15,8.3),(17,20,1),1700,7)
data=bpy.data.lights.new('Sun','SUN');data.energy=1.2;data.angle=.15
sun=bpy.data.objects.new('Sun',data);scene.collection.objects.link(sun);sun.rotation_euler=(.3,-.35,-.45)

original_positions={o.name:o.location.copy() for o,p in records}
def visibility(level=None,route=None,full=False,explode=False,labels=True):
    for o,p in records:
        o.location=original_positions[o.name]
        show=level is None or p['level'] in (-1,level)
        if level==1 and p['zone']=='hall' and p['role'] in ['stairs','stairs-trim','rails','structure']:show=True
        if level in (1,2) and p.get('spans') and level in p['spans']:show=True
        if p['role']=='route':show=show and p.get('route')==route
        if p['role']=='label':show=show and labels
        if not full and (p.get('tier')=='high' or p['role'] in ['ceiling','frame','glass']):show=False
        if explode and p['level']>=0:o.location.z+=p['level']*7.0
        o.hide_render=not show;o.hide_set(not show)
    bpy.context.view_layer.update()
def fit_orthographic():
    bpy.context.view_layer.update()
    inv=cam.matrix_world.inverted()
    corners=[inv@(o.matrix_world@Vector(c)) for o,p in records if not o.hide_render for c in o.bound_box]
    xmin,xmax=min(v.x for v in corners),max(v.x for v in corners)
    ymin,ymax=min(v.y for v in corners),max(v.y for v in corners)
    aspect=scene.render.resolution_x/scene.render.resolution_y
    cam.data.ortho_scale=max(xmax-xmin,(ymax-ymin)*aspect)*1.12
    cam.location+=cam.matrix_world.to_quaternion()@Vector(((xmin+xmax)/2,(ymin+ymax)/2,0))

def render(name):
    only=next((a.split('=',1)[1] for a in sys.argv if a.startswith('--only=')),None)
    if only and name not in only.split(','):return
    if name[:2] in ['01','02','03','04','05','10']:fit_orthographic()
    scene.render.filepath=str(OUT/'Previews'/f'{name}.png');bpy.ops.render.render(write_still=True);print('RENDERED '+name,flush=True)

visibility(explode=True);camera((82,-72,84),(20,17,13),ortho=78);render('01_Whole_Exploded')
visibility(level=0,route='escape');camera((60,-45,65),(20,17,0),ortho=68);render('02_Ground')
visibility(level=1,route='arrival');camera((60,-45,67),(23,17,4),ortho=66);render('03_Upper')
visibility(level=2,route='hide');camera((56,-26,57),(15,23,9),ortho=48);render('04_Attic')
visibility(level=1,route='hide');camera((60,-45,67),(23,17,4),ortho=66);render('05_HideRoute')
visibility(full=True,labels=False);camera((16.4,10.8,1.05),(17,20.9,3.5),lens=23);render('06_Hall_Eye')
visibility(level=1,route='arrival',labels=False);camera((60,49,38),(38,26.5,4.5),ortho=35);render('07_Children_Wing')
visibility(level=2,route='hide');camera((16,13,24),(4,27,10),ortho=23);render('08_Wardrobe_Approach')
visibility(level=0,route='escape');camera((-16,5,23),(2,16,1),ortho=32);render('09_WestGarden_Incident')
visibility(full=True,labels=False)
for o,p in records:
    if p['level']>=0 and not (p.get('zone') in ['S2','ground/S2','upper/S2','attic/S2'] or 'S2' in p['id']):o.hide_render=True
    if p['role']=='wall' or p['level']==-1:o.hide_render=True
camera((40,16,19),(27,30,5),ortho=19);render('10_Secondary_Stairs')
visibility(level=1,labels=False)
for o,p in records:
    if p['id'].startswith(('B02N','B12')):o.hide_render=False
camera((6,44,24),(17,25.5,3.3),ortho=31);render('11_RearGarden')
visibility(explode=False)
camera((75,-60,65),(22,18,5),ortho=68)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Models/ProfessorHouse_ConnectedLayout_01.blend'))
print('BUILD_COMPLETE '+json.dumps(dict(parts=len(records),triangles=triangles,routeFailures=len(failures))),flush=True)
