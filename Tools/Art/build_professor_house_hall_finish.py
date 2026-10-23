"""Build a reversible decorative layer over the approved manor layout.

Blender --background --factory-startup --python this_file -- --project PATH
Authoring: X east, Y rear, Z up. Export: Unity X east, Y up, Z rear.
"""
import argparse
from collections import defaultdict
import hashlib
import json
import math
import os
from pathlib import Path
import random
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Matrix, Vector

parser = argparse.ArgumentParser()
parser.add_argument('--project', required=True)
options = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
project = Path(options.project).resolve()
source = project / 'ArtSource/ProfessorHouse/HallFinish_v1'
output = project / 'Assets/@Project/10.Models/ProfessorHouseHallFinish'
source.mkdir(parents=True, exist_ok=True)
output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
random.seed(23)
palette = {
    'HallWalnut': '513122', 'HallWalnutEdge': '795039', 'HallGold': 'BE9253',
    'HallStone': 'DCCDB3', 'HallIvory': 'EEE1C6', 'HallSage': '647C61',
    'HallLeaf': '516B38', 'HallLeafLight': '8D9B52', 'HallPetal': 'F1DFB0',
    'HallRugLight': '9B5441', 'HallRugDark': '62362E', 'HallRugGold': 'BC8C50',
    'HallFlame': 'FFE2A4', 'HallPainting': 'FFFFFF', 'HallBook': '586F72', 'HallPlaster': 'E0CDAA',
}
from professor_house_finish_kit import FinishKit
kit = FinishKit(palette)
register = kit.register
box = kit.box
sphere = kit.sphere
beam = kit.beam
lathe = kit.lathe
collider = kit.collider
vase = kit.vase
painting = kit.painting
sconce = kit.sconce
table = kit.table
kit.group='NorthWall'
painting('Gentleman portrait',13.45,21.7,6.12,1.45,1.95,0)
painting('Lady portrait',19.38,21.72,5.6,1.28,1.75,1)
painting('Pastoral landscape',20.0,21.7,2.25,1.3,1.0,3)
sconce(12.22,21.68,5.25)
sconce(19.5,21.68,7.15)
vase(16.06,21.22,1.83,.9)
vase(17.94,21.22,1.83,.9)
collider('Landing urn west',(16.06,21.22,2.08),(.42,.42,.5))
collider('Landing urn east',(17.94,21.22,2.08),(.42,.42,.5))

kit.group='WestFurnishing'
painting('Manor landscape',12.35,21.7,2.22,1.4,1.0,2)
painting('West gallery portrait',10.29,16.1,6.1,1.05,1.45,1,math.pi/2)
sconce(10.25,17.5,2.35,math.pi/2)
sconce(10.25,12,6.0,math.pi/2)
table(10.95,20.12)
x,y=12.72,20.65
for xx in [-.32,.32]:
    for yy in [-.25,.25]:box('Armchair foot',(x+xx,y+yy,.24),(.085,.085,.42),'HallWalnut',.025)
box('Chair seat rim',(x,y,.48),(.83,.77,.13),'HallWalnutEdge',.055)
box('Rounded seat cushion',(x,y-.035,.59),(.69,.68,.14),'HallSage',.09)
box('Chair back carved rim',(x,y+.29,1.02),(.88,.17,.92),'HallWalnutEdge',.085)
box('Upholstered back',(x,y+.18,1.05),(.69,.13,.69),'HallSage',.09)
for xx in [-.4,.4]:
    beam('Arm support',(x+xx,y-.25,.5),(x+xx,y-.25,.83),.035,'HallWalnut')
    box('Rounded chair arm',(x+xx,y,.84),(.12,.75,.1),'HallWalnutEdge',.04)
collider('Armchair',(x,y,.75),(.94,.85,1.5))
x,y=14.25,20.12
box('Bust pedestal foot',(x,y,.10),(.62,.52,.2),'HallStone',.03)
box('Bust pedestal shaft',(x,y,.63),(.41,.38,.92),'HallIvory',.022)
box('Bust pedestal cap',(x,y,1.12),(.57,.48,.12),'HallStone',.025)
lathe('Bust plinth',(x,y,1.18),[(0,.19),(.08,.19),(.11,.14)],'HallIvory')
torso=lathe('Draped sculpture torso',(x,y,1.29),[(0,.16),(.04,.17),(.12,.25),(.23,.27),(.34,.2),(.38,.1)],'HallIvory')
for vertex in torso.data.vertices:
    vertex.co.y=y+(vertex.co.y-y)*.58
