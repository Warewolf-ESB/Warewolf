using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	internal class TablePartElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_TablePart">
		//  <attribute ref="r:id" use="required"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "tablePart";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			TablePartElement.LocalName;


		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.tablePart; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];
			if (worksheet == null)
			{
				Utilities.DebugFail("Cannot find the Worksheet on the context stack.");
				return;
			}

			string id = string.Empty;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case XmlElementBase.RelationshipIdAttributeName:
						id = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_RelationshipID, id);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			if (String.IsNullOrEmpty(id))
			{
				Utilities.DebugFail("The id was not present in the tablePart element.");
				return;
			}

			string tablePartPath = manager.GetRelationshipPathFromActivePart(id);
			if (tablePartPath == null)
			{
				Utilities.DebugFail("The relationship id was not in the tablePart element is not valid.");
				return;
			}

			WorksheetTable table = manager.GetPartData(tablePartPath) as WorksheetTable;
			if (table == null)
			{
				Utilities.DebugFail("The data at the relationship id in the tablePart element was not loaded as a WorksheetTable.");
				return;
			}

			worksheet.Tables.InternalAdd(table);
			Debug.Assert(table.Worksheet == worksheet, "The WorksheetTable was not added to the collection correctly.");
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			ListContext<WorksheetTable> tables = (ListContext<WorksheetTable>)manager.ContextStack[typeof(ListContext<WorksheetTable>)];
			if (tables == null)
			{
				Utilities.DebugFail("Cannot find the ListContext<WorksheetTable> on the context stack.");
				return;
			}

			WorksheetTable table = tables.ConsumeCurrentItem();
			string path = manager.GetPartPath(table);
			string id = manager.GetRelationshipId(path);

			string attributeValue = String.Empty;

			// Add the 'id' attribute
			attributeValue = XmlElementBase.GetXmlString(id, DataType.ST_RelationshipID);
			XmlElementBase.AddAttribute(element, XmlElementBase.RelationshipIdAttributeName, attributeValue);
		}

		#endregion // Save

		#endregion // Base Class Overrides
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