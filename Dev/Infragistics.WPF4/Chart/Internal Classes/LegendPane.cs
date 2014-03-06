
#region Using

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using Infragistics.Shared;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class LegendPane : ChartCanvas
    {
        #region Fields

        // Private fields
        private object _chartParent;
        private bool _itemForPoint;
        private Legend _legend;
        private double _maxItemSize = 30;
       
        #endregion Fields

        #region Properties

        /// <summary>
        /// Maximum size of legend item rectangle
        /// </summary>
        internal double MaxItemSize
        {
            get
            {
                return _maxItemSize;
            }
            set
            {
                _maxItemSize = value;
            }
        }

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        /// <summary>
        /// True if legend an item exist for every data point, otherwise there is 
        /// one legend item per series.
        /// </summary>
        internal bool ItemForPoint
        {
            get
            {
                return _itemForPoint;
            }
            set
            {
                _itemForPoint = value;
            }
        }

        #endregion Properties

        #region Methods

        internal LegendPane(Legend legend)
        {
            _legend = legend;
        }

        internal void Draw()
        {
            Children.Clear();

            Chart chart = _chartParent as Chart;
            chart.CreateFakeData();

            DrawItems();

            chart.RemoveFakeData();
        }
       
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Size = new Size(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);

            Draw();
         }

        internal int GetNumOfItems()
        {
            // Custom Legend Items
            if (_legend.Items.Count > 0)
            {
                return _legend.Items.Count;
            }

            SeriesCollection series = GetSeries();
            int numSeries = series.Count;

            int numItems = 0;
            for (int seriesIndex = 0; seriesIndex < numSeries; seriesIndex++)
            {
                // Skeep legend items for second series for pie and doughnut chart.
                if (!ChartSeries.GetChartAttribute(ChartTypeAttribute.Scene, series[seriesIndex]) && seriesIndex > 0)
                {
                    continue;
                }

                bool legendFromPoints = DataPoint.IsColorFromPoint(series[seriesIndex]);

                int numPoints = series[seriesIndex].DataPoints.Count;

                if (legendFromPoints)
                {
                    numItems += numPoints;
                }
                else
                {
                    numItems++;
                }
            }

            return numItems;
        }

        /// <summary>
        /// Draws legend items
        /// </summary>
        private void DrawItems()
        {
            SeriesCollection series = GetSeries();
            int numSeries = series.Count;

            int numItems = GetNumOfItems();
            double margin = 0.05;
            double leftPosition = Size.Width * margin;
            double topPosition = 0;

            double maxItemHeight = Size.Height / numItems;
            double itemSpace;
            if (Size.Width / Size.Height < SizeProportion)
            {
                itemSpace = Math.Min(Size.Width / SizeProportion, maxItemHeight);
            }
            else
            {
                itemSpace = maxItemHeight;
            }
            double itemSize = itemSpace * 0.8;

            if (itemSize > _maxItemSize)
            {
                itemSize = _maxItemSize;
            }
            
            topPosition = (Size.Height - itemSpace * numItems) / 2.0;

            // Custom Legend Items
            if (_legend.Items.Count > 0)
            {
                int itemIndex = 0;
                foreach (LegendItem legendItem in _legend.Items)
                {
                    string text = legendItem.Text;
                    if (_legend.UseDataTemplate)
                    {
                        LegendItemTemplate item = new LegendItemTemplate();
                        ContentControl content = new ContentControl();

                        item.Text = text;
                        item.Fill = legendItem.Fill;
                        item.Stroke = legendItem.Stroke;
                        item.StrokeThickness = legendItem.StrokeThickness;

                        content.Content = item;
                        ChartCanvas.SetLeft(content, 0);
                        ChartCanvas.SetTop(content, topPosition + itemIndex * itemSpace);
                        content.Width = this.ActualWidth;
                        content.Height = itemSpace;
                        Children.Add(content);
                    }
                    else
                    {
                        DrawItem(itemSize, series, 0, 0, leftPosition, topPosition, itemSpace, itemIndex, text, legendItem);
                    }
                    itemIndex++;
                }
            }
            else
            {

                int itemIndex = 0;
                for (int seriesIndex = 0; seriesIndex < numSeries; seriesIndex++)
                {
                    // Skeep legend items for second series for pie and doughnut chart.
                    if (!ChartSeries.GetChartAttribute(ChartTypeAttribute.Scene, series[seriesIndex]) && seriesIndex > 0)
                    {
                        continue;
                    }

                    bool legendFromPoints = ChartSeries.GetChartAttribute(ChartTypeAttribute.ColorFromPoint, series[seriesIndex]);
                    if (series[seriesIndex].DataPointColor == DataPointColor.Same)
                    {
                        legendFromPoints = false;
                    }
                    else if (series[seriesIndex].DataPointColor == DataPointColor.Different)
                    {
                        legendFromPoints = true;
                    }

                    if (!legendFromPoints)
                    {
                        string text = series[seriesIndex].Label;
                        if (string.IsNullOrEmpty(text))
                        {
                            text = SR.GetString("Item") + itemIndex;
                        }

                        if (_legend.UseDataTemplate)
                        {
                            LegendItemTemplate item = new LegendItemTemplate();
                            ContentControl content = new ContentControl();

                            item.Text = text;
                            if (series[seriesIndex].DataPoints.Count == 0)
                            {
                                item.Fill = series[seriesIndex].Fill;
                                item.Stroke = series[seriesIndex].Stroke;
                                item.StrokeThickness = series[seriesIndex].StrokeThickness;
                            }
                            else
                            {
                                DataPoint point = series[seriesIndex].DataPoints[0];
                                point.UpdateActualFill(seriesIndex, 0);
                                item.Fill = point.ActualFill;

                                item.Stroke = series[seriesIndex].DataPoints[0].GetLegendStroke();
                                item.StrokeThickness = series[seriesIndex].DataPoints[0].GetStrokeThickness(seriesIndex, 0);
                            }

                            content.Content = item;
                            ChartCanvas.SetLeft(content, 0);
                            ChartCanvas.SetTop(content, topPosition + itemIndex * itemSpace);
                            content.Width = this.ActualWidth;
                            content.Height = itemSpace;
                            Children.Add(content);
                        }
                        else
                        {
                            DrawItem(itemSize, series, seriesIndex, 0, leftPosition, topPosition, itemSpace, itemIndex, text, null);
                        }

                        itemIndex++;
                        continue;
                    }
                    int numPoints = series[seriesIndex].DataPoints.Count;
                    for (int pointIndex = 0; pointIndex < numPoints; pointIndex++)
                    {
                        string text = series[seriesIndex].DataPoints[pointIndex].Label;
                        if (string.IsNullOrEmpty(text))
                        {
                            text = SR.GetString("Item") + itemIndex;
                        }


                        if (_legend.UseDataTemplate)
                        {
                            LegendItemTemplate item = new LegendItemTemplate();
                            ContentControl content = new ContentControl();

                            item.Text = text;

                            DataPoint point = series[seriesIndex].DataPoints[pointIndex];
                            point.UpdateActualFill(seriesIndex, pointIndex);
                            item.Fill = point.ActualFill;

                            content.Content = item;
                            ChartCanvas.SetLeft(content, 0);
                            ChartCanvas.SetTop(content, topPosition + itemIndex * itemSpace);
                            content.Width = this.ActualWidth;
                            content.Height = itemSpace;
                            item.Width = content.Width;
                            item.Height = content.Height;
                            Children.Add(content);
                        }
                        else
                        {
                            DrawItem(itemSize, series, seriesIndex, pointIndex, leftPosition, topPosition, itemSpace, itemIndex, text, null);
                        }

                        itemIndex++;
                    }
                }
            }
        }

        private void DrawItem(double itemSize, SeriesCollection series, int seriesIndex, int pointIndex, double leftPosition, double topPosition, double itemSpace, int itemIndex, string text, LegendItem item)
        {
            
            Rectangle rect = new Rectangle();

            rect.Width = itemSize;
            rect.Height = itemSize;

            if (item != null)
            {
                rect.Fill = item.Fill;
                rect.Stroke = item.Stroke;
                rect.StrokeThickness = item.StrokeThickness;
            }
            else
            {
                if (pointIndex >= series[seriesIndex].DataPoints.Count)
                {
                    rect.Fill = series[seriesIndex].Fill;
                    rect.Stroke = series[seriesIndex].Stroke;
                    rect.StrokeThickness = series[seriesIndex].StrokeThickness;
                }
                else
                {
                    rect.Fill = series[seriesIndex].DataPoints[pointIndex].GetLegendFill(seriesIndex, pointIndex);
                    rect.Stroke = series[seriesIndex].DataPoints[pointIndex].GetLegendStroke();
                    rect.StrokeThickness = series[seriesIndex].DataPoints[pointIndex].GetLegendStrokeThickness();
                }
            }
            
            ChartCanvas.SetLeft(rect, leftPosition);
            ChartCanvas.SetTop(rect, topPosition + itemIndex * itemSpace + (itemSpace - itemSize) / 2.0);
                       
            double width = Size.Width - leftPosition - rect.Width * 1.5;
            width = width > 0 ? width : 0;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            Children.Add(textBlock);
            Size size = ChartCreator.GetTextSize(textBlock);
            textBlock.Background = Brushes.Transparent;
            textBlock.Width = width;

            if (itemSpace < textBlock.FontSize)
            {
                if (itemSpace * 0.7 <= 0)
                {
                    textBlock.FontSize = 1;
                }
                else
                {
                    textBlock.FontSize = itemSpace * 0.7;
                }
                textBlock.Height = itemSpace;
                Canvas.SetTop(textBlock, topPosition + itemIndex * itemSpace);
            }
            else
            {
                textBlock.Height = itemSpace;
                Canvas.SetTop(textBlock, topPosition + itemIndex * itemSpace + itemSpace / 2.0 - size.Height / 2.0);
            }

            Canvas.SetLeft(textBlock, leftPosition + rect.Width * 1.5);
           
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
           
            Children.Add(rect);
        }
        

        /// <summary>
        /// Get series from the current chart.
        /// </summary>
        private SeriesCollection GetSeries()
        {
            Chart container = _chartParent as Chart;
            return container.GetContainer().Series;
        }

        #endregion Methods
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