package warewolf.studio.app

import kotlin.test.Test
import kotlin.test.assertEquals

class StudioLinksTest {
    private val studio = StudioAddress("https://app.warewolf.online/")

    @Test
    fun normalizesTheStudioAddress() {
        assertEquals("https://app.warewolf.online", studio.origin)
        assertEquals("https://app.warewolf.online", StudioAddress(" app.warewolf.online// ").origin)
        assertEquals("http://localhost:4200", StudioAddress("http://localhost:4200").origin)
        assertEquals("localhost:4200", StudioAddress("http://localhost:4200").host)
    }

    @Test
    fun signsInThroughAccessBackToTheRoot() {
        assertEquals("https://app.warewolf.online/api/studio/login?returnTo=%2F", studio.signInUrl)
    }

    @Test
    fun runsAUiOnItsStandalonePage() {
        assertEquals("https://app.warewolf.online/organization-ui/ui-1?standalone=true", uiPageUrl(studio, "ui-1"))
        assertEquals("https://app.warewolf.online/organization-ui/a%20b?standalone=true", uiPageUrl(studio, "a b"))
    }

    @Test
    fun encodesTheEditorLink() {
        val workflow = OrganizationUiWorkflow("list", " https://wwprofilerlw.azurewebsites.net/ ", "Sales Orders\\List all")

        assertEquals(
            "https://app.warewolf.online/workflow?engine=https%3A%2F%2Fwwprofilerlw.azurewebsites.net%2F&path=Sales%20Orders%5CList%20all",
            workflowEditUrl(studio, workflow),
        )
    }

    @Test
    fun opensTheOnlyWorkflowWithoutAPicker() {
        val only = OrganizationUiWorkflow("list", "https://engine", "Orders/List")

        assertEquals(EditTarget.Open(workflowEditUrl(studio, only)), editTarget(studio, OrganizationUiSummary("ui-1", "Orders", workflows = listOf(only))))
    }

    @Test
    fun picksBetweenSeveralWorkflows() {
        val workflows = listOf(OrganizationUiWorkflow("a", "https://engine", "A"), OrganizationUiWorkflow("b", "https://other", "B"))

        assertEquals(EditTarget.Pick(workflows), editTarget(studio, OrganizationUiSummary("ui-1", "Orders", workflows = workflows)))
    }

    @Test
    fun hasNothingToEditWithoutWorkflows() {
        assertEquals(EditTarget.Nothing, editTarget(studio, OrganizationUiSummary("ui-1", "Orders")))
    }
}
