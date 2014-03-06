using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Commands;
using System.Windows.Input;

namespace Infragistics.Windows.Controls
{
    /// <summary>
    /// Provides the list of RoutedCommands supported by the <see cref="TabItemEx"/>. 
    /// </summary>
    public class TabItemExCommands : Commands<TabItemEx>
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

        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
        /// <summary>
        /// Closes the tab item. This command will be disabled if the tab item does not allow being closed (i.e. it is not closable).
        /// </summary>
        public static readonly RoutedCommand Close = new IGRoutedCommand("Close",
                                                                                      typeof(TabItemExCommands),
                                                                                      (Int64)0,
                                                                                      (Int64)TabItemExStates.AllowsClosing);
        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
        /// <summary>
        /// Closes all closable tab items within the containing XamTabControl except the current tab item. If this command is executed against the XamTabControl itself (i.e. with the XamTabControl as the target of the command), all closable tabs except the selected tab will be closed. 
        /// </summary>
        public static readonly RoutedCommand CloseAllButThis = new IGRoutedCommand("CloseAllButThis",
                                                                                      typeof(TabItemExCommands),
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


				new CommandWrapper(	Close,							(Int64)0,
																									(Int64)(TabItemExStates.SelectedTab | TabItemExStates.AllowsClosing),
				                                                                                                                    new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.W, ModifierKeys.Control) }),
																																	ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Windows),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 


				new CommandWrapper(	CloseAllButThis ),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

			};
        }
        #endregion //CommandWrapper Definitions

        // ====================================================================================================================================


        static TabItemExCommands()
        {
            // Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
            // by our CommandWrappers.
            Commands<TabItemEx>.Initialize(TabItemExCommands.GetCommandWrappers());
        }


        /// <summary>
        /// This method is provided as a convenience for initializing the statics in this class which kicks off
        /// the process of setting up and registering the commands.
        /// </summary>
        public static void LoadCommands()
        {
        }

        private static TabItemExCommands g_instance;
        internal static TabItemExCommands Instance
        {
            get
            {
                if (g_instance == null)
                    g_instance = new TabItemExCommands();

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