/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services.Esb;
using Dev2.Workspaces;
using TechTalk.SpecFlow.Assist;
using Warewolf.Client;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class ClusterJoinTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterJoinService))]
        public void ClusterJoinService_()
        {
            var service = new ClusterJoinService();
            Assert.AreEqual(AuthorizationContext.Any, service.GetAuthorizationContextForService());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ClusterJoinService))]
        public void ClusterJoinService_GivenEmptyKey_ExpectNullResponse()
        {
            var serializer = new Dev2JsonSerializer();
            var workspaceId = Guid.NewGuid();
            var values = new Dictionary<string, StringBuilder>();
            var workspace = new Workspace(workspaceId);

            var service = new ClusterJoinService();
            var resultString = service.Execute(values, workspace);

            var result = serializer.Deserialize<ClusterJoinResponse>(resultString);
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ClusterJoinService))]
        public void ClusterJoinService_GivenInvalidKey_ExpectNullResponse()
        {
            var serializer = new Dev2JsonSerializer();
            var workspaceId = Guid.NewGuid();
            var values = new Dictionary<string, StringBuilder>();
            values.Add(Warewolf.Service.Cluster.ClusterJoinRequest.Key, new StringBuilder(Guid.NewGuid().ToString()));
            var workspace = new Workspace(workspaceId);

            var service = new ClusterJoinService();
            var resultString = service.Execute(values, workspace);

            var result = serializer.Deserialize<ClusterJoinResponse>(resultString);
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ClusterJoinService))]
        public void ClusterJoinService_GivenValidKey_ExpectValidResponse()
        {
            var serializer = new Dev2JsonSerializer();
            var workspaceId = Guid.NewGuid();
            var values = new Dictionary<string, StringBuilder>();
            values.Add(Warewolf.Service.Cluster.ClusterJoinRequest.Key, new StringBuilder(Config.Cluster.Key));

            var workspace = new Workspace(workspaceId);

            var service = new ClusterJoinService();
            var resultString = service.Execute(values, workspace);

            var result = serializer.Deserialize<ClusterJoinResponse>(resultString);
            Assert.IsNotNull(result);
            Assert.AreNotEqual(Guid.Empty, result.Token);
        }
    }
}
