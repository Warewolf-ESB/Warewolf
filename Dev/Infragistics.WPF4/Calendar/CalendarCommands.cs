using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace Infragistics.Controls.Editors.Primitives
{
	#region CalendarCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref="CalendarBase"/>.
	/// </summary>
	public class CalendarCommand : CommandBase
	{
		#region Private Members

		private CalendarCommandType _commandType;

		#endregion //Private Members	
    
		#region Constructor

		internal CalendarCommand(CalendarCommandType commandTyoe)
		{
			_commandType = commandTyoe;
		}

		#endregion //Constructor	
    
		#region Overrides

		#region Public

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			CalendarBase cal = GetCalendar(parameter);
			if (cal != null)
			{
				CommandSource source = this.CommandSource;

				return cal.CanExecuteCommand(_commandType, source.SourceElement);
			}

			return base.CanExecute(parameter);
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter">The <see cref="CalendarBase"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			CalendarBase cal = GetCalendar(parameter);
			if (cal != null)
			{
				CommandSource source = this.CommandSource;

				cal.ExecuteCommand(_commandType, source.ParameterResolved, source.SourceElement);
				this.CommandSource.Handled = true;

				return;
			}

			base.Execute(parameter);
		}

		private static CalendarBase GetCalendar(object parameter)
		{
			CalendarBase cal = parameter as CalendarBase;
			if (cal == null)
			{
				DependencyObject target = parameter as DependencyObject;

				if (target != null)
					cal = CalendarBase.GetCalendar(target); ;

			}
			return cal;
		}

		#endregion Execute

		#endregion // Public

		#endregion // Overrides
	}
	#endregion // CalendarCommandBase Class

	#region CalendarCommandSource Class
	/// <summary>
	/// The command source object for <see cref="CalendarCommand"/> object.
	/// </summary>
	public class CalendarCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the CalendarCommand which is to be executed by the command.
		/// </summary>
		public CalendarCommandType CommandType
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
			return new CalendarCommand(this.CommandType);
		}
	}

	#endregion //CalendarCommandSource Class


//        // ====================================================================================================================================


//        // ====================================================================================================================================
//        // ADD COMMANDWRAPPERS HERE FOR EACH COMMAND DEFINED ABOVE.
//        // ------------------------------------------------------------------------------------------------------------------------------------
//        //
//        /// <summary>
//        /// The list of CommandWrappers for each supported command.
//        /// </summary>
//        /// <seealso cref="Infragistics.Windows.Commands.CommandWrapper"/>

//        #region CommandWrapper Definitions

//        private static CommandWrapper[] GetCommandWrappers()
//        {
//            return new CommandWrapper[] {
//                //					RoutedCommand					StateDisallowed					StateRequired								InputGestures
//                //					=============					===============					=============								=============
//                // move the caret to right by one ( Right )
//                //new CommandWrapper(	
//                //    NextMonth,	// Action
//                //    (Int64)0,	// Disallowed state		
//                //    (Int64)CalendarStates.Character,	// Required state
//                //    new InputGesture[] { new KeyGesture(Key.Right, ModifierKeys.None), new KeyGesture(Key.Right, ModifierKeys.Shift) },
//                //    ModifierKeys.Control ),
//                new CommandWrapper(	
//                    ScrollNextGroup,	// Action
//                    (Int64)0,	// Disallowed state		
//                    (Int64)0,	// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ScrollPreviousGroup,	// Action
//                    (Int64)0,	// Disallowed state	
//                    (Int64)0,	// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ScrollNextGroups,	// Action
//                    (Int64)0,	// Disallowed state		
//                    (Int64)0,	// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ScrollPreviousGroups,	// Action
//                    (Int64)0,	// Disallowed state		
//                    (Int64)0,	// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ActivateDate,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ActivateSelectedDate,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    Today,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    PreviousItem,	// Action
//                    (Int64)CalendarStates.RightToLeft/* 0 AS 1/5/10 TFS23198 */,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.Left, AllCtrlShiftModifiers),
//                    ModifierKeys.Alt ),

//                new CommandWrapper(	
//                    NextItem,	// Action
//                    (Int64)CalendarStates.RightToLeft/* 0 AS 1/5/10 TFS23198 */,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.Right, AllCtrlShiftModifiers),
//                    ModifierKeys.Alt ),

//                // AS 1/5/10 TFS23198
//                new CommandWrapper(	
//                    PreviousItem,	// Action
//                    (Int64)0,		// Disallowed state
//                    (Int64)CalendarStates.RightToLeft,		// Required state		
//                    CreateGestureCombinations(Key.Right, AllCtrlShiftModifiers),
//                    ModifierKeys.Alt ),

//                // AS 1/5/10 TFS23198
//                new CommandWrapper(	
//                    NextItem,	// Action
//                    (Int64)0,		// Disallowed state
//                    (Int64)CalendarStates.RightToLeft,		// Required state		
//                    CreateGestureCombinations(Key.Left, AllCtrlShiftModifiers),
//                    ModifierKeys.Alt ),

//                new CommandWrapper(	
//                    PreviousItemRow,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.Up, AllCtrlShiftModifiers),
//                    ModifierKeys.Alt ),

//                new CommandWrapper(	
//                    NextItemRow,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.Down, AllCtrlShiftModifiers),
//                    ModifierKeys.Alt ),

