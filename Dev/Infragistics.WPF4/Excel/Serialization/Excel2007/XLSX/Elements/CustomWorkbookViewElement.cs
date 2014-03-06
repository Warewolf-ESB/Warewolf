using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{



	internal class CustomWorkbookViewElement : XLSXElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_CustomWorkbookView">
		// <sequence>
		// <element name="extLst" minOccurs="0" type="CT_ExtensionList"/>
		// </sequence>
		// <attribute name="name" type="ST_Xstring" use="required"/>
		// <attribute name="guid" type="ST_Guid" use="required"/>
		// <attribute name="autoUpdate" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="mergeInterval" type="xsd:unsignedInt" use="optional"/>
		// <attribute name="changesSavedWin" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="onlySync" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="personalView" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="includePrintSettings" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="includeHiddenRowCol" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="maximized" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="minimized" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="showHorizontalScroll" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="showVerticalScroll" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="showSheetTabs" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="xWindow" type="xsd:int" use="optional" default="0"/>
		// <attribute name="yWindow" type="xsd:int" use="optional" default="0"/>
		// <attribute name="windowWidth" type="xsd:unsignedInt" use="required"/>
		// <attribute name="windowHeight" type="xsd:unsignedInt" use="required"/>
		// <attribute name="tabRatio" type="xsd:unsignedInt" use="optional" default="600"/>
		// <attribute name="activeSheetId" type="xsd:unsignedInt" use="required"/>
		// <attribute name="showFormulaBar" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="showStatusbar" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="showComments" type="ST_Comments" use="optional" default="commIndicator"/>
		// <attribute name="showObjects" type="ST_Objects" use="optional" default="all"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>customWorkbookView</summary>
		public const string LocalName = "customWorkbookView";
		
		/// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/customWorkbookView</summary>
		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CustomWorkbookViewElement.LocalName;

		private const string NameAttributeName = "name";
		private const string GuidAttributeName = "guid";
		private const string AutoUpdateAttributeName = "autoUpdate";
		private const string MergeIntervalAttributeName = "mergeInterval";
		private const string ChangesSavedWinAttributeName = "changesSavedWin";
		private const string OnlySyncAttributeName = "onlySync";
		private const string PersonalViewAttributeName = "personalView";
		private const string IncludePrintSettingsAttributeName = "includePrintSettings";
		private const string IncludeHiddenRowColAttributeName = "includeHiddenRowCol";
		private const string MaximizedAttributeName = "maximized";
		private const string MinimizedAttributeName = "minimized";
		private const string ShowHorizontalScrollAttributeName = "showHorizontalScroll";
		private const string ShowVerticalScrollAttributeName = "showVerticalScroll";
		private const string ShowSheetTabsAttributeName = "showSheetTabs";
		private const string XWindowAttributeName = "xWindow";
		private const string YWindowAttributeName = "yWindow";
		private const string WindowWidthAttributeName = "windowWidth";
		private const string WindowHeightAttributeName = "windowHeight";
		private const string TabRatioAttributeName = "tabRatio";
		private const string ActiveSheetIdAttributeName = "activeSheetId";
		private const string ShowFormulaBarAttributeName = "showFormulaBar";
		private const string ShowStatusbarAttributeName = "showStatusbar";
		private const string ShowCommentsAttributeName = "showComments";
		private const string ShowObjectsAttributeName = "showObjects";

		#endregion Constants

		#region Base class overrides

			#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.customWorkbookView; }
		}

			#endregion Type

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			Workbook workBook = manager.Workbook;
			object attributeValue = null;

            bool maximized = false;
            bool showHorizontalScroll = true;
            bool showVerticalScroll = true;
            int xWindow = 0, yWindow = 0, windowWidth = 0, windowHeight = 0;
            int tabRatio = WorkbookWindowOptions.defaultTabBarWidth;    //  a.k.a., TabBarWidth
            bool showSheetTabs = true;  //  a.k.a., TabBarVisible
            int activeSheetId = 0;  //  a.k.a., SelectedWorksheetTabId
            bool showFormulaBar = true;
            bool showStatusbar = true;
            ObjectDisplayStyle showObjects = ObjectDisplayStyle.ShowAll;

            //  BF 8/8/08
            //  Since Worksheets are processed before Workbooks, the CustomViews collection
            //  should theoretically already have been populated at this point, because the
            //  CustomSheetViewElement adds new instances as it needs them. To accommodate
            //  that, get the GUID up front and try to find an existing CustomView instance
            //  with that GUID; if we find it (theoretically we should always find it), we
            //  will use that instead of creating a new instance.
            ExcelXmlAttribute guidAttribute = element.Attributes[CustomWorkbookViewElement.GuidAttributeName];
	        attributeValue = XmlElementBase.GetAttributeValue( guidAttribute, DataType.ST_Guid, Guid.Empty );
            Guid guid = (Guid)attributeValue;
            CustomView customView = workBook.CustomViews[guid];
                        
            if (customView == null)
            {
				// MD 4/30/11
				// If there was a customer view and all worksheet for which it had data were deleted, we would not have a custom view at 
				// that id yet, so create it.
				//Utilities.DebugFail("We should have created the custom view by now");
				//return;
				customView = new CustomView(manager.Workbook, true, true);
				customView.Id = guid;
				workBook.CustomViews.Add(customView);
            }

            Debug.Assert(customView.Id == guid, "Expected the view to have the same guid");   

            CustomViewWindowOptions windowOptions = customView.WindowOptions;                    
            Rectangle defaultBounds = CustomViewWindowOptions.defaultBoundsInPixels;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					// ***REQUIRED***
					// Name = 'name', Type = ST_Xstring, Default = 
					case CustomWorkbookViewElement.NameAttributeName:
					{
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Xstring, string.Empty );
                        string name = attributeValue as string;
                        customView.Name = name;
					}
					break;

					// ***REQUIRED***
					// Name = 'guid', Type = ST_Guid, Default = 
					case CustomWorkbookViewElement.GuidAttributeName:
					{
                        //  We do this before entering this loop (see above)
                        //attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Guid, "null" );
                        //Guid guid = (Guid)attributeValue;
                        //customView.Id = guid;
					}
					break;

					// Name = 'autoUpdate', Type = Boolean, Default = False
					case CustomWorkbookViewElement.AutoUpdateAttributeName:
					{
                        //  RoundTrip - Page 1885
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool autoUpdate = (bool)attributeValue
					}
					break;

					// Name = 'mergeInterval', Type = UInt32, Default = 
					case CustomWorkbookViewElement.MergeIntervalAttributeName:
					{
                        //  RoundTrip - Page 1885
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, null );
                        //int mergeInterval = Utilities.ToInteger(attributeValue);
					}
					break;

					// Name = 'changesSavedWin', Type = Boolean, Default = False
					case CustomWorkbookViewElement.ChangesSavedWinAttributeName:
					{
                        //  RoundTrip - Page 1885
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool changesSavedWin = (bool)attributeValue;
					}
					break;

					// Name = 'onlySync', Type = Boolean, Default = False
					case CustomWorkbookViewElement.OnlySyncAttributeName:
					{
                        //  RoundTrip - Page 1885
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool onlySync = (bool)attributeValue;
					}
					break;

					// Name = 'personalView', Type = Boolean, Default = False
					case CustomWorkbookViewElement.PersonalViewAttributeName:
					{
                        //  RoundTrip - Page 1885
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool personalView = (bool)attributeValue;
					}
					break;

					// Name = 'includePrintSettings', Type = Boolean, Default = True
					case CustomWorkbookViewElement.IncludePrintSettingsAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        customView.SetSavePrintOptions( (bool)attributeValue );
					}
					break;

					// Name = 'includeHiddenRowCol', Type = Boolean, Default = True
					case CustomWorkbookViewElement.IncludeHiddenRowColAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        customView.SetSaveHiddenRowsAndColumns( (bool)attributeValue );
					}
					break;

					// Name = 'maximized', Type = Boolean, Default = False
					case CustomWorkbookViewElement.MaximizedAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        maximized = (bool)attributeValue;
					}
					break;

					// Name = 'minimized', Type = Boolean, Default = False
					case CustomWorkbookViewElement.MinimizedAttributeName:
					{
                        //  RoundTrip - Page 1885
                        //attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        //bool minimized = (bool)attributeValue;
					}
					break;

					// Name = 'showHorizontalScroll', Type = Boolean, Default = True
					case CustomWorkbookViewElement.ShowHorizontalScrollAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showHorizontalScroll = (bool)attributeValue;
					}
					break;

					// Name = 'showVerticalScroll', Type = Boolean, Default = True
					case CustomWorkbookViewElement.ShowVerticalScrollAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showVerticalScroll = (bool)attributeValue;
					}
					break;

					// Name = 'showSheetTabs', Type = Boolean, Default = True
					case CustomWorkbookViewElement.ShowSheetTabsAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showSheetTabs = (bool)attributeValue;
					}
					break;

					// Name = 'xWindow', Type = Int32, Default = 0
					case CustomWorkbookViewElement.XWindowAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Int32, defaultBounds.X );
                        xWindow = Utilities.ToInteger(attributeValue);
					}
					break;

					// Name = 'yWindow', Type = Int32, Default = 0
					case CustomWorkbookViewElement.YWindowAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Int32, defaultBounds.Y );
                        yWindow = Utilities.ToInteger(attributeValue);
					}
					break;

					// ***REQUIRED***
					// Name = 'windowWidth', Type = UInt32, Default = 
					case CustomWorkbookViewElement.WindowWidthAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, defaultBounds.Width );
                        windowWidth = Utilities.ToInteger(attributeValue);
					}
					break;

					// ***REQUIRED***
					// Name = 'windowHeight', Type = UInt32, Default = 
					case CustomWorkbookViewElement.WindowHeightAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, defaultBounds.Height );
                        windowHeight = Utilities.ToInteger(attributeValue);
					}
					break;

					// Name = 'tabRatio', Type = UInt32, Default = 600
					case CustomWorkbookViewElement.TabRatioAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, CustomViewWindowOptions.defaultTabBarWidth );
                        tabRatio = Utilities.ToInteger(attributeValue);
					}
					break;

					// ***REQUIRED***
					// Name = 'activeSheetId', Type = UInt32, Default = 
					case CustomWorkbookViewElement.ActiveSheetIdAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, 0 );
                        activeSheetId = Utilities.ToInteger(attributeValue);
					}
					break;

					// Name = 'showFormulaBar', Type = Boolean, Default = True
					case CustomWorkbookViewElement.ShowFormulaBarAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showFormulaBar = (bool)attributeValue;
					}
					break;

					// Name = 'showStatusbar', Type = Boolean, Default = True
					case CustomWorkbookViewElement.ShowStatusbarAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        showStatusbar = (bool)attributeValue;
					}
					break;

					// Name = 'showComments', Type = ST_Comments, Default = commIndicator
					case CustomWorkbookViewElement.ShowCommentsAttributeName:
					{
                        //  RoundTrip - Page 1885
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Comments, CommentDisplayType.Indicator );
                        //CommentDisplayStyle showComments = (CommentDisplayStyle)attributeValue;
					}
					break;

					// Name = 'showObjects', Type = ST_Objects, Default = all
					case CustomWorkbookViewElement.ShowObjectsAttributeName:
					{                        
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Objects, ObjectDisplayStyle.ShowAll );
                        showObjects = (ObjectDisplayStyle)attributeValue;
					}
					break;
				}
			}

            //  Set the properties on the associated CustomViewWindowOptions
            Utilities.SetCustomViewWindowOptionsProperties(
                maximized,
                showFormulaBar,
                showStatusbar,
                showHorizontalScroll,
                showVerticalScroll,
                showSheetTabs,
                tabRatio,
                activeSheetId,
                xWindow,
                yWindow,
                windowWidth,
                windowHeight,
                showObjects,
                windowOptions );
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            //  Get the ListContext object that holds the contents of the CustomViews collection
            ListContext<CustomView> listContext = manager.ContextStack[typeof(ListContext<CustomView>)] as ListContext<CustomView>;
            if (listContext == null)
            {
                Debug.Assert( false, "Could not get ListContext<CustomView> in CustomWorkbookViewElement.Save." );
                return;
            }

            //  Get the current CustomView
            CustomView customView = listContext.ConsumeCurrentItem() as CustomView;
            if (customView == null)
                return;
           
			Workbook workbook = manager.ContextStack[ typeof(Workbook) ] as Workbook;
            CustomViewWindowOptions windowOptions = customView.WindowOptions;
            string attributeValue = String.Empty;

            Rectangle defaultBounds = CustomViewWindowOptions.defaultBoundsInPixels;

			#region Name
			// Name = 'name', Type = ST_Xstring, Default = 
            string name = customView.Name;
            if ( string.IsNullOrEmpty(name) == false )
            {
                attributeValue = XmlElementBase.GetXmlString(customView.Name, DataType.ST_Xstring);
                XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.NameAttributeName, attributeValue);
            }
            else
                Debug.Assert( false, "Empty name in CustomWorkbookViewElement.Save - the name is required, so this is bad." );
			#endregion Name

			#region Guid
			// Name = 'guid', Type = ST_Guid, Default = 
            Guid id = customView.Id;
            if ( Guid.Equals(id, Guid.Empty) == false )
            {
                attributeValue = XmlElementBase.GetXmlString(customView.Id, DataType.ST_Guid);
                XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.GuidAttributeName, attributeValue);
            }
            else
                Debug.Assert( false, "Empty GUID in CustomWorkbookViewElement.Save - the GUID is required, so this is bad." );
			#endregion Guid

			#region AutoUpdate
			// Name = 'autoUpdate', Type = Boolean, Default = False

            // RoundTrip - Page 1885
            //bool autoUpdate = false;
            //attributeValue = XmlElementBase.GetXmlString(autoUpdate, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.AutoUpdateAttributeName, autoUpdate);

			#endregion AutoUpdate

			#region MergeInterval
			// Name = 'mergeInterval', Type = UInt32, Default = 

            // RoundTrip - Page 1885
			//int mergeInterval = 0;
            //attributeValue = XmlElementBase.GetXmlString(mergeInterval, DataType.Boolean);
			//XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.MergeIntervalAttributeName, attributeValue);

			#endregion MergeInterval

			#region ChangesSavedWin
			// Name = 'changesSavedWin', Type = Boolean, Default = False
			
            // RoundTrip - Page 1885
			//bool changesSavedWin = false;
            //attributeValue = XmlElementBase.GetXmlString(changesSavedWin, DataType.Boolean);
			//XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ChangesSavedWinAttributeName, attributeValue);

			#endregion ChangesSavedWin

			#region OnlySync
			// Name = 'onlySync', Type = Boolean, Default = False

            // RoundTrip - Page 1885
			//bool onlySync = false;
            //attributeValue = XmlElementBase.GetXmlString(onlySync, DataType.Boolean);
			//XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.OnlySyncAttributeName, attributeValue);

			#endregion OnlySync

			#region PersonalView
			// Name = 'personalView', Type = Boolean, Default = False

            // RoundTrip - Page 1885
			//bool personalView = false;
            //attributeValue = XmlElementBase.GetXmlString(personalView, DataType.Boolean);
			//XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.PersonalViewAttributeName, attributeValue);

			#endregion PersonalView

			#region IncludePrintSettings
			// Name = 'includePrintSettings', Type = Boolean, Default = True
			
            attributeValue = XmlElementBase.GetXmlString(customView.SavePrintOptions, DataType.Boolean);
			XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.IncludePrintSettingsAttributeName, attributeValue);

			#endregion IncludePrintSettings

			#region IncludeHiddenRowCol
			// Name = 'includeHiddenRowCol', Type = Boolean, Default = True
			
            attributeValue = XmlElementBase.GetXmlString(customView.SaveHiddenRowsAndColumns, DataType.Boolean);
			XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.IncludeHiddenRowColAttributeName, attributeValue);

			#endregion IncludeHiddenRowCol

			#region Maximized
			// Name = 'maximized', Type = Boolean, Default = False

            // RoundTrip - Page 1885
            //bool maximized = false;
            //attributeValue = XmlElementBase.GetXmlString(maximized, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.MaximizedAttributeName, attributeValue);

			#endregion Maximized

			#region Minimized
			// Name = 'minimized', Type = Boolean, Default = False

            // RoundTrip - Page 1885
            //bool minimized = false;
            //attributeValue = XmlElementBase.GetXmlString(minimized, DataType.Boolean);
            //XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.MinimizedAttributeName, attributeValue);

			#endregion Minimized

			#region ShowHorizontalScroll/ShowVerticalScroll
			// Name = 'showHorizontalScroll', Type = Boolean, Default = True
			// Name = 'showVerticalScroll', Type = Boolean, Default = True
            ScrollBars scrollbars = windowOptions.ScrollBars;
            bool showHScroll = (scrollbars & ScrollBars.Horizontal) == ScrollBars.Horizontal;
            bool showVScroll = (scrollbars & ScrollBars.Vertical) == ScrollBars.Vertical;
			
            if ( showHScroll != true )
            {
                attributeValue = XmlElementBase.GetXmlString(showHScroll, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ShowHorizontalScrollAttributeName, attributeValue);
            }

            if ( showVScroll != true )
            {
                attributeValue = XmlElementBase.GetXmlString(showVScroll, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ShowVerticalScrollAttributeName, attributeValue);
            }
			#endregion ShowHorizontalScroll/ShowVerticalScroll

			#region ShowSheetTabs
			// Name = 'showSheetTabs', Type = Boolean, Default = True

            bool tabBarVisible = windowOptions.TabBarVisible;
            if ( tabBarVisible != true )
            {
			    attributeValue = XmlElementBase.GetXmlString(tabBarVisible, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ShowSheetTabsAttributeName, attributeValue);
            }
			#endregion ShowSheetTabs

			#region XWindow/yWIndow/windowWidth/windowHeight

			// Name = 'xWindow', Type = Int32, Default = 0
			// Name = 'yWindow', Type = Int32, Default = 0
			// Name = 'windowWidth', Type = UInt32, Default = 
			// Name = 'windowHeight', Type = UInt32, Default = 

            Rectangle boundsInPixels = windowOptions.BoundsInPixels;

            if ( boundsInPixels.X != defaultBounds.X )
            {
			    attributeValue = XmlElementBase.GetXmlString((int)boundsInPixels.X, DataType.Int32);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.XWindowAttributeName, attributeValue);
            }

            if ( boundsInPixels.Y != defaultBounds.Y )
            {
                attributeValue = XmlElementBase.GetXmlString((int)boundsInPixels.Y, DataType.Int32);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.YWindowAttributeName, attributeValue);
            }

            //  Note that windowWidth and windowHeight are required, so we always write them out.
            attributeValue = XmlElementBase.GetXmlString((int)boundsInPixels.Width, DataType.Int32);
		    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.WindowWidthAttributeName, attributeValue);

            attributeValue = XmlElementBase.GetXmlString((int)boundsInPixels.Height, DataType.Int32);
		    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.WindowHeightAttributeName, attributeValue);

			#endregion XWindow/yWIndow/windowWidth/windowHeight

			#region TabRatio
			// Name = 'tabRatio', Type = UInt32, Default = 600

            int tabBarWidth = windowOptions.TabBarWidth;
            if ( tabBarWidth != CustomViewWindowOptions.defaultTabBarWidth )
            {
			    attributeValue = XmlElementBase.GetXmlString(tabBarWidth, DataType.UInt32);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.TabRatioAttributeName, attributeValue);
            }
			#endregion TabRatio

			#region ActiveSheetId
			// Name = 'activeSheetId', Type = UInt32, Default = 

            //  Note that activeSheetId is required, so we always write it out.
			// MD 9/27/08
			// The selected worksheet will now resolve itself
			//Worksheet selectedWorksheet = windowOptions.SelectedWorksheetResolved;
			Worksheet selectedWorksheet = windowOptions.SelectedWorksheet;

			int sheetId = selectedWorksheet == null ? 0 : selectedWorksheet.SheetId;
			attributeValue = XmlElementBase.GetXmlString( sheetId, DataType.UInt32 );
		    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ActiveSheetIdAttributeName, attributeValue);
			#endregion ActiveSheetId

			#region ShowFormulaBar
			// Name = 'showFormulaBar', Type = Boolean, Default = True

            bool showFormulaBar = windowOptions.ShowFormulaBar;
            if ( showFormulaBar != true )
            {
			    attributeValue = XmlElementBase.GetXmlString(showFormulaBar, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ShowFormulaBarAttributeName, attributeValue);
            }
			#endregion ShowFormulaBar

			#region ShowStatusbar
			// Name = 'showStatusbar', Type = Boolean, Default = True

            bool showStatusBar = windowOptions.ShowStatusBar;
            if ( showStatusBar != true )
            {
			    attributeValue = XmlElementBase.GetXmlString(showStatusBar, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ShowStatusbarAttributeName, attributeValue);
            }
			#endregion ShowStatusbar

			#region ShowComments
			// Name = 'showComments', Type = ST_Comments, Default = commIndicator
            
            //  RoundTrip - Page 1885
            //CommentDisplayStyle showComments = CommentDisplayStyle.Indicator;
            //attributeValue = XmlElementBase.GetXmlString(showComments, DataType.ST_Comments);
            //XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ShowCommentsAttributeName, attributeValue);

			#endregion ShowComments

			#region ShowObjects
			// Name = 'showObjects', Type = ST_Objects, Default = all
                        
            attributeValue = XmlElementBase.GetXmlString(customView.WindowOptions.ObjectDisplayStyle, DataType.ST_Objects);
            XmlElementBase.AddAttribute(element, CustomWorkbookViewElement.ShowObjectsAttributeName, attributeValue);

			#endregion ShowObjects
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