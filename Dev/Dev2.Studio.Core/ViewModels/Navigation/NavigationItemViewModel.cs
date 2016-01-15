
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards.Interfaces;

namespace Dev2.Studio.Core.ViewModels.Navigation
{
    public class NavigationItemViewModel : NavigationItemViewModelBase, INavigationItemViewModel
    {
        #region Fields

        private readonly string _environmentConnectedMediatorKey;
        private readonly string _environmentDisconnectedMediatorKey;
        private string _activityFullName;
        private string _displayName;
        private bool _isCategory;
        private bool _isServerLevel;
        private RelayCommand _buildCommand;
        private RelayCommand _connectCommand;
        private RelayCommand _createWizardCommand;
        private RelayCommand _debugCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _deployCommand;
        private RelayCommand _disconnectCommand;
        private RelayCommand _editCommand;
        private RelayCommand _editWizardCommand;
        private RelayCommand _helpCommand;
        private RelayCommand _manualEditCommand;
        private RelayCommand _removeCommand;
        private RelayCommand _runCommand;
        private RelayCommand _showDependenciesCommand;
        private RelayCommand _showPropertiesCommand;

        #endregion

        #region Properties

        [Import]
        public IWizardEngine WizardEngine { get; set; }

        public bool HasExecutableCommands
        {
            get
            {
                return CanConnect || CanDisconnect || CanBuild || CanDebug ||
                       CanEdit || CanRun || CanDelete || CanHelp || CanShowDependencies || CanShowProperties ||
                       CanDeploy ||
                       CanCreateWizard || CanEditWizard;
        }
        }

        #endregion

        #region Methods
        public IEnvironmentModel EnvironmentModel { get; set; }

        public IContextualResourceModel ResourceModelItem
        {
            get { return DataContext as IContextualResourceModel; }
        }

        public bool IsServerLevel
        {
            get { return _isServerLevel; }
            set
            {
                _isServerLevel = value;
                NotifyOfPropertyChange(() => IsServerLevel);
            }
        }

        public bool IsCategory
        {
            get { return _isCategory; }
            set
            {
                _isCategory = value;
                NotifyOfPropertyChange(() => IsCategory);
            }
        }

        public string ActivityFullName
                {
            get { return _activityFullName; }
            set
            {
                _activityFullName = value;
                NotifyOfPropertyChange(() => ActivityFullName);
                }
            }

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        #endregion

        #region Commands

        #region Connect

