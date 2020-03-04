/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Interfaces;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using System.Collections.ObjectModel;
using Dev2.Communication;
using System.Windows;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Linq;
using Warewolf.Core;
using Dev2.Common.Interfaces.ToolBase.Database;
using TSQL;
using TSQL.Statements;
using Dev2.Studio.Core.Activities.Utils;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using Dev2.Studio.Core;
using System.Text.RegularExpressions;
using Dev2.Data.Util;
using static LanguageAST.LanguageExpression;
using System.Text;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Common;
using Dev2.Activities.Utils;
using static DataStorage;

namespace Dev2.Activities.Designers2.AdvancedRecordset
{
    public class AdvancedRecordsetDesignerViewModel : CustomToolWithRegionBase, ISqliteServiceViewModel
    {
        readonly ModelItem _modelItem;
        ISqliteServiceModel Model { get; set; }
        IOutputsToolRegion _outputsRegion;
        IErrorInfo _worstDesignError;
        bool _generateOutputsVisible;
        bool _outputCountExpandAllowed;
        private ObservableCollection<INameValue> _declareVariables;
        string _recordsetName;
        const string DoneText = "Done";
        const string FixText = "Fix";
        public string SqlQuery
        {
            get
            {
                return GetProperty<string>();
            }
            set
            {
                SetProperty<string>(value);
                OnPropertyChanged();
            }
        }

        public AdvancedRecordsetDesignerViewModel(ModelItem modelItem, ISqliteServiceModel model) : base(modelItem)
        {
            _modelItem = modelItem;
            _propertyBuilder = new ViewPropertyBuilder();
            Model = model;
            SetupCommonProperties();
            SetupDeclareVariables(modelItem);
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_AdvancedRecordset;
        }

        public AdvancedRecordsetDesignerViewModel(ModelItem modelItem) : this(modelItem, new ViewPropertyBuilder()) { }
        public AdvancedRecordsetDesignerViewModel(ModelItem modelItem, IViewPropertyBuilder propertyBuilder)
           : base(modelItem)
        {
            _modelItem = modelItem;
            _propertyBuilder = propertyBuilder;
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<ISqliteServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;
            SetupCommonProperties();
            SetupDeclareVariables(_modelItem);
            this.RunViewSetup();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_AdvancedRecordset;
        }
        public IManageSqliteInputViewModel ManageServiceModel { get; set; }
        public ObservableCollection<INameValue> DeclareVariables
        {
            get => _declareVariables;
            set
            {
                _declareVariables = value;
                OnPropertyChanged(nameof(DeclareVariables));
            }
        }
        public string RecordsetName
        {
            get => _recordsetName;
            set
            {
                _recordsetName = value;
                OnPropertyChanged();
            }
        }
        void SetupCommonProperties()
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(new ManageSqliteServiceInputViewModel(this, Model));
            NoError = new ErrorInfo
            {
                ErrorType = ErrorType.None,
                Message = "Service Working Normally"
            };

            UpdateWorstError();
        }
        void InitialiseViewModel(IManageSqliteInputViewModel manageServiceModel)
        {
            ManageServiceModel = manageServiceModel;

            BuildRegions();

            LabelWidth = 46;
            ButtonDisplayValue = DoneText;

            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;
            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new DelegateCommand(o =>
            {
                IsWorstErrorReadOnly = true;
            });
            SetDisplayName("");
            OutputsRegion.OutputMappingEnabled = true;
            GenerateOutputsCommand = new DelegateCommand(command =>
            {
                GenerateOutputs();
            });
            Properties = _propertyBuilder.BuildProperties(SqlQuery, Type);
            if (OutputsRegion != null && OutputsRegion.IsEnabled)
            {
                var recordsetItem = OutputsRegion.Outputs.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                if (recordsetItem != null)
                {
                    OutputsRegion.IsEnabled = true;
                }
            }
        }
        void SetupDeclareVariables(ModelItem modelItem)
        {
            var existing = modelItem.GetProperty<IList<INameValue>>(nameof(DeclareVariables));
            var nameValues = existing ?? new List<INameValue>();
            DeclareVariables = new ObservableCollection<INameValue>();
            DeclareVariables.CollectionChanged += DeclareVariablesOnCollectionChanged;
            foreach (var nv in nameValues.Where(name => !string.IsNullOrEmpty(name.Name)))
            {
                DeclareVariables.Add(nv);
            }

            AddLastEmptyDeclareVariable();
        }

