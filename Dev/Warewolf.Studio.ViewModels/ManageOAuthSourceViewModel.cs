using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Interfaces;
using Dropbox.Api;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOAuthSourceViewModel : SourceBaseImpl<IOAuthSource>, IManageOAuthSourceViewModel
    {
        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string AccessToken { get; private set; }
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
        public bool HasAuthenticated { get; private set; }
        public IContextualResourceModel Resource { get; set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
        public DropboxClient Client { get; set; }
        //private string AppKey = GlobalConstants.DropBoxApiKey;       
        private readonly IManageOAuthSourceModel _updateManager;
        private string _oauth2State;
        private string _name;
        private string _appKey;
        private string _selectedOAuthProvider;
        private List<string> _types;
        private IOAuthSource _oAuthSource;
        private string _resourceName;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        private string _testMessage;
        private Uri _authUri;
        private IWebBrowser _webBrowser;
        private const string RedirectUri = "https://www.example.com/";

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
            SetupCommands();
            AppKey = "31qf750f1vzffhu";
            Authorise();
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
            // ReSharper disable once VirtualMemberCallInContructor
            FromModel(oAuthSource);
            SetupHeaderTextFromExisting();
            SetupCommands();
            Authorise();
        }

        private void SetupCommands()
        {
            TestCommand = new DelegateCommand(() =>
            {
                Testing = true;
                WebBrowser.Navigate(AuthUri);
            });
        }
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
        private void Authorise()
        {
            _oauth2State = Guid.NewGuid().ToString("N");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, AppKey, new Uri(RedirectUri), _oauth2State);
            AuthUri = authorizeUri;
        }
        void GetAuthTokens(Uri uri)
        {
            if (uri != null && !uri.ToString().StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.
                return;
            }
            try
            {
                if (uri != null)
                {
                    OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(uri);
                    if (result.State != _oauth2State)
                    {
                        return;
                    }
                    AccessToken = result.AccessToken;
                }
                HasAuthenticated = true;
            }
            catch (ArgumentException)
            {
            }
        }

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
            // ReSharper disable UnusedMember.Local
            private set
            // ReSharper restore UnusedMember.Local
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        #region Overrides of SourceBaseImpl<IOAuthSource>

        public override IOAuthSource ToModel()
        {
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
            }
        }

        public override void FromModel(IOAuthSource service)
        {
        }

        public override bool CanSave()
        {
            return false;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        public override void Save()
        {
            SaveOAuthSource();
        }

        void SetupHeaderTextFromExisting()
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

        #endregion

        void Save(IOAuthSource source)
        {
            _updateManager.Save(source);
        }
        IOAuthSource ToSource()
        {
            if (_oAuthSource == null)
                return new DropBoxSource
                {
                    AppKey = AppKey,
                    AccessToken = AccessToken,
                    //Id = _oAuthSource == null ? Guid.NewGuid() : _oAuthSource.Id
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
