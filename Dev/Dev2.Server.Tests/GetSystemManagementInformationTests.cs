/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Services.ESB.Management.Services;
using Dev2.Runtime.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Management;
using System.Runtime.InteropServices;

namespace Dev2.Server.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class GetSystemManagementInformationTests
    {
        private ISystemManagementInformationFactory _systemManagementInformationFactory = new SystemManagementInformationFactory();
        private Runtime.Services.ESB.Management.Services.ManagementObject _managementObject = new Runtime.Services.ESB.Management.Services.ManagementObject();

        [TestMethod]
        [Owner("Tsumbo Mbedzi")]
        public void GetSystemManagementInformation_GetNumberOfCores_Should_Success()
        {
            var systemManagementInformationWrapper = _systemManagementInformationFactory.GetNumberOfCores();
            var getSystemManagementInformation = systemManagementInformationWrapper.GetNumberOfCores();

            var numOfCores = getSystemManagementInformation.GetNumberOfCores();
            var actualNumOfCores = GetActualNumberOfCores();
            Assert.AreEqual(numOfCores, actualNumOfCores);
        }

        [TestMethod]
        [Owner("Tsumbo Mbedzi")]
        public void GetSystemManagementInformation_GetNumberOfCores_Should_Throw()
        {
            _managementObject.OSPlatform = GetOperatingSystem();
            var getSystemManagementInformation = new GetSystemManagementInformation(_managementObject);

            var numOfCores = getSystemManagementInformation.GetNumberOfCores();
            Assert.AreEqual(numOfCores, 0);
        }

        private int GetActualNumberOfCores()
        {
            var coreCount = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _managementObject.ManagementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_Processor");

                using (_managementObject.ManagementObjectSearcher)
                {
                    using (_managementObject.ManagementObjectCollection = _managementObject.ManagementObjectSearcher.Get())
                        foreach (var item in _managementObject.ManagementObjectCollection)
                        {
                            coreCount += int.Parse(item["NumberOfCores"].ToString());
                        }
                }
            }

            return coreCount;
        }

        public static OSPlatform GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }

            throw new Exception("Cannot determine operating system!");
        }
    }
}
