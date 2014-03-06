using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Security;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Threading;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// A custom RichTextBox used for formula editing.
	/// </summary>

	[DesignTimeVisible(false)]

	public class FormulaEditorTextBox : RichTextBox,
		IUndoManagerOwner<FormulaEditorTextBoxState>
	{
		#region Constants

		private static readonly TimeSpan DisplayMouseOverHelpDelay = TimeSpan.FromSeconds(0.1);

		#endregion // Constants

		#region Member Variables


		private int _changesAreFromTextInput;

		// MD 5/9/12 - TFS99784
		// We need to keep track of the cumulative changed from PreviewTextInput events.
		private string _changesFromTextInput = string.Empty;

		private DispatcherTimer _displayMouseOverHelpTimer;
		private int _ignoreSelectionChangedCount;

		private bool _isChangingRichTextProgrammatically;

		private FormulaEditorBase _owner;
		private Action _resumeSelectionChangedCallBack;
		private TextPointer _positionForMouseOverHelp;
		private bool _syncingFormulaOrTextBox;
		private TextBlock _textMeasurementBlock;
		private UndoManager<FormulaEditorTextBoxState> _undoManager;

		#endregion  // Member Variables

		#region Constructor

		static FormulaEditorTextBox()
		{

			// Disable the input bindings for Space and Shift+Space because we want them to get into the normal TextInput logic.
			// Besides, the input bindings were only registered to modify the undo stack when spaces were inserted, but we are handling
			// undo/redo support anyway.
			CommandManager.RegisterClassInputBinding(typeof(FormulaEditorTextBox), new KeyBinding(ApplicationCommands.NotACommand, Key.Space, ModifierKeys.None));
			CommandManager.RegisterClassInputBinding(typeof(FormulaEditorTextBox), new KeyBinding(ApplicationCommands.NotACommand, Key.Space, ModifierKeys.Shift));

			FormulaEditorTextBox.DefaultStyleKeyProperty.OverrideMetadata(typeof(FormulaEditorTextBox), new FrameworkPropertyMetadata(typeof(FormulaEditorTextBox)));
			EventManager.RegisterClassHandler(typeof(FormulaEditorTextBox), Keyboard.KeyDownEvent, new KeyEventHandler(FormulaEditorTextBox.OnKeyDown));

		}

		/// <summary>
		/// Initializes a new <see cref="FormulaEditorTextBox"/>
		/// </summary>
		public FormulaEditorTextBox()
		{
			this.SelectionChanged += new RoutedEventHandler(this.OnSelectionChanged);





			this.AddHandler(CommandManager.CanExecuteEvent, new CanExecuteRoutedEventHandler(this.OnCanExecuteEvent), true);

			DataObject.AddPastingHandler(this, new DataObjectPastingEventHandler(this.OnPaste));
			TextCompositionManager.AddPreviewTextInputHandler(this, new TextCompositionEventHandler(this.OnPreviewTextInput));


			_resumeSelectionChangedCallBack = this.ResumeSelectionChangedTracking;

			this.SyncTextProperty(false);

			_undoManager = new UndoManager<FormulaEditorTextBoxState>(this);
		}

		#endregion //Constructor

		#region Interfaces

		#region IUndoManagerOwner<FormulaEditorTextBoxState> Members

		void IUndoManagerOwner<FormulaEditorTextBoxState>.SetCanRedo(bool canRedo)
		{
			if (_owner != null)
				_owner.CanRedo = canRedo;
		}

		void IUndoManagerOwner<FormulaEditorTextBoxState>.SetCanUndo(bool canUndo)
		{
			if (_owner != null)
				_owner.CanUndo = canUndo;
		}

		FormulaEditorTextBoxState IUndoManagerOwner<FormulaEditorTextBoxState>.GetCurrentState()
		{
			return new FormulaEditorTextBoxState(this);
		}

		void IUndoManagerOwner<FormulaEditorTextBoxState>.SetCurrentState(FormulaEditorTextBoxState currentState)
		{
			this.Text = currentState.Text;
			TextPointer selectionStart = this.GetCharacterPosition(currentState.SelectionStart);
			TextPointer selectionEnd = this.GetNextCharacterPosition(selectionStart, currentState.SelectionLength);
			this.Selection.Select(selectionStart, selectionEnd);

			this.Focus();
		}

		#endregion

		#endregion  // Interfaces

		#region Base Class Overrides

		#region MeasureOverride

		/// <summary>
		/// Called to re-measure the <see cref="FormulaEditorTextBox"/>
		/// </summary>
		/// <param name="constraint">The constraint of the size.</param>
		/// <returns>The new size of the FormulaEditorTextBox.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			double? singleLineHeight = null;

			int maxLineCount = this.MaxLineCount;
			if (0 < maxLineCount)
			{
				this.GetSingleLineHeight(ref singleLineHeight);

				double maxHeight = singleLineHeight.Value * maxLineCount;
				constraint.Height = Math.Min(maxHeight, constraint.Height);
			}

			Size measuredSize = base.MeasureOverride(constraint);

			int minLineCount = this.MinLineCount;
			if (0 < minLineCount)
			{
				this.GetSingleLineHeight(ref singleLineHeight);

				double minHeight = singleLineHeight.Value * minLineCount;
				measuredSize.Height = Math.Max(minHeight, measuredSize.Height);
			}

			return measuredSize;
		}

		#endregion  // MeasureOverride

		#region OnGotFocus

		/// <summary>
		/// Invoked when the user enters the text box.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);

			// If this is in the standalone editor, re-cache the reference tree incase anything has changed.
			if (_owner is XamFormulaEditor)
				_owner.OnCalculationManagerChanged();
		}

		#endregion  // OnGotFocus

		#region OnKeyDown

		/// <summary>
		/// Invoked when a key is pressed down while the <see cref="FormulaEditorTextBox"/> has focus.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			this.PreprocessKeyDownEvent(e);
			base.OnKeyDown(e);
		}

		#endregion  // OnKeyDown

		#region OnLostFocus

		/// <summary>
		/// Invoked when the user leaves the text box.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);

			ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();
			if (contextualHelpHost != null)
				contextualHelpHost.CloseAllHelp();
		}

		#endregion // OnLostFocus

		#region OnMouseDown


		/// <summary>
		/// Invoked when the mouse is clicked on the text box.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();
			if (contextualHelpHost != null)
				contextualHelpHost.CloseAllHelp();

			base.OnMouseDown(e);
		}


		#endregion // OnMouseDown

		#region OnMouseLeave

		/// <summary>
		/// Invoked when the mouse is moved outside the text box.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			_positionForMouseOverHelp = null;

			ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();
			if (contextualHelpHost != null)
				contextualHelpHost.HideMouseOverHelp();

			base.OnMouseLeave(e);
		}

		#endregion  // OnMouseLeave

		#region OnMouseLeftButtonDown



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


		#endregion // OnMouseLeftButtonDown

		#region OnMouseMove

		/// <summary>
		/// Invoked when the mouse is moved over the text box.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			this.OnMouseMoveInternal(e);

			base.OnMouseMove(e);
		}

		#endregion // OnMouseMove

		#region OnMouseRightButtonDown



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


		#endregion // OnMouseRightButtonDown

		#region OnTextChanged


		/// <summary>
		/// Invoked when a text changes in the text box.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			if (_changesAreFromTextInput > 0 && 
				_isChangingRichTextProgrammatically == false)
			{
				_changesAreFromTextInput--;

				ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();
				TextChange change = e.Changes.LastOrDefault();
				if (contextualHelpHost != null && change != null && change.AddedLength > 0)
				{
					TextPointer startPointer = this.GetContentStartPosition().GetPositionAtOffset(change.Offset);
					TextPointer endPointer = startPointer.GetPositionAtOffset(change.AddedLength);
					TextRange range = new TextRange(startPointer, endPointer);
					string text = range.Text;

					// MD 5/9/12 - TFS99784
					// If the text in the range starts with the text accumulated from the PreviewTextInput events,
					// Only use the text from the events because there may be extra text which we don't care about.
					// For example, when the text is cleared and then a single character is typed, the PreviewTextInput
					// will have just the typed character, but the this event will reflect the character and a newline
					// which is added automatically. We only want to just the single character.
					if (_changesFromTextInput.Length > 0 &&
						text.StartsWith(_changesFromTextInput))
					{
						text = _changesFromTextInput;
						_changesFromTextInput = string.Empty;
					}
					else if (_changesFromTextInput.StartsWith(text))
					{
						// Otherwise, if this event reflects only some of the text type and sent in the PreviewTextInput events,
						// Remove this event's text from the _changesFromTextInput member so subsequent OnTextChanged event
						// can use the rest of the text.
						Debug.Assert(_changesFromTextInput.Length >= text.Length, "This is unexpected.");

						if (_changesFromTextInput.Length >= text.Length)
							_changesFromTextInput = _changesFromTextInput.Substring(text.Length);
					}

					if (String.IsNullOrEmpty(text) == false)
					{
						TextSelection selection = this.Selection;
						int startSelectionIndex = selection.Start.GetCharacterIndex(this.Text);
						int endSelectionIndex = selection.End.GetCharacterIndex(this.Text);
						Debug.Assert(startSelectionIndex == endSelectionIndex, "The selection should be a single position when input occurs by the user");

						startSelectionIndex -= text.Length;
						endSelectionIndex -= text.Length;

						contextualHelpHost.PreprocessTextInput(text, false, startSelectionIndex, endSelectionIndex);
					}
				}

				// MD 5/9/12 - TFS99784
				// If we processed the last change from a PreviewTextInput, clear the cumulative text from those events.
				if (_changesAreFromTextInput == 0)
					_changesFromTextInput = string.Empty;
			}

			base.OnTextChanged(e);
			this.OnRichTextChanged();
		}


		#endregion  // OnTextChanged

		#region OnTextInput



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


		#endregion  // OnTextInput

		#endregion  // Base Class Overrides

		#region Methods

		#region Internal Methods

		// MD 11/4/11
		// Found while fixing TFS95193
		// This is redefined as ReinitializeUndoHistory()
		#region Removed

		//#region ClearUndoHistory
		//
		//internal void ClearUndoHistory()
		//{
		//    _undoManager.ClearHistory();
		//} 
		//
		//#endregion  // ClearUndoHistory

		#endregion  // Removed

		#region ExpandSelectionToFullReference







		internal bool ExpandSelectionToFullReference()
		{
			string text = this.Text;
			int startIndex = this.Selection.Start.GetCharacterIndex(text);

			List<FormulaElement> formulaSegments = FormulaParser.Parse(text);
			FormulaToken startToken = FormulaParser.FindToken(formulaSegments, startIndex) as FormulaToken;
			if (startToken != null && startToken.Type == FormulaTokenType.Reference)
			{
				int endIndex = this.Selection.End.GetCharacterIndex(text);
				FormulaToken endToken = FormulaParser.FindToken(formulaSegments, endIndex - 1) as FormulaToken;
				if (endToken == startToken)
				{
					TextPointer referenceStart = this.GetCharacterPosition(startToken.StartIndex);
					TextPointer referenceEnd = this.GetCharacterPosition(startToken.EndIndex + 1);
					this.Selection.Select(referenceStart, referenceEnd);
					return true;
				}
			}

			return false;
		}

		#endregion  // ExpandSelectionToFullReference

		#region GetCharacterPosition

		internal TextPointer GetCharacterPosition(int characterIndex)
		{
			TextPointer contentStart = this.GetContentStartPosition();

			TextPointer currentPosition = contentStart;
			if (currentPosition.IsAtInsertionPosition == false)
				currentPosition = currentPosition.GetNextInsertionPosition(LogicalDirection.Forward);

			currentPosition = this.GetNextCharacterPosition(currentPosition, characterIndex);
			if (currentPosition != null)
				return currentPosition;

			return this.GetContentEndPosition();
		}

		#endregion  // GetCharacterPosition

		#region GetContentEndPosition

		internal TextPointer GetContentEndPosition()
		{
			return



				this.Document.ContentEnd;

		}

		#endregion  // GetContentEndPosition

		#region GetContentStartPosition

		internal TextPointer GetContentStartPosition()
		{
			return



				this.Document.ContentStart;

		}

		#endregion  // GetContentStartPosition

		#region GetContextualHelpHost

		internal ContextualHelpHost GetContextualHelpHost()
		{
			if (_owner == null)
				return null;

			return _owner.ContextualHelpHost;
		}

		#endregion // GetContextualHelpHost

		#region GetNextCharacterPosition

		internal TextPointer GetNextCharacterPosition(TextPointer startPointer, int offset)
		{
			LogicalDirection direction = LogicalDirection.Forward;
			if (offset < 0)
			{
				offset *= -1;
				direction = LogicalDirection.Backward;
			}

			string text = this.Text;
			int startIndex = startPointer.GetCharacterIndex(text);

			TextPointer nextPointer = startPointer;
			for (int i = 0; i < offset && nextPointer != null; i++)
			{
				if (text != null)
				{
					if (direction == LogicalDirection.Forward)
					{
						int currentIndex = startIndex + i;
						if (currentIndex < text.Length - 1 &&
							text[currentIndex] == '\r' &&
							text[currentIndex + 1] == '\n')
						{
							i++;
						}
					}
					else
					{
						int currentIndex = startIndex - i;
						if (0 < currentIndex &&
							text[currentIndex - 1] == '\r' &&
							text[currentIndex] == '\n')
						{
							i++;
						}
					}
				}

				nextPointer = nextPointer.GetNextInsertionPosition(direction);
			}

			if (nextPointer != null)
				return nextPointer;

			return this.GetContentEndPosition();
		}

		#endregion  // GetNextCharacterPosition

		#region GotoPosition

		internal void GotoPosition(int line, int column)
		{
			string text = this.Text;
			if (text == null)
				return;

			int currentLine = 0;
			int currentColumn = 0;
			int currentPositon = 0;
			for (; currentPositon < text.Length; currentPositon++)
			{
				char ch = text[currentPositon];

				bool isNewLine = false;
				if (ch == '\r')
				{
					isNewLine = true;

					if (currentPositon < text.Length - 1 && text[currentPositon + 1] == '\n')
						currentPositon++;
				}
				else if (ch == '\n')
				{
					isNewLine = true;
				}

				if (isNewLine)
				{
					if (currentLine == line)
						break;

					currentColumn = 0;
					currentLine++;
				}
				else
				{
					if (currentLine == line && currentColumn == column)
						break;

					currentColumn++;
				}
			}

			TextPointer selectionStart = this.GetCharacterPosition(currentPositon);
			this.Selection.Select(selectionStart, selectionStart);

			this.Focus();
		}

		#endregion  // GotoPosition

		#region InsertText

		internal void InsertText(string textToInsert, int selectionStartOffsetFromEnd, int selectionEndOffsetFromEnd)
		{
			_undoManager.Suspend();

			try
			{
				this.ExpandSelectionToFullReference();

				TextSelection selection = this.Selection;

				#region Add Whitespace Around The Text If Needed

				this.SuspendSelectionChangedTracking();

				TextPointer start = selection.Start;
				TextPointer end = selection.End;

				TextPointer beforeStart = start.GetNextInsertionPosition(LogicalDirection.Backward);
				TextPointer afterEnd = end.GetNextInsertionPosition(LogicalDirection.Forward);


				if (beforeStart != null)
				{
					selection.Select(beforeStart, start);
					string text = selection.Text;

					// If there is no whitespace before the current selection, insert a space before the inserted text.
					if (string.IsNullOrEmpty(text) || Char.IsWhiteSpace(text[0]) == false)
						textToInsert = " " + textToInsert;
				}

				// If the text is being inserted at the end, insert a space after the inserted text.
				bool insertWhitespaceAtEnd = false;
				if (afterEnd == null)
				{
					insertWhitespaceAtEnd = true;
				}
				else
				{
					selection.Select(end, afterEnd);
					string text = selection.Text;

					// If there is no whitespace after the current selection, insert a space after the inserted text.
					if (string.IsNullOrEmpty(text) || Char.IsWhiteSpace(text[0]) == false)
						insertWhitespaceAtEnd = true;
				}

				if (insertWhitespaceAtEnd)
				{
					textToInsert += " ";

					if (selectionStartOffsetFromEnd < 0)
						selectionStartOffsetFromEnd--;

					if (selectionEndOffsetFromEnd < 0)
						selectionEndOffsetFromEnd--;
				}

				selection.Select(start, end);

				this.ResumeSelectionChangedTracking();

				#endregion  // Add Whitespace Around The Text If Needed

				this.SetSelectedText(textToInsert);

				TextPointer selectionStart = this.GetNextCharacterPosition(selection.End, selectionStartOffsetFromEnd);
				TextPointer selectionEnd = this.GetNextCharacterPosition(selection.End, selectionEndOffsetFromEnd);
				selection.Select(selectionStart, selectionEnd);

				this.Dispatcher.BeginInvoke(new Func<bool>(this.Focus));
			}
			finally
			{
				_undoManager.Resume();
			}
		}

		#endregion  // InsertText

		#region OnMouseMoveInternal

		internal void OnMouseMoveInternal(MouseEventArgs e)
		{
			ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();
			if (contextualHelpHost != null)
			{
				_positionForMouseOverHelp = this.GetPositionFromPointStrict(e.GetPosition(this));

				if (_positionForMouseOverHelp != null)
				{
					if (contextualHelpHost.IsMouseOverHelpOpen)
					{
						this.UpdateMouseOverHelp();
					}
					else
					{
						this.DisplayMouseOverHelpTimer.Stop();
						this.DisplayMouseOverHelpTimer.Start();
					}
				}
				else
				{
					contextualHelpHost.HideMouseOverHelp();
				}
			}
		}

		#endregion  // OnMouseMoveInternal

		#region RedoInternal

		internal bool RedoInternal()
		{
			return _undoManager.Redo();
		}

		#endregion  // RedoInternal

		// MD 11/4/11
		// Found while fixing TFS95193
		#region ReinitializeUndoHistory

		internal void ReinitializeUndoHistory()
		{
			_undoManager.Reinitialize();
		}

		#endregion  // ReinitializeUndoHistory

		#region ResumeSelectionChangedTracking

		internal void ResumeSelectionChangedTracking()
		{
			_ignoreSelectionChangedCount--;
		}

		#endregion  // ResumeSelectionChangedTracking

		#region SetOwner

		internal void SetOwner(FormulaEditorBase owner)
		{
			_owner = owner;

			// MD 11/3/11 - TFS95198
			// When the owner is set or changes, we should re-initialize the undo manager so the current state of the editor 
			// can be up to date.
			_undoManager.Reinitialize();
		}

		#endregion  // SetOwner

		#region SetSelectedText

		internal void SetSelectedText(string selectedText)
		{

			bool oldIsChangingRichTextProgrammatically = _isChangingRichTextProgrammatically;
			try
			{
				_isChangingRichTextProgrammatically = true;


				this.Selection.Text = selectedText;








			}
			finally
			{
				_isChangingRichTextProgrammatically = oldIsChangingRichTextProgrammatically;
			}

		}

		#endregion  // SetSelectedText

		#region SuspendSelectionChangedTracking

		internal void SuspendSelectionChangedTracking()
		{
			_ignoreSelectionChangedCount++;
		}

		#endregion  // SuspendSelectionChangedTracking

		#region UndoInternal

		internal bool UndoInternal()
		{
			return _undoManager.Undo();
		}

		#endregion  // UndoInternal

		#endregion  // Internal Methods

		#region Private Methods

		#region GetCharacterBounds



