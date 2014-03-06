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

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a brush scale that uses value-based brush selection.
    /// </summary>
    public class ValueBrushScale:BrushScale
    {
        /// <summary>
        /// Creates a new instance of the ValueBrushScale.
        /// </summary>
        public ValueBrushScale()
        {
           
        }

        #region MinimumValue Dependency Property

        internal const string MinimumValuePropertyName = "MinimumValue";

        /// <summary>
        /// Identifies the MinimumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register(MinimumValuePropertyName, typeof(double), typeof(ValueBrushScale),
            new PropertyMetadata(double.NaN, (o, e) =>
            {
                (o as ValueBrushScale).RaisePropertyChanged(MinimumValuePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the minimum value for this scale.
        /// </summary>
        public double MinimumValue
        {
            get
            {
                return (double)GetValue(MinimumValueProperty);
            }
            set
            {
                SetValue(MinimumValueProperty, value);
            }
        }
        #endregion

        #region MaximumValue Dependency Property
        internal const string MaximumValuePropertyName = "MaximumValue";

        /// <summary>
        /// Identifies the MaximumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register(MaximumValuePropertyName, typeof(double), typeof(ValueBrushScale),
            new PropertyMetadata(double.NaN, (o, e) =>
            {
                (o as ValueBrushScale).RaisePropertyChanged(MaximumValuePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the maximum value for this scale.
        /// </summary>
        public double MaximumValue
        {
            get
            {
                return (double)GetValue(MaximumValueProperty);
            }
            set
            {
                SetValue(MaximumValueProperty, value);
            }
        }
        #endregion

        #region IsLogarithmic Dependency Property
        internal const string IsLogarithmicPropertyName = "IsLogarithmic";

        /// <summary>
        /// Identifies the IsLogarithmic dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLogarithmicProperty =
            DependencyProperty.Register(IsLogarithmicPropertyName, typeof(bool), typeof(ValueBrushScale),
            new PropertyMetadata(false, (o, e) =>
            {
                (o as ValueBrushScale).RaisePropertyChanged(IsLogarithmicPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether the scale is logarithmic.
        /// </summary>
        public bool IsLogarithmic
        {
            get
            {
                return (bool)GetValue(IsLogarithmicProperty);
            }
            set
            {
                SetValue(IsLogarithmicProperty, value);
            }
        }
        #endregion

        #region LogarithmBase Dependency Property
        internal const string LogarithmBasePropertyName = "LogarithmBase";

        /// <summary>
        /// Identifies the LogarithmBase dependency property.
        /// </summary>
        public static readonly DependencyProperty LogarithmBaseProperty =
            DependencyProperty.Register(LogarithmBasePropertyName, typeof(int), typeof(ValueBrushScale),
            new PropertyMetadata(10, (o, e) =>
            {
                (o as ValueBrushScale).RaisePropertyChanged(LogarithmBasePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the logarithm base for this scale.
        /// </summary>
        public int LogarithmBase
        {
            get
            {
                return (int)GetValue(LogarithmBaseProperty);
            }
            set
            {
                SetValue(LogarithmBaseProperty, value);
            }
        }
        #endregion

        /// <summary>
        /// Selects a brush from the brush collection by index.
        /// </summary>
        /// <param name="index">Index of the brush.</param>
        /// <param name="FillColumn">Datasource column used to calculate the brush scale.</param>
        /// <returns>Brush for a specified index.</returns>
        public virtual Brush GetBrushByIndex(int index, IFastItemColumn<double> FillColumn)
        {
            if (FillColumn == null || Brushes == null || Brushes.Count == 0 || index < 0 || index >= FillColumn.Count)
            {
                return null;
            }

            if (FillColumn.Count == 0)
            {
                return Brushes[0];
            }

            double min = double.IsNaN(MinimumValue) || double.IsInfinity(MinimumValue)? FillColumn.Minimum : MinimumValue;
            double max = double.IsNaN(MaximumValue) || double.IsInfinity(MaximumValue) ? FillColumn.Maximum : MaximumValue;
            double value = FillColumn[index];

            if (min == max)
            {
                return value == min ? Brushes[0] : null;
            }

            return GetInterpolatedBrushLogarithmic(min, max, value);
        }

        /// <summary>
        /// Selects a brush from the brush colleciton by value.
        /// </summary>
        /// <param name="value">Value used to get an interpolated brush.</param>
        /// <param name="FillColumn">Datasource column used to calculate the brush scale.</param>
        /// <returns>Brush for a specified value.</returns>
        public virtual Brush GetBrushByValue(double value, IFastItemColumn<double> FillColumn)
        {
            if (FillColumn == null || Brushes == null || Brushes.Count == 0)
            {
                return null;
            }

            if (FillColumn.Count <= 1)
            {
                return Brushes[0];
            }

            double min = double.IsNaN(MinimumValue) || double.IsInfinity(MinimumValue) ? FillColumn.Minimum : MinimumValue;
            double max = double.IsNaN(MaximumValue) || double.IsInfinity(MaximumValue) ? FillColumn.Maximum : MaximumValue;

            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            return GetInterpolatedBrushLogarithmic(min, max, value);
        }

        private Brush GetInterpolatedBrushLogarithmic(double min, double max, double value)
        {
            if (IsLogarithmic && LogarithmBase > 0)
            {
                double newMin = Math.Log(min, LogarithmBase);
                double newMax = Math.Log(max, LogarithmBase);
                double newValue = Math.Log(value, LogarithmBase);

                return GetInterpolatedBrushLinear(newMin, newMax, newValue);
            }

            return GetInterpolatedBrushLinear(min, max, value);
        }

        private Brush GetInterpolatedBrushLinear(double min, double max, double value)
        {
            //if the value is outside the range, return a null brush
            if (value < min || value > max) return null;

            double scaledValue = (value - min)/(max - min);
            double scaledBrushIndex = scaledValue * (Brushes.Count - 1);

            return GetInterpolatedBrush(scaledBrushIndex);
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