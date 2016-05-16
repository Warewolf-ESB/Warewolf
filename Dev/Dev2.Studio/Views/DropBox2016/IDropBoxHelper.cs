using System.Windows.Controls;
using Dev2.CustomControls;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.ViewModels;

namespace Dev2.Views.DropBox2016
{
    public interface IDropBoxHelper
    {
        DropBoxViewWindow DropBoxViewWindow { get; }
        WebBrowser WebBrowser { get; }
        CircularProgressBar CircularProgressBar { get; }
        void Navigate(string uri);

        void CloseAndSave(ManageOAuthSourceViewModel manageOAuthSourceViewModel);

        IEnvironmentModel ActiveEnvironment { get; set; }
        string ResourceType { get; set; }
        string ResourcePath { get; set; }

    }
}