package warewolf.studio.eval

import warewolf.studio.generator.InvalidChoiceException
import warewolf.studio.generator.LlmChoiceChooser
import warewolf.studio.generator.NextActionChooser
import warewolf.studio.generator.ToolCatalog
import warewolf.studio.generator.ClassifierChooser
import warewolf.studio.generator.Action
import java.nio.file.Files
import java.nio.file.Path
import java.util.Locale

/**
 * System 1 baseline evaluation (Plan.md §2, option b): next-tool accuracy of the small LLM on held-out corpus
 * sets, beside a majority-class floor so the number has something to beat.
 *
 *     gradlew :eval:run --args="<derivedDir> [--model qwen2.5:0.5b] [--eval-sets caps,asws] [--limit N] [--state names|rich]"
 *
 * `--state rich` shows System 1 each earlier step's label and variables, not just its tool name.
 * `--client markov` scores the no-model baseline ([MarkovChooser]) instead of a language model.
 * `--client classifier --model-dir <dir> [--float]` scores the trained classifier ([OnnxClassifier]); it always
 * sees rich state.
 * `--export-features <outDir>` writes the trained classifier's training data ([Features]) and scores nothing.
 */
fun main(args: Array<String>) {
    // The whole-workflow comparison has its own commands (bakeoff/BakeOff.kt).
    if (args.firstOrNull() == "bakeoff") return warewolf.studio.eval.bakeoff.BakeOff.main(args.drop(1))
    val dir = Path.of(args.firstOrNull() ?: error("usage: <derivedDir> [--model m] [--eval-sets a,b] [--limit N]"))
    val option = { name: String -> args.indexOf(name).takeIf { it >= 0 }?.let { args.getOrNull(it + 1) } }
    val model = option("--model") ?: "qwen2.5:0.5b"
    val evalSets = (option("--eval-sets") ?: "caps,asws").split(',').toSet()
    val limit = option("--limit")?.toInt()
    val richState = when (val state = option("--state") ?: "names") {
        "names" -> false
        "rich" -> true
        else -> error("--state must be names or rich, not '$state'")
    }

    val split = Corpus.split(Corpus.loadExamples(dir), evalSets)
    // Small models favour early options, so list the tools most used in training first (END stays last).
    val trainFrequency = split.train.groupingBy { it.next }.eachCount()
    val tools = ToolCatalog.placeable(Corpus.loadTools(dir)).sortedByDescending { trainFrequency[it.name] ?: 0 }
    val placeable = tools.mapTo(hashSetOf()) { it.name } + LlmChoiceChooser.END
    // System 1 can only answer with a placeable tool or END, so examples whose answer is anything else are not scored.
    val scored = split.eval.filter { it.answerIsKnown && it.next in placeable }.let { if (limit != null) it.take(limit) else it }

    option("--export-features")?.let { out ->
        val counts = Features.export(split, ToolCatalog.placeable(Corpus.loadTools(dir)), Path.of(out))
        println("features written to $out: $counts")
        return
    }

    println("train examples ${split.train.size}; eval examples ${split.eval.size} (dropped ${split.droppedForLeakage} sharing a family with training); scored ${scored.size}")

    val majority = split.train.filter { it.answerIsKnown }.groupingBy { it.next }.eachCount().maxBy { it.value }.key
    printReport("majority class ('$majority')", score(scored.map { Prediction(it.next, majority) }, LlmChoiceChooser.END))

    // --client jev compares TypeSafe's Jev (cloud; needs JEV_API_KEY) against the local Ollama baselines;
    // --two-step asks Jev "finished?" and "which tool?" separately (JevTwoStepChooser).
    val useJev = option("--client") == "jev"
    val useMarkov = option("--client") == "markov"
    val classifier = if (option("--client") == "classifier") {
        OnnxClassifier(Path.of(option("--model-dir") ?: error("--client classifier needs --model-dir")), quantized = "--float" !in args)
    } else null
    val twoStep = useJev && "--two-step" in args
    val jevModel = option("--model") ?: "jev-latest"
    val runName = when {
        twoStep -> "$jevModel-two-step"
        useJev -> jevModel
        useMarkov -> "markov"
        classifier != null -> "classifier-" + Path.of(option("--model-dir")!!).fileName + if ("--float" in args) "-float" else ""
        else -> model
    } + if (richState) "-rich" else ""
    val jevClient = if (useJev && !twoStep) JevClient(model = jevModel) else null
    val twoStepChooser = if (twoStep) JevTwoStepChooser(option("--end-threshold")?.toDouble() ?: 0.5, jevModel) else null
    val chooser: NextActionChooser = when {
        twoStepChooser != null -> twoStepChooser
        useMarkov -> MarkovChooser(split.train)
        classifier != null -> ClassifierChooser(classifier, classifier.labels, tools, classifier.recentSteps)
        else -> LlmChoiceChooser(jevClient ?: OllamaClient(model))
    }
    val predictions = mutableListOf<Prediction>()
    val twoStepAnswers = mutableListOf<Pair<Double, String?>>()
    var invalid = 0
    // One file per model, so runs of different clients never overwrite each other.
    Files.newBufferedWriter(dir.resolve("predictions-${runName.replace(Regex("[^A-Za-z0-9.]"), "_")}.jsonl")).use { out ->
        scored.forEachIndexed { i, example ->
            val context = example.context(tools, richState || classifier != null)
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
            // Two-step runs also keep Jev's raw answers, so other END thresholds can be scored without new calls.
            val raw = twoStepChooser?.lastFinished?.let { finished ->
                twoStepAnswers += finished to twoStepChooser.lastTool
                ""","finished":$finished,"tool":"${twoStepChooser.lastTool}""""
            } ?: ""
            out.write("""{"id":"${example.workflowId}","gold":"${example.next}","predicted":"$predicted"$raw}""")
            out.newLine()
            if ((i + 1) % 100 == 0) println("  ${i + 1}/${scored.size}")
        }
    }

    val label = when {
        twoStepChooser != null -> "Jev ${twoStepChooser.resolvedModel ?: "?"}, two-step"
        jevClient != null -> "Jev ${jevClient.resolvedModel ?: "?"}"
        useMarkov -> "Markov (previous tool and arm)"
        classifier != null -> "classifier ${option("--model-dir")}"
        else -> model
    } + if (richState) ", rich state" else ""
    printReport("$label (invalid answers: $invalid)", score(predictions, LlmChoiceChooser.END))

    if (twoStepAnswers.size == predictions.size) {
        // Indicative only: picking the threshold on the eval set itself flatters the result.
        println()
        println("END threshold sweep (re-scored from the recorded answers)")
        for (step in 1..9) {
            val threshold = step / 10.0
            val swept = score(predictions.zip(twoStepAnswers) { p, (finished, tool) ->
                Prediction(p.gold, if (finished >= threshold) LlmChoiceChooser.END else tool ?: "<invalid>")
            }, LlmChoiceChooser.END, confusions = 0)
            println(String.format(Locale.ROOT, "  %.1f  accuracy %.1f%%  END %.1f%%", threshold, swept.accuracy * 100, swept.endAccuracy * 100))
        }
    }
}

private fun printReport(title: String, report: Report) {
    println()
    println(title)
    println(String.format(Locale.ROOT, "  accuracy      %.1f%% (%d/%d)", report.accuracy * 100, report.correct, report.total))
    println(String.format(Locale.ROOT, "  END accuracy  %.1f%% (%d/%d)", report.endAccuracy * 100, report.endCorrect, report.endTotal))
    report.topConfusions.forEach { (pair, count) -> println("  ${count.toString().padStart(5)}  ${pair.first} -> ${pair.second}") }
}
