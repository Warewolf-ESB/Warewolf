
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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Util
{
    [TestClass]
    public class FindResourceHelperTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FindResourceHelper_SerializeResourceForStudio")]
// ReSharper disable InconsistentNaming
        public void FindResourceHelper_SerializeResourceForStudio_WhenNewResource_ExpectValidResource()

        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            IResource res = new Resource { ResourceID = id, IsNewResource = true };

            //------------Execute Test---------------------------

            var result = new FindResourceHelper().SerializeResourceForStudio(res);

            //------------Assert Results-------------------------

            Assert.IsTrue(result.IsNewResource);
            Assert.AreEqual(id, result.ResourceID);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FindResourceHelper_SerializeResourceForStudio")]
        public void FindResourceHelper_SerializeResourceForStudio_WhenNotNewResource_ExpectValidResource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            IResource res = new Resource { ResourceID = id, IsNewResource = false };

            //------------Execute Test---------------------------

            var result = new FindResourceHelper().SerializeResourceForStudio(res);

            //------------Assert Results-------------------------

            Assert.IsFalse(result.IsNewResource);
            Assert.AreEqual(id, result.ResourceID);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FindResourceHelper_SerializeResourceForStudio")]
        public void FindResourceHelper_SerializeResourceForStudio_WhenCheckingAllProperties_ExpectValidResource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();

            var theErrors = new List<IErrorInfo>
            {
                new ErrorInfo
                    {
                        ErrorType = ErrorType.None, 
                        FixData = "fixme", 
                        Message = "message", 
                        FixType = FixType.None, 
                        InstanceID = id, StackTrace = "stacktrace"
                    }
            };

            Resource res = new Resource
            {
                Inputs = "inputs",
                Outputs = "outputs",
                ResourceID = id,
                IsNewResource = false,
                DataList = "abc",
                IsValid = true,
                ResourcePath = "Category",
                ResourceName = "Workflow",
                ResourceType = ResourceType.WorkflowService,
                Errors = theErrors,
                VersionInfo = new VersionInfo(DateTime.Now,"","u","1",id,Guid.NewGuid())
            };

            //------------Execute Test---------------------------
            var result = new FindResourceHelper().SerializeResourceForStudio(res);

            //------------Assert Results-------------------------

            // convert to string due to silly interface problems ;)
            var errorString = JsonConvert.SerializeObject(theErrors);
            var resultErrorString = JsonConvert.SerializeObject(result.Errors);

            Assert.IsFalse(result.IsNewResource);
            Assert.IsTrue(result.IsValid);

            Assert.AreEqual(id, result.ResourceID);
            Assert.AreEqual("abc", result.DataList);
            Assert.AreEqual("Category", result.ResourceCategory);
            Assert.AreEqual("Workflow", result.ResourceName);
            Assert.AreEqual(ResourceType.WorkflowService, result.ResourceType);
            Assert.AreEqual(errorString, resultErrorString);
            Assert.AreEqual("inputs", result.Inputs);
            Assert.AreEqual("outputs", result.Outputs);
            Assert.AreEqual(res.VersionInfo, result.VersionInfo);
        }

    }
    // ReSharper restore InconsistentNaming
}
