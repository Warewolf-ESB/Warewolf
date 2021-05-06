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
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Licensing;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class GetLicenseKeyTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetLicenseKey))]
        public void GetLicenseKey_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var getLicenseKey = new GetLicenseKey();
            //------------Execute Test---------------------------
            var resId = getLicenseKey.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void GetLicenseKey_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var getLicenseKey = new GetLicenseKey();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(nameof(GetLicenseKey), getLicenseKey.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetLicenseKey))]
        public void GetLicenseKey_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var getLicenseKey = new GetLicenseKey();
            //------------Execute Test---------------------------
            var dynamicService = getLicenseKey.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetLicenseKey))]
        public void GetLicenseKey_Execute_Success_Returns_LicenseData()
        {
            //------------Setup for test--------------------------
            var getLicenseKey = new GetLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder>();

            GlobalConstants.LicenseCustomerId = "AzZlx3SU5eKkLDer";
            GlobalConstants.LicensePlanId = "bronze";
            GlobalConstants.ApiKey = "test_cuS2mLPoVv50eDQju3mquk0aC0UM3YYor";
            GlobalConstants.SiteName = "warewolfio-test";

            //------------Execute Test---------------------------
            var jsonResult = getLicenseKey.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);

            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
            var data = serializer.Deserialize<ILicenseData>(result.Message);
            Assert.IsNotNull(data.CustomerFirstName);
            Assert.IsNotNull(data.CustomerLastName);
            Assert.IsNotNull(data.CustomerEmail);
            Assert.IsNotNull(data.CustomerId);
            Assert.IsNotNull(data.PlanId);
            Assert.IsNotNull(data.TrialEnd);
            Assert.IsNotNull(data.Status);
            Assert.IsTrue(data.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicensePlanId,data.PlanId);
            Assert.AreEqual(GlobalConstants.IsLicensed,data.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicenseCustomerId,data.CustomerId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetLicenseKey))]
        public void GetLicenseKey_Execute_LicenseCustomerId_Null_Fails()
        {
            //------------Setup for test--------------------------
            var getLicenseKey = new GetLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var values = new Dictionary<string, StringBuilder>();
            GlobalConstants.LicenseCustomerId = null;
            GlobalConstants.SiteName = "SiteName";
            GlobalConstants.ApiKey = "ApiKey";

            //------------Execute Test---------------------------
            var jsonResult = getLicenseKey.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            var data = serializer.Deserialize<ILicenseData>(result.Message);
            Assert.AreEqual("Unknown",data.CustomerId);
            Assert.IsNotNull("UnRegistered",data.PlanId);
            Assert.IsFalse(data.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicensePlanId,data.PlanId);
            Assert.AreEqual(GlobalConstants.IsLicensed,data.IsLicensed);
            Assert.AreEqual(GlobalConstants.LicenseCustomerId,data.CustomerId);
        }

    }
}