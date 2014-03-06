using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class INDEXRecord : Biff8RecordBase
	{
		public const long PositionOfDefColWidthAddress = 12;
		public const long PositionOfDBCellAddresses = 16;

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			uint mustBeZero = manager.CurrentRecordStream.ReadUInt32();
			Debug.Assert( mustBeZero == 0 );

			uint firstUsedRow = manager.CurrentRecordStream.ReadUInt32();
			uint firstRowOfUnusedTailOfSheet = manager.CurrentRecordStream.ReadUInt32();

			Debug.Assert( INDEXRecord.PositionOfDefColWidthAddress == manager.CurrentRecordStream.Position );

			manager.CurrentRecordStream.ReadUInt32(); // stream position of DEFCOLWIDTH record

			if ( firstUsedRow != firstRowOfUnusedTailOfSheet )
			{
				int numDBCEllRecords = (int)( ( ( firstRowOfUnusedTailOfSheet - firstUsedRow - 1 ) / 32 ) + 1 );

				Debug.Assert( INDEXRecord.PositionOfDBCellAddresses == manager.CurrentRecordStream.Position );

				for ( int i = 0; i < numDBCEllRecords; i++ )
				{
					manager.CurrentRecordStream.ReadUInt32(); // stream position of DBCELL record
				}
			}
			else
			{
				Debug.Assert( firstUsedRow == 0 );
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (uint)0 );
			manager.CurrentRecordStream.Write( (uint)worksheet.FirstRow );
			manager.CurrentRecordStream.Write( (uint)worksheet.FirstRowInUndefinedTail );

			Debug.Assert( INDEXRecord.PositionOfDefColWidthAddress == manager.CurrentRecordStream.Position );

			manager.CurrentRecordStream.Write( (uint)0 );

			Debug.Assert( INDEXRecord.PositionOfDBCellAddresses == manager.CurrentRecordStream.Position );

			if ( worksheet.FirstRow != worksheet.FirstRowInUndefinedTail )
			{
				int numDBCEllRecords = (int)( ( ( worksheet.FirstRowInUndefinedTail - worksheet.FirstRow - 1 ) / 32 ) + 1 );

				// The actual DBCell positions will be written later
				for ( int i = 0; i < numDBCEllRecords; i++ )
					manager.CurrentRecordStream.Write( (uint)0 );
			}
			else
			{
				Debug.Assert( worksheet.FirstRow == 0 );
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.INDEX; }
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