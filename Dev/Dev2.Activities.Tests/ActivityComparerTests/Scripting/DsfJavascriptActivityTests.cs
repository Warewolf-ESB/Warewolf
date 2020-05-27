using System;
using System.Linq;
using Dev2.Activities.Scripting;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.State;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityComparerTests.Scripting
{
    [TestClass]
    public class DsfJavascriptActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void UniqueIDEquals_EmptyJavascript_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId };
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfJavascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfJavascriptActivity.Equals(javascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyCountRecordset_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dsfJavascriptActivity = new DsfJavascriptActivity();
            var javascriptActivity = new DsfJavascriptActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfJavascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfJavascriptActivity.Equals(javascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Script_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Script = "a" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Script = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Script_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Script = "A" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Script = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Script_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Script = "AAA" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Script = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Result = "a" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Result = "A" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Result = "AAA" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeFile_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, IncludeFile = "a" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, IncludeFile = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, IncludeFile = "A" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, IncludeFile = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeFile_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, IncludeFile = "AAA" };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, IncludeFile = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ScriptType_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, ScriptType = enScriptType.JavaScript };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, ScriptType = enScriptType.JavaScript };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ScriptType_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, ScriptType = enScriptType.JavaScript };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, ScriptType = enScriptType.Python };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void EscapeScript_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = true };
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfJavascriptActivity.Equals(javascriptActivity));
            //---------------Execute Test ----------------------
            dsfJavascriptActivity.EscapeScript = true;
            javascriptActivity.EscapeScript = false;
            var @equals = dsfJavascriptActivity.Equals(javascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void EscapeScript_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = true };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = false };
            //---------------Assert Precondition----------------
            Assert.IsFalse(javascriptActivity.Equals(dsfJavascriptActivity));
            //---------------Execute Test ----------------------
            javascriptActivity.EscapeScript = true;
            dsfJavascriptActivity.EscapeScript = true;
            var @equals = javascriptActivity.Equals(dsfJavascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        public void JavascriptActivity_ExecuteTool()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = true };
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = false };

            var mockObject = new Mock<IDSFDataObject>() ;
            var env = new ExecutionEnvironment();
            var esbChannel = new Mock<IEsbChannel>().Object;
            var errorResultTO = new ErrorResultTO();
            mockObject.Setup(o => o.Environment).Returns(() => env);
            mockObject.Setup(o => o.IsDebugMode()).Returns(() => true);
            //---------------Execute Test ----------------------
            javascriptActivity.Execute(mockObject.Object, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual("True", env.HasErrors().ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfJavascriptActivity_GetState")]
        public void DsfJavascriptActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfJavascriptActivity { Script = "def add", EscapeScript = true, IncludeFile = "temp.py", Result = "[[res]]" };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(4, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Script",
                    Type = StateVariable.StateType.Input,
                    Value = "def add"
                },
                new StateVariable
                {
                    Name = "IncludeFile",
                    Type = StateVariable.StateType.Input,
                    Value = "temp.py"
                },
                new StateVariable
                {
                    Name = "EscapeScript",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "[[res]]"
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}

