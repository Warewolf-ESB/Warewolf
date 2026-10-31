package warewolf.studio.app

import io.ktor.client.HttpClient
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.statement.HttpResponse
import io.ktor.client.statement.bodyAsText
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import io.ktor.http.HttpStatusCode
import io.ktor.http.contentType
import io.ktor.http.isSuccess
import kotlinx.coroutines.CancellationException
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive

sealed interface StudioResult<out T> {
    data class Success<T>(val value: T) : StudioResult<T>

    /** No Cloudflare Access session, or it has expired: sign in again. */
    data object SignedOut : StudioResult<Nothing>

    data class Failure(val message: String) : StudioResult<Nothing>
}

/** Cloudflare Access's session cookie on the studio's hostname. */
const val ACCESS_COOKIE = "CF_Authorization"

/** The studio's HTTP client: redirects are not followed, since Access answers a missing session with one. */
fun studioHttpClient(): HttpClient = HttpClient { followRedirects = false }

/**
 * The studio's backend (/api/studio/...), behind Cloudflare Access. The app signs in through Access
 * in a WebView and sends the session cookie it got there ([accessToken]) with each call.
 */
class StudioApi(
    private val http: HttpClient,
    private val studio: StudioAddress,
    private val accessToken: suspend () -> String?,
) {
    private val json = Json { ignoreUnknownKeys = true }

    suspend fun session(): StudioResult<StudioSession> = get("/api/studio/me") { json.decodeFromString<StudioSession>(it) }

    suspend fun organizationUis(): StudioResult<OrganizationUiList> =
        get("/api/studio/organization-uis") { json.decodeFromString<OrganizationUiList>(it) }

    /**
     * Ends the Access session at Cloudflare too (its logout endpoint on the studio's hostname, as the
     * studio's own sign-out does). Best effort: signing out on the device mustn't depend on the network.
     */
    suspend fun signOut() {
        val token = accessToken()?.takeIf { it.isNotBlank() } ?: return
        try {
            http.get(studio.url("/cdn-cgi/access/logout")) { header(HttpHeaders.Cookie, "$ACCESS_COOKIE=$token") }
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            // Offline, or Access unreachable: the device's own session is still cleared.
        }
    }

    private suspend fun <T> get(path: String, decode: (String) -> T): StudioResult<T> {
        val token = accessToken()?.takeIf { it.isNotBlank() } ?: return StudioResult.SignedOut
        val response: HttpResponse
        val body: String
        try {
            response = http.get(studio.url(path)) {
                header(HttpHeaders.Cookie, "$ACCESS_COOKIE=$token")
                header(HttpHeaders.Accept, ContentType.Application.Json.toString())
            }
            body = response.bodyAsText()
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            return StudioResult.Failure("Couldn't reach ${studio.host}: ${e.message ?: e::class.simpleName}")
        }

        // Without a valid session Access redirects to its sign-in page (or refuses), so the call never reaches the backend.
        if (response.status.value in 300..399 || response.status == HttpStatusCode.Unauthorized || response.status == HttpStatusCode.Forbidden) {
            return StudioResult.SignedOut
        }
        if (!response.status.isSuccess()) {
            return StudioResult.Failure(errorMessage(body) ?: "${studio.host} answered HTTP ${response.status.value}.")
        }
        if (response.contentType()?.match(ContentType.Application.Json) != true) {
            // A sign-in page served with 200 instead of the API's JSON.
            return StudioResult.SignedOut
        }
        return try {
            StudioResult.Success(decode(body))
        } catch (e: Exception) {
            StudioResult.Failure("${studio.host} sent a reply the app doesn't understand.")
        }
    }

    /** The backend's `{ "message": ... }`, when it sent one. */
    private fun errorMessage(body: String): String? = try {
        json.parseToJsonElement(body).jsonObject["message"]?.jsonPrimitive?.content?.takeIf { it.isNotBlank() }
    } catch (e: Exception) {
        null
    }
}
