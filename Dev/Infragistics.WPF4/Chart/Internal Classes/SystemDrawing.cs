
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Diagnostics;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class SystemDrawing
    {
        #region Methods

        static internal System.Windows.Point[] CreateSplinePoints(System.Windows.Point[] pointList, out int [] indexes, bool is3D )
        {
            Debug.Assert(pointList.Length >= 2);
            
            GraphicsPath path = new GraphicsPath();
            System.Windows.Point[] spline;

            float reduction = is3D ? 1000f : 1f;

            PointF[] ptf = new PointF[pointList.Length];

            for (int index = 0; index < pointList.Length; index++)
            {
                ptf[index].X = (float)pointList[index].X * reduction;
                ptf[index].Y = (float)pointList[index].Y * reduction;
            }

            path.AddCurve(ptf);
            path.Flatten();
            PointF[] resultPtf = path.PathPoints;

            spline = new System.Windows.Point[resultPtf.Length];

            indexes = new int[resultPtf.Length];
            int currentIndx = 0;
            for (int index = 0; index < resultPtf.Length - 1; index++)
            {
                if (currentIndx < ptf.Length - 1 && ptf[currentIndx + 1].X < resultPtf[index].X)
                {
                    currentIndx++;
                }

                indexes[index] = currentIndx;
            }

            for (int index = 0; index < resultPtf.Length; index++)
            {
                spline[index].X = resultPtf[index].X / reduction;
                spline[index].Y = resultPtf[index].Y / reduction;
            }

            return spline;
        }

        #endregion Methods
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