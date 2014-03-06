using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;





using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	internal abstract class Excel2007WorkbookSerializationManager :
        WorkbookSerializationManager,
        IOfficeDocumentExportManager    //  BF 11/23/10 IGWordStreamer
	{
		#region Constants

        //  NA 2011.1 - Infragistics.Word
        internal const string RelationshipIdPrefix = "rId";

		#endregion Constants

		#region Member Variables

		private IPackagePart activePart;
		private Dictionary<string, object> partData = new Dictionary<string, object>();
		private Stack<PartRelationshipCounter> partRelationshipCounters = new Stack<PartRelationshipCounter>();
		private IPackage zipPackage;
		private int zipPackageRelationshipCount;

		private Dictionary<Image, Uri> imagesSavedInPackage;

        //  BF 7/8/08   OpenPackagingConventions
        private PackageConformanceManager   packageConformanceManager = null;

        private List<BorderInfo> borders;
        private List<FillInfo> fills;
		// MD 1/23/09 - TFS12619
		// Uncommented this so it can be used again.
        private List<NumberFormatInfo> numberFormats;
        private List<FormatInfo> cellStyleXfs;
        private List<FormatInfo> cellXfs;
        private List<StyleInfo> cellStyles;

		// MD 12/21/11 - 12.1 - Table Support
		// Now the Dxfs collection is stored on the base serialization manager.
        //private List<DxfInfo> dxfs;

		// MD 11/29/11 - TFS96205
		// Moved this to the WorkbookSerializationManager because we can use theme colors in XLS files as well.
        //private List<Color> themeColors;

		// MD 1/23/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
        //private List<Color> indexedColors;

        // MBS 8/12/08 
        private Dictionary<WorkbookReferenceBase, int> externalReferences;

        //  BF 8/20/08  Excel2007 Format
        Dictionary<string, WorksheetShapesHolder>   serializedShapes = null;

		// MD 3/30/10 - TFS30253
		private Dictionary<int, Formula> sharedFormulas; // Key: (si) attribute value in the "f" element; Value: shared formula

        // MRS NAS v8.3
        private bool verifyExcel2007Xml = false;

		// MD 1/19/11 - TFS62268
		private List<ColorInfo> fontColorInfos;

		// MD 3/29/11 - TFS63971
		private List<CellReferenceToken> cell3DReferenceTokens;

		// MD 12/21/11 - 12.1 - Table Support
		private bool isLoadingPresetTableStyles;

		// MD 1/8/12 - 12.1 - Cell Format Updates
		private Dictionary<int, WorkbookStyle> stylesByCellStyleXfId;

		// MD 1/10/12 - 12.1 - Cell Format Updates
		private Dictionary<WorksheetCellFormatData, ushort> cellFormatXfIds = new Dictionary<WorksheetCellFormatData, ushort>();
		private Dictionary<WorkbookStyle, ushort> styleFormatXfIds = new Dictionary<WorkbookStyle, ushort>();

		// MD 2/9/12 - TFS89375
		private FontCollection majorFonts;
		private FontCollection minorFonts;

		// MD 2/17/12 - 12.1 - Table Support
		private List<DxfInfo> dxfInfos;

		// MD 2/23/12 - TFS101504
		private List<WorkbookReferenceBase> orderedExternalReferences;

		// MD 3/4/12 - 12.1 - Cell Format Updates
		private Dictionary<FillInfo, int> fillInfos = new Dictionary<FillInfo, int>();
		private Dictionary<BorderInfo, int> borderInfos = new Dictionary<BorderInfo, int>();

		// MD 4/6/12 - TFS102169
		private List<string> loadedPartNames = new List<string>();
		private List<NamedReferenceInfo> namedReferenceInfos = new List<NamedReferenceInfo>();

		#endregion Member Variables

		#region Constructor

        protected Excel2007WorkbookSerializationManager(IPackage zipPackage, Workbook workbook, string loadingPath, bool verifyExcel2007Xml)
			: base( workbook, loadingPath )
		{            
			this.zipPackage = zipPackage;
            this.verifyExcel2007Xml = verifyExcel2007Xml;
		} 

		#endregion Constructor

		#region Base Class Overrides

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region AddFormat

		public override void AddFormat(WorksheetCellFormatData format)
		{
			this.AddFont(format.FontInternal, format);

			FormatInfo formatInfo = new FormatInfo();

			FillInfo fillInfo = FillInfo.CreateFillInfo(this, format);
			int fillId;
			if (this.fillInfos.TryGetValue(fillInfo, out fillId) == false)
			{
				fillId = this.Fills.Count;
				this.fillInfos[fillInfo] = fillId;
				this.Fills.Add(fillInfo);
			}
			formatInfo.FillId = fillId;

			BorderInfo borderInfo = BorderInfo.CreateBorderInfo(this, format);
			int borderId;
			if (this.borderInfos.TryGetValue(borderInfo, out borderId) == false)
			{
				borderId = this.Borders.Count;
				this.borderInfos[borderInfo] = borderId;
				this.Borders.Add(borderInfo);
			}
			formatInfo.BorderId = borderId;

			formatInfo.FormatOptions = format.FormatOptions;
			formatInfo.Alignment = AlignmentInfo.CreateAlignmentInfo(format);
			formatInfo.FontId = format.FontInternal.SaveIndex;
			formatInfo.Protection = ProtectionInfo.CreateProtectionInfo(format);
			formatInfo.NumFmtId = format.FormatStringIndexResolved;

			if (format.Type == WorksheetCellFormatType.StyleFormat)
			{
				this.CellStyleXfs.Add(formatInfo);
			}
			else
			{
				// Initialize the parent style XF id if the format is not a style format.
				if (format.Style != null)
					formatInfo.CellStyleXfId = this.GetStyleFormatIndex(format.Style);

				this.cellFormatXfIds.Add(format, (ushort)this.CellXfs.Count);
				this.CellXfs.Add(formatInfo);
			}
		}

		#endregion // AddFormat

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Removed the AddResolvedFormat method because we no longer need to do format resolving.
		#region Removed

		//#region AddResolvedFormat

		//// MD 3/16/09 - TFS14252
		//// Changed the return type to bool to indicate whether the item was added to the Formats collection.
		////public override void AddResolvedFormat( WorksheetCellFormatData resolvedFormat, bool ensureUniqueness )
		//public override bool AddResolvedFormat( WorksheetCellFormatData resolvedFormat, bool ensureUniqueness )
		//{
		//    // If the base returns False, the manager lists are already populated, so we can just return False here.
		//    //base.AddResolvedFormat( resolvedFormat, ensureUniqueness );
		//    if ( base.AddResolvedFormat( resolvedFormat, ensureUniqueness ) == false )
		//    {
		//        // MD 5/27/09
		//        // Found while fixing TFS17956
		//        // If we bail out here, it means the resolved format was already in the collection, but we still have 
		//        // to sync up the remaining index values which relate to the Excel 2007 format.
		//        if ( resolvedFormat.IndexInFormatCollection >= 0 )
		//            resolvedFormat.IndexInXfsCollection = this.Formats[ resolvedFormat.IndexInFormatCollection ].IndexInXfsCollection;

		//        return false;
		//    }

		//    int i = -1;

		//    // MD 1/8/12 - 12.1 - Cell Format Updates
		//    // Pass along the ensureUniqueness value.
		//    //this.PopulateManagerLists( resolvedFormat, ref i );
		//    this.PopulateManagerLists(resolvedFormat, ensureUniqueness, ref i);

		//    // MD 3/16/09 - TFS14252
		//    // We could only get in here if the base returned True, so return True as well.
		//    return true;
		//} 

		//#endregion AddResolvedFormat

		#endregion // Removed

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			// MD 12/30/11 - 12.1 - Table Support
			// The zip package could be null here.
			//( (IDisposable)this.zipPackage ).Dispose();
			if (this.zipPackage != null)
				((IDisposable)this.zipPackage).Dispose();

			base.Dispose( disposing );
		} 

		#endregion Dispose

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region GetCellFormatIndex

		public override ushort GetCellFormatIndex(WorksheetCellFormatData cellFormat)
		{
			Debug.Assert(cellFormat.Type == WorksheetCellFormatType.CellFormat, "The format is not a cell format.");
			Debug.Assert(cellFormat.Workbook == this.Workbook, "The cell format is from a different workbook.");
			Debug.Assert(this.cellFormatXfIds.ContainsKey(cellFormat), "The cell format is not being saved out.");

			ushort xfId;
			this.cellFormatXfIds.TryGetValue(cellFormat, out xfId);
			return xfId;
		}

		#endregion // GetCellFormatIndex

		// MD 3/13/12 - 12.1 - Table Support
		#region GetLoadedDefaultCellFormat

		public override WorksheetCellFormatData GetLoadedDefaultCellFormat()
		{
			if (this.CellXfs.Count == 0)
			{
				Utilities.DebugFail("This is unexpected.");
				return null;
			}

			return this.CellXfs[0].FormatDataObject;
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

        #region InitializeCellFormats






        protected override void InitializeCellFormats()
        {
			// MD 9/2/08 - Styles
			// The non and gray12.5 fill patterns must be in the list of fill patterns, so make sure they are in here.
			FillInfo fillInfo = new FillInfo();
			fillInfo.PatternFill = new PatternFillInfo();
			fillInfo.PatternFill.PatternStyle = FillPatternStyle.None;
			this.Fills.Add( fillInfo );

			// MD 5/7/12
			// Found while fixing TFS106831
			// This fill should be reused by other xfs, so add it to the dictionary.
			this.fillInfos[fillInfo] = 0;

			fillInfo = new FillInfo();
			fillInfo.PatternFill = new PatternFillInfo();
			fillInfo.PatternFill.PatternStyle = FillPatternStyle.Gray12percent;
			this.Fills.Add( fillInfo );

			// MD 5/7/12
			// Found while fixing TFS106831
			// This fill should be reused by other xfs, so add it to the dictionary.
			this.fillInfos[fillInfo] = 1;

			// MD 2/16/12 - TFS101626
			// The empty borders must be the first in the borders collection.
			//this.Borders.Add(BorderInfo.CreateBorderInfo(this, new WorksheetCellFormatData(null, WorksheetCellFormatType.StyleFormat)));
			// MD 5/7/12
			// Found while fixing TFS106831
			// This border should be reused by other xfs, so add it to the dictionary.
			BorderInfo borderInfo = BorderInfo.CreateBorderInfo(this, new WorksheetCellFormatData(null, WorksheetCellFormatType.StyleFormat));
			this.Borders.Add(borderInfo);
			this.borderInfos[borderInfo] = 0;

			// MD 12/31/11 - 12.1 - Cell Format Updates
			// Refactored this code now that I have a better understanding of how styles work.
			#region Refactored

			//WorksheetCellFormatData standardFormat = this.Workbook.CellFormats.DefaultElement.ResolvedCellFormatData();

			//// MD 10/23/07 - BR27496
			//// The first format stored in the file must reference the first font in the file, and if False if 
			//// passed in as the second argument here, that is impossible. If we don't do this, copying cells
			//// from our exported workbook to another workbook while they are both open in the same instance of 
			//// Excel 2000 will cause an unhandled exception that causes Excel to shutdown.
			////this.AddResolvedFormat( (WorksheetCellFormatData)standardFormat.Clone(), false );
			//WorksheetCellFormatData styleStandardFormat = (WorksheetCellFormatData)standardFormat.Clone();
			//this.AddResolvedFormat( styleStandardFormat, true );

			//// The default style needs to be in both the style a cell format lists, so clone it and turn off the style flag and re-add 
			//// it to the collections of formats.
			//WorksheetCellFormatData nonStyleStandardFormat = (WorksheetCellFormatData)standardFormat.Clone();
			//nonStyleStandardFormat.IsStyle = false;
			//this.AddResolvedFormat( nonStyleStandardFormat, true );

			//if (this.Workbook.HasStyles)
			//{
			//    foreach (WorkbookStyle style in Workbook.Styles)
			//    {
			//        //this.AddResolvedFormat(style.StyleFormatInternal.Element, (style is WorkbookBuiltInStyle) == false);
			//        this.AddFormat(style.StyleFormatInternal.Element, (style is WorkbookBuiltInStyle) == false);

			//        StyleInfo styleInfo = new StyleInfo();

			//        styleInfo.Name = style.Name;
			//        styleInfo.CellStyleXfId = style.StyleFormatInternal.Element.IndexInXfsCollection;

			//        Debug.Assert( styleInfo.CellStyleXfId >= 0, "The style has an invalid format Id." );

			//        WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
			//        if (builtInStyle != null)
			//        {
			//            styleInfo.OutlineStyle = Convert.ToInt32(builtInStyle.OutlineLevel);
			//            styleInfo.BuiltinId = (int)builtInStyle.Type;

			//            // MD 12/28/11 - 12.1 - Table Support
			//            styleInfo.CustomBuiltin = builtInStyle.IsCustomized;
			//        }

			//        this.CellStyles.Add(styleInfo);
			//    }


			//    // Copied from BIFF8... not sure what it does yet, so commenting it out
			//    //WorkbookStyleCollection styleCollection = Workbook.Styles.Clone();
			//    //foreach (WorkbookStyle style in styleCollection)
			//    //{
			//    //    this.Styles.Add(style);
			//    //}
			//}
			//else
			//{
			//    // At least one style needs to be in the styles collection, so if the workbook didn't have any, add in the Normal style and
			//    // have it use the default format.
			//    StyleInfo styleInfo = new StyleInfo();

			//    styleInfo.Name = "Normal";
			//    styleInfo.CellStyleXfId = styleStandardFormat.IndexInXfsCollection;

			//    this.CellStyles.Add( styleInfo );
			//}

			#endregion // Refactored
			WorkbookStyle normalStyle = this.Workbook.Styles.NormalStyle;

			this.AddStyle(normalStyle);
			Debug.Assert(this.GetStyleFormatIndex(normalStyle) == 0, "The normal style has an invalid format Id.");
			this.CellStyles.Add(new StyleInfo(this, normalStyle, false));

			foreach (WorkbookStyle style in this.Workbook.Styles)
			{
				if (style == normalStyle || style.ShouldSaveIn2007 == false)
					continue;

				this.AddStyle(style);
				Debug.Assert(this.GetStyleFormatIndex(style) >= 0, "The style has an invalid format Id.");
				this.CellStyles.Add(new StyleInfo(this, style, false));
			}

			// Save the hidden styles as well
			foreach (WorkbookStyle style in this.Workbook.Styles.GetHiddenStyles())
			{
				if (style == normalStyle)
					continue;

				this.AddStyle(style);
				Debug.Assert(this.GetStyleFormatIndex(style) >= 0, "The style has an invalid format Id.");
				this.CellStyles.Add(new StyleInfo(this, style, true));
			}

			WorksheetCellFormatCollection cellFormats = this.Workbook.CellFormats;
			this.AddFormat(cellFormats.DefaultElement);
			foreach (WorksheetCellFormatData cellFormat in cellFormats)
			{
				if (cellFormat == cellFormats.DefaultElement)
					continue;

				this.AddFormat(cellFormat);
			}

			// -------------------- End of refactoring ----------------------

			// MD 1/23/09 - TFS12619
			// Populate the NumberFormats collection.
			if ( this.Workbook.HasFormats )
			{
				foreach ( int index in this.Workbook.Formats.GetCustomFormatIndices() )
				{
					NumberFormatInfo numberFormat = new NumberFormatInfo();
					numberFormat.NumberFormatId = index;
					numberFormat.FormatCode = this.Workbook.Formats[ index ];
					this.NumberFormats.Add( numberFormat );
				}
			}
        }

        #endregion InitializeCellFormats

        #region InitializeFonts






        protected override void InitializeFonts()
        {
			// MD 1/17/12 - 12.1 - Cell Format Updates
            //WorkbookFontData defaultFontData = ((WorkbookFontData)this.Workbook.Fonts.DefaultElement).ResolvedFontData();
            //this.Fonts.Add(defaultFontData);
        } 

        #endregion InitializeFonts

        #region InitializeReferences

        protected override void InitializeReferences()
        {
            base.InitializeReferences();

            if (this.Workbook.ExternalWorkbooks != null)
            {
                // The external references that are serialized in Excel are 1-based
                int index = 1;
				foreach (ExternalWorkbookReference externalWorkbook in this.Workbook.ExternalWorkbooks.Values)
                {
                    this.ExternalReferences.Add(externalWorkbook, index++);
                }
            }
        }
        #endregion //InitializeReferences

        #region LoadWorkbookContents

        protected override void LoadWorkbookContents()
		{
			this.WorkbookReferences.Add(this.Workbook.CurrentWorkbookReference);

			this.partData = new Dictionary<string, object>();

            //  BF 7/18/08   OpenPackagingConventions
            #region Open Packaging Convention rule verification
            //
            //  Verify conformance for the package. Note that certain non-conformances
            //  can cause an exception to be thrown.
            //
            PackageConformanceManager conformance = this.PackageConformanceManager;
            conformance.VerifyPackageConformance( this.zipPackage );            






            #endregion Open Packaging Convention rule verification

			this.LoadPartRelationships( this.zipPackage.GetRelationships(), null );

			// MD 9/12/08 - 8.3 Performance
			PackageConformanceManager.ClearCachedSchemas();
		} 

		#endregion LoadWorkbookContents

		// MD 9/10/08 - Cell Comments
		#region OnlyAssignShapeIdsToCommentShapes






		public override bool OnlyAssignShapeIdsToCommentShapes
		{
			get { return true; }
		}

		#endregion OnlyAssignShapeIdsToCommentShapes

		// MD 4/28/11 - TFS62775
		#region PrepareShapeForSerialization



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public override void PrepareShapeForSerialization(ref WorksheetShape shape)
		{
			base.PrepareShapeForSerialization(ref shape);

			WorksheetChart chart = shape as WorksheetChart;
			if (chart != null)
			{
				if (chart.Excel2007RoundTripData == null)
					shape = null;

				return;
			}
		}

		#endregion  // PrepareShapeForSerialization

		#region SaveWorkbookContents

		protected override void SaveWorkbookContents( bool workbookHasShapes )
		{
			// MD 1/23/12 - 12.1 - Cell Format Updates
            //this.FillIndexedColorsList();

			this.CreatePartInPackage( ExtendedPropertiesPart.ContentTypeValue, ExtendedPropertiesPart.DefaultPartName, null );
			this.CreatePartInPackage( CorePropertiesPart.ContentTypeValue, CorePropertiesPart.DefaultPartName, null );

			// MD 10/8/10
			// Found while fixing TFS44359
			// Added support to round-trip custom Xml parts.
			this.SaveCustomXmlParts();

			this.WriteWorkbookGlobalData( workbookHasShapes );

		}

		#endregion SaveWorkbookContents

		protected override void WriteCellArrayFormula( bool isMasterCell )
		{
			throw new NotImplementedException();
		}

		protected override void WriteCellBlank()
		{
			throw new NotImplementedException();
		}

		protected override void WriteCellBoolean()
		{
			throw new NotImplementedException();
		}

		protected override void WriteCellDataTable( bool isMasterCell )
		{
			throw new NotImplementedException();
		}

		protected override void WriteCellError()
		{
			throw new NotImplementedException();
		}

		protected override void WriteCellFormattedString()
		{
			throw new NotImplementedException();
		}

		protected override void WriteCellFormula()
		{
			throw new NotImplementedException();
		}

		protected override void WriteCellNumber()
		{
			throw new NotImplementedException();
		}

		protected override bool WriteCellRK()
		{
			throw new NotImplementedException();
		}

        protected override void WriteColumnGroup()
		{
			throw new NotImplementedException();
		}

		protected override void WriteCustomViewWorkbookData()
		{
			throw new NotImplementedException();
		}

		protected override void WriteCustomViewWorksheetData( bool savePrintOptions )
		{
			throw new NotImplementedException();
		}

		protected override void WriteMagnificationLevel()
		{
			throw new NotImplementedException();
		}

		protected override bool WriteMultipleCellBlanks()
		{
			throw new NotImplementedException();
		}

		protected override bool WriteMultipleCellRKs()
		{
			throw new NotImplementedException();
		}

		protected override void WriteNamedReference( bool namedReferenceHasComment )
		{
			throw new NotImplementedException();
		}

		protected override void WritePaneInformation()
		{
			throw new NotImplementedException();
		}

		protected override void WriteStandardFormat()
		{
			throw new NotImplementedException();
		}

		// MD 11/29/11 - TFS96205
		// This is no longer needed.
		//protected override void WriteWorkbookCellFormat()
		//{
		//    throw new NotImplementedException();
		//}

		protected override void WriteWorkbookFont()
		{
			throw new NotImplementedException();
		}

		protected override void WriteWorkbookStyle()
		{
			throw new NotImplementedException();
		}

		protected override void WriteWorksheetCellComment()
		{
			throw new NotImplementedException();
		}

		#endregion Base Class Overrides

		#region Methods

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region AddStyle

		private void AddStyle(WorkbookStyle style)
		{
			this.styleFormatXfIds.Add(style, (ushort)this.CellStyleXfs.Count);
			this.AddFormat(style.StyleFormatInternal);
		}

		#endregion // AddStyle

		#region CreatePartInPackage

		internal Uri CreatePartInPackage( string contentType, string partPath, object context )
		{
			string relationshipId;
			return this.CreatePartInPackage( contentType, partPath, context, out relationshipId );
		}

		internal Uri CreatePartInPackage( string contentType, string partPath, object context, out string relationshipId )
		{
            ContentTypeBase contentTypeHandler = ContentTypeBase.GetContentType( contentType );

            if ( contentTypeHandler == null )
            {
                Utilities.DebugFail( "Cannot create content type: " + contentType );
                relationshipId = null;
                return null;
            }

            Uri partName = new Uri( partPath, UriKind.RelativeOrAbsolute );
            Debug.Assert( PackageUtilities.IsValidPartPath( partName ), "An invalid part name has been created for a part: " + partName );

            relationshipId = this.CreateRelationshipInPackage( partName, contentTypeHandler.RelationshipType );

            IPackagePart part = this.zipPackage.CreatePart( partName, contentType );
            this.partRelationshipCounters.Push( new PartRelationshipCounter( part ) );

            using ( Stream stream = part.GetStream( FileMode.Create, FileAccess.Write ) )
            {
                IPackagePart lastActivePart = this.activePart;
                this.activePart = part;

				// MD 12/30/11 - 12.1 - Table Support
				// The context should always be placed on the context stack.
				if (context != null)
					this.ContextStack.Push(context);

                contentTypeHandler.Save( this, stream );

				// MD 12/30/11 - 12.1 - Table Support
				if (context != null)
					this.ContextStack.Pop(); // context

                this.activePart = lastActivePart;
            }

            this.partRelationshipCounters.Pop();

            this.partData.Add(partPath, context);

            return partName;

        } 

        #endregion CreatePartInPackage

		#region CreateRelationshipId

        //  BF 9/30/10  NA 2011.1 - Infragistics.Word
        //  Broadened scope
		//private static string CreateRelationshipId( int currentRelationshipCount )
		internal static string CreateRelationshipId( int currentRelationshipCount )
		{
            //  BF 9/30/10  NA 2011.1 - Infragistics.Word
			//return "rId" + ( currentRelationshipCount + 1 ).ToString();
			return Excel2007WorkbookSerializationManager.RelationshipIdPrefix + ( currentRelationshipCount + 1 ).ToString();
		} 

		#endregion CreateRelationshipId

		#region CreateRelationshipInPackage

        public string CreateRelationshipInPackage(Uri targetPartName, string relationshipType)
        {
            return this.CreateRelationshipInPackage(targetPartName, relationshipType, RelationshipTargetMode.Internal);
        }

        public string CreateRelationshipInPackage(Uri targetPartName, string relationshipType, RelationshipTargetMode targetMode)
        {
            return this.CreateRelationshipInPackage(targetPartName, relationshipType, targetMode, true);
        }

        public string CreateRelationshipInPackage(Uri targetPartName, string relationshipType, RelationshipTargetMode targetMode, bool createRelativeUri)
		{
            string relationshipId;

            if ( this.partRelationshipCounters.Count == 0 )
            {
                relationshipId = Excel2007WorkbookSerializationManager.CreateRelationshipId( this.zipPackageRelationshipCount++ );

                Uri rootUri = new Uri( PackageUtilities.SegmentSeparator.ToString(), UriKind.RelativeOrAbsolute );
                Uri relativePath = PackageUtilities.GetRelativePath( rootUri, targetPartName );

				// MD 7/5/11 - TFS80111
				// We should be creating the relationship to the relative path, not the absolute path.
                //this.zipPackage.CreateRelationship( targetPartName, RelationshipTargetMode.Internal, relationshipType, relationshipId );
				this.zipPackage.CreateRelationship(relativePath, RelationshipTargetMode.Internal, relationshipType, relationshipId);
            }
            else
            {
                PartRelationshipCounter owningPartCounter = this.partRelationshipCounters.Peek();

                relationshipId = Excel2007WorkbookSerializationManager.CreateRelationshipId( owningPartCounter.RelationshipCount );
                owningPartCounter.IncrementRelationshipCount();

                if (createRelativeUri)
                {
                    Uri relativePath = PackageUtilities.GetRelativePath(owningPartCounter.Part.Uri, targetPartName);
                    owningPartCounter.Part.CreateRelationship(relativePath, targetMode, relationshipType, relationshipId);
                }
                else
                    owningPartCounter.Part.CreateRelationship(targetPartName, targetMode, relationshipType, relationshipId);
            }

            return relationshipId;

		}

		#endregion CreateRelationshipInPackage

		// MD 4/6/12 - TFS102169
		#region GetActivePartData

		internal object GetActivePartData()
		{
			if (this.ActivePart == null)
				return null;

			return this.GetPartData(this.ActivePart.Uri.OriginalString);
		}

		#endregion //GetActivePartData

		// MD 7/10/12 - TFS116306
		#region GetCellValueInfo

		public static void GetCellValueInfo(
			object cellValue,
			out ST_CellType cellType,
			out string valueToWrite)
		{
			bool valueIsNull = cellValue == null || cellValue is DBNull;
			if (valueIsNull == false)
			{
				StringElementIndex sIndex = cellValue as StringElementIndex;

				// Add the cell so that the child element can get the value
				if (sIndex != null)
				{
					cellType = ST_CellType.s;
					valueToWrite = sIndex.IndexInSharedStringTable.ToString();
				}
				else if (Utilities.IsNumericType(cellValue))
				{
					cellType = ST_CellType.n;

					try
					{
						double doubleValue = Convert.ToDouble(cellValue);
						valueToWrite = doubleValue.ToString(Workbook.InvariantFormatProvider);
					}
					catch (InvalidCastException)
					{
						Utilities.DebugFail("The numeric type should convert to a double.");
						valueToWrite = cellValue.ToString();
					}
				}
				else if (cellValue is bool)
				{
					cellType = ST_CellType.b;

					int numericValue = Convert.ToInt32((bool)cellValue);
					valueToWrite = numericValue.ToString();
				}
				else
				{
					if (cellValue is ErrorValue)
						cellType = ST_CellType.e;
					else
						cellType = ST_CellType.str;

					valueToWrite = cellValue.ToString();
				}
			}
			else
			{
				cellType = ST_CellType.n;
				valueToWrite = null;
			}
		}

		#endregion // GetCellValueInfo

		#region GetNumberedPartName

		public string GetNumberedPartName( string basePartName )
		{
			return this.GetNumberedPartName( basePartName, false );
		}

		public string GetNumberedPartName( string basePartName, bool ignoreExtension )
		{
            //  BF 10/8/10  NA 2011.1 - Infragistics.Word
            #region Refactored
            //Dictionary<string, object> partNamesWithoutExtensions = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            //if (ignoreExtension)
            //{
            //    foreach (IPackagePart part in this.zipPackage.GetParts())
            //    {
            //        string partName = part.Uri.ToString();
            //        string partNameExtension = Path.GetExtension(partName);
            //        string partNameWithoutExtension = partName.Substring(0, partName.Length - partNameExtension.Length);

            //        partNamesWithoutExtensions[partNameWithoutExtension] = null;
            //    }
            //}

            //string extension = Path.GetExtension(basePartName);
            //string beginning = basePartName.Substring(0, basePartName.Length - extension.Length);

            //int suffix = 1;
            //while (true)
            //{
            //    if (ignoreExtension)
            //    {
            //        string testValue = beginning + suffix;

            //        if (partNamesWithoutExtensions.ContainsKey(testValue) == false)
            //        {
            //            testValue += extension;
            //            Debug.Assert(PackageUtilities.IsValidPartPath(testValue), "The numbered part name generated is invalid: " + testValue);
            //            return testValue;
            //        }
            //    }
            //    else
            //    {
            //        string testValue = beginning + suffix + extension;
            //        Debug.Assert(PackageUtilities.IsValidPartPath(testValue), "The numbered part name generated is invalid: " + testValue);

            //        if (this.zipPackage.PartExists(new Uri(testValue, UriKind.RelativeOrAbsolute)) == false)
            //            return testValue;
            //    }

            //    suffix++;
            //    Debug.Assert(suffix < 5000, "Something is wrong, there shouldn't be this many numbered parts.");
            //}
            #endregion Refactored

            return Excel2007WorkbookSerializationManager.GetNumberedPartNameHelper( this.zipPackage, basePartName, ignoreExtension );
		} 

        //  BF 10/8/10  NA 2011.1 - Infragistics.Word
        internal static string GetNumberedPartNameHelper( IPackage zipPackage, string basePartName, bool ignoreExtension )
        {
            Dictionary<string, object> partNamesWithoutExtensions = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            if (ignoreExtension)
            {
                foreach (IPackagePart part in zipPackage.GetParts())
                {
                    string partName = part.Uri.ToString();
                    string partNameExtension = Path.GetExtension(partName);
                    string partNameWithoutExtension = partName.Substring(0, partName.Length - partNameExtension.Length);

                    partNamesWithoutExtensions[partNameWithoutExtension] = null;
                }
            }

            string extension = Path.GetExtension(basePartName);
            string beginning = basePartName.Substring(0, basePartName.Length - extension.Length);

            int suffix = 1;
            while (true)
            {
                if (ignoreExtension)
                {
                    string testValue = beginning + suffix;

                    if (partNamesWithoutExtensions.ContainsKey(testValue) == false)
                    {
                        testValue += extension;
                        Debug.Assert(PackageUtilities.IsValidPartPath(testValue), "The numbered part name generated is invalid: " + testValue);
                        return testValue;
                    }
                }
                else
                {
                    string testValue = beginning + suffix + extension;
                    Debug.Assert(PackageUtilities.IsValidPartPath(testValue), "The numbered part name generated is invalid: " + testValue);

                    if (zipPackage.PartExists(new Uri(testValue, UriKind.RelativeOrAbsolute)) == false)
                        return testValue;
                }

                suffix++;
                Debug.Assert(suffix < 5000, "Something is wrong, there shouldn't be this many numbered parts.");
            }
        }

        #endregion GetNumberedPartName

        #region GetPartData

		internal object GetPartData( IPackageRelationship relationship )
        {
			string absolutePath = PackageUtilities.GetTargetPartPath( relationship ).OriginalString;
			return this.GetPartData( absolutePath );
		}

		internal object GetPartData( string absolutePartPath )
		{
            if (this.partData.ContainsKey(absolutePartPath) == false)
                return null;

            return this.partData[absolutePartPath];
        }
        #endregion //GetPartData

        #region GetPartPath

        internal string GetPartPath(object context)
        {
            Dictionary<string, object>.Enumerator enumerator = this.partData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, object> currentItem = enumerator.Current;
                if (currentItem.Value == context)
                    return currentItem.Key;
            }
            return null;
        }
        #endregion //GetPartPath

        #region GetRelationshipId

        internal string GetRelationshipId(string absolutePartPath)
        {
            IEnumerable<IPackageRelationship> relationships = this.ActivePart.GetRelationships();
            if (relationships != null)
            {
                foreach (IPackageRelationship relationship in relationships)
                {
                    if (absolutePartPath == PackageUtilities.GetTargetPartPath(relationship).OriginalString)
                        return relationship.Id;
                }
            }
            return String.Empty;
        }
        #endregion //GetRelationshipId

        #region GetRelationshipDataFromActivePart

        public object GetRelationshipDataFromActivePart(string relationshipId)
        {
            if (this.ActivePart != null)
            {
                IPackageRelationship packageRelationship = this.ActivePart.GetRelationship(relationshipId);
                if (packageRelationship != null)
                {
                    if (packageRelationship.TargetMode == RelationshipTargetMode.External)
                    {
                        // Get the loading path of the manager so that we know what directory
                        // we're currently working in.
                        string path = this.FilePath;
                        if (path != null && File.Exists(path))
                            path = Path.GetDirectoryName(path);

                        // Return a new object that will allow access to various information, such as
                        // the name of the part as well as its absolute path                        
                        return new ExternalLinkPartInfo(packageRelationship.TargetUri, path);
                    }
                    else
                    {
						return this.GetPartData( packageRelationship );
                    }
                }
            }

            Utilities.DebugFail("Unable to retrieve data for the specified relationship");
            return null;
        }
        #endregion //GetRelationshipDataFromActivePart

        //  BF 8/18/08
        #region GetRelationshipPathFromActivePart

        public string GetRelationshipPathFromActivePart( string relationshipId )
        {
            IPackagePart activePart = this.ActivePart;
            IPackageRelationship packageRelationship = activePart != null ?
                activePart.GetRelationship( relationshipId ) :
                null;

            return packageRelationship != null ?
                PackageUtilities.GetTargetPartPath( packageRelationship ).OriginalString :
                null;
        }
        #endregion GetRelationshipPathFromActivePart

        #region LoadPart

        private void LoadPart( IPackagePart part )
        {
            ContentTypeBase contentTypeHandler = ContentTypeBase.GetContentType(part.ContentType);

			// Load the relationship targets before loading the part just in case the part elements need to get data from
			// the targets.
			this.LoadPartRelationships( part.GetRelationships(), contentTypeHandler );

			string partName = part.Uri.ToString();

			// Make sure we don't load the same part two times.
			// MD 4/6/12 - TFS102169
			// We may populate part data before the part is loaded, so this is no longer a valid test to prevent duplicate part
			// loading. Now we will keep a collection of the loaded parts.
			//if ( this.partData.ContainsKey( partName ) )
			if (this.loadedPartNames.Contains(partName))
				return;

			object partData = null;

			if ( contentTypeHandler != null )
			{
				using ( Stream partStream = part.GetStream( FileMode.Open, FileAccess.Read ) )
				{
					IPackagePart lastActivePart = this.activePart;
					this.activePart = part;

					partData = contentTypeHandler.Load( this, partStream );

					// MD 4/6/12 - TFS102169
					this.loadedPartNames.Add(partName);

					this.activePart = lastActivePart;
				}
			}

			// MD 4/6/12 - TFS102169
			// We may populate part data before the part is loaded, so we should just replace the part data here.
			//this.partData.Add( partName, partData );
			this.partData[partName] = partData;

			// MD 4/6/12 - TFS102169
			// Load the post load relationships and notify the part that the part and all relationships have been loaded.
			if (contentTypeHandler != null)
			{
				if (contentTypeHandler.PostLoadRelationshipTypes != null)
					this.LoadPartRelationships(part.GetRelationships(), contentTypeHandler, true);

				contentTypeHandler.OnLoadComplete(this);
			}
		}

		#endregion LoadPart

		#region LoadPartRelationships

		private void LoadPartRelationships( IEnumerable<IPackageRelationship> relationships, ContentTypeBase contentType )
		{
			// MD 4/6/12 - TFS102169
			// Moved all code to the new overload.
			this.LoadPartRelationships(relationships, contentType, false);
		}

		// MD 4/6/12 - TFS102169
		// Added a new overload to take an isPostLoad parameter indicating whether to only load the relationships which should
		// be loaded after the current active part.
		private void LoadPartRelationships(IEnumerable<IPackageRelationship> relationships, ContentTypeBase contentType, bool isPostLoad)
		{
			// MD 4/6/12 - TFS102169
			string[] postLoadRelationshipTypes = null;

            // MBS 7/25/08 
            // Sometimes we need to be able to load the relationships in a certain order,
            // since some of the targets will be dependant on other targets being
            // loaded first.
            List<IPackageRelationship> relationshipList = null;
            if (contentType != null)
            {
				// MD 4/6/12 - TFS102169
				postLoadRelationshipTypes = contentType.PostLoadRelationshipTypes;

                Dictionary<string, int> relationshipPriority = contentType.RelationshipLoadPriorityDictionary;
                if (relationshipPriority != null)
                {
                    relationshipList = new List<IPackageRelationship>(relationships);

                    // Sort the list based on the priority specified in the dictionary.  If an item is not
                    // given a priority, it will be placed after any items that do have a priority specified.
                    relationshipList.Sort( 
                        delegate(IPackageRelationship p1, IPackageRelationship p2) 
                        {
                            bool p1HasPriority = relationshipPriority.ContainsKey(p1.RelationshipType);
                            bool p2HasPriority = relationshipPriority.ContainsKey(p2.RelationshipType);
                            if (p1HasPriority == false && p2HasPriority == false)
                                return 0;
                            else if (p1HasPriority && p2HasPriority == false)
                                return -1;                            
                            else if (p1HasPriority == false && p2HasPriority)
                                return 1;

                            int p1Priority = relationshipPriority[p1.RelationshipType];
                            int p2Priority = relationshipPriority[p2.RelationshipType];
                            return p1Priority.CompareTo(p2Priority);
                        });
                }
            }
            //
            // If we haven't created the list, there's no reason to create a new duplicate
            // list, so we'll just use the list passed into the method
            IEnumerable<IPackageRelationship> relationshipsToEnumerate = null;
            if (relationshipList != null)
                relationshipsToEnumerate = relationshipList;
            else
                relationshipsToEnumerate = relationships;

            foreach (IPackageRelationship relationship in relationshipsToEnumerate)
			{
				switch ( relationship.TargetMode )
				{
					case RelationshipTargetMode.Internal:
						{
							// MD 4/6/12 - TFS102169
							// Skip all relationships which shouldn't be loaded at this time.
							bool isPostLoadContentType =
								postLoadRelationshipTypes != null &&
								Array.IndexOf<string>(postLoadRelationshipTypes, relationship.RelationshipType) >= 0;

							if (isPostLoad != isPostLoadContentType)
								continue;

                            //  BF 7/18/08  Open Packaging Conventions
                            //  The relationship itself can be non-conformant, in which case
                            //  we bypass processing of the targeted part.
                            PackageConformanceManager conformance = this.PackageConformanceManager;
                            if ( conformance.IsPartConformant(relationship.SourceUri) == false )
                                continue;

							Uri absolutePartPath = PackageUtilities.GetTargetPartPath( relationship );

                            //  BF 7/18/08  Open Packaging Conventions
                            //  If the part was determined to be non-conformant, don't load it.
							//if ( PackageUtilities.IsValidPartPath( absolutePartPath ) )
							if ( conformance.IsPartConformant(absolutePartPath) )
							{
								this.LoadPart( this.zipPackage.GetPart( absolutePartPath ) );
							}
							else
							{
                                Utilities.DebugFail("Invalid part name: " + absolutePartPath);
							}
						}
						break;

                    case RelationshipTargetMode.External:
                        // We don't need to load any external data here
						break;

					default:

                        Utilities.DebugFail("Unknown target mode on the relationship: " + relationship.TargetMode);
						break;
				}
			}
		}

		#endregion LoadPartRelationships

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//#region CreateaBorderInfo

		//private BorderInfo CreateBorderInfo(WorksheetCellFormatData formatData)
		//{
		//    if (formatData == null)
		//    {
		//        Utilities.DebugFail("Null WorksheetCellFormatData provided");
		//        return null;
		//    }

		//    if ((formatData.FormatOptions & StyleCellFormatOptions.UseBorderFormatting) != StyleCellFormatOptions.UseBorderFormatting &&
		//        formatData.FormatOptions != StyleCellFormatOptions.None)
		//        return null;

		//    // MD 12/30/11 - 12.1 - Table Support
		//    // Moved this to BorderInfo
		//    #region Moved

		//    //BorderInfo borderInfo = new BorderInfo();

		//    //// MD 10/26/11 - TFS91546
		//    //// We had a lot of duplicate logic here. Refactored it into CreateBorderInfoHelper.
		//    //#region Refactored

		//    ////borderInfo.Bottom.BorderStyle = formatData.BottomBorderStyle;
		//    ////if (formatData.BottomBorderColorIndex == -1)
		//    ////    borderInfo.Bottom.ColorInfo.Indexed = null;
		//    ////else
		//    ////    borderInfo.Bottom.ColorInfo.Indexed = (uint)formatData.BottomBorderColorIndex;
		//    ////borderInfo.Bottom.ColorInfo.RGB = formatData.BottomBorderColor;

		//    ////borderInfo.Top.BorderStyle = formatData.TopBorderStyle;
		//    ////if (formatData.TopBorderColorIndex == -1)
		//    ////    borderInfo.Top.ColorInfo.Indexed = null;
		//    ////else
		//    ////    borderInfo.Top.ColorInfo.Indexed = (uint)formatData.TopBorderColorIndex;
		//    ////borderInfo.Top.ColorInfo.RGB = formatData.TopBorderColor;

		//    ////borderInfo.Left.BorderStyle = formatData.LeftBorderStyle;
		//    ////if (formatData.LeftBorderColorIndex == -1)
		//    ////    borderInfo.Left.ColorInfo.Indexed = null;
		//    ////else
		//    ////    borderInfo.Left.ColorInfo.Indexed = (uint)formatData.LeftBorderColorIndex;
		//    ////borderInfo.Left.ColorInfo.RGB = formatData.LeftBorderColor;

		//    ////borderInfo.Right.BorderStyle = formatData.RightBorderStyle;
		//    ////if (formatData.RightBorderColorIndex == -1)
		//    ////    borderInfo.Right.ColorInfo.Indexed = null;
		//    ////else
		//    ////    borderInfo.Right.ColorInfo.Indexed = (uint)formatData.RightBorderColorIndex;
		//    ////borderInfo.Right.ColorInfo.RGB = formatData.RightBorderColor;

		//    //#endregion  // Refactored
		//    //Excel2007WorkbookSerializationManager.CreateBorderInfoHelper(
		//    //    borderInfo.Bottom, 
		//    //    formatData.BottomBorderStyle, 
		//    //    formatData.BottomBorderColor, 
		//    //    formatData.BottomBorderColorIndex);
		//    //Excel2007WorkbookSerializationManager.CreateBorderInfoHelper(
		//    //    borderInfo.Top,
		//    //    formatData.TopBorderStyle,
		//    //    formatData.TopBorderColor,
		//    //    formatData.TopBorderColorIndex);
		//    //Excel2007WorkbookSerializationManager.CreateBorderInfoHelper(
		//    //    borderInfo.Left,
		//    //    formatData.LeftBorderStyle,
		//    //    formatData.LeftBorderColor,
		//    //    formatData.LeftBorderColorIndex);
		//    //Excel2007WorkbookSerializationManager.CreateBorderInfoHelper(
		//    //    borderInfo.Right,
		//    //    formatData.RightBorderStyle,
		//    //    formatData.RightBorderColor,
		//    //    formatData.RightBorderColorIndex);

		//    //// MD 10/26/11 - TFS91546
		//    //Excel2007WorkbookSerializationManager.CreateBorderInfoHelper(
		//    //    borderInfo.Diagonal,
		//    //    formatData.DiagonalBorderStyle,
		//    //    formatData.DiagonalBorderColor,
		//    //    formatData.DiagonalBorderColorIndex);

		//    //// MD 12/22/11 - 12.1 - Cell Format Updates
		//    //// There is now a bit for when any value is set, so we can't check for zero anymore.
		//    ////if ((formatData.DiagonalBorders & DiagonalBorders.DiagonalDown) != 0)
		//    //if (Utilities.IsDiagonalDownSet(formatData.DiagonalBorders))
		//    //    borderInfo.DiagonalDown = true;

		//    //// MD 12/22/11 - 12.1 - Cell Format Updates
		//    //// There is now a bit for when any value is set, so we can't check for zero anymore.
		//    //if (Utilities.IsDiagonalUpSet(formatData.DiagonalBorders))
		//    //    borderInfo.DiagonalUp = true;
		//    //// ----------End of TFS91546-------------

		//    //return borderInfo;

		//    #endregion // Moved
		//    return BorderInfo.CreateBorderInfo(formatData);
		//}

		//// MD 12/30/11 - 12.1 - Cell Format Updates
		//// Moved this to BorderInfo
		//#region Moved

		////// MD 10/26/11 - TFS91546
		////// Refactored some duplicate code from CreateaBorderInfo.
		////private static void CreateBorderInfoHelper(BorderStyleInfo borderStyleInfo, CellBorderLineStyle borderLineStyle, Color borderColor, int borderColorIndex)
		////{
		////    borderStyleInfo.BorderStyle = borderLineStyle;

		////    if (borderColorIndex == -1)
		////        borderStyleInfo.ColorInfo.Indexed = null;
		////    else
		////        borderStyleInfo.ColorInfo.Indexed = (uint)borderColorIndex;

		////    borderStyleInfo.ColorInfo.RGB = borderColor;
		////}

		//#endregion // Moved

		//#endregion CreateaBorderInfo

		//#region CreateFillInfo

		//private FillInfo CreateFillInfo(WorksheetCellFormatData formatData)
		//{
		//    if (formatData == null)
		//    {
		//        Utilities.DebugFail("Null WorksheetCellFormatData provided");
		//        return null;
		//    }

		//    // MD 3/13/09 - TFS15324
		//    // I'm not really sure why this check was in here. It seems like we should always store the fill info for the format.
		//    // If we do not create this for cells which have some style options, but don't use formatting, they end up not serailizing
		//    // out a fillId in the xf record and this caused problems when the cells are formatted.
		//    //if ((formatData.FormatOptions & StyleCellFormatOptions.UsePatternsFormatting) != StyleCellFormatOptions.UsePatternsFormatting &&
		//    //    formatData.FormatOptions != StyleCellFormatOptions.None)
		//    //    return null;

		//    // MD 12/30/11 - 12.1 - Cell Format Updates
		//    // Moved this to FillInfo.
		//    #region Moved

		//    //FillInfo fillInfo = new FillInfo();

		//    //fillInfo.PatternFill = new PatternFillInfo();

		//    //// MD 3/19/10 - TFS26416
		//    //// This would always pass because we are only calling this with a resolved format, so no values are default.
		//    //// We should have been checking the style options, which says what was set as non-default. I will leave in the
		//    //// original check though, just in case this method is used with unresolved formats in the future.
		//    ////if (formatData.FillPattern != FillPatternStyle.Default)
		//    //if (formatData.FillPattern != FillPatternStyle.Default &&
		//    //    (formatData.FormatOptions & StyleCellFormatOptions.UsePatternsFormatting) == StyleCellFormatOptions.UsePatternsFormatting)
		//    //{
		//    //    fillInfo.PatternFill.PatternStyle = formatData.FillPattern;
		//    //    fillInfo.PatternFill.BackgroundColor = new ColorInfo();
		//    //    fillInfo.PatternFill.ForegroundColor = new ColorInfo();
		//    //    if (formatData.FillPatternBackgroundColorIndex == -1)
		//    //        fillInfo.PatternFill.BackgroundColor.Indexed = null;
		//    //    else
		//    //        fillInfo.PatternFill.BackgroundColor.Indexed = (uint)formatData.FillPatternBackgroundColorIndex;
		//    //    fillInfo.PatternFill.BackgroundColor.RGB = formatData.FillPatternBackgroundColor;

		//    //    if (formatData.FillPatternForegroundColorIndex == -1)
		//    //        fillInfo.PatternFill.ForegroundColor.Indexed = null;
		//    //    else
		//    //        fillInfo.PatternFill.ForegroundColor.Indexed = (uint)formatData.FillPatternForegroundColorIndex;
		//    //    fillInfo.PatternFill.ForegroundColor.RGB = formatData.FillPatternForegroundColor;
		//    //}
		//    //else
		//    //    fillInfo.PatternFill.PatternStyle = FillPatternStyle.None;

		//    //// TODO: where to get the gradient info from
		//    //return fillInfo;

		//    #endregion // Moved
		//    return FillInfo.CreateFillInfo(formatData);
		//}

		//#endregion CreateFillInfo

		#endregion // Removed

		// MD 1/23/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//        #region RetrieveIndexedColor

		//#if DEBUG
		//        /// <summary>
		//        /// Returns the color based on the IndexedColor palette.
		//        /// If the IndexedColors list is empty, it uses the default palette.
		//        /// </summary>
		//        /// <param name="index">Index into the indexed Colors collection</param>
		//        /// <returns>Returns the color found in the indexed palette</returns>
		//#endif
		//        public Color RetrieveIndexedColor(int index)
		//        {
		//            if (this.IndexedColors.Count != 0 &&
		//                index < this.IndexedColors.Count)
		//                return this.IndexedColors[index];


		//            // Default indexed colors table as defined on p. 2126
		//            // Not sure why the Alpha is defined as 00 in the document
		//            switch (index)
		//            {
		//                case 0:
		//                case 8:     return Color.FromArgb(255, 0, 0, 0);          //00000000
		//                case 1:
		//                case 9:     return Color.FromArgb(255, 255, 255, 255);    //00FFFFFF
		//                case 2:
		//                case 10:    return Color.FromArgb(255, 255, 0, 0);        //00FF0000
		//                case 3:
		//                case 11:    return Color.FromArgb(255, 0, 255, 0);        //0000FF00
		//                case 4: 
		//                case 12:    return Color.FromArgb(255, 0, 0, 255);        //000000FF
		//                case 5:
		//                case 13:    return Color.FromArgb(255, 255, 255, 0);      //00FFFF00
		//                case 6: 
		//                case 14:    return Color.FromArgb(255, 255, 0, 255);      //00FF00FF
		//                case 7:
		//                case 15:    return Color.FromArgb(255, 0, 255, 255);      //0000FFFF

		//                case 16:    return Color.FromArgb(255, 128, 0, 0);        //00800000
		//                case 17:    return Color.FromArgb(255, 0, 128, 0);        //00008000
		//                case 18:    return Color.FromArgb(255, 0, 0, 128);        //00000080
		//                case 19:    return Color.FromArgb(255, 128, 128, 0);      //00808000
		//                case 20:    return Color.FromArgb(255, 128, 0, 128);      //00800080
		//                case 21:    return Color.FromArgb(255, 0, 128, 128);      //00008080
		//                case 22:    return Color.FromArgb(255, 192, 192, 192);    //00C0C0C0
		//                case 23:    return Color.FromArgb(255, 128, 128, 128);    //00808080
		//                case 24:    return Color.FromArgb(255, 153, 153, 255);    //009999FF
		//                case 25:    return Color.FromArgb(255, 153, 51, 102);     //00993366
		//                case 26:    return Color.FromArgb(255, 255, 255, 204);    //00FFFFCC
		//                case 27:    return Color.FromArgb(255, 204, 255, 255);    //00CCFFFF
		//                case 28:    return Color.FromArgb(255, 102, 0, 102);      //00660066
		//                case 29:    return Color.FromArgb(255, 255, 128, 128);    //00FF8080
		//                case 30:    return Color.FromArgb(255, 0, 102, 204);      //000066CC
		//                case 31:    return Color.FromArgb(255, 204, 204, 255);    //00CCCCFF
		//                case 32:    return Color.FromArgb(255, 0, 0, 128);        //00000080
		//                case 33:    return Color.FromArgb(255, 255, 0, 255);      //00FF00FF
		//                case 34:    return Color.FromArgb(255, 255, 255, 0);      //00FFFF00
		//                case 35:    return Color.FromArgb(255, 0, 255, 255);      //0000FFFF
		//                case 36:    return Color.FromArgb(255, 128, 0, 128);      //00800080
		//                case 37:    return Color.FromArgb(255, 128, 0, 0);        //00800000
		//                case 38:    return Color.FromArgb(255, 0, 128, 128);      //00008080
		//                case 39:    return Color.FromArgb(255, 0, 0, 255);        //000000FF
		//                case 40:    return Color.FromArgb(255, 0, 204, 255);      //0000CCFF
		//                case 41:    return Color.FromArgb(255, 204, 255, 255);    //00CCFFFF
		//                case 42:    return Color.FromArgb(255, 204, 255, 204);    //00CCFFCC
		//                case 43:    return Color.FromArgb(255, 255, 255, 153);    //00FFFF99
		//                case 44:    return Color.FromArgb(255, 153, 204, 255);    //0099CCFF
		//                case 45:    return Color.FromArgb(255, 255, 153, 204);    //00FF99CC
		//                case 46:    return Color.FromArgb(255, 204, 153, 255);    //00CC99FF
		//                case 47:    return Color.FromArgb(255, 255, 204, 153);    //00FFCC99
		//                case 48:    return Color.FromArgb(255, 51, 102, 255);     //003366FF
		//                case 49:    return Color.FromArgb(255, 51, 204, 204);     //0033CCCC
		//                case 50:    return Color.FromArgb(255, 153, 204, 0);      //0099CC00
		//                case 51:    return Color.FromArgb(255, 255, 204, 0);      //00FFCC00
		//                case 52:    return Color.FromArgb(255, 255, 153, 0);      //00FF9900
		//                case 53:    return Color.FromArgb(255, 255, 102, 0);      //00FF6600
		//                case 54:    return Color.FromArgb(255, 102, 102, 153);    //00666699
		//                case 55:    return Color.FromArgb(255, 150, 150, 150);    //00969696
		//                case 56:    return Color.FromArgb(255, 0, 51, 102);       //00003366
		//                case 57:    return Color.FromArgb(255, 51, 153, 102);     //00339966
		//                case 58:    return Color.FromArgb(255, 0, 51, 0);         //00003300
		//                case 59:    return Color.FromArgb(255, 51, 51, 0);        //00333300
		//                case 60:    return Color.FromArgb(255, 153, 51, 0);       //00993300
		//                case 61:    return Color.FromArgb(255, 153, 51, 102);     //00993366
		//                case 62:    return Color.FromArgb(255, 51, 51, 153);      //00333399
		//                case 63:    return Color.FromArgb(255, 51, 51, 51);       //00333333
		//                case 64:    return Utilities.SystemColorsInternal.WindowTextColor;
		//                case 65:    return Utilities.SystemColorsInternal.DesktopColor;
		//            }

		//            return Utilities.ColorEmpty;
		//        }

		//        #endregion RetrieveIndexedColor

		#endregion // Removed

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// PopulateManagerLists is no longer needed.
		#region Removed

		//#region PopulateManagerLists

		//internal void PopulateManagerLists(WorksheetCellFormatData resolvedFormat, ref int listIndex)
		//{
		//    FormatInfo formatInfo = new FormatInfo();

		//    //originally overrode the Equals and == operator to use Contains() in the Info classes, 
		//    // but changed it to get rid of warnings.
		//    bool exists;
		//    FillInfo fillInfo = CreateFillInfo(resolvedFormat);
		//    if (fillInfo != null)
		//    {
		//        exists = false;
		//        for (int i = 0; i < this.Fills.Count; i++)
		//        {
		//            if (FillInfo.HasSameData(fillInfo, this.Fills[i]))
		//            {
		//                exists = true;
		//                formatInfo.FillId = i;
		//                break;
		//            }
		//        }
		//        if (!exists)
		//        {
		//            formatInfo.FillId = this.Fills.Count;
		//            this.Fills.Add(fillInfo);
		//        }
		//    }

		//    BorderInfo borderInfo = CreateBorderInfo(resolvedFormat);
		//    if (borderInfo != null)
		//    {
		//        exists = false;
		//        for (int i = 0; i < this.Borders.Count; i++)
		//        {
		//            if (BorderInfo.HasSameData(borderInfo, this.Borders[i]))
		//            {
		//                exists = true;
		//                formatInfo.BorderId = i;
		//                break;
		//            }
		//        }
		//        if (!exists)
		//        {
		//            formatInfo.BorderId = this.Borders.Count;
		//            this.Borders.Add(borderInfo);
		//        }
		//    }

		//    //fix the Indent and textRotation which would normally be done when the format was resolved.
		//    resolvedFormat.Indent = (resolvedFormat.Indent == -1) ? 0 : resolvedFormat.Indent;
		//    resolvedFormat.Rotation = (resolvedFormat.Rotation == -1) ? 0 : resolvedFormat.Rotation;

		//    // 8/27/08 CDS - Moved to the beginning of the method so we can set the BorderId and FillId properly
		//    //FormatInfo formatInfo = new FormatInfo();
		//    formatInfo.FormatOptions = resolvedFormat.FormatOptions;

		//    // MD 4/21/11 - TFS73525
		//    // The defaults for the Alignment and VerticalAlignment being checked here of Left and Center, respectively, were wrong.
		//    // The defaults are actually General and Bottom.
		//    //if ((resolvedFormat.Alignment != HorizontalCellAlignment.Left && resolvedFormat.Alignment != HorizontalCellAlignment.Default) ||
		//    //    resolvedFormat.Rotation != 0 ||
		//    //    resolvedFormat.WrapText == ExcelDefaultableBoolean.True ||
		//    //    (resolvedFormat.VerticalAlignment != VerticalCellAlignment.Center && resolvedFormat.VerticalAlignment != VerticalCellAlignment.Default) ||
		//    //    resolvedFormat.Indent != 0 ||
		//    //    resolvedFormat.ShrinkToFit == ExcelDefaultableBoolean.True)
		//    if ((resolvedFormat.Alignment != AlignmentInfo.DEFAULT_HORIZONTAL && resolvedFormat.Alignment != HorizontalCellAlignment.Default) ||
		//        resolvedFormat.Rotation != 0 ||
		//        resolvedFormat.WrapText == ExcelDefaultableBoolean.True ||
		//        (resolvedFormat.VerticalAlignment != AlignmentInfo.DEFAULT_VERTICAL && resolvedFormat.VerticalAlignment != VerticalCellAlignment.Default) ||
		//        resolvedFormat.Indent != 0 ||
		//        resolvedFormat.ShrinkToFit == ExcelDefaultableBoolean.True)
		//    {
		//        formatInfo.Alignment = new AlignmentInfo(
		//            resolvedFormat.AlignmentResolved,
		//            resolvedFormat.IndentResolved, 
		//            false, 
		//            0,
		//            0, 
		//            resolvedFormat.RotationResolved, 
		//            resolvedFormat.ShrinkToFitResolved, 
		//            resolvedFormat.VerticalAlignmentResolved,
		//            resolvedFormat.WrapTextResolved);
		//    }

		//    // 08/27/08 CDS - Set the BorderId and FillId when we search for dupes and add new ones.
		//    //formatInfo.BorderId = this.Borders.IndexOf(borderInfo);
		//    //formatInfo.CellStyleXfId = 
		//    //formatInfo.FillId = this.Fills.IndexOf(fillInfo);
		//    // MD 11/11/11 - TFS85193
		//    //formatInfo.FontId = resolvedFormat.FontInternal.Element.IndexInFontCollection;
		//    formatInfo.FontId = resolvedFormat.FontInternal.Element.GetIndexInFontCollection(FontResolverType.Normal);
		//    //formatInfo.NumFmtId =
		//    //formatInfo.PivotButton =
		//    // MD 3/5/09
		//    // Found while adding unit tests. 
		//    // False is the non-default value here; True is the default value.
		//    //if (resolvedFormat.Locked == ExcelDefaultableBoolean.True)
		//    if ( resolvedFormat.Locked == ExcelDefaultableBoolean.False )
		//        // || resolvedFormat.Hidden)
		//    {
		//        // MD 12/30/11 - 12.1 - Table Support
		//        // Moved this to ProtectionInfo.
		//        #region Moved

		//        //formatInfo.Protection = new ProtectionInfo();

		//        //// MD 3/5/09
		//        //// Found while adding unit tests. 
		//        //// False is the non-default value here; True is the default value.
		//        ////formatInfo.Protection.Locked = true;
		//        //formatInfo.Protection.Locked = false;

		//        #endregion // Moved
		//        formatInfo.Protection = ProtectionInfo.CreateProtectionInfo(resolvedFormat);
		//    }

		//    formatInfo.NumFmtId = (resolvedFormat.FormatStringIndex < 0) ? 0 : resolvedFormat.FormatStringIndex;

		//    // Initialize the parent style XF id if the format is not a style format.
		//    if (resolvedFormat.IsStyle == false)
		//        formatInfo.CellStyleXfId = 0;
		//}

		//#endregion PopulateManagerLists

		#endregion // Removed

		// MD 1/23/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//#region FillIndexedColorsList

		//private void FillIndexedColorsList()
		//{
		//    if (this.Workbook.Palette.PaletteMode == WorkbookPaletteMode.CustomPalette)
		//    {
		//        for ( int i = 0; i < 64; i++ )
		//            this.IndexedColors.Add( this.Workbook.Palette[ i ] );
		//    }
		//}

		//#endregion FillIndexedColorsList

		#endregion // Removed

		// MD 10/8/10
		// Found while fixing TFS44359
		// Added support to round-trip custom Xml parts.
		#region SaveCustomXmlParts

		private void SaveCustomXmlParts()
		{
			if (this.Workbook.CustomXmlPropertiesParts == null)
				return;

			List<Uri> customXmlPropsPartUris = new List<Uri>();
			this.ContextStack.Push(new ListContext<byte[]>(this.Workbook.CustomXmlPropertiesParts));

			foreach (byte[] data in this.Workbook.CustomXmlPropertiesParts)
			{
				Uri partUri = this.CreatePartInPackage(
					CustomXmlPropertiesPart.ContentTypeValue,
					this.GetNumberedPartName(CustomXmlPropertiesPart.BasePartName),
					null);

				customXmlPropsPartUris.Add(partUri);
			}

			this.ContextStack.Pop();

			if (this.Workbook.CustomXmlParts == null)
			{
				Utilities.DebugFail("If there are CustomXmlPropertiesParts, there should be CustomXmlParts");
				return;
			}

			Debug.Assert(
				this.Workbook.CustomXmlPropertiesParts.Count == this.Workbook.CustomXmlParts.Count, 
				"The counts are mismatched for the CustomXmlParts and CustomXmlPropertiesParts");

			this.ContextStack.Push(new ListContext<Uri>(customXmlPropsPartUris));
			this.ContextStack.Push(new ListContext<byte[]>(this.Workbook.CustomXmlParts));

			foreach (byte[] data in this.Workbook.CustomXmlParts)
			{
				this.CreatePartInPackage(
					CustomXmlPart.ContentTypeValue,
					this.GetNumberedPartName(CustomXmlPart.BasePartName),
					null);
			}

			this.ContextStack.Pop();
			this.ContextStack.Pop();
		}

		#endregion // SaveCustomXmlParts 

		// MD 4/6/12 - TFS102169
		#region SetPartData

		internal void SetPartData(string absolutePartPath, object data)
		{
			this.partData[absolutePartPath] = data;
		}

		#endregion // SetPartData

		// MD 3/29/11 - TFS63971
		#region OnFormulaAdded

		public void OnFormulaAdded(Formula formula)
		{
			for (int i = 0; i < formula.PostfixTokenList.Count; i++)
			{
				CellReferenceToken token = formula.PostfixTokenList[i] as CellReferenceToken;

				if (token != null && token.Is3DReference)
				{
					if (this.cell3DReferenceTokens == null)
						this.cell3DReferenceTokens = new List<CellReferenceToken>();

					this.cell3DReferenceTokens.Add(token);
				}
			}
		}

		#endregion // OnFormulaAdded

		// MD 3/29/11 - TFS63971
		#region OnWorkbookLoaded

		public void OnWorkbookLoaded()
		{
			if (this.cell3DReferenceTokens == null)
				return;

			// When the workbook is completely loaded, find all 3d references that have an index value as a workbook file name and 
			// replace it with the path ot the external workbook reference.
			for (int i = 0; i < this.cell3DReferenceTokens.Count; i++)
				this.cell3DReferenceTokens[i].UpdateIndexedWorkbookReference(this);
		}

		#endregion // OnWorkbookLoaded

		// MD 3/29/11 - TFS63971
		#region UpdateIndexedWorkbookReference

		public void UpdateIndexedWorkbookReference(ref WorksheetReference worksheet)
		{
			if (worksheet.IsConnected || 
				worksheet.WorkbookReference == null || 
				String.IsNullOrEmpty(worksheet.WorkbookReference.FileName))
				return;

			int index;
			if (int.TryParse(worksheet.WorkbookReference.FileName, out index) == false)
				return;

			if (index < 0 || this.WorkbookReferences.Count <= index)
				return;

			WorkbookReferenceBase workbookReference = this.WorkbookReferences[index];
			worksheet = worksheet.Connect(workbookReference);
		}

		#endregion // UpdateIndexedWorkbookReference

        #endregion Methods

        #region Properties

        #region ActivePart

        public IPackagePart ActivePart
		{
			get { return this.activePart; }
		}

		#endregion ActivePart 

		#region ImagesSavedInPackage







		public Dictionary<Image, Uri> ImagesSavedInPackage
		{
			get
			{
				if ( this.imagesSavedInPackage == null )
					this.imagesSavedInPackage = new Dictionary<Image, Uri>();

				return this.imagesSavedInPackage;
			}
		} 

		#endregion ImagesSavedInPackage

        //  BF 7/8/08   OpenPackagingConventions
        #region PackageConformanceManager
        private PackageConformanceManager PackageConformanceManager
        {
            get
            {
                if (this.packageConformanceManager == null)
                    this.packageConformanceManager = new PackageConformanceManager(this.verifyExcel2007Xml);

                return this.packageConformanceManager;
            }
        }
        #endregion PackageConformanceManager

        #region Borders







        public List<BorderInfo> Borders
        {
            get
            {
                if (this.borders == null)
                    this.borders = new List<BorderInfo>();
                return this.borders;
            }
        }

        #endregion Borders

		// MD 2/17/12 - 12.1 - Table Support
		#region DxfInfos

		public List<DxfInfo> DxfInfos
		{
			get
			{
				if (this.dxfInfos == null)
					this.dxfInfos = new List<DxfInfo>();

				return this.dxfInfos;
			}
		}

		#endregion DxfInfos

        // MBS 8/18/08 
        #region ExternalReferences







        public Dictionary<WorkbookReferenceBase, int> ExternalReferences
        {
            get 
            {
                if(this.externalReferences == null)
                    this.externalReferences = new Dictionary<WorkbookReferenceBase, int>();

                return this.externalReferences; 
            }
        }
        #endregion //ExternalReferences

        #region Fills







        public List<FillInfo> Fills
        {
            get
            {
                if (this.fills == null)
                    this.fills = new List<FillInfo>();
                return this.fills;
            }
        }

        #endregion Fills

		// MD 1/23/09 - TFS12619
		// Uncommented this so it could be used again.
		#region NumberFormats






		public List<NumberFormatInfo> NumberFormats
		{
			get
			{
				if ( this.numberFormats == null )
					this.numberFormats = new List<NumberFormatInfo>();

				return this.numberFormats;
			}
		}

		#endregion NumberFormats

        #region CellStyles






        public List<StyleInfo> CellStyles
        {
            get
            {
                if (this.cellStyles == null)
                    this.cellStyles = new List<StyleInfo>();
                return this.cellStyles;
            }
        }

        #endregion CellStyles

        #region CellStyleXfs






        public List<FormatInfo> CellStyleXfs
        {
            get
            {
                if (this.cellStyleXfs == null)
                    this.cellStyleXfs = new List<FormatInfo>();
                return this.cellStyleXfs;
            }
        }

        #endregion CellStyleXfs

        #region CellXfs






        public List<FormatInfo> CellXfs
        {
            get
            {
                if (this.cellXfs == null)
                    this.cellXfs = new List<FormatInfo>();
                return this.cellXfs;
            }
        }

        #endregion CellStyleXfs

		// MD 12/21/11 - 12.1 - Table Support
		// Now the Dxfs collection is stored on the base serialization manager.
		#region Removed

		//        #region Dxfs

		//#if DEBUG
		//        /// <summary>
		//        /// Gets a list of incremental or differential (dxf) formats specified in the zip package
		//        /// </summary>
		//#endif
		//        public List<DxfInfo> Dxfs
		//        {
		//            get
		//            {
		//                if (this.dxfs == null)
		//                    this.dxfs = new List<DxfInfo>();

		//                return this.dxfs;
		//            }
		//        }

		//        #endregion Dxfs

		#endregion // Removed

		// MD 11/29/11 - TFS96205
		// Moved this to the WorkbookSerializationManager because we can use theme colors in XLS files as well.
		#region Removed

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

		#endregion  // Removed

		// MD 1/23/12 - 12.1 - Cell Format Updated.
		// This is no longer needed.
		#region Removed

		//        #region IndexedColors

		//#if DEBUG
		//        /// <summary>
		//        /// Gets a list of indexed color loaded from the zip archive
		//        /// This list should be empty in the default color palette is being used.
		//        /// </summary>
		//#endif
		//        public List<Color> IndexedColors
		//        {
		//            get
		//            {
		//                if (this.indexedColors == null)
		//                    this.indexedColors = new List<Color>();
		//                return this.indexedColors;
		//            }
		//        }

		//        #endregion IndexedColors

		#endregion // Removed

		// MD 12/21/11 - 12.1 - Table Support
		#region IsLoadingPresetTableStyles

		public bool IsLoadingPresetTableStyles
		{
			get { return this.isLoadingPresetTableStyles; }
			set { this.isLoadingPresetTableStyles = value; }
		}

		#endregion // IsLoadingPresetTableStyles

		// MD 2/9/12 - TFS89375
		#region MajorFonts

		public FontCollection MajorFonts
		{
			get
			{
				if (this.majorFonts == null)
					this.majorFonts = new FontCollection();

				return this.majorFonts;
			}
		}

		#endregion // MajorFonts

		// MD 2/9/12 - TFS89375
		#region MinorFonts

		public FontCollection MinorFonts
		{
			get
			{
				if (this.minorFonts == null)
					this.minorFonts = new FontCollection();

				return this.minorFonts;
			}
		}

		#endregion // MinorFonts

        //  BF 8/20/08  Excel2007 Format
        #region SerializedShapes


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal Dictionary<string, WorksheetShapesHolder> SerializedShapes
        {
            get
            {
                if ( this.serializedShapes == null )
                    this.serializedShapes = new Dictionary<string, WorksheetShapesHolder>();

                return this.serializedShapes;
            }
        }
        #endregion SerializedShapes

        #region HasSerializedShapes






        internal bool HasSerializedShapes
        {
            get { return this.serializedShapes != null && this.serializedShapes.Count > 0; }
        }
        #endregion HasSerializedShapes

		// MD 3/30/10 - TFS30253
		#region SharedFormulas

		public Dictionary<int, Formula> SharedFormulas
		{
			get
			{
				if (this.sharedFormulas == null)
					this.sharedFormulas = new Dictionary<int, Formula>();

				return this.sharedFormulas;
			}
		}

		#endregion // SharedFormulas

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region StylesByCellStyleXfId

		public Dictionary<int, WorkbookStyle> StylesByCellStyleXfId
		{
			get
			{
				if (this.stylesByCellStyleXfId == null)
					this.stylesByCellStyleXfId = new Dictionary<int, WorkbookStyle>();

				return this.stylesByCellStyleXfId;
			}
		}

		#endregion // StylesByCellStyleXfId

		// MD 1/19/11 - TFS62268
		#region FontColorInfos







		public List<ColorInfo> FontColorInfos
		{
			get
			{
				if (this.fontColorInfos == null)
					this.fontColorInfos = new List<ColorInfo>();

				return this.fontColorInfos;
			}
		} 

		#endregion // FontColorInfos

		// MD 4/6/12 - TFS102169
		#region NamedReferenceInfos

		public List<NamedReferenceInfo> NamedReferenceInfos
		{
			get { return this.namedReferenceInfos; }
		}

		#endregion // NamedReferenceInfos

		// MD 2/23/12 - TFS101504
		#region OrderedExternalReferences

		public List<WorkbookReferenceBase> OrderedExternalReferences
		{
			get
			{
				if (this.orderedExternalReferences == null)
				{
					this.orderedExternalReferences = new List<WorkbookReferenceBase>();
					this.orderedExternalReferences.Add(this.Workbook.CurrentWorkbookReference);
				}

				return this.orderedExternalReferences;
			}
		}

		#endregion // OrderedExternalReferences

        #endregion Properties


		// MD 4/6/12 - TFS102169
		#region NamedReferenceInfo struct

		public struct NamedReferenceInfo
		{
			public bool Hidden;
			public NamedReference Reference;
		}

		#endregion // NamedReferenceInfo struct

        #region PartRelationshipCounter class

        //  BF 9/30/10  NA 2011.1 - Infragistics.Word
        //  Made this internal
        internal class PartRelationshipCounter
		{
			private IPackagePart part;
			private int relationshipCount;

			public PartRelationshipCounter( IPackagePart part )
			{
				this.part = part;
			}

			public void IncrementRelationshipCount()
			{
				this.relationshipCount++;
			}

			public IPackagePart Part
			{
				get { return this.part; }
			}

			public int RelationshipCount
			{
				get { return this.relationshipCount; }
			}
		} 

		#endregion PartRelationshipCounter class
	}

    //  BF 11/23/10 IGWordStreamer
    internal interface IOfficeDocumentExportManager
    {
    }

	// MD 2/9/12 - TFS89375
	#region FontCollection class

	internal class FontCollection
	{
		private string defaultFontName;
		private Dictionary<string, string> fontScripts;

		public FontCollection()
		{
			this.fontScripts = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		}

		public string DefaultFontName
		{
			get { return this.defaultFontName; }
			set { this.defaultFontName = value; }
		}

		public void AddFontScript(string script, string typeface)
		{
			Debug.Assert(this.fontScripts.ContainsKey(script) == false, "We shouldn't have this font script already.");
			this.fontScripts[script] = typeface;
		}

		public string GetCurrentFont()
		{

			// MD 7/3/12 - TFS115690
			// Try to use the appropriate script font first. Since we can't find a good way to determine the script type
			// in .NET, we will hard-code them for now as they are submitted.
			string scriptFont = null;
			if (CultureInfo.CurrentCulture.LCID == 0x411)
				this.fontScripts.TryGetValue("jpan", out scriptFont);

			if (scriptFont != null)
				return scriptFont;


			return this.defaultFontName;
		}
	}

	#endregion // FontCollection class
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