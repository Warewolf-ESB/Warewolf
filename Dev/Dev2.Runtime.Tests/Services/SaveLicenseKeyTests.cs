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
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.License;

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
        public void SaveLicenseKey_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveLicenseKey.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_Success()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var data = new LicenseData
            {
                CustomerId ="qwertyuioplkjhgfd123",
                Customer = "Customer ABC",
                PlanId = "Developer"
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
            Assert.AreEqual("Success",result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_HasError_MissingPlanId()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var data = new LicenseData
            {
                CustomerId ="qwertyuioplkjhgfd123",
                Customer = "Customer ABC"
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
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("License plan is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_HasError_MissingCustomerId()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var data = new LicenseData
            {
                Customer = "Customer ABC",
                PlanId = "Developer"
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
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("Customer Id is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_HasError_MissingCustomer()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var data = new LicenseData
            {
                CustomerId = "Customer ABC",
                PlanId = "Developer"
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
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("Customer name is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveLicenseKey))]
        public void SaveLicenseKey_Execute_HasError_LicenseDataEmpty()
        {
            //------------Setup for test--------------------------
            var saveLicenseKey = new SaveLicenseKey();
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var data = new LicenseData();
            var licenseData = serializer.Serialize<ILicenseData>(data).ToStringBuilder();

            var values = new Dictionary<string, StringBuilder>
            {
                { "LicenseData", licenseData}
            };
            //------------Execute Test---------------------------
            var jsonResult = saveLicenseKey.Execute(values, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("Customer name is missing", result.Message.ToString());
        }
    }
}