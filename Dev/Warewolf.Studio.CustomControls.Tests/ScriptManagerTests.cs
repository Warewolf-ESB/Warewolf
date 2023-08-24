/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using Dev2;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Licensing;

namespace Warewolf.Studio.CustomControls.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ScriptManagerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_RetrieveSubscription()
        {
            var subscriptionData = new SubscriptionData
            {
                PlanId = "planId",
                NoOfCores = 2,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email"
            };
            var jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(subscriptionData);

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.RetrieveSubscription()).Returns(payload);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.ActiveServer).Returns(mockServer.Object);

            CustomContainer.Register(mockShellViewModel.Object);

            var scriptManager = new ScriptManager(new Mock<IView>().Object);
            var subscription = scriptManager.RetrieveSubscription();
            var data = jsonSerializer.Deserialize<ISubscriptionData>(subscription);

            Assert.AreEqual("planId", data.PlanId);
            Assert.AreEqual(2, data.NoOfCores);
            Assert.AreEqual("firstName", data.CustomerFirstName);
            Assert.AreEqual("lastName", data.CustomerLastName);
            Assert.AreEqual("email", data.CustomerEmail);

            mockResourceRepository.Verify(o => o.RetrieveSubscription(), Times.Once);

            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_RetrieveSubscription_Expect_Failure()
        {
            var subscriptionData = new SubscriptionData
            {
                PlanId = "planId",
                NoOfCores = 2,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email"
            };
            var jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(subscriptionData);

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.RetrieveSubscription()).Returns(payload);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.ActiveServer).Returns(mockServer.Object);

            var scriptManager = new ScriptManager(new Mock<IView>().Object);
            var subscription = scriptManager.RetrieveSubscription();

            Assert.AreEqual(GlobalConstants.Failed, subscription);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_CreateSubscription()
        {
            const string Email = "email";
            const string FirstName = "firstName";
            const string LastName = "lastName";
            const string PlanId = "planId";

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.CreateSubscription(It.IsAny<ISubscriptionData>())).Returns(GlobalConstants.Success);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.ActiveServer).Returns(mockServer.Object);

            CustomContainer.Register(mockShellViewModel.Object);

            var scriptManager = new ScriptManager(new Mock<IView>().Object);
            var subscription = scriptManager.CreateSubscription(Email, FirstName, LastName, PlanId);

            Assert.AreEqual(GlobalConstants.Success, subscription);

            mockResourceRepository.Verify(o => o.CreateSubscription(It.IsAny<ISubscriptionData>()), Times.Once);

            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_CreateSubscription_Expect_Failure()
        {
            const string Email = "email";
            const string FirstName = "firstName";
            const string LastName = "lastName";
            const string PlanId = "planId";

            CustomContainer.Register<IShellViewModel>(null);

            var scriptManager = new ScriptManager(new Mock<IView>().Object);
            var subscription = scriptManager.CreateSubscription(Email, FirstName, LastName, PlanId);

            Assert.AreEqual(GlobalConstants.Failed, subscription);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_GetSourceUri_Register()
        {
            var curDir = Directory.GetCurrentDirectory();
            var url = new Uri($"file:///{curDir}/LicenseRegistration.html");

            var sourceUri = ScriptManager.GetSourceUri("Register");

            Assert.AreEqual(url, sourceUri);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_GetSourceUri_Manage()
        {
            var curDir = Directory.GetCurrentDirectory();
            var url = new Uri($"file:///{curDir}/ManageRegistration.html");

            var sourceUri = ScriptManager.GetSourceUri("Manage");

            Assert.AreEqual(url, sourceUri);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_GetSourceUri_Default_Null()
        {
            var sourceUri = ScriptManager.GetSourceUri("");
            Assert.IsNull(sourceUri);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_CloseBrowser()
        {
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.UpdateStudioLicense(false)).Verifiable();

            CustomContainer.Register(mockShellViewModel.Object);

            var scriptManager = new ScriptManager(new Mock<IView>().Object);
            scriptManager.CloseBrowser();

            mockShellViewModel.Verify(o => o.UpdateStudioLicense(false), Times.Once);

            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_CreateSubscriptionWithId()
        {
            const string Email = "email";
            const string SubscriptionId = "firstName";


            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.CreateSubscription(It.IsAny<ISubscriptionData>())).Returns(GlobalConstants.Success);

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.ActiveServer).Returns(mockServer.Object);

            CustomContainer.Register(mockShellViewModel.Object);

            var scriptManager = new ScriptManager(new Mock<IView>().Object);
            var subscription = scriptManager.CreateSubscriptionWithId(Email, SubscriptionId);

            Assert.AreEqual(GlobalConstants.Success, subscription);

            mockResourceRepository.Verify(o => o.CreateSubscription(It.IsAny<ISubscriptionData>()), Times.Once);

            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ScriptManager))]
        public void ScriptManager_CreateSubscriptionWithId_Expect_Failure()
        {
            const string Email = "email";
            const string SubscriptionId = "subscriptionId";

            CustomContainer.Register<IShellViewModel>(null);

            var scriptManager = new ScriptManager(new Mock<IView>().Object);
            var subscription = scriptManager.CreateSubscriptionWithId(Email, SubscriptionId);

            Assert.AreEqual(GlobalConstants.Failed, subscription);
        }
    }
}