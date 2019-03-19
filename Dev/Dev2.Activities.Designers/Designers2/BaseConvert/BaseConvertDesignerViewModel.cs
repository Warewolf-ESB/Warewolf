#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Converters;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using System;
using Dev2.Studio.Core;

namespace Dev2.Activities.Designers2.BaseConvert
{
    public class BaseConvertDesignerViewModel : ActivityCollectionDesignerViewModel<BaseConvertTO>
    {
        internal Func<string> _getDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        public BaseConvertDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarLargeToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.ConvertCollection);

            ConvertTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enDev2BaseConvertType>();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Data_Base_Convert;
        }

        public IList<string> ConvertTypes { get; set; }

        public override string CollectionName => "ConvertCollection";

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as BaseConvertTO;
            if (dto == null)
            {
                yield break;
            }

            foreach (var error in dto.GetRuleSet("FromExpression", _getDatalistString?.Invoke()).ValidateRules("'FromExpression'", () => mi.SetProperty("IsFromExpressionFocused", true)))
            {
                yield return error;
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
