using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.StructuredStorage
{
	internal abstract class SATStreamBase : StructuredStorageStream
	{
		#region Member Variables

		protected readonly int SATEntriesPerSector;
		private List<SATEntry> cachedSAT;

		// MD 4/18/11 - TFS62026
		// Store a sorted list of free sectors so we don't have to do a linear walk through the cachedSAT to find a free sector,
		// because that will get progressively slower for large streams.
		private List<int> freeSectorIds;

		#endregion Member Variables

		#region Constructor

		public SATStreamBase( StructuredStorageManager structuredStorage, int firstSATSectorId, int numberOfSATSectors )
			: base( structuredStorage, firstSATSectorId, numberOfSATSectors * structuredStorage.GetSectorSize( false ) )
		{
			this.SATEntriesPerSector = structuredStorage.GetSectorSize( false ) / 4;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			try
			{
				if ( disposing && this.StorageManager.IsDirty )
				{
					this.Position = 0;

					for ( int i = 0; i < this.cachedSAT.Count; i++ )
						this.WriteInt32( this.cachedSAT[ i ].NextSector );
				}
			}
			finally
			{
				base.Dispose( disposing );
			}
		}

		#endregion Dispose

		#endregion Base Class Overrides

		#region Methods

		#region AllocateNewSectorForStream

		internal int AllocateNewSectorForStream( int lastSectorId, StructuredStorageStream.StreamType streamType )
		{
			// MD 4/18/11 - TFS62026
			// Doing a linear walk through the cachedSAT to find a free sector will get progressively slower for large streams,
			// so use the sorted freeSectorIds list instead.
			#region Refactored

			//// MD 5/23/07 - BR23095
			//// Keep track of whether we found a free sector
			//bool foundFreeSector = false;
			//
			////// Try to find a free block in the current SAT
			//int freeSectorId;
			//int count;
			//// MD 5/23/07 - BR23095
			//// Start at the last sector id, because chances are the free sector will be after the last one
			////for ( freeSectorId = 0, count = this.cachedSAT.Count; freeSectorId < count; freeSectorId++ )
			//for (freeSectorId = lastSectorId + 1, count = this.cachedSAT.Count; freeSectorId < count; freeSectorId++)
			//{
			//    int currentSectorId = this.cachedSAT[freeSectorId].NextSector;
			//
			//    if (currentSectorId == StructuredStorageManager.FreeSectorId)
			//    {
			//        // MD 5/23/07 - BR23095
			//        // Mark the flag to indicate we found the free sector
			//        foundFreeSector = true;
			//        break;
			//    }
			//}
			//
			//// MD 5/23/07 - BR23095
			//// If we didn't find the free sector after the last sector id, check before it
			//if (foundFreeSector == false)
			//{
			//    for (freeSectorId = 0; freeSectorId < lastSectorId; freeSectorId++)
			//    {
			//        int currentSectorId = this.cachedSAT[freeSectorId].NextSector;
			//
			//        if (currentSectorId == StructuredStorageManager.FreeSectorId)
			//        {
			//            foundFreeSector = true;
			//            break;
			//        }
			//    }
			//} 

			#endregion // Refactored
			bool foundFreeSector = false;
			int freeSectorId = Math.Max(0, lastSectorId);
			if (this.freeSectorIds.Count > 0)
			{
				foundFreeSector = true;
				freeSectorId = this.freeSectorIds[0];
				this.freeSectorIds.RemoveAt(0);
			}

			// If we couldn't find a free sector, write a new sector for the SAT
			// and initialize it will all free space ids
			// MD 5/23/07 - BR23095
			// Check the flag to see if we found the free sector, not some other condition that could change depending 
			// on the seraching algorithm
			//if ( freeSectorId >= this.cachedSAT.Count )
			if ( foundFreeSector == false )
			{
				for ( int i = 0; i < this.SATEntriesPerSector; i++ )
				{
					// MD 4/18/11 - TFS62026
					// Add the free entry to the freeSectorIds list.
					this.freeSectorIds.Add(this.cachedSAT.Count);

					this.cachedSAT.Add( SATEntry.FreeEntry );
				}

				// Add a new sector to hold the new cache
				this.SetLength( this.Length + this.sectorSize );

				while ( freeSectorId < this.cachedSAT.Count )
				{
					int currentSectorId = this.cachedSAT[ freeSectorId ].NextSector;

					if ( currentSectorId == StructuredStorageManager.FreeSectorId )
					{
						// MD 4/18/11 - TFS62026
						// Since the sector will no longer be free, remove it from the freeSectorIds collection.
						this.freeSectorIds.Remove(freeSectorId);

						// MD 5/23/07 - BR23095
						// Mark the flag to indicate we found the free sector
						foundFreeSector = true;

						break;
					}

					freeSectorId++;
				}

				// MD 5/23/07 - BR23095
				// Check the flag to see if we found the free sector, not some other condition that could change depending 
				// on the seraching algorithm
				//if ( freeSectorId == this.cachedSAT.Count )
				if ( foundFreeSector == false )
				{
					Utilities.DebugFail( "All newly added entries couldn't have been used up already." );
					return this.AllocateNewSectorForStream( lastSectorId, streamType );
				}
			}

			if ( lastSectorId >= 0 && streamType == StreamType.Normal )
			{
				this.cachedSAT[ lastSectorId ].NextSector = freeSectorId;

				// MD 5/23/07 - BR23095
				// We never set the previous sector id
				this.cachedSAT[ freeSectorId ].PreviousSector = lastSectorId;
			}

			switch ( streamType )
			{
				case StreamType.MasterSAT:
					this.cachedSAT[ freeSectorId ].NextSector = StructuredStorageManager.MasterSATSectorId;
					break;

				case StreamType.Normal:
					this.cachedSAT[ freeSectorId ].NextSector = StructuredStorageManager.EndOfStreamSectorId;
					break;

				case StreamType.SAT:
					this.cachedSAT[ freeSectorId ].NextSector = StructuredStorageManager.SATSectorId;
					break;

				default:
					Utilities.DebugFail( "Unknown stream type" );
					break;
			}

			return freeSectorId;
		}

		#endregion AllocateNewSectorForStream

		#region GetNextSectorIdInSAT

		public int GetNextSectorIdInSAT( int lastSectorId, ref int steps )
		{
			while ( steps < 0 )
			{
				lastSectorId = this.cachedSAT[ lastSectorId ].PreviousSector;

				if ( lastSectorId < 0 )
					return StructuredStorageManager.EndOfStreamSectorId;

				steps++;
			}

			while ( steps > 0 )
			{
				lastSectorId = this.cachedSAT[ lastSectorId ].NextSector;

				if ( lastSectorId < 0 )
					return StructuredStorageManager.EndOfStreamSectorId;

				steps--;
			}

			return lastSectorId;
		}

		#endregion GetNextSectorIdInSAT

		#region ReadSATContents

		protected void ReadSATContents( int firstSATSectorId, int numberOfSATSectors )
		{
			int totalSATEntries = numberOfSATSectors * this.SATEntriesPerSector;
			this.cachedSAT = new List<SATEntry>( totalSATEntries );

			// MD 4/18/11 - TFS62026
			this.freeSectorIds = new List<int>();

			if ( firstSATSectorId < 0 )
			{
				Debug.Assert( numberOfSATSectors == 0 );
				return;
			}

			for ( int i = 0; i < totalSATEntries; i++ )
				this.cachedSAT.Add( new SATEntry( StructuredStorageManager.EndOfStreamSectorId, this.ReadInt32() ) );

			for ( int i = 0; i < this.cachedSAT.Count; i++ )
			{
				SATEntry entry = this.cachedSAT[ i ];

				// MD 5/4/12 - TFS110016
				// Apparently, the NextSector could be out of range, in which case we should just ignore the NextSector value.
				//if ( entry.NextSector >= 0 )
				//    this.cachedSAT[ entry.NextSector ].PreviousSector = i;
				if (entry.NextSector >= 0 && entry.NextSector < this.cachedSAT.Count)
					this.cachedSAT[entry.NextSector].PreviousSector = i;
				// MD 4/18/11 - TFS62026
				// Add all free sectors to the freeSectorIds list.
				else if (entry.NextSector == StructuredStorageManager.FreeSectorId)
					this.freeSectorIds.Add(i);
			}
		}

		#endregion ReadSATContents

		#region RemoveStream

		internal void RemoveStream( StructuredStorageStream stream )
		{
			int sectorId = stream.FirstSectorId;

			while ( sectorId >= 0 )
			{
				int steps = 1;
				int nextSectorId = stream.GetNextSectorId( sectorId, ref steps );
				this.cachedSAT[ sectorId ].NextSector = StructuredStorageManager.FreeSectorId;

				// MD 4/18/11 - TFS62026
				// Since the removed sector is now free, add it to the freeSectorIds list.
				int index = this.freeSectorIds.BinarySearch(sectorId);
				Debug.Assert(index < 0, "The allocated sector should not have been in the freeSectorIds collection");
				if (index < 0)
					this.freeSectorIds.Insert(~index, sectorId);

				sectorId = nextSectorId;
			}
		}

		#endregion RemoveStream

		#region SetSATContents

		protected void SetSATContents( List<SATEntry> SAT )
		{
			Debug.Assert( this.cachedSAT == null );
			Debug.Assert( this.Length == SAT.Count * 4 );
			this.cachedSAT = SAT;

			// MD 4/18/11 - TFS62026
			// Initialize the freeSectorIds list with all free sectors in the initial SAT.
			this.freeSectorIds = new List<int>();
			for (int i = 0; i < this.cachedSAT.Count; i++)
			{
				if (this.cachedSAT[i].NextSector == StructuredStorageManager.FreeSectorId)
					this.freeSectorIds.Add(i);
			}
		}

		#endregion SetSATContents

		#endregion Methods

		[DebuggerDisplay( "Previous: {PreviousSector}, Next: {NextSector}" )]
		public class SATEntry
		{
			public int PreviousSector;
			public int NextSector;

			public SATEntry( int previousSector, int nextSector )
			{
				this.PreviousSector = previousSector;
				this.NextSector = nextSector;
			}

			public static SATEntry FreeEntry
			{
				get
				{
					return new SATEntry( StructuredStorageManager.EndOfStreamSectorId, StructuredStorageManager.FreeSectorId );
				}
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