from drawing_base import *
import drawing_base as drawing

# Extend every plan equally toward the west while retaining the same metre scale.
drawing.MX=324
MX=324

COL.update(professor='#D9CCE4', children='#F1D6A7')
TEAL='#287D97'; FOREST='#3B856A'; ORANGE='#C67832'; RED='#B45F56'; PURPLE='#876296'
SCENE_COL={'overview':TEAL,'arrival':TEAL,'hide':FOREST,'escape':ORANGE}

def furniture(floor):
    s='<g pointer-events="none">'
    if floor=='upper':
        for x,y,w,h in [(3.3,1.5,3.2,4),(31,29,2,2.8),(35.8,29,2,2.8),(39.5,29,2,2.8),(44.3,29,2,2.8),(25,1,2.2,3.1)]:
            s+=rect(x,y,w,h,'#B7A283','#967D5C',1)+rect(x+.12,y+.12,w-.24,h-.24,'#FFF8EB','#BDA78A',.7)+rect(x+.22,y+h-.8,w-.44,.5,'#E8DED2','#BDA78A',.7)
        s+=rect(2.4,12.5,3.1,1.5,'#96785D','#6E5644',1)
        for yy in [11,15.2,18.5,23]:s+=rect(.4,yy,.65,2.4,'#AC9678','#887154',.8)
        s+=rect(30.2,26.3,.55,1.4,'#B5966F','#8A7152',1)
        s+=rect(11.85,22.7,.43,1.4,'#AF916B','#846C52',.8)
        s+=rect(48,29.5,1.2,1.8,'#FFFBF5','#BDA78A',1)
    elif floor=='ground':
        s+=rect(27.3,3.2,3.7,2,'#A88D6C','#8C7255',1)
        for x in [27.7,29,30.3]:
            for y in [2.55,5.4]:s+=rect(x,y,.7,.55,'#C5AD89','#8C7255',.8)
        for y in [23,26,29.5]:s+=rect(.5,y,1,2.7,'#B19D7F','#8C7255',.8)
        # The torso stays on its stand; detached helmet and arm ornament show partial damage.
        s+=rect(1.85,17.45,.95,1.1,'#B4B8AF','#727A77',1)
        s+=f'<circle cx="{X(3.4)}" cy="{Y(17.55)}" r="8" fill="#A7A9A3" stroke="#727A77"/>'
        s+=rect(3.7,17.5,.65,.22,'#B4B8AF','#727A77',1)
        s+=txt(X(6.25),Y(18.15),'갑옷 일부 파손',13,'#74766D')
        for x,y in [(7.4,11),(7.4,18.5)]:s+=rect(x,y,1.3,1.1,'#BCA582','#8E785A',1)
    else:
        s+=rect(2.6,31,2.8,1.5,'#79573F','#64432E',2)+path([(4,31),(4,32.5)],'#D0AA72',1.7)
        s+=txt(X(4),Y(30.25),'옷장',16,'#79573F',weight=650)
        for x,y in [(9,25),(10.3,29.8),(21.1,25),(21.4,30)]:s+=rect(x,y,1.2,1.6,'#BBA386','#967D5D',.9)
    return s+'</g>'

def labels(floor):
    s='<g class="room-labels" pointer-events="none">'
    overrides={
      'ground':{'A':(17,6.6),'B':(17,13.7),'C2':(29,8.7),'D1':(5,19.6),'F0':(27,14.3),'D3':(11.2,31),'F6':(38,27.6),'F1':(39,9.55)},
      'upper':{'F0':(27,14.2)},
      'attic':{'G2':(4,27.8),'G1a':(4,17.5),'G1b':(17,17.5),'G1c':(28.7,24.5),'G3':(10.2,32.8),'G4':(22.15,32.8)}}
    for r in D['floors'][floor]['rooms']:
        if r['id']=='S2':continue
        x,y,w,h=r['rect'];cx,cy=r.get('labelPos',overrides.get(floor,{}).get(r['id'],(x+w/2,y+h/2+.3)))
        if r['id'] in ['B-W','B-E','P0','F2']:
            cx=x+w/2 if r['id']!='F2' else x+1.1
            cy={'B-W':14.4,'B-E':14.4,'P0':16,'F2':18}[r['id']]
            col=PURPLE if r['id']=='P0' else '#507A81'
            s+=txt(X(cx),Y(cy),r['name'],13,col,weight=650,extra=f'transform="rotate(-90 {X(cx)} {Y(cy)})"')
            continue
        size=17 if w>=7 else 15 if w>=3 else 12
        if r['id'] in ['B-NW','B-NE']:size=12
        if h<=2:size=13
        lines=r['label'];s+=txt(X(cx),Y(cy),lines,size,weight=650,extra=f'paint-order="stroke" stroke="{COL[r["kind"]]}" stroke-width="3" stroke-linejoin="round"')
    if floor=='upper':s+=txt(X(17),Y(14.2),['홀 상부','2층 바닥 없음'],19,'#9C8B72',weight=600)
    if floor=='attic':
        s+=txt(X(15),Y(7),['아직 내부를 설계하지 않은 지붕 공간','아래층 교수 구역을 통과하는 길이 아닙니다'],18,'#9E917D')
        s+=txt(X(42),Y(5),['지붕 공간','내부 미설계'],16,'#A09482')
        s+=txt(X(40),Y(30),['지붕 공간','내부 미설계'],16,'#A09482')
    return s+'</g>'

