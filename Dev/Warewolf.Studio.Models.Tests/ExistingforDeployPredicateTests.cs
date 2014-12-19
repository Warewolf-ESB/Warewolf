using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.Models.Tests
{
    [TestClass]
    public class ExistingforDeployPredicateTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExistingForDeployPredicate_Predicate")]
        // ReSharper disable InconsistentNaming
        public void ExistingForDeployPredicate_Predicate_SingleExists_ExpectNoNew()
        {
            //------------Setup for test--------------------------
            var pred = new ExistingForDeployPredicate();

            //------------Execute Test---------------------------

            var resourceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var selected1 = new Mock<IResource>();
            selected1.Setup(a => a.IsSelected).Returns(true);
            selected1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IResource>();
            selected2.Setup(a => a.ResourceID).Returns(resourceIds[0]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IResource>();
            destination1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsTrue(pred.Predicate(selected1.Object, new List<IResource> { selected1.Object, selected2.Object }, new List<IResource> { destination1.Object }));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExistingForDeployPredicate_Predicate")]
        // ReSharper disable InconsistentNaming
        public void ExistingForDeployPredicate_Predicate_SingleNotExists_ExpectOneNew()
        {
            //------------Setup for test--------------------------
            var pred = new ExistingForDeployPredicate();

            //------------Execute Test---------------------------

            var resourceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var selected1 = new Mock<IResource>();
            selected1.Setup(a => a.IsSelected).Returns(true);
            selected1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IResource>();
            selected2.Setup(a => a.ResourceID).Returns(resourceIds[0]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IResource>();
            destination1.Setup(a => a.ResourceID).Returns(resourceIds[0]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsFalse(pred.Predicate(selected1.Object, new List<IResource> { selected1.Object, selected2.Object }, new List<IResource> { destination1.Object }));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExistingForDeployPredicate_Predicate")]
        // ReSharper disable InconsistentNaming
        public void ExistingForDeployPredicate_Predicate_SingleNotExists_NotChecked_ExpectNoNew()
        {
            //------------Setup for test--------------------------
            var pred = new ExistingForDeployPredicate();

            //------------Execute Test---------------------------

            var resourceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var selected1 = new Mock<IResource>();
            selected1.Setup(a => a.IsSelected).Returns(false);
            selected1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IResource>();
            selected2.Setup(a => a.ResourceID).Returns(resourceIds[0]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IResource>();
            destination1.Setup(a => a.ResourceID).Returns(resourceIds[0]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsFalse(pred.Predicate(selected1.Object, new List<IResource> { selected1.Object, selected2.Object }, new List<IResource> { destination1.Object }));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExistingForDeployPredicate_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ExistingForDeployPredicate_Predicate_Ctor()
        {
            //------------Setup for test--------------------------
            var pred = new ExistingForDeployPredicate();
            Assert.AreEqual("Existing Resources", pred.Name);
        }
    }
}
