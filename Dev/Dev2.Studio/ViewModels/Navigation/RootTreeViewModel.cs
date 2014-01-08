using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// Treenode representing the root node
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class RootTreeViewModel : AbstractTreeViewModel
    {
        #region ctor + init

        public RootTreeViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher, null)
        {
        }

        #endregion ctor + init

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is filtered from the tree.
        ///     Always false for root node
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is filtered; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool IsFiltered
        {
            get
            {
                return false;
            }
            set
            {
                //Do Nothing
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is expanded in the tree, always true for rootnode
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        /// <date>2013/01/23</date>
        /// <author>Jurie.smit</author>
        /// <date>3/1/2013</date>
        public override bool IsExpanded
        {
            get
            {
                return true;
            }
            set
            {
                //Do nothing - cant collapse root node
            }
        }

        #region public properties

        /// <summary>
        /// Gets the environment model, which is always null
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        /// <exception cref="System.InvalidOperationException">Cant set</exception>
        public override IEnvironmentModel EnvironmentModel
        {
            get { return null; }
            protected set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string DisplayName
        {
            get { return StringResources.Navigation_Folder_Icon_Pack_Uri; }
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <value>
        /// The icon path.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string IconPath
        {
            get { return StringResources.Navigation_RootNode_Name; }
        }

        #endregion public properties

        protected override ITreeNode CreateParent(string displayName)
        {
            throw new NotImplementedException();
        }
    }
}