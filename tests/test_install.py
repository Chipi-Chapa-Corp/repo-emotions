"""Run the real development installer in disposable profiles, with no game process."""
import contextlib
import io
from pathlib import Path
import runpy
import shutil
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch


class InstallTests(unittest.TestCase):
    def test_no_prior_config_required_or_rewritten(self):
        configurations = (
            None,
            b"[Preloader.Entrypoint]\r\nType = Application\r\n[Logging.Console]\r\nEnabled = true\r\n",
            b"[Preloader.Entrypoint]\nType = MonoBehaviour\n",
        )
        for initial in configurations:
            with self.subTest(config=initial), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                installer = root / "scripts/install.py"
                installer.parent.mkdir()
                shutil.copyfile(Path(__file__).resolve().parents[1] / "scripts/install.py", installer)
                dll = root / "dist/RepoEmoteWheel.dll"
                dll.parent.mkdir()
                dll.write_bytes(b"test plugin payload")
                profile = root / "profile"
                core = profile / "BepInEx/core/BepInEx.dll"
                core.parent.mkdir(parents=True)
                core.write_bytes(b"dependency supplied by manager")
                config = profile / "BepInEx/config/BepInEx.cfg"
                binding = profile / "BepInEx/config/local.repo.mmbemotewheel.cfg"
                if initial is not None:
                    config.parent.mkdir(parents=True)
                    config.write_bytes(initial)
                    binding.write_bytes(b"[Emote wheel]\nButton = Q\nSelf-view = B\n")
                before = {p.relative_to(profile): p.read_bytes() for p in profile.rglob("*") if p.is_file()}
                with patch.object(sys, "argv", [str(installer), "--profile-dir", str(profile)]), \
                        patch("subprocess.run", return_value=subprocess.CompletedProcess(["pgrep"], 1)), \
                        contextlib.redirect_stdout(io.StringIO()):
                    runpy.run_path(str(installer), run_name="__main__")
                after = {p.relative_to(profile): p.read_bytes() for p in profile.rglob("*") if p.is_file()}
                expected = dict(before)
                expected[Path("BepInEx/plugins/RepoEmoteWheel/RepoEmoteWheel.dll")] = dll.read_bytes()
                self.assertEqual(after, expected, "installation must only add the plugin DLL")


if __name__ == "__main__":
    unittest.main()
