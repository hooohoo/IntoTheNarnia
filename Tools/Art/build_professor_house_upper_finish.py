"""Finish the professor study, private library/bedroom and two children's rooms.

Preserves approved walls, windows, doors and story routes. Blender Z-up metres.
Run Blender --background --python this_file -- --project PATH.
"""
import argparse
import json
import math
import os
from pathlib import Path
import random
import sys
import bpy
import bmesh
from mathutils import Matrix, Vector
sys.path.insert(0, str(Path(__file__).resolve().parent))
from professor_house_finish_kit import FinishKit

parser=argparse.ArgumentParser()
parser.add_argument('--project',required=True)
options=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
project=Path(options.project).resolve()
source=project/'ArtSource/ProfessorHouse/UpperFinish_v1'
output=project/'Assets/@Project/10.Models/ProfessorHouseUpperFinish'
source.mkdir(parents=True,exist_ok=True);output.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version=0
palette={'HallWalnut':'513122','HallWalnutEdge':'795039','HallGold':'BE9253','HallStone':'DCCDB3','HallIvory':'EEE1C6','HallSage':'647C61','HallLeaf':'516B38','HallPetal':'F1DFB0','HallRugDark':'62362E','HallFlame':'FFE2A4','HallPainting':'FFFFFF','HallBook':'586F72',
         'UpperPlaster':'E7D8B9','UpperFloor':'876044','UpperFloorAlt':'936D50','UpperRose':'BC8278','UpperLinen':'E5D6B9','UpperBlue':'627E8C','UpperGreen':'7E8C71','UpperPaper':'E6D4A8','UpperBookRed':'874D3D','UpperBookGreen':'657353','UpperLeather':'634233'}
k=FinishKit(palette)
box=k.box;sphere=k.sphere;beam=k.beam;lathe=k.lathe;painting=k.painting;collider=k.collider
base=4.32
layout=json.loads((project/'Docs/Art/ProfessorHouse_ApprovedLayout_v1/Data/layout.json').read_text())['floors']['upper']
spec=json.loads((project/'ArtSource/ProfessorHouse/ConnectedLayout_v1/Data/model-spec.json').read_text())
rooms={r['id']:r for r in layout['rooms']}
finished=['P-STUDY','P-LIB','P-BED','E1','K-BOYS']

def rotate_members(start,origin,angle):
    bpy.context.view_layer.update()
    transform=Matrix.Translation(origin) @ Matrix.Rotation(angle,4,'Z') @ Matrix.Translation(-Vector(origin))
    for item in k.groups[k.group][start:]:item.matrix_world=transform @ item.matrix_world

def books(x,y,z,width,depth=.25,height=.4):
    count=max(2,int(width/.105));step=width/count
    colors=['HallBook','UpperBookRed','UpperBookGreen','UpperLeather']
    for i in range(count):
        h=height*(.76+.24*random.random())
        xx=x-width/2+(i+.5)*step
        box('Rounded book spine',(xx,y,z+h/2),(step*.84,depth,h),colors[i%4],.01)
        for zz in [z+.06,z+h-.06]:box('Book gold band',(xx,y-depth/2-.004,zz),(step*.66,.009,.009),'HallGold',0)

def lamp(x,y,z):
    lathe('Lamp brass base',(x,y,z),[(0,.13),(.035,.15),(.07,.09),(.12,.06),(.25,.065),(.29,.1)],'HallGold')
    lathe('Pleated warm lampshade',(x,y,z+.29),[(0,.22),(.28,.12)],'UpperLinen')
    lathe('Shade piping',(x,y,z+.29),[(0,.225),(.018,.225)],'HallGold')
    sphere('Lamp finial',(x,y,z+.61),(.025,)*3,'HallGold')

