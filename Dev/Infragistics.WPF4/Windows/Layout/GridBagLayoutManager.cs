






using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;

namespace Infragistics.Controls.Layouts.Primitives
{
	#region IGridBagConstraint Interface

	/// <summary>
	/// Interface for providing constraints to the grid-bag layout manager.
	/// </summary>
	/// <remarks>
	/// <b>IGridBagConstraint</b> interface is used to provide constraints of an item to the
	/// <see cref="GridBagLayoutManager"/>. <see cref="GridBagConstraint"/> class implements
	/// this interface so typically there is no need for you to implement this interface as
	/// you can use the <i>GridBagConstraint</i> objects directly to provide item constraints
	/// to the grid-bag layout manager.
	/// </remarks>
	/// <seealso cref="GridBagConstraint"/>
	/// <seealso cref="GridBagLayoutManager"/>
	public interface IGridBagConstraint
	{
		#region Properties

		#region Column

		/// <summary>
		/// <p>Column and Row define where the layout item will be placed in the virtual grid of the grid-bag layout. Column specifies the location horizontally while specifies the location vertically. These locations are the coordinates of the cells in the virtual grid that the grid-bag layout represents.</p>
		/// <p>The leftmost cell has Column of 0. The constant <see cref="GridBagConstraintConstants.Relative"/> specifies that the item be placed just to the right of the item that was added to the layout manager just before this item was added. </p>
		/// <p>The default value is <see cref="GridBagConstraintConstants.Relative"/>. Column should be a non-negative value.</p>
		/// </summary>
		/// <seealso cref="Row"/>
		int Column { get; }

		#endregion // Column

		#region ColumnSpan

		/// <summary>
		/// <p>Specifies the number of cells this item will span horizontally. The constant <see cref="GridBagConstraintConstants.Remainder"/> specifies that this item be the last one in the row and thus occupy remaining space.</p>
		/// </summary>
		/// <seealso cref="RowSpan"/>
		int ColumnSpan { get; }

		#endregion // ColumnSpan

		#region ColumnWeight

		/// <summary>
		/// Indicates how the extra horizontal space will be distributed among items. Default value is 0.0. Higher values give higher priority. The weight of the column in the virtual grid the grid-bag layout represents is the maximum ColumnWeight of all the items in the row.
		/// </summary>
		float ColumnWeight { get; }

		#endregion // ColumnWeight

		#region HorizontalAlignment

		/// <summary>
		/// Horizontal alignment of the item within its logical cell.
		/// </summary>
		HorizontalAlignment HorizontalAlignment { get; }

		#endregion // HorizontalAlignment

		#region Margin

		/// <summary>
		/// Indicates the padding around the layout item.
		/// </summary>
		Thickness Margin { get; }

		#endregion // Margin

		#region Row

		/// <summary>
		/// <p>Column and Row define where the layout item will be placed in the virtual grid of the grid-bag layout. Column specifies the location horizontally while specifies the location vertically. These locations are the coordinates of the cells in the virtual grid that the grid-bag layout represents.</p>
		/// <p>The topmost cell has Row of 0. The constant <see cref="GridBagConstraintConstants.Relative"/> specifies that the item be placed just below the item that was added to the layout manager just before this item was added.</p>
		/// <p>The default value is <see cref="GridBagConstraintConstants.Relative"/>. Row should be a non-negative value.</p>
		/// </summary>
		/// <seealso cref="Column"/>
		int Row { get; }

		#endregion // Row

		#region RowSpan

		/// <summary>
		/// <p>Specifies the number of cells this item will span vertically. The constant <see cref="GridBagConstraintConstants.Remainder"/> specifies that this item be the last one in the column and thus occupy remaining space.</p>
		/// </summary>
		/// <seealso cref="ColumnSpan"/>
		int RowSpan { get; }

		#endregion // RowSpan

		#region RowWeight

		/// <summary>
		/// Indicates how the extra vertical space will be distributed among items. Default value is 0.0. Higher values give higher priority. The weight of the column in the virtual grid the grid-bag layout represents is the maximum RowWeight of all the items in the column.
		/// </summary>
		float RowWeight { get; }

		#endregion // RowWeight

		#region VerticalAlignment

		/// <summary>
		/// Horizontal alignment of the item within its logical row.
		/// </summary>
		VerticalAlignment VerticalAlignment { get; }

		#endregion // VerticalAlignment

		#endregion // Properties
	}

	#endregion // IGridBagConstraint Interface

	#region GridBagConstraintConstants Class

	/// <summary>
	/// Defines constants used by grid-bag constraint object.
	/// </summary>
	public sealed class GridBagConstraintConstants
	{
		#region Public Constants

		/// <summary>
		/// This constant can be assigned to Column and Row to indicate that the cell be positioned relative to the last cell.
		/// </summary>
		public const int Relative = -1;

		/// <summary>
		/// This constant can be assigned to ColumnSpan and RowSpan to indicate that the cell occupy the rest of the row or the column respectively.
		/// </summary>
		public const int Remainder = 0;

		#endregion // Public Constants

		#region Constructor






		private GridBagConstraintConstants( )
		{
		}

		#endregion // Constructor
	}

	#endregion // GridBagConstraintConstants Class

	#region GridBagConstraint Class

	/// <summary>
	/// Class for providing constraint objects to the grid-bag layout manager.
	/// </summary>
	public class GridBagConstraint : PropertyChangeNotifier, IGridBagConstraint

		, ICloneable

