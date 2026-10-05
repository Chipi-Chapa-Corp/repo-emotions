#!/usr/bin/env python3
"""Build a validated Thunderstore ZIP from an explicit file list (no game binaries)."""
from pathlib import Path
import hashlib
import json
import re
import struct
import subprocess
import xml.etree.ElementTree as ET
import zipfile

root = Path(__file__).resolve().parent.parent
subprocess.run([str(root / 'scripts/build.sh')], cwd=root, check=True)
metadata = json.loads((root / 'package/manifest.json').read_text())
version = ET.parse(root / 'src/RepoEmoteWheel.csproj').findtext('./PropertyGroup/Version')
assert metadata['version_number'] == version, 'Manifest and project versions differ'
assert re.fullmatch(r'\d+\.\d+\.\d+', version), 'Invalid semantic version'
assert re.fullmatch(r'[A-Za-z0-9_]{1,128}', metadata['name']), 'Invalid package name'
assert 0 < len(metadata['description']) <= 250, 'Invalid description length'
assert isinstance(metadata['website_url'], str), 'website_url must be a string'
assert metadata['dependencies'] and all(re.fullmatch(r'[A-Za-z0-9_]+-[A-Za-z0-9_]+-\d+\.\d+\.\d+', d) for d in metadata['dependencies'])
plugin = (root / 'src/Plugin.cs').read_text()
assert f'"{version}")]' in plugin, 'Plugin and package versions differ'
icon = (root / 'package/icon.png').read_bytes()
assert icon[:8] == b'\x89PNG\r\n\x1a\n' and struct.unpack('>II', icon[16:24]) == (256, 256), 'Icon must be a 256x256 PNG'
files = {name: root / 'package' / name for name in ('manifest.json', 'README.md', 'CHANGELOG.md', 'icon.png')}
files['BepInEx/plugins/RepoEmoteWheel/RepoEmoteWheel.dll'] = root / 'dist/RepoEmoteWheel.dll'
destination = root / 'dist' / f"{metadata['name']}-{version}.zip"
with zipfile.ZipFile(destination, 'w', zipfile.ZIP_DEFLATED) as archive:
    for name, path in files.items():
        data = path.read_bytes()
        if name.endswith(('.md', '.json')): data.decode('utf-8')
        archive.writestr(name, data)
with zipfile.ZipFile(destination) as archive:
    assert archive.testzip() is None, 'Corrupt archive'
    assert set(archive.namelist()) == set(files), 'Unexpected ZIP contents'
    for name, path in files.items(): assert archive.read(name) == path.read_bytes(), f'Content mismatch: {name}'
checksum = hashlib.sha256(destination.read_bytes()).hexdigest()
(root / 'dist' / f'{destination.name}.sha256').write_text(f'{checksum}  {destination.name}\n')
print(f'Validated: {destination} ({destination.stat().st_size:,} bytes)')
for name in files: print(f'  {name}')
