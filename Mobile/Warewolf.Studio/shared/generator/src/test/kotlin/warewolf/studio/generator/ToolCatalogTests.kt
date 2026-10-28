package warewolf.studio.generator

import kotlin.test.Test
import kotlin.test.assertEquals

class ToolCatalogTests {
    @Test
    fun `placeable drops the legacy decision and keeps catalog order`() {
        assertEquals(listOf(assign, calculate, decision, switch), ToolCatalog.placeable(catalog))
    }
}
