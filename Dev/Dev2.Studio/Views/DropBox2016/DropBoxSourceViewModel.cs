using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces.Dropbox;
using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;
using Dropbox.Api;

namespace Dev2.Views.DropBox2016
{
    public class DropBoxSourceViewModel
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string AccessToken { get; set; }
        public string Uid { get; set; }
        public string Secret { get; set; }
        public string Title { get { return "Dropbox Source"; } }
        public string AuthUri { get; set; }
        IDropBoxHelper DropBoxHelper { get; set; }
        public bool HasAuthenticated { get; set; }
        public IContextualResourceModel Resource { get; set; }


        // ReSharper restore UnusedAutoPropertyAccessor.Local
        readonly INetworkHelper _network;
        public DropboxClient Client { get; set; }
        //private string AppKey = GlobalConstants.DropBoxApiKey;       
        private const string AppKey = "31qf750f1vzffhu";

        private string _oauth2State;
        private const string RedirectUri = "https://www.example.com/";

        // ReSharper disable TooManyDependencies
        public DropBoxSourceViewModel(INetworkHelper network, IDropBoxHelper dropboxHelper, IDropboxFactory dropboxFactory, bool shouldAuthorise)
        // ReSharper restore TooManyDependencies
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "network", network }, { "dropboxHelper", dropboxHelper }, { "dropboxFactory", dropboxFactory } });
            _network = network;
            DropBoxHelper = dropboxHelper;
            CookieHelper.Clear();
            if (shouldAuthorise)
                Authorise();
        }

        public async Task LoadBrowserUri(string uri)
        {
            AuthUri = uri;
            var hasConnection = await _network.HasConnectionAsync(uri);
            if (hasConnection)
            {

                DropBoxHelper.WebBrowser.Navigated += (sender, args) => GetAuthTokens(args);
                DropBoxHelper.WebBrowser.LoadCompleted += (sender, args) => Execute.OnUIThread(() =>
                {
                    DropBoxHelper.CircularProgressBar.Visibility = Visibility.Hidden;
                    DropBoxHelper.WebBrowser.Visibility = Visibility.Visible;
                });

                DropBoxHelper.Navigate(AuthUri);
            }
        }



        public async void Authorise()
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
                Uid = result.Uid;
                HasAuthenticated = true;
                DropBoxHelper.CloseAndSave(this);
            }
            catch (ArgumentException)
            {
                DropBoxHelper.CloseAndSave(this);
            }
        }
    }
}
