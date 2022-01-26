using Dev2.Runtime.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.Services.ESB.Management.Services
{
    public class SystemManagementInformationFactory : ISystemManagementInformationFactory
    {
        public ISystemManagementInformationWrapper GetNumberOfCores()
        {
            return new SystemManagementInformationWrapper(GetOperatingSystem());
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
