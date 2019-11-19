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
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Interfaces;
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
using System.Windows;
using System.Windows.Input;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;
using Warewolf.Service;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityCollectionDesignerObservableViewModel<DecisionTO>, INotifyPropertyChanged, IEnabled
    {
        private string _selectedGateFailure;
        List<(string uniqueId, string activityName)> _gates;
        List<string> _gatesView;
        private string _selectedGate;
        private bool _enabled;
        private string _selectedRetryStrategy;
        private IEnumerable<IOption> _options;
        private IServer _server;
        private IResourceRepository _resourceRepository;

        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            SelectedGateFailure = GetGateFailure(GateFailureAction.StopOnError.ToString()).ToString();
            SelectedRetryStrategy = GetRetryAlgorithm(RetryAlgorithm.NoBackoff.ToString()).ToString();
            Collection = new ObservableCollection<IDev2TOFn>();
            Collection.CollectionChanged += CollectionCollectionChanged;

            LoadDummyDataThatShouldBePopulatedWhenTheActivityIsDone();

            var collection = FindRecsetOptions.FindAllDecision().Select(c => c.HandlesType());
            WhereOptions = new ObservableCollection<string>(collection);
            SearchTypeUpdatedCommand = new DelegateCommand(OnSearchTypeChanged);
            DeleteCommand = new DelegateCommand(x =>
            {
                DeleteRow(x as DecisionTO);
            });

            LoadOptions();
            ClearGates();
            LoadGates(modelItem);
        }

        private void LoadGates(ModelItem modelItem)
        {
            var designerView = FindDependencyParent.FindParent<System.Activities.Presentation.View.DesignerView>(modelItem.View);

            if (designerView != null && designerView.DataContext is IWorkflowDesignerViewModel workflowDesignerViewModel)
            {
                Gates = workflowDesignerViewModel.GetGates(modelItem.Properties["UniqueID"].ComputedValue.ToString());
            }
        }

        private void LoadOptions()
        {
            var activeServer = CustomContainer.Get<IShellViewModel>().ActiveServer;
            _server = activeServer;
            _resourceRepository = _server.ResourceRepository;

            Options = _resourceRepository.FindOptionsBy(_server, OptionsService.GateResume);
        }

        private void LoadDummyDataThatShouldBePopulatedWhenTheActivityIsDone()
        {
            var decisionTo = new DecisionTO
            {
                MatchValue = "[[name]]",
                SearchType = "=",
                SearchCriteria = "bob"
            };
            var decisionTo1 = new DecisionTO
            {
                MatchValue = "[[name1]]",
                SearchType = "=",
                SearchCriteria = "bob1"
            };
            var decisionTo2 = new DecisionTO
            {
                MatchValue = "[[name2]]",
                SearchType = "=",
                SearchCriteria = "bob2"
            };
            Collection.Add(decisionTo);
            Collection.Add(decisionTo1);
            Collection.Add(decisionTo2);
        }

        readonly IList<string> _requiresSearchCriteria = new List<string> { "Doesn't Contain", "Contains", "=", "<> (Not Equal)", "Ends With", "Doesn't Start With", "Doesn't End With", "Starts With", "Is Regex", "Not Regex", ">", "<", "<=", ">=" };
        public ObservableCollection<IDev2TOFn> Tos
        {
            get => Collection;
            set
            {
                Collection.CollectionChanged -= CollectionCollectionChanged;
                Collection = value;
                Collection.CollectionChanged += CollectionCollectionChanged;
            }
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
                Enabled = GetGateFailure(_selectedGateFailure) == GateFailureAction.Retry;
                OnPropertyChanged(nameof(SelectedGateFailure));
            }
        }

        public List<(string uniqueId, string activityName)> Gates
        {
            get => _gates;
            set
            {
                _gates = value;
                foreach (var (uniqueId, activityName) in Gates)
                {
                    GatesView.Add(activityName);
                }
                OnPropertyChanged(nameof(Gates));
            }
        }

        public List<string> GatesView
        {
            get => _gatesView;
            set
            {
                _gatesView = value;
                OnPropertyChanged(nameof(GatesView));
            }
        }

        public string SelectedGate
        {
            get => _selectedGate;
            set
            {
                var (uniqueId, activityName) = Gates.Single(o => o.ToString().Contains(value));
                _selectedGate = activityName;
                OnPropertyChanged(nameof(SelectedGate));
            }
        }

        private static GateFailureAction GetGateFailure(string gateFailure)
        {
            return GateOptionsHelper<GateFailureAction>.GetEnumFromDescription(gateFailure);
        }

        public IEnumerable<string> GateRetryStrategies => GateOptionsHelper<RetryAlgorithm>.GetDescriptionsAsList(typeof(RetryAlgorithm)).ToList();
        public string SelectedRetryStrategy
        {
            get => _selectedRetryStrategy;
            set
            {
                _selectedRetryStrategy = value;
                GetRetryAlgorithm(_selectedRetryStrategy);
                OnPropertyChanged(nameof(SelectedRetryStrategy));
            }
        }

        private static RetryAlgorithm GetRetryAlgorithm(string retryAlgorithm)
        {
            return GateOptionsHelper<RetryAlgorithm>.GetEnumFromDescription(retryAlgorithm);
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


        public IEnumerable<IOption> Options
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
            Gates = new List<(string uniqueId, string activityName)>();
            GatesView = new List<string>();
        }
    }
}
