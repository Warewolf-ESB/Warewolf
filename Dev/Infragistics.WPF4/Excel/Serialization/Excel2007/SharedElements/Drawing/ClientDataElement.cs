using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;






using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing

{



	internal class ClientDataElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_AnchorClientData">
		// <attribute name="fLocksWithSheet" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="fPrintsWithSheet" type="xsd:boolean" use="optional" default="true"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>clientData</summary>
		public const string LocalName = "clientData";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/clientData</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			ClientDataElement.LocalName;

		private const string FLocksWithSheetAttributeName = "fLocksWithSheet";
		private const string FPrintsWithSheetAttributeName = "fPrintsWithSheet";

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return ClientDataElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			Workbook workBook = manager.Workbook;
			object attributeValue = null;

            WorksheetShapesHolder shapesHolder = manager.ContextStack[typeof(WorksheetShapesHolder)] as WorksheetShapesHolder;
            WorksheetShapeSerializationManager serializationManager = shapesHolder.CurrentSerializationManager;
            if ( serializationManager == null )
            {
                Utilities.DebugFail( "No current WorksheetShapeSerializationManager in ClientDataELement.Load - unexpected." );
                return;
            }

            ClientData clientData = new ClientData();
			
			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{

					#region fLocksWithSheet
					// Name = 'fLocksWithSheet', Type = Boolean, Default = True
					case ClientDataElement.FLocksWithSheetAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        clientData.fLocksWithSheet = (bool)attributeValue;
					}
					break;
					#endregion fLocksWithSheet

					#region fPrintsWithSheet
					// Name = 'fPrintsWithSheet', Type = Boolean, Default = True
					case ClientDataElement.FPrintsWithSheetAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        clientData.fPrintsWithSheet = (bool)attributeValue;
					}
					break;
					#endregion fPrintsWithSheet
				}
			}

            serializationManager.ClientData = clientData;
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            //  Get the shape and its serialization manager off the context stack.
            WorksheetShape shape = manager.ContextStack[typeof(WorksheetShape)] as WorksheetShape;
            WorksheetShapeSerializationManager serializationManager = shape.Excel2007ShapeSerializationManager;

			string attributeValue = string.Empty;
            ClientData clientData = serializationManager.ClientData;
            bool locksWithSheet = clientData.fLocksWithSheet;
            bool printsWithSheet = clientData.fPrintsWithSheet;

			

			#region FLocksWithSheet
			// Name = 'fLocksWithSheet', Type = Boolean, Default = True
            if ( locksWithSheet != ClientData.Default_fLocksWithSheet )
            {
			    attributeValue = XmlElementBase.GetXmlString(locksWithSheet, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, ClientDataElement.FLocksWithSheetAttributeName, attributeValue);
            }
			#endregion FLocksWithSheet

			#region FPrintsWithSheet
			// Name = 'fPrintsWithSheet', Type = Boolean, Default = True
            if ( printsWithSheet != ClientData.Default_fPrintsWithSheet )
            {
			    attributeValue = XmlElementBase.GetXmlString(printsWithSheet, DataType.Boolean);
			    XmlElementBase.AddAttribute(element, ClientDataElement.FPrintsWithSheetAttributeName, attributeValue);
            }

			#endregion FPrintsWithSheet

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