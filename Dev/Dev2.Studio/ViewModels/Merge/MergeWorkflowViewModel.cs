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
        readonly ToolModelConflictRowList conflictList;

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadworkflowFromServer)
        {
            _serviceDifferenceParser = CustomContainer.Get<IServiceDifferenceParser>();
            UpdateHelpDescriptor(Warewolf.Studio.Resources.Languages.HelpText.MergeWorkflowStartupHelp);

            WorkflowDesignerViewModel = new WorkflowDesignerViewModel(currentResourceModel, false);
            WorkflowDesignerViewModel.CreateBlankWorkflow();

            _resourceModel = currentResourceModel;
            var (currentTree, diffTree) = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, loadworkflowFromServer);

            // NEW
            var modelFactoryCurrent = new ConflictModelFactory(currentResourceModel, WorkflowDesignerViewModel);
            var modelFactoryDiff = new ConflictModelFactory(differenceResourceModel, WorkflowDesignerViewModel);

            conflictList = new ToolModelConflictRowList(modelFactoryCurrent, modelFactoryDiff, currentTree, diffTree);

            Conflicts = new LinkedList<IConflictRow>();
            var firstConflict = Conflicts.FirstOrDefault();
            SetupBindings(currentResourceModel, differenceResourceModel, firstConflict as IToolConflictRow);

            var stateApplier = new ConflictListStateApplier(conflictList);
            stateApplier.SetConnectorSelectionsToCurrentState();

            var mergePreviewWorkflowStateApplier = new MergePreviewWorkflowStateApplier(conflictList);
        }

        void SetupBindings(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, IToolConflictRow firstConflict)
        {
            if (CurrentConflictModel == null)
            {
                CurrentConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.CurrentViewModel ?? new ToolModelConflictItem
                    {
                        WorkflowDesignerViewModel = WorkflowDesignerViewModel
                    },
                    WorkflowName = currentResourceModel.ResourceName,
                    ServerName = currentResourceModel.Environment.Name
                };

                CurrentConflictModel.GetDataList(currentResourceModel);
                CurrentConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            if (DifferenceConflictModel == null)
            {
                DifferenceConflictModel = new ConflictModelFactory
                {
                    Model = firstConflict?.DiffViewModel ?? new ToolModelConflictItem
                    {
                        WorkflowDesignerViewModel = WorkflowDesignerViewModel
                    },
                    WorkflowName = differenceResourceModel.ResourceName,
                    ServerName = differenceResourceModel.Environment.Name
                };
                DifferenceConflictModel.GetDataList(differenceResourceModel);
                DifferenceConflictModel.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            }

            HasMergeStarted = false;

            HasVariablesConflict = currentResourceModel.DataList != differenceResourceModel.DataList;
            HasWorkflowNameConflict = currentResourceModel.ResourceName != differenceResourceModel.ResourceName;
            IsVariablesEnabled = !HasWorkflowNameConflict && HasVariablesConflict;

            DisplayName = nameof(Merge);

            WorkflowDesignerViewModel.CanViewWorkflowLink = false;
            WorkflowDesignerViewModel.IsTestView = true;
            CurrentConflictModel.IsWorkflowNameChecked = !HasWorkflowNameConflict;
            CurrentConflictModel.IsVariablesChecked = !HasVariablesConflict;
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
                var completeConflict = Conflicts.First.Value as IToolConflictRow;
                completeConflict.IsStartNode = true;
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

        void RemovePreviousToolArm(IConflictRow container)
        {
            var updateNextConflict = conflictList.GetNextConlictToUpdate(container);
            if (updateNextConflict != null)
            {
                if (updateNextConflict is IConnectorConflictRow updateNextArmConflict)
                {
                    ResetToolArmEvents(updateNextArmConflict);
                }
                else
                {
                    RemovePreviousToolArm(updateNextConflict);
                }
            }
        }

        void ResetToolArmEvents(IConnectorConflictRow connectorConflict)
        {
            // TODO: remove this because we can do it from the state applier using just .NextItem
            var index = conflictList.IndexOf(connectorConflict);
            if (index == -1 || index > conflictList.Count)
            {
                return;
            }

            var restOfItems = conflictList.Skip(index).ToList();
            foreach (var conf in restOfItems)
            {
                if (conf is IConnectorConflictRow tool)
                {
                    if (tool.HasConflict)
                    {
                        tool.IsChecked = false;
                    }
                    else
                    {
                        tool.CurrentArmConnector.IsChecked = true;
                    }
                }
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

        public LinkedList<IConflictRow> Conflicts { get; set; }

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public IConflictModelFactory CurrentConflictModel { get; set; }
        public IConflictModelFactory DifferenceConflictModel { get; set; }

        public void Save()
        {
            try
            {
                var resourceId = _resourceModel.ID;
                if (HasWorkflowNameConflict)
                {
                    var resourceName = CurrentConflictModel.IsWorkflowNameChecked ? CurrentConflictModel.WorkflowName : DifferenceConflictModel.WorkflowName;
                    _resourceModel.Environment.ExplorerRepository.UpdateManagerProxy.Rename(resourceId, resourceName);
                }
                if (HasVariablesConflict)
                {
                    _resourceModel.DataList = CurrentConflictModel.IsVariablesChecked ? CurrentConflictModel.DataListViewModel.WriteToResourceModel() : DifferenceConflictModel.DataListViewModel.WriteToResourceModel();
                }
                _resourceModel.WorkflowXaml = WorkflowDesignerViewModel.ServiceDefinition;
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
                var canSave = CurrentConflictModel.IsWorkflowNameChecked || DifferenceConflictModel.IsWorkflowNameChecked;
                canSave &= CurrentConflictModel.IsVariablesChecked || DifferenceConflictModel.IsVariablesChecked;

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
        readonly ToolModelConflictRowList conflicts;
        // TODO: this state applier should also be used to disable Connectors that are unavailable
        public ConflictListStateApplier(ToolModelConflictRowList conflicts)
        {
            this.conflicts = conflicts;
        }
        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var conflict in conflicts)
            {
                if (conflict is IConflictCheckable check)
                {
                    check.IsCurrentChecked = true;
                }
            }
        }
    }
    /// <summary>
    /// Apply selection state from a list of IConflict items
    /// to the Merge Preview Workflow
    /// </summary>
    public class MergePreviewWorkflowStateApplier
    {
        public MergePreviewWorkflowStateApplier(ToolModelConflictRowList conflictList)
        {
            RegisterEventHandler(conflictList);
        }

        public void RegisterEventHandler(ToolModelConflictRowList conflictList)
        {
            foreach (var conflictRow in conflictList)
            {
                var innerConflictRow = conflictRow;

                conflictRow.Current.NotifyIsCheckedChanged += (current, isChecked) =>
                {
                    Handler(current, innerConflictRow.Different);
                };
                conflictRow.Different.NotifyIsCheckedChanged += (diff, isChecked) =>
                {
                    Handler(innerConflictRow.Current, diff);
                };
            }
        }

        private void Handler(IConflictItem currentItem, IConflictItem diffItem)
        {
            // apply tool or connector state to workflow
            // DelinkActivities, LinkActivities, AddActivity, RemoveActivity all should be here?
        }
    }
}