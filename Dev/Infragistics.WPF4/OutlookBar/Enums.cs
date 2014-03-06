using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.OutlookBar
{
    #region OutlookBarGroupLocation

    /// <summary>
    /// Specifies the location of an <see cref="OutlookBarGroup"/>.
    /// </summary>
	/// <seealso cref="OutlookBarGroup.Location"/>
    public enum OutlookBarGroupLocation
    {
        /// <summary>
		/// The <see cref="OutlookBarGroup"/> is not positioned anywhere within the <see cref="XamOutlookBar"/>, i.e. it is not currently in any of <see cref="OutlookBarGroup"/> collections (e.g., <see cref="XamOutlookBar.NavigationAreaGroups"/>, <see cref="XamOutlookBar.ContextMenuGroups"/>, <see cref="XamOutlookBar.OverflowAreaGroups"/>)
        /// </summary>
		/// <seealso cref="XamOutlookBar.NavigationAreaGroups"/>
		/// <seealso cref="XamOutlookBar.ContextMenuGroups"/>
		/// <seealso cref="XamOutlookBar.OverflowAreaGroups"/>
		None = 0,

        /// <summary>
		/// The <see cref="OutlookBarGroup"/>  is located on the navigation area.
        /// </summary>
		/// <seealso cref="XamOutlookBar.NavigationAreaGroups"/>
		NavigationGroupArea = 1,

        /// <summary>
		/// The <see cref="OutlookBarGroup"/>  is located on the overflow area.
        /// </summary>
		/// <seealso cref="XamOutlookBar.OverflowAreaGroups"/>
		OverflowArea = 2,

        /// <summary>
		/// The <see cref="OutlookBarGroup"/> will be displayed inside overflow context menu.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ContextMenuGroups"/>
		OverflowContextMenu = 3,
    }
    #endregion//OutlookBarGroupLocation

    #region VerticalSplitterLocation

    /// <summary>
	/// Specifies the location of <see cref="XamOutlookBar"/>'s vertical splitter.
    /// </summary>
	/// <seealso cref="XamOutlookBar.VerticalSplitterLocation"/>
	public enum VerticalSplitterLocation
    {
        /// <summary>
		/// The vertical splitter is on the left side of <see cref="XamOutlookBar"/> and the location of the <see cref="XamOutlookBar"/> in the minimized state is on the right side of window.
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>This setting will also cause the contents of a minimized <see cref="XamOutlookBar"/> to fly out to the left.</para>
		/// </remarks>
        Left = 0,

        /// <summary>
		/// The vertical splitter is on the right side of <see cref="XamOutlookBar"/> and the location of the <see cref="XamOutlookBar"/> in the minimized state is on the left side of window (default).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>This setting will also cause the contents of a minimized <see cref="XamOutlookBar"/> to fly out to the right.</para>
		/// </remarks>
		Right = 1,
    }


    #endregion //VerticalSplitterLocation

    #region SplitterResizeMode

    /// <summary>
	/// Determines how <see cref="XamOutlookBar"/> resizing is performed when the vertical splitter is dragged by the user. 
    /// </summary>
	/// <seealso cref="XamOutlookBar.VerticalSplitterResizeMode"/>
    public enum SplitterResizeMode
    {
        /// <summary>
		/// The <see cref="XamOutlookBar"/> is not resized until the splitter dragging is completed, while an alternate representation of the splitter is displayed and moved while the dragging is in progress.
        /// </summary>
        Deferred = 0,
        /// <summary>
		/// The <see cref="XamOutlookBar"/> is resized in real time as the splitter is dragged.
		/// </summary>
        Immediate = 1,
    }

    #endregion //SplitterResizeMode	
    
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