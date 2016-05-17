using System;
using Dev2.Factories;
using Dev2.Views.DropBox2016;
using Dev2.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Revised By Nkosinathi Sangweni
    /// </summary>
    [TestClass]
    public class DropBoxSourceViewModelTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropBoxSourceViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void DropBoxSourceViewModel_Ctor_NullParams_ExpectException()
           
        {
            //------------Setup for test--------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new DropBoxSourceViewModel(null, new Mock<IDropBoxHelper>().Object,new DropboxFactory(),false );

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropBoxSourceViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropBoxSourceViewModel_Ctor_NullParams_ExpectException_helper()
        {
            //------------Setup for test--------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new DropBoxSourceViewModel(new Mock<INetworkHelper>().Object, null, new DropboxFactory(), false);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropBoxSourceViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropBoxSourceViewModel_Ctor_NullParams_ExpectException_factory()
        {
            //------------Setup for test--------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new DropBoxSourceViewModel(new Mock<INetworkHelper>().Object, new Mock<IDropBoxHelper>().Object, null, false);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        
        // ReSharper restore InconsistentNaming
    }
}
