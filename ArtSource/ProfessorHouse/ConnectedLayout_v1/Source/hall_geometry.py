"""Professor House proportion review 04. One source per zone; no mesh stretching."""
from pathlib import Path
import copy
import json
import math
from geometry import box, beam, sphere, lathe, poly, arch, wall_box, panel_run, add_stair_trim

OUT=Path(__file__).resolve().parent.parent
parts=[]
H=4.32
CEILING=8.84
TOP=9.0
LX,RX=15.4,18.6
FRONT,BACK=19.35,21.85
MID=1.8
TURN=3.24
WEND=11.48
RSTART,REND=20.84,22.44
RBACK=22.63

def part(pid,title,role,level=0,explode=(0,0,0),hidden=False):
    p=dict(id=pid,title=title,role=role,level=level,explode=list(explode),pivot=None,defaultHidden=hidden,shapes=[])
    parts.append(p);return p

def slab(pid,title,rect,holes=(),z=H,level=1):
    p=part(pid,title,'gallery',level,(0,0,1))
    x,y,w,d=rect
    xs=sorted(set([x,x+w]+[max(x,min(x+w,v)) for a,b,c,e in holes for v in [a,a+c]]))
    ys=sorted(set([y,y+d]+[max(y,min(y+d,v)) for a,b,c,e in holes for v in [b,b+e]]))
    for a,b in zip(xs,xs[1:]):
        for c,e in zip(ys,ys[1:]):
            if any(xx<(a+b)/2<xx+ww and yy<(c+e)/2<yy+dd for xx,yy,ww,dd in holes):continue
            box(p,a,c,z-.24,b-a,e-c,.24,'floor',0)
    return p

def stair(pid,title,origin,direction,n,width,tread=.28):
    p=part(pid,title,'stairs',0,(0,-1,1))
    p['shapes'].append(dict(type='stair',origin=list(origin),direction=list(direction),n=n,width=width,tread=tread,rise=.18,mat='wood'))
    return p

wall_openings={}
def wall(pid,title,axis,line,start,end,openings,exp,hidden=False):
    p=part(pid,title,'wall',0,exp,hidden)
    wall_openings[pid]=dict(axis=axis,line=line,openings=openings)
    cuts=sorted(set([start,end]+[v for a,b,lo,hi in openings for v in [max(start,a),min(end,b)]]))
    for a,b in zip(cuts,cuts[1:]):
        if b-a<.00001:continue
        mid=(a+b)/2;cursor=0
        obs=sorted((lo,hi) for aa,bb,lo,hi in openings if aa<mid<bb)
        for lo,hi in obs+[(TOP,TOP)]:
            if lo>cursor:wall_box(p,axis,line,a,cursor,b-a,lo-cursor,.3,'plaster',.008)
            cursor=max(cursor,hi)
        if not any(aa<mid<bb and lo<1.6 for aa,bb,lo,hi in openings):
            face=line+(.19 if line==10 else -.19)
            panel_run(p,axis,face,a,b)
    for u in [start+.18,end-.18]+[start+v for v in [3,6,9] if start+v<end-.5]:
        if any(a-.2<u<b+.2 for a,b,lo,hi in openings):continue
        wall_box(p,axis,line,u-.12,0,.24,TOP,.44,'wood',.028)
    for z in [H-.32,8.5]:
        for a,b in zip(cuts,cuts[1:]):
            mid=(a+b)/2
            if not any(aa<mid<bb and lo<z+.24 and hi>z for aa,bb,lo,hi in openings):wall_box(p,axis,line,a,z,b-a,.24,.42,'wood',.025)
    return p

p=part('B01','홀 바닥','floor',0,(0,0,-.2));box(p,10,10,-.24,14,12,.24,'stoneAlt',0)
p=part('B01T','석재 타일','surface',0,(0,0,-.5))
for ix in range(14):
    for iy in range(12):box(p,10+ix+.012,10+iy+.012,.003,.976,.976,.022,'stone' if (ix+iy)%3 else 'stoneAlt',.009)
