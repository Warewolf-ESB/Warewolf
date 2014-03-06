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



using System.Linq;


namespace Infragistics.Controls.Charts
{
    internal class AxisRendererBase
    {





        public AxisRendererBase(AxisLabelManager labelManager)
        {
            Clear = () => { };
            ShouldRender = (r1, r2) => { return false; };
            OnRendering = () => { };
            Scaling = (p, v) => { return v; };
            Strip = (p, g, min, max) => { };
            Line = (p, g, v) => { };
            ShouldRenderLines = (p, v) => { return false; };
            ShouldRenderContent = (p, v) => { return ShouldRenderLines(p, v); };
            AxisLine = (p) => { };
            DetermineCrossingValue = (p) => { };
            ShouldRenderLabel = (p, v, last) => { return false; };
            GetLabelLocation = (p, v) => { return new LabelPosition(v); };
            TransformToLabelValue = (p, v) => { return v; };
            GetLabelForItem = (item) => null;
            SnapMajorValue = (p, v, i, interval) => v;
            AdjustMajorValue = (p, v, i, interval) => v;
            LabelManager = labelManager;
            CreateRenderingParams = (r1, r2) => { return null; };
        }

        public Action Clear { get; set; }
        public Func<Rect, Rect, bool> ShouldRender { get; set; }
        public Action OnRendering { get; set; }
        public Func<AxisRenderingParametersBase, double, double> Scaling { get; set; }

        public Action<AxisRenderingParametersBase, GeometryCollection, double, double> Strip { get; set; }
        public Action<AxisRenderingParametersBase, GeometryCollection, double> Line { get; set; }
        public Func<AxisRenderingParametersBase, double, bool> ShouldRenderLines { get; set; }
        public Func<AxisRenderingParametersBase, double, bool> ShouldRenderContent { get; set; }
        public Action<AxisRenderingParametersBase> AxisLine { get; set; }
        public Action<AxisRenderingParametersBase> DetermineCrossingValue { get; set; }
        public Func<AxisRenderingParametersBase, double, bool, bool> ShouldRenderLabel { get; set; }
        public Func<AxisRenderingParametersBase, double, LabelPosition> GetLabelLocation { get; set; }
        public Func<AxisRenderingParametersBase, double, double> TransformToLabelValue { get; set; }
        public AxisLabelManager LabelManager { get; set; }
        public Func<object, object> GetLabelForItem { get; set; }

        
        public Func<Rect, Rect, AxisRenderingParametersBase> CreateRenderingParams { get; set; }
        public Func<AxisRenderingParametersBase, double, int, double, double> SnapMajorValue { get; set; }
        public Func<AxisRenderingParametersBase, double, int, double, double> AdjustMajorValue { get; set; }
        public Func<int, Rect, Rect, double> GetGroupCenter { get; set; }
        public Func<int, double> GetUnscaledGroupCenter { get; set; }

        public void Render(bool animate, Rect viewportRect, Rect windowRect)
        {
            ClearLabels(windowRect, viewportRect);

            if (ShouldRender(viewportRect, windowRect))
            {
                OnRendering();
                AxisRenderingParametersBase renderingParams =
                    CreateRenderingParams(viewportRect, windowRect);

                ClearLabels(windowRect, viewportRect);

                if (renderingParams == null)
                {
                    ResetLabels();
                    return;
                }



                if (renderingParams.RangeInfos.Count > 1
                    && !renderingParams.HasUserInterval)
                {
                    SpreadInterval(renderingParams);
                }


                foreach (RangeInfo range in renderingParams.RangeInfos)
                {
                    renderingParams.CurrentRangeInfo = range;

                    if (double.IsNaN(range.VisibleMaximum) ||
                        double.IsInfinity(range.VisibleMaximum) ||
                        double.IsNaN(range.VisibleMinimum) ||
                        double.IsInfinity(range.VisibleMinimum))
                    {
                        continue;
                    }

                    //also return if min and max are the same
                    if (range.VisibleMinimum == range.VisibleMaximum)
                    {
                        continue;
                    }

                    DetermineCrossingValue(renderingParams);
                    LabelManager.FloatPanel(renderingParams.CrossingValue);

                    CategoryMode mode = CategoryMode.Mode0;
                    int mode2GroupCount = 0;
                    bool isInverted = false;
                    Func<int, double> getUnscaledGroupCenter = (n) => n;
                    if (GetGroupCenter != null)
                    {
                        getUnscaledGroupCenter = GetUnscaledGroupCenter;
                    }
                    if (renderingParams is CategoryAxisRenderingParameters)
                    {
                        mode = ((CategoryAxisRenderingParameters)renderingParams).CategoryMode;
                        mode2GroupCount = ((CategoryAxisRenderingParameters)renderingParams).Mode2GroupCount;
                        isInverted = ((CategoryAxisRenderingParameters)renderingParams).IsInverted;
                    }

                    renderingParams.TickmarkValues = this.GetTickmarkValues(renderingParams);
                    renderingParams.TickmarkValues.Initialize(new TickmarkValuesInitializationParameters()
                    {
                        VisibleMinimum = renderingParams.CurrentRangeInfo.VisibleMinimum,
                        VisibleMaximum = renderingParams.CurrentRangeInfo.VisibleMaximum,
                        ActualMinimum = renderingParams.ActualMinimumValue,
                        ActualMaximum = renderingParams.ActualMaximumValue,
                        Resolution = renderingParams.CurrentRangeInfo.Resolution,
                        HasUserInterval = renderingParams.HasUserInterval,
                        UserInterval = renderingParams.Interval,
                        IntervalOverride = renderingParams.CurrentRangeInfo.IntervalOverride,
                        MinorCountOverride = renderingParams.CurrentRangeInfo.MinorCountOverride,
                        Mode = mode,
                        Mode2GroupCount = mode2GroupCount,
                        Window = renderingParams.WindowRect,
                        Viewport = renderingParams.ViewportRect,
                        IsInverted = isInverted,
                        GetUnscaledGroupCenter = getUnscaledGroupCenter
                    });

                    this.RenderInternal(renderingParams);
                }

                RenderLabels();
            }
        }

