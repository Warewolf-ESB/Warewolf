using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class SharedStringTableElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Sst"> 
        //  <sequence> 
        //      <element name="si" type="CT_Rst" minOccurs="0" maxOccurs="unbounded"/> 
        //      <element name="extLst" minOccurs="0" type="CT_ExtensionList"/> 
        //  </sequence> 
        //  <attribute name="count" type="xsd:unsignedInt" use="optional"/> 
        //  <attribute name="uniqueCount" type="xsd:unsignedInt" use="optional"/> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "sst";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SharedStringTableElement.LocalName;

        private const string CountAttributeName = "count";
        private const string UniqueCountAttributeName = "uniqueCount";

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.sst; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            // We can ignore the various attributes of this element since we're going to 
            // calculate them anyway and resave them from our information
        }

        #endregion Load

        #region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 4/28/11
			Utilities.DebugFail("We should have used the SaveDirect overload instead.");

			// MD 2/1/12 - TFS100573
			// Removed this code because we were never expecting it to get hit.
			#region Removed

			//// Add the 'count' attribute
			//string attributeValue = XmlElementBase.GetXmlString(manager.TotalStringsUsedInDocument, DataType.UInt);
			//XmlElementBase.AddAttribute(element, SharedStringTableElement.CountAttributeName, attributeValue);

			//// Add the 'uniqueCount' attribute
			//attributeValue = XmlElementBase.GetXmlString(manager.SharedStringTable.Count, DataType.UInt);
			//XmlElementBase.AddAttribute(element, SharedStringTableElement.UniqueCountAttributeName, attributeValue);

			//// MD 11/3/10 - TFS49093
			//// There's no need to iterate over each object. Just add the elements in bulk.
			//// Add a 'si' element for each entry we have in the table.              
			////foreach (WorkbookSerializationManager.FormattedStringHolder formattedString in manager.SharedStringTable)
			////{
			////    XmlElementBase.AddElement(element, StringItemElement.QualifiedName);                
			////}
			//XmlElementBase.AddElements(element, XLSXElementBase.DefaultXmlNamespace, StringItemElement.LocalName, string.Empty, manager.SharedStringTable.Count);

			//// We need to put a list of the FormattedStringHolders on the context stack
			//// so that the child elements know what to write out
			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////ListContext<WorkbookSerializationManager.FormattedStringHolder> formattedStrings = new ListContext<WorkbookSerializationManager.FormattedStringHolder>(manager.SharedStringTable);
			//ListContext<StringElement> formattedStrings = new ListContext<StringElement>(manager.SharedStringTable);

			//manager.ContextStack.Push(formattedStrings);

			#endregion // Removed
        }
        #endregion Save

		// MD 11/4/10 - TFS49093
		#region SaveDirect

		protected override bool SaveDirect(Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document)
		{
			XmlWriter writer = document.Writer;

			writer.WriteStartElement(string.Empty, SharedStringTableElement.LocalName, XLSXElementBase.DefaultXmlNamespace);

			// Add the 'count' attribute
			string attributeValue = XmlElementBase.GetXmlString(manager.TotalStringsUsedInDocument, DataType.UInt);
			writer.WriteAttributeString(SharedStringTableElement.CountAttributeName, attributeValue);

			// Add the 'uniqueCount' attribute
			// MD 2/1/12 - TFS100573
			// The SharedStringTable collection now only stores additional strings not in the workbook's string table during 
			// a save operation, so use the SharedStringCountDuringSave instead.
			//attributeValue = XmlElementBase.GetXmlString(manager.SharedStringTable.Count, DataType.UInt);
			attributeValue = XmlElementBase.GetXmlString(manager.SharedStringCountDuringSave, DataType.UInt);

			writer.WriteAttributeString(SharedStringTableElement.UniqueCountAttributeName, attributeValue);

			bool shouldRemoveCarriageReturns = manager.Workbook.ShouldRemoveCarriageReturnsOnSave;

			// MD 2/1/12 - TFS100573
			// Rewrote this code to first write out the workbook's string table, in order, and then the manager's string table (which during a 
			// save only stores additional strings for StringBuilders).
			#region Old Code

			//for (int stringIndex = 0; stringIndex < manager.SharedStringTable.Count; stringIndex++)
			//{
			//    StringElement formattedStringElement = manager.SharedStringTable[stringIndex];
			//
			//    if (shouldRemoveCarriageReturns)
			//        formattedStringElement = formattedStringElement.RemoveCarriageReturns();
			//
			//    StringItemElement.SaveDirectHelper(manager, writer, formattedStringElement);
			//}

			#endregion // Old Code
			foreach (StringElement stringElement in manager.Workbook.SharedStringTable)
				SharedStringTableElement.SaveStringElement(manager, stringElement, shouldRemoveCarriageReturns, writer);

			// Write out the extra shared string table, which should only have the strings from string builders.
			for (int i = 0; i < manager.SharedStringTable.Count; i++)
				SharedStringTableElement.SaveStringElement(manager, manager.SharedStringTable[i], shouldRemoveCarriageReturns, writer);

			writer.WriteEndElement();
			return true;
		} 

		#endregion // SaveDirect

        #endregion Base class overrides

		// MD 2/1/12 - TFS100573
		#region SaveStringElement

		private static void SaveStringElement(Excel2007WorkbookSerializationManager manager, StringElement element, bool removeCarriageReturns, XmlWriter writer)
		{
			if (removeCarriageReturns)
				element = element.RemoveCarriageReturns(manager.Workbook);

			StringItemElement.SaveDirectHelper(manager, writer, element);
		}

		#endregion // SaveStringElement
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