using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Dev2.Common.Interfaces;
using Infragistics.Documents.RichText;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : IHelpView
    {
        public HelpView()
        {
            InitializeComponent();
            HideScriptErrors(WebBrowserHost, true);
            RichTextDocument richTextDocument = XamRichTextEditor.Document;
            // ReSharper disable PossibleNullReferenceException
            Color color = (Color)ColorConverter.ConvertFromString("#FFF4F2EE");
            // ReSharper restore PossibleNullReferenceException
            var colorInfo = new ColorInfo(color);
            richTextDocument.RootNode.Settings = new DocumentSettings
            {
                Background = colorInfo,
            };
            XamRichTextEditor.CaretColor = colorInfo;
        }

        private void HideScriptErrors(WebBrowser wb, bool hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser)
                .GetField("_axIWebBrowser2",
                          BindingFlags.Instance | BindingFlags.NonPublic);
            object objComWebBrowser = fiComWebBrowser?.GetValue(wb);
            objComWebBrowser?.GetType().InvokeMember(
                "Silent", BindingFlags.SetProperty, null, objComWebBrowser,
                new object[] { hide });
        }
        void WebBrowserHost_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var browser = sender as WebBrowser;

            if (browser?.Document == null)
                return;

            dynamic document = browser.Document;

            if (document.readyState != "complete")
                return;

            dynamic script = document.createElement("script");
            script.type = @"text/javascript";
            script.text = @"window.onerror = function(msg,url,line){return true;}";
            document.head.appendChild(script);
        }

    }
}
