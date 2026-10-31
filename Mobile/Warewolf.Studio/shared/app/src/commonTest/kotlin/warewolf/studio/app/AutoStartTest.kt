package warewolf.studio.app

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull

class AutoStartTest {
    private val store = MemorySettingsStore()
    private val autoStart = AutoStartSettings(store)
    private val orders = OrganizationUiSummary("ui-1", "Orders")
    private val stock = OrganizationUiSummary("ui-2", "Stock")

    @Test
    fun settingItOnAnotherUiMovesIt() {
        assertEquals("ui-1", autoStart.toggle("org-1", "ui-1"))
        assertEquals("ui-2", autoStart.toggle("org-1", "ui-2"))
        assertEquals("ui-2", autoStart.get("org-1"))
    }

    @Test
    fun togglingTheUiThatHasItTurnsItOff() {
        autoStart.toggle("org-1", "ui-1")

        assertNull(autoStart.toggle("org-1", "ui-1"))
        assertNull(autoStart.get("org-1"))
    }

    @Test
    fun isKeptPerOrganization() {
        autoStart.toggle("org-1", "ui-1")
        autoStart.toggle("org-2", "ui-9")

        assertEquals("ui-1", autoStart.get("org-1"))
        assertEquals("ui-9", autoStart.get("org-2"))
    }

    @Test
    fun opensTheAutoStartUiWhenItIsStillListed() {
        autoStart.set("org-1", "ui-2")

        assertEquals(LaunchTarget.OpenUi(stock), launchTarget(autoStart, "org-1", listOf(orders, stock)))
    }

    @Test
    fun showsTheListWhenNothingAutoStarts() {
        assertEquals(LaunchTarget.ShowList(), launchTarget(autoStart, "org-1", listOf(orders)))
    }

    @Test
    fun clearsAGoneUiAndSaysSo() {
        autoStart.set("org-1", "ui-gone")

        val target = launchTarget(autoStart, "org-1", listOf(orders))

        assertEquals("The UI set to open on start is no longer available, so it has been turned off.", (target as LaunchTarget.ShowList).notice)
        assertNull(autoStart.get("org-1"))
    }
}
