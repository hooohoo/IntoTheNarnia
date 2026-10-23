"""Approved room graph -> connected architectural review, metres / Blender Z up."""
from pathlib import Path
import copy
import hashlib
import json
import math
from collections import defaultdict
from geometry import box, beam, sphere, lathe, wall_box
import hall_geometry as H

OUT = Path(__file__).resolve().parent.parent
D = json.loads((OUT / 'Data/layout.json').read_text())
PALETTE = json.loads((OUT / 'Data/palette.json').read_text())['palette']
PALETTE.update(dict(publicFloor='C8B99D', professorFloor='B8A9C6', childrenFloor='D9BC86',
    corridorFloor='A5BEB4', serviceFloor='B9B7A9', atticFloor='BF9F7F', lawn='8EAA78',
    routeArrival='328CBA', routeHide='34835C', routeEscape='CC7034', routeAdult='AE534B',
    ink='443C34', cream='F1E7D2', armor='737C80', softGreen='718367', brown='544334',
    childSkin='DBC3A0', reference='9C7980', chalk='F3E6CD'))
(OUT / 'Data/palette.json').write_text(json.dumps(dict(palette=PALETTE), indent=2))
parts = []
walk_surfaces = []
floor_index = {'ground': 0, 'upper': 1, 'attic': 2}
floor_heights = [0, 4.32, 9]
wall_heights = [4.08, 4.52, 3.2]

def part(pid, title, role, level, zone=None, **kwargs):
    # Blender parses a trailing dot followed by digits as its numeric name suffix.
    # Decimal coordinates must not produce a suffix larger than a 32-bit integer.
    pid = pid.replace('.', 'p')
    p = dict(id=pid, title=title, role=role, level=level, zone=zone, shapes=[],
             explode=[0, 0, 0], pivot=None, defaultHidden=False)
    p.update(kwargs); parts.append(p); return p

def floor_color(room):
    return {'professor':'professorFloor', 'children':'childrenFloor', 'hub':'corridorFloor',
            'nook':'corridorFloor', 'stair':'corridorFloor', 'service':'serviceFloor',
            'storage':'serviceFloor', 'story':'atticFloor'}.get(room['kind'], 'publicFloor')

def surface(pid, rect, z):
    walk_surfaces.append(dict(id=pid, rect=list(rect), z=z))

# The reviewed hall already has the garden opening, short landing and stair cut-outs.
for original in H.hall['parts']:
    if original['id'] == 'B13':
        continue
    p = copy.deepcopy(original)
    p['zone'] = 'hall'
    p['defaultHidden'] = False
    if p['id'] == 'B09':
        p['level'] = 1
        p['shapes'][-1]['size'][1] = 6  # y16..22 is the approved attic corridor floor.
    if p['role'] in ('wall', 'window', 'portal-frame'):
        p['sliceArchitectural'] = True
    glass_shapes = [s for s in p['shapes'] if s.get('mat') == 'glass']
    if glass_shapes:
        gp = copy.deepcopy(p)
        gp.update(id=p['id']+'_Glass',role='glass',transparent=True,shapes=glass_shapes)
        parts.append(gp)
        p['shapes'] = [s for s in p['shapes'] if s.get('mat') != 'glass']
    parts.append(p)
    for shape in p['shapes']:
        if p['role'] in ('floor', 'gallery', 'stairs') and shape['type'] == 'box':
            x,y,z = shape['pos']; w,d,h = shape['size']; surface(p['id'], [x,y,w,d], z+h)
        if shape['type'] == 'stair':
            walk_surfaces.append(dict(id=p['id'], stair=shape))

hall_floor_rooms = {'ground/B', 'upper/B-W', 'upper/B-E', 'upper/B-NW', 'upper/B-NE'}
for floor, fd in D['floors'].items():
    level = floor_index[floor]; z = fd['height']
    for room in fd['rooms']:
        zone = floor + '/' + room['id']
        if zone in hall_floor_rooms or room['id'] == 'S2':
            continue
        p = part('Floor_' + floor + '_' + room['id'], room['name'], 'floor', level, zone)
        x,y,w,d = room['rect']; thickness = .16 if level == 2 else .24
        box(p,x,y,z-thickness,w,d,thickness,floor_color(room),0)
        surface(p['id'],room['rect'],z)

