package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.JsonNull
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.buildJsonArray
import kotlinx.serialization.json.buildJsonObject
import kotlinx.serialization.json.contentOrNull
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlinx.serialization.json.put

/** One toolbox entry as `list_tools` reports it, with its field schema from `get_tool_schema`. */
data class CatalogTool(
    val name: String,
    val dataType: String,
    val activityType: String,
    val category: String,
    val description: String,
    val requiresSource: Boolean,
    /** Field name → the schema's prose description ("array, required — …"). */
    val fields: Map<String, String>,
)

/**
 * The toolbox every contender is shown, and the mapping from a node's `data.type` back to a tool name. Generated
 * bodies name a tool by its dataType; definitions the engine writes back use the assembly-qualified class name.
 */
class Catalog(val tools: List<CatalogTool>) {
    private val byKey: Map<String, CatalogTool> = buildMap {
        tools.forEach { put(it.dataType.lowercase(), it); put(it.activityType.lowercase(), it) }
    }

    /** The tool a node's `data.type` names, or null for `start` and anything unknown. */
    fun toolFor(type: String?): CatalogTool? {
        if (type == null) return null
        val simple = type.substringBefore(',').substringAfterLast('.').lowercase()
        return byKey[type.lowercase()] ?: byKey[simple]
    }

    fun named(name: String): CatalogTool? = tools.firstOrNull { it.name == name }

    companion object {
        /** `list_tools.json` and `tool_schemas.json` as the comparison's setup saves them. */
        fun parse(listTools: String, toolSchemas: String): Catalog {
            val schemas = Json.parseToJsonElement(toolSchemas).jsonObject
            return Catalog(Json.parseToJsonElement(listTools).jsonObject.getValue("tools").jsonArray.map { it.jsonObject }.map { t ->
                val name = t.str("name")
                val fields = (schemas[name]?.jsonObject?.get("schema") as? JsonObject)?.get("fields") as? JsonObject
                CatalogTool(
                    name = name,
                    dataType = t.str("dataType"),
                    activityType = t.str("activityType"),
                    category = t.str("category"),
                    description = t.str("description"),
                    requiresSource = t["requiresSource"]?.jsonPrimitive?.content == "true",
                    fields = fields?.mapValues { (_, v) -> (v as? JsonPrimitive)?.content ?: v.toString() }.orEmpty(),
                )
            })
        }

        private fun JsonObject.str(key: String) = getValue(key).jsonPrimitive.content
    }
}

object WorkflowText {
    /**
     * The instructions every whole-workflow contender gets, small model or frontier: the output contract, the
     * rules the validator enforces, and the full toolbox with each tool's fields.
     */
    fun prompt(goal: String, catalog: Catalog, workflowSchema: String): String = buildString {
        appendLine("You are an expert Warewolf workflow author. Build ONE Warewolf workflow that achieves the goal below.")
        appendLine()
        appendLine("Reply with a single JSON object and nothing else: {\"envelope\": {...}, \"body\": {...}}, following this schema:")
        appendLine(workflowSchema)
        appendLine()
        appendLine("Rules:")
        appendLine("- body.cells holds nodes and edges. Include exactly one start node: {\"id\": \"start\", \"shape\": \"start\", \"data\": {\"type\": \"start\"}}.")
        appendLine("- Every other node has a unique id, \"shape\" and \"data.type\" set to the tool's dataType, \"data.displayname\" set to a short label, and the tool's fields inside \"data\".")
        appendLine("- Connect steps with edges {\"shape\": \"edge\", \"source\": {\"cell\": <id>}, \"target\": {\"cell\": <id>}}, starting from the start node.")
        appendLine("- A Decision node needs two outgoing edges, one with \"data\": {\"isDecisionArm\": true, \"isTrue\": true} and one with \"data\": {\"isDecisionArm\": true, \"isTrue\": false}.")
        appendLine("- A Switch node needs one outgoing edge per case, with the case name as the edge's \"label\".")
        appendLine("- Variables are written [[name]], [[recordset(*).field]] or [[@object.property]]. Every variable the workflow uses must be declared in envelope.inputs or envelope.outputs.")
        appendLine("- No other workflows, database sources, web sources or email servers exist. Tools marked (needs a source) cannot be configured; avoid them unless the goal is impossible without them.")
        appendLine()
        appendLine("Tools (name | dataType | category | description) and their fields:")
        catalog.tools.forEach { t ->
            appendLine("* ${t.name} | ${t.dataType} | ${t.category} | ${t.description}${if (t.requiresSource) " (needs a source)" else ""}")
            t.fields.forEach { (field, description) -> appendLine("    - $field: $description") }
        }
        appendLine()
        appendLine("Goal: $goal")
    }

