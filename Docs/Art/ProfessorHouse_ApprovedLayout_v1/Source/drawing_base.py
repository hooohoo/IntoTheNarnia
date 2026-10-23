from pathlib import Path

from html import escape

import json

import math

OUT=Path(__file__).resolve().parent.parent

D=json.loads((OUT/'Data/layout.json').read_text())

S=24;MX=84;MY=184

S=24;MX=84;MY=184

S=24;MX=84;MY=184

COL={'social':'#E7D6BE','private':'#DBD4E4','hub':'#D5E7E9','hall':'#EEE3CD','service':'#E5E0D5','storage':'#DFD2BD','stair':'#E6C59B','nook':'#E7D0A7','story':'#D8BF96'}

BLUE='#237D91';GOLD='#B47A31';INK='#493E33';WALL='#8C7B64';GREEN='#6E884D'

BLUE='#237D91';GOLD='#B47A31';INK='#493E33';WALL='#8C7B64';GREEN='#6E884D'

BLUE='#237D91';GOLD='#B47A31';INK='#493E33';WALL='#8C7B64';GREEN='#6E884D'

BLUE='#237D91';GOLD='#B47A31';INK='#493E33';WALL='#8C7B64';GREEN='#6E884D'

BLUE='#237D91';GOLD='#B47A31';INK='#493E33';WALL='#8C7B64';GREEN='#6E884D'

def X(x):return MX+x*S

def Y(y):return MY+(34-y)*S

def txt(x,y,lines,size=18,fill=INK,anchor='middle',weight=500,extra=''):
    if not isinstance(lines,(list,tuple)):lines=[str(lines)]
    return f'<text x="{x:.2f}" y="{y:.2f}" text-anchor="{anchor}" font-size="{size}" font-weight="{weight}" fill="{fill}" {extra}>'+''.join(f'<tspan x="{x:.2f}" dy="{0 if i==0 else size*1.35}">{escape(str(t))}</tspan>' for i,t in enumerate(lines))+'</text>'

def rect(x,y,w,d,fill,stroke=WALL,sw=1.8,extra=''):
    return f'<rect x="{X(x):.2f}" y="{Y(y+d):.2f}" width="{w*S:.2f}" height="{d*S:.2f}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" {extra}/>'

def path(points,color,width=3,dash='',extra=''):
    return '<polyline points="'+' '.join(f'{X(x):.2f},{Y(y):.2f}' for x,y in points)+f'" fill="none" stroke="{color}" stroke-width="{width}" stroke-linejoin="round" stroke-linecap="round"'+(f' stroke-dasharray="{dash}"' if dash else '')+f' {extra}/>'

def badge(x,y,label,color):
    return f'<circle cx="{X(x)}" cy="{Y(y)}" r="17" fill="{color}" stroke="#FFFBF2" stroke-width="3"/>'+txt(X(x),Y(y)+5,label,16,'white',weight=700)

def main_stair(floor):
    if floor=='attic':return ''
    s='<g class="stairs" pointer-events="none">'
    s+=rect(15.4,19.35,3.2,2.5,'#D9B990')
    for x,y,w,d,n,axis in [(16,16.35,2,3,10,'y'),(11.48,19.35,3.92,2.5,14,'x'),(18.6,19.35,2.24,1.6,8,'x'),(20.84,20.95,1.6,1.68,6,'y')]:
        s+=rect(x,y,w,d,'#C3AA87','#8D775A',1)
        for i in range(1,n):
            points=[(x+i*w/n,y),(x+i*w/n,y+d)] if axis=='x' else [(x,y+i*d/n),(x+w,y+i*d/n)]
            s+=path(points,'#8D775A',.8)
    s+=rect(20.84,19.35,1.6,1.6,'#D9B990','#8D775A',1)
    s+=txt(X(17),Y(20.45),['계단참','+1.80'],12,'#66513B',weight=600)
    s+=txt(X(17),Y(15.85),'↑ 2층' if floor=='ground' else '↓ 1층',15,'#93672F',weight=700)
    return s+'</g>'

