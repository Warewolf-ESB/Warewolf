using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Editors.Primitives;
using System.Globalization;
using System.Windows.Documents;
using System.Windows.Threading;


namespace Infragistics.Controls.Editors
{

	internal class EditInfo
	{
		#region Data Structures

		private class EditorSnapshot
		{
			internal int _caretPos, _pivotPos;
			internal string _text;

			internal EditorSnapshot( EditInfo editInfo )
			{
				_text = editInfo.GetText( InputMaskMode.IncludeBoth );
				_caretPos = editInfo.CaretPosition;
				_pivotPos = editInfo.PivotPosition;
			}

			internal void Apply( EditInfo editInfo )
			{
				editInfo.SetText( _text );
				editInfo.PivotPosition = _pivotPos;
				editInfo.CaretPosition = _caretPos;
			}
		}

        // JJD 9/08/09
        // Added proxy class to prevent rooting of the XamMaskedInput if we don't
        // unwire the static events. This was caught by unit tests.
        #region TextBoxEventProxy

        private class TextBoxEventProxy
        {
            #region Private Members

            private WeakReference _editInfoRef;
            private TextBox _textBoxWired;

			private InputMethod _inputMethodWired;


            #endregion //Private Members	
    
            #region Constructor

            internal TextBoxEventProxy(EditInfo editInfo)
            {
                this._editInfoRef = new WeakReference(editInfo);
            }

            #endregion //Constructor	
    
            #region Properties

            internal EditInfo EditInfo
            {
                get { return CoreUtilities.GetWeakReferenceTargetSafe(this._editInfoRef) as EditInfo; }
            }

            #endregion //Properties	
    
            #region Methods

            #region Internal Methods

            #region UnwireInputMethod_StateChanged

            internal void UnwireInputMethod_StateChanged()
            {

                InputMethod inputMethod = InputMethod.Current;

                if (this._inputMethodWired != null &&
                    this._inputMethodWired == inputMethod)
                {
					// SSP 11/5/10 TFS36555
					// Enclosed the existing code in try-catch block. Apparently running as a non-admin user can
					// lead to an exception from within InitializeCompartmentEventSink method of InputMethod.
					// 
					try
					{
						this._inputMethodWired.StateChanged -= new InputMethodStateChangedEventHandler( InputMethod_StateChanged );
					}
					catch
					{
					}

                    this._inputMethodWired = null;
                }

            }

            #endregion //UnwireInputMethod_StateChanged	
    
            #region UnwireTextBoxEvents

            internal void UnwireTextBoxEvents()
            {
                if (this._textBoxWired != null)
                {

                    this._textBoxWired.ClearValue(TextBox.ContextMenuProperty);

					CommandManager.RemovePreviewCanExecuteHandler(this._textBoxWired, new CanExecuteRoutedEventHandler(ImeTextBox_PreviewCanExecuteCommand));
                    TextCompositionManager.RemoveTextInputStartHandler(this._textBoxWired, ImeTextBox_TextInputStart);


                    this._textBoxWired = null;
                }
            }

            #endregion //UnwireTextBoxEvents	
    
            #region WireInputMethod_StateChanged

            internal void WireInputMethod_StateChanged()
            {

                InputMethod inputMethod = InputMethod.Current;

                if (this._inputMethodWired != inputMethod)
                {
                    this.UnwireInputMethod_StateChanged();

                    this._inputMethodWired = inputMethod;

					if ( null != inputMethod )
					{
						// SSP 11/5/10 TFS36555
						// Enclosed the existing code in try-catch block. Apparently running as a non-admin user can
						// lead to an exception from within InitializeCompartmentEventSink method of InputMethod.
						// 
						try
						{
							this._inputMethodWired.StateChanged += new InputMethodStateChangedEventHandler( InputMethod_StateChanged );
						}
						catch
						{
						}
					}
                }

			}

            #endregion //WireInputMethod_StateChanged	
                
            #region WireTextBoxEvents

            internal void WireTextBoxEvents(TextBox textBox)
            {
                if (this._textBoxWired != textBox)
                {
                    this.UnwireTextBoxEvents();

                    this._textBoxWired = textBox;

                    if (null != this._textBoxWired)
                    {

                        CommandManager.AddPreviewCanExecuteHandler(this._textBoxWired, new CanExecuteRoutedEventHandler(ImeTextBox_PreviewCanExecuteCommand));
                        TextCompositionManager.AddTextInputStartHandler(this._textBoxWired, ImeTextBox_TextInputStart);

                    }
                }
            }

            #endregion //WireTextBoxEvents	
    
            #endregion //Internal Methods	
    
            #region Private Methods

            private void OnCleanup()
            {
                this.UnwireTextBoxEvents();
                this.UnwireInputMethod_StateChanged();
            }

            #endregion //Private Methods	

            #region Event Handlers

            private void ImeTextBox_TextInputStart(object sender, TextCompositionEventArgs e)
            {
                EditInfo info = this.EditInfo;

                if (info != null)
                    info.ImeTextBox_TextInputStart(sender, e);
                else
                    this.OnCleanup();
            }


            private void ImeTextBox_PreviewCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
            {
                EditInfo info = this.EditInfo;

                if (info != null)
                    info.ImeTextBox_PreviewCanExecuteCommand(sender, e);
                else
                    this.OnCleanup();
            }



            private void InputMethod_StateChanged(object sender, InputMethodStateChangedEventArgs e)
            {
                EditInfo info = this.EditInfo;

                if (info != null)
                    info.InputMethod_StateChanged(sender, e);
                else
                    this.OnCleanup();
            }


            #endregion //Event Handlers	
        
            #endregion //Methods
        }

        #endregion //TextBoxEventProxy	
    
		#endregion // Data Structures

		#region private variables

		private MaskInfo _maskInfo;

		// SSP 10/12/08 BR35095
		// Now we are making use of XamMaskedInput.InsertMode property directly.
		// 
		//private bool _insertMode; // = false;

		private int _caretPosition; // = 0;
		private int _pivotPosition; // = 0;

		private char _lastIllegalChar; // = (char)0;
		private DisplayCharBase _lastIllegalDisplayChar; // = null;

		internal bool _inOnTextChanged; // = false;

		// SSP 3/7/07 BR19849
		// 
		private Stack<EditorSnapshot> _undoHistory = null;
		private Stack<EditorSnapshot> _redoHistory = null;
		private bool _inUndoRedoHelper = false;
        
        // JJD 9/08/09
        // Added proxy class to prevent rooting of the XamMaskedInput if we don't
        // unwire the static events. This was caught by unit tests.
        private TextBoxEventProxy _staticEventProxy;


		// SSP 3/23/09 IME
		// 
		internal MaskedInputTextBox _imeTextBox;
		internal bool _inSyncWithIMETextBox;
		internal bool _inSyncIMETextBox;
		
		internal Utils.Range _imeTextBox_PendingDelete;

		// SSP 11/23/11 TFS93756
		// 
		private bool _skipNextTextInput;

		// SSP 6/29/12 TFS115883
		// 
		private MaskedInputStates _prevCurrentState;
		private DispatcherOperation _notifyCommandsManagerOperation;

		#endregion //private variables
	
		#region Constructors






		internal EditInfo( MaskInfo maskInfo )
		{
			this.Initialize( maskInfo );
		}

		#endregion //Constructors

		#region Private/Internal Methods

		#region CalculateInitialCaretPos

		private int CalculateInitialCaretPos( )
		{
			int initializeCaretPos = -1;

			// SSP 8/16/10 TFS36760
			// If in overwrite mode then don't position the caret after the last character
			// of the number section because in overwrite mode, the input is always advanced
			// to the right and right-to-left inputting doesn't work when there's an editable 
			// section after the number section because that's where the caret will be 
			// advanced to on the first character input. Enclosed the existing code into the 
			// if block.
			// 
			if ( this.InsertMode )
			{
				FractionPartContinuous fp = this.LastEditSection as FractionPartContinuous;
				if ( null != fp && null != fp.LastDisplayChar )
				{
					initializeCaretPos = 1 + fp.LastDisplayChar.OverallIndexInEdit;
				}
				else
				{
					// Otherwise position the caret in the number section.
					//
					NumberSection ns = this.FirstEditSection as NumberSection;
					if ( null != ns )
					{
						// SSP 4/29/06 BR11118
						// If there is no input then the caret should be
						// positioned after the last display char.
						// 
						// ----------------------------------------------------
						if ( null != ns.FirstFilledChar )
							initializeCaretPos = ns.FirstFilledChar.OverallIndexInEdit;
						else if ( null != ns.LastDisplayChar )
							initializeCaretPos = 1 + ns.LastDisplayChar.OverallIndexInEdit;
						




						// ----------------------------------------------------
					}
					else if ( null != FirstEditDisplayChar )
					{
						initializeCaretPos = this.FirstEditDisplayChar.OverallIndexInEdit;
					}
				}
			}

			return initializeCaretPos;
		}

		#endregion // CalculateInitialCaretPos

		#region Initialize
		
		private void Initialize( MaskInfo maskInfo )
		{
			this.Reset( );

			if ( null == maskInfo )
				throw new ArgumentNullException( "maskInfo" );

			this._maskInfo = maskInfo;

			
			
			

			// SSP 5/6/05 BR03077
			// If we are in the right to left continuous number editing mode then initialize
			// the caret position to after the last character.
			//
			// ------------------------------------------------------------------------------
			this.InitializeInitialCaretPos( );
			// ------------------------------------------------------------------------------

			// SSP 3/7/07 BR19849
			// 
			this.ManageUndoHistory_TextChanged( false, null );
		}

		#endregion // Initialize

		#region InitializeInitialCaretPos

		internal bool InitializeInitialCaretPos( )
		{
			int initializeCaretPos = this.CalculateInitialCaretPos( );

			if ( initializeCaretPos >= 0 )
			{
				this._caretPosition = this._pivotPosition = initializeCaretPos;
				return true;
			}

			return false;
		} 

		#endregion // InitializeInitialCaretPos

		#region NotifyStateChanged

		// SSP 6/29/12 TFS115883
		// 
		internal void NotifyStateChanged( )
		{
			this.NotifyCommandsManagerAsync( );
		} 

		#endregion // NotifyStateChanged

		#region NotifyCommandsManagerAsync

		// SSP 6/29/12 TFS115883
		// 
		internal void NotifyCommandsManagerAsync( )
		{
			if ( null == _notifyCommandsManagerOperation )
			{
				XamMaskedInput editor = this.MaskedInput;
				Dispatcher dispatcher = null != editor ? editor.Dispatcher : null;
				if ( null != dispatcher )
					_notifyCommandsManagerOperation = dispatcher.BeginInvoke( new Action( this.NotifyCommandsManagerHandler ) );
			}
		}

		// SSP 6/29/12 TFS115883
		// 
		private void NotifyCommandsManagerHandler( )
		{
			_notifyCommandsManagerOperation = null;

			MaskedInputStates state = this.CurrentState;
			if ( _prevCurrentState != state )
			{
				_prevCurrentState = state;
				CommandSourceManager.NotifyCanExecuteChanged( typeof( MaskedInputCommand ) );
			}
		}

		#endregion // NotifyCommandsManagerAsync

		#region Reset

		private void Reset( )
		{
			this._caretPosition = 0;
			this._pivotPosition = 0;

			// SSP 10/12/08 BR35095
			// Now we are making use of XamMaskedInput.InsertMode property directly.
			// 
			//this._insertMode	= true;

			this._lastIllegalChar = (char)0;
			this._lastIllegalDisplayChar = null;
		}
		
		#endregion // Reset

		#endregion Private/Internal Methods

		#region Private/Internal properties

        // JJD 9/08/09
        // Added proxy class to prevent rooting of the XamMaskedInput if we don't
        // unwire the static events. This was caught by unit tests.
        #region TextBoxEventProxy

        private TextBoxEventProxy EventProxy
        {
            get
            {
                if (this._staticEventProxy == null)
                    this._staticEventProxy = new TextBoxEventProxy(this);

                return this._staticEventProxy;
            }
        }

        #endregion //TextBoxEventProxy	

		#region MaskInfo

		internal MaskInfo MaskInfo
		{
			get
			{
				return this._maskInfo;
			}
		}

		#endregion //MaskInfo

		#region MaskedInput

		internal XamMaskedInput MaskedInput
		{
			get
			{
				return this._maskInfo.MaskedInput;
			}
		}

		#endregion // MaskedInput
    
		#endregion // Private/Internal properties

		#region OnInvalidOperation






		internal void OnInvalidOperation( InvalidOperationEventArgs e )
		{
			this.MaskedInput.OnInvalidOperation( e );
		}

		#endregion // OnInvalidOperation

		#region OnInvalidChar






		internal void OnInvalidChar( InvalidCharEventArgs e )
		{
			this.MaskedInput.OnInvalidChar( e );
		}

		#endregion // OnInvalidChar

		#region OnTextChanged






		internal void OnTextChanged( bool viaUserAction )
		{
			this.OnTextChanged( viaUserAction, null );
		}






		internal void OnTextChanged( bool viaUserAction, MaskedInputCommandId? command )
		{
			XamMaskedInput maskedInput = this.MaskedInput;

			// SSP 1/19/10 TFS30067
			// Wrap the logic in BeginSyncValueProperties and EndSyncValueProperties calls to make
			// sure that we raise value changed events after the synchronization process is complete
			// so in case a value property is changed in one of the events, we correctly 
			// resynchronize all the value properties to the new value.
			// 
			maskedInput.BeginSyncValueProperties( );

			bool origInOnTextChanged = _inOnTextChanged;
			_inOnTextChanged = true;

			try
			{
				// Sync with the MaskedInput's Text property.
				// 
				string text = XamMaskedInput.GetText( this.Sections, this.MaskInfo.DataMode, this.MaskInfo, string.Empty );
				maskedInput.Text = text;

				// SSP 3/23/09 IME
				// 
				this.SyncIMETextBox( );

				// SSP 3/7/07 BR19849
				// 
				this.ManageUndoHistory_TextChanged( viaUserAction, command );
			}
			finally
			{
				_inOnTextChanged = origInOnTextChanged;

				// SSP 1/19/10 TFS30067
				// 
				maskedInput.EndSyncValueProperties( );

				// SSP 6/29/12 TFS115883
				// 
				this.NotifyStateChanged( );
			}
		}

		#endregion // OnTextChanged

		#region GetCurrentSection



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        // SSP 5/5/10 TFS31525
        // 
		//internal SectionBase GetCurrentSection()
        internal SectionBase GetCurrentSection( bool returnPrevRightToLeftSection )
		{
			DisplayCharBase dc = this.GetCurrentDisplayChar( );
			
            // SSP 5/5/10 TFS31525
            // 
            // --------------------------------------------------------------------------
			//if ( null == dc )
			//	return null;
            // 
            //return dc.Section;
            if ( null == dc || ( ! dc.IsEditable && dc.Section.FirstDisplayChar == dc ) )
            {
                SectionBase prevDcSection = null == dc ? this.LastEditSection 
                    : ( null != dc.PrevDisplayChar ? dc.PrevDisplayChar.Section : null );

                if ( null != prevDcSection && IsRightToLeft( prevDcSection ) )
                    return prevDcSection;
            }

            return null != dc ? dc.Section : null;
            // --------------------------------------------------------------------------			
		}

		#endregion // GetCurrentSection

		#region GetText



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal string GetText( InputMaskMode maskMode )
		{
			return XamMaskedInput.GetText( this.Sections, maskMode, this.MaskInfo );
		}

		#endregion // GetText

		#region SetText



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal int SetText( string text )
		{
			string oldText = this.GetText( InputMaskMode.IncludeBoth );

			try
			{
				// SSP 2/6/02
				// Commented out below code. Now using the code in ParsedMask
				// to set the text.
				//
				int matchedChars = 0;

                // AS 10/17/08 TFS8886
                //matchedChars = XamMaskedInput.SetText(this.Sections, text, this.MaskInfo.PromptChar, this.MaskInfo.PadChar);
                matchedChars = XamMaskedInput.SetText(this.Sections, text, this.MaskInfo);

				this.DirtyMaskElement( );
				
				return matchedChars;
			}
			finally
			{
				if ( null == oldText )
					oldText = "";

				string newText = this.GetText( InputMaskMode.IncludeBoth );

				// JJD 12/10/02 - FxCop
				// Pass the culture info in explicitly
				//if ( 0 != string.Compare( oldText, newText ) )
				if ( 0 != Utils.CompareStrings( oldText, newText, false ) )
				{
					this.OnTextChanged( false );
				}
			}
		}

		#endregion // SetText

		#region InvalidateMaskElement

		internal void InvalidateMaskElement( )
		{
			this.DirtyMaskElement( );
		}

		#endregion // InvalidateMaskElement

		#region DirtyMaskElement

		internal void DirtyMaskElement( )
		{
			
			this.MaskedInput.InvalidateArrange( );
		}

		#endregion // DirtyMaskElement

		#region IME Related

		#region HookUnhookInputMethodHelper

		// SSP 3/23/09 IME
		// 
		internal void HookUnhookInputMethodHelper( bool hook )
		{
            if (hook)
                this.EventProxy.WireInputMethod_StateChanged();
            else
                this.EventProxy.UnwireInputMethod_StateChanged();
		}

		#endregion // HookUnhookInputMethodHelper

		#region ImeTextBox_PreviewCanExecuteCommand


		// SSP 3/23/09 IME
		// 
		private void ImeTextBox_PreviewCanExecuteCommand( object sender, CanExecuteRoutedEventArgs e )
		{
			RoutedCommand routedCommand = e.Command as RoutedCommand;
			// SSP 11/16/11 TFS95801
			// If the routed command is not an editing command then don't stop it from being executed.
			// Apparently the framework traverses up the element ancestor chain to execute a command
			// and if a change is focus scope is encoutered, it restarts executing the command from
			// the focused element. That means that we are going be called for commands that are not
			// specific to textbox. Enclosed the existing code into the if block.
			// 
			if ( null != routedCommand && typeof( EditingCommands ) == routedCommand.OwnerType )
			{
				bool canExecute = false;
				bool handled = true;
				bool continueRouting = true;

				if ( EditingCommands.MoveLeftByCharacter == routedCommand
					|| EditingCommands.MoveRightByCharacter == routedCommand
					|| EditingCommands.MoveToLineStart == routedCommand
					|| EditingCommands.MoveToLineEnd == routedCommand )
				{
					continueRouting = false;
				}

				e.CanExecute = canExecute;
				e.Handled = handled;
				e.ContinueRouting = continueRouting;
			}
		}


		#endregion // ImeTextBox_PreviewCanExecuteCommand

		#region ImeTextBox_TextInput

		/// <summary>
		/// Params 'e' and 'text' are exclusive.
		/// </summary>
		internal void ImeTextBox_TextInput( object sender, TextCompositionEventArgs e, string text )
		{
			if ( !this.ReadOnly 
				// SSP 11/23/11 TFS93756
				// 
				&& ! _skipNextTextInput 
				)
			{
				int offset = -1;
				if ( null != e )
				{
					text = e.Text;

					TextComposition cc = e.TextComposition as TextComposition;
					if ( null != cc )
					{
						try
						{

						if ( cc is FrameworkTextComposition )
						{
							System.Reflection.PropertyInfo prop = typeof( FrameworkTextComposition ).GetProperty( "ResultOffset" );
							if ( null != prop )
								offset = (int)prop.GetValue( cc, null );
						}

						}
						catch
						{
						}
					}
				}

				// If there was some text selected when the user started entering text via IME,
				// we need to delete that text.
				// 
				if ( null != _imeTextBox_PendingDelete )
				{
					Utils.Range range = _imeTextBox_PendingDelete;
					_imeTextBox_PendingDelete = null;
					this.Select( range.Start, range.Length );
					// SSP 5/16/12 TFS111480
					// Only delete here if the offset provided by the composition is not the same
					// as where the original selection started. The reason for making this change is
					// that the InternalProcessChar method called by the ProcessTextInput call below
					// will delete the selection based on whether the character typed matches the
					// display char.
					// 
					//if ( this.IsAnyTextSelected )
					if ( this.IsAnyTextSelected && offset >= 0 && offset != range.Start )
						this.Delete( );
				}

				if ( offset >= 0 )
					this.SetCaretPivot( offset );

				if ( this.ProcessTextInput( text, true ) && null != e )
					e.Handled = true;
			}

			// SSP 11/23/11 TFS93756
			// Ensure we null this out otherwise we'll bypass further text inputs.
			// 
			_imeTextBox_PendingDelete = null;
		}

		#endregion // ImeTextBox_TextInput

		#region ImeTextBox_TextInputStart

		// SSP 3/23/09 IME
		// 
		internal void ImeTextBox_TextInputStart( object sender, TextCompositionEventArgs e )
		{
			if ( !_skipNextTextInput )
			{
				// We need to delete any currently selected text when the user starts typing via
				// IME. Also when ime composition is committed, we need to enter it at the caret
				// position where the composition started, not where the caret position ends up
				// being after composition is typed.
				// 
				_imeTextBox_PendingDelete = new Utils.Range( this.SelectionStart, this.SelectionLength );
			}
		}

		#endregion // ImeTextBox_TextInputStart

		#region IsTextInputInProgress

		internal bool IsTextInputInProgress
		{
			get
			{
				return null != _imeTextBox_PendingDelete;
			}
		}

		#endregion // IsTextInputInProgress

		#region InitializeIMETextBox

