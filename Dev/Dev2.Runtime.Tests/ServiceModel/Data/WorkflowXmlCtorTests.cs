/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Xml.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Coverage for the <see cref="Workflow"/> XElement constructors
    /// (Crap Score 342, Cyclomatic Complexity 18 on the coverage dashboard).
    ///
    /// These ctors drive both the <see cref="Workflow"/>-specific XML
    /// branches (DataList, Comment, IconPath, Tags, HelpLink, DisplayName,
    /// Action/XamlDefinition) and — through base — the <see cref="Resource"/>
    /// / ResourceBase XML ctor (ID parsing, IsUpgraded, ResourceType
    /// fallback, IsValid, VersionInfo).  The <c>loadExtra</c> overload also
    /// reads Version, ServerID, Category and UnitTestTargetWorkflowService.
    /// </summary>
    [TestClass]
    public class WorkflowXmlCtorTests
    {
        private const string ResId = "00000000-aaaa-bbbb-cccc-000000000001";

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static XElement MinimalServiceXml() =>
            new XElement("Service",
                new XAttribute("ID", ResId),
                new XAttribute("Name", "wf-min"),
                new XAttribute("ResourceType", "WorkflowService"),
                new XElement("DisplayName", "wf-min"),
                new XElement("DataList", new XElement("Inner", "x")));

        private static XElement RichServiceXml(string isValid = "true", string version = "1.0",
            string serverId = "33333333-4444-5555-6666-777777777777",
            string category = "Examples",
            string unitTestTarget = "wf-target") =>
            new XElement("Service",
                new XAttribute("ID", ResId),
                new XAttribute("Name", "wf-rich"),
                new XAttribute("ResourceType", "WorkflowService"),
                new XAttribute("IsValid", isValid),
                new XAttribute("Version", version),
                new XAttribute("ServerID", serverId),
                new XElement("DisplayName", "wf-rich"),
                new XElement("Comment", "a comment"),
                new XElement("IconPath", "icons/wf.png"),
                new XElement("Tags", "tag1,tag2"),
                new XElement("HelpLink", "https://help.example.com"),
                new XElement("Category", category),
                new XElement("UnitTestTargetWorkflowService", unitTestTarget),
                new XElement("DataList", new XElement("Var", "v1")),
                new XElement("Action",
                    new XAttribute("Name", "InvokeWorkflow"),
                    new XAttribute("Type", "Workflow"),
                    new XElement("XamlDefinition", "<Activity/>")));

        // ------------------------------------------------------------------
        // Workflow(XElement) -- minimal element
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_Minimal_SetsCoreFieldsAndDefaultsXamlToEmpty()
        {
            var sut = new Workflow(MinimalServiceXml());

            Assert.AreEqual(new Guid(ResId), sut.ResourceID);
            Assert.AreEqual("WorkflowService", sut.ResourceType);
            // The Workflow xml ctor always uses the DataList element verbatim
            // when present (never the default).
            Assert.IsNotNull(sut.DataList);
            Assert.IsTrue(sut.DataList.ToString().Contains("Inner"));
            // No Action element -> XamlDefinition is initialised to an empty StringBuilder.
            Assert.IsNotNull(sut.XamlDefinition);
            Assert.AreEqual(0, sut.XamlDefinition.Length);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_MissingDataListElement_FallsBackToDefault()
        {
            var xml = new XElement("Service",
                new XAttribute("ID", ResId),
                new XAttribute("Name", "wf"),
                new XAttribute("ResourceType", "WorkflowService"));

            var sut = new Workflow(xml);

            // No <DataList> in xml -> Workflow ctor inserts a fresh, empty one.
            Assert.IsNotNull(sut.DataList);
            Assert.AreEqual("DataList", sut.DataList.Name.LocalName);
            Assert.IsFalse(sut.DataList.HasElements);
        }

        // ------------------------------------------------------------------
        // Workflow(XElement) -- rich element exercises every assignment
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_Rich_CopiesAllOptionalScalarElements()
        {
            var sut = new Workflow(RichServiceXml());

            Assert.AreEqual("a comment", sut.Comment);
            Assert.AreEqual("icons/wf.png", sut.IconPath);
            Assert.AreEqual("tag1,tag2", sut.Tags);
            Assert.AreEqual("https://help.example.com", sut.HelpLink);
            Assert.AreEqual("wf-rich", sut.Name);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_WithActionElement_CapturesXamlDefinition()
        {
            var sut = new Workflow(RichServiceXml());

            Assert.IsNotNull(sut.XamlDefinition);
            // ElementSafeStringBuilder serialises the entire <XamlDefinition>
            // wrapper element verbatim (with inner XML escaped).
            var xaml = sut.XamlDefinition.ToString();
            StringAssert.Contains(xaml, "XamlDefinition");
            StringAssert.Contains(xaml, "Activity");
            Assert.IsTrue(xaml.Length > 0);
        }

        // ------------------------------------------------------------------
        // ResourceBase(xml) branches that flow up through Workflow
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_NoIdAttribute_AssignsNewGuidAndMarksUpgraded()
        {
            var xml = new XElement("Service",
                new XAttribute("Name", "no-id"),
                new XAttribute("ResourceType", "WorkflowService"));

            var sut = new Workflow(xml);

            Assert.AreNotEqual(Guid.Empty, sut.ResourceID);
            Assert.IsTrue(sut.IsUpgraded);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_MissingResourceTypeAttribute_DefaultsBeforeWorkflowOverride()
        {
            // ResourceBase defaults missing ResourceType to "WorkflowService".
            // Workflow's ctor then re-asserts it. Either way, result is the same.
            var xml = new XElement("Service",
                new XAttribute("ID", ResId),
                new XAttribute("Name", "no-type"));

            var sut = new Workflow(xml);

            Assert.AreEqual("WorkflowService", sut.ResourceType);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_IsValidTrue_FlagsResourceAsValid()
        {
            var sut = new Workflow(RichServiceXml(isValid: "true"));

            Assert.IsTrue(sut.IsValid);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_IsValidGarbage_LeavesValidUnset()
        {
            // bool.TryParse fails -> IsValid retains its default (false).
            var sut = new Workflow(RichServiceXml(isValid: "definitely-not-a-bool"));

            Assert.IsFalse(sut.IsValid);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void Ctor_Xml_NullXml_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Workflow((XElement)null));
        }

        // ------------------------------------------------------------------
        // Workflow(XElement, bool loadExtra) -- second overload's branches
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void CtorLoadExtra_PopulatesVersionServerIdCategoryAndUnitTestTarget()
        {
            var sut = new Workflow(RichServiceXml(version: "1.0"), loadExtra: true);

            Assert.AreEqual(new Version(1, 0), sut.Version);
            Assert.AreEqual(new Guid("33333333-4444-5555-6666-777777777777"), sut.ServerID);
            Assert.AreEqual("Examples", sut.Category);
            Assert.AreEqual("wf-target", sut.UnitTestTargetWorkflowService);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void CtorLoadExtra_InvalidVersionAttribute_LeavesVersionNull()
        {
            // Version.TryParse fails -> Version property is never assigned.
            var sut = new Workflow(RichServiceXml(version: "not-a-version"), loadExtra: true);

            Assert.IsNull(sut.Version);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void CtorLoadExtra_InvalidServerIdAttribute_LeavesServerIdEmpty()
        {
            var sut = new Workflow(RichServiceXml(serverId: "not-a-guid"), loadExtra: true);

            Assert.AreEqual(Guid.Empty, sut.ServerID);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Workflow))]
        public void CtorLoadExtra_MissingOptionalElements_LeavesPropertiesNull()
        {
            var xml = new XElement("Service",
                new XAttribute("ID", ResId),
                new XAttribute("Name", "wf-bare"),
                new XAttribute("ResourceType", "WorkflowService"));

            var sut = new Workflow(xml, loadExtra: true);

            Assert.IsNull(sut.Category);
            Assert.IsNull(sut.UnitTestTargetWorkflowService);
            Assert.IsNull(sut.Version);
        }
    }
}
