using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using System.Drawing;
using System.Drawing.Imaging;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
    #region WorksheetShapeSerializationManager class






    internal class WorksheetShapeSerializationManager
    {
        #region Member variables

        private CellAnchor cellAnchor = null;
        private Transform transform = null;
        private ClientData clientData = null;
        private List<ElementDataCache> elements = new List<ElementDataCache>();
        private ShapeAttributes shapeAttributes = null;

		// MD 7/18/11 - Shape support
		// This is no longer needed since we fully support the elements that write out the shape bounds.
        //private Rectangle cachedBoundsInTwips = new Rectangle();

		// MD 4/4/12 - TFS108273
		// Marked this with the ThreadStatic attribute. Any code which needs to get the cached WorksheetShapesSaveHelper
		// will be running on the same thread which added the helper and we don't need to cache anything across threads.
		[ThreadStatic]
        static private Dictionary<Worksheet, WorksheetShapesSaveHelper> saveHelpers = null;

        private ElementDataCache txBodyElementWrapper = null;

        #endregion Member variables

        #region Elements
        /// <summary>
        /// Represents the immediate children of the 'sp', 'pic' and 'grpSp' DrawingML elements.
        /// </summary>
        public List<ElementDataCache> Elements { get { return this.elements; } }
        #endregion Elements

        #region CellAnchor
        /// <summary>
        /// Wraps the element and attributes values of the 'twoCellAnchor'
        /// DrawingML element and its descendants ('to', 'from', 'off', 'ext', etc.)
        /// </summary>
        public CellAnchor CellAnchor
        {
            get
            { 
                if ( this.cellAnchor == null )
                    this.cellAnchor = new CellAnchor();

                return this.cellAnchor;
            }
            set { this.cellAnchor = value; }
        }
        #endregion ClientData

        #region ClientData
        /// <summary>
        /// Wraps the element and attributes values of the 'clientData'
        /// DrawingML element.
        /// </summary>
        public ClientData ClientData
        {
            get
            { 
                if ( this.clientData == null )
                    this.clientData = new ClientData();

                return this.clientData;
            }
            set { this.clientData = value; }
        }
        #endregion ClientData

        #region Transform
        /// <summary>
        /// Wraps the element and attributes values of the 'xfrm'
        /// DrawingML element and its descendants.
        /// </summary>
        public Transform Transform
        {
            get
            { 
                if ( this.transform == null )
                    this.transform = new Transform();

                return this.transform;
            }

            set { this.transform = value; }
        }
        #endregion Transform

        #region ShapeAttributes
        /// <summary>
        /// Wraps the attribute values of the 'sp' DrawingML element.
        /// </summary>
        public ShapeAttributes ShapeAttributes
        {
            get { return this.shapeAttributes; }
        }
        #endregion ShapeAttributes

		// MD 7/15/11 - Shape support
		#region OnAfterShapeLoaded

		public static void OnAfterShapeLoaded(ContextStack contextStack)
		{
			WorksheetShape shape = (WorksheetShape)contextStack[typeof(WorksheetShape)];
			int level = shape is WorksheetShapeGroup ? 1 : 0;

			//  Get a WorksheetShapeCollection off the context stack, if one is there.
			//  If there is one, add the newly created shape to it; otherwise, add it to
			//  the WorksheetShapesHolder list, which represents what will become the
			//  Worksheet's root Shapes collection.
			ICollection<WorksheetShape> containingList = null;
			WorksheetShapeCollection shapesCollection = contextStack[typeof(WorksheetShapeCollection), level] as WorksheetShapeCollection;
			if (shapesCollection != null)
			{
				containingList = shapesCollection;
			}
			else
			{
				WorksheetShapesHolder holder = contextStack[typeof(WorksheetShapesHolder)] as WorksheetShapesHolder;
				containingList = holder.Shapes;
			}

			if (containingList != null)
				containingList.Add(shape);
			else
				Utilities.DebugFail("Could not get a list to add the shape to in WorksheetShapeSerializationManager.OnShapeCreated - unexpected.");

			// MD 3/12/12 - TFS102234
			// Moved the clearing logic for fills and outlines here because we may need them after the main element for the fill 
			// or outline is loaded.
			ISolidColorItem solidColorItem = shape.Fill as ISolidColorItem;
			if (solidColorItem != null && Utilities.ColorIsEmpty(solidColorItem.Color))
				shape.Fill = null;

			solidColorItem = shape.Outline as ISolidColorItem;
			if (solidColorItem != null && Utilities.ColorIsEmpty(solidColorItem.Color))
				shape.Outline = null;
		}

		#endregion // OnAfterShapeLoaded

        #region OnShapeCreated
        /// <summary>
        /// Called when the corresponding DrawingML element
        /// ('sp', 'pic', or 'grpSp') is loaded for the specified shape.
        /// </summary>
        /// <param name="shape">The WorksheetShape-derived instance that was created.</param>
        /// <param name="contextStack">The ContextStack instance onto which the shape will be pushed.</param>
        public static void OnShapeCreated( WorksheetShape shape, ContextStack contextStack )
        {
            WorksheetShapeSerializationManager.OnShapeCreated( shape, contextStack, null );
        }

        /// <summary>
        /// Called when the corresponding DrawingML element
        /// ('sp', 'pic', or 'grpSp') is loaded for the specified shape.
        /// </summary>
        /// <param name="shape">The WorksheetShape-derived instance that was created.</param>
        /// <param name="contextStack">The ContextStack instance onto which the shape will be pushed.</param>
        /// <param name="shapeAttributes">A ShapeAttributes instance which contains the attributes specific to the 'sp' element, which corresponds to our UnknownShape class.</param>
        public static void OnShapeCreated( WorksheetShape shape, ContextStack contextStack, ShapeAttributes shapeAttributes )
        {
            if ( shape == null || contextStack == null )
            {
                Utilities.DebugFail( "Null parameter in WorksheetShapeSerializationManager.OnShapeCreated - unexpected." );
                return;
            }

            //  Assign the shape attributes
            shape.Excel2007ShapeSerializationManager.shapeAttributes = shapeAttributes;

            //  Get the associated CellAnchor and hook it up, if it has
            //  not yet been assigned. Also, since it will remain on the context stack,
            //  mark it as having been assigned so we don't mistakenly assign it to a
            //  descendant shape. We have to do this because the CellAnchor instance
            //  will remain on the context stack until this shape element has been fully
            //  processed, i.e., during the time any descendants are processed.
            CellAnchor cellAnchor = contextStack[ typeof(CellAnchor) ] as CellAnchor;
            if ( cellAnchor != null && cellAnchor.HasBeenAssigned == false )
            {
                shape.Excel2007ShapeSerializationManager.cellAnchor = cellAnchor;
                cellAnchor.HasBeenAssigned = true;
            }

            //  Get the associated Transform.
            Transform transformInfo = shape.Excel2007ShapeSerializationManager.Transform;

            //  Push the newly created shape onto the context stack. This is
            //  necessary so that we know what the "current" shape is; subsequent
            //  elements/attributes that are processed are implied to be associated
            //  with this instance.
            contextStack.Push( shape );

			// MD 7/15/11 - Shape support
			// Moved this code to OnAfterShapeLoaded because the shape instance might be swapped out for a known shape.
			////  Get a WorksheetShapeCollection off the context stack, if one is there.
			////  If there is one, add the newly created shape to it; otherwise, add it to
			////  the WorksheetShapesHolder list, which represents what will become the
			////  Worksheet's root Shapes collection.
			//ICollection<WorksheetShape> containingList = null;
			//WorksheetShapeCollection shapesCollection = contextStack[ typeof(WorksheetShapeCollection) ] as WorksheetShapeCollection;
			//if ( shapesCollection != null )
			//    containingList = shapesCollection;
			//else
			//{
			//    WorksheetShapesHolder holder = contextStack[ typeof(WorksheetShapesHolder) ] as WorksheetShapesHolder;
			//    containingList = holder.Shapes;
			//}
			//
			//if ( containingList != null )
			//    containingList.Add( shape );
			//else
			//    Utilities.DebugFail( "Could not get a list to add the shape to in WorksheetShapeSerializationManager.OnShapeCreated - unexpected." );

            //  See if the shape is a group; if it is, push its Shapes
            //  collection onto the stack, so that descendants of this
            //  group will be added to its Shapes collection.
            WorksheetShapeGroup group = shape as WorksheetShapeGroup;
            if ( group != null )
                contextStack.Push( group.Shapes );
        }
        #endregion OnShapeCreated

		// MD 7/15/11 - Shape support
		// This is no longer needed.
		#region Removed

		//#region ShapeTypeFromName
		//public static ShapeType ShapeTypeFromName( string name )
		//{
		//    switch ( name )
		//    {
		//        // MD 7/15/11 - Shape support
		//        case "line":
		//            return ShapeType.Line;

		//        case "ellipse":
		//            return ShapeType.Ellipse;

		//        case "rect":
		//            return ShapeType.Rectangle;

		//        case "roundRect":
		//            return ShapeType.RoundRectangle;

		//        case "triangle":
		//            return ShapeType.IsocelesTriangle;

		//        case "diamond":
		//            return ShapeType.Diamond;

		//        case "straightConnector1":
		//            return ShapeType.StraightConnector1;

		//        case "bentConnector2":
		//            return ShapeType.BentConnector2;

		//        case "bentConnector3":
		//            return ShapeType.BentConnector3;

		//        case "bentConnector4":
		//            return ShapeType.BentConnector4;

		//        case "bentConnector5":
		//            return ShapeType.BentConnector5;

		//        case "curvedConnector2":
		//            return ShapeType.CurvedConnector2;

		//        case "curvedConnector3":
		//            return ShapeType.CurvedConnector3;

		//        case "curvedConnector4":
		//            return ShapeType.CurvedConnector4;

		//        case "curvedConnector5":
		//            return ShapeType.CurvedConnector5;

		//        case "flowChartMagneticDisk":
		//            return ShapeType.FlowChartMagneticDrum;
		//    }

		//    return ShapeType.NotPrimitive;
		//}
		//#endregion ShapeTypeFromName 

		#endregion // Removed

        #region LoadChildElements
		static public void LoadChildElements( Excel2007WorkbookSerializationManager manager, ExcelXmlNode node, ref bool isReaderOnNextNode )
		{
			WorksheetShape currentShape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			if ( currentShape == null )
			{
				Utilities.DebugFail( "Must have a WorksheetShape on the context stack here." );
				return;
			}

			WorksheetShapesHolder shapesHolder = (WorksheetShapesHolder)manager.ContextStack[ typeof( WorksheetShapesHolder ) ];

			if ( shapesHolder == null )
			{
				Utilities.DebugFail( "Coult not find the shapes holder on the context stack." );
				return;
			}

			XmlElementBase.BeforeLoadElementCallback beforeLoadElementHandler = delegate(
				XmlElementBase elementHandler, ElementDataCache parentElementCache, ref List<ElementDataCache> parentElementCacheCollection )
				{
					if ( WorksheetShapeSerializationManager.IsRootShapeElement( elementHandler ) )
					{
						// The parent cache collection should be null so the child elements of the new shape are not cached with the current shape.
						parentElementCacheCollection = null;
						return;
					}

					//  If we are working with the same shape as we were before getting
					//  the element handler, cache its element and attribute values. If
					//  we just hit a new (nested) shape, we skip this and pass
					//  true as the value for the 'cacheRootElementValues' parameter
					//  when we call this method recursively (see below).
					//
					//  Note that the CacheElementValues method returns the ElementWrapper it creates,
					//  which is the wrapper for the childElement; we pass this wrapper in when we call
					//  this method recursively (see below).
					currentShape = manager.ContextStack[ typeof( WorksheetShape ) ] as WorksheetShape;
					shapesHolder.CurrentSerializationManager = currentShape.Excel2007ShapeSerializationManager;
					parentElementCacheCollection = parentElementCache != null ? parentElementCache.Elements : shapesHolder.CurrentSerializationManager.Elements;
				};

			XmlElementBase.AfterLoadElementCallback afterLoadElementHandler = delegate( XmlElementBase elementHandler )
				{
					if ( WorksheetShapeSerializationManager.IsRootShapeElement( elementHandler ) )
					{
						//  Restore the CurrentSerializationManager because it was overwritten by the child shape element.
						shapesHolder.CurrentSerializationManager = currentShape.Excel2007ShapeSerializationManager;
					}
				};

			XmlElementBase.LoadChildElements( manager, node, beforeLoadElementHandler, afterLoadElementHandler, ref isReaderOnNextNode );
		}
        #endregion LoadChildElements

		#region IsRootShapeElement

		private static bool IsRootShapeElement( XmlElementBase elementHandler )
		{
			return elementHandler is PicElement ||
				elementHandler is GrpSpElement ||
				elementHandler is SpElement ||
				// MD 7/15/11 - Shape support
				elementHandler is CxnSpElement;
		}

		#endregion IsRootShapeElement

		#region SaveTextProperty
		static public bool SaveTextProperty(
            Excel2007WorkbookSerializationManager manager,
            WorksheetShapeWithText shape,
            ElementDataCache txtBodyWrapper )
        {
            //  If the Text property has no value, return so that no
            //  'txBody' element is written out.
            if ( shape.HasText == false )
                return false;

            WorksheetShapeSerializationManager serializationManager = shape.Excel2007ShapeSerializationManager;

            //  Declare the singular, required elements that we know we will be adding
            ElementDataCache bodyPrWrapper = new ElementDataCache( string.Format("{0}/{1}", DrawingsPart.MainNamespace, "bodyPr") );
            ElementDataCache lstStyleWrapper = new ElementDataCache( string.Format("{0}/{1}", DrawingsPart.MainNamespace, "lstStyle") );

			// MD 11/8/11 - TFS85193
			// We may need to create multiple instances of these wrappers, so don't create them here.
			//ElementDataCache pWrapper = new ElementDataCache(string.Format("{0}/{1}", DrawingsPart.MainNamespace, "p"));
			//ElementDataCache pPrWrapper = new ElementDataCache(string.Format("{0}/{1}", DrawingsPart.MainNamespace, "pPr"));

            //  Declare the names of the elements that we will be adding more than one of
            string solidFillName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "solidFill");
            string srgbClrName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "srgbClr");
            string tName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "t");

			// MD 8/23/11 - TFS84306
			string latinName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "latin");

            #region bodyPr

			// MD 7/5/12 - TFS115687
			// The rtlCol attribute value will now be initialized in the RoundTrip2007Properties collection.
			////  Add the 'rtlCol' attribute, with a value of 0
			//bodyPrWrapper.AttributeValues.Add( "rtlCol", XmlElementBase.GetXmlString(0, DataType.Integer) );

            //  Add the 'anchor' attribute, with a value of "ctr"
			// MD 11/8/11 - TFS85193
			// We now fully support this attribute.
            //bodyPrWrapper.AttributeValues.Add( "anchor", "ctr" );
			bodyPrWrapper.AttributeValues.Add(
				BodyPrElement.AnchorAttributeName, 
				XmlElementBase.GetXmlString(shape.Text.VerticalAlignment, DataType.ST_TextAnchoringType));

			// MD 7/5/12 - TFS115687
			// Add in all loaded property values so we don't lose any information when round-tripping 2007 files.
			foreach (KeyValuePair<string, string> roundTripProp in shape.Text.RoundTrip2007Properties)
				bodyPrWrapper.AttributeValues.Add(roundTripProp.Key, roundTripProp.Value);

            txtBodyWrapper.Elements.Add( bodyPrWrapper );
            
            #endregion bodyPr

            #region lstStyle

            txtBodyWrapper.Elements.Add( lstStyleWrapper );
            
            #endregion lstStyle

			// MD 11/8/11 - TFS85193
			// We need to write out multiple paragraphs instead of just one.
			#region Old Code

			//#region p/pPr

			//txtBodyWrapper.Elements.Add( pWrapper );

			////  Add a 'pPr' element, always center aligned since we
			////  don't support paragraph alignment, and that is the default
			//pPrWrapper.AttributeValues.Add( "algn", "ctr" );
			//pWrapper.Elements.Add( pPrWrapper );

			//#endregion p/pPr

			//#region FormattingRuns

			////  Iterate the FormattingRuns list and add the appropriate
			////  elements for each run in the list.
			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////FormattedString text = shape.Text;
			//// MD 4/12/11 - TFS67084
			//// Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
			////FormattedStringElement text = shape.Text.Proxy.Element;
			//FormattedStringElement text = shape.Text.Element;

			//// MD 11/3/10 - TFS49093
			//// Wrapped in an if statement so we don't lazily create the collection when it is null.
			//// MD 7/18/11 - Shape support
			//// Shapes need at least one formatting run.
			////if (text.HasFormatting)
			////{
			//if (text.HasFormatting == false)
			//    text.FormattingRuns.Add(new FormattedStringRun(text, 0, manager.Workbook));

			//foreach ( FormattedStringRun run in text.FormattingRuns )
			//{
			//    //  Create the wrapper for the 'r' element
			//    ElementDataCache rWrapper = new ElementDataCache( string.Format("{0}/{1}", DrawingsPart.MainNamespace, "r") );

			//    //  Create the wrapper for the 'rPr' element
			//    ElementDataCache rPrWrapper = new ElementDataCache( RPrElement.QualifiedName );

			//    //  Process the Font properties...
			//    WorkbookFontProxy font = run.HasFont ? run.Font : null;
			//    if ( font != null )
			//    {
			//        if ( font.Height > 0 )
			//        {
			//            //  Convert the value of the Height property, which is expressed in twips,
			//            //  to hundredths of a point.
			//            int fontHeightInPoints100 = (font.Height / Utilities.TwipsPerPoint) * 100;
			//            rPrWrapper.AttributeValues.Add( RPrElement.SzAttributeName, XmlElementBase.GetXmlString(fontHeightInPoints100, DataType.Integer) );
			//        }

			//        //  Only write out 'b' (bold) if it is set to true.
			//        if ( font.Bold == ExcelDefaultableBoolean.True )
			//            rPrWrapper.AttributeValues.Add( RPrElement.BAttributeName, XmlElementBase.GetXmlString(true, DataType.Boolean) );

			//        //  Only write out 'i' (italic) if it is set to true.
			//        if ( font.Italic == ExcelDefaultableBoolean.True )
			//            rPrWrapper.AttributeValues.Add( RPrElement.IAttributeName, XmlElementBase.GetXmlString(true, DataType.Boolean) );

			//        //  FontUnderlineStyle
			//        FontUnderlineStyle fontUnderlineStyle = font.UnderlineStyle;
			//        if ( fontUnderlineStyle != FontUnderlineStyle.Default )
			//        {
			//            string textUnderline = Utilities.ToTextUnderlineType( fontUnderlineStyle );
			//            rPrWrapper.AttributeValues.Add( RPrElement.UAttributeName, textUnderline );
			//        }

			//        //  Strikeout
			//        if ( font.Strikeout == ExcelDefaultableBoolean.True )
			//            rPrWrapper.AttributeValues.Add( RPrElement.StrikeAttributeName, Utilities.ToTextStrikeType(ExcelDefaultableBoolean.True) );

			//        //  Color
			//        if ( Utilities.ColorIsEmpty(font.Color) == false )
			//        {
			//            // MD 8/23/11 - TFS84306
			//            // This logic and more is now done in the SerializeColor mehtod, so use that instance.
			//            ////  Add a 'solidFill' element
			//            //ElementDataCache solidFillWrapper = new ElementDataCache( solidFillName );
			//            //rPrWrapper.Elements.Add( solidFillWrapper );
			//            //
			//            ////  Add a 'srgbClr' element to the solidFill element
			//            //ElementDataCache srgbClrWrapper = new ElementDataCache( SrgbClrElement.QualifiedName );
			//            //srgbClrWrapper.AttributeValues.Add( SrgbClrElement.ValAttributeName, Utilities.ToHexBinary3(font.Color) );
			//            //solidFillWrapper.Elements.Add( srgbClrWrapper );
			//            WorksheetShapeSerializationManager.SerializeSolidFill(rPrWrapper, font.Color);
			//        }

			//        // MD 8/23/11 - TFS84306
			//        // We need to save out the font name.
			//        if (String.IsNullOrEmpty(font.Name) == false)
			//        {
			//            // Add a 'rPr/latin' element
			//            ElementDataCache latin_Element = new ElementDataCache(latinName);
			//            latin_Element.AttributeValues.Add("typeface", font.Name);
			//            rPrWrapper.Elements.Add(latin_Element);
			//        }
			//    }

			//    //  Make the 'rPr' wrapper a child of the 'r' wrapper
			//    rWrapper.Elements.Add( rPrWrapper );

			//    //  Add the 't' element
			//    int startIndex = run.FirstFormattedChar;
			//    ElementDataCache tWrapper = new ElementDataCache( TElement.QualifiedName, run.UnformattedString );

			//    //  Make the 't' wrapper a child of the 'r' wrapper
			//    rWrapper.Elements.Add( tWrapper );

			//    //  Make the 'r' wrapper a child of the 'p' wrapper
			//    pWrapper.Elements.Add( rWrapper );
			//}
			////}

			//#endregion FormattingRuns 

			#endregion // Old Code
			foreach (FormattedTextParagraph paragraph in shape.Text.Paragraphs)
			{
				ElementDataCache pWrapper = new ElementDataCache(string.Format("{0}/{1}", DrawingsPart.MainNamespace, "p"));
				ElementDataCache pPrWrapper = new ElementDataCache(string.Format("{0}/{1}", DrawingsPart.MainNamespace, "pPr"));

				#region p/pPr

				txtBodyWrapper.Elements.Add(pWrapper);

				pPrWrapper.AttributeValues.Add(
					PPrElement.AlgnAttributeName, 
					XmlElementBase.GetXmlString(paragraph.Alignment, DataType.ST_TextAlignType));

				pWrapper.Elements.Add(pPrWrapper);

				#endregion p/pPr

				#region FormattingRuns

				//  Iterate the FormattingRuns list and add the appropriate
				//  elements for each run in the list.
				// MD 7/23/12 - TFS117429
				//foreach (FormattingRunBase run in paragraph.GetFormattingRuns(manager.Workbook))
				foreach (FormattedTextRun run in paragraph.GetFormattingRuns(manager.Workbook))
				{
					//  Create the wrapper for the 'r' element
					ElementDataCache rWrapper = new ElementDataCache(string.Format("{0}/{1}", DrawingsPart.MainNamespace, "r"));

					//  Create the wrapper for the 'rPr' element
					ElementDataCache rPrWrapper = new ElementDataCache(RPrElement.QualifiedName);

					//  Process the Font properties...
					WorkbookFontProxy font = run.HasFont ? run.GetFontInternal(manager.Workbook) : null;
					if (font != null)
					{
						if (font.Height > 0)
						{
							//  Convert the value of the Height property, which is expressed in twips,
							//  to hundredths of a point.
							int fontHeightInPoints100 = (font.Height / Utilities.TwipsPerPoint) * 100;
							rPrWrapper.AttributeValues.Add(RPrElement.SzAttributeName, XmlElementBase.GetXmlString(fontHeightInPoints100, DataType.Integer));
						}

						//  Only write out 'b' (bold) if it is set to true.
						if (font.Bold == ExcelDefaultableBoolean.True)
							rPrWrapper.AttributeValues.Add(RPrElement.BAttributeName, XmlElementBase.GetXmlString(true, DataType.Boolean));

						//  Only write out 'i' (italic) if it is set to true.
						if (font.Italic == ExcelDefaultableBoolean.True)
							rPrWrapper.AttributeValues.Add(RPrElement.IAttributeName, XmlElementBase.GetXmlString(true, DataType.Boolean));

						//  FontUnderlineStyle
						FontUnderlineStyle fontUnderlineStyle = font.UnderlineStyle;
						if (fontUnderlineStyle != FontUnderlineStyle.Default)
						{
							string textUnderline = Utilities.ToTextUnderlineType(fontUnderlineStyle);
							rPrWrapper.AttributeValues.Add(RPrElement.UAttributeName, textUnderline);
						}

						//  Strikeout
						if (font.Strikeout == ExcelDefaultableBoolean.True)
							rPrWrapper.AttributeValues.Add(RPrElement.StrikeAttributeName, Utilities.ToTextStrikeType(ExcelDefaultableBoolean.True));

						// MD 11/8/11 - TFS85193
						// This property was never being supported before.
						// SuperscriptSubscriptStyle
						switch (font.SuperscriptSubscriptStyle)
						{
							case FontSuperscriptSubscriptStyle.Subscript:
								rPrWrapper.AttributeValues.Add(RPrElement.BaselineAttributeName, XmlElementBase.GetXmlString(RPrElement.SubscriptBaseline, DataType.ST_Percentage));
								break;

							case FontSuperscriptSubscriptStyle.Superscript:
								rPrWrapper.AttributeValues.Add(RPrElement.BaselineAttributeName, XmlElementBase.GetXmlString(RPrElement.SuperScriptBaseline, DataType.ST_Percentage));
								break;
						}

						//  Color
						// MD 1/17/12 - 12.1 - Cell Format Updates
						//if (Utilities.ColorIsEmpty(font.Color) == false)
						//{
						//    // MD 8/23/11 - TFS84306
						//    // This logic and more is now done in the SerializeColor mehtod, so use that instance.
						//    ////  Add a 'solidFill' element
						//    //ElementDataCache solidFillWrapper = new ElementDataCache( solidFillName );
						//    //rPrWrapper.Elements.Add( solidFillWrapper );
						//    //
						//    ////  Add a 'srgbClr' element to the solidFill element
						//    //ElementDataCache srgbClrWrapper = new ElementDataCache( SrgbClrElement.QualifiedName );
						//    //srgbClrWrapper.AttributeValues.Add( SrgbClrElement.ValAttributeName, Utilities.ToHexBinary3(font.Color) );
						//    //solidFillWrapper.Elements.Add( srgbClrWrapper );
						//    WorksheetShapeSerializationManager.SerializeSolidFill(rPrWrapper, font.Color);
						//}
						if (font.ColorInfo != null)
							WorksheetShapeSerializationManager.SerializeSolidFill(rPrWrapper, font.ColorInfo);

						// MD 8/23/11 - TFS84306
						// We need to save out the font name.
						if (String.IsNullOrEmpty(font.Name) == false)
						{
							// Add a 'rPr/latin' element
							ElementDataCache latin_Element = new ElementDataCache(latinName);
							latin_Element.AttributeValues.Add("typeface", font.Name);
							rPrWrapper.Elements.Add(latin_Element);
						}
					}

					// MD 7/23/12 - TFS117429
					foreach (KeyValuePair<string, string> roundTripProp in run.RoundTrip2007Properties)
						rPrWrapper.AttributeValues.Add(roundTripProp.Key, roundTripProp.Value);

					//  Make the 'rPr' wrapper a child of the 'r' wrapper
					rWrapper.Elements.Add(rPrWrapper);

					//  Add the 't' element
					ElementDataCache tWrapper = new ElementDataCache(TElement.QualifiedName, run.UnformattedString);

					//  Make the 't' wrapper a child of the 'r' wrapper
					rWrapper.Elements.Add(tWrapper);

					//  Make the 'r' wrapper a child of the 'p' wrapper
					pWrapper.Elements.Add(rWrapper);
				}

				#endregion FormattingRuns
			}

            return true;
        }

        #endregion SaveTextProperty

        #region SaveWorksheetShape

        static public void SaveWorksheetShape(
            Excel2007WorkbookSerializationManager manager,
            WorksheetShape shape,
            ExcelXmlElement element )
        {
			// MD 7/15/11 - Shape support
			// For most shapes, this will push a duplicate shape on the stack, because the twoCellAnchor pushes the shape. But when the shape is
			// in a group shape, only the group is wrapped in a twoCellAnchor, so we need to push on each shape.
			manager.ContextStack.Push(shape);

			ElementDataCache.ReplaceConsumedValueCallback replaceConsumedValueCallback = delegate( HandledAttributeIdentifier handledAttributeIdentifier )
				{
					return WorksheetShapeSerializationManager.ValueFromAttributeId( manager, shape, handledAttributeIdentifier );
				};

            WorksheetShapeSerializationManager serializationManager = shape.Excel2007ShapeSerializationManager;
            foreach( ElementDataCache wrapper in serializationManager.elements )
            {
                //  BF 9/9/08
                //  If the Text property was initialized via deserialization,
                //  and was changed via the object model, we will blow away
                //  anything that we don't support, and rebuild the XML based
                //  on the current state of the FormattedString.
                if ( wrapper == serializationManager.txBodyElementWrapper &&
                     serializationManager.txBodyElementWrapper.HasElements == false )
                {
					WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;

					if ( shapeWithText == null )
					{
						Utilities.DebugFail( "The shape should have been a shape with text." );
						continue;
					}

					if ( WorksheetShapeSerializationManager.SaveTextProperty( manager, shapeWithText, wrapper ) == false )
                        continue;
                }

				// MD 7/15/11 - Shape support
				// This method needs to take the serialization manager.
				//wrapper.SaveDataUnderParentElement( element, replaceConsumedValueCallback );
				wrapper.SaveDataUnderParentElement(manager, element, replaceConsumedValueCallback);
            }
        }

        #endregion SaveWorksheetShape

        #region ConsumeShape
        /// <summary>
        /// Pulls a generic list containing WorksheetShape instances off the
        /// ContextStack, removes the first item from that list and returns it.
        /// </summary>
        static public WorksheetShape ConsumeShape( ContextStack contextStack, Type shapeType )
        {
            return WorksheetShapeSerializationManager.GetCurrentShapeHelper( contextStack, shapeType, true );
        }
        #endregion ConsumeShape

        #region PeekCurrentShape
        /// <summary>
        /// Pulls a generic list containing WorksheetShape instances off the
        /// ContextStack, removes the first item from that list and returns it.
        /// </summary>
        static public WorksheetShape PeekCurrentShape( ContextStack contextStack )
        {
            return WorksheetShapeSerializationManager.GetCurrentShapeHelper( contextStack, null, false );
        }
        #endregion PeekCurrentShape

        #region GetCurrentShapeHelper
        /// <summary>
        /// Pulls a generic list containing WorksheetShape instances off the
        /// ContextStack, and returns the first item from that list.
        /// </summary>
        static private WorksheetShape GetCurrentShapeHelper( ContextStack contextStack, Type shapeType, bool remove )
        {
            List<WorksheetShape> shapeList = contextStack[typeof(List<WorksheetShape>)] as List<WorksheetShape>;
            WorksheetShape shape = shapeList != null && shapeList.Count > 0 ? shapeList[0] : null;

			// MD 7/15/11 - Shape support
			// Allow a base shape type to be passed in so we can get any shape of that type or derived types.
            //if ( shape == null || ( shapeType != null && shape.GetType() != shapeType) )
			if (shape == null || (shapeType != null && shapeType.IsAssignableFrom(shape.GetType()) == false))
            {
                Utilities.DebugFail( "Unexpected behavior in ConsumeShape/PeekCurrentShape" );
                return null;
            }
            else
            if ( remove )
                shapeList.Remove( shape );
            
            return shape;
        }
        #endregion GetCurrentShapeHelper

        #region ValueFromAttributeId
        static public string ValueFromAttributeId(
            Excel2007WorkbookSerializationManager manager,
            WorksheetShape shape,
            HandledAttributeIdentifier attributeId )
        {            
            string retVal = string.Empty;

            switch ( attributeId )
            {
                #region CNvPrElement_Id
                // CNvPrElement_Id = WorksheetShape.ShapeId
                case HandledAttributeIdentifier.CNvPrElement_Id:
                {
                    retVal = shape.ShapeId.ToString();
                }
                break;
                #endregion CNvPrElement_Id

                #region CNvPrElement_Hidden
                // CNvPrElement_Hidden = NOT WorksheetShape.Visible
                case HandledAttributeIdentifier.CNvPrElement_Hidden:
                {
                    bool hidden = shape.Visible == false;
                    retVal = XmlElementBase.GetXmlString(hidden, DataType.Boolean);
                }
                break;
                #endregion CNvPrElement_Hidden

				// MD 7/18/11 - Shape support
				// The parent elements of these attributes are now fully handled, so we don't need to use the consumed value logic for these anymore.
				#region Removed

				//#region ExtElement_Cx
				//// ExtElement_Cx = WorksheetShape.GetBoundsInTwips.Width
				//case HandledAttributeIdentifier.ExtElement_Cx:
				//{
				//    Rectangle bounds = shape.Excel2007ShapeSerializationManager.cachedBoundsInTwips;
				//    int width = Utilities.TwipsToEMU( bounds.Width );
				//    retVal = width.ToString();
				//}
				//break;
				//#endregion ExtElement_Cx

				//#region ExtElement_Cy
				//// ExtElement_Cy = WorksheetShape.GetBoundsInTwips.Height
				//case HandledAttributeIdentifier.ExtElement_Cy:
				//{
				//    Rectangle bounds = shape.Excel2007ShapeSerializationManager.cachedBoundsInTwips;
				//    int height = Utilities.TwipsToEMU( bounds.Height );
				//    retVal = height.ToString();
				//}
				//break;
				//#endregion ExtElement_Cy

				//#region ChExtElement_Cx
				//case HandledAttributeIdentifier.ChExtElement_Cx:
				//{
				//    return ValueFromAttributeId(manager, shape, HandledAttributeIdentifier.ExtElement_Cx);
				//}
				//#endregion ChxtElement_Cx

				//#region ChExtElement_Cy
				//case HandledAttributeIdentifier.ChExtElement_Cy:
				//{
				//    return ValueFromAttributeId(manager, shape, HandledAttributeIdentifier.ExtElement_Cy);
				//}
				//#endregion ChxtElement_Cx

				//#region ChOffElement_X
				//case HandledAttributeIdentifier.ChOffElement_X:
				//{
				//    return ValueFromAttributeId(manager, shape, HandledAttributeIdentifier.OffElement_X);
				//}
				//#endregion ChOffElement_X

				//#region ChOffElement_Y
				//case HandledAttributeIdentifier.ChOffElement_Y:
				//{
				//    return ValueFromAttributeId(manager, shape, HandledAttributeIdentifier.OffElement_Y);
				//}
				//#endregion ChOffElement_Y

				//#region OffElement_X
				//// OffElement_X = WorksheetShape.GetBoundsInTwips.Left
				//case HandledAttributeIdentifier.OffElement_X:
				//{
				//    Rectangle bounds = shape.Excel2007ShapeSerializationManager.cachedBoundsInTwips;
				//    int left = Utilities.TwipsToEMU( bounds.Left );
				//    retVal = left.ToString();
				//}
				//break;
				//#endregion OffElement_X

				//#region OffElement_Y
				//// OffElement_Y = WorksheetShape.GetBoundsInTwips.Top
				//case HandledAttributeIdentifier.OffElement_Y:
				//{
				//    Rectangle bounds = shape.Excel2007ShapeSerializationManager.cachedBoundsInTwips;
				//    int top = Utilities.TwipsToEMU( bounds.Top );
				//    retVal = top.ToString();
				//}
				//break;
				//#endregion OffElement_Y

				#endregion  // Removed

                #region BlipElement_Embed
                case HandledAttributeIdentifier.BlipElement_Embed:
                {
                    Dictionary<Image, RelationshipIdHolder> imageRelationshipIds =
                        manager.ContextStack[typeof(Dictionary<Image, RelationshipIdHolder>)]
                        as Dictionary<Image, RelationshipIdHolder>;

                    if ( imageRelationshipIds == null )
                    {
                        Utilities.DebugFail( "Could not get a relationship ID in ValueFromMemberToken('BlipElement_Embed')" );
                        return string.Empty;
                    }

                    WorksheetImage imageShape = shape as WorksheetImage;
                    if ( imageShape == null || imageShape.Image == null )
                    {
                        Utilities.DebugFail( "Specified shape is not a WorksheetImage or the Image is null in ValueFromMemberToken('BlipElement_Embed')" );
                        return string.Empty;
                    }

                    retVal = imageRelationshipIds[imageShape.Image].RelationshipId;
                }
                break;
                #endregion BlipElement_Embed

				// MD 10/12/10 - TFS49853
				#region ChartElement_Id
				case HandledAttributeIdentifier.ChartElement_Id:
				{
					RelationshipIdHolder chartDataRelationshipId = (RelationshipIdHolder)manager.ContextStack[typeof(RelationshipIdHolder)];

					if (chartDataRelationshipId == null)
					{
						Utilities.DebugFail("Could not get a relationship ID in ValueFromMemberToken('ChartElement_Id')");
						return string.Empty;
					}

					return chartDataRelationshipId.RelationshipId;
				}
				#endregion // ChartElement_Id

                default:
                    Utilities.DebugFail( "Unrecognized constant in ValueFromAttributeId" );
                    return string.Empty;
            }

            return retVal;
        }
        #endregion ValueFromAttributeId

        #region OnBeforeShapeSaved
        /// <summary>
        /// Called immediately before the corresponding DrawingML element
        /// is created for the specified shape.
        /// </summary>
        /// <param name="shape">The WorksheetShape-derived instance about to be serialized.</param>
        /// <param name="isRootShape">Specifies whether the shape belongs to the worksheet's Shapes collection.</param>
        static public void OnBeforeShapeSaved( WorksheetShape shape, bool isRootShape )
        {
            WorksheetShapeSerializationManager serializationManager = shape.Excel2007ShapeSerializationManager;
            bool hasSerializedData = serializationManager.elements != null && serializationManager.elements.Count > 0;

			// MD 7/18/11 - Shape support
			// cachedBoundsInTwips was removed.
            //serializationManager.cachedBoundsInTwips = shape.GetBoundsInTwips();

            //  If there is no serialized data, that implies that the shape was created
            //  via our public object model. In this case, create the XML that describes
            //  the image and populate the WorksheetShapeSerializationManager instance
            //  with it.
            if ( hasSerializedData == false )
			{
                serializationManager.FromShape( shape );
			}
			// MD 12/2/11 - TFS96974
			// If the shape already has cached elements, we need to clear out and repopulate the items we fully support (fill and outline).
			
			
			
			else if (
				(shape is WorksheetImage) == false &&
				(shape is WorksheetShapeGroup) == false)
			{
				foreach (ElementDataCache element in serializationManager.elements)
				{
					if (element.ToString() != SpPrElement.QualifiedName)
						continue;

					// MD 7/5/12 - TFS115688
					ElementDataCache extLstElement = null;

					// Clear out all previous fill and outline elements
					for (int i = element.Elements.Count - 1; i >= 0; i--)
					{
						ElementDataCache subElement = element.Elements[i];
						string subElementName = subElement.ToString();
						switch (subElementName)
						{
							case NoFillElement.QualifiedName:
							case SolidFillElement.QualifiedName:
							case LnElement.QualifiedName:
							case "http://schemas.openxmlformats.org/drawingml/2006/main/gradFill":
							case "http://schemas.openxmlformats.org/drawingml/2006/main/pattFill":
								element.Elements.RemoveAt(i);
								break;

							// MD 7/5/12 - TFS115688
							// Temporarily remove the extLst element so we can add it to the end.
							case ExtLstElement.QualifiedName:
								extLstElement = subElement;
								element.Elements.RemoveAt(i);
								break;
						}
					}

					// Add the new fill and outline elements.
					shape.FillResolved.PopulateDrawingProperties2007(element);
					shape.OutlineResolved.PopulateDrawingProperties2007(element);

					// MD 7/5/12 - TFS115688
					// Make sure the extLst element is the last element if it is present.
					if (extLstElement != null)
						element.Elements.Add(extLstElement);
				}
			}

            //  Set up the CellAnchor information so the save logic has an easy time
            //  getting the values it needs to write out to the XML elements. Note that
            //  we only have to do this for root shapes.
            if ( isRootShape )
            {
                CellAnchor cellAnchor = serializationManager.CellAnchor;
                cellAnchor.CellPosFrom.FromWorksheetCellAndPosition( shape.TopLeftCornerCell, shape.TopLeftCornerPosition );
                cellAnchor.CellPosTo.FromWorksheetCellAndPosition( shape.BottomRightCornerCell, shape.BottomRightCornerPosition );

                //  Set up the Transform in the same manner
                Transform transform  = serializationManager.Transform;

				// MD 7/18/11 - Shape support
				// cachedBoundsInTwips was removed.
                //transform.FromBoundsInTwips( serializationManager.cachedBoundsInTwips );
				transform.FromBoundsInTwips(shape.GetBoundsInTwips());
            }
        }
        #endregion OnBeforeShapeSaved

        #region FromShape
        private bool FromShape( WorksheetShape shape )
        {
            //  Note that we don't currently support this, but doing so
            //  would probably be fairly simple.
            if ( shape is UnknownShape )
            {
                Utilities.DebugFail( "WorksheetShapeSerializationManager.FromShape called on an UnknownShape - this is currently unsupported." );
                return false;
            }

            Worksheet worksheet = shape.Worksheet;
            if ( worksheet == null )
                return false;

            //  Get the WorksheetShapesSaveHelper for the specified worksheet
            if ( WorksheetShapeSerializationManager.saveHelpers == null )
                WorksheetShapeSerializationManager.saveHelpers = new Dictionary<Worksheet,WorksheetShapesSaveHelper>();

            WorksheetShapesSaveHelper saveHelper = null;
            if (WorksheetShapeSerializationManager.saveHelpers.ContainsKey(worksheet) == false )
            {
                saveHelper = new WorksheetShapesSaveHelper(worksheet);
                WorksheetShapeSerializationManager.saveHelpers.Add(worksheet, saveHelper );
            }
            else
                saveHelper = WorksheetShapeSerializationManager.saveHelpers[worksheet];

            //  Get the ShapeIdentity for this shape
            ShapeIdentity shapeId = saveHelper.ShapeIds[shape];

            //  Determine whether the shape is an image or group.
            WorksheetImage imageShape = shape as WorksheetImage;
            WorksheetShapeGroup groupShape = imageShape == null ? shape as WorksheetShapeGroup : null;

            //  Clear the Elements collection
            List<ElementDataCache> rootElements = this.Elements;
            rootElements.Clear();

            //  Determine whether the shape is a member of the worksheet.Shapes collection;
            //  if it is, it will require certain additional elements.
            bool isRootShape = shape.Collection == shape.Worksheet.Shapes;

			// MD 7/15/11 - Shape support
			// These are no longer needed.
			////  Variables that are common to image and group shapes
			//Rectangle boundsInTwips = shape.GetBoundsInTwips();
			//ElementDataCache xfrm_Element = null;

			// MD 7/15/11 - Shape support
			string nvSpPrName = string.Format("{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "nvSpPr");
			string avLstName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "avLst");

            string cNvPrName = string.Format( "{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "cNvPr" );

            #region 'pic' Element
            if ( imageShape != null )
            {                
                string cNvPicPrName = string.Format( "{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "cNvPicPr" );

				// MD 7/15/11 - Shape support
				// Moved this above
                //string avLstName = string.Format( "{0}/{1}", DrawingsPart.MainNamespace, "avLst" );

                string stretchName = string.Format( "{0}/{1}", DrawingsPart.MainNamespace, "stretch" );
                string fillRectName = string.Format( "{0}/{1}", DrawingsPart.MainNamespace, "fillRect" );

                Image image = imageShape.Image;

                //  Add an 'nvPicPr' element
                ElementDataCache nvPicPr_Element = new ElementDataCache( NvPicPrElement.QualifiedName, string.Empty );
                rootElements.Add( nvPicPr_Element );

                //  Add an 'cNvPr' element
                ElementDataCache cNvPr_Element = new ElementDataCache( cNvPrName );
                nvPicPr_Element.Elements.Add( cNvPr_Element );

                //  Add the attribute values for the 'cNvPr' element
                cNvPr_Element.AttributeValues.Add( "id", XmlElementBase.GetXmlString(shapeId.id, DataType.Integer) );
                cNvPr_Element.AttributeValues.Add( "name", shapeId.name );
                cNvPr_Element.AttributeValues.Add( "descr", shapeId.name );

                //  BF 9/9/08
                //  If the shape's Visible property is set to false, serialize it out.
                if ( imageShape.Visible == false )
                    cNvPr_Element.AttributeValues.Add( CNvPrElement.HiddenAttributeName, XmlElementBase.GetXmlString(true, DataType.Boolean) );

                //  Add an 'cNvPicPr' element
                ElementDataCache cNvPicPr_Element = new ElementDataCache( CNvPicPrElement.QualifiedName );
                nvPicPr_Element.Elements.Add( cNvPicPr_Element );

                //  Add an 'picLocks' element
                ElementDataCache picLocks_Element = new ElementDataCache( PicLocksElement.QualifiedName );
                cNvPicPr_Element.Elements.Add( picLocks_Element );

                //  Add the 'noChangeAspect' attribute for the 'picLocks' element
                picLocks_Element.AttributeValues.Add( "noChangeAspect", XmlElementBase.GetXmlString(true, DataType.Boolean) );

                //  Add a 'blipFill' element
                ElementDataCache blipFill_Element = new ElementDataCache( BlipFillElement.QualifiedName );
                rootElements.Add( blipFill_Element );

                //  Add a 'blip' element
                ElementDataCache blip_Element = new ElementDataCache( BlipElement.QualifiedName );
                blipFill_Element.Elements.Add( blip_Element );

                //  Write a blank to the 'embed' attribute value field, and mark this attribute
                //  as a consumed value, so the save logic knows to get the actual relationship ID.
                blip_Element.AttributeValues.Add( BlipElement.EmbedAttributeName, string.Empty );
                blip_Element.ConsumedValues = new Dictionary<string,HandledAttributeIdentifier>(1);
                blip_Element.ConsumedValues.Add( BlipElement.EmbedAttributeName, HandledAttributeIdentifier.BlipElement_Embed );

                //  Add a 'stretch' element
                ElementDataCache stretch_Element = new ElementDataCache( stretchName );
                blipFill_Element.Elements.Add( stretch_Element );

                //  Add a 'fillRect' element
                ElementDataCache fillRect_Element = new ElementDataCache( fillRectName );
                stretch_Element.Elements.Add( fillRect_Element );

                //  Add an 'spPr' element
                ElementDataCache spPr_Element = new ElementDataCache( SpPrElement.QualifiedName );
                rootElements.Add( spPr_Element );

				// MD 7/15/11 - Shape support
				// This is not needed because we will now write out the xfrm element in the normal way.
				#region Removed

				////  Create a Transform object and let it convert the values
				////  we need to write out.
				//Transform xfrm = new Transform();
				//xfrm.FromBoundsInTwips( boundsInTwips );
				//
				////  Add an 'xfrm' element
				//xfrm_Element = new ElementDataCache( XfrmElement.QualifiedName );
				//xfrm_Element.ContextForDirectSave = xfrm;
				//spPr_Element.Elements.Add( xfrm_Element );
				//
				////  Add the 'off' element
				//ElementDataCache off_Element = new ElementDataCache( OffElement.QualifiedName );
				//off_Element.AttributeValues.Add( "x", XmlElementBase.GetXmlString(xfrm.Off.x, DataType.Long) );
				//off_Element.AttributeValues.Add( "y", XmlElementBase.GetXmlString(xfrm.Off.y, DataType.Long) );
				//
				////  Add the 'ext' element
				//ElementDataCache ext_Element = new ElementDataCache( ExtElement.QualifiedName );
				//ext_Element.AttributeValues.Add( "cx", XmlElementBase.GetXmlString(xfrm.Ext.cx, DataType.Long) );
				//ext_Element.AttributeValues.Add( "cy", XmlElementBase.GetXmlString(xfrm.Ext.cy, DataType.Long) );
				//
				//xfrm_Element.Elements.Add( off_Element );
				//xfrm_Element.Elements.Add( ext_Element );

				#endregion // Removed

                //  Add the 'prstGeom' element, and add a 'prst' attribute to it
                //  with a value of 'rect', since we (and maybe even they) only support
                //  rectangular images.
                ElementDataCache prstGeom_Element = new ElementDataCache( PrstGeomElement.QualifiedName );
                prstGeom_Element.AttributeValues.Add( "prst", "rect" );
                spPr_Element.Elements.Add( prstGeom_Element );
                
                //  Add the 'avLst' element
                ElementDataCache avLst_Element = new ElementDataCache( avLstName );
                prstGeom_Element.Elements.Add( avLst_Element );
                
            }
            #endregion 'pic' Element

            else

            #region 'grpSp' Element
            if ( groupShape != null )
            {
                string cNvGrpSpPrName = string.Format( "{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "cNvGrpSpPr" );

                //  Add an 'nvGrpSpPr' element
                ElementDataCache nvGrpSpPr_Element = new ElementDataCache( NvGrpSpPrElement.QualifiedName );
                rootElements.Add( nvGrpSpPr_Element );

                //  Add an 'cNvPr' element
                ElementDataCache cNvPr_Element = new ElementDataCache( cNvPrName );
                nvGrpSpPr_Element.Elements.Add( cNvPr_Element );

                //  Add the attribute values for the 'cNvPr' element
                cNvPr_Element.AttributeValues.Add( "id", XmlElementBase.GetXmlString(shapeId.id, DataType.Integer) );
                cNvPr_Element.AttributeValues.Add( "name", shapeId.name );

                //  BF 9/9/08
                //  If the group's Visible property is set to false, serialize it out.
                if ( groupShape.Visible == false )
                    cNvPr_Element.AttributeValues.Add( CNvPrElement.HiddenAttributeName, XmlElementBase.GetXmlString(true, DataType.Boolean) );

                //  Add an 'cNvGrpSpPrName' element
                ElementDataCache cNvGrpSpPr_Element = new ElementDataCache( cNvGrpSpPrName );
                nvGrpSpPr_Element.Elements.Add( cNvGrpSpPr_Element );

                //  Add an 'grpSpPr' element
                ElementDataCache grpSpPr_Element = new ElementDataCache( GrpSpPrElement.QualifiedName );
                rootElements.Add( grpSpPr_Element );

				// MD 7/15/11 - Shape support
				// This is not needed because we will now write out the xfrm element in the normal way.
				#region Removed

				////  Create a Transform object and let it convert the values
				////  we need to write out.
				//GroupTransform grpXfrm = new GroupTransform();
				//grpXfrm.FromBoundsInTwips( boundsInTwips );
				//
				////  Add an 'xfrm' element
				//xfrm_Element = new ElementDataCache( XfrmElement.QualifiedName );
				//xfrm_Element.ContextForDirectSave = grpXfrm;
				//grpSpPr_Element.Elements.Add( xfrm_Element );
				//
				////  Add the 'off' element
				//ElementDataCache off_Element = new ElementDataCache( OffElement.QualifiedName );
				//off_Element.AttributeValues.Add( "x", XmlElementBase.GetXmlString(grpXfrm.Off.x, DataType.Long) );
				//off_Element.AttributeValues.Add( "y", XmlElementBase.GetXmlString(grpXfrm.Off.y, DataType.Long) );
				//
				////  Add the 'ext' element
				//ElementDataCache ext_Element = new ElementDataCache( ExtElement.QualifiedName );
				//ext_Element.AttributeValues.Add( "cx", XmlElementBase.GetXmlString(grpXfrm.Ext.cx, DataType.Long) );
				//ext_Element.AttributeValues.Add( "cy", XmlElementBase.GetXmlString(grpXfrm.Ext.cy, DataType.Long) );
				//
				////  Add the 'chOff' element
				//ElementDataCache chOff_Element = new ElementDataCache( ChOffElement.QualifiedName );
				//chOff_Element.AttributeValues.Add( "x", XmlElementBase.GetXmlString(grpXfrm.Off.x, DataType.Long) );
				//chOff_Element.AttributeValues.Add( "y", XmlElementBase.GetXmlString(grpXfrm.Off.y, DataType.Long) );
				//
				////  Add the 'chExt' element
				//ElementDataCache chExt_Element = new ElementDataCache( ChExtElement.QualifiedName );
				//chExt_Element.AttributeValues.Add( "cx", XmlElementBase.GetXmlString(grpXfrm.Ext.cx, DataType.Long) );
				//chExt_Element.AttributeValues.Add( "cy", XmlElementBase.GetXmlString(grpXfrm.Ext.cy, DataType.Long) );
				//
				//xfrm_Element.Elements.Add( off_Element );
				//xfrm_Element.Elements.Add( ext_Element );

				#endregion // Removed

            }
            #endregion 'grpSp' Element

            else
			{
				// MD 10/10/11 - TFS90805
				if (shape.Type2007.HasValue == false)
				{
					Utilities.DebugFail("WorksheetShapeSerializationManager.FromShape called on a shape with no shape type in 2007.");
					return false;
				}

				// MD 7/15/11 - Shape support
				// We now support some shape types, so we can get in here.
				//Utilities.DebugFail( "Unknown WorksheetShape-derived type in WorksheetShapeSerializationManager.FromShape." );
				string nvCxnSpPrName = string.Format("{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "nvCxnSpPr");
				string cNvSpPrName = string.Format("{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "cNvSpPr");
				string cNvCxnSpPrName = string.Format("{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "cNvCxnSpPr");
				

				//  Add an 'nvSpPr' element
				// MD 10/10/11 - TFS81451
				//ElementDataCache nvSpPr_Element = new ElementDataCache(shape.IsConnector ? nvCxnSpPrName : nvSpPrName, string.Empty);
				ElementDataCache nvSpPr_Element = new ElementDataCache(shape.UseCxnShapePropertiesElement ? nvCxnSpPrName : nvSpPrName, string.Empty);

				rootElements.Add(nvSpPr_Element);

				//  Add an 'nvSpPr/cNvPr' element
				ElementDataCache cNvPr_Element = new ElementDataCache(cNvPrName);
				nvSpPr_Element.Elements.Add(cNvPr_Element);

				//  Add the attribute values for the 'cNvPr' element
				cNvPr_Element.AttributeValues.Add("id", XmlElementBase.GetXmlString(shapeId.id, DataType.Integer));
				cNvPr_Element.AttributeValues.Add("name", shapeId.name);

				if (shape.Visible == false)
					cNvPr_Element.AttributeValues.Add(CNvPrElement.HiddenAttributeName, XmlElementBase.GetXmlString(true, DataType.Boolean));

				// MD 10/10/11 - TFS81451
				//ElementDataCache cNvSpPr_Element = new ElementDataCache(shape.IsConnector ? cNvCxnSpPrName : cNvSpPrName);
				ElementDataCache cNvSpPr_Element = new ElementDataCache(shape.UseCxnShapePropertiesElement ? cNvCxnSpPrName : cNvSpPrName);

				nvSpPr_Element.Elements.Add(cNvSpPr_Element);

				//  Add an 'spPr' element
				ElementDataCache spPr_Element = new ElementDataCache(SpPrElement.QualifiedName);
				rootElements.Add(spPr_Element);

				//  Add a 'spPr/prstGeom' element
				ElementDataCache prstGeom_Element = new ElementDataCache(PrstGeomElement.QualifiedName);

				// MD 10/10/11 - TFS90805
				//prstGeom_Element.AttributeValues.Add("prst", XmlElementBase.GetXmlString(WorksheetShape.ConvertShapeType(shape.Type), DataType.ST_ShapeType));
				prstGeom_Element.AttributeValues.Add("prst", XmlElementBase.GetXmlString(shape.Type2007.Value, DataType.ST_ShapeType));

				spPr_Element.Elements.Add(prstGeom_Element);

				//  Add an 'spPr/prstGeom/avLst' element
				ElementDataCache avLst_Element = new ElementDataCache(avLstName);
				prstGeom_Element.Elements.Add(avLst_Element);

				// MD 8/23/11 - TFS84306
				shape.FillResolved.PopulateDrawingProperties2007(spPr_Element);
				shape.OutlineResolved.PopulateDrawingProperties2007(spPr_Element);

				//  Add a 'style' element
				rootElements.Add(WorksheetShapeSerializationManager.CreateDefaultStyleElement(shape));

				WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;
				if (shapeWithText != null && shapeWithText.Text != null)
				{
					this.txBodyElementWrapper = new ElementDataCache(TxBodyElement.QualifiedName);
					rootElements.Add(this.txBodyElementWrapper);
				}
			}

            return true;
        }

        #endregion FromShape

		// MD 7/15/11 - Shape support
		#region CreateDefaultStyleElement

		private static ElementDataCache CreateDefaultStyleElement(WorksheetShape shape)
		{
			string styleName = string.Format("{0}/{1}", DrawingsPart.SpreadsheetDrawingNamespace, "style");
			string lnRefName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "lnRef");
			string fillRefName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "fillRef");
			string effectRefName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "effectRef");
			string fontRefName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "fontRef");
			string schemeClrName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "schemeClr");
			string shadeName = string.Format("{0}/{1}", DrawingsPart.MainNamespace, "shade");

			ElementDataCache style_Element = new ElementDataCache(styleName);

			ElementDataCache lnRef_Element = new ElementDataCache(lnRefName);
			lnRef_Element.AttributeValues.Add("idx", shape.IsConnector ? "1" : "2");
			style_Element.Elements.Add(lnRef_Element);
			ElementDataCache schemeClr_Element = new ElementDataCache(schemeClrName);
			schemeClr_Element.AttributeValues.Add("val", "accent1");
			lnRef_Element.Elements.Add(schemeClr_Element);

			if (shape.IsConnector == false)
			{
				ElementDataCache shade_Element = new ElementDataCache(shadeName);
				shade_Element.AttributeValues.Add("val", "50000");
				schemeClr_Element.Elements.Add(shade_Element);
			}

			ElementDataCache fillRef_Element = new ElementDataCache(fillRefName);
			fillRef_Element.AttributeValues.Add("idx", shape.IsConnector ? "0" : "1");
			style_Element.Elements.Add(fillRef_Element);
			schemeClr_Element = new ElementDataCache(schemeClrName);
			schemeClr_Element.AttributeValues.Add("val", "accent1");
			fillRef_Element.Elements.Add(schemeClr_Element);

			ElementDataCache effectRef_Element = new ElementDataCache(effectRefName);
			effectRef_Element.AttributeValues.Add("idx", "0");
			style_Element.Elements.Add(effectRef_Element);
			schemeClr_Element = new ElementDataCache(schemeClrName);
			schemeClr_Element.AttributeValues.Add("val", "accent1");
			effectRef_Element.Elements.Add(schemeClr_Element);

			ElementDataCache fontRef_Element = new ElementDataCache(fontRefName);
			fontRef_Element.AttributeValues.Add("idx", "minor");
			style_Element.Elements.Add(fontRef_Element);
			schemeClr_Element = new ElementDataCache(schemeClrName);
			schemeClr_Element.AttributeValues.Add("val", shape.IsConnector ? "tx1" : "lt1");
			fontRef_Element.Elements.Add(schemeClr_Element);

			return style_Element;
		}

		#endregion // CreateDefaultStyleElement

        #region OnWorksheetAttached
        public static void OnWorksheetAttached( WorksheetShape shape, Worksheet worksheet )
        {
            if ( shape == null || worksheet == null )
            {
                Utilities.DebugFail( "Null param in OnWorksheetAttached" );
                return;
            }

            WorksheetShapeSerializationManager.OnShapeAttached( shape, worksheet );

            //  Finally, add the shape to the worksheet's Shapes collection.
            worksheet.Shapes.Add( shape );
        }
        #endregion OnWorksheetAttached

        #region OnShapeAttached
        public static void OnShapeAttached( WorksheetShape shape, Worksheet worksheet )
        {
            if ( shape == null || worksheet == null )
            {
                Utilities.DebugFail( "Null param in OnShapeAttached" );
                return;
            }

            //  Get the WorksheetShapeSerializationManager for the shape.
            WorksheetShapeSerializationManager serializationManager = shape.Excel2007ShapeSerializationManager;

            //  Convert the values that came in from the 'off' and 'ext'
            //  elements to twips, create a Rectangle, then assign the
            //  resulting values to the position related properties.
            Transform xfrm = serializationManager.Transform;

			// MD 7/2/12 - TFS115692
			// This is no longer needed.
            //xfrm.SetGroupDelta( shape );

			// MD 10/12/10 - TFS49853
			// In some cases, the 'off' and 'ext' elements are empty. By default, we should rely on the cell anchor. 
			// If it is not specified, then we should rely 'off' and 'ext' elements.
            //WorksheetShapeSerializationManager.SetBoundsFromTransform( shape, worksheet, xfrm );
			if (serializationManager.CellAnchor.HasBeenAssigned)
				WorksheetShapeSerializationManager.SetBoundsFromCellAnchor(shape, worksheet, serializationManager.CellAnchor);
			else
				WorksheetShapeSerializationManager.SetBoundsFromTransform(shape, worksheet, xfrm);

			// MD 8/30/11 - TFS84387
			// We need to actually copy over the positioning mode to the shape.
			shape.PositioningMode = serializationManager.CellAnchor.PositioningMode;

            //  Determine whether the shape is a group; if it is, call this method
            //  recursively on each member of its Shapes collection.
            WorksheetShapeGroup group = shape as WorksheetShapeGroup;
            if ( group != null )
            {
                foreach( WorksheetShape childShape in group.Shapes )
                {
                    WorksheetShapeSerializationManager.OnShapeAttached( childShape, worksheet );
                }
            }
        }

        #endregion OnShapeAttached

		// MD 7/3/12 - TFS115689
		// Added round trip support for line end properties.
		#region SerializeLineEndProperties

		internal static void SerializeLineEndProperties(ElementDataCache parentElement, LineEndProperties properties)
		{
			parentElement.AttributeValues.Add(LineEndPropertiesElementBase.LenAttributeName, XmlElementBase.GetXmlString(properties.len, DataType.ST_LineEndLength));
			parentElement.AttributeValues.Add(LineEndPropertiesElementBase.TypeAttributeName, XmlElementBase.GetXmlString(properties.type, DataType.ST_LineEndType));
			parentElement.AttributeValues.Add(LineEndPropertiesElementBase.WAttributeName, XmlElementBase.GetXmlString(properties.w, DataType.ST_LineEndWidth));
		}

		#endregion // SerializeLineEndProperties

		// MD 8/23/11 - TFS84306
		#region SerializeNoFill

		internal static void SerializeNoFill(ElementDataCache parentElement)
		{
			//  Add a '.../noFill' element
			ElementDataCache noFill_Element = new ElementDataCache(NoFillElement.QualifiedName);
			parentElement.Elements.Add(noFill_Element);
		}

		#endregion  // SerializeNoFill

		// MD 8/23/11 - TFS84306
		#region SerializeSolidFill

		internal static void SerializeSolidFill(ElementDataCache parentElement, Color solidFillColor)
		{
			//  Add a '.../solidFill' element
			ElementDataCache solidFill_Element = new ElementDataCache(SolidFillElement.QualifiedName);
			parentElement.Elements.Add(solidFill_Element);

			//  Add a '.../solidFill/srgbClr' element
			ElementDataCache srgbClr_Element = new ElementDataCache(SrgbClrElement.QualifiedName);
			srgbClr_Element.AttributeValues.Add("val", XmlElementBase.GetXmlString(Utilities.ColorToArgb(solidFillColor), DataType.ST_HexBinary3));
			solidFill_Element.Elements.Add(srgbClr_Element);

			if (solidFillColor.A != Byte.MaxValue)
			{
				//  Add a '.../solidFill/srgbClr/alpha' element
				ElementDataCache alpha_Element = new ElementDataCache(AlphaElement.QualifiedName);
				int val = Convert.ToInt32(solidFillColor.A / 255d * 100000);
				alpha_Element.AttributeValues.Add("val", XmlElementBase.GetXmlString(val, DataType.Int32));
				srgbClr_Element.Elements.Add(alpha_Element);
			}
		}

		// MD 1/17/12 - 12.1 - Cell Format Updates
		internal static void SerializeSolidFill(ElementDataCache parentElement, WorkbookColorInfo solidFillColorInfo)
		{
			//  Add a '.../solidFill' element
			ElementDataCache solidFill_Element = new ElementDataCache(SolidFillElement.QualifiedName);
			parentElement.Elements.Add(solidFill_Element);

			if (solidFillColorInfo.IsAutomatic)
			{
				//  Add a '.../solidFill/sysClr' element
				ElementDataCache sysClr_Element = new ElementDataCache(SysClrElement.QualifiedName);
				sysClr_Element.AttributeValues.Add("val", XmlElementBase.GetXmlString(ST_SystemColorVal.windowText, DataType.ST_SystemColorVal));
				solidFill_Element.Elements.Add(sysClr_Element);
				return;
			}

			ElementDataCache tintOwner;
			if (solidFillColorInfo.Color.HasValue)
			{
				Color color = solidFillColorInfo.Color.Value;

				//  Add a '.../solidFill/srgbClr' element
				ElementDataCache srgbClr_Element = new ElementDataCache(SrgbClrElement.QualifiedName);
				tintOwner = srgbClr_Element;
				srgbClr_Element.AttributeValues.Add("val", XmlElementBase.GetXmlString(Utilities.ColorToArgb(color), DataType.ST_HexBinary3));
				solidFill_Element.Elements.Add(srgbClr_Element);

				if (color.A != Byte.MaxValue)
				{
					//  Add a '.../solidFill/srgbClr/alpha' element
					ElementDataCache alpha_Element = new ElementDataCache(AlphaElement.QualifiedName);
					int val = Convert.ToInt32(color.A / 255d * 100000);
					alpha_Element.AttributeValues.Add("val", XmlElementBase.GetXmlString(val, DataType.Int32));
					srgbClr_Element.Elements.Add(alpha_Element);
				}
			}
			else if (solidFillColorInfo.ThemeColorType.HasValue)
			{
				//  Add a '.../solidFill/schemeClr' element
				ElementDataCache schemeClr_Element = new ElementDataCache(SchemeClrElement.QualifiedName);
				tintOwner = schemeClr_Element;
				schemeClr_Element.AttributeValues.Add("val", XmlElementBase.GetXmlString((ST_SchemeColorVal)solidFillColorInfo.ThemeColorType.Value, DataType.ST_SchemeColorVal));
				solidFill_Element.Elements.Add(schemeClr_Element);
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			if (solidFillColorInfo.Tint.HasValue)
			{
				//  Add a '.../solidFill/.../tint' element
				ElementDataCache tint_Element = new ElementDataCache(TintElement.QualifiedName);
				int tintValue = (int)MathUtilities.MidpointRoundingAwayFromZero((solidFillColorInfo.Tint.Value + 1.0) * 100000);
				tint_Element.AttributeValues.Add("val", XmlElementBase.GetXmlString(solidFillColorInfo.Tint.Value, DataType.Int32));
				tintOwner.Elements.Add(tint_Element);
			}
		}

		#endregion  // SerializeSolidFill

		// MD 10/12/10 - TFS49853
		#region SetBoundsFromCellAnchor

		private static void SetBoundsFromCellAnchor(WorksheetShape shape, Worksheet worksheet, CellAnchor cellAnchor)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//WorksheetCell topLeftCell;
			WorksheetCellAddress topLeftCell;

			PointF topLeftPosition;
			cellAnchor.CellPosFrom.ToWorksheetCellAndPosition(worksheet, out topLeftCell, out topLeftPosition);

			// MD 2/15/11 - TFS66316
			// For oneCellAnchor element, there is no <to> element. There is only a <from> and <ext> element.
			//WorksheetCell bottomRightCell;
			//PointF bottomRightPosition;
			//cellAnchor.CellPosTo.ToWorksheetCellAndPosition(worksheet, out bottomRightCell, out bottomRightPosition);
			//
			//shape.SetAnchors(topLeftCell, topLeftPosition, bottomRightCell, bottomRightPosition);
			if (cellAnchor.CellPosTo.HasValue)
			{
				// MD 3/27/12 - 12.1 - Table Support
				//WorksheetCell bottomRightCell;
				WorksheetCellAddress bottomRightCell;

				PointF bottomRightPosition;
				cellAnchor.CellPosTo.ToWorksheetCellAndPosition(worksheet, out bottomRightCell, out bottomRightPosition);

				// MD 3/27/12 - 12.1 - Table Support
				//shape.SetAnchors(topLeftCell, topLeftPosition, bottomRightCell, bottomRightPosition);
				shape.SetAnchors(worksheet, topLeftCell, topLeftPosition, bottomRightCell, bottomRightPosition);
			}
			else
			{
				// MD 3/27/12 - 12.1 - Table Support
				//Rectangle topLeftRect = WorksheetShape.GetBoundsInTwips(topLeftCell, topLeftPosition, topLeftCell, topLeftPosition);
				Rectangle topLeftRect = WorksheetShape.GetBoundsInTwips(worksheet,
					topLeftCell, topLeftPosition,
					topLeftCell, topLeftPosition);

				Rectangle shapeRect = new Rectangle(
					topLeftRect.Left,
					topLeftRect.Top,
					Utilities.EMUToTwips((int)cellAnchor.ExtentX),
					Utilities.EMUToTwips((int)cellAnchor.ExtentY));

				shape.SetBoundsInTwips(worksheet, shapeRect);
			}
		} 

		#endregion // SetBoundsFromCellAnchor

		// MD 7/2/12 - TFS115692
		private static Rectangle GetRect(Transform.Offset offset, Transform.Extent extent)
		{
			int left = Utilities.EMUToTwips((int)(offset.x));
			int top = Utilities.EMUToTwips((int)(offset.y));
			int width = Utilities.EMUToTwips((int)(extent.cx));
			int height = Utilities.EMUToTwips((int)(extent.cy));
			return new Rectangle(left, top, width, height);
		}

        #region SetBoundsFromTransform
        static private void SetBoundsFromTransform( WorksheetShape shape, Worksheet worksheet, Transform xfrm )
        {
			// MD 7/2/12 - TFS115692
			#region Old Code

			//int left = 0;
			//int top = 0;

			//// MD 7/2/12 - TFS115693
			//// The width will be differnt if the shape is in a group.
			////int width = Utilities.EMUToTwips( (int)(xfrm.Ext.cx) );
			////int height = Utilities.EMUToTwips( (int)(xfrm.Ext.cy) );
			//int width = 0;
			//int height = 0;

			////  BF 9/5/08
			////  Apply the chOff values if this shape belongs to a group
			//WorksheetShapeGroup owningGroup = shape.Owner as WorksheetShapeGroup;
			//if ( owningGroup != null )
			//{
			//    Rectangle groupBoundsInTwips = owningGroup.GetBoundsInTwips();
			//    int offsetX = Utilities.EMUToTwips( (int)(xfrm.GroupDelta.x) );
			//    int offsetY = Utilities.EMUToTwips( (int)(xfrm.GroupDelta.y) );

			//    left = (int)groupBoundsInTwips.Left + offsetX;
			//    top = (int)groupBoundsInTwips.Top + offsetY;

			//    // MD 7/2/12 - TFS115693
			//    width = (int)(xfrm.GroupDelta.widthTranform * groupBoundsInTwips.Width);
			//    height = (int)(xfrm.GroupDelta.heightTranform * groupBoundsInTwips.Height);


			//}
			//else
			//{
			//    left = Utilities.EMUToTwips( (int)(xfrm.Off.x) );
			//    top = Utilities.EMUToTwips( (int)(xfrm.Off.y) );

			//    // MD 7/2/12 - TFS115693
			//    width = Utilities.EMUToTwips((int)(xfrm.Ext.cx));
			//    height = Utilities.EMUToTwips((int)(xfrm.Ext.cy));
			//}

			//Rectangle boundsInTwips = new Rectangle( left, top, width, height );            
			//shape.SetBoundsInTwips( worksheet, boundsInTwips );

			#endregion // Old Code
			Rectangle boundsInTwips = WorksheetShapeSerializationManager.GetRect(xfrm.Off, xfrm.Ext);

			WorksheetShapeGroup owningGroup = shape.Owner as WorksheetShapeGroup;
			if (owningGroup != null)
			{
				GroupTransform groupXfrm = owningGroup.Excel2007ShapeSerializationManager.Transform as GroupTransform;
				if (groupXfrm != null)
				{
					// MD 7/23/12 - TFS117431
					// Our group bounds may be different than those saved out in the Off and Ext elements due to the machine on which
					// the file was saved. Use our calculated groups bounds from the anchor cells instead of what is saved.
					//Rectangle absoluteGroupBounds = WorksheetShapeSerializationManager.GetRect(groupXfrm.Off, groupXfrm.Ext);
					Rectangle absoluteGroupBounds = owningGroup.GetBoundsInTwips();

					Rectangle groupChildrenBounds = WorksheetShapeSerializationManager.GetRect(groupXfrm.ChOff, groupXfrm.ChExt);

					// MD 7/24/12 - TFS115693
					// This conversion method also needs the workbook format and shape rotation.
					//boundsInTwips = Utilities.GetAbsoluteShapeBounds(boundsInTwips, groupChildrenBounds, absoluteGroupBounds);
					boundsInTwips = Utilities.GetAbsoluteShapeBounds(boundsInTwips, groupChildrenBounds, absoluteGroupBounds, 
						worksheet.CurrentFormat, shape.Rotation);
				}
			}
			
			shape.SetBoundsInTwips(worksheet, boundsInTwips);
        }
        #endregion SetBoundsFromTransform

        #region OffsetToPointF

        private static PointF OffsetToPointF( Transform.Offset offset, int colWidth, int rowHeight )
        {
            return WorksheetShapeSerializationManager.OffsetToPointF( offset.x, offset.y, colWidth, rowHeight );
        }

        private static PointF OffsetToPointF( long x, long y, int colWidth, int rowHeight )
        {
            int twips = Utilities.EMUToTwips( (int)x );
            float xPos = ( (float)(twips) / (float)(colWidth) );
            xPos *= 100;

            twips = Utilities.EMUToTwips( (int)y );
            float yPos = ( (float)(twips) / (float)(rowHeight) );
            yPos *= 100;

            return new PointF( xPos, yPos );
        }

        #endregion OffsetToPointF

        #region OnUnformattedStringChanged
		// MD 11/10/11 - TFS85193
        //public void OnUnformattedStringChanged()
		public void OnFormattedStringChanged()
        {
            //  If the Text property was initialized via deserialization,
            //  and the Text property was changed via the object model,
            //  remove all of its child elements. We will use this to signify
            //  to the save logic that we need to build the XML manually.
            if ( this.txBodyElementWrapper != null )
                this.txBodyElementWrapper.Elements.Clear();
        }
        #endregion OnUnformattedStringChanged

		#region TxBodyElementWrapper

		public ElementDataCache TxBodyElementWrapper
		{
			get { return this.txBodyElementWrapper; }
			set { this.txBodyElementWrapper = value; }
		} 

		#endregion TxBodyElementWrapper

        #region ShapeIdentity class
        internal class ShapeIdentity
        {
            public int id = 0;
            public string name = string.Empty;

            public ShapeIdentity( WorksheetShape shape, int id )
            {
                this.name = ShapeIdentity.NameFromId( shape, id );
            }

            static private string NameFromId( WorksheetShape shape, int id )
            {
                string resourceToken =
                    shape is WorksheetImage ?
                    "Image" :
                    shape is WorksheetShapeGroup ?
                    "Group" :
                    "Shape";

                string resourceName = string.Format( "WorksheetShapeSerialization_{0}Name", resourceToken );
                return string.Format( "{0} {1}", SR.GetString(resourceName), id );
            }
        }
        #endregion ShapeIdentity class
    }
    #endregion WorksheetShapeSerializationManager class

    #region WorksheetShapesHolder class






    internal class WorksheetShapesHolder
    {
        #region Member variables
        
        private List<WorksheetShape> shapes = new List<WorksheetShape>();
        WorksheetShapeSerializationManager currentSerializationManager = null;
        
        #endregion Member variables

		public WorksheetShapesHolder() { }

        #region Properties
        
        public WorksheetShapeSerializationManager CurrentSerializationManager
        {
            get { return this.currentSerializationManager; }
            set { this.currentSerializationManager = value; }
        }

        public List<WorksheetShape> Shapes
        {
            get { return this.shapes; }
        }
        
        #endregion Properties
    }
    #endregion WorksheetShapesHolder class

    #region WorksheetShapesSaveHelper class





    internal class WorksheetShapesSaveHelper
    {
        #region Member variables
        
        private Dictionary<WorksheetShape, WorksheetShapeSerializationManager.ShapeIdentity> shapeIds = null;                
        
        #endregion Member variables

        #region Constructor

        public WorksheetShapesSaveHelper( Worksheet worksheet )
        {
            this.shapeIds = new Dictionary<WorksheetShape,WorksheetShapeSerializationManager.ShapeIdentity>();
            WorksheetShapesSaveHelper.GetShapeIdsAndNames( worksheet, this.shapeIds );
        }

        #endregion Constructor

        #region ShapeIds
        public Dictionary<WorksheetShape, WorksheetShapeSerializationManager.ShapeIdentity> ShapeIds
        {
            get { return this.shapeIds; }
        }
        #endregion ShapeIds

        #region GetShapeIdsAndNames

        static private void GetShapeIdsAndNames( Worksheet worksheet, Dictionary<WorksheetShape, WorksheetShapeSerializationManager.ShapeIdentity> shapeIds )
        {
            //  Note that seeding this with zero will make all ids one-based.
            int currentId = 0;

			// MD 4/28/11 - TFS62775
			// Added the serialization manager as a parameter.
            //WorksheetShapesSaveHelper.GetShapeIdsAndNames( worksheet.Shapes, shapeIds, ref currentId );
			WorksheetShapesSaveHelper.GetShapeIdsAndNames(worksheet.Workbook.CurrentSerializationManager, worksheet.Shapes, shapeIds, ref currentId);
        }

		// MD 4/28/11 - TFS62775
		// Added the serialization manager as a parameter.
        //static private void GetShapeIdsAndNames( WorksheetShapeCollection shapes, Dictionary<WorksheetShape, WorksheetShapeSerializationManager.ShapeIdentity> shapeIds, ref int currentId )
		static private void GetShapeIdsAndNames(WorkbookSerializationManager manager, WorksheetShapeCollection shapes, Dictionary<WorksheetShape, WorksheetShapeSerializationManager.ShapeIdentity> shapeIds, ref int currentId)
        {
			// MD 4/28/11 - TFS62775
			// Make sure we go through the PrepareShapeForSerialization method to remove invalid shapes.
			//foreach ( WorksheetShape shape in shapes )
			//{
			//    currentId++;
			//    shapeIds.Add( shape, new WorksheetShapeSerializationManager.ShapeIdentity( shape, currentId ) );
			//
			//    WorksheetShapeGroup group = shape as WorksheetShapeGroup;
			//    if ( group != null )
			//        WorksheetShapesSaveHelper.GetShapeIdsAndNames( group.Shapes, shapeIds, ref currentId );                
			//}
			for (int i = 0; i < shapes.Count; i++)
			{
				WorksheetShape shape = shapes[i];
				manager.PrepareShapeForSerialization(ref shape);

				if (shape == null)
					continue;

				currentId++;
				shapeIds.Add(shape, new WorksheetShapeSerializationManager.ShapeIdentity(shape, currentId));

				WorksheetShapeGroup group = shape as WorksheetShapeGroup;
				if (group != null)
					WorksheetShapesSaveHelper.GetShapeIdsAndNames(manager, group.Shapes, shapeIds, ref currentId);
			}
        }
        #endregion GetShapeIdsAndNames
    }
    #endregion WorksheetShapesSaveHelper class

    #region CellAnchor class






    internal class CellAnchor
    {
        #region Member variables
		
        bool hasBeenAssigned = false;
        private CellPosition cellPosFrom = null;
        private CellPosition cellPosTo = null;
        private CellPosition currentCellPos = null;

		// MD 8/30/11 - TFS84387
		// The default value for the editAs attribute on twoCellAnchor is twoCell, which corresponds to MoveAndSizeWithCells.
		//private ShapePositioningMode positioningMode = ShapePositioningMode.MoveWithCells;
		private ShapePositioningMode positioningMode = ShapePositioningMode.MoveAndSizeWithCells;

		// MD 2/15/11 - TFS66316
		private long extentX;
		private long extentY;

        #endregion Member variables

        #region Constructor
        public CellAnchor()
        {
            this.currentCellPos = this.cellPosFrom;
        }
        #endregion Constructor

        #region Properties

        public bool HasBeenAssigned
        {
            get { return this.hasBeenAssigned; }
            set { this.hasBeenAssigned = value; }
        }

        public CellPosition CellPosFrom
        { 
            get
            {
                if ( this.cellPosFrom == null )
                    this.cellPosFrom = new CellPosition();

                return this.cellPosFrom;
            }
        }

        public CellPosition CellPosTo
        { 
            get
            {
                if ( this.cellPosTo == null )
                    this.cellPosTo = new CellPosition();

                return this.cellPosTo;
            }
        }

        public CellPosition CurrentCellPos
        { 
            get
            { 
                if ( this.currentCellPos == null )
                    this.currentCellPos = this.cellPosFrom;

                return this.currentCellPos;
            }

            set { this.currentCellPos = value; }
        }

		// MD 2/15/11 - TFS66316
		public long ExtentX
		{
			get { return this.extentX; }
			set { this.extentX = value; }
		}

		// MD 2/15/11 - TFS66316
		public long ExtentY
		{
			get { return this.extentY; }
			set { this.extentY = value; }
		}

        public ShapePositioningMode PositioningMode
        { 
            get { return this.positioningMode; }
            set { this.positioningMode = value; }
        }

        #endregion Properties

        #region CellPosition






        internal class CellPosition
        {
            #region Member variables
    		
            private int row = -1;
            private int col = -1;
		    private long rowOffset = -1;
		    private long colOffset = -1;

            #endregion Member variables

            #region Properties

            public int Row
            { 
                get { return this.row; }
                set { this.row = value; }
            }

            public int Col
            { 
                get { return this.col; }
                set { this.col = value; }
            }

            public long RowOffset
            { 
                get { return this.rowOffset; }
                set { this.rowOffset = value; }
            }

            public long ColOffset
            { 
                get { return this.colOffset; }
                set { this.colOffset = value; }
            }

            #endregion Properties

            #region HasValue
            public bool HasValue
            {
                get { return this.col >= 0 && this.colOffset >= 0 && this.row >= 0 && this.rowOffset >= 0; }
            }
            #endregion HasValue

            #region FromWorksheetCellAndPosition
            public void FromWorksheetCellAndPosition( WorksheetCell cell, PointF position )
            {
                //  Set the column and row indices
                this.col = cell.ColumnIndex;
                this.row = cell.RowIndex;

                //  Get the twip sizes of the cell
                Worksheet worksheet = cell.Worksheet;

				// MD 2/29/12 - 12.1 - Table Support
				if (worksheet == null)
				{
					Utilities.DebugFail("This is unexpected.");
					return;
				}

                int colWidth = worksheet.GetColumnWidthInTwips( this.col );
                int rowHeight = worksheet.GetRowHeightInTwips( this.row );

                float posX = ((float)position.X / 100f);
                float posY = ((float)position.Y / 100f);

				// MD 10/12/10 - TFS49853
				// Instead of casting, we should be rounding to prevent one-off errors.
				//this.colOffset = Utilities.TwipsToEMU( (int)( posX * colWidth ) );
				//this.rowOffset = Utilities.TwipsToEMU( (int)( posY * rowHeight ) );
				this.colOffset = Utilities.TwipsToEMU(MathUtilities.MidpointRoundingAwayFromZero(posX * colWidth));
				this.rowOffset = Utilities.TwipsToEMU(MathUtilities.MidpointRoundingAwayFromZero(posY * rowHeight));
            }
            #endregion FromWorksheetCellAndPosition

			// MD 10/12/10 - TFS49853
			#region ToWorksheetCellAndPosition

			// MD 3/27/12 - 12.1 - Table Support
			//public void ToWorksheetCellAndPosition(Worksheet worksheet, out WorksheetCell cell, out PointF position)
			public void ToWorksheetCellAndPosition(Worksheet worksheet, out WorksheetCellAddress cell, out PointF position)
			{
				// MD 3/27/12 - 12.1 - Table Support
				//cell = worksheet.Rows[this.row].Cells[this.col];
				cell = new WorksheetCellAddress(this.row, (short)this.col);

				
				int colWidth = worksheet.GetColumnWidthInTwips(this.col);
				int rowHeight = worksheet.GetRowHeightInTwips(this.row);

				int colOffsetInTwips = Utilities.EMUToTwips((int)this.colOffset);
				int rowOffsetInTwips = Utilities.EMUToTwips((int)this.rowOffset);

				position = new PointF(
					(colOffsetInTwips * 100f) / colWidth,
					(rowOffsetInTwips * 100f) / rowHeight);
			} 

			#endregion // ToWorksheetCellAndPosition
        }
        #endregion CellPosition
    }
    #endregion CellAnchor class

    #region Transform class





    internal class Transform
    {
        #region Member variables
        
        private bool hasBeenAssigned = false;
        private Extent ext = new Extent();
        private Offset off = new Offset();

		// MD 7/2/12 - TFS115692
		#region Removed

		//// MD 7/2/12 - TFS115693
		////private Offset groupDelta = null;
		//private GroupDeltaHelper groupDelta;

		#endregion // Removed

        #endregion Member variables

        #region Properties

        public bool HasBeenAssigned
        {
            get { return this.hasBeenAssigned; }
            set { this.hasBeenAssigned = value; }
        }

        public Extent Ext { get { return this.ext; } }

        public Offset Off { get { return this.off; } }

        #endregion Properties

        #region FromBoundsInTwips
        public virtual void FromBoundsInTwips( Rectangle boundsInTwips )
        {
            //  Convert the rectangle's components into EMUs
            int left =  Utilities.TwipsToEMU( boundsInTwips.Left );
            int top =  Utilities.TwipsToEMU( boundsInTwips.Top );
            int width =  Utilities.TwipsToEMU( boundsInTwips.Width );
            int height =  Utilities.TwipsToEMU( boundsInTwips.Height );

            //  Set the offset and extent to the EMU values
            this.Off.x = left;
            this.Off.y = top;
            this.Ext.cx = width;
            this.Ext.cy = height;
        }
        #endregion FromBoundsInTwips

		// MD 7/2/12 - TFS115692
		#region Removed

        //#region SetGroupDelta
		//public void SetGroupDelta( WorksheetShape shape )
		//{
		//    WorksheetShapeGroup group = shape.Owner as WorksheetShapeGroup;
		//    if ( group != null )
		//    {
		//        GroupTransform groupXfrm = group.Excel2007ShapeSerializationManager.Transform as GroupTransform;
		//        Offset chOff = groupXfrm.ChOff;

		//        // MD 7/2/12 - TFS115693
		//        Extent chExt = groupXfrm.ChExt;

		//        Transform xfrm = shape.Excel2007ShapeSerializationManager.Transform;
		//        Offset off = xfrm.Off;

		//        // MD 7/2/12 - TFS115693
		//        Extent ext = xfrm.Ext;

		//        // MD 7/2/12 - TFS115693
		//        // This offset logic is incorrect. It also need to be scaled by the ratio of the group extent to the child extent 
		//        // (the group's original size). Also, we need to calculate the amount by which to scale the group's extent to get
		//        // child shape's extent.
		//        //Offset groupDelta = new Offset();
		//        //groupDelta.x = (off.x - chOff.x);
		//        //groupDelta.y = (off.y - chOff.y);

		//        GroupDeltaHelper groupDelta = new GroupDeltaHelper();

		//        double distanceOffsetX = groupXfrm.Ext.cx / (double)chExt.cx;
		//        double distanceOffsetY = groupXfrm.Ext.cy / (double)chExt.cy;
		//        groupDelta.x = (long)((off.x - chOff.x) * distanceOffsetX);
		//        groupDelta.y = (long)((off.y - chOff.y) * distanceOffsetY);

		//        groupDelta.widthTranform = ext.cx / (double)chExt.cx;
		//        groupDelta.heightTranform = ext.cy / (double)chExt.cy;

		//        shape.Excel2007ShapeSerializationManager.Transform.groupDelta = groupDelta;
		//    }
		//}

		//// MD 7/2/12 - TFS115693
		////public Offset GroupDelta
		//public GroupDeltaHelper GroupDelta
		//{
		//    get { return this.groupDelta; }
		//}

		//#endregion SetGroupDelta

		#endregion // Removed

        #region Extent class
        internal class Extent
        {
            /// <summary>Width</summary>
            public long cx;
            
            /// <summary>Height</summary>
            public long cy;
        }
        #endregion Extent struct

        #region Offset class
        internal class Offset
        {
            /// <summary>Horizontal coordinate</summary>
            public long x;
            
            /// <summary>Vertical coordinate</summary>
            public long y;
        }
        #endregion Offset struct

		// MD 7/2/12 - TFS115692
		#region Removed

		//// MD 7/2/12 - TFS115693
		//#region GroupDeltaHelper class

		//internal class GroupDeltaHelper
		//{
		//    public long x;
		//    public long y;
		//    public double widthTranform;
		//    public double heightTranform;
		//}

		//#endregion GroupDeltaHelper struct

		#endregion // Removed
    }
    #endregion Transform class

    #region GroupTransform class






    internal class GroupTransform : Transform
    {
        #region Member variables
        private Extent chExt = new Extent();
        private Offset chOff = new Offset();
        #endregion Member variables

        #region Properties

        public Extent ChExt { get { return this.chExt; } }

        public Offset ChOff { get { return this.chOff; } }

        #endregion Properties

        #region NormalizeChildOffsetAndExtent
        public override void FromBoundsInTwips(Rectangle boundsInTwips)
        {
            base.FromBoundsInTwips(boundsInTwips);

            //  Always make the chOff/cfExt values the same as the
            //  regular ones.
            
            
            
            
            
            
            
            this.ChOff.x = this.Off.x;
            this.ChOff.y = this.Off.y;
            this.ChExt.cx = this.Ext.cx;
            this.ChExt.cy = this.Ext.cy;
        }
        #endregion NormalizeChildOffsetAndExtent
    }
    #endregion GroupTransform class

    #region ClientData class





    internal class ClientData
    {
        public const bool Default_fLocksWithSheet = true;
        public const bool Default_fPrintsWithSheet = true;
        public bool fLocksWithSheet = Default_fLocksWithSheet;
        public bool fPrintsWithSheet = Default_fPrintsWithSheet;
    }
    #endregion ClientData class

    #region ShapeAttributes class





    internal class ShapeAttributes
    {
        public const bool Default_fLocksText = true;
        public const bool Default_fPublished = false;
        public string macro = string.Empty;
        public string textlink = string.Empty;
        public bool fLocksText = Default_fLocksText;
        public bool fPublished = Default_fPublished;
    }
    #endregion ShapeAttributes class

    #region IConsumedElementValueProvider interface
    internal interface IConsumedElementValueProvider
    {
        /// <summary>
        /// Returns a dictionary which contains the name of an attribute that
        /// needs to be handled.
        /// </summary>
        Dictionary<string, HandledAttributeIdentifier> ConsumedValues{ get; }
    }
    #endregion IConsumedElementValueProvider interface
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