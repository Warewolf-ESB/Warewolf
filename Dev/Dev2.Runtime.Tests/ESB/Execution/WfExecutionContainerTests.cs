using System;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;



namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class WfExecutionContainerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var obj = new Mock<IDSFDataObject>();
                var workSpace = new Mock<IWorkspace>();
                var channel = new Mock<IEsbChannel>();
                var serviceAction = new ServiceAction();
                
                var wfExecutionContainer = new WfExecutionContainer(serviceAction, obj.Object, workSpace.Object, channel.Object);
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }
    }
}
