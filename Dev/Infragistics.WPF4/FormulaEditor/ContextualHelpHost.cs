using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Calculations;
using System.Windows.Documents;
using Infragistics.Windows;
using Infragistics.Calculations.Engine;
using Infragistics.Collections;
using System.Diagnostics;
using System.Windows.Input;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// Control used to host all popups and elements needed for contextual help support.
	/// </summary>
	[TemplatePart(Name = PartAutoCompleteItemHelp, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartAutoCompleteItemHelpPopup, Type = typeof(Popup))]
	[TemplatePart(Name = PartAutoCompleteList, Type = typeof(AutoCompleteList))]
	[TemplatePart(Name = PartAutoCompleteListPopup, Type = typeof(Popup))]
	[TemplatePart(Name = PartFunctionSignatureHelp, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartFunctionSignatureHelpPopup, Type = typeof(Popup))]
	[TemplatePart(Name = PartMouseOverHelp, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartMouseOverHelpPopup, Type = typeof(Popup))]
	[TemplatePart(Name = PartRootCanvas, Type = typeof(Canvas))]
	public class ContextualHelpHost : Control
	{
		#region Constants

		private const string PartAutoCompleteItemHelp = "PART_AutoCompleteItemHelp";
		private const string PartAutoCompleteItemHelpPopup = "PART_AutoCompleteItemHelpPopup";
		private const string PartAutoCompleteList = "PART_AutoCompleteList";
		private const string PartAutoCompleteListPopup = "PART_AutoCompleteListPopup";
		private const string PartFunctionSignatureHelp = "PART_FunctionSignatureHelp";
		private const string PartFunctionSignatureHelpPopup = "PART_FunctionSignatureHelpPopup";
		private const string PartMouseOverHelp = "PART_MouseOverHelp";
		private const string PartMouseOverHelpPopup = "PART_MouseOverHelpPopup";
		private const string PartRootCanvas = "PART_RootCanvas";

		private static readonly TimeSpan DisplayAutoCompleteItemHelpDelay = TimeSpan.FromSeconds(0.75);
		private static readonly TimeSpan HideAutoCompleteItemHelpDelay = TimeSpan.FromSeconds(10);

		#endregion  // Constants

		#region Member Variables

		private List<IFormulaElement> _allItems;
		private ObservableCollectionExtended<IFormulaElement> _autoCompleteItems;
		private TextBlock _autoCompleteItemHelp;
		private Popup _autoCompleteItemPopup;
		private ControlDropDownManager _autoCompleteItemHelpPopupDropDownManager;
		private AutoCompleteList _autoCompleteList;
		private ContentControl _autoCompleteListExclusionArea;
		private Popup _autoCompleteListPopup;
		private ControlDropDownManager _autoCompleteListPopupDropDownManager;
		private FormulaElement _currentAutoCompleteableElementBeingConstructed;
		private DispatcherTimer _delayedShowFunctionSignatureHelpTimer;
		private DispatcherTimer _displayAutoCompleteItemHelpTimer;
		private FormulaEditorBase _editor;
		private TextBlock _functionSignatureHelp;
		private ContentControl _functionSignatureHelpExclusionArea;
		private Rect _functionSignatureHelpExclusionAreaRect;
		private Popup _functionSignatureHelpPopup;
		private ControlDropDownManager _functionSignatureHelpPopupDropDownManager;

		private FrameworkElement _helpPopupRootWithCapture;

		private DispatcherTimer _hideAutoCompleteItemHelpTimer;
		private bool _isAutoCompleteListFiltered;
		private bool _isAutoCompleteListOpeningExplicitly;
		private bool _isFunctionSignatureHelpOpeningExplicitly;
		private TextBlock _mouseOverHelp;
		private ContentControl _mouseOverHelpExclusionArea;
		private Popup _mouseOverHelpPopup;
		private ControlDropDownManager _mouseOverHelpPopupDropDownManager;
		private Canvas _rootCanvas;
		private List<IFormulaElement> _startsWithFilteredItems;

		#endregion  // Member Variables

		#region Constructor

		static ContextualHelpHost()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ContextualHelpHost), new FrameworkPropertyMetadata(typeof(ContextualHelpHost)));

		}

		/// <summary>
		/// Creates a new <see cref="ContextualHelpHost"/> instance.
		/// </summary>
		public ContextualHelpHost()
		{




			_autoCompleteItems = new ObservableCollectionExtended<IFormulaElement>(false, false);

			// MD 12/5/11 - TFS96832
			// The user should not be allowed to tab to the contextual help host.
			this.IsTabStop = false;

			this.Focusable = false;

		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_autoCompleteList != null)
			{
				_autoCompleteList.ItemsSource = null;
				_autoCompleteList.SelectionChanged -= new SelectionChangedEventHandler(this.OnAutoCompleteListSelectionChanged);
			}

			_autoCompleteItemHelp = this.GetTemplateChild(PartAutoCompleteItemHelp) as TextBlock;
			_autoCompleteItemPopup = this.GetTemplateChild(PartAutoCompleteItemHelpPopup) as Popup;
			_autoCompleteList = this.GetTemplateChild(PartAutoCompleteList) as AutoCompleteList;
			_autoCompleteListPopup = this.GetTemplateChild(PartAutoCompleteListPopup) as Popup;
			_functionSignatureHelp = this.GetTemplateChild(PartFunctionSignatureHelp) as TextBlock;
			_functionSignatureHelpPopup = this.GetTemplateChild(PartFunctionSignatureHelpPopup) as Popup;
			_mouseOverHelp = this.GetTemplateChild(PartMouseOverHelp) as TextBlock;
			_mouseOverHelpPopup = this.GetTemplateChild(PartMouseOverHelpPopup) as Popup;
			_rootCanvas = this.GetTemplateChild(PartRootCanvas) as Canvas;

			_autoCompleteListExclusionArea = ContextualHelpHost.CreateExclusionArea(_rootCanvas);
			_functionSignatureHelpExclusionArea = ContextualHelpHost.CreateExclusionArea(_rootCanvas);
			_mouseOverHelpExclusionArea = ContextualHelpHost.CreateExclusionArea(_rootCanvas);

			if (_autoCompleteList != null)
			{
				_autoCompleteList.ItemsSource = _autoCompleteItems;
				_autoCompleteList.SelectionChanged += new SelectionChangedEventHandler(this.OnAutoCompleteListSelectionChanged);
			}

			this.Initialize();
		}

		#endregion  // OnApplyTemplate

		#endregion  // Base Class Overrides

		#region Methods

		#region Internal Methods

		#region CloseAllHelp






		internal void CloseAllHelp()
		{
			this.SetHelpTypes(HelpTypes.None);

			if (_mouseOverHelpPopupDropDownManager != null)
				_mouseOverHelpPopupDropDownManager.IsOpen = false;
		}

		#endregion // CloseAllHelp

		#region CommitCurrentAutoCompleteItem







		internal bool CommitCurrentAutoCompleteItem()
		{
			return this.CommitCurrentAutoCompleteItem(true, _currentAutoCompleteableElementBeingConstructed);
		}

		private bool CommitCurrentAutoCompleteItem(bool canUpdateSelection, 
			FormulaElement autoCompleteableElementBeingConstructed)
		{
			int startSelectionIndex = -1;
			int endSelectionIndex = -1;
			return this.CommitCurrentAutoCompleteItem(canUpdateSelection, 
				autoCompleteableElementBeingConstructed, 
				ref startSelectionIndex, 
				ref endSelectionIndex);
		}

		private bool CommitCurrentAutoCompleteItem(bool canUpdateSelection,
			FormulaElement autoCompleteableElementBeingConstructed,
			ref int startSelectionIndex,
			ref int endSelectionIndex)
		{
			if (this.IsAutoCompleteListOpen == false)
				return false;

			object autoCompleteItem = _autoCompleteList.SelectedItem;
			if (autoCompleteItem == null)
				return false;

			HelpTypes helpTypes = HelpTypes.None;
			if (this.IsFunctionSignatureHelpOpen)
				helpTypes |= HelpTypes.FunctionSignatureHelp;

			this.SetHelpTypes(helpTypes);

			return this.CommitAutoCompleteItem(autoCompleteItem, 
				canUpdateSelection, 
				autoCompleteableElementBeingConstructed, 
				ref startSelectionIndex, 
				ref endSelectionIndex);
		}

		#endregion  // CommitCurrentAutoCompleteItem

		#region HideMouseOverHelp

		internal void HideMouseOverHelp()
		{
			if (_mouseOverHelpPopupDropDownManager != null)
				_mouseOverHelpPopupDropDownManager.IsOpen = false;
		}

		#endregion // HideMouseOverHelp

		#region NavigateAutoCompleteListByLine







		internal bool NavigateAutoCompleteListByLine(bool navigateDown)
		{
			if (this.IsAutoCompleteListOpen == false)
				return false;

			if (_autoCompleteList == null)
				return false;

			_autoCompleteList.NavigateByLine(navigateDown);
			return true;
		}

		#endregion  // NavigateAutoCompleteListByLine

		#region NavigateAutoCompleteListByPage







		internal bool NavigateAutoCompleteListByPage(bool navigateDown)
		{
			if (this.IsAutoCompleteListOpen == false)
				return false;

			if (_autoCompleteList == null)
				return false;

			_autoCompleteList.NavigateByPage(navigateDown);
			return true;
		}

		#endregion  // NavigateAutoCompleteListByPage

		#region OnCalculationManagerChanged






		internal void OnCalculationManagerChanged()
		{
			this.CacheAllElements();
		}

		#endregion  // OnCalculationManagerChanged

		#region OnContextChanged






		public void OnContextChanged()
		{
			// When the text or selection changes, we should hide the mouse over help.
			this.HideMouseOverHelp();

			if (this.GetTextBox() == null)
				return;

			if (this.IsAutoCompleteListOpen || this.IsFunctionSignatureHelpOpen)
				this.UpdateHelpTypes();
		}

		#endregion  // OnContextChanged

		#region OnFormulaProviderReferenceChanged

		internal void OnFormulaProviderReferenceChanged()
		{
			this.CacheAllElements();
		}

		#endregion  // OnFormulaProviderReferenceChanged

		#region PreprocessTextInput






		internal void PreprocessTextInput(string typedText, bool canUpdateSelection = true, int startSelectionIndex = -1, int endSelectionIndex = -1)
		{
			if (this.GetTextBox() == null)
				return;

			if (this.IsAutoCompleteListOpen)
			{
				FormulaElement autoCompleteableElementBeingConstructed = _currentAutoCompleteableElementBeingConstructed;

				bool preventCommittingItemOnListClosed;
				HelpTypes helpType = this.DetermineCurrentHelpTypes(typedText, out preventCommittingItemOnListClosed, startSelectionIndex, endSelectionIndex);

				// If the user typed a character and it doesn't keep the auto-complete list open, commit the current item,
				// and if the commit succeeded, reevaluate the help type again because it may be different now.
				if (preventCommittingItemOnListClosed == false &&
					(helpType & HelpTypes.AutoCompleteList) == 0 &&
					this.CommitCurrentAutoCompleteItem(canUpdateSelection, autoCompleteableElementBeingConstructed, ref startSelectionIndex, ref endSelectionIndex))
				{
					helpType = this.DetermineCurrentHelpTypes(typedText, out preventCommittingItemOnListClosed, startSelectionIndex, endSelectionIndex);
				}

				this.SetHelpTypes(helpType);
			}
			else
			{
				this.UpdateHelpTypes(typedText, startSelectionIndex, endSelectionIndex);
			}
		}

		#endregion  // PreprocessTextInput

		#region SetEditor

		internal void SetEditor(FormulaEditorBase editor)
		{
			_editor = editor;
			this.OnCalculationManagerChanged();
			this.Initialize();
		}

		#endregion  // SetEditor

		#region ShowAutoCompleteList



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal bool ShowAutoCompleteList(bool preventAutocompletingSingleMatch = false)
		{
			try
			{
				_isAutoCompleteListOpeningExplicitly = true;

				bool notUsed;
				this.SetHelpTypes(this.DetermineCurrentHelpTypes(null, out notUsed), preventAutocompletingSingleMatch);

				return this.IsAutoCompleteListOpen;
			}
			finally
			{
				_isAutoCompleteListOpeningExplicitly = false;
			}
		}

		#endregion  // ShowAutoCompleteList

		#region ShowFunctionSignatureHelpDelayed

		internal void ShowFunctionSignatureHelpDelayed(double delay)
		{
			if (this.DelayedShowFunctionSignatureHelpTimer.IsEnabled)
				this.DelayedShowFunctionSignatureHelpTimer.Stop();

			this.DelayedShowFunctionSignatureHelpTimer.Interval = TimeSpan.FromSeconds(delay);
			this.DelayedShowFunctionSignatureHelpTimer.Start();
		}

		#endregion  // ShowFunctionSignatureHelpDelayed

		#region ShowFunctionSignatureHelp

		internal bool ShowFunctionSignatureHelp()
		{
			this.DelayedShowFunctionSignatureHelpTimer.Stop();

			try
			{
				_isFunctionSignatureHelpOpeningExplicitly = true;

				// If the help is already open, return True.
				if (this.IsFunctionSignatureHelpOpen)
					return true;

				this.UpdateHelpTypes();
				return this.IsFunctionSignatureHelpOpen;
			}
			finally
			{
				_isFunctionSignatureHelpOpeningExplicitly = false;
			}
		}

		#endregion  // ShowFunctionSignatureHelp

		#region UpdateMouseOverHelp

		internal bool UpdateMouseOverHelp(int characterIndex)
		{
			FormulaEditorTextBox textBox = this.GetTextBox();
			if (textBox == null)
				return false;

			if (_mouseOverHelpPopupDropDownManager == null)
				return false;

			_mouseOverHelpPopupDropDownManager.IsOpen = this.UpdateMouseOverHelpHelper(characterIndex);
			return _mouseOverHelpPopupDropDownManager.IsOpen;
		}

		private bool UpdateMouseOverHelpHelper(int characterIndex)
		{
			if (_editor == null || _editor.ShowContextualHelp == false)
				return false;

			int insertionIndex;
			List<FormulaElement> formulaSegments = this.ParseCurrentFormula(null, out insertionIndex);
			if (formulaSegments == null)
				return false;

			FormulaElement elementBefore = FormulaParser.FindToken(formulaSegments, characterIndex);
			if (elementBefore == null)
				return false;

			
			
			
			
			

			if (elementBefore.IsLeftParen && elementBefore.Parent.IsFunction)
				return this.InitializeMouseOverHelp(elementBefore.Parent.Children[0]);

			FormulaProduction parent = elementBefore.Parent;
			while (parent != null)
			{
				if (parent.IsFuncId)
					return this.InitializeMouseOverHelp(parent);

				parent = parent.Parent;
			}

			return false;
		}

		#endregion // UpdateMouseOverHelp

		#endregion  // Internal Methods

		#region Private Methods

		#region CacheAllElements

		private void CacheAllElements()
		{
			_allItems = new List<IFormulaElement>();

			if (_editor == null || _editor.CalculationManager == null)
				return;

			this.CacheAllOperands(FormulaEditorUtilities.GetOperands(_editor, _editor.CalculationManager));

			if (_editor.Functions != null)
			{
				foreach (FunctionInfo functionInfo in _editor.Functions)
					_allItems.Add(functionInfo);
			}

			_allItems.Sort();
		}

		#endregion  // CacheAllElements

		#region CacheAllOperands

		private void CacheAllOperands(FilteredCollection<OperandInfo> operands)
		{
			if (operands == null)
				return;

			foreach (OperandInfo operand in operands.AllItems)
			{
				if (operand.IsDataReference &&
					operand.IsEnabled &&
					_allItems.Contains(operand) == false)
				{
					_allItems.Add(operand);
				}

				this.CacheAllOperands(operand.Children);
			}
		}

		#endregion  // CacheAllOperands

		#region CloseAutoCompleteItemHelpPopup

		private void CloseAutoCompleteItemHelpPopup()
		{
			if (_hideAutoCompleteItemHelpTimer != null)
				_hideAutoCompleteItemHelpTimer.Stop();

			if (_autoCompleteItemHelpPopupDropDownManager != null)
				_autoCompleteItemHelpPopupDropDownManager.IsOpen = false;
		}

		#endregion  // CloseAutoCompleteItemHelpPopup

		#region CommitAutoCompleteItem

		private bool CommitAutoCompleteItem(object autoCompleteItem)
		{
			int startSelectionIndex = -1;
			int endSelectionIndex = -1;
			return this.CommitAutoCompleteItem(autoCompleteItem, true, null, ref startSelectionIndex, ref endSelectionIndex);
		}

		private bool CommitAutoCompleteItem(object autoCompleteItem, 
			bool canUpdateSelection, 
			FormulaElement autoCompleteableElementBeingConstructed,
			ref int startSelectionIndex,
			ref int endSelectionIndex)
		{
			if (autoCompleteItem == null)
				return false;

			FormulaEditorTextBox textBox = this.GetTextBox();

			if (textBox == null)
				return false;

			if (autoCompleteableElementBeingConstructed == null)
				autoCompleteableElementBeingConstructed = _currentAutoCompleteableElementBeingConstructed;

			TextSelection selection = textBox.Selection;

			string fullElementName = null;

			FunctionInfo function = autoCompleteItem as FunctionInfo;
			OperandInfo operand = autoCompleteItem as OperandInfo;

			int selectionStartOffset = 0;
			int selectionEndOffset = 0;

			if (function != null)
			{
				fullElementName = function.Name.ToUpper();
			}
			else if (operand != null)
			{
				fullElementName = FormulaEditorUtilities.PrepareToInsertOperand(_editor, operand, out selectionStartOffset, out selectionEndOffset);
			}

			if (fullElementName != null)
			{
				try
				{
					textBox.SuspendSelectionChangedTracking();

					TextPointer selectionStartPosition = null;
					TextPointer selectionEndPosition = null;
					if (canUpdateSelection == false)
					{
						selectionStartPosition = selection.Start;
						selectionEndPosition = selection.End;
					}

					// If there is an element being constructed, select the entire element so we can replace the text with the 
					// full element name. Otherwise, the user just brought up the auto-complete list and didn't type anything 
					// but just selected an item in the list and committed it.
					if (autoCompleteableElementBeingConstructed != null)
					{
						selection.Select(
							textBox.GetCharacterPosition(autoCompleteableElementBeingConstructed.StartIndex),
							textBox.GetCharacterPosition(autoCompleteableElementBeingConstructed.EndIndex + 1));

						int difference = fullElementName.Length - selection.Text.Length;

						if (autoCompleteableElementBeingConstructed.EndIndex < startSelectionIndex)
							startSelectionIndex += difference;

						if (autoCompleteableElementBeingConstructed.EndIndex < endSelectionIndex)
							endSelectionIndex += difference;
					}
					else if (0 <= startSelectionIndex && 0 <= endSelectionIndex)
					{
						selection.Select(
							textBox.GetCharacterPosition(startSelectionIndex),
							textBox.GetCharacterPosition(endSelectionIndex));

						if (startSelectionIndex == endSelectionIndex)
							startSelectionIndex += fullElementName.Length;

						endSelectionIndex += fullElementName.Length;
					}

					textBox.SetSelectedText(fullElementName);

					if (canUpdateSelection)
					{
						selectionStartPosition = textBox.GetNextCharacterPosition(selection.End, selectionStartOffset);
						selectionEndPosition = textBox.GetNextCharacterPosition(selection.End, selectionEndOffset);
					}

					selection.Select(selectionStartPosition, selectionEndPosition);
					return true;
				}
				finally
				{
					textBox.ResumeSelectionChangedTracking();
				}
			}

			Debug.Assert(false, "Something is wrong here.");
			return false;
		}

		#endregion  // CommitAutoCompleteItem

		#region CreateExclusionArea

		private static ContentControl CreateExclusionArea(Canvas rootCanvas)
		{
			if (rootCanvas == null)
				return null;

			ContentControl exclusionArea = new ContentControl();

			// MD 12/5/11 - TFS96832
			// The user should not be allowed to tab to the exclusion areas.
			exclusionArea.IsTabStop = false;

			exclusionArea.Focusable = false;


			exclusionArea.Width = 2;
			exclusionArea.IsHitTestVisible = false;
			rootCanvas.Children.Add(exclusionArea);
			return exclusionArea;
		}

		#endregion  // CreateExclusionArea

		#region CreatePopupChrome


		private Microsoft.Windows.Themes.SystemDropShadowChrome CreatePopupChrome()
		{
			Microsoft.Windows.Themes.SystemDropShadowChrome chrome = null;

			if (null != _autoCompleteListPopup)
			{
				chrome = new Microsoft.Windows.Themes.SystemDropShadowChrome();

				if (_autoCompleteListPopup.HasDropShadow)
				{
					chrome.Margin = new Thickness(0, 0, 5, 5);
					chrome.Color = Color.FromArgb(113, 0, 0, 0);
				}
				else
				{
					chrome.Color = Colors.Transparent;
				}
			}
			return chrome;
		}


		#endregion  // CreatePopupChrome

		#region DetermineCurrentHelpTypes



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		private HelpTypes DetermineCurrentHelpTypes(string typedText, 
			out bool preventCommittingItemOnListClosed, 
			int startSelectionIndex = -1, 
			int endSelectionIndex = -1)
		{
			preventCommittingItemOnListClosed = false;

			if (_editor == null || _editor.ShowContextualHelp == false)
				return HelpTypes.None;

			int insertionIndex;
			List<FormulaElement> formulaSegments = this.ParseCurrentFormula(typedText, out insertionIndex, startSelectionIndex, endSelectionIndex);
			if (formulaSegments == null)
				return HelpTypes.None;

			FormulaElement previousAutoCompleteableElementBeingConstructed = _currentAutoCompleteableElementBeingConstructed;

			// Reset the current auto-complete-able element help is being shown.
			_currentAutoCompleteableElementBeingConstructed = null;

			// First check the token before the insertion point and try to show help based on that item.
			FormulaElement elementBefore = FormulaParser.FindToken(formulaSegments, insertionIndex - 1);
			FormulaElement elementAfter = FormulaParser.FindToken(formulaSegments, insertionIndex);

			HelpTypes contextualHelpTypes = HelpTypes.None;

			// If an open parenthesis or a comma was typed, we should show the function construction help.
			if (elementBefore != null && typedText != null &&
				(elementBefore.IsArgSep || elementBefore.IsLeftParen))
			{
				if (this.InitializeFunctionSignatureHelp(elementBefore, true))
				{
					contextualHelpTypes |= HelpTypes.FunctionSignatureHelp;
				}
			}

			// If the function construction help was already open, leave it open if we are still within a function argument list.
			if ((contextualHelpTypes & HelpTypes.FunctionSignatureHelp) == 0 &&
				(this.IsFunctionSignatureHelpOpen || _isFunctionSignatureHelpOpeningExplicitly))
			{
				if (this.InitializeFunctionSignatureHelp(elementBefore, true) ||
					this.InitializeFunctionSignatureHelp(elementAfter, false))
				{
					contextualHelpTypes |= HelpTypes.FunctionSignatureHelp;
				}
			}

			FormulaElement autoCompleteableElement = this.GetAutoCompleteableElement(elementBefore) ?? this.GetAutoCompleteableElement(elementAfter);

			if (this.IsAutoCompleteListOpen || _isAutoCompleteListOpeningExplicitly || typedText != null)
			{
				bool shouldShowAutoCompleteList;
				if (autoCompleteableElement != null)
				{
					shouldShowAutoCompleteList = true;

					if (this.IsAutoCompleteListOpen &&
						previousAutoCompleteableElementBeingConstructed != null)
					{
						// If the user had the list open and arrowed into another function or operand, don't show the list anymore.
						if (previousAutoCompleteableElementBeingConstructed.StartIndex != autoCompleteableElement.StartIndex)
						{
							shouldShowAutoCompleteList = false;
						}
						else if (typedText != null)
						{
							FormulaToken previousAutoCompleteableToken = previousAutoCompleteableElementBeingConstructed as FormulaToken;
							FormulaToken currentAutoCompleteableToken = autoCompleteableElement as FormulaToken;

							// If the user completed the reference manually, hide the list and stop the list from auto-completing the currently
							// selected item.
							if (previousAutoCompleteableToken != null &&
								previousAutoCompleteableToken.Type == FormulaTokenType.ErroneousReference &&
								currentAutoCompleteableToken != null &&
								currentAutoCompleteableToken.Type == FormulaTokenType.Reference)
							{
								shouldShowAutoCompleteList = false;
								preventCommittingItemOnListClosed = true;
							}
						}
					}
				}
				else
				{
					if (_isAutoCompleteListOpeningExplicitly)
					{
						shouldShowAutoCompleteList = true;

						FormulaToken tokenBefore = elementBefore as FormulaToken;
						FormulaToken tokenAfter = elementAfter as FormulaToken;
						if (tokenBefore != null)
						{
							if (tokenAfter == null)
							{
								// If a string was opened, but never closed and we are currently at the end of it, don't show the list.
								if (tokenBefore.Type == FormulaTokenType.ErroneousQuotedString)
									shouldShowAutoCompleteList = false;
							}
							else if (tokenBefore == tokenAfter)
							{
								// If the cursor is inside any of the multi-character tokens which cannot occur in a function name or reference name,
								// then don't show the auto-complete list.
								switch (tokenBefore.Type)
								{
									case FormulaTokenType.OpGe:
									case FormulaTokenType.OpLe:
									case FormulaTokenType.OpNe:
									case FormulaTokenType.OpAltNe:
									case FormulaTokenType.RangeSep:
									case FormulaTokenType.QuotedString:
									case FormulaTokenType.ErroneousQuotedString:
										shouldShowAutoCompleteList = false;
										break;
								}
							}
						}
					}
					else if (typedText == null &&
						this.IsAutoCompleteListOpen &&
						previousAutoCompleteableElementBeingConstructed != null &&
						previousAutoCompleteableElementBeingConstructed.StartIndex == insertionIndex)
					{
						// If there is no valid auto-complete-able item at the insertion index, but the list was open and the 
						// insertion index is at the start of the old element, the user probably backspaced and deleted the first 
						// character of the function name, in which case we want to leave the list open.
						shouldShowAutoCompleteList = true;
					}
					else
					{
						shouldShowAutoCompleteList = false;
					}
				}

				if (shouldShowAutoCompleteList && this.InitializeAutoCompleteList(typedText, autoCompleteableElement))
					contextualHelpTypes |= HelpTypes.AutoCompleteList;
			}

			return contextualHelpTypes;
		}

		#endregion  // DetermineCurrentHelpTypes

		#region GetAutoCompleteableElement

		private FormulaElement GetAutoCompleteableElement(FormulaElement element)
		{
			if (element == null)
				return null;

			if (element.IsReference)
				return element;

			// Walk up the parent chain to see if we are in a FuncId element.
			FormulaProduction parent = element as FormulaProduction;
			if (parent == null)
				parent = element.Parent;

			while (parent != null)
			{
				if (parent.Type == FormulaProductionType.FuncId)
					return parent;

				if (parent.Type == FormulaProductionType.Constant)
				{
					// If the auto-complete list was opened previously and the user types a number instead of an alpha character 
					// as the first character, we should leave the list open.
					if (this.IsAutoCompleteListOpen || _isAutoCompleteListOpeningExplicitly)
					{
						return parent;
					}
				}

				parent = parent.Parent;
			}

			return null;
		}

		#endregion  // GetAutoCompleteableElement

		#region GetElementFilterText






		private static string GetElementFilterText(IFormulaElement element)
		{
			FunctionInfo functionInfo = element as FunctionInfo;
			if (functionInfo != null)
				return functionInfo.Name;

			OperandInfo operandInfo = element as OperandInfo;
			if (operandInfo != null)
				return operandInfo.Signature;

			Debug.Assert(false, "Unknown element type.");
			return string.Empty;
		}

		#endregion  // GetElementFilterText

		#region GetTextBox

		private FormulaEditorTextBox GetTextBox()
		{
			if (_editor == null)
				return null;

			return _editor.TextBox;
		}

		#endregion // GetTextBox

		#region HookHelpPopupMouseMessages


		private void HookHelpPopupMouseMessages()
		{
			FrameworkElement capturedElement = Mouse.Captured as FrameworkElement;
			if (capturedElement == _helpPopupRootWithCapture)
				return;

			this.UnhookHelpPopupMouseMessages();

			if (capturedElement == null)
				return;

			bool isHelpPopupRoot = false;

			if (_autoCompleteListPopupDropDownManager != null &&
				capturedElement.IsAncestorOf(_autoCompleteListPopupDropDownManager.OriginalPopupChild))
			{
				isHelpPopupRoot = true;
			}
			else if (_functionSignatureHelpPopupDropDownManager != null &&
				capturedElement.IsAncestorOf(_functionSignatureHelpPopupDropDownManager.OriginalPopupChild))
			{
				isHelpPopupRoot = true;
			}

			if (isHelpPopupRoot == false)
				return;

			Mouse.AddLostMouseCaptureHandler(capturedElement, new MouseEventHandler(this.OnHelpPopupRootLostMouseCapture));
			capturedElement.MouseMove += new MouseEventHandler(this.OnHelpPopupRootMouseMove);
			_helpPopupRootWithCapture = capturedElement;
		}


		#endregion  // HookHelpPopupMouseMessages

		#region Initialize

		private void Initialize()
		{





			this.InitializePopup(_autoCompleteItemPopup, _autoCompleteList, DropDownPlacementMode.Right, true, ref _autoCompleteItemHelpPopupDropDownManager);
			this.InitializePopup(_autoCompleteListPopup, _autoCompleteListExclusionArea, DropDownPlacementMode.Bottom, false, ref _autoCompleteListPopupDropDownManager, this.OnAutoCompleteListPopupOpened, this.OnAutoCompleteListPopupClosed);
			this.InitializePopup(_functionSignatureHelpPopup, _functionSignatureHelpExclusionArea, DropDownPlacementMode.Bottom, false, ref _functionSignatureHelpPopupDropDownManager, null, this.OnFunctionSignatureHelpClosed);
			this.InitializePopup(_mouseOverHelpPopup, _mouseOverHelpExclusionArea, DropDownPlacementMode.Bottom, true, ref _mouseOverHelpPopupDropDownManager);





		}

		#endregion  // Initialize

		#region InitializeAutoCompleteList

		private bool InitializeAutoCompleteList(string typedText, FormulaElement autoCompleteableElement)
		{
			if (_autoCompleteListPopupDropDownManager == null)
				return false;

			FormulaEditorTextBox textBox = this.GetTextBox();
			if (textBox == null)
				return false;

			string currentElementName = autoCompleteableElement == null ? string.Empty : autoCompleteableElement.GetText();

			// If the list isn't open, reset the _isListFiltered flag.
			if (this.IsAutoCompleteListOpen == false)
			{
				_isAutoCompleteListFiltered = false;

				// If part of a function was already present and the user just added text to it, don't show the list. 
				// But if the function just started being typed by the user, the list can show and we should filter it also.
				if (typedText != null)
				{
					if (currentElementName != typedText)
						return false;

					_isAutoCompleteListFiltered = true;
				}
			}

			List<IFormulaElement> itemsInList = new List<IFormulaElement>();
			_startsWithFilteredItems = new List<IFormulaElement>();

			// Filter the items using a 'contains' filter and also try to figure out the best guess for the items
			// the user is trying to type by using the first function that starts with the typed text.
			foreach (IFormulaElement item in _allItems)
			{
				string filterText = ContextualHelpHost.GetElementFilterText(item);
				int index = filterText.IndexOf(currentElementName, StringComparison.CurrentCultureIgnoreCase);

				if (index < 0 && _isAutoCompleteListFiltered)
					continue;

				itemsInList.Add(item);

				if (index == 0)
					_startsWithFilteredItems.Add(item);
			}

			if (itemsInList.Count == 0)
				itemsInList.AddRange(_allItems);

			this.ReplaceAutoCompleteItems(itemsInList);

			// Try to set the selected item in the list based on our best guess.
			if (_autoCompleteList != null && _autoCompleteItems.Count != 0)
			{
				if (_startsWithFilteredItems.Count != 0)
				{
					_autoCompleteList.SelectedItem = _startsWithFilteredItems[0];
				}
				else
				{
					// If there was no best guess and the previously selected item is still in the newly filtered list, leave it selected.
					// Otherwise, select the first item in the list.
					if (_autoCompleteList.SelectedItem == null)
						_autoCompleteList.SelectedItem = _autoCompleteItems[0];
				}
			}

			_currentAutoCompleteableElementBeingConstructed = autoCompleteableElement;
			if (_autoCompleteItems.Count == 0)
				return false;

			TextPointer positionOfFunctionName;
			if (autoCompleteableElement != null)
				positionOfFunctionName = textBox.GetCharacterPosition(autoCompleteableElement.StartIndex);
			else
				positionOfFunctionName = textBox.Selection.Start;

			Rect characterRect = positionOfFunctionName.GetCharacterRect(LogicalDirection.Forward);
			if (Double.IsInfinity(characterRect.Bottom) || Double.IsInfinity(characterRect.Left))
			{
				Debug.Assert(false, "We should have gotten the character bounds here.");
				return false;
			}

			GeneralTransform textBoxToContextualHelpHostTransform = textBox.TransformToVisual(this);
			Rect characterRectInContextualHelpHost = textBoxToContextualHelpHostTransform.TransformBounds(characterRect);

			if (_autoCompleteListExclusionArea == null)
			{
				_autoCompleteListPopupDropDownManager.HorizontalOffset = characterRectInContextualHelpHost.X;
				_autoCompleteListPopupDropDownManager.VerticalOffset = characterRectInContextualHelpHost.Bottom;
			}
			else
			{
				Rect overallRect = characterRectInContextualHelpHost;
				if (_editor != null &&
					_editor.TextBox != null &&
					_functionSignatureHelpExclusionArea != null &&
					_functionSignatureHelpPopupDropDownManager != null &&
					_functionSignatureHelpPopupDropDownManager.IsOpen)
				{
					FrameworkElement functionSignatureHelpContents = _functionSignatureHelpPopupDropDownManager.OriginalPopupChild;

					if (functionSignatureHelpContents != null)
					{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

						Point topLeftPoint = functionSignatureHelpContents.TranslatePoint(new Point(), this);
						Point bottomRightPoint = functionSignatureHelpContents.TranslatePoint(
							new Point(functionSignatureHelpContents.ActualWidth, functionSignatureHelpContents.ActualHeight),
							this);
						Rect functionSignatureHelpContentsRect = Infragistics.Windows.Utilities.RectFromPoints(topLeftPoint, bottomRightPoint);


						overallRect.Union(functionSignatureHelpContentsRect);
					}
				}

				Canvas.SetLeft(_autoCompleteListExclusionArea, characterRectInContextualHelpHost.Left);
				Canvas.SetTop(_autoCompleteListExclusionArea, overallRect.Top);
				_autoCompleteListExclusionArea.Height = overallRect.Height;
			}

			// If the auto-complete list is repositioned and the item description is already open, reposition the item description 
			// so it stays next to the selected item.
			if (_autoCompleteItemHelpPopupDropDownManager != null &&
				_autoCompleteItemHelpPopupDropDownManager.IsOpen)
			{
				object selectedItem = _autoCompleteList.SelectedItem;
				if (selectedItem != null)
				{
					ListBoxItem itemElement = _autoCompleteList.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;
					if (itemElement != null)
						this.PositionAutoCompleteItemHelpPopup(itemElement);
				}
			}

			return true;
		}

		#endregion  // InitializeAutoCompleteList

		#region InitializeFunctionDescription



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private bool InitializeFunctionDescription(TextBlock textBlock, CalculationFunction function, int argumentCount = -1)
		{
			if (textBlock == null || function == null)
				return false;

			textBlock.Inlines.Clear();
			textBlock.Inlines.Add(FormulaEditorUtilities.CreateRun(function.Name.ToUpper()));
			textBlock.Inlines.Add(FormulaEditorUtilities.CreateRun(FormulaEditorUtilities.GetFunctionSignature(function, argumentCount)));
			textBlock.Inlines.Add(new LineBreak());
			textBlock.Inlines.Add(FormulaEditorUtilities.CreateRun(function.Description));
			return true;
		}

		#endregion  // InitializeFunctionDescription

		#region InitializeFunctionSignatureHelp

		private bool InitializeFunctionSignatureHelp(FormulaElement element, bool isTokenBeforeInsertionIndex)
		{
			if (element == null || _editor == null || _editor.Functions == null || _functionSignatureHelpPopupDropDownManager == null)
				return false;

			FormulaEditorTextBox textBox = this.GetTextBox();
			if (textBox == null)
				return false;

			List<FormulaElement> ancestorChainUnderFunction;
			FormulaProduction functionProduction = FormulaEditorUtilities.GetOwningFunctionProduction(
				element,
				isTokenBeforeInsertionIndex,
				out ancestorChainUnderFunction);

			string functionName = FormulaEditorUtilities.GetFuncId(functionProduction);
			if (String.IsNullOrEmpty(functionName))
				return false;

			FunctionInfo functionInfo = FormulaEditorUtilities.GetFunctionInfo(_editor.Functions, functionName);
			if (functionInfo == null)
				return false;

			CalculationFunction function = functionInfo.Function;

			int argumentCount;
			int currentArgumentIndex = FormulaEditorUtilities.GetArgumentIndex(functionProduction, element, ancestorChainUnderFunction, out argumentCount);

			if (this.InitializeFunctionSignatureHelp(function, argumentCount, currentArgumentIndex) == false)
				return false;

			TextPointer start = textBox.GetCharacterPosition(functionProduction.StartIndex);
			TextPointer end = textBox.GetCharacterPosition(functionProduction.EndIndex);
			Rect startCharacterRect = start.GetCharacterRect(LogicalDirection.Forward);
			Rect endCharacterRect = end.GetCharacterRect(LogicalDirection.Backward);

			if (Double.IsInfinity(startCharacterRect.Left) || Double.IsInfinity(endCharacterRect.Left))
			{
				// The character positions may be updated asynchronously, so if they are not valid, leave the function construction help window 
				// in the state it was in and asynchronously update the help being displayed so we can get the valid positions next time. 
				textBox.Dispatcher.BeginInvoke(new Action(() => this.UpdateHelpTypes()));
				return this.IsFunctionSignatureHelpOpen;
			}

			GeneralTransform textBoxToContextualHelpHostTransform = textBox.TransformToVisual(this);
			Rect startCharacterRectInContextualHelpHost = textBoxToContextualHelpHostTransform.TransformBounds(startCharacterRect);
			Rect endCharacterRectInContextualHelpHost = textBoxToContextualHelpHostTransform.TransformBounds(endCharacterRect);

			if (_functionSignatureHelpExclusionArea == null)
			{
				_functionSignatureHelpPopupDropDownManager.HorizontalOffset = startCharacterRectInContextualHelpHost.Left;
				_functionSignatureHelpPopupDropDownManager.VerticalOffset = endCharacterRectInContextualHelpHost.Bottom;
			}
			else
			{
				Canvas.SetLeft(_functionSignatureHelpExclusionArea, startCharacterRectInContextualHelpHost.Left);
				Canvas.SetTop(_functionSignatureHelpExclusionArea, startCharacterRectInContextualHelpHost.Top);
				_functionSignatureHelpExclusionArea.Height = endCharacterRectInContextualHelpHost.Bottom - startCharacterRectInContextualHelpHost.Top;
				_functionSignatureHelpExclusionArea.UpdateLayout();

				Rect newRect = new Rect(
					startCharacterRectInContextualHelpHost.Left, 
					startCharacterRectInContextualHelpHost.Top, 
					2, 
					endCharacterRectInContextualHelpHost.Bottom - startCharacterRectInContextualHelpHost.Top);

				// If the popup is already open and the exclusion rectangle top or bottom has changed, we need to hide and show the popup 
				// again so it will be positioned correctly.
				if (_functionSignatureHelpPopupDropDownManager.IsOpen)
				{
					if (CoreUtilities.AreClose(newRect.Top, _functionSignatureHelpExclusionAreaRect.Top) == false ||
						CoreUtilities.AreClose(newRect.Bottom, _functionSignatureHelpExclusionAreaRect.Bottom) == false ||
						CoreUtilities.AreClose(newRect.Left, _functionSignatureHelpExclusionAreaRect.Left) == false)
					{
						_functionSignatureHelpPopupDropDownManager.IsOpen = false;
						_functionSignatureHelpPopupDropDownManager.IsOpen = true;
					}
				}

				_functionSignatureHelpExclusionAreaRect = newRect;
			}

			return true;
		}



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		private bool InitializeFunctionSignatureHelp(CalculationFunction function, int argumentCount, int currentArgumentIndex)
		{
			if (_functionSignatureHelp == null)
				return false;

			_functionSignatureHelp.Inlines.Clear();
			_functionSignatureHelp.Inlines.Add(FormulaEditorUtilities.CreateRun(function.Name.ToUpper()));

			Span argumentDescription;
			List<Inline> signatureInlines = FormulaEditorUtilities.GetFunctionSignature(function, argumentCount, currentArgumentIndex, out argumentDescription);
			for (int i = 0; i < signatureInlines.Count; i++)
				_functionSignatureHelp.Inlines.Add(signatureInlines[i]);

			_functionSignatureHelp.Inlines.Add(new LineBreak());
			_functionSignatureHelp.Inlines.Add(FormulaEditorUtilities.CreateRun(function.Description));

			if (argumentDescription != null)
			{
				_functionSignatureHelp.Inlines.Add(new LineBreak());
				_functionSignatureHelp.Inlines.Add(argumentDescription);
			}

			return true;
		}

		#endregion // InitializeFunctionSignatureHelp

		#region InitializeMouseOverHelp

		private bool InitializeMouseOverHelp(FormulaElement funcIdElement)
		{
			if (_editor == null || _editor.Functions == null || _functionSignatureHelpPopupDropDownManager == null)
				return false;

			FormulaEditorTextBox textBox = this.GetTextBox();
			if (textBox == null)
				return false;

			FunctionInfo functionInfo = FormulaEditorUtilities.GetFunctionInfo(_editor.Functions, funcIdElement.GetText());
			if (functionInfo == null)
				return false;

			int argumentCount;
			FormulaEditorUtilities.GetArgumentIndex(funcIdElement.Parent, funcIdElement, new List<FormulaElement>(), out argumentCount);

			if (this.InitializeFunctionDescription(_mouseOverHelp, functionInfo.Function, argumentCount) == false)
				return false;

			TextPointer start = textBox.GetCharacterPosition(funcIdElement.StartIndex);
			Rect startCharacterRect = start.GetCharacterRect(LogicalDirection.Forward);

			GeneralTransform textBoxToContextualHelpHostTransform = textBox.TransformToVisual(this);
			Rect startCharacterRectInContextualHelpHost = textBoxToContextualHelpHostTransform.TransformBounds(startCharacterRect);

			if (_functionSignatureHelpExclusionArea == null)
			{
				_mouseOverHelpPopupDropDownManager.HorizontalOffset = startCharacterRectInContextualHelpHost.Left;
				_mouseOverHelpPopupDropDownManager.VerticalOffset = startCharacterRectInContextualHelpHost.Bottom;
			}
			else
			{
				Canvas.SetLeft(_mouseOverHelpExclusionArea, startCharacterRectInContextualHelpHost.Left);
				Canvas.SetTop(_mouseOverHelpExclusionArea, startCharacterRectInContextualHelpHost.Top);
				_mouseOverHelpExclusionArea.Height = startCharacterRectInContextualHelpHost.Height;
			}

			return true;
		}

		#endregion  // InitializeMouseOverHelp

		#region InitializePopup

		private void InitializePopup(
			Popup popup,
			Control owningControl,
			DropDownPlacementMode placement,
			bool staysOpen,
			ref ControlDropDownManager dropDownManager,
			Action openAction = null,
			Action closeAction = null)
		{
			if (dropDownManager != null)
			{
				dropDownManager.Dispose();
				dropDownManager = null;
			}

			if (_editor == null || popup == null)
				return;

			if (owningControl == null)
			{
				if (placement == DropDownPlacementMode.Bottom)
				{
					owningControl = _editor.TextBox;
					placement = DropDownPlacementMode.Relative;
				}

				if (owningControl == null)
					return;
			}


			popup.PopupAnimation = PopupAnimation.None;
			popup.PlacementTarget = owningControl;


			dropDownManager = new ControlDropDownManager(owningControl, popup, null, staysOpen, openAction, closeAction

				, this.CreatePopupChrome()

			);






			dropDownManager.Placement = placement;
		}

		#endregion  // InitializePopup

		#region OnAutoCompleteListPopupClosed

		private void OnAutoCompleteListPopupClosed()
		{
			// When the auto-complete list closes, so should the help describing the select item.
			this.CloseAutoCompleteItemHelpPopup();

			// If the auto-complete list is closed and the function signature help is still open, set it's StaysOpen value
			// back to False so it closes when the user clicks outside the popup.
			if (_functionSignatureHelpPopupDropDownManager != null && _functionSignatureHelpPopupDropDownManager.IsOpen)
				_functionSignatureHelpPopupDropDownManager.StaysOpen = false;
		}

		#endregion //OnAutoCompleteListPopupClosed

		#region OnAutoCompleteListPopupClosingFromOutsideClick



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


		#endregion  // OnAutoCompleteListPopupClosingFromOutsideClick

		#region OnAutoCompleteListPopupOpened

		private void OnAutoCompleteListPopupOpened()
		{
			this.DisplayAutoCompleteItemHelpTimer.Start();
		}

		#endregion //OnAutoCompleteListPopupOpened

		#region OnAutoCompleteListSelectionChanged

		private void OnAutoCompleteListSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// When the user selects a new auto-complete item, close the help describing the select item and reopen 
			// it after a delay.
			this.CloseAutoCompleteItemHelpPopup();

			this.DisplayAutoCompleteItemHelpTimer.Stop();
			this.DisplayAutoCompleteItemHelpTimer.Start();
		}

		#endregion  // OnAutoCompleteListSelectionChanged

		#region OnDelayedShowFunctionSignatureHelpTimerTick

		private void OnDelayedShowFunctionSignatureHelpTimerTick(object sender, EventArgs e)
		{
			this.ShowFunctionSignatureHelp();
		}

		#endregion  // OnDelayedShowFunctionSignatureHelpTimerTick

		#region OnDisplayAutoCompleteItemHelpTimerTick

		private void OnDisplayAutoCompleteItemHelpTimerTick(object sender, EventArgs e)
		{
			this.DisplayAutoCompleteItemHelpTimer.Stop();

			if (_autoCompleteList == null ||
				_autoCompleteListPopupDropDownManager == null ||
				_autoCompleteListPopupDropDownManager.IsOpen == false ||
				_autoCompleteItemHelpPopupDropDownManager == null)
				return;

			object selectedItem = _autoCompleteList.SelectedItem;
			if (selectedItem == null)
				return;

			ListBoxItem itemElement = _autoCompleteList.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;
			if (itemElement == null)
				return;

			FunctionInfo functionInfo = selectedItem as FunctionInfo;
			if (functionInfo == null)
				return;

			if (this.PositionAutoCompleteItemHelpPopup(itemElement) == false)
				return;

			if (this.InitializeFunctionDescription(_autoCompleteItemHelp, functionInfo.Function) == false)
				return;

			this.HideAutoCompleteItemHelpTimer.Start();
		}

		#endregion  // OnDisplayAutoCompleteItemHelpTimerTick

		#region OnHelpPopupRootLostMouseCapture


		private void OnHelpPopupRootLostMouseCapture(object sender, MouseEventArgs e)
		{
			Debug.Assert(_helpPopupRootWithCapture == sender, "There is something wrong here.");
			this.HookHelpPopupMouseMessages();
		}


		#endregion  // OnHelpPopupRootLostMouseCapture

		#region OnHelpPopupRootMouseMove


		private void OnHelpPopupRootMouseMove(object sender, MouseEventArgs e)
		{
			Debug.Assert(_helpPopupRootWithCapture == sender, "There is something wrong here.");

			FormulaEditorTextBox textBox = this.GetTextBox();
			if (textBox != null)
				textBox.OnMouseMoveInternal(e);
		}


		#endregion  // OnHelpPopupRootMouseMove

		#region OnHideAutoCompleteItemHelpTimerTick

		private void OnHideAutoCompleteItemHelpTimerTick(object sender, EventArgs e)
		{
			this.CloseAutoCompleteItemHelpPopup();
		}

		#endregion  // OnHideAutoCompleteItemHelpTimerTick

		#region OnFunctionSignatureHelpClosed

		private void OnFunctionSignatureHelpClosed()
		{
			// If the function construction help opens and then the auto-complete list opens as well, the auto-complete list won't capture 
			// the mouse to determine when to close, so if anything causes the function construction help to close, we should also close
			// the auto-complete list. There is no situation when the function construction help should close but the auto-complete list 
			// should stay open.
			if (_autoCompleteListPopupDropDownManager != null)
				_autoCompleteListPopupDropDownManager.IsOpen = false;
		}

		#endregion //OnFunctionSignatureHelpClosed

		#region ParseCurrentFormula

		private List<FormulaElement> ParseCurrentFormula(string typedText, out int insertionIndex, int startSelectionIndex = -1, int endSelectionIndex = -1)
		{
			insertionIndex = -1;

			FormulaEditorTextBox textBox = this.GetTextBox();
			if (textBox == null)
				return null;

			string formula = textBox.Text ?? string.Empty;

			// If the user typed text, combine it with the current text to see what the text will be after the typed text is committed.
			int endIndex = 0 <= endSelectionIndex 
				? endSelectionIndex 
				: textBox.Selection.End.GetCharacterIndex(formula);
			endIndex = Math.Min(endIndex, formula.Length);

			if (typedText != null)
			{
				int startIndex = 0 <= startSelectionIndex 
					? startSelectionIndex 
					: textBox.Selection.Start.GetCharacterIndex(formula);
				startIndex = Math.Min(startIndex, formula.Length);

				formula = formula.Substring(0, startIndex) + typedText + formula.Substring(endIndex);
				insertionIndex = startIndex + typedText.Length;
			}
			else
			{
				insertionIndex = endIndex;
			}

			return FormulaParser.Parse(formula);
		}

		#endregion // ParseCurrentFormula

		#region PositionAutoCompleteItemHelpPopup

		private bool PositionAutoCompleteItemHelpPopup(ListBoxItem autoCompleteItemElement)
		{
			Point itemTopLeft = new Point();
			Point itemBottomRight = new Point(autoCompleteItemElement.DesiredSize.Width, autoCompleteItemElement.DesiredSize.Height);






			Point itemTopLeftInListCoordinates = autoCompleteItemElement.TranslatePoint(itemTopLeft, _autoCompleteList);
			Point itemBottomRightInListCoordinates = autoCompleteItemElement.TranslatePoint(itemBottomRight, _autoCompleteList);


			if (0 <= itemBottomRightInListCoordinates.Y && itemTopLeftInListCoordinates.Y <= _autoCompleteList.DesiredSize.Height)
			{
				_autoCompleteItemHelpPopupDropDownManager.VerticalOffset = itemTopLeftInListCoordinates.Y;
				_autoCompleteItemHelpPopupDropDownManager.IsOpen = true;
				return true;
			}
			else
			{
				return false;
			}
		}

		#endregion  // PositionAutoCompleteItemHelpPopup

		#region ReplaceAutoCompleteItems

		private void ReplaceAutoCompleteItems(List<IFormulaElement> newAutoCompleteItems)
		{
			// Remove the items in _autoCompleteItems which should no longer be displayed.
			for (int i = _autoCompleteItems.Count - 1; i >= 0; i--)
			{
				int newIndex = newAutoCompleteItems.LastIndexOf(_autoCompleteItems[i]);

				// If the item isn't in the new list, remove it from _autoCompleteItems. Otherwise, remove the item from the new list so
				// it isn't re-added to _autoCompleteItems in the next loop below.
				if (newIndex < 0)
					_autoCompleteItems.RemoveAt(i);
				else
					newAutoCompleteItems.RemoveAt(newIndex);
			}

			// Add all remaining new items to _autoCompleteItems. The only thing left should be items which weren't already in the 
			// _autoCompleteItems collection. They must be added in sorted order as well.
			for (int i = 0; i < newAutoCompleteItems.Count; i++)
			{
				IFormulaElement newElement = newAutoCompleteItems[i];
				int index = _autoCompleteItems.BinarySearch(newElement);

				Debug.Assert(index < 0, "The item should not be in the list.");
				if (index < 0)
					_autoCompleteItems.Insert(~index, newElement);
			}
		}

		#endregion  // ReplaceAutoCompleteItems

		#region SetHelpTypes

		private void SetHelpTypes(HelpTypes helpTypes, bool preventAutocompletingSingleMatch = false)
		{
			bool shouldOpenAutoCompleteList = (helpTypes & HelpTypes.AutoCompleteList) != 0;
			bool shouldOpenFunctionSignatureHelp = (helpTypes & HelpTypes.FunctionSignatureHelp) != 0;

			if (_autoCompleteListPopupDropDownManager != null)
			{
				// If this operation is user initiated (not implicitly due to text being typed), and the cursor is currently 
				// in a function, and there is only one function which is valid, commit it instead of showing the list.
				if (shouldOpenAutoCompleteList &&
					_isAutoCompleteListOpeningExplicitly &&
					preventAutocompletingSingleMatch == false &&
					_currentAutoCompleteableElementBeingConstructed != null &&
					_currentAutoCompleteableElementBeingConstructed.IsFuncId &&
					_startsWithFilteredItems.Count == 1 &&
					this.CommitAutoCompleteItem(_startsWithFilteredItems[0]))
				{
					shouldOpenAutoCompleteList = false;
				}

				_autoCompleteListPopupDropDownManager.IsOpen = shouldOpenAutoCompleteList;

				if (shouldOpenAutoCompleteList)
				{
					if (_autoCompleteList != null && _autoCompleteList.SelectedItem != null)
						_autoCompleteList.BringItemIntoView(_autoCompleteList.SelectedItem);
				}
			}

			if (_functionSignatureHelpPopupDropDownManager != null)
			{
				// If the function signature help will be open, set its StaysOpen value to the value indicating whether the 
				// auto-complete list will be open. If the auto-complete list is open, StaysOpen should be True so the user
				// can interact with the auto-complete list and the function signature help will remain open. If the auto-complete
				// list is closed, StaysOpen should be False so the function signature help will close when the user clicks 
				// outside the popup.
				if (shouldOpenFunctionSignatureHelp)
					_functionSignatureHelpPopupDropDownManager.StaysOpen = shouldOpenAutoCompleteList;

				_functionSignatureHelpPopupDropDownManager.IsOpen = shouldOpenFunctionSignatureHelp;
			}


			if (this.IsAutoCompleteListOpen || this.IsFunctionSignatureHelpOpen)
				this.HookHelpPopupMouseMessages();
			else
				this.UnhookHelpPopupMouseMessages();

		}

		#endregion // SetHelpTypes

		#region UnhookHelpPopupMouseMessages


		private void UnhookHelpPopupMouseMessages()
		{
			if (_helpPopupRootWithCapture == null)
				return;

			Mouse.RemoveLostMouseCaptureHandler(_helpPopupRootWithCapture, new MouseEventHandler(this.OnHelpPopupRootLostMouseCapture));
			_helpPopupRootWithCapture.MouseMove -= new MouseEventHandler(this.OnHelpPopupRootMouseMove);
			_helpPopupRootWithCapture = null;
		}


		#endregion  // UnhookHelpPopupMouseMessages

		#region UpdateHelpTypes

		private void UpdateHelpTypes(string typedText = null, int startSelectionIndex = -1, int endSelectionIndex = -1)
		{
			bool preventCommittingItemOnListClosed;
			this.SetHelpTypes(this.DetermineCurrentHelpTypes(typedText, out preventCommittingItemOnListClosed, startSelectionIndex, endSelectionIndex));
		}

		#endregion  // UpdateHelpTypes

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Internal Properties

		#region IsAutoCompleteListOpen

		internal bool IsAutoCompleteListOpen
		{
			get
			{
				if (_autoCompleteListPopupDropDownManager == null)
					return false;

				return _autoCompleteListPopupDropDownManager.IsOpen;
			}
		}

		#endregion  // IsAutoCompleteListOpen

		#region IsFunctionSignatureHelpOpen

		internal bool IsFunctionSignatureHelpOpen
		{
			get
			{
				if (_functionSignatureHelpPopupDropDownManager == null)
					return false;

				return _functionSignatureHelpPopupDropDownManager.IsOpen;
			}
		}

		#endregion // IsFunctionSignatureHelpOpen

		#region IsHelpShowing

		internal bool IsHelpShowing
		{
			get
			{
				return 
					this.IsAutoCompleteListOpen || 
					this.IsFunctionSignatureHelpOpen || 
					this.IsMouseOverHelpOpen;
			}
		}

		#endregion // IsHelpShowing

		#region IsMouseOverHelpOpen

		internal bool IsMouseOverHelpOpen
		{
			get
			{
				if (_mouseOverHelpPopupDropDownManager == null)
					return false;

				return _mouseOverHelpPopupDropDownManager.IsOpen;
			}
		}

		#endregion // IsMouseOverHelpOpen

		#endregion // Internal Properties

		#region Private Properties

		#region DelayedShowFunctionSignatureHelpTimer

		private DispatcherTimer DelayedShowFunctionSignatureHelpTimer
		{
			get
			{
				if (_delayedShowFunctionSignatureHelpTimer == null)
				{
					_delayedShowFunctionSignatureHelpTimer = new DispatcherTimer();
					_delayedShowFunctionSignatureHelpTimer.Tick += this.OnDelayedShowFunctionSignatureHelpTimerTick;
				}

				return _delayedShowFunctionSignatureHelpTimer;
			}
		}

		#endregion  // DelayedShowFunctionSignatureHelpTimer

		#region DisplayAutoCompleteItemHelpTimer

		private DispatcherTimer DisplayAutoCompleteItemHelpTimer
		{
			get
			{
				if (_displayAutoCompleteItemHelpTimer == null)
				{
					_displayAutoCompleteItemHelpTimer = new DispatcherTimer();
					_displayAutoCompleteItemHelpTimer.Interval = DisplayAutoCompleteItemHelpDelay;
					_displayAutoCompleteItemHelpTimer.Tick += this.OnDisplayAutoCompleteItemHelpTimerTick;
				}

				return _displayAutoCompleteItemHelpTimer;
			}
		}

		#endregion  // DisplayAutoCompleteItemHelpTimer

		#region HideAutoCompleteItemHelpTimer

		private DispatcherTimer HideAutoCompleteItemHelpTimer
		{
			get
			{
				if (_hideAutoCompleteItemHelpTimer == null)
				{
					_hideAutoCompleteItemHelpTimer = new DispatcherTimer();
					_hideAutoCompleteItemHelpTimer.Interval = HideAutoCompleteItemHelpDelay;
					_hideAutoCompleteItemHelpTimer.Tick += this.OnHideAutoCompleteItemHelpTimerTick;
				}

				return _hideAutoCompleteItemHelpTimer;
			}
		}

		#endregion  // HideAutoCompleteItemHelpTimer

		#endregion  // Private Properties

		#endregion  // Properties


		#region HelpTypes enum

		[Flags]
		private enum HelpTypes
		{
			None = 0x00,

			AutoCompleteList = 0x01,
			FunctionSignatureHelp = 0x02,
		}

		#endregion // HelpTypes enum
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