using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using Infragistics.Controls.Editors;
using System.Linq;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Editors.Primitives
{
	#region MaskedInputCommandId Enum

	/// <summary>
	/// Identifies <see cref="XamMaskedInput"/> commands.
	/// </summary>
	public enum MaskedInputCommandId
	{
		/// <summary>
		/// Does nothing. <b>NotACommand</b> is always ignored.
		/// </summary>
		NotACommand,

		/// <summary>
		/// Command for moving the caret position to the next character.
		/// </summary>
		NextCharacter,

		/// <summary>
		/// Command for moving the caret position to the next section.
		/// </summary>
		NextSection,

		/// <summary>
		/// Command for moving the caret position after the last character.
		/// </summary>
		AfterLastCharacter,

		/// <summary>
		/// Command for setting the pivot at where the caret is currently.
		/// </summary>
		SetPivot,

		/// <summary>
		/// Command for moving the caret position to the previous character.
		/// </summary>
		PreviousCharacter,

		/// <summary>
		/// Command for moving the caret position to the previous section.
		/// </summary>
		PreviousSection,

		/// <summary>
		/// Command for moving the caret position to the first character.
		/// </summary>
		FirstCharacter,

		/// <summary>
		/// Command for selecting all the characters of the current section.
		/// </summary>
		SelectSection,

		/// <summary>
		/// Command for selecting all the characters.
		/// </summary>
		SelectAll,

		/// <summary>
		/// Command for deleting selected text. If nothing is selected then the character at 
		/// the current caret position will be deleted.
		/// </summary>
		Delete,

		/// <summary>
		/// Command for performing a 'Backspace' key operation. This command deletes the selected text. If nothing 
		/// is selected then the character before the current caret position will be deleted.
		/// </summary>
		Backspace,

		/// <summary>
		/// Command for copying the selected text.
		/// </summary>
		Copy,

		/// <summary>
		/// Command for cutting the selected text.
		/// </summary>
		Cut,

		/// <summary>
		/// Command for pasting clipboard contents into the editor.
		/// </summary>
		Paste,

		/// <summary>
		/// Command for undoing last change to the value.
		/// </summary>
		Undo,

		/// <summary>
		/// Command for undoing last change to the value.
		/// </summary>
		Redo,

		/// <summary>
		/// Command for toggling insert mode.
		/// </summary>
		ToggleInsertionMode,

		/// <summary>
		/// Command for spinning up the value of the section.
		/// </summary>
		SpinUp,

		/// <summary>
		/// Command for spinning down the value of the section.
		/// </summary>
		SpinDown,

		/// <summary>
		/// Command for toggling the drop down state of the <b>XamDateTimeEditor</b>.
		/// </summary>
		ToggleDropDown
	} 

	#endregion // MaskedInputCommandId Enum

	#region MaskedInputStates Enum

	/// <summary>
	/// Represents the different states of the MaskedInput control.  Used to evaluate whether a specific command can be executed.
	/// </summary>
	[Flags]
	public enum MaskedInputStates : long
	{
		/// <summary>
		/// caret is positioned right before a display char
		/// </summary>
		Character = 0x00000001,

		/// <summary>
		/// the caret is positioned right before the first display char
		/// </summary>
		FirstCharacter = 0x00000002,

		/// <summary>
		/// the caret is positioned right before the last display char 
		/// </summary>
		LastCharacter = 0x00000004,

		/// <summary>
		/// the caret is in the first section
		/// </summary>
		FirstSection = 0x00000008,

		/// <summary>
		/// the caret is in the last section
		/// </summary>
		LastSection = 0x00000010,

		/// <summary>
		/// the caret is positioned right before the first character in a section
		/// </summary>
		FirstCharacterInSection = 0x00000020,

		/// <summary>
		/// the caret is positioned right before the last character in a section
		/// </summary>
		LastCharacterInSection = 0x00000040,

		/// <summary>
		/// the caret is positioned after the last display character		
		/// </summary>
		AfterLastCharacter = 0x00000100,

		/// <summary>
		/// some text is selected
		/// </summary>
		Selected = 0x00000200,

		/// <summary>
		/// Mask has not been initialized, and thus no sections or
		/// display chars collection exists
		/// </summary>
		Uninitialized = 0x00000400,

		/// <summary>
		/// the caret is in the first edit section
		/// </summary>
		FirstEditSection = 0x00000800,

		/// <summary>
		/// the caret is in the last edit section
		/// </summary>
		LastEditSection = 0x00001000,

		/// <summary>
		/// State where the editor is not in edit mode.
		/// </summary>
		NotInEditMode = 0x00002000,

		/// <summary>
		/// State where the editor permits tabbing by sections based on the TabNavigation proeprty.
		/// </summary>
		TabBySections = 0x00004000,

		// When the '.' numpad key is pressed, it's supposed to go to the fraction part
		// regardless of whether the decimal separator is comma or dot. This is because
		// the key code is always Decimal for numpad '.' key.
		// 
		/// <summary>
		/// Next edit section is a fraction part.
		/// </summary>
		NextSectionFraction = 0x00008000,

		/// <summary>
		/// Current section can be decremented.
		/// </summary>
		CanSpinDown = 0x00010000,

		/// <summary>
		/// Current section can be incremented.
		/// </summary>
		CanSpinUp = 0x00020000,

		/// <summary>
		/// Can perform Undo action.
		/// </summary>
		CanUndo = 0x00040000,

		/// <summary>
		/// Can perform Redo action.
		/// </summary>
		CanRedo = 0x00080000,

		// AS 9/5/08 NA 2008 Vol 2
		/// <summary>
		/// There is an associated dropdown
		/// </summary>
		HasDropDown = 0x00100000,

		/// <summary>
		/// The associated dropdown is open
		/// </summary>
		IsDropDownOpen = 0x00200000,

		/// <summary>
		/// The associated dropdown is open
		/// </summary>
		All = 0xffffffff,
	};

	#endregion // MaskedInputStates Enum

	#region MaskedInputCommand Class

	/// <summary>
	/// Class for all commands that deal with a <see cref="XamMaskedInput"/> and derived editors.
	/// </summary>
	public class MaskedInputCommand : CommandBase
	{
		#region Private Members

		private MaskedInputCommandId _commandId;

		#endregion //Private Members

		#region Constructor

		internal MaskedInputCommand( MaskedInputCommandId commandId )
		{
			_commandId = commandId;
		}

		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region CommandId

		/// <summary>
		/// Gets the associated command id.
		/// </summary>
		public MaskedInputCommandId CommandId
		{
			get
			{
				return _commandId;
			}
		}

		#endregion // CommandId

		#endregion // Public Properties 

		#endregion // Properties

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
			XamMaskedInput editor = GetMaskedInput( parameter );
			if ( editor != null )
			{
				CommandSource source = this.CommandSource;

				return editor.CanExecuteCommand( _commandId, null != source ? source.SourceElement : null );
			}

			return base.CanExecute( parameter );
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter">The <see cref="XamMaskedInput"/> object that will be executed against.</param>
		public override void Execute( object parameter )
		{
			XamMaskedInput editor = GetMaskedInput( parameter );
			if ( editor != null )
			{
				CommandSource source = this.CommandSource;

				editor.ExecuteCommand( _commandId,
					null != source ? source.ParameterResolved : null, 
					null != source ? source.SourceElement : null 
				);

				if ( null != source )
					source.Handled = true;

				return;
			}

			base.Execute( parameter );
		}

		private static XamMaskedInput GetMaskedInput( object parameter )
		{
			XamMaskedInput editor = parameter as XamMaskedInput;
			
			return editor;
		}

		#endregion Execute

		#endregion // Public

		#endregion // Overrides
	}
	#endregion // CalendarCommandBase Class

	#region MaskedInputCommandSource Class
	/// <summary>
	/// The command source object for <see cref="MaskedInputCommand"/> object.
	/// </summary>
	public class MaskedInputCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the CalendarCommand which is to be executed by the command.
		/// </summary>
		public MaskedInputCommandId CommandId
		{
			get;
			set;
		}

		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand( )
		{
			return new MaskedInputCommand( this.CommandId );
		}
	}

	#endregion // MaskedInputCommandSource Class

	#region MaskedInputCommandsHelper Class

	/// <summary>
	/// Provides the list of RoutedCommands supported by the XamMaskedInput. 
	/// </summary>
	internal class MaskedInputCommandsHelper : CommandsHelper<MaskedInputCommandId>
	{
		#region Constructor

		internal MaskedInputCommandsHelper( )
			: base( GetCommandWrappers( ), GetMinimumStates( ) )
		{
		}

		#endregion // Constructor

		#region Minimum States

		private static CommandDefinition[] GetMinimumStates( )
		{
			return new CommandDefinition[]
			{
				new CommandDefinition( MaskedInputCommandId.NextCharacter,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)MaskedInputStates.Character ),

				new CommandDefinition( MaskedInputCommandId.NextSection, 
									(Int64)(MaskedInputStates.LastSection | MaskedInputStates.IsDropDownOpen),
									(Int64)MaskedInputStates.Character ),
				new CommandDefinition( MaskedInputCommandId.AfterLastCharacter,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)0 ),
				new CommandDefinition( MaskedInputCommandId.SetPivot,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)0 ),
				new CommandDefinition( MaskedInputCommandId.PreviousCharacter,
									(Int64)(MaskedInputStates.FirstCharacter | MaskedInputStates.IsDropDownOpen),
									(Int64)0 ),
				new CommandDefinition( MaskedInputCommandId.PreviousSection,
									(Int64)(MaskedInputStates.FirstSection | MaskedInputStates.IsDropDownOpen),
									(Int64)0 ),
				new CommandDefinition( MaskedInputCommandId.FirstCharacter,
									(Int64)(MaskedInputStates.FirstCharacter | MaskedInputStates.IsDropDownOpen),
									(Int64)0 ),
				new CommandDefinition( MaskedInputCommandId.SelectSection,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)0 ),
		
				new CommandDefinition( MaskedInputCommandId.Backspace,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)0 ),
				new CommandDefinition( MaskedInputCommandId.ToggleInsertionMode,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)0 ),
				new CommandDefinition( MaskedInputCommandId.SpinUp,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)MaskedInputStates.CanSpinUp ),
				new CommandDefinition( MaskedInputCommandId.SpinDown,
									(Int64)MaskedInputStates.IsDropDownOpen,
									(Int64)MaskedInputStates.CanSpinDown ),

				new CommandDefinition( MaskedInputCommandId.ToggleDropDown,
									(Int64)0,
									(Int64)MaskedInputStates.HasDropDown ),

				new CommandDefinition( MaskedInputCommandId.Copy,
									(Int64)0,
									(Int64)MaskedInputStates.Selected ),

				new CommandDefinition( MaskedInputCommandId.Cut,
									(Int64)0,
									(Int64)MaskedInputStates.Selected ),

				new CommandDefinition( MaskedInputCommandId.Delete,
									(Int64)0,
									(Int64)0 ),

				new CommandDefinition( MaskedInputCommandId.Undo,
									(Int64)0,
									(Int64)MaskedInputStates.CanUndo ),

				new CommandDefinition( MaskedInputCommandId.Redo,
									(Int64)0,
									(Int64)MaskedInputStates.CanRedo )
			};
		}

		#endregion // Minimum States

		#region CommandWrapper Definitions

		private static CommandWrapper[] GetCommandWrappers( )
		{
			return new CommandWrapper[] {
				//					RoutedCommand					StateDisallowed					StateRequired								InputGestures
				//					=============					===============					=============								=============
				// move the caret to right by one ( Right )
				new CommandWrapper(	
					MaskedInputCommandId.NextCharacter,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedInputStates.Character,	// Required state
					new KeyGesture[] { new KeyGesture(Key.Right, ModifierKeys.None), new KeyGesture(Key.Right, ModifierKeys.Shift) },
					ModifierKeys.Control ),

				// move to next section	( Ctl + Right )
				new CommandWrapper(	
					MaskedInputCommandId.NextSection,	// Action
					(Int64)MaskedInputStates.LastSection,	// Disallowed state		
					(Int64)MaskedInputStates.Character,	// Required state
					new KeyGesture(Key.Right, ModifierKeys.Control),
					ModifierKeys.None ),

				// move to after last character when in last section ( Ctl + Right )
				new CommandWrapper(	
					MaskedInputCommandId.AfterLastCharacter,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedInputStates.LastSection,	// Required state
					new KeyGesture(Key.Right, ModifierKeys.Control),
					ModifierKeys.None ),

				// move the pivot to the new position (Right)
				new CommandWrapper(	
					MaskedInputCommandId.SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Right, ModifierKeys.None),
					ModifierKeys.Shift),

				// move the caret to left by one ( Left )
				new CommandWrapper(	
					MaskedInputCommandId.PreviousCharacter,	// Action
					(Int64)MaskedInputStates.FirstCharacter,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture[] { new KeyGesture(Key.Left, ModifierKeys.None), new KeyGesture(Key.Left, ModifierKeys.Shift) },
					ModifierKeys.Control),

				// move to prev section	( Ctl + Left )
				new CommandWrapper(	
					MaskedInputCommandId.PreviousSection,	// Action
					(Int64)MaskedInputStates.FirstSection,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Left, ModifierKeys.Control),
					ModifierKeys.None),

				// move to next section	( Ctl + Left )
				new CommandWrapper(	
					MaskedInputCommandId.FirstCharacter,	// Action
					(Int64)MaskedInputStates.FirstCharacter,	// Disallowed state		
					(Int64)MaskedInputStates.FirstSection,	// Required state
					new KeyGesture(Key.Left, ModifierKeys.Control),
					ModifierKeys.None),

				// move the pivot to the new position
				new CommandWrapper(	
					MaskedInputCommandId.SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Left, ModifierKeys.None),
					ModifierKeys.Shift),

				// move the caret to beggining of the text  ( Home )
				new CommandWrapper(	
					MaskedInputCommandId.FirstCharacter,	// Action
					(Int64)MaskedInputStates.FirstCharacter,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture[] { new KeyGesture(Key.Home, ModifierKeys.None),new KeyGesture(Key.Home, ModifierKeys.Shift) },
					ModifierKeys.None),

				// move the pivot to beggining of the text  ( Home )
				new CommandWrapper(	
					MaskedInputCommandId.SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.Home, ModifierKeys.None),
					ModifierKeys.Shift),

				// move the caret to the end of the text   ( End )
				new CommandWrapper(	
					MaskedInputCommandId.AfterLastCharacter,	// Action
					(Int64)MaskedInputStates.AfterLastCharacter,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture[] { new KeyGesture(Key.End, ModifierKeys.None), new KeyGesture(Key.End, ModifierKeys.Shift)  },
					ModifierKeys.None),

				// move the pivot to the end of the text ( End )
				new CommandWrapper(	
					MaskedInputCommandId.SetPivot,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture(Key.End, ModifierKeys.None),
					ModifierKeys.Shift),

				// following 3 move to next section and select it  ( Tab )
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					MaskedInputCommandId.NextSection,	// Action
					(Int64)MaskedInputStates.LastSection,	// Disallowed state		
					(Int64)( MaskedInputStates.Character | MaskedInputStates.TabBySections ),	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.None),
					ModifierKeys.Shift),

				new CommandWrapper(	
					MaskedInputCommandId.SetPivot,	// Action
					(Int64)MaskedInputStates.LastSection,	// Disallowed state		
					(Int64)( MaskedInputStates.Character | MaskedInputStates.TabBySections ),	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.None),
					ModifierKeys.Shift),

				new CommandWrapper(	
					MaskedInputCommandId.SelectSection,	// Action
					(Int64)MaskedInputStates.LastSection,	// Disallowed state		
					(Int64)( MaskedInputStates.Character | MaskedInputStates.TabBySections ),	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.None),
					ModifierKeys.Shift),
				// ------------------------------------------------------------------------}

				// Following is for selecting all text via Ctrl + A
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					MaskedInputCommandId.SelectAll,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.A, ModifierKeys.Control),
					ModifierKeys.Shift | ModifierKeys.Alt ),
				// ------------------------------------------------------------------------}

				// following 4 move to prev section and select it  ( Shift+Tab )
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					MaskedInputCommandId.PreviousSection,	// Action
					(Int64)MaskedInputStates.FirstSection,	// Disallowed state		
					(Int64)MaskedInputStates.TabBySections,	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.Shift),
					ModifierKeys.None),

				new CommandWrapper(	
					MaskedInputCommandId.SetPivot,	// Action
					(Int64)MaskedInputStates.FirstSection,	// Disallowed state		
					(Int64)MaskedInputStates.TabBySections,	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.Shift),
					ModifierKeys.None),

				new CommandWrapper(	
					MaskedInputCommandId.SelectSection,	// Action
					(Int64)MaskedInputStates.FirstSection,	// Disallowed state		
					(Int64)MaskedInputStates.TabBySections,	// Required state
					new KeyGesture(Key.Tab, ModifierKeys.Shift),
					ModifierKeys.None),
				// ------------------------------------------------------------------------}

				// Delete when nothing is selected and with no special keys ( Delete )
				new CommandWrapper(	
					MaskedInputCommandId.Delete,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedInputStates.Selected,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.None),
					ModifierKeys.Shift),

				// Delete when nothing is selected and with no special keys ( Delete )
				new CommandWrapper(	
					MaskedInputCommandId.Delete,	// Action
					(Int64)MaskedInputStates.Selected,	// Disallowed state
					(Int64)MaskedInputStates.Character,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.None),
					ModifierKeys.Shift),

				// Delete with shift when nothing is selected ( Shift + Delete )
				new CommandWrapper(	
					MaskedInputCommandId.Backspace,	// Action
					(Int64)MaskedInputStates.Selected,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Shift),
					ModifierKeys.Control),

				// Following 2 are for Ctl + Delete when nothing is selected  ( Ctl + Delete )
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					MaskedInputCommandId.AfterLastCharacter,	// Action
					(Int64)MaskedInputStates.Selected,	// Disallowed state
					(Int64)MaskedInputStates.Character,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Control),
					ModifierKeys.Shift),

				new CommandWrapper(	
					MaskedInputCommandId.Delete,	// Action
					(Int64)MaskedInputStates.Selected,	// Disallowed state
					(Int64)MaskedInputStates.Character,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Control),
					ModifierKeys.Shift),
				// ------------------------------------------------------------------------}

				// following 2 are for backspacing in insert and ovwrtite mode
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					MaskedInputCommandId.Backspace,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.Selected,	// Required state
					new KeyGesture(Key.Back, ModifierKeys.None),
					ModifierKeys.None),

				new CommandWrapper(	
					MaskedInputCommandId.Backspace,	// Action
					(Int64)( MaskedInputStates.Selected | MaskedInputStates.FirstCharacter ),	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Back, ModifierKeys.None),
					ModifierKeys.None),
				// ------------------------------------------------------------------------}

				// Copy through Ctrl + C
				new CommandWrapper(	
					MaskedInputCommandId.Copy,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.Selected,	// Required state
					new KeyGesture(Key.C, ModifierKeys.Control),
					ModifierKeys.None),

				// Cut through Ctrl + X
				new CommandWrapper(	
					MaskedInputCommandId.Cut,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.Selected,	// Required state
					new KeyGesture(Key.X, ModifierKeys.Control),
					ModifierKeys.None),

				// Paste through Ctrl + V
				new CommandWrapper(	
					MaskedInputCommandId.Paste,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.V, ModifierKeys.Control),
					ModifierKeys.None),

				// Toggle insert mode through ( Insert )
				new CommandWrapper(	
					MaskedInputCommandId.ToggleInsertionMode,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Insert, ModifierKeys.None),
					ModifierKeys.Shift | ModifierKeys.Control ),

				// Up key action ( Up )
				new CommandWrapper(	
					MaskedInputCommandId.SpinUp,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.CanSpinUp,	// Required state
					new KeyGesture(Key.Up, ModifierKeys.None),
					ModifierKeys.None ),

				// Down key action ( Down )
				new CommandWrapper(	
					MaskedInputCommandId.SpinDown,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.CanSpinDown,	// Required state
					new KeyGesture(Key.Down, ModifierKeys.None),
					ModifierKeys.None ),

				// Eat Up and Down keys
				new CommandWrapper(	
					MaskedInputCommandId.NotACommand,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture[] { 
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
					MaskedInputCommandId.Copy,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.Selected,	// Required state
					new KeyGesture(Key.Insert, ModifierKeys.Control),
					ModifierKeys.Shift ),

				new CommandWrapper(	
					MaskedInputCommandId.Paste,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture(Key.Insert, ModifierKeys.Shift),
					ModifierKeys.Control ),

				new CommandWrapper(	
					MaskedInputCommandId.Cut,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.Selected,	// Required state
					new KeyGesture(Key.Delete, ModifierKeys.Shift),
					ModifierKeys.Control ),
				// ------------------------------------------------------------------------}

				// When the '.' numpad key is pressed, it's supposed to go to the fraction part
				// regardless of whether the decimal separator is comma or dot. This is because
				// the key code is always Decimal for numpad '.' key.
				// 
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					MaskedInputCommandId.NextSection,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.NextSectionFraction,	// Required state
					new KeyGesture(Key.Decimal, ModifierKeys.None),
					ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt ),

				new CommandWrapper(	
					MaskedInputCommandId.SetPivot,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.NextSectionFraction,	// Required state
					new KeyGesture(Key.Decimal, ModifierKeys.None),
					ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt ),
				// ------------------------------------------------------------------------}

				// The following two are for Undo and Redo - Ctrl+Z and Ctrl+Y
				// ------------------------------------------------------------------------{
				new CommandWrapper(	
					MaskedInputCommandId.Undo,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.CanUndo,	// Required state
					new KeyGesture(Key.Z, ModifierKeys.Control),
					ModifierKeys.Shift | ModifierKeys.Alt ),

				new CommandWrapper(	
					MaskedInputCommandId.Redo,	// Action
					(Int64)0,	// Disallowed state
					(Int64)MaskedInputStates.CanRedo,	// Required state
					new KeyGesture(Key.Y, ModifierKeys.Control),
					ModifierKeys.Shift | ModifierKeys.Alt ),

                // AS 10/15/08 TFS8969
				// Eat Backspace keys
				new CommandWrapper(	
					MaskedInputCommandId.NotACommand,	// Action
					(Int64)0,	// Disallowed state
					(Int64)0,	// Required state
					new KeyGesture[] { 
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
					MaskedInputCommandId.ToggleDropDown,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new KeyGesture[] { new KeyGesture(Key.Up, ModifierKeys.Alt), new KeyGesture(Key.Down, ModifierKeys.Alt), new KeyGesture(Key.F4, ModifierKeys.None) },
					ModifierKeys.Control ),

                // close on enter/space
				new CommandWrapper(	
					MaskedInputCommandId.ToggleDropDown,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedInputStates.IsDropDownOpen,	// Required state
					new KeyGesture[] { new KeyGesture(Key.Enter, ModifierKeys.None), new KeyGesture(Key.Space, ModifierKeys.None) },
					ModifierKeys.Control ),

                // close on escape
				new CommandWrapper(	
					MaskedInputCommandId.ToggleDropDown,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)MaskedInputStates.IsDropDownOpen,	// Required state
					new KeyGesture[] { new KeyGesture(Key.Escape, ModifierKeys.None) },
					ModifierKeys.Control ),

			};
		}
		#endregion //CommandWrapper Definitions
	} 

	#endregion // MaskedInputCommandsHelper Class
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