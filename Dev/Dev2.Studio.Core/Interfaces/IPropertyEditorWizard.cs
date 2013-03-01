using System.Windows;

namespace Dev2.Studio.Core.Interfaces
{
    public delegate void NavigateRequestedEventHandler(string uri);

    public interface IPropertyEditorWizard
    {
        Window Owner { get; set; }

        ILayoutObjectViewModel SelectedLayoutObject { get; }

        void Save(string value, bool closeBrowserWindow = true);

        void NavigateTo(string uri, string args, string returnUri);

        void OpenPropertyEditor();

        void Dev2Set(string data, string uri);

        void Dev2SetValue(string value);

        void Dev2Done();

        void Dev2ReloadResource(string resourceName, string resourceType);

        void Close();

        void Cancel();

        string FetchData(string args);

        string GetIntellisenseResults(string searchTerm, int caretPosition);

        event NavigateRequestedEventHandler NavigateRequested;
    }
}
