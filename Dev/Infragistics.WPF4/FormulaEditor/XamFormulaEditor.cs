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


using Infragistics.Windows.Licensing;


using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Calculations;
using Infragistics.Controls.Interactions.Primitives;
using System.Windows.Data;
using Infragistics.Calculations.Engine;
using System.Windows.Documents;
using Infragistics.Controls.Menus;

namespace Infragistics.Controls.Interactions
{
	/// <summary>
	/// An editor for viewing and editing a formula in a <see cref="XamCalculationManager"/>.
	/// </summary>
	[TemplatePart(Name = PartErrorIndicator, Type = typeof(FrameworkElement))] // MD 5/8/12 - TFS104497

	
	

	public class XamFormulaEditor : FormulaEditorBase
	{
		#region Constants

		// MD 5/8/12 - TFS104497
		private const string PartErrorIndicator = "PART_ErrorIndicator";

		#endregion  // Constants

		#region Member Variables


		private UltraLicense _license;


		private FormulaEditorDialog _currentDialog;

		// MD 11/4/11 - TFS95193
		// The dialog will now manage the bindings.
		//private Binding _dialogFormulaBinding;
		//private Binding _dialogShowContextualHelpBinding;
		//private Binding _dialogTargetBinding;

		// MD 5/8/12 - TFS104497
		private FrameworkElement _errorIndicator;

		// MD 1/5/12 - TFS96832
		private Dictionary<string, string> _localizedStrings;

		#endregion //Member Variables

		#region Constructor

