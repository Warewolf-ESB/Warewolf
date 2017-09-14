using System;
using Dev2.Activities.Scripting;
using Dev2.Common.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Scripting
{
    [TestClass]
    public class DsfJavascriptActivityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
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
        [Owner("Nkosinathi Sangweni")]
        public void EscapeScript_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfJavascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = true };
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript = true};
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
        [Owner("Nkosinathi Sangweni")]
        public void EscapeScript_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var javascriptActivity = new DsfJavascriptActivity() { UniqueID = uniqueId, EscapeScript =true };
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
    }
}
