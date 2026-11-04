package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import org.junit.jupiter.api.Assumptions.assumeTrue
import java.nio.file.Files
import java.nio.file.Path
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNull
import kotlin.test.assertTrue

private fun obj(json: String) = Json.parseToJsonElement(json).jsonObject

/** A workflow with inputs `city` (scalar) and `stops` (recordset of name, eta), and output `summary`. */
private val uiSubject = Subject(
    "def67890-0000", "Plan a delivery route for a city", "Bakeoff/def67890",
    obj(
        """{"envelope":{"name":"Route","inputs":[{"kind":"scalar","name":"city"},{"kind":"recordset","name":"stops","fields":["name","eta"]}],
           "outputs":[{"kind":"scalar","name":"summary"}]},
           "body":{"resourcename":"Route","cells":[{"id":"s","shape":"start","data":{"type":"start"}}]}}""",
    ),
)

private const val PASSED = """{"compiled":true,"started":true,"ran":true,"runKey":"main","inputsSent":["city","stops"],
    "inputsMissing":[],"outputsShown":["summary"],"outputsMissing":[],"errors":[]}"""
private const val NO_OUTPUT = """{"compiled":true,"started":true,"ran":true,"runKey":"main","inputsSent":["city","stops"],
    "inputsMissing":[],"outputsShown":[],"outputsMissing":["summary"],"errors":["never shows output(s) summary from the workflow's reply"]}"""

/** A check that answers with [results] in order (repeating the last) and records each source it was given. */
private class ScriptedCheck(vararg results: String) {
    private val queue = ArrayDeque(results.toList())
    val sources = mutableListOf<String>()
    fun check(subject: Subject, source: String): UiCheck.Result {
        sources += source
        return UiCheck.Result.parse(if (queue.size > 1) queue.removeFirst() else queue.first())
    }
}

class OrganizationUiGeneratorTests {
    @Test
    fun `the prompt lists inputs and outputs with their kinds, the workflow's key and what the UI may import`() {
        val prompt = OrganizationUiGenerator(ScriptedModel(""), testCatalog, ScriptedCheck(PASSED)::check).prompt(uiSubject)

        assertTrue("- city (scalar)" in prompt)
        assertTrue("- stops (recordset, fields: name, eta)" in prompt)
        assertTrue("- summary (scalar)" in prompt)
        assertTrue("runWorkflow('main', inputs)" in prompt)
        assertTrue("@angular/core, @angular/common, @angular/forms and @warewolf/ui" in prompt)
    }

    @Test
    fun `the TypeScript is the first fenced block, or the whole reply without one`() {
        assertEquals("export default class A {}", OrganizationUiGenerator.source("Here it is:\n```ts\nexport default class A {}\n```\nDone."))
        assertEquals("export default class A {}", OrganizationUiGenerator.source("```\nexport default class A {}\n```"))
        assertEquals("export default class A {}", OrganizationUiGenerator.source("  export default class A {}\n"))
        assertNull(OrganizationUiGenerator.source("   "))
    }

    @Test
    fun `an answer after fenced fragments of the model's reasoning is the one chosen, and its indent is removed`() {
        val reply = "Plan:\n```ts\n@Component({})\n```\nFinal:\n```ts\nimport { a } from 'a';\n    export default class A {\n      x = 1;\n    }\n```"

        assertEquals("import { a } from 'a';\nexport default class A {\n  x = 1;\n}", OrganizationUiGenerator.source(reply))
    }

    @Test
    fun `a reply cut off inside its fence still yields the code so far, and prose with no component is no answer`() {
        assertEquals("export default class A {", OrganizationUiGenerator.source("Here:\n```ts\nexport default class A {"))
        assertNull(OrganizationUiGenerator.source("*   Goal: build a UI\n*   Wait, the prompt says"))
    }

