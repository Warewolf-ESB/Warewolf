
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.ConnectionHelpers;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Threading;
using Dev2.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Models
{
    public class ExplorerItemModel : ObservableObject, IExplorerItemModel
    {
        #region Fields
        ICommand _editCommand;
        ICommand _deployCommand;
        ICommand _renameCommand;
        ICommand _toggleVersionHistoryCommand;
        ICommand _rollbackCommand;
        ICommand _deleteVersionCommand;
        ICommand _deleteCommand;
        ICommand _removeCommand;
        ICommand _showDependenciesCommand;
        ICommand _debugCommand;
        ICommand _connectCommand;
        ICommand _disconnectCommand;

        bool _isExplorerExpanded;
        bool _isResourcePickerExpanded;
        bool _isDeploySourceExpanded;
        bool _isDeployTargetExpanded;
        bool _isAuthorized;
        bool _isExplorerSelected;
        bool _isResourcePickerSelected;
        bool _isDeploySourceSelected;
        bool _isDeployTargetSelected;
        bool _isConnected;
        private bool? _isChecked = false;
        private bool _isRenaming;
        private string _displayName;
        private readonly IStudioResourceRepository _studioResourceRepository;
        private static bool _serverRefreshing;
        ObservableCollection<IExplorerItemModel> _children;
        ICommand _refreshCommand;
        private Permissions _permissions;
        bool _isRefreshing;
        ICommand _newFolderCommand;
        readonly IAsyncWorker _asyncWorker;
        readonly IConnectControlSingleton _connectControlSingleton;
        bool _isOverwrite;

        private readonly Dictionary<ResourceType, Type> _activityNames;
        IVersionInfo _versionInfo;
        string _toggleVersionHistoryHeader;

        #endregion


        public ExplorerItemModel()
            : this(ConnectControlSingleton.Instance, StudioResourceRepository.Instance)
        {

        }

        public ExplorerItemModel(IConnectControlSingleton connectControlSingleton, IStudioResourceRepository studioResourceRepository)
        {
            Children = new ObservableCollection<IExplorerItemModel>();
            _isAuthorized = true;
            _isConnected = true;
            _studioResourceRepository = studioResourceRepository;
            Children.CollectionChanged -= ChildrenCollectionChanged;
            Children.CollectionChanged += ChildrenCollectionChanged;
            _asyncWorker = new AsyncWorker();
            _activityNames = new Dictionary<ResourceType, Type>
                {
                    {
                        ResourceType.DbService, typeof(DsfDatabaseActivity) 
                    },
                    {
                        ResourceType.PluginService, typeof(DsfPluginActivity) 
                    },
                    {
                        ResourceType.WebService, typeof(DsfWebserviceActivity) 
                    }
                };
            _connectControlSingleton = connectControlSingleton;
            _connectControlSingleton.ConnectedStatusChanged += ConnectedStatusChanged;
            ToggleVersionHistoryHeader = "Show Version History";
        }

        private void ConnectedStatusChanged(object sender, ConnectionStatusChangedEventArg e)
        {
            if(EnvironmentId == e.EnvironmentId && ResourceType == ResourceType.Server)
            {
                IsRefreshing = _serverRefreshing = e.ConnectedStatus == ConnectionEnumerations.ConnectedState.Busy;
            }
        }

        void ChildrenCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnChildrenChanged();
        }

        public ExplorerItemModel(IStudioResourceRepository studioResourceRepository, IAsyncWorker asyncWorker, IConnectControlSingleton connectControlSingleton)
            : this(connectControlSingleton, studioResourceRepository)
        {
            _studioResourceRepository = studioResourceRepository;
            _asyncWorker = asyncWorker;
            _connectControlSingleton = connectControlSingleton;
        }

        #region Properties

        public bool CanCreateNewFolder
        {
            get
            {
                if((ResourceType == ResourceType.Folder || ResourceType == ResourceType.Server) && (Permissions.HasFlag(Permissions.Contribute) || Permissions.HasFlag(Permissions.Administrator)))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsNew { get; set; }

        public bool CanSelectDependencies
        {
            get
            {
                return CanShowDependencies;
            }
        }

        public bool IsAuthorizedDeployFrom
        {
            get
            {
                if(ResourceType == ResourceType.Server)
                {
                    return Permissions.HasFlag(Permissions.Administrator) || Permissions.HasFlag(Permissions.DeployFrom);
                }
                return true;
            }
        }

        public bool IsLocalHost
        {
            get
            {
                if(ResourceType == ResourceType.Server && EnvironmentId == new Guid())
                {
                    return true;
                }
                return false;
            }
        }

        public int ChildrenCount
        {
            get
            {
                return GetChildrenCount();
            }
        }


        public string ActivityName
        {
            get
            {

                return (_activityNames.ContainsKey(ResourceType) ? _activityNames[ResourceType] : typeof(DsfActivity)).AssemblyQualifiedName;

            }
        }
        public void RemoveChild(IExplorerItemModel child)
        {
            Children.Remove(child);
            OnChildrenChanged();
        }
        public Guid EnvironmentId { get; set; }
        public IExplorerItemModel Parent { get; set; }
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                bool renamed = true;
                if(!String.IsNullOrEmpty(_displayName)
                    && ResourceType != ResourceType.Server
                    && Children.All(model => model.DisplayName != value))
                {
                    renamed= Rename(value);
                }
               if(renamed)  _displayName = value;
                IsRenaming = false;
                OnPropertyChanged();
            }
        }

        public void SetDisplay(string display)
        {
            _displayName = display;
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("DisplayName");
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public Guid ResourceId { get; set; }
        public ResourceType ResourceType { get; set; }

        public ObservableCollection<IExplorerItemModel> Children
        {
            get
            {
                return _children;
            }
            set
            {
                _children = value;
                OnPropertyChanged();
            }
        }

        public List<IExplorerItemModel> DeployChildren
        {
            get
            {
                return _children.Where(c => c.ResourceType != ResourceType.Version && c.ResourceType != ResourceType.Message).ToList();
            }
        }

        public Permissions Permissions
        {
            get { return _permissions; }
            set
            {
                if(value != _permissions)
                {

                    if(_permissions != value)
                    {
                        _permissions = value;
                        IsAuthorized = !((_permissions == Permissions.None) && this.Descendants().All(model => model.Permissions == Permissions.None));

                        OnPropertyChanged();
                        // ReSharper disable ExplicitCallerInfoArgument
                        OnPropertyChanged("CanEdit");
                        OnPropertyChanged("CanAddResoure");
                        OnPropertyChanged("CanCreateNewFolder");
                        OnPropertyChanged("CanDelete");
                        OnPropertyChanged("CanDeploy");
                        OnPropertyChanged("CanRename");
                        OnPropertyChanged("CanExecute");
                        OnPropertyChanged("IsAuthorizedDeployTo");
                        OnPropertyChanged("IsAuthorizedDeployFrom");
                        OnPropertyChanged("CanExecute");
                        OnPropertyChanged("IsAuthorized");
                        // ReSharper restore ExplicitCallerInfoArgument
                    }
                }

            }
        }

        public bool IsExplorerExpanded
        {
            get
            {
                return _isExplorerExpanded;
            }
            set
            {
                _isExplorerExpanded = value;
                OnPropertyChanged();
            }
        }
        public bool IsResourcePickerExpanded
        {
            get
            {
                return _isResourcePickerExpanded;
            }
            set
            {
                _isResourcePickerExpanded = value;
                OnPropertyChanged();
            }
        }
        public bool IsDeploySourceExpanded
        {
            get
            {
                return _isDeploySourceExpanded;
            }
            set
            {
                _isDeploySourceExpanded = value;
                OnPropertyChanged();
            }
        }
        public bool IsDeployTargetExpanded
        {
            get
            {
                return _isDeployTargetExpanded;
            }
            set
            {
                _isDeployTargetExpanded = value;
                OnPropertyChanged();
            }
        }
        public bool IsExplorerSelected
        {
            get
            {
                return _isExplorerSelected;
            }
            set
            {
                _isExplorerSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsResourcePickerSelected
        {
            get
            {
                return _isResourcePickerSelected;
            }
            set
            {
                _isResourcePickerSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsDeploySourceSelected
        {
            get
            {
                return _isDeploySourceSelected;
            }
            set
            {
                _isDeploySourceSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsDeployTargetSelected
        {
            get
            {
                return _isDeployTargetSelected;
            }
            set
            {
                _isDeployTargetSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsConnected
        {
            get
            {
                if(ResourceType != ResourceType.Server)
                {
                    return true;
                }
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
        public bool IsRenaming
        {
            get
            {
                return _isRenaming;
            }
            set
            {
                _isRenaming = value;
                if(_isRenaming && OnRenameChangedAction != null)
                {
                    OnRenameChangedAction(true);
                }
                OnPropertyChanged();
            }
        }
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public IVersionInfo VersionInfo
        {
            get
            {
                return _versionInfo;
            }
            set
            {
                _versionInfo = value;
                OnPropertyChanged();
            }
        }

        public string DisplayNameValidationRegex
        {
            get
            {
                return Common.ExtMethods.StringExtension.IsValidResourcename.ToString();
            }
        }
        public bool CanAddResoure
        {
            get
            {
                if((ResourceType == ResourceType.Server || ResourceType == ResourceType.Folder) && (Permissions.HasFlag(Permissions.Contribute) || Permissions.HasFlag(Permissions.Administrator)))
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanDebug
        {
            get
            {
                if(ResourceType >= ResourceType.WorkflowService && ResourceType <= ResourceType.WebService && (Permissions.HasFlag(Permissions.Execute) || Permissions.HasFlag(Permissions.Administrator) || Permissions.HasFlag(Permissions.Contribute)))
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanRename
        {
            get
            {

                if(ResourceType <= ResourceType.Folder && (Permissions.HasFlag(Permissions.Contribute) || Permissions.HasFlag(Permissions.Administrator)))
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanRemove
        {
            get
            {
                if(ResourceType == ResourceType.Server)
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanDelete
        {
            get
            {
                if(ResourceType <= ResourceType.Folder && (Permissions.HasFlag(Permissions.Contribute) || Permissions.HasFlag(Permissions.Administrator)))
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsVersion
        {
            get
            {
                if(ResourceType == ResourceType.Version && !CanExecute)
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanEdit
        {
            get
            {
                if(ResourceType <= ResourceType.ServerSource && (Permissions.HasFlag(Permissions.View) || Permissions.HasFlag(Permissions.Administrator) || Permissions.HasFlag(Permissions.Contribute)))
                {

                    return true;
                }

                return false;
            }
        }
        public bool CanExecute
        {
            get
            {
                if(ResourceType <= ResourceType.EmailSource && (Permissions.HasFlag(Permissions.Execute) || Permissions.HasFlag(Permissions.Administrator) || Permissions.HasFlag(Permissions.Contribute)))
                {

                    return true;
                }

                return false;
            }
        }
        public bool CanConnect
        {
            get
            {
                if(ResourceType == ResourceType.Server && Permissions >= Permissions.View)
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanDeploy
        {
            get
            {
                if(ResourceType <= ResourceType.Folder && Permissions >= Permissions.DeployFrom)
                {

                    return true;
                }

                return false;
            }
        }
        public bool CanShowDependencies
        {
            get
            {
                if(ResourceType <= ResourceType.EmailSource && Permissions >= Permissions.View && ResourceType != ResourceType.Version)
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanShowHistory
        {
            get
            {
                if(ResourceType == ResourceType.WorkflowService && Permissions >= Permissions.View)
                {
                    return true;
                }
                return false;
            }
        }
        public string ToggleVersionHistoryHeader
        {
            get
            {
                return _toggleVersionHistoryHeader;
            }
            set
            {
                _toggleVersionHistoryHeader = value;
                OnPropertyChanged();
            }
        }
        public bool CanDisconnect
        {
            get
            {
                if(ResourceType == ResourceType.Server && Permissions >= Permissions.View)
                {
                    return true;
                }
                return false;
            }
        }
        public string DeployTitle
        {
            get
            {
                if(ResourceType == ResourceType.Folder || ResourceType == ResourceType.Server)
                {
                    return "Deploy All " + DisplayName;
                }
                return "Deploy " + DisplayName;
            }
        }
        public string ResourcePath { get; set; }

        public bool IsAuthorized
        {
            get
            {
                if(ResourceType == ResourceType.Server ||
                    ResourceType == ResourceType.Version ||
                    ResourceType == ResourceType.Message)
                {
                    return _isAuthorized;
                }
                return Parent.IsAuthorized;
            }
            set
            {
                _isAuthorized = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand NewFolderCommand
        {
            get
            {
                return _newFolderCommand ?? (_newFolderCommand = new RelayCommand(param => AddNewFolder()));
            }
            set
            {
                _newFolderCommand = value;
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand =
                    new RelayCommand(param => Refresh(), o => _serverRefreshing == false));
            }
            set
            {
                _refreshCommand = value;
            }
        }

        public ICommand NewResourceCommand
        {
            get
            {
                return new RelayCommand(ShowNewResourceWizard);
            }
        }

        /// <summary>
        /// Gets the debug command.
        /// </summary>
        /// <value>
        /// The debug command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand DebugCommand
        {
            get
            {
                return _debugCommand ?? (_debugCommand =
                    new RelayCommand(param => Debug()));
            }
        }

        /// <summary>
        /// Gets the show dependencies command.
        /// </summary>
        /// <value>
        /// The show dependencies command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ICommand ShowDependenciesCommand
        {
            get
            {
                return _showDependenciesCommand ?? (_showDependenciesCommand =
                    new RelayCommand(param => ShowDependencies()));
            }
        }

        /// <summary>
        /// Gets the remove command.
        /// </summary>
        /// <value>
        /// The remove command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand RemoveCommand
        {
            get { return _removeCommand ?? (_removeCommand = new RelayCommand(parm => Remove())); }
        }

        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand DeleteCommand
        {
            get
            {
                DelegateCommand deleteCommand = new DelegateCommand(p => Delete());
                return _deleteCommand ?? (_deleteCommand = deleteCommand);
            }
        }

        /// <summary>
        /// Gets the toggle version history command.
        /// </summary>
        /// <value>
        /// Reads the version history
        /// </value>
        /// <author>Tshepo Ntlhokoa</author>
        public ICommand ToggleVersionHistoryCommand
        {
            get
            {
                DelegateCommand toggleVersionHistoryCommand = new DelegateCommand(p => ToggleVersionHistory());
                return _toggleVersionHistoryCommand ?? (_toggleVersionHistoryCommand = toggleVersionHistoryCommand);
            }
        }

        /// <summary>
        /// Gets the toggle version history command.
        /// </summary>
        /// <value>
        /// Rollsback to the selected version
        /// </value>
        /// <author>Tshepo Ntlhokoa</author>
        public ICommand RollbackCommand
        {
            get
            {
                DelegateCommand rollbackCommand = new DelegateCommand(p => Rollback());
                return _rollbackCommand ?? (_rollbackCommand = rollbackCommand);
            }
        }

        /// <summary>
        /// Gets the delete version command.
        /// </summary>
        /// <value>
        /// Deletes a selected version off the server
        /// </value>
        /// <author>Tshepo Ntlhokoa</author>
        public ICommand DeleteVersionCommand
        {
            get
            {
                DelegateCommand deleteVersionCommand = new DelegateCommand(p => DeleteVersion());
                return _deleteVersionCommand ?? (_deleteVersionCommand = deleteVersionCommand);
            }
        }

        /// <summary>
        /// Gets the rename command.
        /// </summary>
        /// <value> 
        /// The delete command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand RenameCommand
        {
            get
            {
                DelegateCommand renameCommand = new DelegateCommand(obj =>
                    {
                        IsRenaming = true;
                    }
                    );
                return _renameCommand ?? (_renameCommand = renameCommand);
            }
            set
            {
                _renameCommand = value;
            }
        }

        /// <summary>
        /// Gets the deploy command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand DeployCommand
        {
            get
            {
                DelegateCommand deployCommand = new DelegateCommand(param => Deploy());
                return _deployCommand ?? (_deployCommand = deployCommand);
            }
        }

        /// <summary>
        /// Gets the edit command.
        /// </summary>
        /// <value>
        /// The edit command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand EditCommand
        {
            get
            {
                DelegateCommand relayCommand = new DelegateCommand(param => Edit());
                return _editCommand ?? (_editCommand = relayCommand);
            }
        }

        /// <summary>
        /// Gets the connect command.
        /// </summary>
        /// <value>
        /// The connect command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand ConnectCommand
        {
            get
            {
                RelayCommand connectCommand = new RelayCommand(param => Connect(), c => !IsConnected);
                return _connectCommand ?? (_connectCommand = connectCommand);
            }
        }

        /// <summary>
        /// Gets the disconnect command.
        /// </summary>
        /// <value>
        /// The disconnect command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        public ICommand DisconnectCommand
        {
            get
            {
                RelayCommand disconnectCommand = new RelayCommand(param => Disconnect(), c => IsConnected);
                return _disconnectCommand ?? (_disconnectCommand = disconnectCommand);
            }
        }
        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                SetIsChecked(value, true, true);
            }
        }
        public bool IsOverwrite
        {
            get
            {
                return _isOverwrite;
            }
            set
            {
                _isOverwrite = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Public methods

        public void OnChildrenChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("ChildrenCount");
            // ReSharper restore ExplicitCallerInfoArgument
            if(Parent != null)
            {
                Parent.OnChildrenChanged();
            }
        }

        public ExplorerItemModel Clone()
        {
            ExplorerItemModel result = new ExplorerItemModel();
            var fieldInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach(FieldInfo field in fieldInfos)
            {
                if(field.FieldType.GetInterface("IList", false) == null)
                {
                    field.SetValue(result, field.GetValue(this));
                }
                else
                {
                    IList listObject = (IList)field.GetValue(result);
                    if(listObject != null)
                    {
                        foreach(ExplorerItemModel item in ((IList)field.GetValue(this)))
                        {
                            listObject.Add(item.Clone());
                        }
                    }
                }
            }
            return result;
        }

        public ExplorerItemModel Clone(IConnectControlSingleton connectControlSingleton, IStudioResourceRepository studioResourceRepository)
        {
            ExplorerItemModel result = new ExplorerItemModel(connectControlSingleton, studioResourceRepository);
            var fieldInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach(FieldInfo field in fieldInfos)
            {
                if(field.FieldType.GetInterface("IList", false) == null)
                {
                    field.SetValue(result, field.GetValue(this));
                }
                else
                {
                    IList listObject = (IList)field.GetValue(result);
                    if(listObject != null)
                    {
                        foreach(ExplorerItemModel item in ((IList)field.GetValue(this)))
                        {
                            listObject.Add(item.Clone(connectControlSingleton, studioResourceRepository));
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        #region Private methods

        public void AddNewFolder()
        {
            IsExplorerExpanded = true;
            var name = GetUniqueName();
            string resourcePath = String.IsNullOrEmpty(ResourcePath) ? name : string.Format("{0}\\{1}", ResourcePath, name);

            ExplorerItemModel explorerItemModel = new ExplorerItemModel(_connectControlSingleton, _studioResourceRepository)
                {
                    DisplayName = name,
                    ResourceType = ResourceType.Folder,
                    Parent = this,
                    EnvironmentId = EnvironmentId,
                    Permissions = Permissions,
                    ResourcePath = resourcePath
                };
            _studioResourceRepository.AddItem(explorerItemModel);

            var firstOrDefault = Children.FirstOrDefault(c => c.DisplayName == name);
            if(firstOrDefault != null)
            {
                firstOrDefault.IsRenaming = true;
            }

        }

        string GetUniqueName()
        {
            string name = "New Folder";
            string name1 = name;
            var nameConflicts = Children.Where(a => a.DisplayName.Contains(name1)).ToList();

            if(nameConflicts.Any())
            {
                int i = 1;
                while(true)
                {
                    var newName = name + i;
                    if(nameConflicts.Any(a => a.DisplayName == newName))
                    {
                        i++;
                    }
                    else
                    {
                        name = newName;
                        break;
                    }
                }
            }
            return name;
        }

        void Refresh()
        {
            if(ResourceType == ResourceType.Server)
            {
                _connectControlSingleton.Refresh(EnvironmentId);
            }
        }

        private int GetChildrenCount()
        {
            int total = 0;
            foreach(ExplorerItemModel explorerItemModel in Children)
            {
                if(explorerItemModel.ResourceType != ResourceType.Version &&
                   explorerItemModel.ResourceType != ResourceType.Message)
                {
                    if(explorerItemModel.ResourceType == ResourceType.Folder)
                    {
                        total += explorerItemModel.ChildrenCount;
                    }
                    else
                    {
                        total++;
                    }
                }
            }
            return total;
        }

        private void ShowNewResourceWizard(object obj)
        {
            var resourceType = obj as string;
            if(!String.IsNullOrEmpty(resourceType))
            {
                EventPublishers.Aggregator.Publish(new ShowNewResourceWizard(resourceType, ResourcePath));
            }
        }

        /// <summary>
        ///  Executes the edit command and send a message to debug
        /// </summary>
        /// <author>Massimo Guerrera</author>
        private void Debug()
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
            if(environmentModel != null)
            {
                var resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == ResourceId);
                if(resourceModel != null)
                {
                    Edit();
                    EventPublishers.Aggregator.Publish(new DebugResourceMessage(resourceModel as IContextualResourceModel));
                }
            }
        }

        /// <summary>
        ///  Sends a message to show the dependencies.
        /// </summary>
        /// <author>Massimo Guerrera</author>        
        private void ShowDependencies()
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
            if(environmentModel != null)
            {
                var resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == ResourceId);
                if(resourceModel != null)
                {
                    EventPublishers.Aggregator.Publish(new ShowDependenciesMessage(resourceModel as IContextualResourceModel, true));
                }
            }
        }

        /// <summary>
        ///     Removes the specified child from the children.
        /// </summary>
        /// <author>Massimo Guerrera</author>
        private void Remove()
        {
            _connectControlSingleton.Remove(EnvironmentId);
        }

        /// <summary>
        /// Sends a message to delete this specific instance
        /// </summary>
        /// <author>Massimo Guerrera</author>
        private void Delete()
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
            if(environmentModel != null)
            {
                var folderList = new List<IExplorerItemModel>();
                var contextualResourceModels = new Collection<IContextualResourceModel>();
                foreach(var childModel in this.Descendants())
                {
                    if(childModel.ResourceType != ResourceType.Folder)
                    {
                        var child = childModel;
                        var resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == child.ResourceId);
                        if(resourceModel == null && childModel.ResourceType == ResourceType.WebSource)
                        {
                            environmentModel.ResourceRepository.ReloadResource(child.ResourceId, Studio.Core.AppResources.Enums.ResourceType.Source, ResourceModelEqualityComparer.Current, true);
                            resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == child.ResourceId);
                        }
                        if(resourceModel != null)
                        {
                            contextualResourceModels.Add(resourceModel as IContextualResourceModel);
                        }
                    }
                    else
                    {
                        folderList.Add(childModel);
                    }
                }

                if(contextualResourceModels.Any())
                {
                    var displayName = DisplayName;
                    EventPublishers.Aggregator.Publish(new DeleteResourcesMessage(contextualResourceModels, displayName, true, () =>
                    {
                        for(int i = folderList.Count - 1; i >= 0; i--)
                        {
                            if(folderList[i].ResourceType == ResourceType.Folder && (folderList[i].Children.Count == 0 || folderList[i].Children.All(c => c.ResourceType == ResourceType.Folder)))
                            {
                                _studioResourceRepository.DeleteFolder(folderList[i]);
                            }
                        }
                    }));
                }
                else
                {
                    EventPublishers.Aggregator.Publish(new DeleteFolderMessage(DisplayName, () =>
                    {
                        for(int i = folderList.Count - 1; i >= 0; i--)
                        {
                            if(folderList[i].ResourceType == ResourceType.Folder && (folderList[i].Children.Count == 0 || folderList[i].Children.All(c => c.ResourceType == ResourceType.Folder)))
                            {
                                _studioResourceRepository.DeleteFolder(folderList[i]);
                            }
                        }
                    }));
                }
            }
        }

        private void ToggleVersionHistory()
        {
            if(Children.Count == 0)
            {
                _studioResourceRepository.ShowVersionHistory(EnvironmentId, ResourceId);
                ToggleVersionHistoryHeader = "Hide Version History";
            }
            else
            {
                _studioResourceRepository.HideVersionHistory(EnvironmentId, ResourceId);
                ToggleVersionHistoryHeader = "Show Version History";
            }
        }

        private void Rollback()
        {
            IPopupController popup = CustomContainer.Get<IPopupController>();
            var res = popup.ShowRollbackVersionMessage(string.Format("{0} (v.{1})", Parent.DisplayName, VersionInfo.VersionNumber));

            if(res == MessageBoxResult.Yes)
            {
                var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
                if(environmentModel == null)
                {
                    return;
                }

                IContextualResourceModel resourceModel = environmentModel.ResourceRepository
                                                                         .FindSingle(model => model.ID == Parent.ResourceId)
                                                                         as IContextualResourceModel;
                if(resourceModel != null)
                {
                    IMainViewModel mainViewModel = CustomContainer.Get<IMainViewModel>();
                    var isOpen = mainViewModel.IsWorkFlowOpened(resourceModel);
                    if(isOpen)
                    {
                        EventPublishers.Aggregator.Publish(new RemoveResourceAndCloseTabMessage(resourceModel, true));
                    }
                    _studioResourceRepository.RollbackTo(VersionInfo, EnvironmentId);

                    if(isOpen)
                    {
                        EventPublishers.Aggregator.Publish(new AddWorkSurfaceMessage(resourceModel));
                    }
                }
            }
        }

        private void DeleteVersion()
        {
            IPopupController popup = CustomContainer.Get<IPopupController>();
            var res = popup.ShowDeleteVersionMessage(string.Format("{0} (v.{1})", Parent.DisplayName, VersionInfo.VersionNumber));

            if(res == MessageBoxResult.Yes)
            {
                _studioResourceRepository.DeleteVersion(VersionInfo, EnvironmentId);
            }
        }

        public void UpdateCategoryIfOpened(string category)
        {
           
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
            if (environmentModel != null)
            {
                var resource = environmentModel.ResourceRepository.FindSingle(a => a.ID == ResourceId) as IContextualResourceModel;


                if (resource != null && ResourceType <= ResourceType.ServerSource)
                {
                    IMainViewModel mainViewModel = CustomContainer.Get<IMainViewModel>();
                    mainViewModel.UpdateWorkflowLink(resource,category,resource.Category);
                    var xaml = resource.WorkflowXaml;
                    if (xaml != null)
                    {
                        resource.WorkflowXaml = xaml.Replace("Category=\"" + resource.Category, "Category=\"" + category);

                    }
                    resource.Category = category;
                    
                }
            }
        }

        private string GetRenamePath(string existingPath, string newName)
        {
            if (!existingPath.Contains("\\"))
                return newName;
            return  existingPath.Substring(0, 1+existingPath.LastIndexOf('\\')) + newName;
        }

        private bool Rename(string newName)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
            if(ResourceType == ResourceType.Folder)
            {
                var itemExists = null != _studioResourceRepository.FindItem(a => a.ResourceType == ResourceType.Folder && a.ResourcePath == GetRenamePath(ResourcePath, newName));
                if ((Children.Count == 0) && itemExists)
                {
                    EventPublishers.Aggregator.Publish(new DisplayMessageBoxMessage("Error Renaming Folder", "You are not allowed to Rename an empty folder to an existing folder name", MessageBoxImage.Warning));
                    return false;
                }
                _studioResourceRepository.RenameFolder(this, newName);
            }
            else
                if(environmentModel != null)
                {
                    var resourceModel = environmentModel.ResourceRepository.FindSingle(a => a.ID == ResourceId) as IContextualResourceModel;
                    if(resourceModel != null)
                    {
                        IMainViewModel mainViewModel = CustomContainer.Get<IMainViewModel>();
                        var isOpen = mainViewModel.IsWorkFlowOpened(resourceModel);
                        if(isOpen)
                        {
                            EventPublishers.Aggregator.Publish(new RemoveResourceAndCloseTabMessage(resourceModel, true));
                        }
                        if(ResourceType == ResourceType.Folder)
                        {
                            _studioResourceRepository.RenameFolder(this, newName);
                        }
                        else
                        {
                            _studioResourceRepository.RenameItem(this, newName);
                        }

                        RefreshName(newName);

                        if(isOpen)
                        {
                            EventPublishers.Aggregator.Publish(new AddWorkSurfaceMessage(resourceModel));
                        }
                    }
                }
            return true;
        }

        public void RefreshName(string newName)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
            if(environmentModel != null)
            {
                var resource = environmentModel.ResourceRepository.FindSingle(a => a.ID == ResourceId);
                SetDisplay(newName);
                if(resource != null)
                {
                    var oldName = resource.DisplayName;
                    resource.DisplayName = newName;
                    resource.ResourceName = newName;
                    resource.Category = resource.Category.Replace(oldName, newName);
                }
                if(resource != null && ResourceType <= ResourceType.ServerSource)
                {
                    var xaml = resource.WorkflowXaml;
                    if(xaml != null)
                    {
                        resource.WorkflowXaml = xaml
                            .Replace("x:Class=\"" + resource.ResourceName, "x:Class=\"" + newName)
                            .Replace("Name=\"" + resource.ResourceName, "Name=\"" + newName)
                            .Replace("ToolboxFriendlyName=\"" + resource.ResourceName, "ToolboxFriendlyName=\"" + newName)
                            .Replace("DisplayName=\"" + resource.ResourceName, "DisplayName=\"" + newName);
                    }
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public void CancelRename(KeyEventArgs eventArgs)
        {
            if(eventArgs.Key == Key.Escape)
            {
                CancelRename();
            }
        }

        public void CancelRename()
        {
            IsRenaming = false;
        }

        /// <summary>
        /// Edits this instance.
        /// </summary>
        /// <author>Massimo Guerrera</author>
        private void Edit()
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == EnvironmentId);
            if(environmentModel == null)
            {
                return;
            }

            if(ResourceType == ResourceType.Version)
            {
                var workflowXaml = _studioResourceRepository.GetVersion(VersionInfo, EnvironmentId);
                if(workflowXaml != null)
                {
                    IResourceModel resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == Parent.ResourceId);
                    var resourceVersion = new ResourceModel(environmentModel, EventPublishers.Aggregator)
                        {
                            ResourceType = resourceModel.ResourceType,
                            ResourceName = string.Format("{0} (v.{1})", Parent.DisplayName, VersionInfo.VersionNumber),
                            WorkflowXaml = workflowXaml,
                            UserPermissions = Permissions.View,
                            IsVersionResource = true,
                            ID = ResourceId
                        };
                    WorkflowDesignerUtils.EditResource(resourceVersion, EventPublishers.Aggregator);
                }
            }
            else
            {
                IResourceModel resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == ResourceId);
                if(environmentModel.ResourceRepository != null)
                {
                    if(resourceModel != null)
                    {
                        WorkflowDesignerUtils.EditResource(resourceModel, EventPublishers.Aggregator);
                    }
                    else
                    {
                        Parent.Children.Remove(this);
                    }
                }
            }
        }



        /// <summary>
        /// Deploys this instance.
        /// </summary>
        /// <author>Massimo Guerrera</author>
        private void Deploy()
        {
            EventPublishers.Aggregator.Publish(new DeployResourcesMessage(this));
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <author>Massimo Guerrera</author>
        private void Connect()
        {
            _connectControlSingleton.ToggleConnection(EnvironmentId);
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <author>Massimo Guerrera</author>
        private void Disconnect()
        {
            _connectControlSingleton.ToggleConnection(EnvironmentId);
        }

        public bool IsAuthorizedDeployTo
        {
            get
            {
                if(ResourceType == ResourceType.Server)
                {
                    return Permissions.HasFlag(Permissions.Administrator) || Permissions.HasFlag(Permissions.DeployTo);
                }
                return true;
            }
        }
        public IAsyncWorker AsyncWorker
        {
            get
            {
                return _asyncWorker;
            }
        }

        #endregion

        /// <summary>
        ///     Sets the IsChecked Property and updates children and parent
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="updateChildren">
        ///     if set to <c>true</c> [update children].
        /// </param>
        /// <param name="updateParent">
        ///     if set to <c>true</c> [update parent].
        /// </param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            var preState = _isChecked;
            if(value == _isChecked)
            {
                return;
            }

            if(_isChecked == true && value == false)
            {
                if(Parent != null)
                {
                    Parent.IsOverwrite = false;
                }
                IsOverwrite = false;
            }

            _isChecked = value;

            UpdateChildren(updateChildren);
            UpdateParent(updateParent);

            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsChecked");
            // ReSharper restore ExplicitCallerInfoArgument           
            CheckStateChangedArgs checkStateChangedArgs = new CheckStateChangedArgs(preState.GetValueOrDefault(false), value.GetValueOrDefault(false), ResourceId, ResourceType);
            if(OnCheckedStateChangedAction != null)
            {
                OnCheckedStateChangedAction.Invoke(checkStateChangedArgs);
            }
        }

        void UpdateParent(bool updateParent)
        {
            if(updateParent && Parent != null)
            {
                Parent.VerifyCheckState();
            }
        }

        void UpdateChildren(bool updateChildren)
        {
            if(!updateChildren || !_isChecked.HasValue)
            {
                return;
            }
            foreach(var c in Children)
            {
                c.SetIsChecked(_isChecked, true, false);
            }
        }

        public static Action<CheckStateChangedArgs> OnCheckedStateChangedAction;
        public static Action<bool> OnRenameChangedAction;
        /// <summary>
        ///     Verifies the state of the IsChecked property by taking the childrens IsChecked State into account
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public virtual void VerifyCheckState()
        {
            bool? state = null;
            var count = Children.Count();
            var i = 0;
            var stateNull = false;
            while(i < count && !stateNull)
            {
                var current = Children.ToArray()[i].IsChecked;
                if(i == 0)
                {
                    state = current;
                }
                else if(state != current)
                {
                    state = null;
                    stateNull = true;
                }
                i++;
            }
            SetIsChecked(state, false, true);
        }
    }


    public class CheckStateChangedArgs
    {
        public bool PreviousState { get; set; }
        public bool NewState { get; set; }
        public Guid ResourceId { get; set; }
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public CheckStateChangedArgs(bool previousState, bool newState, Guid resourceId, ResourceType resourceType)
        {
            PreviousState = previousState;
            NewState = newState;
            ResourceId = resourceId;
            ResourceType = resourceType;
        }
    }
}
