
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Tests.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkspaceItemTests
    {
        const int VersionNo = 9999;

        const string ServiceName = "Calculate_RecordSet_Subtract";

        const string ServiceNameUnsigned = "TestDecisionUnsigned";

        const string SourceName = "CitiesDatabase";

        Guid SourceID = Guid.NewGuid();

        Guid ServiceID = Guid.NewGuid();

        Guid UnsignedServiceID = Guid.NewGuid();

        public const string ServerConnection1Name = "ServerConnection1";

        public const string ServerConnection1ID = "68F5B4FE-4573-442A-BA0C-5303F828344F";

        public const string ServerConnection2Name = "ServerConnection2";

        public const string ServerConnection2ID = "70238921-FDC7-4F7A-9651-3104EEDA1211";

        Guid _workspaceID;

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            _workspaceID = Guid.NewGuid();

            List<IResource> resources;
            ResourceCatalogTests.SaveResources(_workspaceID, VersionNo.ToString(), true, false,
                new[] { SourceName, ServerConnection1Name, ServerConnection2Name },
                new[] { ServiceName, ServiceNameUnsigned },
                out resources,
                new[] { SourceID, Guid.Parse(ServerConnection1ID), Guid.Parse(ServerConnection2ID) },
                new[] { ServiceID, UnsignedServiceID });

            ResourceCatalog.Instance.LoadWorkspace(_workspaceID);
        }

        #endregion

        [TestMethod]
        [Description("Test Constructor")]
        [Owner("Huggs")]
        public void WorkspaceItem_UnitTest_ConstructorWhereParametersPassed_ObjectHasProperties()
        {
            //------------Setup for test--------------------------
            Guid workspaceID = Guid.NewGuid();
            Guid resourceID = Guid.NewGuid();
            Guid environmentID = Guid.NewGuid();
            Guid serverID = Guid.NewGuid();
            //------------Execute Test---------------------------
            WorkspaceItem workspaceItem = new WorkspaceItem(workspaceID, serverID, environmentID, resourceID);
            //------------Assert Results-------------------------
            Assert.AreEqual(workspaceID, workspaceItem.WorkspaceID);
            Assert.AreEqual(serverID, workspaceItem.ServerID);
            Assert.AreEqual(environmentID, workspaceItem.EnvironmentID);
        }

        [TestMethod]
        [Description("Test Constructor")]
        [Owner("Huggs")]
        public void WorkspaceItem_UnitTest_ConstructorWhereParametersXML_ObjectHasProperties()
        {
            //------------Setup for test--------------------------
            Guid workspaceID = Guid.NewGuid();
            Guid resourceID = Guid.NewGuid();
            Guid environmentID = Guid.NewGuid();
            Guid serverID = Guid.NewGuid();
            WorkspaceItem workspaceItem = new WorkspaceItem(workspaceID, serverID, environmentID, resourceID);
            workspaceItem.IsWorkflowSaved = true;
            XElement xElement = workspaceItem.ToXml();
            //------------Execute Test---------------------------
            WorkspaceItem newWorkspaceItem = new WorkspaceItem(xElement);
            //------------Assert Results-------------------------
            Assert.AreEqual(workspaceID, newWorkspaceItem.WorkspaceID);
            Assert.AreEqual(serverID, newWorkspaceItem.ServerID);
            Assert.AreEqual(environmentID, newWorkspaceItem.EnvironmentID);
            Assert.IsTrue(newWorkspaceItem.IsWorkflowSaved);
        }

        [TestMethod]
        [Description("Test ToXML")]
        [Owner("Huggs")]
        public void WorkspaceItem_UnitTest_ToXMLWhereParametersXML_HasElementWithAttributes()
        {
            //------------Setup for test--------------------------
            Guid workspaceID = Guid.NewGuid();
            Guid resourceID = Guid.NewGuid();
            Guid environmentID = Guid.NewGuid();
            Guid serverID = Guid.NewGuid();
            WorkspaceItem workspaceItem = new WorkspaceItem(workspaceID, serverID, environmentID, resourceID);
            workspaceItem.IsWorkflowSaved = true;
            //------------Execute Test---------------------------
            XElement xElement = workspaceItem.ToXml();
            //------------Assert Results-------------------------
            Assert.IsNotNull(xElement);
            Assert.AreEqual(xElement.Name, "WorkspaceItem");
            Assert.AreEqual(workspaceItem.ID.ToString(), xElement.Attribute("ID").Value);
            Assert.AreEqual(workspaceID.ToString(), xElement.Attribute("WorkspaceID").Value);
            Assert.AreEqual(serverID.ToString(), xElement.Attribute("ServerID").Value);
            Assert.AreEqual(environmentID.ToString(), xElement.Attribute("EnvironmentID").Value);
            Assert.AreEqual(workspaceItem.Action.ToString(), xElement.Attribute("Action").Value);
        }
    }
}
