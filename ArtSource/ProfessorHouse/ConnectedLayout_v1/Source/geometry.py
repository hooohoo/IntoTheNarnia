"""Shared native geometry constructors; metres, Z up."""
import math

def box(p,x,y,z,w,d,h,mat='wood',bevel=.018):
    assert min(w,d,h)>0
    p['shapes'].append(dict(type='box',pos=[x,y,z],size=[w,d,h],mat=mat,bevel=bevel))

def beam(p,a,b,r=.08,mat='wood',round=False):
    p['shapes'].append(dict(type='beam',a=list(a),b=list(b),r=r,mat=mat,round=round))

def sphere(p,center,r,mat='wood',scale=None):
    p['shapes'].append(dict(type='sphere',pos=list(center),r=r,mat=mat,scale=scale))

def lathe(p,center,profile,mat='wood',sides=10):
    p['shapes'].append(dict(type='lathe',pos=list(center),profile=profile,mat=mat,sides=sides))

def poly(p,points,axis,line,depth,mat='wood',bevel=.015):
    p['shapes'].append(dict(type='poly',points=points,axis=axis,line=line,depth=depth,mat=mat,bevel=bevel))

def arch(p,axis,line,center,width,spring,rise,depth=.42,thickness=.22,mat='stone'):
    p['shapes'].append(dict(type='arch',axis=axis,line=line,center=center,width=width,
                            spring=spring,rise=rise,depth=depth,thickness=thickness,mat=mat))

def wall_box(p,axis,line,u,z,w,h,depth,mat='wood',bevel=.018):
    if axis=='h': box(p,u,line-depth/2,z,w,depth,h,mat,bevel)
    else: box(p,line-depth/2,u,z,depth,w,h,mat,bevel)

def panel_run(p,axis,line,start,end,base=0,height=1.5):
    if end-start<.14:return
    wall_box(p,axis,line,start,base,end-start,height,.075,'woodInset',.01)
    for z,h,dep in [(base,.16,.15),(base+height-.12,.12,.13),(base+height,.065,.19)]:
        wall_box(p,axis,line,start,z,end-start,h,dep,'wood',.014)
    count=max(1,round((end-start)/.85));width=(end-start)/count
    for i in range(count+1):
        u=start+i*width
        wall_box(p,axis,line,u-.045,base+.15,.09,height-.27,.115,'wood',.012)
    for i in range(count):
        if width>.34:
            wall_box(p,axis,line,start+i*width+.14,base+.27,width-.28,height-.53,.09,'woodLight',.016)

def add_stair_trim(stairpart,trim,runner):
    s=stairpart['shapes'][0];n=s['n'];t=s['tread'];r=s['rise'];w=s['width'];dx,dy=s['direction'];x,y,z=s['origin'];sx,sy=dy,-dx
    def at(u,v,h):return [x+dx*u+sx*v,y+dy*u+sy*v,z+h]
    for v in [0,w]:
        beam(trim,at(0,v,-.04),at(n*t,v,n*r-.04),.12,'wood')
        # Continuous vertical stringer profile, not a row of separate blocks.
        points=[(0,-.22),(0,.13),(n*t,n*r+.13),(n*t,n*r-.22)]
        trim['shapes'].append(dict(type='stairStringer',origin=[x,y,z],direction=[dx,dy],v=v,depth=.17,points=points,mat='wood'))
    rw=max(.75,w*.62)
    for i in range(n):
        # Rounded projecting tread noses.
        a=at(i*t-.022,0,(i+1)*r-.041)
        if dx==0:box(trim,min(a[0],a[0]+sx*w),min(a[1],a[1]+dy*(t+.022)),a[2],w,t+.022,.045,'woodEdge',.012)
        else:box(trim,min(a[0],a[0]+dx*(t+.022)),min(a[1],a[1]+sy*w),a[2],t+.022,w,.045,'woodEdge',.012)
        # Runner follows both treads and risers on every stair branch.
        c=at(i*t,(w-rw)/2,(i+1)*r+.006)
        if dx==0:box(runner,min(c[0],c[0]+sx*rw),min(c[1],c[1]+dy*t),c[2],rw,t,.012,'rug',0)
        else:box(runner,min(c[0],c[0]+dx*t),min(c[1],c[1]+sy*rw),c[2],t,rw,.012,'rug',0)
        for v in [(w-rw)/2+.04,(w+rw)/2-.055]:
            beam(runner,at(i*t,v,(i+1)*r+.022),at((i+1)*t,v,(i+1)*r+.022),.009,'rugGold')
        a=at(i*t-.012,(w-rw)/2,i*r+.006)
        if dx==0:box(runner,min(a[0],a[0]+sx*rw),a[1]-.006,a[2],rw,.012,r,'rug',0)
        else:box(runner,a[0]-.006,min(a[1],a[1]+sy*rw),a[2],.012,rw,r,'rug',0)