		// SSP 3/23/09 IME
		// 
		internal void InitializeIMETextBox( DependencyObject focusSite )
		{
			XamMaskedInput maskedInput = this.MaskedInput;

			if ( null != _imeTextBox )
			{
                // JJD 9/08/09
                // Added proxy class to prevent rooting of the XamMaskedInput if we don't
                // unwire the static events. This was caught by unit tests.
                //CommandManager.RemovePreviewCanExecuteHandler(_imeTextBox, new CanExecuteRoutedEventHandler(ImeTextBox_PreviewCanExecuteCommand));
				//TextCompositionManager.RemoveTextInputStartHandler( _imeTextBox, ImeTextBox_TextInputStart );
                this.EventProxy.UnwireTextBoxEvents();

				_imeTextBox = null;
			}

			MaskedInputTextBox textBox = focusSite as MaskedInputTextBox;

			_imeTextBox = textBox;

			if ( null != _imeTextBox )
			{

				// ReadLocalValue is for making sure that if the ContextMenu property is explicitly 
				// set to null on the MaskedInput, then we also set the context menu of the text 
				// box to null. 
				// 
				ContextMenu menu = null;
				if ( maskedInput.ReadLocalValue( XamMaskedInput.ContextMenuProperty ) != null )
				{
					menu = maskedInput.ContextMenu;
					if ( null == menu )
						menu = maskedInput.CreateContextMenu( );
				}

				_imeTextBox.ContextMenu = menu;


				_imeTextBox.Initialize( this.MaskedInput );

				// SSP 1/18/10 TFS27161
				// 
				this.SynchronizeTextBoxToInputMethodState( );

                // JJD 9/08/09
                // Added proxy class to prevent rooting of the XamMaskedInput if we don't
                // unwire the static events. This was caught by unit tests.
                //CommandManager.AddPreviewCanExecuteHandler(_imeTextBox, new CanExecuteRoutedEventHandler(ImeTextBox_PreviewCanExecuteCommand));
				//TextCompositionManager.AddTextInputStartHandler( _imeTextBox, ImeTextBox_TextInputStart );
                this.EventProxy.WireTextBoxEvents(_imeTextBox);

				this.SyncIMETextBox( );
			}
		}

		#endregion // InitializeIMETextBox

		#region InputMethod_StateChanged

		// SSP 3/23/09 IME
		// 

		private void InputMethod_StateChanged( object sender, InputMethodStateChangedEventArgs e )
		{
			// SSP 1/18/10 TFS27161
			// Synchronize IsInputMethodEnabled setting with the text box.
			// 
			this.SynchronizeTextBoxToInputMethodState( );
		}


		// SSP 1/18/10 TFS27161
		// 
		private void SynchronizeTextBoxToInputMethodState( )
		{
			XamMaskedInput maskedInput = this.MaskedInput;
			

			if ( null != _imeTextBox )
				InputMethod.SetIsInputMethodEnabled( _imeTextBox, maskedInput.IsInputMethodEnabled );

		}

		#endregion // InputMethod_StateChanged

		#region MapCaretPosFromImeTextBox

		// SSP 3/23/09 IME
		// 
		internal int MapCaretPosFromImeTextBox( int pos )
		{
			// Added support for 0 character prompt character. When the prompt char
			// is 0, that character is not included in the display string.
			// 
			// ------------------------------------------------------------------------
			int maxCaretPos = this.GetTotalNumberOfDisplayChars( );

			int target = pos;
			
			
			
			int candidate = Math.Min( pos, maxCaretPos );

			int delta = 0;
			int iiTarget;

			do
			{
				iiTarget = this.MapCaretPosToImeTextBox( candidate );
				if ( iiTarget == target )
					break;

				if ( 0 == delta )
					delta = iiTarget < target ? 1 : -1;

				candidate += delta;
			}
			while ( candidate >= 0 && candidate <= maxCaretPos );

			return Math.Max( 0, Math.Min( candidate, maxCaretPos ) );
			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------
		}

		#endregion // MapCaretPosFromImeTextBox

		#region MapCaretPosToImeTextBox

		// SSP 3/23/09 IME
		// 
		private int MapCaretPosToImeTextBox( int pos )
		{
			int delta = 0;
			foreach ( DisplayCharBase dc in this.Sections.AllDisplayCharacters )
			{
				if ( dc.OverallIndexInEdit < pos )
				{
					// SSP 9/20/11 TFS88629
					// Added support for 0 character prompt character. When the prompt char
					// is 0, that character is not included in the display string.
					// 
					// ------------------------------------------------------------------------
					//if ( dc.DrawComma )
					//	delta++;
					delta += GetDrawStringHelper( dc, null ) - 1;
					// ------------------------------------------------------------------------
				}
				else
					break;
			}

			return pos + delta;
		}

		#endregion // MapCaretPosToImeTextBox

		#region GetDrawString

		// SSP 9/20/11 TFS88629
		// Added support for 0 character prompt character.
		// 
		private int GetDrawStringHelper( DisplayCharBase dc, StringBuilder sb )
		{
			int r = 0;

			char c = dc.GetChar( InputMaskMode.IncludeBoth, _maskInfo.PromptChar, _maskInfo.PadChar );

			if ( 0 == c && dc is DecimalSeperatorChar )
				c = dc.Char;

			if ( 0 != c )
			{
				r++;
				if ( null != sb )
					sb.Append( c );

				DigitChar digitChar = dc as DigitChar;
				if ( null != digitChar && digitChar.ShouldIncludeComma( InputMaskMode.IncludeBoth ) )
				{
					r++;

					if ( null != sb )
						sb.Append( _maskInfo.CommaChar );
				}
			}

			return r;
		}

		// SSP 9/20/11 TFS88629
		// Added support for 0 character prompt character.
		// 
		private string GetDrawString( )
		{
			StringBuilder sb = new StringBuilder( 4 + this.GetTotalNumberOfDisplayChars( ) );

			foreach ( DisplayCharBase dc in this.Sections.AllDisplayCharacters )
				this.GetDrawStringHelper( dc, sb );

			return sb.ToString( );
		}

		#endregion // GetDrawString

		#region SyncIMETextBox

		// SSP 3/23/09 IME
		// 
		internal void SyncIMETextBox( bool fromIsInEditModeChanged = false )
		{
			if ( this.IsTextInputInProgress )
				return;

			if ( _inSyncIMETextBox || _inSyncWithIMETextBox )
				return;

			_inSyncIMETextBox = true;
			try
			{
				if ( null != _imeTextBox )
				{
					XamMaskedInput maskedInput = this.MaskedInput;

					string text;
					if ( _maskInfo.IsBeingEditedAndFocused
						// SSP 11/8/11 TFS93756
						// If the input is not parsable, then returning the DisplayText below will correspond to
						// the previously parseable value, which we don't want to display when the user tries 
						// to leave the control with a non-parseable input.
						// 
						|| null != maskedInput && ! maskedInput.IsValueValid && ! this.IsInputParseable( )
						)
					{
						// SSP 9/20/11 TFS88629
						// Use the new GetDrawString method instead which takes into account the 
						// prompt char being 0.
						// 
						//string text = this.GetText( MaskMode.IncludeBoth );
						text = this.GetDrawString( );

						// If focusing the control via mouse, we need to position the caret at a location
						// where the user can start entering value.
						if ( fromIsInEditModeChanged && _imeTextBox._enteringEditModeViaMouseDown.HasValue && this.IsInputNull( ) )
							this.InitializeInitialCaretPos( );
					}
					else
					{
						text = _maskInfo.MaskedInput.DisplayText;

						if ( this.IsInputNull( ) )
						{
							string nullText = this.MaskedInput.NullText;
							if ( InputMaskMode.IncludeBoth != _maskInfo.DisplayMode || !string.IsNullOrEmpty( nullText ) )
								text = nullText;
						}
					}

					_imeTextBox.Text = text;

					this.SyncSelectionOnImeTextBoxHelper( );

					// AS 8/2/11
					// Let the textbox know when we have updated its Text & Selection info so it 
					// can snapshot the current state.
					//
					_imeTextBox.OnSyncComplete();
				}
			}
			finally
			{
				_inSyncIMETextBox = false;
			}
		}

		#endregion // SyncIMETextBox

		#region SyncWithIMETextBox

		// SSP 3/23/09 IME
		// 
		internal void SyncWithIMETextBox( )
		{
			if ( _inSyncIMETextBox || _inSyncWithIMETextBox )
				return;

			_inSyncWithIMETextBox = true;

			try
			{
				if ( null != _imeTextBox )
				{
					int textBoxStart = _imeTextBox.SelectionStart;
					int start = this.MapCaretPosFromImeTextBox( textBoxStart );
					int end = 0 == _imeTextBox.SelectionLength ? start
						: this.MapCaretPosFromImeTextBox( textBoxStart + _imeTextBox.SelectionLength );

					if ( _maskInfo.IsBeingEditedAndFocused )
					{
						bool adjusted = false;

						if ( _imeTextBox._enteringEditModeViaMouseDown.HasValue )
						{
							int newPos = -1;

							if ( start == end )
							{
								int maxCaretPos = this.MapCaretPosToImeTextBox( this.GetTotalNumberOfDisplayChars( ) );

								if ( 0 == start || this.IsInputNull( ) )
								{
									newPos = this.CalculateInitialCaretPos( );
								}
								else if ( maxCaretPos == start )
								{
									DisplayCharBase dc = this.LastFilledDisplayChar;
									if ( null != dc )
										newPos = 1 + dc.OverallIndexInEdit;
								}
							}

							if ( newPos >= 0 )
							{
								start = end = newPos;
								adjusted = true;
							}
						}

						this.SelectionStart = start;
						this.SelectionLength = end - start;

						if ( adjusted )
							this.SyncSelectionOnImeTextBoxHelper( );
					}
				}
			}
			finally
			{
				_inSyncWithIMETextBox = false;
			}
		}

		#endregion // SyncWithIMETextBox

		#region SyncSelectionOnImeTextBoxHelper

		private void SyncSelectionOnImeTextBoxHelper( )
		{
			if ( this.IsTextInputInProgress )
				return;

			int thisSelectionStart = this.SelectionStart;

			int start = this.MapCaretPosToImeTextBox( thisSelectionStart );
			int end = 0 == this.SelectionLength ? start
				: this.MapCaretPosToImeTextBox( thisSelectionStart + this.SelectionLength );

			int length = end - start;

			if ( null != _imeTextBox && ( _imeTextBox.SelectionStart != start || _imeTextBox.SelectionLength != length ) )
				_imeTextBox.Select( start, length );
		}

		#endregion // SyncSelectionOnImeTextBoxHelper

		#endregion // IME Related







		internal void SetValue( object val )
		{		
			// SSP 1/22/02
			// If the the text has changed, then fire TextChanged event
			//
			string oldText = this.GetText( InputMaskMode.IncludeBoth );

			try
			{
				XamMaskedInput.SetDataValue( 
					this.Sections, 
					this.MaskInfo.DataType,
					val,
					this.MaskInfo );
			}
			finally
			{
				this.DirtyMaskElement( );

				// SSP 1/22/02
				// If the the text has changed, then fire TextChanged event
				//				
				string newText = this.GetText( InputMaskMode.IncludeBoth );			
				// JJD 12/10/02 - FxCop
				// Pass the culture info in explicitly
				//if ( 0 != String.Compare( newText, oldText ) )
				if ( 0 != Utils.CompareStrings( newText, oldText, false ) )
				{
					// SSP 1/24/02
					// Took the try-catch out around the event firing
					//
					//try
					//{
					this.OnTextChanged( false );				
					//}
					//catch ( Exception )
					//{
					//}
				}
			}
		}

		#region Value






		internal object Value
		{
			get
			{
				return XamMaskedInput.GetDataValue( this.MaskInfo );
			}
			set
			{
				this.SetValue( value );
			}
		} 

		#endregion // Value

		#region IsInputParseable

