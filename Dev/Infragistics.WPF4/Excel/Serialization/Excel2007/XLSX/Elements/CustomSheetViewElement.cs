using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	internal class CustomSheetViewElement : XLSXElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_CustomSheetView">
		// <sequence>
		// <element name="pane" type="CT_Pane" minOccurs="0" maxOccurs="1"/>
		// <element name="selection" type="CT_Selection" minOccurs="0" maxOccurs="1"/>
		// <element name="rowBreaks" type="CT_PageBreak" minOccurs="0" maxOccurs="1"/>
		// <element name="colBreaks" type="CT_PageBreak" minOccurs="0" maxOccurs="1"/>
		// <element name="pageMargins" type="CT_PageMargins" minOccurs="0" maxOccurs="1"/>
		// <element name="printOptions" type="CT_PrintOptions" minOccurs="0" maxOccurs="1"/>
		// <element name="pageSetup" type="CT_PageSetup" minOccurs="0" maxOccurs="1"/>
		// <element name="headerFooter" type="CT_HeaderFooter" minOccurs="0" maxOccurs="1"/>
		// <element name="autoFilter" type="CT_AutoFilter" minOccurs="0" maxOccurs="1"/>
		// <element name="extLst" minOccurs="0" type="CT_ExtensionList"/>
		// </sequence>
		// <attribute name="guid" type="ST_Guid" use="required"/>
		// <attribute name="scale" type="xsd:unsignedInt" default="100"/>
		// <attribute name="colorId" type="xsd:unsignedInt" default="64"/>
		// <attribute name="showPageBreaks" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="showFormulas" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="showGridLines" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="showRowCol" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="outlineSymbols" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="zeroValues" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="fitToPage" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="printArea" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="filter" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="showAutoFilter" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="hiddenRows" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="hiddenColumns" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="state" type="ST_SheetState" default="visible"/>
		// <attribute name="filterUnique" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="view" type="ST_SheetViewType" default="normal"/>
		// <attribute name="showRuler" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="topLeftCell" type="ST_CellRef" use="optional"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>customSheetView</summary>
		public const string LocalName = "customSheetView";
		
		/// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/customSheetView</summary>
		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CustomSheetViewElement.LocalName;

		private const string GuidAttributeName = "guid";
		private const string ScaleAttributeName = "scale";
		private const string ColorIdAttributeName = "colorId";
		private const string ShowPageBreaksAttributeName = "showPageBreaks";
		private const string ShowFormulasAttributeName = "showFormulas";
		private const string ShowGridLinesAttributeName = "showGridLines";
		private const string ShowRowColAttributeName = "showRowCol";
		private const string OutlineSymbolsAttributeName = "outlineSymbols";
		private const string ZeroValuesAttributeName = "zeroValues";
		private const string FitToPageAttributeName = "fitToPage";
		private const string PrintAreaAttributeName = "printArea";
		private const string FilterAttributeName = "filter";
		private const string ShowAutoFilterAttributeName = "showAutoFilter";
		private const string HiddenRowsAttributeName = "hiddenRows";
		private const string HiddenColumnsAttributeName = "hiddenColumns";
		private const string StateAttributeName = "state";
		private const string FilterUniqueAttributeName = "filterUnique";
		private const string ViewAttributeName = "view";
		private const string ShowRulerAttributeName = "showRuler";
		private const string TopLeftCellAttributeName = "topLeftCell";

		#endregion Constants

		#region Base class overrides

			#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.customSheetView; }
		}

			#endregion Type

			#region Load

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            //  Get the Worksheet
            Worksheet worksheet = Utilities.GetWorksheet( manager.ContextStack );
            if ( worksheet == null )
                return;

			object attributeValue = null;
            
            //  Get the GUID attribute, since we need that to get back to the member
            //  of the CustomViews collection with which this instance is associated.
            //  Then get the GUID off that
			ExcelXmlAttributeCollection attributes = element.Attributes;
			ExcelXmlAttribute guidAttribute = attributes[ CustomSheetViewElement.GuidAttributeName ];
            attributeValue = XmlElementBase.GetAttributeValue( guidAttribute, DataType.ST_Guid, Guid.Empty );
            Guid id = (Guid)attributeValue;
            if ( id == Guid.Empty )
            {
                Debug.Assert( false, "Could not get a GUID from the customSheetView element - unexpected." );
                return;
            }

            //  Create a new CustomViewDisplayOptions instance for the worksheet;
            //  initialize it from the Worksheet's DisplayOptions.
            CustomViewDisplayOptions displayOptions = new CustomViewDisplayOptions( worksheet );

			// MD 4/6/12 - TFS102169
			// The displayOptions are just temporary to get the values and we will copy them to the DisplayOptions
			// for the worksheet in the custom view when the loading is done.
			////  Push the DisplayOptions onto the context stack; this is so
			////  the PaneElement class can support parent elements of type
			////  SheetViewElement and CustomSheetViewElement (this one).
			//manager.ContextStack.Push( displayOptions );

            UnfrozenPaneSettings unfrozenPaneSettings = null;            
            
			foreach ( ExcelXmlAttribute attribute in attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					#region guid
                    //
                    //  We do this before entering this loop now (see above)
                    //
                    //// ***REQUIRED***
                    //// Name = 'guid', Type = ST_Guid, Default = 
                    //case CustomSheetViewElement.GuidAttributeName:
                    //{
                    //    attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Guid, Guid.Empty );
                    //    id = (Guid)attributeValue;
                    //}
                    //break;
					#endregion guid

					#region scale
					// Name = 'scale', Type = UInt32, Default = 100
					case CustomSheetViewElement.ScaleAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, 100 );
                        int scale = Utilities.ToInteger(attributeValue);
                        displayOptions.MagnificationInCurrentView = scale;
					}
					break;
					#endregion scale

					#region colorId
					// Name = 'colorId', Type = UInt32, Default = 64
					case CustomSheetViewElement.ColorIdAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, 64 );
                        int colorId = Utilities.ToInteger( attributeValue );

                        // If we're loading a color, then we shouldn't try to use an automatic color,
                        // especially as it seems that Excel 2007 doesn't have this concept
						// MD 1/16/12 - 12.1 - Cell Format Updates
                        //if (colorId != WorkbookColorCollection.AutomaticColor)
						if (colorId != WorkbookColorPalette.AutomaticColor)
                            displayOptions.UseAutomaticGridlineColor = false;

                        displayOptions.GridlineColorIndex = colorId;
					}
					break;
					#endregion colorId

					#region showPageBreaks
					// Name = 'showPageBreaks', Type = Boolean, Default = False
					case CustomSheetViewElement.ShowPageBreaksAttributeName:
					{
                        //  RoundTrip - Page 1953
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool showPageBreaks = (bool)attributeValue;
					}
					break;
					#endregion showPageBreaks

					#region showFormulas
					// Name = 'showFormulas', Type = Boolean, Default = False
					case CustomSheetViewElement.ShowFormulasAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        bool showFormulas = (bool)attributeValue;
						displayOptions.ShowFormulasInCells = showFormulas;

					}
					break;
					#endregion showFormulas

					#region showGridLines
					// Name = 'showGridLines', Type = Boolean, Default = True
					case CustomSheetViewElement.ShowGridLinesAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        bool showGridLines = (bool)attributeValue;
                        displayOptions.ShowGridlines = showGridLines;
					}
					break;
					#endregion showGridLines

					#region showRowCol
					// Name = 'showRowCol', Type = Boolean, Default = True
					case CustomSheetViewElement.ShowRowColAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        bool showRowCol = (bool)attributeValue;
                        displayOptions.ShowRowAndColumnHeaders = showRowCol;
					}
					break;
					#endregion showRowCol

					#region outlineSymbols
					// Name = 'outlineSymbols', Type = Boolean, Default = True
					case CustomSheetViewElement.OutlineSymbolsAttributeName:
					{
                        //  RoundTrip - Page 1953
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        bool outlineSymbols = (bool)attributeValue;
                        displayOptions.ShowOutlineSymbols = outlineSymbols;
					}
					break;
					#endregion outlineSymbols

					#region zeroValues
					// Name = 'zeroValues', Type = Boolean, Default = True
					case CustomSheetViewElement.ZeroValuesAttributeName:
					{
                        //  RoundTrip - Page 1953
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        bool zeroValues = (bool)attributeValue;
                        displayOptions.ShowZeroValues = zeroValues;
					}
					break;
					#endregion zeroValues

					#region fitToPage
					// Name = 'fitToPage', Type = Boolean, Default = False
					case CustomSheetViewElement.FitToPageAttributeName:
					{
                        //  RoundTrip - Page 1953
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool fitToPage = (bool)attributeValue;
					}
					break;
					#endregion fitToPage

					#region printArea
					// Name = 'printArea', Type = Boolean, Default = False
					case CustomSheetViewElement.PrintAreaAttributeName:
					{
                        //  RoundTrip - Page 1953
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool printArea = (bool)attributeValue;
					}
					break;
					#endregion printArea

					#region filter
					// Name = 'filter', Type = Boolean, Default = False
					case CustomSheetViewElement.FilterAttributeName:
					{
                        //  RoundTrip - Page 1953
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool filter = (bool)attributeValue;
					}
					break;
					#endregion filter

					#region showAutoFilter
					// Name = 'showAutoFilter', Type = Boolean, Default = False
					case CustomSheetViewElement.ShowAutoFilterAttributeName:
					{
                        //  RoundTrip - Page 1953
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool showAutoFilter = (bool)attributeValue;
					}
					break;
					#endregion showAutoFilter

					#region hiddenRows
					// Name = 'hiddenRows', Type = Boolean, Default = False
					case CustomSheetViewElement.HiddenRowsAttributeName:
					{
                        //  RoundTrip - Page 1953
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool hiddenRows = (bool)attributeValue;
					}
					break;
					#endregion hiddenRows

					#region hiddenColumns
					// Name = 'hiddenColumns', Type = Boolean, Default = False
					case CustomSheetViewElement.HiddenColumnsAttributeName:
					{
                        //  RoundTrip - Page 1953
                        //attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool hiddenColumns = (bool)attributeValue;
					}
					break;
					#endregion hiddenColumns

					#region state
					// Name = 'state', Type = ST_SheetState, Default = visible
					case CustomSheetViewElement.StateAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_SheetState, "visible" );
                        WorksheetVisibility visibility = (WorksheetVisibility)attributeValue ;
						displayOptions.Visibility = visibility;
					}
					break;
					#endregion state

					#region filterUnique
					// Name = 'filterUnique', Type = Boolean, Default = False
					case CustomSheetViewElement.FilterUniqueAttributeName:
					{
                        //  RoundTrip - Page 1953
                        //attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool filterUnique = (bool)attributeValue;
					}
					break;
					#endregion filterUnique

					#region view
					// Name = 'view', Type = ST_SheetViewType, Default = normal
					case CustomSheetViewElement.ViewAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_SheetViewType, "normal" );
                        WorksheetView view = (WorksheetView)attributeValue;
                        displayOptions.View = view;
					}
					break;
					#endregion view

					#region showRuler
					// Name = 'showRuler', Type = Boolean, Default = True
					case CustomSheetViewElement.ShowRulerAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        bool showRuler = (bool)attributeValue;
                        displayOptions.ShowRulerInPageLayoutView = showRuler;
					}
					break;
					#endregion showRuler

					#region topLeftCell
					// Name = 'topLeftCell', Type = ST_CellRef, Default = 
					case CustomSheetViewElement.TopLeftCellAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_CellRef, String.Empty );
                        CellRef cellRef = CellRef.FromString( attributeValue as string );
                        unfrozenPaneSettings = displayOptions.UnfrozenPaneSettings;
                        unfrozenPaneSettings.FirstRowInBottomPane = cellRef.Row;
                        unfrozenPaneSettings.FirstColumnInRightPane = cellRef.Column;
					}
					break;
					#endregion topLeftCell
				}
			}

            //  BF 8/8/08
            //  Get the CustomView from the GUID...if we can't, assume that the 
            //  CustomViews collection has not yet been populated (actually at the
            //  time this was implemented, we expect that it hasn't yet)
            Workbook workbook = manager.Workbook;
            CustomViewCollection customViews = workbook.CustomViews;
            CustomView customView = null;            
            customView = customViews[id];            

            if ( customView == null )
            {
				// MD 4/6/12 - TFS102169
				// We will now load the workbook part before the worksheet parts, so the custom view should be created already.
				//customView = new CustomView( manager.Workbook, true, true );
				//customView.Id = id;
				//customViews.Add( customView );
				Utilities.DebugFail("Cannot find the custom sheet view.");
				return;
            }

			// MD 4/6/12 - TFS102169
			#region Old Code

			//////  We can't add the CustomViewDisplayOptions to the dictionary
			//////  yet, because the worksheet's DisplayOptions have yet to be
			//////  serialized in, and when they are, the CustomViewDisplayOptions
			//////  have to be initialized with the worksheet's DisplayOptions,
			//////  which would overwrite any settings we applied here, so we have
			//////  to store these CustomViewDisplayOptions until after the worksheet
			//////  is added to the dictionaries that the CustomView maintains.
			//customView.SetPendingCustomViewDisplayOptions( worksheet, displayOptions );

			////  Populate the ContextStack with the references that will be needed
			////  by the child elements like PaneElement, PrintOptionsElement
			//manager.ContextStack.Push( displayOptions );

			////  Note that we might not ultimately have print options, but we don't
			////  know that yet because the workbook's CustomViews collection has
			////  not been populated yet. Add the reference now, and worry about
			////  whether we need it later.
			//PrintOptions printOptions = new PrintOptions( worksheet );

			////  Add the print options to the "pending" dictionary (see above)
			//customView.SetPendingPrintOptions( worksheet, printOptions );	
			//manager.ContextStack.Push( printOptions );

			#endregion // Old Code
			DisplayOptions actualDisplayOptions = customView.GetDisplayOptions(worksheet);
			if (actualDisplayOptions != null)
			{
				actualDisplayOptions.InitializeFrom(displayOptions);
				manager.ContextStack.Push(actualDisplayOptions);
			}

			PrintOptions actualPrintOptions = customView.GetPrintOptions(worksheet);
			if (actualPrintOptions != null)
				manager.ContextStack.Push(actualPrintOptions);
		}

			#endregion Load

			#region Save

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            //  Get the Worksheet
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get the worksheet from the ContextStack");
                return;
            }
            
            //  Get the ListContext object that holds the CustomViewDisplayOptions list
            ListContext<CustomView> listContext = manager.ContextStack[typeof(ListContext<CustomView>)] as ListContext<CustomView>;
            if (listContext == null)
            {
                Debug.Assert( false, "Could not get ListContext<CustomViewDisplayOptions> in CustomSheetViewElement.Save." );
                return;
            }

            //  Get the current CustomView instance
            CustomView customView = listContext.ConsumeCurrentItem() as CustomView;
            if (customView == null)
                return;

            //  Get the CustomViewDisplayOptions from the CustomView
            CustomViewDisplayOptions displayOptions = customView.GetDisplayOptions( worksheet );
            if (displayOptions == null)
            {
                Debug.Assert( false, "Could not get a CustomViewDisplayOptions from a CustomView that is expected to contain one for this worksheet." );
                return;
            }

            //  BF 8/8/08
            //  Push the DisplayOptions onto the context stack; this is so
            //  the PaneElement class can support parent elements of type
            //  SheetViewElement and CustomSheetViewElement (this one).
            manager.ContextStack.Push( displayOptions );

            //  Push the associated PrintOptions onto the stack as well, if the
            //  CustomView saved them.
            if ( customView.SavePrintOptions )
            {
                PrintOptions printOptions = customView.GetPrintOptions( worksheet );
                manager.ContextStack.Push( printOptions );
            }

            string attributeValue = string.Empty;

			#region Guid
			// Name = 'guid', Type = ST_Guid, Default = 
			attributeValue = XmlElementBase.GetXmlString(customView.Id, DataType.ST_Guid);
			XmlElementBase.AddAttribute(element, CustomSheetViewElement.GuidAttributeName, attributeValue);

			#endregion Guid

			#region Scale
			// Name = 'scale', Type = UInt32, Default = 100

            int scale = displayOptions.MagnificationInCurrentView;
            if ( scale != DisplayOptions.DefaultMagnificationInNormalView )
            {
			    attributeValue = XmlElementBase.GetXmlString(scale, DataType.UInt32);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ScaleAttributeName, attributeValue);
            }
			#endregion Scale

			#region ColorId
			// Name = 'colorId', Type = UInt32, Default = 64
            int gridlineColorIndex = displayOptions.GridlineColorIndex;
			// MD 1/16/12 - 12.1 - Cell Format Updates
            //if ( gridlineColorIndex != WorkbookColorCollection.AutomaticColor )
			if (gridlineColorIndex != WorkbookColorPalette.AutomaticColor)
            {
    			attributeValue = XmlElementBase.GetXmlString(gridlineColorIndex, DataType.UInt32);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ColorIdAttributeName, attributeValue);

            }
			#endregion ColorId

			#region ShowPageBreaks
			// Name = 'showPageBreaks', Type = Boolean, Default = False

            //  RoundTrip - Page 1953
            //bool showPageBreaks = false;
            //attributeValue = XmlElementBase.GetXmlString(showPageBreaks, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomSheetViewElement.ShowPageBreaksAttributeName, attributeValue);

			#endregion ShowPageBreaks

			#region ShowFormulas
			// Name = 'showFormulas', Type = Boolean, Default = False
            bool showFormulas = displayOptions.ShowFormulasInCells;
            if ( showFormulas != false )
            {
			    attributeValue = XmlElementBase.GetXmlString(showFormulas, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ShowFormulasAttributeName, attributeValue);
            }
			#endregion ShowFormulas

			#region ShowGridLines
			// Name = 'showGridLines', Type = Boolean, Default = True

            bool showGridlines = displayOptions.ShowGridlines;
            if ( showGridlines != true )
            {
			    attributeValue = XmlElementBase.GetXmlString(showGridlines, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ShowGridLinesAttributeName, attributeValue);
            }
			#endregion ShowGridLines

			#region ShowRowCol
			// Name = 'showRowCol', Type = Boolean, Default = True

            bool showRowCol = displayOptions.ShowRowAndColumnHeaders;
            if ( showRowCol != true )
            {
			    attributeValue = XmlElementBase.GetXmlString(showRowCol, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ShowRowColAttributeName, attributeValue);
            }
			#endregion ShowRowCol

			#region OutlineSymbols
			// Name = 'outlineSymbols', Type = Boolean, Default = True
            
            attributeValue = XmlElementBase.GetXmlString(displayOptions.ShowOutlineSymbols, DataType.Boolean);
            XmlElementBase.AddAttribute(element, CustomSheetViewElement.OutlineSymbolsAttributeName, attributeValue);
			#endregion OutlineSymbols

			#region ZeroValues
			// Name = 'zeroValues', Type = Boolean, Default = True

            bool zeroValues = displayOptions.ShowZeroValues;
            if ( zeroValues != true )
            {
			    attributeValue = XmlElementBase.GetXmlString(zeroValues, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ZeroValuesAttributeName, attributeValue);
            }
			#endregion ZeroValues

			#region FitToPage
			// Name = '', Type = Boolean, Default = False

            //  RoundTrip - Page 1953
            //bool fitToPage = false;
            //attributeValue = XmlElementBase.GetXmlString(fitToPage, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomSheetViewElement.FitToPageAttributeName, attributeValue);

			#endregion FitToPage

			#region PrintArea
			// Name = 'printArea', Type = Boolean, Default = False

            //  RoundTrip - Page 1953
            //bool printArea = false;
            //attributeValue = XmlElementBase.GetXmlString(printArea, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomSheetViewElement.PrintAreaAttributeName, attributeValue);

			#endregion PrintArea

			#region Filter
			// Name = 'filter', Type = Boolean, Default = False

            //  RoundTrip - Page 1953
            //bool filter = false;
            //attributeValue = XmlElementBase.GetXmlString(filter, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomSheetViewElement.FilterAttributeName, attributeValue);

			#endregion Filter

			#region ShowAutoFilter
			// Name = 'showAutoFilter', Type = Boolean, Default = False

            //  RoundTrip - Page 1953
            //bool showAutoFilter = false;
            //attributeValue = XmlElementBase.GetXmlString(showAutoFilter, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomSheetViewElement.ShowAutoFilterAttributeName, attributeValue);

			#endregion ShowAutoFilter

			#region HiddenRows
			// Name = '', Type = Boolean, Default = False

			HiddenRowCollection hiddenRowsCollection = customView.GetHiddenRows( worksheet );
			bool hiddenRows = hiddenRowsCollection != null && hiddenRowsCollection.Count > 0;
            attributeValue = XmlElementBase.GetXmlString(hiddenRows, DataType.Boolean);
            XmlElementBase.AddAttribute(element, CustomSheetViewElement.HiddenRowsAttributeName, attributeValue);

			#endregion HiddenRows

			#region HiddenColumns
			// Name = 'hiddenColumns', Type = Boolean, Default = False

			HiddenColumnCollection hiddenColumnCollection = customView.GetHiddenColumns( worksheet );
			bool hiddenColumns = hiddenColumnCollection != null && hiddenColumnCollection.Count > 0;
            attributeValue = XmlElementBase.GetXmlString(hiddenColumns, DataType.Boolean);
            XmlElementBase.AddAttribute(element, CustomSheetViewElement.HiddenColumnsAttributeName, attributeValue);

			#endregion HiddenColumns

			#region State
			// Name = 'state', Type = ST_SheetState, Default = visible

            WorksheetVisibility visibility = displayOptions.Visibility;
            if ( visibility != WorksheetVisibility.Visible )
            {
			    attributeValue = XmlElementBase.GetXmlString(visibility, DataType.ST_SheetState);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.StateAttributeName, attributeValue);
            }
			#endregion State

			#region FilterUnique
			// Name = 'filterUnique', Type = Boolean, Default = False

            //  RoundTrip - Page 1953
            //bool filterUnique = false;
            //attributeValue = XmlElementBase.GetXmlString(filterUnique, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomSheetViewElement.FilterUniqueAttributeName, attributeValue);

			#endregion FilterUnique

			#region View
			// Name = 'view', Type = ST_SheetViewType, Default = normal

            WorksheetView view = displayOptions.View;
            if ( view != WorksheetView.Normal )
            {
			    attributeValue = XmlElementBase.GetXmlString(view, DataType.ST_SheetViewType);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ViewAttributeName, attributeValue);
            }
			#endregion View

			#region ShowRuler
			// Name = 'showRuler', Type = Boolean, Default = True

            bool showRuler = displayOptions.ShowRulerInPageLayoutView;
            if ( showRuler != true )
            {
			    attributeValue = XmlElementBase.GetXmlString(showRuler, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomSheetViewElement.ShowRulerAttributeName, attributeValue);
            }
			#endregion ShowRuler

			#region TopLeftCell
			// Name = 'topLeftCell', Type = ST_CellRef, Default = 

            PaneSettingsBase paneSettings = null;
            if ( displayOptions.PanesAreFrozen )
                paneSettings = displayOptions.FrozenPaneSettings;
            else
                paneSettings = displayOptions.UnfrozenPaneSettings;

            CellRef cellRef = new CellRef( paneSettings.FirstRowInBottomPane, paneSettings.FirstColumnInRightPane );
		    attributeValue = cellRef.ToString();
			XmlElementBase.AddAttribute(element, CustomSheetViewElement.TopLeftCellAttributeName, attributeValue);

			#endregion TopLeftCell

            #region Add the child elements

            //  PaneElement
            if ( displayOptions.ShouldSerializeFrozenPaneSettings() ||
                 displayOptions.ShouldSerializeUnfrozenPaneSettings() )
                XmlElementBase.AddElement(element, PaneElement.QualifiedName);

            //  Serialize the printer settings
            if ( customView.SavePrintOptions )
            {
				// MD 5/25/11 - Data Validations / Page Breaks
				//----------------------------------------------
				PrintOptions printOptions = customView.GetPrintOptions(worksheet);

				if ( printOptions == null )
                    Utilities.DebugFail( "Expecting a PrintOptions to be associated with this worksheet." );
                else
                {
					if (printOptions.HasHorizontalPageBreaks)
						XmlElementBase.AddElement(element, RowBreaksElement.QualifiedName);

					if (printOptions.HasVerticalPageBreaks)
						XmlElementBase.AddElement(element, ColBreaksElement.QualifiedName);
				//----------------------------------------------

                //  PageMarginsElement
                //  Note that all the margin attributes are required, so we always
                //  write the element out. Also note that this element MUST APPEAR
                //  BEFORE THE PrintOptionsElement.
                XmlElementBase.AddElement(element, PageMarginsElement.QualifiedName);

				// MD 5/25/11 - Data Validations / Page Breaks
				// Moved above
				//PrintOptions printOptions = customView.GetPrintOptions( worksheet );
				//if ( printOptions == null )
				//    Utilities.DebugFail( "Expecting a PrintOptions to be associated with this worksheet." );
				//else
				//{
                    //  PrintOptionsElement
                    XmlElementBase.AddElement(element, PrintOptionsElement.QualifiedName);

                    //  PageSetupElement
                    if ( printOptions.ShouldSerializePageSetupInfo() )
                        XmlElementBase.AddElement(element, PageSetupElement.QualifiedName);

                    //  HeaderFooterElement
                    if ( string.IsNullOrEmpty(printOptions.Header) == false ||
                         string.IsNullOrEmpty(printOptions.Footer) == false )
                        XmlElementBase.AddElement(element, HeaderFooterElement.QualifiedName);
                }

            }

            #endregion Add the child elements
		}

			#endregion Save

		#endregion Base class overrides

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