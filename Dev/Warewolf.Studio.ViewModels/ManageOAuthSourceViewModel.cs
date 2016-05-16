using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
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
                if(WebBrowser != null)
                {
                    WebBrowser.Navigate(_authUri);
                }
                OnPropertyChanged(()=>AuthUri);
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
        private Uri _authUri;
        private const string RedirectUri = "https://www.example.com/";

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel)
            : base("OAuth")
        {
            if(updateManager == null)
            {
                throw new ArgumentNullException("updateManager");
            }
            if(requestServiceNameViewModel == null)
            {
                throw new ArgumentNullException("requestServiceNameViewModel");
            }
            _updateManager = updateManager;
            RequestServiceNameViewModel = requestServiceNameViewModel;
            Types = new List<string>
            {
                "Dropbox"
            };
//            _network = network;
//            DropBoxHelper = dropboxHelper;
            CookieHelper.Clear();
            //            if (shouldAuthorise)
            //                Authorise();
            SetupCommands();
            AppKey = "31qf750f1vzffhu";
            Authorise();
        }

        

        public ManageOAuthSourceViewModel(IManageOAuthSourceModel updateManager, IOAuthSource oAuthSource) : base("OAuth")
        {
            if(oAuthSource == null)
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
            Navigated = new DelegateCommand<NavigationEventArgs>(GetAuthTokens);
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
        void GetAuthTokens(NavigationEventArgs args)
        {

            if (!args.Uri.ToString().StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.
                return;
            }
            try
            {
                OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(args.Uri);
                if (result.State != _oauth2State)
                {
                    return;
                }
                AccessToken = result.AccessToken;
                HasAuthenticated = true;
            }
            catch (ArgumentException)
            {
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
                OnPropertyChanged(()=>SelectedOAuthProvider);
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
                OnPropertyChanged(()=>AppKey);
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
            get;set;
        }
        public IWebBrowser WebBrowser { get; set; }

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