# S2 keeps the approved 6 x 8 m room. A western landing strip reaches the upper ER door.
for level,z in enumerate(floor_heights):
    p = part('S2_Landing_' + str(level), '보조 계단 층 도착부', 'floor', level, list(floor_index)[level]+'/S2')
    rectangles = [[24,26,6,8]] if level == 0 else [[24,26,6,2.2],[24,28.2,1.25,5.8]]
    for rect in rectangles:
        x,y,w,d = rect; box(p,x,y,z-.24,w,d,.24,'corridorFloor',0); surface(p['id'],rect,z)

secondary_paths = []
for level,n in [(0,12),(1,13)]:
    z = floor_heights[level]; rise=.18; run=3.64; tread=run/n; mid=z+n*rise
    flights = [dict(type='stair',origin=[25.35,28.2,z],direction=[0,1],n=n,width=1.65,tread=tread,rise=rise,mat='wood'),
               dict(type='stair',origin=[29.45,31.84,mid],direction=[0,-1],n=n,width=1.65,tread=tread,rise=rise,mat='wood')]
    p = part('S2_Flights_'+str(level), f'보조 계단 {n}+{n}단', 'stairs', level, 'S2', spans=[level,level+1])
    p['shapes'] += flights
    for s in flights: walk_surfaces.append(dict(id=p['id'],stair=s))
    rect=[25.35,31.84,4.10,1.96]
    box(p,*rect[:2],mid-.24,*rect[2:],.24,'wood',0); surface(p['id'],rect,mid)
    rail = part('S2_Rails_'+str(level),'보조 계단 난간','rails',level,'S2',spans=[level,level+1])
    def guard(a,b):
        beam(rail,[a[0],a[1],a[2]+1.0],[b[0],b[1],b[2]+1.0],.055,'wood')
        beam(rail,[a[0],a[1],a[2]+.12],[b[0],b[1],b[2]+.12],.04,'wood')
        count=max(1,math.ceil(math.dist(a[:2],b[:2])/.22))
        for i in range(count+1):
            t=i/count; q=[a[j]*(1-t)+b[j]*t for j in range(3)]
            beam(rail,[q[0],q[1],q[2]+.12],[q[0],q[1],q[2]+1.0],.027,'wood',True)
    for x in [25.35,27.0]: guard([x,28.2,z],[x,31.84,mid])
    for x in [27.8,29.45]: guard([x,31.84,mid],[x,28.2,z+2*n*rise])
    guard([27.0,31.84,mid],[27.8,31.84,mid])
    # Floor landing edges face the stair void; entrances to each flight remain open.
    edge = part('S2_UpperGuard_'+str(level+1),'보조 계단 층 바닥 가장자리','rails',level+1,'S2')
    previous = rail; rail = edge
    top=z+2*n*rise
    guard([25.25,28.2,top],[25.25,33.85,top])
    guard([27.0,28.2,top],[27.8,28.2,top])
    guard([29.45,28.2,top],[29.85,28.2,top])
    rail=previous
    points=[[27,27,z],[26.175,27,z],[26.175,28.1,z]]
    for i in range(n): points.append([26.175,28.2+(i+.5)*tread,z+(i+1)*rise])
    points += [[26.175,32.65,mid],[28.625,32.65,mid]]
    for i in range(n): points.append([28.625,31.84-(i+.5)*tread,mid+(i+1)*rise])
    points += [[28.625,27.2,top],[27,27,top]]
    secondary_paths.append(points)

def hall_boundary(axis,line,a,b,level):
    if level>1: return False
    if axis=='h' and line in (10,22) and 10<=a and b<=24: return True
    if axis=='v' and line in (10,24) and 10<=a and b<=22: return True
    if level==1 and axis=='v' and line in (12,22) and 10<=a and b<=22: return True
    return False

