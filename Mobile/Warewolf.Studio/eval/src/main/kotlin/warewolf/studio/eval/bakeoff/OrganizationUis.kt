package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.contentOrNull
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import java.nio.file.Files
import java.nio.file.Path
import java.util.concurrent.TimeUnit

/**
 * Task C: the model writes an organization UI for the workflow, the Angular component Web Studio publishes under
 * Organization settings → UIs and runs sandboxed (`~/angular/src/organization-ui-runtime`). [UiCheck] compiles it,
 * starts it in that runtime, fills its controls, presses submit and answers its `runWorkflow` call; a failing UI goes
 * back to the model with the problems, up to [repairs] times. The artifact is `{"source": "<TypeScript>"}`.
 */
class OrganizationUiGenerator(
    private val model: TextModel,
    private val catalog: Catalog,
    private val check: (Subject, String) -> UiCheck.Result,
    private val repairs: Int = 2,
) : ArtifactGenerator {
    override fun generate(subject: Subject): ArtifactOutcome {
        val base = prompt(subject)
        var prompt = base
        var last: JsonObject? = null
        var result = UiCheck.Result.failed(NOT_CODE)
        for (attempt in 1..repairs + 1) {
            // Code has no decoding grammar, so local models answer unconstrained, as Claude does.
            val reply = model.complete(prompt, null, attempt)
            val source = source(reply)
            result = if (source == null) UiCheck.Result.failed(NOT_CODE) else {
                last = buildJsonObject { put("source", source) }
                check(subject, source)
            }
            if (result.ok) return ArtifactOutcome(last, true, attempt == 1, attempt, result.scores(subject), emptyList())
            prompt = base + "\n\n" + repairPrompt(source, result.errors)
        }
        return ArtifactOutcome(last, false, false, repairs + 1, result.scores(subject), result.errors)
    }

    internal fun prompt(subject: Subject): String = buildString {
        appendLine("Write the page a person uses to run a Warewolf workflow: an organization UI for Warewolf Web Studio. Warewolf")
        appendLine("workflows are flowcharts of tools that pass data in [[variables]]; the page collects the workflow's inputs, runs")
        appendLine("it and shows its outputs.")
        appendLine()
        appendLine("Goal of the workflow: ${subject.goal}")
        appendLine()
        appendLine("Inputs (send each by name):")
        subject.inputs.forEach { appendLine("- ${describe(it)}") }
        if (subject.inputs.isEmpty()) appendLine("- none")
        appendLine("Outputs (runWorkflow resolves to an object with these keys):")
        subject.outputs.forEach { appendLine("- ${describe(it)}") }
        if (subject.outputs.isEmpty()) appendLine("- none")
        appendLine()
        appendLine("How the workflow uses them:")
        appendLine(WorkflowText.summary(subject.definition, catalog))
        appendLine("Rules:")
        appendLine("- Write one TypeScript file whose default export is a standalone Angular 18 component with an inline template")
        appendLine("  and styles. It may import only @angular/core, @angular/common, @angular/forms and @warewolf/ui.")
        appendLine("- `import { runWorkflow } from '@warewolf/ui'`; `runWorkflow('$KEY', inputs)` runs this workflow as the person")
        appendLine("  viewing the page and resolves to its outputs, or rejects with an error to show them. inputs is an object with")
        appendLine("  one key per input: a string for a scalar, an object for an object, an array of row objects (one key per")
        appendLine("  field) for a recordset. Outputs come back the same way.")
        appendLine("- Use a <form> with a submit button that runs the workflow, a labelled control for every input, and show every")
        appendLine("  output once the workflow answers.")
        appendLine("- It runs in a Web Worker with no DOM: render only through the template, no document, window, storage or")
        appendLine("  network. Use inject() rather than constructor parameters. No links (href), [innerHTML] or external images.")
        appendLine("- Follow the example's style of page: a heading, a label and placeholder for every input (number, date or textarea")
        appendLine("  where they fit), a submit button disabled while running, outputs under readable headings (a recordset as a table,")
        appendLine("  booleans as Yes/No), and a visible error box. A recordset input is a list of rows to add to and remove from.")
        appendLine("- Reply with ONE complete TypeScript file in a single ```ts block and nothing else: no reasoning, no notes, and no")
        appendLine("  partial snippets or placeholders: every template and style must be written out in full. A complete example for")
        appendLine("  a different workflow (one text input, one text output) shows the shape; write yours for the workflow above:")
        appendLine(EXAMPLE)
    }

    private fun repairPrompt(previous: String?, errors: List<String>): String = buildString {
        appendLine("Your previous UI has these problems:")
        errors.forEach { appendLine("- $it") }
        appendLine()
        // A reply with no code in it is not echoed back: the model would repair its own commentary.
        if (previous != null) {
            appendLine("Previous UI:")
            appendLine(previous)
            appendLine()
        }
        appendLine("Fix them and reply with the corrected TypeScript only.")
    }

    private fun describe(variable: JsonObject): String {
        val fields = (variable["fields"] as? JsonArray)?.mapNotNull { it.jsonPrimitive.contentOrNull }.orEmpty()
        val kind = variable["kind"]?.jsonPrimitive?.contentOrNull.orEmpty()
        return "${variable["name"]?.jsonPrimitive?.contentOrNull.orEmpty()} ($kind${if (fields.isEmpty()) "" else ", fields: ${fields.joinToString(", ")}"})"
    }

    internal companion object {
        /** The key the UI runs its workflow by; the check answers no other. */
        const val KEY = "main"
        const val NOT_CODE = "the reply has no TypeScript in it: it ran out of room while thinking. Do not deliberate or repeat yourself; " +
            "decide quickly and write the whole file"

        /** Written out in full: a skeleton with placeholders gets copied with its placeholders. */
        private val EXAMPLE = """
            |```ts
            |import { Component } from '@angular/core';
            |import { FormsModule } from '@angular/forms';
            |import { CommonModule } from '@angular/common';
            |import { runWorkflow } from '@warewolf/ui';
            |
            |@Component({
            |  selector: 'organization-ui',
            |  standalone: true,
            |  imports: [CommonModule, FormsModule],
            |  template: `
            |    <main class="card">
            |      <h1>Shout a message</h1>
            |      <form (ngSubmit)="run()">
            |        <label for="message">Message</label>
            |        <input id="message" name="message" [(ngModel)]="message" placeholder="e.g. hello world" required>
            |        <button type="submit" [disabled]="running || !message">{{ running ? 'Running…' : 'Run' }}</button>
            |      </form>
            |      @if (error) { <p class="error">{{ error }}</p> }
            |      @if (outputs) {
            |        <section>
            |          <h2>Result</h2>
            |          <p><strong>Shouted message</strong> {{ outputs.ShoutedMessage || 'Nothing returned' }}</p>
            |        </section>
            |      }
            |    </main>
            |  `,
            |  styles: `
            |    .card { max-width: 32rem; margin: 1rem auto; padding: 1.5rem; font: 16px sans-serif; }
            |    label, input, button { display: block; width: 100%; box-sizing: border-box; margin-bottom: .75rem; }
            |    input { padding: .5rem; }
            |    button { padding: .6rem; background: #2563eb; color: white; border: 0; border-radius: .25rem; }
            |    button:disabled { background: #94a3b8; }
            |    .error { padding: .75rem; background: #fee2e2; color: #991b1b; border-radius: .25rem; }
            |  `,
            |})
            |export default class ShoutUi {
            |  message = '';
            |  running = false;
            |  error = '';
            |  outputs: any = null;
            |
            |  async run() {
            |    this.running = true;
            |    this.error = '';
            |    this.outputs = null;
            |    try {
            |      this.outputs = await runWorkflow('main', { Message: this.message });
            |    } catch (e: any) {
            |      this.error = e?.message ?? String(e);
            |    } finally {
            |      this.running = false;
            |    }
            |  }
            |}
            |```
        """.trimMargin()

        private val FENCE = Regex("```[A-Za-z]*[ \\t]*\\r?\\n(.*?)```", RegexOption.DOT_MATCHES_ALL)
        private val OPEN_FENCE = Regex("```[A-Za-z]*[ \\t]*\\r?\\n")

        private const val MESSAGE = "<|message|>"

        /**
         * The TypeScript in a reply: its first fenced block, or the whole reply when it has none; null when empty.
         * Some local models leak chat-template channels into the content (`to=self<|message|>reasoning<|eom|><|start|>
         * assistant to=user<|message|>code`); only the last message is the answer.
         */
        fun source(reply: String): String? {
            val answer = if (MESSAGE in reply) reply.substringAfterLast(MESSAGE).substringBefore("<|") else reply
            val blocks = FENCE.findAll(answer).map { it.groupValues[1] }.toList()
            // A reply cut off mid-code has an opening fence and no closing one.
            val open = OPEN_FENCE.findAll(answer).lastOrNull()?.let { answer.substring(it.range.last + 1) }
            // Models that think aloud in the reply quote fragments in earlier fences; the answer is the last full component.
            val chosen = blocks.lastOrNull { "export default" in it } ?: open?.takeIf { "export default" in it }
                ?: blocks.firstOrNull() ?: answer
            // Reasoning prose with no component in it is no answer, and is not echoed back for repair.
            if (blocks.isEmpty() && "export default" !in chosen && "@Component" !in chosen) return null
            return dedent(chosen).trim().ifEmpty { null }
        }

        /** Models often indent the whole answer, with the first line flush: strip the indent the later lines share. */
        private fun dedent(code: String): String {
            val lines = code.lines()
            val indent = lines.drop(1).filter { it.isNotBlank() }.minOfOrNull { it.length - it.trimStart().length } ?: 0
            return lines.mapIndexed { i, line -> if (i == 0) line.trimStart() else line.drop(minOf(indent, line.length - line.trimStart().length)) }
                .joinToString("\n")
        }
    }
}

