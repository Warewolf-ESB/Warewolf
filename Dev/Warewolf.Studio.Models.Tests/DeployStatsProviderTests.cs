using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.Models.Tests
{
    [TestClass]
    public class DeployStatsProviderTests
    {
        //Given a set of predicates, the deploy stats calculator must return a summary that matches those predicates

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsProvider_CalculateStats")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void DeployStatsProvider_CalculateStats_NullSource_ExpectError()

        {
            //------------Setup for test--------------------------
            var deployStatsProvider = new DeployStatsProvider();
            deployStatsProvider.CalculateStats(null, new List<IResource>(), new List<IDeployPredicate>());
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsProvider_CalculateStats")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployStatsProvider_CalculateStats_NullDestination_ExpectError()
        {
            //------------Setup for test--------------------------
            var deployStatsProvider = new DeployStatsProvider();
            deployStatsProvider.CalculateStats(new List<IResource>(), null, new List<IDeployPredicate>());
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsProvider_CalculateStats")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployStatsProvider_CalculateStats_NullPredicates_ExpectError()
        {
            //------------Setup for test--------------------------
            var deployStatsProvider = new DeployStatsProvider();
            deployStatsProvider.CalculateStats(new List<IResource>(), new List<IResource>(), null);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsProvider_CalculateStats")]
        public void DeployStatsProvider_CalculateStats_EmptyValues_Expect_EmptyResults()
        {
            //------------Setup for test--------------------------
            var deployStatsProvider = new DeployStatsProvider();
              //------------Execute Test---------------------------
            var output = deployStatsProvider.CalculateStats(new List<IResource>(), new List<IResource>(), new List<IDeployPredicate>());
         
            //------------Assert Results-------------------------
            Assert.IsNotNull(output);
            Assert.AreEqual(output.Count,0);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsProvider_CalculateStats")]
        public void DeployStatsProvider_CalculateStats_HasPredicatesAlwaysReturnsTrue_Expect_CountOfProjects()
        {
            //------------Setup for test--------------------------
            var deployStatsProvider = new DeployStatsProvider();
            var pred = new Mock<IDeployPredicate>();
            // ReSharper disable once MaximumChainedReferences
            pred.Setup(a => a.Predicate(It.IsAny<IResource>(), It.IsAny<IList<IResource>>(), It.IsAny<IList<IResource>>())).Returns(true);
            pred.Setup(a => a.Name).Returns("BobThePredicate");
            //------------Execute Test---------------------------
            var output = deployStatsProvider.CalculateStats(new List<IResource> { new Mock<IResource>().Object }, new List<IResource> { new Mock<IResource>().Object }, new List<IDeployPredicate> {pred.Object});

            //------------Assert Results-------------------------
            Assert.IsNotNull(output);
            Assert.AreEqual(output.Count, 1);
            Assert.AreEqual("BobThePredicate",output[0].Name);
            Assert.AreEqual("1",output[0].Description);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsProvider_CalculateStats")]
        public void DeployStatsProvider_CalculateStats_HasMultiplePredicatesAlwaysReturnsTrue_Expect_CountOfProjects()
        {
            //------------Setup for test--------------------------
            var deployStatsProvider = new DeployStatsProvider();
            var pred = new Mock<IDeployPredicate>();
            // ReSharper disable once MaximumChainedReferences
            pred.Setup(a => a.Predicate(It.IsAny<IResource>(), It.IsAny<IList<IResource>>(), It.IsAny<IList<IResource>>())).Returns(true);
            pred.Setup(a => a.Name).Returns("BobThePredicate");

            var pred2 = new Mock<IDeployPredicate>();
            // ReSharper disable once MaximumChainedReferences
            pred2.Setup(a => a.Predicate(It.IsAny<IResource>(), It.IsAny<IList<IResource>>(), It.IsAny<IList<IResource>>())).Returns(false);
            pred2.Setup(a => a.Name).Returns("DoraThePredicate");
            //------------Execute Test---------------------------
            var output = deployStatsProvider.CalculateStats(new List<IResource> { new Mock<IResource>().Object }, new List<IResource> { new Mock<IResource>().Object }, new List<IDeployPredicate> { pred.Object , pred2.Object });

            //------------Assert Results-------------------------
            Assert.IsNotNull(output);
            Assert.AreEqual(output.Count, 2);
            Assert.AreEqual("BobThePredicate", output[0].Name);
            Assert.AreEqual("1", output[0].Description);
            Assert.AreEqual("DoraThePredicate", output[1].Name);
            Assert.AreEqual("0", output[1].Description);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployStatsProvider_CalculateStats")]
        public void DeployStatsProvider_CalculateStats_PredicateWithMultipleResources_Expect_CountOfProjects()
        {
            //------------Setup for test--------------------------
            var deployStatsProvider = new DeployStatsProvider();
            var pred = new Mock<IDeployPredicate>();
            var resourceIds = new List<Guid> {Guid.NewGuid(),Guid.NewGuid()};
            var selected1 = new Mock<IResource>();
            selected1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var selected2 = new Mock<IResource>();
            selected2.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            selected2.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            var destination1 = new Mock<IResource>();
            destination1.Setup(a => a.ResourceID).Returns(resourceIds[1]);
            destination1.Setup(a => a.ResourceType).Returns(ResourceType.DbService);
            // ReSharper disable once MaximumChainedReferences
            pred.Setup(a => a.Predicate(It.IsAny<IResource>(), It.IsAny<IList<IResource>>(), It.IsAny<IList<IResource>>())).Returns(true);
            pred.Setup(a => a.Name).Returns("BobThePredicate");

            var pred2 = new Mock<IDeployPredicate>();
            // ReSharper disable once MaximumChainedReferences
            pred2.Setup(a => a.Predicate(It.IsAny<IResource>(), It.IsAny<IList<IResource>>(), It.IsAny<IList<IResource>>())).Returns(false);
            pred2.Setup(a => a.Name).Returns("DoraThePredicate");
            //------------Execute Test---------------------------
            var output = deployStatsProvider.CalculateStats(new List<IResource> { selected1.Object,selected2.Object}, new List<IResource> { destination1.Object }, new List<IDeployPredicate> { pred.Object, pred2.Object });

            //------------Assert Results-------------------------
            Assert.IsNotNull(output);
            Assert.AreEqual(output.Count, 2);
            Assert.AreEqual("BobThePredicate", output[0].Name);
            Assert.AreEqual("2", output[0].Description);
            Assert.AreEqual("DoraThePredicate", output[1].Name);
            Assert.AreEqual("0", output[1].Description);
        }

    }
    // ReSharper restore InconsistentNaming
}
