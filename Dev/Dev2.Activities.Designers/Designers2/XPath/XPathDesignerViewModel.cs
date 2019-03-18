#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Data.Util;
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.XPath
{
    public class XPathDesignerViewModel : ActivityCollectionDesignerViewModel<XPathDTO>
    {
        internal Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        public XPathDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Utility_Xpath;
        }
        public override string CollectionName => "ResultsCollection";


        public bool IsSourceStringFocused { get => (bool)GetValue(IsSourceStringFocusedProperty); set => SetValue(IsSourceStringFocusedProperty, value); }
        public static readonly DependencyProperty IsSourceStringFocusedProperty = DependencyProperty.Register("IsSourceStringFocused", typeof(bool), typeof(XPathDesignerViewModel), new PropertyMetadata(default(bool)));

        string SourceString => GetProperty<string>();

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {            
            foreach(var error in GetRuleSet("SourceString").ValidateRules("'XML'", () => IsSourceStringFocused = true))            
            {
                yield return error;
            }
        }

        IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch (propertyName)
            {
                case "SourceString":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => SourceString));

                    if (!string.IsNullOrEmpty(SourceString) && !DataListUtil.IsEvaluated(SourceString))
                    {
                        ruleSet.Add(new IsValidXmlRule(() => SourceString));
                    }

                    var outputExprRule = new IsValidExpressionRule(() => SourceString, GetDatalistString?.Invoke(), "1", new VariableUtils());
                    ruleSet.Add(outputExprRule);

                    break;
                default:
                    break;
            }
            return ruleSet;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as XPathDTO;
            if(dto == null)
            {
                yield break;
            }

            foreach(var error in dto.GetRuleSet("OutputVariable", GetDatalistString?.Invoke()).ValidateRules("'Results'", () => mi.SetProperty("IsOutputVariableFocused", true)))
            {
                yield return error;
            }
            foreach(var error in dto.GetRuleSet("XPath", GetDatalistString?.Invoke()).ValidateRules("'XPath'", () => mi.SetProperty("IsXpathVariableFocused", true)))
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
