@file:OptIn(ExperimentalForeignApi::class)

package warewolf.studio.app

// Not yet compiled: Kotlin/Native iOS targets only build on macOS (README.md). The first Mac build
// may need small interop fixes here.

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.safeDrawingPadding
import androidx.compose.material3.TextButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.UIKitView
import androidx.compose.ui.window.ComposeUIViewController
import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.cinterop.ObjCSignatureOverride
import kotlinx.coroutines.suspendCancellableCoroutine
import platform.CoreGraphics.CGRectMake
import platform.Foundation.NSDate
import platform.Foundation.NSHTTPCookie
import platform.Foundation.NSURL
import platform.Foundation.NSURLRequest
import platform.Foundation.NSUserDefaults
import platform.UIKit.UIApplication
import platform.UIKit.UIViewAutoresizingFlexibleHeight
import platform.UIKit.UIViewAutoresizingFlexibleWidth
import platform.UIKit.UIViewController
import platform.WebKit.WKNavigation
import platform.WebKit.WKNavigationDelegateProtocol
import platform.WebKit.WKWebView
import platform.WebKit.WKWebViewConfiguration
import platform.WebKit.WKWebsiteDataStore
import platform.darwin.NSObject
import kotlin.coroutines.resume

/** The iOS app's root view controller, shown by the Xcode host's SwiftUI/UIKit entry point. */
fun MainViewController(): UIViewController = ComposeUIViewController { App(IosHost(StudioAddress(StudioAddress.DEFAULT))) }

/**
 * The iOS host. Sign-in and UIs use WKWebView's default data store, so they share the Cloudflare
 * Access session the app also sends on its own API calls. Edit opens Safari, the user's browser,
 * whose own studio sign-in is reused (an in-app Safari view would not share its cookies).
 */
private class IosHost(override val studio: StudioAddress) : AppHost {
    override val settings: SettingsStore = UserDefaultsStore()

    override suspend fun accessToken(): String? = suspendCancellableCoroutine { continuation ->
        WKWebsiteDataStore.defaultDataStore().httpCookieStore.getAllCookies { cookies ->
            val token = cookies.orEmpty()
                .filterIsInstance<NSHTTPCookie>()
                .firstOrNull { it.name == ACCESS_COOKIE && studio.receivesCookiesFor(it.domain) }
                ?.value
            continuation.resume(token?.takeIf { it.isNotBlank() })
        }
    }

    override suspend fun clearSession() = suspendCancellableCoroutine { continuation ->
        // Cookies (the Access session) and the studio's local storage (its MSAL tokens), all of it.
        WKWebsiteDataStore.defaultDataStore().removeDataOfTypes(
            WKWebsiteDataStore.allWebsiteDataTypes(),
            modifiedSince = NSDate(timeIntervalSince1970 = 0.0),
        ) { continuation.resume(Unit) }
    }

    override fun openInBrowser(url: String) {
        NSURL.URLWithString(url)?.let { UIApplication.sharedApplication.openURL(it, emptyMap<Any?, Any?>(), null) }
    }

    @Composable
    override fun SignInPage(onSignedIn: () -> Unit, onCancel: () -> Unit) {
        Column(Modifier.fillMaxSize().safeDrawingPadding()) {
            TextButton(onClick = onCancel) { Text("Cancel") }
            // The list reloads afterwards and asks again if Access didn't leave a session.
            StudioWebView(studio.signInUrl, Modifier.fillMaxSize()) { url -> if (studio.isStudioPage(url)) onSignedIn() }
        }
    }

    @Composable
    override fun UiPage(url: String, title: String, onBack: () -> Unit) {
        // iOS has no system Back, so a small Close stays over the UI.
        Box(Modifier.fillMaxSize().safeDrawingPadding()) {
            StudioWebView(url, Modifier.fillMaxSize())
            TextButton(onClick = onBack, modifier = Modifier.align(Alignment.TopEnd)) { Text("Close") }
        }
    }
}

@Composable
private fun StudioWebView(url: String, modifier: Modifier, onPageFinished: (String) -> Unit = {}) {
    // WKWebView holds its navigation delegate weakly, so it's kept here.
    val delegate = remember { PageFinishedDelegate(onPageFinished) }
    UIKitView(
        factory = {
            WKWebView(frame = CGRectMake(0.0, 0.0, 0.0, 0.0), configuration = WKWebViewConfiguration()).apply {
                // Fill whatever Compose sizes it to, so CSS 100vh is never 0 (the Android white page, README.md).
                autoresizingMask = UIViewAutoresizingFlexibleWidth or UIViewAutoresizingFlexibleHeight
                navigationDelegate = delegate
                NSURL.URLWithString(url)?.let { loadRequest(NSURLRequest.requestWithURL(it)) }
            }
        },
        modifier = modifier,
    )
}

private class PageFinishedDelegate(private val onFinished: (String) -> Unit) : NSObject(), WKNavigationDelegateProtocol {
    @ObjCSignatureOverride
    override fun webView(webView: WKWebView, didFinishNavigation: WKNavigation?) {
        webView.URL?.absoluteString?.let(onFinished)
    }
}

private class UserDefaultsStore : SettingsStore {
    private val defaults = NSUserDefaults.standardUserDefaults

    override fun get(key: String): String? = defaults.stringForKey(key)

    override fun set(key: String, value: String?) {
        if (value == null) defaults.removeObjectForKey(key) else defaults.setObject(value, forKey = key)
    }
}
