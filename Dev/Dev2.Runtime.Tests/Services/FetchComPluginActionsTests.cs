using System.ComponentModel;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchComPluginActionsTests
    {
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
