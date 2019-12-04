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
using Dev2.Common;
using Dev2.Common.Gates;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Communication;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Dev2.TO;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;
using Warewolf.Service;
using Warewolf.UI;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityCollectionDesignerObservableViewModel<DecisionTO>, INotifyPropertyChanged, IEnabled
    {
        private string _selectedGateFailure;
        List<NameValue> _gates;
        private NameValue _selectedGate;
        private bool _enabled;
        private bool _isExpanded;
        private OptionsWithNotifier _options;
        private IServer _server;
        private IResourceRepository _resourceRepository;
        private string expressionText;
        private readonly ModelItem _modelItem;

        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            _modelItem = modelItem;
            LoadDefaults();
            ClearGates();
            LoadGates();

            PopulateFields();
            LoadOptions();
        }

        private void LoadDefaults()
        {
            AddTitleBarLargeToggle();
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            IsExpanded = false;
            Enabled = true;

            Collection = new ObservableCollection<IDev2TOFn>();
            Collection.CollectionChanged += CollectionCollectionChanged;

            var collection = FindRecsetOptions.FindAllDecision().Select(c => c.HandlesType());
            WhereOptions = new ObservableCollection<string>(collection);
            SearchTypeUpdatedCommand = new DelegateCommand(OnSearchTypeChanged);

            ConfigureDecisionExpression(_modelItem);
            InitializeItems(Tos);
            DeleteCommand = new DelegateCommand(x =>
            {
                DeleteRow(x as DecisionTO);
            });
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_Gate;
        }

        private void PopulateFields()
        {
            var gateFailure = _modelItem.Properties["GateFailure"].ComputedValue;
            if (gateFailure is null)
            {
                SelectedGateFailure = GetGateFailure(GateFailureAction.StopOnError.ToString()).ToString();
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
                UpdateModelItem();
            }
        }

        readonly IList<string> _requiresSearchCriteria = new List<string> { "Doesn't Contain", "Contains", "=", "<> (Not Equal)", "Ends With", "Doesn't Start With", "Doesn't End With", "Starts With", "Is Regex", "Not Regex", ">", "<", "<=", ">=" };

        void ConfigureDecisionExpression(ModelItem modelItem)
        {
            var condition = modelItem;
            var expression = condition.Properties[GlobalConstants.ExpressionPropertyText];
            var defaultStack = DataListConstants.DefaultStack;

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
                var ser = new Dev2JsonSerializer();
                ExpressionText = ser.Serialize(defaultStack);
            }

            var displayName = modelItem.Properties[GlobalConstants.DisplayNamePropertyText];
            if (displayName?.Value != null)
            {
                defaultStack.DisplayText = displayName.Value.ToString();
            }
            Tos = ToObservableCollection();
        }

        ObservableCollection<IDev2TOFn> ToObservableCollection()
        {
            if (!string.IsNullOrWhiteSpace(ExpressionText))
            {
                var val = new StringBuilder(ExpressionText);
                var decisions = DataListUtil.ConvertFromJsonToModel<Dev2DecisionStack>(val);
                if (decisions?.TheStack != null)
                {
                    var collection = decisions.TheStack.Select((a, i) => new DecisionTO(a, i + 1, null, DeleteRow));
                    return new ObservableCollection<IDev2TOFn>(collection);
                }
            }
            return new ObservableCollection<IDev2TOFn> { new DecisionTO() };
        }

        public string ExpressionText 
        { 
            get => expressionText;
            set
            {
                expressionText = value;
                _modelItem.Properties["ExpressionText"]?.SetValue(value);
            }
        }
        public ObservableCollection<IDev2TOFn> Tos
        {
            get => Collection;
            set
            {
                Collection.CollectionChanged -= CollectionCollectionChanged;
                Collection = value;
                Collection.CollectionChanged += CollectionCollectionChanged;
                var stack = SetupTos(Collection);
                ExpressionText = DataListUtil.ConvertModelToJson(stack).ToString();
            }
        }
        static Dev2DecisionStack SetupTos(IEnumerable<IDev2TOFn> dev2TOs)
        {
            var dev2DecisionStack = new Dev2DecisionStack { TheStack = new List<Dev2Decision>() };
            var value = dev2TOs.Select(a => a as DecisionTO);
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
                dev2DecisionStack.TheStack.Add(dev2Decision);
            }
            return dev2DecisionStack;
        }

        static readonly IList<IFindRecsetOptions> Whereoptions = FindRecsetOptions.FindAll();
        public ObservableCollection<string> WhereOptions { get; private set; }
        public ICommand SearchTypeUpdatedCommand { get; private set; }
        public ICommand DeleteCommand { get; set; }

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
            if (!row.Equals(Collection.Last()))
            {
                Collection.Remove(row);
            }
        }
        void OnSearchTypeChanged(object indexObj)
        {
            var index = (int)indexObj;
            try
            {
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
                Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
            }
        }

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

        private void UpdateModelItem()
        {
            _modelItem.Properties["GateOptions"]?.SetValue(OptionConvertor.Convert(typeof(GateOptions), Options.Options));
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


        public OptionsWithNotifier Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged(nameof(Options));
                _options.OptionChanged += UpdateModelItem;
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

        public override string CollectionName => "ResultsCollection";

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(IDev2TOFn mi)
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        public void ClearGates()
        {
            Gates = new List<NameValue> { new NameValue { Name = " - Select Gate - ", Value = Guid.Empty.ToString() } };
        }
    }
}
