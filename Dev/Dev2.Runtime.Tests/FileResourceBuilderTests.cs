using System;
using System.Linq;
using Dev2.Runtime.ESB.Management.Services;
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
            string path = @"C:\ProgramData\Warewolf";
            var resourceHolder = new ResourceHolder(path);
            var fileResourceBuilder = new FileResourceBuilder(resourceHolder);
            var list = fileResourceBuilder.Build();
            
            //---------------Assert Precondition----------------

            Assert.AreEqual(1, list.Count());

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
