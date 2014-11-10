using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;
using DropNet;

namespace Dev2.Views.DropBox
{
    public class DropBoxSourceViewModel
    {
        string AuthUri { get; set; }
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string Key { get; set; }

        public  string Secret { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        IDropBoxHelper DropBoxHelper { get; set; }
        public bool HasAuthenticated { get; set; }
        public IResourceModel Resource { get; set; }
        readonly INetworkHelper _network;
        readonly IDropboxFactory _dropboxFactory;
        IDropNetClient _client;
        public string Title { get { return "Dropbox Source"; } }
        public DropBoxSourceViewModel(INetworkHelper network,IDropBoxHelper dropboxHelper,IDropboxFactory dropboxFactory)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "network", network }, { "dropboxHelper", dropboxHelper }, { "dropboxFactory",dropboxFactory } });
            _network = network;
            _dropboxFactory = dropboxFactory;
            DropBoxHelper = dropboxHelper;
            DropBoxHelper.WebBrowser.Navigated += WebBrowserNavigated;
            Authorise();
        }

        void WebBrowserNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("https://www.google") && _client != null)
            {
               var token =  _client.GetAccessToken();
               Key = token.Token;
               Secret = token.Secret;

            }

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
            _client = _dropboxFactory.Create();
            var authorizeUrl = _client.GetTokenAndBuildUrl("http://www.google.com");
            await LoadBrowserUri(authorizeUrl);
        }
        void GetAuthTokens(NavigationEventArgs args)
        {
            if (args.Uri.ToString().StartsWith("https://www.google"))
            {
                HasAuthenticated = true;
                DropBoxHelper.CloseAndSave(this);
            }

        }
    }

    public interface IDropboxFactory
    {
        IDropNetClient Create();
    }

    public class DropboxFactory : IDropboxFactory
    {
        #region Implementation of IDropboxFactory

        public IDropNetClient Create()
        {
            return new DropNetClient(GlobalConstants.DropBoxApiKey, GlobalConstants.DropBoxAppSecret);
        }

        #endregion
    }
}
