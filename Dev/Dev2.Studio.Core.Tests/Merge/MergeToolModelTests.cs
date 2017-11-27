﻿using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Windows;

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
            bool wasCalled = false;
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
            bool wasCalled = false;
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
            bool wasCalled = false;
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
        public void DisableEvents_VerifySets()
        {
            //------------Setup for test--------------------------
            var model = new MergeToolModel();
            model.IsMergeChecked = true;
            //------------Execute Test---------------------------
            model.DisableEvents();
            //------------Assert Results-------------------------
            Assert.IsFalse(model.IsMergeChecked);


        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_ParentDescription_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            bool wasCalled = false;
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
            bool wasCalled = false;
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
            var model = new MergeToolModel();
            bool wasCalled = false;
            bool eventCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsMergeChecked")
                {
                    wasCalled = true;
                }
            };
            model.SomethingModelToolChanged += (a, b) => 
            {
                eventCalled = true;
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.IsMergeChecked);
            model.IsMergeChecked = true;
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
            bool wasCalled = false;
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
        public void MergeToolModel_IsMergeEnabled_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            bool wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsMergeEnabled")
                {
                    wasCalled = true;
                }
            };
            
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.IsMergeChecked);
            model.IsMergeEnabled = true;
            Assert.IsTrue(wasCalled);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeToolModel_IsMergeVisible_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            bool wasCalled = false;
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
            bool wasCalled = false;
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
            bool wasCalled = false;
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
            bool wasCalled = false;
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
            System.Windows.Point point = new System.Windows.Point();
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
            bool wasCalled = false;
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
            bool wasCalled = false;
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
            Assert.AreEqual(default(bool), model.IsMergeEnabled);
            Assert.AreEqual(default(bool), model.IsMergeEnabled);
            Assert.AreEqual(default(string), model.NodeArmDescription);
            //------------Assert Results-------------------------
            model.IsTrueArm = true;
            model.NodeLocation = new Point(1, 1);
            model.FlowNode = new FlowStep();
            model.UniqueId = Guid.NewGuid();
            model.Parent = new MergeToolModel();
            model.Container = new ToolConflict();
            model.IsMergeEnabled = true;
            model.IsMergeEnabled = true;
            model.NodeArmDescription = "";
            Assert.AreNotEqual(default(bool), model.IsTrueArm);
            Assert.AreNotEqual(default(Point), model.NodeLocation);
            Assert.AreNotEqual(default(FlowNode), model.FlowNode);
            Assert.AreNotEqual(default(Guid), model.UniqueId);
            Assert.AreNotEqual(default(IMergeToolModel), model.Parent);
            Assert.AreNotEqual(default(IToolConflict), model.Container);
            Assert.AreNotEqual(default(bool), model.IsMergeEnabled);
            Assert.AreNotEqual(default(bool), model.IsMergeEnabled);
            Assert.AreNotEqual(default(string), model.NodeArmDescription);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EnableEvents_processEvents_Tests()
        {
            //------------Setup for test--------------------------
            var model = new MergeToolModel();
            //------------Execute Test---------------------------
            var initValue=typeof(MergeToolModel).GetField("_processEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(model);
            var boolValueInit=(bool)initValue;
            Assert.IsTrue(boolValueInit);
            //------------Assert Results-------------------------
            model.DisableEvents();
            initValue = typeof(MergeToolModel).GetField("_processEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(model);
            boolValueInit = (bool)initValue;
            Assert.IsFalse(boolValueInit);
            model.EnableEvents();
            initValue = typeof(MergeToolModel).GetField("_processEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(model);
            boolValueInit = (bool)initValue;
            Assert.IsTrue(boolValueInit);
        }
    }
}
