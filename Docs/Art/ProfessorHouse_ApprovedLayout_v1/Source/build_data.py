from pathlib import Path
import copy
import json

OUT = Path(__file__).resolve().parent.parent
D = json.loads((OUT / 'Data/layout_01.json').read_text())
D['version'] = 'floor-review-03-west-garden'
D['status'] = 'Approved layout v1 (2026-10-22), based on FloorPlans_03; Unity/model implementation pending'

def room(id, name, rect, kind, note, label=None, pos=None):
    r = dict(id=id, name=name, rect=rect, kind=kind, note=note, label=label or [name])
    if pos: r['labelPos'] = pos
    return r

def door(a, b, axis, point, width=1.4, **extra):
    return dict(a=a, b=b, axis=axis, point=point, width=width, kind='door', **extra)

def route(scene, name, points, kind='walk', **extra):
    return dict(scene=scene, name=name, points=points, kind=kind, **extra)

ground = D['floors']['ground']
ground.update(title='1층 · 서쪽 정원에서 시작된 유물 파손 사고', subtitle='서쪽 정원 → 유물 전시실 창 → 갑옷 일부 파손 → 홀을 지나 보조 계단으로',
              help='서쪽 정원에서 친 공이 유물 전시실 창을 깨고 갑옷 일부를 망가뜨립니다. 아이들은 응접실의 정원문으로 확인하러 들어왔다가, 뒤쪽에서 들리는 발소리를 피해 홀의 아치로 빠져나갑니다.', default='D1')
for r in ground['rooms']:
    if r['id']=='D1': r.update(name='유물 전시실 · 창문 사고', label=['유물 전시실','창문·갑옷 사고'], labelPos=[5.2,20.5], note='서쪽 정원 쪽 창으로 들어온 공이 창 안쪽 갑옷의 투구·팔 장식 일부를 떨어뜨립니다. 유물을 망가뜨렸다는 두려움과 응접실 쪽에서 다가오는 발소리가 피신의 계기가 됩니다. 동쪽 아치를 통해 홀로 빠져나갑니다.')
    if r['id']=='D2': r.update(name='공용 도서실', label=['공용 도서실'], note='1층에서 손님과 아이들이 이용하는 도서실입니다. 2층 교수의 개인 서재·서고와 구별합니다.')
    if r['id']=='D4': r.update(name='음악 · 휴게실', label=['음악·휴게실'], note='전면 동쪽의 공용 음악·휴게 공간입니다.')
    if r['id']=='C1': r.update(name='응접실 · 서쪽 정원문', label=['응접실','서쪽 정원문'], labelPos=[5,4.5], note='서쪽 정원에서 들어오는 출입문을 둡니다. 아이들은 깨진 창을 넘지 않고 이 문과 전시실의 남쪽 문으로 들어갑니다. 이후 맥크리디가 현관 쪽에서 이 방을 지나 접근합니다.')
    if r['id']=='B': r['labelPos']=[13.5,17.2]
    if r['id']=='F0': r['note']='전시실에서 홀을 가로질러 들어오는 동쪽 연결 복도입니다. 아이들은 이 복도 끝의 보조 계단으로 올라가 다락에 숨습니다.'
ground['doors'].append(door('OUT','C1','v',[0,7.5],1.6))
ground['windows']=[w for w in ground['windows'] if not(w['axis']=='v' and w['point']==[0,18])]
ground['windows'].append(dict(axis='v',point=[0,18],width=2.3,event='break'))
ground['routes']=[
    route('escape','정원문으로 들어와 파손 확인',[[ -6,10],[-6,7.5],[5,7.5],[5,12],[2.5,12],[2.5,17]],phase='entry',outside=True),
    route('escape','전시실 아치를 통해 피신',[[2.5,17],[5.7,17],[5.7,14],[22,14],[22,16],[27,16],[27,27]],phase='flight'),
    route('escape','공이 창을 뚫고 갑옷에 맞음',[[-7,18],[2.3,18]],kind='ball'),
    route('escape','맥크리디가 응접실에서 접근',[[13,6],[5.45,6],[5.45,9.3]],kind='adult'),
]
ground['route']=[]

