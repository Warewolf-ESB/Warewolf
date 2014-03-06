using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart stacked column series.
    /// </summary>
    public class StackedColumnSeries : HorizontalStackedSeriesBase
    {
        #region C'tor & Initialization
        /// <summary>
        /// Initializes a new instance of a StackedColumnSeries class.
        /// </summary>
        public StackedColumnSeries()
        {
            DefaultStyleKey = typeof(StackedColumnSeries);
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
        #endregion

        #region View-related
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new StackedColumnSeriesView(this);
        }
        
        internal StackedColumnSeriesView StackedColumnView { get; set; }

        /// <summary>
        /// Called when the view has been created.
        /// </summary>
        /// <param name="view">The view class for the current series</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            StackedColumnView = (StackedColumnSeriesView)view;
        }
        #endregion

        #region Public Properties
        #region RadiusX Dependency Property
        /// <summary>
        /// Gets or sets the x-radius of the ellipse that is used to round the corners of the column.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>  
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
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(StackedColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as StackedColumnSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region RadiusY Dependency Property
        /// <summary>
        /// Gets or sets the y-radius of the ellipse that is used to round the corners of the column.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
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
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(StackedColumnSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as StackedColumnSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        #endregion

        #region Method Overrides

        internal override CategorySeriesView GetSeriesView()
        {
            return StackedColumnView;
        }

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode2;
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (ItemsSource == null)
            {
                return null;
            }

            if (axis == XAxis)
            {
                return new AxisRange(0, FastItemsSource.Count - 1);
            }

            if (axis == YAxis)
            {
                PrepareData();
                return new AxisRange(Minimum, Maximum);
            }

            return null;
        }

        internal override void RenderFragment(AnchoredCategorySeries series, CategoryFrame frame, CategorySeriesView view)
        {
            ColumnFragment columnSeries = series as ColumnFragment;
            ColumnFragmentView fragmentView = view as ColumnFragmentView;
            if (!ValidateSeries(view.Viewport, view.WindowRect, view) || columnSeries == null || fragmentView == null)
            {
                return;
            }
            
            if (columnSeries == null) return;

            double groupWidth = XAxis.GetGroupSize(view.WindowRect, view.Viewport);

            if (double.IsNaN(groupWidth) || double.IsInfinity(groupWidth))
            {
                columnSeries.ColumnFragmentView.Columns.Count = 0;
                return;
            }

            int counter = 0;

            foreach (var bucket in frame.Buckets)
            {
                //avoid trying to render rectangles with invalid dimensions, skip to the next one.
                if (double.IsInfinity(bucket[0]) || double.IsNaN(bucket[0])
                    || double.IsInfinity(bucket[1]) || double.IsInfinity(bucket[2])
                    || double.IsNaN(bucket[1]) || double.IsNaN(bucket[2]))
                {
                    continue;
                }

                double left = bucket[0] - 0.5 * groupWidth;
                double top = bucket[1];
                double bottom = bucket[2];

                //to avoid a glitch when most of the column renders outside the viewport
                //we clip the rectangle. 100px should be sufficient.
                top = Math.Max(top, -100);
                bottom = Math.Min(bottom, view.Viewport.Height + 100);

                Rectangle column = fragmentView.Columns[counter];
                column.Width = groupWidth;
                column.Height = Math.Abs(bottom - top);
                column.RenderTransform = new TranslateTransform() { X = left, Y = Math.Min(bottom, top) };

                counter++;
            }

            fragmentView.Columns.Count = counter;
        }
        #endregion

        #region Property Updates
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
                case RadiusXPropertyName:
                case RadiusYPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateRadiusX();
                        series.UpdateRadiusY();
                    }
                    RenderSeries(false);
                    break;

                case SyncLinkPropertyName:
                    if (YAxis != null) YAxis.UpdateRange();
                    break;

                case SeriesViewerPropertyName:
                    if (YAxis != null) YAxis.UpdateRange();
                    break;
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