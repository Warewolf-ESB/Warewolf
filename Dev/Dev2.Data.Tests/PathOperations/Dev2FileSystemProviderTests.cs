using System;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class Dev2FileSystemProviderTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("Dev2FileSystemProvider_CRUDOperationTests")]        
        public void Dev2FileSystemProvider_GetOperation_NonExistingPath_FriendlyError()
        {
            bool pass = false;
            Dev2FileSystemProvider testProvider = new Dev2FileSystemProvider();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString("C:/dadsdascasxxxacvaawqf",false);
            try
            {
                testProvider.Get(path);    
            }
            catch(Exception ex)
            {
                Assert.AreEqual("File not found [ C:/dadsdascasxxxacvaawqf ]", ex.Message);
                pass = true;
            }
            if(!pass)
            {
                Assert.Fail("The corrrect error wasnt returned");    
            }            
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("Dev2FileSystemProvider_CRUDOperationTests")]
        public void Dev2FileSystemProvider_GetDirectoryOperation_NonExistingPath_FriendlyError()
        {
            bool pass = false;
            Dev2FileSystemProvider testProvider = new Dev2FileSystemProvider();
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString("C:/dadsdascasxxxacvaawqf", false);
            try
            {
                testProvider.ListDirectory(path);
            }
            catch(Exception ex)
            {
                Assert.AreEqual("Directory not found [ C:/dadsdascasxxxacvaawqf ] ", ex.Message);
                pass = true;
            }
            if(!pass)
            {
                Assert.Fail("The corrrect error wasnt returned");
            }
        }
    }
}
