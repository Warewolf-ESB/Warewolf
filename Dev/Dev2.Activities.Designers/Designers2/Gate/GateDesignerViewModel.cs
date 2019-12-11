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
using Dev2.Common.Gates;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;
using Warewolf.Service;
using Warewolf.UI;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged, IEnabled
    {
        private string _selectedGateFailure;
        List<NameValue> _gates;
        private NameValue _selectedGate;
        private bool _enabled;
        private bool _isExpanded;
        private OptionsWithNotifier _options;
        private OptionsWithNotifier _conditionExpressionOptions;
        private IServer _server;
        private IResourceRepository _resourceRepository;
        private ConditionExpression _conditionExpression;
        private readonly ModelItem _modelItem;

        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            _modelItem = modelItem;
            LoadDefaults();
            ClearGates();
            LoadGates();

            PopulateFields();
            LoadConditionExpressionOptions();
            LoadOptions();
        }

        private void LoadDefaults()
        {
            AddTitleBarLargeToggle();
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            IsExpanded = false;
            Enabled = true;

            ConditionExpression = new ConditionExpression();

            DeleteConditionCommand = new DelegateCommand(o =>
            {

            });
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_Gate;
        }

        public ConditionExpression ConditionExpression
        {
            get => _conditionExpression;
            set
            {
                _conditionExpression = value;
                OnPropertyChanged(nameof(ConditionExpression));
            }
        }

        private void PopulateFields()
        {
            var gateFailure = _modelItem.Properties["GateFailure"].ComputedValue;
            if (gateFailure is null)
            {
                SelectedGateFailure = GetGateFailure(GateFailureAction.StopProcessing.ToString()).ToString();
            }
            else
            {
                SelectedGateFailure = GetGateFailure(gateFailure.ToString()).ToString();
                IsExpanded = true;
            }

            var id = _modelItem.Properties["RetryEntryPointId"].ComputedValue;
            if (id != null && id.ToString() != Guid.Empty.ToString() && Gates.Count > 1)
            {
                var nameValue = Gates.First(o => o.Value == id.ToString());
                SelectedGate = nameValue;
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
            var conditionExpression = _modelItem.Properties["Conditions"].ComputedValue as ConditionExpression;
            if (conditionExpression is null)
            {
                conditionExpression = new ConditionExpression();
            }
            var result = OptionConvertor.Convert(conditionExpression);
            ConditionExpressionOptions = new OptionsWithNotifier { Options = result };
            UpdateConditionExpressionOptionsModelItem();
        }

        private void LoadOptions()
        {
            var gateOptions = _modelItem.Properties["GateOptions"].ComputedValue as GateOptions;
            if (gateOptions != null)
            {
                var result = new List<IOption>();
                var failureOptions = OptionConvertor.Convert(gateOptions);
                result.AddRange(failureOptions);
                Options = new OptionsWithNotifier { Options = result };
            }
            else
            {
                var activeServer = CustomContainer.Get<IShellViewModel>().ActiveServer;
                _server = activeServer;
                _resourceRepository = _server.ResourceRepository;

                Options = new OptionsWithNotifier { Options = _resourceRepository.FindOptionsBy(_server, OptionsService.GateResume) };
            }
            UpdateOptionsModelItem();
        }

        public ICommand DeleteConditionCommand { get; set; }

        public IEnumerable<string> GateFailureOptions => GateOptionsHelper<GateFailureAction>.GetDescriptionsAsList(typeof(GateFailureAction)).ToList();
        public string SelectedGateFailure
        {
            get => _selectedGateFailure;
            set
            {
                var gateFailure = GateFailureOptions.Single(p => p.ToString().Contains(value));
                _selectedGateFailure = gateFailure;
                OnPropertyChanged(nameof(SelectedGateFailure));

                var enumGateFailure = GateOptionsHelper<GateFailureAction>.GetEnumFromDescription(gateFailure);
                _modelItem.Properties["GateFailure"]?.SetValue(enumGateFailure.ToString());
            }
        }

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

        private void UpdateOptionsModelItem()
        {
            if (Options?.Options != null)
            {
                _modelItem.Properties["GateOptions"]?.SetValue(OptionConvertor.Convert(typeof(GateOptions), Options.Options));
                OnPropertyChanged(nameof(Options));
            }
        }

        private static GateFailureAction GetGateFailure(string gateFailure)
        {
            return GateOptionsHelper<GateFailureAction>.GetEnumFromDescription(gateFailure);
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
                _options.OptionChanged += UpdateOptionsModelItem;
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
            Gates = new List<NameValue> { new NameValue { Name = " - Select Gate - ", Value = Guid.Empty.ToString() } };
        }
    }
}
