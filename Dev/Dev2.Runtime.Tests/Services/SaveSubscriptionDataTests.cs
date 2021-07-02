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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Communication;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Subscription;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Enums;
using Warewolf.Licensing;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class SaveSubscriptionDataTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveSubscriptionData = new SaveSubscriptionData();
            //------------Execute Test---------------------------
            var resId = saveSubscriptionData.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveSubscriptionData = new SaveSubscriptionData();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(nameof(SaveSubscriptionData), saveSubscriptionData.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var subscriptionData = new SaveSubscriptionData();
            //------------Execute Test---------------------------
            var dynamicService = subscriptionData.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_Execute_Success_Returns_SubscriptionData()
        {
            //------------Setup for test--------------------------

            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var machineName = "machineName";
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var subscriptionData = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com",
                MachineName = machineName
            };
            var serializedSubsciptionData = serializer.SerializeToBuilder(subscriptionData);

            subscriptionData.SubscriptionKey = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionSiteName = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            var subscriptionDataPlanCreated = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com",
                SubscriptionId = "asdsad",
                CustomerId = "asdasdsd",
                Status = SubscriptionStatus.Active,
                IsLicensed = true,
                SubscriptionSiteName = "16BjmNSXISIQjctO",
                SubscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3",
                MachineName = "machineName"
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { Warewolf.Service.SaveSubscriptionData.SubscriptionData, serializedSubsciptionData }
            };
            var mockWarewolfLicense = new Mock<IWarewolfLicense>();
            mockWarewolfLicense.Setup(o => o.CreatePlan(It.IsAny<ISubscriptionData>())).Returns(subscriptionDataPlanCreated);
            mockWarewolfLicense.Setup(o => o.SubscriptionExists(subscriptionData)).Returns(false);
            //------------Execute Test---------------------------
            var mockSecurityProvider = new Mock<ISubscriptionProvider>();
            mockSecurityProvider.Setup(o => o.SaveSubscriptionData(subscriptionDataPlanCreated));

            var mockSerializer = new Mock<IBuilderSerializer>();
            mockSerializer.Setup(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData)).Returns(subscriptionData);

            var expectedPassedResult = new StringBuilder("Passed");
            mockSerializer.Setup(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>())).Returns(expectedPassedResult);

            var saveSubscriptionData = new SaveSubscriptionData(
                mockSerializer.Object,
                mockWarewolfLicense.Object,
                mockSecurityProvider.Object,
                machineName);
            var jsonResult = saveSubscriptionData.Execute(values, workspaceMock.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedPassedResult, jsonResult);

            mockWarewolfLicense.Verify(o => o.CreatePlan(It.IsAny<ISubscriptionData>()), Times.Once);
            mockWarewolfLicense.Verify(o => o.SubscriptionExists(It.IsAny<ISubscriptionData>()), Times.Once);
            mockSecurityProvider.Verify(o => o.SaveSubscriptionData(subscriptionDataPlanCreated), Times.Once);
            mockSerializer.Verify(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData), Times.Once);
            mockSerializer.Verify(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>()), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_WithSubscriptionId_Execute_Success_Returns_SubscriptionData()
        {
            //------------Setup for test--------------------------

            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var subscriptionSiteName = "16BjmNSXISIQjctO";
            var subscriptionId = "16BjmNSXISIQjctO";
            var subscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            var machineName = "machineName";
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var subscriptionData = new SubscriptionData
            {
                SubscriptionId = "16BjmNSXISIQjctO",
                CustomerEmail = "email@email.com"
            };
            var serializedSubsciptionData = serializer.SerializeToBuilder(subscriptionData);

            subscriptionData.SubscriptionKey = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionSiteName = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            var subscriptionDataPlanCreated = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com",
                SubscriptionId = "16BjmNSXISIQjctO",
                CustomerId = "16BjmNSXISIQjctO",
                Status = SubscriptionStatus.Active,
                IsLicensed = true,
                SubscriptionSiteName = "16BjmNSXISIQjctO",
                SubscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3",
                MachineName = machineName
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { Warewolf.Service.SaveSubscriptionData.SubscriptionData, serializedSubsciptionData }
            };
            var mockWarewolfLicense = new Mock<IWarewolfLicense>();
            mockWarewolfLicense.Setup(
                    o => o.RetrievePlan(
                        subscriptionId,
                        subscriptionKey,
                        subscriptionSiteName))
                .Returns(subscriptionDataPlanCreated);

            //------------Execute Test---------------------------
            var mockSubscriptionProvider = new Mock<ISubscriptionProvider>();
            mockSubscriptionProvider.Setup(o => o.SubscriptionId).Returns("");
            mockSubscriptionProvider.Setup(o => o.PlanId).Returns("");
            mockSubscriptionProvider.Setup(o => o.CustomerId).Returns("");
            mockSubscriptionProvider.Setup(o => o.Status).Returns(SubscriptionStatus.NotActive);
            mockSubscriptionProvider.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionProvider.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            mockSubscriptionProvider.Setup(o => o.SaveSubscriptionData(subscriptionDataPlanCreated));

            var mockSerializer = new Mock<IBuilderSerializer>();
            mockSerializer.Setup(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData)).Returns(subscriptionData);

            var expectedPassedResult = new StringBuilder("Passed");
            mockSerializer.Setup(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>())).Returns(expectedPassedResult);

            var saveSubscriptionData = new SaveSubscriptionData(
                mockSerializer.Object,
                mockWarewolfLicense.Object,
                mockSubscriptionProvider.Object,
                machineName);
            var jsonResult = saveSubscriptionData.Execute(values, workspaceMock.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedPassedResult, jsonResult);

            mockWarewolfLicense.Verify(
                o => o.RetrievePlan(
                    subscriptionId,
                    subscriptionKey,
                    subscriptionSiteName),
                Times.Once);
            mockSubscriptionProvider.Verify(o => o.SaveSubscriptionData(subscriptionDataPlanCreated), Times.Once);
            mockSerializer.Verify(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData), Times.Once);
            mockSerializer.Verify(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>()), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_WithSubscriptionId_Execute_Fail_Returns_EmailsDontMatch()
        {
            //------------Setup for test--------------------------

            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var subscriptionSiteName = "16BjmNSXISIQjctO";
            var subscriptionId = "16BjmNSXISIQjctO";
            var subscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            var machineName = "machineName";
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var subscriptionData = new SubscriptionData
            {
                SubscriptionId = "16BjmNSXISIQjctO",
                CustomerEmail = "IncorrectEmail@email.com"
            };
            var serializedSubsciptionData = serializer.SerializeToBuilder(subscriptionData);

            subscriptionData.SubscriptionKey = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionSiteName = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            var subscriptionDataPlanCreated = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com",
                SubscriptionId = "16BjmNSXISIQjctO",
                CustomerId = "16BjmNSXISIQjctO",
                Status = SubscriptionStatus.Active,
                IsLicensed = true,
                SubscriptionSiteName = "16BjmNSXISIQjctO",
                SubscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3",
                MachineName = machineName
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { Warewolf.Service.SaveSubscriptionData.SubscriptionData, serializedSubsciptionData }
            };
            var mockWarewolfLicense = new Mock<IWarewolfLicense>();
            mockWarewolfLicense.Setup(
                    o => o.RetrievePlan(
                        subscriptionId,
                        subscriptionKey,
                        subscriptionSiteName))
                .Returns(subscriptionDataPlanCreated);

            //------------Execute Test---------------------------
            var mockSubscriptionProvider = new Mock<ISubscriptionProvider>();
            mockSubscriptionProvider.Setup(o => o.SubscriptionId).Returns("");
            mockSubscriptionProvider.Setup(o => o.PlanId).Returns("");
            mockSubscriptionProvider.Setup(o => o.CustomerId).Returns("");
            mockSubscriptionProvider.Setup(o => o.Status).Returns(SubscriptionStatus.NotActive);
            mockSubscriptionProvider.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionProvider.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            mockSubscriptionProvider.Setup(o => o.SaveSubscriptionData(subscriptionDataPlanCreated));

            var mockSerializer = new Mock<IBuilderSerializer>();
            mockSerializer.Setup(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData)).Returns(subscriptionData);

            var expectedFailedResult = new ExecuteMessage { HasError = true };
            expectedFailedResult.SetMessage("Email Address does not match email address for this Subscription.");
            mockSerializer.Setup(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>())).Returns(serializer.SerializeToBuilder(expectedFailedResult));

            var saveSubscriptionData = new SaveSubscriptionData(mockSerializer.Object, mockWarewolfLicense.Object, mockSubscriptionProvider.Object, machineName);
            var jsonResult = saveSubscriptionData.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual(expectedFailedResult.Message.ToString(), result.Message.ToString());

            mockWarewolfLicense.Verify(
                o => o.RetrievePlan(
                    subscriptionId,
                    subscriptionKey,
                    subscriptionSiteName),
                Times.Once);
            mockSubscriptionProvider.Verify(o => o.SaveSubscriptionData(subscriptionDataPlanCreated), Times.Never);
            mockSerializer.Verify(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData), Times.Once);
            mockSerializer.Verify(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>()), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_WithSubscriptionId_Execute_Fail_Returns_MachinesDontMatch()
        {
            //------------Setup for test--------------------------

            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var subscriptionSiteName = "16BjmNSXISIQjctO";
            var subscriptionId = "16BjmNSXISIQjctO";
            var subscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";

            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var subscriptionData = new SubscriptionData
            {
                SubscriptionId = "16BjmNSXISIQjctO",
                CustomerEmail = "IncorrectEmail@email.com"
            };
            var serializedSubsciptionData = serializer.SerializeToBuilder(subscriptionData);

            subscriptionData.SubscriptionKey = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionSiteName = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            var subscriptionDataPlanCreated = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com",
                SubscriptionId = "16BjmNSXISIQjctO",
                CustomerId = "16BjmNSXISIQjctO",
                Status = SubscriptionStatus.Active,
                IsLicensed = true,
                SubscriptionSiteName = "16BjmNSXISIQjctO",
                SubscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3",
                MachineName = "SavedMachineName"
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { Warewolf.Service.SaveSubscriptionData.SubscriptionData, serializedSubsciptionData }
            };
            var mockWarewolfLicense = new Mock<IWarewolfLicense>();
            mockWarewolfLicense.Setup(
                    o => o.RetrievePlan(
                        subscriptionId,
                        subscriptionKey,
                        subscriptionSiteName))
                .Returns(subscriptionDataPlanCreated);
            mockWarewolfLicense.Setup(o => o.SubscriptionExists(subscriptionData)).Returns(false);
            //------------Execute Test---------------------------
            var mockSubscriptionProvider = new Mock<ISubscriptionProvider>();
            mockSubscriptionProvider.Setup(o => o.SubscriptionId).Returns("");
            mockSubscriptionProvider.Setup(o => o.PlanId).Returns("");
            mockSubscriptionProvider.Setup(o => o.CustomerId).Returns("");
            mockSubscriptionProvider.Setup(o => o.Status).Returns(SubscriptionStatus.NotActive);
            mockSubscriptionProvider.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionProvider.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            mockSubscriptionProvider.Setup(o => o.SaveSubscriptionData(subscriptionDataPlanCreated));

            var mockSerializer = new Mock<IBuilderSerializer>();
            mockSerializer.Setup(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData)).Returns(subscriptionData);

            var expectedFailedResult = new ExecuteMessage { HasError = true };
            expectedFailedResult.SetMessage("This subscription is configured for a different machine. For help please contact support@warewolf.io");
            mockSerializer.Setup(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>())).Returns(serializer.SerializeToBuilder(expectedFailedResult));

            var saveSubscriptionData = new SaveSubscriptionData(mockSerializer.Object, mockWarewolfLicense.Object, mockSubscriptionProvider.Object, "WrongMachineName");
            var jsonResult = saveSubscriptionData.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual(expectedFailedResult.Message.ToString(), result.Message.ToString());

            mockWarewolfLicense.Verify(
                o => o.RetrievePlan(
                    subscriptionId,
                    subscriptionKey,
                    subscriptionSiteName),
                Times.Once);
            mockSubscriptionProvider.Verify(o => o.SaveSubscriptionData(subscriptionDataPlanCreated), Times.Never);
            mockSerializer.Verify(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData), Times.Once);
            mockSerializer.Verify(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>()), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void SaveSubscriptionData_Execute_SubscriptionExists_Fails()
        {
            //------------Setup for test--------------------------

            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var machineName = "machineName";
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var subscriptionData = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com",
                MachineName = machineName
            };
            var serializedSubsciptionData = serializer.SerializeToBuilder(subscriptionData);

            subscriptionData.SubscriptionKey = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionSiteName = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            var subscriptionDataPlanCreated = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com",
                SubscriptionId = "asdsad",
                CustomerId = "asdasdsd",
                Status = SubscriptionStatus.Active,
                IsLicensed = true,
                SubscriptionSiteName = "16BjmNSXISIQjctO",
                SubscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3",
                MachineName = "machineName"
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { Warewolf.Service.SaveSubscriptionData.SubscriptionData, serializedSubsciptionData }
            };
            var mockWarewolfLicense = new Mock<IWarewolfLicense>();
            mockWarewolfLicense.Setup(o => o.CreatePlan(It.IsAny<ISubscriptionData>())).Returns(subscriptionDataPlanCreated);
            mockWarewolfLicense.Setup(o => o.SubscriptionExists(subscriptionData)).Returns(true);
            //------------Execute Test---------------------------
            var mockSecurityProvider = new Mock<ISubscriptionProvider>();
            mockSecurityProvider.Setup(o => o.SaveSubscriptionData(subscriptionDataPlanCreated));

            var mockSerializer = new Mock<IBuilderSerializer>();
            mockSerializer.Setup(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData)).Returns(subscriptionData);

            var expectedPassedResult = new StringBuilder("Passed");
            mockSerializer.Setup(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>())).Returns(expectedPassedResult);

            var saveSubscriptionData = new SaveSubscriptionData(
                mockSerializer.Object,
                mockWarewolfLicense.Object,
                mockSecurityProvider.Object,
                machineName);
            var jsonResult = saveSubscriptionData.Execute(values, workspaceMock.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedPassedResult, jsonResult);

            mockWarewolfLicense.Verify(o => o.CreatePlan(It.IsAny<ISubscriptionData>()), Times.Never);
            mockWarewolfLicense.Verify(o => o.SubscriptionExists(It.IsAny<ISubscriptionData>()), Times.Once);
            mockSecurityProvider.Verify(o => o.SaveSubscriptionData(subscriptionDataPlanCreated), Times.Never);
            mockSerializer.Verify(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData), Times.Once);
            mockSerializer.Verify(p => p.SerializeToBuilder(It.IsAny<ExecuteMessage>()), Times.Once);
        }
    }
}