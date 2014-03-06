using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Commands;
using System.Windows.Input;

namespace Infragistics.Windows.Editors
{
	/// <summary>
	/// Provides the list of RoutedCommands supported by the <see cref="XamMonthCalendar"/>. 
	/// </summary>
    /// <see cref="XamMonthCalendar"/>
    /// <see cref="XamMonthCalendar.ExecuteCommand(RoutedCommand)"/>
    /// <see cref="XamMonthCalendar.ExecutingCommand"/>
    /// <see cref="XamMonthCalendar.ExecutedCommand"/>
    public class MonthCalendarCommands : Commands<XamMonthCalendar>
	{
		private const ModifierKeys AllCtrlShiftModifiers = ModifierKeys.None | ModifierKeys.Shift | ModifierKeys.Control;

		// ====================================================================================================================================
		// ADD NEW COMMANDS HERE with the minimum required control state (also add a CommandWrapper for each command to the CommandWrappers array
		// below which will let you specify the triggering KeyGestures and required/disallowed states)
		//
		// Note that while individual commands in this static list are defined as type RoutedCommand or RoutedUICommand,
		// we actually create IGRoutedCommands or IGRoutedUICommands (both derived from RoutedCommand) so we can specify
		// and store the minimum control state needed to execute the command.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//

		#region Command Definitions

		#region Scroll

		/// <summary>
		/// Scroll forward by one group
		/// </summary>
		public static readonly RoutedCommand ScrollNextGroup = new IGRoutedCommand("ScrollNextGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)MonthCalendarStates.MaxDateInView,
																					  (Int64)0);
		/// <summary>
		/// Scroll backward by one group
		/// </summary>
		public static readonly RoutedCommand ScrollPreviousGroup = new IGRoutedCommand("ScrollPreviousGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)MonthCalendarStates.MinDateInView,
																					  (Int64)0);
		/// <summary>
		/// Scroll forward by the number of groups currently in view
		/// </summary>
		public static readonly RoutedCommand ScrollNextGroups = new IGRoutedCommand("ScrollNextGroups",
																					  typeof(MonthCalendarCommands),
																					  (Int64)MonthCalendarStates.MaxDateInView,
																					  (Int64)0);
		/// <summary>
		/// Scroll backward by the number of groups currently in view
		/// </summary>
		public static readonly RoutedCommand ScrollPreviousGroups = new IGRoutedCommand("ScrollPreviousGroups",
																					  typeof(MonthCalendarCommands),
																					  (Int64)MonthCalendarStates.MinDateInView,
																					  (Int64)0);
		
		/// <summary>
		/// Scrolls to the date specified in the command parameter. If this command is sent from within a <see cref="CalendarItemGroup"/>, the date will be scrolled into view in that group if possible - even if it is already in view within another <see cref="CalendarItemGroup"/>.
		/// </summary>
		public static readonly RoutedCommand ScrollToDate = new IGRoutedCommand("ScrollToDate",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)0);
		#endregion //Scroll

