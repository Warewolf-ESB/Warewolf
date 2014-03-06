using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a scale that is used determine an object's size.
    /// </summary>
    [DontObfuscate]
    [WidgetModule("ScatterSeries")]
    public class SizeScale:DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new instance of SizeScale.
        /// </summary>
        public SizeScale()
        {
            Series = new List<Series>();
            PropertyUpdated += (o, e) => PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue);
        }

        /// <summary>
        /// Gets or sets the collection of series objects.
        /// </summary>
        protected internal List<Series> Series { get; set; }

        #region MinimumValue Dependency Property

        internal const string MinimumValuePropertyName = "MinimumValue";

        /// <summary>
        /// Identifies the MinimumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register(MinimumValuePropertyName, typeof(double), typeof(SizeScale),
            new PropertyMetadata(double.NaN, (o, e) =>
            {
                (o as SizeScale).RaisePropertyChanged(MinimumValuePropertyName, e.OldValue, e.NewValue);
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
            DependencyProperty.Register(MaximumValuePropertyName, typeof(double), typeof(SizeScale),
            new PropertyMetadata(double.NaN, (o, e) =>
            {
                (o as SizeScale).RaisePropertyChanged(MaximumValuePropertyName, e.OldValue, e.NewValue);
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
            DependencyProperty.Register(IsLogarithmicPropertyName, typeof(bool), typeof(SizeScale),
            new PropertyMetadata(false, (o, e) =>
            {
                (o as SizeScale).RaisePropertyChanged(IsLogarithmicPropertyName, e.OldValue, e.NewValue);
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
            DependencyProperty.Register(LogarithmBasePropertyName, typeof(int), typeof(SizeScale),
            new PropertyMetadata(10, (o, e) =>
            {
                (o as SizeScale).RaisePropertyChanged(LogarithmBasePropertyName, e.OldValue, e.NewValue);
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

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyUpdatedEventHandler PropertyUpdated;

        /// <summary>
        /// Raises the property changed and updated events.
        /// </summary>
        /// <param name="name">The name of the property being changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected void RaisePropertyChanged(string name, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(name, oldValue, newValue));
            }
        }
        #endregion

        #region PropertyUpdated Handler
        /// <summary>
        /// Handles property updates.
        /// </summary>
        /// <param name="sender">source object</param>
        /// <param name="propertyName">property name</param>
        /// <param name="oldValue">old property value</param>
        /// <param name="newValue">new property value</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            foreach (var series in Series)
            {
                series.RenderSeries(false);
                if (series.SeriesViewer != null)
                {
                    series.NotifyThumbnailAppearanceChanged();
                }
            }
        }
        #endregion
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