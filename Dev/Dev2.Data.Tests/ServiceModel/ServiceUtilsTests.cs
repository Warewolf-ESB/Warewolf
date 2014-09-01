using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel.Helper;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.ServiceModel
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServiceUtilsTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingValuesChanged")]
        public void ServiceUtils_MappingValuesChanged_OldMappingsIsNull_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingValuesChanged(null, new List<IDev2Definition>());

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingValuesChanged")]
        public void ServiceUtils_MappingValuesChanged_NewMappingsIsNull_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingValuesChanged(new List<IDev2Definition>(), null);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingValuesChanged")]
        public void ServiceUtils_MappingValuesChanged_OldAndNewMappingsAreSame_False()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateValueMapping("Locales(*).LocationID"), 
                CreateValueMapping("Locales(*).Address"), 
                CreateValueMapping("Locales(*).Lat"), 
                CreateValueMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateValueMapping("Locales(*).LocationID"),
                CreateValueMapping("Locales(*).Address"),
                CreateValueMapping("Locales(*).Lat"),
                CreateValueMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingValuesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingValuesChanged")]
        public void ServiceUtils_MappingValuesChanged_OldAndNewMappingsHaveDifferentCounts_True()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateValueMapping("Locales(*).LocationID"), 
                CreateValueMapping("Locales(*).Address"), 
                CreateValueMapping("Locales(*).Lat"), 
                CreateValueMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateValueMapping("Locales(*).LocationID"), 
                CreateValueMapping("Locales(*).Address"), 
                CreateValueMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingValuesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingValuesChanged")]
        public void ServiceUtils_MappingValuesChanged_OldAndNewMappingsAreDifferent_True()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateValueMapping("Locales(*).LocationID"),
                CreateValueMapping("Locales(*).Address"),
                CreateValueMapping("Locales(*).Lat"),
                CreateValueMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateValueMapping("Locales(*).LocationID"),
                CreateValueMapping("Locales(*).Address"),
                CreateValueMapping("Locales(*).Latitude"),
                CreateValueMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingValuesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        static IDev2Definition CreateValueMapping(string value)
        {
            return new Dev2Definition { Value = value };
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldMappingsIsNull_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(null, new List<IDev2Definition>());

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_NewMappingsIsNull_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(new List<IDev2Definition>(), null);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldAndNewMappingsAreSame_False()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"), 
                CreateNameMapping("Locales(*).Address"), 
                CreateNameMapping("Locales(*).Lat"), 
                CreateNameMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"),
                CreateNameMapping("Locales(*).Address"),
                CreateNameMapping("Locales(*).Lat"),
                CreateNameMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldAndNewMappingsHaveDifferentCounts_True()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"), 
                CreateNameMapping("Locales(*).Address"), 
                CreateNameMapping("Locales(*).Lat"), 
                CreateNameMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"), 
                CreateNameMapping("Locales(*).Address"), 
                CreateNameMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceUtils_MappingNamesChanged")]
        public void ServiceUtils_MappingNamesChanged_OldAndNewMappingsAreDifferent_True()
        {
            //------------Setup for test--------------------------
            var oldMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"),
                CreateNameMapping("Locales(*).Address"),
                CreateNameMapping("Locales(*).Lat"),
                CreateNameMapping("Locales(*).Long")
            };
            var newMappings = new List<IDev2Definition>
            {
                CreateNameMapping("Locales(*).LocationID"),
                CreateNameMapping("Locales(*).Address"),
                CreateNameMapping("Locales(*).Latitude"),
                CreateNameMapping("Locales(*).Long")
            };

            //------------Execute Test---------------------------
            var actual = ServiceUtils.MappingNamesChanged(oldMappings, newMappings);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        static IDev2Definition CreateNameMapping(string name)
        {
            return new Dev2Definition { Name = name };
        }
    }
}
