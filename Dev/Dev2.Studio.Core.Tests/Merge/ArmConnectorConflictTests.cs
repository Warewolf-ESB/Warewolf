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
        public void Equals_OtherIsNull_Returns_False()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ConnectorConflictRow();

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
            var completeConflict = new ConnectorConflictRow();
            var completeConflict1 = new ConnectorConflictRow();

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
            var id = Guid.NewGuid();
            var a = new ConnectorConflictItem(id, "", id, id, "a");
            var completeConflict = new ConnectorConflictRow {CurrentArmConnector= a, DifferentArmConnector=a };
            var completeConflict1 = new ConnectorConflictRow { CurrentArmConnector = a, DifferentArmConnector = a };

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
            var id = Guid.NewGuid();
            var a = new ConnectorConflictItem(id, "", id, id, "a");
            var completeConflict = new ConnectorConflictRow { CurrentArmConnector = a, DifferentArmConnector = a };
            var completeConflict1 = new ConnectorConflictRow { CurrentArmConnector = a, DifferentArmConnector = a };

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
            var id = Guid.NewGuid();
            var a = new ConnectorConflictItem(id, "", id, id, "a");
            var completeConflict = new ConnectorConflictRow { CurrentArmConnector = a, DifferentArmConnector = a };
            var completeConflict1 = new ConnectorConflictRow { CurrentArmConnector = a, DifferentArmConnector = a };

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
            var id = Guid.NewGuid();
            var a = new ConnectorConflictItem(id, "", id, id, "a");
            var completeConflict = new ConnectorConflictRow { CurrentArmConnector = a, DifferentArmConnector = a};
            var completeConflict1 = new ConnectorConflictRow { CurrentArmConnector = a, DifferentArmConnector = a };
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
            var completeConflict = new ConnectorConflictRow();
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
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArmConnectorConflict_UniqueId_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ConnectorConflictRow();
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
            Assert.IsFalse(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArmConnectorConflict_AutoProperties_tests()
        {
            //------------Setup for test--------------------------
            var model = new ConnectorConflictRow();
            //------------Execute Test---------------------------
            Assert.AreEqual(default(string), model.Key);
            Assert.AreEqual(default(Guid), model.UniqueId);
            Assert.AreEqual(default(bool), model.IsChecked);
            //------------Assert Results-------------------------

            model.Key = "";
            model.IsChecked = true;

            Assert.AreNotEqual(default(string), model.Key);
            Assert.AreNotEqual(default(Guid), model.UniqueId);
            Assert.AreNotEqual(default(bool), model.IsChecked);

        }
    }
}
