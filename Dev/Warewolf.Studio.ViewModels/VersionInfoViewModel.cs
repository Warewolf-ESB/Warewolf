using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Versioning;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class VersionInfoViewModel: BindableBase, IVersionInfoViewModel
    {
        string _versionName;
        string _version;
        DateTime _versionDate;
        bool _canRollBack;
        bool _isVisible;
        string _reason;
        bool _canRollback;
        IExplorerTreeItem _parent;

        #region Implementation of IVersionInfoViewModel

        public  VersionInfoViewModel(IVersionInfo version, IExplorerRepository repository, IExplorerItemViewModel explorerItemViewModel)
        {
            VersionName = version.VersionNumber;
            VersionDate = version.DateTimeStamp;
            ResourceId = version.ResourceId;
            Version = version.VersionNumber;
            Reason = version.Reason;
            IsVisible = true;
            RollbackCommand = new DelegateCommand(() =>
            {
               var output = repository.Rollback(ResourceId, Version);
                 _parent.ShowVersionHistory.Execute(null);
                 _parent.ResourceName = output.DisplayName;

            });
            _parent = explorerItemViewModel;
            CanShowVersions = false;
            CanRollBack = true;
            Expand = new DelegateCommand(() => { });
        }

        public string DisplayName { get; set; }
        public ResourceType ResourceType { get; set; }

        public string ResourceName { get; set; }

        public bool AreVersionsVisible { get; set; }
        public string VersionName
        {
            get
            {
                return _versionName;
            }
            set
            {
                _versionName = value;
                OnPropertyChanged(()=>VersionName);
            }
        }
        public Guid ResourceId { get; set; }
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                OnPropertyChanged(Version);
            }
        }
        public DateTime VersionDate
        {
            get
            {
                return _versionDate;
            }
            set
            {
                _versionDate = value;
                OnPropertyChanged(() => VersionDate);
            }
        }
        public bool CanRollBack
        {
            get
            {
                return _canRollBack;
            }
            set
            {
                _canRollBack = value;
                OnPropertyChanged(() => CanRollBack);
            }
        }
        public ICommand OpenCommand { get; set; }
        public ICommand ShowVersionHistory { get; set; }
        public ICommand RollbackCommand { get; set; }
        public IServer Server { get; set; }

        public ICommand Expand
        {
            get; set;
        }
        public ICollection<IExplorerItemViewModel> Children { get; set; }

        public IExplorerTreeItem Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public void AddChild(IExplorerItemViewModel child)
        {            
        }

        public void RemoveChild(IExplorerItemViewModel child)
        {
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => CanRollBack);
            }
        }
        public bool IsExpanderVisible { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand DeployCommand { get; set; }
        public bool CanCreateDbService { get; set; }
        public bool CanCreateDbSource { get; set; }
        public bool CanCreateServerSource { get; set; }
        public bool CanCreateWebService { get; set; }
        public bool CanCreateWebSource { get; set; }
        public bool CanCreatePluginService { get; set; }
        public bool CanCreatePluginSource { get; set; }
        public bool CanRename { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCreateFolder { get; set; }
        public bool CanDeploy { get; set; }
        public bool CanShowVersions { get; set; }
        public bool CanRollback
        {
            get
            {
                return _canRollback;
            }
            set
            {
                _canRollback = value;
                OnPropertyChanged(()=>CanRollback);
            }
        }
        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }
        public bool CanShowServerVersion { get; set; }

        public ICommand RenameCommand { get; set; }
        public ICommand CreateFolderCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public string VersionHeader
        {
            get
            {
                return  string.Format("{0} {1} {2}", Version, VersionDate.ToLongDateString(), Reason);
            }            
        }
        public string Reason
        {
            get
            {
                return _reason;
            }
            set
            {

                _reason = value;
                OnPropertyChanged(Reason);
            }
        }

        #endregion
    }
}