		/// <summary>
		/// Activate a particular date. The source must be a <see cref="CalendarItem"/> or within it - or the CommandParameter must be the date of the day to activate.
		/// </summary>
		public static readonly RoutedCommand ActivateDate = new IGRoutedCommand("ActivateDate",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)0);
		/// <summary>
		/// Ensures the <see cref="CalendarItem"/> that represents the <see cref="XamMonthCalendar.SelectedDate"/> is in view and has the input focus.
		/// </summary>
		public static readonly RoutedCommand ActivateSelectedDate = new IGRoutedCommand("ActivateSelectedDate",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
                                                                                      (Int64)0);

		/// <summary>
		/// Activating the <see cref="CalendarDay"/> that represents the current date.
		/// </summary>
		public static readonly RoutedCommand Today = new IGRoutedCommand("Today",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.TodayIsEnabled);
		#region Navigate

		/// <summary>
		/// Navigates to the <see cref="CalendarItem"/> previous to the <see cref="XamMonthCalendar.ActiveDate"/>.
		/// </summary>
		public static readonly RoutedCommand PreviousItem = new IGRoutedCommand("PreviousItem",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the <see cref="CalendarItem"/> after the <see cref="XamMonthCalendar.ActiveDate"/>.
		/// </summary>
		public static readonly RoutedCommand NextItem = new IGRoutedCommand("NextItem",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the <see cref="CalendarItem"/> in the previous row (e.g. the same day of the week as the <see cref="XamMonthCalendar.ActiveDate"/> in the previous week when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand PreviousItemRow = new IGRoutedCommand("PreviousItemRow",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the <see cref="CalendarItem"/> in the previous row (e.g. the same day of the week as the <see cref="XamMonthCalendar.ActiveDate"/> in the following week when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand NextItemRow = new IGRoutedCommand("NextItemRow",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the <see cref="CalendarItem"/> in the previous group (e.g. the same day of the month as the <see cref="XamMonthCalendar.ActiveDate"/> in the previous month when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand PreviousGroup = new IGRoutedCommand("PreviousGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the <see cref="CalendarItem"/> in the previous group (e.g. the same day of the month as the <see cref="XamMonthCalendar.ActiveDate"/> in the following month when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand NextGroup = new IGRoutedCommand("NextGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);

		/// <summary>
		/// Navigates to the first <see cref="CalendarItem"/> in the current group (e.g. the first day of the month containing the current <see cref="XamMonthCalendar.ActiveDate"/>  when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand FirstItemOfGroup = new IGRoutedCommand("FirstItemOfGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the last <see cref="CalendarItem"/> in the current group (e.g. the last day of the month containing the current <see cref="XamMonthCalendar.ActiveDate"/>  when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand LastItemOfGroup = new IGRoutedCommand("LastItemOfGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the first <see cref="CalendarItem"/> of the first <see cref="CalendarItemGroup"/> in a <see cref="XamMonthCalendar"/> (e.g. the first day of the first month currently in view when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand FirstItemOfFirstGroup = new IGRoutedCommand("FirstItemOfFirstGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);
		/// <summary>
		/// Navigates to the last <see cref="CalendarItem"/> of the last <see cref="CalendarItemGroup"/> in a <see cref="XamMonthCalendar"/> (e.g. the first day of the last month currently in view when <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>).
		/// </summary>
		public static readonly RoutedCommand LastItemOfLastGroup = new IGRoutedCommand("LastItemOfLastGroup",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.ActiveDate);

		#endregion //Navigate

		/// <summary>
		/// Increases the <see cref="XamMonthCalendar.CurrentCalendarMode"/> to a larger date range - e.g. from <b>Days</b> to <b>Months</b>.
		/// </summary>
        public static readonly RoutedCommand ZoomOutCalendarMode = new IGRoutedCommand("ZoomOutCalendarMode",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.CanZoomOutCalendarMode);

		/// <summary>
		/// Decreases the <see cref="XamMonthCalendar.CurrentCalendarMode"/> to a smaller date range - e.g. from <b>Months</b> to <b>Days</b>.
		/// </summary>
		public static readonly RoutedCommand ZoomInCalendarMode = new IGRoutedCommand("ZoomInCalendarMode",
																					  typeof(MonthCalendarCommands),
																					  (Int64)0,
																					  (Int64)MonthCalendarStates.CanZoomInCalendarMode);

        /// <summary>
        /// Toggles the selection of the item represented by the <see cref="XamMonthCalendar.ActiveDate"/>.
        /// </summary>
        public static readonly RoutedCommand ToggleActiveDateSelection = new IGRoutedCommand("ToggleSelection",
                                                                                      typeof(MonthCalendarCommands),
                                                                                      (Int64)0,
                                                                                      (Int64)(MonthCalendarStates.ActiveDate | MonthCalendarStates.MinCalendarMode));
        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

		#endregion //Command Definitions

		// ====================================================================================================================================


		// ====================================================================================================================================
		// ADD COMMANDWRAPPERS HERE FOR EACH COMMAND DEFINED ABOVE.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//
		/// <summary>
		/// The list of CommandWrappers for each supported command.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Commands.CommandWrapper"/>

		#region CommandWrapper Definitions

		private static CommandWrapper[] GetCommandWrappers()
		{
			return new CommandWrapper[] {
				//					RoutedCommand					StateDisallowed					StateRequired								InputGestures
				//					=============					===============					=============								=============
				// move the caret to right by one ( Right )
				//new CommandWrapper(	
				//    NextMonth,	// Action
				//    (Int64)0,	// Disallowed state		
				//    (Int64)MonthCalendarStates.Character,	// Required state
				//    new InputGesture[] { new KeyGesture(Key.Right, ModifierKeys.None), new KeyGesture(Key.Right, ModifierKeys.Shift) },
				//    ModifierKeys.Control ),
				new CommandWrapper(	
				    ScrollNextGroup,	// Action
				    (Int64)0,	// Disallowed state		
				    (Int64)0,	// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ScrollPreviousGroup,	// Action
				    (Int64)0,	// Disallowed state	
				    (Int64)0,	// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ScrollNextGroups,	// Action
				    (Int64)0,	// Disallowed state		
				    (Int64)0,	// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ScrollPreviousGroups,	// Action
				    (Int64)0,	// Disallowed state		
				    (Int64)0,	// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ActivateDate,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ActivateSelectedDate,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    Today,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    PreviousItem,	// Action
				    (Int64)MonthCalendarStates.RightToLeft,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.Left, AllCtrlShiftModifiers),
				    ModifierKeys.Alt ),

				new CommandWrapper(	
				    NextItem,	// Action
				    (Int64)MonthCalendarStates.RightToLeft,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.Right, AllCtrlShiftModifiers),
				    ModifierKeys.Alt ),

				// AS 1/5/10 TFS23198
				new CommandWrapper(	
				    PreviousItem,	// Action
				    (Int64)0,		// Disallowed state
				    (Int64)MonthCalendarStates.RightToLeft,		// Required state		
				    CreateGestureCombinations(Key.Right, AllCtrlShiftModifiers),
				    ModifierKeys.Alt ),

				// AS 1/5/10 TFS23198
				new CommandWrapper(	
				    NextItem,	// Action
				    (Int64)0,		// Disallowed state
				    (Int64)MonthCalendarStates.RightToLeft,		// Required state		
				    CreateGestureCombinations(Key.Left, AllCtrlShiftModifiers),
				    ModifierKeys.Alt ),

				new CommandWrapper(	
				    PreviousItemRow,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.Up, AllCtrlShiftModifiers),
				    ModifierKeys.Alt ),

				new CommandWrapper(	
				    NextItemRow,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.Down, AllCtrlShiftModifiers),
				    ModifierKeys.Alt ),

				new CommandWrapper(	
				    FirstItemOfGroup,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.Home, ModifierKeys.None | ModifierKeys.Shift),
				    ModifierKeys.Alt | ModifierKeys.Control ),

				new CommandWrapper(	
				    LastItemOfGroup,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.End, ModifierKeys.None | ModifierKeys.Shift),
				    ModifierKeys.Alt | ModifierKeys.Control ),

				new CommandWrapper(	
				    PreviousGroup,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.PageUp, AllCtrlShiftModifiers),
				    ModifierKeys.None ),

				new CommandWrapper(	
				    NextGroup,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.PageDown, AllCtrlShiftModifiers),
				    ModifierKeys.None ),

				new CommandWrapper(	
				    FirstItemOfFirstGroup,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    new InputGesture[] { new KeyGesture(Key.Home, ModifierKeys.Control), new KeyGesture(Key.Home, ModifierKeys.Control | ModifierKeys.Shift) },
				    ModifierKeys.Alt ),

				new CommandWrapper(	
				    LastItemOfLastGroup,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    new InputGesture[] { new KeyGesture(Key.End, ModifierKeys.Control), new KeyGesture(Key.Home, ModifierKeys.Control | ModifierKeys.Shift) },
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ZoomOutCalendarMode,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    new InputGesture[] { new KeyGesture(Key.Add, ModifierKeys.Control) },
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ZoomInCalendarMode,	// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)MonthCalendarStates.ActiveDate,		// Required state
				    new InputGesture[] { new KeyGesture(Key.Enter, ModifierKeys.None), new KeyGesture(Key.Space, ModifierKeys.None), new KeyGesture(Key.Subtract, ModifierKeys.Control) },
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ScrollToDate,	// Action
				    (Int64)0,	// Disallowed state		
				    (Int64)0,	// Required state
				    (InputGesture)null,
				    ModifierKeys.None ),

				new CommandWrapper(	
				    ToggleActiveDateSelection,// Action
				    (Int64)0,		// Disallowed state		
				    (Int64)0,		// Required state
				    CreateGestureCombinations(Key.Space, ModifierKeys.None | ModifierKeys.Control),
				    ModifierKeys.Alt | ModifierKeys.Shift ),
				// ------------------------------------------------------------------------}

			};
		}
		#endregion //CommandWrapper Definitions

		// ====================================================================================================================================


		static MonthCalendarCommands( )
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<XamMonthCalendar>.Initialize( MonthCalendarCommands.GetCommandWrappers() );
		}


		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands( )
		{
		}

		private static MonthCalendarCommands g_instance;
		internal static MonthCalendarCommands Instance
		{
			get
			{
				if ( g_instance == null )
					g_instance = new MonthCalendarCommands( );

				return g_instance;
			}
		}

		#region CreateGestureCombinations





		private static InputGestureCollection CreateGestureCombinations(Key key, ModifierKeys modifiers)
		{
			InputGestureCollection gestures = new InputGestureCollection();

			AddGesture(gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Control);
			AddGesture(gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt);
			AddGesture(gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Alt);
			AddGesture(gestures, key, modifiers, ModifierKeys.Alt | ModifierKeys.Control);
			AddGesture(gestures, key, modifiers, ModifierKeys.Shift);
			AddGesture(gestures, key, modifiers, ModifierKeys.Control);
			AddGesture(gestures, key, modifiers, ModifierKeys.Alt);
			AddGesture(gestures, key, modifiers, ModifierKeys.None);

			return gestures;
		}
		#endregion //CreateGestureCombinations

		#region AddGesture
		private static void AddGesture(InputGestureCollection gestures, Key key, ModifierKeys gestureModifiers, ModifierKeys modifierToCheck)
		{
			if ((gestureModifiers & modifierToCheck) == modifierToCheck)
				gestures.Add(new KeyGesture(key, modifierToCheck));
		}
		#endregion //AddGesture

		// AS 10/15/09 TFS23867
		#region GetKeyboardParameter

		internal static readonly object NavigationKeyParameter = new object();

		/// <summary>
		/// Returns the parameter that should be used for a given command that is being processed by the <see cref="Commands&lt;T&gt;.ProcessKeyboardInput"/> method.
		/// </summary>
		/// <param name="commandHost">The ICommandHost instance for which the keyboard input is being processed.</param>
		/// <param name="command">The command to be executed.</param>
		/// <param name="keyArgs">The key event arguments for which the command is being invoked</param>
		/// <returns>Returns the object to supply as the parameter for the command execution.</returns>
		protected override object GetKeyboardParameter(ICommandHost commandHost, RoutedCommand command, KeyEventArgs keyArgs)
		{
			Key key = keyArgs.Key;

			if (key == Key.System)
				key = keyArgs.SystemKey;

			switch (key)
			{
				case Key.Down:
				case Key.Up:
				case Key.Left:
				case Key.Right:
					return NavigationKeyParameter;
			}

			return null;
		}
		#endregion //GetKeyboardParameter
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