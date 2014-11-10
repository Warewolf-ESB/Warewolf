using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Caliburn.Micro;
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
        DropNetClient _client;
        public string Title { get { return "Dropbox Source"; } }
        public DropBoxSourceViewModel(INetworkHelper network,IDropBoxHelper dropboxHelper)
        {
            _network = network;
            DropBoxHelper = dropboxHelper;
            DropBoxHelper.WebBrowser.Navigated += WebBrowser_Navigated;
            Authorise();
        }

        void WebBrowser_Navigated(object sender, NavigationEventArgs e)
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


                DropBoxHelper.WebBrowser.Navigated += (sender, args) => GetAuthTokens(DropBoxHelper.WebBrowser,args);
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
            _client = new DropNetClient("l6vuufdy2psuyif", "tqtil4c1ibja8dn");
            var authorizeUrl = _client.GetTokenAndBuildUrl("http://www.google.com");
            await LoadBrowserUri(authorizeUrl);
        }
        void GetAuthTokens(WebBrowser webBrowser, NavigationEventArgs args)
        {
            if (args.Uri.ToString().StartsWith("https://www.google"))
            {
                HasAuthenticated = true;
               DropBoxHelper.CloseAndSave(this);
            }

        }
    }
}
