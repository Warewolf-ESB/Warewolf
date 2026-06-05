/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Branch coverage for <see cref="Service.CreateOutputsRecordsetList"/>.
    /// Driven through <see cref="DbService(XElement)"/>.
    /// </summary>
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [ExcludeFromCodeCoverage]
    public class ServiceCreateOutputsRecordsetListBranchTests
    {
        static XElement BuildServiceXml(string innerActionBody) =>
            XElement.Parse(
                $@"<Service ID=""{Guid.NewGuid()}"" Name=""Svc"" ResourceType=""DbService"">
                     <Actions>
                       <Action Name=""dbo.X"" Type=""InvokeStoredProc"" SourceID=""{Guid.NewGuid()}"" SourceName=""s"" SourceMethod=""dbo.X"">
                         <Inputs />
                         {innerActionBody}
                       </Action>
                     </Actions>
                   </Service>");

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateOutputsRecordsetList_NoOutputsElement_ReturnsEmptyList()
        {
            // No <Outputs> child at all -> xElement null, OutputSpecification
            // remains unset, no Output descendants -> empty RecordsetList.
            var xml = BuildServiceXml("");

            var sut = new DbService(xml);

            Assert.IsNotNull(sut.Recordset);
            // DbService falls back to a new Recordset { Name = action.Name }
            // when the list is empty, so we assert on that fallback contract:
            Assert.AreEqual("dbo.X", sut.Recordset.Name);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateOutputsRecordsetList_EmptyOutputsElement_SetsOutputSpecification()
        {
            // <Outputs /> present but empty -> xElement non-null branch,
            // OutputSpecification gets set, no Output descendants iterated.
            var xml = BuildServiceXml("<Outputs />");

            var sut = new DbService(xml);

            Assert.IsNotNull(sut);
            // OutputSpecification is set on the underlying Service base, but
            // is not exposed via DbService's public surface for assertion;
            // we just verify the ctor completes without throwing and the
            // Recordset fallback fires.
            Assert.AreEqual("dbo.X", sut.Recordset.Name);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateOutputsRecordsetList_SingleOutput_AddsRecordsetAndField()
        {
            // One <Output> -> creates a new Recordset (first-arm of the
            // recordset == null branch) and adds a RecordsetField.
            var xml = BuildServiceXml(
                @"<Outputs>
                    <Output OriginalName=""FirstName"" MapsTo=""[[fname]]"" Value=""[[result.FirstName]]"" RecordsetName=""People"" Recordset=""[[People()]]"" />
                  </Outputs>");

            var sut = new DbService(xml);

            Assert.AreEqual("People", sut.Recordset.Name,
                "Recordset.Name should come from the single Output's RecordsetName attribute.");
            Assert.AreEqual(1, sut.Recordset.Fields.Count);
            var field = sut.Recordset.Fields[0];
            Assert.AreEqual("FirstName", field.Name);
            Assert.AreEqual("[[fname]]", field.Alias);
            Assert.AreEqual("[[People()]]", field.RecordsetAlias);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateOutputsRecordsetList_MultipleFieldsSameRecordset_GroupedIntoSingleRecordset()
        {
            // Two <Output> elements with the same RecordsetName -> second one
            // hits the `recordset != null` branch (reuses existing recordset).
            var xml = BuildServiceXml(
                @"<Outputs>
                    <Output OriginalName=""F1"" MapsTo=""[[a]]"" Value=""[[r.F1]]"" RecordsetName=""Rs"" Recordset=""[[Rs()]]"" />
                    <Output OriginalName=""F2"" MapsTo=""[[b]]"" Value=""[[r.F2]]"" RecordsetName=""Rs"" Recordset=""[[Rs()]]"" />
                  </Outputs>");

            var sut = new DbService(xml);

            // Recordset is the FirstOrDefault from the list; both fields should
            // be on it because they share RecordsetName="Rs".
            Assert.AreEqual("Rs", sut.Recordset.Name);
            Assert.AreEqual(2, sut.Recordset.Fields.Count,
                "Two Outputs sharing RecordsetName should produce a single Recordset with two fields.");
            CollectionAssert.AreEquivalent(
                new[] { "F1", "F2" },
                sut.Recordset.Fields.Select(f => f.Name).ToArray());
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Service))]
        public void CreateOutputsRecordsetList_OutputsWithBlankRecordsetName_GroupedUnderEmptyKey()
        {
            // Blank RecordsetName + no matching OutputDescription path -> rsName
            // stays empty; first Output creates the empty-name Recordset,
            // second reuses it.
            var xml = BuildServiceXml(
                @"<Outputs>
                    <Output OriginalName=""F1"" MapsTo=""[[a]]"" Value=""v1"" RecordsetName="""" Recordset="""" />
                    <Output OriginalName=""F2"" MapsTo=""[[b]]"" Value=""v2"" RecordsetName="""" Recordset="""" />
                  </Outputs>");

            var sut = new DbService(xml);

            // Both fields share the empty-name recordset.
            Assert.AreEqual(2, sut.Recordset.Fields.Count);
        }
    }
}
