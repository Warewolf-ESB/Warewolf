using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// TextBox that automatically transitions from read-only mode to edit mode when clicked.  Transitions back when Enter or Escape are pressed or focus is lost.  (for internal use only)
	/// </summary>
	[TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
	[TemplatePart(Name = PartTextBlock, Type = typeof(TextBlock))]
	public class DualModeTextBox : Control
	{
		#region Member Variables

		// Template part names
		private const string PartTextBox				= "TextBox";
		private const string PartTextBlock				= "TextBlock";

		private bool							_initialized;
		private TextBox							_textBox;
		private TextBlock						_textBlock;
		private bool							_ignoreIsInEditModeChange;

		#endregion //Member Variables

		#region Constructors
		static DualModeTextBox()
		{

			DualModeTextBox.DefaultStyleKeyProperty.OverrideMetadata(typeof(DualModeTextBox), new FrameworkPropertyMetadata(typeof(DualModeTextBox)));

		}

		/// <summary>
		/// Creates an instance of the DualModeTextBox.  For internal use only.
		/// </summary>
		public DualModeTextBox()
		{



		}
		#endregion //Constructors

		#region Base Class Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// Initialize.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.Initialize));
		}
		#endregion //OnApplyTemplate

		#endregion //Base Class Overrides

		#region Properties

		#region IsEditingAllowed
		/// <summary>
		/// Identifies the <see cref="IsEditingAllowed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsEditingAllowedProperty = DependencyPropertyUtilities.Register("IsEditingAllowed",
			typeof(bool), typeof(DualModeTextBox),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsEditingAllowedChanged)));

		private static void OnIsEditingAllowedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DualModeTextBox control = d as DualModeTextBox;
			if (null != control)
			{
				if ((bool)e.NewValue == false)
					control.IsInEditMode = false;
			}
		}

		/// <summary>
		/// Returns or sets whether the control supports edit mode.
		/// </summary>
		public bool IsEditingAllowed
		{
			get { return (bool)this.GetValue(DualModeTextBox.IsEditingAllowedProperty); }
			set { this.SetValue(DualModeTextBox.IsEditingAllowedProperty, value); }
		}
		#endregion //IsEditingAllowed

		#region IsInEditMode
		/// <summary>
		/// Identifies the <see cref="IsInEditMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInEditModeProperty = DependencyPropertyUtilities.Register("IsInEditMode",
			typeof(bool), typeof(DualModeTextBox),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsInEditModeChanged)));

		private static void OnIsInEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DualModeTextBox control = d as DualModeTextBox;
			if (null != control)
			{
				if (false == control._ignoreIsInEditModeChange)
				{
					if ((bool)e.NewValue == true)
						control.GoToEditMode();
					else
						control.GoToReadOnlyMode(true);
				}
			}
		}

		/// <summary>
		/// Returns or sets whether the control is in edit mode.
		/// </summary>
		public bool IsInEditMode
		{
			get { return (bool)this.GetValue(DualModeTextBox.IsInEditModeProperty); }
			set { this.SetValue(DualModeTextBox.IsInEditModeProperty, value); }
		}
		#endregion //IsInEditMode

		#region Text
		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyPropertyUtilities.Register("Text",
			typeof(string), typeof(DualModeTextBox),
			DependencyPropertyUtilities.CreateMetadata(string.Empty, new PropertyChangedCallback(OnTextChanged)));

		private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DualModeTextBox control = d as DualModeTextBox;
			if (null != control)
			{
			}
		}

		/// <summary>
		/// Returns or sets the text associated with the control.
		/// </summary>
		public string Text
		{
			get { return (string)this.GetValue(DualModeTextBox.TextProperty); }
			set { this.SetValue(DualModeTextBox.TextProperty, value); }
		}
		#endregion //Text

		#endregion //Properties

		#region Methods

		#region GoToEditMode
		private bool GoToEditMode()
		{
			if (false == this.IsEditingAllowed)
			{
				this.IsInEditMode		= false;
				return false;
			}

			this._textBox.MinWidth		= this._textBlock.ActualWidth;
			this._textBox.Text			= this.Text;
			this._textBox.Visibility	= Visibility.Visible;
			this._textBox.Focus();

			this._ignoreIsInEditModeChange	= true;
			this.IsInEditMode				= true;
			this._ignoreIsInEditModeChange	= false;

			return true;
		}
		#endregion //GoToEditMode

		#region GoToReadOnlyMode
		private void GoToReadOnlyMode(bool saveEditedText)
		{
			this._textBox.Visibility = Visibility.Collapsed;

			if (saveEditedText)
				this.Text = this._textBox.Text;

			this._ignoreIsInEditModeChange	= true;
			this.IsInEditMode				= false;
			this._ignoreIsInEditModeChange	= false;
		}
		#endregion //GoToReadOnlyMode

		#region Initialize
		private void Initialize()
		{
			if (true == this._initialized)
			{
			}
			
			// Find the parts we need.
			//
			// The TextBox.
			this._textBox = this.GetTemplateChild(PartTextBox) as TextBox;
			if (null != this._textBox)
			{
				this._textBox.KeyUp		+=new KeyEventHandler(OnTextBoxKeyUp);
				this._textBox.LostFocus	+=new RoutedEventHandler(OnTextBoxLostFocus);
				this._textBox.GotFocus += new RoutedEventHandler(OnTextBoxGotFocus);
			}

			// The TextBlock.
			this._textBlock = this.GetTemplateChild(PartTextBlock) as TextBlock;
			if (null != this._textBlock)
			{
			}

			this._initialized = true;
		}
		#endregion //Initialize

		#region OnTextBoxGotFocus
		void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
		{
			this._textBox.SelectAll();
		}
		#endregion //OnTextBoxGotFocus

		#region OnTextBoxLostFocus
		void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			if (true == this.IsInEditMode)
				this.GoToReadOnlyMode(true);
		}
		#endregion //OnTextBoxLostFocus

		#region OnTextBoxKeyUp
		void OnTextBoxKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && true == this.IsInEditMode)
			{
				this.GoToReadOnlyMode(false);
				e.Handled = true;
			}
			else
			if (e.Key == Key.Enter && true == this.IsInEditMode)
			{
				this.GoToReadOnlyMode(true);
				e.Handled = true;
			}
		}
		#endregion //OnTextBoxKeyUp

		#endregion //Methods
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