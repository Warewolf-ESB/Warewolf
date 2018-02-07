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
        public void MergeToolModel_Constructor_DefaultConstruction_ShouldHaveChildren()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Children);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_MergeDescription_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
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
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
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
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
            Assert.AreEqual(null, model.MergeDescription);
            model.FlowNode = new FlowStep();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_ParentDescription_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "ParentDescription")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Children);
            Assert.AreEqual(null, model.ParentDescription);
            model.ParentDescription = "ParentDescription";
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_MergeIcon_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
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
            var model = new MergeToolModel
            {
                Container = new ToolConflictRow()
            };
            model.Container.HasConflict = true;
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
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.IsChecked);
            model.IsChecked = true;
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(eventCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_HasParent_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "HasParent")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.HasParent);
            model.HasParent = true;
            Assert.IsTrue(wasCalled);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_IsMergeVisible_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.IsMergeVisible);
            model.IsMergeVisible = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_Parent_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            var wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "Parent")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.IsMergeVisible);
            model.Parent = new MergeToolModel();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_UniqueId_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.IsMergeVisible);
            model.UniqueId =new System.Guid();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_NodeLocation_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
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
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
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
            var model = new MergeToolModel();
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
            Assert.IsNotNull(model.Children);
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
            var model = new MergeToolModel();
            //------------Execute Test---------------------------
            Assert.AreEqual(default(bool), model.IsTrueArm);
            Assert.AreEqual(default(Point), model.NodeLocation);
            Assert.AreEqual(default(FlowNode), model.FlowNode);
            Assert.AreEqual(default(Guid), model.UniqueId);
            Assert.AreEqual(default(IMergeToolModel), model.Parent);
            Assert.AreEqual(default(IToolConflict), model.Container);
            Assert.AreEqual(default(string), model.NodeArmDescription);
            //------------Assert Results-------------------------
            model.IsTrueArm = true;
            model.NodeLocation = new Point(1, 1);
            model.FlowNode = new FlowStep();
            model.UniqueId = Guid.NewGuid();
            model.Parent = new MergeToolModel();
            model.Container = new ToolConflictRow();
            model.Container.HasConflict = true;
            model.NodeArmDescription = "";
            Assert.AreNotEqual(default(bool), model.IsTrueArm);
            Assert.AreNotEqual(default(Point), model.NodeLocation);
            Assert.AreNotEqual(default(FlowNode), model.FlowNode);
            Assert.AreNotEqual(default(Guid), model.UniqueId);
            Assert.AreNotEqual(default(IMergeToolModel), model.Parent);
            Assert.AreNotEqual(default(IToolConflict), model.Container);
            Assert.AreNotEqual(default(string), model.NodeArmDescription);
        }

        [TestMethod]
        public void RemovePreviousActivity_Given_MergeToolModel_verifyCall()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mergeToolModel = new MergeToolModel
            {
                UniqueId = id,
                IsChecked = true,
                Container = new ToolConflictRow { IsChecked = true }
            };
            object sender = mergeToolModel;
            IMergeToolModel args = new MergeToolModel();
            args.Container = new ToolConflictRow
            {
                UniqueId = id,
                IsChecked = true,
                CurrentViewModel = mergeToolModel,
                DiffViewModel = args
            };
            mergeToolModel.Container = args.Container;
            var wfDesignerVm = new Mock<IWorkflowDesignerViewModel>();
            wfDesignerVm.Setup(p => p.RemoveItem(It.IsAny<IMergeToolModel>()));

            mergeToolModel.WorkflowDesignerViewModel = wfDesignerVm.Object;

            var methodToRun = typeof(MergeToolModel).GetMethod("RemovePreviousContainerActivity", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodToRun);
            //---------------Execute Test ----------------------
            var aaaa = methodToRun.Invoke(mergeToolModel, new object[] { });
            //---------------Test Result -----------------------
            wfDesignerVm.Verify(p => p.RemoveItem(It.IsAny<IMergeToolModel>()), Times.Exactly(1));
        }

        [TestMethod]
        public void AddActivity_Given_MergeToolModel_verifyCall()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mergeToolModel = new MergeToolModel
            {
                UniqueId = id,
                IsChecked = true,
                ModelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity()),
                Container = new ToolConflictRow { IsChecked = true }
            };
            object sender = mergeToolModel;
            IMergeToolModel args = new MergeToolModel();
            args.Container = new ToolConflictRow
            {
                UniqueId = id,
                IsChecked = true,
                CurrentViewModel = mergeToolModel,
                DiffViewModel = args
            };
            mergeToolModel.Container = args.Container;
            var wfDesignerVm = new Mock<IWorkflowDesignerViewModel>();
            wfDesignerVm.Setup(p => p.AddItem(It.IsAny<IMergeToolModel>()));

            mergeToolModel.WorkflowDesignerViewModel = wfDesignerVm.Object;

            var methodToRun = typeof(MergeToolModel).GetMethod("AddActivity", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodToRun);
            //---------------Execute Test ----------------------
            var aaaa = methodToRun.Invoke(mergeToolModel, new object[] { });
            //---------------Test Result -----------------------
            wfDesignerVm.Verify(p => p.AddItem(It.IsAny<IMergeToolModel>()), Times.Exactly(1));
        }
    }
}
