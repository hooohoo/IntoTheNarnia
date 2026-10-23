"""Build a reversible decorative layer over the approved manor layout.

Blender --background --factory-startup --python this_file -- --project PATH
Authoring: X east, Y rear, Z up. Export: Unity X east, Y up, Z rear.
"""
import argparse
from collections import defaultdict
import hashlib
import json
import math
import os
from pathlib import Path
import random
import sys

import bpy
from mathutils import Matrix, Vector

parser = argparse.ArgumentParser()
parser.add_argument('--project', required=True)
options = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
project = Path(options.project).resolve()
source = project / 'ArtSource/ProfessorHouse/HallFinish_v1'
output = project / 'Assets/@Project/10.Models/ProfessorHouseHallFinish'
source.mkdir(parents=True, exist_ok=True)
output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
random.seed(23)
palette = {
    'HallWalnut': '513122', 'HallWalnutEdge': '795039', 'HallGold': 'BE9253',
    'HallStone': 'DCCDB3', 'HallIvory': 'EEE1C6', 'HallSage': '647C61',
    'HallLeaf': '516B38', 'HallLeafLight': '8D9B52', 'HallPetal': 'F1DFB0',
    'HallRugLight': '9B5441', 'HallRugDark': '62362E', 'HallRugGold': 'BC8C50',
    'HallFlame': 'FFE2A4', 'HallPainting': 'FFFFFF', 'HallBook': '586F72', 'HallPlaster': 'E0CDAA',
}
materials = {}
groups = defaultdict(list)
colliders = []
flames = []
group = 'NorthWall'

for name, color in palette.items():
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*[int(color[i:i+2],16)/255 for i in (0,2,4)], 1)
    mat.use_nodes = True
    node = mat.node_tree.nodes.get('Principled BSDF')
    node.inputs['Base Color'].default_value = mat.diffuse_color
    node.inputs['Roughness'].default_value = .5
    materials[name] = mat

def register(obj, name, material, smooth=False):
    obj.name = name
    obj.data.materials.append(materials[material])
    if smooth:
        for face in obj.data.polygons:
            face.use_smooth = True
    groups[group].append(obj)
    return obj

def box(name, center, size, material='HallWalnut', bevel=.025):
    bpy.ops.mesh.primitive_cube_add(size=1, location=center)
    obj = bpy.context.object
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        mod = obj.modifiers.new('Soft edges', 'BEVEL')
        mod.width = min(bevel, min(size)*.3)
        mod.segments = 2
        bpy.ops.object.modifier_apply(modifier=mod.name)
        mod = obj.modifiers.new('Weighted corner normals', 'WEIGHTED_NORMAL')
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return register(obj, name, material)

def sphere(name, center, scale, material):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=8, radius=1, location=center)
    obj = bpy.context.object
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return register(obj, name, material, True)

def beam(name, a, b, radius, material):
    a,b = Vector(a),Vector(b)
    bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=radius, depth=(b-a).length, location=(a+b)*.5)
    obj = bpy.context.object
    obj.rotation_euler = (b-a).to_track_quat('Z','Y').to_euler()
    return register(obj,name,material,True)

def lathe(name, center, profile, material):
    n = 16
    verts = [(center[0]+r*math.cos(2*math.pi*j/n),center[1]+r*math.sin(2*math.pi*j/n),center[2]+z) for z,r in profile for j in range(n)]
    faces = [(k*n+j,k*n+(j+1)%n,(k+1)*n+(j+1)%n,(k+1)*n+j) for k in range(len(profile)-1) for j in range(n)]
    faces += [tuple(reversed(range(n))),tuple((len(profile)-1)*n+j for j in range(n))]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    obj = bpy.data.objects.new(name,mesh)
    bpy.context.collection.objects.link(obj)
    register(obj,name,material)
    for face in mesh.polygons:
        face.use_smooth = len(face.vertices)==4
    return obj

