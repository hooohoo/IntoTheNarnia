"""Finish the artifact gallery and eight public connecting corridors.

Uses the approved openings/floors; preserves the west-window accident and escape.
Blender --background --python-exit-code 1 --python this_file -- --project PATH
"""
import argparse
import json
import math
import os
from pathlib import Path
import sys
import bpy
from mathutils import Matrix, Vector
from mathutils.bvhtree import BVHTree
sys.path.insert(0,str(Path(__file__).resolve().parent))
from professor_house_finish_kit import FinishKit

parser=argparse.ArgumentParser()
parser.add_argument('--project',required=True)
options=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
project=Path(options.project).resolve()
source=project/'ArtSource/ProfessorHouse/GalleryFinish_v1'
output=project/'Assets/@Project/10.Models/ProfessorHouseGalleryFinish'
source.mkdir(parents=True,exist_ok=True);output.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version=0
palette={'HallWalnut':'513122','HallWalnutEdge':'795039','HallGold':'BE9253','HallStone':'DCCDB3','HallIvory':'EEE1C6','HallSage':'647C61','HallLeaf':'516B38','HallPetal':'F1DFB0','HallRugDark':'62362E','HallFlame':'FFE2A4','HallPainting':'FFFFFF','HallBook':'586F72',
         'GalleryPlaster':'E7D8B9','GalleryTile':'CFC2A7','GalleryTileLight':'DECFB5','GalleryGrout':'988F7A','GalleryFloor':'876044','GalleryFloorAlt':'936D50','GallerySteel':'88959B','GallerySteelEdge':'BFC4BD','GalleryDarkMetal':'303C40','GalleryGlass':'B5D6D0','GalleryBlue':'516F80','GalleryBronze':'997047'}
k=FinishKit(palette)
box=k.box;sphere=k.sphere;beam=k.beam;lathe=k.lathe;painting=k.painting;collider=k.collider
layout=json.loads((project/'Docs/Art/ProfessorHouse_ApprovedLayout_v1/Data/layout.json').read_text())
spec=json.loads((project/'ArtSource/ProfessorHouse/ConnectedLayout_v1/Data/model-spec.json').read_text())
finished={'ground':['D1','F0','F1','F2'],'upper':['E3','F0','F1','F2','K0']}

def rotate_members(group,start,origin,angle):
    bpy.context.view_layer.update()
    transform=Matrix.Translation(origin) @ Matrix.Rotation(angle,4,'Z') @ Matrix.Translation(-Vector(origin))
    for obj in k.groups[group][start:]:obj.matrix_world=transform @ obj.matrix_world

def panel(axis,face,a,b,lo,hi,base,sign,rid,top):
    # Original wall segments already exclude every doorway and window.
    for bottom,upper,mat in [(lo,min(hi,base+1.12),'HallWalnut'),(max(lo,base+1.12),hi,'GalleryPlaster')]:
        if upper<=bottom:continue
        pos=((a+b)/2,face,(bottom+upper)/2) if axis=='h' else (face,(a+b)/2,(bottom+upper)/2)
        size=(b-a,.008,upper-bottom) if axis=='h' else (.008,b-a,upper-bottom)
        box('Wall finish '+rid,pos,size,mat,0)
    for zz,thick in [(base+.12,.14),(base+.96,.065),(base+1.12,.075),(top-.19,.14)]:
        if not lo+.04<zz<hi-.04:continue
        pos=((a+b)/2,face+sign*.035,zz) if axis=='h' else (face+sign*.035,(a+b)/2,zz)
        size=(b-a,.07,thick) if axis=='h' else (.07,b-a,thick)
        box('Panel rail '+rid,pos,size,'HallWalnutEdge',.012)
    if lo<=base+.2 and hi>=base+.9:
        count=max(1,math.ceil((b-a)/.7))
        for i in range(count):
            u=a+(i+.5)*(b-a)/count
            pos=(u,face+sign*.025,base+.55) if axis=='h' else (face+sign*.025,u,base+.55)
            size=(.045,.05,.68) if axis=='h' else (.05,.045,.68)
            box('Panel stile '+rid,pos,size,'HallWalnutEdge',.008)

