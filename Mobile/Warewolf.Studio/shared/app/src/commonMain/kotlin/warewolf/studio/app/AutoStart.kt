package warewolf.studio.app

/** The device's own key/value settings (SharedPreferences, NSUserDefaults, or memory on the desktop). */
interface SettingsStore {
    fun get(key: String): String?

    /** `null` removes the key. */
    fun set(key: String, value: String?)
}

class MemorySettingsStore : SettingsStore {
    private val values = mutableMapOf<String, String>()
    override fun get(key: String): String? = values[key]
    override fun set(key: String, value: String?) {
        if (value == null) values.remove(key) else values[key] = value
    }
}

/**
 * The UI that opens fullscreen when the app starts: at most one per organization, kept on the
 * device only (Plan.md, App layout). Setting it on another UI moves it there.
 */
class AutoStartSettings(private val store: SettingsStore) {
    fun get(orgId: String): String? = store.get(key(orgId))

    fun set(orgId: String, uiId: String?) = store.set(key(orgId), uiId)

    /** Turns it on for [uiId], or off when [uiId] already has it. Returns the UI that now has it, if any. */
    fun toggle(orgId: String, uiId: String): String? {
        val next = if (get(orgId) == uiId) null else uiId
        set(orgId, next)
        return next
    }

    private fun key(orgId: String) = "autoStart.$orgId"
}

/** Where the app goes once it has the organization's UIs. */
sealed interface LaunchTarget {
    data class OpenUi(val ui: OrganizationUiSummary) : LaunchTarget

    /** [notice] says why the auto-start UI didn't open, when it was set but has gone. */
    data class ShowList(val notice: String? = null) : LaunchTarget
}

/**
 * Opens the auto-start UI when it's set and still in the list. A UI that has gone (deleted, or the
 * caller's View revoked) clears the setting, and the list says so.
 */
fun launchTarget(autoStart: AutoStartSettings, orgId: String, uis: List<OrganizationUiSummary>): LaunchTarget {
    val uiId = autoStart.get(orgId) ?: return LaunchTarget.ShowList()
    uis.firstOrNull { it.id == uiId }?.let { return LaunchTarget.OpenUi(it) }
    autoStart.set(orgId, null)
    return LaunchTarget.ShowList("The UI set to open on start is no longer available, so it has been turned off.")
}
