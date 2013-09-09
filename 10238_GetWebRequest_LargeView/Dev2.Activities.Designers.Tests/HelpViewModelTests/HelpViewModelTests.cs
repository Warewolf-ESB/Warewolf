using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Adorners;
using Dev2.Activities.Help;
using Dev2.Providers.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.HelpViewModelTests
{
    [TestClass]
    public class HelpViewModelTests
    {
        [TestMethod]
        [TestCategory("HelpViewModel_Constructor")]
        [Description("Help ViewModel initializes correctly")]
        [Owner("Ashley Lewis")]
        public void HelpViewModel_Constructor_Initialized()
        {
            //init
            const string Expected = "Some Testing Help Text";

            //exe
            var vm = new HelpViewModel { HelpText = Expected };
            vm.HelpText = Expected;

            //assert
            Assert.IsInstanceOfType(vm, typeof(HelpViewModel), "Help activity view model cannot initialize");
            Assert.AreEqual(Expected, vm.HelpText, "Help view model cannot initialize help text");
            Assert.IsNotNull(vm.OpenHyperlinkCommand);
        }

        [TestMethod]
        [TestCategory("HelpViewModel_GetView")]
        [Owner("Hagashen Naidu")]
        public void HelpViewModel_GetView_ReturnsHelpView()
        {
            //--------------------------Setup---------------------------------------
            var vm = new HelpViewModel();
            //--------------------------Execute-----------------------------------
            var view = vm.GetView();
            //--------------------------Assert-------------------------------------
            Assert.IsNotNull(view);
            Assert.IsInstanceOfType(view, typeof(HelpView), "Help view model cannot get help view");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("HelpViewModel_Errors")]
        public void HelpViewModel_Errors_Property_ReturnsErrors()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel();
            var errors = new List<IActionableErrorInfo>();
            //------------Execute Test---------------------------
            helpViewModel.Errors = errors;
            //------------Assert Results-------------------------
            Assert.IsNotNull(helpViewModel.Errors);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("HelpViewModel_Errors")]
        public void HelpViewModel_Errors_SetProperty_FiresPropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel();
            bool _wasCalled = false;
            helpViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "Errors")
                {
                    _wasCalled = true;
                }
            };
            var errors = new List<IActionableErrorInfo>();
            //------------Execute Test---------------------------
            helpViewModel.Errors = errors;
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpViewModel_OpenHyperlinkCommand")]
        public void HelpViewModel_OpenHyperlinkCommand_WithActionableError_InvokesDo()
        {
            //------------Setup for test--------------------------
            var error = new Mock<IActionableErrorInfo>();
            error.Setup(e => e.Do()).Verifiable();

            var helpViewModel = new HelpViewModel();

            //------------Execute Test---------------------------
            helpViewModel.OpenHyperlinkCommand.Execute(error.Object);

            //------------Assert Results-------------------------
            error.Verify(e => e.Do());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpViewModel_OpenHyperlinkCommand")]
        public void HelpViewModel_OpenHyperlinkCommand_WithNull_DoesNotThrowException()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel();

            //------------Execute Test---------------------------
            helpViewModel.OpenHyperlinkCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(true);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpViewModel_DisplayHelpText")]
        public void HelpViewModel_DisplayHelpText_WithErrors_Hidden()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel { Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() } };

            //------------Execute Test---------------------------
            var actual = helpViewModel.DisplayHelpText;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Hidden, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpViewModel_DisplayHelpText")]
        public void HelpViewModel_DisplayHelpText_WithoutErrors_Visible()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel();

            //------------Execute Test---------------------------
            var actual = helpViewModel.DisplayHelpText;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, actual);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpViewModel_DisplayErrors")]
        public void HelpViewModel_DisplayErrors_WithErrors_Visible()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel { Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() } };

            //------------Execute Test---------------------------
            var actual = helpViewModel.DisplayErrors;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpViewModel_DisplayErrors")]
        public void HelpViewModel_DisplayErrors_WithoutErrors_Hidden()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel();

            //------------Execute Test---------------------------
            var actual = helpViewModel.DisplayErrors;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Hidden, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpViewModel_ResetProperties")]
        public void HelpViewModel_ResetProperties_PropertiesCleared()
        {
            //------------Setup for test--------------------------
            var helpViewModel = new HelpViewModel
            {
                HelpText = "help text",
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() }
            };
            
            //------------Execute Test---------------------------
            helpViewModel.ResetProperties();

            //------------Assert Results-------------------------
            Assert.IsNotNull(helpViewModel.Errors);
            Assert.AreEqual(0, helpViewModel.Errors.Count);
            Assert.AreEqual(string.Empty, helpViewModel.HelpText);
        }
    }
}
