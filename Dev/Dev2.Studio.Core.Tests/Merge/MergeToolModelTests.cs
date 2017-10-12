using Dev2.Studio.Core.Activities.Utils;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;

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
        public void CompleteConflict_MergeDescription_DefaultConstruction()
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
        public void CompleteConflict_ParentDescription_DefaultConstruction()
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
        public void CompleteConflict_MergeIcon_DefaultConstruction()
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
        public void CompleteConflict_IsMergeChecked_DefaultConstruction()
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
        public void CompleteConflict_HasParent_DefaultConstruction()
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
        public void CompleteConflict_IsMergeEnabled_DefaultConstruction()
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
        public void CompleteConflict_IsMergeVisible_DefaultConstruction()
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
        public void CompleteConflict_Parent_DefaultConstruction()
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
        public void CompleteConflict_UniqueId_DefaultConstruction()
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
        public void CompleteConflict_ActivityType_DefaultConstruction()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = new MergeToolModel();
            bool wasCalled = false;
            model.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "ActivityType")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Children);
            Assert.IsFalse(model.IsMergeVisible);
            model.ActivityType = new FlowStep();
            Assert.IsTrue(wasCalled);
        }

        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CompleteConflict_NodeLocation_DefaultConstruction()
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
            Assert.IsNull(model.FlowNode);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CompleteConflict_FlowNode_DefaultConstruction()
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
            model.FlowNode = ModelItemUtils.CreateModelItem(new object());
            Assert.IsFalse(wasCalled);
            Assert.IsNotNull(model.FlowNode);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CompleteConflict_ActivityDesignerViewModel_DefaultConstruction()
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
    }
}