    fun repairPrompt(previous: String, errors: List<String>): String = buildString {
        appendLine("Your workflow was rejected by the Warewolf validator:")
        errors.take(MAX_ERRORS_SHOWN).forEach { appendLine("- $it") }
        if (errors.size > MAX_ERRORS_SHOWN) appendLine("- … and ${errors.size - MAX_ERRORS_SHOWN} more")
        appendLine()
        appendLine("Here is the workflow you sent:")
        appendLine(previous)
        appendLine()
        appendLine("Reply with the corrected workflow as a single JSON object {\"envelope\": ..., \"body\": ...} and nothing else.")
    }

    private const val MAX_ERRORS_SHOWN = 15

    /**
     * The `{envelope, body}` object in a model's reply: the first fenced ```json block if there is one, otherwise
     * the text from the first `{` to the last `}`. Null when no such object parses.
     */
    fun extractWorkflow(reply: String): JsonObject? {
        val fenced = Regex("```(?:json)?\\s*\\n([\\s\\S]*?)```").find(reply)?.groupValues?.get(1)
        val candidates = listOfNotNull(fenced, reply.substring(reply.indexOf('{').coerceAtLeast(0), (reply.lastIndexOf('}') + 1).coerceAtLeast(0)))
        for (text in candidates) {
            val parsed = runCatching { Json.parseToJsonElement(text.trim()) }.getOrNull() as? JsonObject ?: continue
            if (parsed["envelope"] is JsonObject && parsed["body"] is JsonObject) return parsed
        }
        return null
    }

    /** [reply] as a JSON object, or the text from its first `{` to its last `}` as one; null when neither parses. */
    fun firstObject(reply: String): JsonObject? {
        runCatching { Json.parseToJsonElement(reply.trim()) }.getOrNull()?.let { return it as? JsonObject }
        val start = reply.indexOf('{')
        val end = reply.lastIndexOf('}')
        if (start < 0 || end <= start) return null
        return runCatching { Json.parseToJsonElement(reply.substring(start, end + 1)) }.getOrNull() as? JsonObject
    }

    /** The outermost `[[…]]` expressions in [text], brackets removed; `[[rs([[i]]).f]]` is one. */
    fun variables(text: String): List<String> {
        val out = mutableListOf<String>()
        var depth = 0
        var start = 0
        var i = 0
        while (i < text.length - 1) {
            if (text[i] == '[' && text[i + 1] == '[') {
                if (depth++ == 0) start = i + 2
                i += 2
            } else if (text[i] == ']' && text[i + 1] == ']' && depth > 0) {
                if (--depth == 0 && i > start) out += text.substring(start, i)
                i += 2
            } else {
                i++
            }
        }
        return out
    }

    /** Every variable mentioned in any string inside [element], in order, without repeats. */
    fun variablesIn(element: JsonElement): List<String> {
        val found = linkedSetOf<String>()
        fun walk(e: JsonElement) {
            when (e) {
                is JsonObject -> e.values.forEach(::walk)
                is JsonArray -> e.forEach(::walk)
                is JsonPrimitive -> if (e.isString) found += variables(e.content)
                JsonNull -> {}
            }
        }
        walk(element)
        return found.toList()
    }

    /**
     * Envelope entries declaring [variables] as outputs: `rs(*).f` as recordset `rs` with field `f`, `@o.p` as
     * object `o`, anything else as a scalar. Expressions with nested variables declare those too.
     */
    fun declarations(variables: List<String>): JsonArray {
        val scalars = linkedSetOf<String>()
        val objects = linkedSetOf<String>()
        val recordsets = linkedMapOf<String, LinkedHashSet<String>>()
        fun declare(expression: String) {
            variables(expression).forEach(::declare)
            val e = expression.trim()
            when {
                e.startsWith("@") -> objects += e.drop(1).split('.', '(', '[').first()
                '(' in e -> {
                    val field = e.substringAfter(").", "").substringBefore('.').trim()
                    recordsets.getOrPut(e.substringBefore('(').trim()) { linkedSetOf() }.apply { if (field.isNotEmpty() && "[[" !in field) add(field) }
                }
                "[[" !in e && e.isNotEmpty() -> scalars += e
            }
        }
        variables.forEach(::declare)
        return buildJsonArray {
            scalars.forEach { add(buildJsonObject { put("kind", "scalar"); put("name", it) }) }
            objects.forEach { add(buildJsonObject { put("kind", "object"); put("name", it) }) }
            recordsets.forEach { (name, fields) ->
                add(buildJsonObject {
                    put("kind", "recordset"); put("name", name)
                    put("fields", buildJsonArray { fields.forEach { add(JsonPrimitive(it)) } })
                })
            }
        }
    }

