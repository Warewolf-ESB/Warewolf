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

namespace Infragistics.Windows.Editors
{
	#region MaskedEditorStates

	/// <summary>
	/// Represents the different states of the masked editor control.  Used to evaluate whether a specific command can be executed.
	/// </summary>
	[Flags]
	public enum MaskedEditorStates : long
	{
		/// <summary>
		/// caret is positioned right before a display char
		/// </summary>
		Character					= 0x00000001,

		/// <summary>
		/// the caret is positioned right before the first display char
		/// </summary>
		FirstCharacter				= 0x00000002,

		/// <summary>
		/// the caret is positioned right before the last display char 
		/// </summary>
		LastCharacter				= 0x00000004,

		/// <summary>
		/// the caret is in the first section
		/// </summary>
		FirstSection				= 0x00000008,

		/// <summary>
		/// the caret is in the last section
		/// </summary>
		LastSection					= 0x00000010,

		/// <summary>
		/// the caret is positioned right before the first character in a section
		/// </summary>
		FirstCharacterInSection		= 0x00000020,

		/// <summary>
		/// the caret is positioned right before the last character in a section
		/// </summary>
		LastCharacterInSection		= 0x00000040,

		/// <summary>
		/// the caret is positioned after the last display character		
		/// </summary>
		AfterLastCharacter			= 0x00000100,

		/// <summary>
		/// some text is selected
		/// </summary>
		Selected					= 0x00000200,

		/// <summary>
		/// Mask has not been initialized, and thus no sections or
		/// display chars collection exists
		/// </summary>
		Uninitialized				= 0x00000400,

		/// <summary>
		/// the caret is in the first edit section
		/// </summary>
		FirstEditSection			= 0x00000800,

		/// <summary>
		/// the caret is in the last edit section
		/// </summary>
		LastEditSection				= 0x00001000,
	
		/// <summary>
		/// State where the editor is not in edit mode.
		/// </summary>
		NotInEditMode				= 0x00002000, 

		/// <summary>
		/// State where the editor permits tabbing by sections based on the TabNavigation proeprty.
		/// </summary>
		TabBySections				= 0x00004000,

		// When the '.' numpad key is pressed, it's supposed to go to the fraction part
		// regardless of whether the decimal separator is comma or dot. This is because
		// the key code is always Decimal for numpad '.' key.
		// 
		/// <summary>
		/// Next edit section is a fraction part.
		/// </summary>
		NextSectionFraction			= 0x00008000,

		/// <summary>
		/// Current section can be decremented.
		/// </summary>
		CanSpinDown					= 0x00010000,

		/// <summary>
		/// Current section can be incremented.
		/// </summary>
		CanSpinUp					= 0x00020000,

		/// <summary>
		/// Can perform Undo action.
		/// </summary>
		CanUndo						= 0x00040000,

		/// <summary>
		/// Can perform Redo action.
		/// </summary>
		CanRedo						= 0x00080000,

        // AS 9/5/08 NA 2008 Vol 2
		/// <summary>
		/// There is an associated dropdown
		/// </summary>
		HasDropDown					= 0x00100000,		

		/// <summary>
		/// The associated dropdown is open
		/// </summary>
		IsDropDownOpen				= 0x00200000,

		/// <summary>
		/// The associated dropdown is open
		/// </summary>
		All							= 0xffffffff,
    };

	#endregion // MaskedEditorStates

	#region MaskedEditorCommands Class

	/// <summary>
	/// Provides the list of RoutedCommands supported by the XamMaskedEditor. 
	/// </summary>
	public class MaskedEditorCommands : Commands<XamMaskedEditor>
	{
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
		/// Represents a command which is always ignored.
		/// </summary>
		public static readonly RoutedCommand NotACommand = ApplicationCommands.NotACommand;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

