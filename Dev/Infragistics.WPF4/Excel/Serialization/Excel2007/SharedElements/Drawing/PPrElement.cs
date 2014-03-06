using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	// MD 11/8/11 - TFS85193
	internal class PPrElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		//<complexType name="CT_TextParagraphProperties">
		//    <sequence>
		//        <element name="lnSpc" type="CT_TextSpacing" minOccurs="0" maxOccurs="1"/>
		//        <element name="spcBef" type="CT_TextSpacing" minOccurs="0" maxOccurs="1"/>
		//        <element name="spcAft" type="CT_TextSpacing" minOccurs="0" maxOccurs="1"/>
		//        <group ref="EG_TextBulletColor" minOccurs="0" maxOccurs="1"/>
		//        <group ref="EG_TextBulletSize" minOccurs="0" maxOccurs="1"/>
		//        <group ref="EG_TextBulletTypeface" minOccurs="0" maxOccurs="1"/>
		//        <group ref="EG_TextBullet" minOccurs="0" maxOccurs="1"/>
		//        <element name="tabLst" type="CT_TextTabStopList" minOccurs="0" maxOccurs="1"/>
		//        <element name="defRPr" type="CT_TextCharacterProperties" minOccurs="0" maxOccurs="1"/>
		//        <element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		//    </sequence>
		//    <attribute name="marL" type="ST_TextMargin" use="optional"/>
		//    <attribute name="marR" type="ST_TextMargin" use="optional"/>
		//    <attribute name="lvl" type="ST_TextIndentLevelType" use="optional"/>
		//    <attribute name="indent" type="ST_TextIndent" use="optional"/>
		//    <attribute name="algn" type="ST_TextAlignType" use="optional"/>
		//    <attribute name="defTabSz" type="ST_Coordinate32" use="optional"/>
		//    <attribute name="rtl" type="xsd:boolean" use="optional"/>
		//    <attribute name="eaLnBrk" type="xsd:boolean" use="optional"/>
		//    <attribute name="fontAlgn" type="ST_TextFontAlignType" use="optional"/>
		//    <attribute name="latinLnBrk" type="xsd:boolean" use="optional"/>
		//    <attribute name="hangingPunct" type="xsd:boolean" use="optional"/>
		//</complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		public const string LocalName = "pPr";

		// http://schemas.openxmlformats.org/drawingml/2006/main/pPr
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			PPrElement.LocalName;

		public const string AlgnAttributeName = "algn";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return PPrElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			FormattedTextParagraph paragraph = (FormattedTextParagraph)manager.ContextStack[typeof(FormattedTextParagraph)];
			if (paragraph == null)
			{
				Utilities.DebugFail("Cannot find a FormattedTextParagraph on the context stack.");
				return;
			}

			object attributeValue = null;
			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case PPrElement.AlgnAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_TextAlignType, ST_TextAlignType.ctr);
							paragraph.Alignment = (HorizontalTextAlignment)(ST_TextAlignType)attributeValue;
						}
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