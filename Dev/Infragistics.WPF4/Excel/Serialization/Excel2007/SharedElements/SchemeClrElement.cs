using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 1/18/12 - 12.1 - Cell Format Updates
	internal class SchemeClrElement : XmlElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_SchemeColor">
		//  <sequence>
		//    <group ref="EG_ColorTransform" minOccurs="0" maxOccurs="unbounded"/>
		//  </sequence>
		//  <attribute name="val" type="ST_SchemeColorVal" use="required"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "schemeClr";






		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			SchemeClrElement.LocalName;

		private const string ValAttributeName = "val";

		#endregion // Constants

		#region Base Class Overrides

		#region ElementName

		public override string ElementName
		{
			get { return SchemeClrElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			ST_SchemeColorVal? val = null;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case SchemeClrElement.ValAttributeName:
						val = (ST_SchemeColorVal)XmlElementBase.GetAttributeValue(attribute, DataType.ST_SchemeColorVal, ST_SchemeColorVal.lt1);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			if (val.HasValue)
			{
				int valIndex = (int)val.Value;

				
				if (0 <= valIndex && valIndex < manager.Workbook.ThemeColors.Length)
				{
					Color color = manager.Workbook.ThemeColors[valIndex];

					ISolidColorItem solidColorItem = (ISolidColorItem)manager.ContextStack[typeof(ISolidColorItem)];
					if (solidColorItem != null)
						solidColorItem.Color = color;

					ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
					if (dataItem != null)
						dataItem.Data = color;
				}
			}
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			
			Utilities.DebugFail("Implement this record.");

			string attributeValue = String.Empty;

			// Add the 'val' attribute
			//attributeValue = XmlElementBase.GetXmlString(, DataType.ST_SchemeColorVal, default(ST_SchemeColorVal), true);
			//XmlElementBase.AddAttribute(element, SchemeColorElement.ValAttributeName, attributeValue);



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