def cabinet(name,x,y,z,w,d,h,rotation=0,drawers=2):
    start=len(k.groups[k.group])
    box(name+' carcass',(x,y,z+h/2),(w,d,h-.12),'HallWalnut',.035)
    box(name+' top',(x,y,z+h),(w+.08,d+.07,.08),'HallWalnutEdge',.027)
    for xx in [-w*.38,w*.38]:
        for yy in [-d*.34,d*.34]:lathe(name+' foot',(x+xx,y+yy,z),[(0,.06),(.08,.075),(.17,.05)],'HallWalnut')
    for i in range(drawers):
        zz=z+.16+(i+.5)*(h-.23)/drawers
        box('Inset drawer',(x,y-d/2-.018,zz),(w-.15,.045,(h-.23)/drawers-.04),'HallWalnutEdge',.015)
        for xx in [-w*.24,w*.24]:sphere('Brass pull',(x+xx,y-d/2-.06,zz),(.032,.025,.032),'HallGold')
    rotate_members(start,(x,y,z),rotation)
    sizes=(w+.1,d+.12,h+.08) if not rotation else (d+.12,w+.1,h+.08)
    collider(name,(x,y,z+h/2),sizes)

def chair(x,y,z,material='HallSage',rotation=0):
    start=len(k.groups[k.group])
    for xx in [-.31,.31]:
        for yy in [-.27,.27]:beam('Chair leg',(x+xx,y+yy,z+.03),(x+xx,y+yy,z+.49),.045,'HallWalnut')
    box('Chair seat',(x,y,z+.52),(.8,.78,.14),'HallWalnutEdge',.06)
    box('Chair cushion',(x,y,z+.63),(.69,.68,.14),material,.06)
    box('Rounded chair back',(x,y+.29,z+1.04),(.82,.18,.94),'HallWalnut',.06)
    box('Upholstered chair back',(x,y+.18,z+1.08),(.66,.12,.69),material,.045)
    for xx in [-.39,.39]:
        beam('Chair arm upright',(x+xx,y-.23,z+.56),(x+xx,y-.23,z+.87),.035,'HallWalnut')
        box('Chair arm',(x+xx,y,z+.9),(.11,.7,.09),'HallWalnutEdge')
    rotate_members(start,(x,y,z),rotation)
    collider('Chair',(x,y,z+.74),(.9,.92,1.48))

def arched_headboard(x,y,z,w):
    # A single extruded arch avoids a seam between a box and an ellipsoid.
    outline=[(-w/2,.42),(w/2,.42)]+[(w*.5*math.cos(i*math.pi/12),1.25+.23*math.sin(i*math.pi/12)) for i in range(13)]
    n=len(outline)
    verts=[(x+xx,y+yy,z+zz) for yy in [-.07,.07] for xx,zz in outline]
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh=bpy.data.meshes.new('Unified arched headboard');mesh.from_pydata(verts,[],faces);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bmesh.ops.bevel(bm,geom=list(bm.edges),offset=.024,segments=2,profile=.5,affect='EDGES')
    bm.to_mesh(mesh);bm.free();mesh.update()
    obj=bpy.data.objects.new('Unified arched headboard',mesh);bpy.context.collection.objects.link(obj)
    k.register(obj,'Unified arched headboard','HallWalnut')

def bed(x,y,z,w,cloth):
    # Local Y points toward the headboard. The original blockout footprint is retained.
    length=2.3
    box('Bed platform',(x,y,z+.38),(w,length,.2),'HallWalnut',.055)
    for xx in [-w/2,w/2]:
        for yy,h in [(-1.13,1.0),(1.13,1.42)]:
            lathe('Turned bed post',(x+xx,y+yy,z),[(0,.055),(.08,.08),(.16,.05),(h-.12,.05),(h-.08,.08)],'HallWalnutEdge')
            sphere('Round bed finial',(x+xx,y+yy,z+h),(.095,)*3,'HallWalnutEdge')
    box('Footboard',(x,y-1.12,z+.61),(w,.12,.65),'HallWalnut',.035)
    box('Footboard inset',(x,y-1.193,z+.62),(w-.2,.03,.36),'HallWalnutEdge',.015)
    arched_headboard(x,y+1.12,z,w)
    box('Mattress',(x,y,z+.56),(w-.08,2.13,.24),'UpperLinen',.075)
    box('Rounded quilt',(x,y-.23,z+.72),(w-.02,1.64,.18),cloth,.07)
    box('Folded quilt edge',(x,y+.48,z+.76),(w, .22,.15),'UpperLinen',.05)
    for xx in [-w*.46,w*.46]:box('Draped blanket side',(x+xx,y-.22,z+.54),(.095,1.58,.39),cloth,.03)
    box('Soft pillow',(x,y+.77,z+.79),(w*.7,.42,.19),'UpperLinen',.06)
    box('Small accent cushion',(x,y+.62,z+.89),(w*.43,.24,.19),'UpperGreen' if cloth=='UpperRose' else 'HallRugDark',.055)
    # Woven embroidery is deliberately simple geometry, not an imitation of high-detail cloth.
    for xx in [-.37,0,.37]:
        for yy in [-.72,-.23,.25]:
            sphere('Quilt embroidered petal',(x+xx,y+yy,z+.814),(.045,.09,.004),'UpperLinen')
    collider('Bed',(x,y,z+.7),(w+.22,2.49,1.4))

