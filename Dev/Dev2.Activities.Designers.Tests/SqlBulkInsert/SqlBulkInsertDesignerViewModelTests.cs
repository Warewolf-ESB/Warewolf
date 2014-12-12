
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.QuickVariableInput;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.SqlBulkInsert
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
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
            // ReSharper disable ObjectCreationAsStatement
            new SqlBulkInsertDesignerViewModel(CreateModelItem(), null, null, null);
            // ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlBulkInsertDesignerViewModel_Constructor_ModelItem()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            var x = CreateModelItem();
            var vm = new SqlBulkInsertDesignerViewModel(x);
            Assert.AreEqual(x,vm.ModelItem);
            // ReSharper restore ObjectCreationAsStatement

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
            // ReSharper disable ObjectCreationAsStatement
            new SqlBulkInsertDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, null, null);
            // ReSharper restore ObjectCreationAsStatement

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
            // ReSharper disable ObjectCreationAsStatement
            new SqlBulkInsertDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, new Mock<IEnvironmentModel>().Object, null);
            // ReSharper restore ObjectCreationAsStatement

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
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Properties")]
        public void SqlBulkInsertDesignerViewModel_Properties()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
 
            const int DatabaseCount = 2;
            var databases = CreateDatabases(DatabaseCount);

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(modelItem, databases);
            viewModel.IsTimeoutFocused = true;
            Assert.IsTrue(viewModel.IsTimeoutFocused);
            
            viewModel.IsSelectedDatabaseFocused = true;
            Assert.IsTrue(viewModel.IsSelectedDatabaseFocused);
            
            viewModel.IsResultFocused = true;
            Assert.IsTrue(viewModel.IsResultFocused);

            Assert.IsFalse(viewModel.IsTableSelected);
            
            viewModel.IsTimeoutFocused = true;
            Assert.IsTrue(viewModel.IsTimeoutFocused);
            viewModel.IsBatchSizeFocused = true;
            Assert.IsTrue(viewModel.IsBatchSizeFocused);
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
            var selectedTable = expectedTables.Items[1];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.FullName);

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

            Assert.AreEqual(expectedTables.Items.Count, viewModel.Tables.Count);
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
            var selectedTable = selectedTables.Items[1];
            selectedTable.Columns.Add(new DbColumn {ColumnName = "monkey see"});
            var initialDatabase = databases.Keys.Skip(1).First();
            var initialTable = databases[initialDatabase].Items[3];
            initialTable.TableName = selectedTable.TableName;

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", initialDatabase);
            modelItem.SetProperty("TableName", initialTable.FullName);
            modelItem.SetProperty("InputMappings", initialTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c, InputColumn = "monkey see" }).ToList());

            var viewModel = CreateViewModel(modelItem, databases);

            Assert.AreEqual(initialTable.Columns.Count, viewModel.InputMappings.Count);

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = selectedDatabase;


            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.OnSelectedDatabaseChangedHitCount);
            Assert.AreEqual(selectedTable.TableName, viewModel.SelectedTable.TableName);

            VerifyTables(selectedTables, viewModel.Tables.ToList());

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();
            Assert.AreEqual(viewModel.InputMappings[2].InputColumn, "[[Db0_Table_1(*).monkeysee]]");
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

            var initialDatabase = databases.Keys.Skip(1).First();
            var initialTable = databases[initialDatabase].Items[3];

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
        [TestCategory("SqlBulkInsertDesignerViewModel_Database")]
        public void SqlBulkInsertDesignerViewModel_SelectedDatabase_ChangedAndTableListHasErrors_ErrorsNotNull()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            selectedTables.HasErrors = true;
            selectedTables.Errors = "There was an error.";

            var initialDatabase = databases.Keys.Skip(1).First();
            var initialTable = databases[initialDatabase].Items[3];

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

            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.Errors.Count);
            Assert.AreEqual(selectedTables.Errors, viewModel.Errors[0].Message);
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
            var dbTable = databases[dbSource].Items[0];

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = dbSource;
            viewModel.SelectedTable = dbTable;

            //------------Assert Results-------------------------
            Assert.AreEqual(dbTable.FullName, viewModel.TableName);

            Assert.IsTrue(viewModel.IsTableSelected);
            Assert.AreEqual(1, viewModel.OnSelectedTableChangedHitCount);

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(dbTable.Columns, actual);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_SelectedTable")]
        public void SqlBulkInsertDesignerViewModel_SelectedTable_ChangedAndColumnListHasErrors_ErrorsNotNull()
        {
            //------------Setup for test--------------------------
            const string ErrorMessage = "A column error occurred.";

            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases, ErrorMessage);

            var dbSource = databases.Keys.First();
            var dbTable = databases[dbSource].Items[0];

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = dbSource;
            viewModel.SelectedTable = dbTable;

            //------------Assert Results-------------------------
            Assert.AreEqual(dbTable.FullName, viewModel.TableName);

            Assert.IsTrue(viewModel.IsTableSelected);
            Assert.AreEqual(1, viewModel.OnSelectedTableChangedHitCount);

            var actual = viewModel.InputMappings.Select(m => m.OutputColumn).ToList();

            VerifyColumns(dbTable.Columns, actual);

            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.Errors.Count);
            Assert.AreEqual(ErrorMessage, viewModel.Errors[0].Message);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_SelectedTable")]
        public void SqlBulkInsertDesignerViewModel_SelectedTable_Changed_LoadsDefaultInputColumnMappings()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var selectedDatabase = databases.Keys.First();
            var selectedTable = databases[selectedDatabase].Items[0];

            //------------Execute Test---------------------------
            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;

            //------------Assert Results-------------------------
            var actualInputColumns = viewModel.InputMappings.Select(m => m.InputColumn).ToList();

            var expectedInputColumns = selectedTable.Columns.Select(c => string.Format("[[{0}(*).{1}]]", selectedTable.TableName, c.ColumnName)).ToList();

            for(var i = 0; i < expectedInputColumns.Count; i++)
            {
                Assert.AreEqual(expectedInputColumns[i], actualInputColumns[i]);
            }
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
            var selectedTable = selectedTables.Items[3];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.FullName);
            modelItem.SetProperty("InputMappings", selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c ,InputColumn = "bob the"}).ToList());

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
            var selectedTable = selectedTables.Items[3];

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);
            modelItem.SetProperty("InputMappings", selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList());

            ShowEditResourceWizardMessage message = null;
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<ShowEditResourceWizardMessage>())).Callback((object m) => message = m as ShowEditResourceWizardMessage).Verifiable();

            var resourceModel = new Mock<IResourceModel>();

            var viewModel = CreateViewModel(modelItem, databases, eventPublisher.Object, resourceModel.Object, true);

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
            var selectedTable = selectedTables.Items[3];

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
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_NotNullableColumnWithNoValue_SetsErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[1];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;

            //------------Execute Test---------------------------
            viewModel.InputMappings[0].InputColumn = string.Empty;
            viewModel.Validate();


            //------------Assert Results-------------------------
            var errors = viewModel.Errors;
            Assert.IsNotNull(errors);
            Assert.AreEqual(2, errors.Count);
            StringAssert.Contains(errors[0].Message, "Db0_Column_1_0 does not allow NULL");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_NullableColumnWithNoValue_SetsNoErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2, true);
            var viewModel = CreateViewModel(databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[1];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;

            //------------Execute Test---------------------------
            viewModel.InputMappings[0].InputColumn = string.Empty;
            viewModel.Validate();

            //------------Assert Results-------------------------
            var errors = viewModel.Errors;
            Assert.AreEqual(1,errors.Count);
            StringAssert.Contains(errors[0].Message, "'Input Data or [[Variable]]' - Recordset Field [ db0_column_1_1 ] does not exist for [ Db0_Table_1(*) ]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_IdentityColumnWithNoValueKeepIdentitySet_SetsErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2, false, true);
            var viewModel = CreateViewModel(databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";


            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[1];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;
            viewModel.ModelItem.SetProperty("KeepIdentity", true);

            //------------Execute Test---------------------------
            viewModel.InputMappings[1].InputColumn = string.Empty;
            viewModel.Validate();

            //------------Assert Results-------------------------
            var errors = viewModel.Errors;
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            StringAssert.Contains(errors[0].Message, "Db0_Column_1_1 is an IDENTITY. You must enter a mapping when the Keep Identity option is enabled.");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_IdentityColumnWithNoValueKeepIdentityNotSet_SetsNoErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2, false, true);
            var viewModel = CreateViewModel(databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";
            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[1];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;
            viewModel.ModelItem.SetProperty("KeepIdentity", false);

            //------------Execute Test---------------------------
            viewModel.InputMappings[1].InputColumn = string.Empty;
            viewModel.Validate();

            //------------Assert Results-------------------------
            var errors = viewModel.Errors;
            Assert.IsNull(errors);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_IdentityColumnWithValueKeepIdentityNotSet_SetsErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2, false, true);
            var viewModel = CreateViewModel(databases);

            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";


            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[1];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;
            viewModel.ModelItem.SetProperty("KeepIdentity", false);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            var errors = viewModel.Errors;
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            StringAssert.Contains(errors[0].Message, "Db0_Column_1_1 is an IDENTITY. You may not enter a mapping when the Keep Identity option is disabled.");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_InvalidValues_SetsErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);


            //------------Execute Test---------------------------
            viewModel.ModelItem.SetProperty("BatchSize", "");
            viewModel.ModelItem.SetProperty("Timeout", "");
            Verify_Validate_Values_SetsErrors(viewModel, false, false);

            viewModel.ModelItem.SetProperty("BatchSize", (string)null);
            viewModel.ModelItem.SetProperty("Timeout", (string)null);
            Verify_Validate_Values_SetsErrors(viewModel, false, false);

            viewModel.ModelItem.SetProperty("BatchSize", "a");
            viewModel.ModelItem.SetProperty("Timeout", "a");
            Verify_Validate_Values_SetsErrors(viewModel, false, false);

            viewModel.ModelItem.SetProperty("BatchSize", "-1");
            viewModel.ModelItem.SetProperty("Timeout", "-1");
            Verify_Validate_Values_SetsErrors(viewModel, false, false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_ValidValues_DoesNotSetErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";

            //------------Execute Test---------------------------
            viewModel.ModelItem.SetProperty("BatchSize", "0");
            viewModel.ModelItem.SetProperty("Timeout", "0");
            Verify_Validate_Values_SetsErrors(viewModel, true, true);

            viewModel.ModelItem.SetProperty("BatchSize", "20");
            viewModel.ModelItem.SetProperty("Timeout", "20");
            Verify_Validate_Values_SetsErrors(viewModel, true, true);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[1];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;
            Verify_Validate_Values_SetsErrors(viewModel, true, true);
        }

        void Verify_Validate_Values_SetsErrors(TestSqlBulkInsertDesignerViewModel viewModel, bool isBatchSizeValid, bool isTimeoutValid)
        {
            //------------Execute Test---------------------------
            viewModel.Errors = null;
            viewModel.Validate();


            //------------Assert Results-------------------------
            Assert.AreEqual(viewModel.IsDatabaseSelected, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "A database must be selected.") == null);
            Assert.AreEqual(viewModel.IsTableSelected, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "A table must be selected.") == null);
            Assert.AreEqual(isBatchSizeValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Batch size must be a number greater than or equal to zero.") == null);
            Assert.AreEqual(isTimeoutValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Timeout must be a number greater than or equal to zero.") == null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_InputMappingsHasAllEmptyInputColumns_SetsErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[3];

            var modelItem = CreateModelItem();

            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);
            modelItem.SetProperty("InputMappings", selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList());

            var viewModel = CreateViewModel(modelItem, databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";

            var inputMapping = viewModel.InputMappings.FirstOrDefault(m => !string.IsNullOrEmpty(m.InputColumn));
            Assert.IsNull(inputMapping);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.IsNotNull(viewModel.Errors.FirstOrDefault(e => e.Message == "At least one input mapping must be provided."));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_InputMappingsHasOneNonEmptyInputColumn_DoesNotSetErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[3];

            var n = 0;
            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.FullName);
            //we need to make columns null-able to make test pass ;)
            selectedTable.Columns.ForEach(c => c.IsNullable = true);
            modelItem.SetProperty("InputMappings", selectedTable.Columns
                .Select(c => new DataColumnMapping { OutputColumn = c, InputColumn = n++ == 0 ? "[[rs(*).f1]]" : null }).ToList());

            var viewModel = CreateViewModel(modelItem, databases);
            viewModel.GetDatalistString = () => "<DataList></DataList>";
            var inputMapping = viewModel.InputMappings.FirstOrDefault(m => !string.IsNullOrEmpty(m.InputColumn));
            Assert.IsNotNull(inputMapping);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
            StringAssert.Contains(viewModel.Errors[0].Message, "'Input Data or [[Variable]]' - [[rs()]] does not exist in your variable list");

        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_InvalidVariables_SetsErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[3];
            var inputMappings = selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList();
            var inputMapping = inputMappings[0];
            inputMapping.InputColumn = "rs(*).f1]]";

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);
            modelItem.SetProperty("InputMappings", inputMappings);

            var viewModel = CreateViewModel(modelItem, databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";

            //------------Execute Test---------------------------
            Verify_Validate_Variables_SetsErrors(viewModel, false, true, true, true, inputMapping.OutputColumn.ColumnName);

            inputMapping.InputColumn = null;

            modelItem.SetProperty("BatchSize", "a]]");
            modelItem.SetProperty("Timeout", "");
            modelItem.SetProperty("Result", "");
            Verify_Validate_Variables_SetsErrors(viewModel, true, false, true, true);

            modelItem.SetProperty("BatchSize", "");
            modelItem.SetProperty("Timeout", "a]]");
            modelItem.SetProperty("Result", "");
            Verify_Validate_Variables_SetsErrors(viewModel, true, true, false, true);

            modelItem.SetProperty("BatchSize", "");
            modelItem.SetProperty("Timeout", "");
            modelItem.SetProperty("Result", "a]]");
            Verify_Validate_Variables_SetsErrors(viewModel, true, true, true, false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Validate")]
        public void SqlBulkInsertDesignerViewModel_Validate_ValidVariables_DoesNotSetErrors()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);

            var selectedDatabase = databases.Keys.First();
            var selectedTables = databases[selectedDatabase];
            var selectedTable = selectedTables.Items[3];
            var inputMappings = selectedTable.Columns.Select(c => new DataColumnMapping { OutputColumn = c }).ToList();
            var inputMapping = inputMappings[0];
            inputMapping.InputColumn = "[[rs(*).f1]]";

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Database", selectedDatabase);
            modelItem.SetProperty("TableName", selectedTable.TableName);
            modelItem.SetProperty("InputMappings", inputMappings);

            var viewModel = CreateViewModel(modelItem, databases);
            viewModel.GetDatalistString = () => "<DataList><Db0_Table_1><Db0_Column_1_0/></Db0_Table_1></DataList>";
            //------------Execute Test---------------------------
            Verify_Validate_Variables_SetsErrors(viewModel, true, true, true, true, inputMapping.OutputColumn.ColumnName);

            inputMapping.InputColumn = null;

            modelItem.SetProperty("BatchSize", "[[a]]");
            modelItem.SetProperty("Timeout", "");
            modelItem.SetProperty("Result", "");
            Verify_Validate_Variables_SetsErrors(viewModel, true, true, true, true);

            modelItem.SetProperty("BatchSize", "");
            modelItem.SetProperty("Timeout", "[[a]]");
            modelItem.SetProperty("Result", "");
            Verify_Validate_Variables_SetsErrors(viewModel, true, true, true, true);

            modelItem.SetProperty("BatchSize", "");
            modelItem.SetProperty("Timeout", "");
            modelItem.SetProperty("Result", "[[a]]");
            Verify_Validate_Variables_SetsErrors(viewModel, true, true, true, true);
        }

        void Verify_Validate_Variables_SetsErrors(TestSqlBulkInsertDesignerViewModel viewModel, bool isInputMappingsValid, bool isBatchSizeValid, bool isTimeoutValid, bool isResultValid, string toField = "")
        {
            //------------Execute Test---------------------------
            viewModel.Errors = null;
            viewModel.Validate();


            //------------Assert Results-------------------------
            Assert.AreEqual(isInputMappingsValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Input Mapping To Field '" + toField + "' Invalid region detected: A close ]] without a related open [[") == null);
            Assert.AreEqual(isBatchSizeValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Batch Size Invalid region detected: A close ]] without a related open [[") == null);
            Assert.AreEqual(isTimeoutValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Timeout Invalid region detected: A close ]] without a related open [[") == null);
            Assert.AreEqual(isResultValid, viewModel.Errors == null || viewModel.Errors.FirstOrDefault(e => e.Message == "Result Invalid region detected: A close ]] without a related open [[") == null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_AddToCollection")]
        public void SqlBulkInsertDesignerViewModel_AddToCollection_AlwaysUpdatesInputMappings()
        {
            Verify_AddToCollection_AlwaysUpdatesInputMappings(true);
            Verify_AddToCollection_AlwaysUpdatesInputMappings(false);
        }

        void Verify_AddToCollection_AlwaysUpdatesInputMappings(bool overwrite)
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var selectedDatabase = databases.Keys.First();
            var selectedTable = databases[selectedDatabase].Items[2];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;

            foreach(var mapping in viewModel.InputMappings)
            {
                var expectedInputColumn = string.Format("[[{0}(*).{1}]]", selectedTable.TableName, mapping.OutputColumn.ColumnName);
                Assert.AreEqual(expectedInputColumn, mapping.InputColumn);
            }

            var expectedInputColumns = selectedTable.Columns.Select(c => string.Format("[[rs(*).{0}]]", c.ColumnName)).ToList();
            expectedInputColumns[1] = string.Empty; // randomly blank one of the values to simulate user leaving entry blank in QVI

            //------------Execute Test---------------------------
            viewModel.TestAddToCollection(expectedInputColumns, overwrite);

            //------------Assert Results-------------------------
            var actualInputColumns = viewModel.InputMappings.Select(m => m.InputColumn).ToList();
            for(var i = 0; i < expectedInputColumns.Count; i++)
            {
                Assert.AreEqual(expectedInputColumns[i], actualInputColumns[i]);
            }
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_ShowQuickVariableInputProperty")]
        public void SqlBulkInsertDesignerViewModel_ShowQuickVariableInputProperty_IsFalse_DoesNothing()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var selectedDatabase = databases.Keys.First();
            var selectedTable = databases[selectedDatabase].Items[0];

            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;

            //------------Execute Test---------------------------
            viewModel.ShowQuickVariableInput = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.QuickVariableInputViewModel.Overwrite);
            Assert.IsTrue(viewModel.QuickVariableInputViewModel.IsOverwriteEnabled);
            Assert.IsTrue(viewModel.QuickVariableInputViewModel.RemoveEmptyEntries);
            Assert.AreNotEqual(QuickVariableInputViewModel.SplitTypeNewLine, viewModel.QuickVariableInputViewModel.SplitType);
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.QuickVariableInputViewModel.VariableListString));
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.QuickVariableInputViewModel.Prefix));
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_ShowQuickVariableInputProperty")]
        public void SqlBulkInsertDesignerViewModel_ShowQuickVariableInputProperty_IsTrue_InitializesQuickVariableInputViewModel()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabases(2);
            var viewModel = CreateViewModel(databases);

            var selectedDatabase = databases.Keys.First();
            var selectedTable = databases[selectedDatabase].Items[3];


            viewModel.SelectedDatabase = selectedDatabase;
            viewModel.SelectedTable = selectedTable;

            const int BlankIndex = 2;
            viewModel.ModelItemCollection[BlankIndex].SetProperty("InputColumn", "");

            //------------Execute Test---------------------------
            viewModel.ShowQuickVariableInput = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.QuickVariableInputViewModel.Overwrite);
            Assert.IsFalse(viewModel.QuickVariableInputViewModel.IsOverwriteEnabled);
            Assert.IsFalse(viewModel.QuickVariableInputViewModel.RemoveEmptyEntries);
            Assert.AreEqual(QuickVariableInputViewModel.SplitTypeNewLine, viewModel.QuickVariableInputViewModel.SplitType);
            Assert.IsFalse(string.IsNullOrEmpty(viewModel.QuickVariableInputViewModel.VariableListString));
            Assert.IsFalse(string.IsNullOrEmpty(viewModel.QuickVariableInputViewModel.Prefix));

            var i = 0;
            var expectedVariableList = string.Join(Environment.NewLine, selectedTable.Columns.Select(c => i++ == BlankIndex ? "" : c.ColumnName));

            Assert.AreEqual(expectedVariableList, viewModel.QuickVariableInputViewModel.VariableListString);
            Assert.AreEqual(string.Format("{0}(*).", selectedTable.TableName), viewModel.QuickVariableInputViewModel.Prefix);
        }


        static void VerifyTables(DbTableList expectedTables, List<DbTable> actualTables)
        {
            for(var i = 0; i < expectedTables.Items.Count; i++)
            {
                var expected = expectedTables.Items[i];
                var actual = actualTables[i];
                Assert.AreEqual(expected.TableName, actual.TableName);
                Assert.AreEqual(expected.Columns.Count, actual.Columns.Count);
            }
        }

        static void VerifyColumns(List<IDbColumn> expected, List<IDbColumn> actual)
        {
            for(var j = 0; j < expected.Count; j++)
            {
                Assert.AreEqual(expected[j].ColumnName, actual[j].ColumnName);
                Assert.AreEqual(expected[j].SqlDataType, actual[j].SqlDataType);
                Assert.AreEqual(expected[j].DataType, actual[j].DataType);
                Assert.AreEqual(expected[j].MaxLength, actual[j].MaxLength);
            }
        }

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(Dictionary<DbSource, DbTableList> sources, string columnListErrors = "")
        {
            var modelItem = CreateModelItem();
            return CreateViewModel(modelItem, sources, false, columnListErrors);
        }

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(ModelItem modelItem, Dictionary<DbSource, DbTableList> sources, bool single = false, string columnListErrors = "")
        {
            return CreateViewModel(modelItem, sources, new Mock<IEventAggregator>().Object, new Mock<IResourceModel>().Object, single, columnListErrors);
        }

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(ModelItem modelItem, Dictionary<DbSource, DbTableList> sources, IEventAggregator eventAggregator, IResourceModel resourceModel, bool configureFindSingle = false, string columnListErrors = "")
        {
            var sourceDefs = sources == null ? null : sources.Select(s => s.Key.ToXml().ToString());

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection.WorkspaceID).Returns(Guid.NewGuid());

            var resourceRepo = new Mock<IResourceRepository>();

            envModel.Setup(e => e.Connection.ExecuteCommand(It.Is<StringBuilder>(s => s.Contains("FindSourcesByType")), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder(string.Format("<XmlData>{0}</XmlData>", sourceDefs == null ? "" : string.Join("\n", sourceDefs))));

            // return the resource repository now ;)
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

            // setup the FindSourcesByType command
            if(sources != null)
            {
                var dbs = sources.Keys.ToList();
                resourceRepo.Setup(r => r.FindSourcesByType<DbSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SqlDatabase)).Returns(dbs);
            }

            var tableJson = new DbTableList();
            // ReSharper disable ImplicitlyCapturedClosure
            resourceRepo.Setup(r => r.GetDatabaseTables(It.IsAny<DbSource>())).Callback((DbSource src) =>
            // ReSharper restore ImplicitlyCapturedClosure
            {
                if(sources != null)
                {
                    var tableList = sources[src];
                    tableJson = tableList;
                }
                // ReSharper disable ImplicitlyCapturedClosure
            }).Returns(() => tableJson);
            // ReSharper restore ImplicitlyCapturedClosure

            var columnsJson = new DbColumnList();
            // ReSharper disable ImplicitlyCapturedClosure
            resourceRepo.Setup(r => r.GetDatabaseTableColumns(It.IsAny<DbSource>(), It.IsAny<DbTable>())).Callback((DbSource src, DbTable tbl) =>
            // ReSharper restore ImplicitlyCapturedClosure
            {
                var tableName = tbl.TableName;
                if(sources != null)
                {
                    var tables = sources[src];

                    var table = tables.Items.First(t => t.TableName == tableName.Trim(new[] { '"' }));
                    var columnList = new DbColumnList();
                    columnList.Items.AddRange(table.Columns);
                    if(!string.IsNullOrEmpty(columnListErrors))
                    {
                        columnList.HasErrors = true;
                        columnList.Errors = columnListErrors;
                    }
                    columnsJson = columnList;
                }
                // ReSharper disable ImplicitlyCapturedClosure
            }).Returns(() => columnsJson);
            // ReSharper restore ImplicitlyCapturedClosure
            
            if(configureFindSingle)
            {
                envModel.Setup(e => e.ResourceRepository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(resourceModel);
            }

            return new TestSqlBulkInsertDesignerViewModel(modelItem, envModel.Object, eventAggregator);
        }


        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSqlBulkInsertActivity());
        }

        static Dictionary<DbSource, DbTableList> CreateDatabases(int count, bool varcharNullable = false, bool intAsIdentity = false)
        {
            var result = new Dictionary<DbSource, DbTableList>();

            for(var i = 0; i < count; i++)
            {
                var dbName = "Db" + i;

                var tables = new List<DbTable>();
                for(var j = 0; j < 10; j++)
                {
                    var columns = new List<IDbColumn>();
                    var colCount = ((j % 4) + 1) * (i + 1);
                    for(var k = 0; k < colCount; k++)
                    {
                        var t = k % 4;
                        switch(t)
                        {
                            case 0:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.VarChar, MaxLength = 50, IsNullable = varcharNullable });
                                break;
                            case 1:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.Int, IsAutoIncrement = intAsIdentity });
                                break;
                            case 2:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.Money });
                                break;
                            case 3:
                                columns.Add(new DbColumn { ColumnName = dbName + "_Column_" + j + "_" + k, SqlDataType = SqlDbType.Float });
                                break;
                        }
                    }

                    tables.Add(new DbTable { Schema = "MySchema", TableName = dbName + "_Table_" + j, Columns = columns });
                }

                var tableList = new DbTableList();
                tableList.Items.AddRange(tables);

                result.Add(new DbSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = dbName,
                }, tableList);
            }

            return result;
        }
    }
}