//                new CommandWrapper(	
//                    FirstItemOfGroup,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.Home, ModifierKeys.None | ModifierKeys.Shift),
//                    ModifierKeys.Alt | ModifierKeys.Control ),

//                new CommandWrapper(	
//                    LastItemOfGroup,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.End, ModifierKeys.None | ModifierKeys.Shift),
//                    ModifierKeys.Alt | ModifierKeys.Control ),

//                new CommandWrapper(	
//                    PreviousGroup,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.PageUp, AllCtrlShiftModifiers),
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    NextGroup,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.PageDown, AllCtrlShiftModifiers),
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    FirstItemOfFirstGroup,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    new InputGesture[] { new KeyGesture(Key.Home, ModifierKeys.Control), new KeyGesture(Key.Home, ModifierKeys.Control | ModifierKeys.Shift) },
//                    ModifierKeys.Alt ),

//                new CommandWrapper(	
//                    LastItemOfLastGroup,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    new InputGesture[] { new KeyGesture(Key.End, ModifierKeys.Control), new KeyGesture(Key.Home, ModifierKeys.Control | ModifierKeys.Shift) },
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ZoomOutCalendarMode,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    new InputGesture[] { new KeyGesture(Key.Add, ModifierKeys.Control) },
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ZoomInCalendarMode,	// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)CalendarStates.ActiveDate,		// Required state
//                    new InputGesture[] { new KeyGesture(Key.Enter, ModifierKeys.None), new KeyGesture(Key.Space, ModifierKeys.None), new KeyGesture(Key.Subtract, ModifierKeys.Control) },
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ScrollToDate,	// Action
//                    (Int64)0,	// Disallowed state		
//                    (Int64)0,	// Required state
//                    (InputGesture)null,
//                    ModifierKeys.None ),

//                new CommandWrapper(	
//                    ToggleActiveDateSelection,// Action
//                    (Int64)0,		// Disallowed state		
//                    (Int64)0,		// Required state
//                    CreateGestureCombinations(Key.Space, ModifierKeys.None | ModifierKeys.Control),
//                    ModifierKeys.Alt | ModifierKeys.Shift ),
//                // ------------------------------------------------------------------------}

//            };
//        }
//        #endregion //CommandWrapper Definitions

//        // ====================================================================================================================================


//        static CalendarCommands( )
//        {
//            // Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
//            // by our CommandWrappers.
//            Commands<CalendarBase>.Initialize( CalendarCommands.GetCommandWrappers() );
//        }


//        /// <summary>
//        /// This method is provided as a convenience for initializing the statics in this class which kicks off
//        /// the process of setting up and registering the commands.
//        /// </summary>
//        public static void LoadCommands( )
//        {
//        }

//        private static CalendarCommands g_instance;
//        internal static CalendarCommands Instance
//        {
//            get
//            {
//                if ( g_instance == null )
//                    g_instance = new CalendarCommands( );

//                return g_instance;
//            }
//        }

//        #region CreateGestureCombinations
//#if DEBUG
//        /// <summary>
//        /// Creates all the combinations of gestures that include all the modifiers as well as None.
//        /// </summary>
//#endif
//        private static InputGestureCollection CreateGestureCombinations(Key key, ModifierKeys modifiers)
//        {
//            InputGestureCollection gestures = new InputGestureCollection();

//            AddGesture(gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Control);
//            AddGesture(gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt);
//            AddGesture(gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Alt);
//            AddGesture(gestures, key, modifiers, ModifierKeys.Alt | ModifierKeys.Control);
//            AddGesture(gestures, key, modifiers, ModifierKeys.Shift);
//            AddGesture(gestures, key, modifiers, ModifierKeys.Control);
//            AddGesture(gestures, key, modifiers, ModifierKeys.Alt);
//            AddGesture(gestures, key, modifiers, ModifierKeys.None);

//            return gestures;
//        }
//        #endregion //CreateGestureCombinations

//        #region AddGesture
//        private static void AddGesture(InputGestureCollection gestures, Key key, ModifierKeys gestureModifiers, ModifierKeys modifierToCheck)
//        {
//            if ((gestureModifiers & modifierToCheck) == modifierToCheck)
//                gestures.Add(new KeyGesture(key, modifierToCheck));
//        }
//        #endregion //AddGesture

//        // AS 10/15/09 TFS23867
//        #region GetKeyboardParameter

//        internal static readonly object NavigationKeyParameter = new object();

//        /// <summary>
//        /// Returns the parameter that should be used for a given command that is being processed by the <see cref="Commands&lt;T&gt;.ProcessKeyboardInput"/> method.
//        /// </summary>
//        /// <param name="commandHost">The ICommandHost instance for which the keyboard input is being processed.</param>
//        /// <param name="command">The command to be executed.</param>
//        /// <param name="keyArgs">The key event arguments for which the command is being invoked</param>
//        /// <returns>Returns the object to supply as the parameter for the command execution.</returns>
//        protected override object GetKeyboardParameter(ICommandHost commandHost, RoutedCommand command, KeyEventArgs keyArgs)
//        {
//            Key key = keyArgs.Key;

//            if (key == Key.System)
//                key = keyArgs.SystemKey;

//            switch (key)
//            {
//                case Key.Down:
//                case Key.Up:
//                case Key.Left:
//                case Key.Right:
//                    return NavigationKeyParameter;
//            }

//            return null;
//        }
//        #endregion //GetKeyboardParameter
//    }
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