using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Controls.Charts.Util;
using System.Windows.Shapes;
using System.Windows.Data;
using Infragistics.Controls.Charts;
using System.Windows.Controls;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    internal interface IHasCategoryModePreference
        : IHasCategoryAxis
    {
        CategoryMode PreferredCategoryMode(CategoryAxisBase axis);
    }

    internal interface IHasCategoryAxis
    {
        CategoryAxisBase CategoryAxis { get; }
    }

    /// <summary>
    /// Represents the base class for XamDataChart category series.
    /// </summary>
    [WidgetIgnoreDepends("NumericYAxis")]
    [WidgetIgnoreDepends("CategoryXAxis")]
    [WidgetIgnoreDepends("CategoryYAxis")]
    [WidgetIgnoreDepends("NumericXAxis")]
    public abstract class CategorySeries : MarkerSeries, IHasCategoryModePreference, ISupportsErrorBars
    {
        internal CategoryFramePreparer FramePreparer { get; set; }
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            var view = new CategorySeriesView(this);
            return view;
        }
        internal CategorySeriesView CategoryView { get; set; }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            CategoryView = (CategorySeriesView)view;
        }

        internal virtual CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode0;
        }

        CategoryMode IHasCategoryModePreference.PreferredCategoryMode(CategoryAxisBase axis)
        {
            return PreferredCategoryMode(axis);
        }

        CategoryAxisBase IHasCategoryAxis.CategoryAxis
        {
            get { return this.GetXAxis() as CategoryAxisBase; }
        }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a default CategorySeries object.
        /// </summary>
        internal protected CategorySeries()
        {
           

            DefaultStyleKey = typeof(CategorySeries);         
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
        }
        #endregion

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            if (wipeClean)
            {
                ClearMarkers(view);
            }
        }

        internal override SeriesComponentsForView GetSeriesComponentsForView()
        {
            var ret = base.GetSeriesComponentsForView();




            ret.ErrorBarSettings = ErrorBarSettings;

            return ret;
        }



        /// <summary>
        /// Invalidates the axes associated with the series.
        /// </summary>
        protected internal override void InvalidateAxes()
        {
            base.InvalidateAxes();
            Axis xAxis = this.GetXAxis();
            if (xAxis != null)
            {
                xAxis.RenderAxis(false);
            }
            Axis yAxis = this.GetYAxis();
            if (yAxis != null)
            {
                yAxis.RenderAxis(false);
            }
        }

        #region ErrorBarSettings

        /// <summary>
        /// The error bar settings for the series.
        /// </summary>
        public CategoryErrorBarSettings ErrorBarSettings
        {
            get { return (CategoryErrorBarSettings)GetValue(ErrorBarSettingsProperty); }
            set { SetValue(ErrorBarSettingsProperty, value); }
        }

        /// <summary>
        /// Identifies the ErrorBarSettings dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorBarSettingsProperty =
            DependencyProperty.Register(ErrorBarSettingsPropertyName, typeof(CategoryErrorBarSettings), typeof(CategorySeries),
            new PropertyMetadata(null, (sender, e) =>
                    {
                        (sender as CategorySeries).RaisePropertyChanged(ErrorBarSettingsPropertyName, e.OldValue, e.NewValue);
                    }));


        internal const string ErrorBarSettingsPropertyName = "ErrorBarSettings";
        #endregion

        /// <summary>
        /// Overridden in derived classes when they want to respond to the chart's window changing.
        /// </summary>
        /// <param name="oldWindowRect">The old window rectangle of the chart.</param>
        /// <param name="newWindowRect">The new window rectangle of the chart.</param>
        protected override void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            base.WindowRectChangedOverride(oldWindowRect, newWindowRect);
            this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
            RenderSeries(false);
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the viewport changing.
        /// </summary>
        /// <param name="oldViewportRect">The old viewport rectangle.</param>
        /// <param name="newViewportRect">The new viewport rectangle.</param>
        protected override void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
            base.ViewportRectChangedOverride(oldViewportRect, newViewportRect);
            this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
            RenderSeries(false);
        }

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

            Axis yAxis = this.GetYAxis();
            Axis xAxis = this.GetXAxis();

            switch (propertyName)
            {
                case Series.SeriesViewerPropertyName:
                    //dont stay registered with axes while not in chart
                    if (oldValue != null && newValue == null)
                    {
                        DeregisterForAxis(xAxis);
                        DeregisterForAxis(yAxis);
                    }
                    if (oldValue == null && newValue != null)
                    {
                        RegisterForAxis(xAxis);
                        RegisterForAxis(yAxis);
                    }

                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    break;

                case Series.SyncLinkPropertyName:
                    if (SyncLink != null && SeriesViewer != null)
                    {
                        this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    }

                    RenderSeries(false);
                    break;

                case Series.TransitionProgressPropertyName:
                    TransitionFrame.Interpolate(
                        (float)TransitionProgress,
                        PreviousFrame,
                        CurrentFrame);

                    if (ClearAndAbortIfInvalid(View))
                    {
                        return;
                    }

                    if (TransitionProgress == 1.0)
                    {
                        RenderFrame(this.CurrentFrame, this.CategoryView);
                    }
                    else
                    {
                        RenderFrame(this.TransitionFrame, this.CategoryView);
                    }

                    break;

                case CategorySeries.ErrorBarSettingsPropertyName:

                    if (this.ErrorBarSettings != null)
                    {
                        this.ErrorBarSettings.Series = this;
                    }

                    this.NotifyThumbnailAppearanceChanged();
                    break;

            }
        }

        #region Data and Buckets

        /// <summary>
        /// Gets the index of the item based on world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The index of the item for the specified coordinates.</returns>
        protected internal virtual int GetItemIndexSorted(Point world)
        {
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            if (windowRect.IsEmpty || viewportRect.IsEmpty)
            {
                return -1;
            }
            
            Axis xAxis = this.GetXAxis();
            ISortingAxis sorting = xAxis as ISortingAxis;

            ScalerParams p = new ScalerParams(windowRect, viewportRect, xAxis.IsInverted);

            double left = xAxis.GetUnscaledValue(viewportRect.Left, p);
            double right = xAxis.GetUnscaledValue(viewportRect.Right, p);
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
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            int index = 0;
            Axis xAxis = this.GetXAxis();
            if (xAxis is ISortingAxis)
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
        protected override int GetItemIndex(Point world)
        {
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;

            int rowIndex = -1;
            CategoryAxisBase xAxis = this.GetXAxis() as CategoryAxisBase;
            if (xAxis != null && !windowRect.IsEmpty && !viewportRect.IsEmpty)
            {
                double left = xAxis.GetUnscaledValue(viewportRect.Left, windowRect, viewportRect, xAxis.CategoryMode);
                double right = xAxis.GetUnscaledValue(viewportRect.Right, windowRect, viewportRect, xAxis.CategoryMode);

                double windowX = (world.X - windowRect.Left) / windowRect.Width;
                double bucket = left + (windowX * (right - left));
                if (xAxis.CategoryMode != CategoryMode.Mode0)
                {
                    bucket -= .5;
                }
                int bucketNumber = (int)Math.Round(bucket);

                //the row index is the bucket number at this point. It doesn't depend on the bucket size, because 
                //GetUnscaledValue uses total items count
                //rowIndex = bucketNumber * BucketSize;

                rowIndex = bucketNumber;
            }
            return rowIndex;
        }

        

        internal CategoryFrame PreviousFrame = new CategoryFrame(3);
        internal CategoryFrame TransitionFrame = new CategoryFrame(3);
        internal CategoryFrame CurrentFrame = new CategoryFrame(3);
        internal CategoryFrame ThumbnailFrame = new CategoryFrame(3);


        #endregion        

        /// <summary>
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal override bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = base.ValidateSeries(viewportRect, windowRect, view);
            isValid &= this.ValidateAxis(this.GetXAxis());
            isValid &= this.ValidateAxis(this.GetYAxis());
            var categoryView = (CategorySeriesView)view;

            if (!view.HasSurface()
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || FastItemsSource == null)
            {
                isValid = false;
            }
            if (!isValid)
            {
                categoryView.BucketCalculator.BucketSize = 0; // this doesn't belong here
            }
            return isValid;
        }
        private bool ValidateAxis(Axis axis)
        {
            if (axis == null || axis.SeriesViewer == null)
            {
                return false;
            }
            CategoryAxisBase categoryAxis = axis as CategoryAxisBase;
            if (categoryAxis != null)
            {
                return categoryAxis.ItemsSource != null;
            }
            else
            {
                NumericAxisBase numericAxis = axis as NumericAxisBase;
                if (numericAxis != null)
                {
                    return numericAxis.ActualMinimumValue != numericAxis.ActualMaximumValue;
                }
            }
            return true;
        }
        /// <summary>
        /// Renders the series visual.
        /// </summary>
        /// <param name="animate">True if the change should be animated.</param>
        protected internal override void RenderSeriesOverride(bool animate)
        {
            base.RenderSeriesOverride(animate);

            //bucketsize needs to be calculated already to determine if series
            //is valid.
            this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
            if (ClearAndAbortIfInvalid(View))
            {
                return;
            }

            if (ShouldAnimate(animate))
            {
                CategoryFrame previousFrame = PreviousFrame;

                if (AnimationActive())
                {
                    PreviousFrame = TransitionFrame;
                    TransitionFrame = previousFrame;
                }
                else
                {
                    PreviousFrame = CurrentFrame;
                    CurrentFrame = previousFrame;
                }

                //this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                PrepareFrame(CurrentFrame, this.CategoryView);
                StartAnimation();
            }
            else
            {
                //this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                PrepareFrame(CurrentFrame, this.CategoryView);
                RenderFrame(this.CurrentFrame, this.CategoryView);
            }
        }

        /// <summary>
        /// Gets the mode 2 index for the series.
        /// </summary>
        /// <returns>The mode 2 index.</returns>
        protected internal virtual int GetMode2Index()
        {
            int result = 0;
            Axis xAxis = this.GetXAxis();
            foreach (Series currentSeries in SeriesViewer.Series)
            {
                if (currentSeries == this)
                {
                    return result;
                }
                CategorySeries currentCategorySeries = currentSeries as CategorySeries;
                if (currentCategorySeries != null)
                {
                    CategoryAxisBase currentXAxis = currentCategorySeries.GetXAxis() as CategoryAxisBase;
                    if (currentXAxis == xAxis && currentCategorySeries.PreferredCategoryMode(currentXAxis) == CategoryMode.Mode2)
                    {
                        result++;
                    }
                }
            }
            //Debug.Assert(false, "CategorySeries.GetMode2Index failed to find series");
            return -1;
        }



        #region ISupportsErrorBars Implementation


        bool ISupportsErrorBars.ShouldDisplayErrorBars()
        {
            if (this.ErrorBarSettings == null ||
                this.ErrorBarSettings.Calculator == null ||
                !View.Ready())
            {
                return false;
            }

            return true;
        }

        bool ISupportsErrorBars.ShouldSyncErrorBarsWithMarkers()
        {
            return this.ShouldDisplayMarkers();
        }

        ErrorBarSettingsBase ISupportsErrorBars.ErrorBarSettings
        {
            get
            {
                return this.ErrorBarSettings;
            }
        }




