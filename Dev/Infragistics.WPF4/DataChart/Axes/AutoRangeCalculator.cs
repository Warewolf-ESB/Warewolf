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
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    [WidgetIgnoreDepends("NumericAxisBase")]
    [WidgetIgnoreDepends("NumericRadiusAxis")]
    internal class AutoRangeCalculator
    {
        public static void CalculateRange(NumericAxisBase target, double userMinimum, double userMaximum,
            bool isLogarithmic, int logarithmBase, 
            out double minimumValue, out double maximumValue)
        {
            minimumValue = !double.IsNaN(userMinimum) && !double.IsInfinity(userMinimum) ? userMinimum : double.PositiveInfinity;
            maximumValue = !double.IsNaN(userMaximum) && !double.IsInfinity(userMaximum) ? userMaximum : double.NegativeInfinity;

            if (double.IsInfinity(minimumValue) || double.IsInfinity(maximumValue))
            {
                if (target != null)
                {
                    AxisRange axisRange = target.GetAxisRange();
                    if (axisRange != null)
                    {
                        minimumValue = Math.Min(minimumValue, axisRange.Minimum);
                        maximumValue = Math.Max(maximumValue, axisRange.Maximum);
                    }
                }
            }

            if (!double.IsInfinity(minimumValue) && !double.IsInfinity(maximumValue))
            {
                // take care of zero range

                if (minimumValue == maximumValue && minimumValue != 0)
                {
                    // make minimumValue lesser and maximumValue greater.
                    minimumValue *= minimumValue > 0.0 ? 0.9 : 1.1;
                    maximumValue *= maximumValue > 0.0 ? 1.1 : 0.9;
                }                    
                 
                if (minimumValue == maximumValue && minimumValue == 0)
                {
                    maximumValue = 1.0;
                }

                //make sure minimum is not greater than maximum
                //this can only happen if set explicitly by the user
                if (userMinimum > userMaximum)
                {
                    //swap min and max
                    double temp = userMaximum;
                    userMaximum = userMinimum;
                    userMinimum = temp;
                }

                // do some snapping

                double actualMinimum = double.IsNaN(userMinimum) || double.IsInfinity(userMinimum) ? minimumValue : userMinimum;
                double actualMaximum = double.IsNaN(userMaximum) || double.IsInfinity(userMaximum) ? maximumValue : userMaximum;

                if (isLogarithmic)
                {
                    if (actualMinimum <= 0)
                    {
                        if (actualMaximum > 1)
                        {
                            actualMinimum = 1; 
                        }
                        else
                        {
                            actualMinimum = Math.Pow(logarithmBase, Math.Floor(Math.Log(actualMaximum, logarithmBase)));
                        }
                    }

                    if (double.IsNaN(userMinimum) || double.IsInfinity(userMinimum))
                    {
                        minimumValue = Math.Pow(logarithmBase, Math.Floor(Math.Log(actualMinimum, logarithmBase)));
                    }
                    else
                    {
                        minimumValue = actualMinimum;
                    }

                    if (double.IsNaN(userMaximum) || double.IsInfinity(userMaximum))
                    {
                        maximumValue = Math.Pow(logarithmBase, Math.Ceiling(Math.Log(actualMaximum, logarithmBase)));
                    }
                    else
                    {
                        maximumValue = actualMaximum;
                    }
                }
                else
                {
                    double n = Math.Pow(10.0, Math.Floor(Math.Log10(actualMaximum - actualMinimum)) - 1.0);
                    
                    //this code segment uses a linear snapper to determine min and max values, so that the zero value is included in the range.
                    //also, this should only be used when axis range is not set by the end user.
                    NumericAxisBase targetAxis = target as NumericAxisBase;



                    double axisResolution = targetAxis.ActualWidth;

                    if (targetAxis is NumericYAxis)
                    {



                        axisResolution = targetAxis.ActualHeight;

                    }

                    if (targetAxis is NumericRadiusAxis && axisResolution > 0)
                    {
                        double radiusExtentScale = (targetAxis as NumericRadiusAxis).ActualRadiusExtentScale;
                        double innerRadiusExtentScale = (targetAxis as NumericRadiusAxis).ActualInnerRadiusExtentScale;
                        axisResolution = 
                            Math.Min(targetAxis.ActualWidth, targetAxis.ActualHeight) * 
                            (radiusExtentScale - innerRadiusExtentScale) / 
                            2.0;
                        
                        axisResolution = Math.Max(axisResolution, 14.0);
                    }


                    if (targetAxis != null && axisResolution > 0
                        && (!targetAxis.HasUserMinimum
                        && !targetAxis.HasUserMaximum))
                    {
                        LinearNumericSnapper snapper = new LinearNumericSnapper(minimumValue, maximumValue, axisResolution);
                        n = snapper.Interval;
                    }

                    if ((double.IsNaN(userMinimum) || double.IsInfinity(userMinimum)) && !double.IsNaN(minimumValue) && !double.IsNaN(n) && n != 0.0) 

                    {
                        // [DN June 20 2011 : 79057] use decimal math to avoid rounding errors

                        bool useDecimal = n <= XamDataChart.DecimalMinimumValueAsDouble;
                        if (useDecimal)
                        {
                            minimumValue = Convert.ToDouble(Convert.ToDecimal(n) * Convert.ToDecimal(Math.Floor(minimumValue / n)));
                        }
                        else

                        {
                            minimumValue = n * Math.Floor(minimumValue / n);
                        }
                    }
                    else
                    {
                        minimumValue = actualMinimum;
                    }

                    if ((double.IsNaN(userMaximum) || double.IsInfinity(userMaximum)) && !double.IsNaN(maximumValue) && !double.IsNaN(n) && n != 0.0

                        

                        )
                    {
                        // [DN June 20 2011 : 79057] use decimal math to avoid rounding errors
                        double ceilingOfQuotient = Math.Ceiling(maximumValue / n);


                        bool useDecimal = ceilingOfQuotient <= XamDataChart.DecimalMaximumValueAsDouble && n <= XamDataChart.DecimalMaximumValueAsDouble;
                        if (useDecimal)
                        {
                            maximumValue = Convert.ToDouble(Convert.ToDecimal(n) * Convert.ToDecimal(ceilingOfQuotient));
                        }
                        else

                        {
                            maximumValue = n * ceilingOfQuotient;
                        }
                    }
                    else
                    {
                        maximumValue = actualMaximum;
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