using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Models.Deploy;

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
            var selected1 = new Mock<IExplorerItemViewModel>();
            selected1.Setup(a => a.Checked).Returns(true);
            selected1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IExplorerItemViewModel>();
            selected2.Setup(a => a.ResourceId).Returns(resourceIds[0]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IExplorerItemViewModel>();
            destination1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsTrue(pred.Predicate(selected1.Object, new List<IExplorerItemViewModel> { selected1.Object, selected2.Object }, new List<IExplorerItemViewModel> { destination1.Object }));

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
            var selected1 = new Mock<IExplorerItemViewModel>();
            selected1.Setup(a => a.Checked).Returns(true);
            selected1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IExplorerItemViewModel>();
            selected2.Setup(a => a.ResourceId).Returns(resourceIds[0]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IExplorerItemViewModel>();
            destination1.Setup(a => a.ResourceId).Returns(resourceIds[0]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsFalse(pred.Predicate(selected1.Object, new List<IExplorerItemViewModel> { selected1.Object, selected2.Object }, new List<IExplorerItemViewModel> { destination1.Object }));

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
            var selected1 = new Mock<IExplorerItemViewModel>();
            selected1.Setup(a => a.Checked).Returns(false);
            selected1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IExplorerItemViewModel>();
            selected2.Setup(a => a.ResourceId).Returns(resourceIds[0]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IExplorerItemViewModel>();
            destination1.Setup(a => a.ResourceId).Returns(resourceIds[0]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsFalse(pred.Predicate(selected1.Object, new List<IExplorerItemViewModel> { selected1.Object, selected2.Object }, new List<IExplorerItemViewModel> { destination1.Object }));

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
