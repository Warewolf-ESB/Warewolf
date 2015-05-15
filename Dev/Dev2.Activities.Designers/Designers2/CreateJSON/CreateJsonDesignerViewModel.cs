
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Util;
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
            AddTitleBarHelpToggle();

            dynamic mi = ModelItem;
            InitializeItems(mi.JsonMappings);
        }

        public override string CollectionName { get { return "JsonMappings"; } }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        #region Overrides of ActivityCollectionDesignerViewModel<JsonMappingTo>

        #region Overrides of ActivityDesignerViewModel

        #endregion

        protected override void DoCustomAction(string propertyName)
        {
            if (propertyName == "SourceName")
            {                     
                if (CurrentModelItem != null)
                {
                    var dto = CurrentModelItem.GetCurrentValue() as JsonMappingTo;
                    if (dto != null)
                    {
                        if (String.IsNullOrEmpty(dto.DestinationName))
                        {
                            if (DataListUtil.IsFullyEvaluated(dto.SourceName))
                            {
                                string destName;
                                if (DataListUtil.IsValueRecordset(dto.SourceName) || DataListUtil.IsValueRecordsetWithFields(dto.SourceName))
                                {
                                    destName = DataListUtil.ExtractRecordsetNameFromValue(dto.SourceName);
                                }
                                else
                                {
                                    destName = DataListUtil.StripBracketsFromValue(dto.SourceName);
                                }
                                CurrentModelItem.SetProperty("DestinationName", destName);
                            }
                        }          
                        
                    }
                }
            }
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
        }
    }
}