def runner(x,y,z,w,d):
    box('Gallery woven runner',(x,y,z+.027),(w,d,.025),'HallRugDark',.005)
    for margin in [.13,.22]:
        for xx in [-w/2+margin,w/2-margin]:box('Runner gold edge',(x+xx,y,z+.043),(.02,d-2*margin,.005),'HallGold',0)
        for yy in [-d/2+margin,d/2-margin]:box('Runner gold edge',(x,y+yy,z+.043),(w-2*margin,.02,.005),'HallGold',0)
    length=max(w,d);count=max(1,int(length/1.8))
    for i in range(count):
        u=(i-(count-1)/2)*1.6;xx=x+(u if w>d else 0);yy=y+(u if d>=w else 0)
        obj=box('Runner woven diamond',(xx,yy,z+.046),(.32,.32,.004),'HallGold',0);obj.rotation_euler.z=math.pi/4
        obj=box('Runner inset diamond',(xx,yy,z+.049),(.24,.24,.004),'HallRugDark',0);obj.rotation_euler.z=math.pi/4

for floor,ids in finished.items():
    fd=layout['floors'][floor];base=fd['height'];top=4.08 if floor=='ground' else 8.84
    for rid in ids:
        k.group=floor+'_'+rid+'_Shell'
        room=next(r for r in fd['rooms'] if r['id']==rid);x,y,w,d=room['rect']
        box('Ceiling '+rid,(x+w/2,y+d/2,top-.05),(w-.31,d-.31,.06),'GalleryPlaster',0)
        if rid=='D1':
            box('Stone joint bed',(5,16,.005),(9.7,11.7,.01),'GalleryGrout',0)
            for ix in range(12):
                for iy in range(15):
                    xx=.16+ix*.807;yy=10.16+iy*.778
                    box('Rounded limestone tile',(xx+.398,yy+.383,.018),(.796,.766,.022),'GalleryTile' if (ix+iy)%4 else 'GalleryTileLight',.003)
        else:
            for i in range(math.ceil(w/.45)):
                a=x+.16+i*.45;b=min(a+.443,x+w-.16)
                if b>a:box('Corridor oak board',((a+b)/2,y+d/2,base+.009),(b-a,d-.32,.016),'GalleryFloor' if i%3 else 'GalleryFloorAlt',0)
        for wall in spec['walls']:
            if not wall['part'].startswith('Wall_'+floor+'_') or rid not in wall['owners']:continue
            lo,hi=wall['height'];a,b=wall['span'];axis=wall['axis'];line=wall['line']
            sign=1 if line==(y if axis=='h' else x) else -1
            panel(axis,line+sign*.125,a,b,lo,hi,base,sign,rid,top)
        # Hall-owned boundaries are absent from spec.walls: dress their opposite face.
        if rid=='D1':
            for a,b in [(10.31,12.2),(16.13,21.69)]:panel('v',9.845,a,b,0,4.0,0,-1,rid,top)
        if rid=='F0':
            intervals=[(10.31,14.9),(17.1,21.69)] if floor=='ground' else [(10.31,21.69)]
            for a,b in intervals:panel('v',24.155,a,b,base,top,base,1,rid,top)
        if rid=='E3':panel('h',9.845,11.9,22.65,base,top,base,-1,rid,top)
        # Coffers run across the short dimension. End clearances respect wall frames.
        if w>=d:
            for i in range(max(2,math.ceil(w/3))):
                xx=x+.35+i*(w-.7)/(max(2,math.ceil(w/3))-1)
                box('Corridor crossbeam',(xx,y+d/2,top-.21),(.15,d-.34,.21),'HallWalnutEdge',.022)
        else:
            for i in range(max(2,math.ceil(d/3))):
                yy=y+.35+i*(d-.7)/(max(2,math.ceil(d/3))-1)
                end=x+w-.17
                if rid=='D1' and 12.2<yy<15.8:end=9.62  # Stop before the existing stone arch face.
                box('Corridor crossbeam',((x+.17+end)/2,yy,top-.21),(end-x-.17,.15,.21),'HallWalnutEdge',.022)
        k.group='Runners'
        if rid=='D1':runner(5.4,16.1,base,4.6,5.4)
        elif rid=='K0':runner(x+w/2+.4,y+d/2,base,w-2.2,1.05)
        elif w>=d:runner(x+w/2,y+d/2,base,w-2.2,1.45)
        else:runner(x+w/2,y+d/2,base,1.8,d-2.4)

