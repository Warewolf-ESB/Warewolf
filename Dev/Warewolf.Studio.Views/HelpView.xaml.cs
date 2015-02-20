using System.IO;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Infragistics.Documents.RichText;
using Infragistics.Documents.RichText.Html;

namespace Warewolf.Studio.Views
{
	/// <summary>
	/// Interaction logic for HelpView.xaml
	/// </summary>
	public partial class HelpView: IHelpView
	{
		public HelpView()
		{
			InitializeComponent();
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

	    public string GetCurrentHelpText()
	    {
	        string currentHelpText;
            using (var memoryStream = new MemoryStream())
            {
                XamRichTextEditor.Document.Save(new HtmlSerializationProvider(),memoryStream);
                memoryStream.Position = 0;
                using (var reader = new StreamReader(memoryStream))
                {
                    currentHelpText = reader.ReadToEnd();
                }
            }
	        return currentHelpText;
	    }
	}
}