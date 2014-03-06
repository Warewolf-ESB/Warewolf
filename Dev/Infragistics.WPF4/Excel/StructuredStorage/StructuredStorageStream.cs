using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.StructuredStorage

{
	internal abstract class StructuredStorageStream : Stream
	{
		#region Member Variables

		private StructuredStorageManager storageManager;

		private Stream containerStream;

		private int firstSectorId;
		protected int sectorSize;
		protected bool isShortStream;

		private int length;
		private int position;

		private int currentSectorId;
		private int currentSectorIndex;
		private int positionInCurrentSector;

		private long cachedContainerStreamPosition;

		private byte[] readBuffer;

		#endregion Member Variables

		#region Constructor

		public StructuredStorageStream( StructuredStorageManager structuredStorage, int firstSectorId, int length )
			: this( structuredStorage, firstSectorId, length, false ) { }

		public StructuredStorageStream( StructuredStorageManager structuredStorage, int firstSectorId, int length, bool isShortStream )
		{
			this.readBuffer = new byte[ 8 ];

			this.storageManager = structuredStorage;
			this.firstSectorId = firstSectorId;
			this.length = length;
			this.isShortStream = isShortStream;

			Debug.Assert( this.firstSectorId >= 0 || this.length == 0 );

			this.ResetCache();
		}

		#endregion Constructor

		#region Base Class Overrides

		#region CanRead

		public override bool CanRead
		{
			get { return true; }
		}

		#endregion CanRead

		#region CanSeek

		public override bool CanSeek
		{
			get { return true; }
		}

		#endregion CanSeek

		#region CanWrite

		public override bool CanWrite
		{
			get { return true; }
		}

		#endregion CanWrite

		#region Flush

		public override void Flush() { }

		#endregion Flush

		#region Length

		public override long Length
		{
			get { return this.length; }
		}

		#endregion Length

		#region Position

		public override long Position
		{
			get { return this.position; }
			set
			{
				if ( this.position == value )
					return;

				if ( value < 0 )
				{
					Utilities.DebugFail( "Invalid stream position" );
					return;
				}

				int difference = (int)( value - this.position );
				int newPositionInCurrentSector = this.positionInCurrentSector + difference;

				this.position = (int)value;

				if ( 0 <= newPositionInCurrentSector && newPositionInCurrentSector < this.sectorSize &&
					this.cachedContainerStreamPosition == this.containerStream.Position )
				{
					this.positionInCurrentSector = newPositionInCurrentSector;
					this.cachedContainerStreamPosition += difference;
					this.containerStream.Position = this.cachedContainerStreamPosition;
				}
				else
				{
					// MD 5/23/07 - BR23095
					// Save the old position values so we can restore them later
					int oldCurrentSectorIndex = this.currentSectorIndex;
					int oldCurrentSectorId = this.currentSectorId;
					int oldPositionInCurrentSector = this.positionInCurrentSector;

					this.SyncPositionMembers();

					if ( this.currentSectorId >= 0 )
					{
						this.cachedContainerStreamPosition = this.storageManager.GetSectorPosition(
							this.currentSectorId,
							this.positionInCurrentSector,
							this.isShortStream );
					}
					// MD 5/23/07 - BR23095
					// Restore the old position values but invalidate the cached container stream position.
					// Then later, when we are allocating new sectors, we can start with the current sector id
					// instead of starting with the first sector id.
					else
					{
						this.currentSectorIndex = oldCurrentSectorIndex;
						this.currentSectorId = oldCurrentSectorId;
						this.positionInCurrentSector = oldPositionInCurrentSector;

						this.cachedContainerStreamPosition = -1;
					}
				}
			}
		}

		#endregion Position

		#region Read

		public override int Read( byte[] buffer, int offset, int count )
		{
			#region Do safety checks

			if ( buffer == null )
			{
				Utilities.DebugFail( "The buffer cannot be null." );
				return 0;
			}

			if ( offset < 0 )
			{
				Utilities.DebugFail( "The offset cannot be less than zero." );
				return 0;
			}

			if ( count < 0 )
			{
				Utilities.DebugFail( "The count cannot be less than zero." );
				return 0;
			}

			if ( offset + count > buffer.Length )
			{
				Utilities.DebugFail( "Cannot write past the end of the buffer." );
				return 0;
			}

			#endregion Do safety checks

			// Determine how many bytes should actually be read from the record
			int bytesToRead = Math.Min( count, (int)( this.length - this.position ) );

			if ( bytesToRead < 0 )
				return 0;

			int bytesRead = 0;

			while ( bytesRead < bytesToRead )
			{
				// MD 10/5/07
				// Found while fixing BR26292
				// We could get into an infinite loop here if we run into a problem. If no bytes were read on this pass, 
				// none will be read on subsequent passes, so break out of the loop
				//bytesRead += this.ReadFromCurrentSector( buffer, offset + bytesRead, bytesToRead - bytesRead );
				int currentBytesRead = this.ReadFromCurrentSector( buffer, offset + bytesRead, bytesToRead - bytesRead );

				if ( currentBytesRead == 0 )
					break;

				bytesRead += currentBytesRead;
			}

			Debug.Assert( bytesRead == bytesToRead );
			return bytesRead;
		}

		#endregion Read

		#region Seek

		public override long Seek( long offset, SeekOrigin origin )
		{
			switch ( origin )
			{
				case SeekOrigin.Begin:
					Debug.Assert( offset >= 0 );
					this.Position = offset;
					break;

				case SeekOrigin.Current:
					Debug.Assert( this.position + offset >= 0 );
					this.Position = this.position + offset;
					break;

				case SeekOrigin.End:
					Debug.Assert( this.length + offset >= 0 );
					this.Position = this.length + offset;
					break;

				default:
					Utilities.DebugFail( "Unknown seek origin" );
					break;
			}

			return this.position;
		}

		#endregion Seek

		#region SetLength

		public override void SetLength( long value )
		{
			if ( value == this.length )
				return;

			if ( value < this.length )
			{
				Utilities.DebugFail( "This stream cannot be shortened yet." );
				return;
			}

			if ( this.firstSectorId < 0 )
				this.SetFirstSectorId( this.AllocateNewSector( -1 ) );

			int numberOfSectorsNeeded = (int)( ( ( value - 1 ) / this.sectorSize ) + 1 );

			int sectorIndex = this.currentSectorIndex;
			int sectorId = this.currentSectorId;

			if ( sectorId < 0 )
			{
				sectorIndex = 0;
				sectorId = this.firstSectorId;
			}

			while ( true )
			{
				if ( numberOfSectorsNeeded <= sectorIndex + 1 )
					break;

				int steps = 1;
				int nextSectorId = this.GetNextSectorId( sectorId, ref steps );

				if ( nextSectorId < 0 )
					break;

				sectorId = nextSectorId;
				sectorIndex++;
			}

			while ( sectorIndex < numberOfSectorsNeeded - 1 )
			{
				sectorId = this.AllocateNewSector( sectorId );
				sectorIndex++;
			}

			this.SetLengthInternal( (int)value );
		}

		#endregion SetLength

		#region Write

		public override void Write( byte[] buffer, int offset, int count )
		{
			#region Do safety checks

			if ( buffer == null )
			{
				Utilities.DebugFail( "The buffer cannot be null." );
				return;
			}

			if ( offset < 0 )
			{
				Utilities.DebugFail( "The offset cannot be less than zero." );
				return;
			}

			if ( count < 0 )
			{
				Utilities.DebugFail( "The count cannot be less than zero." );
				return;
			}

			if ( offset + count > buffer.Length )
			{
				Utilities.DebugFail( "Cannot write past the end of the buffer." );
				return;
			}

			#endregion Do safety checks

			int bytesWritten = 0;

			// If writing the new bytes will cause the stream to go past its length, set the new length on the stream
			long newPosition = this.position + count;
			if ( newPosition > this.length )
				this.SetLength( newPosition );

			while ( bytesWritten < count )
				bytesWritten += this.WriteToCurrentSector( buffer, offset + bytesWritten, count - bytesWritten );

			Debug.Assert( bytesWritten == count );
		}

		#endregion Write

		#endregion Base Class Overrides

		#region Methods

		#region AllocateNewSector

		protected virtual int AllocateNewSector( int lastSectorId )
		{
			return this.storageManager.AllocateNewSector( lastSectorId, this.isShortStream, this.Type );
		}

		#endregion AllocateNewSector

		#region GetNextSectorId

		internal virtual int GetNextSectorId( int lastSectorId, ref int steps )
		{
			return this.storageManager.GetNextSectorId( lastSectorId, ref steps, this.isShortStream );
		}

		#endregion GetNextSectorId

		#region ReadBytes

		public byte[] ReadBytes( int count )
		{
			byte[] data = new byte[ count ];
			int bytesRead = this.Read( data, 0, data.Length );

			// If we were able to read the requested amount of blocks, return the temp array
			if ( bytesRead == count )
				return data;

			// If no bytes were read, return an empty array
			if ( bytesRead == 0 )
				return new byte[ 0 ];

			byte[] retValue = new byte[ bytesRead ];
			Buffer.BlockCopy( data, 0, retValue, 0, bytesRead );

			return retValue;
		}

		#endregion ReadBytes

		#region ReadFromCurrentSector

		private int ReadFromCurrentSector( byte[] buffer, int offset, int count )
		{
			this.SyncContainerStreamPosition();

			// If we have reached the end of the current sector, we must update the reader's position
			if ( this.positionInCurrentSector == this.sectorSize )
			{
				int steps = 1;
				this.currentSectorId = this.GetNextSectorId( this.currentSectorId, ref steps );
				this.currentSectorIndex++;
				this.positionInCurrentSector = 0;

				if ( this.currentSectorId >= 0 )
				{
					this.cachedContainerStreamPosition = this.storageManager.SeekToSector( this.currentSectorId, this.isShortStream );
				}
				else
				{
					this.currentSectorIndex = -1;
					this.cachedContainerStreamPosition = -1;

					// MD 10/5/07
					// Found while fixing BR26292
					// If we got to the end of the stream because of an error, return 0 so we don't 
					// go to the code below and read invalid data.
					return 0;
				}
			}

			// Read the maximum amount of bytes we can from the current sector, up to the count
			int bytesToRead = Math.Min( count, this.sectorSize - this.positionInCurrentSector );
			int bytesRead = this.containerStream.Read( buffer, offset, bytesToRead );
			Debug.Assert( bytesRead == bytesToRead );

			// Advance the position in the current sector
			this.position += bytesRead;
			this.positionInCurrentSector += bytesRead;
			this.cachedContainerStreamPosition += bytesRead;
			Debug.Assert( this.positionInCurrentSector <= this.sectorSize );

			return bytesRead;
		}

		#endregion ReadFromCurrentSector

		#region ReadInt16

		public short ReadInt16()
		{
			if ( this.Read( this.readBuffer, 0, 2 ) != 2 )
			{
				Utilities.DebugFail( "The end of the stream has been reached." );
				return -1;
			}

			return BitConverter.ToInt16( this.readBuffer, 0 );
		}

		#endregion ReadInt32

		#region ReadInt32

		public int ReadInt32()
		{
			if ( this.Read( this.readBuffer, 0, 4 ) != 4 )
			{
				Utilities.DebugFail( "The end of the stream has been reached." );
				return -1;
			}

			return BitConverter.ToInt32( this.readBuffer, 0 );
		}

		#endregion ReadInt32

		#region ReadUInt16

		public ushort ReadUInt16()
		{
			if ( this.Read( this.readBuffer, 0, 2 ) != 2 )
			{
				Utilities.DebugFail( "The end of the stream has been reached." );
				return 0;
			}

			return BitConverter.ToUInt16( this.readBuffer, 0 );
		}

		#endregion ReadUInt16

		#region ResetCache

		protected void ResetCache()
		{
			this.currentSectorId = -1;
			this.currentSectorIndex = -1;
			this.positionInCurrentSector = -1;
			this.cachedContainerStreamPosition = -1;

			this.sectorSize = this.storageManager.GetSectorSize( this.isShortStream );
			this.containerStream = this.storageManager.GetStreamContainerStream( this.isShortStream );
		}

		#endregion ResetCache

		#region SetFirstSectorId

		protected virtual void SetFirstSectorId( int firstSectorId )
		{
			Debug.Assert( firstSectorId >= 0 );
			this.firstSectorId = firstSectorId;
		}

		#endregion SetFirstSectorId

		#region SetLengthInternal

		internal virtual void SetLengthInternal( int value )
		{
			this.length = value;
		}

		#endregion SetLengthInternal

		#region SyncContainerStreamPosition

		private void SyncContainerStreamPosition()
		{
			// If the underlying stream's position hasn't changed since the last time we synced, don't do anything
			if ( this.cachedContainerStreamPosition == this.containerStream.Position )
				return;

			this.SyncPositionMembers();

			// Cahce the new position in the container stream
			this.cachedContainerStreamPosition =
				this.storageManager.SeekToSector( this.currentSectorId, this.positionInCurrentSector, this.isShortStream );
		}

		#endregion SyncContainerStreamPosition

		#region SyncPositionMembers

		private void SyncPositionMembers()
		{
			if ( this.currentSectorId < 0 || 
				this.currentSectorIndex < 0 )
			{
				this.currentSectorId = this.firstSectorId;
				this.currentSectorIndex = 0;

				Debug.Assert( this.currentSectorId >= 0 );
			}

			this.positionInCurrentSector = (int)( this.position % this.sectorSize );
			int neededSectorIndex = (int)( this.position / this.sectorSize );

			int steps = neededSectorIndex - this.currentSectorIndex;
			this.currentSectorId = this.GetNextSectorId( this.currentSectorId, ref steps );
			this.currentSectorIndex = neededSectorIndex;

			if ( this.currentSectorId < 0 )
			{
				this.currentSectorIndex = -1;
				this.currentSectorId = -1;
				this.positionInCurrentSector = -1;
				this.cachedContainerStreamPosition = -1;
				return;
			}
		}

		#endregion SyncPositionMembers

		#region WriteInt16

		public void WriteInt16( short value )
		{
			this.Write( BitConverter.GetBytes( value ), 0, 2 );
		}

		#endregion WriteInt16

		#region WriteInt32

		public void WriteInt32( int value )
		{
			this.Write( BitConverter.GetBytes( value ), 0, 4 );
		}

		#endregion WriteInt32

		#region WriteToCurrentSector

		private int WriteToCurrentSector( byte[] buffer, int offset, int count )
		{
			this.SyncContainerStreamPosition();

			// If we have reached the end of the current sector, advance to the next sector
			if ( this.positionInCurrentSector == this.sectorSize )
			{
				int steps = 1;
				int nextSectorId = this.GetNextSectorId( this.currentSectorId, ref steps );

				// If the next sector is the end of the stream, allocate a new sector for the stream
				if ( nextSectorId < 0 )
				{
					Utilities.DebugFail( "We should have had enough sectors to write to here" );
					this.currentSectorId = this.AllocateNewSector( this.currentSectorId );
				}
				else
				{
					this.currentSectorId = nextSectorId;
				}

				Debug.Assert( this.currentSectorId >= 0 );

				this.currentSectorIndex++;
				this.positionInCurrentSector = 0;

				// Advance the base stream to the start of the next sector
				this.cachedContainerStreamPosition =
					this.storageManager.SeekToSector( this.currentSectorId, this.isShortStream );
			}

			// Write the maximum amount of bytes we can to the current sector, up to the count
			int bytesToWrite = Math.Min( count, this.sectorSize - this.positionInCurrentSector );
			this.containerStream.Write( buffer, offset, bytesToWrite );
			int bytesWritten = (int)( this.containerStream.Position - this.cachedContainerStreamPosition );
			Debug.Assert( bytesWritten == bytesToWrite );

			if ( bytesWritten > 0 )
				this.storageManager.IsDirty = true;

			// Advance the position in the current sector
			this.position += bytesWritten;
			this.positionInCurrentSector += bytesWritten;
			this.cachedContainerStreamPosition += bytesWritten;
			Debug.Assert( this.positionInCurrentSector <= this.sectorSize );

			return bytesWritten;
		}

		#endregion WriteToCurrentSector

		#endregion Methods

		#region Properties

		#region FirstSectorId

		internal int FirstSectorId
		{
			get { return this.firstSectorId; }
		}

		#endregion FirstSectorId

		#region StorageManager

		public StructuredStorageManager StorageManager
		{
			get { return this.storageManager; }
		}

		#endregion StorageManager

		#region Type

		protected virtual StreamType Type
		{
			get { return StreamType.Normal; }
		}

		#endregion Type

		#endregion Properties


		#region StreamType enum

		public enum StreamType
		{
			Normal,
			SAT,
			MasterSAT,
		}

		#endregion StreamType enum
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