def rug(x,y,z,w,d,cloth):
    box('Woven rug',(x,y,z+.022),(w,d,.022),cloth,.005)
    for margin in [.12,.22]:
        for yy in [-d/2+margin,d/2-margin]:box('Rug border',(x,y+yy,z+.037),(w-2*margin,.025,.006),'HallGold',0)
        for xx in [-w/2+margin,w/2-margin]:box('Rug border',(x+xx,y,z+.037),(.025,d-2*margin,.006),'HallGold',0)
    for xx in [-w*.28,0,w*.28]:
        o=box('Woven medallion',(x+xx,y,z+.038),(.4,.4,.006),'HallGold',0);o.rotation_euler.z=math.pi/4
        o=box('Medallion inset',(x+xx,y,z+.042),(.31,.31,.005),cloth,0);o.rotation_euler.z=math.pi/4
        for a in range(4):
            angle=a*math.pi/2
            leaf=sphere('Woven leaf',(x+xx+.07*math.cos(angle),y+.07*math.sin(angle),z+.045),(.07,.03,.002),'UpperRose')
            leaf.rotation_euler.z=angle
    for i in range(int((w-.6)/.26)):
        for yy in [-d/2+.18,d/2-.18]:
            o=box('Rug border stitch',(x-w/2+.36+i*.26,y+yy,z+.041),(.04,.04,.004),'UpperRose',0);o.rotation_euler.z=math.pi/4

def bookshelf(x,y,z,w=2.1,rotation=0):
    start=len(k.groups[k.group]);d=.48;h=2.85
    box('Bookcase back',(x,y+.2,z+h/2),(w,.08,h),'HallWalnut',.015)
    for xx in [-w/2,w/2]:box('Bookcase side',(x+xx,y,z+h/2),(.12,d,h),'HallWalnutEdge',.025)
    for zz in [.12,.65,1.2,1.75,2.3,2.85]:box('Bookcase shelf',(x,y,z+zz),(w+.12,d+.04,.09),'HallWalnut',.018)
    box('Bookcase crown',(x,y,z+h+.07),(w+.24,d+.11,.12),'HallWalnutEdge',.03)
    for i,zz in enumerate([.17,.7,1.25,1.8,2.35]):
        books(x-.08*(i%2),y-.045,z+zz,w-.25,.29,.39)
    rotate_members(start,(x,y,z),rotation)
    collider('Bookcase',(x,y,z+1.5),(w+.26,.62,3.0) if not rotation else (.62,w+.26,3.0))

def curtains(x,y,z,width=1.5,rotation=0,cloth='UpperRose'):
    start=len(k.groups[k.group])
    # Wall-side rod at +3.08 m; cloth never covers the approved window opening.
    beam('Curtain pole',(x-width/2-.45,y-.18,z+3.08),(x+width/2+.45,y-.18,z+3.08),.028,'HallGold')
    for side in [-1,1]:
        xx=x+side*(width/2+.21)
        sphere('Curtain pole finial',(x+side*(width/2+.48),y-.18,z+3.08),(.07,)*3,'HallWalnutEdge')
        # One continuous low-poly drape, with soft accordion folds and a gathered waist.
        verts=[];faces=[];cols=12
        for j,(zz,spread) in enumerate([(0.16,1.05),(1.1,.65),(1.2,.65),(2.97,1.0)]):
            for i in range(cols+1):
                u=i/cols
                verts.append((xx+(u-.5)*.42*spread,y-.21-.045*math.cos(u*math.tau*4),z+zz))
        for j in range(3):
            for i in range(cols):a=j*(cols+1)+i;faces.append((a,a+1,a+cols+2,a+cols+1))
        mesh=bpy.data.meshes.new('Gathered curtain');mesh.from_pydata(verts,[],faces);mesh.update()
        obj=bpy.data.objects.new('Gathered curtain',mesh);bpy.context.collection.objects.link(obj)
        k.register(obj,'Gathered curtain',cloth,True)
        solid=obj.modifiers.new('Cloth thickness','SOLIDIFY');solid.thickness=.008;bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=solid.name)
        box('Curtain tie',(xx,y-.23,z+1.14),(.3,.07,.065),'HallGold',.015)
    # Diamond leaded panes sit just inside the original transparent glass.
    for offset in [-1.3,-.65,0,.65,1.3]:
        for slope in [-1,1]:
            hits=[]
            for xx in [-width/2+.06,width/2-.06]:
                zz=1.9+offset+slope*xx
                if 1.08<=zz<=2.72:hits.append((xx,zz))
            for zz in [1.08,2.72]:
                xx=(zz-1.9-offset)/slope
                if -width/2+.06<=xx<=width/2-.06:hits.append((xx,zz))
            if len(hits)==2:beam('Diamond window lead',(x+hits[0][0],y-.025,z+hits[0][1]),(x+hits[1][0],y-.025,z+hits[1][1]),.009,'HallGold')
    rotate_members(start,(x,y,z),rotation)