def secondary_stair(floor):
    n=12 if floor=='ground' else 13;t=.28;ytop=27+n*t
    s='<g class="stairs" pointer-events="none">'
    for x in [25,26.7]:
        s+=rect(x,27,1.4,n*t,'#C1A17A','#8C7255',1)
        for i in range(1,n):s+=path([(x,27+i*t),(x+1.4,27+i*t)],'#8C7255',.8)
    s+=rect(25,ytop,3.1,1.5,'#DBC19C','#8C7255',1)
    points=[(25.7,27.2),(25.7,ytop+.7),(27.4,ytop+.7),(27.4,27.2)]
    if floor=='attic':points.reverse()
    s+=path(points,'#9A6425',2,extra=f'marker-end="url(#{floor}-gold)"')
    s+=txt(X(27),Y(33.25),['S2 · 보조 계단'],14,'#704D27',weight=700)
    s+=txt(X(27),Y(26.6),{'ground':'↑ 2층','upper':'↑ 다락 / ↓ 1층','attic':'↓ 2층'}[floor],13,'#8F612F',weight=700)
    return s+'</g>'

def doors(floor):
    s='<g class="doors" pointer-events="none">'
    for d in D['floors'][floor]['doors']:
        x,y=d['point'];w=d['width'];axis=d['axis'];start=(x-w/2,y) if axis=='h' else (x,y-w/2);end=(x+w/2,y) if axis=='h' else (x,y+w/2)
        s+=path([start,end],'#FFFBF2',7)
        if d['kind']=='arch':
            s+=path([start,end],'#B8A17F',2,'3 3');continue
        # Door symbol opens toward the room side; short swing is schematic.
        tip=(start[0],start[1]+w) if axis=='h' else (start[0]+w,start[1])
        s+=path([start,tip],WALL,2)
        s+=f'<path d="M {X(end[0])} {Y(end[1])} A {w*S} {w*S} 0 0 0 {X(tip[0])} {Y(tip[1])}" fill="none" stroke="{WALL}" stroke-width=".9"/>'
    for w in D['floors'][floor]['windows']:
        x,y=w['point'];half=w['width']/2
        pts=[(x-half,y),(x+half,y)] if w['axis']=='h' else [(x,y-half),(x,y+half)]
        s+=path(pts,'#649BA4',5.2)
    if floor!='attic':
        s+=path([(15.9,22),(18.1,22)],'#3B93A1',7)
    return s+'</g>'

