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
    /// Baseline ctor coverage for <see cref="WcfService"/> and
    /// <see cref="ComPluginService"/>. Neither had a dedicated test fixture
    /// before, so every assertion here is incremental coverage.
    /// </summary>
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [ExcludeFromCodeCoverage]
    public class WcfAndComPluginServiceCtorTests
    {
        // ------------------------------------------------------------------
        // WcfService
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("WcfService_Constructor")]
        public void WcfService_DefaultCtor_SetsExpectedDefaults()
        {
            var sut = new WcfService();

            Assert.AreEqual(Guid.Empty, sut.ResourceID);
            // Pre-existing SUT behaviour: WcfService default ctor sets
            // ResourceType to "PluginService" (same as XElement ctor).
            Assert.AreEqual("PluginService", sut.ResourceType);
            Assert.IsInstanceOfType(sut.Source, typeof(WcfSource));
            Assert.IsNotNull(sut.Recordsets);
            Assert.IsNotNull(sut.Method);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("WcfService_Constructor")]
        public void WcfService_XmlCtor_NoActionAndNoAttributes_ReturnsEarly()
        {
            var xml = XElement.Parse("<Service></Service>");

            var sut = new WcfService(xml);

            Assert.AreEqual("PluginService", sut.ResourceType,
                "ResourceType is set before the early-return.");
            Assert.IsNull(sut.Source, "Early-return path leaves Source unassigned.");
            Assert.IsNull(sut.Recordsets, "Early-return path leaves Recordsets unassigned.");
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("WcfService_Constructor")]
        public void WcfService_XmlCtor_RootIsWcfServiceAction_TreatsRootAsAction()
        {
            var sourceId = Guid.NewGuid();
            var xml = XElement.Parse(
                $@"<Service Type=""WcfService"" SourceID=""{sourceId}"" SourceName=""mySrc"" SourceMethod=""DoIt"">
                     <Inputs />
                     <Outputs />
                   </Service>");

            var sut = new WcfService(xml);

            Assert.AreEqual("PluginService", sut.ResourceType);
            Assert.IsNotNull(sut.Source);
            Assert.AreEqual(sourceId, sut.Source.ResourceID);
            Assert.AreEqual("mySrc", sut.Source.ResourceName);
            Assert.IsNotNull(sut.Method);
            Assert.AreEqual("DoIt", sut.Method.Name);
            Assert.IsNotNull(sut.Recordsets);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("WcfService_Constructor")]
        public void WcfService_XmlCtor_WithActionsChild_ConsumesActionElement()
        {
            var sourceId = Guid.NewGuid();
            var xml = XElement.Parse(
                $@"<Service ID=""{Guid.NewGuid()}"" Name=""Svc"" ResourceType=""WcfService"">
                     <Actions>
                       <Action Type=""WcfService"" SourceID=""{sourceId}"" SourceName=""s"" SourceMethod=""m"">
                         <Inputs />
                         <Outputs />
                       </Action>
                     </Actions>
                   </Service>");

            var sut = new WcfService(xml);

            Assert.AreEqual("PluginService", sut.ResourceType);
            Assert.IsNotNull(sut.Source);
            Assert.AreEqual(sourceId, sut.Source.ResourceID);
            Assert.AreEqual("m", sut.Method.Name);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("WcfService_ToXml")]
        public void WcfService_ToXml_EmitsPluginActionType()
        {
            var sut = new WcfService();
            sut.ResourceName = "MyWcf";
            sut.ResourceID = Guid.NewGuid();

            var xml = sut.ToXml();

            Assert.IsNotNull(xml);
            // Inspect the Action element's Type attribute; WcfService.ToXml
            // delegates to CreateXml(enActionType.Plugin, ...).
            var action = xml.Descendants("Action").FirstOrDefault();
            Assert.IsNotNull(action, "ToXml should emit an Action element.");
            Assert.AreEqual("Plugin", action.Attribute("Type")?.Value);
        }

        // ------------------------------------------------------------------
        // ComPluginService
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("ComPluginService_Constructor")]
        public void ComPluginService_DefaultCtor_SetsExpectedDefaults()
        {
            var sut = new ComPluginService();

            Assert.AreEqual(Guid.Empty, sut.ResourceID);
            Assert.AreEqual("ComPluginService", sut.ResourceType);
            Assert.IsInstanceOfType(sut.Source, typeof(ComPluginSource));
            Assert.IsNotNull(sut.Recordsets);
            Assert.IsNotNull(sut.Method);
            Assert.IsNull(sut.Namespace);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("ComPluginService_Constructor")]
        public void ComPluginService_XmlCtor_WithAction_PopulatesNamespaceSourceMethodAndRecordsets()
        {
            var sourceId = Guid.NewGuid();
            var xml = XElement.Parse(
                $@"<Service ID=""{Guid.NewGuid()}"" Name=""Svc"" ResourceType=""ComPluginService"">
                     <Actions>
                       <Action Type=""Plugin""
                               Namespace=""My.Com.Namespace""
                               SourceID=""{sourceId}""
                               SourceName=""comSrc""
                               SourceMethod=""DoThing"">
                         <Inputs />
                         <Outputs />
                       </Action>
                     </Actions>
                   </Service>");

            var sut = new ComPluginService(xml);

            Assert.AreEqual("ComPluginService", sut.ResourceType);
            Assert.AreEqual("My.Com.Namespace", sut.Namespace);
            Assert.IsNotNull(sut.Source);
            Assert.AreEqual(sourceId, sut.Source.ResourceID);
            Assert.AreEqual("comSrc", sut.Source.ResourceName);
            Assert.IsNotNull(sut.Method);
            Assert.AreEqual("DoThing", sut.Method.Name);
            Assert.IsNotNull(sut.Recordsets);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory("ComPluginService_Constructor")]
        public void ComPluginService_XmlCtor_MissingNamespaceAttribute_LeavesNamespaceEmpty()
        {
            // Namespace attribute absent -> AttributeSafe returns empty string.
            var xml = XElement.Parse(
                $@"<Service ID=""{Guid.NewGuid()}"" Name=""Svc"" ResourceType=""ComPluginService"">
                     <Actions>
                       <Action Type=""Plugin"" SourceID=""{Guid.NewGuid()}"" SourceName=""s"" SourceMethod=""m"">
                         <Inputs />
                         <Outputs />
                       </Action>
                     </Actions>
                   </Service>");

            var sut = new ComPluginService(xml);

            Assert.AreEqual(string.Empty, sut.Namespace ?? string.Empty);
            Assert.AreEqual("m", sut.Method.Name);
        }
    }
}
