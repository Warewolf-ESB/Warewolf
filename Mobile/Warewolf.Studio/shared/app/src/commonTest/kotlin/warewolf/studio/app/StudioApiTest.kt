package warewolf.studio.app

import io.ktor.client.HttpClient
import io.ktor.client.engine.mock.MockEngine
import io.ktor.client.engine.mock.MockRequestHandleScope
import io.ktor.client.engine.mock.respond
import io.ktor.client.request.HttpRequestData
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import io.ktor.http.HttpStatusCode
import io.ktor.http.headersOf
import kotlinx.coroutines.test.runTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertTrue

class StudioApiTest {
    private val studio = StudioAddress("https://app.warewolf.online")
    private val requests = mutableListOf<HttpRequestData>()

    private fun api(token: String? = "cf-token", reply: MockRequestHandleScope.(HttpRequestData) -> io.ktor.client.request.HttpResponseData): StudioApi {
        val engine = MockEngine { request -> requests += request; reply(request) }
        return StudioApi(HttpClient(engine) { followRedirects = false }, studio) { token }
    }

    private fun MockRequestHandleScope.json(body: String, status: HttpStatusCode = HttpStatusCode.OK) =
        respond(body, status, headersOf(HttpHeaders.ContentType, ContentType.Application.Json.toString()))

    @Test
    fun parsesTheListAndIgnoresFieldsItDoesNotUse() = runTest {
        val result = api {
            json("""{"canEdit":false,"uis":[{"id":"ui-1","name":"Orders","description":"Track orders","dateUpdated":"2026-10-30T09:15:00Z","canEdit":true,
                |"workflows":[{"key":"list","server":"https://wwprofilerlw.azurewebsites.net","workflow":"Orders/List"}]}]}""".trimMargin())
        }.organizationUis()

        val ui = assertIs<StudioResult.Success<OrganizationUiList>>(result).value.uis.single()
        assertEquals("Orders", ui.name)
        assertEquals("2026-10-30", ui.updatedOn)
        assertEquals(OrganizationUiWorkflow("list", "https://wwprofilerlw.azurewebsites.net", "Orders/List"), ui.workflows.single())
        assertEquals("https://app.warewolf.online/api/studio/organization-uis", requests.single().url.toString())
    }

    @Test
    fun sendsTheAccessSessionCookie() = runTest {
        api { json("""{"email":"a@b.c","organization":{"orgId":"org-1","orgName":"Acme"}}""") }.session()

        assertEquals("CF_Authorization=cf-token", requests.single().headers[HttpHeaders.Cookie])
    }

    @Test
    fun isSignedOutWithoutASessionAndDoesNotCallTheStudio() = runTest {
        assertEquals(StudioResult.SignedOut, api(token = null) { json("{}") }.organizationUis())
        assertEquals(StudioResult.SignedOut, api(token = " ") { json("{}") }.organizationUis())
        assertTrue(requests.isEmpty())
    }

    @Test
    fun isSignedOutWhenAccessRedirectsOrRefuses() = runTest {
        for (status in listOf(HttpStatusCode.Found, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden)) {
            val result = api { respond("", status, headersOf(HttpHeaders.Location, "https://warewolf.cloudflareaccess.com/cdn-cgi/access/login")) }.session()
            assertEquals(StudioResult.SignedOut, result, "HTTP ${status.value}")
        }
    }

    @Test
    fun isSignedOutWhenASignInPageComesBackInsteadOfJson() = runTest {
        val result = api { respond("<html>Sign in</html>", HttpStatusCode.OK, headersOf(HttpHeaders.ContentType, ContentType.Text.Html.toString())) }.organizationUis()

        assertEquals(StudioResult.SignedOut, result)
    }

    @Test
    fun showsTheStudiosErrorMessage() = runTest {
        val result = api { json("""{"message":"The organization is suspended."}""", HttpStatusCode.InternalServerError) }.organizationUis()

        assertEquals(StudioResult.Failure("The organization is suspended."), result)
    }

    @Test
    fun namesTheStatusWhenThereIsNoMessage() = runTest {
        val result = api { respond("oops", HttpStatusCode.BadGateway) }.organizationUis()

        assertEquals(StudioResult.Failure("app.warewolf.online answered HTTP 502."), result)
    }

    @Test
    fun signsOutAtCloudflareWithTheSession() = runTest {
        api { respond("", HttpStatusCode.Found) }.signOut()

        assertEquals("https://app.warewolf.online/cdn-cgi/access/logout", requests.single().url.toString())
        assertEquals("CF_Authorization=cf-token", requests.single().headers[HttpHeaders.Cookie])
    }

    @Test
    fun signingOutDoesNotFailOnErrorsOrWithoutANetwork() = runTest {
        api { respond("", HttpStatusCode.InternalServerError) }.signOut()
        api { throw IllegalStateException("no network") }.signOut()
    }

    @Test
    fun signingOutWithoutASessionCallsNothing() = runTest {
        api(token = null) { respond("", HttpStatusCode.OK) }.signOut()

        assertTrue(requests.isEmpty())
    }

    @Test
    fun saysWhenTheStudioCannotBeReached() = runTest {
        val result = api { throw IllegalStateException("no network") }.organizationUis()

        assertEquals(StudioResult.Failure("Couldn't reach app.warewolf.online: no network"), result)
    }
}
