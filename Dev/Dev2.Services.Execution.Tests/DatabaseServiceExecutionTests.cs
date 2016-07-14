using System;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Services.Execution.Tests
{
    [TestClass]
    public class DatabaseServiceExecutionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_GivenDataObject_ShouldConstruct()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                //---------------Test Result -----------------------
                
                // ReSharper disable once ObjectCreationAsStatement
                new DatabaseServiceExecution(new Mock<IDSFDataObject>().Object);
            }
            catch(Exception ex) 
            {
                Assert.Fail(ex.Message);
            }
            
        }
       
    }
}
