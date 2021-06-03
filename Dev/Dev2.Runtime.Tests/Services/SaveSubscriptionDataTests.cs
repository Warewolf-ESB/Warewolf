﻿/*
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

            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var subscriptionData = new SubscriptionData
            {
                PlanId = "developer",
                NoOfCores = 1,
                CustomerFirstName = "firstName",
                CustomerLastName = "lastName",
                CustomerEmail = "email@email.com"
            };
            var serializedSubsciptionData = serializer.SerializeToBuilder(subscriptionData);
            var mockSerializer =  new Mock<IBuilderSerializer>();
            mockSerializer.Setup(o => o.Deserialize<SubscriptionData>(serializedSubsciptionData)).Returns(subscriptionData);

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
                SubscriptionKey="test_VMxitsiobdAyth62k0DiqpAUKocG6sV3"
            };

            var values = new Dictionary<string, StringBuilder>
            {
                { Warewolf.Service.SaveSubscriptionData.SubscriptionData, serializedSubsciptionData }
            };
            var mockWarewolfLicense = new Mock<IWarewolfLicense>();
            mockWarewolfLicense.Setup(o => o.CreatePlan(It.IsAny<ISubscriptionData>())).Returns(subscriptionDataPlanCreated);

            var serializedsubscriptionDataPlanCreated = serializer.SerializeToBuilder(subscriptionDataPlanCreated);
            mockSerializer.Setup(p => p.SerializeToBuilder(subscriptionDataPlanCreated)).Returns(serializedsubscriptionDataPlanCreated);

            mockSerializer.Setup(p => p.SerializeToBuilder(serializedsubscriptionDataPlanCreated)).Returns(new StringBuilder("message"));

            //------------Execute Test---------------------------
            var saveSubscriptionData = new SaveSubscriptionData(mockSerializer.Object,mockWarewolfLicense.Object,mockSubscriptionData.Object);
            var jsonResult = saveSubscriptionData.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
            var res = serializer.Deserialize<ISubscriptionData>(result.Message);
            Assert.IsNotNull(res.CustomerId);
            Assert.IsNotNull(res.SubscriptionId);
            Assert.AreEqual(subscriptionData.PlanId, res.PlanId);
        }
    }
}