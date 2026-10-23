"""Rebuild the rounded Professor's House v2 source meshes.

Blender 5.1: blender --background --factory-startup --python this_file -- --project PATH
Unity coordinates are used in this file (metres, Y up). No external assets required.
The scene builder adds gameplay, simple colliders, navigation and Unity materials.
"""
import argparse
import json
import math
import os
import random
import sys
from pathlib import Path

import bpy
import bmesh
from mathutils import Vector

args = argparse.ArgumentParser()
args.add_argument('--project', required=True)
opts = args.parse_args(sys.argv[sys.argv.index('--') + 1:])
project = Path(opts.project)
out = project / 'Assets/@Project/10.Models/ProfessorHouse'
source = project / 'ArtSource/ProfessorHouse'
out.mkdir(parents=True, exist_ok=True)
source.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
random.seed(21)

palette = {
    'Walnut': ('35231B', .28), 'WalnutEdge': ('463024', .32),
    'WalnutPanel': ('402C21', .24), 'Plaster': ('D5C19D', .08),
    'Limestone': ('BAAB90', .12), 'StoneLight': ('CCBDA3', .14),
    'StoneWarm': ('B7A187', .13), 'Mortar': ('A99B84', .08),
    'RugRed': ('984E3C', .08), 'RugBorder': ('C29456', .10),
    'RugInset': ('B06A4D', .08), 'Olive': ('68705A', .13),
    'Brass': ('AF8951', .42), 'Ivory': ('E2D8BD', .18),
    'Ink': ('303C3D', .10), 'Skin': ('C69270', .16),
    'BookBlue': ('526D70', .12), 'BookRed': ('793B34', .12),
    'Leaves': ('56684B', .10), 'Glow': ('FFD389', .0),
    'Sky': ('B5CFCD', .0), 'DistantGreen': ('80947A', .0)
}
materials = {}
for name, (hexcolor, smoothness) in palette.items():
    col = tuple(int(hexcolor[i:i+2], 16) / 255 for i in (0, 2, 4))
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*col, 1)
    mat.use_nodes = True
    bs = mat.node_tree.nodes.get('Principled BSDF')
    bs.inputs['Base Color'].default_value = (*col, 1)
    bs.inputs['Roughness'].default_value = 1 - smoothness
    materials[name] = mat

groups = {}
group = 'Shell'
# Unity's FBX importer changes handedness by reflecting X.
def xyz(p): return Vector((-p[0], -p[2], p[1]))
def finish(obj, mat, bevel=0):
    obj.data.materials.append(materials[mat])
    if bevel:
        mod = obj.modifiers.new('Soft crafted edge', 'BEVEL')
        mod.width = bevel
        mod.segments = 2
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
    groups.setdefault(group, []).append(obj)
    return obj

def box(pos, size, mat='Walnut', bevel=.025, name=''):
    bpy.ops.mesh.primitive_cube_add(size=1, location=xyz(pos))
    obj = bpy.context.object
    obj.name = name or mat
    obj.dimensions = (size[0], size[2], size[1])
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, mat, min(bevel, min(size) * .24))

def sphere(pos, size, mat='Walnut', subdivisions=2):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdivisions, radius=1, location=xyz(pos))
    obj = bpy.context.object
    obj.scale = (size[0], size[2], size[1])
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, mat)

def bar(a, b, width, depth=None, mat='WalnutEdge', bevel=.025):
    delta = xyz(b) - xyz(a)
    obj = box((0, 0, 0), (width, delta.length, depth or width), mat, bevel)
    obj.location = (xyz(a) + xyz(b)) * .5
    obj.rotation_mode = 'QUATERNION'
    obj.rotation_quaternion = Vector((0,0,1)).rotation_difference(delta)
    return obj

