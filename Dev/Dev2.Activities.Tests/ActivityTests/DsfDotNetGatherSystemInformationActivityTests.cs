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
        public void GetCorrectSystemInformation_AllInformationGatherShouldHaveValues()
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
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.WarewolfCPU, "[[warewolfCPU]]", 1),
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
            var operatingSystem = env.EvalAsListOfStrings("[[operatingSystem]]", 0);
            var operatingSystemVersion = env.EvalAsListOfStrings("[[operatingSystemVersion]]", 0);
            var servicePack = env.EvalAsListOfStrings("[[servicePack]]", 0);
            var oSBitValue = env.EvalAsListOfStrings("[[oSBitValue]]", 0);
            var fullDateTime = env.EvalAsListOfStrings("[[fullDateTime]]", 0);
            var dateTimeFormat = env.EvalAsListOfStrings("[[dateTimeFormat]]", 0);
            var diskAvailable = env.EvalAsListOfStrings("[[diskAvailable]]", 0);
            var diskTotal = env.EvalAsListOfStrings("[[diskTotal]]", 0);
            var virtualMemoryAvailable = env.EvalAsListOfStrings("[[virtualMemoryAvailable]]", 0);
            var virtualMemoryTotal = env.EvalAsListOfStrings("[[virtualMemoryTotal]]", 0);
            var physicalMemoryAvailable = env.EvalAsListOfStrings("[[physicalMemoryAvailable]]", 0);
            var physicalMemoryTotal = env.EvalAsListOfStrings("[[physicalMemoryTotal]]", 0);
            var cPUAvailable = env.EvalAsListOfStrings("[[cPUAvailable]]", 0);
            var cPUTotal = env.EvalAsListOfStrings("[[cPUTotal]]", 0);
            var language = env.EvalAsListOfStrings("[[language]]", 0);
            var region = env.EvalAsListOfStrings("[[region]]", 0);
            var userRoles = env.EvalAsListOfStrings("[[userRoles]]", 0);
            var userName = env.EvalAsListOfStrings("[[userName]]", 0);
            var domain = env.EvalAsListOfStrings("[[domain]]", 0);
            var numberOfServerNICS = env.EvalAsListOfStrings("[[numberOfServerNICS]]", 0);
            var macAddress = env.EvalAsListOfStrings("[[macAddress]]", 0);
            var gateWayAddress = env.EvalAsListOfStrings("[[gateWayAddress]]", 0);
            var dNSAddress = env.EvalAsListOfStrings("[[dNSAddress]]", 0);
            var iPv4Address = env.EvalAsListOfStrings("[[iPv4Address]]", 0);
            var iPv6Address = env.EvalAsListOfStrings("[[iPv6Address]]", 0);
            var warewolfMemory = env.EvalAsListOfStrings("[[warewolfMemory]]", 0);
            var warewolfCPU = env.EvalAsListOfStrings("[[warewolfCPU]]", 0);
            var warewolfServerVersion = env.EvalAsListOfStrings("[[warewolfServerVersion]]", 0);

            if (System.Environment.MachineName != computerName[0] || 
                string.IsNullOrWhiteSpace(operatingSystem[0]) ||
                string.IsNullOrWhiteSpace(operatingSystemVersion[0]) ||
                1 != servicePack.Count ||
                string.IsNullOrWhiteSpace(oSBitValue[0]) ||
                string.IsNullOrWhiteSpace(fullDateTime[0]) ||
                string.IsNullOrWhiteSpace(dateTimeFormat[0]) ||
                string.IsNullOrWhiteSpace(diskAvailable[0]) ||
                string.IsNullOrWhiteSpace(diskTotal[0]) ||
                string.IsNullOrWhiteSpace(virtualMemoryAvailable[0]) ||
                string.IsNullOrWhiteSpace(virtualMemoryTotal[0]) ||
                string.IsNullOrWhiteSpace(physicalMemoryAvailable[0]) ||
                string.IsNullOrWhiteSpace(physicalMemoryTotal[0]) ||
                string.IsNullOrWhiteSpace(cPUAvailable[0]) ||
                string.IsNullOrWhiteSpace(cPUTotal[0]) ||
                string.IsNullOrWhiteSpace(language[0]) ||
                string.IsNullOrWhiteSpace(region[0]) ||
                string.IsNullOrWhiteSpace(userRoles[0]) ||
                string.IsNullOrWhiteSpace(userName[0]) ||
                string.IsNullOrWhiteSpace(domain[0]) ||
                string.IsNullOrWhiteSpace(numberOfServerNICS[0]) ||
                string.IsNullOrWhiteSpace(macAddress[0]) ||
                string.IsNullOrWhiteSpace(gateWayAddress[0]) ||
                string.IsNullOrWhiteSpace(dNSAddress[0]) ||
                string.IsNullOrWhiteSpace(iPv4Address[0]) ||
                string.IsNullOrWhiteSpace(iPv6Address[0]) ||
                string.IsNullOrWhiteSpace(warewolfMemory[0]) ||
                string.IsNullOrWhiteSpace(warewolfCPU[0]) ||
                string.IsNullOrWhiteSpace(warewolfServerVersion[0]))
            {

                ob.Execute(data.Object, 0);

                outputs = ob.GetOutputs();
                computer = env.EvalAsListOfStrings(outputs[0], 0);

                computerName = env.EvalAsListOfStrings("[[computerName]]", 0);
                operatingSystem = env.EvalAsListOfStrings("[[operatingSystem]]", 0);
                operatingSystemVersion = env.EvalAsListOfStrings("[[operatingSystemVersion]]", 0);
                servicePack = env.EvalAsListOfStrings("[[servicePack]]", 0);
                oSBitValue = env.EvalAsListOfStrings("[[oSBitValue]]", 0);
                fullDateTime = env.EvalAsListOfStrings("[[fullDateTime]]", 0);
                dateTimeFormat = env.EvalAsListOfStrings("[[dateTimeFormat]]", 0);
                diskAvailable = env.EvalAsListOfStrings("[[diskAvailable]]", 0);
                diskTotal = env.EvalAsListOfStrings("[[diskTotal]]", 0);
                virtualMemoryAvailable = env.EvalAsListOfStrings("[[virtualMemoryAvailable]]", 0);
                virtualMemoryTotal = env.EvalAsListOfStrings("[[virtualMemoryTotal]]", 0);
                physicalMemoryAvailable = env.EvalAsListOfStrings("[[physicalMemoryAvailable]]", 0);
                physicalMemoryTotal = env.EvalAsListOfStrings("[[physicalMemoryTotal]]", 0);
                cPUAvailable = env.EvalAsListOfStrings("[[cPUAvailable]]", 0);
                cPUTotal = env.EvalAsListOfStrings("[[cPUTotal]]", 0);
                language = env.EvalAsListOfStrings("[[language]]", 0);
                region = env.EvalAsListOfStrings("[[region]]", 0);
                userRoles = env.EvalAsListOfStrings("[[userRoles]]", 0);
                userName = env.EvalAsListOfStrings("[[userName]]", 0);
                domain = env.EvalAsListOfStrings("[[domain]]", 0);
                numberOfServerNICS = env.EvalAsListOfStrings("[[numberOfServerNICS]]", 0);
                macAddress = env.EvalAsListOfStrings("[[macAddress]]", 0);
                gateWayAddress = env.EvalAsListOfStrings("[[gateWayAddress]]", 0);
                dNSAddress = env.EvalAsListOfStrings("[[dNSAddress]]", 0);
                iPv4Address = env.EvalAsListOfStrings("[[iPv4Address]]", 0);
                iPv6Address = env.EvalAsListOfStrings("[[iPv6Address]]", 0);
                warewolfMemory = env.EvalAsListOfStrings("[[warewolfMemory]]", 0);
                warewolfCPU = env.EvalAsListOfStrings("[[warewolfCPU]]", 0);
                warewolfServerVersion = env.EvalAsListOfStrings("[[warewolfServerVersion]]", 0);
            }

            Assert.AreEqual(System.Environment.MachineName, computerName[0], "[[computerName]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(operatingSystem[0]), "[[operatingSystem]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(operatingSystemVersion[0]), "[[operatingSystemVersion]]");
            Assert.AreEqual(1, servicePack.Count, "[[servicePack]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(oSBitValue[0]), "[[oSBitValue]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(fullDateTime[0]), "[[fullDateTime]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(dateTimeFormat[0]), "[[dateTimeFormat]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(diskAvailable[0]), "[[diskAvailable]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(diskTotal[0]), "[[diskTotal]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(virtualMemoryAvailable[0]), "[[virtualMemoryAvailable]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(virtualMemoryTotal[0]), "[[virtualMemoryTotal]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(physicalMemoryAvailable[0]), "[[physicalMemoryAvailable]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(physicalMemoryTotal[0]), "[[physicalMemoryTotal]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(cPUAvailable[0]), "[[cPUAvailable]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(cPUTotal[0]), "[[cPUTotal]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(language[0]), "[[language]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(region[0]), "[[region]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(userRoles[0]), "[[userRoles]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(userName[0]), "[[userName]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(domain[0]), "[[domain]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(numberOfServerNICS[0]), "[[numberOfServerNICS]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(macAddress[0]), "[[macAddress]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(gateWayAddress[0]), "[[gateWayAddress]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(dNSAddress[0]), "[[dNSAddress]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(iPv4Address[0]), "[[iPv4Address]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(iPv6Address[0]), "[[iPv6Address]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(warewolfMemory[0]), "[[warewolfMemory]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(warewolfCPU[0]), "[[warewolfCPU]]");
            Assert.IsFalse(string.IsNullOrWhiteSpace(warewolfServerVersion[0]), "[[warewolfServerVersion]]");
        }

        [TestMethod]
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
