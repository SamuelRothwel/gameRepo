# Gameplay test launcher

Set `godotDotNetExecutable` in `godot-path.json` to a Godot 4 .NET executable, or pass it temporarily:

```powershell
.\tools\run-gameplay-tests.ps1 -Suite moba-projectile-arena -GodotPath 'C:\path\to\Godot_v4.2.2-stable_mono_win64.exe'
```

The runner launches Godot headlessly, runs the requested suite, writes one `GAMEPLAY_TEST_REPORT { ... }` JSON line to stdout, and returns `0` when every test passes or `1` on a test failure. A missing/invalid executable returns `2`.