slab('B07W','서측 회랑 +4.32',(10,10,2,12),[(WEND,FRONT,12-WEND,BACK-FRONT)])
slab('B07E','동측 회랑 +4.32',(22,10,2,12),[(22,FRONT,REND-22,22-FRONT)])
# Two enclosed arrival spaces; no floor crosses the outdoor garden.
slab('B07NW','서쪽 창가 전실',(10,22,2.4,3))
slab('B07NE','동쪽 계단 도착 전실',(20.3,22,3.7,3),[(RSTART,22,REND-RSTART,RBACK-22)])
stair('B03','중앙 10단 · 18 cm 유지',(16,16.35,0),(0,1),10,2,.3)
p=part('B04','공통 계단참 · 3.2 × 2.5 m','stairs',0,(0,-.5,1));box(p,LX,FRONT,MID-.28,RX-LX,BACK-FRONT,.28,'wood',0)
stair('B05','왼쪽 14단 · 창 벽을 따라 상승',(LX,FRONT,MID),(-1,0),14,2.5)
stair('B06a','오른쪽 8단',(RX,20.95,MID),(1,0),8,1.6)
p=part('B06b','오른쪽 회전 계단참 +3.24','stairs',0,(0,-1,1));box(p,RSTART,FRONT,TURN-.28,1.6,1.6,.28,'wood',0)
stair('B06c','오른쪽 6단 · 후면 회랑으로 도착',(RSTART,20.95,TURN),(0,1),6,1.6)

north=wall('B02N','창 벽 · 계단 통과 개구부','h',22,10,24,[(15.9,18.1,2.6,8.2),(10.2,11.8,H,7.1),(20.62,22.64,3.65,7.1),(22.84,23.86,H,7.1)],(0,3,0))
for side in [-1,1]:
    points=[(17+side*1.1,8.2),(17,8.2)]+[(17+side*1.1*math.sin(i*math.pi/32),6.9+1.1*math.cos(i*math.pi/32)) for i in range(17)]
    poly(north,points,'h',22,.3,'plaster',0)
wall('B02W','서측 높은 벽 · 석재 아치','v',10,10,22,[(12.2,15.8,0,3.8),(18.25,19.75,H,7.02)],(-3,0,0))
wall('B02E','동측 높은 벽','v',24,10,22,[(14.9,17.1,0,3.2)],(3,0,0),True)
wall('B02S','현관 쪽 벽','h',10,10,24,[(15.5,18.5,0,3.2),(10.25,11.75,H,7.1),(22.75,23.95,H,7.1)],(0,-3,0),True)
trim=part('B14','계단 옆판 · 둥근 디딤판','stairs-trim',0,(0,-1,1))
runner=part('B11','계단 러너와 홀 러그','decor',0,(0,-2,.5))
for p in list(parts):
    if p['role']=='stairs' and p['shapes'][0]['type']=='stair':add_stair_trim(p,trim,runner)
box(runner,LX+.22,FRONT+.05,MID+.007,RX-LX-.44,BACK-FRONT-.15,.018,'rug',0)
box(runner,RSTART+.14,FRONT+.12,TURN+.007,1.32,1.36,.018,'rug',0)
box(runner,13,11.0,.035,8,3.4,.018,'rug',.008)
for x,y,w,d in [(13.14,11.14,7.72,.035),(13.14,14.24,7.72,.035),(13.14,11.14,.035,3.1),(20.82,11.14,.035,3.1)]:box(runner,x,y,.054,w,d,.008,'rugGold',0)
p=part('B16','작은 계단참 하부 받침','structure',0,(0,0,-.2))
box(p,LX+.02,FRONT+.02,0,RX-LX-.04,BACK-FRONT-.04,MID-.28,'wood',.018)
panel_run(p,'h',FRONT,LX,16,0,MID-.28);panel_run(p,'h',FRONT,18,RX,0,MID-.28)
panel_run(p,'v',RX,20.95,BACK,0,MID-.28)

rails={pid:part(pid,title,'rails',level,ex) for pid,title,level,ex in [
 ('B08W','서측 회랑 난간',1,(-3,0,2)),('B08E','동측 회랑 난간',1,(3,0,2)),
 ('B08S','계단 손잡이와 난간',0,(0,-2,1)),('B08L','계단참·후면 회랑 난간',1,(0,0,2)),
 ('B08P','연결부 공통 기둥 · 작은 둥근 장식',0,(0,0,2))]}
