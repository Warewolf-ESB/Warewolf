using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class FileResourceBuilderTests
    {
       
        [TestMethod]
        public void OnInit_Givenx_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                //new FileResourceBuilder();
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------                
                Assert.Fail(e.Message);
            }

        }

        
    }
}
