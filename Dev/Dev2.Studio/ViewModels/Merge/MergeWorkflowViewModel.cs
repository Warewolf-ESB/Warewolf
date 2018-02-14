/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Studio.Interfaces.DataList;
using Dev2.ViewModels.Merge.Utils;

namespace Dev2.ViewModels.Merge
{
    public class MergeWorkflowViewModel : BindableBase, IMergeWorkflowViewModel
    {
        readonly IServiceDifferenceParser _serviceDifferenceParser;
        string _displayName;
        bool _hasMergeStarted;
        bool _hasWorkflowNameConflict;
        bool _hasVariablesConflict;
        bool _isVariablesEnabled;
        readonly IContextualResourceModel _resourceModel;
        readonly ConflictRowList conflictList;
        readonly IConflictModelFactory modelFactoryCurrent;
        readonly IConflictModelFactory modelFactoryDifferent;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadworkflowFromServer)
        {
            _serviceDifferenceParser = CustomContainer.Get<IServiceDifferenceParser>();
            UpdateHelpDescriptor(Warewolf.Studio.Resources.Languages.HelpText.MergeWorkflowStartupHelp);

            MergePreviewWorkflowDesignerViewModel = new MergePreviewWorkflowDesignerViewModel(currentResourceModel);
            MergePreviewWorkflowDesignerViewModel.CreateBlankWorkflow();

            _resourceModel = currentResourceModel;
            var (currentTree, diffTree) = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, loadworkflowFromServer);

            modelFactoryCurrent = new ConflictModelFactory(currentResourceModel);
            modelFactoryCurrent.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            modelFactoryDifferent = new ConflictModelFactory(differenceResourceModel);
            modelFactoryDifferent.SomethingConflictModelChanged += SourceOnConflictModelChanged;

            conflictList = new ConflictRowList(modelFactoryCurrent, modelFactoryDifferent, currentTree, diffTree);

            Conflicts = conflictList;
            SetupNamesAndVariables(currentResourceModel, differenceResourceModel);

            var stateApplier = new ConflictListStateApplier(conflictList);
            stateApplier.SetConnectorSelectionsToCurrentState();

