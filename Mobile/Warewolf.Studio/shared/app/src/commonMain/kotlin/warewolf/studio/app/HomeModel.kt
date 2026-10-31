package warewolf.studio.app

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

sealed interface HomeState {
    data object Loading : HomeState

    data object SignedOut : HomeState

    data class Failed(val message: String) : HomeState

    data class Ready(
        val organization: StudioOrganization,
        val uis: List<OrganizationUiSummary>,
        val autoStartId: String?,
        val notice: String? = null,
    ) : HomeState
}

/**
 * The UI list (home): who is signed in, their organization's UIs, and which one auto-starts.
 * [clearSession] removes the sign-in the WebViews keep on the device ([AppHost.clearSession]).
 */
class HomeModel(
    private val api: StudioApi,
    private val autoStart: AutoStartSettings,
    private val clearSession: suspend () -> Unit = {},
) {
    private val _state = MutableStateFlow<HomeState>(HomeState.Loading)
    val state: StateFlow<HomeState> = _state.asStateFlow()

    /**
     * (Re)loads the list. With [launch], the first load after the app starts, returns the
     * auto-start UI to open fullscreen, if there is one and it's still in the list.
     */
    suspend fun load(launch: Boolean = false): OrganizationUiSummary? {
        _state.value = HomeState.Loading
        val organization = when (val session = api.session()) {
            is StudioResult.SignedOut -> return signedOut()
            is StudioResult.Failure -> return failed(session.message)
            is StudioResult.Success -> session.value.organization
                ?: return failed("${session.value.email.ifBlank { "This account" }} isn't in an organization yet. Join or create one in the Web Studio.")
        }
        val list = when (val uis = api.organizationUis()) {
            is StudioResult.SignedOut -> return signedOut()
            is StudioResult.Failure -> return failed(uis.message)
            is StudioResult.Success -> uis.value.uis
        }

        val target = if (launch) launchTarget(autoStart, organization.orgId, list) else LaunchTarget.ShowList()
        _state.value = HomeState.Ready(organization, list, autoStart.get(organization.orgId), (target as? LaunchTarget.ShowList)?.notice)
        return (target as? LaunchTarget.OpenUi)?.ui
    }

    /** Auto-start on [uiId], or off when it already has it (only ever one UI). */
    fun toggleAutoStart(uiId: String) {
        val ready = _state.value as? HomeState.Ready ?: return
        _state.value = ready.copy(autoStartId = autoStart.toggle(ready.organization.orgId, uiId), notice = null)
    }

    /**
     * Signs out at Cloudflare (best effort) and on the device. The auto-start setting stays, so it
     * applies again when the user signs back in to the same organization.
     */
    suspend fun signOut() {
        try {
            api.signOut()
        } finally {
            clearSession()
            _state.value = HomeState.SignedOut
        }
    }

    fun dismissNotice() {
        val ready = _state.value as? HomeState.Ready ?: return
        _state.value = ready.copy(notice = null)
    }

    private fun signedOut(): OrganizationUiSummary? {
        _state.value = HomeState.SignedOut
        return null
    }

    private fun failed(message: String): OrganizationUiSummary? {
        _state.value = HomeState.Failed(message)
        return null
    }
}
