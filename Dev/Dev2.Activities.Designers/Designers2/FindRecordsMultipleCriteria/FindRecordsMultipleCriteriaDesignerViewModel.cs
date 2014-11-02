
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.DataList;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public class FindRecordsMultipleCriteriaDesignerViewModel : ActivityCollectionDesignerViewModel<FindRecordsTO>
    {
        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        readonly IList<string> _requiresSearchCriteria = new List<string> { "Doesn't Contain", "Contains", "=", "<> (Not Equal)", "Ends With", "Doesn't Start With", "Doesn't End With", "Starts With", "Is Regex", "Not Regex", ">", "<", "<=", ">=" };

        public FindRecordsMultipleCriteriaDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            WhereOptions = new ObservableCollection<string>(FindRecsetOptions.FindAll().Select(c => c.HandlesType()));
            SearchTypeUpdatedCommand = new DelegateCommand(OnSearchTypeChanged);

            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);
        }

        public override string CollectionName { get { return "ResultsCollection"; } }

        public ICommand SearchTypeUpdatedCommand { get; private set; }

        public ObservableCollection<string> WhereOptions { get; private set; }

        string FieldsToSearch { get { return GetProperty<string>(); } }

        public bool IsFieldsToSearchFocused { get { return (bool)GetValue(IsFieldsToSearchFocusedProperty); } set { SetValue(IsFieldsToSearchFocusedProperty, value); } }
        public static readonly DependencyProperty IsFieldsToSearchFocusedProperty = DependencyProperty.Register("IsFieldsToSearchFocused", typeof(bool), typeof(FindRecordsMultipleCriteriaDesignerViewModel), new PropertyMetadata(default(bool)));

        string Result { get { return GetProperty<string>(); } }

        public bool IsResultFocused { get { return (bool)GetValue(IsResultFocusedProperty); } set { SetValue(IsResultFocusedProperty, value); } }
        public static readonly DependencyProperty IsResultFocusedProperty = DependencyProperty.Register("IsResultFocused", typeof(bool), typeof(FindRecordsMultipleCriteriaDesignerViewModel), new PropertyMetadata(default(bool)));

        void OnSearchTypeChanged(object indexObj)
        {
            var index = (int)indexObj;

            if(index == -1)
            {
                index = 0;
            }

            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];

            var searchType = mi.GetProperty("SearchType") as string;

            if(searchType == "Is Between" || searchType == "Not Between")
            {
                mi.SetProperty("IsSearchCriteriaVisible", false);
            }
            else
            {
                mi.SetProperty("IsSearchCriteriaVisible", true);
            }

            var requiresCriteria = _requiresSearchCriteria.Contains(searchType);
            mi.SetProperty("IsSearchCriteriaEnabled", requiresCriteria);
            if(!requiresCriteria)
            {
                mi.SetProperty("SearchCriteria", string.Empty);
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var error in GetRuleSet("FieldsToSearch").ValidateRules("'In Field(s)'", () => IsFieldsToSearchFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var error in GetRuleSet("Result").ValidateRules("'Result'", () => IsResultFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as FindRecordsTO;
            if(dto == null)
            {
                yield break;
            }

            foreach (var error in dto.GetRuleSet("SearchCriteria", GetDatalistString()).ValidateRules("'Match'", () => mi.SetProperty("IsSearchCriteriaFocused", true)))
            {
                yield return error;
            }

            foreach(var error in dto.GetRuleSet("From", GetDatalistString()).ValidateRules("'From'", () => mi.SetProperty("IsFromFocused", true)))
            {
                yield return error;
            }

            foreach(var error in dto.GetRuleSet("To", GetDatalistString()).ValidateRules("'To'", () => mi.SetProperty("IsToFocused", true)))
            {
                yield return error;
            }
        }

        public IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch(propertyName)
            {
                case "FieldsToSearch":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => FieldsToSearch));
                    ruleSet.Add(new IsValidExpressionRule(() => FieldsToSearch, GetDatalistString(), "1"));
                    ruleSet.Add(new HasNoDuplicateEntriesRule(() => FieldsToSearch));
                    ruleSet.Add(new HasNoIndexsInRecordsetsRule(() => FieldsToSearch));
                    break;

                case "Result":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => Result));
                    ruleSet.Add(new IsValidExpressionRule(() => Result, GetDatalistString(),"1"));
                    break;
            }
            return ruleSet;
        }
    }
}
