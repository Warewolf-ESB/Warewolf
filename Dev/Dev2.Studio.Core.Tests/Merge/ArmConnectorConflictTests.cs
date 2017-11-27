using Dev2.Common.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ArmConnectorConflictTests
    {

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArmConnectorConflict_IsMergeExpanderEnabled_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ArmConnectorConflict();
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
            Assert.IsFalse(completeConflict.IsMergeExpanderEnabled);
            completeConflict.IsMergeExpanderEnabled = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_OtherIsNull_Returns_False()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ArmConnectorConflict();

            //------------Assert Results-------------------------
            var areEqual = completeConflict.Equals(null);
            Assert.AreEqual(false, areEqual);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_OtherTwoNewObjects_Returns_False()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ArmConnectorConflict();
            var completeConflict1 = new ArmConnectorConflict();

            //------------Assert Results-------------------------
            var areEqual = completeConflict.Equals(completeConflict1);
            Assert.AreEqual(false, areEqual);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_OtherTwoNewObjectsEqualsSides_Returns_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var container = new Mock<IArmConnectorConflict>();
            var a = new MergeArmConnectorConflict(container.Object);
            var completeConflict = new ArmConnectorConflict() {CurrentArmConnector= a, DifferentArmConnector=a };
            var completeConflict1 = new ArmConnectorConflict() { CurrentArmConnector = a, DifferentArmConnector = a };

            //------------Assert Results-------------------------
            var areEqual = completeConflict.Equals(completeConflict1);
            Assert.AreEqual(true, areEqual);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_TwoNewObjectsEqualsSides_Returns_True_Unkown_Object()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var container = new Mock<IArmConnectorConflict>();
            var a = new MergeArmConnectorConflict(container.Object);
            var completeConflict = new ArmConnectorConflict() { CurrentArmConnector = a, DifferentArmConnector = a };
            var completeConflict1 = new ArmConnectorConflict() { CurrentArmConnector = a, DifferentArmConnector = a };

            //------------Assert Results-------------------------
            var areEqual = completeConflict.Equals((object)completeConflict1);
            Assert.AreEqual(true, areEqual);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetHashCode_TwoNewObjectsEqualsSides_Returns_SameValue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var container = new Mock<IArmConnectorConflict>();
            var a = new MergeArmConnectorConflict(container.Object);
            var completeConflict = new ArmConnectorConflict() { CurrentArmConnector = a, DifferentArmConnector = a };
            var completeConflict1 = new ArmConnectorConflict() { CurrentArmConnector = a, DifferentArmConnector = a };

            //------------Assert Results-------------------------
            var hash = completeConflict.GetHashCode();
            var hash1 = completeConflict1.GetHashCode();
            Assert.AreEqual(hash, hash1);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetHashCode_TwoNewObjectsDiffSides_Returns_SameValue()
        {
            //------------Setup for test--------------------------
            var container = new Mock<IArmConnectorConflict>();
            var a = new MergeArmConnectorConflict(container.Object);
            var completeConflict = new ArmConnectorConflict() { CurrentArmConnector = a, DifferentArmConnector = a, UniqueId = Guid.NewGuid() };
            var completeConflict1 = new ArmConnectorConflict() { CurrentArmConnector = a, DifferentArmConnector = a };
            //------------Execute Test---------------------------
            Assert.IsFalse(completeConflict.HasConflict);
            var hash = completeConflict.GetHashCode();
            var hash1 = completeConflict1.GetHashCode();

            //------------Assert Results-------------------------

            Assert.AreNotEqual(hash, hash1);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArmConnectorConflict_HasConflict_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ArmConnectorConflict();
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
            completeConflict.HasConflict = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArmConnectorConflict_UniqueId_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ArmConnectorConflict();
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
            Assert.AreEqual(System.Guid.Empty, completeConflict.UniqueId);
            completeConflict.UniqueId = new System.Guid();
            Assert.IsFalse(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArmConnectorConflict_AutoProperties_tests()
        {
            //------------Setup for test--------------------------
            var model = new ArmConnectorConflict();
            //------------Execute Test---------------------------
            Assert.AreEqual(default(string), model.Key);
            Assert.AreEqual(default(Guid), model.UniqueId);
            Assert.AreEqual(default(bool), model.IsChecked);
            //------------Assert Results-------------------------

            model.Key = "";
            model.UniqueId = Guid.NewGuid();
            model.IsChecked = true;

            Assert.AreNotEqual(default(string), model.Key);
            Assert.AreNotEqual(default(Guid), model.UniqueId);
            Assert.AreNotEqual(default(bool), model.IsChecked);

        }
    }
}