def collider(name, center, size):
    colliders.append(dict(name=name,center=dict(x=center[0],y=center[2],z=center[1]),size=dict(x=size[0],y=size[2],z=size[1])))

def vase(x,y,z,scale=1):
    lathe('Ceramic urn',(x,y,z),[(h*scale,r*scale) for h,r in [(0,.15),(.06,.18),(.11,.12),(.3,.22),(.44,.16),(.5,.1),(.54,.14)]],'HallIvory')
    for i in range(9):
        angle = i*2.39996
        radius = (.12 if i%3==0 else .27)*scale
        tip = (x+math.cos(angle)*radius,y+math.sin(angle)*radius,z+(.66+random.random()*.18)*scale)
        beam('Flower stem',(x,y,z+.42*scale),tip,.012*scale,'HallLeaf')
        leaf = sphere('Oval leaf',(tip[0],tip[1],tip[2]-.12*scale),(.11*scale,.05*scale,.045*scale),'HallLeaf')
        leaf.rotation_euler.z=angle
        for petal in range(5):
            a = petal*math.tau/5
            sphere('Ivory blossom',(tip[0]+.045*scale*math.cos(a),tip[1]+.045*scale*math.sin(a),tip[2]),(.048*scale,.045*scale,.025*scale),'HallPetal')
        sphere('Flower center',tip,(.024*scale,)*3,'HallGold')

def painting(name, x,y,z,w,h,tile,rotation=0):
    start = len(groups[group])
    box(name+' backing',(x,y,z),(w+.16,.12,h+.16),'HallWalnut',.015)
    for pad,thick,depth,mat in [(0,.085,.16,'HallGold'),(.075,.025,.19,'HallWalnutEdge'),(.104,.018,.2,'HallGold')]:
        ww,hh=w-2*pad,h-2*pad
        for xx in [-ww/2,ww/2]:box('Frame upright',(x+xx,y-depth/2,z),(thick,depth,hh+thick),mat,.012)
        for zz in [-hh/2,hh/2]:box('Frame rail',(x,y-depth/2,z+zz),(ww,depth,thick),mat,.012)
    ww,hh=w-.22,h-.22
    mesh=bpy.data.meshes.new(name+' canvas')
    mesh.from_pydata([(x-ww/2,y-.112,z-hh/2),(x+ww/2,y-.112,z-hh/2),(x+ww/2,y-.112,z+hh/2),(x-ww/2,y-.112,z+hh/2)],[],[(0,1,2,3)])
    mesh.update()
    obj=bpy.data.objects.new(name+' canvas',mesh)
    bpy.context.collection.objects.link(obj)
    register(obj,name+' canvas','HallPainting')
    col,row=tile%2,tile//2
    u0,u1=col*.5+.012,(col+1)*.5-.012
    v0,v1=(1-row)*.5+.012,(2-row)*.5-.012
    uv=mesh.uv_layers.new()
    for i,p in enumerate([(u0,v0),(u1,v0),(u1,v1),(u0,v1)]):uv.data[i].uv=p
    if rotation:
        transform=Matrix.Translation((x,y,z)) @ Matrix.Rotation(rotation,4,'Z') @ Matrix.Translation((-x,-y,-z))
        for item in groups[group][start:]:item.matrix_world=transform @ item.matrix_world

def sconce(x,y,z,rotation=0):
    start=len(groups[group])
    box('Brass wall plate',(x,y,z),(.14,.09,.38),'HallGold',.055)
    beam('Candle arm',(x,y,z-.08),(x,y-.28,z-.08),.028,'HallGold')
    lathe('Candle cup',(x,y-.28,z-.1),[(0,.055),(.04,.09),(.075,.075)],'HallGold')
    lathe('Wax candle',(x,y-.28,z-.025),[(0,.033),(.25,.035)],'HallIvory')
    sphere('Amber flame',(x,y-.28,z+.29),(.032,.032,.09),'HallFlame')
    transform=Matrix.Translation((x,y,z)) @ Matrix.Rotation(rotation,4,'Z') @ Matrix.Translation((-x,-y,-z))
    for item in groups[group][start:]:item.matrix_world=transform @ item.matrix_world
    p=transform @ Vector((x,y-.28,z+.29))
    flames.append(dict(x=p.x,y=p.z,z=p.y))

