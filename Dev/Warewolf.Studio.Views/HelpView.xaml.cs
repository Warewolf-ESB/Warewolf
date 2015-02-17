using Dev2.Common.Interfaces;

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
            //DataContextChanged+=OnDataContextChanged;
            //RichTextDocument richTextDocument = XamRichTextEditor.Document;
            //// ReSharper disable PossibleNullReferenceException
            //Color color = (Color)ColorConverter.ConvertFromString("#FFF4F2EE");
            //// ReSharper restore PossibleNullReferenceException
            //var colorInfo = new ColorInfo(color);
            //richTextDocument.RootNode.Settings = new DocumentSettings
            //{
            //    Background = colorInfo,
            //    //DefaultCharacterSettings = new CharacterSettings
            //    {
            //        FontSettings = new FontSettings { Ascii = "Consolas" }
            //    }
            //};
            //XamRichTextEditor.CaretColor = colorInfo;
		}

	    public string GetCurrentHelpText()
	    {
	        var currentHelpText = "";
            //using (var memoryStream = new MemoryStream())
            //{
            //    XamRichTextEditor.Document.Save(new HtmlSerializationProvider(),memoryStream);
            //    memoryStream.Position = 0;
            //    using (StreamReader reader = new StreamReader(memoryStream))
            //    {
            //        currentHelpText = reader.ReadToEnd();
            //    }
            //}
	        return currentHelpText;
	    }
	}
}