from pathlib import Path
from collections import defaultdict,deque
import json
import math
import xml.etree.ElementTree as ET
import ast

OUT=Path(__file__).resolve().parent.parent
D=json.loads((OUT/'Data/layout.json').read_text())
def overlap(a,b):
    x,y,w,h=a;xx,yy,ww,hh=b
    return min(x+w,xx+ww)-max(x,xx)>1e-6 and min(y+h,yy+hh)-max(y,yy)>1e-6
def contains(rect,p):
    x,y,w,h=rect;px,py=p
    return x-1e-6<=px<=x+w+1e-6 and y-1e-6<=py<=y+h+1e-6
def edge(rect,axis,point,width):
    x,y,w,h=rect;px,py=point;r=width/2
    if axis=='h':return (abs(py-y)<1e-6 or abs(py-y-h)<1e-6) and x-1e-6<=px-r and px+r<=x+w+1e-6
    return (abs(px-x)<1e-6 or abs(px-x-w)<1e-6) and y-1e-6<=py-r and py+r<=y+h+1e-6
graph=defaultdict(set);room_count=0;door_count=0;route_samples=0
gardens={g['id']:g['rect'] for g in D['gardens']}
for level,f in D['floors'].items():
    rooms={r['id']:r for r in f['rooms']};room_count+=len(rooms)
    assert len(rooms)==len(f['rooms'])
    for i,r in enumerate(f['rooms']):
        assert r['rect'][0]>=0 and r['rect'][1]>=0 and r['rect'][0]+r['rect'][2]<=50.00001 and r['rect'][1]+r['rect'][3]<=34.00001
        for q in f['rooms'][i+1:]:assert not overlap(r['rect'],q['rect']),(level,r['id'],q['id'])
        for g in gardens.values():assert not overlap(r['rect'],g),(level,r['id'],'outdoor overlap')
    for door in f['doors']:
        for end in ['a','b']:
            id=door[end]
            if id=='OUT':continue
            rect=rooms[id]['rect'] if id in rooms else gardens[id]
            assert edge(rect,door['axis'],door['point'],door['width']),(level,id,door)
        a=f'{level}/{door["a"]}';b=f'{level}/{door["b"]}'
        graph[a].add(b);graph[b].add(a);door_count+=1
    for route in f['route']:
        for a,b in zip(route['points'],route['points'][1:]):
            steps=math.ceil(math.dist(a,b)/.12)
            for i in range(steps+1):
                t=i/steps;p=[aa*(1-t)+bb*t for aa,bb in zip(a,b)]
                assert any(contains(r['rect'],p) for r in rooms.values()),(level,route['name'],p)
                assert not any(contains(g,p) for g in gardens.values()),(level,route['name'],'crosses outdoors')
                route_samples+=1
for c in D['connections']:
    a='/'.join(c['source']);b='/'.join(c['target']);graph[a].add(b);graph[b].add(a)
    delta=D['floors'][c['target'][0]]['height']-D['floors'][c['source'][0]]['height']
    assert abs(sum(c['rises'])*c['riser']-delta)<1e-6,c['id']
rects=[next(r['rect'] for r in f['rooms'] if r['id']=='S2') for f in D['floors'].values()]
assert all(r==[24,26,6,8] for r in rects)
seen={'ground/OUT'};queue=deque(seen)
while queue:
    for nxt in graph[queue.popleft()]-seen:seen.add(nxt);queue.append(nxt)
missing=[f'{f}/{r["id"]}' for f,fd in D['floors'].items() for r in fd['rooms'] if f'{f}/{r["id"]}' not in seen]
assert not missing,missing
for floor in ['ground','upper','attic','section','story','hide_upper']:ET.parse(OUT/'Plans'/f'{floor}.svg')
for p in (OUT/'Source').glob('*.py'):ast.parse(p.read_text())
assert not any('침실' in r['name'] or '손님방' in r['name'] for r in D['floors']['ground']['rooms'])
def strict_contains(rect,p):
    x,y,w,h=rect;px,py=p
    return x+1e-7<px<x+w-1e-7 and y+1e-7<py<y+h-1e-7

def through_door(door,p):
    x,y=door['point'];px,py=p;half=door['width']/2
    return abs(py-y)<1e-5 and abs(px-x)<=half+1e-5 if door['axis']=='h' else abs(px-x)<1e-5 and abs(py-y)<=half+1e-5