def table(x,y):
    box('Console polished top',(x,y,.86),(1.25,.53,.1),'HallWalnutEdge',.035)
    box('Console drawer',(x,y,.71),(1.1,.44,.23),'HallWalnut',.028)
    sphere('Drawer pull',(x,y-.25,.73),(.055,.045,.045),'HallGold')
    for xx in [-.47,.47]:
        for yy in [-.15,.15]:lathe('Turned table leg',(x+xx,y+yy,.05),[(0,.045),(.08,.055),(.3,.035),(.5,.06),(.64,.045)],'HallWalnut')
    box('Console lower shelf',(x,y,.18),(1.02,.4,.07),'HallWalnutEdge')
    for i in range(3):box('Old book',(x-.25,y,.94+i*.055),(.35,.24,.055),'HallBook' if i%2 else 'HallRugDark',.008)
    vase(x+.3,y,.91,.72)
    collider('Console', (x,y,.48),(1.24,.55,.96))

group='NorthWall'
painting('Gentleman portrait',13.45,21.72,5.65,1.45,1.95,0)
painting('Lady portrait',19.38,21.72,5.6,1.28,1.75,1)
painting('Pastoral landscape',20.0,21.72,2.0,1.3,1.0,3)
sconce(12.22,21.68,5.25)
sconce(19.5,21.68,7.15)
vase(16.06,21.22,1.83,.9)
vase(17.94,21.22,1.83,.9)
collider('Landing urn west',(16.06,21.22,2.08),(.42,.42,.5))
collider('Landing urn east',(17.94,21.22,2.08),(.42,.42,.5))

group='WestFurnishing'
painting('Manor landscape',12.85,21.7,2.12,1.6,1.15,2)
painting('West gallery portrait',10.25,16.1,6.1,1.05,1.45,1,math.pi/2)
sconce(10.25,17.5,2.35,math.pi/2)
sconce(10.25,12,6.0,math.pi/2)
table(10.95,20.12)
x,y=12.72,20.65
for xx in [-.32,.32]:
    for yy in [-.25,.25]:box('Armchair foot',(x+xx,y+yy,.24),(.085,.085,.42),'HallWalnut',.025)
box('Chair seat rim',(x,y,.48),(.83,.77,.13),'HallWalnutEdge',.055)
box('Rounded seat cushion',(x,y-.035,.59),(.69,.68,.14),'HallSage',.09)
box('Chair back carved rim',(x,y+.29,1.02),(.88,.17,.92),'HallWalnutEdge',.085)
box('Upholstered back',(x,y+.18,1.05),(.69,.13,.69),'HallSage',.09)
for xx in [-.4,.4]:
    beam('Arm support',(x+xx,y-.25,.5),(x+xx,y-.25,.83),.035,'HallWalnut')
    box('Rounded chair arm',(x+xx,y,.84),(.12,.75,.1),'HallWalnutEdge',.04)
collider('Armchair',(x,y,.75),(.94,.85,1.5))
x,y=14.25,20.12
box('Bust pedestal foot',(x,y,.10),(.62,.52,.2),'HallStone',.03)
box('Bust pedestal shaft',(x,y,.63),(.41,.38,.92),'HallIvory',.022)
box('Bust pedestal cap',(x,y,1.12),(.57,.48,.12),'HallStone',.025)
lathe('Bust plinth',(x,y,1.18),[(0,.19),(.08,.19),(.11,.14)],'HallIvory')
torso=lathe('Draped sculpture torso',(x,y,1.29),[(0,.16),(.04,.17),(.12,.25),(.23,.27),(.34,.2),(.38,.1)],'HallIvory')
for vertex in torso.data.vertices:
    vertex.co.y=y+(vertex.co.y-y)*.58
