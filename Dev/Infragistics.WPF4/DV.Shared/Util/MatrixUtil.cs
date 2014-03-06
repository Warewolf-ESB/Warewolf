using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// Utility class for matrix operations.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Util is a word.")]
    public static class MatrixUtil
    {
        /// <summary>
        /// Gets the inverse of the current transform.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>Inverse matrix.</returns>
        public static Matrix GetInverse(this Matrix matrix)
        {
            Matrix im = new Matrix();

            double det = matrix.M11 * matrix.M22 - matrix.M21 * matrix.M12;

            if (det != 0)
            {
                im.M11 = matrix.M22 / det;
                im.M21 = -matrix.M21 / det;
                im.OffsetX = (matrix.M21 * matrix.OffsetY - matrix.OffsetX * matrix.M22) / det;

                im.M12 = -matrix.M12 / det;
                im.M22 = matrix.M11 / det;
                im.OffsetY = (matrix.OffsetX * matrix.M12 - matrix.M11 * matrix.OffsetY) / det;
            }

            return im;
        }

        /// <summary>
        /// Transforms a rectangle.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="rc"></param>
        /// <returns>Transformed rectangle.</returns>
        public static Rect Transform(this Matrix matrix, Rect rc)
        {
            Point p0 = matrix.Transform(new Point(rc.Left, rc.Top));
            Point p1 = matrix.Transform(new Point(rc.Right, rc.Bottom));

            return new Rect(p0, p1);
        }

        /// <summary>
        /// Gets a duplicate of the current Matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>Duplicate matrix.</returns>
        public static Matrix Duplicate(this Matrix matrix)
        {
            return new Matrix() { M11 = matrix.M11, M12 = matrix.M12, OffsetX = matrix.OffsetX, M21 = matrix.M21, M22 = matrix.M22, OffsetY = matrix.OffsetY };
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