        private void AddLastEmptyDeclareVariable()
        {
            var shouldAddEmpty = !DeclareVariables.Any();

            if (!shouldAddEmpty)
            {
                var nameValue = DeclareVariables.Last();
                shouldAddEmpty = !string.IsNullOrWhiteSpace(nameValue.Name) || !string.IsNullOrWhiteSpace(nameValue.Value);
            }

            if (shouldAddEmpty)
            {
                AddDeclareVariables();
            }
        }

        private void AddDeclareVariables()
        {
            DeclareVariables.Add(new ObservableAwareNameValue(DeclareVariables, s =>
            {
                _modelItem.SetProperty(nameof(DeclareVariables), _declareVariables.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
            }));
        }

        void DeclareVariablesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddItemPropertyChangeEvent(e);
            RemoveItemPropertyChangeEvent(e);
            _modelItem.SetProperty(nameof(DeclareVariables), _declareVariables.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
        }
        void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_declareVariables.Where(name => string.IsNullOrEmpty(name.Name)).ToList().Count() >= 2)
            {
                foreach (var nv in _declareVariables.Where(name => string.IsNullOrEmpty(name.Name) && string.IsNullOrEmpty(name.Value)).ToList())
                {
                    DeclareVariables.Remove(nv);
                }

                AddDeclareVariables();
            }
            _modelItem.SetProperty(nameof(DeclareVariables), _declareVariables.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
        }

        private void GenerateOutputs()
        {
            try
            {
                OutputsRegion.Outputs.Clear();
                if (string.IsNullOrWhiteSpace(SqlQuery))
                {
                    OutputsRegion.RecordsetName = string.Empty;
                    return;
                }

                var service = ToModel();
                ManageServiceModel.Model = service;
                ValidateDeclareVariables();
                ValidateSql();
                Validate();
                _modelItem.SetProperty("SqlQuery", SqlQuery);
                ManageServiceModel.SqlQuery = SqlQuery;

                GenerateOutputsVisible = true;
            }
            catch (Exception e)
            {
                OutputsRegion.IsEnabled = false;
                OutputsRegion.Outputs = new List<IServiceOutputMapping>();
                ErrorMessage(e, true);
            }
        }
        public ISqliteService ToModel()
        {
            var sqliteService = new SqliteService();
            return sqliteService;
        }

        public StringBuilder CleanUpSql()
        {
            var sqlString = new StringBuilder();
            if (EvaluationFunctions.parseLanguageExpressionWithoutUpdate(SqlQuery, ShouldTypeCast.Yes) is ComplexExpression res)
            {
                foreach (var i in res.Item)
                {
                    switch (i)
                    {
                        case RecordSetExpression rec:
                            sqlString.Append(rec.Item.Name + "." + rec.Item.Column);
                            break;
                        case RecordSetNameExpression recSet:
                            sqlString.Append(recSet.Item.Name);
                            break;
                        case ScalarExpression scalar:
                            sqlString.Append(scalar.Item);
                            break;
                        default:
                            var atomExpression = i as WarewolfAtomExpression;
                            sqlString.Append(atomExpression.Item.ToString());
                            break;
                    }
                }
            }
            else
            {
                sqlString.Append(SqlQuery);
            }
            return sqlString;
        }
        public void ValidateSql()
        {
            try
            {
                SqlQuery = CleanUpSql().ToString();
                var statements = TSQLStatementReader.ParseStatements(SqlQuery);
                if (statements.Count > 0)
                {
                    LoadRecordsets(SqlQuery);
                    FormatSql();
                }
            }
            catch (Exception e)
            {
                ErrorMessage(e, true);
            }
        }

