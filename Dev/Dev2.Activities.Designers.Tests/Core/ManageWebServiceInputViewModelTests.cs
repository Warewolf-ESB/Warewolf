using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class ManageWebServiceInputViewModelTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void ManageWebServiceInputViewModel_Ctor()
        {
            ManageWebServiceInputViewModel vm = new ManageWebServiceInputViewModel();
            Assert.IsNotNull(vm.CloseCommand);
            Assert.IsNotNull(vm.PasteResponseCommand);
            Assert.IsNotNull(vm.CloseCommand);


            //------------Assert Results-------------------------
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void ManageWebServiceInputViewModel_TestAction()
        {
            bool called=false;
            bool calledOk=false;
            ManageWebServiceInputViewModel vm = new ManageWebServiceInputViewModel();
            vm.TestAction = () => { called = true; };
            vm.OkAction = () =>
            {
                calledOk = true;
            };
            vm.TestAction();
            vm.OkAction();


            //------------Assert Results-------------------------

            Assert.IsTrue(called);
            Assert.IsTrue(calledOk);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void ManageWebServiceInputViewModel_Properties()
        {

            ManageWebServiceInputViewModel vm = new ManageWebServiceInputViewModel();
            var lst = new List<IServiceInput>();
            vm.Inputs = lst;
            Assert.AreEqual(lst,vm.Inputs);
            var lsto = new List<IServiceOutputMapping>();
            vm.OutputMappings = lsto;
            Assert.AreEqual(lst, vm.Inputs);
            vm.TestResults = "bob";
            Assert.AreEqual("bob",vm.TestResults);
            vm.TestResultsAvailable = true;
            Assert.IsTrue(vm.TestResultsAvailable);
            vm.OkSelected = true;
            Assert.IsTrue(vm.OkSelected);
            vm.IsTestResultsEmptyRows = true;
            Assert.IsTrue(vm.IsTestResultsEmptyRows);
            vm.IsTesting = true;
            Assert.IsTrue(vm.IsTesting);
            vm.PasteResponseVisible = true;
            Assert.IsTrue(vm.PasteResponseVisible);
            vm.PasteResponseAvailable = true;
            Assert.IsTrue(vm.PasteResponseAvailable);
            var b = new WebServiceDefinition() { Headers = new List<NameValue>() { new NameValue("a", "b") } };
            vm.Model = b;
            Assert.IsNotNull(vm.Model);

        }




    }
}
