using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Infragistics.Controls.Grids.Primitives;
using System.Globalization;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A conditional formatting rule which will scale from a minimum color to a median color over the first part of a range
    /// and between the median color and the maximum color over the second part of the range.
    /// </summary>
    public class ThreeColorScaleConditionalFormatRule : TwoColorScaleConditionalFormatRule
    {
        #region MedianValueType

        /// <summary>
        /// Identifies the <see cref="MedianValueType"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MedianValueTypeProperty = DependencyProperty.Register("MedianValueType", typeof(ValueType), typeof(ThreeColorScaleConditionalFormatRule), new PropertyMetadata(ValueType.GeneratedValue, new PropertyChangedCallback(MedianValueTypeChanged)));

        /// <summary>
        /// The <see cref="ValueType"/> which describes how the <see cref="MedianValue"/> will be derived and evaluated.
        /// </summary>
        public ValueType MedianValueType
        {
            get { return (ValueType)this.GetValue(MedianValueTypeProperty); }
            set { this.SetValue(MedianValueTypeProperty, value); }
        }

        private static void MedianValueTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ThreeColorScaleConditionalFormatRule rule = (ThreeColorScaleConditionalFormatRule)obj;
            rule.OnPropertyChanged("MedianValueType");
        }

        #endregion // MedianValueType

        #region MedianValue

        /// <summary>
        /// Identifies the <see cref="MedianValue"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MedianValueProperty = DependencyProperty.Register("MedianValue", typeof(double?), typeof(ThreeColorScaleConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MedianValueChanged)));

        /// <summary>
        /// The value that will be seen as the middle value for formatting purposes.
        /// </summary>
        [TypeConverter(typeof(NullableDoubleConverter))]
        public double? MedianValue
        {
            get { return (double?)this.GetValue(MedianValueProperty); }
            set { this.SetValue(MedianValueProperty, value); }
        }

        private static void MedianValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ThreeColorScaleConditionalFormatRule rule = (ThreeColorScaleConditionalFormatRule)obj;
            rule.OnPropertyChanged("MedianValue");
        }

        #endregion // MedianValue

        #region MedianColor

        /// <summary>
        /// Identifies the <see cref="MedianColor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MedianColorProperty = DependencyProperty.Register("MedianColor", typeof(Color), typeof(ThreeColorScaleConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(MedianColorChanged)));

        /// <summary>
        /// The Color which acts as the median value for the color scale line.
        /// </summary>
        public Color MedianColor
        {
            get { return (Color)this.GetValue(MedianColorProperty); }
            set { this.SetValue(MedianColorProperty, value); }
        }

        private static void MedianColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ThreeColorScaleConditionalFormatRule rule = (ThreeColorScaleConditionalFormatRule)obj;
            rule.OnPropertyChanged("MedianColor");
        }

        #endregion // MedianColor

        #region Overrides
        /// <summary>
        /// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override IConditionalFormattingRuleProxy CreateProxy()
        {
            return new ThreeColorScaleConditionalFormatRuleProxy() { MedianValue = this.MedianValue, MaximumValue = this.MaximumValue, MinimumValue = this.MinimumValue };
        }
        #endregion // Overrides
    }

    /// <summary>
    /// The execution proxy for the <see cref="ThreeColorScaleConditionalFormatRule"/>.
    /// </summary>
    public class ThreeColorScaleConditionalFormatRuleProxy : TwoColorScaleConditionalFormatRuleProxy
    {
        #region Properties

        #region MedianValue

        /// <summary>
        /// Gets / sets the value which will be seen as the median point in the range.
        /// </summary>
        protected internal double? MedianValue { get; set; }

        #endregion // MedianValue

        #region RedSlope2

        /// <summary>
        /// The slope of the gradient line on the red axis for the second range from median to the upper bound.
        /// </summary>
        protected double RedSlope2 { get; set; }

        #endregion // RedSlope2

        #region RedYIntercept2

        /// <summary>
        /// The Y intercept of the gradient line on the red axis for the second range from median to the upper bound.
        /// </summary>
        protected double RedYIntercept2 { get; set; }

        #endregion // RedYIntercept2

        #region GreenSlope2

        /// <summary>
        /// The slope of the gradient line on the green axis for the second range from median to the upper bound.
        /// </summary>
        protected double GreenSlope2 { get; set; }

        #endregion // GreenSlope2

        #region GreenYIntercept2

        /// <summary>
        /// The Y intercept of the gradient line on the green axis for the second range from median to the upper bound.
        /// </summary>
        protected double GreenYIntercept2 { get; set; }

        #endregion // GreenYIntercept2

        #region BlueSlope2

        /// <summary>
        /// The slope of the gradient line on the blue axis for the second range from median to the upper bound.
        /// </summary>
        protected double BlueSlope2 { get; set; }

        #endregion // BlueSlope2

        #region BlueYIntercept2

        /// <summary>
        /// The Y intercept of the gradient line on the blue axis for the second range from median to the upper bound.
        /// </summary>
        protected double BlueYIntercept2 { get; set; }

        #endregion // BlueYIntercept2

        #region MedianConvertedValue
        private double MedianConvertedValue { get; set; }
        #endregion // MedianConvertedValue

        #endregion // Properties

        #region Methods

        #region GatherData
        /// <summary>
        /// Called at the stage provided by <see cref="IRule.RuleExecution"/> so that values can be derived for formatting purposes.
        /// </summary>
        /// <param name="query"></param>
        protected override void GatherData(IQueryable query)
        {
            ThreeColorScaleConditionalFormatRule rule = (ThreeColorScaleConditionalFormatRule)this.Parent;

            if (rule.MedianValueType == ValueType.GeneratedValue)
            {
                SummaryContext sc = SummaryContext.CreateGenericSummary(this.Parent.Column.ColumnLayout.ObjectDataTypeInfo, this.Parent.Column.Key, LinqSummaryOperator.Average);

                this.MedianValue = Convert.ToDouble(sc.Execute(query), CultureInfo.InvariantCulture);
            }

            base.GatherData(query);
        }
        #endregion // GatherData

        #region GenerateLineEquations

        /// <summary>
        /// Calculates the slopes and intercepts for the color line.
        /// </summary>
        protected override void GenerateLineEquations()
        {
            ThreeColorScaleConditionalFormatRule rule = (ThreeColorScaleConditionalFormatRule)this.Parent;

            double minimumValue = 0.0;
            double maximumValue = 0.0;
            double medianValue = 0.0;

            #region Minimum
            // first see if we min out
            switch (rule.MinimumValueType)
            {
                case (ValueType.GeneratedValue):
                    {
                        minimumValue = (double)this.MinimumValue;
                        break;
                    }
                case (ValueType.Number):
                    {
                        minimumValue = (double)this.MinimumValue;
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            minimumValue = this.PercentileData.GetValueJustOverPercentile((double)this.MinimumValue);
                        }
                        break;
                    }
                case (ValueType.Percent):
                    {
                        if (this.PercentData != null)
                        {
                            minimumValue = this.PercentData.GetRealValue((double)this.MinimumValue);
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
                        maximumValue = (double)this.MaximumValue;
                        break;
                    }
                case (ValueType.Number):
                    {
                        maximumValue = (double)this.MaximumValue;
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            maximumValue = this.PercentileData.GetValueJustUnderPercentile((double)this.MaximumValue);
                        }
                        break;
                    }
                case (ValueType.Percent):
                    {
                        if (this.PercentData != null)
                        {
                            maximumValue = this.PercentData.GetRealValue((double)this.MaximumValue);
                        }
                        break;
                    }
            }

            #endregion // Maximum

            #region Median
            // first see if we max out
            switch (rule.MedianValueType)
            {
                case (ValueType.GeneratedValue):
                    {
                        medianValue = (double)this.MedianValue;
                        break;
                    }
                case (ValueType.Number):
                    {
                        medianValue = (double)this.MedianValue;
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            medianValue = this.PercentileData.GetValueJustUnderPercentile((double)this.MedianValue);
                        }
                        break;
                    }
                case (ValueType.Percent):
                    {
                        if (this.PercentData != null)
                        {
                            medianValue = this.PercentData.GetRealValue((double)this.MedianValue);
                        }
                        break;
                    }
            }

            #endregion // Median

            this.MedianConvertedValue = medianValue;

            double lowerValue = minimumValue;
            double higherValue = medianValue;
            Color lowerColor = rule.MinimumColor;
            Color higherColor = rule.MedianColor;

            this.RedSlope = ConditionalFormattingRuleBase.CalculateColorSlope(lowerValue, higherValue, lowerColor.R, higherColor.R);
            this.RedYIntercept = ConditionalFormattingRuleBase.CalculateYIntercept(this.RedSlope, lowerValue, lowerColor.R);
            this.GreenSlope = ConditionalFormattingRuleBase.CalculateColorSlope(lowerValue, higherValue, lowerColor.G, higherColor.G);
            this.GreenYIntercept = ConditionalFormattingRuleBase.CalculateYIntercept(this.GreenSlope, lowerValue, lowerColor.G);
            this.BlueSlope = ConditionalFormattingRuleBase.CalculateColorSlope(lowerValue, higherValue, lowerColor.B, higherColor.B);
            this.BlueYIntercept = ConditionalFormattingRuleBase.CalculateYIntercept(this.BlueSlope, lowerValue, lowerColor.B);


            lowerValue = medianValue;
            higherValue = maximumValue;
            lowerColor = rule.MedianColor;
            higherColor = rule.MaximumColor;

            this.RedSlope2 = ConditionalFormattingRuleBase.CalculateColorSlope(lowerValue, higherValue, lowerColor.R, higherColor.R);
            this.RedYIntercept2 = ConditionalFormattingRuleBase.CalculateYIntercept(this.RedSlope2, lowerValue, lowerColor.R);
            this.GreenSlope2 = ConditionalFormattingRuleBase.CalculateColorSlope(lowerValue, higherValue, lowerColor.G, higherColor.G);
            this.GreenYIntercept2 = ConditionalFormattingRuleBase.CalculateYIntercept(this.GreenSlope2, lowerValue, lowerColor.G);
            this.BlueSlope2 = ConditionalFormattingRuleBase.CalculateColorSlope(lowerValue, higherValue, lowerColor.B, higherColor.B);
            this.BlueYIntercept2 = ConditionalFormattingRuleBase.CalculateYIntercept(this.BlueSlope2, lowerValue, lowerColor.B);
        }

        #endregion // GenerateLineEquations

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

            ThreeColorScaleConditionalFormatRule rule = (ThreeColorScaleConditionalFormatRule)this.Parent;

            double? value = null;
            double lowerEndOfMedian = 0.0;
            double higherEndOfMedian = 0.0;
            double medianValue = 0.0;

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
                        lowerEndOfMedian = (double)this.MinimumValue;
                        break;
                    }
                case (ValueType.Number):
                    {
                        value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
                        if (value <= this.MinimumValue)
                        {
                            return ConditionalFormattingRuleBase.CreateStyle(rule.MinimumColor);
                        }
                        lowerEndOfMedian = (double)this.MinimumValue;
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            value = this.PercentileData.GetPercentileValue(sourceDataValue);
                            if (value <= this.MinimumValue)
                            {
                                return ConditionalFormattingRuleBase.CreateStyle(rule.MinimumColor);
                            }
                            lowerEndOfMedian = this.PercentileData.GetValueJustOverPercentile((double)this.MinimumValue);
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
                            lowerEndOfMedian = this.PercentData.GetRealValue((double)this.MinimumValue);
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
                        higherEndOfMedian = (double)this.MaximumValue;
                        break;
                    }
                case (ValueType.Number):
                    {
                        value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
                        if (value >= this.MaximumValue)
                        {
                            return ConditionalFormattingRuleBase.CreateStyle(rule.MaximumColor);
                        }
                        higherEndOfMedian = (double)this.MaximumValue;
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            value = this.PercentileData.GetPercentileValue(sourceDataValue);
                            if (value >= this.MaximumValue)
                            {
                                return ConditionalFormattingRuleBase.CreateStyle(rule.MaximumColor);
                            }
                            higherEndOfMedian = this.PercentileData.GetValueJustUnderPercentile((double)this.MaximumValue);
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
                            higherEndOfMedian = this.PercentData.GetRealValue((double)this.MaximumValue);
                        }
                        break;
                    }
            }

            #endregion // Maximum

            



            #region Median


            #region Median
            // first see if we max out
            switch (rule.MedianValueType)
            {
                case (ValueType.GeneratedValue):
                    {
                        medianValue = (double)this.MedianValue;
                        break;
                    }
                case (ValueType.Number):
                    {
                        medianValue = (double)this.MedianValue;
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            medianValue = this.PercentileData.GetValueJustUnderPercentile((double)this.MedianValue);
                        }
                        break;
                    }
                case (ValueType.Percent):
                    {
                        if (this.PercentData != null)
                        {
                            medianValue = this.PercentData.GetRealValue((double)this.MedianValue);
                        }
                        break;
                    }
            }

            #endregion // Median


            #endregion // Median

            double convertedValue = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);

            double red = 0, green = 0, blue = 0;

            if (convertedValue < this.MedianConvertedValue)
            {
                red = this.RedSlope * convertedValue + this.RedYIntercept;
                green = this.GreenSlope * convertedValue + this.GreenYIntercept;
                blue = this.BlueSlope * convertedValue + this.BlueYIntercept;
            }
            else
            {
                red = this.RedSlope2 * convertedValue + this.RedYIntercept2;
                green = this.GreenSlope2 * convertedValue + this.GreenYIntercept2;
                blue = this.BlueSlope2 * convertedValue + this.BlueYIntercept2;
            }

            return ConditionalFormattingRuleBase.CreateStyle(Color.FromArgb(255, this.NormalizeValue(red), this.NormalizeValue(green), this.NormalizeValue(blue)));

        }
        #endregion // EvaluateCondition


        private Byte NormalizeValue(double value)
        {
            return Convert.ToByte((Math.Max(0, Math.Min(255.0, value))));
        }
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