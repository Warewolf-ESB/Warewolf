using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;



namespace Dev2.Services.Execution.Tests
{
    [TestClass]
    public class DatabaseServiceExecutionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        public void OnConstruction_GivenDataObject_ShouldConstruct()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var newDatabaseServiceExecution = new DatabaseServiceExecution(new Mock<IDSFDataObject>().Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(newDatabaseServiceExecution, "Cannot create new DatabaseServiceExecution object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TranslateDataTableToEnvironment_Given3PopulatedOutPuts_ShouldMappAll()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IDSFDataObject>();
            var dt = GetTable();
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.HasRecordSet(It.IsAny<string>()));
            var newDatabaseServiceExecution = new DatabaseServiceExecution(mock.Object)
            {
                Outputs = new List<IServiceOutputMapping>()
                {
                    new ServiceOutputMapping("rec().a", "rec().a", "rec"),
                    new ServiceOutputMapping("rec().b", "rec().b", "rec"),
                    new ServiceOutputMapping("rec().b", "rec().b", "rec"),
                }
            };
            //---------------Assert Precondition----------------
            var methodInfo = typeof(DatabaseServiceExecution).GetMethod("TranslateDataTableToEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(newDatabaseServiceExecution, new object[] { dt, env.Object, 0 });
            //---------------Test Result -----------------------
            env.Verify(environment => environment.HasRecordSet(It.IsAny<string>()), Times.Exactly(3));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TranslateDataTableToEnvironment_Given3OneEmptyPopulatedOutPuts_ShouldMapp2()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IDSFDataObject>();
            var dt = GetTable();
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.HasRecordSet(It.IsAny<string>()));
            var newDatabaseServiceExecution = new DatabaseServiceExecution(mock.Object)
            {
                Outputs = new List<IServiceOutputMapping>()
                {
                    new ServiceOutputMapping("rec().a", "rec().a", "rec"),
                    new ServiceOutputMapping("rec().b", "rec().b", "rec"),
                    new ServiceOutputMapping("rec().b", "", "rec"),
                }
            };
            //---------------Assert Precondition----------------
            var methodInfo = typeof(DatabaseServiceExecution).GetMethod("TranslateDataTableToEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Execute Test ----------------------
            methodInfo.Invoke(newDatabaseServiceExecution, new object[] { dt, env.Object, 0 });
            //---------------Test Result -----------------------
            env.Verify(environment => environment.HasRecordSet(It.IsAny<string>()), Times.Exactly(2));
        }
        static DataTable GetTable()
        {
            // Here we create a DataTable with four columns.
            var table = new DataTable();
            table.Columns.Add("Dosage", typeof(int));
            table.Columns.Add("Drug", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Date", typeof(DateTime));

            // Here we add five DataRows.
            table.Rows.Add(25, "Indocin", "David", DateTime.Now);
            return table;
        }
    }
}