        readonly List<(string hashCode, IRecordSetItemModel recSet)> _hashedRecSets = new List<(string hashCode, IRecordSetItemModel recSet)>();
        void LoadRecordsets(string sqlQuery)
        {
            var advancedRecordset = new Dev2.Activities.AdvancedRecordset();
            foreach (var recSet in DataListSingleton.ActiveDataList.RecsetCollection)
            {
                if (!string.IsNullOrEmpty(recSet.DisplayName))
                {
                    var recSetHash = "A" + recSet.GetHashCode().ToString().Replace("-", "B");
                    _hashedRecSets.Add((recSetHash, recSet));
                    advancedRecordset.AddRecordsetAsTable((recSetHash, recSet.Children.Select(c => c.DisplayName).ToList()));
                }
            }
            var statements = TSQLStatementReader.ParseStatements(sqlQuery);
            var countOfStatements = statements.Count;
            if (sqlQuery.Contains("UNION") && countOfStatements == 2)
            {
                var sqlQueryToUpdate = sqlQuery;
                foreach (var item in _hashedRecSets)
                {
                    sqlQueryToUpdate = sqlQueryToUpdate.Replace(item.recSet.DisplayName, item.hashCode);
                }
                var sql = Regex.Replace(sqlQueryToUpdate, @"\@\w+\b", match => "''");
                var result = advancedRecordset.ExecuteQuery(sql);
                var table = result.Tables[0];
                if (table.Columns.Count > 0)
                {
                    var fields = GetFields(table);
                    AddToRegionOutputs(fields, table.TableName);
                    OutputsRegion.IsEnabled = OutputsRegion.Outputs.Count > 0;
                    OutputsRegion.IsOutputsEmptyRows = OutputsRegion.Outputs.Count <= 0;
                    OutputCountExpandAllowed = OutputsRegion.Outputs.Count > 3;
                }
            }
            else
            {
                LoadRecordsets(advancedRecordset, statements, countOfStatements);
            }
        }

        private void LoadRecordsets(Activities.AdvancedRecordset advancedRecordset, List<TSQLStatement> statements, int countOfStatements)
        {
            for (var i = 0; i < countOfStatements; i++)
            {
                var statement = statements[i];

                var sql = UpdateSqlWithHashCodes(statement);
                sql = Regex.Replace(sql, @"\@\w+\b", match => "''");
                var result = advancedRecordset.ExecuteStatement(statement, sql);
                if (i == countOfStatements - 1)
                {
                    var table = result.Tables[0];
                    if (table.Columns.Count > 0)
                    {
                        var fields = GetFields(table);
                        AddToRegionOutputs(fields, table.TableName);
                        OutputsRegion.IsEnabled = OutputsRegion.Outputs.Count > 0;
                        OutputsRegion.IsOutputsEmptyRows = OutputsRegion.Outputs.Count <= 0;
                        OutputCountExpandAllowed = OutputsRegion.Outputs.Count > 3;
                    }
                }
            }
        }

        private string UpdateSqlWithHashCodes(TSQLStatement statement)
        {
            var sqlBuildUp = new List<string>();
            foreach (var token in statement.Tokens)
            {
                if (token.Type == TSQL.Tokens.TSQLTokenType.Identifier && sqlBuildUp.Count >= 1)
                {
                    if (sqlBuildUp[sqlBuildUp.Count - 1] == ".")
                    {
                        sqlBuildUp.Add(token.Text);
                    }
                    else
                    {
                        var hash = _hashedRecSets.FirstOrDefault(x => x.recSet.DisplayName == token.Text);
                        sqlBuildUp.Add(!hash.Equals(default((string, IRecordSetItemModel))) ? hash.hashCode : token.Text);
                    }
                }
                else
                {
                    sqlBuildUp.Add(token.Text);
                }
            }
            return string.Join(" ", sqlBuildUp);
        }

