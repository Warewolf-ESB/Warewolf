using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.BIFF8;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements;
using Infragistics.Documents.Excel.StructuredStorage;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

using System.Drawing;
using System.Windows.Forms;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
    /// <summary>
	/// Represents a Microsoft Excel workbook.
	/// </summary>
	/// <remarks>
	/// <p class="body">Every workbook consists of one or more worksheets (<see cref="Worksheet"/>). The default constructor creates an empty workbook.</p>
	/// </remarks>



	public

		 class Workbook
		// MD 2/29/12 - 12.1 - Table Support
		// This is no longer needed.
		//// MD 7/26/10 - TFS34398
		//// The workbook will now be the owner of the default cell format data and the cell format styles.
		//IWorksheetCellFormatProxyOwner
	{
        #region Constants

		/// <summary>
		/// Maximum number of rows in the worksheet allowed by the Excel 2007 file format.
		/// </summary>
		public const int MaxExcel2007RowCount = 1048576;

		/// <summary>
		/// Maximum number of columns in the worksheet allowed by the Excel 2007 file format.
		/// </summary>
		public const int MaxExcel2007ColumnCount = 16384;

		/// <summary>
		/// Maximum number of rows in the worksheet allowed by the Excel 97-2003 file format.
		/// </summary>
        public const int MaxExcelRowCount = 65536;

		/// <summary>
		/// Maximum number of columns in the worksheet allowed by the Excel 97-2003 file format.
		/// </summary>
		public const int MaxExcelColumnCount = 256;

		/// <summary>
		/// Obsolete.
		/// </summary>
		// MD 1/16/12 - 12.1 - Cell Format Updates
		//public const int MaxExcelColorCount = 56;
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("MaxExcelColorCount is no longer valid because there is no longer a limit on the number of colors that can be used in a Workbook.")]
		public const int MaxExcelColorCount = WorkbookColorPalette.UserPaletteSize;

		/// <summary>
		/// Maximum number of distinct cell formats in the workbook allowed by the Excel 97-2003 file format.
		/// </summary>
		public const int MaxExcelCellFormatCount = 4000;

        // MBS 7/15/08 - Excel 2007 Format
        /// <summary>
		/// Maximum number of distinct cell formats in the workbook allowed by the Excel 2007 file format.
        /// </summary>
        public const int MaxExcel2007CellFormatCount = 64000;
        
        /// <summary>
        /// Maximum number fonts in a workbook allowed by Excel.
        /// </summary>
        public const int MaxExcelWorkbookFonts = 512;

		// MD 1/11/08 - BR29105
		// Made this into a constant because it is not used in other places.
		// If this is made into a property, make sure the setter calls ResetZeroWidthCharacter, 
		// because it will no longer be valid.
		// MD 1/8/12 - 12.1 - Cell Format Updates
		// This is no longer needed now that the default font name is exposed off the normal style.
		//internal const string DefaultFontName = "Arial"; 

		// MD 10/1/08 - TFS8453
		private const string Excel2003StructuredStorageAlternateWorkbookFileName = "Book";
		internal const string Excel2003StructuredStorageDocumentSummaryInformationFileName = "DocumentSummaryInformation";
		internal const string Excel2003StructuredStorageSummaryInformationFileName = "SummaryInformation";
		private const string Excel2003StructuredStorageWorkbookFileName = "Workbook";

		// MD 2/24/12 - 12.1 - Table Support
		internal const WorkbookFormat LatestFormat = WorkbookFormat.Excel2007;
		private const int AverageSubtotalFunction = 101;
		private const int CountNumsSubtotalFunction = 102;
		private const int CountSubtotalFunction = 103;
		private const int MaxSubtotalFunction = 104;
		private const int MinSubtotalFunction = 105;
		private const int StdDevSubtotalFunction = 107;
		private const int SumSubtotalFunction = 109;
		private const int VarSubtotalFunction = 110;

		#endregion Constants

		#region Static Variables

		// MD 1/9/09 - TFS12270
		// Moved all culture specific information needed by the serailization logic to the Workbook.
		// These are formats and encodings which must be used in the output no matter what the machine's or thread's culture is.
        internal readonly static Encoding InvariantCompressedTextEncoding = Utilities.EncodingGetEncoding(1252);

		// MD 9/11/09 - TFS20376
		// The 1033 culture can still have the separators changed. The InvariantCulture's separators do not change.
		//internal readonly static IFormatProvider InvariantFormatProvider = new CultureInfo( 1033 );
		internal readonly static IFormatProvider InvariantFormatProvider = CultureInfo.InvariantCulture;

		#endregion Static Variables

		#region Member Variables

		// MD 10/1/08 - TFS8453
		private Dictionary<string, byte[]> cachedStructuredStorageFiles;

		// MD 7/14/08 - Excel formula solving
		private ExcelCalcEngine calcEngine;

		private CalculationMode calculationMode = CalculationMode.Automatic;

		// MD 8/18/08 - Excel formula solving
		private int calculationSuspensionCount;

		// MD 4/18/11 - TFS62026
		// The cellFormats collection is now a derived type.
		//private GenericCachedCollection<WorksheetCellFormatData> cellFormats;
		private WorksheetCellFormatCollection cellFormats;

		private CellReferenceMode cellReferenceMode = CellReferenceMode.A1;

		// MD 2/10/12 - TFS97827
		// In an effort to clean up and simplify some of the column measuring logic, I am removing this because it can easily be calculated.
		//// MD 7/23/10 - TFS35969
		//private double characterWidth256thsPerPixel;

		// MD 6/31/08 - Excel 2007 Format
		private WorkbookFormat currentFormat;

		private CustomViewCollection customViews;
		private DateSystem dateSystem;

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// This is no longer needed now that the default font height is exposed off the normal style.
		//private int defaultFontHeight;

		// MD 1/11/08 - BR29105
		private int defaultFontVersion;

		private DocumentProperties documentProperties;

		// MD 8/18/08 - Excel formula solving
		private Dictionary<ExcelRefBase, Formula> referencesRequiringFormulaCompilation;

		// MD 2/2/12 - TFS100573
		//private GenericCachedCollection<WorkbookFontData> fonts;
		private GenericCachedCollectionEx<WorkbookFontData> fonts;

		private WorkbookFormatCollection formats;

		// MD 10/9/07 - BR27172
		private List<NamedReference> hiddenNamedReferences;

		// MD 8/1/08 - BR35121
		private bool isLoading;

		private bool iterativeCalculationsEnabled;
		private double maxChangeInIteration = 0.001;
		private int maxRecursionIterations = 100;
		private NamedReferenceCollection namedReferences;

		// MD 1/16/12 - 12.1 - Cell Format Updates
		//private WorkbookColorCollection palette;
		private WorkbookColorPalette palette;

		// MD 8/22/08 - Excel formula solving - Performance
		private Dictionary<string, Formula> parsedR1C1Formulas;

		private Precision precision = Precision.UseRealCellValues;
		private bool protectedMbr;
		private bool recalculateBeforeSave = true;
		private bool refreshAll;
		private bool saveExternalLinkedValues = true;

		// MD 5/2/08 - BR32461/BR01870
		private bool shouldRemoveCarriageReturnsOnSave = true;

		private WorkbookStyleCollection styles;

		// MD 10/1/08 - TFS8471
		private byte[] vbaData2007;

		// MD 10/1/08 - TFS8453
		private string vbaObjectName;

		private WorkbookWindowOptions windowOptions;
		private WorksheetCollection worksheets;

		// MD 1/11/08 - BR29105
		private int zeroCharacterWidth;
		
		private List<PropertyTableBase.PropertyValue> drawingProperties1;
		private List<PropertyTableBase.PropertyValue> drawingProperties3;

        // MBS 9/10/08 - Excel 2007
        // We need to keep track of the last loaded path for use with formulas and defined names
        private string lastLoadingPath;

		// MD 10/8/10
		// Found while fixing TFS44359
		// Added support to round-trip custom Xml parts.
		private List<byte[]> customXmlParts;
		private List<byte[]> customXmlPropertiesParts;

		// MD 11/3/10 - TFS49093
		// Added the shared string table to the workbook so we don't have to generate it on each save.
		// MD 4/12/11 - TFS67084
		// Use a derived collection for the shared string table.
		//private GenericCachedCollection<FormattedStringElement> sharedStringTable;
		private SharedStringTable sharedStringTable;

		// MD 3/30/11 - TFS69969
		private Dictionary<string, ExternalWorkbookReference> externalWorkbooks;
		private WorkbookSerializationManager currentSerializationManager;

		// MD 11/29/11 - TFS96205
		private byte[] customThemeData;

		// MD 12/30/11 - 12.1 - Table Support
		private CustomTableStyleCollection customTableStyles;
		private WorksheetTableStyle defaultTableStyle;
		private Color[] themeColors;
		private bool validateFormatStrings;
		private Dictionary<string, NamedReferenceBase> workbookScopedNamedItems = new Dictionary<string, NamedReferenceBase>(StringComparer.CurrentCultureIgnoreCase);
		private uint nextTableId = 1;

		// MD 4/6/12 - TFS101506
		private CultureInfo culture;

		// MD 6/13/12 - CalcEngineRefactor
		private AddInFunctionsWorkbookReference addInFunctionsWorkbookReference;
		private CurrentWorkbookReference currentWorkbookReference;

		#endregion Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="Workbook"/> class.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The new workbook created is empty. At least one <see cref="Worksheet"/> must be added to it before 
		/// it can be saved.
		/// </p>
		/// </remarks>
		/// <seealso cref="Worksheets"/>
		//MRS 6/28/05 - BR04756
		public Workbook()
#pragma warning disable 0618
			: this( WorkbookPaletteMode.CustomPalette ) { }
#pragma warning restore 0618

		// MD 6/31/08 - Excel 2007 Format
		/// <summary>
		/// Creates a new instance of the <see cref="Workbook"/> class.
		/// </summary>
		/// <param name="format">The file format to use when imposing format restrictions and saving.</param>
		/// <remarks>
		/// <p class="body">
		/// The new workbook created is empty. At least one <see cref="Worksheet"/> must be added to it before 
		/// it can be saved.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="format"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <seealso cref="Worksheets"/>
		public Workbook( WorkbookFormat format)
#pragma warning disable 0618
			: this( format, WorkbookPaletteMode.CustomPalette ) { }
