from pathlib import Path
import json, html

SCRIPT_DIR = Path(__file__).resolve().parent
OUT = SCRIPT_DIR.parent if SCRIPT_DIR.name == 'Plans' else SCRIPT_DIR / 'Deliverables'
PLAN = OUT / 'Plans'
PLAN.mkdir(exist_ok=True)

# All dimensions and adjacency here are design proposals, not surveyed film-set data.
# Coordinates: x east, y toward the rear; 1 unit = 1 proposed metre.
ground = [
 ('C1','응접실',0,0,10,10,'F+S'),
 ('A','현관 · 전실',10,0,14,10,'P'),
 ('C2','식당',24,0,10,10,'P'),
 ('F1','긴 전면 갤러리',34,0,16,10,'U+P'),
 ('D1','교수의 서재',0,10,10,12,'F+P'),
 ('B','주 계단 홀',10,10,14,12,'F+U+S'),
 ('F0','동쪽 연결 복도',24,10,6,16,'U+P'),
 ('F2','동측 회랑',44,10,6,16,'U+P'),
 ('D2','서고',0,22,10,12,'P'),
 ('D3','수집품 전실',10,22,14,6,'P'),
 ('D4','음악 · 휴게실',10,28,14,6,'P'),
 ('S2','보조 계단',24,26,6,8,'P'),
 ('F3','주방',30,28,10,6,'P'),
 ('F4','식료품실',40,28,5,6,'P'),
 ('F5','준비실',45,28,5,6,'P'),
 ('F6','후면 관리 복도',30,26,20,2,'P'),
]
upper = [
 ('E1','여자아이 침실',0,0,10,10,'F+P'),
 ('E2','남자아이 침실',10,0,14,6,'P'),
 ('E4','손님 침실',24,0,10,6,'P'),
 ('E3','가족 복도',10,6,24,4,'U+P'),
 ('F1','상층 긴 갤러리',34,0,16,10,'U+P'),
 ('E5','서쪽 손님 침실',0,10,10,10,'P'),
 ('E6','욕실',0,20,10,6,'F+P'),
 ('E7','린넨실',0,26,10,8,'P'),
 ('B-W','서측 발코니',10,10,2,12,'U+P'),
 ('B-E','동측 발코니',22,10,2,12,'U+P'),
 ('B-N','상층 연결 갤러리',10,22,14,6,'U+P'),
 ('E8','북쪽 복도',10,28,14,6,'U+P'),
 ('F0','동쪽 연결 복도',24,10,6,16,'U+P'),
 ('F2','동측 회랑',44,10,6,16,'U+P'),
 ('S2','다락행 보조 계단',24,26,6,8,'P'),
 ('F7','관리인 공간',30,26,20,8,'P'),
]
attic = [
 ('G1a','목재 복도',0,16,8,6,'F+P'),
 ('G1b','꺾이는 접근 복도',8,16,22,6,'U+P'),
 ('G2','옷장 방',0,22,8,12,'F+S'),
 ('G3','닫힌 창고',8,22,8,12,'P'),
 ('G4','다락 창고',16,22,8,12,'P'),
 ('G1c','계단 전실',24,22,6,4,'P'),
 ('S2','보조 계단',24,26,6,8,'P'),
]

doors = {
 'ground':[(17,0,'h'),(17,10,'h'),(27,10,'h'),(10,6,'v'),(24,6,'v'),(34,6,'v'),(47,10,'h'),(47,26,'h'),(47.5,28,'h'),(42.5,28,'h'),(35,28,'h'),(30,27,'v'),(27,26,'h'),(24,16,'v'),(10,14,'v'),(5,10,'h'),(5,22,'h'),(10,25,'v'),(17,28,'h'),(24,25,'v')],
 'upper':[(11,10,'h'),(23,10,'h'),(27,10,'h'),(11,22,'h'),(23,22,'h'),(10,8,'v'),(17,6,'h'),(29,6,'h'),(34,8,'v'),(47,10,'h'),(47,26,'h'),(30,30,'v'),(27,26,'h'),(24,25,'v'),(17,28,'h'),(10,30,'v'),(10,23,'v'),(10,16,'v')],
 'attic':[(4,22,'h'),(12,22,'h'),(20,22,'h'),(27,22,'h'),(27,26,'h'),(8,19,'v')]
}

