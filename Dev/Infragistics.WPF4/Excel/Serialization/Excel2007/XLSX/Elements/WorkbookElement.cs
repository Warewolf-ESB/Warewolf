using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	internal class WorkbookElement : XLSXElementBase
    {
		#region XML Schema Fragment

		//<complexType name="CT_Workbook">
		//    <sequence>
		//        <element name="fileVersion" type="CT_FileVersion" minOccurs="0" maxOccurs="1"/>
		//        <element name="fileSharing" type="CT_FileSharing" minOccurs="0" maxOccurs="1"/>
		//        <element name="workbookPr" type="CT_WorkbookPr" minOccurs="0" maxOccurs="1"/>
		//        <element name="workbookProtection" type="CT_WorkbookProtection" minOccurs="0" maxOccurs="1"/>
		//        <element name="bookViews" type="CT_BookViews" minOccurs="0" maxOccurs="1"/>
		//        <element name="sheets" type="CT_Sheets" minOccurs="1" maxOccurs="1"/>
		//        <element name="functionGroups" type="CT_FunctionGroups" minOccurs="0" maxOccurs="1"/>
		//        <element name="externalReferences" type="CT_ExternalReferences" minOccurs="0" maxOccurs="1"/>
		//        <element name="definedNames" type="CT_DefinedNames" minOccurs="0" maxOccurs="1"/>
		//        <element name="calcPr" type="CT_CalcPr" minOccurs="0" maxOccurs="1"/>
		//        <element name="oleSize" type="CT_OleSize" minOccurs="0" maxOccurs="1"/>
		//        <element name="customWorkbookViews" type="CT_CustomWorkbookViews" minOccurs="0"
		//        maxOccurs="1"/>
		//        <element name="pivotCaches" type="CT_PivotCaches" minOccurs="0" maxOccurs="1"/>
		//        <element name="smartTagPr" type="CT_SmartTagPr" minOccurs="0" maxOccurs="1"/>
		//        <element name="smartTagTypes" type="CT_SmartTagTypes" minOccurs="0" maxOccurs="1"/>
		//        <element name="webPublishing" type="CT_WebPublishing" minOccurs="0" maxOccurs="1"/>
		//        <element name="fileRecoveryPr" type="CT_FileRecoveryPr" minOccurs="0" maxOccurs="unbounded"/>
		//        <element name="webPublishObjects" type="CT_WebPublishObjects" minOccurs="0" maxOccurs="1"/>
		//        <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
		//    </sequence>
		//</complexType>

		#endregion //Xml Schema Fragment

        #region Constants

        /// <summary>
        /// "workbook"
        /// </summary>
        public const string LocalName = "workbook";

        /// <summary>
        /// "http://schemas.openxmlformats.org/spreadsheetml/2006/main/workbook"
        /// </summary>
        public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			WorkbookElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		//private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
		//private const string RelationshipsNamespacePrefix = "r";

        #endregion Constants

        #region Base class overrides

            #region Type
        /// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.workbook; }
        }
            #endregion Type

            #region Load


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
        {
			#region Populate the data item for the owning content type

			ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[ typeof( ChildDataItem ) ];

			if ( dataItem != null )
			{
				Debug.Assert( dataItem.Data == null, "Only the workbook element should have populated the data context." );
				dataItem.Data = manager.Workbook;
			}

			#endregion Populate the data item for the owning content type

			manager.ContextStack.Push( manager.Workbook );
        }
            #endregion Load

			#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		{
			base.OnAfterLoadChildElements( manager, elementCache );

			// MD 4/6/12 - TFS102169
			// Now that the workbook part is loaded before the worksheet parts, this has been moved to WorkbookPartBase.OnLoadComplete.
			// Also, now that the custom views have been loaded, we can load the named references.
			//manager.Workbook.OnAfterLoadGlobalSettings( manager );
			//
			//// MD 3/29/11 - TFS63971
			//manager.OnWorkbookLoaded();
			foreach (Excel2007WorkbookSerializationManager.NamedReferenceInfo info in manager.NamedReferenceInfos)
				manager.AddNonExternalNamedReferenceDuringLoad(info.Reference, info.Hidden);
		} 

			#endregion OnAfterLoadChildElements

            #region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
        {
			manager.ContextStack.Push( manager.Workbook );
			manager.ContextStack.Push( manager.Workbook.WindowOptions );

			XmlElementBase.AddNamespaceDeclaration( 
				element, 
				WorkbookElement.RelationshipsNamespacePrefix, 
				WorkbookElement.RelationshipsNamespace );

            // Add the workbookPr element
            XmlElementBase.AddElement( element, WorkbookPrElement.QualifiedName );

			// MD 11/22/11
			// Found while writing Interop tests
			// Add the workbookProtection element
			if (manager.Workbook.Protected)
				XmlElementBase.AddElement(element, WorkbookProtectionElement.QualifiedName);

            // Add the bookViews element
			XmlElementBase.AddElement( element, BookViewsElement.QualifiedName ); 
           
            // Add the sheets element
            XmlElementBase.AddElement( element, SheetsElement.QualifiedName );

            // Add the externalReferences element
            if (manager.ExternalReferences.Count > 0)
                XmlElementBase.AddElement(element, ExternalReferencesElement.QualifiedName);

            // Add the definedNames element
			if ( manager.Workbook.CurrentWorkbookReference.NamedReferences.Count > 0 )
                XmlElementBase.AddElement(element, DefinedNamesElement.QualifiedName);
           
            //  Add the calcPr element
			XmlElementBase.AddElement( element, CalculationPropertiesElement.QualifiedName);            

            //  BF 8/8/08
            // Add the customWorkbookViews element
            if ( manager.Workbook.HasCustomViews )
                XmlElementBase.AddElement( element, CustomWorkbookViewsElement.QualifiedName );
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