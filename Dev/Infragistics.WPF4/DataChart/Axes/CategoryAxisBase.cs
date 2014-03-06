using System;
using System.Collections;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using Infragistics.Controls.Charts.Util;
using System.Collections.Generic;

using System.Windows.Data;
using System.Reflection;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for all XamDataChart category-based axes.
    /// </summary>
    [WidgetIgnoreDepends("XamDataChart")]
    public abstract class CategoryAxisBase : Axis, ICategoryScaler
    {
        internal override AxisView CreateView()
        {
            return new CategoryAxisBaseView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);
            CategoryView = (CategoryAxisBaseView)view;
        }
        internal CategoryAxisBaseView CategoryView { get; set; }

        /// <summary>
        /// CategoryAxisBase constructor.
        /// </summary>
        protected CategoryAxisBase()
            : base()
        {




        }

        internal void OnDetached()
        {
            if (FastItemsSource != null
                && FastItemsSourceProvider != null
                && ItemsSource != null)
            {
                FastItemsSource = FastItemsSourceProvider
                    .ReleaseFastItemsSource(ItemsSource);
            }
        }

        internal void OnAttached()
        {
            if (FastItemsSource == null
                && FastItemsSourceProvider != null
                && ItemsSource != null)
            {
                FastItemsSource = FastItemsSourceProvider
                    .GetFastItemsSource(ItemsSource);
            }
        }

        #region FastItemsSource and FastItemsSource Event Handler
        internal const string FastItemsSourcePropertyName = "FastItemsSource";

        /// <summary>
        /// Gets the FastItemsSource for the current CategoryAxis object
        /// </summary>
        internal FastItemsSource FastItemsSource
        {
            get
            {
                return (FastItemsSource)GetValue(FastItemsSourceProperty);
            }
            set
            {
                SetValue(FastItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// Link to the fast items source.
        /// </summary>
        protected FastItemsSource _fastItemsSource;

        /// <summary>
        /// Identifies the FastItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty FastItemsSourceProperty = DependencyProperty.Register(FastItemsSourcePropertyName, typeof(FastItemsSource), typeof(CategoryAxisBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as CategoryAxisBase).RaisePropertyChanged(FastItemsSourcePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion

        #region ItemsSource
        internal const string ItemsSourcePropertyName = "ItemsSource";

        /// <summary>
        /// Gets or sets the ItemsSource property for the current axis.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public 



            IEnumerable 

            ItemsSource
        {
            get
            {
                return (



            IEnumerable 

                )GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(ItemsSourcePropertyName, typeof(System.Collections.IEnumerable), typeof(CategoryAxisBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                CategoryAxisBase axis = sender as CategoryAxisBase;

                if (axis.FastItemsSourceProvider != null)
                {
                    axis.FastItemsSourceProvider
                        .ReleaseFastItemsSource(



                        (IEnumerable) 

                        e.OldValue);
                }

                (sender as CategoryAxisBase).RaisePropertyChanged(ItemsSourcePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion


#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)


#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)



        #region ItemsCount read-only property
        internal const string ItemsCountPropertyName = "ItemsCount";

        /// <summary>
        /// Gets the number of items in the current category axis items source.
        /// </summary>
        public int ItemsCount
        {
            get
            {
                return _fastItemsSource == null ? 0 : _fastItemsSource.Count;
            }
        }
        #endregion

        #region CategoryMode Property
        internal CategoryMode CategoryMode
        {
            get { return _categoryMode; }
            set
            {
                if (_categoryMode != value)
                {
                    CategoryMode oldValue = _categoryMode;

                    _categoryMode = value;
                    RaisePropertyChanged(CategoryModePropertyName, oldValue, value);
                }
            }
        }
        private CategoryMode _categoryMode;
        internal const string CategoryModePropertyName = "CategoryMode";
        #endregion

        #region Gap Property
        /// <summary>
        /// Gets or sets the amount of space between adjacent categories for the current axis object.
        /// <para>This is a dependency property.</para>
        /// </summary>
        /// <remarks>
        /// The gap is silently clamped to the range [0, inf] when used.
        /// </remarks>
        public double Gap
        {
            get
            {
                return (double)GetValue(GapProperty);
            }
            set
            {
                SetValue(GapProperty, value);
            }
        }

        internal const string GapPropertyName = "Gap";

        /// <summary>
        /// Identifies the Gap dependency property.
        /// </summary>
        public static readonly DependencyProperty GapProperty = DependencyProperty.Register(GapPropertyName, typeof(double), typeof(CategoryAxisBase),
            new PropertyMetadata(0.2, (sender, e) =>
            {
                (sender as CategoryAxisBase).RaisePropertyChanged(GapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region Overlap Property
        /// <summary>
        /// Gets or sets the amount of overlap between adjacent categories for the current axis object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The overlap is silently clamped to the range [-1, 1] when used.
        /// </remarks>
        public double Overlap
        {
            get
            {
                return (double)GetValue(OverlapProperty);
            }
            set
            {
                SetValue(OverlapProperty, value);
            }
        }

        internal const string OverlapPropertyName = "Overlap";

        /// <summary>
        /// Identifies the Overlap dependency property.
        /// </summary>
        public static readonly DependencyProperty OverlapProperty = DependencyProperty.Register(OverlapPropertyName, typeof(double), typeof(CategoryAxisBase),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as CategoryAxisBase).RaisePropertyChanged(OverlapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region GroupCount Property
        /// <summary>
        /// Gets the number of category groups in the current Chart object.
        /// </summary>
        internal int Mode2GroupCount
        {
            get { return mode2GroupCount; }
            set
            {
                if (value != mode2GroupCount)
                {
                    int oldGroupCount = mode2GroupCount;

                    mode2GroupCount = value;
                    RaisePropertyChanged(GroupCountPropertyName, oldGroupCount, mode2GroupCount);
                }
            }
        }
        internal const string GroupCountPropertyName = "GroupCount";
        private int mode2GroupCount = 0;
        #endregion


        /// <summary>
        /// Gets the unscaled axis value from a scaled viewport value.
        /// </summary>
        /// <param name="scaledValue">The scaled viewport value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>The unscaled axis value.</returns>
        public override double GetUnscaledValue(double scaledValue, ScalerParams p) 
        {
            return double.NaN; 
        }
        internal virtual double GetUnscaledValue(double scaledValue, Rect windowRect, Rect viewportRect, CategoryMode categoryMode) 
        {
            return double.NaN; 
        }
        internal virtual double GetCategorySize(Rect windowRect, Rect viewportRect)
        {
            return double.NaN; 
        }
        internal virtual double GetGroupSize(Rect windowRect, Rect viewportRect)
        {
            return double.NaN; 
        }
        internal virtual double GetGroupCenter(int index, Rect windowRect, Rect viewportRect)
        {
            return double.NaN; 
        }

        /// <summary>
        /// Unscales a value from screen space into axis space.
        /// </summary>
        /// <param name="unscaledValue">The scaled value in screen coordinates to unscale into axis space.</param>
        /// <returns>The unscaled value in axis space.</returns>
        public double UnscaleValue(double unscaledValue)
        {
            Rect windowRect = SeriesViewer.WindowRect;
            Rect viewportRect = ViewportRect;
            ScalerParams sParams = new ScalerParams(windowRect, viewportRect, IsInverted);
            return GetUnscaledValue(unscaledValue, sParams);
        }

        private IEnumerable<Series> RelatedSeries()
        {





            foreach (var currentSeries in Series)
            {

                yield return currentSeries;
            }
            if (SeriesViewer != null &&
                SeriesViewer.IsSyncReady &&
                ShouldShareMode(SeriesViewer))
            {
                foreach (var chart in SeriesViewer.SynchronizedCharts())
                {
                    if (chart != SeriesViewer)
                    {
                        foreach (var currentSeries in chart.Series)
                        {
                            yield return currentSeries;
                        }
                    }
                }
            }
        }

        private bool HasSeries(Series series)
        {
            return



                Series.Contains(series);

        }

        internal virtual bool ShouldShareMode(SeriesViewer chart)
        {
            return false;
        }

        private IEnumerable<CategoryAxisBase> RelatedAxes()
        {
            XamDataChart dataChart = SeriesViewer as XamDataChart;
            if (dataChart != null &&
                dataChart.IsSyncReady &&
                ShouldShareMode(dataChart))
            {
                foreach (var chart in dataChart.SynchronizedCharts())
                {
                    if (chart != SeriesViewer)
                    {
                        foreach (var axis in dataChart.Axes)
                        {
                            if (axis is CategoryAxisBase)
                            {
                                yield return axis as CategoryAxisBase;
                            }
                        }
                    }
                }
            }
        }

        private bool _spreading = false;

        private void Spread()
        {
            if (_spreading)
            {
                return;
            }

            try
            {
                _spreading = true;
                CategoryMode categoryMode = CategoryMode.Mode0;
                int mode2GroupCount = 0;
                bool mode2Present = false;

                foreach (Series currentSeries in RelatedSeries())
                {
                    IHasCategoryModePreference categorySeries = currentSeries as IHasCategoryModePreference;
                    if (categorySeries != null)
                    {
                        CategoryMode seriesMode = categorySeries.PreferredCategoryMode(this);

                        if (seriesMode == CategoryMode.Mode2)
                        {
                            categoryMode = CategoryMode.Mode2;
                            mode2Present = true;

                            if (HasSeries(currentSeries))
                            {
                                mode2GroupCount++;
                            }
                        }

                        if (seriesMode == CategoryMode.Mode1 && !mode2Present)
                        // this axis should use mode 2 if there are any mode 2 series present.
                        {
                            categoryMode = CategoryMode.Mode1;
                        }
                    }
                }

                foreach (var axis in RelatedAxes())
                {
                    axis.Spread();
                }

                // these shouldn't generate individual events

                CategoryMode = categoryMode;
                Mode2GroupCount = mode2GroupCount;

                // if anything changed, generate a range changed event
            }
            finally
            {
                _spreading = false;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the axis. Gives the axis a chance to respond to the various property updates.
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
                case CategoryAxisBase.FastItemsSourceProviderPropertyName:
                    if ((oldValue as IFastItemsSourceProvider) != null)
                    {
                        FastItemsSource =
                            (oldValue as IFastItemsSourceProvider).ReleaseFastItemsSource(ItemsSource);
                    }

                    if ((newValue as IFastItemsSourceProvider) != null)
                    {
                        FastItemsSource =
                            (newValue as IFastItemsSourceProvider).GetFastItemsSource(ItemsSource);
                    }

                    Spread();
                    break;
                case CategoryAxisBase.ItemsSourcePropertyName:
                    if (FastItemsSourceProvider != null)
                    {
                        FastItemsSource =
                            FastItemsSourceProvider.GetFastItemsSource(
                            ItemsSource);
                    }
                    break;

                case CategoryAxisBase.FastItemsSourcePropertyName:
                    FastItemsSource oldFastItemsSource = oldValue as FastItemsSource;
                    CacheFastItemsSource();
                    MustInvalidateLabels = true;
                    if (oldFastItemsSource != null)
                    {
                        oldFastItemsSource.Event -= HandleFastItemsSourceEvent;
                    }
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.Event += HandleFastItemsSourceEvent;

                        //shouldn't try rendering axes or seres when the itemsource becomes null.
                        RenderAxis(false);

                        foreach (Series currentSeries in DirectSeries())
                        {
                            currentSeries.RenderSeries(false);
                            if (currentSeries.SeriesViewer != null)
                            {
                                currentSeries.NotifyThumbnailAppearanceChanged();
                            }
                        }
                    }
                    else
                    {
                        //some light clean up is in order. 
                        //We may not want axes and series rendering themselves for a non-existent source, 
                        //but we also want to clean up the axis an series geometries.

                        ClearAllMarks();

                        foreach (Series currentSeries in DirectSeries())
                        {
                            currentSeries.ClearRendering(true, currentSeries.View);
                            if (currentSeries.SeriesViewer != null)
                            {
                                currentSeries.NotifyThumbnailAppearanceChanged();
                            }
                        }
                    }
                    break;

                case CategoryAxisBase.ItemsCountPropertyName:
                    RaiseRangeChanged(new AxisRangeChangedEventArgs(0, 0, (int)(oldValue) - 1, (int)(newValue) - 1));
                    RenderAxis(false);
                    break;

                case CategoryAxisBase.CategoryModePropertyName:
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    RenderCrossingAxis();

                    foreach (Series currentSeries in DirectSeries())
                    {
                        IHasCategoryModePreference currentCategorySeries = currentSeries as IHasCategoryModePreference;
                        if (currentCategorySeries != null
                            && currentCategorySeries.PreferredCategoryMode(this) == (CategoryMode)oldValue)
                        {
                            currentSeries.RenderSeries(false);
                        }
                    }
                    break;
                case CategoryAxisBase.OverlapPropertyName:
                case CategoryAxisBase.GapPropertyName:
                    MustInvalidateLabels = true;

                    foreach (Series currentSeries in DirectSeries())
                    {
                        currentSeries.ThumbnailDirty = true;
                        IHasCategoryModePreference currentCategorySeries = currentSeries as IHasCategoryModePreference;
                        // re-render all mode 2 series on this axis
                        if (currentCategorySeries != null
                            && currentCategorySeries.PreferredCategoryMode(this) == CategoryMode.Mode2)
                        {
                            currentSeries.RenderSeries(false);
                        }
                    }
                    RenderAxis(false);
                    if (this.SeriesViewer != null)
                    {
                        this.SeriesViewer.NotifyThumbnailAppearanceChanged();
                    }
                    break;

                case CategoryAxisBase.CrossingValuePropertyName:
                case CategoryAxisBase.CrossingAxisPropertyName:
                    MustInvalidateLabels = true;
                    RenderAxis(true);
                    break;





            }
        }

        /// <summary>
        /// Updates the axis based on a change in the data source.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The FastItemsSource event arguments.</param>
        protected internal void HandleFastItemsSourceEvent(object sender, FastItemsSourceEventArgs e)
        {
            switch (e.Action)
            {
                case FastItemsSourceEventAction.Change:
                case FastItemsSourceEventAction.Remove:
                case FastItemsSourceEventAction.Insert:
                case FastItemsSourceEventAction.Replace:
                case FastItemsSourceEventAction.Reset:
                    this.RenderAxis(false);
                    break;
            }
        }

        private void UpdateCategoryMode()
        {
            bool mode1Present = false, mode2Present = false;





            foreach (Series currentSeries in Series)
            {

                IHasCategoryModePreference currentCategorySeries = currentSeries as IHasCategoryModePreference;
                if (currentCategorySeries == null)
                {
                    continue;
                }
                CategoryMode currentMode = currentCategorySeries.PreferredCategoryMode(this);
                mode1Present |= currentMode == CategoryMode.Mode1;
                mode2Present |= currentMode == CategoryMode.Mode2;
            }
            CategoryMode =
                mode2Present ? CategoryMode.Mode2
                : mode1Present ? CategoryMode.Mode1
                : CategoryMode.Mode0;
        }

        /// <summary>
        /// Registers a series that uses an axis with the axis.
        /// </summary>
        /// <param name="series">The series to register.</param>
        /// <returns>If the registration was a success.</returns>
        public override bool RegisterSeries(Series series)
        {
            bool success = base.RegisterSeries(series);
            if (success)
            {
                this.Spread();
                IHasCategoryModePreference registeredCategorySeries = series as IHasCategoryModePreference;
                if (registeredCategorySeries != null
                    && registeredCategorySeries.PreferredCategoryMode(this) == CategoryMode.Mode2)
                {
                    foreach (Series currentSeries in DirectSeries())
                    {
                        IHasCategoryModePreference currentCategorySeries = currentSeries as IHasCategoryModePreference;
                        if (currentCategorySeries != null &&
                            currentCategorySeries != registeredCategorySeries &&
                            currentCategorySeries.PreferredCategoryMode(this) == CategoryMode.Mode2)
                        {
                            currentSeries.RenderSeries(false);
                        }
                    }
                }

                RenderAxis(false);
                UpdateRange();
            }
            return success;
        }

        /// <summary>
        /// Deregisters a series that uses an axis from the axis.
        /// </summary>
        /// <param name="series">The series to deregister.</param>
        /// <returns>If the deregistration was a success.</returns>
        public override bool DeregisterSeries(Series series)
        {
            bool success = base.DeregisterSeries(series);
            if (success)
            {
                this.Spread();
                IHasCategoryModePreference deregisteredCategorySeries = series as IHasCategoryModePreference;
                if (deregisteredCategorySeries != null
                    && deregisteredCategorySeries.PreferredCategoryMode(this) != CategoryMode.Mode0)
                {
                    foreach (Series currentSeries in DirectSeries())
                    {
                        IHasCategoryModePreference currentCategorySeries = currentSeries as IHasCategoryModePreference;
                        if (currentCategorySeries != null)
                        {
                            currentSeries.RenderSeries(false);
                        }
                    }
                }

                RenderAxis(false);
            }
            return success;
        }

        /// <summary>
        /// Updates the crossing axis. Useful when category mode changes.
        /// </summary>
        private void RenderCrossingAxis()
        {
            Axis crossingAxis = null;

            foreach (Series currentSeries in DirectSeries())
            {
                CategorySeries categorySeries = currentSeries as CategorySeries;
                if (categorySeries != null)
                {
                    Axis yAxis = categorySeries.GetYAxis();
                
                    if (yAxis != null && yAxis.CrossingAxis == this)
                    {
                        crossingAxis = yAxis;
                    }
                }
            }

            if (crossingAxis != null)
            {
                crossingAxis.RenderAxis();
            }
        }

        CategoryMode ICategoryScaler.CategoryMode
        {
            get { return this.CategoryMode; }
        }

        double ICategoryScaler.GetCategorySize(Rect windowRect, Rect viewportRect)
        {
            return this.GetCategorySize(windowRect, viewportRect);
        }

        double ICategoryScaler.GetGroupCenter(int index, Rect windowRect, Rect viewportRect)
        {
            return this.GetGroupCenter(index, windowRect, viewportRect);
        }

        bool IScaler.IsInverted
        {
            get { return (bool)GetValue(IsInvertedProperty); }
        }

        double IScaler.GetUnscaledValue(double scaledValue, ScalerParams p)
        {
            return this.GetUnscaledValue(scaledValue, p);
        }

        double IScaler.GetScaledValue(double unscaledValue, ScalerParams p)
        {
            return this.GetScaledValue(unscaledValue, p);
        }

        void IScaler.GetScaledValueList(IList<double> unscaledValues, ScalerParams p)
        {
            this.GetScaledValueList(unscaledValues, p);
        }

        void IScaler.GetUnscaledValueList(IList<double> scaledValues, ScalerParams p)
        {
            this.GetUnscaledValueList(scaledValues, p);
        }

        internal void CacheFastItemsSource()
        {
            _fastItemsSource = FastItemsSource;
        }

        internal void RenderLabels()
        {
            AxisLabelSettings labelSettings = LabelSettings;
            if (labelSettings == null)
            {
                labelSettings = new AxisLabelSettings();
            }

            if (labelSettings.Visibility == System.Windows.Visibility.Collapsed)
            {
                TextBlocks.Count = 0;
            }
            else
            {
                int textBlockCount = 0;
                textBlockCount = CategoryView.AddLabels(LabelDataContext);
                TextBlocks.Count = textBlockCount;
            }
        }



#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)

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