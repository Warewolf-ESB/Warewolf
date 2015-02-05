using System;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SaveServiceViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullEnvironment_ArgumentNullExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SaveServiceViewModel(null);
            //------------Assert Results-------------------------

        }
    }

    public class SaveServiceViewModel
    {
        public SaveServiceViewModel(IEnvironmentViewModel environmentViewModel)
        {
            if (environmentViewModel == null)
            {
                throw new ArgumentNullException("environmentViewModel");
            }
        }
    }
}
