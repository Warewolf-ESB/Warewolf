using System;
using Dev2.Runtime.WebServer.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    public class AuthorizeHubAttributeTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_Constructor")]
        public void AuthorizeHubAttribute_Constructor_Default_ProviderIsAuthorizationProviderInstance()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new AuthorizeHubAttribute();

            //------------Assert Results-------------------------
            Assert.AreSame(AuthorizationProvider.Instance, attribute.Provider);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeHubAttribute_Constructor_AuthorizationProviderIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new AuthorizeHubAttribute(null);

            //------------Assert Results-------------------------
        }
    }
}