		// SSP 11/8/11 TFS93756
		// 
		internal bool IsInputParseable( )
		{
			try
			{
				object val = this.Value;

				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion // IsInputParseable
      





		internal SectionsCollection Sections
		{
			get
			{
				return this.MaskInfo.Sections;
			}
		}

		#region GetCurrentState

		internal long GetCurrentStateLong( long statesToQuery )
		{
			return (long)GetCurrentState( (MaskedInputStates)statesToQuery );
		}

		// SSP 8/16/10 TFS27897 - Optimizations
		// Added logic to calculate only the states that are queried for.
		// 
		internal MaskedInputStates GetCurrentState( MaskedInputStates statesToQuery )
		{
			// SSP 6/8/01 UWM 15, 16, 18
			// When no mask is initialized, state is UnInitialized
			if ( !this.IsInitialized )
				return MaskedInputStates.Uninitialized;

			XamMaskedInput maskedInput = this.MaskedInput;

			MaskedInputStates state = 0;

			if ( 0 != ( MaskedInputStates.Character & statesToQuery ) )
			{
				if ( null != this.GetCurrentDisplayChar( ) )
				{
					state |= MaskedInputStates.Character;
				}
				else
				{
					// if there is no current display character, that means that the caret
					// is positioned after the last display char
					state |= MaskedInputStates.AfterLastCharacter;
				}
			}

			if ( 0 != ( MaskedInputStates.Selected & statesToQuery ) && this.SelectionLength > 0 )
			{
				state |= MaskedInputStates.Selected;
			}

			if ( 0 != ( MaskedInputStates.FirstSection & statesToQuery ) && null != this.FirstSection && this.CurrentSection == this.FirstSection )
			{
				state |= MaskedInputStates.FirstSection;
			}

			if ( 0 != ( MaskedInputStates.LastSection & statesToQuery ) && null != this.LastSection && this.CurrentSection == this.LastSection )
			{
				state |= MaskedInputStates.LastSection;
			}

			if ( 0 != ( MaskedInputStates.FirstEditSection & statesToQuery ) )
			{
				if ( null != this.FirstEditSection && this.CurrentSection == this.FirstEditSection )
				{
					state |= MaskedInputStates.FirstEditSection;
				}
				else if (
					// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
					// Use the IsRightToLeft instead.
					// 
					//null != this.FirstEditSection && this.FirstEditSection is NumberSection &&
					IsRightToLeft( this.FirstEditSection )
					&& this.FirstEditSection.LastDisplayChar.NextDisplayChar == this.GetCurrentDisplayChar( ) )
				{
					state |= MaskedInputStates.FirstEditSection;
				}
			}

			if ( 0 != ( MaskedInputStates.LastEditSection & statesToQuery ) && null != this.LastEditSection && this.CurrentSection == this.LastEditSection )
			{
				state |= MaskedInputStates.LastEditSection;
			}

			if ( 0 != ( MaskedInputStates.FirstCharacter & statesToQuery ) && null != this.FirstSection &&
				this.GetCurrentDisplayChar( ) == this.FirstSection.FirstDisplayChar )
			{
				state |= MaskedInputStates.FirstCharacter;
			}

			if ( 0 != ( MaskedInputStates.LastCharacter & statesToQuery ) && null != this.LastSection &&
				this.GetCurrentDisplayChar( ) == this.LastSection.LastDisplayChar )
			{
				state |= MaskedInputStates.LastCharacter;
			}

			if ( 0 != ( MaskedInputStates.FirstCharacterInSection & statesToQuery ) && null != this.CurrentSection &&
				this.CurrentSection.FirstDisplayChar == this.GetCurrentDisplayChar( ) )
			{
				state |= MaskedInputStates.FirstCharacterInSection;
			}

			if ( 0 != ( MaskedInputStates.LastCharacterInSection & statesToQuery ) && null != this.CurrentSection &&
				this.CurrentSection.LastDisplayChar == this.GetCurrentDisplayChar( ) )
			{
				state |= MaskedInputStates.LastCharacterInSection;
			}

			// SSP 8/16/02 UWM117
			// If we are on the first section and that section is
			// number section, the caret would actually be positioned
			// before the first character of the next section. So 
			// take into account that here.
			//
			if ( 0 != ( MaskedInputStates.FirstSection & statesToQuery )
				&& 0 == ( state & MaskedInputStates.FirstSection ) )
			{
				DisplayCharBase dc = this.GetCurrentDisplayChar( );

				if ( null != dc && null != dc.PrevDisplayChar )
				{
					SectionBase section = dc.PrevDisplayChar.Section;

					if ( null != section && null == section.PreviousSection &&
						this.IsRightToLeft( section ) )
					{
						state |= MaskedInputStates.FirstSection;
					}
				}
			}

			// SSP 10/31/05 BR07039 BR07339
			// When the '.' numpad key is pressed, it's supposed to go to the fraction part
			// regardless of whether the decimal separator is comma or dot. This is because
			// the key code is always Decimal for numpad '.' key.
			// 
			if ( 0 != ( MaskedInputStates.NextSectionFraction & statesToQuery )
				&& null != this.CurrentSection && this.CurrentSection.NextEditSection is FractionPart )
			{
				state |= MaskedInputStates.NextSectionFraction;
			}

			
			
			if ( 0 != ( MaskedInputStates.CanSpinUp & statesToQuery ) && this.CanSpin( true ) )
			{
				state |= MaskedInputStates.CanSpinUp;
			}

			
			
			if ( 0 != ( MaskedInputStates.CanSpinDown & statesToQuery ) && this.CanSpin( false ) )
			{
				state |= MaskedInputStates.CanSpinDown;
			}

			
			
			if ( 0 != ( MaskedInputStates.CanUndo & statesToQuery ) && this.HasUndoHistory )
			{
				state |= MaskedInputStates.CanUndo;
			}

			
			
			if ( 0 != ( MaskedInputStates.CanRedo & statesToQuery ) && this.HasRedoHistory )
			{
				state |= MaskedInputStates.CanRedo;
			}

			
			
			// AS 9/5/08 NA 2008 Vol 2
			//if ( null != maskedInput && MaskedEditTabNavigation.NextSection == maskedInput.TabNavigationResolved )
			if ( null != maskedInput )
			{
				if ( 0 != ( MaskedInputStates.TabBySections & statesToQuery )
					&& MaskedEditTabNavigation.NextSection == maskedInput.SectionTabNavigationResolved )
				{
					state |= MaskedInputStates.TabBySections;
				}

				if ( 0 != ( ( MaskedInputStates.HasDropDown | MaskedInputStates.IsDropDownOpen ) & statesToQuery ) )
				{
					// AS 9/5/08 NA 2008 Vol 2
					if ( maskedInput.HasDropDown )
					{
						state |= MaskedInputStates.HasDropDown;

						if ( maskedInput.HasOpenDropDown )
							state |= MaskedInputStates.IsDropDownOpen;
					}
				}
			}

			return state;
		} 

		#endregion // GetCurrentState

		#region CurrentState

		/// <summary>
		/// Returns bit flags that signify the current editing state of the control.
		/// </summary>
		internal MaskedInputStates CurrentState
		{
			get
			{
				// SSP 8/16/10 TFS27897 - Optimizations
				// Added logic to calculate only the states that are queried for.
				// Moved the existing logic into the new GetCurrentState method.
				// 
				return this.GetCurrentState( MaskedInputStates.All );
			}
		} 

		#endregion // CurrentState


		/// <summary>
		/// validates the from section. It returns true to proceed, and false to
		/// cancel the moving of input position from section to to section
		/// </summary>
		/// <returns></returns>
		internal bool ValidateChangeSection( SectionBase from, SectionBase to )
		{
			EditSectionBase fromEditSection = from as EditSectionBase;

			if ( null != fromEditSection )
			{
				fromEditSection.ValidateSection( );
			}

			return true;
		}


		/// <summary>
		/// Calls ValidateSection on all the editable sections and returns
		/// true if all such calls return true. Otherwise it returns false.
		/// </summary>
		/// <returns></returns>
		internal bool ValidateAllSections( )
		{
			return this.ValidateAllSections( false );
		}

        // SSP 10/27/05 BR07075
		// Added an overload that takes in loosingFocus parameter.
		// 
		/// <summary>
		/// Calls ValidateSection on all the editable sections and returns
		/// true if all such calls return true. Otherwise it returns false.
		/// </summary>
		/// <returns></returns>
		internal bool ValidateAllSections( bool loosingFocus )
		{
			if ( !this.IsInitialized )
				return false;

			// SSP 9/15/11 TFS87816
			// 
			string oldText = this.GetText( InputMaskMode.IncludeBoth );
			try
			{
				// SSP 10/27/05 BR07075
				// Pass the loosingFocus parameter.
				// 
				//return XamMaskedInput.ValidateAllSections( this.Sections );
				return XamMaskedInput.ValidateAllSections( this.Sections, loosingFocus );
			}
			finally
			{
				// SSP 9/15/11 TFS87816
				// 
				string newText = this.GetText( InputMaskMode.IncludeBoth );
				if ( oldText != newText )
					this.OnTextChanged( false );
			}
		}

		/// <summary>
		/// returns true if any text is selected, otherwise false
		/// </summary>
		internal bool IsAnyTextSelected
		{
			get
			{
				return this.CaretPosition != this.PivotPosition;
			}
		}

		// SSP 3/14/12 TFS95408
		// 
		/// <summary>
		/// Indicates if the specified display char is selected.
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>
		internal bool IsDisplayCharSelected( DisplayCharBase dc )
		{
			int index = dc.OverallIndexInEdit;
			int start = this.SelectionStart;
			int end = this.SelectionStart + this.SelectionLength - 1;

			return start <= index && index <= end;
		}

		/// <summary>
		/// indicates if the caret position is after the last char
		/// </summary>
		internal bool IsAfterLastCharacter
		{
			get
			{
				return this.CaretPosition >= this.GetTotalNumberOfDisplayChars( );
			}
		}

		/// <summary>
		/// indicates if the caret position is at the first char
		/// </summary>
		internal bool IsAtFirstChar
		{
			get
			{
				return 0 == this.CaretPosition;
			}
		}

		#region CanDeleteWithoutShiftingAcrrossSectionBoundaries

		
#region Infragistics Source Cleanup (Region)























































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)






































































#endregion // Infragistics Source Cleanup (Region)

 

		#endregion //CanDeleteWithoutShiftingAcrrossSectionBoundaries

		#region DeleteWithoutShiftingAcrrossSectionBoundaries

		internal bool DeleteWithoutShiftingAcrrossSectionBoundaries( )
		{
			if ( this.CanDelete( ) )
			{		
				if ( !this.IsAnyTextSelected )
				{
					DisplayCharBase dc = this.GetCurrentDisplayChar( );
					EditSectionBase editSection = dc.Section as EditSectionBase;

					if ( null != editSection )
						return editSection.DeleteCharAndShift( dc.Index );
					else
						return false;
				}
				else
				{
					int selStart = this.SelectionStart;  // index of the first char in selection
					int selLen   = this.SelectionLength;
					int selEnd	 = selStart + selLen - 1; // index of the last char in the selection

					Debug.Assert( selLen > 0, "selection length must be greater that 0 if something is selected" );
					Debug.Assert( selStart >= 0, "sel start must not be a negative number" );
					Debug.Assert( selStart < this.GetTotalNumberOfDisplayChars( ), 
						"selection start can not be beyond total number of display chars when something is selected" );

					bool couldDelete = true;

					for ( int i = 0; i < this.Sections.Count; i++ )
					{
						EditSectionBase section = this.Sections[ i ] as EditSectionBase;

						// skip edit sections
						if ( null == section )
							continue;

						// skip sections that do not fall within the selection
						if ( section.LastDisplayChar.OverallIndexInEdit < selStart
							|| section.FirstDisplayChar.OverallIndexInEdit > selEnd )
							continue;
				
						// skip sections that completely fall within the selection, because
						// we can always erase the whole section. (no shifting is necessary when
						// deleting all the characters in a section)
						if ( section.FirstDisplayChar.OverallIndexInEdit >= selStart && 
							section.LastDisplayChar.OverallIndexInEdit <= selEnd )
						{
							section.EraseAllChars( );
							continue;
						}

						// if end of the section is selected, then we can skip because
						// no characters to shift after the selection in the section
						// SSP 5/15/06 BR12377
						// 
						//if ( section.LastDisplayChar.OverallIndexInEdit <= selEnd )
						if ( ! this.IsRightToLeft( section ) && section.LastDisplayChar.OverallIndexInEdit <= selEnd )
						{
							section.EraseChars( this.GetDisplayCharAtPosition( selStart ).Index, section.DisplayChars.Count - 1 );
							continue;
						}

						// no we have a section that is partially selected, so see if chars in there
						// can be shifted
						int selStartWithinSection = System.Math.Max( selStart, section.FirstDisplayChar.OverallIndexInEdit );
						int selEndWithinSection = System.Math.Min( selEnd, section.LastDisplayChar.OverallIndexInEdit );
						int numOfCharsToShift = selEndWithinSection - selStartWithinSection + 1;

						DisplayCharBase selStartDisplayChar = this.GetDisplayCharAtPosition( selStartWithinSection );

						Debug.Assert( null != selStartDisplayChar, "null returned by GetDisplayCharAtPosition at a valid pos" );
						if ( null == selStartDisplayChar ) // this should never happen
							continue;

						// SSP 5/15/06 BR12377
						// Added the if block and eclosed the existing code in the else block.
						// Number sections which are right to left need to be handled differently.
						// 
						if ( this.IsRightToLeft( section ) || section is FractionPart )
						{
							if ( ! section.DeleteCharsAndShift( selStartDisplayChar.Index + numOfCharsToShift - 1, numOfCharsToShift ) )
								couldDelete = false;
						}
						// if a section can not shift, then return false
						else if ( !section.ShiftLeft( selStartDisplayChar.Index, numOfCharsToShift ) )
						{
							couldDelete = false;
						}
					}

					if ( couldDelete )
					{
						// SSP 5/15/06 BR12377
						// For number sections, since characters are shifted right to left, after deletion
						// the caret should be at where the selection ended.
						// 
						// ------------------------------------------------------------------------------------
						//this.CaretPosition = selStart;
						// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
						// Use the IsRightToLeft instead.
						// 
						//if ( this.GetSectionContainingPosition( selStart ) is NumberSection )
						if ( IsRightToLeft( this.GetSectionContainingPosition( selStart ) ) )
						{
							SectionBase tmp = this.GetSectionContainingPosition( selStart );
							this.CaretPosition = Math.Min( 1 + selEnd, 1 + tmp.LastDisplayChar.OverallIndexInEdit );
						}
						else
						{
							this.CaretPosition = selStart;
						}
						// ------------------------------------------------------------------------------------

						this.SetPivot( );
					}

					return couldDelete;
				}

			}

			return false;
		}

		#endregion //DeleteWithoutShiftingAcrrossSectionBoundaries
		
		private bool InsertCharAtShiftGlobalHelper( char c, int from, int to )
		{
			DisplayCharBase dc = null;

			if ( from == to )
			{
				dc = this.GetDisplayCharAtPosition( from );

				if ( null != dc && dc.IsEmpty && dc.MatchChar( c ) )
				{
					dc.Char = c;
				}
				else
					return false;
			}

			// SSP 4/8/02
			// Added this flag
			bool emptyDcFound = false;

			// Now find the last empty char in from-to range.
			//
			for ( int i = to; i >= from; i-- )
			{
				DisplayCharBase dc1 = this.GetDisplayCharAtPosition( i );

				if ( dc1.IsEmpty )
				{
					// SSP 4/8/02
					//
					emptyDcFound = true;
					
					to = i;
					break;
				}
			}

			// SSP 4/8/02
			//
			//if ( from == to )
			if ( !emptyDcFound || from == to )
			{
				dc = this.GetDisplayCharAtPosition( from );

				if ( null != dc && dc.IsEmpty && dc.MatchChar( c ) )
				{
					dc.Char = c;
				}
				else
					return false;
			}			

			
			int delta = 1;

			// At this point display char at 'to' is empty
			//
			for ( int i = to - 1; i >= from; i-- )
			{
				DisplayCharBase dc1 = this.GetDisplayCharAtPosition( i );
				DisplayCharBase dc2 = this.GetDisplayCharAtPosition( i + delta );

				// Take care of any literals popping up here and there.
				//
				if ( null != dc1 && !dc1.IsEditable )
				{
					delta++;
					continue;
				}

				if ( null != dc2 && !dc2.IsEditable )
				{
					delta--;
					if ( delta > 0 )
						i++;
					else
						delta = 1;
					continue;
				}


				if ( !dc1.IsEmpty && !dc2.MatchChar( dc1.Char ) )
				{
					return this.InsertCharAtShiftGlobalHelper( c, from, i - 1 );
				}

				// Make sure c matches the display char where it's supposed to go.
				//
				if ( i == from && !dc1.MatchChar( c ) )
					return false;
			}

			// Reset the delta back to 1
			//
			delta = 1;

			for ( int i = to - 1; i >= from; i-- )
			{
				DisplayCharBase dc1 = this.GetDisplayCharAtPosition( i );
				DisplayCharBase dc2 = this.GetDisplayCharAtPosition( i + delta );

				// Take care of any literals popping up here and there.
				//
				if ( null != dc1 && !dc1.IsEditable )
				{
					delta++;
					continue;
				}

				if ( null != dc2 && !dc2.IsEditable )
				{
					delta--;
					if ( delta > 0 )
						i++;
					else
						delta = 1;
					continue;
				}


				if ( !dc1.IsEmpty && !dc2.MatchChar( dc1.Char ) )
				{
					Debug.Assert( false, "This should not have happened. There is a logical problem with this method." );
					return false;
				}

				dc2.Char = dc1.Char;

				if ( i == from )
					dc1.Char = c;
			}

			return true;
		}




#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		private bool InsertCharAtShiftGlobal( char c, int pos )
		{
			DisplayCharBase dc = this.GetDisplayCharAtPosition( pos );

			// If it's a number section, return false since we don't
			// shift accross number section boundaries.
			//
			if ( null == dc || this.IsRightToLeft( dc.Section ) )
				return false;

			// SSP 3/12/02
			//
			// Also if it's a fraction section, then return false as
			// we don't shift accross fraction part sections either 
			// because after all it is like a number section.
			//
			if ( null == dc || dc.Section is FractionPart )
				return false;


			int count = this.GetTotalNumberOfDisplayChars( );


			// can't have an invalid starting point for shifting the characters
			if ( pos < 0 || pos >= count )
				return false;

			// Find the last character after pos that does not cross a number
			// section
			//
			DisplayCharBase lastValidDC = null;
			for ( int i = count - 1; i >= pos; i-- )
			{
				dc = this.GetDisplayCharAtPosition( i );

				if ( this.IsRightToLeft( dc.Section ) )
				{
					lastValidDC = null;
					continue;
				}

				if ( null == lastValidDC )
					lastValidDC = dc;				
			}

			// If the pos is right before a number seciton, then return false
			//
			if ( null == lastValidDC )
				return false;
			
			int from = pos;
			int to = lastValidDC.OverallIndexInEdit;

			return this.InsertCharAtShiftGlobalHelper( c, from, to );
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal bool CanDelete( )
		{
			return this.CanDelete( false );
		}
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// SSP 4/8/05 BR00890 BR03077
		// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
		// the caret if delete key is pressed right before a literal. It also advances the caret
		// if it can not shift characters so it's more user friendly.
		//
		//internal bool CanDelete( )
		internal bool CanDelete( bool emulateDeleteKey )
		{
			if ( !IsAnyTextSelected )
			{
				// nothing is selected and caret is after the last char, so nothing to delete
				// and thus we should return false;
				if ( this.IsAfterLastCharacter )
					return false;
				
				DisplayCharBase dc = this.GetCurrentDisplayChar();

				Debug.Assert( null != dc, 
					"if caret is not after the last char, there must be a current display char and a current section" );
				if ( null == dc )
					return false;

				EditSectionBase editSection = dc.Section as EditSectionBase;

				// in a literal section, delete can't work
				// SSP 4/8/05 BR00890 BR03077
				// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
				// the caret if delete key is pressed right before a literal. It also advances the caret
				// if it can not shift characters so it's more user friendly.
				//
				//if ( null == editSection )
				if ( null == editSection && ! emulateDeleteKey )
					return false;
			
				// regular text section
				// since we are implementing deletion strategy in which we are going to
				// delete and shift as much as we can, CanDelete should just return true
				// when at a non-literal char
				// NOTE: in case this is changed, look at CanDeleteWithoutShiftingAcrrossSectionBoundaries()
				// function
				return true;
			
			}
			else
			{
				// since we are implementing deletion strategy in which we are going to
				// delete and shift as much as we can, CanDelete should just return true
				// NOTE: in case this is changed, look at CanDeleteWithoutShiftingAcrrossSectionBoundaries()
				// function

				return true;
			}
		}
		

		#region code commented out
		
#region Infragistics Source Cleanup (Region)
































































































#endregion // Infragistics Source Cleanup (Region)

		#endregion



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private int ShiftGlobalLeft( int from, int positionsToShift )
		{
			int count = this.GetTotalNumberOfDisplayChars( );

			// can't have an invalid starting point for shifting the characters
			if ( from < 0 || from >= count )
				return 0;

			// can't have negative number for number of characters to shift
			if ( positionsToShift < 0 )
				return 0;


			// can't shift more characters from a position than we have
			if ( from + positionsToShift > count )
				return 0;

			int ret = 0;
			int tmp;

			int positionsToShiftOrig = positionsToShift;

			// shifting left
			// SSP 9/28/11 TFS89588
			// 
			//for ( int i = from; i < count; i++ )
			for ( int i = from; i < count && 0 != positionsToShift; i++ )
			{
				DisplayCharBase dc1 = this.GetDisplayCharAtPosition( i );
				DisplayCharBase dc2 = this.GetDisplayCharAtPosition( i + positionsToShift );

				EditSectionBase editSection = dc1.Section as EditSectionBase;

				if ( null != dc1 && null != editSection &&
					EditOrientation.RightToLeft == editSection.Orientation )
				{
					// SSP 4/19/02
					//
					//i = 1 + editSection.LastDisplayChar.OverallIndexInEdit;
					// ----------------------------------------------------------
					tmp = 1 + editSection.LastDisplayChar.OverallIndexInEdit;

					// Remember we can't shift accross number seciton boundaries. And
					// this method is called when selected text is deleted from a number
					// section. We don't want to continue shifting characters in next section
					// if the selected text was entiredly in the number section. In which
					// case EraseSelectedText would have taken care of any necessary shifting.
					// So just break.
					//
					if ( positionsToShiftOrig <= ( tmp - i ) )
						break;
					// ----------------------------------------------------------


					// SSP 5/10/02
					// Don't allow crossing number section boundaries.
					//
					if ( from + positionsToShiftOrig < tmp )
						break;

					// SSP 9/28/11 TFS89588
					// Any number of characters that we skip from the number section we have
					// to adjust the positionsToShift accordingly.
					// 
					positionsToShift--;

					continue;
				}

				// in case we encounter a literal, we have to get a non-literal
				// character before it

				if ( null != dc1 && !dc1.IsEditable )
				{
					positionsToShift--;
					continue;
				}

				tmp = 0;
				while ( null != dc2 && !dc2.IsEditable )
				{
					tmp++;
					dc2 = this.GetDisplayCharAtPosition( i + positionsToShift + tmp );						
				}

				positionsToShift += tmp;
			
				char c = (char)0;

				if ( null != dc2 )
					c = dc2.Char;

				if ( null != dc1 && ( c == (int)0 || dc1.MatchChar( c ) ) )
				{
					dc1.Char = c;

					// SSP 12/2/04 BR00890
					// Erase the display character from which we copied the value.
					//
					if ( null != dc2 )
						dc2.EraseChar( );						

					// SSP 12/2/04 BR00890
					// Only bump ret if the shifted character was non-empty.
					//
					if ( ! dc1.IsEmpty )
						ret++; // increase the number of characters shifted counter				
				}
				else
				{
					// can't shift any further
					break;
				}					
			}


			return ret;
		}








		internal void SetPivot( )
		{
			this.PivotPosition = this.CaretPosition;
		}

		// SSP 12/2/04 BR00890
		// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
		// the caret if delete key is pressed right before a literal. It also advances the caret
		// if it can not shift characters so it's more user friendly.
		//


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal bool Delete( )
		{
            
            
            
			
            return this.Delete( false, false );
		}
	


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 12/2/04 BR00890
		// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
		// the caret if delete key is pressed right before a literal. It also advances the caret
		// if it can not shift characters so it's more user friendly.
		//
		//internal bool Delete( )
        // SSP 5/13/10 TFS 31103
        // Added fireTextChanged parameter.
        // 
		//internal bool Delete( bool emulateDeleteKey )
        internal bool Delete( bool emulateDeleteKey, bool fireTextChanged )
		{
            string oldText = null;
            if ( fireTextChanged )
                oldText = this.GetText( InputMaskMode.IncludeBoth );
            
            bool ret = this.DeleteHelper( emulateDeleteKey );

            if ( fireTextChanged )
            {
                string newText = this.GetText( InputMaskMode.IncludeBoth );
                if ( oldText != newText )
                    this.OnTextChanged( true );
            }

            return ret;
        }

        // SSP 5/13/10 TFS 31103
        // Refactored Delete code into the new DeleteHelper method.
        // 
        private bool DeleteHelper( bool emulateDeleteKey )
        {
			// SSP 6/9/03 UWG2311
			// Applied the same fix to Delete and Cut methods as the one that was applied to Paste method.
			//
			// ------------------------------------------------------------------------------------------------
			if ( this.ReadOnly )
				return false;
			// ------------------------------------------------------------------------------------------------

			// SSP 11/18/04 BR00499
			// Added AllowShiftingAcrossSections property.
			//
			// ------------------------------------------------------------------------------
			if ( ! this.MaskedInput.AllowShiftingAcrossSections )
				return this.DeleteWithoutShiftingAcrrossSectionBoundaries( );
			// ------------------------------------------------------------------------------

			// if no text is selected, delete the char at current input position
			if ( !this.IsAnyTextSelected )
			{
				// SSP 4/8/05 BR00890 BR03077
				// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
				// the caret if delete key is pressed right before a literal. It also advances the caret
				// if it can not shift characters so it's more user friendly.
				//
				//if ( this.CanDelete( ) )
				if ( this.CanDelete( emulateDeleteKey ) )
				{
					DisplayCharBase dc = this.GetCurrentDisplayChar( );

					Debug.Assert( null != dc, 
						"there must be a current display char if we are in a position to delete a char when there is no text selected" );
					if ( null == dc )
						return false;

					// if display char at current position is a literal then, retrun false
					if ( !dc.IsEditable )
					{
						// SSP 12/2/04 BR00890
						// Advance the caret to the next filled character so the user can keep the 
						// delete key pressed and delete the whole content of the masked edit even 
						// if some characters occuring after the current character can not be deleted. 
						// If this change causes a problem then it can be safely removed.
						//
						if ( emulateDeleteKey && null != dc.NextFilledEditableDisplayChar )
							this.SetCaretPivot( dc.NextFilledEditableDisplayChar.OverallIndexInEdit );
						// SSP 4/8/05 BR00890 BR03077
						// Enclosed the statement that returns false in the else block.
						//
						else
							return false;
					}
					else
					{
						// display char is non-literal, and thus it must be in an EditSectionBase instance
						EditSectionBase editSection = dc.Section  as EditSectionBase;
						Debug.Assert( null != editSection, "edit section assoiciated with a non-literal character is not an instance of EditSectionBase" );
						if ( null == editSection )
							return false;

						if ( this.IsRightToLeft( editSection )  )
						{
							if ( editSection.DeleteCharAndShift( dc.Index ) )
							{
								this.SetCaretPivot( 1 + dc.OverallIndexInEdit );
								return true;
							}
							else
							{
								return false;
							}
						}
						else
						{
							// SSP 12/2/04 BR00890
							//
							bool wasEmpty = dc.IsEmpty;

							// just a regular edit section
							int shiftedChars = this.ShiftGlobalLeft( dc.OverallIndexInEdit, 1 );

							// SSP 12/2/04 BR00890
							// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
							// the caret if delete key is pressed right before a literal. It also advances the caret
							// if it can not shift characters so it's more user friendly. Added the following block
							// of code.
							//
							// --------------------------------------------------------------------------------------
							if ( 0 == shiftedChars )
							{
								if ( ! dc.IsEmpty )
									dc.EraseChar( );
									// Advance the caret to the next filled character so the user can keep the 
									// delete key pressed and delete the whole content of the masked edit even 
									// if some characters occuring after the current character can not be shifted 
									// left. If this change causes a problem then it can be safely removed.
									//
								else if ( emulateDeleteKey && wasEmpty && null != dc.NextFilledEditableDisplayChar )
									this.SetCaretPivot( dc.NextFilledEditableDisplayChar.OverallIndexInEdit );
							}
							// --------------------------------------------------------------------------------------
						}
					}
				}
				this.SetPivot( );
				return true;
			}
			else
			{
				// SSP 4/8/05 BR00890 BR03077
				// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
				// the caret if delete key is pressed right before a literal. It also advances the caret
				// if it can not shift characters so it's more user friendly.
				//
				//if ( this.CanDelete( ) )
				if ( this.CanDelete( emulateDeleteKey ) )
				{				
					this.EraseSelectedText( );

					int selStart = this.SelectionStart;  // index of the first char in selection
					int selLen   = this.SelectionLength;
					int selEnd	 = selStart + selLen - 1; // index of the last char in the selection

					Debug.Assert( selLen > 0, "selection length must be greater that 0 if something is selected" );
					Debug.Assert( selStart >= 0, "sel start must not be a negative number" );
					Debug.Assert( selStart < this.GetTotalNumberOfDisplayChars( ), 
						"selection start can not be beyond total number of display chars when something is selected" );

					this.ShiftGlobalLeft( selStart, selLen );
					
					bool flag = false;
					DisplayCharBase dc = this.GetDisplayCharAtPosition( selStart );
					EditSectionBase editSection = dc.Section as EditSectionBase;
					// SSP 8/6/07 BR24752 BR24720
					// In overwrite mode, position the caret at the selection start regardless of whether we are
					// in a number section or not.
					// 
					//if ( null != dc && null != editSection && this.IsRightToLeft( editSection ) )
					if ( this.InsertMode && null != dc && null != editSection && this.IsRightToLeft( editSection ) )
					{
						dc = this.GetDisplayCharAtPosition( System.Math.Min( selEnd, editSection.LastDisplayChar.OverallIndexInEdit ) );
						if ( null != dc )
							editSection = dc.Section as EditSectionBase;

						if ( null != dc )
						{
							if ( null != editSection && null !=  editSection.FirstFilledChar && selEnd < editSection.FirstFilledChar.OverallIndexInEdit - 1 ) 
							{
								this.SetCaretPivot( editSection.FirstFilledChar.OverallIndexInEdit - 1 );
							}
							else if ( null != dc )
							{
								int index = dc.OverallIndexInEdit;
								if ( this.IsRightToLeft( dc.Section ) )
								{
									EditSectionBase editSection3 = dc.Section as EditSectionBase;									

									if ( null != editSection3 )
									{ 
										index = 1 + System.Math.Min( selEnd, editSection3.LastDisplayChar.OverallIndexInEdit );

										int tmp = editSection3.LastDisplayChar.OverallIndexInEdit + 1;
										if ( null != editSection3.FirstFilledChar )
											tmp = editSection3.FirstFilledChar.OverallIndexInEdit;
										
										if ( index < tmp )
											index = tmp;
									}
								}

								this.SetCaretPivot( index );
							}
							flag = true;
						}
					}

					if ( !flag )
						this.SetCaretPivot( selStart );

					return true;
				}

			}

			return false;
		}


	






		private void GotoSection( SectionBase section )
		{
			this.GotoSection( section, false );
		}

		// SSP 12/2/04 BR00890
		// Added an overload of GotoSection that takes in the new
		// positionRightAfterLastCharacter parameter. NOTE: We are defaulting the new
		// parameter to true and changing the behavior of the function.
		//


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		private void GotoSection( SectionBase section, bool positionToRight )
		{
			this.GotoSection( section, positionToRight, true );
		}
		


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		// SSP 12/2/04 BR00890
		// Added an overload of GotoSection that takes in the new
		// positionRightAfterLastCharacter parameter.
		//
		//private void GotoSection( SectionBase section, bool positionToRight )
		private void GotoSection( SectionBase section, bool positionToRight, bool positionRightAfterLastCharacter )
		{
			Debug.Assert( null != section, "You can't go into a null section" );
			if ( null == section )
				return;

			// crossing section boundaries
			if ( section != this.CurrentSection&&
				!this.ValidateChangeSection( this.CurrentSection, section ) )
			{
				// if cancelled
				return;
			}

			EditSectionBase editSection = section as EditSectionBase;

			// if we have an edit section with right-to-left ( number section )			
			if ( this.IsRightToLeft( editSection ) )
			{
				// in case of a number section
				if ( positionToRight )
				{
					this.CaretPosition = 1 + editSection.LastDisplayChar.OverallIndexInEdit;
				}
				else
				{					
					this.CaretPosition =
						null != editSection.FirstFilledChar ?
						editSection.FirstFilledChar.OverallIndexInEdit :
						1 + editSection.LastDisplayChar.OverallIndexInEdit;
				}				
			}
			else
			{
				// in case of a regular section
				if ( positionToRight )
				{
					// SSP 12/2/04 BR00890
					// Added an overload of GotoSection that takes in the new
					// positionRightAfterLastCharacter parameter. NOTE: We are defaulting the new
					// parameter to true and changing the behavior of the function.
					//
					//this.CaretPosition = section.LastDisplayChar.OverallIndexInEdit;
					this.CaretPosition = section.LastDisplayChar.OverallIndexInEdit
						+ ( positionRightAfterLastCharacter ? 1 : 0 );
				}
				else
				{					
					this.CaretPosition = section.FirstDisplayChar.OverallIndexInEdit;
				}
				
			}
		}

	
		





		internal void GotoNextChar( )
		{
			if ( this.IsAfterLastCharacter )
			{
				string msg = XamMaskedInput.GetString("IllegalOperationMessageAtLastCharacter");
				this.IllegalOperation( msg );
			}
			else
			{				
				DisplayCharBase dc = this.GetCurrentDisplayChar( );
				DisplayCharBase dc2 = this.GetDisplayCharAtPosition( this.CaretPosition + 1 );				

				if ( null != dc && null != dc2 && dc.Section != dc2.Section )
				{
					EditSectionBase editSection = dc.Section as EditSectionBase;

					// crossing section boundaries
					this.GotoSection( dc2.Section, false );
				}
				else
				{
					// we are not crossing any section-boundaries, so just move the caret by 1

					if ( !this.InsertMode )
					{
						// if in overwrite mode, then make sure the caret position does not
						// go beyond the last char
						// SSP 3/23/11 TFS37056
						// Commented out the below condition. Allow the caret to go beyond the last character
						// even when in overwrite mode. We are already doing this as part of input processing.
						// When a character is typed in while on the last display char, we do move the caret
						// to after the last character. So no harm in doing that when using arrow keys. 
						// Furthermore, this is to allow selection of the last character using keyboard.
						// 
						//if ( this.CaretPosition + 1 < this.GetTotalNumberOfDisplayChars( ) )
						//{
							this.CaretPosition++;
						//}
					}
					else
					{
						// SSP 6/14/12 TFS108867
						// 
						if ( 0 == _maskInfo.PromptChar )
							this.CaretPosition = this.SkipDisplayCharsWithEmptyDrawString( this.CaretPosition );

						this.CaretPosition++;
					}
				}
			}			
		}

		// SSP 6/14/12 TFS108867
		// 
		/// <summary>
		/// This method skips display chars whose GetDrawChar return 0, which can happen when prompt char is 0. 
		/// Returns the caretPos of the next display char whose GetDrawChar( ) is not 0.
		/// </summary>
		/// <param name="caretPos"></param>
		/// <returns></returns>
		internal int SkipDisplayCharsWithEmptyDrawString( int caretPos )
		{
			while ( true )
			{
				DisplayCharBase ii = this.GetDisplayCharAtPosition( caretPos );
				if ( null != ii && ii.IsEmpty && 0 == ii.GetDrawChar( ) )
					caretPos++;
				else
					break;
			}

			return caretPos;
		}






		internal void GotoPrevEditSection( )
		{
			SectionBase section =
				null != this.CurrentSection ? this.CurrentSection : this.LastSection;

			// SSP 7/11/02 UWM93
			// Use the caret position instead of the selection start because if the
			// text is selected, caret position is the one that you want to user and
			// not the selection start which would be different than the position of
			// the caret.
			//
			//DisplayCharBase dc = this.GetDisplayCharAtPosition( this.SelectionStart - 1 );
			DisplayCharBase dc = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );
			

			if ( null != dc )
			{
				EditSectionBase prevSection = dc.Section as EditSectionBase;

				if ( null != prevSection )
				{
					if ( this.IsRightToLeft( prevSection ) )
					{
						if ( dc.IsEmpty )
						{
							if ( null != prevSection.PreviousEditSection )
							{
								// SSP 12/2/04 BR00890
								// We should be positioning the caret after the last character of the 
								// previous section.
								//
								//this.GotoSection( prevSection.PrevEditSection, false );
								this.GotoSection( prevSection.PreviousEditSection, true );

								return;
							}
						}
						else
						{
							this.GotoSection( prevSection, false );

							return;
						}
					}
					else
					{
						this.GotoSection( prevSection, false );

						return;
					}
				}
			}


			if ( null != section.PreviousEditSection )
			{
				this.GotoSection( section.PreviousEditSection );
			}
			else
			{

				string msg = XamMaskedInput.GetString("IllegalOperationMessageNoPrevEditSection");
				this.IllegalOperation( msg );
			}
		}







		internal void GotoNextEditSection( )
		{
			SectionBase section = this.CurrentSection;

			if ( null == section )
			{
				string msg = XamMaskedInput.GetString("IllegalOperationMessageNoNextEditSection");
				this.IllegalOperation( msg );
				return;
			}

			if ( null != section.NextEditSection )
			{
				this.GotoSection( section.NextEditSection, false );
			}
			else
			{
				string msg = XamMaskedInput.GetString("IllegalOperationMessageNoNextEditSection");
				this.IllegalOperation( msg );
			}
		}

		private Stack<EditorSnapshot> UndoHistory
		{
			get
			{
				if ( null == _undoHistory )
					_undoHistory = new Stack<EditorSnapshot>( );

				return _undoHistory;
			}
		}

		private Stack<EditorSnapshot> RedoHistory
		{
			get
			{
				if ( null == _redoHistory )
					_redoHistory = new Stack<EditorSnapshot>( );

				return _redoHistory;
			}
		}

		private bool HasUndoHistory
		{
			get
			{
				return null != _undoHistory && _undoHistory.Count >= 2;
			}
		}

		private bool HasRedoHistory
		{
			get
			{
				return null != _redoHistory && _redoHistory.Count > 0;
			}
		}

		internal void ManageUndoHistory_TextChanged( bool viaUserAction, MaskedInputCommandId? command )
		{
			if ( !_inUndoRedoHelper && ( ! command.HasValue || MaskedInputCommandId.Undo != command.Value && MaskedInputCommandId.Redo != command.Value ) )
			{
				// When a change is made the redo history becomes invalid.
				// 
				this.ClearRedoHistory( );

				// If the editor's value is being set externally (for example via the Value property)
				// then clear the undo history as well.
				// 
				if ( ! viaUserAction )
					this.ClearUndoHistory( );

				this.UndoHistory.Push( new EditorSnapshot( this ) );
			}
		}

		internal void Undo( bool fireValueChanged )
		{
			this.UndoRedoHelper( true, fireValueChanged );
		}

		internal void Redo( bool fireValueChanged )
		{
			this.UndoRedoHelper( false, fireValueChanged );
		}

		internal void UndoRedoHelper( bool undo, bool fireValueChanged )
		{
			if ( _inUndoRedoHelper )
				return;

			_inUndoRedoHelper = true;
			try
			{
				string oldText = this.GetText( InputMaskMode.IncludeBoth );

				try
				{
					Stack<EditorSnapshot> ss = undo ? _undoHistory : _redoHistory;

					if ( null != ss && ss.Count >= 2 )
					{
						EditorSnapshot snapshot = ss.Pop( );

						if ( undo )
							this.RedoHistory.Push( snapshot );
						else
							this.UndoHistory.Push( snapshot );

						snapshot = ss.Peek( );
						snapshot.Apply( this );
					}
				}
				finally
				{
					if ( fireValueChanged )
					{
						string newText = this.GetText( InputMaskMode.IncludeBoth );
						if ( 0 != Utils.CompareStrings( oldText, newText, false ) )
							this.OnTextChanged( true );
					}
				}
			}
			finally
			{
				_inUndoRedoHelper = false;
			}
		}

		internal void ClearRedoHistory( )
		{
			_redoHistory = null;
		}

		internal void ClearUndoHistory( )
		{
			_undoHistory = null;
		}






		internal void GotoPrevChar( )
		{
			DisplayCharBase dc = null;
			DisplayCharBase prevDc = null;

			if ( this.IsAfterLastCharacter )
			{
				EditSectionBase editSection = this.LastSection as EditSectionBase;

				if ( this.IsRightToLeft( editSection ) )
				{
					// in a number section, only goto previous char if the char
					// is not empty

					if ( !editSection.LastDisplayChar.IsEmpty )
					{
						// if caret is AFTER the last char, then position it BEFORE the last char
						this.CaretPosition = this.GetTotalNumberOfDisplayChars( ) - 1;
					}
					// SSP 8/6/07 BR24752 BR24720
					// Added the following else-if block. In over-write mode, go to the first
					// display character when left is pressed and we are on the first number
					// section which is empty.
					// 
					else if ( !this.InsertMode && null == editSection.PreviousEditSection )
					{
						this.SetCaretPivot( 0 );
					}
					else
					{
						this.GotoPrevEditSection( );
					}
				}
				else
				{
					// if caret is AFTER the last char, then position it BEFORE the last char
					this.CaretPosition = this.GetTotalNumberOfDisplayChars( ) - 1;
				}
			}
			else if ( this.IsAtFirstChar )
			{
				// AS 2/19/02
				// Added localization support.
				//
				string msg = XamMaskedInput.GetString("IllegalOperationMessageNoPrevCharacter");
				this.IllegalOperation( msg ); //"already at the first char, can't go to prev char" );
			}
			else
			{
				dc = this.GetCurrentDisplayChar( );
				prevDc = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );

				Debug.Assert( null != dc && null != prevDc, "GetDisplayCharAtPosition retruned null" );

				if ( null != dc && null != prevDc &&
					dc.Section == prevDc.Section )
				{

					EditSectionBase editSection = dc.Section as EditSectionBase;

					if ( this.IsRightToLeft( editSection ) )
					{
						// number section
						if ( prevDc.IsEmpty )
						{
							// if number section, and previous char is empty, then move to previous section

							if ( null != editSection.PreviousEditSection )
								this.GotoSection( editSection.PreviousEditSection, true );
							// SSP 8/6/07 BR24752 BR24720
							// Added the following else-if block. In over-write mode, go to the first
							// display character when left is pressed and we are on the first number
							// section which is empty.
							// 
							else if ( !this.InsertMode )
								this.SetCaretPivot( 0 );

							return;
						}
						else
						{
							// just move the caret to prev display char position
							this.CaretPosition--;						
						}
					}
					else
					{
					
						// in a non-number section
						// just move the caret to prev display char position
						this.CaretPosition--;						
						
					}
				}
				else if ( null != dc && null != prevDc )
				{
					// crossing section boundaries

					EditSectionBase editSection = prevDc.Section as EditSectionBase;

					if ( null != editSection && this.IsRightToLeft( editSection ) )
					{
						if ( !editSection.LastDisplayChar.IsEmpty )
						{
							if ( dc.Section != editSection )
								this.ValidateChangeSection( dc.Section, editSection );

							// if caret is at a literal right after a number section, and the number section is
							// not empty, then move the caret to there
							this.CaretPosition = editSection.LastDisplayChar.OverallIndexInEdit;
						}
						// SSP 8/6/07 BR24752 BR24720
						// Added the following else-if block. In over-write mode, go to the first
						// display character when left is pressed and we are on the first number
						// section which is empty.
						// 
						else if ( !this.InsertMode && null == editSection.PreviousEditSection )
						{
							this.SetCaretPivot( 0 );
						}
						else
						{
							this.GotoPrevEditSection( );
						}
					}
					else
					{
						if ( null != prevDc.Section )
							// SSP 12/2/04 BR00890
							// Added an overload of GotoSection that takes in the new
							// positionRightAfterLastCharacter parameter.
							//
							//this.GotoSection( prevDc.Section, true );
							this.GotoSection( prevDc.Section, true, false );
						else
						{
							// AS 2/19/02
							// Added localization support.
							//
							string msg = XamMaskedInput.GetString("IllegalOperationMessageCannotMoveToPrevCharacter");
							this.IllegalOperation( msg ); //"can't go to prev char" );
						}
					}
				}
			}
		}






