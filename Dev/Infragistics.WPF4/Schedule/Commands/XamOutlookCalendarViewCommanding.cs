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
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	#region XamOutlookCalendarViewCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref="XamOutlookCalendarView"/>.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public abstract class XamOutlookCalendarViewCommandBase : CommandBase
	{
		#region Overrides

		#region Public

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute( object parameter )
		{
			return true;
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter">The <see cref="XamOutlookCalendarView"/> object that will be executed against.</param>
		public override void Execute( object parameter )
		{
			XamOutlookCalendarView ctrl = parameter as XamOutlookCalendarView;
			if ( ctrl != null )
			{
				this.ExecuteCommand(ctrl);

				if (this.CommandSource != null)
					this.CommandSource.Handled = true;
			}

			base.Execute(parameter);
		}
		#endregion // Execute

		#endregion // Public

		#region Protected

		#region ExecuteCommand
		/// <summary>
		/// Executes the specific command on the specified <see cref="XamOutlookCalendarView"/>
		/// </summary>
		/// <param name="control">The control for which the command will be executed.</param>
		protected abstract void ExecuteCommand( XamOutlookCalendarView control );
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // XamOutlookCalendarViewCommandBase Class

	#region XamOutlookCalendarViewCommandSource Class
	/// <summary>
	/// The command source object for <see cref="XamOutlookCalendarView"/> object.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public class XamOutlookCalendarViewCommandSource : CommandSource
	{
		#region Member Variables

		private static Dictionary<XamOutlookCalendarViewCommand, int> _dayCountTable; 

		#endregion // Member Variables

		#region Constructor
		static XamOutlookCalendarViewCommandSource()
		{
			_dayCountTable = new Dictionary<XamOutlookCalendarViewCommand, int>();
			_dayCountTable[XamOutlookCalendarViewCommand.Show1Day] = 1;
			_dayCountTable[XamOutlookCalendarViewCommand.Show2Days] = 2;
			_dayCountTable[XamOutlookCalendarViewCommand.Show3Days] = 3;
			_dayCountTable[XamOutlookCalendarViewCommand.Show4Days] = 4;
			_dayCountTable[XamOutlookCalendarViewCommand.Show5Days] = 5;
			_dayCountTable[XamOutlookCalendarViewCommand.Show6Days] = 6;
			_dayCountTable[XamOutlookCalendarViewCommand.Show7Days] = 7;
			_dayCountTable[XamOutlookCalendarViewCommand.Show8Days] = 8;
			_dayCountTable[XamOutlookCalendarViewCommand.Show9Days] = 9;
			_dayCountTable[XamOutlookCalendarViewCommand.Show10Days] = 10;
		}

		/// <summary>
		/// Initializes a new <see cref="XamOutlookCalendarViewCommandSource"/>
		/// </summary>
		public XamOutlookCalendarViewCommandSource()
		{
		}
		#endregion // Constructor

		/// <summary>
		/// Gets or sets the id of the command to be executed.
		/// </summary>
		public XamOutlookCalendarViewCommand CommandType
		{
			get;
			set;
		}

		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			switch ( this.CommandType )
			{
				case XamOutlookCalendarViewCommand.SwitchToDayView:
					return new XamOutlookCalendarViewChangeToDayViewCommand();
				case XamOutlookCalendarViewCommand.SwitchToScheduleView:
					return new XamOutlookCalendarViewChangeToScheduleViewCommand();
				case XamOutlookCalendarViewCommand.SwitchToFullWeekView:
					return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.DayViewWeek);
				case XamOutlookCalendarViewCommand.SwitchToMonthView:
					return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.MonthView);
				case XamOutlookCalendarViewCommand.SwitchToWorkWeekView:
					return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.DayViewWorkWeek);
				case XamOutlookCalendarViewCommand.NavigateBack:
					return new XamOutlookCalendarViewNavigateCommand(false);
				case XamOutlookCalendarViewCommand.NavigateForward:
					return new XamOutlookCalendarViewNavigateCommand(true);
				case XamOutlookCalendarViewCommand.Next7Days:
					return new XamOutlookCalendarViewDayCountCommand(7, DateTime.Today);
				default:
					{
						int dayCount;

						if ( _dayCountTable.TryGetValue(this.CommandType, out dayCount) )
							return new XamOutlookCalendarViewDayCountCommand(dayCount);

						Debug.Assert(false, "Unrecognized command type:" + this.CommandType.ToString());
						break;
					}
			}
			return null;
		}
	}

	#endregion //XamOutlookCalendarViewCommandSource Class

	#region XamOutlookCalendarView Commands

	#region XamOutlookCalendarViewDayCountCommand
	/// <summary>
	/// Switches to day or schedule view and shows the specified # of days.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public class XamOutlookCalendarViewDayCountCommand : XamOutlookCalendarViewCommandBase
	{
		private int _dayCount;
		private DateTime? _startingDate;

		/// <summary>
		/// Initializes a new <see cref="XamOutlookCalendarViewDayCountCommand"/>
		/// </summary>
		/// <param name="newDayCount">The number of days to display</param>
		/// <param name="startingDate">The date to use as the starting date for the new visible date range</param>
		/// <exception cref="ArgumentOutOfRangeException">The value must be 1 or greater</exception>
		public XamOutlookCalendarViewDayCountCommand( int newDayCount, DateTime? startingDate = null )
		{
			if ( newDayCount <= 0 )
				throw new ArgumentOutOfRangeException("");

			_dayCount = newDayCount;
			_startingDate = startingDate;
		}

		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute( object parameter )
		{
			if ( parameter is XamOutlookCalendarView )
				return true;

			return true;
		}

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="control">The <see cref="XamOutlookCalendarView"/> object that will execute the command.</param>
		protected override void ExecuteCommand( XamOutlookCalendarView control )
		{
			control.ChangeDayCount(_dayCount, _startingDate);
		}
	}
	#endregion // XamOutlookCalendarViewDayCountCommand

	#region XamOutlookCalendarViewChangeViewCommand
	/// <summary>
	/// Switches the <see cref="XamOutlookCalendarView.CurrentViewMode"/> to the specified view.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public class XamOutlookCalendarViewChangeViewCommand : XamOutlookCalendarViewCommandBase
	{
		private OutlookCalendarViewMode _view;

		/// <summary>
		/// Initializes a new <see cref="XamOutlookCalendarViewChangeViewCommand"/>
		/// </summary>
		/// <param name="view">The new view</param>
		public XamOutlookCalendarViewChangeViewCommand( OutlookCalendarViewMode view )
		{
			_view = view;
		}

		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute( object parameter )
		{
			if ( parameter is XamOutlookCalendarView )
				return true;

			return true;
		}

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="control">The <see cref="XamOutlookCalendarView"/> object that will execute the command.</param>
		protected override void ExecuteCommand( XamOutlookCalendarView control )
		{
			control.SetCurrentView(_view);
		}
	}
	#endregion // XamOutlookCalendarViewChangeViewCommand

	#region XamOutlookCalendarViewChangeToScheduleViewCommand
	/// <summary>
	/// Switches to day or schedule view and shows the specified # of days.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public class XamOutlookCalendarViewChangeToScheduleViewCommand : XamOutlookCalendarViewCommandBase
	{
		/// <summary>
		/// Initializes a new <see cref="XamOutlookCalendarViewChangeToScheduleViewCommand"/>
		/// </summary>
		public XamOutlookCalendarViewChangeToScheduleViewCommand()
		{
		}

		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute( object parameter )
		{
			if ( parameter is XamOutlookCalendarView )
				return true;

			return true;
		}

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="control">The <see cref="XamOutlookCalendarView"/> object that will execute the command.</param>
		protected override void ExecuteCommand( XamOutlookCalendarView control )
		{
			switch ( control.CurrentViewMode )
			{
				case OutlookCalendarViewMode.ScheduleViewDay:
				case OutlookCalendarViewMode.ScheduleViewWeek:
				case OutlookCalendarViewMode.ScheduleViewWorkWeek:
					// nothing to do
					break;
				case OutlookCalendarViewMode.MonthView:
					{
						control.ChangeDayCount(1, OutlookCalendarViewMode.ScheduleViewDay);
						break;
					}
				case OutlookCalendarViewMode.DayViewDay:
					control.SetCurrentView(OutlookCalendarViewMode.ScheduleViewDay);
					break;
				case OutlookCalendarViewMode.DayViewWeek:
					control.SetCurrentView(OutlookCalendarViewMode.ScheduleViewWeek);
					break;
				case OutlookCalendarViewMode.DayViewWorkWeek:
					control.SetCurrentView(OutlookCalendarViewMode.ScheduleViewWorkWeek);
					break;
			}
		}
	}
	#endregion // XamOutlookCalendarViewChangeToScheduleViewCommand

	#region XamOutlookCalendarViewChangeToDayViewCommand
	/// <summary>
	/// Switches to day or schedule view and shows the specified # of days.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public class XamOutlookCalendarViewChangeToDayViewCommand : XamOutlookCalendarViewCommandBase
	{
		/// <summary>
		/// Initializes a new <see cref="XamOutlookCalendarViewChangeToScheduleViewCommand"/>
		/// </summary>
		public XamOutlookCalendarViewChangeToDayViewCommand()
		{
		}

		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute( object parameter )
		{
			if ( parameter is XamOutlookCalendarView )
				return true;

			return true;
		}

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="control">The <see cref="XamOutlookCalendarView"/> object that will execute the command.</param>
		protected override void ExecuteCommand( XamOutlookCalendarView control )
		{
			switch ( control.CurrentViewMode )
			{
				case OutlookCalendarViewMode.DayViewDay:
					// nothing to do
					break;

				// in outlook clicking the day button whenever the view is month or one of the week modes
				// switches to a single day selection
				case OutlookCalendarViewMode.DayViewWeek:
				case OutlookCalendarViewMode.DayViewWorkWeek:
				case OutlookCalendarViewMode.ScheduleViewWeek:
				case OutlookCalendarViewMode.ScheduleViewWorkWeek:
				case OutlookCalendarViewMode.MonthView:
					{
						control.ChangeDayCount(1, OutlookCalendarViewMode.DayViewDay);
						break;
					}
				case OutlookCalendarViewMode.ScheduleViewDay:
					control.SetCurrentView(OutlookCalendarViewMode.DayViewDay);
					break;
			}
		}
	}
	#endregion // XamOutlookCalendarViewChangeToDayViewCommand

	#region XamOutlookCalendarViewNavigateCommand
	/// <summary>
	/// Shifts the visible dates currently in view.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public class XamOutlookCalendarViewNavigateCommand : XamOutlookCalendarViewCommandBase
	{
		private bool _forward;

		/// <summary>
		/// Initializes a new <see cref="XamOutlookCalendarViewNavigateCommand"/>
		/// </summary>
		/// <param name="forward">True to navigate forward and false to navigate backwards</param>
		public XamOutlookCalendarViewNavigateCommand( bool forward )
		{
			_forward = forward;
		}

		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute( object parameter )
		{
			if ( parameter is XamOutlookCalendarView )
				return true;

			return true;
		}

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="control">The <see cref="XamOutlookCalendarView"/> object that will execute the command.</param>
		protected override void ExecuteCommand( XamOutlookCalendarView control )
		{
			control.Navigate(_forward);
		}
	}
	#endregion // XamOutlookCalendarViewNavigateCommand

	#endregion // XamOutlookCalendarView Commands
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