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
using Infragistics.Controls.Schedules;
using Infragistics.Controls.Schedules.Primitives;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using Infragistics.Controls;

namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// Exposes <see cref="TimeRangePresenterBase"/> types to UI Automation
	/// </summary>
	public class TimeRangePresenterBaseAutomationPeer : FrameworkElementAutomationPeer
		, ISelectionItemProvider
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeRangePresenterBaseAutomationPeer"/>
		/// </summary>
		/// <param name="owner">The <see cref="TimeRangePresenterBase"/> that the peer represents</param>
		public TimeRangePresenterBaseAutomationPeer(TimeRangePresenterBase owner)
			: base(owner)
		{
		}
		#endregion // Constructor

		#region Base class overrides

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
		/// Returns the name of the <see cref="TimeRangePresenterBase"/>
		/// </summary>
		/// <returns>A string that contains 'TimeRangePresenterBase'</returns>
		protected override string GetClassNameCore()
		{
			return this.Owner.GetType().Name;
		}

		#endregion //GetClassNameCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label of the element that is associated with this peer.
		/// </summary>
		/// <returns>The automation name of the element or the name of the associated calendar.</returns>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();

			if (string.IsNullOrEmpty(name))
			{
				TimeslotBase ts = this.Element.Timeslot;

				if (null != ts)
					name = string.Format("{0:g}-{1:g}", ts.Start, ts.End); 
			}

			return name;
		}
		#endregion // GetNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern for the element that is associated with this peer.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		/// <returns>The pattern provider or null</returns>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.SelectionItem)
			{
				TimeRangePresenterBase element = this.Element;

				if (element is TimeslotHeader)
					return null;

				return this;
			}

			return base.GetPattern(patternInterface);
		}
		#endregion // GetPattern

		#endregion //Base class overrides

		#region Properties

		#region Element
		internal TimeRangePresenterBase Element
		{
			get { return (TimeRangePresenterBase)this.Owner; }
		}
		#endregion // Element

		#endregion // Properties

		#region ISelectionItemProvider Members

		void ISelectionItemProvider.AddToSelection()
		{
			if (!this.IsEnabled())
				throw new ElementNotEnabledException();

			if (((ISelectionItemProvider)this).IsSelected)
				return;

			TimeRangePresenterBase trp = this.Element;

			ScheduleControlBase control = trp.Control;

			if (control != null)
				control.SetSelectedTimeRange(trp.LocalRange, true);
		}

		bool ISelectionItemProvider.IsSelected
		{
			get 
			{
				TimeRangePresenterBase trp = this.Element;

				ScheduleControlBase control = trp.Control;

				if (control == null)
					return false;

				DateRange? selectedRange = control.SelectedTimeRangeNormalized;
				DateRange trpRange = trp.LocalRange;

				return selectedRange.HasValue &&
						selectedRange.Value.Start <= trpRange.Start &&
						selectedRange.Value.End >= trpRange.End;
			}
		}

		void ISelectionItemProvider.RemoveFromSelection()
		{
			if (!this.IsEnabled())
				throw new ElementNotEnabledException();

			TimeRangePresenterBase trp = this.Element;

			if (!((ISelectionItemProvider)this).IsSelected)
				return;

			ScheduleControlBase control = trp.Control;

			if (control != null)
			{
				DateRange? selectedTimeRange = control.SelectedTimeRange;

				if (!selectedTimeRange.HasValue)
					return;

				DateRange trpRange = trp.LocalRange;
				DateRange? newRange = null;
				if (selectedTimeRange.Value.Start <= selectedTimeRange.Value.End)
				{
					if ( trpRange.Start >= selectedTimeRange.Value.Start )
						newRange = new DateRange(selectedTimeRange.Value.Start, trpRange.Start);
				}
				else
				{
					if ( trpRange.Start >= selectedTimeRange.Value.End )
						newRange = new DateRange(selectedTimeRange.Value.End, trpRange.Start);
				}

				if (newRange.HasValue)
					control.SetSelectedTimeRange(newRange.Value, false);
				else
				{
					var anchor = control.GetSelectionAnchor();

					if ( anchor != null )
						control.SetSelectedTimeRange(anchor.Value, false);
				}
			}
		}

		void ISelectionItemProvider.Select()
		{
			if (!this.IsEnabled())
				throw new ElementNotEnabledException();

			TimeRangePresenterBase trp = this.Element;

			ScheduleControlBase control = trp.Control;

			if (control != null)
				control.SetSelectedTimeRange(trp.LocalRange, false);
		}

		IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
		{
			get
			{
				if (this.Element is MultiDayActivityArea)
				{
					DayViewDayHeaderArea dvdArea = PresentationUtilities.GetVisualAncestor<DayViewDayHeaderArea>(this.Element, null);

					if (dvdArea != null)
						return this.ProviderFromPeer(FrameworkElementAutomationPeer.CreatePeerForElement(dvdArea));
				}

				CalendarGroupTimeslotArea tsArea = PresentationUtilities.GetVisualAncestor<CalendarGroupTimeslotArea>(this.Element, null);

				if (tsArea != null)
					return this.ProviderFromPeer(FrameworkElementAutomationPeer.CreatePeerForElement(tsArea));

				return null;
			}
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