using System;
using System.Linq;
using Dev2.Activities.Scripting;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Scripting
{
    [TestClass]
    public class DsfRubyActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void UniqueIDEquals_EmptyJavascript_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId };
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfRubyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfRubyActivity.Equals(javascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyCountRecordset_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dsfRubyActivity = new DsfRubyActivity();
            var javascriptActivity = new DsfRubyActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfRubyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfRubyActivity.Equals(javascriptActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Script_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, Script = "a" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, Script = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Script_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, Script = "A" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, Script = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Script_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, Script = "AAA" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, Script = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, Result = "a" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, Result = "A" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, Result = "AAA" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeFile_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, IncludeFile = "a" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, IncludeFile = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, IncludeFile = "A" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, IncludeFile = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeFile_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, IncludeFile = "AAA" };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, IncludeFile = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ScriptType_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, ScriptType = enScriptType.JavaScript };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, ScriptType = enScriptType.JavaScript };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ScriptType_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, ScriptType = enScriptType.JavaScript };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, ScriptType = enScriptType.Python };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(javascriptActivity);
            //---------------Execute Test ----------------------
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void EscapeScript_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, EscapeScript = true };
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, EscapeScript = true};
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfRubyActivity.Equals(javascriptActivity));
            //---------------Execute Test ----------------------
            dsfRubyActivity.EscapeScript = true;
            javascriptActivity.EscapeScript = false;
            var @equals = dsfRubyActivity.Equals(javascriptActivity);
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
            var javascriptActivity = new DsfRubyActivity() { UniqueID = uniqueId, EscapeScript =true };
            var dsfRubyActivity = new DsfRubyActivity() { UniqueID = uniqueId, EscapeScript = false };
            //---------------Assert Precondition----------------
            Assert.IsFalse(javascriptActivity.Equals(dsfRubyActivity));
            //---------------Execute Test ----------------------
            javascriptActivity.EscapeScript = true;
            dsfRubyActivity.EscapeScript = true;
            var @equals = javascriptActivity.Equals(dsfRubyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfRubyActivity_GetState")]
        public void DsfRubyActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfRubyActivity { Script = "def add", EscapeScript = true, IncludeFile = "temp.py", Result = "[[res]]" };
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