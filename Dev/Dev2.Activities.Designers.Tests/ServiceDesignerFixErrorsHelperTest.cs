using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests
{
    [TestClass]
    public class ServiceDesignerFixErrorsHelperTest
    {
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerFixErrorsHelper_FetchInputMappings")]
        public void ServiceDesignerFixErrorsHelper_FetchInputMappings_WhenNullXmlStringAndOldMappings_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var serviceDesignerFixErrorsHelper = new ServiceDesignerFixErrorsHelper();

            //------------Execute Test---------------------------
            var result = serviceDesignerFixErrorsHelper.FetchInputs(null, null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerFixErrorsHelper_FetchInputMappings")]
        public void ServiceDesignerFixErrorsHelper_FetchInputMappings_WhenEmptyXmlStringAndNullOldMappings_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var serviceDesignerFixErrorsHelper = new ServiceDesignerFixErrorsHelper();

            //------------Execute Test---------------------------
            var result = serviceDesignerFixErrorsHelper.FetchInputs(string.Empty, null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }


        // ReSharper restore InconsistentNaming
    }

    public class ServiceDesignerFixErrorsHelper
    {
        public IList<IInputOutputViewModel> FetchInputs(string fixData, IList<IInputOutputViewModel> oldMappings)
        {
            return new List<IInputOutputViewModel>();
        }
    }
}
