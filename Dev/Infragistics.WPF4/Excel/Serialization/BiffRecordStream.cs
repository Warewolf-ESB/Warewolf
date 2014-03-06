using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Infragistics.Documents.Excel.FormulaUtilities;

namespace Infragistics.Documents.Excel.Serialization
{
	// MD 4/28/11 - TFS62775
	// Added a way for BiffRecordStream instances to have sub-streams of other types.
	internal interface IBiffRecordStream
	{
		BiffRecordStream ParentStream { get; set; }
		long EndUnderlyingStreamPosition { get; }
	}

	internal abstract class BiffRecordStream : Stream
		, IBiffRecordStream		// MD 4/28/11 - TFS62775
	{
		#region Constants

		// MD 6/14/07 - BR23880
		// For some reason, TXO records cannot go over 18 bytes in length
		// MD 7/20/2007 - BR25039
		// The previous assumption was incorrect, the first block is always 18 bytes in a TXO record, 
		// and the remaining CONTINUE blocks are of variable length
		//private const int MaxBlockSizeForTXORecord = 18;

		// MD 1/9/09 - TFS12270
		// This has been moved to the Workbook and renamed to InvariantCompressedTextEncoding.
		//// MD 4/9/08 - BR31743
		//// The compressed text encoding remains constant, probably so a workbook saved on one machine 
		//// and opened on another is guaranteed to look the same on both machines.
        //private readonly static Encoding CompressedTextEncoding = Utilities.EncodingGetEncoding( 1252 );

		#endregion Constants

		#region Member Variables

		private WorkbookSerializationManager manager;

		private bool isOpen;

		private long cachedWorkbookStreamPosition;
		private Stream ownerStream;
		private BinaryReader ownerStreamReader;

		private int length;
		private int position;

		private List<RecordBlockInfo> blocks = new List<RecordBlockInfo>();
		private int currentBlock;
		private bool currentBlockWasModified;
		private int positionInCurrentBlock;

		private int recordType;
		private int nextBlockType;

		private BiffRecordStream parentStream;

		// MD 4/28/11 - TFS62775
		// Added a way for BiffRecordStream instances to have sub-streams of other types.
		//private List<BiffRecordStream> subStreams;
		private List<IBiffRecordStream> subStreams;

		private byte[] readBuffer;

		// MD 4/18/11 - TFS62026
		// As a performance improvement, we will lazily write the first block header.
		private bool isFirstBlockHeaderWritten;

		#endregion Member Variables

		#region Constructor

		protected BiffRecordStream() 
		{
			this.readBuffer = new byte[ 8 ];
		}

		protected BiffRecordStream( WorkbookSerializationManager manager, Stream ownerStream, BinaryReader ownerStreamReader )
			: this()
		{
			// MD 4/18/11 - TFS62026
			// For records being read in, the first block header is always written.
			this.isFirstBlockHeaderWritten = true;

			this.isOpen = true;
			this.manager = manager;
			this.ownerStream = ownerStream;
			this.ownerStreamReader = ownerStreamReader;

			this.recordType = this.ReadBlockType( ownerStreamReader );

			// MD 5/22/07 - BR23135
			// If the record type is invalid, return, but add a block so we can dispose the record correctly
			if ( this.recordType == this.DefaultRecordId )
			{
				// MD 6/14/07 - BR23880
				// Use the factory method to create new record blocks now
				//this.blocks.Add( new RecordBlockInfo( this.workbookStream.Position, 0 ) );
				this.blocks.Add( this.CreateRecordBlock( this.ownerStream.Position, 0 ) );
				return;
			}

			int firstBlockLength = this.ReadBlockLength( this.ownerStreamReader );

			this.cachedWorkbookStreamPosition = this.ownerStream.Position;

			this.length = firstBlockLength;

			// MD 6/14/07 - BR23880
			// Use the factory method to create new record blocks now
			//this.blocks.Add( new RecordBlockInfo( this.workbookStream.Position, firstBlockLength ) );
			this.blocks.Add( this.CreateRecordBlock( this.ownerStream.Position, firstBlockLength ) );

			// Advance the stream to the next record
			this.ownerStream.Position = this.cachedWorkbookStreamPosition + this.length;

			this.AppendContinueBlocks();

			this.ownerStream.Position = this.cachedWorkbookStreamPosition;
		}

		protected BiffRecordStream( WorkbookSerializationManager manager, Stream ownerStream, int recordType )
			: this()
		{
			this.isOpen = true;
			this.manager = manager;
			this.ownerStream = ownerStream;
			this.recordType = recordType;

			// MD 4/18/11 - TFS62026
			// As a performance improvement, we will lazily write the first block header.
			// Instead, just add the first block to the internal blocks collection.
			//this.WriteNewBlock( this.recordType );
			this.blocks.Add(this.CreateRecordBlock(this.ownerStream.Position + this.BlockLengthSize + this.BlockTypeSize, 0));

			// MD 4/18/11 - TFS62026
			// We should cache the current workbook position so the first write doesn't have to sync position members.
			this.cachedWorkbookStreamPosition = this.ownerStream.Position;
		}

		#endregion Constructor

		#region Interfaces

		// MD 4/28/11 - TFS62775
		#region IBiffRecordStream Members

		BiffRecordStream IBiffRecordStream.ParentStream
		{
			get { return this.parentStream; }
			set { this.parentStream = value; }
		}

		#endregion

		#endregion  // Interfaces

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

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			try
			{
				if ( disposing )
				{
					// Close the current block if it was written to
					this.CloseCurrentBlock();

					// Make sure the stream is at the end of the record so the next one can be read in
					this.ownerStream.Position = this.EndUnderlyingStreamPosition;

					if ( this.parentStream != null )
						this.parentStream.isOpen = true;
				}

				this.isOpen = false;
			}
			finally
			{
				base.Dispose( disposing );
			}
		}

		#endregion Dispose

		#region Length

		public override long Length
		{
			get { return this.length; }
		}

		#endregion Length

		#region Flush

		public override void Flush() { }

		#endregion Flush

		#region Position

		public override long Position
		{
			get
			{
				Debug.Assert( this.isOpen, "The record stream is not open!" );
				return this.position;
			}
			set
			{
				if ( this.isOpen == false )
				{
					Utilities.DebugFail( "The record stream is not open!" );
					return;
				}

				if ( this.position == value )
					return;

				if ( value < 0 )
				{
					Utilities.DebugFail( "The stream position cannot be less than zero." );
					return;
				}

				int difference = (int)( value - this.position );
				int newPositionInCurrentBlock = this.positionInCurrentBlock + difference;

				this.position = (int)value;

				if ( this.cachedWorkbookStreamPosition == this.ownerStream.Position &&
					0 <= newPositionInCurrentBlock &&
					newPositionInCurrentBlock < this.blocks[ this.currentBlock ].BlockLength )
				{
					this.positionInCurrentBlock = newPositionInCurrentBlock;
					this.cachedWorkbookStreamPosition += difference;
					this.ownerStream.Position = this.cachedWorkbookStreamPosition;
				}
				else
				{
					this.SyncPositionMembers();
				}
			}
		}

		#endregion Position

		#region Read

		public override int Read( byte[] buffer, int offset, int count )
		{
			if ( this.isOpen == false )
			{
				Utilities.DebugFail( "The record stream is not open!" );
				return 0;
			}

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

			// The data might extend into subsequent CONTINUE records, so keep readng from 
			// the current block until the total amount of bytes have been read
			while ( bytesRead < bytesToRead )
				bytesRead += this.ReadFromCurrentBlock( buffer, offset + bytesRead, bytesToRead - bytesRead );

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
					this.Position += offset;
					break;

				case SeekOrigin.End:
					Debug.Assert( this.length + offset >= 0 );
					this.Position = this.Length + offset;
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
				Utilities.DebugFail( "This stream cannot be shortened." );
				return;
			}

			long difference = value - this.length;
			this.length = (int)value;

			bool workbookPositionChanged = false;
			long oldPosition = this.ownerStream.Position;

			while ( true )
			{
				int lastBlockIndex = this.blocks.Count - 1;

				RecordBlockInfo currentBlockInfo = this.blocks[ lastBlockIndex ];

				int lastBlockLength = currentBlockInfo.BlockLength;

				if ( lastBlockLength + difference <= currentBlockInfo.MaximumBlockLength )
				{
					currentBlockInfo.BlockLength = (ushort)( lastBlockLength + difference );
					break;
				}
				else
				{
					int distanceToEndOfBlock = currentBlockInfo.MaximumBlockLength - currentBlockInfo.BlockLength;
					difference -= distanceToEndOfBlock;

					currentBlockInfo.BlockLength = currentBlockInfo.MaximumBlockLength;

					this.ownerStream.Position += distanceToEndOfBlock;
					this.WriteNewBlock( this.GetNextBlockType() );

					workbookPositionChanged = true;
				}
			}

			if ( workbookPositionChanged )
				this.ownerStream.Position = oldPosition;
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

			// MD 4/18/11 - TFS62026
			// If the first block header wasn't written yet, we should write it here.
			// Also, if we do end up writing the first block header, don't set the currentBlockWasModified flag to True
			// because we are going to write the correct block length as well assuming this is the only data being written.
			// If that is incorrect and more data will be written later, then the next write will set the flag to True and the
			// length will be updated when the block is closed.
			bool shouldSetModifiedFlag = true;
			if (this.isFirstBlockHeaderWritten == false)
			{
				this.WriteFirstBlockHeader(Math.Min(count, this.blocks[0].MaximumBlockLength));
				shouldSetModifiedFlag = false;
			}

			// If writing the new bytes will cause the stream to go past its length, set the new length on the stream
			long newPosition = this.position + count;

			if ( newPosition > this.Length )
				this.SetLength( newPosition );

			while ( bytesWritten < count )
				bytesWritten += this.WriteToCurrentBlock( buffer, offset + bytesWritten, count - bytesWritten );

			// MD 4/18/11 - TFS62026
			// If we weren't supposed to set the currentBlockWasModified flag, reset it to False.
			if (shouldSetModifiedFlag == false)
				this.currentBlockWasModified = false;

