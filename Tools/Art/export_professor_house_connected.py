"""Derive Unity FBX chunks and collision ramps from the approved connected model.

Blender: --background --factory-startup --python this_file -- --project PATH
The reviewed source is never overwritten. Unity coordinates are X east, Y up, Z rear.
"""
import argparse
from collections import defaultdict
import hashlib
import json
import os
from pathlib import Path
import re
import sys

import bpy
import bmesh
from mathutils import Matrix, Vector

parser = argparse.ArgumentParser()
parser.add_argument('--project', required=True)
parser.add_argument('--chunks', nargs='+', help='Export only selected FBX files; manifest always covers all chunks.')
options = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
project = Path(options.project).resolve()
source = project / 'ArtSource/ProfessorHouse/ConnectedLayout_v1'
output = project / 'Assets/@Project/10.Models/ProfessorHouseConnected'
output.mkdir(parents=True, exist_ok=True)
spec = json.loads((source / 'Data/model-spec.json').read_text())
layout = json.loads((source / 'Data/layout.json').read_text())
bpy.ops.wm.open_mainfile(filepath=str(source / 'Models/ProfessorHouse_ConnectedLayout_01.blend'))
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1

def point(v):
    return dict(x=round(v[0], 6), y=round(v[2], 6), z=round(v[1], 6))

def chunk(level, zone):
    if zone == 'hall':
        return 'Hall'
    if zone == 'S2' or zone.endswith('/S2'):
        return 'SecondaryStairs'
    return {-1:'Gardens', 0:'GroundRooms', 1:'UpperRooms', 2:'AtticRooms'}[level]

def safe(value):
    return re.sub('[^A-Za-z0-9_]', '_', str(value))

# Matches the project's established Unity FBX handedness conversion.
# Authoring (x,y,z) -> Unity (x,z,y), through Blender export basis (-x,-y,z).
conversion = Matrix.Diagonal((-1, -1, 1, 1))
groups = defaultdict(list)
objects = list(scene.objects)
for obj in objects:
    if obj.type != 'MESH' or 'part_id' not in obj:
        continue
    # Replaced by UpperFinish; keep the immutable authoring source for comparison.
    if obj.get('part_id') in ['ProfessorDesk', 'Beds_E1', 'Beds_K-BOYS']:
        continue
    role = obj.get('role', '')
    if role in ['route', 'label', 'proxy']:
        continue
    zone = obj.get('zone', '')
    level = int(obj.get('level', 0))
    region = chunk(level, zone)
    obj.hide_set(False)
    obj.hide_render = False
    obj.hide_viewport = False
    key = (region, safe(zone or 'Exterior'), 'Glass' if role == 'glass' else 'Opaque')
    groups[key].append((obj, conversion @ obj.matrix_world))
    if role not in ['floor', 'stairs', 'gallery', 'outdoor', 'surface', 'stairs-trim', 'decor']:
        groups[(region, safe(zone or 'Exterior'), 'Collision')].append((obj, conversion @ obj.matrix_world))

def support_mesh(name, vertices, faces, level, zone):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    scene.collection.objects.link(obj)
    groups[(chunk(level, zone), 'WalkSurfaces', 'Collision')].append((obj, conversion))

