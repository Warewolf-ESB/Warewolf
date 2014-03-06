using System;
using System.Collections.Generic;



using System.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart bubble series.
    /// </summary>
    [WidgetModule("ScatterChart")]
    public class BubbleSeries : ScatterBase
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new BubbleSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            BubbleView = (BubbleSeriesView)view;
        }
        internal BubbleSeriesView BubbleView { get; set; }

        #region constructor and initialisation
        /// <summary>
        /// Initializes a new instance of the BubbleSeries class. 
        /// </summary>
        public BubbleSeries()
        {
            DefaultStyleKey = typeof(BubbleSeries);
            PreviousFrame = new ScatterFrame();
            TransitionFrame = new ScatterFrame();
            CurrentFrame = new ScatterFrame();

           
        }
        #endregion

        private Rect OperatingWindowRect { get; set; }
        private Rect OperatingViewportRect { get; set; } 

        /// <summary>
        /// Gets the radius column from the datasource.
        /// </summary>
        internal IFastItemColumn<double> InternalRadiusColumn { get { return RadiusColumn; } }



        internal List<UIElement> LegendItems { get; set; }

        //internal Dictionary<object, OwnedPoint> AllMarkers { get; set; }

        #region RadiusMemberPath Dependency Property and RadiusColumn Property
        /// <summary>
        /// Gets or sets the radius mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string RadiusMemberPath
        {
            get
            {
                return (string)GetValue(RadiusMemberPathProperty);
            }
            set
            {
                SetValue(RadiusMemberPathProperty, value);
            }
        }

        internal const string RadiusMemberPathPropertyName = "RadiusMemberPath";

        /// <summary>
        /// Identifies the RadiusMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusMemberPathProperty = DependencyProperty.Register(RadiusMemberPathPropertyName, typeof(string), typeof(BubbleSeries), new PropertyMetadata(null, (sender, e) =>
        {
            (sender as BubbleSeries).RaisePropertyChanged(RadiusMemberPathPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The IFastItemColumn containing Radius values.
        /// </summary>
        protected internal IFastItemColumn<double> RadiusColumn
        {
            get { return _radiusColumn; }
            private set
            {
                if (_radiusColumn != value)
                {
                    IFastItemColumn<double> oldZColumn = RadiusColumn;

                    _radiusColumn = value;
                    RaisePropertyChanged(RadiusColumnPropertyName, oldZColumn, RadiusColumn);
                }
            }
        }
        private IFastItemColumn<double> _radiusColumn;
        internal const string RadiusColumnPropertyName = "RadiusColumn";
        #endregion

        #region RadiusScale Dependency Property

        internal const string RadiusScalePropertyName = "RadiusScale";

        /// <summary>
        /// Identifies the RadiusScale dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusScaleProperty = DependencyProperty.Register(RadiusScalePropertyName, typeof(SizeScale), typeof(BubbleSeries), new PropertyMetadata((sender, e) =>
        {
            (sender as BubbleSeries).RaisePropertyChanged(RadiusScalePropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the radius size scale for the bubbles.
        /// </summary>
        public SizeScale RadiusScale
        {
            get
            {
                return (SizeScale)GetValue(RadiusScaleProperty);
            }
            set
            {
                SetValue(RadiusScaleProperty, value);
            }
        }
        #endregion

        #region LabelMemberPath Dependency Property
        internal const string LabelMemberPathPropertyName = "LabelMemberPath";

        /// <summary>
        /// Identifies the LabelMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMemberPathProperty = DependencyProperty.Register(LabelMemberPathPropertyName, typeof(string), typeof(BubbleSeries), new PropertyMetadata((sender, e) =>
        {
            (sender as BubbleSeries).RaisePropertyChanged(LabelMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the Label mapping property for the current series object.
        /// </summary>
        public string LabelMemberPath
        {
            get
            {
                return (string)GetValue(LabelMemberPathProperty);
            }
            set
            {
                SetValue(LabelMemberPathProperty, value);
            }
        }
        #endregion

        #region LabelColumn internal Dependency Property

        internal const string LabelColumnPropertyName = "LabelColumn";
        private IFastItemColumn<object> _labelColumn;

        /// <summary>
        /// Gets or sets the data column used for labels.
        /// </summary>
        protected internal IFastItemColumn<object> LabelColumn
        {
            get { return _labelColumn; }
            private set
            {
                if (_labelColumn != value)
                {
                    IFastItemColumn<object> oldColumn = LabelColumn;

                    _labelColumn = value;
                    RaisePropertyChanged(LabelColumnPropertyName, oldColumn, LabelColumn);
                }
            }
        }
        #endregion

        #region FillMemberPath Dependency Property
        internal const string FillMemberPathPropertyName = "FillMemberPath";

        /// <summary>
        /// Identifies the FillMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty FillMemberPathProperty = DependencyProperty.Register(FillMemberPathPropertyName, typeof(string), typeof(BubbleSeries), new PropertyMetadata((sender, e) =>
        {
            (sender as BubbleSeries).RaisePropertyChanged(FillMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the fill mapping property for the current series object.
        /// </summary>
        public string FillMemberPath
        {
            get
            {
                return (string)GetValue(FillMemberPathProperty);
            }
            set
            {
                SetValue(FillMemberPathProperty, value);
            }
        }
        #endregion

        #region FillScale Dependency Property
        internal const string FillScalePropertyName = "FillScale";

        /// <summary>
        /// Identifies the FillScale dependency property.
        /// </summary>
        public static readonly DependencyProperty FillScaleProperty = DependencyProperty.Register(FillScalePropertyName, typeof(BrushScale), typeof(BubbleSeries), new PropertyMetadata((sender, e) =>
        {
            (sender as BubbleSeries).RaisePropertyChanged(FillScalePropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the brush scale for the marker brush.
        /// </summary>
        public BrushScale FillScale
        {
            get
            {
                return (BrushScale)GetValue(FillScaleProperty);
            }
            set
            {
                SetValue(FillScaleProperty, value);
            }
        }
        #endregion

        #region FillColumn internal Dependency Property

        internal const string FillColumnPropertyName = "FillColumn";
        private IFastItemColumn<double> _fillColumn;

        /// <summary>
        /// Represents the column of fill values to use.
        /// </summary>
        protected internal IFastItemColumn<double> FillColumn
        {
            get { return _fillColumn; }
            private set
            {
                if (_fillColumn != value)
                {
                    IFastItemColumn<double> oldZColumn = FillColumn;

                    _fillColumn = value;
                    RaisePropertyChanged(FillColumnPropertyName, oldZColumn, FillColumn);
                }
            }
        }
        #endregion

        internal override void CalculateCachedPoints(ScatterFrame frame, int count, Rect windowRect, Rect viewportRect)
        {
            //Cached points are used for animations.
            //currently only try for accurate animations if we arent winnowing.
            if (count <= MaximumMarkers)
            {
                frame.CachedPoints = new Dictionary<object, OwnedPoint>(count);
            }

            //AllMarkers = new Dictionary<object, OwnedPoint>(count);

            FastItemsSource itemsSource = FastItemsSource;
            double x;
            double y;

            ScalerParams px = new ScalerParams(windowRect, viewportRect, AxisInfoCache.XAxisIsInverted);

            ScalerParams py = new ScalerParams(windowRect, viewportRect, AxisInfoCache.YAxisIsInverted);

            for (int i = 0; i < count; i++)
            {
                x = XColumn[i];
                y = YColumn[i];
                Point point = new Point(
                            AxisInfoCache.XAxis.GetScaledValue(x, px),
                            AxisInfoCache.YAxis.GetScaledValue(y, py));

                if (!double.IsInfinity(point.X) &&
                    !double.IsInfinity(point.Y))
                {
                    object item = itemsSource[i];

                    //Cached points are used for animations.
                    //currently only try for accurate animations if we arent winnowing.
                    if (count <= MaximumMarkers)
                    {
                        if (!frame.CachedPoints.ContainsKey(item))
                        {
                            var columnValues = new Point(x, y);
                            
                            frame.CachedPoints.Add(item,
                                                   new OwnedPoint()
                                                   {
                                                       OwnerItem = item,
                                                       ColumnValues = columnValues,
                                                       Point = point
                                                   });
                        }
                    }

                    //if (!AllMarkers.ContainsKey(item))
                    //{
                    //    AllMarkers.Add(item,
                    //                    new OwnedPoint()
                    //                    {
                    //                        OwnerItem = item,
                    //                        ColumnValues = new Point(x, y),
                    //                        Point = point
                    //                    });
                    //}
                }
            }
        }

        /// <summary>
        /// Render the current bubble series.
        /// </summary>
        internal override void RenderFrame(ScatterFrame frame, ScatterBaseView view)
        {
            AxisInfoCache = new ScatterAxisInfoCache
            {
                XAxis = XAxis,
                YAxis = YAxis,
                XAxisIsInverted = XAxis.IsInverted,
                YAxisIsInverted = YAxis.IsInverted,




            };
            BubbleSeriesView bubbleView = view as BubbleSeriesView;
            bubbleView.MarkerManager.Render(frame.Markers, UseLightweightMarkers);

            Clipper clipper = new Clipper(double.NaN, view.Viewport.Bottom, double.NaN, view.Viewport.Top, false)
            {
                Target = view.TrendLineManager.TrendPolyline.Points
            };
            view.TrendLineManager.RasterizeTrendLine(frame.TrendLine, clipper);
        }

        /// <summary>
        /// Prepare the current bubble series for rendering.
        /// </summary>
        internal override void PrepareFrame(ScatterFrame frame, ScatterBaseView view)
        {
            frame.Markers.Clear();
            frame.TrendLine.Clear();
            BubbleSeriesView bubbleView = view as BubbleSeriesView;
            var bubbleManager = (BubbleMarkerManager)bubbleView.MarkerManager;
            bubbleManager.RadiusColumn = RadiusColumn;

            int count =
                Math.Min(XColumn != null ? XColumn.Count : 0, YColumn != null ? YColumn.Count : 0);

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);

            if (count < 1)
            {
                return;
            }

            AxisInfoCache = new ScatterAxisInfoCache
            {
                XAxis = XAxis,
                YAxis = YAxis,
                XAxisIsInverted = XAxis.IsInverted,
                YAxisIsInverted = YAxis.IsInverted,




                FastItemsSource = FastItemsSource
            };

            CalculateCachedPoints(frame, count, windowRect, viewportRect);





            bubbleView.MarkerManager.WinnowMarkers(frame.Markers, MaximumMarkers,
                                        windowRect, viewportRect, Resolution);

            bubbleView.CreateMarkerSizes();

            bubbleView.SetMarkerColors();

            DrawLegend();

            Clipper clipper = new Clipper(viewportRect, false) { Target = frame.TrendLine };
            double xmin = XAxis.GetUnscaledValue(viewportRect.Left, xParams);
            double xmax = XAxis.GetUnscaledValue(viewportRect.Right, xParams);

            bubbleView.TrendLineManager.PrepareLine(frame.TrendLine, TrendLineType, XColumn, YColumn,
                                         TrendLinePeriod, x => XAxis.GetScaledValue(x, xParams),
                                         y => YAxis.GetScaledValue(y, yParams),
                                         new TrendResolutionParams
                                         {
                                             Resolution = Resolution,
                                             Viewport = viewportRect,
                                             Window = windowRect
                                         }, clipper, xmin, xmax);
        }

        private void DrawLegend()
        {
            if (SeriesViewer == null) return;

            ItemLegend itemLegend = ActualLegend as ItemLegend;
            if (itemLegend != null)
            {
                itemLegend.ClearLegendItems(this);
                CreateLegendItems();
                itemLegend.RenderLegend(this);
            }

            var scaleLegend =  ActualLegend as ScaleLegend;
            if (scaleLegend != null)
            {
                scaleLegend.RestoreOriginalState();
                scaleLegend.InitializeLegend(this);
            }
        }

        

      

        /// <summary>
        /// Returns the a marker size for a given value based on a linear scale.
        /// </summary>
        internal static double GetLinearSize(double min, double max, double smallSize, double largeSize, double value)
        {
            //smaller than min size or invalid
            if (value <= min || double.IsNaN(value) || double.IsInfinity(value))
            {
                return smallSize;
            }

            if (value >= max)
            {
                return largeSize;
            }

            double result = smallSize + ((largeSize - smallSize) / (max - min)) * (value - min);
            return result;
        }

        /// <summary>
        /// Returns the marker size for a given value based on a logarithmic scale.
        /// </summary>
        internal static double GetLogarithmicSize(double min, double max, double smallSize, double largeSize, double logBase, double value)
        {
            double newValue = Math.Log(value, logBase);
            double newMin = Math.Log(min, logBase);
            double newMax = Math.Log(max, logBase);
            return GetLinearSize(newMin, newMax, smallSize, largeSize, newValue);
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
            bool isValid = base.ValidateSeries(viewportRect, windowRect, view);

            if (RadiusColumn == null
                || FastItemsSource == null
                || RadiusColumn.Count == 0
                || FastItemsSource.Count != RadiusColumn.Count)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Returns whether or not a property changed event should be raised for the given property name.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="action">DataSource event action</param>
        /// <returns>True if property changed event should be raised, otherwise false</returns>
        protected override bool MustReact(string propertyName, FastItemsSourceEventAction action)
        {
            if (action != FastItemsSourceEventAction.Change)
            {
                return true;
            }

            if (propertyName == null)
            {
                return true;
            }

            if (XMemberPath == propertyName ||
                YMemberPath == propertyName ||
                RadiusMemberPath == propertyName)
            {
                return true;
            }

            return false;
        }

        private void CreateLegendItems()
        {
            ItemLegend itemLegend = ActualLegend as ItemLegend;
            if (itemLegend == null || FastItemsSource == null) return;

            LegendItems = new List<UIElement>();

            for (int i = 0; i < FastItemsSource.Count; i++)
            {
                Brush brush = null;
                if (FillScale is ValueBrushScale && FillColumn != null)
                {
                    brush = ((ValueBrushScale)FillScale).GetBrushByIndex(i, FillColumn);
                }
                if (FillScale is CustomPaletteBrushScale)
                {
                    brush = ((CustomPaletteBrushScale)FillScale).GetBrush(i, FastItemsSource.Count);
                }

                ContentControl item = new ContentControl();

                item.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = itemLegend });
                item.SetBinding(VisibilityProperty, new Binding(LegendItemVisibilityPropertyName) { Source = this });


                string itemLabel = LabelColumn != null && LabelColumn[i] != null
                    ? LabelColumn[i].ToString()
                    : "";

                item.Content = new DataContext { Series = this, Item = FastItemsSource[i], ItemBrush = brush, ItemLabel = itemLabel };
                item.ContentTemplate = DiscreteLegendItemTemplate;

                LegendItems.Add(item);
            }
        }

        /// <summary>
        /// Called when the underlying data changes.
        /// </summary>
        /// <param name="action">DataSource event action</param>
        /// <param name="position">Item index</param>
        /// <param name="count">Item count</param>
        /// <param name="propertyName">Property name</param>
        protected override void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            base.DataUpdatedOverride(action, position, count, propertyName);
            DrawLegend();
        }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated on the series or owning chart. Gives the series a chance to respond to the various property updates.
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
                case FastItemsSourcePropertyName:
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).DeregisterColumn(RadiusColumn);
                        (oldValue as FastItemsSource).DeregisterColumn(FillColumn);
                        (oldValue as FastItemsSource).DeregisterColumn(LabelColumn);
                        RadiusColumn = null;
                        FillColumn = null;
                        LabelColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        RadiusColumn = RegisterDoubleColumn(RadiusMemberPath);
                        FillColumn = RegisterDoubleColumn(FillMemberPath);
                        LabelColumn = RegisterObjectColumn(LabelMemberPath);
                    }

                    RenderSeries(false);
                    DrawLegend();

                    break;

                case RadiusMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(RadiusColumn);
                        RadiusColumn = RegisterDoubleColumn(RadiusMemberPath);
                        DrawLegend();
                    }
                    break;

                case RadiusColumnPropertyName:
                    this.ScatterView.TrendLineManager.Reset();
                    RenderSeries(false);
                    this.NotifyThumbnailDataChanged();
                    break;

                case RadiusScalePropertyName:
                    this.RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case FillScalePropertyName:
                    RenderSeries(false);
                    DrawLegend();
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case FillColumnPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case FillMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(FillColumn);
                        FillColumn = RegisterDoubleColumn(FillMemberPath);
                        DrawLegend();
                    }
                    break;

                case LabelMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(LabelColumn);
                        LabelColumn = RegisterObjectColumn(LabelMemberPath);
                        DrawLegend();
                    }
                    break;

                case ActualLegendPropertyName:
                    //the legend is changing, so we must remove this series from the old legend.
                    ItemLegend legend = oldValue as ItemLegend;
                    if (legend != null)
                    {
                        legend.ClearLegendItems(this);
                    }

                    ScaleLegend scaleLegend = oldValue as ScaleLegend;
                    if (scaleLegend != null)
                    {
                        //this series doesn't use the scale legend anymore, so it's a good idea to reset the legend.
                        //But what if another series uses this same legend?
                        //In that case, we populate the legend with the right series.
                        bool restoreLegend = true;
                        Series series = null;

                        if (SeriesViewer != null)
                        {
                            foreach (var currentSeries in SeriesViewer.Series)
                            {
                                if (currentSeries.Legend == scaleLegend)
                                {
                                    series = currentSeries;
                                    restoreLegend = false;
                                }
                            }
                        }

                        if (restoreLegend)
                        {
                            scaleLegend.RestoreOriginalState();
                        }
                        else
                        {
                            scaleLegend.InitializeLegend(series);
                        }
                    }

                    DrawLegend();
                    break;
            }
        }

        internal void SizeBubbles(List<Marker> actualMarkers, List<double> actualRadiusColumn, Rect viewportRect, bool isThumbnail)
        {
            double min = RadiusColumn.Minimum;
            double max = RadiusColumn.Maximum;

            if (RadiusScale != null)
            {
                double smallSize = RadiusScale.MinimumValue;
                double largeSize = RadiusScale.MaximumValue;
                int logBase = RadiusScale.LogarithmBase;

                if (!RadiusScale.Series.Contains(this))
                {
                    RadiusScale.Series.Add(this);
                }

                //Size scale is set. Use it to scale the markers.
                if (RadiusScale.IsLogarithmic)
                {
                    //set the radius size in a logarithmic fashion based on small and large values.
                    for (int i = 0; i < actualRadiusColumn.Count; i++)
                    {
                        actualRadiusColumn[i] = BubbleSeries.GetLogarithmicSize(min, max, smallSize, largeSize, logBase, actualRadiusColumn[i]);
                    }
                }
                else
                {
                    //set the radius size in a linear fashion based on small and large values.
                    for (int i = 0; i < actualRadiusColumn.Count; i++)
                    {
                        actualRadiusColumn[i] = BubbleSeries.GetLinearSize(min, max, smallSize, largeSize, actualRadiusColumn[i]);
                    }
                }
            }
            if (isThumbnail)
            {
                var fullWidth = viewportRect.Width;
                if (!View.Viewport.IsEmpty)
                {
                    fullWidth = View.Viewport.Width;
                }
                else if (SeriesViewer != null && !SeriesViewer.ViewportRect.IsEmpty)
                {
                    fullWidth = SeriesViewer.ViewportRect.Width;
                }

                double scale = viewportRect.Width / fullWidth;
                for (int ii = 0; ii < actualRadiusColumn.Count; ii++)
                {
                    actualRadiusColumn[ii] = actualRadiusColumn[ii] * scale;
                }
            }

            for (int i = 0; i < actualMarkers.Count; i++)
            {
                Marker marker = actualMarkers[i];

                marker.ClearValue(Marker.MinWidthProperty);
                marker.ClearValue(Marker.MinHeightProperty);
                marker.Margin = new Thickness(0);

                marker.Width = Math.Max(0, actualRadiusColumn[i]);
                marker.Height = Math.Max(0, actualRadiusColumn[i]);
            }
        }

        internal void SetMarkerColors(List<Marker> actualMarkers)
        {
            if (this.FillScale != null && !this.FillScale.Series.Contains(this))
            {
                this.FillScale.Series.Add(this);
            }

            CustomPaletteBrushScale customPaletteColorAxis = FillScale as CustomPaletteBrushScale;
            ValueBrushScale valueBrushScale = FillScale as ValueBrushScale;
            bool clearMarkerBrushes =
                this.FillScale == null ||
                this.FillScale.Brushes == null ||
                this.FillScale.Brushes.Count == 0 ||
                (valueBrushScale != null && this.FillMemberPath == null);
            
            if (clearMarkerBrushes)
            {
                this.BubbleView.ClearMarkerBrushes();
                BubbleSeriesView bubbleThumbnailView = this.ThumbnailView as BubbleSeriesView;
                if (bubbleThumbnailView != null)
                {
                    bubbleThumbnailView.ClearMarkerBrushes();
                }
                return;
            }

            int markerCount = actualMarkers.Count;
            //List<OwnedPoint> allMarkers = AllMarkers.Values.ToList();

            for (int i = 0; i < markerCount; i++)
            {
                Marker marker = actualMarkers[i];
                DataContext markerContext = marker.Content as DataContext;

                if (markerContext != null)
                {
                    Brush brush = null;
                    int markerIndex = FastItemsSource.IndexOf(markerContext.Item);

                    if (customPaletteColorAxis != null)
                    {
                        //custom pallette axis is used for this bubble series
                        brush = customPaletteColorAxis.GetBrush(markerIndex, FastItemsSource.Count);
                    }
                    if (valueBrushScale != null)
                    {
                        //value scaling color axis is used for this bubble series.
                        brush = valueBrushScale.GetBrushByIndex(markerIndex, FillColumn);
                    }

                    markerContext.ItemBrush = brush;
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