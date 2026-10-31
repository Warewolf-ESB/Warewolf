package warewolf.studio.app

import io.ktor.http.encodeURLParameter
import io.ktor.http.encodeURLPathPart
import kotlinx.serialization.Serializable

/** The Web Studio the app works against, e.g. https://app.warewolf.online: its origin, without a trailing slash. */
class StudioAddress(address: String) {
    val origin: String = normalize(address)
    val host: String get() = origin.substringAfter("://").substringBefore('/')

    /** `path` starts with '/'. */
    fun url(path: String): String = origin + path

    /** Cloudflare Access sign-in; the studio's backend sends the browser back to the root once it's done. */
    val signInUrl: String get() = url("/api/studio/login?returnTo=%2F")

    override fun toString(): String = origin

    companion object {
        const val DEFAULT = "https://app.warewolf.online"

        /** Trimmed, trailing slashes dropped, https:// when no scheme is given (as the studio does for engines). */
        fun normalize(address: String): String {
            val trimmed = address.trim().trimEnd('/')
            return if (Regex("^https?://", RegexOption.IGNORE_CASE).containsMatchIn(trimmed)) trimmed else "https://$trimmed"
        }
    }
}

/** A workflow a UI may run (`organization-uis.service.ts` OrganizationUiWorkflow). */
@Serializable
data class OrganizationUiWorkflow(
    /** The name the UI's code calls it by. */
    val key: String,
    /** The engine's address. */
    val server: String,
    /** The workflow's path on that engine. */
    val workflow: String,
)

/** One row of GET /api/studio/organization-uis. */
@Serializable
data class OrganizationUiSummary(
    val id: String,
    val name: String,
    val description: String? = null,
    val workflows: List<OrganizationUiWorkflow> = emptyList(),
    val dateUpdated: String = "",
) {
    /** The date part of [dateUpdated] (ISO 8601), for the list. */
    val updatedOn: String get() = dateUpdated.substringBefore('T')
}

@Serializable
data class OrganizationUiList(
    /** Administrator: may publish new UIs. */
    val canEdit: Boolean = false,
    /** Only the UIs the caller may View. */
    val uis: List<OrganizationUiSummary> = emptyList(),
)

@Serializable
data class StudioOrganization(val orgId: String, val orgName: String = "")

/** GET /api/studio/me: the signed-in caller; [organization] is null when they don't belong to one. */
@Serializable
data class StudioSession(val email: String = "", val name: String? = null, val organization: StudioOrganization? = null)

/** A UI run fullscreen: the studio's own page for it, without the side menu. */
fun uiPageUrl(studio: StudioAddress, uiId: String): String =
    studio.url("/organization-ui/${uiId.encodeURLPathPart()}?standalone=true")

/** The Web Studio's editor on one of a UI's workflows (`/workflow?engine=&path=`, WorkflowDesignerComponent). */
fun workflowEditUrl(studio: StudioAddress, workflow: OrganizationUiWorkflow): String =
    studio.url("/workflow?engine=${workflow.server.trim().encodeURLParameter()}&path=${workflow.workflow.trim().encodeURLParameter()}")

/** What Edit does for a UI. */
sealed interface EditTarget {
    /** The UI runs no workflows, so there's nothing to edit. */
    data object Nothing : EditTarget

    /** One workflow: open it straight away. */
    data class Open(val url: String) : EditTarget

    /** Several: the user picks one. */
    data class Pick(val workflows: List<OrganizationUiWorkflow>) : EditTarget
}

fun editTarget(studio: StudioAddress, ui: OrganizationUiSummary): EditTarget = when (ui.workflows.size) {
    0 -> EditTarget.Nothing
    1 -> EditTarget.Open(workflowEditUrl(studio, ui.workflows.single()))
    else -> EditTarget.Pick(ui.workflows)
}
