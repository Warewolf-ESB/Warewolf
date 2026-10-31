package warewolf.studio.app

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.safeDrawingPadding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.FilterChip
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import kotlinx.coroutines.launch

/** What each platform supplies: its settings, the Access session, the browser and its WebViews. */
interface AppHost {
    val studio: StudioAddress
    val settings: SettingsStore

    /** The Cloudflare Access session ([ACCESS_COOKIE]) the sign-in page left in the WebView's cookies, if any. */
    suspend fun accessToken(): String?

    /**
     * Removes the sign-in the WebViews keep on the device: the Access cookie and the studio's own
     * storage (its MSAL tokens). The phone browser's sign-in, which Edit uses, isn't the app's to clear.
     */
    suspend fun clearSession()

    /** Opens [url] in the phone's default browser, so the studio's own sign-in there is reused (Edit). */
    fun openInBrowser(url: String)

    /** Signs in through Cloudflare Access at [StudioAddress.signInUrl]; [onSignedIn] once the session exists. */
    @Composable
    fun SignInPage(onSignedIn: () -> Unit, onCancel: () -> Unit)

    /** Runs a UI fullscreen at [url] ([uiPageUrl]), with no app chrome; [onBack] returns to the list. */
    @Composable
    fun UiPage(url: String, title: String, onBack: () -> Unit)
}

private sealed interface Screen {
    data object Home : Screen
    data object SignIn : Screen
    data class Ui(val ui: OrganizationUiSummary) : Screen
}

/** Warewolf orange, as the Web Studio's buttons use it (`styles.css` .btn-ww-orange: --ww-accent, --ww-accent-dark). */
private val WarewolfOrange = Color(0xFFF4710E)
private val WarewolfOrangeDark = Color(0xFFB34600)

/** Material's default (purple) scheme with Warewolf orange for buttons, chips and progress. */
private val WarewolfColors = lightColorScheme(
    primary = WarewolfOrange,
    onPrimary = Color.White,
    primaryContainer = Color(0xFFFFDCC6),
    onPrimaryContainer = WarewolfOrangeDark,
    secondaryContainer = Color(0xFFFFDCC6),
    onSecondaryContainer = WarewolfOrangeDark,
)

@Composable
fun App(host: AppHost) {
    val model = remember(host) { HomeModel(StudioApi(studioHttpClient(), host.studio, host::accessToken), AutoStartSettings(host.settings), host::clearSession) }
    var screen by remember { mutableStateOf<Screen>(Screen.Home) }
    // Bumped to reload the list (after sign-in, or Retry); the first load may open the auto-start UI.
    var loads by remember { mutableIntStateOf(0) }

    MaterialTheme(colorScheme = WarewolfColors) {
        Surface(Modifier.fillMaxSize()) {
            LaunchedEffect(loads) {
                model.load(launch = loads == 0)?.let { screen = Screen.Ui(it) }
            }
            when (val current = screen) {
                Screen.Home -> HomeScreen(host, model, onOpen = { screen = Screen.Ui(it) }, onSignIn = { screen = Screen.SignIn }, onRetry = { loads++ })
                Screen.SignIn -> host.SignInPage(onSignedIn = { screen = Screen.Home; loads++ }, onCancel = { screen = Screen.Home })
                is Screen.Ui -> host.UiPage(uiPageUrl(host.studio, current.ui.id), current.ui.name, onBack = { screen = Screen.Home })
            }
        }
    }
}

