using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Security;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DeployStatsViewerViewModel : BindableBase, IDeployStatsViewerViewModel
    {
        readonly IExplorerViewModel _destination;
        int _connectors;
        int _services;
        int _sources;
        int _unknown;
        int _newResources;
        int _overrides;
        string _status;
        public string RenameErrors { get; private set; }
        List<Conflict> _conflicts;
        IEnumerable<IExplorerTreeItem> _new;
        IList<IExplorerTreeItem> _items;

        public DeployStatsViewerViewModel(IExplorerViewModel destination)
        {
            VerifyArgument.IsNotNull(@"destination", destination);
            _destination = destination;
            Status = @"";
        }

        #region Implementation of IDeployStatsViewerViewModel

        /// <summary>
        /// Services being deployed
        /// </summary>
        public int Connectors
        {
            get
            {
                return _connectors;
            }
            set
            {
                _connectors = value;
                OnPropertyChanged(() => Connectors);
            }
        }
        /// <summary>
        /// Services Being Deployed
        /// </summary>
        public int Services
        {
            get
            {
                return _services;
            }
            set
            {
                _services = value;
                OnPropertyChanged(() => Services);
            }
        }
        /// <summary>
        /// Sources being Deployed
        /// </summary>
        public int Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                OnPropertyChanged(() => Sources);
            }
        }
        /// <summary>
        /// What is unknown is unknown to me
        /// </summary>
        public int Unknown
        {
            get
            {
                return _unknown;
            }
            set
            {
                _unknown = value;
                OnPropertyChanged(() => Unknown);
            }
        }
        /// <summary>
        /// The count of new resources being deployed
        /// </summary>
        public int NewResources
        {
            get
            {
                return _newResources;
            }
            set
            {
                _newResources = value;
                OnPropertyChanged(() => NewResources);
            }
        }
        /// <summary>
        /// The count of overidded resources
        /// </summary>
        public int Overrides
        {
            get
            {
                return _overrides;
            }
            set
            {
                _overrides = value;
                OnPropertyChanged(() => Overrides);
            }
        }
        /// <summary>
        /// The status of the last deploy
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(() => Status);
            }
        }
        public void ReCalculate()
        {
            if (_items != null)
            {
                Calculate(_items);
            }
        }

        public void CheckPermissionPersmisions()
        {
            var destItems = _destination.SelectedEnvironment.AsList();
            if (destItems != null)
            {
                foreach(var explorerItemViewModel in destItems)
                {
                    var currentItem = _items.FirstOrDefault(p=>p.ResourceName == explorerItemViewModel.ResourceName);
                    {
                        if(currentItem != null)
                        {
                            var permission = explorerItemViewModel.Server.Permissions.FirstOrDefault(p => p.ResourceID == explorerItemViewModel.ResourceId);
                            if(permission?.Permissions == Permissions.View)
                                currentItem.CanDeploy = false;
                        }
                    }
                }
            }
        }

        public void Calculate(IList<IExplorerTreeItem> items)
        {
            _items = items;
            if (items != null)
            {
                //Connectors = items.Count(a => a.ResourceType >= "DbService" && a.ResourceType <= "WebService");
                // FIX?

                Connectors = items.Count(a =>
                                        !string.IsNullOrEmpty(a.ResourceType)
                                        && a.ResourceType.Contains(@"Service")
                                        && a.ResourceType != @"WorkflowService"
                                        && a.ResourceType != @"ReservedService");



                Services = items.Count(a => !string.IsNullOrEmpty(a.ResourceType)
                                        && a.ResourceType == @"WorkflowService");

                Sources = items.Count(a => !string.IsNullOrEmpty(a.ResourceType)
                                            && IsSource(a.ResourceType));
                Unknown = items.Count(a => a.ResourceType == @"Unknown" || string.IsNullOrEmpty(a.ResourceType));

                if (_destination.SelectedEnvironment != null)
                {
                    var explorerItemViewModels = _destination.SelectedEnvironment.AsList();
                    var conf = from b in explorerItemViewModels
                               join explorerTreeItem in items on new { b.ResourceId, b.ResourcePath } equals new { explorerTreeItem.ResourceId, explorerTreeItem.ResourcePath }
                               where b.ResourceType != @"Folder" && explorerTreeItem.ResourceType != @"Folder"
                               select new Conflict { SourceName = explorerTreeItem.ResourceName, DestinationName = b.ResourceName };

                    _conflicts = conf.ToList();
                    _new = items.Except(explorerItemViewModels);
                    var ren = from b in explorerItemViewModels
                              join explorerTreeItem in items on new { b.ResourcePath } equals new { explorerTreeItem.ResourcePath }
                              where (b.ResourceType != @"Folder" && explorerTreeItem.ResourceType != @"Folder")
                              select new { SourceName = explorerTreeItem.ResourcePath, DestinationName = b.ResourcePath, SourceId = explorerTreeItem.ResourceId, DestinationId = b.ResourceId };
                    var errors = ren.Where(ax => ax.SourceId != ax.DestinationId).ToArray();
                    if (errors.Any())
                    {

                        RenameErrors = @"The following resources have the same path and name on the source and destination server but different Ids";
                        foreach (var error in errors)
                        {
                            RenameErrors += $"\n{error.SourceName}-->{error.DestinationName}";
                        }
                        RenameErrors += $"\nPlease rename either the source or destination before continueing";
                    }
                    else
                    {
                        RenameErrors = @"";
                    }
                }
                else
                {
                    _conflicts = new List<Conflict>();
                    _new = new List<IExplorerTreeItem>();
                }

                Overrides = Conflicts.Count;
                NewResources = New.Count;
            }
            else
            {
                Connectors = 0;
                Services = 0;
                Sources = 0;
                Unknown = 0;
                _conflicts = new List<Conflict>();
                _new = new List<IExplorerTreeItem>();
            }

            OnPropertyChanged(() => Conflicts);
            OnPropertyChanged(() => New);
            CalculateAction?.Invoke();
        }

        public IList<Conflict> Conflicts => _conflicts.ToList();

        public IList<IExplorerTreeItem> New
        {
            get
            {
                var explorerTreeItems = _new.Where(a => a.ResourceType != @"Folder").ToList();
                return explorerTreeItems;
            }
        }
        public Action CalculateAction { get; set; }

        bool IsSource(string res)
        {
            //return (res >= "DbSource" && res <= "ServerSource") || (res == "DropboxSource");
            // FIX?
            return res.Contains(@"Source");
        }
        #endregion
    }
}