namespace Infragistics.Controls.Interactions
{

    /// <summary>
    /// An Enum that describes startup position for a XamDialogWindow. 
    /// </summary>
    public enum StartupPosition
    {






        /// <summary>
        /// Startup position with values for Top and Left properties, relative to the container 
        /// </summary>

        Manual = 0,







        /// <summary>
        /// Startup position in the center of the container.
        /// </summary>

        Center = 1
    }

    /// <summary>
    /// An Enum that describes the state of the XamDialogWindow. 
    /// </summary>
    public enum WindowState
    {
        /// <summary>
        /// XamDialogWindow is in normal state: IsMinimized=false, IsMaximized=false
        /// </summary>
        Normal,

        /// <summary>
        /// XamDialogWindow is minimized
        /// </summary>
        Minimized,
			
        /// <summary>
        /// XamDialogWindow is maximized
        /// </summary>
        Maximized,	

        /// <summary>
        /// XamDialogWindow is hidden
        /// </summary>
        Hidden
    }

    #region XamDialogWindowCommand
    /// <summary>
    /// An enumeration of available commands for the <see cref="XamDialogWindow"/> object.
    /// </summary>
    public enum XamDialogWindowCommand
    {
        /// <summary>
        /// Maximizes the <see cref="XamDialogWindow"/> object.
        /// </summary>
        Maximize,

        /// <summary>
        /// Minimizes the <see cref="XamDialogWindow"/> object.
        /// </summary>
        Minimize,

		/// <summary>
		/// Toggles between the minimized and normal window state. 
		/// </summary>
		ToggleMinimize,

		/// <summary>
		/// Toggles between the maximized and normal window state
		/// </summary>
		ToggleMaximize,

		/// <summary>
		/// Puts the <see cref="XamDialogWindow"/> into the normal window state. 
		/// </summary>
		Restore, 

        /// <summary>
        /// Closes the <see cref="XamDialogWindow"/> object.
        /// </summary>
        Close,



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //XamDialogWindowCommand
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