@Composable
private fun HomeScreen(host: AppHost, model: HomeModel, onOpen: (OrganizationUiSummary) -> Unit, onSignIn: () -> Unit, onRetry: () -> Unit) {
    val state by model.state.collectAsState()
    var picking by remember { mutableStateOf<List<OrganizationUiWorkflow>?>(null) }
    var confirmingSignOut by remember { mutableStateOf(false) }
    val scope = rememberCoroutineScope()

    Column(Modifier.fillMaxSize().safeDrawingPadding().padding(16.dp)) {
        when (val s = state) {
            HomeState.Loading -> Box(Modifier.fillMaxSize(), contentAlignment = Alignment.Center) { CircularProgressIndicator() }
            HomeState.SignedOut -> Message("Sign in to Warewolf", "Sign in with your Microsoft account to see your organization's UIs.") {
                Button(onClick = onSignIn) { Text("Sign in") }
            }
            is HomeState.Failed -> {
                // Also how an account without an organization switches to another one.
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.End) {
                    TextButton(onClick = { confirmingSignOut = true }) { Text("Sign out") }
                }
                Message("Can't show your UIs", s.message) {
                    Button(onClick = onRetry) { Text("Retry") }
                }
            }
            is HomeState.Ready -> {
                Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
                    Text(s.organization.orgName.ifBlank { "Your UIs" }, Modifier.weight(1f), style = MaterialTheme.typography.headlineSmall)
                    TextButton(onClick = { confirmingSignOut = true }) { Text("Sign out") }
                }
                s.notice?.let { notice ->
                    Card(Modifier.fillMaxWidth().padding(top = 12.dp).clickable { model.dismissNotice() }) {
                        Text(notice, Modifier.padding(12.dp), style = MaterialTheme.typography.bodyMedium)
                    }
                }
                if (s.uis.isEmpty()) {
                    Message("No UIs yet", "Your organization hasn't published any UIs you can open. An Administrator publishes them through the Warewolf MCP server.") {
                        OutlinedButton(onClick = onRetry) { Text("Refresh") }
                    }
                } else {
                    LazyColumn(Modifier.padding(top = 12.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                        items(s.uis, key = { it.id }) { ui ->
                            UiRow(
                                ui = ui,
                                autoStarts = ui.id == s.autoStartId,
                                onOpen = { onOpen(ui) },
                                onToggleAutoStart = { model.toggleAutoStart(ui.id) },
                                onEdit = {
                                    when (val target = editTarget(host.studio, ui)) {
                                        EditTarget.Nothing -> Unit
                                        is EditTarget.Open -> host.openInBrowser(target.url)
                                        is EditTarget.Pick -> picking = target.workflows
                                    }
                                },
                            )
                        }
                    }
                }
            }
        }
    }

    if (confirmingSignOut) {
        AlertDialog(
            onDismissRequest = { confirmingSignOut = false },
            title = { Text("Sign out of Warewolf?") },
            text = { Text("You'll need to sign in again to open your organization's UIs.") },
            confirmButton = {
                TextButton(onClick = { confirmingSignOut = false; scope.launch { model.signOut() } }) { Text("Sign out") }
            },
            dismissButton = { TextButton(onClick = { confirmingSignOut = false }) { Text("Cancel") } },
        )
    }

    picking?.let { workflows ->
        AlertDialog(
            onDismissRequest = { picking = null },
            title = { Text("Edit which workflow?") },
            text = {
                LazyColumn {
                    items(workflows, key = { it.key }) { workflow ->
                        Column(Modifier.fillMaxWidth().clickable { picking = null; host.openInBrowser(workflowEditUrl(host.studio, workflow)) }.padding(vertical = 10.dp)) {
                            Text(workflow.workflow, style = MaterialTheme.typography.bodyLarge)
                            Text(StudioAddress(workflow.server).host, style = MaterialTheme.typography.bodySmall)
                        }
                    }
                }
            },
            confirmButton = { TextButton(onClick = { picking = null }) { Text("Cancel") } },
        )
    }
}

@Composable
private fun UiRow(ui: OrganizationUiSummary, autoStarts: Boolean, onOpen: () -> Unit, onToggleAutoStart: () -> Unit, onEdit: () -> Unit) {
    Card(Modifier.fillMaxWidth().clickable(onClick = onOpen)) {
        Column(Modifier.padding(16.dp)) {
            Text(ui.name, style = MaterialTheme.typography.titleMedium)
            ui.description?.takeIf { it.isNotBlank() }?.let {
                Text(it, style = MaterialTheme.typography.bodyMedium, maxLines = 3, overflow = TextOverflow.Ellipsis)
            }
            if (ui.updatedOn.isNotBlank()) Text("Updated ${ui.updatedOn}", style = MaterialTheme.typography.bodySmall)
            Row(Modifier.padding(top = 8.dp), verticalAlignment = Alignment.CenterVertically) {
                Button(onClick = onOpen) { Text("Open") }
                Spacer(Modifier.width(8.dp))
                FilterChip(selected = autoStarts, onClick = onToggleAutoStart, label = { Text("Auto-start") })
                Spacer(Modifier.width(8.dp))
                if (ui.workflows.isNotEmpty()) OutlinedButton(onClick = onEdit) { Text("Edit") }
            }
        }
    }
}

@Composable
private fun Message(title: String, body: String, action: @Composable () -> Unit) {
    Column(Modifier.fillMaxWidth().padding(top = 32.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        Text(title, style = MaterialTheme.typography.headlineSmall)
        Text(body, style = MaterialTheme.typography.bodyMedium)
        action()
    }
}
