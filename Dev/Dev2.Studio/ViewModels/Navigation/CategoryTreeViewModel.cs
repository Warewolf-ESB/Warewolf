//6180 CODEREVIEW - Please region you code

#region

using Caliburn.Micro;
using Dev2.Common.ExtMethods;
using Dev2.Providers.Logs;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;


#endregion

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    ///     ViewModel for a node representing a category
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class CategoryTreeViewModel : AbstractTreeViewModel
    {
        #region private fields

        ResourceType _resourceType;
        bool _isRenaming;
        string _displayName;
        ICommand _showNewWorkflowWizard;
        ICommand _renameCommand;
        ICommand _deleteFolderCommand;

        #endregion

        #region ctor + init

        public CategoryTreeViewModel(IEventAggregator eventPublisher, ITreeNode parent, string name, ResourceType resourceType)
            : base(eventPublisher, parent)
        {
            IsRenaming = false;
            DisplayName = name;
            ResourceType = resourceType;
        }

        #endregion

        #region public properties

        [Import]
        public IPopupController PopupProvider { get; set; }

        /// <summary>
        ///     Gets or sets the type of the resource.
        /// </summary>
        /// <value>
        ///     The type of the resource represented by the <see cref="CategoryTreeViewModel" /> parent of this category.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ResourceType ResourceType
        {
            get { return _resourceType; }
            set
            {
                if(_resourceType == value)
                    return;

                _resourceType = value;
                NotifyOfPropertyChange(() => ResourceType);
            }
        }

        /// <summary>
        ///     Gets the environment model by walking the tree to the <see cref="EnvironmentTreeViewModel" /> class..
        /// </summary>
        /// <value>
        ///     The environment model.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        /// <exception cref="System.InvalidOperationException">Can not set this property</exception>
        public override IEnvironmentModel EnvironmentModel
        {
            get { return TreeParent == null ? null : TreeParent.EnvironmentModel; }
            protected set { throw new InvalidOperationException(); }
        }

        /// <summary>
        ///     Gets the icon path for this category.
        /// </summary>
        /// <value>
        ///     The icon path.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string IconPath
        {
            get
            {
                if(IsExpanded)
                {
                    return StringResources.Navigation_OpenFolder_Icon_Pack_Uri;
                }
                return StringResources.Navigation_Folder_Icon_Pack_Uri;
            }
        }

        public override ObservableCollection<ITreeNode> Children
        {
            get
            {
                if(_children == null)
                {
                    _children = new SortedObservableCollection<ITreeNode>();
                    _children.CollectionChanged += ChildrenOnCollectionChanged;
                }
                return _children;
            }
            set
            {
                if(_children == value)
                    return;

                _children = value;
                _children.CollectionChanged -= ChildrenOnCollectionChanged;
                _children.CollectionChanged += ChildrenOnCollectionChanged;
            }
        }

        //2013.05.18: Ashley Lewis for PBI 8858 - workflow folder context menu upgrades
        public override ICommand NewResourceCommand
        {
            get
            {
                return _showNewWorkflowWizard ?? (_showNewWorkflowWizard =
                    new RelayCommand(ShowNewResourceWizard));
            }
        }

        void ShowNewResourceWizard(object obj)
        {
            IResourceModel hydrateWizard = new ResourceModel(EnvironmentModel);
            hydrateWizard.Category = DisplayName;
            hydrateWizard.DisplayName = obj.ToString();
            this.TraceInfo("Publish message of type - " + typeof(ShowEditResourceWizardMessage));
            EventPublisher.Publish(new ShowEditResourceWizardMessage(hydrateWizard));
        }

        public override bool HasNewWorkflowMenu
        {
            get
            {
                //return ResourceType == ResourceType.WorkflowService;
                return false;
            }
        }

        public override bool IsExpanded
        {
            get
            {
                return base.IsExpanded;
            }
            set
            {
                base.IsExpanded = value;
                NotifyOfPropertyChange(() => IconPath);
            }
        }

        public override bool HasNewServiceMenu
        {
            get
            {
                return false; // ResourceType == ResourceType.Service;
            }
        }

        public override bool HasNewSourceMenu
        {
            get
            {
                return false; // ResourceType == ResourceType.Source;
            }
        }

        public override sealed bool IsRenaming
        {
            get { return _isRenaming; }
            set
            {
                if(_isRenaming == value)
                    return;

                _isRenaming = value;
                NotifyOfPropertyChange(() => IsRenaming);
            }
        }

        public string DisplayNameValidationRegex
        {
            get
            {
                return Common.ExtMethods.StringExtension.IsValidCategoryname.ToString();
            }
        }

        //2013.05.19: Ashley Lewis for PBI 8858 - Rename folder context menu item
        public override ICommand RenameCommand
        {
            get
            {
                return _renameCommand ?? (_renameCommand =
                    new RelayCommand(obj => { IsRenaming = true; }));
            }
        }

        //2013.07.01: Ashley Lewis for PBI 9487 - rename folder
        public override sealed string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                if(_displayName == value)
                {
                    return;
                }
                if(_displayName == null)
                {
                    _displayName = value;
                    return;
                }
                CancelRename();
                RenameCategory(value);
            }
        }

        protected virtual void RenameCategory(string newCategory)
        {
            if(string.IsNullOrEmpty(newCategory) || !newCategory.IsValidCategoryName())
            {
                throw new ArgumentException(StringResources.InvalidCategoryNameExceptionMessage, "newCategory");
            }

            EnvironmentModel.ResourceRepository.RenameCategory(DisplayName, newCategory, ResourceType);
            Reparent(newCategory);
        }

        public void CancelRename()
        {
            IsRenaming = false;
        }

        public void CancelRename(KeyEventArgs eventArgs)
        {
            if(eventArgs.Key == Key.Escape)
            {
                CancelRename();
            }
        }

        public override bool CanRename
        {
            get
            {
                return true;
            }
        }

        public override bool CanDelete
        {
            get
            {
                return true;
            }
        }

        public override bool CanDeploy
        {
            get
            {
                if(TreeParent != null)
                {
                    return TreeParent.DisplayName == "WORKFLOWS" || TreeParent.DisplayName == "SOURCES" || TreeParent.DisplayName == "SERVICES";
                }
                return false;
            }
        }

        public override ICommand DeleteCommand
        {
            get { return _deleteFolderCommand ?? (_deleteFolderCommand = new RelayCommand(DeleteFolder)); }
        }

        public override string NewWorkflowTitle
        {
            get
            {
                return "New Workflow in " + DisplayName + "   (Ctrl+W)";
            }
        }

        void DeleteFolder(object obj)
        {
            this.TraceInfo("Publish message of type - " + typeof(DeleteResourcesMessage));
            EventPublisher.Publish(new DeleteResourcesMessage(Children.Select(child =>
            {
                var model = child as ResourceTreeViewModel;
                return model != null ? model.DataContext : null;
            }).ToList()));
        }

        public override string DeployTitle
        {
            get
            {
                if(DisplayName.Length > 0)
                {
                    return "Deploy All " + DisplayName.ToUpper();
                }
                return "Deploy";
            }
        }

        #endregion

        #region public methods

        public override void SetFilter(string filterText, bool updateChildren)
        {
            //bool originalFilter = IsFiltered;
            if(!updateChildren)
                return;

            IsFiltered = !DisplayName.ToUpper().Contains(filterText.ToUpper());
            if(!IsFiltered)
            {
                Children.ToList().ForEach(c => { c.IsFiltered = false; });
            }
            else
            {
                Children.ToList().ForEach(c => c.SetFilter(filterText, false));
                IsFiltered = Children.All(c => c.IsFiltered);
            }
            VerifyCheckState();
        }

        /// <summary>
        ///     Finds the child containing a specific resource,
        ///     in this case a string as the displayname
        /// </summary>
        /// <typeparam name="T">Type of the resource to find</typeparam>
        /// <param name="resourceToFind">The resource to find.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ITreeNode FindChild<T>(T resourceToFind)
        {
            if(resourceToFind is string)
            {
                var name = resourceToFind as string;
                if(String.Compare(DisplayName, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return this;
            }
            return base.FindChild(resourceToFind);
        }

        #endregion

        #region IComparable Implementation

        public override int CompareTo(object obj)
        {
            var model = obj as CategoryTreeViewModel;
            if(model != null)
            {
                CategoryTreeViewModel other = model;
                if((String.Compare(DisplayName, StringResources.Navigation_Category_Unassigned,
                    StringComparison.InvariantCultureIgnoreCase)) == 0)
                    return -1;
                return String.Compare(DisplayName, other.DisplayName, StringComparison.InvariantCultureIgnoreCase);
            }
            return base.CompareTo(obj);
        }

        #endregion

        protected override ITreeNode CreateParent(string displayName)
        {
            return new CategoryTreeViewModel(EventPublisher, TreeParent, displayName, ResourceType);
        }
    }
}