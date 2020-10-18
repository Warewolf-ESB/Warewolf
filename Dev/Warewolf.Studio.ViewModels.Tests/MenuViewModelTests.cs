/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Help;
using Dev2.Security;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class MenuViewModelTests
    {
        #region Fields

        Mock<IShellViewModel> _mainViewModelMock;
        AuthorizeCommand<string> _newCommand;
        Mock<ICommand> _deployCommandMock;
        AuthorizeCommand _saveCommand;
        AuthorizeCommand _openSchedulerCommand;
        AuthorizeCommand _openTasksCommand;
        AuthorizeCommand _openSearchCommand;
        AuthorizeCommand _openSettingsCommand;
        AuthorizeCommand _executeServiceCommand;
        Mock<ICommand> _startPageCommandMock;
        List<string> _changedProperties;

        MenuViewModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _mainViewModelMock = new Mock<IShellViewModel>();
            _deployCommandMock = new Mock<ICommand>();
            _newCommand = new AuthorizeCommand<string>(new AuthorizationContext(), str => { }, str => true);
            _saveCommand = new AuthorizeCommand(new AuthorizationContext(), obj => { }, obj => true);
            _openSearchCommand = new AuthorizeCommand(new AuthorizationContext(), obj => { }, obj => true);
            _openSchedulerCommand = new AuthorizeCommand(new AuthorizationContext(), obj => { }, obj => true);
            _openTasksCommand = new AuthorizeCommand(new AuthorizationContext(), obj => { }, obj => true);
            _openSettingsCommand = new AuthorizeCommand(new AuthorizationContext(), obj => { }, obj => true);
            _executeServiceCommand = new AuthorizeCommand(new AuthorizationContext(), obj => { }, obj => true);
            _startPageCommandMock = new Mock<ICommand>();

            _mainViewModelMock.Setup(it => it.CheckForNewVersionAsync()).ReturnsAsync(true);
            _mainViewModelMock.SetupGet(it => it.NewServiceCommand).Returns(_newCommand);
            _mainViewModelMock.SetupGet(it => it.DeployCommand).Returns(_deployCommandMock.Object);
            _mainViewModelMock.SetupGet(it => it.SaveCommand).Returns(_saveCommand);
            _mainViewModelMock.SetupGet(it => it.SearchCommand).Returns(_openSearchCommand);
            _mainViewModelMock.SetupGet(it => it.SchedulerCommand).Returns(_openSchedulerCommand);
            _mainViewModelMock.SetupGet(it => it.TasksCommand).Returns(_openTasksCommand);
            _mainViewModelMock.SetupGet(it => it.SettingsCommand).Returns(_openSettingsCommand);
            _mainViewModelMock.SetupGet(it => it.DebugCommand).Returns(_executeServiceCommand);
            _mainViewModelMock.SetupGet(it => it.ShowStartPageCommand).Returns(_startPageCommandMock.Object);

            _target = new MenuViewModel(_mainViewModelMock.Object);

            _changedProperties = new List<string>();

            _target.PropertyChanged += _target_PropertyChanged;
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [Timeout(100)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullMainViewModel()
        { 
            new MenuViewModel(null);
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        [Timeout(100)]
        [Owner("Pieter Terblanche")]
        [TestCategory("MenuViewModel_ShowStartPage")]
        public void MenuViewModel_ShowStartPage_Execute_Result()
        {
            //------------Setup for test--------------------------
            var call = false;
            var x = new DelegateCommand(() => { call = true; });
            _mainViewModelMock.Setup(a => a.ShowStartPageCommand).Returns(x);
            _target = new MenuViewModel(_mainViewModelMock.Object);

            //------------Execute Test---------------------------
            _target.StartPageCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(call);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestNewCommand()
        {
            Assert.AreSame(_newCommand, _target.NewServiceCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDeployCommand()
        {
            Assert.AreSame(_deployCommandMock.Object, _target.DeployCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSaveCommand()
        {
            Assert.AreSame(_saveCommand, _target.SaveCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestOpenSearchCommand()
        {
            Assert.AreSame(_openSearchCommand, _target.OpenSearchCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestOpenSchedulerCommand()
        {
            Assert.AreSame(_openSchedulerCommand, _target.OpenSchedulerCommand);
        }

        [TestMethod, Timeout(60000)]
        public void TestOpenTasksCommand()
        {
            Assert.AreSame(_openTasksCommand, _target.OpenTasksCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestOpenSettingsCommand()
        {
            Assert.AreSame(_openSettingsCommand, _target.OpenSettingsCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestExecuteServiceCommand()
        {
            Assert.AreSame(_executeServiceCommand, _target.ExecuteServiceCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestStartPageCommand()
        {
            Assert.AreSame(_startPageCommandMock.Object, _target.StartPageCommand);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestCheckForNewVersionCommand()
        {
            //act
            _target.CheckForNewVersionCommand.Execute(null);
            Assert.IsTrue(_target.CheckForNewVersionCommand.CanExecute(null));

            //assert
            _mainViewModelMock.Verify(it => it.DisplayDialogForNewVersion());
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSupportCommand()
        {
            Assert.IsTrue(_target.SupportCommand.CanExecute(null));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockCommand()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.LockCommand.Execute(null);
            Assert.IsTrue(_target.LockCommand.CanExecute(null));

            //assert
            this.VerifyUpdateProperties();
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideOpenCommand_isOverLockfalseIsPanelLockedOpenfalse()
        {
            //arrange
            _target.ButtonWidth = 3;
            _target.IsPanelOpen = false;
            _target.IsPanelLockedOpen = false;
            _target.IsNotOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideOpenCommand.Execute(null);
            Assert.IsTrue(_target.SlideOpenCommand.CanExecute(null));

            //assert
            Assert.IsFalse(_target.IsPanelOpen);
            Assert.AreEqual(3, _target.ButtonWidth);
            Assert.IsFalse(_changedProperties.Any());
            _mainViewModelMock.VerifySet(it=>it.MenuExpanded = It.IsAny<bool>(), Times.Never);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideOpenCommand_isOverLockfalseIsPanelLockedOpentrue()
        {
            //arrange
            _target.ButtonWidth = 3;
            _target.IsPanelOpen = false;
            _target.IsPanelLockedOpen = true;
            _target.IsNotOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideOpenCommand.Execute(null);
            Assert.IsTrue(_target.SlideOpenCommand.CanExecute(null));

            //assert
            Assert.IsTrue(_target.IsPanelOpen);
            Assert.AreEqual(125, _target.ButtonWidth);
            VerifyUpdateProperties();
            _mainViewModelMock.VerifySet(it => it.MenuExpanded = true);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideOpenCommand_isOverLocktrue()
        {
            //arrange
            _target.ButtonWidth = 3;
            _target.IsPanelOpen = false;
            _target.IsPanelLockedOpen = false;
            _target.IsOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideOpenCommand.Execute(null);
            Assert.IsTrue(_target.SlideOpenCommand.CanExecute(null));

            //assert
            Assert.IsFalse(_target.IsPanelOpen);
            Assert.AreEqual(3, _target.ButtonWidth);
            Assert.IsFalse(_changedProperties.Any());
            _mainViewModelMock.VerifySet(it => it.MenuExpanded = It.IsAny<bool>(), Times.Never);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideClosedCommand_isMenuPanelWidthLess80OverLocktrue()
        {
            //arrange
            _mainViewModelMock.SetupGet(it => it.MenuPanelWidth).Returns(23);
            _target.IsOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideClosedCommand.Execute(null);
            Assert.IsTrue(_target.SlideClosedCommand.CanExecute(null));

            //assert
            Assert.IsFalse(_changedProperties.Any());
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideClosedCommand_isOverLocktrue()
        {
            //arrange
            _mainViewModelMock.SetupGet(it => it.MenuPanelWidth).Returns(81);
            _target.IsOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideClosedCommand.Execute(null);
            Assert.IsTrue(_target.SlideClosedCommand.CanExecute(null));

            //assert
            Assert.IsFalse(_changedProperties.Any());
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideClosedCommand()
        {
            //arrange
            _target.IsPanelLockedOpen = false;
            _target.IsPanelOpen = false;
            _target.ButtonWidth = 3;
            _mainViewModelMock.SetupGet(it => it.MenuPanelWidth).Returns(81);
            _target.IsNotOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideClosedCommand.Execute(null);
            Assert.IsTrue(_target.SlideClosedCommand.CanExecute(null));

            //assert
            VerifyUpdateProperties();
            Assert.AreEqual(3,_target.ButtonWidth);
            Assert.IsFalse(_target.IsPanelOpen);
            _mainViewModelMock.VerifySet(it=>it.MenuExpanded = It.IsAny<bool>(), Times.Never);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideClosedCommandIsPanelOpenFalse()
        {
            //arrange
            _target.IsPanelLockedOpen = true;
            _target.IsPanelOpen = false;
            _mainViewModelMock.SetupGet(it => it.MenuPanelWidth).Returns(81);
            _target.IsNotOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideClosedCommand.Execute(null);
            Assert.IsTrue(_target.SlideClosedCommand.CanExecute(null));

            //assert
            VerifyUpdateProperties();
            _mainViewModelMock.VerifySet(it=>it.MenuExpanded = true);
            Assert.AreEqual(125, _target.ButtonWidth);
            Assert.IsTrue(_target.IsPanelOpen);
        }

        [TestMethod, Timeout(60000)]
        public void TestSlideClosedCommandIsPopoutViewOpenTrue()
        {
            //arrange
            Assert.IsFalse(_target.IsPopoutViewOpen);
            _target.IsPopoutViewOpen = true;
            _mainViewModelMock.SetupGet(it => it.MenuPanelWidth).Returns(81);
            _target.IsNotOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideClosedCommand.Execute(null);
            Assert.IsTrue(_target.SlideClosedCommand.CanExecute(null));

            //assert
            Assert.AreEqual(125, _target.ButtonWidth);
            Assert.IsTrue(_target.IsPanelOpen);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSlideClosedCommandIsPanelOpenTrue()
        {
            //arrange
            _target.IsPanelLockedOpen = true;
            _target.IsPanelOpen = true;
            _mainViewModelMock.SetupGet(it => it.MenuPanelWidth).Returns(81);
            _target.IsNotOverLockCommand.Execute(null);
            _changedProperties.Clear();

            //act
            _target.SlideClosedCommand.Execute(null);
            Assert.IsTrue(_target.SlideClosedCommand.CanExecute(null));

            //assert
            VerifyUpdateProperties();
            _mainViewModelMock.VerifySet(it => it.MenuExpanded = false);
            Assert.AreEqual(125, _target.ButtonWidth);
            Assert.IsFalse(_target.IsPanelOpen);
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestButtonWidth_Default()
        {
            Assert.AreEqual(125, _target.ButtonWidth);
        }
        [TestMethod]
        [Timeout(100)]
        public void TestIsPanelOpen_Default()
        {
            Assert.IsTrue(_target.IsPanelOpen);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsPanelLockedOpen_Default()
        {
            Assert.IsTrue(_target.IsPanelLockedOpen);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDebugIcon_Default()
        {
            Assert.AreEqual(FontAwesome.WPF.FontAwesomeIcon.Bug, _target.DebugIcon);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDebugIcon()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.DebugIcon = FontAwesome.WPF.FontAwesomeIcon.ShoppingCart;
            var value = _target.DebugIcon;

            //assert
            Assert.AreEqual(FontAwesome.WPF.FontAwesomeIcon.ShoppingCart, value);
            Assert.IsTrue(_changedProperties.Contains("DebugIcon"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestHasNewVersion_Default()
        {
            Assert.IsTrue(_target.HasNewVersion);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockImage()
        {
            //arrange
            _target.IsPanelLockedOpen = false;

            //act
            var value = _target.LockImage;

            //assert
            Assert.AreEqual("Lock", value);

        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockImageIsPanelLockedOpentrue()
        {
            //arrange
            _target.IsPanelLockedOpen = true;

            //act
            var value = _target.LockImage;

            //assert
            Assert.AreEqual("UnlockAlt", value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsPanelLockedOpen()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.IsPanelLockedOpen = false;
            var value = _target.IsPanelLockedOpen;

            //assert
            Assert.IsFalse(value);
            Assert.IsTrue(_changedProperties.Contains("LockLabel"));
            Assert.IsTrue(_changedProperties.Contains("LockImage"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestNewLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.NewLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestNewLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.NewLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TesDebugLabelNotNullOrEmptyIsProcessingFalse()
        {
            //arrange
            _target.ButtonWidth = 125;
            _target.IsProcessing = false;

            //act
            var value = _target.DebugLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TesDebugLabelNotNullOrEmptyIsProcessingTrue()
        {
            //arrange
            _target.ButtonWidth = 125;
            _target.IsProcessing = true;

            //act
            var value = _target.DebugLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TesDebugLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 3;

            //act
            var value = _target.DebugLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSaveLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.SaveLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSaveLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.SaveLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDeployLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.DeployLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDeployLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.DeployLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestTaskLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.TaskLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestTaskLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.TaskLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod, Timeout(60000)]
        public void TestSchedulerLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.SchedulerLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod, Timeout(60000)]
        public void TestSchedulerLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.SchedulerLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }



        [TestMethod, Timeout(60000)]
        public void TestQueueEventsLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.QueueEventsLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod, Timeout(60000)]
        public void TestQueueEventsLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.QueueEventsLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSettingsLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.SettingsLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSettingsLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.SettingsLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }
        [TestMethod]
        [Timeout(100)]
        public void TestSupportLabelNotNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 125;

            //act
            var value = _target.SupportLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSupportLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.SupportLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestNewVersionLabelNullOrEmpty()
        {
            //arrange
            _target.ButtonWidth = 1;

            //act
            var value = _target.NewVersionLabel;

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsProcessingTrue()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.IsProcessing = true;
            var value = _target.IsProcessing;

            //assert
            Assert.IsTrue(value);
            Assert.IsTrue(_changedProperties.Contains("DebugLabel"));
            Assert.IsTrue(_changedProperties.Contains("IsProcessing"));
            Assert.AreEqual(FontAwesome.WPF.FontAwesomeIcon.Stop, _target.DebugIcon);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsProcessingFalse()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.IsProcessing = false;
            var value = _target.IsProcessing;
            //assert
            Assert.IsFalse(value);
            Assert.IsTrue(_changedProperties.Contains("DebugLabel"));
            Assert.AreEqual(FontAwesome.WPF.FontAwesomeIcon.Bug, _target.DebugIcon);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockLabelIsPanelLockedOpenTrue()
        {
            //arrange
            _target.IsPanelLockedOpen = true;

            //act
            var value = _target.LockLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockLabelIsPanelLockedOpenFalse()
        {
            //arrange
            _target.IsPanelLockedOpen = false;

            //act
            var value = _target.LockLabel;

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDataContext()
        {
            //arrange
            var expectedValue = new object();

            //act
            _target.DataContext = expectedValue;
            var value = _target.DataContext;

            //assert
            Assert.AreSame(expectedValue, value);
        }

        #endregion Test properties

        #region Test methods 

        [TestMethod]
        [Timeout(250)]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var helpViewModel = new Mock<IHelpWindowViewModel>();
            _mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModel.Object);
            var helpText = "someText";

            //act
            _target.UpdateHelpDescriptor(helpText);

            //assert
            helpViewModel.Verify(it=>it.UpdateHelpText(helpText));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockIsPanelLockedOpenfalse()
        {
            //arrange
            _target.IsPanelLockedOpen = false;
            _changedProperties.Clear();

            //act
            _target.Lock();

            //assert
            Assert.IsTrue(_target.IsPanelLockedOpen);
            VerifyUpdateProperties();
        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockIsPanelLockedOpentrueButtonWidth125()
        {
            //arrange
            _target.IsPanelOpen = false;
            _target.ButtonWidth = 125;
            _target.IsPanelLockedOpen = true;
            _changedProperties.Clear();

            //act
            _target.Lock();

            //assert
            Assert.IsFalse(_target.IsPanelLockedOpen);
            Assert.AreEqual(35, _target.ButtonWidth);
            VerifyUpdateProperties();
        }

        [TestMethod]
        [Timeout(100)]
        public void TestLockIsPanelLockedOpentrueButtonWidth35()
        {
            //arrange
            _target.IsPanelOpen = true;
            _target.ButtonWidth = 35;
            _target.IsPanelLockedOpen = true;
            _changedProperties.Clear();

            //act
            _target.Lock();

            //assert
            Assert.IsFalse(_target.IsPanelLockedOpen);
            Assert.AreEqual(125, _target.ButtonWidth);
            VerifyUpdateProperties();
        }

        #endregion Test methods 

        #region Private helper methods

        void _target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _changedProperties.Add(e.PropertyName);
        }

        void VerifyUpdateProperties()
        {
            Assert.IsTrue(_changedProperties.Contains("NewLabel"));
            Assert.IsTrue(_changedProperties.Contains("SaveLabel"));
            Assert.IsTrue(_changedProperties.Contains("DeployLabel"));
            Assert.IsTrue(_changedProperties.Contains("TaskLabel"));
            Assert.IsTrue(_changedProperties.Contains("DebugLabel"));
            Assert.IsTrue(_changedProperties.Contains("SettingsLabel"));
            Assert.IsTrue(_changedProperties.Contains("SupportLabel"));
            Assert.IsTrue(_changedProperties.Contains("NewVersionLabel"));
            Assert.IsTrue(_changedProperties.Contains("LockLabel"));
            Assert.IsTrue(_changedProperties.Contains("ButtonWidth"));
        }

        #endregion Private helper methods
    }
}