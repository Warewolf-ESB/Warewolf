/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ListToolsTool (warewolf-lee-mcp-v3-spec.md, "Tools" §
 *  list_tools): verifies the response shape mirrors ToolCatalog exactly,
 *  including the "primary data type" projection and always-true editable flag.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class ListToolsToolTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ReturnsOneToolPerCatalogEntry()
        {
            var result = ListToolsTool.Handle();

            Assert.AreEqual(ToolCatalog.Entries.Count, result.Tools.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EveryToolIsAlwaysEditable()
        {
            var result = ListToolsTool.Handle();

            Assert.IsTrue(result.Tools.All(t => t.Editable),
                "list_tools reports toolbox capability, not per-workflow fidelity — every entry must be editable:true.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_MapsFieldsFromCatalogEntry_ForKnownRow()
        {
            var assign = ToolCatalog.Entries.Single(e => e.Name == "Assign");

            var result = ListToolsTool.Handle();
            var tool = result.Tools.Single(t => t.Name == "Assign");

            Assert.AreEqual(assign.ActivityType, tool.ActivityType);
            Assert.AreEqual(assign.DataTypes[0], tool.DataType);
            Assert.AreEqual(assign.Category, tool.Category);
            Assert.AreEqual(assign.Description, tool.Description);
            Assert.AreEqual(assign.RequiresSource, tool.RequiresSource);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_DataType_UsesFirstAlias_ForMultiAliasEntry()
        {
            var switchEntry = ToolCatalog.Entries.Single(e => e.Name == "Switch");
            Assert.IsTrue(switchEntry.DataTypes.Count > 1, "Test assumes Switch has multiple accepted aliases.");

            var result = ListToolsTool.Handle();
            var tool = result.Tools.Single(t => t.Name == "Switch");

            Assert.AreEqual(switchEntry.DataTypes[0], tool.DataType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_RequiresSource_ReflectsCatalogValue_ForSourceBoundActivity()
        {
            var result = ListToolsTool.Handle();
            var sqlTool = result.Tools.Single(t => t.Name == "SQL Server Database");

            Assert.IsTrue(sqlTool.RequiresSource);
        }
    }
}
