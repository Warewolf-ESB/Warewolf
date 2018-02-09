using Dev2.Common.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class MergeArmConnectorConflictTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_OtherIsNull_Returns_False()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var a = Guid.NewGuid();
            var completeConflict = new ConnectorConflictItem("a -> b", a, a, "a", new ConnectorConflictRow());

            //------------Assert Results-------------------------
            var areEqual = completeConflict.Equals(null);
            Assert.AreEqual(false, areEqual);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_TwoNewObjects_Returns_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var a = Guid.NewGuid();
            var completeConflict = new ConnectorConflictItem("a -> b", a, a, "a", new ConnectorConflictRow());
            var completeConflict1 = new ConnectorConflictItem("a -> b", a, a, "a", new ConnectorConflictRow());

            //------------Assert Results-------------------------
            var areEqual = completeConflict.Equals(completeConflict1);
            Assert.AreEqual(true, areEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void MergeArmConnectorConflict_LeftArmDescription_ShouldHaveValue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var a = Guid.NewGuid();
            var completeConflict = new ConnectorConflictItem("a -> b", a, a, "a", new ConnectorConflictRow());
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.AreEqual("a ", completeConflict.LeftArmDescription);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void MergeArmConnectorConflict_RightArmDescription_ShouldHaveValue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var a = Guid.NewGuid();
            var completeConflict = new ConnectorConflictItem("a -> b", a, a, "a", new ConnectorConflictRow());
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.AreEqual(" b", completeConflict.RightArmDescription);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_OtherTwoNewObjectsEqualsSides_Returns_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var container = new Mock<IConnectorConflictRow>();
            var completeConflict = new ConnectorConflictItem(container.Object);
            var completeConflict1 = new ConnectorConflictItem(container.Object);

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
            var container = new Mock<IConnectorConflictRow>();
            var completeConflict = new ConnectorConflictItem(container.Object);
            var completeConflict1 = new ConnectorConflictItem(container.Object);

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
            var container = new Mock<IConnectorConflictRow>();
            var id=Guid.NewGuid();
            var completeConflict = new ConnectorConflictItem(container.Object) {SourceUniqueId=id, DestinationUniqueId=id };
            var completeConflict1 = new ConnectorConflictItem(container.Object) { SourceUniqueId = id, DestinationUniqueId = id }; ;

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
            var container = new Mock<IConnectorConflictRow>();
            var a = new ConnectorConflictItem(container.Object);
            var id = Guid.NewGuid();
            var completeConflict = new ConnectorConflictItem("", id, id, "a", new ConnectorConflictRow());
            var completeConflict1 = new ConnectorConflictItem("", Guid.Empty, id, "a", new ConnectorConflictRow());
            //------------Execute Test---------------------------
            var hash = completeConflict.GetHashCode();
            var hash1 = completeConflict1.GetHashCode();

            //------------Assert Results-------------------------

            Assert.AreNotEqual(hash, hash1);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MergeArmConnectorConflict_IsChecked_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            var container = new Mock<IConnectorConflictRow>();
            //------------Execute Test---------------------------
            var completeConflict = new ConnectorConflictItem(container.Object);
            var wasCalled = false;
            var onChecked = false;
            completeConflict.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsChecked")
                {
                    wasCalled = true;
                }
            };
            completeConflict.OnChecked += (a, b) => { onChecked = true; };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            completeConflict.IsChecked = true;
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(onChecked);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ArmConnectorConflict_AutoProperties_tests()
        {
            //------------Setup for test--------------------------
            //------------Setup for test--------------------------
            var container = new Mock<IConnectorConflictRow>();
            //------------Execute Test---------------------------
            var model = new ConnectorConflictItem(container.Object);
            //------------Execute Test---------------------------
            Assert.AreEqual(default(string), model.ArmDescription);
            Assert.AreEqual(default(string), model.Key);
            Assert.AreEqual(default(bool), model.IsChecked);
            Assert.AreNotEqual(default(bool), model.ConnectorConflictRow);

            //------------Assert Results-------------------------

            model.Key = "";
            model.ArmDescription = "";
            model.IsChecked = true;

            Assert.AreNotEqual(default(string), model.Key);
            Assert.AreNotEqual(default(string), model.ArmDescription);
            Assert.AreNotEqual(default(bool), model.IsChecked);
        }
    }
}
