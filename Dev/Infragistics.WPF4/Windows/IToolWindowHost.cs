using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace Infragistics.Windows.Controls
{
	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	/// <summary>
	/// Interface implemented by a class that hosts the <see cref="ToolWindow"/>
	/// </summary>
	internal interface IToolWindowHost
	{
		/// <summary>
		/// Used to trigger the initiation of a drag operation.
		/// </summary>
        /// <param name="e">The mouse event args available when the drag started.</param>
        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        //void DragMove();
        void DragMove(MouseEventArgs e);

		/// <summary>
		/// Used to trigger the initiation of a resize operation.
		/// </summary>
		/// <param name="location">The type of resize operation</param>
		/// <param name="cursor">The cursor to use during the drag operation</param>
        /// <param name="e">The mouse event args available when the drag started.</param>
        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        //void DragResize(ToolWindowResizeElementLocation location, Cursor cursor);
        void DragResize(ToolWindowResizeElementLocation location, Cursor cursor, MouseEventArgs e);

		/// <summary>
		/// Used to activate the specified window.
		/// </summary>
		void Activate();

		/// <summary>
		/// Used to close/unload the window.
		/// </summary>
		void Close();

		/// <summary>
		/// Used to notify the host that the relative position state of the item has changed.
		/// </summary>
		void RelativePositionStateChanged();

		// AS 5/14/08 BR32842
		// Moved to interface
		// 
		/// <summary>
		/// Moves the toolwindow to the front of the zorder.
		/// </summary>
		void BringToFront();

        // AS 10/13/08 TFS6107/BR34010
        bool HandlesDelayedMinMaxRequests { get; }

        // AS 3/30/09 TFS16355 - WinForms Interop
        bool IsWindow { get; }

		// AS 9/11/09 TFS21330
		// Used to indicate if the host currently allows transparency.
		bool AllowsTransparency { get; }

		// used to indicate if the host can use the specified transparency
		bool SupportsAllowTransparency(bool allowsTransparency);

		// AS 8/4/11 TFS83465/TFS83469
		void EnsureOnScreen(bool fullyInView);

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		Rect GetRestoreBounds();
		void SetRestoreBounds(Rect rect);
		void SetWindowState(WindowState newState);

		// AS 6/8/11 TFS76337
		Rect GetWindowBounds();

		// AS 11/17/11 TFS91061
		bool IsUsingOsNonClientArea { get; }
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