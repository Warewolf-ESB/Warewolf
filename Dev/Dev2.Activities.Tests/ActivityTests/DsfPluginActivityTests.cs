
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
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Services.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfPluginActivityTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Plugin Service Execution

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfPluginActivity_Execute")]
// ReSharper disable InconsistentNaming
        public void DsfPluginActivity_Execute_WhenErrors_ExpectErrors()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var dsfPluginActivity = new FaultyMockDsfPluginActivity();
            
            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            dsfPluginActivity.MockExecutionImpl(null, null, null, null, out invokeErrors);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, invokeErrors.FetchErrors().Count);
            Assert.AreEqual("Something bad happened", invokeErrors.FetchErrors()[0]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test 'GetNewPluginServiceExecution' for 'DsfPluginActivity': A valid plugin service execution is constructed by DsfPluginActivity")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void DsfPluginActivity_DsfPluginActivityUnitTest_GetNewPluginServiceExecution_ServiceConstructed()
        // ReSharper restore InconsistentNaming
        {
            //init
            var expected = Guid.NewGuid();
            var pluginActivity = new MockDsfPluginActivity();
            var mockContext = new DsfDataObject("<DataList></DataList>", expected);

            //exe
            var actual = pluginActivity.MockGetNewPluginServiceExecution(mockContext);

            //assert
            Assert.AreEqual(typeof(PluginServiceExecution), actual.GetType(), "DsfPluginActivity did not construct a correct PluginServiceExecution");
            Assert.AreEqual(expected, actual.DataObj.DataListID, "The Plugin Service Execution constructed by DsfPluginActivity is using the incorrect datalist");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for Execution of 'PluginServiceExecution': A valid plugin service is executed")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void DsfPluginActivity_DsfPluginActivityUnitTest_ExecutePluginService_ServiceExecuted()
        // ReSharper restore InconsistentNaming
        {
            //init
            var pluginActivity = new MockDsfPluginActivity();
            var errors = new ErrorResultTO();
            var mockContainer = new Mock<PluginServiceExecution>(new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>()), It.IsAny<bool>());
            mockContainer.Setup(c => c.Execute(out errors)).Verifiable();

            //exe
            pluginActivity.MockExecutePluginService(mockContainer.Object);

            //assert
            Assert.IsFalse(errors.HasErrors(), "Errors where thrown while executing a plugin service");
            mockContainer.Verify(c => c.Execute(out errors), Times.Once());
        }

        #endregion
    }
}
