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
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Studio.Interfaces;
using System;
using Dev2.TO;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Activities.Factories.Case;

namespace Dev2.Activities.Designers2.CaseConvert
{
    public class CaseConvertDesignerViewModel : ActivityCollectionDesignerViewModel<CaseConvertTO>
    {
        public ObservableCollection<string> ItemsList { get; private set; }

        internal Func<string> _getDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        public CaseConvertDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarLargeToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.ConvertCollection);

            if (mi.ConvertCollection == null || mi.ConvertCollection.Count <= 0)
            {
                mi.ConvertCollection.Add(CaseConverterFactory.CreateCaseConverterTO("", "UPPER", "", 1));
                mi.ConvertCollection.Add(CaseConverterFactory.CreateCaseConverterTO("", "UPPER", "", 2));
            }

            ItemsList = CaseConverter.ConvertTypes.ToObservableCollection();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Data_Case_Convert;
        }

        public override string CollectionName => "ConvertCollection";

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as CaseConvertTO;
            if (dto == null)
            {
                yield break;
            }

            foreach (var error in dto.GetRuleSet("StringToConvert", _getDatalistString?.Invoke()).ValidateRules("'StringToConvert'", () => mi.SetProperty("IsStringToConvertFocused", true)))
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
