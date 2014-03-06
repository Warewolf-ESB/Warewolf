using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class CNvPrElement : XmlElementBase, IConsumedElementValueProvider
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_NonVisualDrawingProps">
		// <sequence>
		// <element name="hlinkClick" type="CT_Hyperlink" minOccurs="0" maxOccurs="1"/>
		// <element name="hlinkHover" type="CT_Hyperlink" minOccurs="0" maxOccurs="1"/>
		// <element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// <attribute name="id" type="ST_DrawingElementId" use="required"/>
		// <attribute name="name" type="xsd:string" use="required"/>
		// <attribute name="descr" type="xsd:string" use="optional" default=""/>
		// <attribute name="hidden" type="xsd:boolean" use="optional" default="false"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>cNvPr</summary>
		public const string LocalName = "cNvPr";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/cNvPr</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			CNvPrElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		//private const string IdAttributeName = "id";

		internal const string HiddenAttributeName = "hidden";

		#endregion Constants

		#region Base class overrides

        #region ElementName

        public override string ElementName
        {
            get { return CNvPrElement.QualifiedName; }
        }

        #endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            if ( this.consumedValues != null )
                this.consumedValues.Clear();

			Workbook workBook = manager.Workbook;
			object attributeValue = null;

            WorksheetShape shape = manager.ContextStack[typeof(WorksheetShape)] as WorksheetShape;
            WorksheetShapeSerializationManager serializationCache = shape.Excel2007ShapeSerializationManager;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{

					#region id
					// ***REQUIRED***
					// Name = 'id', Type = ST_DrawingElementId, Default = 
					case CNvPrElement.IdAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_DrawingElementId, "null" );
                        shape.ShapeId = (uint)attributeValue;

                        //  Add the 'id' attribute to the consumed values list for this
                        //  element, so that when we serialize out, we know not to use
                        //  the attribute value, but rather use the value of the corresponding
                        //  property of the public object model instead.
                        this.consumedValues.Add( attributeName, HandledAttributeIdentifier.CNvPrElement_Id );
					}
					break;
					#endregion id

					#region hidden
					// Name = 'hidden', Type = xsd:boolean, Default = false
					case CNvPrElement.HiddenAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        bool hidden = (bool)attributeValue;
                        shape.Visible = ! hidden;

                        //  Add the 'hidden' attribute to the consumed values list for this
                        //  element, so that when we serialize out, we know not to use
                        //  the attribute value, but rather use the value of the corresponding
                        //  property of the public object model instead.
                        this.consumedValues.Add( attributeName, HandledAttributeIdentifier.CNvPrElement_Hidden );
					}
					break;
					#endregion hidden
				}
			}
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
		}

			#endregion Save

		#endregion Base class overrides

        //  BF 8/25/08
        #region IConsumedElementValueProvider implementation

		private Dictionary<string, HandledAttributeIdentifier> consumedValues = new Dictionary<string, HandledAttributeIdentifier>( 1 );

        /// <summary>
        /// cNvPr.id = Infragistics.Documents.Excel.WorksheetShape.ShapeId
        /// </summary>
        Dictionary<string, HandledAttributeIdentifier> IConsumedElementValueProvider.ConsumedValues
        {
            get{ return this.consumedValues; }
        }

        #endregion IConsumedElementValueProvider implementation

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