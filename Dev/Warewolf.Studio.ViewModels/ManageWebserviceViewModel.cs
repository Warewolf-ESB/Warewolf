using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Dev2.Controller;
using Dev2.Network;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebServiceViewModel : SourceBaseImpl<IWebService>, IManageWebServiceViewModel
    {
        readonly IWebServiceModel _model;
        readonly IRequestServiceNameViewModel _saveDialog;
        WebRequestMethod _selectedWebRequestMethod;
        ICollection<WebRequestMethod> _webRequestMethods;
        ICommand _newWebSourceCommand;
    
        ICollection<IWebServiceSource> _sources;
        IWebServiceSource _selectedSource;
        IWebService _webService;
        ICollection<INameValue> _headers;
        string _requestUrlQuery;
        string _sourceUrl;
        string _requestBody;
        ICollection<INameValue> _variables;
        string _response;
        ICommand _pastResponseCommand;
        ICommand _testCommand;
        ICommand _saveCommand;
        ICommand _cancelCommand;
        bool _hasChanged;
        ICollection<IServiceOutputMapping> _outputs;
        ICollection<IServiceInput> _inputs;
        string _outputName;
        bool _isActive;
        string _header;
        ResourceType? _image;
        string _resourceName;
        bool _requestBodyEnabled;
        ICommand _editWebSourceCommand;
        IList<IServiceOutputMapping> _outputMapping;

        #region Implementation of IManageWebServiceViewModel

        public ManageWebServiceViewModel(ResourceType? image, IWebServiceModel model, IRequestServiceNameViewModel saveDialog)
            : base(image)
        {
            _model = model;
            _saveDialog = saveDialog;
            TestCommand = new DelegateCommand(() => model.TestService(ToModel()), CanTest);
            SaveCommand = new DelegateCommand(() => model.SaveService(ToModel()), CanSave);
            NewWebSourceCommand = new DelegateCommand(model.CreateNewSource);
            
            Init();
        }

        bool CanSave()
        {
            return false;
        }

        public ManageWebServiceViewModel(ResourceType webService)
            : base(webService)
        {
            var commController = new  CommunicationControllerFactory();
            var connection = new ServerProxy( new Uri( "http://localhost:3142"));
            connection.Connect(Guid.NewGuid());
            IStudioUpdateManager updateRepository = new StudioResourceUpdateManager(commController,connection); 
            IQueryManager queryProxy = new QueryManagerProxy(commController,connection);
            
            _model = new WebServiceModel(updateRepository,queryProxy,"bob");
            TestCommand = new DelegateCommand(() =>
            {
                var output = _model.TestService(ToModel());
                Response = output;
            }, CanTest);
            SaveCommand = new DelegateCommand(() => _model.SaveService(ToModel()), CanSave);
            NewWebSourceCommand = new DelegateCommand(_model.CreateNewSource);
            Init();
           
        }

        void Init()
        {
            WebService = new WebServiceDefinition();
            Header = Resources.Languages.Core.WebserviceTabHeader;
            WebRequestMethods = new ObservableCollection<WebRequestMethod>(Dev2EnumConverter.GetEnumsToList<WebRequestMethod>());
            SelectedWebRequestMethod = WebRequestMethods.First();
            Sources = new ObservableCollection<IWebServiceSource>(_model.RetrieveSources());
            Inputs = new ObservableCollection<IServiceInput>{new ServiceInput("a","a"),new ServiceInput("b","b")};
            Outputs = new ObservableCollection<IServiceOutputMapping> { new DbOutputMapping("bob", "builder"), new DbOutputMapping("dora", "explorer") };
            EditWebSourceCommand = new DelegateCommand(() => _model.EditSource(SelectedSource), () => SelectedSource != null);
            var headerCollection = new ObservableCollection<INameValue>();
            headerCollection.CollectionChanged += HeaderCollectionOnCollectionChanged;
            Headers = new ObservableCollection<INameValue>(new List<INameValue>{new NameValue()});
            Variables =  new ObservableCollection<INameValue>(new List<INameValue>{new NameValue()});
        }

        bool CanTest()
        {
            return SelectedSource!=null;
        }

        void HeaderCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            foreach(var nameValue in Headers)
            {
                UpdateRequestVariables(nameValue.Value);
                UpdateRequestVariables(nameValue.Name);
            }
        }

        public void UpdateRequestVariables(string name)
        {
            var exp = WarewolfDataEvaluationCommon.ParseLanguageExpression(name);
            if(Variables.Any(a=>a.Name == name))
            {
                return;
            }
            if(exp.IsScalarExpression)
            {
                var scalar = exp as LanguageAST.LanguageExpression.ScalarExpression;
                Variables.Add( new NameValue(){Name = WarewolfDataEvaluationCommon.LanguageExpressionToString(scalar), Value = ""  });
            }
            if (exp.IsRecordSetExpression)
            {
                var rec = exp as LanguageAST.LanguageExpression.RecordSetExpression;
                Variables.Add(new NameValue() { Name = WarewolfDataEvaluationCommon.LanguageExpressionToString(rec), Value = "" });
            }
            if (exp.IsComplexExpression)
            {
                var rec = exp as LanguageAST.LanguageExpression.ComplexExpression;
                foreach(var languageExpression in rec.Item)
                {
                    UpdateRequestVariables(WarewolfDataEvaluationCommon.LanguageExpressionToString(languageExpression));
                }
               
            }

            Variables.Add( new NameValue{Name="BOB",Value = "Builder"} );
            Variables.Add(  new NameValue{Name = "var",Value = "100"});
           
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
                if (value == WebRequestMethod.Get)
                {
                    RequestBodyEnabled = false;
                }
                else
                {
                    RequestBodyEnabled = true;
                }
                OnPropertyChanged(() => SelectedWebRequestMethod);
            }
        }
        public bool RequestBodyEnabled
        {
            get
            {
                return _requestBodyEnabled;
            }
            set
            {
                _requestBodyEnabled = value;
                OnPropertyChanged(() => RequestBodyEnabled);
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
                _newWebSourceCommand = value;
                OnPropertyChanged(() => NewWebSourceCommand);
            }
        }
        /// <summary>
        /// Command to create a new web source 
        /// </summary>
        public ICommand EditWebSourceCommand
        {
            get
            {
                return _editWebSourceCommand;
            }
            set
            {
                _editWebSourceCommand = value;
                OnPropertyChanged(() => EditWebSourceCommand);
            }
        }
        /// <summary>
        /// Available Sources
        /// </summary>
        public ICollection<IWebServiceSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                OnPropertyChanged(()=> Sources);
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
                _selectedSource = value;
                if(_selectedSource != null)
                {
                    RequestUrlQuery = _selectedSource.DefaultQuery??"";
                    SourceUrl = _selectedSource.HostName;
                    ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                }
                OnPropertyChanged(() => SelectedSource);
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
                OnPropertyChanged(() => WebService);
            }
        }

        /// <summary>
        /// Label for selecteing a header
        /// </summary>
        public string SelectSourceHeader
        {
            get { return Resources.Languages.Core.WebserviceHeader; }
        }
        /// <summary>
        /// Request headers
        /// </summary>
        public ICollection<INameValue> Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
                OnPropertyChanged(() => Headers);
            }
        }
        /// <summary>
        /// Select the headers
        /// </summary>
        public string SelectHeadersHeader
        {
            get { return Resources.Languages.Core.WebserviceHeadersHeader; }
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
        ///  The form Header
        /// </summary>
        public string RequestUrlHeader
        {
            get { return Resources.Languages.Core.WebserviceRequestURLHeader; }
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
        /// Request Body Header
        /// </summary>
        public string RequestBodyHeader
        {
            get { return Resources.Languages.Core.WebserviceRequestBodyHeader; }
        }
        /// <summary>
        /// Request Header
        /// </summary>
        [ExcludeFromCodeCoverage]
        public string RequestHeader
        {
            get { return Resources.Languages.Core.WebserviceRequestHeader; }

        }
        /// <summary>
        /// Variables Header
        /// </summary>
        [ExcludeFromCodeCoverage]
        public string VariablesHeader
        {
            get { return Resources.Languages.Core.WebserviceVariablesHeader; }
            
        }
        /// <summary>
        /// Variables Header
        /// </summary>
        [ExcludeFromCodeCoverage]
        public string MappingsHeader
        {
            get { return Resources.Languages.Core.DefaultMappings; }
        }
        /// <summary>
        /// the warewolf variables defined in the body,headers and query string
        /// </summary>
        public ICollection<INameValue> Variables
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
        /// Response Header
        /// </summary>
        [ExcludeFromCodeCoverage]
        public string ResponseHeader
        {
            get { return Resources.Languages.Core.WebserviceResponseHeader; }
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
        /// Has the Source changed
        /// </summary>

        /// <summary>
        /// List OfInputs
        /// </summary>
        public ICollection<IServiceOutputMapping> Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {
                _outputs = value;
                OnPropertyChanged(() => Outputs);
            }
        }
        /// <summary>
        /// List Of Outputs
        /// </summary>
        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                _inputs = value;
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
                OnPropertyChanged(() => OutputMapping);
            }
        }
        /// <summary>
        /// Input region ColumnHeader
        /// </summary>
        public string InputsHeader { get; set; }
        /// <summary>
        ///  input column Header
        /// </summary>
        public string InputNameHeader { get; set; }
        /// <summary>
        /// Default value column Header
        /// </summary>
        public string DefaultValueHeader { get; set; }
        /// <summary>
        /// IsRequiredFieldColumnHeader
        /// </summary>
        public string RequiredFieldHeader { get; set; }
        /// <summary>
        /// EmptyIsNullColumnHeader
        /// </summary>
        public string EmptyIsNullHeader { get; set; }
        /// <summary>
        /// Outputs RegionHeader
        /// </summary>
        public string OutputsHeader { get; set; }
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
            }
        }
        /// <summary>
        /// Output Column alias Header
        /// </summary>
        public string OutputAliasHeader { get; set; }
        public IRequestServiceNameViewModel SaveDialog
        {
            get
            {
                return _saveDialog;
            }
        }

        #endregion

        #region Implementation of IActiveAware




        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        #region Implementation of IDockAware

        public override IWebService ToModel()
        {
            return WebService;
        }



        #endregion

  

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }

    public class NameValue : INameValue
    {
        string _name;
        string _value;

        #region Implementation of INameValue

        public  NameValue()
        {
            Name = "Bob";
            Value = "Bob";
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        #endregion
    }
}
