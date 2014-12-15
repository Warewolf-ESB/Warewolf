
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Dev2.Common.ExtMethods;
using Dev2.Data.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.EnumTests
{
    /// <summary>
    /// Summary description for enTypeOfSystemInformationTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class enTypeOfSystemInformationTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void OperatingSystemEnumExpectedDiscriptionOfOperatingSystem()
        {
            string disc = enTypeOfSystemInformationToGather.OperatingSystem.GetDescription();
            Assert.AreEqual("Operating System", disc);
        }

        [TestMethod]
        public void ServicePackEnumExpectedDiscriptionOfServicePack()
        {
            string disc = enTypeOfSystemInformationToGather.ServicePack.GetDescription();
            Assert.AreEqual("Service Pack", disc);
        }

        [TestMethod]
        public void OsBitValueEnumExpectedDiscriptionOf32Slash64Bit()
        {
            string disc = enTypeOfSystemInformationToGather.OSBitValue.GetDescription();
            Assert.AreEqual("32/64 Bit", disc);
        }

        [TestMethod]
        public void DateAndTimeEnumExpectedDiscriptionOfDateAndTime()
        {
            string disc = enTypeOfSystemInformationToGather.FullDateTime.GetDescription();
            Assert.AreEqual("Date & Time", disc);
        }

        [TestMethod]
        public void DateTimeFormatEnumExpectedDiscriptionOfDateAndTimeFormat()
        {
            string disc = enTypeOfSystemInformationToGather.DateTimeFormat.GetDescription();
            Assert.AreEqual("Date & Time Format", disc);
        }

        [TestMethod]
        public void DiskAvailableEnumExpectedDiscriptionOfDiskAvailable()
        {
            string disc = enTypeOfSystemInformationToGather.DiskAvailable.GetDescription();
            Assert.AreEqual("Disk Available (GB)", disc);
        }

        [TestMethod]
        public void DiskTotalEnumExpectedDiscriptionOfDiskTotal()
        {
            string disc = enTypeOfSystemInformationToGather.DiskTotal.GetDescription();
            Assert.AreEqual("Disk Total (GB)", disc);
        }

        [TestMethod]
        public void MemoryAvailableEnumExpectedDiscriptionOfMemoryAvailable()
        {
            string disc = enTypeOfSystemInformationToGather.PhysicalMemoryAvailable.GetDescription();
            Assert.AreEqual("RAM Available (MB)", disc);
        }

        [TestMethod]
        public void MemoryTotalEnumExpectedDiscriptionOfMemoryTotal()
        {
            string disc = enTypeOfSystemInformationToGather.PhysicalMemoryTotal.GetDescription();
            Assert.AreEqual("RAM Total (MB)", disc);
        } 

        [TestMethod]
        public void CPUAvailableEnumExpectedDiscriptionOfCPUAvailable()
        {
            string disc = enTypeOfSystemInformationToGather.CPUAvailable.GetDescription();
            Assert.AreEqual("CPU Available", disc);
        }

        [TestMethod]
        public void CPUTotalEnumExpectedDiscriptionOfCPUTotal()
        {
            string disc = enTypeOfSystemInformationToGather.CPUTotal.GetDescription();
            Assert.AreEqual("CPU Total", disc);
        }

        [TestMethod]
        public void LanguageEnumExpectedDiscriptionOfLanguage()
        {
            string disc = enTypeOfSystemInformationToGather.Language.GetDescription();
            Assert.AreEqual("Language", disc);
        }
        
        [TestMethod]
        public void RegionEnumExpectedDiscriptionOfRegion()
        {
            string disc = enTypeOfSystemInformationToGather.Region.GetDescription();
            Assert.AreEqual("Region", disc);
        }

        [TestMethod]
        public void UserRolesEnumExpectedDiscriptionOfUserRoles()
        {
            string disc = enTypeOfSystemInformationToGather.UserRoles.GetDescription();
            Assert.AreEqual("User Roles", disc);
        }

        [TestMethod]
        public void UserNameEnumExpectedDiscriptionOfUserName()
        {
            string disc = enTypeOfSystemInformationToGather.UserName.GetDescription();
            Assert.AreEqual("User Name", disc);
        }
        
        [TestMethod]
        public void DomainEnumExpectedDiscriptionOfDomain()
        {
            string disc = enTypeOfSystemInformationToGather.Domain.GetDescription();
            Assert.AreEqual("Domain", disc);
        }

        [TestMethod]
        public void NumberOfWarewolfAgentsEnumExpectedDiscriptionOfNumberOfWarewolfAgents()
        {
            string disc = enTypeOfSystemInformationToGather.NumberOfWarewolfAgents.GetDescription();
            Assert.AreEqual("Warewolf Agents", disc);
        }
    }
}