        public bool CanConnect
        {
            get { return EnvironmentModel != null && !EnvironmentModel.IsConnected && IsServerLevel && !IsCategory; }
        }

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ??
                       (_connectCommand = new RelayCommand(param =>
                        {
                               if (EnvironmentModel.IsConnected) return;

                            EnvironmentModel.Connect();
                               RaisePropertyChangedForCommands();
                           }, o => CanConnect));
            }
        }

        #endregion Connect

        #region Deploy

        public bool CanDeploy
        {
            get { return EnvironmentModel != null && EnvironmentModel.IsConnected; }
        }

        public ICommand DeployCommand
        {
            get
            {
                return _deployCommand ??
                       (_deployCommand =
                        new RelayCommand(param => Mediator.SendMessage(MediatorMessages.DeployResources, this),
                                         o => CanDeploy));
            }
        }

        #endregion Deploy

        #region Disconnect

        public bool CanDisconnect
        {
            get
            {
                return EnvironmentModel != null && EnvironmentModel.EnvironmentConnection != null &&
                       EnvironmentModel.IsConnected && IsServerLevel && !IsCategory;
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                return _disconnectCommand ?? (_disconnectCommand = new RelayCommand(param =>
                        {
                        if (!EnvironmentModel.IsConnected) return;

                            EnvironmentModel.Disconnect();
                        RaisePropertyChangedForCommands();
                    }, param => CanDisconnect));
            }
        }

        #endregion Disconnect

        #region Build

        public bool CanBuild
        {
            get { return ResourceModelItem != null && !IsServerLevel && !IsCategory; }
        }

        public ICommand BuildCommand
        {
            get
            {
                return _buildCommand ?? (_buildCommand = new RelayCommand(param =>
                    {
                        EditCommand.Execute(null);
                        Mediator.SendMessage(MediatorMessages.SaveResource, ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanBuild));
            }
        }

        #endregion Build

        #region Debug

        public bool CanDebug
        {
            get
            {
                return ResourceModelItem != null && ResourceModelItem.ResourceType == enResourceType.WorkflowService &&
                       !IsServerLevel && !IsCategory;
            }
        }

        public ICommand DebugCommand
        {
            get
            {
                return _debugCommand ?? (_debugCommand = new RelayCommand(param =>
                    {
                        EditCommand.Execute(null);
                        Mediator.SendMessage(MediatorMessages.DebugResource, ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanDebug));
            }
        }

        #endregion Debug

        #region Edit

        public bool CanEdit
        {
            get
            {
                return ResourceModelItem != null && !IsServerLevel && !IsCategory &&
                       (ResourceModelItem.ResourceType == enResourceType.WorkflowService ||
                       ResourceModelItem.ResourceType == enResourceType.Service ||
                       ResourceModelItem.ResourceType == enResourceType.Source);
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new RelayCommand(param =>
                        {
                        if (ResourceModelItem == null) return;

                            //Change to only show for resource wizards not system wizards
                        if (WizardEngine.IsResourceWizard(ResourceModelItem))
                            {
                                WizardEngine.EditWizard(ResourceModelItem);
                            }
                            else
                            {
                            switch (ResourceModelItem.ResourceType)
                                {
                                    case enResourceType.WorkflowService:
                                        Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, ResourceModelItem);
                                        break;

                                    case enResourceType.Source:
                                        Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, ResourceModelItem);
                                        break;
                                    case enResourceType.Service:
                                        Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, ResourceModelItem);
                                        break;
                                }
                            }

                        RaisePropertyChangedForCommands();
                    }, param => CanEdit));
            }
        }

        #endregion Edit

        #region ManualEdit

        public bool CanManualEdit
        {
            get
            {
                return ResourceModelItem != null && !IsServerLevel && !IsCategory &&
                       (ResourceModelItem.ResourceType == enResourceType.WorkflowService ||
                       ResourceModelItem.ResourceType == enResourceType.Service ||
                        ResourceModelItem.ResourceType == enResourceType.Source) &&
                       WizardEngine.IsResourceWizard(ResourceModelItem);
            }
        }

        public ICommand ManualEditCommand
        {
            get
            {
                return _manualEditCommand ?? (_manualEditCommand = new RelayCommand(param =>
                {
                        if (ResourceModelItem == null) return;

                        switch (ResourceModelItem.ResourceType)
                    {
                                case enResourceType.WorkflowService:
                                    Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, ResourceModelItem);
                                    break;

                                case enResourceType.Source:
                                    Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, ResourceModelItem);
                                    break;
                                case enResourceType.Service:
                                    Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, ResourceModelItem);
                                    break;
                            }

                        RaisePropertyChangedForCommands();
                    }, param => CanManualEdit));
                        }
                }

        #endregion ManualEdit

        #region Run

        public bool CanRun
        {
            get
            {
                return ResourceModelItem != null && !IsServerLevel && !IsCategory &&
                       ResourceModelItem.ResourceType == enResourceType.WorkflowService &&
                       ResourceModelItem.ResourceType == enResourceType.Service;
            }
        }

        public ICommand RunCommand
        {
            get
            {
                return _runCommand ?? (_runCommand = new RelayCommand(param =>
                {
                        Mediator.SendMessage(MediatorMessages.ExecuteResource, ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanRun));
                }
            }

        #endregion Run

        #region Remove

        public bool CanRemove
        {
            get
            {
                if (IsServerLevel && EnvironmentModel != null && !IsCategory)
                {
                    return EnvironmentModel.ID != Guid.Empty;
                }
                return false;
            }
        }

        public ICommand RemoveCommand
        {
            get { return _removeCommand ?? (_removeCommand = new RelayCommand(Remove_RelayCommand, o => CanRemove)); }
                }

        #endregion Delete

        #region Delete

        public bool CanDelete
        {
            get
            {
                return ResourceModelItem != null && !IsServerLevel && !IsCategory &&
                       (ResourceModelItem.ResourceType == enResourceType.WorkflowService ||
                       ResourceModelItem.ResourceType == enResourceType.Service ||
                       ResourceModelItem.ResourceType == enResourceType.Source);
            }
        }

        public ICommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(Delete_RelayCommand, o => CanDelete)); }
                }

        #endregion Delete

        #region Help

        public bool CanHelp
        {
            get { return ResourceModelItem != null && !IsServerLevel && !IsCategory; }
            }

        public ICommand HelpCommand
        {
            get
            {
                return _helpCommand ?? (_helpCommand = new RelayCommand(param =>
                {
                        Mediator.SendMessage(MediatorMessages.AddHelpDocument, ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanHelp));
                }
            }

        #endregion Help

        #region ShowDependencies

        public bool CanShowDependencies
        {
            get { return ResourceModelItem != null && !IsServerLevel && !IsCategory; }
            }

        public ICommand ShowDependenciesCommand
        {
            get
            {
                return _showDependenciesCommand ?? (_showDependenciesCommand = new RelayCommand(param =>
                {
                        Mediator.SendMessage(MediatorMessages.ShowDependencyGraph, ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanShowDependencies));
                }
            }

        #endregion ShowDependencies

        #region ShowProperties

        public bool CanShowProperties
        {
            get { return ResourceModelItem != null && !IsServerLevel && !IsCategory; }
            }

        public ICommand ShowPropertiesCommand
        {
            get
            {
                return _showPropertiesCommand ?? (_showPropertiesCommand = new RelayCommand(param =>
                {
                        Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanShowProperties));
                }
            }

        #endregion ShowProperties

        #region CreateWizard

        public bool CanCreateWizard
        {
            get
            {
                return ResourceModelItem != null && WizardEngine != null && !IsServerLevel && !IsCategory &&
                       !WizardEngine.IsWizard(ResourceModelItem) &&
                       WizardEngine.GetWizard(ResourceModelItem) == null &&
                       (ResourceModelItem.ResourceType == enResourceType.Service ||
                        ResourceModelItem.ResourceType == enResourceType.WorkflowService);
            }
        }

        public ICommand CreateWizardCommand
        {
            get
            {
                return _createWizardCommand ?? (_createWizardCommand = new RelayCommand(param =>
                {
                        if (ResourceModelItem == null) return;

                            WizardEngine.CreateResourceWizard(ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanCreateWizard));
                        }
                }

        #endregion CreateWizard

        #region EditWizard

        public bool CanEditWizard
        {
            get
            {
                return ResourceModelItem != null && WizardEngine != null && !IsServerLevel && !IsCategory &&
                       !WizardEngine.IsWizard(ResourceModelItem) &&
                       WizardEngine.GetWizard(ResourceModelItem) != null &&
                       (ResourceModelItem.ResourceType == enResourceType.Service ||
                        ResourceModelItem.ResourceType == enResourceType.WorkflowService);
            }
        }

        public ICommand EditWizardCommand
        {
            get
            {
                return _editWizardCommand ?? (_editWizardCommand = new RelayCommand(param =>
                {
                        if (ResourceModelItem == null) return;

                            WizardEngine.EditResourceWizard(ResourceModelItem);
                        RaisePropertyChangedForCommands();
                    }, param => CanEditWizard));
                        }
                }

        #endregion EditWizard

        #endregion

        #region Ctor

        public NavigationItemViewModel(string name,
                                       string iconPath,
                                       NavigationItemViewModelBase parent,
                                       object dataContext,
                                       bool isExpanded = false,
                                       bool isSelected = false,
                                       string activityFullName = null,
                                       bool isServerLevel = false,
                                       bool isCategory = false,
                                       IEnvironmentModel environment = null,
                                       Func<NavigationItemViewModelBase, bool> childCountPredicate = null,
                                       bool isChecked = false)
            : base(name, iconPath, parent, dataContext, isExpanded, isSelected, childCountPredicate, isChecked)
        {
            ActivityFullName = activityFullName;
            IsServerLevel = isServerLevel;
            IsCategory = isCategory;
            EnvironmentModel = environment;
            DisplayName = name;

            _environmentConnectedMediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.EnvironmentConnected,
                                                                                 a => RaisePropertyChangedForCommands());
            _environmentDisconnectedMediatorKey =
                Mediator.RegisterToReceiveMessage(MediatorMessages.EnvironmentDisconnected,
                                                  a => RaisePropertyChangedForCommands());
        }

        #endregion

        #region Methods

        public void RaisePropertyChangedForCommands()
            {
            NotifyOfPropertyChange(() => CanDisconnect);
            NotifyOfPropertyChange(() => CanConnect);
            NotifyOfPropertyChange(() => CanBuild);
            NotifyOfPropertyChange(() => CanDebug);
            NotifyOfPropertyChange(() => CanEdit);
            NotifyOfPropertyChange(() => CanRun);
            NotifyOfPropertyChange(() => CanDelete);
            NotifyOfPropertyChange(() => CanHelp);
            NotifyOfPropertyChange(() => CanShowDependencies);
            NotifyOfPropertyChange(() => CanShowProperties);
            NotifyOfPropertyChange(() => CanDeploy);
            NotifyOfPropertyChange(() => CanCreateWizard);
            NotifyOfPropertyChange(() => CanEditWizard);
            NotifyOfPropertyChange(() => HasExecutableCommands);
            NotifyOfPropertyChange(() => ChildCount);
        }

        public BitmapImage GetImage(string uri)
            {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        private void Remove_RelayCommand(object state)
            {
            if (!IsServerLevel || EnvironmentModel == null || IsCategory) return;

            Mediator.SendMessage(MediatorMessages.RemoveServerFromExplorer, EnvironmentModel);
            RaisePropertyChangedForCommands();
        }

        private void Delete_RelayCommand(object state)
            {
            if (ResourceModelItem == null) return;

            var data = new KeyValuePair<INavigationItemViewModel, IContextualResourceModel>(this, ResourceModelItem);

            switch (ResourceModelItem.ResourceType)
            {
                case enResourceType.Source:
                    Mediator.SendMessage(MediatorMessages.DeleteSourceExplorerResource, data);
                    break;
                case enResourceType.WorkflowService:
                    Mediator.SendMessage(MediatorMessages.DeleteWorkflowExplorerResource, data);
                    break;
                case enResourceType.Service:
                    Mediator.SendMessage(MediatorMessages.DeleteServiceExplorerResource, data);
                    break;
                default:
                    throw new ArgumentException(StringResources.NavigationItemViewModel_Unexpected_Resource, "state");
            }

            RaisePropertyChangedForCommands();
        }

        #endregion Methods

        #region Tear Down

        protected override void OnDispose()
        {
            foreach (NavigationItemViewModel child in Children)
            {
                child.Dispose();
            }

            Mediator.DeRegister(MediatorMessages.EnvironmentConnected, _environmentConnectedMediatorKey);
            Mediator.DeRegister(MediatorMessages.EnvironmentDisconnected, _environmentDisconnectedMediatorKey);
        }

        #endregion Tear Down
    }
}