def room_record(r):
 return dict(zip(['id','name','x','y','width','depth','basis'],r))
contract = {'status':'review-only, scene not modified','units':'proposed metres; not film measurements', 'orientation':'front at south; chosen for design consistency, not canonical compass direction', 'outer_envelope':[50,34], 'levels':{'ground':0,'split_landing':1.44,'right_intermediate_landing':2.70,'family':3.60,'attic':7.20},'stairs':{'main':'8 risers × 0.18 to split landing; left 12 risers to +3.60; right 7 risers to +2.70 then 5 risers to +3.60','secondary':'two 10-riser flights per 3.60m storey, 0.18 riser / 0.28 tread, 1.4m clear width; repeated same footprint'},'rooms':{'ground':[room_record(r) for r in ground],'upper':[room_record(r) for r in upper],'attic':[room_record(r) for r in attic]},'courtyard':{'x':30,'y':10,'width':14,'depth':16,'basis':'P; introduced to give original long corridors a coherent exterior boundary'},'important':'F describes local visible features, never proof of whole room adjacency. S is approved style, not film evidence. All room positions and connections are proposals.'}
(OUT/'Plans/spatial-contract.json').write_text(json.dumps(contract,ensure_ascii=False,indent=2))

INK='#43382c'; BLUE='#3d7182'; AMBER='#bd793c'; CREAM='#f7f1e6'; PALE='#ece3d4'
SX=120; SY=240; K=31
def X(x):return SX+x*K
def Y(y):return SY+(34-y)*K
def text(x,y,t,size=21,fill=INK,anchor='start',weight=400):
 return f'<text x="{x}" y="{y}" font-size="{size}" fill="{fill}" text-anchor="{anchor}" font-weight="{weight}">{html.escape(t)}</text>'
def rect(x,y,w,d,fill=PALE,stroke=INK,sw=3,dash=''):
 return f'<rect x="{X(x)}" y="{Y(y+d)}" width="{w*K}" height="{d*K}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" stroke-dasharray="{dash}"/>'
def line(x1,y1,x2,y2,color=INK,w=3,dash=''):
 return f'<line x1="{X(x1)}" y1="{Y(y1)}" x2="{X(x2)}" y2="{Y(y2)}" stroke="{color}" stroke-width="{w}" stroke-dasharray="{dash}"/>'
def path(points,color=AMBER,w=6,dash='10 8',arrow=True):
 pts=' '.join(f'{X(x)},{Y(y)}' for x,y in points)
 return f'<polyline points="{pts}" fill="none" stroke="{color}" stroke-width="{w}" stroke-dasharray="{dash}" stroke-linejoin="round"'+(' marker-end="url(#arrow)"' if arrow else '')+'/>'
def stair(x,y,w,d,steps=10,horizontal=False,reverse=False):
 s=rect(x,y,w,d,'#d6baa0',INK,2)
 for i in range(1,steps):
  if horizontal:s+=line(x+i*w/steps,y,x+i*w/steps,y+d,w=1.3)
  else:s+=line(x,y+i*d/steps,x+w,y+i*d/steps,w=1.3)
 points=[(x+.2,y+d/2),(x+w-.2,y+d/2)] if horizontal else [(x+w/2,y+.2),(x+w/2,y+d-.2)]
 s+=path(points[::-1] if reverse else points,BLUE,3,'',True)
 return s