NOTES={
 'ground':[(255,['서쪽 정원에서 친 공','전시실 창 → 갑옷','투구·팔 장식 일부 파손.']), (387,['1 · 확인하러 들어옴','서쪽 정원문 → 응접실','→ 유물 전시실.']), (519,['겁이 나는 순간','유물을 망가뜨린 뒤,','돌아갈 쪽에서 발소리.']), (651,['2 · 안쪽 아치로 피신','전시실 → 홀을 지나','동쪽 연결 복도로.']), (805,['3 · 이미 아는 은신처','S2 → 2층 → 다락','목재 복도 끝 옷장방.'])],
 'upper-arrival':[(255,['1 · 안내가 멈추는 곳','계단 위에서 서쪽 문을','가리키며 교수 구역 경고.']), (387,['2 · 반대 방향으로','E3 → 전면 갤러리 →','중정을 따라 긴 회랑.']), (519,['3 · 외진 아이들 방','끝에서 한 번 더 꺾여','홀에서 침실문이 안 보임.']), (651,['침실 복도의 끝','서쪽은 린넨장과 끝벽.','S2로 바로 통하지 않음.']), (805,['보라색은 교수 구역','서재·침실·서고를','전용 복도로 묶었습니다.'])],
 'upper-hide':[(255,['1 · 놀이방에서 시작','빈 손님방·응접실을','숨을 곳으로 살펴봅니다.']), (387,['2 · 회랑을 탐색','녹색 실선: 주 이동','녹색 점선: 방 탐색 갈래.']), (519,['3 · 보조 계단 발견','동쪽 연결 복도 끝의','S2를 통해 다락으로.']), (651,['금지 구역은 통과 안 함','서쪽 교수 구역에 들어갈','필요가 없는 탐험 동선.']), (805,['침실과 옷장방 분리','침실에서 문 하나로','곧바로 옷장에 닿지 않음.'])],
 'upper-escape':[(255,['1층에서 계속 위로','S2의 중간 층입니다.','방으로 빠지지 않습니다.']), (387,['다락으로 이어짐','다락 탭을 누르면','이어지는 경로를 봅니다.']), (519,['이제는 아는 길','숨바꼭질에서 발견한','계단·복도로 피신합니다.']), (651,['교수 구역과 분리','교수 서재·개인 복도는','피신 경로에 없습니다.']), (805,['아이들 방과 분리','침실 복도와 S2 사이','직통문을 두지 않았습니다.'])],
 'upper-overview':[(255,['서쪽 · 교수의 공간','서재·침실·개인 서고.','경계문으로 구분합니다.']), (387,['동쪽 끝 · 아이들','남녀 침실과 욕실을','한 조용한 구역에 배치.']), (519,['가운데 · 공용 공간','홀·회랑·빈 손님방은','탐험할 수 있는 범위.']), (651,['S2 · 다락으로','침실과 떨어진 공용','연결 복도에서 접근.']), (805,['후정은 계속 비움','창 뒤로 정원이 보이고','가로지르는 복도는 없음.'])],
 'attic':[(255,['1 · 같은 계단 도착','S2의 위치는 세 층에서','똑같이 겹쳐집니다.']), (387,['2 · 돌아서 탐색','창고 문들을 지나며','목재 복도를 걷습니다.']), (519,['3 · 옷장방 발견','끝에서 꺾어 방문 통과.','입구에서 옷장까지 이동.']), (651,['발견 후 다시 방문','숨바꼭질: 처음 발견','창문 사고: 함께 재방문.']), (805,['정원 위는 계속 비움','홀 천장 위 복도를 이용.','교수 구역에는 안 들어감.'])],
}