        private void ResetLabels()
        {
            LabelManager.ResetLabels();
        }

        private void SpreadInterval(AxisRenderingParametersBase renderingParams)
        {
            double maxInterval = double.MinValue;
            int maxMinorCount = int.MinValue;

            CategoryMode mode = CategoryMode.Mode0;
            int mode2GroupCount = 0;
            bool isInverted = false;
            Func<int, double> getUnscaledGroupCenter = (n) => n;
            if (GetGroupCenter != null)
            {
                getUnscaledGroupCenter = GetUnscaledGroupCenter;
            }
            if (renderingParams is CategoryAxisRenderingParameters)
            {
                mode = ((CategoryAxisRenderingParameters)renderingParams).CategoryMode;
                mode2GroupCount = ((CategoryAxisRenderingParameters)renderingParams).Mode2GroupCount;
                isInverted = ((CategoryAxisRenderingParameters)renderingParams).IsInverted;
            }

            foreach (var rangeInfo in renderingParams.RangeInfos)
            {
                renderingParams.CurrentRangeInfo = rangeInfo;
                renderingParams.TickmarkValues.Initialize(new TickmarkValuesInitializationParameters()
                {
                    VisibleMinimum = rangeInfo.VisibleMinimum,
                    VisibleMaximum = rangeInfo.VisibleMaximum,
                    ActualMinimum = renderingParams.ActualMinimumValue,
                    ActualMaximum = renderingParams.ActualMaximumValue,
                    Resolution = rangeInfo.Resolution,
                    HasUserInterval = renderingParams.HasUserInterval,
                    UserInterval = renderingParams.Interval,
                    IntervalOverride = rangeInfo.IntervalOverride,
                    MinorCountOverride = rangeInfo.MinorCountOverride,
                    Mode = mode,
                    Mode2GroupCount = mode2GroupCount,
                    Window = renderingParams.WindowRect,
                    Viewport = renderingParams.ViewportRect,
                    IsInverted = isInverted,
                    GetUnscaledGroupCenter = getUnscaledGroupCenter
                });
                rangeInfo.IntervalOverride = renderingParams.TickmarkValues.Interval;
                rangeInfo.MinorCountOverride = renderingParams.TickmarkValues.MinorCount;
                if (!double.IsNaN(renderingParams.TickmarkValues.Interval))
                {
                    maxInterval = Math.Max(maxInterval, renderingParams.TickmarkValues.Interval);
                    maxMinorCount = Math.Max(maxMinorCount, renderingParams.TickmarkValues.MinorCount);
                }
            }

            foreach (var rangeInfo in renderingParams.RangeInfos)
            {
                if (rangeInfo.IntervalOverride == maxInterval)
                {
                    rangeInfo.IntervalOverride = -1;
                    rangeInfo.MinorCountOverride = -1;
                }
                else
                {
                    rangeInfo.IntervalOverride = maxInterval;
                    rangeInfo.MinorCountOverride = maxMinorCount;
                }
            }
        }

        protected void ClearLabels(Rect windowRect, Rect viewportRect)
        {
            Clear();
            LabelManager.Clear(windowRect, viewportRect);
            LabelManager.UpdateLabelPanel();
        }

