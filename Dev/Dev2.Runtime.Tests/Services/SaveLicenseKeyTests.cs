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
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Licensing;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveLicenseKeyTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            //------------Execute Test---------------------------
            var resId = saveLicenseKey.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(nameof(SaveLicenseKey), saveLicenseKey.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            //------------Execute Test---------------------------
            var dynamicService = saveLicenseKey.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_NullValues_HasError_IsTrue_ReturnsUnRegisteredLicenseInfo()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            GlobalConstants.ApiKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            GlobalConstants.SiteName = "warewolf-test";
            GlobalConstants.LicenseSubscriptionId = "None";
            GlobalConstants.LicensePlanId = "NotActive";
            //------------Execute Test---------------------------
            var jsonResult = saveLicenseKey.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
            var res = serializer.Deserialize<ILicenseData>(result.Message);
            Assert.AreEqual("Unknown",res.CustomerId);
            Assert.IsNotNull("NotActive",res.PlanId);
            Assert.IsFalse(res.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicensePlanId,res.PlanId);
            Assert.AreEqual(GlobalConstants.IsLicensed,res.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicenseCustomerId,res.CustomerId);
            Assert.AreEqual(GlobalConstants.LicenseSubscriptionId,res.SubscriptionId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_Success_ReturnsLicenceInfo()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            GlobalConstants.LicenseCustomerId = "16BjmNSXISIQjctO";
            GlobalConstants.LicenseSubscriptionId = "16BjmNSXISIQjctO";
            GlobalConstants.LicensePlanId = "developer";
            GlobalConstants.ApiKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            GlobalConstants.SiteName = "warewolf-test";
            var data = new LicenseData
            {
                CustomerId = GlobalConstants.LicenseCustomerId ,
                PlanId =  GlobalConstants.LicensePlanId,
                SubscriptionId =  GlobalConstants.LicenseSubscriptionId
            };
            var licenseData = serializer.Serialize<ILicenseData>(data).ToStringBuilder();

            var values = new Dictionary<string, StringBuilder>
            {
                { "LicenseData", licenseData}
            };

            //------------Execute Test---------------------------
            var jsonResult = saveLicenseKey.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
            var res = serializer.Deserialize<ILicenseData>(result.Message);
            Assert.AreEqual(data.CustomerId,res.CustomerId);
            Assert.AreEqual(data.SubscriptionId,res.SubscriptionId);
            Assert.AreEqual(data.PlanId,res.PlanId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_HasError_MissingCustomerId_ReturnsUnRegisteredLicenseInfo()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var data = new LicenseData
            {
                CustomerFirstName = "Customer ABC",
                PlanId = "Developer"
            };
            var licenseData = serializer.Serialize<ILicenseData>(data).ToStringBuilder();

            var values = new Dictionary<string, StringBuilder>
            {
                { "LicenseData", licenseData}
            };
            GlobalConstants.ApiKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            GlobalConstants.SiteName = "warewolf-test";
            GlobalConstants.LicenseSubscriptionId = "None";
            GlobalConstants.LicensePlanId = "UnRegistered";
            //------------Execute Test---------------------------
            var jsonResult = saveLicenseKey.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
            var res = serializer.Deserialize<ILicenseData>(result.Message);
            Assert.AreEqual("Unknown",res.CustomerId);
            Assert.IsNotNull("UnRegistered",res.PlanId);
            Assert.IsFalse(res.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicensePlanId,res.PlanId);
            Assert.AreEqual(GlobalConstants.IsLicensed,res.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicenseCustomerId,res.CustomerId);
            Assert.AreEqual(GlobalConstants.LicenseSubscriptionId,res.SubscriptionId);
        }
    }
}