def notes(floor,scene):
    key=f'upper-{scene}' if floor=='upper' else floor
    s='<rect x="1560" y="184" width="230" height="816" rx="12" fill="#F2EBDD"/>'
    s+=txt(1580,219,'장면과 공간 읽기',19,anchor='start',weight=700)
    for yy,lines in NOTES[key]:
        s+=txt(1580,yy,lines[0],17,anchor='start',weight=700)
        s+=txt(1580,yy+32,lines[1:],14.5,'#80715D',anchor='start')
    return s

def routes(floor,scene):
    s='<g class="routes" pointer-events="none">'
    for r in D['floors'][floor]['routes']:
        if r['scene']!=scene:continue
        color=SCENE_COL[scene];dash=''
        if r['kind']=='point':color=PURPLE;dash='4 4'
        if r['kind']=='adult':color=RED
        if r['kind']=='ball':color=ORANGE;dash='4 5'
        if r.get('phase')=='entry':color=TEAL;dash='5 6'
        if r.get('phase')=='explore':dash='5 6'
        key={TEAL:'blue',PURPLE:'purple',RED:'red',ORANGE:'orange',FOREST:'forest'}[color]
        s+=path(r['points'],'#FCF8EF',8)+path(r['points'],color,4,dash,extra=f'marker-end="url(#{floor}-{key})"')
        for a,b in zip(r['points'],r['points'][1:]):
            if math.dist(a,b)>7:
                u=[aa*.55+bb*.45 for aa,bb in zip(a,b)];v=[aa*.45+bb*.55 for aa,bb in zip(a,b)]
                s+=path([u,v],color,4,extra=f'marker-end="url(#{floor}-{key})"')
    badges=[]
    if floor=='upper' and scene=='arrival':badges=[(11,19,'1'),(37,8,'2'),(47,27,'3')]
    if floor=='upper' and scene=='hide':badges=[(39,3,'1'),(23.35,17,'2'),(27,25,'3')]
    if floor=='ground' and scene=='escape':badges=[(-.5,18,'!'),(1.8,7.5,'1'),(14,14,'2'),(27,27,'3')]
    if floor=='attic' and scene in ['hide','escape']:badges=[(27,24,'1'),(17,19,'2'),(4,24,'3')]
    for x,y,n in badges:s+=badge(x,y,n,SCENE_COL[scene])
    return s+'</g>'

