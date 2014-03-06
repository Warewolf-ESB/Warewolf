using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Editors;
using System.ComponentModel;

namespace Infragistics.Windows.Editors
{
	/// <summary>
	/// Represents the header for a specific <see cref="CalendarItemGroup"/> in a <see cref="XamMonthCalendar"/>
	/// </summary>
    //[System.ComponentModel.ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CalendarItemGroupTitle : ContentControl
	{
		#region Constructor

		static CalendarItemGroupTitle()
		{
			UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(typeof(CalendarItemGroupTitle)));
            Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
            Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(CalendarItemGroupTitle), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));
        } 
		#endregion //Constructor

        #region Base class overrides

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an <see cref="AutomationPeer"/> that represents the element
        /// </summary>
        /// <returns>A <see cref="CalendarItemGroupTitleAutomationPeer"/> instance</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CalendarItemGroupTitleAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #endregion //Base class overrides
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