using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;







using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of <see cref="WorksheetShape"/> instances in a <see cref="Worksheet"/> 
	/// or <see cref="WorksheetShapeGroup"/>.
	/// </summary>
	/// <seealso cref="T:Worksheet.Shapes"/>
	/// <seealso cref="T:WorksheetShapeGroup.Shapes"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class WorksheetShapeCollection : 
		ICollection<WorksheetShape>
	{
		#region Member Variables

		private IWorksheetShapeOwner owner;
		private List<WorksheetShape> shapes;

		#endregion Member Variables

		#region Constructor

		internal WorksheetShapeCollection( IWorksheetShapeOwner owner )
		{
			this.owner = owner;
			this.shapes = new List<WorksheetShape>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<WorksheetShape> Members

		void ICollection<WorksheetShape>.Add( WorksheetShape item )
		{
			this.Add( item );
		}

		void ICollection<WorksheetShape>.Clear()
		{
			this.Clear();
		}

		bool ICollection<WorksheetShape>.Contains( WorksheetShape item )
		{
			return this.Contains( item );
		}

		void ICollection<WorksheetShape>.CopyTo( WorksheetShape[] array, int arrayIndex )
		{
			this.shapes.CopyTo( array, arrayIndex );
		}

		int ICollection<WorksheetShape>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<WorksheetShape>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<WorksheetShape>.Remove( WorksheetShape item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<WorksheetShape> Members

		IEnumerator<WorksheetShape> IEnumerable<WorksheetShape>.GetEnumerator()
		{
			return this.shapes.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.shapes.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		// MD 7/14/11 - Shape support
		#region Add ( PredefinedShapeType, Rectangle )

		/// <summary>
		/// Adds a predefined shape to the collection with the specified bounds.
		/// </summary>
		/// <param name="shapeType">The type of shape to add to the collection.</param>
		/// <param name="boundsInTwips">The bounds of the shape on the <see cref="Worksheet"/> in twips (1/20th of a point).</param>
		/// <exception cref="InvalidOperationException">
		/// This shapes collection belongs to a <see cref="WorksheetShapeGroup"/> which isn't placed on a Worksheet yet. In this case,
		/// call <see cref="Add(PredefinedShapeType,Worksheet,Rectangle)"/> instead and specify the Worksheet where the group will be added.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="shapeType"/> is not defined in the <see cref="PredefinedShapeType"/> enumeration.
		/// </exception>
		/// <returns>A <see cref="WorksheetShape"/>-derived instance representing the predefined shape.</returns>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public WorksheetShape Add(PredefinedShapeType shapeType, Rectangle boundsInTwips)
		{
			if (this.Worksheet == null)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_NoWorksheetContextToSetShapeBounds"));

			return this.Add(shapeType, this.Worksheet, boundsInTwips);
		}

		#endregion // Add ( PredefinedShapeType, Rectangle )

		#region Add ( PredefinedShapeType, Worksheet, Rectangle )

		/// <summary>
		/// Adds a predefined shape to the collection with the specified bounds.
		/// </summary>
		/// <param name="shapeType">The type of shape to add to the collection.</param>
		/// <param name="worksheet">The <see cref="Worksheet"/> to which the <paramref name="boundsInTwips"/> relate.</param>
		/// <param name="boundsInTwips">The bounds of the shape on the <paramref name="worksheet"/> in twips (1/20th of a point).</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="shapeType"/> is not defined in the <see cref="PredefinedShapeType"/> enumeration.
		/// </exception>
		/// <returns>A <see cref="WorksheetShape"/>-derived instance representing the predefined shape.</returns>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public WorksheetShape Add(PredefinedShapeType shapeType, Worksheet worksheet, Rectangle boundsInTwips)
		{
			if (worksheet == null)
				throw new ArgumentNullException("worksheet");

			WorksheetShape shape = WorksheetShape.CreatePredefinedShape(shapeType);
			if (shape == null)
				return null;

			shape.SetBoundsInTwips(worksheet, boundsInTwips);
			this.Add(shape);
			return shape;
		}

		#endregion // Add ( PredefinedShapeType, Worksheet, Rectangle )

		#region Add ( PredefinedShapeType, WorksheetCell, PointF, WorksheetCell, PointF )

		/// <summary>
		/// Adds a predefined shape to the collection with the specified anchors.
		/// </summary>
		/// <param name="shapeType">The type of shape to add to the collection.</param>
		/// <param name="topLeftCornerCell">The cell where the top-left corner of the shape resides.</param>
		/// <param name="topLeftCornerPosition">The position in the <paramref name="topLeftCornerCell"/> of the shape's top-left corner,
		/// expressed in percentages.</param>
		/// <param name="bottomRightCornerCell">The cell where the bottom-right corner of the shape resides.</param>
		/// <param name="bottomRightCornerPosition">the position in the <paramref name="bottomRightCornerCell"/> of the shape's bottom-right 
		/// corner, expressed in percentages.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="shapeType"/> is not defined in the <see cref="PredefinedShapeType"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="topLeftCornerCell"/> or <paramref name="bottomRightCornerCell"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="topLeftCornerCell"/> and <paramref name="bottomRightCornerCell"/> belong to different worksheets or a worksheet other 
		/// than the worksheet to which this collection belongs.
		/// </exception>
		/// <returns>A <see cref="WorksheetShape"/>-derived instance representing the predefined shape.</returns>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public WorksheetShape Add(PredefinedShapeType shapeType,
			WorksheetCell topLeftCornerCell,
			PointF topLeftCornerPosition,
			WorksheetCell bottomRightCornerCell,
			PointF bottomRightCornerPosition)
		{
			WorksheetShape shape = WorksheetShape.CreatePredefinedShape(shapeType);
			if (shape == null)
				return null;

			// MD 3/27/12 - 12.1 - Table Support
			//shape.SetAnchors(topLeftCornerCell, topLeftCornerPosition, bottomRightCornerCell, bottomRightCornerPosition);
			if (topLeftCornerCell == null || bottomRightCornerCell == null)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_AnchorShapeBeforeAddingToCollection"));

			if (topLeftCornerCell.Worksheet != this.Worksheet)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_TopLeftAnchorFromOtherWorksheet"));

			if (bottomRightCornerCell.Worksheet != this.Worksheet)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_BottomRightAnchorFromOtherWorksheet"));

			shape.SetAnchors(topLeftCornerCell.Worksheet, topLeftCornerCell.Address, topLeftCornerPosition, bottomRightCornerCell.Address, bottomRightCornerPosition);

			this.Add(shape);
			return shape;
		}

		#endregion // Add ( PredefinedShapeType, WorksheetCell, PointF, WorksheetCell, PointF )

		#region Add ( WorksheetShape )

		/// <summary>
		/// Adds a shape to the collection.
		/// </summary>
		/// <param name="shape">The shape to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="shape"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="shape"/> has already been added to a worksheet or group.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="shape"/> does not have the <see cref="WorksheetShape.TopLeftCornerCell"/> 
		/// or <see cref="WorksheetShape.BottomRightCornerCell"/> set.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Adding <paramref name="shape"/> to this collection will place it on a different worksheet then either its 
		/// TopLeftCornerCell or BottomRightCornerCell or <paramref name="shape"/> is a <see cref="WorksheetShapeGroup"/> 
		/// and adding it to this collection will create a similar situation for one of its descendant shapes.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="shape"/> is a WorksheetShapeGroup and this is the group's collection of shapes.
		/// </exception>
		public void Add( WorksheetShape shape )
		{
			// MD 7/20/2007 - BR25039
			// Moved all code to new overload, verify the shape can be added by default
			this.Add( shape, true );
		}

		// MD 7/20/2007 - BR25039
		// Added new overload which specifies whether to check if the shape can be added
		internal void Add( WorksheetShape shape, bool verifyCanBeAdded )
		{
			if ( shape == null )
				throw new ArgumentNullException( "shape", SR.GetString( "LE_ArgumentNullException_Shape" ) );

			// MD 7/20/2007 - BR25039
			// Verify whether the shape can be added to the collection
			if ( verifyCanBeAdded && shape.CanBeAddedToShapesCollection == false )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_ShapeCannotBeAdded" ), "shape" );

			shape.OnAddingToCollection( this );
			this.shapes.Add( shape );
			this.owner.OnShapeAdded( shape );
		}

		#endregion Add ( WorksheetShape )

		#region Clear

		/// <summary>
		/// Clears all shapes from the collection.
		/// </summary>
		public void Clear()
		{
			for ( int i = this.shapes.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether a shape is in the collection.
		/// </summary>
		/// <param name="shape">The shape to locate in the collection.</param>
		/// <returns>True if the shape is found; False otherwise.</returns>
		public bool Contains( WorksheetShape shape )
		{
			return this.shapes.Contains( shape );
		}

		#endregion Contains

		#region Remove

		/// <summary>
		/// Removes the specified shape from the collection.
		/// </summary>
		/// <param name="shape">The shape to remove from the collection.</param>
		/// <returns>
		/// True if the shape was successfully removed from the collection; 
		/// False if the shape did not exist in the collection.
		/// </returns>
		public bool Remove( WorksheetShape shape )
		{
			int index = this.shapes.IndexOf( shape );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the shape at the specified index from the collection.
		/// </summary>
		/// <param name="index">The index of the shape to remove from the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			WorksheetShape shape = this.shapes[ index ];

			this.shapes.RemoveAt( index );
			shape.OnRemovedFromCollection();
			this.owner.OnShapeRemoved( shape );
		}

		#endregion RemoveAt

		#endregion Public Methods

		#region Intneral Methods

		// MD 7/20/2007 - BR25039
		#region RemoveInvalidShapes






		internal void RemoveInvalidShapes()
		{
			// Clear out all shapes which shouldn't be in the shapes collection
			for ( int i = this.Count - 1; i >= 0; i-- )
			{
				WorksheetShape shape = this[ i ];

				if ( shape.CanBeAddedToShapesCollection == false )
				{
					// MD 8/1/07 - BR25039
					// This is not an expected code path, don't Fail anymore
					// Utilities.DebugFail( "This shape cannot be added to a shapes collection." );

					this.RemoveAt( i );
					continue;
				}

				WorksheetShapeGroup group = shape as WorksheetShapeGroup;

				if ( group != null )
					group.Shapes.RemoveInvalidShapes();
			}
		}

		#endregion RemoveInvalidShapes

		#endregion Intneral Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of shapes in the collection.
		/// </summary>
		/// <value>The number of shapes in the collection.</value>
		public int Count
		{
			get { return this.shapes.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the shape at the specified index in the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the shape to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The shape at the specified index.</value>
		public WorksheetShape this[ int index ]
		{
			get
			{
				if ( index < 0 || this.Count <= index )
					throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

				return this.shapes[ index ];
			}
		}

		#endregion Indexer [ int ]

		#endregion Public Properties

		#region Internal Properties

		#region IsTopMost






		internal bool IsTopMost
		{
			get { return this.owner.AreChildrenTopMost; }
		}

		#endregion IsTopMost

		#region Owner

		internal IWorksheetShapeOwner Owner
		{
			get { return this.owner; }
		} 

		#endregion Owner

		#region Worksheet

		internal Worksheet Worksheet
		{
			get { return this.owner.Worksheet; }
		}

		#endregion Worksheet

		#endregion Internal Properties

		#endregion Properties
	}

	internal interface IWorksheetShapeOwner
	{
		void OnChildShapeBoundsChanged( WorksheetShape childShape );
		void OnShapeAdded( WorksheetShape shape );
		void OnShapeRemoved( WorksheetShape shape );

		bool AreChildrenTopMost { get;}
		WorksheetShapeCollection Shapes { get;}
		Worksheet Worksheet { get;}
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