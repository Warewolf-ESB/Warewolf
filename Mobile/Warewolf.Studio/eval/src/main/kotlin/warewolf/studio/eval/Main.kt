package warewolf.studio.eval

import warewolf.studio.generator.BranchTail
import warewolf.studio.generator.ChoiceContext
import warewolf.studio.generator.InvalidChoiceException
import warewolf.studio.generator.LlmChoiceChooser
import warewolf.studio.generator.ToolCatalog
import warewolf.studio.generator.Action
import java.nio.file.Files
import java.nio.file.Path
import java.util.Locale

/**
 * System 1 baseline evaluation (Plan.md §2, option b): next-tool accuracy of the small LLM on held-out corpus
 * sets, beside a majority-class floor so the number has something to beat.
 *
 *     gradlew :eval:run --args="<derivedDir> [--model qwen2.5:0.5b] [--eval-sets caps,asws] [--limit N]"
 */
fun main(args: Array<String>) {
    val dir = Path.of(args.firstOrNull() ?: error("usage: <derivedDir> [--model m] [--eval-sets a,b] [--limit N]"))
    val option = { name: String -> args.indexOf(name).takeIf { it >= 0 }?.let { args.getOrNull(it + 1) } }
    val model = option("--model") ?: "qwen2.5:0.5b"
    val evalSets = (option("--eval-sets") ?: "caps,asws").split(',').toSet()
    val limit = option("--limit")?.toInt()

    val tools = ToolCatalog.placeable(Corpus.loadTools(dir))
    val placeable = tools.mapTo(hashSetOf()) { it.name } + LlmChoiceChooser.END
    val split = Corpus.split(Corpus.loadExamples(dir), evalSets)
    // System 1 can only answer with a placeable tool or END, so examples whose answer is anything else are not scored.
    val scored = split.eval.filter { it.answerIsKnown && it.next in placeable }.let { if (limit != null) it.take(limit) else it }

    println("train examples ${split.train.size}; eval examples ${split.eval.size} (dropped ${split.droppedForLeakage} sharing a family with training); scored ${scored.size}")

    val majority = split.train.filter { it.answerIsKnown }.groupingBy { it.next }.eachCount().maxBy { it.value }.key
    printReport("majority class ('$majority')", score(scored.map { Prediction(it.next, majority) }, LlmChoiceChooser.END))

    val chooser = LlmChoiceChooser(OllamaClient(model))
    val predictions = mutableListOf<Prediction>()
    var invalid = 0
    Files.newBufferedWriter(dir.resolve("predictions-${model.replace(Regex("[^A-Za-z0-9.]"), "_")}.jsonl")).use { out ->
        scored.forEachIndexed { i, example ->
            val context = ChoiceContext(example.goal, BranchTail(afterStepId = null, arm = example.arm, path = example.path), tools)
            val predicted = try {
                when (val action = chooser.choose(context)) {
                    is Action.End -> LlmChoiceChooser.END
                    is Action.Place -> action.tool.name
                }
            } catch (e: InvalidChoiceException) {
                invalid++
                "<invalid>"
            }
            predictions += Prediction(example.next, predicted)
            out.write("""{"id":"${example.workflowId}","gold":"${example.next}","predicted":"$predicted"}""")
            out.newLine()
            if ((i + 1) % 100 == 0) println("  ${i + 1}/${scored.size}")
        }
    }

    printReport("$model (invalid answers: $invalid)", score(predictions, LlmChoiceChooser.END))
}

private fun printReport(title: String, report: Report) {
    println()
    println(title)
    println(String.format(Locale.ROOT, "  accuracy      %.1f%% (%d/%d)", report.accuracy * 100, report.correct, report.total))
    println(String.format(Locale.ROOT, "  END accuracy  %.1f%% (%d/%d)", report.endAccuracy * 100, report.endCorrect, report.endTotal))
    report.topConfusions.forEach { (pair, count) -> println("  ${count.toString().padStart(5)}  ${pair.first} -> ${pair.second}") }
}
