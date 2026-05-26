using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;

namespace Warewolf.Core.Tests
{
    [TestClass]
    public class DbActionTests
    {
        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_Properties_RoundTrip()
        {
            var id = Guid.NewGuid();
            var input = new Mock<IServiceInput>().Object;
            var action = new DbAction
            {
                Name = "GetItems",
                SourceId = id,
                ExecuteAction = "exec",
                Inputs = new List<IServiceInput> { input }
            };

            Assert.AreEqual("GetItems", action.Name);
            Assert.AreEqual(id, action.SourceId);
            Assert.AreEqual("exec", action.ExecuteAction);
            Assert.AreEqual(1, action.Inputs.Count);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_ToString_ReturnsName()
        {
            var a = new DbAction { Name = "act" };
            Assert.AreEqual("act", a.ToString());
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_GetIdentifier_ConcatenatesSourceIdAndName()
        {
            var id = Guid.NewGuid();
            var a = new DbAction { Name = "x", SourceId = id };
            Assert.AreEqual(id + "x", a.GetIdentifier());
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_Equals_Null_ReturnsFalse()
        {
            var a = new DbAction { Name = "n" };
            Assert.IsFalse(a.Equals((DbAction)null));
            Assert.IsFalse(a.Equals((object)null));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_Equals_Self_ReturnsTrue()
        {
            var a = new DbAction { Name = "n" };
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals((object)a));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_Equals_DifferentType_ReturnsFalse()
        {
            var a = new DbAction { Name = "n" };
            Assert.IsFalse(a.Equals((object)"n"));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_Equals_SameNameAndNullInputs_ReturnsTrue()
        {
            var a = new DbAction { Name = "n" };
            var b = new DbAction { Name = "n" };
            // Equal hashcodes short-circuit to true.
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_Equals_DifferentNames_ReturnsFalse()
        {
            var a = new DbAction { Name = "a" };
            var b = new DbAction { Name = "b" };
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_GetHashCode_NullValues_ReturnsZero()
        {
            var a = new DbAction();
            Assert.AreEqual(0, a.GetHashCode());
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(DbAction))]
        public void DbAction_Equals_BothNullInputs_FallsThroughInputEquality()
        {
            var a = new DbAction { Name = "x", Inputs = null };
            var b = new DbAction { Name = "x", Inputs = null };
            Assert.IsTrue(a.Equals(b));
        }
    }

    [TestClass]
    public class WarewolfWebClientTests
    {
        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfWebClient))]
        public void WarewolfWebClient_DownloadString_EmptyAddress_ReturnsNull()
        {
            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);
            Assert.IsNull(client.DownloadString(string.Empty));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfWebClient))]
        public void WarewolfWebClient_DownloadString_NullAddress_ReturnsNull()
        {
            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);
            Assert.IsNull(client.DownloadString(null));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfWebClient))]
        public async Task WarewolfWebClient_DownloadStringAsync_EmptyAddress_ReturnsNull()
        {
            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);
            Assert.IsNull(await client.DownloadStringAsync(string.Empty));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfWebClient))]
        public void WarewolfWebClient_IsBusy_DefaultFalse()
        {
            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);
            Assert.IsFalse(client.IsBusy);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfWebClient))]
        public void WarewolfWebClient_DownloadProgressChanged_AddRemove_ForwardsToInner()
        {
            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);
            System.Net.DownloadProgressChangedEventHandler handler = (_, __) => { };
            client.DownloadProgressChanged += handler;
            client.DownloadProgressChanged -= handler;
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfWebClient))]
        public void WarewolfWebClient_DownloadFileCompleted_AddRemove_ForwardsToInner()
        {
            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);
            System.ComponentModel.AsyncCompletedEventHandler handler = (_, __) => { };
            client.DownloadFileCompleted += handler;
            client.DownloadFileCompleted -= handler;
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfWebClient))]
        public void WarewolfWebClient_Dispose_DoesNotThrow()
        {
            var inner = new WebClient();
            var client = new WarewolfWebClient(inner);
            client.Dispose();
            // Calling Dispose a second time on the wrapper is safe.
            client.Dispose();
        }
    }

    [TestClass]
    public class WarewolfTypeTests
    {
        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfType))]
        public void WarewolfType_Constructor_SetsProperties()
        {
            var v = new Version(1, 2, 3, 4);
            var t = new WarewolfType("Foo.Bar", v, @"c:\foo.dll");
            Assert.AreEqual("Foo.Bar", t.FullyQualifiedName);
            Assert.AreEqual(v, t.Version);
            Assert.AreEqual(@"c:\foo.dll", t.ContainingAssemblyPath);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfType))]
        public void WarewolfType_ActualType_RoundTrips()
        {
            var t = new WarewolfType("F", new Version(1, 0), "p") { ActualType = typeof(int) };
            Assert.AreEqual(typeof(int), t.ActualType);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfType))]
        public void WarewolfType_Equals_Null_ReturnsFalse()
        {
            var t = new WarewolfType("F", new Version(1, 0), "p");
            Assert.IsFalse(t.Equals((object)null));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfType))]
        public void WarewolfType_Equals_Self_ReturnsTrue()
        {
            var t = new WarewolfType("F", new Version(1, 0), "p");
            Assert.IsTrue(t.Equals((object)t));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(WarewolfType))]
        public void WarewolfType_Equals_DifferentType_ReturnsFalse()
        {
            var t = new WarewolfType("F", new Version(1, 0), "p");
            Assert.IsFalse(t.Equals((object)"x"));
        }
    }

    [TestClass]
    public class ToolDescriptorInfoTests
    {
        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptorInfo))]
        public void ToolDescriptorInfo_Constructor_PopulatesAllProperties()
        {
            var id = Guid.NewGuid();
            var info = new ToolDescriptorInfo(
                "iconName",
                "MyTool",
                Dev2.Common.Interfaces.Toolbox.ToolType.Native,
                id.ToString(),
                "MyAsm",
                "1.2.3.4",
                "Foo.Bar",
                "Recordset",
                "iconUri",
                "Assign");

            Assert.AreEqual("iconName", info.Icon);
            Assert.AreEqual("MyTool", info.Name);
            Assert.AreEqual(Dev2.Common.Interfaces.Toolbox.ToolType.Native, info.ToolType);
            Assert.AreEqual(id, info.Id);
            Assert.AreEqual("Recordset", info.Category);
            Assert.AreEqual("iconUri", info.IconUri);
            Assert.IsNotNull(info.Designer);
            Assert.AreEqual("MyAsm", info.Designer.FullyQualifiedName);
            Assert.AreEqual(new Version(1, 2, 3, 4), info.Designer.Version);
            Assert.AreEqual("Foo.Bar", info.Designer.ContainingAssemblyPath);
            // Resource-driven strings: never null, default to empty for unknown tag.
            Assert.IsNotNull(info.FilterTag);
            Assert.IsNotNull(info.ResourceToolTip);
            Assert.IsNotNull(info.ResourceHelpText);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptorInfo))]
        public void ToolDescriptorInfo_Constructor_UnknownTag_DefaultsToEmpty()
        {
            var info = new ToolDescriptorInfo(
                "i",
                "n",
                Dev2.Common.Interfaces.Toolbox.ToolType.Native,
                Guid.NewGuid().ToString(),
                "Asm",
                "1.0",
                "P",
                "Cat",
                "U",
                "ThisTagWillNotExistAnywhere_8431");

            Assert.AreEqual(string.Empty, info.FilterTag);
            Assert.AreEqual(string.Empty, info.ResourceToolTip);
            Assert.AreEqual(string.Empty, info.ResourceHelpText);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptorInfo))]
        public void ToolDescriptorInfo_FilterTag_IsSettable()
        {
            var info = new ToolDescriptorInfo("i", "n", Dev2.Common.Interfaces.Toolbox.ToolType.Native,
                Guid.NewGuid().ToString(), "A", "1.0", "P", "C", "U", "no_such_tag");
            info.FilterTag = "ft";
            info.ResourceToolTip = "tt";
            info.ResourceHelpText = "ht";
            Assert.AreEqual("ft", info.FilterTag);
            Assert.AreEqual("tt", info.ResourceToolTip);
            Assert.AreEqual("ht", info.ResourceHelpText);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptorInfo))]
        public void ToolDescriptorInfo_IsAttribute()
        {
            Assert.IsTrue(typeof(Attribute).IsAssignableFrom(typeof(ToolDescriptorInfo)));
        }
    }
}
