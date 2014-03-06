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

namespace Infragistics.Controls.Charts
{
    internal class LegendBaseView
    {
        public LegendBaseView(LegendBase model)
        {
            Model = model;
        }
        protected LegendBase Model { get; set; }

        internal virtual void OnInit()
        {
            
        }

        protected DataChartLegendMouseButtonEventArgs CreateMouseButtonArgs(object legendItem, MouseButtonEventArgs e)
        {
            SeriesViewer chart;
            Series series;
            object item;
            FetchLegendEnvironment(legendItem, out chart, out series, out item);

            DataChartLegendMouseButtonEventArgs args =
                new DataChartLegendMouseButtonEventArgs(chart, series, item, e, legendItem);

            return args;
        }

        protected ChartLegendMouseEventArgs CreateMouseArgs(object legendItem, MouseEventArgs e)
        {
            SeriesViewer chart;
            Series series;
            object item;
            FetchLegendEnvironment(legendItem, out chart, out series, out item);

            ChartLegendMouseEventArgs args =
                new ChartLegendMouseEventArgs(chart, series, item, e, legendItem);

            return args;
        }

        protected internal virtual void FetchLegendEnvironment(object legendItem, out SeriesViewer chart,
            out Series series, out object item)
        {
            chart = (Model.Owner as SeriesViewer);
            series = null;
            item = null;
            if (legendItem != null)
            {
                ContentControl contentControl = legendItem as ContentControl;
                if (contentControl != null && contentControl.Content != null &&
                    contentControl.Content is DataContext)
                {
                    DataContext dc = contentControl.Content as DataContext;
                    series = dc.Series as Series;
                    if (series != null)
                    {
                        chart = series.SeriesViewer;
                    }
                    item = dc.Item;
                }
            }
        }

        internal virtual void DetachContent()
        {

        }

        internal virtual void OnTemplateProvided()
        {
            
        }

        internal bool Ready()
        {
            if (Model.ContentPresenter == null)
            {
                return false;
            }

            return true;
        }

        internal void AttachItemEvents(UIElement uiElement)
        {
            uiElement.MouseLeftButtonDown += RaiseLegendItemMouseLeftButtonDown;
            uiElement.MouseLeftButtonUp += RaiseLegendItemMouseLeftButtonUp;
            //uiElement.MouseRightButtonDown += RaiseLegendItemMouseRightButtonDown;
            //uiElement.MouseRightButtonUp += RaiseLegendItemMouseRightButtonUp;

            uiElement.MouseEnter += RaiseLegendItemMouseEnter;
            uiElement.MouseLeave += RaiseLegendItemMouseLeave;
            uiElement.MouseMove += RaiseLegendItemMouseMove;

        }

        internal void RemoveItemEvents(UIElement uiElement)
        {
            uiElement.MouseLeftButtonDown -= RaiseLegendItemMouseLeftButtonDown;
            uiElement.MouseLeftButtonUp -= RaiseLegendItemMouseLeftButtonUp;
            //uiElement.MouseRightButtonDown -= RaiseLegendItemMouseRightButtonDown;
            //uiElement.MouseRightButtonUp -= RaiseLegendItemMouseRightButtonUp;

            uiElement.MouseEnter -= RaiseLegendItemMouseEnter;
            uiElement.MouseLeave -= RaiseLegendItemMouseLeave;
            uiElement.MouseMove -= RaiseLegendItemMouseMove;

        }

        #region Mouse Events       

        internal void RaiseLegendItemMouseLeftButtonDown(object legendItem, MouseButtonEventArgs e)
        {
            DataChartLegendMouseButtonEventArgs args = CreateMouseButtonArgs(legendItem, e);

            Model.OnLegendItemMouseLeftButtonDown(args);
        }

        

        internal void RaiseLegendItemMouseLeftButtonUp(object legendItem, MouseButtonEventArgs e)
        {
            DataChartLegendMouseButtonEventArgs args = CreateMouseButtonArgs(legendItem, e);

            Model.OnLegendItemMouseLeftButtonUp(args);
        }


       
        internal void RaiseLegendItemMouseEnter(object legendItem, MouseEventArgs e)
        {
            ChartLegendMouseEventArgs args = CreateMouseArgs(legendItem, e);
            Model.OnLegendItemMouseEnter(args);
        }

        
        internal void RaiseLegendItemMouseLeave(object legendItem, MouseEventArgs e)
        {
            ChartLegendMouseEventArgs args = CreateMouseArgs(legendItem, e);
            Model.OnLegendItemMouseLeave(args);
        }

        
        internal void RaiseLegendItemMouseMove(object legendItem, MouseEventArgs e)
        {
            ChartLegendMouseEventArgs args = CreateMouseArgs(legendItem, e);
            Model.OnLegendItemMouseMove(args);
        }


        #endregion Mouse Events
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