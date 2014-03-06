using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{





	internal class WorkbookViewElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
        //<complexType name="CT_BookView">
        //<sequence>
        //<element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
        //</sequence>
        //<attribute name="visibility" type="ST_Visibility" use="optional" default="visible"/>
        //<attribute name="minimized" type="xsd:boolean" use="optional" default="false"/>
        //<attribute name="showHorizontalScroll" type="xsd:boolean" use="optional" default="true"/>
        //<attribute name="showVerticalScroll" type="xsd:boolean" use="optional" default="true"/>
        //<attribute name="showSheetTabs" type="xsd:boolean" use="optional" default="true"/>
        //<attribute name="xWindow" type="xsd:int" use="optional"/>
        //<attribute name="yWindow" type="xsd:int" use="optional"/>
        //<attribute name="windowWidth" type="xsd:unsignedInt" use="optional"/>
        //<attribute name="windowHeight" type="xsd:unsignedInt" use="optional"/>
        //<attribute name="tabRatio" type="xsd:unsignedInt" use="optional" default="600"/>
        //<attribute name="firstSheet" type="xsd:unsignedInt" use="optional" default="0"/>
        //<attribute name="activeTab" type="xsd:unsignedInt" use="optional" default="0"/>
        //<attribute name="autoFilterDateGrouping" type="xsd:boolean" use="optional" default="true"/>
        //</complexType>
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>
        /// "workbookview"
        /// </summary>
        public const string LocalName = "workbookView";

        /// <summary>
        /// "http://schemas.openxmlformats.org/spreadsheetml/2006/main/workbookView"
        /// </summary>
        public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			WorkbookViewElement.LocalName;

        private const string VisibilityAttributeName = "visibility";
        private const string MinimizedAttributeName = "minimized";
        private const string ShowHorizontalScrollAttributeName = "showHorizontalScroll";
        private const string ShowVerticalScrollAttributeName = "showVerticalScroll";
        private const string ShowSheetTabsAttributeName = "showSheetTabs";
        private const string XWindowAttributeName = "xWindow";
        private const string YWindowAttributeName = "yWindow";
        private const string WindowWidthAttributeName = "windowWidth";
        private const string WindowHeightAttributeName = "windowHeight";
        private const string TabRatioAttributeName = "tabRatio";
        private const string FirstSheetAttributeName = "firstSheet";
        private const string ActiveTabAttributeName = "activeTab";
        private const string AutoFilterDateGroupingAttributeName = "autoFilterDateGrouping";

        #endregion Constants

        #region Base class overrides

        #region Type
        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.workbookView; }
        }
            #endregion Type

            #region Load


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
        {
            Workbook workBook = manager.Workbook;
            WorkbookWindowOptions windowOptions = workBook.WindowOptions;
            object attributeValue = null;
            bool showHorizontalScroll = true;
            bool showVerticalScroll = true;
            int xWindow = 0, yWindow = 0, windowWidth = 0, windowHeight = 0;
            int tabRatio = WorkbookWindowOptions.defaultTabBarWidth;    //  a.k.a., TabBarWidth
            bool minimized = false;
            bool showSheetTabs = true;  //  a.k.a., TabBarVisible
            int firstSheet = 0; //  a.k.a., FirstVisibleTabIndex
            int activeTab = 0; //  a.k.a., SelectedWorksheet

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

                switch ( attributeName )
                {
                    case WorkbookViewElement.VisibilityAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Visibility, WorksheetVisibility.Visible );

                        //  RoundTrip - Page 1925
                        //WorksheetVisibility visibility = attributeValue is WorksheetVisibility ? (WorksheetVisibility)attributeValue : WorksheetVisibility.Visible;
                    }
                    break;

                    case WorkbookViewElement.MinimizedAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        minimized = (bool)attributeValue;
                    }
                    break;

                    case WorkbookViewElement.ShowHorizontalScrollAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showHorizontalScroll = (bool)attributeValue;
                    }
                    break;

                    case WorkbookViewElement.ShowVerticalScrollAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showVerticalScroll = (bool)attributeValue;
                    }
                    break;

                    case WorkbookViewElement.ShowSheetTabsAttributeName:
                    {
                        //  ASSUMPTION: showSheetTabs -> TabBarVisible
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showSheetTabs = (bool)attributeValue;
                    }
                    break;

                    #region BoundsInTwips (xWindow, yWindow, windowWidth, windowHeight)
                    //
                    //  When loading in, if all four elements are present and set to zero,
                    //  the workbook has the default size and position. If a particular element
                    //  is missing, it seems to take on a value of zero. Also, if one or more are
                    //  missing, and one of the elements has an actual value of zero, that actual
                    //  value is used. I'm not sure that they really gave a lot of thought to this,
                    //  except to make sure that the default window size and position is used when
                    //  the elements are all missing.

                    //  Examples:
                    //
                    //  THIS: xWindow="0" yWindow="0" windowWidth="0" windowHeight="0" 
                    //  results in the default window size and position.
                    //
                    //  THIS: xWindow="-100" windowWidth="18855" windowHeight="0" 
                    //  results in an effective bounds of (-100, 0, 18855, 0).
                    //
                    case WorkbookViewElement.XWindowAttributeName:
                    case WorkbookViewElement.YWindowAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Integer, 0 );
                        switch ( attributeName )
                        {
                            case WorkbookViewElement.XWindowAttributeName: { xWindow = (int)attributeValue; }
                            break;

                            case WorkbookViewElement.YWindowAttributeName: { yWindow = (int)attributeValue; }
                            break;
                        }
                    }
                    break;

                    case WorkbookViewElement.WindowWidthAttributeName:
                    case WorkbookViewElement.WindowHeightAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt, 0 );
                        int intValue = Utilities.ToInteger( attributeValue );
                        switch ( attributeName )
                        {
                            case WorkbookViewElement.WindowWidthAttributeName: { windowWidth = intValue; }
                            break;
                            
                            case WorkbookViewElement.WindowHeightAttributeName: { windowHeight = intValue; }
                            break;
                        }
                    }
                    break;
                    #endregion BoundsInTwips (xWindow, yWindow, windowWidth, windowHeight)

                    case WorkbookViewElement.TabRatioAttributeName:
                    {
                        //  ASSUMPTION: tabRatio -> TabBarWidth
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt, tabRatio );
                        tabRatio = Utilities.ToInteger( attributeValue );                        
                    }
                    break;

                    case WorkbookViewElement.FirstSheetAttributeName:
                    {
                        //  ASSUMPTION: firstSheet -> FirstVisibleTabIndex
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt, 0 );
                        firstSheet = Utilities.ToInteger( attributeValue );
                    }
                    break;

                    case WorkbookViewElement.ActiveTabAttributeName:
                    {
                        //  ASSUMPTION: activeTab -> SelectedWorksheet(Index)
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt, 0 );
                        activeTab = Utilities.ToInteger( attributeValue );
                    }
                    break;

                    case WorkbookViewElement.AutoFilterDateGroupingAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );

                        //  RoundTrip - Page 1924
                        //bool autoFilterDateGrouping = (bool)attributeValue;
                    }
                    break;
                }
            }

            Utilities.SetWorkbookWindowOptionsProperties(
                minimized,
                showHorizontalScroll,
                showVerticalScroll,
                showSheetTabs,
                xWindow,
                yWindow,
                windowWidth,
                windowHeight,
                tabRatio,
                firstSheet,
                activeTab,
                windowOptions );

        }
            #endregion Load

            #region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
        {
            //  Get a reference to the WorkbookWindowOptions off the context stack
            WorkbookWindowOptions windowOptions = manager.ContextStack[ typeof(WorkbookWindowOptions) ] as WorkbookWindowOptions;

            string attributeValue = string.Empty;
            string attributeName = string.Empty;

            //  This gets used so often there's no point in making the conversion mutiple times
            string booleanFalse = XmlElementBase.GetXmlString( false, DataType.Boolean);

            #region Visibility
            //<attribute name="visibility" type="ST_Visibility" use="optional" default="visible"/>

            //  RoundTrip - Page 1925
            //  visibility is round-tripped, so get the value that was cached
            //WorksheetVisibility visibility = WorksheetVisibility.Visible;
            //if ( visibility != WorksheetVisibility.Visible )
            //{
            //    attributeValue = XmlElementBase.GetXmlString( visibility, DataType.ST_Visibility, WorksheetVisibility.Visible, true );
                
            //    attributeName = WorkbookViewElement.VisibilityAttributeName;

            //    XmlElementBase.AddAttributeLocal( document, element, attributeName, attributeValue );
            //}
            #endregion Visibility

            #region Minimized
            //<attribute name="minimized" type="xsd:boolean" use="optional" default="false"/>

            bool minimized = windowOptions.Minimized;
            if ( minimized == true )
            {
                attributeValue = XmlElementBase.GetXmlString( true, DataType.Boolean);

                attributeName = WorkbookViewElement.MinimizedAttributeName;

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }

            #endregion Minimized

            #region Scrollbars (showHorizontalScroll, showVerticalScroll)
            //<attribute name="showHorizontalScroll" type="xsd:boolean" use="optional" default="true"/>
            //<attribute name="showVerticalScroll" type="xsd:boolean" use="optional" default="true"/>

            ScrollBars scrollBars = windowOptions.ScrollBars;
            if ( scrollBars != ScrollBars.Both )
            {
                //  If the horizontal scrollbar was explicitly hidden...
                if ( (scrollBars & ScrollBars.Horizontal) == 0 )
                {
                    attributeName = WorkbookViewElement.ShowHorizontalScrollAttributeName;

					XmlElementBase.AddAttribute( element, attributeName, booleanFalse );
                }

                //  If the vertical scrollbar was explicitly hidden...
                if ( (scrollBars & ScrollBars.Vertical) == 0 )
                {
                    attributeName = WorkbookViewElement.ShowVerticalScrollAttributeName;

					XmlElementBase.AddAttribute( element, attributeName, booleanFalse );
                }
            }

            #endregion Scrollbars (showHorizontalScroll, showVerticalScroll)

            #region TabBarVisible (showSheetTabs)
            //<attribute name="showSheetTabs" type="xsd:boolean" use="optional" default="true"/>
            if ( windowOptions.TabBarVisible == false )
            {
                attributeName = WorkbookViewElement.ShowSheetTabsAttributeName;

				XmlElementBase.AddAttribute( element, attributeName, booleanFalse );
            }
            #endregion TabBarVisible (showSheetTabs)

            #region BoundsInTwips
            //<attribute name="xWindow" type="xsd:int" use="optional"/>
            //<attribute name="yWindow" type="xsd:int" use="optional"/>
            //<attribute name="windowWidth" type="xsd:unsignedInt" use="optional"/>
            //<attribute name="windowHeight" type="xsd:unsignedInt" use="optional"/>

            Rectangle boundsInTwips = windowOptions.BoundsInTwips;
            if ( boundsInTwips.IsEmpty == false )
            {
                if ( boundsInTwips.X != 0 )
                {
                    attributeValue = XmlElementBase.GetXmlString( (int)boundsInTwips.X, DataType.Int32 );

                    attributeName = WorkbookViewElement.XWindowAttributeName;

					XmlElementBase.AddAttribute( element, attributeName, attributeValue );
                }

                if ( boundsInTwips.Y != 0 )
                {
                    attributeValue = XmlElementBase.GetXmlString((int)boundsInTwips.Y, DataType.Int32);

                    attributeName = WorkbookViewElement.YWindowAttributeName;

					XmlElementBase.AddAttribute( element, attributeName, attributeValue );
                }

                if ( boundsInTwips.Width != 0 )
                {
                    attributeValue = XmlElementBase.GetXmlString((int)boundsInTwips.Width, DataType.Integer);

                    attributeName = WorkbookViewElement.WindowWidthAttributeName;

					XmlElementBase.AddAttribute( element, attributeName, attributeValue );
                }

                if ( boundsInTwips.Height != 0 )
                {
                    attributeValue = XmlElementBase.GetXmlString((int)boundsInTwips.Height, DataType.Integer);

                    attributeName = WorkbookViewElement.WindowHeightAttributeName;

					XmlElementBase.AddAttribute( element, attributeName, attributeValue );
                }
            }

            #endregion BoundsInTwips

            #region TabBarWidth (tabRatio)
            //<attribute name="tabRatio" type="xsd:unsignedInt" use="optional" default="600"/>

            if ( windowOptions.TabBarWidth != WindowOptions.defaultTabBarWidth )
            {
                attributeValue = XmlElementBase.GetXmlString( windowOptions.TabBarWidth, DataType.Integer );

                attributeName = WorkbookViewElement.TabRatioAttributeName;

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }

            #endregion TabBarWidth (tabRatio)

            #region FirstVisibleTabIndex (firstSheet)
            //<attribute name="firstSheet" type="xsd:unsignedInt" use="optional" default="0"/>

            //  ASSUMPTION: firstSheet -> FirstVisibleTabIndex
            if ( windowOptions.FirstVisibleTabIndex != 0 )
            {
                attributeValue = XmlElementBase.GetXmlString( windowOptions.FirstVisibleTabIndex, DataType.Integer );

                attributeName = WorkbookViewElement.FirstSheetAttributeName;

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion FirstVisibleTabIndex (firstSheet)

            #region SelectedWorksheetIndex (activeTab)
            //<attribute name="firstSheet" type="xsd:unsignedInt" use="optional" default="0"/>

			// MD 9/27/08
			// The selected worksheet will now resolve itself
			//int selectedWorksheetIndex = windowOptions.SelectedWorksheetResolved.Index;
			int selectedWorksheetIndex = windowOptions.SelectedWorksheet.Index;

			if ( selectedWorksheetIndex != 0 )
            {
				attributeValue = XmlElementBase.GetXmlString( selectedWorksheetIndex, DataType.Integer );

                attributeName = WorkbookViewElement.ActiveTabAttributeName;

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion SelectedWorksheetIndex (activeTab)

            #region (autoFilterDateGrouping)
            //<attribute name="autoFilterDateGrouping" type="xsd:boolean" use="optional" default="true"/>

            //  RoundTrip - Page 1924
            //if ( autoFilterDateGrouping == false )
            //{
            //    attributeName = WorkbookViewElement.AutoFilterDateGroupingAttributeName;

            //    XmlElementBase.AddAttributeLocal( document, element, attributeName, booleanFalse );
            //}
            #endregion (autoFilterDateGrouping)

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