def door(x,y,axis):
 if axis=='h':
  return line(x-.7,y,x+.7,y,CREAM,9)+line(x-.7,y,x-.7,y+1.4,BLUE,2)+f'<path d="M {X(x+.7)} {Y(y)} A {1.4*K} {1.4*K} 0 0 0 {X(x-.7)} {Y(y+1.4)}" fill="none" stroke="{BLUE}" stroke-width="1.5"/>'
 return line(x,y-.7,x,y+.7,CREAM,9)+line(x,y-.7,x+1.4,y-.7,BLUE,2)+f'<path d="M {X(x)} {Y(y+.7)} A {1.4*K} {1.4*K} 0 0 1 {X(x+1.4)} {Y(y-.7)}" fill="none" stroke="{BLUE}" stroke-width="1.5"/>'
def window(x,y,axis,length=2):
 if axis=='h':return line(x-length/2,y,x+length/2,y,CREAM,11)+line(x-length/2,y-.07,x+length/2,y-.07,BLUE,3)+line(x-length/2,y+.07,x+length/2,y+.07,BLUE,3)
 return line(x,y-length/2,x,y+length/2,CREAM,11)+line(x-.07,y-length/2,x-.07,y+length/2,BLUE,3)+line(x+.07,y-length/2,x+.07,y+length/2,BLUE,3)

