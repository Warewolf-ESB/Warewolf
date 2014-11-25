
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Data;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph.DataTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;

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
