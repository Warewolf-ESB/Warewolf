using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart column series.
    /// <para>Compare values across categories by using vertical rectangles.</para>
    /// <para>Use it when the order of categories is not important or for displaying
    /// item counts such as a histogram.</para>
    /// </summary>
    public sealed class ColumnSeries : HorizontalAnchoredCategorySeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new ColumnSeriesView(this);
        }
        internal ColumnSeriesView ColumnView { get; set; }
        
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            ColumnView = (ColumnSeriesView)view;
        }


        #region constructor and intialisation
        /// <summary>
        /// Initializes a new instance of the ColumnSeries class. 
        /// </summary>
        public ColumnSeries()
        {
           

            DefaultStyleKey = typeof(ColumnSeries);
        }

        

        #endregion

        #region RadiusX Dependency Property
        /// <summary>
        /// Gets or sets the x-radius of the ellipse that is used to round the corners of the column.
        /// <para>
        /// This is a dependency property.
        /// </para>
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
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(ColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as ColumnSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region RadiusY Dependency Property
        /// <summary>
        /// Gets or sets the y-radius of the ellipse that is used to round the corners of the column.
        /// <para>
        /// This is a dependency property.
        /// </para>
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
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(ColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as ColumnSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
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

            var columnView = (ColumnSeriesView)view;
            if (wipeClean && columnView.Columns != null)
            {
                columnView.Columns.Count = 0;
            }
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);

            // this is the brain-dead version. one column per bucket

            List<float[]> buckets = frame.Buckets;

            if (!view.Ready())
            {
                return;
            }

            // GetViewInfo(out viewportRect, out windowRect);
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);
            
            NumericYAxis yscale = YAxis;

            double zero = yscale.GetScaledValue(yscale.ReferenceValue, yParams);
            double groupWidth = XAxis.GetGroupSize(windowRect, viewportRect);

            ColumnSeriesView columnView = (ColumnSeriesView)view;

            if (double.IsNaN(groupWidth) || double.IsInfinity(groupWidth))
            {
                columnView.Columns.Count = 0;
                return;
            }

            for (int i = 0; i < buckets.Count; ++i)
            {
                double left = buckets[i][0] - 0.5 * groupWidth;
                double top = buckets[i][1];
                double bottom = zero;

                //to avoid a glitch when most of the column renders outside the viewport
                //we clip the rectangle. 100px should be sufficient.
                top = Math.Max(top, -100);
                bottom = Math.Min(bottom, viewportRect.Height + 100);

                Rectangle column = columnView.Columns[i];
                column.Width = groupWidth;
                column.Height = Math.Abs(bottom - top);
                columnView.PositionRectangle(column, left, Math.Min(bottom, top));
            }

            columnView.Columns.Count = buckets.Count;
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