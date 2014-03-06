using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class MERGEDCELLSRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// MD 8/2/11 - TFS82965
			// Keep track of the regions we have already added in.
			Dictionary<Rectangle, object> regions = new Dictionary<Rectangle, object>();

			while ( manager.CurrentRecordStream.Position < manager.CurrentRecordStream.Length )
			{
				ushort numberOfCellRangeAddresses = manager.CurrentRecordStream.ReadUInt16();

				for ( int i = 0; i < numberOfCellRangeAddresses; i++ )
				{
					ushort firstRow = manager.CurrentRecordStream.ReadUInt16();
					ushort lastRow = manager.CurrentRecordStream.ReadUInt16();
					ushort firstColumn = manager.CurrentRecordStream.ReadUInt16();
					ushort lastColumn = manager.CurrentRecordStream.ReadUInt16();

					// MD 8/2/11 - TFS82965
					// Check to make sure we haven't already added in the merged region, in case there are duplicates.
					// If not, add the region to the set so we only add it once.
					// MD 2/17/12 - TFS102093
					// This was written incorrectly. The left value should be the firstColumn, not the lastRow.
					//Rectangle region = new Rectangle(lastRow, firstRow, lastColumn - firstColumn, lastRow - firstRow);
					Rectangle region = new Rectangle(firstColumn, firstRow, lastColumn - firstColumn, lastRow - firstRow);

					if (regions.ContainsKey(region))
						continue;

					regions.Add(region, null);

					worksheet.MergedCellsRegions.Add( firstRow, firstColumn, lastRow, lastColumn );
				}
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

			int regionIndex = 0;

			while ( true )
			{
				long numberOfRegionsAddress = manager.CurrentRecordStream.Position;

				int regionsWritten = 0;
				manager.CurrentRecordStream.Write( (ushort)0 );

				for ( ; regionIndex < worksheet.MergedCellsRegions.Count; regionIndex++ )
				{
					WorksheetMergedCellsRegion region = worksheet.MergedCellsRegions[ regionIndex ];

					if ( manager.CurrentRecordStream.BytesAvailableInCurrentBlock < 8 )
						break;

					manager.CurrentRecordStream.Write( (ushort)region.FirstRow );
					manager.CurrentRecordStream.Write( (ushort)region.LastRow );
					manager.CurrentRecordStream.Write( (ushort)region.FirstColumn );
					manager.CurrentRecordStream.Write( (ushort)region.LastColumn );

					regionsWritten++;
				}

				long endPosition = manager.CurrentRecordStream.Position;

				manager.CurrentRecordStream.Position = numberOfRegionsAddress;
				manager.CurrentRecordStream.Write( (ushort)regionsWritten );

				manager.CurrentRecordStream.Position = endPosition;
				manager.CurrentRecordStream.CapCurrentBlock();

				if ( regionIndex >= worksheet.MergedCellsRegions.Count )
					break;
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.MERGEDCELLS; }
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