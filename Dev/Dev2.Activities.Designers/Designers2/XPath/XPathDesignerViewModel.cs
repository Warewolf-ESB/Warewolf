using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Data.Util;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.XPath
{
    public class XPathDesignerViewModel : ActivityCollectionDesignerViewModel<XPathDTO>
    {
        public Func<string> GetDatalistString = () => DataListSingleton.DataListAsXmlString;
        public XPathDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();
            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);
        }
        public override string CollectionName { get { return "ResultsCollection"; } }


        public bool IsSourceStringFocused { get { return (bool)GetValue(IsSourceStringFocusedProperty); } set { SetValue(IsSourceStringFocusedProperty, value); } }
        public static readonly DependencyProperty IsSourceStringFocusedProperty = DependencyProperty.Register("IsSourceStringFocused", typeof(bool), typeof(XPathDesignerViewModel), new PropertyMetadata(default(bool)));

        string SourceString { get { return GetProperty<string>(); } }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var error in GetRuleSet("SourceString").ValidateRules("'XML'", () => IsSourceStringFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
        }

        IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch(propertyName)
            {
                case "SourceString":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => SourceString));

                    if(!string.IsNullOrEmpty(SourceString) && !DataListUtil.IsEvaluated(SourceString))
                    {
                        ruleSet.Add(new IsValidXmlRule(() => SourceString));
                    }

                    var outputExprRule = new IsValidExpressionRule(() => SourceString, GetDatalistString(), "1");
                    ruleSet.Add(outputExprRule);

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

            foreach(var error in dto.GetRuleSet("OutputVariable", GetDatalistString()).ValidateRules("'Results'", () => mi.SetProperty("IsOutputVariableFocused", true)))
            {
                yield return error;
            }
            foreach(var error in dto.GetRuleSet("XPath", GetDatalistString()).ValidateRules("'XPath'", () => mi.SetProperty("IsXpathVariableFocused", true)))
            {
                yield return error;
            }
        }
    }
}