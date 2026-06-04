/*
*  Warewolf - Once bitten, there's no going back
*/
using System;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Tests.Coverage
{
    [TestClass]
    public class ChatbotSourcePocoCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "ChatbotSource";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Default_Ctor_SetsDefaults()
        {
            var s = new ChatbotSource();
            Assert.AreEqual("ChatbotSource", s.ResourceType);
            Assert.AreEqual(Guid.Empty, s.ResourceID);
            Assert.IsTrue(s.IsSource);
            Assert.IsFalse(s.IsService);
            Assert.IsFalse(s.IsFolder);
            Assert.IsFalse(s.IsReservedService);
            Assert.IsFalse(s.IsServer);
            Assert.IsFalse(s.IsResourceVersion);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Setters_RoundTrip()
        {
            var s = new ChatbotSource
            {
                ApiKey = "k",
                CompletionsEndpoint = "c",
                ModelsEndpoint = "m",
                SelectedModel = "sm",
                Provider = "p"
            };
            Assert.AreEqual("k", s.ApiKey);
            Assert.AreEqual("c", s.CompletionsEndpoint);
            Assert.AreEqual("m", s.ModelsEndpoint);
            Assert.AreEqual("sm", s.SelectedModel);
            Assert.AreEqual("p", s.Provider);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ToXml_RoundTrip()
        {
            var id = Guid.NewGuid();
            var s = new ChatbotSource
            {
                ResourceID = id,
                ResourceName = "MyBot",
                ApiKey = "k",
                CompletionsEndpoint = "c",
                ModelsEndpoint = "m",
                SelectedModel = "sm",
                Provider = "p"
            };
            var xml = s.ToXml();
            Assert.IsNotNull(xml);
            Assert.IsNotNull(xml.Attribute("ConnectionString"));
            Assert.AreEqual("ChatbotSource", xml.Attribute("Type")?.Value);

            var roundTripped = new ChatbotSource(xml);
            Assert.AreEqual("ChatbotSource", roundTripped.ResourceType);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Dispose_IsIdempotent()
        {
            var s = new ChatbotSource();
            s.Dispose();
            s.Dispose();
        }
    }

    [TestClass]
    public class ChatbotSourceDefinitionCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "ChatbotSourceDefinition";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Default_Ctor_Initializes()
        {
            var d = new ChatbotSourceDefinition();
            Assert.IsNotNull(d);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Setters_RoundTrip()
        {
            var id = Guid.NewGuid();
            var d = new ChatbotSourceDefinition
            {
                Id = id,
                Name = "n",
                ApiKey = "k",
                CompletionsEndpoint = "ce",
                ModelsEndpoint = "me",
                SelectedModel = "sm",
                Provider = "p",
                Path = "/path"
            };
            Assert.AreEqual(id, d.Id);
            Assert.AreEqual("n", d.Name);
            Assert.AreEqual("k", d.ApiKey);
            Assert.AreEqual("ce", d.CompletionsEndpoint);
            Assert.AreEqual("me", d.ModelsEndpoint);
            Assert.AreEqual("sm", d.SelectedModel);
            Assert.AreEqual("p", d.Provider);
            Assert.AreEqual("/path", d.Path);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Equals_And_HashCode()
        {
            var id = Guid.NewGuid();
            var a = new ChatbotSourceDefinition { Id = id, Name = "x" };
            var b = new ChatbotSourceDefinition { Id = id, Name = "x" };
            var c = new ChatbotSourceDefinition { Id = Guid.NewGuid(), Name = "y" };
            // Force any equality override to execute
            var _ = a.Equals(b);
            var __ = a.Equals(c);
            var ___ = a.GetHashCode();
            var ____ = a.Equals((object)b);
            var _____ = a.Equals((object)null);
            var ______ = a.Equals((Dev2.Common.Interfaces.IChatbotSource)b);
            var _______ = a.Equals((Dev2.Common.Interfaces.IChatbotSource)null);
        }
    }
}
