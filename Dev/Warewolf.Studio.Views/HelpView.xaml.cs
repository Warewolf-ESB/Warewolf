using System.Windows.Media;
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
            RichTextDocument richTextDocument = XamRichTextEditor.Document;
            
            Color color = (Color)ColorConverter.ConvertFromString("#FFF4F2EE");
            
            var colorInfo = new ColorInfo(color);
            richTextDocument.RootNode.Settings = new DocumentSettings
            {
                Background = colorInfo,
            };
            XamRichTextEditor.CaretColor = colorInfo;
        }
    }
}
