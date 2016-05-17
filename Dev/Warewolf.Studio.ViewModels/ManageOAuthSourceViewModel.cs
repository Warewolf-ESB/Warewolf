using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Dropbox.Api;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOAuthSourceViewModel : SourceBaseImpl<IOAuthSource>, IManageOAuthSourceViewModel
    {
        //private string AppKey = GlobalConstants.DropBoxApiKey;
        private readonly IManageOAuthSourceModel _updateManager;

        private string _oauth2State;
        private string _name;
        private string _appKey;
        private string _selectedOAuthProvider;
        private List<string> _types;
        private IOAuthSource _oAuthSource;
        private string _resourceName;
        private bool _testPassed;
        private bool _testFailed;
        private bool _testing;
        private string _testMessage;
        private Uri _authUri;
        private IWebBrowser _webBrowser;
        private const string RedirectUri = "https://www.dropbox.com/1/oauth2/redirect_receiver/";
        private string _path;
        private string _accessToken;

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel)
            : base("OAuth")
        {
            if (updateManager == null)
            {
                throw new ArgumentNullException("updateManager");
            }
            if (requestServiceNameViewModel == null)
            {
                throw new ArgumentNullException("requestServiceNameViewModel");
            }
            _updateManager = updateManager;
            RequestServiceNameViewModel = requestServiceNameViewModel;
            Header = Resources.Languages.Core.OAuthSourceNewHeaderLabel;
            Types = new List<string>
            {
                "Dropbox"
            };
            SelectedOAuthProvider = Types[0];
            //            _network = network;
            //            DropBoxHelper = dropboxHelper;
            CookieHelper.Clear();
            //            if (shouldAuthorise)
            //                Authorise();
            Testing = false;
            HasAuthenticated = false;
            SetupCommands();
            //AppKey = "31qf750f1vzffhu";
            
        }

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, IOAuthSource oAuthSource)
            : base("OAuth")
        {
            if (oAuthSource == null)
            {
                throw new ArgumentNullException("oAuthSource");
            }
            _oAuthSource = oAuthSource;
            _updateManager = updateManager;
            Types = new List<string>
            {
                "Dropbox"
            };
            // ReSharper disable once VirtualMemberCallInContructor
            FromModel(oAuthSource);
            SetupHeaderTextFromExisting();
            SetupCommands();
            SetupAuthorizeUri();
        }

        private void SetupCommands()
        {
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            TestCommand = new DelegateCommand(() =>
            {
                Testing = true;
                TestPassed = false;
                SetupAuthorizeUri();
                WebBrowser.Navigate(AuthUri);
            }, CanTest);
        }

        private bool CanTest()
        {
            return SelectedOAuthProvider != null && !string.IsNullOrWhiteSpace(AppKey);
        }

        private void SetupAuthorizeUri()
        {
            _oauth2State = Guid.NewGuid().ToString("N");
            if(AppKey != null)
            {
                var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, AppKey, new Uri(RedirectUri), _oauth2State);
                AuthUri = authorizeUri;
            }
        }

        private void GetAuthTokens(Uri uri)
        {
            if (uri != null)
            {
                if (!uri.ToString().StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
                {
                    // we need to ignore all navigation that isn't to the redirect uri.
                    TestMessage = "Waiting for user details...";
                    return;
                }
                Testing = false;
                if (!uri.ToString().Equals(RedirectUri, StringComparison.OrdinalIgnoreCase))
                {
                    OAuth2Response result = null;
                    try
                    {
                        result = DropboxOAuth2Helper.ParseTokenFragment(uri);
                    }
                    catch (ArgumentException)
                    {
                    }

                    if (result != null)
                    {
                        if (result.State != _oauth2State)
                        {
                            TestPassed = false;
                            TestFailed = true;
                            TestMessage = "Authentication failed";
                            AccessToken = string.Empty;
                            HasAuthenticated = false;
                        }
                        else
                        {
                            TestPassed = true;
                            TestFailed = false;
                            TestMessage = "";
                            AccessToken = result.AccessToken;
                            HasAuthenticated = true;
                        }
                    }
                    else
                    {
                        string errorDescription = HttpUtility.ParseQueryString(uri.ToString()).Get("error_description");

                        TestMessage = errorDescription ?? "Authentication failed";
                    }
                }
            }
        }

        // ReSharper disable once ConvertToAutoProperty
        public List<string> Types
        {
            get
            {
                return _types;
            }
            set
            {
                _types = value;
            }
        }

        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }

        public string AccessToken
        {
            get
            {
                return _accessToken;
            }
            private set
            {
                _accessToken = value;
                OnPropertyChanged(() => AccessToken);
            }
        }

        public Uri AuthUri
        {
            get
            {
                return _authUri;
            }
            set
            {
                _authUri = value;
                OnPropertyChanged(() => AuthUri);
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

        private void SaveConnection()
        {
            Testing = true;
            TestFailed = false;
            TestPassed = false;
            if (_oAuthSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var res = RequestServiceNameViewModel.Result.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        _resourceName = RequestServiceNameViewModel.Result.ResourceName.Name;
                        var src = ToSource();

                        src.ResourcePath = RequestServiceNameViewModel.Result.ResourceName.Path ?? RequestServiceNameViewModel.Result.ResourceName.Name;
                        Save(src);
                        _oAuthSource = src;
                        Path = _oAuthSource.ResourcePath;
                        SetupHeaderTextFromExisting();
                    }
                }
                else
                {
                    throw RequestServiceNameViewModel.Exception;
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                _oAuthSource = src;
            }
        }

        public bool HasAuthenticated { get; private set; }

        public IContextualResourceModel Resource { get; set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
        public DropboxClient Client { get; set; }

        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
            }
        }

        public bool TestFailed
        {
            get
            {
                return _testFailed;
            }
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }

        public bool Testing
        {
            get
            {
                return _testing;
            }
            set
            {
                _testing = value;

                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            }
        }

        public string TestMessage
        {
            get { return _testMessage; }
            private set
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        #region Overrides of SourceBaseImpl<IOAuthSource>

        public override IOAuthSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            if (SelectedOAuthProvider == "Dropbox")
            {
                return new DropBoxSource
                {
                    AppKey = AppKey,
                    AccessToken = AccessToken
                };
            }
            return null;
        }

        public string SelectedOAuthProvider
        {
            get
            {
                return _selectedOAuthProvider;
            }
            set
            {
                _selectedOAuthProvider = value;
                OnPropertyChanged(() => SelectedOAuthProvider);
            }
        }

        public string AppKey
        {
            get
            {
                return _appKey;
            }
            set
            {
                _appKey = value;
                OnPropertyChanged(() => AppKey);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            }
        }

        // ReSharper disable once ConvertToAutoProperty
        public override string Name
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

        public override void FromModel(IOAuthSource source)
        {
            ResourceName = source.ResourceName;
            SelectedOAuthProvider = Types[0];
            AppKey = source.AppKey;
            AccessToken = source.AccessToken;
            Path = source.ResourcePath;
        }

        public override bool CanSave()
        {
            return TestPassed && !String.IsNullOrEmpty(AccessToken);
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        public override void Save()
        {
            SaveOAuthSource();
        }

        private void SetupHeaderTextFromExisting()
        {
            if (_oAuthSource != null)
            {
                Header = (_oAuthSource.ResourceName ?? ResourceName).Trim();
            }
        }

        private void SaveOAuthSource()
        {
            if (_oAuthSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var res = RequestServiceNameViewModel.Result.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        var src = ToSource();
                        src.ResourceName = RequestServiceNameViewModel.Result.ResourceName.Name;
                        src.ResourcePath = RequestServiceNameViewModel.Result.ResourceName.Path ?? RequestServiceNameViewModel.Result.ResourceName.Name;
                        Save(src);
                        Item = src;
                        _oAuthSource = src;
                        ResourceName = _oAuthSource.ResourceName;
                        SetupHeaderTextFromExisting();
                    }
                }
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
            }
        }

        public ICommand Navigated
        {
            get;
            set;
        }

        public IWebBrowser WebBrowser
        {
            get
            {
                return _webBrowser;
            }
            set
            {
                _webBrowser = value;
                if (_webBrowser != null)
                {
                    _webBrowser.Navigated += GetAuthTokens;
                }
            }
        }

        public ICommand TestCommand
        {
            get;
            set;
        }

        public ICommand OkCommand { get; set; }

        #endregion Overrides of SourceBaseImpl<IOAuthSource>

        private void Save(IOAuthSource source)
        {
            try
            {
                _updateManager.Save(source);
                Item = ToSource();
                SetupHeaderTextFromExisting();
            }
            catch (Exception ex)
            {
                TestMessage = ex.Message;
                TestFailed = true;
                TestPassed = false;
            }
        }

        private IOAuthSource ToSource()
        {
            if (_oAuthSource == null)
                return new DropBoxSource
                {
                    AppKey = AppKey,
                    AccessToken = AccessToken,
                    ResourceID = _oAuthSource == null ? Guid.NewGuid() : _oAuthSource.ResourceID
                }
            ;
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                _oAuthSource.AppKey = AppKey;
                _oAuthSource.AccessToken = AccessToken;
                return _oAuthSource;
            }
        }
    }

    public interface IManageOAuthSourceViewModel
    {
    }
}