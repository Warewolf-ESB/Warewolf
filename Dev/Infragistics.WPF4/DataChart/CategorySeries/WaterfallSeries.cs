using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart waterfall column series.
    /// </summary>
    public sealed class WaterfallSeries : HorizontalAnchoredCategorySeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new WaterfallSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            WaterfallView = (WaterfallSeriesView)view;
        }
        internal WaterfallSeriesView WaterfallView { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a new instance of the WaterfallSeries class. 
        /// </summary>
        public WaterfallSeries()
        {
            DefaultStyleKey = typeof(WaterfallSeries);
        }
        #endregion

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode2;
        }

        #region NegativeBrush Dependency Property
        /// <summary>
        /// Gets or sets the brush to use for negative portions of the series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush NegativeBrush
        {
            get
            {
                return (Brush)GetValue(NegativeBrushProperty);
            }
            set
            {
                SetValue(NegativeBrushProperty, value);
            }
        }

        internal const string NegativeBrushPropertyName = "NegativeBrush";

        /// <summary>
        /// Identifies the Fill dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeBrushProperty = DependencyProperty.Register(NegativeBrushPropertyName, typeof(Brush), typeof(WaterfallSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as WaterfallSeries).RaisePropertyChanged(NegativeBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
 
        #region RadiusX Dependency Property
        /// <summary>
        /// Gets or sets the x-radius of the ellipse that is used to round the corners of the column.
        /// </summary> 
        [WidgetDefaultNumber(2.0)]
        public double RadiusX
        {
            get
            {
                return (double)GetValue(RadiusXProperty);
            }
            set
            {
                SetValue(RadiusXProperty, value);
            }
        }

        internal const string RadiusXPropertyName = "RadiusX";

        /// <summary>
        /// Identifies the RadiusX dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(WaterfallSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as WaterfallSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region RadiusY Dependency Property
        /// <summary>
        /// Gets or sets the y-radius of the ellipse that is used to round the corners of the column.
        /// </summary>
        [WidgetDefaultNumber(2.0)]
        public double RadiusY
        {
            get
            {
                return (double)GetValue(RadiusYProperty);
            }
            set
            {
                SetValue(RadiusYProperty, value);
            }
        }

        internal const string RadiusYPropertyName = "RadiusY";

        /// <summary>
        /// Identifies the RadiusY dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(WaterfallSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as WaterfallSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
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
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case RadiusYPropertyName:
                case RadiusXPropertyName:
                case NegativeBrushPropertyName:
                    RenderSeries(false);
                    break;
            }
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var waterfallView = (WaterfallSeriesView)view;
            waterfallView.ClearWaterfall();
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            WaterfallSeriesView waterfallView = (WaterfallSeriesView)view;
            waterfallView.ClearWaterfall();

            base.RenderFrame(frame, view);

            GeometryGroup positiveGroup = waterfallView.GetPositiveGeometryGroup();
            GeometryGroup negativeGroup = waterfallView.GetNegativeGeometryGroup();
            
            // GetViewInfo(out viewportRect, out windowRect);
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            double groupWidth = XAxis.GetGroupSize(windowRect, viewportRect);
            double radiusX=RadiusX;
            double radiusY=RadiusY;

            RectangleGeometry rectangleGeometry;
            Rect rect;
            double left;

            double zeroVal = GetWorldZeroValue(view);
            double lastKnownValue = double.NaN;
            if (frame.Buckets.Count > 0)
            {
                left = frame.Buckets[0][0] - 0.5 * groupWidth;
                float currentValue = frame.Buckets[0][1];
                if (!double.IsNaN(currentValue))
                {
                    //reference value (the zero value) is used in the first column of the waterfall series only.
                    //however, depending on the reference value the first group can be positive or negative.
                    if (currentValue > zeroVal)
                    {
                        //zero value above the column value, the group should be negative
                        rect = new Rect(left, zeroVal, groupWidth, Math.Abs(zeroVal - currentValue));
                        rectangleGeometry = new RectangleGeometry() { Rect = rect, RadiusX = radiusX, RadiusY = radiusY };
                        negativeGroup.Children.Add(rectangleGeometry);
                    }
                    else
                    {
                        //zero value below the column value, the group should be positive
                        rect = new Rect(left, currentValue, groupWidth, Math.Abs(currentValue - zeroVal));
                        rectangleGeometry = new RectangleGeometry() { Rect = rect, RadiusX = radiusX, RadiusY = radiusY };
                        positiveGroup.Children.Add(rectangleGeometry);
                    }
                    lastKnownValue = currentValue;
                }
                else
                {
                    lastKnownValue = zeroVal;
                }
            }

            for (int i = 1; i < frame.Buckets.Count; ++i)
            {
                float[] bucket1 = frame.Buckets[i];
                left = frame.Buckets[i][0] - 0.5 * groupWidth;

                double currentValue = bucket1[1];
                if (!double.IsNaN(currentValue))
                {
                    rect = new Rect(left, Math.Min(lastKnownValue, currentValue), groupWidth, Math.Abs(lastKnownValue - currentValue));
                    rectangleGeometry = new RectangleGeometry() { Rect = rect, RadiusX = radiusX, RadiusY = radiusY };

                    (lastKnownValue > currentValue ? positiveGroup : negativeGroup).Children.Add(rectangleGeometry);

                    lastKnownValue = currentValue;
                }
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