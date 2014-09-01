using System.Data;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph.DataTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Tests.ConverterTests.GraphTests.DataTable_Test
{
    [TestClass]
    public class DataTableInterrogatorTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InterrogatorFactory_CreateMapper")]
        public void DataTableInterrogator_CreateMapper_WhenDataTable_ExpectDataTableMapper()
        {
            //------------Setup for test--------------------------
            DataTable obj = new DataTable();
            //------------Execute Test---------------------------
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(obj.GetType());
            var mapper = interrogator.CreateMapper(obj);
            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(DataTableMapper), mapper.GetType());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InterrogatorFactory_CreateMapper")]
        public void DataTableInterrogator_CreateMapper_WhenNull_ExpectDataTableMapper()
        {
            //------------Setup for test--------------------------
            DataTable obj = new DataTable();
            //------------Execute Test---------------------------
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(obj.GetType());
            var mapper = interrogator.CreateMapper(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(DataTableMapper), mapper.GetType());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InterrogatorFactory_CreateNavigator")]
        public void DataTableInterrogator_CreateMapper_WhenNull_ExpectDataTableNavigator()
        {
            //------------Setup for test--------------------------
            DataTable obj = new DataTable();
            //------------Execute Test---------------------------
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(obj.GetType());
            var nav = interrogator.CreateNavigator(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(DataTableNavigator), nav.GetType());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InterrogatorFactory_CreateNavigator")]
        public void DataTableInterrogator_CreateMapper_WhenDataTable_ExpectDataTableNavigator()
        {
            //------------Setup for test--------------------------
            DataTable obj = new DataTable();
            //------------Execute Test---------------------------
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(obj.GetType());
            var nav = interrogator.CreateNavigator(obj, typeof(DataTable));
            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(DataTableNavigator), nav.GetType());
        }

    }
}
