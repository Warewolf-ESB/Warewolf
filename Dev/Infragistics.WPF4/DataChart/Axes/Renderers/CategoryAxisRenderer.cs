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
    internal class CategoryAxisRenderingParameters
        : AxisRenderingParametersBase
    {
        public int Count { get; set; }
        public CategoryMode CategoryMode { get; set; }
        public bool WrapAround { get; set; }
        public int Mode2GroupCount { get; set; }
        public bool IsInverted { get; set; }
    }

    internal class CategoryAxisRenderer
        : AxisRendererBase
    {
        public CategoryAxisRenderer(AxisLabelManager labelManager)
            : base(labelManager)
        {
            
        }

        internal void GetSnapperInfoInternal(AxisRenderingParametersBase renderingParams,
            out double interval, out int minorCount, out int first, out int last)
        {
            var catParams = renderingParams as CategoryAxisRenderingParameters;
            CategoryMode mode = CategoryMode.Mode0;
            if (catParams != null)
            {
                mode = catParams.CategoryMode;
            }

            LinearCategorySnapper snapper = new LinearCategorySnapper(
                renderingParams.CurrentRangeInfo.VisibleMinimum,
                renderingParams.CurrentRangeInfo.VisibleMaximum, 
                renderingParams.CurrentRangeInfo.Resolution, 
                renderingParams.Interval, 
                mode);

            interval = snapper.Interval;
            if (renderingParams.CurrentRangeInfo.IntervalOverride != -1)
            {
                interval = renderingParams.CurrentRangeInfo.IntervalOverride;
            }

            double firstValue = Math.Floor((renderingParams.CurrentRangeInfo.VisibleMinimum - renderingParams.ActualMinimumValue) / interval);
            double lastValue = Math.Ceiling((renderingParams.CurrentRangeInfo.VisibleMaximum - renderingParams.ActualMinimumValue) / interval);

            first = (int)firstValue;
            last = (int)lastValue;

            minorCount = (int)snapper.MinorCount;
            if (renderingParams.CurrentRangeInfo.MinorCountOverride != -1)
            {
                minorCount = renderingParams.CurrentRangeInfo.MinorCountOverride;
            }
        }

        protected override object GetLabel(AxisRenderingParametersBase renderingParams, double unscaledValue, int index, double interval)
        {
            var catParams = renderingParams as CategoryAxisRenderingParameters;
            if (catParams == null)
            {
                return null;
            }

            int itemIndex = 0;
            if (interval >= 1)
            {
                itemIndex = index * (int)Math.Floor(interval);
            }
            else
            {
                if ((index * interval) * 2 % 2 == 0)
                {
                    itemIndex = (int)Math.Floor(index * interval);
                }
                else
                {
                    itemIndex = -1;
                }
            }

            object label = null;
            if ((catParams.Count > 0 && itemIndex < catParams.Count && itemIndex >= 0)
                || catParams.WrapAround)
            {
                while (itemIndex >= catParams.Count && catParams.WrapAround)
                {
                    itemIndex -= catParams.Count;
                }
                label = GetLabelForItem(itemIndex);
            }

            return label;
        }

        private void RenderMinorLines(
            AxisRenderingParametersBase renderingParams, 
            double interval, 
            int minorCount, 
            double majorValue, 
            int i, 
            double nextMajorValue)
        {
            var catParams = renderingParams as CategoryAxisRenderingParameters;
            if (catParams.CategoryMode != Charts.CategoryMode.Mode0 && catParams.Mode2GroupCount != 0)
            {
                for (int categoryNumber = 0; categoryNumber < (int)interval; categoryNumber++)
                {
                    //display a minor line in te middle of each group.
                    for (int groupNumber = 0; groupNumber < catParams.Mode2GroupCount; groupNumber++)
                    {
                        double center = GetGroupCenter(
                            groupNumber, 
                            renderingParams.WindowRect, 
                            renderingParams.ViewportRect);
                        if (catParams.IsInverted) center = -center;
                        double minorValue = Scaling(
                            renderingParams,
                            categoryNumber + i * interval) + center;
                        Line(renderingParams, renderingParams.Minor, minorValue);
                    }
                }
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