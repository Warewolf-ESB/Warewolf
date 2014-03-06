using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a legend that displays an item for each point in the series.
    /// </summary>

    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]

    [WidgetIgnoreDepends("BubbleSeries")]
    [WidgetModule("CategoryChart")]
    [WidgetModule("RangeCategoryChart")]
    [WidgetModule("ScatterChart")]
    [WidgetModule("FinancialChart")]
    [WidgetModule("PieChart")]
    [WidgetModule("RadialChart")]
    [WidgetModule("PolarChart")]
    public class ItemLegend : LegendBase
    {
        internal override LegendBaseView CreateView()
        {
            return new ItemLegendView(this);
        }
        internal override void OnViewCreated(LegendBaseView view)
        {
            base.OnViewCreated(view);

            ItemView = (ItemLegendView)view;
        }
        internal ItemLegendView ItemView { get; set; }

        #region C'tor

        /// <summary>
        /// Creates a new instance of the item legend.
        /// </summary>
        public ItemLegend()
        {
            DefaultStyleKey = typeof(ItemLegend);

            Children.CollectionChanged += (o, e) =>
            {
                if (e.OldItems != null)
                {
                    foreach (object item in e.OldItems)
                    {
                        ItemView.RemoveItemVisual(item);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (object item in e.NewItems)
                    {
                        ItemView.AddItemVisual(item);
                    }
                }
            };
        }

        #endregion C'tor

        /// <summary>
        /// Adds a child, in order, to the legend.
        /// </summary>
        /// <param name="legendItem">The item to add.</param>
        /// <param name="series">The series with which the item is associated.</param>
        protected internal override void AddChildInOrder(UIElement legendItem, Series series)
        {
            if (!View.Ready())
            {
                return;
            }

            RenderLegend(series);
        }

        /// <summary>
        /// Creates legend items for the legend.
        /// </summary>
        /// <param name="legendItems">The legend items to add.</param>
        /// <param name="itemsHost">The host for the items.</param>
        protected internal override void CreateLegendItems(List<UIElement> legendItems, Control itemsHost)
        {
            ClearLegendItems(itemsHost);

            if (itemsHost == null || legendItems == null || legendItems.Count == 0)
            {
                return;
            }

            foreach (var currentLegendItem in legendItems)
            {

                ContentControl contentControl = currentLegendItem as ContentControl;
                if (contentControl != null && contentControl.Content != null)
                {
                    DataContext context = contentControl.Content as DataContext;
                    if (context != null && !ContainsContext(context))
                    {
                        Children.Add(currentLegendItem);
                    }
                }
            }
        }

        internal void RenderLegend(Control itemsHost)
        {
            ClearLegendItems(itemsHost);

            BubbleSeries bubbleSeries = itemsHost as BubbleSeries;
            if (bubbleSeries != null && bubbleSeries.LabelColumn != null && bubbleSeries.LegendItems != null && bubbleSeries.LegendItems.Count > 0)
            {
                foreach (var legendItem in bubbleSeries.LegendItems)
                {
                    ContentControl contentControl = legendItem as ContentControl;
                    if (contentControl != null && contentControl.Content != null)
                    {
                        DataContext context = contentControl.Content as DataContext;
                        if (context != null && !ContainsContext(context))
                        {
                            Children.Add(legendItem);
                        }
                    }
                }
            }
        }

        internal void ClearLegendItems(Control itemsHost)
        {
            if (itemsHost == null || Children == null || Children.Count == 0)
                return;

            ObservableCollection<UIElement> legendItems = new ObservableCollection<UIElement>();

            foreach (var existingLegendItem in Children)
            {
                ContentControl contentControl = existingLegendItem as ContentControl;
                if (contentControl != null && contentControl.Content != null)
                {
                    DataContext context = contentControl.Content as DataContext;
                    if (context != null && context.Series == itemsHost)
                    {
                        legendItems.Add(existingLegendItem);
                    }
                }
            }

            foreach (var legendItem in legendItems)
            {
                Children.Remove(legendItem);
            }
        }

        private bool ContainsContext(DataContext dataContext)
        {
            return ItemView.ContainsContext(dataContext);
        }

        internal IFastItemColumn<double> FillColumn { get; set; }

        internal BrushScale ColorAxis { get; set; }
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