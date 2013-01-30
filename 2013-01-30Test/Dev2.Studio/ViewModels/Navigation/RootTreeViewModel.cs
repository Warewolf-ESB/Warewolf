//6180 CODEREVIEW - Please region you code

#region

using System;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

#endregion

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

        /// <summary>
        /// Initializes a new instance of the <see cref="RootTreeViewModel" /> class.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public RootTreeViewModel()
            : base(null)
        {
            IsExpanded = true;
        }

        #endregion ctor + init

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
    }
}