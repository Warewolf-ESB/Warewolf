using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Schedules;
using System.Diagnostics;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents a group of timeslots
	/// </summary>
	[TemplatePart(Name = PartTimeslotPanel, Type = typeof(TimeslotPanel))]
	[TemplatePart(Name = PartActivityPanel, Type = typeof(ScheduleActivityPanel))]
	public class TimeslotArea : ResourceCalendarElementBase
		, ITimeRange
		, ITimeRangeArea
	{
		#region Member Variables

		private const string PartTimeslotPanel = "TimeslotPanel";
		private const string PartActivityPanel = "ActivityPanel";

		private TimeslotPanel _timeslotPanel;
		private ScheduleActivityPanel _activityPanel;

		#endregion //Member Variables

		#region Constructor
		static TimeslotArea()
		{

			TimeslotArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeslotArea), new FrameworkPropertyMetadata(typeof(TimeslotArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="TimeslotArea"/>
		/// </summary>
		public TimeslotArea()
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

			ScheduleControlBase ctrl = ScheduleControlBase.GetControl(this);
			Debug.Assert(null != ctrl || System.ComponentModel.DesignerProperties.GetIsInDesignMode(this));
			TimeslotAreaAdapter groupAdapter = this.AreaAdapter;
			TimeslotCollection timeslots = groupAdapter == null ? null : groupAdapter.Timeslots;

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

				if (null != ctrl)
					_timeslotPanel.TimeslotOrientation = ctrl.TimeslotAreaTimeslotOrientation;

				_timeslotPanel.Timeslots = timeslots;
				_timeslotPanel.SetIsAttachedElement(groupAdapter != null);
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
				_activityPanel.SetValue(ScheduleControlBase.ControlProperty, ctrl);

				if (null != ctrl)
					_activityPanel.TimeslotOrientation = ctrl.TimeslotAreaTimeslotOrientation;

				_activityPanel.Timeslots = timeslots;
				_activityPanel.ActivityProvider = groupAdapter != null ? groupAdapter.ActivityProvider : null;
				_activityPanel.SetIsAttachedElement(groupAdapter != null);

				if ( null != ctrl )
					ctrl.VerifyTimeslotPanelExtent(_activityPanel);
			} 
			#endregion // ActivityPanel
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="TimeslotArea"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="TimeslotAreaAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new TimeslotAreaAutomationPeer(this);
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

		#region ToString
		/// <summary>
		/// Returns the string representation of the element.
		/// </summary>
		/// <returns>The ToString of the associated <see cref="AreaAdapter"/></returns>
		public override string ToString()
		{
			if (null != this.AreaAdapter)
				return this.AreaAdapter.ToString();

			return base.ToString();
		}
		#endregion //ToString

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region ComputedBorderThickness

		private static readonly DependencyPropertyKey ComputedBorderThicknessPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderThickness",
			typeof(Thickness), typeof(TimeslotArea), new Thickness(), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderThickness"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderThicknessProperty = ComputedBorderThicknessPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the border thickness to use for the BorderBrush based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBorderThicknessProperty"/>
		public Thickness ComputedBorderThickness
		{
			get
			{
				return (Thickness)this.GetValue(TimeslotArea.ComputedBorderThicknessProperty);
			}
			internal set
			{
				this.SetValue(TimeslotArea.ComputedBorderThicknessPropertyKey, value);
			}
		}

		#endregion //ComputedBorderThickness

		#endregion // Public Properties

		#region Internal Properties

		#region AreaAdapter

		private TimeslotAreaAdapter _areaAdapter;

		internal TimeslotAreaAdapter AreaAdapter
		{
			get
			{
				return _areaAdapter;
			}
			set
			{
				_areaAdapter = value;

				TimeslotCollection timeslots = value != null ? value.Timeslots : null;

				if ( _timeslotPanel != null )
				{
					_timeslotPanel.Timeslots = timeslots;
					_timeslotPanel.SetIsAttachedElement(value != null);
				}

				if (_activityPanel != null)
				{
					_activityPanel.Timeslots = timeslots;
					_activityPanel.ActivityProvider = value != null ? value.ActivityProvider : null;
					_activityPanel.SetIsAttachedElement(value != null);
				}

				this.OnAreaAdapterChanged();
			}
		}
		#endregion //AreaAdapter

		#region BrushIds

		internal virtual CalendarBrushId BorderBrushId { get { return CalendarBrushId.DayBorder; } }

		#endregion //BrushIds

		#region IsTodayInternal
		internal virtual bool IsTodayInternal
		{
			get { return false; }
			set { }
		} 
		#endregion // IsTodayInternal

		#endregion // Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region OnAreaAdapterChanged
		internal virtual void OnAreaAdapterChanged()
		{
		}
		#endregion // OnAreaAdapterChanged

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			if (!this.IsBrushVersionBindingInitialized)
				return;

			ScheduleControlBase ctrl = ScheduleUtilities.GetControl(this);
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, ctrl);

			#region Set border brush

			Brush br = null;

			if (brushProvider != null)
			{
				CalendarBrushId brushId = this.BorderBrushId;
				br = brushProvider.GetBrush(brushId);
			}

			if (br != null)
			{
				this.ComputedBorderBrush = br;

				Thickness borderThickness;
				if (ctrl == null || ctrl.CalendarGroupOrientation == Orientation.Vertical)
					borderThickness = new Thickness(0, 1, 0, 1);
				else
					borderThickness = new Thickness(1, 0, 1, 1);

				this.ComputedBorderThickness = borderThickness;
			}

			#endregion //Set border brush
		}

		#endregion //SetProviderBrushes

		#endregion //Internal Methods	
	
		#endregion //Methods	
	
		#region ITimeRange members
		DateTime ITimeRange.Start
		{
			get
			{
				TimeslotAreaAdapter ts = this.AreaAdapter;

				if (ts != null)
					return ts.FirstSlotStart;

				return DateTime.MinValue;
			}
		}

		DateTime ITimeRange.End
		{
			get
			{
				TimeslotAreaAdapter ts = this.AreaAdapter;

				if (ts != null)
					return ts.LastSlotEnd;

				return DateTime.MaxValue;
			}
		}
		#endregion //ITimeRange members

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