def lathe(pos, profile, mat='WalnutEdge', sides=8):
    verts, faces = [], []
    for height, radius in profile:
        for k in range(sides):
            ang = 2 * math.pi * k / sides
            verts.append(xyz((pos[0] + radius * math.cos(ang), pos[1] + height, pos[2] + radius * math.sin(ang))))
    for j in range(len(profile)-1):
        for k in range(sides):
            a = j*sides+k; b=j*sides+(k+1)%sides
            faces.append((a,b,b+sides,a+sides))
    faces.append(tuple(reversed(range(sides))))
    faces.append(tuple((len(profile)-1)*sides+k for k in range(sides)))
    mesh=bpy.data.meshes.new('Turned profile'); mesh.from_pydata(verts,[],faces); mesh.update()
    bm=bmesh.new(); bm.from_mesh(mesh); bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.to_mesh(mesh); bm.free()
    obj=bpy.data.objects.new('Turned profile',mesh); bpy.context.collection.objects.link(obj)
    return finish(obj,mat)

def flat_mesh(name, verts, faces, mat):
    mesh = bpy.data.meshes.new(name); mesh.from_pydata([xyz(v) for v in verts],[],[tuple(reversed(f)) for f in faces]); mesh.update()
    obj=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(obj)
    return finish(obj,mat)

def post(x,y,z, height=1.28):
    box((x,y+height*.44,z),(.34,height*.88,.34), 'Walnut', .045)
    box((x,y+.09,z),(.44,.18,.44), 'WalnutEdge', .025)
    box((x,y+height-.20,z),(.45,.14,.45), 'WalnutEdge', .035)
    box((x,y+height-.08,z),(.36,.10,.36), 'WalnutPanel', .02)
    sphere((x,y+height+.13,z),(.22,.22,.22), 'WalnutEdge', 2)

def railing(a,b, count=None):
    a,b=Vector(a),Vector(b)
    n=count or max(2, int((b-a).length/.46))
    bar(a+Vector((0,.18,0)),b+Vector((0,.18,0)),.19,.18,'Walnut',.025)
    bar(a+Vector((0,1.05,0)),b+Vector((0,1.05,0)),.23,.28,'WalnutEdge',.045)
    profile=[(0,.085),(.11,.085),(.14,.055),(.30,.055),(.43,.105),(.55,.09),(.64,.048),(.81,.048),(.85,.08)]
    for i in range(1,n):
        p=a+(b-a)*(i/n)
        lathe((p.x,p.y+.20,p.z), profile)

def rug(x,y,z,w,d):
    box((x,y,z),(w,.035,d),'RugRed',.018)
    for off,thick,mat in [(.12,.07,'RugBorder'),(.28,.035,'RugBorder'),(.40,.13,'RugInset')]:
        box((x,y+.024,z-d/2+off),(w-2*off,.007,thick),mat,.002)
        box((x,y+.024,z+d/2-off),(w-2*off,.007,thick),mat,.002)
        box((x-w/2+off,y+.024,z),(thick,.007,d-2*off),mat,.002)
        box((x+w/2-off,y+.024,z),(thick,.007,d-2*off),mat,.002)
    for zz in [-d*.22,0,d*.22]:
        r=.25 if zz else .42
        flat_mesh('Woven diamond',[(x,y+.030,z+zz-r),(x+r,y+.030,z+zz),(x,y+.030,z+zz+r),(x-r,y+.030,z+zz)],[(3,2,1,0)],'RugBorder')

# Floor stones are joined in spatial sections, not individual renderers.
for cx in range(2):
    for cz in range(2):
        group=f'Ground_Flagstones_{cx}_{cz}'
        box((-3+cx*6,-.12,-3.5+cz*7),(6,.28,7),'Mortar',.02)
        for ix in range(8):
            for iz in range(9):
                x=-6+cx*6+(ix+.5)*.75
                z=-7+cz*7+(iz+.5)*7/9
                box((x,-.025,z),(.738,.10,7/9-.012),random.choice(['Limestone','Limestone','StoneLight','StoneWarm']),.016)

