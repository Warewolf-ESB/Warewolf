using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Statements;
using System.Windows;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class MergeToolModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_MergeDescription_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "MergeDescription")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(null,model.MergeDescription);
            model.MergeDescription = "MergeDescription";
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_NodeArmDescription_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "NodeArmDescription")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(null, model.MergeDescription);
            model.NodeArmDescription = "NodeArmDescription";
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FlowNode_NodeArmDescription_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "FlowNode")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(null, model.MergeDescription);
            model.FlowNode = new FlowStep();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_MergeIcon_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "MergeIcon")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNull(model.MergeIcon);
            model.MergeIcon = null;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_IsMergeChecked_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            var eventCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsChecked")
                {
                    wasCalled = true;
                }
            };
            model.NotifyToolModelChanged += (_) =>
            {
                eventCalled = true;
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsFalse(model.IsChecked);
            model.IsChecked = true;
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(eventCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_IsMergeVisible_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsMergeVisible")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsFalse(model.IsMergeVisible);
            model.IsMergeVisible = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_UniqueId_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "UniqueId")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsFalse(model.IsMergeVisible);
            model.UniqueId =new Guid();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_NodeLocation_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "NodeLocation")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsFalse(model.IsMergeVisible);
            var point = new Point();
            model.NodeLocation = point;
            Assert.IsFalse(wasCalled);
            Assert.IsNull(model.ModelItem);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_FlowNode_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "FlowNode")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsFalse(model.IsMergeVisible);
            model.ModelItem = ModelItemUtils.CreateModelItem(new object());
            Assert.IsFalse(wasCalled);
            Assert.IsNotNull(model.ModelItem);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_ActivityDesignerViewModel_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new ToolModelConflictItem();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "ActivityDesignerViewModel")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsFalse(model.IsMergeVisible);
            model.ActivityDesignerViewModel = null ;
            Assert.IsFalse(wasCalled);
            Assert.IsNull(model.ActivityDesignerViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_AutoProperties_tests()
        {
            //------------Setup for test--------------------------
            var model = new ToolModelConflictItem();
            //------------Execute Test---------------------------
            Assert.AreEqual(default(Point), model.NodeLocation);
            Assert.AreEqual(default(FlowNode), model.FlowNode);
            Assert.AreEqual(default(Guid), model.UniqueId);
            Assert.AreEqual(default(string), model.NodeArmDescription);
            //------------Assert Results-------------------------
            model.NodeLocation = new Point(1, 1);
            model.FlowNode = new FlowStep();
            model.UniqueId = Guid.NewGuid();
            model.NodeArmDescription = "";
            Assert.AreNotEqual(default(Point), model.NodeLocation);
            Assert.AreNotEqual(default(FlowNode), model.FlowNode);
            Assert.AreNotEqual(default(Guid), model.UniqueId);
            Assert.AreNotEqual(default(string), model.NodeArmDescription);
        }

        [TestMethod]
        public void AddActivity_Given_MergeToolModel_verifyCall()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mergeToolModel = new ToolModelConflictItem
            {
                UniqueId = id,
                IsChecked = true,
                ModelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity())
            };
            object sender = mergeToolModel;
            IToolModelConflictItem args = new ToolModelConflictItem();
            var wfDesignerVm = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            wfDesignerVm.Setup(p => p.AddItem(It.IsAny<IToolModelConflictItem>()));

            var methodToRun = typeof(ToolModelConflictItem).GetMethod("AddActivity", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodToRun);
            //---------------Execute Test ----------------------
            var aaaa = methodToRun.Invoke(mergeToolModel, new object[] { });
            //---------------Test Result -----------------------
            wfDesignerVm.Verify(p => p.AddItem(It.IsAny<IToolModelConflictItem>()), Times.Exactly(1));
        }
    }
}
