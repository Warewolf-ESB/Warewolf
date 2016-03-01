using System.Net;
using System.Net.Http;
using Dev2.Activities;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class HttpClientCredentialManagerTests
    {

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetCredentialOnHandler_GivenHandlerIsNull_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            var webSource = CreateWebSourceWithCredentials();
            var httpClientHandler = HttpClientCredentialManager.SetCredentialOnHandler(webSource, null);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(httpClientHandler);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetCredentialOnHandler_GivenHandlerIsNotNull_ShouldReturnHttpHandler()
        {
            //---------------Set up test pack-------------------
            var webSource = CreateWebSourceWithCredentials();
            var clientHandler = new HttpClientHandler();
            var httpClientHandler = HttpClientCredentialManager.SetCredentialOnHandler(webSource, clientHandler);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(clientHandler);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(httpClientHandler);
        } 
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetCredentialOnHandler_GivenSourceIsAnonymousAuth_ShouldReturnHttpHandlerWithNoCredentials()
        {
            //---------------Set up test pack-------------------
            var webSource = CreateWebSourceWithNoCredentials();
            var clientHandler = new HttpClientHandler();
            var httpClientHandler = HttpClientCredentialManager.SetCredentialOnHandler(webSource, clientHandler);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(clientHandler);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(httpClientHandler);
        } 
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetCredentialOnHandler_GivenSourceIsUserAuth_ShouldReturnHttpHandlerWithCredentials()
        {
            //---------------Set up test pack-------------------
            var webSource = CreateWebSourceWithCredentials();
            var clientHandler = new HttpClientHandler();
            var httpClientHandler = HttpClientCredentialManager.SetCredentialOnHandler(webSource, clientHandler);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(clientHandler);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(httpClientHandler);
            var credential = (NetworkCredential)httpClientHandler.Credentials;
            Assert.AreEqual("Passwr1", credential.Password);
            Assert.AreEqual("User1", credential.UserName);
        }



        private static WebSource CreateWebSourceWithCredentials()
        {
            return new WebSource()
            {
                AuthenticationType = AuthenticationType.User, 
                Password = "Passwr1",
                UserName = "User1"
            };
        }
        private static WebSource CreateWebSourceWithNoCredentials()
        {
            return new WebSource()
            {
                AuthenticationType = AuthenticationType.Anonymous, 
            };
        }
    }
}