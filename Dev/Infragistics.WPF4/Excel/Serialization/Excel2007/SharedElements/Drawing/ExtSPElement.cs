using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	// MD 2/15/11 - TFS66316



	internal class ExtSPElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_PositiveSize2D">
		// <attribute name="cx" type="ST_PositiveCoordinate" use="required"/>
		// <attribute name="cy" type="ST_PositiveCoordinate" use="required"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>ext</summary>
		public const string LocalName = "ext";

		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/ext</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			ExtSPElement.LocalName;

		private const string CxAttributeName = "cx";
		private const string CyAttributeName = "cy";

		#endregion Constants

		#region Base class overrides

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			CellAnchor cellAnchor = (CellAnchor)manager.ContextStack[typeof(CellAnchor)];
			if (cellAnchor == null)
			{
				Utilities.DebugFail("Couldn't find the CellAnchor on the context stack.");
				return;
			}

			object attributeValue = null;
			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{

					#region cx
					// ***REQUIRED***
					// Name = 'cx', Type = ST_PositiveCoordinate, Default = 
					case ExtSPElement.CxAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Coordinate, 0);
							cellAnchor.ExtentX = (long)attributeValue;
						}
						break;
					#endregion cx

					#region cy
					// ***REQUIRED***
					// Name = 'cy', Type = ST_PositiveCoordinate, Default = 
					case ExtSPElement.CyAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Coordinate, 0);
							cellAnchor.ExtentY = (long)attributeValue;
						}
						break;
					#endregion cy

				}
			}
		}

		#region ElementName

		public override string ElementName
		{
			get { return ExtSPElement.QualifiedName; }
		}

		#endregion ElementName

		#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			CellAnchor cellAnchor = (CellAnchor)manager.ContextStack[typeof(CellAnchor)];
			if (cellAnchor == null)
			{
				Utilities.DebugFail("Couldn't find the CellAnchor on the context stack.");
				return;
			}

			string attributeValue;
			attributeValue = XmlElementBase.GetXmlString(cellAnchor.ExtentX, DataType.ST_Coordinate);
			XmlElementBase.AddAttribute(element, ExtSPElement.CxAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(cellAnchor.ExtentY, DataType.ST_Coordinate);
			XmlElementBase.AddAttribute(element, ExtSPElement.CyAttributeName, attributeValue);
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