# Continuous wall finish follows the exact opening segments from the approved source.
k.group='RoomShell'
for rid in finished:
    x,y,w,d=rooms[rid]['rect']
    box('Pale ceiling '+rid,(x+w/2,y+d/2,8.79),(w-.27,d-.27,.06),'UpperPlaster',0)
    # Narrow wood joints imply boards while keeping one broad collision floor.
    for i in range(math.ceil(w/.42)):
        start=x+.14+i*.42;end=min(start+.413,x+w-.14)
        if end<=start:continue
        box('Oak floorboard '+rid,((start+end)/2,y+d/2,base+.009),(end-start,d-.28,.016),'UpperFloor' if i%3 else 'UpperFloorAlt',0)
    for wall in spec['walls']:
        if not wall['part'].startswith('Wall_upper_') or rid not in wall['owners']:continue
        lo,hi=wall['height'];a,b=wall['span'];axis=wall['axis'];line=wall['line']
        # Interior face, with dimensions clipped exactly to each doorway/window.
        sign=1 if line==(y if axis=='h' else x) else -1
        face=line+sign*.125
        for bottom,top,material in [(lo,min(hi,base+1.12),'HallWalnut'),(max(lo,base+1.12),hi,'UpperPlaster')]:
            if top<=bottom:continue
            center=((a+b)/2,face,(bottom+top)/2) if axis=='h' else (face,(a+b)/2,(bottom+top)/2)
            size=(b-a,.008,top-bottom) if axis=='h' else (.008,b-a,top-bottom)
            box('Continuous interior '+rid,center,size,material,0)
        for zz in [base+.12,base+.93,base+1.1,8.55]:
            if not lo+.04<zz<hi-.04:continue
            center=((a+b)/2,face+sign*.035,zz) if axis=='h' else (face+sign*.035,(a+b)/2,zz)
            size=(b-a,.07,.075) if axis=='h' else (.07,b-a,.075)
            box('Panel rail '+rid,center,size,'HallWalnutEdge',.013)
        if lo<=base+.2 and hi>=base+.9:
            count=max(1,math.ceil((b-a)/.65))
            for i in range(count):
                u=a+(i+.5)*(b-a)/count
                center=(u,face+sign*.026,base+.53) if axis=='h' else (face+sign*.026,u,base+.53)
                size=(.045,.05,.7) if axis=='h' else (.05,.045,.7)
                box('Panel stile '+rid,center,size,'HallWalnutEdge',.008)
    for xx in [x+.25,x+w/2,x+w-.25]:box('Ceiling beam '+rid,(xx,y+d/2,8.62),(.14,d-.3,.21),'HallWalnutEdge',.023)
    for yy in [y+.25,y+d/2,y+d-.25]:box('Ceiling crossbeam '+rid,(x+w/2,yy,8.61),(w-.3,.14,.23),'HallWalnutEdge',.023)

