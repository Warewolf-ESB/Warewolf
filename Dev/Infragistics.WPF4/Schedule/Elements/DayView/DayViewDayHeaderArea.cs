using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;
using System.Collections;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header area for a <see cref="XamDayView"/> for a specific <see cref="CalendarGroup"/>
	/// </summary>
	[TemplatePart(Name = PartTimeslotPanel, Type = typeof(TimeslotPanel))]
	[TemplatePart(Name = PartActivityPanel, Type = typeof(ScheduleActivityPanel))]
	public class DayViewDayHeaderArea : CalendarGroupItemsPresenterBase
		, ITimeRangeArea
	{
		#region Member Variables

		private const string PartTimeslotPanel = "TimeslotPanel";
		private const string PartActivityPanel = "ActivityPanel";

		private TimeslotPanel _timeslotPanel;
		private ScheduleActivityPanel _activityPanel;

		#endregion // Member Variables

		#region Constructor
		static DayViewDayHeaderArea()
		{

			DayViewDayHeaderArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(DayViewDayHeaderArea), new FrameworkPropertyMetadata(typeof(DayViewDayHeaderArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="DayViewDayHeaderArea"/>
		/// </summary>
		public DayViewDayHeaderArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region GetBorderThickness

		internal override Thickness GetBorderThickness(double borderSize)
		{
			return new Thickness(borderSize, borderSize, borderSize, 0);
		}

		#endregion //GetBorderThickness	

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			var ctrl = ScheduleControlBase.GetControl(this);
			Orientation orientation = (ctrl != null ? ctrl.TimeslotAreaTimeslotOrientation : Orientation.Vertical) == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;

			#region TimeslotPanel
			if (_timeslotPanel != null)
			{
				_timeslotPanel.SetIsAttachedElement(false);
				_timeslotPanel.ClearValue(ScheduleControlBase.ControlProperty);
				_timeslotPanel.ClearValue(TimeslotPanel.TimeslotOrientationProperty);
				_timeslotPanel.Timeslots = null;
			}

			_timeslotPanel = this.GetTemplateChild(PartTimeslotPanel) as TimeslotPanel;

			if (null != _timeslotPanel)
			{
				_timeslotPanel.SetValue(ScheduleControlBase.ControlProperty, ctrl);
				_timeslotPanel.TimeslotOrientation = orientation;
				_timeslotPanel.Timeslots = _timeslots;
				_timeslotPanel.SetIsAttachedElement(_timeslots != null);
			}
			#endregion // TimeslotPanel

			#region ActivityPanel
			if (_activityPanel != null)
			{
				_activityPanel.SetIsAttachedElement(false);
				_activityPanel.ClearValue(ScheduleControlBase.ControlProperty);
				_activityPanel.ClearValue(TimeslotPanel.TimeslotOrientationProperty);
				_activityPanel.Timeslots = null;
				_activityPanel.ActivityProvider = null;
			}

			_activityPanel = this.GetTemplateChild(PartActivityPanel) as ScheduleActivityPanel;

			if (null != _activityPanel)
			{
				// panels like the all day activity area and the monthview will always align
				// the starting and ending edges of the activity to the timeslots edges. also 
				// they both sort multi-timeslot activities before single timeslot activities
				_activityPanel.AlignToTimeslot = true;
				_activityPanel.SortByTimeslotCountFirst = true;

				_activityPanel.SetValue(ScheduleControlBase.ControlProperty, ctrl);
				_activityPanel.TimeslotOrientation = orientation;
				_activityPanel.Timeslots = _timeslots;
				_activityPanel.ActivityProvider = _activityProvider;
				_activityPanel.SetIsAttachedElement(_timeslots != null);

				if ( null != ctrl )
					ctrl.VerifyTimeslotPanelExtent(_activityPanel);
			}
			#endregion // ActivityPanel
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="DayViewDayHeaderArea"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="DayViewDayHeaderAreaAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new DayViewDayHeaderAreaAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnItemsPanelChanged
		internal override void OnItemsPanelChanged(ScheduleItemsPanel oldPanel, ScheduleItemsPanel newPanel)
		{
			base.OnItemsPanelChanged(oldPanel, newPanel);

			if (oldPanel != null)
			{
				oldPanel.ClearValue(ScheduleItemsPanel.OrientationProperty);
			}

			if (newPanel != null)
			{
				var ctrl = ScheduleControlBase.GetControl(this);
				newPanel.Orientation = ctrl != null ? ctrl.TimeslotGroupOrientation : Orientation.Horizontal;
			}
		}
		#endregion // OnItemsPanelChanged

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

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region MultiDayActivityAreaVisibility

		private static readonly DependencyPropertyKey MultiDayActivityAreaVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MultiDayActivityAreaVisibility",
			typeof(Visibility), typeof(DayViewDayHeaderArea), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="MultiDayActivityAreaVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MultiDayActivityAreaVisibilityProperty = MultiDayActivityAreaVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating whether the all day event area elements should be displayed.
		/// </summary>
		/// <seealso cref="MultiDayActivityAreaVisibilityProperty"/>
		public Visibility MultiDayActivityAreaVisibility
		{
			get
			{
				return (Visibility)this.GetValue(DayViewDayHeaderArea.MultiDayActivityAreaVisibilityProperty);
			}
			internal set
			{
				this.SetValue(DayViewDayHeaderArea.MultiDayActivityAreaVisibilityPropertyKey, value);
			}
		}

		#endregion //MultiDayActivityAreaVisibility

		#endregion // Public Properties

		#region Internal Properties

		#region ActivityProvider

		private AdapterActivitiesProvider _activityProvider;

		/// <summary>
		/// Returns or sets the object that provides the activities.
		/// </summary>
		internal AdapterActivitiesProvider ActivityProvider
		{
			get
			{
				return _activityProvider;
			}
			set
			{
				if (value != _activityProvider)
				{
					_activityProvider = value;

					if (_activityPanel != null)
						_activityPanel.ActivityProvider = value;
				}
			}
		}

		#endregion //ActivityProvider

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
					_timeslots = value;

					if ( _timeslotPanel != null )
					{
						_timeslotPanel.Timeslots = value;
						_timeslotPanel.SetIsAttachedElement(value != null);
					}

					if ( _activityPanel != null )
					{
						_activityPanel.Timeslots = value;
						_activityPanel.SetIsAttachedElement(value != null);
					}
				}
			}
		}

		#endregion //Timeslots

		#endregion // Internal Properties

		#endregion // Properties

		#region ITimeRangeArea Members

		TimeslotPanelBase ITimeRangeArea.TimeRangePanel
		{
			get { return this._timeslotPanel; }
		}

		TimeslotPanelBase ITimeRangeArea.ActivityPanel
		{
			get { return this._activityPanel; }
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