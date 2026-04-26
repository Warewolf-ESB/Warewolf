#pragma warning disable
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dropbox.Api;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.Commands;
#else
using Prism.Commands;
#endif
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Interfaces;
using Warewolf.Studio.Core;
using Dev2.Common;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOAuthSourceViewModel : SourceBaseImpl<IOAuthSource>, IManageOAuthSourceViewModel
    {
        readonly IManageOAuthSourceModel _updateManager;

        string _oauth2State;
        string _name;
        string _appKey;
        string _selectedOAuthProvider;
        List<string> _types;
        IOAuthSource _oAuthSource;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _testMessage;
        Uri _authUri;
        IWebBrowser _webBrowser;
        readonly string _redirectUri = Resources.Languages.Core.OAuthSourceRedirectUri;
        string _path;
        string _accessToken;
        string _refreshToken;
        string _codeVerifier;

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel)
            : base("OAuth")
        {
            _updateManager = updateManager ?? throw new ArgumentNullException(nameof(updateManager));
            RequestServiceNameViewModel = requestServiceNameViewModel ?? throw new ArgumentNullException(nameof(requestServiceNameViewModel));
            Header = Resources.Languages.Core.OAuthSourceNewHeaderLabel;
            Types = new List<string>
            {
                "Dropbox"
            };
            SelectedOAuthProvider = Types[0];
            CookieHelper.InternetSetOption(IntPtr.Zero, CookieHelper.InternetOptionEndBrowserSession, IntPtr.Zero, 0);
            HasAuthenticated = false;
            SetupCommands();
        }

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, IOAuthSource oAuthSource, IAsyncWorker asyncWorker)
            : base("OAuth")
        {
            if (oAuthSource == null)
            {
                throw new ArgumentNullException(nameof(oAuthSource));
            }
            _updateManager = updateManager ?? throw new ArgumentNullException(nameof(updateManager));
            Types = new List<string>
            {
                "Dropbox"
            };

            asyncWorker.Start(() => updateManager.FetchSource(oAuthSource.ResourceID), source =>
            {
                _oAuthSource = source;
                _oAuthSource.ResourcePath = oAuthSource.ResourcePath;

                FromModel(_oAuthSource);
                SetupHeaderTextFromExisting();
                SetupCommands();
                SetupAuthorizeUri();
            });
        }

        void SetupCommands()
        {
            OkCommand = new DelegateCommand(Save, CanSave);
            TestCommand = new DelegateCommand(() =>
            {
                SetupAuthorizeUri();
                if (WebBrowser != null && AuthUri != null)
                {
                    Testing = true;
                    TestPassed = false;
                    TestFailed = false;
                    WebBrowser.Navigate(AuthUri);
                }
            }, CanTest);
        }

        public override bool CanSave() => TestPassed && !string.IsNullOrEmpty(AccessToken);

        bool CanTest() => SelectedOAuthProvider != null && !string.IsNullOrWhiteSpace(AppKey);

        void SetupAuthorizeUri()
        {
            _oauth2State = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(AppKey))
            {
                _codeVerifier = DropboxOAuth2Helper.GeneratePKCECodeVerifier();
                var codeChallenge = DropboxOAuth2Helper.GeneratePKCECodeChallenge(_codeVerifier);
                var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(
                    oauthResponseType: OAuthResponseType.Code,
                    clientId: AppKey,
                    redirectUri: new Uri(_redirectUri),
                    state: _oauth2State,
                    codeChallenge: codeChallenge,
                    tokenAccessType: TokenAccessType.Offline,
                    scopeList: new[] { "account_info.read", "files.metadata.read", "files.content.read", "files.content.write" });
                Dev2Logger.Debug("[OAuthSource] AuthorizeUri = " + authorizeUri, "Warewolf Debug");
                AuthUri = authorizeUri;
            }
        }

        public void GetAuthTokens(Uri uri)
        {
            if (uri == null)
            {
                return;
            }

            var uriString = uri.ToString();
            if (!uriString.StartsWith(_redirectUri, StringComparison.OrdinalIgnoreCase))
            {
                // Ignore all navigation that isn't to the redirect URI.
                TestMessage = "Waiting for user details...";
                return;
            }

            Testing = false;

            if (uriString.Equals(_redirectUri, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // PKCE Authorization Code flow — Dropbox appends ?code=...&state=... to the redirect URI.
            var query = HttpUtility.ParseQueryString(uri.Query);
            var code = query.Get("code");
            var returnedState = query.Get("state");

            if (!string.IsNullOrEmpty(code))
            {
                if (returnedState != _oauth2State)
                {
                    TestPassed = false;
                    TestFailed = true;
                    TestMessage = "Authentication failed: state mismatch";
                    AccessToken = string.Empty;
                    RefreshToken = string.Empty;
                    HasAuthenticated = false;
                    return;
                }
                ExchangeCodeForToken(code);
            }
            else
            {
                var errorDescription = query.Get("error_description") ?? query.Get("error") ?? "Authentication failed";
                TestPassed = false;
                TestFailed = true;
                TestMessage = errorDescription;
                AccessToken = string.Empty;
                RefreshToken = string.Empty;
                HasAuthenticated = false;
            }
        }

        async void ExchangeCodeForToken(string code)
        {
            Dev2Logger.Debug("[OAuthSource] ExchangeCodeForToken called, code length = " + code?.Length, "Warewolf Debug");
            try
            {
                var result = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                    code: code,
                    appKey: AppKey,
                    appSecret: null,
                    redirectUri: _redirectUri,
                    client: new HttpClient(),
                    codeVerifier: _codeVerifier);

                var tokenPreview = result.AccessToken?.Length > 8 ? result.AccessToken.Substring(0, 8) + "..." : "(empty)";
                Dev2Logger.Debug($"[OAuthSource] Token exchange OK. AccessToken prefix={tokenPreview} len={result.AccessToken?.Length} RefreshToken={(result.RefreshToken != null ? "present" : "null")} ExpiresAt={result.ExpiresAt}", "Warewolf Debug");

                TestPassed = true;
                TestFailed = false;
                TestMessage = $"Authorised (token prefix: {tokenPreview}, refresh: {(result.RefreshToken != null ? "yes" : "no")})";
                AccessToken = result.AccessToken;
                RefreshToken = result.RefreshToken;
                HasAuthenticated = true;
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn("Dropbox PKCE token exchange failed: " + ex.Message, "Warewolf Warn");
                Dev2Logger.Warn("Dropbox PKCE token exchange failed (inner): " + ex.InnerException?.Message, "Warewolf Warn");
                TestPassed = false;
                TestFailed = true;
                TestMessage = "Authentication failed: " + (ex.InnerException?.Message ?? ex.Message);
                AccessToken = string.Empty;
                RefreshToken = string.Empty;
                HasAuthenticated = false;
            }
        }

        public List<string> Types
        {
            get => _types;
            set => _types = value;
        }

        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }

        public string AccessToken
        {
            get => _accessToken;
            set
            {
                _accessToken = value;
                OnPropertyChanged(() => AccessToken);
            }
        }

        public string RefreshToken
        {
            get => _refreshToken;
            set
            {
                _refreshToken = value;
                OnPropertyChanged(() => RefreshToken);
            }
        }

        public Uri AuthUri
        {
            get => _authUri;
            set
            {
                _authUri = value;
                OnPropertyChanged(() => AuthUri);
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }

        public bool HasAuthenticated { get; private set; }

        public bool TestPassed
        {
            get => _testPassed;
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
            }
        }

        public bool TestFailed
        {
            get => _testFailed;
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }

        public bool Testing
        {
            get => _testing;
            set
            {
                _testing = value;
                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            }
        }

        public string TestMessage
        {
            get => _testMessage;
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
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken
                };
            }
            return null;
        }

        public string SelectedOAuthProvider
        {
            get => _selectedOAuthProvider;
            set
            {
                _selectedOAuthProvider = value;
                OnPropertyChanged(() => SelectedOAuthProvider);
            }
        }

        public string AppKey
        {
            get => _appKey;
            set
            {
                _appKey = value;
                OnPropertyChanged(() => AppKey);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            }
        }

        #region Overrides of SourceBaseImpl<IOAuthSource>

        public override string Name
        {
            get => _name;
            set => _name = value;
        }

        public override void FromModel(IOAuthSource source)
        {
            ResourceName = source.ResourceName;
            SelectedOAuthProvider = Types[0];
            AppKey = source.AppKey;
            AccessToken = source.AccessToken;
            RefreshToken = source.RefreshToken;
            Path = source.ResourcePath;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void Save()
        {
            SaveConnection();
        }

        public string ResourceName { get; set; }

        #endregion Overrides of SourceBaseImpl<IOAuthSource>

        public ICommand TestCommand { get; set; }

        public ICommand OkCommand { get; set; }

        public ICommand Navigated { get; set; }

        public IWebBrowser WebBrowser
        {
            get => _webBrowser;
            set
            {
                _webBrowser = value;
                if (_webBrowser != null)
                {
                    _webBrowser.Navigated += GetAuthTokens;
                }
            }
        }

        void SaveConnection()
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
                        AfterSave(requestServiceNameViewModel, src);

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

        void AfterSave(IRequestServiceNameViewModel requestServiceNameViewModel, IOAuthSource src)
        {
            if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
            {
                AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.ResourceID);
            }
        }

        void Save(IOAuthSource source)
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

        void SetupHeaderTextFromExisting()
        {
            if (_oAuthSource != null)
            {
                Header = (_oAuthSource.ResourceName ?? ResourceName).Trim();
            }
        }

        IOAuthSource ToSource()
        {
            if (_oAuthSource == null)
            {
                return new DropBoxSource
                {
                    AppKey = AppKey,
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    ResourceID = Guid.NewGuid()
                };
            }
            else
            {
                _oAuthSource.AppKey = AppKey;
                _oAuthSource.AccessToken = AccessToken;
                _oAuthSource.RefreshToken = RefreshToken;
                return _oAuthSource;
            }
        }
    }

    public interface IManageOAuthSourceViewModel
    {
    }
}