def floor_svg(floor,scene):
    data=D['floors'][floor]
    s=f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1840 1320" width="1840" height="1320" role="img" aria-label="{data["title"]}" data-floor-map="{floor}" data-scene-map="{scene}"><style>text{{font-family:-apple-system,BlinkMacSystemFont,\'Apple SD Gothic Neo\',sans-serif}}.zone{{cursor:pointer}}.zone:focus .zone-fill,.zone:hover .zone-fill{{stroke:#576E6B;stroke-width:4}}.zone.selected .zone-fill{{stroke:#267B8D;stroke-width:5}}.stair-focus .zone:not(.stair-zone){{opacity:.45}}</style><defs>'
    s+=f'<pattern id="{floor}-hatch" width="12" height="12" patternUnits="userSpaceOnUse"><path d="M0,12 L12,0" stroke="#C9BEAC" stroke-width=".8"/></pattern>'
    for key,color in [('blue',TEAL),('gold',GOLD),('purple',PURPLE),('red',RED),('orange',ORANGE),('forest',FOREST)]:
        s+=f'<marker id="{floor}-{key}" markerUnits="userSpaceOnUse" markerWidth="13" markerHeight="13" refX="11" refY="6.5" orient="auto"><path d="M0 0 L13 6.5 L0 13 Z" fill="{color}"/></marker>'
    s+='</defs><rect width="1840" height="1320" fill="#FCF8EF"/>'
    s+=txt(84,46,'PROFESSOR’S HOUSE / FLOOR PLAN REVIEW 03 · WEST GARDEN',14,'#9C7E58',anchor='start',weight=600)
    s+=txt(84,95,data['title'],32,anchor='start',weight=750)
    s+=txt(84,137,data['subtitle'],19,'#86735C',anchor='start')
    s+=txt(1755,73,f'+{data["height"]:.2f} m',24,TEAL,anchor='end',weight=700)
    s+=txt(1755,111,D['scenes'][scene]['title'],18,SCENE_COL[scene],anchor='end',weight=700)
    s+=txt(X(25),165,'↑ 후면 · 뒤뜰 쪽',17,'#89775F',weight=600)
    if floor=='attic':
        for r in D['footprint']:s+=rect(*r,'#F1EBDE','#BEB2A0',1.5)+rect(*r,f'url(#{floor}-hatch)','none',0)
    if floor=='upper':s+=rect(10,10,14,12,'#F4EEE2')+rect(10,10,14,12,f'url(#{floor}-hatch)','none',0)
    for g in D['gardens']:
        x,y,w,h=g['rect'];s+=rect(x,y,w,h,'#D9E5C3','#90A370',2)
        if g['openRear']:s+=path([(x,y+h),(x+w,y+h)],'#D9E5C3',7)
        s+=txt(X(x+w/2),Y(y+h*.62),[g['name'] if floor=='ground' else '아래 '+g['name'],'하늘로 열린 공간'],20,'#55723A',weight=650)
        if g['id']=='GARDEN':s+=txt(X(x+w/2),Y(23.35),'아래층 아치창' if floor=='attic' else '아치창 정면',14,'#66804D')
        if floor!='ground':s+=txt(X(x+w/2),Y(y+h*.4),'이 층의 바닥·지붕 없음',13,'#74895C')
    s+=rect(*D['storyConstraints']['lawn']['rect'],'#E4ECCF','#AABB92',1)
    s+=path([(-10,4),(-10,28)],'#E4ECCF',6)
    s+=txt(X(-5),Y(26.5),['서쪽 정원' if floor=='ground' else '아래 서쪽 정원','공놀이' if floor=='ground' else '지상 야외'],19,'#66804D',weight=650)
    s+=txt(X(-5),Y(24.1),'정원 범위 일부',13,'#8B9A73')
    if floor=='ground':s+=txt(X(-7),Y(19.2),'공',14,ORANGE,weight=650)
    for r in data['rooms']:
        s+=f'<g class="zone {"stair-zone" if r["kind"]=="stair" else ""}" role="button" tabindex="0" data-room="{r["id"]}" aria-label="{escape(r["name"])}"><title>{escape(r["name"])} · {escape(r["note"])}</title>'+rect(*r['rect'],COL[r['kind']],extra='class="zone-fill"')+'</g>'
    s+=furniture(floor)+main_stair(floor)+secondary_stair(floor)+doors(floor)
    if floor=='upper':
        s+=path([(10,18.15),(10,19.85)],PURPLE,4)
        s+=path([(30,26),(30,28)],WALL,5)
    s+=routes(floor,scene)+labels(floor)+notes(floor,scene)
    s+=txt(X(25),1045,'↓ 현관 · 저택의 앞쪽',18,'#89775F',weight=600)
    s+=path([(-10,-3),(0,-3)],'#8C7B64',3)
    for x in [0,5,10]:s+=path([(x-10,-2.8),(x-10,-3.2)],'#8C7B64',2)+txt(X(x-10),Y(-4),f'{x} m',13,'#8C7B64')
    s+=txt(380,1092,['동일 방향·축척 · 건물 외곽 50 × 34 m','문 위치·경로는 설계 제안'],15,'#8F7E67',anchor='start')
    if floor=='ground' and scene=='escape':s+=txt(1580,1060,['점선은 공 또는 진입','실선은 피신 동선','붉은 선은 어른 접근'],14,'#8F7E67',anchor='start')
    legend=[('교수 전용',COL['professor']),('아이들 구역',COL['children']),('공용 공간',COL['social']),('복도·회랑',COL['hub']),('정원·중정','#D9E5C3')]
    for i,(label,color) in enumerate(legend):
        xx=84+i*245;s+=f'<rect x="{xx}" y="1195" width="18" height="18" rx="4" fill="{color}" stroke="#B9AD9A"/>'+txt(xx+28,1210,label,16,'#796A56',anchor='start')
    s+=txt(84,1265,'검토용 게임 설계안 · 영화 실측 도면 아님 · 경로는 장면 연출 제안 · Unity 반영 전',15,'#97866D',anchor='start')
    s+=txt(84,1293,'청록색 짧은 선은 창, 갈색 문호는 출입구입니다. 이동선은 계단이 있는 층에서만 다음 층으로 이어집니다.',13,'#97866D',anchor='start')
    return s+'</svg>'

