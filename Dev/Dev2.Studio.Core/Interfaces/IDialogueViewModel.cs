using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Studio.Core.AppResources;

namespace Dev2.Studio.Core.Interfaces {
    // Sashen Naidoo - 29-08-2012 - PBI 5037
    public interface IDialogueViewModel : IDisposable {
        
        String Title { get; }
        String DescriptionTitleText { get; }
        ImageSource ImageSource { get; }
        String DescriptionText { get; }
        String Hyperlink { get; }
        string HyperlinkText { get; }
        Visibility HyperlinkVisibility { get; }
        ICommand OKCommand { get; }
        //event ClosedOperationEventHandler OnOkClick;
        void SetupDialogue(string title, string description, string imageSourceuri, string DescriptionTitleText, string hyperlink = null, string linkText = null);

    }
}
