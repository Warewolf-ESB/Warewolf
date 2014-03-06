using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;






using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing

{
    internal class ClientDataLegacyElement : XmlElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_ClientData">
		//    <choice minOccurs="0" maxOccurs="unbounded">
		//        <element name="MoveWithCells" type="ST_TrueFalseBlank"/>
		//        <element name="SizeWithCells" type="ST_TrueFalseBlank"/>
		//        <element name="Anchor" type="xsd:string"/>
		//        <element name="Locked" type="ST_TrueFalseBlank"/>
		//        <element name="DefaultSize" type="ST_TrueFalseBlank"/>
		//        <element name="PrintObject" type="ST_TrueFalseBlank"/>
		//        <element name="Disabled" type="ST_TrueFalseBlank"/>
		//        <element name="AutoFill" type="ST_TrueFalseBlank"/>
		//        <element name="AutoLine" type="ST_TrueFalseBlank"/>
		//        <element name="AutoPict" type="ST_TrueFalseBlank"/>
		//        <element name="FmlaMacro" type="xsd:string"/>
		//        <element name="TextHAlign" type="xsd:string"/>
		//        <element name="TextVAlign" type="xsd:string"/>
		//        <element name="LockText" type="ST_TrueFalseBlank"/>
		//        <element name="JustLastX" type="ST_TrueFalseBlank"/>
		//        <element name="SecretEdit" type="ST_TrueFalseBlank"/>
		//        <element name="Default" type="ST_TrueFalseBlank"/>
		//        <element name="Help" type="ST_TrueFalseBlank"/>
		//        <element name="Cancel" type="ST_TrueFalseBlank"/>
		//        <element name="Dismiss" type="ST_TrueFalseBlank"/>
		//        <element name="Accel" type="xsd:integer"/>
		//        <element name="Accel2" type="xsd:integer"/>
		//        <element name="Row" type="xsd:integer"/>
		//        <element name="Column" type="xsd:integer"/>
		//        <element name="Visible" type="ST_TrueFalseBlank"/>
		//        <element name="RowHidden" type="ST_TrueFalseBlank"/>
		//        <element name="ColHidden" type="ST_TrueFalseBlank"/>
		//        <element name="VTEdit" type="xsd:integer"/>
		//        <element name="MultiLine" type="ST_TrueFalseBlank"/>
		//        <element name="VScroll" type="ST_TrueFalseBlank"/>
		//        <element name="ValidIds" type="ST_TrueFalseBlank"/>
		//        <element name="FmlaRange" type="xsd:string"/>
		//        <element name="WidthMin" type="xsd:integer"/>
		//        <element name="Sel" type="xsd:integer"/>
		//        <element name="NoThreeD2" type="ST_TrueFalseBlank"/>
		//        <element name="SelType" type="xsd:string"/>
		//        <element name="MultiSel" type="xsd:string"/>
		//        <element name="LCT" type="xsd:string"/>
		//        <element name="ListItem" type="xsd:string"/>
		//        <element name="DropStyle" type="xsd:string"/>
		//        <element name="Colored" type="ST_TrueFalseBlank"/>
		//        <element name="DropLines" type="xsd:integer"/>
		//        <element name="Checked" type="xsd:integer"/>
		//        <element name="FmlaLink" type="xsd:string"/>
		//        <element name="FmlaPict" type="xsd:string"/>
		//        <element name="NoThreeD" type="ST_TrueFalseBlank"/>
		//        <element name="FirstButton" type="ST_TrueFalseBlank"/>
		//        <element name="FmlaGroup" type="xsd:string"/>
		//        <element name="Val" type="xsd:integer"/>
		//        <element name="Min" type="xsd:integer"/>
		//        <element name="Max" type="xsd:integer"/>
		//        <element name="Inc" type="xsd:integer"/>
		//        <element name="Page" type="xsd:integer"/>
		//        <element name="Horiz" type="ST_TrueFalseBlank"/>
		//        <element name="Dx" type="xsd:integer"/>
		//        <element name="MapOCX" type="ST_TrueFalseBlank"/>
		//        <element name="CF" type="ST_CF"/>
		//        <element name="Camera" type="ST_TrueFalseBlank"/>
		//        <element name="RecalcAlways" type="ST_TrueFalseBlank"/>
		//        <element name="AutoScale" type="ST_TrueFalseBlank"/>
		//        <element name="DDE" type="ST_TrueFalseBlank"/>
		//        <element name="UIObj" type="ST_TrueFalseBlank"/>
		//        <element name="ScriptText" type="xsd:string"/>
		//        <element name="ScriptExtended" type="xsd:string"/>
		//        <element name="ScriptLanguage" type="xsd:nonNegativeInteger"/>
		//        <element name="ScriptLocation" type="xsd:nonNegativeInteger"/>
		//        <element name="FmlaTxbx" type="xsd:string"/>
		//    </choice>
		//    <attribute name="ObjectType" type="ST_ObjectType" use="required"/>
		//</complexType>

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "ClientData";






		public const string QualifiedName =
			LegacyDrawingsPart.ExcelNamespace +
			XmlElementBase.NamespaceSeparator +
			ClientDataLegacyElement.LocalName;

		private const string ObjectTypeAttributeName = "ObjectType";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return ClientDataLegacyElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					case ClientDataLegacyElement.ObjectTypeAttributeName:
						string type = attribute.Value;
						Debug.Assert( type == "Note", "The client data is for the wrong kind of shape." );
						break;

					default:
						Utilities.DebugFail( "Unknown attribute type in the ClientData element: " + attributeName );
						break;
				}
			}
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			LegacyShapeData shapeData = (LegacyShapeData)manager.ContextStack[ typeof( LegacyShapeData ) ];

			if ( shapeData == null )
			{
				Utilities.DebugFail( "Could not find the shape data on the context stack." );
				return;
			}

			XmlElementBase.AddAttribute( element, ClientDataLegacyElement.ObjectTypeAttributeName, "Note" );

			XmlElementBase.AddElement( element, AnchorLegacyElement.QualifiedName );
			XmlElementBase.AddElement( element, RowLegacyElement.QualifiedName );
			XmlElementBase.AddElement( element, ColumnLegacyElement.QualifiedName );

			if ( shapeData.Visible )
				XmlElementBase.AddElement( element, VisibleLegacyElement.QualifiedName );

			if ( shapeData.RowHidden )
				XmlElementBase.AddElement( element, RowHiddenLegacyElement.QualifiedName );

			if ( shapeData.ColHidden )
				XmlElementBase.AddElement( element, ColHiddenLegacyElement.QualifiedName );
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