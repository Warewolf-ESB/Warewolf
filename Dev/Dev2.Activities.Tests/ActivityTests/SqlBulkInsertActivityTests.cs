using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{

    [TestClass]
    public class SqlBulkInsertActivityTests : BaseActivityUnitTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Construct")]
        public void DsfSqlBulkInsertActivity_Construct_Paramterless_InputMappingsNotNull()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfSqlBulkInsertActivity = new DsfSqlBulkInsertActivity();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfSqlBulkInsertActivity);
            Assert.AreEqual("SQL Bulk Insert", dsfSqlBulkInsertActivity.DisplayName);
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
            Assert.IsInstanceOfType(sqlBulkInserter,typeof(ISqlBulkInserter));
            Assert.IsInstanceOfType(sqlBulkInserter,typeof(SqlBulkInserter));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_SqlBulkInserter")]
        public void DsfSqlBulkInsertActivity_SqlBulkInserter_WhenSet_ReturnsSetValue()
        {
            //------------Setup for test--------------------------
            var dsfSqlBulkInsertActivity = new DsfSqlBulkInsertActivity();
            dsfSqlBulkInsertActivity.SqlBulkInserter = new Mock<ISqlBulkInserter>().Object;
            //------------Execute Test---------------------------
            var sqlBulkInserter = dsfSqlBulkInsertActivity.SqlBulkInserter;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(sqlBulkInserter,typeof(ISqlBulkInserter));
            Assert.IsNotInstanceOfType(sqlBulkInserter,typeof(SqlBulkInserter));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithNoInputMappings_EmptyDataTableToInsert()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<SqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>",mockSqlBulkInserter.Object,null,"[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()),Times.Once());
            Assert.IsNull(returnedDataTable);            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_OptionsNotSet_HasSqlBulkCopyWithOptionsWithDefaultValues()
        {
            //------------Setup for test--------------------------
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()));
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>", mockSqlBulkInserter.Object, null, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(mockSqlBulkInserter.Object.CurrentOptions);  
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.CheckConstraints));  
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.FireTriggers));  
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.KeepIdentity));  
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.TableLock));  
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.UseInternalTransaction));  
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.KeepNulls));  
            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_OptionsSet_HasSqlBulkCopyWithOptionsWithValues()
        {
            //------------Setup for test--------------------------
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter = mockSqlBulkInserter.SetupAllProperties();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()));
            TestStartNode = new FlowStep
            {
                Action = new DsfSqlBulkInsertActivity
                {
                    InputMappings = null, 
                    CheckConstraints = true,
                    FireTriggers = true,
                    KeepIdentity = true,
                    UseDefaultValues = true,
                    UseInternalTransaction = true,
                    KeepTableLock = true,
                    SqlBulkInserter = mockSqlBulkInserter.Object,
                    Result = "[[result]]"
                }
            };

            CurrentDl = "<root><recset1><field1/></recset1></root>";
            TestData = "<root><recset1><field1/></recset1></root>";
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(mockSqlBulkInserter.Object.CurrentOptions);  
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.CheckConstraints));
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.FireTriggers));
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.KeepIdentity));
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.TableLock));
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.UseInternalTransaction));
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.KeepNulls));  
            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_OptionsSetMixed_HasSqlBulkCopyWithOptionsWithValues()
        {
            //------------Setup for test--------------------------
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter = mockSqlBulkInserter.SetupAllProperties();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()));
            TestStartNode = new FlowStep
            {
                Action = new DsfSqlBulkInsertActivity
                {
                    InputMappings = null, 
                    CheckConstraints = true,
                    FireTriggers = false,
                    KeepIdentity = true,
                    UseDefaultValues = false,
                    UseInternalTransaction = true,
                    KeepTableLock = false,
                    SqlBulkInserter = mockSqlBulkInserter.Object,
                    Result = "[[result]]"
                }
            };

            CurrentDl = "<root><recset1><field1/></recset1></root>";
            TestData = "<root><recset1><field1/></recset1></root>";
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()), Times.Once());
            Assert.IsNotNull(mockSqlBulkInserter.Object.CurrentOptions);  
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.CheckConstraints));
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.FireTriggers));
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.KeepIdentity));
            Assert.IsFalse(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.TableLock));
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.UseInternalTransaction));
            Assert.IsTrue(mockSqlBulkInserter.Object.CurrentOptions.HasFlag(SqlBulkCopyOptions.KeepNulls));  
            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSqlBulkInsertActivity_Execute")]
        public void DsfSqlBulkInsertActivity_Execute_WithInputMappings_HasDataTableToInsert()
        {
            //------------Setup for test--------------------------
            DataTable returnedDataTable = null;
            var mockSqlBulkInserter = new Mock<ISqlBulkInserter>();
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<SqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "tests",
                    OutputColumnName = "TestCol",
                    OutputColumnDataType = typeof(Int32),
                    OutputColumnMaxLength = 100
                }
            };
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>",mockSqlBulkInserter.Object,dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()),Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(1,returnedDataTable.Columns.Count);
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
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<SqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "tests",
                    OutputColumnName = "TestCol",
                    OutputColumnDataType = typeof(String),
                    OutputColumnMaxLength = 100
                }
            };
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>",mockSqlBulkInserter.Object,dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()),Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(1,returnedDataTable.Columns.Count);
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
            mockSqlBulkInserter.Setup(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()))
                .Callback<SqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            var dataColumnMappings = new List<DataColumnMapping>
            {
                new DataColumnMapping
                {
                    InputColumn = "tests",
                    OutputColumnName = "TestCol",
                    OutputColumnDataType = typeof(String),
                    OutputColumnMaxLength = 100
                }, new DataColumnMapping
                {
                    InputColumn = "tests2",
                    OutputColumnName = "TestCol2",
                    OutputColumnDataType = typeof(Int32),
                    OutputColumnMaxLength = 100
                }, new DataColumnMapping
                {
                    InputColumn = "tests3",
                    OutputColumnName = "TestCol3",
                    OutputColumnDataType = typeof(char),
                    OutputColumnMaxLength = 100
                }, new DataColumnMapping
                {
                    InputColumn = "tests4",
                    OutputColumnName = "TestCol4",
                    OutputColumnDataType = typeof(decimal),
                    OutputColumnMaxLength = 100
                }
            };
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>",mockSqlBulkInserter.Object,dataColumnMappings, "[[result]]");
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mockSqlBulkInserter.Verify(inserter => inserter.Insert(It.IsAny<SqlBulkCopy>(), It.IsAny<DataTable>()),Times.Once());
            Assert.IsNotNull(returnedDataTable);
            Assert.AreEqual(4,returnedDataTable.Columns.Count);
            
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

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData,ISqlBulkInserter sqlBulkInserter, IList<DataColumnMapping> inputMappings, string resultString)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSqlBulkInsertActivity
                {
                    InputMappings = inputMappings, 
                    SqlBulkInserter = sqlBulkInserter,
                    Result = resultString
                }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}