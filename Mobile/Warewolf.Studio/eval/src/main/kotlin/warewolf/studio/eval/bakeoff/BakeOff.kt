package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.buildJsonArray
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.contentOrNull
import kotlinx.serialization.json.double
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put
import warewolf.studio.eval.Corpus
import warewolf.studio.eval.MarkovChooser
import warewolf.studio.eval.OnnxClassifier
import warewolf.studio.generator.ClassifierChooser
import warewolf.studio.generator.LlmChoiceChooser
import warewolf.studio.generator.ToolCatalog
import java.nio.file.Files
import java.nio.file.Path
import java.util.Locale
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit
import kotlin.random.Random

/**
 * The whole-workflow comparison (Plan.md, "Whole-workflow comparison"): System 1 against small and frontier
 * models writing the workflow JSON, on held-out corpus goals, scored by the engine's validator, structural
 * closeness to the author's workflow, and a blind judge.
 *
 *     gradlew :eval:run --args="bakeoff select <derivedDir> <bakeoffDir> [--count 50] [--seed 7]"
 *     gradlew :eval:run --args="bakeoff run <derivedDir> <bakeoffDir> <contender> [--parallel N] [--limit N] [--model-dir <dir>]"
 *     gradlew :eval:run --args="bakeoff judge <bakeoffDir> [--model opus] [--parallel N]"
 *     gradlew :eval:run --args="bakeoff score <bakeoffDir>"
 *
 * Contenders: `ollama:<model>`, `lmstudio:<model>` (LM Studio at `LLM_BASE_URL`, default ask.leylew.is, with
 * `LLM_API_KEY`) and `claude:<model>` write whole workflows; `system1-markov:<fillModel>` and
 * `system1-classifier:<fillModel>` (with `--model-dir`) are System 1 with an Ollama field filler. `<bakeoffDir>`
 * holds `list_tools.json` and `tool_schemas.json` from the engine; the engine runs at `--engine`
 * (default http://127.0.0.1:3142).
 */
object BakeOff {
    fun main(args: List<String>) {
        val option = { name: String -> args.indexOf(name).takeIf { it >= 0 }?.let { args.getOrNull(it + 1) } }
        when (args.firstOrNull()) {
            "select" -> select(Path.of(args[1]), Path.of(args[2]), option("--count")?.toInt() ?: 50, option("--seed")?.toInt() ?: 7)
            "run" -> run(Path.of(args[1]), Path.of(args[2]), args[3], option)
            "judge" -> judge(Path.of(args[1]), option("--model") ?: "opus", option("--parallel")?.toInt() ?: 4)
            "score" -> score(Path.of(args[1]))
            "spotcheck" -> spotcheck(Path.of(args[1]), option("--count")?.toInt() ?: 15)
            "subjects", "tests", "forms", "uis" -> ArtifactBakeOff.main(args)
            else -> error("usage: bakeoff select|run|judge|score|spotcheck|subjects|tests|forms|uis …")
        }
    }

    private val evalSets = setOf("caps", "asws")

    /**
     * Picks [count] held-out workflows (families never seen in training), a third each with 1–4, 5–12 and 13+ steps,
     * and records each one's goal and the tools its author placed.
     */
    private fun select(derived: Path, out: Path, count: Int, seed: Int) {
        Files.createDirectories(out)
        val split = Corpus.split(Corpus.loadExamples(derived), evalSets)
        val trainFamilies = split.train.mapTo(hashSetOf()) { it.family }
        val byWorkflow = split.eval.groupBy { it.workflowId }
        val candidates = jsonLines(derived.resolve("workflows.jsonl")).filter { w ->
            w.str("set") in evalSets && w["error"].isNullJson() && w.getValue("steps").jsonPrimitive.int > 0 &&
                w.str("family") !in trainFamilies && w.str("id") in byWorkflow
        }
        val band = { steps: Int -> if (steps <= 4) "small" else if (steps <= 12) "medium" else "large" }
        val random = Random(seed)
        val chosen = candidates.groupBy { band(it.getValue("steps").jsonPrimitive.int) }
            .toSortedMap()
            .flatMap { (_, rows) -> rows.shuffled(random).take(count / 3 + 1) }
            .shuffled(random).take(count)
        Files.newBufferedWriter(out.resolve("set.jsonl")).use { writer ->
            chosen.forEach { w ->
                val id = w.str("id")
                val examples = byWorkflow.getValue(id)
                writer.write(buildJsonObject {
                    put("id", id); put("set", w.str("set")); put("name", w.str("name"))
                    put("goal", examples.first().goal)
                    put("steps", w.getValue("steps").jsonPrimitive.int); put("band", band(w.getValue("steps").jsonPrimitive.int))
                    put("referenceTools", buildJsonArray { examples.filter { it.next != LlmChoiceChooser.END }.forEach { add(JsonPrimitive(it.next)) } })
                }.toString())
                writer.newLine()
            }
        }
        println("selected ${chosen.size} of ${candidates.size} held-out workflows: ${chosen.groupingBy { band(it.getValue("steps").jsonPrimitive.int) }.eachCount()}")
    }