# Upper L-shaped gallery and broad supporting beams.
group='Gallery_Back'
box((0,3.43,5.55),(12,.34,2.9),'Walnut',.045)
box((0,3.38,4.08),(12,.43,.28),'WalnutEdge',.045)
for i in range(24): box((-5.75+i*.5,3.615,5.55),(.485,.07,2.9),'WalnutPanel',.012)
group='Gallery_Left'
box((-4.75,3.43,-1.45),(2.5,.34,11.1),'Walnut',.045)
box((-3.51,3.38,-1.45),(.30,.43,11.1),'WalnutEdge',.045)
for i in range(22): box((-4.75,3.615,-6.75+i*.5),(2.5,.07,.485),'WalnutPanel',.012)
group='Gallery_Supports'
for x,z in [(-3.65,3.95),(-3.65,-1.0),(-3.65,-6.6),(4.35,4.15)]:
    box((x,1.7,z),(.38,3.4,.38),'Walnut',.04)
    box((x,.15,z),(.52,.30,.52),'WalnutEdge',.035)
    box((x,3.20,z),(.65,.22,.65),'WalnutEdge',.035)
    bar((x,2.6,z),(x+.65,3.25,z),.20,.27,'Walnut',.025)

# Wall segments are consciously large connected units. Door voids are real openings.
def back_wall_segment(x,width,center_y,height, z=7.1):
    box((x,center_y,z),(width,height,.24),'Plaster',.035)

group='Wall_Back'
back_wall_segment(0,12,1.8,3.6)
for x,w in [(-5.15,1.7),(-1.65,1.5),(3.45,5.1)]: back_wall_segment(x,w,5.6,4.0)
for x in [-3.3,0]: back_wall_segment(x,1.8,7.05,1.1)
for y in [.15,1.55,3.0,3.72,5.05,7.3]:
    # Upper trim stops at the openings.
    if y in [3.72,5.05]:
        for x,w in [(-5.15,1.7),(-1.65,1.5),(3.45,5.1)]: box((x,y,6.93),(w,.16,.14),'WalnutEdge',.022)
    else: box((0,y,6.93),(12,.19,.18),'WalnutEdge',.028)
for x in [-5.8,-4.3,-2.3,-1.0,1.0,2.8,4.6,5.8]:
    box((x,3.7,6.88),(.22,7.4,.25),'Walnut',.028)
for ix in range(12):
    x=-5.5+ix
    box((x,.82,6.96),(.95,1.3,.11),'WalnutPanel',.02)
    box((x,.81,6.87),(.73,1.02,.10),'Walnut',.035)
    box((x,.81,6.79),(.59,.85,.045),'WalnutPanel',.025)
    if not (-4.2 < x < -2.4 or -.9 < x < .9):
        box((x,4.38,6.96),(.95,1.28,.11),'WalnutPanel',.02)
        box((x,4.38,6.87),(.69,.96,.10),'Walnut',.025)

group='Wall_Left'
box((-6.12,3.8,0),(.24,7.6,14.2),'Plaster',.035)
for y in [.14,1.55,3.0,3.73,5.05,7.3]: box((-5.93,y,0),(.17,.20,14),'WalnutEdge',.025)
for iz in range(14):
    z=-6.5+iz
    for y in [.82,4.38]:
        box((-5.97,y,z),(.12,1.28,.95),'WalnutPanel',.025)
        box((-5.87,y,z),(.10,.98,.71),'Walnut',.025)
        box((-5.80,y,z),(.04,.80,.55),'WalnutPanel',.018)
for z in [-6.7,-4.5,-2.2,0,2.2,4.5,6.7]: box((-5.86,3.75,z),(.27,7.5,.23),'Walnut',.035)

group='Wall_Right'
box((6.12,.9,0),(.24,1.8,14.2),'Plaster',.03)
box((6.12,7.05,0),(.24,1.1,14.2),'Plaster',.03)
for z,d in [(-6.2,1.6),(-2.45,1.5),(1.25,1.5),(5.7,2.6)]: box((6.12,4.15,z),(.24,4.7,d),'Plaster',.025)
for y in [.15,1.55,7.3]: box((5.94,y,0),(.18,.20,14),'WalnutEdge',.025)
for iz in range(14):
    z=-6.5+iz
    box((5.98,.8,z),(.12,1.3,.95),'WalnutPanel',.025)
    box((5.89,.8,z),(.10,1.0,.72),'Walnut',.025)
