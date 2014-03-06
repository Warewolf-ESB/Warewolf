using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class ExternalBookElement : XLSXElementBase
    {
        #region Constants






        public const string LocalName = "externalBook";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            ExternalBookElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
        //private const string RelationshipIdAttributeName = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/id";
        //private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        //private const string RelationshipsNamespacePrefix = "r";

        #endregion //Constants

        #region Base Class Overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.externalBook; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            string rId = null;
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                switch (attributeName)
                {
					// MD 10/12/10
					// Found while fixing TFS49853
					// Moved multiply defined constants to a single location.
                    //case ExternalBookElement.RelationshipIdAttributeName:
					case XmlElementBase.RelationshipIdAttributeName:
                        rId = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_RelationshipID, String.Empty);
                        break;
                }
            }

            ExternalLinkPartInfo partInfo = manager.GetRelationshipDataFromActivePart(rId) as ExternalLinkPartInfo;
            if (partInfo != null)
            {
				WorkbookReferenceBase workbookReference = manager.Workbook.GetWorkbookReference(partInfo.FullPath);
				manager.WorkbookReferences.Add(workbookReference);

				ExternalWorkbookReference externalWorkbookReference = workbookReference as ExternalWorkbookReference;
				if (externalWorkbookReference != null)
                {
					// MD 2/23/12 - TFS101504
					// Set the item that was loaded in the part so we can get it later when other parts reference this part.
					ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
					if (dataItem != null && dataItem.Data == null)
						dataItem.Data = externalWorkbookReference;

                    // Push the reference onto the context stack so that the child elements can 
                    // take the appropriate action
					manager.ContextStack.Push(externalWorkbookReference);
                }
                else
					Utilities.DebugFail("Could not get the external reference from the specified part information");
            }
            else
				Utilities.DebugFail("Unable to get the external part information from the specified relationship ID");
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            ExternalWorkbookReference workbook = manager.ContextStack[typeof(ExternalWorkbookReference)] as ExternalWorkbookReference;
            if (workbook == null)
            {
				Utilities.DebugFail("Could not get external workbook off the context stack");
                return;                
            }

            RelationshipIdHolder holder = (RelationshipIdHolder)manager.ContextStack[typeof(RelationshipIdHolder)];
            if (holder == null)
            {
				Utilities.DebugFail("Could not get the relationship id from the context stack");
                return;
            }

            XmlElementBase.AddNamespaceDeclaration(
                element,
                ExternalBookElement.RelationshipsNamespacePrefix,
                ExternalBookElement.RelationshipsNamespace);

            // Add the 'id' attribute
			// MD 10/12/10
			// Found while fixing TFS49853
			// Moved multiply defined constants to a single location.
            //XmlElementBase.AddAttribute(element, ExternalBookElement.RelationshipIdAttributeName, holder.RelationshipId);
			XmlElementBase.AddAttribute(element, XmlElementBase.RelationshipIdAttributeName, holder.RelationshipId);

            // Add the 'sheetNames' element
            if (workbook.WorksheetNames.Count > 0)                
                XmlElementBase.AddElement(element, SheetNamesElement.QualifiedName);    
        
            // Add the 'definedNames' element
            if (workbook.NamedReferences.Count > 0)
                XmlElementBase.AddElement(element, DefinedNamesElement.QualifiedName);

			// MD 7/10/12 - TFS116306
			// Write out the cached external workbook data as well.
			// Add the 'sheetDataSet' element
			if (workbook.WorksheetNames.Count > 0)
				XmlElementBase.AddElement(element, SheetDataSetElement.QualifiedName);
        }
        #endregion //Save

        #endregion //Base Class Overrides
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