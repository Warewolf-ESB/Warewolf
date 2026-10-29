package warewolf.studio.eval.bakeoff

import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.jsonObject

val testCatalog = Catalog(listOf(
    CatalogTool("Assign", "dsfdotnetmultiassignactivity", "DsfDotNetMultiAssignActivity", "Data", "Assigns values.", false,
        mapOf("fields" to "array, required — a JSON array of { FieldName, FieldValue }.")),
    CatalogTool("Decision", "flowdecision", "DsfFlowDecisionActivity", "Control Flow", "Branches.", false,
        mapOf("expression" to "string, required — the condition.", "and" to "boolean, optional — AND-combine.")),
    CatalogTool("Switch", "dsfflowswitchactivity", "DsfFlowSwitchActivity", "Control Flow", "Branches across cases.", false,
        mapOf("switch" to "string, required — the value to switch on.")),
    CatalogTool("SQL Server Database", "dsfsqlserverdatabaseactivity", "DsfSqlServerDatabaseActivity", "Database", "Runs SQL.", true, emptyMap()),
))

/** An engine whose validate_workflow answers with [validation] and records every call. */
class FakeEngine(var validation: String = """{"valid":true,"errors":[]}""") {
    val calls = mutableListOf<Pair<String, JsonObject>>()
    val client = EngineClient { tool, body ->
        calls += tool to Json.parseToJsonElement(body).jsonObject
        when (tool) {
            "validate_workflow" -> 200 to validation
            else -> 400 to """{"error":"bad_request","message":"not faked: $tool"}"""
        }
    }
}

/** A text model that answers with the replies in order (repeating the last) and records its prompts. */
class ScriptedModel(vararg replies: String) : TextModel {
    private val queue = ArrayDeque(replies.toList())
    val prompts = mutableListOf<String>()
    val formats = mutableListOf<JsonObject?>()

    override fun complete(prompt: String, format: JsonObject?): String {
        prompts += prompt
        formats += format
        return if (queue.size > 1) queue.removeFirst() else queue.first()
    }
}

const val VALID_WORKFLOW = """{"envelope":{"name":"w","inputs":[],"outputs":[{"kind":"scalar","name":"a"}]},
 "body":{"resourcename":"w","cells":[{"id":"start","shape":"start","data":{"type":"start"}},
 {"id":"n1","shape":"dsfdotnetmultiassignactivity","data":{"type":"dsfdotnetmultiassignactivity","displayname":"Set a","fields":[{"FieldName":"[[a]]","FieldValue":"1"}]}},
 {"shape":"edge","source":{"cell":"start"},"target":{"cell":"n1"}}]}}"""
