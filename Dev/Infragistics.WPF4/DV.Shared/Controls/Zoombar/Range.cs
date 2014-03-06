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

namespace Infragistics.Controls
{
    /// <summary>
    /// Represents a Minimum - Maximum range.
    /// </summary>
    public class Range : DependencyObject
    {
        private const double MinScale = 0.001;
        private const double MaxScale = 1;

        private const double MinScroll = 0;
        private const double MaxScroll = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        public Range()
        {

        }

        #region Minimum

        /// <summary>
        /// Gets or sets the minimum value of the Range.
        /// </summary>
        /// <value>The minimum. The default is 0.</value>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(Range), new PropertyMetadata(0.0, new PropertyChangedCallback(OnMinimumChanged)));

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Range range = d as Range;

            range.OnMinimumChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Called when Minimum is changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMinimumChanged(double oldValue, double newValue)
        {
            if (this.Zoombar == null || this.Initialize)
            {
                return;
            }

            if (newValue < this.Zoombar.Minimum)
            {
                this.Minimum = this.Zoombar.Minimum;
                return;
            }

            double max = System.Math.Min(this.Zoombar.Maximum, this.Maximum);
            max = System.Math.Max(this.Zoombar.Minimum, max);

            if (newValue > max)
            {
                this.Minimum = max;
                return;
            }

            Range oldRange = new Range();
            this.Initialize = true;
            oldRange.Minimum = oldValue;
            oldRange.Maximum = this.Maximum;
            this.Initialize = false;

            this.Zoombar.ChangeRange(oldRange, this.Zoombar.Range);
        }

        #endregion

        #region Maximum

        /// <summary>
        /// Gets or sets the maximum value of the Range.
        /// </summary>
        /// <value>The maximum value. The default is 1.</value>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(Range), new PropertyMetadata(1.0, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Range range = d as Range;

            range.OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Called when Maximum is changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMaximumChanged(double oldValue, double newValue)
        {
            if (this.Zoombar == null || this.Initialize)
            {
                return;
            }

            if (newValue > this.Zoombar.Maximum)
            {
                this.Maximum = this.Zoombar.Maximum;
                return;
            }

            double min = System.Math.Max(this.Zoombar.Minimum, this.Minimum);
            min = System.Math.Min(this.Zoombar.Maximum, min);

            if (newValue < min)
            {
                this.Maximum = min;
                return;
            }

            Range oldRange = new Range();
            this.Initialize = true;
            oldRange.Minimum = this.Minimum;
            oldRange.Maximum = oldValue;
            this.Initialize = false;

            this.Zoombar.ChangeRange(oldRange, this.Zoombar.Range);
        }

        #endregion

        /// <summary>
        /// Gets the scale of the Zoombar.
        /// </summary>
        /// <remarks>
        /// The scale represents the ratio between the Range (Maximum - Minimum) and the Zoombar (Maximum - Minimum).
        /// </remarks>
        /// <value>The scale. The value is between 0 and 1.</value>
        public double Scale
        {
            get
            {
                double scale = 0;

                if (this.Zoombar == null)
                {
                    return scale;
                }

                double fullRange = this.Zoombar.Maximum - this.Zoombar.Minimum;
                double localRange = this.Maximum - this.Minimum;

                if (localRange > 0 && fullRange > 0)
                {
                    scale = localRange / fullRange;
                }

                if (scale < MinScale)
                {
                    scale = MinScale;
                }

                return scale;
            }
        }
        /// <summary>
        /// Gets the scroll of the Zoombar.
        /// </summary>
        /// <remarks>
        /// The scroll represents how the range element is moved along the track area.
        /// </remarks>
        /// <value>The scroll. The value is between 0 and 1.</value>
        public double Scroll
        {
            get
            {
                double scroll = 0;

                if (this.Zoombar == null)
                {
                    return scroll;
                }

                double fullRange = this.Zoombar.Maximum - this.Zoombar.Minimum;
                double localRange = this.Maximum - this.Minimum;

                double diffMin = this.Minimum - this.Zoombar.Minimum;
                double diffRanges = fullRange - localRange;

                if (diffRanges > 0)
                {
                    scroll = diffMin / diffRanges;
                }

                return scroll;
            }
        }

        /// <summary>
        /// Inits <see cref="Minimum"/> and <see cref="Maximum"/> from scale and scroll.
        /// </summary>
        /// <param name="scale">The scale.</param>
        /// <param name="scroll">The scroll.</param>
        public void FromScaleScroll(double scale, double scroll)
        {
            if (this.Zoombar == null)
            {
                return;
            }

            scale = GetPossibleScale(scale);
            scroll = GetPossibleScroll(scroll);

            double globalRange = this.Zoombar.Maximum - this.Zoombar.Minimum;

            double localRange = globalRange * scale;
            double rangeDiff = globalRange - localRange;

            double minimum = rangeDiff * scroll;
            double maximum = minimum + localRange;

            this.Initialize = true;
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Initialize = false;

            this.Zoombar.ChangeRange(null, this.Zoombar.Range);
        }

        private double GetPossibleScale(double scale)
        {
            double minScale = this.Zoombar.CalculateMinScale();

            if (minScale < MinScale)
            {
                minScale = MinScale;
            }

            if (scale < minScale)
            {
                scale = minScale;
            }
            else if (scale > MaxScale)
            {
                scale = MaxScale;
            }

            return scale;
        }
        private static double GetPossibleScroll(double scroll)
        {
            if (scroll < MinScroll)
            {
                scroll = MinScroll;
            }
            else if (scroll > MaxScroll)
            {
                scroll = MaxScroll;
            }

            return scroll;
        }

        private bool _initialize;
        internal bool Initialize
        {
            get { return this._initialize; }
            set { this._initialize = value; }
        }

        private XamZoombar _zoombar;
        internal XamZoombar Zoombar
        {
            get { return this._zoombar; }
            set { this._zoombar = value; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>        
        public override string ToString()
        {
            return string.Format("Minimum = {0}, Maximum = {1}", Minimum, Maximum);
        }

        internal Range Clone()
        {
            Range range = new Range();
            range.Initialize = true;
            range.Minimum = this.Minimum;
            range.Maximum = this.Maximum;
            range.Initialize = false;

            return range;
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