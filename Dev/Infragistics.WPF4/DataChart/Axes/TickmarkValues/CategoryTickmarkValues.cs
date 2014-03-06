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
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    internal class CategoryTickmarkValues : TickmarkValues
    {
        internal CategoryMode Mode { get; set; }
        internal int Mode2GroupCount { get; set; }
        internal Rect Viewport { get; set; }
        internal Rect Window { get; set; }
        internal bool IsInverted { get; set; }
        internal Func<int, double> GetUnscaledGroupCenter { get; set; }

        public override void Initialize(TickmarkValuesInitializationParameters initializationParameters)
        {
            base.Initialize(initializationParameters);

            var mode = initializationParameters.Mode;
            //var catParams = renderingParams as CategoryAxisRenderingParameters;
            //CategoryMode mode = CategoryMode.Mode0;
            //if (catParams != null)
            //{
            //    mode = catParams.CategoryMode;
            //}
            this.Mode = mode;
            this.Mode2GroupCount = initializationParameters.Mode2GroupCount;
            this.Viewport = initializationParameters.Viewport;
            this.Window = initializationParameters.Window;
            this.IsInverted = initializationParameters.IsInverted;
            this.GetUnscaledGroupCenter = initializationParameters.GetUnscaledGroupCenter;

            LinearCategorySnapper snapper = new LinearCategorySnapper(
                initializationParameters.VisibleMinimum,
                initializationParameters.VisibleMaximum,
                initializationParameters.Resolution,
                initializationParameters.UserInterval,
                mode);

            var interval = snapper.Interval;
            if (initializationParameters.IntervalOverride != -1)
            {
                interval = initializationParameters.IntervalOverride;
            }

            double firstValue = Math.Floor((initializationParameters.VisibleMinimum - initializationParameters.ActualMinimum) / interval);
            double lastValue = Math.Ceiling((initializationParameters.VisibleMaximum - initializationParameters.ActualMinimum) / interval);

            var first = (int)firstValue;
            var last = (int)lastValue;

            var minorCount = (int)snapper.MinorCount;
            if (initializationParameters.MinorCountOverride != -1)
            {
                minorCount = initializationParameters.MinorCountOverride;
            }

            this.Interval = interval;
            this.FirstIndex = first;
            this.LastIndex = last;
            this.MinorCount = minorCount;
            //this.Interval = double.IsNaN(initializationParameters.UserInterval) ? 1.0 : initializationParameters.UserInterval;

            //this.FirstIndex = (int)Math.Floor((initializationParameters.VisibleMinimum - initializationParameters.ActualMinimum) / this.Interval);
            //this.LastIndex = (int)Math.Ceiling((initializationParameters.VisibleMaximum - initializationParameters.ActualMinimum) / this.Interval);
            this.ActualMinimum = initializationParameters.ActualMinimum;
        }
        private double ActualMinimum { get; set; }



#region Infragistics Source Cleanup (Region)




































#endregion // Infragistics Source Cleanup (Region)

        public override IEnumerable<double> MajorValues()
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            decimal actualMinimumDecimal = Convert.ToDecimal(this.ActualMinimum);
            decimal intervalDecimal = Convert.ToDecimal(this.Interval);

            for (int i = this.FirstIndex; i <= this.LastIndex; ++i)
            {
                double major = Convert.ToDouble(actualMinimumDecimal + Convert.ToDecimal(i) * intervalDecimal);
                yield return major;
            }

        }

        public override IEnumerable<double> MinorValues()
        {
            //for (int i = this.FirstIndex; i < this.LastIndex; ++i)
            //{
            //    for (int j = 1; j < this.MinorCount; ++j)
            //    {
            //        double minor = this.ActualMinimum + i * this.Interval + (j * this.Interval / this.MinorCount);
            //        if (minor <= this.VisibleMaximum)
            //        {
            //            yield return minor;
            //        }
            //    }
            //}
            //avoid drawing too many minor lines
            var interval = Math.Min(this.Interval, 20.0);

            for (int i = this.FirstIndex; i < this.LastIndex; ++i)
            {
                if (Mode != Charts.CategoryMode.Mode0 && Mode2GroupCount != 0)
                {
                    for (int categoryNumber = 0; categoryNumber < (int)interval; categoryNumber++)
                    {
                        //display a minor line in te middle of each group.
                        for (int groupNumber = 0; groupNumber < Mode2GroupCount; groupNumber++)
                        {
                            double center = GetUnscaledGroupCenter(groupNumber);
                            //if (IsInverted) center = -center;
                            double minorValue = categoryNumber + (i * this.Interval) + center;
                            yield return minorValue;
                        }
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