for z in [-6.7,-2.45,1.25,5.45,6.7]: box((5.89,3.75,z),(.24,7.5,.24),'Walnut',.035)

for z in [-4.3,-.6,3.1]:
    group=f'Window_{z}'
    for zz in [z-1.17,z+1.17]: box((5.94,4.15,zz),(.42,4.85,.22),'Limestone',.045)
    for y in [1.82,6.48]: box((5.87,y,z),(.57,.24,2.58),'StoneLight',.045)
    box((6.02,4.15,z),(.17,4.45,.11),'Limestone',.015)
    box((6.02,4.15,z),(.17,.12,2.25),'Limestone',.015)
    # Thin diamond leadwork, visible against the garden beyond.
    for zz in [z-.57,z+.57]:
        for yy in [2.4,3.55,4.70,5.85]:
            for sign in [-1,1]:
                bar((6.10,yy-.55,zz-sign*.50),(6.10,yy+.55,zz+sign*.50),.025,.025,'WalnutEdge',.002)
    group='Garden_View'
    box((9.0,4.2,z),(.1,5.8,3.6),'Sky',0)
    sphere((8.7,1.35,z),(.85,1.4,1.9),'DistantGreen',2)
    sphere((9.2,1.8,z+1.2),(1.1,1.4,1.2),'Leaves',2)

group='Wall_Front'
box((0,3.8,-7.12),(12.3,7.6,.24),'Plaster',.035)
for y in [.15,1.55,3.0,7.3]: box((0,y,-6.94),(12,.20,.18),'WalnutEdge',.025)
for x in [-5.8,-3.3,0,3.3,5.8]: box((x,3.75,-6.88),(.25,7.5,.25),'Walnut',.035)
for ix in range(12): box((-5.5+ix,.82,-6.96),(.95,1.3,.12),'WalnutPanel',.025)
group='Ceiling'
box((0,7.68,0),(12.5,.16,14.5),'Plaster',.025)
for z in [-6.6,-3.3,0,3.3,6.6]:
    box((0,7.36,z),(12.1,.36,.32),'Walnut',.05)
    for x in [-5.7,5.7]:
        bar((x,6.5,z),(x+(.8 if x<0 else -.8),7.25,z),.24,.25,'Walnut',.035)
for x in [-3,0,3]: box((x,7.47,0),(.18,.20,14),'WalnutPanel',.025)

# Stair with a continuous runner. Collisions use a separate invisible ramp in Unity.
group='Stair_Body'
nsteps=18
for i in range(nsteps):
    y=(i+1)*.2; z=-2.2+(i+.5)*.35
    box((2.9,y/2,z),(2.6,y,.35),'Walnut',.025)
    box((2.9,y+.015,z-.015),(2.72,.065,.39),'WalnutEdge',.025)
    box((2.9,y+.053,z-.012),(1.54,.015,.36),'RugRed',.005)
    box((2.9,y-.07,z-.185),(1.54,.16,.012),'RugRed',.003)
    for x in [2.20,3.60]:
        box((x,y+.065,z-.012),(.055,.008,.35),'RugBorder',.002)
        box((x,y-.07,z-.196),(.055,.16,.008),'RugBorder',.002)
group='Stair_Rails'
for x in [1.54,4.26]:
    post(x,.05,-2.34)
    post(x,3.62,4.18)
    railing((x,.22,-2.18),(x,3.62,4.12),17)
    bar((x,.22,-2.18),(x,3.62,4.12),.30,.25,'Walnut',.035)