doors_report=[]; walls_report=[]
for floor,fd in D['floors'].items():
    level=floor_index[floor]; base=fd['height']; top=base+wall_heights[level]
    lines=defaultdict(list)
    for room in fd['rooms']:
        x,y,w,h=room['rect']
        for axis,line,a,b in [('h',y,x,x+w),('h',y+h,x,x+w),('v',x,y,y+h),('v',x+w,y,y+h)]:
            lines[(axis,round(line,5))].append((a,b,room['id']))
    for (axis,line),edges in sorted(lines.items()):
        features=[]
        for kind,items in [('door',fd['doors']),('window',fd['windows'])]:
            for i,obj in enumerate(items):
                if obj['axis']!=axis or abs(obj['point'][1 if axis=='h' else 0]-line)>1e-5: continue
                center=obj['point'][0 if axis=='h' else 1]; width=obj['width']
                low=base if kind=='door' else base+1.0
                high=base+(2.7 if kind=='door' else 2.8)
                features.append(dict(start=center-width/2,end=center+width/2,low=low,high=high,kind=kind,source=obj,index=i))
        cuts=sorted(set([v for a,b,_ in edges for v in (a,b)]+[v for f in features for v in (f['start'],f['end'])]+([10,12,22,24] if axis=='h' else [10,22])))
        for a,b in zip(cuts,cuts[1:]):
            mid=(a+b)/2; owners=sorted(set(r for x,y,r in edges if x<mid<y))
            if not owners or hall_boundary(axis,line,a,b,level): continue
            openings=[f for f in features if f['start']<mid<f['end']]
            cuts_z=sorted(set([base,base+.95,top]+[v for f in openings for v in (f['low'],f['high'])]))
            for lo,hi in zip(cuts_z,cuts_z[1:]):
                if hi-lo<1e-5 or hi>top or any(f['low']<(lo+hi)/2<f['high'] for f in openings): continue
                p=part(f'Wall_{floor}_{axis}{line}_{a}_{lo}', ' / '.join(owners)+' 벽', 'wall',level,floor+'/'+owners[0],tier='low' if hi<=base+.951 else 'high')
                wall_box(p,axis,line,a,lo,b-a,hi-lo,.24,'plaster',.008)
                walls_report.append(dict(part=p['id'],axis=axis,line=line,span=[a,b],height=[lo,hi],owners=owners))
        for f in features:
            obj=f['source']; a,b=f['start'],f['end']
            if not any(x<(a+b)/2<y for x,y,_ in edges): continue
            # Existing hall source owns its window and most frames. New professor entrance needs a frame.
            is_hall=hall_boundary(axis,line,a,b,level)
            if is_hall and not (f['kind']=='door' and obj.get('restriction')=='professor'):
                if f['kind']=='door': doors_report.append(dict(floor=floor,**obj,frameOwner='hall source'))
                continue
            p=part(f"Frame_{floor}_{f['kind']}_{f['index']}", '문틀' if f['kind']=='door' else '창틀','frame',level,floor,tier='high')
            for u in (a-.08,b): wall_box(p,axis,line,u,f['low'],.08,f['high']-f['low'],.34,'wood',.012)
            wall_box(p,axis,line,a-.1,f['high'],b-a+.2,.12,.36,'wood',.012)
            if f['kind']=='window':
                wall_box(p,axis,line,a-.1,f['low']-.1,b-a+.2,.1,.4,'stone',.014)
                wall_box(p,axis,line,(a+b)/2-.025,f['low'],.05,f['high']-f['low'],.15,'wood',.005)
                glass=part(f"Glass_{floor}_{f['index']}",'창 유리','glass',level,floor,tier='high',transparent=True)
                wall_box(glass,axis,line,a,f['low'],b-a,f['high']-f['low'],.02,'glass',0)
            else:
                doors_report.append(dict(floor=floor,**obj,frameOwner=p['id']))
                if obj.get('restriction')=='professor':
                    leaf=part('Professor_Gateway','교수 전용 경계문','door',level,'upper/P0')
                    wall_box(leaf,axis,line,a,base,b-a,2.68,.09,'wood',.02)

# Window seats and cupboard ends make the end of each corridor intentional.
p=part('WestNookSeat','후정 창가 자리','prop',1,'upper/B-NW')
box(p,11.8,22.6,4.32,.42,1.6,.48,'wood',.03);box(p,11.78,22.62,4.8,.46,1.56,.1,'linen',.03)
p=part('ChildrenEndCupboard','침실 복도 서쪽 끝 린넨장','prop',1,'upper/K0')
box(p,30.16,26.15,4.32,.45,1.65,2.25,'wood',.04)
for floor,room_id in [('upper','E1'),('upper','K-BOYS')]:
    room=next(r for r in D['floors'][floor]['rooms'] if r['id']==room_id); x,y,w,h=room['rect']
    p=part('Beds_'+room_id,room['name']+' 위치 확인용 가구','prop',1,floor+'/'+room_id)
    for bx in [x+1.1,x+w-2.7]:
        box(p,bx,31.0,4.32,1.55,2.3,.4,'wood',.05);box(p,bx+.04,31.04,4.72,1.47,2.22,.14,'linen',.07)
        box(p,bx,33.2,4.32,1.55,.15,1.05,'wood',.04)