def section_svg():
    s='<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1600 1240" width="1600" height="1240" role="img" aria-label="1층, 2층, 다락과 높은 계단 홀의 관계"><style>text{font-family:-apple-system,BlinkMacSystemFont,"Apple SD Gothic Neo",sans-serif}</style><rect width="1600" height="1240" fill="#FCF8EF"/>'
    s+=txt(84,52,'PROFESSOR’S HOUSE / FLOOR RELATION',14,'#9C7E58',anchor='start',weight=600)
    s+=txt(84,105,'2층은 홀의 양옆에, 다락 복도는 홀의 천장 위에',35,anchor='start',weight=750)
    s+=txt(84,148,'층 관계를 이해하기 위한 개념 단면입니다. 가로 길이는 축척을 맞추지 않았습니다.',19,'#8D7B63',anchor='start')
    bottom=880;scale=40
    Z=lambda z:bottom-z*scale
    # Front rooms, tall hall and open garden are separate vertical volumes.
    s+=f'<rect x="275" y="{Z(4.32)}" width="300" height="{4.32*scale}" fill="#E7D6BE" stroke="#8C7B64" stroke-width="2"/>'
    s+=f'<rect x="275" y="{Z(9)}" width="300" height="{(9-4.32)*scale}" fill="#DBD4E4" stroke="#8C7B64" stroke-width="2"/>'
    s+=f'<rect x="575" y="{Z(8.84)}" width="390" height="{8.84*scale}" fill="#EEE3CD" stroke="#8C7B64" stroke-width="2"/>'
    s+=f'<path d="M275 {Z(9)} L275 {Z(11.85)} L640 {Z(13.8)} L965 {Z(11.85)} L965 {Z(9)} Z" fill="#EEE7DB" stroke="#8C7B64" stroke-width="3"/>'
    s+=f'<rect x="675" y="{Z(11.4)}" width="290" height="{2.4*scale}" fill="#D5E7E9"/>'
    s+=f'<rect x="575" y="{Z(9)}" width="390" height="{.16*scale}" fill="#7D6850"/>'
    s+=f'<rect x="965" y="290" width="430" height="590" fill="#E2EBCF"/>'
    s+=f'<line x1="965" x2="965" y1="{Z(2.6)}" y2="{Z(8.0)}" stroke="#4693A0" stroke-width="10"/>'
    # The split landing and common flight in the lower hall.
    s+=f'<rect x="871" y="{Z(1.8)}" width="94" height="{1.8*scale}" fill="#BD9B76" stroke="#917859"/>'
    for i in range(10):s+=f'<rect x="{731+i*14}" y="{Z((i+1)*.18)}" width="14" height="{(i+1)*.18*scale}" fill="#C7AB87" stroke="#917859" stroke-width=".7"/>'
    s+=f'<path d="M 575 {Z(4.32)} H 627" stroke="#607F83" stroke-width="9"/>'
    s+=txt(425,Z(2.2),['1층','현관·응접·식사'],25,weight=700)
    s+=txt(425,Z(6.9),['2층','서재·침실·손님방'],25,weight=700)
    s+=txt(780,Z(6.9),['높은 계단 홀','2층 중앙에 바닥 없음'],24,'#8B775E',weight=650)
    s+=txt(815,Z(10.6),['다락 복도','홀 천장 위로 이동'],21,'#3F727B',weight=700)
    s+=txt(1180,470,['후정 · 정원','위로 열린 야외'],28,'#55723A',weight=700)
    s+=txt(1180,555,['2층·다락 높이에도','복도나 지붕을 놓지 않음'],20,'#708754')
    s+=txt(1000,746,'← 아치창',19,'#3F8390',anchor='start',weight=650)
    s+=txt(920,835,'계단참 +1.80 m',15,'#7E6042')
    for label,z,color in [('다락 +9.00 m',9,BLUE),('2층 +4.32 m',4.32,BLUE),('1층 ±0.00 m',0,'#8C7B64')]:
        y=Z(z);s+=f'<line x1="84" y1="{y}" x2="255" y2="{y}" stroke="{color}" stroke-width="2"/>'+txt(84,y-12,label,21,color,anchor='start',weight=700)
    s+=txt(635,Z(9)+33,'홀 천장 +8.84 m',16,'#7E6C55',anchor='start')
    s+=txt(710,260,['지붕 형태·높이는 설명용','다락의 실제 유효 높이는 후속 모델에서 검토'],16,'#9A8971')
    s+=txt(84,990,'계단 연결',23,anchor='start',weight=700)
    s+=txt(84,1032,['주 계단: 1층 → 계단참 +1.80 → 좌우 회랑 +4.32','보조 계단 S2: 1층 → 2층 → 다락. 평면 위치는 세 층 모두 동일.'],20,'#786952',anchor='start')
    s+=txt(84,1132,['다락의 옷장방은 서쪽 건물에 있습니다. 위 그림은 층의 높이 관계를 설명하며, 모든 방을 한 단면에 모아 놓은 것은 아닙니다.',
                       '층고는 검토 04의 제작 제안치를 사용했습니다. 영화 실측이나 실제 시공 구조를 확정한 도면은 아닙니다.'],17,'#9A8971',anchor='start')
    return s+'</svg>'
