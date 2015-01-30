using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Infragistics.Documents.RichText;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.ViewModels.Help;

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
		    RichTextDocument richTextDocument = XamRichTextEditor.Document;
            // ReSharper disable PossibleNullReferenceException
            Color color = (Color)ColorConverter.ConvertFromString("#FFF4F2EE");
		    // ReSharper restore PossibleNullReferenceException
		    var colorInfo = new ColorInfo(color);
            richTextDocument.RootNode.Settings = new DocumentSettings
		    {
                Background = colorInfo,
                DefaultCharacterSettings = new CharacterSettings
                {
                    FontSettings = new FontSettings { Ascii = "Consolas" }
                }
		    };
		    XamRichTextEditor.CaretColor = colorInfo;
		}
	}
}