        private void RenderLabels()
        {
            LabelManager.UpdateLabelPanel();

            if (LabelManager.LabelsHidden)
            {
                LabelManager.SetTextBlockCount(0);
            }
            else
            {
                int textBlockCount = 0;
                foreach (object labelObj in LabelManager.LabelDataContext)
                {
                    FrameworkElement label = labelObj as FrameworkElement;
                    if (label == null)
                    {
                        label = LabelManager.GetTextBlock(textBlockCount);



                        label.SetValue(TextBlock.TextProperty, labelObj);

                        textBlockCount++;
                    }
                    else
                    {
                        LabelManager.AddLabel(label);
                    }
                }
                LabelManager.SetTextBlockCount(textBlockCount);
            }
        }
        private TickmarkValues GetTickmarkValues(AxisRenderingParametersBase renderingParams)
        {
            LogarithmicTickmarkValues logTicks = renderingParams.TickmarkValues as LogarithmicTickmarkValues;
            if (logTicks != null)
            {
                double trueVisibleMinimum = Math.Min(renderingParams.CurrentRangeInfo.VisibleMinimum, renderingParams.CurrentRangeInfo.VisibleMaximum);
                double trueVisibleMaximum = Math.Max(renderingParams.CurrentRangeInfo.VisibleMinimum, renderingParams.CurrentRangeInfo.VisibleMaximum);
                double logMin = Math.Floor(Math.Log(trueVisibleMinimum, logTicks.LogarithmBase));
                double logMax = Math.Ceiling(Math.Log(trueVisibleMaximum, logTicks.LogarithmBase));
                if (logMax - logMin < 2.0)
                {
                    return new LinearTickmarkValues();
                }
            }
            return renderingParams.TickmarkValues;
        }
        private void RenderInternal(AxisRenderingParametersBase renderingParams)
        {




            double[] majorTicks = renderingParams.TickmarkValues.MajorValues().ToArray();
            double[] minorTicks = renderingParams.TickmarkValues.MinorValues().ToArray();


            this.LabelManager.SetLabelInterval(this.Scaling(renderingParams, renderingParams.TickmarkValues.Interval));
            this.AxisLine(renderingParams);

            for (int maj = 0; maj < majorTicks.Length; maj++)
            {
                int absoluteIndex = renderingParams.TickmarkValues.FirstIndex + maj;
                double majorTick = majorTicks[maj];

                double unscaledValue = majorTick;
                double nextUnscaledValue = 0;
                if (maj < majorTicks.Length - 1)
                {
                    nextUnscaledValue = majorTicks[maj + 1];
                }
                else
                {
                    nextUnscaledValue = double.PositiveInfinity;
                }

                unscaledValue = this.SnapMajorValue(renderingParams, unscaledValue, absoluteIndex, renderingParams.TickmarkValues.Interval);
                nextUnscaledValue = this.SnapMajorValue(renderingParams, nextUnscaledValue, absoluteIndex, renderingParams.TickmarkValues.Interval);

                double majorValue = this.Scaling(renderingParams, unscaledValue);
                double nextMajorValue = this.Scaling(renderingParams, nextUnscaledValue);

                if (this.ShouldRenderLines(renderingParams, majorValue))
                {
                    if (absoluteIndex % 2 == 0 && this.ShouldRenderContent(renderingParams, nextMajorValue) && !double.IsInfinity(nextMajorValue))
                    {
                        this.Strip(renderingParams, renderingParams.Strips, majorValue, nextMajorValue);
                    }

                    this.Line(renderingParams, renderingParams.Major, majorValue);
                }

                majorValue = this.AdjustMajorValue(renderingParams, majorValue, absoluteIndex, renderingParams.TickmarkValues.Interval);

                if (!double.IsNaN(majorValue) && !double.IsInfinity(majorValue) && this.ShouldRenderLabel(renderingParams, majorValue, maj == majorTicks.Length - 1))
                {
                    object label = this.GetLabel(renderingParams, unscaledValue, absoluteIndex, renderingParams.TickmarkValues.Interval);

                    if (label != null)
                    {
                        this.LabelManager.AddLabelObject(label, this.GetLabelLocation(renderingParams, majorValue));
                    }
                }
            }
            if (renderingParams.ShouldRenderMinorLines)
            {
                for (int min = 0; min < minorTicks.Length; min++)
                {
                    double minorTick = minorTicks[min];
                    double minorValue = this.Scaling(renderingParams, minorTick);
                    this.Line(renderingParams, renderingParams.Minor, minorValue);
                }
            }
        }
        protected virtual object GetLabel(AxisRenderingParametersBase renderingParams, double unscaledValue, int index, double interval)
        {
            return null;
        }


    }

    internal delegate void GetSnapperInfoStrategy(AxisRenderingParametersBase renderingParams,
            out double interval,
            out int minorCount,
            out int first,
            out int last);
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