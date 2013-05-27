//6180 CODEREVIEW - Please region you code

#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;

#endregion

namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    ///     ViewModel for a node representing a category
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public sealed class CategoryTreeViewModel : AbstractTreeViewModel
    {
        #region private fields

        private ResourceType _resourceType;
        private bool _isRenaming = false;
        RelayCommand _showNewWorkflowWizard;

        #endregion

        #region ctor + init

        /// <summary>
        ///     Initializes a new instance of the <see cref="CategoryTreeViewModel" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="parent">The parent.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public CategoryTreeViewModel(string name, ResourceType resourceType, ITreeNode parent)
            : base(null)
        {
            DisplayName = name;
            ResourceType = resourceType;
            if (parent != null)
            {
                parent.Add(this);
            }
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
                if (_resourceType == value) return;

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
            get { return StringResources.Navigation_Folder_Icon_Pack_Uri; }
        }

        public override ObservableCollection<ITreeNode> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new SortedObservableCollection<ITreeNode>();
                    _children.CollectionChanged += ChildrenOnCollectionChanged;
                }
                return _children;
            }
            set
            {
                if (_children == value) return;

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
            EventAggregator.Publish(new ShowEditResourceWizardMessage(hydrateWizard));
        }

        public override bool HasNewWorkflowMenu
        {
            get
            {
                //return ResourceType == ResourceType.WorkflowService;
                return false;
            }
        }

        public override bool HasNewServiceMenu
        {
            get
            {
                return false;// ResourceType == ResourceType.Service;
            }
        }

        public override bool HasNewSourceMenu
        {
            get
            {
                return false;// ResourceType == ResourceType.Source;
            }
        }

        public override bool IsRenaming
        {
            get
            {
                return _isRenaming;
            }
        }

        //2013.05.19: Ashley Lewis for PBI 8858 - Rename folder context menu item
        public override ICommand RenameCommand
        {
            get
            {
                return new RelayCommand(RenameFolder);
            }
        }

        void RenameFolder(object obj)
        {
            _isRenaming = true;
        }

        public override bool CanRename
        {
            get
            {
                return false;
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
                    return TreeParent.DisplayName == "WORKFLOWS";
                }
                return false;
            }
        }

        public override ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(DeleteFolder);
            }
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
            IPopupController result = new PopupController();
            string deletePrompt = "Are you sure you want to delete this folder?";
            var deleteAnswer = new MessageBoxResult();
            //Translate tree parent name to resource type
            switch(TreeParent.DisplayName)
            {
                case "WORKFLOWS":
                    deletePrompt = String.Format(StringResources.DialogBody_ConfirmFolderDelete, DisplayName);
                    deleteAnswer = result.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete,
                                                  MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (deleteAnswer == MessageBoxResult.OK)
                    {
                        foreach (var resource in EnvironmentModel.ResourceRepository.Find(resource => resource.Category.ToLower() == (DisplayName == "Unassigned" ? string.Empty : DisplayName.ToLower()) && (resource.ResourceType == ResourceType.WorkflowService) && !resource.ResourceName.EndsWith(".wiz")))
                        {
                            EventAggregator.Publish(new DeleteResourceMessage(resource, false));
                        }
                    }
                    break;
                case "SERVICES":
                    deletePrompt = String.Format(StringResources.DialogBody_ConfirmFolderDelete, DisplayName);
                    deleteAnswer = result.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete,
                                                  MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if(deleteAnswer == MessageBoxResult.OK)
                    {
                        foreach(var resource in EnvironmentModel.ResourceRepository.Find(resource => resource.Category.ToLower() == (DisplayName == "Unassigned" ? string.Empty : DisplayName.ToLower()) && (resource.ResourceType == ResourceType.Service) && !resource.ResourceName.EndsWith(".wiz")))
                        {
                            EventAggregator.Publish(new DeleteResourceMessage(resource, false));
                        }
                    }
                    break;
                case "SOURCES":
                    deletePrompt = String.Format(StringResources.DialogBody_ConfirmFolderDelete, DisplayName);
                    deleteAnswer = result.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete,
                                                  MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if(deleteAnswer == MessageBoxResult.OK)
                    {
                        foreach(var resource in EnvironmentModel.ResourceRepository.Find(resource => resource.Category.ToLower() == (DisplayName == "Unassigned" ? string.Empty : DisplayName.ToLower()) && (resource.ResourceType == ResourceType.Source) && !resource.ResourceName.EndsWith(".wiz")))
                        {
                            EventAggregator.Publish(new DeleteResourceMessage(resource, false));
                        }
                    }
                    break;
            }
            if(Children.Count == 0)
            {
                TreeParent.Remove(this);
            }
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
            if (!updateChildren) return;

            IsFiltered = !DisplayName.ToUpper().Contains(filterText.ToUpper());
            if (!IsFiltered)
            {
                Children.ToList().ForEach(c => { c.IsFiltered = false; });
            }
            else
            {
                Children.ToList().ForEach(c => c.SetFilter(filterText, false));
                IsFiltered = Children.All(c => c.IsFiltered);
            }
            VerifyCheckState();

            ////Notify parent to verify filterstate
            //if (TreeParent != null && originalFilter != IsFiltered)
            //{
            //    TreeParent.SetFilter(filterText, false);
            //}

            ////Notify parent to update check status
            //if (TreeParent != null)
            //{
            //    TreeParent.VerifyCheckState();
            //}
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
            if (resourceToFind is string)
            {
                var name = resourceToFind as string;
                if (String.Compare(DisplayName, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return this;
            }
            return base.FindChild(resourceToFind);
        }

        #endregion

        #region IComparable Implementation

        public override int CompareTo(object obj)
        {
            var model = obj as CategoryTreeViewModel;
            if (model != null)
            {
                CategoryTreeViewModel other = model;
                if ((String.Compare(DisplayName, StringResources.Navigation_Category_Unassigned,
                                    StringComparison.InvariantCultureIgnoreCase)) == 0)
                    return -1;
                return String.Compare(DisplayName, other.DisplayName, StringComparison.InvariantCultureIgnoreCase);
            }
            return base.CompareTo(obj);
        }

        #endregion
    }
}