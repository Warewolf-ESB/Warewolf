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
using System.Linq;
using System.Globalization;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A conditional formatting rule which will apply a color gradient between two points.
	/// </summary>
	public class TwoColorScaleConditionalFormatRule : TwoInputConditionalFormatRule
	{
		#region MinimumColor

		/// <summary>
		/// Identifies the <see cref="MinimumColor"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MinimumColorProperty = DependencyProperty.Register("MinimumColor", typeof(Color), typeof(TwoColorScaleConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MinimumColorChanged)));

		/// <summary>
		/// The Color which acts as the minimum value for the color scale line.
		/// </summary>
		public Color MinimumColor
		{
			get { return (Color)this.GetValue(MinimumColorProperty); }
			set { this.SetValue(MinimumColorProperty, value); }
		}

		private static void MinimumColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TwoColorScaleConditionalFormatRule rule = (TwoColorScaleConditionalFormatRule)obj;
			rule.OnPropertyChanged("MinimumColor");
		}

		#endregion // MinimumColor

		#region MaximumColor

		/// <summary>
		/// Identifies the <see cref="MaximumColor"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MaximumColorProperty = DependencyProperty.Register("MaximumColor", typeof(Color), typeof(TwoColorScaleConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MaximumColorChanged)));

		/// <summary>
		/// The Color which acts as the maximum value for the color scale line.
		/// </summary>
		public Color MaximumColor
		{
			get { return (Color)this.GetValue(MaximumColorProperty); }
			set { this.SetValue(MaximumColorProperty, value); }
		}

		private static void MaximumColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TwoColorScaleConditionalFormatRule rule = (TwoColorScaleConditionalFormatRule)obj;
			rule.OnPropertyChanged("MaximumColor");
		}

		#endregion // MaximumColor

		#region Overrides

		/// <summary>
		/// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override IConditionalFormattingRuleProxy CreateProxy()
		{
			return new TwoColorScaleConditionalFormatRuleProxy() { MaximumValue = this.MaximumValue, MinimumValue = this.MinimumValue };
		}

		#endregion // Overrides
	}

	/// <summary>
	/// The execution proxy for the <see cref="TwoColorScaleConditionalFormatRule"/>.
	/// </summary>
	public class TwoColorScaleConditionalFormatRuleProxy : TwoInputConditionalFormatRuleProxy
	{
		#region Protected

		#region RedSlope

		/// <summary>
		/// The slope of the gradient line on the red axis.
		/// </summary>
		protected double RedSlope { get; set; }

		#endregion // RedSlope

		#region GreenSlope

		/// <summary>
		/// The slope of the gradient line on the green axis.
		/// </summary>
		protected double GreenSlope { get; set; }

		#endregion // GreenSlope

		#region BlueSlope
		/// <summary>
		/// The slope of the gradient line on the blue axis.
		/// </summary>
		protected double BlueSlope { get; set; }

		#endregion // BlueSlope

		#region RedYIntercept

		/// <summary>
		/// The Y intercept of the gradient line on the red axis.
		/// </summary>
		protected double RedYIntercept { get; set; }

		#endregion // RedYIntercept

		#region BlueYIntercept

		/// <summary>
		/// The Y intercept of the gradient line on the blue axis.
		/// </summary>
		protected double BlueYIntercept { get; set; }

		#endregion // BlueYIntercept

		#region GreenYIntercept

		/// <summary>
		/// The Y intercept of the gradient line on the green axis.
		/// </summary>
		protected double GreenYIntercept { get; set; }

		#endregion // GreenYIntercept

		#endregion // Protected

		#region Methods

		#region Overrides

		#region GatherData

		/// <summary>
		/// Called at the stage provided by <see cref="IRule.RuleExecution"/> so that values can be derived for formatting purposes.
		/// </summary>
		/// <param name="query"></param>
		protected override void GatherData(IQueryable query)
		{
			base.GatherData(query);

			this.GenerateLineEquations();
		}

		#endregion // GatherData

		#region EvaluateCondition

		/// <summary>
		/// Determines whether the inputted value meets the condition of <see cref="ConditionalFormattingRuleBase"/> and returns the style 
		/// that should be applied.
		/// </summary>
		/// <param name="sourceDataObject"></param>
		/// <param name="sourceDataValue"></param>
		/// <returns></returns>
		protected override Style EvaluateCondition(object sourceDataObject, object sourceDataValue)
		{
			if (sourceDataValue == null)
			{
				return null;
			}

			double? value = null;

			TwoColorScaleConditionalFormatRule rule = (TwoColorScaleConditionalFormatRule)this.Parent;

			#region Minimum
			// first see if we min out
			switch (rule.MinimumValueType)
			{
				case (ValueType.GeneratedValue):
					{
						value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
						if (value <= this.MinimumValue)
						{
							return ConditionalFormattingRuleBase.CreateStyle(rule.MinimumColor);
						}
						break;
					}
				case (ValueType.Number):
					{
						value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
						if (value <= this.MinimumValue)
						{
							return ConditionalFormattingRuleBase.CreateStyle(rule.MinimumColor);
						}
						break;
					}
				case (ValueType.Percentile):
					{
						if (this.PercentileData != null )//&& this.PercentileData.Dictionary.Contains(sourceDataValue))
						{
							value = this.PercentileData.GetPercentileValue(sourceDataValue);
							if (value <= this.MinimumValue)
							{
								return ConditionalFormattingRuleBase.CreateStyle(rule.MinimumColor);
							}
						}
						break;
					}
				case (ValueType.Percent):
					{
						if (this.PercentData != null && this.PercentData.Range > 0)
						{
							value = this.PercentData.GetPercentValue(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture));
							if (value <= this.MinimumValue)
							{
								return ConditionalFormattingRuleBase.CreateStyle(rule.MinimumColor);
							}
						}
						break;
					}
			}
			#endregion // Minimum

			#region Maximum
			// first see if we max out
			switch (rule.MaximumValueType)
			{
				case (ValueType.GeneratedValue):
					{
						value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
						if (value >= this.MaximumValue)
						{
							return ConditionalFormattingRuleBase.CreateStyle(rule.MaximumColor);
						}
						break;
					}
				case (ValueType.Number):
					{
						value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
						if (value >= this.MaximumValue)
						{
							return ConditionalFormattingRuleBase.CreateStyle(rule.MaximumColor);
						}
						break;
					}
				case (ValueType.Percentile):
					{
						if (this.PercentileData != null )//&& this.PercentileData.Dictionary.Contains(sourceDataValue))
						{
							value = this.PercentileData.GetPercentileValue(sourceDataValue);
							if (value >= this.MaximumValue)
							{
								return ConditionalFormattingRuleBase.CreateStyle(rule.MaximumColor);
							}
						}
						break;
					}
				case (ValueType.Percent):
					{
						if (this.PercentData != null && this.PercentData.Range > 0)
						{
							value = this.PercentData.GetPercentValue(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture));
							if (value >= this.MaximumValue)
							{
								return ConditionalFormattingRuleBase.CreateStyle(rule.MaximumColor);
							}
						}
						break;
					}
			}

			#endregion // Maximum

			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


			if (rule.MinimumValueType == ValueType.Number && rule.MaximumValueType == ValueType.Number)
			{
				if (this.MaximumValue == null || this.MinimumValue == null)
				{
					throw new NullConditionalFormatEvaluationValueException();
				}

				return ConditionalFormattingRuleBase.CreateStyle(CalculateColor(Convert.ToDouble(value, CultureInfo.InvariantCulture)));
			}

			if (rule.MinimumValueType == ValueType.GeneratedValue && rule.MaximumValueType == ValueType.GeneratedValue)
			{
				return ConditionalFormattingRuleBase.CreateStyle(CalculateColor(Convert.ToDouble(value, CultureInfo.InvariantCulture)));
			}

			if (rule.MinimumValueType == ValueType.Percent && rule.MaximumValueType == ValueType.Percent)
			{
				return ConditionalFormattingRuleBase.CreateStyle(CalculateColor(Convert.ToDouble(value, CultureInfo.InvariantCulture)));
			}

			if (rule.MinimumValueType == ValueType.Percentile && rule.MaximumValueType == ValueType.Percentile)
			{

				if (this.PercentileData == null )//|| !this.PercentileData.Dictionary.Contains(sourceDataValue))
				{
					return null;
				}

				return ConditionalFormattingRuleBase.CreateStyle(CalculateColor(Convert.ToDouble(value, CultureInfo.InvariantCulture)));
			}

			



			double leftEdgeRealValue = double.NaN;

			double rightEdgeRealValue = double.NaN;

			if (rule.MinimumValueType == ValueType.GeneratedValue || rule.MinimumValueType == ValueType.Number)
			{
				leftEdgeRealValue = (double)this.MinimumValue;
			}
			if (rule.MaximumValueType == ValueType.GeneratedValue || rule.MaximumValueType == ValueType.Number)
			{
				rightEdgeRealValue = (double)this.MaximumValue;
			}
			if (!double.IsNaN(leftEdgeRealValue) && !double.IsNaN(rightEdgeRealValue))
			{
				return ConditionalFormattingRuleBase.CreateStyle(CalculateColor(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture)));
			}

			if (rule.MinimumValueType == ValueType.Percent)
			{
				leftEdgeRealValue = this.PercentData.GetRealValue((double)this.MinimumValue);
			}
			else if (rule.MinimumValueType == ValueType.Percentile)
			{
				rightEdgeRealValue = this.PercentileData.GetValueJustUnderPercentile((double)this.MinimumValue);
			}
			if (rule.MaximumValueType == ValueType.Percent)
			{
				rightEdgeRealValue = this.PercentData.GetRealValue((double)this.MaximumValue);
			}
			else if (rule.MaximumValueType == ValueType.Percentile)
			{
				rightEdgeRealValue = this.PercentileData.GetValueJustOverPercentile((double)this.MaximumValue);
			}

			RangeData newRangePercentData = RangeData.CreateRangeData(leftEdgeRealValue, rightEdgeRealValue, null, null, null);

			return ConditionalFormattingRuleBase.CreateStyle(CalculateColor(newRangePercentData.GetPercentValue(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture))));
		}

		#endregion // EvaluateCondition

		#endregion // Overrides

		#region Protected

		#region GenerateLineEquations

		/// <summary>
		/// Calculates the slopes and intercepts for the color line.
		/// </summary>
		protected virtual void GenerateLineEquations()
		{
			if (this.MaximumValue == null || this.MinimumValue == null)
				return;

			TwoColorScaleConditionalFormatRule rule = (TwoColorScaleConditionalFormatRule)this.Parent;
			double minValue = (double)this.MinimumValue;
			double maxValue = (double)this.MaximumValue;

			this.RedSlope = ConditionalFormattingRuleBase.CalculateColorSlope(minValue, maxValue, rule.MinimumColor.R, rule.MaximumColor.R);
			this.RedYIntercept = ConditionalFormattingRuleBase.CalculateYIntercept(this.RedSlope, minValue, rule.MinimumColor.R);
			this.GreenSlope = ConditionalFormattingRuleBase.CalculateColorSlope(minValue, maxValue, rule.MinimumColor.G, rule.MaximumColor.G);
			this.GreenYIntercept = ConditionalFormattingRuleBase.CalculateYIntercept(this.GreenSlope, minValue, rule.MinimumColor.G);
			this.BlueSlope = ConditionalFormattingRuleBase.CalculateColorSlope(minValue, maxValue, rule.MinimumColor.B, rule.MaximumColor.B);
			this.BlueYIntercept = ConditionalFormattingRuleBase.CalculateYIntercept(this.BlueSlope, minValue, rule.MinimumColor.B);
		}

		#endregion // GenerateLineEquations

		#endregion // Protected

		#region Private

		#region CalculateColor

		private Color CalculateColor(double value)
		{
			TwoColorScaleConditionalFormatRule rule = (TwoColorScaleConditionalFormatRule)this.Parent;
			double red = 0, green = 0, blue = 0;

			red = this.RedSlope * value + this.RedYIntercept;
			green = this.GreenSlope * value + this.GreenYIntercept;
			blue = this.BlueSlope * value + this.BlueYIntercept;

			Color newColor = Colors.Transparent;

			if (red > 255 || blue > 255 || green > 255)
			{
				return rule.MaximumColor;
			}
			else if (red < 0 || blue < 0 || green < 0)
			{
				return rule.MinimumColor;
			}
			else
			{
				newColor = Color.FromArgb(255, Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));
			}

			return newColor;
		}
		#endregion // CalculateColor

		#endregion // Private

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