group='Gallery_Rails'
for x,z in [(-3.55,-6.65),(-3.55,-1.05),(-3.55,4.06),(1.50,4.06),(5.65,4.06)]: post(x,3.62,z)
railing((-3.55,3.62,-6.65),(-3.55,3.62,-1.05))
railing((-3.55,3.62,-1.05),(-3.55,3.62,4.06))
railing((-3.55,3.62,4.06),(1.50,3.62,4.06))
railing((4.28,3.62,4.06),(5.65,3.62,4.06),3)

group='Carpets'
rug(-.9,.052,-1.7,3.8,5.0)
rug(-1.0,3.68,5.65,7.2,1.95)
rug(-4.75,3.68,-1.0,1.65,8.2)

# Soft stone portal surrounds. Upper doors remain independent animated objects.
def portal_frame(x,y,z, width=1.8, height=2.6):
    for side in [-1,1]:
        box((x+side*(width/2+.12),y+height/2,z),(.24,height,.32),'Limestone',.05)
        for yy in [.4,1.0,1.6,2.2]:
            box((x+side*(width/2+.12),y+yy,z-.17),(.25,.018,.015),'StoneWarm',.003)
        box((x+side*(width/2+.12),y+.1,z),(.34,.2,.42),'StoneLight',.035)
    box((x,y+height+.08,z),(width+.5,.25,.35),'StoneLight',.06)
    box((x,y+height+.23,z),(width+.65,.12,.43),'Limestone',.03)

for x,label in [(-3.3,'Siblings'),(0,'Wardrobe')]:
    group=f'DoorFrame_{label}'
    portal_frame(x,3.65,6.91)
    # A short vestibule keeps the portal behind the closed door.
    box((x,3.52,8.0),(1.82,.25,2.0),'Walnut',.025)
    for side in [-1,1]: box((x+side*1.04,5.05,8.0),(.26,2.9,2.1),'Plaster',.025)
    box((x,6.56,8.0),(2.2,.14,2.2),'Plaster',.025)
    box((x,5.0,9.0),(2.2,3,.22),'WalnutPanel',.025)

group='Study_Door'
portal_frame(-3.3,0,6.67,1.65,2.5)
box((-3.3,1.22,6.68),(1.58,2.44,.14),'WalnutPanel',.06)
for x in [-3.83,-3.3,-2.77]: box((x,1.22,6.59),(.035,2.32,.07),'Walnut',.012)
for y in [.30,2.1]: box((-3.3,y,6.53),(1.55,.15,.09),'WalnutEdge',.025)
lathe((-2.72,1.02,6.43),[(0,.07),(.10,.07)],'Brass',12)
sphere((-2.72,1.14,6.43),(.07,.07,.06),'Brass',2)

# Framed portraits made of flat shapes: restrained stylised detail, no photo textures.
def portrait(x,y,z,w,h,variant=0, turn=False):
    start=len(groups.setdefault(group,[]))
    box((x,y,z),(w,.05+h,.12),'Walnut',.04)
    box((x,y,z-.073),(w-.13,h-.13,.03),'Brass',.025)
    box((x,y,z-.096),(w-.22,h-.22,.018),'Ink' if variant%2==0 else 'Olive',.01)
    zz=z-.114
    flat_mesh('Portrait coat',[(x-w*.31,y-h*.37,zz),(x+w*.31,y-h*.37,zz),(x+w*.20,y-h*.04,zz),(x+w*.08,y+h*.05,zz),(x-w*.10,y+h*.05,zz),(x-w*.23,y-h*.05,zz)],[(5,4,3,2,1,0)],'WalnutPanel' if variant%2==0 else 'BookRed')
    sphere((x,y+h*.19,zz-.006),(w*.14,h*.16,.018),'Skin',2)
    sphere((x-w*.02,y+h*.29,zz-.009),(w*.155,h*.08,.021),'Walnut',2)
    flat_mesh('Portrait collar',[(x-w*.1,y+h*.035,zz-.01),(x,y-h*.06,zz-.01),(x+w*.08,y+h*.035,zz-.01)],[(2,1,0)],'Ivory')
    if turn:
        center=xyz((x,y,z))
        from mathutils import Matrix
        rot=Matrix.Rotation(-math.pi/2,4,'Z')
        for ob in groups[group][start:]: ob.matrix_world=rot@ob.matrix_world; ob.location += center-rot@center

