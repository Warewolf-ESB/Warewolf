using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Interfaces;
using Dropbox.Api;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOAuthSourceViewModel : SourceBaseImpl<IOAuthSource>, IManageOAuthSourceViewModel
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string AccessToken { get; private set; }
        private string AuthUri { get; set; }
        public bool HasAuthenticated { get; private set; }
        public IContextualResourceModel Resource { get; set; }


        // ReSharper restore UnusedAutoPropertyAccessor.Local
        public DropboxClient Client { get; set; }
        //private string AppKey = GlobalConstants.DropBoxApiKey;       

        private string _oauth2State;
        private string _name;
        private string _appKey;
        private string _selectedOAuthProvider;
        private List<string> _types;
        private const string RedirectUri = "https://www.example.com/";

        public ManageOAuthSourceViewModel(Task<IRequestServiceNameViewModel> requestServiceNameViewModel)
            : base("OAuth")
        {
            Types = new List<string>
            {
                "Dropbox"
            };
//            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "network", network }, { "dropboxHelper", dropboxHelper }, { "dropboxFactory", dropboxFactory } });
//            _network = network;
//            DropBoxHelper = dropboxHelper;
            CookieHelper.Clear();
//            if (shouldAuthorise)
//                Authorise();
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

        private async Task LoadBrowserUri(string uri)
        {
            AuthUri = uri;
            //var hasConnection = await _network.HasConnectionAsync(uri);
            //if (hasConnection)
            {

//                DropBoxHelper.WebBrowser.Navigated += (sender, args) => GetAuthTokens(args);
//                DropBoxHelper.WebBrowser.LoadCompleted += (sender, args) => Execute.OnUIThread(() =>
//                {
//                    DropBoxHelper.CircularProgressBar.Visibility = Visibility.Hidden;
//                    DropBoxHelper.WebBrowser.Visibility = Visibility.Visible;
//                });
//
//                DropBoxHelper.Navigate(AuthUri);
            }
        }

        private async void Authorise()
        {

            _oauth2State = Guid.NewGuid().ToString("N");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, AppKey, new Uri(RedirectUri), _oauth2State);
            await LoadBrowserUri(authorizeUri.ToString());
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
        }

        #endregion
    }

    public interface IManageOAuthSourceViewModel
    {
    }
}