/** Runs `ui-check/check-ui.mjs` against Web Studio's real organization UI runtime; see that script. */
class UiCheck(private val run: (request: JsonObject) -> String) {
    data class Result(
        val compiled: Boolean,
        val started: Boolean,
        val ran: Boolean,
        val inputsSent: List<String>,
        val outputsShown: List<String>,
        val errors: List<String>,
    ) {
        val ok: Boolean get() = errors.isEmpty() && ran

        fun scores(subject: Subject) = buildJsonObject {
            put("inputs", subject.inputs.size)
            put("outputs", subject.outputs.size)
            put("compiled", compiled)
            put("started", started)
            put("ran", ran)
            put("inputsSent", inputsSent.size)
            put("outputsShown", outputsShown.size)
        }

        companion object {
            fun failed(error: String) = Result(false, false, false, emptyList(), emptyList(), listOf(error))

            fun parse(line: String): Result {
                val json = Json.parseToJsonElement(line).jsonObject
                fun flag(key: String) = json[key]?.jsonPrimitive?.boolean == true
                fun names(key: String) = (json[key] as? JsonArray)?.map { it.jsonPrimitive.content }.orEmpty()
                return Result(flag("compiled"), flag("started"), flag("ran"), names("inputsSent"), names("outputsShown"), names("errors"))
            }
        }
    }