def floor(name,title,level,rooms,notes):
 s=f'<svg xmlns="http://www.w3.org/2000/svg" width="2200" height="1580" viewBox="0 0 2200 1580"><defs><marker id="arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="5" markerHeight="5" orient="auto-start-reverse"><path d="M0 0L10 5L0 10Z" fill="{AMBER}"/></marker></defs><rect width="2200" height="1580" fill="{CREAM}"/><g font-family="Apple SD Gothic Neo, Noto Sans KR, sans-serif">'
 s+=text(100,80,'PROFESSOR’S HOUSE  /  INTEGRATED DESIGN 01',30,weight=600)+text(100,139,title,43,weight=600)
 s+=text(100,182,'영화의 핵심 공간 + 승인 시안 + 기존 탐험 동선  |  검토용 제안 배치 · 영화 실측 도면 아님',22)
 s+=text(1695,140,level,29,BLUE,weight=600)
 if name!='attic':
  s+=rect(30,10,14,16,'#dce3ce','#6c7b57',3)
  s+=text(X(37),Y(19),'중정',30,'#526043','middle',600)+text(X(37),Y(17.5),'14 × 16 m · P',20,'#526043','middle')
 else:
  s+=rect(0,0,50,34,'none','#b8afa1',2,'10 8')+text(X(38),Y(19),'다락 미개방 / 지붕 구간',22,'#918676','middle')
 room_labels=[]
 label_positions={'ground':{'C1':(5,8.1),'C2':(29,8.3),'D1':(5,20),'B':(17,12.4),'S2':(27,32.5)},'upper':{'E1':(5,8),'E2':(17,3),'S2':(27,32.5)},'attic':{'G2':(4,27),'S2':(27,32.5)}}
 for r in rooms:
  rid,label,x,y,w,d,basis=r
  fill='#e5edf0' if 'F' in basis else ('#eee1d4' if 'U' in basis else '#eeeae3')
  s+=rect(x,y,w,d,fill)
  lx,ly=label_positions.get(name,{}).get(rid,(x+w/2,y+d/2))
  cx=X(lx); cy=Y(ly)
  if w<=2.2:
   room_labels.append(f'<text x="{cx}" y="{cy}" transform="rotate(-90 {cx} {cy})" text-anchor="middle" font-size="17" fill="{INK}">{rid} {label}</text>')
  elif d<=2.2:
   room_labels.append(text(cx,cy+6,f'{rid}  {label}  ·  {basis}',18,INK,'middle',600))
  else:
   bw=min(174,w*K-14)
   box=f'<rect x="{cx-bw/2}" y="{cy-43}" width="{bw}" height="89" rx="6" fill="{fill}" fill-opacity=".96"/>'
   box+=text(cx,cy-22,rid,23,BLUE if 'F' in basis else INK,'middle',700)
   box+=text(cx,cy+7,label,22,INK,'middle',600)
   box+=text(cx,cy+34,f'{w:g} × {d:g} m  ·  {basis}',17,'#786b5c','middle')
   room_labels.append(box)
 for x,y,axis in doors[name]:s+=door(x,y,axis)
 if name in ['ground','upper']:
  for x in [3,7,12,22,28,32,37,42,47]:s+=window(x,0,'h',1.8)
  for y in [4,8,14,18,24,30]:s+=window(0,y,'v',1.8)
  for y in [13,18,23]:s+=window(44,y,'v',1.8)
  # Secondary stair stays in the same footprint on every floor.
  s+=stair(25,27,1.4,2.8,10)+rect(25,29.8,3.1,1.5,'#d6baa0',INK,2)+stair(26.7,27,1.4,2.8,10,reverse=True)
 if name=='ground':
  # Stair hall: short common flight then asymmetric split routes.
  s+=rect(13,11,8,3,'#b9725b',INK,1)+stair(16,14,2,2.4,8)
  s+=rect(15,16.4,4,2.5,'#d6baa0',INK,2)
  s+=stair(11.6,16.4,3.4,2.5,12,True,True)+stair(19,16.4,2.1,2.5,7,True)
  s+=rect(21.1,16.4,2,2.5,'#d6baa0',INK,2)+stair(21.1,18.9,2,1.4,5)
  s+=text(X(17),Y(21),'S1 · 분기 계단',18,INK,'middle',600)
  s+=rect(2,2,5.5,1.3,'#b5bb94',INK,2)+rect(3.3,4.2,2.7,1.2,'#b88a63',INK,2)
  s+=rect(1,5,1.4,2,'#b5bb94',INK,2)+rect(6.6,5,1.4,2,'#b5bb94',INK,2)
  s+=rect(.2,7.7,.8,2,'#b3aaa0',INK,2)
  s+=rect(2,12,5.7,.5,'#9c7960',INK,1)+rect(3.6,16,3,1.4,'#b88a63',INK,2)
  s+=rect(.2,14,.6,6,'#9c7960',INK,1)
  s+=rect(27,2.8,4.2,2.6,'#b88a63',INK,2)
  for x in [27.4,28.8,30.2]:
   s+=rect(x,2,.7,.6,'#b5bb94',INK,1)+rect(x,5.7,.7,.6,'#b5bb94',INK,1)
  s+=path([(17,1),(17,8),(17,13)],AMBER,6)
  s+=path([(25,6),(37,6),(47,6),(47,24)],AMBER,5)
  s+=path([(27,16),(27,24),(27,28)],AMBER,5)
 if name=='upper':
  s+=rect(12,10,10,12,CREAM,'#a39888',2,'10 7')
  s+=text(X(17),Y(12.8),'계단 홀 상부 보이드',23,INK,'middle',600)
  s+=text(X(17),Y(11.7),'바닥 없음 / 아래층 조망',18,INK,'middle')
  s+=stair(16,14,2,2.4,8)+rect(15,16.4,4,2.5,'#d6baa0',INK,2)
  s+=stair(11.6,16.4,3.4,2.5,12,True,True)+stair(19,16.4,2.1,2.5,7,True)
  s+=rect(21.1,16.4,2,2.5,'#d6baa0',INK,2)+stair(21.1,18.9,2,1.4,5)
  for x,y in [(1.5,2),(6.2,2),(12,1),(21,1)]:
   s+=rect(x,y,2,3.2,'#c5baab',INK,2)+rect(x+.1,y+2.3,1.8,.8,CREAM,INK,1)
  s+=path([(11,21),(11,8),(32,8),(40,8)],AMBER,5)
  s+=path([(22,23.8),(27,23.8),(27,27.6)],AMBER,5)
 if name=='attic':
  s+=stair(25,27,1.4,2.8,10)+rect(25,29.8,3.1,1.5,'#d6baa0',INK,2)+stair(26.7,27,1.4,2.8,10,reverse=True)
  s+=window(0,25,'v',2)+window(0,29,'v',2)
  s+=rect(2.7,31.1,2.6,1.0,'#8b644b',INK,3)
  s+=text(X(4),Y(30),'옷장',17,INK,'middle',600)
  s+=path([(27,27),(27,19),(4,19),(4,28)],AMBER,6)
  s+=line(1,22.5,1,33.5,BLUE,2,'8 5')
  s+=text(X(-.5),Y(27),'창 2개',18,BLUE,'end')
  s+=text(X(4),Y(24),'중앙 가구 없음',17,INK,'middle')
 s+=''.join(room_labels)
 # Common dimension datum and orientation.
 s+=line(0,-1.3,50,-1.3,'#918676',2)+line(0,-1,0,-1.6,'#918676',2)+line(50,-1,50,-1.6,'#918676',2)
 s+=text(X(25),Y(-2.6),'전체 외곽 기준 50 m  /  정면 · 진입 방향 ↓',21,INK,'middle')
 s+=text(1790,255,'도면 읽는 법',26,weight=600)
 labels=[('F','영화에서 확인한 부분 요소'),('S','승인 시안의 조형·재질'),('U','기존 씬의 동선에서 계승'),('P','통합을 위한 새 설계')]
 for i,(code,desc) in enumerate(labels):
  yy=310+i*65;s+=text(1790,yy,code,25,BLUE,weight=700)+text(1830,yy,desc,19)
 s+=text(1790,605,'모든 치수와 방의 연결 위치는',20)+text(1790,635,'게임용 제안치입니다.',20)
 s+=text(1790,680,'F 표시는 실제 방 배치가',20)+text(1790,710,'확정됐다는 뜻이 아닙니다.',20)
 for i,n in enumerate(notes):s+=text(1790,795+i*39,n,19)
 s+=text(1790,1175,'지형의 북쪽은 미확정.',19)+text(1790,1205,'이 도면의 위 = 저택 후면.',19)
 s+=text(100,1485,'검토 초점: B 계단 분기  →  E 상층 복도  →  G 옷장 방 / F 긴 복도는 중정 둘레로 연결',22)
 s+=text(100,1530,'정밀 시공도 아님 · 검토 이미지 우선 · Unity 씬 적용 전',20,'#867869')
 s+='</g></svg>'
 (PLAN/f'{name}.svg').write_text(s)

