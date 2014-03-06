using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.StructuredStorage
{
	internal sealed class MasterSATStream : StructuredStorageStream
	{
		#region Member Variables

		private readonly int masterSATEntriesPerSector;

		private int[] headerMasterSAT;
		private List<int> cachedMasterSAT;

		private List<int> masterSATSectorIds;

		// MD 4/18/11 - TFS62026
		// Doing linear searches for entries in the master SAT gets progressively slower as the file gets bigger, so instead
		// we will store a reverse lookup table from the entry index to the master SAT indexes.
		private Dictionary<int, int> headerMasterSATLookupTable;
		private Dictionary<int, int> cachedMasterSATLookupTable;

		#endregion Member Variables

		#region Constructor

		public MasterSATStream( StructuredStorageManager structuredStorage, int firstMasterSATSectorId, int numberOfMasterSATSectors, int[] headerMasterSAT )
			: base( structuredStorage, firstMasterSATSectorId, numberOfMasterSATSectors * structuredStorage.GetSectorSize( false ) )
		{
			this.headerMasterSAT = headerMasterSAT;
			Debug.Assert( this.headerMasterSAT.Length == StructuredStorageManager.HeaderMasterSATLength );

			// MD 4/18/11 - TFS62026
			// Initialize the headerMasterSATLookupTable.
			this.headerMasterSATLookupTable = new Dictionary<int, int>();
			for (int i = 0; i < headerMasterSAT.Length; i++)
			{
				int sectorIndex = headerMasterSAT[i];
				if (0 <= sectorIndex)
					this.headerMasterSATLookupTable.Add(sectorIndex, i);
			}

			this.masterSATEntriesPerSector = ( this.sectorSize / 4 ) - 1;

			this.masterSATSectorIds = new List<int>();
			this.cachedMasterSAT = new List<int>();

			// MD 4/18/11 - TFS62026
			this.cachedMasterSATLookupTable = new Dictionary<int, int>();

			int masterSATSectorId = firstMasterSATSectorId;
			while ( masterSATSectorId >= 0 )
			{
				this.StorageManager.SeekToSector( masterSATSectorId, false );
				masterSATSectorIds.Add( masterSATSectorId );

				for ( int i = 0; i < this.sectorSize - 4; i += 4 )
				{
					// MD 4/18/11 - TFS62026
					// Store the value in a local variable so we can also initialize the cachedMasterSATLookupTable.
					//this.cachedMasterSAT.Add( this.ReadInt32() );
					int sectorIndex = this.ReadInt32();
					this.cachedMasterSAT.Add(sectorIndex);

					if (0 <= sectorIndex)
						this.cachedMasterSATLookupTable.Add(sectorIndex, this.cachedMasterSAT.Count - 1);
				}

				masterSATSectorId = this.ReadInt32();
			}
		}

		#endregion Constructor

		#region Base Class Overrides

		#region AllocateNewSector

		protected override int AllocateNewSector( int lastSectorId )
		{
			int newSector = base.AllocateNewSector( lastSectorId );
			this.masterSATSectorIds.Add( newSector );
			return newSector;
		}

		#endregion AllocateNewSector

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			try
			{
				if ( disposing && this.StorageManager.IsDirty )
				{
					this.Position = 0;

					int valueIndex = 0;
					for ( int sectorIndex = 0; sectorIndex < this.masterSATSectorIds.Count; sectorIndex++ )
					{
						for ( int i = 0; i < this.masterSATEntriesPerSector; i++ )
						{
							this.WriteInt32( this.cachedMasterSAT[ valueIndex ] );
							valueIndex++;
						}

						if ( sectorIndex == this.masterSATSectorIds.Count - 1 )
							this.WriteInt32( -1 );
						else
							this.WriteInt32( this.masterSATSectorIds[ sectorIndex + 1 ] );
					}
				}
			}
			finally
			{
				base.Dispose( disposing );
			}
		}

		#endregion Dispose

		#region GetNextSectorId

		internal override int GetNextSectorId( int lastSectorId, ref int steps )
		{
			if ( steps < 0 )
			{
				int i = this.masterSATSectorIds.Count;
				while ( steps < 0 )
				{
					if ( this.masterSATSectorIds[ 0 ] == lastSectorId )
						return StructuredStorageManager.EndOfStreamSectorId;

					// MD 5/23/07 - BR23095
					// Keep track of whether we found the next sector on this step
					bool foundSector = false;

					for ( i = 1; i < this.masterSATSectorIds.Count; i++ )
					{
						if ( this.masterSATSectorIds[ i ] == lastSectorId )
						{
							lastSectorId = this.masterSATSectorIds[ i - 1 ];

							// MD 5/23/07 - BR23095
							// We found the sector on this step
							foundSector = true;

							break;
						}
					}

					// MD 5/23/07 - BR23095
					// If we couldn't find the sector on this step, return the end of stream identifier
					//if ( lastSectorId < 0 )
					if ( lastSectorId < 0 || foundSector == false )
						return StructuredStorageManager.EndOfStreamSectorId;

					steps++;
				}

				// MD 5/23/07 - BR23095
				// If we completed all steps, return the sector id
				if ( steps == 0 )
					return lastSectorId;

				Debug.Assert( i == this.masterSATSectorIds.Count, "Couldnt find the sector" );
				return StructuredStorageManager.EndOfStreamSectorId;
			}
			else
			{
				int i = this.masterSATSectorIds.Count;
				while ( steps > 0 )
				{
					// MD 5/23/07 - BR23095
					// Keep track of whether we found the next sector on this step
					bool foundSector = false;

					for ( i = 0; i < this.masterSATSectorIds.Count - 1; i++ )
					{
						if ( this.masterSATSectorIds[ i ] == lastSectorId )
						{
							lastSectorId = this.masterSATSectorIds[ i + 1 ];

							// MD 5/23/07 - BR23095
							// We found the sector on this step
							foundSector = true;

							break;
						}
					}

					// MD 5/23/07 - BR23095
					// If we couldn't find the sector on this step, return the end of stream identifier
					//if ( lastSectorId < 0 )
					if ( lastSectorId < 0 || foundSector == false )
						return StructuredStorageManager.EndOfStreamSectorId;

					steps--;
				}

				// MD 5/23/07 - BR23095
				// If we completed all steps, return the sector id
				if ( steps == 0 )
					return lastSectorId;

				Debug.Assert( i == this.masterSATSectorIds.Count - 1, "Couldnt find the sector" );
				return StructuredStorageManager.EndOfStreamSectorId;
			}
		}

		#endregion GetNextSectorId

		#region Type

		protected override StreamType Type
		{
			get { return StreamType.MasterSAT; }
		}

		#endregion Type

		#endregion Base Class Overrides

		#region Methods

		#region AddNewSATEntry

		internal void AddNewSATEntry( int SATSectorId )
		{
			// Try to find a free block in the header master SAT
			for ( int i = 0; i < this.headerMasterSAT.Length; i++ )
			{
				int sectorId = this.headerMasterSAT[ i ];

				if ( sectorId < 0 )
				{
					Debug.Assert( sectorId == StructuredStorageManager.FreeSectorId );
					this.headerMasterSAT[ i ] = SATSectorId;

					// MD 4/18/11 - TFS62026
					// When a new entry is added to the headerMasterSAT, also add it to the headerMasterSATLookupTable.
					this.headerMasterSATLookupTable.Add(SATSectorId, i);

					return;
				}
			}

			// Try to find a free block in the current master SAT
			for ( int i = 0; i < this.cachedMasterSAT.Count; i++ )
			{
				int sectorId = this.cachedMasterSAT[ i ];

				if ( sectorId < 0 )
				{
					Debug.Assert( sectorId == StructuredStorageManager.FreeSectorId );
					this.cachedMasterSAT[ i ] = SATSectorId;

					// MD 4/18/11 - TFS62026
					// When a new entry is added to the cachedMasterSAT, also add it to the cachedMasterSATLookupTable.
					this.cachedMasterSATLookupTable.Add(SATSectorId, i);

					return;
				}
			}

			// If we couldn't find a free sector, write a new sector for the SAT and initialize it with free space ids
			this.cachedMasterSAT.Add( SATSectorId );

			// MD 4/18/11 - TFS62026
			// When a new entry is added to the cachedMasterSAT, also add it to the cachedMasterSATLookupTable.
			this.cachedMasterSATLookupTable.Add(SATSectorId, this.cachedMasterSAT.Count - 1);

			// Fill the rest of the entries in the new sector with the free space id
			for ( int i = 1; i < this.masterSATEntriesPerSector; i++ )
				this.cachedMasterSAT.Add( StructuredStorageManager.FreeSectorId );

			// Add a new sector to hold the new cache
			this.SetLength( this.Length + this.sectorSize );
		}

		#endregion AddNewSATEntry

		#region GetNextSATSector

		internal int GetNextSATSector( int lastSectorId )
		{
			// MD 4/18/11 - TFS62026
			// Doing linear searches for entries in the master SAT gets progressively slower as the file gets bigger, so instead
			// use a reverse lookup table from the entry index to the master SAT indexes.
			#region Refactored

			//int i;
			//for ( i = 0; i < this.headerMasterSAT.Length - 1; i++ )
			//{
			//    if ( this.headerMasterSAT[ i ] == lastSectorId )
			//        return this.headerMasterSAT[ i + 1 ];
			//}
			//
			//if ( this.headerMasterSAT[ this.headerMasterSAT.Length - 1 ] == lastSectorId )
			//{
			//    if ( this.cachedMasterSAT.Count == 0 )
			//        return -1;
			//
			//    return this.cachedMasterSAT[ 0 ];
			//}
			//
			//for ( i = 0; i < this.cachedMasterSAT.Count - 1; i++ )
			//{
			//    if ( this.cachedMasterSAT[ i ] == lastSectorId )
			//        return this.cachedMasterSAT[ i + 1 ];
			//}
			//
			//Debug.Assert( i == this.cachedMasterSAT.Count - 1, "Couldn't find the sector" );
			//return -1; 

			#endregion // Refactored
			int headerMasterSATIndex;
			if (this.headerMasterSATLookupTable.TryGetValue(lastSectorId, out headerMasterSATIndex))
			{
				if (headerMasterSATIndex == this.headerMasterSAT.Length - 1)
				{
					if (this.cachedMasterSAT.Count == 0)
						return -1;

					return this.cachedMasterSAT[0];
				}

				return this.headerMasterSAT[headerMasterSATIndex + 1];
			}

			int cacheMasterSATIndex;
			if (this.cachedMasterSATLookupTable.TryGetValue(lastSectorId, out cacheMasterSATIndex))
			{
				if (cacheMasterSATIndex == this.cachedMasterSAT.Count - 1)
					return -1;

				return this.cachedMasterSAT[cacheMasterSATIndex + 1];
			}

			return -1;
		}

		#endregion GetNextSATSector

		#region GetPreviousSATSector

		internal int GetPreviousSATSector( int lastSectorId )
		{
			// MD 4/18/11 - TFS62026
			// Doing linear searches for entries in the master SAT gets progressively slower as the file gets bigger, so instead
			// use a reverse lookup table from the entry index to the master SAT indexes.
			#region Refactored

			//if ( this.headerMasterSAT[ 0 ] == lastSectorId )
			//    return -1;
			//
			//int i;
			//for ( i = 1; i < this.headerMasterSAT.Length; i++ )
			//{
			//    if ( this.headerMasterSAT[ i ] == lastSectorId )
			//        return this.headerMasterSAT[ i - 1 ];
			//}
			//
			//if ( this.cachedMasterSAT.Count == 0 )
			//    return -1;
			//
			//if ( this.cachedMasterSAT[ 0 ] == lastSectorId )
			//    return this.headerMasterSAT[ this.headerMasterSAT.Length - 1 ];
			//
			//for ( i = 1; i < this.cachedMasterSAT.Count; i++ )
			//{
			//    if ( this.cachedMasterSAT[ i ] == lastSectorId )
			//        return this.cachedMasterSAT[ i - 1 ];
			//}
			//
			//Debug.Assert( i == this.cachedMasterSAT.Count, "Couldn't find the sector" );
			//return -1;

			#endregion // Refactored
			int headerMasterSATIndex;
			if (this.headerMasterSATLookupTable.TryGetValue(lastSectorId, out headerMasterSATIndex))
			{
				if (headerMasterSATIndex == 0)
					return -1;

				return this.headerMasterSAT[headerMasterSATIndex - 1];
			}

			int cacheMasterSATIndex;
			if (this.cachedMasterSATLookupTable.TryGetValue(lastSectorId, out cacheMasterSATIndex) && 1 <= cacheMasterSATIndex)
			{
				if (cacheMasterSATIndex == 0)
					return this.headerMasterSAT[this.headerMasterSAT.Length - 1];

				return this.cachedMasterSAT[cacheMasterSATIndex - 1];
			}

			return -1;
		}

		#endregion GetPreviousSATSector

		#endregion Methods

		#region Properties

		#region HeaderMasterSAT

		public int[] HeaderMasterSAT
		{
			get { return this.headerMasterSAT; }
		}

		#endregion HeaderMasterSAT

		#endregion Properties
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