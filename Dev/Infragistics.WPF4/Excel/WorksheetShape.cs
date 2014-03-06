using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing;
using Infragistics.Documents.Excel.PredefinedShapes;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	

	/// <summary>
	/// Abstract base class for all shapes (atomic or compound) in a worksheet.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Atomic shapes are singular shape entities, such as an image, a polygon, or text.
	/// Compound shapes are groupings of shapes, and are represented by <see cref="WorksheetShapeGroup"/>
	/// instances.
	/// </p>
	/// <p class="body">
	/// Currently, not all shape information is customizable (such as shape rotation).  However,
	/// for round-tripping purposes, when a shape is loaded from an Excel file, this 
	/// information in maintained with the shape.  See <see cref="ClearUnknownData"/> for more
	/// information about unsupported data.
	/// </p>
	/// </remarks>



	public

		 abstract class WorksheetShape
	{
		#region Constants

		// MD 7/18/11 - Shape support
		// MD 8/23/11 - TFS84306
		// Renamed this constant to ExtendedProperties_Hidden to be consistent with the other property related constants.
		//private const uint HiddenBitOnExtendedProperties = 0x02;
		internal const uint ExtendedProperties_Default = 0x00020000u;
		internal const uint ExtendedProperties_Hidden = 0x00000002u;
		internal const uint FillStyleNoFillHitTest_Default = 0x00100000u;
		internal const uint FillStyleNoFillHitTest_HasFill = 0x00000010u;
		internal const uint LineStyleNoLineDrawDash_Default = 0x00080000u;
		internal const uint LineStyleNoLineDrawDash_ConnectorFlags = 0x00100010u; 
		internal const uint LineStyleNoLineDrawDash_HasOutline = 0x00000008u;

		#endregion  // Constants

		#region Static Variables

		// MD 7/15/11 - Shape support
		private static Dictionary<ST_ShapeType, ShapeType> excel2003ShapeTypeByExcel2007ShapeType;
		private static Dictionary<ShapeType, ST_ShapeType> excel2007ShapeTypeByExcel2003ShapeType;

		#endregion // Static Variables

		#region Member Variables

		private WorksheetShapeCollection collection;

		// MD 3/27/12 - 12.1 - Table Support
		//private WorksheetCell bottomRightCornerCell;
		private Worksheet bottomRightCornerCellWorksheet;
		private WorksheetCellAddress bottomRightCornerCell = WorksheetCellAddress.InvalidReference;

		private PointF bottomRightCornerPosition;

		// MD 3/3/10 - TFS26342
		private Rectangle boundsBeforeWorksheetElementResizeInTwips;

		private ShapePositioningMode positioningMode = ShapePositioningMode.MoveWithCells;

		// MD 3/27/12 - 12.1 - Table Support
		//private WorksheetCell topLeftCornerCell;
		private Worksheet topLeftCornerCellWorksheet;
		private WorksheetCellAddress topLeftCornerCell = WorksheetCellAddress.InvalidReference;

		private PointF topLeftCornerPosition;

		// MD 9/2/08 - Cell Comments
		private bool visible = true;

		// Unknown data
		private List<PropertyTableBase.PropertyValue> drawingProperties1;
		private List<PropertyTableBase.PropertyValue> drawingProperties2;
		private List<PropertyTableBase.PropertyValue> drawingProperties3;

		// MD 10/30/11 - TFS90733
		// Instead of manually populating the ObjRecords collection, we will now create one Obj instance, which will 
		// manage its stored records.
		//private List<OBJRecordBase> objRecords;
		private Obj obj;

		// MD 9/2/08 - Cell Comments
		// This data is now "known" and is stored in the textFormatted.
		//private byte[] txoData;

		// MD 9/2/08 - Cell Comments
		// MD 7/15/11 - Shape support
		// This will now be public, so it should be a float so degrees can be specified with fractional portions.
		//private ushort rotation;
		private float rotation;

		// MD 7/18/11 - Shape support
		// This is only the default for comments, so we will initialize this value only for comment.
		//private ushort txoOptionFlags = 530;
		private ushort txoOptionFlags = 0x224;

		// MD 7/24/12 - TFS115693
		private ushort txoRotation = 0;

		// MD 6/14/07 - BR23880
		// We need to store the callout on the shape
		private CalloutRule calloutRule;

		#region Serialization Cache

		// These are only valid when the shpae's worksheet is about to be saved
		private uint shapeId;

		#endregion Serialization Cache

        //  BF 8/20/08
        private WorksheetShapeSerializationManager excel2007ShapeSerializationManager = null;

		// MD 7/14/11 - Shape support
		private bool flippedHorizontally;
		private bool flippedVertically;

		// MD 8/23/11 - TFS84306
		private ShapeFill fill;
		private ShapeOutline outline;

		// MD 10/10/11 - TFS81451
		private string shapeProperties2007Element;

		#endregion Member Variables

		#region Constructor

		// MD 7/15/11 - Shape support
		static WorksheetShape()
		{
			WorksheetShape.excel2003ShapeTypeByExcel2007ShapeType = new Dictionary<ST_ShapeType, ShapeType>();
			WorksheetShape.excel2007ShapeTypeByExcel2003ShapeType = new Dictionary<ShapeType, ST_ShapeType>();

			WorksheetShape.AddMapping(ShapeType.AccentBorderCallout1, ST_ShapeType.accentBorderCallout1);
			WorksheetShape.AddMapping(ShapeType.AccentBorderCallout2, ST_ShapeType.accentBorderCallout2);
			WorksheetShape.AddMapping(ShapeType.AccentBorderCallout3, ST_ShapeType.accentBorderCallout3);
			WorksheetShape.AddMapping(ShapeType.AccentCallout1, ST_ShapeType.accentCallout1);
			WorksheetShape.AddMapping(ShapeType.AccentCallout2, ST_ShapeType.accentCallout2);
			WorksheetShape.AddMapping(ShapeType.AccentCallout3, ST_ShapeType.accentCallout3);
			WorksheetShape.AddMapping(ShapeType.ActionButtonBackPrevious, ST_ShapeType.actionButtonBackPrevious);
			WorksheetShape.AddMapping(ShapeType.ActionButtonBeginning, ST_ShapeType.actionButtonBeginning);
			WorksheetShape.AddMapping(ShapeType.ActionButtonBlank, ST_ShapeType.actionButtonBlank);
			WorksheetShape.AddMapping(ShapeType.ActionButtonDocument, ST_ShapeType.actionButtonDocument);
			WorksheetShape.AddMapping(ShapeType.ActionButtonEnd, ST_ShapeType.actionButtonEnd);
			WorksheetShape.AddMapping(ShapeType.ActionButtonForwardNext, ST_ShapeType.actionButtonForwardNext);
			WorksheetShape.AddMapping(ShapeType.ActionButtonHelp, ST_ShapeType.actionButtonHelp);
			WorksheetShape.AddMapping(ShapeType.ActionButtonHome, ST_ShapeType.actionButtonHome);
			WorksheetShape.AddMapping(ShapeType.ActionButtonInformation, ST_ShapeType.actionButtonInformation);
			WorksheetShape.AddMapping(ShapeType.ActionButtonMovie, ST_ShapeType.actionButtonMovie);
			WorksheetShape.AddMapping(ShapeType.ActionButtonReturn, ST_ShapeType.actionButtonReturn);
			WorksheetShape.AddMapping(ShapeType.ActionButtonSound, ST_ShapeType.actionButtonSound);
			WorksheetShape.AddMapping(ShapeType.Arrow, ST_ShapeType.rightArrow);
			WorksheetShape.AddMapping(ShapeType.BentConnector2, ST_ShapeType.bentConnector2);
			WorksheetShape.AddMapping(ShapeType.BentConnector3, ST_ShapeType.bentConnector3);
			WorksheetShape.AddMapping(ShapeType.BentConnector4, ST_ShapeType.bentConnector4);
			WorksheetShape.AddMapping(ShapeType.BentConnector5, ST_ShapeType.bentConnector5);
			WorksheetShape.AddMapping(ShapeType.Bevel, ST_ShapeType.bevel);
			WorksheetShape.AddMapping(ShapeType.BorderCallout1, ST_ShapeType.borderCallout1);
			WorksheetShape.AddMapping(ShapeType.BorderCallout2, ST_ShapeType.borderCallout2);
			WorksheetShape.AddMapping(ShapeType.BorderCallout3, ST_ShapeType.borderCallout3);
			WorksheetShape.AddMapping(ShapeType.BracePair, ST_ShapeType.bracePair);
			WorksheetShape.AddMapping(ShapeType.BracketPair, ST_ShapeType.bracketPair);
			WorksheetShape.AddMapping(ShapeType.Callout1, ST_ShapeType.callout1);
			WorksheetShape.AddMapping(ShapeType.Callout2, ST_ShapeType.callout2);
			WorksheetShape.AddMapping(ShapeType.Callout3, ST_ShapeType.callout3);
			WorksheetShape.AddMapping(ShapeType.Can, ST_ShapeType.can);
			WorksheetShape.AddMapping(ShapeType.Chevron, ST_ShapeType.chevron);
			WorksheetShape.AddMapping(ShapeType.CloudCallout, ST_ShapeType.cloudCallout);
			WorksheetShape.AddMapping(ShapeType.Cube, ST_ShapeType.cube);
			WorksheetShape.AddMapping(ShapeType.CurvedConnector2, ST_ShapeType.curvedConnector2);
			WorksheetShape.AddMapping(ShapeType.CurvedConnector3, ST_ShapeType.curvedConnector3);
			WorksheetShape.AddMapping(ShapeType.CurvedConnector4, ST_ShapeType.curvedConnector4);
			WorksheetShape.AddMapping(ShapeType.CurvedConnector5, ST_ShapeType.curvedConnector5);
			WorksheetShape.AddMapping(ShapeType.CurvedDownArrow, ST_ShapeType.curvedDownArrow);
			WorksheetShape.AddMapping(ShapeType.CurvedLeftArrow, ST_ShapeType.curvedLeftArrow);
			WorksheetShape.AddMapping(ShapeType.CurvedRightArrow, ST_ShapeType.curvedRightArrow);
			WorksheetShape.AddMapping(ShapeType.CurvedUpArrow, ST_ShapeType.curvedUpArrow);
			WorksheetShape.AddMapping(ShapeType.Diamond, ST_ShapeType.diamond);
			WorksheetShape.AddMapping(ShapeType.DoubleWave, ST_ShapeType.doubleWave);
			WorksheetShape.AddMapping(ShapeType.DownArrow, ST_ShapeType.downArrow);
			WorksheetShape.AddMapping(ShapeType.DownArrowCallout, ST_ShapeType.downArrowCallout);
			WorksheetShape.AddMapping(ShapeType.Ellipse, ST_ShapeType.ellipse);
			WorksheetShape.AddMapping(ShapeType.EllipseRibbon, ST_ShapeType.ellipseRibbon);
			WorksheetShape.AddMapping(ShapeType.EllipseRibbon2, ST_ShapeType.ellipseRibbon2);
			WorksheetShape.AddMapping(ShapeType.FlowChartAlternateProcess, ST_ShapeType.flowChartAlternateProcess);
			WorksheetShape.AddMapping(ShapeType.FlowChartCollate, ST_ShapeType.flowChartCollate);
			WorksheetShape.AddMapping(ShapeType.FlowChartConnector, ST_ShapeType.flowChartConnector);
			WorksheetShape.AddMapping(ShapeType.FlowChartDecision, ST_ShapeType.flowChartDecision);
			WorksheetShape.AddMapping(ShapeType.FlowChartDelay, ST_ShapeType.flowChartDelay);
			WorksheetShape.AddMapping(ShapeType.FlowChartDisplay, ST_ShapeType.flowChartDisplay);
			WorksheetShape.AddMapping(ShapeType.FlowChartDocument, ST_ShapeType.flowChartDocument);
			WorksheetShape.AddMapping(ShapeType.FlowChartExtract, ST_ShapeType.flowChartExtract);
			WorksheetShape.AddMapping(ShapeType.FlowChartInputOutput, ST_ShapeType.flowChartInputOutput);
			WorksheetShape.AddMapping(ShapeType.FlowChartInternalStorage, ST_ShapeType.flowChartInternalStorage);
			WorksheetShape.AddMapping(ShapeType.FlowChartMagneticDisk, ST_ShapeType.flowChartMagneticDisk);
			WorksheetShape.AddMapping(ShapeType.FlowChartMagneticDrum, ST_ShapeType.flowChartMagneticDrum);
			WorksheetShape.AddMapping(ShapeType.FlowChartMagneticTape, ST_ShapeType.flowChartMagneticTape);
			WorksheetShape.AddMapping(ShapeType.FlowChartManualInput, ST_ShapeType.flowChartManualInput);
			WorksheetShape.AddMapping(ShapeType.FlowChartManualOperation, ST_ShapeType.flowChartManualOperation);
			WorksheetShape.AddMapping(ShapeType.FlowChartMerge, ST_ShapeType.flowChartMerge);
			WorksheetShape.AddMapping(ShapeType.FlowChartMultidocument, ST_ShapeType.flowChartMultidocument);
			WorksheetShape.AddMapping(ShapeType.FlowChartOfflineStorage, ST_ShapeType.flowChartOfflineStorage);
			WorksheetShape.AddMapping(ShapeType.FlowChartOffpageConnector, ST_ShapeType.flowChartOffpageConnector);
			WorksheetShape.AddMapping(ShapeType.FlowChartOnlineStorage, ST_ShapeType.flowChartOnlineStorage);
			WorksheetShape.AddMapping(ShapeType.FlowChartOr, ST_ShapeType.flowChartOr);
			WorksheetShape.AddMapping(ShapeType.FlowChartPredefinedProcess, ST_ShapeType.flowChartPredefinedProcess);
			WorksheetShape.AddMapping(ShapeType.FlowChartPreparation, ST_ShapeType.flowChartPreparation);
			WorksheetShape.AddMapping(ShapeType.FlowChartProcess, ST_ShapeType.flowChartProcess);
			WorksheetShape.AddMapping(ShapeType.FlowChartPunchedCard, ST_ShapeType.flowChartPunchedCard);
			WorksheetShape.AddMapping(ShapeType.FlowChartPunchedTape, ST_ShapeType.flowChartPunchedTape);
			WorksheetShape.AddMapping(ShapeType.FlowChartSort, ST_ShapeType.flowChartSort);
			WorksheetShape.AddMapping(ShapeType.FlowChartSummingJunction, ST_ShapeType.flowChartSummingJunction);
			WorksheetShape.AddMapping(ShapeType.FlowChartTerminator, ST_ShapeType.flowChartTerminator);
			WorksheetShape.AddMapping(ShapeType.FoldedCorner, ST_ShapeType.foldedCorner);
			WorksheetShape.AddMapping(ShapeType.Heart, ST_ShapeType.heart);
			WorksheetShape.AddMapping(ShapeType.Hexagon, ST_ShapeType.hexagon);
			WorksheetShape.AddMapping(ShapeType.HomePlate, ST_ShapeType.homePlate);
			WorksheetShape.AddMapping(ShapeType.HorizontalScroll, ST_ShapeType.horizontalScroll);
			WorksheetShape.AddMapping(ShapeType.IrregularSeal1, ST_ShapeType.irregularSeal1);
			WorksheetShape.AddMapping(ShapeType.IrregularSeal2, ST_ShapeType.irregularSeal2);
			WorksheetShape.AddMapping(ShapeType.IsocelesTriangle, ST_ShapeType.triangle);
			WorksheetShape.AddMapping(ShapeType.LeftArrow, ST_ShapeType.leftArrow);
			WorksheetShape.AddMapping(ShapeType.LeftArrowCallout, ST_ShapeType.leftArrowCallout);
			WorksheetShape.AddMapping(ShapeType.LeftBrace, ST_ShapeType.leftBrace);
			WorksheetShape.AddMapping(ShapeType.LeftBracket, ST_ShapeType.leftBracket);
			WorksheetShape.AddMapping(ShapeType.LeftRightArrow, ST_ShapeType.leftRightArrow);
			WorksheetShape.AddMapping(ShapeType.LeftRightArrowCallout, ST_ShapeType.leftRightArrowCallout);
			WorksheetShape.AddMapping(ShapeType.LightningBolt, ST_ShapeType.lightningBolt);
			WorksheetShape.AddMapping(ShapeType.Line, ST_ShapeType.line);
			WorksheetShape.AddMapping(ShapeType.Moon, ST_ShapeType.moon);
			WorksheetShape.AddMapping(ShapeType.NotchedRightArrow, ST_ShapeType.notchedRightArrow);
			WorksheetShape.AddMapping(ShapeType.Octagon, ST_ShapeType.octagon);
			WorksheetShape.AddMapping(ShapeType.Parallelogram, ST_ShapeType.parallelogram);
			WorksheetShape.AddMapping(ShapeType.Pentagon, ST_ShapeType.pentagon);
			WorksheetShape.AddMapping(ShapeType.Plaque, ST_ShapeType.plaque);
			WorksheetShape.AddMapping(ShapeType.Plus, ST_ShapeType.plus);
			WorksheetShape.AddMapping(ShapeType.Rectangle, ST_ShapeType.rect);
			WorksheetShape.AddMapping(ShapeType.Ribbon, ST_ShapeType.ribbon);
			WorksheetShape.AddMapping(ShapeType.Ribbon2, ST_ShapeType.ribbon2);
			WorksheetShape.AddMapping(ShapeType.RightArrowCallout, ST_ShapeType.rightArrowCallout);
			WorksheetShape.AddMapping(ShapeType.RightBrace, ST_ShapeType.rightBrace);
			WorksheetShape.AddMapping(ShapeType.RightBracket, ST_ShapeType.rightBracket);
			WorksheetShape.AddMapping(ShapeType.RightTriangle, ST_ShapeType.rtTriangle);
			WorksheetShape.AddMapping(ShapeType.RoundRectangle, ST_ShapeType.roundRect);
			WorksheetShape.AddMapping(ShapeType.Seal16, ST_ShapeType.star16);
			WorksheetShape.AddMapping(ShapeType.Seal24, ST_ShapeType.star24);
			WorksheetShape.AddMapping(ShapeType.Seal32, ST_ShapeType.star32);
			WorksheetShape.AddMapping(ShapeType.Seal4, ST_ShapeType.star4);
			WorksheetShape.AddMapping(ShapeType.Seal8, ST_ShapeType.star8);
			WorksheetShape.AddMapping(ShapeType.SmileyFace, ST_ShapeType.smileyFace);
			WorksheetShape.AddMapping(ShapeType.StraightConnector1, ST_ShapeType.straightConnector1);
			WorksheetShape.AddMapping(ShapeType.Sun, ST_ShapeType.sun);
			WorksheetShape.AddMapping(ShapeType.UpArrow, ST_ShapeType.upArrow);
			WorksheetShape.AddMapping(ShapeType.UpArrowCallout, ST_ShapeType.upArrowCallout);
			WorksheetShape.AddMapping(ShapeType.UpDownArrow, ST_ShapeType.upDownArrow);
			WorksheetShape.AddMapping(ShapeType.UpDownArrowCallout, ST_ShapeType.upDownArrowCallout);
			WorksheetShape.AddMapping(ShapeType.VerticalScroll, ST_ShapeType.verticalScroll);
			WorksheetShape.AddMapping(ShapeType.Wave, ST_ShapeType.wave);
			WorksheetShape.AddMapping(ShapeType.WedgeEllipseCallout, ST_ShapeType.wedgeEllipseCallout);
			WorksheetShape.AddMapping(ShapeType.WedgeRectCallout, ST_ShapeType.wedgeRectCallout);
			WorksheetShape.AddMapping(ShapeType.WedgeRRectCallout, ST_ShapeType.wedgeRoundRectCallout);
		}

		// MD 9/14/11 - TFS86093
		//internal WorksheetShape() { }
		internal WorksheetShape()
			: this(true) { }

		internal WorksheetShape(bool initializeDefaults)
		{
			if (initializeDefaults)
				this.InitializeDefaults();
		}

		// MD 5/4/09 - TFS17197
		// Added a copy constructor 
		internal WorksheetShape( WorksheetShape shape )
		{
			// MD 7/14/11 - Shape support
			// Moved this code to the InitializeFrom method.
			//this.bottomRightCornerCell = shape.bottomRightCornerCell;
			//this.bottomRightCornerPosition = shape.bottomRightCornerPosition;
			//this.positioningMode = shape.positioningMode;
			//this.topLeftCornerCell = shape.topLeftCornerCell;
			//this.topLeftCornerPosition = shape.topLeftCornerPosition;
			//this.visible = shape.visible;
			//this.drawingProperties1 = shape.drawingProperties1;
			//this.drawingProperties2 = shape.drawingProperties2;
			//this.drawingProperties3 = shape.drawingProperties3;
			//this.objRecords = shape.objRecords;
			//this.rotation = shape.rotation;
			//this.txoOptionFlags = shape.txoOptionFlags;
			//this.calloutRule = shape.calloutRule;
			//this.shapeId = shape.shapeId;
			//this.excel2007ShapeSerializationManager = shape.excel2007ShapeSerializationManager;
			this.InitializeFrom(shape);
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#region ClearUnknownData

		/// <summary>
		/// Clears the cached unknown shape data which was read in from a parsed excel file.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method will only be needed until all shape information is supported and customizable.
		/// After all shape data can be controlled, this method will become obsolete. Currently, all
		/// unsupported data will be stored with the shape for round-tripping purposes.  For example, 
		/// if an Excel file with complex and unsupported shapes is loaded into a 
		/// <see cref="Workbook"/> instance, some cell values are changed, and it is saved to
		/// the same file, the complex shapes will still exist in the workbook.  However, if a
		/// loaded shape needs to be modified before it is saved back, this method
		/// allows for that unsupported data to be removed while all supported data is maintained.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> This method only clears unsupported data.  In future versions of the product, 
		/// as more shape data is supported, this method will have different effects on the shape, 
		/// until eventually all data is supported and this method will have no effect on the shape.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// This method is called on an <see cref="UnknownShape"/> instance.
		/// </exception>
		public virtual void ClearUnknownData()
		{
			// MD 6/14/07 - BR23880
			// Clear the callout rule, because it is also unknown data
			this.calloutRule = null;

			this.drawingProperties1 = null;
			this.drawingProperties2 = null;
			this.drawingProperties3 = null;

			// MD 9/2/08 - Cell Comments
			// This round-trip data doesn't need to be stored anymore
			//this.txoData = null;

			// MD 10/30/11 - TFS90733
			//this.objRecords = null;
			this.obj = null;
		}

		#endregion ClearUnknownData

		// MD 7/14/11 - Shape support
		#region CreatePredefinedShape

		/// <summary>
		/// Creates a shape which is predefined in Microsoft Excel.
		/// </summary>
		/// <param name="shapeType">The type of shape to create.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="shapeType"/> is not defined in the <see cref="PredefinedShapeType"/> enumeration.
		/// </exception>
		/// <returns>A <see cref="WorksheetShape"/>-derived instance representing the predefined shape.</returns>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public static WorksheetShape CreatePredefinedShape(PredefinedShapeType shapeType)
		{
			// MD 8/23/11 - TFS84306
			// Moved all code to the new overload.
			return WorksheetShape.CreatePredefinedShape(shapeType, true);
		}

		// MD 8/23/11 - TFS84306
		// Added a new overload to take a value indicating whether we should initialize default property values.
		internal static WorksheetShape CreatePredefinedShape(PredefinedShapeType shapeType, bool initializeDefaults)
		{
			if (Enum.IsDefined(typeof(PredefinedShapeType), shapeType) == false)
				throw new InvalidEnumArgumentException("shapeType", (int)shapeType, typeof(PredefinedShapeType));

			switch (shapeType)
			{
				case PredefinedShapeType.Diamond:				return new DiamondShape(initializeDefaults);
				case PredefinedShapeType.Heart:					return new HeartShape(initializeDefaults);
				case PredefinedShapeType.IrregularSeal1:		return new IrregularSeal1Shape(initializeDefaults);
				case PredefinedShapeType.IrregularSeal2:		return new IrregularSeal2Shape(initializeDefaults);
				case PredefinedShapeType.LightningBolt:			return new LightningBoltShape(initializeDefaults);
 				case PredefinedShapeType.Line:					return new LineShape(initializeDefaults);
				case PredefinedShapeType.Ellipse:				return new EllipseShape(initializeDefaults);
				case PredefinedShapeType.Pentagon:				return new PentagonShape(initializeDefaults);
				case PredefinedShapeType.Rectangle:				return new RectangleShape(initializeDefaults);
				case PredefinedShapeType.RightTriangle:			return new RightTriangleShape(initializeDefaults);
				case PredefinedShapeType.StraightConnector1:	return new StraightConnector1Shape(initializeDefaults);

				default:
					Utilities.DebugFail("Unknown shape type: " + shapeType);
					return new RectangleShape(initializeDefaults);
			}
		}

		#endregion // CreatePredefinedShape

		#region GetBoundsInTwips

		/// <summary>
		/// Gets the bounds of the shape in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before or within the shape are resized, these bounds will no longer reflect the 
		/// position of the shape.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="TopLeftCornerCell"/> or <see cref="BottomRightCornerCell"/> are null, in which case the shape has no bounds.
		/// </exception>
		/// <returns>The bounds of the shape on its worksheet.</returns>
		/// <seealso cref="SetBoundsInTwips(Worksheet,Rectangle)"/>
		public virtual Rectangle GetBoundsInTwips()
		{
			// MD 3/24/10 - TFS28374
			// Moved all code to a new overload.
			return this.GetBoundsInTwips(PositioningOptions.None);
		}

		// MD 3/24/10 - TFS28374
		// Added a new overload.
		/// <summary>
		/// Gets the bounds of the shape in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before or within the shape are resized, these bounds will no longer reflect the 
		/// position of the shape.
		/// </p>
		/// </remarks>
		/// <param name="options">The options to use when getting the bounds of the shape.</param>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="TopLeftCornerCell"/> or <see cref="BottomRightCornerCell"/> are null, in which case the shape has no bounds.
		/// </exception>
		/// <returns>The bounds of the shape on its worksheet.</returns>
		/// <seealso cref="SetBoundsInTwips(Worksheet,Rectangle)"/>
		public virtual Rectangle GetBoundsInTwips(PositioningOptions options)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//if ( this.TopLeftCornerCell == null || this.BottomRightCornerCell == null )
			//    throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_GetBoundsBeforeAnchorsSet" ) );
			//return WorksheetShape.GetBoundsInTwips(
			//    this.TopLeftCornerCell,
			//    this.TopLeftCornerPosition,
			//    this.BottomRightCornerCell,
			//    this.BottomRightCornerPosition,
			//    options	// MD 3/24/10 - TFS28374
			//    );
			if (this.topLeftCornerCellWorksheet == null || this.bottomRightCornerCellWorksheet == null)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_GetBoundsBeforeAnchorsSet"));

			return WorksheetShape.GetBoundsInTwips(
				topLeftCornerCellWorksheet,
				this.TopLeftCornerCellInternal,
				this.TopLeftCornerPosition,
				this.BottomRightCornerCellInternal,
				this.BottomRightCornerPosition,
				options	// MD 3/24/10 - TFS28374
				);
		}

		#endregion GetBoundsInTwips

		#region SetBoundsInTwips

		/// <summary>
		/// Sets the bounds of the shape in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The shape will only be positioned at the specified bounds while the worksheet remains in the current configuration.
		/// Depending on the <see cref="PositioningMode"/> of the shape, it may change bounds if any rows or columns before or within the shape are resized.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <param name="worksheet">The worksheet on which the shape should be placed.</param>
		/// <param name="bounds">The new bounds where the shape should be placed.</param> 
		public void SetBoundsInTwips( Worksheet worksheet, Rectangle bounds )
		{
			// MD 3/24/10 - TFS28374
			// Call off to the new overload that takes the options.
			//// MD 3/5/10 - TFS26342
			//// Moved code to new overload.
			//this.SetBoundsInTwips(worksheet, bounds, true);
			this.SetBoundsInTwips(worksheet, bounds, PositioningOptions.None);
		}

		// MD 3/24/10 - TFS28374
		// Added a new overload.
		/// <summary>
		/// Sets the bounds of the shape in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The shape will only be positioned at the specified bounds while the worksheet remains in the current configuration.
		/// Depending on the <see cref="PositioningMode"/> of the shape, it may change bounds if any rows or columns before or within the shape are resized.
		/// </p>
		/// </remarks>s
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <param name="worksheet">The worksheet on which the shape should be placed.</param>
		/// <param name="bounds">The new bounds where the shape should be placed.</param> 
		/// <param name="options">The options to use when setting the bounds of the shape.</param>
		public void SetBoundsInTwips(Worksheet worksheet, Rectangle bounds, PositioningOptions options)
		{
			this.SetBoundsInTwips(worksheet, bounds, options, true);
		}

		// MD 3/5/10 - TFS26342
		// Added new overload.


#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

		// MD 3/23/10 - TFS28374
		// Added an options paremter.
		//private void SetBoundsInTwips(Worksheet worksheet, Rectangle bounds, bool callOnBoundsChanged)
		private void SetBoundsInTwips(Worksheet worksheet, Rectangle bounds, PositioningOptions options, bool callOnBoundsChanged)
		{
			if ( worksheet == null )
				throw new ArgumentNullException( "worksheet" );

			// MD 3/24/10 - TFS28374
			// Cache the value indicating whether we should ignore the Hidden property on rows and collumns.
			bool ignoreHidden = (options & PositioningOptions.IgnoreHiddenRowsAndColumns) != 0;

			// Store the number of twips need to get to the right and left edges of the shape
			int twipsToLeftEdge = (int)bounds.Left;
            int twipsToRightEdge = (int)bounds.Right;

			// Start a the first column for each edge
			int firstColIndex = 0;
			int lastColIndex = 0;

			// MD 9/11/08 - Cell Comments
			// This is slow, especially when only a few column are defined and we are trying to get the bounds of a cell at the right of
			// the worksheet. We should really only iterate the defined columns and just calculate the space in the skipped columns.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)


			#endregion Old Code

			// MD 3/15/12 - TFS104581
			// This is no longer needed.
			//int previousColumnIndex = -1;

			int defaultColumnWidthInTwips = worksheet.GetColumnWidthInTwips( -1 );

			// MD 3/5/10
			// Found while fixing TFS26342
			// We need some flags to indicate whether we found the columns and should stop looking.
			bool foundLeftColumn = false;
			bool foundRightColumn = false;

			// MD 3/15/12 - TFS104581
			// Refactored this code to measure with column blocks instead of individual columns.
			#region Old Code

			//foreach ( WorksheetColumn column in worksheet.Columns )
			//{
			//    // Determine how many columns were skipped since the last column
			//    int columnsSkipped = column.Index - previousColumnIndex - 1;

			//    if ( columnsSkipped > 0 )
			//    {
			//        int twipsSkipped = columnsSkipped * defaultColumnWidthInTwips;

			//        //// Add in all the space for the skipped columns if they were before the top left cell.
			//        // MD 3/5/10
			//        // Found while fixing TFS26342
			//        // We only need to do this when we haven't found the desired column yet.
			//        //int twipsSkippedBeforeLeftCell = Math.Min(twipsSkipped, twipsToLeftEdge);
			//        //int wholeColumnsSkippedBeforeLeftCell = twipsSkippedBeforeLeftCell / defaultColumnWidthInTwips;
			//        //int twipsInWholeColumnsSkippedBeforeLeftCell = wholeColumnsSkippedBeforeLeftCell * defaultColumnWidthInTwips;
			//        //twipsToLeftEdge -= twipsInWholeColumnsSkippedBeforeLeftCell;
			//        //firstColIndex += wholeColumnsSkippedBeforeLeftCell;
			//        if (foundLeftColumn == false)
			//        {
			//            if (twipsToLeftEdge <= twipsSkipped)
			//                foundLeftColumn = true;

			//            // Add in all the space for the skipped columns if they were before the top left cell.
			//            int twipsSkippedBeforeLeftCell = Math.Min(twipsSkipped, twipsToLeftEdge);

			//            // MD 2/23/12 - TFS101326
			//            //int wholeColumnsSkippedBeforeLeftCell = twipsSkippedBeforeLeftCell / defaultColumnWidthInTwips;
			//            int wholeColumnsSkippedBeforeLeftCell =
			//                WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsSkippedBeforeLeftCell, defaultColumnWidthInTwips, columnsSkipped);

			//            int twipsInWholeColumnsSkippedBeforeLeftCell = wholeColumnsSkippedBeforeLeftCell * defaultColumnWidthInTwips;
			//            twipsToLeftEdge -= twipsInWholeColumnsSkippedBeforeLeftCell;
			//            firstColIndex += wholeColumnsSkippedBeforeLeftCell;
			//        }

			//        //// Add in all the space for the skipped columns if they were before the bottom right cell.
			//        // MD 3/5/10
			//        // Found while fixing TFS26342
			//        // We only need to do this when we haven't found the desired column yet.
			//        //int twipsSkippedBeforeRightCell = Math.Min(twipsSkipped, twipsToRightEdge);
			//        //int wholeColumnsSkippedBeforeRightCell = twipsSkippedBeforeRightCell / defaultColumnWidthInTwips;
			//        //int twipsInWholeColumnsSkippedBeforeRightCell = wholeColumnsSkippedBeforeRightCell * defaultColumnWidthInTwips;
			//        //twipsToRightEdge -= twipsInWholeColumnsSkippedBeforeRightCell;
			//        //lastColIndex += wholeColumnsSkippedBeforeRightCell;
			//        if (foundRightColumn == false)
			//        {
			//            if (twipsToRightEdge <= twipsSkipped)
			//                foundRightColumn = true;

			//            // Add in all the space for the skipped columns if they were before the bottom right cell.
			//            int twipsSkippedBeforeRightCell = Math.Min(twipsSkipped, twipsToRightEdge);

			//            // MD 2/23/12 - TFS101326
			//            //int wholeColumnsSkippedBeforeRightCell = twipsSkippedBeforeRightCell / defaultColumnWidthInTwips;
			//            int wholeColumnsSkippedBeforeRightCell =
			//                WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsSkippedBeforeRightCell, defaultColumnWidthInTwips, columnsSkipped);

			//            int twipsInWholeColumnsSkippedBeforeRightCell = wholeColumnsSkippedBeforeRightCell * defaultColumnWidthInTwips;
			//            twipsToRightEdge -= twipsInWholeColumnsSkippedBeforeRightCell;
			//            lastColIndex += wholeColumnsSkippedBeforeRightCell;
			//        }
			//    }

			//    // Store the width of the current column
			//    // MD 3/24/10 - TFS28374
			//    // Pass of the ignoreHidden parameter.
			//    //int colWidthTwips = worksheet.GetColumnWidthInTwips( column.Index );
			//    // MD 7/23/10 - TFS35969
			//    // Since we already have a reference to the column, there's no reason to call the overload that searches for the column 
			//    // from the index again. Call the new overload that takes a column directly.
			//    //int colWidthTwips = worksheet.GetColumnWidthInTwips(column.Index, ignoreHidden);
			//    //int colWidthTwips = worksheet.GetColumnWidthInTwips(column, ignoreHidden);
			//    int colWidthTwips = worksheet.GetColumnWidthInTwips(worksheet.GetColumnBlock((short)column.Index), ignoreHidden);

			//    // If both edges are within the current column, stop iterating through the columns
			//    if ( twipsToLeftEdge < colWidthTwips && twipsToRightEdge < colWidthTwips )
			//        break;

			//    // If the left edge is past the current column, update how many more twips we need 
			//    // to go to get to the left edge...also increment the first column index
			//    // MD 3/5/10
			//    // Found while fixing TFS26342
			//    // We only need to do this when we haven't found the desired column yet.
			//    //if ( colWidthTwips <= twipsToLeftEdge )
			//    //{
			//    //    twipsToLeftEdge -= colWidthTwips;
			//    //    firstColIndex++;
			//    //}
			//    if (foundLeftColumn == false)
			//    {
			//        if (colWidthTwips <= twipsToLeftEdge)
			//        {
			//            twipsToLeftEdge -= colWidthTwips;
			//            firstColIndex++;
			//        }
			//        else
			//        {
			//            foundLeftColumn = true;
			//        }
			//    }

			//    // If the right edge is past the current column, update how many more twips we need 
			//    // to go to get to the right edge...also increment the last column index
			//    // MD 3/5/10
			//    // Found while fixing TFS26342
			//    // We only need to do this when we haven't found the desired column yet.
			//    //if ( colWidthTwips <= twipsToRightEdge )
			//    //{
			//    //    twipsToRightEdge -= colWidthTwips;
			//    //    lastColIndex++;
			//    //}
			//    if (foundRightColumn == false)
			//    {
			//        if (colWidthTwips <= twipsToRightEdge)
			//        {
			//            twipsToRightEdge -= colWidthTwips;
			//            lastColIndex++;
			//        }
			//        else
			//        {
			//            foundRightColumn = true;
			//        }
			//    }

			//    // MD 3/5/10
			//    // Found while fixing TFS26342
			//    // If we found both columns already, we can break out of the loop.
			//    if (foundLeftColumn && foundRightColumn)
			//        break;

			//    // MD 10/2/08 - TFS8467
			//    // This was a pretty big oversight. I was never setting the previous index.
			//    previousColumnIndex = column.Index;
			//}

			#endregion // Old Code
			foreach (WorksheetColumnBlock columnBlock in worksheet.ColumnBlocks.Values)
			{
				// Determine how many columns were skipped since the last column
				int columnsInBlock = columnBlock.LastColumnIndex - columnBlock.FirstColumnIndex + 1;

				int columnInBlockWidth = 0;
				if (ignoreHidden || columnBlock.Hidden == false)
				{
					if (columnBlock.Width < 0)
						columnInBlockWidth = defaultColumnWidthInTwips;
					else
						columnInBlockWidth = (int)columnBlock.GetWidth(worksheet, WorksheetColumnWidthUnit.Twip);
				}

				int twipsInBlock = columnsInBlock * columnInBlockWidth;

				// Add in all the space for the columns in the block if they were before the top left cell.
				if (foundLeftColumn == false)
				{
					if (twipsToLeftEdge <= twipsInBlock)
						foundLeftColumn = true;

					// Add in all the space for the columns in the block if they were before the top left cell.
					int twipsBeforeLeftCell = Math.Min(twipsInBlock, twipsToLeftEdge);
					int wholeColumnsBeforeLeftCell =
						WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsBeforeLeftCell, columnInBlockWidth, columnsInBlock);

					int twipsInWholeColumnsBeforeLeftCell = wholeColumnsBeforeLeftCell * columnInBlockWidth;
					twipsToLeftEdge -= twipsInWholeColumnsBeforeLeftCell;
					firstColIndex += wholeColumnsBeforeLeftCell;
				}

				// Add in all the space for the columns in the block if they were before the bottom right cell.
				if (foundRightColumn == false)
				{
					if (twipsToRightEdge <= twipsInBlock)
						foundRightColumn = true;

					// Add in all the space for the columns in the block if they were before the bottom right cell.
					int twipsBeforeRightCell = Math.Min(twipsInBlock, twipsToRightEdge);
					int wholeColumnsBeforeRightCell =
						WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsBeforeRightCell, columnInBlockWidth, columnsInBlock);

					int twipsInWholeColumnsBeforeRightCell = wholeColumnsBeforeRightCell * columnInBlockWidth;
					twipsToRightEdge -= twipsInWholeColumnsBeforeRightCell;
					lastColIndex += wholeColumnsBeforeRightCell;
				}

				// If we found both columns already, we can break out of the loop.
				if (foundLeftColumn && foundRightColumn)
					break;
			}

			// MD 10/2/08 - TFS8467
			// Cache the widths of the columns for each anchor cell because they are now used twice below.
			// MD 3/24/10 - TFS28374
			// Pass of the ignoreHidden parameter.
			//int firstColumnWidth = worksheet.GetColumnWidthInTwips( firstColIndex );
			//int lastColumnWidth = worksheet.GetColumnWidthInTwips( lastColIndex );
			int firstColumnWidth = worksheet.GetColumnWidthInTwips(firstColIndex, ignoreHidden);
			int lastColumnWidth = worksheet.GetColumnWidthInTwips(lastColIndex, ignoreHidden);

			// MD 10/2/08 - TFS8467
			// This check was oncorrect. To see if any columns are skipped, see if the anchor is still out farther than the anchor's 
			// column width, not the default column width.
			//if ( defaultColumnWidthInTwips < twipsToLeftEdge )
			// MD 3/5/10
			// Found while fixing TFS26342
			// We only need to do this when we haven't found the desired column yet.
			//if ( firstColumnWidth < twipsToLeftEdge )
			if (foundLeftColumn == false && firstColumnWidth < twipsToLeftEdge)
			{
				// MD 2/23/12 - TFS101326
				//int wholeColumnsSkippedBeforeLeftCell = twipsToLeftEdge / defaultColumnWidthInTwips;
				int wholeColumnsSkippedBeforeLeftCell =
					WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsToLeftEdge, defaultColumnWidthInTwips, 0);

				int twipsInWholeColumnsSkippedBeforeLeftCell = wholeColumnsSkippedBeforeLeftCell * defaultColumnWidthInTwips;
				twipsToLeftEdge -= twipsInWholeColumnsSkippedBeforeLeftCell;
				firstColIndex += wholeColumnsSkippedBeforeLeftCell;
			}

			// MD 10/2/08 - TFS8467
			// This check was oncorrect. To see if any columns are skipped, see if the anchor is still out farther than the anchor's 
			// column width, not the default column width.
			//if ( defaultColumnWidthInTwips < twipsToRightEdge )
			// MD 3/5/10
			// Found while fixing TFS26342
			// We only need to do this when we haven't found the desired column yet.
			//if ( lastColumnWidth < twipsToRightEdge )
			if (foundRightColumn == false && lastColumnWidth < twipsToRightEdge)
			{
				// MD 2/23/12 - TFS101326
				//int wholeColumnsSkippedBeforeRightCell = twipsToRightEdge / defaultColumnWidthInTwips;
				int wholeColumnsSkippedBeforeRightCell =
					WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsToRightEdge, defaultColumnWidthInTwips, 0);

				int twipsInWholeColumnsSkippedBeforeRightCell = wholeColumnsSkippedBeforeRightCell * defaultColumnWidthInTwips;
				twipsToRightEdge -= twipsInWholeColumnsSkippedBeforeRightCell;
				lastColIndex += wholeColumnsSkippedBeforeRightCell;
			}

			// Get the percentage across the cells where the left and right edges are
			// MD 10/2/08 - TFS8467
			// Use the cached widths from above
			//float firstColPercentage = ( twipsToLeftEdge * 100f ) / worksheet.GetColumnWidthInTwips( firstColIndex );
			//float lastColPercentage = ( twipsToRightEdge * 100f ) / worksheet.GetColumnWidthInTwips( lastColIndex );
			float firstColPercentage = ( twipsToLeftEdge * 100f ) / firstColumnWidth;
			float lastColPercentage = ( twipsToRightEdge * 100f ) / lastColumnWidth;

			// Store the number of twips need to get to the top and bottom edges of the shape
            int twipsToTopEdge = (int)bounds.Top;
            int twipsToBottomEdge = (int)bounds.Bottom;

			// Start a the first row for each edge
			int firstRowIndex = 0;
			int lastRowIndex = 0;

			// MD 9/10/08 - Cell Comments
			// This is slow, especially when only a few rows are defined and we are trying to get the bounds of a cell at the bottom of
			// the worksheet. We should really only iterate the defined rows and just calculate the space in the skipped rows.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)


			#endregion Old Code
			int previousRowIndex = -1;
			int defaultRowHeightInTwips = worksheet.GetRowHeightInTwips( -1 );

			// MD 3/5/10
			// Found while fixing TFS26342
			// We need some flags to indicate whether we found the rows and should stop looking.
			bool foundTopRow = false;
			bool foundBottomRow = false;

			foreach ( WorksheetRow row in worksheet.Rows )
			{
				// Determine how many rows were skipped since the last row
				int rowsSkipped = row.Index - previousRowIndex - 1;

				if ( rowsSkipped > 0 )
				{
					int twipsSkipped = rowsSkipped * defaultRowHeightInTwips;

					//// Add in all the space for the skipped rows if they were before the top left cell.
					// MD 3/5/10
					// Found while fixing TFS26342
					// We only need to do this when we haven't found the desired row yet.
					//int twipsSkippedBeforeTopCell = Math.Min(twipsSkipped, twipsToTopEdge);
					//int wholeRowsSkippedBeforeTopCell = twipsSkippedBeforeTopCell / defaultRowHeightInTwips;
					//int twipsInWholeRowsSkippedBeforeTopCell = wholeRowsSkippedBeforeTopCell * defaultRowHeightInTwips;
					//twipsToTopEdge -= twipsInWholeRowsSkippedBeforeTopCell;
					//firstRowIndex += wholeRowsSkippedBeforeTopCell;
					if (foundTopRow == false)
					{
						if (twipsToTopEdge <= twipsSkipped)
							foundTopRow = true;

						// Add in all the space for the skipped rows if they were before the top left cell.
						int twipsSkippedBeforeTopCell = Math.Min(twipsSkipped, twipsToTopEdge);

						// MD 2/23/12 - TFS101326
						//int wholeRowsSkippedBeforeTopCell = twipsSkippedBeforeTopCell / defaultRowHeightInTwips;
						int wholeRowsSkippedBeforeTopCell = 
							WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsSkippedBeforeTopCell, defaultRowHeightInTwips, rowsSkipped);

						int twipsInWholeRowsSkippedBeforeTopCell = wholeRowsSkippedBeforeTopCell * defaultRowHeightInTwips;
						twipsToTopEdge -= twipsInWholeRowsSkippedBeforeTopCell;
						firstRowIndex += wholeRowsSkippedBeforeTopCell;
					}

					//// Add in all the space for the skipped rows if they were before the bottom right cell.
					// MD 3/5/10
					// Found while fixing TFS26342
					// We only need to do this when we haven't found the desired row yet.
					//int twipsSkippedBeforeBottomCell = Math.Min(twipsSkipped, twipsToBottomEdge);
					//int wholeRowsSkippedBeforeBottomCell = twipsSkippedBeforeBottomCell / defaultRowHeightInTwips;
					//int twipsInWholeRowsSkippedBeforeBottomCell = wholeRowsSkippedBeforeBottomCell * defaultRowHeightInTwips;
					//twipsToBottomEdge -= twipsInWholeRowsSkippedBeforeBottomCell;
					//lastRowIndex += wholeRowsSkippedBeforeBottomCell;
					if (foundBottomRow == false)
					{
						if (twipsToBottomEdge <= twipsSkipped)
							foundBottomRow = true;

						// Add in all the space for the skipped rows if they were before the bottom right cell.
						int twipsSkippedBeforeBottomCell = Math.Min(twipsSkipped, twipsToBottomEdge);

						// MD 2/23/12 - TFS101326
						// If the default rows have no height, we skipped all of them.
						//int wholeRowsSkippedBeforeBottomCell = twipsSkippedBeforeBottomCell / defaultRowHeightInTwips;
						int wholeRowsSkippedBeforeBottomCell =
							WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsSkippedBeforeBottomCell, defaultRowHeightInTwips, rowsSkipped);

						int twipsInWholeRowsSkippedBeforeBottomCell = wholeRowsSkippedBeforeBottomCell * defaultRowHeightInTwips;
						twipsToBottomEdge -= twipsInWholeRowsSkippedBeforeBottomCell;
						lastRowIndex += wholeRowsSkippedBeforeBottomCell;
					}
				}

				// Store the height of the current row
				// MD 3/24/10 - TFS28374
				// Pass of the ignoreHidden parameter.
				//int rowHeightTwips = worksheet.GetRowHeightInTwips( row.Index );
				// MD 7/23/10 - TFS35969
				// Since we already have a reference to the row, there's no reason to call the overload that searches for the row 
				// from the index again. Call the new overload that takes a row directly.
				//int rowHeightTwips = worksheet.GetRowHeightInTwips(row.Index, ignoreHidden);
				int rowHeightTwips = worksheet.GetRowHeightInTwips(row, ignoreHidden);

				// If both edges are within the current row, stop iterating through the rows
				if ( twipsToTopEdge < rowHeightTwips && twipsToBottomEdge < rowHeightTwips )
					break;

				// If the top edge is past the current row, update how many more twips we need 
				// to go to get to the top edge...also increment the first row index
				// MD 3/5/10
				// Found while fixing TFS26342
				// We only need to do this when we haven't found the desired row yet.
				//if ( rowHeightTwips <= twipsToTopEdge )
				//{
				//    twipsToTopEdge -= rowHeightTwips;
				//    firstRowIndex++;
				//}
				if (foundTopRow == false)
				{
					if (rowHeightTwips <= twipsToTopEdge)
					{
						twipsToTopEdge -= rowHeightTwips;
						firstRowIndex++;
					}
					else
					{
						foundTopRow = true;
					}
				}

				// If the bottom edge is past the current row, update how many more twips we need 
				// to go to get to the bottom edge...also increment the last row index
				// MD 3/5/10
				// Found while fixing TFS26342
				// We only need to do this when we haven't found the desired row yet.
				//if ( rowHeightTwips <= twipsToBottomEdge )
				//{
				//    twipsToBottomEdge -= rowHeightTwips;
				//    lastRowIndex++;
				//}
				if (foundBottomRow == false)
				{
					if (rowHeightTwips <= twipsToBottomEdge)
					{
						twipsToBottomEdge -= rowHeightTwips;
						lastRowIndex++;
					}
					else
					{
						foundBottomRow = true;
					}
				}

				// MD 3/5/10
				// Found while fixing TFS26342
				// If we found both columns already, we can break out of the loop.
				if (foundTopRow && foundBottomRow)
					break;

				// MD 10/2/08 - TFS8467
				// This was a pretty big oversight. I was never setting the previous index.
				previousRowIndex = row.Index;
			}

			// MD 10/2/08 - TFS8467
			// Cache the heights of the rows for each anchor cell because they are now used twice below.
			// MD 3/24/10 - TFS28374
			// Pass of the ignoreHidden parameter.
			//int firstRowHeight = worksheet.GetRowHeightInTwips( firstRowIndex );
			//int lastRowHeight = worksheet.GetRowHeightInTwips( lastRowIndex );
			int firstRowHeight = worksheet.GetRowHeightInTwips(firstRowIndex, ignoreHidden);
			int lastRowHeight = worksheet.GetRowHeightInTwips(lastRowIndex, ignoreHidden);

			// MD 10/2/08 - TFS8467
			// This check was oncorrect. To see if any rows are skipped, see if the anchor is still out farther than the anchor's 
			// row height, not the default row height.
			//if ( defaultRowHeightInTwips < twipsToTopEdge )
			// MD 3/5/10
			// Found while fixing TFS26342
			// We only need to do this when we haven't found the desired row yet.
			//if ( firstRowHeight < twipsToTopEdge )
			if (foundTopRow == false && firstRowHeight < twipsToTopEdge)
			{
				// MD 2/23/12 - TFS101326
				//int wholeRowsSkippedBeforeTopCell = twipsToTopEdge / defaultRowHeightInTwips;
				int wholeRowsSkippedBeforeTopCell =
					WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsToTopEdge, defaultRowHeightInTwips, 0);

				int twipsInWholeRowsSkippedBeforeTopCell = wholeRowsSkippedBeforeTopCell * defaultRowHeightInTwips;
				twipsToTopEdge -= twipsInWholeRowsSkippedBeforeTopCell;
				firstRowIndex += wholeRowsSkippedBeforeTopCell;
			}

			// MD 10/2/08 - TFS8467
			// This check was oncorrect. To see if any rows are skipped, see if the anchor is still out farther than the anchor's 
			// row height, not the default row height.
			//if ( defaultRowHeightInTwips < twipsToBottomEdge )
			// MD 3/5/10
			// Found while fixing TFS26342
			// We only need to do this when we haven't found the desired row yet.
			//if ( lastRowHeight < twipsToBottomEdge )
			if (foundBottomRow == false && lastRowHeight < twipsToBottomEdge)
			{
				// MD 2/23/12 - TFS101326
				//int wholeRowsSkippedBeforeBottomCell = twipsToBottomEdge / defaultRowHeightInTwips;
				int wholeRowsSkippedBeforeBottomCell =
					WorksheetShape.GetNumberOfMissingRowsOrColumns(twipsToBottomEdge, defaultRowHeightInTwips, 0);

				int twipsInWholeRowsSkippedBeforeBottomCell = wholeRowsSkippedBeforeBottomCell * defaultRowHeightInTwips;
				twipsToBottomEdge -= twipsInWholeRowsSkippedBeforeBottomCell;
				lastRowIndex += wholeRowsSkippedBeforeBottomCell;
			}

			// Get the percentage across the cells where the top and bottom edges are
			// MD 10/2/08 - TFS8467
			// Use the cached heights from above
			//float firstRowPercentage = ( twipsToTopEdge * 100f ) / worksheet.GetRowHeightInTwips( firstRowIndex );
			//float lastRowPercentage = ( twipsToBottomEdge * 100f ) / worksheet.GetRowHeightInTwips( lastRowIndex );
			float firstRowPercentage = ( twipsToTopEdge * 100f ) / firstRowHeight;
			float lastRowPercentage = ( twipsToBottomEdge * 100f ) / lastRowHeight;

			// MD 9/26/08
			// If this is a group, setting each anchor property separately will resize the child shapes multiple times and we may lose some 
			// data as a result of rounding errors. Instead, set all anchors at once and only recalculate the child shape bounds once.
			//// Set the shape's anchors based on the calculated cells and percentages
			//this.TopLeftCornerPosition = new PointF( firstColPercentage, firstRowPercentage );
			//this.BottomRightCornerPosition = new PointF( lastColPercentage, lastRowPercentage );
			//this.TopLeftCornerCell = worksheet.Rows[ firstRowIndex ].Cells[ firstColIndex ];
			//this.BottomRightCornerCell = worksheet.Rows[ lastRowIndex ].Cells[ lastColIndex ];
			PointF oldTopLeftCornerPosition = this.topLeftCornerPosition;
			PointF oldBottomRightCornerPosition = this.bottomRightCornerPosition;

			// MD 3/27/12 - 12.1 - Table Support
			//WorksheetCell oldTopLeftCornerCell = this.topLeftCornerCell;
			//WorksheetCell oldBottomRightCornerCell = this.bottomRightCornerCell;
			WorksheetCellAddress oldTopLeftCornerCell = this.topLeftCornerCell;
			WorksheetCellAddress oldBottomRightCornerCell = this.bottomRightCornerCell;

			this.topLeftCornerPosition = new PointF( firstColPercentage, firstRowPercentage );
			this.bottomRightCornerPosition = new PointF( lastColPercentage, lastRowPercentage );

			// MD 3/27/12 - 12.1 - Table Support
			//this.topLeftCornerCell = worksheet.Rows[ firstRowIndex ].Cells[ firstColIndex ];
			//this.bottomRightCornerCell = worksheet.Rows[ lastRowIndex ].Cells[ lastColIndex ];
			this.topLeftCornerCellWorksheet = worksheet;
			this.topLeftCornerCell = new WorksheetCellAddress(firstRowIndex, (short)firstColIndex);
			this.bottomRightCornerCellWorksheet = worksheet;
			this.bottomRightCornerCell = new WorksheetCellAddress(lastRowIndex, (short)lastColIndex);

			// MD 3/5/10 - TFS26342
			// Honor the new callOnBoundsChanged parameter.
			//this.OnBoundsChanged( oldTopLeftCornerCell, oldTopLeftCornerPosition, oldBottomRightCornerCell, oldBottomRightCornerPosition );
			if (callOnBoundsChanged)
			{
				// MD 3/27/12 - 12.1 - Table Support
				//this.OnBoundsChanged(oldTopLeftCornerCell, oldTopLeftCornerPosition, oldBottomRightCornerCell, oldBottomRightCornerPosition);
				this.OnBoundsChanged(worksheet, oldTopLeftCornerCell, oldTopLeftCornerPosition, oldBottomRightCornerCell, oldBottomRightCornerPosition);
			}
		}

		#endregion SetBoundsInTwips

		#endregion Public Methods

		#region Internal Methods

		// MD 7/18/11 - Shape support
		#region ConvertShapeType

		// MD 10/10/11 - TFS90805
		// We may not have a value to return.
		//internal static ST_ShapeType ConvertShapeType(ShapeType excel2003ShapeType)
		internal static ST_ShapeType? ConvertShapeType(ShapeType excel2003ShapeType)
		{
			// MD 11/1/11
			// Found while fixing TFS90733
			// The is no equivalent to the NotPrimitive shape type in Excel 2007
			if (excel2003ShapeType == ShapeType.NotPrimitive)
				return null;

			ST_ShapeType excel2007ShapeType;
			if (WorksheetShape.excel2007ShapeTypeByExcel2003ShapeType.TryGetValue(excel2003ShapeType, out excel2007ShapeType) == false)
			{
				Utilities.DebugFail("Could not find a mapping for this shape type: " + excel2003ShapeType);

				// MD 10/10/11 - TFS90805
				// Return null if we couldn't find the equivalent shape type.
				//return ST_ShapeType.rect;
				return null;
			}

			return excel2007ShapeType;
		}

		// MD 10/10/11 - TFS90805
		// We may not have a value to return.
		//internal static ShapeType ConvertShapeType(ST_ShapeType excel2007ShapeType)
		internal static ShapeType? ConvertShapeType(ST_ShapeType excel2007ShapeType)
		{
			ShapeType excel2003ShapeType;
			if (WorksheetShape.excel2003ShapeTypeByExcel2007ShapeType.TryGetValue(excel2007ShapeType, out excel2003ShapeType) == false)
			{
				// MD 10/10/11 - TFS90805
				// Return null if we couldn't find the equivalent shape type. Also, don't assert because not all
				// 2007 shapes types can convert to 2003 shape types.
				//Utilities.DebugFail("Could not find a mapping for this shape type: " + excel2007ShapeType);
				//return ShapeType.Rectangle;
				return null;
			}

			return excel2003ShapeType;
		}

		#endregion // ConvertShapeType

		#region GetBoundsInTwips






		// MD 3/27/12 - 12.1 - Table Support
		//internal static Rectangle GetBoundsInTwips(
		//    WorksheetCell topLeftCornerCell, 
		//    PointF topLeftCornerPosition,
		//    WorksheetCell bottomRightCornerCell, 
		//    PointF bottomRightCornerPosition )
		//{
		//    // MD 3/24/10 - TFS28374
		//    // Moved code to new overload.
		//    return WorksheetShape.GetBoundsInTwips(topLeftCornerCell, topLeftCornerPosition, bottomRightCornerCell, bottomRightCornerPosition, PositioningOptions.None);
		//}
		internal static Rectangle GetBoundsInTwips(
			Worksheet worksheet,
			WorksheetCellAddress topLeftCornerCell,
			PointF topLeftCornerPosition,
			WorksheetCellAddress bottomRightCornerCell,
			PointF bottomRightCornerPosition)
		{
			return WorksheetShape.GetBoundsInTwips(worksheet,
				topLeftCornerCell, topLeftCornerPosition,
				bottomRightCornerCell, bottomRightCornerPosition,
				PositioningOptions.None);
		}

		// MD 3/24/10 - TFS28374
		// Added new overload.





		// MD 3/27/12 - 12.1 - Table Support
		//internal static Rectangle GetBoundsInTwips(
		//    WorksheetCell topLeftCornerCell,
		//    PointF topLeftCornerPosition,
		//    WorksheetCell bottomRightCornerCell,
		//    PointF bottomRightCornerPosition,
		//    PositioningOptions options)
		internal static Rectangle GetBoundsInTwips(
			Worksheet worksheet,
			WorksheetCellAddress topLeftCornerCell,
			PointF topLeftCornerPosition,
			WorksheetCellAddress bottomRightCornerCell,
			PointF bottomRightCornerPosition,
			PositioningOptions options)
		{
			// MD 3/27/12 - 12.1 - Table Support
			#region Old Code

			//// MD 2/29/12 - 12.1 - Table Support
			//// The worksheet can now be null.
			//if (topLeftCornerCell.Row == null || bottomRightCornerCell.Row == null)
			//{
			//    Utilities.DebugFail("This is unexpected");
			//    return Rectangle.Empty;
			//}
			//
			//// MD 4/12/11 - TFS67084
			//// Moved all code to the new overload.
			//return WorksheetShape.GetBoundsInTwips(
			//    topLeftCornerCell.Row,
			//    topLeftCornerCell.ColumnIndexInternal,
			//    topLeftCornerPosition,
			//    bottomRightCornerCell.Row,
			//    bottomRightCornerCell.ColumnIndexInternal,
			//    bottomRightCornerPosition,
			//    options);

			#endregion // Old Code
			if (topLeftCornerCell == WorksheetCellAddress.InvalidReference ||
				bottomRightCornerCell == WorksheetCellAddress.InvalidReference)
			{
				Utilities.DebugFail("This is unexpected");
				return Rectangle.Empty;
			}

			// MD 4/12/11 - TFS67084
			// Moved all code to the new overload.
			return WorksheetShape.GetBoundsInTwips(
				worksheet,
				topLeftCornerCell.RowIndex,
				topLeftCornerCell.ColumnIndex,
				topLeftCornerPosition,
				bottomRightCornerCell.RowIndex,
				bottomRightCornerCell.ColumnIndex,
				bottomRightCornerPosition,
				options);
		}

		// MD 4/12/11 - TFS67084
		// Added a new overload so we don't have to use WorksheetCell objects.
		// MD 3/27/12 - 12.1 - Table Support
		//internal static Rectangle GetBoundsInTwips(
		//    WorksheetRow topRow,
		//    short leftColumnIndex,
		//    PointF topLeftCornerPosition,
		//    WorksheetRow bottomRow,
		//    short rightColumnIndex,
		//    PointF bottomRightCornerPosition,
		//    PositioningOptions options)
		internal static Rectangle GetBoundsInTwips(
			Worksheet worksheet,
			int topRowIndex,
			short leftColumnIndex,
			PointF topLeftCornerPosition,
			int bottomRowIndex,
			short rightColumnIndex,
			PointF bottomRightCornerPosition,
			PositioningOptions options)
		{
			// Use the worksheet of the top-left anchor cell as the worksheet to get twip dimensions from
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Worksheet worksheet = topLeftCornerCell.Worksheet;
			// MD 3/27/12 - 12.1 - Table Support
			//Worksheet worksheet = topRow.Worksheet;
			if (worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return Rectangle.Empty;
			}

			// MD 3/24/10 - TFS28374
			// Cache the value indicating whether we should ignore the Hidden property on rows and columns.
			bool ignoreHidden = (options & PositioningOptions.IgnoreHiddenRowsAndColumns) != 0;

			// MD 7/23/10 - TFS35969
			// I made this code much more efficient by moving it off to the load on demand tree, which now caches item 
			// extents on the nodes.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)


#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)


			#endregion Old Code
			int firstColTwips;
			int lastColTwips;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//worksheet.Columns.GetDistanceToLeftOfColumns(topLeftCornerCell.ColumnIndex, bottomRightCornerCell.ColumnIndex, ignoreHidden, out firstColTwips, out lastColTwips);
			worksheet.Columns.GetDistanceToLeftOfColumns(leftColumnIndex, rightColumnIndex, ignoreHidden, out firstColTwips, out lastColTwips);

			// Add in the partial width of the anchor cells
			// MD 8/1/08 - BR34692
			// Casting directly to an int caused rounding issues, we need to round to the nearest number instead.
			//firstColTwips += (int)( ( worksheet.GetColumnWidthInTwips( topLeftCornerCell.ColumnIndex ) * topLeftCornerPosition.X ) / 100 );
			//lastColTwips += (int)( ( worksheet.GetColumnWidthInTwips( bottomRightCornerCell.ColumnIndex ) * bottomRightCornerPosition.X ) / 100 );
			// MD 3/5/10 - TFS26342
			// Now that the positions can be over 100, we need to cap them when calculating the actual bounds, because anything over 100 
			// acts as if it were 100 when getting the absolute position.
			//float twipsForPartialFirstCol = ( worksheet.GetColumnWidthInTwips( topLeftCornerCell.ColumnIndex ) * topLeftCornerPosition.X ) / 100;
			// MD 3/24/10 - TFS28374
			// Pass of the ignoreHidden parameter.
			//float twipsForPartialFirstCol = (worksheet.GetColumnWidthInTwips(topLeftCornerCell.ColumnIndex) * Math.Min((float)topLeftCornerPosition.X, 100)) / 100;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//float twipsForPartialFirstCol = (worksheet.GetColumnWidthInTwips(topLeftCornerCell.ColumnIndex, ignoreHidden) * Math.Min((float)topLeftCornerPosition.X, 100)) / 100;
			float twipsForPartialFirstCol = (worksheet.GetColumnWidthInTwips(leftColumnIndex, ignoreHidden) * Math.Min((float)topLeftCornerPosition.X, 100)) / 100;

			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
			//firstColTwips += Utilities.MidpointRoundingAwayFromZero(twipsForPartialFirstCol);
			firstColTwips += (int)MathUtilities.MidpointRoundingAwayFromZero(twipsForPartialFirstCol);

			// MD 3/5/10 - TFS26342
			// Now that the positions can be over 100, we need to cap them when calculating the actual bounds, because anything over 100 
			// acts as if it were 100 when getting the absolute position.
			//float twipsForPartialLastCol = ( worksheet.GetColumnWidthInTwips( bottomRightCornerCell.ColumnIndex ) * bottomRightCornerPosition.X ) / 100;
			// MD 3/24/10 - TFS28374
			// Pass of the ignoreHidden parameter.
			//float twipsForPartialLastCol = (worksheet.GetColumnWidthInTwips(bottomRightCornerCell.ColumnIndex) * Math.Min((float)bottomRightCornerPosition.X, 100)) / 100;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//float twipsForPartialLastCol = (worksheet.GetColumnWidthInTwips(bottomRightCornerCell.ColumnIndex, ignoreHidden) * Math.Min((float)bottomRightCornerPosition.X, 100)) / 100;
			float twipsForPartialLastCol = (worksheet.GetColumnWidthInTwips(rightColumnIndex, ignoreHidden) * Math.Min((float)bottomRightCornerPosition.X, 100)) / 100;
			
			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
			//lastColTwips += Utilities.MidpointRoundingAwayFromZero(twipsForPartialLastCol);
			lastColTwips += (int)MathUtilities.MidpointRoundingAwayFromZero(twipsForPartialLastCol);

			// MD 7/23/10 - TFS35969
			// I made this code much more efficient by moving it off to the load on demand tree, which now caches item 
			// extents on the nodes.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)


#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)


			#endregion Old Code
			int firstRowTwips;
			int lastRowTwips;

			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, cache the rows and get the index values from there.
			//worksheet.Rows.GetDistanceToTopOfRows(topLeftCornerCell.RowIndex, bottomRightCornerCell.RowIndex, ignoreHidden, out firstRowTwips, out lastRowTwips);
			// MD 4/12/11 - TFS67084
			// The rows are now passed in.
			//WorksheetRow topLeftCornerCellRow = topLeftCornerCell.Row;
			//WorksheetRow bottomRightCornerCellRow = bottomRightCornerCell.Row;
			//worksheet.Rows.GetDistanceToTopOfRows(topLeftCornerCellRow.Index, bottomRightCornerCellRow.Index, ignoreHidden, out firstRowTwips, out lastRowTwips);
			worksheet.Rows.GetDistanceToTopOfRows(topRowIndex, bottomRowIndex, ignoreHidden, out firstRowTwips, out lastRowTwips);

			// Add in the partial height of the anchor cells
			// MD 8/1/08 - BR34692
			// Casting directly to an int caused rounding issues, we need to round to the nearest number instead.
			//firstRowTwips += (int)( ( worksheet.GetRowHeightInTwips( topLeftCornerCell.RowIndex ) * topLeftCornerPosition.Y ) / 100 );
			//lastRowTwips += (int)( ( worksheet.GetRowHeightInTwips( bottomRightCornerCell.RowIndex ) * bottomRightCornerPosition.Y ) / 100 );
			// MD 3/5/10 - TFS26342
			// Now that the positions can be over 100, we need to cap them when calculating the actual bounds, because anything over 100 
			// acts as if it were 100 when getting the absolute position.
			//float twipsForPartialFirstRow = ( worksheet.GetRowHeightInTwips( topLeftCornerCell.RowIndex ) * topLeftCornerPosition.Y ) / 100;
			// MD 3/24/10 - TFS28374
			// Pass of the ignoreHidden parameter.
			//float twipsForPartialFirstRow = (worksheet.GetRowHeightInTwips(topLeftCornerCell.RowIndex) * Math.Min((float)topLeftCornerPosition.Y, 100)) / 100;
			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, we can just use that instead of passing in the row index.
			//float twipsForPartialFirstRow = (worksheet.GetRowHeightInTwips(topLeftCornerCell.RowIndex, ignoreHidden) * Math.Min((float)topLeftCornerPosition.Y, 100)) / 100;
			// MD 4/12/11 - TFS67084
			// Use the passed in row.
			//float twipsForPartialFirstRow = (worksheet.GetRowHeightInTwips(topLeftCornerCellRow, ignoreHidden) * Math.Min((float)topLeftCornerPosition.Y, 100)) / 100;
			float twipsForPartialFirstRow = (worksheet.GetRowHeightInTwips(topRowIndex, ignoreHidden) * Math.Min((float)topLeftCornerPosition.Y, 100)) / 100;

			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
			//firstRowTwips += Utilities.MidpointRoundingAwayFromZero(twipsForPartialFirstRow);
			firstRowTwips += (int)MathUtilities.MidpointRoundingAwayFromZero(twipsForPartialFirstRow);

			// MD 3/5/10 - TFS26342
			// Now that the positions can be over 100, we need to cap them when calculating the actual bounds, because anything over 100 
			// acts as if it were 100 when getting the absolute position.
			//float twipsForPartialLastRow = ( worksheet.GetRowHeightInTwips( bottomRightCornerCell.RowIndex ) * bottomRightCornerPosition.Y ) / 100;
			// MD 3/24/10 - TFS28374
			// Pass of the ignoreHidden parameter.
			//float twipsForPartialLastRow = (worksheet.GetRowHeightInTwips(bottomRightCornerCell.RowIndex) * Math.Min((float)bottomRightCornerPosition.Y, 100)) / 100;
			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, we can just use that instead of passing in the row index.
			//float twipsForPartialLastRow = (worksheet.GetRowHeightInTwips(bottomRightCornerCell.RowIndex, ignoreHidden) * Math.Min((float)bottomRightCornerPosition.Y, 100)) / 100;
			// MD 4/12/11 - TFS67084
			// Use the passed in row.
			//float twipsForPartialLastRow = (worksheet.GetRowHeightInTwips(bottomRightCornerCellRow, ignoreHidden) * Math.Min((float)bottomRightCornerPosition.Y, 100)) / 100;
			float twipsForPartialLastRow = (worksheet.GetRowHeightInTwips(bottomRowIndex, ignoreHidden) * Math.Min((float)bottomRightCornerPosition.Y, 100)) / 100;

			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
			//lastRowTwips += Utilities.MidpointRoundingAwayFromZero(twipsForPartialLastRow);
			lastRowTwips += (int)MathUtilities.MidpointRoundingAwayFromZero(twipsForPartialLastRow);

            return new Rectangle(firstColTwips, firstRowTwips, lastColTwips - firstColTwips, lastRowTwips - firstRowTwips);
		}

		#endregion GetBoundsInTwips

		// MD 9/14/11 - TFS86093
		#region InitializeDefaults

		// MD 3/12/12 - TFS102234
		// Made this virtual so it could be overridden in derived classes.
		//internal void InitializeDefaults()
		internal virtual void InitializeDefaults()
		{
			if (this.IsConnector)
			{
				this.Outline = ShapeOutline.FromColor(Color.FromArgb(0xFF, 0x4A, 0x7E, 0xBB));
			}
			else
			{
				this.Fill = ShapeFill.FromColor(Color.FromArgb(0xFF, 0x4F, 0x81, 0xBD));
				this.Outline = ShapeOutline.FromColor(Color.FromArgb(0xFF, 0x38, 0x5D, 0x8A));
			}
		}

		#endregion  // InitializeDefaults

		// MD 7/14/11 - Shape support
		#region InitializeFrom

		internal void InitializeFrom(WorksheetShape shape)
		{
			// MD 3/27/12 - 12.1 - Table Support
			this.bottomRightCornerCellWorksheet = shape.bottomRightCornerCellWorksheet;
			this.topLeftCornerCellWorksheet = shape.topLeftCornerCellWorksheet;

			this.bottomRightCornerCell = shape.bottomRightCornerCell;
			this.bottomRightCornerPosition = shape.bottomRightCornerPosition;
			this.positioningMode = shape.positioningMode;
			this.topLeftCornerCell = shape.topLeftCornerCell;
			this.topLeftCornerPosition = shape.topLeftCornerPosition;
			this.visible = shape.visible;
			this.drawingProperties1 = shape.drawingProperties1;
			this.drawingProperties2 = shape.drawingProperties2;
			this.drawingProperties3 = shape.drawingProperties3;

			// MD 10/30/11 - TFS90733
			//this.objRecords = shape.objRecords;
			this.obj = shape.obj;

			this.rotation = shape.rotation;
			this.txoOptionFlags = shape.txoOptionFlags;
			this.calloutRule = shape.calloutRule;
			this.shapeId = shape.shapeId;
			this.excel2007ShapeSerializationManager = shape.excel2007ShapeSerializationManager;
			this.flippedHorizontally = shape.flippedHorizontally;
			this.flippedVertically = shape.flippedVertically;

			// MD 10/10/11 - TFS81451
			this.shapeProperties2007Element = shape.shapeProperties2007Element;

			// MD 3/12/12 - TFS102234
			this.fill = shape.fill;
			this.outline = shape.outline;
		}

		#endregion // InitializeFrom

		// MD 8/23/11 - TFS84306
		// This is needed now because strings on shapes are no longer placed in the shared string table.
		//// MD 11/3/10 - TFS49093
		//// This is no longer needed because the shared formatted strings will be iterated over and their fonts will be placed 
		//// in the manager when saving.
		//// MD 9/2/08 - Cell Comments
		#region InitSerializationCache

		internal virtual void InitSerializationCache(WorkbookSerializationManager serializationManager) { }

		#endregion InitSerializationCache 

		#region OnAddingToCollection







		internal virtual void OnAddingToCollection( WorksheetShapeCollection collection )
		{
			// Make sure the shape does not belong to another collection
			if ( this.collection != null )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_ShapeInAnotherCollection" ) );

            //  BF 8/21/08  Excel2007 Format
            //  Don't throw this exception if the collection belongs to a group
            //  that is loading.
            if ( this.IsLoading(collection) == false )
            {
			    // Make sure the anchor cells have been set before the shape is added to a collection.
			    if ( this.topLeftCornerCell == null || this.bottomRightCornerCell == null )
				    throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_AnchorShapeBeforeAddingToCollection" ) );
            }

			// Get the worksheet from the new parent collection
			Worksheet worksheet = collection.Worksheet;

			// If the collection is sited on a worksheet, this shape is now sited on a worksheet
			if ( worksheet != null )
				this.OnSitedOnWorksheet( worksheet );

			// Store the collection the shape has been added to
			this.collection = collection;
		}

		#endregion OnAddingToCollection

		// MD 3/3/10 - TFS26342
		#region OnAfterWorksheetElementResize



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void OnAfterWorksheetElementResize(RowColumnBase worksheetElement, int oldElementExtentInTwipsHiddenIgnored, bool wasHidden)
		{
			// Cache the original positions of the shape and store them in temp variables. We may change these.
			PointF tempTopLeftCornerPosition = this.TopLeftCornerPosition;
			PointF tempBottomRightCornerPosition = this.BottomRightCornerPosition;

			float topLeftPosition;
			float bottomRightPosition;

			// Determine the relevant dimensions of the shape position, depending on whether a row or column was resized.
			if (worksheetElement is WorksheetRow)
			{
                topLeftPosition = (float)tempTopLeftCornerPosition.Y;
                bottomRightPosition = (float)tempBottomRightCornerPosition.Y;
			}
			else
			{
                topLeftPosition = (float)tempTopLeftCornerPosition.X;
                bottomRightPosition = (float)tempBottomRightCornerPosition.X;
			}

			// Calculate all the adjustments that need to be made to the bounds, anchor cell positions, or both.
			int absolutePositionOffsetInTwips;
			int absoluteExtentOffsetInTwips;
			this.CalculateAdjustmentsForWorksheetElementResize(
				worksheetElement,
				oldElementExtentInTwipsHiddenIgnored,
				wasHidden,
				ref topLeftPosition,
				ref bottomRightPosition,
				out absolutePositionOffsetInTwips,
				out absoluteExtentOffsetInTwips);

			// Apply the calculated adjustments to the bounds and anchor cell positions.
			Rectangle boundsAfterWorksheetElementResizeInTwips = this.boundsBeforeWorksheetElementResizeInTwips;
			if (worksheetElement is WorksheetRow)
			{
				tempTopLeftCornerPosition.Y = topLeftPosition;
				tempBottomRightCornerPosition.Y = bottomRightPosition;
				boundsAfterWorksheetElementResizeInTwips.Y += absolutePositionOffsetInTwips;
				boundsAfterWorksheetElementResizeInTwips.Height += absoluteExtentOffsetInTwips;
			}
			else
			{
				tempTopLeftCornerPosition.X = topLeftPosition;
				tempBottomRightCornerPosition.X = bottomRightPosition;
				boundsAfterWorksheetElementResizeInTwips.X += absolutePositionOffsetInTwips;
				boundsAfterWorksheetElementResizeInTwips.Width += absoluteExtentOffsetInTwips;
			}

			// Resize the shape based on the current positioning mode of the shape.
			switch (this.PositioningMode)
			{
				case ShapePositioningMode.DontMoveOrSizeWithCells:
					// If the mode is DontMoveOrSizeWithCells, the absolute bounds will remain the same regardless of the changes to the rows or columns,
					// so just reapply the bounds to the shape. No absolute positioning changed should have been calculated, so the old and new bounds 
					// should be the same.
					Debug.Assert(boundsAfterWorksheetElementResizeInTwips == this.boundsBeforeWorksheetElementResizeInTwips, "No changes should have been calculated for the absolute bounds.");
					this.SetBoundsInTwips(this.Worksheet, boundsAfterWorksheetElementResizeInTwips);
					Debug.Assert(this.GetBoundsInTwips() == boundsAfterWorksheetElementResizeInTwips, "Something went wrong when setting the bounds.");
					break;

				case ShapePositioningMode.MoveAndSizeWithCells:
					{
						PointF oldTopLeftCornerPosition = this.TopLeftCornerPosition;
						PointF oldBottomRightCornerPosition = this.BottomRightCornerPosition;

						// If the mode is MoveAndSizeWithCells, the bounds are irrelevant. The anchor cells will not change, but the relative positions 
						// within those cells may change becasue the absolute position within the cell remains constant (if possible). So appply the newly
						// calculated anchor cell positions.
						this.topLeftCornerPosition = tempTopLeftCornerPosition;
						this.bottomRightCornerPosition = tempBottomRightCornerPosition;

						// Notify all listeners that the bounds of the shape may have changed. 
						// MD 3/27/12 - 12.1 - Table Support
						//this.OnBoundsChanged(
						//    this.TopLeftCornerCell,
						//    oldTopLeftCornerPosition,
						//    this.BottomRightCornerCell,
						//    oldBottomRightCornerPosition);
						this.OnBoundsChanged(
							worksheetElement.Worksheet,
							this.TopLeftCornerCellInternal,
							oldTopLeftCornerPosition,
							this.BottomRightCornerCellInternal,
							oldBottomRightCornerPosition);
					}
					break;

				case ShapePositioningMode.MoveWithCells:
					{
						// MD 3/27/12 - 12.1 - Table Support
						//WorksheetCell oldTopLeftCornerCell = this.topLeftCornerCell;
						//PointF oldTopLeftCornerPosition = this.topLeftCornerPosition;
						//WorksheetCell oldBottomRightCornerCell = this.bottomRightCornerCell;
						//PointF oldBottomRightCornerPosition = this.bottomRightCornerPosition;
						WorksheetCellAddress oldTopLeftCornerCell = this.topLeftCornerCell;
						PointF oldTopLeftCornerPosition = this.topLeftCornerPosition;
						WorksheetCellAddress oldBottomRightCornerCell = this.bottomRightCornerCell;
						PointF oldBottomRightCornerPosition = this.bottomRightCornerPosition;

						// If the mode is MoveWithCells, the Size of the original bounds must be maintained, so we need to first call SetBoundsInTwips with the new bounds. 
						// The Location of boundsAfterWorksheetElementResizeInTwips is technically the correct absolute position for the shape here, so the call to SetBoundsInTwips
						// would be enough. However, if the new positions have percentages value greater than 100, SetBoundsInTwips will just assign 100 to those position values, 
						// becasue anything over 100 uses the same absolute position as 100. Therefore, we need to prevent SetBoundsInTwips from calling OnBoundsChanged, because 
						// the new positions may not be correct. When it returns, we will assign the correct positions and manually call OnBoundsChanged.
						// MD 3/24/10 - TFS28374
						// This helper method has a new parameter now.
						//this.SetBoundsInTwips(this.Worksheet, boundsAfterWorksheetElementResizeInTwips, false);
						this.SetBoundsInTwips(this.Worksheet, boundsAfterWorksheetElementResizeInTwips, PositioningOptions.None, false);

						this.topLeftCornerCell = oldTopLeftCornerCell;
						this.topLeftCornerPosition = tempTopLeftCornerPosition;

						this.OnBoundsChanged(
							// MD 3/27/12 - 12.1 - Table Support
							worksheetElement.Worksheet,
							oldTopLeftCornerCell,
							oldTopLeftCornerPosition,
							oldBottomRightCornerCell,
							oldBottomRightCornerPosition);

						Debug.Assert(this.GetBoundsInTwips() == boundsAfterWorksheetElementResizeInTwips, "Something went wrong when setting the bounds.");
					}
					break;
			}

			// Reset the cached bounds of the shape.
			this.boundsBeforeWorksheetElementResizeInTwips = Rectangle.Empty;
		}

		#endregion // OnAfterWorksheetElementResize

		// MD 3/4/10 - TFS26342
		#region OnBeforeWorksheetElementResize






		internal void OnBeforeWorksheetElementResize()
		{
			// MD 7/23/10 - TFS35969
			// When the positioning mode is MoveAndSizeWithCells, we never use the boundsBeforeWorksheetElementResizeInTwips,
			// so don't even bother caching it.
			if (this.PositioningMode == ShapePositioningMode.MoveAndSizeWithCells)
			    return;

			// Cache the absolute bounds of the shape before the resize occurs.
			this.boundsBeforeWorksheetElementResizeInTwips = this.GetBoundsInTwips();
		}

		#endregion // OnBeforeWorksheetElementResize

		#region OnBoundsChanged






		// MD 3/27/12 - 12.1 - Table Support
		//internal virtual void OnBoundsChanged(
		//    WorksheetCell oldTopLeftCornerCell, 
		//    PointF oldTopLeftCornerPosition,
		//    WorksheetCell oldBottomRightCornerCell, 
		//    PointF oldBottomRightCornerPosition )
		internal virtual void OnBoundsChanged(
			Worksheet worksheet,
			WorksheetCellAddress oldTopLeftCornerCell,
			PointF oldTopLeftCornerPosition,
			WorksheetCellAddress oldBottomRightCornerCell,
			PointF oldBottomRightCornerPosition)
		{
			// If the bounds of this shape have changed, let the owner of the shape's parent 
			// collection know about it
			if ( this.collection != null )
				this.collection.Owner.OnChildShapeBoundsChanged( this );
		}

		#endregion OnBoundsChanged

		// MD 9/11/08 - Cell Comments
		#region OnDrawingProperties1Changed

		// MD 7/18/11 - Shape support
		// Moved all logic from WorksheetShapeWithText.OnDrawingProperties1Changed to here because it is needed by all shapes, not just those with text.
		//internal virtual void OnDrawingProperties1Changed() { }
		internal void OnDrawingProperties1Changed() 
		{
			// MD 7/18/11
			// Found while adding Shape support. We should be checking for null here.
			if (this.DrawingProperties1 == null)
				return;

			foreach (PropertyTableBase.PropertyValue property in this.DrawingProperties1)
			{
				switch (property.PropertyType)
				{
					case PropertyType.ExtendedProperties:
						{
							if ((property.Value is uint) == false)
							{
								Utilities.DebugFail("This property should have had a uint value.");
								break;
							}

							uint value = (uint)property.Value;

							// MD 8/23/11 - TFS84306
							// Renamed this constant to be consistent with the other property related constants.
							//this.Visible = (value & WorksheetShapeWithText.HiddenBitOnExtendedProperties) == 0;
							this.Visible = (value & WorksheetShapeWithText.ExtendedProperties_Hidden) == 0;

							break;
						}
				}
			}
		}

		#endregion OnDrawingProperties1Changed

		#region OnRemovedFromCollection






		// MD 11/3/10 - TFS49093
		//internal void OnRemovedFromCollection()
		internal virtual void OnRemovedFromCollection()
		{
			Debug.Assert( this.collection != null );

			// The shape doesn't belong to its parent collection anymore, so dont keep a reference to it
			this.collection = null;
		}

		#endregion OnRemovedFromCollection

		#region OnSitedOnWorksheet







		internal virtual void OnSitedOnWorksheet( Worksheet worksheet )
		{
			if ( this.topLeftCornerCellWorksheet != worksheet )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_TopLeftAnchorFromOtherWorksheet" ) );

			if ( this.bottomRightCornerCellWorksheet != worksheet )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_BottomRightAnchorFromOtherWorksheet" ) );
		}

		#endregion OnSitedOnWorksheet

		// MD 8/23/11 - TFS84306
		#region PopulateColorProperties






		internal void PopulateColorProperties(Color color, PropertyType colorPropertyType, PropertyType opacityPropertyType)
		{
			uint colorValue = (uint)(
				(color.B << 16) |
				(color.G << 8) |
				color.R);

			byte alpha = color.A;

			this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(colorPropertyType, colorValue));

			if (alpha != 255)
			{
				double opactiy = alpha / 255d;
				uint opacityFixedValue = Utilities.ToFixedPoint16_16Value(opactiy);
				this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(opacityPropertyType, opacityFixedValue));
			}
		}

		#endregion  // PopulateColorProperties

		#region PopuplateDrawingProperties



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal virtual void PopuplateDrawingProperties( WorkbookSerializationManager manager ) 
		{
			// MD 7/18/11 - Shape support

			// Only do this for pre-defined shapes.
			// MD 10/10/11 - TFS90805
			//if (Enum.IsDefined(typeof(PredefinedShapeType), (PredefinedShapeType)this.Type))
			if (this.Type2003.HasValue && Enum.IsDefined(typeof(PredefinedShapeType), (PredefinedShapeType)this.Type2003.Value))
				this.PopulateDrawingPropertiesForPredefinedShapes();

			// MD 8/23/11 - TFS84306
			// Populate all "known" properties (properties which are directly supported by the public object model).
			// UpdateExtendedPropertiesForVisibility no longer needs to be called because its logic has been merged into PopulateKnownProperties.
			//this.UpdateExtendedPropertiesForVisibility();
			this.PopulateKnownProperties();
		}

		#endregion PopuplateDrawingProperties

		// MD 8/23/11 - TFS84306
		#region PopulateKnownProperties






		// MD 8/7/12 - TFS115692
		//internal void PopulateKnownProperties()
		internal virtual void PopulateKnownProperties()
		{
			bool foundExtendedPropertiesValue = false;

			if (this.DrawingProperties1 == null)
			{
				this.DrawingProperties1 = new List<PropertyTableBase.PropertyValue>();
			}
			else
			{
				// Remove known properties if they are already in the DrawingProperties1 collection.
				for (int i = this.DrawingProperties1.Count - 1; i >= 0; i--)
				{
					PropertyTableBase.PropertyValue propertyValue = this.DrawingProperties1[i];
					switch (propertyValue.PropertyType)
					{
						case PropertyType.TextLeft:	// MD 8/7/12 - TFS115692
						case PropertyType.TextTop:	// MD 8/7/12 - TFS115692
						case PropertyType.TextRight: // MD 8/7/12 - TFS115692
						case PropertyType.TextBottom: // MD 8/7/12 - TFS115692
						case PropertyType.FillStyleColor:
						case PropertyType.FillStyleOpacity:
						case PropertyType.FillStyleNoFillHitTest:
						case PropertyType.LineStyleColor:
						case PropertyType.LineStyleOpacity:
						case PropertyType.LineStyleNoLineDrawDash:
						case PropertyType.TransformRotation: // MD 7/24/12 - TFS115693
							this.DrawingProperties1.RemoveAt(i);
							break;

						// MD 8/7/12 - TFS115692
						case PropertyType.TextFitToShape:
							{
								if ((propertyValue.Value is uint) == false)
								{
									Utilities.DebugFail("This property should have had a uint value.");
									this.DrawingProperties1.RemoveAt(i);
									break;
								}

								uint textFlags = (uint)propertyValue.Value;

								// MD 8/9/12 - TFS115692
								// For some reason, removing these bits may change the font in the text box. Temporarily rolling back this
								// part of the fix until a better solution can be found.
								//// Strip out the fUsefAutoTextMargin and fAutoTextMargin bits since we will specify the margins
								//textFlags &= 0xFFF7FFF7;

								propertyValue.Value = textFlags;
							}
							break;

						case PropertyType.ExtendedProperties:
							{
								if ((propertyValue.Value is uint) == false)
								{
									Utilities.DebugFail("This property should have had a uint value.");
									this.DrawingProperties1.RemoveAt(i);
									break;
								}

								foundExtendedPropertiesValue = true;
								uint value = (uint)propertyValue.Value;

								if (this.Visible)
									value &= ~WorksheetShape.ExtendedProperties_Hidden;
								else
									value |= WorksheetShape.ExtendedProperties_Hidden;

								propertyValue.Value = value;
								break;
							}
					}
				}
			}

			if (foundExtendedPropertiesValue == false)
			{
				uint extendedPropertiesDefaultValue = WorksheetShape.ExtendedProperties_Default;
				if (this.Visible == false)
					extendedPropertiesDefaultValue |= WorksheetShape.ExtendedProperties_Hidden;

				this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.ExtendedProperties, extendedPropertiesDefaultValue));
			}

			if (this.CanHaveFill)
				this.FillResolved.PopulateDrawingProperties2003(this);

			if (this.CanHaveOutline)
				this.OutlineResolved.PopulateDrawingProperties2003(this);

			// MD 7/24/12 - TFS115693
			// We now handle the TransformRotation property
			if (this.Rotation != 0)
			{
				uint rotationValue = Utilities.ToFixedPoint16_16Value(this.Rotation);
				this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TransformRotation, rotationValue));
			}
		}

		#endregion  // PopulateKnownProperties

		#region PopulateObjRecords






		// MD 9/2/08 - Cell Comments
		//internal void PopulateObjRecords()
		internal virtual void PopulateObjRecords()
		{
			// MD 10/30/11 - TFS90733
			// Instead of manually populating the ObjRecords collection, we will now create one Obj instance, which will 
			// manage its stored records.
			//if ( this.objRecords != null )
			//    return;
			//
			//this.objRecords = new List<OBJRecordBase>();
			//
			//// MD 9/2/08 - Cell Comments
			//// The object id is a unique value and will be assigned later.
			////this.objRecords.Add( new CommonObjectData( ObjectType.Picture, 1 ) );
			//this.objRecords.Add( new CommonObjectData( ObjectType.Picture ) );
			//
			//this.objRecords.Add( new ClipboardFormat() );
			//this.objRecords.Add( new PictureOptionFlags() );
			//this.objRecords.Add( new End() );
			if ( this.obj == null )
				this.obj = new Obj(this);
		}

		#endregion PopulateObjRecords

		// MD 10/12/10 - TFS49853
		#region SetAnchors






		// MD 3/27/12 - 12.1 - Table Support
		//internal void SetAnchors(
		//    WorksheetCell topLeftCell,
		//    PointF topLeftPosition,
		//    WorksheetCell bottomRightCell,
		//    PointF bottomRightPosition)
		internal void SetAnchors(
			Worksheet worksheet,
			WorksheetCellAddress topLeftCell,
			PointF topLeftPosition,
			WorksheetCellAddress bottomRightCell,
			PointF bottomRightPosition)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//WorksheetCell oldTopLeftCell = this.topLeftCornerCell;
			//PointF oldTopLeftPosition = this.topLeftCornerPosition;
			//WorksheetCell oldBottomRightCell = this.bottomRightCornerCell;
			//PointF oldBottomRightPosition = this.bottomRightCornerPosition;
			WorksheetCellAddress oldTopLeftCell = this.topLeftCornerCell;
			PointF oldTopLeftPosition = this.topLeftCornerPosition;
			WorksheetCellAddress oldBottomRightCell = this.bottomRightCornerCell;
			PointF oldBottomRightPosition = this.bottomRightCornerPosition;

			this.topLeftCornerCellWorksheet = worksheet;
			this.bottomRightCornerCellWorksheet = worksheet;

			this.topLeftCornerCell = topLeftCell;
			this.topLeftCornerPosition = topLeftPosition;
			this.bottomRightCornerCell = bottomRightCell;
			this.bottomRightCornerPosition = bottomRightPosition;

			// MD 3/27/12 - 12.1 - Table Support
			//this.OnBoundsChanged(oldTopLeftCell, oldTopLeftPosition, oldBottomRightCell, oldBottomRightPosition);
			this.OnBoundsChanged(worksheet, oldTopLeftCell, oldTopLeftPosition, oldBottomRightCell, oldBottomRightPosition);
		} 

		#endregion // SetAnchors

		// MD 8/24/07 - BR25924
		// We need a way to specify whether the verify the shape positioning mode, copied all code from the PositioningMode setter
		#region SetPositioningMode

		internal void SetPositioningMode( ShapePositioningMode value, bool verifyIsDefined )
		{
			if ( this.positioningMode != value )
			{
				if ( verifyIsDefined && Enum.IsDefined( typeof( ShapePositioningMode ), value ) == false )
					throw new InvalidEnumArgumentException( "value", (int)value, typeof( ShapePositioningMode ) );

				// MD 7/20/2007 - BR25039
				// Some positioning modes cannot be applied to certain shapes, verify the new mode is valid
				this.VerifyPositioningMode( value );

				this.positioningMode = value;
			}
		}

		#endregion SetPositioningMode

		// MD 3/27/12 - 12.1 - Table Support
		#region ShiftShape

		internal ShiftAddressResult ShiftShape(CellShiftOperation shiftOperation)
		{
			WorksheetCellAddress shiftedTopLeftCellAddress;
			PointF shiftedTopLeftCellPosition;
			WorksheetCellAddress shiftedBottomRightCellAddress;
			PointF shiftedBottomRightCellPosition;
			ShiftAddressResult result = this.ShiftShapeHelper(shiftOperation,
				out shiftedTopLeftCellAddress, out shiftedTopLeftCellPosition,
				out shiftedBottomRightCellAddress, out shiftedBottomRightCellPosition);

			if (result.DidShift && result.IsDeleted == false)
			{
				this.SetAnchors(this.topLeftCornerCellWorksheet,
					shiftedTopLeftCellAddress, shiftedTopLeftCellPosition,
					shiftedBottomRightCellAddress, shiftedBottomRightCellPosition);
			}

			return result;
		}

		internal ShiftAddressResult ShiftShapeHelper(CellShiftOperation shiftOperation,
			out WorksheetCellAddress shiftedTopLeftCellAddress, out PointF shiftedTopLeftCellPosition,
			out WorksheetCellAddress shiftedBottomRightCellAddress, out PointF shiftedBottomRightCellPosition)
		{
			shiftedBottomRightCellAddress = this.bottomRightCornerCell;
			shiftedBottomRightCellPosition = this.bottomRightCornerPosition;
			shiftedTopLeftCellAddress = this.topLeftCornerCell;
			shiftedTopLeftCellPosition = this.topLeftCornerPosition;

			if (this.PositioningMode == ShapePositioningMode.DontMoveOrSizeWithCells)
				return ShiftAddressResult.NoShiftResult;

			WorksheetRegionAddress topShapeRegionAddress = new WorksheetRegionAddress(
				shiftedTopLeftCellAddress.RowIndex, shiftedTopLeftCellAddress.RowIndex,
				shiftedTopLeftCellAddress.ColumnIndex, shiftedBottomRightCellAddress.ColumnIndex);

			WorksheetRegionAddress bottomShapeRegionAddress = new WorksheetRegionAddress(
				shiftedBottomRightCellAddress.RowIndex, shiftedBottomRightCellAddress.RowIndex,
				shiftedTopLeftCellAddress.ColumnIndex, shiftedBottomRightCellAddress.ColumnIndex);

			ShiftAddressResult topShiftResult = shiftOperation.ShiftRegionAddress(ref topShapeRegionAddress, false);
			ShiftAddressResult bottomShiftResult = shiftOperation.ShiftRegionAddress(ref bottomShapeRegionAddress, false);

			if (topShiftResult.DidShift)
			{
				// If the top of the shape was shifted, we should also try to shift the bottom of the shape.
				if (bottomShiftResult.DidShift)
				{
					if (bottomShiftResult.IsDeleted)
					{
						// If both the top and the bottom of the shape were deleted, or the bottom of the shape was deleted because 
						// it was shifted off the bottom of the worksheet, the shape should be deleted.
						if (topShiftResult.IsDeleted ||
							bottomShiftResult.DeleteReason == CellShiftDeleteReason.ShiftedOffWorksheetBottom)
						{
							shiftedTopLeftCellAddress = WorksheetCellAddress.InvalidReference;
							shiftedBottomRightCellAddress = WorksheetCellAddress.InvalidReference;
							return bottomShiftResult;
						}
					}

					WorksheetShape.ShiftShapeEdge(shiftOperation, bottomShapeRegionAddress.FirstRowIndex, bottomShiftResult,
						ref shiftedBottomRightCellAddress, ref shiftedBottomRightCellPosition);
				}

				WorksheetShape.ShiftShapeEdge(shiftOperation, topShapeRegionAddress.FirstRowIndex, topShiftResult,
					ref shiftedTopLeftCellAddress, ref shiftedTopLeftCellPosition);
			}
			else
			{
				// If the top left anchor cell didn't shift and the positioning mode is MoveWithCells, we don't need to update anything.
				if (this.PositioningMode == ShapePositioningMode.MoveWithCells)
					return ShiftAddressResult.NoShiftResult;

				// Otherwise if the positioning mode is MoveAndSizeWithCells, we should try to shift the bottom-right anchor cell.
				if (bottomShiftResult.DidShift == false)
					return ShiftAddressResult.NoShiftResult;

				WorksheetShape.ShiftShapeEdge(shiftOperation, bottomShapeRegionAddress.FirstRowIndex, bottomShiftResult,
					ref shiftedBottomRightCellAddress, ref shiftedBottomRightCellPosition);
			}

			// If the bottom of the shape is now above the top of the shape, move the bottom back down to the top of the shape.
			if (shiftedBottomRightCellAddress.RowIndex < shiftedTopLeftCellAddress.RowIndex)
			{
				shiftedBottomRightCellAddress.RowIndex = shiftedTopLeftCellAddress.RowIndex;
				shiftedBottomRightCellPosition.Y = shiftedTopLeftCellPosition.Y;
			}

			return bottomShiftResult;
		}

		#endregion // ShiftShape

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region ShiftShapeEdge

		private static void ShiftShapeEdge(CellShiftOperation shiftOperation, int newRowIndex, ShiftAddressResult shiftResult,
			ref WorksheetCellAddress anchorCellAddress, ref PointF anchorCellPosition)
		{
			if (shiftResult.IsDeleted)
			{
				if (shiftResult.DeleteReason == CellShiftDeleteReason.ShiftUpCoveredAddress ||
					shiftResult.DeleteReason == CellShiftDeleteReason.ShiftedOffWorksheetTop)
				{
					// If the cells were shifted up and deleted the edge of the shape, make the shape edge at the very top 
					// of the region after shift.
					anchorCellAddress.RowIndex = shiftOperation.RegionAddressAfterShift.FirstRowIndex;
					anchorCellPosition.Y = 0;
				}
				else
				{
					// If the cells were shifted down and deleted the edge of the shape, make the shape edge at the very bottom 
					// of the region after shift.
					anchorCellAddress.RowIndex = shiftOperation.RegionAddressAfterShift.LastRowIndex;
					anchorCellPosition.Y = 100;
				}
			}
			else
			{
				anchorCellAddress.RowIndex = newRowIndex;
			}
		}

		#endregion // ShiftShapeEdge

		// MD 7/14/11 - Shape support
		#region VerifyOrientationChange

		internal virtual void VerifyOrientationChange() { }

		#endregion // VerifyOrientationChange

		// MD 7/20/2007 - BR25039
		#region VerifyPositioningMode

		internal virtual void VerifyPositioningMode( ShapePositioningMode value )
		{

		}

		#endregion VerifyPositioningMode

        #endregion Internal Methods

        #region Private Methods

		// MD 7/15/11 - Shape support
		#region AddMapping

		private static void AddMapping(ShapeType excel2003ShapeType, ST_ShapeType excel2007ShapeType)
		{
			if (excel2003ShapeType == ShapeType.NotPrimitive)
			{
				Utilities.DebugFail("We should not be mapping the NotPrimitive type.");
				return;
			}

			WorksheetShape.excel2003ShapeTypeByExcel2007ShapeType.Add(excel2007ShapeType, excel2003ShapeType);
			WorksheetShape.excel2007ShapeTypeByExcel2003ShapeType.Add(excel2003ShapeType, excel2007ShapeType);
		}

		#endregion // AddMapping

		// MD 3/5/10 - TFS26342
		#region AdjustPositionInAnchorCellForWorksheetElementResize



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		private int AdjustPositionInAnchorCellForWorksheetElementResize(RowColumnBase worksheetElement, int oldElementExtentInTwipsHiddenIgnored, bool wasHidden, ref float position)
		{
			// Determine the actual amount of twips the element was using before.
			int oldElementExtentInTwips = wasHidden ? 0 : oldElementExtentInTwipsHiddenIgnored;

			// Determine the number of twips the element would use, regardless of whether or not it is shown.
			int newElementExtentInTwipsHiddenIgnored = worksheetElement.GetExtentInTwips(true);

			// Determine the actual amount of twips the element is using now.
			int newElementExtentInTwips = worksheetElement.Hidden ? 0 : newElementExtentInTwipsHiddenIgnored;

			// The preferred distance in the cell remains constant regardless of whether the row or column is resized or hidden, so figure out the 
			// preferred distance in the cell based on the last valid size.
			float preferredDistanceInCell = (oldElementExtentInTwipsHiddenIgnored * position) / 100;

			// Determine the distance into the anchor cell the shape's corner should actually be positioned before and after the resize.
			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
			//int oldDistanceInCell = Utilities.MidpointRoundingAwayFromZero(Math.Min(oldElementExtentInTwips, preferredDistanceInCell));
			//int newDistanceInCell = Utilities.MidpointRoundingAwayFromZero(Math.Min(newElementExtentInTwips, preferredDistanceInCell));
			int oldDistanceInCell = (int)MathUtilities.MidpointRoundingAwayFromZero(Math.Min(oldElementExtentInTwips, preferredDistanceInCell));
			int newDistanceInCell = (int)MathUtilities.MidpointRoundingAwayFromZero(Math.Min(newElementExtentInTwips, preferredDistanceInCell));

			// If the anchor cell was just unhidden, the position will remain the same, but the shape's corner will move out to its 
			// distance in the cell.
			if (wasHidden && worksheetElement.Hidden == false)
				return newDistanceInCell;

			// If the anchor cell was just hidden, the position will remain the same, but the shape's corner will move in from its 
			// old distance in the cell.
			if (wasHidden == false && worksheetElement.Hidden)
				return -oldDistanceInCell;

			// Since the position is a percentage of the preferred distance into the extent of the cell, recalculate it now, becasue the extent 
			// has changed.
			position = (100.0f * preferredDistanceInCell) / newElementExtentInTwipsHiddenIgnored;

			// If the column is hidden while the size changed, the bounds won't actually change. We just wanted to get in here so the position in 
			// the cell was updated.
			if (worksheetElement.Hidden)
				return 0;

			// If the element was just resized, adjust the position in the cell.
			return (newDistanceInCell - oldDistanceInCell);
		}

		#endregion // AdjustPositionInAnchorCellForWorksheetElementResize

		// MD 3/5/10 - TFS26342
		#region CalculateAdjustmentsForWorksheetElementResize



