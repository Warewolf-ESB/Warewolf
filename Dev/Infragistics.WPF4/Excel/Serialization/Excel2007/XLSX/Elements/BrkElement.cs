using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 5/13/11 - Data Validations / Page Breaks
    internal class BrkElement : XLSXElementBase
    {
        #region XML Schema Fragment

		//<complexType name="CT_Break">
		//<attribute name="id" type="xsd:unsignedInt" use="optional" default="0"/>
		//<attribute name="min" type="xsd:unsignedInt" use="optional" default="0"/>
		//<attribute name="max" type="xsd:unsignedInt" use="optional" default="0"/>
		//<attribute name="man" type="xsd:boolean" use="optional" default="false"/>
		//<attribute name="pt" type="xsd:boolean" use="optional" default="false"/>
		//</complexType>

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "brk";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
			BrkElement.LocalName;

		private const string ManAttributeName = "man";
		private const string MaxAttributeName = "max";
		private const string MinAttributeName = "min";
		private const string PtAttributeName = "pt";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.brk; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			CollectionContext pageBreaksContext = (CollectionContext)manager.ContextStack[typeof(CollectionContext)];
			PrintOptions printOptions = (PrintOptions)manager.ContextStack[typeof(PrintOptions)];
			if (pageBreaksContext == null || printOptions == null)
			{
				Utilities.DebugFail("Could not get the collection of page breaks or the print options from the ContextStack.");
				return;
			}

			Type type = pageBreaksContext.GetType();
			Type itemType = type.GetGenericArguments()[0];

			bool isVertical = (itemType == typeof(VerticalPageBreak));

			int id = 0;
			bool man = false;
			int? min = null;
			int? max = null;
			bool pt = false;

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case BrkElement.IdAttributeName:
						id = (int)XmlElementBase.GetValue(attribute.Value, DataType.Int32, 0);
						break;

					case BrkElement.ManAttributeName:
						man = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
						break;

					case BrkElement.MaxAttributeName:
						int maxValue = (int)XmlElementBase.GetValue(attribute.Value, DataType.Int32, 0);
						if (itemType == typeof(HorizontalPageBreak))
						{
							if (maxValue != manager.Workbook.MaxColumnCount - 1)
								max = maxValue;
						}
						else
						{
							if (maxValue != manager.Workbook.MaxRowCount - 1)
								max = maxValue;
						}
						break;

					case BrkElement.MinAttributeName:
						int minValue = (int)XmlElementBase.GetValue(attribute.Value, DataType.Int32, 0);
						if (minValue != 0)
							min = minValue;
						break;

					case BrkElement.PtAttributeName:
						pt = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
						break;

					default:
						Utilities.DebugFail("Unknown attribute");
						break;
				}
			}

			PageBreak pageBreak = isVertical
				? (PageBreak)new VerticalPageBreak(id, min, max)
				: (PageBreak)new HorizontalPageBreak(id, min, max);

			pageBreak.ManuallyCreated = man;
			pageBreak.CreatedByPivotTable = pt;

			// MD 4/6/12 - TFS102169
			// Now that the workbook part is loaded before the worksheet parts, the custom views' and worksheet's PrintAreas
			// collections will already be populated.
			pageBreak.PrintArea = printOptions.PrintAreas.GetPrintArea(pageBreak.Id, pageBreak.Min, pageBreak.Max, isVertical);

			pageBreaksContext.AddItem(pageBreak);
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			CollectionContext pageBreaksContext = (CollectionContext)manager.ContextStack[typeof(CollectionContext)];
			if (pageBreaksContext == null)
			{
				Utilities.DebugFail("Could not get the collection of page breaks from the ContextStack");
				return;
			}

			PageBreak pageBreak = (PageBreak)pageBreaksContext.ConsumeCurrentItem();

			string attributeValue = null;

			attributeValue = XmlElementBase.GetXmlString(pageBreak.Id, DataType.Int32, 0, false);
			XmlElementBase.AddAttribute(element, BrkElement.IdAttributeName, attributeValue);

			if (pageBreak.Min.HasValue)
			{
				attributeValue = XmlElementBase.GetXmlString(pageBreak.Min.Value, DataType.Int32, 0, false);
				XmlElementBase.AddAttribute(element, BrkElement.MinAttributeName, attributeValue);
			}

			if (pageBreak.Max.HasValue)
			{
				attributeValue = XmlElementBase.GetXmlString(pageBreak.Max.Value, DataType.Int32, 0, false);
				XmlElementBase.AddAttribute(element, BrkElement.MaxAttributeName, attributeValue);
			}

			attributeValue = XmlElementBase.GetXmlString(pageBreak.ManuallyCreated, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, BrkElement.ManAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(pageBreak.CreatedByPivotTable, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, BrkElement.PtAttributeName, attributeValue);
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