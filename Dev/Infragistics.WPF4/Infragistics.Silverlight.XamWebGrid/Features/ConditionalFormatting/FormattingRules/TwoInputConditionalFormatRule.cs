using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// Base class for two input formatting rules.
	/// </summary>
	public abstract class TwoInputConditionalFormatRule : ConditionalFormattingRuleBase
	{
		#region Properties

		#region MinimumValue

		/// <summary>
		/// Identifies the <see cref="MinimumValue"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.Register("MinimumValue", typeof(double?), typeof(TwoInputConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MinimumValueChanged)));

		/// <summary>
		/// The value that will be seen as the low value for formatting.
		/// </summary>
		[TypeConverter(typeof(NullableDoubleConverter))]
		public double? MinimumValue
		{
			get { return (double?)this.GetValue(MinimumValueProperty); }
			set { this.SetValue(MinimumValueProperty, value); }
		}

		private static void MinimumValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TwoInputConditionalFormatRule rule = (TwoInputConditionalFormatRule)obj;
			rule.OnPropertyChanged("MinimumValue");
		}

		#endregion // MinimumValue

		#region MaximumValue

		/// <summary>
		/// Identifies the <see cref="MaximumValue"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.Register("MaximumValue", typeof(double?), typeof(TwoInputConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MaximumValueChanged)));

		/// <summary>
		/// The value that will be seen as the high value for formatting.
		/// </summary>
		[TypeConverter(typeof(NullableDoubleConverter))]
		public double? MaximumValue
		{
			get { return (double?)this.GetValue(MaximumValueProperty); }
			set { this.SetValue(MaximumValueProperty, value); }
		}

		private static void MaximumValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TwoInputConditionalFormatRule rule = (TwoInputConditionalFormatRule)obj;
			rule.OnPropertyChanged("MaximumValue");
		}

		#endregion // MaximumValue

		#region MinimumValueType

		/// <summary>
		/// Identifies the <see cref="MinimumValueType"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MinimumValueTypeProperty = DependencyProperty.Register("MinimumValueType", typeof(ValueType), typeof(TwoInputConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MinimumValueTypeChanged)));

		/// <summary>
		/// The <see cref="ValueType"/> which describes how the <see cref="MinimumValue"/> will be derived and evaluated.
		/// </summary>
		public ValueType MinimumValueType
		{
			get { return (ValueType)this.GetValue(MinimumValueTypeProperty); }
			set { this.SetValue(MinimumValueTypeProperty, value); }
		}

		private static void MinimumValueTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TwoInputConditionalFormatRule rule = (TwoInputConditionalFormatRule)obj;
			rule.OnPropertyChanged("MinimumValueType");
		}

		#endregion // MinimumValueType

		#region MaximumValueType

		/// <summary>
		/// Identifies the <see cref="MaximumValueType"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MaximumValueTypeProperty = DependencyProperty.Register("MaximumValueType", typeof(ValueType), typeof(TwoInputConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MaximumValueTypeChanged)));

		/// <summary>
		/// The <see cref="ValueType"/> which describes how the <see cref="MaximumValue"/> will be derived and evaluated.
		/// </summary>
		public ValueType MaximumValueType
		{
			get { return (ValueType)this.GetValue(MaximumValueTypeProperty); }
			set { this.SetValue(MaximumValueTypeProperty, value); }
		}

		private static void MaximumValueTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TwoInputConditionalFormatRule rule = (TwoInputConditionalFormatRule)obj;
			rule.OnPropertyChanged("MaximumValueType");
		}

		#endregion // MaximumValueType

		#endregion // Properties
	}

	/// <summary>
	/// The execution proxy for the <see cref="TwoInputConditionalFormatRuleProxy"/>.
	/// </summary>
	public abstract class TwoInputConditionalFormatRuleProxy : ConditionalFormattingRuleBaseProxy
	{
		#region Properties

		#region PercentileData

		/// <summary>
		/// Gets / sets the <see cref="PercentileData"/> object that can be used to determine where a value would translate.
		/// </summary>
		protected PercentileData PercentileData { get; set; }

		#endregion // PercentileData

		#region PercentData

		/// <summary>
		/// Gets / sets the <see cref="PercentData"/> object that can be used to determine where a value would translate.
		/// </summary>
		protected PercentData PercentData { get; set; }

		#endregion // PercentData

		#region MaximumValue

		/// <summary>
		/// Gets / sets the value that was calculated or set by the user to be the maximum.
		/// </summary>
		protected internal double? MaximumValue { get; set; }

		#endregion // MaximumValue

		#region MinimumValue

		/// <summary>
		/// Gets / sets the value that was calculated or set by the user to be the minimum.
		/// </summary>
		protected internal double? MinimumValue
		{
			get;
			set;
		}

		#endregion // MinimumValue

		#endregion // Properties

		#region Methods

		#region GatherData

		/// <summary>
		/// Called at the stage provided by <see cref="IRule.RuleExecution"/> so that values can be derived for formatting purposes.
		/// </summary>
		/// <param name="query"></param>
		protected override void GatherData(IQueryable query)
		{
			TwoInputConditionalFormatRule rule = (TwoInputConditionalFormatRule)this.Parent;

			if (rule.MaximumValueType == ValueType.GeneratedValue)
			{
				this.MaximumValue = AutomaticData.CreateAutomaticData(rule.Column.ColumnLayout.ObjectDataTypeInfo,
																		rule.Column.Key,
																		LinqSummaryOperator.Maximum,
																		query);
			}
            else if (rule.MaximumValueType == ValueType.Number && this.MaximumValue == null)
            {
                throw new ArgumentException(SRGrid.GetString("ConditionalFormattingMaximumNotSet"));
            }

			if (rule.MinimumValueType == ValueType.GeneratedValue)
			{
				this.MinimumValue = AutomaticData.CreateAutomaticData(rule.Column.ColumnLayout.ObjectDataTypeInfo,
																		rule.Column.Key,
																		LinqSummaryOperator.Minimum,
																		query);
			}
            else if (rule.MinimumValueType == ValueType.Number && this.MinimumValue == null)
            {
                throw new ArgumentException(SRGrid.GetString("ConditionalFormattingMinimumNotSet"));
            }

			bool needPercentileData = rule.MinimumValueType == ValueType.Percentile || rule.MaximumValueType == ValueType.Percentile;

			bool needPercentData = rule.MinimumValueType == ValueType.Percent || rule.MaximumValueType == ValueType.Percent;

			if (needPercentileData)
			{
				this.PercentileData = PercentileData.CreateGenericPercentileData(rule.Column.ColumnLayout.ObjectDataTypeInfo,
																		rule.Column.Key,
																		rule.Column.DataType == typeof(DateTime),
																		query);

				if (rule.MinimumValueType == ValueType.Percentile && this.MinimumValue == null)
				{
					this.MinimumValue = this.PercentileData.CalculatedMinimumPercentileValue;
				}

				if (rule.MaximumValueType == ValueType.Percentile && this.MaximumValue == null)
				{
					this.MaximumValue = this.PercentileData.CalculatedMaximumPercentileValue; ;
				}
			}

			if (needPercentData)
			{
				this.PercentData = PercentData.CreatePercentData(rule.Column.ColumnLayout.ObjectDataTypeInfo,
																		rule.Column.Key,
																		query);

				if (rule.MinimumValueType == ValueType.Percent && this.MinimumValue == null)
				{
					this.MinimumValue = 0;
				}

				if (rule.MaximumValueType == ValueType.Percent && this.MaximumValue == null)
				{
					this.MaximumValue = 100;
				}
			}
		}

		#endregion // GatherData

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