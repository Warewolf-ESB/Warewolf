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
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a numeric axis scaler.
    /// </summary>
    public abstract class NumericScaler : DependencyObject
    {
        /// <summary>
        /// Calculates the range of the scaler.
        /// </summary>
        /// <param name="target">Target axis</param>
        /// <param name="minimumValue">Desired minimum value</param>
        /// <param name="maximumValue">Desired maximum value</param>
        /// <param name="actualMinimumValue">Actual minimum value</param>
        /// <param name="actualMaximumValue">Actual maximum value</param>
        public abstract void CalculateRange(NumericAxisBase target, double minimumValue, double maximumValue, out double actualMinimumValue, out double actualMaximumValue);
        
        /// <summary>
        /// String used to determine the name of the ActualMinimumValue dependency property.
        /// </summary>
        protected const string ActualMinimumValuePropertyName = "ActualMinimumValue";
        
        /// <summary>
        /// Identifies the ActualMinimumValue dependency property.
        /// </summary>
        protected internal static readonly DependencyProperty ActualMinimumValueProperty = DependencyProperty.Register(ActualMinimumValuePropertyName, typeof(double), typeof(NumericScaler), new PropertyMetadata(double.NaN, (sender, e) =>
        {
            (sender as NumericScaler).OnPropertyChanged(ActualMinimumValuePropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the actual minimum value.
        /// </summary>
        protected internal double ActualMinimumValue
        {
            get
            {
                return (double)this.GetValue(ActualMinimumValueProperty);
            }
            protected set
            {
                this.SetValue(ActualMinimumValueProperty, value);
            }
        }

        /// <summary>
        /// String used to determine the name of the ActualMaximumValue dependency property.
        /// </summary>
        protected const string ActualMaximumValuePropertyName = "ActualMaximumValue";

        /// <summary>
        /// Identifies the ActualMaximumValue dependency property.
        /// </summary>
        protected internal static readonly DependencyProperty ActualMaximumValueProperty = DependencyProperty.Register(ActualMaximumValuePropertyName, typeof(double), typeof(NumericScaler), new PropertyMetadata(double.NaN, (sender, e) =>
        {
            (sender as NumericScaler).OnPropertyChanged(ActualMaximumValuePropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the actual maximum value.
        /// </summary>
        protected internal double ActualMaximumValue
        {
            get
            {
                return (double)this.GetValue(ActualMaximumValueProperty);
            }
            protected set
            {
                this.SetValue(ActualMaximumValueProperty, value);
            }
        }
        /// <summary>
        /// A copy of the ActualMinimumValue property value, cached for performance reasons.
        /// </summary>
        protected double CachedActualMinimumValue;
        /// <summary>
        /// A copy of the ActualMaximumValue property value, cached for performance reasons.
        /// </summary>
        protected double CachedActualMaximumValue;

        internal void SetActualMinimumValue(double value)
        {
            ActualMinimumValue = value;
        }

        internal void SetActualMaximumValue(double value)
        {
            ActualMaximumValue = value;
        }

        /// <summary>
        /// Called when a property value changes.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case ActualMinimumValuePropertyName:
                    CachedActualMinimumValue = this.ActualMinimumValue;
                    this.UpdateActualRange();
                    break;
                case ActualMaximumValuePropertyName:
                    CachedActualMaximumValue = this.ActualMaximumValue;
                    this.UpdateActualRange();
                    break;
            }

        }
        private void UpdateActualRange()
        {
            if (double.IsNaN(this.ActualMinimumValue) ||
                double.IsNaN(this.ActualMaximumValue) ||
                double.IsInfinity(this.ActualMinimumValue) ||
                double.IsInfinity(this.ActualMaximumValue) ||
                this.ActualMinimumValue < (double)(Decimal.MinValue) ||
                this.ActualMaximumValue > (double)(Decimal.MaxValue)
                )
            {
                this.ActualRange = this.ActualMaximumValue - this.ActualMinimumValue;
            }
            else
            {



                bool useDecimalPrecision = this.ActualMaximumValue < XamDataChart.DecimalMaximumValueAsDouble && this.ActualMaximumValue > XamDataChart.DecimalMinimumValueAsDouble && this.ActualMinimumValue > XamDataChart.DecimalMinimumValueAsDouble && this.ActualMinimumValue < XamDataChart.DecimalMaximumValueAsDouble;
                if (useDecimalPrecision)
                {
                    // avoid rounding error by doing decimal subtraction then converting back to double.
                    this.ActualRange = Convert.ToDouble(Convert.ToDecimal(this.ActualMaximumValue) - Convert.ToDecimal(this.ActualMinimumValue));
                }
                else
                {
                    this.ActualRange = this.ActualMaximumValue - this.ActualMinimumValue;
                }

            }
        }

        /// <summary>
        /// Gets or sets the actual scaler range.
        /// </summary>
        protected double ActualRange { get; private set; }

        /// <summary>
        /// Returns an unscaled value in the viewport.
        /// </summary>
        /// <param name="scaledValue">The scaled value.</param>
        /// <param name="p">Scaler parameters.</param>
        /// <returns>Unscaled value in the viewport</returns>
        public abstract double GetUnscaledValue(double scaledValue, ScalerParams p);

        /// <summary>
        /// Gets a value on a numeric scale.
        /// </summary>
        /// <param name="unscaledValue">The value to be scaled.</param>
        /// <param name="p">Scaler parameters.</param>
        /// <returns>Value on a numeric scale</returns>
        public abstract double GetScaledValue(double unscaledValue, ScalerParams p);

        /// <summary>
        /// Returns a list of unscaled values in the viewport.
        /// </summary>
        /// <param name="scaledValues">list of scaled values</param>
        /// <param name="p">Scaler parameters.</param>
        /// <returns>List of unscaled values in the viewport</returns>
        public virtual IList<double> GetUnscaledValueList(IList<double> scaledValues, ScalerParams p)
        {
            IList<double> result = new List<double>(scaledValues.Count);
            for (int i = 0; i < scaledValues.Count; i++)
            {
                result.Add(GetUnscaledValue(scaledValues[i], p));
            }
            return result;
        }

        /// <summary>
        /// Gets a list of values on a numeric scale.
        /// </summary>
        /// <param name="unscaledValues">values to be scaled</param>
        /// <param name="p">Scaler parameters.</param>
        /// <returns>List of values on a numeric scale</returns>
        public virtual void GetScaledValueList(IList<double> unscaledValues, ScalerParams p)
        {
            for (int i = 0; i < unscaledValues.Count; i++)
            {
                unscaledValues[i] = GetScaledValue(unscaledValues[i], p);
            }
        }
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