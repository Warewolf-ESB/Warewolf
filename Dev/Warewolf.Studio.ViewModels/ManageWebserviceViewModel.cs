using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Warewolf.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebServiceViewModel : SourceBaseImpl<IWebService>, IManageWebServiceViewModel
    {
        WebRequestMethod _selectedWebRequestMethod;
        ICollection<WebRequestMethod> _webRequestMethods;
        ICommand _newWebSourceCommand;
        ICollection<IWebServiceSource> _sources;
        IWebServiceSource _selectedSource;
        IWebService _webService;
        ICollection<INameValue> _headers;
        string _selectHeadersHeader;
        string _requestUrlQuery;
        string _sourceUrl;
        string _requestUrlHeader;
        string _requestBody;
        ICollection<INameValue> _variables;
        string _response;
        ICommand _pastResponseCommand;
        ICommand _testCommand;
        ICommand _saveCommand;
        ICommand _cancelCommand;
        bool _hasChanged;
        ICollection<IWebserviceOutputs> _outputs;
        ICollection<IWebserviceInputs> _inputs;
        string _outputName;
        bool _isActive;
        string _header;
        ResourceType? _image;
        string _resourceName;

        #region Implementation of IManageWebServiceViewModel

        public ManageWebServiceViewModel(ResourceType? image)
            : base(image)
        {
            WebService = new WebService();
            Header = "Bob";
            


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
                OnPropertyChanged(() => SelectedWebRequestMethod);
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
        /// Available Sources
        /// </summary>
        public ICollection<IWebServiceSource> Sources
        {
            get
            {
                return _sources;
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
        public string SelectSourceHeader { get; set; }
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
            get
            {
                return _selectHeadersHeader;
            }
            set
            {
                _selectHeadersHeader = value;
                OnPropertyChanged(() => SelectHeadersHeader);
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
            get
            {
                return _requestUrlHeader;
            }
            set
            {
                _requestUrlHeader = value;
                OnPropertyChanged(() => RequestUrlHeader);
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
                OnPropertyChanged(() => RequestBody);
            }
        }
        /// <summary>
        /// Request Body Header
        /// </summary>
        public string RequestBodyHeader { get; set; }
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
        public bool HasChanged
        {
            get
            {
                return _hasChanged;
            }
            set
            {
                _hasChanged = value;
                OnPropertyChanged(() => HasChanged);
            }
        }
        /// <summary>
        /// List OfInputs
        /// </summary>
        public ICollection<IWebserviceOutputs> Outputs
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
        public ICollection<IWebserviceInputs> Inputs
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

        #endregion

        #region Implementation of IActiveAware

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the object is active; otherwise <see langword="false"/>.
        /// </value>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged(() => Inputs);
            }
        }


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
}