p=part('ProfessorDesk','서재의 책상 위치','prop',1,'upper/P-STUDY')
box(p,1.8,15,4.32,2.2,1.2,.8,'wood',.035)
for y in [12,20]:box(p,.3,y,4.32,.45,1.5,2.5,'wood',.025)

# Reuse only the wardrobe object; the room itself comes from the approved plan.
wardrobe=json.loads((OUT/'Data/wardrobe-source.json').read_text())
for original in wardrobe['parts']:
    if original['id'] not in ('G07','G08L','G08R','G09'): continue
    p=copy.deepcopy(original);p.update(level=2,zone='attic/G2',translation=[0,21.3,9],defaultHidden=False,role='prop')
    parts.append(p)

p=part('ArtifactArmor','전시 갑옷 · 부분 파손 대상','prop',0,'ground/D1')
box(p,1.88,17.58,0,.84,.84,.12,'stone',.04)
for x in [2.13,2.47]:beam(p,[x,18,.12],[x,18,.75],.10,'armor',True)
sphere(p,[2.3,18,1.12],.34,'armor',[1,.72,1.25]);sphere(p,[2.3,18,1.64],.20,'armor')
for x in [1.85,2.75]:beam(p,[x,18,1.2],[x,18,.72],.08,'armor',True)
p=part('ArtifactCases','유물 진열대 위치','prop',0,'ground/D1')
for y in [11,20.8]:box(p,7.2,y,0,1.4,.6,.9,'wood',.025)

for name,rect in [('후정',D['gardens'][0]['rect']),('동측 중정',D['gardens'][1]['rect']),('서쪽 정원',D['storyConstraints']['lawn']['rect'])]:
    p=part('Garden_'+name,name,'outdoor',-1,'outdoor');x,y,w,d=rect;box(p,x,y,-.17,w,d,.12,'lawn',0)
    surface(p['id'],rect,-.05)
    # Shrubs stop clear of the ball line and the entrance path.
    for xx,yy in [(x+.5,y+.5),(x+w-.5,y+d-.5)]: sphere(p,[xx,yy,.3],.4,'softGreen',[1,1,.8])

# A small exterior threshold completes the declared front OUT -> A doorway.
# It does not add a room or alter the approved interior envelope.
p=part('FrontEntryApron','정문 바깥 문턱 바닥','outdoor',-1,'outdoor')
box(p,15.2,-2.0,-.2,3.6,2.0,.2,'stoneAlt',.02)
surface(p['id'],[15.2,-2.0,3.6,2.0],0)

labels=[]
for floor,fd in D['floors'].items():
    level=floor_index[floor]
    for r in fd['rooms']:
        x,y,w,h=r['rect']; px,py=r.get('labelPos',[x+w/2,y+h*.36])
        if r['id']=='S2': px,py=27,26.55
        if r['id']=='B': px,py=17,11.3
        labels.append(dict(id=floor+'/'+r['id'],text=r['id'],name=r['name'],position=[px,py,fd['height']+.04],size=min(.75,w*.22,h*.21),level=level))

def rise_path(shape, v=None):
    x,y,z=shape['origin'];dx,dy=shape['direction'];v=shape['width']/2 if v is None else v
    return [[x+dx*(i+.5)*shape['tread']+dy*v,y+dy*(i+.5)*shape['tread']-dx*v,z+(i+1)*shape['rise']] for i in range(shape['n'])]
