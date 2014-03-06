using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Sorting;






using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization
{
	internal abstract class WorkbookSerializationManager : IDisposable
	{
		#region Constants

		// MD 7/20/2007 - BR25039
		// Surrounded constants in #if block


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion Constants

		#region Member Variables

		private ContextStack contextStack;

		private List<WorkbookFontData> fonts;

		// MD 1/10/12 - 12.1 - Cell Format Updates
		private Dictionary<WorkbookFontData, short> fontIndexHash;

		private List<ImageHolder> images; 

		// MD 10/30/11 - TFS90733
		// This is no longer needed.
		//// MD 9/2/08 - Cell Comments
		//private int maxAssignedObjectIdNumber;

		// MD 9/23/09 - TFS19150
		private WorksheetCellFormatData resolvedDefaultCellFormat;

		// MD 11/3/10 - TFS49093
		// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
		// we are now storing it in a collection on the manager.
		//private List<FormattedStringHolder> sharedStringTable;
		private List<StringElement> sharedStringTable;

		// MD 8/20/07 - BR25818
		// Keep a flag now indicating whether parsed NAME formula tokens should immediately resolve 
		// named references or defer resolving until the workbook global section has been parsed.
		private bool shouldResolveNamedReferences;

		// MD 11/12/07 - BR27987
		// MD 1/1/12 - 12.1 - Cell Format Updates
		// We don't need this to be a full WorkbookStyleCollection, which contains all the presets now.
		//private WorkbookStyleCollection styles;
		private List<WorkbookStyle> styles;

		private uint totalStringsUsedInDocument;
		private Workbook workbook;
		private List<WorkbookReferenceBase> workbookReferences;
		private List<WorksheetReference> worksheetReferences;

		// MD 11/3/10 - TFS49093
		// We need to store StringBuilders that were added to the string table later in the process.
		private Dictionary<StringBuilder, int> additionalStringsInStringTable;

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// The theme colors are now exposed off the workbook.
		//// MD 11/29/11 - TFS96205
		//// Moved this from the Excel2007WorkbookSerializationManager because we can use theme colors in XLS files as well.
		//private List<Color> themeColors;

		// MD 12/21/11 - 12.1 - Table Support
		private List<WorksheetCellFormatData> dxfs;

		#region Loading-only variables

		// MD 9/23/09 - TFS19150
		private WorksheetRow currentRowForLoadingCellValues;

		// Used only for loading
		private string loadingPath;
		private int nextWorksheetIndex;

		// MD 9/23/09 - TFS19150
		private WorksheetRow[] rowsInLoadingBlock;

		private Dictionary<int, int> worksheetIndices; // Indexed by the worksheet tab id ( 1-based )

		// MD 11/10/11 - TFS85193
		private WorksheetShape shapeBeingLoaded;

		// MD 1/1/12 - 12.1 - Cell Format Updates
		private Dictionary<int, List<WorksheetCellFormatData>> parentStylesToChildren;

		#endregion Loading-only variables

		#region Saving-only variables

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//// MD 7/2/09 - TFS18634
		//private Dictionary<WorksheetCellFormatData, int> formatIndexValues =
		//    new Dictionary<WorksheetCellFormatData, int>();

		// MD 9/10/08 - Cell Comments
		private uint maxShapeId;

		// MD 7/2/09 - TFS18634
		// MD 7/26/10 - TFS34398
		// Renamed for clarity
		//private Dictionary<WorksheetCellFormatData, WorksheetCellFormatData> resolvedCellFormatsForCells;
		private Dictionary<WorksheetCellFormatData, WorksheetCellFormatData> resolvedCellFormatsByFormat;

		// MD 7/26/10 - TFS34398
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private Dictionary<WorksheetCell, WorksheetCellFormatData> resolvedCellFormatsByCell;
		// MD 4/18/11 - TFS62026
		// This uses too much memory. We will store the resolve format index on the row cache now.
		//private Dictionary<WorksheetCellAddress, WorksheetCellFormatData> resolvedCellFormatsByCell;

		private Dictionary<WorksheetRow, WorksheetRowSerializationCache> rowSerializationCaches;

		// MD 4/28/11 - TFS62775
		private bool currentWorksheetHasShapes;
		private bool worksheetReferencesInitializedForCharts;

		#endregion Saving-only variables

		#endregion Member Variables

		#region Constructor

		protected WorkbookSerializationManager( Workbook workbook, string loadingPath )
		{
			this.workbook = workbook;
			this.loadingPath = loadingPath;

			this.contextStack = new ContextStack();

			this.fonts = new List<WorkbookFontData>();

			// MD 1/10/12 - 12.1 - Cell Format Updates
			this.fontIndexHash = new Dictionary<WorkbookFontData, short>();

			this.images = new List<ImageHolder>();

			// MD 11/3/10 - TFS49093
			// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
			// we are now storing it in a collection on the manager.
			//this.sharedStringTable = new List<FormattedStringHolder>();
			this.sharedStringTable = new List<StringElement>();
		}

		#endregion Constructor

		#region Finalizer

		~WorkbookSerializationManager()
		{
			Utilities.DebugFail( "The WorkbookSerializationManager was never disposed." );
			this.Dispose( false );
		}

		#endregion Finalizer

		#region Interfaces

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			GC.SuppressFinalize( this );
			this.Dispose( true );
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Abstract Methods

		// MD 1/10/12 - 12.1 - Cell Format Updates
		public abstract void AddFormat(WorksheetCellFormatData format);
		public abstract ushort GetCellFormatIndex(WorksheetCellFormatData cellFormat);
		public abstract WorksheetCellFormatData GetLoadedDefaultCellFormat();
		public abstract ushort GetStyleFormatIndex(WorkbookStyle style);

		protected abstract void LoadWorkbookContents();
		protected abstract void SaveWorkbookContents( bool workbookHasShapes );
		protected abstract void WriteCellArrayFormula( bool isMasterCell );
		protected abstract void WriteCellBlank();
		protected abstract void WriteCellBoolean();
		protected abstract void WriteCellDataTable( bool isMasterCell );
		protected abstract void WriteCellError();
		protected abstract void WriteCellFormattedString();
		protected abstract void WriteCellFormula();
		protected abstract void WriteCellNumber();
		protected abstract bool WriteCellRK(); 
		protected abstract void WriteColumnGroup();
		protected abstract void WriteCustomViewWorkbookData();
		protected abstract void WriteCustomViewWorksheetData( bool savePrintOptions );
		protected abstract void WriteMagnificationLevel();
		protected abstract bool WriteMultipleCellBlanks(); 
		protected abstract bool WriteMultipleCellRKs(); 
		protected abstract void WriteNamedReference( bool namedReferenceHasComment );
		protected abstract void WritePaneInformation();
		protected abstract void WriteStandardFormat();

		// MD 11/29/11 - TFS96205
		// This is no longer needed.
		//protected abstract void WriteWorkbookCellFormat();

		protected abstract void WriteWorkbookFont();
		protected abstract void WriteWorkbookGlobalData( bool hasShapes );
		protected abstract void WriteWorkbookStyle();
		protected abstract void WriteWorksheet( Worksheet worksheet, bool hasShapes );
		protected abstract void WriteWorksheetCellComment(); 

		#endregion Abstract Methods

		#region Public Methods

		// MD 2/17/12 - 12.1 - Table Support
		#region AddDxf

		public uint AddDxf(WorksheetCellFormatData dxf)
		{
			uint index = (uint)this.Dxfs.Count;
			this.Dxfs.Add(dxf);
			return index;
		}

		#endregion // AddDxf

		#region AddFont

		// MD 1/18/12 - 12.1 - Cell Format Updates
		// This overload is no longer needed.
		#region Removed

		//#if DEBUG
		//        /// <summary>
		//        /// Resolves the properties of the specified font and adds it to the manager's collection of 
		//        /// fonts if it doesn't already exist.
		//        /// </summary>
		//#endif
		//        // MD 1/10/12 - 12.1 - Cell Format Updates
		//        //public void AddFont( WorkbookFontData font, bool ensureUniqueness )
		//        public void AddFont(WorkbookFontProxy fontProxy)
		//        {
		//            // MD 8/23/11 - TFS84306
		//            // Moved all code to the new overload.
		//            // MD 11/11/11 - TFS85193
		//            //this.AddFont(font, null, ensureUniqueness);
		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            // Removed the ensureUniqueness parameter.
		//            //this.AddFont(font, null, FontResolverType.Normal, ensureUniqueness);
		//            this.AddFont(fontProxy, null);
		//        }

		#endregion // Removed

		// MD 8/23/11 - TFS84306
		// Added a new overload to take an instance to resolve font property defaults.
		// MD 11/11/11 - TFS85193
		// Added a FontResolverType property so we know the type of object resolving the font.
		//public void AddFont(WorkbookFontData font, IWorkbookFontDefaultsResolver defaultsResolver, bool ensureUniqueness)
		// MD 1/10/12 - 12.1 - Cell Format Updates
		//public void AddFont(WorkbookFontData font, IWorkbookFontDefaultsResolver defaultsResolver, FontResolverType fontResolverType, bool ensureUniqueness)
		public void AddFont(WorkbookFontProxy fontProxy, IWorkbookFontDefaultsResolver defaultsResolver)
		{
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Rewrote this code to be more performant.
			#region Old Code

			////WorkbookFontData resolvedFont = font.ResolvedFontData();
			//WorkbookFontData resolvedFont = font.ResolvedFontData(defaultsResolver);

			//int index = -1;

			//if ( ensureUniqueness )
			//    index = this.fonts.IndexOf( resolvedFont );

			//if ( index < 0 )
			//{
			//    index = this.fonts.Count;
			//    this.fonts.Add( resolvedFont );
			//}

			//// MD 11/11/11 - TFS85193
			////font.IndexInFontCollection = index;
			//font.SetIndexInFontCollection(fontResolverType, index);

			#endregion // Old Code
			WorkbookFontData resolvedFont = fontProxy.Element.ResolvedFontData(defaultsResolver);

			short index;
			if (this.fontIndexHash.TryGetValue(resolvedFont, out index) == false)
			{
				index = (short)this.fonts.Count;
				this.fonts.Add(resolvedFont);
				this.fontIndexHash.Add(resolvedFont, index);
			}

			fontProxy.SaveIndex = index;
		}

		#endregion AddFont

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Removed the AddFormat methods because we no longer need to do format resolving.
		#region Removed

		//        #region AddFormat

		//#if DEBUG
		//        /// <summary>
		//        /// Resolves the properties of the specified formats and adds it to the manager's collection of 
		//        /// formats if it doesn't already exist.
		//        /// </summary>
		//        /// <param name="format">The format to resolve and add to the collection.</param>
		//#endif
		//        public void AddFormat( WorksheetCellFormatData format )
		//        {
		//            // MD 11/12/07 - BR27987
		//            // Call new overload of the method
		//            this.AddFormat( format, true );
		//        }

		//        // MD 11/12/07 - BR27987
		//        // Added new overload to allow the caller to prevent the number formatting flag from being resolved
		//        // in the style options.
		//#if DEBUG
		//        /// <summary>
		//        /// Resolves the properties of the specified formats and adds it to the manager's collection of 
		//        /// formats if it doesn't already exist.
		//        /// </summary>
		//        /// <param name="format">The format to resolve and add to the collection.</param>
		//        /// <param name="ensureUniqueness">False to always add the format to the collection; True to add it only if it unique.</param> 
		//#endif
		//        // MD 12/31/11 - 12.1 - Cell Format Updates
		//        // The allowNumberFormattingFlag is no longer needed now that we are no longer resolving format flags in ResolvedCellFormatData, so I changed this parameter to mean something else
		//        //public void AddFormat( WorksheetCellFormatData format, bool allowNumberFormattingFlag )
		//        public void AddFormat(WorksheetCellFormatData format, bool ensureUniqueness)
		//        {
		//            // MD 12/31/11 - 12.1 - Cell Format Updates
		//            // The allowNumberFormattingFlag is no longer needed now that we are no longer resolving format flags in ResolvedCellFormatData.
		//            //// MD 11/12/07 - BR27987
		//            //// Pass the new parameter to the new overload of the ResolvedCellFormatData method
		//            ////WorksheetCellFormatData resolvedFormat = format.ResolvedCellFormatData();
		//            //WorksheetCellFormatData resolvedFormat = format.ResolvedCellFormatData( allowNumberFormattingFlag );
		//            WorksheetCellFormatData resolvedFormat = format.ResolvedCellFormatData();

		//            // MD 12/31/11 - 12.1 - Cell Format Updates
		//            //this.AddResolvedFormat( resolvedFormat, true );
		//            this.AddResolvedFormat(resolvedFormat, ensureUniqueness);

		//            format.IndexInFormatCollection = resolvedFormat.IndexInFormatCollection;

		//            // MD 11/11/11 - TFS85193
		//            //format.FontInternal.Element.IndexInFontCollection =
		//            //    resolvedFormat.FontInternal.Element.IndexInFontCollection;
		//            format.FontInternal.Element.CopyFontCollectionIndexesFrom(resolvedFormat.FontInternal.Element);

		//            format.IndexInXfsCollection = resolvedFormat.IndexInXfsCollection;
		//        }

		//        #endregion AddFormat

		#endregion // Removed

		// MD 9/2/08 - Excel2007 format
		// Moved from NAMERecord.Load
		#region AddNonExternalNamedReferenceDuringLoad

		public void AddNonExternalNamedReferenceDuringLoad( NamedReference reference, bool isHidden )
		{
			// MD 10/30/11 - TFS90733
			// Macro references don't need to be stored anywhere at this time.
			if (reference.IsMacroName)
				return;

			// MD 11/23/11 - TFS96468
			// Apparently the loaded formula can be null. In this case, we should just ignore the named reference.
			if (reference.FormulaInternal == null)
				return;

			if ( isHidden )
			{
				// MD 10/9/07 - BR27172
				// If we fail extracting special data from the name, store the named reference as a hidden named reference
				//NAMERecord.TryExtractCustomViewInfo( manager, reference );
				if ( WorkbookSerializationManager.TryExtractCustomViewInfo( this, reference ) == false )
					this.Workbook.HiddenNamedReferences.Add( reference );
			}
			else if ( reference.FormulaInternal.PostfixTokenList.Count > 0 )
			{
				this.Workbook.NamedReferences.Add( reference );

				// MD 5/25/11 - Data Validations / Page Breaks
				Worksheet worksheet = reference.Scope as Worksheet;
				if (worksheet != null &&
					reference.IsBuiltIn &&
					reference.BuiltInName == BuiltInName.PrintArea)
				{
					PrintOptions printOptions = worksheet.PrintOptions;

					// MD 4/6/12 - TFS102169
					// We no longer need this code now that the workbook part is loaded before the worksheet parts.
					//printOptions.HorizontalPageBreaks.OnPrintAreasLoaded();
					//printOptions.VerticalPageBreaks.OnPrintAreasLoaded();
				}
			}
		} 

		#endregion AddNonExternalNamedReferenceDuringLoad

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Removed the AddResolvedFormat method because we no longer need to do format resolving.
		#region Removed

		//        #region AddResolvedFormat

		//#if DEBUG
		//        /// <summary>
		//        /// Adds an already resolved format to the manager's collection of formats if it doesn't already exist.
		//        /// </summary>
		//        /// <param name="resolvedFormat">The format to add to the collection.</param>
		//        /// <param name="ensureUniqueness">False to always add the format to the collection; True to add it only if it unique.</param>  
		//        /// <returns>
		//        /// True if the format was added to the collection; False if ensureUniqueness is True and a copy of the format already 
		//        /// exists in the Format collection.
		//        /// </returns>
		//#endif
		//        // MD 3/16/09 - TFS14252
		//        // Changed the return type to bool to indicate whether the item was added to the Formats collection.
		//        //public virtual void AddResolvedFormat( WorksheetCellFormatData resolvedFormat, bool ensureUniqueness )
		//        public virtual bool AddResolvedFormat( WorksheetCellFormatData resolvedFormat, bool ensureUniqueness )
		//        {
		//            // MD 3/16/09 - TFS14252
		//            // This method now returns a bool.
		//            bool returnValue = true;

		//            int index = -1;

		//            if ( ensureUniqueness )
		//            {
		//                // MD 7/2/09 - TFS18634
		//                // Instead of doing a linear search, use a hash table to get the index
		//                //index = this.formats.IndexOf( resolvedFormat );
		//                if ( this.formatIndexValues.TryGetValue( resolvedFormat, out index ) == false )
		//                    index = -1;
		//            }

		//            if ( index < 0 )
		//            {
		//                index = this.formats.Count;

		//                // MD 7/2/09 - TFS18634
		//                // Store the format index in the dictionary of index values.
		//                this.formatIndexValues[ resolvedFormat ] = index;

		//                this.formats.Add( resolvedFormat );

		//                // MD 1/8/12 - 12.1 - Cell Format Updates
		//                // Formats which have to be in separate XF records don't need their fonts to also be in separate font records, so always pass in True for ensureUniqueness.
		//                //this.AddFont( resolvedFormat.FontInternal.Element, ensureUniqueness );
		//                this.AddFont(resolvedFormat.FontInternal.Element, true);
		//            }
		//            // MD 3/16/09 - TFS14252
		//            // If the index was greater than or equal to zero, return False.
		//            else
		//            {
		//                returnValue = false;
		//            }

		//            //resolvedFormat.IndexInFormatCollection = index;

		//            // MD 11/11/11 - TFS85193
		//            //resolvedFormat.FontInternal.Element.IndexInFontCollection = this.formats[index].FontInternal.Element.IndexInFontCollection;
		//            resolvedFormat.FontInternal.Element.CopyFontCollectionIndexesFrom(this.formats[index].FontInternal.Element);

		//            // MD 3/16/09 - TFS14252
		//            // This method now returns a bool.
		//            return returnValue;
		//        }

		//        #endregion AddResolvedFormat

		#endregion // Removed

		// MD 2/20/12 - 12.1 - Table Support
		#region CombineDxfInfo

		public void CombineDxfInfo<T>(WorksheetTableAreaFormatsCollection<T> areaFormats,
			T area,
			CanAreaFormatValueBeSetCallback<T> callback,
			WorkbookStyle style,
			WorksheetCellFormatData dxf)
		{
			this.CombineDxfInfo(areaFormats, area, callback, style, dxf, null);
		}

		public void CombineDxfInfo<T>(WorksheetTableAreaFormatsCollection<T> areaFormats,
			T area,
			CanAreaFormatValueBeSetCallback<T> callback,
			WorkbookStyle style,
			WorksheetCellFormatData dxf,
			WorksheetCellFormatData dxfBorders,
			params CellFormatValue[] borderValues)
		{
			if (style != null || dxf != null || dxfBorders != null)
			{
				if (dxf == null)
					dxf = this.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);

				if (style == null)
					style = this.Workbook.Styles.NormalStyle;

				dxf.SetStyleInternal(style);

				if (dxfBorders != null && borderValues.Length != 0)
				{
					if (Utilities.TestFlag(dxfBorders.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting))
					{
						for (int i = 0; i < borderValues.Length; i++)
							Utilities.CopyCellFormatValue(dxfBorders, dxf, borderValues[i]);
					}
				}

				WorksheetCellFormatProxy areaFormat = areaFormats.GetFormatProxy(this.Workbook, area);
				foreach (CellFormatValue formatValue in WorksheetCellFormatData.AllCellFormatValues)
				{
					if (callback(area, formatValue) == false)
						continue;

					Utilities.CopyCellFormatValue(dxf, areaFormat, formatValue);
				}
			}
		}

		#endregion // CombineDxfInfo

		// MD 2/20/12 - 12.1 - Table Support
		#region ExtractDxfInfo

		public void ExtractDxfInfo<T>(WorksheetTableAreaFormatsCollection<T> areaFormats, T area, out WorkbookStyle style, out WorksheetCellFormatData dxf)
		{
			WorksheetCellFormatData borderDxf;
			this.ExtractDxfInfo(areaFormats, area, out style, out dxf, out borderDxf);
			Debug.Assert(borderDxf == null, "This is unexpected.");
		}

		public void ExtractDxfInfo<T>(WorksheetTableAreaFormatsCollection<T> areaFormats, T area, out WorkbookStyle style, out WorksheetCellFormatData dxf, out WorksheetCellFormatData borderDxf, params CellFormatValue[] borderValues)
		{
			style = null;
			dxf = null;
			borderDxf = null;

			WorksheetTableAreaFormatProxy<T> areaFormatProxy = areaFormats.GetFormatProxy(this.Workbook, area, false);
			if (areaFormatProxy == null)
				return;

			WorksheetCellFormatData areaFormat = areaFormatProxy.Element;
			style = areaFormat.Style;

			if (borderValues.Length != 0)
			{
				areaFormat = areaFormat.CloneInternal();
				borderDxf = this.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);
				for (int i = 0; i < borderValues.Length; i++)
				{
					CellFormatValue formatValue = borderValues[i];
					Utilities.CopyCellFormatValue(areaFormat, borderDxf, formatValue);
					areaFormat.ResetValue(formatValue);
				}

				if (borderDxf.IsEmpty)
					borderDxf = null;

				if (areaFormat.IsAnyBorderPropertySet() == false)
					areaFormat.FormatOptions &= ~WorksheetCellFormatOptions.ApplyBorderFormatting;
			}

			if (areaFormat.IsEmpty == false)
				dxf = areaFormat;
		}

		#endregion // ExtractDxfInfo

		// MD 3/15/12 - TFS104581
		// This is no longer needed now that we actually sotre column blocks on the worksheet.
		#region Removed

		//// MBS 7/30/08 - Excel 2007 Format
		//#region GetColumnBlocks

		//// MD 1/10/12 - 12.1 - Cell Format Updates
		//// This needs to be an instance method.
		////public static List<ColumnBlockInfo> GetColumnBlocks(Worksheet worksheet)
		//public List<ColumnBlockInfo> GetColumnBlocks(Worksheet worksheet)
		//{
		//    if (worksheet.HasColumns == false)
		//        return null;

		//    List<ColumnBlockInfo> list = new List<ColumnBlockInfo>();

		//    // We will be keeping track of adjacent columns with the exact same data, each block of 
		//    // identical columns saves as one COLINFO record
		//    ColumnBlockInfo currentColumnInfo = null;

		//    foreach (WorksheetColumn column in worksheet.Columns)
		//    {
		//        // If the column has default data, we don't need to save a record for it
		//        if (column.HasData == false)
		//            continue;

		//        // If there is no current block, initialize a new block with the current column
		//        if (currentColumnInfo == null)
		//        {
		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            // The manager is needed by the constructor.
		//            //currentColumnInfo = new ColumnBlockInfo(column);
		//            currentColumnInfo = new ColumnBlockInfo(this, column);

		//            continue;
		//        }

		//        // Try to add the next column to the current column info block. It will only be added if it is 
		//        // adjacent to the last column in the block and it has the same data as that column.
		//        // MD 1/10/12 - 12.1 - Cell Format Updates
		//        // The manager is needed by the TryAddColumn method.
		//        //if (currentColumnInfo.TryAddColumn(column) == false)
		//        if (currentColumnInfo.TryAddColumn(this, column) == false)
		//        {
		//            // If the current column could not be added to the current block, save the block
		//            list.Add(currentColumnInfo);

		//            // Start the next column block with the current column
		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            // The manager is needed by the constructor.
		//            //currentColumnInfo = new ColumnBlockInfo(column);
		//            currentColumnInfo = new ColumnBlockInfo(this, column);
		//        }
		//    }

		//    // Save the last column block
		//    if (currentColumnInfo != null)
		//        list.Add(currentColumnInfo);

		//    return list;
		//}
		//#endregion //GetColumnBlocks

		#endregion // Removed

		// MD 9/23/09 - TFS19150
		#region GetRow

		public WorksheetRow GetRow( Worksheet worksheet, int rowIndex )
		{
			if ( this.currentRowForLoadingCellValues != null &&
				this.currentRowForLoadingCellValues.Index == rowIndex &&
				this.currentRowForLoadingCellValues.Worksheet == worksheet )
			{
				return this.currentRowForLoadingCellValues;
			}

			WorksheetRow row = null;

			if ( this.rowsInLoadingBlock != null )
			{
				WorksheetRow firstRow = this.rowsInLoadingBlock[ 0 ];

				if ( firstRow != null )
				{
					int cacheIndex = rowIndex - firstRow.Index;
					if ( 0 <= cacheIndex && cacheIndex < this.rowsInLoadingBlock.Length )
						row = this.rowsInLoadingBlock[ cacheIndex ];
				}
			}

			if ( row == null )
			{
				row = worksheet.Rows[ rowIndex ];
				this.currentRowForLoadingCellValues = row;
			}

			return row;
		}

		#endregion GetRow

		// MD 8/20/07 - BR25818
		// A helper method was created to index worksheet references, for bounds checking purposes
		#region GetWorksheetReference

		public WorksheetReference GetWorksheetReference(int externalSheetIndex)
		{
			if ( externalSheetIndex < 0 || this.WorksheetReferences.Count <= externalSheetIndex )
			{
				Utilities.DebugFail( "The worksheet index was out of range" );
				return null;
			}

			return this.WorksheetReferences[externalSheetIndex];
		}

		#endregion GetWorksheetReference

		#region InitializeImagesFromShapes



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		public void InitializeImagesFromShapes( WorksheetShapeCollection shapes, ref bool hasShapes )
		{
			if ( shapes == null )
				return;

			foreach ( WorksheetShape shape in shapes )
			{
				WorksheetShapeGroup group = shape as WorksheetShapeGroup;

				if ( group != null )
				{
					// If the current shape is a group shape, recursively call this method to add all sub-shapes
					this.InitializeImagesFromShapes( group.Shapes, ref hasShapes );
					continue;
				}

				// MD 4/28/11 - TFS62775
				// The method name no longer applies, but since we are looping over shapes here, we should also do
				// fix-ups for WorksheetChart instances. Apparently, when there is a chart, there must be workbook 
				// and worksheet references to all worksheets from which it uses data. Since adding in extra worksheet 
				// references won't hurt, just add references to all worksheets in the current workbook on the first
				// WorksheetChart we encounter.
				if (shape is WorksheetChart)
				{
					WorksheetShape temp = shape;
					this.PrepareShapeForSerialization(ref temp);
					if (temp == null)
						continue;

					hasShapes = true;

					if (this.worksheetReferencesInitializedForCharts == false)
					{
						foreach (Worksheet worksheet in shapes.Worksheet.Workbook.Worksheets)
							this.Workbook.CurrentWorkbookReference.GetWorksheetReference(worksheet.Name, null);

						this.worksheetReferencesInitializedForCharts = true;
					}
					continue;
				}

				// Set the flag true if it is not already to indicate the worksheet has shapes.
				hasShapes = true;

				// MD 10/30/11 - TFS90733
				// Other shapes can store images now, so look for an IWorksheetImage.
				//WorksheetImage imageShape = shape as WorksheetImage;
				IWorksheetImage imageShape = shape as IWorksheetImage;

				if ( imageShape != null )
				{
					// If the current shape is an image shape, we want to add its image to the manager
					Image image = imageShape.Image;

					// MD 10/30/11 - TFS90733
					// Not all IWorksheetImage instances need to have an image present. Only the WorksheetImage instances need an image.
					//Debug.Assert( image != null );
					Debug.Assert(image != null || (shape is WorksheetImage) == false, "WorksheetImage shapes must have an image.");

					if ( image != null )
					{
						// Create an image holder which will hold the image as well as a reference count to that image
						ImageHolder holder = new ImageHolder( image );

						// Get the index of another image holder holding the same image
						int index = this.images.IndexOf( holder );

						if ( index < 0 )
						{
							// If another image holder doesn't exists, add the one we have created
							this.images.Add( holder );
						}
						else
						{
							// Otherwise, ditch the image holder we created and increment the reference count of the 
							// equivalent holder.
							holder = this.images[ index ];
							holder.ReferenceCount++;
						}
					}
				}
			}
		}

		#endregion InitializeImagesFromShapes

		#region Load






		public void Load()
		{
			// MD 11/10/11 - TFS85193
			//// MD 3/30/11 - TFS69969
			//this.Workbook.OnSavingOrLoading(this);
			bool isLoadingShape = this.ShapeBeingLoaded != null;
			if (isLoadingShape == false)
				this.Workbook.OnSavingOrLoading(this);

			this.worksheetIndices = new Dictionary<int, int>();
			this.LoadWorkbookContents();

			// MD 5/4/09 - TFS17197
			// Remove all worksheets which are not used for actual worksheets.
			for ( int i = this.workbook.Worksheets.Count - 1; i >= 0; i-- )
			{
				if ( this.workbook.Worksheets[ i ].Type != SheetType.Worksheet )
					this.workbook.Worksheets.RemoveAt( i );
			}

			if (Utilities.Is2003Format(this.workbook.CurrentFormat) == false)
			{
				
			}

			// MD 11/10/11 - TFS85193
			//// MD 3/30/11 - TFS69969
			//this.Workbook.OnSavedOrLoaded(this);
			if (isLoadingShape == false)
				this.Workbook.OnSavedOrLoaded(this);
		}

		#endregion Load

		// MD 9/23/09 - TFS19150
		#region OnDBCellRecordLoaded

		public void OnDBCellRecordLoaded()
		{
			this.rowsInLoadingBlock = null;
		}

		#endregion OnDBCellRecordLoaded

		// MD 9/23/09 - TFS19150
		#region OnRowLoaded

		public void OnRowLoaded( WorksheetRow row )
		{
			if ( this.rowsInLoadingBlock == null )
				this.rowsInLoadingBlock = new WorksheetRow[ 32 ];

			WorksheetRow firstRow = this.rowsInLoadingBlock[ 0 ];

			if ( firstRow == null )
			{
				this.rowsInLoadingBlock[ 0 ] = row;
			}
			else
			{
				int cacheIndex = row.Index - firstRow.Index;
				if ( 0 <= cacheIndex && cacheIndex < this.rowsInLoadingBlock.Length )
					this.rowsInLoadingBlock[ cacheIndex ] = row;
			}
		}

		#endregion OnRowLoaded

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// The theme colors are now exposed off the workbook.
		#region Removed

		//// MD 11/29/11 - TFS96205
		//#region PopulateDefaultThemeColors

		//public void PopulateDefaultThemeColors()
		//{
		//    this.ThemeColors.Clear();

		//    this.ThemeColors.Add(Utilities.SystemColorsInternal.WindowTextColor);
		//    this.ThemeColors.Add(Utilities.SystemColorsInternal.WindowColor);
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFF1F497D)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFFEEECE1)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFF4F81BD)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFFC0504D)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFF9BBB59)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFF8064A2)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFF4BACC6)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFFF79646)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFF0000FF)));
		//    this.ThemeColors.Add(Utilities.ColorFromArgb(unchecked((int)0xFF800080)));
		//}

		//#endregion  // PopulateDefaultThemeColors

		#endregion // Removed

		// MD 4/28/11 - TFS62775
		#region PrepareShapeForSerialization



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public virtual void PrepareShapeForSerialization(ref WorksheetShape shape)
		{
			WorksheetShapeGroup group = shape as WorksheetShapeGroup;

			if (group == null)
				return;

			int shapeCount = 0;
			WorksheetShape firstChildPrepared = null;
			for (int i = 0; i < group.Shapes.Count; i++)
			{
				WorksheetShape childShape = group.Shapes[i];
				this.PrepareShapeForSerialization(ref childShape);

				if (childShape == null)
					continue;

				shapeCount++;

				if (firstChildPrepared == null)
					firstChildPrepared = childShape;

				if (shapeCount > 1)
					break;
			}

			if (shapeCount == 0)
			{
				shape = null;
				return;
			}
			else if (shapeCount == 1)
			{
				shape = firstChildPrepared;
				return;
			}
		}

		#endregion  // PrepareShapeForSerialization

		// MD 8/20/07 - BR25818
		// Added helper method to resolved named references from loaded NAME tokens which were 
		// parsed from other named reference formulas
		#region ResolveNamedReferences

		public void ResolveNamedReferences()
		{
			this.shouldResolveNamedReferences = true;

			foreach (NamedReferenceBase namedReference in this.workbook.CurrentWorkbookReference.NamedReferences)
			{
				// MD 1/13/09 - TFS6720
				// The formula could be null if VB Macro named references are stored on the workbook. Just ignore those.
				if (namedReference.FormulaInternal == null)
					continue;

				namedReference.FormulaInternal.ResolveNamedReferencesAfterLoad(this);
			}

			if ( this.Workbook.ExternalWorkbooks != null )
			{
				foreach (ExternalWorkbookReference externalWorkbook in this.Workbook.ExternalWorkbooks.Values)
				{
					foreach ( NamedReferenceBase namedReference in externalWorkbook.NamedReferences )
						namedReference.FormulaInternal.ResolveNamedReferencesAfterLoad( this );
				}
			}
		}

		#endregion ResolveNamedReferences

		// MD 6/16/12 - CalcEngineRefactor
		#region RetainNamedReference

		public void RetainNamedReference(ref NamedReferenceBase namedReferenceBase)
		{
			this.RetainWorkbookReference(namedReferenceBase.WorkbookReference);

			WorkbookReferenceBase workbookReference = namedReferenceBase.WorkbookReference ?? this.workbook.CurrentWorkbookReference;
			object scope = namedReferenceBase.Scope ?? workbookReference.WorkbookScope;
			namedReferenceBase = workbookReference.GetNamedReference(namedReferenceBase.Name, scope, true);
		}

		#endregion // RetainNamedReference

		// MD 6/16/12 - CalcEngineRefactor
		#region RetainWorkbookReference

		public void RetainWorkbookReference(WorkbookReferenceBase workbookReference)
		{
			if (workbookReference == null)
				return;

			if (this.WorkbookReferences.Contains(workbookReference) == false)
				this.WorkbookReferences.Add(workbookReference);
		}

		#endregion // RetainWorkbookReference

		// MD 6/16/12 - CalcEngineRefactor
		#region RetainWorksheetReference

		public void RetainWorksheetReference(WorksheetReference worksheetReference)
		{
			this.RetainWorkbookReference(worksheetReference.WorkbookReference);

			if (this.WorksheetReferences.Contains(worksheetReference) == false)
				this.WorksheetReferences.Add(worksheetReference);
		}

		#endregion // RetainWorksheetReference

		#region Save






		public void Save()
        {
			// MD 2/1/12 - TFS100573
			// Let the string table prepare for saving.
			this.Workbook.SharedStringTable.OnSaving();

			// MD 5/10/12 - TFS111420
			// Moved from below. We need to lock the cell formats collection earlier becuase it can't be modified once the 
			// InitializeCellFormats method is called.
			this.Workbook.CellFormats.OnSaving();

			// MD 2/1/12 - TFS100573
			// Wrapped in a try...finally so the string table can clean up even when there is an error.
			try
			{
			// MD 3/30/11 - TFS69969
			this.Workbook.OnSavingOrLoading(this);

			// MD 9/2/08 - Excel formula solving
			if ( this.workbook.CalculationMode == CalculationMode.Manual && this.workbook.RecalculateBeforeSave )
				this.workbook.Recalculate();

            // Before we can add the cell formats to the manager, we need to add the fonts
            this.InitializeFonts();

			// Initialize some data we need to save later
			this.InitializeCellFormats();

			// MD 2/28/12 - 12.1 - Table Support
			// Moved below because we need to initialize shapes before initializing references.
			//this.InitializeReferences();

			// MD 11/3/10 - TFS49093
			// Initialize the formatted strings and their fonts.
			this.InitializeStrings();

			// MD 2/22/12 - 12.1 - Table Support
			foreach (WorksheetTableStyle customStyle in this.Workbook.CustomTableStyles)
				customStyle.InitSerializationCache(this);

			// Initialize the serialization cache of all worksheet and keep track of whether any worksheets have shapes
			bool hasShapes = false;
			foreach ( Worksheet worksheet in this.workbook.Worksheets )
			{
				worksheet.InitSerializationCache( this, ref hasShapes );
			}

			// MD 9/10/08 - Cell Comments
			// Cache info about the shapes before saving in either format
			this.IntializeShapes();

			// MD 2/28/12 - 12.1 - Table Support
			// Moved from above because we need to initialize shapes before initializing references.
			this.InitializeReferences();

			// MD 5/10/12 - TFS111420
			// Moved above. We need to lock the cell formats collection earlier becuase it can't be modified once the 
			// InitializeCellFormats method is called.
			//// MD 3/13/12 - 12.1 - Table Support
			//// After everything has a chance to initialize, freeze the cell formats collection and let it prepare for saving.
			//this.Workbook.CellFormats.OnSaving();

			this.SaveWorkbookContents( hasShapes );

			// Make sure we balanced our push and pop calls to the context stack
			Debug.Assert( this.ContextStack.Current == null );

			// MD 3/30/11 - TFS69969
			this.Workbook.OnSavedOrLoaded(this);
			}
			// MD 2/1/12 - TFS100573
			finally
			{
				// Let the string table clean up and allow modifications again.
				this.Workbook.SharedStringTable.OnSaved();

				// MD 3/13/12 - 12.1 - Table Support
				// Let the cells formats collection allow for modifications again.
				this.Workbook.CellFormats.OnSaved();
			}
		}

		#endregion Save

		#region WriteWorksheetRecords



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public void WriteWorksheetRecords( Worksheet worksheet, bool hasShapes )
		{
			// Push on the appropriate data just in case any record need that information
			this.ContextStack.Push( worksheet.DisplayOptions );
			this.ContextStack.Push( worksheet.PrintOptions );
			this.ContextStack.Push( worksheet );

			// MD 4/28/11 - TFS62775
			this.currentWorksheetHasShapes = hasShapes;

			this.WriteWorksheet( worksheet, hasShapes );

			// MD 4/28/11 - TFS62775
			this.currentWorksheetHasShapes = false;

			this.ContextStack.Pop(); // Worksheet
			this.ContextStack.Pop(); // Print options
			this.ContextStack.Pop(); // Display options

			// MD 7/20/2007 - BR25039
			// We need to cleanup so references after serializing the worksheet
			worksheet.CleanupAfterSerialization();
		}

		#endregion WriteWorksheetRecords

		// MD 2/1/12 - TFS100573
		// This is no longer needed. The WorksheetCellBlock will return the StringElementIndex which refers to the label value when we ask 
		// for the serializable cell value.
		#region Removed

		//        #region TryGetLabelValue

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the FormattedString which represents the specified value and returns True if the value should
		//        /// be represented by a formatted string.
		//        /// </summary>  
		//#endif
		//        // MD 5/2/08 - BR32461/BR01870
		//        // Added a Workbook parameter
		//        //public static bool TryGetLabelValue( object value, out FormattedString equivalentString )
		//        // MD 11/3/10 - TFS49093
		//        // The formatted string data is now stored on the FormattedStringElement.
		//        //public static bool TryGetLabelValue( object value, Workbook associatedWorkbook, out FormattedString equivalentString )
		//        public static bool TryGetLabelValue(object value, Workbook associatedWorkbook, out StringElement equivalentString)
		//        {
		//            // MD 4/12/11 - TFS67084
		//            // We always assign this now below, so we don't need to initialize it to null.
		//            //equivalentString = null;

		//            // MD 11/3/10 - TFS49093
		//            // We don't need the try...finally anymore.
		//            //// MD 5/2/08 - BR32461/BR01870
		//            //// Wrapped everything in a try...finally so we can remove carriage returns if needed
		//            //try
		//            //{
		//                // If the value is already a formatted string, just return it
		//                // MD 11/3/10 - TFS49093
		//                // The formatted string data is now stored on the FormattedStringElement.
		//                //equivalentString = value as FormattedString;
		//                //
		//                //if ( equivalentString != null )
		//                //    return true;
		//                // MD 4/12/11 - TFS67084
		//                // Removed the FormattedStringProxy type. Elements can be held directly.
		//                //FormattedStringProxy proxy = Utilities.GetFormattedStringProxy(value);
		//                //
		//                //if (proxy != null)
		//                //{
		//                //    equivalentString = proxy.Element;
		//                //    return true;
		//                //}
		//                equivalentString = Utilities.GetFormattedStringElement(value);
		//                if (equivalentString != null)
		//                    return true;

		//                // If the value is null, it cannot be converted to a label
		//                if ( value == null )
		//                    return false;

		//                // MD 11/3/10 - TFS49093
		//                // These are now stored as FormattedStringValueReferences, except the StringBuilder, which is saved manually.
		//                //// If the value should be converted to a formatted string, return the equivalent formatted string
		//                //if ( value is string ||
		//                //    value is char ||
		//                //    value is StringBuilder || 
		//                //    value is Guid ||			// MD 5/28/10 - TFS31886
		//                //    value.GetType().IsEnum )
		//                //{
		//                //    // MD 11/3/10 - TFS49093
		//                //    // The formatted string data is now stored on the FormattedStringElement.
		//                //    //equivalentString = new FormattedString( value.ToString() );
		//                //    equivalentString = new FormattedStringElement(associatedWorkbook, value.ToString());
		//                //    return true;
		//                //}

		//                // MRS 10/1/2008 - TFS8377
		//                // If the value is a DateTime and fails to convert to an OADate, 
		//                // then we need to store it as a string (as opposed to raising an 
		//                // exception).
		//                if (value is DateTime)
		//                {
		//                    try
		//                    {
		//                        // MD 11/5/10
		//                        // Found while fixing TFS49093.
		//                        // If the value doesn't convert to an ExcelDatTime, it may just return null.
		//                        //ExcelCalcValue.DateTimeToExcelDate(associatedWorkbook, (DateTime)value);
		//                        if (ExcelCalcValue.DateTimeToExcelDate(associatedWorkbook, (DateTime)value).HasValue == false)
		//                        {
		//                            Utilities.DebugFail("We should never get it here. We should have determined this was an invalid date when it was set on the cell.");
		//                            equivalentString = new StringElement(associatedWorkbook, value.ToString());
		//                            return true;
		//                        }
		//                    }
		//                    catch (OverflowException)
		//                    {
		//                        // MD 11/3/10 - TFS49093
		//                        // The formatted string data is now stored on the FormattedStringElement.
		//                        //equivalentString = new FormattedString(value.ToString());
		//                        equivalentString = new StringElement(associatedWorkbook, value.ToString());
		//                        return true;
		//                    }
		//                }

		//                return false;

		//            // MD 11/3/10 - TFS49093
		//            // We don't need the try...finally anymore.
		//            //}
		//            //finally
		//            //{
		//            //    if ( equivalentString != null && associatedWorkbook.ShouldRemoveCarriageReturnsOnSave )
		//            //        equivalentString = equivalentString.RemoveCarriageReturns();
		//            //}
		//        }

		//        #endregion TryGetLabelValue

		#endregion // Removed

		#endregion Public Methods

		#region Protected Methods

		#region Dispose

		protected virtual void Dispose( bool disposing )
		{

		}

		#endregion Dispose

		#region WriteCellRecord( WorksheetCell )







		// MD 10/19/07
		// Found while fixing BR27421
		// Changed the return type to support saving out multiple cell value records
		//private void WriteCellRecord( WorksheetCell cell )
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//protected int WriteCellRecord( WorksheetCell cell )
		protected int WriteCellRecord(CellContext cellContext, CellDataContext cellDataContext)
		{
			// MD 4/18/11 - TFS62026
			// Update the columnIndex of the cell context object which we are reusing for all cells being written out.
			cellContext.ColumnIndex = cellDataContext.ColumnIndex;

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// We need to get the CellDataContext in the CellValueRecordBase.Save method.
			this.ContextStack.Push(cellDataContext);

			// The cell is now the current context of what is being saved
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//this.ContextStack.Push( cell );
			// MD 4/18/11 - TFS62026
			// We don't need to push the column index on the context stack anymore since the CellContext contains the column index.
			//this.ContextStack.Push((ColumnIndex)columnIndexData.columnIndex);

			// MD 10/19/07
			// Found while fixing BR27421
			// Moved all code to a centralized method to support multiple cell value records
			//WorksheetMergedCellsRegion mergedRegion = cell.AssociatedMergedCellsRegion;
			//
			//// Only save the cell's value if it is not part of a merged cell region or it is the top-left 
			//// cell of its merged cell region, otherwise, just save its format
			//if ( mergedRegion == null ||
			//    ( cell.ColumnIndex == mergedRegion.FirstColumn && cell.RowIndex == mergedRegion.FirstRow ) )
			//{
			//    // Save the cell and its value
			//    this.WriteCellRecord( cell, cell.Value );
			//}
			//else
			//{
			//    // Save the cell only, not the value
			//    this.WriteCellRecord( cell, null );
			//}
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//int ignoreUpToIndex = this.WriteCellRecord( cell, WorkbookSerializationManager.GetSerializableCellValue( cell ) );
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//int ignoreUpToIndex = this.WriteCellRecord(cell, WorkbookSerializationManager.GetSerializableCellValue(cell.Row, cell.ColumnIndexInternal));
			// MD 4/18/11 - TFS62026
			//int ignoreUpToIndex = this.WriteCellRecord(row, columnIndex, WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex));
			int ignoreUpToIndex = this.WriteCellRecord(cellContext,
				// MD 2/1/12 - TFS100573
				// This is now an instance method.
				//WorkbookSerializationManager.GetSerializableCellValue(cellContext.Row, cellDataContext.ColumnIndex, cellDataContext.CellBlock));
				this.GetSerializableCellValue(cellContext.Row, cellDataContext.ColumnIndex, cellDataContext.CellBlock));

			// MD 4/18/11 - TFS62026
			// We don't need to push the column index on the context stack anymore since the CellContext contains the column index.
			//this.ContextStack.Pop(); // columnIndex

			// MD 1/10/12 - 12.1 - Cell Format Updates
			this.ContextStack.Pop(); // cellDataContext

			// MD 10/19/07
			// Found while fixing BR27421
			// Return the new return value from the WriteCellRecord call
			return ignoreUpToIndex;
		}

		#endregion WriteCellRecord( WorksheetCell )

		#region WriteColumnRecords







		protected void WriteColumnRecords( Worksheet worksheet )
		{
            // MBS 7/30/08 - Excel 2007 Format
            // Refactored and altered this into a helper method, since we need to be able to serialize
            // each of the column blocks as a child element for the 2007 format, and so need to 
            // push a list onto the context stack
            //
            #region Refactored

            //// If no columns were accessed in the worksheet, nothing needs to be saved
            //if ( worksheet.HasColumns == false )
            //    return;

            //// We will be keeping track of adjacent columns with the exact same data, each block of 
            //// identical columns saves as one COLINFO record
            //ColumnBlockInfo currentColumnInfo = null;

            //foreach ( WorksheetColumn column in worksheet.Columns )
            //{
            //    // If the column has default data, we don't need to save a record for it
            //    if ( column.HasData == false )
            //        continue;

            //    // If there is no current block, initialize a new block with the current column
            //    if ( currentColumnInfo == null )
            //    {
            //        currentColumnInfo = new ColumnBlockInfo( column );
            //        continue;
            //    }

            //    // Try to add the next column to the current column info block. It will only be added if it is 
            //    // adjacent to the last column in the block and it has the same data as that column.
            //    if ( currentColumnInfo.TryAddColumn( column ) == false )
            //    {
            //        // If the current column could not be added to the current block, save the block
            //        this.ContextStack.Push( currentColumnInfo );
            //        this.WriteColumnGroup();
            //        this.ContextStack.Pop(); // currentColumnInfo

            //        // Start the next column block with the current column
            //        currentColumnInfo = new ColumnBlockInfo( column );
            //    }
            //}

            //// Save the last column block
            //if ( currentColumnInfo != null )
            //{
            //    this.ContextStack.Push( currentColumnInfo );
            //    this.WriteColumnGroup();
            //    this.ContextStack.Pop(); // currentColumnInfo
            //}

            #endregion //Refactored
            //
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// GetColumnBlocks is now an instance method. 
            //List<ColumnBlockInfo> columnBlocks = WorkbookSerializationManager.GetColumnBlocks(worksheet);
			// MD 3/15/12 - TFS104581
			//List<ColumnBlockInfo> columnBlocks = this.GetColumnBlocks(worksheet);
			List<WorksheetColumnBlock> columnBlocks = new List<WorksheetColumnBlock>(worksheet.ColumnBlocks.Values);

			// MD 3/15/12 - TFS104581
            //if (columnBlocks != null)
			if (columnBlocks.Count > 1 || columnBlocks[0].IsEmpty == false)
            {
				// MD 3/15/12 - TFS104581
                //foreach (ColumnBlockInfo info in columnBlocks)
				foreach (WorksheetColumnBlock info in columnBlocks)
                {
					// MD 3/22/12
					// Found while fixing TFS104630
					// Don't write out the default column blocks.
					if (info.IsEmpty)
						continue;

                    this.ContextStack.Push(info);
                    this.WriteColumnGroup();
                    this.ContextStack.Pop();
                }
            }
        }

		#endregion WriteColumnRecords

		#region WriteCustomViewOptions






		protected void WriteCustomViewOptions( Worksheet worksheet )
		{
			if ( this.Workbook.HasCustomViews == false )
				return;

			// Write out the data for the specified worksheet for each custom view
			foreach ( CustomView customView in this.Workbook.CustomViews )
			{
				if ( customView.SavePrintOptions )
				{
					this.ContextStack.Push( customView.GetPrintOptions( worksheet ) );
					Debug.Assert( this.ContextStack.Current != null );
				}

				// Push the display options on the context stack (they should be valid)s
				this.ContextStack.Push( customView.GetDisplayOptions( worksheet ) );
				Debug.Assert( this.ContextStack.Current != null, "The display options could not be obtained for the worksheet." );

				this.ContextStack.Push( customView );

				this.WriteCustomViewWorksheetData( customView.SavePrintOptions );

				this.ContextStack.Pop(); // cutsom view
				this.ContextStack.Pop(); // display options

				if ( customView.SavePrintOptions )
					this.ContextStack.Pop(); // print options
			}
		}

		#endregion WriteCustomViewOptions

		#region WriteCustomViews






		protected void WriteCustomViews()
		{
			if ( this.Workbook.HasCustomViews == false )
				return;

			foreach ( CustomView customView in this.Workbook.CustomViews )
			{
				this.ContextStack.Push( customView );
				this.WriteCustomViewWorkbookData();
				this.ContextStack.Pop();
			}
		}

		#endregion WriteCustomViews

		#region WriteMagnificationLevels






		protected void WriteMagnificationLevels( Worksheet worksheet )
		{
			// MD 7/23/12 - TFS117430
			// The rules for writing out the SCL record was incorrect here. It needs to be written out whenever the current magnification 
			// level is anything other than 100, not anything other than the default value for that view.
			//bool writeSCLRecord = false;
			//
			//if ( worksheet.DisplayOptions.View == WorksheetView.Normal )
			//    writeSCLRecord = worksheet.DisplayOptions.MagnificationInNormalView != DisplayOptions.DefaultMagnificationInNormalView;
			//else if ( worksheet.DisplayOptions.View == WorksheetView.PageBreakPreview )
			//    writeSCLRecord = worksheet.DisplayOptions.MagnificationInPageBreakView != DisplayOptions.DefaultMagnificationInPageBreakView;
			//else
			//    writeSCLRecord = worksheet.DisplayOptions.MagnificationInPageLayoutView != DisplayOptions.DefaultMagnificationInPageLayoutView;
			//
			//if ( writeSCLRecord )
			//    this.WriteMagnificationLevel();
			if (worksheet.DisplayOptions.CurrentMagnificationLevel != 100)
				this.WriteMagnificationLevel();
		}

		#endregion WriteMagnificationLevels

		#region WriteNamedReferences






		protected void WriteNamedReferences()
		{
			foreach ( NamedReferenceBase namedReferenceBase in this.Workbook.CurrentWorkbookReference.NamedReferences )
			{
				Debug.Assert(namedReferenceBase is NamedReference || namedReferenceBase is NamedReferenceUnconnected, "This is an unexpected type.");

				this.ContextStack.Push(namedReferenceBase);
				this.WriteNamedReference(namedReferenceBase.Comment != null && namedReferenceBase.Comment.Length > 0);
				this.ContextStack.Pop(); // namedReference
			}
		}

		#endregion WriteNamedReferences

		#region WritePaneInformation






		protected void WritePaneInformation( Worksheet worksheet )
		{
			bool writePaneRecord;
			if ( worksheet.DisplayOptions.PanesAreFrozen )
			{
				writePaneRecord =
					worksheet.DisplayOptions.FrozenPaneSettings.FrozenColumns != 0 ||
					worksheet.DisplayOptions.FrozenPaneSettings.FrozenRows != 0;
			}
			else
			{
				writePaneRecord =
					worksheet.DisplayOptions.UnfrozenPaneSettings.LeftPaneWidth != 0 ||
					worksheet.DisplayOptions.UnfrozenPaneSettings.TopPaneHeight != 0;
			}

			if ( writePaneRecord )
				this.WritePaneInformation();
		}

		#endregion WritePaneInformation

		// MD 11/29/11 - TFS96205
		// This is no longer needed.
		#region Removed

		//        #region WriteWorkbookCellFormats

		//#if DEBUG
		//        /// <summary>
		//        /// Saves an XF record for each resolved cell format in the workbook.
		//        /// </summary>  
		//#endif
		//        protected void WriteWorkbookCellFormats()
		//        {
		//            foreach (WorksheetCellFormatData format in this.Formats)
		//            {
		//                this.ContextStack.Push(format);
		//                this.WriteWorkbookCellFormat();
		//                this.ContextStack.Pop(); // format
		//            }
		//        }

		//        #endregion  // Removed

		#endregion  // Removed

		#region WriteWorkbookFonts






		protected void WriteWorkbookFonts()
		{
			foreach ( WorkbookFontData font in this.Fonts )
			{
				if ( font == null )
					continue;

				this.ContextStack.Push( font );
				this.WriteWorkbookFont();
				this.ContextStack.Pop(); // font
			}
		}

		#endregion WriteWorkbookFonts

		#region WriteWorkbookGlobals

		/// <summary>
		/// Writes the global records that define the workbook data (not the worksheets).
		/// </summary>
		/// <param name="hasShapes">True if any workshete in the workbookhas shapes; False otherwise</param>
		protected void WriteWorkbookGlobals( bool hasShapes )
		{
			this.ContextStack.Push( this.Workbook );
			this.WriteWorkbookGlobalData( hasShapes );
			this.ContextStack.Pop(); // workbook
		}

		#endregion WriteWorkbookGlobals

		#region WriteWorkbookNumberFormats






		protected void WriteWorkbookNumberFormats()
		{
			// MD 10/19/07
			// Found while fixing BR27421
			// Moved these blocks into a helper method to prevent code repetition
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 5, "\"$\"#,##0_);\\(\"$\"#,##0\\)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			//
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 6, "\"$\"#,##0_);[Red]\\(\"$\"#,##0\\)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			//
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 7, "\"$\"#,##0.00_);\\(\"$\"#,##0.00\\)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			//
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 8, "\"$\"#,##0.00_);[Red]\\(\"$\"#,##0.00\\)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			//
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 42, "_(\"$\"* #,##0_);_(\"$\"* \\(#,##0\\);_(\"$\"* \"-\"_);_(@_)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			//
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 41, "_(* #,##0_);_(* \\(#,##0\\);_(* \"-\"_);_(@_)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			//
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 44, "_(\"$\"* #,##0.00_);_(\"$\"* \\(#,##0.00\\);_(\"$\"* \"-\"??_);_(@_)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			//
			//this.ContextStack.Push( new FORMATRecord.FORMATInfo( 43, "_(* #,##0.00_);_(* \\(#,##0.00\\);_(* \"-\"??_);_(@_)" ) );
			//this.WriteRecord( BIFFType.FORMAT );
			//this.ContextStack.Pop();
			this.WriteStandardFormat( 5 );
			this.WriteStandardFormat( 6 );
			this.WriteStandardFormat( 7 );
			this.WriteStandardFormat( 8 );
			this.WriteStandardFormat( 42 );
			this.WriteStandardFormat( 41 );
			this.WriteStandardFormat( 44 );
			this.WriteStandardFormat( 43 );

			if ( this.Workbook.HasFormats )
			{
				// MD 9/9/08 - TFS 6913
				// The number formats don't have to necessarily be right at the custom table start or in contiguous order. Instead, just iterate
				// the actual indices which have been added.
				//for ( int i = 0; i < this.workbook.Formats.Count; i++ )
				//{
				//    // MD 10/19/07
				//    // Found while fixing BR27421
				//    // Use the helper method created for above
				//    //int formatIndex = i + WorkbookFormatCollection.FormatTableOffset;
				//    //
				//    //this.ContextStack.Push( new FORMATRecord.FORMATInfo( formatIndex, this.workbook.Formats[ formatIndex ] ) );
				//    //this.WriteRecord( BIFFType.FORMAT );
				//    //this.ContextStack.Pop(); // format info
				//    this.WriteStandardFormat( i + WorkbookFormatCollection.FormatTableOffset );
				//}
				foreach ( int index in this.workbook.Formats.GetCustomFormatIndices() )
					this.WriteStandardFormat( index );
			}
		}

		#endregion WriteWorkbookNumberFormats

		#region WriteWorkbookStyles






		protected void WriteWorkbookStyles()
		{
			// MD 11/12/07 - BR27987
			// Use the Styles collection of the serialization manager, not the workbook
			//if ( workbook.HasStyles == false )
			if ( this.Styles.Count == 0 )
				return;

			// MD 11/12/07 - BR27987
			// Use the Styles collection of the serialization manager, not the workbook
			//foreach ( WorkbookStyle style in workbook.Styles )
			foreach ( WorkbookStyle style in this.Styles )
			{
				this.ContextStack.Push( style );
				this.WriteWorkbookStyle();
				this.ContextStack.Pop(); // style
			}
		}

		#endregion WriteWorkbookStyles

		// MD 7/20/2007 - BR25039
		// Added helper to write cell comments
		#region WriteWorksheetCellComments






		protected void WriteWorksheetCellComments( Worksheet worksheet )
		{
			if ( worksheet.HasCommentShapes )
			{
				// MD 9/2/08 - Cell Comments
				//foreach ( WorksheetCellCommentShape comment in worksheet.CommentShapes )
				foreach ( WorksheetCellComment comment in worksheet.CommentShapes )
				{
					this.ContextStack.Push( comment.Cell );
					this.WriteWorksheetCellComment();
					this.ContextStack.Pop(); // note.Cell
				}
			}
		}

		#endregion WriteWorksheetCellComments

		#endregion Protected Methods

		#region Internal Methods

		// MD 10/19/07
		// Found while fixing BR27421
		// Moved the code from both WriteCellRecord overloads which converts the cell value to a 
		// serializable value to this method.
		#region GetSerializableCellValue



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal static object GetSerializableCellValue( WorksheetCell cell )
		//{
		//    // MD 9/2/08 - Excel formula solving
		//    // Moved all code to the new overload
		//    return WorkbookSerializationManager.GetSerializableCellValue( cell, false );
		//}
		// MD 2/1/12 - TFS100573
		// This is now an instance method.
		//internal static object GetSerializableCellValue(WorksheetRow row, short columnIndex)
		//{
		//    return WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex, null, false, false);
		//}
		internal object GetSerializableCellValue(WorksheetRow row, short columnIndex)
		{
			return this.GetSerializableCellValue(row, columnIndex, null, false, false);
		}

		// MD 2/1/12 - TFS100573
		// This is now an instance method.
		//internal static object GetSerializableCellValue(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock)
		//{
		//    return WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex, cellBlock, true, false);
		//}
		internal object GetSerializableCellValue(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock)
		{
			return this.GetSerializableCellValue(row, columnIndex, cellBlock, true, false);
		}

		// MD 9/2/08 - Excel formula solving
		// Added overload where you can get the serializable calculated cell value for cells with formulas.


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal static object GetSerializableCellValue( WorksheetCell cell, bool ignoreFormulas )
		// MD 2/1/12 - TFS100573
		// This is now an instance method.
		//internal static object GetSerializableCellValue(WorksheetRow row, short columnIndex, bool ignoreFormulas)
		//{
		//    return WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex, null, false, ignoreFormulas);
		//}
		internal object GetSerializableCellValue(WorksheetRow row, short columnIndex, bool ignoreFormulas)
		{
			return this.GetSerializableCellValue(row, columnIndex, null, false, ignoreFormulas);
		}

		// MD 2/1/12 - TFS100573
		// This is now an instance method.
		//internal static object GetSerializableCellValue(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock, bool ignoreFormulas)
		//{
		//    return WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex, cellBlock, true, ignoreFormulas);
		//}
		internal object GetSerializableCellValue(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock, bool ignoreFormulas)
		{
			return this.GetSerializableCellValue(row, columnIndex, cellBlock, true, ignoreFormulas);
		}

		// MD 2/1/12 - TFS100573
		// This is now an instance method.
		//internal static object GetSerializableCellValue(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock, bool isCellBlockValid, bool ignoreFormulas)
		internal object GetSerializableCellValue(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock, bool isCellBlockValid, bool ignoreFormulas)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetMergedCellsRegion mergedRegion = cell.AssociatedMergedCellsRegion;
			WorksheetMergedCellsRegion mergedRegion = row.GetCellAssociatedMergedCellsRegionInternal(columnIndex);

			// Only save the cell's value if it is not part of a merged cell region or it is the top-left 
			// cell of its merged cell region, otherwise, just save its format
			// MD 8/20/08
			// Found while implementing Excel formula solving
			// This can be simplified with the new TopLeftCell property.
			//if ( mergedRegion == null ||
			//    ( cell.ColumnIndex == mergedRegion.FirstColumn && cell.RowIndex == mergedRegion.FirstRow ) )
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ( mergedRegion == null || cell == mergedRegion.TopLeftCell )
			if (mergedRegion == null || 
				(row.Index == mergedRegion.FirstRow && columnIndex == mergedRegion.FirstColumnInternal))
			{
				// MD 10/20/10 - TFS36617
				// This code can be simplified now that the cell keep status bits.
				#region Old Code

				//// MD 7/2/09 - TFS18634
				//// Cache these values so we don't have to get them multiple times.
				//object value = cell.ValueInternal;
				//Formula formula = value as Formula;
				//WorksheetDataTable dataTable = value as WorksheetDataTable;
				//
				//// MD 7/14/08 - Excel formula solving
				//// The Value property no longer returns the Formula instance on the cell.
				////object value = cell.Value;
				//// MD 9/2/08 - Excel formula solving
				//// Refactored to honor the new ignoreFormulas parameters
				////object value = cell.Formula;
				////
				////if ( value == null )
				////{
				////    // MD 7/21/08 - Excel formula solving
				////    // The Value property no longer returns the data table instance on the cell.
				////    value = cell.AssociatedDataTable;
				////
				////    if ( value == null )
				////        value = cell.Value;
				////}
				//if ( ignoreFormulas == false )
				//{
				//    // MD 7/2/09 - TFS18634
				//    // This value is cached above.
				//    //Formula formula = cell.Formula;				
				//
				//    if ( formula != null )
				//        return formula;
				//
				//    // MD 7/2/09 - TFS18634
				//    // This value is cached above.
				//    //WorksheetDataTable dataTable = cell.AssociatedDataTable;			
				//
				//    if ( dataTable != null )
				//        return dataTable;
				//}
				//// MD 7/2/09 - TFS18634
				//// If we should ignore formula values and the value was a formula, use the calculated value instead.
				//else if ( formula != null || dataTable != null )
				//{
				//    // If the value is a formula and we should not ignored formulas, get the calculated value instead.
				//    value = cell.CalculatedValue;
				//} 

				#endregion // Old Code
				object value;

				// If the cell uses calculations, it has a formula or data table
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//if (cell.UsesCalculations)
				//{
				//    // If we should return formula values and not ignore them, return the formula or data table instance.
				//    if (ignoreFormulas == false)
				//        return cell.ValueInternal;
				//
				//    // If we should ignore formulas and serialized the calculated value, use that instead.
				//    value = cell.CalculatedValue;
				//}
				//else
				//{
				//    // In all other cases, the value is the internal value.
				//    value = cell.ValueInternal;
				//}
				if (isCellBlockValid == false)
					row.TryGetCellBlock(columnIndex, out cellBlock);

				if (cellBlock != null)
				{
					if (cellBlock.DoesCellUseCalculations(columnIndex))
					{
						// If we should return formula values and not ignore them, return the formula or data table instance.
						if (ignoreFormulas == false)
						{
							// MD 2/1/12 - TFS100573
							// Pass along the manager so it know to get the SST index for strings instead of the actual string value.
							//return cellBlock.GetCellValueInternal(row, columnIndex);
							return cellBlock.GetCellValueRaw(row, columnIndex, this);
						}

						// If we should ignore formulas and serialized the calculated value, use that instead.
						value = cellBlock.GetCalculatedValue(row, columnIndex);
					}
					else
					{
						// In all other cases, the value is the internal value.
						// MD 2/1/12 - TFS100573
						// Pass along the manager so it know to get the SST index for strings instead of the actual string value.
						//value = cellBlock.GetCellValueInternal(row, columnIndex);
						value = cellBlock.GetCellValueRaw(row, columnIndex, this);
					}
				}
				else
				{
					value = null;
				}

				// MD 2/9/12 - TFS101326
				// This was erroneously put in the GetCellValueInternal overload called above byu the fix for TFS100573, but the circularity error is 
				// returned by GetCalculatedValue, so I moved this back here.
				//
				// The Circularity error should be serialized as a 0 in the workbook file.
				ErrorValue errorValue = value as ErrorValue;
				if (errorValue == ErrorValue.Circularity)
					return 0d;

				// MD 7/2/09 - TFS18634
				// This value is cached above.
				//object value = cell.Value;

                // MRS 10/1/2008 - TFS8377
                // I moved this code down below. We need to try TryGetLabelValue before we check 
                // for DateTimes. 
                #region Moved down
                //if ( value is DateTime )
                //{
                //    // MD 10/24/07 - BR27751
                //    // Use a helper method: We may need to coerce the value returned from ToOADate to get around a bug in excel.
                //    //return ( (DateTime)value ).ToOADate();
                //    // 8/8/08 - Excel formula solving
                //    // This utility method has been moved
                //    //return Utilities.DateTimeToExcelDate( (DateTime)value );
                //    return ExcelCalcValue.DateTimeToExcelDate( cell.Worksheet.Workbook, (DateTime)value );
                //}
                #endregion //Moved down

				// MD 2/1/12 - TFS100573
				// This is no longer needed. The WorksheetCellBlock will return the StringElementIndex which refers to the label value when we ask 
				// for the serializable cell value.
				#region Removed

				//// MD 11/3/10 - TFS49093
				//// The formatted string data is now stored on the FormattedStringElement.
				////FormattedString formattedString;
				//StringElement formattedString;

				//// MD 5/2/08 - BR32461/BR01870
				//// Added a workbook parameter to the TryGetLabelValue method
				////if ( WorkbookSerializationManager.TryGetLabelValue( value, out formattedString ) )
				//// MD 7/26/10 - TFS34398
				//// Now that the row is stored on the cell, the Worksheet getter is now a bit slower, so cache it.
				////if ( WorkbookSerializationManager.TryGetLabelValue( value, cell.Worksheet.Workbook, out formattedString ) )
				//// MD 4/12/11 - TFS67084
				//// Moved away from using WorksheetCell objects.
				////Worksheet worksheet = cell.Worksheet;
				//Worksheet worksheet = row.Worksheet;
				//if (WorkbookSerializationManager.TryGetLabelValue(value, worksheet.Workbook, out formattedString))
				//    return formattedString;

				#endregion // Removed

				// MD 2/1/12 - TFS100573
				// This logic is now moved to the WorksheetCellBlock.GetCellValueInternal and is used when a manager for saving is passed in, as it
				// is in this method when called above.
				#region Removed

				//// MRS 10/1/2008 - TFS8377
				//// I moved this code down from above. Checking for dates has to be done after
				//// the call to TryGetLabelValue.
				////
				//if (value is DateTime)
				//{
				//    // MD 10/24/07 - BR27751
				//    // Use a helper method: We may need to coerce the value returned from ToOADate to get around a bug in excel.
				//    //return ( (DateTime)value ).ToOADate();
				//    // 8/8/08 - Excel formula solving
				//    // This utility method has been moved
				//    //return Utilities.DateTimeToExcelDate( (DateTime)value );
				//    // MD 7/26/10 - TFS34398
				//    // Use the cached worksheet.
				//    //return ExcelCalcValue.DateTimeToExcelDate(cell.Worksheet.Workbook, (DateTime)value);
				//    // MD 9/16/11 - TFS87857
				//    // We may get a null value back from DateTimeToExcelDate. If so, return the string representation of the date instead.
				//    //return ExcelCalcValue.DateTimeToExcelDate(worksheet.Workbook, (DateTime)value);
				//    DateTime dateTime = (DateTime)value;

				//    // MD 2/1/12 - TFS100573
				//    // We no longer get the worksheet, so use the manager's workbook instead, which is the same workbook anyway.
				//    //double? excelDate = ExcelCalcValue.DateTimeToExcelDate(worksheet.Workbook, dateTime);
				//    double? excelDate = ExcelCalcValue.DateTimeToExcelDate(this.Workbook, dateTime);

				//    if (excelDate.HasValue)
				//        return excelDate;

				//    Utilities.DebugFail("We should never get into this situation.");
				//    return dateTime.ToString();
				//}

				//// MD 8/20/08 - Excel formula solving
				//ErrorValue errorValue = value as ErrorValue;

				//// The Circularity error should be serialized as a 0 in the workbook file.
				//if ( errorValue == ErrorValue.Circularity )
				//    return 0d;

				#endregion // Removed

				return value;
			}
			else
			{
				return null;
			}
		}

		#endregion GetSerializableCellValue

		// MD 10/19/07
		// Found while fixing BR27421
		// Moved code from WriteCellRecord which determines whether a value is an RK value
		#region IsRKValue

		internal static bool IsRKValue( object value, out uint rkValue )
		{
			rkValue = 0;

			// All numbers save with either an RK or NUMBER record
			// MD 4/3/12 - TFS107243
			// Moved these checks to a helper method.
			//if ( value is sbyte ||
			//    value is byte ||
			//    value is short ||
			//    value is ushort ||
			//    value is int ||
			//    value is uint ||
			//    value is long ||
			//    value is ulong ||
			//    value is float ||
			//    value is double ||
			//    value is decimal )
			if (Utilities.IsNumber(value))
			{
				// All numbers are doubles in excel
				// MD 4/6/12 - TFS101506
				//double number = Convert.ToDouble( value, CultureInfo.CurrentCulture );
				double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);

				if ( Utilities.TryEncodeRKValue( number, out rkValue ) )
					return true;
			}

			return false;
		}

		#endregion IsRKValue

		// MD 10/19/07
		// Found while fixing BR27421
		// Moved code from WriteCellRecord which determines whether a value is blank
		#region IsValueBlank

		internal static bool IsValueBlank( object value )
		{
			return value == null || value is DBNull;
		}

		#endregion IsValueBlank

		#endregion Internal Methods

		#region Private Methods

		// MD 9/2/08 - Excel2007 format
		// Moved from NAMERecord
		#region ExtractHiddenColumns

		private static void ExtractHiddenColumns( Worksheet worksheet, CustomView customView, Formula formula )
		{
			HiddenColumnCollection hiddenColumns = customView.GetHiddenColumns( worksheet );

			if ( hiddenColumns == null )
			{
				Utilities.DebugFail( "Couldn't get the hidden columns collection." );
				return;
			}

			// MD 5/25/11 - Data Validations / Page Breaks
			// Moved this code to a helper method.
			//foreach ( FormulaToken token in formula.PostfixTokenList )
			//{
			//    if ( token is MemOperatorBase )
			//        continue;
			//
			//    if ( token is UnionOperator )
			//        continue;
			//
			//    Area3DToken area = token as Area3DToken;
			//
			//    if ( area == null )
			//    {
			//        Utilities.DebugFail( "Invalid formula token." );
			//        continue;
			//    }
			//
			//    CellAddressRange range = area.CellAddressRange;
			//
			//    for ( int columnIndex = range.TopLeftCellAddress.Column; columnIndex <= range.BottomRightCellAddress.Column; columnIndex++ )
			//        hiddenColumns.Add( worksheet.Columns[ columnIndex ] );
			//}
			List<CellAddressRange> ranges = Utilities.GetRangesFromFormula(formula);

			for (int i = 0; i < ranges.Count; i++)
			{
				CellAddressRange range = ranges[i];

				for (int columnIndex = range.TopLeftCellAddress.Column; columnIndex <= range.BottomRightCellAddress.Column; columnIndex++)
					hiddenColumns.Add(worksheet.Columns[columnIndex]);
			}
		}

		#endregion ExtractHiddenColumns

		// MD 9/2/08 - Excel2007 format
		// Moved from NAMERecord
		#region ExtractHiddenRows

		private static void ExtractHiddenRows( Worksheet worksheet, CustomView customView, Formula formula )
		{
			HiddenRowCollection hiddenRows = customView.GetHiddenRows( worksheet );

			if ( hiddenRows == null )
			{
				Utilities.DebugFail( "Couldn't get the hidden rows collection." );
				return;
			}

			// MD 5/25/11 - Data Validations / Page Breaks
			// Moved this code to a helper method.
			//foreach ( FormulaToken token in formula.PostfixTokenList )
			//{
			//    if ( token is MemOperatorBase )
			//        continue;
			//
			//    if ( token is UnionOperator )
			//        continue;
			//
			//    Area3DToken area = token as Area3DToken;
			//
			//    if ( area == null )
			//    {
			//        Utilities.DebugFail( "Invalid formula token." );
			//        continue;
			//    }
			//
			//    CellAddressRange range = area.CellAddressRange;
			//
			//    for ( int rowIndex = range.TopLeftCellAddress.Row; rowIndex <= range.BottomRightCellAddress.Row; rowIndex++ )
			//        hiddenRows.Add( worksheet.Rows[ rowIndex ] );
			//}
			List<CellAddressRange> ranges = Utilities.GetRangesFromFormula(formula);

			for (int i = 0; i < ranges.Count; i++)
			{
				CellAddressRange range = ranges[i];

				for (int rowIndex = range.TopLeftCellAddress.Row; rowIndex <= range.BottomRightCellAddress.Row; rowIndex++)
					hiddenRows.Add(worksheet.Rows[rowIndex]);
			}
		}

		#endregion ExtractHiddenRows

		#region InitializeCellFormats






		protected abstract void InitializeCellFormats();

		#endregion InitializeCellFormats

		#region InitializeFonts






		protected abstract void InitializeFonts();

		#endregion InitializeFonts

		// MD 10/30/11 - TFS90733
		#region InitializeMacroNamedReferences

		private void InitializeMacroNamedReferences(WorksheetShapeCollection shapes)
		{
			foreach (WorksheetShape shape in shapes)
			{
				WorksheetShapeGroup group = shape as WorksheetShapeGroup;
				if (group != null)
					this.InitializeMacroNamedReferences(group.Shapes);

				if (shape.Obj != null &&
					shape.Obj.Macro != null)
				{
					Formula formula = shape.Obj.Macro.GetFormula();
					if (formula != null)
					{
						for (int i = 0; i < formula.PostfixTokenList.Count; i++)
						{
							NameXToken nameToken = formula.PostfixTokenList[i] as NameXToken;
							if (nameToken == null)
								continue;

							Debug.Assert(nameToken.ScopeReference == this.Workbook, "Handle the case where the NameX token is a 3D ref.");
							
							NamedReference namedRefernece = nameToken.NamedReference as NamedReference;
							Debug.Assert(namedRefernece != null, "Handle the case where the NamedReference is of ");

							if (namedRefernece != null)
								this.Workbook.CurrentWorkbookReference.AddNamedReference(namedRefernece);
						}
					}
				}
			}
		}

		#endregion // InitializeMacroNamedReferences

		#region InitializeNamedReferences






		private void InitializeNamedReferences()
		{
			// MD 6/16/12 - CalcEngineRefactor
			this.workbook.CurrentWorkbookReference.ClearNamedReferences();

			// MD 10/30/11 - TFS90733
			// Add in all macro references from shapes
			foreach (Worksheet worksheet in this.Workbook.Worksheets)
				this.InitializeMacroNamedReferences(worksheet.Shapes);

			// Add all named references from the workbook itself
			if ( this.workbook.HasNamedReferences )
			{
				foreach ( NamedReference namedReference in this.workbook.NamedReferences )
					this.Workbook.CurrentWorkbookReference.AddNamedReference( namedReference );
			}

			// MD 10/9/07 - BR27172
			// Add all hidden named references from the workbook itself
			if ( this.workbook.HasHiddenNamedReferences )
			{
				foreach ( NamedReference namedReference in this.workbook.HiddenNamedReferences )
					this.Workbook.CurrentWorkbookReference.AddNamedReference(namedReference);
			}

			// Add all hidden named references which describe special info about custom views.
			if ( this.workbook.HasCustomViews )
			{
				foreach ( CustomView customView in this.workbook.CustomViews )
				{
					// MD 5/25/11 - Data Validations / Page Breaks
					// We may also want to store print options here.
					//if ( customView.SaveHiddenRowsAndColumns == false )
					if (customView.SaveHiddenRowsAndColumns == false && customView.SavePrintOptions == false)
						continue;

					foreach ( Worksheet worksheet in this.Workbook.Worksheets )
					{
						// MD 5/25/11 - Data Validations / Page Breaks
						// Wrapped in an if statement.
						if (customView.SaveHiddenRowsAndColumns)
						{
						HiddenColumnCollection hiddenColumns = customView.GetHiddenColumns( worksheet );

						if ( hiddenColumns != null )
						{
							NamedReference reference = hiddenColumns.CreateNamedReference();

							if ( reference != null )
								this.Workbook.CurrentWorkbookReference.AddNamedReference(reference);
						}

						HiddenRowCollection hiddenRows = customView.GetHiddenRows( worksheet );

						if ( hiddenRows != null )
						{
							NamedReference reference = hiddenRows.CreateNamedReference();

							if ( reference != null )
								this.Workbook.CurrentWorkbookReference.AddNamedReference(reference);
						}
						}

						// MD 5/25/11 - Data Validations / Page Breaks
						if (customView.SavePrintOptions)
						{
							PrintOptions printOptions = customView.GetPrintOptions(worksheet);

							if (printOptions.ColumnsToRepeatAtLeft != null || printOptions.RowsToRepeatAtTop != null)
							{
								// MD 4/9/12 - TFS101506
								//string name = String.Format(
								//    CultureInfo.CurrentCulture,
								//    "Z_{0}_.wvu.PrintTitles",
								//    customView.Id.ToString("D").Replace("-", "_").ToUpper(CultureInfo.CurrentCulture));
								string name = String.Format(
									CultureInfo.InvariantCulture,
									"Z_{0}_.wvu.PrintTitles",
									customView.Id.ToString("D").Replace("-", "_").ToUpper(CultureInfo.InvariantCulture));

								NamedReference namedRef = null;
								bool shouldAdd;
								printOptions.UpdatePrintTitlesNamedReference(workbook, name, ref namedRef, out shouldAdd);

								namedRef.Hidden = true;

								Debug.Assert(shouldAdd, "Something is wrong.");
								this.Workbook.CurrentWorkbookReference.AddNamedReference(namedRef);
							}

							if (printOptions.HasPrintAreas)
							{
								// MD 4/9/12 - TFS101506
								//string name = String.Format(
								//    CultureInfo.CurrentCulture,
								//    "Z_{0}_.wvu.PrintArea",
								//    customView.Id.ToString("D").Replace("-", "_").ToUpper(CultureInfo.CurrentCulture));
								string name = String.Format(
									CultureInfo.InvariantCulture,
									"Z_{0}_.wvu.PrintArea",
									customView.Id.ToString("D").Replace("-", "_").ToUpper(CultureInfo.InvariantCulture));

								NamedReference namedRef = null;
								bool shouldAdd;
								printOptions.UpdatePrintAreasNamedReference(workbook, name, ref namedRef, out shouldAdd);

								namedRef.Hidden = true;

								Debug.Assert(shouldAdd, "Something is wrong.");
								this.Workbook.CurrentWorkbookReference.AddNamedReference(namedRef);
							}
						}
					}
				}
			}
		}

		#endregion InitializeNamedReferences

		// MD 2/28/12 - 12.1 - Table Support
		#region Removed

		//// MD 10/30/11 - TFS90733
		//#region InitializeObjReferences

		//private void InitializeObjReferences<T>(ICollection<T> shapes) where T : WorksheetShape
		//{
		//    foreach (WorksheetShape shape in shapes)
		//    {
		//        WorksheetShapeGroup group = shape as WorksheetShapeGroup;
		//        if (group != null)
		//            this.InitializeObjReferences(group.Shapes);

		//        if (shape.Obj == null)
		//            continue;

		//        if (shape.Obj.Macro != null && shape.Obj.Macro.Fmla != null)
		//            shape.Obj.Macro.Fmla.ResolveReferences(this);

		//        if (shape.Obj.LinkFmla != null && shape.Obj.LinkFmla.Fmla != null)
		//            shape.Obj.LinkFmla.Fmla.ResolveReferences(this);

		//        if (shape.Obj.List != null && shape.Obj.List.Fmla != null)
		//            shape.Obj.List.Fmla.ResolveReferences(this);

		//        if (shape.Obj.PictFmla != null)
		//        {
		//            if (shape.Obj.PictFmla.Fmla != null)
		//                shape.Obj.PictFmla.Fmla.ResolveReferences(this);

		//            if (shape.Obj.PictFmla.Key != null)
		//            {
		//                if (shape.Obj.PictFmla.Key.FmlaLinkedCell != null)
		//                    shape.Obj.PictFmla.Key.FmlaLinkedCell.ResolveReferences(this);

		//                if (shape.Obj.PictFmla.Key.FmlaListFillRange != null)
		//                    shape.Obj.PictFmla.Key.FmlaListFillRange.ResolveReferences(this);
		//            }
		//        }
		//    }
		//}

		//#endregion // InitializeObjReferences

		#endregion // Removed

		#region InitializeReferences






        // MBS 8/18/08 
		//private void InitializeReferences()
        protected virtual void InitializeReferences()
		{
			// MD 6/16/12 - CalcEngineRefactor
			this.WorkbookReferences.Add(this.Workbook.CurrentWorkbookReference);

			// Initialize the named references of the workbook before we resolve all other refernence
			this.InitializeNamedReferences();

			// MD 2/28/12 - 12.1 - Table Support
			// This code can be refactored now that we have an IterateRows helper method.
			#region Refactored

			//// MD 10/23/07
			//// Found while fixing BR27496
			//// We should not lazily create the CurrentWorkbookReference by getting it here.
			//// If we do, we will unnecessarily write out SUPBOOK and EXTERNSHEET records. 
			//// Only access the CurrentWorkbookReference if it has already been created (the code below was wrapped in the if statement).
			//if ( this.currentWorkbookReference != null )
			//{
			//    // Resolve the references used of the formula of each named reference
			//    // NOTE: Can't use a foreach here because the ResolveReferences call on the formula will add 
			//    // the unresolved named references to the end of the named references collection
			//    for ( int i = 0; i < this.CurrentWorkbookReference.NamedReferences.Count; i++ )
			//    {
			//        NamedReferenceBase namedReference = this.CurrentWorkbookReference.NamedReferences[ i ];

			//        // MD 8/20/07 - BR25818
			//        // Since the formula stores its formula type now, we don't need to know whether this is for a named reference
			//        //namedReference.FormulaInternal.ResolveReferences( this, false );
			//        // MD 10/30/11 - TFS90733
			//        // Macro references will not have a formula.
			//        //namedReference.FormulaInternal.ResolveReferences( this );
			//        if (namedReference.FormulaInternal != null)
			//        {
			//            namedReference.FormulaInternal.ResolveReferences(this, null);
			//        }
			//        else
			//        {
			//            Debug.Assert(namedReference is NamedReference && ((NamedReference)namedReference).IsMacroName,
			//                "If the formula is null, we are expecting it to be a reference to a macro.");
			//        }
			//    }
			//}

			//// Resolve the references used by the formulas of cells
			//foreach ( Worksheet worksheet in this.workbook.Worksheets )
			//{
			//    // MD 3/10/09 - TFS14741
			//    // We now need to get into the loop, even if the worksheet has no rows.
			//    //if ( worksheet.HasRows == false )
			//    //	continue;

			//    // We want to add the worksheet to the context stack because the Name formula token
			//    // needs to know what worksheet it exists on.
			//    this.ContextStack.Push( worksheet );

			//    // MD 2/1/11 - Data Validation support
			//    //// MD 3/10/09 - TFS14741
			//    //// If the are data validation formulas on the worksheet, we need to resolve their references 
			//    //// as well.
			//    //if ( worksheet.DataValidationInfo2003 != null )
			//    //{
			//    //    foreach ( DataValidationCriteria criteria in worksheet.DataValidationInfo2003.DataValidations )
			//    //    {
			//    //        if ( criteria.FormulaForFirstCondition != null )
			//    //            criteria.FormulaForFirstCondition.ResolveReferences( this );
			//    //
			//    //        if ( criteria.FormulaForSecondCondition != null )
			//    //            criteria.FormulaForSecondCondition.ResolveReferences( this );
			//    //    }
			//    //}
			//    if ( worksheet.HasDataValidationRules )
			//    {
			//        foreach (DataValidationRule rule in ((IDictionary<DataValidationRule, WorksheetReferenceCollection>)worksheet.DataValidationRules).Keys)
			//        {
			//            Formula formula1 = rule.GetFormula1(null);
			//            Formula formula2 = rule.GetFormula2(null);

			//            if (formula1 != null)
			//                formula1.ResolveReferences(this, worksheet);

			//            if (formula2 != null)
			//                formula2.ResolveReferences(this, worksheet);
			//        }
			//    }

			//    foreach ( WorksheetRow row in worksheet.Rows )
			//    {
			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //if (row.HasCells == false)
			//        //    continue;
			//        //
			//        //foreach (WorksheetCell cell in row.Cells)
			//        //{
			//        //    // MD 7/14/08 - Excel formula solving
			//        //    // The Value property no longer returns the Formula instance on the cell.
			//        //    //Formula formula = cell.Value as Formula;
			//        //    Formula formula = cell.Formula;
			//        //
			//        //    // If the cell has a formula applied to it, resolve the formula references
			//        //    if (formula != null)
			//        //    {
			//        //        // MD 8/20/07 - BR25818
			//        //        // Since the formula stores its formula type now, we don't need to know whether this is for a named reference
			//        //        //formula.ResolveReferences( this, false );
			//        //        formula.ResolveReferences(this);
			//        //    }
			//        //}
			//        foreach (Formula formula in row.CellOwnedFormulas)
			//            formula.ResolveReferences(this, worksheet);
			//    }

			//    this.ContextStack.Pop(); // worksheet
			//}

			#endregion // Refactored
			this.Workbook.IterateFormulas(this.Workbook.CurrentWorkbookReference.NamedReferences, new InitializeReferencesHelper(this).FormulaCallback);

			// MD 6/16/12 - CalcEngineRefactor
			//// If there are some external references, but no current workbook reference, we need to
			//// create the current workbook reference. Get the reference to the first worksheet of the
			//// current workbook so it is lazily created.
			//if ( this.WorkbookReferences.Count > 0 && this.currentWorkbookReference == null )
			//{
			//    // MD 8/20/07 - BR25818
			//    // There are more than two options now with regards to how requested worksheet references should be handled, 
			//    // so an enum must be used instead of a boolean
			//    //this.CurrentWorkbookReference.GetWorksheetReference( 0, false, true );
			//    this.CurrentWorkbookReference.GetWorksheetReference( 0, false, WorksheetRequestAction.AddToReferenceListIfUnique );
			//}
		}

		// MD 2/28/12 - 12.1 - Table Support
		private class InitializeReferencesHelper
		{
			private WorkbookSerializationManager _manager;

			public InitializeReferencesHelper(WorkbookSerializationManager manager)
			{
				_manager = manager;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.InitializeSerializationManager(_manager, owningWorksheet);
			}
		}

		#endregion InitializeReferences

		// MD 9/10/08 - Cell Comments
		#region IntializeShapes






		private void IntializeShapes()
		{
			uint nextShapeId = 1024;

			// MD 10/30/11 - TFS90733
			ushort nextObjId = 0;
			
			foreach ( Worksheet worksheet in this.workbook.Worksheets )
			{
				nextShapeId = (uint)Utilities.RoundUpToMultiple( (int)nextShapeId, 1024 );

				// MD 10/30/11 - TFS90733
				//worksheet.AssignShapeIds( ref nextShapeId, this.OnlyAssignShapeIdsToCommentShapes );
				worksheet.InitializeShapes(ref nextShapeId, ref nextObjId, this.OnlyAssignShapeIdsToCommentShapes);

				// MD 10/30/11 - TFS90733
				// We need to initialize the references in the Obj records as well.
				// MD 2/28/12 - 12.1 - Table Support
				//this.InitializeObjReferences(worksheet.CommentShapes);
				//this.InitializeObjReferences(worksheet.Shapes);
			}

			this.maxShapeId = nextShapeId;
		} 

		#endregion IntializeShapes

		// MD 11/3/10 - TFS49093
		#region InitializeStrings

		private void InitializeStrings()
		{
			this.sharedStringTable = new List<StringElement>();

			// MD 2/1/12 - TFS100573
			// This is no longer needed.
			//int count = 0;

			foreach (StringElement element in this.Workbook.SharedStringTable)
			{
				// MD 2/1/12 - TFS100573
				// The string index is no longer stored on the string element.
				//element.IndexInStringTable = count++;

				// MD 1/18/12 - 12.1 - Cell Format Updates
				//element.InitSerializationCache(this);
				element.InitSerializationCache(this, null);

				// MD 2/1/12 - TFS100573
				// The shared string table will now only store additional strings (for StringBuilders) during a save.
				//this.sharedStringTable.Add(element);

				this.totalStringsUsedInDocument += (uint)element.ReferenceCount;
			}
		}

		#endregion // InitializeStrings	

		// MD 9/2/08 - Excel2007 format
		// Moved from NAMERecord
		#region TryExtractCustomViewInfo

		// MD 10/9/07 - BR27172
		// Changed the return type so we can determine when this fails
		//private static void TryExtractCustomViewInfo( WorkbookSerializationManager manager, NamedReference reference )
		private static bool TryExtractCustomViewInfo( WorkbookSerializationManager manager, NamedReference reference )
		{
			Worksheet worksheet = reference.Scope as Worksheet;

			if ( worksheet == null )
			{
				// MD 10/9/07 - BR27172
				// The return type has changed
				//return;
				return false;
			}

			Match match = WorkbookSerializationManager.CustomViewInfoName.Match( reference.Name );

			if ( match.Success == false )
			{
				// MD 10/9/07 - BR27172
				// The return type has changed
				//return;
				return false;
			}

			string guidString = match.Value.Substring( 2, 36 );
			guidString = guidString.Replace( "_", "" );

			Guid customViewId = new Guid( guidString );

			CustomView customView = manager.Workbook.CustomViews[ customViewId ];

			if ( customView == null )
			{
				Utilities.DebugFail( "There is no custom view with the specified ID." );

				// MD 10/9/07 - BR27172
				// The return type has changed
				//return;
				return false;
			}

			Debug.Assert( customView.SaveHiddenRowsAndColumns );

			int startIndex = match.Length;
			int length = reference.Name.Length - startIndex;

			// MD 4/6/12 - TFS101506
			//string infoType = reference.Name.Substring( startIndex, length ).ToLower( CultureInfo.CurrentCulture );
			string infoType = reference.Name.Substring(startIndex, length).ToLower(CultureInfo.InvariantCulture);

			switch ( infoType )
			{
				case "cols":
					WorkbookSerializationManager.ExtractHiddenColumns( worksheet, customView, reference.FormulaInternal );
					break;

				case "rows":
					WorkbookSerializationManager.ExtractHiddenRows( worksheet, customView, reference.FormulaInternal );
					break;

				// MD 5/25/11 - Data Validations / Page Breaks
				case "printarea":
					{
						PrintOptions printOptions = customView.GetPrintOptions(worksheet);
						NamedReferenceBase.LoadPrintAreas(reference, printOptions);

						// MD 4/6/12 - TFS102169
						// We no longer need this code now that the workbook part is loaded before the worksheet parts.
						//printOptions.HorizontalPageBreaks.OnPrintAreasLoaded();
						//printOptions.VerticalPageBreaks.OnPrintAreasLoaded();
					}
					break;

				// MD 5/25/11 - Data Validations / Page Breaks
				case "printtitles":
					NamedReferenceBase.LoadPrintTitles(reference, customView.GetPrintOptions(worksheet));
					break;

				default:
					Utilities.DebugFail( "Unknown custom info type." );
					break;
			}

			// MD 10/9/07 - BR27172
			// The return type has changed
			return true;
		}

		#endregion TryExtractCustomViewInfo

		#region WriteCellRecord( WorksheetCell, object )



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 10/19/07
		// Found while fixing BR27421
		// Changed the return type to support saving out multiple cell value records
		//private void WriteCellRecord( WorksheetCell cell, object value )
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private int WriteCellRecord( WorksheetCell cell, object value )
		private int WriteCellRecord(CellContext cellContext, object value)
		{
			// MD 4/18/11 - TFS62026
			// We don't need to push the value on the context stack anymore since the CellContext contains the value.
			// Also, update the value of the cell context object which we are reusing for all cells being written out.
			// Push the value on the context stack so the record knows what to save
			//this.ContextStack.Push( value );
			cellContext.Value = value;

			// MD 4/18/11 - TFS62026
			// Cache this.
			short columnIndex = cellContext.ColumnIndex;

			// MD 4/18/11 - TFS62026
			// We don't need the try...finally anymore
			//try
			{
				// BLANK record covers no value
				// MD 10/19/07
				// Found while fixing BR27421
				// Moved code to IsValueBlank to determine whether a cell value is blank
				//if ( value == null ||
				//    value is DBNull )
				if ( WorkbookSerializationManager.IsValueBlank( value ) )
				{
					// MD 10/19/07
					// Found while fixing BR27421
					// We need to support MULBLANK records
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//MultipleCellBlankInfo mulBlankInfo = MultipleCellBlankInfo.GetMultipleCellBlankInfo( cell, value );
					// MD 2/1/12 - TFS100573
					// This method needs a reference to the manager now.
					//MultipleCellBlankInfo mulBlankInfo = MultipleCellBlankInfo.GetMultipleCellBlankInfo(cellContext);
					MultipleCellBlankInfo mulBlankInfo = MultipleCellBlankInfo.GetMultipleCellBlankInfo(this, cellContext);

					if ( mulBlankInfo != null )
					{
						this.ContextStack.Push( mulBlankInfo );
						this.WriteMultipleCellBlanks();
						this.ContextStack.Pop(); // mulBlankInfo

						return mulBlankInfo.LastColumnIndex;
					}

					this.WriteCellBlank();

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

				// All numbers save with either an RK or NUMBER record
				// MD 4/3/12 - TFS107243
				// Moved these checks to a helper method.
				//if ( value is sbyte ||
				//    value is byte ||
				//    value is short ||
				//    value is ushort ||
				//    value is int ||
				//    value is uint ||
				//    value is long ||
				//    value is ulong ||
				//    value is float ||
				//    value is double ||
				//    value is decimal )
				if (Utilities.IsNumber(value))
				{
					// All numbers are doubles in excel
					// MD 4/6/12 - TFS101506
					//double number = Convert.ToDouble( value, CultureInfo.CurrentCulture );
					double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);

					// If number can be encoded as an RK number, save that record, otherwise, 
					// save a NUMBER record
					uint rkValue;

					// MD 6/20/07 - BR24186
					// For number that cannot be encoded with an RK record, we were erroneously writing out a 
					// NUMBER and an RK record. We should only write out one record
					//if ( Utilities.TryEncodeRKValue( number, out rkValue ) == false )
					//    this.WriteRecord( BIFFType.NUMBER );
					//
					//this.WriteRecord( BIFFType.RK );
					if ( Utilities.TryEncodeRKValue( number, out rkValue ) )
					{
						// MD 10/19/07
						// Found while fixing BR27421
						// We need to support MULRK records
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//MultipleCellRKInfo mulRKInfo = MultipleCellRKInfo.GetMultipleCellRKInfo( cell, rkValue );
						// MD 1/10/12 - 12.1 - Cell Format Updates
						// The GetMultipleCellRKInfo method no longer needs the rkValue.
						//MultipleCellRKInfo mulRKInfo = MultipleCellRKInfo.GetMultipleCellRKInfo(cellContext, rkValue);
						// MD 2/1/12 - TFS100573
						// This method needs a reference to the manager now.
						//MultipleCellRKInfo mulRKInfo = MultipleCellRKInfo.GetMultipleCellRKInfo(cellContext);
						MultipleCellRKInfo mulRKInfo = MultipleCellRKInfo.GetMultipleCellRKInfo(this, cellContext);

						if ( mulRKInfo != null )
						{
							this.ContextStack.Push( mulRKInfo );
							this.WriteMultipleCellRKs();
							this.ContextStack.Pop(); // mulRKInfo

							return mulRKInfo.LastColumnIndex;
						}

						this.WriteCellRK();
					}
					else
						this.WriteCellNumber();

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

				if ( value is bool )
				{
					this.WriteCellBoolean();

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

				if ( value is ErrorValue )
				{
					this.WriteCellError();

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

                // MRS 10/1/2008 - TFS8377
                // I moved this code down below. We need to try TryGetLabelValue before we check 
                // for DateTimes. 
                #region Moved down 
                //// The DateTime struct must be saved with its corresponding OLE automation date
                //if ( value is DateTime )
                //{
                //    // MD 10/19/07
                //    // Found while fixing BR27421
                //    // We shouldn't get in here anymore
                //    Utilities.DebugFail( "This should have already been converted in GetSerializableCellValue" );

                //    // MD 10/24/07 - BR27751
                //    // Use a helper method: We may need to coerce the value returned from ToOADate to get around a bug in excel.
                //    //this.WriteCellRecord( cell, ( (DateTime)value ).ToOADate() );
                //    // 8/8/08 - Excel formula solving
                //    // This utility method has been moved
                //    //return this.WriteCellRecord( cell, Utilities.DateTimeToExcelDate( (DateTime)value ) );
                //    return this.WriteCellRecord( cell, ExcelCalcValue.DateTimeToExcelDate( this.Workbook, (DateTime)value ) );
                //}
                #endregion //Moved down

				// MD 11/3/10 - TFS49093
				// StringBuilders are also processed manually.
                //if ( value is FormattedString )
				// MD 2/1/12 - TFS100573
				// The save values for strings will now be their index instead of the StringElement itself.
				//if (value is StringElement || value is StringBuilder)
				if (value is StringElementIndex)
				{
					this.WriteCellFormattedString();

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

				// MD 2/1/12 - TFS100573
				// Removed this code because we didn't expect it to be called anyway.
				//// Call this method recursively for types that should be converted to a formatted string before 
				//// saving, and pass in their formatted string representation as the value to save
				//// MD 11/3/10 - TFS49093
				//// The formatted string data is now stored on the FormattedStringElement.
				////FormattedString formattedString;
				//StringElement formattedString;
				//
				//// MD 5/2/08 - BR32461/BR01870
				//// Added a workbook parameter to the TryGetLabelValue method
				////if ( WorkbookSerializationManager.TryGetLabelValue( value, out formattedString ) )
				//if ( WorkbookSerializationManager.TryGetLabelValue( value, this.Workbook, out formattedString ) )
				//{
				//    // MD 10/19/07
				//    // Found while fixing BR27421
				//    // We shouldn't get in here anymore
				//    Utilities.DebugFail( "This should have already been converted in GetSerializableCellValue" );
				//
				//    // MD 4/12/11 - TFS67084
				//    // Moved away from using WorksheetCell objects.
				//    //return this.WriteCellRecord( cell, formattedString );
				//    return this.WriteCellRecord(cellContext, formattedString);
				//}

                // MRS 10/1/2008 - TFS8377                
                // I moved this code down from above. Checking for dates has to be done after
                // the call to TryGetLabelValue.
                //
                // The DateTime struct must be saved with its corresponding OLE automation date
                if (value is DateTime)
                {
                    // MD 10/19/07
                    // Found while fixing BR27421
                    // We shouldn't get in here anymore
                    Utilities.DebugFail("This should have already been converted in GetSerializableCellValue");

                    // MD 10/24/07 - BR27751
                    // Use a helper method: We may need to coerce the value returned from ToOADate to get around a bug in excel.
                    //this.WriteCellRecord( cell, ( (DateTime)value ).ToOADate() );
                    // 8/8/08 - Excel formula solving
                    // This utility method has been moved
                    //return this.WriteCellRecord( cell, Utilities.DateTimeToExcelDate( (DateTime)value ) );
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
                    //return this.WriteCellRecord(cell, ExcelCalcValue.DateTimeToExcelDate(this.Workbook, (DateTime)value));
					return this.WriteCellRecord(cellContext, ExcelCalcValue.DateTimeToExcelDate(this.Workbook, (DateTime)value));
                }

				// If the value is an array formula, we need to save a FORMULA record which saves a special
				// formula that points to the top-left cell of the array region. Also, for the top-left cell, we need
				// to save the ARRAY record which contains the actual array formula.
				ArrayFormula arrayFormula = value as ArrayFormula;
				if ( arrayFormula != null )
				{
					// The top-left cell is the master cell
					bool isMasterCell =
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//arrayFormula.CellRange.FirstRow == cell.RowIndex &&
						//arrayFormula.CellRange.FirstColumn == cell.ColumnIndex;
						arrayFormula.CellRange.FirstRow == cellContext.Row.Index &&
						arrayFormula.CellRange.FirstColumn == columnIndex;

					this.WriteCellArrayFormula( isMasterCell );

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

				// The FORMULA record saves all other formulas
				if ( value is Formula )
				{
					this.WriteCellFormula();

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

				// If the value is a data table, we need to save a FORMULA record which saves a special formula 
				// that points to the top-left cell of the interior cells of the data table region. Also, for the top-left 
				// cell, we need to save the TABLE record which contains the actual data table information.
				WorksheetDataTable dataTable = value as WorksheetDataTable;
				if ( dataTable != null )
				{
					// MD 3/12/12 - 12.1 - Table Support
					WorksheetRegion cellsInTable = dataTable.CellsInTable;

					// The top-left interior cell is the master cell
					bool isMasterCell =
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//dataTable.CellsInTable.FirstRow == cell.RowIndex - 1 &&
						//dataTable.CellsInTable.FirstColumn == cell.ColumnIndex - 1;
						// MD 3/12/12 - 12.1 - Table Support
						//dataTable.CellsInTable.FirstRow == cellContext.Row.Index - 1 &&
						//dataTable.CellsInTable.FirstColumn == columnIndex - 1;
						cellsInTable.FirstRow == cellContext.Row.Index - 1 &&
						cellsInTable.FirstColumn == columnIndex - 1;

					this.WriteCellDataTable( isMasterCell );

					// MD 10/19/07
					// Found while fixing BR27421
					// Changed the return type of the method
					//return;
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//return cell.ColumnIndex;
					return columnIndex;
				}

				Utilities.DebugFail( "Unsupported cell type: " + value.GetType().ToString() );

				// MD 10/19/07
				// Found while fixing BR27421
				// Changed the return type of the method
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//return cell.ColumnIndex;
				return columnIndex;
			}
			// MD 4/18/11 - TFS62026
			// We don't need the try...finally anymore
			//finally
			{
				// MD 4/18/11 - TFS62026
				// We don't need to push the value on the context stack anymore since the CellContext contains the value.
				//this.ContextStack.Pop(); // value
			}
		}

		#endregion WriteCellRecord( WorksheetCell, object )
	
		// MD 10/19/07
		// Found while fixing BR27421
		// Added helper method to prevent code repetition in WriteWorkbookNumberFormats
		#region WriteStandardFormat

		private void WriteStandardFormat( int index )
		{
			this.ContextStack.Push( new FormatHolder( index, this.Workbook.Formats[ index ] ) );
			this.WriteStandardFormat();
			this.ContextStack.Pop(); // format info
		}

		#endregion WriteStandardFormat

		#endregion Private Methods

		#endregion Methods

		#region Properties

		// MD 11/3/10 - TFS49093
		// We need to store StringBuilders that were added to the string table later in the process.
		#region AdditionalStringsInStringTable

		public Dictionary<StringBuilder, int> AdditionalStringsInStringTable
		{
			get
			{
				if (this.additionalStringsInStringTable == null)
					this.additionalStringsInStringTable = new Dictionary<StringBuilder, int>();

				return this.additionalStringsInStringTable;
			}
		} 

		#endregion // AdditionalStringsInStringTable

		#region ContextStack

		public ContextStack ContextStack
		{
			get { return this.contextStack; }
		}

		#endregion ContextStack

		// MD 4/28/11 - TFS62775
		#region CurrentWorksheetHasShapes

		public bool CurrentWorksheetHasShapes
		{
			get { return this.currentWorksheetHasShapes; }
		}

		#endregion  // CurrentWorksheetHasShapes

		// MD 9/2/08 - Excel2007 format
		// Moved from NAMERecord
		#region CustomViewInfoName

		// MD 4/18/08 - BR32154
		// Made the static variable thread static so we do not get into a race condition when creating it.
		[ThreadStatic]
		private static Regex customViewInfoName;

		// "Z_F863994A_A367_43F6_BF59_A96D3D9B3797_.wvu.Cols"
		private static Regex CustomViewInfoName
		{
			get
			{
				if ( WorkbookSerializationManager.customViewInfoName == null )
                    WorkbookSerializationManager.customViewInfoName = new Regex(@"\GZ_[0-9A-Z]{8}_([0-9A-Z]{4}_){3}[0-9A-Z]{12}_\.wvu\.", RegexOptions.IgnoreCase | Utilities.RegexOptionsCompiled);

				return WorkbookSerializationManager.customViewInfoName;
			}
		}

		#endregion CustomViewInfoName

		// MD 12/21/11 - 12.1 - Table Support
		#region Dxfs

		public List<WorksheetCellFormatData> Dxfs
		{
			get
			{
				if (this.dxfs == null)
					this.dxfs = new List<WorksheetCellFormatData>();

				return this.dxfs;
			}
		}

		#endregion // Dxfs

        #region FilePath

        // MBS 8/19/08 
        // Renamed to FilePath since we now use it during a save as well
        public string FilePath
        {
            get { return this.loadingPath; }
        }

        #endregion FilePath

        #region Fonts

        public List<WorkbookFontData> Fonts
		{
			get { return this.fonts; }
		}

		#endregion Fonts

		#region Images

		public List<ImageHolder> Images
		{
			get { return this.images; }
		}

		#endregion Images

		// MD 10/30/11 - TFS90733
		// This is no longer needed.
		#region Removed

		//        // MD 9/2/08 - Cell Comments
		//        #region MaxAssignedObjectIdNumber

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the maximum previously assigned object ID number for an OBJ record group's common object data header.
		//        /// </summary>  
		//#endif
		//        public int MaxAssignedObjectIdNumber
		//        {
		//            get { return this.maxAssignedObjectIdNumber; }
		//            set { this.maxAssignedObjectIdNumber = value; }
		//        } 

		//        #endregion MaxAssignedObjectIdNumber 

		#endregion // Removed

		// MD 9/10/08 - Cell Comments
		#region MaxShapeId






		public uint MaxShapeId
		{
			get { return this.maxShapeId; }
		} 

		#endregion MaxShapeId

		#region NextWorksheetIndex

		public int NextWorksheetIndex
		{
			get { return this.nextWorksheetIndex; }
			set { this.nextWorksheetIndex = value; }
		}

		#endregion NextWorksheetIndex

		// MD 9/10/08 - Cell Comments
		#region OnlyAssignShapeIdsToCommentShapes






		public abstract bool OnlyAssignShapeIdsToCommentShapes { get; } 

		#endregion OnlyAssignShapeIdsToCommentShapes

		// MD 1/1/12 - 12.1 - Cell Format Updates
		#region ParentStylesToChildren

		public Dictionary<int, List<WorksheetCellFormatData>> ParentStylesToChildren
		{
			get
			{
				if (this.parentStylesToChildren == null)
					this.parentStylesToChildren = new Dictionary<int, List<WorksheetCellFormatData>>();

				return this.parentStylesToChildren;
			}
		}

		#endregion // ParentStylesToChildren

		// MD 4/18/11 - TFS62026
		// This uses too much memory. We will store the resolve format index on the row cache now.
		#region Removed

		//#region ResolvedCellFormatsByCell
		//
		//// MD 4/12/11 - TFS67084
		//// Moved away from using WorksheetCell objects.
		////public Dictionary<WorksheetCell, WorksheetCellFormatData> ResolvedCellFormatsByCell
		//public Dictionary<WorksheetCellAddress, WorksheetCellFormatData> ResolvedCellFormatsByCell
		//{
		//    get
		//    {
		//        if (this.resolvedCellFormatsByCell == null)
		//        {
		//            // MD 4/12/11 - TFS67084
		//            // Moved away from using WorksheetCell objects.
		//            //this.resolvedCellFormatsByCell = new Dictionary<WorksheetCell, WorksheetCellFormatData>();
		//            this.resolvedCellFormatsByCell = new Dictionary<WorksheetCellAddress, WorksheetCellFormatData>();
		//        }
		//
		//        return this.resolvedCellFormatsByCell;
		//    }
		//}
		//
		//#endregion ResolvedCellFormatsByCell 

		#endregion  // Removed

		// MD 7/2/09 - TFS18634
		#region ResolvedCellFormatsByFormat

		// MD 7/26/10 - TFS34398
		// Renamed for clarity
		//public Dictionary<WorksheetCellFormatData, WorksheetCellFormatData> ResolvedCellFormatsForCells
		public Dictionary<WorksheetCellFormatData, WorksheetCellFormatData> ResolvedCellFormatsByFormat
		{
			get
			{
				if ( this.resolvedCellFormatsByFormat == null )
					this.resolvedCellFormatsByFormat = new Dictionary<WorksheetCellFormatData, WorksheetCellFormatData>();

				return this.resolvedCellFormatsByFormat;
			}
		}

		#endregion ResolvedCellFormatsByFormat

		// MD 9/23/09 - TFS19150
		#region ResolvedDefaultCellFormat

		public WorksheetCellFormatData ResolvedDefaultCellFormat
		{
			get { return this.resolvedDefaultCellFormat; }
			set { this.resolvedDefaultCellFormat = value; }
		}

		#endregion ResolvedDefaultCellFormat

		// MD 7/26/10 - TFS34398
		#region RowSerializationCaches

		public Dictionary<WorksheetRow, WorksheetRowSerializationCache> RowSerializationCaches
		{
			get
			{
				if (this.rowSerializationCaches == null)
					this.rowSerializationCaches = new Dictionary<WorksheetRow, WorksheetRowSerializationCache>();

				return this.rowSerializationCaches;
			}
		} 

		#endregion // RowSerializationCaches

		// MD 11/10/11 - TFS85193
		#region ShapeBeingLoaded

		public WorksheetShape ShapeBeingLoaded
		{
			get { return this.shapeBeingLoaded; }
			set
			{
				Debug.Assert(
					this.workbook.CurrentSerializationManager != this,
					"The ShapeBeingLoaded must be set before the workbook begins loading from the stream.");
				this.shapeBeingLoaded = value;
			}
		}

		#endregion  // ShapeBeingLoaded

		// MD 2/1/12 - TFS100573
		#region SharedStringCountDuringSave

		public int SharedStringCountDuringSave
		{
			get { return this.Workbook.SharedStringTable.Count + this.SharedStringTable.Count; }
		}

		#endregion // SharedStringCountDuringSave

		#region SharedStringTable

		// MD 11/3/10 - TFS49093
		// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
		// we are now storing it in a collection on the manager.
		//public List<FormattedStringHolder> SharedStringTable
		public List<StringElement> SharedStringTable
		{
			get { return this.sharedStringTable; }
		}

		#endregion SharedStringTable

		// MD 8/20/07 - BR25818
		// Keep a flag now indicating whether parsed NAME formula tokens should immediately resolve 
		// named references or defer resolving until the workbook global section has been parsed.
		#region ShouldResolveNamedReferences

		public bool ShouldResolveNamedReferences
		{
			get { return this.shouldResolveNamedReferences; }
		}

		#endregion ShouldResolveNamedReferences

		// MD 11/12/07 - BR27987
		// Added a styles collection because we need to add some styles before serializing and we don't 
		// want to directly manipulate the workbook's styles collection.
		#region Styles

		// MD 1/1/12 - 12.1 - Cell Format Updates
		// We don't need this to be a full WorkbookStyleCollection, which contains all the presets now.
		//protected WorkbookStyleCollection Styles
		public List<WorkbookStyle> Styles
		{
			get
			{
				if ( this.styles == null )
				{
					// MD 1/1/12 - 12.1 - Cell Format Updates
					this.styles = new List<WorkbookStyle>();
				}

				return this.styles;
			}
		}

		#endregion Styles

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// The theme colors are now exposed off the workbook.
		#region Removed

		//        // MD 11/29/11 - TFS96205
		//        // Moved this from the Excel2007WorkbookSerializationManager because we can use theme colors in XLS files as well.
		//        #region ThemeColors

		//#if DEBUG
		//        /// <summary>
		//        /// Gets a list of scheme colors loaded from the zip archive
		//        /// </summary>
		//#endif
		//        public List<Color> ThemeColors
		//        {
		//            get
		//            {
		//                if (this.themeColors == null)
		//                    this.themeColors = new List<Color>();

		//                return this.themeColors;
		//            }
		//        }

		//        #endregion ThemeColors

		#endregion // Removed

		#region TotalStringsUsedInDocument

		public uint TotalStringsUsedInDocument
		{
			get { return this.totalStringsUsedInDocument; }
			set { this.totalStringsUsedInDocument = value; }
		}

		#endregion TotalStringsUsedInDocument

		#region Workbook

		public Workbook Workbook
		{
			get { return this.workbook; }
		}

		#endregion Workbook

		#region WorkbookReferences

		internal List<WorkbookReferenceBase> WorkbookReferences
		{
			get
			{
				if ( this.workbookReferences == null )
					this.workbookReferences = new List<WorkbookReferenceBase>();

				return this.workbookReferences;
			}
		}

		#endregion WorkbookReferences

		#region WorksheetIndices

		public Dictionary<int, int> WorksheetIndices
		{
			get { return this.worksheetIndices; }
		}

		#endregion WorksheetIndices

		#region WorksheetReferences

		internal List<WorksheetReference> WorksheetReferences
		{
			get
			{
				if ( this.worksheetReferences == null )
					this.worksheetReferences = new List<WorksheetReference>();

				return this.worksheetReferences;
			}
		}

		#endregion WorksheetReferences

		#endregion Properties

		// MD 3/15/12 - TFS104581
		// This is no longer needed now that we actually sotre column blocks on the worksheet.
		#region Removed

		//        #region ColumnBlockInfo class

		//#if DEBUG
		//        /// <summary>
		//        /// Represents a contiguous block of columns which all have the same settings.
		//        /// </summary> 
		//#endif
		//        internal class ColumnBlockInfo
		//        {
		//            #region Member Variables

		//            // block bounds
		//            private int firstColumnIndex;
		//            private int lastColumnIndex;

		//            // column settings
		//            private int formatIndex;
		//            private bool hasCollapseIndicator;
		//            private bool hidden;
		//            private int outlineLevel;
		//            private int width;

		//            #endregion Member Variables

		//            #region Constuctor

		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            // We need the manager in this constructor.
		//            //public ColumnBlockInfo( WorksheetColumn column )
		//            public ColumnBlockInfo(WorkbookSerializationManager manager, WorksheetColumn column)
		//            {
		//                this.firstColumnIndex = column.Index;
		//                this.lastColumnIndex = this.firstColumnIndex;

		//                this.width = column.Width;

		//                // MD 1/10/12 - 12.1 - Cell Format Updates
		//                //// MD 1/28/09 - TFS14200
		//                //Debug.Assert( column.HasCellFormat, "If a COLINFO record will be written out for this column, the cell format should have been lazily created by the fix for TFS12400." );
		//                //
		//                //if ( column.HasCellFormat )
		//                //    this.formatIndex = column.CellFormatInternal.Element.IndexInFormatCollection;
		//                this.formatIndex = ColumnBlockInfo.GetColumnFormatIndex(manager, column);

		//                this.hidden = column.Hidden;
		//                this.outlineLevel = column.OutlineLevel;
		//                this.hasCollapseIndicator = column.HasCollapseIndicator;
		//            }

		//            #endregion Constuctor

		//            #region Methods

		//            // MD 1/11/08 - BR29105
		//            #region DoColumnWidthsMatch

		//            private static bool DoColumnWidthsMatch( Worksheet worksheet, int columnWidth1, int columnWidth2 )
		//            {
		//                if ( columnWidth1 == columnWidth2 )
		//                    return true;

		//                // MD 2/10/12 - TFS97827
		//                // The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
		//                //if ( columnWidth1 == 0 && columnWidth2 == worksheet.DefaultColumnWidthResolved )
		//                if (columnWidth1 == 0 && columnWidth2 == worksheet.DefaultColumnWidth)
		//                    return true;

		//                // MD 2/10/12 - TFS97827
		//                // The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
		//                //if ( columnWidth2 == 0 && columnWidth1 == worksheet.DefaultColumnWidthResolved )
		//                if (columnWidth2 == 0 && columnWidth1 == worksheet.DefaultColumnWidth)
		//                    return true;

		//                return false;
		//            }

		//            #endregion DoColumnWidthsMatch

		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            #region GetColumnFormatIndex

		//            private static int GetColumnFormatIndex(WorkbookSerializationManager manager, WorksheetColumn column)
		//            {
		//                WorksheetCellFormatData columnFormat;
		//                if (column.HasCellFormat)
		//                    columnFormat = column.CellFormatInternal.Element;
		//                else
		//                    columnFormat = manager.Workbook.CellFormats.DefaultElement;

		//                return manager.GetCellFormatIndex(columnFormat);
		//            }

		//            #endregion // GetColumnFormatIndex

		//            #region TryAddColumn

		//#if DEBUG
		//            /// <summary>
		//            /// Adds the specified column to the block if it contains the same settings as the other 
		//            /// columns in the block and is adjacent to the last column in the block. Returns True if
		//            /// the column was successfully added; False otherwise.
		//            /// </summary>
		//#endif
		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            // We need the manager in this method.
		//            //public bool TryAddColumn( WorksheetColumn column )
		//            public bool TryAddColumn(WorkbookSerializationManager manager, WorksheetColumn column)
		//            {
		//                // If the column is not the column right after the last one, it can't be added
		//                if ( column.Index != this.lastColumnIndex + 1 )
		//                    return false;

		//                // If any of the column settings are different than the columns in the block, it 
		//                // can't be added
		//                if ( column.Hidden != this.hidden )
		//                    return false;

		//                if ( column.HasCollapseIndicator != this.hasCollapseIndicator )
		//                    return false;

		//                if ( column.OutlineLevel != this.outlineLevel )
		//                    return false;

		//                // MD 1/11/08 - BR29105
		//                // The columns' widths can also be the same if one is zero and the other is the resolved default column width 
		//                // for the worksheet they belong to. Use a helper method which will check this for both columns.
		//                //if ( column.Width != this.width )
		//                if ( ColumnBlockInfo.DoColumnWidthsMatch( column.Worksheet, column.Width, this.width ) == false )
		//                    return false;

		//                // MD 1/10/12 - 12.1 - Cell Format Updates
		//                //// MD 1/28/09 - TFS12400
		//                //Debug.Assert( column.HasCellFormat, "If a COLINFO record will be written out for this column, the cell format should have been lazily created by the fix for TFS12400." );
		//                //
		//                //int formatIndex = column.HasCellFormat
		//                //    ? column.CellFormatInternal.Element.IndexInFormatCollection
		//                //    : 0;
		//                int formatIndex = ColumnBlockInfo.GetColumnFormatIndex(manager, column);

		//                if ( formatIndex != this.formatIndex )
		//                    return false;

		//                this.lastColumnIndex = column.Index;
		//                return true;
		//            }

		//            #endregion TryAddColumn

		//            #endregion Methods

		//            #region Properties

		//            #region Hidden

		//            public bool Hidden
		//            {
		//                get { return this.hidden; }
		//            }

		//            #endregion Hidden

		//            #region FirstColumnIndex

		//            public int FirstColumnIndex
		//            {
		//                get { return this.firstColumnIndex; }
		//            }

		//            #endregion FirstColumnIndex

		//            #region FormatIndex

		//            public int FormatIndex
		//            {
		//                get { return this.formatIndex; }
		//            }

		//            #endregion FormatIndex

		//            #region HasCollapseIndicator

		//            public bool HasCollapseIndicator
		//            {
		//                get { return this.hasCollapseIndicator; }
		//            }

		//            #endregion HasCollapseIndicator

		//            #region LastColumnIndex

		//            public int LastColumnIndex
		//            {
		//                get { return this.lastColumnIndex; }
		//            }

		//            #endregion LastColumnIndex

		//            #region OutlineLevel

		//            public int OutlineLevel
		//            {
		//                get { return this.outlineLevel; }
		//            }

		//            #endregion OutlineLevel

		//            #region Width

		//            public int Width
		//            {
		//                get { return this.width; }
		//            }

		//            #endregion Width

		//            #endregion Properties
		//        }

		//        #endregion ColumnBlockInfo class

		#endregion // Removed

		#region FormatHolder class

		internal class FormatHolder
		{
			private int index;
			private string format;

			public FormatHolder( int index, string format )
			{
				this.index = index;
				this.format = format;
			}

			public string Format
			{
				get { return this.format; }
			}

			public int Index
			{
				get { return this.index; }
			}
		} 

		#endregion FormatHolder class

		// MD 11/3/10 - TFS49093
		// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
		// we are now storing it in a collection on the manager.
		#region Removed

		//        #region FormattedStringHolder class
		//
		//#if DEBUG
		//        /// <summary>
		//        /// Represents a formatted string used as the value of a cell
		//        /// </summary> 
		//#endif
		//        internal class FormattedStringHolder :
		//            IComparable<FormattedStringHolder>	// MD 6/11/07 - BR23706
		//        {
		//            #region Member Variables
		//
		//            // MD 11/3/10 - TFS49093
		//            // The formatted string data is now stored on the FormattedStringElement.
		//            //private FormattedString value;
		//            private FormattedStringElement value;
		//
		//            private int offsetInRecordBlock;
		//            private long absolutePosition;
		//
		//            #endregion Member Variables
		//
		//            #region Constructor
		//
		//            // MBS 7/25/08 - Excel 2007 Format
		//            // Added a new overload since the offset and position aren't needed for loading
		//            // MD 11/3/10 - TFS49093
		//            // The formatted string data is now stored on the FormattedStringElement.
		//            //public FormattedStringHolder(FormattedString value)
		//            public FormattedStringHolder(FormattedStringElement value)
		//                : this(value, 0, 0) { }
		//
		//            // MD 11/3/10 - TFS49093
		//            // The formatted string data is now stored on the FormattedStringElement.
		//            //public FormattedStringHolder( FormattedString value, int offsetInRecordBlock, long absolutePosition )
		//            public FormattedStringHolder(FormattedStringElement value, int offsetInRecordBlock, long absolutePosition)
		//            {
		//                this.value = value;
		//                this.offsetInRecordBlock = offsetInRecordBlock;
		//                this.absolutePosition = absolutePosition;
		//            }
		//
		//            #endregion Constructor
		//
		//            #region Base Class Overrides
		//
		//            #region Equals
		//
		//            public override bool Equals( object obj )
		//            {
		//                FormattedStringHolder holder = obj as FormattedStringHolder;
		//
		//                if ( holder == null )
		//                    return false;
		//
		//                return this.value.Equals( holder.value );
		//            }
		//
		//            #endregion Equals
		//
		//            #region GetHashCode
		//
		//            public override int GetHashCode()
		//            {
		//                return this.value.GetHashCode();
		//            }
		//
		//            #endregion GetHashCode
		//
		//            // MD 10/22/07
		//            // Override ToString to aid with debugging
		//            #region ToString
		//
		//            public override string ToString()
		//            {
		//                return this.value.ToString();
		//            }
		//
		//            #endregion ToString
		//
		//            #endregion Base Class Overrides
		//
		//            #region Interfaces
		//
		//            #region IComparable<FormattedStringHolder> Members
		//
		//            // MD 1/24/08
		//            // Made changes to allow for VS2008 style unit test accessors
		//            //int IComparable<FormattedStringHolder>.CompareTo( FormattedStringHolder other )
		//            public int CompareTo( FormattedStringHolder other )
		//            {
		//                // MD 11/3/10 - TFS49093
		//                // The FormattedStringElement defines the IComparable implicitly.
		//                //return ( (IComparable<FormattedString>)this.value ).CompareTo( other.value );
		//                return this.value.CompareTo(other.value);
		//            }
		//
		//            #endregion
		//
		//            #endregion Interfaces
		//
		//            #region Properties
		//
		//            #region AbsolutePosition
		//
		//            public long AbsolutePosition
		//            {
		//                get { return this.absolutePosition; }
		//                set { this.absolutePosition = value; }
		//            }
		//
		//            #endregion AbsolutePosition
		//
		//            #region OffsetInRecordBlock
		//
		//            public int OffsetInRecordBlock
		//            {
		//                get { return this.offsetInRecordBlock; }
		//                set { this.offsetInRecordBlock = value; }
		//            }
		//
		//            #endregion OffsetInRecordBlock
		//
		//            #region Value
		//
		//            // MD 11/3/10 - TFS49093
		//            // The formatted string data is now stored on the FormattedStringElement.
		//            //public FormattedString Value
		//            public FormattedStringElement Value
		//            {
		//                get { return this.value; }
		//            }
		//
		//            #endregion Value
		//
		//            #endregion Properties
		//        }
		//
		//        #endregion FormattedStringHolder class 

		#endregion // Removed

		#region ImageHolder class






		public class ImageHolder
		{
			#region Member Variables

			private Image image;
			private uint referenceCount;

			#endregion Member Variables

			#region Constructor

			public ImageHolder( Image image )
				: this( image, 1 ) { }

			public ImageHolder( Image image, uint referenceCount )
			{
				this.image = image;
				this.referenceCount = referenceCount;
			}

			#endregion Constructor

			#region Base Class Overrides

			#region Equals

			public override bool Equals( object obj )
			{
				ImageHolder holder = obj as ImageHolder;

				if ( holder == null )
					return false;

				return this.image.Equals( holder.image );
			}

			#endregion Equals

			#region GetHashCode

			public override int GetHashCode()
			{
				return this.image.GetHashCode();
			}

			#endregion GetHashCode

			#endregion Base Class Overrides

			#region Properties

			#region Image

			public Image Image
			{
				get { return this.image; }
			}

			#endregion Image

			#region ReferenceCount

			public uint ReferenceCount
			{
				get { return this.referenceCount; }
				set { this.referenceCount = value; }
			}

			#endregion ReferenceCount

			#endregion Properties
		}

		#endregion ImageHolder class
	}

	// MD 4/18/11 - TFS62026
	internal class CellContext
	{
		private short columnIndex;
		private WorksheetRow row;
		private WorksheetRowSerializationCache rowCache;
		private object value;

		public CellContext(WorksheetRow row, WorksheetRowSerializationCache rowCache)
		{
			this.row = row;
			this.rowCache = rowCache;
		}

		public short ColumnIndex
		{
			get { return this.columnIndex; }
			set { this.columnIndex = value; }
		}

		public WorksheetRow Row
		{
			get { return this.row; }
		}

		public WorksheetRowSerializationCache RowCache
		{
			get { return this.rowCache; }
		}

		public object Value
		{
			get { return this.value; }
			set { this.value = value; }
		}
	}

	// MD 4/12/11 - TFS67084
	internal struct ColumnIndex
	{
		private short columnIndex;

		private ColumnIndex(short columnIndex)
		{
			this.columnIndex = columnIndex;
		}

		public static implicit operator short(ColumnIndex index)
		{
			return index.columnIndex;
		}

		public static implicit operator ColumnIndex(short index)
		{
			return new ColumnIndex(index); 
		}
	}

	// MD 2/1/12 - TFS100573
	internal class StringElementIndex
	{
		public readonly int IndexInSharedStringTable;

		public StringElementIndex(int indexInSharedStringTable)
		{
			this.IndexInSharedStringTable = indexInSharedStringTable;
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