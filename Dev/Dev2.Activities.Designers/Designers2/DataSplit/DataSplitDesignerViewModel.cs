using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataSplit
{
    public class DataSplitDesignerViewModel : ActivityCollectionDesignerViewModel<DataSplitDTO>
    {
        public IList<string> ItemsList { get; private set; }

        public DataSplitDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            ProcessDirectionGroup = string.Format("ProcessDirectionGroup{0}", Guid.NewGuid());

            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();

            ItemsList = new List<string> { "Index", "Chars", "New Line", "Space", "Tab", "End" };
            SplitTypeUpdatedCommand = new RelayCommand(OnSplitTypeChanged, o => true);

            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);

            for(var i = 0; i < mi.ResultsCollection.Count; i++)
            {
                OnSplitTypeChanged(i);
            }
        }

        public override string CollectionName { get { return "ResultsCollection"; } }

        public ICommand SplitTypeUpdatedCommand { get; private set; }

        public bool IsSourceStringFocused { get { return (bool)GetValue(IsSourceStringFocusedProperty); } set { SetValue(IsSourceStringFocusedProperty, value); } }
        public static readonly DependencyProperty IsSourceStringFocusedProperty = DependencyProperty.Register("IsSourceStringFocused", typeof(bool), typeof(DataSplitDesignerViewModel), new PropertyMetadata(default(bool)));

        public string ProcessDirectionGroup { get { return (string)GetValue(ProcessDirectionGroupProperty); } set { SetValue(ProcessDirectionGroupProperty, value); } }
        public static readonly DependencyProperty ProcessDirectionGroupProperty = DependencyProperty.Register("ProcessDirectionGroup", typeof(string), typeof(DataSplitDesignerViewModel), new PropertyMetadata(default(string)));

        string SourceString { get { return GetProperty<string>(); } }

        void OnSplitTypeChanged(object indexObj)
        {
            var index = (int)indexObj;
            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];
            var splitType = mi.GetProperty("SplitType") as string;

            if(splitType == "Index" || splitType == "Chars")
            {
                mi.SetProperty("EnableAt", true);
            }
            else
            {
                mi.SetProperty("At", string.Empty);
                mi.SetProperty("EnableAt", false);
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var error in GetRuleSet("SourceString").ValidateRules("'String to Split'", () => IsSourceStringFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as DataSplitDTO;
            if(dto == null)
            {
                yield break;
            }

            foreach(var error in dto.GetRuleSet("OutputVariable").ValidateRules("'Results'", () => mi.SetProperty("IsOutputVariableFocused", true)))
            {
                yield return error;
            }
            foreach(var error in dto.GetRuleSet("At").ValidateRules("'Using'", () => mi.SetProperty("IsAtFocused", true)))
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
                    var sourceExprRule = new IsValidExpressionRule(() => SourceString);
                    ruleSet.Add(sourceExprRule);
                    ruleSet.Add(new IsStringNullOrWhiteSpaceRule(() => sourceExprRule.ExpressionValue));
                    break;
            }
            return ruleSet;
        }
    }
}