package warewolf.studio.android

import android.annotation.SuppressLint
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.view.ViewGroup
import android.webkit.CookieManager
import android.webkit.WebStorage
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.activity.ComponentActivity
import androidx.activity.compose.BackHandler
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.safeDrawingPadding
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.AndroidView
import warewolf.studio.app.ACCESS_COOKIE
import warewolf.studio.app.App
import warewolf.studio.app.AppHost
import warewolf.studio.app.SettingsStore
import warewolf.studio.app.StudioAddress
import kotlin.coroutines.resume
import kotlin.coroutines.suspendCoroutine

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        val host = AndroidHost(applicationContext, StudioAddress(StudioAddress.DEFAULT))
        setContent { App(host) }
    }
}

/**
 * The Android host. Sign-in and UIs share the app's WebView cookie store (CookieManager), so the
 * Cloudflare Access session the sign-in page gets is also what the UI pages and the app's own API
 * calls use. Edit goes to the default browser, whose own studio sign-in is reused (Plan.md).
 */
private class AndroidHost(private val context: Context, override val studio: StudioAddress) : AppHost {
    override val settings: SettingsStore = PreferencesStore(context)

    override suspend fun accessToken(): String? = accessCookie()

    override suspend fun clearSession() = suspendCoroutine { continuation ->
        // The studio keeps its MSAL tokens in the WebView's local/session storage.
        WebStorage.getInstance().deleteAllData()
        val cookies = CookieManager.getInstance()
        cookies.removeAllCookies {
            cookies.flush()
            continuation.resume(Unit)
        }
    }

    override fun openInBrowser(url: String) {
        context.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(url)).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK))
    }

    @Composable
    override fun SignInPage(onSignedIn: () -> Unit, onCancel: () -> Unit) {
        BackHandler(onBack = onCancel)
        StudioWebView(studio.signInUrl, Modifier.fillMaxSize().safeDrawingPadding()) { url ->
            // Access, then the studio's backend, send the browser back to a studio page once the session exists.
            if (studio.isStudioPage(url) && accessCookie() != null) {
                CookieManager.getInstance().flush()
                onSignedIn()
            }
        }
    }

    @Composable
    override fun UiPage(url: String, title: String, onBack: () -> Unit) {
        BackHandler(onBack = onBack)
        StudioWebView(url, Modifier.fillMaxSize().safeDrawingPadding())
    }

    private fun accessCookie(): String? =
        CookieManager.getInstance().getCookie(studio.origin)
            ?.split(';')
            ?.map { it.trim() }
            ?.firstOrNull { it.startsWith("$ACCESS_COOKIE=") }
            ?.substringAfter('=')
            ?.takeIf { it.isNotBlank() }
}

@SuppressLint("SetJavaScriptEnabled")
@Composable
private fun StudioWebView(url: String, modifier: Modifier, onPageFinished: (String) -> Unit = {}) {
    AndroidView(
        modifier = modifier,
        factory = { context ->
            WebView(context).apply {
                // Without this the WebView keeps the WRAP_CONTENT default from AndroidView, and sizes its
                // viewport to the page: CSS 100vh becomes 0 and the studio's 100vh layout collapses (white page).
                layoutParams = ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT)
                settings.javaScriptEnabled = true
                settings.domStorageEnabled = true
                // MSAL's silent token renewal runs login.microsoftonline.com in a hidden frame.
                CookieManager.getInstance().setAcceptThirdPartyCookies(this, true)
                webViewClient = object : WebViewClient() {
                    override fun onPageFinished(view: WebView, finishedUrl: String) = onPageFinished(finishedUrl)
                }
                loadUrl(url)
            }
        },
        onRelease = { it.destroy() },
    )
}

private class PreferencesStore(context: Context) : SettingsStore {
    private val preferences = context.getSharedPreferences("warewolf-studio", Context.MODE_PRIVATE)

    override fun get(key: String): String? = preferences.getString(key, null)

    override fun set(key: String, value: String?) {
        preferences.edit().apply { if (value == null) remove(key) else putString(key, value) }.apply()
    }
}
