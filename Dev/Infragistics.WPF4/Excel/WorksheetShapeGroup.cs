using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;






using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a group of shapes in a worksheet.  This group is also a shape which can be 
	/// positioned and contained within another group or on a worksheet.
	/// </summary>



	public

		 class WorksheetShapeGroup : WorksheetShape,
		IWorksheetShapeOwner
	{
		#region Member Variables

		private WorksheetShapeCollection shapes;

		private bool loading;

		// MD 7/2/12 - TFS115692
		// This is no longer needed on the group.
		//private Point loadedLocation;

		private bool resizeShapesOnBoundsChanged = true;
		private bool resizeBoundsOnChildShapeBoundsChanged = true;

		#endregion Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="WorksheetShapeGroup"/> instance.
		/// </summary>
		public WorksheetShapeGroup() { }

		internal WorksheetShapeGroup( bool loading )
			: this()
		{
			Debug.Assert( loading, "This overload should only be used when loading the group." );
			this.loading = loading;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 8/23/11 - TFS84306
		// The WorksheetShapeGroup cannot have it's Fill changed.
		#region CanHaveFill

		internal override bool CanHaveFill
		{
			get { return false; }
		}

		#endregion  // CanHaveFill

		// MD 8/23/11 - TFS84306
		// The WorksheetShapeGroup cannot have it's Outline changed.
		#region CanHaveOutline

		internal override bool CanHaveOutline
		{
			get { return false; }
		}

		#endregion  // CanHaveOutline

		// MD 7/2/12 - TFS115692
		// This was an oversight. We were not calling InitSerializationCache on any children of group shapes.
		#region InitSerializationCache

		internal override void InitSerializationCache(Serialization.WorkbookSerializationManager serializationManager)
		{
			base.InitSerializationCache(serializationManager);

			foreach (WorksheetShape shape in this.Shapes)
				shape.InitSerializationCache(serializationManager);
		}

		#endregion // InitSerializationCache

		#region OnAddingToCollection







		internal override void OnAddingToCollection( WorksheetShapeCollection collection )
		{
			// Make sure the group is not being added to its own collection of shapes.
			if ( collection != null && collection == this.shapes )
				throw new InvalidOperationException( SR.GetString( "LE_ArgumentOutOfRangeException_GroupAddedToSelf" ) );

			base.OnAddingToCollection( collection );
		}

		#endregion OnAddingToCollection

		#region OnBoundsChanged






		// MD 3/27/12 - 12.1 - Table Support
		//internal override void OnBoundsChanged(
		//    WorksheetCell oldTopLeftCornerCell,
		//    PointF oldTopLeftCornerPosition,
		//    WorksheetCell oldBottomRightCornerCell,
		//    PointF oldBottomRightCornerPosition )
		internal override void OnBoundsChanged(
			Worksheet worksheet,
			WorksheetCellAddress oldTopLeftCornerCell,
			PointF oldTopLeftCornerPosition,
			WorksheetCellAddress oldBottomRightCornerCell,
			PointF oldBottomRightCornerPosition)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//if ( this.resizeShapesOnBoundsChanged 
			//    && this.TopLeftCornerCell != null 
			//    && this.BottomRightCornerCell != null
			//    && oldTopLeftCornerCell != null
			//    && oldBottomRightCornerCell != null)
			if (this.resizeShapesOnBoundsChanged
				&& worksheet != null
				&& this.TopLeftCornerCellInternal != WorksheetCellAddress.InvalidReference
				&& this.BottomRightCornerCellInternal != WorksheetCellAddress.InvalidReference
				&& oldTopLeftCornerCell != WorksheetCellAddress.InvalidReference
				&& oldBottomRightCornerCell != WorksheetCellAddress.InvalidReference)
			{
				try
				{
					// Set the anti-recursion flag so we know not to resize the bounds of this shape when we 
					// resize the child shapes below
					this.resizeBoundsOnChildShapeBoundsChanged = false;

					// Determine the old twip bounds of the group
					Rectangle oldGroupBounds = WorksheetShape.GetBoundsInTwips(
						worksheet,
						oldTopLeftCornerCell,
						oldTopLeftCornerPosition,
						oldBottomRightCornerCell,
						oldBottomRightCornerPosition );

					// Get the new twip bounds of the group
					Rectangle newGroupBounds = this.GetBoundsInTwips();

					// If the bounds of the group have changed, change the bounds of the child shapes inside.
					if ( oldGroupBounds != newGroupBounds )
					{
						// MD 3/27/12 - 12.1 - Table Support
						//// Get the worksheet which the shape will be on.
						//Worksheet worksheet = this.TopLeftCornerCell.Worksheet;
						//
						//// MD 2/29/12 - 12.1 - Table Support
						//// The worksheet can now be null.
						//if (worksheet == null)
						//{
						//    Utilities.DebugFail("This is unexpected");
						//    return;
						//}

						// Determine the ratio of change for the group's bounds in each dimension
                        float widthAdjustment = (int)newGroupBounds.Width / (float)oldGroupBounds.Width;
						float heightAdjustment = (int)newGroupBounds.Height / (float)oldGroupBounds.Height;

						// Reposition each shape in the group
						foreach ( WorksheetShape shape in this.Shapes )
						{
							// Get the current shape bounds (based on its old position)
							Rectangle shapeBounds = shape.GetBoundsInTwips();

							// Get the old bounds as a position within the group (upper-left corner of old group bounds is (0,0))
							Rectangle shapeBoundsInGroup = shapeBounds;
							shapeBoundsInGroup.Offset( -oldGroupBounds.X, -oldGroupBounds.Y );

							// Shift the old bounds to the new bounds and apply it to the shape
							shape.SetBoundsInTwips( worksheet, new Rectangle(
								(int)( shapeBoundsInGroup.Left * widthAdjustment ) + newGroupBounds.Left,
								(int)( shapeBoundsInGroup.Top * heightAdjustment ) + newGroupBounds.Top,
								(int)( shapeBoundsInGroup.Width * widthAdjustment ),
								(int)( shapeBoundsInGroup.Height * heightAdjustment ) ) );
						}
					}
				}
				finally
				{
					// Reset the anit-recursion flag that was set above
					this.resizeBoundsOnChildShapeBoundsChanged = true;
				}
			}

			base.OnBoundsChanged(
				// MD 3/27/12 - 12.1 - Table Support
				worksheet,
				oldTopLeftCornerCell,
				oldTopLeftCornerPosition,
				oldBottomRightCornerCell,
				oldBottomRightCornerPosition );
		}

		#endregion OnBoundsChanged

		// MD 11/3/10 - TFS49093
		// Now that strings are in the shard string table, we need to unroot strings on the shapes when this is removed.
		#region OnRemovedFromCollection

		internal override void OnRemovedFromCollection()
		{
			base.OnRemovedFromCollection();

			// If this group has no shapes, don't do anything
			if (this.shapes == null)
				return;

			// If the group has shapes, let all child shapes know they have been sited on a worksheet.
			foreach (WorksheetShape shape in this.shapes)
				shape.OnRemovedFromCollection();
		} 

		#endregion // OnRemovedFromCollection

		#region OnSitedOnWorksheet







		internal override void OnSitedOnWorksheet( Worksheet worksheet )
		{
			base.OnSitedOnWorksheet( worksheet );

			// If this group has no shapes, don't do anything
			if ( this.shapes == null )
				return;

			// If the group has shapes, let all child shapes know they have been sited on a worksheet.
			foreach ( WorksheetShape shape in this.shapes )
				shape.OnSitedOnWorksheet( worksheet );
		}

		#endregion OnSitedOnWorksheet

		// MD 10/10/11 - TFS90805
		#region Removed

		//// MD 7/20/2007 - BR25039
		//#region Type

		//internal override ShapeType Type
		//{
		//    get { return ShapeType.NotPrimitive; }
		//}

		//#endregion Type 

		#endregion  // Removed

		// MD 10/10/11 - TFS90805
		#region Type2003

		internal override ShapeType? Type2003
		{
			get { return ShapeType.NotPrimitive; }
		}

		#endregion  // Type2003

		// MD 10/10/11 - TFS90805
		#region Type2007

		internal override ST_ShapeType? Type2007
		{
			get { return null; }
		}

		#endregion  // Type2007

		#endregion Base Class Overrides

		#region IWorksheetShapeOwner Members

		bool IWorksheetShapeOwner.AreChildrenTopMost
		{
			get 
			{
				// If this group only has one shape, the shape will be promoted, so return whether this group is top most
				if ( this.shapes != null && this.shapes.Count == 1 )
					return this.IsTopMost;

				// Otherwise, the child shapes are not top-most, because they are contained in a group
				return false;
			}
		}

		void IWorksheetShapeOwner.OnChildShapeBoundsChanged( WorksheetShape childShape )
		{
			// The child shape resized should be in this group
			Debug.Assert( this.Shapes.Contains( childShape ) );

			// If the anti-recursion flag has been set, don't recalculate the group bounds based on the child bounds
			// (the flags is set when we are changing the child shape bounds based on the gorup bounds changing, so
			// we don't want to change the group bounds again).
			if ( this.resizeBoundsOnChildShapeBoundsChanged == false )
				return;

			// If the child shape bounds were changed explicitly, the group's bounds should be recalculated to encompass 
			// all child shapes.
			this.RecalculateBounds();
		}

		void IWorksheetShapeOwner.OnShapeAdded( WorksheetShape shape )
		{
			// If a shape was added to the group, extend the bounds of the group to encompass all child shapes.
			this.RecalculateBounds();
		}

		void IWorksheetShapeOwner.OnShapeRemoved( WorksheetShape shape )
		{
			// If a shape was removed from the group, restrict the bounds of the group to encompass all child shapes.
			this.RecalculateBounds();
		}

		#endregion

		#region Methods

		#region Internal Methods

		// MD 6/14/07 - BR23880
		#region GetShapeById






		internal WorksheetShape GetShapeById( uint shapeId )
		{
			return Worksheet.GetShapeById( this.shapes, shapeId );
		}

		#endregion GetShapeById

		// MD 6/14/07 - BR23880
		#region GetSolverRecords






		internal void GetSolverRecords( List<EscherRecordBase> records )
		{
			Worksheet.GetSolverRecords( this.shapes, records );
		}

		#endregion GetSolverRecords

		#region OnLoadComplete

		internal void OnLoadComplete()
		{
			this.loading = false;

			// MD 7/2/12 - TFS115692
			// This is no longer needed on the group.
			//this.loadedLocation = Utilities.PointEmpty;
		}

		#endregion OnLoadComplete

		#endregion Internal Methods

		#region Private Methods

		#region RecalculateBounds

		private void RecalculateBounds()
		{
			// If we are loading, don't ever recalculate the bounds, let all shapes be added to the group first
			if ( this.loading )
				return;

			// Try to get the worksheet associated with this group (if the group has not been sited yet, we will take 
			// the worksheet of one of the anchor cells for a child shape)
			Worksheet worksheet = this.Worksheet;

			// Initialize the cell indicies so Math.Min and Math.Max operations with actual values will provide valid indicies.
			int leftCellColumnIndex = Int32.MaxValue;
			int topCellRowIndex = Int32.MaxValue;
			int rightCellColumnIndex = 0;
			int bottomCellRowIndex = 0;

			// Initialize the cell positions in a similar fashion.
			float leftCellPosition = 100;
			float topCellPosition = 100;
			float rightCellPosition = 0;
			float bottomCellPosition = 0;

			// Iterate each shape, expanding the bounds of the group with each shape so all shapes can be contained in the bounds of the group
			foreach ( WorksheetShape shape in this.Shapes )
			{
				// Cache the shape's anchor cells
				WorksheetCell topLeftCell = shape.TopLeftCornerCell;
				WorksheetCell bottomRightCell = shape.BottomRightCornerCell;

				// Neither anchor cell should be null, but if they are, go to the next shape
				Debug.Assert( topLeftCell != null && bottomRightCell != null );
				if ( topLeftCell == null || bottomRightCell == null )
					continue;

				// If the group was not sited on a worksheet, get the worksheet from the first shape's anchor cell.
				if ( worksheet == null )
					worksheet = topLeftCell.Worksheet;

				// Cache the positions of the shape's anchor cells.
				PointF topLeftPosition = shape.TopLeftCornerPosition;
				PointF bottomRightPosition = shape.BottomRightCornerPosition;

				// Resolve each side with both anchor point (just incase we allow inverting anchors later)
				WorksheetShapeGroup.ResolveLeftAnchor( ref leftCellColumnIndex, ref leftCellPosition, topLeftCell, topLeftPosition );
				WorksheetShapeGroup.ResolveLeftAnchor( ref leftCellColumnIndex, ref leftCellPosition, bottomRightCell, bottomRightPosition );

				WorksheetShapeGroup.ResolveTopAnchor( ref topCellRowIndex, ref topCellPosition, topLeftCell, topLeftPosition );
				WorksheetShapeGroup.ResolveTopAnchor( ref topCellRowIndex, ref topCellPosition, bottomRightCell, bottomRightPosition );

				WorksheetShapeGroup.ResolveRightAnchor( ref rightCellColumnIndex, ref rightCellPosition, topLeftCell, topLeftPosition );
				WorksheetShapeGroup.ResolveRightAnchor( ref rightCellColumnIndex, ref rightCellPosition, bottomRightCell, bottomRightPosition );

				WorksheetShapeGroup.ResolveBottomAnchor( ref bottomCellRowIndex, ref bottomCellPosition, topLeftCell, topLeftPosition );
				WorksheetShapeGroup.ResolveBottomAnchor( ref bottomCellRowIndex, ref bottomCellPosition, bottomRightCell, bottomRightPosition );
			}

			// Check conditions to make sure we calculated the group bounds correctly
			Debug.Assert( leftCellColumnIndex <= rightCellColumnIndex, "The left column cannot be past the right column." );
			Debug.Assert( topCellRowIndex <= bottomCellRowIndex, "The top cell cannot be below the bottom cell." );
			Debug.Assert( leftCellColumnIndex != rightCellColumnIndex || leftCellPosition <= rightCellPosition, "The left anchor cannot be past the right anchor." );
			Debug.Assert( topCellRowIndex != bottomCellRowIndex || topCellPosition <= bottomCellPosition, "The top anchor cannot be below the bottom anchor." );

			try
			{
				// Set the anti-recursion flag so the child shapes are not repositioned and resized when we set the bounds of the group
				this.resizeShapesOnBoundsChanged = false;

				// Set the anchors for the group so its bounds completely enclose all child shapes.
				this.TopLeftCornerCell = worksheet.Rows[ topCellRowIndex ].Cells[ leftCellColumnIndex ];
				this.BottomRightCornerCell = worksheet.Rows[ bottomCellRowIndex ].Cells[ rightCellColumnIndex ];

				this.TopLeftCornerPosition = new PointF( leftCellPosition, topCellPosition );
				this.BottomRightCornerPosition = new PointF( rightCellPosition, bottomCellPosition );
			}
			finally
			{
				// Reset the anti-recursion flasg that was set above
				this.resizeShapesOnBoundsChanged = true;
			}
		}

		#endregion RecalculateBounds

		#region ResolveBottomAnchor

		private static void ResolveBottomAnchor( ref int bottomCellIndex, ref float bottomCellPosition, WorksheetCell bottomRightCell, PointF bottomRightPosition )
		{
			// If the row indices are equal, just resolve the position in the cell to the lowest position
			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, the RowIndex getter is now a bit slower, so cache it.
			//if ( bottomRightCell.RowIndex == bottomCellIndex )
			int bottomRightCellRowIndex = bottomRightCell.RowIndex;
			if (bottomRightCellRowIndex == bottomCellIndex)
			{
                bottomCellPosition = Math.Max((float)bottomRightPosition.Y, bottomCellPosition);
			}
			// MD 7/26/10 - TFS34398
			// Use the cached RowIndex.
			//else if ( bottomCellIndex < bottomRightCell.RowIndex )
			else if (bottomCellIndex < bottomRightCellRowIndex)
			{
				// Otherwise, take the row index and position of the lower cell
				// MD 7/26/10 - TFS34398
				// Use the cached RowIndex.
				//bottomCellIndex = bottomRightCell.RowIndex;
				bottomCellIndex = bottomRightCellRowIndex;

                bottomCellPosition = (float)bottomRightPosition.Y;
			}
		}

		#endregion ResolveBottomAnchor

		#region ResolveLeftAnchor

		private static void ResolveLeftAnchor( ref int leftCellIndex, ref float leftCellPosition, WorksheetCell topLeftCell, PointF topLeftPosition )
		{
			// If the column indices are equal, just resolve the position in the cell to the left-most position
			if ( topLeftCell.ColumnIndex == leftCellIndex )
			{
				leftCellPosition = Math.Min( (float)topLeftPosition.X, leftCellPosition );
			}
			else if ( topLeftCell.ColumnIndex < leftCellIndex )
			{
				// Otherwise, take the column index and position of the left-most cell
				leftCellIndex = topLeftCell.ColumnIndex;
                leftCellPosition = (float)topLeftPosition.X;
			}
		}

		#endregion ResolveLeftAnchor

		#region ResolveRightAnchor

		private static void ResolveRightAnchor( ref int rightCellIndex, ref float rightCellPosition, WorksheetCell bottomRightCell, PointF bottomRightPosition )
		{
			// If the column indices are equal, just resolve the position in the cell to the right-most position
			if ( bottomRightCell.ColumnIndex == rightCellIndex )
			{
                rightCellPosition = Math.Max((float)bottomRightPosition.X, rightCellPosition);
			}
			else if ( rightCellIndex < bottomRightCell.ColumnIndex )
			{
				// Otherwise, take the column index and position of the right-most cell
				rightCellIndex = bottomRightCell.ColumnIndex;
                rightCellPosition = (float)bottomRightPosition.X;
			}
		}

		#endregion ResolveRightAnchor

		#region ResolveTopAnchor

		private static void ResolveTopAnchor( ref int topCellIndex, ref float topCellPosition, WorksheetCell topLeftCell, PointF topLeftPosition )
		{
			// If the row indices are equal, just resolve the position in the cell to the highest position
			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, the RowIndex getter is now a bit slower, so cache it.
			//if ( topLeftCell.RowIndex == topCellIndex )
			int topLeftCellRowIndex = topLeftCell.RowIndex;
			if (topLeftCellRowIndex == topCellIndex)
			{
                topCellPosition = Math.Min((float)topLeftPosition.Y, topCellPosition);
			}
			// MD 7/26/10 - TFS34398
			// Use the cached RowIndex.
			//else if ( topLeftCell.RowIndex < topCellIndex )
			else if (topLeftCellRowIndex < topCellIndex)
			{
				// Otherwise, take the row index and position of the higher cell
				// MD 7/26/10 - TFS34398
				// Use the cached RowIndex.
				//topCellIndex = topLeftCell.RowIndex;
				topCellIndex = topLeftCellRowIndex;
                topCellPosition = (float)topLeftPosition.Y;
			}
		}

		#endregion ResolveTopAnchor

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Shapes

		/// <summary>
		/// Gets the collection of shapes contained in the group.
		/// </summary>
		/// <value>The collection of shapes contained in the group.</value>
		public WorksheetShapeCollection Shapes
		{
			get
			{
				if ( this.shapes == null )
					this.shapes = new WorksheetShapeCollection( this );

				return this.shapes;
			}
		}

		#endregion Shapes

		#endregion Public Properties

		#region Internal Properties

		// MD 7/2/12 - TFS115692
		// This is no longer needed on the group.
		#region Removed

		//#region LoadedLocation

		//internal Point LoadedLocation
		//{
		//    get { return this.loadedLocation; }
		//    set { this.loadedLocation = value; }
		//}

		//#endregion LoadedLocation

		#endregion // Removed

        //  BF 8/21/08  Excel2007 Format
        #region Loading





        internal bool Loading { get { return this.loading; } }
        #endregion Loading

        #endregion Internal Properties

        #endregion Properties
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