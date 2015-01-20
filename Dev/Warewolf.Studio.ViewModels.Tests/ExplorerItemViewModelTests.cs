using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerItemViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullShellViewModel_ExpectException()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new ExplorerItemViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Constructor")]
        public void Constructor_SetsUpOpenCommand()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
