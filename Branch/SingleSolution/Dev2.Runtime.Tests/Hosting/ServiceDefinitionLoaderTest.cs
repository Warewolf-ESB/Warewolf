using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ServiceDefinitionLoaderTest
    {

        // GenerateServiceGraph

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDefinitionLoader_GenerateServiceGraph")]
        public void ServiceDefinitionLoader_GenerateServiceGraph_WhenLoadingSource_SourceIsLoaded()
        {
            //------------Setup for test--------------------------
            var serviceDefinitionLoader = new ServiceDefinitionLoader();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDefinitionLoader_GenerateServiceGraph")]
        public void ServiceDefinitionLoader_GenerateServiceGraph_WhenLoadingWorkflow_WorkflowIsLoaded()
        {
            //------------Setup for test--------------------------
            var serviceDefinitionLoader = new ServiceDefinitionLoader();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDefinitionLoader_GenerateServiceGraph")]
        public void ServiceDefinitionLoader_GenerateServiceGraph_WhenLoadingWorkerService_ServiceIsLoaded()
        {
            //------------Setup for test--------------------------
            var serviceDefinitionLoader = new ServiceDefinitionLoader();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
