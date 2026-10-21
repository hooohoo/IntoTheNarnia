#!/usr/bin/env python3
"""Offline guard for the shared Chalk contracts; no source edits or Unity license required."""
from pathlib import Path
import json
import re
import sys

root = Path(__file__).resolve().parents[1]
failures = []

def check(value, message):
    if not value:
        failures.append(message)

expected = {
    'com.unity.addressables': '1.22.3',
    'com.unity.collections': '2.2.1',
    'com.unity.nuget.mono-cecil': '1.11.6',
    'com.unity.mathematics': '1.3.1',
    'com.unity.test-framework': '1.3.3',
    'com.unity.render-pipelines.universal': '14.0.12',
    'com.unity.cinemachine': '2.10.5',
    'com.unity.textmeshpro': '3.0.7',
    'com.unity.nuget.newtonsoft-json': '3.2.2',
    'com.unity.timeline': '1.7.7',
    'com.unity.memoryprofiler': '1.1.11',
    'com.unity.recorder': '4.0.3',
}
manifest = json.loads((root / 'Packages/manifest.json').read_text())['dependencies']
locked = json.loads((root / 'Packages/packages-lock.json').read_text())['dependencies']
for name, version in expected.items():
    check(manifest.get(name) == version, name + ': manifest version differs from Chalk baseline')
    check(locked.get(name, {}).get('version') == version, name + ': lock version does not match manifest')
check('2022.3.62f3' in (root / 'ProjectSettings/ProjectVersion.txt').read_text(), 'Editor version differs')
editor = (root / 'ProjectSettings/EditorSettings.asset').read_text()
for setting in ('m_SerializationMode: 2', 'm_LineEndingsForNewScripts: 2', 'm_SpritePackerMode: 5', 'm_SerializeInlineMappingsOnOneLine: 0'):
    check(setting in editor, 'Missing editor contract: ' + setting)
check('m_Mode: Visible Meta Files' in (root / 'ProjectSettings/VersionControlSettings.asset').read_text(), 'Visible Meta Files required')
player = (root / 'ProjectSettings/ProjectSettings.asset').read_text()
check('m_ActiveColorSpace: 1' in player, 'Linear color space required')
check('activeInputHandler: 0' in player, 'Legacy input backend contract changed')
check('m_CustomRenderPipeline: {fileID: 0}' not in (root / 'ProjectSettings/GraphicsSettings.asset').read_text(), 'URP pipeline not assigned')
graphics = (root / 'ProjectSettings/GraphicsSettings.asset').read_text()
for setting in ('m_LightsUseLinearIntensity: 1', 'm_LightsUseColorTemperature: 1'):
    check(setting in graphics, 'Missing URP lighting contract: ' + setting)
for setting in ('webGLInitialMemorySize: 32', 'webGLMaximumMemorySize: 1024', 'webGLMemoryGrowthMode: 2', 'webGLPowerPreference: 2'):
    check(setting in player, 'Missing WebGL contract: ' + setting)
project = root / 'Assets/@Project'
check(not list(project.rglob('Resources')), 'Own game content must not live in Resources')
for folder in ['00.Scenes','01.Scripts','02.Prefabs','03.Sprites','04.Materials','05.Animations','06.Data','07.Sounds','08.Fonts','09.Shaders','10.Models','Settings','Editor','Tests','Samples']:
    check((project / folder).is_dir(), 'Missing content category: ' + folder)
for path in (project / '01.Scripts').rglob('*.cs'):
    text = path.read_text(encoding='utf-8')
    check(not re.search(r'\bResources\.Load(?:All)?\s*[<(]', text), 'Resources loader: ' + str(path.relative_to(root)))
    check('using System.Linq' not in text, 'LINQ: ' + str(path.relative_to(root)))
    check(not re.search(r'\b(?:void|IEnumerator)\s+(?:Update|FixedUpdate|LateUpdate)\s*\(', text), 'Unity frame callback: ' + str(path.relative_to(root)))
    check('IEventListener<' not in text, 'Obsolete event contract: ' + str(path.relative_to(root)))
    if path.name != 'LogUtil.cs':
        check(not re.search(r'\bDebug\.Log', text), 'Direct log: ' + str(path.relative_to(root)))
settings = root / 'Assets/AddressableAssetsData/AddressableAssetSettings.asset'
check(settings.is_file(), 'Addressables settings missing')
for label in ['Title','Loading','ProfessorHouse','Story']:
    check((root / ('Assets/AddressableAssetsData/AssetGroups/' + label + '.asset')).is_file(), 'Feature group missing: ' + label)
for prefab in ['UI_Title','UI_Loading','UI_Field','UI_Settings','UI_Dialogue']:
    check((project / ('02.Prefabs/UI/' + prefab + '.prefab')).is_file(), 'Addressable UI missing: ' + prefab)
attrs = (root / '.gitattributes').read_text()
for extension in ['png','jpg','fbx','wav']:
    check('*.' + extension + ' filter=lfs' in attrs, 'LFS rule missing: ' + extension)
check('Assets/Asset filter=' not in attrs, 'Non-recursive LFS directory pattern remains')
readme = (root / 'CLAUDE.md').read_text()
for contract in ['main은 병합 전용','1 push · 1 merge','PR은 선택 사항']:
    check(contract in readme, 'User Git policy missing: ' + contract)
print(f'Chalk contract checks: {len(failures)} failure(s)')
for message in failures:
    print(message)
sys.exit(bool(failures))
