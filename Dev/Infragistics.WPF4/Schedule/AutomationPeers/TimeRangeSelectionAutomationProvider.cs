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
using Infragistics.Controls.Schedules;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Diagnostics;
using System.Collections.Generic;

namespace Infragistics.AutomationPeers
{
	#region IRawElementProviderSource interface

	internal interface IRawElementProviderSource
	{
		IRawElementProviderSimple GetProviderForPeer(AutomationPeer peer);
	}

	#endregion //IRawElementProviderSource interface

	#region TimeRangeSelectionAutomationProvider class

	internal class TimeRangeSelectionAutomationProvider : object
	, ISelectionProvider
	{
		#region Member Variables

		private FrameworkElementAutomationPeer _peer;
		private IRawElementProviderSource _rawElementSource;
		#endregion // Member Variables

		#region Constructor
		internal TimeRangeSelectionAutomationProvider(FrameworkElementAutomationPeer peer, IRawElementProviderSource rawElementSource)
		{
			CoreUtilities.ValidateNotNull(peer, "peer");
			CoreUtilities.ValidateNotNull(rawElementSource, "rawElementSource");
			_peer = peer;
			_rawElementSource = rawElementSource;


			SelectionChangeListener listener = new SelectionChangeListener(peer);
		}
		#endregion // Constructor

		#region ISelectionProvider Members

		bool ISelectionProvider.CanSelectMultiple
		{
			get { return true; }
		}

		IRawElementProviderSimple[] ISelectionProvider.GetSelection()
		{
			CalendarGroupItemsPresenterBase owner = _peer.Owner as CalendarGroupItemsPresenterBase;

			ScheduleControlBase control = ScheduleUtilities.GetControl(_peer.Owner);

			Debug.Assert(control != null);
			Debug.Assert(owner != null);

			if (control == null || owner == null || !control.SelectedTimeRange.HasValue)
				return new IRawElementProviderSimple[0];

			DateRange selectedTimeRange = control.SelectedTimeRangeNormalized.Value;
			List<IRawElementProviderSimple> elementProviders = new List<IRawElementProviderSimple>();

			DayViewDayHeaderArea dayHeaderArea = owner as DayViewDayHeaderArea;

			if (dayHeaderArea != null)
				this.AppendSelectedElements(selectedTimeRange, elementProviders, dayHeaderArea);
			else
			{
				ScheduleItemsPanel panel = owner.ItemsPanel;

				UIElementCollection children = panel.Children;

				foreach (UIElement child in children)
				{
					if (child.Visibility == Visibility.Collapsed)
						continue;

					ITimeRangeArea trArea = child as ITimeRangeArea;
					
					if (trArea == null)
						continue;

					this.AppendSelectedElements(selectedTimeRange, elementProviders, trArea);

				}
			}

			return elementProviders.ToArray();
		}

		private void AppendSelectedElements(DateRange selectedTimeRange, List<IRawElementProviderSimple> elementProviders, ITimeRangeArea trArea)
		{
			TimeslotPanelBase tsPanel = trArea.TimeRangePanel;

			if (tsPanel == null)
				return;

			foreach (UIElement ts in tsPanel.Children)
			{
				TimeRangePresenterBase trp = ts as TimeRangePresenterBase;

				if (trp == null || trp.Visibility == Visibility.Collapsed)
					continue;

				if (selectedTimeRange.IntersectsWithExclusive(trp.LocalRange))
					elementProviders.Add(_rawElementSource.GetProviderForPeer(FrameworkElementAutomationPeer.CreatePeerForElement(trp)));
			}
		}

		bool ISelectionProvider.IsSelectionRequired
		{
			get { return false; }
		}

		#endregion

		#region SelectionChangeListener private class

		private class SelectionChangeListener
		{
			#region Private Members

			private WeakReference _peerRef;
			private ScheduleControlBase _control;

			#endregion //Private Members

			#region Constructor

			internal SelectionChangeListener(FrameworkElementAutomationPeer peer)
			{
				_peerRef = new WeakReference(peer);

				_control = ScheduleUtilities.GetControl(peer.Owner);

				Debug.Assert(_control != null);

				// listen for SelectedTimeRangeChanged event
				if (_control != null)
					_control.SelectedTimeRangeChanged += OnSelectedTimeRangeChanged;
			}

			#endregion //Constructor

			#region OnSelectedTimeRangeChanged

			private void OnSelectedTimeRangeChanged(object sender, RoutedPropertyChangedEventArgs<DateRange?> e)
			{
				CalendarGroupTimeslotAreaAutomationPeer peer = CoreUtilities.GetWeakReferenceTargetSafe(_peerRef) as CalendarGroupTimeslotAreaAutomationPeer;

				if (peer != null)
					peer.RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
				else
					_control.SelectedTimeRangeChanged -= OnSelectedTimeRangeChanged;
			}

			#endregion //OnSelectedTimeRangeChanged
		}

		#endregion //SelectionChangeListener private class

	}

	#endregion //TimeRangeSelectionAutomationProvider class
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