upper=D['floors']['upper']
keep={r['id']:copy.deepcopy(r) for r in upper['rooms']}
upper.update(title='2층 · 교수의 서쪽 날개와 외진 아이들 방', subtitle='계단 도착 → 교수 구역 경고 → 반대편 긴 복도 → 아이들 침실',
             help='왼쪽 계단을 올라온 자리에서 서쪽 교수 공간을 가리킵니다. 안내는 현관 쪽 복도로 돌아 동측 회랑 끝의 아이들 방으로 이어집니다.',default='P-STUDY')
upper['rooms']=[
    room('P-BED','교수 침실',[0,0,10,10],'professor','개인 서재와 같은 서쪽 날개입니다. 공용 E3 복도에는 방문을 내지 않고 교수 전용 복도에서 접근합니다.',pos=[5,7.5]),
    room('P-STUDY','교수의 서재',[0,10,8,12],'professor','2층으로 옮긴 교수의 업무 공간. 회랑의 경계문 → 전용 복도 → 서재 순서로 들어갑니다. 아이들 안내·탐험·피신 경로에서는 통과하지 않습니다.',pos=[4,19.4]),
    room('P-LIB','개인 서고',[0,22,8,6],'professor','교수의 책과 연구자료를 보관합니다. 1층 공용 도서실과 다릅니다.',pos=[4,25.7]),
    room('P-BATH','교수 욕실',[0,28,8,6],'professor','서쪽 전용 구역 안의 욕실입니다.',pos=[4,31.5]),
    room('P0','교수 전용 복도',[8,10,2,24],'professor','서측 회랑의 경계문 한 곳으로 공용 공간과 연결합니다. 침실·서재·개인 서고를 내부에서 잇습니다.',label=['교수 전용 복도']),
    room('P-STORE','자료 보관실',[10,25,2.4,9],'professor','전용 복도의 옆문으로 들어갑니다. 창가 전실의 북쪽 벽을 뚫어 통로로 만들지 않습니다.',label=['자료','보관'],pos=[11.2,31.2]),
    room('E2','작은 응접실',[10,0,14,6],'social','숨바꼭질 중 살펴볼 수 있는 공용 방입니다. 기존 남자아이 침실 기능은 동쪽 끝으로 이동합니다.',pos=[17,3]),
    keep['E4'], keep['E3'], keep['F8'], keep['F9'], keep['F1'],
    keep['B-W'],keep['B-E'],keep['B-NW'],keep['B-NE'],keep['ER'],keep['F0'],keep['F2'],keep['S2'],
    room('K0','아이들 침실 앞 복도',[30,26,20,2],'children','동측 회랑 끝에서 한 번 더 꺾어 들어오는 조용한 침실 구역입니다. 서쪽 끝은 붙박이 린넨장과 벽으로 마감하며 보조 계단 S2로 통하는 문은 없습니다.',pos=[40,27.1]),
    room('E1','여자아이 침실',[30,28,8.5,6],'children','수잔과 루시의 방. 중앙 홀에서 보이지 않는 동쪽 후면 끝에 배치합니다. 남자아이 방과 같은 침실 복도를 사용합니다.',label=['여자아이 침실'],pos=[34.25,33]),
    room('K-BOYS','남자아이 침실',[38.5,28,8.5,6],'children','피터와 에드먼드의 방. 여자아이 방 옆이지만 각각 방문이 있으며 홀·교수 공간에서 멀리 떨어져 있습니다.',label=['남자아이 침실'],pos=[42.75,33]),
    room('K-BATH','아이들 욕실',[47,28,3,6],'children','침실 앞 복도에서 바로 접근합니다. 욕실 때문에 교수의 공간을 지나갈 필요가 없습니다.',label=['아이들','욕실'],pos=[48.5,32.2]),
]
for r in upper['rooms']:
    if r['id']=='E3': r.update(name='전면 연결 복도 · E3',label=['전면 연결 복도 E3'],labelPos=[22,9.5],note='홀 앞쪽에서 좌우 회랑과 동쪽 날개를 잇는 공용 복도입니다. 아이들 침실은 이 복도에 바로 붙지 않고 동쪽 회랑 끝으로 이동했습니다.')
    if r['id']=='F8': r.update(name='놀이 · 휴게실',kind='social',label=['놀이·휴게실'],labelPos=[39,3.8],note='비 오는 날 숨바꼭질을 시작하는 방으로 제안합니다. 놀이가 시작되면 빈 손님방·응접실·회랑으로 흩어져 탐색합니다.')
    if r['id']=='F9': r.update(name='공용 욕실 · 린넨',label=['공용 욕실','린넨'],kind='service',labelPos=[47,3.7],note='공용 구역의 욕실과 린넨 공간입니다. 동쪽 침실 구역의 생활용품을 이쪽에서 관리합니다.')
    if r['id']=='B-NW': r.update(label=['창가 전실'],note='앞은 끝벽, 정원 쪽은 창가 좌석인 작은 목적 공간입니다. 서쪽 교수 전용 복도로 연결되는 옆문은 없앴습니다.',labelPos=[11.2,24.3])
    if r['id']=='B-NE': r.update(label=['도착 전실'],labelPos=[22.3,24.3])
    if r['id']=='E4': r.update(name='빈 손님방',kind='social',label=['빈 손님방'],labelPos=[30.6,3],note='처음 탐험할 수 있는 빈 손님방입니다. 옷장방으로 가는 통과 방이 아니라 숨을 곳을 찾는 작은 탐색 갈래입니다.')
    if r['id']=='F1': r.update(name='전면 갤러리',label=['전면 갤러리'],labelPos=[39,9.55])
    if r['id']=='F2': r.update(name='동측 긴 회랑',label=['동측 긴 회랑'],note='중정을 따라 북쪽으로 걸으면 끝에서 아이들 침실 복도로 꺾입니다. 홀에서는 침실 문이 보이지 않습니다.')
    if r['id']=='S2': r['note']='공용 복도 F0에서 들어가는 1층·2층·다락 연결 계단입니다. 2층에서는 아이들 침실 복도 K0와 벽으로 분리해 침실 바로 앞이 다락 지름길이 되지 않게 했습니다.'

