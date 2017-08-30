/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchServerPermissionsTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAuthorizationContextForService_Returns_Contribute()
        {
            //------------Setup for test-------------------------
            FetchServerPermissions fetchServerPermissions = new FetchServerPermissions();
            //------------Execute Test---------------------------
            var authorizationContextForService = fetchServerPermissions.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, authorizationContextForService);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CreateServiceEntry_Returns_New_Dynamic_Service()
        {
            //------------Setup for test-------------------------
            FetchServerPermissions fetchServerPermissions = new FetchServerPermissions();
            //------------Execute Test---------------------------
            var dynamicService = fetchServerPermissions.CreateServiceEntry();
            var handleType = fetchServerPermissions.HandlesType();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsFalse(string.IsNullOrEmpty(handleType));
            Assert.AreEqual(handleType, dynamicService.Name);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetResourceID_GivenEmptyArgs_Returns_EmptyGuid()
        {
            //------------Setup for test-------------------------
            FetchServerPermissions fetchServerPermissions = new FetchServerPermissions();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var resourceID = fetchServerPermissions.GetResourceID(requestArgs);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resourceID);
            Assert.AreEqual(Guid.Empty, resourceID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetResourceID_GivenSomeArgs_Returns_Id()
        {
            //------------Setup for test-------------------------
            FetchServerPermissions fetchServerPermissions = new FetchServerPermissions();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var resourceID = fetchServerPermissions.GetResourceID(requestArgs);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resourceID);
            Assert.AreEqual(Guid.Empty, resourceID);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Execute_Returns_Permissions()
        {
            //------------Setup for test-------------------------
            FetchServerPermissions fetchServerPermissions = new FetchServerPermissions();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var executeResults = fetchServerPermissions.Execute(requestArgs, workspaceMock.Object);
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            Assert.IsNotNull(executeResults);
            var deserializedResults = jsonSerializer.Deserialize<PermissionsModifiedMemo>(executeResults);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deserializedResults);
            Assert.IsNotNull(deserializedResults.ModifiedPermissions);
        }
    }
}