def urn(x,y,z,scale=1,color='GalleryBlue'):
    lathe('Collected ceramic urn',(x,y,z),[(h*scale,r*scale) for h,r in [(0,.13),(.05,.16),(.1,.11),(.26,.2),(.4,.15),(.47,.08),(.53,.12)]],color)
    for zz,rr in [(.055,.16),(.26,.201),(.49,.105)]:
        lathe('Urn gold band',(x,y,z+zz*scale),[(0,rr*scale),(.015*scale,rr*scale)],'HallGold')

def trophy(x,y,z):
    lathe('Antique trophy',(x,y,z),[(0,.12),(.04,.13),(.09,.065),(.26,.045),(.3,.12),(.38,.18),(.52,.21),(.55,.22)],'HallGold')
    for side in [-1,1]:
        for i in range(8):
            a=-math.pi/2+i*math.pi/8;b=a+math.pi/8
            beam('Trophy loop handle',(x+side*(.15+.13*math.cos(a)),y,z+.4+.12*math.sin(a)),(x+side*(.15+.13*math.cos(b)),y,z+.4+.12*math.sin(b)),.018,'HallGold')

def armillary(x,y,z,scale=1):
    lathe('Armillary foot',(x,y,z),[(0,.17*scale),(.05*scale,.2*scale),(.12*scale,.08*scale),(.23*scale,.055*scale)],'HallGold')
    zz=z+.55*scale
    sphere('Armillary center',(x,y,zz),(.085*scale,)*3,'HallGold')
    for plane in range(3):
        for i in range(24):
            a=i*math.tau/24;b=(i+1)*math.tau/24
            def at(t):
                u=.31*scale*math.cos(t);v=.31*scale*math.sin(t)
                return (x+u,y+v,zz) if plane==0 else ((x+u,y,zz+v) if plane==1 else (x,y+u,zz+v))
            beam('Armillary brass ring',at(a),at(b),.014*scale,'HallGold')

def horse(x,y,z):
    sphere('Bronze horse body',(x,y,z+.25),(.25,.1,.14),'GalleryBronze')
    for xx in [-.16,.16]:
        for yy in [-.065,.065]:beam('Horse leg',(x+xx,y+yy,z+.03),(x+xx,y+yy,z+.25),.025,'GalleryBronze')
    beam('Horse neck',(x+.17,y,z+.28),(x+.23,y,z+.49),.075,'GalleryBronze')
    sphere('Horse head',(x+.27,y,z+.51),(.12,.065,.065),'GalleryBronze')
    beam('Horse tail',(x-.23,y,z+.28),(x-.32,y,z+.13),.023,'GalleryBronze')
    for yy in [-.035,.035]:sphere('Horse ear',(x+.23,y+yy,z+.59),(.025,.018,.055),'GalleryBronze')

def case(name,x,y,w=2.4,d=.76,tall=True,rotation=0,contents=0):
    group=k.group;start=len(k.groups[group]);glass_start=len(k.groups['CaseGlass'])
    h=.85;top=2.16 if tall else 1.63
    box(name+' base',(x,y,h/2),(w,d,h),'HallWalnut',.035)
    for xx in [-w/2+.08,w/2-.08]:
        for yy in [-d/2+.07,d/2-.07]:box('Case foot',(x+xx,y+yy,.08),(.15,.15,.16),'HallWalnutEdge',.025)
    for side in [-1,1]:
        xx=x+side*w*.25
        box('Inset cabinet door',(xx,y-d/2-.018,.47),(w*.46,.045,.57),'HallWalnutEdge',.018)
        sphere('Cabinet brass knob',(x+side*.08,y-d/2-.065,.5),(.028,)*3,'HallGold')
    for zz in [.12,h,top]:box('Case moulding',(x,y,zz),(w+.12,d+.09,.09),'HallWalnutEdge',.023)
    box('Case back',(x,y+d/2-.025,(h+top)/2),(w,.05,top-h),'HallWalnut',.015)
    for xx in [-w/2+.035,w/2-.035]:
        for yy in [-d/2+.035,d/2-.035]:box('Glazed case corner',(x+xx,y+yy,(h+top)/2),(.055,.055,top-h),'HallGold',.01)
    box('Case felt shelf',(x,y,h+.06),(w-.12,d-.12,.02),'HallSage',.004)
    # Thin glass surfaces are a separate mesh/material; the contents remain readable.
    k.group='CaseGlass'
    verts=[(x-w/2+.07,y-d/2+.012,h+.06),(x+w/2-.07,y-d/2+.012,h+.06),(x+w/2-.07,y-d/2+.012,top-.06),(x-w/2+.07,y-d/2+.012,top-.06)]
    mesh=bpy.data.meshes.new('Display glass');mesh.from_pydata(verts,[],[(0,1,2,3)]);mesh.update()
    obj=bpy.data.objects.new('Display glass',mesh);bpy.context.collection.objects.link(obj);k.register(obj,'Display glass','GalleryGlass')
    k.group=group
    z=h+.08
    if contents==0:
        armillary(x-.5,y,z,.95 if tall else .5);urn(x+.5,y,z,.95 if tall else .6)
    elif contents==1:
        trophy(x-.55,y,z);urn(x+.12,y,z,.85);horse(x+.63,y,z)
    else:
        urn(x-.53,y,z,.8 if tall else .65);trophy(x+.45,y,z)
    rotate_members(group,start,(x,y,0),rotation)
    rotate_members('CaseGlass',glass_start,(x,y,0),rotation)
    size=(w+.14,d+.16,top+.1) if abs(math.sin(rotation))<.5 else (d+.16,w+.14,top+.1)
    collider(name,(x,y,(top+.1)/2),size)

