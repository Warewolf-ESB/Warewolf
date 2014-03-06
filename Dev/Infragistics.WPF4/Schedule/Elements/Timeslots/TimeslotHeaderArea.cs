using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.AutomationPeers;
using System.ComponentModel;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the timeline for a specific time zone in a <see cref="ScheduleTimeControlBase"/>
	/// </summary>
	[TemplatePart(Name = PartTimeslotPanel, Type = typeof(TimeslotPanel))]
	[DesignTimeVisible(false)]
	public class TimeslotHeaderArea : Control
		, ITimeRangeArea
	{
		#region Member Variables

		private const string PartTimeslotPanel = "TimeslotPanel";
		private TimeslotPanel _timeslotPanel;
		private Visibility _currentTimeIndicatorVisibility = Visibility.Collapsed;

		#endregion //Member Variables

		#region Constructor
		static TimeslotHeaderArea()
		{

			UIElement.FocusableProperty.OverrideMetadata(typeof(TimeslotHeaderArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="TimeslotHeaderArea"/>
		/// </summary>
		public TimeslotHeaderArea()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_timeslotPanel != null)
			{
				_timeslotPanel.SetIsAttachedElement(false);
				_timeslotPanel.ClearValue(ScheduleControlBase.ControlProperty);
				_timeslotPanel.ClearValue(TimeslotPanel.TimeslotOrientationProperty);
				_timeslotPanel.Timeslots = null;
				_timeslotPanel.CurrentTimeIndicatorVisibility = Visibility.Collapsed;
			}

			_timeslotPanel = this.GetTemplateChild(PartTimeslotPanel) as TimeslotPanel;

			if (null != _timeslotPanel)
			{
				var ctrl = ScheduleControlBase.GetControl(this);
				_timeslotPanel.SetValue(ScheduleControlBase.ControlProperty, ctrl);
				_timeslotPanel.Timeslots = this.Timeslots;

				if (null != ctrl)
				{
					_timeslotPanel.TimeslotOrientation = ctrl.TimeslotAreaTimeslotOrientation;
					_timeslotPanel.TemplateItem = ctrl.CreateHeaderTemplateItem();
				}

				_timeslotPanel.SetIsAttachedElement(this.Timeslots != null);
			}

			this.VerifyCurrentTimeIndicatorVisibility();
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="TimeslotHeaderArea"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="TimeslotHeaderAreaAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new TimeslotHeaderAreaAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnMouseWheel
		/// <summary>
		/// Invoked when the mouse wheel has been scrolled
		/// </summary>
		/// <param name="e">Provides information about the event.</param>
		protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
		{
			if (!e.Handled && this.IsEnabled)
			{
				ScheduleControlBase ctrl = ScheduleControlBase.GetControl(this);

				if (null != ctrl)
					ctrl.ProcessMouseWheel(this, e);
			}

			base.OnMouseWheel(e);
		}
		#endregion // OnMouseWheel

		#endregion //Base class overrides

		#region Properties

		#region Timeslots

		private TimeslotCollection _timeslots;

		/// <summary>
		/// Returns or sets the collection of timeslots that represent the headers
		/// </summary>
		internal TimeslotCollection Timeslots
		{
			get
			{
				return _timeslots;
			}
			set
			{
				if (value != _timeslots)
				{
					TimeslotCollection old = _timeslots;
					_timeslots = value;

					this.OnTimeslotsChanged(old, value);
				}
			}
		}

		internal virtual void OnTimeslotsChanged(TimeslotCollection oldValue, TimeslotCollection newValue)
		{
			if ( _timeslotPanel != null )
			{
				_timeslotPanel.Timeslots = newValue;
				_timeslotPanel.SetIsAttachedElement(newValue != null);
			}
		}

		#endregion //Timeslots

		#region CurrentTimeIndicatorVisibility
		internal Visibility CurrentTimeIndicatorVisibility
		{
			get { return _currentTimeIndicatorVisibility; }
			set
			{
				_currentTimeIndicatorVisibility = value;
				this.VerifyCurrentTimeIndicatorVisibility();
			}
		} 
		#endregion // CurrentTimeIndicatorVisibility

		#endregion //Properties

		#region Methods

		#region OnCalendarHeaderAreaVisibilityChanged
		internal virtual void OnCalendarHeaderAreaVisibilityChanged()
		{
		}
		#endregion // OnCalendarHeaderAreaVisibilityChanged

		#region RefreshDisplay
		internal virtual void RefreshDisplay()
		{
		} 
		#endregion // RefreshDisplay

		#region VerifyCurrentTimeIndicatorVisibility
		private void VerifyCurrentTimeIndicatorVisibility()
		{
			if (_timeslotPanel != null)
				_timeslotPanel.CurrentTimeIndicatorVisibility = _currentTimeIndicatorVisibility;
		}
		#endregion // VerifyCurrentTimeIndicatorVisibility
		
		#endregion // Methods

		#region ITimeRangeArea Members

		TimeslotPanelBase ITimeRangeArea.TimeRangePanel
		{
			get { return this._timeslotPanel; }
		}

		TimeslotPanelBase ITimeRangeArea.ActivityPanel
		{
			get { return null; }
		}

		#endregion //ITimeRangeArea Members
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