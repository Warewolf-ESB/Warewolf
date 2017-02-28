using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchComPluginActionsTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var comPluginActions = new FetchComPluginActions();

            //------------Execute Test---------------------------
            var resId = comPluginActions.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchComPluginActions = new FetchComPluginActions();

            //------------Execute Test---------------------------
            var resId = fetchComPluginActions.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildServiceInputName_GivenTypeNames_ShouldConcatinateTypeWithName()
        {
            //---------------Set up test pack-------------------
            FetchComPluginActions comPluginActions = new FetchComPluginActions();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(comPluginActions);
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(comPluginActions);
            var invoke = privateObject.Invoke("BuildServiceInputName", "Class2", "Project1.Class2&, Project1, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null");
            //---------------Test Result -----------------------
            Assert.AreEqual("Class2 (Project1.Class2)", invoke.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildServiceInputName_GivenCursorLocationEnumGetCorrectEnumName()
        {
            //---------------Set up test pack-------------------
            FetchComPluginActions comPluginActions = new FetchComPluginActions();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(comPluginActions);
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(comPluginActions);

            var typeConverter = TypeDescriptor.GetConverter("ADODB.CursorLocationEnum, ADODB, Version=6.1.0.0, Culture=neutral, PublicKeyToken=null");

            var invoke = privateObject.Invoke("BuildServiceInputName", "Class2", "ADODB.CursorLocationEnum, ADODB, Version=6.1.0.0, Culture=neutral, PublicKeyToken=null");
            //---------------Test Result -----------------------
            Assert.AreEqual("Class2 (ADODB.CursorLocationEnum)", invoke.ToString());
        }
    }
}