k.group='GalleryCases'
case('North armillary cabinet',2.25,21.30,2.55,contents=0)
case('North trophy cabinet',8.0,21.30,2.45,contents=1)
case('South ceramic cabinet',7.65,10.73,2.3,contents=2,rotation=math.pi)
case('Window low case',.68,14,1.85,.76,False,math.pi/2,0)
case('East low case',9.08,19,2.1,.76,False,-math.pi/2,2)

k.group='GalleryPedestals'
for x,y in [(6.25,21.25),(1.05,20.5)]:
    for zz,w,h in [(.08,.65,.16),(.21,.54,.1),(.73,.42,.94),(1.22,.57,.11)]:box('Marble bust pedestal',(x,y,zz),(w,w,h),'HallStone',.025)
    lathe('Bust socle',(x,y,1.28),[(0,.21),(.06,.23),(.16,.13)],'HallIvory')
    sphere('Bust shoulders',(x,y,1.55),(.28,.17,.18),'HallIvory')
    sphere('Bust neck',(x,y,1.74),(.085,.085,.16),'HallIvory')
    sphere('Bust head',(x,y,1.94),(.15,.13,.2),'HallIvory')
    sphere('Bust nose',(x,y-.133,1.94),(.035,.04,.05),'HallStone')
    for xx in [-.058,.058]:sphere('Sculpted brow',(x+xx,y-.12,2.005),(.043,.022,.016),'HallStone')
    collider('Bust pedestal',(x,y,1.1),(.69,.69,2.2))

# The knight stays on the approved accident axis; helmet/shoulder meshes are separable.
x,y=2.3,18
k.group='ArmorBase'
box('Armor stone plinth',(x,y,.13),(1.22,1.02,.26),'HallStone',.04)
box('Armor plinth step',(x,y,.285),(1.12,.92,.07),'GalleryTileLight',.025)
box('Armor brass label',(x,y-.518,.16),(.38,.018,.13),'HallGold',.016)
collider('Armor plinth',(x,y,.16),(1.24,1.04,.32))
k.group='ArmorBody'
for side in [-1,1]:
    xx=x+side*.18
    sphere('Rounded steel sabaton',(xx,y-.095,.405),(.145,.235,.11),'GallerySteel')
    beam('Greave',(xx,y,.46),(xx,y,.80),.105,'GallerySteel')
    sphere('Round knee plate',(xx,y-.035,.87),(.135,.125,.13),'GallerySteelEdge')
    beam('Thigh armor',(xx,y,.92),(x+side*.17,y,1.19),.125,'GallerySteel')
    sphere('Layered hip tasset',(x+side*.20,y-.04,1.20),(.19,.16,.21),'GallerySteel')
