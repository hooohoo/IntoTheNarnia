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

import math
from mathutils.bvhtree import BVHTree
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


# Validate every door at its declared opening, excluding the intentionally closed professor boundary.
door_checks=[];door_failures=[]
for d in SPEC['doors']:
    if d.get('restriction')=='professor':
        door_checks.append(dict(floor=d['floor'],point=d['point'],status='intentionally closed boundary'))
        continue
    level={'ground':0,'upper':1,'attic':2}[d['floor']];base=SPEC['levels'][level]
    status=[]
    for distance in [-.35,0,.35]:
        x,y=d['point']
        if d['axis']=='h':y+=distance
        else:x+=distance
        point=Vector((x,y,base))
        hit,normal,index,distance=floors.ray_cast(point+Vector((0,0,.3)),Vector((0,0,-1)),1.0)
        if hit is None or abs(hit.z-base)>.235:
            status.append(dict(kind='support',point=list(point),hit=list(hit) if hit else None));continue
        for height in [.35,.72,1.10]:
            for j in range(8):
                angle=j*math.pi/4
                hit2,norm,idx,dist=solid.ray_cast(Vector((x,y,hit.z+height)),Vector((math.cos(angle),math.sin(angle),0)),.22)
                if hit2:status.append(dict(kind='clearance',part=owners[idx],point=[x,y,hit.z+height]))
        top,norm,idx,dist=solid.ray_cast(Vector((x,y,hit.z+.24)),Vector((0,0,1)),1.06)
        if top:status.append(dict(kind='headroom',part=owners[idx],point=[x,y,top.z]))
    record=dict(floor=d['floor'],point=d['point'],width=d['width'],fromRoom=d.get('a'),toRoom=d.get('b'),failures=status)
    door_checks.append(record)
    if status:door_failures.append(record)
report=dict(scope='Door-centre mesh samples using the 1.20 m comparison proxy, not a full Unity capsule sweep.',doors=len(door_checks),intentionallyClosed=1,checkedOpenDoors=len(door_checks)-1,failures=door_failures,passed=not door_failures)
(OUT/'Data/mesh-door-checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
print('DOOR_CHECK '+json.dumps({k:v for k,v in report.items() if k!='failures'})+' failureCount='+str(len(door_failures)),flush=True)