sphere('Sculpture head',(x,y,1.88),(.145,.13,.205),'HallIvory')
sphere('Sculpture nose',(x,y-.13,1.87),(.027,.046,.055),'HallIvory')
beam('Bust neck',(x,y,1.6),(x,y,1.77),.075,'HallIvory')
sphere('Sculpture swept hair',(x,y+.014,2.045),(.147,.129,.085),'HallStone')
for side in [-1,1]:
    sphere('Sculpture ear',(x+side*.14,y,1.88),(.027,.035,.049),'HallIvory')
    sphere('Sculpture eye',(x+side*.058,y-.12,1.918),(.022,.012,.01),'HallStone')
    beam('Sculpture brow',(x+side*.036,y-.126,1.944),(x+side*.082,y-.109,1.944),.011,'HallIvory')
    beam('Carved lapel',(x+side*.16,y-.12,1.59),(x+side*.025,y-.13,1.38),.016,'HallStone')
sphere('Sculpture mouth',(x,y-.125,1.804),(.039,.018,.01),'HallStone')
collider('Pedestal',(x,y,.65),(.62,.52,1.3))

group='EastFurnishing'
table(23.04,20.15)
painting('East manor painting',23.75,18.9,2.2,1.4,1.05,2,-math.pi/2)
painting('East gallery portrait',23.75,13.6,6.0,1.1,1.5,0,-math.pi/2)
sconce(23.75,18.0,2.4,-math.pi/2)
sconce(23.75,17.0,6.05,-math.pi/2)

group='Ceiling'
# Pale ceiling panels sit below the dark attic underside, between existing beams.
for x in [11.85,15.3,18.7,22.15]:
    for y,d in [(13.23,4.54),(18.55,4.72)]:
        box('Ivory coffer',(x,y,8.77),(3.14,d,.08),'HallIvory',.018)
for x in [13.6,17,20.4]:box('Long ceiling beam',(x,16,8.65),(.18,11.4,.22),'HallWalnutEdge',.025)

group='RugInlay'
# Shallow geometry inlay avoids new image dependencies and z-fighting.
for inset in [.25,.39]:
    x0,x1,y0,y1=13+inset,21-inset,11+inset,14.4-inset
    for y in [y0,y1]:box('Rug woven border',((x0+x1)/2,y,.066),(x1-x0,.026,.006),'HallRugGold',0)
    for x in [x0,x1]:box('Rug woven border',(x,(y0+y1)/2,.066),(.026,y1-y0,.006),'HallRugGold',0)
for x in [13.6+i*.32 for i in range(22)]:
    for y in [11.32,14.08]:
        obj=box('Border lozenge',(x,y,.068),(.06,.06,.006),'HallRugLight',0)
        obj.rotation_euler.z=math.pi/4
for x in [14.4,15.7,17,18.3,19.6]:
    obj=box('Rug medallion',(x,12.7,.067),(.52,.52,.006),'HallRugGold',0)
    obj.rotation_euler.z=math.pi/4
    obj=box('Rug medallion inset',(x,12.7,.073),(.42,.42,.006),'HallRugLight',0)
    obj.rotation_euler.z=math.pi/4
    sphere('Rug motif',(x,12.7,.074),(.15,.15,.005),'HallRugDark')

group='GardenVista'
for x,y,height in [(14.1,27.9,5.7),(19.8,28.6,6.1),(16.6,32,5.6)]:
    beam('Garden tree trunk',(x,y,0),(x,y,height*.78),.16,'HallWalnutEdge')
    collider('Garden trunk',(x,y,height*.37),(.34,.34,height*.74))
    for j in range(6):
        a=j*2.4
        sphere('Rounded tree crown',(x+.65*math.cos(a),y+.6*math.sin(a),height-.4+(j%3)*.35),(1.02,.87,.94),'HallLeafLight' if j%2 else 'HallLeaf')

