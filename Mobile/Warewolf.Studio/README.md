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

## Not proven yet

- Signing in to Microsoft (MSAL) inside the WebView for engine calls from a UI. The studio uses
  redirects, which WebViews handle. Its Azure guest check uses a pop-up, which the WebView doesn't
  open, and some Entra tenants block sign-in in WebViews.
- Anything on a real device or emulator, and the whole iOS host.
