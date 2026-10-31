package warewolf.studio.app

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.compose.ui.window.rememberWindowState
import androidx.compose.ui.unit.DpSize
import java.awt.Desktop
import java.net.URI

/**
 * Runs the app on a developer's machine, in a phone-sized window, to try the UI list, auto-start and
 * Edit. Desktop has no WebView: sign-in takes the CF_Authorization cookie pasted from a browser that
 * is signed in to the studio (or WW_CF_AUTHORIZATION), and UIs open in the browser. Settings are
 * kept in memory. WW_STUDIO picks another studio.
 */
fun main() = application {
    val host = remember { DesktopHost(StudioAddress(System.getenv("WW_STUDIO") ?: StudioAddress.DEFAULT), System.getenv("WW_CF_AUTHORIZATION")) }
    Window(onCloseRequest = ::exitApplication, title = "Warewolf Studio", state = rememberWindowState(size = DpSize(420.dp, 860.dp))) {
        App(host)
    }
}

private class DesktopHost(override val studio: StudioAddress, private var token: String?) : AppHost {
    override val settings: SettingsStore = MemorySettingsStore()

    override suspend fun accessToken(): String? = token

    override suspend fun clearSession() {
        token = null
    }

    override fun openInBrowser(url: String) = Desktop.getDesktop().browse(URI(url))

    @Composable
    override fun SignInPage(onSignedIn: () -> Unit, onCancel: () -> Unit) {
        var pasted by remember { mutableStateOf("") }
        Column(Modifier.fillMaxSize().padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Text("Sign in to ${studio.host} in your browser, then paste its $ACCESS_COOKIE cookie here.")
            OutlinedButton(onClick = { openInBrowser(studio.signInUrl) }) { Text("Open sign-in") }
            OutlinedTextField(pasted, { pasted = it.trim() }, Modifier.fillMaxWidth(), label = { Text(ACCESS_COOKIE) }, singleLine = true)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                Button(onClick = { token = pasted; onSignedIn() }, enabled = pasted.isNotBlank()) { Text("Continue") }
                OutlinedButton(onClick = onCancel) { Text("Cancel") }
            }
        }
    }

    @Composable
    override fun UiPage(url: String, title: String, onBack: () -> Unit) {
        LaunchedEffect(url) { openInBrowser(url) }
        Column(Modifier.fillMaxSize().padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Text("$title opened in your browser (the desktop has no WebView).")
            Button(onClick = onBack) { Text("Back") }
        }
    }
}
