
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
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    // ReSharper disable InconsistentNaming
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            var result = CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            //------------Assert Results-------------------------
            Assert.AreEqual(5, inRes.Count);
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
            var result = CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            //------------Assert Results-------------------------
            Assert.AreEqual(5, inRes.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity_BeforeExecutionStart")]
        public void DsfActivity_BeforeExecutionStart_ResourceIDNotAutorised_ThrowsException()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            DsfActivity act = new DsfActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping, ResourceID = new InArgument<Guid>(resourceID), ServiceName = resourceID.ToString() };
            var mockAutorizationService = new Mock<IAuthorizationService>();
            mockAutorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, resourceID.ToString())).Returns(false);
            act.AuthorizationService = mockAutorizationService.Object;
            var mockPrincipal = new Mock<IPrincipal>();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(identity => identity.Name).Returns("SomeUser");
            mockPrincipal.Setup(principal => principal.Identity).Returns(mockIdentity.Object);
            List<DebugItem> inRes;
            List<DebugItem> outRes;
            //------------Execute Test---------------------------
            var result = CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes, mockPrincipal.Object);

            // remove test datalist ;)
            var compiler = DataListFactory.CreateDataListCompiler();
            var errors = compiler.FetchErrors(result.DataListID);
            DataListRemoval(result.DataListID);
            //--------------Assert result----------------------------
            StringAssert.Contains(errors, string.Format("User: SomeUser does not have Execute Permission to resource {0}.", resourceID));

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
            act.AuthorizationService = mockAutorizationService.Object;
            List<DebugItem> inRes;
            List<DebugItem> outRes;
            //------------Execute Test---------------------------
            var result = CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes, new Mock<IPrincipal>().Object);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            //------------Assert Results-------------------------
            Assert.AreEqual(5, inRes.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfActivity")]
        public void DsfActivity_ExecutionError_ShouldBeInErrors()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            DsfActivity act = new DsfErrorActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping, ResourceID = new InArgument<Guid>(resourceID) };
            var mockAutorizationService = new Mock<IAuthorizationService>();
            mockAutorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, resourceID.ToString())).Returns(true);
            act.AuthorizationService = mockAutorizationService.Object;
            List<DebugItem> inRes;
            List<DebugItem> outRes;
            //------------Execute Test---------------------------
            var result = CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                               "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes, new Mock<IPrincipal>().Object);


            var compiler = DataListFactory.CreateDataListCompiler();
            var errors = compiler.FetchErrors(result.DataListID);
            DataListRemoval(result.DataListID);
            //------------Assert Results-------------------------
            StringAssert.Contains(errors, "This is an error");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfActivity_OnExecute")]
        public void DsfActivity_OnExecute_WhenRemoteExecutionInLocalContext_ExpectEnviromentIDRemainsRemoteAndOverrideSetToTrue()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var environmentID = Guid.NewGuid();
            DsfActivity act = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = Guid.Empty
            };
            var mockAutorizationService = new Mock<IAuthorizationService>();
            mockAutorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, resourceID.ToString())).Returns(true);
            act.AuthorizationService = mockAutorizationService.Object;

            //------------Execute Test---------------------------
            ErrorResultTO errors;
            TestStartNode = new FlowStep
            {
                Action = act
            };

            TestData = "<DataList></DataList>";
            CurrentDl = "<DataList></DataList>";
            User = new Mock<IPrincipal>().Object;
            Compiler = DataListFactory.CreateDataListCompiler();
            ExecutionId = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl.ToStringBuilder(), out errors);
            var result = ExecuteProcess(null, true, null, false, true, false, environmentID) as IDSFDataObject;

            // ReSharper disable PossibleNullReferenceException
            var isRemoteOverridden = result.IsRemoteInvokeOverridden;
            // ReSharper restore PossibleNullReferenceException

            //------------Assert Results-------------------------
            Assert.IsTrue(isRemoteOverridden);
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
            act.AuthorizationService = mockAutorizationService.Object;

            //------------Execute Test---------------------------
            ErrorResultTO errors;
            TestStartNode = new FlowStep
            {
                Action = act
            };

            TestData = "<DataList></DataList>";
            CurrentDl = "<DataList></DataList>";
            User = new Mock<IPrincipal>().Object;
            Compiler = DataListFactory.CreateDataListCompiler();
            ExecutionId = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl.ToStringBuilder(), out errors);
            var result = ExecuteProcess(null, true, null, false, true, false, environmentID) as IDSFDataObject;

            // ReSharper disable PossibleNullReferenceException
            var resultEnvironmentID = result.EnvironmentID;
            var isRemoteOverridden = result.IsRemoteInvokeOverridden;
            // ReSharper restore PossibleNullReferenceException

            //------------Assert Results-------------------------
            Assert.AreEqual(environmentID, resultEnvironmentID);
            Assert.IsFalse(isRemoteOverridden);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfActivity_BeforeExecutionStart")]
        [ExpectedException(typeof(DebugCopyException))]
        public void DsfActivity_GetDebugInputs_ThrowsErrorIfUnableToParse()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            DsfActivity act = new DsfActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping, ResourceID = new InArgument<Guid>(resourceID) };
            var mockAutorizationService = new Mock<IAuthorizationService>();
            mockAutorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), AuthorizationContext.Execute, resourceID.ToString())).Returns(true);
            act.AuthorizationService = mockAutorizationService.Object;
            var compiler = new Mock<IDataListCompiler>();
            var parser = new Mock<IDev2LanguageParser>();
            var inp1 = new Mock<IDev2Definition>();
            var inp2 = new Mock<IDev2Definition>();
            parser.Setup(a => a.Parse(It.IsAny<string>())).Returns(new List<IDev2Definition> { inp1.Object, inp2.Object });

            var errors = new ErrorResultTO();
            errors.AddError("bob");
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), Dev2.DataList.Contract.enActionType.User, It.IsAny<string>(), false, out errors));
            //------------Execute Test---------------------------
            var dl = new Mock<IBinaryDataList>();
            var guid = Guid.NewGuid();
            dl.Setup(a => a.UID).Returns(guid);
            try
            {
                act.GetDebugInputs(new ExecutionEnvironment());
            }
            catch(Exception err)
            {
                Assert.IsTrue(err.Message.Contains("bob"));
                throw;
            }

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfActivity_UpdateDebugParentID")]
        // ReSharper disable InconsistentNaming
        public void DsfActivity_UpdateDebugParentID_UniqueIdSameIfNestingLevelNotChanged()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void DsfActivity_UpdateDebugParentID_UniqueIdNotSameIfNestingLevelIncreased()
        // ReSharper restore InconsistentNaming
        {
            var dataObject = new DsfDataObject(CurrentDl, Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
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
    }

    public class DsfErrorActivity : DsfActivity
    {
        #region Overrides of DsfActivity

        protected override Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            tmpErrors = new ErrorResultTO();
            tmpErrors.AddError("This is an error");
            return Guid.NewGuid();
        }

        #endregion
    }
}
