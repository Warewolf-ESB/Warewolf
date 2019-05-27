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
        string _displayName;
        bool _hasMergeStarted;
        bool _hasWorkflowNameConflict;
        bool _hasVariablesConflict;
        bool _isVariablesEnabled;
        readonly IContextualResourceModel _resourceModel;
        public IConflictModelFactory ModelFactoryCurrent { get; private set; }
        public IConflictModelFactory ModelFactoryDifferent { get; private set; }

        public MergeWorkflowViewModel(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadworkflowFromServer)
        {
            var _serviceDifferenceParser = CustomContainer.Get<IServiceDifferenceParser>();
            UpdateHelpDescriptor(Warewolf.Studio.Resources.Languages.HelpText.MergeWorkflowStartupHelp);

            MergePreviewWorkflowDesignerViewModel = new MergePreviewWorkflowDesignerViewModel(currentResourceModel);

            _resourceModel = currentResourceModel;
            var (currentTree, diffTree) = _serviceDifferenceParser.GetDifferences(currentResourceModel, differenceResourceModel, loadworkflowFromServer);

            MergePreviewWorkflowDesignerViewModel.CreateBlankWorkflow();

            ModelFactoryCurrent = new ConflictModelFactory(currentResourceModel)
            {
                Header = currentResourceModel.ResourceName,
                HeaderVersion = SetHeaderVersion(currentResourceModel)
            };
            ModelFactoryCurrent.SomethingConflictModelChanged += SourceOnConflictModelChanged;
            ModelFactoryDifferent = new ConflictModelFactory(differenceResourceModel)
            {
                Header = differenceResourceModel.ResourceName,
                HeaderVersion = SetHeaderVersion(differenceResourceModel)
            };
            ModelFactoryDifferent.SomethingConflictModelChanged += SourceOnConflictModelChanged;

            var conflictList = new ConflictRowList(ModelFactoryCurrent, ModelFactoryDifferent, currentTree, diffTree);

            Conflicts = conflictList;
            SetupNamesAndVariables(currentResourceModel, differenceResourceModel);

            var stateApplier = new ConflictListStateApplier(ModelFactoryCurrent, conflictList);
            stateApplier.SetConnectorSelectionsToCurrentState();

            var mergePreviewWorkflowStateApplier = new MergePreviewWorkflowStateApplier(conflictList, MergePreviewWorkflowDesignerViewModel);
            mergePreviewWorkflowStateApplier.Apply();
        }

        private static string SetHeaderVersion(IContextualResourceModel resourceModel) => resourceModel.IsVersionResource ? "[v." + resourceModel.VersionInfo.VersionNumber + "]" : "[Current]";

        void SetupNamesAndVariables(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel)
        {
            HasMergeStarted = false;

            HasVariablesConflict = currentResourceModel.DataList != differenceResourceModel.DataList;
            HasWorkflowNameConflict = currentResourceModel.ResourceName != differenceResourceModel.ResourceName;
            IsVariablesEnabled = !HasWorkflowNameConflict && HasVariablesConflict;

            DisplayName = nameof(Merge);

            MergePreviewWorkflowDesignerViewModel.CanViewWorkflowLink = false;
            MergePreviewWorkflowDesignerViewModel.IsTestView = true;
            ModelFactoryCurrent.IsWorkflowNameChecked = !HasWorkflowNameConflict;
            ModelFactoryCurrent.IsVariablesChecked = !HasVariablesConflict;
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
                    var resourceName = ModelFactoryCurrent.IsWorkflowNameChecked ? ModelFactoryCurrent.WorkflowName : ModelFactoryDifferent.WorkflowName;
                    _resourceModel.Environment.ExplorerRepository.UpdateManagerProxy.Rename(resourceId, resourceName);
                }
                if (HasVariablesConflict)
                {
                    _resourceModel.DataList = ModelFactoryCurrent.IsVariablesChecked ? ModelFactoryCurrent.DataListViewModel.WriteToResourceModel() : ModelFactoryDifferent.DataListViewModel.WriteToResourceModel();
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
                var canSave = ModelFactoryCurrent.IsWorkflowNameChecked || ModelFactoryDifferent.IsWorkflowNameChecked;
                canSave &= ModelFactoryCurrent.IsVariablesChecked || ModelFactoryDifferent.IsVariablesChecked;

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
}