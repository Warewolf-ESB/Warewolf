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
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxHelper_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropboxHelper_Ctor_NullParams_ExpectException_SecondaryCtor_Env()
        {
            //------------Setup for test--------------------------
            var dropboxHelper = new DropBoxHelper(new DropBoxViewWindow(), null, "", "");

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxHelper_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropboxHelper_Ctor_NullParams_ExpectException_SecondaryCtor_Type()
        {
            //------------Setup for test--------------------------
            var dropboxHelper = new DropBoxHelper(new DropBoxViewWindow(), new Mock<IEnvironmentModel>().Object, null, "");

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxHelper_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropboxHelper_Ctor_NullParams_ExpectException_SecondaryCtor_Path()
        {
            //------------Setup for test--------------------------
            var dropboxHelper = new DropBoxHelper(new DropBoxViewWindow(), new Mock<IEnvironmentModel>().Object, "",null);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxHelper_Ctor")]

        public void DropboxHelper_Ctor_PropertiesSet()
        {
            var env = new Mock<IEnvironmentModel>().Object;
            var win = new DropBoxViewWindow();
            //------------Setup for test--------------------------
            var dropboxHelper = new DropBoxHelper(win, env, "a", "b");

            //------------Execute Test---------------------------
            Assert.AreEqual(dropboxHelper.ActiveEnvironment,env);
            Assert.AreEqual(dropboxHelper.DropBoxViewWindow,win);
            Assert.AreEqual(dropboxHelper.ResourcePath,"b");
            Assert.AreEqual(dropboxHelper.ResourceType, "a");
            Assert.AreEqual(dropboxHelper.CircularProgressBar,win.CircularProgressBar);
            Assert.AreEqual(dropboxHelper.WebBrowser,win.WebBrowserHost);
            //------------Assert Results-------------------------
        }
        // ReSharper restore InconsistentNaming
    }
}
