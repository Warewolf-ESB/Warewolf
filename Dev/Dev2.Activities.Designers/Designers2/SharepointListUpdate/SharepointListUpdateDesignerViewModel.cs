/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Interfaces;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Threading;
using Dev2.TO;

namespace Dev2.Activities.Designers2.SharepointListUpdate
{
    public class SharepointListUpdateDesignerViewModel : SharepointListDesignerViewModelBase
    {
        public SharepointListUpdateDesignerViewModel(ModelItem modelItem)
            : base(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator,true)
        {
            WhereOptions = new ObservableCollection<string>(SharepointSearchOptions.SearchOptions());
            dynamic mi = ModelItem;
            InitializeItems(mi.FilterCriteria);
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Update_List_Item;
        }

        public override string CollectionName => "FilterCriteria";

        public ObservableCollection<string> WhereOptions { get; private set; }

        #region Overrides of ActivityCollectionDesignerViewModel<SharepointSearchTo>

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion
    }
}
