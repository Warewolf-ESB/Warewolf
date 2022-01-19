using Dev2.Runtime.Services.ESB.Management.Services;
using Dev2.Runtime.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Server.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class GetSystemManagementInformationTest
    {
        [TestMethod]
        [Owner("Tsumbo Mbedzi")]
        public void GetNumberOfCores()
        {
            var isFailed = false;
            var numOfCores = GetNumberOfCores(isFailed);
            var actualNumOfCores = GetActualNumberOfCores();
            Assert.AreEqual(numOfCores, actualNumOfCores);
        }

        public int GetNumberOfCores(bool isFailed)
        {
            var getSystemManagementInformation = new GetSystemManagementInformation();
            return getSystemManagementInformation.GetNumberOfCores();    

        }

        private int GetActualNumberOfCores()
        {
            var coreCount = 0;
            using (ManagementObjectCollection managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                foreach (var item in managementObjectSearcher)
                {
                    coreCount += int.Parse(item["NumberOfCores"].ToString());
                }
            }

            return coreCount;
        }
    }
}
