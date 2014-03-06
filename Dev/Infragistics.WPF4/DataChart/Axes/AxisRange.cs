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
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a chart axis or break range.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AxisRange: IEquatable<AxisRange>
    {
        /// <summary>
        /// Creates and initializes an immutable AxisRange object.
        /// </summary>
        /// <param name="minimum">The range minimum value.</param>
        /// <param name="maximum">The range maximum value.</param>
        public AxisRange(double minimum, double maximum)
        {


            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Gets the range minimum value.
        /// </summary>
        public double Minimum { get; private set; }

        /// <summary>
        /// Gets the range maximum value.
        /// </summary>
        public double Maximum { get; private set; }

        #region IEquatable<AxisRange> Members

        /// <summary>
        /// Returns true if this AxisRange is equal to the other AxisRange.
        /// </summary>
        /// <param name="other">The AxisRange to check against.</param>
        /// <returns>True if the two AxisRange objects are equal.</returns>
        public bool Equals(AxisRange other)
        {
            return other!=null && Minimum == other.Minimum && Maximum == other.Maximum;
        }

        /// <summary>
        /// Returns true if this AxisRange is equal to the other AxisRange.
        /// </summary>
        /// <param name="other">The AxisRange to check against.</param>
        /// <returns>True if the two AxisRange objects are equal.</returns>
        public override bool Equals(Object other)
        {
            return Equals(other as AxisRange);
        }


        /// <summary>
        /// Computes a hash code value.
        /// </summary>
        /// <returns>A hash code value.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
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