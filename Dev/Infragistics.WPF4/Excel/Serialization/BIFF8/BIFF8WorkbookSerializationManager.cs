using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;
using Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords;
using Infragistics.Documents.Excel.Sorting;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8
{





	internal sealed class BIFF8WorkbookSerializationManager : WorkbookSerializationManager
	{
		#region Constants

		// MD 1/10/12 - 12.1 - Cell Format Updates
		private const int DefaultCellFormatXfId = 15;

		#endregion // Constants

		#region Member Variables

		private List<uint> boundSheetStreamPositions;
		private Biff8RecordStream currentRecordStream;

		// MD 7/9/10 - TFS34476
		private Dictionary<FontInfo, int> defaultRowHeights;

		// MD 10/19/07
		// Found while fixing BR27421
		// This is not needed anymore
		//private Dictionary<int, string> numberFormats;

		// MD 3/30/10 - TFS30253
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private Dictionary<WorksheetCell, Formula> sharedFormulas; // Key: top-left cell of the shared range; Value: shared formula
		private Dictionary<WorksheetCellAddress, Formula> sharedFormulas; // Key: top-left cell of the shared range; Value: shared formula

		private Stream workbookStream;
		private BinaryReader workbookStreamReader;

		// MD 11/3/10 - TFS49093
		// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
		// we are now storing it in a collection on the manager.
		private List<EXTSSTItem> extSSTData = new List<EXTSSTItem>();

		// MD 3/30/11 - TFS69969
		private ExternalWorkbookReference currentExternalWorkbookReference;
		private WorksheetReferenceExternal currentExternalWorksheetReference;

		// MD 4/18/11 - TFS62026
		// Cache common record types so we don't have to do a dictionary lookup for them.
		private BOOLERRRecord boolerrRecord = new BOOLERRRecord();
		private BLANKRecord blankRecord = new BLANKRecord();
		private LABELSSTRecord labelsstRecord = new LABELSSTRecord();
		private FORMULARecord formulaRecord = new FORMULARecord();
		private NUMBERRecord numberRecord = new NUMBERRecord();
		private RKRecord rkRecord = new RKRecord();
		private ROWRecord rowRecord = new ROWRecord();

		// MD 5/26/11 - TFS76587
		private Dictionary<WorksheetCellAddress, bool> pendingSharedFormulaRoots;

		// MD 11/8/11 - TFS85193
		// The comments which are loaded but not applied to cells need to be stored here.
		private List<WorksheetCellComment> loadedComments;

		// MD 11/10/11 - TFS85193
		// We may need the package factory to load Excel 2007 data embedded in the BIFF8 file.
		private IPackageFactory packageFactory;
		private bool verifyExcel2007Xml;

		// MD 11/29/11 - TFS96205
		// MD 1/17/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//private Dictionary<int, ExtProp[]> extProps;
		private bool ignoreXFEXTData;
		private byte[] xfRecordData;

		// MD 1/10/12 - 12.1 - Cell Format Updates
		private int firstCellFormatXfId;
		private Dictionary<WorkbookStyle, ushort> styleFormatXfIds = new Dictionary<WorkbookStyle, ushort>();

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Moved this from the base manager because it is not needed by the Excel 2007 formats.
		private List<WorksheetCellFormatData> formats;

		// MD 2/19/12 - 12.1 - Table Support
		private List<WorkbookStyle> loadedStyles;

		#endregion Member Variables

		#region Constructor

		public BIFF8WorkbookSerializationManager( Stream workbookStream, Workbook workbook )
			// MD 11/10/11 - TFS85193
			//: this( workbookStream, workbook, null ) { }
			: this(workbookStream, workbook, null, null, false) { }

		// MD 11/10/11 - TFS85193
		// We may need the package factory to load Excel 2007 data embedded in the BIFF8 file.
		//public BIFF8WorkbookSerializationManager( Stream workbookStream, Workbook workbook, string loadingPath )
		public BIFF8WorkbookSerializationManager(Stream workbookStream, Workbook workbook, string loadingPath, IPackageFactory packageFactory, bool verifyExcel2007Xml)
			: base( workbook, loadingPath )
		{
			this.workbookStream = new WorkbookBufferedStream( workbookStream );

			// MD 10/19/07
			// Found while fixing BR27421
			// This is not needed anymore
			//this.numberFormats = new Dictionary<int, string>();

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Moved this from the base manager because it is not needed by the Excel 2007 formats.
			this.formats = new List<WorksheetCellFormatData>();

			// MD 11/10/11 - TFS85193
			// We may need the package factory to load Excel 2007 data embedded in the BIFF8 file.
			this.packageFactory = packageFactory;
			this.verifyExcel2007Xml = verifyExcel2007Xml;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region AddFormat

		public override void AddFormat(WorksheetCellFormatData format)
		{
			this.AddFont(format.FontInternal, format);
			this.formats.Add(format);
		}

		#endregion // AddFormat

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			if ( this.currentRecordStream != null )
			{
                Utilities.DebugFail("This should have been closed already");
				this.currentRecordStream.Dispose();
			}

			// MD 6/25/07 - BR24226
			// The workbookStreamReader is lazy loaded and therefore may not always be created when the 
			// serialization manager is disposed, so check it for null first
			//this.workbookStreamReader.Close();
			if ( this.workbookStreamReader != null )
				this.workbookStreamReader.Close();

			this.workbookStream.Dispose();

			base.Dispose( disposing );
		}

		#endregion Dispose

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region GetCellFormatIndex

		public override ushort GetCellFormatIndex(WorksheetCellFormatData cellFormat)
		{
			Debug.Assert(cellFormat.Type == WorksheetCellFormatType.CellFormat, "The format is not a cell format.");
			Debug.Assert(cellFormat.Workbook == this.Workbook, "The cell format is from a different workbook.");

			WorksheetCellFormatCollection cellFormats = this.Workbook.CellFormats;
			WorksheetCellFormatData defaultElement = cellFormats.DefaultElement;
			if (cellFormat == defaultElement)
				return BIFF8WorkbookSerializationManager.DefaultCellFormatXfId;

			int defaultElementIndex = cellFormats.FindIndex(defaultElement);
			int index = cellFormats.FindIndex(cellFormat);

			// Since we skipped the default element when writing out the cell XFs at the end of the XF stream,
			// we need to correct the index by one if it is after the default element in the collection.
			if (index > defaultElementIndex)
				index--;

			return (ushort)(this.firstCellFormatXfId + index);
		}

		#endregion // GetCellFormatIndex

		// MD 3/13/12 - 12.1 - Table Support
		#region GetLoadedDefaultCellFormat

		public override WorksheetCellFormatData GetLoadedDefaultCellFormat()
		{
			if (this.formats.Count <= BIFF8WorkbookSerializationManager.DefaultCellFormatXfId)
			{
				Utilities.DebugFail("This is unexpected.");
				return null;
			}

			return this.Formats[BIFF8WorkbookSerializationManager.DefaultCellFormatXfId];
		}

		#endregion // GetLoadedDefaultCellFormat

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region GetStyleFormatIndex

		public override ushort GetStyleFormatIndex(WorkbookStyle style)
		{
			Debug.Assert(this.styleFormatXfIds.ContainsKey(style), "The style is not being saved out.");

			ushort xfId;
			this.styleFormatXfIds.TryGetValue(style, out xfId);
			return xfId;
		}

		#endregion // GetStyleFormatIndex

		#region LoadWorkbookContents






		protected override void LoadWorkbookContents()
		{
			// MD 11/29/11 - TFS96205
			// We need to keep track of when the XF record block starts because we need to cache the data from that block.
			long xfRecordsStart = -1;

			// MD 10/7/10 - TFS36582
			// When reading in an Excel file from a 3rd party, it had an extra byte at the end of the file. We need to 
			// read two bytes when reading a record type, so make sure we have enough room to read two bytes.
			//while ( this.workbookStream.Position < this.workbookStream.Length )
			long maxPosition = this.workbookStream.Length - 2;
			while (this.workbookStream.Position <= maxPosition)
			{
				// MD 11/29/11 - TFS96205
				// Keep track of the start of each record.
				long currentRecordStart = this.workbookStream.Position;

				// Read the next biff record in the workbook stream
				using ( this.currentRecordStream = new Biff8RecordStream( this ) )
				{
					// MD 11/29/11 - TFS96205
					// If the XF block is just starting, cache the start of the raw XF block. If it is just ending, cache the 
					// entire XF data block.
					if (xfRecordsStart < 0)
					{
						if (this.currentRecordStream.RecordType == BIFF8RecordType.XF)
							xfRecordsStart = currentRecordStart;
					}
					else if (this.xfRecordData == null)
					{
						if (this.currentRecordStream.RecordType != BIFF8RecordType.XF)
							this.CacheXFRecordData(xfRecordsStart);
					}

					// MD 5/22/07 - BR23135
					// If the record stream is invalid, stop loading, and null out the current record stream so 
					// we don't dispose it again later
					if ( this.currentRecordStream.RecordType == BIFF8RecordType.Default )
					{
						this.currentRecordStream = null;
						return;
					}

					// MD 7/20/2007 - BR25039
					// Moved the following code to the new LoadCurrentRecord helper method
					#region Moved

					//                    // Get the record class to load the record
					//                    BiffRecordBase record = BiffRecordBase.GetBiffRecord( this.currentRecordStream.RecordType );
					//
					//                    if ( record != null )
					//                    {
					//                        Debug.Assert( record.Type == this.currentRecordStream.RecordType, "The type of the record doesn't match the record id" );
					//
					//                        // load the record's data
					//                        record.Load( this );
					//                    }

					#endregion Moved
					this.LoadCurrentRecord();
				}
			}

			Dictionary<ushort, WorksheetShape> shapeByObjIds = new Dictionary<ushort, WorksheetShape>();
			List<FtRboData> radioButtonsRequiringFixup = new List<FtRboData>();
			foreach (Worksheet worksheet in this.Workbook.Worksheets)
			{
				BIFF8WorkbookSerializationManager.GetsShapeIdsAndRadioButtons(worksheet.Shapes, shapeByObjIds, radioButtonsRequiringFixup);
			}

			for (int i = 0; i < radioButtonsRequiringFixup.Count; i++)
			{
				FtRboData radioButton = radioButtonsRequiringFixup[i];
				WorksheetShape shape;
				if (shapeByObjIds.TryGetValue(radioButton.IdRadNext, out shape))
					radioButton.IdRadNextShape = shape;
				else
					Utilities.DebugFail("Cannot find the shape with the id: " + radioButton.IdRadNext);
			}

			this.currentRecordStream = null;
		}

		#endregion LoadWorkbookContents

        #region InitializeCellFormats






		protected override void InitializeCellFormats()
		{
			// MD 1/1/12 - 12.1 - Cell Format Updates
			// Rewrote this code to function more closely with the way Excel works.
			#region Old Code

			//WorksheetCellFormatData standardFormat = this.Workbook.CellFormats.DefaultElement.ResolvedCellFormatData();

			//// MD 12/9/10 - TFS60712
			//// The default format must have no style options set even though it actually has all style options set. This is needed so all checkboxes are checked
			//// on the style dialog when looking at the Normal style.
			//standardFormat.FormatOptions = StyleCellFormatOptions.None;

			//// MD 10/23/07 - BR27496
			//// The first format stored in the file must reference the first font in the file, and if False if 
			//// passed in as the second argument here, that is impossible. If we don't do this, copying cells
			//// from our exported workbook to another workbook while they are both open in the same instance of 
			//// Excel 2000 will cause an unhandled exception that causes Excel to shutdown.
			////this.AddResolvedFormat( (WorksheetCellFormatData)standardFormat.Clone(), false );
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), true);

			//// MD 11/12/07 - BR27987
			//// Create a normal built in style based on the last added format which will be added to the 
			//// styles collection if it doesn't already contain this built in style type.
			//WorkbookBuiltInStyle normalStyle = new WorkbookBuiltInStyle(this.Workbook, this.Formats[this.Formats.Count - 1], BuiltInStyleType.Normal, 255, false);

			//standardFormat.Font.SetFontFormatting(this.Fonts[1]);

			//standardFormat.FormatOptions =
			//    StyleCellFormatOptions.UseBorderFormatting |
			//    StyleCellFormatOptions.UseFontFormatting |
			//    StyleCellFormatOptions.UseNumberFormatting |
			//    StyleCellFormatOptions.UsePatternsFormatting |
			//    StyleCellFormatOptions.UseProtectionFormatting;
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//standardFormat.Font.SetFontFormatting(this.Fonts[2]);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//standardFormat.Font.SetFontFormatting(this.Fonts[0]);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//standardFormat.IsStyle = false;
			//standardFormat.FormatOptions = StyleCellFormatOptions.None;

			//// MD 12/31/11 - 12.1 - Table Support
			//// When the FormatOptions are set to None, all properties are reset, so we need to resolve again here.
			//standardFormat = standardFormat.ResolvedCellFormatData();

			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//standardFormat.Font.SetFontFormatting(this.Fonts[1]);
			//standardFormat.FormatStringIndex = 43;
			//standardFormat.IsStyle = true;
			//standardFormat.FormatOptions =
			//    StyleCellFormatOptions.UseAlignmentFormatting |
			//    StyleCellFormatOptions.UseBorderFormatting |
			//    StyleCellFormatOptions.UseFontFormatting |
			//    // MD 11/12/07 - BR27987
			//    // This cannot be included in the built in style formats 
			//    // (we are specifying a new format and therefore do not want to use the default number formatting)
			//    //StyleCellFormatOptions.UseNumberFormatting |
			//    StyleCellFormatOptions.UsePatternsFormatting |
			//    StyleCellFormatOptions.UseProtectionFormatting;
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//// MD 11/12/07 - BR27987
			//// Create a comma built in style based on the last added format which will be added to the 
			//// styles collection if it doesn't already contain this built in style type.
			//WorkbookBuiltInStyle commaStyle = new WorkbookBuiltInStyle(this.Workbook, this.Formats[this.Formats.Count - 1], BuiltInStyleType.Comma, 255, false);

			//standardFormat.FormatStringIndex = 41;
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//// MD 11/12/07 - BR27987
			//// Create a comma0 built in style based on the last added format which will be added to the 
			//// styles collection if it doesn't already contain this built in style type.
			//WorkbookBuiltInStyle comma0Style = new WorkbookBuiltInStyle(this.Workbook, this.Formats[this.Formats.Count - 1], BuiltInStyleType.Comma0, 255, false);

			//standardFormat.FormatStringIndex = 44;
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//// MD 11/12/07 - BR27987
			//// Create a currency built in style based on the last added format which will be added to the 
			//// styles collection if it doesn't already contain this built in style type.
			//WorkbookBuiltInStyle currencyStyle = new WorkbookBuiltInStyle(this.Workbook, this.Formats[this.Formats.Count - 1], BuiltInStyleType.Currency, 255, false);

			//standardFormat.FormatStringIndex = 42;
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//// MD 11/12/07 - BR27987
			//// Create a currency0 built in style based on the last added format which will be added to the 
			//// styles collection if it doesn't already contain this built in style type.
			//WorkbookBuiltInStyle currency0Style = new WorkbookBuiltInStyle(this.Workbook, this.Formats[this.Formats.Count - 1], BuiltInStyleType.Currency0, 255, false);

			//standardFormat.FormatStringIndex = 9;
			//this.AddResolvedFormat((WorksheetCellFormatData)standardFormat.Clone(), false);

			//// MD 11/12/07 - BR27987
			//// Create a percent built in style based on the last added format which will be added to the 
			//// styles collection if it doesn't already contain this built in style type.
			//WorkbookBuiltInStyle percentStyle = new WorkbookBuiltInStyle(this.Workbook, this.Formats[this.Formats.Count - 1], BuiltInStyleType.Percent, 255, false);

			//if (Workbook.HasStyles)
			//{
			//    foreach (WorkbookStyle style in Workbook.Styles)
			//    {
			//        // MD 12/9/10 - TFS60712
			//        // The Normal built in style must use the default format or Excel 2003 will blow up when opening the style dialog.
			//        // Just hardcode this style to use the first format and font. We don't have to add in the resolved format, because we 
			//        // know they are already in the collection.
			//        WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
			//        if (builtInStyle != null && builtInStyle.Type == BuiltInStyleType.Normal)
			//        {
			//            style.StyleFormatInternal.Element.IndexInXfsCollection = 0;
			//            style.StyleFormatInternal.Element.IndexInFormatCollection = 0;
			//            // MD 11/11/11 - TFS85193
			//            //style.StyleFormatInternal.Element.FontInternal.Element.IndexInFontCollection = 0;
			//            style.StyleFormatInternal.Element.FontInternal.Element.SetIndexInFontCollection(FontResolverType.Normal, 0);
			//            continue;
			//        }

			//        // MD 11/12/07 - BR27987
			//        // Pass 2nd parameter to new overload of the AddFormat method
			//        // If the style is a WorkbookBuiltInStyle instance, we want to pass in false as the second parameter
			//        // so the number format style options can not be resolved.
			//        //this.AddFormat( style.StyleFormatInternal.Element );
			//        // MD 12/9/10 - TFS60712
			//        // We already did an "as" cast above, so we don't need to also do an "is" check here. We can just check whether the built 
			//        // in style is null.
			//        //this.AddFormat(style.StyleFormatInternal.Element, (style is WorkbookBuiltInStyle) == false);
			//        // MD 12/31/11 - 12.1 - Table Support
			//        // The allowNumberFormattingFlag is no longer needed now that we are no longer resolving format flags in ResolvedCellFormatData.
			//        //this.AddFormat(style.StyleFormatInternal.Element, builtInStyle == null);
			//        this.AddFormat(style.StyleFormatInternal.Element);
			//    }

			//    // MD 11/12/07 - BR27987
			//    // Initialize the styles collection with the styles of the workbook
			//    //this.Styles = Workbook.Styles.Clone();
			//    WorkbookStyleCollection styleCollection = Workbook.Styles.Clone();
			//    foreach (WorkbookStyle style in styleCollection)
			//    {
			//        this.Styles.Add(style);
			//    }
			//}

			//// MD 11/12/07 - BR27987
			//// Add the styles if they do not already exist in the collection
			//if (this.Styles.ContainsBuiltInStyle(commaStyle.Type) == false)
			//    this.Styles.Add(commaStyle);

			//if (this.Styles.ContainsBuiltInStyle(comma0Style.Type) == false)
			//    this.Styles.Add(comma0Style);

			//if (this.Styles.ContainsBuiltInStyle(currencyStyle.Type) == false)
			//    this.Styles.Add(currencyStyle);

			//if (this.Styles.ContainsBuiltInStyle(currency0Style.Type) == false)
			//    this.Styles.Add(currency0Style);

			//if (this.Styles.ContainsBuiltInStyle(normalStyle.Type) == false)
			//    this.Styles.Add(normalStyle);

			//if (this.Styles.ContainsBuiltInStyle(percentStyle.Type) == false)
			//    this.Styles.Add(percentStyle);

			#endregion // Old Code
			WorkbookStyleCollection styles = this.Workbook.Styles;
			this.AddStyle(styles.NormalStyle);
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.RowLevelX, 0));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.ColLevelX, 0));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.RowLevelX, 1));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.ColLevelX, 1));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.RowLevelX, 2));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.ColLevelX, 2));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.RowLevelX, 3));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.ColLevelX, 3));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.RowLevelX, 4));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.ColLevelX, 4));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.RowLevelX, 5));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.ColLevelX, 5));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.RowLevelX, 6));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.ColLevelX, 6));
			Debug.Assert(this.Formats.Count == BIFF8WorkbookSerializationManager.DefaultCellFormatXfId, "The default cell element should is going to be at the wrong index.");
			this.AddFormat(this.Workbook.CellFormats.DefaultElement);
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.Comma));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.Comma0));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.Currency));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.Currency0));
			this.AddStyle(styles.GetBuiltInStyle(BuiltInStyleType.Percent));

			foreach (WorkbookStyle style in this.Workbook.Styles)
			{
				if (style.ShouldSaveIn2003 == false)
					continue;

				WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
				if (builtInStyle != null)
				{
					// For the built in styles which already have their formats added, just add them to the styles collection.
					// Don't add their format again.
					switch (builtInStyle.Type)
					{
						case BuiltInStyleType.ColLevelX:
						case BuiltInStyleType.RowLevelX:
						case BuiltInStyleType.Comma:
						case BuiltInStyleType.Comma0:
						case BuiltInStyleType.Currency:
						case BuiltInStyleType.Currency0:
						case BuiltInStyleType.Normal:
						case BuiltInStyleType.Percent:
							this.Styles.Add(style);
							continue;
					}
				}

				this.Styles.Add(style);
				this.AddStyle(style);
			}

			this.firstCellFormatXfId = this.Formats.Count;

			WorksheetCellFormatCollection cellFormats = this.Workbook.CellFormats;
			foreach (WorksheetCellFormatData cellFormat in cellFormats)
			{
				if (cellFormat == cellFormats.DefaultElement)
					continue;

				this.AddFormat(cellFormat);
			}
		}

        #endregion InitializeCellFormats

        #region InitializeFonts






        protected override void  InitializeFonts()
        {
			// MD 1/18/12 - 12.1 - Cell Format Updates
			#region Old Code

			//// MD 1/17/12 - 12.1 - Cell Format Updates
			////WorkbookFontData defaultFontData = ((WorkbookFontData)this.Workbook.Fonts.DefaultElement).ResolvedFontData();
			//WorkbookFontData defaultFontData = ((WorkbookFontData)this.Workbook.Fonts.DefaultElement).ResolvedFontData(null);
			//
			//// Add the defaut font 4 times
			//for (int i = 0; i < 4; ++i)
			//    this.Fonts.Add((WorkbookFontData)defaultFontData.Clone());
			//
			//// There is never an font with index 5, so put a null in the collection so we never get 5 when doing an IndexOf
			//this.Fonts.Add(null);

			#endregion // Old Code
			WorksheetCellFormatData normalStyle = this.Workbook.Styles.NormalStyle.StyleFormatInternal;
			this.AddFont(normalStyle.FontInternal, normalStyle);

			this.Fonts.Add(this.Fonts[0]);
			this.Fonts.Add(this.Fonts[0]);
			this.Fonts.Add(this.Fonts[0]);
			this.Fonts.Add(null);
        }

        #endregion InitializeFonts

		// MD 9/10/08 - Cell Comments
		#region OnlyAssignShapeIdsToCommentShapes






		public override bool OnlyAssignShapeIdsToCommentShapes
		{
			get { return false; }
		} 

		#endregion OnlyAssignShapeIdsToCommentShapes

		// MD 4/28/11 - TFS62775
		#region PrepareShapeForSerialization



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public override void PrepareShapeForSerialization(ref WorksheetShape shape)
		{
			base.PrepareShapeForSerialization(ref shape);

			// MD 10/10/11 - TFS90805
			if (shape == null)
				return;

			// MD 10/10/11 - TFS90805
			if (shape == null || shape.Type2003.HasValue == false)
			{
				shape = null;
				return;
			}

			WorksheetChart chart = shape as WorksheetChart;
			if (chart != null)
			{
				if (chart.Excel2003RoundTripData == null)
					shape = null;

				return;
			}
		} 

		#endregion  // PrepareShapeForSerialization

		#region SaveWorkbookContents






		protected override void SaveWorkbookContents( bool workbookHasShapes )
		{
			// Write the workbook globals section of the biff stream and store the absolute stream positions of the 
			// BOUNDSHEET records: we will need to come back to them later to write the worksheet positions
			this.boundSheetStreamPositions = new List<uint>();
			this.WriteWorkbookGlobals( workbookHasShapes );

			// Write each worksheet section in the biff stream and store the absole stream position of each worksheet
			List<uint> worksheetStreamPositions = new List<uint>();
			foreach ( Worksheet worksheet in this.Workbook.Worksheets )
			{
				// Save the position of the worksheet stream, we will eventually return it
				uint worksheetPosition = (uint)this.workbookStream.Position;

				this.WriteWorksheetRecords( worksheet, workbookHasShapes );
				worksheetStreamPositions.Add( worksheetPosition );
			}

			Debug.Assert( worksheetStreamPositions.Count == this.boundSheetStreamPositions.Count );

			// Go back to each BOUNDSHEET record and store the absolute stream position of its associated worksheet
			for ( int i = 0; i < worksheetStreamPositions.Count; i++ )
			{
				// Move the stream back to the beginning of the BOUNDSHEET record
				this.workbookStream.Position = this.boundSheetStreamPositions[ i ];

				using ( Biff8RecordStream stream = new Biff8RecordStream( this ) )
				{
					Debug.Assert( stream.RecordType == BIFF8RecordType.BOUNDSHEET );

					// Write the associated worksheet's position in the biff stream
					stream.Write( worksheetStreamPositions[ i ] );
				}
			}

			this.boundSheetStreamPositions = null;
		}

		#endregion SaveWorkbookContents

		#region WriteCellArrayFormula

		protected override void WriteCellArrayFormula( bool isMasterCell )
		{
			this.WriteRecord( BIFF8RecordType.FORMULA );

			// If this is the top-left cell, save the ARRAY record with the actual array formula
			if ( isMasterCell )
				this.WriteRecord( BIFF8RecordType.ARRAY );
		} 

		#endregion WriteCellArrayFormula

		#region WriteCellBlank

		protected override void WriteCellBlank()
		{
			this.WriteRecord( BIFF8RecordType.BLANK );
		} 

		#endregion WriteCellBlank

		#region WriteCellBoolean

		protected override void WriteCellBoolean()
		{
			this.WriteRecord( BIFF8RecordType.BOOLERR );
		} 

		#endregion WriteCellBoolean

		#region WriteCellDataTable

		protected override void WriteCellDataTable( bool isMasterCell )
		{
			this.WriteRecord( BIFF8RecordType.FORMULA );

			// If this is the top-left cell of the data table interior, save the TABLE record
			if ( isMasterCell )
				this.WriteRecord( BIFF8RecordType.TABLE );
		} 

		#endregion WriteCellDataTable

		#region WriteCellError

		protected override void WriteCellError()
		{
			this.WriteRecord( BIFF8RecordType.BOOLERR );
		} 

		#endregion WriteCellError

		#region WriteCellFormattedString

		protected override void WriteCellFormattedString()
		{
			this.WriteRecord( BIFF8RecordType.LABELSST );
		} 

		#endregion WriteCellFormattedString

		#region WriteCellFormula

		protected override void WriteCellFormula()
		{
			this.WriteRecord( BIFF8RecordType.FORMULA );
		} 

		#endregion WriteCellFormula

		#region WriteMultipleCellBlanks

		protected override bool WriteMultipleCellBlanks()
		{
			this.WriteRecord( BIFF8RecordType.MULBLANK );
			return true;
		} 

		#endregion WriteMultipleCellBlanks

		#region WriteMultipleCellRKs

		protected override bool WriteMultipleCellRKs()
		{
			this.WriteRecord( BIFF8RecordType.MULRK );
			return true;
		} 

		#endregion WriteMultipleCellRKs

		#region WriteCellNumber

		protected override void WriteCellNumber()
		{
			this.WriteRecord( BIFF8RecordType.NUMBER );
		} 

		#endregion WriteCellNumber

		#region WriteCellRK

		protected override bool WriteCellRK()
		{
			this.WriteRecord( BIFF8RecordType.RK );
			return true;
		} 

		#endregion WriteCellRK

		#region WriteColumnGroup

		protected override void WriteColumnGroup()
		{
			this.WriteRecord( BIFF8RecordType.COLINFO );
		} 

		#endregion WriteColumnGroup

		#region WriteCustomViewWorkbookData

		protected override void WriteCustomViewWorkbookData()
		{
			this.WriteRecord( BIFF8RecordType.USERBVIEW );
		} 

		#endregion WriteCustomViewWorkbookData

		#region WriteCustomViewWorksheetData

		protected override void WriteCustomViewWorksheetData( bool savePrintOptions )
		{
			this.WriteRecord( BIFF8RecordType.USERSVIEWBEGIN );
			this.WriteRecord( BIFF8RecordType.SELECTION );

			// Write out the print options if they are valid
			if ( savePrintOptions )
			{
				// MD 5/25/11 - Data Validations / Page Breaks
				// -------------------------------------------------------
				PrintOptions printOptions = (PrintOptions)this.ContextStack[typeof(PrintOptions)];

				if (printOptions.HasHorizontalPageBreaks)
					this.WriteRecord(BIFF8RecordType.HORIZONTALPAGEBREAKS);

				if (printOptions.HasVerticalPageBreaks)
					this.WriteRecord(BIFF8RecordType.VERTICALPAGEBREAKS);
				// -------------------------------------------------------

				this.WriteRecord( BIFF8RecordType.HEADER );
				this.WriteRecord( BIFF8RecordType.FOOTER );
				this.WriteRecord( BIFF8RecordType.HCENTER );
				this.WriteRecord( BIFF8RecordType.VCENTER );
				this.WriteRecord( BIFF8RecordType.LEFTMARGIN );
				this.WriteRecord( BIFF8RecordType.RIGHTMARGIN );
				this.WriteRecord( BIFF8RecordType.TOPMARGIN );
				this.WriteRecord( BIFF8RecordType.BOTTOMMARGIN );
				this.WriteRecord( BIFF8RecordType.SETUP );
			}

			this.WriteRecord( BIFF8RecordType.USERSVIEWEND );
		} 

		#endregion WriteCustomViewWorksheetData

		#region WriteMagnificationLevel

		protected override void WriteMagnificationLevel()
		{
			this.WriteRecord( BIFF8RecordType.SCL );
		} 

		#endregion WriteMagnificationLevel

		#region WriteNamedReference

		protected override void WriteNamedReference( bool namedReferenceHasComment )
		{
			this.WriteRecord( BIFF8RecordType.NAME );

			// If the name has a valid comment, we need to write a NAMEEXT record for it
			if ( namedReferenceHasComment )
				this.WriteRecord( BIFF8RecordType.NAMEEXT );
		} 

		#endregion WriteNamedReference

		#region WritePaneInformation

		protected override void WritePaneInformation()
		{
			this.WriteRecord( BIFF8RecordType.PANE );
		} 

		#endregion WritePaneInformation

		#region WriteStandardFormat

		protected override void WriteStandardFormat()
		{
			this.WriteRecord( BIFF8RecordType.FORMAT );
		} 

		#endregion WriteStandardFormat

		// MD 11/29/11 - TFS96205
		// This is no longer needed.
		#region Removed

		//#region WriteWorkbookCellFormat

		//protected override void WriteWorkbookCellFormat()
		//{
		//    this.WriteRecord( BIFF8RecordType.XF );
		//} 

		//#endregion WriteWorkbookCellFormat

		#endregion  // Removed

		#region WriteWorkbookFont

		protected override void WriteWorkbookFont()
		{
			this.WriteRecord( BIFF8RecordType.FONT );
		} 

		#endregion WriteWorkbookFont

		#region WriteWorkbookGlobalData

		// http://msdn.microsoft.com/en-us/library/dd952177(v=office.12).aspx
		protected override void WriteWorkbookGlobalData( bool hasShapes )
		{
			this.WriteRecord( BIFF8RecordType.BOF );

			// MD 5/7/10 - 10.2 - Excel Templates
			if (this.Workbook.CurrentFormat == WorkbookFormat.Excel97To2003Template)
				this.WriteRecord(BIFF8RecordType.TEMPLATE);

			// INTERFACEHDR
			// MMS
			// INTERFACEEND
			// WRITEACCESS
			this.WriteRecord( BIFF8RecordType.CODEPAGE );
			this.WriteRecord( BIFF8RecordType.DSF );

			// MD 1/26/12 - 12.1 - Cell Format Updates
			this.WriteRecord(BIFF8RecordType.EXCEL9FILE);

			this.WriteRecord( BIFF8RecordType.TABID );

			// MD 10/1/08 - TFS8471
			// The object name might be set if it was read in as a 2007 workbook and converted to 2003, so make sure we only do this 
			// when 2003 VBA data is stored.
			//// MD 10/1/08 - TFS8453
			//if ( this.Workbook.VBAObjectName != null )
			if ( this.Workbook.Has2003VBACode )
			{
				this.WriteRecord( BIFF8RecordType.OBPROJ );
				this.WriteRecord( BIFF8RecordType.VBAOBJECTNAME );
			}

			this.WriteRecord( BIFF8RecordType.FNGROUPCOUNT );
			// WINDOWPROTECT
			this.WriteRecord( BIFF8RecordType.PROTECT );
			// PASSWORD
			// PROT4REV
			// PROT4REVPASS
			this.WriteRecord( BIFF8RecordType.WINDOW1 );
			this.WriteRecord( BIFF8RecordType.BACKUP );
			this.WriteRecord( BIFF8RecordType.HIDEOBJ );
			this.WriteRecord( BIFF8RecordType.Record1904 );
			this.WriteRecord( BIFF8RecordType.PRECISION );
			this.WriteRecord( BIFF8RecordType.REFRESHALL );
			this.WriteRecord( BIFF8RecordType.BOOKBOOL );

			// FONT record
			this.WriteWorkbookFonts();

			// FORMAT records
			this.WriteWorkbookNumberFormats();

			// XF records
			// MD 11/29/11 - TFS96205
			// ---------------------------------------------------
			// We need to do something more specialized here, so don't call into the base WriteWorkbookCellFormats method.
			//this.WriteWorkbookCellFormats();
			long xfRecordsStart = this.workbookStream.Position;

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Rewrote this loop so we can push XFContext instances.
			//List<WorksheetCellFormatData> formatsWithRoundTripData = new List<WorksheetCellFormatData>();
			//foreach (WorksheetCellFormatData format in this.Formats)
			//{
			//    this.ContextStack.Push(format);
			//    this.WriteRecord(BIFF8RecordType.XF);
			//    this.ContextStack.Pop(); // format
			//
			//    if (format.HasRoundTripData)
			//        formatsWithRoundTripData.Add(format);
			//}
			List<XFRecord.XFContext> formatsWithRoundTripData = new List<XFRecord.XFContext>();
			for (ushort i = 0; i < this.Formats.Count; i++)
			{
				WorksheetCellFormatData format = this.Formats[i];
				XFRecord.XFContext xfContext = new XFRecord.XFContext(format, i, format.GetXFEXTProps());

				this.ContextStack.Push(xfContext);
				this.WriteRecord(BIFF8RecordType.XF);
				this.ContextStack.Pop(); // xfContext

				if (xfContext.ExtProps.Count != 0)
					formatsWithRoundTripData.Add(xfContext);
			}

			this.CacheXFRecordData(xfRecordsStart);

			if (formatsWithRoundTripData.Count != 0)
			{
				this.WriteRecord(BIFF8RecordType.XFCRC);

				// MD 1/10/12 - 12.1 - Cell Format Updates
				// Rewrote this loop so we can push XFContext instances.
				//foreach (WorksheetCellFormatData format in formatsWithRoundTripData)
				//{
				//    this.ContextStack.Push(format);
				//    this.WriteRecord(BIFF8RecordType.XFEXT);
				//    this.ContextStack.Pop(); // format
				//}
				foreach (XFRecord.XFContext xfContext in formatsWithRoundTripData)
				{
					this.ContextStack.Push(xfContext);
					this.WriteRecord(BIFF8RecordType.XFEXT);
					this.ContextStack.Pop(); // xfContext
				}
			}
			// --------------- End of TFS96205 fix ----------------

			// MD 2/21/12 - 12.1 - Table Support
			foreach (WorksheetCellFormatData dxf in this.Dxfs)
			{
				this.ContextStack.Push(dxf);
				this.WriteRecord(BIFF8RecordType.DXF);
				this.ContextStack.Pop(); // dxf
			}

			// STYLE records
			this.WriteWorkbookStyles();

			// MD 2/19/12 - 12.1 - Table Support
			this.WriteRecord(BIFF8RecordType.TABLESTYLES);
			foreach (WorksheetTableStyle customStyle in this.Workbook.CustomTableStyles)
				this.WriteWorksheetTableStyle(customStyle);

			// USERBVIEW records
			this.WriteCustomViews();

			// MD 1/16/12 - 12.1 - Cell Format Updates
			//if ( this.Workbook.Palette.PaletteMode == WorkbookPaletteMode.CustomPalette )
			if (this.Workbook.Palette.IsCustom)
				this.WriteRecord( BIFF8RecordType.PALETTE );

			this.WriteRecord( BIFF8RecordType.USESELFS );

			// BOUNDSHEET records
			this.boundSheetStreamPositions = this.WriteBoundSheetRecords();

			this.WriteRecord( BIFF8RecordType.COUNTRY );

			// SUPBOOK, EXTERNNAME, and EXTERNSHEET records
			this.WriteExternalReferences();

			// NAME and NAMEEXT records
			this.WriteNamedReferences();

			if ( hasShapes )
				this.WriteRecord( BIFF8RecordType.MSODRAWINGGROUP );

			this.WriteRecord( BIFF8RecordType.SST );
			this.WriteRecord( BIFF8RecordType.EXTSST );
			// BOOKEXT

			// MD 11/29/11 - TFS96205
			this.WriteRecord(BIFF8RecordType.THEME);

			this.WriteRecord( BIFF8RecordType.EOF );
		}

		#endregion WriteWorkbookGlobalData

		#region WriteWorkbookStyle

		protected override void WriteWorkbookStyle()
		{
			this.WriteRecord( BIFF8RecordType.STYLE );

			// MD 3/22/12 - TFS104630
			// We may have to also write out a STYLEEXT record.
			WorkbookStyle style = this.ContextStack.Get<WorkbookStyle>();
			if (style.StyleFormatInternal.GetXFProps().Count != 0)
				this.WriteRecord(BIFF8RecordType.STYLEEXT);
		} 

		#endregion WriteWorkbookStyle

		#region WriteWorksheet

		protected override void WriteWorksheet( Worksheet worksheet, bool hasShapes )
		{
			this.WriteRecord( BIFF8RecordType.BOF );

			// Save the position of the INDEX record, we need to come back later and write more data
			long indexRecordPosition = this.workbookStream.Position;
			this.WriteRecord( BIFF8RecordType.INDEX );

			this.WriteRecord( BIFF8RecordType.CALCMODE );
			this.WriteRecord( BIFF8RecordType.CALCCOUNT );
			this.WriteRecord( BIFF8RecordType.REFMODE );
			this.WriteRecord( BIFF8RecordType.ITERATION );
			this.WriteRecord( BIFF8RecordType.DELTA );
			this.WriteRecord( BIFF8RecordType.SAVERECALC );
			this.WriteRecord( BIFF8RecordType.PRINTHEADERS );
			this.WriteRecord( BIFF8RecordType.PRINTGRIDLINES );
			this.WriteRecord( BIFF8RecordType.GRIDSET );
			this.WriteRecord( BIFF8RecordType.GUTS );
			this.WriteRecord( BIFF8RecordType.DEFAULTROWHEIGHT );
			this.WriteRecord( BIFF8RecordType.WSBOOL );

			// MD 2/1/11 - Page Break support
			if (worksheet.PrintOptions.HasHorizontalPageBreaks)
				this.WriteRecord(BIFF8RecordType.HORIZONTALPAGEBREAKS);

			// MD 2/1/11 - Page Break support
			if (worksheet.PrintOptions.HasVerticalPageBreaks)
				this.WriteRecord(BIFF8RecordType.VERTICALPAGEBREAKS);

			this.WriteRecord( BIFF8RecordType.HEADER );
			this.WriteRecord( BIFF8RecordType.FOOTER );
			this.WriteRecord( BIFF8RecordType.HCENTER );
			this.WriteRecord( BIFF8RecordType.VCENTER );
			this.WriteRecord( BIFF8RecordType.LEFTMARGIN );
			this.WriteRecord( BIFF8RecordType.RIGHTMARGIN );
			this.WriteRecord( BIFF8RecordType.TOPMARGIN );
			this.WriteRecord( BIFF8RecordType.BOTTOMMARGIN );
			this.WriteRecord( BIFF8RecordType.SETUP );

			if ( worksheet.ImageBackground != null )
				this.WriteRecord( BIFF8RecordType.BITMAP );

			// MD 10/4/07 - BR26953
			// Locked cells on protected worksheets were still editable. 
			// The workbook and the worksheet have to be marked as protected.
			if ( worksheet.Protected )
				this.WriteRecord( BIFF8RecordType.PROTECT );

			// Save the position of the DEFCOLWIDTH record, we need to write in in the INDEX record above
			long defColWidthAddress = this.workbookStream.Position;
			this.WriteRecord( BIFF8RecordType.DEFCOLWIDTH );

			// COLINFO record
			this.WriteColumnRecords( worksheet );

			this.WriteRecord( BIFF8RecordType.DIMENSIONS );

			// ROW, cell value, and DBCELL records
			List<uint> dbcellRecordPositions;
			this.WriteWorksheetRowBlocks( worksheet, out dbcellRecordPositions );

			this.CorrectINDEXRecord( indexRecordPosition, defColWidthAddress, dbcellRecordPositions );

			if ( hasShapes )
				this.WriteRecord( BIFF8RecordType.MSODRAWING );

			// MD 7/20/2007 - BR25039
			// Write the comments for the worksheet
			// NOTE records
			this.WriteWorksheetCellComments( worksheet );

			this.WriteRecord( BIFF8RecordType.WINDOW2 );
			this.WriteRecord( BIFF8RecordType.PAGELAYOUTINFO );

			// PANE record
			this.WritePaneInformation( worksheet );

			// SCL record
			this.WriteMagnificationLevels( worksheet );

			this.WriteRecord( BIFF8RecordType.SELECTION );

			// Custom view records
			this.WriteCustomViewOptions( worksheet );

			// MD 2/10/12 - TFS97827
			// Writing out the STANDARDWIDTH record gives a more accurate default column width specification than the DEFCOLWIDTH record.
			// MD 3/21/12 - TFS104630
			// If the STANDARDWIDTH record was not loaded from a BIFF8 file, we don't want to add one to the round-tripped file.
			//this.WriteRecord(BIFF8RecordType.STANDARDWIDTH);
			if (worksheet.ShouldSaveDefaultColumnWidths256th)
				this.WriteRecord(BIFF8RecordType.STANDARDWIDTH);

			if ( worksheet.HasMergedCellRegions )
				this.WriteRecord( BIFF8RecordType.MERGEDCELLS );

			// MD 2/1/11 - Data Validation support
			//// MD 9/12/08 - TFS6887
			//// Write out the data validation round trip data if it exists.
			//if ( worksheet.DataValidationInfo2003 != null )
			//{
			//    this.WriteRecord( BIFF8RecordType.DVAL );
			//
			//    foreach ( DataValidationCriteria criteria in worksheet.DataValidationInfo2003.DataValidations )
			//    {
			//        this.ContextStack.Push( criteria );
			//        this.WriteRecord( BIFF8RecordType.DV );
			//        this.ContextStack.Pop(); // criteria
			//    }
			//}
			if ( worksheet.HasDataValidationRules )
			{
				this.WriteRecord( BIFF8RecordType.DVAL );

				foreach ( KeyValuePair<DataValidationRule, WorksheetReferenceCollection> ruleReferencesPair in worksheet.DataValidationRules )
				{
					this.ContextStack.Push( ruleReferencesPair );
					this.WriteRecord( BIFF8RecordType.DV );
					this.ContextStack.Pop(); // ruleReferencesPair
				}
			}

			// MD 10/1/08 - TFS8471
			// The object name might be set if it was read in as a 2007 workbook and converted to 2003, so make sure we only do this 
			// when 2003 VBA data is stored.
			//// MD 10/1/08 - TFS8453
			//if (  worksheet.VBAObjectName != null )
			if ( this.Workbook.Has2003VBACode && worksheet.VBAObjectName != null )
				this.WriteRecord( BIFF8RecordType.VBAOBJECTNAME );

			// MD 1/27/12 - 12.1 - Cell Format Updates
			//if ( worksheet.DisplayOptions.TabColorIndex != WorkbookColorCollection.AutomaticColor )
			if (worksheet.DisplayOptions.TabColorInfo != null)
				this.WriteRecord(BIFF8RecordType.SHEETEXT);

			// MD 2/19/12 - 12.1 - Table Support
			this.WriteTables(worksheet);

			this.WriteRecord( BIFF8RecordType.EOF );
		}

		#endregion WriteWorksheet

		#region WriteWorksheetCellComment

		protected override void WriteWorksheetCellComment()
		{
			this.WriteRecord( BIFF8RecordType.NOTE );
		} 

		#endregion WriteWorksheetCellComment

		#endregion Base Class Overrides

		#region Methods

		#region Public Methods

		// MD 7/9/10 - TFS34476
		#region GetDefaultRowHeight

		public int GetDefaultRowHeight(string fontName, int fontHeight)
		{
			if (this.defaultRowHeights == null)
				this.defaultRowHeights = new Dictionary<FontInfo, int>();

			FontInfo info = new FontInfo(fontName, fontHeight);

			int defaultRowHeight;
			if (this.defaultRowHeights.TryGetValue(info, out defaultRowHeight) == false)
			{
				defaultRowHeight = Worksheet.GetDefaultRowHeight(fontName, fontHeight);
				this.defaultRowHeights[info] = defaultRowHeight;
			}

			return defaultRowHeight;
		} 

		#endregion // GetDefaultRowHeight

		// MD 10/19/07
		// Found while fixing BR27421
		// This is not needed anymore
		#region Not Used

		//        #region GetFormatString
		//
		//#if DEBUG
		//        /// <summary>
		//        /// Gets the number format string with the specified index.
		//        /// </summary> 
		//#endif
		//        public string GetFormatString( int index )
		//        {
		//            if ( this.numberFormats.ContainsKey( index ) )
		//                return this.numberFormats[ index ];
		//
		//            string format;
		//
		//            switch ( index )
		//            {
		//                case 0: format = "General"; break;
		//                case 1: format = "0"; break;
		//                case 2: format = "0.00"; break;
		//                case 3: format = "#,##0"; break;
		//                case 4: format = "#,##0.00"; break;
		//                case 9: format = "0%"; break;
		//                case 10: format = "0.00%"; break;
		//                case 11: format = "0.00E+00"; break;
		//                case 12: format = "# ?/?"; break;
		//                case 13: format = "# ??/??"; break;
		//                case 14: format = "M/D/YY"; break;
		//                case 15: format = "D-MMM-YY"; break;
		//                case 16: format = "D-MMM"; break;
		//                case 17: format = "MMM-YY"; break;
		//                case 18: format = "h:mm AM/PM"; break;
		//                case 19: format = "h:mm:ss AM/PM"; break;
		//                case 20: format = "h:mm"; break;
		//                case 21: format = "h:mm:ss"; break;
		//                case 22: format = "M/D/YY h:mm"; break;
		//                case 37: format = "_(#,##0_);(#,##0)"; break;
		//                case 38: format = "_(#,##0_);[Red](#,##0)"; break;
		//                case 39: format = "_(#,##0.00_);(#,##0.00)"; break;
		//                case 40: format = "_(#,##0.00_);[Red](#,##0.00)"; break;
		//                case 45: format = "mm:ss"; break;
		//                case 46: format = "[h]:mm:ss"; break;
		//                case 47: format = "mm:ss.0"; break;
		//                case 48: format = "##0.0E+0"; break;
		//                case 49: format = "@"; break;
		//
		//                default:
        //                    Utilities.DebugFail( "Unknown number format" );
		//                    return null;
		//            }
		//
		//            this.numberFormats.Add( index, format );
		//            return format;
		//        }
		//
		//        #endregion GetFormatString

		#endregion Not Used

		// MD 2/19/12 - 12.1 - Table Support
		#region GetLoadedStyle

		public WorkbookStyle GetLoadedStyle(int styleIndex)
		{
			if (styleIndex != -1)
			{
				if (0 <= styleIndex && styleIndex < this.LoadedStyles.Count)
					return this.LoadedStyles[styleIndex];

				Utilities.DebugFail("Invalid style index.");
			}

			return null;
		}

		#endregion // GetLoadedStyle

		// MD 7/20/2007 - BR25039
		// Added helper method to load a specific record stream
		#region LoadRecord






		public void LoadRecord( Biff8RecordStream recordStream )
		{
			Biff8RecordStream oldRecordStream = this.currentRecordStream;
			this.currentRecordStream = recordStream;

			this.LoadCurrentRecord();

			this.currentRecordStream = oldRecordStream;
		}

		#endregion LoadRecord

		// MD 1/17/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 11/29/11 - TFS96205
		//#region OnThemesLoaded

		//public void OnThemesLoaded()
		//{
		//    if (this.extProps == null)
		//        return;

		//    // Once the themes are loaded, we can parse through the theme data.
		//    foreach (KeyValuePair<int, ExtProp[]> pair in this.extProps)
		//    {
		//        WorksheetCellFormatData formatData = this.Formats[pair.Key];
		//        ExtProp[] props = pair.Value;

		//        for (int i = 0; i < props.Length; i++)
		//            props[i].ApplyTo(this, formatData);
		//    }
		//}

		//#endregion  // OnThemesLoaded

		#endregion // Removed

		// MD 3/30/11 - TFS69969
		#region PrepareForDataFromExternalWorkbook

		public void PrepareForDataFromExternalWorkbook(ExternalWorkbookReference workbookReference)
		{
			this.currentExternalWorkbookReference = workbookReference;
		}

		#endregion // PrepareForDataFromExternalWorkbook

		// MD 3/30/11 - TFS69969
		#region PrepareForDataFromExternalWorksheetIndex

		public void PrepareForDataFromExternalWorksheetIndex(int index)
		{
			if (this.currentExternalWorkbookReference != null)
			{
				this.currentExternalWorksheetReference =
					(WorksheetReferenceExternal)this.currentExternalWorkbookReference.GetWorksheetReference(index);
			}
			else
			{
				this.currentExternalWorksheetReference = null;
			}
		}

		#endregion // PrepareForDataFromExternalWorksheetIndex

		// MD 10/7/10 - TFS36582
		#region UpdateWorksheetReferenceInCurrentWorkbook



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public void UpdateWorksheetReferenceInCurrentWorkbook(Worksheet worksheet)
		{
			foreach (WorkbookReferenceBase workbookRef in this.WorkbookReferences)
			{
				CurrentWorkbookReference currentWorkbookRef = workbookRef as CurrentWorkbookReference;

				if (currentWorkbookRef == null)
					continue;

				currentWorkbookRef.UpdateWorksheetReference(worksheet);
			}
		} 

		#endregion // UpdateWorksheetReferenceInCurrentWorkbook

		#endregion Public Methods

		#region Private Methods

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region AddStyle

		private void AddStyle(WorkbookStyle style)
		{
			if (style != null)
			{
				this.styleFormatXfIds.Add(style, (ushort)this.Formats.Count);
				this.AddFormat(style.StyleFormatInternal);
			}
			else
			{
				WorksheetCellFormatData styleFormat = new WorksheetCellFormatData(this.Workbook, WorksheetCellFormatType.StyleFormat);
				this.AddFormat(styleFormat);
			}
		}

		#endregion // AddStyle

		// MD 11/29/11 - TFS96205
		#region CacheXFRecordData

		private void CacheXFRecordData(long xfRecordsStart)
		{
			long pos = this.workbookStream.Position;

			try
			{
				byte[] xfRecordDataWithHeaders = new byte[(int)(this.workbookStream.Position - xfRecordsStart)];
				this.workbookStream.Position = xfRecordsStart;
				this.workbookStream.Read(xfRecordDataWithHeaders, 0, xfRecordDataWithHeaders.Length);
				
				const int HeaderSize = 4;
				const int XFRecordSize = 20;
				const int XFRecordSizeWithHeader = XFRecordSize + HeaderSize;

				int itemCount = xfRecordDataWithHeaders.Length / XFRecordSizeWithHeader;
				this.xfRecordData = new byte[itemCount * XFRecordSize];
				for (int i = 0; i < itemCount; i++)
				{
					int xfRecordStart = (i * XFRecordSizeWithHeader) + HeaderSize;
					int xfRecordDataStart = (i * XFRecordSize);
					Buffer.BlockCopy(xfRecordDataWithHeaders, xfRecordStart, this.xfRecordData, xfRecordDataStart, XFRecordSize);
				}
			}
			finally
			{
				this.workbookStream.Position = pos;
			}
		}

		#endregion  // CacheXFRecordData

		#region CorrectINDEXRecord



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void CorrectINDEXRecord( long indexRecordPosition, long defColWidthAddress, List<uint> dbcellRecordPositions )
		{
			long oldStreamPosition = this.workbookStream.Position;

			this.workbookStream.Position = indexRecordPosition;

			// MD 4/18/11
			// Found while fixing TFS62026
			// We were never disposing this stream.
			//Biff8RecordStream indexRecordStream = new Biff8RecordStream( this );
			//Debug.Assert( indexRecordStream.RecordType == BIFF8RecordType.INDEX );
			//
			//indexRecordStream.Position = INDEXRecord.PositionOfDefColWidthAddress;
			//indexRecordStream.Write( (uint)defColWidthAddress );
			//indexRecordStream.Position = INDEXRecord.PositionOfDBCellAddresses;
			//
			//foreach ( uint position in dbcellRecordPositions )
			//    indexRecordStream.Write( position );
			using (Biff8RecordStream indexRecordStream = new Biff8RecordStream(this))
			{
				Debug.Assert(indexRecordStream.RecordType == BIFF8RecordType.INDEX);

				indexRecordStream.Position = INDEXRecord.PositionOfDefColWidthAddress;
				indexRecordStream.Write((uint)defColWidthAddress);
				indexRecordStream.Position = INDEXRecord.PositionOfDBCellAddresses;

				foreach (uint position in dbcellRecordPositions)
					indexRecordStream.Write(position);
			}

			this.workbookStream.Position = oldStreamPosition;
		}

		#endregion CorrectINDEXRecord

		// MD 4/18/11 - TFS62026
		#region GetBiffRecord

		private Biff8RecordBase GetBiffRecord(BIFF8RecordType type)
		{
			// MD 4/18/11 - TFS62026
			// Use cached common record types so we don't have to do a dictionary lookup for them.
			switch (type)
			{
				case BIFF8RecordType.BOOLERR:
					return boolerrRecord;

				case BIFF8RecordType.BLANK:
					return blankRecord;

				case BIFF8RecordType.LABELSST:
					return labelsstRecord;

				case BIFF8RecordType.FORMULA:
					return formulaRecord;

				case BIFF8RecordType.NUMBER:
					return numberRecord;

				case BIFF8RecordType.RK:
					return rkRecord;

				case BIFF8RecordType.ROW:
					return rowRecord;
			}

			// MD 4/28/11
			// Found while fixing TFS62775
			// Pass in the type parameter that came into the method.
			//return (Biff8RecordBase)Biff8RecordBase.GetBiffRecord(this.currentRecordStream.RecordType, Biff8RecordBase.CreateBiffRecordCallback);
			return (Biff8RecordBase)Biff8RecordBase.GetBiffRecord(type, Biff8RecordBase.CreateBiffRecordCallback);
		} 

		#endregion // GetBiffRecord

		// MD 10/30/11 - TFS90733
		#region GetsShapeIdsAndRadioButtons

		private static void GetsShapeIdsAndRadioButtons(WorksheetShapeCollection shapes, Dictionary<ushort, WorksheetShape> shapeByObjIds, List<FtRboData> radioButtonsRequiringFixup)
		{
			foreach (WorksheetShape shape in shapes)
			{
				WorksheetShapeGroup group = shape as WorksheetShapeGroup;
				if (group != null)
					BIFF8WorkbookSerializationManager.GetsShapeIdsAndRadioButtons(group.Shapes, shapeByObjIds, radioButtonsRequiringFixup);

				if (shape.Obj == null || shape.Obj.Cmo == null)
					continue;

				shapeByObjIds[shape.Obj.Cmo.Id] = shape;

				if (shape.Obj.RadioButton != null)
					radioButtonsRequiringFixup.Add(shape.Obj.RadioButton);
			}
		}

		#endregion // GetsShapeIdsAndRadioButtons

		// MD 7/20/2007 - BR25039
		// Moved code from Load to this helper method
		#region LoadCurrentRecord






		private void LoadCurrentRecord()
		{
			// Get the record class to load the record
			// MD 4/18/11 - TFS62026
			// Use a helper method which is slightly faster.
			//Biff8RecordBase record = (Biff8RecordBase)Biff8RecordBase.GetBiffRecord( this.currentRecordStream.RecordType, Biff8RecordBase.CreateBiffRecord );
			Biff8RecordBase record = this.GetBiffRecord(this.currentRecordStream.RecordType);

			if ( record != null )
			{
				Debug.Assert( record.Type == this.currentRecordStream.RecordType, "The type of the record doesn't match the record id" );

				// MD 1/14/09
				// Found while fixing TFS12404
				// This was moved below the output of the record, because loading a record may load other records before it returns
				// and then the order of the records will be incorrect in the output and could cause confusion when debugging.
				//// load the record's data
				//record.Load( this );



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


				// MD 1/14/09
				// Found while fixing TFS12404
				// This was moved below the output of the record, because loading a record may load other records before it returns
				// and then the order of the records will be incorrect in the output and could cause confusion when debugging.
				// load the record's data
				record.Load( this );

				this.currentRecordStream.Close();
			}
		}

		#endregion LoadCurrentRecord

		// MD 2/20/12 - 12.1 - Table Support
		#region ShouldWriteFEAT12Record

		private static bool ShouldWriteFEAT12Record(WorksheetTable table)
		{
			// See this for rules about when FEAT12 is needed: http://msdn.microsoft.com/en-us/library/dd946316(v=office.12).aspx

			if (table.IsHeaderRowVisible == false) // This isn't the case when fSingleCell is True (only occurs with XML tables).
				return true;

			for (int i = 0; i < table.Columns.Count; i++)
			{
				WorksheetTableColumn column = table.Columns[i];
				if (column.TotalFormula != null || column.TotalLabel != null)
					return true;
			}

			return false;
		}

		#endregion // ShouldWriteFEAT12Record

		#region WriteBoundSheetRecords






		private List<uint> WriteBoundSheetRecords()
		{
			List<uint> boundSheetRecordPositions = new List<uint>();

			foreach ( Worksheet worksheet in this.Workbook.Worksheets )
			{
				boundSheetRecordPositions.Add( (uint)this.workbookStream.Position );

				this.ContextStack.Push( worksheet );
				this.WriteRecord( BIFF8RecordType.BOUNDSHEET );
				this.ContextStack.Pop(); // worksheet
			}

			return boundSheetRecordPositions;
		}

		#endregion WriteBoundSheetRecords

		#region WriteExternalReferences






		private void WriteExternalReferences()
		{
			if ( this.WorkbookReferences.Count == 0 )
				return;

			// Save a SUPBOOK record for each external or 3D workbook reference
			foreach ( WorkbookReferenceBase workbookReference in this.WorkbookReferences )
			{
				this.ContextStack.Push( workbookReference );
				this.WriteRecord( BIFF8RecordType.SUPBOOK );

				// If it is an external workbook, save an EXTERNNAME for each named reference
				// MD 10/8/07 - BR27172
				// There are other reference types now, and we only didn't want to do this for the current workbook
				//if ( workbookReference is ExternalWorkbookReference )
				if ( ( workbookReference is CurrentWorkbookReference ) == false )
				{
					foreach ( NamedReferenceBase namedReferenceBase in workbookReference.NamedReferences )
					{
						// MD 10/8/07 - BR27172
						// There are other named reference types now, changed the condition to be more accurate
						//Debug.Assert( namedReferenceBase is ExternalNamedReference );
						Debug.Assert( ( namedReferenceBase is NamedReference ) == false );

						this.ContextStack.Push( namedReferenceBase );
						this.WriteRecord( BIFF8RecordType.EXTERNNAME );
						this.ContextStack.Pop(); // namedReferenceBase
					}
				}

				this.ContextStack.Pop(); // workbookReference
			}

			// Save an the EXTERNSHEET which contains information about the worksheets in each workbook reference
			this.WriteRecord( BIFF8RecordType.EXTERNSHEET );
		}

		#endregion WriteExternalReferences

		#region WriteRecord






		// MD 7/20/2007 - BR25039
		// Made public to be used externally
		//private void WriteRecord( BIFFType type )
		public void WriteRecord( BIFF8RecordType type )
		{
			// MD 7/20/2007 - BR25039
			// This condition is no longer always true
			//Debug.Assert( this.currentRecordStream == null );

			// MD 7/20/2007 - BR25039
			// Save the old record stream so we can restore it later
			Biff8RecordStream oldRecordStream = this.currentRecordStream;

			// Create a record stream to write the record
			using ( this.currentRecordStream = new Biff8RecordStream( this, type ) )
			{
				// MD 7/20/2007 - BR25039
				// If the old record stream is valid, the one being written now should be a sub record of it
				if ( oldRecordStream != null )
					oldRecordStream.AddSubStream( this.currentRecordStream );

				// MD 4/18/11 - TFS62026
				// Use a helper method which is slightly faster.
				//Biff8RecordBase.GetBiffRecord( type, Biff8RecordBase.CreateBiffRecord ).Save( this );
				this.GetBiffRecord(type).Save(this);

				// MD 7/20/2007 - BR25039
				// Added debug code to output record


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


			}

			// MD 7/20/2007 - BR25039
			// Restore the old record stream
			//this.currentRecordStream = null;
			this.currentRecordStream = oldRecordStream;
		}

		#endregion WriteRecord

		// MD 2/20/12 - 12.1 - Table Support
		#region WriteTables

		private void WriteTables(Worksheet worksheet)
		{
			if (worksheet.Tables.Count == 0)
				return;

			this.WriteRecord(BIFF8RecordType.FEATHEADR11);

			foreach (WorksheetTable table in worksheet.Tables)
			{
				SortedList<int, TableColumnFilterData> columnsFilterData = new SortedList<int, TableColumnFilterData>();
				for (int i = 0; i < table.Columns.Count; i++)
				{
					WorksheetTableColumn column = table.Columns[i];
					TableColumnFilterData filterData = new TableColumnFilterData(column);
					if (filterData.ShouldSaveIn2003Formats)
						columnsFilterData.Add(column.Index, filterData);
				}

				this.ContextStack.Push(table);
				this.ContextStack.Push(columnsFilterData);

				if (BIFF8WorkbookSerializationManager.ShouldWriteFEAT12Record(table))
					this.WriteRecord(BIFF8RecordType.FEAT12);
				else
					this.WriteRecord(BIFF8RecordType.FEAT11);

				this.ContextStack.Push(LIST12Record.LIST12DataType.BlockLevelFormatting);
				this.WriteRecord(BIFF8RecordType.LIST12);
				this.ContextStack.Pop(); // LIST12DataType

				foreach (TableColumnFilterData filterData in columnsFilterData.Values)
				{
					if (filterData.NeedsAUTOFILTER12Record)
					{
						this.ContextStack.Push(filterData);
						this.WriteRecord(BIFF8RecordType.AUTOFILTER12);
						this.ContextStack.Pop(); // filterData
					}
				}

				this.ContextStack.Push(LIST12Record.LIST12DataType.StyleInfo);
				this.WriteRecord(BIFF8RecordType.LIST12);
				this.ContextStack.Pop(); // LIST12DataType

				this.ContextStack.Push(LIST12Record.LIST12DataType.DisplayName);
				this.WriteRecord(BIFF8RecordType.LIST12);
				this.ContextStack.Pop(); // LIST12DataType

				if (table.SortSettings.IsDirty)
					this.WriteRecord(BIFF8RecordType.SORTDATA12);

				this.ContextStack.Pop(); // columnsFilterData
				this.ContextStack.Pop(); // table
			}
		}

		#endregion // WriteTables

		#region WriteWorksheetRowBlock







		private uint WriteWorksheetRowBlock( List<WorksheetRow> rowsInBlock )
		{
			// Store the position of the first ROW record so it can be written in the DBCELL record later
			uint firstRowPosition = (uint)this.workbookStream.Position;

			// Write a ROW record for each row in the block
			foreach ( WorksheetRow row in rowsInBlock )
			{
				this.ContextStack.Push( row );

				// MD 7/26/10 - TFS34398
				WorksheetRowSerializationCache rowCache = this.RowSerializationCaches[row];
				this.ContextStack.Push(rowCache);

				this.WriteRecord( BIFF8RecordType.ROW );

				// MD 7/26/10 - TFS34398
				this.ContextStack.Pop(); // rowCache

				this.ContextStack.Pop(); // row
			}

			// We need to keep track of the positions of the first cell in each row
			List<uint> firstCellInRowPositions = new List<uint>();

			// MD 1/10/12 - 12.1 - Cell Format Updates
			WorksheetCellFormatData emptyFormat = this.Workbook.CellFormats.DefaultElement;

			foreach ( WorksheetRow row in rowsInBlock )
			{
				// MD 1/10/12 - 12.1 - Cell Format Updates
				WorksheetCellFormatData rowFormat = emptyFormat;
				if (row.HasCellFormat)
					rowFormat = row.CellFormatInternal.Element;

				// Store the position of the first cell in the row
				firstCellInRowPositions.Add( (uint)this.workbookStream.Position );

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//if ( row.HasCells == false )
				//    continue;

				// MD 4/18/11 - TFS62026
				// We don't need to push the row on the context stack anymore since the CellContext contains the row.
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects, so we need the row and column index on the context stack.
				//this.ContextStack.Push(row);

				// MD 4/18/11 - TFS62026
				// The cells need to row cache to get their format index values.
				WorksheetRowSerializationCache rowCache = this.RowSerializationCaches[row];

				// MD 4/18/11 - TFS62026
				// Create a cell context object which we will leave on the context stack for all cells.
				CellContext cellContext = new CellContext(row, rowCache);
				this.ContextStack.Push(cellContext);

				// MD 10/19/07
				// Found while fixing BR27421
				// Added support for multiple cell value records
				int ignoreUpToIndex = -1;

				// Write a cell record for each cell in the row
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//foreach ( WorksheetCell cell in row.Cells )
				foreach (CellDataContext cellDataContext in row.GetCellsWithData())
				{
					// MD 10/19/07
					// Found while fixing BR27421
					// Ignore new cells if they were already serialized by previous MUL records
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//if ( cell.ColumnIndex <= ignoreUpToIndex )
					if (cellDataContext.ColumnIndex <= ignoreUpToIndex)
						continue;

					// MD 1/10/12 - 12.1 - Cell Format Updates
					// Make sure the CellFormatData is always non-null.
					if (cellDataContext.CellFormatData == null)
						cellDataContext.CellFormatData = rowFormat;

					// MD 4/12/11 - TFS67084
					// We are only going to get cells that have data anyway now.
					//if ( cell.HasData == false )
					//    continue;

					// MD 10/19/07
					// Found while fixing BR27421
					// Added support for multiple cell value records
					//this.WriteCellRecord( cell );
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//ignoreUpToIndex = this.WriteCellRecord( cell );
					ignoreUpToIndex = this.WriteCellRecord(cellContext, cellDataContext);
				}

				// MD 4/12/11 - TFS67084
				this.ContextStack.Pop(); // cellContext
			}

			// Store the position of the DBCELL record, which will be written next
			uint dbCellPosition = (uint)this.workbookStream.Position;

			// Save the info needed for the DBCELL record and push it on the context stack
			DBCELLRecord.DBCELLInfo dbCellInfo = new DBCELLRecord.DBCELLInfo( firstRowPosition, firstCellInRowPositions );

			this.ContextStack.Push( dbCellInfo );
			this.WriteRecord( BIFF8RecordType.DBCELL );
			this.ContextStack.Pop(); // dbCellInfo

			// Return the position of the DBCELL record
			return dbCellPosition;
		}

		#endregion WriteWorksheetRowBlock

		#region WriteWorksheetRowBlocks



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private void WriteWorksheetRowBlocks( Worksheet worksheet, out List<uint> dbcellRecordPositions )
		{
			dbcellRecordPositions = new List<uint>();

			if ( worksheet.HasRows == false )
				return;

			List<WorksheetRow> rowsInBlock = new List<WorksheetRow>();

			int firstRowInBlock = -1;
			foreach ( WorksheetRow row in worksheet.Rows )
			{
				// MD 7/26/10 - TFS34398
				//if ( row.HasData == false )
				// MD 4/18/11 - TFS62026
				// Only rows which need to be written out will have a row cache now.
				//WorksheetRowSerializationCache rowCache = this.RowSerializationCaches[row];
				//if (rowCache.hasData == false)
				//    continue;
				WorksheetRowSerializationCache rowCache;
				if (this.RowSerializationCaches.TryGetValue(row, out rowCache) == false)
					continue;

				Debug.Assert(rowCache.hasData, "Only rows with data should have a row cache.");

				// If no rows have been saved yet, the current row is the first row in the row block
				if ( firstRowInBlock < 0 )
					firstRowInBlock = row.Index;

				// If this row is more than 32 rows away from the first row in the block, the current block has
				// to be saved and this row has to start a new row block
				if ( row.Index - firstRowInBlock > 31 )
				{
					dbcellRecordPositions.Add( this.WriteWorksheetRowBlock( rowsInBlock ) );

					// Reset the row block
					rowsInBlock.Clear();
					firstRowInBlock = row.Index;
				}

				// Add the current row to the row block
				rowsInBlock.Add( row );
			}

			// Save the last row block
			if ( rowsInBlock.Count > 0 )
				dbcellRecordPositions.Add( this.WriteWorksheetRowBlock( rowsInBlock ) );
		}

		#endregion WriteWorksheetRowBlocks

		// MD 2/22/12 - 12.1 - Table Support
		#region WrtieWorksheetTableStyle

		private void WriteWorksheetTableStyle(WorksheetTableStyle customStyle)
		{
			this.ContextStack.Push(customStyle);
			this.WriteRecord(BIFF8RecordType.TABLESTYLE);

			foreach (KeyValuePair<WorksheetTableStyleArea, uint> pair in customStyle.DxfIdsByAreaDuringSave)
			{
				this.ContextStack.Push(pair.Key);
				this.ContextStack.Push(pair.Value);
				this.WriteRecord(BIFF8RecordType.TABLESTYLEELEMENT);
				this.ContextStack.Pop(); // format
				this.ContextStack.Pop(); // area
			}

			this.ContextStack.Pop(); // customStyle
		}

		#endregion // WrtieWorksheetTableStyle

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region CurrentRecordStream

		public Biff8RecordStream CurrentRecordStream
		{
			get { return this.currentRecordStream; }
		}

		#endregion CurrentRecordStream

		// MD 3/30/11 - TFS69969
		#region CurrentWorksheetReference

		public WorksheetReferenceExternal CurrentExternalWorksheetReference
		{
			get { return this.currentExternalWorksheetReference; }
		}

		#endregion // CurrentWorksheetReference

		// MD 1/17/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 11/29/11 - TFS96205
		//#region ExtProps

		//public Dictionary<int, ExtProp[]> ExtProps
		//{
		//    get
		//    {
		//        if (this.extProps == null)
		//            this.extProps = new Dictionary<int, ExtProp[]>();

		//        return this.extProps;
		//    }
		//}

		//#endregion  // ExtProps

		#endregion // Removed

		// MD 11/3/10 - TFS49093
		// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
		// we are now storing it in a collection on the manager.
		#region EXTSSTData

		public List<EXTSSTItem> EXTSSTData
		{
			get { return this.extSSTData; }
		} 

		#endregion // EXTSSTData

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Moved this from the base manager because it is not needed by the Excel 2007 formats.
		#region Formats

		public List<WorksheetCellFormatData> Formats
		{
			get { return this.formats; }
		}

		#endregion Formats

		// MD 11/29/11 - TFS96205
		#region IgnoreXFEXTData

		public bool IgnoreXFEXTData
		{
			get { return this.ignoreXFEXTData; }
			set { this.ignoreXFEXTData = value; }
		}

		#endregion  // IgnoreXFEXTData

		// MD 11/8/11 - TFS85193
		// The comments which are loaded but not applied to cells need to be stored here.
		#region LoadedComments

		internal List<WorksheetCellComment> LoadedComments
		{
			get
			{
				if (this.loadedComments == null)
					this.loadedComments = new List<WorksheetCellComment>();

				return this.loadedComments;
			}
		}

		#endregion // LoadedComments

		// MD 2/19/12 - 12.1 - Table Support
		#region LoadedStyles

		public List<WorkbookStyle> LoadedStyles
		{
			get
			{
				if (this.loadedStyles == null)
					this.loadedStyles = new List<WorkbookStyle>();

				return this.loadedStyles;
			}
		}

		#endregion // LoadedStyles

		// MD 10/19/07
		// Found while fixing BR27421
		// This is not needed anymore
		//#region NumberFormats
		//
		//public Dictionary<int, string> NumberFormats
		//{
		//    get { return this.numberFormats; }
		//}
		//
		//#endregion NumberFormats

		// MD 11/10/11 - TFS85193
		#region PackageFactory

		public IPackageFactory PackageFactory
		{
			get { return this.packageFactory; }
		}

		#endregion  // PackageFactory

		// MD 5/26/11 - TFS76587
		#region PendingSharedFormulaRoots

		public Dictionary<WorksheetCellAddress, bool> PendingSharedFormulaRoots
		{
			get
			{
				if (this.pendingSharedFormulaRoots == null)
					this.pendingSharedFormulaRoots = new Dictionary<WorksheetCellAddress, bool>();

				return this.pendingSharedFormulaRoots;
			}
		}

		#endregion  // PendingSharedFormulaRoots

		// MD 3/30/10 - TFS30253
		#region SharedFormulas

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public Dictionary<WorksheetCell, Formula> SharedFormulas
		public Dictionary<WorksheetCellAddress, Formula> SharedFormulas
		{
			get
			{
				if (this.sharedFormulas == null)
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//this.sharedFormulas = new Dictionary<WorksheetCell, Formula>();
					this.sharedFormulas = new Dictionary<WorksheetCellAddress, Formula>();
				}

				return this.sharedFormulas;
			}
		}

		#endregion // SharedFormulas

		// MD 11/10/11 - TFS85193
		#region VerifyExcel2007Xml

		public bool VerifyExcel2007Xml
		{
			get { return this.verifyExcel2007Xml; }
		}

		#endregion  // VerifyExcel2007Xml

		#region WorkbookStream

		public Stream WorkbookStream
		{
			get { return this.workbookStream; }
		}

		#endregion WorkbookStream

		#region WorkbookStreamReader

		public BinaryReader WorkbookStreamReader
		{
			get
			{
				if ( this.workbookStreamReader == null )
					this.workbookStreamReader = new BinaryReader( this.workbookStream );

				return this.workbookStreamReader;
			}
		}

		#endregion WorkbookStreamReader

		// MD 11/29/11 - TFS96205
		#region XfRecordData

		public byte[] XfRecordData
		{
			get { return this.xfRecordData; }
		}

		#endregion  // XfRecordData

		#endregion Properties


		// MD 11/3/10 - TFS49093
		// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
		// we are now storing it in a collection on the manager.
		#region EXTSSTItem struct

		public struct EXTSSTItem
		{
			public long AbsolutePosition;
			public int OffsetInRecordBlock;
		}

		#endregion // EXTSSTItem struct

		// MD 7/9/10 - TFS34476
		#region FontInfo class

		private class FontInfo
		{
			private string fontName;
			private int fontHeight;

			public FontInfo(string fontName, int fontHeight)
			{
				this.fontName = fontName;
				this.fontHeight = fontHeight;
			}

			public override bool Equals(object obj)
			{
				FontInfo other = obj as FontInfo;

				if (other == null)
					return false;

				if (other.fontHeight != this.fontHeight)
					return false;

				if (String.Equals(other.fontName, this.fontName, StringComparison.Ordinal) == false)
					return false;

				return true;
			}

			public override int GetHashCode()
			{
				return this.fontName.GetHashCode() ^ this.fontHeight;
			}
		}

		#endregion // FontInfo class
	}

	// MD 3/16/12 - 12.1 - Table Support
	internal class TableColumnFilterData
	{
		public readonly IList<string> AllowedTextValues;
		public readonly WorksheetTableColumn Column;
		public readonly bool NeedsAUTOFILTER12Record;
		public readonly bool ShouldSaveIn2003Formats;

		public TableColumnFilterData(WorksheetTableColumn column)
		{
			this.Column = column;

			if (this.Column.Filter == null)
			{
				this.ShouldSaveIn2003Formats = false;
				return;
			}

			this.ShouldSaveIn2003Formats = this.Column.Filter.ShouldSaveIn2003Formats(
				out this.NeedsAUTOFILTER12Record, out this.AllowedTextValues);
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