    @Test
    fun `leaked chat-template channels are dropped and only the last message is the answer`() {
        val leaked = "to=self<|message|>The user wants TypeScript only.<|eom|><|start|>assistant to=user<|message|>export default class A {}<|return|>"

        assertEquals("export default class A {}", OrganizationUiGenerator.source(leaked))
        assertNull(OrganizationUiGenerator.source("<|start|>assistant<|message|><|end|>"))
    }

    @Test
    fun `a reply with no code is repaired without echoing it back`() {
        val model = ScriptedModel("<|start|>assistant<|message|><|end|> ignore me", "export default class B {}")

        val outcome = OrganizationUiGenerator(model, testCatalog, ScriptedCheck(PASSED)::check).generate(uiSubject)

        assertTrue(outcome.accepted)
        assertTrue(OrganizationUiGenerator.NOT_CODE in model.prompts[1])
        assertFalse("ignore me" in model.prompts[1])
        assertFalse("Previous UI:" in model.prompts[1])
    }

    @Test
    fun `a UI that passes first time is stored as its source, and a local model gets no format`() {
        val model = ScriptedModel("```ts\nexport default class A {}\n```")

        val outcome = OrganizationUiGenerator(model, testCatalog, ScriptedCheck(PASSED)::check).generate(uiSubject)

        assertTrue(outcome.accepted)
        assertTrue(outcome.firstTryAccepted)
        assertEquals("export default class A {}", outcome.artifact?.getValue("source")?.jsonPrimitive?.content)
        assertEquals(listOf<JsonObject?>(null), model.formats)
        assertEquals(1, outcome.scores.getValue("outputsShown").jsonPrimitive.int)
    }

    @Test
    fun `a failing UI is repaired with the check's problems`() {
        val model = ScriptedModel("export default class A {}", "export default class B {}")
        val check = ScriptedCheck(NO_OUTPUT, PASSED)

        val outcome = OrganizationUiGenerator(model, testCatalog, check::check).generate(uiSubject)

        assertTrue(outcome.accepted)
        assertFalse(outcome.firstTryAccepted)
        assertEquals(2, outcome.attempts)
        assertTrue("never shows output(s) summary" in model.prompts[1])
        assertTrue("export default class A {}" in model.prompts[1])
        assertEquals(listOf("export default class A {}", "export default class B {}"), check.sources)
    }

    @Test
    fun `a UI still failing after every repair keeps its scores and problems`() {
        val outcome = OrganizationUiGenerator(ScriptedModel("export default class A {}"), testCatalog, ScriptedCheck(NO_OUTPUT)::check)
            .generate(uiSubject)

        assertFalse(outcome.accepted)
        assertEquals(3, outcome.attempts)
        assertEquals(2, outcome.scores.getValue("inputsSent").jsonPrimitive.int)
        assertEquals(0, outcome.scores.getValue("outputsShown").jsonPrimitive.int)
        assertEquals(listOf("never shows output(s) summary from the workflow's reply"), outcome.errors)
    }

    @Test
    fun `an empty reply is never checked and is not accepted`() {
        val check = ScriptedCheck(PASSED)

        val outcome = OrganizationUiGenerator(ScriptedModel(" "), testCatalog, check::check).generate(uiSubject)

        assertFalse(outcome.accepted)
        assertTrue(check.sources.isEmpty())
        assertEquals(listOf(OrganizationUiGenerator.NOT_CODE), outcome.errors)
    }
}

class UiCheckTests {
    @Test
    fun `the check is sent the source with the workflow's inputs and outputs, and reads the script's last JSON line`() {
        var sent: JsonObject? = null
        val check = UiCheck { request -> sent = request; "Debugger attached.\n$PASSED".replace("\n    ", " ") }

        val result = check.check(uiSubject, "export default class A {}")

        assertTrue(result.ok)
        assertEquals("export default class A {}", sent?.getValue("source")?.jsonPrimitive?.content)
        assertEquals(2, sent?.getValue("inputs")?.jsonArray?.size)
        assertEquals("summary", sent?.getValue("outputs")?.jsonArray?.single()?.jsonObject?.getValue("name")?.jsonPrimitive?.content)
    }

