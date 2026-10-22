/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetBiteDefinitionSchemaTool: the generated JSON Schema of the version 1
 *  <BiteDefinition> body (WF removal T3.1) is returned whole, with its format version.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetBiteDefinitionSchemaToolTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ReturnsTheVersion1SchemaWithEveryActivity()
        {
            var result = GetBiteDefinitionSchemaTool.Handle();

            Assert.AreEqual(1, result.FormatVersion);
            Assert.AreEqual("https://json-schema.org/draft/2020-12/schema", result.Schema.GetProperty("$schema").GetString());
            var definitions = result.Schema.GetProperty("$defs");
            Assert.IsTrue(definitions.GetProperty("activity").GetProperty("anyOf").GetArrayLength() > 100,
                "The schema must name every activity a .bite can hold.");
            Assert.AreEqual(JsonValueKind.Object,
                definitions.GetProperty("Unlimited.Applications.BusinessDesignStudio.Activities.DsfCommentActivity").ValueKind);
        }
    }
}
