using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infragistics.Calculations;
using Infragistics.Controls.Interactions.Primitives;
using Infragistics.Calculations.Engine;

namespace Infragistics.Controls.Interactions
{
	/// <summary>
	/// Abstract base class for the <see cref="XamFormulaEditor"/> and <see cref="FormulaEditorDialog"/>.
	/// </summary>
	[TemplatePart(Name = PartContextualHelpHost, Type = typeof(ContextualHelpHost))]
	[TemplatePart(Name = PartFormulaTextBox, Type = typeof(FormulaEditorTextBox))]
	[TemplateVisualState(Name = VisualStateUtilities.StateError, GroupName = VisualStateUtilities.GroupError)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNoError, GroupName = VisualStateUtilities.GroupError)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNormal, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateDisabled, GroupName = VisualStateUtilities.GroupCommon)]
	public abstract class FormulaEditorBase : Control,
		ICommandTarget,
		ITypedPropertyChangeListener<object, string>	// MD 10/21/11 - TFS93511
	{
		#region Constants

		private const string PartContextualHelpHost = "PART_ContextualHelpHost";
		private const string PartFormulaTextBox = "PART_FormulaTextBox";

		#endregion  // Constants

		#region Member Variables

		private XamCalculationManager _calculationManager;

		// MD 10/21/11 - TFS93511
		private bool _calculationManagerListenerAttached;

		private ContextualHelpHost _contextualHelpHost;
		private FormulaCompiler _formulaCompiler;
		private IFormulaProvider _formulaProvider;
		private List<FunctionInfo> _functions;

		// MD 5/15/12 - TFS111115
		private bool _isCalculationManagerInitialized;

		private RedoFormulaEditCommand _redoCommand;
		private UndoFormulaEditCommand _undoCommand;
		private TargetAttachedPropertyListener _targetAttachedPropertyListener;
		private FormulaEditorTextBox _textBox;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="FormulaEditorBase"/> instance.
		/// </summary>
		protected FormulaEditorBase()
		{
			this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(this.OnIsEnabledChanged);

			// The control should be initially disabled because there is no target.
			this.IsEnabled = false;
		}

		#endregion  // Constructor

		#region Interfaces

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is FormulaEditorCommandBase)
				return this;

			return null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			CommandBase commandBase = command as CommandBase;
			if (null != commandBase &&
				null != commandBase.CommandSource &&
				null != commandBase.CommandSource.Parameter)
				return commandBase.CommandSource.Parameter == this;

			return command is FormulaEditorCommandBase;
		}

		#endregion

		// MD 10/21/11 - TFS93511
		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			if (dataItem == _calculationManager &&
				_isCalculationManagerInitialized == false &&
				(property == "IsInitialized" || property == "") &&
				_calculationManager.IsInitialized)
			{
				// MD 5/15/12 - TFS111115
				// We always want to listen to changes to the calc manager after it is initialized.
				//this.RemoveCalculationManagerListener();

				this.OnCalculationManagerChanged();
			}

			// MD 5/15/12 - TFS111115
			_isCalculationManagerInitialized = _calculationManager.IsInitialized;

			// If the named references or controls change in the calc manager, we should notify ourselves of the change 
			// to the operands tree and contextual help can be updated.
			if (dataItem is NamedReferenceCollection || dataItem is ControlReferenceManagerCollection)
				this.OnCalculationManagerChanged();
		}

		#endregion

		#endregion  // Interfaces

		#region Base Class Overrides

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_contextualHelpHost != null)
				_contextualHelpHost.SetEditor(null);

			if (_textBox != null)
				_textBox.SetOwner(null);

			_contextualHelpHost = this.GetTemplateChild(PartContextualHelpHost) as ContextualHelpHost;
			_textBox = this.GetTemplateChild(PartFormulaTextBox) as FormulaEditorTextBox;

			if (_contextualHelpHost != null)
				_contextualHelpHost.SetEditor(this);

			if (_textBox != null)
				_textBox.SetOwner(this);

			this.ChangeVisualState(false);
		}

		#endregion //OnApplyTemplate

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		/// <summary>
		/// Cancels the edit of the formula and reverts back to the current formula on the <see cref="Target"/> being edited.
		/// </summary>
		public abstract void CancelEdit();

		/// <summary>
		/// Commits the formula to the <see cref="Target"/> being edited.
		/// </summary>
		public abstract void CommitEdit();

		#region Redo

		/// <summary>
		/// Performs a redo operation if possible.
		/// </summary>
		/// <returns>True if the redo operation was performed; False otherwise.</returns>
		public bool Redo()
		{
			if (_textBox != null)
				return _textBox.RedoInternal();

			return false;
		}

		#endregion  // Redo

		#region Undo

		/// <summary>
		/// Performs a undo operation if possible.
		/// </summary>
		/// <returns>True if the undo operation was performed; False otherwise.</returns>
		public bool Undo()
		{
			if (_textBox != null)
				return _textBox.UndoInternal();

			return false;
		}

		#endregion  // Undo

		#endregion  // Public Methods

		#region Internal Methods

		#region OnCalculationManagerChanged

		internal virtual void OnCalculationManagerChanged()
		{
			if (this.CalculationManager == null)
			{
				_functions = null;
			}
			else
			{
				_functions = new List<FunctionInfo>();
				foreach (CalculationFunction function in this.CalculationManager.GetAllFunctions())
					_functions.Add(new FunctionInfo(this, function));
			}

			// This must be done after we cache the functions because the ContextualHelpHost.OnCalculationManagerChanged method
			// may use the functions.
			if (_contextualHelpHost != null)
				_contextualHelpHost.OnCalculationManagerChanged();
		}

		#endregion  // OnCalculationManagerChanged

		#region OnFormulaProviderReferenceChanged

		internal virtual void OnFormulaProviderReferenceChanged()
		{
			this.IsEnabled = _formulaProvider != null && _formulaProvider.Reference != null;

			if (_contextualHelpHost != null)
				_contextualHelpHost.OnFormulaProviderReferenceChanged();
		}

		#endregion  // OnFormulaProviderReferenceChanged

		#region OnTargetAttachedPropertyChanged

		internal void OnTargetAttachedPropertyChanged(string propertyName)
		{
			// If the CalculationManager attached property on the target has changed, OnTargetChanged should be called again so we 
			// re-get the formula provider.
			if (propertyName == "CalculationManager")
				this.OnTargetChanged(this.Target);
		}

		#endregion  // OnTargetAttachedPropertyChanged

		#endregion  // Internal Methods

		#region Private Methods

		// MD 10/21/11 - TFS93511
		#region AddCalculationManagerListener

		private void AddCalculationManagerListener()
		{
			if (_calculationManagerListenerAttached || _calculationManager == null)
				return;

			((ITypedSupportPropertyChangeNotifications<object, string>)_calculationManager).AddListener(this, true);
			_calculationManagerListenerAttached = true;
		}

		#endregion  // AddCalculationManagerListener

		#region ChangeVisualState

		private void ChangeVisualState(bool useTransitions)
		{
			if (this.IsEnabled == false)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDisabled, VisualStateUtilities.StateNormal);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

			if (this.HasSyntaxError)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateError, VisualStateUtilities.StateNoError);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateNoError, useTransitions);
		}

		#endregion // ChangeVisualState

		#region OnFormulaProviderPropertyChanged

		private void OnFormulaProviderPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Debug.Assert(_formulaProvider != null && _formulaProvider == sender, "We should have target here if the event is hooked.");

			if (_formulaProvider != null)
			{
				bool didCalculationManagerChange = false;
				bool didFormulaChange = false;
				bool didReferenceChange = false;

				switch (e.PropertyName)
				{
					case "":
						didCalculationManagerChange = true;
						didFormulaChange = true;
						didReferenceChange = true;
						break;

					case "CalculationManager":
						didCalculationManagerChange = true;
						break;

					case "Reference":
						didReferenceChange = true;
						break;

					case "Formula":
						didFormulaChange = true;
						break;

					default:
						Debug.Assert(false, "Unknown property name: " + e.PropertyName);
						break;
				}

				if (didCalculationManagerChange)
					this.CalculationManager = (XamCalculationManager)_formulaProvider.CalculationManager;

				if (didFormulaChange)
					this.Formula = _formulaProvider.Formula;

				if (didReferenceChange)
					this.OnFormulaProviderReferenceChanged();

				this.FormulaCompiler.Compile();
			}
		}

		#endregion  // OnFormulaProviderPropertyChanged

		#region OnIsEnabledChanged

		private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.ChangeVisualState(true);
		}

		#endregion  // OnIsEnabledChanged

		// MD 10/21/11 - TFS93511
		#region RemoveCalculationManagerListener

		private void RemoveCalculationManagerListener()
		{
			if (_calculationManagerListenerAttached == false || _calculationManager == null)
				return;

			((ITypedSupportPropertyChangeNotifications<object, string>)_calculationManager).RemoveListener(this);
			_calculationManagerListenerAttached = false;
		}

		#endregion  // RemoveCalculationManagerListener

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region CanRedo

		private static readonly DependencyPropertyKey CanRedoPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CanRedo",
			typeof(bool), typeof(FormulaEditorBase),
			false,
			new PropertyChangedCallback(OnCanRedoChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="CanRedo"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanRedoProperty = CanRedoPropertyKey.DependencyProperty;

		private static void OnCanRedoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorBase instance = (FormulaEditorBase)d;

			if (instance._redoCommand != null)
				instance._redoCommand.RaiseCanExecuteChanged();
		}

		/// <summary>
		/// Gets the value which indicates whether a redo operation can be performed.
		/// </summary>
		/// <seealso cref="CanRedoProperty"/>
		/// <seealso cref="RedoCommand"/>
		public bool CanRedo
		{
			get
			{
				return (bool)this.GetValue(FormulaEditorBase.CanRedoProperty);
			}
			internal set
			{
				this.SetValue(FormulaEditorBase.CanRedoPropertyKey, value);
			}
		}

		#endregion //CanRedo

		#region CanUndo

		private static readonly DependencyPropertyKey CanUndoPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CanUndo",
			typeof(bool), typeof(FormulaEditorBase), false, new PropertyChangedCallback(OnCanUndoChanged));

		/// <summary>
		/// Identifies the read-only <see cref="CanUndo"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanUndoProperty = CanUndoPropertyKey.DependencyProperty;

		private static void OnCanUndoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorBase instance = (FormulaEditorBase)d;

			if (instance._undoCommand != null)
				instance._undoCommand.RaiseCanExecuteChanged();
		}

		/// <summary>
		/// Gets the value indicating whether an undo operation can be performed.
		/// </summary>
		/// <seealso cref="CanUndoProperty"/>
		/// <seealso cref="UndoCommand"/>
		public bool CanUndo
		{
			get
			{
				return (bool)this.GetValue(FormulaEditorBase.CanUndoProperty);
			}
			internal set
			{
				this.SetValue(FormulaEditorBase.CanUndoPropertyKey, value);
			}
		}

		#endregion //CanUndo

		#region Formula

		/// <summary>
		/// Identifies the <see cref="Formula"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FormulaProperty = DependencyPropertyUtilities.Register("Formula",
			typeof(string), typeof(FormulaEditorBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnFormulaChanged))
			);

		private static void OnFormulaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorBase instance = (FormulaEditorBase)d;
			instance.OnFormulaChanged();
		}

		internal virtual void OnFormulaChanged()
		{
			this.FormulaCompiler.CompileAsync();
		}

		/// <summary>
		/// Gets or sets the formula being edited in the dialog.
		/// </summary>
		/// <seealso cref="FormulaProperty"/>
		public string Formula
		{
			get
			{
				return (string)this.GetValue(FormulaEditorBase.FormulaProperty);
			}
			set
			{
				this.SetValue(FormulaEditorBase.FormulaProperty, value);
			}
		}

		#endregion //Formula

		#region HasSyntaxError

		private static readonly DependencyPropertyKey HasSyntaxErrorPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("HasSyntaxError",
			typeof(bool), typeof(FormulaEditorBase), false, new PropertyChangedCallback(OnHasSyntaxErrorChanged));

		/// <summary>
		/// Identifies the read-only <see cref="HasSyntaxError"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasSyntaxErrorProperty = HasSyntaxErrorPropertyKey.DependencyProperty;

		private static void OnHasSyntaxErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorBase instance = (FormulaEditorBase)d;

			instance.ChangeVisualState(true);
		}

		/// <summary>
		/// Gets the value which indicates whether the formula has a syntax error.
		/// </summary>
		/// <seealso cref="SyntaxError"/>
		/// <seealso cref="HasSyntaxErrorProperty"/>
		public bool HasSyntaxError
		{
			get
			{
				return (bool)this.GetValue(FormulaEditorBase.HasSyntaxErrorProperty);
			}
			private set
			{
				this.SetValue(FormulaEditorBase.HasSyntaxErrorPropertyKey, value);
			}
		}

		#endregion //HasSyntaxError

		#region RedoCommand

		/// <summary>
		/// Gets the command which performs a redo operation.
		/// </summary>
		/// <seealso cref="CanRedo"/>
		public RedoFormulaEditCommand RedoCommand
		{
			get
			{
				if (_redoCommand == null)
					_redoCommand = new RedoFormulaEditCommand(this);

				return _redoCommand;
			}
		}

		#endregion  // RedoCommand

		#region ShowContextualHelp

		/// <summary>
		/// Identifies the <see cref="ShowContextualHelp"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowContextualHelpProperty = DependencyPropertyUtilities.Register("ShowContextualHelp",
			typeof(bool), typeof(FormulaEditorBase),
			DependencyPropertyUtilities.CreateMetadata(true)
			);

		/// <summary>
		/// Gets or sets the value indicating whether help should be shown to the user based on context while they are editing the formula.
		/// </summary>
		/// <seealso cref="ShowContextualHelpProperty"/>
		public bool ShowContextualHelp
		{
			get
			{
				return (bool)this.GetValue(FormulaEditorBase.ShowContextualHelpProperty);
			}
			set
			{
				this.SetValue(FormulaEditorBase.ShowContextualHelpProperty, value);
			}
		}

		#endregion //ShowContextualHelp

		#region SyntaxError

		private static readonly DependencyPropertyKey SyntaxErrorPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SyntaxError",
			typeof(string), typeof(FormulaEditorBase), null, new PropertyChangedCallback(OnSyntaxErrorChanged));

		/// <summary>
		/// Identifies the read-only <see cref="SyntaxError"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SyntaxErrorProperty = SyntaxErrorPropertyKey.DependencyProperty;

		private static void OnSyntaxErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorBase instance = (FormulaEditorBase)d;
			instance.HasSyntaxError = (e.NewValue != null);
		}

		/// <summary>
		/// Gets or sets the syntax error of the formula being edited or null if it is a valid formula.
		/// </summary>
		/// <seealso cref="SyntaxErrorProperty"/>
		/// <seealso cref="HasSyntaxError"/>
		public string SyntaxError
		{
			get
			{
				return (string)this.GetValue(FormulaEditorBase.SyntaxErrorProperty);
			}
			internal set
			{
				this.SetValue(FormulaEditorBase.SyntaxErrorPropertyKey, value);
			}
		}

		#endregion //SyntaxError

		#region Target

		/// <summary>
		/// Identifies the <see cref="Target"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TargetProperty = DependencyPropertyUtilities.Register("Target",
			typeof(object), typeof(FormulaEditorBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnTargetChanged))
			);

		private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormulaEditorBase instance = (FormulaEditorBase)d;
			instance.OnTargetChanged(e.OldValue);
		}

		internal virtual void OnTargetChanged(object oldTarget)
		{
			object target = this.Target;
			if (target != oldTarget)
			{
				if (_targetAttachedPropertyListener != null)
					XamCalculationManager.AttachedPropertyListeners.Remove(_targetAttachedPropertyListener);

				if (target != null)
				{
					_targetAttachedPropertyListener = new TargetAttachedPropertyListener(this, target);
					XamCalculationManager.AttachedPropertyListeners.Add(_targetAttachedPropertyListener, true);
				}
				else
				{
					_targetAttachedPropertyListener = null;
				}
			}

			IFormulaProvider newFormulaProvider = XamCalculationManager.GetFormulaProvider(this.Target);

			if (newFormulaProvider == _formulaProvider)
				return;

			if (_formulaProvider != null)
				_formulaProvider.PropertyChanged -= new PropertyChangedEventHandler(this.OnFormulaProviderPropertyChanged);

			_formulaProvider = newFormulaProvider;

			if (_formulaProvider == null)
			{
				this.CalculationManager = null;
				this.Formula = null;
				this.IsEnabled = false;
			}
			else
			{
				this.CalculationManager = (XamCalculationManager)_formulaProvider.CalculationManager;
				this.Formula = _formulaProvider.Formula;
				this.IsEnabled = _formulaProvider.Reference != null;
				_formulaProvider.PropertyChanged += new PropertyChangedEventHandler(this.OnFormulaProviderPropertyChanged);

				this.FormulaCompiler.CompileAsync();
			}

			if (_textBox != null)
			{
				// MD 11/4/11
				// Found while fixing TFS95193
				// Instead of just clearing the history, we should reinitialize the entire history so it also re-caches the
				// initial state of the text box.
				//_textBox.ClearUndoHistory();
				_textBox.ReinitializeUndoHistory();
			}
		}

		/// <summary>
		/// Gets or sets the object having its formula edited.
		/// </summary>
		/// <seealso cref="TargetProperty"/>
		public object Target
		{
			get
			{
				return this.GetValue(FormulaEditorBase.TargetProperty);
			}
			set
			{
				this.SetValue(FormulaEditorBase.TargetProperty, value);
			}
		}

		#endregion //Target

		#region UndoCommand

		/// <summary>
		/// Gets the command which performs an undo operation.
		/// </summary>
		/// <seealso cref="CanUndo"/>
		public UndoFormulaEditCommand UndoCommand
		{
			get
			{
				if (_undoCommand == null)
					_undoCommand = new UndoFormulaEditCommand(this);

				return _undoCommand;
			}
		}

		#endregion  // UndoCommand

		#endregion  // Public Properties

		#region Internal Properties

		#region CalculationManager

		internal XamCalculationManager CalculationManager
		{
			get { return _calculationManager; }
			set
			{
				if (_calculationManager == value)
					return;

				// MD 10/21/11 - TFS93511
				// We may have a listener attached to the old calculation manager, so make sure we remove it.
				this.RemoveCalculationManagerListener();

				_calculationManager = value;

				// MD 10/21/11 - TFS93511
				// If the calculation manager is not initialized, don't call OnCalculationManagerChanged yet because that will
				// get the functions and operands. We should add a property change listener so we can call OnCalculationManagerChanged
				// when IsInitialized changes to True.
				//this.OnCalculationManagerChanged();
				if (_calculationManager == null || _calculationManager.IsInitialized)
				{
					// MD 5/15/12 - TFS111115
					_isCalculationManagerInitialized = true;

					this.OnCalculationManagerChanged();
				}
				// MD 5/15/12 - TFS111115
				// We always want to listen to changes to the calc manager after it is initialized.
				//else
				//    this.AddCalculationManagerListener();
				this.AddCalculationManagerListener();
			}
		}

		#endregion  // CalculationManager

		#region ContextualHelpHost

		internal ContextualHelpHost ContextualHelpHost
		{
			get { return _contextualHelpHost; }
		}

		#endregion // ContextualHelpHost

		#region Functions

		internal List<FunctionInfo> Functions
		{
			get { return _functions; }
		}

		#endregion  // Functions

		#region FormulaProvider

		internal IFormulaProvider FormulaProvider
		{
			get { return _formulaProvider; }
		}

		#endregion  // FormulaProvider

		#region TextBox

		internal FormulaEditorTextBox TextBox
		{
			get { return _textBox; }
		}

		#endregion  // TextBox

		#endregion  // Internal Properties

		#region Private Properties

		#region FormulaCompiler

		private FormulaCompiler FormulaCompiler
		{
			get
			{
				if (_formulaCompiler == null)
					_formulaCompiler = new FormulaCompiler(this);

				return _formulaCompiler;
			}
		}

		#endregion  // FormulaCompiler

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