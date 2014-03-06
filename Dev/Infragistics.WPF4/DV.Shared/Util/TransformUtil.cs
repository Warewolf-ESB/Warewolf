
using System;
using System.Windows;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// Utility class for graphical transform operations.
    /// </summary>
    public static class TransformUtil
    {
        /// <summary>
        /// Gets a duplicate of this transform. 
        /// </summary>
        /// <param name="transform">The Transform to duplicate.</param>
        /// <returns>The duplicate Transform.</returns>
        public static Transform Duplicate(this Transform transform)
        {
            Transform duplicate = null;

            MatrixTransform matrixTransform = transform as MatrixTransform;
            if (matrixTransform != null)
            {
                duplicate = (Transform)matrixTransform.Duplicate();
            }

            RotateTransform rotateTransform = transform as RotateTransform;
            if (rotateTransform != null)
            {
                duplicate = (Transform)rotateTransform.Duplicate();
            }

            ScaleTransform scaleTransform = transform as ScaleTransform;
            if(scaleTransform != null)
            {
                duplicate = (Transform)scaleTransform.Duplicate();
            }

            SkewTransform skewTransform = transform as SkewTransform;
            if(skewTransform != null)
            {
                duplicate = (Transform)skewTransform.Duplicate();
            }

            TransformGroup transformGroup = transform as TransformGroup;
            if(transformGroup != null)
            {
                duplicate = (Transform)transformGroup.Duplicate();
            }

            TranslateTransform translateTransform = transform as TranslateTransform;
            if(translateTransform != null)
            {
                duplicate = (Transform)translateTransform.Duplicate();
            }

            if (transform!=null && duplicate == null)
            {
                throw new NotImplementedException();
            }

            return duplicate;
        }

        /// <summary>
        /// Gets a duplicate of this matrix transform.
        /// </summary>
        /// <param name="transform">The MatrixTransform to duplicate.</param>
        /// <returns>The duplicate Transform.</returns>
        public static MatrixTransform Duplicate(this MatrixTransform transform)
        {
            return transform != null ? new MatrixTransform() { Matrix = transform.Matrix.Duplicate() } : null;
        }

        /// <summary>
        /// Gets a duplicate of this rotate transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>duplicate transform</returns>
        public static RotateTransform Duplicate(this RotateTransform transform)
        {
            return transform != null ? new RotateTransform() { Angle = transform.Angle, CenterX = transform.CenterX, CenterY = transform.CenterY } : null;
        }

        /// <summary>
        /// Gets a duplicate of this scale transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>duplicate transform</returns>
        public static ScaleTransform Duplicate(this ScaleTransform transform)
        {
            return transform != null ? new ScaleTransform() { CenterX = transform.CenterX, CenterY = transform.CenterY, ScaleX = transform.ScaleX, ScaleY = transform.ScaleY } : null;
        }

        /// <summary>
        /// Gets a duplicate of this skew transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>duplicate transform</returns>
        public static SkewTransform Duplicate(this SkewTransform transform)
        {
            return transform != null ? new SkewTransform() { AngleX = transform.AngleX, AngleY = transform.AngleY, CenterX = transform.CenterX, CenterY = transform.CenterY } : null;
        }

        /// <summary>
        /// Gets a duplicate of this transform group
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>Duplicate transform.</returns>
        public static TransformGroup Duplicate(this TransformGroup transform)
        {
            TransformGroup duplicate = null;

            if (transform != null)
            {
                duplicate=new TransformGroup();

                foreach (Transform child in transform.Children)
                {
                    duplicate.Children.Add(child.Duplicate());
                }
            }

            return duplicate;
        }

        /// <summary>
        /// Gets a duplicate of this translate transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>duplicate transform</returns>
        public static TranslateTransform Duplicate(this TranslateTransform transform)
        {
            return transform != null ? new TranslateTransform() { X = transform.X, Y = transform.Y } : null;
        }

        /// <summary>
        /// Adjusts the current MatrixTransform object to project the source rectangle to the target rectangle. 
        /// </summary>
        /// <param name="matrixTransform">The current MatrixTransform object.</param>
        /// <param name="source">Source rectangle.</param>
        /// <param name="target">Target rectangle.</param>
        /// <returns>The current MatrixTransform</returns>
        public static MatrixTransform SetProjectionMatrix(this MatrixTransform matrixTransform, Rect source, Rect target)
        {
            double umin = target.Left;
            double umax = target.Left + target.Width;
            double vmin = target.Top + target.Height;
            double vmax = target.Top;

            double xmin = source.Left;
            double xmax = source.Left + source.Width;
            double ymin = source.Top + source.Height;
            double ymax = source.Top;

            double m11 = (umax - umin) / (xmax - xmin);
            double m21 = 0.0;
            double offsetX = -xmin * m11 + umin;

            double m12 = 0.0;
            double m22 = (vmax - vmin) / (ymax - ymin);
            double offsetY = -ymin * m22 + vmin;

            matrixTransform.Matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            return matrixTransform;
        }

        /// <summary>
        /// Gets an equivalent affine matrix for this transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>new matrix</returns>
        public static Matrix GetMatrix(this GeneralTransform transform)
        {
            MatrixTransform matrixTransform = transform as MatrixTransform;
            if (matrixTransform != null)
            {
                return matrixTransform.Matrix.Duplicate();
            }

            Point po = transform.Transform(P0);
            Point px = transform.Transform(PX);
            Point py = transform.Transform(PY);

            return new Matrix(px.X - po.X, py.X - po.X, px.Y - po.Y, py.Y - po.Y, po.X, po.Y);
        }

        /// <summary>
        /// Gets the inverse transform (as a MatrixTransform) for this transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>new matrix transform, or null if matrix is not invertible</returns>
        public static MatrixTransform GetInverse(this GeneralTransform transform)
        {
            return new MatrixTransform() { Matrix = transform.GetMatrix().GetInverse() };
        }

        /// <summary>
        /// Transforms a rectangle
        /// </summary>
        /// <param name="generalTransform"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rect Transform(this GeneralTransform generalTransform, Rect rect)
        {
            Point p0 = generalTransform.Transform(new Point(rect.Left, rect.Top));
            Point p1 = generalTransform.Transform(new Point(rect.Right, rect.Bottom));

            return new Rect(p0, p1);
        }

        private static Point P0 = new Point(0, 0);
        private static Point PX = new Point(1, 0);
        private static Point PY = new Point(0, 1);
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