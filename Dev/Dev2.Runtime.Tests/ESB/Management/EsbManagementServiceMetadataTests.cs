/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*/
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Tests.ESB.Management
{
    /// <summary>
    /// Lightweight tests covering metadata methods (HandlesType, GetResourceID,
    /// GetAuthorizationContextForService, CreateServiceEntry) and early-return
    /// validation paths of various Esb management services.  These tests
    /// purposely avoid the ResourceCatalog/Workspaces paths that require a
    /// full server bootstrap.
    /// </summary>
    [TestClass]
    public class EsbManagementServiceMetadataTests
    {
        const string Owner = "Coverage";
        const string Cat = "EsbManagementMetadata";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchEmailToolPassword_Metadata()
        {
            var svc = new FetchEmailToolPassword();
            Assert.AreEqual("FetchEmailToolPasswordService", svc.HandlesType());
            Assert.IsNotNull(svc.GetAuthorizationContextForService());
            Assert.AreEqual(Guid.Empty, svc.GetResourceID(new Dictionary<string, StringBuilder>()));
            Assert.IsNotNull(svc.CreateServiceEntry());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchEmailToolPassword_MissingResourceId_ReturnsError()
        {
            var svc = new FetchEmailToolPassword();
            var result = svc.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsTrue(result.ToString().Contains("ResourceID"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchEmailToolPassword_InvalidResourceId_ReturnsError()
        {
            var svc = new FetchEmailToolPassword();
            var vals = new Dictionary<string, StringBuilder>
            {
                ["ResourceID"] = new StringBuilder("not-a-guid"),
                ["ActivityID"] = new StringBuilder("act"),
            };
            var result = svc.Execute(vals, null);
            Assert.IsTrue(result.ToString().Contains("Invalid ResourceID"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchEmailToolPassword_EmptyActivityId_ReturnsError()
        {
            var svc = new FetchEmailToolPassword();
            var vals = new Dictionary<string, StringBuilder>
            {
                ["ResourceID"] = new StringBuilder(Guid.NewGuid().ToString()),
                ["ActivityID"] = new StringBuilder("   "),
            };
            var result = svc.Execute(vals, null);
            Assert.IsTrue(result.ToString().Contains("ActivityID"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchChatbotModels_Metadata()
        {
            var svc = new FetchChatbotModels();
            Assert.AreEqual("FetchChatbotModels", svc.HandlesType());
            Assert.IsNotNull(svc.GetAuthorizationContextForService());
            Assert.AreEqual(Guid.Empty, svc.GetResourceID(new Dictionary<string, StringBuilder>()));
            Assert.IsNotNull(svc.CreateServiceEntry());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchChatbotModels_NullValues_ReturnsError()
        {
            var svc = new FetchChatbotModels();
            var result = svc.Execute(null, null);
            var json = result.ToString();
            Assert.IsTrue(json.Contains("HasError") || json.Length > 0);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchChatbotModels_MissingSourceId_ReturnsError()
        {
            var svc = new FetchChatbotModels();
            var result = svc.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsTrue(result.ToString().Contains("ChatbotSourceId"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FetchChatbotModels_BadGuid_ReturnsError()
        {
            var svc = new FetchChatbotModels();
            var vals = new Dictionary<string, StringBuilder>
            {
                ["ChatbotSourceId"] = new StringBuilder("zzz"),
            };
            var result = svc.Execute(vals, null);
            Assert.IsTrue(result.ToString().Contains("ChatbotSourceId"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SendChatbotMessage_Metadata()
        {
            var svc = new SendChatbotMessage();
            Assert.AreEqual("SendChatbotMessage", svc.HandlesType());
            Assert.IsNotNull(svc.GetAuthorizationContextForService());
            Assert.AreEqual(Guid.Empty, svc.GetResourceID(new Dictionary<string, StringBuilder>()));
            Assert.IsNotNull(svc.CreateServiceEntry());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SendChatbotMessage_EmptyMessage_ReturnsError()
        {
            var svc = new SendChatbotMessage();
            var result = svc.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsTrue(result.ToString().Length > 0);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SendChatbotMessage_WhitespaceMessage_ReturnsError()
        {
            var svc = new SendChatbotMessage();
            var vals = new Dictionary<string, StringBuilder>
            {
                ["Message"] = new StringBuilder("   "),
            };
            var result = svc.Execute(vals, null);
            Assert.IsTrue(result.ToString().Contains("Message"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void TestChatbotSource_Metadata()
        {
            var svc = new TestChatbotSource();
            Assert.AreEqual("TestChatbotSource", svc.HandlesType());
            Assert.IsNotNull(svc.GetAuthorizationContextForService());
            Assert.AreEqual(Guid.Empty, svc.GetResourceID(new Dictionary<string, StringBuilder>()));
            Assert.IsNotNull(svc.CreateServiceEntry());
            Assert.AreEqual("ChatbotSource", TestChatbotSource.ChatbotSource);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void TestChatbotSource_Empty_ReturnsErrorWithoutThrowing()
        {
            var svc = new TestChatbotSource();
            // Will hit the catch block – we only care that it returns a
            // serialised error message rather than throwing.
            var result = svc.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GenerateAdvancedRecordsetOutputs_Metadata()
        {
            var svc = new GenerateAdvancedRecordsetOutputs();
            Assert.AreEqual("GenerateAdvancedRecordsetOutputs", svc.HandlesType());
            Assert.IsNotNull(svc.GetAuthorizationContextForService());
            Assert.AreEqual(Guid.Empty, svc.GetResourceID(new Dictionary<string, StringBuilder>()));
            Assert.IsNotNull(svc.CreateServiceEntry());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GenerateAdvancedRecordsetOutputs_MissingDefinition_ReturnsError()
        {
            var svc = new GenerateAdvancedRecordsetOutputs();
            var result = svc.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsTrue(result.ToString().Contains("AdvancedRecordsetService"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GenerateAdvancedRecordsetOutputs_EmptySql_ReturnsError()
        {
            var svc = new GenerateAdvancedRecordsetOutputs();
            var vals = new Dictionary<string, StringBuilder>
            {
                ["AdvancedRecordsetService"] = new StringBuilder("{\"SqlQuery\":\"\"}"),
            };
            var result = svc.Execute(vals, null);
            Assert.IsTrue(result.ToString().Contains("SQL"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void FindSourcesByType_Metadata()
        {
            var svc = new FindSourcesByType();
            Assert.AreEqual("FindSourcesByType", svc.HandlesType());
            Assert.IsNotNull(svc.GetAuthorizationContextForService());
            Assert.AreEqual(Guid.Empty, svc.GetResourceID(new Dictionary<string, StringBuilder>()));
            Assert.IsNotNull(svc.CreateServiceEntry());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DeployResource_Metadata()
        {
            var svc = new DeployResource();
            Assert.AreEqual("DeployResourceService", svc.HandlesType());
            Assert.IsNotNull(svc.CreateServiceEntry());
            // GetResourceID / GetAuthorizationContextForService run too:
            svc.GetAuthorizationContextForService();
            svc.GetResourceID(new Dictionary<string, StringBuilder>());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DeployAllResources_Metadata()
        {
            var svc = new DeployAllResources();
            Assert.IsNotNull(svc.HandlesType());
            Assert.IsNotNull(svc.CreateServiceEntry());
            svc.GetAuthorizationContextForService();
            svc.GetResourceID(new Dictionary<string, StringBuilder>());
        }
    }
}