		// SSP 10/5/09 - NAS10.1 Spin Buttons
		// Pass along the command parameter to the method.
		// 
		//internal bool ExecuteCommandImpl( RoutedCommand command, bool shift, bool control )
		internal bool ExecuteCommandImpl( MaskedInputCommandId command, object commandParameter, bool shift, bool control, FrameworkElement sourceElement )
		{
			bool dirtyElements = false;
			bool repaint = false;

			// SSP 1/8/02 UWM59
			// When the caret is moved out of a section, we should
			// call ValidateSection so that that section will have 
			// a chance to change the contents (like for example a
			// fraction section padding it's contents with 0's).
			// 
			SectionBase oldCurrentSection = this.GetCurrentSection( true );

			// SSP 1/17/02
			// Check for the DesignTime value
			//
			// see if action is allowed
			//if ( this.IsInitialized )
			if ( !this.IsInitialized || this.MaskInfo.DesignMode )
				return false;

			DisplayCharBase dc = null;
			DisplayCharBase prevDc = null;
			EditSectionBase editSection = null;

			// SSP 10/2/01
			// After such operations as backspance or delete
			// or up arrow keys, we want to see if the text
			// was changed, and if so then fire TextChanged event
			//
			string oldText = this.GetText( InputMaskMode.IncludeBoth );
			XamMaskedInput maskedInput = this.MaskedInput;

			switch ( 1 )
			{
				case 1:

					if ( MaskedInputCommandId.SetPivot == command )
					{
						this.SetPivot( );
						repaint = true;
					}
					else if ( MaskedInputCommandId.Backspace == command )
                    {
                        #region Backspace

						// SSP 12/4/01 UWG809
						// Added ReadOnly property to the control. If set
						// do not allow the user to modify the text.
						//
						if ( this.ReadOnly )
							break;

						dc = this.GetCurrentDisplayChar( );
						prevDc = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );

						if ( this.IsAnyTextSelected )
						{
							// if some text is selected, backspace works just like delete
							if ( !this.Delete( ) )
							{
								// AS 2/19/02
								// Added localization support.
								//
								string msg = XamMaskedInput.GetString("IllegalOperationMessageCannotDelete");
								this.IllegalOperation( msg );
							}
						}
						else if ( this.IsAfterLastCharacter )
						{
							editSection = this.LastSection as EditSectionBase;
							// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
							// 
							//if ( this.IsRightToLeft( editSection ) )
							if ( this.IsRightToLeft( editSection ) && this.InsertMode )
							{
								// number section

								dc = editSection.LastDisplayChar;

								if ( !dc.IsEmpty )
								{
									// SSP 4/8/05 BR03077
									//
									//numberSection = dc.Section as NumberSection;
									//if ( null != numberSection && !numberSection.DeleteCharAndShift( dc.Index ) )
									if ( null != editSection && !editSection.DeleteCharAndShift( dc.Index ) )
									{
										// AS 2/19/02
										// Added localization support.
										//
										string msg = XamMaskedInput.GetString("IllegalOperationMessageGeneral");
										this.IllegalOperation( msg ); //"illegal operation" );
									}
								}
								else
								{
									// the whole number section is empty, so move to prevois section
									if ( null != editSection.PreviousEditSection )
									{
										this.GotoSection( editSection.PreviousEditSection, true );
										this.SetPivot( );
									}
									else
									{
										// AS 2/19/02
										// Added localization support.
										//
										string msg = XamMaskedInput.GetString("IllegalOperationMessageNoPrevEditableCharacter");
										this.IllegalOperation( msg ); //"no editable char to backspace to" );
									}
								}

							}
							else
							{
								// non number seciton

								dc = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );
								Debug.Assert( null != dc, "GetCurrentDisplayChar returned null at a valid input position" );

								editSection = dc.Section as EditSectionBase;

								if ( null != editSection )
								{
									// we are in an edit section

									// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
									// 
									// --------------------------------------------------------------------
									//editSection.DeleteCharAndShift( dc.Index );
									//this.CaretPosition--;
									//this.SetPivot( );
									if ( this.InsertMode )
									{
										editSection.DeleteCharAndShift( dc.Index );
										this.CaretPosition--;
										this.SetPivot( );
									}
									else
									{
										this.CaretPosition--;
										if ( this.CanDelete( ) )
											this.Delete( );
										else
											this.SetPivot( );
									}
									// --------------------------------------------------------------------
								}
								else
								{
									// in case we have a literal section
									this.CaretPosition--;
									this.SetPivot( );
								}
							}
						}
						else if ( this.IsAtFirstChar )
						{
							// AS 2/19/02
							// Added localization support.
							//
							string msg = XamMaskedInput.GetString("IllegalOperationMessageNoPrevCharacter");
							this.IllegalOperation( msg ); //"can't backspace, already at first char" );
						}
						else if ( null != dc )
						{
							editSection = dc.Section as EditSectionBase;

							// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
							// 
							//if ( this.IsRightToLeft( editSection ) )
							if ( this.IsRightToLeft( editSection ) && this.InsertMode )
							{
								// number section	

								prevDc = this.GetDisplayCharAtPosition( dc.OverallIndexInEdit - 1 );

								Debug.Assert( null != prevDc );

								if ( prevDc.Section != editSection )
								{
									if ( prevDc.IsEditable )
									{
										int oldCaret = this.CaretPosition;
										this.CaretPosition--;
										if ( this.CanDelete( ) )
											this.Delete( );
										else
											this.CaretPosition = oldCaret;

										this.SetPivot( );
									}
									else
									{
										this.SetCaretPivot( prevDc.OverallIndexInEdit );
									}
								}
								else
								{

									if ( !prevDc.IsEmpty )
									{
										if ( !editSection.DeleteCharAndShift( prevDc.Index ) )
										{
											// AS 2/19/02
											// Added localization support.
											//
											string msg = XamMaskedInput.GetString("IllegalOperationMessageGeneral");
											this.IllegalOperation( msg ); // "illegal operation" );
										}
									}
									else
									{
										// the prev char in the number section is empty, so move to previous section
										if ( null != editSection.PreviousEditSection )
										{
											this.GotoSection( editSection.PreviousEditSection, true );
											this.SetPivot( );
										}
										// SSP 8/6/07 BR24752 BR24720
										// Added the following else-if block. In over-write mode when the last of 
										// the characters is deleted in the first numberic section, move the caret 
										// to the first display character. This is so the characters can be input
										// into the first edit section. Otherwise it will be impossible to enter
										// characters there if the input section is not numeric.
										// 
										else if ( !this.InsertMode )
										{
											this.SetCaretPivot( 0 );
										}
										else
										{
											// AS 2/19/02
											// Added localization support.
											//
											string msg = XamMaskedInput.GetString("IllegalOperationMessageNoPrevEditableCharacter");
											this.IllegalOperation( msg ); //"no editable char to backspace to" );
										}
									}
								}
							}
							else
							{
								if ( this.IsAtFirstChar )
								{
									// AS 2/19/02
									// Added localization support.
									//
									string msg = XamMaskedInput.GetString("IllegalOperationMessageNoPrevCharacter");
									this.IllegalOperation( msg ); //"can't backspace, already at first char" );

								}
								else
								{

									dc = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );
									Debug.Assert( null != dc, "GetCurrentDisplayChar returned null at a valid input position" );

									editSection = dc.Section as EditSectionBase;

									if ( null != editSection )
									{
										// we are in an edit section

										// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
										// 
										//if ( this.IsRightToLeft( editSection ) )
										if ( this.IsRightToLeft( editSection ) && this.InsertMode )
										{
											// number section

											if ( !dc.IsEmpty )
											{
												editSection = dc.Section as EditSectionBase;

												if ( !editSection.DeleteCharAndShift( dc.Index ) )
												{
													// AS 2/19/02
													// Added localization support.
													//
													string msg = XamMaskedInput.GetString("IllegalOperationMessageGeneral");
													this.IllegalOperation( msg ); //"illegal operation" );
												}
											}
											else
											{
												// the whole number section is empty, so move to prevois section
												if ( null != editSection.PreviousEditSection )
												{
													this.GotoSection( editSection.PreviousEditSection, true );
													this.SetPivot( );
												}
												// SSP 8/6/07 BR24752 BR24720
												// Added the following else-if block. In over-write mode when the last of 
												// the characters is deleted in the first numberic section, move the caret 
												// to the first display character. This is so the characters can be input
												// into the first edit section. Otherwise it will be impossible to enter
												// characters there if the input section is not numeric.
												// 
												else if ( !this.InsertMode )
												{
													this.SetCaretPivot( 0 );
												}
												else
												{
													// AS 2/19/02
													// Added localization support.
													//
													string msg = XamMaskedInput.GetString("IllegalOperationMessageNoPrevEditableCharacter");
													this.IllegalOperation( msg ); //"no editable char to backspace to" );
												}
											}
										}
										else
										{
											// not a number section
											int oldCaret = this.CaretPosition;
											this.CaretPosition--;
											if ( this.CanDelete( ) )
												this.Delete( );
											else
												this.CaretPosition = oldCaret;

											this.SetPivot( );
										}
									}
									else
									{
										// in case we have a literal section
										this.CaretPosition--;
										this.SetPivot( );
									}
								}
							}
						}
						else
						{
							// we should never get here
						}

						// SSP 4/8/05 BR03077
						//
						if ( null != this.CurrentSection && this.CurrentSection.NextEditSection is FractionPartContinuous )
							this.EnsureNumberSectionState( );

