using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable PossibleUnintendedReferenceComparison

namespace Warewolf.Studio.ViewModels.ToolBox.Tests
{
    [TestClass]
    public class ToolboxViewModelTests
    {
        #region Fields

        private Mock<IToolboxModel> _localModelMock;
        private Mock<IToolboxModel> _remoteModelMock;

        private List<string> _changedProperties;

        private ToolboxViewModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _localModelMock = new Mock<IToolboxModel>();
            _remoteModelMock = new Mock<IToolboxModel>();
            _remoteModelMock.Setup(it => it.GetTools()).Returns(new List<IToolDescriptor>());
            _target = new ToolboxViewModel(_localModelMock.Object, _remoteModelMock.Object);

            _changedProperties = new List<string>();

            _target.PropertyChanged += _target_PropertyChanged;
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullLocalModel()
        {
            new ToolboxViewModel(null, _remoteModelMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullRemoteModel()
        {
            new ToolboxViewModel(_localModelMock.Object, null);
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        public void TestClearFilterCommand()
        {
            //act
            _target.ClearFilterCommand.Execute(null);
            Assert.IsTrue(_target.ClearFilterCommand.CanExecute(null));

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(_target.SearchTerm));
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestCategorisedTools()
        {
            //arrange
            var activityMock = new Mock<IWarewolfType>();
            var toolDescriptorMock1 = new Mock<IToolDescriptor>();
            toolDescriptorMock1.SetupGet(it => it.Name).Returns("someText1");
            toolDescriptorMock1.SetupGet(it => it.Category).Returns("someCat");
            toolDescriptorMock1.SetupGet(it => it.Activity).Returns(activityMock.Object);

            var toolDescriptorMock2 = new Mock<IToolDescriptor>();
            toolDescriptorMock2.SetupGet(it => it.Name).Returns("someText2");
            toolDescriptorMock2.SetupGet(it => it.Category).Returns("someCat");
            toolDescriptorMock2.SetupGet(it => it.Activity).Returns(activityMock.Object);

            var toolDescriptorMock3 = new Mock<IToolDescriptor>();
            toolDescriptorMock3.SetupGet(it => it.Name).Returns("someOtherText");
            toolDescriptorMock3.SetupGet(it => it.Category).Returns("someCat2");
            toolDescriptorMock3.SetupGet(it => it.Activity).Returns(activityMock.Object);

            _remoteModelMock.Setup(it => it.GetTools())
                .Returns(
                    new List<IToolDescriptor>()
                        {
                            toolDescriptorMock1.Object,
                            toolDescriptorMock2.Object,
                            toolDescriptorMock3.Object
                        });
            _localModelMock.Setup(it => it.GetTools()).Returns(new List<IToolDescriptor>());
            _target.ClearFilter();

            //act
            var values = _target.CategorisedTools;

            //assert
            Assert.AreEqual(2, values.Count);
            Assert.IsTrue(
                values.Any(
                    it =>
                    it.Name == "someCat" && it.Tools.Count == 2
                    && it.Tools.Any(tool => tool.Tool == toolDescriptorMock1.Object)
                    && it.Tools.Any(tool => tool.Tool == toolDescriptorMock2.Object)));
            Assert.IsTrue(
                values.Any(
                    it =>
                    it.Name == "someCat2" && it.Tools.Count == 1
                    && it.Tools.Any(tool => tool.Tool == toolDescriptorMock3.Object)));
        }

        [TestMethod]
        public void TestIsEnabledIsDesignerFocusedfalse()
        {
            //arrange
            _target.IsDesignerFocused = false;

            //act
            var value = _target.IsEnabled;

            //assert
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void TestIsEnabledLocalRemoteDisabled()
        {
            //arrange
            _target.IsDesignerFocused = true;
            _localModelMock.Setup(it => it.IsEnabled()).Returns(false);
            _remoteModelMock.Setup(it => it.IsEnabled()).Returns(false);

            //act
            var value = _target.IsEnabled;

            //assert
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void TestIsEnabledLocalEnabledRemoteDisabled()
        {
            //arrange
            _target.IsDesignerFocused = true;
            _localModelMock.Setup(it => it.IsEnabled()).Returns(true);
            _remoteModelMock.Setup(it => it.IsEnabled()).Returns(false);

            //act
            var value = _target.IsEnabled;

            //assert
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void TestIsEnabledLocalRemoteEnabled()
        {
            //arrange
            _target.IsDesignerFocused = true;
            _localModelMock.Setup(it => it.IsEnabled()).Returns(true);
            _remoteModelMock.Setup(it => it.IsEnabled()).Returns(true);

            //act
            var value = _target.IsEnabled;

            //assert
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void TestIsDesignerFocused()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.IsDesignerFocused = true;
            var value = _target.IsDesignerFocused;

            //assert
            Assert.IsTrue(value);
            Assert.IsTrue(_changedProperties.Contains("IsEnabled"));
        }

        [TestMethod]
        public void TestSelectedTool()
        {
            //arrange
            _changedProperties.Clear();
            var expectedValueMock = new Mock<IToolDescriptorViewModel>();

            //act
            _target.SelectedTool = expectedValueMock.Object;
            var value = _target.SelectedTool;

            //assert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_changedProperties.Contains("SelectedTool"));
        }

        [TestMethod]
        public void TestSearchTerm()
        {
            //arrange
            _changedProperties.Clear();
            var expectedValue = "someText";

            //act
            _target.SearchTerm = expectedValue;
            var value = _target.SearchTerm;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("SearchTerm"));
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestFilterEmptySearchString()
        {
            //arrange
            _target.SearchTerm = "Some";
            _changedProperties.Clear();

            //act
            _target.Filter("");

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(_target.SearchTerm));
            Assert.IsTrue(_changedProperties.Contains("Tools"));
            Assert.IsTrue(_changedProperties.Contains("CategorisedTools"));
        }

        [TestMethod]
        public void TestFilter()
        {
            //arrange
            var searchString = "someSearchString";

            var toolDescriptorMockContainingInLocal = new Mock<IToolDescriptor>();

            var activityMock = new Mock<IWarewolfType>();
            toolDescriptorMockContainingInLocal.SetupGet(it => it.Name).Returns(searchString);
            toolDescriptorMockContainingInLocal.SetupGet(it => it.Activity).Returns(activityMock.Object);

            var toolDescriptorMockNotContainingInLocal = new Mock<IToolDescriptor>();
            toolDescriptorMockNotContainingInLocal.SetupGet(it => it.Name).Returns(searchString);
            toolDescriptorMockNotContainingInLocal.SetupGet(it => it.Activity).Returns(activityMock.Object);

            var toolDescriptorMockNotMatching = new Mock<IToolDescriptor>();
            toolDescriptorMockNotMatching.SetupGet(it => it.Name).Returns("someOtherText");

            _remoteModelMock.Setup(it => it.GetTools())
                .Returns(
                    new List<IToolDescriptor>()
                        {
                            toolDescriptorMockContainingInLocal.Object,
                            toolDescriptorMockNotContainingInLocal.Object,
                            toolDescriptorMockNotMatching.Object
                        });
            _localModelMock.Setup(it => it.GetTools())
                .Returns(new List<IToolDescriptor>() { toolDescriptorMockContainingInLocal.Object });
            _changedProperties.Clear();

            //act
            _target.Filter(searchString);

            //assert
            Assert.AreEqual(2, _target.Tools.Count);
            Assert.IsTrue(_target.Tools.Any(it => it.Tool == toolDescriptorMockContainingInLocal.Object && it.IsEnabled));
            Assert.IsTrue(_target.Tools.Any(it => it.Tool == toolDescriptorMockNotContainingInLocal.Object && !it.IsEnabled));
            Assert.IsFalse(_target.Tools.Any(it => it.Tool == toolDescriptorMockNotMatching.Object));
            Assert.IsTrue(_changedProperties.Contains("Tools"));
            Assert.IsTrue(_changedProperties.Contains("CategorisedTools"));
        }

        [TestMethod]
        public void TestClearFilter()
        {
            //arrange
            var toolDescriptorMockContainingInLocal = new Mock<IToolDescriptor>();

            var activityMock = new Mock<IWarewolfType>();
            toolDescriptorMockContainingInLocal.SetupGet(it => it.Name).Returns("someText1");
            toolDescriptorMockContainingInLocal.SetupGet(it => it.Activity).Returns(activityMock.Object);

            var toolDescriptorMockNotContainingInLocal = new Mock<IToolDescriptor>();
            toolDescriptorMockNotContainingInLocal.SetupGet(it => it.Name).Returns("someText2");
            toolDescriptorMockNotContainingInLocal.SetupGet(it => it.Activity).Returns(activityMock.Object);

            var toolDescriptorMockNotContainingInLocal2 = new Mock<IToolDescriptor>();
            toolDescriptorMockNotContainingInLocal2.SetupGet(it => it.Name).Returns("someOtherText");
            toolDescriptorMockNotContainingInLocal2.SetupGet(it => it.Activity).Returns(activityMock.Object);

            _remoteModelMock.Setup(it => it.GetTools())
                .Returns(
                    new List<IToolDescriptor>()
                        {
                            toolDescriptorMockContainingInLocal.Object,
                            toolDescriptorMockNotContainingInLocal.Object,
                            toolDescriptorMockNotContainingInLocal2.Object
                        });
            _localModelMock.Setup(it => it.GetTools())
                .Returns(new List<IToolDescriptor>() { toolDescriptorMockContainingInLocal.Object });
            _changedProperties.Clear();

            //act
            _target.ClearFilter();

            //assert
            Assert.AreEqual(3, _target.Tools.Count);
            Assert.IsTrue(_target.Tools.Any(it => it.Tool == toolDescriptorMockContainingInLocal.Object && it.IsEnabled));
            Assert.IsTrue(_target.Tools.Any(it => it.Tool == toolDescriptorMockNotContainingInLocal.Object && !it.IsEnabled));
            Assert.IsTrue(_target.Tools.Any(it => it.Tool == toolDescriptorMockNotContainingInLocal2.Object && !it.IsEnabled));
            Assert.IsTrue(_changedProperties.Contains("Tools"));
            Assert.IsTrue(_changedProperties.Contains("CategorisedTools"));
        }

        [TestMethod]
        public void Test_remoteModel_OnserverDisconnected()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _remoteModelMock.Raise(it => it.OnserverDisconnected += null, _remoteModelMock.Object);

            //assert
            _changedProperties.Contains("IsEnabled");
        }
        
        [TestMethod]
        public void Test_localModel_OnserverDisconnected()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _localModelMock.Raise(it => it.OnserverDisconnected += null, _localModelMock.Object);

            //assert
            _changedProperties.Contains("IsEnabled");
        }

        [TestMethod]
        public void TestDispose()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.Dispose();
        
            //assert
            _localModelMock.Raise(it => it.OnserverDisconnected += null, _localModelMock.Object);
            _remoteModelMock.Raise(it => it.OnserverDisconnected += null, _remoteModelMock.Object);
            Assert.IsFalse(_changedProperties.Any());
        }

        [TestMethod]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mainViewModelMock = new Mock<IMainViewModel>();
            var helpWindowViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpWindowViewModelMock.Object);
            CustomContainer.Register<IMainViewModel>(mainViewModelMock.Object);

            //act
            _target.UpdateHelpDescriptor("someText");

            //assert
            helpWindowViewModelMock.Verify(it => it.UpdateHelpText("someText"));
        }

        #endregion Test methods

        #region Private helper methods

        private void _target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _changedProperties.Add(e.PropertyName);
        }

        #endregion Private helper methods
    }
}