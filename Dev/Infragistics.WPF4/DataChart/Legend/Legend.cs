using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.Controls.Charts.AutomationPeers;
using System;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a legend in a XamDataChart control.
    /// </summary>

    [TemplatePart(Name = Legend.ContentPresenterName, Type = typeof(ContentPresenter))]

    [WidgetIgnoreDepends("FragmentBase")]
    [WidgetIgnoreDepends("SplineFragmentBase")]
    [WidgetIgnoreDepends("XamDataChart")]
    [WidgetModule("CategoryChart")]
    [WidgetModule("RangeCategoryChart")]
    [WidgetModule("ScatterChart")]
    [WidgetModule("FinancialChart")]
    [WidgetModule("PieChart")]
    [WidgetModule("RadialChart")]
    [WidgetModule("PolarChart")]
    public sealed class Legend : LegendBase
    {
        internal override LegendBaseView CreateView()
        {
            return new LegendView(this);
        }
        internal override void OnViewCreated(LegendBaseView view)
        {
            base.OnViewCreated(view);

            LegendView = (LegendView)view;
        }
        internal LegendView LegendView { get; set; }

        /// <summary>
        /// Initializes a default, empty Legend object.
        /// </summary>
        public Legend()
        {
            DefaultStyleKey = typeof(Legend);
            
            Children.CollectionChanged += (o, e) =>
            {
                if (e.OldItems != null)
                {
                    foreach (object item in e.OldItems)
                    {
                        LegendView.RemoveItemVisual(item);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (object item in e.NewItems)
                    {
                        LegendView.AddItemVisual(item);
                    }
                }
            };
        }

        /// <summary>
        /// Adds a child to the Legend maintaining the correct ordering.
        /// </summary>
        /// <param name="legendItem">The item to add.</param>
        /// <param name="series">The series represented by the item.</param>
        protected internal override void AddChildInOrder(UIElement legendItem, Series series)
        {

            if (series is StackedSeriesBase) return;


            int index = 0;
            foreach (UIElement item in Children)
            {
                SeriesViewer itemChart;
                Series itemSeries;
                object itemItem;
                View.FetchLegendEnvironment(item, out itemChart,
                    out itemSeries, out itemItem);

                if (series.SeriesViewer != null &&
                    itemChart != null &&
                    (GetSortOrder(series.SeriesViewer) < GetSortOrder(itemChart) ||
                    (GetSortOrder(series.SeriesViewer) == -1 &&
                    GetSortOrder(itemChart) == -1 &&
                    series.SeriesViewer.GetHashCode() < itemChart.GetHashCode())))
                {
                    break;
                }

                if (series.SeriesViewer != null &&
                   itemChart != null &&
                   series.SeriesViewer == itemChart &&
                   itemSeries != null)
                {
                    int indexOfSeries = series.Index;
                    int indexOfItemSeries = itemSeries.Index;


                    if (series is FragmentBase || series is SplineFragmentBase)
                    {
                        StackedSeriesBase parentSeries = series is FragmentBase
                            ? (series as FragmentBase).ParentSeries
                            : (series as SplineFragmentBase).ParentSeries;

                        if (parentSeries.ReverseLegendOrder)
                        {
                            indexOfSeries = parentSeries.Index + parentSeries.ActualSeries.Count - parentSeries.StackedSeriesManager.SeriesVisual.IndexOf(series as AnchoredCategorySeries);
                        }
                    }

                    if (itemSeries is FragmentBase || itemSeries is SplineFragmentBase)
                    {
                        StackedSeriesBase parentSeries = itemSeries is FragmentBase
                            ? (itemSeries as FragmentBase).ParentSeries
                            : (itemSeries as SplineFragmentBase).ParentSeries;

                        if (parentSeries.ReverseLegendOrder)
                        {
                            indexOfItemSeries = parentSeries.Index + parentSeries.ActualSeries.Count - parentSeries.StackedSeriesManager.SeriesVisual.IndexOf(itemSeries as AnchoredCategorySeries);
                        }
                    }


                    if (indexOfSeries <= indexOfItemSeries)
                    {
                        break;
                    }
                }

                index++;
            }

            Children.Insert(index, legendItem);
        }

        #region OnCreateAutomationPeer


        /// <summary>
        /// Called when the automation peer has been created.
        /// </summary>
        /// <returns>Created automation peer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new LegendAutomationPeer(this);
        }


        #endregion OnCreateAutomationPeer

        #region Orientation Dependency Property

        /// <summary>
        /// Gets or sets the current Legend object's orientation.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(OrientationPropertyName, typeof(Orientation), typeof(Legend),
            new PropertyMetadata(Orientation.Vertical, (sender, e) =>
            {
                (sender as Legend).RaisePropertyChanged(OrientationPropertyName, e.OldValue, e.NewValue);
            }));

        internal const string OrientationPropertyName = "Orientation";

        #endregion Orientation Dependency Property

        #region "SortOrder Attached Property"


        /// <summary>
        /// Identifies the SortOrder attached property.
        /// </summary>
        public static readonly DependencyProperty SortOrderProperty =
            DependencyProperty.RegisterAttached("SortOrder",
            typeof(int),
            typeof(Legend),
            new PropertyMetadata(-1,
                (o, e) =>
                {
                    (o as XamDataChart).OnLegendSortChanged();
                }));

        /// <summary>
        /// Set the sort order in the legend for a chart.
        /// </summary>
        /// <param name="target">The chart to set the sort order for.</param>
        /// <param name="sortOrder">The sort order to set.</param>
        public static void SetSortOrder(DependencyObject target, int sortOrder)
        {
            target.SetValue(SortOrderProperty, sortOrder);
        }

        /// <summary>
        /// Get the sort order in the legend for a chart.
        /// </summary>
        /// <param name="target">The chart to get the sort order for.</param>
        public static int GetSortOrder(DependencyObject target)
        {
            return (int)target.GetValue(SortOrderProperty);
        }






        #endregion "SortOrder Attached Property"
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