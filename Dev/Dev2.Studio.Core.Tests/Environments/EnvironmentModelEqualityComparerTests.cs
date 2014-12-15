
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Environments
{
    [TestClass]
    public class EnvironmentModelEqualityComparerTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModelEqualityComparer_Instance")]
        public void EnvironmentModelEqualityComparer_Instance_IsSingleton()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var comparer1 = EnvironmentModelEqualityComparer.Current;
            var comparer2 = EnvironmentModelEqualityComparer.Current;

            //------------Assert Results-------------------------
            Assert.AreSame(comparer1, comparer2);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModelEqualityComparer_Equals")]
        public void EnvironmentModelEqualityComparer_Equals_XIsNull_False()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";
            const string Name = "test";

            var environment = EnvironmentModelTest.CreateEqualityEnvironmentModel(resourceID, Name, serverID, ServerUri);

            //------------Execute Test---------------------------
            var actual = EnvironmentModelEqualityComparer.Current.Equals(null, environment);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModelEqualityComparer_Equals")]
        public void EnvironmentModelEqualityComparer_Equals_YIsNull_False()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";
            const string Name = "test";

            var environment = EnvironmentModelTest.CreateEqualityEnvironmentModel(resourceID, Name, serverID, ServerUri);

            //------------Execute Test---------------------------
            var actual = EnvironmentModelEqualityComparer.Current.Equals(environment, null);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModelEqualityComparer_Equals")]
        public void EnvironmentModelEqualityComparer_Equals_YIsNotEnvironmentModel_False()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";
            const string Name = "test";

            var environment = EnvironmentModelTest.CreateEqualityEnvironmentModel(resourceID, Name, serverID, ServerUri);

            //------------Execute Test---------------------------
            var actual = EnvironmentModelEqualityComparer.Current.Equals(environment, new object());

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModelEqualityComparer_Equals")]
        public void EnvironmentModelEqualityComparer_Equals_YIsEnvironmentModel_InvokesEqualsOfX()
        {
            //------------Setup for test--------------------------
            var environment1 = new Mock<IEnvironmentModel>();
            var environment2 = new Mock<IEnvironmentModel>();

            environment1.Setup(e => e.Equals(environment2.Object)).Verifiable();

            //------------Execute Test---------------------------
           var actual = EnvironmentModelEqualityComparer.Current.Equals(environment1.Object, environment2.Object);

            //------------Assert Results-------------------------
           environment1.Verify(e => e.Equals(environment2.Object));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentModelEqualityComparer_GetHashCode")]
        public void EnvironmentModelEqualityComparer_GetHashCode_InvokesGetHashCodeOfArg()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            const string ServerUri = "https://myotherserver1:3143";
            const string Name = "test";

            var environment = EnvironmentModelTest.CreateEqualityEnvironmentModel(resourceID, Name, serverID, ServerUri);
            var expected = environment.GetHashCode();

            //------------Execute Test---------------------------
            var actual = EnvironmentModelEqualityComparer.Current.GetHashCode(environment);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
