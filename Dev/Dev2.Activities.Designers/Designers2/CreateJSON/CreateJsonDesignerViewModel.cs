/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.TO;

namespace Dev2.Activities.Designers2.CreateJSON
{
    public class CreateJsonDesignerViewModel : ActivityCollectionDesignerViewModel<JsonMappingTo>
    {
        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        public CreateJsonDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();

            dynamic mi = ModelItem;
            InitializeItems(mi.JsonMappings);
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Utility_Create_JSON;
        }

        public override string CollectionName => "JsonMappings";

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        #region Overrides of ActivityCollectionDesignerViewModel<JsonMappingTo>

        protected override void DoCustomAction(string propertyName)
        {
            if (propertyName == "SourceName")
            {
                var dto = CurrentModelItem?.GetCurrentValue() as JsonMappingTo;
                if (dto != null)
                {
                    var destinationWithName = dto.GetDestinationWithName(dto.SourceName);
                    if (String.IsNullOrEmpty(dto.DestinationName))
                    {
                        CurrentModelItem.SetProperty("DestinationName", destinationWithName);
                    }
                }
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as JsonMappingTo;
            if(dto == null)
            {
                yield break;
            }

            foreach (var error in dto.GetRuleSet("SourceName", GetDatalistString()).ValidateRules("'Data'", () => mi.SetProperty("IsSourceNameFocused", true)))
            {
                yield return error;
            } 
            
            foreach (var error in dto.GetRuleSet("DestinationName", GetDatalistString()).ValidateRules("'Name'", () => mi.SetProperty("IsDestinationNameFocused", true)))
            {
                yield return error;
            }            
        }
    }
}
