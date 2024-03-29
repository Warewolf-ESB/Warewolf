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

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataSplit
{
    public class DataSplitDesignerViewModel : ActivityCollectionDesignerViewModel<DataSplitDTO>
    {
        internal Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        public IList<string> ItemsList { get; private set; }

        public DataSplitDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            ProcessDirectionGroup = $"ProcessDirectionGroup{Guid.NewGuid()}";

            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();

            ItemsList = new List<string>
            {
                DataSplitDTO.SplitTypeIndex,
                DataSplitDTO.SplitTypeChars,
                DataSplitDTO.SplitTypeNewLine,
                DataSplitDTO.SplitTypeSpace,
                DataSplitDTO.SplitTypeTab,
                DataSplitDTO.SplitTypeEnd
            };
            SplitTypeUpdatedCommand = new DelegateCommand(OnSplitTypeChanged);

            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);

            for(var i = 0; i < mi.ResultsCollection.Count; i++)
            {
                OnSplitTypeChanged(i);
            }

            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Data_Data_Split;
        }

        public override string CollectionName => "ResultsCollection";

        public ICommand SplitTypeUpdatedCommand { get; private set; }

        public bool IsSourceStringFocused { get => (bool)GetValue(IsSourceStringFocusedProperty); set => SetValue(IsSourceStringFocusedProperty, value); }
        public static readonly DependencyProperty IsSourceStringFocusedProperty = DependencyProperty.Register("IsSourceStringFocused", typeof(bool), typeof(DataSplitDesignerViewModel), new PropertyMetadata(default(bool)));

        public string ProcessDirectionGroup { get => (string)GetValue(ProcessDirectionGroupProperty); set => SetValue(ProcessDirectionGroupProperty, value); }
        public static readonly DependencyProperty ProcessDirectionGroupProperty = DependencyProperty.Register("ProcessDirectionGroup", typeof(string), typeof(DataSplitDesignerViewModel), new PropertyMetadata(default(string)));

        string SourceString => GetProperty<string>();

        void OnSplitTypeChanged(object indexObj)
        {
            var index = (int)indexObj;
            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];
            var splitType = mi.GetProperty("SplitType") as string;
            switch (splitType)
            {
                case DataSplitDTO.SplitTypeIndex:
                    mi.SetProperty("IsEscapeCharEnabled", false);
                    mi.SetProperty("EscapeChar", string.Empty);
                    mi.SetProperty("EnableAt", true);
                    break;
                case DataSplitDTO.SplitTypeChars:
                    mi.SetProperty("IsEscapeCharEnabled", true);
                    mi.SetProperty("EnableAt", true);
                    break;
                case DataSplitDTO.SplitTypeNewLine:
                    mi.SetProperty("IsEscapeCharEnabled", true);
                    mi.SetProperty("EnableAt", false);
                    mi.SetProperty("At", string.Empty);
                    break;
                case DataSplitDTO.SplitTypeSpace:
                    mi.SetProperty("IsEscapeCharEnabled", true);
                    mi.SetProperty("EnableAt", false);
                    mi.SetProperty("At", string.Empty);
                    break;
                case DataSplitDTO.SplitTypeTab:
                    mi.SetProperty("IsEscapeCharEnabled", true);
                    mi.SetProperty("EnableAt", false);
                    mi.SetProperty("At", string.Empty);
                    break;
                case DataSplitDTO.SplitTypeEnd:
                    mi.SetProperty("IsEscapeCharEnabled", false);
                    mi.SetProperty("EscapeChar", string.Empty);
                    mi.SetProperty("EnableAt", false);
                    mi.SetProperty("At", string.Empty);
                    break;
                default:
                    break;
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
         
            foreach(var error in GetRuleSet("SourceString").ValidateRules("'String to Split'", () => IsSourceStringFocused = true))
            
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


            foreach(var error in dto.GetRuleSet("OutputVariable", GetDatalistString?.Invoke()).ValidateRules("'Results'", () => mi.SetProperty("IsOutputVariableFocused", true)))
            {
                yield return error;
            }
            foreach(var error in dto.GetRuleSet("At", GetDatalistString?.Invoke()).ValidateRules("'Using'", () => mi.SetProperty("IsAtFocused", true)))
            {
                yield return error;
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch (propertyName)
            {
                case "SourceString":
                    if (!string.IsNullOrEmpty(SourceString) && !string.IsNullOrWhiteSpace(SourceString))
                    {
                        var inputExprRule = new IsValidExpressionRule(() => SourceString, GetDatalistString?.Invoke(), "1", new VariableUtils());
                        ruleSet.Add(inputExprRule);
                    }
                    else
                    {
                        ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => SourceString));
                    }

                    break;
                default:
                    break;
            }
            return ruleSet;
        }
    }
}
