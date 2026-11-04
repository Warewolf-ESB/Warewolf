package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.buildJsonArray
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.double
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import java.nio.file.Files
import java.nio.file.Path
import java.nio.file.StandardOpenOption
import java.util.Locale
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit
import kotlin.random.Random

/**
 * The artifact comparison (Plan.md, "Service tests and input forms"), on the workflows one whole-workflow contender
 * produced:
 *
 *     gradlew :eval:run --args="bakeoff subjects <bakeoffDir> [--from claude_opus]"
 *     gradlew :eval:run --args="bakeoff tests|forms|uis run <bakeoffDir> <contender> [--parallel N] [--limit N]"
 *     gradlew :eval:run --args="bakeoff tests|forms|uis judge <bakeoffDir> [--model opus] [--parallel N]"
 *     gradlew :eval:run --args="bakeoff tests|forms|uis score <bakeoffDir>"
 *
 * `subjects` stores the valid workflows from `results-<from>.jsonl` in the engine as `Bakeoff/<id>` (node ids made
 * GUIDs) and writes `artifacts-subjects.jsonl`. Contenders are `ollama:<model>`, `lmstudio:<model>` and
 * `claude:<model>`; results go to `<bakeoffDir>/<task>/`. The engine runs at `--engine`. `uis` needs no engine: it
 * checks each UI with Node against the Web Studio checkout at `--angular` (default `~/angular`, with Playwright's
 * Chromium installed) using `--ui-check` (default `ui-check/check-ui.mjs`, from this project's directory).
 */
object ArtifactBakeOff {
    fun main(args: List<String>) {
        val option = { name: String -> args.indexOf(name).takeIf { it >= 0 }?.let { args.getOrNull(it + 1) } }
        val engineUrl = option("--engine") ?: "http://127.0.0.1:3142"
        if (args[0] == "subjects") return subjects(Path.of(args[1]), option("--from") ?: "claude_opus", engineUrl)
        val task = args[0]
        val dir = Path.of(args[2])
        val uiCheck = {
            UiCheck.node(
                Path.of(option("--ui-check") ?: "ui-check/check-ui.mjs").toAbsolutePath(),
                Path.of(option("--angular") ?: Path.of(System.getProperty("user.home"), "angular").toString()),
            )
        }
        when (args[1]) {
            "run" -> run(task, dir, args[3], engineUrl, option("--parallel")?.toInt() ?: 1, option("--limit")?.toInt(), uiCheck)
            "judge" -> judge(task, dir, option("--model") ?: "opus", option("--parallel")?.toInt() ?: 4)
            "score" -> score(task, dir)
            else -> error("usage: bakeoff tests|forms|uis run|judge|score …")
        }
    }

    private fun subjects(dir: Path, from: String, engineUrl: String) {
        val engine = EngineClient(EngineClient.http(engineUrl))
        val goals = BakeOff.jsonLines(dir.resolve("set.jsonl")).associate { it.str("id") to it.str("goal") }
        val rows = BakeOff.jsonLines(dir.resolve("results-$from.jsonl")).filter { it.getValue("valid").jsonPrimitive.boolean && it["workflow"] is JsonObject }
        var stored = 0
        Files.newBufferedWriter(dir.resolve(SUBJECTS)).use { writer ->
            rows.forEach { row ->
                val id = row.str("id")
                val name = "Bakeoff/${id.take(8)}"
                val workflow = Subjects.withGuidIds(row.getValue("workflow").jsonObject)
                engine.delete(name)
                val created = engine.create(name, workflow.getValue("envelope"), workflow.getValue("body"))
                val read = if (created.ok) engine.definition(name) else created
                val editable = read.ok && (read.json as? JsonObject)?.get("bodyEditable")?.jsonPrimitive?.boolean == true
                if (!editable) {
                    println("  ${id.take(8)} skipped: ${read.error ?: "body not editable"}")
                    return@forEach
                }
                val definition = read.json.jsonObject
                writer.write(buildJsonObject {
                    put("id", id); put("goal", goals.getValue(id)); put("name", name)
                    put("definition", buildJsonObject { put("envelope", definition.getValue("envelope")); put("body", definition.getValue("body")) })
                }.toString())
                writer.newLine()
                stored++
            }
        }
        println("stored $stored of ${rows.size} $from workflows as subjects")
    }

    private fun loadSubjects(dir: Path): List<Subject> = BakeOff.jsonLines(dir.resolve(SUBJECTS)).map {
        Subject(it.str("id"), it.str("goal"), it.str("name"), it.getValue("definition").jsonObject)
    }

    private fun catalog(dir: Path) = Catalog.parse(Files.readString(dir.resolve("list_tools.json")), Files.readString(dir.resolve("tool_schemas.json")))

    /** The model behind a contender, and whether it is held to a JSON schema (local models are; Claude is not). */
    internal fun textModel(contender: String): Pair<TextModel, Boolean> {
        val model = contender.substringAfter(':')
        return when (contender.substringBefore(':')) {
            "ollama" -> OllamaText(model) to true
            "lmstudio" -> LmStudioText(model, maxReplyTokens = 16_384) to true
            "claude" -> ClaudeCli(model) to false
            else -> error("unknown contender '$contender'")
        }
    }

