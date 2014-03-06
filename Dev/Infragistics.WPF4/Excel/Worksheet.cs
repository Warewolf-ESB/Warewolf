using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Sorting;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents one worksheet in a Microsoft Excel workbook.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// An Excel worksheet is essentially a table with a limited number of rows and columns. To create new worksheet, use 
	/// <see cref="WorksheetCollection.Add"/> method of the <see cref="Excel.Workbook.Worksheets"/> 
	/// collection on a <see cref="Workbook"/> instance.
	/// </p>
	/// </remarks>
	[DebuggerDisplay( "Worksheet: {name}" )]



	public

		 class Worksheet : 
		IWorksheetShapeOwner
	{
		#region Constants

		// MD 1/11/08 - BR29105
		// This const was not correct and is not needed anymore
		//private const double CharacterWidthToTwipsRatioAt96DPI = 0.46875;

		// MD 7/23/10 - TFS35969
		// Made internal to be used in other places.
		//// MD 1/11/08 - BR29105
		//private const double TwipsPerPixelAt96DPI = 1440 / 96.0;
		internal const double TwipsPerPixelAt96DPI = 1440 / 96.0;

		#endregion Constants

		#region Member Variables

		// MD 7/25/08 - Excel formula solving
		private List<WeakReference> cachedRegions = new List<WeakReference>();

		// MD 2/27/12
		// Found while implementing 12.1 - Table Support
		// We will now store the cached height on the row itself.
		//// MD 1/18/11 - TFS62762
		//private Dictionary<WorksheetRow, int> cachedRowHeightsWithWrappedText;

		// MD 3/16/09 - TFS14252
		// Removed seldom used member variables from the cell and cached them on the worksheet so cells don't 
		// have such a large memory footprint.
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private Dictionary<WorksheetCell, WorksheetMergedCellsRegion> cellOwnedAssociatedMergedCellsRegions;
		//private Dictionary<WorksheetCell, WorksheetRegion> cellOwnedCachedRegions;
		//private Dictionary<WorksheetCell, WorksheetCellFormatProxy> cellOwnedCellFormats;
		//private Dictionary<WorksheetCell, WorksheetCellComment> cellOwnedComments;
		//private Dictionary<WorksheetCell, List<WorksheetCell>> cellOwnedDataTableDependantCells;
		
		private Dictionary<WorksheetCellAddress, WorksheetMergedCellsRegion> cellOwnedAssociatedMergedCellsRegions;
		private Dictionary<WorksheetCellAddress, WorksheetCellComment> cellOwnedComments;
		private Dictionary<WorksheetCellAddress, List<WorksheetCellAddress>> cellOwnedDataTableDependantCells;

		// MD 4/12/11 - TFS67084
		// This holds uncommon object types which are set as the value on cells (Guid, Enum, decimal, StringBuilder).
		private Dictionary<WorksheetCellAddress, object> cellValues;

		private WorksheetColumnCollection columns;
		private WorksheetDataTableCollection dataTables;

		// MD 9/12/08 - TFS6887
		// MD 2/1/11 - Data Validation support
		//private DataValidationRoundTripInfo dataValidationInfo2003;
		//private ElementDataCache dataValidationInfo2007;

		// MD 2/10/12 - TFS97827
		// We will now store the default column width again so the column and worksheet store the widths in the same units.
		//// MD 1/11/08 - BR29105
		//// This member is not needed anymore, because the DefaultColumnWidth can be obtained from the new defaultMinimumCharsPerColumn
		////private int defaultColumnWidth;
		private int defaultColumnWidth;

		// MD 2/10/12 - TFS97827
		// These are no longer needed.
		//// MD 1/11/08 - BR29105
		//private int defaultColumnWidthResolved;
		//private int defaultColumnWidthResolved_DefaultFontVersion;

		// MD 2/10/12 - TFS97827
		// This doesn't give us much flexibility of column widths. We are now going to to storing the width in 256ths of the zero character
		// width (in actual characters spanning the column).
		//private int defaultMinimumCharsPerColumn;

		private int defaultRowHeightResolved;
		private int defaultRowHeightResolved_DefaultFontVersion;

		private int defaultRowHeight;
		private bool defaultRowHidden;
		private WorksheetDisplayOptions displayOptions;



		private Bitmap imageBackground;

		private WorksheetMergedCellsRegionCollection mergedCells;
		private string name;

		// MD 4/12/11
		// Found while fixing TFS67084
		// We use the Workbook more often than the parent collection, so we will store that instead.
		//private WorksheetCollection parentCollection;
		private Workbook workbook;

		private PrintOptions printOptions;
		private bool protectedMbr;
		private WorksheetRowCollection rows;
		private WorksheetShapeCollection shapes;

		// MD 5/4/09 - TFS17197
		// The worksheets will not keep track of their sheet type because we may have to create temporary worksheets for VB modules.
		private SheetType type;

        // MD 6/19/07 - BR23998
		// For new worksheets, like the ones which will be added when exporting a grid, default the value to false so the
        // expansion indicator will appear above the group, so it appears with the row containing its children.
		// MD 8/27/08 - Code Analysis - Performance
		//private bool showExpansionIndicatorBelowGroup = false;
		private bool showExpansionIndicatorBelowGroup;

        // MBS 7/21/08 - Excel 2007 Format
        private bool hasLoadedSheetView;

		// MD 2/1/11 - Data Validation support
		private DataValidationRuleCollection dataValidationRules;

		#region Serialization Cache

		// These are only valid when the worksheet is about to be saved

		// MD 9/2/08 - Cell Comments
		//private List<WorksheetCellCommentShape> commentShapes;				// MD 7/20/2007 - BR25039
		private List<WorksheetCellComment> commentShapes;

		private int firstRow;
		private int firstRowInUndefinedTail;
		private int firstColumn;
		private int firstColumnInUndefinedTail;
		private int maxColumnOutlineLevel;
		private int maxRowOutlineLevel;
		private uint maxAssignedShapeId;
		private uint numberOfShapes;
		private uint patriarchShapeId;
        private ImageFormat preferredImageBackgroundFormat;

        // Needs to be internal since it needs to be passed as an 'out' param
        internal string imageBackgroundId;

		#endregion Serialization Cache

        //  BF 8/18/08  Excel2007 Format
        internal string drawingPartPath = string.Empty;
        internal string drawingRelationshipId = string.Empty;

		// MD 9/10/08 - Cell Comments
		internal string legacyDrawingRelationshipId;
        // MD 10/1/08 - TFS8453
		private string vbaObjectName;

		// MD 10/24/11 - TFS89375
		internal bool hasExplicitlyLoadedDefaultColumnWidth;

		// MD 12/7/11 - 12.1 - Table Support
		private WorksheetTableCollection tables;
		private List<CellShiftOperation> cellShiftHistory;

		// MD 3/15/12 - TFS104581
		private SortedList<short, WorksheetColumnBlock> columnBlocks = new SortedList<short, WorksheetColumnBlock>();

		// MD 3/21/12 - TFS104630
		private bool shouldSaveDefaultColumnWidths256th = true;

		// MD 7/30/12 - TFS117846
		private WorksheetCellFormatData defaultColumnFormat;

		#endregion Member Variables

		#region Constructor

		// MD 4/12/11
		// Found while fixing TFS67084
		// We use the Workbook more often than the parent collection, so we will store that instead.
		//internal Worksheet( string name, WorksheetCollection parentCollection )
		//{
		//    this.parentCollection = parentCollection;
		internal Worksheet(string name, Workbook workbook)
		{
			this.workbook = workbook;

			this.name = name;

			// MD 1/22/08 - BR29105
			// This can't be hard coded, we need to calculate it now
			//this.defaultRowHeight = 255;
			this.defaultRowHeight = -1;

			// MD 3/15/12 - TFS104581
			// Initialize the column blocks collection with a default block for all columns.
			WorksheetColumnBlock defaultColumnBlock = new WorksheetColumnBlock(
				0, (short)(this.workbook.MaxColumnCount - 1),
				this.workbook.CellFormats.DefaultElement);
			defaultColumnBlock.CellFormat.IncrementReferenceCount();
			this.columnBlocks.Add(defaultColumnBlock.FirstColumnIndex, defaultColumnBlock);

			// MD 1/11/08 - BR29105
			// Use DefaultMinimumCharsPerColumn instead of defaultColumnWidth, it is more intuitive and slightly more performant.
			//this.defaultColumnWidth = 2048;
			// MD 2/10/12 - TFS97827
			// We no longer store the DefaultMinimumCharsPerColumn, so instead set the equivalent here, which is to use 8 characters excluding 
			// padding but round up to the nearest multiple of 8 pixels (this is done by passing in True as the last parameter).
			//this.DefaultMinimumCharsPerColumn = 8;
			this.SetDefaultColumnWidth(8, WorksheetColumnWidthUnit.CharacterPaddingExcluded, true);
		}

		#endregion Constructor

		#region Interfaces

		#region IWorksheetShapeOwner Members

		bool IWorksheetShapeOwner.AreChildrenTopMost
		{
			get { return true; }
		}

		void IWorksheetShapeOwner.OnChildShapeBoundsChanged( WorksheetShape childShape )
		{
			Debug.Assert( this.Shapes.Contains( childShape ) );
		}

		void IWorksheetShapeOwner.OnShapeAdded( WorksheetShape shape ) { }

		void IWorksheetShapeOwner.OnShapeRemoved( WorksheetShape shape ) { }

		Worksheet IWorksheetShapeOwner.Worksheet
		{
			get { return this; }
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		// MD 8/26/08 - Excel formula solving
		#region GetCell

		/// <summary>
		/// Gets the cell at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
        /// The <see cref="Excel.Workbook.CellReferenceMode"/> of the workbook will be used to parse the cell address.
		/// </p>
		/// <p class="body">
		/// If one or more region references are specified instead of a cell reference, the top-left cell of the first region will be returned.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the cell.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address. The overload taking an origin cell must be used to resolve relative R1C1 references.
		/// </exception>
		/// <returns>A cell represented by the specified address or name.</returns>
        /// <seealso cref="Excel.Workbook.CellReferenceMode"/>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetCell GetCell( string address )
		{
			return this.GetCell( address, this.CellReferenceMode, null );
		}

		/// <summary>
		/// Gets the cell at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If one or more region references are specified instead of a cell reference, the top-left cell of the first region will be returned.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the cell.</param>
		/// <param name="cellReferenceMode">The cell reference mode to use to parse the cell address.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the specified cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address. The overload taking an origin cell must be used to resolve relative R1C1 references.
		/// </exception>
		/// <returns>A cell represented by the specified address or name.</returns>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetCell GetCell( string address, CellReferenceMode cellReferenceMode )
		{
			return this.GetCell( address, cellReferenceMode, null );
		}

		/// <summary>
		/// Gets the cell at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
        /// The <see cref="Excel.Workbook.CellReferenceMode"/> of the workbook will be used to parse the cell address.
		/// </p>
		/// <p class="body">
		/// If one or more region references are specified instead of a cell reference, the top-left cell of the first region will be returned.
		/// </p>
		/// <p class="body">
		/// The origin cell specified will not be used if a name is specified, if the workbook has an A1 cell reference mode, or if an absolute R1C1 
		/// address is specified.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the cell.</param>
		/// <param name="originCell">The origin for resolving relative R1C1 references.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address and <paramref name="originCell"/> is null. An origin cell must be specified to resolve relative 
		/// R1C1 references.
		/// </exception>
		/// <returns>A cell represented by the specified address or name.</returns>
        /// <seealso cref="Excel.Workbook.CellReferenceMode"/>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetCell GetCell( string address, WorksheetCell originCell )
		{
			return this.GetCell( address, this.CellReferenceMode, originCell );
		}

		/// <summary>
		/// Gets the cell at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If one or more region references are specified instead of a cell reference, the top-left cell of the first region will be returned.
		/// </p>
		/// <p class="body">
		/// The origin cell specified will not be used if a name is specified, if the workbook has an A1 cell reference mode, or if an absolute R1C1 
		/// address is specified.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the cell.</param>
		/// <param name="cellReferenceMode">The cell reference mode to use to parse the cell address.</param>
		/// <param name="originCell">The origin for resolving relative R1C1 references.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the specified cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address and <paramref name="originCell"/> is null. An origin cell must be specified to resolve relative 
		/// R1C1 references.
		/// </exception>
		/// <returns>A cell represented by the specified address or name.</returns>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetCell GetCell( string address, CellReferenceMode cellReferenceMode, WorksheetCell originCell )
		{
			// MD 2/27/12 - TFS102520
			//WorksheetRegion region;
			WorksheetRegion[] regions;

			WorksheetCell cell;

			// MD 2/24/12 - 12.1 - Table Support
			//this.GetCellOrRegionHelper( address, cellReferenceMode, originCell, out cell, out region );
			bool throwError;
			this.GetCellOrRegionHelper(address, cellReferenceMode, originCell, out cell, out regions, out throwError);

			if ( cell != null )
				return cell;

			// MD 2/27/12 - TFS102520
			//if ( region != null )
			//{
			//    // MD 4/12/11 - TFS67084
			//    // Moved away from using WorksheetCell objects.
			//    //return region.TopLeftCell;
			//    return region.TopRow.Cells[region.FirstColumnInternal];
			//}
			if (regions != null && regions.Length != 0)
			{
				WorksheetRegion region = regions[0];
				return region.TopRow.Cells[region.FirstColumnInternal];
			}

			// MD 2/24/12 - 12.1 - Table Support
			if (throwError == false)
				return null;

			throw new ArgumentException( SR.GetString( "LE_ArgumentException_InvalidCellAddress" ), "address" );
		}

		#endregion GetCell

		// MD 2/10/12 - TFS97827
		#region GetDefaultColumnWidth

		/// <summary>
		/// Gets the default column width in the specified units.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If <paramref name="units"/> is Character256th, the value returned will be the same as the value of the 
		/// <see cref="DefaultColumnWidth"/> property.
		/// </p>
		/// </remarks>
		/// <param name="units">The units in which the width should be returned.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="units"/> is not defined in the <see cref="WorksheetColumnWidthUnit"/> enumeration.
		/// </exception>
		/// <returns>The default column width in the specified units.</returns>
		/// <seealso cref="DefaultColumnWidth"/>
		/// <seealso cref="SetDefaultColumnWidth(double,WorksheetColumnWidthUnit)"/>
		/// <seealso cref="WorksheetColumn.GetWidth"/>
		public double GetDefaultColumnWidth(WorksheetColumnWidthUnit units)
		{
			double value = this.ConvertFromCharacter256thsInt(this.DefaultColumnWidth, units);
			if (Double.IsNaN(value))
			{
				Utilities.DebugFail("This is unexpected.");
				return 0;
			}

			return value;
		} 

		#endregion // GetDefaultColumnWidth

		// MD 8/26/08 - Excel formula solving
		#region GetRegion

		/// <summary>
		/// Gets the region at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
        /// The <see cref="Excel.Workbook.CellReferenceMode"/> of the workbook will be used to parse the region address.
		/// </p>
		/// <p class="body">
		/// If a cell reference is specified instead of a region reference, a 1x1 region containing the cell at the address will be returned.
		/// </p>
		/// <p class="body">
		/// If a list of references is specified, the region specified by the first reference will be returned.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address. The overload taking an origin cell must be used to resolve relative R1C1 references.
		/// </exception>
		/// <returns>A region represented by the specified address or name.</returns>
        /// <seealso cref="Excel.Workbook.CellReferenceMode"/>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion GetRegion( string address )
		{
			return this.GetRegion( address, this.CellReferenceMode, null );
		}

		/// <summary>
		/// Gets the region at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If a cell reference is specified instead of a region reference, a 1x1 region containing the cell at the address will be returned.
		/// </p>
		/// <p class="body">
		/// If a list of references is specified, the region specified by the first reference will be returned.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <param name="cellReferenceMode">The cell reference mode to use to parse the region address.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the specified cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address. The overload taking an origin cell must be used to resolve relative R1C1 references.
		/// </exception>
		/// <returns>A region represented by the specified address or name.</returns>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion GetRegion( string address, CellReferenceMode cellReferenceMode )
		{
			return this.GetRegion( address, cellReferenceMode, null );
		}

		/// <summary>
		/// Gets the region at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
        /// The <see cref="Excel.Workbook.CellReferenceMode"/> of the workbook will be used to parse the region address.
		/// </p>
		/// <p class="body">
		/// If a cell reference is specified instead of a region reference, a 1x1 region containing the cell at the address will be returned.
		/// </p>
		/// <p class="body">
		/// If a list of references is specified, the region specified by the first reference will be returned.
		/// </p>
		/// <p class="body">
		/// The origin cell specified will not be used if a name is specified, if the workbook has an A1 cell reference mode, or if an absolute R1C1 
		/// address is specified.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <param name="originCell">The origin for resolving relative R1C1 references.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address and <paramref name="originCell"/> is null. An origin cell must be specified to resolve relative 
		/// R1C1 references.
		/// </exception>
		/// <returns>A region represented by the specified address or name.</returns>
        /// <seealso cref="Excel.Workbook.CellReferenceMode"/>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion GetRegion( string address, WorksheetCell originCell )
		{
			return this.GetRegion( address, this.CellReferenceMode, originCell );
		}

		/// <summary>
		/// Gets the region at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If a cell reference is specified instead of a region reference, a 1x1 region containing the cell at the address will be returned.
		/// </p>
		/// <p class="body">
		/// If a list of references is specified, the region specified by the first reference will be returned.
		/// </p>
		/// <p class="body">
		/// The origin cell specified will not be used if a name is specified, if the workbook has an A1 cell reference mode, or if an absolute R1C1 
		/// address is specified.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to a cell or region in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <param name="cellReferenceMode">The cell reference mode to use to parse the region address.</param>
		/// <param name="originCell">The origin for resolving relative R1C1 references.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the specified cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address and <paramref name="originCell"/> is null. An origin cell must be specified to resolve relative 
		/// R1C1 references.
		/// </exception>
		/// <returns>A region represented by the specified address or name.</returns>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion GetRegion( string address, CellReferenceMode cellReferenceMode, WorksheetCell originCell )
		{
			// MD 2/27/12 - TFS102520
			//WorksheetRegion region;
			WorksheetRegion[] regions;

			WorksheetCell cell;

			// MD 2/24/12 - 12.1 - Table Support
			//this.GetCellOrRegionHelper( address, cellReferenceMode, originCell, out cell, out region );
			bool throwError;
			this.GetCellOrRegionHelper(address, cellReferenceMode, originCell, out cell, out regions, out throwError);

			// MD 2/27/12 - TFS102520
			//if ( region != null )
			//    return region;
			if (regions != null && regions.Length != 0)
				return regions[0];

			if (cell != null)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//return cell.CachedRegion;
				return this.GetCachedRegion(cell.RowIndex, cell.ColumnIndexInternal, cell.RowIndex, cell.ColumnIndexInternal);
			}

			// MD 2/24/12 - 12.1 - Table Support
			if (throwError == false)
				return null;

			throw new ArgumentException( SR.GetString( "LE_ArgumentException_InvalidRegionAddress" ), "address" );
		}

		#endregion GetRegion 

		// MD 2/27/12 - TFS102520
		#region GetRegions

		/// <summary>
		/// Gets the regions at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The <see cref="Excel.Workbook.CellReferenceMode"/> of the workbook will be used to parse the region address.
		/// </p>
		/// <p class="body">
		/// The address can be a list of references, each one referring to a separate region on the Worksheet.
		/// </p>
		/// <p class="body">
		/// If a cell or single region reference is specified instead, an array of one region at the address will be returned.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to cells or regions in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address. The overload taking an origin cell must be used to resolve relative R1C1 references.
		/// </exception>
		/// <returns>An array of regions represented by the specified address or name.</returns>
		/// <seealso cref="Excel.Workbook.CellReferenceMode"/>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion[] GetRegions(string address)
		{
			return this.GetRegions(address, this.CellReferenceMode, null);
		}

		/// <summary>
		/// Gets the regions at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The address can be a list of references, each one referring to a separate region on the Worksheet.
		/// </p>
		/// <p class="body">
		/// If a cell or single region reference is specified instead, an array of one region at the address will be returned.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to cells or regions in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <param name="cellReferenceMode">The cell reference mode to use to parse the region address.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the specified cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address. The overload taking an origin cell must be used to resolve relative R1C1 references.
		/// </exception>
		/// <returns>An array of regions represented by the specified address or name.</returns>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion[] GetRegions(string address, CellReferenceMode cellReferenceMode)
		{
			return this.GetRegions(address, cellReferenceMode, null);
		}

		/// <summary>
		/// Gets the regions at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The <see cref="Excel.Workbook.CellReferenceMode"/> of the workbook will be used to parse the region address.
		/// </p>
		/// <p class="body">
		/// The address can be a list of references, each one referring to a separate region on the Worksheet.
		/// </p>
		/// <p class="body">
		/// If a cell or single region reference is specified instead, an array of one region at the address will be returned.
		/// </p>
		/// <p class="body">
		/// The origin cell specified will not be used if a name is specified, if the workbook has an A1 cell reference mode, or if an absolute R1C1 
		/// address is specified.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to cells or regions in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <param name="originCell">The origin for resolving relative R1C1 references.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address and <paramref name="originCell"/> is null. An origin cell must be specified to resolve relative 
		/// R1C1 references.
		/// </exception>
		/// <returns>An array of regions represented by the specified address or name.</returns>
		/// <seealso cref="Excel.Workbook.CellReferenceMode"/>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion[] GetRegions(string address, WorksheetCell originCell)
		{
			return this.GetRegions(address, this.CellReferenceMode, originCell);
		}

		/// <summary>
		/// Gets the regions at the specified address or name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The address can be a list of references, each one referring to a separate region on the Worksheet.
		/// </p>
		/// <p class="body">
		/// If a cell or single region reference is specified instead, an array of one region at the address will be returned.
		/// </p>
		/// <p class="body">
		/// The origin cell specified will not be used if a name is specified, if the workbook has an A1 cell reference mode, or if an absolute R1C1 
		/// address is specified.
		/// </p>
		/// <p class="body">
		/// If a name is specified, it must refer to cells or regions in the <see cref="Worksheet"/> and it must be scoped to the <see cref="Workbook"/> 
		/// or the Worksheet or null will be returned.
		/// </p>
		/// </remarks>
		/// <param name="address">The address or name of the region.</param>
		/// <param name="cellReferenceMode">The cell reference mode to use to parse the region address.</param>
		/// <param name="originCell">The origin for resolving relative R1C1 references.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="address"/> is not a valid name or a valid cell or region address in the specified cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="address"/> is a relative R1C1 address and <paramref name="originCell"/> is null. An origin cell must be specified to resolve relative 
		/// R1C1 references.
		/// </exception>
		/// <returns>An array of regions represented by the specified address or name.</returns>
		/// <seealso cref="NamedReference.ReferencedCell"/>
		/// <seealso cref="NamedReference.ReferencedRegion"/>
		/// <seealso cref="NamedReference.ReferencedRegions"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion[] GetRegions(string address, CellReferenceMode cellReferenceMode, WorksheetCell originCell)
		{
			WorksheetRegion[] regions;
			WorksheetCell cell;

			bool throwError;
			this.GetCellOrRegionHelper(address, cellReferenceMode, originCell, out cell, out regions, out throwError);

			if (regions != null)
				return regions;

			if (cell != null)
				return new WorksheetRegion[] { this.GetCachedRegion(cell.RowIndex, cell.ColumnIndexInternal, cell.RowIndex, cell.ColumnIndexInternal) };

			if (throwError == false)
				return null;

			throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidRegionAddress"), "address");
		}

		#endregion GetRegions

		// MD 9/9/08 - Worksheet Moving
		#region MoveToIndex

		/// <summary>
		/// Moves the worksheet to a new position in the owning workbook's collections of worksheets.
		/// </summary>
		/// <param name="index">The new 0-based index to where the worksheet should be moved.</param>
		/// <exception cref="InvalidOperationException">
		/// The worksheet has previously been removed from its workbook.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to the number of worksheets in the owning workbook.
		/// </exception>
        /// <seealso cref="Excel.Workbook.Worksheets"/>
		/// <seealso cref="WorksheetCollection.IndexOf"/>
		/// <seealso cref="Index"/>
		public void MoveToIndex( int index )
		{
			// MD 4/12/11
			// Found while fixing TFS67084
			// We use the Workbook more often than the parent collection, so we will store that instead.
			//if ( this.parentCollection == null )
			//    throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CannotMoveDisconnectedWorksheet" ) );
			//
			//if ( index < 0 || this.parentCollection.Count <= index )
			//    throw new ArgumentOutOfRangeException( "index" );
			//
			//this.parentCollection.MoveWorksheet( this.Index, index );
			if (this.workbook == null)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotMoveDisconnectedWorksheet"));

			if (index < 0 || this.workbook.Worksheets.Count <= index)
				throw new ArgumentOutOfRangeException("index");

			this.workbook.Worksheets.MoveWorksheet(this.Index, index);
		} 

		#endregion MoveToIndex

		// MD 2/10/12 - TFS97827
		#region SetDefaultColumnWidth

		/// <summary>
		/// Sets the default column width in the specified units.
		/// </summary>
		/// <param name="value">The default column width to set on the worksheet, expressed in the specified <paramref name="units"/>.</param>
		/// <param name="units">The units in which the <paramref name="value"/> is expressed.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="value"/> is infinity or NaN.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="units"/> is not defined in the <see cref="WorksheetColumnWidthUnit"/> enumeration.
		/// </exception>
		/// <seealso cref="DefaultColumnWidth"/>
		/// <seealso cref="GetDefaultColumnWidth"/>
		/// <seealso cref="WorksheetColumn.SetWidth"/>
		public void SetDefaultColumnWidth(double value, WorksheetColumnWidthUnit units)
		{
			this.SetDefaultColumnWidth(value, units, false);
		}

		internal void SetDefaultColumnWidth(double value, WorksheetColumnWidthUnit units, bool alignTo8Pixels)
		{
			if (double.IsNaN(value))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_NaNDefaultColumnWidth"), "value");

			this.DefaultColumnWidth = this.ConvertToCharacter256thsInt(value, units);

			// MD 3/21/12 - TFS104630
			// If the default column width is explicitly set by the developer, we should write out the STANDARDWIDTH Record
			if (this.Workbook != null && this.Workbook.IsLoading == false)
				this.ShouldSaveDefaultColumnWidths256th = true;

			if (alignTo8Pixels)
			{
				double pixels = this.GetDefaultColumnWidth(WorksheetColumnWidthUnit.Pixel);
				pixels = Math.Ceiling(pixels / 8) * 8;
				this.DefaultColumnWidth = this.ConvertToCharacter256thsInt(pixels, WorksheetColumnWidthUnit.Pixel);
			}
		}

		#endregion // SetDefaultColumnWidth

		#endregion Public Methods

		#region Internal Methods

		// MD 8/21/08 - Excel formula solving
		#region AddCachedRegion

		internal void AddCachedRegion( WorksheetRegion region, out WorksheetRegion foundRegion )
		{
			// MD 2/29/12 - 12.1 - Table Support
			// Moved all code to a new overload.
			this.AddCachedRegion(region, false, out foundRegion);
		}

		// MD 2/29/12 - 12.1 - Table Support
		private void AddCachedRegion(WorksheetRegion region, bool allowDuplicates, out WorksheetRegion foundRegion)
		{
			// MD 11/1/10
			// Found while fixing TFS56976
			// This logic was incorrect because it was just doing a binary search in a list of weak references without accounting 
			// for the fact that the entries could get released, which would mess up the sort order and cause binary searches to fail. 
			// Call off to the new helper method which accounts for this while searching.
			//foundRegion = null;
			//
			//WeakReference regionReference = new WeakReference( region );
			//int index = this.cachedRegions.BinarySearch( regionReference, WorksheetRegion.HorizontalSorter.Instance );
			//
			//if ( index >= 0 )
			//{
			//    foundRegion = (WorksheetRegion)Utilities.GetWeakReferenceTarget( this.cachedRegions[ index ] );
			//
			//    if ( foundRegion != null )
			//        return;
			//
			//    this.cachedRegions[ index ] = regionReference;
			//}
			//else
			//{
			//    this.cachedRegions.Insert( ~index, regionReference );
			//}
			// MD 2/29/12 - 12.1 - Table Support
			//int index = Utilities.BinarySearchWeakReferences<WorksheetRegion>(this.cachedRegions, region, WorksheetRegion.HorizontalSorter.Instance, out foundRegion);
			int index = Utilities.BinarySearchWeakReferences<WorksheetRegion>(this.cachedRegions, region, WorksheetRegion.HorizontalSorter.Instance, allowDuplicates, out foundRegion);

			if (index < 0)
				this.cachedRegions.Insert(~index, new WeakReference(region));
		} 

		#endregion AddCachedRegion

		// MD 2/23/12 - 12.1 - Table Support
		#region AreCellsNonTrivial






		internal bool AreCellsNonTrivial(int rowIndex, short firstColumnIndex, short lastColumnIndex)
		{
			return this.AreCellsNonTrivial(rowIndex, firstColumnIndex, lastColumnIndex, false);
		}

		private bool AreCellsNonTrivial(int rowIndex, short firstColumnIndex, short lastColumnIndex, bool isPerformingShift)
		{
			// We don't have to use a cached region for this, because we will discard it when we are done.
			WorksheetRegionAddress regionOfCells = new WorksheetRegionAddress(rowIndex, rowIndex, firstColumnIndex, lastColumnIndex);

			if (this.Workbook != null)
			{
				WorksheetRow row = this.Rows.GetIfCreated(rowIndex);
				if (row != null)
				{
					for (short i = firstColumnIndex; i <= lastColumnIndex; i++)
					{
						CellCalcReference cellCalcReference;
						if (row.TryGetCellCalcReference(i, out cellCalcReference) == false)
							continue;

						
						if (isPerformingShift == false && this.Workbook.CalcEngine.IsReferenced(cellCalcReference))
							return true;
					}
				}
			}

			if (this.HasMergedCellRegions)
			{
				
				for (int i = 0; i < this.MergedCellsRegions.Count; i++)
				{
					WorksheetMergedCellsRegion region = this.MergedCellsRegions[i];
					if (region.FirstRow <= rowIndex && rowIndex <= region.LastRow)
					{
						if (firstColumnIndex <= region.LastColumnInternal && region.FirstColumnInternal <= lastColumnIndex)
						{
							return true;
						}
					}
				}
			}

			
			for (int i = 0; i < this.Tables.Count; i++)
			{
				WorksheetTable table = this.Tables[i];
				WorksheetRegionAddress tableAddress = table.WholeTableAddress;

				if (tableAddress.FirstRowIndex <= rowIndex && rowIndex <= tableAddress.LastRowIndex)
				{
					if (firstColumnIndex <= tableAddress.LastColumnIndex && tableAddress.FirstColumnIndex <= lastColumnIndex)
					{
						return true;
					}
				}
			}

			
			for (int i = 0; i < this.DataTables.Count; i++)
			{
				WorksheetDataTable table = this.DataTables[i];
				WorksheetRegion region = table.CellsInTable;
				if (region == null || region.Worksheet == null)
				{
					Utilities.DebugFail("This is unexpected.");
					continue;
				}

				if (region.FirstRow <= rowIndex && rowIndex <= region.LastRow)
				{
					if (firstColumnIndex <= region.LastColumnInternal && region.FirstColumnInternal <= lastColumnIndex)
					{
						return true;
					}
				}
			}

			if (this.IsDataPresentInRow(rowIndex, firstColumnIndex, lastColumnIndex))
				return true;

			if (isPerformingShift == false &&
				this.HasDataValidationRules &&
				this.DataValidationRules.Contains(regionOfCells))
			{
				return true;
			}

			return false;
		}

		#endregion // AreCellsNonTrivial

		#region AssignShapeIds



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 9/10/08 - Cell Comments
		// In Excel 2007 format, only comments need shape ids. A parameter has been added to indicate what should get ids.
		//internal void AssignShapeIds( ref uint nextShapeId )
		// MD 10/30/11 - TFS90733
		// Changed this method signature because it now does more than just assign shape ids.
		//internal void AssignShapeIds( ref uint nextShapeId, bool onlyAssignIdsToCommentShapes )
		internal void InitializeShapes(ref uint nextShapeId, ref ushort nextObjId, bool onlyAssignIdsToCommentShapes)
		{
			// The worksheet itself counts as one shape: the patriarch shape
			this.numberOfShapes = 1;
			this.patriarchShapeId = nextShapeId++;

			// Assign shape ids to the child shapes
			// MD 9/10/08 - Cell Comments
			// Regaular shapes might not need shape ids anymore.
			//this.AssignShapeIdsHelpher( this.Shapes, ref nextShapeId );
			if ( onlyAssignIdsToCommentShapes == false )
			{
				// MD 10/30/11 - TFS90733
				//this.AssignShapeIdsHelpher( this.Shapes, ref nextShapeId );
				this.InitializeShapesHelper(this.Shapes, ref nextShapeId, ref nextObjId);
			}

			// MD 9/2/08 - Cell Comments
			// Assign shape ids to the comments in cells.
			// MD 10/30/11 - TFS90733
			//this.AssignShapeIdsHelpher( this.CommentShapes, ref nextShapeId );
			this.InitializeShapesHelper(this.CommentShapes, ref nextShapeId, ref nextObjId);

			// Store the last used shape id
			this.maxAssignedShapeId = nextShapeId - 1;
		}

		#endregion AssignShapeIds

		// MD 7/20/2007 - BR25039
		// We need to release references created during serialization
		#region CleanupAfterSerialization

		internal void CleanupAfterSerialization()
		{
			this.commentShapes = null;
		}

		#endregion CleanupAfterSerialization

		// MD 2/10/12 - TFS97827
		#region ConvertCharactersPaddingExcludedToPixels

		internal double ConvertCharactersPaddingExcludedToPixels(double displayWidth)
		{
			double zeroCharacterWidth = this.ZeroCharacterWidth;
			int padding = Worksheet.GetColumnWidthPadding(zeroCharacterWidth);

			double pixelsForSingleCharacterDisplayWidth = zeroCharacterWidth + padding;

			double pixels;
			if (displayWidth < 1)
				pixels = displayWidth * pixelsForSingleCharacterDisplayWidth;
			else
				pixels = (displayWidth * zeroCharacterWidth) + padding;

			pixels = MathUtilities.MidpointRoundingAwayFromZero(pixels);
			if (this.DisplayOptions.ShowFormulasInCells)
				pixels *= 2;

			return pixels;
		}

		#endregion // ConvertCharactersPaddingExcludedToPixels

		// MD 2/10/12 - TFS97827
		#region ConvertCharactersToPixels

		internal double ConvertCharactersToPixels(double width)
		{
			double pixels = MathUtilities.MidpointRoundingAwayFromZero(width * this.ZeroCharacterWidth);
			if (this.DisplayOptions.ShowFormulasInCells)
				pixels *= 2;

			return pixels;
		}

		#endregion // ConvertCharactersToPixels

		// MD 2/10/12 - TFS97827
		#region ConvertFromCharacter256thsInt

		internal double ConvertFromCharacter256thsInt(int character256ths, WorksheetColumnWidthUnit units)
		{
			Utilities.VerifyEnumValue(units);

			if (character256ths < 0)
				return double.NaN;

			double actualCharacters256ths = character256ths;
			if (units == WorksheetColumnWidthUnit.Character256th)
				return actualCharacters256ths;

			double actualCharacters = actualCharacters256ths / 256;
			if (units == WorksheetColumnWidthUnit.Character)
				return actualCharacters;

			double pixels = this.ConvertCharactersToPixels(actualCharacters);
			if (units == WorksheetColumnWidthUnit.Pixel)
				return pixels;

			if (units == WorksheetColumnWidthUnit.CharacterPaddingExcluded)
				return this.ConvertPixelsToCharactersPaddingExcluded(pixels);

			double twips = Worksheet.ConvertPixelsToTwips(pixels);
			if (units == WorksheetColumnWidthUnit.Twip)
				return twips;

			if (units == WorksheetColumnWidthUnit.Point)
				return Worksheet.ConvertTwipsToPoints(twips);

			Utilities.DebugFail("Unknown WidthUnits: " + units);
			return double.NaN;
		}

		#endregion // ConvertFromCharacter256thsInt

		// MD 2/10/12 - TFS97827
		#region ConvertPixelsToCharacters

		internal double ConvertPixelsToCharacters(double pixels)
		{
			if (this.DisplayOptions.ShowFormulasInCells)
				pixels = Math.Floor(pixels / 2);

			return pixels / this.ZeroCharacterWidth;
		}

		#endregion // ConvertPixelsToCharacters

		// MD 2/10/12 - TFS97827
		#region ConvertPixelsToCharactersPaddingExcluded

		internal double ConvertPixelsToCharactersPaddingExcluded(double pixels)
		{
			if (this.DisplayOptions.ShowFormulasInCells)
				pixels = Math.Floor(pixels / 2);

			double zeroCharacterWidth = this.ZeroCharacterWidth;
			int padding = Worksheet.GetColumnWidthPadding(zeroCharacterWidth);

			double pixelsForSingleCharacterDisplayWidth = zeroCharacterWidth + padding;

			double rawValue;
			if (pixels < pixelsForSingleCharacterDisplayWidth)
				rawValue = pixels / pixelsForSingleCharacterDisplayWidth;
			else
				rawValue = (pixels - padding) / zeroCharacterWidth;

			return MathUtilities.MidpointRoundingAwayFromZero(rawValue, 2);
		}

		#endregion // ConvertPixelsToCharactersPaddingExcluded

		// MD 2/10/12 - TFS97827
		#region ConvertPixelsToTwips

		internal static double ConvertPixelsToTwips(double pixels)
		{
			return pixels * Worksheet.TwipsPerPixelAt96DPI;
		}

		#endregion // ConvertPixelsToTwips

		// MD 2/10/12 - TFS97827
		#region ConvertPointsToTwips

		internal static double ConvertPointsToTwips(double points)
		{
			return MathUtilities.MidpointRoundingAwayFromZero(points * 20);
		}

		#endregion // ConvertPointsToTwips

		// MD 2/10/12 - TFS97827
		#region ConvertToCharacter256thsInt

		internal int ConvertToCharacter256thsInt(double value, WorksheetColumnWidthUnit units)
		{
			if (Double.IsInfinity(value))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InfiniteColumnWidth"), "value");

			Utilities.VerifyEnumValue(units);

			if (Double.IsNaN(value))
				return -1;

			switch (units)
			{
				case WorksheetColumnWidthUnit.Character:
					value *= 256;
					goto case WorksheetColumnWidthUnit.Character256th;

				case WorksheetColumnWidthUnit.Character256th:
					return (int)MathUtilities.Truncate(value);

				case WorksheetColumnWidthUnit.CharacterPaddingExcluded:
					value = this.ConvertCharactersPaddingExcludedToPixels(value);
					goto case WorksheetColumnWidthUnit.Pixel;

				case WorksheetColumnWidthUnit.Pixel:
					value = this.ConvertPixelsToCharacters(value);
					goto case WorksheetColumnWidthUnit.Character;

				case WorksheetColumnWidthUnit.Point:
					value = Worksheet.ConvertPointsToTwips(value);
					goto case WorksheetColumnWidthUnit.Twip;

				case WorksheetColumnWidthUnit.Twip:
					value = Worksheet.ConvertTwipsToPixels(value);
					goto case WorksheetColumnWidthUnit.Pixel;

				default:
					Utilities.DebugFail("Unknown WidthUnits: " + units);
					goto case WorksheetColumnWidthUnit.Character256th;
			}
		}

		#endregion // ConvertToCharacter256thsInt

		// MD 2/10/12 - TFS97827
		#region ConvertTwipsToPixels

		internal static double ConvertTwipsToPixels(double twips)
		{
			return MathUtilities.MidpointRoundingAwayFromZero(twips / Worksheet.TwipsPerPixelAt96DPI);
		} 

		#endregion // ConvertTwipsToPixels

		// MD 2/10/12 - TFS97827
		#region ConvertTwipsToPoints

		internal static double ConvertTwipsToPoints(double twips)
		{
			return twips / 20;
		}

		#endregion // ConvertTwipsToPoints

		// MD 2/22/12 - 12.1 - Table Support
		#region DeleteCellsAndShiftUp

		internal CellShiftResult DeleteCellsAndShiftUp(int rowIndex, short firstColumnIndex, short lastColumnIndex)
		{
			return this.ShiftCellsVertically(rowIndex + 1, this.Rows.MaxCount - 1, firstColumnIndex, lastColumnIndex, -1, CellShiftInitializeFormatType.UseDefaultFormat);
		}

		#endregion // DeleteCellsAndShiftUp

		// MD 7/25/08 - Excel formula solving
		#region GetCachedRegion

		// MD 3/13/12 - 12.1 - Table Support





		internal WorksheetRegion GetCachedRegion(WorksheetRegionAddress regionAddress)
		{
			return this.GetCachedRegion(
				regionAddress.FirstRowIndex, regionAddress.FirstColumnIndex,
				regionAddress.LastRowIndex, regionAddress.LastColumnIndex);
		}






		internal WorksheetRegion GetCachedRegion( int firstRow, int firstColumn, int lastRow, int lastColumn )
		{
			WorksheetRegion region = new WorksheetRegion( this, firstRow, firstColumn, lastRow, lastColumn, false );
			WorksheetRegion foundRegion;
			this.AddCachedRegion( region, out foundRegion );

			return foundRegion ?? region;
		}

		#endregion GetCachedRegion

		// MD 2/25/12 - 12.1 - Table Support
		#region GetCellFormatElementReadOnly

		internal WorksheetCellFormatData GetCellFormatElementReadOnly(WorksheetRow row, short columnIndex)
		{
			return this.GetCellFormatElementReadOnly(row, columnIndex, null);
		}

		internal WorksheetCellFormatData GetCellFormatElementReadOnly(WorksheetRow row, short columnIndex, CellFormatValue? valueBeingUsed)
		{
			bool resolveWithOwningColumn = true;
			bool resolveWithOwningRow = true;
			if (valueBeingUsed.HasValue)
			{
				switch (valueBeingUsed.Value)
				{
					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						resolveWithOwningColumn = false;
						break;

					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
						resolveWithOwningRow = false;
						break;
				}
			}

			if (row != null)
			{
				WorksheetCellFormatData cellFormat;
				if (row.TryGetCellFormat(columnIndex, out cellFormat))
					return cellFormat;

				if (resolveWithOwningRow && row.HasCellFormat)
					return row.CellFormatInternal.Element;
			}

			if (resolveWithOwningColumn == false)
			{
				if (this.Workbook != null)
					return this.Workbook.CellFormats.DefaultElement;
				else
					return new WorksheetCellFormatData(null, WorksheetCellFormatType.CellFormat);
			}

			WorksheetColumnBlock columnBlock = this.GetColumnBlock(columnIndex);
			return columnBlock.CellFormat;
		}

		#endregion // GetCellFormatElementReadOnly

		// MD 3/15/12 - TFS104581
		#region GetColumnBlock

		internal WorksheetColumnBlock GetColumnBlock(short columnIndex)
		{
			int index = Utilities.BinarySearch(this.columnBlocks.Keys, columnIndex, this.columnBlocks.Comparer);
			if (index < 0)
				index = (~index - 1);

			WorksheetColumnBlock block = this.columnBlocks.Values[index];
			Debug.Assert(
				block.FirstColumnIndex <= columnIndex && columnIndex <= block.LastColumnIndex,
				"The block does not cover the specified column index.");

			return block;
		}

		#endregion // GetColumnBlock

		// MD 3/15/12 - TFS104581
		#region GetColumnsInRange

		internal IEnumerable<WorksheetColumn> GetColumnsInRange(short startIndex, short endIndex, bool enumerateForwards)
		{
			short incremenetValue = enumerateForwards ? (short)1 : (short)-1;
			short startIterateIndex = enumerateForwards ? startIndex : endIndex;
			short endIterateIndex = enumerateForwards ? endIndex : startIndex;

			WorksheetColumnCollection columns = this.Columns;

			for (short columnIndex = startIterateIndex;
				enumerateForwards ? columnIndex <= endIterateIndex : columnIndex >= endIterateIndex;
				columnIndex += incremenetValue)
			{
				WorksheetColumnBlock block = this.GetColumnBlock(columnIndex);
				if (block.IsEmpty)
					continue;

				short blockEndIndex = enumerateForwards ? block.LastColumnIndex : block.FirstColumnIndex;
				Debug.Assert((enumerateForwards ? block.FirstColumnIndex : block.LastColumnIndex) == columnIndex, "Something is wrong here.");

				while (true)
				{
					yield return columns[columnIndex];

					if (columnIndex == blockEndIndex)
						break;

					columnIndex += incremenetValue;
				}
			}
		}

		#endregion // GetColumnsInRange

		#region GetColumnWidthInTwips

		// MD 8/1/08 - BR34692
		// Renamed this method for clarity, because with the old name, it wasn't clear what the widths units returned would be.
		// All references to this method have been updated also.
		//internal int GetColumnWidth( int columnIndex )


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int GetColumnWidthInTwips( int columnIndex )
		{
			// MD 3/5/10 - TFS26342
			// Moved code to new overload.
			return this.GetColumnWidthInTwips(columnIndex, false);
		}

		// MD 3/5/10 - TFS26342
		// Added new overload.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal int GetColumnWidthInTwips(int columnIndex, bool ignoreHidden)
		{
			// MD 9/10/08 - Cell Comments
			// Moved code to GetColumnWidthInPixels and only kept the pixel->twip converstion code because other places need the 
			// pixel width for the column.
			#region Moved

			//WorksheetColumn column = null;
			//
			//if ( this.columns != null )
			//    column = this.columns.GetIfCreated( columnIndex );
			//
			//// MD 1/11/08 - BR29105
			//// The default column width is not the actual width of default columns, use the resolved property instead.
			////int columnWidth = this.defaultColumnWidth;
			//int columnWidth = this.DefaultColumnWidthResolved;
			//
			//if ( column != null && column.Width >= 0 )
			//    columnWidth = column.Width;
			//
			//// MD 1/11/08 - BR29105
			//// The calculations here weren't quite correct. Now they more closely mimic the way Excel 
			//// calculations column widths
			////return (int)( columnWidth * CharacterWidthToTwipsRatioAt96DPI );
			//double charWidth256thsPerPixel = 256.0 / this.Workbook.ZeroCharacterWidth;
			//double columnWidthPixels = columnWidth / charWidth256thsPerPixel; 

			#endregion Moved
			// MD 3/5/10 - TFS26342
			// Use new overload that takes the ignoreHidden parameter.
			//double columnWidthPixels = this.GetColumnWidthInPixels( columnIndex );
			double columnWidthPixels = this.GetColumnWidthInPixels(columnIndex, ignoreHidden);

			// MD 2/10/12 - TFS97827
			// Consolidated a lot of the unit conversion code so we don't duplicate code.
			//double columnWidthTwips = columnWidthPixels * Worksheet.TwipsPerPixelAt96DPI;
			//
			//return Utilities.MidpointRoundingAwayFromZero(columnWidthTwips);
			return (int)Worksheet.ConvertPixelsToTwips(columnWidthPixels);
		}

		// MD 7/23/10 - TFS35969


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		// MD 3/15/12 - TFS104581
		//internal int GetColumnWidthInTwips(WorksheetColumn column, bool ignoreHidden)
		internal int GetColumnWidthInTwips(WorksheetColumnBlock column, bool ignoreHidden)
		{
			double columnWidthPixels = this.GetColumnWidthInPixels(column, ignoreHidden);

			// MD 2/10/12 - TFS97827
			// Consolidated a lot of the unit conversion code so we don't duplicate code.
			//double columnWidthTwips = columnWidthPixels * Worksheet.TwipsPerPixelAt96DPI;
			//
			//return Utilities.MidpointRoundingAwayFromZero(columnWidthTwips);
			return (int)Worksheet.ConvertPixelsToTwips(columnWidthPixels);
		}

		#endregion GetColumnWidthInTwips

		// MD 9/10/08 - Cell Comments
		#region GetColumnWidthInPixels



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal double GetColumnWidthInPixels( int columnIndex )
		{
			// MD 3/5/10 - TFS26342
			// Moved code to new overload.
			return this.GetColumnWidthInPixels(columnIndex, false);
		}

		// MD 3/5/10 - TFS26342
		// Added new overload.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal double GetColumnWidthInPixels(int columnIndex, bool ignoreHidden)
		{
			// MD 3/15/12 - TFS104581
			//WorksheetColumn column = null;
			WorksheetColumnBlock column = null;

			// MD 9/10/08 - Cell Comments
			// Allowed -1 to be specified to get the resolved default width of a column.
			//if ( this.columns != null )
			// MD 3/15/12 - TFS104581
			//if ( columnIndex >= 0 && this.columns != null )
			if (columnIndex >= 0)
			{
				// MD 3/15/12 - TFS104581
				//column = this.columns.GetIfCreated( columnIndex );
				column = this.GetColumnBlock((short)columnIndex);
			}

			// MD 7/23/10 - TFS35969
			// Moved all code to the new overload.
			return this.GetColumnWidthInPixels(column, ignoreHidden);
		}

		// MD 7/23/10 - TFS35969
		// Added a new overload so we can pass in the column directly if we have it.
		// MD 3/15/12 - TFS104581
		//internal double GetColumnWidthInPixels(WorksheetColumn column, bool ignoreHidden)
		internal double GetColumnWidthInPixels(WorksheetColumnBlock column, bool ignoreHidden)
		{
			// MD 3/5/10 - TFS26342
			// Honor the new ignoreHidden parameter and don't return 0 if it is True and the column is hidden.
			//if ( column != null && column.Hidden )
			if (ignoreHidden == false && column != null && column.Hidden)
				return 0;

			// MD 2/10/12 - TFS97827
			// Consolidated a lot of the unit conversion code so we don't duplicate code.
			#region Old Code

			//// MD 1/11/08 - BR29105
			//// The default column width is not the actual width of default columns, use the resolved property instead.
			////int columnWidth = this.defaultColumnWidth;
			//int columnWidth = this.DefaultColumnWidth;
			//
			//// MD 9/10/08 - Cell Comments
			//// Hidden columns have a resolved width of 0.
			//if ( column != null && column.Width >= 0 )
			//    columnWidth = column.Width;
			//
			//// MD 1/11/08 - BR29105
			//// The calculations here weren't quite correct. Now they more closely mimic the way Excel 
			//// calculations column widths
			////return (int)( columnWidth * CharacterWidthToTwipsRatioAt96DPI );
			//// MD 7/23/10 - TFS35969
			//// We now cache the value assigned to charWidth256thsPerPixel so we don't have to calculate it each time we 
			//// need a column width.
			////double charWidth256thsPerPixel = 256.0 / this.Workbook.ZeroCharacterWidth;
			////double columnWidthPixels = columnWidth / charWidth256thsPerPixel;
			//double columnWidthPixels = columnWidth / this.workbook.CharacterWidth256thsPerPixel;
			//
			//return columnWidthPixels;

			#endregion // Old Code
			if (column != null && column.Width >= 0)
			{
				// MD 3/15/12 - TFS104581
				//return column.GetWidth(WorksheetColumnWidthUnit.Pixel);
				return column.GetWidth(this, WorksheetColumnWidthUnit.Pixel);
			}

			return this.GetDefaultColumnWidth(WorksheetColumnWidthUnit.Pixel);
		}

		#endregion GetColumnWidthInPixels

		// MD 2/14/12 - 12.1 - Table Support
		#region GetColumnWidthPadding

		internal int GetColumnWidthPadding()
		{
			return Worksheet.GetColumnWidthPadding(this.ZeroCharacterWidth);
		}

		#endregion // GetColumnWidthPadding

		#region GetRowHeightInTwips

		// MD 8/1/08 - BR34692
		// Renamed this method for clarity, because with the old name, it wasn't clear what the widths units returned would be.
		// All references to this method have been updated also.
		//internal int GetRowHeight( int rowIndex )


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int GetRowHeightInTwips( int rowIndex )
		{
			// MD 3/5/10 - TFS26342
			// Moved code to new overload.
			return this.GetRowHeightInTwips(rowIndex, false);
		}

		// MD 3/5/10 - TFS26342
		// Added new overload.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal int GetRowHeightInTwips(int rowIndex, bool ignoreHidden)
		{
			WorksheetRow row = null;

			// MD 9/10/08 - Cell Comments
			// Allowed -1 to be specified to get the resolved default width of a column.
			//if ( this.rows != null )
			if ( rowIndex >= 0 && this.rows != null )
				row = this.rows.GetIfCreated( rowIndex );

			// MD 7/23/10 - TFS35969
			// Moved all code to new overload
			return this.GetRowHeightInTwips(row, ignoreHidden);
		}

		// MD 7/23/10 - TFS35969
		// Added a new overload so we can pass in the column directly if we have it.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal int GetRowHeightInTwips(WorksheetRow row, bool ignoreHidden)
		{
			// MD 1/18/11 - TFS62762
			// Refactored this to use the new WorksheetRow.GetResolvedHeight method. I also noticed a bug here
			// where we would return 0 if the rwo was null or had the default height and DefaultRowHidden was
			// true, even when ignoreHidden was False. When ignoreHidden is False, we should reutrn the DefaultRowHeight
			// in that case.
			//// MD 9/10/08 - Cell Comments
			//// Hidden rows have a resolved height of 0.
			//// MD 3/5/10 - TFS26342
			//// Honor the new ignoreHidden parameter and don't return 0 if it is True and the column is hidden.
			////if ( row != null && row.Hidden )
			//if (ignoreHidden == false && row != null && row.Hidden)
			//    return 0;
			//
			//if ( row == null || row.Height < 0 )
			//{
			//    // MD 9/10/08 - Cell Comments
			//    // Hidden rows have a resolved height of 0.
			//    if ( this.DefaultRowHidden )
			//        return 0;
			//
			//    // MD 1/22/08 - BR29105
			//    // Use the property instead of the member, which is now defaultable.
			//    // Getting the property will do the actualy calculations.
			//    //return this.defaultRowHeight;
			//    return this.DefaultRowHeight;
			//}
			//
			//return row.Height;
			if (row == null)
			{
				if (ignoreHidden == false && this.DefaultRowHidden)
					return 0;

				return this.DefaultRowHeight;
			}

			return row.GetResolvedHeight(ignoreHidden);
		}

		#endregion GetRowHeightInTwips

		// MD 9/10/08 - Cell Comments
		#region GetRowHeightInPixels



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal double GetRowHeightInPixels( int rowIndex )
		{
			// MD 3/5/10 - TFS26342
			// Moved code to new overload.
			return this.GetRowHeightInPixels(rowIndex, false);
		}

		// MD 3/5/10 - TFS26342
		// Added new overload.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal double GetRowHeightInPixels(int rowIndex, bool ignoreHidden)
		{
			// MD 3/5/10 - TFS26342
			// Use new overload that takes the ignoreHidden parameter.
			//int rowHeightInTwips = this.GetRowHeightInTwips( rowIndex );
			int rowHeightInTwips = this.GetRowHeightInTwips(rowIndex, ignoreHidden);

			// MD 2/10/12 - TFS97827
			// Consolidated a lot of the unit conversion code so we don't duplicate code.
			//double rowHeightInPixels = rowHeightInTwips / Worksheet.TwipsPerPixelAt96DPI;
			//return rowHeightInPixels;
			return Worksheet.ConvertTwipsToPixels(rowHeightInTwips);
		}

		#endregion GetRowHeightInPixels

		// MD 6/14/07 - BR23880
		#region GetShapeById






		internal WorksheetShape GetShapeById( uint shapeId )
		{
			return Worksheet.GetShapeById( this.shapes, shapeId );
		}

		internal static WorksheetShape GetShapeById( WorksheetShapeCollection shapes, uint shapeId )
		{
			if ( shapes == null )
				return null;

			foreach ( WorksheetShape shape in shapes )
			{
				if ( shape.ShapeId == shapeId )
					return shape;

				WorksheetShapeGroup group = shape as WorksheetShapeGroup;

				if ( group != null )
				{
					WorksheetShape subShape = group.GetShapeById( shapeId );

					if ( subShape != null )
						return subShape;
				}
			}

			return null;
		}

		#endregion GetShapeById

		// MD 6/14/07 - BR23880
		#region GetSolverRecords






		internal List<EscherRecordBase> GetSolverRecords()
		{
			List<EscherRecordBase> records = new List<EscherRecordBase>();

			Worksheet.GetSolverRecords( this.shapes, records );

			return records;
		}

		internal static void GetSolverRecords( WorksheetShapeCollection shapes, List<EscherRecordBase> records )
		{
			if ( shapes == null )
				return;

			foreach ( WorksheetShape shape in shapes )
			{
				if ( shape.CalloutRule != null )
					records.Add( shape.CalloutRule );

				WorksheetShapeGroup group = shape as WorksheetShapeGroup;

				if ( group != null )
					group.GetSolverRecords( records );
			}
		}

		#endregion GetSolverRecords

		#region InitSerializationCache

		internal void InitSerializationCache( WorkbookSerializationManager serializationManager, ref bool hasShapes )
		{
			// MD 8/22/08 - Excel formula solving - Performance
			// This should really be done each time we search through the cached regions collection, but it 
			// is way too slow, so do it before each save so the collection is at least cleaned occasionally.
			this.CleanCachedRegions();

			// Populate the serialization manager with the images from this worksheet's shapes collection
			serializationManager.InitializeImagesFromShapes( this.shapes, ref hasShapes );

			#region Init the serialization cache for all columns

			this.firstColumn = -1;
			this.maxColumnOutlineLevel = 0;

			// MD 3/15/12 - TFS104581
			//if ( this.HasColumns )
			{
				// MD 3/15/12 - TFS104581
				// We need to loop over the column blocks now because they hold the actual data for the columns.
				//WorksheetColumn previousColumn = null;
				//
				//foreach ( WorksheetColumn column in this.Columns )
				WorksheetColumnBlock previousColumn = null;
				foreach (WorksheetColumnBlock column in this.ColumnBlocks.Values)
				{
					// MD 3/15/12 - TFS104581
					//column.InitSerializationCache( serializationManager, previousColumn );
					//
					//if ( column.HasData )
					if (column.InitSerializationCache(this, serializationManager, previousColumn))
					{
						this.maxColumnOutlineLevel = Math.Max( column.OutlineLevel, this.maxColumnOutlineLevel );

						// MD 3/15/12 - TFS104581
						//if ( this.firstColumn < 0 )
						//    this.firstColumn = column.Index;
						//
						//this.firstColumnInUndefinedTail = column.Index + 1;
						if (this.firstColumn < 0)
							this.firstColumn = column.FirstColumnIndex;

						this.firstColumnInUndefinedTail = column.LastColumnIndex + 1;
					}

					previousColumn = column;
				}
			}

			#endregion Init the serialization cache for all columns

			// We need to keep a count of the hidden states of all rows with no data
			int noDataRowsHidden = 0;
			int noDataRowsVisible = 0;

			// Initialize the counts based on the rows which haven't been created yet
			if ( this.defaultRowHidden )
			{
				// MD 6/31/08 - Excel 2007 Format
				//noDataRowsHidden = Workbook.MaxExcelRowCount - ( this.HasRows ? this.Rows.Count : 0 );
				noDataRowsHidden = this.workbook.MaxRowCount - ( this.HasRows ? this.Rows.Count : 0 );
			}
			else
			{
				// MD 6/31/08 - Excel 2007 Format
				//noDataRowsVisible = Workbook.MaxExcelRowCount - ( this.HasRows ? this.Rows.Count : 0 );
				noDataRowsVisible = this.workbook.MaxRowCount - (this.HasRows ? this.Rows.Count : 0);
			}

			if ( this.HasRows )
			{
				// MD 2/15/11 - TFS66333
				// Use the EmptyElement for the default data element. The DefaultElement will be populated with data if 
				// the workbook was loaded from a file or stream. Also, renamed the value for clarity. Also, I noticed that the
				// default element had the Style value set to True, which is wrong when resolving cell format data, so we now 
				// clone the element and set the Style to false.
				//WorksheetCellFormatData defaultFormatData = this.Workbook.CellFormats.DefaultElement;
				// MD 1/8/12 - 12.1 - Cell Format Updates
				// We no longer need this.
				//WorksheetCellFormatData emptyFormatData = (WorksheetCellFormatData)this.workbook.CellFormats.EmptyElement.Clone();
				//emptyFormatData.Style = false;

				WorksheetRow previousRow = null;

				foreach ( WorksheetRow row in this.Rows )
				{
					// MD 7/20/2007 - BR25039
					// A new parameter was added to InitSerializationCache
					//row.InitSerializationCache( serializationManager, previousRow, defaultFormatData, this.defaultRowHidden );
					// MD 7/26/10 - TFS34398
					// Get the cache returned from the method.
					//row.InitSerializationCache( serializationManager, previousRow, defaultFormatData, this.defaultRowHidden, ref hasShapes );
					// MD 2/15/11 - TFS66333
					//WorksheetRowSerializationCache rowCache = row.InitSerializationCache(serializationManager, previousRow, defaultFormatData, this.defaultRowHidden, ref hasShapes);
					// MD 1/9/12 - 12.1 - Cell Format Updates
					//WorksheetRowSerializationCache rowCache = row.InitSerializationCache(serializationManager, previousRow, emptyFormatData, this.defaultRowHidden, ref hasShapes);
					WorksheetRowSerializationCache rowCache = row.InitSerializationCache(serializationManager, previousRow, this.defaultRowHidden, ref hasShapes);

					// If the row doesn't have data, increment the appropriate count of the rows' hidden states.
					// MD 7/26/10 - TFS34398
					//if ( row.HasData == false )
					if (rowCache.hasData == false)
					{
						if ( row.Hidden )
							noDataRowsHidden++;
						else
							noDataRowsVisible++;
					}

					previousRow = row;
				}
			}

			// MD 4/12/11 - TFS67084
			#region Store info for cell comments

			if (this.HasCellOwnedComments)
			{
				hasShapes = true;
				this.CommentShapes.AddRange(this.CellOwnedComments.Values);

				// MD 10/17/11 - TFS92047
				// Now that shapes don't store their text in the shared string table, we need to call InitSerializationCache on 
				// them so they can let their text initialize before saving.
				foreach (WorksheetShape shape in this.CommentShapes)
					shape.InitSerializationCache(serializationManager);
			}

			#endregion Store info for cell comments

			// Make the new default row hidden state based on whether more no data rows are hidden or visible.
			bool newDefaultRowHidden = noDataRowsVisible < noDataRowsHidden;
			bool defaultRowHiddenHasChanged = newDefaultRowHidden != this.defaultRowHidden;

			if ( this.HasRows || defaultRowHiddenHasChanged )
			{
				IEnumerator<WorksheetRow> rowEnumerator;

				// If the default has changed, we want to enumerate all rows, because now, the rows which 
				// haven't been created need to be saved.
				if ( defaultRowHiddenHasChanged )
					rowEnumerator = this.Rows.GetAllItemsEnumerator().GetEnumerator();
				else
					rowEnumerator = ( (IEnumerable<WorksheetRow>)this.Rows ).GetEnumerator();

				#region Init the serialization cache for all rows

				this.firstRow = -1;
				this.maxRowOutlineLevel = 0;

				while ( rowEnumerator.MoveNext() )
				{
					WorksheetRow row = rowEnumerator.Current;
					// We don't need to init the serialization cache for newly created rows, they have default data

					// MD 7/26/10 - TFS34398
					// Get the cache associated with the row.
					WorksheetRowSerializationCache rowCache;
					bool wasCacheCreated = false;
					if (serializationManager.RowSerializationCaches.TryGetValue(row, out rowCache) == false)
					{
						rowCache = new WorksheetRowSerializationCache();
						wasCacheCreated = true;
					}

					// Change the HasData flag for all created rows which now have a different hidden value than the default.
					// MD 7/26/10 - TFS34398
					//if ( row.HasData == false && row.Hidden != newDefaultRowHidden )
					//    row.HasData = true;
					if (rowCache.hasData == false && row.Hidden != newDefaultRowHidden)
					{
						rowCache.hasData = true;

						if (wasCacheCreated)
							serializationManager.RowSerializationCaches[row] = rowCache;
					}

					// If the row has data, update the dimensions
					// MD 7/26/10 - TFS34398
					//if ( row.HasData )
					if (rowCache.hasData)
					{
						this.maxRowOutlineLevel = Math.Max( row.OutlineLevel, this.maxRowOutlineLevel );

						if ( this.firstRow < 0 )
							this.firstRow = row.Index;

						this.firstRowInUndefinedTail = row.Index + 1;

						// MD 7/26/10 - TFS34398
						// Use the cached values on the row cache.
						//if ( this.firstColumn < 0 || row.FirstCell < this.firstColumn )
						//    this.firstColumn = row.FirstCell;
						//
						//if ( this.firstColumnInUndefinedTail < 0 || this.firstColumnInUndefinedTail < row.FirstCellInUndefinedTail )
						//    this.firstColumnInUndefinedTail = row.FirstCellInUndefinedTail;
						if (this.firstColumn < 0 || rowCache.firstCell < this.firstColumn)
							this.firstColumn = rowCache.firstCell;

						if (this.firstColumnInUndefinedTail < 0 || this.firstColumnInUndefinedTail < rowCache.firstCellInUndefinedTail)
							this.firstColumnInUndefinedTail = rowCache.firstCellInUndefinedTail;
					}
				}

				// MD 7/26/10
				// Found while fixing TFS34398
				// We should dispose the enumerator.
				rowEnumerator.Dispose();

				#endregion Init the serialization cache for all rows
			}

			this.defaultRowHidden = newDefaultRowHidden;

			if ( this.firstRow < 0 )
			{
				this.firstRow = 0;
				this.firstRowInUndefinedTail = 0;
			}

			if ( this.firstColumn < 0 )
			{
				this.firstColumn = 0;
				this.firstColumnInUndefinedTail = 0;
			}

			// MD 8/23/11 - TFS84306
			// This is needed now because strings on shapes are no longer placed in the shared string table.
			//// MD 11/3/10 - TFS49093
			//// This is no longer needed because the shared formatted strings will be iterated over and their fonts will be placed 
			//// in the manager when saving.
			////// MD 9/2/08 - Cell Comments
			foreach (WorksheetShape shape in this.Shapes)
				shape.InitSerializationCache(serializationManager);

			// MD 2/22/12 - 12.1 - Table Support
			foreach (WorksheetTable table in this.Tables)
				table.InitializeSerializationManager(serializationManager);
		}

		#endregion InitSerializationCache

		// MD 2/22/12 - 12.1 - Table Support
		#region InsertCellsAndShiftDown

		internal CellShiftResult InsertCellsAndShiftDown(int rowIndex, short firstColumnIndex, short lastColumnIndex, CellShiftInitializeFormatType initializeFormatType)
		{
			return this.ShiftCellsVertically(rowIndex, this.Rows.MaxCount - 2, firstColumnIndex, lastColumnIndex, 1, initializeFormatType);
		}

		#endregion // InsertCellsAndShiftDown

		// MD 9/11/08 - Cell Comments
		#region IsColumnHidden

		// MD 3/15/12 - TFS104581
		// Changed the type to a short to be consistent with the other column indexes.
		//internal bool IsColumnHidden( int columnIndex )
		internal bool IsColumnHidden(short columnIndex)
		{
			// MD 3/15/12 - TFS104581
			#region Old Code

			//WorksheetColumn column = this.Columns.GetIfCreated( columnIndex );

			//if ( column == null )
			//{
			//    // MD 9/27/08
			//    // The wrong default was being returned here.
			//    //return true;
			//    return false;
			//}

			//return column.Hidden;

			#endregion // Old Code
			return this.GetColumnBlock(columnIndex).Hidden;
		} 

		#endregion IsColumnHidden

		// MD 2/23/12 - 12.1 - Table Support
		#region IsDataPresentInRow

		internal bool IsDataPresentInRow(int rowIndex, short firstColumnIndex, short lastColumnIndex)
		{
			WorksheetRow row = this.Rows.GetIfCreated(rowIndex);
			if (row == null)
				return false;

			foreach (CellDataContext cellContext in row.GetCellsWithData(firstColumnIndex, lastColumnIndex))
			{
				if (cellContext.HasValue)
					return true;
			}

			return false;
		}

		#endregion // IsDataPresentInRow

		// MD 9/11/08 - Cell Comments
		#region IsRowHidden

		internal bool IsRowHidden( int rowIndex )
		{
			WorksheetRow row = this.Rows.GetIfCreated( rowIndex );

			// MD 2/25/12 - 12.1 - Table Support
			// Moved the code to a new overload.
			//if ( row == null )
			//    return this.DefaultRowHidden;
			//
			//return row.Hidden;
			return this.IsRowHidden(row);
		}

		// MD 2/25/12 - 12.1 - Table Support
		internal bool IsRowHidden(WorksheetRow row)
		{
			if (row == null)
				return this.DefaultRowHidden;

			return row.Hidden;
		} 

		#endregion IsRowHidden

		// MD 3/5/10 - TFS26342
		#region OnAfterWorksheetElementResized



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void OnAfterWorksheetElementResized(RowColumnBase worksheetElement, int oldElementExtentInTwipsHiddenIgnored, bool wasHidden)
		{
			if (this.IsLoading)
				return;

			// MD 7/23/10 - TFS35969
			WorksheetRow row = worksheetElement as WorksheetRow;
			WorksheetColumn column = worksheetElement as WorksheetColumn;

			for (int i = 0; i < this.Shapes.Count; i++)
			{
				// MD 7/23/10 - TFS35969
				// Only call OnAfterWorksheetElementResize on the shape if the row or column is before or inside the shape.
				// We should ignore resized rows or columns if they are after the shape, because they can't affect the shape.
				//this.Shapes[i].OnAfterWorksheetElementResize(worksheetElement, oldElementExtentInTwipsHiddenIgnored, wasHidden);
				WorksheetShape shape = this.Shapes[i];

				if (row != null)
				{
					if (shape.BottomRightCornerCell.RowIndex < row.Index)
						continue;
				}
				else
				{
					if (shape.BottomRightCornerCell.ColumnIndex < column.Index)
						continue;
				}

				shape.OnAfterWorksheetElementResize(worksheetElement, oldElementExtentInTwipsHiddenIgnored, wasHidden);
			}

			foreach (WorksheetCellComment comment in this.CellOwnedComments.Values)
			{
				// MD 7/23/10 - TFS35969
				// Only call OnAfterWorksheetElementResize on the shape if the row or column is before or inside the shape.
				// We should ignore resized rows or columns if they are after the shape, because they can't affect the shape.
				if (row != null)
				{
					if (comment.BottomRightCornerCell.RowIndex < row.Index)
						continue;
				}
				else
				{
					if (comment.BottomRightCornerCell.ColumnIndex < column.Index)
						continue;
				}

				comment.OnAfterWorksheetElementResize(worksheetElement, oldElementExtentInTwipsHiddenIgnored, wasHidden);
			}
		}

		#endregion // OnAfterWorksheetElementResized

		// MD 3/5/10 - TFS26342
		#region OnBeforeWorksheetElementResize






		// MD 7/23/10 - TFS35969
		// Now the row or column is passed to this method so we can ignore shapes before the row or column.
		//internal void OnBeforeWorksheetElementResize()
		internal void OnBeforeWorksheetElementResize(RowColumnBase worksheetElement)
		{
			if (this.IsLoading)
				return;

			// MD 7/23/10 - TFS35969
			WorksheetRow row = worksheetElement as WorksheetRow;
			WorksheetColumn column = worksheetElement as WorksheetColumn;

			for (int i = 0; i < this.Shapes.Count; i++)
			{
				// MD 7/23/10 - TFS35969
				// Only call OnBeforeWorksheetElementResize on the shape if the row or column is before or inside the shape.
				// We should ignore resized rows or columns if they are after the shape, because they can't affect the shape.
				//this.Shapes[i].OnBeforeWorksheetElementResize();
				WorksheetShape shape = this.Shapes[i];

				if (row != null)
				{
					if (shape.BottomRightCornerCell.RowIndex < row.Index)
						continue;
				}
				else
				{
					if (shape.BottomRightCornerCell.ColumnIndex < column.Index)
						continue;
				}

				shape.OnBeforeWorksheetElementResize();
			}

			foreach (WorksheetCellComment comment in this.CellOwnedComments.Values)
			{
				// MD 7/23/10 - TFS35969
				// Only call OnBeforeWorksheetElementResize on the shape if the row or column is before or inside the shape.
				// We should ignore resized rows or columns if they are after the shape, because they can't affect the shape.
				if (row != null)
				{
					if (comment.BottomRightCornerCell.RowIndex < row.Index)
						continue;
				}
				else
				{
					if (comment.BottomRightCornerCell.ColumnIndex < column.Index)
						continue;
				}

				comment.OnBeforeWorksheetElementResize();
			}
		}

		#endregion // OnBeforeWorksheetElementResize

		// MD 3/15/12 - TFS104581
		#region OnColumnBlockLoaded

		internal void OnColumnBlockLoaded(short firstColumnIndex, short lastColumnIndex,
			int width, bool hidden, byte outlineLevel, WorksheetCellFormatData cellFormat)
		{
			WorksheetColumnBlock block = this.SplitColumnBlock(firstColumnIndex, lastColumnIndex);

			if (cellFormat != null && cellFormat.IsEmpty == false)
			{
				GenericCachedCollection<WorksheetCellFormatData> collection = GenericCacheElementEx.ReleaseEx(block.CellFormat);
				block.CellFormat = GenericCacheElementEx.FindExistingOrAddToCacheEx(cellFormat, collection);
			}

			block.Hidden = hidden;
			block.OutlineLevel = outlineLevel;
			block.Width = width;
		}

		#endregion // OnColumnBlockLoaded

		// MD 9/12/08 - TFS6887
		#region OnCurrentFormatChanged

		internal void OnCurrentFormatChanged()
		{
			// MD 3/15/12 - TFS104581
			// We may need to increase or decrease the number of column blocks being used when the format changes
			// Do this before calling OnCurrentFormatChanged on the columns collection so it can iterate only the valid
			// column blocks in the new format.
			WorksheetColumnBlock lastBlock = this.columnBlocks.Values[this.columnBlocks.Count - 1];
			short lastColumnIndex = (short)(this.Workbook.MaxColumnCount - 1);

			// If the last block ends after new last column, keep removing blocks until all blocks are in range again. 
			do
			{
				if (lastBlock.LastColumnIndex <= lastColumnIndex)
					break;

				// If this is the last remaining block, just shorten it instead of removing it.
				if (this.columnBlocks.Count == 1)
				{
					lastBlock.LastColumnIndex = lastColumnIndex;
					break;
				}

				GenericCacheElementEx.ReleaseEx(lastBlock.CellFormat);

				this.columnBlocks.RemoveAt(this.columnBlocks.Count - 1);
				lastBlock = this.columnBlocks.Values[this.columnBlocks.Count - 1];
			}
			while (true);

			// If the last block ends before the new last column, we need to expand range of column blocks.
			if (lastBlock.LastColumnIndex < lastColumnIndex)
			{
				// If the last block has no data, just extend it to the end of the sheet. Otherwise, add a new default block
				// to the end of the sheet.
				if (lastBlock.IsEmpty)
				{
					lastBlock.LastColumnIndex = lastColumnIndex;
				}
				else
				{
					WorksheetColumnBlock defaultColumnBlock = new WorksheetColumnBlock(
						(short)(lastBlock.LastColumnIndex + 1), lastColumnIndex,
						this.workbook.CellFormats.DefaultElement);
					defaultColumnBlock.CellFormat.IncrementReferenceCount();
					this.columnBlocks.Add(defaultColumnBlock.FirstColumnIndex, defaultColumnBlock);
				}
			}

			// MD 2/1/11 - Data Validation support
			//this.dataValidationInfo2003 = null;
			//this.dataValidationInfo2007 = null;

			// MD 7/2/09 - TFS18634
			// Added OnCurrentFormatChanged methods to the row, column, and cell collections for performance enhancements.
			this.Rows.OnCurrentFormatChanged();
			this.Columns.OnCurrentFormatChanged();
		}

		#endregion OnCurrentFormatChanged

		#region OnRemovedFromCollection

		internal void OnRemovedFromCollection()
		{
			// MD 4/12/11 - TFS67084
			// Cache the workbook before clearing out the parent collection.
			Workbook oldWorkbook = this.workbook;

			// MD 2/2/12 - TFS100573
			SharedStringTable sharedStringTable = oldWorkbook.SharedStringTable;

			// MD 3/15/12 - TFS104581
			// Moved this below. The workbook reference may be needed while we iterate the rows and columns collections 
			// (the determine the maximum row and column counts).
			//// If the worksheet was removed from its workbook, make its parent collection null
			//// MD 4/12/11
			//// Found while fixing TFS67084
			//// We use the Workbook more often than the parent collection, so we will store that instead.
			////this.parentCollection = null;
			//this.workbook = null;

			// MD 11/3/10 - TFS49093
			// Now that we are using shared strings, we need to detach strings from cells which are removed.
			foreach (WorksheetRow row in this.Rows)
			{
				// MD 6/16/12 - CalcEngineRefactor
				foreach (Formula formula in row.CellOwnedFormulas)
					formula.DisconnectReferences();

				// MD 4/18/11 - TFS62026
				// We should unroot all cell formats on rows when the worksheet is removed.
				if (row.HasCellFormat)
					row.CellFormatInternal.OnUnrooted();

				// MD 4/18/11 - TFS62026
				// We should unroot all cell formats on cells when the worksheet is removed.
				// MD 12/21/11 - 12.1 - Table Support
				//row.UnrootAllCellFormatsForCells(oldWorkbook.CellFormats);
				row.UnrootAllCellFormatsForCells();

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//foreach (WorksheetCell cell in row.Cells)
				//{
				//    FormattedStringProxy proxy = Utilities.GetFormattedStringProxy(cell.ValueInternal);
				//
				//    if (proxy != null)
				//        proxy.SetWorkbook(null);
				//}
				foreach (KeyValuePair<short, WorksheetCellBlock> pair in row.GetCellBlocksWithValues())
				{
					WorksheetCellBlock cellBlock = pair.Value;
					short columnIndex = pair.Key;
					object valueRaw = cellBlock.GetCellValueRaw(row, columnIndex);

					StringElement element = Utilities.GetFormattedStringElement(valueRaw);

					if (element != null)
					{
						IFormattedString formattedString = valueRaw as IFormattedString;
						if (formattedString != null)
						{
							formattedString.SetWorkbook(null);
						}
						else
						{
							// MD 12/12/11 - 12.1 - Table Support
							// This was not cloning the string when it was shared by other cells which weren't removed.
							
							//GenericCacheElement.Release(element, oldWorkbook.SharedStringTable);
							//element = GenericCacheElement.FindExistingOrAddToCache(element, null);
							// MD 2/2/12 - TFS100573
							//GenericCacheElementProxy<StringElement>.SetCollection(null, ref element);
							GenericCachedCollection<StringElement> temp = sharedStringTable;
							GenericCacheElement.SetCollection(null, ref temp, ref element);
							Debug.Assert(temp == null, "Something is wrong here.");

							// MD 5/31/11 - TFS75574
							// We need more information than just the key from the element, so just pass the element directly.
							//cellBlock.SetFormattedStringKey(columnIndex, element.Key);
							cellBlock.UpdateFormattedStringElement(columnIndex, element);
						}
					}
				}
			}

			// MD 3/15/12 - TFS104581
			#region Old Code

			//// MD 4/18/11 - TFS62026
			//// We should unroot all cell formats on columns when the worksheet is removed.
			//foreach (WorksheetColumn column in this.Columns)
			//{
			//    if (column.HasCellFormat)
			//        column.CellFormatInternal.OnUnrooted();
			//}

			#endregion // Old Code
			foreach (WorksheetColumnBlock block in this.columnBlocks.Values)
				GenericCacheElementEx.ReleaseEx(block.CellFormat);

			// MD 4/18/11 - TFS62026
			// We should unroot all cell formats on merged cells.
			foreach (WorksheetMergedCellsRegion region in this.MergedCellsRegions)
			{
				if (region.HasCellFormat)
					region.CellFormatInternal.OnUnrooted();
			}

			Worksheet.UnrootStringsInShapes(this);

			// MD 2/16/12 - 12.1 - Table Support
			foreach (WorksheetTable table in this.Tables)
				table.OnRemovedFromWorkbook(oldWorkbook);

			// MD 3/15/12 - TFS104581
			// Moved from above. The workbook reference may be needed while we iterate the rows and columns collections 
			// (the determine the maximum row and column counts).
			this.workbook = null;
		}

		#endregion OnRemovedFromCollection

		// MD 2/25/12 - 12.1 - Table Support
		#region ReapplyFilters

		internal void ReapplyFilters(int firstRowIndex, int lastRowIndex, List<IFilterable> filterableItems)
		{
			// Filters shouldn't be reapplied when loading.
			if (this.Workbook != null && this.Workbook.IsLoading)
				return;

			// If there are no filers, unhide all the rows.
			if (filterableItems.Count == 0)
			{
				if (this.DefaultRowHidden)
				{
					for (int i = firstRowIndex; i <= lastRowIndex; i++)
						this.Rows[i].Hidden = false;
				}
				else
				{
					foreach (WorksheetRow row in this.Rows.GetItemsInRange(firstRowIndex, lastRowIndex))
						row.Hidden = false;
				}

				return;
			}

			for (int filterableItemIndex = 0; filterableItemIndex < filterableItems.Count; filterableItemIndex++)
			{
				IFilterable filterableItem = filterableItems[filterableItemIndex];
				if (filterableItem.Filter.OnBeforeFilterColumn(this, firstRowIndex, lastRowIndex, filterableItem.ColumnIndex) == false)
					return;
			}

			for (int rowIndex = firstRowIndex; rowIndex <= lastRowIndex; rowIndex++)
			{
				WorksheetRow row = this.Rows.GetIfCreated(rowIndex);

				bool shouldRowBeHidden = false;
				for (int filterableItemIndex = 0; filterableItemIndex < filterableItems.Count; filterableItemIndex++)
				{
					IFilterable filterableItem = filterableItems[filterableItemIndex];
					if (filterableItem.Filter.MeetsCriteria(this, row, filterableItem.ColumnIndex) == false)
					{
						shouldRowBeHidden = true;
						break;
					}
				}

				if (shouldRowBeHidden != this.IsRowHidden(row))
				{
					if (row == null)
						row = this.Rows[rowIndex];

					row.Hidden = shouldRowBeHidden;
				}
			}
		}

		#endregion // ReapplyFilters

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 1/14/08 - BR29635
		//#region RemoveUsedColorIndicies

		//internal void RemoveUsedColorIndicies( List<int> unusedIndicies )
		//{
		//    if ( this.displayOptions != null )
		//        this.displayOptions.RemoveUsedColorIndicies( unusedIndicies );
		//}

		//#endregion RemoveUsedColorIndicies

		#endregion // Removed

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region RotateCellsVertically



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal CellShiftResult RotateCellsVertically(int firstRowIndex, int lastRowIndex, short firstColumnIndex, short lastColumnIndex, int shiftAmount)
		{
			CellShiftOperation shiftOperation = new CellShiftOperation(this, CellShiftType.VerticalRotate,
				firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex,
				shiftAmount);

			return this.ShiftCells(shiftOperation, CellShiftInitializeFormatType.UseDefaultFormat);
		}

		#endregion // RotateCellsVertically

		// MD 2/23/12 - 12.1 - Table Support
		#region ShiftCells

		private CellShiftResult ShiftCells(CellShiftOperation shiftOperation, CellShiftInitializeFormatType initializeFormatType)
		{
			Debug.Assert(shiftOperation.IsVertical, "The shift direction should be vertical.");
			Debug.Assert(
				shiftOperation.RegionAddressBeforeShift.FirstColumnIndex != 0 ||
				shiftOperation.RegionAddressBeforeShift.LastColumnIndex != this.Columns.MaxCount - 1,
				"Do a row insert/remove/move here when it is supported.");

			List<ArrayFormula> arrayFormulasToShift = new List<ArrayFormula>();
			List<WorksheetTable> tablesToShift = new List<WorksheetTable>();
			List<WorksheetMergedCellsRegion> mergedRegionsToShift = new List<WorksheetMergedCellsRegion>();

			CellShiftResult result =
				this.VerifyCellShift(shiftOperation, arrayFormulasToShift, tablesToShift, mergedRegionsToShift);

			if (result != CellShiftResult.Success)
				return result;

			WorksheetRegionAddress? deletedRegionAddress = shiftOperation.DeletedRegionAddress;

			// Clear the data from the cells which will be overwritten or shifted off the worksheet.
			if (deletedRegionAddress != null)
				this.ClearCells(deletedRegionAddress.Value, null);

			// Notify all data tables that cells are about to shift so they can be temporarily removed from the shifting region.
			if (this.HasDataTables)
			{
				foreach (WorksheetDataTable dataTable in this.DataTables)
					dataTable.OnCellsShifting(shiftOperation);
			}

			// Notify all array formulas that cells are about to shift so they can be temporarily removed from the shifting region.
			foreach (ArrayFormula arrayFormula in arrayFormulasToShift)
				arrayFormula.OnBeforeShift(shiftOperation);

			// --------------------------------------------------------------
			// Actually shift the cells
			shiftOperation.ShiftWorksheetCellData(this);
			// --------------------------------------------------------------

			// Clear the cells which were inserted and initialize them with the correct cell format.
			WorksheetRegionAddress? insertedRegionAddress = shiftOperation.InsertedRegionAddress;
			if (insertedRegionAddress != null)
			{
				WorksheetRegionAddress insertedRegionAddressResolved = insertedRegionAddress.Value;
				WorksheetRegionAddress regionAddressAfterShift = shiftOperation.RegionAddressAfterShift;

				int? indexOfRowForInitializingCellFormat;
				switch (initializeFormatType)
				{
					case CellShiftInitializeFormatType.UseDefaultFormat:
						indexOfRowForInitializingCellFormat = null;
						break;

					case CellShiftInitializeFormatType.FromShiftedCellsAdjacentToInsertRegion:
						if (insertedRegionAddressResolved.FirstRowIndex < regionAddressAfterShift.FirstRowIndex)
							indexOfRowForInitializingCellFormat = regionAddressAfterShift.FirstRowIndex;
						else
							indexOfRowForInitializingCellFormat = regionAddressAfterShift.LastRowIndex;
						break;

					case CellShiftInitializeFormatType.FromStationaryCellsAdjacentToInsertRegion:
						if (insertedRegionAddressResolved.FirstRowIndex < regionAddressAfterShift.FirstRowIndex)
						{
							if (0 < insertedRegionAddressResolved.FirstRowIndex)
								indexOfRowForInitializingCellFormat = insertedRegionAddressResolved.FirstRowIndex - 1;
							else
								indexOfRowForInitializingCellFormat = null;
						}
						else
						{
							if (insertedRegionAddressResolved.LastRowIndex < this.Rows.MaxCount - 1)
								indexOfRowForInitializingCellFormat = insertedRegionAddressResolved.LastRowIndex + 1;
							else
								indexOfRowForInitializingCellFormat = null;
						}
						break;

					default:
						Utilities.DebugFail("Unknown CellShiftInitializeFormatType: " + initializeFormatType);
						goto case CellShiftInitializeFormatType.UseDefaultFormat;
				}

				this.ClearCells(insertedRegionAddressResolved, indexOfRowForInitializingCellFormat);
			}

			foreach (WorksheetTable table in tablesToShift)
			{
				ShiftAddressResult tableShiftResult = table.ShiftTable(shiftOperation);
				Debug.Assert(tableShiftResult.DidShift, "This table should have been shifted.");
			}

			for (int i = 0; i < mergedRegionsToShift.Count; i++)
			{
				WorksheetMergedCellsRegion mergedRegion = mergedRegionsToShift[i];
				ShiftAddressResult shiftResult = mergedRegion.ShiftRegion(shiftOperation, false);
				Debug.Assert(shiftResult.IsDeleted == false, "The merged regions cannot shift off the worksheet");
			}

			this.IterateCachedRegions(new ShiftRegionHelper(shiftOperation).RegionCallback);

			if (shiftOperation.IsVertical)
			{
				this.PrintOptions.HorizontalPageBreaks.ShiftPageBreaksVertically(shiftOperation);
				if (this.Workbook != null)
				{
					foreach (CustomView customView in this.Workbook.CustomViews)
					{
						PrintOptions printOptions = customView.GetPrintOptions(this);
						if (printOptions != null)
							printOptions.HorizontalPageBreaks.ShiftPageBreaksVertically(shiftOperation);
					}
				}
			}
			else
			{
				
			}

			for (int i = this.Shapes.Count - 1; i >= 0; i--)
			{
				ShiftAddressResult shapeShiftResult = this.Shapes[i].ShiftShape(shiftOperation);
				if (shapeShiftResult.IsDeleted)
					this.Shapes.RemoveAt(i);
			}

			foreach (WorksheetCellComment comment in this.CellOwnedComments.Values)
			{
				ShiftAddressResult commentShiftResult = comment.ShiftShape(shiftOperation);
				Debug.Assert(commentShiftResult.IsDeleted == false, "The comments cannot be deleted.");
			}

			if (this.HasDataValidationRules)
			{
				List<DataValidationRule> rulesToDelete = new List<DataValidationRule>();
				foreach (KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair in this.DataValidationRules)
				{
					bool isDeleted;
					pair.Value.OnCellsShifted(shiftOperation, out isDeleted);

					if (isDeleted)
						rulesToDelete.Add(pair.Key);
				}

				for (int i = 0; i < rulesToDelete.Count; i++)
					this.DataValidationRules.Remove(rulesToDelete[i]);
			}

			// Notify all array formulas that cells have shifted so they can be re-added to the worksheet.
			foreach (ArrayFormula arrayFormula in arrayFormulasToShift)
				arrayFormula.OnAfterShift();

			// Notify the workbook that cells have shifted so all formulas in the workbook can be redirected to the new 
			// cell positions.
			if (this.Workbook != null)
				this.Workbook.OnCellsShifted(shiftOperation, ReferenceShiftType.MaintainReference);

			// Notify all data tables that cells have shifted so they can be re-added to the worksheet.
			if (this.HasDataTables)
			{
				foreach (WorksheetDataTable dataTable in this.DataTables)
					dataTable.OnCellsShifted(shiftOperation);
			}

			this.CellShiftHistory.Add(shiftOperation);
			return CellShiftResult.Success;
		}

		#endregion // ShiftCells

		// MD 2/23/12 - 12.1 - Table Support
		#region ShiftCellsVertically

		internal CellShiftResult ShiftCellsVertically(int firstRowIndex, int lastRowIndex, short firstColumnIndex, short lastColumnIndex, int shiftAmount, CellShiftInitializeFormatType initializeFormatType)
		{
			CellShiftOperation shiftOperation = new CellShiftOperation(this, CellShiftType.VerticalShift,
				firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex,
				shiftAmount);

			return this.ShiftCells(shiftOperation, initializeFormatType);
		}

		#endregion // ShiftCellsVertically

		// MD 3/15/12 - TFS104581
		#region SplitColumnBlock

		internal WorksheetColumnBlock SplitColumnBlock(short firstColumnIndex, short lastColumnIndex)
		{
			WorksheetColumnBlock blockForCurrentRange = this.GetColumnBlock(firstColumnIndex);
			short lastColumnIndexNeededForAllBlocks = blockForCurrentRange.LastColumnIndex;

			// If the current block doesn't start at the current column, move it to be just before the current column and create a 
			// new block starting at the current column.
			if (blockForCurrentRange.FirstColumnIndex != firstColumnIndex)
			{
				blockForCurrentRange.LastColumnIndex = (short)(firstColumnIndex - 1);

				WorksheetColumnBlock newBlockForCurrentColumn = new WorksheetColumnBlock(
					firstColumnIndex, lastColumnIndexNeededForAllBlocks,
					blockForCurrentRange);
				this.columnBlocks.Add(newBlockForCurrentColumn.FirstColumnIndex, newBlockForCurrentColumn);

				blockForCurrentRange = newBlockForCurrentColumn;
			}

			// If the current block doesn't end at the current column, shorten it to be a single column and create a 
			// new block starting just after the current column.
			if (blockForCurrentRange.LastColumnIndex != lastColumnIndex)
			{
				blockForCurrentRange.LastColumnIndex = lastColumnIndex;

				WorksheetColumnBlock blockAfterColumn = new WorksheetColumnBlock(
					(short)(lastColumnIndex + 1), lastColumnIndexNeededForAllBlocks,
					blockForCurrentRange);
				this.columnBlocks.Add(blockAfterColumn.FirstColumnIndex, blockAfterColumn);
			}

			return blockForCurrentRange;
		}

		#endregion // SplitColumnBlock

		#region TryGetDateTimeFromCell

		internal bool TryGetDateTimeFromCell(WorksheetRow row, short columnIndex, out DateTime value)
		{
			value = DateTime.MinValue;

			Workbook workbook = this.Workbook;

			double numericValue;
			if (row == null ||
				Utilities.TryGetNumericValue(workbook, row.GetCellValueInternal(columnIndex), out numericValue) == false)
				return false;

			DateTime? dateTimeTestValue = ExcelCalcValue.ExcelDateToDateTime(workbook, numericValue);
			if (dateTimeTestValue.HasValue == false)
				return false;

			WorksheetCellFormatData cellFormat = this.GetCellFormatElementReadOnly(row, columnIndex);

			ValueFormatter formatter;
			if (workbook == null)
			{
				// MD 4/9/12 - TFS101506
				//formatter = new ValueFormatter(null, cellFormat.FormatStringResolved);
				formatter = new ValueFormatter(null, cellFormat.FormatStringResolved, this.Culture);
			}
			else
			{
				formatter = workbook.Formats.GetValueFormatter(cellFormat.FormatStringIndexResolved);
			}

			if (formatter.IsValid == false ||
				formatter.GetSectionType(numericValue) != ValueFormatter.SectionType.Date)
				return false;

			value = dateTimeTestValue.Value;
			return true;
		}

		#endregion // TryGetDateTimeFromCell

		// MD 3/15/12 - TFS104581
		#region TryMergeColumnBlock

		internal WorksheetColumnBlock TryMergeColumnBlock(short columnIndex)
		{
			WorksheetColumnBlock block = this.GetColumnBlock(columnIndex);

			if (block.FirstColumnIndex != 0)
			{
				WorksheetColumnBlock previousBlock = this.GetColumnBlock((short)(block.FirstColumnIndex - 1));
				if (block.Equals(previousBlock))
				{
					this.ColumnBlocks.Remove(block.FirstColumnIndex);
					GenericCacheElementEx.ReleaseEx(block.CellFormat);

					previousBlock.LastColumnIndex = block.LastColumnIndex;
					block = previousBlock;
				}
			}

			int maxColumnIndex = this.Columns.MaxCount - 1;
			if (block.LastColumnIndex != maxColumnIndex)
			{
				WorksheetColumnBlock nextBlock = this.GetColumnBlock((short)(block.LastColumnIndex + 1));
				if (block.Equals(nextBlock))
				{
					this.ColumnBlocks.Remove(nextBlock.FirstColumnIndex);
					GenericCacheElementEx.ReleaseEx(nextBlock.CellFormat);

					block.LastColumnIndex = nextBlock.LastColumnIndex;
				}
			}

			return block;
		}

		#endregion // TryMergeColumnBlock

		// MD 3/5/12 - 12.1 - Table Support
		#region VerifyCellAddress

		internal bool VerifyCellAddress(ref WorksheetRow row, ref short columnIndex, ref int cellShiftHistoryVersion)
		{
			if (this.CellShiftHistoryVersion == cellShiftHistoryVersion)
				return true;

			int currentVersion = this.CellShiftHistoryVersion;

			int shiftedRowIndex = row.Index;

			bool isCellShifted = false;
			for (int i = cellShiftHistoryVersion; i < currentVersion; i++)
			{
				CellShiftOperation shiftOperation = this.CellShiftHistory[i];

				Debug.Assert(shiftOperation.IsVertical, "Write code to handle horizontal cell shifting.");

				ShiftAddressResult result = shiftOperation.ShiftCellAddress(ref shiftedRowIndex, columnIndex);
				isCellShifted |= result.DidShift;

				if (result.IsDeleted)
					return false;
			}

			if (isCellShifted && shiftedRowIndex != row.Index)
				row = this.Rows[shiftedRowIndex];

			cellShiftHistoryVersion = currentVersion;
			return true;
		}

		#endregion // VerifyCellAddress

		// MD 2/29/12 - 12.1 - Table Support
		#region VerifyCellShift

		internal CellShiftResult VerifyCellShift(
			int firstRowIndex, int lastRowIndex, 
			short firstColumnIndex, short lastColumnIndex, 
			int shiftAmount, CellShiftType shiftType)
		{
			CellShiftOperation shiftOperation = new CellShiftOperation(this, shiftType,
				firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex,
				shiftAmount);

			return this.VerifyCellShift(shiftOperation, null, null, null);
		}

		private CellShiftResult VerifyCellShift(CellShiftOperation shiftOperation,
			List<ArrayFormula> arrayFormulasToShift,
			List<WorksheetTable> tablesToShift, 
			List<WorksheetMergedCellsRegion> mergedRegionsToShift)
		{
			WorksheetRegionAddress beforeShiftAddress = shiftOperation.RegionAddressBeforeShift;
			WorksheetRegionAddress afterShiftAddress = shiftOperation.RegionAddressAfterShift;
			if (afterShiftAddress != beforeShiftAddress)
			{
				if (beforeShiftAddress.LastRowIndex < afterShiftAddress.LastRowIndex)
				{
					int lastRowOfSheetIndex = this.Rows.MaxCount - 1;
					Debug.Assert(shiftOperation.RegionAddressBeforeShift.LastRowIndex < lastRowOfSheetIndex, "The last row being moved down cannot be the last row of the sheet.");

					// If the last row is being moved to the bottom of the sheet and the last row has data, the move is not allowed.
					if (lastRowOfSheetIndex <= afterShiftAddress.LastRowIndex)
					{
						Debug.Assert(afterShiftAddress.LastRowIndex == lastRowOfSheetIndex, "This type of shift should never occur.");
						for (int rowIndex = beforeShiftAddress.LastRowIndex + 1; rowIndex <= lastRowOfSheetIndex; rowIndex++)
						{
							// Pass in True for ignoreReferencedCells because when inserting cells, we can shift off a referenced cell
							// and allow the owning formula to have a #REF! error.
							if (this.AreCellsNonTrivial(lastRowOfSheetIndex, beforeShiftAddress.FirstColumnIndex, beforeShiftAddress.LastColumnIndex, true))
								return CellShiftResult.ErrorLossOfData;
						}
					}
				}
				else if (afterShiftAddress.FirstRowIndex < 0)
				{
					Utilities.DebugFail("This type of shift should never occur.");
					return CellShiftResult.ErrorLossOfData;
				}
			}

			// Get tables and merged cells which need to be shifted, and if any will only be partially shifted, the shift 
			// is not allowed.
			
			foreach (WorksheetTable table in this.Tables)
			{
				if (table.IsResizing == false &&
					shiftOperation.VerifyShiftForNonSplittableItem(tablesToShift, table, table.WholeTableAddress) == false)
				{
					return CellShiftResult.ErrorSplitTable;
				}
			}

			
			foreach (WorksheetMergedCellsRegion mergedRegion in this.MergedCellsRegions)
			{
				if (shiftOperation.VerifyShiftForNonSplittableItem(mergedRegionsToShift, mergedRegion, mergedRegion.Address) == false)
					return CellShiftResult.ErrorSplitMergedRegion;
			}

			
			foreach (WorksheetDataTable dataTable in this.DataTables)
			{
				WorksheetRegionAddress cellsInTableAddress = dataTable.CellsInTableAddress;
				if (cellsInTableAddress.IsValid == false)
				{
					Utilities.DebugFail("This is unexpected.");
					continue;
				}

				if (shiftOperation.VerifyShiftForNonSplittableItem(null, dataTable, cellsInTableAddress) == false)
					return CellShiftResult.ErrorSplitBlockingValue;
			}

			
			// Check to see if any blocking values are in the cells.
			Dictionary<IWorksheetRegionBlockingValue, byte> blockingValuesChecked = new Dictionary<IWorksheetRegionBlockingValue, byte>();
			foreach (WorksheetRow sourceRow in this.Rows.GetItemsInRange(beforeShiftAddress.FirstRowIndex, beforeShiftAddress.LastRowIndex))
			{
				foreach (CellDataContext cellContext in sourceRow.GetCellsWithData(beforeShiftAddress.FirstColumnIndex, beforeShiftAddress.LastColumnIndex))
				{
					IWorksheetRegionBlockingValue blockingValue = sourceRow.GetCellValueRaw(cellContext.ColumnIndex) as IWorksheetRegionBlockingValue;
					if (blockingValue != null &&
						blockingValue.Region != null &&
						blockingValuesChecked.ContainsKey(blockingValue) == false)
					{
						blockingValuesChecked[blockingValue] = 0;

						WorksheetRegion region = blockingValue.Region;
						ArrayFormula arrayFormula = blockingValue as ArrayFormula;
						if (arrayFormula != null)
						{
							if (shiftOperation.VerifyShiftForNonSplittableItem(arrayFormulasToShift, arrayFormula, region.Address) == false)
								return CellShiftResult.ErrorSplitBlockingValue;
						}
						else if (shiftOperation.VerifyShiftForNonSplittableItem(null, blockingValue, region.Address) == false)
						{
							return CellShiftResult.ErrorSplitBlockingValue;
						}
					}
				}
			}

			
			foreach (WorksheetShape shape in this.Shapes)
			{
				if (this.VerifyCellShiftForShape(shape, shiftOperation) == false)
					return CellShiftResult.ErrorLossOfObject;
			}

			
			foreach (WorksheetCellComment comment in this.CellOwnedComments.Values)
			{
				if (this.VerifyCellShiftForShape(comment, shiftOperation) == false)
					return CellShiftResult.ErrorLossOfObject;
			}

			return CellShiftResult.Success;
		}

		#endregion // VerifyCellShift

		// MD 2/29/12 - 12.1 - Table Support
		#region VerifyCellShiftForShape

		private bool VerifyCellShiftForShape(WorksheetShape shape, CellShiftOperation shiftOperation)
		{
			WorksheetCellAddress shiftedTopLeftCellAddress;
			PointF shiftedTopLeftCellPosition;
			WorksheetCellAddress shiftedBottomRightCellAddress;
			PointF shiftedBottomRightCellPosition;
			ShiftAddressResult result = shape.ShiftShapeHelper(shiftOperation,
				out shiftedTopLeftCellAddress, out shiftedTopLeftCellPosition,
				out shiftedBottomRightCellAddress, out shiftedBottomRightCellPosition);

			if (result.DidShift)
			{
				// We can delete shapes when we are deleted the cells they are in. 
				// But we can't delete shapes by having them shift off the worksheet.
				if (result.IsDeleted &&
					result.DeleteReason == CellShiftDeleteReason.ShiftedOffWorksheetBottom)
				{
					return false;
				}
			}

			return true;
		}

		#endregion // VerifyCellShiftForShape

		// MD 1/9/08 - BR29299
		#region VerifyNameIsValid

		internal static void VerifyNameIsValid( string name, string paramName )
		{
			if ( name.IndexOfAny( new char[] { ':', '\\', '/', '?', '*', '[', ']' } ) >= 0 )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_InvalidWorksheetName" ), paramName );

			// MD 1/24/12
			// Found while fixing TFS100044
			if (name.StartsWith("'"))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidWorksheetNameStartingQuote"), paramName);

			// MD 12/3/08 - TFS11149
			// The length of the name has to be validated.
			if ( name.Length > 31 )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_WorksheetNameTooLong" ), paramName );
		}

		#endregion VerifyNameIsValid

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			// MD 3/15/12 - TFS104581
			// This should always be done now. The columns collection may not be created even if the column blocks have data.
			//if ( this.HasColumns )
			//    this.Columns.VerifyFormatLimits( limitErrors, testFormat );
			this.Columns.VerifyFormatLimits(limitErrors, testFormat);

			if ( this.HasRows )
				this.Rows.VerifyFormatLimits( limitErrors, testFormat );

			if ( this.HasMergedCellRegions )
				this.MergedCellsRegions.VerifyFormatLimits( limitErrors, testFormat );

			// MD 5/25/11 - Data Validations / Page Breaks
			this.PrintOptions.VerifyFormatLimits(limitErrors, testFormat);
		}

		#endregion VerifyFormatLimits

		// MD 2/24/12 - 12.1 - Table Support
		#region VerifyFormula

		internal void VerifyFormula(Formula formula, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
		{
			Workbook workbook = this.Workbook;
			if (workbook != null)
				workbook.VerifyFormula(formula, formulaOwnerRow, formulaOwnerColumnIndex);
		}

		#endregion // VerifyFormula

		//  BF 8/21/08  Excel2007 Format
        #region InitShapesCollection






        internal void InitShapesCollection( WorksheetShapesHolder shapesHolder )
        {
            foreach( WorksheetShape shape in shapesHolder.Shapes )
            {
                WorksheetShapeSerializationManager.OnWorksheetAttached( shape, this );
            }
        }
        #endregion InitShapesCollection

        #endregion Internal Methods

        #region Private Methods

        #region AssignShapeId

        private void AssignShapeId( WorksheetShape shape, ref uint nextShapeId )
		{
			// Increment the shape count for the worksheet
			this.numberOfShapes++;

			// Assign the shape id to the current shape
			shape.ShapeId = nextShapeId++;
		}

		#endregion AssignShapeId

		// MD 3/5/12 - 12.1 - Table Support
		#region ClearCells

		private void ClearCells(WorksheetRegionAddress regionAddressToClear, int? indexOfRowForInitializingCellFormat)
		{
			WorksheetRow rowForInitializingCellFormats = indexOfRowForInitializingCellFormat.HasValue
				? this.Rows.GetIfCreated(indexOfRowForInitializingCellFormat.Value)
				: null;

			foreach (WorksheetRow rowToClear in this.Rows.GetItemsInRange(regionAddressToClear.FirstRowIndex, regionAddressToClear.LastRowIndex))
			{
				foreach (CellDataContext cellContext in rowToClear.GetCellsWithData(regionAddressToClear.FirstColumnIndex, regionAddressToClear.LastColumnIndex))
				{
					if (cellContext.CellBlock != null)
						cellContext.CellBlock.ClearCell(rowToClear, cellContext.ColumnIndex);
				}

				for (short columnIndex = regionAddressToClear.FirstColumnIndex; 
					columnIndex <= regionAddressToClear.LastColumnIndex; 
					columnIndex++)
				{
					
					WorksheetCellFormatProxy cellFormatToClear = rowToClear.GetCellFormatInternal(columnIndex);
					cellFormatToClear.Reset();

					if (rowForInitializingCellFormats != null)
						cellFormatToClear.SetFormatting(rowForInitializingCellFormats.GetCellFormatInternal(columnIndex));
				}
			}
		}

		#endregion // ClearCells

		#region InitializeShapesHelper

		// MD 9/2/08 - Cell Comments
		// Made generic so different types could be passed in for the first parameter
		//private void AssignShapeIdsHelpher( WorksheetShapeCollection shapes, ref uint nextShapeId )
		// MD 10/30/11 - TFS90733
		// Changed this method signature because it now does more than just assign shape ids.
		//private void AssignShapeIdsHelpher<T>( IEnumerable<T> shapes, ref uint nextShapeId ) where T : WorksheetShape
		private void InitializeShapesHelper<T>(IEnumerable<T> shapes, ref uint nextShapeId, ref ushort nextObjId) where T : WorksheetShape
		{
			// MD 4/28/11 - TFS62775
			// This can now be done in a simpler way by using PrepareShapeForSerialization.
			#region Refactored

			//foreach ( WorksheetShape shape in shapes )
			//{
			//    WorksheetShapeGroup group = shape as WorksheetShapeGroup;
			//
			//    if ( group != null )
			//    {
			//        // If the current shape is a shape group, assign shape ids to the shapes it contains
			//
			//        if ( group.Shapes.Count == 1 )
			//        {
			//            // If the group only has one shape, we will promote its child shape to the level of the group
			//            // and the group will not be saved.
			//            this.AssignShapeId( group.Shapes[ 0 ], ref nextShapeId );
			//        }
			//        else if ( group.Shapes.Count > 1 )
			//        {
			//            // If the group has multiple shapes assign a shape id to the group and the shapes within the group
			//            this.AssignShapeId( group, ref nextShapeId );
			//            this.AssignShapeIdsHelpher( group.Shapes, ref nextShapeId );
			//        }
			//        else
			//        {
			//            // If the group has no shapes, we will not save the group
			//        }
			//    }
			//    else
			//    {
			//        // For all other shapes, just assign a shape id to the shape
			//        this.AssignShapeId( shape, ref nextShapeId );
			//    }
			//} 

			#endregion  // Refactored
			WorkbookSerializationManager manager = this.Workbook.CurrentSerializationManager;
			foreach (WorksheetShape shape in shapes)
			{
				WorksheetShape shapeToSerialize = shape;
				manager.PrepareShapeForSerialization(ref shapeToSerialize);
				if (shapeToSerialize == null)
					continue;

				// MD 10/30/11 - TFS90733
				// We need to initialize the Obj record when the shape is initialized for serialization.
				shape.PopulateObjRecords();

				Debug.Assert(shape.Obj.Cmo != null, "The cmo field should never be null on the Obj.");
				if (shape.Obj.Cmo != null)
					shape.Obj.Cmo.Id = nextObjId++;

				// For all shapes to serialize, assign a shape id
				this.AssignShapeId(shapeToSerialize, ref nextShapeId);

				WorksheetShapeGroup group = shapeToSerialize as WorksheetShapeGroup;
				if (group != null)
				{
					// MD 10/30/11 - TFS90733
					//this.AssignShapeIdsHelpher(group.Shapes, ref nextShapeId);
					this.InitializeShapesHelper(group.Shapes, ref nextShapeId, ref nextObjId);
				}
			}
		}

		#endregion InitializeShapesHelper

		// MD 8/21/08 - Excel formula solving
		#region CleanCachedRegions

		private void CleanCachedRegions()
		{
			// MD 2/29/12 - 12.1 - Table Support
			// Refactored code into the IterateCachedRegions method.
			//if ( this.cachedRegions == null )
			//    return;
			//
			//for ( int i = this.cachedRegions.Count - 1; i >= 0; i-- )
			//{
			//    if ( Utilities.GetWeakReferenceTarget( this.cachedRegions[ i ] ) == null )
			//        this.cachedRegions.RemoveAt( i );
			//}
			this.IterateCachedRegions(null);
		} 

		#endregion CleanCachedRegions

		// MD 8/28/08 - Excel formula solving
		#region GetCellOrRegionHelper

		// MD 2/24/12 - 12.1 - Table Support
		//private void GetCellOrRegionHelper( string address, CellReferenceMode cellReferenceMode, WorksheetCell originCell, out WorksheetCell cell, out WorksheetRegion region )
		private void GetCellOrRegionHelper(string address, CellReferenceMode cellReferenceMode, WorksheetCell originCell, out WorksheetCell cell, out WorksheetRegion[] regions, out bool throwOnError)
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (originCell != null && originCell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "originCell");

			throwOnError = true;

			// MD 2/7/12 - 12.1 - Cell Format Updates
			// Moved this code from the other GetCellOrRegionHelper overload because we need to validate and trim the address for named references as well.
			if (address == null)
				throw new ArgumentNullException("address");

			address = address.TrimStart();

			// MD 4/12/11 - TFS67084
			// Moved all code to new overload.
			int firstRowIndex;
			short firstColumnIndex;
			int lastRowIndex;
			short lastColumnIndex;
			this.GetCellOrRegionHelper(address, cellReferenceMode, originCell, out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			cell = null;

			// MD 2/27/12 - TFS102520
			//region = null;
			regions = null;
			if (0 <= lastRowIndex)
			{
				// MD 2/27/12 - TFS102520
				//region = this.GetCachedRegion(firstRowIndex, firstColumnIndex, lastRowIndex, lastColumnIndex);
				regions = new WorksheetRegion[] { this.GetCachedRegion(firstRowIndex, firstColumnIndex, lastRowIndex, lastColumnIndex) };

				return;
			}

			if (0 <= firstRowIndex)
			{
				cell = this.Rows[firstRowIndex].Cells[firstColumnIndex];
				return;
			}

			// MD 2/24/12 - 12.1 - Table Support
			// If the address couldn't be parsed as a normal row/column address, parse it as a reference.
			if (this.Workbook == null)
				return;

			cell = null;
			regions = null;

			WorksheetRow originRow = null;
			short originColumnIndex = -1;
			if (originCell != null)
			{
				originRow = originCell.Row;
				originColumnIndex = originCell.ColumnIndexInternal;
			}

			bool isNamedReference;
			ExcelRefBase excelReference = this.Workbook.ParseReference(address.Trim(), cellReferenceMode, this, originRow, originColumnIndex, out isNamedReference);

			// Names which can't be found don't throw an error.
			if (isNamedReference)
				throwOnError = false;

			if (excelReference == null)
				return;

			CellCalcReference cellCalcReference = excelReference as CellCalcReference;
			if (cellCalcReference != null)
			{
				if (excelReference.Row.Worksheet == this)
					cell = cellCalcReference.Row.Cells[cellCalcReference.ColumnIndex];

				return;
			}

			object context = excelReference.Context;
			NamedReference namedReference = context as NamedReference;
			if (namedReference != null)
			{
				cell = namedReference.ReferencedCell;

				if (cell == null)
				{
					WorksheetRegion region = namedReference.ReferencedRegion;
					if (region != null)
						regions = new WorksheetRegion[] { region };
					else
						regions = namedReference.ReferencedRegions;
				}
			}
			else
			{
				cell = context as WorksheetCell;

				if (cell == null)
				{
					WorksheetRegion region = context as WorksheetRegion;
					IEnumerable<WorksheetRegion> regionsEnumerable = context as IEnumerable<WorksheetRegion>;
					if (region != null)
						regions = new WorksheetRegion[] { region };
					else if (regionsEnumerable != null)
						regions = new List<WorksheetRegion>(regionsEnumerable).ToArray();
				}
			}

			if (cell != null && cell.Worksheet != this)
				cell = null;

			if (regions != null && regions.Length != 0 && regions[0].Worksheet != this)
				regions = null;
		}

		// MD 4/12/11 - TFS67084
		// Added new overload so we don't have to use WorksheetCell objects.
		private void GetCellOrRegionHelper(
			string address, 
			CellReferenceMode cellReferenceMode, 
			WorksheetCell originCell,
			out int firstRowIndex, out short firstColumnIndex,
			out int lastRowIndex, out short lastColumnIndex)
		{
			// MD 2/7/12 - 12.1 - Cell Format Updates
			// Moved this to the other GetCellOrRegionHelper overload because we need to validate and trim the address for named references as well.
			//if ( address == null )
			//    throw new ArgumentNullException( "address" );

			if ( Enum.IsDefined( typeof( CellReferenceMode ), cellReferenceMode ) == false )
				throw new InvalidEnumArgumentException( "cellReferenceMode", (int)cellReferenceMode, typeof( CellReferenceMode ) );

			// MD 2/7/12 - 12.1 - Cell Format Updates
			// Moved this to the other GetCellOrRegionHelper overload because we need to validate and trim the address for named references as well.
			//address = address.TrimStart();

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Utilities.ParseRegionAddress( address, this, cellReferenceMode, originCell, out region, out cell );
			WorksheetRow originCellRow = null;
			short originCellColumnIndex = -1;
			if (originCell != null)
			{
				originCellRow = originCell.Row;
				originCellColumnIndex = originCell.ColumnIndexInternal;
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Utilities.ParseRegionAddress(address, this, cellReferenceMode, originCellRow, originCellColumnIndex, out region, out cell);
			//
			//if ( cell != null || region != null )
			//    return;
			//
			//if (cellReferenceMode == CellReferenceMode.R1C1 && originCell == null)
			//{
			//    Utilities.ParseRegionAddress( address, this, cellReferenceMode, this.Rows[ 0 ].Cells[ 0 ], out region, out cell );
			//	
			//    if ( cell != null || region != null )
			//        throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_RelativeR1C1AddressNeedsOriginCell" ) );
			//}
			// MD 5/13/11 - Data Validations / Page Breaks
			//Utilities.ParseRegionAddress(address, this, cellReferenceMode, originCellRow, originCellColumnIndex, 
			WorkbookFormat format = this.CurrentFormat;
			// MD 4/9/12 - TFS101506
			//Utilities.ParseRegionAddress(address, format, cellReferenceMode, originCellRow, originCellColumnIndex, 
			CultureInfo culture = this.Culture;
			Utilities.ParseRegionAddress(address, format, cellReferenceMode, culture, originCellRow, originCellColumnIndex, 
				out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			if (firstRowIndex < 0 && cellReferenceMode == CellReferenceMode.R1C1 && originCell == null)
			{
				// MD 5/13/11 - Data Validations / Page Breaks
				//Utilities.ParseRegionAddress(address, this, cellReferenceMode, this.Rows[0], 0,
				// MD 4/9/12 - TFS101506
				//Utilities.ParseRegionAddress(address, format, cellReferenceMode, this.Rows[0], 0,
				Utilities.ParseRegionAddress(address, format, cellReferenceMode, culture, this.Rows[0], 0,
					out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

				if (0 <= firstRowIndex)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_RelativeR1C1AddressNeedsOriginCell"));
			}
		} 

		#endregion GetCellOrRegionHelper

		// MD 2/10/12 - TFS97827
		#region GetColumnWidthPadding

		private static int GetColumnWidthPadding(double zeroCharacterWidth)
		{
			return (int)Math.Ceiling(zeroCharacterWidth / 4.0) * 2 + 1;
		}

		#endregion // GetColumnWidthPadding

		// MD 2/29/12 - 12.1 - Table Support
		#region IterateCachedRegions

		private delegate void IterateCachedRegionCallback(WorksheetRegion region, out bool modified);

		private void IterateCachedRegions(IterateCachedRegionCallback callback)
		{
			if (this.cachedRegions == null)
				return;

			List<WorksheetRegion> modifiedRegionsToReadd = null;
			for (int i = this.cachedRegions.Count - 1; i >= 0; i--)
			{
				WorksheetRegion region = (WorksheetRegion)Utilities.GetWeakReferenceTarget(this.cachedRegions[i]);
				if (region == null)
				{
					this.cachedRegions.RemoveAt(i);
					continue;
				}

				if (callback != null)
				{
					bool modified;
					callback(region, out modified);

					if (modified)
					{
						this.cachedRegions.RemoveAt(i);

						if (region.Worksheet != null)
						{
							if (modifiedRegionsToReadd == null)
								modifiedRegionsToReadd = new List<WorksheetRegion>();

							modifiedRegionsToReadd.Add(region);
						}
					}
				}
			}

			if (modifiedRegionsToReadd != null)
			{
				for (int i = 0; i < modifiedRegionsToReadd.Count; i++)
				{
					WorksheetRegion modifiedRegion = modifiedRegionsToReadd[i];

					WorksheetRegion existingRegion;
					this.AddCachedRegion(modifiedRegion, true, out existingRegion);
				}
			}
		}

		#endregion // IterateCachedRegions

		// MD 11/3/10 - TFS49093
		#region UnrootStringsInShapes

		private static void UnrootStringsInShapes(IWorksheetShapeOwner owner)
		{
			foreach (WorksheetShape shape in owner.Shapes)
			{
				WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;

				// MD 11/8/11 - TFS85193
				// The Text is no longer lazily initialized when gotten. So we must check for null here.
				//if (shapeWithText != null)
				if (shapeWithText != null && shapeWithText.Text != null)
				    shapeWithText.Text.SetWorksheet(null);

				WorksheetShapeGroup group = shape as WorksheetShapeGroup;

				if (group != null)
					Worksheet.UnrootStringsInShapes(group);
			}
		} 

		#endregion // UnrootStringsInShapes

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Columns

		/// <summary>
		/// Gets the collection of columns in the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The collection of columns is a fixed length collection, with the maximum number of columns in the collection being 
        /// <see cref="Excel.Workbook.MaxExcelColumnCount"/> or <see cref="Excel.Workbook.MaxExcel2007ColumnCount"/>,
        /// depending on the <see cref="Excel.Workbook.CurrentFormat">Workbook.CurrentFormat</see>. Internally, the columns 
		/// are only created and added to the collection when they are requested.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> Iterating the collection will not create all columns. It will only iterate the columns which have already 
		/// been used.  To create and iterate all columns in the worksheet use a For loop, iterating from 0 to one less than 
		/// the maximum column count, and pass in each index to the collection's indexer.
		/// </p>
		/// </remarks>
		/// <value>The collection of columns in the worksheet.</value>
		/// <seealso cref="WorksheetColumn"/>
		public WorksheetColumnCollection Columns
		{
			get
			{
				if ( this.columns == null )
					this.columns = new WorksheetColumnCollection( this );

				return this.columns;
			}
		}

		// MD 3/15/12 - TFS104581
		// This is no longer needed.
		//internal bool HasColumns
		//{
		//    get
		//    {
		//        return
		//            this.columns != null &&
		//            this.columns.Count > 0;
		//    }
		//}

		#endregion Columns

		#region DataTables

		/// <summary>
		/// Gets the collection of data tables in the worksheet.
		/// </summary>
		/// <value>The collection of data tables in the worksheet.</value>
		/// <seealso cref="WorksheetDataTable"/>
		public WorksheetDataTableCollection DataTables
		{
			get
			{
				if ( this.dataTables == null )
					this.dataTables = new WorksheetDataTableCollection( this );

				return this.dataTables;
			}
		}

		internal bool HasDataTables
		{
			get
			{
				return
					this.dataTables != null &&
					this.dataTables.Count > 0;
			}
		}

		#endregion DataTables

		// MD 2/1/11 - Data Validation support
		#region DataValidationRules

		/// <summary>
		/// Gets the collection of data validation rules applied to cells in the Worksheet.
		/// </summary>
		/// <seealso cref="DataValidationRule"/>
		/// <seealso cref="WorksheetCell.DataValidationRule"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public DataValidationRuleCollection DataValidationRules
		{
			get
			{
				if (this.dataValidationRules == null)
					this.dataValidationRules = new DataValidationRuleCollection(this);

				return this.dataValidationRules;
			}
		}

		internal bool HasDataValidationRules
		{
			get
			{
				return 
					this.dataValidationRules != null && 
					this.dataValidationRules.Count > 0;
			}
		}

		#endregion  // DataValidationRules

		#region DefaultColumnWidth

		/// <summary>
		/// Gets or sets the default column width including padding, in 256ths of the '0' digit character width in the workbook's default font.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default column width is the width of all columns which do not have their width set.
		/// </p>
		/// <p class="body">
		/// The value assigned must be between 0 and 65535. Invalid values will be automatically adjusted to valid values.
		/// </p>
		/// <p class="body">
		/// Setting or getting this property is equivalent to calling <see cref="SetDefaultColumnWidth(double,WorksheetColumnWidthUnit)"/> 
		/// or <see cref="GetDefaultColumnWidth"/> using the <see cref="WorksheetColumnWidthUnit"/> value of Character256th.
		/// </p>
		/// </remarks>
		/// <value>
		/// The default column width including padding, in 256ths of the '0' digit character width in the workbook's default font.
		/// </value>
		/// <seealso cref="GetDefaultColumnWidth(WorksheetColumnWidthUnit)"/>
		/// <seealso cref="SetDefaultColumnWidth(double,WorksheetColumnWidthUnit)"/>
		/// <seealso cref="WorksheetColumn.Width"/>
		/// <seealso cref="Infragistics.Documents.Excel.Workbook.CharacterWidth256thsToPixels"/>
		/// <seealso cref="Infragistics.Documents.Excel.Workbook.PixelsToCharacterWidth256ths"/>
		public int DefaultColumnWidth
		{
			// MD 1/11/08 - BR29105
			// The defaultColumnWidth member is not needed anymore, because the DefaultColumnWidth can be obtained 
			// from the new DefaultMinimumCharsPerColumn
			//get { return this.defaultColumnWidth; }
			// MD 2/10/12 - TFS97827
			// We now are back to storing the default column width in 256ths of the zero character width.
			//get { return this.DefaultMinimumCharsPerColumn * 256; }
			get { return this.defaultColumnWidth; }
			set
			{
				// MD 1/11/08 - BR29105
				// The defaultColumnWidth member is not needed anymore, because the DefaultColumnWidth can be obtained 
				// from the new DefaultMinimumCharsPerColumn
				//if ( this.defaultColumnWidth != value )
				if ( this.DefaultColumnWidth != value )
				{
					// MD 7/3/07 - BR24403
					// This exception breaks backwards compatibility. Now the invalid value is "fixed". Comments have been updated.
					//if ( value < 0 || 65535 < value )
					//    throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_DefaultColumnWidth" ) );
					if ( value < 0 )
						value = 0;

					if ( 65535 < value )
						value = 65535;

					// MD 1/11/08 - BR29105
					// The defaultColumnWidth member is not needed anymore, because the DefaultColumnWidth can be obtained 
					// from the new DefaultMinimumCharsPerColumn
					//this.defaultColumnWidth = value;
					// MD 2/10/12 - TFS97827
					// We now are back to storing the default column width in 256ths of the zero character width.
					// Also, clear the width cache on the columns collection (this was being done in the DefaultMinimumCharsPerColumn previously).
					//this.DefaultMinimumCharsPerColumn = value / 256;
					this.defaultColumnWidth = value;

					// MD 3/15/12 - TFS104581
					// The column widths are no longer cached on the balance tree nodes.
					//this.Columns.ResetColumnWidthCache(false);
				}
			}
		}

		#endregion DefaultColumnWidth

		#region DefaultRowHeight

		/// <summary>
		/// Gets or sets the default row height in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default row height is the height of all rows which do not have their height explicitly set 
		/// to a positive number.
		/// </p>
		/// <p class="body">
		/// If the assigned value is -1, the default row height will then be calculated based on the default font 
		/// for the workbook, and subsequently getting this property will return the font-based default row height.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is outside the value range of -1 and 8192.
		/// </exception>
		/// <value>The default row height in twips (1/20th of a point).</value>
		public int DefaultRowHeight
		{
			// MD 1/22/08 - BR29105
			// The defaultRowHeight is now defaultable, so if it is less than zero, we will need ot do calculations.
			//get { return this.defaultRowHeight; }
			get 
			{
				if ( this.defaultRowHeight < 0 )
					return this.DefaultRowHeightResolved;

				return this.defaultRowHeight; 
			}
			set
			{
				if ( this.defaultRowHeight != value )
				{
					// MD 1/22/08 - BR29105
					// The defaultRowHeight is now defaultable, so we have to allow -1.
					//if ( value < 0 || 8192 < value )
					if ( value < -1 || 8192 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_DefaultRowHeight" ) );

					this.defaultRowHeight = value;

					// MD 7/23/10 - TFS35969
					// If the default row height has changed, reset the cache of all row heights.
					this.Rows.ResetHeightCache(false);
				}
			}
		}

		#endregion DefaultRowHeight

		#region DisplayOptions

		/// <summary>
		/// Gets the object which controls the display of the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The display options include any settings which affect the display of the worksheet when viewed in Microsoft Excel.
		/// These settings will not affect the printed worksheet or the data stored in the worksheet.
		/// </p>
		/// </remarks>
		/// <value>The object which controls the display of the worksheet.</value>
		/// <seealso cref="CustomView.GetDisplayOptions(Worksheet)"/>
		public WorksheetDisplayOptions DisplayOptions
		{
			get
			{
				if ( this.displayOptions == null )
					this.displayOptions = new WorksheetDisplayOptions( this );

				return this.displayOptions;
			}
		}

		#endregion DisplayOptions

		#region ImageBackground

		/// <summary>
		/// Gets or sets the background image for the worksheet.
		/// </summary>
		///	<remarks>
		/// <p class="body">
		/// This image is tiled across the background of the worksheet.  If null, the worksheet will have no background.
		/// </p>
		/// </remarks>
		/// <value>The background image for the worksheet.</value>



		public Bitmap ImageBackground

		{
			get { return this.imageBackground; }
			set 
            { 
                this.imageBackground = value; 

                // MBS 8/6/08 - Excel 2007 Format
                // Reset our preferred format flag here, since if the user
                // is specifying an image that we're not loading, the format
                // should be determined from the new image
                this.preferredImageBackgroundFormat = null;
            }
		}

		#endregion ImageBackground

		#region Index

		/// <summary>
        /// Gets the zero-based index of this worksheet in its parent <see cref="Excel.Workbook.Worksheets"/> collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Negative one indicates the worksheet has been removed from its parent collection.
		/// </p>
		/// </remarks>
		/// <value>The zero-based index of this worksheet in its parent Worksheets collection.</value>
		/// <seealso cref="WorksheetCollection.IndexOf"/>
		/// <seealso cref="MoveToIndex"/>
		public int Index
		{
			get
			{
				// MD 4/12/11
				// Found while fixing TFS67084
				// We use the Workbook more often than the parent collection, so we will store that instead.
				//if ( this.parentCollection == null )
				//    return -1;
				//
				//return this.parentCollection.IndexOf( this );
				if (this.workbook == null)
					return -1;

				return this.workbook.Worksheets.IndexOf(this);
			}
		}

		#endregion Index

		#region MergedCellsRegions

		/// <summary>
		/// Gets the collection of merged cell ranges in this worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">Use <see cref="WorksheetMergedCellsRegionCollection.Add"/> method to add new merged cell ranges to the worksheet.</p>
		/// </remarks>
		/// <value>The collection of merged cell ranges in this worksheet.</value>
		/// <seealso cref="WorksheetMergedCellsRegion"/>
		public WorksheetMergedCellsRegionCollection MergedCellsRegions
		{
			get
			{
				if ( this.mergedCells == null )
					this.mergedCells = new WorksheetMergedCellsRegionCollection( this );

				return this.mergedCells;
			}
		}

		internal bool HasMergedCellRegions
		{
			get
			{
				return
					this.mergedCells != null &&
					this.mergedCells.Count > 0;
			}
		}

		#endregion MergedCellsRegions

		#region Name

		/// <summary>
		/// Gets or sets the worksheet name.
		/// </summary>
		/// <remarks>
		/// <p class="body">The worksheet name is case-insensitively unique in the workbook.</p>
		/// <p class="body">
		/// The worksheet name is shown in the tab for the worksheet. In addition, the worksheet name can be used by formulas 
		/// from other worksheets to refer to cells in this worksheet.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned contains the invalid characters: ':', '\', '/', '?', '*', '[', or ']'.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned exceeds 31 characters in length.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is being used as the name of another worksheet (worksheet names are case-insensitively compared).
		/// </exception>
		/// <value>The worksheet name.</value>
		public string Name
		{
            //  BF 8/26/08  Excel2007 Format
            //  This can be during null deserialization



			get { return this.name; }


            set
			{
				if ( this.name != value )
				{
					if ( String.IsNullOrEmpty( value ) )
						throw new ArgumentNullException( "value", SR.GetString( "LE_ArgumentNullException_WorksheetName" ) );

					// MD 1/9/08 - BR29299
					// The name could have invalid characters
					Worksheet.VerifyNameIsValid( value, "value" );

					if ( this.workbook != null )
						this.workbook.VerifyWorksheetName(this, value, "value");

					string oldName = this.name;

					this.name = value;

					if ( this.workbook != null &&
						// MD 4/6/12 - TFS101506
						//String.Compare( this.name, oldName, StringComparison.CurrentCultureIgnoreCase ) != 0 )
						String.Compare(this.name, oldName, this.Culture, CompareOptions.IgnoreCase) != 0)
					{
						this.workbook.OnWorksheetRenamed( this, oldName );
					}
				}
			}
		}

		#endregion Name

		#region PrintOptions

		/// <summary>
		/// Gets the object which controls how the worksheet prints.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The print options include any settings which affect the printed appearance of the worksheet.  These settings will
		/// not affect the data in the worksheet.  Although these are not display settings, some worksheet display styles will
		/// display all or some of the print options, so these settings may affect the display of the worksheet when viewed in 
		/// Microsoft Excel.
		/// </p>
		/// </remarks>
		/// <value>The object which controls how the worksheet prints.</value>
		/// <seealso cref="CustomView.GetPrintOptions(Worksheet)"/>
		public PrintOptions PrintOptions
		{
			get
			{
				if ( this.printOptions == null )
				{
					// MD 8/1/08 - BR35121
					// The PrintOptions constructor now needs a reference to the worksheet.
					//this.printOptions = new PrintOptions();
					this.printOptions = new PrintOptions( this );
				}

				return this.printOptions;
			}
		}

		#endregion PrintOptions

		#region Protected

		/// <summary>
		/// Gets or sets the protection state of Excel worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">In protected worksheet cells which are locked can not be modified.</p>
		/// </remarks>
		/// <value>The protection state of Excel worksheet.</value>
		public bool Protected
		{
			get { return this.protectedMbr; }
			set { this.protectedMbr = value; }
		}

		#endregion Protected

		#region Rows

		/// <summary>
		/// Gets the collection of rows in the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The collection of rows is a fixed length collection, with the maximum number of rows in the collection being 
        /// <see cref="Excel.Workbook.MaxExcelRowCount"/> or <see cref="Excel.Workbook.MaxExcel2007RowCount"/>,
        /// depending on the <see cref="Excel.Workbook.CurrentFormat">Workbook.CurrentFormat</see>.  Internally, the rows are only created and added to the collection
		/// when they are requested.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> Iterating the collection will not create all rows. It will only iterate the rows which have already 
		/// been used.  To create and iterate all rows in the worksheet use a For loop, iterating from 0 to one less than 
		/// the maximum row count, and pass in each index to the collection's indexer.
		/// </p>
		/// </remarks>
		/// <value>The collection of rows in the worksheet.</value>
		/// <seealso cref="WorksheetRow"/>
		public WorksheetRowCollection Rows
		{
			get
			{
				if ( this.rows == null )
					this.rows = new WorksheetRowCollection( this );

				return this.rows;
			}
		}

		internal bool HasRows
		{
			get
			{
				return
					this.rows != null &&
					this.rows.Count > 0;
			}
		}

		#endregion Rows

		#region Selected

		/// <summary>
		/// Gets the value which indicates whether this worksheet is selected.
		/// </summary>
		/// <remarks>
		/// <p class="body">
        /// If the worksheet has been removed from its parent <see cref="Excel.Workbook.Worksheets"/> collection, this will always return False.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether this worksheet is selected.</value>
		/// <seealso cref="WindowOptions"/>
		/// <seealso cref="T:WindowOptions.SelectedWorksheet"/>
		public bool Selected
		{
			get
			{
				// MD 4/12/11
				// Found while fixing TFS67084
				// We use the Workbook more often than the parent collection, so we will store that instead.
				//Workbook workbook = this.Workbook;
				//
				//if ( workbook == null )
				//    return false;
				//
				//return this == workbook.WindowOptions.SelectedWorksheet;
				if (this.workbook == null)
					return false;

				return this == this.workbook.WindowOptions.SelectedWorksheet;
			}
		}

		#endregion Selected

		#region Shapes

		/// <summary>
		/// Gets the collection of shapes on the worksheet.
		/// </summary>
		/// <value>The collection of shapes on the worksheet.</value>
		/// <seealso cref="WorksheetShape"/>
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

		// MD 12/7/11 - 12.1 - Table Support
		#region Tables

		/// <summary>
		/// Gets the collection of <see cref="WorksheetTable"/> instances, or regions formatted as tables, in the worksheet.
		/// </summary>
		/// <seealso cref="WorksheetTable"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public WorksheetTableCollection Tables
		{
			get
			{
				if (this.tables == null)
					this.tables = new WorksheetTableCollection(this);

				return this.tables;
			}
		}

		#endregion // Tables

		#region Workbook

		/// <summary>
		/// Gets the <see cref="Workbook"/> that owns the worksheet.
		/// </summary>
		/// <value>The Workbook that owns the worksheet.</value>
		public Workbook Workbook
		{
			get
			{
				// MD 4/12/11
				// Found while fixing TFS67084
				// We use the Workbook more often than the parent collection, so we will store that instead.
				//if ( this.parentCollection == null )
				//    return null;
				//
				//return this.parentCollection.Workbook;
				return this.workbook;
			}
		}

		#endregion Workbook

		#endregion Public Properties

		#region Internal Properties

		// MD 2/27/12
		// Found while implementing 12.1 - Table Support
		// We will now store the cached height on the row itself.
		#region Removed

		//        // MD 1/18/11 - TFS62762
		//        #region CachedRowHeightsWithWrappedText

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the collection of cached calculated row heights base don the wrapping for cell text values.
		//        /// If the row has a non-default height, it will never calculate the wrapped text height and so it will 
		//        /// never cache a value in this collection.
		//        /// </summary>  
		//#endif
		//        internal Dictionary<WorksheetRow, int> CachedRowHeightsWithWrappedText
		//        {
		//            get
		//            {
		//                if (this.cachedRowHeightsWithWrappedText == null)
		//                    this.cachedRowHeightsWithWrappedText = new Dictionary<WorksheetRow, int>();

		//                return this.cachedRowHeightsWithWrappedText;
		//            }
		//        }

		//        // MD 2/3/12 - TFS100573
		//        internal bool HasCachedRowHeightsWithWrappedText
		//        {
		//            get { return this.cachedRowHeightsWithWrappedText != null && this.cachedRowHeightsWithWrappedText.Count != 0; }
		//        }

		//        #endregion // CachedRowHeightsWithWrappedText

		#endregion // Removed

		// MD 3/16/09 - TFS14252
		// Removed seldom used member variables from the cell and cached them on the worksheet so cells don't 
		// have such a large memory footprint.
		#region CellOwnedAssociatedMergedCellsRegions

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal Dictionary<WorksheetCell, WorksheetMergedCellsRegion> CellOwnedAssociatedMergedCellsRegions
		internal Dictionary<WorksheetCellAddress, WorksheetMergedCellsRegion> CellOwnedAssociatedMergedCellsRegions
		{
			get
			{
				if ( this.cellOwnedAssociatedMergedCellsRegions == null )
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//this.cellOwnedAssociatedMergedCellsRegions = new Dictionary<WorksheetCell, WorksheetMergedCellsRegion>();
					this.cellOwnedAssociatedMergedCellsRegions = new Dictionary<WorksheetCellAddress, WorksheetMergedCellsRegion>();
				}

				return this.cellOwnedAssociatedMergedCellsRegions;
			}
		}

		// MD 4/12/11 - TFS67084
		internal bool HasCellOwnedAssociatedMergedCellsRegions
		{
			get { return this.cellOwnedAssociatedMergedCellsRegions != null && this.cellOwnedAssociatedMergedCellsRegions.Count > 0; }
		}

		#endregion CellOwnedAssociatedMergedCellsRegions

		// MD 2/29/12 - 12.1 - Table Support
		#region CellShiftHistory

		internal List<CellShiftOperation> CellShiftHistory
		{
			get
			{
				if (this.cellShiftHistory == null)
					this.cellShiftHistory = new List<CellShiftOperation>();

				return this.cellShiftHistory;
			}
		}

		#endregion // CellShiftHistory

		// MD 2/29/12 - 12.1 - Table Support
		#region CellShiftHistoryVersion

		internal int CellShiftHistoryVersion
		{
			get
			{
				if (this.cellShiftHistory == null)
					return 0;

				return this.cellShiftHistory.Count;
			}
		}

		#endregion // CellShiftHistoryVersion

		// MD 4/12/11 - TFS67084
		// This is no longer needed
		#region Removed

		//#region CellOwnedCachedRegions

		//internal Dictionary<WorksheetCell, WorksheetRegion> CellOwnedCachedRegions
		//{
		//    get
		//    {
		//        if ( this.cellOwnedCachedRegions == null )
		//            this.cellOwnedCachedRegions = new Dictionary<WorksheetCell, WorksheetRegion>();

		//        return this.cellOwnedCachedRegions;
		//    }
		//}

		//#endregion CellOwnedCachedRegions 

		#endregion  // Removed

		// MD 4/12/11 - TFS67084
		// This is no longer needed because the cell formats were moved to the row.
		#region Removed

		//#region CellOwnedCellFormats
		//
		//internal Dictionary<WorksheetCell, WorksheetCellFormatProxy> CellOwnedCellFormats
		//{
		//    get
		//    {
		//        if ( this.cellOwnedCellFormats == null )
		//            this.cellOwnedCellFormats = new Dictionary<WorksheetCell, WorksheetCellFormatProxy>();
		//
		//        return this.cellOwnedCellFormats;
		//    }
		//}
		//
		//#endregion CellOwnedCellFormats 

		#endregion  // Removed

		#region CellOwnedComments

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal Dictionary<WorksheetCell, WorksheetCellComment> CellOwnedComments
		internal Dictionary<WorksheetCellAddress, WorksheetCellComment> CellOwnedComments
		{
		    get
		    {
		        if ( this.cellOwnedComments == null )
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
		            //this.cellOwnedComments = new Dictionary<WorksheetCell, WorksheetCellComment>();
					this.cellOwnedComments = new Dictionary<WorksheetCellAddress, WorksheetCellComment>();
				}

		        return this.cellOwnedComments;
		    }
		}

		// MD 4/12/11 - TFS67084
		internal bool HasCellOwnedComments
		{
			get { return this.cellOwnedComments != null && this.cellOwnedComments.Count > 0; }
		}

		#endregion CellOwnedComments

		#region CellOwnedDataTableDependantCells

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal Dictionary<WorksheetCell, List<WorksheetCell>> CellOwnedDataTableDependantCells
		internal Dictionary<WorksheetCellAddress, List<WorksheetCellAddress>> CellOwnedDataTableDependantCells
		{
		    get
		    {
		        if ( this.cellOwnedDataTableDependantCells == null )
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
		            //this.cellOwnedDataTableDependantCells = new Dictionary<WorksheetCell, List<WorksheetCell>>();
					this.cellOwnedDataTableDependantCells = new Dictionary<WorksheetCellAddress, List<WorksheetCellAddress>>();
				}

		        return this.cellOwnedDataTableDependantCells;
		    }
		}

		// MD 4/12/11 - TFS67084
		internal bool HasCellOwnedDataTableDependantCells
		{
			get { return this.cellOwnedDataTableDependantCells != null && this.cellOwnedDataTableDependantCells.Count > 0; }
		}

		#endregion CellOwnedDataTableDependantCells

		// MD 8/26/08 - Excel formula solving
		#region CellReferenceMode

		internal CellReferenceMode CellReferenceMode
		{
			get
			{
				// MD 4/12/11
				// Found while fixing TFS67084
				// We use the Workbook more often than the parent collection, so we will store that instead.
				//Workbook workbook = this.Workbook;
				//
				//if ( workbook == null )
				//    return CellReferenceMode.A1;
				//
				//return workbook.CellReferenceMode;
				if (this.workbook == null)
					return CellReferenceMode.A1;

				return this.workbook.CellReferenceMode;
			}
		} 

		#endregion CellReferenceMode

		// MD 4/12/11 - TFS67084
		#region CellValues

		internal Dictionary<WorksheetCellAddress, object> CellValues
		{
			get
			{
				if (this.cellValues == null)
					this.cellValues = new Dictionary<WorksheetCellAddress, object>();

				return this.cellValues;
			}
		}

		// MD 4/12/11 - TFS67084
		internal bool HasCellValues
		{
			get { return this.cellValues != null && this.cellValues.Count > 0; }
		}

		#endregion  // CellValues

		// MD 3/15/12 - TFS104581
		#region ColumnBlocks

		internal SortedList<short, WorksheetColumnBlock> ColumnBlocks
		{
			get { return this.columnBlocks; }
		}

		#endregion // ColumnBlocks

        // MD 7/20/2007 - BR25039
		#region CommentShapes

		// MD 9/2/08 - Cell Comments
		//internal List<WorksheetCellCommentShape> CommentShapes
		internal List<WorksheetCellComment> CommentShapes
		{
			get
			{
				if ( this.commentShapes == null )
				{
					// MD 9/2/08 - Cell Comments
					//this.commentShapes = new List<WorksheetCellCommentShape>();
					this.commentShapes = new List<WorksheetCellComment>();
				}

				return this.commentShapes;
			}
		}

		internal bool HasCommentShapes
		{
			get { return this.commentShapes != null && this.commentShapes.Count > 0; }
		}

		#endregion CommentShapes

		// MD 4/6/12 - TFS101506
		#region Culture

		internal CultureInfo Culture
		{
			get
			{
				if (this.Workbook == null)
					return CultureInfo.CurrentCulture;

				return this.Workbook.CultureResolved;
			}
		}

		#endregion // Culture

		// MD 7/9/08 - Excel 2007 Format
		#region CurrentFormat

		internal WorkbookFormat CurrentFormat
		{
			get
			{
				// MD 4/12/11
				// Found while fixing TFS67084
				// We use the Workbook more often than the parent collection, so we will store that instead.
				//Workbook workbook = this.Workbook;
				//
				//if ( workbook == null )
				//    return WorkbookFormat.Excel97To2003;
				//
				//return workbook.CurrentFormat;
				if (this.workbook == null)
				{
					// MD 2/24/12
					// Found while implementing 12.1 - Table Support
					// We should use the least restrictive format version when there is no workbook, not the most.
					//return WorkbookFormat.Excel97To2003;
					return Workbook.LatestFormat;
				}

				return this.workbook.CurrentFormat;
			}
		} 

		#endregion CurrentFormat

		// MD 2/1/11 - Data Validation support
		//// MD 9/12/08 - TFS6887
		//#region DataValidationElements
		//internal ElementDataCache DataValidationInfo2007
		//{
		//    get { return this.dataValidationInfo2007; }
		//    set { this.dataValidationInfo2007 = value; }
		//} 
		//
		//#endregion DataValidationElements
		//
		//// MD 9/12/08 - TFS6887
		//#region DataValidationInfo
		//
		//internal DataValidationRoundTripInfo DataValidationInfo2003
		//{
		//    get { return this.dataValidationInfo2003; }
		//    set { this.dataValidationInfo2003 = value; }
		//} 
		//
		//#endregion DataValidationInfo

		// MD 7/30/12 - TFS117846
		#region DefaultColumnFormat

		internal WorksheetCellFormatData DefaultColumnFormat
		{
			get { return this.defaultColumnFormat; }
			set { this.defaultColumnFormat = value; }
		}

		#endregion // DefaultColumnFormat

		// MD 2/10/12 - TFS97827
		// In an effort to clean up and simplify some of the column measuring logic, I am removing these.
		#region Removed

		//        // MD 1/11/08 - BR29105
		//        #region DefaultColumnWidthResolved

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the resolved default width of a column, in 1/256s of zero character width.
		//        /// </summary> 
		//#endif
		//        internal int DefaultColumnWidthResolved
		//        {
		//            get
		//            {
		//                if ( this.defaultColumnWidthResolved == 0 ||
		//                    this.defaultColumnWidthResolved_DefaultFontVersion != this.workbook.DefaultFontVersion )
		//                {
		//                    this.defaultColumnWidthResolved_DefaultFontVersion = this.workbook.DefaultFontVersion;

		//                    int zeroCharacterWidth = this.workbook.ZeroCharacterWidth;

		//                    // They add a little spacing to the column and its size is based on the zero character width
		//                    int initialSpacing = (int)Math.Ceiling((zeroCharacterWidth / 2.0) + 1);

		//                    // The initialSpacing must be rounded up to the nearest odd number
		//                    if (initialSpacing % 2 == 0)
		//                        initialSpacing += 1;

		//                    // Determine the minimum number of pixels for the column width
		//                    //int columnWidthPixels = initialSpacing + ( zeroCharacterWidth * this.DefaultMinimumCharsPerColumn );
		//                    int columnWidthPixels = Utilities.MidpointRoundingAwayFromZero(initialSpacing + (zeroCharacterWidth * this.DefaultMinimumCharsPerColumn));

		//                    // columnWidthPixels must be rounded up to the next multiple of 8.
		//                    int remainderBy8 = columnWidthPixels % 8;
		//                    if (remainderBy8 != 0)
		//                        columnWidthPixels += 8 - remainderBy8;

		//                    // Determine the number of 256ths of a character width exist in each pixel
		//                    double charWidth256thsPerPixel = 256.0 / zeroCharacterWidth;

		//                    // Convert from the column width in pixels to the column width in 256ths of the zero character width
		//                    this.defaultColumnWidthResolved = (int)(charWidth256thsPerPixel * columnWidthPixels);
		//                }

		//                return this.defaultColumnWidthResolved;
		//            }
		//            // MBS 7/22/08 - Excel 2007 Format
		//            set
		//            {
		//                // MD 10/24/11 - TFS89375
		//                // We should set the version here so we don't re-get the default column width the next time it is requested.
		//                this.defaultColumnWidthResolved_DefaultFontVersion = this.workbook.DefaultFontVersion;

		//                this.defaultColumnWidthResolved = value;

		//                //this.DefaultMinimumCharsPerColumn = value / 256;
		//                this.DefaultMinimumCharsPerColumn = this.Foo(value);
		//            }
		//        }

		//        #endregion DefaultColumnWidthResolved

		//        // MD 1/11/08 - BR29105
		//        #region DefaultMinimumCharsPerColumn

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the minimum number of zero characters that must fit across the default column.
		//        /// This property is actually meant to supercede DefaultColumnWidth, but we do not need to obsolete or
		//        /// introduce new properties for this fix. If it is needed externally, add validation code to the setter 
		//        /// and complete the /// comments.
		//        /// </summary>  
		//#endif
		//        //internal int DefaultMinimumCharsPerColumn
		//        internal double DefaultMinimumCharsPerColumn
		//        {
		//            get { return this.defaultMinimumCharsPerColumn; }
		//            // MD 7/23/10 - TFS35969
		//            // If the default column width has changed, reset the cache of all row heights.
		//            //set { this.defaultMinimumCharsPerColumn = value; }
		//            set 
		//            {
		//                if (this.defaultMinimumCharsPerColumn == value)
		//                    return;

		//                this.defaultMinimumCharsPerColumn = value;
		//                this.Columns.ResetColumnWidthCache(false);
		//            }
		//        }

		//        #endregion DefaultMinimumCharsPerColumn

		#endregion // Removed

		// MD 1/22/08 - BR29105
		#region DefaultRowHeightResolved






		internal int DefaultRowHeightResolved
		{
			get
			{
				if ( this.defaultRowHeightResolved == 0 ||
					this.defaultRowHeightResolved_DefaultFontVersion != this.workbook.DefaultFontVersion )
				{
					this.defaultRowHeightResolved_DefaultFontVersion = this.workbook.DefaultFontVersion;

					// MD 7/9/10 - TFS34476
					// Moved this code to a helper method so it could be used in other places.
					#region Moved

					
#region Infragistics Source Cleanup (Region)






































































#endregion // Infragistics Source Cleanup (Region)

					
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


					#endregion // Moved

					// MD 1/8/12 - 12.1 - Cell Format Updates
					// The default font information is now exposed off the normal style.
					//this.defaultRowHeightResolved = Worksheet.GetDefaultRowHeight(Workbook.DefaultFontName, this.workbook.DefaultFontHeight);
					WorksheetCellFormatData normalFormat = this.workbook.Styles.NormalStyle.StyleFormatInternal;

					// MD 2/9/12 - TFS89375
					// Cache the default font name and height.
					//this.defaultRowHeightResolved = Worksheet.GetDefaultRowHeight(normalFormat.FontNameResolved, normalFormat.FontHeightResolved);
					string defaultFontName = normalFormat.FontNameResolved;
					int defaultFontHeight = normalFormat.FontHeightResolved;

					this.defaultRowHeightResolved = 0;

					// MD 2/9/12 - TFS89375
					// We also need to measure the fonts from the columns because each column intersects with all rows.
					Dictionary<FontNameHeightPair, bool> measuredFonts = new Dictionary<FontNameHeightPair, bool>();

					// MD 3/15/12 - TFS104581
					#region Old Code

					//for (int i = 0; i < Workbook.GetMaxColumnCount(this.CurrentFormat); i++)
					//{
					//    WorksheetColumn column = this.Columns.GetIfCreated(i);
					//    string fontName;
					//    int fontHeight;
					//    if (column != null && column.HasCellFormat)
					//    {
					//        WorksheetCellFormatData formatData = column.CellFormatInternal.Element;
					//        fontName = formatData.FontNameResolved;
					//        fontHeight = formatData.FontHeightResolved;
					//    }
					//    else
					//    {
					//        fontName = defaultFontName;
					//        fontHeight = defaultFontHeight;
					//    }

					#endregion // Old Code
					foreach (WorksheetColumnBlock column in this.ColumnBlocks.Values)
					{
						WorksheetCellFormatData formatData = column.CellFormat;
						string fontName = formatData.FontNameResolved;
						int fontHeight = formatData.FontHeightResolved;

						FontNameHeightPair fontPair = new FontNameHeightPair(fontName, fontHeight);
						if (measuredFonts.ContainsKey(fontPair))
							continue;

						measuredFonts.Add(fontPair, true);

						this.defaultRowHeightResolved =
							Math.Max(this.defaultRowHeightResolved, Worksheet.GetDefaultRowHeight(fontName, fontHeight));
					}
                }

				return this.defaultRowHeightResolved;
			}
		}

		// MD 2/9/12 - TFS89375
		// Added a way to reset the default row height.
		internal void ResetDefaultRowHeightResolved()
		{
			this.defaultRowHeightResolved = 0;
		}

		// MD 2/9/12 - TFS89375
		private struct FontNameHeightPair
		{
			public readonly int Height;
			public readonly string Name;

			public FontNameHeightPair(string name, int height)
			{
				this.Name = name;
				this.Height = height;
			}

			public override bool Equals(object obj)
			{
				if ((obj is FontNameHeightPair) == false)
					return false;

				FontNameHeightPair other = (FontNameHeightPair)obj;
				return this.Name == other.Name && this.Height == other.Height;
			}

			public override int GetHashCode()
			{
				return this.Name.GetHashCode() ^ this.Height.GetHashCode();
			}
		}

		// MD 7/9/10 - TFS34476
		// Moved this code from the DefaultRowHeightResolved proeprty getter.

		// MD 8/23/11
		// Found while fixing TFS84306
		[SecuritySafeCritical]

		internal static int GetDefaultRowHeight(string fontName, int fontHeightInTwips)
		{



			// Get the default size of the font in points
			float defaultFontSize = fontHeightInTwips / 20.0f;

			// Create a bitmap so we have something to base a graphics object on
			// Also create the font object based on the default name and size.
			using (Bitmap bitmap = new Bitmap(1, 1))
			using (Font font = new Font(fontName, defaultFontSize))
			{
				using (Graphics grfx = Graphics.FromImage(bitmap))
				{
					try
					{
						return Worksheet.GetDefaultRowHeightResolvedUnsafe(grfx, font);
					}
					// If we don't have security rights to do the unmanaged calcualtions, we can try to get close with 
					// GDI+ calculations
					catch (SecurityException)
					{
						// MD 5/6/12 - TFS108013
						// Moved this code to a helper method so it could be used in another catch block.
						#region Moved

						//FontFamily fontFamily = font.FontFamily;
						//int emHeight = fontFamily.GetEmHeight(font.Style);
						//int emLineSpacing = fontFamily.GetLineSpacing(font.Style);

						//float fHeight = font.GetHeight(grfx);
						//double lineSpacing = fHeight * emLineSpacing / emHeight;

						//double defaultRowHeightInPixels = (fHeight + lineSpacing) / 2;

						//// MD 2/10/12 - TFS97827
						//// Consolidated a lot of the unit conversion code so we don't duplicate code.
						////return Math.Max(
						////    7,
						////    (int)Math.Round(defaultRowHeightInPixels * Worksheet.TwipsPerPixelAt96DPI, 0, MidpointRounding.AwayFromZero));
						//return Math.Max(7, (int)Worksheet.ConvertPixelsToTwips(defaultRowHeightInPixels));

						#endregion // Moved
						return Worksheet.GetDefaultRowHeightSafe(font, grfx);
					}
					// MD 5/6/12 - TFS108013
					// In some cases, such as running on a Mono server, a DllNotFoundException may be thrown, in which case we should
					// also use the safe alternative.
					catch (DllNotFoundException)
					{
						return Worksheet.GetDefaultRowHeightSafe(font, grfx);
					}
				}
			}

		}



		// MD 5/6/12 - TFS108013
		// Moved code from GetDefaultRowHeight into a helper method so it could be used in multiple places.
		private static int GetDefaultRowHeightSafe(Font font, Graphics grfx)
		{
			FontFamily fontFamily = font.FontFamily;
			int emHeight = fontFamily.GetEmHeight(font.Style);
			int emLineSpacing = fontFamily.GetLineSpacing(font.Style);

			float fHeight = font.GetHeight(grfx);
			double lineSpacing = fHeight * emLineSpacing / emHeight;

			double defaultRowHeightInPixels = (fHeight + lineSpacing) / 2;

			// MD 2/10/12 - TFS97827
			// Consolidated a lot of the unit conversion code so we don't duplicate code.
			//return Math.Max(
			//    7,
			//    (int)Math.Round(defaultRowHeightInPixels * Worksheet.TwipsPerPixelAt96DPI, 0, MidpointRounding.AwayFromZero));
			return Math.Max(7, (int)Worksheet.ConvertPixelsToTwips(defaultRowHeightInPixels));
		}

		// MD 7/9/10 - TFS34476
		// Made this a static method.
		//private int GetDefaultRowHeightResolvedUnsafe( Graphics grfx, Font font )

		// MD 8/23/11
		// Found while fixing TFS84306
		[SecurityCritical]

		private static int GetDefaultRowHeightResolvedUnsafe(Graphics grfx, Font font)
		{
			NativeWindowMethods.TEXTMETRIC textMetric;

			#region Get the TEXTMETRIC class for the font

			IntPtr hdc = IntPtr.Zero;
			IntPtr oldHFont = IntPtr.Zero;
			IntPtr hfont = IntPtr.Zero;

			try
			{
				hdc = grfx.GetHdc();
				hfont = font.ToHfont();
				oldHFont = NativeWindowMethods.SelectObjectApi( hdc, hfont );

				NativeWindowMethods.GetTextMetricsApi( hdc, out textMetric );
			}
			finally
			{
				// MD 5/6/09 - TFS17286
				// We don't need to use the ReleaseHdc which takes an IntPtr. We can use the parameterless one.
				// And the one which takes a pointer does a demand for unmanaged code permissions while the
				// other overload does not. Also, we should probably check to make sure oldHFont is non-zero 
				// too, so I changed that code as well.
				//if ( hdc != IntPtr.Zero )
				//{
				//    NativeWindowMethods.SelectObjectApi( hdc, oldHFont );
				//    grfx.ReleaseHdc( hdc );
				//}
				if ( oldHFont != IntPtr.Zero && hdc != IntPtr.Zero )
					NativeWindowMethods.SelectObjectApi( hdc, oldHFont );

				// MD 7/8/10 - TFS34814
				// We should be deleting the pointer to the font if it is valid.
				if (hfont != IntPtr.Zero)
					NativeWindowMethods.DeleteObjectApi(hfont);

				grfx.ReleaseHdc();
			}

			#endregion Get the TEXTMETRIC class for the font

			int externalLeadingRoundedDownToOdd = Math.Max( 1, textMetric.tmExternalLeading );

			if ( externalLeadingRoundedDownToOdd % 2 == 0 )
				externalLeadingRoundedDownToOdd--;

			// This algorithm for calculation the default row height seems to work very well for some fonts, like
			// Arial and MS Gothic, but doesn't work for others, like Tunga. However, since we hard code the default
			// font to be Arial right now, it is ok. In the future though, this algorithm may have to be changed.
			int defaultRowHeightInPixels =
				textMetric.tmAscent
				+ ( 2 * Math.Max( 2, textMetric.tmDescent ) )
				+ externalLeadingRoundedDownToOdd
				- textMetric.tmInternalLeading;

			// MD 2/10/12 - TFS97827
			// Consolidated a lot of the unit conversion code so we don't duplicate code.
			//return (int)( defaultRowHeightInPixels * Worksheet.TwipsPerPixelAt96DPI );
			return (int)Worksheet.ConvertPixelsToTwips(defaultRowHeightInPixels);
		}


		#endregion DefaultRowHeightResolved

		#region DefaultRowHidden






		internal bool DefaultRowHidden
		{
			get { return this.defaultRowHidden; }
			// MD 7/23/10 - TFS35969
			// If the default row hidden value has changed, reset the cache of all row heights.
			//set { this.defaultRowHidden = value; }
			set
			{
				if (this.defaultRowHidden == value)
					return;

				this.defaultRowHidden = value;
				this.Rows.ResetHeightCache(true);
			}
		}

		#endregion DefaultRowHidden

		#region FirstColumn

		internal int FirstColumn
		{
			get { return this.firstColumn; }
		}

		#endregion FirstColumn

		#region FirstColumnInUndefinedTail

		internal int FirstColumnInUndefinedTail
		{
			get { return this.firstColumnInUndefinedTail; }
		}

		#endregion FirstColumnInUnderfinedTail

		#region FirstRow

		internal int FirstRow
		{
			get { return this.firstRow; }
		}

		#endregion FirstRow

		#region FirstRowInUndefinedTail

		internal int FirstRowInUndefinedTail
		{
			get { return this.firstRowInUndefinedTail; }
		}

		#endregion FirstRowInUndefinedTail

        // MBS 7/21/08 - Excel 2007 Format
        #region HasLoadedSheetView

        internal bool HasLoadedSheetView
        {
            get { return this.hasLoadedSheetView; }
            set { this.hasLoadedSheetView = value; }
        }
        #endregion //HasLoadedSheetView        

		// MD 8/1/08 - BR35121
		#region IsLoading






		internal bool IsLoading
		{
			get 
			{
				// MD 4/12/11
				// Found while fixing TFS67084
				// We use the Workbook more often than the parent collection, so we will store that instead.
				//Workbook workbook = this.Workbook;
				//
				//if ( workbook == null )
				//    return false;
				//
				//return workbook.IsLoading;
				if (this.workbook == null)
					return false;

				return this.workbook.IsLoading;
			}
		}

		#endregion IsLoading

		#region MaxAssignedShapeId

        internal uint MaxAssignedShapeId
		{
			get { return this.maxAssignedShapeId; }
		}

		#endregion MaxAssignedShapeId

		#region MaxColumnOutlineLevel

		internal int MaxColumnOutlineLevel
		{
			get { return this.maxColumnOutlineLevel; }
		}

		#endregion MaxColumnOutlineLevel

		#region MaxRowOutlineLevel

		internal int MaxRowOutlineLevel
		{
			get { return this.maxRowOutlineLevel; }
		}

		#endregion MaxRowOutlineLevel

		#region NumberOfShapes

		internal uint NumberOfShapes
		{
			get { return this.numberOfShapes; }
		}

		#endregion NumberOfShapes

		#region PatriarchShapeId

		internal uint PatriarchShapeId
		{
			get { return this.patriarchShapeId; }
		}

		#endregion PatriarchShapeId

        // MBS 8/6/08 - Excel 2007 Format
        #region PreferredImageBackgroundFormat

        internal ImageFormat PreferredImageBackgroundFormat
        {
            get { return this.preferredImageBackgroundFormat; }
            set { this.preferredImageBackgroundFormat = value; }
        }
        #endregion //PreferredImageBackgroundFormat

		// MD 9/9/08 - Excel 2007 Format
		#region SheetId






		internal int SheetId
		{
			get
			{
				int index = this.Index;

				if ( index < 0 )
					return -1;

				return index + 1;
			}
		} 

		#endregion SheetId

		// MD 3/21/12 - TFS104630
		#region ShouldSaveDefaultColumnWidths256th

		internal bool ShouldSaveDefaultColumnWidths256th
		{
			get { return this.shouldSaveDefaultColumnWidths256th; }
			set { this.shouldSaveDefaultColumnWidths256th = value; }
		}

		#endregion // ShouldSaveDefaultColumnWidths256th

		// MD 6/19/07 - BR23998
		#region ShowExpansionIndicatorBelowGroup

		internal bool ShowExpansionIndicatorBelowGroup
		{
			get { return this.showExpansionIndicatorBelowGroup; }
			set { this.showExpansionIndicatorBelowGroup = value; }
		}

		#endregion ShowExpansionIndicatorBelowGroup

		// MD 5/4/09 - TFS17197
		// The worksheets will not keep track of their sheet type because we may have to create temporary worksheets for VB modules.
		#region Type

		internal SheetType Type
		{
			get { return this.type; }
			set { this.type = value; }
		} 

		#endregion Type

		// MD 10/1/08 - TFS8453
		#region VBAObjectName

		internal string VBAObjectName
		{
			get { return this.vbaObjectName; }
			set { this.vbaObjectName = value; }
		} 

		#endregion VBAObjectName

		// MD 2/10/12 - TFS97827
		#region ZeroCharacterWidth

		internal int ZeroCharacterWidth
		{
			get
			{
				if (this.Workbook == null)
					return 7;

				return this.Workbook.ZeroCharacterWidth;
			}
		}

		#endregion // ZeroCharacterWidth

		#endregion Internal Properties

		#endregion Properties


		// MD 2/29/12 - 12.1 - Table Support
		#region ShiftRegionHelper class

		private class ShiftRegionHelper
		{
			private CellShiftOperation _shiftOperation;

			public ShiftRegionHelper(CellShiftOperation shiftOperation)
			{
				_shiftOperation = shiftOperation;
			}

			public void RegionCallback(WorksheetRegion region, out bool modified)
			{
				ShiftAddressResult result = region.ShiftRegion(_shiftOperation, true);
				modified = result.DidShift;
			}
		}

		#endregion // ShiftRegionVerticallyHelper class
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