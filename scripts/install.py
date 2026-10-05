#!/usr/bin/env python3
"""Install into the detected Steam game. Run with the game closed."""
import argparse, os, shutil, pathlib, re, subprocess
p=argparse.ArgumentParser()
p.add_argument('--game-dir', type=pathlib.Path, default=pathlib.Path.home()/'.var/app/com.valvesoftware.Steam/data/Steam/steamapps/common/REPO')
p.add_argument('--proton-prefix', type=pathlib.Path, help='Optional Proton pfx directory; enables native winhttp for REPO.exe only')
a=p.parse_args(); root=pathlib.Path(__file__).resolve().parent.parent
if subprocess.run(['pgrep','-f','^.*[ /]REPO.exe$'],stdout=subprocess.DEVNULL).returncode==0:
    raise SystemExit('Close R.E.P.O. before installing.')
if not (a.game_dir/'REPO.exe').is_file(): raise SystemExit('REPO.exe not found')
dll=root/'dist/RepoEmoteWheel.dll'
if not dll.is_file(): raise SystemExit('Run scripts/build.sh first')
if not (a.game_dir/'BepInEx/core/BepInEx.dll').exists():
    loader=root/'.tools/bepinex'
    if not (loader/'winhttp.dll').is_file(): raise SystemExit('Run scripts/bootstrap.sh first')
    for name in ('BepInEx','winhttp.dll','doorstop_config.ini'):
        target=a.game_dir/name
        if target.exists(): raise SystemExit(f'Refusing to overwrite existing loader file: {target}')
    shutil.copytree(loader/'BepInEx',a.game_dir/'BepInEx')
    for name in ('winhttp.dll','doorstop_config.ini'): shutil.copy2(loader/name,a.game_dir/name)
# Application..cctor can run before Unity registers plugin Update/OnGUI callbacks.
# Use the MonoBehaviour entrypoint for this Unity game, preserving other settings.
loader_config=a.game_dir/'BepInEx/config/BepInEx.cfg'
loader_config.parent.mkdir(parents=True,exist_ok=True)
config_text=loader_config.read_text() if loader_config.exists() else ''
section_match=re.search(r'^\[Preloader\.Entrypoint\]\s*$',config_text,re.M)
if section_match:
    start=section_match.end()
    end=config_text.find('\n[',start)
    if end<0: end=len(config_text)
    body=config_text[start:end]
    type_match=re.search(r'^Type\s*=\s*(.*?)\s*$',body,re.M)
    if not type_match or type_match.group(1)=='Application':
        if loader_config.exists():
            backup=loader_config.with_suffix('.cfg.before-emote-wheel-entrypoint')
            if not backup.exists(): shutil.copy2(loader_config,backup)
        if type_match: body=body[:type_match.start()]+'Type = MonoBehaviour'+body[type_match.end():]
        else: body+='\nType = MonoBehaviour\n'
        loader_config.write_text(config_text[:start]+body+config_text[end:])
else:
    if loader_config.exists():
        backup=loader_config.with_suffix('.cfg.before-emote-wheel-entrypoint')
        if not backup.exists(): shutil.copy2(loader_config,backup)
    loader_config.write_text(config_text+'\n[Preloader.Entrypoint]\nAssembly = UnityEngine.CoreModule.dll\nType = MonoBehaviour\nMethod = .cctor\n')
target=a.game_dir/'BepInEx/plugins/RepoEmoteWheel'; target.mkdir(parents=True,exist_ok=True)
shutil.copy2(dll,target/dll.name)
if a.proton_prefix:
    registry=a.proton_prefix/'user.reg'
    text=registry.read_text()
    section=r'[Software\\Wine\\AppDefaults\\REPO.exe\\DllOverrides]'
    backup=registry.with_name('user.reg.before-emote-wheel')
    if not backup.exists(): shutil.copy2(registry,backup)
    if section in text:
        start=text.index(section)+len(section)
        end=text.find('\n[',start)
        if end<0: end=len(text)
        body=text[start:end]
        if re.search(r'^"winhttp"=',body,re.M):
            body=re.sub(r'^"winhttp"=.*$', '"winhttp"="native,builtin"',body,flags=re.M)
        else: body+='\n"winhttp"="native,builtin"\n'
        registry.write_text(text[:start]+body+text[end:])
    else:
        with registry.open('a') as f: f.write('\n'+section+'\n"winhttp"="native,builtin"\n')
    print('Enabled native winhttp for REPO.exe; registry backup:',backup)
print('Installed:',target/dll.name)
