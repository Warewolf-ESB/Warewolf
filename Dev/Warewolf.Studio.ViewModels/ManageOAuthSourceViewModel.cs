using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dropbox.Api;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Threading;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOAuthSourceViewModel : SourceBaseImpl<IOAuthSource>, IManageOAuthSourceViewModel
    {
        private readonly IManageOAuthSourceModel _updateManager;

        private string _oauth2State;
        private string _name;
        private string _appKey;
        private string _selectedOAuthProvider;
        private List<string> _types;
        private IOAuthSource _oAuthSource;
        private bool _testPassed;
        private bool _testFailed;
        private bool _testing;
        private string _testMessage;
        private Uri _authUri;
        private IWebBrowser _webBrowser;
        private readonly string _redirectUri = Resources.Languages.Core.OAuthSourceRedirectUri;
        private string _path;
        private string _accessToken;

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel)
            : base("OAuth")
        {
            if (updateManager == null)
            {
                throw new ArgumentNullException(nameof(updateManager));
            }
            if (requestServiceNameViewModel == null)
            {
                throw new ArgumentNullException(nameof(requestServiceNameViewModel));
            }
            _updateManager = updateManager;
            RequestServiceNameViewModel = requestServiceNameViewModel;
            Header = Resources.Languages.Core.OAuthSourceNewHeaderLabel;
            Types = new List<string>
            {
                "Dropbox"
            };
            SelectedOAuthProvider = Types[0];
            CookieHelper.Clear();
            HasAuthenticated = false;
            SetupCommands();
        }

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, IOAuthSource oAuthSource,IAsyncWorker asyncWorker)
            : base("OAuth")
        {
            if (updateManager == null)
            {
                throw new ArgumentNullException(nameof(updateManager));
            }
            if (oAuthSource == null)
            {
                throw new ArgumentNullException(nameof(oAuthSource));
            }
            _updateManager = updateManager;
            Types = new List<string>
            {
                "Dropbox"
            };


            asyncWorker.Start(() => updateManager.FetchSource(oAuthSource.ResourceID), source =>
            {
                _oAuthSource = source;
                _oAuthSource.ResourcePath = oAuthSource.ResourcePath;
                // ReSharper disable once VirtualMemberCallInContructor
                FromModel(_oAuthSource);
                SetupHeaderTextFromExisting();
                SetupCommands();
                SetupAuthorizeUri();
            });
        }

        private void SetupCommands()
        {
            OkCommand = new DelegateCommand(Save, CanSave);
            TestCommand = new DelegateCommand(() =>
            {
                SetupAuthorizeUri();
                if (WebBrowser != null &&
                    AuthUri != null)
                {
                    Testing = true;
                    TestPassed = false;
                    TestFailed = false;
                    WebBrowser.Navigate(AuthUri);
                }
            }, CanTest);
        }

        public override bool CanSave()
        {
            return TestPassed && !string.IsNullOrEmpty(AccessToken);
        }

        private bool CanTest()
        {
            return SelectedOAuthProvider != null && !string.IsNullOrWhiteSpace(AppKey);
        }

        private void SetupAuthorizeUri()
        {
            _oauth2State = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(AppKey))
            {
                var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, AppKey, new Uri(_redirectUri), _oauth2State);
                AuthUri = authorizeUri;
            }
        }

        public void GetAuthTokens(Uri uri)
        {
            if (uri != null)
            {
                if (!uri.ToString().StartsWith(_redirectUri, StringComparison.OrdinalIgnoreCase))
                {
                    // we need to ignore all navigation that isn't to the redirect uri.
                    TestMessage = "Waiting for user details...";
                    return;
                }
                Testing = false;
                if (!uri.ToString().Equals(_redirectUri, StringComparison.OrdinalIgnoreCase))
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
                        TestPassed = false;
                        TestFailed = true;
                        TestMessage = "Authentication failed";
                        AccessToken = string.Empty;
                        HasAuthenticated = false;

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
            set
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

        public bool HasAuthenticated { get; private set; }

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
            set
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

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

        #region Overrides of SourceBaseImpl<IOAuthSource>

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

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void Save()
        {
            SaveConnection();
        }

        public string ResourceName { get; set; }

        #endregion Overrides of SourceBaseImpl<IOAuthSource>

        public ICommand TestCommand
        {
            get;
            set;
        }

        public ICommand OkCommand { get; set; }

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

        private void SaveConnection()
        {
            if (_oAuthSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var requestServiceNameViewModel = RequestServiceNameViewModel.Result;
                    var res = requestServiceNameViewModel.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        var src = ToSource();
                        src.ResourceName = requestServiceNameViewModel.ResourceName.Name;
                        src.ResourcePath = requestServiceNameViewModel.ResourceName.Path ?? requestServiceNameViewModel.ResourceName.Name;
                        Save(src);
                        if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
                            AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.ResourceID);
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
                src.ResourcePath = Item.ResourcePath ?? "";
                src.ResourceName = Item.ResourceName;
                Save(src);
                Item = src;
                _oAuthSource = src;
                SetupHeaderTextFromExisting();
            }
        }

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
            }
        }

        private void SetupHeaderTextFromExisting()
        {
            if (_oAuthSource != null)
            {
                Header = (_oAuthSource.ResourceName ?? ResourceName).Trim();
            }
        }

        private IOAuthSource ToSource()
        {
            if (_oAuthSource == null)
                return new DropBoxSource
                {
                    AppKey = AppKey,
                    AccessToken = AccessToken,
                    ResourceID = _oAuthSource?.ResourceID ?? Guid.NewGuid()
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