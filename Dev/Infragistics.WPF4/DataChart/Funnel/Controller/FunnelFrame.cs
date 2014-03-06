using System;

using System.Windows;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a visual frame for the funnel chart.
    /// </summary>
    internal class FunnelFrame
    {
        public FunnelFrame()
        {
            Slices = new SliceInfoList();
        }

        private bool _innerLabelsShown;
        public bool InnerLabelsShown
        {
            get { return _innerLabelsShown; }
            set { _innerLabelsShown = value; }
        }

        private bool _outerLabelsShown;
        public bool OuterLabelsShown
        {
            get { return _outerLabelsShown; }
            set { _outerLabelsShown = value; }
        }

        private bool _outerAlignedLeft;
        public bool OuterAlignedLeft
        {
            get { return _outerAlignedLeft; }
            set { _outerAlignedLeft = value; }
        }

        private SliceInfoList _slices;
        public SliceInfoList Slices
        {
            get { return _slices; }
            set { _slices = value; }
        }

        private double _outerLabelWidth;
        public double OuterLabelWidth
        {
            get { return _outerLabelWidth; }
            set { _outerLabelWidth = value; }
        }

        internal static void Interpolate(FunnelFrame interpolatedFrame, FunnelFrame previousFrame, FunnelFrame nextFrame, double p)
        {
            double q = 1.0 - p;

            interpolatedFrame.InnerLabelsShown = nextFrame.InnerLabelsShown;
            interpolatedFrame.OuterAlignedLeft = nextFrame.OuterAlignedLeft;
            interpolatedFrame.OuterLabelsShown = nextFrame.OuterLabelsShown;
            interpolatedFrame.OuterLabelWidth = (previousFrame.OuterLabelWidth * q) + (nextFrame.OuterLabelWidth * p);

            int minCount = previousFrame.Slices.Count;
            int maxCount = nextFrame.Slices.Count;
            int count = Math.Max(minCount, maxCount);

            if (interpolatedFrame.Slices.Count < count)
            {
                interpolatedFrame.Slices.InsertRange(interpolatedFrame.Slices.Count, new SliceInfo[count - interpolatedFrame.Slices.Count]);
            }

            if (interpolatedFrame.Slices.Count > count)
            {
                interpolatedFrame.Slices.RemoveRange(count, interpolatedFrame.Slices.Count - count);
            }

            for (int i = 0; i < Math.Min(minCount, maxCount); ++i)
            {
                interpolatedFrame.Slices[i] = SliceInfo.Interpolate(
                    interpolatedFrame.Slices[i],
                    previousFrame.Slices[i],
                    nextFrame.Slices[i], p, q);
            }
            
            if (minCount < maxCount)
            {
                SliceInfo mn = minCount > 0 ? previousFrame.Slices[minCount - 1] : new SliceInfo();

                for (int i = minCount; i < maxCount; ++i)
                {
                    interpolatedFrame.Slices[i] =
                        SliceInfo.Interpolate(
                        interpolatedFrame.Slices[i],
                        mn,
                        nextFrame.Slices[i],
                        p, q);
                }
            }

            if (minCount > maxCount)
            {
                SliceInfo mx = maxCount > 0 ? nextFrame.Slices[maxCount - 1] : new SliceInfo();

                for (int i = maxCount; i < minCount; ++i)
                {
                    interpolatedFrame.Slices[i] =
                        SliceInfo.Interpolate(
                        interpolatedFrame.Slices[i],
                        previousFrame.Slices[i],
                        mx,
                        p, q);
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