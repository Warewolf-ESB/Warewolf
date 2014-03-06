using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;








using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes;
using System.Drawing;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
	internal class LegacyDrawingElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_LegacyDrawing">
		//    <attribute ref="r:id" use="required"/>
		//</complexType>

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "legacyDrawing";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			LegacyDrawingElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		//private const string IdAttributeName = 
		//    WorksheetElement.RelationshipsNamespace +
		//    XmlElementBase.NamespaceSeparator + 
		//    "id";

		#endregion //Constants

		#region Base class overrides

		#region Type
		/// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
		/// </summary>
		public override XLSXElementType Type
		{
			get { return XLSXElementType.legacyDrawing; }
		}
		#endregion Type

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			ChildDataItem item = manager.ContextStack[ typeof( ChildDataItem ) ] as ChildDataItem;
			if ( item == null )
			{
				Utilities.DebugFail( "Could not get the ChildDataItem from the ContextStack" );
				return;
			}

			Worksheet worksheet = item.Data as Worksheet;
			if ( worksheet == null )
			{
				Utilities.DebugFail( "There was no worksheet on the context stack." );
				return;
			}

			object attributeValue = null;
			string id = null;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );
				switch ( attributeName )
				{
					// MD 10/12/10
					// Found while fixing TFS49853
					// Moved multiply defined constants to a single location.
					//case LegacyDrawingElement.IdAttributeName:
					case LegacyDrawingElement.RelationshipIdAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.String, String.Empty );
							id = (string)attributeValue;
						}
						break;

					default:
						Utilities.DebugFail( "Unknown attribute: " + attributeName );
						break;
				}
			}

			if ( id == null )
			{
				Utilities.DebugFail( "No legacy drawing id" );
				return;
			}

			List<LegacyShapeData> shapeDataList = manager.GetRelationshipDataFromActivePart( id ) as List<LegacyShapeData>;

			if ( shapeDataList == null )
			{
				Utilities.DebugFail( "No shape data at the part id." );
				return;
			}

			List<WorksheetCellCommentData> commentDataList = null;

			foreach ( IPackageRelationship relationship in manager.ActivePart.GetRelationships() )
			{
				if ( relationship.RelationshipType != CommentsPart.RelationshipTypeValue )
					continue;

				commentDataList = manager.GetPartData( relationship ) as List<WorksheetCellCommentData>;
				break;
			}

			if ( commentDataList == null )
			{
				Utilities.DebugFail( "No comment data in the relationship." );
				return;
			}

			Debug.Assert( shapeDataList.Count == commentDataList.Count, "There should be the same amount of comment and legacy shapes." );

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Dictionary<WorksheetCell, WorksheetCellCommentData> commentDataByCell = new Dictionary<WorksheetCell, WorksheetCellCommentData>();
			//foreach ( WorksheetCellCommentData commentData in commentDataList )
			//{
			//    WorksheetCell cell = worksheet.Rows[ commentData.RowIndex ].Cells[ commentData.ColumnIndex ];
			//    commentDataByCell.Add( cell, commentData );
			//}
			Dictionary<WorksheetCellAddress, WorksheetCellCommentData> commentDataByCell = new Dictionary<WorksheetCellAddress, WorksheetCellCommentData>();
			foreach (WorksheetCellCommentData commentData in commentDataList)
			{
				// MD 3/27/12 - 12.1 - Table Support
				//commentDataByCell.Add(new WorksheetCellAddress(worksheet.Rows[commentData.RowIndex], commentData.ColumnIndex), commentData);
				commentDataByCell.Add(new WorksheetCellAddress(commentData.RowIndex, commentData.ColumnIndex), commentData);
			}

			foreach ( LegacyShapeData shapeData in shapeDataList )
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell cell = worksheet.Rows[ shapeData.RowIndex ].Cells[ shapeData.ColumnIndex ];
				WorksheetRow row = worksheet.Rows[shapeData.RowIndex];
				short columnIndex = shapeData.ColumnIndex;

				// MD 3/27/12 - 12.1 - Table Support
				//WorksheetCellAddress cellAddress = new WorksheetCellAddress(row, columnIndex);
				WorksheetCellAddress cellAddress = new WorksheetCellAddress(row.Index, columnIndex);

				WorksheetCellCommentData commentData;

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//if ( commentDataByCell.TryGetValue( cell, out commentData ) == false )
				if (commentDataByCell.TryGetValue(cellAddress, out commentData) == false)
				{
					Utilities.DebugFail( "There is no corresponding comment data for the shape data." );
					continue;
				}

				WorksheetCellComment comment = commentData.Comment;

				comment.Visible = shapeData.Visible;

				int leftColumn = shapeData.AnchorData[ 0 ];
				int leftOffsetPixels = shapeData.AnchorData[ 1 ];
				int topRow = shapeData.AnchorData[ 2 ];
				int topOffsetPixels = shapeData.AnchorData[ 3 ];
				int rightColumn = shapeData.AnchorData[ 4 ];
				int rightOffsetPixels = shapeData.AnchorData[ 5 ];
				int bottomRow = shapeData.AnchorData[ 6 ];
				int bottomOffsetPixels = shapeData.AnchorData[ 7 ];

				comment.TopLeftCornerCell = worksheet.Rows[ topRow ].Cells[ leftColumn ];
				comment.BottomRightCornerCell = worksheet.Rows[ bottomRow ].Cells[ rightColumn ];

				double leftColumnWidthInPixels = worksheet.GetColumnWidthInPixels( leftColumn );
				Debug.Assert( leftOffsetPixels < leftColumnWidthInPixels, "The left column offset is too far." );

				double topRowHeightInPixels = worksheet.GetRowHeightInPixels( topRow );
				Debug.Assert( topOffsetPixels < topRowHeightInPixels, "The top row offset is too far." );

				comment.TopLeftCornerPosition = new PointF(
					(float)( ( 100 * leftOffsetPixels ) / leftColumnWidthInPixels ),
					(float)( ( 100 * topOffsetPixels ) / topRowHeightInPixels ) );

				double rightColumnWidthInPixels = worksheet.GetColumnWidthInPixels( rightColumn );
				Debug.Assert( rightOffsetPixels <= rightColumnWidthInPixels, "The right column offset is too far." );

				double bottomRowHeightInPixels = worksheet.GetRowHeightInPixels( bottomRow );
				Debug.Assert( bottomOffsetPixels <= bottomRowHeightInPixels, "The bottom row offset is too far." );

				comment.BottomRightCornerPosition = new PointF(
					(float)( ( 100 * rightOffsetPixels ) / rightColumnWidthInPixels ),
					(float)( ( 100 * bottomOffsetPixels ) / bottomRowHeightInPixels ) );

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//cell.Comment = comment;
				row.SetCellCommentInternal(columnIndex, comment);
			}
		}

		#endregion Load

		#region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			Worksheet worksheet = manager.ContextStack[ typeof( Worksheet ) ] as Worksheet;

			if ( worksheet == null )
			{
				Utilities.DebugFail( "Could not get the worksheet from the ContextStack" );
				return;
			}

			// MD 10/12/10
			// Found while fixing TFS49853
			// Moved multiply defined constants to a single location.
			//XmlElementBase.AddAttribute( element, LegacyDrawingElement.IdAttributeName, worksheet.legacyDrawingRelationshipId );
			XmlElementBase.AddAttribute(element, LegacyDrawingElement.RelationshipIdAttributeName, worksheet.legacyDrawingRelationshipId);
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