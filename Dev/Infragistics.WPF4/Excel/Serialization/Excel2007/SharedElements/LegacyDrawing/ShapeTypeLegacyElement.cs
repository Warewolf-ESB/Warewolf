using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;






using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing

{
	internal class ShapeTypeLegacyElement : XmlElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Shapetype">
		//    <sequence>
		//        <group ref="EG_ShapeElements" minOccurs="0" maxOccurs="unbounded"/>
		//        <element ref="o:complex" minOccurs="0"/>
		//    </sequence>
		//    <attributeGroup ref="AG_AllCoreAttributes"/>
		//    <attributeGroup ref="AG_AllShapeAttributes"/>
		//    <attributeGroup ref="AG_Adj"/>
		//    <attributeGroup ref="AG_Path"/>
		//    <attribute ref="o:master"/>
		//</complexType>

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "shapetype";       






		public const string QualifiedName =
			LegacyDrawingsPart.VmlNamespace +
			XmlElementBase.NamespaceSeparator +
			ShapeTypeLegacyElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		//private const string IdAttributeName = "id";

		private const string SptAttributeName = 
			LegacyDrawingsPart.OfficeNamespace +
			XmlElementBase.NamespaceSeparator + 
			"spt";

        private const string PathAttributeName = "path";
		private const string CoordSizeAttributeName = "coordsize";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return ShapeTypeLegacyElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			string attributeValue = null;

			// Add the 'id' attribute.
			attributeValue = "_x0000_t202";
			XmlElementBase.AddAttribute( element, ShapeTypeLegacyElement.IdAttributeName, attributeValue );

			// Add the 'coordsize' attribute
			attributeValue = "21600,21600";
			XmlElementBase.AddAttribute( element, ShapeTypeLegacyElement.CoordSizeAttributeName, attributeValue );

			// Add the 'type' attribute.
			attributeValue = "202";
			XmlElementBase.AddAttribute( element, ShapeTypeLegacyElement.SptAttributeName, attributeValue );

            // Add the 'path' attribute
            attributeValue = "m,l,21600r21600,l21600,xe";
            XmlElementBase.AddAttribute(element, ShapeTypeLegacyElement.PathAttributeName, attributeValue);


			XmlElementBase.AddElement( element, StrokeLegacyElement.QualifiedName );
			XmlElementBase.AddElement( element, PathLegacyElement.QualifiedName );
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