using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

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
    }
}
