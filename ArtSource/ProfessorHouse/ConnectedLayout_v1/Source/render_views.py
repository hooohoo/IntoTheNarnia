from pathlib import Path
import json,sys,copy,re
import bpy
from mathutils import Vector
OUT=Path(__file__).resolve().parent.parent
SPEC=json.loads((OUT/'Data/model-spec.json').read_text())
bpy.ops.wm.open_mainfile(filepath=str(OUT/'Models/ProfessorHouse_ConnectedLayout_01.blend'))
scene=bpy.context.scene
source_parts={p['id']:p for p in SPEC['parts']}
records=[]
for o in list(scene.objects):
    if 'part_id' not in o:continue
    pid=o['part_id']
    p=copy.deepcopy(source_parts.get(pid))
    if p is None:
        match=re.match(r'^(.*)_([01])_(low|high)$',pid)
        if match and match[1] in source_parts:
            p=copy.deepcopy(source_parts[match[1]])
            p.update(id=pid,level=int(match[2]),tier=match[3])
        else:p={k:o[k] for k in ('part_id','zone','level','role')}
    p['id']=pid
    records.append((o,p))
# Replace render apparatus from the saved source rather than doubling the lights.
for o in list(scene.objects):
    if o.type in ('LIGHT','CAMERA'):bpy.data.objects.remove(o,do_unlink=True)
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
print('RENDERS_COMPLETE',flush=True)