upper['doors']=[
    door('B-W','P0','v',[10,19],1.5,restriction='professor'),
    door('P0','P-BED','h',[9,10],1.3),door('P0','P-STUDY','v',[8,18],1.6),
    door('P0','P-LIB','v',[8,25]),door('P0','P-BATH','v',[8,31]),door('P0','P-STORE','v',[10,29]),
    door('E3','E2','h',[17,6]),door('E3','E4','h',[29,6]),
    door('E3','B-W','h',[11,10],1.5),door('E3','B-E','h',[23.35,10],1.2),
    door('E3','F0','h',[27,10],1.8),door('E3','F1','v',[34,8]),
    door('B-W','B-NW','h',[11,22],1.6),door('B-E','B-NE','h',[23.35,22],1.02),
    door('B-NE','F0','v',[24,23.5],1.7),door('ER','S2','v',[24,33]),
    door('F0','S2','h',[27,26],2),door('F1','F8','h',[39,6]),door('F1','F9','h',[47,6]),
    door('F1','F2','h',[47,10],1.6),door('F2','K0','h',[47,26],1.5),
    door('K0','E1','h',[34.25,28]),door('K0','K-BOYS','h',[42.75,28]),door('K0','K-BATH','h',[48.5,28],1.2),
]
upper['windows']=[w for w in upper['windows'] if w['point']!=[9,34]]
upper['routes']=[
    route('arrival','아이들 안내',[[11,20.6],[11,19],[11,8],[47,8],[47,27],[34.25,27],[34.25,29]],phase='main'),
    route('arrival','남자아이 방',[[42.75,27],[42.75,29]],phase='branch'),
    route('arrival','교수 쪽을 가리킴',[[11,19],[9.15,19]],kind='point'),
    route('hide','루시의 탐색',[[39,3],[39,8],[23.35,8],[23.35,23.5],[27,23.5],[27,27]],phase='main'),
    route('hide','손님방을 살펴봄',[[29,8],[29,4]],phase='explore'),
    route('hide','응접실을 살펴봄',[[23.35,8],[17,8],[17,4]],phase='explore'),
    route('escape','계단을 계속 올라감',[[27,27],[27,29]],phase='flight'),
]
upper['route']=[]

