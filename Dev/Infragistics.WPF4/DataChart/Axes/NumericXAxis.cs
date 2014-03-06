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
using System.Windows.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart numeric X axis.
    /// </summary>
    [WidgetModule("ScatterChart")]
    [WidgetModule("CategoryChart")]
    [WidgetIgnoreDepends("CategoryAxisBase")]
    public class NumericXAxis : StraightNumericAxisBase, IScaler
    {
        internal override AxisView CreateView()
        {
            return new NumericXAxisView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);

            XView = (NumericXAxisView)view;
        }
        internal NumericXAxisView XView { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a new instance of the NumericXAxis class. 
        /// </summary>
        public NumericXAxis()
        {
            Renderer = CreateRenderer();
        }
        #endregion

        internal override AxisLabelPanelBase CreateLabelPanel()
        {
            return new HorizontalAxisLabelPanel();
        }

        /// <summary>
        /// Creates the rendering provider for the axis.
        /// </summary>
        /// <returns>The axis renderer.</returns>
        internal override NumericAxisRenderer CreateRenderer()
        {
            NumericAxisRenderer renderer = base.CreateRenderer();

            renderer.LabelManager.FloatPanelAction = (crossing) =>
            {
                if ((this.LabelSettings == null || this.LabelSettings.Visibility == Visibility.Visible) && this.CrossingAxis != null)
                {
                    this.LabelPanel.CrossingValue = crossing;
                    if (this.LabelSettings != null && (this.LabelSettings.Location == AxisLabelsLocation.InsideTop || this.LabelSettings.Location == AxisLabelsLocation.InsideBottom))
                    {
                        SeriesViewer.InvalidatePanels();
                    }
                }
            };

            renderer.Line = (p, g, value) =>
            {
                VerticalLine(g, value, p.ViewportRect);
            };

            renderer.Strip = (p, g, start, end) =>
            {
                VerticalStrip(g, start, end, p.ViewportRect);
            };

            renderer.Scaling = (p, unscaled) =>
            {
                ScalerParams sParams = new ScalerParams(p.WindowRect, p.ViewportRect, this.IsInvertedCached);
                return GetScaledValue(unscaled, sParams);
            };

            renderer.ShouldRenderLines = (p, value) =>
            {
                return true;
            };

            renderer.AxisLine = (p) =>
            {
                HorizontalLine(p.AxisGeometry, p.CrossingValue, p.ViewportRect);
            };

            renderer.DetermineCrossingValue = (p) =>
            {
                //p.CrossingValue = (this.LabelSettings.Location == AxisLabelsLocation.InsideTop
                //|| this.LabelSettings.Location == AxisLabelsLocation.OutsideTop)
                //? p.ViewportRect.Top : p.ViewportRect.Bottom;

                //this axis is always at the bottom unless crossig axis is set
                p.CrossingValue = p.ViewportRect.Bottom;

                ScalerParams sParams = new ScalerParams(p.WindowRect, p.ViewportRect, this.IsInvertedCached);

                if (this.CrossingAxis != null && this.CrossingAxis.SeriesViewer != null)
                {




                    p.CrossingValue = Convert.ToDouble(CrossingValue);

                    p.CrossingValue = this.CrossingAxis.GetScaledValue(p.CrossingValue, sParams);

                    CategoryAxisBase categoryAxis = CrossingAxis as CategoryAxisBase;
                    if (categoryAxis != null && categoryAxis.CategoryMode == CategoryMode.Mode2)
                    {
                        double offset = 0.5 * categoryAxis.GetCategorySize(p.WindowRect, p.ViewportRect);
                        if (!categoryAxis.IsInverted) offset = -offset;
                        p.CrossingValue = p.CrossingValue + offset;
                    }
                    
                    if (p.CrossingValue < p.ViewportRect.Top)
                    {
                        p.CrossingValue = p.ViewportRect.Top;
                    }
                    else if (p.CrossingValue > p.ViewportRect.Bottom)
                    {
                        p.CrossingValue = p.ViewportRect.Bottom;
                    }
                }
            };

            renderer.ShouldRenderLabel = (p, value, last) =>
                {
                    double pixelValue = Math.Round(value);
                    return pixelValue >= Math.Floor(p.ViewportRect.Left) && pixelValue <= Math.Ceiling(p.ViewportRect.Right);
                };

            return renderer;
        }
        /// <summary>
        /// Gets the scaled viewport value from an unscaled axis value.
        /// </summary>
        /// <param name="unscaledValue">The unscaled axis value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>The scaled viewport value.</returns>
        public override double GetScaledValue(double unscaledValue, ScalerParams p)
        {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            return this.ActualScaler.GetScaledValue(unscaledValue, p);
        }
        /// <summary>
        /// Gets the unscaled axis value from an scaled viewport value.
        /// </summary>
        /// <param name="scaledValue">The scaled viewport value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>The unscaled axis value.</returns>
        public override double GetUnscaledValue(double scaledValue, ScalerParams p)
        {






            return this.ActualScaler.GetUnscaledValue(scaledValue, p);
        }
        /// <summary>
        /// Get a list of scaled viewport values from a list of unscaled axis values.
        /// </summary>
        /// <param name="unscaledValues">The list of unscaled axis values.</param>
        /// <param name="p">Scaler parameters</param>
        public override void GetScaledValueList(IList<double> unscaledValues, ScalerParams p)
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            this.ActualScaler.GetScaledValueList(unscaledValues, p);
        }
        /// <summary>
        /// Gets a list of unscaled axis values from a list of scaled viewport values.
        /// </summary>
        /// <param name="scaledValues">A list containing the scaled viewport values to unscale.</param>
        /// <param name="p">Scaler parameters</param>
        public override void GetUnscaledValueList(IList<double> scaledValues, ScalerParams p)
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            this.ActualScaler.GetUnscaledValueList(scaledValues, p);
        }

        /// <summary>
        /// Creates the parameters for the rendering.
        /// </summary>
        /// <param name="viewportRect">The viewport rectangle.</param>
        /// <param name="windowRect">The window rectangle.</param>
        /// <returns>The rendering parameters.</returns>
        internal override NumericAxisRenderingParameters CreateRenderingParams(
            Rect viewportRect,
            Rect windowRect)
        {
            NumericAxisRenderingParameters renderingParams =
                base.CreateRenderingParams(viewportRect, windowRect);

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, IsInverted);

            double visibleMinimum = double.NaN;
            double visibleMaximum = double.NaN;

            // avoid rounding errors related to the maximum value.
            if (!this.IsInverted && windowRect.Right == 1.0)
            {
                visibleMaximum = this.ActualMaximumValue;
            }
            else if (this.IsInverted && windowRect.Left == 0.0)
            {
                visibleMinimum = this.ActualMaximumValue;
            }

            if (double.IsNaN(visibleMinimum))
            {
                visibleMinimum = GetUnscaledValue(viewportRect.Left, xParams);
            }
            if (double.IsNaN(visibleMaximum))
            {
                visibleMaximum = GetUnscaledValue(viewportRect.Right, xParams);
            }          

            double trueVisibleMinimum = Math.Min(visibleMinimum, visibleMaximum);
            double trueVisibleMaximum = Math.Max(visibleMinimum, visibleMaximum);

            renderingParams.RangeInfos.Add(
                new RangeInfo()
                {
                    VisibleMinimum = trueVisibleMinimum,
                    VisibleMaximum = trueVisibleMaximum,
                    Resolution = viewportRect.Width
                });

            return renderingParams;
        }

        /// <summary>
        /// Renders or updates the axis visuals.
        /// </summary>
        /// <param name="animate">Whether of not the visual changes should be animated.</param>
        protected override void RenderAxisOverride(bool animate)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = !windowRect.IsEmpty ? ViewportRect : Rect.Empty;

            Renderer.Render(animate, viewportRect, windowRect);
        }

        /// <summary>
        /// Overridden in derived classes if they want to respond to the viewport of the axis changing.
        /// </summary>
        /// <param name="oldRect">The old viewport rectangle.</param>
        /// <param name="newRect">The new viewport rectangle.</param>
        protected override void ViewportChangedOverride(Rect oldRect, Rect newRect)
        {
            base.ViewportChangedOverride(oldRect, newRect);
            if (newRect.Height != oldRect.Height)
            {
                // [DN 5/3/2010:29979] axis range is dependent on viewport size (see the bit about axisResolution in AutoRangeCalculator).  therefore, we should invoke UpdateRange when the ViewportRect has changed.
                this.UpdateRange();
            }
        }

        /// <summary>
        /// Gets the axis orientation.
        /// </summary>
        protected internal override AxisOrientation Orientation
        {
            get { return AxisOrientation.Horizontal; }
        }

        /// <summary>
        /// Creates a new axis scaler.
        /// </summary>
        /// <returns>New axis scaler</returns>
        protected internal override NumericScaler CreateScalerOverride()
        {
            if (this.IsLogarithmic) // IsLogarithmic takes precedence over ScaleMode for legacy/BC support
            {
                return new HorizontalLogarithmicScaler();
            }
            switch (this.ScaleMode)
            {
                case NumericScaleMode.Linear:
                    return new HorizontalLinearScaler();
                case NumericScaleMode.Logarithmic:
                    return new HorizontalLogarithmicScaler();
            }
            return null;
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