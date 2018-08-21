using Dev2.Web2.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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
    }
}
