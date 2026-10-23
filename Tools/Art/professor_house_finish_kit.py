"""Shared rounded mesh primitives for approved manor finish layers (Blender)."""
from collections import defaultdict
import hashlib
import json
import math
import random
from pathlib import Path
import bpy
import bmesh
from mathutils import Matrix, Vector
from mathutils.bvhtree import BVHTree

class FinishKit:
    def __init__(self, palette):
        self.palette = palette
        self.materials = {}
        self.groups = defaultdict(list)
        self.colliders = []
        self.flames = []
        self.group = 'Default'
        self.mesh_cache = {}
        random.seed(23)
        for name, color in palette.items():
            mat = bpy.data.materials.new(name)
            mat.diffuse_color = (*[int(color[i:i+2],16)/255 for i in (0,2,4)], 1)
            mat.use_nodes = True
            node = mat.node_tree.nodes.get('Principled BSDF')
            node.inputs['Base Color'].default_value = mat.diffuse_color
            node.inputs['Roughness'].default_value = .5
            self.materials[name] = mat

    def register(self, obj, name, material, smooth=False):
        obj.name = name
        if not obj.data.materials: obj.data.materials.append(self.materials[material])
        if smooth:
            for face in obj.data.polygons:
                face.use_smooth = True
        self.groups[self.group].append(obj)
        return obj

    def box(self, name, center, size, material='HallWalnut', bevel=.025):
        key = ('box', tuple(round(v,6) for v in size), material, bevel)
        if key not in self.mesh_cache:
            mesh = bpy.data.meshes.new(name)
            bm = bmesh.new()
            bmesh.ops.create_cube(bm, size=1)
            bmesh.ops.transform(bm, matrix=Matrix.Diagonal((*size,1)), verts=list(bm.verts))
            if bevel:
                bmesh.ops.bevel(bm, geom=list(bm.edges), offset=min(bevel,min(size)*.3), segments=2, profile=.5, affect='EDGES')
            bm.normal_update();bm.to_mesh(mesh);bm.free();mesh.update()
            self.mesh_cache[key] = mesh
        obj = bpy.data.objects.new(name, self.mesh_cache[key])
        bpy.context.collection.objects.link(obj)
        obj.location = center
        return self.register(obj,name,material)

    def sphere(self, name, center, scale, material):
        key = ('sphere',material)
        if key not in self.mesh_cache:
            mesh = bpy.data.meshes.new(name)
            bm = bmesh.new()
            bmesh.ops.create_uvsphere(bm,u_segments=12,v_segments=8,radius=1)
            bm.to_mesh(mesh);bm.free();mesh.update()
            self.mesh_cache[key] = mesh
        obj = bpy.data.objects.new(name,self.mesh_cache[key])
        bpy.context.collection.objects.link(obj)
        obj.location = center;obj.scale = scale
        return self.register(obj,name,material,True)

    def beam(self, name, a, b, radius, material):
        a,b=Vector(a),Vector(b)
        key=('beam',material)
        if key not in self.mesh_cache:
            n=10
            verts=[(math.cos(i*math.tau/n),math.sin(i*math.tau/n),z) for z in [-.5,.5] for i in range(n)]
            faces=[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]+[tuple(reversed(range(n))),tuple(range(n,2*n))]
            mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
            self.mesh_cache[key]=mesh
        obj=bpy.data.objects.new(name,self.mesh_cache[key]);bpy.context.collection.objects.link(obj)
        obj.location=(a+b)*.5;obj.scale=(radius,radius,(b-a).length)
        obj.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
        return self.register(obj,name,material,True)

    def lathe(self, name, center, profile, material):
        n = 16
        verts = [(center[0]+r*math.cos(2*math.pi*j/n),center[1]+r*math.sin(2*math.pi*j/n),center[2]+z) for z,r in profile for j in range(n)]
        faces = [(k*n+j,k*n+(j+1)%n,(k+1)*n+(j+1)%n,(k+1)*n+j) for k in range(len(profile)-1) for j in range(n)]
        faces += [tuple(reversed(range(n))),tuple((len(profile)-1)*n+j for j in range(n))]
        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata(verts,[],faces)
        mesh.update()
        obj = bpy.data.objects.new(name,mesh)
        bpy.context.collection.objects.link(obj)
        self.register(obj,name,material)
        for face in mesh.polygons:
            face.use_smooth = len(face.vertices)==4
        return obj

    def collider(self, name, center, size):
        self.colliders.append(dict(name=name,center=dict(x=center[0],y=center[2],z=center[1]),size=dict(x=size[0],y=size[2],z=size[1])))

    def vase(self, x,y,z,scale=1):
        self.lathe('Ceramic urn',(x,y,z),[(h*scale,r*scale) for h,r in [(0,.15),(.06,.18),(.11,.12),(.3,.22),(.44,.16),(.5,.1),(.54,.14)]],'HallIvory')
        for i in range(9):
            angle = i*2.39996
            radius = (.12 if i%3==0 else .27)*scale
            tip = (x+math.cos(angle)*radius,y+math.sin(angle)*radius,z+(.66+random.random()*.18)*scale)
            self.beam('Flower stem',(x,y,z+.42*scale),tip,.012*scale,'HallLeaf')
            leaf = self.sphere('Oval leaf',(tip[0],tip[1],tip[2]-.12*scale),(.11*scale,.05*scale,.045*scale),'HallLeaf')
            leaf.rotation_euler.z=angle
            for petal in range(5):
                a = petal*math.tau/5
                self.sphere('Ivory blossom',(tip[0]+.045*scale*math.cos(a),tip[1]+.045*scale*math.sin(a),tip[2]),(.048*scale,.045*scale,.025*scale),'HallPetal')
            self.sphere('Flower center',tip,(.024*scale,)*3,'HallGold')

    def painting(self, name, x,y,z,w,h,tile,rotation=0):
        start = len(self.groups[self.group])
        self.box(name+' backing',(x,y,z),(w+.16,.12,h+.16),'HallWalnut',.015)
        for pad,thick,depth,mat in [(0,.085,.16,'HallGold'),(.075,.025,.19,'HallWalnutEdge'),(.104,.018,.2,'HallGold')]:
            ww,hh=w-2*pad,h-2*pad
            for xx in [-ww/2,ww/2]:self.box('Frame upright',(x+xx,y-depth/2,z),(thick,depth,hh+thick),mat,.012)
            for zz in [-hh/2,hh/2]:self.box('Frame rail',(x,y-depth/2,z+zz),(ww,depth,thick),mat,.012)
        ww,hh=w-.22,h-.22
        mesh=bpy.data.meshes.new(name+' canvas')
        mesh.from_pydata([(x-ww/2,y-.112,z-hh/2),(x+ww/2,y-.112,z-hh/2),(x+ww/2,y-.112,z+hh/2),(x-ww/2,y-.112,z+hh/2)],[],[(0,1,2,3)])
        mesh.update()
        obj=bpy.data.objects.new(name+' canvas',mesh)
        bpy.context.collection.objects.link(obj)
        self.register(obj,name+' canvas','HallPainting')
        col,row=tile%2,tile//2
        u0,u1=col*.5+.012,(col+1)*.5-.012
        v0,v1=(1-row)*.5+.012,(2-row)*.5-.012
        uv=mesh.uv_layers.new()
        for i,p in enumerate([(u0,v0),(u1,v0),(u1,v1),(u0,v1)]):uv.data[i].uv=p
        if rotation:
            bpy.context.view_layer.update()
            transform=Matrix.Translation((x,y,z)) @ Matrix.Rotation(rotation,4,'Z') @ Matrix.Translation((-x,-y,-z))
            for item in self.groups[self.group][start:]:item.matrix_world=transform @ item.matrix_world

    def sconce(self, x,y,z,rotation=0):
        start=len(self.groups[self.group])
        self.box('Brass wall plate',(x,y,z),(.14,.09,.38),'HallGold',.055)
        self.beam('Candle arm',(x,y,z-.08),(x,y-.28,z-.08),.028,'HallGold')
        self.lathe('Candle cup',(x,y-.28,z-.1),[(0,.055),(.04,.09),(.075,.075)],'HallGold')
        self.lathe('Wax candle',(x,y-.28,z-.025),[(0,.033),(.25,.035)],'HallIvory')
        self.sphere('Amber flame',(x,y-.28,z+.29),(.032,.032,.09),'HallFlame')
        bpy.context.view_layer.update()
        transform=Matrix.Translation((x,y,z)) @ Matrix.Rotation(rotation,4,'Z') @ Matrix.Translation((-x,-y,-z))
        for item in self.groups[self.group][start:]:item.matrix_world=transform @ item.matrix_world
        p=transform @ Vector((x,y-.28,z+.29))
        self.flames.append(dict(x=p.x,y=p.z,z=p.y))

    def table(self, x,y):
        self.box('Console polished top',(x,y,.86),(1.25,.53,.1),'HallWalnutEdge',.035)
        self.box('Console drawer',(x,y,.71),(1.1,.44,.23),'HallWalnut',.028)
        self.sphere('Drawer pull',(x,y-.25,.73),(.055,.045,.045),'HallGold')
        for xx in [-.47,.47]:
            for yy in [-.15,.15]:self.lathe('Turned table leg',(x+xx,y+yy,.05),[(0,.045),(.08,.055),(.3,.035),(.5,.06),(.64,.045)],'HallWalnut')
        self.box('Console lower shelf',(x,y,.18),(1.02,.4,.07),'HallWalnutEdge')
        for i in range(3):self.box('Old book',(x-.25,y,.94+i*.055),(.35,.24,.055),'HallBook' if i%2 else 'HallRugDark',.008)
        self.vase(x+.3,y,.91,.72)
        self.collider('Console', (x,y,.48),(1.24,.55,.96))

    def validate_architecture(self, project, group_names, report_path):
        """Triangle crossings against the approved architecture, before joining props.

        Floors/support contact and the replaced blockout props are intentionally excluded.
        This is a surface-intersection check, not a proof against fully enclosed geometry.
        """
        before = set(bpy.data.objects)
        path = project / 'ArtSource/ProfessorHouse/ConnectedLayout_v1/Models/ProfessorHouse_ConnectedLayout_01.blend'
        with bpy.data.libraries.load(str(path), link=False) as (available, loaded):
            loaded.objects = available.objects
        roles = {'stairs', 'stairs-trim', 'rails', 'gallery', 'structure', 'wall', 'frame', 'door', 'window', 'portal-frame'}
        bases = [o for o in loaded.objects if o and o.type == 'MESH' and o.get('role') in roles]
        def bounds(obj):
            points = [obj.matrix_world @ Vector(v) for v in obj.bound_box]
            return ([min(p[i] for p in points) for i in range(3)], [max(p[i] for p in points) for i in range(3)])
        def tree(obj):
            return BVHTree.FromPolygons([obj.matrix_world @ v.co for v in obj.data.vertices], [tuple(f.vertices) for f in obj.data.polygons])
        for obj in bases: bpy.context.collection.objects.link(obj)
        bpy.context.view_layer.update()
        base_data = [(o, bounds(o), tree(o)) for o in bases]
        failures = []
        count = 0
        for name in group_names:
            for obj in self.groups[name]:
                count += 1
                low, high = bounds(obj)
                prop_tree = None
                for other, (a,b), base_tree in base_data:
                    if not all(low[i] <= b[i] and high[i] >= a[i] for i in range(3)): continue
                    if prop_tree is None: prop_tree = tree(obj)
                    overlaps = prop_tree.overlap(base_tree)
                    if overlaps:
                        failures.append(dict(object=obj.name, group=name, structure=other.get('part_id',other.name), intersections=len(overlaps)))
        for obj in set(bpy.data.objects)-before: bpy.data.objects.remove(obj, do_unlink=True)
        report = dict(method='world-space triangle BVH overlap', checkedPrimitives=count, structuralMeshes=len(bases), failures=failures,
                      scope='Selected furnishings vs approved walls, windows, frames, stairs, galleries, rails and supports; support floors excluded.')
        report_path.parent.mkdir(parents=True, exist_ok=True)
        report_path.write_text(json.dumps(report,indent=2)+'\n')
        if failures: raise RuntimeError('Decor intersects architecture: '+json.dumps(failures))
        print('PLACEMENT_CHECK_PASS '+str(count),flush=True)

    def validate_group_pairs(self, pairs, report_path):
        """Check furnishings against curtains/window dressings in the same room."""
        bpy.context.view_layer.update()
        trees={}
        for name in set(n for pair in pairs for n in pair):
            vertices,faces,owners=[],[],[]
            for obj in self.groups[name]:
                offset=len(vertices)
                vertices.extend(obj.matrix_world @ v.co for v in obj.data.vertices)
                for f in obj.data.polygons:
                    faces.append(tuple(offset+i for i in f.vertices));owners.append(obj.name)
            trees[name]=(BVHTree.FromPolygons(vertices,faces),owners)
        failures=[]
        for a,b in pairs:
            ta,oa=trees[a];tb,ob=trees[b]
            pairs_hit=sorted(set((oa[i],ob[j]) for i,j in ta.overlap(tb)))
            for first,second in pairs_hit:failures.append(dict(first=first,second=second,groups=[a,b]))
        report_path.write_text(json.dumps(dict(groupPairs=pairs,failures=failures),indent=2)+'\n')
        if failures:raise RuntimeError('Furnishing/window decoration intersections: '+json.dumps(failures))
        print('DECORATION_CLEARANCE_PASS '+str(len(pairs)),flush=True)

    def export(self, project, source, output, label, manifest_name, version):
        meshes=[]
        bpy.context.view_layer.update()
        # Build independent group meshes. Joining an active shared mesh in-place can
        # expand the next group and corrupt its materials, so never mutate templates.
        for name,members in sorted(self.groups.items()):
            if not members: continue
            vertices,faces,slots,smooth,uvs,mats=[],[],[],[],[],[]
            expected_triangles=0
            for obj in members:
                offset=len(vertices)
                vertices.extend(obj.matrix_world @ v.co for v in obj.data.vertices)
                remap={}
                for i,mat in enumerate(obj.data.materials):
                    if mat not in mats:mats.append(mat)
                    remap[i]=mats.index(mat)
                uv=obj.data.uv_layers.active
                for f in obj.data.polygons:
                    faces.append(tuple(offset+i for i in f.vertices))
                    slots.append(remap[f.material_index]);smooth.append(f.use_smooth)
                    uvs.extend(tuple(uv.data[i].uv) if uv else (0,0) for i in f.loop_indices)
                    expected_triangles+=len(f.vertices)-2
            mesh=bpy.data.meshes.new('V_'+label+'_'+name)
            mesh.from_pydata(vertices,[],faces);mesh.update()
            for mat in mats:mesh.materials.append(mat)
            for i,f in enumerate(mesh.polygons):f.material_index=slots[i];f.use_smooth=smooth[i]
            uv=mesh.uv_layers.new()
            for i,value in enumerate(uvs):uv.data[i].uv=value
            if sum(len(f.vertices)-2 for f in mesh.polygons)!=expected_triangles:raise RuntimeError('Group topology mismatch: '+name)
            obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj)
            obj['role']='finish';obj['approved_layout_unchanged']=True
            meshes.append(obj)
            for old in members:bpy.data.objects.remove(old,do_unlink=True)
        if sum(len(f.vertices)-2 for obj in meshes for f in obj.data.polygons)>500000:
            raise RuntimeError('Finish layer exceeds 500,000 triangle review budget')
        image_path=project/'Assets/@Project/03.Sprites/ProfessorHouseHallFinish/TEX_Hall_Paintings.png'
        if image_path.exists() and 'HallPainting' in self.materials:
            atlas=bpy.data.images.load(str(image_path));atlas.pack()
            mat=self.materials['HallPainting']
            node=mat.node_tree.nodes.new('ShaderNodeTexImage');node.image=atlas
            mat.node_tree.links.new(node.outputs['Color'],mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
        bpy.context.scene.unit_settings.system='METRIC'
        bpy.context.scene.unit_settings.scale_length=1
        for mesh in list(bpy.data.meshes):
            if mesh.users == 0: bpy.data.meshes.remove(mesh)
        bpy.ops.wm.save_as_mainfile(filepath=str(source/('ProfessorHouse_'+label+'_v1.blend')))
        entries=[]
        conversion=Matrix.Diagonal((-1,-1,1,1))
        for obj in meshes:
            points=[obj.matrix_world @ v.co for v in obj.data.vertices]
            points=[Vector((v.x,v.z,v.y)) for v in points]
            entries.append(dict(name=obj.name,boundsMin=dict(zip(('x','y','z'),[min(v[i] for v in points) for i in range(3)])),boundsMax=dict(zip(('x','y','z'),[max(v[i] for v in points) for i in range(3)])),triangles=sum(len(f.vertices)-2 for f in obj.data.polygons)))
            obj.data.transform(conversion @ obj.matrix_world);obj.matrix_world=Matrix.Identity(4)
        bpy.ops.object.select_all(action='DESELECT')
        for obj in meshes:obj.select_set(True)
        bpy.context.view_layer.objects.active=meshes[0]
        bpy.ops.export_scene.fbx(filepath=str(output/('FBX_ProfessorHouse_'+label+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,mesh_smooth_type='FACE',add_leaf_bones=False,bake_anim=False,path_mode='STRIP',embed_textures=False)
        manifest=dict(version=version,sourceLayoutSha256=hashlib.sha256((project/'Docs/Art/ProfessorHouse_ApprovedLayout_v1/Data/layout.json').read_bytes()).hexdigest(),palette=[dict(name=n,hex=c) for n,c in self.palette.items()],meshes=entries,colliders=self.colliders,flames=self.flames)
        (output/manifest_name).write_text(json.dumps(manifest,indent=2)+'\n')
        print(label.upper()+'_EXPORT_COMPLETE '+json.dumps(dict(meshes=len(meshes),triangles=sum(e['triangles'] for e in entries),colliders=len(self.colliders))),flush=True)
