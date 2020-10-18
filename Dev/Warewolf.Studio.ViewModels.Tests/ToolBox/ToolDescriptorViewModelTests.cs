using System;
using System.Activities.Presentation;
using System.Activities.Statements;
using System.IO;
using System.Windows;
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

        Mock<IToolDescriptor> _toolMock;

        Mock<IWarewolfType> _warewolfTypeMock;

        ToolDescriptorViewModel _target;

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
        [Timeout(100)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullLocalModel()
        {
            new ToolDescriptorViewModel(null, false);
        }

        #endregion Test construction

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestTool()
        {
            //act
            var value = _target.Tool;

            //assert
            Assert.AreSame(_toolMock.Object, value);
        }

        [TestMethod]
        [Timeout(100)]
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
        [Timeout(250)]
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
        [Timeout(100)]
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
        [Timeout(100)]
        public void TestActivity()
        {
            //act
            var value = _target.Activity;

            //assert
            Assert.AreEqual(_warewolfTypeMock.Object, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestActivityTypeNull()
        {
            //act
            var value = _target.ActivityType;

            //assert
            Assert.IsNull(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestActivityType()
        {
            //arrange
            _warewolfTypeMock.SetupGet(it => it.FullyQualifiedName).Returns(typeof(DsfActivity).FullName);
            _target = new ToolDescriptorViewModel(_toolMock.Object, false);

            //act
            var value = _target.ActivityType as DataObject;

            //assert
            var formats = value.GetFormats();
            Assert.IsNotNull(formats);
            Assert.AreEqual(DragDropHelper.WorkflowItemTypeNameFormat, formats[0]);
            var name = typeof(DsfActivity).AssemblyQualifiedName;
            Assert.AreEqual(name, value.GetData(formats[0]));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestActivityTypeFlowDecision()
        {
            //arrange
            _warewolfTypeMock.SetupGet(it => it.FullyQualifiedName).Returns(typeof(DsfFlowDecisionActivity).FullName);
            _target = new ToolDescriptorViewModel(_toolMock.Object, false);

            //act
            var value = _target.ActivityType as DataObject;

            //assert
            var formats = value.GetFormats();
            Assert.IsNotNull(formats);
            Assert.AreEqual(DragDropHelper.WorkflowItemTypeNameFormat, formats[0]);
            var name = typeof(FlowDecision).AssemblyQualifiedName;
            Assert.AreEqual(name, value.GetData(formats[0]));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestActivityTypeFlowSwitchString()
        {
            //arrange
            _warewolfTypeMock.SetupGet(it => it.FullyQualifiedName).Returns(typeof(DsfFlowSwitchActivity).FullName);
            _target = new ToolDescriptorViewModel(_toolMock.Object, false);

            //act
            var value = _target.ActivityType as DataObject;

            //assert
            var formats = value.GetFormats();
            Assert.IsNotNull(formats);
            Assert.AreEqual(DragDropHelper.WorkflowItemTypeNameFormat, formats[0]);
            var name = typeof(FlowSwitch<string>).AssemblyQualifiedName;
            Assert.AreEqual(name, value.GetData(formats[0]));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsEnabledfalse()
        {
            //act
            var value = _target.IsEnabled;

            //assert
            Assert.IsFalse(value);
        }

        [TestMethod]
        [Timeout(100)]
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