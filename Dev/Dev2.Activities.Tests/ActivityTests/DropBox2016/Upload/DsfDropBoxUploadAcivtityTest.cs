using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.DropBox2016.UploadActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Upload
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfDropBoxUploadAcivtityTest
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUpload_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = new DsfDropBoxUploadAcivtity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(boxUploadAcivtity);
        }
    }
}