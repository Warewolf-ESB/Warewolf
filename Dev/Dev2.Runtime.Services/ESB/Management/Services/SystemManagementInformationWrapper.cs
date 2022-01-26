using Dev2.Runtime.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
