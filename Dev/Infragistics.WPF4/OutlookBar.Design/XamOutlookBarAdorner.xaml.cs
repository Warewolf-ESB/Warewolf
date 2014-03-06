using System.Windows;
using System.Windows.Controls;
using Microsoft.Windows.Design;

namespace Infragistics.Windows.Design.OutlookBar
{
    /// <summary>
    /// Represents an adorner for XamOutlookBar
    /// </summary>
	[ToolboxBrowsable(false)]	// JM 10-20-08
	public partial class XamOutlookBarAdorner : UserControl
    {
        #region Constructor

        /// <summary>
        /// Creates a <see cref="XamOutlookBarAdorner"/>
        /// </summary>
        public XamOutlookBarAdorner()
        {
            InitializeComponent();
        }

        #endregion //Constructor	
        
        #region Events

        #region ClosePopupEvent

        /// <summary>
        /// Event ID for the <see cref="ClosePopupEvent"/> routed event
        /// </summary>
        public static readonly RoutedEvent ClosePopupEvent = EventManager.RegisterRoutedEvent("ClosePopupEvent", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(XamOutlookBarAdorner));

        /// <summary>
        /// Occurs when the popup for has closed up.
        /// </summary>
        public event RoutedEventHandler ClosePopup
        {
            add { AddHandler(ClosePopupEvent, value); }
            remove { RemoveHandler(ClosePopupEvent, value); }
        }

        /// <summary>
        /// This method raises the ClosePopupEvent
        /// </summary>
        protected void RaiseClosePopupEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(XamOutlookBarAdorner.ClosePopupEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion //ClosePopupEvent

        #endregion //Events	

        #region Event Handlers

        #region btnClose_Click

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            RaiseClosePopupEvent();
        }

        #endregion //btnClose_Click

        #endregion //Event Handlers	
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