		static XamFormulaEditor()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamFormulaEditor), new FrameworkPropertyMetadata(typeof(XamFormulaEditor)));

		}

		/// <summary>
		/// Initializes a new <see cref="XamFormulaEditor" />.
		/// </summary>
		public XamFormulaEditor()
		{


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

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
				this._license = LicenseManager.Validate(typeof(XamFormulaEditor), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

		}

		#endregion //Constructor

		#region Base Class Overrides

		#region CancelEdit

		/// <summary>
		/// Cancels the edit of the formula and reverts back to the current formula on the <see cref="FormulaEditorBase.Target"/> being edited.
		/// </summary>
		public override void CancelEdit()
		{
			if (this.FormulaProvider != null)
				this.Formula = this.FormulaProvider.Formula;
		}

		#endregion  // CancelEdit

		#region CommitEdit

		/// <summary>
		/// Commits the formula to the <see cref="FormulaEditorBase.Target"/> being edited.
		/// </summary>
		public override void CommitEdit()
		{
			if (this.FormulaProvider != null)
				this.FormulaProvider.Formula = this.Formula;
		}

		#endregion  // CommitEdit

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			// MD 5/8/12 - TFS104497
			if (_errorIndicator != null)
			{
				_errorIndicator.MouseLeftButtonDown -= new MouseButtonEventHandler(this.OnErrorIndicatorMouseLeftButtonDown);
				_errorIndicator.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnErrorIndicatorMouseLeftButtonUp);
			}

			if (this.TextBox != null)
			{
				this.TextBox.KeyDown -= new KeyEventHandler(this.OnFormulaEditorTextBoxKeyDown);
				this.TextBox.LostFocus -= new RoutedEventHandler(this.OnFormulaEditorTextBoxLostFocus);
			}

			base.OnApplyTemplate();

			// MD 5/8/12 - TFS104497
			_errorIndicator = this.GetTemplateChild(PartErrorIndicator) as FrameworkElement;
			if (_errorIndicator != null)
			{
				_errorIndicator.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnErrorIndicatorMouseLeftButtonDown);
				_errorIndicator.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnErrorIndicatorMouseLeftButtonUp);


				// We want the user to just be able to tap the error indicator to show the message, so we must disable 
				// press and hold.
				Stylus.SetIsPressAndHoldEnabled(_errorIndicator, false);

			}

			if (this.TextBox != null)
			{
				this.TextBox.KeyDown += new KeyEventHandler(this.OnFormulaEditorTextBoxKeyDown);
				this.TextBox.LostFocus += new RoutedEventHandler(this.OnFormulaEditorTextBoxLostFocus);
			}
		}

		#endregion //OnApplyTemplate

		#endregion  // Base Class Overrides

		#region Events

		#region FormulaEditorDialogClosing

		/// <summary>
		/// Invokes the <see cref="FormulaEditorDialogClosing"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		/// <seealso cref="FormulaEditorDialogClosing"/>
		/// <seealso cref="FormulaEditorDialogClosingEventArgs"/>
		protected virtual void OnFormulaEditorDialogClosing(FormulaEditorDialogClosingEventArgs e)
		{
			var handler = this.FormulaEditorDialogClosing;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs when the <see cref="FormulaEditorDialog"/> is about to close.
		/// </summary>
		public event EventHandler<FormulaEditorDialogClosingEventArgs> FormulaEditorDialogClosing;

		#endregion  // FormulaEditorDialogClosing

		#region FormulaEditorDialogDisplaying

		/// <summary>
		/// Invokes the <see cref="FormulaEditorDialogDisplaying"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		/// <seealso cref="FormulaEditorDialogDisplaying"/>
		/// <seealso cref="FormulaEditorDialogDisplayingEventArgs"/>
		protected virtual void OnFormulaEditorDialogDisplaying(FormulaEditorDialogDisplayingEventArgs e)
		{
			var handler = this.FormulaEditorDialogDisplaying;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs when the <see cref="FormulaEditorDialog"/> is about to be displayed.
		/// </summary>
		public event EventHandler<FormulaEditorDialogDisplayingEventArgs> FormulaEditorDialogDisplaying;

		#endregion  // FormulaEditorDialogDisplaying

		#endregion  // Events

		#region Methods

		#region Static

		#region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

		#endregion // RegisterResources

		#region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

		#endregion // UnregisterResources

		#endregion // Static

		#region Public Methods

		#region DisplayFormulaEditorDialog

		/// <summary>
		/// Displays a dialog which gives the user more assistance in editing the formula. 
		/// </summary>
		/// <see cref="FormulaEditorDialog"/>
		public void DisplayFormulaEditorDialog()
		{
			if (_currentDialog != null)
			{
				DialogManager.ActivateDialog(_currentDialog);
				return;
			}

			// MD 11/4/11 - TFS95193
			// Give the dialog a reference to this editor.
			//_currentDialog = new FormulaEditorDialog();
			_currentDialog = new FormulaEditorDialog(this);

			// MD 11/4/11 - TFS95193
			// The dialog will now manage the bindings.
			//_dialogTargetBinding = new Binding("Target");
			//_dialogTargetBinding.Source = this;
			//_dialogTargetBinding.Mode = BindingMode.TwoWay;
			//BindingOperations.SetBinding(_currentDialog, FormulaEditorDialog.TargetProperty, _dialogTargetBinding);
			//
			//_dialogFormulaBinding = new Binding("Formula");
			//_dialogFormulaBinding.Source = this;
			//_dialogFormulaBinding.Mode = BindingMode.TwoWay;
			//BindingOperations.SetBinding(_currentDialog, FormulaEditorDialog.FormulaProperty, _dialogFormulaBinding);
			//
			//_dialogShowContextualHelpBinding = new Binding("ShowContextualHelp");
			//_dialogShowContextualHelpBinding.Source = this;
			//_dialogShowContextualHelpBinding.Mode = BindingMode.TwoWay;
			//BindingOperations.SetBinding(_currentDialog, FormulaEditorDialog.ShowContextualHelpProperty, _dialogShowContextualHelpBinding);

			FormulaEditorDialogDisplayingEventArgs e = new FormulaEditorDialogDisplayingEventArgs(_currentDialog);
			this.OnFormulaEditorDialogDisplaying(e);
			if (e.Cancel)
			{
				this.DialogClosedCallback(null);
				return;
			}

			_currentDialog.DisplayAsDialog(this, new Size(800, 700), true, this.DialogClosingCallback, this.DialogClosedCallback);
		}

		#endregion  // DisplayFormulaEditorDialog

		#endregion  // Public Methods

		#region Private Methods

		#region DialogClosedCallback

		private void DialogClosedCallback(bool? result)
		{
			if (_currentDialog == null)
				return;

			// MD 11/4/11 - TFS95193
			// The dialog will now manage the bindings.
			//_currentDialog.ClearValue(FormulaEditorDialog.FormulaProperty);
			//_dialogFormulaBinding = null;
			//
			//_currentDialog.ClearValue(FormulaEditorDialog.ShowContextualHelpProperty);
			//_dialogShowContextualHelpBinding = null;
			//
			//_currentDialog.ClearValue(FormulaEditorDialog.TargetProperty);
			//_dialogTargetBinding = null;

			_currentDialog = null;
		}

		#endregion  // DialogClosedCallback

		#region DialogClosingCallback

		private bool DialogClosingCallback()
		{
			FormulaEditorDialogClosingEventArgs e = new FormulaEditorDialogClosingEventArgs(_currentDialog);
			this.OnFormulaEditorDialogClosing(e);
			return e.Cancel;
		}

		#endregion  // DialogClosingCallback

		// MD 5/8/12 - TFS104497
		#region OnErrorIndicatorMouseLeftButtonDown

		private void OnErrorIndicatorMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ToolTip tooltip = ToolTipService.GetToolTip(_errorIndicator) as ToolTip;
			if (tooltip != null)
				tooltip.IsOpen = true;
		}

		#endregion // OnErrorIndicatorMouseLeftButtonDown

		// MD 5/8/12 - TFS104497
		#region OnErrorIndicatorMouseLeftButtonUp

		private void OnErrorIndicatorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ToolTip tooltip = ToolTipService.GetToolTip(_errorIndicator) as ToolTip;
			if (tooltip != null)
			{
				// Leave the tooltip open for 5 seconds after the left mouse button is released.
				DispatcherTimer timer = new DispatcherTimer();
				timer.Interval = TimeSpan.FromSeconds(5);
				timer.Tick += (sender2, e2) =>
					{
						tooltip.IsOpen = false;
						timer.Stop();
					};
				timer.Start();
			}
		}

		#endregion // OnErrorIndicatorMouseLeftButtonUp

		#region OnFormulaEditorTextBoxKeyDown

		private void OnFormulaEditorTextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Handled)
				return;

			switch (PresentationUtilities.GetKey(e))
			{
				case Key.Enter:
					if (Keyboard.Modifiers == ModifierKeys.Alt)
					{
						if (this.TextBox == null)
							return;

						this.TextBox.SetSelectedText(Environment.NewLine);

						TextSelection selection = this.TextBox.Selection;
						selection.Select(selection.End, selection.End);
					}
					else
					{
						this.CommitEdit();
					}

					e.Handled = true;
					break;

				case Key.Escape:
					this.CancelEdit();
					e.Handled = true;
					break;

					// MD 1/5/12 - TFS96832
					// Added shortcuts to display the dialog
					// Ctrl+F3 or Shift+F3
				case Key.F3:
					if (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Control)
					{
						this.DisplayFormulaEditorDialog();
						e.Handled = true;
					}
					break;
			}
		}

		#endregion  // OnFormulaEditorTextBoxKeyDown

		#region OnFormulaEditorTextBoxLostFocus

		private void OnFormulaEditorTextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			this.CommitEdit();
		}

		#endregion  // OnFormulaEditorTextBoxLostFocus

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		// MD 1/5/12 - TFS96832
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

					FormulaEditorUtilities.AddLocalizedString(_localizedStrings, "ShowDialogButtonToolTip");
				}

				return this._localizedStrings;
			}
		}

		#endregion //LocalizedStrings

		#region MaxLineCount

		/// <summary>
		/// Identifies the <see cref="MaxLineCount"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxLineCountProperty = DependencyPropertyUtilities.Register("MaxLineCount",
			typeof(int), typeof(XamFormulaEditor),
			DependencyPropertyUtilities.CreateMetadata(1)
			);

		/// <summary>
		/// Gets or sets the maximum number of lines to display, or zero or less to display as many lines as possible.
		/// </summary>
		/// <seealso cref="MaxLineCountProperty"/>
		/// <seealso cref="MinLineCount"/>
		public int MaxLineCount
		{
			get
			{
				return (int)this.GetValue(XamFormulaEditor.MaxLineCountProperty);
			}
			set
			{
				this.SetValue(XamFormulaEditor.MaxLineCountProperty, value);
			}
		}

		#endregion //MaxLineCount

		#region MinLineCount

		/// <summary>
		/// Identifies the <see cref="MinLineCount"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinLineCountProperty = DependencyPropertyUtilities.Register("MinLineCount",
			typeof(int), typeof(XamFormulaEditor),
			DependencyPropertyUtilities.CreateMetadata(1)
			);

		/// <summary>
		/// Gets or sets the minimum number of lines to display, or zero or less to display a minimum of one line.
		/// </summary>
		/// <seealso cref="MinLineCountProperty"/>
		/// <seealso cref="MaxLineCount"/>
		public int MinLineCount
		{
			get
			{
				return (int)this.GetValue(XamFormulaEditor.MinLineCountProperty);
			}
			set
			{
				this.SetValue(XamFormulaEditor.MinLineCountProperty, value);
			}
		}

		#endregion //MinLineCount

		#endregion  // Public Properties

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