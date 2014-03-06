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
    /// Represents a set of logarithmic tickmarks.
    /// </summary>
    public class LogarithmicTickmarkValues : TickmarkValues
    {



        private const double MINIMUM_VALUE_GREATER_THAN_ZERO = double.Epsilon;


        /// <summary>
        /// Initializes a new set of tickmark values.
        /// </summary>
        /// <param name="initializationParameters">initialization parameters</param>
        public override void Initialize(TickmarkValuesInitializationParameters initializationParameters)
        {
            base.Initialize(initializationParameters);
            LogarithmicNumericSnapper snapper = new LogarithmicNumericSnapper(initializationParameters.VisibleMinimum, initializationParameters.VisibleMaximum, this.LogarithmBase, initializationParameters.Resolution);
            this.Interval = 1.0;
            this.MinorCount = (int)snapper.MinorCount;

            this.FirstIndex = (int)Math.Floor(Math.Log(Math.Max(MINIMUM_VALUE_GREATER_THAN_ZERO, initializationParameters.VisibleMinimum), this.LogarithmBase));
            this.LastIndex = (int)Math.Ceiling(Math.Log(Math.Max(MINIMUM_VALUE_GREATER_THAN_ZERO, initializationParameters.VisibleMaximum), this.LogarithmBase));
        }

        private const string LogarithmBasePropertyName = "LogarithmBase";

        /// <summary>
        /// Identifies the LogarithBase dependency property.
        /// </summary>
        public static readonly DependencyProperty LogarithmBaseProperty = DependencyProperty.Register(LogarithmBasePropertyName, typeof(int), typeof(LogarithmicTickmarkValues), new PropertyMetadata(10, (sender, e) =>
            {
                
            }));

        /// <summary>
        /// Gets or sets the logarithm base.
        /// </summary>
        public int LogarithmBase
        {
            get
            {
                return (int)this.GetValue(LogarithmBaseProperty);
            }
            set
            {
                this.SetValue(LogarithmBaseProperty, value);
            }
        }
    
        /// <summary>
        /// Returns a major value at a specified index.
        /// </summary>
        /// <param name="tickIndex">tickmark index</param>
        /// <returns>Major value at a given index</returns>
        private double MajorValueAt(int tickIndex)
        {
            double majorLog = tickIndex * this.Interval;
            return Math.Pow(this.LogarithmBase, majorLog);
        }



#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Returns a collection of major values.
        /// </summary>
        /// <returns>Major values</returns>
        public override IEnumerable<double> MajorValues()
        {
            for (int i = this.FirstIndex; i <= this.LastIndex; ++i)
            {
                double major = this.MajorValueAt(i);
                if (major <= this.VisibleMaximum)
                {
                    yield return major;
                }
            }
        }

        /// <summary>
        /// Returns a collection of minor values.
        /// </summary>
        /// <returns>Minor values</returns>
        public override IEnumerable<double> MinorValues()
        {
            for (int i = this.FirstIndex; i <= this.LastIndex; ++i)
            {
                double majorValue = this.MajorValueAt(i);
                double minorInterval = Math.Pow(this.LogarithmBase, i);
                for (int j = 1; j < this.MinorCount - 1; ++j)
                {
                    double minor = majorValue + j * minorInterval;
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