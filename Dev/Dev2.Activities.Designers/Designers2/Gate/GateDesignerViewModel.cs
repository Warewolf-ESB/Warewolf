/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Warewolf;
using Warewolf.Data;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;
using Warewolf.Service;
using Warewolf.UI;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged, IEnabled
    {
        List<NameValue> _gates;
        private NameValue _selectedGate;
        private bool _enabled;
        private string _conditionExpressionText;
        private bool _isExpanded;
        private OptionsWithNotifier _options;
        private OptionsWithNotifier _conditionExpressionOptions;
        private readonly ModelItem _modelItem;

        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            _modelItem = modelItem;
            LoadDefaults();
            ClearGates();
            LoadGates();
            LoadOptions();
            PopulateFields();
            LoadConditionExpressionOptions();
            this.RunViewSetup();
        }

        private void LoadDefaults()
        {
            AddTitleBarLargeToggle();
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            IsExpanded = false;
            Enabled = true;

            DeleteConditionCommand = new DelegateCommand(o =>
            {

            });
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_Gate;
        }

        private void PopulateFields()
        {
            var id = _modelItem.Properties["RetryEntryPointId"].ComputedValue;
            if (id != null && id.ToString() != Guid.Empty.ToString() && Gates.Count > 1)
            {
                var nameValue = Gates.First(o => o.Value == id.ToString());
                SelectedGate = nameValue;
                IsExpanded = true;
            }
            else
            {
                SelectedGate = Gates[0];
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        private void LoadGates()
        {
            var designerView = FindDependencyParent.FindParent<System.Activities.Presentation.View.DesignerView>(_modelItem.View);

            if (designerView != null && designerView.DataContext is IWorkflowDesignerViewModel workflowDesignerViewModel)
            {
                Gates = workflowDesignerViewModel.GetSelectableGates(_modelItem.Properties["UniqueID"].ComputedValue.ToString());
            }
        }

        private void LoadConditionExpressionOptions()
        {
            var conditionExpressionList = _modelItem.Properties["Conditions"].ComputedValue as IList<ConditionExpression>;
            if (conditionExpressionList is null)
            {
                conditionExpressionList = new List<ConditionExpression>();
            }
            var result = OptionConvertor.ConvertFromListOfT(conditionExpressionList);
            ConditionExpressionOptions = new OptionsWithNotifier { Options = result };
            UpdateConditionExpressionOptionsModelItem();
        }

        private void LoadOptions()
        {
            var gateOptions = _modelItem.Properties["GateOptions"].ComputedValue as GateOptions;
            if (gateOptions is null)
            {
                gateOptions = new GateOptions();
                _modelItem.Properties["GateOptions"].SetValue(gateOptions);
            }

            gateOptions.OnChange += UpdateOptionsModelItem;
            _gateOptionsInst = gateOptions;
            var result = new List<IOption>();
            var failureOptions = OptionConvertor.Convert(gateOptions);
            result.AddRange(failureOptions);
            Options = new OptionsWithNotifier { Options = result };
        }

        private GateOptions _gateOptionsInst { get; set; }

        public ICommand DeleteConditionCommand { get; set; }

        public List<NameValue> Gates
        {
            get => _gates;
            set
            {
                _gates = value;
                OnPropertyChanged(nameof(Gates));
            }
        }

        public NameValue SelectedGate
        {
            get => _selectedGate;
            set
            {
                _selectedGate = value;
                OnPropertyChanged(nameof(SelectedGate));

                var retryEntryPointId = value?.Value;
                if (retryEntryPointId is null)
                {
                    _modelItem.Properties["RetryEntryPointId"]?.SetValue(Guid.Empty);
                }
                else
                {
                    _modelItem.Properties["RetryEntryPointId"]?.SetValue(Guid.Parse(retryEntryPointId));
                }
            }
        }

        private void UpdateConditionExpressionOptionsModelItem()
        {
            if (ConditionExpressionOptions?.Options != null)
            {
                var tmp = OptionConvertor.ConvertToListOfT<ConditionExpression>(ConditionExpressionOptions.Options);
                _modelItem.Properties["Conditions"]?.SetValue(tmp);
                AddEmptyConditionExpression();
                SetExpressionText();
                foreach (var item in ConditionExpressionOptions.Options)
                {
                    if (item is OptionConditionExpression conditionExpression)
                    {
                        conditionExpression.DeleteCommand = new DelegateCommand(o =>
                        {
                            RemoveConditionExpression(conditionExpression);
                        });
                    }
                }
            }
        }

        private void SetExpressionText()
        {
            var conditionExpressionList = _modelItem.Properties["Conditions"].ComputedValue as IList<ConditionExpression>;

            var text = new StringBuilder();
            var dds = conditionExpressionList.GetEnumerator();
            if (dds.MoveNext() && dds.Current.Cond.MatchType != enDecisionType.Choose)
            {
                dds.Current.RenderDescription(text);
            }
            while (dds.MoveNext())
            {
                var conditionExpression = dds.Current;
                if (conditionExpression.Cond.MatchType == enDecisionType.Choose)
                {
                    continue;
                }

                text.Append("\n AND \n");
                conditionExpression.RenderDescription(text);
            }
            ConditionExpressionText = text.ToString();
        }

        private void UpdateOptionsModelItem()
        {
            if (Options?.Options != null)
            {
                _modelItem.Properties["GateOptions"]?.ClearValue();
                // Call Convert to ensure the model is _fully_ updated
                OptionConvertor.Convert(typeof(GateOptions), Options.Options, _gateOptionsInst);
                _modelItem.Properties["GateOptions"]?.SetValue(_gateOptionsInst);
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public string ConditionExpressionText 
        {
            get => _conditionExpressionText;
            set
            {
                _conditionExpressionText = value;
                OnPropertyChanged(nameof(ConditionExpressionText));
            }
        }

        private void AddEmptyConditionExpression()
        {
            var emptyRows = ConditionExpressionOptions.Options.Where(o => o is OptionConditionExpression optionCondition && optionCondition.IsEmptyRow);

            if (!emptyRows.Any())
            {
                var conditionExpression = new OptionConditionExpression();
                var list = new List<IOption>(_conditionExpressionOptions.Options)
                {
                    conditionExpression
                };
                ConditionExpressionOptions.Options = list;
                OnPropertyChanged(nameof(ConditionExpressionOptions));
            }
        }

        private void RemoveConditionExpression(OptionConditionExpression conditionExpression)
        {
            var count = ConditionExpressionOptions.Options.Count(o => o is OptionConditionExpression optionCondition && optionCondition.IsEmptyRow);
            var empty = conditionExpression.IsEmptyRow;
            var allow = !empty || (empty && count > 1);

            if (_conditionExpressionOptions.Options.Count > 1 && allow)
            {
                var list = new List<IOption>(_conditionExpressionOptions.Options);
                list.Remove(conditionExpression);
                ConditionExpressionOptions.Options = list;
                OnPropertyChanged(nameof(ConditionExpressionOptions));
            }
        }

        public OptionsWithNotifier ConditionExpressionOptions
        {
            get => _conditionExpressionOptions;
            set
            {
                _conditionExpressionOptions = value;
                OnPropertyChanged(nameof(ConditionExpressionOptions));
                _conditionExpressionOptions.OptionChanged += UpdateConditionExpressionOptionsModelItem;
            }
        }

        public OptionsWithNotifier Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged(nameof(Options));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public void ClearGates()
        {
            Gates = new List<NameValue> { new NameValue { Name = "End", Value = Guid.Empty.ToString() } };
        }
    }
}
