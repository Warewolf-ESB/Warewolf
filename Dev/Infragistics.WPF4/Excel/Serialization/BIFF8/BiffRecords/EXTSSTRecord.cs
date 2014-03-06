using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class EXTSSTRecord : Biff8RecordBase
	{
		private const ushort BucketSize = 8;

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			ushort numberOfStringsInPortion = manager.CurrentRecordStream.ReadUInt16();

			// MD 10/28/08 - TFS9670
			// This is never supposed to be less than 8, but when it is 0, rather than throw a divide by zero exception below, just bail out.
			if ( numberOfStringsInPortion == 0 )
			{
                Utilities.DebugFail("I'm not really sure why this is happening, but I still don't know what it means, so just bail because it doesn't seem to cause any problems.");
				return;
			}

			int numberOfStrings = manager.SharedStringTable.Count;

			if ( numberOfStrings > 0 )
			{
				int numberOfPortions = ( ( numberOfStrings - 1 ) / numberOfStringsInPortion ) + 1;

				for ( int i = 0; i < numberOfPortions; i++ )
				{
					manager.CurrentRecordStream.ReadUInt32(); // stream position of 1st string in portion
					manager.CurrentRecordStream.ReadUInt16(); // record position of 1st string in portions
					manager.CurrentRecordStream.ReadUInt16(); // Not used

					// MD 10/30/09 - TFS24320
					// If this was written out by a 3rd party program, this record may be written out incorrectly. If so, bail
					// if we hit the end of the stream so we don't get an exception.
					if ( manager.CurrentRecordStream.Position == manager.CurrentRecordStream.Length )
						break;
				}
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			manager.CurrentRecordStream.Write( BucketSize );

			// MD 2/1/12 - TFS100573
			// The SharedStringTable collection now only stores additional strings not in the workbook's string table during 
			// a save operation, so use the SharedStringCountDuringSave instead.
			//if ( manager.SharedStringTable.Count > 0 )
			if (manager.SharedStringCountDuringSave > 0)
			{
				// MD 11/3/10 - TFS49093
				// Instead of storing this position info on a holder for each string where 1/7th of them will be unused
				// we are now storing it in a collection on the manager.
				//for ( int i = 0; i < manager.SharedStringTable.Count; i += 8 )
				//{
				//    WorkbookSerializationManager.FormattedStringHolder holder = manager.SharedStringTable[ i ];
				//
				//    manager.CurrentRecordStream.Write( (uint)holder.AbsolutePosition );
				//    manager.CurrentRecordStream.Write( (ushort)holder.OffsetInRecordBlock );
				//
				//    // Reserved; must be 0 
				//    manager.CurrentRecordStream.Write( (ushort)0 );
				//}
				for (int i = 0; i < manager.EXTSSTData.Count; i++)
				{
					BIFF8WorkbookSerializationManager.EXTSSTItem item = manager.EXTSSTData[i];

					manager.CurrentRecordStream.Write((uint)item.AbsolutePosition);
					manager.CurrentRecordStream.Write((ushort)item.OffsetInRecordBlock);

					// Reserved; must be 0 
					manager.CurrentRecordStream.Write((ushort)0);
				}
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.EXTSST; }
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