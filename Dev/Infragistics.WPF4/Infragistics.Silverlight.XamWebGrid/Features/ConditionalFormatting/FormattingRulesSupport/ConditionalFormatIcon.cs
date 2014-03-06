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

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A class representing the rule and the icon that will be displayed when the rule is valid.
	/// </summary>
	public class ConditionalFormatIcon : DependencyObjectNotifier
	{
		#region Parent

		/// <summary>
		/// Gets / sets the <see cref="IconConditionalFormatRule"/> which this indiviual rule belongs to.
		/// </summary>
		protected internal IconConditionalFormatRule Parent { get; set; }

		#endregion // Parent

		#region Icon

		/// <summary>
		/// Identifies the <see cref="Icon"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(DataTemplate), typeof(ConditionalFormatIcon), new PropertyMetadata(new PropertyChangedCallback(IconChanged)));

		/// <summary>
		/// Gets / sets the <see cref="DataTemplate"/> which will act as the icon when this rule is applied.
		/// </summary>
		public DataTemplate Icon
		{
			get { return (DataTemplate)this.GetValue(IconProperty); }
			set { this.SetValue(IconProperty, value); }
		}

		private static void IconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormatIcon icon = (ConditionalFormatIcon)obj;
			icon.OnPropertyChanged("Icon");
		}

		#endregion // Icon

		#region Operator

		/// <summary>
		/// Identifies the <see cref="Operator"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty OperatorProperty = DependencyProperty.Register("Operator", typeof(IconGroupOperator), typeof(ConditionalFormatIcon), new PropertyMetadata(IconGroupOperator.GreaterThanOrEqualTo, new PropertyChangedCallback(OperatorChanged)));

		/// <summary>
		/// Gets / sets the <see cref="IconGroupOperator"/> which this rule uses for evaluation.
		/// </summary>
		public IconGroupOperator Operator
		{
			get { return (IconGroupOperator)this.GetValue(OperatorProperty); }
			set { this.SetValue(OperatorProperty, value); }
		}

		private static void OperatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormatIcon icon = (ConditionalFormatIcon)obj;
			icon.OnPropertyChanged("Operator");
		}

		#endregion // Operator

		#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(ConditionalFormatIcon), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

		/// <summary>
		/// Gets / sets the value that this rule will use in conjunction with the <see cref="Operator"/> to evaluate the rule.
		/// </summary>
		public double Value
		{
			get { return (double)this.GetValue(ValueProperty); }
			set { this.SetValue(ValueProperty, value); }
		}

		private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormatIcon icon = (ConditionalFormatIcon)obj;
			icon.OnPropertyChanged("Value");
		}

		#endregion // Value

		#region ValueType

		/// <summary>
		/// Identifies the <see cref="ValueType"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register("ValueType", typeof(IconRuleValueType), typeof(ConditionalFormatIcon), new PropertyMetadata(new PropertyChangedCallback(ValueTypeChanged)));

		/// <summary>
		/// Gets / sets what the value represents.
		/// </summary>
		public IconRuleValueType ValueType
		{
			get { return (IconRuleValueType)this.GetValue(ValueTypeProperty); }
			set { this.SetValue(ValueTypeProperty, value); }
		}

		private static void ValueTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormatIcon icon = (ConditionalFormatIcon)obj;
			icon.OnPropertyChanged("ValueType");
		}

		#endregion // ValueType

		#region Methods

		#region PassesConditions
		/// <summary>
		/// Evaluates the condition
		/// </summary>
		/// <param name="value">True if the inputted value meets the requirements of this rule.</param>
		/// <returns></returns>
		protected internal virtual bool PassesConditions(double value)
		{
			if (this.Operator == IconGroupOperator.GreaterThan)
			{
				return this.Value < value;
			}
			return this.Value <= value;
		}
		#endregion // PassesConditions

		#region GenerateStyle
		/// <summary>
		/// Generates a <see cref="ConditionalFormattingCellControl"/> including the Icon information.
		/// </summary>
		/// <returns></returns>
		protected internal virtual Style GenerateStyle()
		{
			Style newStyle = new Style(typeof(ConditionalFormattingCellControl)) { };
			newStyle.Setters.Add(new Setter(ConditionalFormattingCellControl.IconProperty, this.Icon));
			return newStyle;
		}

		#endregion // GenerateStyle

		#endregion // Methods
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