    private fun run(derived: Path, dir: Path, contender: String, option: (String) -> String?) {
        val engine = EngineClient(EngineClient.http(option("--engine") ?: "http://127.0.0.1:3142"))
        val catalog = Catalog.parse(Files.readString(dir.resolve("list_tools.json")), Files.readString(dir.resolve("tool_schemas.json")))
        val schema = engine.invoke("get_workflow_schema", JsonObject(emptyMap())).also { check(it.ok) { "engine not reachable: ${it.error}" } }.json.toString()
        val generator = generatorFor(contender, derived, catalog, engine, schema, option)
        val set = jsonLines(dir.resolve("set.jsonl")).let { rows -> option("--limit")?.toInt()?.let { rows.take(it) } ?: rows }
        val out = dir.resolve("results-${fileSafe(contender)}.jsonl")
        val done = if (Files.exists(out)) jsonLines(out).mapTo(hashSetOf()) { it.str("id") } else hashSetOf()
        val todo = set.filter { it.str("id") !in done }
        println("$contender: ${todo.size} to run, ${done.size} already done")

        val pool = Executors.newFixedThreadPool(option("--parallel")?.toInt() ?: 1)
        Files.newBufferedWriter(out, java.nio.file.StandardOpenOption.CREATE, java.nio.file.StandardOpenOption.APPEND).use { writer ->
            todo.forEach { row ->
                pool.submit {
                    val id = row.str("id")
                    val started = System.nanoTime()
                    val outcome = try {
                        generator.generate(id, row.str("goal"))
                    } catch (e: Exception) {
                        Outcome(null, EngineClient.Validation(false, listOf("harness error: ${e.toString().take(300)}")), false, 0, "error")
                    }
                    val seconds = (System.nanoTime() - started) / 1e9
                    val line = resultJson(id, contender, outcome, seconds, catalog).toString()
                    synchronized(writer) { writer.write(line); writer.newLine(); writer.flush() }
                    println(String.format(Locale.ROOT, "  %s valid=%s attempts=%d %.0fs %s", id.take(8), outcome.validation.valid, outcome.attempts, seconds,
                        outcome.validation.errors.firstOrNull()?.take(120).orEmpty()))
                }
            }
            pool.shutdown()
            pool.awaitTermination(7, TimeUnit.DAYS)
        }
    }

    private fun generatorFor(contender: String, derived: Path, catalog: Catalog, engine: EngineClient, schema: String, option: (String) -> String?): WorkflowGenerator {
        val kind = contender.substringBefore(':')
        val model = contender.substringAfter(':')
        return when (kind) {
            "ollama" -> WholeWorkflowGenerator(OllamaText(model), catalog, engine, schema, constrained = true)
            "lmstudio" -> WholeWorkflowGenerator(LmStudioText(model), catalog, engine, schema, constrained = true)
            "claude" -> WholeWorkflowGenerator(ClaudeCli(model), catalog, engine, schema, constrained = false)
            "system1-markov", "system1-classifier" -> {
                val split = Corpus.split(Corpus.loadExamples(derived), evalSets)
                val tools = ToolCatalog.placeable(Corpus.loadTools(derived))
                // The validator wants both Decision arms wired, so the first step on a Decision arm cannot be END.
                val endAllowed = { context: warewolf.studio.generator.ChoiceContext -> context.tail.arm != "True" && context.tail.arm != "False" }
                val chooser = if (kind == "system1-markov") MarkovChooser(split.train, endAllowed) else {
                    val classifier = OnnxClassifier(Path.of(option("--model-dir") ?: error("system1-classifier needs --model-dir")), quantized = false)
                    ClassifierChooser(classifier, classifier.labels, tools, classifier.recentSteps, endAllowed)
                }
                System1Generator(chooser, FieldFiller(OllamaText(model, maxReplyTokens = 2_048)), tools, catalog, engine)
            }
            else -> error("unknown contender '$contender'")
        }
    }

