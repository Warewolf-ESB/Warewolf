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
using System.Linq;
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;
using System.Globalization;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A conditional formatting rule that will display databars in the cell representing the data.
    /// </summary>
    public class DataBarConditionalFormatRule : TwoInputConditionalFormatRule
    {
        #region Public

        #region DataBarDirection

        /// <summary>
        /// Identifies the <see cref="DataBarDirection"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DataBarDirectionProperty = DependencyProperty.Register("DataBarDirection", typeof(DataBarDirection), typeof(DataBarConditionalFormatRule), new PropertyMetadata(DataBarDirection.UnidirectionalLeftToRight, new PropertyChangedCallback(DataBarDirectionChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataBarDirection"/> for the databars displayed by this control.
        /// </summary>
        public DataBarDirection DataBarDirection
        {
            get { return (DataBarDirection)this.GetValue(DataBarDirectionProperty); }
            set { this.SetValue(DataBarDirectionProperty, value); }
        }

        private static void DataBarDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DataBarConditionalFormatRule rule = (DataBarConditionalFormatRule)obj;
            rule.OnPropertyChanged("DataBarDirection");
        }

        #endregion // DataBarDirection

        #region DataBrush

        /// <summary>
        /// Identifies the <see cref="DataBrush"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DataBrushProperty = DependencyProperty.Register("DataBrush", typeof(Brush), typeof(DataBarConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(DataBrushChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Brush"/> that will be used to color the positive data bar.
        /// </summary>
        public Brush DataBrush
        {
            get { return (Brush)this.GetValue(DataBrushProperty); }
            set { this.SetValue(DataBrushProperty, value); }
        }

        private static void DataBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DataBarConditionalFormatRule rule = (DataBarConditionalFormatRule)obj;
            rule.OnPropertyChanged("DataBrush");
        }

        #endregion // DataBrush

        #region NegativeDataBrush

        /// <summary>
        /// Identifies the <see cref="NegativeDataBrush"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NegativeDataBrushProperty = DependencyProperty.Register("NegativeDataBrush", typeof(Brush), typeof(DataBarConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(NegativeDataBrushChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Brush"/> that will be used to color the negative data bar.
        /// </summary>
        public Brush NegativeDataBrush
        {
            get { return (Brush)this.GetValue(NegativeDataBrushProperty); }
            set { this.SetValue(NegativeDataBrushProperty, value); }
        }

        private static void NegativeDataBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DataBarConditionalFormatRule rule = (DataBarConditionalFormatRule)obj;
            rule.OnPropertyChanged("NegativeDataBrush");
        }

        #endregion // NegativeDataBrush

        #region UseNegativeDataBar

        /// <summary>
        /// Identifies the <see cref="UseNegativeDataBar"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty UseNegativeDataBarProperty = DependencyProperty.Register("UseNegativeDataBar", typeof(bool), typeof(DataBarConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(UseNegativeDataBarChanged)));

        /// <summary>
        /// Gets / sets if the negative data bar should be used.
        /// </summary>
        public bool UseNegativeDataBar
        {
            get { return (bool)this.GetValue(UseNegativeDataBarProperty); }
            set { this.SetValue(UseNegativeDataBarProperty, value); }
        }

        private static void UseNegativeDataBarChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DataBarConditionalFormatRule rule = (DataBarConditionalFormatRule)obj;
            rule.OnPropertyChanged("UseNegativeDataBar");
        }

        #endregion // UseNegativeDataBar

        #endregion // Public

        #region Overrides

        #region CreateProxy

        /// <summary>
        /// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override IConditionalFormattingRuleProxy CreateProxy()
        {
            return new DataBarConditionalFormatRuleProxy() { MaximumValue = this.MaximumValue, MinimumValue = this.MinimumValue };
        }

        #endregion // CreateProxy

        #endregion // Overrides
    }

    /// <summary>
    /// The execution proxy for the <see cref="DataBarConditionalFormatRule"/>.
    /// </summary>
    public class DataBarConditionalFormatRuleProxy : TwoInputConditionalFormatRuleProxy
    {
        #region Methods

        #region GenerateStyle

        /// <summary>
        /// Creates as <see cref="Style"/> object for a <see cref="ConditionalFormattingCellControl"/> based on the current state of the object.
        /// </summary>
        /// <param name="percentageValue"></param>
        /// <param name="barColorationAndDirection"></param>
        /// <returns></returns>
        protected virtual Style GenerateStyle(double percentageValue, DataBarPositiveNegative barColorationAndDirection)
        {
            DataBarConditionalFormatRule rule = (DataBarConditionalFormatRule)this.Parent;
            Style newStyle = new Style(typeof(ConditionalFormattingCellControl)) { };
            newStyle.Setters.Add(new Setter(ConditionalFormattingCellControl.BarPositiveOrNegativeProperty, barColorationAndDirection));
            if (rule.DataBrush != null)
                newStyle.Setters.Add(new Setter(ConditionalFormattingCellControl.BarBrushProperty, rule.DataBrush));
            if (rule.NegativeDataBrush != null)
                newStyle.Setters.Add(new Setter(ConditionalFormattingCellControl.NegativeBarBrushProperty, rule.NegativeDataBrush));
            newStyle.Setters.Add(new Setter(ConditionalFormattingCellControl.BarDirectionProperty, rule.DataBarDirection));
            newStyle.Setters.Add(new Setter(ConditionalFormattingCellControl.BarPercentageProperty, percentageValue));
            return newStyle;
        }

        #endregion // GenerateStyle


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
            DataBarConditionalFormatRule rule = (DataBarConditionalFormatRule)this.Parent;

            double? value = null;
            bool canUseNegativeAxis = false;
            #region Minimum
            // first see if we min out
            switch (rule.MinimumValueType)
            {
                case (ValueType.GeneratedValue):
                    {
                        value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);

                        if (value < 0)
                            canUseNegativeAxis = true;

                        if (value <= this.MinimumValue)
                        {
                            bool useNegativeAxis = rule.UseNegativeDataBar && canUseNegativeAxis;

                            if (useNegativeAxis)
                            {
                                return this.GenerateStyle(100, DataBarPositiveNegative.Negative);
                            }

                            return this.GenerateStyle(0, DataBarPositiveNegative.Positive);
                        }
                        break;
                    }
                case (ValueType.Number):
                    {
                        value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);

                        if (value < 0)
                            canUseNegativeAxis = true;

                        if (value <= this.MinimumValue)
                        {
                            bool useNegativeAxis = rule.UseNegativeDataBar && canUseNegativeAxis;

                            if (useNegativeAxis)
                            {
                                return this.GenerateStyle(100, DataBarPositiveNegative.Negative);
                            }

                            return this.GenerateStyle(0, DataBarPositiveNegative.Positive);
                        }
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            value = this.PercentileData.GetPercentileValue(sourceDataValue);
                            if (value <= this.MinimumValue)
                            {
                                return this.GenerateStyle(0, DataBarPositiveNegative.Positive);
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
                                return this.GenerateStyle(0, DataBarPositiveNegative.Positive);
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

                        if (value < 0)
                            canUseNegativeAxis = true;

                        if (value >= this.MaximumValue)
                        {
                            bool useNegativeAxis = rule.UseNegativeDataBar && canUseNegativeAxis;

                            if (useNegativeAxis)
                            {
                                

                                break;
                            }

                            return this.GenerateStyle(100, DataBarPositiveNegative.Positive);
                        }
                        break;
                    }
                case (ValueType.Number):
                    {
                        value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);

                        if (value < 0)
                            canUseNegativeAxis = true;

                        if (value >= this.MaximumValue)
                        {
                            bool useNegativeAxis = rule.UseNegativeDataBar && canUseNegativeAxis;

                            if (useNegativeAxis)
                            {
                                return this.GenerateStyle(100, DataBarPositiveNegative.Negative);
                            }

                            return this.GenerateStyle(100, DataBarPositiveNegative.Positive);
                        }
                        break;
                    }
                case (ValueType.Percentile):
                    {
                        if (this.PercentileData != null)
                        {
                            value = this.PercentileData.GetPercentileValue(sourceDataValue);
                            if (value >= this.MaximumValue)
                            {
                                return this.GenerateStyle(100, DataBarPositiveNegative.Positive);
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
                                return this.GenerateStyle(100, DataBarPositiveNegative.Positive);
                            }
                        }
                        break;
                    }
            }

            #endregion // Maximum

            
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


            if ((rule.MinimumValueType == ValueType.Number || rule.MinimumValueType == ValueType.GeneratedValue) &&
                (rule.MaximumValueType == ValueType.Number || rule.MaximumValueType == ValueType.GeneratedValue))
            {
                return GenerateStyleForDiscreetEndPointedValues(sourceDataValue, rule);
            }

            if (rule.MinimumValueType == ValueType.Percent && rule.MaximumValueType == ValueType.Percent)
            {
                double calculatedPercentage = this.PercentData.GetPercentValue(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture));

                if (this.MinimumValue != null && this.MaximumValue != null)
                {
                    double effectiveRange = (double)this.MaximumValue - (double)this.MinimumValue;

                    calculatedPercentage -= (double)this.MinimumValue;

                    calculatedPercentage = (calculatedPercentage / effectiveRange) * 100;
                }
                return this.GenerateStyle(calculatedPercentage, DataBarPositiveNegative.Positive);
            }

            if (rule.MinimumValueType == ValueType.Percentile && rule.MaximumValueType == ValueType.Percentile)
            {
                if (this.PercentileData == null)
                {
                    return null;
                }

                double percentileValue = this.PercentileData.GetPercentileValue(sourceDataValue);

                double min = Convert.ToDouble(this.MinimumValue, CultureInfo.InvariantCulture);
                double max = Convert.ToDouble(this.MaximumValue, CultureInfo.InvariantCulture);

                return this.GenerateStyle(((percentileValue - min) / (max - min)) * 100.0, DataBarPositiveNegative.Positive);
            }

            



            double leftEdgeRealValue = double.NaN;

            double rightEdgeRealValue = double.NaN;

            if (rule.MinimumValueType == ValueType.GeneratedValue || rule.MinimumValueType == ValueType.Number)
            {
                leftEdgeRealValue = (double)this.MinimumValue;
            }
            else if (rule.MinimumValueType == ValueType.Percent)
            {
                leftEdgeRealValue = this.PercentData.GetRealValue((double)this.MinimumValue);
            }
            else if (rule.MinimumValueType == ValueType.Percentile)
            {
                rightEdgeRealValue = this.PercentileData.GetValueJustUnderPercentile((double)this.MinimumValue);
            }

            if (rule.MaximumValueType == ValueType.GeneratedValue || rule.MaximumValueType == ValueType.Number)
            {
                rightEdgeRealValue = (double)this.MaximumValue;
            }
            else if (rule.MaximumValueType == ValueType.Percent)
            {
                rightEdgeRealValue = this.PercentData.GetRealValue((double)this.MaximumValue);
            }
            else if (rule.MaximumValueType == ValueType.Percentile)
            {
                rightEdgeRealValue = this.PercentileData.GetValueJustOverPercentile((double)this.MaximumValue);
            }

            if (canUseNegativeAxis)
            {
                RangeData negativeRangeSizing = RangeData.CreateRangeData(0, Math.Abs(leftEdgeRealValue));

                double negativeBarPercentageValue = negativeRangeSizing.GetPercentValue(Math.Abs(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture)));

                return this.GenerateStyle(negativeBarPercentageValue, DataBarPositiveNegative.Negative);
            }

            if (rule.DataBarDirection == DataBarDirection.Bidirectional)
            {
                RangeData positiveRangeSizing = RangeData.CreateRangeData(0, rightEdgeRealValue);

                double positiveBarPercentageValue = positiveRangeSizing.GetPercentValue((Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture)));

                return this.GenerateStyle(positiveBarPercentageValue, DataBarPositiveNegative.Positive);
            }

            RangeData newRangePercentData = RangeData.CreateRangeData(leftEdgeRealValue, rightEdgeRealValue, null, null, null);

            double percenValue = newRangePercentData.GetPercentValue(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture));

            return this.GenerateStyle(percenValue, DataBarPositiveNegative.Positive);
        }
        #endregion // EvaluateCondition

        private Style GenerateStyleForDiscreetEndPointedValues(object sourceDataValue, DataBarConditionalFormatRule rule)
        {
            if (this.MaximumValue == null || this.MinimumValue == null)
            {
                throw new NullConditionalFormatEvaluationValueException();
            }
            double convertedValue = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
            double minValue = (double)this.MinimumValue;
            double maxValue = (double)this.MaximumValue;
            DataBarPositiveNegative posNeg = DataBarPositiveNegative.Positive;

            


            RangeData tempPercentData = null;
            if (convertedValue >= 0)
            {
                


                if (rule.UseNegativeDataBar)
                {
                    minValue = 0;
                }
                

                tempPercentData = RangeData.CreateRangeData(minValue, maxValue, null, null, null);

                return this.GenerateStyle(tempPercentData.GetPercentValue(convertedValue), posNeg);
            }

            
            if (rule.UseNegativeDataBar)
            {
                minValue = 0;
                maxValue = Math.Abs((double)this.MinimumValue);
                posNeg = DataBarPositiveNegative.Negative;
                convertedValue = Math.Abs(convertedValue);
            }

            tempPercentData = RangeData.CreateRangeData(minValue, maxValue, null, null, null);

            return this.GenerateStyle(tempPercentData.GetPercentValue(convertedValue), posNeg);

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