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
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FindResourceTestsTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAuthorizationContextForService_Returns_Any()
        {
            //------------Setup for test-------------------------
            FindResource findResource = new FindResource();
            //------------Execute Test---------------------------
            var authorizationContextForService = findResource.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, authorizationContextForService);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CreateServiceEntry_Returns_New_Dynamic_Service_DeleteAllTestsService()
        {
            //------------Setup for test-------------------------
            FindResource findResource = new FindResource();
            //------------Execute Test---------------------------
            var dynamicService = findResource.CreateServiceEntry();
            var handleType = findResource.HandlesType();
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
            FindResource findResource = new FindResource();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var resourceID = findResource.GetResourceID(requestArgs);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resourceID);
            Assert.AreEqual(Guid.Empty, resourceID);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Execute_GivenNoArgs_Exception()
        {
            //------------Setup for test-------------------------
            FindResource findResource = new FindResource();
            var workspaceMock = new Mock<IWorkspace>();
            var resId = Guid.NewGuid();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>
            {
                {"FindResourceTests", "SomeName".ToStringBuilder()},
                {"ResourceType", "ResourceType".ToStringBuilder()}
            };
            var executeResults = findResource.Execute(requestArgs, workspaceMock.Object);
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            Assert.IsNotNull(executeResults);
            var deserializedResults = jsonSerializer.Deserialize<IList<SerializableResource>>(executeResults);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deserializedResults);
        }        
    }
}
