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
using Infragistics.AutomationPeers;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header area for a <see cref="CalendarGroup"/> in a <see cref="ScheduleControlBase"/> derived control
	/// </summary>
	public class CalendarHeaderArea : CalendarGroupItemsPresenterBase
	{
		#region Constructor
		static CalendarHeaderArea()
		{

			CalendarHeaderArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarHeaderArea), new FrameworkPropertyMetadata(typeof(CalendarHeaderArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="CalendarHeaderArea"/>
		/// </summary>
		public CalendarHeaderArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="CalendarHeaderArea"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="CalendarHeaderAreaAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new CalendarHeaderAreaAutomationPeer(this);
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
				ScheduleControlBase ctrl = ScheduleControlBase.GetControl(this);
				Debug.Assert(null != ctrl || System.ComponentModel.DesignerProperties.GetIsInDesignMode(this));
				
				if (null != ctrl)
				{
					Debug.Assert(this.Orientation == ctrl.CalendarGroupOrientation);
					newPanel.Orientation = ctrl.CalendarGroupOrientation;
					this.VerifyTabLayoutStyle(ctrl);
				}
			}

		}
		#endregion // OnItemsPanelChanged

		#endregion //Base class overrides

		#region Properties

		#region Orientation

		private static readonly DependencyPropertyKey OrientationPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Orientation",
			typeof(Orientation), typeof(CalendarHeaderArea),
			KnownBoxes.OrientationHorizontalBox, null
			);

		/// <summary>
		/// Identifies the read-only <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns or sets the orientation of the items within the CalendarHeaderArea
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(CalendarHeaderArea.OrientationProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeaderArea.OrientationPropertyKey, value);
			}
		}

		#endregion //Orientation

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region VerifyState
		internal void VerifyState()
		{
			if (null != this.ItemsPanel)
				this.VerifyTabLayoutStyle(ScheduleControlBase.GetControl(this));
		}
		#endregion // VerifyState

		#region SelectCalendar
		internal void SelectCalendar(CalendarHeader item)
		{
			var group = this.CalendarGroup;
			var calendar = item.Calendar;

			ScheduleControlBase control = ScheduleUtilities.GetControl(this);

			if (control != null)
				control.SelectCalendar(calendar, true, group);
		}
		#endregion // SelectCalendar

		#endregion // Internal Methods

		#region Private Methods

		#region VerifyTabLayoutStyle
		private void VerifyTabLayoutStyle(ScheduleControlBase ctrl)
		{
			ScheduleTabPanel tabPanel = this.ItemsPanel as ScheduleTabPanel;

			if (null != tabPanel && null != ctrl)
			{
				ScheduleTabLayoutStyle layoutStyle = ctrl.CalendarDisplayModeResolved == CalendarDisplayMode.Overlay
					? ScheduleTabLayoutStyle.SingleRowJustified
					: ScheduleTabLayoutStyle.SingleRowSizeToFit;

				tabPanel.TabLayoutStyle = layoutStyle;
			}
		}
		#endregion // VerifyTabLayoutStyle

		#endregion // Private Methods

		#endregion // Methods
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