using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class ExternalCellElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
        // <complexType name="CT_ExternalCell"> 
        //   <sequence> 
        //   <element name="v" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //   </sequence> 
        //   <attribute name="r" type="ST_CellRef" use="optional"/> 
        //   <attribute name="t" type="ST_CellType" use="optional" default="n"/> 
        //   <attribute name="vm" type="xsd:unsignedInt" use="optional" default="0"/> 
        // </complexType> 
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>cell</summary>
        public const string LocalName = "cell";

        /// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/cell</summary>
        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            ExternalCellElement.LocalName;

        private const string RAttributeName = "r";
        private const string TAttributeName = "t";
        private const string VmAttributeName = "vm";

        #endregion Constants

        #region Base class overrides

        #region Type
		public override XLSXElementType Type
		{
			get { return XLSXElementType.cell; }
		}
		#endregion Type

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			WorksheetReferenceExternal.RowValues rowValues = manager.ContextStack.Get<WorksheetReferenceExternal.RowValues>();
			if (rowValues == null)
			{
				Utilities.DebugFail("Could not get the row values off of the context stack");
				return;
			}

			short columnIndex = -1;
			int rowIndex = -1;
			ST_CellType cellType = ST_CellType.n;

			// Roundtrip - Variables for load
			//uint valueMetadata = 0;

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case ExternalCellElement.RAttributeName:
						string cellRef = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellRef, String.Empty);
						// MD 4/6/12 - TFS101506
						//if (Utilities.ParseA1CellAddress(cellRef, WorkbookFormat.Excel2007, out columnIndex, out rowIndex) == false)
						if (Utilities.ParseA1CellAddress(cellRef, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out columnIndex, out rowIndex) == false)
							Utilities.DebugFail("Could not parse address");

						break;

					case ExternalCellElement.TAttributeName:
						cellType = (ST_CellType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellType, ST_CellType.n);
						break;

					case ExternalCellElement.VmAttributeName:
						// Roundtrip - Page 1930
						//valueMetadata = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);
						break;
				}
			}

			// Push the cell onto the context stack so that the child elements can access it
			// MD 4/12/11 - TFS67084
			//manager.ContextStack.Push(columnIndex);
			manager.ContextStack.Push((ColumnIndex)columnIndex);

			// Push the cell type onto the context stack so that the child value element can parse it
			manager.ContextStack.Push(cellType);
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			// MD 7/10/12 - TFS116306
			// Implemented this override.

			EnumerableContext<KeyValuePair<short, object>> valuesContext =
				manager.ContextStack.Get<EnumerableContext<KeyValuePair<short, object>>>();
			WorksheetReferenceExternal.RowValues rowValues = manager.ContextStack.Get<WorksheetReferenceExternal.RowValues>();

			if (valuesContext == null || rowValues == null)
			{
				Utilities.DebugFail("Could not get the values from the ContextStack");
				return;
			}

			KeyValuePair<short, object> pair = valuesContext.ConsumeCurrentItem();

			string attrbuteValue = CellAddress.GetCellReferenceString(
				rowValues.RowIndex, pair.Key, true, true, manager.Workbook.CurrentFormat, 0, 0, false, CellReferenceMode.A1);
			XmlElementBase.AddAttribute(element, ExternalCellElement.RAttributeName, attrbuteValue);

			ST_CellType cellType;
			string valueToWrite;
			Excel2007WorkbookSerializationManager.GetCellValueInfo(
				pair.Value,
				out cellType,
				out valueToWrite);

			attrbuteValue = XmlElementBase.GetXmlString(cellType, DataType.ST_CellType, ST_CellType.n, false);
			XmlElementBase.AddAttribute(element, ExternalCellElement.TAttributeName, attrbuteValue);

			manager.ContextStack.Push(valueToWrite);
			XmlElementBase.AddElement(element, CellValueElement.QualifiedName);
		}

		#endregion // Save

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