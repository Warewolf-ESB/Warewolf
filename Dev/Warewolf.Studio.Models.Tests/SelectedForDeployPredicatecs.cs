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
    public class SelectedForDeployPredicatesTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedForDeployPredicate_Predicate")]
        // ReSharper disable InconsistentNaming
        public void SelectedForDeployPredicate_Predicate_ItemChecked_ExpectTrue()

        {
  
            //------------Setup for test--------------------------

            var pred = new SelectedForDeployPredicate(new List<ResourceType>());
            var resourceIds = new List<Guid> {Guid.NewGuid(),Guid.NewGuid()};
            var selected1 = new Mock<IExplorerItemViewModel>();
            selected1.Setup(a => a.Checked).Returns(true);
            selected1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IExplorerItemViewModel>();
            selected2.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IExplorerItemViewModel>();
            destination1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsTrue(pred.Predicate(selected1.Object, new List<IExplorerItemViewModel> { selected1.Object, selected2.Object }, new List<IExplorerItemViewModel> { destination1.Object }));
            //------------Assert Results-------------------------

        
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedForDeployPredicate_Predicate")]
        public void SelectedForDeployPredicate_Predicate_Ctor_ExpectThatNameAndExclusionsAreCorrect()
        {

            //------------Setup for test--------------------------
            var excludedTypes = new List<ResourceType>{ResourceType.Folder};

            var pred = new SelectedForDeployPredicate(excludedTypes);


            //------------Execute Test---------------------------
            Assert.AreEqual("Selected",pred.Name);
            Assert.AreEqual(pred.ExcludedTypes, excludedTypes);
               //------------Assert Results-------------------------


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedForDeployPredicate_Predicate")]
        public void SelectedForDeployPredicate_Predicate_ItemNoChecked_ExpectFalse()
        {

            //------------Setup for test--------------------------

            var pred = new SelectedForDeployPredicate(new List<ResourceType>());
            var resourceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var selected1 = new Mock<IExplorerItemViewModel>();
            selected1.Setup(a => a.Checked).Returns(false);
            selected1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IExplorerItemViewModel>();
            selected2.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IExplorerItemViewModel>();
            destination1.Setup(a => a.ResourceId).Returns(resourceIds[1]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsFalse(pred.Predicate(selected1.Object, new List<IExplorerItemViewModel> { selected1.Object, selected2.Object }, new List<IExplorerItemViewModel> { destination1.Object }));
            //------------Assert Results-------------------------


        }
        // ReSharper restore InconsistentNaming
    }
}
