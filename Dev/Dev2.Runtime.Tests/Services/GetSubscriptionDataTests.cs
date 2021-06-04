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
    public class GetSubscriptionDataTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetSubscriptionData))]
        public void GetSubscriptionData_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var getSubscriptionData = new GetSubscriptionData();
            //------------Execute Test---------------------------
            var resId = getSubscriptionData.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveSubscriptionData))]
        public void GetSubscriptionData_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var getSubscriptionData = new GetSubscriptionData();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(nameof(GetSubscriptionData), getSubscriptionData.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetSubscriptionData))]
        public void GetSubscriptionData_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var getSubscriptionData = new GetSubscriptionData();
            //------------Execute Test---------------------------
            var dynamicService = getSubscriptionData.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetSubscriptionData))]
        public void GetSubscriptionData_Execute_Success_Returns_SubscriptionData()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder>();

            var resultSubscriptionData = new SubscriptionData
            {
                CustomerLastName = "Dom",
                CustomerFirstName = "john",
                CustomerEmail = "john2@user.com",
                PlanId = "developer",
                Status = SubscriptionStatus.Active,
                CustomerId = "asdsadsdsadsad",
                SubscriptionId = "asdsadsdsadsad",
                NoOfCores = 1,
                IsLicensed = true
            };

            var mockSubscriptionProvider = new Mock<ISubscriptionProvider>();
            mockSubscriptionProvider.Setup(o => o.SubscriptionId).Returns("16BjmNSXISIQjctO");
            mockSubscriptionProvider.Setup(o => o.PlanId).Returns("developer");
            mockSubscriptionProvider.Setup(o => o.CustomerId).Returns("16BjmNSXISIQjctO");
            mockSubscriptionProvider.Setup(o => o.Status).Returns(SubscriptionStatus.Active);
            mockSubscriptionProvider.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionProvider.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var mockSubscriptionData = new Mock<ISubscriptionData>();
            var subscriptionSiteName = "16BjmNSXISIQjctO";
            var subscriptionId = "16BjmNSXISIQjctO";
            var subscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns(subscriptionSiteName);

            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns(subscriptionKey);

            mockSubscriptionProvider.Setup(o => o.SubscriptionId).Returns(subscriptionId);

            var mockWarewolfLicense = new Mock<IWarewolfLicense>();
            mockWarewolfLicense.Setup(o => o.RetrievePlan(subscriptionId, subscriptionKey, subscriptionSiteName))
                .Returns(resultSubscriptionData);

            //------------Execute Test---------------------------
            var getSubscriptionData = new GetSubscriptionData(mockWarewolfLicense.Object, mockSubscriptionProvider.Object);
            var jsonResult = getSubscriptionData.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);

            //------------Assert Results-------------------------
            var data = serializer.Deserialize<ISubscriptionData>(result.Message);
            Assert.IsNotNull(data.CustomerFirstName);
            Assert.IsNotNull(data.CustomerLastName);
            Assert.IsNotNull(data.CustomerEmail);
            Assert.IsNotNull(data.CustomerId);
            Assert.IsNotNull(data.SubscriptionId);
            Assert.IsNotNull(data.PlanId);
            Assert.IsNotNull(data.Status);
            Assert.IsTrue(data.IsLicensed);
        }
    }
}