lathe('Rounded cuirass',(x,y,1.25),[(0,.23),(.09,.25),(.37,.32),(.52,.25)],'GallerySteel')
lathe('Cuirass brass belt',(x,y,1.30),[(0,.252),(.045,.26)],'HallGold')
lathe('Gorget',(x,y,1.76),[(0,.17),(.08,.18),(.12,.13)],'GallerySteelEdge')
for side in [-1,1]:
    beam('Upper arm',(x+side*.37,y,1.64),(x+side*.44,y,1.36),.09,'GallerySteel')
    sphere('Elbow plate',(x+side*.44,y-.025,1.32),(.115,.11,.12),'GallerySteelEdge')
    beam('Forearm',(x+side*.44,y,1.28),(x+side*.43,y-.06,1.08),.09,'GallerySteel')
    sphere('Gauntlet',(x+side*.43,y-.07,1.03),(.105,.09,.12),'GallerySteel')
collider('Armor body',(x,y,1.26),(1.15,.64,1.9))
k.group='ArmorShoulders'
for side in [-1,1]:
    sphere('Rounded detachable pauldron',(x+side*.34,y,1.73),(.22,.20,.15),'GallerySteel')
    for i in range(3):sphere('Pauldron brass rivet',(x+side*(.27+i*.065),y-.18,1.71),(.018,)*3,'HallGold')
k.group='ArmorHelmet'
sphere('Detachable rounded helmet',(x,y,2.035),(.23,.21,.24),'GallerySteel')
box('Helmet dark visor slit',(x,y-.205,2.055),(.32,.028,.055),'GalleryDarkMetal',.012)
beam('Helmet visor brow',(x-.17,y-.21,2.10),(x+.17,y-.21,2.10),.018,'GallerySteelEdge')
for xx in [-.095,-.047,0,.047,.095]:beam('Helmet breathing slots',(x+xx,y-.207,1.95),(x+xx,y-.217,2.005),.009,'GalleryDarkMetal')
beam('Helmet center ridge',(x,y-.11,2.23),(x,y+.12,2.23),.018,'GallerySteelEdge')

k.group='GalleryPictures'
painting('Gallery north landscape',2.25,21.73,3.04,1.75,1.25,2)
painting('Gallery north portrait',8.0,21.73,3.04,1.05,1.25,0)
painting('Gallery west landscape',.29,16,2.05,1.1,1.3,3,math.pi/2)
painting('Gallery east portrait',9.57,19,2.68,1.2,1.5,1,-math.pi/2)
painting('Gallery south landscape',2.1,10.27,2.4,1.8,1.25,2,math.pi)
for yy in [11.5,20.3]:k.sconce(.31,yy,2.55,math.pi/2)
for xx in [3.8,6.35]:k.sconce(xx,21.70,2.9)

# Leaded frames are decorations attached inside existing glazing, including the accident window.
k.group='GalleryWindows'
for cy,width in [(14,1.5),(18,2.3)]:
    x=.025;z=1.9
    for yy in [cy-width/2+.065,cy+width/2-.065]:beam('Leaded window border',(x,yy,1.08),(x,yy,2.72),.009,'HallGold')
    for zz in [1.08,2.72]:beam('Leaded window border',(x,cy-width/2+.065,zz),(x,cy+width/2-.065,zz),.009,'HallGold')
    for offset in [-1.3,-.65,0,.65,1.3]:
        for slope in [-1,1]:
            hits=[]
            for yy in [-width/2+.065,width/2-.065]:
                zz=z+offset+slope*yy
                if 1.08<=zz<=2.72:hits.append((yy,zz))
            for zz in [1.08,2.72]:
                yy=(zz-z-offset)/slope
                if -width/2+.065<=yy<=width/2-.065:hits.append((yy,zz))
            if len(hits)==2:beam('Diamond lead',(x,cy+hits[0][0],hits[0][1]),(x,cy+hits[1][0],hits[1][1]),.009,'HallGold')

# Art stays on solid wall spans. In the 2 m children's corridor only shallow frames are used.
k.group='CorridorPictures'
for floor,base in [('ground',0),('upper',4.32)]:
    for yy,tile in [(11.7,0),(18.1,2),(21.6,1)]:
        painting('East corridor portrait',49.72,yy,base+2.2,.85,1.14,tile,-math.pi/2)
    for xx,tile in [(36.3,2),(42.8,3)]:painting('Front gallery landscape',xx,9.73,base+2.22,1.7,1.15,tile)
    painting('Stair corridor landscape',29.73,16.6,base+2.25,1.7,1.2,2,-math.pi/2)
    for yy in [11.3,18.1,21.4]:k.sconce(44.30,yy,base+2.6,math.pi/2)
    for xx in [35.3,44.0]:k.sconce(xx,9.69,base+2.6)
    for yy in [11.4,18.6]:k.sconce(29.70,yy,base+2.6,-math.pi/2)
