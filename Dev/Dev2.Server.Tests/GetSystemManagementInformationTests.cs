/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Services.ESB.Management.Services;
using Dev2.Runtime.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Management;
using System.Runtime.InteropServices;

namespace Dev2.Server.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class GetSystemManagementInformationTests
    {
        private ISystemManagementInformationFactory _systemManagementInformationFactory = new SystemManagementInformationFactory();
        private IManagementObjectSearcherFactory _managementObjectSearcherFactory = new ManagementObjectSearcherFactory();
     
        [TestMethod]
        [Owner("Tsumbo Mbedzi")]
        public void GetSystemManagementInformation_GetNumberOfCores_Should_Success()
        {
            var systemManagementInformationWrapper = _systemManagementInformationFactory.GetNumberOfCores();
            var getSystemManagementInformation = systemManagementInformationWrapper.GetNumberOfCores();

            var numOfCores = getSystemManagementInformation.GetNumberOfCores();
            var actualNumOfCores = GetActualNumberOfCores(getSystemManagementInformation);
            Assert.AreEqual(numOfCores, actualNumOfCores);
        }

        [TestMethod]
        [Owner("Tsumbo Mbedzi")]
        public void GetSystemManagementInformation_GetNumberOfCores_Failed_Incorrect_ObjectQuery()
        {
            var _managementObject = new WarewolfManagementObject
            {
                OSPlatform = OSPlatform.Windows,
                ObjectQuery = new ObjectQuery("SELECT * FROM Win32_Processors"),
                OperationObject = "NumberOfCores"
            };

            _managementObject.OSPlatform = GetOperatingSystem();
            var getSystemManagementInformation = new GetSystemManagementInformation(_managementObject);

            var numOfCores = getSystemManagementInformation.GetNumberOfCores();
            Assert.AreEqual(numOfCores, 0);
        }

        [TestMethod]
        [Owner("Tsumbo Mbedzi")]
        public void GetSystemManagementInformation_GetNumberOfCores_Failed_Incorrect_Oparetion()
        {
            var _managementObject = new WarewolfManagementObject
            {
                OSPlatform = OSPlatform.Windows,
                ObjectQuery = new ObjectQuery("SELECT * FROM Win32_Processor"),
                OperationObject = "NumberOfProcessors"
            };

            _managementObject.OSPlatform = GetOperatingSystem();
            var getSystemManagementInformation = new GetSystemManagementInformation(_managementObject);

            var numOfCores = getSystemManagementInformation.GetNumberOfCores();
            Assert.AreEqual(numOfCores, 0);
        }

        private int GetActualNumberOfCores(IGetSystemManagementInformation getSystemManagementInformation)
        {
            var coreCount = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var _managementObject = new WarewolfManagementObject
                {
                    OSPlatform = OSPlatform.Windows,
                    ObjectQuery = new ObjectQuery("SELECT * FROM Win32_Processor"),
                    OperationObject = "NumberOfCores"
                };

                using (var managementObjectSearcherWrapper = _managementObjectSearcherFactory.New(_managementObject.OSPlatform, _managementObject.ObjectQuery))
                {
                    foreach (var item in managementObjectSearcherWrapper.Get())
                    {
                        coreCount += int.Parse(item[_managementObject.OperationObject].ToString());
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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }
            else
            {
                return new OSPlatform();
            }
        }
    }
}
