using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing
{
	internal class ShapeLegacyElement : XmlElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Shape">
		//    <choice maxOccurs="unbounded">
		//        <group ref="EG_ShapeElements"/>
		//        <element ref="o:ink"/>
		//        <element ref="p:iscomment"/>
		//    </choice>
		//    <attributeGroup ref="AG_AllCoreAttributes"/>
		//    <attributeGroup ref="AG_AllShapeAttributes"/>
		//    <attributeGroup ref="AG_Type"/>
		//    <attributeGroup ref="AG_Adj"/>
		//    <attributeGroup ref="AG_Path"/>
		//    <attribute ref="o:gfxdata"/>
		//    <attribute name="equationxml" type="xsd:string" use="optional"/>
		//</complexType>

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "shape";






		public const string QualifiedName =
			LegacyDrawingsPart.VmlNamespace +
			XmlElementBase.NamespaceSeparator +
			ShapeLegacyElement.LocalName;

		private const string FillColorAttributeName = "fillcolor";

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		//private const string IdAttributeName = "id";

		private const string InsetModeAttributeName = 
			LegacyDrawingsPart.OfficeNamespace + 
			XmlElementBase.NamespaceSeparator +
			"insetmode";
		private const string TypeAttributeName = "type";
		private const string StyleAttributeName = "style";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return ShapeLegacyElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			ListContext<LegacyShapeData> shapeListContext =
				(ListContext<LegacyShapeData>)manager.ContextStack[ typeof( ListContext<LegacyShapeData> ) ];

			if ( shapeListContext == null )
			{
				Utilities.DebugFail( "There was no shape list on the context stack." );
				return;
			}

			object attributeValue = null;
			string idValue = null;
			string typeValue = null;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );
				switch ( attributeName )
				{
					case ShapeLegacyElement.IdAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.String, String.Empty );
							idValue = (string)attributeValue;
						}
						break;

					case ShapeLegacyElement.TypeAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.String, String.Empty );
							typeValue = (string)attributeValue;
						}
						break;
				}
			}

			if ( idValue == null || idValue.StartsWith( "_x0000_s" ) == false )
			{
				Utilities.DebugFail( "Invalid legacy shape id: " + idValue );
				return;
			}

			string idNumberValue = idValue.Substring( 8 );

			int id;
			if ( Int32.TryParse( idNumberValue, out id ) == false )
			{
				Utilities.DebugFail( "Invalid legacy shape id: " + idValue );
				return;
			}

			// Excel will change the shape type id because we are not writing something out exactly as Excel expects, so we cannot assume the shape type.
			//if ( typeValue == null || typeValue.Length != 12 || typeValue.StartsWith( "#_x0000_t" ) == false )
			//{
			//    Utilities.DebugFail( "Invalid legacy shape type: " + typeValue );
			//    return;
			//}
			//
			//string typeNumberValue = typeValue.Substring( 9 );
			//
			//int type;
			//if ( Int32.TryParse( typeNumberValue, out type ) == false )
			//{
			//    Utilities.DebugFail( "Invalid legacy shape type: " + typeValue );
			//    return;
			//}
			//
			//if ( type != 202 )
			//{
			//    Utilities.DebugFail( "Invalid legacy shape type: " + typeValue );
			//    return;
			//}

			LegacyShapeData legacyShape = new LegacyShapeData();
			legacyShape.ShapeId = id;

			shapeListContext.AddItem( legacyShape );
			manager.ContextStack.Push( legacyShape );
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			ListContext<LegacyShapeData> shapeDataListContext = 
				(ListContext<LegacyShapeData>)manager.ContextStack[ typeof( ListContext<LegacyShapeData> ) ];

			if ( shapeDataListContext == null )
			{
				Utilities.DebugFail( "Could not find the shape data list context on the context stack." );
				return;
			}

			LegacyShapeData shapeData = (LegacyShapeData)shapeDataListContext.ConsumeCurrentItem();
			manager.ContextStack.Push( shapeData );

			string attributeValue = null;

			// Add the 'id' attribute.
			attributeValue = XmlElementBase.GetXmlString( shapeData.ShapeId, DataType.Int32 );
			attributeValue = "_x0000_s" + attributeValue;
			XmlElementBase.AddAttribute( element, ShapeLegacyElement.IdAttributeName, attributeValue );

			// Add the 'type' attribute.
			attributeValue = "#_x0000_t202";
			XmlElementBase.AddAttribute( element, ShapeLegacyElement.TypeAttributeName, attributeValue );

			// Add the 'fillcolor' attribute.
			// MD 1/18/12 - 12.1 - Cell Format Updates
			//attributeValue = "#ffffe1";
			//XmlElementBase.AddAttribute( element, ShapeLegacyElement.FillColorAttributeName, attributeValue );
			ShapeFillSolid solidFill = shapeData.Shape.FillResolved as ShapeFillSolid;
			if (solidFill != null)
			{
				attributeValue = Utilities.ColorToHtml(solidFill.Color);
				XmlElementBase.AddAttribute(element, ShapeLegacyElement.FillColorAttributeName, attributeValue);
			}

			// Add the 'auto' attribute.
			attributeValue = "auto";
			XmlElementBase.AddAttribute( element, ShapeLegacyElement.InsetModeAttributeName, attributeValue );

			if ( shapeData.VisibleResolved == false )
			{
				// Add the 'style' attribute.
				attributeValue = "visibility:hidden";
				XmlElementBase.AddAttribute( element, ShapeLegacyElement.StyleAttributeName, attributeValue );
			}

            // Add the 'shadow' element
            XmlElementBase.AddElement(element, ShadowLegacyElement.QualifiedName);

			// Add the 'ClientData' element.
			XmlElementBase.AddElement( element, ClientDataLegacyElement.QualifiedName );
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