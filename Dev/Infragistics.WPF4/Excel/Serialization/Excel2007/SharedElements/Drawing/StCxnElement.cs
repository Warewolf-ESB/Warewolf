using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;






using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing

{



	internal class StCxnElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_Connection">
		// <attribute name="id" type="ST_DrawingElementId" use="required"/>
		// <attribute name="idx" type="xsd:unsignedInt" use="required"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>stCxn</summary>
		public const string LocalName = "stCxn";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/stCxn</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			StCxnElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		//private const string IdAttributeName = "id";

		private const string IdxAttributeName = "idx";

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return StCxnElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			Workbook workBook = manager.Workbook;
			object attributeValue = null;

			

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{

					#region id
					// ***REQUIRED***
					// Name = 'id', Type = ST_DrawingElementId, Default = 
					case StCxnElement.IdAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_DrawingElementId, "null" );

						

					}
					break;
					#endregion id

					#region idx
					// ***REQUIRED***
					// Name = 'idx', Type = UInt32, Default = 
					case StCxnElement.IdxAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, null );

						

					}
					break;
					#endregion idx

				}
			}
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			Workbook workbook = manager.ContextStack[ typeof(Workbook) ] as Workbook;
			string attributeValue = string.Empty;

			

			#region Id
			// Name = 'id', Type = ST_DrawingElementId, Default = 
			// ***REQUIRED***
			// attributeValue = XmlElementBase.GetXmlString(FOO, DataType.ST_DrawingElementId);
			XmlElementBase.AddAttribute(element, StCxnElement.IdAttributeName, attributeValue);

			#endregion Id

			#region Idx
			// Name = 'idx', Type = UInt32, Default = 
			// ***REQUIRED***
			// attributeValue = XmlElementBase.GetXmlString(FOO, DataType.UInt32);
			XmlElementBase.AddAttribute(element, StCxnElement.IdxAttributeName, attributeValue);

			#endregion Idx

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