using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarewolfCOMIPC.Client;

// ReSharper disable InconsistentNaming

namespace WarewolfCOMIPC.Test
{
    [TestClass]
    public class WarewolfComIpcTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfCOMIPCClient_Execute")]
        public void WarewolfCOMIPCClient_Execute_GetType_ShouldReturnType()
        {
            //------------Setup for test--------------------------

            var clsid = new Guid("00000514-0000-0010-8000-00AA006D2EA4");
            //------------Execute Test---------------------------
            using (Client.Client client = new Client.Client())
            {
                var execute = client.Invoke(clsid, "", "GetType", new object[] { });
                Assert.IsNotNull(execute);
            }
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetMethods_GivenPersonLib_PersonController_ShouldReturnMethodList()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid("2AC49130-C532-4154-B0DC-E930370D36EA");
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (Client.Client client = new Client.Client())
            {
                var execute = client.Invoke(classId, "", "GetMethods", new object[] { });
                var enumerable = execute as List<MethodInfoTO>;
                Assert.IsNotNull(enumerable);
                //---------------Test Result -----------------------
                Assert.AreNotEqual(10, enumerable.Count);
            }

        }
    }
}