    private fun resultJson(id: String, contender: String, outcome: Outcome, seconds: Double, catalog: Catalog) = buildJsonObject {
        put("id", id); put("contender", contender)
        put("produced", outcome.workflow != null)
        put("valid", outcome.validation.valid)
        put("validIgnoringDeclarations", outcome.workflow != null && outcome.validation.validIgnoringDeclarations)
        put("firstTryValid", outcome.firstTryValid)
        put("attempts", outcome.attempts)
        put("seconds", seconds)
        outcome.note?.let { put("note", it) }
        put("errors", buildJsonArray { outcome.validation.errors.take(10).forEach { add(JsonPrimitive(it)) } })
        put("tools", buildJsonArray {
            outcome.workflow?.let { w -> WorkflowText.toolNames(w.getValue("body").jsonObject, catalog).forEach { add(JsonPrimitive(it)) } }
        })
        outcome.workflow?.let { put("workflow", it) }
    }

    /** Rates every produced workflow that has no verdict yet; the judge sees only the goal and the blind summary. */
    private fun judge(dir: Path, model: String, parallel: Int) {
        val catalog = Catalog.parse(Files.readString(dir.resolve("list_tools.json")), Files.readString(dir.resolve("tool_schemas.json")))
        val goals = jsonLines(dir.resolve("set.jsonl")).associate { it.str("id") to it.str("goal") }
        val out = dir.resolve("judge.jsonl")
        val done = if (Files.exists(out)) jsonLines(out).mapTo(hashSetOf()) { it.str("contender") + "|" + it.str("id") } else hashSetOf()
        val todo = resultFiles(dir).flatMap { jsonLines(it) }
            .filter { it["workflow"] is JsonObject && (it.str("contender") + "|" + it.str("id")) !in done && it.str("id") in goals }
            .shuffled(Random(11))
        println("judging ${todo.size} workflows with $model")
        val judge = Judge(ClaudeCli(model))
        val pool = Executors.newFixedThreadPool(parallel)
        Files.newBufferedWriter(out, java.nio.file.StandardOpenOption.CREATE, java.nio.file.StandardOpenOption.APPEND).use { writer ->
            todo.forEach { row ->
                pool.submit {
                    val verdict = runCatching {
                        judge.rate(goals.getValue(row.str("id")), WorkflowText.summary(row.getValue("workflow").jsonObject, catalog))
                    }.getOrNull()
                    val line = verdictJson(row.str("id"), row.str("contender"), verdict).toString()
                    synchronized(writer) { writer.write(line); writer.newLine(); writer.flush() }
                }
            }
            pool.shutdown()
            pool.awaitTermination(7, TimeUnit.DAYS)
        }
    }

    private fun score(dir: Path) {
        val set = jsonLines(dir.resolve("set.jsonl")).associateBy { it.str("id") }
        val verdicts = if (Files.exists(dir.resolve("judge.jsonl"))) {
            jsonLines(dir.resolve("judge.jsonl")).filter { it["score"] != null }.associate { (it.str("contender") + "|" + it.str("id")) to it.getValue("score").jsonPrimitive.int }
        } else emptyMap()
        println("| Contender | Runs | Produced | Valid 1st try | Valid | Valid ignoring declarations | Tool F1 | Steps vs author | Branching agrees | Judge mean | Judge ≥4 | Median s |")
        println("|---|---|---|---|---|---|---|---|---|---|---|---|")
        resultFiles(dir).forEach { file ->
            val rows = jsonLines(file).filter { it.str("id") in set }
            if (rows.isEmpty()) return@forEach
            val pct = { n: Int -> String.format(Locale.ROOT, "%.0f%%", 100.0 * n / rows.size) }
            val produced = rows.filter { it.getValue("produced").jsonPrimitive.boolean }
            val f1 = rows.map { r ->
                val tools = r.getValue("tools").jsonArray.map { it.jsonPrimitive.content }
                Structure.toolF1(tools, set.getValue(r.str("id")).getValue("referenceTools").jsonArray.map { it.jsonPrimitive.content })
            }.average()
            val ratios = produced.map { r -> r.getValue("tools").jsonArray.size.toDouble() / set.getValue(r.str("id")).getValue("steps").jsonPrimitive.int }.sorted()
            val branchAgree = rows.count { r ->
                Structure.branches(r.getValue("tools").jsonArray.map { it.jsonPrimitive.content }) ==
                    Structure.branches(set.getValue(r.str("id")).getValue("referenceTools").jsonArray.map { it.jsonPrimitive.content })
            }
            val scores = rows.mapNotNull { verdicts[it.str("contender") + "|" + it.str("id")] }
            val seconds = rows.map { it.getValue("seconds").jsonPrimitive.double }.sorted()
            println(String.format(Locale.ROOT, "| %s | %d | %s | %s | %s | %s | %.2f | %s | %s | %s | %s | %.0f |",
                rows.first().str("contender"), rows.size, pct(produced.size),
                pct(rows.count { it.getValue("firstTryValid").jsonPrimitive.boolean }),
                pct(rows.count { it.getValue("valid").jsonPrimitive.boolean }),
                pct(rows.count { it.getValue("validIgnoringDeclarations").jsonPrimitive.boolean }),
                f1, if (ratios.isEmpty()) "—" else String.format(Locale.ROOT, "%.2f×", ratios[ratios.size / 2]), pct(branchAgree),
                if (scores.isEmpty()) "—" else String.format(Locale.ROOT, "%.2f (%d)", scores.average(), scores.size),
                if (scores.isEmpty()) "—" else String.format(Locale.ROOT, "%.0f%%", 100.0 * scores.count { it >= 4 } / rows.size),
                seconds[seconds.size / 2]))
        }
    }

