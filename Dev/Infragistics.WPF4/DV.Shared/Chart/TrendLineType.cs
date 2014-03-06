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
    ///// <summary>
    ///// Provides classifications for trendline types.
    ///// </summary>
    //public static class TrendLineTypeUtil
    //{
    //    /// <summary>
    //    /// Returns true if the given trendline type is a fit trenline.
    //    /// </summary>
    //    /// <param name="trendLineType">The trendline type to examine.</param>
    //    /// <returns>True if the trendline type represents a fit trendline.</returns>
    //    public static bool IsFit(this TrendLineType trendLineType)
    //    {
    //        return trendLineType == TrendLineType.LinearFit ||
    //                trendLineType == TrendLineType.QuadraticFit ||
    //                trendLineType == TrendLineType.CubicFit ||
    //                trendLineType == TrendLineType.QuarticFit ||
    //                trendLineType == TrendLineType.QuinticFit ||
    //                trendLineType == TrendLineType.LogarithmicFit ||
    //                trendLineType == TrendLineType.ExponentialFit ||
    //                trendLineType == TrendLineType.PowerLawFit;
    //    }

    //    /// <summary>
    //    /// Returns true if the given trendline type is an average trendline.
    //    /// </summary>
    //    /// <param name="trendLineType">The trendline type to examine.</param>
    //    /// <returns>True if the trendline type represents an average trendline.</returns>
    //    public static bool IsAverage(this TrendLineType trendLineType)
    //    {
    //        return trendLineType == TrendLineType.SimpleAverage ||
    //                trendLineType == TrendLineType.ExponentialAverage ||
    //                trendLineType == TrendLineType.ModifiedAverage ||
    //                trendLineType == TrendLineType.CumulativeAverage;
    //    }
    //}

    /// <summary>
    /// Enumerates the possible types of trendlines supported by the chart.
    /// </summary>
    public enum TrendLineType
    {
        /// <summary>
        /// No trendline should display.
        /// </summary>
        None,

        /// <summary>
        /// Linear fit.
        /// </summary>
        LinearFit,

        /// <summary>
        /// Quadratic polynomial fit.
        /// </summary>
        QuadraticFit,

        /// <summary>
        /// Cubic polynomial fit.
        /// </summary>
        CubicFit,

        /// <summary>
        /// Quartic polynomial fit.
        /// </summary>
        QuarticFit,

        /// <summary>
        /// Quintic polynomial fit.
        /// </summary>
        QuinticFit,

        /// <summary>
        /// Logarithmic fit.
        /// </summary>
        LogarithmicFit,

        /// <summary>
        /// Exponential fit.
        /// </summary>
        ExponentialFit,

        /// <summary>
        /// Powerlaw fit.
        /// </summary>
        PowerLawFit,

        /// <summary>
        /// Simple moving average.
        /// </summary>
        SimpleAverage,

        /// <summary>
        /// Exponential moving average.
        /// </summary>
        ExponentialAverage,

        /// <summary>
        /// Modified moving average.
        /// </summary>
        ModifiedAverage,

        /// <summary>
        /// Cumulative moving average.
        /// </summary>
        CumulativeAverage,

        /// <summary>
        /// Weighted moving average.
        /// </summary>
        WeightedAverage
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