attic=D['floors']['attic']
attic.update(title='다락 · 탐험 끝에 발견하고 다시 찾아오는 옷장방',subtitle='S2 → 계단 전실 → 목재 복도 → 꺾인 끝의 방문 → 옷장',
             help='숨바꼭질 때 처음 발견한 길을 창문 사고 뒤 네 아이가 다시 이용합니다. 이 경로는 아래층 교수 구역을 통과하지 않습니다.')
for r in attic['rooms']:
    if r['id']=='G2':r['note']='목재 복도를 걷다가 끝에서 꺾어 들어가는 옷장방입니다. 첫 방문은 탐색의 발견, 이후 단체 피신은 이미 아는 길을 되짚는 장면입니다.'
    if r['id']=='G3':r['note']='문과 물품이 있는 창고. 탐색할 수 있는 갈래이며 통로가 잘려 보이는 막다른 길이 되지 않게 합니다.'
attic['routes']=[]
for scene in ['hide','escape']:
    attic['routes'].append(route(scene,'발견한 옷장방으로',[[27,27],[27,24],[27,19],[4,19],[4,24],[4,29.6]],phase='main'))
attic['routes'].append(route('hide','창고를 살펴봄',[[22,19],[22,24]],phase='explore'))
attic['route']=[]

D['scenes']={
 'overview':dict(title='방 배치',startFloor='upper',text='서쪽 교수 구역, 동쪽 후면 아이들 구역, 중앙·전면의 공용 공간을 구분합니다.',steps=['교수의 서재·침실·서고는 2층 서쪽 날개로 묶습니다.','남녀 아이들 침실은 같은 외진 구역에 두되 각각 방문을 둡니다.','주 계단, 후정, 다락 옷장방의 기존 관계를 유지합니다.']),
 'arrival':dict(title='① 첫 안내',startFloor='upper',text='주 계단의 왼쪽 갈래로 올라와 교수 구역을 가리킨 다음, 몸을 돌려 반대편 침실로 안내합니다.',steps=['회랑 도착 지점에서 가까운 교수 전용 경계문을 가리킵니다.','현관 쪽 E3 → 전면 갤러리 → 동측 긴 회랑으로 이동합니다.','회랑 끝에서 다시 꺾으면 남녀 침실과 욕실이 있는 조용한 구역입니다.']),
 'hide':dict(title='② 숨바꼭질',startFloor='upper',text='빈 방과 회랑을 탐색하다 보조 계단을 발견하고, 다락 복도 끝에서 옷장방을 찾습니다.',steps=['2층 놀이·휴게실에서 시작해 빈 손님방과 응접실을 살펴봅니다.','홀의 회랑 → 동쪽 연결 복도 → S2 계단으로 이동합니다.','다락 창고 문들을 지나 목재 복도 끝의 옷장방을 발견합니다.']),
 'escape':dict(title='③ 창문·유물 사고',startFloor='ground',text='서쪽 정원에서 친 공이 유물 전시실 창과 갑옷 일부를 망가뜨립니다. 유물을 훼손했다는 두려움에 멈칫한 아이들이, 응접실 쪽 발소리를 듣고 옷장방으로 도망칩니다.',steps=['공은 전시실 서쪽 창으로, 아이들은 응접실의 정원문으로 들어갑니다.','떨어진 투구·팔 장식과 유리 조각을 확인합니다. 돌아갈 쪽에서 발소리가 나자 전시실 동쪽 아치로 빠져나갑니다.','홀 → 동쪽 연결 복도 → S2 → 다락 → 옷장방. 숨바꼭질 때 발견한 곳으로 네 아이가 함께 피신합니다.']),
}
D['storyConstraints']={
 'professorFloor':'upper', 'privateGateway':['upper','B-W','P0'],
 'childrenRooms':['upper/E1','upper/K-BOYS'],
 'childrenGateway':['upper/F2','upper/K0'],
 'noUpperShortcut':['S2','K0'],
 'brokenWindow':dict(floor='ground',room='D1',axis='v',point=[0,18],source='User-requested west garden and artifact gallery adaptation'),
 'lawn':dict(rect=[-10,4,10,24],kind='partial-west-garden-story-area; not a complete estate boundary'),
 'damagedArtifact':dict(room='D1',position=[2.3,18],parts=['helmet','arm ornament'],extent='partial damage; the complete suit does not collapse'),
 'gardenEntry':dict(room='C1',point=[0,7.5]),
 'escapeGroundRooms':['D1','B','F0','S2'],
 'wardrobe':['attic','G2'],
 'escapeCrossFloor':['ground/S2','upper/S2','attic/S2'],
}
D['evidence']=[
 dict(type='user',claim='교수 구역을 가리켜 경고한 뒤 반대 방향의 외진 남녀 침실로 안내하는 장면을 설계 기준으로 삼습니다.'),
 dict(type='transcript',url='https://thescriptsavant.com/movies/Chronicles_Of_Narnia_The_Lion_The_Witch_And_The_Wardrobe.pdf',pages='PDF 3–5, 18–20',note='비공식 전사본. 교수 방해 금지, 숨바꼭질 중 옷장 발견, 크리켓 공과 창·갑옷 사고, 맥크리디를 피해 옷장으로 숨는 사건 순서를 보조 확인했습니다. 정확한 건물 평면은 제공하지 않습니다.'),
 dict(type='difference',note='전사본은 사고 창을 위층으로 설명합니다. 본 검토안의 서쪽 정원·1층 유물 전시실 배치는 사용자 요청을 따른 게임용 제안이며 영화 실측 위치로 표시하지 않습니다.'),
 dict(type='user',claim='서쪽 정원에서 공놀이 → 유물 전시실 창 파손 → 갑옷 일부 파손 → 유물 훼손에 겁먹고 도망치는 흐름으로 수정합니다.'),
]
D['changes']=[
 '교수 서재를 2층 서쪽 날개로 옮기고 개인 침실·서고·전용 복도를 한 구역으로 묶음.',
 '남녀 아이들 방을 동쪽 후면 끝으로 옮기고, 중앙 홀에서 문이 보이지 않는 두 번 꺾이는 접근을 둠.',
 '2층 S2와 침실 복도 사이 문을 없애고 침실 복도 끝을 린넨장으로 마감함.',
 '사고를 서쪽 정원과 1층 유물 전시실로 옮기고, 창 안쪽 갑옷 일부를 파손하는 사건으로 수정함.',
 '응접실의 서쪽 정원문으로 사고를 확인하러 들어온 뒤, 전시실 동쪽 아치 → 홀 → 연결 복도 → S2로 피신하도록 함.',
 '첫 안내·숨바꼭질·창문 사고를 별도 경로로 표시하고 공의 궤적과 아이들 이동을 구분함.',
 '루시의 발견 경로와 이후 네 아이의 재방문 경로가 같은 다락 복도에서 합류하도록 함.',
]
D['limits']=[
 '방 배치와 사건 경로는 사용자 장면 요구를 적용한 게임용 제안입니다. 영화의 확정 평면도가 아닙니다.',
 '도면의 동서 방향은 비교를 위한 임의 방향입니다. 실제 촬영장의 방위를 뜻하지 않습니다.',
 '계단·층고·정원 관계는 검토 04와 층별 검토 01을 기준으로 합니다. 세부 치수와 벽 두께는 후속 모델에서 조정합니다.',
 'Unity, 기존 3D 모델, 플레이 가능한 충돌·카메라·전환은 이번 다운로드 도면에 구현하지 않습니다.',
]
(OUT/'Data/layout.json').write_text(json.dumps(D,ensure_ascii=False,indent=2))
print(json.dumps({f:len(v['rooms']) for f,v in D['floors'].items()}))
