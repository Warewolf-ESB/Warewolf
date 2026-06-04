/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*/
using System;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Tests.Workspaces
{
    [TestClass]
    public class WorkspaceItemCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = nameof(WorkspaceItem);

        static readonly Guid WkId = Guid.NewGuid();
        static readonly Guid SrvId = Guid.NewGuid();
        static readonly Guid EnvId = Guid.NewGuid();
        static readonly Guid ResId = Guid.NewGuid();

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void StaticServiceTypeConstants_HaveExpectedValues()
        {
            Assert.AreEqual("DynamicService", WorkspaceItem.ServiceServiceType);
            Assert.AreEqual("Source", WorkspaceItem.SourceServiceType);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Ctor_GuidArgs_AssignsAllIds()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            Assert.AreEqual(WkId, w.WorkspaceID);
            Assert.AreEqual(SrvId, w.ServerID);
            Assert.AreEqual(EnvId, w.EnvironmentID);
            Assert.AreEqual(ResId, w.ID);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Properties_GettersAndSetters_RoundTrip()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId)
            {
                Action = WorkspaceItemAction.Edit,
                ServiceName = "MyService",
                ServiceType = "DynamicService",
                IsWorkflowSaved = false
            };
            Assert.AreEqual(WorkspaceItemAction.Edit, w.Action);
            Assert.AreEqual("MyService", w.ServiceName);
            Assert.AreEqual("DynamicService", w.ServiceType);
            Assert.IsFalse(w.IsWorkflowSaved);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ToXml_RoundTrip_RestoresAllProperties()
        {
            var original = new WorkspaceItem(WkId, SrvId, EnvId, ResId)
            {
                Action = WorkspaceItemAction.Edit,
                ServiceName = "Svc",
                ServiceType = "DynamicService",
                IsWorkflowSaved = true
            };
            var xml = original.ToXml();
            Assert.AreEqual("WorkspaceItem", xml.Name.LocalName);

            var roundtrip = new WorkspaceItem(xml);
            Assert.AreEqual(original.ID, roundtrip.ID);
            Assert.AreEqual(original.WorkspaceID, roundtrip.WorkspaceID);
            Assert.AreEqual(original.ServerID, roundtrip.ServerID);
            Assert.AreEqual(original.EnvironmentID, roundtrip.EnvironmentID);
            Assert.AreEqual(original.ServiceName, roundtrip.ServiceName);
            Assert.AreEqual(original.ServiceType, roundtrip.ServiceType);
            Assert.AreEqual(original.IsWorkflowSaved, roundtrip.IsWorkflowSaved);
            Assert.AreEqual(original.Action, roundtrip.Action);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ToXml_NullServiceName_WritesEmptyString()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            var xml = w.ToXml();
            Assert.AreEqual(string.Empty, (string)xml.Attribute("ServiceName"));
            Assert.AreEqual(string.Empty, (string)xml.Attribute("ServiceType"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void XmlCtor_MissingIsWorkflowSaved_DefaultsToTrue()
        {
            var xml = new XElement("WorkspaceItem",
                new XAttribute("ID", ResId),
                new XAttribute("WorkspaceID", WkId),
                new XAttribute("ServerID", SrvId),
                new XAttribute("EnvironmentID", EnvId),
                new XAttribute("Action", "Edit"),
                new XAttribute("ServiceName", "Svc"),
                new XAttribute("IsWorkflowSaved", ""),
                new XAttribute("ServiceType", "DynamicService"));
            var w = new WorkspaceItem(xml);
            Assert.IsTrue(w.IsWorkflowSaved);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void XmlCtor_InvalidGuids_FallBackToEmpty()
        {
            var xml = new XElement("WorkspaceItem",
                new XAttribute("ID", ResId),
                new XAttribute("WorkspaceID", "not-a-guid"),
                new XAttribute("ServerID", "still-not-a-guid"),
                new XAttribute("EnvironmentID", "nope"),
                new XAttribute("Action", "None"),
                new XAttribute("ServiceName", "X"),
                new XAttribute("IsWorkflowSaved", "true"),
                new XAttribute("ServiceType", "Source"));
            var w = new WorkspaceItem(xml);
            Assert.AreEqual(Guid.Empty, w.WorkspaceID);
            Assert.AreEqual(Guid.Empty, w.ServerID);
            Assert.AreEqual(Guid.Empty, w.EnvironmentID);
            Assert.AreEqual(ResId, w.ID);
            Assert.IsTrue(w.IsWorkflowSaved);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void XmlCtor_InvalidAction_DefaultsToNone()
        {
            var xml = new XElement("WorkspaceItem",
                new XAttribute("ID", ResId),
                new XAttribute("WorkspaceID", WkId),
                new XAttribute("ServerID", SrvId),
                new XAttribute("EnvironmentID", EnvId),
                new XAttribute("Action", "BogusAction"),
                new XAttribute("ServiceName", "Svc"),
                new XAttribute("IsWorkflowSaved", "false"),
                new XAttribute("ServiceType", "DynamicService"));
            var w = new WorkspaceItem(xml);
            Assert.AreEqual(WorkspaceItemAction.None, w.Action);
            Assert.IsFalse(w.IsWorkflowSaved);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void XmlCtor_MissingAttributes_ServiceNameIsEmpty()
        {
            var xml = new XElement("WorkspaceItem",
                new XAttribute("ID", ResId));
            var w = new WorkspaceItem(xml);
            Assert.AreEqual(string.Empty, w.ServiceName);
            Assert.AreEqual(string.Empty, w.ServiceType);
            Assert.AreEqual(Guid.Empty, w.WorkspaceID);
            Assert.AreEqual(WorkspaceItemAction.None, w.Action);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_OtherNull_ReturnsFalse()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            Assert.IsFalse(w.Equals((IWorkspaceItem)null));
            Assert.IsFalse(w.Equals((object)null));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_ObjectNotIWorkspaceItem_ReturnsFalse()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            Assert.IsFalse(w.Equals("nope"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_SameIdAndEnv_ReturnsTrue()
        {
            var a = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            var b = new WorkspaceItem(Guid.NewGuid(), Guid.NewGuid(), EnvId, ResId);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_DifferentId_ReturnsFalse()
        {
            var a = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            var b = new WorkspaceItem(WkId, SrvId, EnvId, Guid.NewGuid());
            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetHashCode_MatchesIdHash()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            Assert.AreEqual(ResId.GetHashCode(), w.GetHashCode());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetObjectData_NullInfo_Throws()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId);
            Assert.ThrowsException<ArgumentNullException>(() => w.GetObjectData(null, default));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GetObjectData_PopulatesAllFields()
        {
            var w = new WorkspaceItem(WkId, SrvId, EnvId, ResId)
            {
                Action = WorkspaceItemAction.Edit,
                ServiceName = "Svc",
                IsWorkflowSaved = false
            };
            var info = new SerializationInfo(typeof(WorkspaceItem), new FormatterConverter());
            w.GetObjectData(info, default);
            Assert.AreEqual(ResId, (Guid)info.GetValue("ID", typeof(Guid)));
            Assert.AreEqual(WkId, (Guid)info.GetValue("WorkspaceID", typeof(Guid)));
            Assert.AreEqual(SrvId, (Guid)info.GetValue("ServerID", typeof(Guid)));
            Assert.AreEqual(WorkspaceItemAction.Edit, (WorkspaceItemAction)info.GetValue("Action", typeof(WorkspaceItemAction)));
            Assert.AreEqual("Svc", (string)info.GetValue("ServiceName", typeof(string)));
            Assert.AreEqual(false, (bool)info.GetValue("IsWorkflowSaved", typeof(bool)));
        }
    }
}
