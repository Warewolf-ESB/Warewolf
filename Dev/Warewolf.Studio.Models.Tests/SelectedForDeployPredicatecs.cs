using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            var selected1 = new Mock<IResource>();
            selected1.Setup(a => a.IsSelected).Returns(true);
            selected1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IResource>();
            selected2.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IResource>();
            destination1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsTrue(pred.Predicate(selected1.Object,new List<IResource>{selected1.Object,selected2.Object},new List<IResource>{destination1.Object} ));
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
            var selected1 = new Mock<IResource>();
            selected1.Setup(a => a.IsSelected).Returns(false);
            selected1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IResource>();
            selected2.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IResource>();
            destination1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);

            //------------Execute Test---------------------------
            Assert.IsFalse(pred.Predicate(selected1.Object, new List<IResource> { selected1.Object, selected2.Object }, new List<IResource> { destination1.Object }));
            //------------Assert Results-------------------------


        }
        // ReSharper restore InconsistentNaming
    }
}
