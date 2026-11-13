# AGENTS.md

Unity **6000.3.25f1** (Unity 6.3 LTS) URP starter (C#). Verify C# with Unity batchmode (no wrapper script, no git hooks). Style: root `.editorconfig` (hints only, not a gate). CI: GitHub Actions under `.github/workflows/` (PR compile gate `unity-compile` + itch.io deploy + Drive track/retrieve).

## Sources of truth

- `ARCHITECTURE.md` — folder rules, base classes, lifecycle, naming. Prefer over README.
- `README.md` — pitch + setup; only scene is `Boot.unity`.
- `GITHUB_WORKFLOWS.md` — secrets/vars, deploy inputs, Discord + release changelog.
- Executable wins over docs: `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `.gitattributes`, `.github/workflows/`.

## Layout

```
Assets/_Game/          # all game content
  00_Scripts/          # Core, Systems, Manager, Game, UI, Utils
  01_Objects/          # Prefabs/ + Data/ (SO variants)
  02_Art/Sprite/       # Drive-synced sprites (retrieve wipes + restores)
  03_Audio/            # GameAudioMixer.mixer (+ Music/, SFX/ Drive clips)
  04_Scenes/           # Boot.unity only (Build Settings)
  05_Settings/         # URP, Input
Assets/Editor/         # BuildScript.cs (CI -executeMethod) + GameAssetCreator
Assets/_Vendor/        # third-party — do not edit; do not commit churn
.github/workflows/     # unity-compile, track, retrieve, unity-itchio-deploy
scripts/test_state.py  # Drive state regressions
retrieve.py, track.py, core/, state/, requirements.txt  # Drive -> assets tooling
```

- Namespaces mirror folders: `Slafurry.Systems.*`, `Slafurry.Core.*`, `Slafurry.Utils.*`. Folder is `Systems/` (plural).
- **No `.asmdef`** — every non-`Editor` script compiles into `Assembly-CSharp`. Editor-only code must live under `Editor/`.
- Max 6 subfolders per folder (ARCHITECTURE.md).
- Serialization: Force Text YAML + Visible Meta Files. **Commit `.meta` with every new/renamed asset**; never hand-edit GUIDs.
- **No Git LFS** — binaries are plain git blobs (`.gitattributes` `binary`). Do not reintroduce `filter=lfs`. `*.csproj`/`*.sln` are gitignored, Editor-generated — never hand-edit or commit.

## Boot / lifecycle (easy to get wrong)

- Only scene: `Boot.unity` (`EditorBuildSettings` lists only it). `LoadingSystem` must exist first — on `====== LOADING ======` root GameObject. New systems attach to `====== SYSTEM ======` root (or their own root), alongside `SceneLoader`, `SaveSystem`, `InputHub`, `PauseSystem`, `LocalizationSystem`.
- `Singleton<T>.Awake()` sets `Instance` + calls `OnSingletonAwake()` — does **not** register. Registration to `LoadingSystem` happens in `Singleton<T>.Start()` (for `GameSystem<T>`) or `Manager.Awake()`. Never touch other objects in `Awake`/`Initialize`.
- `Initialize()` = `IEnumerator` coroutine, internal setup only, 10s per-object timeout (logs warning, continues). Cross-object wiring only in `PostInitialize()` (after every `Initialize`). Avoid `Start()` (guard `_isReady` if unavoidable).
- `IInitializable.Priority` orders boot (lower first). Late registrants after boot → one-frame delayed batch.
- Base class: cross-scene singleton → `GameSystem<T>` (`DontDestroyOnLoad`); scene-bound singleton → `LocalSingleton<T>`; session coordinator → `Manager` (registers with `GameManager` in `PostInitialize` via abstract `RegisterToGameManager`/`OnPostInitialize`).
- **`GameManager` does not exist** — `Manager.cs`/`ARCHITECTURE.md` still describe it; any `Manager` subclass will not compile until you add one (or drop the abstract hooks). No subclasses exist today.
- `BootstrapLoader` (Systems/Scene) optional: wire `systemsToWaitFor` + `targetSceneName` if you add a boot scene. Default `targetSceneName` is `"MainMenu"` (not in Build Settings).

## Static facades (prefer over `.Instance` chains)

`Audio` · `Save` · `Pause` · `Localize` · `SceneSystem`

- `Audio.PlayMusic(...)` / `PlaySFX2D(...)` / `PlaySFX3D(...)`. Linear volume → dB via `Mathf.Log10(x) * 20` before `AudioMixer.SetFloat`.
- `Save.To<T>(data, fileName)` / `Save.From<T>(fileName, fallback)` — Newtonsoft.Json → `Application.persistentDataPath/{fileName}.json` (caller omits `.json`).
- `Pause.On(key)` / `Pause.Off(key)` / `Pause.Toggle(key)` — **key stack** (`HashSet`, default `"Global"`), not a bool; multiple holders keep pause.
- `Localize.Text(key)` / `Localize.SetLanguage(code)`.
- `SceneSystem.Load(sceneName)` / `SceneSystem.Load(SceneReference)`.
- `GameFeel` is a **MonoBehaviour**, not a static facade.
- `Controls` does **not** exist. Input System only — no `Input.GetKey*` anywhere. Actions: `Assets/_Game/05_Settings/Input/Main Input.inputactions` → generated `Main Input.cs` (regenerate in Editor after editing).

## Pause / time

- `Time.timeScale` is single source of truth for pausing. Gameplay/animation uses `Time.deltaTime` (auto-freezes); UI/gamefeel that must run while paused uses `Time.unscaledDeltaTime` / `WaitForSecondsRealtime`.

## Editor

- Menu **Slafurry → Game Data** (GameAssetCreator). Types for that window use `[GameAssetCreator(category, displayName, order)]`; other SOs use `[CreateAssetMenu]`.
- `GameAudioMixer.mixer` — commit it + local `state/` to `main` before relying on Drive workflows.

## GitHub Actions — config in workflow-level `env:`

All four workflows declare config in `env:`; `run:` steps read `"$NAME"` only, never hardcoded.

**Secrets** (credentials): `GEMINI_API_KEY`, `GOOGLE_SERVICE_ACCOUNT_JSON_B64`, `DISCORD_WEBHOOK_URL`, `DISCORD_WEBHOOK_URL_DEPLOY`, `UNITY_EMAIL`, `UNITY_PASSWORD`, `BUTLER_API_KEY`, `PR_PAT` (retrieve only; compile/deploy use default `GITHUB_TOKEN`).

**Variables** (non-credential): `DRIVE_SPRITE_FOLDER_ID`, `DRIVE_AUDIO_FOLDER_ID`, `GEMINI_MODEL`, `GEMINI_PERSONA`, `GEMINI_LANGUAGE`, `BOT_GIT_USERNAME`, `BOT_GIT_EMAIL`, `ITCH_*`, `GAME_DISPLAY_NAME`, `UNITY_PROJECT_PATH`.

- `unity-compile` — PR-only gate (paths `Assets/`, `Packages/`, `ProjectSettings/`, the workflow file). Batchmode compile; fails on `error CS` or non-successful exit. Require in branch protection.
- `track` — daily schedule (`0 23 * * *`) + manual. Scans Drive sprite/audio, persists `state/` to `main` (`[skip ci]`) so `deleted_in_drive` flags stick.
- `retrieve` — manual. `rm -rf`s `state/`, `Sprite`, `Audio` then restores from `origin/main` via `git archive`; downloads new Drive files; Unity batchmode import generates `.meta`; commits `state/` to `main` immediately; assets → disposable `chore/asset` PR (`chore/asset` recreated from `main` each run — never push manually). Requires `UNITY_EMAIL`/`UNITY_PASSWORD`.
- `unity-itchio-deploy` — manual inputs: `platforms` (All/Windows/macOS/WebGL), `release_type` (release/beta/alpha), `release_tag` (`1.2.3` → `v1.2.3-release` etc.; empty = unversioned, no GitHub Release). Builds via `BuildScript.Build` (`-buildTarget`), pushes to itch.io via butler, writes Gemini release notes + Discord flavor from `git log` changelog.

## Verify changes

- **Style only:** `.editorconfig`.
- **Compile:** run Unity batchmode directly. Pass = exit 0, log contains `Exiting batchmode successfully`, no `error CS` lines. Editor path is not in the repo — read it from `ProjectSettings/ProjectVersion.txt` / your Unity Hub install.

```bash
~/Unity/Hub/Editor/6000.3.25f1/Editor/Unity \
  -batchmode -nographics -quit \
  -projectPath "$PWD" -logFile /tmp/unity-compile.log
grep -n "error CS" /tmp/unity-compile.log   # must print nothing
grep -q "Exiting batchmode successfully" /tmp/unity-compile.log && echo OK
```

- **Drive tooling:** `pip install -r requirements.txt`, then `python3 scripts/test_state.py` after touching `core/state.py` / `track.py` / `retrieve.py`.
- Play-mode behavior still needs a manual Editor run (`Boot.unity`).

## Gotchas

- `.gitattributes` sets `merge=unityyamlmerge` for `*.unity`/`*.asset`/etc., but the driver is **not in git config** — Unity YAML conflict merges fail until UnityYAMLMerge is configured.
- **No git hooks.** `.githooks/pre-push` and `scripts/check-compile.sh` were removed deliberately — do not recreate them; local verification is the batchmode command above.
- Drive bots re-sync from `origin/main` every run; commit `GameAudioMixer.mixer` + `state/` to `main` first.
- `core/state.py` must keep retrieve fields when track merges a fresh listing (`scripts/test_state.py`).
