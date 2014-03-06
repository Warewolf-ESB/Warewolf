
namespace Infragistics.Controls.Menus
{
    #region TreeInvokeAction

    /// <summary>
    /// An enumeration of different interactions.
    /// </summary>
    public enum TreeInvokeAction
    {
        /// <summary>
        /// An action was raised via the keyboard. 
        /// </summary>
        Keyboard,

        /// <summary>
        /// An action was raised via the mouse moving. 
        /// </summary>
        MouseMove,

        /// <summary>
        /// An action was raised via a MouseDown or Spacebar press.
        /// </summary>
        Click,

        /// <summary>
        /// An action was raised via the API.
        /// </summary>
        Code
    }
    #endregion // TreeInvokeAction

    #region TreeSelectionType

    /// <summary>
    /// Describes the type of selection that should be performed. 
    /// </summary>
    public enum TreeSelectionType
    {
        /// <summary>
        /// Selection should be disabled
        /// </summary>
        None = 0,

        /// <summary>
        /// Only one item should be selected at a given time. 
        /// </summary>
        Single,

        /// <summary>
        /// Multiple items can be selected via the ctrl and shift keys. 
        /// </summary>
        Multiple,
    }

    #endregion // TreeSelectionType

    #region DragSelectType
    /// <summary>
    /// Describes the type of drag selection that should occur in the <see cref="XamDataTree"/>
    /// </summary>
    public enum TreeDragSelectType
    {
        /// <summary>
        /// A DragSelect operation shouldn't occur.
        /// </summary>
        None,

        /// <summary>
        /// Drag selection will select nodes.
        /// </summary>
        Node
    }
    #endregion // DragSelectType

    #region TreeCheckBoxMode

    /// <summary>
    /// Specifies how the CheckBox will be used on the <see cref="XamDataTree"/>.
    /// </summary>
    public enum TreeCheckBoxMode
    {
        /// <summary>
        /// Each <see cref="XamDataTreeNode"/>'s CheckBox is independent of every other XamTreeItem in the <see cref="XamDataTree"/>
        /// </summary>
        Manual,

        /// <summary>
        /// In this mode all leaf <see cref="XamDataTreeNode"/>s have two state checkboxes, and all other <see cref="XamDataTreeNode"/>s are tristate.
        /// The checked state of an item will cause its children to placed in the same state and its parent to be notified.
        /// If all of the children of the parent are in the same state, the parent will be in the same state, otherwise it will be in a third state.
        /// </summary>
        Auto
    }

    #endregion // TreeCheckBoxMode

    #region TreeMouseEditingAction

    /// <summary>
    /// Describes the type of action that can cause a <see cref="XamDataTreeNode" /> to enter edit mode with a mouse.
    /// </summary>
    public enum TreeMouseEditingAction
    {
        /// <summary>
        /// Clicking on a <see cref="XamDataTreeNode" /> will cause it to enter edit mode.
        /// </summary>
        SingleClick,

        /// <summary>
        /// Double clicking on a <see cref="XamDataTreeNode" /> will cause it to enter edit mode.
        /// </summary>
        DoubleClick,

        /// <summary>
        /// No mouse action will cause a <see cref="XamDataTreeNode" /> to enter edit mode.
        /// </summary>
        None
    }

    #endregion // TreeMouseEditingAction

    #region NodeLineTemination

    /// <summary>
    /// An enumeration that describes what type of node connector should appear attached to a leaf node.
    /// </summary>
    public enum NodeLineTemination
    {
        /// <summary>
        /// No terminator should appear.
        /// </summary>
        None,

        /// <summary>
        /// A connector attaching the node to the previous node and the following node should appear.
        /// </summary>
        TShape,

        /// <summary>
        /// A connector attaching the node to the previous node should appear.
        /// </summary>
        LShape
    }

    #endregion // NodeLineTemination

    #region TreeDropDestination

    /// <summary>
    /// An enumeration that describes where an object is dropped in relation to it's drop target.
    /// </summary>
    public enum TreeDropDestination
    {
        /// <summary>
        /// The <see cref="XamDataTreeNode"/> is dropped before the destination object.
        /// </summary>
        DropBefore,

        /// <summary>
        /// The <see cref="XamDataTreeNode"/> is dropped after the destination object.
        /// </summary>
        DropAfter,

        /// <summary>
        /// The <see cref="XamDataTreeNode"/> is dropped onto the destination object.
        /// </summary>
        DropOnto
    }

    #endregion // TreeDropDestination
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