print('UPPER_SHELL_COMPLETE',flush=True)
k.group='StudyFurniture'
# Shelving follows the internal walls; the west windows at Y14/18 remain clear.
for xx in [1.6,4.1,6.5]:bookshelf(xx,21.54,base,1.9)
for yy in [11.5,20.3]:bookshelf(.48,yy,base,1.65,math.pi/2)
# Desk with open knee-space, pedestal drawers and a reading lamp.
x,y=3.8,17.1
for xx in [-1.02,1.02]:cabinet('Desk pedestal',x+xx,y,base,.65,1.18,.86,drawers=3)
box('Polished desktop',(x,y,base+.96),(3.12,1.43,.15),'HallWalnutEdge',.04)
box('Green leather blotter',(x,y-.08,base+1.043),(1.12,.65,.014),'HallSage',.009)
box('Open book left',(x-.2,y-.05,base+1.076),(.35,.42,.05),'UpperPaper',.008)
box('Open book right',(x+.16,y-.05,base+1.076),(.35,.42,.05),'UpperPaper',.008)
beam('Book center seam',(x-.02,y-.26,base+1.104),(x-.02,y+.16,base+1.104),.008,'UpperBookRed')
for i in range(3):box('Stacked research volume',(x-.95,y+.22,base+1.09+i*.075),(.47,.31,.072),'HallBook' if i%2 else 'UpperBookRed',.01)
lamp(x+1.0,y+.2,base+1.043)
lathe('Inkpot',(x+.64,y-.37,base+1.043),[(0,.06),(.07,.06),(.1,.034)],'HallGold')
beam('Quill',(x+.64,y-.37,base+1.12),(x+.72,y-.35,base+1.37),.012,'UpperLinen')
collider('Desktop',(x,y,base+.96),(3.16,1.47,.17))
chair(x,18.55,base,'UpperLeather')
chair(1.55,14.2,base,'HallSage',-.35)
cabinet('Reading table',2.6,14.3,base,.65,.65,.64,drawers=1)
lamp(2.6,14.3,base+.68)
cabinet('Globe stand',6.8,20.2,base,.78,.7,.87,drawers=1)
lathe('Globe base',(6.8,20.2,base+.92),[(0,.23),(.07,.25),(.11,.15),(.25,.065)],'HallGold')
sphere('Antique globe',(6.8,20.2,base+1.51),(.37,)*3,'UpperBlue')
# Continents as shallow rounded patches, plus a tilted brass meridian ring.
for a,z in [(0,.06),(1.8,.15),(3.4,-.1)]:
    sphere('Globe continent',(6.8+.34*math.cos(a),20.2+.34*math.sin(a),base+1.51+z),(.1,.08,.13),'UpperGreen')
for i in range(24):
    a=i*math.tau/24;b=(i+1)*math.tau/24
    beam('Globe brass meridian',(6.8+.42*math.cos(a),20.2,base+1.51+.42*math.sin(a)),(6.8+.42*math.cos(b),20.2,base+1.51+.42*math.sin(b)),.014,'HallGold')
painting('Study landscape',4.05,21.78,base+3.68,1.65,.86,2)
k.group='StudyWindows'
for yy in [14,18]:curtains(.03,yy,base,1.5,math.pi/2,'UpperBookRed')
k.group='StudyRug';rug(3.8,17.15,base,4.6,4.3,'HallRugDark')

k.group='PrivateLibrary'
for xx in [1.5,3.8,6.2]:bookshelf(xx,27.52,base,1.8)
bookshelf(.48,26.35,base,1.65,math.pi/2)
cabinet('Library reading table',3.7,24.95,base,1.8,.85,.8,drawers=1)
books(3.7,25.04,base+.85,1.1)
chair(3.7,26.05,base,'HallSage')
k.group='PrivateBedroom'
bed(4.8,7.8,base,2.0,'HallSage')
for xx in [3.3,6.3]:
    cabinet('Professor bedside cabinet',xx,8.55,base,.65,.55,.68,drawers=2);lamp(xx,8.55,base+.73)
cabinet('Professor dresser',8.8,4.5,base,2.0,.62,1.07,-math.pi/2,3)
chair(1.4,3.4,base,'UpperLeather',math.pi/2)
painting('Bedroom manor picture',4.8,9.79,base+2.5,1.4,1.0,2)
k.group='BedroomWindows'
for xx in [3,7]:curtains(xx,.03,base,1.5,math.pi,'UpperGreen')
curtains(.03,5,base,1.5,math.pi/2,'UpperGreen')
k.group='BedroomRug';rug(4.8,5.9,base,4.6,3.3,'HallRugDark')

