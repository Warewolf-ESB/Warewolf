
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
using System.Windows;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Providers.Events;
using Dev2.Studio.Core.AppResources.Converters;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DeployViewConnectedToVisiblityConverterTest
    {
        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("DeployViewConnectedToVisiblityConverter")]
        public void DeployViewConnectedToVisiblityConverter_Convert_IsConnectedIsFalse_VisibilityIsCollapsed()
        {
            //Arrange
            var converter = new DeployViewConnectedToVisiblityConverter();
            Mock<IEnvironmentConnection> mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(m => m.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            mockEnvironmentConnection.Setup(m => m.IsConnected).Returns(false);
            var studioRepo = new Mock<IStudioResourceRepository>().Object;
            IEnvironmentModel environmentModel = new EnvironmentModel(Guid.NewGuid(), mockEnvironmentConnection.Object,studioRepo);

            //Act
            var actual = (Visibility)converter.Convert(environmentModel, typeof(bool), null, null);
            //Assert
            Assert.AreEqual(Visibility.Collapsed, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("DeployViewConnectedToVisiblityConverter")]
        public void DeployViewConnectedToVisiblityConverter_Convert_IsConnectedIsTrue_VisibilityIsCollapsed()
        {
            //Arrange
            var converter = new DeployViewConnectedToVisiblityConverter();
            Mock<IEnvironmentConnection> mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(m => m.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            mockEnvironmentConnection.Setup(m => m.IsConnected).Returns(true);
            IEnvironmentModel environmentModel = new EnvironmentModel(Guid.NewGuid(), mockEnvironmentConnection.Object, new Mock<IStudioResourceRepository>().Object);

            //Act
            var actual = (Visibility)converter.Convert(environmentModel, typeof(bool), null, null);
            //Assert
            Assert.AreEqual(Visibility.Visible, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("DeployViewConnectedToVisiblityConverter")]
        public void DeployViewConnectedToVisiblityConverter_Convert_EnvironmentModelIsNull_VisibilityIsCollapsed()
        {
            //Arrange
            var converter = new DeployViewConnectedToVisiblityConverter();
            //Act
            var actual = (Visibility)converter.Convert(null, typeof(bool), null, null);
            //Assert
            Assert.AreEqual(Visibility.Collapsed, actual);
        }
    }
}
