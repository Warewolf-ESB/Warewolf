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
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using ActivityUnitTests;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Enums;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    
    [TestClass]
    public class DsfActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_BeforeExecutionStart")]
        public void DsfActivity_BeforeExecutionStart_NullResourceID_DoesNothing()
        {
            //------------Setup for test--------------------------
            DsfActivity act = new DsfActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping, ResourceID = null };
            List<DebugItem> inRes;
            List<DebugItem> outRes;
            //------------Execute Test---------------------------
            CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes);

            // remove test datalist ;)

            //------------Assert Results-------------------------
            Assert.AreEqual(5, inRes.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_GetForEachInputs")]
        [ExpectedException(typeof(NotImplementedException))]
        public void DsfActivity_GetForEachInputs_WhenExecuted_ThrowsException()
        {
            //------------Setup for test--------------------------
            var dsfActivity = new DsfActivity();

            //------------Execute Test---------------------------
            dsfActivity.GetForEachInputs();
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_GetForEachOutputs")]
        [ExpectedException(typeof(NotImplementedException))]
        public void DsfActivity_GetForEachOutputs_WhenExecuted_ThrowsException()
        {
            //------------Setup for test--------------------------
            var dsfActivity = new DsfActivity();

            //------------Execute Test---------------------------
            dsfActivity.GetForEachOutputs();
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_UpdateForEachInputs")]
        [ExpectedException(typeof(NotImplementedException))]
        public void DsfActivity_UpdateForEachInputs_WhenExecuted_ThrowsException()
        {
            //------------Setup for test--------------------------
            var dsfActivity = new DsfActivity();

            //------------Execute Test---------------------------
            dsfActivity.UpdateForEachInputs(new List<Tuple<string, string>>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_UpdateForEachOutputs")]
        [ExpectedException(typeof(NotImplementedException))]
        public void DsfActivity_UpdateForEachOutputs_WhenExecuted_ThrowsException()
        {
            //------------Setup for test--------------------------
            var dsfActivity = new DsfActivity();

            //------------Execute Test---------------------------
            dsfActivity.UpdateForEachOutputs(new List<Tuple<string, string>>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_GetFindMissingType")]
        public void DsfActivity_GetFindMissingType_Executed_ReturnsDsfActivity()
        {
            //------------Setup for test--------------------------
            var dsfActivity = new DsfActivity();

            //------------Execute Test---------------------------
            var findMissingType = dsfActivity.GetFindMissingType();
            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.DsfActivity,findMissingType);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_GetOutputs")]
        public void DsfActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfActivity { Outputs =  new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping {MappedTo = "[[res1]]"},
                new ServiceOutputMapping {MappedTo = "[[res2]]"}
            } };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, outputs.Count);
            Assert.AreEqual("[[res1]]", outputs[0]);
            Assert.AreEqual("[[res1]]", outputs[0]);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_BeforeExecutionStart")]
        public void DsfActivity_BeforeExecutionStart_EmptyResourceID_DoesNothing()
        {
            //------------Setup for test--------------------------
            DsfActivity act = new DsfActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping, ResourceID = new InArgument<Guid>(Guid.Empty) };
            List<DebugItem> inRes;
            List<DebugItem> outRes;
            //------------Execute Test---------------------------
            CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes);

            // remove test datalist ;)

            //------------Assert Results-------------------------
            Assert.AreEqual(5, inRes.Count);
        }

     
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_BeforeExecutionStart")]
        public void DsfActivity_BeforeExecutionStart_ResourceIDAutorised_Executes()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            DsfActivity act = new DsfActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping, ResourceID = new InArgument<Guid>(resourceID) };
            var mockAutorizationService = new Mock<IAuthorizationService>();
            mockAutorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, resourceID.ToString())).Returns(true);

            PrivateObject p = new PrivateObject(act);
            p.SetProperty("AuthorizationService", mockAutorizationService.Object);

            List<DebugItem> inRes;
            List<DebugItem> outRes;
            //------------Execute Test---------------------------
            CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes, new Mock<IPrincipal>().Object);

            // remove test datalist ;)
            //------------Assert Results-------------------------
            Assert.AreEqual(5, inRes.Count);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfActivity_OnExecute")]
        public void DsfActivity_OnExecute_WhenLocalExecutionInLocalContext_ExpectEnviromentIDRemainsLocalAndOverrideSetToFalse()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var environmentID = Guid.Empty;
            DsfActivity act = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = Guid.Empty
            };
            var mockAutorizationService = new Mock<IAuthorizationService>();
            mockAutorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, resourceID.ToString())).Returns(true);

            PrivateObject p = new PrivateObject(act);
            p.SetProperty("AuthorizationService", mockAutorizationService.Object);

            //------------Execute Test---------------------------
            TestStartNode = new FlowStep
            {
                Action = act
            };

            TestData = "<DataList></DataList>";
            CurrentDl = "<DataList></DataList>";
            User = new Mock<IPrincipal>().Object;
            var result = ExecuteProcess(null, true, null, false, true, false, environmentID);

            
            var resultEnvironmentID = result.EnvironmentID;
            var isRemoteOverridden = result.IsRemoteInvokeOverridden;
            

            //------------Assert Results-------------------------
            Assert.AreEqual(environmentID, resultEnvironmentID);
            Assert.IsFalse(isRemoteOverridden);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfActivity_UpdateDebugParentID")]
        
        public void DsfActivity_UpdateDebugParentID_UniqueIdSameIfNestingLevelNotChanged()

        {
            var dataObject = new DsfDataObject(CurrentDl, Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = true,
            };

            DsfActivity act = new DsfActivity();
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfActivity_UpdateDebugParentID")]
        
        public void DsfActivity_UpdateDebugParentID_UniqueIdNotSameIfNestingLevelIncreased()

        {
            var dataObject = new DsfDataObject(CurrentDl, Guid.NewGuid())
            {
                ServerID = Guid.NewGuid(),
                IsDebug = true,
                ForEachNestingLevel = 1
            };

            DsfActivity act = new DsfActivity();
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreNotEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_Constructor")]
        public void DsfActivity_Constructor_WithParameters_ShouldSetValues()
        {
            //------------Setup for test--------------------------
            const string toolboxFriendlyName = "toolBoxName";
            const string iconPath = "iconPath";
            const string serviceName = "serviceName";
            const string dataTags = "tags";
            const string resultValidationRequiredTags = "resValidationTags";
            const string resultValidationExpression = "resValidationExp";
            //------------Execute Test---------------------------

            var dsfActivity = new DsfActivity(toolboxFriendlyName,iconPath,serviceName,dataTags,resultValidationRequiredTags,resultValidationExpression);
            //------------Assert Results-------------------------
            Assert.AreEqual(toolboxFriendlyName,dsfActivity.ToolboxFriendlyName);
            Assert.AreEqual(serviceName, dsfActivity.ServiceName);
            Assert.AreEqual(dataTags, dsfActivity.DataTags);
            Assert.AreEqual(resultValidationRequiredTags, dsfActivity.ResultValidationRequiredTags);
            Assert.AreEqual(resultValidationExpression, dsfActivity.ResultValidationExpression);
            Assert.IsFalse(dsfActivity.IsService);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_Inputs")]
        public void DsfActivity_Inputs_GivenValue_ShouldSetProperty()
        {
            //------------Setup for test--------------------------
            var dsfActivity = new DsfActivity();
            
            //------------Execute Test---------------------------
            dsfActivity.Inputs = new List<IServiceInput>();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfActivity.Inputs);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_Inputs")]
        public void DsfActivity_Inputs_GivenNull_ShouldNotSetProperty()
        {
            //------------Setup for test--------------------------
            var dsfActivity = new DsfActivity();
            dsfActivity.Inputs = new List<IServiceInput>();
            //------------Assert Precondition--------------------
            Assert.IsNotNull(dsfActivity.Inputs);
            //------------Execute Test---------------------------
            dsfActivity.Inputs = null;
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfActivity.Inputs);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_ExecuteTool")]
        
        public void DsfActivity_ExecuteTool_DataObjectWithNullChannel_ShouldError()

        {
            var dataObject = new DsfDataObject(CurrentDl, Guid.NewGuid())
            {
                ServerID = Guid.NewGuid(),
                IsDebug = true,
                ForEachNestingLevel = 1
            };

            DsfActivity act = new DsfActivity();
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.Execute(dataObject, 0);
            Assert.IsTrue(dataObject.Environment.HasErrors());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_BeforeExecutionStart")]
        public void DsfActivity_GetOutputs_NotObjectAndOutputMapping_GetsOutputs()
        {
            //------------Setup for test--------------------------
            DsfActivity act = new DsfActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping, ResourceID = null };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.IsNotNull(outputs);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_BeforeExecutionStart")]
        public void DsfActivity_GetOutptus_ObjectAndOutputMapping_GetsObjectName()
        {
            //------------Setup for test--------------------------
            DsfActivity act = new DsfActivity
            {
                InputMapping = ActivityStrings.DsfActivityInputMapping,
                OutputMapping = ActivityStrings.DsfActivityOutputMapping,
                ResourceID = null,
                IsObject =  true,
                ObjectName = "Obj",
                ObjectResult = "{Name:BOb}"
            };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.IsNotNull(outputs);
            Assert.AreEqual("Obj",outputs[0]);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_OnExecute")]
        public void DsfActivity_OnExecute_WhenIsTestExecutionShouldUpdateDataObject_SetBackToTrue()
        {
            //------------Setup for test--------------------------
            var environmentID = Guid.Empty;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {

                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };
            var resourceID = Guid.NewGuid();
            
            DsfActivity act = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = Guid.Empty
            };
            var mockAutorizationService = new Mock<IAuthorizationService>();
            mockAutorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, resourceID.ToString())).Returns(true);

            PrivateObject p = new PrivateObject(act);
            p.SetProperty("AuthorizationService", mockAutorizationService.Object);

            //------------Execute Test---------------------------
            TestStartNode = new FlowStep
            {
                Action = act
            };

            TestData = "<DataList></DataList>";
            CurrentDl = "<DataList></DataList>";
            User = new Mock<IPrincipal>().Object;
            ExecuteProcess(dataObject, true, null, false, true, false, environmentID);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataObject.IsServiceTestExecution);
        }
    }
}