    /** The tool names of a body's nodes, in cell order; the start node and edges are left out. */
    fun toolNames(body: JsonObject, catalog: Catalog): List<String> = nodes(body).mapNotNull { node ->
        val type = (node["data"] as? JsonObject)?.get("type")?.jsonPrimitive?.contentOrNull
        if (type.equals("start", ignoreCase = true)) null else catalog.toolFor(type)?.name ?: "?$type"
    }

    /**
     * What the judge sees of a workflow, the same for every contender so it cannot tell them apart: each step's
     * tool, label and filled-in fields (engine bookkeeping such as positions and Studio properties dropped), then
     * each connection with its arm or case. [showIds] names steps by their node ids instead of S1, S2, …, for a
     * model that has to refer to them (a service test's `activityId`).
     */
    fun summary(workflow: JsonObject, catalog: Catalog, showIds: Boolean = false): String = buildString {
        val body = workflow.getValue("body").jsonObject
        val envelope = workflow["envelope"] as? JsonObject
        val ids = mutableMapOf<String, String>()
        appendLine("Inputs: ${names(envelope?.get("inputs"))}")
        appendLine("Outputs: ${names(envelope?.get("outputs"))}")
        appendLine("Steps:")
        nodes(body).forEach { node ->
            val data = node["data"] as? JsonObject ?: JsonObject(emptyMap())
            val type = data["type"]?.jsonPrimitive?.contentOrNull
            val id = node["id"]?.jsonPrimitive?.contentOrNull ?: return@forEach
            val label = if (type.equals("start", true)) "start" else if (showIds) id else "S${ids.size}"
            ids[id] = label
            if (label == "start") return@forEach
            val tool = catalog.toolFor(type)?.name ?: "unknown type $type"
            val fields = JsonObject(data.filterKeys { it !in IGNORED_KEYS }.mapValues { (_, v) -> prune(v) }.filterValues { it != null }.mapValues { it.value!! })
            appendLine("  $label: $tool \"${data["displayname"]?.jsonPrimitive?.contentOrNull.orEmpty()}\" $fields")
        }
        appendLine("Connections:")
        edges(body).forEach { edge ->
            val from = ids[edge["source"]?.jsonObject?.get("cell")?.jsonPrimitive?.contentOrNull] ?: "?"
            val to = ids[edge["target"]?.jsonObject?.get("cell")?.jsonPrimitive?.contentOrNull] ?: "?"
            val data = edge["data"] as? JsonObject
            val arm = when {
                data?.get("isDecisionArm")?.jsonPrimitive?.contentOrNull == "true" ->
                    if (data["isTrue"]?.jsonPrimitive?.contentOrNull == "true") " (true)" else " (false)"
                !edge["label"]?.jsonPrimitive?.contentOrNull.isNullOrBlank() -> " (case ${edge["label"]!!.jsonPrimitive.content})"
                else -> ""
            }
            appendLine("  $from -> $to$arm")
        }
    }

    private val IGNORED_KEYS = setOf("type", "displayname", "properties", "onerrordata", "UniqueID", "id", "isNested", "parentId")

    /** Drops nulls, empty values and Studio-only bookkeeping keys, so the judge sees what the author filled in. */
    private fun prune(e: JsonElement): JsonElement? = when (e) {
        JsonNull -> null
        is JsonPrimitive -> if (e.isString && e.content.isEmpty()) null else e
        is JsonArray -> JsonArray(e.mapNotNull(::prune)).takeIf { it.isNotEmpty() }
        is JsonObject -> JsonObject(e.filterKeys { it !in BOOKKEEPING }.mapValues { (_, v) -> prune(v) }.filterValues { it != null }.mapValues { it.value!! })
            .takeIf { it.isNotEmpty() }
    }

    private val BOOKKEEPING = setOf(
        "WatermarkTextVariable", "WatermarkTextValue", "IndexNumber", "Inserted", "OutList", "ErrorMessage", "HasError",
        "IsFieldNameFocused", "IsFieldValueFocused", "Error", "Errors", "Path",
    )

    private fun names(entries: JsonElement?): String =
        (entries as? JsonArray)?.mapNotNull { (it as? JsonObject)?.get("name")?.jsonPrimitive?.contentOrNull }?.joinToString(", ").orEmpty().ifEmpty { "(none)" }

    fun nodes(body: JsonObject): List<JsonObject> =
        (body["cells"] as? JsonArray).orEmpty().mapNotNull { it as? JsonObject }.filter { it["shape"]?.jsonPrimitive?.contentOrNull != "edge" && it["source"].let { s -> s == null || s is JsonNull } }

    fun edges(body: JsonObject): List<JsonObject> =
        (body["cells"] as? JsonArray).orEmpty().mapNotNull { it as? JsonObject }.filter { it["shape"]?.jsonPrimitive?.contentOrNull == "edge" || it["source"] is JsonObject }
}
