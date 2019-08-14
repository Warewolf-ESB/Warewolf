/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Serializers;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Warewolf.Options;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FindOptionsTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FindOptions))]
        public void FindOptions_CreateServiceEntry_Returns_Options()
        {
            //------------Setup for test-------------------------
            var findOptions = new FindOptions();
            //------------Execute Test---------------------------
            var dynamicService = findOptions.CreateServiceEntry();
            var handleType = findOptions.HandlesType();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsFalse(string.IsNullOrEmpty(handleType));
            Assert.AreEqual(handleType, dynamicService.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FindOptions))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindOptions_GivenEmptyArgs_Returns_ArgumentNullException()
        {
            //------------Setup for test-------------------------
            var findOptions = new FindOptions();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            findOptions.Execute(requestArgs, workspaceMock.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FindOptions))]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FindOptions_GivenNullArgs_Returns_InvalidDataContractException()
        {
            //------------Setup for test-------------------------
            var findOptions = new FindOptions();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------           
            findOptions.Execute(null, workspaceMock.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FindOptions))]
        public void FindOptions_Execute_SelectedSource_HasResult()
        {
            //------------Setup for test-------------------------
            var findOptions = new FindOptions();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>
            {
                {"SelectedSource", "Source".ToStringBuilder()}
            };
            var executeResults = findOptions.Execute(requestArgs, workspaceMock.Object);
            var jsonSerializer = new Dev2JsonSerializer();
            Assert.IsNotNull(executeResults);
            var deserializedResults = jsonSerializer.Deserialize<List<IOption>>(executeResults);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deserializedResults);
            Assert.IsTrue(deserializedResults.Count > 0);
        }
    }
}
