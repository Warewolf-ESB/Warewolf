using Dev2.Common;
using Dev2.Runtime.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.Services.ESB.Management.Services
{
    public class GetSystemManagementInformation : IGetSystemManagementInformation
    {
        public int GetNumberOfCores()
        {
            var coreCount = 0;

            try
            {
                using (ManagementObjectCollection managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                {
                    foreach (var item in managementObjectSearcher)
                    {
                        coreCount += int.Parse(item["NumberOfCores"].ToString());
                    }
                }
            }
            catch(Exception err)
            {
                Dev2Logger.Warn(err.Message, GlobalConstants.WarewolfWarn);
            }

            return coreCount;
        }
    }
}
