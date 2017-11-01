using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class CompleteConflictTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CompleteConflict_Constructor_DefaultConstruction_ShouldHaveChildren()
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
        public void CompleteConflict_CurrentViewModel_DefaultConstruction_ShouldBeNull()
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
        public void CompleteConflict_DiffViewModel_DefaultConstruction_ShouldBeNull()
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
        public void CompleteConflict_HasConflict_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
            bool wasCalled = false;
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
        public void CompleteConflict_IsMergeExpanderEnabled_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
            bool wasCalled = false;
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
        public void CompleteConflict_IsMergeExpanded_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
            bool wasCalled = false;
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
        public void CompleteConflict_UniqueId_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ToolConflict();
            bool wasCalled = false;
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
    }
}