    fun check(subject: Subject, source: String): Result {
        val request = buildJsonObject {
            put("source", source)
            put("inputs", JsonArray(subject.inputs))
            put("outputs", JsonArray(subject.outputs))
        }
        val output = run(request)
        val line = output.lines().lastOrNull { it.trimStart().startsWith("{") }
            ?: return Result.failed("check error: no result from check-ui.mjs: ${output.takeLast(300)}")
        return runCatching { Result.parse(line) }.getOrElse { Result.failed("check error: unreadable result: ${line.take(300)}") }
    }

    companion object {
        /** A check that runs [script] with Node, against the Web Studio checkout at [angularDir]. */
        fun node(script: Path, angularDir: Path, timeoutSeconds: Long = 180) = UiCheck { request ->
            val file = Files.createTempFile("ui-check-", ".json")
            val log = Files.createTempFile("ui-check-", ".log")
            try {
                Files.writeString(file, request.toString())
                // Into a file, not a pipe: reading a pipe would block past the timeout on a UI that never settles.
                val process = ProcessBuilder("node", script.toString(), angularDir.toString(), file.toString())
                    .redirectErrorStream(true).redirectOutput(log.toFile()).start()
                if (!process.waitFor(timeoutSeconds, TimeUnit.SECONDS)) {
                    process.destroyForcibly()
                    """{"errors":["check error: check-ui.mjs did not finish within ${timeoutSeconds}s"]}"""
                } else Files.readString(log)
            } finally {
                Files.deleteIfExists(file)
                Files.deleteIfExists(log)
            }
        }
    }
}
