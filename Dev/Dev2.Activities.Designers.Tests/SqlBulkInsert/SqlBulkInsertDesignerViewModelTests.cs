using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Activities.Designers.Tests.SqlBulkInsert
{
    [TestClass]
    public class SqlBulkInsertDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlBulkInsertDesignerViewModel_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SqlBulkInsertDesignerViewModel(CreateModelItem(), null);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        public void SqlBulkInsertDesignerViewModel_Constructor_InitializesProperties()
        {
            //------------Setup for test--------------------------
            const int DatabaseCount = 2;
            var databases = CreateDatabaseList(DatabaseCount);

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(databases);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.ModelItemCollection);
            Assert.IsNotNull(viewModel.EditDatabaseCommand);
            Assert.IsNotNull(viewModel.RefreshTablesCommand);
            Assert.IsNotNull(viewModel.Databases);
            Assert.IsNotNull(viewModel.Tables);
            Assert.IsNull(viewModel.Database);
            Assert.IsFalse(viewModel.CanEditDatabase);

            Assert.AreEqual("InputMappings", viewModel.CollectionName);
            Assert.AreEqual(DatabaseCount + 1, viewModel.Databases.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_CanEditDatabase")]
        public void SqlBulkInsertDesignerViewModel_CanEditDatabase_DatabaseIsNull_False()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabaseList(2);
            var viewModel = CreateViewModel(databases);

            viewModel.Database = databases[0];

            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.CanEditDatabase);

            //------------Execute Test---------------------------
            viewModel.Database = null;

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.Database);
            Assert.IsFalse(viewModel.CanEditDatabase);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_CanEditDatabase")]
        public void SqlBulkInsertDesignerViewModel_CanEditDatabase_DatabaseIsNotNull_True()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabaseList(2);
            var viewModel = CreateViewModel(databases);

            //------------Execute Test---------------------------
            viewModel.Database = databases[0];

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.CanEditDatabase);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged")]
        public void SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged_PropertyNameIsDatabase_LoadsDatabaseTables()
        {
            //------------Setup for test--------------------------
            var tables = CreatTableList();
            var databases = CreateDatabaseList(2);
            var viewModel = CreateViewModel(databases, tables);

            var dbSource = databases[0];

            //------------Execute Test---------------------------
            viewModel.Database = dbSource;

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.CanEditDatabase);

            VerifyTables(tables, viewModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged")]
        public void SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged_PropertyNameIsTableName_LoadsTableColumns()
        {
            //------------Setup for test--------------------------
            var tables = CreatTableList();
            var databases = CreateDatabaseList(2);
            var viewModel = CreateViewModel(databases, tables);

            var dbSource = databases[0];
            viewModel.Database = dbSource;

            var dbTable = tables[0];

            //------------Execute Test---------------------------
            viewModel.TableName = dbTable.TableName;

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.TableName);

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(dbTable.Columns, actual);
        }

        static void VerifyTables(List<DbTable> tables, TestSqlBulkInsertDesignerViewModel viewModel)
        {
            for(var i = 0; i < tables.Count; i++)
            {
                var expected = tables[i];
                var actual = viewModel.Tables[i];
                Assert.AreEqual(expected.TableName, actual.TableName);
                Assert.AreEqual(expected.Columns.Count, actual.Columns.Count);
                VerifyColumns(expected.Columns, actual.Columns);
            }
        }

        static void VerifyColumns(List<DbColumn> expected, List<DbColumn> actual)
        {
            for(var j = 0; j < expected.Count; j++)
            {
                Assert.AreEqual(expected[j].ColumnName, actual[j].ColumnName);
                Assert.AreEqual(expected[j].DataType, actual[j].DataType);
                Assert.AreEqual(expected[j].MaxLength, actual[j].MaxLength);
            }
        }
        static TestSqlBulkInsertDesignerViewModel CreateViewModel(IEnumerable<DbSource> sources = null, List<DbTable> tables = null)
        {
            var sourceDefs = sources == null ? null : sources.Select(s => s.ToXml().ToString());
            if(tables == null)
            {
                tables = CreatTableList();
            }

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection.WorkspaceID).Returns(Guid.NewGuid());

            envModel.Setup(e => e.Connection.ExecuteCommand(It.Is<string>(s => s.Contains("FindSourcesByType")), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<XmlData>{0}</XmlData>", sourceDefs == null ? "" : string.Join("\n", sourceDefs)));

            envModel.Setup(e => e.Connection.ExecuteCommand(It.Is<string>(s => s.Contains("GetDatabaseTablesService")), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(JsonConvert.SerializeObject(tables));

            var modelItem = CreateModelItem();
            return new TestSqlBulkInsertDesignerViewModel(modelItem, envModel.Object);
        }


        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSqlBulkInsertActivity());
        }

        static List<DbSource> CreateDatabaseList(int count)
        {
            var result = new List<DbSource>();
            for(var i = 0; i < count; i++)
            {
                result.Add(new DbSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "Db" + i,
                });
            }

            return result;
        }

        static List<DbTable> CreatTableList()
        {
            var tables = new List<DbTable>();
            for(var i = 0; i < 10; i++)
            {
                tables.AddRange(new List<DbTable>
                {
                    new DbTable
                    {
                        TableName = "Table" + i, Columns = new List<DbColumn>
                        {
                            new DbColumn { ColumnName = "Column" + i + "_1", DataType = typeof(string), MaxLength = 50 },
                            new DbColumn { ColumnName = "Column" + i + "_2", DataType = typeof(int) },
                            new DbColumn { ColumnName = "Column" + i + "_3", DataType = typeof(double) },
                            new DbColumn { ColumnName = "Column" + i + "_4", DataType = typeof(float) }
                        }
                    },
                });
            }
            return tables;
        }
    }
}
