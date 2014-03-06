using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;







using System.Drawing;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords

{
	internal class ClientAnchor : EscherRecordBase
	{
		private ShapePositioningMode positioningMode;
		private Point topLeft;
		private Point topLeftOffset;
		private Point bottomRight;
		private Point bottomRightOffset;

		public ClientAnchor( WorksheetShape shape )
			: base( 0x00, 0x0000, 18 )
		{
			this.positioningMode = shape.PositioningMode;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (shape.TopLeftCornerCell.Worksheet == null || shape.BottomRightCornerCell.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			this.topLeft = new Point( shape.TopLeftCornerCell.ColumnIndex, shape.TopLeftCornerCell.RowIndex );
			this.bottomRight = new Point( shape.BottomRightCornerCell.ColumnIndex, shape.BottomRightCornerCell.RowIndex );

			this.topLeftOffset = ClientAnchor.CellOffsetToClientAnchorPoint( shape.TopLeftCornerPosition );
			this.bottomRightOffset = ClientAnchor.CellOffsetToClientAnchorPoint( shape.BottomRightCornerPosition );
		}

		public ClientAnchor( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x00 );
			Debug.Assert( instance == 0x0000 );
		}

		private static Point CellOffsetToClientAnchorPoint( PointF cellOffset )
		{
			return new Point(
				(int)( ( cellOffset.X / 100 ) * 1024 ),
				(int)( ( cellOffset.Y / 100 ) * 256 ) );
		}

		private static PointF ClientAnchorPointToCellOffset( Point anchorPoint )
		{
			// MD 4/18/08 - BR32046
			// Apparently the anchor point can be greater than 1024 in the X direction (not sure about the Y direction, 
			// but we should apply the fix for that too). In this case, we should treat the anchor point as if the max 
			// value for the direction were used. This was verified by forcing a workbook to save an anchor point with 
			// a value of 2000 (well above 1024), and the resulting file displayed the image shape as if 1024 were 
			// written out. We should also make sure the value is not outside the minimum.
			//
			//return new PointF(
			//    (float)( ( anchorPoint.X * 100 ) / 1024.0 ),
			//    (float)( ( anchorPoint.Y * 100 ) / 256.0 ) );
			// MD 3/5/10 - TFS26342
			// There doesn't seem to be an upper bound on the anchor cell position. If it is over 100, the shape will still be positioned
			// as if the position we 100, but when the cell is made larger in the Excel UI, the shape will move out to its preferred location.
			//float cellOffsetX = Math.Max( 0.0f, Math.Min( 100.0f, ( anchorPoint.X * 100 ) / 1024.0f ) );
			//float cellOffsetY = Math.Max( 0.0f, Math.Min( 100.0f, ( anchorPoint.Y * 100 ) / 256.0f ) );
			float cellOffsetX = Math.Max(0.0f, ((float)anchorPoint.X * 100) / 1024.0f);
            float cellOffsetY = Math.Max(0.0f, ((float)anchorPoint.Y * 100) / 256.0f);

			return new PointF( cellOffsetX, cellOffsetY );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			this.positioningMode = (ShapePositioningMode)manager.CurrentRecordStream.ReadUInt16();

			ushort col1 = manager.CurrentRecordStream.ReadUInt16();
			ushort dx1 = manager.CurrentRecordStream.ReadUInt16();
			ushort row1 = manager.CurrentRecordStream.ReadUInt16();
			ushort dy1 = manager.CurrentRecordStream.ReadUInt16();

			ushort col2 = manager.CurrentRecordStream.ReadUInt16();
			ushort dx2 = manager.CurrentRecordStream.ReadUInt16();
			ushort row2 = manager.CurrentRecordStream.ReadUInt16();
			ushort dy2 = manager.CurrentRecordStream.ReadUInt16();

			this.topLeft = new Point( col1, row1 );
			this.topLeftOffset = new Point( dx1, dy1 );
			this.bottomRight = new Point( col2, row2 );
			this.bottomRightOffset = new Point( dx2, dy2 );

			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "There is no worksheet in the context stack." );
				return;
			}

			WorksheetShape shape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			if ( shape == null )
			{
				Utilities.DebugFail( "There is no shape in the context stack." );
				return;
			}

			// MD 8/24/07 - BR25924
			// We don't want to verify the positioning mode set
			//shape.PositioningMode = this.positioningMode;
			shape.SetPositioningMode( this.positioningMode, false );

			shape.BottomRightCornerPosition = ClientAnchor.ClientAnchorPointToCellOffset( this.bottomRightOffset );
			shape.TopLeftCornerPosition = ClientAnchor.ClientAnchorPointToCellOffset( this.topLeftOffset );

			shape.BottomRightCornerCell =	worksheet.Rows[ (int)bottomRight.Y ].Cells[ (int)bottomRight.X ];
			shape.TopLeftCornerCell =		worksheet.Rows[ (int)topLeft.Y ].Cells[ (int)topLeft.X ];
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			manager.CurrentRecordStream.Write( (ushort)this.positioningMode );

			manager.CurrentRecordStream.Write( (ushort)this.topLeft.X );
			manager.CurrentRecordStream.Write( (ushort)this.topLeftOffset.X );
			manager.CurrentRecordStream.Write( (ushort)this.topLeft.Y );
			manager.CurrentRecordStream.Write( (ushort)this.topLeftOffset.Y );

			manager.CurrentRecordStream.Write( (ushort)this.bottomRight.X );
			manager.CurrentRecordStream.Write( (ushort)this.bottomRightOffset.X );
			manager.CurrentRecordStream.Write( (ushort)this.bottomRight.Y );
			manager.CurrentRecordStream.Write( (ushort)this.bottomRightOffset.Y );
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			sb.Append( "Top Left Cell: " + this.topLeft );
			sb.Append( "\n" );

			sb.Append( "Top Left Offset: " + this.topLeftOffset );
			sb.Append( "\n" );

			sb.Append( "Bottom Right Cell: " + this.bottomRight );
			sb.Append( "\n" );

			sb.Append( "Bottom Right Offset: " + this.bottomRightOffset );
			sb.Append( "\n" );

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.ClientAnchor; }
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