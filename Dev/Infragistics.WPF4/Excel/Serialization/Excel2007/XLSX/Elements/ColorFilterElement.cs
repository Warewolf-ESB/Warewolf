using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Filtering;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class ColorFilterElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_ColorFilter">
		//  <attribute name="dxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="cellColor" type="xsd:boolean" use="optional" default="true"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "colorFilter";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			ColorFilterElement.LocalName;

		private const string CellColorAttributeName = "cellColor";
		private const string DxfIdAttributeName = "dxfId";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.colorFilter; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			FilterColumnElementContext filterContext = (FilterColumnElementContext)manager.ContextStack[typeof(FilterColumnElementContext)];
			if (filterContext == null)
			{
				Utilities.DebugFail("Cannot find the FilterColumnElementContext on the context stack.");
				return;
			}

			bool cellColor = true;
			uint dxfId = default(uint);

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case ColorFilterElement.CellColorAttributeName:
						cellColor = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, cellColor);
						break;

					case ColorFilterElement.DxfIdAttributeName:
						dxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, dxfId);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			WorksheetCellFormatData format = manager.Dxfs[(int)dxfId];
			if (cellColor)
				filterContext.Filter = FillFilter.CreateFillFilter(null, format);
			else
				filterContext.Filter = FontColorFilter.CreateFontColorFilter(null, format);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			IColorFilter filter = (IColorFilter)manager.ContextStack[typeof(IColorFilter)];
			if (filter == null)
			{
				Utilities.DebugFail("Cannot find the ColorFilter on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'cellColor' attribute
			attributeValue = XmlElementBase.GetXmlString(filter.IsCellColorFilter, DataType.Boolean, true, false);
			XmlElementBase.AddAttribute(element, ColorFilterElement.CellColorAttributeName, attributeValue);

			WorksheetCellFormatData format = filter.GetDxf(manager);
			uint dxfId = manager.AddDxf(format);

			// Add the 'dxfId' attribute
			attributeValue = XmlElementBase.GetXmlString(dxfId, DataType.ST_DxfId);
			XmlElementBase.AddAttribute(element, ColorFilterElement.DxfIdAttributeName, attributeValue);
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