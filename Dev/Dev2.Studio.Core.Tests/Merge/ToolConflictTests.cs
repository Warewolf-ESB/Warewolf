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
            var completeConflict = new ToolConflict();
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsNotNull(completeConflict.Children);
            Assert.IsNull(completeConflict.CurrentViewModel);
            Assert.IsNull(completeConflict.DiffViewModel);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_CurrentViewModel_DefaultConstruction_ShouldBeNull()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsNotNull(completeConflict.Children);
            Assert.IsNull(completeConflict.CurrentViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_DiffViewModel_DefaultConstruction_ShouldBeNull()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();            
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsNotNull(completeConflict.Children);
            Assert.IsNull(completeConflict.CurrentViewModel);
            Assert.IsNull(completeConflict.DiffViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_HasConflict_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
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
            Assert.IsNotNull(completeConflict.Children);
            Assert.IsFalse(completeConflict.HasConflict);
            completeConflict.HasConflict = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_HasNodeArmConflict_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
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
            Assert.IsNotNull(completeConflict.Children);
            Assert.IsFalse(completeConflict.HasNodeArmConflict);
            completeConflict.HasNodeArmConflict = true;
            Assert.IsTrue(wasCalled);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_IsMergeExpanderEnabled_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
            var wasCalled = false;
            completeConflict.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsMergeExpanderEnabled")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsNotNull(completeConflict.Children);
            Assert.IsFalse(completeConflict.IsMergeExpanderEnabled);
            completeConflict.IsMergeExpanderEnabled = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_IsMergeExpanded_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
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
            Assert.IsNotNull(completeConflict.Children);
            Assert.IsFalse(completeConflict.IsMergeExpanded);
            completeConflict.IsMergeExpanded = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Find_GivenNomatch_ExpectNull()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            //------------Execute Test---------------------------
            var result=completeConflict.Find(new ToolConflict());
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Find_GivenExists_ExpectResults()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            var child = new ToolConflict();

            completeConflict.Children.AddFirst(child);
            //------------Execute Test---------------------------
            var result = completeConflict.Find(child);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreSame(child, result.Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Find_GivenGrandChildExists_ExpectResults()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            var child = new ToolConflict();
            var gChild = new ToolConflict();
            child.Children.AddFirst(gChild);
            completeConflict.Children.AddFirst(child);
            //------------Execute Test---------------------------
            var result = completeConflict.Find(gChild);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreSame(gChild, result.Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetNextConflict_GivenEmptyChildren_ExpectNull()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();

            //------------Execute Test---------------------------
            var result = completeConflict.GetNextConflict();
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetNextConflict_GivenOnChildChildren_ExpectNull()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            var value = new ToolConflict();
            completeConflict.Children.AddFirst(value);
            //------------Execute Test---------------------------
            var result = completeConflict.GetNextConflict();
            //------------Assert Results-------------------------
            Assert.AreSame(value, result);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetNextConflict_GivenOnChildChildren_ExpectNull_Multiple()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            var value = new ToolConflict();
            var value1 = new ToolConflict();
            completeConflict.Children.AddFirst(value);
            value.Children.AddFirst(value1);
            //------------Execute Test---------------------------
            var result = completeConflict.GetNextConflict();
            Assert.AreSame(value, result);
            Assert.IsNotNull(result);
            //------------Assert Results-------------------------
             result = completeConflict.GetNextConflict();
            Assert.AreSame(value1, result);
            Assert.IsNotNull(result);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Find_GivenNull_ExpectNull()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            //------------Execute Test---------------------------
            var result = completeConflict.Find(new ToolConflict());
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void All_GivenIncorrectMatch_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            //------------Execute Test---------------------------
            var result = completeConflict.All(p=>p.UniqueId == Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void All_GivenMatch_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            completeConflict.Children.AddFirst(new ToolConflict());
            //------------Execute Test---------------------------
            var result = completeConflict.All(p => p.UniqueId == Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void All_GivenMatch_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var completeConflict = new ToolConflict();
            completeConflict.Children.AddFirst(new ToolConflict());
            //------------Execute Test---------------------------
            var result = completeConflict.All(p => p.UniqueId == Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_UniqueId_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
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
            Assert.IsNotNull(completeConflict.Children);
            Assert.AreEqual(System.Guid.Empty, completeConflict.UniqueId);
            completeConflict.UniqueId = new System.Guid();
            Assert.IsFalse(wasCalled);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToolConflict_AutoProperties_tests()
        {
            //------------Setup for test--------------------------
            var model = new ToolConflict();
            //------------Execute Test---------------------------
            //Assert.AreEqual(default(string), model.Key);
            Assert.AreEqual(default(Guid), model.UniqueId);
            Assert.AreEqual(default(bool), model.IsChecked);
            Assert.AreEqual(default(bool), model.IsContainerTool);
            Assert.AreEqual(default(IMergeToolModel), model.DiffViewModel);
            Assert.AreEqual(default(IMergeToolModel), model.CurrentViewModel);
            Assert.AreEqual(default(IToolConflict), model.Parent);
            //------------Assert Results-------------------------

            model.UniqueId = Guid.NewGuid();
            model.IsChecked = true;
            model.IsContainerTool = true;
            model.Parent = new Mock<IToolConflict>().Object;
            model.CurrentViewModel = new Mock<IMergeToolModel>().Object;
            model.DiffViewModel = new Mock<IMergeToolModel>().Object;

            Assert.AreNotEqual(default(Guid), model.UniqueId);
            Assert.AreNotEqual(default(bool), model.IsChecked);
            Assert.AreNotEqual(default(bool), model.IsContainerTool);
            Assert.AreNotEqual(default(IToolConflict), model.Parent);
            Assert.AreNotEqual(default(IMergeToolModel), model.DiffViewModel);
            Assert.AreNotEqual(default(IMergeToolModel), model.CurrentViewModel);

        }
    }
}