        private static List<string> GetFields(DataTable table)
        {
            var fields = new List<string>();
            foreach (var item in table.Columns)
            {
                if (!item.ToString().Contains("Primary_Id"))
                {
                    fields.Add(item.ToString());
                }
            }

            return fields;
        }



        void FormatSql()
        {
            var replacement = Regex.Replace(SqlQuery, @"\t|\n|\r", " ");
            var statements = TSQLStatementReader.ParseStatements(replacement);

            var allStatementsCorrected = new List<string>();
            var newLineKeywords = new List<string> { "WHERE", "ORDER BY", "HAVING", "GROUP BY", "JOIN", "FROM" };
            foreach (var statement in statements)
            {
                var correctedItems = statement.Tokens.Select(s =>
                {
                    if (s.Type == TSQL.Tokens.TSQLTokenType.Keyword)
                    {
                        var uppedText = s.Text.ToUpperInvariant();
                        if (newLineKeywords.Contains(uppedText))
                        {
                            uppedText = Environment.NewLine + uppedText;
                        }
                        if (uppedText == "AS" || uppedText == "SET")
                        {
                            uppedText = " " + uppedText;
                        }
                        return uppedText + " ";
                    }
                    return s.Text + " ";
                });
                var correctedString = string.Join("", correctedItems);
                allStatementsCorrected.Add(correctedString);
            }
            SqlQuery = string.Join(Environment.NewLine, allStatementsCorrected).Replace(" . ",".");
        }

        void AddToRegionOutputs(List<string> fieldNames, string recorsetName)
        {
            var recordsetName = OutputsRegion.RecordsetName;
            if (string.IsNullOrEmpty(recordsetName))
            {
                recordsetName = recorsetName + "Copy";
                OutputsRegion.RecordsetName = recordsetName;
            }
            var outputs = new List<IServiceOutputMapping>();
            foreach (var fieldName in fieldNames)
            {
                outputs.Add(new ServiceOutputMapping(fieldName, fieldName, recordsetName));
            }
            OutputsRegion.Outputs = new ObservableCollection<IServiceOutputMapping>(outputs);
        }

        void ValidateDeclareVariables()
        {
            try
            {
                var variable = DeclareVariables.FirstOrDefault(mapping => string.IsNullOrEmpty(mapping.Value));
                if (variable != null && variable.Name != "" && variable.Value == "")
                {
                    throw new ArgumentException("Declare Variable cannot be null.");
                }
            }
            catch (Exception e)
            {
                ErrorMessage(e, true);

            }
        }

        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();

            OutputsRegion = new OutputsRegion(ModelItem);
            regions.Add(OutputsRegion);