#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

		private void CalculateAdjustmentsForWorksheetElementResize(
			RowColumnBase worksheetElement,
			int oldElementExtentInTwipsHiddenIgnored,
			bool wasHidden,
			ref float topLeftPosition,
			ref float bottomRightPosition,
			out int absolutePositionOffsetInTwips,
			out int absoluteExtentOffsetInTwips)
		{
			absolutePositionOffsetInTwips = 0;
			absoluteExtentOffsetInTwips = 0;

			// When using MoveAndSizeWithCells or MoveWithCells modes, the shape must remain at the same absolute offsets in the top left corner cell,
			// unless the offset extends passed the new extent of the top left corner cell, in which case the shape will be anchored at the far end of 
			// the cell.
			if (this.PositioningMode == ShapePositioningMode.MoveAndSizeWithCells ||
				this.PositioningMode == ShapePositioningMode.MoveWithCells)
			{
				// If the element resized was before the shape, we just need to adjust the absolute position by the amount the top-left anchor cell's 
				// absolute position was offset, which is the amount by which the element's extent changed.
				// MD 7/26/10
				// Found while fixing TFS34398.
				// We should have been use the RowIndex only when the element was a row.
				//if (worksheetElement.Index < this.TopLeftCornerCell.RowIndex)
				int topLeftCornerIndex = worksheetElement is WorksheetRow
					? this.TopLeftCornerCell.RowIndex
					: this.TopLeftCornerCell.ColumnIndex;

				if (worksheetElement.Index < topLeftCornerIndex)
				{
					// Determine the actual amount of twips the element was using before.
					int oldElementExtentInTwips = wasHidden ? 0 : oldElementExtentInTwipsHiddenIgnored;

					// Determine the actual amount of twips the element is using now.
					int newElementExtentInTwips = worksheetElement.GetExtentInTwips(false);

					// Since an element before the shape has resized, the shape will just offset by the difference in the sizes.
					absolutePositionOffsetInTwips = (newElementExtentInTwips - oldElementExtentInTwips);
				}
				// MD 7/26/10
				// Found while fixing TFS34398.
				// We should have been use the RowIndex only when the element was a row.
				//else if (worksheetElement.Index == this.TopLeftCornerCell.RowIndex)
				else if (worksheetElement.Index == topLeftCornerIndex)
				{
					// If the element which resized contains the top left anchor cell, we need to adjust the top left position value and the actual position of 
					// the shape on the worksheet.
					absolutePositionOffsetInTwips = this.AdjustPositionInAnchorCellForWorksheetElementResize(worksheetElement, oldElementExtentInTwipsHiddenIgnored, wasHidden, ref topLeftPosition);
				}
			}

			if (this.PositioningMode == ShapePositioningMode.MoveAndSizeWithCells)
			{
				// MD 7/26/10
				// Found while fixing TFS34398.
				// We should have been use the RowIndex only when the element was a row.
				//if (worksheetElement.Index == this.BottomRightCornerCell.RowIndex)
				int bottomRightCornerIndex = worksheetElement is WorksheetRow
					? this.BottomRightCornerCell.RowIndex
					: this.BottomRightCornerCell.ColumnIndex;

				if (worksheetElement.Index == bottomRightCornerIndex)
				{
					// If the element which resized contains the bottom right anchor cell, we need to adjust the bottom right position value and the actual extent 
					// (width or height) of the shape on the worksheet.
					absoluteExtentOffsetInTwips = this.AdjustPositionInAnchorCellForWorksheetElementResize(worksheetElement, oldElementExtentInTwipsHiddenIgnored, wasHidden, ref bottomRightPosition);
				}
			}
		}

		#endregion // CalculateAdjustmentsForWorksheetElementResize

		// MD 2/23/12 - 12.1 - Table Support
		#region GetNumberOfMissingRowsOrColumns

		private static int GetNumberOfMissingRowsOrColumns(int missingExtent, int defaultItemExtent, int maxItems)
		{
			if (defaultItemExtent <= 0)
				return maxItems;

			return missingExtent / defaultItemExtent;
		}

		#endregion // GetNumberOfMissingRowsOrColumns

		// MD 7/18/11 - Shape support
		#region PopulateDrawingPropertiesForPredefinedShapes

		private void PopulateDrawingPropertiesForPredefinedShapes()
		{
			if (this.DrawingProperties1 == null)
				this.DrawingProperties1 = new List<PropertyTableBase.PropertyValue>();

			if (this.DrawingProperties1.Count == 0)
			{
				if (this.IsConnector)
				{
					this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.GeometryFillOK, (uint)0x00010000, false, false));
					// MD 8/23/11 - TFS84306
					// This is set dynamically now by the instance on the Fill and Outline properties of the shape.
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.FillStyleNoFillHitTest, (uint)0x00100000, false, false));
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleColor, (uint)0x00BB7E4A, false, false));
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleNoLineDrawDash, (uint)0x00180018, false, false));
					this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue((PropertyType)0x0303, (uint)0x00000000, false, false));
					// MD 8/23/11 - TFS84306
					// This doesn't need to be set because it is being set dynamically.
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.ExtendedProperties, (uint)0x00020000, false, false));
				}
				else
				{
					WorksheetShapeWithText shapeWithText = this as WorksheetShapeWithText;
					bool hasText = shapeWithText != null && shapeWithText.HasText;

					if (hasText)
					{
						this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.ProtectionLockAgainstGrouping, (uint)0x00040000, false, false));
						this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TextAnchorText, (uint)0x00000001, false, false));
						this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TextDirection, (uint)0x00000002, false, false));
					}

					this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TextFitToShape, (uint)0x001F0018, false, false));
					// MD 8/23/11 - TFS84306
					// This is set dynamically now by the instance on the Fill and Outline properties of the shape.
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.FillStyleColor, (uint)0x00BD814F, false, false));
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.FillStyleNoFillHitTest, (uint)0x00100010, false, false));
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleColor, (uint)0x008A5D38, false, false));
					this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleWidth, (uint)0x00006338, false, false));
					// MD 8/23/11 - TFS84306
					// This is set dynamically now by the instance on the Outline property of the shape.
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleNoLineDrawDash, (uint)0x00080008, false, false));
					// MD 8/23/11 - TFS84306
					// This doesn't need to be set because it is being set dynamically.
					//this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.ExtendedProperties, (uint)0x00020000, false, false));
				}
			}

			if (this.DrawingProperties3 == null)
				this.DrawingProperties3 = new List<PropertyTableBase.PropertyValue>();

			if (this.DrawingProperties3.Count == 0)
			{
				if (this.IsConnector)
				{
					this.DrawingProperties3.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleNoLineDrawDash, (uint)0x00400000, false, false));
				}
				else
				{
					this.DrawingProperties3.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleNoLineDrawDash, (uint)0x00400000, false, false));
				}
			}
		}

		#endregion  // PopulateDrawingPropertiesForPredefinedShapes

		// MD 8/23/11 - TFS84306
		// This logic has been merged into PopulateKnownProperties
		#region Removed

		//// MD 7/18/11 - Shape support
		//#region UpdateExtendedPropertiesForVisibility
		//
		//private void UpdateExtendedPropertiesForVisibility()
		//{
		//    if (this.DrawingProperties1 == null)
		//        this.DrawingProperties1 = new List<PropertyTableBase.PropertyValue>();
		//
		//    if (this.DrawingProperties1.Count > 0)
		//    {
		//        foreach (PropertyTableBase.PropertyValue property in this.DrawingProperties1)
		//        {
		//            if (property.PropertyType != PropertyType.ExtendedProperties)
		//                continue;
		//
		//            if ((property.Value is uint) == false)
		//            {
		//                Utilities.DebugFail("This property should have had a uint value.");
		//                continue;
		//            }
		//
		//            uint value = (uint)property.Value;
		//
		//            if (this.Visible)
		//                value &= ~WorksheetShapeWithText.HiddenBitOnExtendedProperties;
		//            else
		//                value |= WorksheetShapeWithText.HiddenBitOnExtendedProperties;
		//
		//            property.Value = value;
		//            return;
		//        }
		//    }
		//
		//    uint extendedPropertiesDefaultValue = 0x00020000;
		//    if (this.Visible == false)
		//        extendedPropertiesDefaultValue |= WorksheetShapeWithText.HiddenBitOnExtendedProperties;
		//
		//    this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.ExtendedProperties, extendedPropertiesDefaultValue, false, false));
		//}
		//
		//#endregion  // UpdateExtendedPropertiesForVisibility

		#endregion  // Removed

        #region VerifyAnchorCell







		private void VerifyAnchorCell( WorksheetCell value )
		{
			if ( value == null && this.collection != null )
				throw new ArgumentNullException( "value", SR.GetString( "LE_ArgumentNullException_AnchorCell" ) );

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (value != null && value.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			Worksheet worksheet = this.Worksheet;

			if ( worksheet != null && value != null && value.Worksheet != worksheet )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_AnchorCellFromOtherWorksheet" ), "value" );
		}

		#endregion VerifyAnchorCell

		#region VerifyAnchorPosition







		private static void VerifyAnchorPosition( PointF value )
		{
			// MD 3/5/10 - TFS26342
			// There doesn't seem to be an upper bound on the anchor cell position. If it is over 100, the shape will still be positioned
			// as if the position we 100, but when the cell is made larger in the Excel UI, the shape will move out to its preferred location.
			//if ( value.X < 0.0 || 100.0 < value.X ||
			//    value.Y < 0.0 || 100.0 < value.Y )
			if (value.X < 0.0 || value.Y < 0.0)
			{
				throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_AnchorPosition" ) );
			}
		}

		#endregion VerifyAnchorPosition

        //  BF 8/21/08  Excel2007 Format
        #region IsLoading

        private bool IsLoading( WorksheetShapeCollection collection )
        {
            WorksheetShapeGroup group = collection != null ? collection.Owner as WorksheetShapeGroup : null;
            return group != null && group.Loading;
        }
        
        #endregion IsLoading

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		// MD 8/23/11 - TFS84306
		#region Fill

		/// <summary>
		/// Gets or sets the fill to use in the background of the shape.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> some shapes, such as connectors or groups, cannot have a fill set. For these shapes, the value on this property
		/// will be ignored and lost when the workbook is saved.
		/// </p>
		/// </remarks>
		/// <value>A <see cref="ShapeFill"/>-derived instance describing the fill of the shape, or null for no fill.</value>
		/// <seealso cref="ShapeFill.FromColor"/>
		/// <seealso cref="ShapeFillSolid"/>
		/// <seealso cref="Outline"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public ShapeFill Fill
		{
			get { return this.fill; }
			set { this.fill = value; }
		}

		internal ShapeFill FillResolved
		{
			get { return this.fill ?? ShapeFillNoFill.Instance; }
		}

		#endregion  // Fill

		#region BottomRightCornerCell

		/// <summary>
		/// Gets or sets the cell where the bottom-right corner of the shape resides.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This anchor cell, along with the <see cref="TopLeftCornerCell"/>, determines where the shape will be
		/// positioned on the worksheet.  In addition, the <see cref="BottomRightCornerPosition"/> and 
		/// <see cref="TopLeftCornerPosition"/> properties allow for finer control of the shape's position.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The value assigned is a cell whose worksheet is not the same as this shape's worksheet.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null and this shape already exists on a worksheet or group.
		/// </exception>
		/// <value>The cell where the bottom-right corner of the shape resides.</value>
		/// <seealso cref="BottomRightCornerPosition"/>
		/// <seealso cref="TopLeftCornerCell"/>
		/// <seealso cref="TopLeftCornerPosition"/>
		public WorksheetCell BottomRightCornerCell
		{
			// MD 3/27/12 - 12.1 - Table Support
			//get { return this.bottomRightCornerCell; }
			//set
			//{
			//    if ( this.bottomRightCornerCell != value )
			//    {
			//        this.VerifyAnchorCell( value );

			//        WorksheetCell oldBottomRightCornerCell = this.bottomRightCornerCell;

			//        this.bottomRightCornerCell = value;

			//        this.OnBoundsChanged(
			//            this.topLeftCornerCell,
			//            this.topLeftCornerPosition,
			//            oldBottomRightCornerCell,
			//            this.bottomRightCornerPosition );
			//    }
			//}
			get 
			{
				if (this.bottomRightCornerCellWorksheet == null || 
					this.bottomRightCornerCell == WorksheetCellAddress.InvalidReference)
					return null;

				return this.bottomRightCornerCellWorksheet.Rows[this.bottomRightCornerCell.RowIndex].Cells[this.bottomRightCornerCell.ColumnIndex];
			}
			set
			{
				Worksheet newAnchorCellWorksheet;
				WorksheetCellAddress newAnchorCellAddress;
				if (value == null || value.Worksheet == null)
				{
					newAnchorCellWorksheet = null;
					newAnchorCellAddress = WorksheetCellAddress.InvalidReference;
				}
				else
				{
					newAnchorCellWorksheet = value.Worksheet;
					newAnchorCellAddress = value.Address;
				}

				if (newAnchorCellWorksheet != this.bottomRightCornerCellWorksheet ||
					newAnchorCellAddress != this.bottomRightCornerCell)
				{
					this.VerifyAnchorCell(value);

					WorksheetCellAddress oldBottomRightCornerCell = this.bottomRightCornerCell;

					this.bottomRightCornerCellWorksheet = newAnchorCellWorksheet;
					this.bottomRightCornerCell = newAnchorCellAddress;

					this.OnBoundsChanged(
						newAnchorCellWorksheet,
						this.topLeftCornerCell,
						this.topLeftCornerPosition,
						oldBottomRightCornerCell,
						this.bottomRightCornerPosition);
				}
			}
		}

		#endregion BottomRightCornerCell

		#region BottomRightCornerPosition

		/// <summary>
		/// Gets or sets the position in the <see cref="BottomRightCornerCell"/> of the shape's bottom-right corner,
		/// expressed in percentages.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// These percentages are expressed as distance across the associated dimension of the cell, starting at the 
		/// top-left corner of the cell.  For example, (0.0, 0.0) represents the top-left corner of the cell, whereas
		/// (100.0, 100.0) represents the bottom-right corner of the cell.  (50.0, 10.0) would represent the location 
		/// in the cell which is centered horizontally, and a tenth of the way down from the top.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Either coordinate of the value assigned is outside the range of 0.0 to 100.0.
		/// </exception>
		/// <value>The position in the bottom-right corner cell of the shape's bottom-right corner.</value>
		/// <seealso cref="BottomRightCornerCell"/>
		/// <seealso cref="TopLeftCornerCell"/>
		/// <seealso cref="TopLeftCornerPosition"/>
		public PointF BottomRightCornerPosition
		{
			get { return this.bottomRightCornerPosition; }
			set
			{
				if ( this.bottomRightCornerPosition != value )
				{
					WorksheetShape.VerifyAnchorPosition( value );

					PointF oldBottomRightCornerPosition = this.bottomRightCornerPosition;

					this.bottomRightCornerPosition = value;

					this.OnBoundsChanged(
						// MD 3/27/12 - 12.1 - Table Support
						this.bottomRightCornerCellWorksheet,
						this.topLeftCornerCell,
						this.topLeftCornerPosition,
						this.bottomRightCornerCell,
						oldBottomRightCornerPosition );
				}
			}
		}

		#endregion BottomRightCornerPosition

		// MD 7/14/11 - Shape support
		#region FlippedHorizontally

		/// <summary>
		/// Gets or sets the value which indicates whether the shape is flipped horizontally along the vertical center line.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The value is set to True and this shape doesn't allow flipping or rotating, such as a <see cref="WorksheetChart"/>.
		/// </exception>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public bool FlippedHorizontally
		{
			get { return this.flippedHorizontally; }
			set
			{
				if (this.flippedHorizontally == value)
					return;

				this.VerifyOrientationChange();
				this.flippedHorizontally = value;
			}
		}

		#endregion // FlippedHorizontally

		// MD 7/14/11 - Shape support
		#region FlippedVertically

		/// <summary>
		/// Gets or sets the value which indicates whether the shape is flipped vertically along the horizontal center line.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The value is set to True and this shape doesn't allow flipping or rotating, such as a <see cref="WorksheetChart"/>.
		/// </exception>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public bool FlippedVertically
		{
			get { return this.flippedVertically; }
			set
			{
				if (this.flippedVertically == value)
					return;

				this.VerifyOrientationChange();
				this.flippedVertically = value;
			}
		}

		#endregion // FlippedVertically

		// MD 8/23/11 - TFS84306
		#region Outline

		/// <summary>
		/// Gets or sets the outline to use for the shape.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> some shapes, such as comments or groups, cannot have a outline set. For these shapes, the value on this property
		/// will be ignored and lost when the workbook is saved.
		/// </p>
		/// </remarks>
		/// <value>A <see cref="ShapeOutline"/>-derived instance describing the outline of the shape, or null for no outline.</value>
		/// <seealso cref="ShapeOutline.FromColor"/>
		/// <seealso cref="ShapeOutlineSolid"/>
		/// <seealso cref="Fill"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 

		public ShapeOutline Outline
		{
			get { return this.outline; }
			set { this.outline = value; }
		}

		internal ShapeOutline OutlineResolved
		{
			get { return this.outline ?? ShapeOutlineNoOutline.Instance; }
		}

		#endregion  // Outline

		#region PositioningMode

		/// <summary>
		/// Gets or sets the way shapes will be repositioned in excel when cells before or within the shape are resized.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value will not be saved for shapes contained in a <see cref="WorksheetShapeGroup"/>, which inherit their
		/// positioning mode from their parent group.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ShapePositioningMode"/> enumeration.
		/// </exception>
		/// <value>The way shapes will be repositioned in excel when cells before or within the shape are resized.</value>
		public ShapePositioningMode PositioningMode
		{
			get { return this.positioningMode; }
			set
			{
				// MD 8/24/07 - BR25924
				// Moved all code to the SetPositioningMode method
				//if ( this.positioningMode != value )
				//{
				//    if ( Enum.IsDefined( typeof( ShapePositioningMode ), value ) == false )
				//        throw new InvalidEnumArgumentException( "value", (int)value, typeof( ShapePositioningMode ) );
				//
				//    // MD 7/20/2007 - BR25039
				//    // Some positioning modes cannot be applied to certain shapes, verify the new mode is valid
				//    this.VerifyPositioningMode( value );
				//
				//    this.positioningMode = value;
				//}
				this.SetPositioningMode( value, true );
			}
		}

		#endregion PositioningMode

		// MD 7/14/11 - Shape support
		#region Rotation

		
		
		
		
		internal float Rotation
		{
			get { return this.rotation; }
			set
			{
				if (this.rotation == value)
					return;

				this.VerifyOrientationChange();
				this.rotation = Math.Max(Int16.MinValue, Math.Min(value, Int16.MaxValue));
			}
		}

		#endregion Rotation

		#region TopLeftCornerCell

		/// <summary>
		/// Gets or sets the cell where the top-left corner of the shape resides.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This anchor cell, along with the <see cref="BottomRightCornerCell"/>, determines where the shape will be
		/// positioned on the worksheet.  In addition, the <see cref="BottomRightCornerPosition"/> and 
		/// <see cref="TopLeftCornerPosition"/> properties allow for finer control of the shape's position.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The value assigned is a cell whose worksheet is not the same as this shape's worksheet.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null and this shape already exists on a worksheet or group.
		/// </exception>
		/// <value>The cell where the top-left corner of the shape resides.</value>
		/// <seealso cref="BottomRightCornerCell"/>
		/// <seealso cref="BottomRightCornerPosition"/>
		/// <seealso cref="TopLeftCornerPosition"/>
		public WorksheetCell TopLeftCornerCell
		{
			// MD 3/27/12 - 12.1 - Table Support
			//get { return this.topLeftCornerCell; }
			//set
			//{
			//    if ( this.topLeftCornerCell != value )
			//    {
			//        this.VerifyAnchorCell( value );

			//        WorksheetCell oldTopLeftCornerCell = this.topLeftCornerCell;

			//        this.topLeftCornerCell = value;

			//        this.OnBoundsChanged(
			//            oldTopLeftCornerCell,
			//            this.topLeftCornerPosition,
			//            this.bottomRightCornerCell,
			//            this.bottomRightCornerPosition );
			//    }
			//}
			get
			{
				if (this.topLeftCornerCellWorksheet == null ||
					this.topLeftCornerCell == WorksheetCellAddress.InvalidReference)
					return null;

				return this.topLeftCornerCellWorksheet.Rows[this.topLeftCornerCell.RowIndex].Cells[this.topLeftCornerCell.ColumnIndex];
			}
			set
			{
				Worksheet newAnchorCellWorksheet;
				WorksheetCellAddress newAnchorCellAddress;
				if (value == null || value.Worksheet == null)
				{
					newAnchorCellWorksheet = null;
					newAnchorCellAddress = WorksheetCellAddress.InvalidReference;
				}
				else
				{
					newAnchorCellWorksheet = value.Worksheet;
					newAnchorCellAddress = value.Address;
				}

				if (newAnchorCellWorksheet != this.topLeftCornerCellWorksheet ||
					newAnchorCellAddress != this.topLeftCornerCell)
				{
					this.VerifyAnchorCell(value);

					WorksheetCellAddress oldTopLeftCornerCell = this.topLeftCornerCell;

					this.topLeftCornerCellWorksheet = newAnchorCellWorksheet;
					this.topLeftCornerCell = newAnchorCellAddress;

					this.OnBoundsChanged(
						newAnchorCellWorksheet,
						oldTopLeftCornerCell,
						this.topLeftCornerPosition,
						this.bottomRightCornerCell,
						this.bottomRightCornerPosition);
				}
			}
		}

		#endregion TopLeftCornerCell

		#region TopLeftCornerPosition

		/// <summary>
		/// Gets or sets the position in the <see cref="TopLeftCornerCell"/> of the shape's top-left corner,
		/// expressed in percentages.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// These percentages are expressed as distance across the associated dimension of the cell, starting at the 
		/// top-left corner of the cell.  For example, (0.0, 0.0) represents the top-left corner of the cell, whereas
		/// (100.0, 100.0) represents the bottom-right corner of the cell.  (50.0, 10.0) would represent the location 
		/// in the cell which is centered horizontally, and a tenth of the way down from the top.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Either coordinate of the value assigned is outside the range of 0.0 to 100.0.
		/// </exception>
		/// <value>The position in the top-left corner cell of the shape's top-left corner.</value>
		/// <seealso cref="BottomRightCornerCell"/>
		/// <seealso cref="BottomRightCornerPosition"/>
		/// <seealso cref="TopLeftCornerCell"/>
		public PointF TopLeftCornerPosition
		{
			get { return this.topLeftCornerPosition; }
			set
			{
				if ( this.topLeftCornerPosition != value )
				{
					WorksheetShape.VerifyAnchorPosition( value );

					PointF oldTopLeftCornerPosition = this.topLeftCornerPosition;

					this.topLeftCornerPosition = value;

					this.OnBoundsChanged(
						// MD 3/27/12 - 12.1 - Table Support
						this.topLeftCornerCellWorksheet,
						this.topLeftCornerCell,
						oldTopLeftCornerPosition,
						this.bottomRightCornerCell,
						this.bottomRightCornerPosition );
				}
			}
		}

		#endregion TopLeftCornerPosition

		// MD 9/2/08 - Cell Comments
		#region Visible

		/// <summary>
		/// Gets or sets the value indicating whether the shape is visible on the worksheet.
		/// </summary>
		/// <value>The value indicating whether the shape is visible on the worksheet.</value>
		public bool Visible
		{
			get { return this.visible; }
			set { this.visible = value; }
		}

		#endregion Visible

		#region Worksheet

		/// <summary>
		/// Gets the worksheet on which the shape resides.
		/// </summary>
		/// <value>The worksheet on which the shape resides.</value>
		/// // MD 9/2/08 - Cell Comments
		//public Worksheet Worksheet
		public virtual Worksheet Worksheet
		{
			get 
			{
				if ( this.collection == null )
					return null;

				return this.collection.Worksheet; 
			}
		}

		#endregion Worksheet

		#endregion Public Properties

		#region Internal Properties

		// MD 3/27/12 - 12.1 - Table Support
		#region BottomRightCornerCellInternal

		internal WorksheetCellAddress BottomRightCornerCellInternal
		{
			get { return this.bottomRightCornerCell; }
		}

		#endregion BottomRightCornerCellInternal

		// MD 6/14/07 - BR23880
		// We need to store the callout on the shape
		#region CalloutRule

		internal CalloutRule CalloutRule
		{
			get { return this.calloutRule; }
			set { this.calloutRule = value; }
		}

		#endregion CalloutRule

		// MD 7/20/2007 - BR25039
		#region CanBeAddedToShapesCollection

		internal virtual bool CanBeAddedToShapesCollection
		{
			get { return true; }
		}

		#endregion CanBeAddedToShapesCollection

		// MD 8/23/11 - TFS84306
		#region CanHaveFill

		internal virtual bool CanHaveFill
		{
			get { return true; }
		}

		#endregion  // CanHaveFill

		// MD 8/23/11 - TFS84306
		#region CanHaveOutline

		internal virtual bool CanHaveOutline
		{
			get { return true; }
		}

		#endregion  // CanHaveOutline

		#region DrawingProperties1

		// MD 9/2/08 - Cell Comments
		// Note sure why these were virtual, but it is unnecessary
		//internal virtual List<PropertyTableBase.PropertyValue> DrawingProperties1
		internal List<PropertyTableBase.PropertyValue> DrawingProperties1
		{
			get { return this.drawingProperties1; }
			// MD 9/2/08 - Cell Comments
			//set { this.drawingProperties1 = value; }
			set 
			{
				if ( this.drawingProperties1 == value )
					return;

				this.drawingProperties1 = value;
				this.OnDrawingProperties1Changed();
			}
		}

		internal bool HasDrawingProperties1
		{
			get
			{
				return
					this.drawingProperties1 != null &&
					this.drawingProperties1.Count > 0;
			}
		}

		#endregion DrawingProperties1

		#region DrawingProperties2

		// MD 9/2/08 - Cell Comments
		// Note sure why these were virtual, but it is unnecessary
		//internal virtual List<PropertyTableBase.PropertyValue> DrawingProperties2
		internal List<PropertyTableBase.PropertyValue> DrawingProperties2
		{
			get { return this.drawingProperties2; }
			set { this.drawingProperties2 = value; }
		}

		internal bool HasDrawingProperties2
		{
			get
			{
				return
					this.drawingProperties2 != null &&
					this.drawingProperties2.Count > 0;
			}
		}

		#endregion DrawingProperties2

		#region DrawingProperties3

		// MD 9/2/08 - Cell Comments
		// Note sure why these were virtual, but it is unnecessary
		//internal virtual List<PropertyTableBase.PropertyValue> DrawingProperties3
		internal List<PropertyTableBase.PropertyValue> DrawingProperties3
		{
			get { return this.drawingProperties3; }
			set { this.drawingProperties3 = value; }
		}

		internal bool HasDrawingProperties3
		{
			get
			{
				return
					this.drawingProperties3 != null &&
					this.drawingProperties3.Count > 0;
			}
		} 

		#endregion DrawingProperties3

		#region Collection

		internal WorksheetShapeCollection Collection
		{
			get { return this.collection; }
		}

		#endregion Collection

		// MD 7/14/11 - Shape support
		#region IsConnector

		internal virtual bool IsConnector
		{
			get { return false; }
		}

		#endregion // IsConnector

		#region IsTopMost

		internal virtual bool IsTopMost
		{
			get { return this.collection.IsTopMost; }
		}

		#endregion IsTopMost

		// MD 10/30/11 - TFS90733
		#region Removed

		//#region ObjRecords

		//internal List<OBJRecordBase> ObjRecords
		//{
		//    get { return this.objRecords; }
		//    set { this.objRecords = value; }
		//}

		//#endregion ObjRecords

		#endregion // Removed

		#region Obj

		internal Obj Obj
		{
			get { return this.obj; }
			set { this.obj = value; }
		}

		#endregion Obj

		#region Owner

		internal IWorksheetShapeOwner Owner
		{
			get
			{
				if ( this.collection == null )
					return null;

				return this.collection.Owner;
			}
		}

		#endregion Owner

		// MD 7/15/11 - Shape support
		// This is now public, so it is redinfed above.
		#region Removed

		//// MD 9/2/08 - Cell Comments
		//#region Rotation
		//
		//internal ushort Rotation
		//{
		//    get { return this.rotation; }
		//    set { this.rotation = value; }
		//}
		//
		//#endregion Rotation

		#endregion // Removed

		#region ShapeId

		internal virtual uint ShapeId
		{
			get { return this.shapeId; }

			// MD 6/14/07 - BR23880
			// The new shape id will need to be set on any unknown record which points back to the shape it applied to.
			//set { this.shapeId = value; }
			set 
			{
				if ( this.shapeId != value )
				{
					this.shapeId = value;

					if ( this.calloutRule != null )
						this.calloutRule.ShapeId = value;
				}
			}
		}

		#endregion ShapeId

		// MD 10/10/11 - TFS81451
		#region ShapeProperties2007Element

		internal string ShapeProperties2007Element
		{
			get { return this.shapeProperties2007Element; }
			set { this.shapeProperties2007Element = value; }
		}

		#endregion  // ShapeProperties2007Element

		// MD 3/27/12 - 12.1 - Table Support
		#region TopLeftCornerCellInternal

		internal WorksheetCellAddress TopLeftCornerCellInternal
		{
			get { return this.topLeftCornerCell; }
		}

		#endregion TopLeftCornerCellInternal

		// MD 9/2/08 - Cell Comments
		// The round-trip data no longer needs to be stored
		//#region TxoData
		//
		//internal byte[] TxoData
		//{
		//    get { return this.txoData; }
		//    set { this.txoData = value; }
		//}
		//
		//// MD 7/20/2007 - BR25039
		//// Made virtual
		////internal bool HasTxoData
		//internal virtual bool HasTxoData
		//{
		//    get
		//    {
		//        return
		//            this.txoData != null &&
		//            this.txoData.Length > 0;
		//    }
		//}
		//
		//#endregion TxoData

		// MD 9/2/08 - Cell Comments
		#region TxoOptionFlags

		internal ushort TxoOptionFlags
		{
			get { return this.txoOptionFlags; }
			set { this.txoOptionFlags = value; }
		}

		#endregion TxoOptionFlags

		// MD 7/24/12 - TFS115693
		#region TxoRotation

		internal ushort TxoRotation
		{
			get { return this.txoRotation; }
			set { this.txoRotation = value; }
		}

		#endregion TxoRotation

		// MD 7/20/2007 - BR25039
		#region Type

		// MD 10/10/11 - TFS90805
		//internal abstract ShapeType Type { get;}
		internal abstract ShapeType? Type2003 { get; }
		internal abstract ST_ShapeType? Type2007 { get; }

		#endregion Type

		// MD 10/10/11 - TFS81451
		#region UseCxnShapePropertiesElement

		internal bool UseCxnShapePropertiesElement
		{
			get
			{
				if (this.IsConnector == false)
					return false;

				if (this.shapeProperties2007Element != null &&
					this.shapeProperties2007Element != CxnSpElement.QualifiedName)
				{
					return false;
				}

				return true;
			}
		}

		#endregion  // UseCxnShapePropertiesElement

        //  BF 8/20/08  Excel2007 Format
        #region Excel2007SerializationManager





        internal WorksheetShapeSerializationManager Excel2007ShapeSerializationManager
        {
            get
            {
                if ( this.excel2007ShapeSerializationManager == null )
                    this.excel2007ShapeSerializationManager = new WorksheetShapeSerializationManager();

                return this.excel2007ShapeSerializationManager;
            }
        }

		internal bool HasExcel2007ShapeSerializationManager { get { return this.excel2007ShapeSerializationManager != null && this.excel2007ShapeSerializationManager.Elements.Count > 0; } }
        #endregion Excel2007SerializationManager

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