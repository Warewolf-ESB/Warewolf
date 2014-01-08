using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IWebsiteEditorViewModel : IPropertyEditorWizard
    {
        IUserMessageProvider UserMessageProvider { get; set; }
        bool IsValidMarkup { get; set; }
        string MetaTags { get; set; }
        IWebResourceViewModel SelectedWebResource { get; set; }
        object Browser { get; set; }
        IEnvironmentModel ResourceEnvironment { get; }
        int Rows { get; set; }
        IWebResourceViewModel RootWebResource { get; }
        ObservableCollection<ILayoutObjectViewModel> LayoutObjects { get; }
        ObservableCollection<IResourceModel> WebpageServices { get; }
        IResourceModel SelectedDefaultWebpage { get; set; }
        string Url { get; }
        string WizardUrl { get; }
        string ResourceName { get; }
        string XmlConfiguration { get; set; }
        IWebActivity WebActivity { get; }
        IResourceModel ResourceModel { get; }
        string Html { get; set; }
        string SearchEngineKeywords { get; set; }
        bool CanRemove { get; }
        ICommand AddExistingWebResourceCommand { get; }
        ICommand RemoveWebResourceCommand { get; }
        ICommand CopyCommand { get; }
        ICommand EditCommand { get; }
        void Deploy();
        void Navigate();
        void SetSelected(ILayoutObjectViewModel obj);
        void UpdateModelItem();
    }
}