group='Portraits_Back'
portrait(-.45,2.15,6.71,1.15,1.45,1)
portrait(2.8,5.72,6.72,1.25,1.6,0)
portrait(5.0,5.70,6.72,1.15,1.5,1)
group='Portraits_Left'
for i,z in enumerate([-4.4,-.5,3.1]): portrait(-5.65,5.9,z,1.15,1.65,i,True)

group='Bust_And_Pl inth'.replace(' ','')
x,z=-1.8,5.95
for y,s,h in [(.10,.72,.20),(.76,.46,1.12),(1.4,.72,.18),(1.57,.55,.15)]: box((x,y,z),(s,h,s),'Limestone',.035)
sphere((x,1.75,z),(.30,.16,.18),'Ivory',2)
lathe((x,1.73,z),[(0,.20),(.17,.14),(.32,.10)],'Ivory',8)
sphere((x,2.18,z),(.18,.26,.18),'Ivory',2)
sphere((x,2.32,z+.02),(.195,.16,.19),'StoneLight',2)
sphere((x,2.18,z-.175),(.05,.06,.07),'Ivory',1)

def chair(x,z):
    for xx in [x-.36,x+.36]:
        for zz in [z-.32,z+.32]:
            bar((xx,.05,zz),(xx,.68,zz),.10,.12,'WalnutEdge',.025)
    box((x,.64,z),(.90,.18,.80),'WalnutEdge',.045)
    box((x,.77,z-.02),(.80,.17,.72),'Olive',.075)
    box((x,1.18,z+.31),(.90,1.02,.17),'WalnutEdge',.075)
    box((x,1.22,z+.195),(.69,.75,.14),'Olive',.065)
    for xx in [x-.45,x+.45]:
        box((xx,1.02,z),(.13,.12,.76),'WalnutEdge',.04)
        box((xx,.88,z-.29),(.10,.30,.10),'WalnutEdge',.02)
group='Reading_Chair'
chair(.25,5.82)

group='Console_And_Books'
x,z=-4.85,-2.65
box((x,1.10,z),(1.35,.16,1.9),'WalnutEdge',.05)
box((x,.88,z),(1.17,.30,1.72),'WalnutPanel',.035)
for xx in [x-.48,x+.48]:
    for zz in [z-.70,z+.70]:
        lathe((xx,.05,zz),[(0,.065),(.12,.09),(.50,.055),(.78,.09),(.85,.085)],'WalnutEdge')
box((x,.27,z),(1.15,.10,1.7),'Walnut',.03)
for i in range(3):
    box((x+.05,1.23+i*.11,z+.42),(.57,.11,.43),['BookBlue','BookRed','Olive'][i],.018)
    box((x+.06,1.23+i*.11,z+.42),(.53,.07,.445),'Ivory',.008)
lathe((x,1.19,z-.47),[(0,.12),(.08,.19),(.28,.20),(.42,.11),(.49,.12)],'Ivory',10)
for i in range(5):
    a=i*2.4
    top=(x+math.cos(a)*.20,2.0+random.random()*.13,z-.47+math.sin(a)*.18)
    bar((x,1.60,z-.47),top,.018,.018,'Leaves',.002)
    sphere((top[0]+.055,top[1]-.16,top[2]),(.10,.035,.07),'Leaves',1)
    for j in range(5):
        a2=j*math.pi*2/5
        sphere((top[0]+math.cos(a2)*.055,top[1]+math.sin(a2)*.055,top[2]),(.055,.05,.03),'Ivory',1)
    sphere(top,(.025,.025,.04),'RugBorder',1)

