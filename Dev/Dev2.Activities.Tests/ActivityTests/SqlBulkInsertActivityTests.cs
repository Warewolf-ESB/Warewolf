using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Activities.SqlBulkInsert;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.DataList.Contract;
using Dev2.Enums;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// There are some very awkward corners in this tool.
    /// Seems as though it WAS NOT DEVELOPMED WITH TDD.
    /// I would say it was built, then testing was applied to it after the fact ;(
    /// I do not understand how else we could miss-null checks and have this string property grab to check values in test scenario. 
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SqlBulkInsertActivityTests : BaseActivityUnitTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Construct")]
        public void DsfSqlBulkInsertActivity_Construct_Paramterless_SetsDefaultPropertyValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfSqlBulkInsertActivity = new DsfSqlBulkInsertActivity();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfSqlBulkInsertActivity);
            Assert.AreEqual("SQL Bulk Insert", dsfSqlBulkInsertActivity.DisplayName);
            Assert.AreEqual("0", dsfSqlBulkInsertActivity.Timeout);
            Assert.AreEqual("0", dsfSqlBulkInsertActivity.BatchSize);
            Assert.IsTrue(dsfSqlBulkInsertActivity.IgnoreBlankRows);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_SqlBulkInserter")]
        public void DsfSqlBulkInsertActivity_SqlBulkInserter_NotSet_ReturnsConcreateType()
        {
            //------------Setup for test--------------------------
            var dsfSqlBulkInsertActivity = new DsfSqlBulkInsertActivity();
            //------------Execute Test---------------------------
            var sqlBulkInserter = dsfSqlBulkInsertActivity.SqlBulkInserter;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(sqlBulkInserter, typeof(ISqlBulkInserter));
            Assert.IsInstanceOfType(sqlBulkInserter, typeof(SqlBulkInserter));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_SqlBulkInserter")]
        public void DsfSqlBulkInsertActivity_SqlBulkInserter_WhenSet_ReturnsSetValue()
        {
            //------------Setup for test--------------------------
            var dsfSqlBulkInsertActivity = new DsfSqlBulkInsertActivity { SqlBulkInserter = new Mock<ISqlBulkInserter>().Object };
            //------------Execute Test---------------------------
            var sqlBulkInserter = dsfSqlBulkInsertActivity.SqlBulkInserter;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(sqlBulkInserter, typeof(ISqlBulkInserter));
            Assert.IsNotInstanceOfType(sqlBulkInserter, typeof(SqlBulkInserter));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithNoInputMappings_EmptyDataTableToInsert()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>", mockSqlBulkInserter.Object, null, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNull(returnedDataTable);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_OptionsNotSet_HasSqlBulkCopyWithOptionsWithDefaultValues()
        {
            //------------Setup for test--------------------------
            var activity = new DsfSqlBulkInsertActivity { TableName = "myTable" };
            //------------Execute Test---------------------------
            var options = activity.BuildSqlBulkCopyOptions();
            var bulkCopy = activity.SetupSqlBulkCopy(null, null, null, CreateDbSource(), options);
            //------------Assert Results-------------------------
            Assert.IsNotNull(options);
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.CheckConstraints));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.FireTriggers));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.KeepIdentity));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.TableLock));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.UseInternalTransaction));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.KeepNulls));
            Assert.AreEqual(0, bulkCopy.BulkCopyTimeout);
            Assert.AreEqual(0, bulkCopy.BatchSize);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfSqlBulkInsertActivity_Execute_WhenNotTableSet_ExpectException()
        {
            //------------Setup for test--------------------------
            var activity = new DsfSqlBulkInsertActivity { CheckConstraints = true, FireTriggers = true, KeepIdentity = true, UseInternalTransaction = true, KeepTableLock = true };
            //------------Execute Test---------------------------
            var options = activity.BuildSqlBulkCopyOptions();
            activity.SetupSqlBulkCopy(null, null, null, CreateDbSource(), options);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_OptionsSet_HasSqlBulkCopyWithOptionsWithValues()
        {
            //------------Setup for test--------------------------
            var activity = new DsfSqlBulkInsertActivity { TableName = "myTable", CheckConstraints = true, FireTriggers = true, KeepIdentity = true, UseInternalTransaction = true, KeepTableLock = true };
            //------------Execute Test---------------------------
            var options = activity.BuildSqlBulkCopyOptions();
            var bulkCopy = activity.SetupSqlBulkCopy(null, null, null, CreateDbSource(), options);
            //------------Assert Results-------------------------
            Assert.IsNotNull(options);
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.CheckConstraints));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.FireTriggers));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.KeepIdentity));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.TableLock));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.UseInternalTransaction));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.KeepNulls));
            Assert.AreEqual(0, bulkCopy.BulkCopyTimeout);
            Assert.AreEqual(0, bulkCopy.BatchSize);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_OptionsSetMixed_HasSqlBulkCopyWithOptionsWithValues()
        {
            //------------Setup for test--------------------------
            var activity = new DsfSqlBulkInsertActivity { TableName = "myTable", CheckConstraints = true, FireTriggers = false, KeepIdentity = true, UseInternalTransaction = true, KeepTableLock = true, BatchSize = "10", Timeout = "120" };
            //------------Execute Test---------------------------
            var options = activity.BuildSqlBulkCopyOptions();
            var bulkCopy = activity.SetupSqlBulkCopy(null, null, null, CreateDbSource(), options);
            //------------Assert Results-------------------------
            Assert.IsNotNull(options);
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.CheckConstraints));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.FireTriggers));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.KeepIdentity));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.TableLock));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.UseInternalTransaction));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.KeepNulls));
            Assert.AreEqual(120, bulkCopy.BulkCopyTimeout);
            Assert.AreEqual(10, bulkCopy.BatchSize);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_OptionsSetMixedUsingDataList_HasSqlBulkCopyWithOptionsWithValues()
        {
            //------------Setup for test--------------------------
            var activity = new DsfSqlBulkInsertActivity { TableName = "myTable", CheckConstraints = true, FireTriggers = false, KeepIdentity = true, UseInternalTransaction = true, KeepTableLock = false, BatchSize = "100", Timeout = "240" };
            //------------Execute Test---------------------------
            var options = activity.BuildSqlBulkCopyOptions();
            var bulkCopy = activity.SetupSqlBulkCopy(null, null, null, CreateDbSource(), options);
            //------------Assert Results-------------------------
            Assert.IsNotNull(options);
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.CheckConstraints));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.FireTriggers));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.KeepIdentity));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.TableLock));
            Assert.IsTrue(options.HasFlag(SqlBulkCopyOptions.UseInternalTransaction));
            Assert.IsFalse(options.HasFlag(SqlBulkCopyOptions.KeepNulls));
            Assert.AreEqual(240, bulkCopy.BulkCopyTimeout);
            Assert.AreEqual(100, bulkCopy.BatchSize);
        }

        static DbSource CreateDbSource()
        {
            var resourceID = Guid.NewGuid();
            var dbSource = new DbSource { ResourcePath = "SQL Tests\\" + resourceID, ResourceName = resourceID.ToString(), ResourceID = resourceID };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, dbSource);
            return dbSource;
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappings_HasDataTableToInsert()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "2",
                    OutputColumn = new DbColumn { ColumnName = "TestCol",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(1, returnedDataTable.Columns.Count);
            Assert.AreEqual("TestCol", returnedDataTable.Columns[0].ColumnName);
            Assert.AreEqual(typeof(Int32), returnedDataTable.Columns[0].DataType);
            Assert.AreEqual(-1, returnedDataTable.Columns[0].MaxLength); // Max Length Only applies to strings            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsStringMapping_HasDataTableToInsertWithStringColumnHavingMaxLength()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "tests",
                    OutputColumn = new DbColumn { ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(1, returnedDataTable.Columns.Count);
            Assert.AreEqual("TestCol", returnedDataTable.Columns[0].ColumnName);
            Assert.AreEqual(typeof(String), returnedDataTable.Columns[0].DataType);
            Assert.AreEqual(100, returnedDataTable.Columns[0].MaxLength); // Max Length Only applies to strings            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithMultipleInputMappings_HasDataTableWithMultipleColumns()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "tests",
                    OutputColumn = new DbColumn { ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "2",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "3",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "23",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);

            Assert.AreEqual("TestCol", returnedDataTable.Columns[0].ColumnName);
            Assert.AreEqual(typeof(String), returnedDataTable.Columns[0].DataType);
            Assert.AreEqual(100, returnedDataTable.Columns[0].MaxLength); // Max Length Only applies to strings            

            Assert.AreEqual("TestCol2", returnedDataTable.Columns[1].ColumnName);
            Assert.AreEqual(typeof(Int32), returnedDataTable.Columns[1].DataType);
            Assert.AreEqual(-1, returnedDataTable.Columns[1].MaxLength); // Max Length Only applies to strings            

            Assert.AreEqual("TestCol3", returnedDataTable.Columns[2].ColumnName);
            Assert.AreEqual(typeof(char), returnedDataTable.Columns[2].DataType);
            Assert.AreEqual(-1, returnedDataTable.Columns[2].MaxLength); // Max Length Only applies to strings            

            Assert.AreEqual("TestCol4", returnedDataTable.Columns[3].ColumnName);
            Assert.AreEqual(typeof(decimal), returnedDataTable.Columns[3].DataType);
            Assert.AreEqual(-1, returnedDataTable.Columns[3].MaxLength); // Max Length Only applies to strings            


        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsAndDataFromDataListAppend_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1().field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1().field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1().field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1().field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);
            Assert.AreEqual(1, returnedDataTable.Rows.Count);
            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsAndDataFromDataListStar_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(3, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithBlankRowsInData_HasDataTableWithDataExcludingBlankRows()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1></field1><field2></field2><field3></field3><field4></field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1></field1><field2></field2><field3></field3><field4></field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(3, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithIgnoreBlankRowsFalse_HasDataTableWithDataIncludingBlankRows()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1></field1><field2></field2><field3></field3><field4></field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1></field1><field2></field2><field3></field3><field4></field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]", null, null, PopulateOptions.PopulateBlankRows);
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);
            Assert.AreEqual(5, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual("2", returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual("C", returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual("21.2", returnedDataTable.Rows[0]["TestCol4"]);

            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol4"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual("3", returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual("G", returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual("26.4", returnedDataTable.Rows[2]["TestCol4"]);

            Assert.AreEqual("", returnedDataTable.Rows[3]["TestCol"]);
            Assert.AreEqual("", returnedDataTable.Rows[3]["TestCol2"]);
            Assert.AreEqual("", returnedDataTable.Rows[3]["TestCol3"]);
            Assert.AreEqual("", returnedDataTable.Rows[3]["TestCol4"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[4]["TestCol"]);
            Assert.AreEqual("1999", returnedDataTable.Rows[4]["TestCol2"]);
            Assert.AreEqual("Z", returnedDataTable.Rows[4]["TestCol3"]);
            Assert.AreEqual("60", returnedDataTable.Rows[4]["TestCol4"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsAndDataFromDataListStar_HasDataTableWithDataColumnHaveCorrectDataTypes()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                },
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field5]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol5",
                    SqlDataType = SqlDbType.DateTime,
                    MaxLength = 100 }
                },
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field6]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol6",
                    SqlDataType = SqlDbType.Time,
                    MaxLength = 100 }
                },
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field7]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol7",
                    SqlDataType = SqlDbType.UniqueIdentifier,
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4><field5>2013/11/15 10:10:02 AM</field5><field6>13:10:02</field6><field7>52ed8fe2-80c3-42e1-8a9f-f52715988efb</field7></recset1></root>", "<root><recset1><field1/><field2/><field3/><field4/><field5/><field6/><field7/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(7, returnedDataTable.Columns.Count);
            Assert.AreEqual(typeof(String), returnedDataTable.Columns[0].DataType);
            Assert.AreEqual(typeof(Int32), returnedDataTable.Columns[1].DataType);
            Assert.AreEqual(typeof(char), returnedDataTable.Columns[2].DataType);
            Assert.AreEqual(typeof(decimal), returnedDataTable.Columns[3].DataType);
            Assert.AreEqual(typeof(DateTime), returnedDataTable.Columns[4].DataType);
            Assert.AreEqual(typeof(TimeSpan), returnedDataTable.Columns[5].DataType);
            Assert.AreEqual(typeof(Guid), returnedDataTable.Columns[6].DataType);
            Assert.AreEqual(1, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual(DateTime.Parse("2013/11/15 10:10:02 AM"), returnedDataTable.Rows[0]["TestCol5"]);
            Assert.AreEqual(TimeSpan.Parse("13:10:02"), returnedDataTable.Rows[0]["TestCol6"]);
            Assert.AreEqual(Guid.Parse("52ed8fe2-80c3-42e1-8a9f-f52715988efb"), returnedDataTable.Rows[0]["TestCol7"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsAndDataFromDataListAppendMulitpleRows_HasDataTableWithDataOnlyLastRowFromDataList()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1().field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1().field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1().field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1().field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);
            Assert.AreEqual(1, returnedDataTable.Rows.Count);

            Assert.AreEqual("Jill", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[0]["TestCol4"]);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsAndDataFromDataListStarWithOneScalar_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val",
                    DataType = typeof(String),
                    MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(5, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(3, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithIdentityColumnWithDataMappedAndKeepIdentityOnWhenMultiTable_HasTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100, IsAutoIncrement = true}
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]", null, null, PopulateOptions.IgnoreBlankRows, true);
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(5, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(3, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithIdentityColumnWithDataMappedAndKeepIdentityOffWhenMultiTable_HasErrorAboutColumn()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100, IsAutoIncrement = true}
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            var dataObject = ExecuteProcess() as IDSFDataObject;
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Never());
            Assert.IsNull(returnedDataTable);
            if(dataObject != null)
            {
                var dlID = dataObject.DataListID;
                var compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO errors;
                var bdl = compiler.FetchBinaryDataList(dlID, out errors);
                var executionErrors = bdl.FetchErrors();
                StringAssert.Contains(executionErrors, "The column TestCol2 is an IDENTITY and you have the Keep Identity option disabled. Either enable it or remove the mapping.");
            }
            else
            {
                Assert.Fail("Bad cast!");
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithInvalidIndex_HasErrorAboutColumn()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(-1).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100}
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            var dataObject = ExecuteProcess() as IDSFDataObject;
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Never());
            Assert.IsNull(returnedDataTable);
            if(dataObject != null)
            {
                var dlID = dataObject.DataListID;
                var compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO errors;
                var bdl = compiler.FetchBinaryDataList(dlID, out errors);
                var executionErrors = bdl.FetchErrors();
                StringAssert.Contains(executionErrors, "Recordset index [ -1 ] is not greater than zero");
                Assert.IsFalse(executionErrors.Contains("Problems with Iterators for SQLBulkInsert"), "Iterator exception has been added ;(");
            }
            else
            {
                Assert.Fail("Bad cast!");
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithInvalidEvaluatedIndex_HasErrorAboutColumn()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1([[val]]).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100}
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            var dataObject = ExecuteProcess() as IDSFDataObject;
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Never());
            Assert.IsNull(returnedDataTable);
            if(dataObject != null)
            {
                var dlID = dataObject.DataListID;
                var compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO errors;
                var bdl = compiler.FetchBinaryDataList(dlID, out errors);
                var executionErrors = bdl.FetchErrors();
                StringAssert.Contains(executionErrors, "Input string was not in a correct format");
                Assert.IsFalse(executionErrors.Contains("Problems with Iterators for SQLBulkInsert"), "Iterator exception has been added ;(");
            }
            else
            {
                Assert.Fail("Bad cast!");
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithIdentityColumnWithNoDataMappedAndKeepIdentityOnWhenMultiTable_HasErrorAboutColumn()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = string.Empty,
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100, IsAutoIncrement = true}
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]", null, null, PopulateOptions.IgnoreBlankRows, true);
            //------------Execute Test---------------------------
            var dataObject = ExecuteProcess() as IDSFDataObject;
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Never());
            Assert.IsNull(returnedDataTable);
            if(dataObject != null)
            {
                var dlID = dataObject.DataListID;
                var compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO errors;
                var bdl = compiler.FetchBinaryDataList(dlID, out errors);
                var executionErrors = bdl.FetchErrors();
                StringAssert.Contains(executionErrors, "The column TestCol2 is an IDENTITY and you have the Keep Identity option enabled. Either disable this option or map data.");
            }
            else
            {
                Assert.Fail("Bad cast!");
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithIdentityColumnWithNoDataMappedAndKeepIdentityOffWhenMultiTable_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = string.Empty,
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100, IsAutoIncrement = true}
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithNullableColumnWithNoDataMappedWhenMultiTable_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = string.Empty,
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100, IsNullable = true},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100 }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual(3, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithNullableColumnWithDataMappedWhenMultiTable_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100, IsNullable = true},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100 }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(5, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(3, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithNonNullableColumnWithDataMappedWhenMultiTable_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100, IsNullable = false},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100 }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(5, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(2, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(3, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsWithNonNullableColumnWithNoDataMappedWhenMultiTable_HasErrorAboutColumn()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol", DataType = typeof(String), MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = string.Empty,
                    OutputColumn = new DbColumn { ColumnName = "TestCol2", DataType = typeof(Int32), MaxLength = 100, IsNullable = false}
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val", DataType = typeof(String), MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3", DataType = typeof(char), MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4", DataType = typeof(decimal), MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            var dataObject = ExecuteProcess() as IDSFDataObject;
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Never());
            Assert.IsNull(returnedDataTable);
            if(dataObject != null)
            {
                var dlID = dataObject.DataListID;
                var compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO errors;
                var bdl = compiler.FetchBinaryDataList(dlID, out errors);
                var executionErrors = bdl.FetchErrors();
                StringAssert.Contains(executionErrors, "The column TestCol2 does not allow NULL. Please check your mappings to ensure you have mapped data into it.");
            }
            else
            {
                Assert.Fail("Bad cast!");
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_IgnoreBlankRowsFalse_HasDataTableWithBlankRowData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);

            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val",
                    DataType = typeof(String),
                    MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1></field1><field2></field2><field3></field3><field4></field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val></val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]", null, null, PopulateOptions.PopulateBlankRows);
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(5, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual("2", returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual("C", returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual("21.2", returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual("1999", returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual("Z", returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual("60", returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_MixedData_IgnoreBlankRowsTrue_HasDataTableWithOutBlankRowData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val",
                    DataType = typeof(String),
                    MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(String),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1></field1><field2></field2><field3></field3><field4></field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val></val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(5, returnedDataTable.Columns.Count);
            Assert.AreEqual(2, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual("2", returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual("C", returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual("21.2", returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual("1999", returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual("Z", returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual("60", returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("", returnedDataTable.Rows[1]["Val"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsAndDataFromDataListStarWithOneScalarAndAppend_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn {ColumnName = "TestCol",
                    DataType = typeof(String),
                    MaxLength = 100},
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1().field2]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol2",
                    DataType = typeof(Int32),
                    MaxLength = 100 }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn { ColumnName = "Val",
                    DataType = typeof(String),
                    MaxLength = 100 }
                },new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol3",
                    DataType = typeof(char),
                    MaxLength = 100 }
                }, new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn { ColumnName = "TestCol4",
                    DataType = typeof(decimal),
                    MaxLength = 100 }
                }
            };
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(5, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappingsAndDataFromDataListStarWithOneScalarAndAppendMixedRecsets_HasDataTableWithData()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            SetupArguments("<root><recset1><field1>Bob</field1><field2>2</field2><field3>C</field3><field4>21.2</field4></recset1><recset1><field1>Jane</field1><field2>3</field2><field3>G</field3><field4>26.4</field4></recset1><recset1><field1>Jill</field1><field2>1999</field2><field3>Z</field3><field4>60</field4></recset1><rec><f1>JJU</f1><f2>89</f2></rec><rec><f1>KKK</f1><f2>67</f2></rec><val>Hello</val></root>", "<root><recset1><field1/><field2/><field3/><field4/></recset1><rec><f1/><f2/></rec><val/></root>", mockSqlBulkInserter.Object, dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<ISqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(7, returnedDataTable.Columns.Count);
            Assert.AreEqual(3, returnedDataTable.Rows.Count);

            Assert.AreEqual("Bob", returnedDataTable.Rows[0]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[0]["TestCol2"]);
            Assert.AreEqual('C', returnedDataTable.Rows[0]["TestCol3"]);
            Assert.AreEqual(21.2m, returnedDataTable.Rows[0]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[0]["Val"]);
            Assert.AreEqual("JJU", returnedDataTable.Rows[0]["Col1"]);
            Assert.AreEqual(67, returnedDataTable.Rows[0]["Col2"]);

            Assert.AreEqual("Jane", returnedDataTable.Rows[1]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[1]["TestCol2"]);
            Assert.AreEqual('G', returnedDataTable.Rows[1]["TestCol3"]);
            Assert.AreEqual(26.4m, returnedDataTable.Rows[1]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[1]["Val"]);
            Assert.AreEqual("KKK", returnedDataTable.Rows[1]["Col1"]);
            Assert.AreEqual(67, returnedDataTable.Rows[1]["Col2"]);

            Assert.AreEqual("Jill", returnedDataTable.Rows[2]["TestCol"]);
            Assert.AreEqual(1999, returnedDataTable.Rows[2]["TestCol2"]);
            Assert.AreEqual('Z', returnedDataTable.Rows[2]["TestCol3"]);
            Assert.AreEqual(60m, returnedDataTable.Rows[2]["TestCol4"]);
            Assert.AreEqual("Hello", returnedDataTable.Rows[2]["Val"]);
            Assert.AreEqual("KKK", returnedDataTable.Rows[2]["Col1"]);
            Assert.AreEqual(67, returnedDataTable.Rows[2]["Col2"]);
        }

        static List<DataColumnMapping> DataColumnMappingsMixedMappings()
        {
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field1]]",
                    OutputColumn = new DbColumn
                    {
                        ColumnName = "TestCol",
                        DataType = typeof(String),
                        SqlDataType = SqlDbType.VarChar,
                        MaxLength = 100
                    },
                },
                new DataColumnMapping
                {
                    InputColumn = "[[recset1().field2]]",
                    OutputColumn = new DbColumn
                    {
                        ColumnName = "TestCol2",
                        DataType = typeof(Int32),
                        SqlDataType = SqlDbType.Int,
                        MaxLength = 100
                    }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[rec().f2]]",
                    OutputColumn = new DbColumn
                    {
                        ColumnName = "Col2",
                        DataType = typeof(Int32),
                        SqlDataType = SqlDbType.Int,
                        MaxLength = 100
                    }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[val]]",
                    OutputColumn = new DbColumn
                    {
                        ColumnName = "Val",
                        DataType = typeof(String),
                        SqlDataType = SqlDbType.VarChar,
                        MaxLength = 100
                    }
                }
                , new DataColumnMapping
                {
                    InputColumn = "[[rec(*).f1]]",
                    OutputColumn = new DbColumn
                    {
                        ColumnName = "Col1",
                        DataType = typeof(string),
                        SqlDataType = SqlDbType.VarChar,
                        MaxLength = 100
                    }
                },
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field3]]",
                    OutputColumn = new DbColumn
                    {
                        ColumnName = "TestCol3",
                        DataType = typeof(char),
                        SqlDataType = SqlDbType.Char,
                        MaxLength = 100
                    }
                },
                new DataColumnMapping
                {
                    InputColumn = "[[recset1(*).field4]]",
                    OutputColumn = new DbColumn
                    {
                        ColumnName = "TestCol4",
                        DataType = typeof(decimal),
                        SqlDataType = SqlDbType.Decimal,
                        MaxLength = 100
                    }
                }
            };
            return dataColumnMappings;
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_UpdateForEachInputs")]
        public void DsfSqlBulkInsertActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string BatchSize = "[[batchsize]]";
            const string TimeOut = "[[timeout]]";
            const string TableName = "TestTable";
            const string Result = "[[res]]";
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            var act = new DsfSqlBulkInsertActivity
            {
                BatchSize = "[[batchsize]]",
                Timeout = "[[timeout]]",
                Database = new DbSource(),
                TableName = "TestTable",
                InputMappings = dataColumnMappings,
                Result = Result
            };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(BatchSize, act.BatchSize);
            Assert.AreEqual(TimeOut, act.Timeout);
            Assert.AreEqual(TableName, act.TableName);
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_UpdateForEachInputs")]
        public void DsfSqlBulkInsertActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string BatchSize = "[[batchsize]]";
            const string TimeOut = "[[timeout]]";
            const string TableName = "TestTable";
            const string Result = "[[res]]";
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            var act = new DsfSqlBulkInsertActivity
            {
                BatchSize = "[[batchsize]]",
                Timeout = "[[timeout]]",
                Database = new DbSource(),
                TableName = "TestTable",
                InputMappings = dataColumnMappings,
                Result = Result
            };
            var tuple1 = new Tuple<string, string>(BatchSize, "Test");
            var tuple2 = new Tuple<string, string>(TimeOut, "Test2");
            var tuple3 = new Tuple<string, string>(TableName, "Test3");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.Timeout);
            Assert.AreEqual("Test", act.BatchSize);
            Assert.AreEqual("Test3", act.TableName);
            Assert.AreEqual(Result, act.Result);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_UpdateForEachOutputs")]
        public void DsfSqlBulkInsertActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Result = "[[res]]";
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            var act = new DsfSqlBulkInsertActivity
            {
                BatchSize = "[[batchsize]]",
                Timeout = "[[timeout]]",
                Database = new DbSource(),
                TableName = "TestTable",
                InputMappings = dataColumnMappings,
                Result = Result
            };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_UpdateForEachOutputs")]
        public void DsfSqlBulkInsertActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Result = "[[res]]";
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            var act = new DsfSqlBulkInsertActivity
            {
                BatchSize = "[[batchsize]]",
                Timeout = "[[timeout]]",
                Database = new DbSource(),
                TableName = "TestTable",
                InputMappings = dataColumnMappings,
                Result = Result
            };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_UpdateForEachOutputs")]
        public void DsfSqlBulkInsertActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string Result = "[[res]]";
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            var act = new DsfSqlBulkInsertActivity
            {
                BatchSize = "[[batchsize]]",
                Timeout = "[[timeout]]",
                Database = new DbSource(),
                TableName = "TestTable",
                InputMappings = dataColumnMappings,
                Result = Result
            };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_GetForEachInputs")]
        public void DsfSqlBulkInsertActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string BatchSize = "[[batchsize]]";
            const string TimeOut = "[[timeout]]";
            const string TableName = "TestTable";
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            var act = new DsfSqlBulkInsertActivity
            {
                BatchSize = "[[batchsize]]",
                Timeout = "[[timeout]]",
                Database = new DbSource(),
                TableName = "TestTable",
                InputMappings = dataColumnMappings,
                Result = "[[result]]"
            };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(10, dsfForEachItems.Count);
            Assert.AreEqual(BatchSize, dsfForEachItems[0].Name);
            Assert.AreEqual(BatchSize, dsfForEachItems[0].Value);
            Assert.AreEqual(TimeOut, dsfForEachItems[1].Name);
            Assert.AreEqual(TimeOut, dsfForEachItems[1].Value);
            Assert.AreEqual(TableName, dsfForEachItems[2].Name);
            Assert.AreEqual(TableName, dsfForEachItems[2].Value);
            Assert.AreEqual("[[recset1(*).field1]]", dsfForEachItems[3].Name);
            Assert.AreEqual("[[recset1(*).field1]]", dsfForEachItems[3].Value);
            Assert.AreEqual("[[recset1().field2]]", dsfForEachItems[4].Name);
            Assert.AreEqual("[[recset1().field2]]", dsfForEachItems[4].Value);
            Assert.AreEqual("[[rec().f2]]", dsfForEachItems[5].Name);
            Assert.AreEqual("[[rec().f2]]", dsfForEachItems[5].Value);
            Assert.AreEqual("[[val]]", dsfForEachItems[6].Name);
            Assert.AreEqual("[[val]]", dsfForEachItems[6].Value);
            Assert.AreEqual("[[rec(*).f1]]", dsfForEachItems[7].Name);
            Assert.AreEqual("[[rec(*).f1]]", dsfForEachItems[7].Value);
            Assert.AreEqual("[[recset1(*).field3]]", dsfForEachItems[8].Name);
            Assert.AreEqual("[[recset1(*).field3]]", dsfForEachItems[8].Value);
            Assert.AreEqual("[[recset1(*).field4]]", dsfForEachItems[9].Name);
            Assert.AreEqual("[[recset1(*).field4]]", dsfForEachItems[9].Value);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_GetForEachOutputs")]
        public void DsfSqlBulkInsertActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string Result = "[[res]]";
            var dataColumnMappings = DataColumnMappingsMixedMappings();
            var act = new DsfSqlBulkInsertActivity
            {
                BatchSize = "[[batchsize]]",
                Timeout = "[[timeout]]",
                Database = new DbSource(),
                TableName = "TestTable",
                InputMappings = dataColumnMappings,
                Result = Result
            };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(Result, dsfForEachItems[0].Name);
            Assert.AreEqual(Result, dsfForEachItems[0].Value);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DsfSqlBulkInsertActivity_GetFindMissingType")]
        public void DsfSqlBulkInsertActivity_GetFindMissingType_MixedActivity()
        {
            //------------Setup for test--------------------------
            var activity = new DsfSqlBulkInsertActivity();

            //------------Execute Test---------------------------
            var findMissingType = activity.GetFindMissingType();

            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.MixedActivity, findMissingType);
        }

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, ISqlBulkInserter sqlBulkInserter, IList<DataColumnMapping> inputMappings, string resultString, DbSource dbSource = null, string destinationTableName = null, PopulateOptions populateOptions = PopulateOptions.IgnoreBlankRows, bool keepIdentity = false)
        {
            var ignoreBlankRows = populateOptions == PopulateOptions.IgnoreBlankRows;
            if(dbSource == null)
            {
                dbSource = CreateDbSource();
            }
            if(destinationTableName == null)
            {
                destinationTableName = "SomeTestTable";
            }
            TestStartNode = new FlowStep
            {
                Action = new DsfSqlBulkInsertActivity
                {
                    Database = dbSource,
                    TableName = destinationTableName,
                    InputMappings = inputMappings,
                    SqlBulkInserter = sqlBulkInserter,
                    Result = resultString,
                    IgnoreBlankRows = ignoreBlankRows,
                    KeepIdentity = keepIdentity
                }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}