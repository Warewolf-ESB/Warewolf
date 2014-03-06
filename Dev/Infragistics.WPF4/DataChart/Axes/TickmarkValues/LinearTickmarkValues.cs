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
    /// <summary>
    /// Represents a set of linear tickmarks.
    /// </summary>
    public class LinearTickmarkValues : TickmarkValues
    {
        /// <summary>
        /// Initializes a new instance of the tickmarks.
        /// </summary>
        public LinearTickmarkValues()
        {
            MinTicks = 0;
        }

        /// <summary>
        /// Gets or sets the number of minor tickmarks
        /// </summary>
        protected internal int MinTicks { get; set; }

        /// <summary>
        /// Initializes a set of new tickmark values.
        /// </summary>
        /// <param name="initializationParameters">Initialization parameters</param>
        public override void Initialize(TickmarkValuesInitializationParameters initializationParameters)
        {
            base.Initialize(initializationParameters);
            LinearNumericSnapper snapper;

            if (this.MinTicks != 0)
            {
                snapper = new LinearNumericSnapper(initializationParameters.VisibleMinimum, initializationParameters.VisibleMaximum, initializationParameters.Resolution, this.MinTicks);
            }
            else
            {
                snapper = new LinearNumericSnapper(initializationParameters.VisibleMinimum, initializationParameters.VisibleMaximum, initializationParameters.Resolution);
            }

            this.Interval = snapper.Interval;

            if ((initializationParameters.HasUserInterval) && initializationParameters.UserInterval > 0 && (initializationParameters.VisibleMaximum - initializationParameters.VisibleMinimum) / initializationParameters.UserInterval < 1000)
            {
                this.Interval = initializationParameters.UserInterval;
            }
            if (initializationParameters.IntervalOverride != -1)
            {
                this.Interval = initializationParameters.IntervalOverride;
            }

            this.FirstIndex = (int)Math.Floor((initializationParameters.VisibleMinimum - initializationParameters.ActualMinimum) / this.Interval);
            this.LastIndex = (int)Math.Ceiling((initializationParameters.VisibleMaximum - initializationParameters.ActualMinimum) / this.Interval);
            this.MinorCount = snapper.MinorCount;
            if (initializationParameters.MinorCountOverride != -1)
            {
                this.MinorCount = initializationParameters.MinorCountOverride;
            }
            this.ActualMinimum = initializationParameters.ActualMinimum;
        }
        private double ActualMinimum { get; set; }



#region Infragistics Source Cleanup (Region)









































#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Returns a collection of marjor tickmark values.
        /// </summary>
        /// <returns>Major tickmark values</returns>
        public override IEnumerable<double> MajorValues()
        {
            if (double.IsNaN(this.Interval))
            {
                yield break;
            }


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            bool useDecimal = (this.ActualMinimum > XamDataChart.DecimalMinimumValueAsDouble) 
                && (this.ActualMinimum < XamDataChart.DecimalMaximumValueAsDouble) 
                && (this.Interval > XamDataChart.DecimalMinimumValueAsDouble) 
                && (this.Interval < XamDataChart.DecimalMaximumValueAsDouble);
            decimal actualMinimumDecimal;
            decimal intervalDecimal;
            if (useDecimal)
            {
                actualMinimumDecimal = Convert.ToDecimal(this.ActualMinimum);
                intervalDecimal = Convert.ToDecimal(this.Interval);
            }
            else
            {
                actualMinimumDecimal = decimal.MinValue;
                intervalDecimal = decimal.MinValue;
            }
            for (int i = this.FirstIndex; i <= this.LastIndex; ++i)
            {
                double major;
                if (useDecimal)
                {
                    major = Convert.ToDouble(actualMinimumDecimal + Convert.ToDecimal(i) * intervalDecimal);
                }
                else
                {
                    major = this.ActualMinimum + i * this.Interval;
                }
                yield return major;
            }

        }

        /// <summary>
        /// Returns a collection of minor tickmark values.
        /// </summary>
        /// <returns>Minor tickmark values</returns>
        public override IEnumerable<double> MinorValues()
        {
            for (int i = this.FirstIndex; i < this.LastIndex; ++i)
            {
                for (int j = 1; j < this.MinorCount; ++j)
                {
                    double minor = this.ActualMinimum + i * this.Interval + (j * this.Interval / this.MinorCount);
                    if (minor <= this.VisibleMaximum)
                    {
                        yield return minor;
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