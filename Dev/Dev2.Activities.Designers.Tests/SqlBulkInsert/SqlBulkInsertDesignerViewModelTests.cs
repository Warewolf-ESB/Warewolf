using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Caliburn.Micro;
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
    [Ignore]
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
            var viewModel = new SqlBulkInsertDesignerViewModel(CreateModelItem(), null, null);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlBulkInsertDesignerViewModel_Constructor_EventAggregatorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SqlBulkInsertDesignerViewModel(CreateModelItem(), new Mock<IEnvironmentModel>().Object, null);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        public void SqlBulkInsertDesignerViewModel_Constructor_EmptyInitializesProperties()
        {
            //------------Setup for test--------------------------
            const int DatabaseCount = 2;
            var databases = CreateDatabases(DatabaseCount);

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
            Assert.IsFalse(viewModel.IsDatabaseSelected);

            Assert.AreEqual("InputMappings", viewModel.CollectionName);
            Assert.AreEqual(DatabaseCount + 1, viewModel.Databases.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_CanEditDatabase")]
        public void SqlBulkInsertDesignerViewModel_CanEditDatabase_DatabaseIsNull_False()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            viewModel.Database = databases.Keys.First();

            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.IsDatabaseSelected);

            //------------Execute Test---------------------------
            viewModel.Database = null;

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.Database);
            Assert.IsFalse(viewModel.IsDatabaseSelected);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_CanEditDatabase")]
        public void SqlBulkInsertDesignerViewModel_CanEditDatabase_DatabaseIsNotNull_True()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            //------------Execute Test---------------------------
            viewModel.Database = databases.Keys.First();

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.IsDatabaseSelected);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged")]
        public void SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged_PropertyNameIsDatabase_LoadsDatabaseTables()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var dbSource = databases.Keys.First();

            //------------Execute Test---------------------------
            viewModel.Database = dbSource;

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.IsDatabaseSelected);

            VerifyTables(databases[dbSource], viewModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged")]
        public void SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged_PropertyNameIsTableName_LoadsTableColumns()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var dbSource = databases.Keys.First();
            viewModel.Database = dbSource;

            var dbTable = databases[dbSource][0];

            //------------Execute Test---------------------------
            viewModel.TableName = dbTable.TableName;

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.TableName);

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(dbTable.Columns, actual);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Database")]
        public void SqlBulkInsertDesignerViewModel_Database_ChangedAndTableNameExists_SelectsTableAndLoadsColumns()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var dbSource = databases.Keys.First();
            var dbTable = databases[dbSource][0];

            viewModel.TableName = dbTable.TableName;
            Assert.IsNotNull(viewModel.TableName);

            Assert.AreEqual(0, viewModel.InputMappings.Count);

            //------------Execute Test---------------------------
            viewModel.Database = dbSource;

            //------------Assert Results-------------------------

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(dbTable.Columns, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Database")]
        [Ignore]
        public void SqlBulkInsertDesignerViewModel_Database_ChangedAndTableNameDoesNotExists_ClearsTableNameAndTableColumns()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var dbSource1 = databases.Keys.First();
            var dbTable1 = databases[dbSource1][0];

            viewModel.TableName = dbTable1.TableName;
            viewModel.Database = dbSource1;

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();
            VerifyColumns(dbTable1.Columns, actual);

            var dbSource2 = databases.Keys.Skip(1).First();

            //------------Execute Test---------------------------
            viewModel.Database = dbSource2;

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.TableName);
            Assert.AreEqual(0, viewModel.InputMappings.Count);
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

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(Dictionary<DbSource, List<DbTable>> sources, IEventAggregator eventAggregator = null)
        {
            var sourceDefs = sources == null ? null : sources.Select(s => s.Key.ToXml().ToString());

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection.WorkspaceID).Returns(Guid.NewGuid());

            envModel.Setup(e => e.Connection.ExecuteCommand(It.Is<string>(s => s.Contains("FindSourcesByType")), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<XmlData>{0}</XmlData>", sourceDefs == null ? "" : string.Join("\n", sourceDefs)));

            string tableJson = string.Empty;
            envModel.Setup(e => e.Connection.ExecuteCommand(It.Is<string>(s => s.Contains("GetDatabaseTablesService")), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback((string xmlRequest, Guid workspaceID, Guid dataListID) =>
                {
                    var xml = XElement.Parse(xmlRequest);
                    var database = xml.Element("Database");
                    var dbSource = JsonConvert.DeserializeObject<DbSource>(database.Value);
                    var tables = sources[dbSource];
                    tableJson = JsonConvert.SerializeObject(tables);
                })
                .Returns(() => tableJson);

            if(eventAggregator == null)
            {
                eventAggregator = new Mock<IEventAggregator>().Object;
            }

            var modelItem = CreateModelItem();
            return new TestSqlBulkInsertDesignerViewModel(modelItem, envModel.Object, eventAggregator);
        }


        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSqlBulkInsertActivity());
        }

        static Dictionary<DbSource, List<DbTable>> CreateDatabases(int count)
        {
            var result = new Dictionary<DbSource, List<DbTable>>();

            for(var i = 0; i < count; i++)
            {
                var dbName = "Db" + i;

                var tables = new List<DbTable>();
                for(var j = 0; j < 10; j++)
                {
                    var columns = new List<DbColumn>();
                    var colCount = ((j % 4) + 1) * (i + 1);
                    for(var k = 0; k < colCount; k++)
                    {
                        var t = k % 4;
                        switch(t)
                        {
                            case 0:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, DataType = typeof(string), MaxLength = 50 });
                                break;
                            case 1:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, DataType = typeof(int) });
                                break;
                            case 2:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, DataType = typeof(double) });
                                break;
                            case 3:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, DataType = typeof(float) });
                                break;
                        }
                    }

                    tables.Add(new DbTable { TableName = dbName + "_Table_" + j, Columns = columns });
                }

                result.Add(new DbSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = dbName,
                }, tables);
            }

            return result;
        }
    }
}
