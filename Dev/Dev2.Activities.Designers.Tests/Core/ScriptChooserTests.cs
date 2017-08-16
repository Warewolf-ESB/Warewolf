using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class ScriptChooserTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ScriptChooser_Constructor")]
        public void ScriptChooser_Constructor_Constructor_Execute()
        {
            //------------Setup for test--------------------------
            var scriptChooser = new ScriptChooser();

            //------------Execute Test---------------------------
            Assert.IsNotNull(scriptChooser);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ChooseScriptSources_GivenFileSource_ShouldCallPropertyChange()
        {
            //---------------Set up test pack-------------------
            var scriptChooser = new ScriptChooser();
            var tempFileName = Path.GetTempFileName();
            var chooseScriptSources = scriptChooser.ChooseScriptSources(tempFileName);

            bool wasCalled = false;
            chooseScriptSources.PropertyChanged += (sender, args) =>
             {
                 if(args.PropertyName == "SelectedFiles")
                 {
                     wasCalled = true;
                 }
             };
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            chooseScriptSources.SelectedFiles = new List<string>() { tempFileName };
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ChooseScriptSources_GivenincludeFile_ShouldaddFileToMsg()
        {
            //---------------Set up test pack-------------------
            var scriptChooser = new ScriptChooser();
            var tempFileName = Path.GetTempFileName();
            var chooseScriptSources = scriptChooser.ChooseScriptSources(tempFileName);

            bool wasCalled = false;
            chooseScriptSources.PropertyChanged += (sender, args) =>
             {
                 if(args.PropertyName == "SelectedFiles")
                 {
                     wasCalled = true;
                 }
             };
            chooseScriptSources.SelectedFiles = new List<string>() { tempFileName };
            //---------------Assert Precondition----------------
            Assert.IsTrue(wasCalled);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(1, chooseScriptSources.SelectedFiles.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ChooseScriptSources_GivenNoincludeFile_ShouldNotaddFileToMsg()
        {
            //---------------Set up test pack-------------------
            var scriptChooser = new ScriptChooser();
            var tempFileName = Path.GetTempFileName();
            var chooseScriptSources = scriptChooser.ChooseScriptSources(string.Empty);

            bool wasCalled = false;
            chooseScriptSources.PropertyChanged += (sender, args) =>
             {
                 if(args.PropertyName == "SelectedFiles")
                 {
                     wasCalled = true;
                 }
             };
            chooseScriptSources.SelectedFiles = new List<string>() {  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(wasCalled);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(0, chooseScriptSources.SelectedFiles.Count());
        }


    }
}