            if (OutputsRegion.Outputs.Count > 0)
            {
                OutputsRegion.IsEnabled = true;
            }
            ErrorRegion = new ErrorRegion();
            regions.Add(ErrorRegion);
            Regions = regions;
            return regions;
        }
        void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in args.NewItems)
            {
                if (item != null)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in args.OldItems)
            {
                if (item != null)
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
        }
        public bool OutputCountExpandAllowed
        {
            get
            {
                return _outputCountExpandAllowed;
            }
            set
            {
                _outputCountExpandAllowed = value;
                OnPropertyChanged();
            }
        }
        string Type => GetProperty<string>();
        public int LabelWidth { get; set; }
        public List<KeyValuePair<string, string>> Properties { get; private set; }
        public ICommand GenerateOutputsCommand { get; set; }
        public DelegateCommand FixErrorsCommand { get; set; }
        IErrorInfo NoError { get; set; }
        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }
        public static readonly DependencyProperty WorstErrorProperty = DependencyProperty.Register("WorstError", typeof(ErrorType),
                                                                    typeof(AdvancedRecordsetDesignerViewModel), new PropertyMetadata(ErrorType.None));
        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty = DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool),
                                                                    typeof(AdvancedRecordsetDesignerViewModel), new PropertyMetadata(false));
        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public ErrorRegion ErrorRegion { get; private set; }
        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }
        public bool GenerateOutputsVisible
        {
            get
            {
                return _generateOutputsVisible;
            }
            set
            {
                _generateOutputsVisible = value;
                OnPropertyChanged();
            }
        }
        public IOutputsToolRegion OutputsRegion
        {
            get => _outputsRegion;
            set
            {
                _outputsRegion = value;
                OnPropertyChanged();
            }
        }
        public void SetDisplayName(string displayName)
        {
            var index = DisplayName.IndexOf(" -", StringComparison.Ordinal);

            if (index > 0)
            {
                DisplayName = DisplayName.Remove(index);
            }

            var displayName2 = DisplayName;

            if (!string.IsNullOrEmpty(displayName2))
            {
                DisplayName = displayName2;
            }
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                DisplayName = displayName2 + displayName;
            }
        }
        private Guid GetUniqueId() => GetProperty<Guid>();
        readonly IViewPropertyBuilder _propertyBuilder;
        public string ButtonDisplayValue { get; set; }
        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
        void UpdateWorstError()
        {
            if (DesignValidationErrors.Count == 0)
            {
                DesignValidationErrors.Add(NoError);
            }

            IErrorInfo[] worstError = { DesignValidationErrors[0] };

            foreach (var error in DesignValidationErrors.Where(error => error.ErrorType > worstError[0].ErrorType))
            {
                worstError[0] = error;
                if (error.ErrorType == ErrorType.Critical)
                {
                    break;
                }
            }
            SetWorstDesignError(worstError[0]);
        }
        void SetWorstDesignError(IErrorInfo value)
        {
            if (_worstDesignError != value)
            {
                _worstDesignError = value;
                IsWorstErrorReadOnly = value == null || value.ErrorType == ErrorType.None || value.FixType == FixType.None || value.FixType == FixType.Delete;
                WorstError = value?.ErrorType ?? ErrorType.None;
            }
        }
        public bool IsWorstErrorReadOnly
        {
            get { return (bool)GetValue(IsWorstErrorReadOnlyProperty); }
            private set
            {
                ButtonDisplayValue = value ? DoneText : FixText;
                SetValue(IsWorstErrorReadOnlyProperty, value);
            }
        }
        public void ClearValidationMemoWithNoFoundError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = GetUniqueId(),
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = GetUniqueId(),
                ErrorType = ErrorType.None,
                FixType = FixType.None,
                Message = ""
            });
            UpdateDesignValidationErrors(memo.Errors);
        }
        void UpdateDesignValidationErrors(IEnumerable<IErrorInfo> errors)
        {
            DesignValidationErrors.Clear();
            foreach (var error in errors)
            {
                DesignValidationErrors.Add(error);
            }
            UpdateWorstError();
        }
        public override void Validate()
        {
            if (Errors == null)
            {
                Errors = new List<IActionableErrorInfo>();
            }
            Errors.Clear();

            Errors = Regions.SelectMany(a => a.Errors).Select(a => new ActionableErrorInfo(new ErrorInfo { Message = a, ErrorType = ErrorType.Critical }, () => { }) as IActionableErrorInfo).ToList();
            if (Errors.Count <= 0)
            {
                ClearValidationMemoWithNoFoundError();
            }
            UpdateWorstError();
            Properties = _propertyBuilder.BuildProperties(SqlQuery, Type);
        }
        public void ErrorMessage(Exception exception, bool hasError)
        {
            Errors = new List<IActionableErrorInfo>();
            if (hasError)
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, FixData = "", FixType = FixType.None, Message = exception.Message, StackTrace = exception.StackTrace }, () => { }) };
            }
        }


    }


}
