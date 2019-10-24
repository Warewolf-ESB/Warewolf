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
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Warewolf.Data.Options.Enums;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityCollectionDesignerObservableViewModel<DecisionTO>, INotifyPropertyChanged
    {
        private string _selectedGateFailure;
        private bool _gateSelectionVisible;
        private string _selectedRetryStrategy;
        private DrawingBrush _gateIcon;

        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            SelectedGateFailure = GetGateFailure(GateFailureAction.StopOnError.ToString()).ToString();
            SelectedRetryStrategy = GetRetryAlgorithm(RetryAlgorithm.NoBackoff.ToString()).ToString();
            Collection = new ObservableCollection<IDev2TOFn>();
            Collection.CollectionChanged += CollectionCollectionChanged;
            var collection = FindRecsetOptions.FindAllDecision().Select(c => c.HandlesType());
            WhereOptions = new ObservableCollection<string>(collection);
            SearchTypeUpdatedCommand = new DelegateCommand(OnSearchTypeChanged);
            DeleteCommand = new DelegateCommand(x =>
            {
                DeleteRow(x as DecisionTO);
            });
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


        public DrawingBrush GateIcon
        {
            get => _gateIcon;
            set
            {
                _gateIcon = value;
                OnPropertyChanged(nameof(GateIcon));
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
                GateSelectionVisible = GetGateFailure(_selectedGateFailure) == GateFailureAction.Retry;
                UpdateGateIcon();
                OnPropertyChanged(nameof(SelectedGateFailure));
            }
        }

        private void UpdateGateIcon()
        {
            string icon = GateSelectionVisible ? "ControlFlow-Gate-Open-Icon" : "ControlFlow-Gate-Icon";
            if (Application.Current != null)
            {
                GateIcon = Application.Current.Resources[icon] as DrawingBrush;
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

        public bool GateSelectionVisible
        {
            get => _gateSelectionVisible;
            set
            {
                _gateSelectionVisible = value;
                OnPropertyChanged(nameof(GateSelectionVisible));
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
    }
}
