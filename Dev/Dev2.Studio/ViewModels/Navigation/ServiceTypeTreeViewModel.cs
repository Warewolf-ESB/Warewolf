using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// A node representing a specific type of service - ie, workflow, source or service
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class ServiceTypeTreeViewModel : AbstractTreeViewModel
    {
        #region private fields

        private ResourceType _resourceType;
        RelayCommand _showNewWorkflowWizard;

        #endregion private fields

        #region ctor + init

        public ServiceTypeTreeViewModel(IEventAggregator eventPublisher, ITreeNode parent, ResourceType resourceCategory)
            : base(eventPublisher, parent)
        {
            ResourceType = resourceCategory;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            IsExpanded = true;
            if(parent != null)
            {
                parent.IsExpanded = true;
            }
        }

        #endregion ctor + init

        #region public properties

        /// <summary>
        /// Gets the display name, by looking at the <see cref="ResourceType" />.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string DisplayName
        {
            get { return ResourceType.GetTreeDescription(); }
        }

        /// <summary>
        /// Gets the icon path, by looking at the <see cref="ResourceType" />.
        /// </summary>
        /// <value>
        /// The icon path.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string IconPath
        {
            get { return ResourceType.GetIconLocation(); }
        }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        /// <value>
        /// The type of the resource.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ResourceType ResourceType
        {
            get { return _resourceType; }
            set
            {
                if(_resourceType == value) return;

                _resourceType = value;
                NotifyOfPropertyChange(() => ResourceType);
                NotifyOfPropertyChange(() => DisplayName);
                NotifyOfPropertyChange(() => IconPath);
            }
        }

        /// <summary>
        /// Gets the environment model by walking up the tree.
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        /// <exception cref="System.InvalidOperationException">Cant be set</exception>
        public override IEnvironmentModel EnvironmentModel
        {
            get { return TreeParent.EnvironmentModel; }
            protected set { throw new InvalidOperationException(); }
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
                if(_children == value) return;

                _children = value;
                _children.CollectionChanged -= ChildrenOnCollectionChanged;
                _children.CollectionChanged += ChildrenOnCollectionChanged;
            }
        }

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
            var commandParameter = obj.ToString();
            this.TraceInfo("Publish message of type - " + typeof(ShowNewResourceWizard));
            EventPublisher.Publish(new ShowNewResourceWizard(commandParameter));
        }

        public override bool HasNewWorkflowMenu
        {
            get
            {
                return ResourceType == ResourceType.WorkflowService;
            }
        }

        public override bool HasExecutableCommands
        {
            get
            {
                return true;
            }
        }

        public override bool HasNewServiceMenu
        {
            get
            {
                return ResourceType == ResourceType.Service;
            }
        }

        public override bool HasNewSourceMenu
        {
            get
            {
                return ResourceType == ResourceType.Source;
            }
        }

        public override string DeployTitle
        {
            get
            {
                return "Deploy All " + DisplayName.ToUpper();
            }
        }

        public override bool CanDeploy
        {
            get
            {
                return ResourceType == ResourceType.WorkflowService || ResourceType == ResourceType.Source || ResourceType == ResourceType.Service;
            }
        }

        public override ICommand RenameCommand
        {
            get
            {
                //not implimented
                return null;
            }
        }

        #endregion public properties

        #region public methods

        /// <summary>
        /// Finds the child containing a specific resource (or environmentmodel).
        /// THis specific implementation returns if it is searched by <see cref="ResourceType" />
        /// </summary>
        /// <typeparam name="T">Type of the resource to find</typeparam>
        /// <param name="resourceToFind">The resource to find.</param>
        /// <returns></returns>
        /// <date>2013/01/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ITreeNode FindChild<T>(T resourceToFind)
        {
            if(resourceToFind is ResourceType)
            {
                var type = (ResourceType)Enum.Parse(typeof(ResourceType), resourceToFind.ToString());
                if(ResourceType == type)
                    return this;
            }
            return base.FindChild(resourceToFind);
        }

        /// <summary>
        ///     Adds the specified child to children.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override void Add(ITreeNode child)
        {
            child.TreeParent = this;
            Children.Add(child);
            if(child.DisplayName == StringResources.Navigation_Category_Unassigned)
            {
                Children.Move(Children.IndexOf(child), 0);
            }
        }

        #endregion public methods

        #region Implementation of IComparable

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/25</date>
        public override int CompareTo(object obj)
        {
            ServiceTypeTreeViewModel model = obj as ServiceTypeTreeViewModel;
            if(model != null)
            {
                var other = model;
                var order1 = ResourceType.GetDisplayOrder();
                var order2 = other.ResourceType.GetDisplayOrder();
                if(order1 != order2)
                    return order1.CompareTo(order2);
                return String.Compare(DisplayName, other.DisplayName, StringComparison.Ordinal);
            }
            return base.CompareTo(obj);
        }

        #endregion

        protected override ITreeNode CreateParent(string displayName)
        {
            throw new NotImplementedException();
        }
    }
}
