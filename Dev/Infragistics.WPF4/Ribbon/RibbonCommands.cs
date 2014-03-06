using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Windows.Commands;

namespace Infragistics.Windows.Ribbon
{
	#region RibbonStates

	/// <summary>
	/// Represents the different states of the <see cref="XamRibbon"/>. Used to evaluate whether a specific command can be executed.
	/// </summary>
	[Flags]
	public enum RibbonStates : long
	{
		/// <summary>
		/// An <see cref="XamRibbon.ActiveItem"/> exists.
		/// </summary>
		ActiveItem					= 0x00000001,

		/// <summary>
		/// An <see cref="XamRibbon.ActiveItem"/> exists and it (or a logically related instance) is on the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		ActiveItemOnQat				= 0x00000002,

		/// <summary>
		/// An <see cref="XamRibbon.ActiveItem"/> exists and it is directly on the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		ActiveItemDirectlyOnQat		= 0x00000004,

		/// <summary>
		/// The <see cref="XamRibbon.AllowMinimize"/> is true and the minimized state of the Ribbon may be toggled via the UI.
		/// </summary>
		RibbonCanMinimize			= 0x00000008,
	};

	#endregion // RibbonStates

	#region RibbonCommands Class

	/// <summary>
	/// Provides the list of RoutedCommands supported by the <see cref="XamRibbon"/>. 
	/// </summary>
	public class RibbonCommands : Commands<XamRibbon>
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

		/// <summary>
		/// Null command.
		/// </summary>
		public static readonly RoutedCommand NotACommand = ApplicationCommands.NotACommand;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

		/// <summary>
		/// Command for toggling the <see cref="XamRibbon.IsMinimized"/> state of the <see cref="XamRibbon"/>.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.AllowMinimize"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.IsMinimized"/>
		public static readonly RoutedCommand ToggleRibbonMinimizedState = new IGRoutedCommand("ToggleRibbonMinimizedState",
																				  typeof(RibbonCommands),
																				  (Int64)0,
																				  (Int64)RibbonStates.RibbonCanMinimize);
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

		/// <summary>
		/// Command for toggling the location of the <see cref="QuickAccessToolbar"/> with respect to the <see cref="XamRibbon"/>.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.QuickAccessToolbarLocation"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.QuickAccessToolbar"/>
		public static readonly RoutedCommand ToggleQatLocation = new IGRoutedCommand("ToggleQatLocation",
																						 typeof(RibbonCommands),
																						 (Int64)0,
																						 (Int64)0);
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
				// Toggle the ribbon's minimized state
				new CommandWrapper(	
					ToggleRibbonMinimizedState,						// Action
					(Int64)0,										// Disallowed state		
					(Int64)RibbonStates.RibbonCanMinimize,			// Required state
					new InputGesture[] { new KeyGesture(Key.M, ModifierKeys.Control | ModifierKeys.Shift) },
					ModifierKeys.None ),

				// Toggle the Qat's location with respect to the ribbon
				new CommandWrapper(	
					ToggleQatLocation,								// Action
					(Int64)0,										// Disallowed state		
					(Int64)0,										// Required state
					new InputGesture[] { new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift) },
					ModifierKeys.None ),
			};
		}
		#endregion //CommandWrapper Definitions

		// ====================================================================================================================================


		static RibbonCommands( )
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<XamRibbon>.Initialize( RibbonCommands.GetCommandWrappers() );
		}


		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands( )
		{
		}

		private static RibbonCommands g_instance;
		internal static RibbonCommands Instance
		{
			get
			{
				if ( g_instance == null )
					g_instance = new RibbonCommands( );

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
	}

	#endregion // RibbonCommands Class

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