crossings=0;routes_checked=0
for level,f in D['floors'].items():
    rooms={r['id']:r for r in f['rooms']}
    for route in f['routes']:
        if route['kind'] in ['ball','point']:continue
        zones={k:r['rect'] for k,r in rooms.items()}
        if route.get('outside'):zones['OUT']=D['storyConstraints']['lawn']['rect']
        for a,b in zip(route['points'],route['points'][1:]):
            # Divide each segment at every wall line. Every room change must use an actual door.
            times={0.,1.}
            for x,y,w,h in zones.values():
                for axis,edges in [(0,[x,x+w]),(1,[y,y+h])]:
                    delta=b[axis]-a[axis]
                    if abs(delta)>1e-7:
                        for edge_at in edges:
                            t=(edge_at-a[axis])/delta
                            if 0<t<1:times.add(round(t,10))
            times=sorted(times);previous=None
            for t0,t1 in zip(times,times[1:]):
                mid=(t0+t1)/2;p=[aa*(1-mid)+bb*mid for aa,bb in zip(a,b)]
                matches=[k for k,r in zones.items() if strict_contains(r,p)]
                assert len(matches)==1,(level,route['name'],'segment is not inside a walkable room',p,matches)
                current=matches[0]
                if previous is not None and previous!=current:
                    crossing=[aa*(1-t0)+bb*t0 for aa,bb in zip(a,b)]
                    valid=[d for d in f['doors'] if {d['a'],d['b']}=={previous,current} and through_door(d,crossing)]
                    assert valid,(level,route['name'],'crosses wall away from door',previous,current,crossing)
                    crossings+=1
                previous=current
            steps=max(1,math.ceil(math.dist(a,b)/.1))
            for i in range(steps+1):
                p=[aa*(1-i/steps)+bb*i/steps for aa,bb in zip(a,b)]
                assert any(contains(r,p) for r in zones.values()),(level,route['name'],p)
                assert not any(strict_contains(r,p) for r in gardens.values()),(level,'crosses garden')
                if route['kind']=='walk':
                    assert not any(strict_contains(r['rect'],p) for r in f['rooms'] if r['kind']=='professor'),(level,'enters professor wing')
                route_samples+=1
        routes_checked+=1

def reachable(g,start,excluded=frozenset()):
    visited={start};todo=deque([start])
    while todo:
        for nxt in g[todo.popleft()]-visited-excluded:visited.add(nxt);todo.append(nxt)
    return visited

private={f'{f}/{r["id"]}' for f,fd in D['floors'].items() for r in fd['rooms'] if r['kind']=='professor'}
public_seen=reachable(graph,'ground/OUT',private)
assert 'attic/G2' in public_seen and 'upper/E1' in public_seen and 'upper/K-BOYS' in public_seen
assert not private.intersection(public_seen)
upper=D['floors']['upper']
assert len([d for d in upper['doors'] if (f'upper/{d["a"]}' in private)!=(f'upper/{d["b"]}' in private)])==1
assert not any({d['a'],d['b']}=={'S2','K0'} for d in upper['doors'])
assert next(r for r in upper['rooms'] if r['id']=='P-STUDY')['name']=='교수의 서재'
assert not any(r['name']=='교수의 서재' for r in D['floors']['ground']['rooms'])
blocked={k:set(v) for k,v in graph.items()}
blocked['upper/F2'].remove('upper/K0');blocked['upper/K0'].remove('upper/F2')
isolated=reachable(blocked,'ground/OUT')
assert 'upper/E1' not in isolated and 'upper/K-BOYS' not in isolated
assert any(w['point']==[0,18] and w['axis']=='v' for w in D['floors']['ground']['windows'])
assert any({d['a'],d['b']}=={'OUT','C1'} and d['point']==[0,7.5] for d in D['floors']['ground']['doors'])
incident=D['storyConstraints']['brokenWindow']
display_room=next(r for r in D['floors']['ground']['rooms'] if r['id']==incident['room'])
assert edge(display_room['rect'],incident['axis'],incident['point'],2.3)
ball=next(r for r in D['floors']['ground']['routes'] if r['kind']=='ball')
assert contains(D['storyConstraints']['lawn']['rect'],ball['points'][0])
assert strict_contains(display_room['rect'],ball['points'][-1])
assert ball['points'][0][0]<incident['point'][0]<ball['points'][-1][0]
assert ball['points'][0][1]==incident['point'][1]==ball['points'][-1][1]
assert D['storyConstraints']['damagedArtifact']['position']==ball['points'][-1]
assert all('/'.join(c['source']) in graph and '/'.join(c['target']) in graph for c in D['connections'])
report=dict(rooms=room_count,doors=door_count,accessibleRoomGraph=True,routeSamples=route_samples,
    storyRoutesChecked=routes_checked,doorCrossingsChecked=crossings,
    professorHasSinglePublicGateway=True,publicRoutesAvoidProfessorWing=True,
    childrenAccessibleOnlyViaEastGallery=True,noUpperStairBedroomShortcut=True,
    ballAndPeopleUseDifferentOpenings=True,
    roomOverlaps=0,roomsAcrossOutdoorAreas=0,secondaryStairAligned=True,riserHeightsAgree=True,
    levels=D['levels'],scope='Diagram geometry, doorway adjacency and room-graph checks. No Unity traversal, collision, structural or historical verification.')
(OUT/'Data/validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
print(json.dumps(report,ensure_ascii=False))
