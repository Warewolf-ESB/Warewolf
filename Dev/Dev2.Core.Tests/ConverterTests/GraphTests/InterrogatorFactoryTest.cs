using System.Data;
using Dev2.Converters.Graph.DataTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Tests.ConverterTests.GraphTests
{
    /// <summary>
    /// Summary description for InterrogatorFactoryTest
    /// </summary>
    [TestClass]
    public class InterrogatorFactoryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InterrogatorFactory_CreateInteregator")]
        // ReSharper disable InconsistentNaming
        public void DataTableInterrogator_CreateMapper_WhenDataTable_ExpectDataTableInterrogator()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            DataTable obj = new DataTable();
            //------------Execute Test---------------------------
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(obj.GetType());
            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(DataTableInterrogator), interrogator.GetType());
        }
    }
}
