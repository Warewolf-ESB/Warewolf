# Warewolf Mobile Studio

The phone app for an organization's Warewolf UIs (Android and iOS, Kotlin Multiplatform). The plan,
decisions and research record are in [docs/Plan.md](docs/Plan.md).

## What the app does (P2)

- **Sign-in:** Cloudflare Access in an in-app WebView (`/api/studio/login`). The app sends the
  `CF_Authorization` session cookie that Access leaves there on its own calls to `/api/studio/*`.
- **Sign out** (after a confirmation): ends the session at Cloudflare (`/cdn-cgi/access/logout`, best effort)
  and clears the WebViews' cookies and storage on the device. The auto-start setting is kept.
- **UI list (home):** `GET /api/studio/organization-uis`, with Open, Auto-start and Edit on each UI.
- **Auto-start:** at most one UI per organization, kept on the device; it opens fullscreen on launch.
- **Fullscreen UI:** a WebView on the studio's own `/organization-ui/:id?standalone=true` page, so the
  studio's sandbox and engine calls run each UI unchanged. Back (Android) or Close (iOS) returns to the list.
- **Edit:** opens `/workflow?engine=<engine>&path=<workflow>` in the phone's default browser, where the
  studio's own sign-in is reused. A UI with several workflows asks which one first.

The studio is `https://app.warewolf.online` (`StudioAddress.DEFAULT`).

## Layout

| Module | What |
|---|---|
| `shared/app` | Kotlin Multiplatform: models, the studio API client, auto-start, links, the home model and the Compose screens (`commonMain`); the desktop runner (`desktopMain`); the iOS host (`iosMain`) |
| `androidApp` | Android host: the activity, the sign-in and UI WebViews, SharedPreferences |
| `shared/generator`, `eval` | The deprecated System 1 baseline and its evaluation harness (Plan.md), plain JVM |

## Building

Needs JDK 21. The Gradle wrapper fetches everything else.

```powershell
# Shared tests (run on the desktop JVM)
.\gradlew.bat :shared:app:desktopTest

# Android debug APK (androidApp\build\outputs\apk\debug\)
.\gradlew.bat :androidApp:assembleDebug

# The app in a phone-sized desktop window (list, auto-start, Edit; UIs open in the browser).
# Paste the CF_Authorization cookie from a browser signed in to the studio, or set WW_CF_AUTHORIZATION.
.\gradlew.bat :shared:app:run
```

**Android SDK:** platform `android-37.0` and build-tools. Put its path in `local.properties`
(`sdk.dir=...`, not committed) or set `ANDROID_HOME`.

**iOS:** Kotlin/Native iOS targets only build on macOS, so on Windows they are skipped
(`kotlin.native.ignoreDisabledTargets`). `iosMain` has not been compiled yet. On a Mac, build the
`WarewolfStudio` framework (`:shared:app:linkDebugFrameworkIosSimulatorArm64`) and show
`MainViewControllerKt.MainViewController()` from an Xcode app target; that Xcode project doesn't exist yet.

## CI and release

- **Every CI run** (`Dev/.azure/pipeline.yml`, Test stage, `Mobile_Studio_Tests`) runs
  `:shared:app:desktopTest` on `ubuntu-latest` and publishes the results.
- **Main branches only** (develop, 7580\*, 7232\*): `Compile_Release` > `build_release_apk` builds
  `:androidApp:assembleRelease` with `versionName` = the release version just tagged and `versionCode` =
  the build id, signs it with the upload key, checks it with `apksigner`, and publishes the
  `MobileStudioApk` artifact.
- **Release** (`Dev/.azure/pipeline-MOBILE.yml`, "Mobile Studio Release") runs after that stage and
  uploads the APK to the public release container. Its run summary links both:
  - `https://stwwreleases.blob.core.windows.net/releases/WarewolfStudio-<version>.apk` (never overwritten)
  - `https://stwwreleases.blob.core.windows.net/releases/WarewolfStudio-latest.apk` (only moves forward)
- `Dev/.azure/pipeline-MOBILE.Tests.ps1` checks both pipelines offline (Pester 5.7.1 + powershell-yaml).

**Signing.** The release build is signed only when all four of `WW_ANDROID_KEYSTORE` (keystore path),
`WW_ANDROID_KEYSTORE_PASSWORD`, `WW_ANDROID_KEY_ALIAS` and `WW_ANDROID_KEY_PASSWORD` are set; otherwise
it is `androidApp-release-unsigned.apk`. The version comes from `-PwwVersionName` / `-PwwVersionCode`
(defaults `0.1.0` / `1`).

One-time Azure DevOps setup:

1. Create the upload keystore once and keep it (and its passwords) safe: every later APK must be signed
   with the same key to install over the last one.
   ```powershell
   keytool -genkeypair -v -keystore WarewolfStudio-upload.jks -alias warewolf-studio -keyalg RSA -keysize 4096 -validity 10000
   ```
2. Upload it under **Pipelines > Library > Secure files** as `WarewolfStudio-upload.jks` and authorize
   the "Warewolf" pipeline to use it.
3. On the "Warewolf" pipeline, add the variables `AndroidKeystorePassword` (secret), `AndroidKeyAlias`
   and `AndroidKeyPassword` (secret).
4. Create the pipeline "Mobile Studio Release" from `Dev/.azure/pipeline-MOBILE.yml` and authorize it for
   the `AzureStorage-WarewolfReleases` service connection and the "Warewolf" pipeline's artifacts.

## Not proven yet

- Signing in to Microsoft (MSAL) inside the WebView for engine calls from a UI. The studio uses
  redirects, which WebViews handle. Its Azure guest check uses a pop-up, which the WebView doesn't
  open, and some Entra tenants block sign-in in WebViews.
- The whole iOS host. Android has run on one phone (Galaxy S22+, WebView 134): sign-in, the UI list
  and a fullscreen UI. A WebView in Compose's `AndroidView` must get `MATCH_PARENT` layout params; with
  the `WRAP_CONTENT` default, CSS `100vh` is 0 and the studio's layout collapses to a white page. The iOS
  host may need the same check.

**Debugging the app's WebViews:** a debug build can be inspected from `chrome://inspect`, or with
`adb forward tcp:9333 localabstract:webview_devtools_remote_<pid>` and the DevTools protocol on port 9333.
