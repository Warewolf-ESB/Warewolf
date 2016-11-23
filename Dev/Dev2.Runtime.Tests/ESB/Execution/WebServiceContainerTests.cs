using System;
using Dev2.Data.TO;
using Dev2.Runtime.ESB.Execution;
using Dev2.Services.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class WebServiceContainerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebServiceContainer_GivenGivenServiceExec_ShouldConstruct()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new WebServiceContainer(new Mock<IServiceExecution>().Object);
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenValidArgs_ShouldExecuteCorrectly()
        {
            //---------------Set up test pack-------------------
            var serviceExec = new Mock<IServiceExecution>();
            serviceExec.SetupProperty(execution => execution.InstanceInputDefinitions);
            serviceExec.SetupProperty(execution => execution.InstanceOutputDefintions);
            ErrorResultTO resultTO;
            var valueFunction = Guid.NewGuid();
            serviceExec.Setup(execution => execution.Execute(out resultTO, It.IsAny<int>())).Returns(valueFunction);
            var webServiceContainer = new WebServiceContainer(serviceExec.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var execute = webServiceContainer.Execute(out resultTO, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(valueFunction, execute);
        }
    }
}
