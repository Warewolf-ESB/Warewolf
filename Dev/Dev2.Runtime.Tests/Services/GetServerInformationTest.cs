using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetServerInformationTest
    {
        [TestMethod]
        [Owner("Peter Bezuidenhout")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var serverInformation = new GetServerInformation();

            //------------Execute Test---------------------------
            var resId = serverInformation.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Peter Bezuidenhout")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var serverInformation = new GetServerInformation();

            //------------Execute Test---------------------------
            var resId = serverInformation.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

        [TestMethod]
        [Owner("Peter Bezuidenhout")]
        [TestCategory("GetServerInformation_HandlesType")]
        // ReSharper disable InconsistentNaming
        public void GetServerInformation_HandlesType_ExpectName()

        {
            //------------Setup for test--------------------------
            var getInformation = new GetServerInformation();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("GetServerInformation", getInformation.HandlesType());
        }

        [TestMethod]
        [Owner("Peter Bezuidenhout")]
        [TestCategory("GetServerInformation_Execute")]
        public void GetServerInformation_Execute_NullValuesParameter_ErrorResult()
        {
            //------------Setup for test--------------------------
            var getInformation = new GetServerInformation();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = getInformation.Execute(null, null);
            var result = serializer.Deserialize<Dictionary<string, string>>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
        }
    }
}

// ReSharper restore InconsistentNaming