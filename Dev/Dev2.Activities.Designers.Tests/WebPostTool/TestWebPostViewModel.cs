using System;
using Dev2.Activities.Designers.Tests.WebGetTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.WebPostTool
{
    [TestClass]
    public class TestWebPostViewModel
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WebPost_MethodName")]
        public void WebPost_MethodName_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = new Guid();
            var mod = new MyWebModel();
            var webPost = new DsfWebPostActivity();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