						dirtyElements = true;
						break;
                        #endregion //Backspace
                    }
					else if ( MaskedInputCommandId.Delete == command )
                    {
                        #region Delete

						// SSP 12/4/01 UWG809
						// Added ReadOnly property to the control. If set
						// do not allow the user to modify the text.
						//
						if ( this.ReadOnly )
							break;

						// SSP 12/2/04 BR00890
						// Added an overload of Delete with the new emulateDeleteKey parameter. This will advance
						// the caret if delete key is pressed right before a literal. It also advances the caret
						// if it can not shift characters so it's more user friendly.
						//
						//if ( !this.Delete( ) )
                        
                        
                        
						
                        if ( !this.Delete( true, false ) )
						{
							// AS 2/19/02
							// Added localization support.
							//
							string msg = XamMaskedInput.GetString( "IllegalOperationMessageGeneral" );
							this.IllegalOperation( msg ); //"illegal operation" );
						}

						dirtyElements = true;
						break;
                        #endregion //Delete
                    }
					else if ( MaskedInputCommandId.FirstCharacter == command )
                    {
                        #region FirstCharacter
                        SectionBase section = this.FirstSection;

						// SSP 10/23/01 UWM25
						// When selecting, home key will always position
						// the caret to the begginging even if that means that
						// the caret is going to end up in an empty char position 
						// in number section. Added currentlySelecting check in the
						// if statemenet.
						// 
						bool currentlySelecting =
							0 != ( Keyboard.Modifiers & ModifierKeys.Shift );

						// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
						// Added InsertMode check.
						// 
						//if ( !currentlySelecting && this.IsRightToLeft( section ) )
						if ( !currentlySelecting && this.IsRightToLeft( section ) && this.InsertMode )
						{
							// first section is a number section

							editSection = section as EditSectionBase;

							Debug.Assert( null != editSection );

							if ( null != editSection )
							{
								if ( null != editSection.FirstFilledChar )
								{
									this.CaretPosition = editSection.FirstFilledChar.OverallIndexInEdit;
								}
								else
								{
									this.CaretPosition = 1 + editSection.LastDisplayChar.OverallIndexInEdit;
								}
							}
						}
						else
						{
							this.CaretPosition = 0;
						}

						repaint = true;
						break;
                        #endregion //FirstCharacter
                    }
					else if ( MaskedInputCommandId.AfterLastCharacter == command )
					{
						this.CaretPosition = this.GetTotalNumberOfDisplayChars( );
						repaint = true;
						break;
					}
					else if ( MaskedInputCommandId.NextCharacter == command )
					{
						this.GotoNextChar( );
						repaint = true;
						break;
					}
					else if ( MaskedInputCommandId.PreviousCharacter == command )
					{
						this.GotoPrevChar( );

						// SSP 4/8/05 BR03077
						//
						if ( null != this.CurrentSection && this.CurrentSection.NextEditSection is FractionPartContinuous )
							this.EnsureNumberSectionState( );

						repaint = true;
						break;
					}
					else if ( MaskedInputCommandId.NextSection == command )
					{
						this.GotoNextEditSection( );
						repaint = true;
						break;
					}
					else if ( MaskedInputCommandId.PreviousSection == command )
					{
						this.GotoPrevEditSection( );

						// SSP 4/8/05 BR03077
						//
						if ( null != this.CurrentSection && this.CurrentSection.NextEditSection is FractionPartContinuous )
							this.EnsureNumberSectionState( );

						repaint = true;
						break;
					}
					else if ( MaskedInputCommandId.SpinUp == command
						|| MaskedInputCommandId.SpinDown == command )
					{
						// SSP 12/4/01 UWG809
						// Added ReadOnly property to the control. If set
						// do not allow the user to modify the text.
						//
						if ( this.ReadOnly )
							break;

						// This is to fix a pefromance issue with windows phone where spnning is slow
						// if there's something selected.
						// 


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


						// SSP 8/8/12 TFS118267
						// In WPF, the masked input doesn't get focused when a spin button is clicked upon.
						// This logic is to focus the masked input. The issue didn't occur in SL however we
						// are intentionally adding the logic for that too since it should cause any issues
						// and we try to minimize code differences.
						// 
						if ( null != sourceElement && !maskedInput.IsInEditMode
							&& null != Utils.GetAncestorFromName( sourceElement, "PART_SpinButtons" ) )
						{
							maskedInput.SetFocusToFocusSite( );
						}

						// SSP 3/22/02
						// Moved all the code into this IncrementDecrement method so
						// spin button can also use that method.
						//
						// SSP 10/5/09 - NAS10.1 Spin Buttons
						// Pass along the command parameter to the method.
						// 
						//this.Spin( MaskedInputCommands.SpinUp == command, false, false );
						this.Spin( MaskedInputCommandId.SpinUp == command, commandParameter, false, false );

						dirtyElements = true;
						break;
					}
					else if ( MaskedInputCommandId.ToggleInsertionMode == command )
					{
						this.ToggleInsertMode( );
						break;
					}
					else if ( MaskedInputCommandId.SelectSection == command )
					{
						editSection = this.CurrentSection as EditSectionBase;

						if ( null != editSection )
						{
							if ( null != editSection.FirstFilledChar )
							{
								// if the section has any chars input
								this.CaretPosition = editSection.FirstFilledChar.OverallIndexInEdit;
								this.PivotPosition = 1 + editSection.LastFilledChar.OverallIndexInEdit;

								repaint = true;
							}
							else
							{
								// section doesn't have any input it, thus can't select anything in it
							}
						}
						break;
					}
					else if ( MaskedInputCommandId.SelectAll == command )
					{
						this.SelectAll( );
						break;
					}
					else if ( MaskedInputCommandId.Copy == command )
					{
						this.Copy( );
						break;
					}
					else if ( MaskedInputCommandId.Cut == command )
					{
						// Pass in false for the fireValueChanged because we are firing ValueChanged below.
						// 
						this.Cut( false );
						dirtyElements = true;
						break;
					}
					else if ( MaskedInputCommandId.Paste == command )
					{
						// Pass in false for the fireValueChanged because we are firing ValueChanged below.
						// 
						this.Paste( false );
						dirtyElements = true;
						break;
					}
					else if ( MaskedInputCommandId.Undo == command )
					{
						// Pass in false for the fireValueChanged because we are firing ValueChanged below.
						// 
						this.Undo( false );
						break;
					}
					else if ( MaskedInputCommandId.Redo == command )
					{
						// Pass in false for the fireValueChanged because we are firing ValueChanged below.
						// 
						this.Redo( false );
						break;
					}
                    // AS 9/5/08 NA 2008 Vol 2
					else if ( MaskedInputCommandId.ToggleDropDown == command )
                    {
                        XamMaskedInput editor = this.MaskedInput;

                        if (null != editor)
                        {
                            editor.ToggleDropDown();
                        }
                        break;
                    }
					else if ( MaskedInputCommandId.NotACommand == command )
					{
						// Do nothing
					}
					else
					{
						Debug.Assert( false, "unknown action" );
						break;
					}
					
