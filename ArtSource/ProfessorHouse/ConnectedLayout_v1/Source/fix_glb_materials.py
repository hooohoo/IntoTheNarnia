"""Encode the review window transparency in glTF's standard PBR material fields."""
from pathlib import Path
import json
import struct

def fix_glass(path):
    raw=path.read_bytes()
    magic,version,total=struct.unpack_from('<4sII',raw)
    assert magic==b'glTF' and version==2 and total==len(raw)
    size,kind=struct.unpack_from('<II',raw,12)
    assert kind==0x4E4F534A
    data=json.loads(raw[20:20+size])
    glass=next(m for m in data['materials'] if m.get('name')=='glass')
    # Blender's transparent-only Cycles shader has no glTF Principled equivalent.
    glass.update(alphaMode='BLEND',doubleSided=True,pbrMetallicRoughness=dict(
        baseColorFactor=[.82,.91,.94,.14],metallicFactor=0,roughnessFactor=.25))
    chunk=json.dumps(data,separators=(',',':'),ensure_ascii=False).encode()
    chunk+=b' '*((-len(chunk))%4)
    rest=raw[20+size:]
    result=struct.pack('<4sII',magic,version,20+len(chunk)+len(rest))+struct.pack('<II',len(chunk),kind)+chunk+rest
    path.write_bytes(result)
    return dict(material='glass',alphaMode=glass['alphaMode'],alpha=.14,bytes=len(result))

if __name__=='__main__':
    print(fix_glass(Path(__file__).resolve().parent.parent/'Models/ProfessorHouse_ConnectedLayout_01.glb'))