guards=[];nodes={}
def guard(pid,label,a,b,hand=False):
    a=list(a);b=list(b);p=rails[pid]
    guards.append(dict(id=label,part=pid,a=a,b=b,handrailOnly=hand,height=1.0,barrierThickness=.14))
    beam(p,[*a[:2],a[2]+1.0],[*b[:2],b[2]+1.0],.075)
    if not hand:
        beam(p,[*a[:2],a[2]+.12],[*b[:2],b[2]+.12],.055)
        n=max(2,math.ceil(math.dist(a[:2],b[:2])/.24))
        for i in range(1,n):
            t=i/n;c=[a[j]*(1-t)+b[j]*t for j in range(3)]
            lathe(p,c,[(.17,.046),(.27,.046),(.31,.064),(.38,.057),(.46,.039),(.67,.039),(.73,.056),(.82,.056),(.88,.043),(.94,.043)],'woodLight',8)
            box(p,c[0]-.055,c[1]-.055,c[2]+.12,.11,.11,.13,'wood',.014)
    for c in [a,b]:
        key=tuple(round(v,4) for v in c[:2]);n=nodes.setdefault(key,dict(x=c[0],y=c[1],levels=[],edges=[]));n['levels'].append(c[2]);n['edges'].append(label)

guard('B08S','main_left',(16,16.35,0),(16,FRONT,MID))
guard('B08S','main_right',(18,16.35,0),(18,FRONT,MID))
guard('B08L','landing_front_w',(LX,FRONT,MID),(16,FRONT,MID))
guard('B08L','landing_front_e',(18,FRONT,MID),(RX,FRONT,MID))
guard('B08L','landing_east_return',(RX,20.95,MID),(RX,BACK,MID))
guard('B08S','left_front',(LX,FRONT,MID),(WEND,FRONT,H))
guard('B08S','left_wall_handhold',(LX,BACK,MID),(WEND,BACK,H),True)
guard('B08W','west_front',(11.92,10.15,H),(11.92,FRONT,H))
guard('B08W','west_stair_return',(11.92,FRONT,H),(WEND,FRONT,H))
guard('B08S','right_front',(RX,FRONT,MID),(RSTART,FRONT,TURN))
guard('B08S','right_rear',(RX,20.95,MID),(RSTART,20.95,TURN))
guard('B08L','right_turn_a',(RSTART,FRONT,TURN),(22,FRONT,TURN))
guard('B08L','right_turn_b',(22,FRONT,TURN),(REND,FRONT,TURN))
guard('B08S','right_final_inner',(RSTART,20.95,TURN),(RSTART,RBACK,H))
guard('B08E','east_front',(22,10.15,H),(22,FRONT,H))
guard('B08E','east_cut_return',(22,FRONT,H),(REND,FRONT,H))
guard('B08E','east_cut_side_turn',(REND,FRONT,H),(REND,20.95,H))
guard('B08E','east_cut_side_stair',(REND,20.95,H),(REND,RBACK,H))
box(rails['B08E'],REND-.04,FRONT,TURN-.28,.08,RBACK-FRONT,H-(TURN-.28),'wood',.012)
guard('B08S','right_outer_turn_handhold',(REND,FRONT,TURN),(REND,20.95,TURN),True)
guard('B08S','right_outer_final_handhold',(REND,20.95,TURN),(REND,RBACK,H),True)
# Outdoor garden; its boundaries are building walls, not a railing around an indoor hole.
guard('B08L','rear_stair_left_return',(20.62,22,H),(RSTART,22,H))
guard('B08L','rear_stair_left_edge',(RSTART,22,H),(RSTART,RBACK,H))
box(rails['B08L'],RSTART-.04,22,3.68,.08,RBACK-22,H-3.68,'wood',.012)
p=rails['B08P']
for n in nodes.values():
    x,y=n['x'],n['y']
    # This 22 cm return is anchored to the doorway; a second finial would overlap.
    if abs(x-20.62)<1e-6 and abs(y-22)<1e-6:continue
    z=min(n['levels']);h=max(n['levels'])+1-z
    box(p,x-.135,y-.135,z,.27,.27,.15,'wood',.025)
    box(p,x-.10,y-.10,z+.13,.20,.20,h-.07,'wood',.022)
    box(p,x-.145,y-.145,z+h,.29,.29,.09,'woodEdge',.022)
    lathe(p,[x,y,z+h+.085],[(0,.072),(.045,.089),(.075,.064)],'wood',10)
    sphere(p,[x,y,z+h+.21],.13,'wood')

p=part('B15','높인 회랑 받침 · 바닥 아래로만 배치','structure',0,(0,0,1))
beams=[('west_front',11.76,10.15,.24,FRONT-10.15),('west_cut_front',WEND,FRONT-.24,11.76-WEND,.24),
 ('west_cut_side',WEND-.24,FRONT,.24,BACK-FRONT),('west_rear',11.76,BACK,.24,22.24-BACK),
 ('east_front',22,10.15,.24,FRONT-10.15),('east_cut_front',22.24,FRONT-.24,REND+.24-22.24,.24),
 ('east_cut_side',REND,FRONT,.24,RBACK-FRONT),('north_stair_left',RSTART-.24,22.15,.24,RBACK-22.15),
 ('north_stair_rear',RSTART-.24,RBACK,REND-RSTART+.48,.24),
 ('north_west',12,22,.4,.24),('north_east',20.3,22,RSTART-20.3,.24)]