sphere('Sculpture head',(x,y,1.88),(.145,.13,.205),'HallIvory')
sphere('Sculpture nose',(x,y-.13,1.87),(.027,.046,.055),'HallIvory')
beam('Bust neck',(x,y,1.6),(x,y,1.77),.075,'HallIvory')
sphere('Sculpture swept hair',(x,y+.014,2.045),(.147,.129,.085),'HallStone')
for side in [-1,1]:
    sphere('Sculpture ear',(x+side*.14,y,1.88),(.027,.035,.049),'HallIvory')
    sphere('Sculpture eye',(x+side*.058,y-.12,1.918),(.022,.012,.01),'HallStone')
    beam('Sculpture brow',(x+side*.036,y-.126,1.944),(x+side*.082,y-.109,1.944),.011,'HallIvory')
    beam('Carved lapel',(x+side*.16,y-.12,1.59),(x+side*.025,y-.13,1.38),.016,'HallStone')
sphere('Sculpture mouth',(x,y-.125,1.804),(.039,.018,.01),'HallStone')
collider('Pedestal',(x,y,.65),(.62,.52,1.3))

kit.group='EastFurnishing'
table(23.04,20.15)
painting('East manor painting',23.71,18.9,2.2,1.4,1.05,2,-math.pi/2)
painting('East gallery portrait',23.71,13.6,6.0,1.1,1.5,0,-math.pi/2)
sconce(23.75,18.0,2.4,-math.pi/2)
sconce(23.75,17.0,6.05,-math.pi/2)

kit.group='Ceiling'
# Pale ceiling panels sit below the dark attic underside, between existing beams.
for x in [11.85,15.3,18.7,22.15]:
    for y,d in [(13.23,4.54),(18.55,4.72)]:
        box('Ivory coffer',(x,y,8.77),(3.14,d,.08),'HallIvory',.018)
for x in [13.6,17,20.4]:box('Long ceiling beam',(x,16,8.65),(.18,11.4,.22),'HallWalnutEdge',.025)

kit.group='RugInlay'
# Shallow geometry inlay avoids new image dependencies and z-fighting.
for inset in [.25,.39]:
    x0,x1,y0,y1=13+inset,21-inset,11+inset,14.4-inset
    for y in [y0,y1]:box('Rug woven border',((x0+x1)/2,y,.066),(x1-x0,.026,.006),'HallRugGold',0)
    for x in [x0,x1]:box('Rug woven border',(x,(y0+y1)/2,.066),(.026,y1-y0,.006),'HallRugGold',0)
for x in [13.6+i*.32 for i in range(22)]:
    for y in [11.32,14.08]:
        obj=box('Border lozenge',(x,y,.068),(.06,.06,.006),'HallRugLight',0)
        obj.rotation_euler.z=math.pi/4
for x in [14.4,15.7,17,18.3,19.6]:
    obj=box('Rug medallion',(x,12.7,.067),(.52,.52,.006),'HallRugGold',0)
    obj.rotation_euler.z=math.pi/4
    obj=box('Rug medallion inset',(x,12.7,.073),(.42,.42,.006),'HallRugLight',0)
    obj.rotation_euler.z=math.pi/4
    sphere('Rug motif',(x,12.7,.074),(.15,.15,.005),'HallRugDark')

kit.group='GardenVista'
for x,y,height in [(14.1,27.9,5.7),(19.8,28.6,6.1),(16.6,32,5.6)]:
    beam('Garden tree trunk',(x,y,0),(x,y,height*.78),.16,'HallWalnutEdge')
    collider('Garden trunk',(x,y,height*.37),(.34,.34,height*.74))
    for j in range(6):
        a=j*2.4
        sphere('Rounded tree crown',(x+.65*math.cos(a),y+.6*math.sin(a),height-.4+(j%3)*.35),(1.02,.87,.94),'HallLeafLight' if j%2 else 'HallLeaf')

kit.group='WallFinish'
spec=json.loads((project/'ArtSource/ProfessorHouse/ConnectedLayout_v1/Data/model-spec.json').read_text())
for part in spec['parts']:
    if part.get('zone')!='hall' or part['role']!='wall':continue
    for shape in part['shapes']:
        if shape['type']!='box' or shape['mat']!='plaster':continue
        x,y,z=shape['pos'];w,d,h=shape['size']
        # Continuous flat plaster faces bridge the original 8 mm construction bevels.
        if part['id']=='B02N':center,size=(x+w/2,y-.003,z+h/2),(w,.004,h)
        elif part['id']=='B02S':center,size=(x+w/2,y+d+.003,z+h/2),(w,.004,h)
        elif part['id']=='B02W':center,size=(x+w+.003,y+d/2,z+h/2),(.004,d,h)
        else:center,size=(x-.003,y+d/2,z+h/2),(.004,d,h)
        box('Continuous plaster face',center,size,'HallPlaster',0)

kit.validate_architecture(project, ['NorthWall', 'WestFurnishing', 'EastFurnishing'], output / 'PlacementValidation.json')
kit.export(project, source, output, 'HallFinish', 'HallFinish.json', 'hall-finish-02')
os._exit(0)
