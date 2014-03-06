using System;

namespace Infragistics.Controls.Interactions
{
	/// <summary>
	/// A class listing the information needed during the <see cref="XamDialogWindow.Moved"/> event.
	/// </summary>
    public class MovedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Left coordinate property
        /// </summary>
        public double Left { get; internal set; }

        /// <summary>
        /// Gets or sets the Top coordinate property
        /// </summary>
        public double Top { get; internal set; }
    }

	/// <summary>
	/// A class listing the information needed during the <see cref="XamDialogWindow.Moving"/> event.
	/// </summary>
	/// <remarks>This event can be cancelled</remarks>
    public class MovingEventArgs : CancellableEventArgs
    {
        /// <summary>
        /// Gets or sets the Left coordinate property
        /// </summary>
        public double Left { get; internal set; }

        /// <summary>
        /// Gets or sets the Top coordinate property
        /// </summary>
        public double Top { get; internal set; }
    }

	/// <summary>
	/// A class listing the information needed during the <see cref="XamDialogWindow.WindowStateChanging"/> event.
	/// </summary>
	/// <remarks>This event can be cancelled</remarks>
    public class WindowStateChangingEventArgs : CancellableEventArgs
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowStateChangingEventArgs"/> class.
        /// </summary>
        /// <param name="currentWindowState">Current state of the window.</param>
        /// <param name="newWindowState">New state of the window.</param>
        public WindowStateChangingEventArgs(WindowState currentWindowState, WindowState newWindowState)
        {
            this.CurrentWindowState = currentWindowState;
            this.NewWindowState = newWindowState;
        }
        #endregion Constructor

		#region NewWindowState
		/// <summary>
		/// Gets/Sets the new <see cref="WindowState"/> of the window.
		/// </summary>
        public WindowState NewWindowState { get; set; }
		#endregion // NewWindowState

		#region CurrentWindowState
		/// <summary>
		/// Gets/Sets the current <see cref="WindowState"/> of the window.
		/// </summary>
        public WindowState CurrentWindowState { get; set; }
		#endregion // CurrentWindowState
	}

	/// <summary>
	/// A class listing the information needed during the <see cref="XamDialogWindow.WindowStateChanged"/> event.
	/// </summary>
    public class WindowStateChangedEventArgs : EventArgs
	{
		#region Constructor
		/// <summary>
        /// Initializes a new instance of the <see cref="WindowStateChangedEventArgs"/> class.
        /// </summary>
		/// <param name="newWindowState">New state of the window.</param>
		/// <param name="previousWindowState">Previous state of the window.</param>
        public WindowStateChangedEventArgs(WindowState newWindowState, WindowState previousWindowState)
        {
			this.NewWindowState = newWindowState;
            this.PreviousWindowState = previousWindowState;
		}
		#endregion // Constructor

		#region CurrentWindowState
		/// <summary>
        /// Gets/Sets the previous <see cref="WindowState"/> of the window.
        /// </summary>
        public WindowState PreviousWindowState { get; private set; }
		#endregion // CurrentWindowState

		#region NewWindowState
		/// <summary>
		/// Gets/Sets the new <see cref="WindowState"/> of the window.
		/// </summary>
		public WindowState NewWindowState { get; private set; }
		#endregion // NewWindowState
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