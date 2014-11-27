
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
