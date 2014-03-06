using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
	internal class CommentsElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Comments">
		//    <sequence>
		//        <element name="authors" type="CT_Authors" minOccurs="1" maxOccurs="1"/>
		//        <element name="commentList" type="CT_CommentList" minOccurs="1" maxOccurs="1"/>
		//        <element name="extLst" minOccurs="0" type="CT_ExtensionList"/>
		//    </sequence>
		//</complexType> 

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "comments";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CommentsElement.LocalName;

		#endregion //Constants

		#region Base class overrides

		#region Type
		/// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
		/// </summary>
		public override XLSXElementType Type
		{
			get { return XLSXElementType.comments; }
		}
		#endregion Type

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[ typeof( ChildDataItem ) ];
			Debug.Assert( dataItem != null, "There was no child data item on the context stack." );

			List<WorksheetCellCommentData> commentsList = new List<WorksheetCellCommentData>();

			if ( dataItem != null )
				dataItem.Data = commentsList;

			ListContext<WorksheetCellCommentData> commentsListContext = new ListContext<WorksheetCellCommentData>( commentsList );
			manager.ContextStack.Push( commentsListContext );

			List<string> authorsList = new List<string>();
			ListContext<string> authorsListContext = new ListContext<string>( authorsList );
			manager.ContextStack.Push( authorsListContext );
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
				Utilities.DebugFail( "Could not get the worksheet from the context stack" );
				return;
			}

			List<WorksheetCellCommentData> commentHolders = new List<WorksheetCellCommentData>();
			List<string> authors = new List<string>();

			foreach ( WorksheetCellComment comment in worksheet.CommentShapes )
			{
				int authorId = authors.BinarySearch( comment.Author );

				if ( authorId < 0 )
				{
					authorId = ~authorId;
					authors.Insert( authorId, comment.Author );
				}

				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (comment.Cell.Worksheet == null)
				{
					Utilities.DebugFail("This is unexpected");
					return;
				}

				WorksheetCellCommentData commentHolder =
					new WorksheetCellCommentData( comment, (uint)authorId, comment.Cell.RowIndex, (short)comment.Cell.ColumnIndex );

				commentHolders.Add( commentHolder );
			}

			manager.ContextStack.Push( authors );
			manager.ContextStack.Push( commentHolders );

			// Add the 'authors' element
			XmlElementBase.AddElement( element, AuthorsElement.QualifiedName );

			// Add the 'commentList' element
			XmlElementBase.AddElement( element, CommentListElement.QualifiedName );
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