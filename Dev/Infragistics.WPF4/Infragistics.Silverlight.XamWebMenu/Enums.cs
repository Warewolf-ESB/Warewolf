namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// Defines the different roles that a <see cref="XamMenuItem"/> can have. 
    /// </summary>
    public enum MenuItemRole
    {
        /// <summary>
        /// Top-level menu item that can rise click event.
        /// </summary>
        TopLevelItem,

        /// <summary>
        /// Header for top-level menus. 
        /// </summary>
        TopLevelHeader,

        /// <summary>
        /// Menu item in a submenu that can rise click event.
        /// </summary>
        SubmenuItem,

        /// <summary>
        /// Header for a submenu. 
        /// </summary>
        SubmenuHeader
    }

    /// <summary>
    /// Defines the different custom positions of the submenu items can have. 
    /// </summary>
    public enum MenuItemPosition
    {
        /// <summary>
        /// The menu takes care about position of its popup. 
        /// </summary>
        Auto,

        /// <summary>
        /// The preferred position is on top of item. 
        /// </summary>
        Top,

        /// <summary>
        /// The preferred position is on bottom of item. 
        /// </summary>
        Bottom,

        /// <summary>
        /// The preferred position is on left of item. 
        /// </summary>
        Left,

        /// <summary>
        /// The preferred position is on right of item. 
        /// </summary>
        Right
    }

    #region ContextMenu Enumerations

    #region PlacementMode
    /// <summary>
    /// Describes the placement of where a <see cref="XamContextMenu"/> control appears on the screen.
    /// The target area of the <see cref="XamContextMenu"/> is the <see cref="XamContextMenu.PlacementRectangle"/> 
    /// which is relative to the <see cref="XamContextMenu.PlacementTarget"/>.
    /// If the <see cref="XamContextMenu.PlacementTarget"/> is not set, the element where the context menu is attached is used as a target.
    /// </summary>
    public enum PlacementMode
    {

        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control relative to the mouse position
        /// and at an offset that is defined by the <see cref="XamContextMenu.HorizontalOffset"/>
        /// and <see cref="XamContextMenu.VerticalOffset"/> property values.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        MouseClick,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control that aligns
        /// its lower edge with the upper edge of the target
        /// and aligns its left edge with the left edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        AlignedAbove,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control where
        /// the control aligns its upper edge with the lower edge of the target
        /// and aligns its left edge with the left edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        AlignedBelow,


        /// <summary>
        /// A <see cref="XamContextMenu"/> control that aligns its right
        /// edge with the left edge of the target
        /// and aligns its upper edge with the upper edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        AlignedToTheLeft,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control that aligns
        /// its left edge with the right edge of the target
        /// and aligns its upper edge with the upper edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        AlignedToTheRight,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control where
        /// it is centered over the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        Center,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control relative
        /// to the upper-left corner of the target
        /// and at an offset that is defined by the <see cref="XamContextMenu.HorizontalOffset"/>
        /// and <see cref="XamContextMenu.VerticalOffset"/> property values.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        Manual,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control that aligns
        /// its upper edge with the upper edge of the target
        /// and aligns its left edge with the left edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        AlignedTop,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control that aligns
        /// its lower edge with the lower edge of the target
        /// and aligns its left edge with the left edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        AlignedBottom,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control that aligns
        /// its left edge with the left edge of the target
        /// and aligns its upper edge with the upper edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        AlignedLeft,


        /// <summary>
        /// A position of the <see cref="XamContextMenu"/> control that aligns
        /// its right edge with the right edge of the target
        /// and aligns its upper edge with the upper edge of the target.
        /// If a screen edge obscures the <see cref="XamContextMenu"/>,
        /// the control repositions itself to align with the screen edge.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        AlignedRight
    }

    #endregion //PlacementMode

    #region ContextMenuOpeningMode

    /// <summary>
    /// Specifies on which event the <see cref="XamContextMenu"/> opens.
    /// </summary>
    public enum OpenMode
    {
        /// <summary>
        /// The <see cref="XamContextMenu"/> does not open when when the user clicks mouse button.
        /// In this case a <seealso cref="Infragistics.Controls.Menus.Primitives.OpenCommand"/> can be used to open the context menu.
        /// </summary>
        None = 0,

        /// <summary>
        /// The <see cref="XamContextMenu"/> opens using the left mouse button.
        /// </summary>
        LeftClick = 1,

        /// <summary>
        /// The <see cref="XamContextMenu"/> opens using the right mouse button.
        /// </summary>
        RightClick = 2
    }
    #endregion //ContextMenuOpeningMode

    #region XamContextMenuCommand
    /// <summary>
    ///  An enumeration of available commands for the <see cref="XamContextMenu"/> object.
    /// </summary>
    public enum XamContextMenuCommand
    {
        /// <summary>
        /// Opens the XamContextMenu
        /// </summary>
        Open = 0,

        /// <summary>
        /// Closes the XamContextMenu
        /// </summary>
        Close = 1
    }
    #endregion //XamContextMenuCommands

    #endregion //ContextMenu Enumerations
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