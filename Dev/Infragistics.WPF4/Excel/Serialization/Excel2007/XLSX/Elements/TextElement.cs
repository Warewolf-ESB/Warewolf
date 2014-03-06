using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class TextElement : XLSXElementBase
    {
        #region Constants






        public const string LocalName = "t";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            TextElement.LocalName;

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.t; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//// Get the FormattedString off of the context stack so that we can set the value
			//WorkbookSerializationManager.FormattedStringHolder holder = manager.ContextStack[typeof(WorkbookSerializationManager.FormattedStringHolder)] as WorkbookSerializationManager.FormattedStringHolder;
			//if (holder == null)
			//{
			//    Utilities.DebugFail("Could not get the formatted string holder from the context stack");
			//    return;
			//}
            //FormattedString formattedString = holder.Value;
			//if (formattedString.UnformattedString == null)
			//    formattedString.UnformattedString = value;
			//else
			//    formattedString.UnformattedString += value;
			StringElement formattedStringElement = manager.ContextStack[typeof(StringElement)] as StringElement;
			if (formattedStringElement == null)
			{
				Utilities.DebugFail("Could not get the formatted string element from the context stack");
				return;
			}

			// 1/23/12 - TFS99818
			value = TextElement.UnescapeString(value);

			if (formattedStringElement.UnformattedString == null)
				formattedStringElement.UnformattedString = value;
			else
				formattedStringElement.UnformattedString += value;
        }

        #endregion Load

        #region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            string text = manager.ContextStack[typeof(string)] as string;
            if (text != null)
            {
                // We've already performed the relevant logic within our own object model to break up a formatted string into 
                // its relevant blocks, so we should make sure that we preserve any spaces within these values
                if (text.Trim().Length != text.Length)
                {
                    ExcelXmlAttribute attribute = element.OwnerDocument.CreateAttribute("xml", "space", "http://www.w3.org/XML/1998/namespace");
                    attribute.Value = "preserve";
                    element.Attributes.Add(attribute);
                }

                value = text;
            }
            else
                Utilities.DebugFail("Could not get the string from the context stack");
        }
        #endregion Save

        #endregion Base class overrides

		// 1/23/12 - TFS99818
		#region AppendPreviousCharBlock

		private static void AppendPreviousCharBlock(StringBuilder result, string value, int currentCharIndex, ref int firstCharIndexToAppend)
		{
			result.Append(value, firstCharIndexToAppend, currentCharIndex - firstCharIndexToAppend);
			firstCharIndexToAppend = currentCharIndex + 1;
		}

		#endregion // AppendPreviousCharBlock

		// 1/23/12 - TFS99818
		#region EscapeString

		private static string EscapeString(string value)
		{
			if (value == null)
				return null;

			StringBuilder result = new StringBuilder(value.Length);

			int nextCharToAppend = 0;
			for (int i = 0; i < value.Length; i++)
			{
				char currentChar = value[i];
				if (currentChar == '_')
				{
					if (value.Length <= i + 6 || value[i + 1] != 'x' || value[i + 6] != '_')
						continue;

					TextElement.AppendPreviousCharBlock(result, value, i, ref nextCharToAppend);
					result.Append("_x005F_");
				}

				// Skip past supported characters.
				if (currentChar == 0x9 ||
					currentChar == 0xA ||
					currentChar == 0xD ||
					(0x20 <= currentChar && currentChar <= 0xD7FF) ||
					(0xE000 <= currentChar && currentChar <= 0xFFFD))
				{
					continue;
				}

				
				if (0xFFFF <= currentChar)
					continue;

				TextElement.AppendPreviousCharBlock(result, value, i, ref nextCharToAppend);
				result.AppendFormat("_x{0:X4}_", (int)currentChar);
			}

			if (nextCharToAppend == 0)
				return value;

			TextElement.AppendPreviousCharBlock(result, value, value.Length, ref nextCharToAppend);
			return result.ToString();
		}

		#endregion // EscapeString

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(XmlWriter writer, string value)
		{
			writer.WriteStartElement(TextElement.LocalName);

			// 1/23/12 - TFS99818
			value = TextElement.EscapeString(value);

			XmlElementBase.WriteString(writer, value);
			writer.WriteEndElement();
		}

		#endregion // SaveDirectHelper

		// 1/23/12 - TFS99818
		#region UnescapeString

		private static string UnescapeString(string value)
		{
			if (value == null)
				return null;

			StringBuilder result = new StringBuilder(value.Length);

			int nextCharToAppend = 0;
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] != '_')
					continue;

				if (value.Length <= i + 6 || value[i + 1] != 'x' || value[i + 6] != '_')
					continue;

				string unicodeString = value.Substring(i + 2, 4);
				int unicodeValue;
				if (int.TryParse(unicodeString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out unicodeValue) == false)
				{
					Utilities.DebugFail("This is unexpected.");
					continue;
				}

				TextElement.AppendPreviousCharBlock(result, value, i, ref nextCharToAppend);
				result.Append((char)unicodeValue);

				i += 6;
				nextCharToAppend += 6;
			}

			if (nextCharToAppend == 0)
				return value;

			TextElement.AppendPreviousCharBlock(result, value, value.Length, ref nextCharToAppend);
			return result.ToString();
		}

		#endregion // UnescapeString
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