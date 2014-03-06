using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace Infragistics.Windows.Chart
{
    internal class ChartPane : ChartCanvas
    {
        private Chart _chart;

        private Size FinalSize
        {
            get
            {
                return _chart.FinalSize;
            }
            set
            {
                _chart.FinalSize = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChartPane(Chart chart)
        {
            _chart = chart;
        }

        
        /// <summary>
        /// Measures the child elements of a Canvas in anticipation of arranging 
        /// them during the ArrangeOverride pass. 
        /// </summary>
        /// <param name="availableSize">An upper limit Size that should not be exceeded.</param>
        /// <returns>A Size that represents the size that is required to arrange child content.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // Introduce because of Infinite size exception when 
            // stacked panel is used.
            if (double.IsInfinity(availableSize.Width))
            {
                availableSize.Width = 400.0;
            }

            if (double.IsInfinity(availableSize.Height))
            {
                availableSize.Height = 300.0;
            }

            Size childSize = availableSize;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(childSize);
            }
            return availableSize;
        }

        /// <summary>
        /// Arranges the content of a Canvas element.
        /// </summary>
        /// <param name="arrangeSize">The size that this Canvas element should use to arrange its child elements.</param>
        /// <returns>A Size that represents the arranged size of this Canvas element and its descendants.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            FinalSize = arrangeSize;
            
            foreach (UIElement child in InternalChildren)
            {
                if (child is Rectangle)
                {
                    // Find absolute positions
                    Rectangle plottingPane = child as Rectangle;
                    double x = 0;
                    double y = 0;
                    double width = FinalSize.Width;
                    double height = FinalSize.Height;

                    child.Arrange(new Rect(x, y, width, height));
                    plottingPane.Width = width;
                    plottingPane.Height = height;
                }
                else if (child is ChartCanvas)
                {
                    // Find absolute positions
                    ChartCanvas plottingPane = child as ChartCanvas;
                    double x = plottingPane.RelativePosition.X / 100 * FinalSize.Width;
                    double y = plottingPane.RelativePosition.Y / 100 * FinalSize.Height;
                    double width = plottingPane.RelativePosition.Width / 100 * FinalSize.Width;
                    double height = plottingPane.RelativePosition.Height / 100 * FinalSize.Height;

                    child.Arrange(new Rect(x, y, width, height));
                    plottingPane.Width = width;
                    plottingPane.Height = height;
                }
                else if (child is ChartContentControl)
                {
                    // Find absolute positions
                    ChartContentControl chartContentControl = child as ChartContentControl;
                    double x = chartContentControl.RelativePosition.X / 100 * FinalSize.Width;
                    double y = chartContentControl.RelativePosition.Y / 100 * FinalSize.Height;
                    double width = chartContentControl.RelativePosition.Width / 100 * FinalSize.Width;
                    double height = chartContentControl.RelativePosition.Height / 100 * FinalSize.Height;

                    child.Arrange(new Rect(x, y, width, height));
                    chartContentControl.Width = width;
                    chartContentControl.Height = height;
                }
            }
            return arrangeSize; // Returns the final Arranged size
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