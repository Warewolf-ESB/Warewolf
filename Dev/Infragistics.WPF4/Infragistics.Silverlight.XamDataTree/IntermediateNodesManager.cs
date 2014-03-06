
namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A class that is used by the <see cref="XamDataTree"/> as the <see cref="NodesManager"/> for nodes representing header nodes.
    /// </summary>
    public class IntermediateNodesManager : NodesManager
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="IntermediateNodesManager"/> class.
        /// </summary>
        public IntermediateNodesManager(int level, NodeLayout nodeLayout, XamDataTreeNode parentLayoutNode)
            : base(level, nodeLayout, parentLayoutNode)
        {

        }
        #endregion // Constructor

        #region Overrides

        #region Methods

        #region GetDataItem
        /// <summary>
        /// Returns the <see cref="XamDataTreeNode"/> for the given index.
        /// </summary>
        /// <param propertyName="index">The index of the node to retrieve.</param>
        /// <returns></returns>
        protected override XamDataTreeNode GetDataItem(int index)
        {
            NodeLayout layout = this.NodeLayout.NodeLayouts[index];
            if (layout != null)
            {
                if (this.CachedNodes.ContainsKey(layout))
                    return this.CachedNodes[layout];
                else
                {
                    XamDataTreeNode node = new XamDataTreeNode(index, this, this.ParentNode.Data, true, layout);
                    this.CachedNodes.Add(layout, node);
                    return node;
                }
            }

            return null;
        }
        #endregion // GetDataItem

        #region EnsureDataManager

        /// <summary>
        /// This method checks to ensure that a DataManagerBase is created for a given level and if not creates it for that level.
        /// </summary>
        protected override void EnsureDataManager()
        {

        }

        #endregion // EnsureDataManager

        #endregion // Methods

        #region Properties

        #region DataCount
        /// <summary>
        /// Gets the amount of <see cref="NodeLayout"/>s in the <see cref="NodesManager"/>.
        /// </summary>
        protected override int DataCount
        {
            get
            {
                return this.NodeLayout.NodeLayouts.Count;
            }
        }
        #endregion // DataCount

        #region DataManager

        /// <summary>
        /// Gets a reference to the <see cref="DataManagerBase"/> of the <see cref="NodesManager"/>.
        /// </summary>
        protected internal override DataManagerBase DataManager
        {
            get
            {
                return null;
            }
        }

        #endregion // DataManager

        #endregion // Properties

        #endregion // Overrides
    }
}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved