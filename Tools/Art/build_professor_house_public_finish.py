"""Rounded reception, dining room, public library and arrival foyer.

Approved ground-floor openings and story routes remain unchanged.
Blender --background --python-exit-code 1 --python this_file -- --project PATH
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

parser = argparse.ArgumentParser()
parser.add_argument('--project', required=True)
options = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
project = Path(options.project).resolve()
source = project/'ArtSource/ProfessorHouse/PublicFinish_v1'
output = project/'Assets/@Project/10.Models/ProfessorHousePublicFinish'
source.mkdir(parents=True, exist_ok=True)
output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
palette = {'HallWalnut':'513122','HallWalnutEdge':'795039','HallGold':'BE9253','HallStone':'DCCDB3','HallIvory':'EEE1C6','HallSage':'647C61','HallLeaf':'516B38','HallPetal':'F1DFB0','HallRugDark':'62362E','HallFlame':'FFE2A4','HallPainting':'FFFFFF','HallBook':'586F72',
           'PublicPlaster':'E5D5B6','PublicFloor':'886044','PublicFloorAlt':'956C4C','PublicStone':'DDD1B9','PublicStoneAlt':'B8AE96','PublicRose':'99665C','PublicLinen':'E3D0A6','PublicBookRed':'844D42','PublicBookGreen':'69765B','PublicInk':'302C29','PublicChina':'F2E6CB','PublicBlue':'608184','PublicFire':'DE8736'}
k = FinishKit(palette)
box=k.box; sphere=k.sphere; beam=k.beam; lathe=k.lathe; collider=k.collider
layout = json.loads((project/'Docs/Art/ProfessorHouse_ApprovedLayout_v1/Data/layout.json').read_text())
spec = json.loads((project/'ArtSource/ProfessorHouse/ConnectedLayout_v1/Data/model-spec.json').read_text())
ground = layout['floors']['ground']
rooms = {r['id']:r for r in ground['rooms']}

def rotate(start, origin, angle):
    bpy.context.view_layer.update()
    transform = Matrix.Translation(origin) @ Matrix.Rotation(angle,4,'Z') @ Matrix.Translation(-Vector(origin))
    for obj in k.groups[k.group][start:]: obj.matrix_world = transform @ obj.matrix_world

def obstacle(name, x, y, w, d, h, angle=0):
    # Exact AABB for a rotated footprint, including the non-square dining seats.
    c,s=abs(math.cos(angle)),abs(math.sin(angle))
    collider(name,(x,y,h/2),(w*c+d*s,w*s+d*c,h))

def panel(axis, face, a, b, lo, hi, sign):
    for bottom, top, mat in [(lo,min(hi,1.12),'HallWalnut'),(max(lo,1.12),hi,'PublicPlaster')]:
        if top <= bottom: continue
        pos=((a+b)/2,face,(bottom+top)/2) if axis=='h' else (face,(a+b)/2,(bottom+top)/2)
        size=(b-a,.008,top-bottom) if axis=='h' else (.008,b-a,top-bottom)
        box('Wall finish',pos,size,mat,0)
    for z,h in [(.12,.14),(.96,.065),(1.12,.075),(3.87,.14)]:
        if not lo+.04 < z < hi-.04: continue
        pos=((a+b)/2,face+sign*.035,z) if axis=='h' else (face+sign*.035,(a+b)/2,z)
        size=(b-a,.07,h) if axis=='h' else (.07,b-a,h)
        box('Wall rail',pos,size,'HallWalnutEdge',.012)
    if lo<=.2 and hi>=.9:
        count=max(1,math.ceil((b-a)/.7))
        for i in range(count):
            u=a+(i+.5)*(b-a)/count
            pos=(u,face+sign*.025,.55) if axis=='h' else (face+sign*.025,u,.55)
            box('Panel stile',pos,(.045,.05,.68) if axis=='h' else (.05,.045,.68),'HallWalnutEdge',.008)

def rug(x,y,w,d,cloth='HallRugDark'):
    box('Woven rug',(x,y,.033),(w,d,.018),cloth,.004)
    for margin in [.14,.23]:
        for yy in [-d/2+margin,d/2-margin]:box('Rug border',(x,y+yy,.045),(w-2*margin,.022,.005),'HallGold',0)
        for xx in [-w/2+margin,w/2-margin]:box('Rug border',(x+xx,y,.045),(.022,d-2*margin,.005),'HallGold',0)
    for xx in [-w*.28,0,w*.28]:
        obj=box('Rug medallion',(x+xx,y,.049),(.42,.42,.004),'HallGold',0);obj.rotation_euler.z=math.pi/4
        obj=box('Rug medallion inset',(x+xx,y,.052),(.3,.3,.003),cloth,0);obj.rotation_euler.z=math.pi/4

for rid in ['C1','A','C2','D2']:
    x,y,w,d=rooms[rid]['rect']; k.group=rid+'_Shell'
    box('Plaster ceiling',(x+w/2,y+d/2,4.025),(w-.32,d-.32,.06),'PublicPlaster',0)
    if rid=='A':
        for ix in range(14):
            for iy in range(10):
                box('Foyer limestone',(10.17+(ix+.5)*13.66/14,.17+(iy+.5)*9.66/10,.018),(13.66/14-.012,9.66/10-.012,.022),'PublicStone' if (ix+iy)%2 else 'PublicStoneAlt',.003)
    else:
        for ix in range(math.ceil(w/.45)):
            a=x+.16+ix*.45;b=min(a+.443,x+w-.16)
            if b>a:box('Oak floorboard',((a+b)/2,y+d/2,.017),(b-a,d-.32,.024),'PublicFloor' if ix%3 else 'PublicFloorAlt',0)
    for wall in spec['walls']:
        if not wall['part'].startswith('Wall_ground_') or rid not in wall['owners']:continue
        lo,hi=wall['height'];a,b=wall['span'];axis=wall['axis'];line=wall['line']
        sign=1 if line==(y if axis=='h' else x) else -1
        panel(axis,line+sign*.125,a,b,lo,hi,sign)
    if rid=='A':
        # Reverse face of the hall wall, whose structure is owned by the hall chunk.
        for a,b in [(10.3,15.5),(18.5,23.7)]:panel('h',9.845,a,b,0,4.0,-1)
        panel('h',9.845,15.5,18.5,3.2,4.0,-1)
    # Two-direction coffers have deliberate join contacts; not standalone props.
    for yy in [y+1.3,y+d/2,y+d-1.3]:
        box('Ceiling crossbeam',(x+w/2,yy,3.87),(w-.34,.16,.2),'HallWalnutEdge',.02)
    for xx in [x+w*.28,x+w*.72]:
        box('Ceiling long beam',(xx,y+d/2,3.9),(.14,d-.34,.14),'HallWalnutEdge',.02)

def books(x,y,z,width,depth=.24,height=.38):
    count=max(2,int(width/.125));step=width/count
    for i in range(count):
        h=height*(.76+.24*random.random());xx=x-width/2+(i+.5)*step
        box('Book cover',(xx,y,z+h/2),(step*.87,depth,h),['HallBook','PublicBookRed','PublicBookGreen','PublicLinen'][i%4],.008)
        for zz in [z+.055,z+h-.045]:box('Book spine band',(xx,y-depth/2-.004,zz),(step*.6,.008,.008),'HallGold',0)

def lamp(x,y,z):
    lathe('Brass reading lamp',(x,y,z),[(0,.14),(.04,.16),(.08,.08),(.3,.055),(.34,.1)],'HallGold')
    lathe('Linen lampshade',(x,y,z+.34),[(0,.24),(.3,.13)],'PublicLinen')
    lathe('Shade rim',(x,y,z+.34),[(0,.245),(.016,.245)],'HallGold')
    sphere('Lamp finial',(x,y,z+.67),(.025,)*3,'HallGold')

def chair_back(x,y,w):
    # A single extruded silhouette avoids overlapping box/sphere seams at the crest.
    outline=[(-w/2,.555),(w/2,.555)]+[(w*.5*math.cos(i*math.pi/16),1.43+.15*math.sin(i*math.pi/16)) for i in range(17)]
    n=len(outline)
    verts=[(x+xx,y+yy,z) for yy in [-.08,.08] for xx,z in outline]
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh=bpy.data.meshes.new('Unified arched chair back');mesh.from_pydata(verts,[],faces);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bmesh.ops.bevel(bm,geom=list(bm.edges),offset=.024,segments=2,profile=.5,affect='EDGES')
    bm.to_mesh(mesh);bm.free();mesh.update()
    obj=bpy.data.objects.new('Unified arched chair back',mesh);bpy.context.collection.objects.link(obj);k.register(obj,'Unified arched chair back','HallWalnut')

def chair(x,y,angle=0,dining=False,cloth='HallSage'):
    start=len(k.groups[k.group]);w=.66 if dining else .94;d=.66 if dining else .9
    for xx in [-w*.34,w*.34]:
        for yy in [-d*.34,d*.34]:lathe('Chair turned leg',(x+xx,y+yy,.03),[(0,.035),(.07,.05),(.36,.03),(.47,.045)],'HallWalnut')
    box('Chair frame',(x,y,.5),(w,d,.12),'HallWalnutEdge',.035)
    box('Soft seat',(x,y-.02,.6),(w-.1,d-.12,.16),cloth,.055)
    chair_back(x,y+d*.38,w)
    box('Back upholstery',(x,y+d*.38-.095,1.04),(w-.14,.1,.67),cloth,.03)
    if not dining:
        for xx in [-w/2+.025,w/2-.025]:
            box('Chair soft arm',(x+xx,y,.84),(.17,d-.06,.18),cloth,.055)
            beam('Arm support',(x+xx,y-.27,.52),(x+xx,y-.27,.81),.04,'HallWalnut')
    rotate(start,(x,y,0),angle)
    obstacle('Dining chair' if dining else 'Armchair',x,y,w+.06,d+.07,1.59,angle)

def sofa(x,y,angle=0):
    start=len(k.groups[k.group]);w=2.75;d=.96
    for xx in [-1.08,1.08]:
        for yy in [-.29,.29]:lathe('Sofa bun foot',(x+xx,y+yy,.04),[(0,.075),(.07,.095),(.19,.07)],'HallWalnut')
    box('Sofa walnut base',(x,y,.32),(w,d,.2),'HallWalnut',.05)
    box('Soft rolled back',(x,y+.37,.93),(w,.31,1.03),'PublicRose',.09)
    for xx in [-1.25,1.25]:
        box('Sofa padded arm',(x+xx,y,.78),(.3,1.02,.63),'PublicRose',.09)
        beam('Sofa arm roll',(x+xx,y-.38,1.06),(x+xx,y+.39,1.06),.14,'PublicRose')
    for xx in [-.76,0,.76]:
        box('Sofa seat cushion',(x+xx,y-.08,.53),(.74,.75,.2),'PublicRose',.06)
        box('Sofa back cushion',(x+xx,y+.18,1.0),(.7,.16,.65),'PublicRose',.045)
    for xx in [-.89,.89]:
        obj=box('Sofa accent cushion',(x+xx,y-.04,.89),(.38,.18,.39),'PublicLinen',.05);obj.rotation_euler.y=xx*.17
    rotate(start,(x,y,0),angle);obstacle('Sofa',x,y,2.84,1.05,1.48,angle)

def oval(name,x,y,z,w,d,h,material):
    n=40;verts=[(x+w*.5*math.cos(i*math.tau/n),y+d*.5*math.sin(i*math.tau/n),zz) for zz in [z-h/2,z+h/2] for i in range(n)]
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bmesh.ops.bevel(bm,geom=list(bm.edges),offset=min(.025,h*.2),segments=2,profile=.5,affect='EDGES')
    bm.to_mesh(mesh);bm.free();mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);k.register(obj,name,material)

def table(x,y,w,d,h,rounded=False):
    if rounded:oval('Oval polished table',x,y,h,w,d,.11,'HallWalnutEdge')
    else:box('Rounded table top',(x,y,h),(w,d,.11),'HallWalnutEdge',.035)
    box('Table apron',(x,y,h-.14),(w-.2,d-.2,.16),'HallWalnut',.025)
    for xx in [-w*.35,w*.35]:
        for yy in [-d*.35,d*.35]:lathe('Table turned leg',(x+xx,y+yy,.05),[(0,.05),(.08,.07),(h-.28,.04),(h-.18,.065)],'HallWalnut')
    obstacle('Table',x,y,w+.03,d+.03,h+.06)

def sideboard(x,y,w=2.6,angle=0):
    start=len(k.groups[k.group]);d=.65;h=1.04
    box('Sideboard body',(x,y,.58),(w,d,.88),'HallWalnut',.03)
    box('Sideboard crown',(x,y,h),(w+.1,d+.1,.1),'HallWalnutEdge',.03)
    for i in range(3):
        xx=x+(i-1)*w/3
        box('Cabinet inset',(xx,y-.342,.56),(w/3-.09,.045,.63),'HallWalnutEdge',.018)
        sphere('Cabinet knob',(xx+.13,y-.382,.65),(.028,)*3,'HallGold')
    for xx in [-w*.4,w*.4]:
        for yy in [-.23,.23]:lathe('Sideboard foot',(x+xx,y+yy,.03),[(0,.055),(.08,.075),(.18,.055)],'HallWalnut')
    rotate(start,(x,y,0),angle);obstacle('Sideboard',x,y,w+.12,d+.14,1.12,angle)

def chandelier(x,y,r=.62):
    z=2.88
    # One closed torus: no cylinder caps or bright seams between ring segments.
    n,m=32,8;verts=[];faces=[]
    for i in range(n):
        a=i*math.tau/n
        for j in range(m):
            b=j*math.tau/m;rr=r+.027*math.cos(b)
            verts.append((x+rr*math.cos(a),y+rr*math.sin(a),z+.027*math.sin(b)))
    for i in range(n):
        for j in range(m):faces.append((i*m+j,((i+1)%n)*m+j,((i+1)%n)*m+(j+1)%m,i*m+(j+1)%m))
    mesh=bpy.data.meshes.new('Continuous chandelier ring');mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new('Continuous chandelier ring',mesh);bpy.context.collection.objects.link(obj);k.register(obj,'Continuous chandelier ring','HallGold',True)
    beam('Pendant stem',(x,y,3.23),(x,y,3.89),.032,'HallWalnut')
    for i in range(6):
        a=i*math.tau/6;xx=x+r*math.cos(a);yy=y+r*math.sin(a)
        beam('Pendant spoke',(x,y,3.25),(xx,yy,z),.016,'HallGold')
        lathe('Candle dish',(xx,yy,z),[(0,.06),(.04,.085),(.06,.075)],'HallGold')
        lathe('Chandelier candle',(xx,yy,z+.06),[(0,.032),(.23,.034)],'HallIvory')
        sphere('Candle flame',(xx,yy,z+.36),(.032,.032,.09),'HallFlame')

def fireplace(x,y,angle):
    start=len(k.groups[k.group])
    box('Stone hearth',(x,y-.18,.13),(2.8,1.1,.2),'HallStone',.04)
    box('Dark firebox',(x,y+.23,1.03),(1.72,.075,1.65),'PublicInk',.02)
    for xx in [-1.03,1.03]:
        box('Fireplace jamb',(x+xx,y,.98),(.35,.62,1.65),'HallStone',.04)
        box('Jamb base',(x+xx,y,.31),(.47,.74,.24),'PublicStone',.025)
        box('Jamb capital',(x+xx,y,1.77),(.47,.7,.15),'PublicStone',.025)
    # Rounded Tudor-inspired lintel, assembled into one exported fireplace mesh.
    for i in range(9):
        xx=(i-4)*.22;zz=1.72+.23*math.sqrt(max(0,1-(xx/1.02)**2))
        box('Carved lintel',(x+xx,y,zz),(.23,.64,.26),'HallStone',.035)
    box('Mantel shelf',(x,y-.03,2.17),(2.8,.84,.15),'PublicStone',.04)
    box('Chimney breast',(x,y+.16,3.115),(2.52,.4,1.76),'PublicPlaster',.02)
    for xx in [-.5,0,.5]:beam('Firewood',(x+xx-.2,y-.05,.34),(x+xx+.2,y+.04,.38),.085,'HallWalnut')
    for xx in [-.45,-.16,.16,.45]:
        height=.5 if abs(xx)<.3 else .36
        lathe('Tapered ember flame',(x+xx,y-.04,.34),[(0,.045),(.1,.11),(height*.6,.075),(height,.006)],'PublicFire')
    for xx in [-.7,-.35,0,.35,.7]:beam('Fireguard upright',(x+xx,y-.4,.26),(x+xx,y-.4,.69),.018,'PublicInk')
    beam('Fireguard rail',(x-.76,y-.4,.64),(x+.76,y-.4,.64),.025,'PublicInk')
    k.painting('Overmantel landscape',x,y-.08,3.0,1.5,.98,3)
    rotate(start,(x,y,0),angle)
    obstacle('Fireplace',x+.18*math.sin(angle),y-.18*math.cos(angle),2.86,1.16,3.995,angle)

def curtains(x,y,width=1.5,angle=0,cloth='PublicRose'):
    start=len(k.groups[k.group])
    if cloth:beam('Curtain pole',(x-width/2-.4,y-.18,3.12),(x+width/2+.4,y-.18,3.12),.025,'HallGold')
    for side in ([-1,1] if cloth else []):
        xx=x+side*(width/2+.2)
        sphere('Curtain finial',(x+side*(width/2+.43),y-.18,3.12),(.06,)*3,'HallWalnutEdge')
        verts=[];faces=[];cols=12
        for z,spread in [(.16,1.02),(1.1,.65),(1.2,.65),(3.02,1.0)]:
            for i in range(cols+1):
                u=i/cols;verts.append((xx+(u-.5)*.38*spread,y-.22-.04*math.cos(u*math.tau*4),z))
        for row in range(3):
            for i in range(cols):a=row*(cols+1)+i;faces.append((a,a+1,a+cols+2,a+cols+1))
        mesh=bpy.data.meshes.new('Gathered drape');mesh.from_pydata(verts,[],faces);mesh.update()
        obj=bpy.data.objects.new('Gathered drape',mesh);bpy.context.collection.objects.link(obj);k.register(obj,'Gathered drape',cloth,True)
        solid=obj.modifiers.new('Fabric thickness','SOLIDIFY');solid.thickness=.008;bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=solid.name)
        box('Curtain tie',(xx,y-.24,1.14),(.27,.065,.065),'HallGold',.012)
    for zz in [1.08,2.72]:beam('Leaded frame',(x-width/2+.06,y-.025,zz),(x+width/2-.06,y-.025,zz),.009,'HallGold')
    for xx in [-width/2+.06,width/2-.06]:beam('Leaded frame',(x+xx,y-.025,1.08),(x+xx,y-.025,2.72),.009,'HallGold')
    for offset in [-1.3,-.65,0,.65,1.3]:
        for slope in [-1,1]:
            hits=[]
            for xx in [-width/2+.06,width/2-.06]:
                z=1.9+offset+slope*xx
                if 1.08<=z<=2.72:hits.append((xx,z))
            for z in [1.08,2.72]:
                xx=(z-1.9-offset)/slope
                if -width/2+.06<=xx<=width/2-.06:hits.append((xx,z))
            if len(hits)==2:beam('Diamond lead',(x+hits[0][0],y-.025,hits[0][1]),(x+hits[1][0],y-.025,hits[1][1]),.009,'HallGold')
    rotate(start,(x,y,0),angle)

# Reception: all seating lies south of the two story approaches at Y6 and Y7.5.
k.group='ReceptionSeats';sofa(3,3.1,math.pi/2)
chair(6,1.65,math.pi,cloth='HallSage');chair(6,4.6,cloth='HallSage')
k.group='ReceptionTable';table(5.25,3.15,1.2,1.55,.61,True)
k.vase(5.25,3.3,.67,.5)
for i in range(2):box('Tea table book',(5.3,2.75,.7+i*.05),(.32,.25,.05),'HallBook',.008)
k.group='ReceptionFireplace';fireplace(9.27,2.8,-math.pi/2)
k.group='ReceptionDecor';sideboard(2.15,9.29,2.5);lamp(1.35,9.25,1.1);k.vase(2.8,9.24,1.1,.7)
k.painting('Reception landscape',2.15,9.72,2.36,2.0,1.34,2)
for yy in [1.0,4.5]:k.sconce(9.68,yy,2.6,-math.pi/2)
k.group='ReceptionWindows'
for x in [3,7]:curtains(x,0,angle=math.pi)
curtains(0,5,angle=math.pi/2)
k.group='PublicRugs';rug(5.15,3.1,5.6,4.4);rug(17,5,2.65,8.6);rug(29.5,4.5,5.8,7.5);rug(5,27.2,5.2,4.4,'PublicBookGreen')

# Dining table stays south of the northern through aisle between all three doors.
k.group='DiningTable'
oval('Dining polished oval',29.5,4.4,.94,2.15,5.45,.12,'HallWalnutEdge')
oval('Dining apron',29.5,4.4,.8,1.97,5.12,.18,'HallWalnut')
for yy in [3.0,5.8]:
    lathe('Dining pedestal',(29.5,yy,.06),[(0,.4),(.1,.43),(.19,.26),(.49,.19),(.66,.26),(.76,.34)],'HallWalnut')
    for side in [-1,1]:beam('Splayed pedestal foot',(29.5,yy,.21),(29.5+side*.76,yy,.09),.065,'HallWalnutEdge')
obstacle('Dining table',29.5,4.4,2.19,5.49,1.02)
box('Dining linen runner',(29.5,4.4,1.006),(.58,4.6,.012),'PublicLinen',.003)
k.vase(29.5,4.4,1.015,.62)
for xx in [-.7,.7]:
    for yy in [2.6,3.8,5,6.2]:
        lathe('Cream ceramic plate',(29.5+xx,yy,1.009),[(0,.15),(.015,.18),(.03,.19),(.04,.16),(.022,.12)],'PublicChina')
        lathe('Plate gilt rim',(29.5+xx,yy,1.042),[(0,.173),(.004,.173)],'HallGold')
        beam('Silverware',(29.5+xx,yy+.24,1.025),(29.5+xx+.19,yy+.24,1.025),.012,'HallGold')
k.group='DiningChairs'
for yy in [2.6,3.8,5,6.2]:chair(27.95,yy,math.pi/2,True,'PublicRose');chair(31.05,yy,-math.pi/2,True,'PublicRose')
chair(29.5,1.13,math.pi,True,'PublicRose');chair(29.5,7.67,0,True,'PublicRose')
k.group='DiningCabinets';sideboard(32,9.28,2.7)
k.vase(32,9.25,1.1,.72)
for xx in [31.1,32.9]:lathe('Sideboard ceramic bowl',(xx,9.2,1.1),[(0,.09),(.08,.16),(.13,.2),(.16,.18)],'PublicBlue')
k.group='DiningPictures';k.painting('Dining landscape',32,9.72,2.6,2.15,1.4,3)
for xx in [30.3,33.5]:k.sconce(xx,9.69,2.7)
k.painting('Dining west portrait',24.28,2.6,2.38,1.12,1.5,1,math.pi/2)
k.group='DiningWindows'
for xx in [27,31]:curtains(xx,0,angle=math.pi,cloth='PublicBookGreen')

def bookshelf(x,y,w=2.4,angle=0):
    start=len(k.groups[k.group]);h=3.25;d=.5
    box('Bookcase back',(x,y+.21,h/2),(w,.07,h),'HallWalnut',.01)
    for xx in [-w/2,w/2]:box('Bookcase upright',(x+xx,y,h/2),(.12,d,h),'HallWalnutEdge',.02)
    for z in [.14,.68,1.21,1.74,2.27,2.8,3.25]:box('Bookcase shelf',(x,y,z),(w+.12,d+.04,.08),'HallWalnut',.018)
    box('Bookcase cornice',(x,y,3.32),(w+.25,.62,.12),'HallWalnutEdge',.03)
    for row,z in enumerate([.19,.73,1.26,1.79,2.32,2.85]):
        # Leave occasional breathing space instead of a uniformly packed wall.
        books(x+(.14 if row%3==0 else -.08),y-.03,z,w-.47,.28,.37)
    rotate(start,(x,y,0),angle);obstacle('Public bookcase',x,y,w+.27,.64,3.4,angle)

k.group='LibraryShelves'
for xx in [2,8]:bookshelf(xx,22.52,2.4,math.pi)
for yy in [24.6,31.4]:bookshelf(9.48,yy,2.4,-math.pi/2)
bookshelf(.52,27,2.1,math.pi/2);bookshelf(6.55,33.46,2.0)
k.group='LibraryTable';table(5.2,27.2,2.5,1.3,.87)
for xx in [4.5,5.9]:lamp(xx,27.2,.93)
for xx in [4.6,5.9]:
    for i in range(2):box('Reading book',(xx,26.97,.96+i*.045),(.36,.25,.045),'PublicBookRed' if i else 'HallBook',.006)
k.group='LibrarySeats'
for xx in [4.45,5.95]:chair(xx,26.05,math.pi,True,'HallSage');chair(xx,28.35,0,True,'HallSage')
chair(1.85,31.1,math.pi,False,'PublicRose');chair(5.9,31.2,-math.pi/2,False,'PublicRose')
# Window seat below the northern glazing; the seat remains between the drapes.
box('Library window seat',(4,33.35,.34),(1.25,.65,.55),'HallWalnut',.045)
box('Window seat cushion',(4,33.34,.67),(1.22,.64,.18),'HallSage',.06)
obstacle('Library window seat',4,33.35,1.3,.72,.79)
k.group='LibraryNook';table(3.4,30.9,1.0,1.0,.65,True);k.vase(3.4,30.9,.71,.47)
k.group='LibraryWindows'
for yy in [24,30]:curtains(0,yy,angle=math.pi/2,cloth='PublicBookGreen')
curtains(4,34,cloth='PublicBookGreen')
# The corner window ends beside the east wall: keep its glazing border, omit wide drapes.
curtains(9,34,cloth=None)
k.group='LibraryPictures'
k.painting('Library pastoral painting',.29,32.5,2.4,1.15,1.1,3,math.pi/2)
for yy in [26.5,29.5]:k.sconce(9.7,yy,2.65,-math.pi/2)

# Foyer is a broad arrival space: central axis and cross-room approaches stay clear.
k.group='FoyerFurniture'
for x,angle in [(10.72,math.pi/2),(23.28,-math.pi/2)]:
    start=len(k.groups[k.group])
    box('Arrival bench',(x,3,.48),(2.0,.61,.14),'HallWalnutEdge',.035)
    box('Bench cushion',(x,3,.61),(1.83,.56,.17),'HallSage',.05)
    for xx in [-.8,.8]:
        for yy in [-.2,.2]:lathe('Bench foot',(x+xx,3+yy,.03),[(0,.045),(.08,.06),(.39,.04)],'HallWalnut')
    rotate(start,(x,3,0),angle);obstacle('Arrival bench',x,3,2.04,.67,.73,angle)
sideboard(21.25,9.05,2.5);k.vase(21.25,9.04,1.1,.9)
k.group='FoyerPictures';k.painting('Arrival manor painting',21.25,9.6,2.5,2.1,1.4,2)
k.painting('Arrival portrait',11.65,9.64,2.42,1.12,1.48,0)
for xx in [14.2,20.0,22.7]:k.sconce(xx,9.6,2.7)
k.group='FoyerWindows'
for xx in [12,21]:curtains(xx,0,angle=math.pi,cloth='HallSage')
k.group='PublicChandeliers'
for x,y,r in [(5.2,3.2,.56),(29.5,4.4,.7),(5.2,27.2,.62),(17,4.8,.75)]:chandelier(x,y,r)

print('PUBLIC_FURNITURE_COMPLETE', flush=True)
checks=['ReceptionSeats','ReceptionTable','ReceptionFireplace','ReceptionDecor','DiningTable','DiningChairs','DiningCabinets','DiningPictures','LibraryShelves','LibraryTable','LibrarySeats','LibraryNook','LibraryPictures','FoyerFurniture','FoyerPictures','PublicChandeliers']
k.validate_architecture(project,checks,output/'PlacementValidation.json')
pairs=[('ReceptionSeats','ReceptionTable'),('ReceptionSeats','ReceptionWindows'),('ReceptionFireplace','ReceptionWindows'),('ReceptionDecor','ReceptionWindows'),('DiningTable','DiningChairs'),('DiningChairs','DiningWindows'),('DiningCabinets','DiningPictures'),('LibraryShelves','LibraryWindows'),('LibraryShelves','LibraryPictures'),('LibrarySeats','LibraryTable'),('LibrarySeats','LibraryWindows'),('LibrarySeats','LibraryNook'),('FoyerFurniture','FoyerWindows'),('FoyerFurniture','FoyerPictures')]
k.validate_group_pairs(pairs,output/'DecorationClearance.json')
k.export(project,source,output,'PublicFinish','PublicFinish.json','public-finish-01')
os._exit(0)