#pragma warning restore 0618

		/// <summary>
		/// Obsolete. Use <see cref="Workbook()"/> instead.
		/// </summary>
		//MRS 6/28/05 - BR04756
		//public Workbook() 
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
		[Obsolete("This constructor has been deprecated because the paletteMode is no longer used.")] // MD 1/16/12 - 12.1 - Cell Format Updates
		public Workbook( WorkbookPaletteMode paletteMode )
			// MD 6/31/08 - Excel 2007 Format
			// Call new new constructor
			: this( WorkbookFormat.Excel97To2003, paletteMode )
		{
			// MD 6/31/08 - Excel 2007 Format
			// Moved all code to the new constructor
		}

		// MD 6/31/08 - Excel 2007 Format
		/// <summary>
		/// Obsolete. Use <see cref="Workbook(WorkbookFormat)"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
		[Obsolete("This constructor has been deprecated because the paletteMode is no longer used.")] // MD 1/16/12 - 12.1 - Cell Format Updates
		public Workbook( WorkbookFormat format, WorkbookPaletteMode paletteMode )
		{
			// MD 6/31/08 - Excel 2007 Format
			if ( Enum.IsDefined( typeof( WorkbookFormat ), format ) == false )
				throw new InvalidEnumArgumentException( "format", (int)format, typeof( WorkbookFormat ) );

			// MD 1/16/12 - 12.1 - Cell Format Updates
			// The paletteMode is now ignored.
			//if ( Enum.IsDefined( typeof( WorkbookPaletteMode ), paletteMode ) == false )
			//    throw new InvalidEnumArgumentException( "paletteMode", (int)paletteMode, typeof( WorkbookPaletteMode ) );

			// MD 6/31/08 - Excel 2007 Format
			this.currentFormat = format;

			WorkbookFontData defaultFont = new WorkbookFontData( this );

            // MBS 7/15/08 - Excel 2007 Format
			//this.fonts = new GenericCachedCollection<WorkbookFontData>( defaultFont, this, Int32.MaxValue );
			// MD 2/2/12 - TFS100573
            //this.fonts = new GenericCachedCollection<WorkbookFontData>(defaultFont, this, Workbook.MaxExcelWorkbookFonts);
			this.fonts = new GenericCachedCollectionEx<WorkbookFontData>(defaultFont, this, Workbook.MaxExcelWorkbookFonts);

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// We can't create the cell formats collection until the palette is created, because accessing the Styles collection needs to 
			// use the palette, so this is moved below.
			#region Old Code

			//WorksheetCellFormatData defaultFormat = new WorksheetCellFormatData( this, this.fonts );
			//defaultFormat.Style = true;

			//// MBS 7/15/08 - Excel 2007 Format
			////this.cellFormats = new GenericCachedCollection<WorksheetCellFormatData>( defaultFormat, this, Workbook.MaxExcelCellFormatCount );
			//// MD 4/18/11 - TFS62026
			//// The cellFormats collection is now a derived type.
			////this.cellFormats = new GenericCachedCollection<WorksheetCellFormatData>(defaultFormat, this, this.MaxCellFormats);
			//this.cellFormats = new WorksheetCellFormatCollection(defaultFormat, this);

			#endregion // Old Code

			//MRS 6/28/05 - BR04756
			//this.palette = new WorkbookColorCollection();
			// MD 1/15/08 - BR29635
			// Added an extra parameter to the constructor
			//this.palette = new WorkbookColorCollection( paletteMode );
			// MD 1/16/12 - 12.1 - Cell Format Updates
			//this.palette = new WorkbookColorCollection( this, paletteMode );
			this.palette = new WorkbookColorPalette();

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// This is no longer needed now that the default font height is exposed off the normal style.
			//this.defaultFontHeight = 200;

			// MD 11/3/10 - TFS49093
			// Added the shared string table to the workbook so we don't have to generate it on each save.
			// MD 4/12/11 - TFS67084
			// Use a derived collection for the shared string table.
			//this.sharedStringTable = new GenericCachedCollection<FormattedStringElement>(null, this, Int32.MaxValue);
			// MD 2/2/12 - TFS100573
			//this.sharedStringTable = new SharedStringTable(null, this);
			this.sharedStringTable = new SharedStringTable(this);

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Create the cell formats collection last, because accessing the Styles collection here requires a few other things to be initialized.
			WorksheetCellFormatData defaultFormat = new WorksheetCellFormatData(this, WorksheetCellFormatType.CellFormat);
			defaultFormat.Style = this.Styles.NormalStyle;
			this.cellFormats = new WorksheetCellFormatCollection(defaultFormat, this);
		}

		#endregion Constructor

		#region Interfaces

		// MD 2/29/12 - 12.1 - Table Support
		// This is no longer needed.
		#region Removed
      
		//// MD 7/26/10 - TFS34398
		//#region IWorksheetCellFormatProxyOwner Members

		//// MD 10/21/10 - TFS34398
		//// We need to pass along options to the handlers of the cell format value change.
		////void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(CellFormatValue value) { }
		//// MD 4/12/11 - TFS67084
		//// We need to pass along the sender now because some object own multiple cell formats.
		////void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(CellFormatValue value, CellFormatValueChangedOptions options) { }
		//// MD 4/18/11 - TFS62026
		//// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
		////void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, CellFormatValue value, CellFormatValueChangedOptions options) { }
		//void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options) { }

		//// MD 11/1/11 - TFS94534
		//bool IWorksheetCellFormatProxyOwner.CanOwnStyleFormat
		//{
		//    get { return true; }
		//}

		//// MD 1/17/12 - 12.1 - Cell Format Updates
		//// This is no longer needed.
		////Workbook IWorksheetCellFormatProxyOwner.Workbook
		////{
		////    get { return this; }
		////}

		//#endregion 
    
		#endregion // Removed

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		// MD 1/10/11 - TFS62763
		#region CharacterWidth256thsToPixels

		/// <summary>
		/// Converts units of 1/256s of the average character width to pixels.
		/// </summary>
		/// <param name="characterWidth256ths">The number of units of 1/256s of the average character width.</param>
		/// <returns>The number of pixels equivalent to the <paramref name="characterWidth256ths"/> value.</returns>
		/// <remarks>
		/// <p class="body">
		/// The units of 1/256s of the average character width are based on the <see cref="DefaultFontHeight"/>.
		/// </p>
		/// </remarks>
		/// <seealso cref="PixelsToCharacterWidth256ths"/>
		/// <seealso cref="DefaultFontHeight"/>
		/// <seealso cref="WorksheetColumn.Width"/>
		/// <seealso cref="Worksheet.DefaultColumnWidth"/>
		public double CharacterWidth256thsToPixels(double characterWidth256ths)
		{
			// MD 2/10/12 - TFS97827
			// Now that we are not storing the CharacterWidth256thsPerPixel, do the conversion manually.
			//return characterWidth256ths / this.CharacterWidth256thsPerPixel;
			return MathUtilities.MidpointRoundingAwayFromZero(this.ZeroCharacterWidth * (characterWidth256ths / 256d));
		} 

		#endregion // CharacterWidth256thsToPixels

		#region CreateNewWorkbookFont

		/// <summary>
		/// Factory method which creates new workbook font.
		/// </summary>
		/// <remarks>
		/// <p class="body"><see cref="IWorkbookFont"/> describes font used in excel workbook.
		/// If many parts of excel workbook have same and complex (more than one property in common) font formatting, use this method in following manner: 
		/// <ol>
		/// <li class="taskitem"><span class="taskitemtext">Create new font format with <see cref="CreateNewWorkbookFont"/>,</span></li>
		/// <li class="taskitem"><span class="taskitemtext">Set all necessary properties on given font format,</span></li>
		/// <li class="taskitem"><span class="taskitemtext">Apply font format to all excel objects which use it with <see cref="IWorkbookFont.SetFontFormatting"/> method.</span></li>
		/// </ol></p>
		/// <p class="body">Use of this procedure will simplify you code for complex font formats and increase speed of resulting program. It will not reduce total number of font formats in a workbook as font formats are internally cached no matter which method is used.</p>
		/// </remarks>
		/// <returns>The created excel font object.</returns>
		public IWorkbookFont CreateNewWorkbookFont()
		{
			return new WorkbookFontData( this );
		}

		#endregion CreateNewWorkbookFont

		#region CreateNewWorksheetCellFormat

		/// <summary>
		/// Creates new worksheet cell format.
		/// </summary>
		/// <returns>The cell format which was created.</returns>
		/// <remarks>
		/// <p class="body"><see cref="IWorksheetCellFormat"/> describes cell specific formatting (font, number format, appearance etc.). Total number of different cell formats in excel workbook is limited to <see cref="MaxExcelCellFormatCount"/>. 
		/// If many parts of excel workbook have same and complex (more than one property in common) cell formatting, use this method in following manner: 
		/// <ol>
		/// <li class="taskitem"><span class="taskitemtext">Create new cell format with <see cref="CreateNewWorksheetCellFormat"/>,</span></li>
		/// <li class="taskitem"><span class="taskitemtext">Set all necessary properties on given cell format,</span></li>
		/// <li class="taskitem"><span class="taskitemtext">Apply cell format to all excel objects which use it with <see cref="IWorksheetCellFormat.SetFormatting"/> method.</span></li>
		/// </ol></p>
		/// <p class="body">Use of this procedure will simplify you code for complex cell formats and increase speed of resulting program. It will not reduce total number of cell formats in a workbook as cell formats are internally cached no matter which method is used.</p>
		/// </remarks>
		public IWorksheetCellFormat CreateNewWorksheetCellFormat()
		{
			// MD 12/22/11 - 12.1 - Cell Format Updates
			// Moved all code to the new CreateNewWorksheetCellFormatInternal method so we could call that when needing a 
			// WorksheetCellFormatData instance associated with the workbook.
			return this.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.CellFormat);
		}

		// MD 12/22/11 - 12.1 - Cell Format Updates
		// Added a method to get a WorksheetCellFormatData instance associated with the workbook.
		internal WorksheetCellFormatData CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType type)
		{
			// MD 1/2/12 - 12.1 - Cell Format Updates
			// Initialize the style reference before returning the new format.
			//return new WorksheetCellFormatData( this, null );
			WorksheetCellFormatData data = new WorksheetCellFormatData(this, type);

			if (type == WorksheetCellFormatType.CellFormat)
				data.Style = this.Styles.NormalStyle;

			return data;
		}

		#endregion CreateNewWorksheetCellFormat

		// MD 12/20/11 - 12.1 - Table Support
		#region GetTable

		/// <summary>
		/// Gets the table with the specified name.
		/// </summary>
		/// <param name="name">The name of the table to get.</param>
		/// <remarks>
		/// <p class="body">
		/// Table names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <returns>A <see cref="WorksheetTable"/> instance if a table exists with the specified name; Otherwise null.</returns>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public WorksheetTable GetTable(string name)
		{
			NamedReferenceBase namedItem;
			if (this.workbookScopedNamedItems.TryGetValue(name, out namedItem) == false)
				return null;

			return namedItem as WorksheetTable;
		}

		#endregion // GetTable

		// MD 1/10/11 - TFS62763
		#region PixelsToCharacterWidth256ths

		/// <summary>
		/// Converts pixels to units of 1/256s of the average character width.
		/// </summary>
		/// <param name="pixels">The number of pixels.</param>
		/// <returns>The number of units of 1/256s of the average character width equivalent to the <paramref name="pixels"/> value.</returns>
		/// <remarks>
		/// <p class="body">
		/// The units of 1/256s of the average character width are based on the <see cref="DefaultFontHeight"/>.
		/// </p>
		/// </remarks>
		/// <seealso cref="CharacterWidth256thsToPixels"/>
		/// <seealso cref="DefaultFontHeight"/>
		/// <seealso cref="WorksheetColumn.Width"/>
		/// <seealso cref="Worksheet.DefaultColumnWidth"/>
		public double PixelsToCharacterWidth256ths(double pixels)
		{
			// MD 2/10/12 - TFS97827
			// Now that we are not storing the CharacterWidth256thsPerPixel, do the conversion manually.
			//return pixels * this.CharacterWidth256thsPerPixel;
			return MathUtilities.Truncate((pixels / this.ZeroCharacterWidth) * 256.0);
		} 

		#endregion // PixelsToCharacterWidth256ths

		// MD 7/14/08 - Excel formula solving
		#region Recalculate

		/// <summary>
		/// Recalculates all formulas on the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This can be used when the <see cref="Workbook.CalculationMode"/> is Manual. In Manual mode, when cells are dirtied, formulas referencing
		/// those cells will not be recalculated until Recalculate is called or <see cref="RecalculateBeforeSave"/> is True and the workbook is saved.
		/// </p>
		/// </remarks>
		/// <seealso cref="Workbook.CalculationMode"/>
		/// <seealso cref="RecalculateBeforeSave"/>
		public void Recalculate()
		{
			// MD 7/19/12
			// Found while fixing TFS116808 (Table resizing)
			if (this.IsLoading)
				return;

			// On manual recalculations, make sure the calc engine knows about everything that should be dirtied.
			this.CalcEngine.DirtyAlwaysDirtyList();
			this.CalcEngine.Recalc();
		}

		#endregion Recalculate

		// MRS NAS v8.3 - Excel formula solving - public extensibility to add/replace functions
		#region RegisterUserDefinedFunctionLibrary

		/// <summary>
		/// Registers an assembly containing <see cref="ExcelCalcFunction"/> derived types.
		/// </summary>
		/// <param name="assembly">Loaded assembly to register</param>
		/// <returns>Returns true if the assembly was registered successfully, else false if the registration failed</returns>
		/// <remarks>
		/// <p class="body">
		/// All types within the registered assembly are enumerated and any that derive from <see cref="ExcelCalcFunction"/> class are added to the list of available formula functions
		/// </p>
		/// </remarks>
		public bool RegisterUserDefinedFunctionLibrary( System.Reflection.Assembly assembly )
		{
			return this.CalcEngine.FunctionFactory.AddLibrary(assembly);
		}

		#endregion RegisterUserDefinedFunctionLibrary

		// MRS NAS v8.3 - Excel formula solving - public extensibility to add/replace functions
		#region RegisterUserDefinedFunction
		/// <summary>
		/// Registers a single <see cref="ExcelCalcFunction"/> instance.
		/// </summary>
		/// <param name="userDefinedFunction">User defined function instance to register</param>
		/// <returns>Returns true if the type was registered successfully, else false if the registration failed</returns>
		/// <remarks>
		/// <p class="body">
		/// Users can build custom functions used in formulas by sub-classing the <see cref="ExcelCalcFunction"/> class.  
		/// Once the derived class is instantiated it must be registered by using the RegisterUserDefinedFunction method before being available and referenced by a formulas.
		/// Users can build a library of functions packaged in an assembly and register all the functions within the assembly by using the <see cref="RegisterUserDefinedFunctionLibrary"/> method.
		/// </p>
		/// </remarks>
		public bool RegisterUserDefinedFunction( ExcelCalcFunction userDefinedFunction )
		{
			return this.CalcEngine.FunctionFactory.Add( userDefinedFunction );
		}
		#endregion RegisterUserDefinedFunction

		// MD 8/18/08 - Excel formula solving
		#region ResumeCalculations

		/// <summary>
		/// Resumes the calculation of formulas.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If calculations were not suspended when this is called, it will have no effect.
		/// </p>
		/// <p class="body">
		/// For each call to <see cref="SuspendCalculations"/>, a call to ResumeCalculations must be made. As soon as the number of calls to 
		/// ResumeCalculations equals the number of calls to SuspendCalculations, calculations will be resumed.
		/// </p>
		/// </remarks>
		/// <seealso cref="SuspendCalculations"/>
		public void ResumeCalculations()
		{
			this.ResumeCalculations( true );
		}

		#endregion ResumeCalculations

		#region Save( string )

        /// <summary>
		/// Writes the workbook to a file.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The workbook will be written in the format specified by the <see cref="CurrentFormat"/>.
		/// </p>
		/// <p class="body">
		/// The <paramref name="fileName"/> specified should have an extension corresponding to the current format so it can be opened in Microsoft Excel 
		/// by default (if it is installed).
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> This method creates a <see cref="FileStream"/> using the 
		/// <a href="http://msdn2.microsoft.com/en-us/library/tyhc0kft.aspx">FileStream(string, FileMode, FileAccess)</a> overload of the constructor.
		/// See the remarks section of this overload to for the exceptions that could be thrown.
		/// </p>
		/// </remarks>
		/// <param name="fileName">The file to write the workbook to.</param>
		/// <exception cref="InvalidOperationException">
		/// The workbook has no worksheets in its <see cref="Worksheets"/> collection.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// No worksheet in this workbook's Worksheets collection has its <see cref="DisplayOptions.Visibility"/> 
		/// set to Visible. At least one worksheet in the workbook must be visible.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// A <see cref="CustomView"/> in the workbook's <see cref="CustomViews"/> collection has all worksheets hidden.
		/// At least one worksheet must be visible in all custom views.
		/// </exception>
        public void Save(string fileName)
        {
            this.Save(fileName, null);
        }

		#endregion Save( string )

		#region Save( string, IPackageFactory )

		/// <summary>
		/// Writes the workbook to a file.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The workbook will be written in the format specified by the <see cref="CurrentFormat"/>.
		/// </p>
		/// <p class="body">
		/// The <paramref name="fileName"/> specified should have an extension corresponding to the current format so it can be opened in Microsoft Excel 
		/// by default (if it is installed).
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> This method creates a <see cref="FileStream"/> using the 
		/// <a href="http://msdn2.microsoft.com/en-us/library/tyhc0kft.aspx">FileStream(string, FileMode, FileAccess)</a> overload of the constructor.
		/// See the remarks section of this overload to for the exceptions that could be thrown.
		/// </p>
		/// </remarks>
		/// <param name="fileName">The file to write the workbook to.</param>
        /// <param name="packageFactory">An IPackageFactory which can be used to open an IPackage from a stream.</param>
		/// <exception cref="InvalidOperationException">
		/// The workbook has no worksheets in its <see cref="Worksheets"/> collection.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// No worksheet in this workbook's Worksheets collection has its <see cref="DisplayOptions.Visibility"/> 
		/// set to Visible. At least one worksheet in the workbook must be visible.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// A <see cref="CustomView"/> in the workbook's <see cref="CustomViews"/> collection has all worksheets hidden.
		/// At least one worksheet must be visible in all custom views.
		/// </exception>
		public void Save( string fileName, IPackageFactory packageFactory )
		{
			// MD 10/5/07 - BR26292
			// We should be verifying the workbook contents before creating the workbook file.
			// Then if there is a problem, an empty workbook file is not created on the user's machine
			this.VerifyBeforeSave();

			

			using ( FileStream stream = new FileStream( fileName, FileMode.Create, FileAccess.ReadWrite ) )
			{
				// MD 10/5/07 - BR26292
				// Since we already did the verification, just call the SaveHelper to do the actual saving.
				//this.Save( stream );
				this.SaveHelper( stream, packageFactory );
			}


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion SaveSave( string, IPackageFactory )

		#region Save( Stream )

		/// <summary>
		/// Writes the workbook to a stream.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The workbook will be written in the format specified by the <see cref="CurrentFormat"/>.
		/// </p>
		/// </remarks>
		/// <param name="stream">The stream to write the workbook to.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="stream"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The workbook has no worksheets in its <see cref="Worksheets"/> collection.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// No worksheet in this workbook's Worksheets collection has its <see cref="DisplayOptions.Visibility"/> 
		/// set to Visible.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// A <see cref="CustomView"/> in the workbook's <see cref="CustomViews"/> collection has all worksheets hidden.
		/// At least one worksheet must be visible in all custom views.
		/// </exception>
        public void Save(Stream stream)
        {



            this.Save(stream, null);

        }

		#endregion Save( Stream )

		#region Save( Stream, IPackageFactory )

		/// <summary>
		/// Writes the workbook to a stream.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The workbook will be written in the format specified by the <see cref="CurrentFormat"/>.
		/// </p>
		/// </remarks>
		/// <param name="stream">The stream to write the workbook to.</param>
        /// <param name="packageFactory">An IPackageFactory which can be used to open an IPackage from a stream.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="stream"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The workbook has no worksheets in its <see cref="Worksheets"/> collection.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// No worksheet in this workbook's Worksheets collection has its <see cref="DisplayOptions.Visibility"/> 
		/// set to Visible.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// A <see cref="CustomView"/> in the workbook's <see cref="CustomViews"/> collection has all worksheets hidden.
		/// At least one worksheet must be visible in all custom views.
		/// </exception>
		public void Save( Stream stream, IPackageFactory packageFactory )
		{
			if ( stream == null )
				throw new ArgumentNullException( "stream", SR.GetString( "LE_ArgumentNullException_SaveStream" ) );

			// MD 4/8/08 - BR31623
			// Only save directly to the stream if it can seek. Otherwise, we will not be able to go back and write 
			// the structured storage header at the end, so we should instead write the full workbook to another stream
			// and copy it to the passed in stream.
			if ( stream.CanSeek == false )
			{
				using ( MemoryStream memoryStream = new MemoryStream() )
				{
					// Save directly to the memory stream
					this.Save( memoryStream, packageFactory );

					// Copy the contents of the stream to the passed in stream
					byte[] data = memoryStream.GetBuffer();

					// MD 4/19/11 - TFS72977
					// The data length is the full buffer of the memory stream, which is probably not fully used.
					// Instead, only copy over the bytes that were actually used in the memory stream.
					//stream.Write( data, 0, data.Length );
					stream.Write(data, 0, (int)memoryStream.Length);

					return;
				}
			}

			// MD 10/5/07 - BR26292
			// Moved all verification code to VerifyBeforeSave
			this.VerifyBeforeSave();

			// MD 10/5/07 - BR26292
			// Moved all saving code to SaveHelper
            this.SaveHelper(stream, packageFactory);
		}

		#endregion Save( Stream, IPackageFactory )

		// MD 6/31/08 - Excel 2007 Format
		#region SetCurrentFormat

		/// <summary>
		/// Sets the current format of the workbook.
		/// </summary>
		/// <param name="format">The file format to use when imposing format restrictions and saving.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="format"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The workbook already contains data which exceeds the limits imposed by <paramref name="format"/>.
		/// </exception>
		/// <seealso cref="CurrentFormat"/>
		public void SetCurrentFormat( WorkbookFormat format )
		{
			if ( this.currentFormat == format )
				return;

			if ( Enum.IsDefined( typeof( WorkbookFormat ), format ) == false )
				throw new InvalidEnumArgumentException( "format", (int)format, typeof( WorkbookFormat ) );

            // MBS 7/15/08 - Excel 2007 Format    
            this.UpdateCellFormatLimit(format);

			FormatLimitErrors errors = new FormatLimitErrors();
			this.VerifyFormatLimits( errors, format );

			if ( errors.HasErrors )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_WorkbookDataViolatesFormatLimits" ) );
			// MD 5/7/10 - 10.2 - Excel Templates
			//this.currentFormat = format;
			this.SetCurrentFormatInternal(format);

			this.OnCurrentFormatChanged();
		}

		#endregion SetCurrentFormat

		// MD 8/18/08 - Excel formula solving
		#region SuspendCalculations

		/// <summary>
		/// Temporarily suspends the calculation of formulas.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This should be used when adding many formulas or modifying large amounts of data on a workbook at once so formulas are not calculated 
		/// each time cells are dirtied.
		/// </p>
		/// <p class="body">
		/// For each call to SuspendCalculations, a call to <see cref="ResumeCalculations()"/> must be made. As soon as the number of calls to 
		/// ResumeCalculations equals the number of calls to SuspendCalculations, calculations will be resumed.
		/// </p>
		/// </remarks>
		/// <seealso cref="ResumeCalculations()"/>
		public void SuspendCalculations()
		{
			this.calculationSuspensionCount++;

			if ( this.referencesRequiringFormulaCompilation == null )
				this.referencesRequiringFormulaCompilation = new Dictionary<ExcelRefBase, Formula>();
		} 

		#endregion SuspendCalculations

		#endregion Public Methods

		#region Static Methods

		#region Load( string )
		/// <summary>
		/// Reads a workbook from a file.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If the extension is
		/// a standard Excel format extension, the workbook will be assumed to be in the corresponding format. Otherwise, the contents of the
		/// file will be examined to try to determine the format. The <see cref="CurrentFormat"/> of the returned workbook will indicate the
		/// format the workbook was loaded from.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> This method creates a <see cref="FileStream"/> using the 
		/// <a href="http://msdn2.microsoft.com/en-us/library/47ek66wy.aspx">FileStream(string, FileMode)</a> overload of the constructor.  
		/// See the remarks section of this overload for the exceptions that could be thrown.
		/// </p>
		/// </remarks>
		/// <param name="fileName">The file from which to read the workbook.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="fileName"/> is a path to an invalid Microsoft Excel file.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified file.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The format of the workbook data is in an unsupported format.
		/// </exception>
		/// <exception cref="OpenPackagingNonConformanceException">
		/// (<see cref="WorkbookFormat">Excel2007</see>-format specific)
        /// Thrown when the markup which contains the SpreadsheetML data is determined to be
        /// non-compliant with the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions'
        /// document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is being used. The overload taking an 
		/// <see cref="IPackageFactory"/> must be used in this case so the Excel 2007 file package can be accessed.
		/// </exception>
		/// <returns>The workbook loaded from the file.</returns>        
        public static Workbook Load(string fileName)
        {
            return Load(fileName, null);
        }
     
        /// <summary>
        /// Reads a workbook from a file.
        /// </summary>
        /// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If the extension is
		/// a standard Excel format extension, the workbook will be assumed to be in the corresponding format. Otherwise, the contents of the
		/// file will be examined to try to determine the format. The <see cref="CurrentFormat"/> of the returned workbook will indicate the
		/// format the workbook was loaded from.
		/// </p>
        /// <p class="note">
        /// <B>Note:</B> This method creates a <see cref="FileStream"/> using the 
        /// <a href="http://msdn2.microsoft.com/en-us/library/47ek66wy.aspx">FileStream(string, FileMode)</a> overload of the constructor.  
        /// See the remarks section of this overload for the exceptions that could be thrown.
        /// </p>
        /// </remarks>
        /// <param name="fileName">The file from which to read the workbook.</param>        
        /// <param name="verifyExcel2007Xml">A boolean specifying whether or not to verify the contents of the markup against the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions' document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is a path to an invalid Microsoft Excel file.
        /// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified file.
		/// </exception>
        /// <exception cref="InvalidOperationException">
        /// The format of the workbook data is in an unsupported format.
        /// </exception>
        /// <exception cref="OpenPackagingNonConformanceException">
        /// (<see cref="WorkbookFormat">Excel2007</see>-format specific)
        /// Thrown when the markup which contains the SpreadsheetML data is determined to be
        /// non-compliant with the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions'
        /// document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).
        /// </exception>
		/// <exception cref="NotSupportedException">
		/// The workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is being used. The overload taking an 
		/// <see cref="IPackageFactory"/> must be used in this case so the Excel 2007 file package can be accessed.
		/// </exception>
        /// <returns>The workbook loaded from the file.</returns>        
        public static Workbook Load(string fileName, bool verifyExcel2007Xml)
        {
            return Load(fileName, null, verifyExcel2007Xml);
        }


        /// <summary>
		/// Reads a workbook from a file.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If the extension is
		/// a standard Excel format extension, the workbook will be assumed to be in the corresponding format. Otherwise, the contents of the
		/// file will be examined to try to determine the format. The <see cref="CurrentFormat"/> of the returned workbook will indicate the
		/// format the workbook was loaded from.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> This method creates a <see cref="FileStream"/> using the 
		/// <a href="http://msdn2.microsoft.com/en-us/library/47ek66wy.aspx">FileStream(string, FileMode)</a> overload of the constructor.  
		/// See the remarks section of this overload for the exceptions that could be thrown.
		/// </p>
		/// </remarks>
		/// <param name="fileName">The file from which to read the workbook.</param>
        /// <param name="packageFactory">An IPackageFactory which can be used to open an IPackage from a stream.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="fileName"/> is a path to an invalid Microsoft Excel file.        
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified file.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The format of the workbook data is in an unsupported format.
		/// </exception>
		/// <exception cref="OpenPackagingNonConformanceException">
		/// (<see cref="WorkbookFormat">Excel2007</see>-format specific)
        /// Thrown when the markup which contains the SpreadsheetML data is determined to be
        /// non-compliant with the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions'
        /// document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <paramref name="packageFactory"/> is null and the workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is 
		/// being used. An <see cref="IPackageFactory"/> must be specified so the Excel 2007 file package can be accessed.
		/// </exception>
		/// <returns>The workbook loaded from the file.</returns>  
        public static Workbook Load( string fileName, IPackageFactory packageFactory )
		{
            return Load(fileName, packageFactory, true);
		}

        /// <summary>
        /// Reads a workbook from a file.
        /// </summary>
        /// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If the extension is
		/// a standard Excel format extension, the workbook will be assumed to be in the corresponding format. Otherwise, the contents of the
		/// file will be examined to try to determine the format. The <see cref="CurrentFormat"/> of the returned workbook will indicate the
		/// format the workbook was loaded from.
		/// </p>
        /// <p class="note">
        /// <B>Note:</B> This method creates a <see cref="FileStream"/> using the 
        /// <a href="http://msdn2.microsoft.com/en-us/library/47ek66wy.aspx">FileStream(string, FileMode)</a> overload of the constructor.  
        /// See the remarks section of this overload for the exceptions that could be thrown.
        /// </p>
        /// </remarks>
        /// <param name="fileName">The file from which to read the workbook.</param>
        /// <param name="packageFactory">An IPackageFactory which can be used to open an IPackage from a stream.</param>
        /// <param name="verifyExcel2007Xml">A boolean specifying whether or not to verify the contents of the markup against the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions' document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is a path to an invalid Microsoft Excel file.        
        /// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified file.
		/// </exception>
        /// <exception cref="InvalidOperationException">
        /// The format of the workbook data is in an unsupported format.
        /// </exception>
        /// <exception cref="OpenPackagingNonConformanceException">
        /// (<see cref="WorkbookFormat">Excel2007</see>-format specific)
        /// Thrown when the markup which contains the SpreadsheetML data is determined to be
        /// non-compliant with the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions'
        /// document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).
        /// </exception>
		/// <exception cref="NotSupportedException">
		/// <paramref name="packageFactory"/> is null and the workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is 
		/// being used. An <see cref="IPackageFactory"/> must be specified so the Excel 2007 file package can be accessed.
		/// </exception>
        /// <returns>The workbook loaded from the file.</returns>        

        public static Workbook Load(string fileName, IPackageFactory packageFactory, bool verifyExcel2007Xml)
        {
            // MD 6/30/08 - Excel 2007 Format
            WorkbookFormat fileFormat = Utilities.GetWorkbookFormat(fileName, "fileName", packageFactory);

            // MD 9/9/08 - TFS6961
            // When opening read-only files, this was causing an exception. Since we only need to open the file for read anyway,
            // specify that so read-only files can be opened.
            //using ( FileStream stream = new FileStream( fileName, FileMode.Open ) )
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                // MD 6/30/08 - Excel 2007 Format
                //return Workbook.LoadHelper( stream, "fileName" );
                return Workbook.LoadHelper(stream, fileFormat, "fileName", packageFactory, verifyExcel2007Xml);
            }
        }

        #endregion Load( string )

        #region Load( Stream )

        /// <summary>
		/// Reads a workbook from a stream.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If a FileStream is 
		/// specified, the extension of the file will be examined. If the extension is a standard Excel format extension, the workbook will 
		/// be assumed to be in the corresponding format. Otherwise, the contents of the file will be examined to try to determine the format. 
		/// The <see cref="CurrentFormat"/> of the returned workbook will indicate the format the workbook was loaded from.
		/// </p>
		/// </remarks>
		/// <param name="stream">The stream to read the workbook from.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="stream"/> does not contain valid Microsoft Excel file contents.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified stream.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The format of the workbook data is in an unsupported format.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is being used. The overload taking an 
		/// <see cref="IPackageFactory"/> must be used in this case so the Excel 2007 file package can be accessed.
		/// </exception>
		/// <returns>The workbook loaded from the stream.</returns>
        public static Workbook Load(Stream stream)
        {



            return Load(stream, null);

        }

        /// <summary>
        /// Reads a workbook from a stream.
        /// </summary>
		/// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If a FileStream is 
		/// specified, the extension of the file will be examined. If the extension is a standard Excel format extension, the workbook will 
		/// be assumed to be in the corresponding format. Otherwise, the contents of the file will be examined to try to determine the format. 
		/// The <see cref="CurrentFormat"/> of the returned workbook will indicate the format the workbook was loaded from.
		/// </p>
		/// </remarks>
        /// <param name="stream">The stream to read the workbook from.</param>
        /// <param name="verifyExcel2007Xml">A boolean specifying whether or not to verify the contents of the markup against the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions' document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="stream"/> does not contain valid Microsoft Excel file contents.
        /// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified stream.
		/// </exception>
        /// <exception cref="InvalidOperationException">
        /// The format of the workbook data is in an unsupported format.
        /// </exception>
		/// <exception cref="NotSupportedException">
		/// The workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is being used. The overload taking an 
		/// <see cref="IPackageFactory"/> must be used in this case so the Excel 2007 file package can be accessed.
		/// </exception>
        /// <returns>The workbook loaded from the stream.</returns>
        public static Workbook Load(Stream stream, bool verifyExcel2007Xml)
        {
            return Load(stream, null, verifyExcel2007Xml);
        }

		/// <summary>
		/// Reads a workbook from a stream.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If a FileStream is 
		/// specified, the extension of the file will be examined. If the extension is a standard Excel format extension, the workbook will 
		/// be assumed to be in the corresponding format. Otherwise, the contents of the file will be examined to try to determine the format. 
		/// The <see cref="CurrentFormat"/> of the returned workbook will indicate the format the workbook was loaded from.
		/// </p>
		/// </remarks>
		/// <param name="stream">The stream to read the workbook from.</param>
        /// <param name="packageFactory">An IPackageFactory which can be used to open an IPackage from a stream.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="stream"/> does not contain valid Microsoft Excel file contents.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified stream.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The format of the workbook data is in an unsupported format.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <paramref name="packageFactory"/> is null and the workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is 
		/// being used. An <see cref="IPackageFactory"/> must be specified so the Excel 2007 file package can be accessed.
		/// </exception>
		/// <returns>The workbook loaded from the stream.</returns>
        public static Workbook Load(Stream stream, IPackageFactory packageFactory)
		{
            return Load(stream, packageFactory, true);
		}

        /// <summary>
        /// Reads a workbook from a stream.
        /// </summary>
		/// <remarks>
		/// <p class="body">
		/// When loading the workbook, the format will be determined the file extension or by the contents of the file. If a FileStream is 
		/// specified, the extension of the file will be examined. If the extension is a standard Excel format extension, the workbook will 
		/// be assumed to be in the corresponding format. Otherwise, the contents of the file will be examined to try to determine the format. 
		/// The <see cref="CurrentFormat"/> of the returned workbook will indicate the format the workbook was loaded from.
		/// </p>
		/// </remarks>
        /// <param name="stream">The stream to read the workbook from.</param>
        /// <param name="packageFactory">An IPackageFactory which can be used to open an IPackage from a stream.</param>
        /// <param name="verifyExcel2007Xml">A boolean specifying whether or not to verify the contents of the markup against the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions' document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="stream"/> does not contain valid Microsoft Excel file contents.
        /// </exception>
		/// <exception cref="ArgumentException">
		/// The file format cannot be determined from the specified stream.
		/// </exception>
        /// <exception cref="InvalidOperationException">
        /// The format of the workbook data is in an unsupported format.
        /// </exception>
		/// <exception cref="NotSupportedException">
		/// <paramref name="packageFactory"/> is null and the workbook being loaded is in an Excel 2007 format and the CLR 2.0 Excel assembly is 
		/// being used. An <see cref="IPackageFactory"/> must be specified so the Excel 2007 file package can be accessed.
		/// </exception>
        /// <returns>The workbook loaded from the stream.</returns>
        public static Workbook Load(Stream stream, IPackageFactory packageFactory, bool verifyExcel2007Xml)
        {
            // MD 6/30/08 - Excel 2007 Format
            //return Workbook.LoadHelper( stream, "stream" );
            return Workbook.LoadHelper(stream, Utilities.GetWorkbookFormat(stream, "stream", packageFactory), "stream", packageFactory, verifyExcel2007Xml);
        }

		#endregion Load( Stream )

        // MRS NAS v8.3 - WinGrid Excel Exporter - design new public methods/events
        #region GetWorkbookFormat

        /// <summary>
        /// Returns the WorkbookFormat based on the file extension of the specified file. 
        /// </summary>
        /// <param name="fileName">The filename of an excel file.</param>
        /// <returns>The workbook format based on the file extension of the file, or null if the correct format cannot be determined.</returns>
        public static WorkbookFormat? GetWorkbookFormat(string fileName)
        {
            string extensionLower = Path.GetExtension(fileName).ToLower();

            switch (extensionLower)
            {
                case ".xls":
                    return WorkbookFormat.Excel97To2003;

				// MD 5/7/10 - 10.2 - Excel Templates
				case ".xlt":
					return WorkbookFormat.Excel97To2003Template;

                case ".xlsx":
                    return WorkbookFormat.Excel2007;

				// MD 10/1/08 - TFS8471
				case ".xlsm":
					return WorkbookFormat.Excel2007MacroEnabled;

				// MD 5/7/10 - 10.2 - Excel Templates
				case ".xltm":
					return WorkbookFormat.Excel2007MacroEnabledTemplate;

				// MD 5/7/10 - 10.2 - Excel Templates
				case ".xltx":
					return WorkbookFormat.Excel2007Template;
                //case ".xlsb":
                //	return WorkbookFormat.Excel2007Binary;
            }
            return null;
        }

        #endregion GetWorkbookFormat( string, string, bool )

		#endregion Static Methods

		#region Internal Methods

		// MD 10/8/10
		// Found while fixing TFS44359
		// Added support to round-trip custom Xml parts.
		#region AddCustomXmlPart

		internal void AddCustomXmlPart(byte[] partData)
		{
			if (this.customXmlParts == null)
				this.customXmlParts = new List<byte[]>();

			this.customXmlParts.Add(partData);
		}

		#endregion // AddCustomXmlPart

		#region AddCustomXmlPropertiesPart

		internal void AddCustomXmlPropertiesPart(byte[] partData)
		{
			if (this.customXmlPropertiesParts == null)
				this.customXmlPropertiesParts = new List<byte[]>();

			this.customXmlPropertiesParts.Add(partData);
		}

		#endregion // AddCustomXmlPropertiesPart

		// MD 7/14/08 - Excel formula solving
		#region AddFormula

		internal void AddFormula( IExcelCalcFormula formula )
		{
			if ( formula == null )
			{
				Utilities.DebugFail( "The formula cannot be null." );
				return;
			}

			this.CalcEngine.AddFormula( formula );
		}

		#endregion AddFormula

		// MD 7/14/08 - Excel formula solving
		#region AddReference

		// MD 3/30/11 - TFS69969
		//internal void AddReference( NamedCalcReference reference )
		internal void AddReference(IExcelCalcReference reference)
		{
			if ( reference == null )
			{
				Utilities.DebugFail( "The reference cannot be null." );
				return;
			}

			this.CalcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Create );
		}

		#endregion AddReference

		// MD 7/14/08 - Excel formula solving
		#region CompileFormula

		internal ExcelCalcFormula CompileFormula( IExcelCalcReference baseReference, Formula excelFormula )
		{
			if ( baseReference == null )
			{
				Utilities.DebugFail( "The baseReference cannot be null." );
				return null;
			}

			if ( excelFormula == null )
			{
				Utilities.DebugFail( "The formula cannot be null." );
				return null;
			}

			return new ExcelCalcFormula(excelFormula, baseReference, this);
		}

		#endregion CompileFormula

		// MD 7/14/08 - Excel formula solving
		#region EnsureCalculated

		internal void EnsureCalculated( IExcelCalcReference reference )
		{
			// If calculations are disabled, don't do anything
			if ( this.AreCalculationsSuspended || this.CalculationMode == CalculationMode.Manual )
				return;

			// MD 3/13/12 - TFS104753
			// The volatile functions don't recalculate when their value is asked for. They only recalculate
			// when something else is dirtied or Recalculate is called on the workbook.
			//this.CalcEngine.DirtyAlwaysDirtyList();

			this.CalcEngine.IsDirty( reference, true );
		} 

		#endregion EnsureCalculated

		// MBS 7/15/08 - Excel 2007 Format
		#region GetMaxCellFormats

		internal static int GetMaxCellFormats( WorkbookFormat format )
		{
			// MD 2/4/11
			// Done while fixing TFS65015
			// Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
			//            switch ( format )
			//            {
			//                case WorkbookFormat.Excel97To2003:
			//                // MD 5/7/10 - 10.2 - Excel Templates
			//                case WorkbookFormat.Excel97To2003Template:
			//                    return Workbook.MaxExcelCellFormatCount;
			//
			//                case WorkbookFormat.Excel2007:
			//                // MD 10/1/08 - TFS8471
			//                case WorkbookFormat.Excel2007MacroEnabled:
			//                // MD 5/7/10 - 10.2 - Excel Templates
			//                case WorkbookFormat.Excel2007MacroEnabledTemplate:
			//                case WorkbookFormat.Excel2007Template:
			//                    return Workbook.MaxExcel2007CellFormatCount;
			//
			//                default:
			//                    Utilities.DebugFail( "Unknown workbook format: " + format );
			//                    goto case WorkbookFormat.Excel97To2003;
			//            }
			if (Utilities.Is2003Format(format))
				return Workbook.MaxExcelCellFormatCount;

			return Workbook.MaxExcel2007CellFormatCount;
		}
		#endregion //GetMaxCellFormats

		#region GetMaxColumnCount

        // MBS 4/29/09 - TFS17050
        // Made public so that we can check this in the exporter
        /// <summary>
        /// Returns the number of columns that are supported by the specified format.
        /// </summary>
        /// <param name="format">The format used by the workbook.</param>
        /// <returns>The maximum number of columns supported by the format.</returns>
        public static int GetMaxColumnCount( WorkbookFormat format )
		{
			// MD 4/12/11 - TFS67084
			// Moved all code to new overload.
			return Workbook.GetMaxColumnCountInternal(format);
		}

		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		internal static short GetMaxColumnCountInternal(WorkbookFormat format)
		{
			// MD 2/4/11
			// Done while fixing TFS65015
			// Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
			//            switch ( format )
			//            {
			//                case WorkbookFormat.Excel97To2003:
			//                // MD 5/7/10 - 10.2 - Excel Templates
			//                case WorkbookFormat.Excel97To2003Template:
			//                    return Workbook.MaxExcelColumnCount;
			//
			//                case WorkbookFormat.Excel2007:
			//                // MD 10/1/08 - TFS8471
			//                case WorkbookFormat.Excel2007MacroEnabled:
			//                // MD 5/7/10 - 10.2 - Excel Templates
			//                case WorkbookFormat.Excel2007MacroEnabledTemplate:
			//                case WorkbookFormat.Excel2007Template:
			//                    //case WorkbookFormat.Excel2007Binary:
			//                    return Workbook.MaxExcel2007ColumnCount;
			//
			//                default:
			//                    Utilities.DebugFail( "Unknown workbook format: " + format );
			//                    goto case WorkbookFormat.Excel97To2003;
			//            }
			if (Utilities.Is2003Format(format))
				return Workbook.MaxExcelColumnCount;

			return Workbook.MaxExcel2007ColumnCount;
		} 

		#endregion GetMaxColumnCount

		#region GetMaxRowCount

        // MBS 4/29/09 - TFS17050
        // Made public so that we can check this in the exporter
        /// <summary>
        /// Returns the number of rows that are supported by the specified format.
        /// </summary>
        /// <param name="format">The format used by the workbook.</param>
        /// <returns>The maximum number of rows supported by the format.</returns>
		public static int GetMaxRowCount( WorkbookFormat format )
		{
			// MD 2/4/11
			// Done while fixing TFS65015
			// Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
			//            switch ( format )
			//            {
			//                case WorkbookFormat.Excel97To2003:
			//                // MD 5/7/10 - 10.2 - Excel Templates
			//                case WorkbookFormat.Excel97To2003Template:
			//                    return Workbook.MaxExcelRowCount;
			//
			//                case WorkbookFormat.Excel2007:
			//                // MD 10/1/08 - TFS8471
			//                case WorkbookFormat.Excel2007MacroEnabled:
			//                // MD 5/7/10 - 10.2 - Excel Templates
			//                case WorkbookFormat.Excel2007MacroEnabledTemplate:
			//                case WorkbookFormat.Excel2007Template:
			//                    //case WorkbookFormat.Excel2007Binary:
			//                    return Workbook.MaxExcel2007RowCount;
			//
			//                default:
			//                    Utilities.DebugFail( "Unknown workbook format: " + format );
			//                    goto case WorkbookFormat.Excel97To2003;
			//            }
			if (Utilities.Is2003Format(format))
				return Workbook.MaxExcelRowCount;

			return Workbook.MaxExcel2007RowCount;
		} 

		#endregion GetMaxRowCount

		// MD 2/23/12 - 12.1 - Table Support
		#region GetNewTableId

		internal uint GetNewTableId()
		{
			return this.nextTableId++;
		}

		#endregion // GetNewTableId

		// MD 2/19/12 - 12.1 - Table Support
		#region GetTableStyle

		internal WorksheetTableStyle GetTableStyle(string name)
		{
			WorksheetTableStyle style = this.CustomTableStyles[name];
			if (style != null)
				return style;

			style = this.StandardTableStyles[name];
			Debug.Assert(style != null, "The style could not be found.");
			return style;
		}

		#endregion // GetTableStyle

		// MD 2/20/12 - 12.1 - Table Support
		#region GetTotalFormula

		internal Formula GetTotalFormula(WorksheetTableColumn column, ST_TotalsRowFunction totalsRowFunction)
		{
			int subtotalValue;
			switch (totalsRowFunction)
			{
				case ST_TotalsRowFunction.none:
				case ST_TotalsRowFunction.custom:
					return null;

				case ST_TotalsRowFunction.average:
					subtotalValue = Workbook.AverageSubtotalFunction;
					break;

				case ST_TotalsRowFunction.countNums:
					subtotalValue = Workbook.CountNumsSubtotalFunction;
					break;

				case ST_TotalsRowFunction.count:
					subtotalValue = Workbook.CountSubtotalFunction;
					break;

				case ST_TotalsRowFunction.max:
					subtotalValue = Workbook.MaxSubtotalFunction;
					break;

				case ST_TotalsRowFunction.min:
					subtotalValue = Workbook.MinSubtotalFunction;
					break;

				case ST_TotalsRowFunction.stdDev:
					subtotalValue = Workbook.StdDevSubtotalFunction;
					break;

				case ST_TotalsRowFunction.sum:
					subtotalValue = Workbook.SumSubtotalFunction;
					break;

				case ST_TotalsRowFunction.var:
					subtotalValue = Workbook.VarSubtotalFunction;
					break;

				default:
					Utilities.DebugFail("Unknown ST_TotalsRowFunction: " + totalsRowFunction);
					return null;
			}

			if (Utilities.Is2003Format(this.CurrentFormat))
			{
				WorksheetTable table = column.Table;
				if (table == null)
				{
					Utilities.DebugFail("We should have a table at this point.");
					return null;
				}

				int dataAreaTopRowIndex;
				int dataAreaBottomRowIndex;
				table.GetDataAreaRowIndexes(out dataAreaTopRowIndex, out dataAreaBottomRowIndex);

				CellAddressRange range = new CellAddressRange(
					new CellAddress(dataAreaTopRowIndex, true, column.WorksheetColumnIndex, true),
					new CellAddress(dataAreaBottomRowIndex, true, column.WorksheetColumnIndex, true));

				string columnArea = range.ToString(dataAreaBottomRowIndex + 1, column.WorksheetColumnIndex,
					false,
					this.CellReferenceMode,
					this.CurrentFormat);

				return Formula.Parse(
					string.Format("=SUBTOTAL({0},{1})", subtotalValue, columnArea),
					this.CellReferenceMode,
					this.CurrentFormat,
					CultureInfo.InvariantCulture);
			}
			else
			{
				return Formula.Parse(
					string.Format("=SUBTOTAL({0},[{1}])", subtotalValue, column.Name),
					this.CellReferenceMode,
					this.CurrentFormat,
					CultureInfo.InvariantCulture);
			}
		}

		#endregion // GetTotalFormula

		// MD 2/20/12 - 12.1 - Table Support
		#region GetTotalRowFunction

		internal ST_TotalsRowFunction GetTotalRowFunction(WorksheetTableColumn column)
		{
			Formula formula = column.TotalFormula;
			if (formula == null)
				return ST_TotalsRowFunction.none;

			ST_TotalsRowFunction? function = this.GetTotalRowFunctionHelper(column, formula);
			if (function.HasValue)
				return function.Value;

			return ST_TotalsRowFunction.custom;
		}

		private ST_TotalsRowFunction? GetTotalRowFunctionHelper(WorksheetTableColumn column, Formula formula)
		{
			if (formula.PostfixTokenList.Count != 3)
				return null;

			IntToken subTotalParam1Token = formula.PostfixTokenList[0] as IntToken;
			if (subTotalParam1Token == null)
				return null;

			ST_TotalsRowFunction functionType;
			switch (subTotalParam1Token.Value)
			{
				case Workbook.AverageSubtotalFunction:
					functionType = ST_TotalsRowFunction.average;
					break;

				case Workbook.CountNumsSubtotalFunction:
					functionType = ST_TotalsRowFunction.countNums;
					break;

				case Workbook.CountSubtotalFunction:
					functionType = ST_TotalsRowFunction.count;
					break;

				case Workbook.MaxSubtotalFunction:
					functionType = ST_TotalsRowFunction.max;
					break;

				case Workbook.MinSubtotalFunction:
					functionType = ST_TotalsRowFunction.min;
					break;

				case Workbook.StdDevSubtotalFunction:
					functionType = ST_TotalsRowFunction.stdDev;
					break;

				case Workbook.SumSubtotalFunction:
					functionType = ST_TotalsRowFunction.sum;
					break;

				case Workbook.VarSubtotalFunction:
					functionType = ST_TotalsRowFunction.var;
					break;

				default:
					return null;
			}


			AreaToken areaToken = formula.PostfixTokenList[1] as AreaToken;
			if (areaToken != null)
			{
				WorksheetTable table = column.Table;
				WorksheetRegion targetRegion = areaToken.CellAddressRange.GetTargetRegion(table.Worksheet,
					table.DataAreaRegion.LastRow + 1, column.WorksheetColumnIndex, areaToken.AreRelativeAddressesOffsets);

				if (targetRegion == null || column.DataAreaRegion.Equals(targetRegion) == false)
					return null;
			}
			else if (Utilities.Is2003Format(this.CurrentFormat) == false)
			{
				// MD 4/9/12 - TFS101506
				CultureInfo culture = this.CultureResolved;

				StructuredTableReference subTotalParam2Token = formula.PostfixTokenList[1] as StructuredTableReference;
				if (subTotalParam2Token == null ||
					// MD 4/9/12 - TFS101506
					//String.Equals(subTotalParam2Token.SimpleColumnName, column.Name, StringComparison.CurrentCultureIgnoreCase) == false)
					String.Compare(subTotalParam2Token.SimpleColumnName, column.Name, culture, CompareOptions.IgnoreCase) != 0)
					return null;

				if (subTotalParam2Token.TableName != null &&
					// MD 4/9/12 - TFS101506
					//String.Equals(subTotalParam2Token.TableName, column.Table.Name, StringComparison.CurrentCultureIgnoreCase) == false)
					String.Compare(subTotalParam2Token.TableName, column.Table.Name, culture, CompareOptions.IgnoreCase) != 0)
					return null;
			}

			FunctionVOperator subTotalFuncToken = formula.PostfixTokenList[2] as FunctionVOperator;
			if (subTotalFuncToken == null || subTotalFuncToken.Function != Function.SUBTOTAL)
				return null;

			return functionType;
		}

		#endregion // GetTotalRowFunction

		// MD 7/9/08 - Excel 2007 Format
		#region GetWorkbookOptions






		// MD 4/6/12 - TFS101506
		//internal static void GetWorkbookOptions( Workbook workbook, out CellReferenceMode cellReferenceMode, out WorkbookFormat currentFormat )
		internal static void GetWorkbookOptions(Workbook workbook, out CellReferenceMode cellReferenceMode, out WorkbookFormat currentFormat, out CultureInfo culture)
		{
			if ( workbook == null )
			{
				cellReferenceMode = CellReferenceMode.A1;

				// MD 2/24/12
				// Found while implementing 12.1 - Table Support
				// We should use the least restrictive format version when there is no workbook, not the most.
				//currentFormat = WorkbookFormat.Excel97To2003;
				currentFormat = Workbook.LatestFormat;

				// MD 4/6/12 - TFS101506
				culture = CultureInfo.CurrentCulture;

				// MD 9/26/08
				return;
			}

			cellReferenceMode = workbook.CellReferenceMode;
			currentFormat = workbook.CurrentFormat;

			// MD 4/6/12 - TFS101506
			culture = workbook.CultureResolved;
		} 

		#endregion GetWorkbookOptions

		// MD 6/18/12 - TFS102878
		#region GetWorkbookReference

		internal WorkbookReferenceBase GetWorkbookReference(string workbookFilePath)
		{
			if (workbookFilePath == null || workbookFilePath == this.LoadingPath)
				return this.CurrentWorkbookReference;

			if (workbookFilePath == AddInFunctionsWorkbookReference.AddInFunctionsWorkbookName)
				return this.AddInFunctionsWorkbookReference;

			if (this.externalWorkbooks == null)
				this.externalWorkbooks = new Dictionary<string, ExternalWorkbookReference>(StringComparer.InvariantCultureIgnoreCase);

			ExternalWorkbookReference externalWorkbookReference;
			if (this.externalWorkbooks.TryGetValue(workbookFilePath, out externalWorkbookReference) == false)
			{
				externalWorkbookReference = new ExternalWorkbookReference(workbookFilePath, this);
				this.externalWorkbooks.Add(workbookFilePath, externalWorkbookReference);
			}

			return externalWorkbookReference;
		}

		#endregion // GetWorkbookReference

		// MD 2/23/12 - 12.1 - Table Support
		#region GetWorkbookScopedNamedItem

		internal NamedReferenceBase GetWorkbookScopedNamedItem(string name)
		{
			NamedReferenceBase namedItem;
			this.workbookScopedNamedItems.TryGetValue(name, out namedItem);
			return namedItem;
		}

		#endregion // GetWorkbookScopedNamedItem

		// MD 2/28/12 - 12.1 - Table Support
		#region IterateFormulas

		internal delegate void IterateFormulasCallback(Worksheet owningWorksheet, Formula formula);

		internal void IterateFormulas(IterateFormulasCallback callback)
		{
			// Notify the formulas of other named references which might use the name
			foreach (NamedReference namedReference in this.NamedReferences)
			{
				if (namedReference.FormulaInternal != null)
					callback(null, namedReference.FormulaInternal);
			}

			this.IterateFormulasHelper(callback);
		}

		internal void IterateFormulas(IEnumerable<NamedReferenceBase> namedReferences, IterateFormulasCallback callback)
		{
			// Notify the formulas of other named references which might use the name
			foreach (NamedReference namedReference in namedReferences)
			{
				if (namedReference.FormulaInternal != null)
					callback(null, namedReference.FormulaInternal);
			}

			this.IterateFormulasHelper(callback);
		}

		private void IterateFormulasHelper(IterateFormulasCallback callback)
		{
			// Notify the formulas in all cells which might use the name
			foreach (Worksheet worksheet in this.Worksheets)
			{
				if (worksheet.HasDataValidationRules)
				{
					foreach (DataValidationRule rule in ((IDictionary<DataValidationRule, WorksheetReferenceCollection>)worksheet.DataValidationRules).Keys)
					{
						Formula formula1 = rule.GetFormula1(null);
						Formula formula2 = rule.GetFormula2(null);

						if (formula1 != null)
							callback(worksheet, formula1);

						if (formula2 != null)
							callback(worksheet, formula2);
					}
				}

				if (worksheet.HasCommentShapes)
					this.IterateFormulas(worksheet, worksheet.CommentShapes, callback);

				this.IterateFormulas(worksheet, worksheet.Shapes, callback);

				if (worksheet.HasRows)
				{
					foreach (WorksheetRow row in worksheet.Rows)
					{
						foreach (Formula formula in row.CellOwnedFormulas)
							callback(worksheet, formula);
					}
				}
			}
		}

		private void IterateFormulas<T>(Worksheet worksheet, ICollection<T> shapes, IterateFormulasCallback callback) where T : WorksheetShape
		{
			foreach (WorksheetShape shape in shapes)
			{
				WorksheetShapeGroup group = shape as WorksheetShapeGroup;
				if (group != null)
					this.IterateFormulas(worksheet, group.Shapes, callback);

				if (shape.Obj == null)
					continue;

				if (shape.Obj.Macro != null && shape.Obj.Macro.Fmla != null)
					shape.Obj.Macro.Fmla.IterateFormulas(worksheet, callback);

				if (shape.Obj.LinkFmla != null && shape.Obj.LinkFmla.Fmla != null)
					shape.Obj.LinkFmla.Fmla.IterateFormulas(worksheet, callback);

				if (shape.Obj.List != null && shape.Obj.List.Fmla != null)
					shape.Obj.List.Fmla.IterateFormulas(worksheet, callback);

				if (shape.Obj.PictFmla != null)
				{
					if (shape.Obj.PictFmla.Fmla != null)
						shape.Obj.PictFmla.Fmla.IterateFormulas(worksheet, callback);

					if (shape.Obj.PictFmla.Key != null)
					{
						if (shape.Obj.PictFmla.Key.FmlaLinkedCell != null)
							shape.Obj.PictFmla.Key.FmlaLinkedCell.IterateFormulas(worksheet, callback);

						if (shape.Obj.PictFmla.Key.FmlaListFillRange != null)
							shape.Obj.PictFmla.Key.FmlaListFillRange.IterateFormulas(worksheet, callback);
					}
				}
			}
		}

		#endregion // IterateFormulas

		// MD 3/2/12 - 12.1 - Table Support
		#region NotifyTableDirtied

		internal void NotifyTableDirtied(WorksheetTable table)
		{
			this.CalcEngine.NotifyTableDirtied(table);
		}

		#endregion // NotifyTableDirtied

		// MD 7/14/08 - Excel formula solving
		#region NotifyValueChanged

		// MD 3/16/09 - TFS14252
		// Added a return type of bool to indicate whether any formulas were actually dirtied.
		//internal void NotifyValueChanged( CellCalcReference reference )
		internal bool NotifyValueChanged( CellCalcReference reference )
		{
			// MD 4/12/11
			// Found while fixing TFS67084
			// We don't need to do this check anymore.
			//if ( reference == null )
			//{
			//    Utilities.DebugFail( "The reference cannot be null." );
			//
			//    // MD 3/16/09 - TFS14252
			//    // The method now has a return value.
			//    //return;
			//    return false;
			//}

			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			//this.CalcEngine.NotifyValueChanged( reference );
			return this.CalcEngine.NotifyValueChanged( reference );
		}

		#endregion NotifyValueChanged

		// MD 9/9/08 - Excel 2007 Format
		#region OnAfterLoadGlobalSettings







		internal void OnAfterLoadGlobalSettings( WorkbookSerializationManager manager )
		{
			this.WindowOptions.SelectedWorksheet = this.Worksheets[ this.WindowOptions.SelectedWorksheetIndex ];

			if ( this.HasCustomViews )
			{
				foreach ( CustomView customView in this.CustomViews )
				{
					int sheetIndex;
					if ( manager.WorksheetIndices.TryGetValue( customView.WindowOptions.SelectedWorksheetTabId, out sheetIndex ) == false )
					{
						// MD 1/24/12 - TFS99944
						// We can't return here because we now to other things below.
						//return;
						break;
					}

					customView.WindowOptions.SelectedWorksheet = this.Worksheets[ sheetIndex ];
				}
			}

			// MD 8/20/07 - BR25818
			// After the workbook global section has been deserialized, resolve all 
			// named reference names in other named reference formulas.
			manager.ResolveNamedReferences();

			// MD 3/13/12 - 12.1 - Table Support
			manager.Workbook.CellFormats.DefaultElement = manager.GetLoadedDefaultCellFormat();
			manager.Workbook.Fonts.DefaultElement = manager.Fonts[0];
		} 

		#endregion OnAfterLoadGlobalSettings

		// MD 2/27/12 - 12.1 - Table Support
		#region OnCellsShifted

		internal void OnCellsShifted(CellShiftOperation shiftOperation, ReferenceShiftType shiftType)
		{
			this.IterateFormulas(new OnCellsShiftedHelper(shiftOperation, shiftType).FormulaCallback);
		}

		private class OnCellsShiftedHelper
		{
			private CellShiftOperation _shiftOperation;
			private ReferenceShiftType _shiftType;

			public OnCellsShiftedHelper(CellShiftOperation shiftOperation, ReferenceShiftType shiftType)
			{
				_shiftOperation = shiftOperation;
				_shiftType = shiftType;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.OnCellsShifted(owningWorksheet,
					_shiftOperation,
					_shiftType);
			}
		}

		#endregion // OnCellsShifted

		// MD 9/12/08 - TFS6887
		#region OnCurrentFormatChanged

		internal void OnCurrentFormatChanged()
		{
			// MD 5/10/12 - TFS104961
			// AddVariantDateFormats has been removed. 
			//// MD 2/15/11 - TFS66403
			//// Some of the built in formats change based on format, so update them now.
			//if (this.formats != null)
			//    this.formats.AddVariantDateFormats(this.CurrentFormat);

			// MD 1/19/12 - 12.1 - Cell Format Updates
			//// MD 1/19/11 - TFS62268
			//// Notify all font data objects that the format has changed.
			//foreach (WorkbookFontData fontData in this.Fonts)
			//    fontData.OnCurrentFormatChanged();
			//
			//// MD 1/19/11 - TFS62268
			//// We only need to notify the elements that the format changed once, so do it here once rather than have each 
			//// cell, row and column do it.
			//foreach (WorksheetCellFormatData formatData in this.CellFormats)
			//    formatData.OnCurrentFormatChanged();

			foreach ( Worksheet worksheet in this.Worksheets )
				worksheet.OnCurrentFormatChanged();
		} 

		#endregion OnCurrentFormatChanged

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region OnDefaultFontChanged

		internal void OnDefaultFontChanged()
		{
			// MD 1/11/08 - BR29105
			// Increment the version so if anything is cached based on the font height, it can be recalculated
			this.defaultFontVersion++;

			// If the default font height has changed, the zero width is now invalid.
			this.ResetZeroCharacterWidth();

			// MD 7/23/10 - TFS35969
			// If the default font height changes, all cache heights and widths are invalid, so reset them.
			foreach (Worksheet worksheet in this.Worksheets)
			{
				worksheet.Rows.ResetHeightCache(false);

				// MD 3/15/12 - TFS104581
				// The widths are no longer cached on the balance tree nodes.
				//worksheet.Columns.ResetColumnWidthCache(false);
			}
		}

		#endregion // OnDefaultFontChanged

		#region OnNamedReferenceAdded

		internal void OnNamedReferenceAdded(NamedReferenceBase namedReference)
		{
			if (namedReference.Scope == this)
				this.OnWorkbookScopedNamedItemAdded(namedReference);

			this.IterateFormulasHelper(new ConnectReferencesHelper(this).FormulaCallback);
		}

		#endregion // OnNamedReferenceAdded

		#region OnNamedReferenceRemoved

		internal void OnNamedReferenceRemoved(NamedReferenceBase namedReference)
		{
			if (namedReference.Scope == this)
				this.OnWorkbookScopedNamedItemRemoved(namedReference);

			this.IterateFormulasHelper(new OnNamedReferenceRemovedHelper(namedReference).FormulaCallback);
		}

		private class OnNamedReferenceRemovedHelper
		{
			private NamedReferenceBase _namedReference;

			public OnNamedReferenceRemovedHelper(NamedReferenceBase namedReference)
			{
				_namedReference = namedReference;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.OnNamedReferenceRemoved(_namedReference);
			}
		}

		#endregion // OnNamedReferenceRemoved

		#region OnNamedReferenceRenamed



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// MD 2/22/12 - 12.1 - Table Support
		//internal void OnNamedReferenceRenamed( NamedReference namedReference, string oldName )
		internal void OnNamedReferenceRenamed(NamedReferenceBase namedReference, string oldName)
		{
			if (namedReference.Scope == this)
			{
				if (this.workbookScopedNamedItems.Remove(oldName) == false)
					Utilities.DebugFail("The item should have been in the workbookScopedNamedItems collection.");

				Debug.Assert(
					this.workbookScopedNamedItems.ContainsKey(namedReference.Name) == false,
					"The new named reference name should not be in the workbookScopedNamedItems collection.");

				this.workbookScopedNamedItems[namedReference.Name] = namedReference;
			}

			this.RenameReference(namedReference.CalcReference);

			// MD 2/28/12 - 12.1 - Table Support
			// Refactored this code so we can reuse the code to iterate formulas.
			#region Refactored

			//// Notify the formulas of other named references which might use the name
			//for ( int i = this.NamedReferences.Count - 1; i >= 0; i-- )
			//{
			//    NamedReference subNamedReference = this.NamedReferences[ i ];

			//    if ( subNamedReference.FormulaInternal != null )
			//        subNamedReference.FormulaInternal.OnNamedReferenceRenamed( namedReference, oldName );
			//}

			//// Notify the formulas in all cells which might use the name
			//foreach ( Worksheet worksheet in this.Worksheets )
			//{
			//    if ( worksheet.HasRows == false )
			//        continue;

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
			//        //    if (formula != null)
			//        //        formula.OnNamedReferenceRenamed(namedReference, oldName);
			//        //}
			//        foreach (Formula formula in row.CellOwnedFormulas)
			//            formula.OnNamedReferenceRenamed(namedReference, oldName);
			//    }
			//}

			#endregion // Refactored
			this.IterateFormulas(new FormulaClearCacheHelper().FormulaCallback);
		}

		#endregion OnNamedReferenceRenamed

		// MD 3/30/11 - TFS69969
		#region OnSavedOrLoaded

		internal void OnSavedOrLoaded(WorkbookSerializationManager manager)
		{
			this.currentSerializationManager = null;
		}

		#endregion // OnSavedOrLoaded

		// MD 3/30/11 - TFS69969
		#region OnSavingOrLoading

		internal void OnSavingOrLoading(WorkbookSerializationManager manager)
		{
			this.currentSerializationManager = manager;
		}

		#endregion // OnSavingOrLoading

		// MD 2/6/12 - 12.1 - Cell Format Updates
		#region OnStyleRemoved

		internal void OnStyleRemoved(WorkbookStyle style)
		{
			WorkbookStyle normalStyle = this.Styles.NormalStyle;
			Debug.Assert(style != normalStyle, "The normal style should never be removed.");

			List<WorksheetCellFormatData> cellFormatsToFixUp = new List<WorksheetCellFormatData>();
			foreach (WorksheetCellFormatData cellFormat in this.CellFormats)
			{
				if (cellFormat.Style == style)
					cellFormatsToFixUp.Add(cellFormat);
			}

			bool shouldRemoveStyleManually = false;
			for (int i = 0; i < cellFormatsToFixUp.Count; i++)
			{
				WorksheetCellFormatData cellFormat = cellFormatsToFixUp[i];
				WorksheetCellFormatData cellFormatWithNormalStyle = cellFormat.CloneInternal();
				cellFormatWithNormalStyle.Style = normalStyle;

				// If a duplicate cell format with the normal style is already in the collection, we can't simply change and replace it,
				// because all cell format proxies will point to the abandoned format. We will need to walk over everything owning a cell 
				// format and change the style manually.
				if (this.CellFormats.Find(cellFormatWithNormalStyle) != null)
				{
					shouldRemoveStyleManually = true;
					continue;
				}

				this.CellFormats.Remove(cellFormat);
				cellFormat.Style = normalStyle;
				this.CellFormats.AddDirect(cellFormat);
			}

			if (shouldRemoveStyleManually)
			{
				foreach (Worksheet worksheet in this.Worksheets)
				{
					foreach (WorksheetRow row in worksheet.Rows)
					{
						if (row.HasCellFormat && row.CellFormat.Style == style)
							row.CellFormat.Style = normalStyle;

						if (row.HasCellFormatsForCells == false)
							continue;

						foreach (WorksheetCellFormatData[] cellFormatBlock in row.CellFormatsForCells.Values)
						{
							for (int i = 0; i < cellFormatBlock.Length; i++)
							{
								WorksheetCellFormatData cellFormat = cellFormatBlock[i];
								if (cellFormat == null || cellFormat.Style != style)
									continue;

								GenericCacheElementEx.BeforeSetEx(ref cellFormat, true);
								cellFormat.Style = normalStyle;
								GenericCacheElementEx.AfterSetEx(this.CellFormats, ref cellFormat);
								cellFormatBlock[i] = cellFormat;
							}
						}
					}

					foreach (WorksheetColumnBlock columnBlock in worksheet.ColumnBlocks.Values)
					{
						WorksheetCellFormatData cellFormat = columnBlock.CellFormat;
						if (cellFormat.Style != style)
							continue;

						GenericCacheElementEx.BeforeSetEx(ref cellFormat, true);
						cellFormat.Style = normalStyle;
						GenericCacheElementEx.AfterSetEx(this.CellFormats, ref cellFormat);
						columnBlock.CellFormat = cellFormat;
					}
				}
			}
		}

		#endregion // OnStyleRemoved

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnTableResizing

		internal void OnTableResizing(WorksheetTable table, List<WorksheetTableColumn> columnsBeingRemoved, List<Formula> formulasReferencingTable)
		{
			this.IterateFormulas(new OnTableResizingHelper(table, columnsBeingRemoved, formulasReferencingTable).FormulaCallback);
		}

		private class OnTableResizingHelper
		{
			private List<WorksheetTableColumn> _columnsBeingRemoved;
			private List<Formula> _formulasReferencingTable;
			private WorksheetTable _table;

			public OnTableResizingHelper(WorksheetTable table, List<WorksheetTableColumn> columnsBeingRemoved, List<Formula> formulasReferencingTable)
			{
				_table = table;
				_columnsBeingRemoved = columnsBeingRemoved;
				_formulasReferencingTable = formulasReferencingTable;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				if (formula.OnTableResizing(_table, _columnsBeingRemoved))
					_formulasReferencingTable.Add(formula);
			}
		}

		#endregion // OnTableColumnsRemoved

		// MD 3/1/12 - 12.1 - Table Support
		#region OnTableColumnsRenamed

		internal void OnTableColumnsRenamed(WorksheetTable table, List<KeyValuePair<WorksheetTableColumn, string>> changedColumnNames)
		{
			this.IterateFormulas(new OnTableColumnsRenamedHelper(table, changedColumnNames).FormulaCallback);
		}

		// MD 3/1/12 - 12.1 - Table Support
		private class OnTableColumnsRenamedHelper
		{
			private List<KeyValuePair<WorksheetTableColumn, string>> _changedColumnNames;
			private WorksheetTable _table;

			public OnTableColumnsRenamedHelper(WorksheetTable table, List<KeyValuePair<WorksheetTableColumn, string>> changedColumnNames)
			{
				_table = table;
				_changedColumnNames = changedColumnNames;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.OnTableColumnsRenamed(_table, _changedColumnNames);
			}
		}

		#endregion // OnTableColumnsRenamed

		// MD 3/2/12 - 12.1 - Table Support
		#region OnTableRemoved

		internal void OnTableRemoved(WorksheetTable table)
		{
			this.CalcEngine.NotifyTableRemoved(table);
			this.IterateFormulas(new OnTableRemovedHelper(table).FormulaCallback);

			this.OnWorkbookScopedNamedItemRemoved(table);
		}

		private class OnTableRemovedHelper
		{
			private WorksheetTable _table;

			public OnTableRemovedHelper(WorksheetTable table)
			{
				_table = table;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.ConvertTableReferencesToRanges(_table.Workbook, _table);
			}
		}

		#endregion // OnTableRemoved

		#region OnWorksheetAdded







		internal void OnWorksheetAdded( Worksheet worksheet )
		{
			// If there is currently no selected worksheet, make the newly added worksheet the 
			// selected worksheet.
			if ( this.WindowOptions.SelectedWorksheet == null )
				this.WindowOptions.SelectedWorksheet = worksheet;

			// Let all custom views in this workbook know that a new worksheet was added.
			if ( this.customViews != null )
			{
				foreach ( CustomView customView in this.customViews )
					customView.OnWorksheetAdded( worksheet );
			}
		}

		#endregion OnWorksheetAdded

		// MD 6/18/12 - TFS102878
		#region OnWorksheetMoved

		internal void OnWorksheetMoved(Worksheet worksheet, int oldIndex)
		{
			// MD 6/16/12 - CalcEngineRefactor
			this.CurrentWorkbookReference.OnWorksheetIndexesChanged();

			this.IterateFormulas(new OnWorksheetMovedHelper(worksheet, oldIndex).FormulaCallback);
		}

		private class OnWorksheetMovedHelper
		{
			private int _oldIndex;
			private Worksheet _worksheet;

			public OnWorksheetMovedHelper(Worksheet worksheet, int oldIndex)
			{
				_worksheet = worksheet;
				_oldIndex = oldIndex;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.OnWorksheetMoved(_worksheet, _oldIndex);
			}
		}

		#endregion // OnWorksheetMoved

		#region OnWorksheetRemoved



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void OnWorksheetRemoved(Worksheet worksheet, int oldIndex)
		{
			// MD 6/16/12 - CalcEngineRefactor
			this.CurrentWorkbookReference.OnWorksheetRemoved(oldIndex);

			// If the worksheet removed was the current selected worksheet, select a new worksheet 
			// if there is another.
			if ( this.WindowOptions.SelectedWorksheet == worksheet )
			{
				if ( this.Worksheets.Count > 0 )
					this.WindowOptions.SelectedWorksheet = this.Worksheets[ 0 ];
				else
					this.WindowOptions.SelectedWorksheet = null;
			}

			// MD 2/28/12 - 12.1 - Table Support
			// This code can be refactored now that we have an IterateFormulas helper method.
			#region Refactored

			//if ( this.HasNamedReferences )
			//{
			//    // If any named references has a scope of the removed worksheet, remove the named references as well.
			//    for ( int i = this.NamedReferences.Count - 1; i >= 0; i-- )
			//    {
			//        NamedReference namedReference = this.NamedReferences[ i ];

			//        if ( namedReference.Scope == worksheet )
			//        {
			//            // MD 8/21/08 - Excel formula solving
			//            // It appears this was incorrect. When the worksheet is removed, the named references scoped to it are rescoped to
			//            // the workbook.
			//            //this.NamedReferences.RemoveAt( i );
			//            namedReference.ScopeInternal = this;
			//        }

			//        // MD 8/21/08 - Excel formula solving
			//        // Let all formulas know that the workbook has been removed so they can point to the new scoped named references.
			//        if ( namedReference.FormulaInternal != null )
			//            namedReference.FormulaInternal.OnWorksheetRemoved( worksheet );
			//    }
			//}

			//// MD 8/21/08 - Excel formula solving
			//// Let all formulas know that the workbook has been removed so they can point to the new scoped named references.
			//foreach ( Worksheet subWorksheet in this.Worksheets )
			//{
			//    if ( subWorksheet.HasRows == false )
			//        continue;

			//    foreach ( WorksheetRow row in subWorksheet.Rows )
			//    {
			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //if ( row.HasCells == false )
			//        //    continue;
			//        //
			//        //foreach ( WorksheetCell cell in row.Cells )
			//        //{
			//        //    if ( cell.Formula != null )
			//        //        cell.Formula.OnWorksheetRemoved( worksheet );
			//        //}
			//        foreach (Formula formula in row.CellOwnedFormulas)
			//            formula.OnWorksheetRemoved(worksheet);
			//    }
			//}

			#endregion // Refactored
			if (this.HasNamedReferences)
			{
				// If any named references has a scope of the removed worksheet, remove the named references as well.
				for (int i = this.NamedReferences.Count - 1; i >= 0; i--)
				{
					NamedReference namedReference = this.NamedReferences[i];

					if (namedReference.Scope == worksheet)
					{
						// MD 8/21/08 - Excel formula solving
						// It appears this was incorrect. When the worksheet is removed, the named references scoped to it are rescoped to
						// the workbook.
						//this.NamedReferences.RemoveAt( i );
						namedReference.ScopeInternal = this;
					}
				}
			}

			this.IterateFormulas(new OnWorksheetRemovedHelper(worksheet, oldIndex).FormulaCallback);

			if ( this.HasCustomViews )
			{
				// Let all custom views in this workbook know that a worksheet was removed.
				foreach ( CustomView customView in this.CustomViews )
					customView.OnWorksheetRemoved( worksheet );
			}
		}

		// MD 2/28/12 - 12.1 - Table Support
		private class OnWorksheetRemovedHelper
		{
			private Worksheet _worksheet;
			private int _oldIndex;

			public OnWorksheetRemovedHelper(Worksheet worksheet, int oldIndex)
			{
				_worksheet = worksheet;
				_oldIndex = oldIndex;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.OnWorksheetRemoved(_worksheet, _oldIndex);
			}
		}

		#endregion OnWorksheetRemoved

		// MD 8/20/08 - Excel formula solving
		#region OnWorksheetRemoving







		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" )]
		internal void OnWorksheetRemoving( Worksheet worksheet )
		{
			// Clear the data from all cells on the worksheet being removed so the formulas on the cells are removed from the calc network
			// and formulas referencing the cells in the worksheet will get notified that the cells has changed.
			// MD 7/19/12 - TFS116808 (Table resizing)
			// Iterate the rows backwards so array formula and data table interior cells can still reference the source value in the 
			// top-left corner interior cell before it is removed.
			//foreach (WorksheetRow row in worksheet.Rows)
			foreach (WorksheetRow row in worksheet.Rows.GetItemsInRange(0, worksheet.Rows.MaxCount - 1, false))
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//if ( row.HasCells == false )
				//    continue;
				//
				//foreach ( WorksheetCell cell in row.Cells )
				//{
				//    if ( cell.HasCalcReference )
				//    {
				//        // MD 9/23/09 - TFS19150
				//        // The Value setter may get the row again, which will be slow, so pass in the row we already have.
				//        //cell.InternalSetValue( null, false );
				//        cell.InternalSetValue( null, row, false );
				//    }
				//}
				// MD 7/19/12 - TFS116808 (Table resizing)
				// Iterate the columns backwards so array formula and data table interior cells can still reference the source value in the 
				// top-left corner interior cell before it is removed.
				//foreach (short columnIndex in row.GetColumnIndexesWithCalcReference())
				foreach (short columnIndex in row.GetColumnIndexesWithCalcReference(false))
					row.SetCellValueRaw(columnIndex, null, false);
			}
		}

		#endregion OnWorksheetRemoving

		#region OnWorksheetRenamed



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void OnWorksheetRenamed( Worksheet worksheet, string oldName )
		{
			// MD 2/28/12 - 12.1 - Table Support
			// This code can be refactored now that we have an IterateRows helper method.
			#region Refactored

			//if ( this.HasNamedReferences )
			//{
			//    // Notify the formulas of all named references which might use the worksheet's name
			//    for ( int i = this.NamedReferences.Count - 1; i >= 0; i-- )
			//    {
			//        NamedReference namedReference = this.NamedReferences[ i ];

			//        if ( namedReference.FormulaInternal != null )
			//            namedReference.FormulaInternal.OnWorksheetRenamed( worksheet, oldName );
			//    }
			//}

			//// Notify the formulas in all cells which might use the worksheet's name
			//foreach ( Worksheet subWorksheet in this.Worksheets )
			//{
			//    if ( subWorksheet.HasRows == false )
			//        continue;

			//    foreach ( WorksheetRow row in subWorksheet.Rows )
			//    {
			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //if ( row.HasCells == false )
			//        //    continue;
			//        //
			//        //foreach ( WorksheetCell cell in row.Cells )
			//        //{
			//        //    // MD 7/14/08 - Excel formula solving
			//        //    // The Value property no longer returns the Formula instance on the cell.
			//        //    //Formula formula = cell.Value as Formula;
			//        //    Formula formula = cell.Formula;
			//        //
			//        //    if ( formula != null )
			//        //        formula.OnWorksheetRenamed( worksheet, oldName );
			//        //}
			//        foreach (Formula formula in row.CellOwnedFormulas)
			//            formula.OnWorksheetRenamed(worksheet, oldName);
			//    }
			//}

			#endregion // Refactored
			this.IterateFormulas(new FormulaClearCacheHelper().FormulaCallback);
		}

		#endregion OnWorksheetRenamed

		// MD 2/24/12 - 12.1 - Table Support
		#region ParseReference

		internal ExcelRefBase ParseReference(string reference, CellReferenceMode cellReferenceMode, Worksheet worksheet, WorksheetRow originRow, short originColumnIndex, out bool isNamedReference)
		{
			isNamedReference = false;

			Formula formula;
			// MD 4/9/12 - TFS101506
			//if (Formula.TryParse("=" + reference, cellReferenceMode, this.CurrentFormat, CultureInfo.CurrentCulture, out formula) == false)
			if (Formula.TryParse("=" + reference, cellReferenceMode, this.CurrentFormat, this.CultureResolved, out formula) == false)
				return null;

			// MD 6/16/12 - CalcEngineRefactor
			FormulaContext context = new FormulaContext(this, worksheet, originRow, originColumnIndex, formula);
			formula.ConnectReferences(context);

			return this.ParseReference(context, out isNamedReference);
		}

		internal ExcelRefBase ParseReference(FormulaContext context, out bool isNamedReference)
		{
			Formula formula = context.Formula;

			isNamedReference = false;
			if (formula.PostfixTokenList.Count == 1)
				return this.ParseReference(formula.PostfixTokenList[0], context, out isNamedReference);

			Stack<ExcelRefBase> evaluationStack = new Stack<ExcelRefBase>();
			for (int i = 0; i < formula.PostfixTokenList.Count; i++)
			{
				FormulaToken token = formula.PostfixTokenList[i];

				if (token is UnionOperator)
				{
					if (evaluationStack.Count < 2)
						return null;

					RegionGroupCalcReference reference1 = RegionGroupCalcReference.FromReference(evaluationStack.Pop());
					if (reference1 == null)
						return null;

					RegionGroupCalcReference reference2 = RegionGroupCalcReference.FromReference(evaluationStack.Pop());
					if (reference2 == null)
						return null;

					if (reference1.Worksheet != reference2.Worksheet)
						return null;

					evaluationStack.Push((ExcelRefBase)RegionGroupCalcReference.Union(reference1, reference2));
					continue;
				}

				bool temp;
				ExcelRefBase reference = this.ParseReference(token, context, out temp);
				if (reference == null)
					return null;

				evaluationStack.Push(reference);
			}

			if (evaluationStack.Count == 1)
				return evaluationStack.Peek();

			return null;
		}

		private ExcelRefBase ParseReference(FormulaToken token, FormulaContext context, out bool isNamedReference)
		{
			isNamedReference = false;

			ReferenceToken referenceToken = token as ReferenceToken;
			if (referenceToken == null || referenceToken.IsExternalReference)
				return null;

			CellReferenceToken cellReferenceToken = referenceToken as CellReferenceToken;
			if (cellReferenceToken != null)
			{
				if (cellReferenceToken.IsReferenceError)
					return null;

				if (context.OwningCellAddress == WorksheetCellAddress.InvalidReference &&
					cellReferenceToken.AreRelativeAddressesOffsets &&
					cellReferenceToken.HasRelativeAddresses)
				{
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_RelativeR1C1AddressNeedsOriginCell"));
				}
			}
			else
			{
				isNamedReference = referenceToken is NameToken || referenceToken is StructuredTableReference;
			}

			if (context.OwningCellAddress == WorksheetCellAddress.InvalidReference)
			{
				StructuredTableReference structuredTableReference = referenceToken as StructuredTableReference;
				if (structuredTableReference != null &&
					structuredTableReference.HasThisRowKeyword)
				{
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CurrentTableRowAddressNeedsOriginCell"));
				}
			}

			ExcelRefBase excelReference = referenceToken.GetCalcValue(context) as ExcelRefBase;
			if (excelReference == ExcelReferenceError.Instance)
				return null;

			return excelReference;
		}

		#endregion // ParseReference

		// MD 8/18/08 - Excel formula solving
		#region ReferencesRequiringFormulaCompilation

		internal Dictionary<ExcelRefBase, Formula> ReferencesRequiringFormulaCompilation
		{
			get { return this.referencesRequiringFormulaCompilation; }
		} 

		#endregion ReferencesRequiringFormulaCompilation

		// MD 7/14/08 - Excel formula solving
		#region RemoveFormula

		internal void RemoveFormula( IExcelCalcFormula formula )
		{
			if ( formula == null )
			{
				Utilities.DebugFail( "The formula cannot be null." );
				return;
			}

			this.CalcEngine.DeleteFormula( formula );
		}

		#endregion RemoveFormula

		// MD 8/26/08 - Excel formula solving
		#region RemoveReference

		// MD 3/30/11 - TFS69969
		//internal void RemoveReference( NamedCalcReference reference )
		internal void RemoveReference(IExcelCalcReference reference)
		{
			if ( reference == null )
			{
				Utilities.DebugFail( "The reference cannot be null." );
				return;
			}

			this.CalcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Remove );
		}

		#endregion RemoveReference

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 1/14/08 - BR29635
		//#region RemoveUsedColorIndicies

		//internal void RemoveUsedColorIndicies( List<int> unusedIndicies )
		//{
		//    this.cellFormats.RemoveUsedColorIndicies( unusedIndicies );
		//    this.fonts.RemoveUsedColorIndicies( unusedIndicies );

		//    if ( this.customViews != null )
		//    {
		//        foreach ( CustomView customView in this.customViews )
		//            customView.RemoveUsedColorIndicies( unusedIndicies );
		//    }

		//    if ( this.worksheets != null )
		//    {
		//        foreach ( Worksheet worksheet in this.worksheets )
		//            worksheet.RemoveUsedColorIndicies( unusedIndicies );
		//    }
		//}

		//#endregion RemoveUsedColorIndicies

		#endregion // Removed

		// MD 9/2/08 - Excel formula solving
		#region ResumeCalculations



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void ResumeCalculations( bool doFirstEvaluationIfInManualMode )
		{
			if ( this.calculationSuspensionCount > 0 )
				this.calculationSuspensionCount--;

			if ( this.AreCalculationsSuspended || this.referencesRequiringFormulaCompilation == null )
				return;

			foreach ( KeyValuePair<ExcelRefBase, Formula> pair in this.referencesRequiringFormulaCompilation )
				pair.Key.SetAndCompileFormula( pair.Value, doFirstEvaluationIfInManualMode );

			this.referencesRequiringFormulaCompilation = null;
		} 

		#endregion ResumeCalculations

		// MD 5/7/10 - 10.2 - Excel Templates
		#region SetCurrentFormatInternal

		internal void SetCurrentFormatInternal(WorkbookFormat format)
		{
			this.currentFormat = format;
		} 

		#endregion // SetCurrentFormatInternal

		// MD 2/22/12 - 12.1 - Table Support
		#region VerifyItemName

		internal void VerifyItemName(string name, NamedReferenceBase item)
		{
			object itemScope = this;
			if (item != null)
				itemScope = item.Scope;

			if (this.HasNamedReferences)
			{
				NamedReference[] nameMatches = this.NamedReferences.FindAll(name);

				foreach (NamedReference namedReference in nameMatches)
				{
					if (namedReference == item)
						continue;

					if (item.IsNameUniqueAcrossScopes == false &&
						namedReference.IsNameUniqueAcrossScopes == false &&
						namedReference.Scope != itemScope)
						continue;

					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_NamedReferenceNameAlreadyExists", name));
				}
			}

			// Table names are unique across scopes, so check for a table with the name regardless of what the scope is.
			WorksheetTable table = this.GetTable(name);
			if (table != null && table != item)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_NamedReferenceNameAlreadyExists", name));
		}

		#endregion // VerifyItemName

		// MD 2/24/12 - 12.1 - Table Support
		#region VerifyFormula

		internal void VerifyFormula(Formula formula, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
		{
			if (this.IsLoading)
				return;

			for (int i = 0; i < formula.PostfixTokenList.Count; i++)
			{
				StructuredTableReference tableReference = formula.PostfixTokenList[i] as StructuredTableReference;
				if (tableReference == null)
					continue;

				WorksheetTable table = null;
				if (tableReference.TableName == null)
				{
					if (formulaOwnerRow != null)
						table = formulaOwnerRow.GetCellAssociatedTableInternal(formulaOwnerColumnIndex);

					if (table == null)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_TableReferenceMustBeUsedFromInsideTable"));
				}
				else
				{
					table = this.GetWorkbookScopedNamedItem(tableReference.TableName) as WorksheetTable;

					if (table == null)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_TableReferenceToMissingTable"));
				}

				bool foundAllColumns = true;
				if (tableReference.SimpleColumnName != null)
				{
					foundAllColumns = (table.Columns[tableReference.SimpleColumnName] != null);
				}
				else if (tableReference.InnerStructuredReference != null)
				{
					string firstColumnName = tableReference.InnerStructuredReference.FirstColumnName;
					string lastColumnName = tableReference.InnerStructuredReference.LastColumnName;

					foundAllColumns =
						(firstColumnName == null || table.Columns[firstColumnName] != null) &&
						(lastColumnName == null || table.Columns[lastColumnName] != null);
				}

				if (foundAllColumns == false)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_TableReferenceToMissingTableColumn"));
			}
		}

		#endregion // VerifyFormula

		#region VerifyWorksheetNameIsUnique



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void VerifyWorksheetName( Worksheet excludeWorksheet, string worksheetName, string paramName )
		{
			foreach ( Worksheet worksheet in this.Worksheets )
			{
				if ( worksheet == excludeWorksheet )
					continue;

				// MD 4/6/12 - TFS101506
				//if ( String.Compare( worksheet.Name, worksheetName, StringComparison.CurrentCultureIgnoreCase ) == 0 )
				if (String.Compare(worksheet.Name, worksheetName, this.CultureResolved, CompareOptions.IgnoreCase) == 0)
					throw new ArgumentException( SR.GetString( "LE_ArgumentException_WorksheetNameAlreadyExists", worksheet.Name ), paramName );
			}
		}

		#endregion VerifyWorksheetNameIsUnique

		#endregion Internal Methods

		#region Private Methods 

		// MD 10/1/08 - TFS8453
		#region CacheAdditionalStructuredStorageFiles






		private void CacheAdditionalStructuredStorageFiles( 
			StructuredStorageManager structuredStorageManager, 
			StructuredStorage.Directory directory, 
			string directoryPath )
		{
			foreach ( DirectoryEntry entry in directory.Children )
			{
				// Determine the full path for the current entry in the structured storage system.
				string entryPath = entry.Name;
				if ( String.IsNullOrEmpty( directoryPath ) == false )
					entryPath = directoryPath + "\\" + entryPath;

				// See if the current entry is a directory.
				StructuredStorage.Directory subDirectory = 
					entry as StructuredStorage.Directory;

				// If it is a directory, cache the files in the directory and move to the next entry. We don't have to cache anything about the directory.
				if ( subDirectory != null )
				{
					this.CacheAdditionalStructuredStorageFiles( structuredStorageManager, subDirectory, entryPath );
					continue;
				}

				// Otherwise the entry must be a file
				Excel.StructuredStorage.File file = (StructuredStorage.File)entry;

				// Skip all "known" file types. We will be re-writing them out during the save operation so they don't need to be cached for round-tripping.
				switch ( entryPath )
				{
					case Workbook.Excel2003StructuredStorageAlternateWorkbookFileName:
					case Workbook.Excel2003StructuredStorageDocumentSummaryInformationFileName:
					case Workbook.Excel2003StructuredStorageSummaryInformationFileName:
					case Workbook.Excel2003StructuredStorageWorkbookFileName:
						continue;
				}

				// Lazy create the list of round-trip files.
				if ( this.cachedStructuredStorageFiles == null )
					this.cachedStructuredStorageFiles = new Dictionary<string, byte[]>();

				// Create a stream for the file and cache it's contents.
				using ( Stream stream = new UserFileStream( structuredStorageManager, file ) )
				{
					byte[] data = new byte[ stream.Length ];
					stream.Read( data, 0, data.Length );

					this.cachedStructuredStorageFiles.Add( entryPath, data );
				}
			}
		} 

		#endregion CacheAdditionalStructuredStorageFiles

		// MD 7/14/08 - Excel formula solving
		#region CreateCalcEngine

		private void CreateCalcEngine()
		{
			this.calcEngine = new ExcelCalcEngine(this);
		}

		#endregion CreateCalcEngine

		#region LoadHelper

		// MD 6/30/08 - Excel 2007 Format
		//private static Workbook LoadHelper( Stream stream, string parameterName )
		private static Workbook LoadHelper( Stream stream, WorkbookFormat format, string parameterName, IPackageFactory packageFactory, bool verifyExcel2007Xml )
		{            
            

            if (packageFactory == null)
                packageFactory = new PackageFactory();

            

			// MD 6/30/08 - Excel 2007 Format
			// Moved all code to the new LoadBIFF8File method

			if ( Enum.IsDefined( typeof( WorkbookFormat ), format ) == false )
				throw new InvalidEnumArgumentException( "format", (int)format, typeof( WorkbookFormat ) );

			// MD 1/16/12 - 12.1 - Cell Format Updates
			//Workbook workbook = new Workbook( format, WorkbookPaletteMode.StandardPalette );
			Workbook workbook = new Workbook(format);

			// MD 8/1/08 - BR35121
			// Set the flag indicating the workbook is loading
			workbook.isLoading = true;

			workbook.SuspendCalculations();

            // MBS 9/10/08 
            // Keep track of the last loading path of the workbook so that we can use it
            // when building the formulas and defined names
            workbook.lastLoadingPath = Utilities.GetFileName(stream);
			
			try
			{
				switch ( format )
				{
					case WorkbookFormat.Excel97To2003:
					// MD 5/7/10 - 10.2 - Excel Templates
					case WorkbookFormat.Excel97To2003Template:
						// MD 11/10/11 - TFS85193
						// We may need the package factory to load Excel 2007 data embedded in the BIFF8 file.
						//Workbook.LoadBIFF8File( workbook, stream, parameterName );
						Workbook.LoadBIFF8File(workbook, stream, packageFactory, verifyExcel2007Xml, parameterName);
						break;
					case WorkbookFormat.Excel2007:
					// MD 10/1/08 - TFS8471
					case WorkbookFormat.Excel2007MacroEnabled:
					// MD 5/7/10 - 10.2 - Excel Templates
					case WorkbookFormat.Excel2007MacroEnabledTemplate:
					case WorkbookFormat.Excel2007Template:
                        Workbook.LoadXLSXFile(workbook, stream, packageFactory, verifyExcel2007Xml);
						break;
					//case WorkbookFormat.Excel2007Binary:
					//    Workbook.LoadXLSBFile( workbook, stream, parameterName );
					//    break;
					default:
						Utilities.DebugFail( "This file format is not being loaded: " + format );
						return null;
				}
			}
			finally
			{
				// MD 8/1/08 - BR35121
				// Reset the flag which indicates if the workbook is loading
				workbook.isLoading = false;

				workbook.ResumeCalculations( workbook.CalculationMode != CalculationMode.Manual );

				workbook.IterateFormulas(new ConnectReferencesHelper(workbook).FormulaCallback);
			}

			return workbook;
		}

		#endregion LoadHelper

		// MD 6/30/08 - Excel 2007 Format
		#region LoadBIFF8File

		// MD 11/10/11 - TFS85193
		// We may need the package factory to load Excel 2007 data embedded in the BIFF8 file.
		//private static void LoadBIFF8File( Workbook workbook, Stream stream, string parameterName )
		private static void LoadBIFF8File(Workbook workbook, Stream stream, IPackageFactory packageFactory, bool verifyExcel2007Xml, string parameterName)
		{
			// MD 2/3/12 - 12.1 - Cell Format Updates
			// Remove the non-automatic styles so the styles collection only has the styles loaded from the file and the automatic styles.
			for (int i = workbook.Styles.Count - 1; i >= 0; i--)
			{
				if (workbook.Styles[i].IsAutomatic == false)
					workbook.Styles.RemoveAt(i);
			}

			using ( StructuredStorageManager structuredStorageManager = new StructuredStorageManager( stream, true ) )
			// MD 5/22/07 - BR23134
			// The sub-stream might be named Workbook in some versions
			//using ( Stream workbookStream = structuredStorageManager.GetFileStream( "Workbook" ) )
			{
				// MD 10/1/08 - TFS8453
				// Moved this string to a constant
				//Stream workbookStream = structuredStorageManager.GetFileStream( "Workbook" );
				Stream workbookStream = structuredStorageManager.GetFileStream( Workbook.Excel2003StructuredStorageWorkbookFileName );

				if ( workbookStream == null )
				{
					// MD 10/1/08 - TFS8453
					// Moved this string to a constant
					//workbookStream = structuredStorageManager.GetFileStream( "Book" );
					workbookStream = structuredStorageManager.GetFileStream( Workbook.Excel2003StructuredStorageAlternateWorkbookFileName );
				}

				if ( workbookStream == null )
					throw new ArgumentException( SR.GetString( "LE_ArgumentException_FileDoesntContainsWorkbookStream" ), parameterName );

				// MD 5/22/07 - BR23134
				// Put in a try block so we can manually dispose the stream (it is not created in a using anymore)
				try
				{
					workbook.DocumentProperties.Load( structuredStorageManager );

					// MD 6/30/08 - Excel 2007 Format
					// This has been moved to a helper method
					//FileStream fileStream = stream as FileStream;
					//string fileName = fileStream == null ? null : fileStream.Name;
					string fileName = Utilities.GetFileName( stream );

					// MD 11/10/11 - TFS85193
					// We may need the package factory to load Excel 2007 data embedded in the BIFF8 file.
					//using ( BIFF8WorkbookSerializationManager manager = new BIFF8WorkbookSerializationManager( workbookStream, workbook, fileName ) )
					using (BIFF8WorkbookSerializationManager manager = 
						new BIFF8WorkbookSerializationManager(workbookStream, workbook, fileName, packageFactory, verifyExcel2007Xml))
					{
						manager.Load();
					}

					// MD 10/1/08 - TFS8453
					// Cache all additional files in the structured stored file system so they can be written out again when the workbook is round-tripped.
					// Among other things, these files could contain VBA code and should be maintained.
					workbook.CacheAdditionalStructuredStorageFiles( structuredStorageManager, structuredStorageManager.RootDirectory, string.Empty );
				}
				finally
				{
					workbookStream.Close();
				}
			}
		}

		#endregion LoadBIFF8File

		// MD 6/30/08 - Excel 2007 Format
		#region LoadXLSXFile
        private static void LoadXLSXFile(Workbook workbook, Stream stream, IPackageFactory packageFactory, bool verifyExcel2007Xml)
		{
			if (packageFactory == null)
				throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_NoPackageFactory" ) );

			IPackage package = packageFactory.Open( stream, FileMode.Open );

			using ( XLSXWorkbookSerializationManager manager =
                new XLSXWorkbookSerializationManager(package, workbook, Utilities.GetFileName(stream), verifyExcel2007Xml))
			{
				manager.Load();
			}
		}
		#endregion LoadBIFF8File

		// MD 2/23/12 - 12.1 - Table Support
		#region OnWorkbookScopedNamedItemAdded

		private void OnWorkbookScopedNamedItemAdded(NamedReferenceBase item)
		{
			Debug.Assert(item.Scope == this, "The named item has the wrong scope.");
			Debug.Assert(
				this.workbookScopedNamedItems.ContainsKey(item.Name) == false,
				"The new named reference should not be in the workbookScopedNamedItems collection.");

			this.workbookScopedNamedItems[item.Name] = item;
		}

		#endregion // OnWorkbookScopedNamedItemAdded

		// MD 2/23/12 - 12.1 - Table Support
		#region OnWorkbookScopedNamedItemRemoved

		private void OnWorkbookScopedNamedItemRemoved(NamedReferenceBase item)
		{
			Debug.Assert(item.Scope == this, "The named item has the wrong scope.");
			if (this.workbookScopedNamedItems.Remove(item.Name) == false)
				Utilities.DebugFail("The item name should be in the workbookScopedNamedItems collection.");
		}

		#endregion // OnWorkbookScopedNamedItemRemoved

		#region RenameReference

		private void RenameReference(IExcelCalcReference reference)
		{
			this.CalcEngine.NotifyTopologicalChange(reference, ReferenceActionCode.Rename);
		}

		#endregion // RenameReference

		// MD 10/5/07 - BR26292
		// Moved all saving code from Save( Stream )
		#region SaveHelper

		// MD 7/2/08 - Excel 2007 Format
		private void SaveHelper( Stream stream, IPackageFactory packageFactory )
		{
			// MD 7/2/08 - Excel 2007 Format
			// Moved all code to the new LoadBIFF8File method

            

            if (packageFactory == null)
                packageFactory = new PackageFactory();

            




			string fileName = Utilities.GetFileName( stream );


			if ( fileName != null )
			{                
                // MBS 7/18/8 - Excel 2007 Format
                // If we already have the stream, we shouldn't be passing in the fileName, since the
                // user could already have the file open with a stream
                //
				//WorkbookFormat fileFormat = Utilities.GetWorkbookFormat( fileName );
                WorkbookFormat fileFormat = Utilities.GetWorkbookFormat(fileName, stream, packageFactory);

				
				//if ( (int)fileFormat != -1 && fileFormat != this.currentFormat )
				if ( ( (int)fileFormat != -1 && fileFormat != this.currentFormat ) || Path.GetExtension( fileName ).ToLower() == ".xlsb" )
				{
					throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_ExtensionDoesntMatchCurrentFormat" ) );
				}
			}

			switch ( this.currentFormat )
			{
				case WorkbookFormat.Excel97To2003:
				// MD 5/7/10 - 10.2 - Excel Templates
				case WorkbookFormat.Excel97To2003Template:
					this.SaveBIFF8File( stream );
					break;
				case WorkbookFormat.Excel2007:
				// MD 10/1/08 - TFS8471
				case WorkbookFormat.Excel2007MacroEnabled:
				// MD 5/7/10 - 10.2 - Excel Templates
				case WorkbookFormat.Excel2007MacroEnabledTemplate:
				case WorkbookFormat.Excel2007Template:
					this.SaveXLSXFile( stream, packageFactory );
					break;
				//case WorkbookFormat.Excel2007Binary:
				//    this.SaveXLSBFile( stream );
				//    break;

				default:
					Utilities.DebugFail( "This file format is not being saved: " + this.currentFormat );
					break;
			}
		}

		#endregion SaveHelper

		// MD 7/2/08 - Excel 2007 Format
		#region SaveBIFF8File

		private void SaveBIFF8File( Stream stream )
		{
			using ( StructuredStorageManager structuredStorageManager = new StructuredStorageManager( stream, false ) )
			{
				if ( this.documentProperties != null )
					this.documentProperties.Save( structuredStorageManager );

				using ( Stream workbookStream = structuredStorageManager.AddFile( "Workbook" ) )
				using ( BIFF8WorkbookSerializationManager manager = new BIFF8WorkbookSerializationManager( workbookStream, this ) )
				{
					manager.Save();
				}

				// MD 10/1/08 - TFS8453
				// Write out any cached files into the final structured storage system.
				if ( this.cachedStructuredStorageFiles != null )
				{
					foreach ( KeyValuePair<string, byte[]> cachedFile in this.cachedStructuredStorageFiles )
					{
						using ( Stream fileStream = structuredStorageManager.AddFile( cachedFile.Key ) )
							fileStream.Write( cachedFile.Value, 0, cachedFile.Value.Length );
					}
				}
			}
		} 

		#endregion SaveBIFF8File

		// MD 7/2/08 - Excel 2007 Format
		#region SaveXLSXFile
		private void SaveXLSXFile( Stream stream, IPackageFactory packageFactory )
		{
            if (packageFactory == null)
                throw new NotSupportedException("packageFactory cannot be null. When saving to Excel2007 workbook format and using the Infragistics2.Documents.Excel assembly, you must provide an IPackageFactory to handle the packaging of data. If you are using the DotNet Framework 3.0 or higher, use the Infragistics3.Documents.Excel assembly instead, and the packaging will be handled by the WindowsBase class");

			IPackage package = packageFactory.Open( stream, FileMode.Create );

            // MBS 8/19/08 - Excel 2007 Format
            // Keep track of the file name when saving as well
			//using ( XLSXWorkbookSerializationManager manager = new XLSXWorkbookSerializationManager( package, this ) )
            using (XLSXWorkbookSerializationManager manager = new XLSXWorkbookSerializationManager(package, this, Utilities.GetFileName(stream)))
				manager.Save();
		} 
		#endregion SaveXLSXFile

        // MBS 7/15/08 - Excel 2007 Format
        #region UpdateCellFormatLimit

        private void UpdateCellFormatLimit(WorkbookFormat format)
        {
            this.CellFormats.MaxCount = Workbook.GetMaxCellFormats(format);
        }
        #endregion //UpdateCellFormatLimit

        // MD 10/5/07 - BR26292
		// Moved all verification code from Save( Stream )
		#region VerifyBeforeSave

		private void VerifyBeforeSave()
		{
			#region Verify the state of the worksheets

			if ( this.Worksheets.Count == 0 )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantSaveWithNoWorksheets" ) );

			bool hasVisibleWorksheet = false;
			foreach ( Worksheet worksheet in this.Worksheets )
			{
				if ( worksheet.DisplayOptions.Visibility == WorksheetVisibility.Visible )
				{
					hasVisibleWorksheet = true;
					break;
				}
			}

			if ( hasVisibleWorksheet == false )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantSaveWithNoVisibleWorksheets" ) );

			#endregion Verify the state of the worksheets

			#region Verify the state of the custom views

			if ( this.HasCustomViews )
			{
				foreach ( CustomView customView in this.CustomViews )
				{
					bool customViewHasVisibleWorksheet = false;
					foreach ( Worksheet worksheet in this.Worksheets )
					{
						DisplayOptions displayOptions = customView.GetDisplayOptions( worksheet );

						if ( displayOptions == null )
						{
							Utilities.DebugFail( "The display options should not be null for this worksheet." );
							continue;
						}

						if ( displayOptions.Visibility == WorksheetVisibility.Visible )
						{
							customViewHasVisibleWorksheet = true;
							break;
						}
					}

					if ( customViewHasVisibleWorksheet == false )
						throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CustomViewNoVisibleWorksheets", customView.Name ) );
				}
			}

			#endregion Verify the state of the custom views
		}

		#endregion VerifyBeforeSave

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits
		internal void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			// MD 5/7/10 - 10.2 - Excel Templates
			//// MD 10/1/08 - TFS8471
			//// The macro-enabled workbook cannot be changed to a regular workbook
			//if ( this.currentFormat == WorkbookFormat.Excel2007MacroEnabled &&
			//    testFormat != WorkbookFormat.Excel97To2003 &&
			//    this.VBAData2007 != null )
			if (this.VBAData2007 != null &&
				Utilities.Is2007Format(testFormat) &&
				Utilities.IsMacroEnabledFormat(testFormat) == false)
			{
				limitErrors.AddError( SR.GetString( "LE_FormatLimitError_MacroWorkbook" ) );
			}

			if ( this.HasNamedReferences )
				this.NamedReferences.VerifyFormatLimits( limitErrors, testFormat );

			if ( this.worksheets != null )
				this.worksheets.VerifyFormatLimits( limitErrors, testFormat );                        
		}

		#endregion VerifyFormatLimits

		#endregion Private Methods

        #endregion Methods

        #region Properties

        #region Public Properties

        #region ActiveWorksheet

        /// <summary>
		/// Gets or sets the active worksheet.
		/// </summary>
		/// <remarks>
        /// <p class="body">This property is deprecated. Use <see cref="Excel.WindowOptions.SelectedWorksheet"/> instead.</p>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never )]
		[Obsolete( "Deprecated. Use the WindowOptions.SelectedWorksheet instead.", false )]
		public Worksheet ActiveWorksheet
		{
			get { return this.WindowOptions.SelectedWorksheet; }
			set { this.WindowOptions.SelectedWorksheet = value; }
		}

		#endregion ActiveWorksheet

		#region CalculationMode

		/// <summary>
		/// Gets or sets the value which indicates how a formula will be recalculated when a referenced value changes.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is set to a value of Manual, the <see cref="RecalculateBeforeSave"/> property will determine
		/// if formulas are recalculated just before saving the file.  Otherwise, that property is ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="CalculationMode"/> enumeration.
		/// </exception>
		/// <value>The value which indicates how a formula will be recalculated when a referenced value changes.</value>
		/// <seealso cref="RecalculateBeforeSave"/>
		public CalculationMode CalculationMode
		{
			get { return this.calculationMode; }
			set
			{
				if ( this.calculationMode != value )
				{
					if ( Enum.IsDefined( typeof( CalculationMode ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( CalculationMode ) );

					this.calculationMode = value;

					// MD 8/21/08 - Excel formula solving
					// When switching from one of the automatic modes to Manual, we have to make sure each formula is updated to the
					// value that it would have been if the user asked for each value.
					if ( this.calculationMode == CalculationMode.Manual )
						this.Recalculate();
				}
			}
		}

		#endregion CalculationMode

		#region CellReferenceMode

		/// <summary>
		/// Gets or sets the value which indicates the way cells in the workbook are referenced.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of this property will affect the row and columns labels of the workbook when opened in Microsoft Excel.
		/// In addition, it will affect the display of formulas referencing different cells.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <value>The value which indicates the way cells in the workbook are referenced.</value>
		public CellReferenceMode CellReferenceMode
		{
			get { return this.cellReferenceMode; }
			set
			{
				if ( this.cellReferenceMode != value )
				{
					if ( Enum.IsDefined( typeof( CellReferenceMode ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( CellReferenceMode ) );

					this.cellReferenceMode = value;
				}
			}
		}

		#endregion CellReferenceMode

		// MD 4/6/12 - TFS101506
		#region Culture

		/// <summary>
		/// Gets or sets the culture to use as the current culture for the workbook when doing any culture-aware conversions 
		/// or comparisons.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> The culture is not saved or loaded in workbook files, so this is only used at when accessing and 
		/// manipulating objects owned or associated with the Workbook.
		/// </p>
		/// </remarks>
		/// <value>The current culture for the workbook or Null to use the thread's current culture.</value>
		public CultureInfo Culture
		{
			get { return this.culture; }
			set { this.culture = value; }
		}

		#endregion // Culture

		// MD 6/31/08 - Excel 2007 Format
		
		#region CurrentFormat

		/// <summary>
		/// Gets the current format of the workbook. This is the format which will be used when saving and imposing format restrictions.
		/// </summary>
		/// <seealso cref="SetCurrentFormat(WorkbookFormat)"/>
		public WorkbookFormat CurrentFormat
		{
			get { return this.currentFormat; }
		} 

		#endregion CurrentFormat

		// MD 12/16/11 - 12.1 - Table Support
		#region CustomTableStyles

		/// <summary>
		/// Gets the collection of custom table styles in the workbook.
		/// </summary>
		/// <seealso cref="DefaultTableStyle"/>
		/// <seealso cref="StandardTableStyles"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public CustomTableStyleCollection CustomTableStyles
		{
			get
			{
				if (this.customTableStyles == null)
					this.customTableStyles = new CustomTableStyleCollection(this);

				return this.customTableStyles;
			}
		}

		#endregion // CustomTableStyles

		#region CustomViews

		/// <summary>
		/// Gets the collection of custom views for the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Each custom view stores display settings and optionally print settings, which can later be applied to the workbook
		/// and its worksheets as one operation, through both the Microsoft Excel UI and the Excel assembly by calling the 
		/// <see cref="CustomView.Apply"/> method.
		/// </p>
		/// </remarks>
		/// <value>The collection of custom views for the workbook.</value>
		/// <seealso cref="CustomView"/>
		public CustomViewCollection CustomViews
		{
			get
			{
				if ( this.customViews == null )
					this.customViews = new CustomViewCollection( this );

				return this.customViews;
			}
		}

		internal bool HasCustomViews
		{
			get
			{
				return
					this.customViews != null &&
					this.customViews.Count > 0;
			}
		}

		#endregion CustomViews

		#region DateSystem

		/// <summary>
		/// Gets or sets the date system used internally by Microsoft Excel.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="DateSystem"/> enumeration.
		/// </exception>
		/// <value>The date system used internally by Microsoft Excel.</value>
		public DateSystem DateSystem
		{
			get { return this.dateSystem; }
			set
			{
				if ( this.dateSystem != value )
				{
					if ( Enum.IsDefined( typeof( DateSystem ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( DateSystem ) );

					this.dateSystem = value;

					// MD 8/12/08 - Excel formula solving
					// When the date system changes, numbers will be converted to dates differently (and vice versa), so dirty all formulas. It would be too difficult 
					// to only dirty the formulas that will change because of this and this shouldn't change often anyway, so it is not a bad performance hit.
					if ( this.calcEngine != null )
						this.calcEngine.DirtyAllFormulas();
				}
			}
		}

		#endregion DateSystem

		#region DefaultFontHeight

		/// <summary>
		/// Obsolete. Gets or sets the default font height in twips (1/20th of a point).
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid font height range of 20 and 8180.
		/// </exception>
		/// <value>The default font height in twips (1/20th of a point).</value>
		/// <seealso cref="IWorkbookFont.Height"/>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/8/12 - 12.1 - Cell Format Updates
		[Obsolete("The Workbook.DefaultFontHeight is deprecated. The default font properties should now be set on the Workbook.Styles.NormalStyle.StyleFormat.Font.")] // MD 1/8/12 - 12.1 - Cell Format Updates
		public int DefaultFontHeight
		{
			// MD 1/8/12 - 12.1 - Cell Format Updates
			// This is no longer needed now that the default font height is exposed off the normal style.
			// Just get/set the values on the NormalStyle
			#region Old Code

			//get { return this.defaultFontHeight; }
			//set 
			//{
			//    if ( this.defaultFontHeight != value )
			//    {
			//        if ( value < 20 || 8180 < value )
			//            throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_DefaultFontHeight" ) );
			//
			//        this.defaultFontHeight = value;
			//
			//        // MD 1/11/08 - BR29105
			//        // Increment the version so if anything is cached based on the font height, it can be recalculated
			//        this.defaultFontVersion++;
			//
			//        // If the default font height has changed, the zero width is now invalid.
			//        this.ResetZeroCharacterWidth();
			//
			//        // MD 7/23/10 - TFS35969
			//        // If the default font height changes, all cache heights and widths are invalid, so reset them.
			//        foreach (Worksheet worksheet in this.Worksheets)
			//        {
			//            worksheet.Rows.ResetHeightCache(false);
			//            worksheet.Columns.ResetColumnWidthCache(false);
			//        }
			//    }
			//}

			#endregion // Old Code
			get { return this.Styles.NormalStyle.StyleFormatInternal.FontHeightResolved; }
			set { this.Styles.NormalStyle.StyleFormatInternal.Font.Height = value; }
		}

		#endregion DefaultFontHeight

		// MD 12/13/11 - 12.1 - Table Support
		#region DefaultTableStyle

		/// <summary>
		/// Gets or sets the default style for tables in the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This can be set to any <see cref="WorksheetTableStyle"/> in the <see cref="CustomTableStyles"/> or <see cref="StandardTableStyles"/> collection.
		/// </p>
		/// <p class="body">
		/// This will never return a null value. If it is set to null, it will be reset to the TableStyleMedium2 table style.
		/// </p>
		/// <p class="body">
		/// If this value is changed, it will not be applied to existing tables in the workbook. Only newly created tables will use
		/// default table style on the workbook.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The specified value is not in the <see cref="CustomTableStyles"/> or <see cref="StandardTableStyles"/> collections.
		/// </exception>
		/// <seealso cref="CustomTableStyles"/>
		/// <seealso cref="StandardTableStyles"/>
		/// <seealso cref="WorksheetTable.Style"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public WorksheetTableStyle DefaultTableStyle
		{
			get
			{
				if (this.defaultTableStyle == null)
					this.defaultTableStyle = this.StandardTableStyles.DefaultTableStyle;

				return this.defaultTableStyle;
			}
			set
			{
				if (this.defaultTableStyle == value)
					return;

				if (value != null &&
					value.IsCustom &&
					(value.CustomCollection == null || value.CustomCollection.Workbook != this))
				{
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DefaultTableStyleNotInWorkbook"), "value");
				}

				this.defaultTableStyle = value;
			}
		}

		#endregion // DefaultTableStyle

		#region DocumentProperties

		/// <summary>
		/// Gets the properties associated with the workbook document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The document properties are pieces of information which provide details on the content of the workbook,
		/// such as the author, title, and subject of the workbook.
		/// </p>
		/// </remarks>
		/// <value>The properties associated with the workbook document.</value>
		public DocumentProperties DocumentProperties
		{
			get
			{
				if ( this.documentProperties == null )
					this.documentProperties = new DocumentProperties();

				return this.documentProperties;
			}
		}

		#endregion DocumentProperties

		// 8/5/08 - Excel formula solving
		#region IsValidFunctionName

		/// <summary>
		/// Gets a value indicating whether the specified function will be recognized and solved by Microsoft Excel when the workbook is saved out.
		/// </summary>
		/// <param name="functionName">The case-insensitive name of the function.</param>
		/// <returns>True if the function will be recognized in Microsoft Excel; False otherwise.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" )]
		public bool IsValidFunctionName( string functionName )
		{
			Function function = Function.GetFunction( functionName );
			return function != null && function.IsUnknownAddInFunction == false;
		} 

		#endregion IsValidFunctionName

		#region IterativeCalculationsEnabled

		/// <summary>
		/// Gets or sets the value which indicates whether iterations are allowed while calculating formulas containing 
		/// circular references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When iterative calculations are enabled, a formula is allowed to use circular references, 
		/// or directly or indirectly reference the cell to which it belongs. Microsoft Excel stops iteratively
		/// calculating formulas after iterating <see cref="MaxRecursionIterations"/> times or when all formula
		/// values change by less than <see cref="MaxChangeInIteration"/> between two iterations.
		/// </p>
		/// <p class="body">
		/// When iterative calculations are disabled, circular references are not allowed, and a formula which 
		/// references the cell to which it belongs, directly or indirectly, will cause Microsoft Excel to show an 
		/// error message and the cell will contain a <see cref="ErrorValue.Circularity">Circularity</see> error.
		/// </p>
		/// </remarks>
		/// <value>
		/// The value which indicates whether iterations are allowed while calculating recursive formulas.
		/// </value>
		/// <seealso cref="ErrorValue.Circularity"/>
		/// <seealso cref="MaxChangeInIteration"/>
		/// <seealso cref="MaxRecursionIterations"/>
		public bool IterativeCalculationsEnabled
		{
			get { return this.iterativeCalculationsEnabled; }
			// MD 8/20/08 - Excel formula solving
			//set { this.iterativeCalculationsEnabled = value; }
			set 
			{
				if ( this.iterativeCalculationsEnabled == value )
					return;
 
				this.iterativeCalculationsEnabled = value;
			}
		}

		#endregion IterativeCalculationsEnabled

		#region MaxChangeInIteration

		/// <summary>
		/// Gets or sets the maximum change of the values in a formula between iterations which will exit from iteration.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This property is only valid when <see cref="IterativeCalculationsEnabled"/> is True. Otherwise it is ignored.
		/// </p>
		/// <p class="body">
		/// When iterative calculations, or circular references, are enabled, this property determines the maximum change in 
		/// all values of a formula between two iterations that will cause the formula to exit iterative calculations. Iterative
		/// calculations will also be stopped if the formula iterates <see cref="MaxRecursionIterations"/> times.
		/// </p>
		/// </remarks>
		/// <value>The maximum change of the values in a formula between iterations which will exit from iteration.</value>
		/// <seealso cref="IterativeCalculationsEnabled"/>
		/// <seealso cref="MaxRecursionIterations"/>
		public double MaxChangeInIteration
		{
			get { return this.maxChangeInIteration; }
			// MD 8/20/08 - Excel formula solving
			//set { this.maxChangeInIteration = value; }
			set 
			{
				if ( this.maxChangeInIteration == value )
					return;

				this.maxChangeInIteration = value;
			}
		}

		#endregion MaxChangeInIteration

		#region MaxRecursionIterations

		/// <summary>
		/// Gets or sets the maximum number of times formulas should be iteratively calculated.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This property is only valid when <see cref="IterativeCalculationsEnabled"/> is True. Otherwise it is ignored.
		/// </p>
		/// <p class="body">
		/// When iterative calculations, or circular references, are enabled, this property determines the number of iterations
		/// allowed when calculating iteratively.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of 1 and 32767.
		/// </exception>
		/// <value>The maximum number of times formulas should be iteratively calculated.</value>
		/// <seealso cref="IterativeCalculationsEnabled"/>
		/// <seealso cref="MaxChangeInIteration"/>
		public int MaxRecursionIterations
		{
			get { return this.maxRecursionIterations; }
			set
			{
				// MD 2/15/12 - TFS101854
				// When loading, make sure the value is within range before setting it.
				if (this.IsLoading)
					value = Math.Min(Math.Max(1, Utilities.ToInteger(value)), Int16.MaxValue);

				if ( this.maxRecursionIterations != value )
				{
					if ( value < 1 || 32767 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_MaxRecursionIterations" ) );
					
					this.maxRecursionIterations = value;
				}
			}
		}

		#endregion MaxRecursionIterations

		#region NamedReferences

		/// <summary>
		/// Gets the collection of named references in the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Named references are typically used to refer to cells or ranges of cells by name.
		/// The named reference names are used by formulas instead of explicitly naming the 
		/// cells or cell ranges.
		/// </p>
		/// </remarks>
		/// <value>The collection of named references in the workbook.</value>
		/// <seealso cref="NamedReference"/>
		public NamedReferenceCollection NamedReferences
		{
			get
			{
				if ( this.namedReferences == null )
					this.namedReferences = new NamedReferenceCollection( this );

				return this.namedReferences;
			}
		}

		internal bool HasNamedReferences
		{
			get
			{
				return
					this.namedReferences != null &&
					this.namedReferences.Count > 0;
			}
		}

		#endregion NamedReferences

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region Palette

		/// <summary>
		/// Gets the color palette used when the saved file is opened in Microsoft Excel 2003 and earlier versions.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When the file is opened in Microsoft Excel 2003 and earlier versions, the actual colors used in cells and shapes may not be displayed. 
		/// Instead, the closest color in the palette will be displayed instead. Therefore, the palette can be customized if necessary to keep the 
		/// colors as accurate as possible in older versions of Excel.
		/// </p>
		/// </remarks>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		public WorkbookColorPalette Palette
		{
			get { return this.palette; }
		}

		#endregion // Palette

		#region Precision

		/// <summary>
		/// Gets or sets the precision to use when obtaining a cell's value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The precision determines whether to use the actual value of the cell or the display value of the cell.
		/// These are typically the same, but the format of a cell could cause a loss of precision in the displayed
		/// value.  For example, if a cell's value is 18.975, and a currency format is used for the cell, the display 
		/// value will be 18.98.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="T:Precision"/> enumeration.
		/// </exception>
		/// <value>The precision to use when obtaining a cell's value.</value>
		public Precision Precision
		{
			get { return this.precision; }
			set 
			{
				if ( this.precision != value )
				{
					if ( Enum.IsDefined( typeof( Precision ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( Precision ) );

					this.precision = value;
				}
			}
		}

		#endregion Precision

		#region Protected

		/// <summary>
		/// Gets or sets the value which indicates whether the workbook is protected.
		/// </summary>
		/// <remarks>
		/// <p class="body">If True, prevents changes to worksheet and to locked cells.</p>
		/// </remarks>
		/// <value>The value which indicates whether the workbook is protected.</value>
		public bool Protected
		{
			get { return this.protectedMbr; }
			set { this.protectedMbr = value; }
		}

		#endregion Protected

		#region RecalculateBeforeSave

		/// <summary>
		/// Gets or sets the value which indicates whether the workbook should recalculate all formulas before saving.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This property only applies if the <see cref="CalculationMode"/> is set to Manual.  Otherwise, it is ignored.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether the workbook should recalculate all formulas before saving.</value>
		/// <seealso cref="Recalculate"/>
		/// <seealso cref="Workbook.CalculationMode"/>
		public bool RecalculateBeforeSave
		{
			get { return this.recalculateBeforeSave; }
			set { this.recalculateBeforeSave = value; }
		}

		#endregion RecalculateBeforeSave

		#region SaveExternalLinkedValues

		/// <summary>
		/// Gets or sets the value which indicates whether to save values linked from external workbooks.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value will only be used when the workbook is opened in Microsoft Excel. When referencing external values
		/// and saving a workbook through the Excel assembly, external linked values will never be saved.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether to save values linked from external workbooks.</value>
		public bool SaveExternalLinkedValues
		{
			get { return this.saveExternalLinkedValues; }
			set { this.saveExternalLinkedValues = value; }
		}

		#endregion SaveExternalLinkedValues

		// MD 5/2/08 - BR32461/BR01870
		#region ShouldRemoveCarriageReturnsOnSave

		/// <summary>
		/// Gets or sets the value which indicates whether carriage return characters should be removed from string values in cells
		/// when the workbook is saved to an Excel file.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// In Microsoft Excel 2003, carriage return characters are displayed as boxes. Most of the time, this should not be seen
		/// and removing the carriage return characters has no adverse effect on the layout of the text within a cell. Therefore,
		/// this property is True by default.
		/// </p>
		/// </remarks>
		/// <value>
		/// True if the saved workbook file should not contain the carriage return characters from cell values; False to export the 
		/// string values as they have been set on the cells.
		/// </value>
		public bool ShouldRemoveCarriageReturnsOnSave
		{
			get { return this.shouldRemoveCarriageReturnsOnSave; }
			set { this.shouldRemoveCarriageReturnsOnSave = value; }
		} 

		#endregion ShouldRemoveCarriageReturnsOnSave

		// MD 12/16/11 - 12.1 - Table Support
		#region StandardTableStyles

		/// <summary>
		/// Gets the read-only collection of preset table styles in the workbook.
		/// </summary>
		/// <seealso cref="DefaultTableStyle"/>
		/// <seealso cref="CustomTableStyles"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public StandardTableStyleCollection StandardTableStyles
		{
			get { return StandardTableStyleCollection.Instance; }
		}

		#endregion // StandardTableStyles

		#region Styles

		/// <summary>
		/// Gets the collection of custom styles in the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Use this collection to add custom styles to Excel workbook. The user can apply those styles to different 
		/// parts of excel workbook and thereby set complex formatting with ease.
		/// </p>
		/// </remarks>
		/// <value>The collection of custom styles in the workbook.</value>
		/// <seealso cref="WorkbookStyle"/>
		public WorkbookStyleCollection Styles
		{
			get 
			{
				if ( this.styles == null )
					this.styles = new WorkbookStyleCollection( this );

				return this.styles; 
			}
		}

		#endregion Styles

		// MD 2/8/12 - 12.1 - Table Support
		#region ValidateFormatStrings

		/// <summary>
		/// Gets or sets the value indicating whether the format strings should be validated when they are set.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value is False by default to maintain backward compatibility.
		/// </p>
		/// <p class="body">
		/// When True, format strings will be validated when a <see cref="IWorksheetCellFormat.FormatString"/> property is set. An invalid
		/// format string will cause an exception. When False, invalid format strings will be allowed, but if the display text of a cell is
		/// requested, an exception will be thrown at that time. If invalid format strings are allowed and the workbook is saved and opened
		/// in Microsoft Excel, it will show an error.
		/// </p>
		/// </remarks>
		/// <seealso cref="IWorksheetCellFormat.FormatString"/>
		/// <seealso cref="WorksheetCell.GetText()"/>
		/// <seealso cref="WorksheetCell.GetText(TextFormatMode)"/>
		/// <seealso cref="WorksheetRow.GetCellText(int)"/>
		/// <seealso cref="WorksheetRow.GetCellText(int,TextFormatMode)"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public bool ValidateFormatStrings
		{
			get { return this.validateFormatStrings; }
			set { this.validateFormatStrings = value; }
		}

		#endregion // ValidateFormatStrings

		#region WindowOptions

		/// <summary>
		/// Gets the options which control various workbook level display properties.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The window options control properties of the child MDI window showing the workbook in Microsoft Excel.
		/// They also control display options of the workbook which do not change based on the selected worksheet.
		/// </p>
		/// </remarks>
		/// <value>The options which control various workbook level display properties.</value>
		/// <seealso cref="CustomView.WindowOptions"/>
		public WorkbookWindowOptions WindowOptions
		{
			get
			{
				if ( this.windowOptions == null )
					this.windowOptions = new WorkbookWindowOptions( this );

				return this.windowOptions;
			}
		}

		#endregion WindowOptions

		#region Worksheets

		/// <summary>
		/// Gets the collection of worksheets in the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
        /// Use <see cref="Excel.WindowOptions.SelectedWorksheet">WindowOptions.SelectedWorksheet</see> to set the 
		/// selected worksheet. The selected worksheet is the worksheet seen when the workbook is opened in Microsoft Excel.
		/// </p>
		/// </remarks>
		/// <value>The collection of worksheets in the workbook.</value>
		public WorksheetCollection Worksheets
		{
			get 
			{
				if ( this.worksheets == null )
					this.worksheets = new WorksheetCollection( this );

				return this.worksheets; 
			}
		}

		#endregion Worksheets

		#endregion Public Properties

		#region Internal Properties

		// MD 6/13/12 - CalcEngineRefactor
		#region AddInFunctionsWorkbookReference






		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal AddInFunctionsWorkbookReference AddInFunctionsWorkbookReference
		{
			get
			{
				if (this.addInFunctionsWorkbookReference == null)
					this.addInFunctionsWorkbookReference = new AddInFunctionsWorkbookReference(this);

				return this.addInFunctionsWorkbookReference;
			}
		}

		#endregion AddInFunctionsWorkbookReference

		// MD 8/18/08 - Excel formula solving
		#region AreCalculationsSuspended






		internal bool AreCalculationsSuspended
		{
			get { return this.calculationSuspensionCount > 0; }
		} 

		#endregion AreCalculationsSuspended

		#region CellFormats






		// MD 4/18/11 - TFS62026
		// The cellFormats collection is now a derived type.
		//internal GenericCachedCollection<WorksheetCellFormatData> CellFormats
		internal WorksheetCellFormatCollection CellFormats
		{
			get { return this.cellFormats; }
		}

		#endregion CellFormats

		// MD 2/10/12 - TFS97827
		// In an effort to clean up and simplify some of the column measuring logic, I am removing this because it can easily be calculated.
		#region Removed

		//// MD 7/23/10 - TFS35969
		//#region CharacterWidth256thsPerPixel
		//
		//internal double CharacterWidth256thsPerPixel
		//{
		//    get 
		//    {
		//        // If the zero character width hasn't been calculated, force it to be calculated, because that is 
		//        // where characterWidth256thsPerPixel is cached.
		//        if (this.zeroCharacterWidth == 0)
		//        {
		//            int dummy = this.ZeroCharacterWidth;
		//        }

		//        return this.characterWidth256thsPerPixel; 
		//    }
		//}
		//
		//#endregion // CharacterWidth256thsPerPixel

		#endregion // Removed

		// MD 4/6/12 - TFS101506
		#region CultureResolved

		internal CultureInfo CultureResolved
		{
			get { return this.culture ?? CultureInfo.CurrentCulture; }
		}

		#endregion // CultureResolved

		// MD 3/30/11 - TFS69969
		#region CurrentSerializationManager

		internal WorkbookSerializationManager CurrentSerializationManager
		{
			get { return this.currentSerializationManager; }
		}

		#endregion // CurrentSerializationManager

		// MD 6/13/12 - CalcEngineRefactor
		#region CurrentWorkbookReference

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal CurrentWorkbookReference CurrentWorkbookReference
		{
			get
			{
				if (this.currentWorkbookReference == null)
					this.currentWorkbookReference = new CurrentWorkbookReference(this, this.LoadingPath);

				return this.currentWorkbookReference;
			}
		}

		#endregion CurrentWorkbookReference

		// MD 11/29/11 - TFS96205
		#region CustomThemeData

		internal byte[] CustomThemeData
		{
			get { return this.customThemeData; }
			set { this.customThemeData = value; }
		}

		#endregion  // CustomThemeData

		// MD 10/8/10
		// Found while fixing TFS44359
		// Added support to round-trip custom Xml parts.
		#region CustomXmlParts

		internal List<byte[]> CustomXmlParts
		{
			get { return this.customXmlParts; }
		}

		#endregion // CustomXmlParts

		#region CustomXmlPropertiesParts

		internal List<byte[]> CustomXmlPropertiesParts
		{
			get { return this.customXmlPropertiesParts; }
		}

		#endregion // CustomXmlPropertiesParts

		// MD 1/11/08 - BR29105
		#region DefaultFontVersion

		internal int DefaultFontVersion
		{
			get { return this.defaultFontVersion; }
		}

		#endregion DefaultFontVersion

		#region DrawingProperties1






		internal List<PropertyTableBase.PropertyValue> DrawingProperties1
		{
			get { return this.drawingProperties1; }
			set { this.drawingProperties1 = value; }
		}

		#endregion DrawingProperties1

		#region DrawingProperties3






		internal List<PropertyTableBase.PropertyValue> DrawingProperties3
		{
			get { return this.drawingProperties3; }
			set { this.drawingProperties3 = value; }
		}

		#endregion DrawingProperties3

		// MD 3/30/11 - TFS69969
		#region ExternalWorkbooks

		internal Dictionary<string, ExternalWorkbookReference> ExternalWorkbooks
		{
			get { return this.externalWorkbooks; }
		}

		#endregion // ExternalWorkbooks

		#region Fonts






		// MD 2/2/12 - TFS100573
		//internal GenericCachedCollection<WorkbookFontData> Fonts
		internal GenericCachedCollectionEx<WorkbookFontData> Fonts
		{
			get { return this.fonts; }
		}

		#endregion Fonts

		#region Formats






		internal WorkbookFormatCollection Formats
		{
			get 
			{
				if ( this.formats == null )
				{
					// MD 2/15/11 - TFS66403
					// Added a parameter to the constructor.
					//this.formats = new WorkbookFormatCollection();
					// MD 2/27/12 - 12.1 - Table Support
					//this.formats = new WorkbookFormatCollection(this.CurrentFormat);
					this.formats = new WorkbookFormatCollection(this);
				}

				return this.formats; 
			}
		}

		internal bool HasFormats
		{
			get
			{
				return
					this.formats != null &&
					this.formats.Count > 0;
			}
		}

		#endregion Formats
		
		// MD 10/1/08 - TFS8471
		#region Has2003VBACode

		internal bool Has2003VBACode
		{
			get
			{
				if ( this.VBAObjectName == null )
					return false;

				if ( this.cachedStructuredStorageFiles == null )
					return false;

				string testName = "\\" + this.VBAObjectName;

				foreach ( KeyValuePair<string, byte[]> cachedFile in this.cachedStructuredStorageFiles )
				{
					if ( cachedFile.Key.EndsWith( testName ) )
						return true;
				}

				return false;
			}
		} 

		#endregion Has2003VBACode

		// MD 10/9/07 - BR27172
		#region HiddenNamedReferences

		internal List<NamedReference> HiddenNamedReferences
		{
			get
			{
				if ( this.hiddenNamedReferences == null )
					this.hiddenNamedReferences = new List<NamedReference>();

				return this.hiddenNamedReferences;
			}
		}

		internal bool HasHiddenNamedReferences
		{
			get
			{
				return
					this.hiddenNamedReferences != null &&
					this.hiddenNamedReferences.Count > 0;
			}
		}

		#endregion HiddenNamedReferences

		// MD 8/1/08 - BR35121
		#region IsLoading






		internal bool IsLoading
		{
			get { return this.isLoading; }
		} 

		#endregion IsLoading

        // MBS 9/10/08
        #region LoadingPath

        internal string LoadingPath
        {
            get { return this.lastLoadingPath; }
        }
        #endregion //LoadingPath

        // MBS 7/15/08 - Excel 2007 Format
        #region MaxCellFormats

        internal int MaxCellFormats
        {
            get { return Workbook.GetMaxCellFormats(this.CurrentFormat); }
        }
        #endregion //MaxCellFormats

        #region MaxColumnCount

        // MD 7/1/08 - Excel 2007 Format
		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		//internal int MaxColumnCount
		//{
		//    get { return Workbook.GetMaxColumnCount( this.CurrentFormat ); }
		//} 
		internal short MaxColumnCount
		{
			get { return Workbook.GetMaxColumnCountInternal(this.CurrentFormat); }
		}

		#endregion MaxColumnCount

		#region MaxRowCount

		// MD 7/1/08 - Excel 2007 Format
		internal int MaxRowCount
		{
			get { return Workbook.GetMaxRowCount( this.CurrentFormat ); }
		} 

		#endregion MaxRowCount

		// MD 2/23/12 - 12.1 - Table Support
		#region NextTableId

		internal uint NextTableId
		{
			get { return this.nextTableId; }
			set { this.nextTableId = value; }
		}

		#endregion // NextTableId

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// Moved the Palette property to the public properties section.
		#region Moved

		//        #region Palette

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the color palette used to resolve colors in the workbook.
		//        /// </summary>  
		//#endif
		//        internal WorkbookColorCollection Palette
		//        {
		//            get { return this.palette; }
		//        }

		//        #endregion Palette

		#endregion // Moved

		// MD 8/22/08 - Excel formula solving - Performance
		#region ParsedR1C1Formulas

		internal Dictionary<string, Formula> ParsedR1C1Formulas
		{
			get
			{
				if ( this.parsedR1C1Formulas == null )
					this.parsedR1C1Formulas = new Dictionary<string, Formula>();

				return this.parsedR1C1Formulas;
			}
		}

		#endregion ParsedR1C1Formulas

		#region RefreshAll






		internal bool RefreshAll
		{
			get { return this.refreshAll; }
			set { this.refreshAll = value; }
		}

		#endregion RefreshAll

		// MD 11/3/10 - TFS49093
		// Added the shared string table to the workbook so we don't have to generate it on each save.
		#region SharedStringTable

		// MD 4/12/11 - TFS67084
		// Use a derived collection for the shared string table.
		//internal GenericCachedCollection<FormattedStringElement> SharedStringTable
		internal SharedStringTable SharedStringTable
		{
			get { return this.sharedStringTable; }
		} 

		#endregion // SharedStringTable

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region ThemeColors

		internal Color[] ThemeColors
		{
			get 
			{
				if (this.themeColors == null)
				{
					this.themeColors = new Color[12];
					this.themeColors[(int)WorkbookThemeColorType.Light1] = Utilities.SystemColorsInternal.WindowColor;
					this.themeColors[(int)WorkbookThemeColorType.Dark1] = Utilities.SystemColorsInternal.WindowTextColor;
					this.themeColors[(int)WorkbookThemeColorType.Light2] = Utilities.ColorFromArgb(unchecked((int)0xFFEEECE1));
					this.themeColors[(int)WorkbookThemeColorType.Dark2] = Utilities.ColorFromArgb(unchecked((int)0xFF1F497D));
					this.themeColors[(int)WorkbookThemeColorType.Accent1] = Utilities.ColorFromArgb(unchecked((int)0xFF4F81BD));
					this.themeColors[(int)WorkbookThemeColorType.Accent2] = Utilities.ColorFromArgb(unchecked((int)0xFFC0504D));
					this.themeColors[(int)WorkbookThemeColorType.Accent3] = Utilities.ColorFromArgb(unchecked((int)0xFF9BBB59));
					this.themeColors[(int)WorkbookThemeColorType.Accent4] = Utilities.ColorFromArgb(unchecked((int)0xFF8064A2));
					this.themeColors[(int)WorkbookThemeColorType.Accent5] = Utilities.ColorFromArgb(unchecked((int)0xFF4BACC6));
					this.themeColors[(int)WorkbookThemeColorType.Accent6] = Utilities.ColorFromArgb(unchecked((int)0xFFF79646));
					this.themeColors[(int)WorkbookThemeColorType.Hyperlink] = Utilities.ColorFromArgb(unchecked((int)0xFF0000FF));
					this.themeColors[(int)WorkbookThemeColorType.FollowedHyperlink] = Utilities.ColorFromArgb(unchecked((int)0xFF800080));
				}

				return this.themeColors; 
			}
		}

		#endregion // ThemeColors

		// MD 10/1/08 - TFS8471
		#region VBAData2007

		internal byte[] VBAData2007
		{
			get { return this.vbaData2007; }
			set { this.vbaData2007 = value; }
		} 

		#endregion VBAData2007

		// MD 10/1/08 - TFS8453
		#region VBAObjectName

		internal string VBAObjectName
		{
			get { return this.vbaObjectName; }
			set { this.vbaObjectName = value; }
		} 

		#endregion VBAObjectName

		// MD 1/11/08 - BR29105
		#region ZeroCharacterWidth






		internal int ZeroCharacterWidth
		{
			get
			{
				if ( this.zeroCharacterWidth == 0 )
				{
					// MD 1/8/12 - 12.1 - Cell Format Updates
					// Cache the default font height.
					WorksheetCellFormatData normalFormat = this.Styles.NormalStyle.StyleFormatInternal;
					int defaultFontHeight = normalFormat.FontHeightResolved;

					// MD 2/10/12 - TFS97827
					float fontSize = (float)(defaultFontHeight / 20.0);
					string fontName = normalFormat.FontNameResolved;



#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

					using ( Bitmap bitmap = new Bitmap( 1, 1 ) )
					// MD 1/8/12 - 12.1 - Cell Format Updates
					// Use the default font height and name from the normal style.
					//using ( Font font = new Font( Workbook.DefaultFontName, (float)( this.DefaultFontHeight / 20.0 ) ) )
					// MD 2/10/12 - TFS97827
					//using (Font font = new Font(normalFormat.FontNameResolved, (float)(defaultFontHeight / 20.0)))
					using (Font font = new Font(fontName, fontSize))
					{
						using ( Graphics grfx = Graphics.FromImage( bitmap ) )
						{
							Size size = TextRenderer.MeasureText( grfx, "0", font, Size.Empty, TextFormatFlags.NoPadding );
							this.zeroCharacterWidth = size.Width;

							// MD 2/10/12 - TFS97827
							// Added in a few more corrections here.
							//// Unfortunately, Excel uses some different calculations to find the character width and a few sizes with 
							//// a few fonts are off by one. This causes only a slight difference when calculating the overall width of
							//// column in 1/256ths of a character width. However, the default font and size of the workbook happens to
							//// be one combination with this error, so we will manually correct just this font and size so normal 
							//// usage will display correct behavior.
							//if ( this.DefaultFontHeight == 200 &&
							//    this.zeroCharacterWidth == 8 )
							//{
							//    this.zeroCharacterWidth--;
							//}
							SortedList<float, bool> corrections;
							if (Workbook.ZeroCharacterWidthCorrections.TryGetValue(fontName, out corrections))
							{
								bool incremenet;
								if (corrections.TryGetValue(fontSize, out incremenet))
								{
									if (incremenet)
										this.zeroCharacterWidth++;
									else
										this.zeroCharacterWidth--;
								}
							}

						}
					}

					// MD 2/10/12 - TFS97827
					// We are no longer storing the characterWidth256thsPerPixel.
					//// MD 7/23/10 - TFS35969
					//// Cache this so we don't have to calculate it each time we are asking for a column width.
					//this.characterWidth256thsPerPixel = 256.0 / this.zeroCharacterWidth;
				}

				return this.zeroCharacterWidth;
			}
		}

		internal void ResetZeroCharacterWidth()
		{
			this.zeroCharacterWidth = 0;
		}

		#endregion ZeroCharacterWidth

		// MD 2/10/12 - TFS97827
		#region ZeroCharacterWidthCorrections

		[ThreadStatic]
		private static Dictionary<string, SortedList<float, bool>> zeroCharacterWidthCorrections;

		private static Dictionary<string, SortedList<float, bool>> ZeroCharacterWidthCorrections
		{
			get
			{
				if (Workbook.zeroCharacterWidthCorrections == null)
				{
					Workbook.zeroCharacterWidthCorrections = new Dictionary<string, SortedList<float, bool>>(StringComparer.InvariantCultureIgnoreCase);

					// Create the 'Arial' font corrections
					SortedList<float, bool> corrections = new SortedList<float, bool>();
					corrections[1] = true;
					corrections[3] = true;
					corrections[6] = true;
					corrections[7] = false;
					corrections[10] = false;
					corrections[13] = false;
					corrections[22] = false;
					corrections[25] = false;
					corrections[34] = false;
					corrections[37] = false;
					corrections[40] = false;
					corrections[49] = false;
					Workbook.zeroCharacterWidthCorrections["Arial"] = corrections;

					// Create the 'Calibri' font corrections
					corrections = new SortedList<float, bool>();
					corrections[1] = true;
					corrections[3] = true;
					corrections[6] = true;
					Workbook.zeroCharacterWidthCorrections["Calibri"] = corrections;

					// Create the 'Courier New' font corrections
					corrections = new SortedList<float, bool>();
					corrections[1] = true;
					corrections[3] = true;
					corrections[4] = false;
					corrections[7] = false;
					corrections[13] = false;
					corrections[19] = false;
					corrections[22] = false;
					corrections[28] = false;
					corrections[34] = false;
					corrections[37] = false;
					corrections[43] = false;
					corrections[49] = false;
					Workbook.zeroCharacterWidthCorrections["Courier New"] = corrections;

					// Create the 'Times New Roman' font corrections
					corrections = new SortedList<float, bool>();
					corrections[1] = true;
					corrections[3] = true;
					corrections[6] = true;
					corrections[10] = false;
					corrections[19] = false;
					corrections[22] = false;
					Workbook.zeroCharacterWidthCorrections["Times New Roman"] = corrections;

					// Create the 'Tahoma' font corrections
					corrections = new SortedList<float, bool>();
					corrections[1] = true;
					corrections[3] = true;
					corrections[6] = true;
					corrections[10] = false;
					corrections[13] = false;
					corrections[16] = false;
					corrections[25] = false;
					corrections[28] = false;
					corrections[31] = false;
					corrections[43] = false;
					corrections[46] = false;
					corrections[49] = false;
					Workbook.zeroCharacterWidthCorrections["Tahoma"] = corrections;

					// Create the 'MS PGothic' font corrections
					corrections = new SortedList<float, bool>();
					corrections[1] = true;
					corrections[3] = true;
					corrections[6] = true;
					Workbook.zeroCharacterWidthCorrections["MS PGothic"] = corrections;
					Workbook.zeroCharacterWidthCorrections[UnicodeStrings.MSPGothicJPName] = corrections;
				}

				return Workbook.zeroCharacterWidthCorrections;
			}
		}

		#endregion // ZeroCharacterWidthCorrections

		#endregion Internal Properties

		#region Private Properties

		// MD 7/14/08 - Excel formula solving
		#region CalcEngine

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		internal ExcelCalcEngine CalcEngine
		{
			get
			{
				if ( this.calcEngine == null )
					this.CreateCalcEngine();

				return this.calcEngine;
			}
		}

		#endregion CalcEngine 

		#endregion Private Properties

		#endregion Properties


		// MD 6/18/12 - TFS102878
		#region ConnectReferencesHelper class

		private class ConnectReferencesHelper
		{
			private Workbook _workbook;

			public ConnectReferencesHelper(Workbook workbook)
			{
				_workbook = workbook;
			}

			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.ConnectReferences(new FormulaContext(_workbook, formula));
			}
		}

		#endregion // ConnectReferencesHelper class

		// MD 6/16/12 - CalcEngineRefactor
		#region FormulaClearCacheHelper class

		private class FormulaClearCacheHelper
		{
			public void FormulaCallback(Worksheet owningWorksheet, Formula formula)
			{
				formula.ClearCache();
			}
		}

		#endregion // FormulaClearCacheHelper class

		// MD 6/6/11 - TFS78116
		// Moved this code from the Workbook.ZeroCharacterWidth property so it can be called on the UI thread.


#region Infragistics Source Cleanup (Region)























































#endregion // Infragistics Source Cleanup (Region)

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