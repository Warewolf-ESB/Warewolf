using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 3/22/11 - TFS66776



    internal class SheetProtectionElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
		//<complexType name="CT_SheetProtection">
		//    <attribute name="password" type="ST_UnsignedShortHex" use="optional"/>
		//    <attribute name="sheet" type="xsd:boolean" use="optional" default="false"/>
		//    <attribute name="objects" type="xsd:boolean" use="optional" default="false"/>
		//    <attribute name="scenarios" type="xsd:boolean" use="optional" default="false"/>
		//    <attribute name="formatCells" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="formatColumns" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="formatRows" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="insertColumns" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="insertRows" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="insertHyperlinks" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="deleteColumns" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="deleteRows" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="selectLockedCells" type="xsd:boolean" use="optional" default="false"/>
		//    <attribute name="sort" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="autoFilter" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="pivotTables" type="xsd:boolean" use="optional" default="true"/>
		//    <attribute name="selectUnlockedCells" type="xsd:boolean" use="optional" default="false"/>
		//</complexType>
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>sheetPr</summary>
		public const string LocalName = "sheetProtection";

		/// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/sheetProtection</summary>
        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
			SheetProtectionElement.LocalName;

        private const string ObjectsAttributeName = "objects";
		private const string ScenariosAttributeName = "scenarios";
        private const string SheetAttributeName = "sheet";

        #endregion Constants

        #region Base class overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.sheetProtection; }
        }

        #endregion Type

        #region Load

        /// <summary>Loads the data for this element from the specified manager.</summary>
        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			// MD 10/1/08 - TFS8471
			// We now need a reference to the worksheet
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];
			if ( worksheet == null )
			{
				Utilities.DebugFail( "Could not get the worksheet off the context stack" );
				return;
			}

			Workbook workBook = manager.Workbook;
			object attributeValue = null;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch (attributeName)
				{
					case SheetProtectionElement.SheetAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
							worksheet.Protected = (bool)attributeValue;
						}
						break;
				}
			}
		}

        #endregion Load

        #region Save

        /// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
			if (worksheet == null)
			{
				Utilities.DebugFail("Could not get the worksheet off the context stack");
				return;
			}

			string attributeValue = string.Empty;
			if (worksheet.Protected)
			{
				attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean, false, false);
				XmlElementBase.AddAttribute(element, SheetProtectionElement.SheetAttributeName, attributeValue);

				attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean, false, false);
				XmlElementBase.AddAttribute(element, SheetProtectionElement.ObjectsAttributeName, attributeValue);

				attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean, false, false);
				XmlElementBase.AddAttribute(element, SheetProtectionElement.ScenariosAttributeName, attributeValue);
			}
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