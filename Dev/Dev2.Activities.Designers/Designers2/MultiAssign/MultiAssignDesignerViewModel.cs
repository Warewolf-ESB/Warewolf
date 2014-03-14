using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.MultiAssign
{
    public class MultiAssignDesignerViewModel : ActivityCollectionDesignerViewModel<ActivityDTO>
    {
        public Func<string> GetDatalistString = () => DataListSingleton.DataListAsXmlString;
        public MultiAssignDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();

            dynamic mi = ModelItem;
            InitializeItems(mi.FieldsCollection);
        }

        public override string CollectionName { get { return "FieldsCollection"; } }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as ActivityDTO;
            if(dto == null)
            {
                yield break;
            }

            foreach(var error in dto.GetRuleSet("FieldName", GetDatalistString()).ValidateRules("'Variable'", () => mi.SetProperty("IsFieldNameFocused", true)))
            {
                yield return error;
            }
            foreach(var error in dto.GetRuleSet("FieldValue", GetDatalistString()).ValidateRules("'New Value'", () => mi.SetProperty("IsFieldValueFocused", true)))
            {
                yield return error;
            }
        }
    }
}