using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Views.DropBox;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{

    [TestClass]
    public class DropboxHelperTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxHelper_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void DropboxHelper_Ctor_NullParams_ExpectException()

        {
            //------------Setup for test--------------------------
            var dropboxHelper = new DropBoxHelper(null);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxHelper_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropboxHelper_Ctor_NullParams_ExpectException_SecondaryCtor()
        {
            //------------Setup for test--------------------------
            var dropboxHelper = new DropBoxHelper(null,new Mock<IEnvironmentModel>().Object,"","" );

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
        // ReSharper restore InconsistentNaming
    }
}
