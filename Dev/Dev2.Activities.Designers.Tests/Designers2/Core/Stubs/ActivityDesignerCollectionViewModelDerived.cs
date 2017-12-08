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
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Stubs
{
    public class ActivityDesignerCollectionViewModelDerived : ActivityCollectionDesignerViewModel<ActivityDTO>
    {
        public ActivityDesignerCollectionViewModelDerived(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public override string CollectionName => "FieldsCollection";

        public void TestAddTitleBarQuickVariableInputToggle()
        {
            AddTitleBarQuickVariableInputToggle();
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            yield break;
        }

        #region Overrides of ActivityDesignerViewModel

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion
    }
}
