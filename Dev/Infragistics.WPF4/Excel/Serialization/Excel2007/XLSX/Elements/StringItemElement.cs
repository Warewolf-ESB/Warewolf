using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class StringItemElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Rst"> 
        //  <sequence> 
        //      <element name="t" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //      <element name="r" type="CT_RElt" minOccurs="0" maxOccurs="unbounded"/> 
        //      <element name="rPh" type="CT_PhoneticRun" minOccurs="0" maxOccurs="unbounded"/> 
        //      <element name="phoneticPr" minOccurs="0" maxOccurs="1" type="CT_PhoneticPr"/> 
        //  </sequence> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "si";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            StringItemElement.LocalName;

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.si; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            // We can ignore the various attributes of this element since we're going to 
            // calculate them anyway and resave them from our information

            // Create an empty FormattedStringHolder and put it on the context stack so that 
            // the child elements can populate it and set various properties
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString formattedString = new FormattedString(String.Empty);
			//WorkbookSerializationManager.FormattedStringHolder holder = new WorkbookSerializationManager.FormattedStringHolder(formattedString);
			//
			//manager.SharedStringTable.Add(holder);
			//manager.ContextStack.Push(holder);
			// MD 2/1/12 - TFS100573
			//FormattedStringElement formattedStringElement = new FormattedStringElement(manager.Workbook, String.Empty);
			FormattedStringElement formattedStringElement = new FormattedStringElement(StringElement.EmptyStringUTF8);

			// MD 1/31/12 - TFS100573
			// This was a mistake. We can't add the string to the table yet because its hash code will change as we load it.
			// Now we will add the string in the OnAfterLoadChildElements override.
			//manager.SharedStringTable.Add(formattedStringElement);

			manager.ContextStack.Push(formattedStringElement);
        }

        #endregion Load

		// MD 1/31/12 - TFS100573
		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			FormattedStringElement fseWithFormatting = (FormattedStringElement)manager.ContextStack[typeof(FormattedStringElement)];
			if (fseWithFormatting == null)
			{
				Utilities.DebugFail("Could not get the formatted string element from the context stack");
				return;
			}

			// If the string has no formatting, represent it with the base type which doesn't have a formatting collection.
			StringElement formattedStringElement = fseWithFormatting;
			if (fseWithFormatting.HasFormatting == false)
				formattedStringElement = new StringElement(formattedStringElement.UnformattedStringUTF8);

			manager.SharedStringTable.Add(formattedStringElement);
		}

		#endregion // OnAfterLoadChildElements

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

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////ListContext<WorkbookSerializationManager.FormattedStringHolder> formattedStrings =
			////    manager.ContextStack[typeof(ListContext<WorkbookSerializationManager.FormattedStringHolder>)] as ListContext<WorkbookSerializationManager.FormattedStringHolder>;
			////
			////WorkbookSerializationManager.FormattedStringHolder formattedString = formattedStrings.ConsumeCurrentItem() as WorkbookSerializationManager.FormattedStringHolder;
			//ListContext<StringElement> formattedStrings = manager.ContextStack[typeof(ListContext<StringElement>)] as ListContext<StringElement>;
			//StringElement formattedStringElement = formattedStrings.ConsumeCurrentItem() as StringElement;

			//// MD 11/3/10 - TFS49093
			////if (formattedString != null)
			//if (formattedStringElement != null)
			//{
			//    // MD 11/3/10 - TFS49093
			//    // Also, we are removing the carriage returns on save so we only need to do it once per shared string.
			//    if (manager.Workbook.ShouldRemoveCarriageReturnsOnSave)
			//        formattedStringElement = formattedStringElement.RemoveCarriageReturns();

			//    // MD 11/3/10 - TFS49093
			//    // The formatted string data is now stored on the FormattedStringElement.
			//    //if (formattedString.Value.HasFormatting)
			//    FormattedStringElement fseWithFormatting = formattedStringElement as FormattedStringElement;
			//    if (fseWithFormatting != null && fseWithFormatting.HasFormatting)
			//    {
			//        // MD 11/3/10 - TFS49093
			//        // There's no need to iterate over each object. Just add the elements in bulk.
			//        // Add an 'r' element for each run that we have
			//        //for (int i = 0; i < formattedString.Value.FormattingRuns.Count; i++)
			//        //{
			//        //    XmlElementBase.AddElement(element, RichTextRunElement.QualifiedName);
			//        //}
			//        XmlElementBase.AddElements(element, XLSXElementBase.DefaultXmlNamespace, RichTextRunElement.LocalName, string.Empty, fseWithFormatting.FormattingRuns.Count);

			//        // Add the formatting runs to the context stack so that we can properly
			//        // serialize the formatting of the value
			//        // MD 11/3/10 - TFS49093
			//        // The formatted string data is now stored on the FormattedStringElement.
			//        //ListContext<FormattingRun> formattingRuns = new ListContext<FormattingRun>(formattedString.Value.FormattingRuns);
			//        ListContext<FormattingRun> formattingRuns = new ListContext<FormattingRun>(fseWithFormatting.FormattingRuns);

			//        manager.ContextStack.Push(formattingRuns);                    

			//        // Add the value so that the child elements can calculate the strings
			//        // MD 11/3/10 - TFS49093
			//        // The formatted string data is now stored on the FormattedStringElement.
			//        //manager.ContextStack.Push(formattedString.Value);
			//        manager.ContextStack.Push(formattedStringElement);
			//    }
			//    else
			//    {
			//        // Add the 't' element
			//        // MD 11/3/10 - TFS49093
			//        // We usually add a lot of text elements, so let's not parse through the fully qualified name each time. 
			//        // We know what the local name and prefix are always going to be anyway.
			//        //XmlElementBase.AddElement(element, TextElement.QualifiedName);
			//        XmlElementBase.AddElement(element, XLSXElementBase.DefaultXmlNamespace, TextElement.LocalName, string.Empty);

			//        // Push the text onto the context stack so that the text element can serialize
			//        // MD 11/3/10 - TFS49093
			//        // The formatted string data is now stored on the FormattedStringElement.
			//        //manager.ContextStack.Push(formattedString.Value != null ? formattedString.Value.UnformattedString : String.Empty);
			//        manager.ContextStack.Push(formattedStringElement.UnformattedString);
			//    }  
			//}
			//else
			//    Utilities.DebugFail("Could not get the FormattedStringElement needed for writing the data");

			#endregion // Removed
        }
        #endregion Save

        #endregion Base class overrides

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			Excel2007WorkbookSerializationManager manager,
			XmlWriter writer,
			StringElement stringElement)
		{
			writer.WriteStartElement(StringItemElement.LocalName);

			string value = stringElement.UnformattedString;

			// MD 1/31/12 - TFS100573
			//if (formattedStringElement.HasFormatting)
			FormattedStringElement formattedStringElement = stringElement as FormattedStringElement;
			if (formattedStringElement != null && formattedStringElement.HasFormatting)
			{
				// MD 11/9/11 - TFS85193
				//List<FormattedStringRun> runs = formattedStringElement.FormattingRuns;
				List<FormattingRunBase> runs = formattedStringElement.FormattingRuns;

				int firstRunStartIndex = runs[0].FirstFormattedCharAbsolute;
				if (0 < firstRunStartIndex)
					TextElement.SaveDirectHelper(writer, value.Substring(0, firstRunStartIndex));

				for (int runIndex = 0; runIndex < runs.Count; runIndex++)
				{
					// MD 11/9/11 - TFS85193
					//RichTextRunElement.SaveDirectHelper(manager, writer, runs[runIndex]);
					RichTextRunElement.SaveDirectHelper(manager, writer, (FormattedStringRun)runs[runIndex]);
				}
			}
			else
			{
				TextElement.SaveDirectHelper(writer, value);
			}

			writer.WriteEndElement();
		}

		#endregion // SaveDirectHelper
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