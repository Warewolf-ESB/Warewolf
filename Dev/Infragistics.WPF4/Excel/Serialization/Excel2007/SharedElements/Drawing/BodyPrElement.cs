using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	// MD 11/8/11 - TFS85193
	internal class BodyPrElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		//<complexType name="CT_TextBodyProperties">
		//    <sequence>
		//        <element name="prstTxWarp" type="CT_PresetTextShape" minOccurs="0" maxOccurs="1"/>
		//        <group ref="EG_TextAutofit" minOccurs="0" maxOccurs="1"/>
		//        <element name="scene3d" type="CT_Scene3D" minOccurs="0" maxOccurs="1"/>
		//        <group ref="EG_Text3D" minOccurs="0" maxOccurs="1"/>
		//        <element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		//    </sequence>
		//    <attribute name="rot" type="ST_Angle" use="optional"/>
		//    <attribute name="spcFirstLastPara" type="xsd:boolean" use="optional"/>
		//    <attribute name="vertOverflow" type="ST_TextVertOverflowType" use="optional"/>
		//    <attribute name="horzOverflow" type="ST_TextHorzOverflowType" use="optional"/>
		//    <attribute name="vert" type="ST_TextVerticalType" use="optional"/>
		//    <attribute name="wrap" type="ST_TextWrappingType" use="optional"/>
		//    <attribute name="lIns" type="ST_Coordinate32" use="optional"/>
		//    <attribute name="tIns" type="ST_Coordinate32" use="optional"/>
		//    <attribute name="rIns" type="ST_Coordinate32" use="optional"/>
		//    <attribute name="bIns" type="ST_Coordinate32" use="optional"/>
		//    <attribute name="numCol" type="ST_TextColumnCount" use="optional"/>
		//    <attribute name="spcCol" type="ST_PositiveCoordinate32" use="optional"/>
		//    <attribute name="rtlCol" type="xsd:boolean" use="optional"/>
		//    <attribute name="fromWordArt" type="xsd:boolean" use="optional"/>
		//    <attribute name="anchor" type="ST_TextAnchoringType" use="optional"/>
		//    <attribute name="anchorCtr" type="xsd:boolean" use="optional"/>
		//    <attribute name="forceAA" type="xsd:boolean" use="optional"/>
		//    <attribute name="upright" type="xsd:boolean" use="optional" default="false"/>
		//    <attribute name="compatLnSpc" type="xsd:boolean" use="optional"/>
		//</complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		public const string LocalName = "bodyPr";

		// http://schemas.openxmlformats.org/drawingml/2006/main/bodyPr
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			BodyPrElement.LocalName;

		public const string AnchorAttributeName = "anchor";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return BodyPrElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			FormattedText fs = (FormattedText)manager.ContextStack[typeof(FormattedText)];
			if (fs == null)
			{
				Utilities.DebugFail("Cannot find FormattedText on the context stack.");
				return;
			}

			object attributeValue = null;
			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case BodyPrElement.AnchorAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_TextAnchoringType, ST_TextAnchoringType.t);
							fs.VerticalAlignment = (VerticalTextAlignment)(ST_TextAnchoringType)attributeValue;
						}
						break;

					// MD 7/5/12 - TFS115687
					// Cache all loaded properties so we don't lose them on round-trip.
					default:
						fs.RoundTrip2007Properties[attributeName] = attribute.Value;
						break;
				}
			}
		}

		#endregion Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			Utilities.DebugFail("This isn't implemented.");
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