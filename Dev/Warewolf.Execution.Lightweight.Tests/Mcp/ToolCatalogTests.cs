/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ToolCatalog: the static "Toolbox subset (v3)" table and its Resolve()
 *  case-insensitive data-type matcher.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Tests.Mcp
{
    [TestClass]
    public class ToolCatalogTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Entries_ContainsExactly66Rows()
        {
            // Spec's ~65-row Toolbox subset (v3) table plus the "Calculate" row.
            Assert.AreEqual(66, ToolCatalog.Entries.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Entries_NamesAreUnique()
        {
            var duplicates = ToolCatalog.Entries
                .GroupBy(e => e.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            CollectionAssert.AreEqual(System.Array.Empty<string>(), duplicates,
                "Every toolbox entry's Studio name must be unique: " + string.Join(", ", duplicates));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Entries_EveryDataTypeAliasIsUnique()
        {
            var allDataTypes = ToolCatalog.Entries.SelectMany(e => e.DataTypes).ToList();
            var duplicates = allDataTypes
                .GroupBy(dt => dt, System.StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            CollectionAssert.AreEqual(System.Array.Empty<string>(), duplicates,
                "Every data.type alias must map to exactly one toolbox entry: " + string.Join(", ", duplicates));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Entries_NoBlankFields()
        {
            foreach (var entry in ToolCatalog.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Name), "Name blank");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.ActivityType), $"ActivityType blank for '{entry.Name}'");
                Assert.IsTrue(entry.DataTypes.Count > 0, $"DataTypes empty for '{entry.Name}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Category), $"Category blank for '{entry.Name}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Description), $"Description blank for '{entry.Name}'");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_PrimaryDataType_ReturnsMatchingEntry()
        {
            var entry = ToolCatalog.Resolve("dsfdotnetmultiassignactivity");

            Assert.IsNotNull(entry);
            Assert.AreEqual("Assign", entry!.Name);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_IsCaseInsensitive()
        {
            var entry = ToolCatalog.Resolve("DSFDOTNETMULTIASSIGNACTIVITY");

            Assert.IsNotNull(entry);
            Assert.AreEqual("Assign", entry!.Name);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_SecondaryAlias_ReturnsSameEntryAsPrimary()
        {
            // Switch accepts both "dsfflowswitchactivity" and "flowswitch".
            var viaPrimary = ToolCatalog.Resolve("dsfflowswitchactivity");
            var viaAlias = ToolCatalog.Resolve("flowswitch");

            Assert.IsNotNull(viaPrimary);
            Assert.IsNotNull(viaAlias);
            Assert.AreEqual(viaPrimary!.Name, viaAlias!.Name);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_UnknownDataType_ReturnsNull()
        {
            var entry = ToolCatalog.Resolve("not-a-real-activity-type");

            Assert.IsNull(entry);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_BlankDataType_ReturnsNull()
        {
            Assert.IsNull(ToolCatalog.Resolve(""));
            Assert.IsNull(ToolCatalog.Resolve("   "));
            Assert.IsNull(ToolCatalog.Resolve(null!));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Entries_ContainsCalculateRow()
        {
            var calculate = ToolCatalog.Entries.SingleOrDefault(e => e.Name == "Calculate");

            Assert.IsNotNull(calculate, "The 'Calculate' row must remain in the catalog.");
            Assert.AreEqual("DsfDotNetCalculateActivity", calculate!.ActivityType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_LegacyCalculateAlias_ReturnsSameEntryAsDotNetPrimary()
        {
            // Regression guard: Calculate used to be unreachable via X6JsonToWorkflow for either
            // variant (see X6-Converter-Missing-Activity-Support-Spec.md §3.1). Both the DotNet
            // primary and the legacy alias must now resolve to the same catalog entry.
            var viaPrimary = ToolCatalog.Resolve("dsfdotnetcalculateactivity");
            var viaLegacyAlias = ToolCatalog.Resolve("dsfcalculateactivity");

            Assert.IsNotNull(viaPrimary);
            Assert.IsNotNull(viaLegacyAlias);
            Assert.AreEqual(viaPrimary!.Name, viaLegacyAlias!.Name);
        }
    }
}
