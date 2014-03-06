using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{



	internal class DrawingElement : XLSXElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_Drawing">
		// <attribute ref="r:id" use="required"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>drawing</summary>
		public const string LocalName = "drawing";
		
		/// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/drawing</summary>
		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			DrawingElement.LocalName;

		#endregion Constants

		#region Base class overrides

			#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.drawing; }
		}

			#endregion Type

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            Worksheet worksheet = Utilities.GetWorksheet(manager.ContextStack);
            if (worksheet == null)
                return;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                switch (attributeName)
                {
                    case XmlElementBase.RelationshipIdAttributeName:

                        //  Get the value of the 'id' attribute, which is a relationship ID,
                        //  get the drawings part from that, then get its URI.
                        string id = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_RelationshipID, String.Empty);
                        string path = manager.GetRelationshipPathFromActivePart( id );

                        //  If shapes were serialized in, hook them up to the appropriate
                        //  Worksheet now.
                        if ( manager.HasSerializedShapes )
                        {
                            WorksheetShapesHolder shapesHolder = null;
                            if ( manager.SerializedShapes.TryGetValue(path, out shapesHolder) )
                            {
                                worksheet.InitShapesCollection( shapesHolder );
                                manager.SerializedShapes.Remove( path );
                            }
                        }
                        break;

                    default:
                        Utilities.DebugFail("Unknown attribute");
                        break;
                }
            }
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get worksheet from context stack");
                return;
            }

            // Add the relationship ID
            string attributeValue = XmlElementBase.GetXmlString(worksheet.drawingRelationshipId, DataType.ST_RelationshipID);
			XmlElementBase.AddAttribute(element, XmlElementBase.RelationshipIdAttributeName, attributeValue);
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