group='WallFinish'
spec=json.loads((project/'ArtSource/ProfessorHouse/ConnectedLayout_v1/Data/model-spec.json').read_text())
for part in spec['parts']:
    if part.get('zone')!='hall' or part['role']!='wall':continue
    for shape in part['shapes']:
        if shape['type']!='box' or shape['mat']!='plaster':continue
        x,y,z=shape['pos'];w,d,h=shape['size']
        # Continuous flat plaster faces bridge the original 8 mm construction bevels.
        if part['id']=='B02N':center,size=(x+w/2,y-.003,z+h/2),(w,.004,h)
        elif part['id']=='B02S':center,size=(x+w/2,y+d+.003,z+h/2),(w,.004,h)
        elif part['id']=='B02W':center,size=(x+w+.003,y+d/2,z+h/2),(.004,d,h)
        else:center,size=(x-.003,y+d/2,z+h/2),(.004,d,h)
        box('Continuous plaster face',center,size,'HallPlaster',0)

meshes=[]
for name, members in sorted(groups.items()):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in members:obj.select_set(True)
    bpy.context.view_layer.objects.active=members[0]
    bpy.ops.object.join()
    obj=bpy.context.object
    obj.name='V_HallFinish_'+name
    bpy.context.scene.cursor.location=(0,0,0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    obj['role']='hall-finish'
    obj['approved_layout_unchanged']=True
    meshes.append(obj)

image_path=project/'Assets/@Project/03.Sprites/ProfessorHouseHallFinish/TEX_Hall_Paintings.png'
if image_path.exists():
    image=bpy.data.images.load(str(image_path))
    image.pack()
    mat=materials['HallPainting']
    node=mat.node_tree.nodes.new('ShaderNodeTexImage')
    node.image=image
    mat.node_tree.links.new(node.outputs['Color'],mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
bpy.context.scene.unit_settings.system='METRIC'
bpy.context.scene.unit_settings.scale_length=1
bpy.ops.wm.save_as_mainfile(filepath=str(source/'ProfessorHouse_HallFinish_v1.blend'))
entries=[]
conversion=Matrix.Diagonal((-1,-1,1,1))
for obj in meshes:
    points=[obj.matrix_world @ v.co for v in obj.data.vertices]
    points=[Vector((p.x,p.z,p.y)) for p in points]
    entries.append(dict(name=obj.name,boundsMin=dict(zip(('x','y','z'),[min(p[a] for p in points) for a in range(3)])),boundsMax=dict(zip(('x','y','z'),[max(p[a] for p in points) for a in range(3)])),triangles=sum(len(p.vertices)-2 for p in obj.data.polygons)))
    obj.data.transform(conversion @ obj.matrix_world)
    obj.matrix_world=Matrix.Identity(4)
bpy.ops.object.select_all(action='DESELECT')
for obj in meshes:obj.select_set(True)
bpy.context.view_layer.objects.active=meshes[0]
bpy.ops.export_scene.fbx(filepath=str(output/'FBX_ProfessorHouse_HallFinish.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,mesh_smooth_type='FACE',add_leaf_bones=False,bake_anim=False,path_mode='STRIP',embed_textures=False)
manifest=dict(version='hall-finish-01',sourceLayoutSha256=hashlib.sha256((project/'Docs/Art/ProfessorHouse_ApprovedLayout_v1/Data/layout.json').read_bytes()).hexdigest(),palette=[dict(name=n,hex=c) for n,c in palette.items()],meshes=entries,colliders=colliders,flames=flames)
(output/'HallFinish.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('HALL_FINISH_EXPORT_COMPLETE '+json.dumps(dict(meshes=len(meshes),triangles=sum(e['triangles'] for e in entries),colliders=len(colliders))),flush=True)
os._exit(0)
