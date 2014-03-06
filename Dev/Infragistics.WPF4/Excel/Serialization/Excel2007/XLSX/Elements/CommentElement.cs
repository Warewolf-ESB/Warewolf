using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.FormulaUtilities;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	internal class CommentElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Comment">
		//    <sequence>
		//        <element name="text" type="CT_Rst" minOccurs="1" maxOccurs="1"/>
		//    </sequence>
		//    <attribute name="ref" type="ST_Ref" use="required"/>
		//    <attribute name="authorId" type="xsd:unsignedInt" use="required"/>
		//    <attribute name="guid" type="ST_Guid" use="optional"/>
		//</complexType>

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "comment";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CommentElement.LocalName;

		private const string AuthorIdAttributeName = "authorId";
		private const string GuidAttributeName = "guid";
		private const string RefAttributeName = "ref";

		#endregion //Constants

		#region Base class overrides

		#region Type
		/// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
		/// </summary>
		public override XLSXElementType Type
		{
			get { return XLSXElementType.comment; }
		}
		#endregion Type

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			ListContext<string> authorListContext = (ListContext<string>)manager.ContextStack[ typeof( ListContext<string> ) ];

			if ( authorListContext == null )
			{
				Utilities.DebugFail( "There was no author list on the context stack." );
				return;
			}

			ListContext<WorksheetCellCommentData> commentsListContext = 
				(ListContext<WorksheetCellCommentData>)manager.ContextStack[ typeof( ListContext<WorksheetCellCommentData> ) ];

			if ( commentsListContext == null )
			{
				Utilities.DebugFail( "There was no comments list on the context stack." );
				return;
			}

			Workbook workBook = manager.Workbook;
			object attributeValue = null;
			uint authorId = 0;
			Guid guid = Guid.Empty;
			short columnIndex = -1;
			int rowIndex = -1;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );
				switch ( attributeName )
				{
					case CommentElement.AuthorIdAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, String.Empty );
							authorId = (uint)attributeValue;
						}
						break;

					case CommentElement.GuidAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Guid, String.Empty );
							guid = (Guid)attributeValue;
						}
						break;

					case CommentElement.RefAttributeName:
						{
							string cellRef = (string)XmlElementBase.GetAttributeValue( attribute, DataType.ST_CellRef, String.Empty );
							// MD 4/6/12 - TFS101506
							//if ( Utilities.ParseA1CellAddress( cellRef, WorkbookFormat.Excel2007, out columnIndex, out rowIndex ) == false )
							if (Utilities.ParseA1CellAddress(cellRef, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out columnIndex, out rowIndex) == false)
								Utilities.DebugFail( "Could not parse address" );       
						}
						break;

					default:
						Utilities.DebugFail( "Unknown attribute: " + attributeName );
						break;
				}
			}

			Debug.Assert( rowIndex >= 0 && columnIndex >= 0, "The cell address was not parsed correctly." );

			string author = (string)authorListContext.GetItem( (int)authorId );

			WorksheetCellComment comment = new WorksheetCellComment();

			// MD 1/31/12 - TFS100573
			// Make sure the comment has a formatted string element in case there is any formatting.
			comment.Text.ConvertToFormattedStringElement();


			// MD 1/18/12 - 12.1 - Cell Format Updates
			manager.ContextStack.Push(comment);

			comment.Author = author;

			WorksheetCellCommentData commentHolder = new WorksheetCellCommentData( comment, authorId, rowIndex, columnIndex );
			commentsListContext.AddItem( commentHolder );

			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//WorkbookSerializationManager.FormattedStringHolder formattedStringHolder = new WorkbookSerializationManager.FormattedStringHolder( comment.Text );
			//
			//manager.ContextStack.Push( formattedStringHolder );
			// MD 4/12/11 - TFS67084
			// Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
			//manager.ContextStack.Push(comment.Text.Proxy.Element);
			manager.ContextStack.Push(comment.Text.Element);
		}

		#endregion Load

		#region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			ListContext<WorksheetCellCommentData> commentHoldersContext = 
				(ListContext<WorksheetCellCommentData>)manager.ContextStack[ typeof( ListContext<WorksheetCellCommentData> ) ];

			if ( commentHoldersContext == null )
			{
				Utilities.DebugFail( "Could not get the comment holders context from the context stack" );
				return;
			}

			WorksheetCellCommentData commentHolder = (WorksheetCellCommentData)commentHoldersContext.ConsumeCurrentItem();
			WorksheetCell cell = commentHolder.Comment.Cell;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell == null || cell.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			string attributeValue = null;

			// Add the 'ref' attribute.
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//attributeValue = CellAddress.GetCellReferenceString( cell.RowIndex, cell.ColumnIndex, true, true, manager.Workbook.CurrentFormat, cell, false, CellReferenceMode.A1 );
			// MD 2/20/12 - 12.1 - Table Support
			//attributeValue = CellAddress.GetCellReferenceString(cell.RowIndex, cell.ColumnIndex, true, true, manager.Workbook.CurrentFormat, cell.Row, cell.ColumnIndexInternal, false, CellReferenceMode.A1);
			attributeValue = CellAddress.GetCellReferenceString(cell.RowIndex, cell.ColumnIndex, true, true, manager.Workbook.CurrentFormat, cell.RowIndex, cell.ColumnIndexInternal, false, CellReferenceMode.A1);

			XmlElementBase.AddAttribute( element, CommentElement.RefAttributeName, attributeValue );

			// Add the 'authorId' attribute.
			attributeValue = XmlElementBase.GetXmlString( commentHolder.AuthorId, DataType.UInt32 );
			XmlElementBase.AddAttribute( element, CommentElement.AuthorIdAttributeName, attributeValue );

			manager.ContextStack.Push( commentHolder );

			// Add the 'text' element
			XmlElementBase.AddElement( element, CommentTextElement.QualifiedName );
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