			Debug.Assert( bytesWritten == count );
		}

		#endregion Write

		#endregion Base Class Overrides

		#region Methods

		#region Abstract Methods

		protected abstract BiffRecordStream CopyForAlternateStream( Stream newWorkbookStream, long startPositionInNewStream );
		protected abstract int GetDefaultContinuationBlockType();
		protected abstract int ReadBlockLength( BinaryReader ownerStreamReader );
		protected abstract int ReadBlockType( BinaryReader ownerStreamReader );
		protected abstract void WriteBlockLength( Stream ownerStream, int length, bool isAfterLengthPosition );
		protected abstract void WriteBlockType( Stream ownerStream, int type ); 

		#endregion Abstract Methods	

		#region Public Methods

		#region AddSubRecord

		// MD 4/28/11 - TFS62775
		// Added a way for BiffRecordStream instances to have sub-streams of other types.
		//public void AddSubStream( BiffRecordStream record )
		public void AddSubStream(IBiffRecordStream record)
		{
			if ( this.subStreams == null )
			{
				// MD 4/28/11 - TFS62775
				// Added a way for BiffRecordStream instances to have sub-streams of other types.
				//this.subStreams = new List<BiffRecordStream>();
				this.subStreams = new List<IBiffRecordStream>();
			}

			this.subStreams.Add( record );

			// MD 4/28/11 - TFS62775
			//Debug.Assert( record.parentStream == null );
			//record.parentStream = this;
			Debug.Assert(record.ParentStream == null);
			record.ParentStream = this;

			// Close this stream until the sub stream is closed
			this.isOpen = false;

			this.blocks[ this.currentBlock ].CapBlock();
		}

		#endregion AddSubRecord

		#region AppendNextRecordIfType

		public bool AppendNextRecordIfType( int type )
		{
			if ( this.AppendNextBlockIfType( type ) == false )
				return false;

			// Advance the stream to the next record
			this.ownerStream.Seek( this.blocks[ this.blocks.Count - 1 ].BlockLength, SeekOrigin.Current );

			this.AppendContinueBlocks();
			return true;
		}

		#endregion AppendNextRecordIfType

		#region CapCurrentBlock

		public void CapCurrentBlock()
		{
			this.SyncWorkbookStreamPosition();
			this.blocks[ this.currentBlock ].CapBlock();
		}

		#endregion CapCurrentBlock

		#region DumpContents



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion DumpContents

		#region GetDataSize

		public int GetDataSize( long startPosition, LengthType lengthType, string value )
		{
			// MD 8/2/07
			// We need to cache the position to resture it later
			long originalPosition = this.Position;

			// MD 8/2/07
			// Put the code in a try...finally so the position is always restored
			try
			{
				// MD 8/2/07
				// The position of the main stream must be at the start position or the copy stream will be 
				// misaligned when the position is set
				this.Position = startPosition;

				Debug.Assert( startPosition != 0 ); // This would never be 0 for formula tokens

				using ( MemoryStream ms = new MemoryStream() )
				{
					BiffRecordStream copyStream = this.CopyForAlternateStream( ms, -startPosition );

					// MD 5/29/07 - BR23353
					// Make sure the length of the copy stream is long enough to set the correct position
					if ( copyStream.length < startPosition )
						copyStream.SetLength( startPosition );

					copyStream.Position = startPosition;

					copyStream.Write( value, lengthType );

					return (int)ms.Position;
				}
			}
			finally
			{
				// MD 8/2/07
				// Restore the old position of the main stream
				this.Position = originalPosition;
			}
		}

		#endregion GetDataSize

		// MD 4/18/11 - TFS62026
		#region GetStartOfRecord

		public long GetStartOfRecord()
		{
			return this.blocks[0].BlockStart - this.BlockLengthSize - this.BlockTypeSize;
		} 

		#endregion // GetStartOfRecord

		// MD 9/23/09 - TFS19150
		#region ReadByteFromBuffer

		public byte ReadByteFromBuffer( ref byte[] data, ref int dataIndex )
		{
			if ( dataIndex == data.Length )
				this.CacheNextBlock( ref data, ref dataIndex );

			return data[ dataIndex++ ];
		}

		#endregion ReadByteFromBuffer

		#region ReadBytes

		public byte[] ReadBytes( int count )
		{
			if ( count < 0 )
			{
				Utilities.DebugFail( "The count cannot be less than zero." );
				return new byte[ 0 ];
			}

			// Create a temp array to hold the requested aqmount of bytes
			byte[] data = new byte[ count ];

			// Try to read the requested amount of blocks
			int bytesRead = this.Read( data, 0, count );

			// If we were able to read the requested amount of blocks, return the temp array
			if ( bytesRead == count )
				return data;

			// If no bytes were read, return an empty array
			if ( bytesRead == 0 )
				return new byte[ 0 ];

			// Otherwise, create the array to return with the correct size, and copy all bytes
			// from the temp array to the return array.
			byte[] retValue = new byte[ bytesRead ];
			Buffer.BlockCopy( data, 0, retValue, 0, bytesRead );

			return retValue;
		}

		#endregion ReadBytes

		// MD 9/23/09 - TFS19150
		#region ReadBytesFromBuffer

		public byte[] ReadBytesFromBuffer( int count, ref byte[] data, ref int dataIndex )
		{
			if ( count < 0 )
			{
				Utilities.DebugFail( "The count cannot be less than zero." );
				return new byte[ 0 ];
			}

			// Create a temp array to hold the requested aqmount of bytes
			byte[] retData = new byte[ count ];
			int retDataLength = 0;

			while ( true )
			{
				int bytesFromCurrentBuffer = Math.Min( count - retDataLength, data.Length - dataIndex );

				Buffer.BlockCopy( data, dataIndex, retData, retDataLength, bytesFromCurrentBuffer );
				dataIndex += bytesFromCurrentBuffer;
				retDataLength += bytesFromCurrentBuffer;

				// If we were able to read the requested amount of blocks, return the temp array
				if ( retDataLength == count )
					return retData;

				if ( this.Position == this.Length )
				{
					byte[] smallerData = new byte[ retDataLength ];
					Buffer.BlockCopy( retData, 0, smallerData, 0, retDataLength );
					return smallerData;
				}

				this.CacheNextBlock( ref data, ref dataIndex );
			}
		}

		#endregion ReadBytesFromBuffer

		#region ReadDouble

		public double ReadDouble()
		{
			if ( this.Read( this.readBuffer, 0, 8 ) != 8 )
				throw new EndOfStreamException();

			return BitConverter.ToDouble( this.readBuffer, 0 );
		}

		#endregion ReadDouble

		// MD 9/23/09 - TFS19150
		#region ReadDoubleFromBuffer

		public double ReadDoubleFromBuffer( ref byte[] data, ref int dataIndex )
		{
			if ( dataIndex + 8 > data.Length )
				this.CacheNextBlock( ref data, ref dataIndex );

			double retVal = BitConverter.ToDouble( data, dataIndex );
			dataIndex += 8;
			return retVal;
		}

		#endregion ReadDoubleFromBuffer

		#region ReadFormattedString

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public FormattedString ReadFormattedString( LengthType lengthType )
		public StringElement ReadFormattedString(LengthType lengthType)
		{
			// Make sure the length tyep entered is correct
			if ( lengthType != LengthType.EightBit && lengthType != LengthType.SixteenBit )
			{
				Utilities.DebugFail( "Invalid length type." );
				return null;
			}

			// Determing the length of the string
			ushort stringLength = lengthType == LengthType.EightBit
				? (byte)this.ReadByte()
				: this.ReadUInt16();

			return this.ReadFormattedString( stringLength );
		}

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public FormattedString ReadFormattedString( ushort stringLength )
		public StringElement ReadFormattedString(ushort stringLength)
		{
			// MD 9/25/08 - TFS8010
			// Moved all code to the new TryReadFormattedString method
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString value;
			StringElement value;

			// If the value can'r be read, we reached the end of the stream, so throw an exception because this method expects the string to be read successfully.
			if ( this.TryReadFormattedString( stringLength, out value ) == false )
				throw new EndOfStreamException();

			return value;
		}

		// MD 9/25/08 - TFS8010
		// Moved all code from ReadFormattedString to this new method so we can see if we can read the string without an exception being thrown.
		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public bool TryReadFormattedString( ushort stringLength, out FormattedString value )
		public bool TryReadFormattedString(ushort stringLength, out StringElement value)
		{
			value = null;

			FormattedStringInfo info = this.ReadFormattedStringHelper( stringLength );

			// MD 9/25/08 - TFS8010
			// The info returned can now be null, so return null if it is.
			if ( info == null )
				return false;

			// Only read the formatting runs and asian phonetic setting if this is not a 
			// continuation section of the string.
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//value = new FormattedString( info.UnformattedText );
			// MD 1/31/12 - TFS100573
			// We only need to create a FormattedStringElement if there is formatting. Otherwise, create a StringElement.
			//value = new FormattedStringElement(this.Manager.Workbook, info.UnformattedText);
			//
			//if ( info.HasRichText )
			//    this.ReadFormattingRuns( value, info.RichTextFormattingRuns );
			if ( info.HasRichText )
			{
				FormattedStringElement formattedStringElement = new FormattedStringElement(info.UnformattedText);
				this.ReadFormattingRuns(formattedStringElement, info.RichTextFormattingRuns);
				value = formattedStringElement;
			}
			else
			{
				value = new StringElement(info.UnformattedText);
			}

			// MD 7/27/10 - TFS36066
			// Just pass in the block size and the method will read the correct amount of bytes.
			//if ( info.HasAsianPhoneticSettings )
			//    this.ReadAsianPhoneticSettingsBlock();
			this.ReadAsianPhoneticSettingsBlock(info.AsianPhoneticSettingsSize);

			return true;
		}

		#endregion ReadFormattedString

		// MD 9/23/09 - TFS19150
		#region ReadFormattedStringBlock

		// MD 11/3/10 - TFS49093
		// Instead of storing position info on a holder for each string where 1/7th of them will be unused
		// we are now storing it in a collection on the manager.
		//public void ReadFormattedStringBlock( uint stringCount, List<WorkbookSerializationManager.FormattedStringHolder> formattedStrings )
		public void ReadFormattedStringBlock(uint stringCount, List<StringElement> formattedStrings)
		{
			if ( stringCount == 0 )
				return;

			byte[] data = new byte[ 0 ];
			int dataIndex = 0;
			this.CacheNextBlock( ref data, ref dataIndex );

			for ( int i = 0; i < stringCount; i++ )
			{
				// MD 11/3/10 - TFS49093
				// Instead of storing position info on a holder for each string where 1/7th of them will be unused
				// we are now storing it in a collection on the manager.
				//WorkbookSerializationManager.FormattedStringHolder holder = new WorkbookSerializationManager.FormattedStringHolder(
				//    this.ReadFormattedStringFromBuffer( LengthType.SixteenBit, ref data, ref dataIndex ),
				//    this.PositionInCurrentBlock,
				//    this.ownerStream.Position );
				//
				//formattedStrings.Add( holder );
				formattedStrings.Add(this.ReadFormattedStringFromBuffer(LengthType.SixteenBit, ref data, ref dataIndex));
			}
		}

		#endregion ReadFormattedStringBlock

		// MD 9/23/09 - TFS19150
		#region ReadFormattedStringFromBuffer

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public FormattedString ReadFormattedStringFromBuffer( LengthType lengthType, ref byte[] data, ref int dataIndex )
		public StringElement ReadFormattedStringFromBuffer(LengthType lengthType, ref byte[] data, ref int dataIndex)
		{
			ushort stringLength = lengthType == LengthType.EightBit
				? this.ReadByteFromBuffer( ref data, ref dataIndex )
				: this.ReadUInt16FromBuffer( ref data, ref dataIndex );

			return this.ReadFormattedStringFromBuffer( stringLength, ref data, ref dataIndex );
		}

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public FormattedString ReadFormattedStringFromBuffer( ushort stringLength, ref byte[] data, ref int dataIndex )
		public StringElement ReadFormattedStringFromBuffer(ushort stringLength, ref byte[] data, ref int dataIndex)
		{
			FormattedStringInfo info = this.ReadFormattedStringFromBufferHelper( stringLength, ref data, ref dataIndex );

			// Only read the formatting runs and asian phonetic setting if this is not a 
			// continuation section of the string.
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString retValue = new FormattedString( info.UnformattedText );
			// MD 1/31/12 - TFS100573
			// We need to create a derived element type if there is formatting, so don't create this yet.
			//FormattedStringElement retValue = new FormattedStringElement(this.Manager.Workbook, info.UnformattedText);
			//if ( info.HasRichText )
			//    this.ReadFormattingRunsFromBuffer( retValue, info.RichTextFormattingRuns, ref data, ref dataIndex );
			StringElement retValue;
			if (info.HasRichText)
			{
				FormattedStringElement formattedStringElement = new FormattedStringElement(info.UnformattedText);
				this.ReadFormattingRunsFromBuffer(formattedStringElement, info.RichTextFormattingRuns, ref data, ref dataIndex);
				retValue = formattedStringElement;
			}
			else
			{
				retValue = new StringElement(info.UnformattedText);
			}

			// MD 7/27/10 - TFS36066
			// Just pass in the block size and the method will read the correct amount of bytes.
			//if ( info.HasAsianPhoneticSettings )
			//    this.ReadAsianPhoneticSettingsBlockFromBuffer( ref data, ref dataIndex );
			this.ReadAsianPhoneticSettingsBlockFromBuffer(ref data, ref dataIndex, info.AsianPhoneticSettingsSize);

			return retValue;
		}

		#endregion ReadFormattedStringFromBuffer

		#region ReadFormulaCellAddress

		public CellAddress ReadFormulaCellAddress()
		{
			ushort encodedRow = this.ReadUInt16();
			ushort encodedColumn = this.ReadUInt16();

			return BiffRecordStream.CellAddressFromEncodedValues( encodedRow, encodedColumn );
		}

		#endregion ReadFormulaCellAddress

		// MD 9/23/09 - TFS19150
		#region ReadFormulaCellAddressFromBuffer

		public CellAddress ReadFormulaCellAddressFromBuffer( ref byte[] data, ref int dataIndex )
		{
			ushort encodedRow = this.ReadUInt16FromBuffer( ref  data, ref dataIndex );
			ushort encodedColumn = this.ReadUInt16FromBuffer( ref  data, ref dataIndex );

			return BiffRecordStream.CellAddressFromEncodedValues( encodedRow, encodedColumn );
		}

		#endregion ReadFormulaCellAddressFromBuffer

		#region ReadFormulaCellAddressRange

		public CellAddressRange ReadFormulaCellAddressRange()
		{
			ushort encodedRowFirst = this.ReadUInt16();
			ushort encodedRowLast = this.ReadUInt16();
			ushort encodedColumnFirst = this.ReadUInt16();
			ushort encodedColumnLast = this.ReadUInt16();

			CellAddress first = BiffRecordStream.CellAddressFromEncodedValues( encodedRowFirst, encodedColumnFirst );
			CellAddress last = BiffRecordStream.CellAddressFromEncodedValues( encodedRowLast, encodedColumnLast );

			return new CellAddressRange( first, last );
		}

		#endregion ReadFormulaCellAddressRange

		// MD 9/23/09 - TFS19150
		#region ReadFormulaCellAddressRangeFromBuffer

		public CellAddressRange ReadFormulaCellAddressRangeFromBuffer( ref byte[] data, ref int dataIndex )
		{
			ushort encodedRowFirst = this.ReadUInt16FromBuffer( ref data, ref dataIndex );
			ushort encodedRowLast = this.ReadUInt16FromBuffer( ref data, ref dataIndex );
			ushort encodedColumnFirst = this.ReadUInt16FromBuffer( ref  data, ref dataIndex );
			ushort encodedColumnLast = this.ReadUInt16FromBuffer( ref data, ref dataIndex );

			CellAddress first = BiffRecordStream.CellAddressFromEncodedValues( encodedRowFirst, encodedColumnFirst );
			CellAddress last = BiffRecordStream.CellAddressFromEncodedValues( encodedRowLast, encodedColumnLast );

			return new CellAddressRange( first, last );
		}

		#endregion ReadFormulaCellAddressRangeFromBuffer

		// MD 9/12/08 - TFS6887
		#region ReadFormulaCellAddressRangeList

		public List<CellAddressRange> ReadFormulaCellAddressRangeList()
		{
			ushort numberOfRanges = this.ReadUInt16();

			List<CellAddressRange> ranges = new List<CellAddressRange>( numberOfRanges );

			for ( int i = 0; i < numberOfRanges; i++ )
				ranges.Add( this.ReadFormulaCellAddressRange() );

			return ranges;
		}

		#endregion ReadFormulaCellAddressRangeList

		// MD 9/23/09 - TFS19150
		#region ReadFormulaCellAddressRangeListFromBuffer

		internal List<CellAddressRange> ReadFormulaCellAddressRangeListFromBuffer( ref byte[] data, ref int dataIndex )
		{
			ushort numberOfRanges = this.ReadUInt16FromBuffer( ref data, ref dataIndex );

			List<CellAddressRange> ranges = new List<CellAddressRange>( numberOfRanges );

			for ( int i = 0; i < numberOfRanges; i++ )
				ranges.Add( this.ReadFormulaCellAddressRangeFromBuffer( ref data, ref dataIndex ) );

			return ranges;
		}

		#endregion ReadFormulaCellAddressRangeListFromBuffer

		#region ReadInt16

		public short ReadInt16()
		{
			if ( this.Read( this.readBuffer, 0, 2 ) != 2 )
				throw new EndOfStreamException();

			return BitConverter.ToInt16( this.readBuffer, 0 );
		}

		#endregion ReadInt16

		#region ReadInt32

		public int ReadInt32()
		{
			if ( this.Read( this.readBuffer, 0, 4 ) != 4 )
				throw new EndOfStreamException();

			return BitConverter.ToInt32( this.readBuffer, 0 );
		}

		#endregion ReadInt32

		// MD 4/12/11 - TFS67084
		#region ReadInt16FromBuffer

		public short ReadInt16FromBuffer(ref byte[] data, ref int dataIndex)
		{
			if (dataIndex + 2 > data.Length)
				this.CacheNextBlock(ref data, ref dataIndex);

			short retVal = BitConverter.ToInt16(data, dataIndex);
			dataIndex += 2;
			return retVal;
		}

		#endregion ReadUInt16FromBuffer

		// MD 9/23/09 - TFS19150
		#region ReadInt32FromBuffer

		public int ReadInt32FromBuffer( ref byte[] buffer, ref int bufferPosition )
		{
			if ( bufferPosition + 4 > buffer.Length )
				this.CacheNextBlock( ref buffer, ref bufferPosition );

			int retVal = BitConverter.ToInt32( buffer, bufferPosition );
			bufferPosition += 4;
			return retVal;
		}

		#endregion ReadInt32FromBuffer

		#region ReadUInt16

		public ushort ReadUInt16()
		{
			if ( this.Read( this.readBuffer, 0, 2 ) != 2 )
				throw new EndOfStreamException();

			return BitConverter.ToUInt16( this.readBuffer, 0 );
		}

		#endregion ReadUInt16

		// MD 9/23/09 - TFS19150
		#region ReadUInt16FromBuffer

		public ushort ReadUInt16FromBuffer( ref byte[] data, ref int dataIndex )
		{
			if ( dataIndex + 2 > data.Length )
				this.CacheNextBlock( ref data, ref dataIndex );

			ushort retVal = BitConverter.ToUInt16( data, dataIndex );
			dataIndex += 2;
			return retVal;
		}

		#endregion ReadUInt16FromBuffer

		#region ReadUInt32

		public uint ReadUInt32()
		{
			if ( this.Read( this.readBuffer, 0, 4 ) != 4 )
				throw new EndOfStreamException();

			return BitConverter.ToUInt32( this.readBuffer, 0 );
		}

		#endregion ReadUInt32

		// MD 9/23/09 - TFS19150
		#region ReadUInt32FromBuffer

		public uint ReadUInt32FromBuffer( ref byte[] buffer, ref int bufferPosition )
		{
			if ( bufferPosition + 4 > buffer.Length )
				this.CacheNextBlock( ref buffer, ref bufferPosition );

			uint retVal = BitConverter.ToUInt32( buffer, bufferPosition );
			bufferPosition += 4;
			return retVal;
		}

		#endregion ReadUInt32FromBuffer

		#region ReadUInt64

		public ulong ReadUInt64()
		{
			if ( this.Read( this.readBuffer, 0, 8 ) != 8 )
				throw new EndOfStreamException();

			return BitConverter.ToUInt64( this.readBuffer, 0 );
		}

		#endregion ReadUInt64

		// MD 9/23/09 - TFS19150
		#region ReadUInt64FromBuffer

		public ulong ReadUInt64FromBuffer( ref byte[] buffer, ref int bufferPosition )
		{
			if ( bufferPosition + 8 > buffer.Length )
				this.CacheNextBlock( ref buffer, ref bufferPosition );

			ulong retVal = BitConverter.ToUInt64( buffer, bufferPosition );
			bufferPosition += 8;
			return retVal;
		}

		#endregion ReadUInt64FromBuffer

		#region SyncWorkbookStreamPosition

		public void SyncWorkbookStreamPosition()
		{
			// If the underlying stream's position hasn't changed since the last time we synced, don't do anything
			if ( this.cachedWorkbookStreamPosition == this.ownerStream.Position )
				return;

			this.SyncPositionMembers();
		}

		#endregion SyncWorkbookStreamPosition

		#region Write( byte )

		public void Write( byte value )
		{
			byte[] buffer = new byte[ 1 ];
			buffer[ 0 ] = value;

			this.Write( buffer );
		}

		#endregion Write( byte )

		#region Write( byte[] )

		public void Write( byte[] buffer )
		{
			if ( buffer == null )
			{
				Utilities.DebugFail( "The buffer cannot be null." );
				return;
			}

			this.Write( buffer, 0, buffer.Length );
		}

		#endregion Write( byte[] )

		#region Write( double )

		public void Write( double value )
		{
			this.Write( BitConverter.GetBytes( value ) );
		}

		#endregion Write( double )

		#region Write( int )

		public void Write( int value )
		{
			this.Write( BitConverter.GetBytes( value ) );
		}

		#endregion Write( int )

		// MD 4/12/11 - TFS67084
		#region Write( short )

		public void Write(short value)
		{
			this.Write(BitConverter.GetBytes(value));
		}

		#endregion Write( short )

		#region Write( ushort )

		public void Write( ushort value )
		{
			this.Write( BitConverter.GetBytes( value ) );
		}

		#endregion Write( ushort )

		#region Write( uint )

		public void Write( uint value )
		{
			this.Write( BitConverter.GetBytes( value ) );
		}

		#endregion Write( uint )

		#region Write( ulong )

		public void Write( ulong value )
		{
			this.Write( BitConverter.GetBytes( value ) );
		}

		#endregion Write( ulong )

		#region Write ( string )

		public void Write( string value )
		{
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//this.Write( new FormattedString( value ) );
			// MD 2/2/12 - TFS100573
			// The string element no longer has a reference to the Workbook.
			//this.Write(new StringElement(this.Manager.Workbook, value));
			this.Write(new StringElement(value));
		}

		public void Write( string value, LengthType lengthType )
		{
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//this.Write( new FormattedString( value ), lengthType );
			// MD 2/2/12 - TFS100573
			// The string element no longer has a reference to the Workbook.
			//this.Write(new StringElement(this.Manager.Workbook, value), lengthType);
			this.Write(new StringElement(value), lengthType);
		}

		public void Write( string value, LengthType lengthType, bool allowCharCompression )
		{
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//this.Write( new FormattedString( value ), lengthType, allowCharCompression );
			// MD 2/2/12 - TFS100573
			// The string element no longer has a reference to the Workbook.
			//this.Write(new StringElement(this.Manager.Workbook, value), lengthType, allowCharCompression);
			this.Write(new StringElement(value), lengthType, allowCharCompression);
		}

		#endregion Write ( string )

		#region Write ( CellAddress )

		public void Write( CellAddress address )
		{
			ushort encodedRow;
			ushort encodedColumn;

			BiffRecordStream.CellAddressToEncodedValues( address, out encodedRow, out encodedColumn );

			this.Write( encodedRow );
			this.Write( encodedColumn );
		}

		#endregion Write ( CellAddress )

		#region Write ( CellAddressRange )

		public void Write( CellAddressRange range )
		{
			ushort encodedRowFirst;
			ushort encodedRowLast;
			ushort encodedColumnFirst;
			ushort encodedColumnLast;

			BiffRecordStream.CellAddressToEncodedValues( range.TopLeftCellAddress, out encodedRowFirst, out encodedColumnFirst );
			BiffRecordStream.CellAddressToEncodedValues( range.BottomRightCellAddress, out encodedRowLast, out encodedColumnLast );

			this.Write( encodedRowFirst );
			this.Write( encodedRowLast );
			this.Write( encodedColumnFirst );
			this.Write( encodedColumnLast );
		}

		#endregion Write ( CellAddressRange )

		// MD 9/12/08 - TFS6887
		#region Write ( List<CellAddressRange> )

		internal void Write( List<CellAddressRange> rangeList )
		{
			this.Write( (ushort)rangeList.Count );

			foreach ( CellAddressRange range in rangeList )
				this.Write( range );
		} 

		#endregion Write ( List<CellAddressRange> )

		#region Write ( StringElement )

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public void Write( FormattedString value )
		public void Write(StringElement value)
		{
			// MD 7/20/2007 - BR25039
			// Call new overload instead, and by default, write the formatting runs
			//this.WriteFormattedStringHelper( value, 0, true, false, LengthType.EightBit, true );
			this.Write( value, true );
		}

		// MD 7/20/2007 - BR25039
		// Added new overload that takes a boolean indicating whether the formatting runs should be ignored
		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public void Write( FormattedString value, bool writeFormattingRuns )
		public void Write(StringElement value, bool writeFormattingRuns)
		{
			this.WriteFormattedStringHelper( value, 0, true, false, LengthType.EightBit, true, writeFormattingRuns );
		}

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public void Write( FormattedString value, LengthType lengthType )
		public void Write(StringElement value, LengthType lengthType)
		{
			this.Write( value, lengthType, true );
		}

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//public void Write( FormattedString value, LengthType lengthType, bool allowCharCompression )
		public void Write(StringElement value, LengthType lengthType, bool allowCharCompression)
		{
			// Make sure the length tyep entered is correct
			if ( lengthType != LengthType.EightBit && lengthType != LengthType.SixteenBit )
			{
				Utilities.DebugFail( "Invalid length type." );
				return;
			}

			// MD 7/20/2007 - BR25039
			// WriteFormattedStringHelper takes another parameter now indicating whether to write the formatting runs
			//this.WriteFormattedStringHelper( value, 0, true, true, lengthType, allowCharCompression );
			this.WriteFormattedStringHelper( value, 0, true, true, lengthType, allowCharCompression, true );
		}

		#endregion Write ( StringElement )

		// MD 4/18/11 - TFS62026
		#region Write ( MemoryStream )

		public void Write(MemoryStream memoryStream)
		{
			short dataSize = (short)memoryStream.Length;
			RecordBlockInfo firstBlock = this.blocks[0];

			// If we already wrote the first block header or there is too much data for the first block, just write the buffer normally.
			// We cannot do any performance workarounds in this case.
			if (this.isFirstBlockHeaderWritten || memoryStream.Length > firstBlock.MaximumBlockLength)
			{
				this.Write(memoryStream.GetBuffer(), 0, dataSize);
				return;
			}

			long headerPosition = this.GetStartOfRecord();

			// Move the stream to the beginning of the record if it is not already there.
			if (this.cachedWorkbookStreamPosition != headerPosition ||
				this.cachedWorkbookStreamPosition != this.ownerStream.Position)
			{
				this.cachedWorkbookStreamPosition = headerPosition;
				this.ownerStream.Position = headerPosition;
			}

			// Create a byte array with the header and memory stream data combined.
			byte[] headerAndData = new byte[4 + dataSize];
			Buffer.BlockCopy(BitConverter.GetBytes((short)this.recordType), 0, headerAndData, 0, 2);
			Buffer.BlockCopy(BitConverter.GetBytes(dataSize), 0, headerAndData, 2, 2);
			Buffer.BlockCopy(memoryStream.GetBuffer(), 0, headerAndData, 4, dataSize);
			this.ownerStream.Write(headerAndData, 0, headerAndData.Length);

			// Set the flag indicating the first block header is now written.
			this.isFirstBlockHeaderWritten = true;

			// Update all relevant members pertaining to the record stream and owner stream.
			this.cachedWorkbookStreamPosition += headerAndData.Length;

			this.length =
				this.position =
				this.positionInCurrentBlock =
				firstBlock.BlockLength = dataSize;
		}

		#endregion  // Write ( MemoryStream )

		// MD 4/28/11 - TFS62775
		#region WriteRawDataAfterRecord






		public void WriteRawDataAfterRecord(byte[] data)
		{
			this.AddSubStream(new RawDataStream(this.ownerStream, data));
		}

		#endregion  // WriteRawDataAfterRecord

		#endregion Public Methods

		#region Protected Methods

		#region AppendContinueBlocks

		protected virtual void AppendContinueBlocks()
		{
			int continueType = this.GetNextBlockType();

			// Look for continuation records of this record
			while ( true )
			{
				if ( this.AppendNextBlockIfType( continueType ) == false )
					break;

				// Advance the stream to the next record
				this.ownerStream.Position += this.blocks[ this.blocks.Count - 1 ].BlockLength;
			}
		}

		#endregion AppendContinueBlocks

		#region AppendNextBlockIfType

		protected bool AppendNextBlockIfType( int type )
		{
			// MD 10/7/10 - TFS36582
			// When reading in an Excel file from a 3rd party, it had an extra byte at the end of the file. We need to 
			// read two bytes when reading a record type, so make sure we have enough room to read two bytes.
			//if ( this.ownerStream.Position == this.ownerStream.Length )
			if (this.ownerStream.Position >= this.ownerStream.Length - 1)
				return false;

			// MD 9/15/08 - TFS7442
			// Save the position so it can be reset later if necessary.
			long position = this.ownerStream.Position;

			// Read the next record type
			int nextRecordType = this.ReadBlockType( this.ownerStreamReader );

			if ( this.ShouldAppendNextBlockIfType( type, nextRecordType ) == false )
			{
				// MD 9/15/08 - TFS7442
				// If the next record type is not what we are looking for, reset the stream back to its original position.
				this.ownerStream.Position = position;

				return false;
			}

			// Read the length of the continuation record
			int nextRecordLength = this.ReadBlockLength( this.ownerStreamReader );

			// MD 6/6/07 - BR23645
			// For some reason, the TXO records don't seem to extend longer than 18 bytes in length.
			// Some shapes have a ClientTextBox record with a following TXO record. There are CONTINUE records
			// after the TXO record which are 18 bytes or less and apply to the TXO. In other cases, there are
			// CONTINUE records after the TXO more than 18 bytes in length. These seems to apply to the MSODRAWING,
			// not the TXO.
			// MD 6/14/07 - BR23880
			// Moved the value 18 into a constant to be used in other places
			//if ( this.recordType == BIFFType.TXO && nextRecordLength > 18 )
			// MD 7/20/2007 - BR25039
			// The previous assumption was incorrect, the first block is always 18 bytes in a TXO record, 
			// and the remaining CONTINUE blocks are of variable length. This check has been removed, because 
			// we will never come in here for TXO records, we will call AppendContinueBlocksForTXORecord instead
			//if ( this.recordType == BIFFType.TXO && nextRecordLength > BiffRecordStream.MaxBlockSizeForTXORecord )
			//    return false;

			this.length += nextRecordLength;

			// MD 6/14/07 - BR23880
			// Use the factory method to create new record blocks now
			//this.blocks.Add( new RecordBlockInfo( this.workbookStream.Position, nextRecordLength ) );
			this.blocks.Add( this.CreateRecordBlock( this.ownerStream.Position, nextRecordLength ) );
			return true;
		}

		#endregion AppendNextBlockIfType

		// MD 6/14/07 - BR23880
		#region CreateRecordBlock



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		protected RecordBlockInfo CreateRecordBlock( long blockStart, int blockLength )
		{
			// MD 7/20/2007 - BR25039
			// The previous assumption was incorrect, the first block is always 18 bytes in a TXO record, 
			// and the remaining CONTINUE blocks are of variable length
			//if ( this.recordType == BIFFType.TXO )
			//    return new RecordBlockInfo( blockStart, blockLength, BiffRecordStream.MaxBlockSizeForTXORecord );

			return new RecordBlockInfo( blockStart, blockLength, this.MaximumBlockSize );
		}

		#endregion CreateRecordBlock

		#region InitializeAlternateStream

		protected virtual void InitializeAlternateStream( BiffRecordStream copyStream, Stream newWorkbookStream, long startPositionInNewStream )
		{
			// MD 8/2/07
			// Let the position re-sync itself the next time we need something
			//stream.cachedWorkbookStreamPosition = newWorkbookStream.Position;
			copyStream.cachedWorkbookStreamPosition = -1;

			copyStream.length = this.length;
			copyStream.position = this.position;

			copyStream.blocks = new List<RecordBlockInfo>();

			long offset = startPositionInNewStream - this.blocks[ 0 ].BlockStart;

			foreach ( RecordBlockInfo block in this.blocks )
			{
				copyStream.blocks.Add( new RecordBlockInfo(
					block.BlockStart + offset,
					block.BlockLength,
					block.MaximumBlockLength ) );
			}

			// MD 5/29/07 - BR23353
			// The current block should never be considered modified on the copy stream
			//stream.currentBlockWasModified = this.currentBlockWasModified;

			copyStream.currentBlock = this.currentBlock;
			copyStream.positionInCurrentBlock = this.positionInCurrentBlock;
			copyStream.recordType = this.recordType;
			copyStream.isOpen = this.isOpen;
			copyStream.manager = this.manager;
			copyStream.ownerStream = newWorkbookStream;
			copyStream.parentStream = this.parentStream;

			if ( this.subStreams != null )
			{
				// MD 4/28/11 - TFS62775
				// Added a way for BiffRecordStream instances to have sub-streams of other types.
				//copyStream.subStreams = new List<BiffRecordStream>( this.subStreams );
				copyStream.subStreams = new List<IBiffRecordStream>(this.subStreams);
			}

			copyStream.nextBlockType = this.nextBlockType;

			// MD 4/28/11
			// Found while fixing TFS62775
			copyStream.isFirstBlockHeaderWritten = this.isFirstBlockHeaderWritten;
		}

		#endregion InitializeAlternateStream

		#region ReadFormattedStringHelper

		protected virtual FormattedStringInfo ReadFormattedStringHelper( int remainingLength )
		{
			// MD 9/25/08 - TFS8010
			// If we are at the end of the stream, return null so an exception doesn't get thrown on the next read operation.
			if ( this.Position == this.Length )
				return null;

			// Read the options for this string
			byte optionFlags = (byte)this.ReadByte();

			bool charCompression = ( optionFlags & 0x01 ) == 0x00;
			bool hasAsianPhoneticSettings = ( optionFlags & 0x04 ) == 0x04;
			bool hasRichText = ( optionFlags & 0x08 ) == 0x08;

			// MD 9/25/08 - TFS8010
			// If we are at the end of the stream, return null so an exception doesn't get thrown on the next read operation.
			if ( hasRichText && this.Position + 1 >= this.Length )
				return null;

			// If this string contains formatting runs, determine the number of runs
			ushort richTextFormattingRuns = hasRichText
				? this.ReadUInt16()
				: (ushort)0;

			// MD 7/27/10 - TFS36066
			int asianPhoneticSettingsSize = 0;

			// If this string has an asian phonetic settings block, determin the size of it
			if ( hasAsianPhoneticSettings )
			{
				// MD 9/25/08 - TFS8010
				// If we are at the end of the stream, return null so an exception doesn't get thrown on the next read operation.
				if ( this.Position + 3 >= this.Length )
					return null;

				// MD 7/27/10 - TFS36066
				// Sotre the value, because this is the exact amount of bytes in the block.
				//this.ReadUInt32();
				asianPhoneticSettingsSize = this.ReadInt32();
			}

			// Determine the correct encoding needed to read the string from the bytes.
			Encoding encoding = charCompression
				// MD 1/9/09 - TFS12270
				// This was moved to the Workbook just in case it is needed in other places.
				//// MD 4/9/08 - BR31743
				//// The compressed text encoding is not dependent on the active code page file. It remains constant.
				////? Encoding.Default
				//? BiffRecordStream.CompressedTextEncoding
				? Workbook.InvariantCompressedTextEncoding
				: Encoding.Unicode;

			// Determine the number of bytes to read from the record based on the string
			// length and whether the characters are compressed to 8-bits values.
			int bytesToRead = remainingLength;

			if ( charCompression == false )
				bytesToRead *= 2;

			// Read the data, but only from the current block, continuations of this string
			// will have new options flags
			byte[] data = new byte[ bytesToRead ];
			int bytesRead = this.ReadFromCurrentBlock( data, 0, bytesToRead );

			// Get the string read from the current block
			string value = encoding.GetString( data, 0, bytesRead );

			// If we did not read enough characters from this block, read the coninutation
			// of the string from the next block
			if ( value.Length != remainingLength )
			{
				FormattedStringInfo additionalInfo = this.ReadFormattedStringHelper( remainingLength - value.Length );

				// MD 9/25/08 - TFS8010
				// The info returned can now be null, so return null if it is.
				if ( additionalInfo == null )
					return null;

				value += additionalInfo.UnformattedText;

				// MD 9/25/08 - TFS8010
				Debug.Assert( value.Length == remainingLength, "The full string waas not read in." );
			}

			// Construct the formatted string info instance and return it.
			FormattedStringInfo info = new FormattedStringInfo();

			// MD 7/27/10 - TFS36066
			// Just store the size now. A size of 0 implies that there is no block here.
			//info.HasAsianPhoneticSettings = hasAsianPhoneticSettings;
			info.AsianPhoneticSettingsSize = asianPhoneticSettingsSize;

			info.HasRichText = hasRichText;
			info.RichTextFormattingRuns = richTextFormattingRuns;
			info.UnformattedText = value;

			return info;
		}

		#endregion ReadFormattedStringHelper

		#region ShouldAppendNextBlockIfType

		protected virtual bool ShouldAppendNextBlockIfType( int expectedType, int actualType )
		{
			return expectedType == actualType;
		}

		#endregion ShouldAppendNextBlockIfType 

		#region WriteToCurrentBlock

		protected virtual int WriteToCurrentBlock( byte[] buffer, int offset, int count )
		{
			this.SyncWorkbookStreamPosition();

			RecordBlockInfo currentBlockInfo = this.blocks[ this.currentBlock ];

			// If we have reached the end of the current block, add a new one to write to
			if ( this.positionInCurrentBlock == currentBlockInfo.MaximumBlockLength )
			{
				// Move to the next block
				this.IncrementCurrentBlock();
				currentBlockInfo = this.blocks[ this.currentBlock ];

				// We are at the start of the next block
				this.positionInCurrentBlock = 0;

				// Move the underlying stream's position past the CONTINUE record's header
				this.ownerStream.Position = currentBlockInfo.BlockStart;
				this.cachedWorkbookStreamPosition = this.ownerStream.Position;
			}

			// Write the maximum amount of bytes we can to the current block, up to the count
			int bytesToWrite = Math.Min( count, currentBlockInfo.MaximumBlockLength - this.positionInCurrentBlock );
			this.ownerStream.Write( buffer, offset, bytesToWrite );
			int bytesWritten = (int)( this.ownerStream.Position - this.cachedWorkbookStreamPosition );

			Debug.Assert( bytesWritten == bytesToWrite );

			if ( bytesWritten > 0 )
				this.currentBlockWasModified = true;

			// Advance the position in the current block
			this.positionInCurrentBlock += bytesWritten;
			this.position += bytesWritten;
			this.cachedWorkbookStreamPosition += bytesWritten;
			Debug.Assert( this.positionInCurrentBlock <= currentBlockInfo.MaximumBlockLength );

			return bytesWritten;
		}

		#endregion WriteToCurrentBlock

		#endregion Protected Methods

		#region Private Methods

		// MD 5/21/10 - TFS31514
		#region AdvancePositionInBuffer

		private void AdvancePositionInBuffer(ref byte[] buffer, ref int bufferPosition, int count)
		{
			if (bufferPosition + count > buffer.Length)
				this.CacheNextBlock(ref buffer, ref bufferPosition);

			bufferPosition += count;
		}

		#endregion // AdvancePositionInBuffer

		#region AppendNewBlock

		private void AppendNewBlock()
		{
			this.WriteNewBlock( this.GetNextBlockType() );

			// Update the position in the biff stream
			this.cachedWorkbookStreamPosition += 4;

			// Move to the next block
			this.IncrementCurrentBlock();

			// We are at the start of the next block
			this.positionInCurrentBlock = 0;
		}

		#endregion AppendNewBlock

		// MD 9/23/09 - TFS19150
		#region CacheNextBlock

		private void CacheNextBlock( ref byte[] buffer, ref int bufferPosition )
		{
			int copyEndCount = 0;
			if ( bufferPosition < buffer.Length )
				copyEndCount = buffer.Length - bufferPosition;

			RecordBlockInfo currentBlockInfo = this.blocks[ this.currentBlock ];
			int currentBlockLength = currentBlockInfo.BlockLength;

			int blockLength;
			if ( this.positionInCurrentBlock == currentBlockLength )
				blockLength = this.blocks[ this.currentBlock + 1 ].BlockLength;
			else
				blockLength = currentBlockLength - this.positionInCurrentBlock;

			byte[] newBuffer = new byte[ copyEndCount + blockLength ];

			if ( copyEndCount != 0 )
				Buffer.BlockCopy( buffer, bufferPosition, newBuffer, 0, copyEndCount );

			this.ReadFromCurrentBlock( newBuffer, copyEndCount, blockLength );

			bufferPosition = 0;
			buffer = newBuffer;
		}

		#endregion CacheNextBlock

		#region CellAddressFromEncodedValues

		private static CellAddress CellAddressFromEncodedValues( ushort encodedRow, ushort encodedColumn )
		{
			bool relativeColumnIndex = ( encodedColumn & 0x4000 ) == 0x4000;
			bool relativeRowIndex = ( encodedColumn & 0x8000 ) == 0x8000;

			short column = (byte)( encodedColumn & 0x00FF );

			return new CellAddress( encodedRow, relativeRowIndex, column, relativeColumnIndex );
		}

		#endregion CellAddressFromEncodedValues

		#region CellAddressToEncodedValues

		private static void CellAddressToEncodedValues( CellAddress address, out ushort encodedRow, out ushort encodedColumn )
		{
			encodedColumn = (ushort)address.Column;

			if ( address.ColumnAddressIsRelative )
				encodedColumn |= 0x4000;

			if ( address.RowAddressIsRelative )
				encodedColumn |= 0x8000;

			encodedRow = (ushort)address.Row;
		}

		#endregion CellAddressToEncodedValues

		#region CloseCurrentBlock

		private void CloseCurrentBlock()
		{
			if ( this.currentBlockWasModified )
			{
				// Cache the current position of the biff stream
				long currentPosition = this.ownerStream.Position;

				RecordBlockInfo currentBlockInfo = this.blocks[ this.currentBlock ];

				// MD 4/18/11 - TFS62026
				// Instead of seeking to the beginning of the record and then making the WriteBlockLength method seek again 
				// by passing in True for the isAfterLengthPosition parameter, we should just seek to the position of the length
				// field in the header and then write it directly.
				//// Seek to the beginning of the record and write the length of the current block
				//this.ownerStream.Position = currentBlockInfo.BlockStart;
				//
				//// Write the new length of the block
				//this.WriteBlockLength( ownerStream, currentBlockInfo.BlockLength, true ); 
				//
				// Seek to the beginning of the block length field
				this.ownerStream.Position = currentBlockInfo.BlockStart - this.BlockLengthSize;

				// Write the new length of the block
				this.WriteBlockLength(ownerStream, currentBlockInfo.BlockLength, false);

				// Move the stream position back to where it was
				this.ownerStream.Position = currentPosition;

				this.currentBlockWasModified = false;
			}
			// MD 4/18/11 - TFS62026
			// If the first block header still wasn't written, write it now with an assumed block length of 0.
			else if (this.isFirstBlockHeaderWritten == false)
			{
				this.WriteFirstBlockHeader(0);
			}
		}

		#endregion CloseCurrentBlock

		#region GetNextBlockType

		private int GetNextBlockType()
		{
			int type = this.NextBlockTypeInternal;

			if ( type == this.DefaultRecordId )
				return this.GetDefaultContinuationBlockType();

			this.NextBlockTypeInternal = this.DefaultRecordId;
			return type;
		} 

		#endregion GetNextBlockType

		#region IncrementCurrentBlock

		private void IncrementCurrentBlock()
		{
			this.CloseCurrentBlock();
			this.currentBlock++;
		}

		#endregion IncrementCurrentBlock

		#region ReadAsianPhoneticSettingsBlock

		// MD 7/27/10 - TFS36066
		// Added a settings block size parameter.
		//private void ReadAsianPhoneticSettingsBlock()
		private void ReadAsianPhoneticSettingsBlock(int asianPhoneticSettingsSize)
		{
			// MD 7/27/10 - TFS36066
			// This is not right. And since we don't use the data now anyway, just skip passed it.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


			#endregion // Old Code
			this.Seek(asianPhoneticSettingsSize, SeekOrigin.Current);
		}

		#endregion ReadAsianPhoneticSettingsBlock

		// MD 9/23/09 - TFS19150
		#region ReadAsianPhoneticSettingsBlockFromBuffer

		// MD 7/27/10 - TFS36066
		// Added a settings block size parameter.
		//private void ReadAsianPhoneticSettingsBlockFromBuffer( ref byte[] buffer, ref int bufferPosition )
		private void ReadAsianPhoneticSettingsBlockFromBuffer(ref byte[] buffer, ref int bufferPosition, int asianPhoneticSettingsSize)
		{
			// MD 7/27/10 - TFS36066
			// This is not right. And since we don't use the data now anyway, just skip passed it.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


			#endregion // Old Code
			this.AdvancePositionInBuffer(ref buffer, ref bufferPosition, asianPhoneticSettingsSize);
		}

		#endregion ReadAsianPhoneticSettingsBlockFromBuffer

		// MD 9/23/09 - TFS19150
		#region ReadFormattedStringFromBufferHelper

		private FormattedStringInfo ReadFormattedStringFromBufferHelper( int remainingLength, ref byte[] data, ref int dataIndex )
		{
			// Read the options for this string
			byte optionFlags = this.ReadByteFromBuffer( ref data, ref dataIndex );

			bool charCompression = ( optionFlags & 0x01 ) == 0x00;
			bool hasAsianPhoneticSettings = ( optionFlags & 0x04 ) == 0x04;
			bool hasRichText = ( optionFlags & 0x08 ) == 0x08;

			// If this string contains formatting runs, determine the number of runs
			ushort richTextFormattingRuns = hasRichText
				? this.ReadUInt16FromBuffer( ref data, ref dataIndex )
				: (ushort)0;

			// MD 7/27/10 - TFS36066
			int asianPhoneticSettingsSize = 0;

			// If this string has an asian phonetic settings block, determin the size of it
			if ( hasAsianPhoneticSettings )
			{
				// MD 7/27/10 - TFS36066
				// Sotre the value, because this is the exact amount of bytes in the block.
				//this.ReadUInt32FromBuffer( ref data, ref dataIndex );
				asianPhoneticSettingsSize = this.ReadInt32FromBuffer(ref data, ref dataIndex);
			}

			// Determine the correct encoding needed to read the string from the bytes.
			Encoding encoding = charCompression
				// MD 4/9/08 - BR31743
				// The compressed text encoding is not dependent on the active code page file. It remains constant.
				//? Encoding.Default
				? Workbook.InvariantCompressedTextEncoding
				: Encoding.Unicode;

			// Determine the number of bytes to read from the record based on the string
			// length and whether the characters are compressed to 8-bits values.
			int bytesToRead = remainingLength;

			if ( charCompression == false )
				bytesToRead *= 2;

			// Read the data, but only from the current block, continuations of this string
			// will have new options flags
			int bytesRead = Math.Min( bytesToRead, data.Length - dataIndex );
			string value = encoding.GetString( data, dataIndex, bytesRead );
			dataIndex += bytesRead;

			// If we did not read enough characters from this block, read the coninutation
			// of the string from the next block
			if ( value.Length != remainingLength )
			{
				this.CacheNextBlock( ref data, ref dataIndex );

				FormattedStringInfo additionalInfo = this.ReadFormattedStringFromBufferHelper( remainingLength - value.Length, ref data, ref dataIndex );
				value += additionalInfo.UnformattedText;
			}

			// Construct the formatted string info instance and return it.
			FormattedStringInfo info = new FormattedStringInfo();

			// MD 7/27/10 - TFS36066
			// Just store the size now. A size of 0 implies that there is no block here.
			//info.HasAsianPhoneticSettings = hasAsianPhoneticSettings;
			info.AsianPhoneticSettingsSize = asianPhoneticSettingsSize;

			info.HasRichText = hasRichText;
			info.RichTextFormattingRuns = richTextFormattingRuns;
			info.UnformattedText = value;

			return info;
		}

		#endregion ReadFormattedStringFromBufferHelper

		#region ReadFormattingRuns

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//private void ReadFormattingRuns( FormattedString formattedString, ushort richTextFormattingRuns )
		//private void ReadFormattingRuns(FormattedStringElement formattedString, ushort richTextFormattingRuns)
		private void ReadFormattingRuns(FormattedStringElement formattedString, ushort richTextFormattingRuns)
		{
			// MD 2/2/12 - TFS100573
			Workbook workbook = this.Manager.Workbook;

			for ( int i = 0; i < richTextFormattingRuns; i++ )
			{
				ushort firstFormattedChar = this.ReadUInt16();
				ushort fontRecordIndex = this.ReadUInt16();

				// MD 11/9/11 - TFS85193
				//FormattingRun formattingRun = new FormattingRun(formattedString, firstFormattedChar, this.manager.Workbook);
				//formattingRun.Font.SetFontFormatting(this.manager.Fonts[fontRecordIndex]);
				FormattedStringRun formattingRun = new FormattedStringRun(formattedString, firstFormattedChar);
				formattingRun.GetFontInternal(this.manager.Workbook).SetFontFormatting( this.manager.Fonts[ fontRecordIndex ] );

				formattedString.FormattingRuns.Add( formattingRun );
			}
		}

		#endregion ReadFormattingRuns

		// MD 9/23/09 - TFS19150
		#region ReadFormattingRunsFromBuffer

		// MD 11/3/10 - TFS49093
		// The formatted string data is now stored on the FormattedStringElement.
		//private void ReadFormattingRunsFromBuffer( FormattedString formattedString, ushort richTextFormattingRuns, ref byte[] buffer, ref int bufferPosition )
		//private void ReadFormattingRunsFromBuffer(FormattedStringElement formattedString, ushort richTextFormattingRuns, ref byte[] buffer, ref int bufferPosition)
		private void ReadFormattingRunsFromBuffer(FormattedStringElement formattedString, ushort richTextFormattingRuns, ref byte[] buffer, ref int bufferPosition)
		{
			// MD 2/2/12 - TFS100573
			Workbook workbook = this.Manager.Workbook;

			for ( int i = 0; i < richTextFormattingRuns; i++ )
			{
				ushort firstFormattedChar = this.ReadUInt16FromBuffer( ref buffer, ref bufferPosition );

				// MD 3/26/12 - TFS106075
				// We don't need to force a formatting run at the start of the string anymore now that we always resolve format
				// properties on FormattedString instances. In addition, this could be wrong when loading some workbooks created
				// on Asian systems because the worksheet don't seem to use the Normal style. They seem to have a different 
				// default format/font for cells.
				#region Removed

				//// MD 2/15/11
				//// Found while fixing TFS66333
				//// To be consistent with the Excel 2007 format, all formatting runs in the formatted string should be fully resolved.
				//// So if the first run is implied instead of specified, we should initialize it manually.
				//if (i == 0 && firstFormattedChar > 0)
				//{
				//    // MD 11/9/11 - TFS85193
				//    //FormattingRun firstFormattingRun = new FormattingRun(formattedString, 0, this.manager.Workbook);
				//    //firstFormattingRun.Font.SetFontFormatting(this.manager.Fonts[0]);
				//    FormattedStringRun firstFormattingRun = new FormattedStringRun(formattedString, 0);
				//    firstFormattingRun.GetFont(this.manager.Workbook).SetFontFormatting(this.manager.Fonts[0]);
				//
				//    formattedString.FormattingRuns.Add(firstFormattingRun);
				//}

				#endregion // Removed

				ushort fontRecordIndex = this.ReadUInt16FromBuffer( ref buffer, ref bufferPosition );

				// MD 11/9/11 - TFS85193
				//FormattingRun formattingRun = new FormattingRun(formattedString, firstFormattedChar, this.manager.Workbook);
				//formattingRun.Font.SetFontFormatting(this.manager.Fonts[fontRecordIndex]);
				FormattedStringRun formattingRun = new FormattedStringRun(formattedString, firstFormattedChar);
				formattingRun.GetFont(this.manager.Workbook).SetFontFormatting(this.manager.Fonts[fontRecordIndex]);

				formattedString.FormattingRuns.Add( formattingRun );
			}
		}

		#endregion ReadFormattingRunsFromBuffer

		#region ReadFromCurrentBlock

		private int ReadFromCurrentBlock( byte[] buffer, int offset, int count )
		{
			// MD 8/20/07 - BR25818
			// If no bytes need to be written, just return 0
			if ( count == 0 )
				return 0;

			this.SyncWorkbookStreamPosition();

			RecordBlockInfo currentBlockInfo = this.blocks[ this.currentBlock ];

			// Find the length of the current block in which the position is located
			int currentBlockLength = currentBlockInfo.BlockLength;

			// If we have reached the end of the current block, we must update the reader's position
			if ( this.positionInCurrentBlock == currentBlockLength )
			{
				if ( this.currentBlock == this.blocks.Count - 1 )
				{
					Utilities.DebugFail( "We shouldn't have read past the end of the stream." );
					return 0;
				}
				else
				{
					// Move to the next block
					this.IncrementCurrentBlock();
					currentBlockInfo = this.blocks[ this.currentBlock ];

					// We are at the start of the next block
					this.positionInCurrentBlock = 0;

					// Move the underlying stream's position past the CONTINUE record's header
					this.ownerStream.Position = currentBlockInfo.BlockStart;
					this.cachedWorkbookStreamPosition = this.ownerStream.Position;

					currentBlockLength = currentBlockInfo.BlockLength;
				}
			}

			// Read the maximum amount of bytes we can from the current block, up to the count
			int bytesToRead = Math.Min( count, currentBlockLength - this.positionInCurrentBlock );
			int bytesRead = this.ownerStream.Read( buffer, offset, bytesToRead );
			Debug.Assert( bytesRead == bytesToRead );

			// Advance the position in the current block
			this.positionInCurrentBlock += bytesRead;
			this.position += bytesRead;
			this.cachedWorkbookStreamPosition += bytesRead;
			Debug.Assert( this.positionInCurrentBlock <= currentBlockLength );

			return bytesToRead;
		}

		#endregion ReadFromCurrentBlock

		#region SyncPositionMembers

		private void SyncPositionMembers()
		{
			// Assume the new position will be in the first block
			this.CloseCurrentBlock();
			this.currentBlock = 0;

			// Keep track of the position in the BIFF record as we walk through
			long recordPosition = 0;

			while ( true )
			{
				// If the block we assume we are in is past the blocks in the record,
				// we are outside the record
				if ( this.currentBlock == this.blocks.Count )
				{
					this.cachedWorkbookStreamPosition = -1;
					return;
				}

				RecordBlockInfo currentBlockInfo = this.blocks[ this.currentBlock ];

				// Get the length of the block we assume we are in
				int currentBlockLength = currentBlockInfo.BlockLength;
				long underlyingStreamPosition = currentBlockInfo.BlockStart;

				// Figure out the position where we would be in this new block
				this.positionInCurrentBlock = (int)( this.position - recordPosition );

				// If the position is valid in the block, update the underlying stream to be at
				// the actual position
				if ( this.positionInCurrentBlock <= currentBlockLength )
				{
					underlyingStreamPosition += this.positionInCurrentBlock;

					// Cache the new position so we don't do any work on the next sync if the 
					// position doesn't change after this.
					this.cachedWorkbookStreamPosition = underlyingStreamPosition;
					this.ownerStream.Position = this.cachedWorkbookStreamPosition;

					return;
				}

				// Increment the record position by the length of the block
				recordPosition += currentBlockLength;

				// Assue we are now in the next block
				this.IncrementCurrentBlock();
			}
		}

		#endregion SyncPositionMembers

		// MD 4/18/11 - TFS62026
		#region WriteFirstBlockHeader

		private void WriteFirstBlockHeader(int assumedBlockLength)
		{
			long headerPosition = this.GetStartOfRecord();

			// Move the stream to the beginning of the record if it is not already there.
			if (this.cachedWorkbookStreamPosition != headerPosition ||
				this.cachedWorkbookStreamPosition != this.ownerStream.Position)
			{
				this.ownerStream.Position = headerPosition;
				this.cachedWorkbookStreamPosition = headerPosition;
			}

			// Write the record type and assumed length to the header.
			this.WriteBlockType(this.ownerStream, this.recordType);
			this.WriteBlockLength(this.ownerStream, assumedBlockLength, false);

			// Set the flag indicating the first block header is now written.
			this.isFirstBlockHeaderWritten = true;

			// Update the position where we assume the owner stream is.
			this.cachedWorkbookStreamPosition = this.ownerStream.Position;
		}

		#endregion // WriteFirstBlockHeader

		#region WriteFormattedStringHelper

		private void WriteFormattedStringHelper(
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString value, 
			StringElement value, 
			int startPosition, 
			bool writeHeader, 
			bool writeLength, 
			LengthType lengthType,
			bool allowCharCompression,
			bool writeFormattingRuns )		// MD 7/20/2007 - BR25039
		{
			// MD 1/31/12 - TFS100573
			FormattedStringElement formattedStringElement = value as FormattedStringElement;

			int headerLength = 0;
			bool hasRichText = false;

			if ( writeHeader )
			{
				// Leave room for length field
				if ( writeLength )
				{
					if ( lengthType == LengthType.EightBit )
						headerLength += 1;
					else
						headerLength += 2;
				}

				// Only write the formatting runs length if this portion has the header
				// MD 7/20/2007 - BR25039
				// If we don't want to write the formatting runs, don't check the FormattingRuns collection
				// and leave hasRichText set to False
				//hasRichText = value.FormattingRuns.Count > 0;
				if ( writeFormattingRuns )
				{
					// MD 11/3/10 - TFS49093
					// We are now lazily creating FormattingRuns, so check the HasFormatting value instead.
					//hasRichText = value.FormattingRuns.Count > 0;
					// MD 1/31/12 - TFS100573
					//hasRichText = value.HasFormatting;
					hasRichText = formattedStringElement != null && formattedStringElement.HasFormatting;
				}
			}

			// Option flags
			headerLength++;

			// Formatting runs length
			if ( hasRichText )
				headerLength += 2;

			int minLengthNeededInCurrentBlock = headerLength;

			// MD 7/5/07 - BR24226
			// Only leave room for at least one char if there is at least one char (if condition was added)
			if ( startPosition < value.UnformattedString.Length )
			{
				// Leave room for first char
				if ( allowCharCompression == false || Utilities.IsUnicodeString( value.UnformattedString.Substring( startPosition, 1 ) ) )
					minLengthNeededInCurrentBlock += 2;
				else
					minLengthNeededInCurrentBlock += 1;
			}

			// If there isnt enough room in the current block for the header and one char, move to the next block
			if ( this.BytesAvailableInCurrentBlock < minLengthNeededInCurrentBlock )
			{
				this.SyncWorkbookStreamPosition();
				this.CloseCurrentBlock();
				this.AppendNewBlock();

				Debug.Assert( this.positionInCurrentBlock == 0 );
			}

			int lengthForStringInBlock = this.BytesAvailableInCurrentBlock - headerLength;

			if ( writeHeader )
			{
				// Write the length of the string
				if ( writeLength )
				{
					if ( lengthType == LengthType.EightBit )
						this.Write( (byte)value.UnformattedString.Length );
					else
						this.Write( (ushort)value.UnformattedString.Length );
				} 
			}

			int availableLengthInString = value.UnformattedString.Length - startPosition;
			string portion;
			bool charCompression;

			if ( allowCharCompression )
			{
				// Get the portion that will be written out using the char compression
				portion = value.UnformattedString.Substring( 
					startPosition,
					Math.Min( availableLengthInString, lengthForStringInBlock ) );

				charCompression = true;

				// If we can't use char compression on the unicode string, shorten the string
				if ( Utilities.IsUnicodeString( portion ) )
				{
					portion = value.UnformattedString.Substring(
						startPosition,
						Math.Min( availableLengthInString, lengthForStringInBlock / 2 ) );

					// MD 1/7/08 - BR29111
					// This caused a problem. What was going on here was if the string portion we were writing to the block
					// contained unicode characters, each character needed to be represented by two bytes instead of one. 
					// Therefore, we create a new (possible shorter) portion to store in the block with two bytes per character. 
					// In the rare occasion that the original portion had a unicode character but the second portion was shorter 
					// and did not have a unicode character, this line of code below would cause us to write out that shorter 
					// portion as a compressed string again (one byte per character instead of two). Apparently Excel doesn't 
					// like this and won't open files where that has happened. If the shorter portion has no unicode characters, 
					// we still have to write out the portion as a unicode string (two bytes per character).
					//charCompression = Utilities.IsUnicodeString( portion ) == false;
					charCompression = false;
				}
			}
			else
			{
				portion = value.UnformattedString.Substring( 
					startPosition,
					Math.Min( availableLengthInString, lengthForStringInBlock / 2 ) );

				charCompression = false;
			}

			// Write the options for this string
			byte optionFlags = 0;

			// MD 7/25/07 - BR25198
			// The options flags are always written, so they must be set correctly even when we don't write the header
			//if ( writeHeader )
			{
				if ( charCompression == false )
					optionFlags |= 0x01;

				if ( hasRichText )
					optionFlags |= 0x08;
			}

			this.Write( optionFlags );

			if ( hasRichText )
			{
				// MD 1/31/12 - TFS100573
				//this.Write( (ushort)value.FormattingRuns.Count );
				this.Write((ushort)formattedStringElement.FormattingRuns.Count);
			}

			// Determine the correct encoding needed to read the string from the bytes.
			Encoding encoding = charCompression
				// MD 1/9/09 - TFS12270
				// This was moved to the Workbook just in case it is needed in other places.
				//// MD 4/9/08 - BR31743
				//// The compressed text encoding is not dependent on the active code page file. It remains constant.
				////? Encoding.Default
				//? BiffRecordStream.CompressedTextEncoding
				? Workbook.InvariantCompressedTextEncoding
				: Encoding.Unicode;

			this.Write( encoding.GetBytes( portion ) );

			int nextStartPosition = startPosition + portion.Length;

			if ( nextStartPosition < value.UnformattedString.Length )
			{
				// MD 7/20/2007 - BR25039
				// WriteFormattedStringHelper takes another parameter now indicating whether to write the formatting runs
				//this.WriteFormattedStringHelper( value, nextStartPosition, false, false, lengthType, allowCharCompression );
				this.WriteFormattedStringHelper( value, nextStartPosition, false, false, lengthType, allowCharCompression, writeFormattingRuns );
			}

			if ( hasRichText )
			{
				// MD 11/9/11 - TFS85193
				//foreach ( FormattingRun run in value.FormattingRuns )
				// MD 1/31/12 - TFS100573
				//foreach ( FormattedStringRun run in value.FormattingRuns )
				foreach (FormattedStringRun run in formattedStringElement.FormattingRuns)
				{
					if ( this.BytesAvailableInCurrentBlock < 4 )
					{
						this.CloseCurrentBlock();
						this.AppendNewBlock();
					}

					this.Write( (ushort)run.FirstFormattedCharAbsolute );

					int fontIndex = 0;

					if ( run.HasFont )
					{
						// MD 11/9/11 - TFS85193
						//fontIndex = run.Font.Element.IndexInFontCollection;
						// MD 1/18/12 - 12.1 - Cell Format Updates
						//fontIndex = run.GetFontInternal(this.manager.Workbook).Element.GetIndexInFontCollection(FontResolverType.Normal);
						fontIndex = run.GetFontInternal(this.manager.Workbook).SaveIndex;

						if ( fontIndex < 0 )
						{
							Utilities.DebugFail( "Unknown font index" );
							fontIndex = 0;
						}
					}

					this.Write( (ushort)fontIndex );
				}
			}
		}

		#endregion WriteFormattedStringHelper

		#region WriteNewBlock

		private void WriteNewBlock( int newBlockType )
		{
			// Write the continuation record type
			this.WriteBlockType( this.ownerStream, newBlockType );

			// Write a length of 0 for now
			this.WriteBlockLength( this.ownerStream, 0, false );

			// Initialize the next block length and start
			// MD 6/14/07 - BR23880
			// Use the factory method to create new record blocks now
			//this.blocks.Add( new RecordBlockInfo( this.workbookStream.Position, 0 ) );
			this.blocks.Add( this.CreateRecordBlock( this.ownerStream.Position, 0 ) );
		}

		#endregion WriteNewBlock

		#endregion Private Methods

		#endregion Methods

		#region Properties

		// MD 4/18/11 - TFS62026
		protected abstract int BlockLengthSize { get; }
		protected abstract long BlockTypeSize { get; }

		protected abstract int DefaultRecordId { get; }
		protected abstract int MaximumBlockSize { get; }

		#region Blocks

		protected List<RecordBlockInfo> Blocks
		{
			get { return this.blocks; }
		} 

		#endregion Blocks

		#region BytesAvailableInCurrentBlock

		public int BytesAvailableInCurrentBlock
		{
			get
			{
				this.SyncWorkbookStreamPosition();
				return this.blocks[ this.currentBlock ].MaximumBlockLength - this.positionInCurrentBlock;
			}
		}

		#endregion BytesAvailableInCurrentBlock

		#region EndUnderlyingStreamPosition

		// MD 4/28/11 - TFS62775
		// Since this is in the IBiffRecordStream interface, it must be defined as public.
		//protected long EndUnderlyingStreamPosition
		public long EndUnderlyingStreamPosition
		{
			get
			{
				long maxPosition = this.blocks[ this.blocks.Count - 1 ].BlockEnd;

				if ( this.subStreams != null )
				{
					// MD 4/28/11 - TFS62775
					// Added a way for BiffRecordStream instances to have sub-streams of other types.
					//foreach ( BiffRecordStream stream in this.subStreams )
					foreach (IBiffRecordStream stream in this.subStreams)
						maxPosition = Math.Max( maxPosition, stream.EndUnderlyingStreamPosition );
				}

				return maxPosition;
			}
		}

		#endregion EndUnderlyingStreamPosition

		#region LengthInternal

		protected int LengthInternal
		{
			get { return this.length; }
			set { this.length = value; }
		} 

		#endregion LengthInternal

		// MD 9/2/08 - Cell Comments
		#region Manager

		public WorkbookSerializationManager Manager
		{
			get { return this.manager; }
		} 

		#endregion Manager

		#region NextBlockTypeInternal

		public int NextBlockTypeInternal
		{
			get { return this.nextBlockType; }
			set { this.nextBlockType = value; }
		}

		#endregion NextBlockTypeInternal

		#region OwnerStream

		protected Stream OwnerStream
		{
			get { return this.ownerStream; }
		} 

		#endregion OwnerStream

		#region OwnerStreamReader

		protected BinaryReader OwnerStreamReader
		{
			get { return this.ownerStreamReader; }
		} 

		#endregion OwnerStreamReader

		#region PositionInCurrentBlock

		public int PositionInCurrentBlock
		{
			get { return this.positionInCurrentBlock; }
		}

		#endregion PositionInCurrentBlock

		#region RecordTypeInternal

		internal int RecordTypeInternal
		{
			get { return this.recordType; }
		}

		#endregion RecordTypeInternal

		#endregion Properties


		// MD 4/28/11 - TFS62775
		#region RawDataStream class






		private class RawDataStream : IBiffRecordStream
		{
			private BiffRecordStream parentStream;
			private long endPosition;

			public RawDataStream(Stream baseStream, byte[] data)
			{
				baseStream.Write(data, 0, data.Length);
				this.endPosition = baseStream.Position;
			}

			#region IBiffRecordStream Members

			BiffRecordStream IBiffRecordStream.ParentStream
			{
				get { return this.parentStream; }
				set { this.parentStream = value; }
			}

			long IBiffRecordStream.EndUnderlyingStreamPosition
			{
				get { return this.endPosition; }
			}

			#endregion
		}

		#endregion  // RawDataStream class

		#region RecordBlockInfo class

		protected class RecordBlockInfo
		{
			private long blockStart;
			private int blockLength;
			private int maximumBlockLength;

			// MD 6/14/07 - BR23880
			// This constructor is not used anymore
			//public RecordBlockInfo( long blockStart, ushort blockLength )
			//    : this( blockStart, blockLength, BiffRecordStream.MaxBlockSize ) { }

			public RecordBlockInfo( long blockStart, int blockLength, int maximumBlockLength )
			{
				this.blockStart = blockStart;
				this.blockLength = blockLength;
				this.maximumBlockLength = maximumBlockLength;
			}

			public void CapBlock()
			{
				this.maximumBlockLength = this.blockLength;
			}

			public long BlockEnd
			{
				get { return this.blockStart + this.blockLength; }
			}

			public int BlockLength
			{
				get { return this.blockLength; }
				set { this.blockLength = value; }
			}

			public long BlockStart
			{
				get { return this.blockStart; }
			}

			public int MaximumBlockLength
			{
				get { return this.maximumBlockLength; }
			}
		}

		#endregion RecordBlockInfo class

		#region FormattedStringInfo class

		protected class FormattedStringInfo
		{
			public string UnformattedText;

			public bool HasRichText;

			// MD 7/27/10 - TFS36066
			// We just store the size now because a size of 0 implies there is no settings block.
			//public bool HasAsianPhoneticSettings;
			public int AsianPhoneticSettingsSize;

			public ushort RichTextFormattingRuns;
		}

		#endregion FormattedStringInfo class
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