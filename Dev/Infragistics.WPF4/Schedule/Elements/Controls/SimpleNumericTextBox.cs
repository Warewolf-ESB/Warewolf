using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Simple Numeric TextBox control for positive integers.  Performs simple range editing via MinValue and MaxValue properties (for internal use only)
	/// </summary>
	[DesignTimeVisible(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SimpleNumericTextBox : TextBox
	{
		#region Member Variables

		private bool				_valueSetDuringOnTextChanged;
		private int					_minValue;
		private int					_maxValue;
		private string				_lastText = string.Empty;

		#endregion //Member variables

		#region Constructor
		/// <summary>
		/// Creates an instance of SimpleNumericTextbox.
		/// </summary>
		public SimpleNumericTextBox()
		{
			this.Text		= "0";

			// Hooking the TextChanged event rather than overriding OnTextChanged since SL does
			// not expose OntTextChanged
			this.TextChanged += new TextChangedEventHandler(NumericTextBox_TextChanged);

			this._minValue	= 0;
			this._maxValue	= int.MaxValue;
		}
		#endregion //Constructor

		#region Event Handlers
		void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			int  result;
			if (int.TryParse(this.Text, out result))
			{
				if (result < this._minValue)
				{
					int pos				= this.SelectionStart;
					this.Text			= this._lastText;
					this.SelectionStart	= pos;
					
					return;
				}

				if (result > this._maxValue)
				{
					int pos				= this.SelectionStart;
					this.Text			= this._lastText;
					this.SelectionStart	= pos;
					
					return;
				}

				this._lastText						= this.Text;

				this._valueSetDuringOnTextChanged	= true;
				this.Value							= result;
				this._valueSetDuringOnTextChanged	= false;

				this.RaiseValueChanged();
			}
		}
		#endregion //Event Handlers

		#region Base Class Overrides
		/// <summary>
		/// Called when System.Windows.UIElement.KeyDown event occurs.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// Only allow numeric keys and other valid keys.
			if ((e.Key >= Key.D0		&& e.Key <= Key.D9)			||
				(e.Key >= Key.NumPad0	&& e.Key <= Key.NumPad9)	|| 
				e.Key == Key.Back									||
				e.Key == Key.Delete									||
				e.Key == Key.Left									||
				e.Key == Key.Right									||
				e.Key == Key.Insert									||
				e.Key == Key.Tab)	// JM 04-25-11 TFS73427 Added.
				base.OnKeyDown(e);
			else
				e.Handled = true;
		}
		#endregion //Base Class Overrides

		#region Properties

		#region MinValue
		/// <summary>
		/// Returns/sets the minimum value allowed.
		/// </summary>
		public int MinValue
		{
			get { return this._minValue; }
			set
			{
				// Only support positive number ranges.
				if (value < 0)
					this._minValue = 0;
				else
					this._minValue = value;

				if (this.Value < this._minValue)
					this.Value = this._minValue;
			}
		}
		#endregion //MinValue

		#region MaxValue
		/// <summary>
		/// Returns/sets the maximum value allowed.
		/// </summary>
		public int MaxValue
		{
			get { return this._maxValue; }
			set
			{
				// Only support positive number ranges.
				if (value < 0)
					this._maxValue = 0;
				else
					this._maxValue = value;

				if (this.Value > this._maxValue)
					this.Value = this._maxValue;
			}
		}
		#endregion //MaxValue

		#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyPropertyUtilities.Register("Value",
			typeof(int), typeof(SimpleNumericTextBox),
			DependencyPropertyUtilities.CreateMetadata((int)0, new PropertyChangedCallback(OnValueChanged)));

		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleNumericTextBox t = d as SimpleNumericTextBox;
			if (false == t._valueSetDuringOnTextChanged)
				t.Text = e.NewValue.ToString();
		}

		/// <summary>
		/// Returns or sets the integer Value of the control
		/// </summary>
		/// <seealso cref="ValueProperty"/>
		public int Value
		{
			get
			{
				return (int)this.GetValue(SimpleNumericTextBox.ValueProperty);
			}
			set
			{
				this.SetValue(SimpleNumericTextBox.ValueProperty, value);
			}
		}

		#endregion //Value

		#endregion //Properties

		#region Events

		#region ValueChanged
		internal event System.EventHandler ValueChanged;
		private void RaiseValueChanged()
		{
			if (ValueChanged != null)
				ValueChanged(this, EventArgs.Empty);
		}
		#endregion ValueChanged

		#endregion //Events
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