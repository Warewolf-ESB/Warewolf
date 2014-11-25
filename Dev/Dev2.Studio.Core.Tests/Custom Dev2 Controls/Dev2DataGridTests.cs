
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.Core.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Custom_Dev2_Controls
{
    /// <summary>
    /// Summary description for Dev2DataGridTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2DataGridTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Dev2DataGridInsertExpectedRowInserted()
        {
            List<object> sourceList = new List<object>();
            sourceList.Add(new TestActivityDTO(new ActivityDTO("[[test1]]", "data1", 1)));
            sourceList.Add(new TestActivityDTO(new ActivityDTO("[[test2]]", "data2", 2)));
            sourceList.Add(new TestActivityDTO(new ActivityDTO("[[test3]]", "data3", 3)));
            sourceList.Add(new TestActivityDTO(new ActivityDTO("[[test4]]", "data4", 4)));
            sourceList.Add(new TestActivityDTO(new ActivityDTO("[[test5]]", "data5", 5)));

            var dataGrid = new Dev2DataGrid();
            dataGrid.ItemsSource = sourceList;
            dataGrid.InsertRow(2);

            Assert.AreEqual(6, dataGrid.Items.Count, "Row was not inserted into DataGrid.");

            ActivityDTO activityDto = dataGrid.Items[2] as ActivityDTO;
            Assert.IsTrue(activityDto != null, "The ActivityDTO is null.");
            Assert.AreEqual(string.Empty, activityDto.FieldName, "Row was not inserted into the right location.");
        }
    }

    public class TestActivityDTO
    {
        private ActivityDTO _act;

        public TestActivityDTO(ActivityDTO activityDto)
        {
            _act = activityDto;
            IndexNumber = activityDto.IndexNumber;
        }
        public ActivityDTO GetCurrentValue()
        {
            return _act;
        }

        public int IndexNumber { get; set; }
    }
}