#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


		#endregion // GetCharacterBounds

		#region GetPlainText

		private string GetPlainText()
		{
			// MD 12/9/11 - TFS97379
			// Moved all code to GetPlainTextHelper so we can coerce empty strings to null.
			// We need to do this for SL because otherwise, the editors have an item in the undo stack when they initially show and 
			// the formula is not set (because the Text property is initially set to null and then changed to an empty string).
			string plainText = this.GetPlainTextHelper();
			if (plainText == string.Empty)
				return null;

			return plainText;
		}

		private string GetPlainTextHelper()
		{
			AutomationPeer peer =



				ContentElementAutomationPeer.CreatePeerForElement(this.Document);


			if (peer != null)
			{
				ITextProvider provider = peer.GetPattern(PatternInterface.Text) as ITextProvider;

				if (provider != null)
				{
					return provider.DocumentRange.GetText(-1);
				}
			}



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			TextRange range = new TextRange(this.GetContentStartPosition(), this.GetContentEndPosition());
			return range.Text;		

		}

		#endregion  // GetPlainText

		#region GetPositionFromPointStrict

		private TextPointer GetPositionFromPointStrict(Point point)
		{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			return this.GetPositionFromPoint(point, false);

		}

		#endregion // GetPositionFromPointStrict

		#region GetSingleLineHeight

		private void GetSingleLineHeight(ref double? singleLineHeight)
		{
			if (singleLineHeight.HasValue)
				return;

			const double multiplier = 1.05;

			this.TextMeasurementBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

			singleLineHeight = multiplier *



				this.TextMeasurementBlock.DesiredSize.Height;

		}

		#endregion  // GetSingleLineHeight

		#region OnCanExecuteEvent


		private void OnCanExecuteEvent(object sender, CanExecuteRoutedEventArgs e)
		{
			// If we disable the undo events, allow the KeyDown event to fire so we can process them manually if we want.
			if (e.OriginalSource == this && e.Handled == false)
			{
				e.CanExecute = true;
				e.ContinueRouting = true;
			}
		}


		#endregion  // OnCanExecuteEvent

		#region OnContentChanged








		#endregion  // OnContentChanged

		#region OnDisplayMouseOverHelpTimerTick

		private void OnDisplayMouseOverHelpTimerTick(object sender, EventArgs e)
		{
			this.DisplayMouseOverHelpTimer.Stop();
			this.UpdateMouseOverHelp();
		}

		#endregion // OnDisplayMouseOverHelpTimerTick

		#region OnKeyDown


		private static void OnKeyDown(object sender, KeyEventArgs e)
		{
			FormulaEditorTextBox textBox = sender as FormulaEditorTextBox;
			if (textBox != null)
				textBox.PreprocessKeyDownEvent(e);
		}


		#endregion  // OnKeyDown

		#region OnPaste


		private void OnPaste(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(DataFormats.Text, true))
				e.FormatToApply = DataFormats.Text;
		}


		#endregion  // OnPaste

		#region OnPreviewTextInput

		private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			_changesAreFromTextInput++;

			// MD 5/9/12 - TFS99784
			// Keep track of the actual text which was added in the PreviewTextInput.
			_changesFromTextInput += e.Text;

		}

		#endregion  // OnPreviewTextInput

		#region OnRichTextChanged

		private void OnRichTextChanged()
		{
			this.SyncTextProperty(true);

			this.SuspendSelectionChangedTracking();
			this.Dispatcher.BeginInvoke(_resumeSelectionChangedCallBack);
		}

		#endregion  // OnRichTextChanged

		#region OnSelectionChanged

		private void OnSelectionChanged(object sender, RoutedEventArgs e)
		{
			// We will only enter this block when the selection changes as a result of the mouse or arrow keys.
			// If the user types text, the selection will obviously change, but we will not get into this block below.
			if (_ignoreSelectionChangedCount == 0 && this.IsInitializedResolved)
			{
				ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();

				if (contextualHelpHost != null)
					contextualHelpHost.OnContextChanged();








				_undoManager.OnStateChanged(false);
			}
		}

		#endregion  // OnSelectionChanged

		#region PreprocessKeyDownEvent

		private void PreprocessKeyDownEvent(KeyEventArgs e)
		{
			ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();

			Key key = PresentationUtilities.GetKey(e);
			ModifierKeys modifiers = Keyboard.Modifiers;
			
			switch (key)
			{
				#region Disabled Commands

				case Key.B: // Bold (Ctrl+B)
				case Key.E: // Align Center (Ctrl+E)
				case Key.I: // Italics (Ctrl+I)
				case Key.R: // Align Right (Ctrl+R)
				case Key.U: // Underline (Ctrl+U)

				case Key.OemCloseBrackets: // Decrease font size (Ctrl+[)
				case Key.OemOpenBrackets: // Increase font size (Ctrl+])

					if (modifiers == ModifierKeys.Control)
						e.Handled = true;
					break;

				case Key.N: // Toggle Numbering (Ctrl+Shift+L)		
					if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
						e.Handled = true;
					break;

				case Key.L: // Toggle Bullets (Ctrl+Shift+L) or Align Left (Ctrl+L)
				case Key.T: // Decrease Indentation (Ctrl+Shift+T) or Increase Indentation (Ctrl+T)
					if (modifiers == ModifierKeys.Control || modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
						e.Handled = true;
					break;

				#endregion  // Disabled Commands

				// Alternate undo (Alt+Backspace)
				case Key.Back:
					if (modifiers == ModifierKeys.Alt)
					{
						this.UndoInternal();
						e.Handled = true;
					}
					break;

				// Navigate through auto-complete items (Up or Down).
				case Key.Down:
				case Key.Up:
					if (modifiers == ModifierKeys.None)
					{
						if (contextualHelpHost != null)
							e.Handled = contextualHelpHost.NavigateAutoCompleteListByLine(key == Key.Down);
					}
					break;

				// Commit auto-complete item ([Maybe Ctrl+] Enter)
				case Key.Enter:
					if (modifiers == ModifierKeys.None || modifiers == ModifierKeys.Control)
					{
						if (contextualHelpHost != null)
						{
							e.Handled = contextualHelpHost.CommitCurrentAutoCompleteItem();

							if (e.Handled == false && modifiers == ModifierKeys.None)
								contextualHelpHost.PreprocessTextInput(Environment.NewLine);
						}
					}
					break;

				// Hide auto-complete list (Esc)
				case Key.Escape:
					if (contextualHelpHost != null && contextualHelpHost.IsHelpShowing)
					{
						contextualHelpHost.CloseAllHelp();
						e.Handled = true;
					}
					break;

				// Show auto-complete list (Ctrl+J)
				case Key.J:
					if (modifiers == ModifierKeys.Control)
					{
						if (contextualHelpHost != null)
						{
							// Pass True for the preventAutocompletingSingleMatch parameter because Ctrl+J should never auto-complete
							// the text. It should always just show the list.
							contextualHelpHost.ShowAutoCompleteList(true);
						}

						// Always disable this. If we didn't show the list, we don't want the default shortcut to work (Justify).
						e.Handled = true;
					}
					break;

				// Page up or down through the auto-complete list ([Maybe Ctrl+] PageUp or PageDown).
				case Key.PageDown:
				case Key.PageUp:
					if (modifiers == ModifierKeys.None || modifiers == ModifierKeys.Control)
					{
						if (contextualHelpHost != null)
							e.Handled = contextualHelpHost.NavigateAutoCompleteListByPage(key == Key.PageDown);
					}
					break;

				// Show auto-complete list (Alt+Right)
				case Key.Right:
					if (modifiers == ModifierKeys.Alt)
					{
						if (contextualHelpHost != null)
						{
							contextualHelpHost.ShowAutoCompleteList();
							e.Handled = true;
						}
					}
					break;

				// Show auto-complete list (Ctrl+Space)
				// Show function signature help (Ctrl+Shift+Space)
				case Key.Space:
					if (modifiers == ModifierKeys.Control)
					{
						if (contextualHelpHost != null)
						{
							contextualHelpHost.ShowAutoCompleteList();
							e.Handled = true;
						}
					}
					else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
					{
						if (contextualHelpHost != null)
						{
							contextualHelpHost.ShowFunctionSignatureHelp();
							e.Handled = true;
						}
					}
					break;

				// Commit auto-complete item (Tab)
				case Key.Tab:
					if (modifiers == ModifierKeys.None)
					{
						if (contextualHelpHost != null)
							e.Handled = contextualHelpHost.CommitCurrentAutoCompleteItem();
					}
					break;

				// Redo (Ctrl+Y)
				case Key.Y:
					if (modifiers == ModifierKeys.Control)
					{
						this.RedoInternal();
						e.Handled = true;
					}
					break;

				// Undo (Ctrl+Z)
				case Key.Z:
					if (modifiers == ModifierKeys.Control)
					{
						this.UndoInternal();
						e.Handled = true;
					}
					break;
			}
		}

		#endregion  // PreprocessKeyDownEvent

		#region SetPlainText

		private void SetPlainText(string plainText)
		{

			bool oldIsChangingRichTextProgrammatically = _isChangingRichTextProgrammatically;

			try
			{
				_isChangingRichTextProgrammatically = true;


				Paragraph paragraph = new Paragraph();
				BlockCollection blocks;




				blocks = this.Document.Blocks;
				paragraph.Margin = new Thickness(0);


				blocks.Clear();

				paragraph.Inlines.Add(FormulaEditorUtilities.CreateRun(plainText));
				blocks.Add(paragraph);

			}
			finally
			{
				_isChangingRichTextProgrammatically = oldIsChangingRichTextProgrammatically;
			}

		}

		#endregion  // SetPlainText

		#region SyncTextProperty

		private void SyncTextProperty(bool syncToTextProperty)
		{
			if (_syncingFormulaOrTextBox)
				return;

			_syncingFormulaOrTextBox = true;

			try
			{
				if (syncToTextProperty)
					this.Text = this.GetPlainText();
				else
					this.SetPlainText(this.Text);
			}
			finally
			{
				_syncingFormulaOrTextBox = false;
			}
		}

		#endregion  // SyncTextProperty

		#region UpdateMouseOverHelp

		private void UpdateMouseOverHelp()
		{
			if (_positionForMouseOverHelp == null)
				return;

			ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();
			if (contextualHelpHost == null)
				return;

			contextualHelpHost.UpdateMouseOverHelp(_positionForMouseOverHelp.GetCharacterIndex(this.Text));
		}

		#endregion // UpdateMouseOverHelp

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


		#region MaxLineCount

		/// <summary>
		/// Identifies the <see cref="MaxLineCount"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxLineCountProperty = DependencyPropertyUtilities.Register("MaxLineCount",
			typeof(int), typeof(FormulaEditorTextBox),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnMaxLineCountChanged))
			);

		private static void OnMaxLineCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorTextBox instance = (FormulaEditorTextBox)d;
			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Gets or sets the maximum number of lines to display, or zero to display as many lines as possible.
		/// </summary>
		/// <seealso cref="MaxLineCountProperty"/>
		/// <seealso cref="MinLineCount"/>
		public int MaxLineCount
		{
			get
			{
				return (int)this.GetValue(FormulaEditorTextBox.MaxLineCountProperty);
			}
			set
			{
				this.SetValue(FormulaEditorTextBox.MaxLineCountProperty, value);
			}
		}

		#endregion //MaxLineCount

		#region MinLineCount

		/// <summary>
		/// Identifies the <see cref="MinLineCount"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinLineCountProperty = DependencyPropertyUtilities.Register("MinLineCount",
			typeof(int), typeof(FormulaEditorTextBox),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnMinLineCountChanged))
			);

		private static void OnMinLineCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorTextBox instance = (FormulaEditorTextBox)d;
			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Gets or sets the minimum number of lines to display, or zero or less to display a minimum of one line.
		/// </summary>
		/// <seealso cref="MinLineCountProperty"/>
		/// <seealso cref="MaxLineCount"/>
		public int MinLineCount
		{
			get
			{
				return (int)this.GetValue(FormulaEditorTextBox.MinLineCountProperty);
			}
			set
			{
				this.SetValue(FormulaEditorTextBox.MinLineCountProperty, value);
			}
		}

		#endregion //MinLineCount

		#region Text

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyPropertyUtilities.Register("Text",
			typeof(string), typeof(FormulaEditorTextBox),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnTextChanged))
			);

		private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorTextBox instance = (FormulaEditorTextBox)d;
			instance.OnTextChanged();
		}

		private void OnTextChanged()
		{
			_positionForMouseOverHelp = null;

			this.SyncTextProperty(false);

			if (this.IsInitializedResolved)
			{
				_undoManager.OnStateChanged();

				ContextualHelpHost contextualHelpHost = this.GetContextualHelpHost();
				if (contextualHelpHost != null)
					contextualHelpHost.OnContextChanged();
			}
		}

		/// <summary>
		/// Gets or sets the plain text from of the text box.
		/// </summary>
		/// <seealso cref="TextProperty"/>
		public string Text
		{
			get
			{
				return (string)this.GetValue(FormulaEditorTextBox.TextProperty);
			}
			set
			{
				this.SetValue(FormulaEditorTextBox.TextProperty, value);
			}
		}

		#endregion //Text

		#endregion  // Public Properties

		#region Private Properties

		#region DisplayMouseOverHelpTimer

		private DispatcherTimer DisplayMouseOverHelpTimer
		{
			get
			{
				if (_displayMouseOverHelpTimer == null)
				{
					_displayMouseOverHelpTimer = new DispatcherTimer();
					_displayMouseOverHelpTimer.Interval = DisplayMouseOverHelpDelay;
					_displayMouseOverHelpTimer.Tick += this.OnDisplayMouseOverHelpTimerTick;
				}

				return _displayMouseOverHelpTimer;
			}
		}

		#endregion // DisplayMouseOverHelpTimer

		#region IsInitializedResolved

		private bool IsInitializedResolved
		{
			get
			{
				// If the owner is null, the owner's OnApplyTemplate hasn't been called yet, so we are still initializing.
				if (_owner == null)
					return false;


				if (this.IsInitialized == false)
					return false;


				return true;
			}
		}

		#endregion  // IsInitializedResolved

		#region TextMeasurementBlock

		private TextBlock TextMeasurementBlock
		{
			get
			{
				if (_textMeasurementBlock == null)
				{
					_textMeasurementBlock = new TextBlock();

					Binding binding = new Binding("FontFamily");
					binding.Source = this;
					binding.Mode = BindingMode.OneWay;
					BindingOperations.SetBinding(_textMeasurementBlock, TextBlock.FontFamilyProperty, binding);

					binding = new Binding("FontStyle");
					binding.Source = this;
					binding.Mode = BindingMode.OneWay;
					BindingOperations.SetBinding(_textMeasurementBlock, TextBlock.FontStyleProperty, binding);

					binding = new Binding("FontWeight");
					binding.Source = this;
					binding.Mode = BindingMode.OneWay;
					BindingOperations.SetBinding(_textMeasurementBlock, TextBlock.FontWeightProperty, binding);

					binding = new Binding("FontStretch");
					binding.Source = this;
					binding.Mode = BindingMode.OneWay;
					BindingOperations.SetBinding(_textMeasurementBlock, TextBlock.FontStretchProperty, binding);

					binding = new Binding("FontSize");
					binding.Source = this;
					binding.Mode = BindingMode.OneWay;
					BindingOperations.SetBinding(_textMeasurementBlock, TextBlock.FontSizeProperty, binding);

					_textMeasurementBlock.Text = "Wj";
				}

				return _textMeasurementBlock;
			}
		}

		#endregion  // TextMeasurementBlock

		#endregion  // Private Properties

		#endregion  // Properties
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