    /**
     * A blind sample for a person to rate, to check the judge: [count] judged workflows spread across contenders,
     * each shown as exactly the summary the judge saw. `spotcheck-items.json` holds what to show; the contender and
     * the judge's score sit apart in `spotcheck-key.json`.
     */
    private fun spotcheck(dir: Path, count: Int) {
        val catalog = Catalog.parse(Files.readString(dir.resolve("list_tools.json")), Files.readString(dir.resolve("tool_schemas.json")))
        val goals = jsonLines(dir.resolve("set.jsonl")).associate { it.str("id") to it.str("goal") }
        val verdicts = jsonLines(dir.resolve("judge.jsonl")).filter { it["score"] != null }
            .associate { (it.str("contender") + "|" + it.str("id")) to it.getValue("score").jsonPrimitive.int }
        val random = Random(23)
        val byContender = resultFiles(dir).map { file ->
            jsonLines(file).filter { it["workflow"] is JsonObject && (it.str("contender") + "|" + it.str("id")) in verdicts }.shuffled(random)
        }.filter { it.isNotEmpty() }
        // Round-robin over contenders, so every one is represented before any appears twice.
        val picked = generateSequence(0) { it + 1 }.takeWhile { round -> byContender.any { it.size > round } }
            .flatMap { round -> byContender.mapNotNull { it.getOrNull(round) } }.take(count).toList().shuffled(random)
        val items = buildJsonArray {
            picked.forEachIndexed { i, row ->
                add(buildJsonObject {
                    put("item", i + 1)
                    put("goal", goals.getValue(row.str("id")))
                    put("summary", WorkflowText.summary(row.getValue("workflow").jsonObject, catalog))
                })
            }
        }
        val key = buildJsonArray {
            picked.forEachIndexed { i, row ->
                add(buildJsonObject {
                    put("item", i + 1); put("id", row.str("id")); put("contender", row.str("contender"))
                    put("judgeScore", verdicts.getValue(row.str("contender") + "|" + row.str("id")))
                })
            }
        }
        Files.writeString(dir.resolve("spotcheck-items.json"), items.toString())
        Files.writeString(dir.resolve("spotcheck-key.json"), key.toString())
        println("wrote ${picked.size} spot-check items")
    }

    internal fun resultFiles(dir: Path): List<Path> =
        Files.list(dir).use { files -> files.filter { it.fileName.toString().startsWith("results-") && it.toString().endsWith(".jsonl") }.sorted().toList() }

    internal fun fileSafe(name: String) = name.replace(Regex("[^A-Za-z0-9.]"), "_")

    internal fun jsonLines(file: Path): List<JsonObject> =
        Files.readAllLines(file).filter { it.isNotBlank() }.map { Json.parseToJsonElement(it).jsonObject }

    private fun JsonObject.str(key: String) = getValue(key).jsonPrimitive.content

    private fun kotlinx.serialization.json.JsonElement?.isNullJson() = this == null || this is kotlinx.serialization.json.JsonNull
}