for xx,tile in [(14,0),(18,2),(30.5,1)]:painting('Family corridor portrait',xx,9.69,6.62,1.05,1.38,tile)
for xx in [32.1,37.6,45.1]:painting('Children corridor landscape',xx,26.27,6.52,1.15,.85,3,math.pi)
k.group='CorridorSeats'
for x,y,z,angle in [(44.72,21.9,0,math.pi/2),(44.72,21.9,4.32,math.pi/2),(40,9.35,0,0),(40,9.35,4.32,0),(17,9.27,4.32,0)]:
    start=len(k.groups[k.group])
    box('Bench seat',(x,y,z+.47),(1.7,.56,.12),'HallWalnutEdge',.045)
    box('Bench cushion',(x,y,z+.57),(1.52,.48,.12),'HallSage',.055)
    for xx in [-.64,.64]:
        for yy in [-.17,.17]:lathe('Bench turned foot',(x+xx,y+yy,z+.025),[(0,.045),(.07,.06),(.31,.035),(.4,.05)],'HallWalnut')
    rotate_members(k.group,start,(x,y,z),angle)
    collider('Corridor bench',(x,y,z+.34),(.62,1.75,.68) if angle else (1.75,.62,.68))

# The approved east window at Y6 straddles a partition. Finish the visible half.
k.group='CorridorEndWindows'
for base in [0,4.32]:
    for yy in [6.15,6.75]:box('End half-window jamb',(49.78,yy,base+1.9),(.09,.065,1.88),'HallWalnutEdge',.012)
    for zz in [1.0,2.8]:box('End half-window rail',(49.78,6.45,base+zz),(.09,.66,.08),'HallWalnutEdge',.012)
    box('End half-window shutter',(49.81,6.45,base+1.9),(.035,.51,1.7),'HallWalnut',.012)
    for i in range(12):box('End window shutter slat',(49.78,6.45,base+1.18+i*.13),(.04,.46,.07),'HallWalnutEdge',.01)
    sphere('End shutter catch',(49.74,6.64,base+1.85),(.025,)*3,'HallGold')

print('GALLERY_FURNITURE_COMPLETE',flush=True)
checks=['GalleryCases','GalleryPedestals','ArmorBase','ArmorBody','ArmorShoulders','ArmorHelmet','GalleryPictures','CorridorPictures','CorridorSeats']
k.validate_architecture(project,checks,output/'PlacementValidation.json')
k.validate_group_pairs([('GalleryCases','GalleryPictures'),('GalleryCases','GalleryPedestals'),('GalleryCases','ArmorBody'),('CorridorSeats','CorridorPictures')],output/'DecorationClearance.json')

# Trace actual mesh surfaces from just inside the broken pane to the helmet.
bpy.context.view_layer.update()
origin=Vector((.23,18.12,2.04));hits=[]
for group in checks:
    for obj in k.groups[group]:
        tree=BVHTree.FromPolygons([obj.matrix_world @ v.co for v in obj.data.vertices],[tuple(f.vertices) for f in obj.data.polygons])
        hit,normal,index,distance=tree.ray_cast(origin,Vector((1,0,0)),3)
        if hit is not None:hits.append((distance,group,obj.name,list(hit)))
hits.sort()
if not hits or hits[0][1]!='ArmorHelmet':raise RuntimeError('Accident flight is obstructed: '+str(hits[:3]))
(output/'AccidentAxis.json').write_text(json.dumps(dict(method='actual mesh BVH ray from inside the accident pane; not a runtime break simulation',origin=list(origin),direction=[1,0,0],firstHit=hits[0],armorCenter=[2.3,18],windowCenter=[0,18],windowWidth=2.3,escapeRouteUnchanged=True),indent=2)+'\n')
k.export(project,source,output,'GalleryFinish','GalleryFinish.json','gallery-finish-01')
os._exit(0)