floor('ground','01 / 1층 · 현관, 홀, 생활 공간','F0  +0.00 m',ground,[
 'B: 중앙 짧은 계단 → 양쪽 분기',
 'C/D: 응접실 → 서재 → 서고',
 'F: 기존 긴 동선을 중정과 연결',
 '주방·관리 공간은 새 설계',
 '주 계단 + 보조 계단의 2개 축'])
floor('upper','02 / 2층 · 가족 방과 상층 갤러리','F1  +3.60 m',upper,[
 'B 중앙은 아래층으로 열린 공간',
 'E3 가족 복도에서 침실로 진입',
 '상층 갤러리에서 계단 홀 조망',
 'S2 보조 계단 → 다락 복도',
 '침실 배치·욕실 내부는 제안'])
floor('attic','03 / 다락층 · 옷장 방으로 가는 길','F2  +7.20 m',attic,[
 'G2: 입구 남쪽 / 옷장 북쪽',
 '서쪽 외벽 창 2개 유지',
 '창 바깥에 방이나 복도 없음',
 'G1: 한 번 꺾이는 접근 동선',
 'G3/G4: 닫힌 창고 공간 제안'])

pages=[]
for f in ['ground','upper','attic']:
 pages.append(f'<section class="page"><img src="{f}.svg"></section>')
(PLAN/'Floorplans.html').write_text('<!doctype html><meta charset="utf-8"><title>Professor House / Floor Plans</title><style>@page{size:440mm 316mm;margin:0}*{box-sizing:border-box}body{margin:0;background:#f7f1e6}.page{width:2200px;height:1580px;break-after:page}.page img{width:100%;height:100%}@media print{.page{width:440mm;height:316mm}}</style>'+''.join(pages))
print('Created three editable floor plans and spatial-contract.json')
