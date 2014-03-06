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
using System.ComponentModel;
using System.Collections.Specialized;



using System.Linq;

using System.Collections.Generic;
using Infragistics.Controls.Charts.AutomationPeers;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;










namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a chart area containing axes, series, an optional legend and other hosted content.
    /// </summary>

    [TemplatePart(Name = "Overlay", Type = typeof(Rectangle))]
    [TemplateVisualState(Name = XamDataChart.IdleVisualStateName, GroupName = "MouseStates")]
    [TemplateVisualState(Name = XamDataChart.DraggingVisualStateName, GroupName = "MouseStates")]
    [TemplateVisualState(Name = XamDataChart.PanningVisualStateName, GroupName = "MouseStates")]
    [TemplateVisualState(Name = XamDataChart.InkingVisualStateName, GroupName = "MouseStates")]
    [TemplateVisualState(Name = XamDataChart.ErasingVisualStateName, GroupName = "MouseStates")]

    [StyleTypedProperty(Property = "ZoombarStyle", StyleTargetType = typeof(XamZoombar))]

    [StyleTypedProperty(Property = "PreviewPathStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "CrosshairLineStyle", StyleTargetType = typeof(Line))]
    [StyleTypedProperty(Property = "ToolTipStyle", StyleTargetType = typeof(ContentControl))]


	
    


    [MainWidget("DataChart")]
    [WidgetModule("ChartCore")]
    [WidgetIgnoreDepends("FragmentBase")]
    [WidgetIgnoreDepends("SplineFragmentBase")]
    [WidgetIgnoreDepends("AnchoredCategorySeries")]
    [WidgetIgnoreDepends("ValueOverlay")]
    [WidgetIgnoreDepends("RadialBase")]
    [WidgetIgnoreDepends("PolarBase")]
    [WidgetIgnoreDepends("NumericRadiusAxis")]
    [WidgetIgnoreDepends("NumericAngleAxis")]
    [WidgetIgnoreDepends("CategoryAngleAxis")]
    [WidgetIgnoreDepends("CategoryAxisBase")]
    public class XamDataChart : SeriesViewer
    {





        internal static readonly double DecimalMinimumValueAsDouble;
        internal static readonly double DecimalMaximumValueAsDouble;

        static XamDataChart()
        {




            XamDataChart.DecimalMinimumValueAsDouble = Convert.ToDouble(decimal.MinValue);
            XamDataChart.DecimalMaximumValueAsDouble = Convert.ToDouble(decimal.MaxValue);

        }


        internal static int FindSeriesIndex(Series series)
        {
            if (series.SeriesViewer == null) return -1;

            var allSeries = XamDataChartView.GetAllSeries(series);

            if (allSeries.Count() == 0) return -1;

            int index = allSeries.Max((s) => s.Index) + 1;

            //int index = series.SeriesViewer.Series.IndexOf(series);


            if (series is FragmentBase || series is SplineFragmentBase)
            {
                StackedSeriesBase parentSeries = series is FragmentBase
                            ? (series as FragmentBase).ParentSeries
                            : (series as SplineFragmentBase).ParentSeries;

                AnchoredCategorySeries anchoredSeries = series as AnchoredCategorySeries;

                if (parentSeries.Index == -1
                    || parentSeries.StackedSeriesManager == null
                    || anchoredSeries == null
                    || parentSeries.StackedSeriesManager.SeriesVisual.IndexOf(anchoredSeries) == -1)
                {
                    return -1;
                }

                index = parentSeries.Index + parentSeries.StackedSeriesManager.SeriesVisual.IndexOf(anchoredSeries);
            }

            return index;
        }

        #region Constructor and Template Parts
   
        /// <summary>
        /// Initializes a default, empty XamDataChart object.
        /// </summary>
        public XamDataChart()
        {

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // [DN Nov 4 2011 : 93211] it is necessary to reference code from the common assembly so that the reference will be added at design time.  I feel so dirty right now.
                Infragistics.CommandBase cmd = new CommandBase();
            }

            // TouchUtil.SetManipulationMode(this, ManipulationModes.Scale|ManipulationModes.Translate);
            DefaultStyleKey = typeof(XamDataChart);


            Axes.CollectionChanged += new NotifyCollectionChangedEventHandler(Axes_CollectionChanged);
            Axes.CollectionResetting += new EventHandler<EventArgs>(Axes_CollectionResetting);
            







            //LayoutUpdated += (o, e) => { Chart = XamDataChart.FindChart(this); };

        








			Infragistics.Windows.Utilities.ValidateLicense(typeof(XamDataChart), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }

     



#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)



        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

        }




     

        #endregion Constructor and Template Parts

        /// <summary>
        /// Called when a property value has been updated.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="propertyName">The name of the updated property.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
       protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            switch (propertyName)
            {
                case XamDataChart.GridModePropertyName:
                    ((XamDataChartView)this.View).UpdateGridMode(newValue);

                    if (oldValue != newValue &&
                        ((GridMode)oldValue == GridMode.None ||
                        (GridMode)newValue == GridMode.None))

                    {
                        foreach (var axis in Axes)
                        {
                            axis.RenderAxis();
                        }
                    }
                    break;
            }
        }
        #region IsSquare Property

        /// <summary>
        /// Gets or sets whether to use a square aspect ratio for the chart. This is locked to true for polar and radial charts.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool IsSquare
        {
            get
            {
                return (bool)GetValue(IsSquareProperty);
            }
            set
            {
                SetValue(IsSquareProperty, value);
            }
        }

        internal const string IsSquarePropertyName = "IsSquare";

        /// <summary>
        /// Identifies the IsSquare dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSquareProperty = DependencyProperty.Register(IsSquarePropertyName, typeof(bool), typeof(XamDataChart),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as XamDataChart).RaisePropertyChanged(IsSquarePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion IsSquare Property

 

        #region GridMode Dependency Property

        /// <summary>
        /// Gets or sets the GridMode property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultString("behindSeries")]
        public GridMode GridMode
        {
            get
            {
                return (GridMode)GetValue(GridModeProperty);
            }
            set
            {
                SetValue(GridModeProperty, value);
            }
        }

        internal const string GridModePropertyName = "GridMode";

        /// <summary>
        /// Identifies the GridMode dependency property.
        /// </summary>
        public static readonly DependencyProperty GridModeProperty = DependencyProperty.Register(GridModePropertyName, typeof(GridMode), typeof(XamDataChart),
            new PropertyMetadata(GridMode.BehindSeries, (sender, e) =>
            {
                (sender as XamDataChart).RaisePropertyChanged(GridModePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion GridMode Dependency Property





#region Infragistics Source Cleanup (Region)
















































#endregion // Infragistics Source Cleanup (Region)




        





#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)

 





#region Infragistics Source Cleanup (Region)


















































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


        #region Appearance

  



        #region Brushes Dependency Property

        /// <summary>
        /// Gets or sets the Brushes property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The brushes property defines the palette from which automatically assigned series brushes are selected.
        /// </remarks>
        public BrushCollection Brushes
        {
            get
            {
                return (BrushCollection)GetValue(BrushesProperty);
            }
            set
            {
                SetValue(BrushesProperty, value);
            }
        }

        internal const string BrushesPropertyName = "Brushes";

        /// <summary>
        /// Identifies the Brushes dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushesProperty = DependencyProperty.Register(BrushesPropertyName, typeof(BrushCollection), typeof(XamDataChart),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as XamDataChart).RaisePropertyChanged(BrushesPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion Brushes Dependency Property

        #region MarkerBrushes Dependency Property

        /// <summary>
        /// Gets or sets the MarkerBrushes property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The brushes property defines the palette from which automatically assigned series brushes are selected.
        /// </remarks>
        public BrushCollection MarkerBrushes
        {
            get
            {
                return (BrushCollection)GetValue(MarkerBrushesProperty);
            }
            set
            {
                SetValue(MarkerBrushesProperty, value);
            }
        }

        internal const string MarkerBrushesPropertyName = "MarkerBrushes";

        /// <summary>
        /// Identifies the MarkerBrushes dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerBrushesProperty = DependencyProperty.Register(MarkerBrushesPropertyName, typeof(BrushCollection), typeof(XamDataChart),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as XamDataChart).RaisePropertyChanged(MarkerBrushesPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MarkerBrushes Dependency Property

        #region Outlines Dependency Property

        /// <summary>
        /// Gets or sets the Outlines property.
        /// </summary>
        /// <remarks>
        /// The outlines property defines the palette from which automatically assigned series outlines are selected.
        /// </remarks>
        public BrushCollection Outlines
        {
            get
            {
                return (BrushCollection)GetValue(OutlinesProperty);
            }
            set
            {
                SetValue(OutlinesProperty, value);
            }
        }

        internal const string OutlinesPropertyName = "Outlines";

        /// <summary>
        /// Identifies the Outlines dependency property.
        /// </summary>
        public static readonly DependencyProperty OutlinesProperty = DependencyProperty.Register(OutlinesPropertyName, typeof(BrushCollection), typeof(XamDataChart),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as XamDataChart).RaisePropertyChanged(OutlinesPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion Outlines Dependency Property

        #region MarkerOutlines Dependency Property

        /// <summary>
        /// Gets or sets the MarkerOutlines property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The brushes property defines the palette from which automatically assigned series brushes are selected.
        /// </remarks>
        public BrushCollection MarkerOutlines
        {
            get
            {
                return (BrushCollection)GetValue(MarkerOutlinesProperty);
            }
            set
            {
                SetValue(MarkerOutlinesProperty, value);
            }
        }

        internal const string MarkerOutlinesPropertyName = "MarkerOutlines";

        /// <summary>
        /// Identifies the MarkerOutlines dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerOutlinesProperty = DependencyProperty.Register(MarkerOutlinesPropertyName, typeof(BrushCollection), typeof(XamDataChart),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as XamDataChart).RaisePropertyChanged(MarkerOutlinesPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MarkerOutlines Dependency Property

   

   

    

        #endregion Appearance

   

        #region Axes

        /// <summary>
        /// Gets the current Chart object's child DataChartAxes.
        /// </summary>
        public AxisCollection Axes
        {
            get { return axes; }
        }

        private AxisCollection axes = new AxisCollection();

        private void Axes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Axis axis in e.OldItems)
                {
                    if (axis != null)
                    {
                        axis.FastItemsSourceProvider = null;
                        axis.SeriesViewer = null;
                        RemoveDataSource(axis);
                        View.RemoveAxis(axis);
                        axis.RangeChanged -= Axis_RangeChanged;
                        View.RemoveLabelPanel(axis);

                        if (axis.Series != null)
                        {
                            foreach (Series series in axis.DirectSeries())
                            {
                                series.RenderSeries(false);
                            }
                        }
                    }
                }
                if (Axes.Count == 0)
                {
                    ResetZoom();
                }
            }

            if (e.NewItems != null)
            {
                foreach (Axis axis in e.NewItems)
                {
                    if (axis != null)
                    {



                        axis.FastItemsSourceProvider = ActualSyncLink;
                        axis.SeriesViewer = this;
                        View.AttachAxis(axis);
                        axis.RangeChanged += Axis_RangeChanged;
                        View.AddLabelPanel(axis);
                    }
                }
            }
            this.NotifyThumbnailAppearanceChanged();
        }

        private void Axes_CollectionResetting(object sender, EventArgs e)
        {
            //construct a list of series that have to be refreshed.
            //this includes series in chart.Series and in axis.Series
            List<Series> seriesList = new List<Series>();

            foreach (Axis axis in Axes)
            {
                if (axis.Series != null)
                {
                    foreach (Series series in axis.DirectSeries())
                    {
                        if (!seriesList.Contains(series))
                        {
                            seriesList.Add(series);
                        }
                    }
                }

                axis.FastItemsSourceProvider = null;
                axis.SeriesViewer = null;
                RemoveDataSource(axis);
                View.RemoveAxis(axis);
                axis.RangeChanged -= Axis_RangeChanged;
                View.RemoveLabelPanel(axis);
                ResetZoom();
            }

            foreach (var series in Series)
            {
                if (!seriesList.Contains(series))
                {
                    seriesList.Add(series);
                }
            }

            foreach (Series series in seriesList)
            {
                series.RenderSeries(false);
            }
        }








        [Weak]
        private void Axis_RangeChanged(object sender, AxisRangeChangedEventArgs args)
        {
            var targetAxis = sender as Axis;
            if (targetAxis == null)
            {
                //this shouldn't be possible.
                return;
            }

            NotifyThumbnailAppearanceChanged();


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


            if (targetAxis.CrossingAxis != null)
            {
                targetAxis.CrossingAxis.Refresh();
            }
            targetAxis.Refresh();

            bool animate = AnimateSeriesWhenAxisRangeChanges;
            if (targetAxis is CategoryAxisBase)
            {
                animate = true;
            }



            //[MaxR] We shouldn't have to re-render all the series in the chart if the axis range changes.
            //Just the series that belong to that axis.
            //foreach (Series series in Series)
            //{
            //    series.RenderSeries(false);
            //}

            foreach (Series series in targetAxis.DirectSeries())
            {
                series.ThumbnailDirty = true;
                NotifyThumbnailAppearanceChanged();
                series.RenderSeries(animate);
            }

            foreach (Series series in targetAxis.SeriesViewer.Series)
            {
                OnValueOverlayRangeChanged(targetAxis, animate, series);
            }

            //Also, there could be other axes that use the current axis as a crossing axis, without the current axis knowing about it.
            //We need to notify such axes that the range has changed.
            foreach (Axis chartAxis in Axes)
            {
                if (chartAxis != targetAxis && chartAxis.CrossingAxis != null && chartAxis.CrossingAxis == targetAxis)
                {
                    chartAxis.Refresh();
                }
            }



        }

        private static void OnValueOverlayRangeChanged(Axis targetAxis, bool animate, Series series)
        {

            ValueOverlay overlay = series as ValueOverlay;
            if (overlay != null)
            {
                if (overlay.Axis == targetAxis)
                {
                    overlay.RenderSeries(animate);
                }
                else if (overlay.Axis is NumericAngleAxis)
                {
                    NumericAngleAxis numericAngleAxis = (NumericAngleAxis)overlay.Axis;
                    if (numericAngleAxis.RadiusAxis == targetAxis)
                    {
                        overlay.RenderSeries(animate);
                    }
                }
            }

        }

        #endregion Axes

    

       


        #region OnCreateAutomationPeer

        /// <summary>
        /// Called when the automation peer has been created.
        /// </summary>
        /// <returns></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamDataChartAutomationPeer(this);
        }

        #endregion OnCreateAutomationPeer



        internal override void UpdateSyncLink(SyncLink oldLink, SyncLink newLink)
        {
            base.UpdateSyncLink(oldLink, newLink);
            foreach (Axis axis in Axes)
            {
                axis.FastItemsSourceProvider = ActualSyncLink;
                axis.SeriesViewer = this;
            }
        }

        /// <summary>
        /// Gets the z index to use for the grid visuals.
        /// </summary>
        public int GridZIndex
        {
            get
            {
                return GridMode == GridMode.BeforeSeries ? 2 : 1;
            }
        }

        /// <summary>
        /// Gets the z index to use for the series visuals.
        /// </summary>
        public int SeriesZIndex
        {
            get
            {
                return GridMode == GridMode.BeforeSeries ? 1 : 2;
            }
        }




#region Infragistics Source Cleanup (Region)









































































#endregion // Infragistics Source Cleanup (Region)


       /// <summary>
       /// Gets a brush based on a series index.
       /// </summary>
       /// <param name="index">The series index.</param>
       /// <returns>The brush based on the provided index.</returns>
        protected internal override Brush GetBrushByIndex(int index)
        {
            return this.GetBrushByIndex(index, this.Brushes);
        }
        private Brush GetBrushByIndex(int index, BrushCollection brushes)
        {
            if (brushes != null && brushes.Count > 0)
            {
                return brushes[index % brushes.Count];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the appropriate marker color to use based on the series index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The color to use.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Brush GetMarkerBrushByIndex(int index)
        {
            return this.GetBrushByIndex(index, this.MarkerBrushes);
        }

        /// <summary>
        /// Gets the appropriate marker outline color to use based on the series index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The color to use.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Brush GetMarkerOutlineByIndex(int index)
        {
            return this.GetBrushByIndex(index, this.MarkerOutlines);
        }

        /// <summary>
        /// Gets the appropriate outline color based on the series index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The color to use.</returns>
        protected internal override Brush GetOutlineByIndex(int index)
        {
            return this.GetBrushByIndex(index, this.Outlines);
        }

        internal override bool EffectiveIsSquare()
        {
            if (IsSquare)
            {
                return true;
            }

            return HasPolarOrRadial();
        }

        private bool HasPolarOrRadial()
        {
            var polarAndRadial =
                from series in Series
                where series is PolarBase ||
                series is RadialBase
                select series;

            if (polarAndRadial.Any())
            {
                return true;
            }

            var angleAndRadius =
                from axis in Axes
                where axis is NumericAngleAxis ||
                axis is NumericRadiusAxis ||
                axis is CategoryAngleAxis
                select axis;

            if (angleAndRadius.Any())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesViewerView CreateView()
        {
            return new XamDataChartView(this);
        }

        /// <summary>
        /// Called when the view for the chart is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected override void OnViewCreated(SeriesViewerView view)
        {
            base.OnViewCreated(view);
            ChartView = (XamDataChartView)view;
        }
        internal XamDataChartView ChartView { get; set; }

        #region PlotAreaBackgroundContent Property

        /// <summary>
        /// Gets or sets the FrameworkElement used as the background for the current Chart object's grid area.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public FrameworkElement PlotAreaBackgroundContent
        {
            get
            {
                return (FrameworkElement)GetValue(PlotAreaBackgroundContentProperty);
            }
            set
            {
                SetValue(PlotAreaBackgroundContentProperty, value);
            }
        }

        internal const string PlotAreaBackgroundContentPropertyName = "PlotAreaBackgroundContent";

        /// <summary>
        /// Identifies the PlotAreaBackgroundContent dependency property.
        /// </summary>
        public static readonly DependencyProperty PlotAreaBackgroundContentProperty = DependencyProperty.Register(PlotAreaBackgroundContentPropertyName, typeof(FrameworkElement), typeof(XamDataChart),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as XamDataChart).RaisePropertyChanged(PlotAreaBackgroundContentPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PlotAreaBackgroundContent Property
        /// <summary>
        /// Returns the chart visuals expressed as a ChartVisualData object.
        /// </summary>
        /// <returns>A ChartVisualData object representing the current chart visuals.</returns>
        public ChartVisualData ExportVisualData()
        {
            var cvd = new ChartVisualData();
            for (var i = 0; i < Axes.Count; i++)
            {
                var avd = Axes[i].ExportVisualData();
                cvd.Axes.Add(avd);
            }
            for (var i = 0; i < Series.Count; i++)
            {
                var svd = this.Series[i].ExportVisualData();
                cvd.Series.Add(svd);
            }
            cvd.Name = Name;

            return cvd;
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