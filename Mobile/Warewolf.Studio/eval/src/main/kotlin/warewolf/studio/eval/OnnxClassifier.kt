package warewolf.studio.eval

import ai.onnxruntime.OnnxTensor
import ai.onnxruntime.OrtEnvironment
import ai.onnxruntime.OrtSession
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.int
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import warewolf.studio.generator.Classifier
import warewolf.studio.generator.WordPieceTokenizer
import java.nio.file.Files
import java.nio.file.Path

/**
 * The trained System 1 classifier as `train/train.py` exports it: `model.int8.onnx` (or `model.onnx` when
 * [quantized] is false), `vocab.txt`, `labels.json` and `config.json`, run on the desktop with ONNX Runtime.
 */
class OnnxClassifier(dir: Path, quantized: Boolean = true) : Classifier, AutoCloseable {
    private val config = Json.parseToJsonElement(Files.readString(dir.resolve("config.json"))).jsonObject
    val labels: List<String> = Json.parseToJsonElement(Files.readString(dir.resolve("labels.json"))).jsonArray.map { it.jsonPrimitive.content }
    val recentSteps: Int = config.getValue("recentSteps").jsonPrimitive.int
    val tokenizer = WordPieceTokenizer(
        Files.readAllLines(dir.resolve("vocab.txt")),
        lowerCase = config.getValue("lowerCase").jsonPrimitive.boolean,
        maxLength = config.getValue("maxLength").jsonPrimitive.int,
    )
    private val env = OrtEnvironment.getEnvironment()
    private val session = env.createSession(dir.resolve(if (quantized) "model.int8.onnx" else "model.onnx").toString(), OrtSession.SessionOptions())

    init {
        check(config["truncation"]?.jsonPrimitive?.content == "left") { "the tokenizer only truncates from the left" }
    }

    override fun scores(text: String): FloatArray {
        val ids = tokenizer.encode(text).map(Int::toLong).toLongArray()
        val shape = longArrayOf(1, ids.size.toLong())
        OnnxTensor.createTensor(env, java.nio.LongBuffer.wrap(ids), shape).use { inputIds ->
            OnnxTensor.createTensor(env, java.nio.LongBuffer.wrap(LongArray(ids.size) { 1 }), shape).use { mask ->
                session.run(mapOf("input_ids" to inputIds, "attention_mask" to mask)).use { result ->
                    @Suppress("UNCHECKED_CAST")
                    return (result.get(0).value as Array<FloatArray>)[0]
                }
            }
        }
    }

    override fun close() = session.close()
}
