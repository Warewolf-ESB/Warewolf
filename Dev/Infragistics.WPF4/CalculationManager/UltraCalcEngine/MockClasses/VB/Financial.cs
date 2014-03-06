using System;


using ExcelResources = SR;


namespace Infragistics
{
    internal static class Financial
    {
        #region Public Static Methods
        public static double IPmt(double rate, double per, double nPer, double pv, double fv, DueDate dueDate)
        {
            double num = dueDate != DueDate.EndOfPeriod ? 2.0 : 1.0;
            if ((per <= 0.0) || (per >= (nPer + 1.0)))
            {
                throw new ArgumentOutOfRangeException("per", ExcelResources.GetString("LE_ArgumentOutOfRangeException_Per"));
            }

            if ((dueDate != DueDate.EndOfPeriod) && (per == 1.0))
            {
                return 0.0;
            }

            double pmt = PmtInternal(rate, nPer, pv, fv, dueDate);
            if (dueDate != DueDate.EndOfPeriod)
            {
                pv += pmt;
            }

            return FvInternal(rate, per - num, pmt, pv, DueDate.EndOfPeriod) * rate;
        }

        public static double PPmt(double rate, double per, double nper, double pv, double fv, DueDate dueDate)
        {
            if ((per <= 0.0) || (per >= (nper + 1.0)))
            {
                throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_Per"), "per");
            }

            double num2 = PmtInternal(rate, nper, pv, fv, dueDate);
            double num = IPmt(rate, per, nper, pv, fv, dueDate);
            return num2 - num;
        }

        public static double NPer(double rate, double pmt, double pv, double fv, DueDate dueDate)
        {
            double num;
            if (rate <= -1.0)
            {
                throw new ArgumentOutOfRangeException("rate", ExcelResources.GetString("LE_ArgumentOutOfRangeException_Rate"));
            }

            if (rate == 0.0)
            {
                if (pmt == 0.0)
                {
                    throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_Pmt"), "pmt");
                }

                return -(pv + fv) / pmt;
            }

            if (dueDate != DueDate.EndOfPeriod)
            {
                num = (pmt * (1.0 + rate)) / rate;
            }
            else
            {
                num = pmt / rate;
            }

            double d = -fv + num;
            double num4 = pv + num;

            if ((d < 0.0) && (num4 < 0.0))
            {
                d = -1.0 * d;
                num4 = -1.0 * num4;
            }
            else if ((d <= 0.0) || (num4 <= 0.0))
            {
                throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_NPerFunction"));
            }

            double num2 = rate + 1.0;
            return (Math.Log(d) - Math.Log(num4)) / Math.Log(num2);
        }
        #endregion Public Static Methods

        #region Private Static Methods
        private static double PmtInternal(double rate, double nPer, double pv, double fv, DueDate due)
        {
            double num;
            if (nPer == 0.0)
            {
                throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_NPer"), "nPer");
            }

            if (rate == 0.0)
            {
                return (-fv - pv) / nPer;
            }

            if (due != DueDate.EndOfPeriod)
            {
                num = 1.0 + rate;
            }
            else
            {
                num = 1.0;
            }

            double x = rate + 1.0;
            double num2 = Math.Pow(x, nPer);
            return ((-fv - (pv * num2)) / (num * (num2 - 1.0))) * rate;
        }

        private static double FvInternal(double rate, double nPer, double pmt, double pv, DueDate due)
        {
            double num;
            if (rate == 0.0)
            {
                return -pv - (pmt * nPer);
            }

            if (due != DueDate.EndOfPeriod)
            {
                num = 1.0 + rate;
            }
            else
            {
                num = 1.0;
            }

            double x = 1.0 + rate;
            double num2 = Math.Pow(x, nPer);
            return (-pv * num2) - (((pmt / rate) * num) * (num2 - 1.0));
        }
        #endregion Private Static Methods
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