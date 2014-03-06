using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing
{
    internal class XmlLegacyElement : XmlElementBase
	{
		#region Constants






		public const string LocalName = "xml";






		public const string QualifiedName =
			XmlLegacyElement.LocalName;

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return XmlLegacyElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[ typeof( ChildDataItem ) ];
			Debug.Assert( dataItem != null, "There was no child data item on the context stack." );

			List<LegacyShapeData> shapeDataList = new List<LegacyShapeData>();

			if ( dataItem != null )
				dataItem.Data = shapeDataList;

			ListContext<LegacyShapeData> shapeDataListContext = new ListContext<LegacyShapeData>( shapeDataList );
			manager.ContextStack.Push( shapeDataListContext );
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "Could not find the worksheet on the context stack." );
				return;
			}

			XmlElementBase.AddNamespaceDeclaration( element, LegacyDrawingsPart.VmlNamespacePrefix, LegacyDrawingsPart.VmlNamespace );
			XmlElementBase.AddNamespaceDeclaration( element, LegacyDrawingsPart.OfficeNamespacePrefix, LegacyDrawingsPart.OfficeNamespace );
			XmlElementBase.AddNamespaceDeclaration( element, LegacyDrawingsPart.ExcelNamespacePrefix, LegacyDrawingsPart.ExcelNamespace );

			List<LegacyShapeData> shapeDataList = new List<LegacyShapeData>();

			foreach ( WorksheetCellComment comment in worksheet.CommentShapes )
			{
				LegacyShapeData shapeData = new LegacyShapeData();

				shapeData.InitializeAnchorData( comment );

				// MD 3/15/12 - TFS104581
				//shapeData.ColHidden = worksheet.IsColumnHidden( comment.Cell.ColumnIndex );
				shapeData.ColHidden = worksheet.IsColumnHidden(comment.Cell.ColumnIndexInternal);

				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (comment.Cell.Worksheet == null)
				{
					Utilities.DebugFail("This is unexpected");
					continue;
				}

				// MD 4/12/11 - TFS67084
				// Use short instead of int so we don't have to cast.
				//shapeData.ColumnIndex = comment.Cell.ColumnIndex;
				shapeData.ColumnIndex = comment.Cell.ColumnIndexInternal;

				shapeData.RowHidden = worksheet.IsRowHidden( comment.Cell.RowIndex );
				shapeData.RowIndex = comment.Cell.RowIndex;
				shapeData.ShapeId = (int)comment.ShapeId;
				shapeData.Visible = comment.Visible;

				shapeDataList.Add( shapeData );
			}

			ListContext<LegacyShapeData> shapeDataListContext = new ListContext<LegacyShapeData>( shapeDataList );
			manager.ContextStack.Push( shapeDataListContext );

			XmlElementBase.AddElement( element, ShapeTypeLegacyElement.QualifiedName );
			XmlElementBase.AddElements( element, ShapeLegacyElement.QualifiedName, worksheet.CommentShapes.Count );
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