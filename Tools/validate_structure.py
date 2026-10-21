#!/usr/bin/env python3
"""Read-only project contract checks; run from any working directory."""
from pathlib import Path
import json
import re
import sys
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("--editor-version", default="2021.3.6f1")
args = parser.parse_args()

root = Path(__file__).resolve().parents[1]
failures = []

def check(condition, message):
    if not condition:
        failures.append(message)

scripts = root / 'Assets/@Project/01.Scripts'
check(scripts.is_dir(), 'Missing project script root')
check(not (root / 'Assets/Script').exists(), 'Old script root remains')
check(not (root / 'Assets/Scenes').exists(), 'Old scene root remains')
report = root / 'Docs/Architecture/ScriptMigration.json'
migrations = json.loads(report.read_text())
check(len(migrations) == 46, 'Expected 46 original script mappings')
for entry in migrations:
    target = root / entry['new']
    check(target.is_file(), 'Missing script: ' + entry['new'])
    meta = Path(str(target) + '.meta')
    check(meta.is_file() and 'guid: ' + entry['guid'] in meta.read_text(), 'Changed script GUID: ' + entry['new'])

for path in scripts.rglob('*.cs'):
    source = path.read_text(encoding='utf-8')
    check('\ufffd' not in source, 'Invalid source encoding: ' + str(path))
    if 'Tests' in path.parts or 'Editor' in path.parts:
        continue
    check(not re.search(r'\bvoid\s+(?:Update|LateUpdate|FixedUpdate)\s*\(', source), 'Unity frame callback: ' + str(path))
    check(not re.search(r'\b(?:StartCoroutine|StopCoroutine|IEnumerator|WaitForSeconds)\b', source), 'Coroutine: ' + str(path))
    check('using System.Linq' not in source, 'LINQ: ' + str(path))
    check('Camera.main' not in source, 'Camera.main: ' + str(path))
    check('GameManager.' not in source, 'Legacy static service access: ' + str(path))

assets = root / 'Assets'
guids = {}
for meta in assets.rglob('*.meta'):
    match = re.search(r'^guid: (\w+)', meta.read_text(errors='replace'), re.M)
    if not match:
        continue
    guid = match[1]
    check(guid not in guids, 'Duplicate GUID: ' + str(meta) + ' / ' + str(guids.get(guid)))
    guids[guid] = meta
for path in (assets / '@Project').rglob('*'):
    if path.suffix == '.meta' or path.name.startswith('.'):
        continue
    check(Path(str(path) + '.meta').exists(), 'Missing .meta: ' + str(path))

build = (root / 'ProjectSettings/EditorBuildSettings.asset').read_text()
for scene in re.findall(r'^    path: (.+)$', build, re.M):
    check((root / scene).is_file(), 'Missing build scene: ' + scene)
scene = (assets / '@Project/00.Scenes/Professor_House.unity').read_text()
mesh_count = references = 0
for asset in (assets / '@Project/00.Scenes/Professor_House/Meshes').glob('*.asset'):
    guid = re.search(r'guid: (\w+)', Path(str(asset) + '.meta').read_text())[1]
    ids = set(re.findall(r'^--- !u!\d+ &(-?\d+)', asset.read_text(), re.M))
    refs = re.findall(r'fileID: (-?\d+), guid: ' + guid, scene)
    check(not set(refs) - ids, 'Invalid mesh subasset reference: ' + str(asset))
    mesh_count += len(ids)
    references += len(refs)
check(mesh_count == 820 and references == 2663, 'Professor_House mesh contract changed')
check((root / 'ProjectSettings/ProjectVersion.txt').read_text().startswith('m_EditorVersion: ' + args.editor_version), 'Unexpected original Unity version')
print(f'Scripts mapped: {len(migrations)}; mesh assets: {mesh_count}; mesh references: {references}; failures: {len(failures)}')
for failure in failures:
    print(failure)
sys.exit(bool(failures))