    @Test
    fun `a script that prints no result fails the UI with the script's output`() {
        val result = UiCheck { "Error: Cannot find module 'playwright'" }.check(uiSubject, "x")

        assertFalse(result.ok)
        assertTrue(result.errors.single().startsWith("check error: no result from check-ui.mjs"))
        assertTrue("Cannot find module 'playwright'" in result.errors.single())
    }

    @Test
    fun `a UI that never ran is not ok even with no errors reported`() {
        val result = UiCheck.Result.parse("""{"compiled":true,"started":true,"ran":false,"errors":[]}""")

        assertFalse(result.ok)
        assertFalse(result.scores(uiSubject).getValue("ran").jsonPrimitive.boolean)
    }
}

class OrganizationUiJudgeTests {
    @Test
    fun `the judge reads a UI as code under the UI rubric`() {
        val prompt = ArtifactJudge(ScriptedModel("{}"), ArtifactBakeOff.rubric("uis"))
            .prompt("Plan a route", "Inputs: city", obj("""{"source":"const a = 1;\nexport default a;"}"""))

        assertTrue("const a = 1;\nexport default a;" in prompt)
        assertFalse("\\n" in prompt)
        assertTrue(ArtifactJudge.Rubric.UI.scale in prompt)
    }
}

/**
 * check-ui.mjs on known UIs (ui-check/fixtures) against Web Studio's real organization UI runtime. Needs Node and a
 * ~/angular checkout with Playwright's Chromium, and takes about 15 s a UI, so it runs only with UI_CHECK=1:
 *
 *     UI_CHECK=1 gradlew :eval:test --tests "*CheckUiScriptTests*"
 */
class CheckUiScriptTests {
    private val fixtures = Path.of("ui-check/fixtures")
    private val angular = Path.of(System.getenv("ANGULAR_DIR") ?: Path.of(System.getProperty("user.home"), "angular").toString())
    private val request = obj(Files.readString(fixtures.resolve("request.json")))
    private val subject = Subject("fixture", "Forecast the weather for a city", "Bakeoff/fixture",
        obj("""{"envelope":{"inputs":${request.getValue("inputs")},"outputs":${request.getValue("outputs")}},"body":{"cells":[]}}"""))

    private fun check(fixture: String): UiCheck.Result {
        assumeTrue(System.getenv("UI_CHECK") == "1", "set UI_CHECK=1 to run check-ui.mjs against the real runtime")
        assumeTrue(Files.isDirectory(angular.resolve("node_modules/playwright")), "needs $angular with Playwright installed")
        return UiCheck.node(Path.of("ui-check/check-ui.mjs").toAbsolutePath(), angular)
            .check(subject, Files.readString(fixtures.resolve("$fixture.ts")))
    }

    @Test
    fun `a UI that runs the workflow with every input and shows its output passes`() {
        val result = check("good")
        assertTrue(result.ok, result.errors.joinToString("; "))
    }

    @Test
    fun `a UI that runs the wrong key fails`() =
        assertTrue(check("wrong-key").errors.any { "but the workflow's key is 'main'" in it })

    @Test
    fun `a UI that never shows the output fails`() =
        assertEquals(listOf("never shows output(s) summary from the workflow's reply"), check("no-outputs").errors)

    @Test
    fun `a UI that leaves an input out fails`() =
        assertTrue(check("missing-input").errors.any { "sends no value for input(s) days" in it })

    @Test
    fun `a UI that imports what the runtime refuses fails before compiling`() =
        assertTrue(check("bad-import").errors.single().startsWith("imports a module the UI can't use: @angular/common/http"))

    @Test
    fun `a UI that does not compile fails`() =
        assertTrue(check("no-compile").errors.single().startsWith("does not compile:"))

    @Test
    fun `a UI the runtime's DOM policy refuses fails to start`() =
        assertTrue(check("link").errors.single().startsWith("does not start: An organization UI can't set href on <a>."))
}
