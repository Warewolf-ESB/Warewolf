package warewolf.studio.app

import io.ktor.client.HttpClient
import io.ktor.client.engine.mock.MockEngine
import io.ktor.client.engine.mock.respond
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import io.ktor.http.HttpStatusCode
import io.ktor.http.headersOf
import kotlinx.coroutines.test.runTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull

class HomeModelTest {
    private val store = MemorySettingsStore()
    private val autoStart = AutoStartSettings(store)
    private val acme = """{"email":"ash@acme.example","organization":{"orgId":"org-1","orgName":"Acme"}}"""
    private val twoUis = """{"uis":[{"id":"ui-1","name":"Orders"},{"id":"ui-2","name":"Stock"}]}"""

    private var sessionsCleared = 0
    private var logoutCalls = 0

    private fun model(
        me: String = acme,
        uis: String = twoUis,
        uisStatus: HttpStatusCode = HttpStatusCode.OK,
        token: String? = "cf-token",
        logoutFails: Boolean = false,
    ): HomeModel {
        val engine = MockEngine { request ->
            if (request.url.encodedPath.endsWith("/cdn-cgi/access/logout")) {
                logoutCalls++
                if (logoutFails) throw IllegalStateException("no network")
                return@MockEngine respond("", HttpStatusCode.Found)
            }
            val (body, status) = if (request.url.encodedPath.endsWith("/me")) me to HttpStatusCode.OK else uis to uisStatus
            respond(body, status, headersOf(HttpHeaders.ContentType, ContentType.Application.Json.toString()))
        }
        return HomeModel(StudioApi(HttpClient(engine) { followRedirects = false }, StudioAddress.DEFAULT.let(::StudioAddress)) { token }, autoStart) { sessionsCleared++ }
    }

    @Test
    fun signsOutAtCloudflareAndOnTheDevice() = runTest {
        val model = model()
        model.load()

        model.signOut()

        assertEquals(HomeState.SignedOut, model.state.value)
        assertEquals(1, logoutCalls)
        assertEquals(1, sessionsCleared)
    }

    @Test
    fun keepsTheAutoStartSettingWhenSigningOut() = runTest {
        val model = model()
        model.load()
        model.toggleAutoStart("ui-2")

        model.signOut()

        assertEquals("ui-2", autoStart.get("org-1"))
    }

    @Test
    fun clearsTheDeviceSessionEvenWhenCloudflareCannotBeReached() = runTest {
        val model = model(logoutFails = true)
        model.load()

        model.signOut()

        assertEquals(HomeState.SignedOut, model.state.value)
        assertEquals(1, sessionsCleared)
    }

    @Test
    fun showsTheOrganizationsUis() = runTest {
        val model = model()

        assertNull(model.load())

        val ready = assertIs<HomeState.Ready>(model.state.value)
        assertEquals("Acme", ready.organization.orgName)
        assertEquals(listOf("Orders", "Stock"), ready.uis.map { it.name })
        assertNull(ready.autoStartId)
    }

    @Test
    fun opensTheAutoStartUiOnLaunchOnly() = runTest {
        autoStart.set("org-1", "ui-2")
        val model = model()

        assertEquals("Stock", model.load(launch = true)?.name)
        assertNull(model.load(launch = false))
        assertEquals("ui-2", (model.state.value as HomeState.Ready).autoStartId)
    }

    @Test
    fun listsWithANoticeWhenTheAutoStartUiHasGone() = runTest {
        autoStart.set("org-1", "ui-gone")
        val model = model()

        assertNull(model.load(launch = true))

        val ready = assertIs<HomeState.Ready>(model.state.value)
        assertEquals("The UI set to open on start is no longer available, so it has been turned off.", ready.notice)
        assertNull(ready.autoStartId)
    }

    @Test
    fun movesAutoStartAndKeepsItOnTheDevice() = runTest {
        val model = model()
        model.load()

        model.toggleAutoStart("ui-1")
        model.toggleAutoStart("ui-2")

        assertEquals("ui-2", (model.state.value as HomeState.Ready).autoStartId)
        assertEquals("ui-2", autoStart.get("org-1"))
    }

    @Test
    fun showsAnEmptyList() = runTest {
        val model = model(uis = """{"canEdit":false,"uis":[]}""")
        model.load()

        assertEquals(emptyList(), assertIs<HomeState.Ready>(model.state.value).uis)
    }

    @Test
    fun asksToSignInWithoutASession() = runTest {
        val model = model(token = null)
        model.load()

        assertEquals(HomeState.SignedOut, model.state.value)
    }

    @Test
    fun saysWhenTheAccountHasNoOrganization() = runTest {
        val model = model(me = """{"email":"ash@acme.example","organization":null}""")
        model.load()

        assertEquals(HomeState.Failed("ash@acme.example isn't in an organization yet. Join or create one in the Web Studio."), model.state.value)
    }

    @Test
    fun showsTheStudiosError() = runTest {
        val model = model(uis = """{"message":"Couldn't read the organization's UIs."}""", uisStatus = HttpStatusCode.InternalServerError)
        model.load()

        assertEquals(HomeState.Failed("Couldn't read the organization's UIs."), model.state.value)
    }
}