    private fun run(task: String, dir: Path, contender: String, engineUrl: String, parallel: Int, limit: Int?, uiCheck: () -> UiCheck) {
        val engine = EngineClient(EngineClient.http(engineUrl))
        val catalog = catalog(dir)
        val (model, constrained) = textModel(contender)
        val subjects = loadSubjects(dir).let { all -> if (task == "forms") all.filter { it.inputs.isNotEmpty() } else all }
            .let { all -> limit?.let { all.take(it) } ?: all }
        val generator = when (task) {
            "tests" -> {
                val schema = engine.invoke("get_workflow_schema", JsonObject(emptyMap()))
                val testSchema = (schema.json as? JsonObject)?.get("test_schema") ?: error("the engine has no test_schema; run a build with it")
                val prefix = BakeOff.fileSafe(contender).replace('.', '_') + "_" + java.lang.Long.toString(System.currentTimeMillis(), 36)
                ServiceTestGenerator(model, engine, catalog, testSchema.toString(), constrained, prefix)
            }
            "forms" -> InputFormGenerator(model, catalog, constrained)
            "uis" -> uiCheck().let { check -> OrganizationUiGenerator(model, catalog, check::check) }
            else -> error("unknown task '$task'")
        }
        val out = taskDir(dir, task).resolve("results-${BakeOff.fileSafe(contender)}.jsonl")
        val done = if (Files.exists(out)) BakeOff.jsonLines(out).mapTo(hashSetOf()) { it.str("id") } else hashSetOf()
        val todo = subjects.filter { it.id !in done }
        println("$task $contender: ${todo.size} to run, ${done.size} already done")
        val pool = Executors.newFixedThreadPool(parallel)
        Files.newBufferedWriter(out, StandardOpenOption.CREATE, StandardOpenOption.APPEND).use { writer ->
            todo.forEach { subject ->
                pool.submit {
                    val started = System.nanoTime()
                    val outcome = try {
                        generator.generate(subject)
                    } catch (e: Exception) {
                        ArtifactOutcome(null, false, false, 0, JsonObject(emptyMap()), listOf("harness error: ${e.toString().take(300)}"))
                    }
                    val seconds = (System.nanoTime() - started) / 1e9
                    val line = buildJsonObject {
                        put("id", subject.id); put("contender", contender)
                        put("accepted", outcome.accepted); put("firstTryAccepted", outcome.firstTryAccepted)
                        put("attempts", outcome.attempts); put("seconds", seconds)
                        put("scores", outcome.scores)
                        put("errors", buildJsonArray { outcome.errors.take(10).forEach { add(JsonPrimitive(it)) } })
                        outcome.artifact?.let { put("artifact", it) }
                    }.toString()
                    synchronized(writer) { writer.write(line); writer.newLine(); writer.flush() }
                    println(String.format(Locale.ROOT, "  %s accepted=%s attempts=%d %.0fs %s %s", subject.id.take(8), outcome.accepted, outcome.attempts,
                        seconds, outcome.scores["result"]?.jsonPrimitive?.content.orEmpty(), outcome.errors.firstOrNull()?.take(120).orEmpty()))
                }
            }
            pool.shutdown()
            pool.awaitTermination(7, TimeUnit.DAYS)
        }
    }

    private fun judge(task: String, dir: Path, model: String, parallel: Int) {
        val catalog = catalog(dir)
        val subjects = loadSubjects(dir).associateBy { it.id }
        val out = taskDir(dir, task).resolve("judge.jsonl")
        val done = if (Files.exists(out)) BakeOff.jsonLines(out).mapTo(hashSetOf()) { it.str("contender") + "|" + it.str("id") } else hashSetOf()
        val todo = BakeOff.resultFiles(taskDir(dir, task)).flatMap { BakeOff.jsonLines(it) }
            .filter { it["artifact"] is JsonObject && it.str("id") in subjects && (it.str("contender") + "|" + it.str("id")) !in done }
            .shuffled(Random(11))
        println("judging ${todo.size} $task with $model")
        val judge = ArtifactJudge(ClaudeCli(model), rubric(task))
        val pool = Executors.newFixedThreadPool(parallel)
        Files.newBufferedWriter(out, StandardOpenOption.CREATE, StandardOpenOption.APPEND).use { writer ->
            todo.forEach { row ->
                pool.submit {
                    val subject = subjects.getValue(row.str("id"))
                    val verdict = runCatching {
                        judge.rate(subject.goal, WorkflowText.summary(subject.definition, catalog), artifactForJudge(row.getValue("artifact").jsonObject))
                    }.getOrNull()
                    val line = verdictJson(row.str("id"), row.str("contender"), verdict).toString()
                    synchronized(writer) { writer.write(line); writer.newLine(); writer.flush() }
                }
            }
            pool.shutdown()
            pool.awaitTermination(7, TimeUnit.DAYS)
        }
    }

