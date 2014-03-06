using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Infragistics.Windows.Chart
{
    internal class ColumnBar3DShapes : Primitives3D
    {
        /// <summary>
        /// Creates sides for the column/bar with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="shapePoints">The shape points of the column/bar profile.</param>
        /// <param name="height">The height of the column/bar</param>
        /// <param name="width">The width of the column/bar</param>
        /// <param name="edge">The edge size of the column/bar</param>
        /// <param name="segments">The number of sides of the column/bar profile.</param>
        protected void AddSides(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point[] shapePoints, double height, double width, double edge, int segments)
        {
            // Profile points (triangle, square, octagon, star). 
            Point[] newPoints = new Point[shapePoints.Length * 2];

            // Copy points to the new array.
            int newIndex = 0;
            for (int pointIndx = 0; pointIndx < shapePoints.Length; pointIndx++)
            {
                newPoints[newIndex] = shapePoints[pointIndx];
                newIndex++;
            }

            for (int pointIndx = 0; pointIndx < shapePoints.Length; pointIndx++)
            {
                newPoints[newIndex] = shapePoints[pointIndx];
                newIndex++;
            }

            // Create sides of the column/bar
            for (int pointIndx = 0; pointIndx < shapePoints.Length; pointIndx++)
            {
                AddSide(points, normals, texturePoints, indices, height, edge, newPoints[pointIndx], newPoints[pointIndx + 1], newPoints[pointIndx + 2], segments);
            }

            // Create top and bottom of the column/bar
            Top(points, normals, texturePoints, indices, shapePoints, height, width, edge);
            Bottom(points, normals, texturePoints, indices, shapePoints, height, width, edge);
        }

        /// <summary>
        /// Creates a side for the column with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="height">The height of the column</param>
        /// <param name="edge">The edge size of the column</param>
        /// <param name="start">The start point of the profile segment.</param>
        /// <param name="end">The end point of the profile segment.</param>
        /// <param name="nextEnd">The end point of the next segment of the profile.</param>
        /// <param name="segments">The number of sides of the column profile.</param>
        protected virtual void AddSide(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double height, double edge, Point start, Point end, Point nextEnd, int segments)
        {
        }
           

        /// <summary>
        /// Creates the top for the bar with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="shapePoints">The shape points of the bar profile.</param>
        /// <param name="height">The height of the bar</param>
        /// <param name="width">The width of the column/bar</param>
        /// <param name="edge">The edge size of the bar</param>
        protected virtual void Top(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point[] shapePoints, double height, double width, double edge)
        {
        }
        
        /// <summary>
        /// Creates the bottom for the column with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="shapePoints">The shape points of the column profile.</param>
        /// <param name="height">The height of the column</param>
        /// <param name="width">The width of the column/bar</param>
        /// <param name="edge">The edge size of the column</param>
        protected virtual void Bottom(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point[] shapePoints, double height, double width, double edge)
        {
        }

        /// <summary>
        /// Makes correction of the profile positions because of the edge size.
        /// </summary>
        /// <param name="start">The start point of the profile segment</param>
        /// <param name="end">The end point of the profile segment</param>
        /// <param name="startEdge">True if the edge is next to the start position, false if the edge is next to the end position</param>
        /// <param name="edge">The size of the edge</param>
        /// <returns>The profile 2D position corrected for the edge size.</returns>
        protected Point GetEdgePosition(Point start, Point end, bool startEdge, double edge)
        {
            double xDist = end.X - start.X;
            double yDist = end.Y - start.Y;
            double distance = Math.Sqrt(xDist * xDist + yDist * yDist);
            double xEdge = xDist * edge / distance;
            double yEdge = yDist * edge / distance;

            if (startEdge)
            {
                // The edge is next to the start position.
                return new Point(start.X + xEdge, start.Y + yEdge);
            }
            else
            {
                // The edge is next to the end position.
                return new Point(end.X - xEdge, end.Y - yEdge);
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