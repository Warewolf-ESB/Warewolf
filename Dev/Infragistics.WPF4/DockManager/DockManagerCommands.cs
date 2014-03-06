using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Commands;
using System.Windows.Input;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Provides the list of RoutedCommands supported by the <see cref="XamDockManager"/>. 
	/// </summary>
	public class DockManagerCommands : Commands<XamDockManager>
	{
		// AS 3/15/07 BR21148
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

        // AS 2/12/09 TFS12819
        // Changed all IGRoutedCommand ctor calls to a helper method since we need to 
        // use a different ctor that allows us to mark a command as always handled.
        //

        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Activates the following document.
		/// </summary>
		public static readonly RoutedCommand ActivateNextDocument = CreateCommand("ActivateNextDocument",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Activates the preceeding document.
		/// </summary>
		public static readonly RoutedCommand ActivatePreviousDocument = CreateCommand("ActivatePreviousDocument",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Activates the next <see cref="ContentPane"/>
		/// </summary>
		public static readonly RoutedCommand ActivateNextPane = CreateCommand("ActivateNextPane",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Activates the previous <see cref="ContentPane"/>
		/// </summary>
		public static readonly RoutedCommand ActivatePreviousPane = CreateCommand("ActivatePreviousPane",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Closes the active document of the associated DocumentContentHost.
		/// </summary>
		public static readonly RoutedCommand CloseActiveDocument = CreateCommand("CloseActiveDocument",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Displays the <see cref="PaneNavigator"/>
		/// </summary>
		public static readonly RoutedCommand ShowPaneNavigator = CreateCommand("ShowPaneNavigator",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

		#endregion //Command Definitions

		// ====================================================================================================================================


		// ====================================================================================================================================
		// ADD COMMANDWRAPPERS HERE FOR EACH COMMAND DEFINED ABOVE.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//
		#region CommandWrapper Definitions

		/// <summary>
		/// The list of CommandWrappers for each supported command.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Commands.CommandWrapper"/>
		internal static CommandWrapper[] GetCommandWrappers()
		{
			return new CommandWrapper[] {
				//					RoutedCommand					StateDisallowed					StateRequired					InputGestures
				//					=============					===============					=============					=============
				new CommandWrapper(	ActivateNextDocument,			(Int64)0,						(Int64)0,
				                                                                                                                    new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F6, ModifierKeys.Control) }),
																																	ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Windows),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
				new CommandWrapper(	ActivatePreviousDocument,		(Int64)0,						(Int64)0,
				                                                                                                                    new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F6, ModifierKeys.Control | ModifierKeys.Shift) }),
																																	ModifierKeys.Alt | ModifierKeys.Windows),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	ActivateNextPane,				(Int64)0,						(Int64)0,
				                                                                                                                    new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F6, ModifierKeys.Alt) }),
																																	ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
				new CommandWrapper(	ActivatePreviousPane,			(Int64)0,						(Int64)0,
				                                                                                                                    new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F6, ModifierKeys.Alt | ModifierKeys.Shift) }),
																																	ModifierKeys.Control | ModifierKeys.Windows),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	CloseActiveDocument,			(Int64)0,						(Int64)DockManagerStates.ActiveDocument,
				                                                                                                                    new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F4, ModifierKeys.Control) }),
																																	ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Windows),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .


				new CommandWrapper(	ShowPaneNavigator ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

			};
		}
		#endregion //CommandWrapper Definitions

		// ====================================================================================================================================


		static DockManagerCommands()
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<XamDockManager>.Initialize(DockManagerCommands.GetCommandWrappers());
		}


		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands()
		{
		}

		private static DockManagerCommands g_instance;
		internal static DockManagerCommands Instance
		{
			get
			{
				if (g_instance == null)
					g_instance = new DockManagerCommands();

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

        // AS 2/12/09 TFS12819
        #region CreateCommand
        internal static IGRoutedCommand CreateCommand(string name, long minimumStateDisallowed, long minimumStateRequired)
        {
            return new IGRoutedCommand(name, typeof(DockManagerCommands), minimumStateDisallowed, minimumStateRequired, true);
        }
        #endregion //CreateCommand

		// AS 9/10/09 TFS19267
		#region GetKeyboardParameter
		/// <summary>
		/// Returns the parameter that should be used for a given command that is being processed by the <see cref="Commands&lt;T&gt;.ProcessKeyboardInput"/> method.
		/// </summary>
		/// <param name="commandHost">The ICommandHost instance for which the keyboard input is being processed.</param>
		/// <param name="command">The command to be executed.</param>
		/// <param name="keyArgs">The key event arguments for which the command is being invoked</param>
		/// <returns>Returns the object to supply as the parameter for the command execution.</returns>
		protected override object GetKeyboardParameter(ICommandHost commandHost, RoutedCommand command, KeyEventArgs keyArgs )
		{
			return commandHost;
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