
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;
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
        }



        public override string CollectionName { get { return "FilterCriteria"; } }

        public ObservableCollection<string> WhereOptions { get; private set; }

        public IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();
            return ruleSet;
        }
    }
}