for rid,cloth in [('E1','UpperRose'),('K-BOYS','UpperBlue')]:
    x,y,w,d=rooms[rid]['rect'];cx=x+w/2
    k.group=rid+'_Furniture'
    for bx in [x+1.875,x+w-1.925]:
        bed(bx,32.1,base,1.55,cloth)
        near=bx+1.13 if bx<cx else bx-1.13
        cabinet('Bedside drawers',near,32.65,base,.55,.52,.67,drawers=2);lamp(near,32.65,base+.72)
        if bx<cx:painting('Bed portrait',bx-(.14 if rid=='K-BOYS' else 0),33.79,base+2.38,.72,.88,1 if rid=='E1' else 0)
        else:painting('Bed landscape',bx+(.4 if rid=='E1' else 0),33.79,base+2.38,.92,.72,2 if rid=='E1' else 3)
    # Door is centered at Y28; wardrobes/dressers stay at the ends of the room.
    cabinet('Bedroom dresser',x+w-.53,29.65,base,1.55,.55,.98,-math.pi/2,3)
    k.vase(x+w-.53,29.35,base+1.02,.65)
    cabinet('Bedroom wardrobe',x+.62,29.65,base,1.55,.7,2.4,math.pi/2,0)
    # Replace drawer-style wardrobe fronts with tall panel doors on a separate facade.
    for yy in [-.38,.38]:
        box('Wardrobe door panel',(x+1.002,29.65+yy,base+1.29),(.045,.64,1.94),'HallWalnut',.02)
        sphere('Wardrobe round knob',(x+1.048,29.65+yy*.4,base+1.15),(.035,)*3,'HallGold')
    chair(x+w-1.05,31.0,base,'HallSage',-math.pi/2)
    # Rounded luggage and straps at the wardrobe; no obstacle across the central aisle.
    box('Travel case',(x+1.9,29.05,base+.25),(.72,.43,.47),'UpperLeather',.06)
    for xx in [-.23,.23]:box('Luggage strap',(x+1.9+xx,29.05,base+.25),(.06,.45,.49),'HallGold',.014)
    beam('Travel case handle',(x+1.78,29.05,base+.51),(x+2.02,29.05,base+.51),.025,'HallWalnut')
    collider('Travel case',(x+1.9,29.05,base+.27),(.76,.47,.54))
    k.group=rid+'_Windows'
    wx=35 if rid=='E1' else 42
    curtains(wx,33.97,base,1.5,0,'UpperLinen' if rid=='E1' else 'UpperBlue')
    if rid=='K-BOYS':
        # The approved X47 north window meets the existing bathroom partition.
        # Finish the visible narrow reveal with a closed wooden shutter; no opening moves.
        for xx in [46.27,46.84]:box('Corner window reveal',(xx,33.79,base+1.9),(.065,.09,1.88),'HallWalnutEdge',.012)
        for zz in [1.0,2.8]:box('Corner window rail',(46.555,33.79,base+zz),(.64,.09,.08),'HallWalnutEdge',.012)
        box('Corner window closed shutter',(46.555,33.82,base+1.9),(.51,.035,1.7),'HallWalnut',.012)
        for zz in [1.18+i*.13 for i in range(12)]:box('Shutter slat',(46.555,33.791,base+zz),(.46,.04,.07),'HallWalnutEdge',.01)
        sphere('Shutter catch',(46.75,33.75,base+1.85),(.025,)*3,'HallGold')
    k.group=rid+'_Rug';rug(cx,30.65,base,5.0,3.4,'HallRugDark' if rid=='E1' else 'UpperGreen')

print('UPPER_FURNITURE_COMPLETE',flush=True)
# The assembled model is checked before merging; intentional floor/wall finish contacts excluded.
checks=['StudyFurniture','PrivateLibrary','PrivateBedroom','E1_Furniture','K-BOYS_Furniture']
k.validate_architecture(project,checks,output/'PlacementValidation.json')
k.validate_group_pairs([('StudyFurniture','StudyWindows'),('PrivateBedroom','BedroomWindows'),('E1_Furniture','E1_Windows'),('K-BOYS_Furniture','K-BOYS_Windows')],output/'DecorationClearance.json')
k.export(project,source,output,'UpperFinish','UpperFinish.json','upper-finish-01')
os._exit(0)
