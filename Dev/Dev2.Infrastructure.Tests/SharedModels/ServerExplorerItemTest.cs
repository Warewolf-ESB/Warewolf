
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
using Dev2.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests.SharedModels
{
    [TestClass]
    public class ServerExplorerItemTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerExplorerItem_Constructor")]
        public void ServerExplorerItem_Constructor_Construct_ExpectAllFieldsAreSetup()
        {
            //------------Setup for test--------------------------
            var guid = Guid.NewGuid();
            const string name = "a";
            const ResourceType explorerItemType = ResourceType.Folder;
            var children = new List<IExplorerItem>();
            const Permissions permissions = Permissions.DeployFrom;


            //------------Execute Test---------------------------
            var serverExplorerItem = new ServerExplorerItem(name, guid, explorerItemType, children, permissions, "/");
            //------------Assert Results-------------------------

            Assert.AreEqual(children, serverExplorerItem.Children);
            Assert.AreEqual(name, serverExplorerItem.DisplayName);
            Assert.AreEqual(serverExplorerItem.ResourceType, explorerItemType);
            Assert.AreEqual(permissions, serverExplorerItem.Permissions);
            Assert.AreEqual(guid, serverExplorerItem.ResourceId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerExplorerItem_Constructor")]
        public void ServerExplorerItem_GetHashCode_ExpectHashCodeSameAsID()
        {
            //------------Setup for test--------------------------
            var guid = Guid.NewGuid();
            const string name = "a";
            const ResourceType explorerItemType = ResourceType.PluginService;
            var children = new List<IExplorerItem>();
            const Permissions permissions = Permissions.DeployFrom;


            //------------Execute Test---------------------------
            var serverExplorerItem = new ServerExplorerItem(name, guid, explorerItemType, children, permissions, "");
            //------------Assert Results-------------------------

            Assert.AreEqual(guid.GetHashCode(), serverExplorerItem.GetHashCode());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerExplorerItem_Constructor")]
        public void ServerExplorerItem_Equals_ExpectEqualityOnGuidOnly()
        {
            //------------Setup for test--------------------------
            var guid = Guid.NewGuid();
            const string name = "a";
            const ResourceType explorerItemType = ResourceType.PluginService;
            var children = new List<IExplorerItem>();
            const Permissions permissions = Permissions.DeployFrom;


            //------------Execute Test---------------------------
            var serverExplorerItem = new ServerExplorerItem(name, guid, explorerItemType, children, permissions, "");
            var serverExplorerItem2 = new ServerExplorerItem(name, guid, explorerItemType, children, Permissions.Administrator, "");
            var serverExplorerItem3 = new ServerExplorerItem(name, Guid.NewGuid(), explorerItemType, children, permissions, "");
            //------------Assert Results-------------------------

            Assert.AreEqual(serverExplorerItem, serverExplorerItem2);
            Assert.AreNotEqual(serverExplorerItem, null);
            Assert.AreEqual(serverExplorerItem, serverExplorerItem);
            Assert.AreNotEqual(serverExplorerItem, guid);
            Assert.AreNotEqual(serverExplorerItem, serverExplorerItem3);

        }
    }
}