    internal fun rubric(task: String) = when (task) {
        "tests" -> ArtifactJudge.Rubric.TEST
        "uis" -> ArtifactJudge.Rubric.UI
        else -> ArtifactJudge.Rubric.FORM
    }

    /** The harness-assigned test name carries the contender, so the judge never sees it. */
    internal fun artifactForJudge(artifact: JsonObject): JsonObject = JsonObject(artifact - "testName")

    private fun score(task: String, dir: Path) {
        val verdicts = taskDir(dir, task).resolve("judge.jsonl").let { file ->
            if (Files.exists(file)) BakeOff.jsonLines(file).filter { it["score"] != null }
                .associate { (it.str("contender") + "|" + it.str("id")) to it.getValue("score").jsonPrimitive.int } else emptyMap()
        }
        if (task == "tests") {
            println("| Contender | Runs | Stored 1st try | Stored | Ran | Passed | Outputs asserted | Mocks per test | Judge mean | Judge ≥4 | Median s |")
            println("|---|---|---|---|---|---|---|---|---|---|---|")
        } else if (task == "uis") {
            println("| Contender | Runs | Accepted 1st try | Accepted | Compiled | Started | Ran | Inputs sent | Outputs shown | Judge mean | Judge ≥4 | Median s |")
            println("|---|---|---|---|---|---|---|---|---|---|---|---|")
        } else {
            println("| Contender | Runs | Accepted 1st try | Accepted | Meta-schema valid | Inputs covered | Shapes right | Judge mean | Judge ≥4 | Median s |")
            println("|---|---|---|---|---|---|---|---|---|---|")
        }
        BakeOff.resultFiles(taskDir(dir, task)).forEach { file ->
            val rows = BakeOff.jsonLines(file)
            if (rows.isEmpty()) return@forEach
            val pct = { n: Int -> String.format(Locale.ROOT, "%.0f%%", 100.0 * n / rows.size) }
            val scores = rows.map { it.getValue("scores").jsonObject }
            val sum = { key: String -> scores.sumOf { it[key]?.jsonPrimitive?.int ?: 0 } }
            val ratio = { part: String, whole: String -> if (sum(whole) == 0) "—" else String.format(Locale.ROOT, "%.0f%%", 100.0 * sum(part) / sum(whole)) }
            val judged = rows.mapNotNull { verdicts[it.str("contender") + "|" + it.str("id")] }
            val judgeMean = if (judged.isEmpty()) "—" else String.format(Locale.ROOT, "%.2f (%d)", judged.average(), judged.size)
            val judgeHigh = if (judged.isEmpty()) "—" else pct(judged.count { it >= 4 })
            val seconds = rows.map { it.getValue("seconds").jsonPrimitive.double }.sorted()[rows.size / 2]
            val head = String.format(Locale.ROOT, "| %s | %d | %s | %s", rows.first().str("contender"), rows.size,
                pct(rows.count { it.getValue("firstTryAccepted").jsonPrimitive.boolean }), pct(rows.count { it.getValue("accepted").jsonPrimitive.boolean }))
            if (task == "tests") {
                val result = { r: JsonObject -> r["result"]?.jsonPrimitive?.content }
                val ran = scores.count { result(it) == "TestPassed" || result(it) == "TestFailed" }
                val mocks = scores.filter { result(it) != null }.map { it["mocks"]?.jsonPrimitive?.int ?: 0 }
                println(String.format(Locale.ROOT, "%s | %s | %s | %s | %s | %s | %s | %.0f |", head, pct(ran), pct(scores.count { result(it) == "TestPassed" }),
                    ratio("outputsAsserted", "outputs"), if (mocks.isEmpty()) "—" else String.format(Locale.ROOT, "%.1f", mocks.average()),
                    judgeMean, judgeHigh, seconds))
            } else if (task == "uis") {
                val flag = { key: String -> pct(scores.count { it[key]?.jsonPrimitive?.boolean == true }) }
                println(String.format(Locale.ROOT, "%s | %s | %s | %s | %s | %s | %s | %s | %.0f |", head, flag("compiled"), flag("started"),
                    flag("ran"), ratio("inputsSent", "inputs"), ratio("outputsShown", "outputs"), judgeMean, judgeHigh, seconds))
            } else {
                println(String.format(Locale.ROOT, "%s | %s | %s | %s | %s | %s | %.0f |", head,
                    pct(scores.count { it["metaValid"]?.jsonPrimitive?.boolean == true }),
                    ratio("inputsCovered", "inputs"), ratio("shapesRight", "inputs"), judgeMean, judgeHigh, seconds))
            }
        }
    }

    private fun taskDir(dir: Path, task: String): Path = dir.resolve(task).also { Files.createDirectories(it) }

    private fun JsonObject.str(key: String) = getValue(key).jsonPrimitive.content

    private const val SUBJECTS = "artifacts-subjects.jsonl"
}