		/// <summary>
		/// Command for moving the caret position to the next character.
		/// </summary>
		public static readonly RoutedCommand NextCharacter = new IGRoutedCommand( "NextCharacter",
																					  typeof(MaskedEditorCommands),
																					  (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)MaskedEditorStates.Character );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for moving the caret position to the next section.
		/// </summary>
		public static readonly RoutedCommand NextSection = new IGRoutedCommand( "NextSection",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)(MaskedEditorStates.LastSection | MaskedEditorStates.IsDropDownOpen),
																					  (Int64)MaskedEditorStates.Character );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for moving the caret position after the last character.
		/// </summary>
		public static readonly RoutedCommand AfterLastCharacter = new IGRoutedCommand( "AfterLastCharacter",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for setting the pivot at where the caret is currently.
		/// </summary>
		public static readonly RoutedCommand SetPivot = new IGRoutedCommand( "SetPivot",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for moving the caret position to the previous character.
		/// </summary>
		public static readonly RoutedCommand PreviousCharacter = new IGRoutedCommand( "PreviousCharacter",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)(MaskedEditorStates.FirstCharacter | MaskedEditorStates.IsDropDownOpen),
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for moving the caret position to the previous section.
		/// </summary>
		public static readonly RoutedCommand PreviousSection = new IGRoutedCommand( "PreviousSection",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)(MaskedEditorStates.FirstSection | MaskedEditorStates.IsDropDownOpen),
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for moving the caret position to the first character.
		/// </summary>
		public static readonly RoutedCommand FirstCharacter = new IGRoutedCommand( "FirstCharacter",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)(MaskedEditorStates.FirstCharacter | MaskedEditorStates.IsDropDownOpen),
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for selecting all the characters of the current section.
		/// </summary>
		public static readonly RoutedCommand SelectSection = new IGRoutedCommand( "SelectSection",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for selecting all the characters.
		/// </summary>
		public static readonly RoutedCommand SelectAll = ApplicationCommands.SelectAll;
		
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		//public static readonly RoutedCommand Delete = new IGRoutedCommand( "Delete",
		//                                                                              typeof( object ),
		//                                                                              (Int64)0,
		//                                                                              (Int64)0 );
		/// <summary>
		/// Command for deleting selected text. If nothing is selected then the character at 
		/// the current caret position will be deleted.
		/// </summary>
		public static readonly RoutedCommand Delete = ApplicationCommands.Delete;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for performing a 'Backspace' key operation. This command deletes the selected text. If nothing 
		/// is selected then the character before the current caret position will be deleted.
		/// </summary>
		public static readonly RoutedCommand Backspace = new IGRoutedCommand( "Backspace",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		//public static readonly RoutedCommand Copy = new IGRoutedCommand( "Copy",
		//                                                                              typeof( object ),
		//                                                                              (Int64)0,
		//                                                                              (Int64)MaskedEditorStates.Selected );
		/// <summary>
		/// Command for copying the selected text.
		/// </summary>
		public static readonly RoutedCommand Copy = ApplicationCommands.Copy;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		//public static readonly RoutedCommand Cut = new IGRoutedCommand( "Cut",
		//                                                                              typeof( object ),
		//                                                                              (Int64)0,
		//                                                                              (Int64)MaskedEditorStates.Selected );
		/// <summary>
		/// Command for cutting the selected text.
		/// </summary>
		public static readonly RoutedCommand Cut = ApplicationCommands.Cut;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		//public static readonly RoutedCommand Paste = new IGRoutedCommand( "Paste",
		//                                                                              typeof( object ),
		//                                                                              (Int64)0,
		//                                                                              (Int64)0 );
		/// <summary>
		/// Command for pasting clipboard contents into the editor.
		/// </summary>
		public static readonly RoutedCommand Paste = ApplicationCommands.Paste;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for undoing last change to the value.
		/// </summary>
		public static readonly RoutedCommand Undo = ApplicationCommands.Undo;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for undoing last change to the value.
		/// </summary>
		public static readonly RoutedCommand Redo = ApplicationCommands.Redo;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for toggling insert mode.
		/// </summary>
		public static readonly RoutedCommand ToggleInsertionMode = new IGRoutedCommand( "ToggleInsertionMode",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for spinning up the value of the section.
		/// </summary>
		public static readonly RoutedCommand SpinUp = new IGRoutedCommand( "SpinUp",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)MaskedEditorStates.CanSpinUp );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Command for spinning down the value of the section.
		/// </summary>
		public static readonly RoutedCommand SpinDown = new IGRoutedCommand( "SpinDown",
																					  typeof(MaskedEditorCommands),
                                                                                      (Int64)MaskedEditorStates.IsDropDownOpen,
																					  (Int64)MaskedEditorStates.CanSpinDown );

		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
        // AS 9/5/08 NA 2008 Vol 2
        /// <summary>
        /// Command for toggling the drop down state of the <see cref="XamDateTimeEditor"/>. If the editor is not in edit mode,
        /// this command will put the editor in edit mode.
        /// </summary>
        public static readonly RoutedCommand ToggleDropDown = new IGRoutedCommand("ToggleDropDown",
                                                                                      typeof(MaskedEditorCommands),
                                                                                      (Int64)0,
                                                                                      (Int64)MaskedEditorStates.HasDropDown);
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
				new CommandWrapper(	
					NextCharacter,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedEditorStates.Character,	// Required state
					new InputGesture[] { new KeyGesture(Key.Right, ModifierKeys.None), new KeyGesture(Key.Right, ModifierKeys.Shift) },
					ModifierKeys.Control ),

				// move to next section	( Ctl + Right )
				new CommandWrapper(	
					NextSection,	// Action
					(Int64)MaskedEditorStates.LastSection,	// Disallowed state		
					(Int64)MaskedEditorStates.Character,	// Required state
					new KeyGesture(Key.Right, ModifierKeys.Control),
					ModifierKeys.None ),

				// move to after last character when in last section ( Ctl + Right )
				new CommandWrapper(	
					AfterLastCharacter,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedEditorStates.LastSection,	// Required state
					new KeyGesture(Key.Right, ModifierKeys.Control),
					ModifierKeys.None ),

				// move the pivot to the new position (Right)
				new CommandWrapper(	
					SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Right, ModifierKeys.None),
					ModifierKeys.Shift),

				// move the caret to left by one ( Left )
				new CommandWrapper(	
					PreviousCharacter,	// Action
					(Int64)MaskedEditorStates.FirstCharacter,	// Disallowed state		
					(Int64)0,	// Required state
					new InputGesture[] { new KeyGesture(Key.Left, ModifierKeys.None), new KeyGesture(Key.Left, ModifierKeys.Shift) },
					ModifierKeys.Control),

				// move to prev section	( Ctl + Left )
				new CommandWrapper(	
					PreviousSection,	// Action
					(Int64)MaskedEditorStates.FirstSection,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Left, ModifierKeys.Control),
					ModifierKeys.None),

				// move to next section	( Ctl + Left )
				new CommandWrapper(	
					FirstCharacter,	// Action
					(Int64)MaskedEditorStates.FirstCharacter,	// Disallowed state		
					(Int64)MaskedEditorStates.FirstSection,	// Required state
					new KeyGesture(Key.Left, ModifierKeys.Control),
					ModifierKeys.None),

				// move the pivot to the new position
				new CommandWrapper(	
					SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Left, ModifierKeys.None),
					ModifierKeys.Shift),

				// move the caret to beggining of the text  ( Home )
				new CommandWrapper(	
					FirstCharacter,	// Action
					(Int64)MaskedEditorStates.FirstCharacter,	// Disallowed state		
					(Int64)0,	// Required state
					new InputGesture[] { new KeyGesture(Key.Home, ModifierKeys.None),new KeyGesture(Key.Home, ModifierKeys.Shift) },
					ModifierKeys.None),

				// move the pivot to beggining of the text  ( Home )
				new CommandWrapper(	
					SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Home, ModifierKeys.None),
					ModifierKeys.Shift),