#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        Axis ISupportsErrorBars.XAxis
        {
            get
            {
                return this.GetXAxis();
            }
        }

        Axis ISupportsErrorBars.YAxis
        {
            get
            {
                return this.GetYAxis();
            }
        }



        #endregion ISupportsErrorBars Implementation

        internal virtual void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            
        }

        internal virtual void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
            
        }

        internal void ClearMarkers(SeriesView view)
        {
            var catView = (CategorySeriesView)view;
            catView.Markers.Count = 0;
        }

        /// <summary>
        /// Renders the thumbnail for the OPD pane.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="surface">The render target.</param>
        protected internal override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            base.RenderThumbnail(viewportRect, surface);

            if (!ThumbnailDirty)
            {
                View.PrepSurface(surface);
                return;
            }

            CategorySeriesView categorySeriesView = this.ThumbnailView as CategorySeriesView;
            categorySeriesView.BucketCalculator.CalculateBuckets(this.Resolution);
            
            View.PrepSurface(surface);
            if (ClearAndAbortIfInvalid(ThumbnailView))
            {
                return;
            }

            RenderThumbnailFrame();

            ThumbnailDirty = false;
        }
        internal virtual void RenderThumbnailFrame()
        {
            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            CategorySeriesView thumbnailView = this.ThumbnailView as CategorySeriesView;
            //thumbnailView.BucketCalculator.CalculateBuckets(this.Resolution);
            this.PrepareFrame(this.ThumbnailFrame, thumbnailView);
            this.RenderFrame(this.ThumbnailFrame, thumbnailView);
        }
        internal abstract Axis GetXAxis();
        internal abstract Axis GetYAxis();
        internal abstract bool UpdateNumericAxisRange();

        internal virtual CategoryFramePreparer GetFramePreparer(CategorySeriesView view)
        {


            CategorySeriesView categoryView = view as CategorySeriesView;
            if (categoryView != null && categoryView == this.ThumbnailView)
            {
                return new CategoryFramePreparer(
                            this as IIsCategoryBased,
                            categoryView as ISupportsMarkers,
                            this.SeriesViewer.View.OverviewPlusDetailViewportHost,
                            this,
                            categoryView.BucketCalculator);
            }
            else


            {
                return this.FramePreparer;
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