					break;
			}

			// SSP 1/8/02 UWM59
			// Read the comment in the beggining of this method referring to
			// UWM59
			//
			SectionBase newCurrentSection = this.GetCurrentSection( true );
			// JAS 11/19/04 BR00818 - SSP recommended this fix: Added the if ( null  != newCurrentSection ) 
			if ( null != newCurrentSection )
			{
				if ( null != oldCurrentSection && oldCurrentSection != newCurrentSection )
				{
					EditSectionBase es = oldCurrentSection as EditSectionBase;

					if ( null != es )
					{
						es.ValidateSection( );
					}
				}
			}

			// SSP 10/2/01
			// After such operations as backspance or delete
			// or up arrow keys, we want to see if the text
			// was changed, and if so then fire TextChanged event
			//				
			string newText = this.GetText( InputMaskMode.IncludeBoth );
			if ( !newText.Equals( oldText ) )
			{
				// SSP 1/24/02
				// Took the try-catch out around the event firing
				//
				//try
				//{
				this.OnTextChanged( true, command );
				//}
				//catch ( Exception )
				//{
				//}
			}

			// SSP 12/4/01
			// Only redraw if needed.
			//

			if ( dirtyElements )
				this.DirtyMaskElement( );
			else if ( repaint )
				this.DirtyMaskElement( );

			// SSP 6/29/12 TFS115883
			// 
			if ( dirtyElements || repaint )
				this.NotifyStateChanged( );

			return true;
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		// SSP 10/5/09 - NAS10.1 Spin Buttons
		// Added amount parameter to support SpinUp and SpinDown commands taking a parameter that 
		// indicates the amount by which to spin.
		// 
		//internal bool Spin( bool up, bool setToLimit, bool fireValueChanged )
		internal bool Spin( bool up, object amount, bool setToLimit, bool fireValueChanged )
		{
			// SSP 10/5/09 - NAS10.1 Spin Buttons
			// Added amount parameter to support SpinUp and SpinDown commands taking a parameter that 
			// indicates the amount by which to spin.
			// 
			//if ( !this.CanSpin( up ) )
			if ( !this.CanSpin( up, amount ) )
				return false;

			string oldText = null;

			if ( fireValueChanged )
				oldText = this.GetText( InputMaskMode.IncludeBoth );

			try
			{

				// SSP 1/21/02
				// If the masked edit has a mask for date or time, then when
				// it's empty and the caret is positioned at the end, then
				// put the current date or time in it.
				//
				if ( this.SelectionStart + this.SelectionLength >= this.GetTotalNumberOfDisplayChars( ) )
				{
					// SSP 9/27/11 - Optimizations
					// 
					//if ( XamMaskedInput.IsMaskValidForDataType( typeof( DateTime ), this.MaskInfo.ResolvedMask, this.MaskInfo.FormatProvider ) )
					if ( null != this.Sections && XamMaskedInput.IsMaskValidForDataType( typeof( DateTime ), this.Sections ) )
					{	
						string str = this.GetText( InputMaskMode.Raw );

						// Only do so if there is no text entered currently
						// JJD 12/9/02 - FxCop
						// Check the length instead of an == check.	
						//if ( null == str || "".Equals( str ) )
						if ( null == str || str.Length == 0 )
						{
							// SSP 5/19/09
							// Setting value can throw an exception if the value is invalid for the data type.
							// In which case we should catch the exception and do nothing. Enclosed the existing
							// code into try-catch block.
							// 
							try
							{
								this.Value = DateTime.Now;
							}
							catch
							{
							}
							
							// break out of the case block skipping below code.
							//
							return true;
						}
					}
				}

				// SSP 10/8/09 - NAS10.1 Spin Buttons
				// 
				// ----------------------------------------------------------------------
				XamMaskedInput editor = this.MaskedInput;
				SpinInfo spinInfo = this.GetResolvedSpinInfo( amount );
				if ( null != amount || null != spinInfo )
					return null != spinInfo && spinInfo.Spin( up );
				// ----------------------------------------------------------------------

				EditSectionBase section = this.GetSpinSection( );

				if ( null != section && section.SupportsSpinning )
					return section.Spin( up, setToLimit );

				return false;
			}
			finally 
			{
				this.DirtyMaskElement( );

				// If fireValueChanged is true and the text has changed, then fire
				// text changed event.
				//
				if ( fireValueChanged )
				{
					string newText = this.GetText( InputMaskMode.IncludeBoth );

					if ( null != newText && null != oldText &&
						0 != Utils.CompareStrings( oldText, newText, false ) )
					{
						this.OnTextChanged( true );
					}
				}
			}
		}







		internal EditSectionBase GetSpinSection( )
		{
			EditSectionBase section = this.CurrentSection as EditSectionBase;

			if ( null != section && section.SupportsSpinning )
				return section;

			DisplayCharBase prevDc = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );

			if ( null != prevDc )
			{
				section = prevDc.Section as EditSectionBase;

				if ( null != section && section.SupportsSpinning )
					return section;
			}

			return null;
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private bool InternalCompareNumerical( object o1, object o2, out int result )
		{
			result = 0;

			//	BF 6.3.02
			//	If either of the comparands are null, return false since at least one
			//	of them is not a numerical type
			if ( o1 == null || o2 == null )
				return false;

			// If types are the same, and they implement IComparer, then use that to
			// compare.
			//
			if ( o1.GetType( ) != o2.GetType( ) )
			{
				IFormatProvider formatProvider = this.MaskInfo.FormatProvider;
				
				// If the types are different, then try to convert them to the same type
				//			
				try
				{
					if ( o1 is IComparable )
						o2 = Convert.ChangeType( o2, o1.GetType( ), formatProvider );
					else if ( o2 is IComparable )
						o1 = Convert.ChangeType( o1, o2.GetType( ), formatProvider );
				}
				catch ( Exception )
				{
				}
			}

			if ( o1.GetType( ) == o2.GetType( ) )
			{
				if ( o1 is IComparable )
				{
					// If min constraint is not met, then return false
					//
					try
					{
						result = ((IComparable)o1).CompareTo( o2 );
						return true;
					}
					catch ( Exception )
					{
						return false;
					}
				}
			}

			return false;
		}

		internal bool ReadOnly
		{
			get
			{
				return this.MaskedInput.IsReadOnly;
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool CanSpin( bool up )
		{
			return this.CanSpin( up, null );
		}

		// SSP 10/8/09 - NAS10.1 Spin Buttons
		// 
		private SpinInfo GetResolvedSpinInfo( object amount )
		{
			XamMaskedInput editor = this.MaskedInput;
			SpinInfo spinInfo = editor._cachedSpinInfo;
			if ( null != amount || null != spinInfo )
			{
				if ( null == spinInfo || null != amount && !spinInfo.IsSameSpinIncrement( amount ) )
					spinInfo = SpinInfo.Parse( editor, amount );
			}

			return spinInfo;
		}

		// SSP 10/5/09 - NAS10.1 Spin Buttons
		// Added an overload that takes in amount parameter.
		// 
		/// <summary>
		/// Indicates whether current section can be incremented or decremented.
		/// </summary>
		/// <param name="up">True for incremenet, false for decrement.</param>
		/// <param name="amount">Optional - amount by which to increment or decrement.</param>
		/// <returns></returns>
		internal bool CanSpin( bool up, object amount )
		{
			// SSP 12/4/01 UWG809
			// Added ReadOnly property to the control. If set
			// do not allow the user to modify the text.
			//
			if ( this.ReadOnly )
				return false;

			// SSP 10/8/09 - NAS10.1 Spin Buttons
			// 
			// ----------------------------------------------------------------------
			XamMaskedInput editor = this.MaskedInput;
			SpinInfo spinInfo = this.GetResolvedSpinInfo( amount );
			if ( null != amount || null != spinInfo )
				return null != spinInfo && spinInfo.CanSpin( up );
			// ----------------------------------------------------------------------

			// If the masked edit has a mask for date or time, then when
			// it's empty and the caret is positioned at the end, then
			// put the current date or time in it.
			//
			// SSP 9/27/11 - Optimizations
			// 
			//if ( XamMaskedInput.IsMaskValidForDataType( typeof( DateTime ), this.MaskInfo.ResolvedMask, this.MaskInfo.FormatProvider ) )
			if ( null != this.Sections && XamMaskedInput.IsMaskValidForDataType( typeof( DateTime ), this.Sections ) )
			{
				string str = this.GetText( InputMaskMode.Raw );

				// Only do so if there is no text entered currently
				// JJD 12/9/02 - FxCop
				// Check the length instead of an == check.	
				//if ( null == str || "".Equals( str ) )
				if ( null == str || str.Length == 0 )
				{
					// break out of the case block skipping below code.
					//
					return true;
				}
			}

			EditSectionBase section = this.GetSpinSection( );

			if ( null != section && section.SupportsSpinning )
			{
				if ( !section.CanSpin( up ) )
					return false;


				object minValue = this.MaskInfo.MinValue;
				object maxValue = this.MaskInfo.MaxValue;

				// JDN 10/29/04 - SpinWrap: support for spin button wrapping
				int minYear = 0;
				int maxYear = 9999;
				int minMonth = 1;
				int maxMonth = 12;

				
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

				DateTime? minDateTime = minValue is DateTime ? (DateTime?)minValue : null;
				DateTime? maxDateTime = maxValue is DateTime ? (DateTime?)maxValue : null;

                // AS 8/25/08 Support Calendar
                System.Globalization.Calendar calendar = this.Sections.Calendar;

				if (minDateTime != null && maxDateTime != null)
				{
                    // AS 8/25/08 Support Calendar
                    //minYear = minDateTime.Value.Year;
					//maxYear = maxDateTime.Value.Year;
					//minMonth = minDateTime.Value.Month;
					//maxMonth = maxDateTime.Value.Month;
					minYear = calendar.GetYear(minDateTime.Value);
                    maxYear = calendar.GetYear(maxDateTime.Value);
                    minMonth = calendar.GetMonth(minDateTime.Value);
                    maxMonth = calendar.GetMonth(maxDateTime.Value);
				}

				// allow spinning for date sections unless the year or month min max values need to be honored
				if ( this.MaskInfo.MaskedInput.SpinWrap )
				{

					if ( section is YearSection && ( minYear != maxYear ) )
						return true;

					if ( section is MonthSection )
					{
						if ( !( ( minYear == maxYear ) && ( minMonth == maxMonth ) ) )
							return true;
					}
					if ( section is DaySection )
						return true;
				}


				if ( up )
				{
					// JDN 10/29/04 SpinWrap
					//object maxValue = this.MaskInfo.Owner.GetMaxValue( this.MaskInfo.OwnerContext );//					
					//maxValue = this.MaskInfo.MaskedInput.ValueToDataValue( maxValue, this.MaskInfo.Owner, this.MaskInfo.OwnerContext );

					if ( null != maxValue )
					{
						NumberSection numberSection = null;

						if ( section is NumberSection )
							numberSection = (NumberSection)section;
						else if ( section is FractionPart )
							numberSection = section.PreviousEditSection as NumberSection;

						bool doFractionComparision = true;

						if ( null != numberSection
							// SSP 8/7/02
							// Also check for the type of the number section.
							//
							&& typeof( NumberSection ) == numberSection.GetType( ) )
						{

							// JDN 10/29/04 - allow spin buttons to wrap value
							if ( this.MaskInfo.MaskedInput.SpinWrap )
								return true;

							try
							{
								decimal intPortion = numberSection.ToDecimal( );

								int result;
								if ( this.InternalCompareNumerical( intPortion, maxValue, out result ) )
								{
									if ( result >= 0 )
										return false;
								}
								doFractionComparision = false;
							}
							catch ( Exception )
							{
							}
						}

						if ( doFractionComparision && section is FractionPart )
						{
							// JDN 10/29/04 - allow spin buttons to wrap value
							if ( this.MaskInfo.MaskedInput.SpinWrap )
								return true;

							numberSection = section.PreviousEditSection as NumberSection;

							double fractionPortion = ( (FractionPart)section ).GetFractionValue( );

							int result;
							if ( this.InternalCompareNumerical( fractionPortion, maxValue, out result ) &&
								result >= 0 )
								return false;
						}
						else if ( section is YearSection )
						{
							try
							{
								// SSP 8/7/02
								// Use GetYear method.
								//
								//int year = ((YearSection)section).ToInt( );
                                // AS 10/8/08 Optimization - TFS8781
								//int year = ( (YearSection)section ).GetYear( );
                                int year;
                                if  (((YearSection)section).TryGetYear(out year))
                                {

								if ( maxDateTime != null )
								{
                                    // AS 8/25/08 Support Calendar
                                    //if ( year >= maxDateTime.Value.Year )
									if ( year >= calendar.GetYear(maxDateTime.Value) )
										return false;
								}

                                }
							}
							catch ( Exception )
							{
							}
						}
						// JDN 10/29/04 UWE1066
						// honor min max constraints
						else if ( section is MonthSection )
						{
							try
							{
                                // AS 10/8/08 Optimization - TFS8781
								//int month = ( (MonthSection)section ).ToInt( );
                                int month;
                                if  (((MonthSection)section).TryToInt(out month))
                                {

								if ( maxDateTime != null )
								{
									if ( minYear == maxYear )
									{
                                        // AS 8/25/08 Support Calendar
                                        //if ( month >= maxDateTime.Value.Month )
										if ( month >= calendar.GetYear(maxDateTime.Value) )
											return false;
									}
								}

                                }
							}
							catch ( Exception )
							{
							}
						}
						// JDN 10/29/04 UWE1066
						// honor min max constraints
						else if ( section is DaySection )
						{
							try
							{
                                // AS 10/8/08 Optimization - TFS8781
								//int day = ( (DaySection)section ).ToInt( );
                                int day;
                                if  (((DaySection)section).TryToInt(out day))
                                {

								if ( maxDateTime != null )
								{
									if ( ( minYear == maxYear ) && ( minMonth == maxMonth ) )
									{
                                        // AS 8/25/08 Support Calendar
                                        //if ( day >= maxDateTime.Value.Day )
										if ( day >= calendar.GetDayOfMonth(maxDateTime.Value) )
											return false;
									}
								}

                                }
							}
							catch ( Exception )
							{
							}
						}
					}
				}
				else
				{
					// JDN 10/29/04 SpinWrap
					//object minValue = this.MaskInfo.Owner.GetMinValue( this.MaskInfo.OwnerContext );					
					//minValue = this.MaskInfo.MaskedInput.ValueToDataValue( minValue, this.MaskInfo.Owner, this.MaskInfo.OwnerContext );

					if ( null != minValue )
					{
						NumberSection numberSection = null;

						if ( section is NumberSection )
							numberSection = (NumberSection)section;
						else if ( section is FractionPart )
							numberSection = section.PreviousEditSection as NumberSection;

						bool doFractionComparision = true;

						if ( null != numberSection
							// SSP 8/7/02
							// Also check for the type of the number section.
							//
							&& typeof( NumberSection ) == numberSection.GetType( ) )
						{
							// JDN 10/29/04 - allow spin buttons to wrap value
							if ( this.MaskInfo.MaskedInput.SpinWrap )
								return true;

							try
							{
								decimal intPortion = numberSection.ToDecimal( );

								int result;
								if ( this.InternalCompareNumerical( intPortion, minValue, out result ) )
								{
									// If the number section is at min value, then still allow decrementing
									// the fraction part.
									//

									// JDN 1/10/05 BRO1265 
									if ( result <= 0 && section == numberSection )
										return false;

									// JDN 1/10/05 BRO1265 
									// if ( result < 0 )
									//return false;

									// JDN 1/10/05 BRO1265 
									// if ( 0 == result && section == numberSection )
									// return false;
								}

								doFractionComparision = false;
							}
							catch ( Exception )
							{
							}
						}

						if ( doFractionComparision && section is FractionPart )
						{
							// JDN 10/29/04 - allow spin buttons to wrap value
							if ( this.MaskInfo.MaskedInput.SpinWrap )
								return true;

							numberSection = section.PreviousEditSection as NumberSection;

							double fractionPortion = ( (FractionPart)section ).GetFractionValue( );

							int result;
							if ( this.InternalCompareNumerical( fractionPortion, minValue, out result ) &&
								result <= 0 )
								return false;
						}
						else if ( section is YearSection )
						{
							try
							{
								// SSP 8/7/02
								// Use GetYear method.
								//
								//int year = ((YearSection)section).ToInt( );
                                // AS 10/8/08 Optimization - TFS8781
								//int year = ( (YearSection)section ).GetYear( );
                                int year;
                                if  (((YearSection)section).TryGetYear(out year))
                                {

								if ( minDateTime != null )
								{
                                    // AS 8/25/08 Support Calendar
                                    //if ( year <= minDateTime.Value.Year )
									if ( year <= calendar.GetYear(minDateTime.Value) )
										return false;
								}

                                }
							}
							catch ( Exception )
							{
							}
						}
						// JDN 10/29/04 UWE1066
						// honor min max constraints
						else if ( section is MonthSection )
						{
							try
							{
                                // AS 10/8/08 Optimization - TFS8781
								//int month = ( (MonthSection)section ).ToInt( );
                                int month;
                                if  (((MonthSection)section).TryToInt(out month))
                                {

								if ( minDateTime != null )
								{
									if ( minYear == maxYear )
									{
                                        // AS 8/25/08 Support Calendar
                                        //if ( month <= minDateTime.Value.Month )
										if ( month <= calendar.GetMonth(minDateTime.Value) )
											return false;
									}
								}

                                }
							}
							catch ( Exception )
							{
							}
						}
						// JDN 10/29/04 UWE1066
						// honor min max constraints
						else if ( section is DaySection )
						{
							try
							{
                                // AS 10/8/08 Optimization - TFS8781
								//int day = ( (DaySection)section ).ToInt( );
                                int day;
                                if  (((DaySection)section).TryToInt(out day))
                                {

								if ( minDateTime != null )
								{
									if ( ( minYear == maxYear ) && ( minMonth == maxMonth ) )
									{
                                        // AS 8/25/08 Support Calendar
                                        //if ( day <= minDateTime.Value.Day )
										if ( day <= calendar.GetDayOfMonth(minDateTime.Value) )
											return false;
									}
								}

                                }
							}
							catch ( Exception )
							{
							}
						}

					}
				}

				return true;
			}

			return false;
		}

		internal void IllegalOperation( string message )
		{
			InvalidOperationEventArgs e = new InvalidOperationEventArgs( message );

			this.OnInvalidOperation( e );

			if ( e.Beep )
			{
				this.MaskInfo.MaskedInput.Beep( );
			}
		}

		private void IllegalCharPressed( DisplayCharBase displayChar, char c )
		{
			InvalidCharEventArgs e = new InvalidCharEventArgs( c, displayChar );

			this.OnInvalidChar( e );

			if ( e.Beep )
			{
				this.MaskInfo.MaskedInput.Beep( );
			}

			_lastIllegalChar		   = c;
			_lastIllegalDisplayChar = displayChar;
		}

		// SSP 12/2/04 BR00890
		// Added EraseDisplayChars method. Moved the code from EraseSelectedText into here.
		//
		private void EraseDisplayChars( int startIndex, int eraseCharCount )
		{
			int charCount = this.GetTotalNumberOfDisplayChars( );

			if ( startIndex < 0 || startIndex > charCount )
			{
				Debug.Assert( false );
				return;
			}

			int i = startIndex;
			int c = i + eraseCharCount;

			for ( ; i < c; i++ )
			{
				int charPos = i;

				if ( charPos >= charCount )
					break;

				DisplayCharBase dc = this.GetDisplayCharAtPosition( charPos );

				Debug.Assert( null != dc, "GetDisplayCharAtPosition returned null" );

				EditSectionBase editSection = dc.Section as EditSectionBase;

				// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
				// 
				//if ( null != editSection && EditOrientation.RightToLeft == editSection.Orientation )
				if ( this.InsertMode && IsRightToLeft( editSection ) )
				{
					int p = System.Math.Min( c-1, editSection.LastDisplayChar.OverallIndexInEdit );
					DisplayCharBase dcTemp = this.GetDisplayCharAtPosition( p );

					if ( null != dcTemp )
					{
						p = dcTemp.Index;
					}
					else
					{
						p = dc.Index;
					}
					editSection.DeleteCharsAndShift( p,
						System.Math.Min( c - dc.OverallIndexInEdit, 
						1 + editSection.LastDisplayChar.Index - dc.Index ) );
					i = editSection.LastDisplayChar.OverallIndexInEdit;
					continue;
				}

				// only erase display chars that are editable, literals will stay the same
				if ( null != dc && dc.IsEditable )					
					dc.Char = (char)0; // erase it by setting it to 0 					
			}
		}
		






		internal void EraseSelectedText( )
		{
			int charCount = this.GetTotalNumberOfDisplayChars( );

			Debug.Assert( this.CaretPosition >= 0 && this.CaretPosition <= charCount,
				"CaretPosition is out of range" );

			// if nothing is selected, then just return
			if ( this.SelectionLength <= 0 )
				return;

			// SSP 12/2/04 BR00890
			// Added EraseDisplayChars method. Moved the code from here into that method.
			//
			this.EraseDisplayChars( this.SelectionStart, this.SelectionLength );
		}


		internal EditSectionBase FirstEditSection
		{
			get
			{				
				SectionBase section = this.FirstSection;

				if ( null == section )
					return null;

				if ( section is EditSectionBase )
					return (EditSectionBase)section;

				return section.NextEditSection;
			}
		}

		internal InputCharBase FirstEditDisplayChar
		{
			get
			{
				SectionBase section = this.FirstSection;

				return null != section ? section.FirstDisplayChar as InputCharBase : null;
			}
		}

		internal EditSectionBase LastEditSection
		{
			get
			{
				SectionBase section = this.LastSection;

				if ( null == section )
					return null;

				if ( section is EditSectionBase )
					return (EditSectionBase)section;

				return section.PreviousEditSection;
			}
		}

		internal SectionBase FirstSection
		{
			get
			{
				if ( null == this.Sections || this.Sections.Count <= 0 )
					return null;

				return this.Sections[ 0 ];
			}
		}

		internal SectionBase LastSection
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                return GetLastSection(this.Sections);
            }
		}

        // AS 10/6/08 TFS7567
        internal static SectionBase GetLastSection(SectionsCollection sections)
        {
            if (null == sections || sections.Count <= 0)
                return null;

            return sections[sections.Count - 1];
        }






		internal InputCharBase FirstFilledDisplayChar
		{
			get
			{
				for ( int i = 0, count = this.GetTotalNumberOfDisplayChars( ); i < count; i++ )
				{
					InputCharBase ic = this.GetDisplayCharAtPosition( i ) as InputCharBase;
					if ( null != ic && ic.IsEditable && ! ic.IsEmpty )
						return ic;
				}

				return null;
			}
		}






		internal InputCharBase LastFilledDisplayChar
		{
			get
			{
				for ( int i = this.GetTotalNumberOfDisplayChars( ) - 1; i >= 0; i-- )
				{
					InputCharBase ic = this.GetDisplayCharAtPosition( i ) as InputCharBase;
					if ( null != ic && ic.IsEditable && ! ic.IsEmpty )
						return ic;
				}

				return null;
			}
		}

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)







		internal DisplayCharBase LastDisplayChar
		{
			get
			{
				return null != this.LastSection ? this.LastSection.LastDisplayChar : null;
			}
		}






		internal SectionBase CurrentSection
		{
			get
			{
				DisplayCharBase dc = this.GetCurrentDisplayChar( );

				// if the caret is positioned after the last display char, then there
				// is no current section
				if ( null == dc )
					return null;
				else // otherwise return the section current input position is in
					return dc.Section;
			}
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal DisplayCharBase GetCurrentDisplayChar( )
		{
			DisplayCharBase displayChar = null;

			int charCount = this.GetTotalNumberOfDisplayChars( );

			// see if the current caret position is within range or beyond the last
			// display char
			if ( this.CaretPosition >= 0 && this.CaretPosition < charCount )
			{			
				displayChar = this.GetDisplayCharAtPosition( this.CaretPosition );
			}
			// else in this case the caret is beyond the last display char
			
			return displayChar;			
		}

		





		internal void ToggleInsertMode( )
		{
			this.InsertMode = !this.InsertMode;			
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal bool InsertMode
		{
			get
			{
				// SSP 10/12/08 BR35095
				// 
				//return this._insertMode;
				XamMaskedInput maskedInput = this.MaskedInput;
				return null == maskedInput || maskedInput.InsertMode;
			}
			set
			{
				if ( value != this.InsertMode )
				{
					XamMaskedInput maskedInput = this.MaskedInput;
					
					if ( null != maskedInput )
						maskedInput.InsertMode = value;

					if ( !this.InsertMode )
					{
						// if we have just toggled into overwrite mode, then make sure the 
						// caret position is not after the last char
						if ( this.IsAfterLastCharacter )
						{
							this.CaretPosition = this.GetTotalNumberOfDisplayChars( ) - 1;
						}
					}
				}
			}
		}






		internal bool IsRightToLeft( SectionBase section )
		{
			EditSectionBase editSection = section as EditSectionBase;

			return null != editSection && EditOrientation.RightToLeft == editSection.Orientation;
		}

		private bool CanInsertAt( char c, int pos )
		{
			DisplayCharBase dc = this.GetDisplayCharAtPosition( pos );

			if ( null == dc )
				return false;

			if ( !dc.IsEditable )
			{
				// if dc is a literal
				return false;
			}


			SectionBase section = dc.Section;

			EditSectionBase editSection = section as EditSectionBase;
			
			if ( null != editSection )
			{
				return editSection.CanInsertCharAt( dc.Index, c );
			}
			else
			{
				// section is non-editable
				return false;
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void EnsureNumberSectionState( )
		{
			// SSP 8/6/07 BR24752 BR24720
			// In overwrite mode, keep the caret where it is.
			// 
			if ( !this.InsertMode )
				return;

			DisplayCharBase dc = this.GetDisplayCharAtPosition( this.CaretPosition );
			DisplayCharBase dc2 = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );

			if ( null != dc && null != dc2 )
			{
				if ( dc2.Section != dc.Section )
				{
					if ( this.IsRightToLeft( dc2.Section ) )
					{
						// SSP 4/8/05 BR03077
						//
						// --------------------------------------------------------------
                        //return
						if ( dc2.Section.NextEditSection is FractionPartContinuous )
							dc = dc2;
						else
							return;
						// --------------------------------------------------------------
					}
				}
			}

			// SSP 4/8/05 BR03077
			//
			if ( null == dc )
				dc = dc2;

			if ( null != dc && this.IsRightToLeft( dc.Section ) && dc.IsEmpty )
			{
				EditSectionBase editSection = (EditSectionBase)dc.Section;

				// SSP 4/8/05 BR03077
				// Added following if block.
				//
				if ( editSection.IsEmpty && editSection.NextEditSection is FractionPartContinuous )
				{
					FractionPartContinuous fp = (FractionPartContinuous)editSection.NextEditSection;
					if ( fp.FirstDisplayChar.IsEmpty )
						editSection = fp;
				}

				int index = null != editSection.FirstFilledChar ?
					editSection.FirstFilledChar.OverallIndexInEdit :
					1 + editSection.LastDisplayChar.OverallIndexInEdit;
				
				this.SetCaretPivot( index );
				return;
			}

			// If in a fraction part section, then position the caret right after the
			// last filled display character if it's farther
			//
			if ( null != dc && dc.Section is FractionPart && dc.IsEmpty )
			{
				EditSectionBase editSection = (EditSectionBase)dc.Section;
				
				int lastFilledIndex = 
					null != editSection.LastFilledChar
					? 1 + editSection.LastFilledChar.OverallIndexInEdit
					: editSection.FirstDisplayChar.OverallIndexInEdit;

				if ( dc.OverallIndexInEdit > lastFilledIndex )
				{
					this.SetCaretPivot( lastFilledIndex );
				}
			}

			// SSP 4/8/05 BR03077
			// If the caret is at the first character of the right-to-left fraction part then
			// the next characters typed should go to the integer part.
			//
			if ( null != dc && dc.Section is FractionPartContinuous )
			{
				FractionPartContinuous fp = (FractionPartContinuous)dc.Section;
				if ( 0 == dc.Index && ! dc.IsEmpty && this.IsRightToLeft( fp.PreviousEditSection ) 
					// SSP 2/9/09 TFS10881
					// Copied the following fix from WinForms MaskedInput.
					// MBS 12/15/08 - TFS10878
					// If the caret is to the right of the digit being edited (which will happen
					// by default if you start typing in a continous section that has 1 digit
					// of precision in the decimal section), we don't want to shift over to the
					// previous section.
					// 
					&& dc.OverallIndexInEdit >= this.CaretPosition 
					)
					this.SetCaretPivot( 1 + fp.PreviousEditSection.LastDisplayChar.OverallIndexInEdit );
			}
		}

		// SSP 4/19/11 TFS70649
		// Added a helper method.
		// 
		internal string NormalizeEncodingHelper( string text )
		{
			// SSP 6/27/11 TFS58898
			// Only normalize for numeric input. For textual input, don't normalize as it may be
			// reasonable to input double width characters, like in japanese IME.
			// 
			// ------------------------------------------------------------------------------------
			//return null != text ? text.Normalize( NormalizationForm.FormKC ) : null;
			if ( null != text )
			{
				bool normalize = true;

				MaskInfo maskInfo = this.MaskInfo;
				if ( null != maskInfo )
				{
					if ( !Utils.IsNumericType( maskInfo.DataType )
						&& typeof( DateTime ) != maskInfo.DataType
						&& null == XamMaskedInput.GetSection( maskInfo.Sections, typeof( NumberSection ) ) )
						normalize = false;
				}

				if ( normalize )
				{
					// Apparently Normalize doesn't exist in SL.
					// 
					//text = text.Normalize( NormalizationForm.FormKC );

					StringBuilder characters = new StringBuilder( "0123456789" );

					foreach ( char c in "+-/:$,." )
					{
						char t = maskInfo.GetCultureChar( c );
						characters.Append( t );
						if ( c != t )
							characters.Append( c );
					}

					List<string> characterStrs = new List<string>( characters.Length );

					for ( int i = 0; i < characters.Length; i++ )
						characterStrs.Add( characters[i].ToString( ) );

					StringBuilder result = new StringBuilder( text.Length );
					CultureInfo culture = CultureInfo.CurrentCulture;

					for ( int i = 0; i < text.Length; i++ )
					{
						char ii = text[i];
						string iiStr = ii.ToString( );
						bool matchFound = false;

						for ( int j = 0; j < characterStrs.Count; j++ )
						{
							string jjStr = characterStrs[j];

							if ( 0 == string.Compare( iiStr, jjStr, culture, CompareOptions.IgnoreWidth ) )
							{
								result.Append( jjStr );
								matchFound = true;
								break;
							}
						}

						// SSP 5/11/12 TFS111480
						// If we did not find a match, we still want to process the input character.
						// 
						if ( ! matchFound )
							result.Append( ii );
					}

					text = result.ToString( );
				}
			}
			// ------------------------------------------------------------------------------------

			return text;
		}

		// SSP 3/23/09 IME
		// Added normalizeEncoding parameter.
		// 
		//internal void ProcessTextInput( TextCompositionEventArgs e )
		internal bool ProcessTextInput( string text, bool normalizeEncoding )
		{
			if ( this.ReadOnly )
				return false;

			try
			{
				_lastIllegalChar = (char)0;

				// SSP 3/23/09 IME
				// 
				// --------------------------------------------------------------------
				int numOfCharsProcessed = 0;

				
				
				
				
				if ( normalizeEncoding )
					// SSP 4/19/11 TFS70649
					// Moved the code into the new NormalizeEncodingHelper helper method.
					// 
					//text = text.Normalize( NormalizationForm.FormKC );
					text = NormalizeEncodingHelper( text );
				// --------------------------------------------------------------------

				foreach ( char c in text )
				{
					this.InternalProcessChar( c );

					// If the character was invalid for the current display character then
					// break out without processing any further characters.
					// 
					if ( 0 != _lastIllegalChar )
						break;

					
					
					numOfCharsProcessed++;
				}

				// If char was processed that means the text was modified
				// in the text box. Therefore we fire text changed event.
				// 
				// SSP 3/23/09 IME
				// 
				//if ( 0 == _lastIllegalChar )
				if ( numOfCharsProcessed > 0 )
					this.OnTextChanged( true );

				return true;
			}
			finally
			{
			}
		}

		// SSP 3/22/11 TFS30509
		// 
		/// <summary>
		/// Returns the display char at which the input processing should start when some text
		/// is selected and a character is input.
		/// </summary>
		/// <returns>Overall position of the display char.</returns>
		internal int GetInputPositionInSelection( char c )
		{
			DisplayCharBase start = this.GetDisplayCharAtPosition( this.SelectionStart );
			DisplayCharBase end = this.GetDisplayCharAtPosition( this.SelectionStart + this.SelectionLength - 1 );

			DisplayCharBase nextEditableDisplayChar = start.NextEditableDisplayChar;

			// If the selection starts at a literal character and spans into or accorss an editable section,
			// then the character that's being input should go into that section - and not into the previous
			// right-to-left section.
			// 
			if ( !start.IsEditable && start.Section != end.Section
				&& null != nextEditableDisplayChar && nextEditableDisplayChar.OverallIndexInEdit <= end.OverallIndexInEdit )
			{
				return nextEditableDisplayChar.OverallIndexInEdit;
			}

			return -1;
		}







		internal void InternalProcessChar( char c )
		{
			// SSP 1/17/02
			// In design-mode return.
			//
			//if ( !this.IsInitialized )
			if ( !this.IsInitialized || this.MaskedInput.DesignMode )
				return;

			// JAS 12/15/04 Japanese DateTime Separators Implementation
			SectionBase dateTimeSection = this.CurrentSection == null ? this.LastEditSection : this.CurrentSection;

			if( dateTimeSection != null && 
				(dateTimeSection.IsDateTimeSeperatorSection || 
				 dateTimeSection.IsDateSection				|| 
				 dateTimeSection.IsTimeSection)				 )
			{
				string movementChars = @"-/.,:";
				if( -1 < movementChars.IndexOf( c ) )
				{					
					SectionBase nextSection = dateTimeSection.NextEditSection;

					// If we are in the last section, wrap back to the first.
					if( nextSection == null )
						nextSection = this.FirstEditSection;

					if( nextSection != null )
					{
						this.PivotPosition = nextSection.FirstDisplayChar.OverallIndexInEdit;
						this.CaretPosition = nextSection.LastDisplayChar.OverallIndexInEdit + 1;
					}

					return;
				}
			}

			// SSP 10/8/03 UWG2548
			// Added below if block. If we are editing a number (like currency) and the decimal
			// separator is pressed, then delete the currently selected text and move the caret to
			// the fraction part so the user can start entering the fraction. This will provide greater
			// convenience when entering numbers less than 1 (like 0.25, 0.dd) without having to enter
			// the '0' since some people may prefer entering fractions without entering 0.
			//
			// ----------------------------------------------------------------------------------------
			if ( this.MaskInfo.DecimalSeperatorChar == c )
			{
				DisplayCharBase tmpDC = this.GetDisplayCharAtPosition( this.SelectionStart );

				if ( null != tmpDC && ! tmpDC.MatchChar( c )  )
				{
					SectionBase decimalSeparatorSection = tmpDC.Section;

					while ( null != decimalSeparatorSection && ! ( decimalSeparatorSection.LastDisplayChar is DecimalSeperatorChar ) )
						decimalSeparatorSection = decimalSeparatorSection.NextSection;

					if ( null != decimalSeparatorSection && decimalSeparatorSection.LastDisplayChar is DecimalSeperatorChar )
					{
						if ( this.IsAnyTextSelected )
							this.Delete( );

						this.SetCaretPivot( 1 + decimalSeparatorSection.LastDisplayChar.OverallIndexInEdit );
						this.DirtyMaskElement( );
						return;
					}
				}
			}
			// ----------------------------------------------------------------------------------------

			// SSP 3/23/11 TFS37056
			// 
			bool overwriteMode_InsertInNumberSectionOverride = false;

			if ( this.IsAnyTextSelected )
			{
				DisplayCharBase dc = this.GetDisplayCharAtPosition( this.SelectionStart );

				Debug.Assert( null != dc, "either invalid SelectionStart or GetDisplayCharAtPosition flawed" );

				// SSP 3/20/03
				// Added the if block and enclosed the already existing code in the else block.
				//
				if ( char.IsDigit( c ) && this.FirstEditSection is NumberSection && 0 == this.SelectionStart )
				{
					this.Delete( );
				}
				else
				{
					// SSP 11/11/11 TFS95408
					// If '+' or '-' is being entered and the selection in the number section starts
					// at the first filled character (filled either with a digit or '+' or '-'), we 
					// should allow replacing the selection with the entered sign. The main behavior
					// this change allows for is that when the user wants to replace the current value
					// with a new value that's negative, he/she can select all of the current value
					// (excluding any empty placeholders) and then type over the new value starting
					// with the '-' symbol.
					// 
					NumberSection dcNumberSection = dc.Section as NumberSection;
					DisplayCharBase firstFilledCharInSection = null != dcNumberSection ? dcNumberSection.FirstFilledChar : null;

					bool acceptCharOverride = null != dcNumberSection
						&& ( c == dcNumberSection.PlusSignChar || c == dcNumberSection.MinusSignChar )
						&& dcNumberSection.FirstFilledChar == dc
						// SSP 3/14/12 TFS95408
						// If some part of the number section is selected and the selection includes the first filled character,
						// then allow digit to be typed as well.
						// 
						|| char.IsDigit( c ) && null != firstFilledCharInSection && this.IsDisplayCharSelected( firstFilledCharInSection );

					if ( null != dc && dc.IsEditable && !dc.MatchChar( c )
						// SSP 11/11/11 TFS95408
						// Related to above.
						// 
						&& ! acceptCharOverride )
					{
						this.IllegalCharPressed( dc, c );
						return;
					}
					else
					{
						// SSP 3/22/11 TFS30509
						// If the selection starts at a literal character and spans into or accross an editable section
						// then the character input should go to that editable section, and not any previous right-to-left
						// section.
						// 
						int inputStartPosition = this.GetInputPositionInSelection( c );

						// SSP 3/23/11 TFS37056
						// If we are in overwrite mode and some text is selected in number section then when we delete
						// it below, the characters will get shifted. When we process the input way below, we end up
						// overwriting the shifted character because in overwrite mode we don't shift but ovwrite the
						// character at the current position. Since text was selected, we need to replace the selected
						// text, not the character that was shifted over to the location of the selected text that we
						// delete.
						// 
						if ( !this.InsertMode && null != dc && dc.Section is NumberSection )
							overwriteMode_InsertInNumberSectionOverride = true;

						// if any text is selected, erase it
						this.Delete( );

						// SSP 3/22/11 TFS30509
						// 
						if ( inputStartPosition >= 0 )
							this.SetCaretPivot( inputStartPosition );
					}
				}
			}

			// SSP 10/24/01 UWM25
			// Now we are allowing the user to position the caret at an empty
			// display char in number sections (only when they are selecting
			// via draging the mouse). So we have to make sure the caret 
			// position is where the entered character is going to go.
			//
			this.EnsureNumberSectionState( );


			DisplayCharBase displayChar = this.GetCurrentDisplayChar( );

			// SSP 8/6/07 BR24752 BR24720
			// When in overwrite mode, always process the current or the next character, even in number
			// sections.
			// 
			// ----------------------------------------------------------------------------------------
			if ( !this.InsertMode && null != displayChar && !displayChar.IsEditable )
			{
				if ( displayChar.MatchChar( c ) )
				{
					if ( null != displayChar.NextDisplayChar )
					{
						displayChar = displayChar.NextDisplayChar;
						this.SetCaretPivot( displayChar.OverallIndexInEdit );

						// SSP 8/16/10 TFS36760 - Overwrite mode with numeric sections.
						// Since we have matched the inputted character to a literal character above
						// and advanced the caret and pivot accordingly, simply return. Otherwise
						// the below logic will potentially process the character again.
						// 
						return;
					}
				}
				else
				{
					if ( null != displayChar.NextEditableDisplayChar )
					{
						displayChar = displayChar.NextEditableDisplayChar;
						this.SetCaretPivot( displayChar.OverallIndexInEdit );
					}
				}				
			}
			// ----------------------------------------------------------------------------------------

			// if displayChar is null, then the current input position is beyond the 
			// last display char (caret is flashing after the last display char, and thus
			// input can not be processed)
			if ( null != displayChar )
			{
				// now try to match the typed in character to the current DisplayChar

				// SSP 5/8/02
				// Added support for plus-minus signs in number sections.
				// So if a plus or a minus is pressed, then do the default processing.
				// Following block of code changes the sign of the number section if
				// plus or minus keys were pressed, otherwise it does the default 
				// processing.
				// -----------------------------------------------------------------
				NumberSection numberSection = displayChar.Section as NumberSection;
				
				// If we are the decimal seperator, use the number section before it
				//
				if ( null == numberSection && displayChar is DecimalSeperatorChar )
				{
					numberSection = null != displayChar.PrevDisplayChar 
						? displayChar.PrevDisplayChar.Section as NumberSection
						: null;
				}

				if ( null == numberSection && displayChar.Section is FractionPart )
				{
					numberSection = displayChar.Section.PreviousEditSection as NumberSection;
				}

                // MBS 8/3/06 BR14699
                // If we had a literal at the end of the mask, then we would be allowed to type multiple '+' or '-', which 
                // would then cause an exception to be thrown when entering a number.
                if (null != displayChar && null != displayChar.PrevDisplayChar && displayChar.PrevDisplayChar.Section is NumberSection)
                {
                    numberSection = displayChar.PrevDisplayChar.Section as NumberSection;
                }

				// See if the negative numbers are allowed.
				//
				if ( null != numberSection 
					// SSP 3/10/06 BR10576
					// Allow negative sign even if the number section's MinValue is 0 as long
					// as the overall min value is negative. For example if the editor's
					// MinValue is set to -0.5 then the number section's min value is going 
					// to be 0 however it should still allow negative sign.
					// 
					//&& numberSection.MinValue < 0 
					&& ( numberSection.MinValue < 0 || numberSection.lastConvertedMinValWithFractionPart < 0 )
					)
				{
					if ( numberSection.PlusSignChar == c )
					{
						if ( !numberSection.SetNumberSign( false ) )
							this.IllegalCharPressed( displayChar, c );

						this.DirtyMaskElement( );
						return;
					}
					else if ( numberSection.MinusSignChar == c )
					{
						if ( !numberSection.SetNumberSign( true ) )
							this.IllegalCharPressed( displayChar, c );

						this.DirtyMaskElement( );
						return;
					}
				}
				// -----------------------------------------------------------------

				// SSP 5/24/02
				// Added code for doing the accelerator key ( k = thousand, m = million )
				//
				// NOTE: Above code for doing the plus-minus signs take care of getting
				// the right number section (like for example, if it's in the fraction part,
				// it still gets the right number section).
				//
				
				//if ( null != numberSection )
				//{
				//}

				// if the displayChar is an input char, then
				if ( displayChar.IsEditable )
				{
					if ( this.InsertMode )
					{
						// since editable character can only be in an EditSection
						EditSectionBase editSection = displayChar.Section as EditSectionBase;

						Debug.Assert( null != editSection, "non-literal character in a literal section" );
						
						if ( this.IsRightToLeft( editSection ) )
						{
							// in a number section

							if ( displayChar.Index - 1 >= 0 && editSection.InsertCharAt( displayChar.Index - 1, c ) )
							{
								this.DirtyMaskElement( );
							}							
							else
							{
								DisplayCharBase nextLiteralDc = null;

								if ( null != editSection.NextLiteralSection )
									nextLiteralDc = editSection.NextLiteralSection.FirstDisplayChar;

								if ( null == nextLiteralDc )
								{
									this.IllegalCharPressed( displayChar, c );
								}
								else if ( nextLiteralDc.MatchChar( c ) )
								{									
									// if it matches the next literal, then goto next section
									DisplayCharBase dc3 = this.GetDisplayCharAtPosition( 1 + nextLiteralDc.OverallIndexInEdit );
										
									if ( null != dc3 && dc3.IsEditable )
									{
										EditSectionBase sectionToGoTo = dc3.Section as EditSectionBase;
										
										Debug.Assert( null != sectionToGoTo, "non-literal char in a non-edit section" );

										if ( null != sectionToGoTo )
										{
											this.GotoSection( sectionToGoTo, false );
											this.SetPivot( );
										}										
									}
									else if ( null != dc3 )
									{
										this.SetCaretPivot( dc3.OverallIndexInEdit );
									}					
									else
									{
										this.IllegalCharPressed( displayChar, c );
									}
								}
								else 
								{
									if ( displayChar == editSection.LastDisplayChar )
									{
										EditSectionBase nextEditSection = editSection.NextEditSection;
									
										if ( null != nextEditSection )
										{											
											if ( this.IsRightToLeft( nextEditSection ) )
											{
												int tmp1 = -1;
												if ( null != nextEditSection.FirstFilledChar )
													tmp1 = nextEditSection.FirstFilledChar.OverallIndexInEdit - 1;
												else 
													tmp1 = nextEditSection.LastDisplayChar.OverallIndexInEdit;

												// CanInsertAt takes care of out of range parameters
												if ( this.CanInsertAt( c, tmp1 ) )
												{
													this.GotoSection( nextEditSection );
													this.SetPivot( );
													this.InternalProcessChar( c );
												}
												else
												{
													this.IllegalCharPressed( displayChar, c );
												}
											}
											else
											{
												int tmp1 = nextEditSection.FirstDisplayChar.OverallIndexInEdit;

												// CanInsertAt takes care of out of range parameters
												if ( this.CanInsertAt( c, tmp1 ) )
												{
													this.GotoSection( nextEditSection );
													this.SetPivot( );
													this.InternalProcessChar( c );
												}
												else
												{
													this.IllegalCharPressed( displayChar, c );
												}
											}											
										}
										else
										{
											this.IllegalCharPressed( displayChar, c );
										}
									}
									else
									{
										this.IllegalCharPressed( displayChar, c );
									}
								}
							}
						}
						else
						{
							// in a non-number section

							if ( null != editSection && editSection.InsertCharAt( displayChar.Index, c ) )
							{
								// now advance the input position to next input position
								this.CaretPosition++;
								this.SetPivot( );

								this.DirtyMaskElement( );
							}
								// SSP 11/19/01 UWM39
								// Try to insert the character shfting across section
								// boundaries if insertion within a section fails.
								// Added below else if clause
							else if ( this.InsertCharAtShiftGlobal( c, displayChar.OverallIndexInEdit ) )
							{
								// now advance the input position to next input position
								this.CaretPosition++;
								this.SetPivot( );

								this.DirtyMaskElement( );
							}
							else
							{
								bool flag2 = false;
								SectionBase section = displayChar.Section;
								if ( null != section.NextLiteralSection )
								{
									if ( section.NextLiteralSection.FirstDisplayChar.MatchChar( c ) )
									{
										this.CaretPosition = 1 + section.NextLiteralSection.FirstDisplayChar.OverallIndexInEdit;
										this.SetPivot( );
										flag2 = true;
									}
								}
								if ( !flag2 )
									this.IllegalCharPressed( displayChar, c );
							}
						}
					}
					else // overwrite mode
					{
						// since editable character can only be in an EditSection
						EditSectionBase editSection = displayChar.Section as EditSectionBase;

						Debug.Assert( null != editSection, "non-literal character in a literal section" );

						if ( null != editSection && EditOrientation.RightToLeft == editSection.Orientation )
						{
							// in a number section

							// SSP 3/23/11 TFS37056
							// 
							//if ( editSection.ReplaceCharAt( displayChar.Index, c ) )
							if ( overwriteMode_InsertInNumberSectionOverride
								? editSection.InsertCharAt( displayChar.Index, c )
								: editSection.ReplaceCharAt( displayChar.Index, c ) )
							{
								// SSP 8/6/07 BR24752 BR24720
								// Advance the caret position even when we are in a number section.
								// 
								// ----------------------------------------------------------------
								if ( XamMaskedInput.IsSectionNumeric( editSection ) )
								{
									this.CaretPosition++;
									this.SetPivot( );
								}
								// ----------------------------------------------------------------

								this.DirtyMaskElement( );
							}
							else
							{
								bool flag2 = false;
								SectionBase section = displayChar.Section;
								if ( null != section.NextLiteralSection )
								{
									if ( section.NextLiteralSection.FirstDisplayChar.MatchChar( c ) )
									{
										this.CaretPosition = 1 + section.NextLiteralSection.FirstDisplayChar.OverallIndexInEdit;
										this.SetPivot( );
										flag2 = true;
									}
								}
								if ( !flag2 )
									this.IllegalCharPressed( displayChar, c );
							}
						}
						else
						{
							// in a non-number section
						
							if ( null != editSection && editSection.ReplaceCharAt( displayChar.Index, c ) )
							{
								// now advance the input position to next input position
								this.CaretPosition++;
								this.SetPivot( );

								this.DirtyMaskElement( );
							}
							else
							{
								bool flag2 = false;
								SectionBase section = displayChar.Section;
								if ( null != section.NextLiteralSection )
								{
									if ( section.NextLiteralSection.FirstDisplayChar.MatchChar( c ) )
									{
										this.CaretPosition = 1 + section.NextLiteralSection.FirstDisplayChar.OverallIndexInEdit;
										this.SetPivot( );
										flag2 = true;
									}
								}
								if ( !flag2 )
									this.IllegalCharPressed( displayChar, c );
							}
						}
					}
				}
				else // in the case of a literal
				{
					if ( displayChar.MatchChar( c ) )
					{
						// in case of a literal input char, when a matching char is typed
						// just advance the input position by one
						DisplayCharBase nextDc = this.GetDisplayCharAtPosition( this.CaretPosition + 1 );

						if ( null != nextDc && nextDc.Section != displayChar.Section )
						{
							this.GotoSection( nextDc.Section );
						}
						else
						{
							this.CaretPosition++;
							this.SetPivot( );
						}
					}
					else
					{
						// in case of a literal input char, when a non-matching char is typed
						// advance the input position to next non-literal input position and try
						// to match to that input position

						DisplayCharBase prevDc = this.GetDisplayCharAtPosition( displayChar.OverallIndexInEdit - 1 );

						if ( null != prevDc && prevDc.Section != displayChar.Section )
						{
							// check to see if previous section is a number section

							EditSectionBase editSection = prevDc.Section as EditSectionBase;

							if ( this.IsRightToLeft( editSection ) )
							{
								// number section

								DisplayCharBase dc = editSection.LastDisplayChar;

								if ( this.CanInsertAt( c, dc.OverallIndexInEdit ) )
								{
									if ( editSection.InsertCharAt( dc.Index, c ) )
									{
										this.DirtyMaskElement( );
									}							
									else
									{										
										if ( dc == editSection.LastDisplayChar )
										{
											EditSectionBase nextEditSection = editSection.NextEditSection;
									
											if ( null != nextEditSection )
											{											
												if ( this.IsRightToLeft( nextEditSection ) )
												{
													int tmp1 = -1;
													if ( null != nextEditSection.FirstFilledChar )
														tmp1 = nextEditSection.FirstFilledChar.OverallIndexInEdit - 1;
													else 
														tmp1 = nextEditSection.LastDisplayChar.OverallIndexInEdit;

													// CanInsertAt takes care of out of range parameters
													if ( this.CanInsertAt( c, tmp1 ) )
													{
														this.GotoSection( nextEditSection );
														this.SetPivot( );
														this.InternalProcessChar( c );
													}
													else
													{
														this.IllegalCharPressed( displayChar, c );
													}
												}
												else
												{
													int tmp1 = nextEditSection.FirstDisplayChar.OverallIndexInEdit;

													// CanInsertAt takes care of out of range parameters
													if ( this.CanInsertAt( c, tmp1 ) )
													{
														this.GotoSection( nextEditSection );
														this.SetPivot( );
														this.InternalProcessChar( c );
													}
													else
													{
														this.IllegalCharPressed( displayChar, c );
													}
												}											
											}
											else
											{
												this.IllegalCharPressed( displayChar, c );
											}
										}
										else
										{
											this.IllegalCharPressed( displayChar, c );
										}
										
									}
								}
								else
								{
									EditSectionBase nextEditSection = displayChar.Section.NextEditSection;
									
									if ( null != nextEditSection )
									{											
										if ( this.IsRightToLeft( nextEditSection ) )
										{
											int tmp1 = -1;
											if ( null != nextEditSection.FirstFilledChar )
												tmp1 = nextEditSection.FirstFilledChar.OverallIndexInEdit - 1;
											else 
												tmp1 = nextEditSection.LastDisplayChar.OverallIndexInEdit;

											// CanInsertAt takes care of out of range parameters
											if ( this.CanInsertAt( c, tmp1 ) )
											{
												this.GotoSection( nextEditSection );
												this.SetPivot( );
												this.InternalProcessChar( c );
											}
											else
											{
												this.IllegalCharPressed( displayChar, c );
											}
										}
										else
										{
											int tmp1 = nextEditSection.FirstDisplayChar.OverallIndexInEdit;

											// CanInsertAt takes care of out of range parameters
											if ( this.CanInsertAt( c, tmp1 ) )
											{
												this.GotoSection( nextEditSection );
												this.SetPivot( );
												this.InternalProcessChar( c );
											}
											else
											{
												this.IllegalCharPressed( displayChar, c );
											}
										}											
									}
									else
									{
										this.IllegalCharPressed( displayChar, c );
									}
								}							
							}
							else
							{
								EditSectionBase nextEditSection = displayChar.Section.NextEditSection;
									
								if ( null != nextEditSection )
								{											
									if ( this.IsRightToLeft( nextEditSection ) )
									{
										int tmp1 = -1;
										if ( null != nextEditSection.FirstFilledChar )
											tmp1 = nextEditSection.FirstFilledChar.OverallIndexInEdit - 1;
										else 
											tmp1 = nextEditSection.LastDisplayChar.OverallIndexInEdit;

										// CanInsertAt takes care of out of range parameters
										if ( this.CanInsertAt( c, tmp1 ) )
										{
											this.GotoSection( nextEditSection );
											this.SetPivot( );
											this.InternalProcessChar( c );
										}
										else
										{
											this.IllegalCharPressed( displayChar, c );
										}
									}
									else
									{
										int tmp1 = nextEditSection.FirstDisplayChar.OverallIndexInEdit;

										// SSP 10/29/01
										DisplayCharBase dc = this.GetDisplayCharAtPosition( tmp1 );

										// CanInsertAt takes care of out of range parameters
										if ( this.CanInsertAt( c, tmp1 ) )
										{
											this.GotoSection( nextEditSection );
											this.SetPivot( );
											this.InternalProcessChar( c );
										}
											// SSP 10/29/01 
											// Added this else if clause
										else if ( !this.InsertMode && dc.MatchChar( c ) )
										{
											this.SetCaretPivot( tmp1 );
											this.InternalProcessChar( c );
										}
										// SSP 6/6/08 - Copied fix from Win MaskedInput - MRS 6/4/2008 - BR33604
										// Addded this 'else if' block
										else if ( this.InsertCharAtShiftGlobal( c, tmp1 ) )
										{
											// now advance the input position to next input position
											this.CaretPosition = 1 + tmp1;
											this.SetPivot( );
											this.DirtyMaskElement( );
										}
										else
										{
											this.IllegalCharPressed( displayChar, c );
										}
									}											
								}
								else
								{
									this.IllegalCharPressed( displayChar, c );
								}
							}
						}	
						else
						{
							EditSectionBase nextEditSection = displayChar.Section.NextEditSection;
									
							if ( null != nextEditSection )
							{											
								if ( this.IsRightToLeft( nextEditSection ) )
								{
									int tmp1 = -1;
									if ( null != nextEditSection.FirstFilledChar )
										tmp1 = nextEditSection.FirstFilledChar.OverallIndexInEdit - 1;
									else 
										tmp1 = nextEditSection.LastDisplayChar.OverallIndexInEdit;

									// CanInsertAt takes care of out of range parameters
									if ( this.CanInsertAt( c, tmp1 ) )
									{
										this.GotoSection( nextEditSection );
										this.SetPivot( );
										this.InternalProcessChar( c );
									}
									else
									{
										this.IllegalCharPressed( displayChar, c );
									}
								}
								else
								{
									int tmp1 = nextEditSection.FirstDisplayChar.OverallIndexInEdit;

									// SSP 10/29/01
									DisplayCharBase dc = this.GetDisplayCharAtPosition( tmp1 );


									// CanInsertAt takes care of out of range parameters
									if ( this.CanInsertAt( c, tmp1 ) )
									{
										this.GotoSection( nextEditSection );
										this.SetPivot( );
										this.InternalProcessChar( c );
									}
										// SSP 10/29/01 
										// Added this else if clause
									else if ( !this.InsertMode && dc.MatchChar( c ) )
									{
										this.SetCaretPivot( tmp1 );
										this.InternalProcessChar( c );
									}
										// SSP 7/29/02 UWE84
										// Try to see if the character can be inserted by shifting globally
										// by calling InsertCharAtShiftGlobal method.
										// Added below else if block
										//
									else if ( this.InsertCharAtShiftGlobal( c, tmp1 ) )
									{
										// now advance the input position to next input position
										this.SetCaretPivot( 1 + tmp1 );
										this.DirtyMaskElement( );
									}
									else
									{
										this.IllegalCharPressed( displayChar, c );
									}
								}											
							}
							else
							{
								this.IllegalCharPressed( displayChar, c );
							}
						}
					}
				}				
			}
			else
			{
				// the caret is positioned after the last display char

				DisplayCharBase dcTemp = this.GetDisplayCharAtPosition( this.CaretPosition - 1 );

				EditSectionBase editSection = null != dcTemp ? dcTemp.Section as EditSectionBase : null;


				// SSP 5/8/02
				// Added support for plus-minus signs in number sections.
				// So if a plus or a minus is pressed, then do the default processing.
				// Following block of code changes the sign of the number section if
				// plus or minus keys were pressed, otherwise it does the default 
				// processing.
				// --------------------------------------------------------------------
				bool plusMinusSignProcessed = false;
				
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

				NumberSection numberSection = null;

				if (editSection is NumberSection)
					numberSection = (NumberSection)editSection;
				else if (editSection is FractionPart)
					numberSection = editSection.PreviousEditSection as NumberSection;

				if (numberSection != null)
				{

					// See if negative numbers are allowed.
					//
					if ( null != numberSection 
						// SSP 3/10/06 BR10576
						// Allow negative sign even if the number section's MinValue is 0 as long
						// as the overall min value is negative. For example if the editor's
						// MinValue is set to -0.5 then the number section's min value is going 
						// to be 0 however it should still allow negative sign.
						// 
						//&& numberSection.MinValue < 0 
						&& ( numberSection.MinValue < 0 || numberSection.lastConvertedMinValWithFractionPart < 0 )
						)
					{
						if ( numberSection.PlusSignChar == c )
						{
							if ( !numberSection.SetNumberSign( false ) )
								this.IllegalCharPressed( displayChar, c );

							this.DirtyMaskElement( );
							plusMinusSignProcessed = true;
						}
						else if ( numberSection.MinusSignChar == c )
						{
							if ( !numberSection.SetNumberSign( true ) )
								this.IllegalCharPressed( displayChar, c );

							this.DirtyMaskElement( );
							plusMinusSignProcessed = true;
						}
					}
				}
				// --------------------------------------------------------------------


				if ( !plusMinusSignProcessed )
				{
					if ( null != editSection && this.IsRightToLeft( editSection ) )
					{
						// number section

						DisplayCharBase dc = editSection.LastDisplayChar;

						if ( this.CanInsertAt( c, dc.OverallIndexInEdit ) )
						{
							if ( editSection.InsertCharAt( dc.Index, c ) )
							{
								this.DirtyMaskElement( );
							}							
							else
							{	
								this.IllegalCharPressed( dc, c );										
							}
						}
						else
						{
							this.IllegalCharPressed( null, c );
						}
					}				
					else
					{

						this.IllegalCharPressed( null, c );
					}
				}
			}

			this.SetPivot( );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal SectionBase GetSectionContainingPosition( int position )
		{
            return GetSectionContainingPosition(position, this.Sections);
        }

        // AS 10/6/08 TFS7567
        internal static SectionBase GetSectionContainingPosition(int position, SectionsCollection sections)
        {
            Debug.Assert(null != sections && sections.Count > 0,
                "no sections collection");

            if (null == sections || sections.Count <= 0)
                return null;

            int c = 0;
            for (int i = 0; i < sections.Count; i++)
            {
                SectionBase section = sections[i];

                // AS 2/28/07
                // We were never incrementing c so we didn't keep track
                // of how many characters were in the section before it.
                //
                //if ( position < c + section.DisplayChars.Count )
                c += section.DisplayChars.Count;

                if (position < c)
                    return section;
            }

            return null;
        }

		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal DisplayCharBase GetDisplayCharAtPosition( int position )
		{
			return XamMaskedInput.GetDisplayCharAtPosition( this.Sections, position );
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int GetTotalNumberOfDisplayChars( )
		{
			return XamMaskedInput.GetTotalNumberOfDisplayChars( this.Sections );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal bool IsInitialized
		{
			get
			{
				return this.GetTotalNumberOfDisplayChars() > 0;
			}
		}








		internal void SetCaretPivot( int pos )
		{
			this.CaretPosition = pos;
			this.PivotPosition = pos;
		}







		internal int CaretPosition
		{
			get
			{
				return this._caretPosition;
			}
			set
			{	
				// only proceed if the mask is initialized
				if ( this.IsInitialized && value != this._caretPosition )
				{
					this._caretPosition = this.CoerceCaretPosition( value );

					// SSP 6/29/12 TFS115883
					// 
					this.NotifyStateChanged( );
				}
			}
		}

		#region ConstraintCaretToFilledCharacters

		// SSP 4/18/11 TFS63991
		// 
		/// <summary>
		/// Indicates whether to allow the caret to be on a character after the right-most filled display char. 
		/// Default value is false. Resolved to true when the PromptChar is set to 0 character.
		/// </summary>
		internal bool ConstraintCaretToFilledCharacters
		{
			get
			{
				// If the prompt character is 0 character then the prompt characters aren't displayed and they
				// don't occupy any space and therefore positioning caret after a prompt character can be
				// confusing to the user as left arrow navigation will seemingly do a NOOP as prompt character
				// is navigated across. Also text inputting can also be confusing. 
				
				
				
				if ( 0 == _maskInfo.PromptChar )
					return true;

				return false;
			}
		}

		#endregion // ConstraintCaretToFilledCharacters

		internal int CoerceCaretPosition( int pos )
		{
			if ( pos < 0 )
				pos = 0;

			int lastPosition = this.GetTotalNumberOfDisplayChars( ) - 1;

			// SSP 4/18/11 TFS63991
			// 
			if ( this.ConstraintCaretToFilledCharacters )
			{
				SectionBase currentSection = this.GetCurrentSection( true );
				// SSP 9/28/11 TFS87304
				// 
				//if ( !this.IsRightToLeft( currentSection ) )
				if ( !this.IsRightToLeft( currentSection ) && !( currentSection is FractionPart ) )
				{
					InputCharBase lastFilledDisplayChar = this.LastFilledDisplayChar;
					lastPosition = null != lastFilledDisplayChar ? lastFilledDisplayChar.OverallIndexInEdit : 0;

					// SSP 11/28/11 TFS96685
					// Also allow navigating to literals.
					// 
					DisplayCharBase nextDc;
					while ( null != ( nextDc = this.GetDisplayCharAtPosition( 1 + lastPosition ) )
							&& !nextDc.IsEmpty )
						lastPosition++;
				}
			}

			EditSectionBase editSection = this.LastSection as EditSectionBase;

			// in a regular text section
			// if pos is beyond the number of display chars, then set it to
			// after the lastPosition (it can't be set to any farther)
			if ( pos > lastPosition )
				pos = lastPosition + 1;

			return pos;
		}

		// SSP 10/14/11
		// 
		internal void OnMaskReparsed( )
		{
			int caretPos = this.CaretPosition;
			int pivotPos = this.PivotPosition;

			if ( !this.InitializeInitialCaretPos( ) )
			{
				this.CaretPosition = this.CoerceCaretPosition( caretPos );
				this.PivotPosition = this.CoerceCaretPosition( pivotPos );
			}

			if ( caretPos != this.CaretPosition || pivotPos != this.PivotPosition )
				this.SyncSelectionOnImeTextBoxHelper( );

			// SSP 6/29/12 TFS115883
			// 
			this.NotifyStateChanged( );
		}






		internal int PivotPosition
		{
			get
			{
				return this._pivotPosition;
			}
			set
			{
				// only proceed if the mask is initialized
				if ( this.IsInitialized && value != this._pivotPosition )
				{
					this._pivotPosition = this.CoercePivotPosition( value );

					this.InvalidateMaskElement( );

					// SSP 6/29/12 TFS115883
					// 
					this.NotifyStateChanged( );
				}
			}
		}

		internal int CoercePivotPosition( int pos )
		{
			if ( pos < 0 )
				pos = 0;

			int lastPosition = this.GetTotalNumberOfDisplayChars( ) - 1;

			// if pos is beyond the number of display chars, then set it to
			// after the lastPosition (it can't be set to any farther)
			if ( pos > lastPosition )
				pos = lastPosition + 1;

			return pos;
		}






		internal void SelectAll( )
		{
			// If we do not have any mask initialized, then
			// just return
			// SSP 1/17/02
			// Added DesignMode check.
			//
			if ( !this.IsInitialized || this.MaskedInput.DesignMode )
				return;

			
			// SSP 31/17/06 BR10825
			// Added SelectAllBehavior property.
			// 
			// --------------------------------------------------------------------------------------------------
			if ( null != this.MaskedInput && MaskSelectAllBehavior.SelectEnteredCharacters == this.MaskedInput.SelectAllBehavior )
			{
				DisplayCharBase dc1 = this.FirstFilledDisplayChar;
				DisplayCharBase dc2 = this.LastFilledDisplayChar;

				// Include adjacent literals as well.
				// 
				while ( null != dc1 && null != dc1.PrevDisplayChar && ! dc1.PrevDisplayChar.IsEmpty )
					dc1 = dc1.PrevDisplayChar;

				while ( null != dc2 && null != dc2.NextDisplayChar && ! dc2.NextDisplayChar.IsEmpty )
					dc2 = dc2.NextDisplayChar;

				if ( null != dc1 && null != dc2 )
				{
					this.Select( dc1.OverallIndexInEdit, 1 + dc2.OverallIndexInEdit - dc1.OverallIndexInEdit, true );
					return;
				}
			}
			// --------------------------------------------------------------------------------------------------
			
			// SSP 10/23/01 UWM30
			// Select all the text even if the user has not entered anything
			//
			this.Select( 0, this.GetTotalNumberOfDisplayChars( ), true );
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void Select( int start, int length, bool syncSelectionWithImeTextBox = false )
		{
			this.SelectionStart = start;
			this.SelectionLength = length;

			if ( syncSelectionWithImeTextBox )
				this.SyncSelectionOnImeTextBoxHelper( );
		}






		internal int SelectionStart
		{
			get
			{
				// return the min of pivot position or the caret position
			
				DisplayCharBase dcCaret = this.GetDisplayCharAtPosition( this.CaretPosition );
				DisplayCharBase dcPivot = this.GetDisplayCharAtPosition( this.PivotPosition );

				if ( null != dcCaret && null != dcPivot )
				{
					// since in number sections, caret is positioned after the display char ui element

					int caretPos, pivotPos;

					caretPos = this.CaretPosition;

					pivotPos = this.PivotPosition;					

					return System.Math.Min( caretPos, pivotPos );
				}
				
				return System.Math.Min( this.PivotPosition, this.CaretPosition );				
			}
			set
			{
				// only proceed if the mask is initialized
				if ( this.IsInitialized && value != this.SelectionStart )
				{
					int pos = value;

					int count = this.GetTotalNumberOfDisplayChars( );

					if ( pos < 0 )
						pos = 0;

					if ( pos > count )
						pos = count;

					int selectionLength = this.SelectionLength;

					this.PivotPosition = pos;
					this.CaretPosition = this.PivotPosition + selectionLength;					
				}
			}
		}






		internal int SelectionLength
		{
			get
			{
				DisplayCharBase dcCaret = this.GetDisplayCharAtPosition( this.CaretPosition );
				DisplayCharBase dcPivot = this.GetDisplayCharAtPosition( this.PivotPosition );

				if ( null != dcCaret && null != dcPivot )
				{
					// since in number sections, caret is positioned after the display char ui element

					int caretPos, pivotPos;

					caretPos = this.CaretPosition;
					pivotPos = this.PivotPosition;					

					return System.Math.Abs( pivotPos - caretPos );
				}

				return System.Math.Abs( this.PivotPosition - this.CaretPosition );
			}
			set
			{				
				// only proceed if the mask is initialized
				if ( this.IsInitialized && value != this.SelectionLength )
				{
					int length = value;

					if ( length < 0 )
						length = 0;

					int count = this.GetTotalNumberOfDisplayChars( );
					
					// adjust the length taking into account CaretPosition so that
					// the selection does not extend beyond the display chars we have
					if ( this.SelectionStart + length > count )
						length = count - this.SelectionStart;


					// To emulate the behavior of the TextBox control in .NET framework,
					// we need to switch if necessary the caret and the pivot position
					// in such a way that the caret is at the end of the selection ( after
					// the pivot )
					int pivot = System.Math.Min( this.CaretPosition, this.PivotPosition );
					this.PivotPosition = pivot;
					this.CaretPosition = pivot + length;
					

					// cause it to redraw
					this.DirtyMaskElement( );
				}
			}
		}

		// SSP 8/7/03 UWG2112
		// Added an overload that indicates whether to fire the OnTextChanged event.
		//
		internal void Cut( )
		{
			this.Cut( true );
		}






		internal void Cut( bool fireValueChanged )
		{
			// SSP 6/9/03 UWG2311
			// Applied the same fix to Delete and Cut methods as the one that was applied to Paste method.
			//
			// ------------------------------------------------------------------------------------------------
			if ( this.ReadOnly )
				return;
			// ------------------------------------------------------------------------------------------------

			string s = this.SelectedText;

			// SSP 1/24/02 UWG986
			// If the text is empty then return without doing anything.
			//
			if ( null == s || 0 == s.Length )
				return;

			// MRS 8/29/05 - BR05927
			// Added a try...catch here so the ClipBoard doesn't raise
			// an unhandled Exception. 
			// Moved the delete and SetPivot down after the setting of 
			// the clipboard text, so that if he clipboard call fails, 
			// we don't delete the text. 
			//--------------------------------------------------------
//			this.Delete( );
//			this.SetPivot( );
//
//			Clipboard.SetDataObject( s, true );			
			
			try
			{
                // AS 3/18/09 TFS15623
                // Since previously we bailed out if the set failed,
                // we will exit if the set was not successful so we don't 
                // cut the text making it look like the cut succeeded.
                //
				//Clipboard.SetDataObject( s, true );
                if (!Utils.SetClipboardText(s))
                    return;
			}
			catch 
			{
				return ;
			}

			this.Delete( );
			this.SetPivot( );
			//--------------------------------------------------------

			
			// SSP 1/24/02 UWG986
			// Fire TextChanged event becuase the text has changed because of
			// the cut operation.
			//
			// SSP 1/25/02
			// Took the try-catch out around the event firing
			//
			//try
			//{
			// SSP 8/7/03 UWG2112
			// Added an overload that indicates whether to fire the OnTextChanged event.
			// Look at fireValueChanged flag before firing the value changed event.
			//
			if ( fireValueChanged )
				this.OnTextChanged( true );
			//}
			//catch ( Exception )
			//{
			//}

			this.DirtyMaskElement( );
		}

		// SSP 8/7/03 UWG2112
		// Added an overload that indicates whether to fire the OnTextChanged event.
		//
		internal void Paste( )
		{
			this.Paste( true );
		}
		





		internal void Paste( bool fireValueChanged )
		{
			// If Read-Only then do nothing.
			// 
			if ( this.ReadOnly )
				return;

            string text = Utils.GetClipboardText( );

			// SSP 4/19/11 TFS70649
			// IME related. We need to normalize the encoding the same way we do in ProcessTextInput in case
			// full-width characters are copied from the clipboard.
			// 
			text = NormalizeEncodingHelper( text );

            if (!string.IsNullOrEmpty(text))
                this.SetSelectedText(text, fireValueChanged);
        }







		internal void Copy( )
		{
			string s = this.SelectedText;

			//	BF 7.24.02	UWE87
			//	If the ClipMode is raw, and the value is 0, we are picking
			//	up the decimal, so that "0." is copied. In the special case where
			//	the SelectedText ends with the decimal separator, we will truncate it,
			//	since that will not change the value of the number it represents
			//
			if ( s != null && 
				s.Length > 1 &&
				this.MaskInfo != null &&
				this.MaskInfo.ClipMode == InputMaskMode.Raw )
			{
				string decimalSep = "" + this.MaskInfo.DecimalSeperatorChar;

				if ( s.EndsWith( decimalSep ) )
					s = s.Substring( 0, s.Length - 1 );
			}

			// Do a try...catch here so the ClipBoard doesn't raise
			// an unhandled Exception.
			// 
			try
			{
				// SSP 11/23/11 TFS93756
				// 
				DateTime operationStartTime = DateTime.Now;

				Utils.SetClipboardText( s );

				// SSP 11/23/11 TFS93756
				// 
				this.OnClipboardOperationAttempted( operationStartTime );
			}
			catch {}
		}

		#region OnClipboardOperationAttempted

		// SSP 11/23/11 TFS93756
		// 
		private void OnClipboardOperationAttempted( DateTime operationStartTime )
		{
			// In silverlight a prompt is displayed asking the end user to allow clipboard access. Somehow this
			// causes a text input event with some weird character (which may potentially be associated with 
			// Ctrl+C key press), which we need to bypass and not process it.
			// 
			// We are checking the time as a way to determine that a prompt was displayed.
			// 
			if ( ( DateTime.Now - operationStartTime ).Milliseconds > 50 )
			{
				var dispatcher = this.MaskedInput.Dispatcher;
				if ( null != dispatcher )
				{
					_skipNextTextInput = true;

					dispatcher.BeginInvoke( new Action( ( ) =>
						{
							_skipNextTextInput = false;
						} )
					);
				}
			}
		}

		#endregion // OnClipboardOperationAttempted






		internal string SelectedText
		{
			get
			{
				if ( !this.IsInitialized )
					return string.Empty;

				
#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

				return this.GetText(this.MaskInfo.ClipMode, this.SelectionStart, this.SelectionLength);
			}

			// SSP 8/7/01 UWM14
			// Since paste operation requires replacing selected text,
			// made this property writable.
			
			set
			{
				// SSP 2/24/05 BR02422
				// Moved the code from here into the new SetSelectedText method.
				//
				this.SetSelectedText( value, true );
			}
		}

		// SSP 2/7/05
		// Added IsInputNull method. Code in there is moved from the Value property's get.
		//
		internal bool IsInputNull( )
		{
			string text = this.GetText( InputMaskMode.Raw );
			return null == text || 0 == text.Length ||
				// SSP 6/6/08 - Copied fix to BR33177 from Win MaskedInput
				// We should only check for a period if we're checking for a numeric type.  Additionally,
				// we should be using the current culture's separator char.
				//
				// ".".Equals( text );
				this.TreatStringAsNull( text );				
		}

		// SSP 6/6/08 - Applied fix to BR33177 from Win MaskedInput and added optimizations as well as
		// made changes to actually make the fix work.
		// 
		private bool TreatStringAsNull( string value )
		{
			MaskInfo maskInfo = this.MaskInfo;
			char decimalChar = maskInfo.GetCultureChar( '.' );

			// We are only checking if we have [NumberSection][LiteralSection][FractionPart], so 
			// if there aren't enough entries in the arry to check for this, we don't need
			// to continue with the loop
			SectionsCollection sections = maskInfo.Sections;
			for ( int i = 0, count = sections.Count; i + 2 < count; i++ )
			{
				if ( sections[i] is NumberSection )
				{
					LiteralSection nextLiteralSection = sections[i + 1] as LiteralSection;
					if ( null != nextLiteralSection
						&& sections[i + 2] is FractionPart
						&& nextLiteralSection.GetText( ) == value )
					{
						return true;
					}
				}
			}
			return false;
		}

		// SSP 2/24/05 BR02422
		// Added SetSelectedText method. Most of the code in there is from the set of 
		// the SelectedText property except for the code that fires the OnTextChanged.
		//


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void SetSelectedText( string text, bool fireValueChanged )
		{
			if ( null == text )
				text = string.Empty;

			string oldText = this.GetText( InputMaskMode.IncludeBoth );

			// Delete selected text first
			if ( this.IsAnyTextSelected )
				this.Delete( );

			// See if there is any text at all. If there's no text, it indicates
			// that the operation is either populating an empty control or
			// that all of the text is being replaced. 
			if ( XamMaskedInput.AreAllDisplayCharsEmpty( this.Sections ) )
			{
				int numberSectionCount = XamMaskedInput.GetNumberSectionCount( this.Sections );
				if ( 1 == numberSectionCount
					|| 2 == numberSectionCount && this.LastEditSection is FractionPart )
				{
					object convertedValue;
					Exception error;
					this.MaskedInput.ConvertTextToValue( text, out convertedValue, out error );
					if ( convertedValue != null )
					{
						// Need to wrap this in a try...catch in case 
						// the value is something invalid for the 
						// data type. 
						try
						{
							this.Value = convertedValue;
						}
						catch { }

						return;
					}
				}
			}

			// SSP 11/21/07 BR27972
			// If the contents of the editor are completely empty and the caret is at the first
			// editable display character (in the beginning of the editor, however potentially after
			// any literals at the beginning of the editor), then treat the paste operation as if
			// we were pasting at the beginning.
			// 
			// --------------------------------------------------------------------------------------
			if ( this.IsInputNull( ) )
			{
				DisplayCharBase currentDC = this.GetCurrentDisplayChar( );
				EditSectionBase firstEditSection = this.FirstEditSection;
				bool startPastingAtFirstDC = false;

				if ( null != firstEditSection )
				{
					startPastingAtFirstDC = currentDC == firstEditSection.FirstDisplayChar
						// If the first edit section is right-to-left then the caret by default
						// will be after the last character.
						|| this.IsRightToLeft( firstEditSection )
						&& ( currentDC == firstEditSection.LastDisplayChar.NextDisplayChar );
				}

				if ( startPastingAtFirstDC )
					this.SetCaretPivot( 0 );
			}
			// --------------------------------------------------------------------------------------

			for ( int i = 0; i < text.Length; i++ )
			{
				this._lastIllegalDisplayChar = null;

				// SSP 9/30/03 UWE503
				// If the current section is a number section and we encounter a group separator
				// character then skip that character.
				//
				DisplayCharBase currentDC = this.GetCurrentDisplayChar( );

				// SSP 10/27/03 UWM181
				// Take into account the fact that when the caret is after the last character,
				// GetCurrentDisplayChar will return null. However if that last section
				// happens to be a number section, then characters processed by 
				// InternalProcessChar will go to that sectione. So if we are after the last
				// display char, then use the last section for the purposes of below 
				// conditions.
				//
				// ----------------------------------------------------------------------------
				




				SectionBase currentSection = null != currentDC ? currentDC.Section : this.LastEditSection;
				if ( ( currentSection is NumberSection || currentDC is DecimalSeperatorChar )
					&& text[i] == this.MaskInfo.CommaChar )
					continue;
				// ----------------------------------------------------------------------------

				
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

				//{
				this._lastIllegalDisplayChar = null;
				this.InternalProcessChar( text[i] );
				//pos = this.CaretPosition;
				//}
			
				if ( null != this._lastIllegalDisplayChar )
				{
					break;
				}
			}

			string newText = this.GetText( InputMaskMode.IncludeBoth );
			if ( oldText != newText )
			{
				this.ValidateAllSections( );

				if ( this.IsRightToLeft( this.CurrentSection ) )
				{
					EditSectionBase editSection = this.CurrentSection as EditSectionBase;

					if ( null != editSection )
					{
						this.GotoSection( editSection );
					}
				}			
	
				if ( fireValueChanged )
					this.OnTextChanged( true );
			}
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal string GetText(InputMaskMode maskMode, int start, int length)
		{
			if (!this.IsInitialized)
				return string.Empty;

            
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

            return GetText(maskMode, start, length, this.Sections);
		}

        // AS 10/6/08 TFS7567
        internal static string GetText(InputMaskMode maskMode, int start, int length, SectionsCollection sections)
        {
            Debug.Assert(length >= 0, "We should not be processing a negative length!");

            int end = start + length;

            if (start == end)
                return string.Empty;

            StringBuilder sb = new StringBuilder(1 + length);

            for (int i = start; i < end; i++)
            {
                DisplayCharBase dc = XamMaskedInput.GetDisplayCharAtPosition(sections, i);

                char c = (char)0;
                if (null != dc)
                    c = dc.GetChar(maskMode);

                if (0 == c)
                    continue;

                sb.Append(c);

                // SSP 11/19/01
                // Include comma when necessary
                //
                if (dc is DigitChar && ((DigitChar)dc).ShouldIncludeComma(maskMode))
                    sb.Append(dc.Section.CommaChar);
            }

            return sb.ToString();
        }
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