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
using System.ComponentModel;

namespace Infragistics
{
	/// <summary>
	/// A base class for operands that will be used in the Summary framework which contains information 
	/// which is needed for display.
	/// </summary>
	public abstract class SummaryOperandBase : DependencyObjectNotifier
	{
		#region Members

		bool _isApplied;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryOperandBase"/> class.
		/// </summary>
		protected SummaryOperandBase()
		{
		}

		#endregion // Constructor

		#region Properties

		#region Abstract

		/// <summary>
		/// Get's the default text that will be displayed in the drop down for this <see cref="SummaryOperandBase"/>
		/// </summary>
		protected abstract string DefaultSelectionDisplayLabel { get; }

		/// <summary>
		/// Get's the default text that will be displayed in a SummaryRow when this <see cref="SummaryOperandBase"/> is selected.
		/// </summary>
		protected abstract string DefaultRowDisplayLabel { get; }

		#endregion // Abstract

		#region Public

		#region SelectionDisplayLabel

		/// <summary>
		/// Identifies the <see cref="SelectionDisplayLabel"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SelectionDisplayLabelProperty = DependencyProperty.Register("SelectionDisplayLabel", typeof(string), typeof(SummaryOperandBase), new PropertyMetadata(new PropertyChangedCallback(SelectionDisplayLabelChanged)));

		/// <summary>
		/// Gets the string that will be displayed in the drop down for this <see cref="SummaryOperandBase"/>
		/// </summary>
		public string SelectionDisplayLabel
		{
			get { return (string)this.GetValue(SelectionDisplayLabelProperty); }
			set { this.SetValue(SelectionDisplayLabelProperty, value); }
		}

		private static void SelectionDisplayLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryOperandBase soe = (SummaryOperandBase)obj;
			soe.OnPropertyChanged("SelectionDisplayLabel");
		}

		#endregion // SelectionDisplayLabel

		#region SelectionDisplayLabelResolved

		/// <summary>
		/// Gets the value that will be displayed in the SummaryRow.
		/// </summary>
		public string SelectionDisplayLabelResolved
		{
			get
			{
				if (string.IsNullOrEmpty(this.SelectionDisplayLabel))
					return this.DefaultSelectionDisplayLabel;
				else
					return this.SelectionDisplayLabel;
			}
		}

		#endregion // RowDisplayLabelResolved

		#region RowDisplayLabel

		/// <summary>
		/// Identifies the <see cref="RowDisplayLabel"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowDisplayLabelProperty = DependencyProperty.Register("RowDisplayLabel", typeof(string), typeof(SummaryOperandBase), new PropertyMetadata(new PropertyChangedCallback(RowDisplayLabelChanged)));

		/// <summary>
		/// Gets / sets the string that will be displayed in the SummaryRow when this <see cref="SummaryOperandBase"/> is selected.
		/// </summary>
		public string RowDisplayLabel
		{
			get { return (string)this.GetValue(RowDisplayLabelProperty); }
			set { this.SetValue(RowDisplayLabelProperty, value); }
		}

		private static void RowDisplayLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryOperandBase soe = (SummaryOperandBase)obj;
			soe.OnPropertyChanged("RowDisplayLabel");
		}

		#endregion // RowDisplayLabel

		#region RowDisplayLabelResolved

		/// <summary>
		/// Gets the value that will be displayed in the SummaryRow.
		/// </summary>
		public string RowDisplayLabelResolved
		{
			get
			{
				if (string.IsNullOrEmpty(this.RowDisplayLabel))
					return this.DefaultRowDisplayLabel;
				else
					return this.RowDisplayLabel;
			}
		}

		#endregion // RowDisplayLabelResolved

		#region FormatString

		/// <summary>
		/// Identifies the <see cref="FormatString"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(SummaryOperandBase), new PropertyMetadata(new PropertyChangedCallback(FormatStringChanged)));

		/// <summary>
		/// Gets/Sets the format string that will be applied the value of this summary. 
		/// </summary>
		public string FormatString
		{
			get { return (string)this.GetValue(FormatStringProperty); }
			set { this.SetValue(FormatStringProperty, value); }
		}

		private static void FormatStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryOperandBase soe = (SummaryOperandBase)obj;
			soe.OnPropertyChanged("FormatString");
		}

		#endregion // FormatString

		#region SummaryCalculator
		/// <summary>
		/// Gets the <see cref="SummaryCalculator"/> which will be used to calculate the summary.
		/// </summary>
		public virtual SummaryCalculatorBase SummaryCalculator
		{
			get
			{
				return null;
			}
		}

		#endregion // SummaryCalculator

		#region LinqSummaryOperatorValue

		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>						
		public virtual LinqSummaryOperator? LinqSummaryOperator
		{
			get
			{
				return null;
			}
		}

		#endregion  // LinqSummaryOperatorValue

		#region IsApplied

		/// <summary>
		/// Gets / sets if the summary should processed for this summary operand.
		/// </summary>
		public virtual bool IsApplied
		{
			get
			{
				return this._isApplied;
			}
			set
			{
				if (this._isApplied != value)
				{
					this._isApplied = value;
					this.OnPropertyChanged("IsApplied");
				}
			}
		}

		#endregion // IsApplied

		#endregion // Public

		#endregion // Properties
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