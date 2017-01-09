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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Communication;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Messages;
using Dev2.TO;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Designers2.Decision
{
    public class DecisionDesignerViewModel : ActivityCollectionDesignerObservableViewModel<DecisionTO>
    {
        readonly IList<string> _requiresSearchCriteria = new List<string> { "Doesn't Contain", "Contains", "=", "<> (Not Equal)", "Ends With", "Doesn't Start With", "Doesn't End With", "Starts With", "Is Regex", "Not Regex", ">", "<", "<=", ">=" };

        static readonly IList<IFindRecsetOptions> Whereoptions = FindRecsetOptions.FindAll();
        public DecisionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            Collection = new ObservableCollection<IDev2TOFn>();
            Collection.CollectionChanged += CollectionCollectionChanged;
            var collection = FindRecsetOptions.FindAllDecision().Select(c => c.HandlesType());
            WhereOptions = new ObservableCollection<string>(collection);
            SearchTypeUpdatedCommand = new DelegateCommand(OnSearchTypeChanged);
            _isInitializing = true;
            ConfigureDecisionExpression(ModelItem);
            InitializeItems(Tos);
            DeleteCommand = new DelegateCommand(x =>
            {
                DeleteRow(x as DecisionTO);
            });
            _isInitializing = false;
            if (DisplayText != DisplayName)
            {
                DisplayName = DisplayText;
            }
            if (string.IsNullOrEmpty(DisplayName) || DisplayName == "Decision")
            {
                DisplayName = "Decision";
                DisplayText = DisplayName;
            }
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_Decision;
        }

        void CollectionCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    ((DecisionTO)newItem).DeleteAction = DeleteRow;
                    ((DecisionTO)newItem).IsLast = true;
                }
            }
            for (int i = 0; i < Tos.Count - 1; i++)
            {
                ((DecisionTO)Tos[i]).IsLast = false;
            }
        }

        public void DeleteRow(DecisionTO row)
        {
            if (row != Collection.Last())
            {
                Collection.Remove(row);
                UpdateDecisionDisplayName((DecisionTO)Tos[0]);
            }
        }
        public ICommand DeleteCommand
        {
            get;
            set;
        }
        void ConfigureDecisionExpression(ModelItem mi)
        {

            var condition = mi;
            var expression = condition.Properties[GlobalConstants.ExpressionPropertyText];
            var ds = DataListConstants.DefaultStack;

            if (expression?.Value != null)
            {
                var eval = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(expression.Value.ToString());

                if (!string.IsNullOrEmpty(eval))
                {
                    ExpressionText = eval;
                }
            }
            else
            {
                Dev2JsonSerializer ser = new Dev2JsonSerializer();
                ExpressionText = ser.Serialize(ds);
            }

            var displayName = mi.Properties[GlobalConstants.DisplayNamePropertyText];
            if (displayName?.Value != null)
            {
                ds.DisplayText = displayName.Value.ToString();
            }
            Tos = ToObservableCollection();
        }

        #region Overrides of ActivityCollectionDesignerObservableViewModel<DecisionTO>

        public override void UpdateDto(IDev2TOFn dto)
        {
            var decto = dto as DecisionTO;
            if (decto != null)
            {
                decto.UpdateDisplayAction = UpdateDecisionDisplayName;
            }
        }

        #endregion

        public void GetExpressionText()
        {
            var stack = SetupTos(Collection);

            stack.DisplayText = DisplayText;
            stack.FalseArmText = FalseArmText;
            stack.TrueArmText = TrueArmText;
            stack.Mode = RequireAllDecisionsToBeTrue ? Dev2DecisionMode.AND : Dev2DecisionMode.OR;
            ExpressionText = DataListUtil.ConvertModelToJson(stack).ToString();

        }

        public override string CollectionName => "ResultsCollection";

        public ICommand SearchTypeUpdatedCommand { get; private set; }

        public ObservableCollection<string> WhereOptions { get; private set; }

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }
        public string TrueArmText { get; set; }
        public string FalseArmText { get; set; }
        public string ExpressionText { get; set; }
        public bool RequireAllDecisionsToBeTrue
        {
            get; set;
        }
        public ObservableCollection<IDev2TOFn> Tos
        {
            get
            {
                return Collection;
            }
            set
            {
                Collection.CollectionChanged -= CollectionCollectionChanged;
                Collection = value;
                Collection.CollectionChanged += CollectionCollectionChanged;
                var stack = SetupTos(Collection);
                ExpressionText = DataListUtil.ConvertModelToJson(stack).ToString();
            }
        }

        ObservableCollection<IDev2TOFn> ToObservableCollection()
        {
            if (!string.IsNullOrWhiteSpace(ExpressionText))
            {
                var val = new StringBuilder(ExpressionText);
                var decisions = DataListUtil.ConvertFromJsonToModel<Dev2DecisionStack>(val);
                if (decisions?.TheStack != null)
                {
                    TrueArmText = decisions.TrueArmText;
                    FalseArmText = decisions.FalseArmText;
                    DisplayText = decisions.DisplayText;
                    RequireAllDecisionsToBeTrue = decisions.Mode == Dev2DecisionMode.AND;
                    return new ObservableCollection<IDev2TOFn>(decisions.TheStack.Select((a, i) => new DecisionTO(a, i + 1, UpdateDecisionDisplayName, DeleteRow)));
                }
            }
            return new ObservableCollection<IDev2TOFn> { new DecisionTO() };
        }

        static Dev2DecisionStack SetupTos(IEnumerable<IDev2TOFn> valuecoll)
        {
            var val = new Dev2DecisionStack { TheStack = new List<Dev2Decision>() };
            var value = valuecoll.Select(a => a as DecisionTO);
            foreach (var decisionTo in value.Where(a => { return a != null && !a.IsEmpty(); }))
            {
                var dev2Decision = new Dev2Decision { Col1 = decisionTo.MatchValue };
                if (!string.IsNullOrEmpty(decisionTo.SearchCriteria))
                {
                    dev2Decision.Col2 = decisionTo.SearchCriteria;
                }
                dev2Decision.EvaluationFn = DecisionDisplayHelper.GetValue(decisionTo.SearchType);
                if (decisionTo.IsBetweenCriteriaVisible)
                {
                    dev2Decision.Col2 = decisionTo.From;
                    dev2Decision.Col3 = decisionTo.To;
                }
                if (string.IsNullOrEmpty(dev2Decision.Col3))
                {
                    dev2Decision.Col3 = "";
                }
                val.TheStack.Add(dev2Decision);
            }
            return val;
        }

        public bool IsDisplayTextFocused { get { return (bool)GetValue(IsDisplayTextFocusedProperty); } set { SetValue(IsDisplayTextFocusedProperty, value); } }
        public static readonly DependencyProperty IsDisplayTextFocusedProperty = DependencyProperty.Register("IsDisplayTextFocused", typeof(bool), typeof(DecisionDesignerViewModel), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(DecisionDesignerViewModel), new PropertyMetadata(default(string)));

        public bool IsTrueArmFocused { get { return (bool)GetValue(IsTrueArmFocusedProperty); } set { SetValue(IsTrueArmFocusedProperty, value); } }
        public static readonly DependencyProperty IsTrueArmFocusedProperty = DependencyProperty.Register("IsTrueArmFocused", typeof(bool), typeof(DecisionDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsFalseArmFocused { get { return (bool)GetValue(IsFalseArmFocusedProperty); } set { SetValue(IsFalseArmFocusedProperty, value); } }
        public static readonly DependencyProperty IsFalseArmFocusedProperty = DependencyProperty.Register("IsFalseArmFocused", typeof(bool), typeof(DecisionDesignerViewModel), new PropertyMetadata(default(bool)));
        private readonly bool _isInitializing;


        void OnSearchTypeChanged(object indexObj)
        {
            var index = (int)indexObj;
            try
            {
                UpdateDecisionDisplayName((DecisionTO)Tos[index]);
                if (index < 0 || index >= Tos.Count)
                {
                    return;
                }

                var mi = (DecisionTO)Tos[index];

                var searchType = mi.SearchType;

                DecisionTO.UpdateMatchVisibility(mi, mi.SearchType, Whereoptions);
                var requiresCriteria = _requiresSearchCriteria.Contains(searchType);
                mi.IsSearchCriteriaEnabled = requiresCriteria;
                if (!requiresCriteria)
                {
                    mi.SearchCriteria = string.Empty;
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, e);
            }
        }

        void UpdateDecisionDisplayName(DecisionTO dec)
        {
            if (DisplayText.StartsWith("If ") || DisplayText == "Decision")
            {
                if (dec != null && !_isInitializing && dec.IndexNumber == 1)
                {
                    DisplayName = string.Format("If {0} {3} {1} {2}", dec.MatchValue, dec.SearchType,
                        dec.IsBetweenCriteriaVisible
                            ? $"{dec.From} and {dec.To}"
                            : dec.SearchCriteria,
                        dec.SearchType == null || dec.SearchType.ToLower().Contains("is") ? "" : "Is");
                    DisplayText = string.Format("If {0} {3} {1} {2}", dec.MatchValue, dec.SearchType,
                        dec.IsBetweenCriteriaVisible
                            ? $"{dec.From} and {dec.To}"
                            : dec.SearchCriteria,
                        dec.SearchType == null || dec.SearchType.ToLower().Contains("is") ? "" : "Is");
                }
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var error in GetRuleSet("DisplayText").ValidateRules("'DisplayText'", () => IsDisplayTextFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var error in GetRuleSet("TrueArmText").ValidateRules("'TrueArmText'", () => IsTrueArmFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var error in GetRuleSet("FalseArmText").ValidateRules("'FalseArmText'", () => IsFalseArmFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(IDev2TOFn mi)
        {
            yield break;
        }

        private IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch (propertyName)
            {
                case "DisplayText":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => DisplayText));
                    break;

                case "TrueArmText":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => TrueArmText));
                    break;

                case "FalseArmText":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => FalseArmText));
                    break;
            }
            return ruleSet;
        }

        #region Implementation of IHandle<ConfigureDecisionExpressionMessage>

        // ReSharper disable once UnusedParameter.Global
        public void Handle(ConfigureDecisionExpressionMessage message)
        {
            ShowLarge = true;
        }

        #endregion

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