	{
		#region Private Variables

		private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Center;
		private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
		private int _column;
		private int _row;
		private int _columnSpan;
		private int _rowSpan;
		private float _columnWeight;
		private float _rowWeight;
		private Thickness _margin;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public GridBagConstraint( ) :
			this( GridBagConstraintConstants.Relative, GridBagConstraintConstants.Relative, 1, 1 )
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="column">Where the layout item will be placed horizontally in the virtual grid of the grid-bag layout.</param>
		/// <param name="row">Where the layout item will be placed horizontally in the virtual grid of the grid-bag layout.</param>
		/// <param name="columnSpan">Specifies the number of cells this item will span horizontally.</param>
		/// <param name="rowSpan">Specifies the number of cells this item will span vertically.</param>
		public GridBagConstraint( int column, int row, int columnSpan, int rowSpan )
		{
			_column = column;
			_row = row;
			_columnSpan = columnSpan;
			_rowSpan = rowSpan;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region HorizontalAlignment

		/// <summary>
		/// Horizontal alignment of the item within its logical cell.
		/// </summary>
		public HorizontalAlignment HorizontalAlignment
		{
			get
			{
				return _horizontalAlignment;
			}
			set
			{
				if ( value != _horizontalAlignment )
				{
					_horizontalAlignment = value;

					this.RaisePropertyChangedEvent( "HorizontalAlignment" );
				}
			}
		}

		#endregion // HorizontalAlignment

		#region Column

		/// <summary>
		/// <p>Column and Row define where the layout item will be placed in the virtual grid of the grid-bag layout. Column specifies the location horizontally while specifies the location vertically. These locations are the coordinates of the cells in the virtual grid that the grid-bag layout represents.</p>
		/// <p>The leftmost cell has Column of 0. The constant <b>Relative</b> specifies that the item be placed just to the right of the item that was added to the layout manager just before this item was added. </p>
		/// <p>The default value is <b>Relative</b>. Column should be a non-negative value.</p>
		/// </summary>
		/// <seealso cref="Row"/>
		//[Description( "Determines the column of the logical cell that will be starting point of the item." )]
		public int Column
		{
			get
			{
				return this._column;
			}
			set
			{
				if ( value == this._column )
					return;

				if ( value < 0 && GridBagConstraintConstants.Relative != value )
					ThrowArgumentOutOfRange( "Column", value, "LE_GridBag_InvalidRowColumn" );

				this._column = value;

				this.RaisePropertyChangedEvent( "Column" );
			}
		}

		#endregion // Column

		#region ColumnSpan

		/// <summary>
		/// <p>Specifies the number of cells this item will span horizontally. The constant <b>Remainder</b> specifies that this item be the last one in the row and thus occupy remaining space.</p>
		/// </summary>
		/// <seealso cref="RowSpan"/>
		//[Description( "Determine the number of columns that the item will span." )]
		public int ColumnSpan
		{
			get
			{
				return this._columnSpan;
			}
			set
			{
				if ( value == this._columnSpan )
					return;

				if ( value < 1 && GridBagConstraintConstants.Remainder != value && GridBagConstraintConstants.Relative != value )
					ThrowArgumentOutOfRange( "ColumnSpan", value, "LE_GridBag_InvalidSpan" );

				this._columnSpan = value;

				this.RaisePropertyChangedEvent( "ColumnSpan" );
			}
		}

		#endregion // ColumnSpan

		#region ColumnWeight

		/// <summary>
		/// Indicates how the extra horizontal space will be distributed among items. Default value is 0.0. Higher values give higher priority. The weight of the column in the virtual grid the grid-bag layout represents is the maximum ColumnWeight of all the items in the row.
		/// </summary>
		//[Description( "Determines how extra horizontal space above the preferred size is distributed. This value is relative to the ColumnWeight of items in other logical columns." )]
		public float ColumnWeight
		{
			get
			{
				return this._columnWeight;
			}
			set
			{
				if ( value == this.ColumnWeight )
					return;

				if ( value < 0.0f )
					ThrowArgumentOutOfRange( "ColumnWeight", value, "LE_GridBag_InvalidWeight" );

				this._columnWeight = value;

				this.RaisePropertyChangedEvent( "ColumnWeight" );
			}
		}

		#endregion // ColumnWeight

		#region Margin

		/// <summary>
		/// Indicates the padding around the layout item.
		/// </summary>
		//[Description( "Determines the amount of padding around the item." )]
		public Thickness Margin
		{
			get
			{
				return this._margin;
			}
			set
			{
				if ( this._margin == value )
					return;

				this._margin = value;

				this.RaisePropertyChangedEvent( "Insets" );
			}
		}

		#endregion // Margin

		#region Row

		/// <summary>
		/// <p>Column and Row define where the layout item will be placed in the virtual grid of the grid-bag layout. Column specifies the location horizontally while specifies the location vertically. These locations are the coordinates of the cells in the virtual grid that the grid-bag layout represents.</p>
		/// <p>The topmost cell has Row of 0. The constant <b>Relative</b> specifies that the item be placed just below the item that was added to the layout manager just before this item was added.</p>
		/// <p>The default value is <b>Relative</b>. Row should be a non-negative value.</p>
		/// </summary>
		/// <seealso cref="Column"/>
		//[Description( "Determines the row of the logical cell that will be starting point of the item." )]
		public int Row
		{
			get
			{
				return this._row;
			}
			set
			{
				if ( value == this._row )
					return;

				if ( value < 0 && GridBagConstraintConstants.Relative != value )
					ThrowArgumentOutOfRange( "Row", value, "LE_GridBag_InvalidRowColumn" );

				this._row = value;

				this.RaisePropertyChangedEvent( "Row" );
			}
		}

		#endregion // Row

		#region RowSpan

		/// <summary>
		/// <p>Specifies the number of cells this item will span vertically. The constant <b>Remainder</b> specifies that this item be the last one in the column and thus occupy remaining space.</p>
		/// </summary>
		/// <seealso cref="ColumnSpan"/>
		//[Description( "Determine the number of rows that the item will span." )]
		public int RowSpan
		{
			get
			{
				return this._rowSpan;
			}
			set
			{
				if ( value == this._rowSpan )
					return;

				if ( value < 1 && GridBagConstraintConstants.Remainder != value && GridBagConstraintConstants.Relative != value )
					ThrowArgumentOutOfRange( "RowSpan", value, "LE_GridBag_InvalidSpan" );

				this._rowSpan = value;

				this.RaisePropertyChangedEvent( "RowSpan" );
			}
		}

		#endregion // RowSpan

		#region RowWeight

		/// <summary>
		/// Indicates how the extra vertical space will be distributed among items. Default value is 0.0. Higher values give higher priority. The weight of the column in the virtual grid the grid-bag layout represents is the maximum RowWeight of all the items in the column.
		/// </summary>
		//[Description( "Determines how extra vertical space above the preferred size is distributed. This value is relative to the RowWeight of items in other logical rows." )]
		public float RowWeight
		{
			get
			{
				return this._rowWeight;
			}
			set
			{
				if ( value == this._rowWeight )
					return;

				if ( value < 0.0f )
					ThrowArgumentOutOfRange( "RowWeight", value, "LE_GridBag_InvalidWeight" );

				this._rowWeight = value;

				this.RaisePropertyChangedEvent( "RowWeight" );
			}
		}

		#endregion // RowWeight

		#region VerticalAlignment

		/// <summary>
		/// Vertical alignment of the item within its logical cell.
		/// </summary>
		public VerticalAlignment VerticalAlignment
		{
			get
			{
				return _verticalAlignment;
			}
			set
			{
				if ( value != _verticalAlignment )
				{
					_verticalAlignment = value;

					this.RaisePropertyChangedEvent( "VerticalAlignment" );
				}
			}
		}

		#endregion // VerticalAlignment

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Clone

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A copy of the current instance.</returns>
		public object Clone( )
		{
			GridBagConstraint gc = (GridBagConstraint)this.MemberwiseClone( );
			return gc;
		}

		#endregion // Clone

		#region ResetColumn

		/// <summary>
		/// Resets the Column property to its default value of GridBagConstraintConstants.Relative.
		/// </summary>
		public void ResetColumn( )
		{
			this.Column = GridBagConstraintConstants.Relative;
		}

		#endregion // ResetColumn

		#region ResetColumnSpan

		/// <summary>
		/// Resets the ColumnSpan property to its default value of 1.
		/// </summary>
		public void ResetColumnSpan( )
		{
			this.ColumnSpan = 1;
		}

		#endregion // ResetColumnSpan

		#region ResetColumnWeight

		/// <summary>
		/// Resets the ColumnWeight property to its default value of 0.0.
		/// </summary>
		public void ResetColumnWeight( )
		{
			this.ColumnWeight = 0.0f;
		}

		#endregion // ResetColumnWeight

		#region ResetHorizontalAlignment

		/// <summary>
		/// Resets the HorizontalAlignment property to its default value of Center.
		/// </summary>
		public void ResetHorizontalAlignment( )
		{
			this.HorizontalAlignment = HorizontalAlignment.Center;
		}

		#endregion // ResetHorizontalAlignment

		#region ResetMargin

		/// <summary>
		/// Resets the Margin property to its default value which is Thickness of left, top, right, bottom being 0.
		/// </summary>
		public void ResetMargin( )
		{
			this._margin = new Thickness( );
		}

		#endregion // ResetMargin

		#region ResetRow

		/// <summary>
		/// Resets the Row property to its default value of GridBagConstraintConstants.Relative.
		/// </summary>
		public void ResetRow( )
		{
			this.Row = GridBagConstraintConstants.Relative;
		}

		#endregion // ResetRow

		#region ResetRowSpan

		/// <summary>
		/// Resets the RowSpan property to its default value of 1.
		/// </summary>
		public void ResetRowSpan( )
		{
			this.RowSpan = 1;
		}

		#endregion // ResetRowSpan

		#region ResetRowWeight

		/// <summary>
		/// Resets the RowWeight property to its default value of 0.0.
		/// </summary>
		public void ResetRowWeight( )
		{
			this.RowWeight = 0.0f;
		}

		#endregion // ResetRowWeight

		#region ResetVerticalAlignment

		/// <summary>
		/// Resets the VerticalAlignment property to its default value of Center.
		/// </summary>
		public void ResetVerticalAlignment( )
		{
			this.VerticalAlignment = VerticalAlignment.Center;
		}

		#endregion // ResetVerticalAlignment

		#region Reset

		/// <summary>
		/// Resets the properties of this object to their default values.
		/// </summary>
		public void Reset( )
		{
			this.ResetHorizontalAlignment( );
			this.ResetVerticalAlignment( );
			this.ResetMargin( );
			this.ResetColumn( );
			this.ResetRow( );
			this.ResetColumnSpan( );
			this.ResetRowSpan( );
			this.ResetColumnWeight( );
			this.ResetRowWeight( );
		}

		#endregion // Reset

		#region ShouldSerialize

		/// <summary>
		/// Returns true is any of the properties have been set to non-default values
		/// </summary>
		/// <returns>True if any properties have been set to non-default values.</returns>
		public bool ShouldSerialize( )
		{
			return this.ShouldSerializeHorizontalAlignment( ) ||
				this.ShouldSerializeVerticalAlignment( ) ||
				this.ShouldSerializeMargin( ) ||
				this.ShouldSerializeColumn( ) ||
				this.ShouldSerializeRow( ) ||
				this.ShouldSerializeColumnSpan( ) ||
				this.ShouldSerializeRowSpan( ) ||
				this.ShouldSerializeColumnWeight( ) ||
				this.ShouldSerializeRowWeight( );
		}

		#endregion // ShouldSerialize

		#region ToString
		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		/// <returns>The string representation of the object.</returns>
		public override string ToString( )
		{
			System.Text.StringBuilder SB = new System.Text.StringBuilder( );

			if ( this.ShouldSerializeColumn( ) || this.ShouldSerializeRow( ) )
			{
				SB.AppendFormat( "Origin={0}, {1}",
					this.Column,
					this.Row );
			}

			if ( this.ShouldSerializeColumnSpan( ) || this.ShouldSerializeRowSpan( ) )
			{
				if ( SB.Length > 0 )
					SB.Append( "; " );

				SB.AppendFormat( "Span={0}, {1}",
					this.ColumnSpan,
					this.RowSpan );
			}

			if ( SB.Length > 0 )
				SB.Append( "; " );

			SB.AppendFormat( "; Horiz Align={0}", this.HorizontalAlignment );
			SB.AppendFormat( "; Vert Align={0}", this.VerticalAlignment );

			return SB.ToString( );
		}
		#endregion //ToString

		#endregion // Public Methods

		#region Protected Methods

		#region ShouldSerializeColumn

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeColumn( )
		{
			return GridBagConstraintConstants.Relative != this._column;
		}

		#endregion // ShouldSerializeColumn

		#region ShouldSerializeColumnSpan

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeColumnSpan( )
		{
			return 1 != this._columnSpan;
		}

		#endregion // ShouldSerializeColumnSpan

		#region ShouldSerializeColumnWeight

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeColumnWeight( )
		{
			return 0.0f != this._columnWeight;
		}

		#endregion // ShouldSerializeColumnWeight

		#region ShouldSerializeHorizontalAlignment

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeHorizontalAlignment( )
		{
			return HorizontalAlignment.Center != _horizontalAlignment;
		}

		#endregion // ShouldSerializeHorizontalAlignment

		#region ShouldSerializeMargin

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeMargin( )
		{
			return
				0 != this._margin.Left ||
				0 != this._margin.Top ||
				0 != this._margin.Right ||
				0 != this._margin.Bottom;
		}

		#endregion // ShouldSerializeMargin

		#region ShouldSerializeRow

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeRow( )
		{
			return GridBagConstraintConstants.Relative != this._row;
		}

		#endregion // ShouldSerializeRow

		#region ShouldSerializeRowSpan

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeRowSpan( )
		{
			return 1 != this._rowSpan;
		}

		#endregion // ShouldSerializeRowSpan

		#region ShouldSerializeRowWeight

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeRowWeight( )
		{
			return 0.0f != this._rowWeight;
		}

		#endregion ShouldSerializeRowWeight

		#region ShouldSerializeVerticalAlignment

		/// <summary>
		/// Returns true if the property is set to a non-default value.
		/// </summary>
		/// <returns>True if the property is set to a non-default value.</returns>
		protected bool ShouldSerializeVerticalAlignment( )
		{
			return VerticalAlignment.Center != _verticalAlignment;
		}

		#endregion // ShouldSerializeVerticalAlignment

		#endregion // Protected Methods

		#region Private Methods

		#region ThrowArgumentOutOfRange
		private static void ThrowArgumentOutOfRange(string property, object value, string errorResource)
		{

			throw new ArgumentOutOfRangeException(property, value, GridBagLayoutManager.GetString(errorResource));



		}
		#endregion // ThrowArgumentOutOfRange

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // GridBagConstraint Class

	#region GridBagLayoutItemDimensions Class

	/// <summary>
	/// For internal use. A class that contains dimensions of a layout item. 
	/// </summary>
	public class GridBagLayoutItemDimensions
	{
		#region Private Vars

		private int _column;
		private int _row;
		private int _columnSpan;
		private int _rowSpan;
		private Rect _bounds = Rect.Empty;

		#endregion // Private Vars

		#region Constructor



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal GridBagLayoutItemDimensions(
			int column,
			int row,
			int columnSpan,
			int rowSpan,
			Rect bounds )
		{
			this._column = column;
			this._row = row;
			this._columnSpan = columnSpan;
			this._rowSpan = rowSpan;
			this._bounds = bounds;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Bounds

		/// <summary>
		/// Item bounds relative to the container's bounds.
		/// </summary>
		public Rect Bounds
		{
			get
			{
				return this._bounds;
			}
		}

		#endregion // Bounds

		#region Column

		/// <summary>
		/// Returns the Column of the item.
		/// </summary>
		public int Column
		{
			get
			{
				return this._column;
			}
		}

		#endregion // Column

		#region ColumnRight

		/// <summary>
		/// Returns Column + ColumnSpan.
		/// </summary>
		internal int ColumnRight
		{
			get
			{
				return _column + _columnSpan;
			}
		}

		#endregion // ColumnRight

		#region ColumnSpan

		/// <summary>
		/// Returns the ColumnSpan of the item.
		/// </summary>
		public int ColumnSpan
		{
			get
			{
				return this._columnSpan;
			}
		}

		#endregion // ColumnSpan

		#region Row

		/// <summary>
		/// Returns the Row of the item.
		/// </summary>
		public int Row
		{
			get
			{
				return this._row;
			}
		}

		#endregion // Row

		#region RowBottom

		/// <summary>
		/// Returns the Row + RowSpan.
		/// </summary>
		internal int RowBottom
		{
			get
			{
				return _row + _rowSpan;
			}
		}

		#endregion // RowBottom

		#region RowSpan

		/// <summary>
		/// Retruns the RowSpan of the item.
		/// </summary>
		public int RowSpan
		{
			get
			{
				return this._rowSpan;
			}
		}

		#endregion // RowSpan

		#endregion // Public Properties

		#region Internal Properties

		#region Size

		/// <summary>
		/// Size of the item bounds.
		/// </summary>
		internal Size Size
		{
			get
			{

				return this._bounds.Size;



			}
		}

		#endregion // Size 

		#endregion // Internal Properties

		#endregion // Properties
	}

	#endregion // GridBagLayoutItemDimensions Class

	#region GridBagLayoutItemDimensionsCollection Class

	/// <summary>
	/// For internal use. A class that contains GridBagLayoutItemDimensions instances each of which 
	/// associated with a ILayoutItem.
	/// </summary>
	public class GridBagLayoutItemDimensionsCollection : ICollection
	{
		#region Private Variables

		private double[] _columnDims;
		private double[] _rowDims;
		private Dictionary<ILayoutItem, GridBagLayoutItemDimensions> _mappings;

		
		
		private Size _preferredSize, _minimumSize, _maximumSize;

		#endregion // Private Variables

		#region Constructor






		internal GridBagLayoutItemDimensionsCollection( double[] columnDims, double[] rowDims )
		{
			if ( null == columnDims )
				throw new ArgumentNullException( "columnDims" );

			if ( null == rowDims )
				throw new ArgumentNullException( "rowDims " );

			this._columnDims = columnDims;
			this._rowDims = rowDims;
			this._mappings = new Dictionary<ILayoutItem, GridBagLayoutItemDimensions>( );
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region ColumnDims

		/// <summary>
		/// For Internal use. Column coordinates relative to left of the container rect. These can be modified freely without effecting the layout.
		/// </summary>
		public double[] ColumnDims
		{
			get
			{
				return this._columnDims;
			}
		}

		#endregion // ColumnDims

		#region MaximumSize

		
		
		/// <summary>
		/// Maximum size of the layout.
		/// </summary>
		public Size MaximumSize
		{
			get
			{
				return _maximumSize;
			}
		}

		#endregion // MaximumSize

		#region MinimumSize

		
		
		/// <summary>
		/// Minimum size of the layout.
		/// </summary>
		public Size MinimumSize
		{
			get
			{
				return _minimumSize;
			}
		}

		#endregion // MinimumSize

		#region PreferredSize

		
		
		/// <summary>
		/// Preferred size of the layout.
		/// </summary>
		public Size PreferredSize
		{
			get
			{
				return _preferredSize;
			}
		}

		#endregion // PreferredSize

		#region RowDims

		/// <summary>
		/// For Internal use. Row coordinates relative to top of the container rect. These can be modified freely without effecting the layout.
		/// </summary>
		public double[] RowDims
		{
			get
			{
				return this._rowDims;
			}
		}

		#endregion // RowDims

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods/Properties

		#region Add



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void Add( ILayoutItem key, GridBagLayoutItemDimensions itemDimensions )
		{
			_mappings[key] = itemDimensions;
		}

		#endregion // Add

		#region InitializeLayoutSizes

		
		
		internal void InitializeLayoutSizes( Size preferredSize, Size minimumSize, Size maximumSize )
		{
			_preferredSize = preferredSize;
			_minimumSize = minimumSize;
			_maximumSize = maximumSize;
		}

		#endregion // InitializeLayoutSizes

		#endregion // Private/Internal Methods/Properties

		#region Public Methods

		#region Exists

		/// <summary>
		/// For Internal use. Returns true if the collection contains an entry for the passed in layout item.
		/// </summary>
		/// <param name="key">The <see cref="ILayoutItem"/> to check for.</param>
		/// <returns>True if the collection contains an entry for the specified key.</returns>
		public bool Exists( ILayoutItem key )
		{
			return _mappings.ContainsKey( key );
		}

		#endregion // Exists

		#region Indexer

		/// <summary>
		/// For Internal use. Indexer. Returns an instance GridBagLayoutItemDimensions associated with passed in layout item.
		/// </summary>
		public GridBagLayoutItemDimensions this[ILayoutItem key]
		{
			get
			{
				GridBagLayoutItemDimensions dim;
				if ( _mappings.TryGetValue( key, out dim ) )
					return dim;

				return null;
			}
		}

		#endregion // Indexer

		#endregion // Public Methods

		#endregion // Methods

		#region Implementation of ICollection

		#region CopyTo

		void ICollection.CopyTo( System.Array array, int index )
		{
			throw new NotSupportedException( );
		}

		#endregion // CopyTo

		#region IsSynchronized

		bool ICollection.IsSynchronized
		{
			get
			{
				return ( (ICollection)_mappings ).IsSynchronized;
			}
		}

		#endregion // IsSynchronized

		#region Count

		int ICollection.Count
		{
			get
			{
				return _mappings.Count;
			}
		}

		#endregion // Count

		#region SyncRoot

		object ICollection.SyncRoot
		{
			get
			{
				return ( (ICollection)_mappings ).SyncRoot;
			}
		}

		#endregion // SyncRoot

		#endregion // Implementation of ICollection

		#region Implementation of IEnumerable

		#region GetEnumerator

		System.Collections.IEnumerator IEnumerable.GetEnumerator( )
		{
			return _mappings.Keys.GetEnumerator( );
		}

		#endregion // GetEnumerator

		#endregion // Implementation of IEnumerable
	}

	#endregion // GridBagLayoutItemDimensionsCollection Class

	#region GridBagLayoutManager Class

	/// <summary>
	/// GridBagLayoutManager class.
	/// </summary>
	public class GridBagLayoutManager : LayoutManagerBase
	{
		#region Private Data Structures

		#region AutoLayoutCalc


		// SSP 10/26/09 - NAS10.1 XamTilesControl - CalculateAutoLayout
		// 
		/// <summary>
		/// Used for calculating an auto-layout.
		/// </summary>
		internal class AutoLayoutCalc
		{
		#region Nested Data Structures

		#region CalcSizeLayoutContainer Class

			private class CalcSizeLayoutContainer : ILayoutContainer
			{
				public Rect GetBounds( object containerContext )
				{
					return (Rect)containerContext;
				}

				public void PositionItem( ILayoutItem item, Rect rect, object containerContext )
				{
				}
			}

		#endregion // CalcSizeLayoutContainer Class

		#region EnumeratorWrapper Class

			private class EnumeratorWrapper<T> : IEnumerator<T>
			{
				private IEnumerator<T> _enumerator;
				private List<T> _list;
				private int _currentIndex;
				private bool _enumeratorExhausted;

				internal EnumeratorWrapper( IEnumerator<T> enumerator )
				{
					_enumerator = enumerator;
					_list = new List<T>( );
					this.Reset( );
				}

				public T Current
				{
					get 
					{
						return _list[ _currentIndex ];
					}
				}

				public bool MoveNext( )
				{
					_currentIndex++;

					if ( _currentIndex < _list.Count )
						return true;

					if ( !_enumeratorExhausted )
					{
						if ( _enumerator.MoveNext( ) )
						{
							_list.Add( _enumerator.Current );
							return true;
						}

						_enumeratorExhausted = true;
					}

					return false;
				}

				public void Reset( )
				{
					_currentIndex = -1;
				}

		#region IEnumerator<T> Members

				object IEnumerator.Current
				{
					get 
					{
						return this.Current;
					}
				}

		#endregion

		#region IDisposable Members

				public void Dispose( )
				{
				}

		#endregion
			}

		#endregion // EnumeratorWrapper Class

		#region Line Class

			private class Line
			{
		#region Private Vars

				private AutoLayoutCalc _layout;				
				internal double _extent;
				internal int _startIndexInFittedItemsList;
				internal int _endIndexInFittedItemsList;

		#endregion // Private Vars

		#region Constructor

				/// <summary>
				/// Constructor
				/// </summary>
				internal Line( AutoLayoutCalc layout )
				{
					_layout = layout;
					_startIndexInFittedItemsList = layout._fittedItems.Count;
					_endIndexInFittedItemsList = _startIndexInFittedItemsList - 1;
				}

		#endregion // Constructor

		#region CalcExtentY

				/// <summary>
				/// Calculates the extent in y direction - height if orientation is 
				/// horizontal, width if the orientation is vertical.
				/// </summary>
				/// <param name="isFirstLine">Whether this is the first line.</param>
				/// <param name="minExtentY">Min extent y will be stored in this out param.</param>
				/// <returns>Extent in y direction of the line.</returns>
				internal double CalcExtentY( bool isFirstLine, out double minExtentY )
				{
					double maxExtentY = 0;
					minExtentY = 0;

					AutoLayoutCalc layout = _layout;
					List<GridBagConstraintCache> items = layout._fittedItems;
					for ( int i = _startIndexInFittedItemsList; i <= _endIndexInFittedItemsList; i++ )
					{
						GridBagConstraintCache gcc = items[i];
						double itemExtentY = layout.GetItemPreferredExtent( gcc, true, !isFirstLine );
						double itemMinExtentY = layout.GetItemMinExtent( gcc, true, !isFirstLine );

						maxExtentY = Math.Max( maxExtentY, itemExtentY );
						minExtentY = Math.Max( minExtentY, itemMinExtentY );
					}

					return maxExtentY;
				}

		#endregion // CalcExtentY

				internal int ItemCount
				{
					get
					{
						return 1 + _endIndexInFittedItemsList - _startIndexInFittedItemsList;
					}
				}

		#region FitItem

				/// <summary>
				/// Tries to fit the specified item in the line and if it can't then returns false.
				/// </summary>
				/// <param name="gcc">Item to fit.</param>
				/// <returns>If the item fits the remaining space in the line then returns true. Returns false otherwise.</returns>
				internal bool FitItem( GridBagConstraintCache gcc )
				{
					int itemCount = this.ItemCount;
					
					bool logicalCellConstraintViolated = _layout._forceCellsX > 0
						? itemCount >= _layout._forceCellsX
						: _layout._maxCellsX > 0 && itemCount >= _layout._maxCellsX;

					if ( logicalCellConstraintViolated )
						return false;

					bool hasInterItemSpacing = itemCount > 0;
					double itemExtentX = _layout.GetItemPreferredExtent( gcc, false, hasInterItemSpacing );
					double itemMinExtentX = _layout.GetItemMinExtent( gcc, false, hasInterItemSpacing );
					double cellExtentX = _layout.GetCellExtentX( itemCount, itemExtentX );

					if ( _layout._forceCellsX <= 0 && ! _layout._autoFitAllItems
						&& _extent + cellExtentX > _layout._layoutExtentX 
						&& itemCount > 0 && itemCount + 1 > _layout._minCellsX )
						return false;

					_layout._fittedItems.Add( gcc );
					_endIndexInFittedItemsList++;
					_layout.UpdateCellExtentX( itemCount, itemExtentX, itemMinExtentX );
					Debug.Assert( _layout._fittedItems.Count - 1 == _endIndexInFittedItemsList );
					_extent += cellExtentX;

					return true;
				}

		#endregion // FitItem
			}

		#endregion // Line Class

		#endregion // Nested Data Structures

		#region Member Vars

			private GridBagLayoutManager _lm;

			// Indicates whether the items are being laied out horizontally or vertically.
			private Orientation _layoutOrientation;

			// Indicates whether all items will be autofitted.
			private bool _autoFitAllItems;

			// The extent of the layout area in which items are to be laid out.
			private double _layoutExtentX;
			private double _layoutExtentY;

			// Min/max rows/columns.
			private int _minCellsX, _minCellsY, _maxCellsX, _maxCellsY;
			private int _forceCellsX;

			// Line represents a logical row or column in the layout and this is a list
			// of lines as we are calculating the layout.
			private List<Line> _lines = new List<Line>( );
			private List<GridBagConstraintCache> _fittedItems = new List<GridBagConstraintCache>( );

			// Used to keep track of extents of the logical columns or rows depending
			// on the orientation.
			private List<double> _cellExtentsX = new List<double>( );
			private List<double> _cellMinExtentsX = new List<double>( );

			// Cache of gcc for layout items.
			private Dictionary<ILayoutItem, GridBagConstraintCache> _gccCache = new Dictionary<ILayoutItem, GridBagConstraintCache>( );

			private Orientation? _constraintedOrientation;

		#endregion // Member Vars

		#region Constuctor

			/// <summary>
			/// Constuctor
			/// </summary>
			internal AutoLayoutCalc( GridBagLayoutManager lm, Orientation layoutOrientation, 
				bool autoFitAllItems, Size layoutExtent, int minRows, int minColumns, int maxRows, int maxColumns,
				Orientation? constraintedOrientation )
			{
				_lm = lm;
				_layoutOrientation = layoutOrientation;
				_autoFitAllItems = autoFitAllItems;
				_constraintedOrientation = constraintedOrientation;

				if ( Orientation.Horizontal == layoutOrientation )
				{
					_layoutExtentX = layoutExtent.Width;
					_layoutExtentY = layoutExtent.Height;
					_minCellsX = minColumns;
					_minCellsY = minRows;
					_maxCellsX = maxColumns;
					_maxCellsY = maxRows;
				}
				else
				{
					_layoutExtentX = layoutExtent.Height;
					_layoutExtentY = layoutExtent.Width;
					_minCellsX = minRows;
					_minCellsY = minColumns;
					_maxCellsX = maxRows;
					_maxCellsY = maxColumns;
				}
			}

		#endregion // Constuctor

		#region AutoCalculateLayout

			private static int GetNumberOfItems( IEnumerator enumerator )
			{
				int count = 0;

				enumerator.Reset( );
				while ( enumerator.MoveNext( ) )
					count++;

				return count;
			}

			internal GridBagLayoutItemDimensionsCollection AutoCalculateLayout( IEnumerator<ILayoutItem> enumerator )
			{
				enumerator = new EnumeratorWrapper<ILayoutItem>( enumerator );

				// Get the min and max cells in the x direction.
				// 
				int min = Math.Max( 1, _minCellsX );
				int max = _maxCellsX;

				// We have to perform multiple passes to re-adjust the number of logical cells that fit
				// in x direction (essentially logical columns when laying out horizontally and logical
				// rows when laying out vertically) since successive logical rows/columns may have 
				// different number of items fit it if items are variable size.
				// 
				int lastValidCellExtentX = 0;
				double lastValidCellExtentX_AutofitRatioDelta = -1;
				Size actualLayoutSize, actualMinLayoutSize;
				for ( int pass = 0; ; pass++ )
				{
					this.AutoCalculateLayoutHelper( enumerator, out actualLayoutSize, out actualMinLayoutSize );

					if ( _autoFitAllItems )
					{
						double xRatio = Math.Max( _layoutExtentX, actualMinLayoutSize.Width ) / actualLayoutSize.Width;
						double yRatio = Math.Max( _layoutExtentY, actualMinLayoutSize.Height ) / actualLayoutSize.Height;

						
						double ratioDelta = CoreUtilities.AreClose( xRatio, yRatio ) ? 0
							: Math.Abs( ( xRatio - yRatio ) / Math.Max( xRatio, yRatio ) );
						double areaDelta = ( _cellExtentsX.Count * Math.Ceiling( (double)_fittedItems.Count / _cellExtentsX.Count ) ) / _fittedItems.Count;
						double delta = ratioDelta + areaDelta;

						if ( 0 == lastValidCellExtentX 
							|| -1 == lastValidCellExtentX_AutofitRatioDelta 
							|| lastValidCellExtentX_AutofitRatioDelta > delta )
						{
							lastValidCellExtentX = _cellExtentsX.Count;
							lastValidCellExtentX_AutofitRatioDelta = delta;
						}

						// If max is set to 0 then initialize to some reasonable value.
						// 
						if ( 0 == max )
							max = _fittedItems.Count;

						// SSP 2/25/10
						// Don't let the layout cross beyond the edge of the container in the orientation
						// dimension. That is if items are being laied out vertically then don't let the
						// layout cross bottom edge of the container. When items are laid out horizontally,
						// don't let the layout cross the right edge of the container. This is especially
						// important in the tiles control where maximized area supports scrolling only in
						// one direction. Added the following two if-else-if blocks.
						// 
						if ( null != _constraintedOrientation && _constraintedOrientation.Value != _layoutOrientation
							&& actualMinLayoutSize.Width > _layoutExtentX && _cellExtentsX.Count > Math.Max( 1, _minCellsX ) )
						{
							max = _cellExtentsX.Count - 1;
							min = Math.Min( min, max );

							if ( lastValidCellExtentX > max )
								lastValidCellExtentX_AutofitRatioDelta = -1;
						}
						else if ( null != _constraintedOrientation && _constraintedOrientation.Value == _layoutOrientation
							&& actualMinLayoutSize.Height > _layoutExtentY && ( _maxCellsX <= 0 || _cellExtentsX.Count < _maxCellsX ) )
						{
							min = _cellExtentsX.Count + 1;
							max = Math.Max( min, max );

							if ( lastValidCellExtentX < min )
								lastValidCellExtentX_AutofitRatioDelta = -1;
						}
						else if ( xRatio > yRatio )
							min = _cellExtentsX.Count + 1;
						else if ( xRatio < yRatio )
							max = _cellExtentsX.Count - 1;
						else
							break;

						if ( min > max )
							break;

						// If _autoFitAllItems is true and max cols is specified then we have to
						// make sure the max rows is at least enough to accomodate all the items.
						// 
						if ( _autoFitAllItems && _maxCellsY > 0 )
						{
							int numberOfItems = GetNumberOfItems( enumerator );
							int requiredMinCellsX = (int)Math.Ceiling( (double)numberOfItems / _maxCellsY );
							min = Math.Max( min, requiredMinCellsX );
							max = Math.Max( max, requiredMinCellsX );
						}

						int estimatedCellX = ( min + max ) / 2;
						if ( estimatedCellX == _forceCellsX )
							break;

						_forceCellsX = estimatedCellX;
					}
					else
					{
						// If the auto-calculated layout extent is larger than the layout area constraint then
						// we have to adjust the number of logical columns (or rows when orientation is vertical).
						// 
						if ( actualLayoutSize.Width > _layoutExtentX )
						{
							max = _cellExtentsX.Count - 1;
						}
						else if ( actualLayoutSize.Width < _layoutExtentX && pass > 0 )
						{
							min = _cellExtentsX.Count + 1;
							lastValidCellExtentX = _cellExtentsX.Count;
						}
						else
							break;

						if ( min <= max )
						{
							int estimatedCellX = (int)( _cellExtentsX.Count * ( _layoutExtentX / actualLayoutSize.Width ) );
							if ( estimatedCellX < min || estimatedCellX > max )
								estimatedCellX = ( min + max ) / 2;

							_forceCellsX = estimatedCellX;
						}
						else
							break;
					}
				}

				if ( lastValidCellExtentX > 0 && lastValidCellExtentX != _cellExtentsX.Count )
				{
					_forceCellsX = lastValidCellExtentX;
					this.AutoCalculateLayoutHelper( enumerator, out actualLayoutSize, out actualMinLayoutSize );
				}

				return this.GetLayoutItemDimensionsHelper( );
			}

		#endregion // AutoCalculateLayout

		#region AutoCalculateLayoutHelper

			private void AutoCalculateLayoutHelper( IEnumerator<ILayoutItem> enumerator, out Size actualLayoutExtent, out Size actualMinLayoutExtent )
			{
				_lines.Clear( );
				_fittedItems.Clear( );
				_cellExtentsX.Clear( );
				_cellMinExtentsX.Clear( );

				List<double> prevCellExtentsX = null, prevMinCellExtentsX = null;
				bool allowPartialItemsOnNonZeroLogicalDim = Orientation.Horizontal == _layoutOrientation
					? !_lm.ExpandToFitHeight : !_lm.ExpandToFitWidth;

				double totalExtentY = 0, totalMinExtentY = 0;

				enumerator.Reset( );
				ILayoutItem item = enumerator.MoveNext( ) ? enumerator.Current : null;
				GridBagConstraintCache gcc = null != item ? this.GetGCC( item ) : null;
				for ( ; null != gcc ; )
				{
					// Create a new line.
					// 
					Line line = new Line( this );
					_lines.Add( line );

					// Try to fill the line with items. When an item doesn't fit, break out
					// of the loop and we'll start a new line above when the outer loop
					// executes again.
					// 					
					while ( null != gcc && line.FitItem( gcc ) )
					{
						item = enumerator.MoveNext( ) ? enumerator.Current : null;
						gcc = null != item ? this.GetGCC( item ) : null;
					}

					// Keep track of the the total extent (height if the orientation is 
					// horizontal or width if the orientation is vertical) of the lines 
					// created so far so we know to stop the layout process since all 
					// the lines have filled up the layout area.
					// 
					double minExtentY;
					double extentY = line.CalcExtentY( 1 == _lines.Count, out minExtentY );
					totalExtentY += extentY;
					totalMinExtentY += minExtentY;

					// If max rows or cols (depending on the orientation) is specified then break 
					// out when we reach that limit.
					// 
					if ( _maxCellsY > 0 && _lines.Count >= _maxCellsY )
						break;

					// Once the area is filled up with lines, break out.
					// 
					if ( !_autoFitAllItems && totalExtentY >= _layoutExtentY && _lines.Count >= _minCellsY )
					{
						if ( ! allowPartialItemsOnNonZeroLogicalDim && null != prevCellExtentsX )
						{
							_lines.RemoveAt( _lines.Count - 1 );
							_fittedItems.RemoveRange( line._startIndexInFittedItemsList, _fittedItems.Count - line._startIndexInFittedItemsList );
							_cellExtentsX = prevCellExtentsX;
							_cellMinExtentsX = prevMinCellExtentsX;
						}

						break;
					}

					if ( !allowPartialItemsOnNonZeroLogicalDim )
					{
						CloneValuesHelper<double>( _cellExtentsX, ref prevCellExtentsX );
						CloneValuesHelper<double>( _cellMinExtentsX, ref prevMinCellExtentsX );
					}
				}

				actualLayoutExtent = new Size( Sum( _cellExtentsX ), totalExtentY );
				actualMinLayoutExtent = new Size( Sum( _cellMinExtentsX ), totalMinExtentY );
			}

		#endregion // AutoCalculateLayoutHelper

		#region CloneValuesHelper

			private static void CloneValuesHelper<T>( List<T> source, ref List<T> dest )
			{
				if ( null == dest )
					dest = new List<T>( source.Count );

				for ( int i = 0, count = source.Count; i < count; i++ )
				{
					if ( i == dest.Count )
						dest.Add( source[i] );
					else
						dest[i] = source[i];
				}
			}

		#endregion // CloneValuesHelper

		#region GetGCC

			private GridBagConstraint g_emptyGCC = new GridBagConstraint( );

			/// <summary>
			/// Gets the cached gcc for the specified item. If none has been cached then 
			/// creates a new gcc and caches it.
			/// </summary>
			/// <param name="item">Layout item.</param>
			/// <returns>Gcc for specified item.</returns>
			private GridBagConstraintCache GetGCC( ILayoutItem item )
			{
				GridBagConstraintCache gcc;
				if ( _gccCache.TryGetValue( item, out gcc ) )
					return gcc;

				gcc = new GridBagConstraintCache( item, g_emptyGCC );
				_gccCache[item] = gcc;
				return gcc;
			}

		#endregion // GetGCC

		#region GetLayoutItemDimensionsHelper

			/// <summary>
			/// Returns calculated GridBagLayoutItemDimensionsCollection based on the _lines collection.
			/// </summary>
			/// <returns></returns>
			private GridBagLayoutItemDimensionsCollection GetLayoutItemDimensionsHelper( )
			{
				List<GridBagConstraintCache> fittedItems = _fittedItems;
				GridBagLayoutManager lm = _lm.Clone( false );
				for ( int iiLine = 0, linesCount = _lines.Count; iiLine < linesCount; iiLine++ )
				{
					Line line = _lines[iiLine];
					int startIndex = line._startIndexInFittedItemsList;
					int endIndex = line._endIndexInFittedItemsList;
					for ( int i = startIndex; i <= endIndex; i++ )
					{
						GridBagConstraintCache gcc = fittedItems[i];
						GridBagConstraint gc = new GridBagConstraint( );

						if ( Orientation.Horizontal == _layoutOrientation )
						{
							gc.Row = iiLine;
							gc.Column = i - startIndex;
						}
						else
						{
							gc.Row = i - startIndex;
							gc.Column = iiLine;
						}

						lm.LayoutItems.Add( gcc._item, gc );
					}
				}

				Size size;
				if ( Orientation.Horizontal == _layoutOrientation )
					size = new Size( _layoutExtentX, _layoutExtentY );
				else
					size = new Size( _layoutExtentY, _layoutExtentX );

				return lm.GetLayoutItemDimensions( new CalcSizeLayoutContainer( ), new Rect( 0, 0, size.Width, size.Height ) );
			}

		#endregion // GetLayoutItemDimensionsHelper

		#region GetItemMinExtent

			private double GetItemMinExtent( GridBagConstraintCache gcc, bool oppositeOrientation, bool includeSpacing )
			{
				bool flag = Orientation.Horizontal == _layoutOrientation ^ oppositeOrientation;

				double extent = flag ? gcc._minWidth : gcc._minHeight;
				Thickness margin = gcc._margin;
				extent += flag ? ( margin.Left + margin.Right ) : ( margin.Top + margin.Bottom );

				if ( includeSpacing )
					extent += flag ? _lm._interItemSpacingHorizontal : _lm._interItemSpacingVertical;

				return extent;
			}

		#endregion // GetItemMinExtent

		#region GetItemPreferredExtent

			private double GetItemPreferredExtent( GridBagConstraintCache gcc, bool oppositeOrientation, bool includeSpacing )
			{
				bool flag = Orientation.Horizontal == _layoutOrientation ^ oppositeOrientation;

				double extent = flag ? gcc._prefWidth : gcc._prefHeight;
				if ( !_lm._preferredSizeIncludesMargin )
				{
					Thickness margin = gcc._margin;
					extent += flag ? ( margin.Left + margin.Right ) : ( margin.Top + margin.Bottom );
				}

				if ( includeSpacing )
					extent += flag ? _lm._interItemSpacingHorizontal : _lm._interItemSpacingVertical;

				return extent;
			}

		#endregion // GetItemPreferredExtent

		#region Sum

			private static double Sum( List<double> list )
			{
				double total = 0;

				for ( int i = 0, count = list.Count; i < count; i++ )
					total += list[i];

				return total;
			}

		#endregion // Sum

		#region UpdateCellExtentX

			private void UpdateCellExtentX( int cellX, double itemExtentX, double itemMinExtentX )
			{
				Debug.Assert( cellX <= _cellExtentsX.Count );
				if ( cellX == _cellExtentsX.Count )
				{
					_cellExtentsX.Add( 0 );
					_cellMinExtentsX.Add( 0 );
				}

				_cellExtentsX[cellX] = Math.Max( _cellExtentsX[cellX], itemExtentX );
				_cellMinExtentsX[cellX] = Math.Max( _cellMinExtentsX[cellX], itemMinExtentX );
			}

			private double GetCellExtentX( int cellX, double itemExtentX )
			{
				if ( cellX < _cellExtentsX.Count )
					return Math.Max( itemExtentX, _cellExtentsX[cellX] );

				return itemExtentX;
			}

		#endregion // UpdateCellExtentX
		}

		#endregion // AutoLayoutCalc

		#region GridBagConstraintCache Class

		private class GridBagConstraintCache
		{
			#region Variables

			internal IGridBagConstraint _gc;
			internal ILayoutItem _item;
			internal int _column, _row, _columnSpan, _rowSpan;
			internal Thickness _margin;

			internal double _minWidth, _minHeight;
			internal double _maxWidth, _maxHeight;
			internal double _prefWidth, _prefHeight;

			internal int _gcColumn, _gcRow, _gcColumnSpan, _gcRowSpan;

			internal float _columnWeight, _rowWeight;

			// SSP 7/22/09 - NAS9.2 Field Auto-sizing
			// 
			internal bool _isAutoWidth, _isAutoHeight;

			// SSP 11/18/09 - NAS10.1 XamTilesControl
			// 
			internal Side _hasSpacing;

			#endregion // Variables

			#region Constructor



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			internal GridBagConstraintCache( ILayoutItem item, IGridBagConstraint gc )
			{
				this._item = item;
				this._gc = gc;
				this._gcColumn = this._column = gc.Column;
				this._gcRow = this._row = gc.Row;
				this._gcColumnSpan = this._columnSpan = gc.ColumnSpan;
				this._gcRowSpan = this._rowSpan = gc.RowSpan;
				this._columnWeight = gc.ColumnWeight;
				this._rowWeight = gc.RowWeight;
				this._margin = gc.Margin;

				Size minSize = item.MinimumSize;
				Size prefSize = item.PreferredSize;
				Size maxSize = item.MaximumSize;

				_minWidth = minSize.Width;
				_minHeight = minSize.Height;
				_maxWidth = Math.Max( maxSize.Width, _minWidth );
				_maxHeight = Math.Max( maxSize.Height, _minHeight );
				_prefWidth = Math.Max( Math.Min( prefSize.Width, _maxWidth ), _minWidth );
				_prefHeight = Math.Max( Math.Min( prefSize.Height, _maxHeight ), _minHeight );
				
				// SSP 7/22/09 - NAS9.2 Field Auto-sizing
				// 
				IAutoSizeLayoutItem itemEx = item as IAutoSizeLayoutItem;
				_isAutoWidth = null != itemEx && itemEx.IsWidthAutoSized;
				_isAutoHeight = null != itemEx && itemEx.IsHeightAutoSized;
			}

			#endregion // Constructor

			#region Methods

			#region GetMarginWithSpacing

			// SSP 11/18/09 - NAS10.1 XamTilesControl
			// Added inter-item-spacing properties.
			// 
			internal Thickness GetMarginWithSpacing( GridBagLayoutManager lm )
			{
				Thickness ret = _margin;

				if ( Side.None != _hasSpacing )
				{
					Thickness spacing = this.GetSpacing( lm );

					ret.Left += spacing.Left;
					ret.Top += spacing.Top;
					ret.Right += spacing.Right;
					ret.Bottom += spacing.Bottom;
				}

				return ret;
			}

			#endregion // GetMarginWithSpacing

			#region GetSpacing

			// SSP 11/18/09 - NAS10.1 XamTilesControl
			// Added inter-item-spacing properties.
			// 
			internal Thickness GetSpacing( GridBagLayoutManager lm )
			{
				Side hasSpacing = _hasSpacing;

				Thickness ret = new Thickness(
					0 != ( Side.Left & hasSpacing ) ? lm._interItemSpacingHorizontal / 2 : 0,
					0 != ( Side.Top & hasSpacing ) ? lm._interItemSpacingVertical / 2 : 0,
					0 != ( Side.Right & hasSpacing ) ? lm._interItemSpacingHorizontal / 2 : 0,
					0 != ( Side.Bottom & hasSpacing ) ? lm._interItemSpacingVertical / 2 : 0 );

				return ret;
			}

			#endregion // GetSpacing

			#endregion // Methods
		}

		#endregion // GridBagConstraintCache Class

		#region GridBagLayoutCache Class

		private class GridBagLayoutCache
		{
			internal GridBagLayoutCache(
				GridBagLayoutManager.GridBagConstraintCache[] gccArr,
				float[] colWeights,
				float[] rowWeights,
				double[] colWidthsPref,
				double[] rowHeightsPref,
				double[] colWidthsMin,
				double[] rowHeightsMin,
				double[] colWidthsMax,
				double[] rowHeightsMax,
				// SSP 7/23/09 - NAS9.2 Auto-sizing
				// 
				bool[] colIsAuto,
				bool[] rowIsAuto )
			{
				this._gccArr = gccArr;
				this._colWeights = colWeights;
				this._rowWeights = rowWeights;
				this._colWidthsPref = colWidthsPref;
				this._rowHeightsPref = rowHeightsPref;
				this._colWidthsMin = colWidthsMin;
				this._rowHeightsMin = rowHeightsMin;
				this._colWidthsMax = colWidthsMax;
				this._rowHeightsMax = rowHeightsMax;

				// SSP 7/23/09 - NAS9.2 Auto-sizing
				// 
				this._colIsAuto = colIsAuto;
				this._rowIsAuto = rowIsAuto;
			}

			internal GridBagLayoutManager.GridBagConstraintCache[] _gccArr;
			internal float[] _colWeights;
			internal float[] _rowWeights;
			internal double[] _colWidthsPref;
			internal double[] _rowHeightsPref;
			internal double[] _colWidthsMin;
			internal double[] _rowHeightsMin;
			internal double[] _colWidthsMax;
			internal double[] _rowHeightsMax;

			// Variables for caching the actual rects.
			//
			internal double _lastContainerWidth;
			internal double _lastContainerHeight;
			internal double[] _colCoordinates;
			internal double[] _rowCoordinates;
			// SSP 2/16/10 TFS27086
			// 
			internal double[] _colCoordinateExtents;
			internal double[] _rowCoordinateExtents;

			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// Added the concept of an item being auto-sized and therefore should be
			// shrunk or expanded when proportionally expanding or contracting items
			// when the container extent is different than the preferred layout extent.
			// 
			internal bool[] _colIsAuto, _rowIsAuto;
		}

		#endregion // GridBagLayoutCache Class

		#region IntExpandableArray Class







		private class IntExpandableArray
		{
			#region Constants

			private const int ELEMS_PER_ARR = 64;

			#endregion // Constants

			#region Private Variables

			private List<int[]> _list;
			private int _length;

			#endregion // Private Variables

			#region Constructor







			internal IntExpandableArray( int initialCapacity )
			{
				// AS 9/29/09 Optimization
				// This could allocate 1 more than was needed.
				//
				//this._list = new List<int[]>( );
				//
				//this._list.Capacity = 1 + initialCapacity / ELEMS_PER_ARR;
				this._list = new List<int[]>((initialCapacity - 1) / ELEMS_PER_ARR + 1);
			}

			#endregion // Constructor

			#region GetArr

			private int[] GetArr( int index )
			{
				int listIndex = index / ELEMS_PER_ARR;

				while ( _list.Count <= listIndex )
					_list.Add( new int[ELEMS_PER_ARR] );

				return _list[listIndex];
			}

			#endregion // GetArr

			#region Length






			internal int Length
			{
				get
				{
					return this._length;
				}
			}

			#endregion // Length

			#region Indexer







			internal int this[int index]
			{
				get
				{
					// Update the length.
					//
					this._length = Math.Max( 1 + index, _length );

					int[] arr = this.GetArr( index );
					return arr[index % ELEMS_PER_ARR];
				}
				set
				{
					// Update the length.
					//
					this._length = Math.Max( 1 + index, _length );

					int[] arr = this.GetArr( index );
					arr[index % ELEMS_PER_ARR] = value;
				}
			}

			#endregion // Indexer

			// AS 9/29/09 Optimization
			// We can use the Initialize method to reset the array values to 0 
			// instead of enumerating them.
			//
			#region Reset
			internal void Reset()
			{
				for (int i = 0, count = _list.Count; i < count; i++)
					_list[i].Initialize();
			}
			#endregion //Reset
		}

		#endregion // IntExpandableArray Class

		#region IntPoint Class

		private class IntPoint
		{
			private int _x, _y;

			public IntPoint( )
				: this( 0, 0 )
			{
			}

			public IntPoint( int x, int y )
			{
				_x = x;
				_y = y;
			}

			internal void SetXY( int x, int y )
			{
				_x = x;
				_y = y;
			}

			internal int X
			{
				get
				{
					return _x;
				}
			}

			internal int Y
			{
				get
				{
					return _y;
				}
			}

			public override int GetHashCode( )
			{
				return _x ^ _y;
			}

			public override bool Equals( object obj )
			{
				if ( obj is IntPoint )
				{
					IntPoint ip = (IntPoint)obj;
					return _x == ip._x && _y == ip._y;
				}

				return false;
			}

			
#region Infragistics Source Cleanup (Region)






















































































#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // IntPoint Class

		#region LayoutItemComparer Class

		private class LayoutItemComparer : IComparer<GridBagConstraintCache>
		{
			private bool _yWise;
			private GridBagLayoutManager _gblm;

			internal LayoutItemComparer( GridBagLayoutManager gblm, bool yWise )
			{
				this._yWise = yWise;
				this._gblm = gblm;
			}

			internal static int Compare( GridBagConstraintCache gcc1, GridBagConstraintCache gcc2, GridBagLayoutManager gblm, bool yWise, bool flag )
			{
				// SSP 4/7/11 TFS32232
				// Added the if block and enclosed the existing code into the else.
				// 
				if ( GridBagLayoutMode.Uniform == gblm.LayoutMode )
				{
					if ( yWise )
					{
						if ( gcc1._rowSpan < gcc2._rowSpan )
							return -1;
						else if ( gcc1._rowSpan > gcc2._rowSpan )
							return 1;
						else if ( gcc1._row < gcc2._row )
							return -1;
						else if ( gcc1._row > gcc2._row )
							return 1;
					}
					else
					{
						if ( gcc1._columnSpan < gcc2._columnSpan )
							return -1;
						else if ( gcc1._columnSpan > gcc2._columnSpan )
							return 1;
						else if ( gcc1._column < gcc2._column )
							return -1;
						else if ( gcc1._column > gcc2._column )
							return 1;
					}
				}
				else if ( GridBagLayoutMode.LeftToRight == gblm.LayoutMode )
				{
					if ( yWise )
					{
						if ( gcc1._row + gcc1._rowSpan < gcc2._row + gcc2._rowSpan )
							return -1;
						else if ( gcc1._row + gcc1._rowSpan > gcc2._row + gcc2._rowSpan )
							return 1;
						else if ( gcc1._row < gcc2._row )
							return -1;
						else if ( gcc1._row > gcc2._row )
							return 1;
					}
					else
					{
						if ( gcc1._column + gcc1._columnSpan < gcc2._column + gcc2._columnSpan )
							return -1;
						else if ( gcc1._column + gcc1._columnSpan > gcc2._column + gcc2._columnSpan )
							return 1;
						else if ( gcc1._column < gcc2._column )
							return -1;
						else if ( gcc1._column > gcc2._column )
							return 1;
					}
				}
				else
				{
					if ( yWise )
					{
						if ( gcc1._rowSpan < gcc2._rowSpan )
							return -1;
						else if ( gcc1._rowSpan > gcc2._rowSpan )
							return 1;
						else if ( gcc1._row < gcc2._row )
							return -1;
						else if ( gcc1._row > gcc2._row )
							return 1;
					}
					else
					{
						if ( gcc1._columnSpan < gcc2._columnSpan )
							return -1;
						else if ( gcc1._columnSpan > gcc2._columnSpan )
							return 1;
						else if ( gcc1._column < gcc2._column )
							return -1;
						else if ( gcc1._column > gcc2._column )
							return 1;
					}
				}

				if ( flag )
					return Compare( gcc1, gcc2, gblm, !yWise, false );

				return 0;
			}

			public int Compare( GridBagConstraintCache o1, GridBagConstraintCache o2 )
			{
				return Compare( o1, o2, this._gblm, this._yWise, true );
			}
		}

		#endregion // LayoutItemComparer Class

		#region ResizeManager Class

		private class ResizeManager
		{
		#region Nested Data Structures

		#region LayoutItemInfo Class

			// Resizing logic creates a clone layout manager and clone items and resizes these 
			// clone items while in the process of calculating the new preferred sizes of items
			// affected as the result of resize operation.
			// 
			/// <summary>
			/// This layout item wraps an item from the actual layout manager.
			/// </summary>
			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// Added IAutoSizeLayoutItem interface to support auto-sized items.
			// 
			//private class LayoutItemInfo : ILayoutItem
			private class LayoutItemInfo : IAutoSizeLayoutItem
			{
				/// <summary>
				/// Original layout item.
				/// </summary>
				internal ILayoutItem _i;

				/// <summary>
				/// Current preferred size. This may be different from the original preferred size of the
				/// underlying layout item.  Since resize process may be multi-pass, this value will be 
				/// updated to reflect the current item dimensions during each pass.
				/// </summary>
				internal Size _preferredSize;

				/// <summary>
				/// Cached GridBagConstraintCache associated with this layout item. This is simply for 
				/// efficiency reasons as the gcc doesn't have to be retrieved from the 
				/// GridBagLayoutItemDimensionsCollection.
				/// </summary>
				internal GridBagConstraintCache _gcc;

				/// <summary>
				/// Cached current dimension of the item. Since resize process may be multi-pass, this
				/// value will be updated to reflect the current item dimensions during each pass.
				/// </summary>
				internal GridBagLayoutItemDimensions _dim;

				// SSP 7/23/09 - NAS9.2 Auto-sizing
				// Added IAutoSizeLayoutItem interface to support auto-sized items.
				// 
				internal bool _isAutoWidth, _isAutoHeight;

				internal LayoutItemInfo( ILayoutItem i )
				{
					_i = i;
					_preferredSize = i.PreferredSize;

					// SSP 7/23/09 - NAS9.2 Auto-sizing
					// Added IAutoSizeLayoutItem interface to support auto-sized items.
					// 
					IAutoSizeLayoutItem iex = i as IAutoSizeLayoutItem;
					_isAutoWidth = null != iex && iex.IsWidthAutoSized;
					_isAutoHeight = null != iex && iex.IsHeightAutoSized;
				}

				public Visibility Visibility
				{
					get 
					{
						return _i.Visibility;
					}
				}

				public Size MaximumSize
				{
					get 
					{
						return _i.MaximumSize;
					}
				}

				public Size MinimumSize
				{
					get 
					{
						return _i.MinimumSize;
					}
				}

				public Size PreferredSize
				{
					get 
					{
						return _preferredSize;
					}
				}

				// SSP 7/23/09 - NAS9.2 Auto-sizing
				// Added IAutoSizeLayoutItem interface to support auto-sized items.
				// 
				public bool IsWidthAutoSized
				{
					get
					{
						return _isAutoWidth;
					}
				}

				// SSP 7/23/09 - NAS9.2 Auto-sizing
				// Added IAutoSizeLayoutItem interface to support auto-sized items.
				// 
				public bool IsHeightAutoSized
				{
					get
					{
						return _isAutoHeight;
					}
				}
			}

			#endregion // LayoutItemInfo Class

		#endregion // Nested Data Structures

		#region Member Vars

			private GridBagLayoutManager _layoutManager;
			private ILayoutContainer _layoutContainer;
			private object _containerContext;
			private bool _autoFitWidth;
			private bool _autoFitHeight;

			
			
			
			
			/// <summary>
			/// Array of LayoutItemInfo where each LayoutItemInfo wraps the original item 
			/// and caches various information about it.
			/// </summary>
			private LayoutItemInfo[] _infos;

			/// <summary>
			/// Gridbag layout cache.
			/// </summary>
			private GridBagLayoutCache _glc;

			/// <summary>
			/// Mapping of original layout items to LayoutItemInfo objects that wrap those layout items.
			/// </summary>
			private Dictionary<ILayoutItem, LayoutItemInfo> _mappings;

			/// <summary>
			/// Used to cache the items that end at a specific logical column or row.
			/// </summary>
			private Dictionary<int, LayoutItemInfo[]> _itemsEndingAtCache = new Dictionary<int, LayoutItemInfo[]>( );

			/// <summary>
			/// Indicates that the resizing operation for which this manager instance is created is for auto-sizing.
			/// </summary>
			internal bool _isAutoSizing = false;

			// SSP 2/15/10 TFS27405
			// 
			internal int _resizeStrategy = 1;

			#endregion // Member Vars

		#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="layoutManager">Layout manager.</param>
			/// <param name="layoutContainer">Layout container.</param>
			/// <param name="containerContext">Layout container context.</param>
			/// <param name="autoFitWidth">Specifies whether items will be autofitted horizontally within the 
			/// layout container rect. Items will be resized in such a way that the width of the preferred size
			/// of the resultant layout will be equal container rect's width.</param>
			/// <param name="autoFitHeight">Specifies whether items will be autofitted vertically within the 
			/// layout container rect. Items will be resized in such a way that the height of the preferred size
			/// of the resultant layout will be equal container rect's height.</param>
			internal ResizeManager( GridBagLayoutManager layoutManager, ILayoutContainer layoutContainer, 
				object containerContext, bool autoFitWidth, bool autoFitHeight )
			{
				_layoutContainer = layoutContainer;
				_containerContext = containerContext;
				_autoFitWidth = autoFitWidth;
				_autoFitHeight = autoFitHeight;

				_layoutManager = this.CloneLayoutManagerHelper( layoutManager );

				
				
				this.ReCalculateLayout( );
			}

			#endregion // Constructor

		#region Methods

		#region Public Methods

		#region GetResizeRange

			public void GetResizeRange( ILayoutItem resizeItem, out double maxDeltaLeft, out double maxDeltaRight,
				out double maxDeltaTop, out double maxDeltaBottom,
				// SSP 7/10/09 NAS9.2 Synchronous Sizing
				// Added capability of resizing multiple items at the same time in a synchronous manner.
				// 
				IList<ILayoutItem> synchronizedItems 
				)
			{
				// SSP 7/10/09 NAS9.2 Synchronous Sizing
				// 
				// --------------------------------------------------------------------------------------------------
				//this.GetResizeRangeHelper( this.GetMappedItem( resizeItem ), out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom );
				LayoutItemInfo[] resizeItems;
				this.GetResizeItemInfosHelper( resizeItem, synchronizedItems, out resizeItems );

				this.GetResizeRangeHelper( resizeItems, out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom );
				// --------------------------------------------------------------------------------------------------
			}

			#endregion // GetResizeRange

		#region ResizeItems

			// SSP 7/22/09 NAS9.2 Auto-sizing
			// Added new ResizeItems method, which is used to auto-size multiple items however
			// it can also be used to resize multiple items at the same time.
			// 
			/// <summary>
			/// Resizes the items to their new sizes.
			/// </summary>
			/// <param name="newSizes">Specifies the items to resize and their new sizes.</param>
			public void ResizeItems( Dictionary<ILayoutItem, Size> newSizes )
			{
				List<LayoutItemInfo> xItems = new List<LayoutItemInfo>( );
				List<Size> xSizes = new List<Size>( );

				List<LayoutItemInfo> yItems = new List<LayoutItemInfo>( );
				List<Size> ySizes = new List<Size>( );

				foreach ( KeyValuePair<ILayoutItem, Size> ii in newSizes )
				{
					LayoutItemInfo itemInfo = this.GetMappedItem( ii.Key );
					Size newSize = ii.Value;
					if ( null != itemInfo )
					{
						if ( newSize.Width > 0 )
						{
							xItems.Add( itemInfo );
							xSizes.Add( newSize );
						}

						if ( newSize.Height > 0 )
						{
							yItems.Add( itemInfo );
							ySizes.Add( newSize );
						}
					}
				}

				this.ResizeItemsHelper( xItems.ToArray( ), xSizes.ToArray( ), true, false );
				this.ResizeItemsHelper( yItems.ToArray( ), ySizes.ToArray( ), false, true );
			}

			#endregion // ResizeItems

		#region ResizeItem

			// SSP 7/29/09 NAS9.2 Auto-sizing
			// Changed method to return void instead of the new sizes. Instead the GetNewPreferredSizes
			// method should be called to get the new preferred sizes. Also added the synchronizedItems 
			// parameter to be able to resize multiple items at the same time in a synchronous manner.
			// 
			//public Dictionary<ILayoutItem, Size> ResizeItem( ILayoutItem item, double deltaX, double deltaY )
			public void ResizeItem( ILayoutItem item, double deltaX, double deltaY, IList<ILayoutItem> synchronizedItems )
			{
				// SSP 7/10/09 NAS9.2 Synchronous Sizing
				// Added capability of resizing multiple items at the same time in a synchronous manner.
				// 
				//this.ResizeItemHelper( this.GetMappedItem( item ), deltaX, deltaY );
				this.ResizeItemHelper( item, deltaX, deltaY, synchronizedItems );
			}

			#endregion // ResizeItem

			#endregion // Public Methods

		#region Private/Internal Methods

		#region AddMargin

			private void AddMargin( ref Size sz, LayoutItemInfo ii )
			{
				// SSP 11/18/09 - NAS10.1 XamTilesControl
				// Added inter-item-spacing properties.
				// 
				//Thickness m = ii._gcc._margin;
				Thickness m = ii._gcc.GetMarginWithSpacing( _layoutManager ); 

				sz.Width += m.Left + m.Right;
				sz.Height += m.Top + m.Bottom;
			}

			#endregion // AddMargin

		#region AreClose

			private static bool AreClose( Size x, Size y, double threshold )
			{
				return Math.Abs( x.Width - y.Width ) <= threshold
					&& Math.Abs( x.Height - y.Height ) <= threshold;
			}

			private static bool AreClose( double x, double y, double threshold )
			{
				return Math.Abs( x - y ) <= threshold;
			}

			#endregion // AreClose

		#region AreSame

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			private static bool AreSame( Dictionary<LayoutItemInfo, Size> xx, Dictionary<LayoutItemInfo, Size> yy )
			{
				if ( xx.Count != yy.Count )
					return false;

				foreach ( KeyValuePair<LayoutItemInfo, Size> ii in xx )
				{
					Size val;
					if ( ! yy.TryGetValue( ii.Key, out val ) || ! CoreUtilities.AreClose( ii.Value, val ) )
						return false;
				}

				return true;
			}

			#endregion // AreSame

		#region ArrayMove<X, Y>

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// Added capability of resizing multiple items at the same time in a synchronous manner.
			// 
			private static void ArrayMove<X, Y>( X[] xxArr, Y[] yyArr, int[] indeces )
			{
				for ( int c = 0; c < indeces.Length; c++ )
				{
					int index = indeces[c];

					CoreUtilities.Swap<X>( xxArr, index, c );
					CoreUtilities.Swap<Y>( yyArr, index, c );
					CoreUtilities.Swap<int>( indeces, index, c );
				}
			}

			#endregion // ArrayMove<X, Y>

		#region CalcDeltaHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			private double CalcDeltaHelper( LayoutItemInfo[] resizeItems, Size[] newSizes, ref int startIndex, bool yDelta )
			{
				double delta = double.NaN;

				LayoutItemInfo item = resizeItems[startIndex];

				int edge = !yDelta ? item._dim.ColumnRight : item._dim.RowBottom;

				while ( startIndex < resizeItems.Length )
				{
					item = resizeItems[startIndex];

					int iiEdge = !yDelta ? item._dim.ColumnRight : item._dim.RowBottom;
					if ( iiEdge != edge )
						break;

					Size newSize = newSizes[startIndex];

					double iiDelta = !yDelta
						? newSize.Width - item._dim.Bounds.Width
						: newSize.Height - item._dim.Bounds.Height;

					if ( double.IsNaN( delta ) )
						delta = iiDelta;
					// When multiple items aligned at the same edge are resized 
					// to different sizes, resize them to the biggest size.
					// 
					else if ( delta < iiDelta )
						delta = iiDelta;

					startIndex++;
				}

				return CloseToZeroHelper( delta );
			}

			#endregion // CalcDeltaHelper

		#region CalcNewSizeHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// Refactored. Moved existing code from ResizeItemHelper into the new CalcNewSizeHelper method
			// 
			private Size CalcNewSizeHelper( LayoutItemInfo item, double deltaX, double deltaY )
			{
				Size newSize = item._dim.Size;






				newSize.Width = Math.Max( newSize.Width + deltaX, 0 );
				newSize.Height = Math.Max( newSize.Height + deltaY, 0 );

				return newSize;
			}

			#endregion // CalcNewSizeHelper

		#region ClearIsAutoState

			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// 
			private bool ClearIsAutoState( LayoutItemInfo[] items, 
				bool clearAutoWidthState, 
				bool clearAutoHeightState, 
				bool alsoClearAlignedItems,
				bool recalcLayout )
			{
				bool stateChanged = false;

				if ( !alsoClearAlignedItems )
				{
					for ( int i = 0; i < items.Length; i++ )
					{
						LayoutItemInfo ii = items[i];

						if ( clearAutoWidthState && ii._isAutoWidth )
						{
							ii._isAutoWidth = false;
							stateChanged = true;
						}

						if ( clearAutoHeightState && ii._isAutoHeight )
						{
							ii._isAutoHeight = false;
							stateChanged = true;
						}
					}
				}
				else
				{

					var xxProcessedItems = new HashSet<LayoutItemInfo>();
					var yyProcessedItems = new HashSet<LayoutItemInfo>();





					for ( int i = 0; i < items.Length; i++ )
					{
						LayoutItemInfo ii = items[i];

						if ( clearAutoWidthState && !xxProcessedItems.Contains( ii ) )
						{
							LayoutItemInfo[] tmpArr = this.GetItemsEndingAt( ii._dim.ColumnRight, false );
							stateChanged = this.ClearIsAutoState( tmpArr, true, false, false, false ) || stateChanged;
							for (int j = 0; j < tmpArr.Length; j++)
								xxProcessedItems.Add(tmpArr[j]);
						}

						if ( clearAutoHeightState && !yyProcessedItems.Contains( ii ) )
						{
							LayoutItemInfo[] tmpArr = this.GetItemsEndingAt( ii._dim.RowBottom, true );
							stateChanged = this.ClearIsAutoState( tmpArr, false, true, false, false ) || stateChanged;
							for (int j = 0; j < tmpArr.Length; j++)
								yyProcessedItems.Add(tmpArr[j]);
						}
					}
				}

				if ( recalcLayout && stateChanged )
					this.ReCalculateLayout( );

				return stateChanged;
			}

			#endregion // ClearIsAutoState

		#region CloneLayoutManagerHelper

			private GridBagLayoutManager CloneLayoutManagerHelper( GridBagLayoutManager source )
			{
				GridBagLayoutManager dest = source.Clone( false );

				// Set IncludeMarginInPositionRect to false. Logic in resize methods assume that the
				// gridbag layout item dimensions do not include margins.
				// 
				dest.IncludeMarginInPositionRect = false;

				LayoutItemsCollection srcItems = source.LayoutItems;
				LayoutItemsCollection destItems = dest.LayoutItems;

				foreach ( ILayoutItem ii in srcItems )
					destItems.Add( new LayoutItemInfo( ii ), srcItems.GetConstraint( ii ) );

				return dest;
			}

			#endregion // CloneLayoutManagerHelper

		#region CloseToZeroHelper

			private static double CloseToZeroHelper( double d )
			{
				if ( CoreUtilities.AreClose( d, 0.0 ) )
					d = 0.0;

				return d;
			}

			#endregion // CloseToZeroHelper

		#region CreateCoordinates

			private double[] CreateCoordinates( double[] extents )
			{
				double[] coordinates = new double[1 + extents.Length];
				Array.Copy( extents, coordinates, extents.Length );

				GridBagLayoutManager.ConvertToCoordinates( coordinates );

				return coordinates;
			}

			#endregion // CreateCoordinates

		#region CreateSizeArr

			private Size[] CreateSizeArr( Size size, int len )
			{
				Size[] arr = new Size[len];
				for ( int i = 0; i < len; i++ )
					arr[i] = size;

				return arr;
			}

			#endregion // CreateSizeArr

		#region GetCellExtent

			private double GetCellExtent( double[] coordinates, int origin, int span )
			{
				return coordinates[origin + span] - coordinates[origin];
			}

			#endregion // GetCellExtent

		#region GetInfo

			
			
			
			
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


			#endregion // GetInfo

		#region GetItemsEndingAt

			// SSP 7/22/09 NAS9.2 Auto-sizing
			// Moved the existing code from here into the new GetNewPreferredSizes method.
			// 			
			/// <summary>
			/// Gets the items that end at the specified edge (logical column or row depending 
			/// on the yDimension parameter).
			/// </summary>
			/// <param name="edge">If yDimension is true then logical column, otherwise logical row.</param>
			/// <param name="yDimension">If yDimension is true then edge parameter designates logical column, otherwise it designates logical row.</param>
			/// <returns></returns>
			private LayoutItemInfo[] GetItemsEndingAt( int edge, bool yDimension )
			{
				LayoutItemInfo[] ret;
				int cacheKey = !yDimension ? edge : -edge - 1;
				if ( _itemsEndingAtCache.TryGetValue( cacheKey, out ret ) )
					return ret;

				LayoutItemInfo[] info = _infos;
				List<LayoutItemInfo> list = new List<LayoutItemInfo>( );

				for ( int i = 0; i < info.Length; i++ )
				{
					LayoutItemInfo ii = info[i];
					GridBagLayoutItemDimensions dim = ii._dim;

					int iiEdge = ! yDimension ? ii._dim.ColumnRight : ii._dim.RowBottom;
					if ( iiEdge == edge )
						list.Add( ii );
				}

				ret = list.ToArray( );
				_itemsEndingAtCache[cacheKey] = ret;

				return ret;
			}

			#endregion // GetItemsEndingAt

		#region GetMappedItem

			private LayoutItemInfo GetMappedItem( ILayoutItem item )
			{
				// SSP 7/10/09 NAS9.2 Synchronous Sizing - Optimizations
				// Added capability of resizing multiple items at the same time in a synchronous manner.
				// 
				// ----------------------------------------------------------------------------------------
				LayoutItemInfo info;
				if ( ! _mappings.TryGetValue( item, out info ) )
					throw new ArgumentException( SR.GetString( "LE_GridBag_ItemNotInLayoutItems", item ) );

				if ( null == info._dim )
					throw new ArgumentException( SR.GetString( "LE_GridBag_ItemNotVisible", item ) );

				return info;

				
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

				// ----------------------------------------------------------------------------------------
			}

			#endregion // GetMappedItem

		#region GetNewPreferredSizes

			// SSP 7/22/09 NAS9.2 Auto-sizing
			// Moved code from ResizeItem into the new GetNewPreferredSizes so it can be reused in the
			// new ResizeItems method.
			// 
			internal Dictionary<ILayoutItem, Size> GetNewPreferredSizes( 
				out Dictionary<ILayoutItem, bool> newIsWidthAutoState,
				out Dictionary<ILayoutItem, bool> newIsHeightAutoState,
				bool xDimension, bool yDimension )
			{
				Dictionary<ILayoutItem, Size> newPreferredSizes = new Dictionary<ILayoutItem, Size>( );
				newIsWidthAutoState = new Dictionary<ILayoutItem, bool>( );
				newIsHeightAutoState = new Dictionary<ILayoutItem, bool>( );

				foreach ( LayoutItemInfo ii in _layoutManager.LayoutItems )
				{
					if ( null != ii._gcc )
					{
						IAutoSizeLayoutItem iex = ii._i as IAutoSizeLayoutItem;
						bool origIsAutoWidth = null != iex && iex.IsWidthAutoSized;
						bool origIsAutoHeight = null != iex && iex.IsHeightAutoSized;

						Size oldSize = ii._i.PreferredSize;
						Size newSize = oldSize;

						if ( xDimension )
						{
							if ( oldSize.Width != ii._preferredSize.Width )
								newSize.Width = ii._preferredSize.Width;

							if ( origIsAutoWidth != ii._isAutoWidth )
								newIsWidthAutoState[ii._i] = ii._isAutoWidth;
						}

						if ( yDimension )
						{
							if ( oldSize.Height != ii._preferredSize.Height )
								newSize.Height = ii._preferredSize.Height;

							if ( origIsAutoHeight != ii._isAutoHeight )
								newIsHeightAutoState[ii._i] = ii._isAutoHeight;
						}

						if ( newSize != oldSize )
							newPreferredSizes[ii._i] = newSize;
					}
				}

				return newPreferredSizes;
			}

			#endregion // GetNewPreferredSizes

		#region GetPreferredSizes

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			private Dictionary<LayoutItemInfo, Size> GetPreferredSizes( LayoutItemInfo[] items )
			{
				Dictionary<LayoutItemInfo, Size> map = new Dictionary<LayoutItemInfo, Size>( );

				for ( int i = 0; i < items.Length; i++ )
				{
					LayoutItemInfo ii = items[i];
					map[ii] = ii._preferredSize;
				}

				return map;
			}

			#endregion // GetPreferredSizes

		#region GetPreferredToActualRatio

			
			
			
			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			#endregion // GetPreferredToActualRatio

		#region GetResizeItemInfosHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			/// <summary>
			/// A helper method that takes in a resizeItem and synchronizedItems and returns an array of
			/// LayoutItemInfo objects associated with resizeItem and synchronizedItems combined, making
			/// sure not duplicating the item.
			/// </summary>
			/// <param name="resizeItem"></param>
			/// <param name="synchronizedItems"></param>
			/// <param name="itemInfos"></param>
			/// <returns></returns>
			private LayoutItemInfo GetResizeItemInfosHelper( ILayoutItem resizeItem, IList<ILayoutItem> synchronizedItems, out LayoutItemInfo[] itemInfos )
			{
				LayoutItemInfo resizeItemInfo = this.GetMappedItem( resizeItem );

				List<LayoutItemInfo> list = new List<LayoutItemInfo>( );
				list.Add( resizeItemInfo );

				if ( null != synchronizedItems )
				{
					for ( int i = 0, count = synchronizedItems.Count; i < count; i++ )
					{
						ILayoutItem ii = synchronizedItems[i];
						if ( ii != resizeItem )
							list.Add( this.GetMappedItem( ii ) );
					}
				}

				itemInfos = list.ToArray( );
				return resizeItemInfo;
			}

			#endregion // GetResizeItemInfosHelper

		#region GetResizeRangeHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// Added capability of resizing multiple items at the same time in a synchronous manner.
			// Added the following overload that takes in an array of items to resize.
			// 
			private void GetResizeRangeHelper( LayoutItemInfo[] resizeItems,
				out double maxDeltaLeft, out double maxDeltaRight,
				out double maxDeltaTop, out double maxDeltaBottom )
			{
				maxDeltaLeft = maxDeltaRight = maxDeltaTop = maxDeltaBottom = double.MaxValue;

				// Grid-bag layout manager constraints an auto-sized item to be at least its preferred size.
				// Therefore we need to turn off auto-sized state on the items that are to be resized.
				// 
				this.ClearIsAutoState( resizeItems, true, true, true, true );


				var processedColumnRights = new HashSet<int>();
				var processedRowBottoms = new HashSet<int>();





				for ( int i = 0; i < resizeItems.Length; i++ )
				{
					LayoutItemInfo ii = resizeItems[i];

					if ( processedColumnRights.Contains( ii._dim.ColumnRight )
						&& processedRowBottoms.Contains( ii._dim.RowBottom ) )
						continue;

					processedColumnRights.Add( ii._dim.ColumnRight );
					processedRowBottoms.Add( ii._dim.RowBottom );

					double left, top, right, bottom;
					this.GetResizeRangeHelperHelper( ii, out left, out right, out top, out bottom );

					maxDeltaLeft = Math.Min( maxDeltaLeft, left );
					maxDeltaRight = Math.Min( maxDeltaRight, right );
					maxDeltaTop = Math.Min( maxDeltaTop, top );
					maxDeltaBottom = Math.Min( maxDeltaBottom, bottom );
				}

				if ( _autoFitWidth )
				{
					maxDeltaLeft = -this.GetResizeRangeHelper_AdjustDeltaHelper( resizeItems, -maxDeltaLeft, false );
					maxDeltaRight = this.GetResizeRangeHelper_AdjustDeltaHelper( resizeItems, maxDeltaRight, false );
				}

				if ( _autoFitHeight )
				{
					maxDeltaTop = -this.GetResizeRangeHelper_AdjustDeltaHelper( resizeItems, -maxDeltaTop, true );
					maxDeltaBottom = this.GetResizeRangeHelper_AdjustDeltaHelper( resizeItems, maxDeltaBottom, true );
				}
			}

			private void GetResizeRangeHelperHelper( LayoutItemInfo resizeItem,
				out double maxDeltaLeft, out double maxDeltaRight,
				out double maxDeltaTop, out double maxDeltaBottom )
			{
				// SSP 7/10/09 NAS9.2 Synchronous Sizing - Optimization
				// 
				//GridBagLayoutCache glc;
				//LayoutItemInfo[] info = this.GetInfo( out glc );
				GridBagLayoutCache glc = _glc;
				LayoutItemInfo[] info = _infos;

				GridBagLayoutItemDimensions resizeItemDim = resizeItem._dim;
				int resizeLeftColumn = resizeItemDim.Column;
				int resizeTopRow = resizeItemDim.Row;

				double maxRight = double.MaxValue;
				double maxBottom = double.MaxValue;

				for ( int i = 0; i < info.Length; i++ )
				{
					LayoutItemInfo ii = info[i];
					GridBagLayoutItemDimensions dim = ii._dim;

					if ( dim.ColumnRight < resizeItemDim.ColumnRight && dim.ColumnRight > resizeItemDim.Column )
						resizeLeftColumn = Math.Max( resizeLeftColumn, dim.ColumnRight );

					if ( dim.RowBottom < resizeItemDim.RowBottom && dim.RowBottom > resizeItemDim.Row )
						resizeTopRow = Math.Max( resizeTopRow, dim.RowBottom );

					// SSP 11/18/09 - NAS10.1 XamTilesControl
					// Added inter-item-spacing properties.
					// 
					//Thickness iiMargin = ii._gcc._margin;
					Thickness iiMargin = ii._gcc.GetMarginWithSpacing( _layoutManager );

					if ( dim.ColumnRight == resizeItemDim.ColumnRight
						// Non-stretch alignment items should not be taken into consideration when calculating
						// max resize constraint since these items can be aligned left, center, right and thus
						// their max-width constraint is irrelevant when calculating the max resize size of some
						// other item. If this behavior is deemed to be in-appropriate then this condition can
						// be safely taken out.
						&& HorizontalAlignment.Stretch == ii._gcc._gc.HorizontalAlignment )
						maxRight = Math.Min( maxRight, dim.Bounds.Left + ii._gcc._maxWidth + iiMargin.Right );

					if ( dim.RowBottom == resizeItemDim.RowBottom
						&& VerticalAlignment.Stretch == ii._gcc._gc.VerticalAlignment )
						maxBottom = Math.Min( maxBottom, dim.Bounds.Top + ii._gcc._maxHeight + iiMargin.Bottom );
				}

				double minWidth = GridBagLayoutManager.ArrSum( glc._colWidthsMin, resizeLeftColumn, resizeItemDim.ColumnRight - resizeLeftColumn );
				double minHeight = GridBagLayoutManager.ArrSum( glc._rowHeightsMin, resizeTopRow, resizeItemDim.RowBottom - resizeTopRow );

				double minLeft = glc._colCoordinates[resizeLeftColumn] + minWidth;
				double minTop = glc._rowCoordinates[resizeTopRow] + minHeight;

				minLeft = Math.Max( minLeft, resizeItemDim.Bounds.Left + resizeItem._gcc._minWidth );
				minTop = Math.Max( minTop, resizeItemDim.Bounds.Top + resizeItem._gcc._minHeight );

				maxDeltaLeft = resizeItemDim.Bounds.Right - minLeft;
				maxDeltaTop = resizeItemDim.Bounds.Bottom - minTop;

				maxDeltaRight = maxRight - resizeItemDim.Bounds.Right;
				maxDeltaBottom = maxBottom - resizeItemDim.Bounds.Bottom;

				// SSP 7/29/09 - NAS9.2 Auto-sizing
				// When auto-fitting, the item can't be bigger than the auto-fit extent. Also we
				// are fine-tuning the values later on to take into account other items and their
				// constraints.
				// 
				// SSP 2/16/10 TFS27086
				// 
				//Rect containerRect = _layoutContainer.GetBounds( _containerContext );
				Rect containerRect = LayoutManagerBase.GetContainerBoundsHelper( _layoutContainer, _containerContext, _layoutManager );

				if ( _autoFitWidth && containerRect.Width > 0 )
					maxDeltaRight = Math.Min( maxDeltaRight, containerRect.Width );

				if ( _autoFitHeight && containerRect.Height > 0 )
					maxDeltaBottom = Math.Min( maxDeltaBottom, containerRect.Height );
			}

			#endregion // GetResizeRangeHelper

		#region GetResizeRangeHelper_AdjustDeltaHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			/// <summary>
			/// Used when auto-fitting, items can't necessarily be resized to their fullest max or min sizes 
			/// because the auto-fitting process can potentially constraint the items. This method adjusts
			/// the delta which are based on the min/max values to a new delta value that takes into account
			/// this implicit constraint forced by the auto-fitting.
			/// </summary>
			/// <param name="resizeItems"></param>
			/// <param name="delta"></param>
			/// <param name="yDimension"></param>
			/// <returns></returns>
			private double GetResizeRangeHelper_AdjustDeltaHelper( LayoutItemInfo[] resizeItems, double delta, bool yDimension )
			{
				double x = 0;
				double y = delta;

				int pass = 0;
				int MAX_PASSES = 20;

				Dictionary<LayoutItemInfo, Size> oldPreferredSizes = this.GetPreferredSizes( _infos );

				while ( !AreClose( x, y, 0.05 ) && pass++ < MAX_PASSES )
				{
					double d33 = x + 0.33 * ( y - x );
					double d66 = x + 0.66 * ( y - x );

					Dictionary<LayoutItemInfo, Size> results33 = this.GetResizeResultsHelper( resizeItems, d33, yDimension, oldPreferredSizes );
					Dictionary<LayoutItemInfo, Size> results66 = this.GetResizeResultsHelper( resizeItems, d66, yDimension, oldPreferredSizes );

					if ( AreSame( results33, results66 ) )
					{
						y = d33;
					}
					else
					{
						x = d33;
					}
				}

				// Round the number.
				y = Math.Round( y, 1 );
				return y;
			}

			#endregion // GetResizeRangeHelper_AdjustDeltaHelper

		#region GetResizeResultsHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			/// <summary>
			/// Calculates the new sizes of the resize items if they were to be resized by the specified delta.
			/// </summary>
			/// <param name="resizeItems"></param>
			/// <param name="delta"></param>
			/// <param name="yDimension"></param>
            /// <param name="restorePreferredSizes"></param>
			/// <returns></returns>
			private Dictionary<LayoutItemInfo, Size> GetResizeResultsHelper( LayoutItemInfo[] resizeItems, 
				double delta, bool yDimension, Dictionary<LayoutItemInfo, Size> restorePreferredSizes )
			{
				Size newSize = this.CalcNewSizeHelper( resizeItems[0], !yDimension ? delta : 0, yDimension ? delta : 0 );
				
				this.ResizeItemsHelper( resizeItems, this.CreateSizeArr( newSize, resizeItems.Length ), yDimension );

				Dictionary<LayoutItemInfo, Size> results = new Dictionary<LayoutItemInfo, Size>( );

				for ( int i = 0; i < resizeItems.Length; i++ )
				{
					LayoutItemInfo ii = resizeItems[i];
					results[ii] = ii._dim.Size;
				}

				if ( null != restorePreferredSizes )
					this.SetPreferredSizes( restorePreferredSizes );

				this.ReCalculateLayout( );

				return results;
			}

			#endregion // GetResizeResultsHelper

		#region IsExtentDifferent

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			/// <summary>
			/// Returns true if the new extent is different than the old extent for any of the resize items.
			/// </summary>
			/// <param name="resizeItems"></param>
			/// <param name="newSizes"></param>
			/// <param name="yDimension"></param>
			/// <returns></returns>
			private bool IsExtentDifferent( LayoutItemInfo[] resizeItems, Size[] newSizes, bool yDimension )
			{
				bool extentChanged = false;

				for ( int i = 0; i < resizeItems.Length && !extentChanged; i++ )
				{
					Size oldSize = resizeItems[i]._dim.Size;
					Size newSize = newSizes[i];

					extentChanged = !yDimension
						? !CoreUtilities.AreClose( oldSize.Width, newSize.Width )
						: !CoreUtilities.AreClose( oldSize.Height, newSize.Height );
				}

				return extentChanged;
			}

			#endregion // IsExtentDifferent

		#region ReCalculateLayout

			// SSP 4/7/11 TFS32232
			// Added invalidateLayout parameter.
			// 
			private void ReCalculateLayout( )
			{
				this.ReCalculateLayout( true );
			}

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// Added capability of resizing multiple items at the same time in a synchronous manner.
			// 
			/// <summary>
			/// This method is used to re-calculate the layout after changing preferred sizes of 
			/// one or more items. It updates the cached values as well.
			/// </summary>
			// SSP 4/7/11 TFS32232
			// Added invalidateLayout parameter.
			// 
			//private void ReCalculateLayout( )
			private void ReCalculateLayout( bool invalidateLayout )
			{
				// SSP 4/7/11 TFS32232
				// Check the new invalidateLayout parameter. Enclosed existing code in the if block.
				// 
				if ( invalidateLayout )
					_layoutManager.InvalidateLayout( );

				_glc = _layoutManager.GetGridBagLayoutCache( );
				
				// SSP 4/7/11 TFS32232
				// Check the new invalidateLayout parameter. Enclosed existing code in the if block.
				// 
				if ( !invalidateLayout )
					_glc._lastContainerWidth = _glc._lastContainerHeight = 0;

				GridBagConstraintCache[] gccArr = _glc._gccArr;
				GridBagLayoutItemDimensionsCollection dims = _layoutManager.GetLayoutItemDimensions( _layoutContainer, _containerContext );

				// If _mappings and _infos are null then initialize them.
				// 
				if ( null == _mappings || null == _infos )
				{
					_mappings = new Dictionary<ILayoutItem, LayoutItemInfo>( );
					List<LayoutItemInfo> list = new List<LayoutItemInfo>( );

					for ( int i = 0; i < gccArr.Length; i++ )
					{
						GridBagConstraintCache gcc = gccArr[i];
						LayoutItemInfo info = (LayoutItemInfo)gcc._item;
						_mappings[info._i] = info;
						list.Add( info );
					}

					_infos = list.ToArray( );
				}

				for ( int i = 0; i < gccArr.Length; i++ )
				{
					GridBagConstraintCache gcc = gccArr[i];
					LayoutItemInfo info = (LayoutItemInfo)gcc._item;

					if ( dims.Exists( info ) )
					{
						info._gcc = gcc;
						info._dim = dims[info];
					}
					else
						Debug.Assert( false );
				}
			}

			#endregion // ReCalculateLayout

		#region RemoveMargin

			private void RemoveMargin( ref Size sz, LayoutItemInfo ii )
			{
				// SSP 11/18/09 - NAS10.1 XamTilesControl
				// Added inter-item-spacing properties.
				// 
				//Thickness m = ii._gcc._margin;
				Thickness m = ii._gcc.GetMarginWithSpacing( _layoutManager ); 

				sz.Width -= m.Left + m.Right;
				sz.Height -= m.Top + m.Bottom;
			}

			#endregion // RemoveMargin

			// SSP 4/7/11 TFS32232
			// 
			private void ResizeLogicalDims( LayoutItemInfo item, double targetExtent, bool yDimension )
			{
				double[] dims = !yDimension ? _layoutManager.ColumnWidths : _layoutManager.RowHeights;

				int start = !yDimension ? item._dim.Column : item._dim.Row;
				int span = !yDimension ? item._dim.ColumnSpan : item._dim.RowSpan;

				Rect itemRect = item._dim.Bounds;
				double currentExtent = !yDimension ? itemRect.Width : itemRect.Height;
				double delta = targetExtent - currentExtent;

				for ( int i = 0; i < span; i++ )
				{
					// SSP 9/26/11 TFS89155
					// 
					//dims[start + i] += delta / span;
					dims[start + i] = Math.Max( dims[start + i] + delta / span,
						!yDimension ? item._gcc._minWidth : item._gcc._minHeight );
				}
			}

		#region ResizeItemHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// Added capability of resizing multiple items at the same time in a synchronous manner.
			// 
			//private void ResizeItemHelper( LayoutItemInfo resizeItem, double deltaX, double deltaY )
			private void ResizeItemHelper( ILayoutItem item, double deltaX, double deltaY, IList<ILayoutItem> synchronizedItems )
			{
				// SSP 7/10/09 NAS9.2 Synchronous Sizing
				// Added synchronizedItems parameter. Also moved existing code below into the new
				// CalcNewSizeHelper method.
				// 
				// --------------------------------------------------------------------------------------
				//Size newSize = this.CalcNewSizeHelper( resizeItem, deltaX, deltaY );
				//this.ResizeItemHelper( resizeItem, newSize, deltaX, deltaY );

				LayoutItemInfo[] resizeItems;
				LayoutItemInfo resizeItem = GetResizeItemInfosHelper( item, synchronizedItems, out resizeItems );

				Size newSize = this.CalcNewSizeHelper( resizeItem, deltaX, deltaY );

				this.ResizeItemsHelper( resizeItems, this.CreateSizeArr( newSize, resizeItems.Length ), 
					! IsCloseToZero( deltaX ), ! IsCloseToZero( deltaY ) );
				// --------------------------------------------------------------------------------------
			}

		#endregion // ResizeItemHelper

		#region ResizeItemsHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// Added capability of resizing multiple items at the same time in a synchronous manner.
			// Refactored existing ResizeItemHelper method into the new ResizeItemsHelper method. The original
			// method is commented out.
			// 
			/// <summary>
			/// Resizes the specified items to their new sizes in either horizontal or vertical dimension
			/// depending upon the yDimension parameter.
			/// </summary>
			/// <param name="resizeItems"></param>
			/// <param name="newSizes"></param>
			/// <param name="yDimension"></param>
			private void ResizeItemsHelper( LayoutItemInfo[] resizeItems, Size[] newSizes, bool yDimension )
			{
				Debug.Assert( resizeItems.Length == newSizes.Length );

				// SSP 2/16/10 TFS27086
				// 
				//Rect containerRect = _layoutContainer.GetBounds( _containerContext );
				Rect containerRect = LayoutManagerBase.GetContainerBoundsHelper( _layoutContainer, _containerContext, _layoutManager );

				Size layoutPrefSize = _layoutManager.CalculatePreferredSize( _layoutContainer, _containerContext );

				bool bAutoFit = !yDimension
					? _autoFitWidth || _layoutManager.ExpandToFitWidth && !AreClose( containerRect.Width, layoutPrefSize.Width, 1 )
					: _autoFitHeight || _layoutManager.ExpandToFitHeight && !AreClose( containerRect.Height, layoutPrefSize.Height, 1 );



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


				// SSP 2/15/10 TFS27405
				// When auto-fitting, the preferred sizes can be significantly different from actual auto-fitted sizes.
				// Below logic relies upon the preferred and actual sizes being close. Therefore simply make the 
				// preferred sizes of all the items their actual sizes when auto-fitting.
				// 
				if ( bAutoFit && 1 == _resizeStrategy )
					this.SetPreferredSizesToActual( yDimension, true );

				// Grid-bag layout manager constraints an auto-sized item to be at least its preferred size.
				// Therefore we need to turn off auto-sized state on the items that are to be resized as well
				// as all the other items that end at the same logical column or row depending on whether
				// the width or the hieght is being resized respectively.
				// 
				this.ClearIsAutoState( resizeItems, !yDimension, yDimension, true, true );

				// However if the items are being auto-sized, then we need to set their isAuto states to true
				// to ensure the end results would be the same after the resize operation as they would be
				// with the below logic.
				// 
				if ( _isAutoSizing )
					this.SetIsAutoState( resizeItems, !yDimension ? (bool?)true : null, yDimension ? (bool?)true : null );

				// Sort the items by their ColumnRight values.
				// 
				this.SortResizeItemsHelper( resizeItems, newSizes, false );
				int MAX_PASSES = !bAutoFit ? 1 : 20;

				// SSP 4/8/11 TFS32232
				// 
				// ----------------------------------------------------------------------------------------------------
				if ( GridBagLayoutMode.Uniform == _layoutManager.LayoutMode )
				{
					for ( int pass = 0; pass < MAX_PASSES; pass++ )
					{
						this.SetPreferredSizesToActual( yDimension, true );

						for ( int i = 0; i < resizeItems.Length; i++ )
						{
							LayoutItemInfo iiResizeItem = resizeItems[0];
							Size iiSize = newSizes[i];

							if ( !yDimension )
								this.ResizeLogicalDims( iiResizeItem, iiSize.Width, false );
							else
								this.ResizeLogicalDims( iiResizeItem, iiSize.Height, true );
						}

						this.SetPreferredSizesToActual( yDimension, false, false );
						this.ReCalculateLayout( true );
					}

					return;
				}
				// ----------------------------------------------------------------------------------------------------

				Dictionary<LayoutItemInfo, double> lastAppliedDeltaMap = new Dictionary<LayoutItemInfo, double>( );
				Dictionary<LayoutItemInfo, double> lastAppliedDeltaFactorMap = new Dictionary<LayoutItemInfo, double>( );

				// We need to perform multiple passes when auto-fitting. Otherwise only a single pass
				// is necessary.
				// 
				for ( int pass = 0; pass < MAX_PASSES; pass++ )
				{
					GridBagLayoutCache glc = _glc;
					LayoutItemInfo[] info = _infos;

					double[] actualCoordinates = !yDimension ? glc._colCoordinates : glc._rowCoordinates;
					double[] prefCoordinates = CreateCoordinates( !yDimension ? glc._colWidthsPref : glc._rowHeightsPref );

					bool isNOOP = true;
					// SSP 2/15/10 TFS27405
					// 
					bool constraintEncountered = false;

					int resizeItemsIndex = 0;
					while ( resizeItemsIndex < resizeItems.Length )
					{
						LayoutItemInfo resizeItem = resizeItems[resizeItemsIndex];
						double delta = this.CalcDeltaHelper( resizeItems, newSizes, ref resizeItemsIndex, yDimension );

						GridBagLayoutItemDimensions resizeItemDim = resizeItem._dim;

						// If we have reached the target size then we are done.
						// Also when auto-fitting, we need to perform multiple 
						// passes. Otherwise a single pass is sufficient.
						// 
						if ( 0 == delta || pass > 0 && !bAutoFit )
							continue;

						// Modify all the items that end at the same logical column or row depending upon
						// whether resizing width or height respectively.
						// 
						LayoutItemInfo[] alignedInfos = this.GetItemsEndingAt( !yDimension ? resizeItemDim.ColumnRight : resizeItemDim.RowBottom, yDimension );
						for ( int i = 0; i < alignedInfos.Length; i++ )
						{
							LayoutItemInfo ii = alignedInfos[i];
							GridBagLayoutItemDimensions dim = ii._dim;

							// SSP 7/10/09 NAS9.2 Synchronous Sizing
							// Made resizing more accurate in auto-fit situation with very big delta's compared
							// to current size of the item as well as current container size.
							// Added the else block and enclosed the existing code into the if block.
							// 
							double factor = 1;
							double lastAppliedDelta;
							if ( lastAppliedDeltaMap.TryGetValue( ii, out lastAppliedDelta ) )
							{
								double lastAchievedDelta = lastAppliedDelta - delta;
								if ( !IsCloseToZero( lastAppliedDelta ) && !IsCloseToZero( lastAchievedDelta ) )
									factor = ( lastAppliedDelta * lastAppliedDeltaFactorMap[ii] ) / lastAchievedDelta;
							}

							// Get the current preferred size.
							// 
							Size currPrefSize = ii._preferredSize;
							if ( _layoutManager.PreferredSizeIncludesMargin )
								this.RemoveMargin( ref currPrefSize, ii );

							Size newPrefSize = currPrefSize;
							
							// SSP 2/15/10 TFS27405
							// Delta should be applied relative to current preferred size and not the current
							// actual size.
							// 
							//double newExtent = !yDimension
							//    ? dim.Bounds.Width + delta * factor
							//    : dim.Bounds.Height + delta * factor;
							double newExtent = !yDimension
								? currPrefSize.Width + delta * factor
								: currPrefSize.Height + delta * factor;

							// Ensure the preferred size doesn't get smaller than the minWidth.
							// 
							if ( !yDimension )
								newPrefSize.Width = Math.Max( ii._gcc._minWidth, Math.Min( ii._gcc._maxWidth, newExtent ) );
							else
								newPrefSize.Height = Math.Max( ii._gcc._minHeight, Math.Min( ii._gcc._maxHeight, newExtent ) );

							// SSP 2/15/10 TFS27405
							// 
							constraintEncountered = constraintEncountered 
								|| ! CoreUtilities.AreClose( newExtent, !yDimension ? newPrefSize.Width : newPrefSize.Height );

							// Set the new preferred size.
							// 
							this.SetPreferredSize( ii, newPrefSize, !yDimension, yDimension );
							lastAppliedDeltaMap[ii] = delta;
							lastAppliedDeltaFactorMap[ii] = factor;
							isNOOP = false;
						}
					}

					// If auto-fitting and preferred size of the layout is significantly different from
					// the actual size, then logic above for calculating the new preferred size required
					// to reach a target size of an item will be off. Therefore instead change preferred
					// sizes of items to reflect their actual auto-fitted sizes.
					// 
					if ( bAutoFit )
					{
						// SSP 2/15/10 TFS27405
						// Moved the commented out code into the new SetPreferredSizesToActual
						// method. Also set the preferred sizes to actual only when breaking 
						// out of this method.
						// 
						
						
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

						if ( 0 == _resizeStrategy || constraintEncountered || isNOOP || pass == MAX_PASSES - 1 )
						{
							this.SetPreferredSizesToActual( yDimension, false );
							lastAppliedDeltaMap.Clear( );
							lastAppliedDeltaFactorMap.Clear( );
						}
						

						this.ReCalculateLayout( );
					}

					// If we have reached the target size then we are done.
					// Also when auto-fitting, we need to perform multiple 
					// passes. Otherwise a single pass is sufficient.
					// 
					if ( isNOOP || pass > 0 && !bAutoFit )
						break;
				}
			}

			private void ResizeItemsHelper( LayoutItemInfo[] resizeItems, Size[] newSizes, bool resizeWidth, bool resizeHeight )
			{
				if ( resizeWidth )
					this.ResizeItemsHelper( resizeItems, newSizes, false );

				if ( resizeHeight )
					this.ResizeItemsHelper( resizeItems, newSizes, true );
			}

		#region Commented Out Code

			
#region Infragistics Source Cleanup (Region)











































































































#endregion // Infragistics Source Cleanup (Region)


		#endregion // Commented Out Code

		#endregion // ResizeItemsHelper

		#region SetIsAutoState

			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// 
			private void SetIsAutoState( LayoutItemInfo[] items, bool? isAutoWidth, bool? isAutoHeight )
			{
				for ( int i = 0; i < items.Length; i++ )
				{
					LayoutItemInfo ii = items[i];

					if ( isAutoWidth.HasValue )
						ii._isAutoWidth = isAutoWidth.Value;

					if ( isAutoHeight.HasValue )
						ii._isAutoHeight = isAutoHeight.Value;
				}
			}

		#endregion // SetIsAutoState

		#region SetPreferredSize

			private void SetPreferredSize( LayoutItemInfo ii, Size newSize, bool modifyWidth, bool modifyHeight )
			{
				if ( _layoutManager.PreferredSizeIncludesMargin )
					this.AddMargin( ref newSize, ii );

				if ( modifyWidth )
					ii._preferredSize.Width = newSize.Width;

				if ( modifyHeight )
					ii._preferredSize.Height = newSize.Height;
			}

		#endregion // SetPreferredSize

		#region SetPreferredSizes

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			/// <summary>
			/// Sets the _preferredSize on the layout items.
			/// </summary>
			/// <param name="sizesToSet"></param>
			private void SetPreferredSizes( Dictionary<LayoutItemInfo, Size> sizesToSet )
			{
				foreach ( KeyValuePair<LayoutItemInfo, Size> ii in sizesToSet )
				{
					ii.Key._preferredSize = ii.Value;
				}
			}

		#endregion // SetPreferredSizes

		#region SetPreferredSizesToActual

			// SSP 4/7/11 TFS32232
			// Added invalidateLayout parameter.
			// 
			private void SetPreferredSizesToActual( bool yDimension, bool exceptAutoSizedItems )
			{
				this.SetPreferredSizesToActual( yDimension, exceptAutoSizedItems, true );
			}

			// SSP 2/15/10 TFS27405
			// 
			// SSP 4/7/11 TFS32232
			// Added invalidateLayout parameter.
			// 
			//private void SetPreferredSizesToActual( bool yDimension, bool exceptAutoSizedItems )
			private void SetPreferredSizesToActual( bool yDimension, bool exceptAutoSizedItems, bool invalidateLayout )
			{
				// SSP 4/7/11 TFS32232
				// Pass along the new invalidateLayout parameter.
				// 
				//this.ReCalculateLayout( );
				this.ReCalculateLayout( invalidateLayout );

				foreach ( LayoutItemInfo ii in _infos )
				{
					// SSP 8/18/10 TFS32094
					// Added exceptAutoSizedItems parameter.
					// 
					//this.SetPreferredSize( ii, ii._dim.Bounds.Size, !yDimension, yDimension );
					bool isAutoSized = !yDimension ? ii._isAutoWidth : ii._isAutoHeight;
					if ( ! isAutoSized || ! exceptAutoSizedItems )

						this.SetPreferredSize( ii, ii._dim.Bounds.Size, !yDimension, yDimension );



				}
			}

		#endregion // SetPreferredSizesToActual

		#region SortResizeItemsHelper

			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// 
			/// <summary>
			/// Sorts the specified resize items and the new sizes so the items are in the order
			/// of lowest ColumnRight value to highest ColumnRight value.
			/// </summary>
			/// <param name="resizeItems"></param>
			/// <param name="newSizes"></param>
            /// <param name="yWise"></param>
			private void SortResizeItemsHelper( LayoutItemInfo[] resizeItems, Size[] newSizes, bool yWise )
			{
				int[] indeces = new int[resizeItems.Length];
				GridBagLayoutManager.ArrInitIndeces( indeces );

				CoreUtilities.SortMergeGeneric<int>( indeces,
					new CoreUtilities.ConverterComparer<int, GridBagConstraintCache>(
						new Converter<int, GridBagConstraintCache>(
							delegate( int ii )
							{
								return resizeItems[ii]._gcc;
							}
						),
						new LayoutItemComparer( _layoutManager, yWise )
					)
				);

				ArrayMove<LayoutItemInfo, Size>( resizeItems, newSizes, indeces );
			}

		#endregion // SortResizeItemsHelper

		#endregion // Private/Internal Methods

		#endregion // Methods
		}

		#endregion // ResizeManager Class

		#region Side Enum

		// SSP 11/18/09 - NAS10.1 XamTilesControl
		// 
		[Flags]
		private enum Side
		{
			None = 0x0,
			Left = 0x1,
			Top = 0x2,
			Right = 0x4,
			Bottom = 0x8,
			All = 0xf
		}

		#endregion // Side Enum

		#endregion // Private Data Structures

		#region Public Data Structures

		#region GridBagLayoutMode Enum

		/// <summary>
		/// GridBagLayout mode.
		/// </summary>
		public enum GridBagLayoutMode
		{
			/// <summary>
			/// Standard.
			/// </summary>
			Standard = 0,

			/// <summary>
			/// Left to right.
			/// </summary>
			LeftToRight = 1,

			// SSP 4/7/11 TFS32232
			// 
			/// <summary>
			/// Uniform extent distribution across logical rows and columns. 
			/// </summary>
			Uniform = 2
		}

		#endregion // GridBagLayoutMode Enum

		#endregion // Public Data Structures

		#region Private Variables

		private bool _expandToFitWidth;
		private bool _expandToFitHeight;
		private GridBagLayoutMode _gridBagMode = GridBagLayoutMode.Standard;
		private bool _preferredSizeIncludesMargin = false;
		private bool _includeMarginInPositionRect = false;

		private GridBagLayoutManager.GridBagLayoutCache _gridBagLayoutCache;

		// SSP 10/26/09 - NAS10.1 XamTilesControl
		// 
		private double _interItemSpacingHorizontal = 0;
		private double _interItemSpacingVertical = 0;
		private HorizontalAlignment _horizontalContentAlignment = HorizontalAlignment.Center;
		private VerticalAlignment _verticalContentAlignment = VerticalAlignment.Center;
		private bool? _shrinkToFitWidth, _shrinkToFitHeight;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public GridBagLayoutManager( )
			: base( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region CalculateMaximumSize

		/// <summary>
		/// Calculates the minimum size required to layout the items.
		/// </summary>
		/// <param name="container">Object that implements the ILayoutContainer to provide bounds information</param>
		/// <param name="containerContext">Context used in calls to the <paramref name="container"/></param>
		/// <returns>A <see cref="Size"/> object representing the minimum size required to layout the items.</returns>
		public override Size CalculateMaximumSize( ILayoutContainer container, object containerContext )
		{
			GridBagLayoutCache c = this.GetGridBagLayoutCache( );
			return GetTotalSize( c._colWidthsMax, c._rowHeightsMax );
		}

		#endregion // CalculateMaximumSize

		#region CalculateMinimumSize

		/// <summary>
		/// Calculates the minimum size required to layout the items.
		/// </summary>
		/// <param name="container">Object that implements the ILayoutContainer to provide bounds information</param>
		/// <param name="containerContext">Context used in calls to the <paramref name="container"/></param>
		/// <returns>A <see cref="Size"/> object representing the minimum size required to layout the items.</returns>
		public override Size CalculateMinimumSize( ILayoutContainer container, object containerContext )
		{
			GridBagLayoutCache c = this.GetGridBagLayoutCache( );
			return GetTotalSize( c._colWidthsMin, c._rowHeightsMin );
		}

		#endregion // CalculateMinimumSize

		#region CalculatePreferredSize

		/// <summary>
		/// Calculates the preferred size required to layout the items.
		/// </summary>
		/// <param name="container">Object that implements the ILayoutContainer to provide bounds information</param>
		/// <param name="containerContext">Context used in calls to the <paramref name="container"/></param>
		/// <returns>A <see cref="Size"/> object representing the preferred size required to layout the items.</returns>
		public override Size CalculatePreferredSize( ILayoutContainer container, object containerContext )
		{
			GridBagLayoutCache c = this.GetGridBagLayoutCache( );
			return GetTotalSize( c._colWidthsPref, c._rowHeightsPref );
		}

		#endregion // CalculatePreferredSize

		#region Clone

		private GridBagLayoutManager Clone( bool copyLayoutItems )
		{
			GridBagLayoutManager clone = new GridBagLayoutManager( );

			clone._expandToFitHeight = _expandToFitHeight;
			clone._expandToFitWidth = _expandToFitWidth;
			clone._gridBagMode = _gridBagMode;
			clone._includeMarginInPositionRect = _includeMarginInPositionRect;
			clone._preferredSizeIncludesMargin = _preferredSizeIncludesMargin;

			// SSP 10/28/09 - NAS10.1 XamTilesControl - CalculateAutoLayout
			// 
			clone._interItemSpacingHorizontal = _interItemSpacingHorizontal;
			clone._interItemSpacingVertical = _interItemSpacingVertical;

			// JJD 3/2/10 - TFS27838
            // Clone the additional settings
            clone._horizontalContentAlignment = _horizontalContentAlignment;
			clone._verticalContentAlignment = _verticalContentAlignment;

            clone._shrinkToFitHeight = _shrinkToFitHeight;
            clone._shrinkToFitWidth = _shrinkToFitWidth;

			if ( copyLayoutItems )
			{
				LayoutItemsCollection srcItems = this.LayoutItems;
				foreach ( ILayoutItem ii in srcItems )
					clone.LayoutItems.Add( ii, srcItems.GetConstraint( ii ) );
			}

			return clone;
		}

		#endregion // Clone

		#region InvalidateLayout

		/// <summary>
		/// Invalidates any cached information so the layout manager recalculates everything next time.
		/// </summary>
		/// <remarks>
		/// <p>Gridbag layout manager caches layout information which needs to be invalidated any time a change is made that would effect how the items are laid out.</p>
		/// </remarks>
		public override void InvalidateLayout( )
		{
			this._gridBagLayoutCache = null;
		}

		#endregion // InvalidateLayout

		#region LayoutContainer

		/// <summary>
		/// Lays out items contained in this layout manager by calling PositionItem off the
		/// passed in container for each item.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="containerContext"></param>
		public override void LayoutContainer( ILayoutContainer container, object containerContext )
		{
			object tmp = null;
			this.GridBagHelper( container, containerContext, true, false, ref tmp );
		}

		// SSP 2/5/10 - NAS10.1 XamTilesControl
		// Added layoutRect parameter.
		// 
		/// <summary>
		/// Lays out items contained in this layout manager by calling PositionItem off the
		/// passed in container for each item.
		/// </summary>
		/// <param name="container">Layout container.</param>
		/// <param name="containerContext">Layout container context.</param>
		/// <param name="layoutRect">This out parameter will be set to the rect in which items were laid out which
		/// can be different from layout container's rect in case of min size constraints.</param>
		public void LayoutContainer( ILayoutContainer container, object containerContext, out Rect layoutRect )
		{
			object tmp = DBNull.Value;
			this.GridBagHelper( container, containerContext, true, false, ref tmp );
			layoutRect = (Rect)tmp;
		}

		#endregion // LayoutContainer

		#region ValidateConstraintObject

		/// <summary>
		/// Implementation should throw an exception if the passed in constraint is not a valid
		/// constraint for this layout manager. It usually checks the type.
		/// </summary>
		/// <param name="constraint">The constraint to validate.</param>
		protected override void ValidateConstraintObject( object constraint )
		{
			if ( !( constraint is IGridBagConstraint ) )
				throw new ArgumentException( GridBagLayoutManager.GetString("LE_GridBag_InvalidConstraint"), "constraint" );
		}

		#endregion // ValidateConstraintObject

		#endregion // Base Overrides

		#region Methods

		#region Public Methods

		#region CalculateAutoLayout

		// SSP 10/26/09 - NAS10.1 XamTilesControl - CalculateAutoLayout
		// 
		/// <summary>
		/// Calculates the row/column values for the specified items to fill the layout area.
		/// </summary>
		/// <param name="items">Items to auto-layout.</param>
		/// <param name="layoutOrientation">Direction in which to layout items.</param>
		/// <param name="autoFitAllItems">If true all items will be auto-fitted within the layout area.</param>
		/// <param name="constraint">Layout are size.</param>
		/// <param name="minRows">Minimum number of logical rows of items.</param>
		/// <param name="minColumns">Minimum number of logical columns of items.</param>
		/// <param name="maxRows">Maximum number of logical rows of items.</param>
		/// <param name="maxColumns">Maximum number of logical columns of items.</param>
		/// <param name="constraintedOrientation">
		/// If specified non-null, then items will be laid in such a way that they will be not be allowed 
		/// to go beyond the edge of the container in the constrained orientation (unless other constraints 
		/// force otherwise). If null then items will not be constrained in either orientation.</param>
		/// <returns>Calculated layout information in the form of a GridBagLayoutItemDimensionsCollection instance.</returns>
		public GridBagLayoutItemDimensionsCollection CalculateAutoLayout(
			IEnumerator<ILayoutItem> items,
			Orientation layoutOrientation,
			bool autoFitAllItems,
			Size constraint,
			int minRows, int minColumns,
			int maxRows, int maxColumns,
			Orientation? constraintedOrientation
			)
		{
			AutoLayoutCalc autoLayout = new AutoLayoutCalc( this, layoutOrientation,
				autoFitAllItems, constraint, minRows, minColumns, maxRows, maxColumns, constraintedOrientation );

			return autoLayout.AutoCalculateLayout( items );
		}

		#endregion // CalculateAutoLayout

		#region GetLayoutItemDimensions

		/// <summary>
		/// For internal use. Returns dimensions of layout items. It contains entries for only the visible items.
		/// </summary>
        /// <param name="containerContext">The context used in calls to the <paramref name="container"/>.</param>
		/// <param name="container">The <see cref="ILayoutContainer"/> from which layout information should be determined.</param>
		/// <returns>The dimensions of layout items.</returns>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public GridBagLayoutItemDimensionsCollection GetLayoutItemDimensions(
			ILayoutContainer container,
			object containerContext )
		{
			object tmp = null;
			return this.GridBagHelper( container, containerContext, false, true, ref tmp );
		}

		#endregion // GetLayoutItemDimensions

		#region GetResizeRange

		/// <summary>
		/// Gets the resize range of an item.
		/// </summary>
		/// <param name="layoutContainer">Layout container. Used for getting the layout area rect, which 
		/// is used when auto-fitting the layout in in an area that's different than the preferred size of the layout.
		/// </param>
		/// <param name="containerContext">Not used by the layout manager directly. It gets passed along to
		/// methods on the layout container.</param>
		/// <param name="resizeItem">Item being resized.</param>
		/// <param name="maxDeltaLeft">This will be set to how much smaller the item can be resized left of its right edge.</param>
		/// <param name="maxDeltaRight">This will be set to how much larger the item can be resized right of its right edge.</param>
		/// <param name="maxDeltaTop">This will be set to how much smaller the item can be resized above its bottom edge.</param>
		/// <param name="maxDeltaBottom">This will be set to how much larger the item can be resized below its bottom edge.</param>
		public void GetResizeRange( ILayoutContainer layoutContainer, object containerContext,
			ILayoutItem resizeItem, out double maxDeltaLeft, out double maxDeltaRight,
			out double maxDeltaTop, out double maxDeltaBottom )
		{
			this.GetResizeRange(layoutContainer, containerContext, resizeItem, 
				out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom, 
				false, false, null);
		}

		// AS 6/29/09 NA 2009.2 Field Sizing
		// Added an overload to support synchronized resizing. The overload takes booleans indicating whether 
		// autosizing will be used and a list of items whose size will be synchronized with the resize item.
		//
		/// <summary>
		/// Gets the resize range of an item.
		/// </summary>
		/// <param name="layoutContainer">Layout container. Used for getting the layout area rect, which 
		/// is used when auto-fitting the layout in in an area that's different than the preferred size of the layout.
		/// </param>
		/// <param name="containerContext">Not used by the layout manager directly. It gets passed along to
		/// methods on the layout container.</param>
		/// <param name="resizeItem">Item being resized.</param>
		/// <param name="maxDeltaLeft">This will be set to how much smaller the item can be resized left of its right edge.</param>
		/// <param name="maxDeltaRight">This will be set to how much larger the item can be resized right of its right edge.</param>
		/// <param name="maxDeltaTop">This will be set to how much smaller the item can be resized above its bottom edge.</param>
		/// <param name="maxDeltaBottom">This will be set to how much larger the item can be resized below its bottom edge.</param>
		/// <param name="autoFitWidth">Specifies whether items will be autofitted horizontally within the 
		/// layout container rect. Items will be resized in such a way that the width of the preferred size
		/// of the resultant layout will be equal container rect's width.</param>
		/// <param name="autoFitHeight">Specifies whether items will be autofitted vertically within the 
		/// layout container rect. Items will be resized in such a way that the height of the preferred size
		/// of the resultant layout will be equal container rect's height.</param>
		/// <param name="synchronizedItems">A list of items whose size will be synchronized during the resize operation with the specified <paramref name="resizeItem"/> or null/empty list if there are no items to be synchronized with the resize item.</param>
		public void GetResizeRange( ILayoutContainer layoutContainer, object containerContext,
			ILayoutItem resizeItem, out double maxDeltaLeft, out double maxDeltaRight,
			out double maxDeltaTop, out double maxDeltaBottom, 
			bool autoFitWidth, bool autoFitHeight, IList<ILayoutItem> synchronizedItems )
		{
			// SSP 7/10/09 NAS9.2 Synchronous Sizing
			// Pass along the new parameters.
			// 
			// ----------------------------------------------------------------------------------------------------
			//ResizeManager manager = new ResizeManager( this, layoutContainer, containerContext, false, false );
			//manager.GetResizeRange( resizeItem, out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom );

			ResizeManager manager = new ResizeManager( this, layoutContainer, containerContext, autoFitWidth, autoFitHeight );

			manager.GetResizeRange( resizeItem, 
				out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom, synchronizedItems );
			// ----------------------------------------------------------------------------------------------------
		}

		#endregion // GetResizeRange

		// AS 6/29/09 NA 2009.2 Field Sizing
		#region PerformAutoSize

		/// <summary>
		/// Resizes the specified layout items to the specified preferred sizes.
		/// </summary>
		/// <param name="layoutContainer">Object used to determine the layout area rect, which is used when 
		/// auto-fitting the layout in in an area that's different than the preferred size of the layout.</param>
		/// <param name="containerContext">Not used by the layout manager directly. It gets passed along to
		/// methods on the layout container.</param>
		/// <param name="preferredSizes">A dictionary of the items to be resized and the preferred sizes to which they would like to be sized.</param>
		/// <param name="autoFitWidth">Specifies whether items will be autofitted horizontally within the 
		/// layout container rect. Items will be resized in such a way that the width of the preferred size
		/// of the resultant layout will be equal container rect's width.</param>
		/// <param name="autoFitHeight">Specifies whether items will be autofitted vertically within the 
		/// layout container rect. Items will be resized in such a way that the height of the preferred size
		/// of the resultant layout will be equal container rect's height.</param>
		/// <param name="newIsWidthAutoState">When an item is resized, items aligned with the resize item need to change
		/// their is-auto state to false. This parameter will be assigned a list of items whose is-auto state needs to
		/// change to false.</param>
		/// <param name="newIsHeightAutoState">Same as newIsWidthAutoState except for heights.</param>
		/// <returns>Returns the new preferred sizes of one or more items that will result in the preferred sizes of the specified layout items.</returns>
		public Dictionary<ILayoutItem, Size> PerformAutoSize(ILayoutContainer layoutContainer, object containerContext,
			Dictionary<ILayoutItem, Size> preferredSizes, bool autoFitWidth, bool autoFitHeight,
			out Dictionary<ILayoutItem, bool> newIsWidthAutoState,
			out Dictionary<ILayoutItem, bool> newIsHeightAutoState )
		{
			newIsWidthAutoState = newIsHeightAutoState = null;

			if (preferredSizes == null || preferredSizes.Count == 0)
				return new Dictionary<ILayoutItem, Size>();

			ResizeManager manager = new ResizeManager( this, layoutContainer, containerContext, autoFitWidth, autoFitHeight );
			manager._isAutoSizing = true;
			
			manager.ResizeItems( preferredSizes );

			return manager.GetNewPreferredSizes( out newIsWidthAutoState, out newIsHeightAutoState, true, true );
		} 

		#endregion //PerformAutoSize

		#region ResizeItem

		/// <summary>
		/// Resizes the specified layout item by specified deltaWidth and deltaHeight. Returns the new
		/// calculated preferred sizes of one or more items that will result in the resize item's target size.
		/// </summary>
		/// <param name="layoutContainer">Layout container. Used for getting the layout area rect, which 
		/// is used when auto-fitting the layout in in an area that's different than the preferred size of the layout.
		/// </param>
		/// <param name="containerContext">Not used by the layout manager directly. It gets passed along to
		/// methods on the layout container.</param>
		/// <param name="resizeItem">Item being resized.</param>
		/// <param name="deltaWidth">Change in the width of the item. Can be 0 to indicate no change.</param>
		/// <param name="deltaHeight">Change in the height of the item. Can be 0 to indicate no change.</param>
		/// <param name="autoFitWidth">Specifies whether items will be autofitted horizontally within the 
		/// layout container rect. Items will be resized in such a way that the width of the preferred size
		/// of the resultant layout will be equal container rect's width.</param>
		/// <param name="autoFitHeight">Specifies whether items will be autofitted vertically within the 
		/// layout container rect. Items will be resized in such a way that the height of the preferred size
		/// of the resultant layout will be equal container rect's height.</param>
		/// <returns>Returns the new preferred sizes of one or more items that will result in the target size
		/// of the resize item.</returns>
		public Dictionary<ILayoutItem, Size> ResizeItem( ILayoutContainer layoutContainer, object containerContext,
			ILayoutItem resizeItem, double deltaWidth, double deltaHeight, bool autoFitWidth, bool autoFitHeight )
		{
			Dictionary<ILayoutItem, bool> tmp1, tmp2;
			return this.ResizeItem(layoutContainer, containerContext, resizeItem, deltaWidth, deltaHeight, autoFitWidth, autoFitHeight, null, out tmp1, out tmp2);
		}

		
		
		
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)


		// AS 6/29/09 NA 2009.2 Field Sizing
		/// <summary>
		/// Resizes the specified layout item by specified deltaWidth and deltaHeight. Returns the new
		/// calculated preferred sizes of one or more items that will result in the resize item's target size.
		/// </summary>
		/// <param name="layoutContainer">Layout container. Used for getting the layout area rect, which 
		/// is used when auto-fitting the layout in in an area that's different than the preferred size of the layout.
		/// </param>
		/// <param name="containerContext">Not used by the layout manager directly. It gets passed along to
		/// methods on the layout container.</param>
		/// <param name="resizeItem">Item being resized.</param>
		/// <param name="deltaWidth">Change in the width of the item. Can be 0 to indicate no change.</param>
		/// <param name="deltaHeight">Change in the height of the item. Can be 0 to indicate no change.</param>
		/// <param name="autoFitWidth">Specifies whether items will be autofitted horizontally within the 
		/// layout container rect. Items will be resized in such a way that the width of the preferred size
		/// of the resultant layout will be equal container rect's width.</param>
		/// <param name="autoFitHeight">Specifies whether items will be autofitted vertically within the 
		/// layout container rect. Items will be resized in such a way that the height of the preferred size
		/// of the resultant layout will be equal container rect's height.</param>
		/// <param name="synchronizedItems">A list of items whose size will be synchronized during the resize operation with the specified <paramref name="resizeItem"/></param>
		/// <param name="newIsWidthAutoState">When an item is resized, items aligned with the resize item need to change
		/// their is-auto state to false. This parameter will be assigned a list of items whose is-auto state needs to
		/// change to false.</param>
		/// <param name="newIsHeightAutoState">Same as newIsWidthAutoState except for heights.</param>
		/// <returns>Returns the new preferred sizes of one or more items that will result in the target size
		/// of the resize item.</returns>
		public Dictionary<ILayoutItem, Size> ResizeItem( ILayoutContainer layoutContainer, object containerContext,
			ILayoutItem resizeItem, double deltaWidth, double deltaHeight, bool autoFitWidth, bool autoFitHeight,
			IList<ILayoutItem> synchronizedItems,
			out Dictionary<ILayoutItem, bool> newIsWidthAutoState,
			out Dictionary<ILayoutItem, bool> newIsHeightAutoState )
		{
			ResizeManager manager = new ResizeManager( this, layoutContainer, containerContext, autoFitWidth, autoFitHeight );

			return this.ResizeItemHelper( manager, resizeItem, deltaWidth, deltaHeight, 
				synchronizedItems, out newIsWidthAutoState, out newIsHeightAutoState );
		}

		#endregion // ResizeItem

		#endregion // Public Methods

		#region Private Methods

		#region ApplyAlignments



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		private Rect ApplyAlignments( double x, double y, double width, double height, GridBagConstraintCache gcc )
		{
			double itemWidth = gcc._prefWidth;
			double itemHeight = gcc._prefHeight;
			double maxWidth = gcc._maxWidth;
			double maxHeight = gcc._maxHeight;

			// Apply the margin first. If IncludeMarginInPositionRect is true then leave the margin
			// in the rect. Also adjust the itemWidth/Height based on PreferredSizeIncludesMargin
			// setting.
			//
			Thickness margin = gcc._margin;
			double hhMargin = margin.Left + margin.Right;
			double vvMargin = margin.Top + margin.Bottom;
			if ( _includeMarginInPositionRect )
			{
				if ( !_preferredSizeIncludesMargin )
				{
					itemWidth += hhMargin;
					itemHeight += vvMargin;
				}

				maxWidth += hhMargin;
				maxHeight += vvMargin;
			}
			else
			{
				if ( _preferredSizeIncludesMargin )
				{
					itemWidth -= hhMargin;
					itemHeight -= vvMargin;
				}

				x += margin.Left;
				width -= hhMargin;
				y += margin.Top;
				height -= vvMargin;
			}

			// SSP 11/18/09 - NAS10.1 XamTilesControl
			// Added inter-item-spacing properties.
			// 
			// ----------------------------------------------------------
			if ( Side.None != gcc._hasSpacing )
			{
				Thickness spacing = gcc.GetSpacing( this );

				x += spacing.Left;
				width -= spacing.Left + spacing.Right;
				y += spacing.Top;
				height -= spacing.Top + spacing.Bottom;
			}
			// ----------------------------------------------------------

			// If the preferred size is bigger than the logical cell (which can happen in auto-fit situation
			// where the container rect is smaller than the preferred size of the layout), then make sure
			// that we use the smaller of the two as the final size of the item. Otherwise items will start
			// overlapping.
			// 
			if ( itemWidth > width )
				itemWidth = width;

			if ( itemHeight > height )
				itemHeight = height;

			HorizontalAlignment hAlign = gcc._gc.HorizontalAlignment;
			VerticalAlignment vAlign = gcc._gc.VerticalAlignment;

			switch ( hAlign )
			{
				case HorizontalAlignment.Left:
					// Do nothing for left.
					break;
				case HorizontalAlignment.Right:
					x += width - itemWidth;
					break;
				default:
					{
						// If Stretch then fill the available area.
						if ( HorizontalAlignment.Stretch == hAlign )
							itemWidth = Math.Min( width, maxWidth );

						// Center the item.
						x += ( width - itemWidth ) / 2;
					}
					break;
			}

			switch ( vAlign )
			{
				case VerticalAlignment.Top:
					// Do nothing for top.
					break;
				case VerticalAlignment.Bottom:
					y += height - itemHeight;
					break;
				default:
					{
						// If Stretch then fill the available area.
						if ( VerticalAlignment.Stretch == vAlign )
							itemHeight = Math.Min( height, maxHeight );

						// Center the item.
						y += ( height - itemHeight ) / 2;
					}
					break;
			}

            // JJD 3/16/09 
            // Make sure the height and width are not negative.
			//return new Rect( x, y, itemWidth, itemHeight );
			return new Rect( x, y, Math.Max(itemWidth, 0d), Math.Max(itemHeight, 0d) );
		}

		#endregion // ApplyAlignments

		#region ArrInit



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static void ArrInit( IntExpandableArray arr, int val )
		{
			GridBagLayoutManager.ArrInit( arr, val, 0, arr.Length );
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static void ArrInit( IntExpandableArray arr, int val, int startIndex, int len )
		{
			for ( int i = 0; i < len; i++ )
				arr[startIndex + i] = val;
		}

		#endregion // ArrInit

		#region ArrInitIndeces

		
		
		
		private static void ArrInitIndeces( int[] arr )
		{
			for ( int i = 0; i < arr.Length; i++ )
				arr[i] = i;
		}

		#endregion // ArrInitIndeces

		#region ArrMax



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static int ArrMax( IntExpandableArray arr, int startIndex, int len )
		{
			int max = arr[startIndex];

			for ( int i = 1; i < len; i++ )
				max = Math.Max( max, arr[startIndex + i] );

			return max;
		}

		#endregion // ArrMax

		#region ArrSet<T>

		
		
		private static void ArrSet<T>( T[] arr, T value, int startIndex, int count )
		{
			for ( int i = 0; i < count; i++ )
				arr[ startIndex + i ] = value;
		}

		#endregion // ArrSet<T>

		#region ArrSum



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static float ArrSum( float[] arr, int startIndex, int len )
		{
			float sum = 0.0f;

			for ( int i = 0; i < len; i++ )
				sum += arr[startIndex + i];

			return sum;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static double ArrSum( double[] arr )
		{
			return ArrSum( arr, 0, arr.Length );
		}

		private static double ArrSum_SkipItems( double[] arr, bool[] skipItems )
		{
			double sum = 0.0f;

			for ( int i = 0, len = arr.Length; i < len; i++ )
			{
				if ( !skipItems[i] )
					sum += arr[i];
			}

			return sum;
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static double ArrSum( double[] arr, int startIndex, int len )
		{
			double sum = 0.0f;

			for ( int i = 0; i < len; i++ )
				sum += arr[startIndex + i];

			return sum;
		}

		#endregion // ArrSum

		#region CalculateCoordinatesHelper

		private static double[] CalculateCoordinatesHelper( 
			double[] extents,
			double[] minExtents, 
			double[] maxExtents, 
			float[] weightsFloat,
			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// Added isAuto parameter.
			// 
			bool[] isAuto, 
			double containerExtent, 
			bool expandToFit,
			// SSP 12/01/09 - NAS10.1 XamTilesControl
			// Added ShrinkToFitWidth/Height properties.
			// 
			bool? shrinkToFit,
			// SSP 2/16/10 TFS27086
			// 
			out double[] calculatedExtents
			)
		{
			// We are doing + 1 because we are going to convert these extents to coordinates below
			// which require 1 more element in the array since the first element in coordinates array
			// is to be 0.0.
			// 
			// SSP 2/16/10 TFS27086
			// 
			//double[] newExtents = new double[extents.Length + 1];
			double[] newExtents = new double[extents.Length];
			extents.CopyTo( newExtents, 0 );

			double[] weights = null;
			double deltaExtent;

			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// 
			//for ( int pass = 0; pass < 2 && !IsCloseToZero( deltaExtent = containerExtent - ArrSum( newExtents ) ); pass++ )
			deltaExtent = containerExtent - ArrSum( newExtents );
			int PASS_COUNT = null != isAuto ? 3 : 2;
			for ( int pass = 0; pass < PASS_COUNT && !IsCloseToZero( deltaExtent ); pass++ )
			{
				
				
				
				
				if ( 0 == pass )
				{
					float totalOfWeights = ArrSum( weightsFloat, 0, weightsFloat.Length );
					// If no weights are specified on any of the items then continue to pass 2 so we fallback 
					// to distributing extra extent proportionally.
					// 
					if ( 0f == totalOfWeights )
						continue;

					weights = new double[extents.Length];
					for ( int i = 0; i < weights.Length; i++ )
						weights[i] = weightsFloat[i];
				}

				
				
				if ( pass > 0 )
				{
					// If ExpandToFitWidth/Height property is set to false, then don't distribute extra
					// extent unless weights are specified.
					// 
					// SSP 12/01/09 - NAS10.1 XamTilesControl
					// Added ShrinkToFitWidth/Height properties.
					// 
					// ----------------------------------------------------------------
					//if ( !expandToFit )
					//	continue;
					bool distributeDelta = deltaExtent < 0
						? ( shrinkToFit.HasValue ? shrinkToFit.Value : expandToFit )
						: expandToFit;

					if ( ! distributeDelta )
						continue;
					// ----------------------------------------------------------------

					if ( null == weights )
						weights = new double[extents.Length];

					// SSP 7/23/09 - NAS9.2 Auto-sizing
					// If the item is auto-sized then don't decrease its extent.
					// 
					// ------------------------------------------------------------
					//extents.CopyTo( weights, 0 );
					for ( int i = 0; i < weights.Length; i++ )
						// In pass 2, don't distribute any delta to auto items. Only distribute to non-auto
						// items. This essentially results in the behavior where non-auto items are given
						// preference for delta distribution over the auto-items. In pass 3 when all the
						// delta that could be distributed to the non-auto items has already been distributed
						// (in pass 2), distribute remaining delta to the auto items.
						// 
						weights[i] = ( null != isAuto && isAuto[i] ) ^ ( 1 == pass ) ? extents[i] : 0.0;
					// ------------------------------------------------------------
				}
				

				while ( !IsCloseToZero( deltaExtent ) && null != weights && NormalizeRatios( weights ) )
				{
					int constraintEncountered = 0;
					for ( int i = 0; i < weights.Length; i++ )
					{
						double delta = weights[i] * deltaExtent;
						if ( 0.0 != delta )
						{
							double newExtent = newExtents[i] + delta;

							
							
							
							//double constrainedExtent = Math.Max( Math.Min( newExtent, maxExtents[i] ), minExtents[i] );
							double constrainedExtent = Math.Max( newExtent, minExtents[i] );
							double maxExtent = maxExtents[i];
							if ( 0 != maxExtent )
								constrainedExtent = Math.Min( constrainedExtent, maxExtent );
							

							newExtents[i] = constrainedExtent;

							if ( constrainedExtent != newExtent )
							{
								// Once we've reached constraint on an item, don't apply delta to it anymore. Set weight to 0.
								// 
								weights[i] = 0.0;
								constraintEncountered++;
							}
						}
					}

					
					
					
					// Calculate the remaining extent that still needs to be re-distributed.
					// 
					deltaExtent = containerExtent - ArrSum( newExtents );

					// If no constraints were encountered then we should have distributed all the extra extent.
					// If all items have reached their constraints then extra extent can't be distributed anymore.
					// 
					if ( 0 == constraintEncountered || constraintEncountered == weights.Length )
						break;
				}
			}

			// SSP 2/16/10 TFS27086
			// 
			// ----------------------------------------------------------------------
			//ConvertToCoordinates( newExtents );
			//return newExtents;
			calculatedExtents = newExtents;
			double[] coordinates = new double[newExtents.Length + 1];
			newExtents.CopyTo( coordinates, 0 );
			
			ConvertToCoordinates( coordinates );
			return coordinates;
			// ----------------------------------------------------------------------
		}

		#endregion // CalculateCoordinatesHelper

		#region CalculateGridBagLayout

		private GridBagLayoutManager.GridBagLayoutCache CalculateGridBagLayout( )
		{
			int i, pass;
			int rows = 0;
			int cols = 0;

			GridBagConstraintCache[] tmpGccArr = null;
			GridBagConstraintCache[] gccArr = this.GetGridBagCacheHelper( out tmpGccArr );

			// Resolve the origins and spans in case Relative origins or Remainder spans
			// were specified.
			// 
			this.ResolveOrigins( gccArr, out rows, out cols );

			// Now that we have figured out which item will fall in which cell and it's dimensions as
			// far as the grid-bag cells go, figure out the widths and heights of columns and rows as well
			// as their weights.
			//
			float[] rowWeights = new float[rows];
			float[] colWeights = new float[cols];
			double[] rowHeightsPref = new double[rows];
			double[] colWidthsPref = new double[cols];
			double[] rowHeightsMin = new double[rows];
			double[] colWidthsMin = new double[cols];
			double[] rowHeightsMax = new double[rows];
			double[] colWidthsMax = new double[cols];

			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// 
			bool[] colIsAuto = null, rowIsAuto = null;

			// Sort the items according to their y and x coordinates and other factors so that
			// distributing weights and widths/heights is more accurate.
			//
			GridBagConstraintCache[] unsortedGccArr = (GridBagConstraintCache[])gccArr.Clone( );

			// Sort the gccArr by column values of the items.
			// 
			CoreUtilities.SortMergeGeneric<GridBagConstraintCache>( gccArr, tmpGccArr, new GridBagLayoutManager.LayoutItemComparer( this, false ) );

			// SSP 7/23/09 - NAS9.2 Auto-sizing
			// Added the concept of an item being auto-sized and therefore should be
			// shrunk or expanded when proportionally expanding or contracting items
			// when the container extent is different than the preferred layout extent.
			// 
			// ------------------------------------------------------------------------
			for ( i = 0; i < gccArr.Length; i++ )
			{
				GridBagConstraintCache gcc = gccArr[i];
				if ( gcc._isAutoWidth )
				{
					if ( null == colIsAuto )
						colIsAuto = new bool[cols];

					ArrSet<bool>( colIsAuto, true, gcc._column, gcc._columnSpan );
				}

				if ( gcc._isAutoHeight )
				{
					if ( null == rowIsAuto )
						rowIsAuto = new bool[rows];

					ArrSet<bool>( rowIsAuto, true, gcc._row, gcc._rowSpan );
				}
			}
			// ------------------------------------------------------------------------

			// SSP 11/18/09 - XamTilesControl
			// Added InterItemSpacing property.
			// 
			this.CalculateInterItemSpacing( gccArr, colWidthsPref, rowHeightsPref );

			// Figure out column weights.
			// 
			for ( i = 0; i < gccArr.Length; i++ )
			{
				GridBagConstraintCache gcc = gccArr[i];
				this.ResolveWeightsHelper( colWeights, gcc._column, gcc._columnSpan, gcc._columnWeight );
			}

			// Figure out the column widths.
			// 
			for ( pass = 0; pass < 3; pass++ )
			{
				double[] colWidths = 0 == pass ? colWidthsMin : ( 1 == pass ? colWidthsMax : colWidthsPref );

				for ( i = 0; i < gccArr.Length; i++ )
				{
					GridBagConstraintCache gcc = gccArr[i];
					double width = 0 == pass
						// SSP 7/23/09 - NAS9.2 Auto-sizing
						// If a layout item is designated as auto then use its pref width for the min width
						// since the item can't be displayed smaller than its auto-size.
						// 
						//? gcc._minWidth 
						? ( ! gcc._isAutoWidth ? gcc._minWidth : gcc._prefWidth )
						: ( 1 == pass ? gcc._maxWidth : gcc._prefWidth );

					// Add margins to the width. If PreferredSizeIncludesMargin is set to true then for
					// the preferred size pass don't add in the margins since the preferred size already
					// includes margins.
					// 
					if ( 2 != pass || ! _preferredSizeIncludesMargin )
						width += gcc._margin.Left + gcc._margin.Right;

					// SSP 11/18/09 - XamTilesControl
					// Added InterItemSpacing property.
					// 
					if ( 0 != ( ( Side.Left | Side.Right ) & gcc._hasSpacing ) )
					{
						Thickness spacing = gcc.GetSpacing( this );
						width += spacing.Left + spacing.Right;
					}

					this.ResolveExtentsHelper( colWidths, colWeights, gcc._column, gcc._columnSpan, width );
				}
			}

			// Sort the gccArr by row values of the items.
			// 
			CoreUtilities.SortMergeGeneric<GridBagConstraintCache>( gccArr, tmpGccArr, new GridBagLayoutManager.LayoutItemComparer( this, true ) );

			// Figure out the row weights. 
			// 
			for ( i = 0; i < gccArr.Length; i++ )
			{
				GridBagConstraintCache gcc = gccArr[i];
				this.ResolveWeightsHelper( rowWeights, gcc._row, gcc._rowSpan, gcc._rowWeight );
			}

			// Figure out the row heights.
			// 
			for ( pass = 0; pass < 3; pass++ )
			{
				double[] rowHeights = 0 == pass ? rowHeightsMin : ( 1 == pass ? rowHeightsMax : rowHeightsPref );

				for ( i = 0; i < gccArr.Length; i++ )
				{
					GridBagConstraintCache gcc = gccArr[i];
					double height = 0 == pass
						// SSP 7/23/09 - NAS9.2 Auto-sizing
						// If a layout item is designated as auto then use its pref width for the min width
						// since the item can't be displayed smaller than its auto-size.
						// 
						//? gcc._minHeight 
						? ( ! gcc._isAutoHeight ? gcc._minHeight : gcc._prefHeight )
						: ( 1 == pass ? gcc._maxHeight : gcc._prefHeight );

					// Add margins to the width. If PreferredSizeIncludesMargin is set to true then for
					// the preferred size pass don't add in the margins since the preferred size already
					// includes margins.
					// 
					if ( 2 != pass || !_preferredSizeIncludesMargin )
						height += gcc._margin.Top + gcc._margin.Bottom;

					// SSP 11/18/09 - XamTilesControl
					// Added InterItemSpacing property.
					// 
					if ( 0 != ( ( Side.Top | Side.Bottom ) & gcc._hasSpacing ) )
					{
						Thickness spacing = gcc.GetSpacing( this );
						height += spacing.Top + spacing.Bottom;
					}

					this.ResolveExtentsHelper( rowHeights, rowWeights, gcc._row, gcc._rowSpan, height );
				}
			}

			return new GridBagLayoutCache( unsortedGccArr, colWeights, rowWeights,
				colWidthsPref, rowHeightsPref, colWidthsMin, rowHeightsMin, colWidthsMax, rowHeightsMax,
				
				
				colIsAuto, rowIsAuto
				);
		}

		#endregion // CalculateGridBagLayout

		#region CalculateInterItemSpacing

		private void CalculateInterItemSpacing( GridBagConstraintCache[] gccArr, double[] colWidthsPref, double[] rowHeightsPref )
		{
			if ( this.HasInterItemSpacingHorizontal || this.HasInterItemSpacingVertical )
			{
				double horizSpacing = this.InterItemSpacingHorizontal;
				double vertSpacing = this.InterItemSpacingVertical;
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				for ( int i = 0; i < gccArr.Length; i++ )
				{
					GridBagConstraintCache gcc = gccArr[i];

					if ( gcc._column > 0 )
						gcc._hasSpacing |= Side.Left;

					if ( gcc._column + gcc._columnSpan < colWidthsPref.Length )
						gcc._hasSpacing |= Side.Right;

					if ( gcc._row > 0 )
						gcc._hasSpacing |= Side.Top;

					if ( gcc._row + gcc._rowSpan < rowHeightsPref.Length )
						gcc._hasSpacing |= Side.Bottom;
				}
				
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

				
			}
		}

		#endregion // CalculateInterItemSpacing

		#region ConvertToCoordinates

		/// <summary>
		/// Given an array that represents extents, it converts it to coordinates. The last item
		/// in the given array should be empty because coordinates require one more item than
		/// the extents.
		/// </summary>
		/// <param name="arr">Array of extents. This array will be modified to be coordinates where
		/// 0th item in the modified array will be 0.</param>
		private static void ConvertToCoordinates( double[] arr )
		{
			double a = 0.0;
			for ( int i = 0; i < arr.Length; i++ )
			{
				double b = a + arr[i];
				arr[i] = a;
				a = b;
			}
		}

		#endregion // ConvertToCoordinates

		#region GetContentOffset

		// SSP 11/19/09 - XamTilesControl
		// Added horizontal and vertical content alignment properties.
		// Refactored - moved existing code from GridBagHelper into the GetContentOffset 
		// new method and modified it to take into account the content alignment properties.
		// 
		/// <summary>
		/// Calculates the offset of contents into the container rect if the container is larger
		/// than the necessary space.
		/// </summary>
		private void GetContentOffset( Rect containerRect, out double offsetX, out double offsetY, double[] colCoordinates, double[] rowCoordinates )
		{
			offsetX = containerRect.X;
			offsetY = containerRect.Y;

			double deltaX = containerRect.Width - ( colCoordinates[colCoordinates.Length - 1] - colCoordinates[0] );
			double deltaY = containerRect.Height - ( rowCoordinates[rowCoordinates.Length - 1] - rowCoordinates[0] );

			// If the container is larger, then position items in the middle.
			//
			if ( deltaX > 0 )
			{
				// SSP 11/18/09 - XamTilesControl
				// Added content alignment properties.
				// 
				// --------------------------------------
				//offsetX += deltaX / 2;
				switch ( _horizontalContentAlignment )
				{
					case HorizontalAlignment.Right:
						offsetX += deltaX;
						break;
					case HorizontalAlignment.Center:
						offsetX += deltaX / 2;
						break;
				}
				// --------------------------------------
			}

			if ( deltaY > 0 )
			{
				// SSP 11/18/09 - XamTilesControl
				// Added content alignment properties.
				// 
				// --------------------------------------
				//offsetY += deltaY / 2;
				switch ( _verticalContentAlignment )
				{
					case VerticalAlignment.Bottom:
						offsetY += deltaY;
						break;
					case VerticalAlignment.Center:
						offsetY += deltaY / 2;
						break;
				}
				// --------------------------------------
			}
		}

		#endregion // GetContentOffset

		#region GetGridBagCacheHelper

		private GridBagConstraintCache[] GetGridBagCacheHelper( out GridBagConstraintCache[] tmpGccArr )
		{
			LayoutItemsCollection layoutItems = this.LayoutItems;
			GridBagConstraintCache[] gccArr = new GridBagConstraintCache[layoutItems.Count];

			int visibleItemCount = 0;
			for ( int i = 0, count = layoutItems.Count; i < count; i++ )
			{
				ILayoutItem item = layoutItems[i];
				IGridBagConstraint gc = (IGridBagConstraint)layoutItems.GetConstraint( item );

				if ( Visibility.Collapsed == item.Visibility )
					continue;

				GridBagConstraintCache gcc = new GridBagConstraintCache( item, gc );

				// If any of the settings are inappropriate, then hide the item.
				//
				if ( ( gcc._gcColumn < 0 && GridBagConstraintConstants.Relative != gcc._gcColumn ) ||
					( gcc._gcRow < 0 && GridBagConstraintConstants.Relative != gcc._gcRow ) ||
					( gcc._gcColumnSpan < 1 && GridBagConstraintConstants.Relative != gcc._gcColumnSpan && GridBagConstraintConstants.Remainder != gcc._gcColumnSpan ) ||
					( gcc._gcRowSpan < 1 && GridBagConstraintConstants.Relative != gcc._gcRowSpan && GridBagConstraintConstants.Remainder != gcc._gcRowSpan ) )
					continue;

				gccArr[visibleItemCount++] = gcc;
			}

			// If there were some hidden items, then truncate the gccArr so that it doesn't
			// contain any empty slots at the end.
			//
			tmpGccArr = new GridBagConstraintCache[visibleItemCount];
			if ( visibleItemCount != gccArr.Length )
			{
				Array.Copy( gccArr, tmpGccArr, visibleItemCount );

				GridBagConstraintCache[] temp = gccArr;
				gccArr = tmpGccArr;

				// Keep a temporary array that will be used for sorting later.
				//
				tmpGccArr = temp;
			}

			return gccArr;
		}

		#endregion // GetGridBagCacheHelper

		#region GetGridBagLayoutCache

		private GridBagLayoutManager.GridBagLayoutCache GetGridBagLayoutCache( )
		{
			if ( null == this._gridBagLayoutCache )
			{
				this._gridBagLayoutCache = this.CalculateGridBagLayout( );
			}

			return this._gridBagLayoutCache;
		}

		#endregion // GetGridBagLayoutCache

		#region GetTotalSize

		/// <summary>
		/// Calculates the total size.
		/// </summary>
		/// <param name="widths">Sum of these values will determine the width of the returned size.</param>
		/// <param name="heights">Sum of these values will determine the height of the returned size.</param>
		/// <returns></returns>
		private static Size GetTotalSize( double[] widths, double[] heights )
		{
			return new Size(
				GridBagLayoutManager.ArrSum( widths ),
				GridBagLayoutManager.ArrSum( heights ) );
		}

		#endregion // GetTotalSize

		#region GridBagHelper



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private GridBagLayoutItemDimensionsCollection GridBagHelper(
			ILayoutContainer container,
			object containerContext,
			bool layoutContainer,
			bool returnItemDimensions,
			// SSP 2/5/10 - NAS10.1 XamTilesControl
			// Added layoutRect parameter.
			// 
			ref object layoutRect )
		{
			int i;
			GridBagLayoutManager.GridBagLayoutCache glc = this.GetGridBagLayoutCache( );

			// SSP 2/16/10 TFS27086
			// If container rect's width or height is NaN or Infinity then use the preferred width or height.
			// 
			//Rect containerRect = container.GetBounds( containerContext );
			Rect containerRect = LayoutManagerBase.GetContainerBoundsHelper( container, containerContext, this );

			// Get the cached gridbag info.
			// 
			double[] colCoordinates = glc._colCoordinates;
			double[] rowCoordinates = glc._rowCoordinates;
			// SSP 2/16/10 TFS27086
			// 
			double[] colCoordinateExtents = glc._colCoordinateExtents;
			double[] rowCoordinateExtents = glc._rowCoordinateExtents;

			bool useCachedValues = null != colCoordinates && null != rowCoordinates &&
				colCoordinates.Length == 1 + glc._colWidthsPref.Length 
				&& rowCoordinates.Length == 1 + glc._rowHeightsPref.Length;

			if ( !useCachedValues || containerRect.Width != glc._lastContainerWidth )
			{
				// SSP 7/23/09 - NAS9.2 Auto-sizing
				// Added isAuto parameter to the CalculateCoordinatesHelper method. Pass that along.
				// 
				
				// SSP 12/01/09 - NAS10.1 XamTilesControl
				// Added ShrinkToFitWidth/Height properties.
				// 
				
				// SSP 2/16/10 TFS27086
				// 
				//colCoordinates = CalculateCoordinatesHelper( glc._colWidthsPref, glc._colWidthsMin, glc._colWidthsMax, glc._colWeights, glc._colIsAuto, containerRect.Width, this.ExpandToFitWidth, this.ShrinkToFitWidth );
				colCoordinates = CalculateCoordinatesHelper( glc._colWidthsPref, glc._colWidthsMin, glc._colWidthsMax, glc._colWeights, glc._colIsAuto, containerRect.Width, this.ExpandToFitWidth, this.ShrinkToFitWidth, out colCoordinateExtents );
				glc._colCoordinateExtents = colCoordinateExtents;

				glc._lastContainerWidth = containerRect.Width;
				glc._colCoordinates = colCoordinates;
			}

			if ( !useCachedValues || containerRect.Height != glc._lastContainerHeight )
			{
				// SSP 7/23/09 - NAS9.2 Auto-sizing
				// Added isAuto parameter to the CalculateCoordinatesHelper method. Pass that along.
				// 
				
				// SSP 12/01/09 - NAS10.1 XamTilesControl
				// Added ShrinkToFitWidth/Height properties.
				// 
				
				// SSP 2/16/10 TFS27086
				// 
				//rowCoordinates = CalculateCoordinatesHelper( glc._rowHeightsPref, glc._rowHeightsMin, glc._rowHeightsMax, glc._rowWeights, glc._rowIsAuto, containerRect.Height, this.ExpandToFitHeight, this.ShrinkToFitHeight );
				rowCoordinates = CalculateCoordinatesHelper( glc._rowHeightsPref, glc._rowHeightsMin, glc._rowHeightsMax, glc._rowWeights, glc._rowIsAuto, containerRect.Height, this.ExpandToFitHeight, this.ShrinkToFitHeight, out rowCoordinateExtents );
				glc._rowCoordinateExtents = rowCoordinateExtents;

				glc._lastContainerHeight = containerRect.Height;
				glc._rowCoordinates = rowCoordinates;
			}

			// SSP 11/19/09 - XamTilesControl
			// Added horizontal and vertical content alignment properties.
			// Refactored - moved existing code into the GetContentOffset and modified it to take into
			// account the content alignment properties.
			// 
			double offsetX, offsetY;
			this.GetContentOffset( containerRect, out offsetX, out offsetY, colCoordinates, rowCoordinates );

			GridBagLayoutItemDimensionsCollection itemDimensions = null;
			if ( returnItemDimensions )
			{
				itemDimensions = new GridBagLayoutItemDimensionsCollection( (double[])colCoordinates.Clone( ), (double[])rowCoordinates.Clone( ) );

				
				
				itemDimensions.InitializeLayoutSizes(
					GetTotalSize( glc._colWidthsPref, glc._rowHeightsPref ),
					GetTotalSize( glc._colWidthsMin, glc._rowHeightsMin ),
					GetTotalSize( glc._colWidthsMax, glc._rowHeightsMax ) );					

				// Make the colsDims and rowDims relative to the ContainerRect as opposed
				// to the LayoutRect. This never mattered in the grid because ExpandToFitHeight
				// and ExpandToFitWidth are always true.
				// 
				this.OffsetEachElem( itemDimensions.ColumnDims, offsetX - containerRect.X );
				this.OffsetEachElem( itemDimensions.RowDims, offsetY - containerRect.Y );
			}

			// SSP 2/5/10 - NAS10.1 XamTilesControl
			// Added layoutRect parameter.
			// 
			if ( null != layoutRect )
			{
				layoutRect = new Rect( colCoordinates[0] + offsetX, rowCoordinates[0] + offsetY,
					colCoordinates[colCoordinates.Length - 1] - colCoordinates[0], 
					rowCoordinates[rowCoordinates.Length - 1] - rowCoordinates[0] );
			}

			GridBagConstraintCache[] gccArr = glc._gccArr;
			for ( i = 0; i < gccArr.Length; i++ )
			{
				GridBagConstraintCache gcc = gccArr[i];

				double x = colCoordinates[gcc._column];
				double y = rowCoordinates[gcc._row];

				// SSP 2/16/10 TFS27086
				// Due to rounding errors when calculating coordinates, the width or height assigned to
				// an item can be different from its preferred width or height (even when there's no
				// auto-fit going on). The difference between the preferred width/height and the actual 
				// width/height that we end up assigning is extremely small - after all it's just a 
				// rounding error - however that can still be large enough for 
				// MS.Internal.DoubleUtil.AreClose to consider them as not close enough and thus cause
				// scrollviewer to be displayed inside the item when there's no need because of this
				// rounding error difference (that's ofcourse assuming that the item's element structure
				// has scroll viewer).
				// 
				//double w = colCoordinates[gcc._column + gcc._columnSpan] - x;
				//double h = rowCoordinates[gcc._row + gcc._rowSpan] - y;
				double w = ArrSum( colCoordinateExtents, gcc._column, gcc._columnSpan );
				double h = ArrSum( rowCoordinateExtents, gcc._row, gcc._rowSpan );
				Debug.Assert( CoreUtilities.AreClose( w, colCoordinates[gcc._column + gcc._columnSpan] - x )
					&& CoreUtilities.AreClose( h, rowCoordinates[gcc._row + gcc._rowSpan] - y ) );

				x += offsetX;
				y += offsetY;

				Rect itemRect = this.ApplyAlignments( x, y, w, h, gcc );

				if ( null != itemDimensions )
					itemDimensions.Add( gcc._item, new GridBagLayoutItemDimensions( gcc._column,
						gcc._row, gcc._columnSpan, gcc._rowSpan, itemRect ) );

				if ( layoutContainer )
					container.PositionItem( gcc._item, itemRect, containerContext );
			}

			return itemDimensions;
		}

		#endregion // GridBagHelper

		#region IsCloseToZero

		private static bool IsCloseToZero( double d )
		{
			return CoreUtilities.AreClose( d, 0.0 );
		}

		#endregion // IsCloseToZero

		#region NormalizeRatios

		/// <summary>
		/// Normalizes ratios so they sum up to 1 by proportionally adjusting each value in the array.
		/// </summary>
		/// <param name="ratios"></param>
		/// <returns></returns>
		private static bool NormalizeRatios( double[] ratios )
		{
			double t = ArrSum( ratios );
			if ( !IsCloseToZero( t ) )
			{
				if ( t != 1.0 )
				{
					for ( int i = 0; i < ratios.Length; i++ )
						ratios[i] /= t;
				}

				return true;
			}

			return false;
		}

		#endregion // NormalizeRatios

		#region OffsetEachElem

		private void OffsetEachElem( double[] arr, double offset )
		{
			for ( int i = 0; i < arr.Length; i++ )
				arr[i] += offset;
		}

		#endregion // OffsetEachElem

		#region ResizeItemHelper

		
		
		
		private Dictionary<ILayoutItem, Size> ResizeItemHelper( ResizeManager manager,
				ILayoutItem resizeItem, double deltaWidth, double deltaHeight,
				IList<ILayoutItem> synchronizedItems,
				out Dictionary<ILayoutItem, bool> newIsWidthAutoState,
				out Dictionary<ILayoutItem, bool> newIsHeightAutoState )
		{
			manager.ResizeItem( resizeItem, deltaWidth, deltaHeight, synchronizedItems );

			return manager.GetNewPreferredSizes( out newIsWidthAutoState, out newIsHeightAutoState, 0 != deltaWidth, 0 != deltaHeight );
		}

		#endregion // ResizeItemHelper

		#region ResolveExtentsHelper

		private void ResolveExtentsHelper( double[] extents, float[] weights, int origin, int span, double layoutItemExtent )
		{
			double oldExtent = GridBagLayoutManager.ArrSum( extents, origin, span );

			// If the old combined width/height of all the columns/rows is less than what is dictated
			// by the current item, distribute the extra width/height among the columns/rows according
			// to their existing weights assigning any remaining width/height to the last column/row.
			//
			if ( oldExtent < layoutItemExtent )
			{
				double extraExtent = layoutItemExtent - oldExtent;
				double newExtent = 0;

				// If the all the columns have weight of 0, then assign the extra width/height to 
				// the last column/row.
				//
				float weight = GridBagLayoutManager.ArrSum( weights, origin, span );
				if ( weight > 0.0f )
				{
					for ( int j = origin; j < origin + span; j++ )
					{
						extents[j] += (int)( extraExtent * weights[j] / weight );
						newExtent += extents[j];
					}
				}
				else
				{
					newExtent = oldExtent;
				}

				// Assign any remaining width/height to the last column/row.
				//
				if ( newExtent < layoutItemExtent )
				{
					// SSP 4/7/11 TFS32232
					// Added Uniform mode.
					// 
					
					//extents[origin + span - 1] += layoutItemExtent - newExtent;
					if ( GridBagLayoutMode.Uniform == _gridBagMode )
					{
						extraExtent = layoutItemExtent - newExtent;

						for ( int i = 0; i < span; i++ )
							extents[origin + i] += extraExtent / span;
					}
					else
					{
						extents[origin + span - 1] += layoutItemExtent - newExtent;
					}
					
				}
			}
		}

		#endregion // ResolveExtentsHelper

		#region ResolveOrigins

		private void ResolveOrigins( GridBagConstraintCache[] gccArr, out int rowCount, out int columnCount )
		{
			IntExpandableArray arrRelativeX = new IntExpandableArray( 64 );
			IntExpandableArray arrRelativeY = new IntExpandableArray( 64 );

			const int MAX_PASSES = 4;

			int rows = 0;
			int cols = 0;

			// We need to perform muliple passes through the items to figure out the Relative origins 
			// Relative/Remainder spans. In the first pass we figure out the number of columns and
			// rows this grid bag layout and then use that in the second pass to assign appropriate 
			// values to Relative/Remainder columnSpan and rowSpan and Relative column and row.
			//
			for ( int pass = 0; pass < MAX_PASSES; pass++ )
			{
				// AS 9/29/09 Optimization
				//GridBagLayoutManager.ArrInit( arrRelativeX, 0 );
				//GridBagLayoutManager.ArrInit( arrRelativeY, 0 );
				arrRelativeX.Reset();
				arrRelativeY.Reset();

				int maxCols = cols;
				int maxRows = rows;

				int nextRelativeY = 0;
				int nextRelativeX = 0;

				bool relativeOrRemainderItemsEncountered = false;

				for ( int i = 0; i < gccArr.Length; i++ )
				{
					GridBagConstraintCache gcc = gccArr[i];

					if ( gcc._gcColumn < 0 || gcc._gcRow < 0 || gcc._gcColumnSpan <= 0 || gcc._gcRowSpan <= 0 )
						relativeOrRemainderItemsEncountered = true;

					gcc._column = gcc._gcColumn;
					gcc._row = gcc._gcRow;
					gcc._columnSpan = gcc._gcColumnSpan;
					gcc._rowSpan = gcc._gcRowSpan;

					// If both the column and row are relative, then put the item right to the last 
					// added item on the current row. If the last item had a columnSpan and start a new row.
					//
					if ( gcc._column < 0 && gcc._row < 0 )
					{
						if ( nextRelativeY >= 0 )
							gcc._row = nextRelativeY;
						else if ( nextRelativeX >= 0 )
							gcc._column = nextRelativeX;
					}

					if ( gcc._column < 0 )
					{
						// If the span is Relative or Remainder, use span of at least one.
						//
						if ( gcc._gcRowSpan <= 0 )
							gcc._rowSpan = Math.Max( 1, rows - gcc._row -
								( GridBagConstraintConstants.Relative == gcc._gcRowSpan ? 1 : 0 ) );

						gcc._column = GridBagLayoutManager.ArrMax( arrRelativeX, gcc._row, gcc._rowSpan );
					}
					else if ( gcc._row < 0 )
					{
						// If the span is Relative or Remainder, use span of at least one.
						//
						if ( gcc._gcColumnSpan <= 0 )
							gcc._columnSpan = Math.Max( 1, cols - gcc._column -
								( GridBagConstraintConstants.Relative == gcc._gcColumnSpan ? 1 : 0 ) );

						gcc._row = GridBagLayoutManager.ArrMax( arrRelativeY, gcc._column, gcc._columnSpan );
					}

					// If the span is Relative or Remainder, use span of at least one.
					//
					if ( gcc._gcColumnSpan <= 0 )
						gcc._columnSpan = Math.Max( 1, cols - gcc._column -
							( GridBagConstraintConstants.Relative == gcc._gcColumnSpan ? 1 : 0 ) );

					// If the span is Relative or Remainder, use span of at least one.
					//
					if ( gcc._gcRowSpan <= 0 )
						gcc._rowSpan = Math.Max( 1, rows - gcc._row -
							( GridBagConstraintConstants.Relative == gcc._gcRowSpan ? 1 : 0 ) );

					// For the last pass, don't increase the number of cols and rows.
					//
					maxCols = Math.Max( maxCols, gcc._column + gcc._columnSpan );
					maxRows = Math.Max( maxRows, gcc._row + gcc._rowSpan );
					if ( pass < MAX_PASSES - 1 )
					{
						cols = maxCols;
						rows = maxRows;
					}

					// Update the relative x's and y's in the arrays
					//
					GridBagLayoutManager.ArrInit( arrRelativeX, gcc._column + gcc._columnSpan, gcc._row, gcc._rowSpan );
					GridBagLayoutManager.ArrInit( arrRelativeY, gcc._row + gcc._rowSpan, gcc._column, gcc._columnSpan );

					if ( GridBagConstraintConstants.Remainder == gcc._gcColumnSpan &&
						GridBagConstraintConstants.Remainder == gcc._gcRowSpan )
					{
						// It is arbitrary as to what we do when items with relative column 
						// and row are encountered after an item with columnSpan and rowSpan of
						// remainder. We will just overlay those items over this item. There
						// shouldn't really be any such items after this (or rather the user
						// should not specify any such items) otherwise they will get undefined
						// behavior.
						//
						nextRelativeY = gcc._row;
					}
					else if ( GridBagConstraintConstants.Remainder == gcc._gcColumnSpan )
					{
						// If the item has a columnSpan of Remainder, then start a new row.
						//
						nextRelativeY = gcc._row + gcc._rowSpan;
					}
					else if ( GridBagConstraintConstants.Remainder == gcc._gcRowSpan )
					{
						// If the item has a rowSpan of Remainder, then start a new column.
						//
						nextRelativeX = gcc._column + gcc._columnSpan;

						// Set the nextRelativeY to -1 so we use the nextRelativeX next time in the loop.
						//
						nextRelativeY = -1;
					}
				}

				rows = maxRows;
				cols = maxCols;

				// If no items with relative column/row or relative/remainder columnSpan/rowSpan were encountered, then
				// there is no need to do a second pass. Just break out of the loop in that case.
				//
				if ( !relativeOrRemainderItemsEncountered )
					break;
			}

			rowCount = rows;
			columnCount = cols;
		}

		#endregion // ResolveOrigins

		#region ResolveWeightsHelper

		private void ResolveWeightsHelper( float[] weights, int origin, int span, float layoutItemWeight )
		{
			float weight = GridBagLayoutManager.ArrSum( weights, origin, span );

			// If the old combined weight of all the columns is less than what is dictated
			// by the current item, distribute the extra weight among the columns according
			// to their existing weights assigning any remaining weight to the last column.
			//
			if ( weight < layoutItemWeight )
			{
				float extraWeight = layoutItemWeight - weight;
				float newWeight = 0.0f;

				// If the all the columns have weight of 0, then assign the extra weight to 
				// the last column.
				//
				if ( weight > 0.0f )
				{
					for ( int j = origin; j < origin + span; j++ )
					{
						weights[j] += weights[j] * extraWeight / weight;
						newWeight += weights[j];
					}
				}

				// Assign any remaining weight to the last column
				//
				if ( newWeight < layoutItemWeight )
					weights[origin + span - 1] += layoutItemWeight - newWeight;
			}
		}

		#endregion // ResolveWeightsHelper

		#endregion // Private Methods

		#region Internal Methods

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion //GetString

		#endregion // Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region ColumnWeights

		/// <summary>
		/// Returns the column weights of the gridbag layout. You can change them however they get recalculated 
		/// once the layout is invalidated. Layout also gets invalidated whenever a layout item is added or removed.
		/// </summary>
		public float[] ColumnWeights
		{
			get
			{
				GridBagLayoutManager.GridBagLayoutCache glc = this.GetGridBagLayoutCache();

				Debug.Assert(null != glc, "GetGridBagLayoutCache returned null !");

				if (null != glc)
					return glc._colWeights;

				return null;
			}
		}

		#endregion //ColumnWeights

		#region ColumnWidths

		/// <summary>
		/// Returns the preferred column widths of the gridbag layout. You can change them however they get recalculated 
		/// once the layout is invalidated. Layout also gets invalidated whenever a layout item is added or removed.
		/// </summary>
		public double[] ColumnWidths
		{
			get
			{
				GridBagLayoutManager.GridBagLayoutCache glc = this.GetGridBagLayoutCache( );

				Debug.Assert( null != glc, "GetGridBagLayoutCache returned null !" );

				if ( null != glc )
					return glc._colWidthsPref;

				return null;
			}
		}

		#endregion // ColumnWidths

		#region ExpandToFitHeight

		/// <summary>
		/// Indicates whether to proportionally expand or shrink the heights of all the items to fit 
		/// the layout rect. This would only get applied if all the items had 0.0 rowWeight's.
		/// </summary>
		public bool ExpandToFitHeight
		{
			get
			{
				return this._expandToFitHeight;
			}
			set
			{
				this._expandToFitHeight = value;

				this.InvalidateLayout( );
			}
		}

		#endregion // ExpandToFitHeight

		#region ExpandToFitWidth

		/// <summary>
		/// Indicates whether to proportionally expand or shrink the widths of all the items to fit 
		/// the layout rect. This would only get applied if all the items had 0.0 columnWeight's.
		/// </summary>
		public bool ExpandToFitWidth
		{
			get
			{
				return this._expandToFitWidth;
			}
			set
			{
				this._expandToFitWidth = value;

				this.InvalidateLayout( );
			}
		}

		#endregion // ExpandToFitWidth

		#region ShrinkToFitHeight

		// SSP 12/01/09 - NAS10.1 XamTilesControl
		// 
		/// <summary>
		/// Indicates whether to shrink or not shrink the items' heights to fit the available layout area.
		/// Overrides the behavior of <see cref="ExpandToFitHeight"/> property.
		/// </summary>
		public bool? ShrinkToFitHeight
		{
			get
			{
				return _shrinkToFitHeight;
			}
			set
			{
				if ( value != _shrinkToFitHeight )
				{
					_shrinkToFitHeight = value;
					this.InvalidateLayout( );
				}
			}
		}

		#endregion // ShrinkToFitHeight

		#region ShrinkToFitWidth

		// SSP 12/01/09 - NAS10.1 XamTilesControl
		// 
		/// <summary>
		/// Indicates whether to shrink or not shrink the items' widths to fit the available layout area.
		/// Overrides the behavior of <see cref="ExpandToFitWidth"/> property.
		/// </summary>
		public bool? ShrinkToFitWidth
		{
			get
			{
				return _shrinkToFitWidth;
			}
			set
			{
				if ( value != _shrinkToFitWidth )
				{
					_shrinkToFitWidth = value;
					this.InvalidateLayout( );
				}
			}
		}

		#endregion // ShrinkToFitWidth

		#region HorizontalContentAlignment

		// SSP 10/18/09 - NAS10.1 XamTilesControl
		// 
		/// <summary>
		/// Specifies where the contents are to be positioned horizontally if there is extra width available.
		/// </summary>
		public HorizontalAlignment HorizontalContentAlignment
		{
			get
			{
				return _horizontalContentAlignment;
			}
			set
			{
				if ( _horizontalContentAlignment != value )
				{
					_horizontalContentAlignment = value;
					this.InvalidateLayout( );
				}
			}
		}

		#endregion // HorizontalContentAlignment

		#region VerticalContentAlignment

		// SSP 10/18/09 - NAS10.1 XamTilesControl
			// 
		/// <summary>
		/// Specifies where the contents are to be positioned vertically if there is extra height available.
		/// </summary>
		public VerticalAlignment VerticalContentAlignment
		{
			get
			{
				return _verticalContentAlignment;
			}
			set
			{
				if ( _verticalContentAlignment != value )
				{
					_verticalContentAlignment = value;
					this.InvalidateLayout( );
				}
			}
		}

		#endregion // VerticalContentAlignment

		#region IncludeMarginInPositionRect

		/// <summary>
		/// Indicates whether to include layout item margins in the rect that's specified in when the layout
		/// item is positioned via PositionItem call on the container. This mirrors the fact that UIElement's 
		/// Arrange call takes a rect that includes the element's margins.
		/// </summary>
		public bool IncludeMarginInPositionRect
		{
			get
			{
				return _includeMarginInPositionRect;
			}
			set
			{
				if ( _includeMarginInPositionRect != value )
				{
					_includeMarginInPositionRect = value;
					this.InvalidateLayout( );
				}
			}
		}

		#endregion // IncludeMarginInPositionRect

		#region InterItemSpacingHorizontal

		// SSP 10/26/09 - NAS10.1 XamTilesControl - CalculateAutoLayout
		// 
		/// <summary>
		/// Specifies the horizontal spacing between each item.
		/// </summary>
		public double InterItemSpacingHorizontal
		{
			get
			{
				return _interItemSpacingHorizontal;
			}
			set
			{
				if ( _interItemSpacingHorizontal != value )
				{
					_interItemSpacingHorizontal = value;
					
					this.InvalidateLayout( );
				}
			}
		}

		#endregion // InterItemSpacingHorizontal

		#region InterItemSpacingVertical

		// SSP 10/26/09 - NAS10.1 XamTilesControl - CalculateAutoLayout
		// 
		/// <summary>
		/// Specifies the vertical spacing between each item.
		/// </summary>
		public double InterItemSpacingVertical
		{
			get
			{
				return _interItemSpacingVertical;
			}
			set
			{
				if ( _interItemSpacingVertical != value )
				{
					_interItemSpacingVertical = value;

					this.InvalidateLayout( );
				}
			}
		}

		#endregion // InterItemSpacingVertical

		#region LayoutMode

		/// <summary>
		/// Internal property.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public GridBagLayoutMode LayoutMode
		{
			get
			{
				return this._gridBagMode;
			}
			set
			{
				if ( this._gridBagMode != value )
				{
					if ( !Enum.IsDefined( typeof( GridBagLayoutMode ), value ) )
					{

						throw new InvalidEnumArgumentException( "LayoutMode", (int)value, typeof( GridBagLayoutMode ) );



					}

					this._gridBagMode = value;

					this.InvalidateLayout( );
				}
			}
		}

		#endregion // LayoutMode

		#region PreferredSizeIncludesMargin

		/// <summary>
		/// Indicates that the preferred size provided by layout items include their margins. This mirrors
		/// DesiredSize of UIElement which includes its margins. However note that MinimumSize and MaximumSize
		/// do not include the margins, just like the UIElement.
		/// </summary>
		public bool PreferredSizeIncludesMargin
		{
			get
			{
				return _preferredSizeIncludesMargin;
			}
			set
			{
				if ( _preferredSizeIncludesMargin != value )
				{
					_preferredSizeIncludesMargin = value;
					this.InvalidateLayout( );
				}
			}
		}

		#endregion // PreferredSizeIncludesMargin

		#region RowHeights

		/// <summary>
		/// Returns the preferred row heights of the gridbag layout. You can change them however they get recalculated 
		/// once the layout is invalidated. Layout also gets invalidated whenever a layout item is added or removed.
		/// </summary>
		public double[] RowHeights
		{
			get
			{
				GridBagLayoutManager.GridBagLayoutCache glc = this.GetGridBagLayoutCache( );

				Debug.Assert( null != glc, "GetGridBagLayoutCache returned null !" );

				if ( null != glc )
					return glc._rowHeightsPref;

				return null;
			}
		}

		#endregion // RowHeights

		#region RowWeights

		/// <summary>
		/// Returns the row weights of the gridbag layout. You can change them however they get recalculated 
		/// once the layout is invalidated. Layout also gets invalidated whenever a layout item is added or removed.
		/// </summary>
		public float[] RowWeights
		{
			get
			{
				GridBagLayoutManager.GridBagLayoutCache glc = this.GetGridBagLayoutCache();

				Debug.Assert(null != glc, "GetGridBagLayoutCache returned null !");

				if (null != glc)
					return glc._rowWeights;

				return null;
			}
		}

		#endregion //RowWeights

		#endregion // Public Properties

		#region Internal Properties

		#region HasInterItemSpacingHorizontal

		// SSP 11/18/09 - NAS10.1 XamTilesControl
		// 
		internal bool HasInterItemSpacingHorizontal
		{
			get
			{
				return !IsCloseToZero( _interItemSpacingHorizontal );
			}
		}

		#endregion // HasInterItemSpacingHorizontal

		#region HasInterItemSpacingVertical

		// SSP 11/18/09 - NAS10.1 XamTilesControl
		// 
		internal bool HasInterItemSpacingVertical
		{
			get
			{
				return !IsCloseToZero( _interItemSpacingVertical );
			}
		} 

		#endregion // HasInterItemSpacingVertical

		#endregion // Internal Properties

		#endregion // Properties
	}

	#endregion // GridBagLayoutManager Class

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