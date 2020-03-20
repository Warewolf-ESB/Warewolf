using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.ServerLifeCycleWorkers;

using Dev2.Runtime;
using Dev2.Interfaces;
using Dev2.Common;
using Warewolf.Auditing;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class RegisterDependenciesWorkerTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(RegisterDependenciesWorkerTests))]
        public void RegisterDependenciesWorker_Execute()
        {
            var worker = new RegisterDependenciesWorker();
            worker.Execute();
            Assert.IsNotNull(CustomContainer.Get<IActivityParser>());
            Assert.IsNotNull(CustomContainer.Get<IExecutionManager>());
            Assert.IsNotNull(CustomContainer.Get<IStateNotifierFactory>());
            Assert.IsNotNull(CustomContainer.Get<IResumableExecutionContainerFactory>());
            Assert.IsNotNull(CustomContainer.Get<IFieldAndPropertyMapper>());
        }
            
    }
}
