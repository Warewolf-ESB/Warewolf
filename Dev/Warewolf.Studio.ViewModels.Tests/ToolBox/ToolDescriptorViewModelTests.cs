using System;
using System.Activities.Presentation;
using System.Activities.Statements;
using System.IO;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Studio.ViewModels.ToolBox.Tests
{
    [TestClass]
    public class ToolDescriptorViewModelTests
    {
        #region Fields

        private Mock<IToolDescriptor> _toolMock;

        private Mock<IWarewolfType> _warewolfTypeMock;

        private ToolDescriptorViewModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _toolMock = new Mock<IToolDescriptor>();
            _warewolfTypeMock = new Mock<IWarewolfType>();
            _warewolfTypeMock.SetupGet(it => it.FullyQualifiedName).Returns("");
            _toolMock.SetupGet(it => it.Activity).Returns(_warewolfTypeMock.Object);
            _target = new ToolDescriptorViewModel(_toolMock.Object, false);
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullLocalModel()
        {
            new ToolDescriptorViewModel(null, false);
        }

        #endregion Test construction

        #region Test properties

        [TestMethod]
        public void TestTool()
        {
            //act
            var value = _target.Tool;

            //assert
            Assert.AreSame(_toolMock.Object, value);
        }

        [TestMethod]
        public void TestName()
        {
            //arrange
            var expectedName = "someName";
            _toolMock.SetupGet(it => it.Name).Returns(expectedName);

            //act
            var value = _target.Name;

            //assert
            Assert.AreEqual(expectedName, value);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void TestIcon()
        {
            //arrange
            _toolMock.SetupGet(it => it.Icon).Returns("icon");
            _toolMock.SetupGet(it => it.IconUri).Returns("iconUri");

            //act
            var img = _target.Icon;
        }

        [TestMethod]
        public void TestDesigner()
        {
            //arrange
            var expectedDesignerMock = new Mock<IWarewolfType>();
            _toolMock.SetupGet(it => it.Designer).Returns(expectedDesignerMock.Object);

            //act
            var value = _target.Designer;

            //assert
            Assert.AreEqual(expectedDesignerMock.Object, value);
        }

        [TestMethod]
        public void TestActivity()
        {
            //act
            var value = _target.Activity;

            //assert
            Assert.AreEqual(_warewolfTypeMock.Object, value);
        }

        [TestMethod]
        public void TestActivityTypeNull()
        {
            //act
            var value = _target.ActivityType;

            //assert
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TestActivityType()
        {
            //arrange
            _warewolfTypeMock.SetupGet(it => it.FullyQualifiedName).Returns(typeof(DsfActivity).FullName);
            _target = new ToolDescriptorViewModel(_toolMock.Object, false);

            //act
            var value = _target.ActivityType;

            //assert
            var formats = value.GetFormats();
            Assert.IsNotNull(formats);
            Assert.AreEqual(DragDropHelper.WorkflowItemTypeNameFormat, formats[0]);
            var name = typeof(DsfActivity).AssemblyQualifiedName;
            Assert.AreEqual(name, value.GetData(formats[0]));
        }

        [TestMethod]
        public void TestActivityTypeFlowDecision()
        {
            //arrange
            _warewolfTypeMock.SetupGet(it => it.FullyQualifiedName).Returns(typeof(DsfFlowDecisionActivity).FullName);
            _target = new ToolDescriptorViewModel(_toolMock.Object, false);

            //act
            var value = _target.ActivityType;

            //assert
            var formats = value.GetFormats();
            Assert.IsNotNull(formats);
            Assert.AreEqual(DragDropHelper.WorkflowItemTypeNameFormat, formats[0]);
            var name = typeof(FlowDecision).AssemblyQualifiedName;
            Assert.AreEqual(name, value.GetData(formats[0]));
        }

        [TestMethod]
        public void TestActivityTypeFlowSwitchString()
        {
            //arrange
            _warewolfTypeMock.SetupGet(it => it.FullyQualifiedName).Returns(typeof(DsfFlowSwitchActivity).FullName);
            _target = new ToolDescriptorViewModel(_toolMock.Object, false);

            //act
            var value = _target.ActivityType;

            //assert
            var formats = value.GetFormats();
            Assert.IsNotNull(formats);
            Assert.AreEqual(DragDropHelper.WorkflowItemTypeNameFormat, formats[0]);
            var name = typeof(FlowSwitch<string>).AssemblyQualifiedName;
            Assert.AreEqual(name, value.GetData(formats[0]));
        }

        [TestMethod]
        public void TestIsEnabledfalse()
        {
            //act
            var value = _target.IsEnabled;

            //assert
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void TestIsEnabledtrue()
        {
            //arrange
            _target = new ToolDescriptorViewModel(_toolMock.Object, true);

            //act
            var value = _target.IsEnabled;

            //assert
            Assert.IsTrue(value);
        }

        #endregion Test properties
    }
}