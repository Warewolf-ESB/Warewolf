using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 7/20/2007 - BR25039
	// Added NOTE record type
	internal class NOTERecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort rowIndex = manager.CurrentRecordStream.ReadUInt16();

			// MD 4/12/11 - TFS67084
			// Use short instead of ushort so we don't have to cast.
			//ushort columnIndex = manager.CurrentRecordStream.ReadUInt16();
			short columnIndex = manager.CurrentRecordStream.ReadInt16();

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell = worksheet.Rows[ rowIndex ].Cells[ columnIndex ];
			//
			//Debug.Assert( cell.Comment == null, "The cell already has a comment applied." );
			WorksheetRow row = worksheet.Rows[rowIndex];

			Debug.Assert(row.GetCellCommentInternal(columnIndex) == null, "The cell already has a comment applied.");

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();
			ushort objId = manager.CurrentRecordStream.ReadUInt16();

			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString author = manager.CurrentRecordStream.ReadFormattedString( LengthType.SixteenBit );
			StringElement author = manager.CurrentRecordStream.ReadFormattedString(LengthType.SixteenBit);

			// MD 11/8/11 - TFS85193
			// Instead of loading the comments temporarily into the worksheet's shapes collection, we will now load them in the 
			// LoadedComments collection of the serialization manager.
			#region Old Code

			//foreach ( WorksheetShape shape in worksheet.Shapes )
			//{
			//    // MD 5/4/09 - TFS17197
			//    // Refactored because now the comment shapes will be created as if they were text boxes first.
			//    // They will only be converted to comments here if they are used in this NOTE record.
			//    #region Refactored

			//    /*
			//    // MD 9/2/08 - Cell Comments
			//    //WorksheetCellCommentShape comment = shape as WorksheetCellCommentShape;
			//    WorksheetCellComment comment = shape as WorksheetCellComment;

			//    if ( comment == null )
			//        continue;

			//    if ( comment.ObjRecords == null || comment.ObjRecords.Count == 0 )
			//    {
			//        Utilities.DebugFail( "The note shape does not contain obj records." );
			//        continue;
			//    }

			//    if ( ( (CommonObjectData)comment.ObjRecords[ 0 ] ).ObjectIdNumber == objId )
			//    {
			//        comment.Author = author.UnformattedString;
			//        comment.NoteOptionFlags = optionFlags;

			//        cell.Comment = comment;
			//        worksheet.Shapes.Remove( comment );
			//        return;
			//    }
			//    */

			//    #endregion Refactored
			//    WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;

			//    // MD 10/10/11 - TFS90805
			//    //if ( shapeWithText == null || shapeWithText.Type != ShapeType.TextBox )
			//    if (shapeWithText == null || 
			//        shapeWithText.Type2003.HasValue == false ||
			//        shapeWithText.Type2003.Value != ShapeType.TextBox)
			//        continue;

			//    // MD 10/30/11 - TFS90733
			//    //if ( shapeWithText.ObjRecords == null || shapeWithText.ObjRecords.Count == 0 )
			//    if ( shapeWithText.Obj == null || shapeWithText.Obj.Cmo == null )
			//    {
			//        Utilities.DebugFail("The note shape does not contain obj records.");
			//        continue;
			//    }

			//    // MD 10/30/11 - TFS90733
			//    //if ( ( (CommonObjectData)shapeWithText.ObjRecords[ 0 ] ).ObjectIdNumber == objId )
			//    if (shapeWithText.Obj.Cmo.Id == objId)
			//    {
			//        worksheet.Shapes.Remove( shape );

			//        WorksheetCellComment comment = new WorksheetCellComment( shapeWithText );

			//        comment.Author = author.UnformattedString;
			//        comment.NoteOptionFlags = optionFlags;

			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //cell.Comment = comment;
			//        row.SetCellCommentInternal(columnIndex, comment);
			//        return;
			//    }
			//}

			#endregion // Old Code
			for (int i = 0; i < manager.LoadedComments.Count; i++)
			{
				WorksheetCellComment comment = manager.LoadedComments[i];

				if (comment.Type2003.HasValue == false ||
					comment.Type2003.Value != ShapeType.TextBox)
				{
					Utilities.DebugFail("The comment should have the TextBox shape type.");
					continue;
				}

				if (comment.Obj == null || comment.Obj.Cmo == null)
				{
					Utilities.DebugFail("The note shape does not contain obj records.");
					continue;
				}

				if (comment.Obj.Cmo.Id != objId)
					continue;

				comment.Author = author.UnformattedString;
				comment.NoteOptionFlags = optionFlags;
				row.SetCellCommentInternal(columnIndex, comment);

				manager.LoadedComments.RemoveAt(i);
				return;
			}

			// MD 5/4/09
            Utilities.DebugFail("The comment with the specified object ID could not be found.");
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			WorksheetCell cell = (WorksheetCell)manager.ContextStack[ typeof( WorksheetCell ) ];

			if ( cell == null )
			{
                Utilities.DebugFail("There is no cell in the context stack.");
				return;
			}

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			if ( cell.Comment == null )
			{
                Utilities.DebugFail("The cell does not contain a note.");
				return;
			}

			// MD 10/30/11 - TFS90733
			//if ( cell.Comment.ObjRecords == null || cell.Comment.ObjRecords.Count == 0 )
			if (cell.Comment.Obj == null && cell.Comment.Obj.Cmo == null)
			{
                Utilities.DebugFail("The note shape does not contain obj records.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)cell.RowIndex );
			manager.CurrentRecordStream.Write( (ushort)cell.ColumnIndex );
			manager.CurrentRecordStream.Write( (ushort)cell.Comment.NoteOptionFlags );

			// MD 10/30/11 - TFS90733
			//manager.CurrentRecordStream.Write( ( (CommonObjectData)cell.Comment.ObjRecords[ 0 ] ).ObjectIdNumber );
			manager.CurrentRecordStream.Write( cell.Comment.Obj.Cmo.Id );

			manager.CurrentRecordStream.Write( cell.Comment.Author, LengthType.SixteenBit );

			// This record must have an odd number of bytes
			if ( manager.CurrentRecordStream.Length % 2 == 0 )
				manager.CurrentRecordStream.Write( (byte)0 );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.NOTE; }
		}
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