def story_svg():
    s='<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1600 1320" width="1600" height="1320"><style>text{font-family:-apple-system,BlinkMacSystemFont,"Apple SD Gothic Neo",sans-serif}</style><rect width="1600" height="1320" fill="#FCF8EF"/>'
    s+=txt(84,50,'PROFESSOR’S HOUSE / THREE SCENES, ONE HOUSE',14,'#9C7E58',anchor='start',weight=600)
    s+=txt(84,108,'처음 안내받은 집을 탐험하고, 아는 길로 피신합니다',34,anchor='start',weight=750)
    s+=txt(84,153,'아래 흐름은 장면을 위한 동선 제안입니다. 각 단계의 위치는 층별 도면과 같은 이름을 사용합니다.',19,'#8D7B63',anchor='start')
    rows=[
      (235,'① 첫 안내',TEAL,[['2층 계단 도착','서측 회랑'],['교수 구역 경고','문을 가리키기만 함'],['E3 → 동측 회랑','반대편으로 돌아감'],['동쪽 후면 침실','남녀 방·욕실']], '교수 전용문은 가까이 보이고, 아이들 방은 두 번 꺾이는 긴 접근 끝에 나타납니다.'),
      (535,'② 숨바꼭질',FOREST,[['2층 놀이방','빈 방도 살펴봄'],['회랑 → 보조 계단','교수 구역은 안 거침'],['다락 목재 복도','창고 문을 지나감'],['끝의 옷장방','루시가 처음 발견']], '침실 앞에서 옷장이 바로 드러나지 않습니다. 방과 복도를 탐색한 끝에 발견합니다.'),
      (835,'③ 창문·유물 사고',ORANGE,[['서쪽 정원 → 전시실','창·갑옷 일부 파손'],['파손 확인 뒤 발소리','겁이 나 안쪽 아치로'],['홀 → 연결 복도 → S2','1층 → 2층 → 다락'],['다시 옷장방으로','네 아이가 함께 이동']], '유물을 망가뜨렸다는 두려움이 먼저 생기고, 돌아갈 쪽에서 들리는 발소리가 피신의 계기가 됩니다.'),
    ]
    for y,title,col,boxes,caption in rows:
        s+=txt(84,y,title,26,col,anchor='start',weight=750)
        for i,lines in enumerate(boxes):
            x=84+i*368
            s+=f'<rect x="{x}" y="{y+28}" width="326" height="132" rx="14" fill="#F3EEE3" stroke="{col}" stroke-width="2"/>'
            s+=txt(x+163,y+79,lines[0],22,col,weight=700)+txt(x+163,y+119,lines[1],18,'#87765E')
            if i<3:s+=txt(x+346,y+106,'→',27,col,weight=700)
        s+=txt(84,y+201,caption,18,'#87765E',anchor='start')
    s+=txt(84,1185,'설계 기준',21,anchor='start',weight=750)
    s+=txt(84,1225,['교수의 서재는 2층 전용 구역 · 아이들 침실은 외진 동쪽 끝 · 옷장방은 다른 층의 탐험 끝점',
                        '사고 창의 1층 배치는 사용자 요청을 적용한 안입니다. 비공식 영화 전사본의 위층 표기와 구분합니다.'],18,'#8F7E67',anchor='start')
    return s+'</svg>'

maps={f:{scene:floor_svg(f,scene) for scene in D['scenes']} for f in D['floors']}
exports={'ground':maps['ground']['escape'],'upper':maps['upper']['arrival'],'attic':maps['attic']['hide'],
         'hide_upper':maps['upper']['hide'],'story':story_svg(),'section':section_svg()}
for name,svg in exports.items():(OUT/'Plans'/f'{name}.svg').write_text(svg)
(OUT/'Data/maps.js').write_text('window.FLOOR_MAPS='+json.dumps(maps,ensure_ascii=False)+';\nwindow.HOUSE_LAYOUT='+json.dumps(D,ensure_ascii=False)+';')
print('WROTE_SIX_DIAGRAMS')