            var mergePreviewWorkflowStateApplier = new MergePreviewWorkflowStateApplier(conflictList, MergePreviewWorkflowDesignerViewModel);
        }

        void SetupNamesAndVariables(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel)
        {
            HasMergeStarted = false;

            HasVariablesConflict = currentResourceModel.DataList != differenceResourceModel.DataList;
            HasWorkflowNameConflict = currentResourceModel.ResourceName != differenceResourceModel.ResourceName;
            IsVariablesEnabled = !HasWorkflowNameConflict && HasVariablesConflict;

            DisplayName = nameof(Merge);

            MergePreviewWorkflowDesignerViewModel.CanViewWorkflowLink = false;
            MergePreviewWorkflowDesignerViewModel.IsTestView = true;
            modelFactoryCurrent.IsWorkflowNameChecked = !HasWorkflowNameConflict;
            modelFactoryCurrent.IsVariablesChecked = !HasVariablesConflict;
        }

        void SourceOnConflictModelChanged(object sender, IConflictModelFactory conflictModel)
        {
            try
            {
                if (!HasMergeStarted)
                {
                    HasMergeStarted = conflictModel.IsWorkflowNameChecked || conflictModel.IsVariablesChecked;
                }
                if (!conflictModel.IsVariablesChecked)
                {
                    return;
                }

                IsVariablesEnabled = HasVariablesConflict;
                if (conflictModel.IsVariablesChecked)
                {
                    DataListViewModel = conflictModel.DataListViewModel;
                }
                var completeConflict = Conflicts.First() as IToolConflictRow;
                completeConflict.ContainsStart = true;
                if (completeConflict.IsChecked)
                {
                    return;
                }
                completeConflict.CurrentViewModel.IsChecked = !completeConflict.HasConflict;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
        }

        IDataListViewModel _dataListViewModel;

        public IDataListViewModel DataListViewModel
        {
            get => _dataListViewModel;
            set
            {
                _dataListViewModel = value;
                OnPropertyChanged(nameof(DataListViewModel));
            }
        }

        public IEnumerable<IConflictRow> Conflicts { get; set; }

        public IMergePreviewWorkflowDesignerViewModel MergePreviewWorkflowDesignerViewModel { get; set; }

        public void Save()
        {
            try
            {
                var resourceId = _resourceModel.ID;
                if (HasWorkflowNameConflict)
                {
                    var resourceName = modelFactoryCurrent.IsWorkflowNameChecked ? modelFactoryCurrent.WorkflowName : modelFactoryDifferent.WorkflowName;
                    _resourceModel.Environment.ExplorerRepository.UpdateManagerProxy.Rename(resourceId, resourceName);
                }
                if (HasVariablesConflict)
                {
                    _resourceModel.DataList = modelFactoryCurrent.IsVariablesChecked ? modelFactoryCurrent.DataListViewModel.WriteToResourceModel() : modelFactoryDifferent.DataListViewModel.WriteToResourceModel();
                }
                _resourceModel.WorkflowXaml = MergePreviewWorkflowDesignerViewModel.ServiceDefinition;
                _resourceModel.Environment.ResourceRepository.SaveToServer(_resourceModel, "Merge");

                HasMergeStarted = false;

                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                var environmentID = _resourceModel.Environment.EnvironmentID;
                mainViewModel?.CloseResource(resourceId, environmentID);
                mainViewModel?.CloseResourceMergeView(resourceId, _resourceModel.ServerID, environmentID);
                mainViewModel?.OpenCurrentVersion(resourceId, environmentID);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, ex.Message);
            }
            finally
            {
                SetDisplayName(IsDirty);
            }
        }

        void SetDisplayName(bool isDirty)
        {
            if (isDirty)
            {
                if (!DisplayName.EndsWith(" *", StringComparison.InvariantCultureIgnoreCase))
                {
                    DisplayName += " *";
                }
            }
            else
            {
                DisplayName = _displayName.Replace("*", "").TrimEnd(' ');
            }
        }
        public bool CanSave
        {
            get
            {
                var canSave = modelFactoryCurrent.IsWorkflowNameChecked || modelFactoryDifferent.IsWorkflowNameChecked;
                canSave &= modelFactoryCurrent.IsVariablesChecked || modelFactoryDifferent.IsVariablesChecked;

                return canSave;
            }
        }

        public bool IsDirty => HasMergeStarted;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

        public bool HasMergeStarted
        {
            get => _hasMergeStarted;
            set
            {
                _hasMergeStarted = value && Conflicts.Any(a => a.HasConflict);
                if (_hasMergeStarted)
                {
                    SetDisplayName(_hasMergeStarted);
                }
                OnPropertyChanged(() => HasMergeStarted);
            }
        }

        public bool HasWorkflowNameConflict
        {
            get => _hasWorkflowNameConflict;
            set
            {
                _hasWorkflowNameConflict = value;
                OnPropertyChanged(() => HasWorkflowNameConflict);
            }
        }

        public bool HasVariablesConflict
        {
            get => _hasVariablesConflict;
            set
            {
                _hasVariablesConflict = value;
                OnPropertyChanged(() => HasVariablesConflict);
            }
        }

        public bool IsVariablesEnabled
        {
            get => _isVariablesEnabled;
            set
            {
                _isVariablesEnabled = value;
                OnPropertyChanged(() => IsVariablesEnabled);
            }
        }

        public void Dispose()
        {
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
    /// <summary>
    /// Manage the selection of items in a
    /// list of IConflictRow entries.
    /// </summary>
    public class ConflictListStateApplier
    {
        readonly ConflictRowList conflicts;
        // TODO: this state applier should also be used to disable Connectors that are unavailable
        public ConflictListStateApplier(ConflictRowList conflicts)
        {
            this.conflicts = conflicts;
            RegisterEventHandlerForConflictItemChanges();
        }
        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var conflict in conflicts)
            {
                if (conflict is IConflictCheckable check)
                {
                    check.IsCurrentChecked = true;
                    // TODO: Verify UI updates based on HasConflicts - every 10 seconds?
                }
            }
        }

        // NEW
        public void RegisterEventHandlerForConflictItemChanges()
        {
            foreach (var conflict in conflicts)
            {
                var innerConflictRow = conflict;

                innerConflictRow.Current.NotifyIsCheckedChanged += (current, isChecked) =>
                {
                    Handler(current, innerConflictRow.Different);
                };
                innerConflictRow.Different.NotifyIsCheckedChanged += (diff, isChecked) =>
                {
                    Handler(innerConflictRow.Current, diff);
                };
            }
        }

        private void Handler(IConflictItem currentItem, IConflictItem diffItem)
        {
            // apply tool or connector state to list view
            // e.g. disable conflicting connectors

        }
    }
    /// <summary>
    /// Apply selection state from a list of IConflict items
    /// to the Merge Preview Workflow
    /// </summary>
    public class MergePreviewWorkflowStateApplier
    {
        readonly IMergePreviewWorkflowDesignerViewModel mergePreviewWorkflowDesignerViewModel;
        readonly ConflictRowList conflictList;
        public MergePreviewWorkflowStateApplier(ConflictRowList conflictList, IMergePreviewWorkflowDesignerViewModel mergePreviewWorkflowDesignerViewModel)
        {
            this.mergePreviewWorkflowDesignerViewModel = mergePreviewWorkflowDesignerViewModel;
            this.conflictList = conflictList;
            RegisterEventHandlerForConflictItemChanges();
            Apply();
        }

        public void RegisterEventHandlerForConflictItemChanges()
        {
            foreach (var conflictRow in conflictList)
            {
                var innerConflictRow = conflictRow;

                conflictRow.Current.NotifyIsCheckedChanged += (current, isChecked) =>
                {
                    Handler(current, innerConflictRow);
                };
                conflictRow.Different.NotifyIsCheckedChanged += (diff, isChecked) =>
                {
                    Handler(diff, innerConflictRow);
                };
            }
        }

        public void Apply()
        {
            foreach (var conflictRow in conflictList)
            {
                Handler(conflictRow.Current, conflictRow);
            }
        }

        private void Handler(IConflictItem changedItem, IConflictRow row)
        {
            if (changedItem is IToolConflictItem toolItem)
            {
                if (row.ContainsStart)
                {
                    return;
                }
                ToolModelHandler(toolItem);
            }
            else if (changedItem is IConnectorConflictItem connectorItem && row is IConnectorConflictRow connectorRow)
            {
                ConnectorHandler(connectorItem, connectorRow);
            }
            else
            {
                throw new NotImplementedException("Only ConflictRow and ConflictItem are supported");
            }
        }
        private void ToolModelHandler(IToolConflictItem changedItem)
        {
            if (changedItem.IsChecked)
            {
                AddActivity(changedItem);
            }
            else
            {
                RemoveActivity(changedItem);
            }
        }

        private void AddActivity(IToolConflictItem toolModelConflictItem)
        {
            mergePreviewWorkflowDesignerViewModel.AddItem(toolModelConflictItem);
        }

        private void RemoveActivity(IToolConflictItem toolModelConflictItem)
        {
            mergePreviewWorkflowDesignerViewModel.RemoveItem(toolModelConflictItem);
        }

        private void ConnectorHandler(IConnectorConflictItem changedItem, IConnectorConflictRow row)
        {
            if (row.ContainsStart)
            {
                mergePreviewWorkflowDesignerViewModel.RemoveStartNodeConnection();
                LinkStartNode(row);
                return;
            }
            if (changedItem.IsChecked)
            {
                AddAndLinkActivity(changedItem, row);
            }
            else
            {
                DeLinkActivities(changedItem.SourceUniqueId, changedItem.DestinationUniqueId, changedItem.Key);
            }
        }

        private void LinkStartNode(IConnectorConflictRow row)
        {
            var startToolRow = conflictList.GetStartToolRow();

            var toolConflictItem = row.Different.IsChecked ? startToolRow.DiffViewModel : startToolRow.CurrentViewModel;

            toolConflictItem.SetAutoChecked();
            AddActivity(toolConflictItem);

            mergePreviewWorkflowDesignerViewModel.LinkStartNode(toolConflictItem);
        }

        private void AddAndLinkActivity(IConnectorConflictItem changedItem, IConnectorConflictRow row)
        {
            var isCurrent = changedItem.DestinationUniqueId.Equals(row.CurrentArmConnector.DestinationUniqueId);

            var toolConflictItem = conflictList.GetToolItemFromId(changedItem.DestinationUniqueId, isCurrent);
            toolConflictItem.SetAutoChecked();
            AddActivity(toolConflictItem);

            LinkActivities(changedItem.SourceUniqueId, changedItem.DestinationUniqueId, changedItem.Key);
        }

        private void LinkActivities(Guid SourceUniqueId, Guid DestinationUniqueId, string Key)
        {
            mergePreviewWorkflowDesignerViewModel?.LinkActivities(SourceUniqueId, DestinationUniqueId, Key);
        }

        private void DeLinkActivities(Guid SourceUniqueId, Guid DestinationUniqueId, string Key)
        {
            mergePreviewWorkflowDesignerViewModel?.DeLinkActivities(SourceUniqueId, DestinationUniqueId, Key);
        }
    }
}