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
using Infragistics.Controls.Charts.Util;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base functionality for a XamDataChart financial overlay series.
    /// </summary>
    /// <remarks>
    /// The difference between a FinancialIndicator and a FinancialOverlay is small.
    /// Overlays are usually drawn against the same axes as the price, but they don't
    /// have to be. Overlays mostly display multiple values, but not all of them, and so
    /// so some indicators.
    /// </remarks>
    public abstract class FinancialOverlay : FinancialSeries
    {
        #region constructor and initialisation
        /// <summary>
        /// FinancialOverlay constructor.
        /// </summary>
        protected FinancialOverlay()
        {
            OverlayValid = false;
        }
        #endregion

        #region Data
        /// <summary>
        /// When overridden in a derived class, DataChangedOverride is called whenever a change is made to
        /// the series data.
        /// </summary>
        /// <param name="action">The action performed on the data</param>
        /// <param name="position">The index of the first item involved in the update.</param>
        /// <param name="count">The number of items in the update.</param>
        /// <param name="propertyName">The name of the updated property.</param>
        protected override void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            if (XAxis != null && XAxis is ISortingAxis)
            {
                (XAxis as ISortingAxis).NotifyDataChanged();
            }

            this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
            ValidateOverlay();

            if (YAxis != null)
            {
                YAxis.UpdateRange();
            }
            RenderSeries(true);
        }
        #endregion

        #region IgnoreFirst Depedency Property
        /// <summary>
        /// Gets or sets the number of values to hide at the beginning of the indicator.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public int IgnoreFirst
        {
            get
            {
                return (int)GetValue(IgnoreFirstProperty);
            }
            set
            {
                SetValue(IgnoreFirstProperty, value);
            }
        }

        internal const string IgnoreFirstPropertyName = "IgnoreFirst";

        /// <summary>
        /// Identifies the IgnoreFirst dependency property.
        /// </summary>
        public static readonly DependencyProperty IgnoreFirstProperty = DependencyProperty.Register(IgnoreFirstPropertyName, typeof(int), typeof(FinancialOverlay),
            new PropertyMetadata(0, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as FinancialOverlay).RaisePropertyChanged(IgnoreFirstPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case FastItemsSourcePropertyName:
                    OverlayValid = false;
                    break;
                case IgnoreFirstPropertyName:
                    OverlayValid = false;
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case XAxisPropertyName:
                    OverlayValid = false;
                    break;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RenderSeries(false);
        }

        /// <summary>
        /// Scrolls the series to display the item for the specified data item.
        /// </summary>
        /// <remarks>
        /// The series is scrolled by the minimum amount required to place the specified data item within
        /// the central 80% of the visible axis.
        /// </remarks>
        /// <param name="item">The data item (item) to scroll to.</param>
        /// <returns>True if the specified item could be displayed.</returns>
        public override bool ScrollIntoView(object item)
        {
            int index = FastItemsSource != null ? FastItemsSource[item] : -1;
            //Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //Rect viewportRect = View.Viewport;

            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            Rect unitRect = new Rect(0, 0, 1, 1);

            ScalerParams xParams = new ScalerParams(unitRect, unitRect, XAxis.IsInverted);
            ScalerParams yParams = new ScalerParams(unitRect, unitRect, YAxis.IsInverted);

            if (index >= 0 && windowRect != null && viewportRect != null)
            {
                if (XAxis != null)
                {
                    double cx = XAxis.GetScaledValue(index, xParams);

                    if (cx < windowRect.Left + 0.1 * windowRect.Width)
                    {
                        cx = cx + 0.4 * windowRect.Width;
                    }

                    if (cx > windowRect.Right - 0.1 * windowRect.Width)
                    {
                        cx = cx - 0.4 * windowRect.Width;
                    }

                    windowRect.X = cx - 0.5 * windowRect.Width;
                }

                if (YAxis != null && LowColumn != null && HighColumn != null && index < LowColumn.Count && index < HighColumn.Count)
                {
                    // scroll so that low and high are both in range
                    double low = YAxis.GetScaledValue(LowColumn[index], yParams);
                    double high = YAxis.GetScaledValue(HighColumn[index], yParams);

                    if (!double.IsNaN(low) && !double.IsNaN(high))
                    {

                        double height = Math.Abs(low - high);

                        if (windowRect.Height < height)
                        {
                            windowRect.Height = height;
                            windowRect.Y = Math.Min(low, high);
                        }
                        else
                        {
                            if (low < windowRect.Top + 0.1 * windowRect.Height)
                            {
                                low = low + 0.4 * windowRect.Height;
                            }

                            if (low > windowRect.Bottom - 0.1 * windowRect.Height)
                            {
                                low = low - 0.4 * windowRect.Height;
                            }

                            windowRect.Y = low - 0.5 * windowRect.Height;
                        }
                    }
                }

                SyncLink.WindowNotify(SeriesViewer, windowRect);
            }

            return index >= 0;
        }
        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (axis != null && axis == YAxis && LowColumn != null && HighColumn != null)
            {
                return new AxisRange(LowColumn.Minimum, HighColumn.Maximum);
            }

            return null;
        }
        /// <summary>
        /// Boolean property indicating whether or not this overlay is valid.
        /// </summary>
        protected bool OverlayValid { get; set; }
        /// <summary>
        /// Validates this overlay.
        /// </summary>
        /// <returns>True if the overlay is valid, otherwise False.</returns>
        protected abstract bool ValidateOverlay();
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