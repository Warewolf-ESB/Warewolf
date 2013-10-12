using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Dev2.TO;
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
        public void SqlBulkInsertDesignerViewModel_Constructor_AsyncWorkerIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SqlBulkInsertDesignerViewModel(CreateModelItem(), null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlBulkInsertDesignerViewModel_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SqlBulkInsertDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, null, null);

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
            var viewModel = new SqlBulkInsertDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, new Mock<IEnvironmentModel>().Object, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        public void SqlBulkInsertDesignerViewModel_Constructor_ModelItemIsNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };
            const int DatabaseCount = 2;
            var databases = CreateDatabases(DatabaseCount);

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(modelItem, databases);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.ModelItemCollection);
            Assert.IsNotNull(viewModel.EditDatabaseCommand);
            Assert.IsNotNull(viewModel.RefreshTablesCommand);
            Assert.IsNotNull(viewModel.Databases);
            Assert.IsNotNull(viewModel.Tables);
            Assert.IsFalse(viewModel.IsDatabaseSelected);
            Assert.IsFalse(viewModel.IsTableSelected);
            Assert.IsFalse(viewModel.IsRefreshing);

            Assert.AreEqual(DatabaseCount + 2, viewModel.Databases.Count);
            Assert.AreEqual(viewModel.Databases[0], viewModel.SelectedDatabase);
            Assert.AreEqual("Select a Database...", viewModel.Databases[0].ResourceName);
            Assert.AreNotEqual(Guid.Empty, viewModel.Databases[0].ResourceID);

            Assert.AreEqual(viewModel.Tables[0], viewModel.SelectedTable);
            Assert.AreEqual("New Database Source...", viewModel.Databases[1].ResourceName);
            Assert.AreNotEqual(Guid.Empty, viewModel.Databases[1].ResourceID);

            Assert.AreEqual(1, viewModel.Tables.Count);
            Assert.AreEqual(viewModel.Tables[0], viewModel.SelectedTable);
            Assert.AreEqual("Select a Table...", viewModel.Tables[0].TableName);

            Assert.AreEqual("InputMappings", viewModel.CollectionName);

            Assert.IsNull(viewModel.Database);
            Assert.IsNull(viewModel.TableName);

            Assert.IsFalse(propertyChanged);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        public void SqlBulkInsertDesignerViewModel_Constructor_ModelItemIsNotNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            const int DatabaseCount = 2;
            var databases = CreateDatabases(DatabaseCount);
            var selectedDatabase = databases.Keys.First();
            var expectedTables = databases[selectedDatabase];
            var selectedTable = expectedTables[1];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);

            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(modelItem, databases);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.ModelItemCollection);
            Assert.IsNotNull(viewModel.EditDatabaseCommand);
            Assert.IsNotNull(viewModel.RefreshTablesCommand);
            Assert.IsNotNull(viewModel.Databases);
            Assert.IsNotNull(viewModel.Tables);
            Assert.IsTrue(viewModel.IsDatabaseSelected);
            Assert.IsTrue(viewModel.IsTableSelected);
            Assert.IsFalse(viewModel.IsRefreshing);

            Assert.AreEqual(DatabaseCount + 1, viewModel.Databases.Count);
            Assert.AreEqual(selectedDatabase, viewModel.SelectedDatabase);

            Assert.AreEqual("New Database Source...", viewModel.Databases[0].ResourceName);
            Assert.AreNotEqual(Guid.Empty, viewModel.Databases[0].ResourceID);

            Assert.AreEqual(expectedTables.Count, viewModel.Tables.Count);
            Assert.AreEqual(selectedTable.TableName, viewModel.SelectedTable.TableName);

            Assert.AreEqual("InputMappings", viewModel.CollectionName);

            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_SelectedDatabase")]
        public void SqlBulkInsertDesignerViewModel_SelectedDatabase_Changed_LoadsDatabaseTables()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var expectedDatabase = databases.Keys.First();

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = expectedDatabase;

            //------------Assert Results-------------------------
            Assert.AreSame(expectedDatabase, viewModel.Database);
            Assert.IsTrue(viewModel.IsDatabaseSelected);
            Assert.IsFalse(viewModel.IsTableSelected);
            Assert.AreEqual(1, viewModel.OnSelectedDatabaseChangedHitCount);
            Assert.AreEqual(0, viewModel.OnSelectedTableChangedHitCount);

            // Skip "Select a table" 
            VerifyTables(databases[expectedDatabase], viewModel.Tables.Skip(1).ToList());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_SelectedDatabase")]
        public void SqlBulkInsertDesignerViewModel_SelectedDatabase_ChangedAndTableNameExists_SelectsTableAndLoadsColumns()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables[1];

            var initialDatabase = databases.Keys.Skip(1).First();
            var initialTable = databases[initialDatabase][3];
            initialTable.TableName = selectedTable.TableName;

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", initialDatabase);
            modelItem.SetProperty("TableName", initialTable.TableName);
            modelItem.SetProperty("InputMappings", initialTable.Columns.Select(c => new DataColumnMapping{ OutputColumn = c }).ToList());

            var viewModel = CreateViewModel(modelItem, databases);

            Assert.AreEqual(initialTable.Columns.Count, viewModel.InputMappings.Count);

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = selectedDatabase;
            

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.OnSelectedDatabaseChangedHitCount);
            Assert.AreEqual(selectedTable.TableName, viewModel.SelectedTable.TableName);

            VerifyTables(selectedTables, viewModel.Tables.ToList());

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(selectedTable.Columns, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Database")]        
        public void SqlBulkInsertDesignerViewModel_SelectedDatabase_ChangedAndTableNameDoesNotExists_ClearsTableNameAndTableColumns()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables[1];

            var initialDatabase = databases.Keys.Skip(1).First();
            var initialTable = databases[initialDatabase][3];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", initialDatabase);
            modelItem.SetProperty("TableName", initialTable.TableName);
            modelItem.SetProperty("InputMappings", initialTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList());

            var viewModel = CreateViewModel(modelItem, databases);

            Assert.AreEqual(initialTable.Columns.Count, viewModel.InputMappings.Count);

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = selectedDatabase;


            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.OnSelectedDatabaseChangedHitCount);            
            Assert.AreEqual("Select a Table...", viewModel.SelectedTable.TableName);

            // Skip "Select a Table" 
            VerifyTables(selectedTables, viewModel.Tables.Skip(1).ToList());

            Assert.AreEqual(0, viewModel.InputMappings.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_SelectedTable")]
        public void SqlBulkInsertDesignerViewModel_SelectedTable_Changed_LoadsTableColumns()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var dbSource = databases.Keys.First();           
            var dbTable = databases[dbSource][0];

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = dbSource;
            viewModel.SelectedTable = dbTable;

            //------------Assert Results-------------------------
            Assert.AreEqual(dbTable.TableName, viewModel.TableName);

            Assert.IsTrue(viewModel.IsTableSelected);
            Assert.AreEqual(1, viewModel.OnSelectedTableChangedHitCount);

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(dbTable.Columns, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_RefreshTablesCommand")]
        public void SqlBulkInsertDesignerViewModel_RefreshTablesCommand_ReloadsTableAndColumns()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables[3];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);
            modelItem.SetProperty("InputMappings", selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList());

            var viewModel = CreateViewModel(modelItem, databases);

            //------------Execute Test---------------------------
            viewModel.RefreshTablesCommand.Execute(null);


            //------------Assert Results-------------------------
            Assert.AreEqual(selectedTable.TableName, viewModel.SelectedTable.TableName);

            VerifyTables(selectedTables, viewModel.Tables.ToList());

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(selectedTable.Columns, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_EditDbSource")]
        public void SqlBulkInsertDesignerViewModel_EditDbSource_PublishesShowEditResourceWizardMessage()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables[3];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);
            modelItem.SetProperty("InputMappings", selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList());

            ShowEditResourceWizardMessage message = null;
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<ShowEditResourceWizardMessage>())).Callback((object m) => message = m as ShowEditResourceWizardMessage).Verifiable();

            var resourceModel = new Mock<IResourceModel>();

            var viewModel = CreateViewModel(modelItem, databases, eventPublisher.Object, resourceModel.Object);

            //------------Execute Test---------------------------
            viewModel.EditDatabaseCommand.Execute(null);


            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<ShowEditResourceWizardMessage>()));
            Assert.AreSame(resourceModel.Object, message.ResourceModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_CreateDbSource")]
        public void SqlBulkInsertDesignerViewModel_CreateDbSource_PublishesShowEditResourceWizardMessage()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables[3];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);
            modelItem.SetProperty("InputMappings", selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList());

            ShowNewResourceWizard message = null;
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<ShowNewResourceWizard>())).Callback((object m) => message = m as ShowNewResourceWizard).Verifiable();

            var resourceModel = new Mock<IResourceModel>();

            var viewModel = CreateViewModel(modelItem, databases, eventPublisher.Object, resourceModel.Object);

            var createDatabase = viewModel.Databases[0];
            Assert.AreEqual("New Database Source...", createDatabase.ResourceName);

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = createDatabase;


            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<ShowNewResourceWizard>()));
            Assert.AreSame("DbSource", message.ResourceType);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_SetsErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);


            //------------Execute Test---------------------------
            viewModel.ModelItem.SetProperty("BatchSize", "a");
            viewModel.ModelItem.SetProperty("Timeout", "a");
            Verify_Validate_SetsErrors(viewModel, isBatchSizeValid: false, isTimeoutSizeValid: false);

            viewModel.ModelItem.SetProperty("BatchSize", "-1");
            viewModel.ModelItem.SetProperty("Timeout", "-1");
            Verify_Validate_SetsErrors(viewModel, isBatchSizeValid: false, isTimeoutSizeValid: false);

            viewModel.ModelItem.SetProperty("BatchSize", "0");
            viewModel.ModelItem.SetProperty("Timeout", "0");
            Verify_Validate_SetsErrors(viewModel, isBatchSizeValid: false, isTimeoutSizeValid: false);

            viewModel.ModelItem.SetProperty("BatchSize", "");
            viewModel.ModelItem.SetProperty("Timeout", "");
            Verify_Validate_SetsErrors(viewModel, isBatchSizeValid: true, isTimeoutSizeValid: true);

            viewModel.ModelItem.SetProperty("BatchSize", "20");
            viewModel.ModelItem.SetProperty("Timeout", "20");
            Verify_Validate_SetsErrors(viewModel, isBatchSizeValid: true, isTimeoutSizeValid: true);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables[1];

            viewModel.ModelItem.SetProperty("BatchSize", (string)null);
            viewModel.ModelItem.SetProperty("Timeout", (string)null);
            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;
            Verify_Validate_SetsErrors(viewModel, isBatchSizeValid: true, isTimeoutSizeValid: true);
        }

        void Verify_Validate_SetsErrors(TestSqlBulkInsertDesignerViewModel viewModel, bool isBatchSizeValid, bool isTimeoutSizeValid)
        {
            //------------Execute Test---------------------------
            viewModel.Errors = null;
            viewModel.Validate();


            //------------Assert Results-------------------------
            Assert.AreEqual(viewModel.IsDatabaseSelected, viewModel.Errors == null ||viewModel.Errors.FirstOrDefault(e => e.Message == "A database must be selected.") == null);
            Assert.AreEqual(viewModel.IsTableSelected, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "A table must be selected.") == null);
            Assert.AreEqual(isBatchSizeValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Batch size must be a number greater than zero or left blank.") == null);
            Assert.AreEqual(isTimeoutSizeValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Timeout must be a number greater than zero or left blank.") == null);
        }

        static void VerifyTables(List<DbTable> expectedTables, List<DbTable> actualTables)
        {
            for(var i = 0; i < expectedTables.Count; i++)
            {
                var expected = expectedTables[i];
                var actual = actualTables[i];
                Assert.AreEqual(expected.TableName, actual.TableName);
                Assert.AreEqual(expected.Columns.Count, actual.Columns.Count);
            }
        }

        static void VerifyColumns(List<DbColumn> expected, List<DbColumn> actual)
        {
            for(var j = 0; j < expected.Count; j++)
            {
                Assert.AreEqual(expected[j].ColumnName, actual[j].ColumnName);
                Assert.AreEqual(expected[j].SqlDataType, actual[j].SqlDataType);
                Assert.AreEqual(expected[j].DataType, actual[j].DataType);
                Assert.AreEqual(expected[j].MaxLength, actual[j].MaxLength);
            }
        }

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(Dictionary<DbSource, List<DbTable>> sources)
        {
            var modelItem = CreateModelItem();
            return CreateViewModel(modelItem, sources);
        }

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(ModelItem modelItem, Dictionary<DbSource, List<DbTable>> sources)
        {
            return CreateViewModel(modelItem, sources, new Mock<IEventAggregator>().Object, new Mock<IResourceModel>().Object);
        }

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(ModelItem modelItem, Dictionary<DbSource, List<DbTable>> sources, IEventAggregator eventAggregator, IResourceModel resourceModel)
        {
            var sourceDefs = sources == null ? null : sources.Select(s => s.Key.ToXml().ToString());

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection.WorkspaceID).Returns(Guid.NewGuid());

            envModel.Setup(e => e.Connection.ExecuteCommand(It.Is<string>(s => s.Contains("FindSourcesByType")), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<XmlData>{0}</XmlData>", sourceDefs == null ? "" : string.Join("\n", sourceDefs)));

            var tableJson = string.Empty;
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

            var columnsJson = string.Empty;
            envModel.Setup(e => e.Connection.ExecuteCommand(It.Is<string>(s => s.Contains("GetDatabaseColumnsForTableService")), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback((string xmlRequest, Guid workspaceID, Guid dataListID) =>
                {
                    var xml = XElement.Parse(xmlRequest);
                    var database = xml.Element("Database");
                    var tableName = xml.Element("TableName");

                    var dbSource = JsonConvert.DeserializeObject<DbSource>(database.Value);
                    var tables = sources[dbSource];

                    var table = tables.First(t => t.TableName == tableName.Value.Trim(new[] { '"' }));
                    columnsJson = JsonConvert.SerializeObject(table.Columns);
                })
                .Returns(() => columnsJson);

            envModel.Setup(e => e.ResourceRepository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(resourceModel);

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
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.VarChar, MaxLength = 50, });
                                break;
                            case 1:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.Int });
                                break;
                            case 2:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.Money });
                                break;
                            case 3:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.Float });
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
