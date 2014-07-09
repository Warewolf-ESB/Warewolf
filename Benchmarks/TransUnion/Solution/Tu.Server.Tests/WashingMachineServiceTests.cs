using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Simulation;
using Tu.Washing;

namespace Tu.Server.Tests
{
    [TestClass]
    public class WashingMachineServiceTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WashingMachineService_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WashingMachineService_Constructor_WithNullArgs_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var washingMachineService = new WashingMachineService(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WashingMachineService_Start")]
        public void WashingMachineService_Start_InvokesWashingMachineAndTransunionProcess()
        {
            //------------Setup for test--------------------------
            var washingMachine = new Mock<IWashingMachine>();
            washingMachine.Setup(w => w.Export()).Verifiable();
            washingMachine.Setup(w => w.Import(It.IsAny<DateTime>())).Verifiable();

            var transunionProcess = new Mock<ITransunionProcess>();
            transunionProcess.Setup(p => p.Run()).Verifiable();

            //------------Execute Test---------------------------
            var washingMachineService = new WashingMachineService(washingMachine.Object, transunionProcess.Object);
            washingMachineService.Start();

            //------------Assert Results-------------------------
            washingMachine.Verify(w => w.Export());
            washingMachine.Verify(w => w.Import(It.IsAny<DateTime>()));
            transunionProcess.Verify(p => p.Run());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WashingMachineService_Dispose")]
        public void WashingMachineService_Dispose_DisposesWashingMachine()
        {
            //------------Setup for test--------------------------
            var washingMachine = new Mock<IWashingMachine>();
            washingMachine.Setup(w => w.Dispose()).Verifiable();

            var transunionProcess = new Mock<ITransunionProcess>();

            //------------Execute Test---------------------------
            var washingMachineService = new WashingMachineService(washingMachine.Object, transunionProcess.Object);
            washingMachineService.Dispose();

            //------------Assert Results-------------------------
            washingMachine.Verify(w => w.Dispose());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WashingMachineService_CreateWashingMachine")]
        public void WashingMachineService_CreateWashingMachine_ReturnsInstance()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var washingMachine = WashingMachineService.CreateWashingMachine();

            //------------Assert Results-------------------------
            Assert.IsNotNull(washingMachine);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WashingMachineService_Main")]
        [Ignore]
        public void WashingMachineService_Main_StartsService()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            WashingMachineService.Main(null);
            Console.Write('c');

            //------------Assert Results-------------------------
        }
    }
}
