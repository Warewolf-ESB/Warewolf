using Dev2.Common.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ToolConflictTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_Constructor_DefaultConstruction_ShouldHaveChildren()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflictRow();
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsNull(completeConflict.CurrentViewModel);
            Assert.IsNull(completeConflict.DiffViewModel);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_CurrentViewModel_DefaultConstruction_ShouldBeNull()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflictRow();
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsNull(completeConflict.CurrentViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_DiffViewModel_DefaultConstruction_ShouldBeNull()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflictRow();            
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsNull(completeConflict.CurrentViewModel);
            Assert.IsNull(completeConflict.DiffViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_HasConflict_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflictRow();
            var wasCalled = false;
            completeConflict.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "HasConflict")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsFalse(completeConflict.HasConflict);
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_HasNodeArmConflict_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflictRow();
            var wasCalled = false;
            completeConflict.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "HasNodeArmConflict")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsFalse(completeConflict.HasNodeArmConflict);
            completeConflict.HasNodeArmConflict = true;
            Assert.IsTrue(wasCalled);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_IsMergeExpanded_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflictRow();
            var wasCalled = false;
            completeConflict.PropertyChanged += (a,b)=> 
            {
                if(b.PropertyName == "IsMergeExpanded")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsFalse(completeConflict.IsMergeExpanded);
            completeConflict.IsMergeExpanded = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_UniqueId_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflictRow();
            var wasCalled = false;
            completeConflict.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "UniqueId")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.AreEqual(System.Guid.Empty, completeConflict.UniqueId);
            completeConflict.UniqueId = new System.Guid();
            Assert.IsFalse(wasCalled);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_AutoProperties_tests()
        {
            //------------Setup for test--------------------------
            var model = new ToolConflictRow();
            //------------Execute Test---------------------------
            //Assert.AreEqual(default(string), model.Key);
            Assert.AreEqual(default(Guid), model.UniqueId);
            Assert.AreEqual(default(bool), model.IsChecked);
            Assert.AreEqual(default(bool), model.IsContainerTool);
            Assert.AreEqual(default(IToolModelConflictItem), model.DiffViewModel);
            Assert.AreEqual(default(IToolModelConflictItem), model.CurrentViewModel);
            //------------Assert Results-------------------------

            model.UniqueId = Guid.NewGuid();
            model.IsChecked = true;
            model.IsContainerTool = true;
            model.CurrentViewModel = new Mock<IToolModelConflictItem>().Object;
            model.DiffViewModel = new Mock<IToolModelConflictItem>().Object;

            Assert.AreNotEqual(default(Guid), model.UniqueId);
            Assert.AreNotEqual(default(bool), model.IsChecked);
            Assert.AreNotEqual(default(bool), model.IsContainerTool);
            Assert.AreNotEqual(default(IToolModelConflictItem), model.DiffViewModel);
            Assert.AreNotEqual(default(IToolModelConflictItem), model.CurrentViewModel);

        }
    }
}
