using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class DBCELLRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 9/23/09 - TFS19150
			manager.OnDBCellRecordLoaded();

			// We don't need any data from this record
			// 
			//int rowsInBlock = (int)( manager.CurrentRecordStream.Length - 4 ) / 2;
			//
			//manager.CurrentRecordStream.ReadUInt32(); // negative offset to first row in block
			//
			//for ( int i = 0; i < rowsInBlock; i++ )
			//    manager.CurrentRecordStream.ReadUInt16(); // offset to next first cell in row
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			DBCELLInfo dbcellInfo = (DBCELLInfo)manager.ContextStack[ typeof( DBCELLInfo ) ];

			if ( dbcellInfo == null )
			{
                Utilities.DebugFail("There is no info for this db cell record.");
				return;
			}

			manager.CurrentRecordStream.SyncWorkbookStreamPosition();

			// MD 4/18/11 - TFS62026
			// This is not the best way to get the start position of the record because it is dependant on the BiffRecordStream being implemented
			// in a specific way. Instead, ask the stream where the record starts.
			//uint startPosition = (uint)( manager.WorkbookStream.Position - 4 );
			uint startPosition = (uint)manager.CurrentRecordStream.GetStartOfRecord();

			// The ROW record size with header is 20
			uint positionAfterFirstRow = dbcellInfo.FirstRowPosition + 20;

			List<ushort> firstCellInRowOffsets = new List<ushort>();
			firstCellInRowOffsets.Add( (ushort)( dbcellInfo.FirstCellInRowPositions[ 0 ] - positionAfterFirstRow ) );

			for ( int i = 0; i < dbcellInfo.FirstCellInRowPositions.Count - 1; i++ )
			{
				firstCellInRowOffsets.Add( (ushort)(
					dbcellInfo.FirstCellInRowPositions[ i + 1 ] -
					dbcellInfo.FirstCellInRowPositions[ i ]
					) );
			}

			manager.CurrentRecordStream.Write( (uint)( startPosition - dbcellInfo.FirstRowPosition ) );

			foreach ( ushort firstCellInRowOffset in firstCellInRowOffsets )
				manager.CurrentRecordStream.Write( firstCellInRowOffset );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.DBCELL; }
		}

		public class DBCELLInfo
		{
			private List<uint> firstCellInRowPositions;
			private uint firstRowPosition;

			public DBCELLInfo( uint firstRowPosition, List<uint> firstCellInRowPositions )
			{
				this.firstCellInRowPositions = firstCellInRowPositions;
				this.firstRowPosition = firstRowPosition;
			}

			public List<uint> FirstCellInRowPositions
			{
				get { return this.firstCellInRowPositions; }
			}

			public uint FirstRowPosition
			{
				get { return this.firstRowPosition; }
			}
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