				// move the caret to the end of the text   ( End )
				new CommandWrapper(	
					AfterLastCharacter,	// Action
					(Int64)MaskedEditorStates.AfterLastCharacter,	// Disallowed state		
					(Int64)0,	// Required state
					new InputGesture[] { new KeyGesture(Key.End, ModifierKeys.None), new KeyGesture(Key.End, ModifierKeys.Shift)  },
					ModifierKeys.None),

				// move the pivot to the end of the text ( End )
				new CommandWrapper(	
					SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.End, ModifierKeys.None),
					ModifierKeys.Shift),

				// following 3 move to next section and select it  ( Tab )
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					NextSection,	// Action
					(Int64)MaskedEditorStates.LastSection,	// Disallowed state		
					(Int64)( MaskedEditorStates.Character | MaskedEditorStates.TabBySections ),	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.None),
					ModifierKeys.Shift),

				new CommandWrapper(	
					SetPivot,	// Action
					(Int64)MaskedEditorStates.LastSection,	// Disallowed state		
					(Int64)( MaskedEditorStates.Character | MaskedEditorStates.TabBySections ),	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.None),
					ModifierKeys.Shift),

				new CommandWrapper(	
					SelectSection,	// Action
					(Int64)MaskedEditorStates.LastSection,	// Disallowed state		
					(Int64)( MaskedEditorStates.Character | MaskedEditorStates.TabBySections ),	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.None),
					ModifierKeys.Shift),
				// ------------------------------------------------------------------------}

				// Following is for selecting all text via Ctrl + A
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					SelectAll,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.A, ModifierKeys.Control),
					ModifierKeys.Shift | ModifierKeys.Alt ),
				// ------------------------------------------------------------------------}

				// following 4 move to prev section and select it  ( Shift+Tab )
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					PreviousSection,	// Action
					(Int64)MaskedEditorStates.FirstSection,	// Disallowed state		
					(Int64)MaskedEditorStates.TabBySections,	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.Shift),
					ModifierKeys.None),

				new CommandWrapper(	
					SetPivot,	// Action
					(Int64)MaskedEditorStates.FirstSection,	// Disallowed state		
					(Int64)MaskedEditorStates.TabBySections,	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.Shift),
					ModifierKeys.None),

				new CommandWrapper(	
					SelectSection,	// Action
					(Int64)MaskedEditorStates.FirstSection,	// Disallowed state		
					(Int64)MaskedEditorStates.TabBySections,	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.Shift),
					ModifierKeys.None),
				// ------------------------------------------------------------------------}

				// Delete when nothing is selected and with no special keys ( Delete )
				new CommandWrapper(	
					Delete,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedEditorStates.Selected,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.None),
					ModifierKeys.Shift),

				// Delete when nothing is selected and with no special keys ( Delete )
				new CommandWrapper(	
					Delete,	// Action
					(Int64)MaskedEditorStates.Selected,	// Disallowed state
					(Int64)MaskedEditorStates.Character,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.None),
					ModifierKeys.Shift),

				// Delete with shift when nothing is selected ( Shift + Delete )
				new CommandWrapper(	
					Backspace,	// Action
					(Int64)MaskedEditorStates.Selected,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Shift),
					ModifierKeys.Control),

				// Following 2 are for Ctl + Delete when nothing is selected  ( Ctl + Delete )
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					AfterLastCharacter,	// Action
					(Int64)MaskedEditorStates.Selected,	// Disallowed state
					(Int64)MaskedEditorStates.Character,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Control),
					ModifierKeys.Shift),

				new CommandWrapper(	
					Delete,	// Action
					(Int64)MaskedEditorStates.Selected,	// Disallowed state
					(Int64)MaskedEditorStates.Character,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Control),
					ModifierKeys.Shift),
				// ------------------------------------------------------------------------}

				// following 2 are for backspacing in insert and ovwrtite mode
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					Backspace,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.Selected,	// Required state
					new KeyGesture(Key.Back, ModifierKeys.None),
					ModifierKeys.None),

				new CommandWrapper(	
					Backspace,	// Action
					(Int64)( MaskedEditorStates.Selected | MaskedEditorStates.FirstCharacter ),	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Back, ModifierKeys.None),
					ModifierKeys.None),
				// ------------------------------------------------------------------------}

				// Copy through Ctrl + C
				new CommandWrapper(	
					Copy,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.Selected,	// Required state
					new KeyGesture(Key.C, ModifierKeys.Control),
					ModifierKeys.None),

				// Cut through Ctrl + X
				new CommandWrapper(	
					Cut,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.Selected,	// Required state
					new KeyGesture(Key.X, ModifierKeys.Control),
					ModifierKeys.None),

				// Paste through Ctrl + V
				new CommandWrapper(	
					Paste,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.V, ModifierKeys.Control),
					ModifierKeys.None),

				// Toggle insert mode through ( Insert )
				new CommandWrapper(	
					ToggleInsertionMode,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Insert, ModifierKeys.None),
					ModifierKeys.Shift | ModifierKeys.Control ),

				// Up key action ( Up )
				new CommandWrapper(	
					SpinUp,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.CanSpinUp,	// Required state
					new KeyGesture(Key.Up, ModifierKeys.None),
					ModifierKeys.None ),

				// Down key action ( Down )
				new CommandWrapper(	
					SpinDown,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.CanSpinDown,	// Required state
					new KeyGesture(Key.Down, ModifierKeys.None),
					ModifierKeys.None ),

				// Eat Up and Down keys
				new CommandWrapper(	
					NotACommand,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new InputGesture[] { 
						new KeyGesture(Key.Up, ModifierKeys.None), 
						new KeyGesture(Key.Down, ModifierKeys.None),
						// SSP 6/7/07 BR22768
						// 
						new KeyGesture(Key.Left, ModifierKeys.None ),
						new KeyGesture(Key.Right, ModifierKeys.None ),
						new KeyGesture(Key.Left, ModifierKeys.Shift ),
						new KeyGesture(Key.Right, ModifierKeys.Shift ),
						new KeyGesture(Key.Left, ModifierKeys.Control ),
						new KeyGesture(Key.Right, ModifierKeys.Control ),
					},
					ModifierKeys.None ),

				// Copy, Paste, Cut ( Ctrl+Insert, Shift+Insert, Shift+Delete etc...)
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					Copy,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.Selected,	// Required state
					new KeyGesture(Key.Insert, ModifierKeys.Control),
					ModifierKeys.Shift ),

				new CommandWrapper(	
					Paste,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Insert, ModifierKeys.Shift),
					ModifierKeys.Control ),

				new CommandWrapper(	
					Cut,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.Selected,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Shift),
					ModifierKeys.Control ),
				// ------------------------------------------------------------------------}

				// When the '.' numpad key is pressed, it's supposed to go to the fraction part
				// regardless of whether the decimal separator is comma or dot. This is because
				// the key code is always Decimal for numpad '.' key.
				// 
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					NextSection,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.NextSectionFraction,	// Required state
					new KeyGesture(Key.Decimal, ModifierKeys.None),
					ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt ),

				new CommandWrapper(	
					SetPivot,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.NextSectionFraction,	// Required state
					new KeyGesture(Key.Decimal, ModifierKeys.None),
					ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt ),
				// ------------------------------------------------------------------------}

				// The following two are for Undo and Redo - Ctrl+Z and Ctrl+Y
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					Undo,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.CanUndo,	// Required state
					new KeyGesture(Key.Z, ModifierKeys.Control),
					ModifierKeys.Shift | ModifierKeys.Alt ),

				new CommandWrapper(	
					Redo,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedEditorStates.CanRedo,	// Required state
					new KeyGesture(Key.Y, ModifierKeys.Control),
					ModifierKeys.Shift | ModifierKeys.Alt ),

                // AS 10/15/08 TFS8969
				// Eat Backspace keys
				new CommandWrapper(	
					NotACommand,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new InputGesture[] { 
						new KeyGesture(Key.Back, ModifierKeys.None), 
						new KeyGesture(Key.Back, ModifierKeys.Shift ),
						new KeyGesture(Key.Back, ModifierKeys.Control ),
					},
					ModifierKeys.None ),
				// ------------------------------------------------------------------------}

                // AS 9/5/08 NA 2008 Vol 2
				// The following is for toggling the dropdown - Alt-Up, Alt-Down, F4
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					ToggleDropDown,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new InputGesture[] { new KeyGesture(Key.Up, ModifierKeys.Alt), new KeyGesture(Key.Down, ModifierKeys.Alt), new KeyGesture(Key.F4, ModifierKeys.None) },
					ModifierKeys.Control ),

                // close on enter/space
				new CommandWrapper(	
					ToggleDropDown,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedEditorStates.IsDropDownOpen,	// Required state
					new InputGesture[] { new KeyGesture(Key.Enter, ModifierKeys.None), new KeyGesture(Key.Space, ModifierKeys.None) },
					ModifierKeys.Control ),

                // close on escape
				new CommandWrapper(	
					ToggleDropDown,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedEditorStates.IsDropDownOpen,	// Required state
					new InputGesture[] { new KeyGesture(Key.Escape, ModifierKeys.None) },
					ModifierKeys.Control ),

			};
		}
		#endregion //CommandWrapper Definitions

		// ====================================================================================================================================


		static MaskedEditorCommands( )
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<XamMaskedEditor>.Initialize( MaskedEditorCommands.GetCommandWrappers() );
		}


		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands( )
		{
		}

		private static MaskedEditorCommands g_instance;
		internal static MaskedEditorCommands Instance
		{
			get
			{
				if ( g_instance == null )
					g_instance = new MaskedEditorCommands( );

				return g_instance;
			}
		}
	}

	#endregion // MaskedEditorCommands Class

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