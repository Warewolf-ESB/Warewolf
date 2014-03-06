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
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// This class manages the creation of various visual series fragments.
    /// </summary>
    internal class StackedSeriesManager
    {
        internal StackedSeriesManager(StackedSeriesBase parent) 
        {
            if (parent == null) return;

            ParentSeries = parent;
            SeriesVisual = new ObservableCollection<AnchoredCategorySeries>();
            SeriesLogical = new StackedSeriesCollection();
            PositiveSeries = new ObservableCollection<AnchoredCategorySeries>();
            NegativeSeries = new ObservableCollection<AnchoredCategorySeries>();
            PlotArea = parent.PlotArea;
            SeriesPanel = parent.SeriesPanel;

            SeriesLogical.CollectionChanged += (o, e) => 
            {
                if (e.OldItems != null)
                {
                    foreach (StackedFragmentSeries logicalSeries in e.OldItems)
                    {
                        if (SeriesVisual.Contains(logicalSeries.VisualSeriesLink))
                        {
                            SeriesVisual.Remove(logicalSeries.VisualSeriesLink);
                        }
                    }
                }

                if (e.NewItems != null)
                {
                    int counter = e.NewStartingIndex;
                    foreach (StackedFragmentSeries logicalSeries in e.NewItems)
                    {
                        AnchoredCategorySeries series = CreateSeries(logicalSeries);
                        SeriesVisual.Insert(counter, series);
                        counter++;
                    }
                }
            };

            SeriesVisual.CollectionChanged += (o, e) => 
            {
                if (e.OldItems != null)
                {
                    foreach (AnchoredCategorySeries visualSeries in e.OldItems)
                    {
                        //clear out columns, markers.
                        visualSeries.ClearRendering(true, visualSeries.View);

                        visualSeries.SeriesViewer = null;
                        visualSeries.SyncLink = null;
                        visualSeries.ItemsSource = null;
                        visualSeries.Legend = null;

                        if (SeriesPanel != null && SeriesPanel.Children.Contains(visualSeries))
                        {
                            SeriesPanel.Children.Remove(visualSeries);
                        }
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (AnchoredCategorySeries visualSeries in e.NewItems)
                    {
                        visualSeries.SeriesViewer = parent.SeriesViewer;
                        visualSeries.SyncLink = parent.SyncLink;

                        if (!SeriesPanel.Children.Contains(visualSeries))
                        {
                            SeriesPanel.Children.Add(visualSeries);
                        }
                    }
                }

                RenderSeries();
            };
        }

        internal StackedSeriesBase ParentSeries { get; set; }
        internal ObservableCollection<AnchoredCategorySeries> SeriesVisual { get; set; }
        internal StackedSeriesCollection SeriesLogical { get; set; }
        internal ObservableCollection<AnchoredCategorySeries> PositiveSeries { get; set; }
        internal ObservableCollection<AnchoredCategorySeries> NegativeSeries { get; set; }
        internal Canvas PlotArea { get; set; }
        internal Grid SeriesPanel { get; set; }


        /// <summary>
        /// Creates a series fragment based on the parent series type.
        /// </summary>
        internal AnchoredCategorySeries CreateSeries(StackedFragmentSeries seriesFragment)
        {
            if (ParentSeries is StackedLineSeries)
            {
                LineFragment series = new LineFragment();
                series.ParentSeries = ParentSeries;
                seriesFragment.VisualSeriesLink = series;
                series.LogicalSeriesLink = seriesFragment;
                SetSeriesBindings(series, seriesFragment);
                return series;
            }
            if (ParentSeries is StackedColumnSeries)
            {
                ColumnFragment series = new ColumnFragment();
                series.ParentSeries = ParentSeries;
                seriesFragment.VisualSeriesLink = series;
                series.LogicalSeriesLink = seriesFragment;
                SetSeriesBindings(series, seriesFragment);
                return series;
            }
            if (ParentSeries is StackedBarSeries)
            {
                BarFragment series = new BarFragment();
                series.ParentSeries = ParentSeries as StackedBarSeries;
                seriesFragment.VisualSeriesLink = series;
                series.LogicalSeriesLink = seriesFragment;
                SetSeriesBindings(series, seriesFragment);
                return series;
            }
            if (ParentSeries is StackedAreaSeries)
            {
                AreaFragment series = new AreaFragment();
                series.ParentSeries = ParentSeries;
                seriesFragment.VisualSeriesLink = series;
                series.LogicalSeriesLink = seriesFragment;
                SetSeriesBindings(series, seriesFragment);
                return series;
            }
            if (ParentSeries is StackedSplineSeries)
            {
                SplineFragment series = new SplineFragment();
                series.ParentSeries = ParentSeries;
                seriesFragment.VisualSeriesLink = series;
                series.LogicalSeriesLink = seriesFragment;
                SetSeriesBindings(series, seriesFragment);
                return series;
            }
            if (ParentSeries is StackedSplineAreaSeries)
            {
                SplineAreaFragment series = new SplineAreaFragment();
                series.ParentSeries = ParentSeries;
                seriesFragment.VisualSeriesLink = series;
                series.LogicalSeriesLink = seriesFragment;
                SetSeriesBindings(series, seriesFragment);
                return series;
            }

            return null;
        }

        private void SetSeriesBindings(AnchoredCategorySeries visualSeries, StackedFragmentSeries logicalSeries)
        {
            visualSeries.SetBinding(Series.BrushProperty, new Binding(StackedFragmentSeries.ActualBrushPropertyName) { Source = logicalSeries });

            visualSeries.SetBinding(Series.CursorProperty, new Binding(StackedFragmentSeries.ActualCursorPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.EffectProperty, new Binding(StackedFragmentSeries.ActualEffectPropertyName) { Source = logicalSeries });

            visualSeries.SetBinding(Series.DashArrayProperty, new Binding(StackedFragmentSeries.ActualDashArrayPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.DashCapProperty, new Binding(StackedFragmentSeries.ActualDashCapPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.EndCapProperty, new Binding(StackedFragmentSeries.ActualEndCapPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.IsHitTestVisibleProperty, new Binding(StackedFragmentSeries.ActualIsHitTestVisiblePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(CategorySeries.ItemsSourceProperty, new Binding(StackedSeriesBase.ItemsSourcePropertyName) { Source = ParentSeries });
            visualSeries.SetBinding(Series.LegendProperty, new Binding(StackedSeriesBase.ActualLegendPropertyName) { Source = ParentSeries });
            //visualSeries.SetBinding(Series.LegendItemBadgeTemplateProperty, new Binding(StackedFragmentSeries.ActualLegendItemBadgeTemplatePropertyName) { Source = logicalSeries });
            //visualSeries.SetBinding(Series.LegendItemTemplateProperty, new Binding(StackedFragmentSeries.ActualLegendItemTemplatePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.LegendItemVisibilityProperty, new Binding(StackedFragmentSeries.ActualLegendItemVisibilityPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.LineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = ParentSeries });
            visualSeries.SetBinding(CategorySeries.MarkerBrushProperty, new Binding(StackedFragmentSeries.ActualMarkerBrushPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(CategorySeries.MarkerOutlineProperty, new Binding(StackedFragmentSeries.ActualMarkerOutlinePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(CategorySeries.MarkerStyleProperty, new Binding(StackedFragmentSeries.ActualMarkerStylePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(CategorySeries.MarkerTemplateProperty, new Binding(StackedFragmentSeries.ActualMarkerTemplatePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(CategorySeries.MarkerTypeProperty, new Binding(StackedFragmentSeries.ActualMarkerTypePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.MiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = ParentSeries });
            visualSeries.SetBinding(Series.OpacityProperty, new Binding(StackedFragmentSeries.OpacityPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.OpacityMaskProperty, new Binding(StackedFragmentSeries.OpacityMaskPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.OutlineProperty, new Binding(StackedFragmentSeries.ActualOutlinePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.ResolutionProperty, new Binding(StackedSeriesBase.ResolutionPropertyName) { Source = ParentSeries });
            visualSeries.SetBinding(Series.StartCapProperty, new Binding(StackedFragmentSeries.ActualStartCapPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.ThicknessProperty, new Binding(StackedFragmentSeries.ActualThicknessPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.TitleProperty, new Binding(StackedFragmentSeries.TitlePropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.ToolTipProperty, new Binding(StackedFragmentSeries.ActualToolTipPropertyName) { Source = logicalSeries });




            visualSeries.SetBinding(MarkerSeries.UseLightweightMarkersProperty, new Binding(StackedFragmentSeries.ActualUseLightweightMarkersPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(AnchoredCategorySeries.ValueMemberPathProperty, new Binding(StackedFragmentSeries.ValueMemberPathPropertyName) { Source = logicalSeries });
            visualSeries.SetBinding(Series.VisibilityProperty, new Binding(StackedFragmentSeries.ActualVisibilityPropertyName) { Source = logicalSeries });

            if (visualSeries is ColumnFragment)
            {
                visualSeries.SetBinding(ColumnFragment.RadiusXProperty, new Binding(StackedFragmentSeries.ActualRadiusXPropertyName) { Source = logicalSeries });
                visualSeries.SetBinding(ColumnFragment.RadiusYProperty, new Binding(StackedFragmentSeries.ActualRadiusYPropertyName) { Source = logicalSeries });
            }

            if (visualSeries is BarFragment)
            {
                visualSeries.SetBinding(BarFragment.RadiusXProperty, new Binding(StackedFragmentSeries.ActualRadiusXPropertyName) { Source = logicalSeries });
                visualSeries.SetBinding(BarFragment.RadiusYProperty, new Binding(StackedFragmentSeries.ActualRadiusYPropertyName) { Source = logicalSeries });
            }
        }

        internal void RenderSeries()
        {
            PositiveSeries.Clear();
            NegativeSeries.Clear();

            foreach (var series in SeriesVisual)
            {
                series.ThumbnailDirty = true;
                series.SeriesViewer = ParentSeries.SeriesViewer;
                series.SyncLink = ParentSeries.SyncLink;
                series.Index = ParentSeries.GetFragmentSeriesIndex(SeriesLogical[SeriesVisual.IndexOf(series)]);

                Canvas.SetZIndex(series, SeriesVisual.Count - series.Index);

                if (SeriesLogical[SeriesVisual.IndexOf(series)].Positive)
                {
                    PositiveSeries.Add(series);
                }
                else
                {
                    NegativeSeries.Add(series);
                }

                //don't set the axes on the column and bar series, to avoid incorrect group size calculation
                if (ParentSeries is StackedLineSeries 
                    || ParentSeries is StackedAreaSeries
                    || ParentSeries is StackedSplineSeries
                    || ParentSeries is StackedSplineAreaSeries)
                {
                    series.SetXAxis(ParentSeries.GetXAxis());
                    series.SetYAxis(ParentSeries.GetYAxis());
                }

                series.RenderSeries(false);
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