# Wall sconces, small warm flame shapes; Unity supplies local lights.
group='Sconces'
for x,y,z in [(-4.8,2.25,6.68),(1.25,2.20,6.68),(-1.65,5.40,6.68)]:
    box((x,y,z),(.13,.43,.12),'Brass',.035)
    bar((x,y-.06,z),(x,y-.06,z-.26),.07,.07,'Brass',.012)
    bar((x-.18,y-.06,z-.26),(x+.18,y-.06,z-.26),.06,.06,'Brass',.012)
    for xx in [x-.18,x+.18]:
        lathe((xx,y-.04,z-.26),[(0,.08),(.04,.09),(.08,.055),(.26,.055)],'Ivory',8)
        sphere((xx,y+.30,z-.26),(.055,.12,.055),'Glow',1)

# Export each room-sized/functional chunk as one mesh, keeping material submeshes.
def consolidate():
    meshes=[]
    for name, objs in groups.items():
        if not objs: continue
        bpy.ops.object.select_all(action='DESELECT')
        for obj in objs: obj.select_set(True)
        bpy.context.view_layer.objects.active=objs[0]
        bpy.ops.object.convert(target='MESH')
        bpy.ops.object.join()
        obj=bpy.context.object; obj.name=name
        bpy.context.scene.cursor.location=(0,0,0)
        bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
        bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
        # Angle-based UVs for an editable source; Unity generates separate lightmap UVs.
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=math.radians(66),island_margin=.015)
        bpy.ops.object.mode_set(mode='OBJECT')
        meshes.append(obj)
    return meshes

environment=consolidate()
def export(path, objects):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects: obj.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'MESH'},
        axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,
        mesh_smooth_type='OFF',use_mesh_modifiers=True,add_leaf_bones=False,
        bake_anim=False,path_mode='AUTO')
export(out/'FBX_ProfessorHouse_Hall_v2.fbx',environment)

# Hinge at local (0,0,0); leaf spans +X and opens by rotating local Unity Y.
groups={}; group='DoorLeaf'
box((.86,1.27,0),(1.72,2.54,.13),'WalnutPanel',.055)
for x in [.10,1.62]: box((x,1.27,-.07),(.15,2.45,.075),'WalnutEdge',.025)
for y in [.12,1.02,2.42]: box((.86,y,-.07),(1.60,.16,.085),'WalnutEdge',.025)
for y,h in [(.57,.63),(1.75,1.13)]:
    for x in [.46,1.26]:
        box((x,y,-.09),(.62,h,.08),'Walnut',.04)
        box((x,y,-.14),(.47,h-.15,.06),'WalnutPanel',.035)
box((1.43,1.12,-.14),(.12,.27,.035),'Brass',.035)
sphere((1.43,1.14,-.22),(.055,.055,.075),'Brass',2)
door=consolidate()
export(out/'FBX_ProfessorHouse_Door_v2.fbx',door)
for obj in door: obj.hide_set(True)
for obj in environment: obj.hide_set(False)
bpy.context.scene.unit_settings.system='METRIC'
bpy.ops.wm.save_as_mainfile(filepath=str(source/'ProfessorHouse_v2.blend'))
report={'revision':2,'units':'metres','meshes':len(environment),
        'vertices':sum(len(o.data.vertices) for o in environment),
        'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in environment),
        'palette':{n:{'hex':v[0],'smoothness':v[1]} for n,v in palette.items()},
        'layout':{'spawn':[-.9,.15,-2.9],'galleryHeight':3.65,'stairsStart':[2.9,.1,-2.5],
                  'stairsEnd':[2.9,3.68,4.6],'siblingsDoor':[-3.3,3.65,6.9],'wardrobeDoor':[0,3.65,6.9]}}
(out/'ProfessorHouse_v2.json').write_text(json.dumps(report,indent=2)+'\n')
print('HOUSE_MODEL_COMPLETE '+json.dumps({k:report[k] for k in ['meshes','vertices','triangles']}),flush=True)
# Exit directly after saving; avoids a Blender/Metal shutdown hang in headless macOS runs.
os._exit(0)
