using Dev2.Common.ExtMethods;
using Dev2.Web2.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Warewolf.Auditing;

namespace Dev2.Web.Tests
{
    [TestClass]
    public class AuditControllerTests
    {
        [TestMethod]
        public void Resuming_Workflow_Given_UnAuthorizedUser_Authentication()
        {
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            request.SetupGet(x => x.Headers).Returns(
                new WebHeaderCollection
                {
                    { "X-Requested-With", "XMLHttpRequest"},
                    { "Authorization", "Basic ZGV2MlxJbnRlcmdyYXRpb25UZXN0ZXI6STczNTczcjA="},
                });

            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Response).Returns(response.Object);
            var controller = new AuditController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            controller.TempData.Add("allowLogin", true);            
            var url = "http://localhost:3142/secure/WorkflowResume";
            string returnedExceptionMessage = "";
            try
            {
                controller.PerformResume(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), url);
            }
            catch (Exception ex)
            {
                returnedExceptionMessage = ex.Message;
            }
            Assert.AreEqual("The remote server returned an error: (401) Unauthorized.", returnedExceptionMessage);
        }

        [TestMethod]
        public void Resuming_Workflow_Does_Not_Require_Authentication_Given_allowLogIn_Is_False()
        {
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            request.SetupGet(x => x.Headers).Returns(
                new WebHeaderCollection
                {
                    { "X-Requested-With", "XMLHttpRequest"},
                });

            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Response).Returns(response.Object);
            var controller = new AuditController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            controller.TempData.Add("allowLogin", false);
            controller.PerformResume(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            response.VerifySet(res => res.StatusCode = 401, Times.AtLeastOnce);
            Assert.AreEqual(1, controller.TempData.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AuditController))]
        public void AuditController_AuditList_JsonDataString_Invalid_Json_ShouldReturn_EmptyModel()
        {
            //----------------------------Arrange-----------------------------
            using (var controller = new AuditController())
            {
                //-------------------Act---------------------------------
                var result = controller.AuditList("invalidJson");
                
                //-------------------Assert-----------------------------------
                Assert.IsInstanceOfType(result, typeof(ActionResult));
                Assert.IsInstanceOfType(result, typeof(PartialViewResult));

                var partialViewResult = (PartialViewResult)result; 
                Assert.AreEqual("AuditList", partialViewResult.ViewName);
            };
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AuditController))]
        public void AuditController_AuditList_JsonDataString_Valid_Json_ShouldReturn_EmptyModel()
        {
            //----------------------------Arrange-----------------------------
            using (var controller = new AuditController())
            {
                //-------------------Arrange-----------------------------
                var validJosonA = @"{ 'AuditType': 'testTypeA','AuditDate':'2019/10/03'}";
                var validJosonB = @"{ 'AuditType': 'testTypeB','AuditDate':'2019/10/03'}";

                var AuditA = JsonConvert.DeserializeObject<Audit>(validJosonA);
                var AuditB = JsonConvert.DeserializeObject<Audit>(validJosonB);

                var audits = new List<Audit>
                {
                    AuditA,
                    AuditB
                };

                //-------------------Act---------------------------------
                var expectedAudits = JsonConvert.SerializeObject(audits);

                var result = controller.AuditList(expectedAudits);

                //-------------------Assert-----------------------------------
                Assert.IsInstanceOfType(result, typeof(ActionResult));
                Assert.IsInstanceOfType(result, typeof(PartialViewResult));

                var partialViewResult = (PartialViewResult)result;
                Assert.AreEqual("AuditList", partialViewResult.ViewName);
                Assert.AreEqual(expectedAudits, JsonConvert.SerializeObject(partialViewResult.ViewData.Model));
            };
        }

    }
}