main=next(p['shapes'][0] for p in H.parts if p['id']=='B03')
left=next(p['shapes'][0] for p in H.parts if p['id']=='B05')
right1=next(p['shapes'][0] for p in H.parts if p['id']=='B06a')
right2=next(p['shapes'][0] for p in H.parts if p['id']=='B06c')
arrival_upper=next(r for r in D['floors']['upper']['routes'] if r['scene']=='arrival' and r['kind']=='walk' and r['phase']=='main')
arrival=[[17,11,0],[17,16.15,0]]+rise_path(main)+[[17,20.6,1.8]]+rise_path(left,1.25)+[[11,20.6,4.32]]
arrival += [[x,y,4.32] for x,y in arrival_upper['points'][1:]]
hide_upper=next(r for r in D['floors']['upper']['routes'] if r['scene']=='hide' and r['phase']=='main')
attic_path=next(r for r in D['floors']['attic']['routes'] if r['scene']=='hide' and r['phase']=='main')
hide=[[x,y,4.32] for x,y in hide_upper['points']]+secondary_paths[1][1:]+[[x,y,9] for x,y in attic_path['points'][1:]]
ground_routes=D['floors']['ground']['routes']
escape=[[x,y,0] for x,y in ground_routes[0]['points']]+[[x,y,0] for x,y in ground_routes[1]['points'][1:]]
escape+=secondary_paths[0][1:]+secondary_paths[1][1:]+[[x,y,9] for x,y in attic_path['points'][1:]]
east_check=[[17,16.15,0]]+rise_path(main)+[[17,20.15,1.8]]+rise_path(right1,.8)+[[21.64,20.15,3.24]]+rise_path(right2,.8)+[[21.64,23.4,4.32],[23.35,23.4,4.32],[23.35,19,4.32]]
routes = dict(arrival=dict(title='첫 안내',color='routeArrival',points=arrival),hide=dict(title='숨바꼭질',color='routeHide',points=hide),escape=dict(title='창문·유물 사고 후 피신',color='routeEscape',points=escape))
for key,r in routes.items():
    r['length']=round(sum(math.dist(a,b) for a,b in zip(r['points'],r['points'][1:])),2)
    r['illustrativeSecondsAt1_6mps']=round(r['length']/1.6)
    for i,(a,b) in enumerate(zip(r['points'],r['points'][1:])):
        if math.dist(a,b)<.01: continue
        level=0 if min(a[2],b[2])<4.31 else 1 if min(a[2],b[2])<8.99 else 2
        p=part('Route_'+key+'_'+str(i),r['title'],'route',level,route=key)
        beam(p,[a[0],a[1],a[2]+.08],[b[0],b[1],b[2]+.08],.045,r['color'],True)
    r['routeKey']=key

# The ball's path is a separate event, never used as a character walk route.
p=part('BallLine','공의 궤적','route',0,'ground/D1',route='escape')
beam(p,[-7,18,1.5],[2.3,18,1.5],.025,'routeAdult',True)

proxies=[dict(id='LucyProxy_Hall',position=[14,15.3,0],level=0),dict(id='LucyProxy_Children',position=[43.7,27.1,4.32],level=1),dict(id='LucyProxy_Wardrobe',position=[3.0,29.7,9],level=2)]
# Fixed wall pieces are authored as a room side, not exported as individual bricks.
grouped={}; combined=[]
for p in parts:
    if p['role'] not in ('wall','route') or p.get('sliceArchitectural'):
        combined.append(p);continue
    key=(p['role'],p['level'],p.get('zone'),p.get('tier'),p.get('route'))
    if key not in grouped:
        target=copy.deepcopy(p);target['shapes']=[];target['sourceSegments']=[]
        grouped[key]=target;combined.append(target)
    grouped[key]['shapes']+=p['shapes'];grouped[key]['sourceSegments'].append(p['id'])
parts=combined
spec=dict(version='connected-layout-01',status='Download model review; approved floor graph, derived stair details; not installed in Unity',units='metres, Blender Z up',palette=PALETTE,parts=[p for p in parts if p['shapes']],labels=labels,proxies=proxies,routes=routes,secondaryStairPaths=secondary_paths,extraValidationRoutes={'eastStair':east_check},walkSurfaces=walk_surfaces,doors=doors_report,walls=walls_report,levels=floor_heights,reviewCharacter=dict(height=1.2,radius=.22,eyeHeight=1.05,status='comparison proxy only; final Lucy proportions are not fixed'),approvedLayoutSha256=hashlib.sha256((OUT/'Data/layout.json').read_bytes()).hexdigest())
(OUT/'Data/model-spec.json').write_text(json.dumps(spec,ensure_ascii=False,indent=2))
print(json.dumps(dict(parts=len(spec['parts']),rooms=len(labels),doors=len(doors_report),routes={k:r['length'] for k,r in routes.items()}),ensure_ascii=False))
