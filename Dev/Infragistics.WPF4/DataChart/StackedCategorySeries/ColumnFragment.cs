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
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents one part of a StackedColumnSeries.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class ColumnFragment : FragmentBase
    {
        #region Initialization
        /// <summary>
        /// Creates a new instance of the ColumnFragment.
        /// </summary>
        internal ColumnFragment()
        {
            DefaultStyleKey = typeof(ColumnFragment);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (ParentSeries != null && ParentSeries.MarkerCanvas != null)
            {
                SetMarkerCanvas(ParentSeries.MarkerCanvas);
            }
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
            return new ColumnFragmentView(this);
        }
        internal ColumnFragmentView ColumnFragmentView { get; set; }
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            ColumnFragmentView = (ColumnFragmentView)view;
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
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(ColumnFragment),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as ColumnFragment).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(ColumnFragment),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as ColumnFragment).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        /// <summary>
        /// Gets or sets the effective x-axis for this series.
        /// </summary>

        public new CategoryAxisBase XAxis
        {
            get
            {
                return ParentSeries != null ? ParentSeries.GetXAxis() as CategoryAxisBase : null;
            }
        }
        /// <summary>
        /// Gets or sets the effective y-axis for this series.
        /// </summary>
        public new NumericYAxis YAxis
        {
            get
            {
                return ParentSeries != null ? ParentSeries.GetYAxis() as NumericYAxis : null;
            }
        }

        #endregion

        #region Non-public Properties
        //internal CategoryFramePreparer FramePreparer { get; set; }
        #endregion

        #region Member Overrides
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

            var columnFragmentView = (ColumnFragmentView)view;
            if (wipeClean && columnFragmentView.Columns != null)
            {
                CurrentFrame.Markers.Clear();
                columnFragmentView.Columns.Count = 0;
            }
        }
        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (ValueColumn == null || ValueColumn.Count == 0)
            {
                return null;
            }

            return new AxisRange(ValueColumn.Minimum, ValueColumn.Maximum);
        }
        /// <summary>
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal override bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            if (ParentSeries == null) return false;
            return ParentSeries.ValidateFragmentSeries(this, viewportRect, windowRect, this.GetParentView(view));
        }

        /// <summary>
        /// Determines if the current series renders its markers to the MarkerCanvas of the parent series. 
        /// Ensures that StackedFragmentSeries don't reparent the MarkerCanvas.
        /// </summary>
        /// <returns>Whether or not to use parent series marker canvas.</returns>
        protected internal override bool UseParentMarkerCanvas()
        {
            return true;
        }

        /// <summary>
        /// When overridden in a derived class gives the opportunity to define how the data source item
        /// for a given set of mouse coordinates is fetched.
        /// </summary>
        /// <param name="sender">The element the mouse is over.</param>
        /// <param name="point">The mouse coordinates for which to fetch the item.</param>
        /// <returns>The retrieved item.</returns>
        protected internal override object Item(object sender, Point point)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            DataContext dataContext = frameworkElement != null ? frameworkElement.DataContext as DataContext : null;
            object item = dataContext != null ? dataContext.Item : null;

            if (item == null)
            {
                Rect viewportRect = new Rect(0, 0, ParentSeries.ActualWidth, ParentSeries.ActualHeight);
                Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
                Point world = new Point(
                    windowRect.Left + windowRect.Width * (point.X - viewportRect.Left) / viewportRect.Width,
                    windowRect.Top + windowRect.Height * (point.Y - viewportRect.Top) / viewportRect.Height);

                item = GetItem(world);
            }

            return item;
        }
        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            int index = 0;

            if (this.XAxis is ISortingAxis)
            {
                index = GetItemIndexSorted(world);
                if (index == -1)
                {
                    return null;
                }
            }
            else
            {
                index = GetItemIndex(world);
            }

            return index >= 0
                && FastItemsSource != null
                && index < FastItemsSource.Count ? FastItemsSource[index] : null;
        }
        /// <summary>
        /// Gets the index of the item based on world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The index of the item for the specified coordinates.</returns>
        protected internal override int GetItemIndexSorted(Point world)
        {
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            if (windowRect.IsEmpty || viewportRect.IsEmpty)
            {
                return -1;
            }
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);

            ISortingAxis sorting = XAxis as ISortingAxis;

            double left = XAxis.GetUnscaledValue(viewportRect.Left, xParams);
            double right = XAxis.GetUnscaledValue(viewportRect.Right, xParams);
            double windowX = (world.X - windowRect.Left) / windowRect.Width;
            double axisValue = left + ((right - left) * windowX);

            if ((long)axisValue <= DateTime.MinValue.Ticks || (long)axisValue >= DateTime.MaxValue.Ticks)
            {
                return -1;
            }
            int itemIndex = sorting.GetIndexClosestToUnscaledValue(axisValue);
            return itemIndex;
        }

        /// <summary>
        /// Gets the index of the item that resides at the provided world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates of the requested item.</param>
        /// <returns>The requested item's index.</returns>
        protected override int GetItemIndex(Point world)
        {
            if (ParentSeries == null) return -1;

            return ParentSeries.GetFragmentItemIndex(world);
        }

        internal override void PrepareMarker(ISupportsMarkers markersHost, CategoryFrame frame, float[] bucket, IDetectsCollisions collisionAvoider,
            double value, int itemIndex, int markerCount)
        {
            //double zero = ParentSeries.GetUnscaledWorldZeroValue();
            double zero = 0.0;
            double x = bucket[0];
            double y = value < zero ? bucket[2] : bucket[1];

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

            if (!double.IsNaN(x) && !double.IsNaN(y) && !double.IsInfinity(x) && !double.IsInfinity(y) && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));
                markersHost.UpdateMarkerTemplate(markerCount, itemIndex);
            }
        }
        private SeriesView GetParentView(SeriesView view)
        {
            if (view == this.ThumbnailView)
            {
                return this.ParentSeries.ThumbnailView;
            }
            else
            {
                return this.ParentSeries.CategoryView;
            }
        }
        /// <summary>
        /// Create buckets and markers for the column fragment.
        /// </summary>
        /// <remarks>
        /// The buckets of the column fragment are managed entirely by the parent series. 
        /// CurrentFrame will always be re-populated regardless of which frame got passed into PrepareFrame.
        /// </remarks>
        internal override void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
        
            if (ValueColumn == null
                || ParentSeries == null
                || LogicalSeriesLink == null
                || LogicalSeriesLink.HighValues.Count == 0
                || LogicalSeriesLink.LowValues.Count == 0)
            {
                return;
            }

            CategoryFrame parentFrame;
            
            if (view == this.ThumbnailView)
            {
                parentFrame = this.ParentSeries.ThumbnailFrame;
            }
            else
            {
                parentFrame = this.ParentSeries.CurrentFrame;
            }
            CategorySeriesView parentView = this.GetParentView(view) as CategorySeriesView;
            frame.Buckets.Clear();
            frame.Markers.Clear();

            int firstBucket = parentView.BucketCalculator.FirstBucket;
            int lastbucket = parentView.BucketCalculator.LastBucket;
            IScaler yScaler = ParentSeries.FramePreparer.CategoryBasedHost.YScaler;
            ISortingAxis sortingScaler = ParentSeries.FramePreparer.CategoryBasedHost.Scaler as ISortingAxis;
            bool isLogarithmicYScaler = yScaler is NumericAxisBase && (yScaler as NumericAxisBase).IsReallyLogarithmic;

            StackedBucketCalculator bucketCalculator = parentView.BucketCalculator as StackedBucketCalculator;

            for (int i = firstBucket; i <= lastbucket; i++)
            {
                if (Visibility != Visibility.Visible) break;
                if (i >= ValueColumn.Count || i >= parentFrame.Buckets.Count + firstBucket) continue;

                double value = ValueColumn[i];
                bool isValidBucket = !isLogarithmicYScaler || (isLogarithmicYScaler && value > 0);
                
                float[] bucket;

                if (sortingScaler == null)
                {
                    bucket = bucketCalculator.GetBucket(this, i, i, view.WindowRect, view.Viewport, parentFrame);
                }
                else
                {
                    bucket = bucketCalculator.GetBucket(this, i, sortingScaler.SortedIndices[i], view.WindowRect, view.Viewport, parentFrame);
                }

                frame.Buckets.Add(bucket);

                if (isValidBucket)
                {
                    PrepareMarker(view, frame, bucket, FramePreparer.CategoryBasedHost.ProvideCollisionDetector(), value, i, i);
                }
            }

            view.Markers.Count = frame.Markers.Count;            
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);

            if (ParentSeries == null)
            {
                return;
            }

            this.ParentSeries.RenderFragment(this, frame, view);
            CategoryMarkerManager.RasterizeMarkers(this, frame.Markers, view.Markers, UseLightweightMarkers);
        }

        #endregion

        #region Property Changed Override
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