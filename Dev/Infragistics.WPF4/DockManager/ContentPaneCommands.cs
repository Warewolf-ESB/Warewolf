using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Commands;
using System.Windows.Input;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Provides the list of RoutedCommands supported by the <see cref="ContentPane"/>. 
	/// </summary>
	public class ContentPaneCommands : Commands<ContentPane>
	{
		// FUTURE consider adding a command to allow the escape key to put focus back into the active document or content of the xdm (if it has an activedocument or content) 

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

		/// <summary>
		/// Changes the <see cref="ContentPane.IsPinned"/> state of the pane from false to true or vice-versa.
		/// </summary>
		public static readonly RoutedCommand TogglePinnedState = CreateCommand("TogglePinnedState",
																					  (Int64)(ContentPaneStates.IsDocument | ContentPaneStates.IsFloating | ContentPaneStates.IsFloatingDockable),
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Changes the state of a Dockable ContentPane from docked to floating or vice-versa.
		/// </summary>
		public static readonly RoutedCommand ToggleDockedState = CreateCommand("ToggleDockedState",
																					  (Int64)(ContentPaneStates.IsDocument | ContentPaneStates.IsFloatingOnly | ContentPaneStates.IsUnpinned),
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Closes the pane.
		/// </summary>
		public static readonly RoutedCommand Close = CreateCommand("Close",
																					  (Int64)0,
																					  (Int64)ContentPaneStates.AllowClose);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Closes all the sibling panes.
		/// </summary>
		public static readonly RoutedCommand CloseAllButThis = CreateCommand("CloseAllButThis",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Moves the pane to the next document group
		/// </summary>
		public static readonly RoutedCommand MoveToNextGroup = CreateCommand("MoveToNextGroup",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Moves the pane to the previous document group
		/// </summary>
		public static readonly RoutedCommand MoveToPreviousGroup = CreateCommand("MoveToPreviousGroup",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Moves the pane to a new horizontal group
		/// </summary>
		public static readonly RoutedCommand MoveToNewHorizontalGroup = CreateCommand("MoveToNewHorizontalGroup",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Moves the pane to a new vertical group
		/// </summary>
		public static readonly RoutedCommand MoveToNewVerticalGroup = CreateCommand("MoveToNewVerticalGroup",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Reposition the pane such that it is displayed within a floating window that cannot be docked with other panes.
		/// </summary>
		public static readonly RoutedCommand ChangeToFloatingOnly = CreateCommand("ChangeToFloatingOnly",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Reposition the pane such that it is docked or floating and can be docked with other panes.
		/// </summary>
		public static readonly RoutedCommand ChangeToDockable = CreateCommand("ChangeToDockable",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Repositions the pane to be within the <see cref="DocumentContentHost"/> of the <see cref="XamDockManager"/>
		/// </summary>
		public static readonly RoutedCommand ChangeToDocument = CreateCommand("ChangeToDocument",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Activates the specified <see cref="ContentPane"/>
		/// </summary>
		public static readonly RoutedCommand ActivatePane = CreateCommand("ActivatePane",
																					  (Int64)0,
																					  (Int64)0);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Displays the <see cref="ContentPane"/> within the <see cref="UnpinnedTabFlyout"/> if the pane is unpinned.
		/// </summary>
		public static readonly RoutedCommand Flyout = CreateCommand("Flyout",
																					  (Int64)0,
																					  (Int64)ContentPaneStates.IsUnpinned);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Hides the <see cref="UnpinnedTabFlyout"/> if the pane is unpinned and currently displayed within the flyout.
		/// </summary>
		public static readonly RoutedCommand FlyIn = CreateCommand("FlyIn",
																					  (Int64)0,
																					  (Int64)ContentPaneStates.IsUnpinned);
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

				new CommandWrapper(	TogglePinnedState ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	ToggleDockedState ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	Close,							(Int64)ContentPaneStates.IsDocument,
																									(Int64)0,
				                                                                                                                    new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.Escape, ModifierKeys.Shift) }),
																																	ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	CloseAllButThis ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	MoveToNextGroup ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	MoveToPreviousGroup ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	MoveToNewHorizontalGroup ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	MoveToNewVerticalGroup ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	ChangeToFloatingOnly ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	ChangeToDockable ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	ChangeToDocument ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	ActivatePane ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	Flyout ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	FlyIn ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

			};
		}
		#endregion //CommandWrapper Definitions

		// ====================================================================================================================================


		static ContentPaneCommands()
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<ContentPane>.Initialize(ContentPaneCommands.GetCommandWrappers());
		}


		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands()
		{
		}

		private static ContentPaneCommands g_instance;
		internal static ContentPaneCommands Instance
		{
			get
			{
				if (g_instance == null)
					g_instance = new ContentPaneCommands();

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
            return new IGRoutedCommand(name, typeof(ContentPaneCommands), minimumStateDisallowed, minimumStateRequired, true);
        }
        #endregion //CreateCommand
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