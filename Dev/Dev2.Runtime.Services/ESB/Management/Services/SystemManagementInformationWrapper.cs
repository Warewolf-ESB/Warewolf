/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Services.Interfaces;
using System;
using System.Management;
using System.Runtime.InteropServices;

namespace Dev2.Runtime.Services.ESB.Management.Services
{
    public class SystemManagementInformationWrapper : ISystemManagementInformationWrapper
    {
        private readonly OSPlatform _osPlatform;
                
        public SystemManagementInformationWrapper(OSPlatform osPlatform)
        {
            _osPlatform = osPlatform;
        }
        public IGetSystemManagementInformation GetNumberOfCores()
        {
            var managementObject = new ManagementObject();

            if(_osPlatform == OSPlatform.Windows)
            {
                managementObject.ManagementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_Processor");
                managementObject.OSPlatform = _osPlatform;

                return new GetSystemManagementInformation(managementObject);
            }

            throw new Exception("Cannot determine operating system!");

        }
    }

    public class ManagementObject
    {
        public ManagementObjectSearcher ManagementObjectSearcher { get; set;}
        public ManagementObjectCollection ManagementObjectCollection { get; set; }
        public OSPlatform OSPlatform { get; set; }
    }
}
