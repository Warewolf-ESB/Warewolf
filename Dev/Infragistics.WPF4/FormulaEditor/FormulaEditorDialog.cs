using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using Infragistics.Controls.Interactions;
using System.Linq;


using Infragistics.Windows.Licensing;
using Infragistics.Windows.Controls;


using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using Infragistics.Controls.Interactions.Primitives;
using System.Windows.Documents;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Provider;
using System.Windows.Data;

namespace Infragistics.Controls.Interactions
{
	/// <summary>
	/// Displays a detailed user interface for viewing and editing a formula in a <see cref="XamCalculationManager"/>.
	/// </summary>

	
	

	public class FormulaEditorDialog : FormulaEditorBase, 
		IDialogElementProxyHost
	{
		#region Member Variables

		private static readonly TimeSpan SearchDelay = TimeSpan.FromSeconds(0.5);


		private UltraLicense _license;


		private CancelDialogCommand _cancelCommand;
		private ClearFormulaCommand _clearCommand;
		private Action<bool?> _closedCallback;
		private Func<bool> _closingCallback;
		private CommitDialogCommand _commitCommand;
		private DispatcherTimer _delayedFunctionSearchTimer;
		private DispatcherTimer _delayedOperandSearchTimer;
		private FrameworkElement _dialogElement;
		private DialogElementProxy _dialogElementProxy;

		// MD 12/2/11 - TFS96596
		// Keep track of whether or not the formula was accepted in the dialog.
		private bool? _formulaAccepted;

		// MD 11/4/11
		// Found while fixing TFS95193
		// Bindings were not being updated when this property changed. This should be a DependencyProperty instead.
		//private FilteredCollection<FunctionCategory> _functionCategories;

		private ReadOnlyCollection<SearchTypeValue> _functionSearchTypes;
		private Dictionary<string, string> _localizedStrings;
		private NextSyntaxErrorCommand _nextSyntaxErrorCommand;

		// MD 11/4/11
		// Found while fixing TFS95193
		// Bindings were not being updated when this property changed. This should be a DependencyProperty instead.
		//private FilteredCollection<OperandInfo> _operands;

		private ReadOnlyCollection<SearchTypeValue> _operandSearchTypes;
		private Dictionary<string, OperatorInfo> _operators;

		// MD 11/4/11 - TFS95193
		private XamFormulaEditor _owningEditor;

		private PreviousSyntaxErrorCommand _previousSyntaxErrorCommand;
		private bool _revertShowDialogButtonsValueOnClose;

		#endregion //Member Variables

		#region Constructor

		static FormulaEditorDialog()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FormulaEditorDialog), new FrameworkPropertyMetadata(typeof(FormulaEditorDialog)));

		}

		/// <summary>
		/// Initializes a new <see cref="FormulaEditorDialog" />
		/// </summary>
		public FormulaEditorDialog()
		{



			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(FormulaEditorDialog), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

		}

		// MD 11/4/11 - TFS95193
		internal FormulaEditorDialog(XamFormulaEditor owningEditor)
			: this()
		{
			_owningEditor = owningEditor;
		}

		#endregion //Constructor

		#region Interfaces

		#region IDialogElementProxyHost Members

		bool IDialogElementProxyHost.OnClosing()
		{
			// MD 12/2/11 - TFS96596
			// Moved all code to a helper method so we can reset the _formulaAccepted flag when the user cancels closing the dialog.
			if (this.OnClosingHelper())
			{
				_formulaAccepted = null;
				return true;
			}

			return false;
		}

		// MD 12/2/11 - TFS96596
		// Moved all code from the IDialogElementProxyHost.OnClosing method.
		private bool OnClosingHelper()
		{
			if (_closingCallback != null && _closingCallback())
				return true;
		
			if (this.IsDirty)
			{
				string title = this.DialogTitle;


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				string message = FormulaEditorUtilities.GetString("SaveChangesPrompt");

				switch (MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes))
				{
					case MessageBoxResult.Yes:
						if (this.CommitFormula() == false)
							return true;
						break;

					case MessageBoxResult.No:
						this.RevertToOriginalFormula();
						break;

					default:
					case MessageBoxResult.Cancel:
						return true;
				}

			}

			return false;
		}

		#endregion

		#endregion  // Interfaces

		#region Base Class Overrides

		#region CancelEdit

		/// <summary>
		/// Cancels the changes to the formula and closes the dialog if <see cref="DisplayAsDialog"/> was called to show the 
		/// <see cref="FormulaEditorDialog"/>.
		/// </summary>
		public override void CancelEdit()
		{
			this.RevertToOriginalFormula();

			// MD 12/2/11 - TFS96596
			// When the user cancels the edit, temporarily store a flag indicating that.
			_formulaAccepted = false;

			this.CloseDialog();
		}

		#endregion  // CancelEdit

		#region CommitEdit

		/// <summary>
		/// Commits the formula to the target and closes the dialog if <see cref="DisplayAsDialog"/> was called to show the 
		/// <see cref="FormulaEditorDialog"/>.
		/// </summary>
		public override void CommitEdit()
		{
			if (this.CommitFormula())
			{
				// MD 12/2/11 - TFS96596
				// When the user commits the edit, temporarily store a flag indicating that.
				_formulaAccepted = true;

				this.CloseDialog();
			}
		}

		#endregion  // CommitEdit

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_dialogElementProxy != null)
				_dialogElementProxy.Initialize();

			if (this.TextBox != null)
				this.UndoRedoButtonVisibility = Visibility.Visible;
			else
				this.UndoRedoButtonVisibility = Visibility.Collapsed;

			// MD 11/4/11 - TFS95193
			// The dialog is now managing the bindings, because we need to add the bindings after the dialog is in the visual tree 
			// to prevent a bug in SL from occurring.
			if (_owningEditor != null)
			{
				Binding dialogTargetBinding = new Binding("Target");
				dialogTargetBinding.Source = _owningEditor;
				dialogTargetBinding.Mode = BindingMode.TwoWay;
				BindingOperations.SetBinding(this, FormulaEditorDialog.TargetProperty, dialogTargetBinding);

				Binding dialogFormulaBinding = new Binding("Formula");
				dialogFormulaBinding.Source = _owningEditor;
				dialogFormulaBinding.Mode = BindingMode.TwoWay;
				BindingOperations.SetBinding(this, FormulaEditorDialog.FormulaProperty, dialogFormulaBinding);

				Binding dialogShowContextualHelpBinding = new Binding("ShowContextualHelp");
				dialogShowContextualHelpBinding.Source = _owningEditor;
				dialogShowContextualHelpBinding.Mode = BindingMode.TwoWay;
				BindingOperations.SetBinding(this, FormulaEditorDialog.ShowContextualHelpProperty, dialogShowContextualHelpBinding);

				if (this.TextBox != null)
					this.TextBox.ReinitializeUndoHistory();
			}
		}

		#endregion //OnApplyTemplate

		#region OnCalculationManagerChanged

		internal override void OnCalculationManagerChanged()
		{
			base.OnCalculationManagerChanged();

			// MD 11/4/11
			// Found while fixing TFS95193
			//_functionCategories = FormulaEditorUtilities.GetFunctionCategories(this.Functions);
			//_operands = FormulaEditorUtilities.GetOperands(this, this.CalculationManager);
			this.FunctionCategories = FormulaEditorUtilities.GetFunctionCategories(this.Functions);
			this.Operands = FormulaEditorUtilities.GetOperands(this, this.CalculationManager);

			this.UpdateDialogTitle();
		}

		#endregion  // OnCalculationManagerChanged

		#region OnFormulaChanged

		internal override void OnFormulaChanged()
		{
			base.OnFormulaChanged();

			if (_clearCommand != null)
				_clearCommand.RaiseCanExecuteChanged();
		}

		#endregion  // OnFormulaChanged

		#region OnFormulaProviderReferenceChanged

		internal override void OnFormulaProviderReferenceChanged()
		{
			base.OnFormulaProviderReferenceChanged();

			// MD 11/4/11
			// Found while fixing TFS95193
			//FormulaEditorUtilities.ReinitializeEnabledStateOfOperands(_operands);
			FormulaEditorUtilities.ReinitializeEnabledStateOfOperands((FilteredCollection<OperandInfo>)this.Operands);

			this.UpdateDialogTitle();
		}

		#endregion  // OnFormulaProviderReferenceChanged

		#region OnTargetChanged

		internal override void OnTargetChanged(object oldTarget)
		{
			// MD 11/4/11
			// Found while fixing TFS95193
			//FilteredCollection<OperandInfo> oldOperands = _operands;
			FilteredCollection<OperandInfo> oldOperands = (FilteredCollection<OperandInfo>)this.Operands;

			base.OnTargetChanged(oldTarget);

			// MD 11/4/11
			// Found while fixing TFS95193
			//if (oldOperands == _operands)
			//    _operands = FormulaEditorUtilities.GetOperands(this, this.CalculationManager);
			if (oldOperands == this.Operands)
				this.Operands = FormulaEditorUtilities.GetOperands(this, this.CalculationManager);

			this.UpdateDialogTitle();
		}

		#endregion  // OnTargetChanged

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region DisplayAsDialog

		/// <summary>
		/// Displays the <see cref="FormulaEditorDialog"/> as a dialog window.
		/// </summary>
		/// <param name="container">The element or window that contains the dialog.</param>
		/// <param name="dialogSize">The size of the displayed dialog.</param>
		/// <param name="showModally">True to show the dialog modally, otherwise False.</param>
		/// <param name="closingCallback">The callback to call when the dialog is closing.</param>
		/// <param name="closedCallback">The callback to call when the dialog is closed.</param>
		public void DisplayAsDialog(FrameworkElement container, Size dialogSize, bool showModally, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			if (this.ShowDialogButtons == false)
			{
				_revertShowDialogButtonsValueOnClose = true;
				this.ShowDialogButtons = true;
			}
			else
			{
				_revertShowDialogButtonsValueOnClose = false;
			}

			_closedCallback = closedCallback;
			_closingCallback = closingCallback;

			if (_dialogElementProxy == null)
				_dialogElementProxy = new DialogElementProxy(this);

			_dialogElement = DialogManager.DisplayDialog(container,
					this,
					dialogSize,
					true,
					this.DialogTitle,
					showModally,
					null,
					null,
					this.DialogClosedCallback,
					true
				);

			// If the element is null, 
			if (_dialogElement == null)
				this.CleanupDialog();
		}

		#endregion  // DisplayAsDialog

		#endregion  // Public Methods

		#region Private Methods

		#region AddOperator

		private void AddOperator(Dictionary<string, OperatorInfo> operators, string operatorValue, string operatorKey)
		{
			operators.Add(operatorKey, new OperatorInfo(this, operatorValue, FormulaEditorUtilities.GetString(operatorKey)));
		}

		#endregion // AddLocalizedString

		#region CleanupDialog

		private void CleanupDialog()
		{
			_dialogElement = null;

			// MD 12/2/11 - TFS96596
			// Reset the _formulaAccepted flag when the dialog closes.
			_formulaAccepted = null;

			if (_revertShowDialogButtonsValueOnClose)
				this.ShowDialogButtons = false;

			if (_dialogElementProxy != null)
			{
				_dialogElementProxy.Dispose(); 
				_dialogElementProxy = null;
			}

			_closingCallback = null;
			_closedCallback = null;

			// MD 11/4/11 - TFS95193
			// The dialog is now managing the bindings.
			if (_owningEditor != null)
			{
				this.ClearValue(FormulaEditorDialog.FormulaProperty);
				this.ClearValue(FormulaEditorDialog.ShowContextualHelpProperty);
				this.ClearValue(FormulaEditorDialog.TargetProperty);
				_owningEditor = null;
			}
		}

		#endregion  // CleanupDialog

		#region CloseDialog

		private void CloseDialog()
		{
			if (_dialogElementProxy != null)
				_dialogElementProxy.Close();
		}

		#endregion  // CloseDialog

		#region CommitFormula

		private bool CommitFormula()
		{
			if (this.IsDirty && this.HasSyntaxError)
			{
				string title = this.DialogTitle;
				string message = FormulaEditorUtilities.GetString("CommitFormulaWithErrorsPrompt");



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

				switch (MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes))
				{
					case MessageBoxResult.Yes:
						break;

					case MessageBoxResult.No:
						this.RevertToOriginalFormula();
						return true;

					default:
					case MessageBoxResult.Cancel:
						return false;
				}

			}

			string formula = this.Formula;

			if (this.FormulaProvider != null)
				this.FormulaProvider.Formula = formula;

			return true;
		}

		#endregion  // CommitFormula

		#region DialogClosedCallback

		private void DialogClosedCallback(bool? result)
		{
			if (_closedCallback != null)
			{
				// MD 12/2/11 - TFS96596
				// The value passed in is always false, so we must keep track of whether the real dialog result.
				//_closedCallback(result);
				_closedCallback(_formulaAccepted);
			}

			this.CleanupDialog();
		}

		#endregion  // DialogClosedCallback

		#region OnDelayedFunctionSearchTimerTick

		private void OnDelayedFunctionSearchTimerTick(object sender, EventArgs e)
		{
			this.DelayedFunctionSearchTimer.Stop();
			this.ReevaluateFunctionSearch();
		}

		#endregion  // OnDelayedFunctionSearchTimerTick

		#region OnDelayedOperandSearchTimerTick

		private void OnDelayedOperandSearchTimerTick(object sender, EventArgs e)
		{
			this.DelayedOperandSearchTimer.Stop();
			this.ReevaluateOperandSearch();
		}

		#endregion  // OnDelayedOperandSearchTimerTick

		#region ReevaluateFunctionSearch

		private void ReevaluateFunctionSearch()
		{
			// MD 11/4/11
			// Found while fixing TFS95193
			//if (_functionCategories == null)
			//    return;
			FilteredCollection<FunctionCategory> functionCategories = (FilteredCollection<FunctionCategory>)this.FunctionCategories;
			if (functionCategories == null)
				return;

			string functionSearchText = this.FunctionSearchTextResolved;

			// This is to filter the categories.
			Func<FunctionCategory, bool> categoryFilterPredicate = null;

			if (functionSearchText != null)
			{
				switch (this.FunctionSearchType)
				{
					case FunctionSearchType.All:
						categoryFilterPredicate = category =>
						{
							// If the category name matches the search, clear the filter on functions so all functions under the 
							// category are displayed.
							if (category.DoesMatchFilterText(functionSearchText))
							{
								category.ApplyFunctionNameFilter(null);
								return true;
							}

							category.ApplyFunctionNameFilter(functionSearchText);

							// If the category has any functions within it, make sure it is expanded so the user can see the functions
							if (category.Functions.FirstOrDefault() != null)
							{
								category.IsExpanded = true;
								return true;
							}

							return false;
						};
						break;

					case FunctionSearchType.FunctionName:
						categoryFilterPredicate = category =>
						{
							category.ApplyFunctionNameFilter(functionSearchText);

							if (category.Functions.FirstOrDefault() != null)
							{
								category.IsExpanded = true;
								return true;
							}

							return false;
						};
						break;

					case FunctionSearchType.Category:
						categoryFilterPredicate = category =>
						{
							category.ApplyFunctionNameFilter(null);
							return category.DoesMatchFilterText(functionSearchText);
						};
						break;

					default:
						Debug.Assert(false, "Unknown FunctionSearchType: " + this.FunctionSearchType);
						break;
				}
			}

			// MD 11/4/11
			// Found while fixing TFS95193
			//_functionCategories.ApplyFilter(categoryFilterPredicate);	
			functionCategories.ApplyFilter(categoryFilterPredicate);	
		}

		#endregion  // ReevaluateFunctionSearch

		#region ReevaluateFunctionSearchAsync

		private void ReevaluateFunctionSearchAsync()
		{
			this.DelayedFunctionSearchTimer.Stop();

			if (String.IsNullOrEmpty(this.FunctionSearchTextResolved))
				this.ReevaluateFunctionSearch();
			else
				this.DelayedFunctionSearchTimer.Start();
		}

		#endregion  // ReevaluateFunctionSearchAsync

		#region ReevaluateOperandSearch

		private void ReevaluateOperandSearch()
		{
			// MD 11/4/11
			// Found while fixing TFS95193
			//if (_operands == null)
			//    return;
			FilteredCollection<OperandInfo> operands = (FilteredCollection<OperandInfo>)this.Operands;
			if (operands == null)
				return;

			string operandSearchText = this.OperandSearchTextResolved;

			// This is to filter the categories.
			Func<OperandInfo, bool> operandFilterPredicate = operand =>
			{
				operand.ClearOperandNameFilterRecursive();
				return true;
			};

			if (String.IsNullOrEmpty(operandSearchText) == false)
			{
				switch (this.OperandSearchType)
				{
					case OperandSearchType.All:
						operandFilterPredicate = operand =>
						{
							// If the category name matches the search, clear the filter on the operand and all its children so they 
							// are all displayed.
							if (operand.DoesMatchFilterText(operandSearchText))
							{
								operand.ClearOperandNameFilterRecursive();
								return true;
							}

							if (operand.Children == null)
								return false;

							operand.ApplyOperandNameFilter(operandFilterPredicate);

							// If the operand has any filtered in operands within it, make sure it is included and expanded so the user 
							// can see the children.
							if (operand.Children.FirstOrDefault() != null)
							{
								operand.IsExpanded = true;
								return true;
							}

							return false;
						};
						break;

					case OperandSearchType.OperandName:
						operandFilterPredicate = operand =>
						{
							if (operand.IsDataReference)
							{
								Debug.Assert(operand.Children == null, "Data references should not have children.");
								return operand.DoesMatchFilterText(operandSearchText);
							}

							if (operand.Children == null)
								return false;

							operand.ApplyOperandNameFilter(operandFilterPredicate);

							if (operand.Children.FirstOrDefault() != null)
							{
								operand.IsExpanded = true;
								return true;
							}

							return false;
						};
						break;

					case OperandSearchType.ControlName:
						operandFilterPredicate = operandCategory =>
						{
							if (operandCategory.NodeType != ReferenceNodeType.ControlsGroup)
								return false;

							operandCategory.IsExpanded = true;
							operandCategory.ApplyOperandNameFilter(control =>
								{
									control.ClearOperandNameFilterRecursive();

									if (control.DoesMatchFilterText(operandSearchText))
										return true;

									return false;
								});

							return true;
						};
						break;

					case OperandSearchType.NamedReferenceCategory:
						operandFilterPredicate = operandCategory =>
						{
							if (operandCategory.NodeType != ReferenceNodeType.NamedReferencesGroup)
								return false;

							operandCategory.IsExpanded = true;
							operandCategory.ApplyOperandNameFilter(namedReferenceCategory =>
							{
								namedReferenceCategory.ClearOperandNameFilterRecursive();

								if (namedReferenceCategory.DoesMatchFilterText(operandSearchText))
								{
									namedReferenceCategory.IsExpanded = true;
									return true;
								}

								return false;
							});

							return true;
						};
						break;

					default:
						Debug.Assert(false, "Unknown FunctionSearchType: " + this.FunctionSearchType);
						break;
				}
			}

			// MD 11/4/11
			// Found while fixing TFS95193
			//_operands.ApplyFilter(operandFilterPredicate);
			operands.ApplyFilter(operandFilterPredicate);
		}

		#endregion  // ReevaluateOperandSearch

		#region ReevaluateOperandSearchAsync

		private void ReevaluateOperandSearchAsync()
		{
			this.DelayedOperandSearchTimer.Stop();

			if (String.IsNullOrEmpty(this.OperandSearchTextResolved))
				this.ReevaluateOperandSearch();
			else
				this.DelayedOperandSearchTimer.Start();
		}

		#endregion  // ReevaluateOperandSearchAsync

		#region RevertToOriginalFormula

		private void RevertToOriginalFormula()
		{
			if (this.FormulaProvider == null)
				this.Formula = null;
			else
				this.Formula = this.FormulaProvider.Formula;
		}

		#endregion  // RevertToOriginalFormula

		#region UpdateDialogTitle

		private void UpdateDialogTitle()
		{

			ToolWindow toolWindow = _dialogElement as ToolWindow;
			if (toolWindow != null)
			{
				toolWindow.Title = this.DialogTitle;
				return;
			}


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			Debug.Assert(_dialogElement == null, "Unknown dialog element type.");
		}

		#endregion  // UpdateDialogTitle

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region CancelCommand

		/// <summary>
		/// Gets the command which cancels out of the <see cref="FormulaEditorDialog"/>.
		/// </summary>
		public CancelDialogCommand CancelCommand
		{
			get
			{
				if (_cancelCommand == null)
					_cancelCommand = new CancelDialogCommand(this);

				return _cancelCommand;
			}
		}

		#endregion  // CancelCommand

		#region ClearCommand

		/// <summary>
		/// Gets the command which performs a clear operation.
		/// </summary>
		public ClearFormulaCommand ClearCommand
		{
			get
			{
				if (_clearCommand == null)
					_clearCommand = new ClearFormulaCommand(this);

				return _clearCommand;
			}
		}

		#endregion  // ClearCommand

		#region CommitCommand

		/// <summary>
		/// Gets the command which commits the formula from the <see cref="FormulaEditorDialog"/> to the target and closes the dialog.
		/// </summary>
		public CommitDialogCommand CommitCommand
		{
			get
			{
				if (_commitCommand == null)
					_commitCommand = new CommitDialogCommand(this);

				return _commitCommand;
			}
		}

		#endregion  // CommitCommand

		#region CurrentSyntaxErrorInfo

		private static readonly DependencyPropertyKey CurrentSyntaxErrorInfoPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CurrentSyntaxErrorInfo",
			typeof(SyntaxErrorInfo), typeof(FormulaEditorDialog), null, new PropertyChangedCallback(OnCurrentSyntaxErrorInfoPropertyChanged));

		/// <summary>
		/// Identifies the read-only <see cref="CurrentSyntaxErrorInfo"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentSyntaxErrorInfoProperty = CurrentSyntaxErrorInfoPropertyKey.DependencyProperty;

		private static void OnCurrentSyntaxErrorInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorDialog instance = (FormulaEditorDialog)d;
			instance.OnCurrentSyntaxErrorInfoPropertyChanged();
		}

		private void OnCurrentSyntaxErrorInfoPropertyChanged()
		{
			if (_nextSyntaxErrorCommand != null)
				_nextSyntaxErrorCommand.RaiseCanExecuteChanged();

			if (_previousSyntaxErrorCommand != null)
				_previousSyntaxErrorCommand.RaiseCanExecuteChanged();

			ReadOnlyCollection<SyntaxErrorInfo> syntaxErrorInfos = this.SyntaxErrorInfos;
			if (syntaxErrorInfos == null || syntaxErrorInfos.Count == 1)
			{
				this.MultipleSyntaxErrorsLabel = null;
			}
			else
			{
				int index = syntaxErrorInfos.IndexOf(this.CurrentSyntaxErrorInfo);

				if (index < 0)
				{
					Debug.Assert(false, "Cannot find the current syntax error in the collection.");
					index = 0;
				}

				this.MultipleSyntaxErrorsLabel = string.Format("{0} of {1} errors", index + 1, syntaxErrorInfos.Count);
			}
		}

		/// <summary>
		/// Gets information about the current syntax error being displayed in the dialog.
		/// </summary>
		/// <seealso cref="CurrentSyntaxErrorInfoProperty"/>
		public SyntaxErrorInfo CurrentSyntaxErrorInfo
		{
			get
			{
				return (SyntaxErrorInfo)this.GetValue(FormulaEditorDialog.CurrentSyntaxErrorInfoProperty);
			}
			internal set
			{
				this.SetValue(FormulaEditorDialog.CurrentSyntaxErrorInfoPropertyKey, value);
			}
		}

		#endregion //CurrentSyntaxErrorInfo

		#region FunctionCategories

		// MD 11/4/11
		// Found while fixing TFS95193
		// Bindings were not being updated when this property changed. This should be a DependencyProperty instead.
		///// <summary>
		///// Gets the collection of function categories to display in the UI based on the current search criteria.
		///// </summary>
		//public IEnumerable<FunctionCategory> FunctionCategories
		//{
		//    get { return _functionCategories; }
		//}
		private static readonly DependencyPropertyKey FunctionCategoriesPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("FunctionCategories",
			typeof(IEnumerable<FunctionCategory>), typeof(FormulaEditorDialog), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="FunctionCategories"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FunctionCategoriesProperty = FunctionCategoriesPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the collection of function categories to display in the UI based on the current search criteria.
		/// </summary>
		/// <seealso cref="FunctionCategoriesProperty"/>
		public IEnumerable<FunctionCategory> FunctionCategories
		{
			get
			{
				return (IEnumerable<FunctionCategory>)this.GetValue(FormulaEditorDialog.FunctionCategoriesProperty);
			}
			private set
			{
				this.SetValue(FormulaEditorDialog.FunctionCategoriesPropertyKey, value);
			}
		}

		#endregion  // FunctionCategories

		#region FunctionSearchText

		/// <summary>
		/// Identifies the <see cref="FunctionSearchText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FunctionSearchTextProperty = DependencyPropertyUtilities.Register("FunctionSearchText",
			typeof(string), typeof(FormulaEditorDialog),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnFunctionSearchTextChanged))
			);

		private static void OnFunctionSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorDialog instance = (FormulaEditorDialog)d;

			if (String.Equals((string)e.NewValue, (string)e.OldValue, StringComparison.CurrentCultureIgnoreCase) == false)
			{
				instance.ReevaluateFunctionSearchAsync();
			}
		}

		/// <summary>
		/// Gets or sets the text on which to search through the functions.
		/// </summary>
		/// <seealso cref="FunctionSearchTextProperty"/>
		/// <seealso cref="FunctionSearchType"/>
		public string FunctionSearchText
		{
			get
			{
				return (string)this.GetValue(FormulaEditorDialog.FunctionSearchTextProperty);
			}
			set
			{
				this.SetValue(FormulaEditorDialog.FunctionSearchTextProperty, value);
			}
		}

		private string FunctionSearchTextResolved
		{
			get
			{
				string functionSearchText = this.FunctionSearchText;

				if (functionSearchText == null)
					return null;

				return functionSearchText.Trim();
			}
		}

		#endregion //FunctionSearchText

		#region FunctionSearchType

		/// <summary>
		/// Identifies the <see cref="FunctionSearchType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FunctionSearchTypeProperty = DependencyPropertyUtilities.Register("FunctionSearchType",
			typeof(FunctionSearchType), typeof(FormulaEditorDialog),
			DependencyPropertyUtilities.CreateMetadata(FunctionSearchType.All, new PropertyChangedCallback(OnFunctionSearchTypeChanged))
			);

		private static void OnFunctionSearchTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorDialog instance = (FormulaEditorDialog)d;

			instance.DelayedFunctionSearchTimer.Stop();
			instance.ReevaluateFunctionSearch();
		}

		/// <summary>
		/// Gets or sets the type of search to perform on the list of functions.
		/// </summary>
		/// <seealso cref="FunctionSearchTypeProperty"/>
		/// <seealso cref="FunctionSearchText"/>
		/// <seealso cref="FunctionSearchTypes"/>
		public FunctionSearchType FunctionSearchType
		{
			get
			{
				return (FunctionSearchType)this.GetValue(FormulaEditorDialog.FunctionSearchTypeProperty);
			}
			set
			{
				this.SetValue(FormulaEditorDialog.FunctionSearchTypeProperty, value);
			}
		}

		#endregion //FunctionSearchType

		#region FunctionSearchTypes

		/// <summary>
		/// Gets the various function search types and their associated localized descriptions.
		/// </summary>
		/// <seealso cref="FunctionSearchType"/>
		public IEnumerable FunctionSearchTypes
		{
			get
			{
				if (_functionSearchTypes == null)
				{
					List<SearchTypeValue> functionSearchTypes = new List<SearchTypeValue>();
					functionSearchTypes.Add(new SearchTypeValue(FunctionSearchType.All));
					functionSearchTypes.Add(new SearchTypeValue(FunctionSearchType.FunctionName));
					functionSearchTypes.Add(new SearchTypeValue(FunctionSearchType.Category));
					_functionSearchTypes = functionSearchTypes.AsReadOnly();
				}

				return _functionSearchTypes;
			}
		}

		#endregion  // FunctionSearchTypes

		#region HasMultipleSyntaxErrors

		private static readonly DependencyPropertyKey HasMultipleSyntaxErrorsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("HasMultipleSyntaxErrors",
			typeof(bool), typeof(FormulaEditorDialog), false, null);

		/// <summary>
		/// Identifies the read-only <see cref="HasMultipleSyntaxErrors"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasMultipleSyntaxErrorsProperty = HasMultipleSyntaxErrorsPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the value which indicates whether there are multiple errors in the formula.
		/// </summary>
		/// <seealso cref="HasMultipleSyntaxErrorsProperty"/>
		public bool HasMultipleSyntaxErrors
		{
			get
			{
				return (bool)this.GetValue(FormulaEditorDialog.HasMultipleSyntaxErrorsProperty);
			}
			private set
			{
				this.SetValue(FormulaEditorDialog.HasMultipleSyntaxErrorsPropertyKey, value);
			}
		}

		#endregion //HasMultipleSyntaxErrors

		#region LocalizedStrings

		/// <summary>
		/// Returns a dictionary of localized strings for use by the controls in the template.
		/// </summary>
		public Dictionary<string, string> LocalizedStrings
		{
			get
			{
				if (_localizedStrings == null)
				{
					_localizedStrings = new Dictionary<string, string>();

					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "CancelButton");
					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "ClearButton");
					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "EditingAreaLabel");
					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "FunctionSignatureLabel");
					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "FunctionTreeLabel");
					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "OKButton");
					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "OperandSignatureLabel");
					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "OperandTreeLabel");
				}

				return this._localizedStrings;
			}
		}

		#endregion //LocalizedStrings

		#region MultipleSyntaxErrorsLabel

		private static readonly DependencyPropertyKey MultipleSyntaxErrorsLabelPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MultipleSyntaxErrorsLabel",
			typeof(string), typeof(FormulaEditorDialog), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="MultipleSyntaxErrorsLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MultipleSyntaxErrorsLabelProperty = MultipleSyntaxErrorsLabelPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the label to display when there are multiple syntax errors.
		/// </summary>
		/// <seealso cref="MultipleSyntaxErrorsLabelProperty"/>
		public string MultipleSyntaxErrorsLabel
		{
			get
			{
				return (string)this.GetValue(FormulaEditorDialog.MultipleSyntaxErrorsLabelProperty);
			}
			private set
			{
				this.SetValue(FormulaEditorDialog.MultipleSyntaxErrorsLabelPropertyKey, value);
			}
		}

		#endregion //MultipleSyntaxErrorsLabel

		#region NextSyntaxErrorCommand

		/// <summary>
		/// Gets the command which shows the next syntax error of the formula.
		/// </summary>
		public NextSyntaxErrorCommand NextSyntaxErrorCommand
		{
			get
			{
				if (_nextSyntaxErrorCommand == null)
					_nextSyntaxErrorCommand = new NextSyntaxErrorCommand(this);

				return _nextSyntaxErrorCommand;
			}
		}

		#endregion  // NextSyntaxErrorCommand

		#region Operands

		// MD 11/4/11
		// Found while fixing TFS95193
		// Bindings were not being updated when this property changed. This should be a DependencyProperty instead.
		///// <summary>
		///// Gets the collection of operands and/or their owners to display in the UI based on the current search criteria.
		///// </summary>
		//public IEnumerable<OperandInfo> Operands
		//{
		//    get { return _operands; }
		//}
		private static readonly DependencyPropertyKey OperandsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Operands",
			typeof(IEnumerable<OperandInfo>), typeof(FormulaEditorDialog), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Operands"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OperandsProperty = OperandsPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the collection of operands and/or their owners to display in the UI based on the current search criteria.
		/// </summary>
		/// <seealso cref="OperandsProperty"/>
		public IEnumerable<OperandInfo> Operands
		{
			get
			{
				return (IEnumerable<OperandInfo>)this.GetValue(FormulaEditorDialog.OperandsProperty);
			}
			private set
			{
				this.SetValue(FormulaEditorDialog.OperandsPropertyKey, value);
			}
		}

		#endregion  // Operands

		#region OperandSearchText

		/// <summary>
		/// Identifies the <see cref="OperandSearchText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OperandSearchTextProperty = DependencyPropertyUtilities.Register("OperandSearchText",
			typeof(string), typeof(FormulaEditorDialog),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnOperandSearchTextChanged))
			);

		private static void OnOperandSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorDialog instance = (FormulaEditorDialog)d;

			if (String.Equals((string)e.NewValue, (string)e.OldValue, StringComparison.CurrentCultureIgnoreCase) == false)
			{
				instance.ReevaluateOperandSearchAsync();
			}
		}

		/// <summary>
		/// Gets or sets the text on which to search through the operands.
		/// </summary>
		/// <seealso cref="OperandSearchTextProperty"/>
		/// <seealso cref="OperandSearchType"/>
		public string OperandSearchText
		{
			get
			{
				return (string)this.GetValue(FormulaEditorDialog.OperandSearchTextProperty);
			}
			set
			{
				this.SetValue(FormulaEditorDialog.OperandSearchTextProperty, value);
			}
		}

		private string OperandSearchTextResolved
		{
			get
			{
				string operandSearchText = this.OperandSearchText;

				if (operandSearchText == null)
					return null;

				return operandSearchText.Trim();
			}
		}

		#endregion //OperandSearchText

		#region OperandSearchType

		/// <summary>
		/// Identifies the <see cref="OperandSearchType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OperandSearchTypeProperty = DependencyPropertyUtilities.Register("OperandSearchType",
			typeof(OperandSearchType), typeof(FormulaEditorDialog),
			DependencyPropertyUtilities.CreateMetadata(OperandSearchType.All, new PropertyChangedCallback(OnOperandSearchTypeChanged))
			);

		private static void OnOperandSearchTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorDialog instance = (FormulaEditorDialog)d;

			instance.DelayedOperandSearchTimer.Stop();
			instance.ReevaluateOperandSearch();
		}

		/// <summary>
		/// Gets or sets the type of search to perform on the list of operands.
		/// </summary>
		/// <seealso cref="OperandSearchTypeProperty"/>
		public OperandSearchType OperandSearchType
		{
			get
			{
				return (OperandSearchType)this.GetValue(FormulaEditorDialog.OperandSearchTypeProperty);
			}
			set
			{
				this.SetValue(FormulaEditorDialog.OperandSearchTypeProperty, value);
			}
		}

		#endregion //OperandSearchType

		#region OperandSearchTypes

		/// <summary>
		/// Gets the various operand search types and their associated localized descriptions.
		/// </summary>
		/// <seealso cref="OperandSearchType"/>
		public IEnumerable OperandSearchTypes
		{
			get
			{
				if (_operandSearchTypes == null)
				{
					List<SearchTypeValue> operandSearchTypes = new List<SearchTypeValue>();
					operandSearchTypes.Add(new SearchTypeValue(OperandSearchType.All));
					operandSearchTypes.Add(new SearchTypeValue(OperandSearchType.OperandName));
					operandSearchTypes.Add(new SearchTypeValue(OperandSearchType.ControlName));
					operandSearchTypes.Add(new SearchTypeValue(OperandSearchType.NamedReferenceCategory));
					_operandSearchTypes = operandSearchTypes.AsReadOnly();
				}

				return _operandSearchTypes;
			}
		}

		#endregion  // OperandSearchTypes

		#region Operators

		/// <summary>
		/// Gets the collection of operators displayed in the dialog.
		/// </summary>
		public Dictionary<string, OperatorInfo> Operators
		{
			get
			{
				if (_operators == null)
				{
					_operators = new Dictionary<string, OperatorInfo>();

					this.AddOperator(_operators, "+", "Operator_Add");
					this.AddOperator(_operators, "/", "Operator_Divide");
					this.AddOperator(_operators, "=", "Operator_Equal");
					this.AddOperator(_operators, ">", "Operator_GreaterThan");
					this.AddOperator(_operators, "<", "Operator_LessThan");
					this.AddOperator(_operators, "*", "Operator_Multiply");
					this.AddOperator(_operators, "<>", "Operator_NotEqual");
					this.AddOperator(_operators, "%", "Operator_Percent");
					this.AddOperator(_operators, "^", "Operator_Power");
					this.AddOperator(_operators, "-", "Operator_Subtract");
				}

				return _operators;
			}
		}

		#endregion  // Operators

		#region PreviousSyntaxErrorCommand

		/// <summary>
		/// Gets the command which shows the previous syntax error of the formula.
		/// </summary>
		public PreviousSyntaxErrorCommand PreviousSyntaxErrorCommand
		{
			get
			{
				if (_previousSyntaxErrorCommand == null)
					_previousSyntaxErrorCommand = new PreviousSyntaxErrorCommand(this);

				return _previousSyntaxErrorCommand;
			}
		}

		#endregion  // PreviousSyntaxErrorCommand

		#region ShowDialogButtons

		/// <summary>
		/// Identifies the <see cref="ShowDialogButtons"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowDialogButtonsProperty = DependencyPropertyUtilities.Register("ShowDialogButtons",
			typeof(bool), typeof(FormulaEditorDialog),
			DependencyPropertyUtilities.CreateMetadata(false)
			);

		/// <summary>
		/// Gets or sets the value indicating whether the OK and Cancel buttons should display.
		/// </summary>
		/// <seealso cref="ShowDialogButtonsProperty"/>
		public bool ShowDialogButtons
		{
			get
			{
				return (bool)this.GetValue(FormulaEditorDialog.ShowDialogButtonsProperty);
			}
			set
			{
				this.SetValue(FormulaEditorDialog.ShowDialogButtonsProperty, value);
			}
		}

		#endregion //ShowDialogButtons

		#region SyntaxErrorInfos

		private static readonly DependencyPropertyKey SyntaxErrorInfosPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SyntaxErrorInfos",
			typeof(ReadOnlyCollection<SyntaxErrorInfo>), typeof(FormulaEditorDialog), null, new PropertyChangedCallback(OnSyntaxErrorInfosPropertyChanged));

		/// <summary>
		/// Identifies the read-only <see cref="SyntaxErrorInfos"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SyntaxErrorInfosProperty = SyntaxErrorInfosPropertyKey.DependencyProperty;

		private static void OnSyntaxErrorInfosPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorDialog instance = (FormulaEditorDialog)d;
			instance.OnSyntaxErrorInfosPropertyChanged((ReadOnlyCollection<SyntaxErrorInfo>)e.NewValue);
		}

		private void OnSyntaxErrorInfosPropertyChanged(ReadOnlyCollection<SyntaxErrorInfo> newErrorInfos)
		{
			if (newErrorInfos == null)
			{
				this.CurrentSyntaxErrorInfo = null;
				this.HasMultipleSyntaxErrors = false;
				return;
			}

			if (newErrorInfos.Count == 1)
				this.HasMultipleSyntaxErrors = false;
			else
				this.HasMultipleSyntaxErrors = true;

			this.CurrentSyntaxErrorInfo = newErrorInfos[0];
		}

		/// <summary>
		/// Gets the collection of syntax errors for the formula in the dialog or null if the formula is valid.
		/// </summary>
		/// <seealso cref="SyntaxErrorInfosProperty"/>
		public ReadOnlyCollection<SyntaxErrorInfo> SyntaxErrorInfos
		{
			get
			{
				return (ReadOnlyCollection<SyntaxErrorInfo>)this.GetValue(FormulaEditorDialog.SyntaxErrorInfosProperty);
			}
			internal set
			{
				this.SetValue(FormulaEditorDialog.SyntaxErrorInfosPropertyKey, value);
			}
		}

		#endregion //SyntaxErrors

		#region UndoRedoButtonVisibility

		private static readonly DependencyPropertyKey UndoRedoButtonVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("UndoRedoButtonVisibility",
			typeof(Visibility), typeof(FormulaEditorBase), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="UndoRedoButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UndoRedoButtonVisibilityProperty = UndoRedoButtonVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the visibility of the undo and redo buttons.
		/// </summary>
		/// <seealso cref="UndoRedoButtonVisibilityProperty"/>
		public Visibility UndoRedoButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(FormulaEditorDialog.UndoRedoButtonVisibilityProperty);
			}
			private set
			{
				this.SetValue(FormulaEditorDialog.UndoRedoButtonVisibilityPropertyKey, value);
			}
		}

		#endregion //UndoRedoButtonVisibility

		#endregion  // Public Properties

		#region Private Properties

		#region DelayedFunctionSearchTimer

		private DispatcherTimer DelayedFunctionSearchTimer
		{
			get
			{
				if (_delayedFunctionSearchTimer == null)
				{
					_delayedFunctionSearchTimer = new DispatcherTimer();
					_delayedFunctionSearchTimer.Interval = SearchDelay;
					_delayedFunctionSearchTimer.Tick += this.OnDelayedFunctionSearchTimerTick;
				}

				return _delayedFunctionSearchTimer;
			}
		}

		#endregion  // DelayedFunctionSearchTimer

		#region DelayedOperandSearchTimer

		private DispatcherTimer DelayedOperandSearchTimer
		{
			get
			{
				if (_delayedOperandSearchTimer == null)
				{
					_delayedOperandSearchTimer = new DispatcherTimer();
					_delayedOperandSearchTimer.Interval = SearchDelay;
					_delayedOperandSearchTimer.Tick += this.OnDelayedOperandSearchTimerTick;
				}

				return _delayedOperandSearchTimer;
			}
		}

		#endregion  // DelayedOperandSearchTimer

		#region DialogTitle

		private string DialogTitle
		{
			get 
			{
				IFormulaProvider provider = this.FormulaProvider;
				if (provider != null && provider.Reference != null)
					return FormulaEditorUtilities.GetString("DialogTitleWithTarget", provider.Reference.AbsoluteName);

				return FormulaEditorUtilities.GetString("DialogTitle");
			}
		}

		#endregion  // DialogTitle

		#region IsDirty

		private bool IsDirty
		{
			get
			{
				// MD 10/25/11 - TFS93899
				// We should treat null and empty strings as the same when considering dirty status.
				//if (this.FormulaProvider == null)
				//    return this.Formula != null;
				//
				//return this.Formula != this.FormulaProvider.Formula;
				if (this.FormulaProvider == null)
					return (string.IsNullOrEmpty(this.Formula) == false);

				string editFormula = this.Formula;
				string providerFormula = this.FormulaProvider.Formula;

				if (editFormula == providerFormula)
					return false;

				if (String.IsNullOrEmpty(editFormula) && String.IsNullOrEmpty(providerFormula))
					return false;

				return true;
			}
		}

		#endregion  // IsDirty

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