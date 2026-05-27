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
    /// Branch coverage for <see cref="DbService"/> ctors and ToXml.
    /// Targets the early-return / fallback paths in DbService(XElement)
    /// that the original DbServiceTests fixture does not exercise.
    /// </summary>
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [ExcludeFromCodeCoverage]
    public class DbServiceCtorBranchTests
    {
        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("DbService_Constructor")]
        public void DbService_DefaultCtor_SetsExpectedDefaults()
        {
            var sut = new DbService();

            Assert.AreEqual("DbService", sut.ResourceType);
            Assert.IsNotNull(sut.Source, "Default Source should be a new DbSource.");
            Assert.IsInstanceOfType(sut.Source, typeof(DbSource));
            Assert.IsNotNull(sut.Recordset, "Default Recordset should be allocated.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("DbService_Constructor")]
        public void DbService_XmlCtor_NoActionAndNoAttributes_ReturnsEarlyWithoutAssigningSource()
        {
            // <Service> with no Actions and no attributes -> ctor falls into the
            // early-return branch (HasAttributes == false).
            var xml = XElement.Parse("<Service></Service>");

            var sut = new DbService(xml);

            Assert.AreEqual("DbService", sut.ResourceType,
                "ResourceType is set before the early-return.");
            Assert.IsNull(sut.Source,
                "Early-return branch leaves Source unassigned (base ctor did not set it).");
            Assert.IsNull(sut.Recordset,
                "Early-return branch leaves Recordset unassigned.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("DbService_Constructor")]
        public void DbService_XmlCtor_RootIsInvokeStoredProcAction_TreatsRootAsAction()
        {
            // No <Actions>/<Action> children, but root <Service Type="InvokeStoredProc" ...>
            // -> action = xml branch. Use the root's Name attribute as Recordset.Name
            // fallback when CreateOutputsRecordsetList returns empty.
            var xml = XElement.Parse(
                @"<Service Type=""InvokeStoredProc"" Name=""dbo.MyProc"" SourceID=""00000000-0000-0000-0000-000000000000"" SourceName=""src"" SourceMethod=""dbo.MyProc"">
                    <Inputs />
                    <Outputs />
                  </Service>");

            var sut = new DbService(xml);

            Assert.AreEqual("DbService", sut.ResourceType);
            Assert.IsNotNull(sut.Source, "Source should be created from the root acting as Action.");
            Assert.IsNotNull(sut.Method, "Method should be created from the root acting as Action.");
            Assert.IsNotNull(sut.Recordset);
            // CreateOutputsRecordsetList returns empty -> falls back to action.AttributeSafe("Name").
            Assert.AreEqual("dbo.MyProc", sut.Recordset.Name,
                "Recordset.Name should fall back to the Action's Name attribute.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("DbService_Create")]
        public void DbService_Create_ProducesEmptyIdsAndDbSource()
        {
            var sut = DbService.Create();

            Assert.AreEqual(Guid.Empty, sut.ResourceID);
            Assert.IsNotNull(sut.Source);
            Assert.AreEqual(Guid.Empty, sut.Source.ResourceID);
            Assert.AreEqual("DbService", sut.ResourceType);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("DbService_ToXml")]
        public void DbService_ToXml_RoundTrip_RestoresIdNameAndResourceType()
        {
            // Exercises ToXml followed by the (XElement) ctor "happy" branch
            // (Action present, recordsets present). Complements existing tests
            // that only verify a single direction.
            var original = DbService.Create();
            original.ResourceID = Guid.NewGuid();
            original.ResourceName = "MyDbService";
            original.Method = new ServiceMethod { Name = "GetUsers" };
            original.Recordset.Name = "Users";

            var xml = original.ToXml();
            var roundTripped = new DbService(xml);

            Assert.AreEqual(original.ResourceID, roundTripped.ResourceID);
            Assert.AreEqual("MyDbService", roundTripped.ResourceName);
            Assert.AreEqual("DbService", roundTripped.ResourceType);
            Assert.AreEqual("Users", roundTripped.Recordset.Name);
        }
    }
}
