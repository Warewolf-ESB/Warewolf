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
using System.Collections.Generic;
using System.Windows.Data;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart range column series.
    /// </summary>
    public sealed class RangeColumnSeries : HorizontalRangeCategorySeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new RangeColumnSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            RangeColumnView = (RangeColumnSeriesView)view;
        }
        internal RangeColumnSeriesView RangeColumnView { get; set; }

        #region constructor and intialisation
        /// <summary>
        /// Initializes a new instance of the AreaSeries class. 
        /// </summary>
        public RangeColumnSeries()
        {
            DefaultStyleKey = typeof(RangeColumnSeries);
        }
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
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(RangeColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as RangeColumnSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(RangeColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as RangeColumnSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode2;
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var rangeColumnView = (RangeColumnSeriesView)view;
            if (wipeClean && rangeColumnView.Columns != null)
            {
                rangeColumnView.Columns.Count = 0;
            }
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);

            RangeColumnSeriesView rangeColumnView = view as RangeColumnSeriesView;

            List<float[]> buckets = frame.Buckets;
            
            // GetViewInfo(out viewportRect, out windowRect);
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            double groupWidth = XAxis.GetGroupSize(windowRect, viewportRect);

            if (double.IsNaN(groupWidth) || double.IsInfinity(groupWidth))
            {
                rangeColumnView.Columns.Count = 0;
                return;
            }
            for (int i = 0; i < buckets.Count; ++i)
            {
                double left = buckets[i][0] - 0.5 * groupWidth;
                // [DN 5/26/2010:32566] Min and Max functions are used here in case bottom is higher than top.  this can happen if YAxis.IsInverted is true.
                double top = Math.Min(buckets[i][1], buckets[i][2]);
                double bottom = Math.Max(buckets[i][1], buckets[i][2]);

                Rectangle rectangle = rangeColumnView.Columns[i];

                rectangle.Width = groupWidth;
                rectangle.Height = Math.Abs(bottom - top);
                rangeColumnView.PositionRectangle(rectangle, left, top);
            }

            rangeColumnView.Columns.Count = buckets.Count;
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