/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Activities.Designers2.Core.QuickVariableInput;
using Dev2.Activities.Properties;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Dev2.TO;
using Dev2.Validation;

namespace Dev2.Activities.Designers2.SqlBulkInsert
{
    public class SqlBulkInsertDesignerViewModel : ActivityCollectionDesignerViewModel<DataColumnMapping>
    {
        internal Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;

        readonly IEventAggregator _eventPublisher;
        readonly IServer _server;
        readonly IAsyncWorker _asyncWorker;

        bool _isInitializing;

        static readonly DbTableList EmptyDbTables = new DbTableList();
        static readonly DbColumnList EmptyDbColumns = new DbColumnList();
        static readonly DbSource NewDbSource = new DbSource
        {
            ResourceID = Guid.NewGuid(),
            ResourceName = "New Database Source..."
        };
        static readonly DbSource SelectDbSource = new DbSource
        {
            ResourceID = Guid.NewGuid(),
            ResourceName = "Select a Database..."
        };
        static readonly DbTable SelectDbTable = new DbTable
        {
            TableName = "Select a Table..."
        };

        public SqlBulkInsertDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), ServerRepository.Instance.ActiveServer, EventPublishers.Aggregator)
        {
            this.RunViewSetup();
        }

        public SqlBulkInsertDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IServer server, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;
            VerifyArgument.IsNotNull("environmentModel", server);
            _server = server;
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;

            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();

            dynamic mi = ModelItem;
            ModelItemCollection = mi.InputMappings;

            Databases = new ObservableCollection<DbSource>();
            Tables = new ObservableCollection<DbTable>();

            EditDatabaseCommand = new RelayCommand(o => EditDbSource(), o => IsDatabaseSelected);
            RefreshTablesCommand = new RelayCommand(o => RefreshTables(), o => IsTableSelected);

            RefreshDatabases(true);
            if(SelectedDatabase != null)
            {
                IsSqlDatabase = SelectedDatabase.ServerType == enSourceType.SqlDatabase;
            }
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Database_SQL_Bulk_Insert;
        }

        #region Properties

        public override string CollectionName => "InputMappings";

        public ObservableCollection<DbSource> Databases { get; private set; }

        public ObservableCollection<DbTable> Tables { get; private set; }

        public RelayCommand EditDatabaseCommand { get; private set; }

        public RelayCommand RefreshTablesCommand { get; private set; }

        public bool IsDatabaseSelected => SelectedDatabase != SelectDbSource;

        public bool IsTableSelected => SelectedTable != SelectDbTable;

        public bool IsRefreshing { get => (bool)GetValue(IsRefreshingProperty); set => SetValue(IsRefreshingProperty, value); }

        public static readonly DependencyProperty IsRefreshingProperty =
            DependencyProperty.Register("IsRefreshing", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsSqlDatabase { get => (bool)GetValue(IsSqlDatabaseProperty); set => SetValue(IsSqlDatabaseProperty, value); }

        public static readonly DependencyProperty IsSqlDatabaseProperty =
            DependencyProperty.Register("IsSqlDatabase", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));


        public DbSource SelectedDatabase
        {
            get
            {
                return (DbSource)GetValue(SelectedDatabaseProperty);
            }
            set
            {
                SetValue(SelectedDatabaseProperty, value);
                if (SelectedDatabase != null)
                {
                    IsSqlDatabase = SelectedDatabase.ServerType == enSourceType.SqlDatabase;
                }
                EditDatabaseCommand.RaiseCanExecuteChanged();
            }
        }

        public static readonly DependencyProperty SelectedDatabaseProperty =
            DependencyProperty.Register("SelectedDatabase", typeof(DbSource), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(null, OnSelectedDatabaseChanged));

        static void OnSelectedDatabaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (SqlBulkInsertDesignerViewModel)d;
            if (viewModel.IsRefreshing)
            {
                return;
            }
            viewModel.OnSelectedDatabaseChanged();
            viewModel.EditDatabaseCommand.RaiseCanExecuteChanged();
        }


        public DbTable SelectedTable
        {
            get
            {
                return (DbTable)GetValue(SelectedTableProperty);
            }
            set
            {
                SetValue(SelectedTableProperty, value);
                RefreshTablesCommand.RaiseCanExecuteChanged();
            }
        }

        public static readonly DependencyProperty SelectedTableProperty =
            DependencyProperty.Register("SelectedTable", typeof(DbTable), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(null, OnSelectedTableChanged));

        static void OnSelectedTableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (SqlBulkInsertDesignerViewModel)d;
            if (viewModel.IsRefreshing)
            {
                return;
            }
            viewModel.OnSelectedTableChanged();
            viewModel.RefreshTablesCommand.RaiseCanExecuteChanged();
        }

        bool _isFieldFocused;
        public bool IsFieldFocused { get => _isFieldFocused; set => _isFieldFocused = value; }

        public bool IsSelectedDatabaseFocused { get => (bool)GetValue(IsSelectedDatabaseFocusedProperty); set => SetValue(IsSelectedDatabaseFocusedProperty, value); }

        public static readonly DependencyProperty IsSelectedDatabaseFocusedProperty =
            DependencyProperty.Register("IsSelectedDatabaseFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsSelectedTableFocused { get => (bool)GetValue(IsSelectedTableFocusedProperty); set => SetValue(IsSelectedTableFocusedProperty, value); }

        public static readonly DependencyProperty IsSelectedTableFocusedProperty =
            DependencyProperty.Register("IsSelectedTableFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));
        
        public bool IsMappingFieldFocused { get => (bool)GetValue(IsSelectedTableFocusedProperty); set => SetValue(IsMappingFieldFocusedProperty, value); }

        public static readonly DependencyProperty IsMappingFieldFocusedProperty =
            DependencyProperty.Register("IsMappingFieldFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));
        
        public bool IsBatchSizeFocused { get => (bool)GetValue(IsBatchSizeFocusedProperty); set => SetValue(IsBatchSizeFocusedProperty, value); }

        public static readonly DependencyProperty IsBatchSizeFocusedProperty =
            DependencyProperty.Register("IsBatchSizeFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsTimeoutFocused { get => (bool)GetValue(IsTimeoutFocusedProperty); set => SetValue(IsTimeoutFocusedProperty, value); }

        public static readonly DependencyProperty IsTimeoutFocusedProperty =
            DependencyProperty.Register("IsTimeoutFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsInputMappingsFocused
        {
            get { return (bool)GetValue(IsInputMappingsFocusedProperty); }
            set { SetValue(IsInputMappingsFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsInputMappingsFocusedProperty =
            DependencyProperty.Register("IsInputMappingsFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsResultFocused
        {
            get { return (bool)GetValue(IsResultFocusedProperty); }
            set { SetValue(IsResultFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsResultFocusedProperty =
            DependencyProperty.Register("IsResultFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));
        
        #endregion

        #region DO NOT bind to these properties - these are here for internal view model use only!!!

        DbSource Database
        {
            get { return GetProperty<DbSource>(); }
            set
            {
                if (!_isInitializing)
                {
                    SetProperty(value);
                   
                }
            }
        }

        string TableName
        {
            get { return GetProperty<string>(); }
            set
            {
                if (!_isInitializing)
                {
                    SetProperty(value);
                }
            }
        }

        string BatchSize => GetProperty<string>();

        string Timeout => GetProperty<string>();

        string Result => GetProperty<string>();

        bool KeepIdentity => GetProperty<bool>();

        #endregion

        protected virtual void OnSelectedDatabaseChanged()
        {
            if (SelectedDatabase == NewDbSource)
            {
                CreateDbSource();
                return;
            }

            IsRefreshing = true;
            IsSqlDatabase = SelectedDatabase.ServerType == enSourceType.SqlDatabase;
            var selectedTableName = GetTableName(SelectedTable);

            Databases.Remove(SelectDbSource);
            Database = SelectedDatabase;

            Tables.Clear();
            LoadDatabaseTables(() =>
            {
                SetSelectedTable(selectedTableName);
                LoadTableColumns(() => { IsRefreshing = false; });
            });
        }

        protected virtual void OnSelectedTableChanged()
        {
            if (SelectedTable != null)
            {
                IsRefreshing = true;
                Tables.Remove(SelectDbTable);
                TableName = SelectedTable.FullName;
                LoadTableColumns(() => { IsRefreshing = false; });
            }
        }

        void RefreshDatabases(bool isInitializing = false)
        {
            IsRefreshing = true;
            if (isInitializing)
            {
                _isInitializing = true;
            }

            LoadDatabases(() =>
            {
                SetSelectedDatabase(Database);
                LoadDatabaseTables(() =>
                {
                    SetSelectedTable(TableName);
                    LoadTableColumns(() =>
                    {
                        IsRefreshing = false;
                        if (isInitializing)
                        {
                            _isInitializing = false;
                        }
                    });
                });
            });
        }

        void RefreshTables()
        {
            IsRefreshing = true;

            LoadDatabaseTables(() =>
            {
                SetSelectedTable(TableName);

                LoadTableColumns(() =>
                {
                    IsRefreshing = false;
                });
            });
        }

        void LoadDatabases(System.Action continueWith = null)
        {
            Databases.Clear();
            Databases.Add(NewDbSource);

            _asyncWorker.Start(() => GetDatabases().OrderBy(r => r.ResourceName), databases =>
            {
                foreach (var database in databases)
                {
                    Databases.Add(database);
                }
                continueWith?.Invoke();
            });
        }

        void LoadDatabaseTables(System.Action continueWith = null)
        {
            Tables.Clear();

            if (!IsDatabaseSelected)
            {
                continueWith?.Invoke();
                return;
            }
            
            var selectedDatabase = SelectedDatabase;
            _asyncWorker.Start(() => GetDatabaseTables(selectedDatabase), tableList =>
            {
                Errors = tableList.HasErrors ? new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(() => IsSelectedTableFocused = true) { ErrorType = ErrorType.Critical, Message = tableList.Errors }
                    } : null;
                foreach (var table in tableList.Items.OrderBy(t => t.TableName))
                {
                    Tables.Add(table);
                }
                continueWith?.Invoke();
            });
        }

        void LoadTableColumns(System.Action continueWith = null)
        {
            if (!IsTableSelected || _isInitializing)
            {
                if (!_isInitializing)
                {
                    ModelItemCollection.Clear();
                }
                continueWith?.Invoke();
                return;
            }

            var oldColumns = GetInputMappings().ToList();
            ModelItemCollection.Clear();
            var selectedDatabase = SelectedDatabase;
            var selectedTable = SelectedTable;
            
            _asyncWorker.Start(() => GetDatabaseTableColumns(selectedDatabase, selectedTable), columnList =>
            
            {
                Errors = columnList.HasErrors ? new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(() => IsSelectedTableFocused = true) { ErrorType = ErrorType.Critical, Message = columnList.Errors }
                    } : null;
                foreach (var mapping in columnList.Items.Select(column => new DataColumnMapping { OutputColumn = column }))
                {
                    var mapping1 = mapping;
                    var oldColumn = oldColumns.FirstOrDefault(c => c.OutputColumn.ColumnName == mapping1.OutputColumn.ColumnName);
                    if (oldColumn?.InputColumn != null)
                    {
                        var inputcolumn = oldColumn.InputColumn;
                        inputcolumn = inputcolumn.Replace("[", "");
                        inputcolumn = inputcolumn.Replace("]", "");
                        inputcolumn = inputcolumn.Replace(" ", "");
                        mapping.InputColumn = string.Format("[[{0}]]", inputcolumn);
                    }
                    if (string.IsNullOrEmpty(mapping.InputColumn))
                    {
                        mapping.InputColumn = string.Format("[[{0}(*).{1}]]", selectedTable.TableName.Replace("[", "").Replace("]", "").Replace(" ", ""), mapping.OutputColumn.ColumnName.Replace("[", "").Replace("]", "").Replace(" ", ""));
                    }

                    ModelItemCollection.Add(mapping);
                }
                continueWith?.Invoke();
            });
        }

        void EditDbSource()
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            if(shellViewModel != null)
            {
                shellViewModel.OpenResource(SelectedDatabase.ResourceID,_server.EnvironmentID, shellViewModel.ActiveServer);
                RefreshDatabases();
            }            
        }

        void CreateDbSource()
        {
            CustomContainer.Get<IShellViewModel>().NewSqlServerSource(string.Empty);
            RefreshDatabases();
        }

        IEnumerable<DbSource> GetDatabases()
        {
            var dbSources = _server.ResourceRepository.FindSourcesByType<DbSource>(_server, enSourceType.SqlDatabase);
            return dbSources.Where(source => source.ResourceType== "SqlDatabase" || source.ServerType==enSourceType.SqlDatabase);
        }

        DbTableList GetDatabaseTables(DbSource dbSource)
        {
            var tables = _server.ResourceRepository.GetDatabaseTables(dbSource);
            return tables ?? EmptyDbTables;
        }

        IDbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable)
        {
            var columns = _server.ResourceRepository.GetDatabaseTableColumns(dbSource, dbTable);
            return columns ?? EmptyDbColumns;
        }

        void SetSelectedDatabase(DbSource dbSource)
        {
            var selectedDatabase = dbSource == null ? null : Databases.FirstOrDefault(d => d.ResourceID == dbSource.ResourceID);
            if (selectedDatabase == null)
            {
                if (Databases.FirstOrDefault(d => d.Equals(SelectDbSource)) == null)
                {
                    Databases.Insert(0, SelectDbSource);
                }
                selectedDatabase = SelectDbSource;
            }
            SelectedDatabase = selectedDatabase;
        }

        void SetSelectedTable(string tableName)
        {
            var selectedTable = Tables.FirstOrDefault(t => t.FullName == tableName);
            if (selectedTable == null)
            {
                if (Tables.FirstOrDefault(t => t.Equals(SelectDbTable)) == null)
                {
                    Tables.Insert(0, SelectDbTable);
                }
                selectedTable = SelectDbTable;
            }
            SelectedTable = selectedTable;
        }

        static string GetTableName(DbTable table) => table?.FullName;

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            var errors = new List<IActionableErrorInfo>();
            errors.AddRange(ValidateValues());
            errors.AddRange(ValidateVariables());
            return errors;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            yield break;
        }

        IEnumerable<IActionableErrorInfo> ValidateValues()
        {
            if (!IsDatabaseSelected)
            {
                yield return new ActionableErrorInfo(() => IsSelectedDatabaseFocused = true) { ErrorType = ErrorType.Critical, Message = ActivityResources.DatabaseMustBeSelectedMsg };
            }
            if (!IsTableSelected)
            {
                yield return new ActionableErrorInfo(() => IsSelectedTableFocused = true) { ErrorType = ErrorType.Critical, Message = ActivityResources.TableMustBeSelectedMsg };
            }

            var batchSize = BatchSize;
            var value = 0;
            if (!IsVariable(batchSize) && (!int.TryParse(batchSize, out value) || value < 0))
            {
                yield return new ActionableErrorInfo(() => IsBatchSizeFocused = true) { ErrorType = ErrorType.Critical, Message = ActivityResources.BatchsizeMustBeNumberMsg };
            }


            var timeout = Timeout;
            if (!IsVariable(timeout) && (!int.TryParse(timeout, out value) || value < 0))
            {
                yield return new ActionableErrorInfo(() => IsTimeoutFocused = true) { ErrorType = ErrorType.Critical, Message = ActivityResources.TimeoutMustBeNumberMsg };
            }


            var nonEmptyCount = ModelItemCollection.Count(mi => !string.IsNullOrEmpty(((DataColumnMapping)mi.GetCurrentValue()).InputColumn));
            if (nonEmptyCount == 0)
            {
                yield return new ActionableErrorInfo(() => IsInputMappingsFocused = true) { ErrorType = ErrorType.Critical, Message = ActivityResources.AtLeastOneMappingMsg };
            }
        }

        IEnumerable<IActionableErrorInfo> ValidateVariables()
        {
            var parser = new Dev2DataLanguageParser();
            var allActionableErrors = new List<IActionableErrorInfo>();
            AddBatchSizeError(parser, ref allActionableErrors);
            AddTimeoutError(parser, ref allActionableErrors);
            AddResultError(parser, ref allActionableErrors);

            foreach (var dc in GetInputMappings())
            {
                var output = dc.OutputColumn;
                var inputColumn = dc.InputColumn;
                var identityChecked = false;

                if (output.IsAutoIncrement)
                {
                    if (KeepIdentity && string.IsNullOrEmpty(inputColumn))
                    {
                        var msg = string.Format(ActivityResources.IdentityWithKeepOptionEnabledMsg, output.ColumnName);
                        allActionableErrors.Add(new ActionableErrorInfo(() => IsInputMappingsFocused = true) { ErrorType = ErrorType.Critical, Message = msg });
                    }

                    if (!KeepIdentity && !string.IsNullOrEmpty(inputColumn))
                    {
                        var msg = string.Format(ActivityResources.IdentityWithKeepOptionDisabledMsg, output.ColumnName);
                        allActionableErrors.Add(new ActionableErrorInfo(() => IsInputMappingsFocused = true) { ErrorType = ErrorType.Critical, Message = msg });
                    }

                    identityChecked = true;
                }

                if (!output.IsNullable && string.IsNullOrEmpty(inputColumn) && !identityChecked)
                {
                    var msg = string.Format(ActivityResources.NotNullableMsg, output.ColumnName);
                    allActionableErrors.Add(new ActionableErrorInfo(() => IsInputMappingsFocused = true) { ErrorType = ErrorType.Critical, Message = msg });
                }

                AddInputMapToFieldError(parser, output, inputColumn, ref allActionableErrors);

                if (!identityChecked)
                {
                    var rs = GetRuleSet("InputColumn", inputColumn).ValidateRules("'Input Data or [[Variable]]'", () => ModelItem.SetProperty("IsMappingFieldFocused", true));

                    foreach (var looperror in rs)
                    {
                        allActionableErrors.Add(looperror);
                    }
                }
            }
            return allActionableErrors;
        }

        void AddInputMapToFieldError(Dev2DataLanguageParser parser, IDbColumn output, string inputColumn, ref List<IActionableErrorInfo> allActionableErrors)
        {
            var error = ValidateVariable(parser, inputColumn, () => IsInputMappingsFocused = true);
            if (error != null)
            {
                error.Message = "Input Mapping To Field '" + output.ColumnName + "' " + error.Message;
                allActionableErrors.Add(error);
            }
        }

        void AddResultError(Dev2DataLanguageParser parser, ref List<IActionableErrorInfo> allActionableErrors)
        {
            var error = ValidateVariable(parser, Result, () => IsResultFocused = true);
            if (error != null)
            {
                error.Message = "Result " + error.Message;
                allActionableErrors.Add(error);
            }
        }

        void AddTimeoutError(Dev2DataLanguageParser parser, ref List<IActionableErrorInfo> allActionableErrors)
        {
            var error = ValidateVariable(parser, Timeout, () => IsTimeoutFocused = true);
            if (error != null)
            {
                error.Message = "Timeout " + error.Message;
                allActionableErrors.Add(error);
            }
        }

        void AddBatchSizeError(Dev2DataLanguageParser parser, ref List<IActionableErrorInfo> allActionableErrors)
        {
            var error = ValidateVariable(parser, BatchSize, () => IsBatchSizeFocused = true);
            if (error != null)
            {
                error.Message = "Batch Size " + error.Message;
                allActionableErrors.Add(error);
            }
        }

        static IActionableErrorInfo ValidateVariable(Dev2DataLanguageParser parser, string variable, System.Action focusAction)
        {
            if (!string.IsNullOrEmpty(variable))
            {
                try
                {
                    parser.MakeParts(variable);
                }
                catch (Exception ex)
                {
                    return new ActionableErrorInfo(focusAction) { ErrorType = ErrorType.Critical, Message = ex.Message };
                }
            }
            return null;
        }

        public IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();
            if (propertyName == "InputColumn")
            {
                ruleSet.Add(new IsValidExpressionRule(() => datalist, GetDatalistString?.Invoke(), "1", new VariableUtils()));
            }
            return ruleSet;
        }

        static bool IsVariable(string variable)
        {
            var regions = DataListCleaningUtils.SplitIntoRegions(variable).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            return regions.Count > 0;
        }

        IEnumerable<DataColumnMapping> GetInputMappings() => ModelItemCollection.Select(mi => (DataColumnMapping)mi.GetCurrentValue());

        protected override void AddToCollection(IEnumerable<string> sources, bool overwrite)
        {
            var newMappings = sources.ToList();
            var max = Math.Min(newMappings.Count, ItemCount);
            for (var i = 0; i < max; i++)
            {
                var mi = ModelItemCollection[i];
                mi.SetProperty("InputColumn", newMappings[i]);
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            base.OnToggleCheckedChanged(propertyName, isChecked);

            if (propertyName == ShowQuickVariableInputProperty.Name)
            {
                if (!ShowQuickVariableInput)
                {
                    return;
                }

                var mappings = GetInputMappings().ToList();
                QuickVariableInputViewModel.Overwrite = true;
                QuickVariableInputViewModel.IsOverwriteEnabled = false;
                QuickVariableInputViewModel.RemoveEmptyEntries = false;
                QuickVariableInputViewModel.SplitType = QuickVariableInputViewModel.SplitTypeNewLine;
                QuickVariableInputViewModel.VariableListString = string.Join(Environment.NewLine, mappings.Select(GetFieldName));
                QuickVariableInputViewModel.Prefix = GetRecordsetName(mappings) + "(*).";
            }
        }

        static string GetFieldName(DataColumnMapping dc) => string.IsNullOrEmpty(dc.InputColumn) ? string.Empty : DataListUtil.ExtractFieldNameFromValue(dc.InputColumn);

        string GetRecordsetName(IEnumerable<DataColumnMapping> mappings)
        {
            var rsName = mappings.Select(m => DataListUtil.ExtractRecordsetNameFromValue(m.InputColumn)).FirstOrDefault(rs => !string.IsNullOrEmpty(rs));
            return string.IsNullOrEmpty(rsName) ? TableName : rsName;
        }
    }
}
