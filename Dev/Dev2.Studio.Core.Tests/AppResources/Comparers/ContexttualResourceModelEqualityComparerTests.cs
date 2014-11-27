
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
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.AppResources.Comparers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ContexttualResourceModelEqualityComparerTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ContexttualResourceModelEqualityComparer_Equals")]
        public void ContexttualResourceModelEqualityComparer_Equals_EnvironmentDifferent_NotEqual()
        {
            const string resourceName = "Resource Name";

            var firstResource = new Mock<IContextualResourceModel>();
            var secondResource = new Mock<IContextualResourceModel>();
            var firstEnvironment = new Mock<IEnvironmentModel>();
            var secondEnvironment = new Mock<IEnvironmentModel>();
            var firstConnection = new Mock<IEnvironmentConnection>();
            var secondConnection = new Mock<IEnvironmentConnection>();

            firstConnection.Setup(conn => conn.AppServerUri).Returns(new Uri("http://10.0.0.1"));
            secondConnection.Setup(conn => conn.AppServerUri).Returns(new Uri("http://10.0.0.2"));
            firstEnvironment.Setup(env => env.Connection).Returns(firstConnection.Object);
            secondEnvironment.Setup(env => env.Connection).Returns(secondConnection.Object);
            firstResource.Setup(res => res.Environment).Returns(firstEnvironment.Object);
            firstResource.Setup(res => res.ResourceName).Returns(resourceName);
            secondResource.Setup(res => res.Environment).Returns(secondEnvironment.Object);
            secondResource.Setup(res => res.ResourceName).Returns(resourceName);

            //------------Execute Test---------------------------
            var actual = ContexttualResourceModelEqualityComparer.Current.Equals(firstResource.Object, secondResource.Object);

            // Assert NotEqual
            Assert.IsFalse(actual, "Equity comparer found contextual resources with different environments to be equal");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ContexttualResourceModelEqualityComparer_Equals")]
        public void ContexttualResourceModelEqualityComparer_Equals_EnvironmentNotDifferent_AreEqual()
        {
            const string resourceName = "Resource Name";

            var firstResource = new Mock<IContextualResourceModel>();
            var secondResource = new Mock<IContextualResourceModel>();
            var sameEnvironment = new Mock<IEnvironmentModel>();
            sameEnvironment.Setup(e => e.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);

            firstResource.Setup(res => res.Environment).Returns(sameEnvironment.Object);
            firstResource.Setup(res => res.ResourceName).Returns(resourceName);
            secondResource.Setup(res => res.Environment).Returns(sameEnvironment.Object);
            secondResource.Setup(res => res.ResourceName).Returns(resourceName);

            //------------Execute Test---------------------------
            var actual = ContexttualResourceModelEqualityComparer.Current.Equals(firstResource.Object, secondResource.Object);

            // Assert Are Equal
            Assert.IsTrue(actual, "Equity comparer found contextual resources with different environments to be equal");
        }
    }
}
