using System;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities.Scripting;
using Dev2.Common.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.Scripting
{
    [TestClass]
    public class DsfPythonActivityTests : BaseActivityUnitTest
    {
        [ClassCleanup]
        public static void Cleaner()
        {
            try
            {
            }
            catch (Exception)
            {
                //supress exceptio
            }
        }
        
      
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Attribute_GivenIsNew_ShouldhaveCorrectValues()
        {
            //---------------Set up test pack-------------------
            var act = new DsfPythonActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(act);
            //---------------Execute Test ----------------------
            var toolDescriptorInfo = typeof(DsfPythonActivity).GetCustomAttributes(typeof(ToolDescriptorInfo), false).Single() as ToolDescriptorInfo;
            //---------------Test Result -----------------------
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual("Scripting", toolDescriptorInfo.Category );
            Assert.AreEqual("python script", toolDescriptorInfo.FilterTag );
            Assert.AreEqual("Scripting-Python", toolDescriptorInfo.Icon );
            Assert.AreEqual("Python", toolDescriptorInfo.Name );
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_GivenType_ShouldInheritCorrectly()
        {
            //---------------Set up test pack-------------------
            var act = new DsfPythonActivity();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsInstanceOfType(act, typeof(DsfActivityAbstract<string>));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_GivenIsNew_ShouldSetJavascript()
        {
            //---------------Set up test pack-------------------
            var act = new DsfPythonActivity();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(act, typeof(DsfActivityAbstract<string>));
            //---------------Execute Test ----------------------
            var displayName = act.DisplayName;
            //---------------Test Result -----------------------
            Assert.AreEqual("Python", displayName);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Script_GivenIsNew_ShouldBeEmpty()
        {
            //---------------Set up test pack-------------------
            var act = new DsfPythonActivity();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(act, typeof(DsfActivityAbstract<string>));
            //---------------Execute Test ----------------------
            var displayName = act.Script;
            //---------------Test Result -----------------------
            Assert.AreEqual("", displayName);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ScriptType_GivenIsNew_ShouldSetJavascript()
        {
            //---------------Set up test pack-------------------
            var act = new DsfPythonActivity();
            //---------------Assert Precondition----------------
            Assert.AreEqual("Python", act.DisplayName);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enScriptType.Python, act.ScriptType);

        }


       

    }
}