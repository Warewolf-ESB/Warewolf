using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;
using Dev2.Data.Interfaces.Enums;
using System.Linq;
using Dev2.Common.State;
using Dev2.Utilities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfDotNetGatherSystemInformationActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        public void GetCorrectSystemInformation_MultipleInformationGatherShouldHaveValues()
        {
            var ob = new DsfDotNetGatherSystemInformationActivity
            {
                SystemInformationCollection = new List<GatherSystemInformationTO>
                {
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.ComputerName, "[[computerName]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.OperatingSystem, "[[operatingSystem]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.OperatingSystemVersion, "[[operatingSystemVersion]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.ServicePack, "[[servicePack]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.OSBitValue, "[[oSBitValue]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, "[[fullDateTime]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.DateTimeFormat, "[[dateTimeFormat]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.DiskAvailable, "[[diskAvailable]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.DiskTotal, "[[diskTotal]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.VirtualMemoryAvailable, "[[virtualMemoryAvailable]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.VirtualMemoryTotal, "[[virtualMemoryTotal]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.PhysicalMemoryAvailable, "[[physicalMemoryAvailable]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.PhysicalMemoryTotal, "[[physicalMemoryTotal]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[cPUAvailable]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUTotal, "[[cPUTotal]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.Language, "[[language]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.Region, "[[region]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.UserRoles, "[[userRoles]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.UserName, "[[userName]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.Domain, "[[domain]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.NumberOfServerNICS, "[[numberOfServerNICS]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.MacAddress, "[[macAddress]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.GateWayAddress, "[[gateWayAddress]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.DNSAddress, "[[dNSAddress]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.IPv4Address, "[[iPv4Address]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.IPv6Address, "[[iPv6Address]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.WarewolfMemory, "[[warewolfMemory]]", 1),
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.WarewolfServerVersion, "[[warewolfServerVersion]]", 1),
                }
            };

            var env = new ExecutionEnvironment();
            var data = new Mock<IDSFDataObject>();
            data.Setup(o => o.Environment).Returns(env);
            data.Setup(o => o.IsDebugMode()).Returns(() => true);

            ob.Execute(data.Object, 0);

            var outputs = ob.GetOutputs();
            var computer = env.EvalAsListOfStrings(outputs[0], 0);

            var computerName = env.EvalAsListOfStrings("[[computerName]]", 0);
            Assert.AreEqual(System.Environment.MachineName, computerName[0], "[[computerName]]");

            var operatingSystem = env.EvalAsListOfStrings("[[operatingSystem]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(operatingSystem[0]), "[[operatingSystem]]");

            var operatingSystemVersion = env.EvalAsListOfStrings("[[operatingSystemVersion]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(operatingSystemVersion[0]), "[[operatingSystemVersion]]");

            var servicePack = env.EvalAsListOfStrings("[[servicePack]]", 0);
            Assert.AreEqual(1, servicePack.Count, "[[servicePack]]");

            var oSBitValue = env.EvalAsListOfStrings("[[oSBitValue]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(oSBitValue[0]), "[[oSBitValue]]");

            var fullDateTime = env.EvalAsListOfStrings("[[fullDateTime]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(fullDateTime[0]), "[[fullDateTime]]");

            var dateTimeFormat = env.EvalAsListOfStrings("[[dateTimeFormat]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(dateTimeFormat[0]), "[[dateTimeFormat]]");

            var diskAvailable = env.EvalAsListOfStrings("[[diskAvailable]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(diskAvailable[0]), "[[diskAvailable]]");

            var diskTotal = env.EvalAsListOfStrings("[[diskTotal]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(diskTotal[0]), "[[diskTotal]]");

            var virtualMemoryAvailable = env.EvalAsListOfStrings("[[virtualMemoryAvailable]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(virtualMemoryAvailable[0]), "[[virtualMemoryAvailable]]");

            var virtualMemoryTotal = env.EvalAsListOfStrings("[[virtualMemoryTotal]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(virtualMemoryTotal[0]), "[[virtualMemoryTotal]]");

            var physicalMemoryAvailable = env.EvalAsListOfStrings("[[physicalMemoryAvailable]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(physicalMemoryAvailable[0]), "[[physicalMemoryAvailable]]");

            var physicalMemoryTotal = env.EvalAsListOfStrings("[[physicalMemoryTotal]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(physicalMemoryTotal[0]), "[[physicalMemoryTotal]]");

            var cPUAvailable = env.EvalAsListOfStrings("[[cPUAvailable]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(cPUAvailable[0]), "[[cPUAvailable]]");

            var cPUTotal = env.EvalAsListOfStrings("[[cPUTotal]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(cPUTotal[0]), "[[cPUTotal]]");

            var language = env.EvalAsListOfStrings("[[language]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(language[0]), "[[language]]");

            var region = env.EvalAsListOfStrings("[[region]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(region[0]), "[[region]]");

            var userRoles = env.EvalAsListOfStrings("[[userRoles]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(userRoles[0]), "[[userRoles]]");

            var userName = env.EvalAsListOfStrings("[[userName]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(userName[0]), "[[userName]]");

            var domain = env.EvalAsListOfStrings("[[domain]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(domain[0]), "[[domain]]");

            var numberOfServerNICS = env.EvalAsListOfStrings("[[numberOfServerNICS]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(numberOfServerNICS[0]), "[[numberOfServerNICS]]");

            var macAddress = env.EvalAsListOfStrings("[[macAddress]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(macAddress[0]), "[[macAddress]]");

            var gateWayAddress = env.EvalAsListOfStrings("[[gateWayAddress]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(gateWayAddress[0]), "[[gateWayAddress]]");

            var dNSAddress = env.EvalAsListOfStrings("[[dNSAddress]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(dNSAddress[0]), "[[dNSAddress]]");

            var iPv4Address = env.EvalAsListOfStrings("[[iPv4Address]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(iPv4Address[0]), "[[iPv4Address]]");

            var iPv6Address = env.EvalAsListOfStrings("[[iPv6Address]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(iPv6Address[0]), "[[iPv6Address]]");

            var warewolfMemory = env.EvalAsListOfStrings("[[warewolfMemory]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(warewolfMemory[0]), "[[warewolfMemory]]");

            var warewolfServerVersion = env.EvalAsListOfStrings("[[warewolfServerVersion]]", 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(warewolfServerVersion[0]), "[[warewolfServerVersion]]");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetCorrectSystemInformation_WarewolfCPU_GatherShouldHaveValues()
        {
            var ob = new DsfDotNetGatherSystemInformationActivity
            {
                SystemInformationCollection = new List<GatherSystemInformationTO>
                {
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.WarewolfCPU, "[[warewolfCPU]]", 1)
                }
            };

            var env = new ExecutionEnvironment();
            var data = new Mock<IDSFDataObject>();
            data.Setup(o => o.Environment).Returns(env);
            data.Setup(o => o.IsDebugMode()).Returns(() => true);
            IList<string> warewolfCPU;
            int retryCount = 0;
            do
            {
                warewolfCPU = GetWarewolfCPUInfo(ob, env, data);
            }
            while (string.IsNullOrWhiteSpace(warewolfCPU[0]) && retryCount++ < 100);
            Assert.IsFalse(string.IsNullOrWhiteSpace(warewolfCPU[0]), "[[warewolfCPU]]");
        }

        static IList<string> GetWarewolfCPUInfo(DsfDotNetGatherSystemInformationActivity ob, ExecutionEnvironment env, Mock<IDSFDataObject> data)
        {
            ob.Execute(data.Object, 0);
            return env.EvalAsListOfStrings("[[warewolfCPU]]", 0);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDotNetGatherSystemInformationActivity_GetState")]
        public void DsfDotNetGatherSystemInformationActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            //------------Setup for test--------------------------
            var act = new DsfDotNetGatherSystemInformationActivity { SystemInformationCollection = systemInformationCollection };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(1, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "SystemInformationCollection",
                    Type = StateVariable.StateType.InputOutput,
                     Value= ActivityHelper.GetSerializedStateValueFromCollection(systemInformationCollection)
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}
