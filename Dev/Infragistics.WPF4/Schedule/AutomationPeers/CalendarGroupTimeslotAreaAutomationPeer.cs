using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Automation.Peers;
using Infragistics.Controls.Schedules.Primitives;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Primitives;
using System.Windows.Automation;
using System.Collections.Generic;
using System.Diagnostics;
using Infragistics.Controls.Schedules;
using System.Windows.Threading;

namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// Exposes <see cref="CalendarGroupTimeslotArea"/> types to UI Automation
	/// </summary>
	public class CalendarGroupTimeslotAreaAutomationPeer : FrameworkElementAutomationPeer, IRawElementProviderSource
	{
		#region Member Variables

		private ScrollInfoAutomationProvider _scrollProvider;
		private TimeRangeSelectionAutomationProvider _selectionProvider;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CalendarGroupTimeslotAreaAutomationPeer"/>
		/// </summary>
		/// <param name="owner">The <see cref="CalendarGroupTimeslotArea"/> that the peer represents</param>
		public CalendarGroupTimeslotAreaAutomationPeer(CalendarGroupTimeslotArea owner)
			: base(owner)
		{
		}

		#endregion // Constructor

		#region Base class overrides

		#region Methods
    
		#endregion //Methods	
        
		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Custom</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Custom;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="CalendarGroupTimeslotArea"/>
		/// </summary>
		/// <returns>A string that contains 'CalendarGroupTimeslotArea'</returns>
		protected override string GetClassNameCore()
		{
			return "CalendarGroupTimeslotArea";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern for the element that is associated with this peer.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		/// <returns>The pattern provider or null</returns>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Scroll)
			{
				this.ScrollProvider.UpdateAutomationInfo();
				return this.ScrollProvider;
			}

			if (patternInterface == PatternInterface.Selection)
				return this.SelectionProvider;

			return base.GetPattern(patternInterface);
		}
		#endregion // GetPattern

		#endregion //Base class overrides

		#region Properties

		#region ScrollProvider
		private ScrollInfoAutomationProvider ScrollProvider
		{
			get
			{
				if (_scrollProvider == null)
				{
					ScheduleControlBase ctrl = ScheduleControlBase.GetControl(this.Owner);

					ScrollInfo hScroll = null;
					ScrollInfo vScroll = null;

					if (null != ctrl)
						ctrl.GetCalendarGroupAutomationScrollInfo(out hScroll, out vScroll);

					_scrollProvider = new ScrollInfoAutomationProvider(this, hScroll, vScroll);
				}

				return _scrollProvider;
			}
		}
		#endregion // ScrollProvider

		#region SelectionProvider
		private TimeRangeSelectionAutomationProvider SelectionProvider
		{
			get
			{
				if (_selectionProvider == null)
					_selectionProvider = new TimeRangeSelectionAutomationProvider(this, this);

				return _selectionProvider;
			}
		}
		#endregion // SelectionProvider

		#endregion // Properties

		#region IRawElementProviderSource Members

		IRawElementProviderSimple IRawElementProviderSource.GetProviderForPeer(AutomationPeer peer)
		{
			return this.ProviderFromPeer(peer);
		}

		#endregion
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