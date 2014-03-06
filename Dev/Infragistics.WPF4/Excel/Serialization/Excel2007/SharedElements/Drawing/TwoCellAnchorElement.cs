using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



    internal class TwoCellAnchorElement : CellAnchorElementBase
    {
		//<complexType name="CT_TwoCellAnchor">
		//    <sequence>
		//        <element name="from" type="CT_Marker"/>
		//        <element name="to" type="CT_Marker"/>
		//        <group ref="EG_ObjectChoices"/>
		//        <element name="clientData" type="CT_AnchorClientData" minOccurs="1" maxOccurs="1"/>
		//    </sequence>
		//    <attribute name="editAs" type="ST_EditAs" use="optional" default="twoCell"/>
		//</complexType>

        #region Constants

        public const string LocalName = "twoCellAnchor";

        /// <summary>
        /// http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/twoCellAnchor
        /// </summary>
        public const string QualifiedName =
            DrawingsPart.DefaultNamespace +
            XmlElementBase.NamespaceSeparator +
            TwoCellAnchorElement.LocalName;

		internal const string EditAsAttributeName = "editAs";

        #endregion Constants

        #region Base Class Overrides

        #region ElementName

        public override string ElementName
        {
            get { return TwoCellAnchorElement.QualifiedName; }
        }

        #endregion ElementName

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            //  Create the TwoCellAnchor for this element and push it onto the context stack
            CellAnchor cellAnchor = new CellAnchor();
            manager.ContextStack.Push( cellAnchor );

            foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );
                object attributeValue = null;

				switch ( attributeName )
				{
					#region editAs
					// Name = 'editAs', Type = ST_EditAs, Default = twoCell
					case TwoCellAnchorElement.EditAsAttributeName:
					{
						// MD 8/30/11 - TFS84387
						// The default value is a ShapePositioningMode value, not a string, and it is the value that corresponds to the "twoCell" value
						//attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_EditAs, "onecell" );
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_EditAs, ShapePositioningMode.MoveAndSizeWithCells);

                        cellAnchor.PositioningMode = (ShapePositioningMode)attributeValue;
					}
					break;
					#endregion editAs

				}
			}
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            base.Save( manager, element, ref value );

            string attributeValue = null;

            //  Get the shape off the context stack without removing it from the consumeable list.
            WorksheetShape shape = WorksheetShapeSerializationManager.PeekCurrentShape( manager.ContextStack );
            WorksheetShapeSerializationManager serializationManager = shape.Excel2007ShapeSerializationManager;

            //  Now push it onto the stack.
            manager.ContextStack.Push( shape );

			#region EditAs
			// Name = 'editAs', Type = ST_EditAs, Default = twoCell
            ShapePositioningMode positioningMode = shape.PositioningMode;

			// MD 8/30/11 - TFS84387
			// The default value for the editAs attribute is twoCell, which corresponds to MoveAndSizeWithCells.
            //if ( positioningMode != ShapePositioningMode.MoveWithCells )
			if (positioningMode != ShapePositioningMode.MoveAndSizeWithCells)
            {
			    attributeValue = XmlElementBase.GetXmlString(positioningMode, DataType.ST_EditAs);
			    XmlElementBase.AddAttribute(element, TwoCellAnchorElement.EditAsAttributeName, attributeValue);
            }
			#endregion EditAs

            //  Add a 'from' element.
            XmlElementBase.AddElement( element, FromElement.QualifiedName );

            //  Add a 'to' element.
            XmlElementBase.AddElement( element, ToElement.QualifiedName );

			// MD 10/12/10
			// Found while fixing TFS49853
			// Moved this code to a common location because it was defined twice.
			////  Add the element appropriate for the type of the current shape
			//if ( shape is WorksheetShapeGroup )
			//    XmlElementBase.AddElement(element, GrpSpElement.QualifiedName);            
			//else
			//if ( shape is WorksheetImage )
			//    XmlElementBase.AddElement(element, PicElement.QualifiedName);   
			//else
			//    //  UnknownShape
			//    XmlElementBase.AddElement(element, SpElement.QualifiedName);
			XmlElementBase.AddRootShapeElement(element, shape);

            //  Add a clientData element.
            XmlElementBase.AddElement(element, ClientDataElement.QualifiedName);            

        }

        #endregion Save

        #endregion Base Class Overrides
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