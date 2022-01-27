/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Services.Interfaces;
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
            var managementObject = new WarewolfManagementObject();

            if(_osPlatform == OSPlatform.Windows)
            {
                managementObject.OSPlatform = _osPlatform;
                managementObject.ObjectQuery = new ObjectQuery("SELECT * FROM Win32_Processor");
                managementObject.OperationObject = "NumberOfCores";

                return new GetSystemManagementInformation(managementObject);
            }

            return new GetSystemManagementInformation();
        }
    }

    public class WarewolfManagementObject
    {
        public ManagementObjectSearcher ManagementObjectSearcher { get; set;}
        public IManagementObjectSearcherFactory ManagementObjectSearcherFactory { get; set; }
        public OSPlatform OSPlatform { get; set; }
        public ObjectQuery ObjectQuery { get; set; }
        public string OperationObject { get; set; }
    }
}
