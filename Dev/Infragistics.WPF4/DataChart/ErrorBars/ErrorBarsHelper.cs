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
    /// <summary>
    /// Implements some common methods for all error bars
    /// </summary>
    internal class ErrorBarsHelper
    {
        #region Constructor
        public ErrorBarsHelper(ISupportsErrorBars errorBarsHost, IProvidesViewport viewportHost)
        {
            this.ErrorBarsHost = errorBarsHost;
            this.ViewportHost = viewportHost;
        }
        #endregion //Constructor

        #region Properties
        private ISupportsErrorBars ErrorBarsHost { get; set; }
        private IProvidesViewport ViewportHost { get; set; }
        #endregion //Properties

        #region Methods

        internal bool IsCalculatorIndependent(IErrorBarCalculator calculator)
        {
            ErrorBarCalculatorType type = calculator.GetCalculatorType();
            if (type == ErrorBarCalculatorType.Percentage ||
                type == ErrorBarCalculatorType.Data)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal void CalculateIndependentErrorBarPosition(IErrorBarCalculator calculator, ref double position)
        {
            if (calculator.HasConstantPosition())
            {
                position = calculator.GetPosition();
            }
        }

        internal void CalculateIndependentErrorBarSize(IErrorBarCalculator calculator, NumericAxisBase axis, ref double errorBarSize)
        {
            Rect windowRect;
            Rect viewportRect;
            this.ViewportHost.GetViewInfo(out viewportRect, out windowRect);
            ScalerParams sParams = new ScalerParams(windowRect, viewportRect, axis.IsInverted);

            double zero = axis.GetScaledValue(axis.ReferenceValue, sParams);

            double errorBarValue = calculator.GetIndependentValue();

            errorBarSize = Math.Abs(Math.Round(zero - axis.GetScaledValue(errorBarValue, sParams)));
        }

        internal void CalculateDependentErrorBarSize(double value, IErrorBarCalculator calculator, NumericAxisBase axis, ref double errorBarSize)
        {
            Rect windowRect;
            Rect viewportRect;
            this.ViewportHost.GetViewInfo(out viewportRect, out windowRect);
            ScalerParams sParams = new ScalerParams(windowRect, viewportRect, axis.IsInverted);

            double unscaledValue = axis.GetUnscaledValue(value, sParams);
            double errorBarValue = calculator.GetDependentValue(unscaledValue);
            double zero = axis.GetScaledValue(axis.ReferenceValue, sParams);

            errorBarSize = Math.Abs(Math.Round(zero - axis.GetScaledValue(errorBarValue, sParams)));
        }

        internal void CalculateDependentErrorBarSize(double value, IErrorBarCalculator calculator, NumericAxisBase refAxis, NumericAxisBase targetAxis, ref double errorBarSize)
        {
            Rect windowRect;
            Rect viewportRect;
            this.ViewportHost.GetViewInfo(out viewportRect, out windowRect);
            ScalerParams refParams = new ScalerParams(windowRect, viewportRect, refAxis.IsInverted);
            ScalerParams targetParams = new ScalerParams(windowRect, viewportRect, targetAxis.IsInverted);

            double unscaledValue = refAxis.GetUnscaledValue(value, refParams);
            double errorBarValue = calculator.GetDependentValue(unscaledValue);
            double zero = targetAxis.GetScaledValue(targetAxis.ReferenceValue, targetParams);

            errorBarSize = Math.Abs(Math.Round(zero - targetAxis.GetScaledValue(errorBarValue, targetParams)));
        }

        internal void CalculateErrorBarSize(double unscaledErrorBarValue, NumericAxisBase axis, ref double errorBarSize)
        {
            Rect windowRect;
            Rect viewportRect;
            this.ViewportHost.GetViewInfo(out viewportRect, out windowRect);
            ScalerParams sParams = new ScalerParams(windowRect, viewportRect, axis.IsInverted);

            double zero = axis.GetScaledValue(axis.ReferenceValue, sParams);
            errorBarSize = Math.Abs(Math.Round(zero - axis.GetScaledValue(unscaledErrorBarValue, sParams)));
        }

        internal void AddErrorBarVertical(PathGeometry errorBarsGeometry, Point position, double errorBarLength, bool positive)
        {

            if (double.IsNaN(position.X) || double.IsNaN(position.Y) || double.IsNaN(errorBarLength))
            {
                return;
            }

            double endY = positive ? position.Y - errorBarLength : position.Y + errorBarLength;

            int capLength = (int)CategoryErrorBarSettings.ErrorBarCapLengthProperty.GetMetadata(typeof(CategoryErrorBarSettings)).DefaultValue;
            if (this.ErrorBarsHost.ErrorBarSettings is CategoryErrorBarSettings)
            {
                capLength = ((CategoryErrorBarSettings)this.ErrorBarsHost.ErrorBarSettings).ErrorBarCapLength;
            }
            else if (this.ErrorBarsHost.ErrorBarSettings is ScatterErrorBarSettings)
            {
                capLength = ((ScatterErrorBarSettings)this.ErrorBarsHost.ErrorBarSettings).VerticalErrorBarCapLength;
            }

            PathFigure errorBar = new PathFigure();
            errorBar.StartPoint = new Point() { X = position.X, Y = position.Y };
            errorBar.Segments.Add(new LineSegment() { Point = new Point(position.X, endY) });

            PathFigure errorCap = new PathFigure();
            errorCap.StartPoint = new Point() { X = position.X - (capLength / 2), Y = endY };
            errorCap.Segments.Add(new LineSegment() { Point = new Point(position.X + (capLength / 2), endY) });

            errorBarsGeometry.Figures.Add(errorBar);
            errorBarsGeometry.Figures.Add(errorCap);

        }

        internal void AddErrorBarHorizontal(PathGeometry errorBarsGeomety, Point position, double errorBarLength, bool positive)
        {

            if (double.IsNaN(position.X) || double.IsNaN(position.Y) || double.IsNaN(errorBarLength))
            {
                return;
            }

            double endX = positive ? position.X + errorBarLength : position.X - errorBarLength;

            int capLength = (int)CategoryErrorBarSettings.ErrorBarCapLengthProperty.GetMetadata(typeof(CategoryErrorBarSettings)).DefaultValue;
            if (this.ErrorBarsHost.ErrorBarSettings is CategoryErrorBarSettings)
            {
                capLength = ((CategoryErrorBarSettings)this.ErrorBarsHost.ErrorBarSettings).ErrorBarCapLength;
            }
            else if (this.ErrorBarsHost.ErrorBarSettings is ScatterErrorBarSettings)
            {
                capLength = ((ScatterErrorBarSettings)this.ErrorBarsHost.ErrorBarSettings).HorizontalErrorBarCapLength;
            }

            PathFigure errorBar = new PathFigure();
            errorBar.StartPoint = new Point() { X = position.X, Y = position.Y };
            errorBar.Segments.Add(new LineSegment() { Point = new Point(endX, position.Y) });

            PathFigure errorCap = new PathFigure();
            errorCap.StartPoint = new Point() { X = endX, Y = position.Y - (capLength / 2) };
            errorCap.Segments.Add(new LineSegment() { Point = new Point(endX, position.Y + (capLength / 2)) });

            errorBarsGeomety.Figures.Add(errorBar);
            errorBarsGeomety.Figures.Add(errorCap);

        }

        internal Point CalculateErrorBarCenterHorizontal(IErrorBarCalculator calculator, NumericAxisBase axis, Point point, double mean)
        {
            Point center = new Point();
            if (calculator.GetCalculatorType() == ErrorBarCalculatorType.StandardDeviation)
            {
                Rect windowRect;
                Rect viewportRect;
                this.ViewportHost.GetViewInfo(out viewportRect, out windowRect);
                ScalerParams sParams = new ScalerParams(windowRect, viewportRect, axis.IsInverted);

                center.X = Math.Round(axis.GetScaledValue(mean, sParams));
                center.Y = Math.Round(point.Y);
            }
            else
            {
                center.X = Math.Round(point.X);
                center.Y = Math.Round(point.Y);
            }
            return center;
        }

        internal Point CalculateErrorBarCenterVertical(IErrorBarCalculator calculator, NumericAxisBase axis, Point point, double mean)
        {
            Point center = new Point();
            if (calculator.GetCalculatorType() == ErrorBarCalculatorType.StandardDeviation)
            {
                Rect windowRect;
                Rect viewportRect;
                this.ViewportHost.GetViewInfo(out viewportRect, out windowRect);
                ScalerParams sParams = new ScalerParams(windowRect, viewportRect, axis.IsInverted);
                center.X = Math.Round(point.X);
                center.Y = Math.Round(axis.GetScaledValue(mean, sParams));
            }
            else
            {
                center.X = Math.Round(point.X);
                center.Y = Math.Round(point.Y);
            }
            return center;
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