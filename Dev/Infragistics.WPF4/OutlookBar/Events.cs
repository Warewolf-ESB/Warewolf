using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Windows.Controls.Events;

namespace Infragistics.Windows.OutlookBar.Events
{
    #region SelectedGroupChangedEventArgs

    /// <summary>
    /// Event arguments for <see cref="XamOutlookBar.SelectedGroupChangedEvent"/> event of the <see cref="XamOutlookBar"/>.
    /// </summary>
    public class SelectedGroupChangedEventArgs : RoutedEventArgs
    {
        #region Member Variables
        
        OutlookBarGroup _previousSelectedOutlookBarGroup;
        OutlookBarGroup _newSelectedOutlookBarGroup;

        #endregion //Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedGroupChangedEventArgs"/> class.
        /// </summary>
		/// <param name="previousSelectedOutlookBarGroup">The previous selected <see cref="OutlookBarGroup"/> in the <see cref="XamOutlookBar"/>.</param>
		/// <param name="newSelectedOutlookBarGroup">The new selected <see cref="OutlookBarGroup"/> in the <see cref="XamOutlookBar"/>.</param>
        /// <remarks>See <see cref="XamOutlookBar.SelectedGroupChanged"/> for more information.</remarks>
        public SelectedGroupChangedEventArgs(
            OutlookBarGroup previousSelectedOutlookBarGroup,
            OutlookBarGroup newSelectedOutlookBarGroup)
        {
            this._newSelectedOutlookBarGroup = newSelectedOutlookBarGroup;
            this._previousSelectedOutlookBarGroup = previousSelectedOutlookBarGroup;
        }
 
	    #endregion //Constructors

        #region Properties
        #region Public Properties

        #region NewSelectedOutlookBarGroup

        /// <summary>
		/// The new selected <see cref="OutlookBarGroup"/> in the <see cref="XamOutlookBar"/>.
        /// </summary>
		/// <seealso cref="PreviousSelectedOutlookBarGroup"/>
		public OutlookBarGroup NewSelectedOutlookBarGroup
        {
            get { return _newSelectedOutlookBarGroup; }
        }

        #endregion //NewSelectedOutlookBarGroup	
    
        #region PreviousSelectedOutlookBarGroup

        /// <summary>
		/// The previous selected <see cref="OutlookBarGroup"/> in the <see cref="XamOutlookBar"/>.
        /// </summary>
		/// <seealso cref="NewSelectedOutlookBarGroup"/>
		public OutlookBarGroup PreviousSelectedOutlookBarGroup
        {
            get { return _previousSelectedOutlookBarGroup; }
        }

        #endregion //PreviousSelectedOutlookBarGroup	
    
        #endregion //Public Properties    
        #endregion //Properties
    }

    #endregion //SelectedGroupChangedEventArgs

    #region SelectedGroupChangingEventArgs
    /// <summary>
    /// Event arguments for <see cref="XamOutlookBar.SelectedGroupChanging"/> event of the <see cref="XamOutlookBar"/>
    /// </summary>
    /// <remarks>See <see cref="XamOutlookBar.SelectedGroupChanging"/> for more information.</remarks>
    public class SelectedGroupChangingEventArgs : CancelableRoutedEventArgs
    {
        #region Member Variables

        OutlookBarGroup _currentSelectedOutlookBarGroup;// cuurent group
        OutlookBarGroup _newSelectedOutlookBarGroup;   // waiting for approval

        #endregion //Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <see cref="SelectedGroupChangingEventArgs"/> class
        /// </summary>
		/// <param name="currentSelectedOutlookBarGroup">The currently selected <see cref="OutlookBarGroup"/></param>
		/// <param name="newSelectedOutlookBarGroup">The new selected <see cref="OutlookBarGroup"/></param>
        public SelectedGroupChangingEventArgs(
            OutlookBarGroup currentSelectedOutlookBarGroup,
            OutlookBarGroup newSelectedOutlookBarGroup)
        {
            this.Cancel= false;
            this._currentSelectedOutlookBarGroup = currentSelectedOutlookBarGroup;
            this._newSelectedOutlookBarGroup = newSelectedOutlookBarGroup;
        }

        #endregion //Constructors

        #region Properties
        #region Public Properties

        #region CurrentSelectedOutlookBarGroup

        /// <summary>
		/// The currently selected <see cref="OutlookBarGroup"/>.
        /// </summary>
		/// <seealso cref="NewSelectedOutlookBarGroup"/>
		public OutlookBarGroup CurrentSelectedOutlookBarGroup
        {
            get { return _currentSelectedOutlookBarGroup; }
        }

        #endregion //CurrentSelectedOutlookBarGroup	
    
        #region NewSelectedOutlookBarGroup

        /// <summary>
		/// The new selected <see cref="OutlookBarGroup"/>.
        /// </summary>
		/// <seealso cref="CurrentSelectedOutlookBarGroup"/>
        public OutlookBarGroup NewSelectedOutlookBarGroup
        {
            get { return _newSelectedOutlookBarGroup; }
        }

        #endregion //NewSelectedOutlookBarGroup	
    
        #endregion //Public Properties    
        #endregion //Properties
    }
    #endregion //SelectedGroupChangingEventArgs

    #region OutlookBarCancelableRoutedEventArgs
    /// <summary>
    /// A class for routed events that are cancelable
    /// </summary>
    /// <remarks>See <see cref="XamOutlookBar.SelectedGroupChanging"/> 
    /// <see cref="XamOutlookBar.NavigationPaneMinimizingEvent"/>
    /// <see cref="XamOutlookBar.NavigationPaneExpandingEvent"/>
    /// <see cref="XamOutlookBar.SelectedGroupPopupOpening"/>
    /// <see cref="XamOutlookBar.SelectedGroupChanging"/>
    /// for more information.</remarks>
    public class OutlookBarCancelableRoutedEventArgs : CancelableRoutedEventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="OutlookBarCancelableRoutedEventArgs"/>
        /// </summary>
        public OutlookBarCancelableRoutedEventArgs()
        {
            this.Cancel= false;
        }

        #endregion //Constructors
    }
    #endregion //OutlookBarCancelableRoutedEventArgs
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