part_by_id = {p['id']:p for p in spec['parts']}
for index, surface in enumerate(spec['walkSurfaces']):
    part = part_by_id[surface['id']]
    if 'rect' in surface:
        x,y,w,d = surface['rect']
        z = surface['z']
        vertices = [(x,y,z-.18),(x+w,y,z-.18),(x+w,y+d,z-.18),(x,y+d,z-.18),
                    (x,y,z),(x+w,y,z),(x+w,y+d,z),(x,y+d,z)]
        faces = [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
    else:
        s = surface['stair']
        x,y,z = s['origin']
        dx,dy = s['direction']
        sx,sy = dy,-dx
        run = s['n'] * s['tread']
        height = s['n'] * s['rise']
        width = s['width']
        def at(u,v,h): return (x+dx*u+sx*v, y+dy*u+sy*v, z+h)
        vertices = [at(0,0,s['rise']*.5),at(0,width,s['rise']*.5),at(run,0,height+.015),
                    at(run,width,height+.015),at(run,0,-.10),at(run,width,-.10)]
        faces = [(0,1,3,2),(0,2,4),(1,5,3),(2,3,5,4),(0,4,5,1)]
    support_mesh('Support_'+str(index), vertices, faces, part['level'], part.get('zone') or '')

# Outdoor grass also supplies walkable collision, while the upper courtyard voids stay empty.
for obj in objects:
    if obj.type == 'MESH' and obj.get('role') == 'outdoor':
        groups[('Gardens','OutdoorSupport','Collision')].append((obj, conversion @ obj.matrix_world))

export_objects = defaultdict(list)
entries = []
for (region, zone, kind), members in sorted(groups.items()):
    vertices, faces, material_indices, materials = [], [], [], []
    for obj, transform in members:
        offset = len(vertices)
        vertices.extend(transform @ v.co for v in obj.data.vertices)
        material_map = {}
        for index, material in enumerate(obj.data.materials):
            if material not in materials:
                materials.append(material)
            material_map[index] = materials.index(material)
        for face in obj.data.polygons:
            faces.append(tuple(offset+i for i in face.vertices))
            material_indices.append(material_map.get(face.material_index, 0))
    name = ('C_' if kind == 'Collision' else 'V_') + safe(region+'_'+zone+'_'+kind)
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    for material in materials:
        mesh.materials.append(material)
    if kind != 'Collision':
        for i, polygon in enumerate(mesh.polygons):
            polygon.material_index = material_indices[i]
    obj = bpy.data.objects.new(name, mesh)
    scene.collection.objects.link(obj)
    export_objects[region].append(obj)
    # Independent expected world bounds in Unity, catches axis/sign and scale mistakes on import.
    unity_points = [Vector((-v.x, v.z, -v.y)) for v in vertices]
    minimum = [min(v[a] for v in unity_points) for a in range(3)]
    maximum = [max(v[a] for v in unity_points) for a in range(3)]
    entries.append(dict(name=name, chunk=region, collision=kind=='Collision', glass=kind=='Glass',
        boundsMin=dict(zip(('x','y','z'),minimum)),boundsMax=dict(zip(('x','y','z'),maximum)),
        vertices=len(vertices), triangles=sum(len(face)-2 for face in faces)))

for region, members in sorted(export_objects.items()):
    if options.chunks and region not in options.chunks:
        continue
    bpy.ops.object.select_all(action='DESELECT')
    for obj in members:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = members[0]
    bpy.ops.export_scene.fbx(filepath=str(output / ('FBX_ProfessorHouse_'+region+'.fbx')),
        use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
        apply_unit_scale=True, bake_space_transform=True, mesh_smooth_type='OFF',
        use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False, path_mode='AUTO')

routes = [dict(key=k,title=r['title'],points=[point(p) for p in r['points']]) for k,r in spec['routes'].items()]
routes += [dict(key=k,title=k,points=[point(p) for p in points]) for k,points in spec['extraValidationRoutes'].items()]
manifest = dict(version='connected-walkthrough-02',replacedBlockoutParts=['ProfessorDesk','Beds_E1','Beds_K-BOYS'],units='metres, Unity Y up; X east, Z rear',
    sourceLayoutSha256=spec['approvedLayoutSha256'], sourceBlendSha256=hashlib.sha256((source/'Models/ProfessorHouse_ConnectedLayout_01.blend').read_bytes()).hexdigest(),
    chunks=sorted(export_objects), meshes=entries,
    palette=[dict(name=name,hex=color) for name,color in sorted(spec['palette'].items())],
    routes=routes, doors=spec['doors'], spawn=point([17,11,0]),
    characterHeight=1.2,characterRadius=.22,roomCount=50,doorCount=54)
(output/'ProfessorHouse_Connected.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
print('CONNECTED_EXPORT_COMPLETE '+json.dumps(dict(chunks=len(export_objects),meshes=len(entries),visuals=sum(not e['collision'] for e in entries))),flush=True)
os._exit(0)
