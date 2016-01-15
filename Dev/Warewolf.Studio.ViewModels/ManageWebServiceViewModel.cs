
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;
// ReSharper disable VirtualMemberCallInContructor

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebServiceViewModel : SourceBaseImpl<IWebService>, IManageWebServiceViewModel
    {
        readonly IWebServiceModel _model;
        readonly Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        WebRequestMethod _selectedWebRequestMethod;
        ICollection<WebRequestMethod> _webRequestMethods;
        ICommand _newWebSourceCommand;
        ObservableCollection<IWebServiceSource> _sources;
        IWebServiceSource _selectedSource;
        IWebService _webService;
        ICollection<NameValue> _headers;
        string _requestUrlQuery;
        string _sourceUrl;
        string _requestBody;
        ICollection<NameValue> _variables;
        string _response;
        ICommand _pastResponseCommand;
        ICommand _testCommand;
        ICommand _saveCommand;
        ICommand _cancelCommand;
        ICollection<IServiceInput> _inputs;
        string _outputName;
        string _resourceName;
        IList<IServiceOutputMapping> _outputMapping;
        string _errorMessage;
        bool _isTesting;
        bool _canEditMappings;
        string _name;
        Guid _id;
        string _path;
        bool _canEditHeadersAndUrl;
        string _recordsetName;
        bool _canEditResponse;
        bool _isInputsEmptyRows;
        bool _isOutputMappingEmptyRows;
        string _headerText;

        #region Implementation of IManageWebServiceViewModel

        public ManageWebServiceViewModel(IWebServiceModel model, Task<IRequestServiceNameViewModel> saveDialog)
            : base(ResourceType.WebService)
        {
            _model = model;
            _requestServiceNameViewModel = saveDialog;
            Init();
            if(_model.UpdateRepository != null)
            {
                _model.UpdateRepository.WebServiceSourceSaved += UpdateSourcesCollection;
            }
        }

        void Init()
        {
            WebRequestMethods = new ObservableCollection<WebRequestMethod>(Dev2EnumConverter.GetEnumsToList<WebRequestMethod>());
            
            Inputs = new ObservableCollection<IServiceInput>();
            OutputMapping = new ObservableCollection<IServiceOutputMapping>();
            var variables = new ObservableCollection<NameValue>();
            variables.CollectionChanged += VariablesOnCollectionChanged;
            Variables = variables;
            var headerCollection = new ObservableCollection<NameValue>();
            headerCollection.CollectionChanged += HeaderCollectionOnCollectionChanged;
            Headers = headerCollection;
            Headers.Add(new ObservableAwareNameValue(headerCollection, UpdateRequestVariables));
            RequestBody = "";
            Response = "";
            try
            {
                Sources = Model.Sources;               
            }
            catch (Exception ex)
            {
                Exception exception = new Exception();
                if (ex.InnerException != null)
                {
                    exception = ex.InnerException;
                }
                ErrorMessage = exception.Message;
            }
            Header = Resources.Languages.Core.WebserviceTabHeader;
            HeaderText = Resources.Languages.Core.WebserviceTabHeader;
            ResourceName = Resources.Languages.Core.WebserviceTabHeader;
            TestCommand = new DelegateCommand(() => Test(_model, ToModel()), CanTest);

            SaveCommand = new DelegateCommand(Save, CanSave);
            NewWebSourceCommand = new DelegateCommand(_model.CreateNewSource);
            EditWebSourceCommand = new DelegateCommand(() => _model.EditSource(SelectedSource));
            PasteResponseCommand = new DelegateCommand(HandlePasteResponse);
            RemoveHeaderCommand = new DelegateCommand(DeleteCell);
            AddHeaderCommand = new DelegateCommand(Add);
        }

        public ManageWebServiceViewModel(IWebServiceModel model, IWebService service)
            : base(ResourceType.WebService)
        {
            _model = model;
            _model.UpdateRepository.WebServiceSourceSaved += UpdateSourcesCollection;
            try
            {
                Sources = _model.Sources;
            }
            catch (Exception ex)
            {
                Exception exception = new Exception();
                if (ex.InnerException != null)
                {
                    exception = ex.InnerException;
                }
                ErrorMessage = exception.Message;
            }
            _webService = service;
            Init();
            Item = service;
            FromModel(service);
        }

        public override void FromModel(IWebService service)
        {
            Name = service.Name;
            HeaderText = service.Name;
            Header = service.Name;
            Id = service.Id;
            Path = service.Path;
            SelectedSource = Sources.FirstOrDefault(a => a.Id == service.Id);
            Item.Source = SelectedSource;
            if (service.Headers != null)
            {
                foreach (var nameValue in service.Headers)
                {
                    Headers.Add(nameValue);
                }
            }
            else
            {
                service.Headers = new List<NameValue>();
            }

            if (SelectedSource != null)
            {
                if (SelectedSource.HostName != null)
                {
                    RequestUrlQuery = String.IsNullOrEmpty(SelectedSource.HostName)? RequestUrlQuery: service.QueryString.Replace(SelectedSource.HostName, "");
                }
                Item.RequestUrl = RequestUrlQuery;
            }
            else
            {
                RequestUrlQuery = "";
            }
            RequestUrlQuery = service.RequestUrl;
            Inputs = service.Inputs;
            OutputMapping = service.OutputMappings;
            CanEditMappings = true;
            CanEditResponse = true;

        }

        void UpdateSourcesCollection(IWebServiceSource serviceSource)
        {
            var webServiceSource = Sources.FirstOrDefault(source => source.Id == serviceSource.Id);
            if (webServiceSource != null)
            {
                var indexOf = Sources.IndexOf(webServiceSource);
                Sources[indexOf] = serviceSource;
                if (_selectedSource != null)
                {
                    RequestUrlQuery = _selectedSource.DefaultQuery ?? "";
                    SourceUrl = _selectedSource.HostName;
                    CanEditHeadersAndUrl = true;
                    ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                }
                else
                {
                    CanEditHeadersAndUrl = false;
                }

                ViewModelUtils.RaiseCanExecuteChanged(EditWebSourceCommand);
            }
            else
            {
                Sources.Add(serviceSource);
            }
        }

        void HandlePasteResponse()
        {
            Response = _model.HandlePasteResponse(Response ?? "");
            var resp = ToModel();
            resp.Response = Response;
            Test(_model, resp);
        }

        private void DeleteCell()
        {
            Headers.Remove(SelectedRow);
        }
        private void Add()
        {
            Headers.Remove(SelectedRow);
        }

        void VariablesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Inputs.Clear();
            var newInputs = MapVariablesToInputs();
            Inputs = newInputs;
        }

        List<IServiceInput> MapVariablesToInputs()
        {
            return Variables.Select(nameValue => new ServiceInput(DataListUtil.RemoveLanguageBrackets(nameValue.Name), nameValue.Value)).Cast<IServiceInput>().ToList();
        }

        bool CanTest()
        {
            return SelectedSource != null;
        }

        public override void Save()
        {
            try
            {
                if (_webService == null)
                {
                    var saveOutPut = RequestServiceNameViewModel.ShowSaveDialog();
                    if (saveOutPut == MessageBoxResult.OK || saveOutPut == MessageBoxResult.Yes)
                    {
                        Name = RequestServiceNameViewModel.ResourceName.Name;
                        Path = RequestServiceNameViewModel.ResourceName.Path;
                        Id = Guid.NewGuid();
                        _model.SaveService(ToModel());
                        Item = ToModel();
                        Header = Path + Name;
                        _webService = ToModel();
                    }
                }
                else
                {
                    _model.SaveService(ToModel());
                    Item = ToModel();
                }
                ErrorMessage = "";
            }
            catch (Exception err)
            {

                ErrorMessage = err.Message;
            }
        }

        void Test(IWebServiceModel model, IWebService service)
        {
            try
            {
                IsTesting = true;
                var serializer = new Dev2JsonSerializer();
                ResponseWebService = serializer.Deserialize<WebService>(model.TestService(service));
                UpdateMappingsFromResponse();
                ErrorMessage = "";
                CanEditMappings = true;
                CanEditResponse = true;
                IsTesting = false;
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
            catch (Exception err)
            {
                ErrorMessage = err.Message;
                OutputMapping = new ObservableCollection<IServiceOutputMapping>();
                IsTesting = false;
                CanEditMappings = false;
                CanEditResponse = false;
            }
        }

        WebService ResponseWebService { get; set; }

        void UpdateMappingsFromResponse()
        {
            if (ResponseWebService != null)
            {
                Response = ResponseWebService.RequestResponse;
                var outputMapping = ResponseWebService.Recordsets.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                {
                    RecordsetName = recordset.Name;
                    var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name);
                    return serviceOutputMapping;
                }).Cast<IServiceOutputMapping>().ToList();

                OutputMapping = outputMapping;
            }
        }

        public override bool CanSave()
        {
            return !String.IsNullOrEmpty(Response);
        }

        void HeaderCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            foreach (var nameValue in Headers)
            {
                UpdateRequestVariables(nameValue.Value);
                UpdateRequestVariables(nameValue.Name);
            }
        }

        void UpdateRequestVariables(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                RemoveUnused();
                return;
            }
            var exp = WarewolfDataEvaluationCommon.ParseLanguageExpression(name, 0);
                if (Variables == null || Variables.Any(a => a.Name == name))
            {
                return;
            }
            if (exp.IsScalarExpression)
            {
                var scalar = exp as LanguageAST.LanguageExpression.ScalarExpression;
                Variables.Add(new NameValue { Name = WarewolfDataEvaluationCommon.LanguageExpressionToString(scalar), Value = "" });
            }
            if (exp.IsRecordSetExpression)
            {
                var rec = exp as LanguageAST.LanguageExpression.RecordSetExpression;
                Variables.Add(new NameValue { Name = WarewolfDataEvaluationCommon.LanguageExpressionToString(rec), Value = "" });
            }
            if (exp.IsComplexExpression)
            {
                var rec = exp as LanguageAST.LanguageExpression.ComplexExpression;
                // ReSharper disable PossibleNullReferenceException
                foreach (var languageExpression in rec.Item)
                // ReSharper restore PossibleNullReferenceException
                {
                    UpdateRequestVariables(WarewolfDataEvaluationCommon.LanguageExpressionToString(languageExpression));
                }
            }
            RemoveUnused();
        }

        void RemoveUnused()
        {
            IList<NameValue> unused = new List<NameValue>();
            if (Variables != null)
            {
                foreach (var nameValue in Variables)
                {
                    if (RequestBody != null && (RequestUrlQuery != null && (!RequestUrlQuery.Contains(nameValue.Name) && !RequestBody.Contains(nameValue.Name) && !Headers.Any(a => a.Name.Contains(nameValue.Name) || a.Value.Contains(nameValue.Name)))))
                    {
                        unused.Add(nameValue);
                    }
                }
            }
            foreach (var nameValue in unused)
            {
                if (Variables != null)
                {
                    Variables.Remove(nameValue);
                }
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }

        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged(() => Id);
            }
        }

        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged(() => Name);
            }
        }
        public bool CanEditHeadersAndUrl
        {
            get
            {
                return _canEditHeadersAndUrl;
            }
            set
            {
                _canEditHeadersAndUrl = value;
                OnPropertyChanged(() => CanEditHeadersAndUrl);
            }
        }
        public bool CanEditResponse
        {
            get
            {
                return _canEditResponse;
            }
            set
            {
                _canEditResponse = value;
                OnPropertyChanged(() => CanEditResponse);
            }
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public ICommand AddHeaderCommand { get; set; }
        public ICommand RemoveHeaderCommand { get; private set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public NameValue SelectedRow { get; set; }
        public ICollection<object> SelectedDataItems { get; set; }

        public bool CanEditMappings
        {
            get
            {
                return _canEditMappings;
            }
            set
            {
                _canEditMappings = value;
                OnPropertyChanged(() => CanEditMappings);
            }
        }

        public bool IsTesting
        {
            get
            {
                return _isTesting;
            }
            set
            {
                _isTesting = value;
                OnPropertyChanged(() => IsTesting);
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(ErrorMessage);
            }
        }

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                OnPropertyChanged(_resourceName);
            }
        }
        public string HeaderText
        {
            get { return _headerText; }
            private set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }

        /// <summary>
        /// currently selected web resquest method post get
        /// </summary>
        public WebRequestMethod SelectedWebRequestMethod
        {
            get
            {
                return _selectedWebRequestMethod;
            }
            set
            {
                _selectedWebRequestMethod = value;
                OnPropertyChanged(() => SelectedWebRequestMethod);
                OnPropertyChanged(() => RequestBodyEnabled);
            }
        }
        public bool RequestBodyEnabled
        {
            get
            {
                return SelectedWebRequestMethod != WebRequestMethod.Get;
            }
            set
            {
            }
        }
        /// <summary>
        /// the collections of supported web request methods
        /// </summary>
        public ICollection<WebRequestMethod> WebRequestMethods
        {
            get
            {
                return _webRequestMethods;
            }
            set
            {
                _webRequestMethods = value;
                OnPropertyChanged(() => WebRequestMethods);
            }
        }
        /// <summary>
        /// Command to create a new web source 
        /// </summary>
        public ICommand NewWebSourceCommand
        {
            get
            {
                return _newWebSourceCommand;
            }
            set
            {
                if(value != null)
                {
                _newWebSourceCommand = value;
                OnPropertyChanged(() => NewWebSourceCommand);
            }
                else
                {
                    _newWebSourceCommand = null;
        }
            }
        }
        /// <summary>
        /// Command to create a new web source 
        /// </summary>
        public ICommand EditWebSourceCommand { get; set; }
        /// <summary>
        /// Available Sources
        /// </summary>
        public ObservableCollection<IWebServiceSource> Sources
        {
            get
            {
                return _sources;
            }
            // ReSharper disable MemberCanBePrivate.Global
            set
            // ReSharper restore MemberCanBePrivate.Global
            {
                _sources = value;
                OnPropertyChanged(() => Sources);
            }
        }
        /// <summary>
        /// Currently Selected Source
        /// </summary>
        public IWebServiceSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                if (!Equals(_selectedSource, value))
                {
                    _selectedSource = value;
                    OnPropertyChanged(() => SelectedSource);
                    PerformRefresh();
                    if (_selectedSource != null)
                    {
                        RequestUrlQuery = _selectedSource.DefaultQuery ?? "";
                        SourceUrl = _selectedSource.HostName;
                        CanEditHeadersAndUrl = true;
                        ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                    }
                    else
                    {
                        CanEditHeadersAndUrl = false;
                    }

                    ViewModelUtils.RaiseCanExecuteChanged(EditWebSourceCommand);
                }
            }
        }

        private void PerformRefresh()
        {
            try
            {
                //Init();
                CanEditHeadersAndUrl = false;
                CanEditResponse = false;
                CanEditMappings = false;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }

        /// <summary>
        /// The underlying Web service
        /// </summary>
        public IWebService WebService
        {
            get
            {
                return _webService;
            }
            set
            {
                _webService = value;
            }
        }

        /// <summary>
        /// Request headers
        /// </summary>
        public ICollection<NameValue> Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
                if (_headers != null)
                {
                    foreach (var nameValue in _headers)
                    {
                        UpdateRequestVariables(nameValue.Name);
                        UpdateRequestVariables(nameValue.Value);
                    }

                }
                OnPropertyChanged(() => Headers);
            }
        }
        /// <summary>
        /// The Web service query string
        /// </summary>
        public string RequestUrlQuery
        {
            get
            {
                return _requestUrlQuery;
            }
            set
            {
                _requestUrlQuery = value;
                UpdateRequestVariables(RequestUrlQuery);
                OnPropertyChanged(() => RequestUrlQuery);
            }
        }
        /// <summary>
        /// The URL as per the Source
        /// </summary>
        public string SourceUrl
        {
            get
            {
                return _sourceUrl;
            }
            set
            {
                _sourceUrl = value;
                OnPropertyChanged(() => SourceUrl);
            }
        }
        /// <summary>
        /// The Request Body
        /// </summary>
        public string RequestBody
        {
            get
            {
                return _requestBody;
            }
            set
            {
                _requestBody = value;
                UpdateRequestVariables(value);
                OnPropertyChanged(() => RequestBody);
            }
        }
        /// <summary>
        /// the warewolf variables defined in the body,headers and query string
        /// </summary>
        public ICollection<NameValue> Variables
        {
            get
            {
                return _variables;
            }
            set
            {
                _variables = value;
                OnPropertyChanged(() => Variables);
            }
        }
        /// <summary>
        /// the response from the web service
        /// </summary>
        public string Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value;
                OnPropertyChanged(() => Response);
            }
        }
        /// <summary>
        /// the command to paste a command into the response
        /// </summary>
        public ICommand PasteResponseCommand
        {
            get
            {
                return _pastResponseCommand;
            }
            set
            {
                _pastResponseCommand = value;
                OnPropertyChanged(() => PasteResponseCommand);
            }
        }
        /// <summary>
        /// Test a web Service
        /// </summary>
        public ICommand TestCommand
        {
            get
            {
                return _testCommand;
            }
            set
            {
                _testCommand = value;
                OnPropertyChanged(() => TestCommand);
            }
        }
        /// <summary>
        /// Text for the Test button
        /// </summary>
        public string TestCommandButtonText { get; set; }
        /// <summary>
        /// Text for the Save button
        /// </summary>
        public string SaveCommandText { get; set; }
        /// <summary>
        /// command to save
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand;
            }
            set
            {
                _saveCommand = value;
                OnPropertyChanged(() => SaveCommand);
            }
        }
        /// <summary>
        /// Cancel Command
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand;
            }
            set
            {
                _cancelCommand = value;
                OnPropertyChanged(() => CancelCommand);
            }
        }

        /// <summary>
        /// List Of Inputs
        /// </summary>
        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            private set
            {
                _inputs = value;
                IsInputsEmptyRows = true;
                if (_inputs != null && _inputs.Count >= 1)
                {
                    IsInputsEmptyRows = false;
                }
                OnPropertyChanged(() => Inputs);
            }
        }
        public IList<IServiceOutputMapping> OutputMapping
        {
            get
            {
                return _outputMapping;
            }
            set
            {
                _outputMapping = value;
                IsOutputMappingEmptyRows = true;
                if (_outputMapping != null && _outputMapping.Count >= 1)
                {
                    IsOutputMappingEmptyRows = false;
                }
                OnPropertyChanged(() => OutputMapping);
            }
        }
        public string RecordsetName
        {
            get
            {
                return _recordsetName;
            }
            set
            {
                if (_recordsetName != value)
                {
                    _recordsetName = value;
                    OnPropertyChanged(() => RecordsetName);
                }
            }
        }
        public bool IsInputsEmptyRows
        {
            get
            {
                return _isInputsEmptyRows;
            }
            set
            {
                _isInputsEmptyRows = value;
                OnPropertyChanged(() => IsInputsEmptyRows);
            }
        }
        public bool IsOutputMappingEmptyRows
        {
            get
            {
                return _isOutputMappingEmptyRows;
            }
            set
            {
                _isOutputMappingEmptyRows = value;
                OnPropertyChanged(() => IsOutputMappingEmptyRows);
            }
        }
        /// <summary>
        /// Output Column BName Header
        /// </summary>
        public string OutputName
        {
            get
            {
                return _outputName;
            }
            set
            {
                _outputName = value;
                OnPropertyChanged(() => OutputName);
            }
        }
        /// <summary>
        /// Output Column alias Header
        /// </summary>
        public string OutputAliasHeader { get; set; }
        IWebServiceModel Model
        {
            get
            {
                return _model;
            }
        }
        IRequestServiceNameViewModel RequestServiceNameViewModel
        {
            get
            {
                _requestServiceNameViewModel.Wait();
                if (_requestServiceNameViewModel.Exception == null)
                {
                    return _requestServiceNameViewModel.Result;
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                    throw _requestServiceNameViewModel.Exception;
                }

            }
        }

        #endregion

        #region Implementation of IActiveAware

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        #endregion

        #region Implementation of IDockAware

        public override IWebService ToModel()
        {
            if (Item == null || Item.Id.Equals(Guid.Empty))
            {
                Item = ToService();
                return Item;
            }
            return new WebServiceDefinition
            {
                Inputs = Inputs == null ? new List<IServiceInput>() : MapVariablesToInputs(),
                OutputMappings = OutputMapping,
                Source = SelectedSource,
                Name = Name,
                Path = Path,
                Id = Guid.NewGuid(),
                PostData = DataListUtil.RemoveLanguageBrackets(RequestBody),
                Headers = Headers.Select(value => new NameValue { Name = DataListUtil.RemoveLanguageBrackets(value.Name), Value = DataListUtil.RemoveLanguageBrackets(value.Value) }).ToList(),
                QueryString = RequestUrlQuery,
                SourceUrl = SourceUrl,
                RequestUrl = RequestUrlQuery,
                Response =Response 
            };
        }

        IWebService ToService()
        {
            if (_webService == null)
                return new WebServiceDefinition
                {
                    Inputs = Inputs == null ? new List<IServiceInput>() : MapVariablesToInputs(),
                    OutputMappings = OutputMapping,
                    Source = SelectedSource,
                    Name = Name,
                    Path = Path,
                    Id = Guid.NewGuid(),
                    PostData = DataListUtil.RemoveLanguageBrackets(RequestBody),
                    Headers = Headers.Select(value => new NameValue { Name = DataListUtil.RemoveLanguageBrackets(value.Name), Value = DataListUtil.RemoveLanguageBrackets(value.Value) }).ToList(),
                    QueryString = RequestUrlQuery,
                    SourceUrl = SourceUrl,
                    RequestUrl = RequestUrlQuery,
                    Response = Response
                };
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                _webService.Inputs = Inputs == null ? new List<IServiceInput>() : MapVariablesToInputs();
                _webService.OutputMappings = OutputMapping;
                _webService.Source = SelectedSource;
                _webService.Name = Name;
                _webService.Path = Path;
                _webService.Id = Guid.NewGuid();
                _webService.PostData = DataListUtil.RemoveLanguageBrackets(RequestBody);
                _webService.Headers = Headers.Select(value => new NameValue { Name = DataListUtil.RemoveLanguageBrackets(value.Name), Value = DataListUtil.RemoveLanguageBrackets(value.Value) }).ToList();
                _webService.QueryString = RequestUrlQuery;
                _webService.SourceUrl = SourceUrl;
                _webService.RequestUrl = RequestUrlQuery;
                _webService.Response = Response;
                return _webService;
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void Dispose()
        {
        }

        #endregion
    }
}