for name,x,y,w,d in beams:
    box(p,x,y,H-.52,w,d,.28,'wood',.025);p['shapes'][-1].update(constructionRole='gallery_edge_beam',segmentId=name)
for x in [10.55,23.45]:
    for y in [10.8,15.5,20.8]:
        box(p,x-.20,y-.20,0,.4,.4,H-.23,'wood',.04)
        box(p,x-.26,y-.26,0,.52,.52,.2,'wood',.025)
        box(p,x-.27,y-.27,H-.55,.54,.54,.22,'woodEdge',.025)
        for sign in [-1,1]:
            reach=min(.7,y-10.15 if sign<0 else BACK-y)
            poly(p,[(y,H-1),(y,H-.32),(y+sign*reach,H-.32),(y+sign*reach*.8,H-.42),(y+sign*reach*.57,H-.68),(y+sign*reach*.36,H-.88)],'v',x,.22)

p=part('B09','홀 천장 +8.84 · 다락 바닥 +9.00','ceiling',2,(0,0,4),True)
for y in [10.7,15.8,21.3]:box(p,10,y-.2,8.5,14,.4,.38,'wood',.045)
box(p,10,10,CEILING,14,12,.16,'plaster',0)
p=part('B10F','높은 석재 아치','kit',0,(-3,-1,0))
for yy in [12.0,15.65]:
    for i in range(5):box(p,9.65,yy,i*.43,.7,.35,.42,'stone' if i%2 else 'stoneAlt',.025)
arch(p,'v',10,14,4,2.15,1.65,.7,.30)
p=part('B12','길어진 아치창 · 계단참과 같은 후면 벽','window',0,(0,3,0))
points=[(15.94,2.64),(18.06,2.64),(18.06,6.9)]+[(17+1.06*math.cos(i*math.pi/16),6.9+1.06*math.sin(i*math.pi/16)) for i in range(1,17)]
poly(p,points,'h',21.95,.055,'glass',0)
for x in [15.87,18.13]:box(p,x-.11,21.65,2.49,.22,.45,4.45,'stone',.022)
arch(p,'h',21.85,17,2.48,6.9,1.24,.45,.18)
box(p,15.76,21.58,2.47,2.48,.6,.16,'stone',.025)
for sign in [-1,1]:
    for k in range(-16,17):
        pts=[]
        for j in range(151):
            x=15.95+2.1*j/150;z=sign*(x-17)*1.6+5.2+k*.44;top=6.9+math.sqrt(max(0,1.06**2-(x-17)**2))
            if 2.66<z<top-.04:pts.append([x,21.87,z])
        if len(pts)>2:beam(p,pts[0],pts[-1],.011,'brass',True)
for x in [16.65,17.35]:box(p,x-.03,21.78,2.64,.06,.12,5.29,'wood',.012)
p=part('B17','벽 촛대','decor',0,(0,1,0))
for x in [14.6,19.4]:
    box(p,x-.10,21.54,3.7,.20,.13,.42,'brass',.03)
    beam(p,[x,21.5,3.9],[x,21.17,3.9],.04,'brass',True)
    lathe(p,[x,21.17,3.87],[(0,.08),(.08,.12),(.11,.07)],'brass',10)
    box(p,x-.035,21.13,3.97,.07,.07,.23,'linen',.015)
    sphere(p,[x,21.17,4.25],.055,'flame',[.7,.7,1.7])
p=part('B18','상층 출입구 목재 문틀','portal-frame',1,(0,2,0))
for a,b,lo,hi in [(10.2,11.8,H,7.1),(20.62,22.64,3.65,7.1),(22.84,23.86,H,7.1)]:
    for x in [a-.12,b]:box(p,x,21.75,lo,.12,.38,hi-lo,'wood',.018)
    box(p,a-.16,21.71,hi,b-a+.32,.44,.18,'wood',.025)
p=part('B13','1.20 m 크기 비교 인형','scale',0,(0,-3,0))
box(p,13,14.8,0,.28,.2,.73,'green',.06);sphere(p,[13.14,14.9,.96],.24,'linen')

hall=dict(title='B · 후정과 좌우 연결 설명 모형',parts=parts,target=[17,18.2,4],radius=13.7)
