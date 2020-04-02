﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services.Esb;
using Dev2.Workspaces;
using Warewolf.Client;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class ClusterJoinRequestTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ClusterJoinRequestService))]
        public void ClusterJoinRequestService_()
        {
            var service = new ClusterJoinRequestService();
            Assert.AreEqual(AuthorizationContext.Any, service.GetAuthorizationContextForService());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ClusterJoinRequestService))]
        public void ClusterJoinRequestService__()
        {
            var serializer = new Dev2JsonSerializer();
            var workspaceId = Guid.NewGuid();
            var values = new Dictionary<string, StringBuilder>();
            var workspace = new Workspace(workspaceId);
            
            var service = new ClusterJoinRequestService();
            var resultString = service.Execute(values, workspace);
            
            var result = serializer.Deserialize<ClusterJoinResponse>(resultString);
            Assert.IsNotNull(result);
        }
    }
}
