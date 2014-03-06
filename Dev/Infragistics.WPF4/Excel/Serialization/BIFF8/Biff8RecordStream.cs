using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements;
using Infragistics.Documents.Excel.Sorting;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8
{
	internal sealed class Biff8RecordStream : BiffRecordStream
	{
		#region Constants

		public const int MaxBlockSize = 8224;

		public const ushort FontNormalWeight = 0x0190;
		public const ushort FontBoldWeight = 0x02BC;

		#endregion Constants

		#region Member Variables

		// MD 7/20/2007 - BR25039
		// In some situations, we will write the TXO record in a full block and prevent further 
		// writing to the record
		private bool allowWriteForTXORecord = true;

		// MD 7/20/2007 - BR25039
		// There are some cases where we need to add CONTINUE blocks for the TXO record as we are reading.
		// Keep a flag to know when reading for the TXO record is taking place
		private bool readingTextForTXORecord = false; 

		#endregion Member Variables

        #region Constructor

        private Biff8RecordStream() { }

		public Biff8RecordStream( BIFF8WorkbookSerializationManager manager )
			: base( manager, manager.WorkbookStream, manager.WorkbookStreamReader ) { }

		public Biff8RecordStream( BIFF8WorkbookSerializationManager manager, BIFF8RecordType recordType )
			: base( manager, manager.WorkbookStream, (int)recordType ) { }

        #endregion Constructor

		#region Base Class Overrides

		// MD 9/2/08 - Cell Comments
		// Now that all shape text data is processed for BIFF8, no special processing needs to be done for TXO records.
		#region Not Used

		//#region AppendContinueBlocks
		//
		//protected override void AppendContinueBlocks()
		//{
		//    // MD 7/20/2007 - BR25039
		//    // Do special processing to append CONTINUE blocks to the TXO record
		//    if ( this.RecordType == BIFF8RecordType.TXO )
		//    {
		//        this.AppendContinueBlocksForTXORecord();
		//        return;
		//    }
		//
		//    base.AppendContinueBlocks();
		//}
		//
		//#endregion AppendContinueBlocks 

		#endregion Not Used

		// MD 4/18/11 - TFS62026
		#region BlockLengthSize

		protected override int BlockLengthSize
		{
			get { return 2; }
		} 

		#endregion // BlockLengthSize

		// MD 4/18/11 - TFS62026
		#region BlockTypeSize

		protected override long BlockTypeSize
		{
			get { return 2; }
		}

		#endregion // BlockTypeSize

		#region CopyForAlternateStream

		protected override BiffRecordStream CopyForAlternateStream( Stream newWorkbookStream, long startPositionInNewStream )
		{
			Biff8RecordStream copyStream = new Biff8RecordStream();
			this.InitializeAlternateStream( copyStream, newWorkbookStream, startPositionInNewStream );
			return copyStream;
		}

		#endregion CopyForAlternateStream

		#region DefaultRecordId

		protected override int DefaultRecordId
		{
			get { return (int)BIFF8RecordType.Default; }
		} 

		#endregion DefaultRecordId

		#region DumpContents



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


		#endregion DumpContents

		#region MaximumBlockSize

		protected override int MaximumBlockSize
		{
			get { return Biff8RecordStream.MaxBlockSize; }
		} 

		#endregion MaximumBlockSize

		#region GetDefaultContinuationBlockType

		protected override int GetDefaultContinuationBlockType()
		{
			return (int)Biff8RecordBase.GetContinuationBlockType( this.RecordType );
		}

		#endregion GetDefaultContinuationBlockType

		#region ReadFormattedStringHelper

		protected override FormattedStringInfo ReadFormattedStringHelper( int remainingLength )
		{
			// MD 7/20/2007 - BR25039
			// If we are reading for the TXO record and are at the end of the stream, 
			// try to append the next CONTINUE blcok.
			if ( this.readingTextForTXORecord && this.Length == this.Position )
				this.AppendNextBlockIfType( (int)BIFF8RecordType.CONTINUE );

			return base.ReadFormattedStringHelper( remainingLength );
		}

		#endregion ReadFormattedStringHelper

		#region ReadRecordLength

		protected override int ReadBlockLength( BinaryReader ownerStreamReader )
		{
			return ownerStreamReader.ReadUInt16();
		} 

		#endregion ReadRecordLength

		#region ReadRecordType

		protected override int ReadBlockType( BinaryReader ownerStreamReader )
		{
			return ownerStreamReader.ReadUInt16();
		}

		#endregion ReadRecordType

		#region ShouldAppendNextBlockIfType

		protected override bool ShouldAppendNextBlockIfType( int expectedType, int actualType )
		{
			// MD 9/25/08 - TFS8010
			// If a TXO record already has the data it needs appended, ignore the next record, even if it is a CONTINUE record. That should be applied to
			// the owning record, which is probably a ClientTextBox escher record.
			if ( this.RecordType == BIFF8RecordType.TXO )
			{
				if ( this.IsAllTXODataAppended() )
					return false;
			}

			if ( base.ShouldAppendNextBlockIfType( expectedType, actualType ) )
				return true;

			BIFF8RecordType actualBIFF8Type = (BIFF8RecordType)actualType;

			// If the next record is not a continue record, stop searching
			if ( actualBIFF8Type == Biff8RecordBase.GetContinuationBlockType( (BIFF8RecordType)expectedType ) )
				return true;

			// This is a special case: occastionally, two MSODRAWINGGROUP records are written conseq.
			// These should be counted as one record
			if ( this.RecordType == BIFF8RecordType.MSODRAWINGGROUP &&
				actualBIFF8Type == BIFF8RecordType.MSODRAWINGGROUP )
			{
				return true;
			}

			return false;
		}

		#endregion ShouldAppendNextBlockIfType

		#region WriteRecordLength

		protected override void WriteBlockLength( Stream ownerStream, int length, bool isAfterLengthPosition )
		{
			// Seek to the beginning of the length field 
			if ( isAfterLengthPosition )
				ownerStream.Seek( -2, SeekOrigin.Current );

			ownerStream.Write( BitConverter.GetBytes( (ushort)length ), 0, 2 );
		} 

		#endregion WriteRecordLength

		#region WriteRecordType

		protected override void WriteBlockType( Stream ownerStream, int type )
		{
			ownerStream.Write( BitConverter.GetBytes( (ushort)type ), 0, 2 );
		} 

		#endregion WriteRecordType

		#region WriteToCurrentBlock

		protected override int WriteToCurrentBlock( byte[] buffer, int offset, int count )
		{
			// MD 7/20/2007 - BR25039
			// In some situations, we will write the TXO record in a full block and prevent further 
			// writing to the record
			if ( this.allowWriteForTXORecord == false )
			{
				Debug.Assert( this.RecordType == BIFF8RecordType.TXO, "allowWriteForTXORecord should only be set to false for TXO records." );
                Utilities.DebugFail("Cannot write to TXO record anymore.");
				return count;
			}

			return base.WriteToCurrentBlock( buffer, offset, count );
		}

		#endregion WriteToCurrentBlock 

		#endregion Base Class Overrides

		#region Methods

		// MD 9/2/08 - Cell Comments
		// Now that all shape text data is processed for BIFF8, no special processing needs to be done for TXO records.
		#region Not Used

		//// MD 7/20/2007 - BR25039
		//// Do special processing to append CONTINUE blocks to the TXO record
		//#region AppendContinueBlocksForTXORecord
		//
		//private void AppendContinueBlocksForTXORecord()
		//{
		//    // Make sure the first block of the TXO record is 18 bytes
		//    Debug.Assert( this.Length == 18, "The TXO record has the wrong length." );
		//
		//    // The length fields begin a index 10, so advance the stream to that position and read 4 bytes
		//    this.Position = 10;
		//    byte[] data = this.ReadBytes( 4 );
		//    this.Position = 0;
		//
		//    // The first 2 bytes are the length (in characters) of the text
		//    int lengthOfText = BitConverter.ToUInt16( data, 0 );
		//
		//    // If there is no text, no CONTINUE blocks occur after the TXO block
		//    if ( lengthOfText == 0 )
		//        return;
		//
		//    // The second 2 bytes are the count of bytes in the formatting runs
		//    int lengthOfFormattingRuns = BitConverter.ToUInt16( data, 2 );
		//
		//    // Move the workbook stream to the end of this stream
		//    this.OwnerStream.Position = this.EndUnderlyingStreamPosition;
		//
		//    #region Read Text Blocks
		//
		//    // Read the next record type
		//    BIFF8RecordType nextRecordType = (BIFF8RecordType)this.OwnerStreamReader.ReadUInt16();
		//
		//    if ( nextRecordType != BIFF8RecordType.CONTINUE )
		//    {
        //        Utilities.DebugFail( "The next record should have been a continue recrod." );
		//        return;
		//    }
		//
		//    // Read the length of the continuation record
		//    ushort nextRecordLength = this.OwnerStreamReader.ReadUInt16();
		//
		//    // Increase the length of the record stream
		//    this.LengthInternal += nextRecordLength;
		//
		//    // Add a block info for the CONTINUE block
		//    this.Blocks.Add( this.CreateRecordBlock( this.OwnerStream.Position, nextRecordLength ) );
		//
		//    this.Position = 18;
		//
		//    // While reading the text's CONTINUE block, set the readingTextForTXO flag to true so if the
		//    // text takes up more bytes than the max for a block, we will automatically append the next 
		//    // CONTINUE block to the record stream.
		//    this.readingTextForTXORecord = true;
		//    FormattedString text = this.ReadFormattedString( (ushort)lengthOfText );
		//    this.readingTextForTXORecord = false;
		//
		//    #endregion Read Text Blocks
		//
		//    #region Append Formatting Runs Blocks
		//
		//    while ( lengthOfFormattingRuns > 0 )
		//    {
		//        // Read the next record type
		//        nextRecordType = (BIFF8RecordType)this.OwnerStreamReader.ReadUInt16();
		//
		//        if ( nextRecordType != BIFF8RecordType.CONTINUE )
		//        {
        //            Utilities.DebugFail( "The next record should have been a continue recrod." );
		//            return;
		//        }
		//
		//        // Read the length of the continuation record
		//        nextRecordLength = this.OwnerStreamReader.ReadUInt16();
		//
		//        // Decrease the amount of space left for the formatting runs
		//        lengthOfFormattingRuns -= nextRecordLength;
		//
		//        // Increase the length of the record stream
		//        this.LengthInternal += nextRecordLength;
		//
		//        // Add a block info for the CONTINUE block
		//        this.Blocks.Add( this.CreateRecordBlock( this.OwnerStream.Position, nextRecordLength ) );
		//
		//        // If we will be making another pass to read more formatting runs, 
		//        // advance the positon of the stream to the next block header
		//        if ( lengthOfFormattingRuns > 0 )
		//            this.OwnerStream.Seek( nextRecordLength, SeekOrigin.Current );
		//    }
		//
		//    Debug.Assert( lengthOfFormattingRuns == 0 );
		//
		//    #endregion Append Formatting Runs Blocks
		//
		//    // Move the stream back the beginning
		//    this.Position = 0;
		//}
		//
		//#endregion AppendContinueBlocksForTXORecord
		//
		//#region WriteEntireTXORecord
		//
		//public void WriteEntireTXORecord( byte[] data )
		//{
		//    // The length of the record should be 0 because this will be the only data written to this record
		//    Debug.Assert( this.Length == 0 && this.RecordType == BIFF8RecordType.TXO );
		//
		//    // Determine the length of the text and formatting runs
		//    int lengthOfText = BitConverter.ToUInt16( data, 10 );
		//    int lengthOfFormattedRuns = BitConverter.ToUInt16( data, 12 );
		//
		//    // Clear the record blocks for this record
		//    this.Blocks.Clear();
		//
		//    // Add a block to hold the TXO header, which is always 18 bytes
		//    // MD 7/30/08 - BR35175
		//    // Since we are manually creating the blocks, we should be setting their lengths
		//    //this.Blocks.Add( new RecordBlockInfo( this.OwnerStream.Position, 0, 18 ) );
		//    this.Blocks.Add( new RecordBlockInfo( this.OwnerStream.Position, 18, 18 ) );
		//
		//    // If there is at least one character, we need to wrtie at least two CONTINUE blocks for the 
		//    // text and formatting runs
		//    if ( lengthOfText > 0 )
		//    {
		//        // Determine the amount of bytes needed to write the text portion
		//        int textDataLength = data.Length - ( 18 + lengthOfFormattedRuns );
		//
		//        // Determine the byte containing the header for the string section in the next CONTINUE block
		//        int byteContainingStringHeader = 18;
		//
		//        // Keep adding CONTINUE blocks until there is no data left
		//        while ( textDataLength > 0 )
		//        {
		//            int blockLength;
		//
		//            // If the remaining text data is less than the max size for a block, just use the remaining 
		//            // size as the block size
		//            if ( textDataLength <= this.MaximumBlockSize )
		//            {
		//                blockLength = textDataLength;
		//            }
		//            else
		//            {
		//                // Get the string header byte
		//                byte stringHeader = data[ byteContainingStringHeader ];
		//
		//                // Determine from the header whether the bytes are compressed or are stored as unicode
		//                bool charCompression = ( stringHeader & 0x01 ) == 0x00;
		//
		//                // If char compression takes place, each char is one byte, so it the data can fill the max block size
		//                // Otherwise, the chars are two bytes each, and counting the one byte string header, only an odd number
		//                // of bytes can fit in the block (the char cannot be split across blocks)
		//                if ( charCompression )
		//                    blockLength = this.MaximumBlockSize;
		//                else
		//                    blockLength = this.MaximumBlockSize - 1;
		//            }
		//
		//            // Add the block to the record
		//            // MD 7/30/08 - BR35175
		//            // Since we are manually creating the blocks, we should be setting their lengths
		//            // Also, the specified block start we were passing in was incorrect.
		//            //this.Blocks.Add( new RecordBlockInfo( this.Blocks[ this.Blocks.Count - 1 ].BlockEnd, 0, blockLength ) );
		//            this.Blocks.Add( new RecordBlockInfo( this.Blocks[ this.Blocks.Count - 1 ].BlockEnd + 4, blockLength, blockLength ) );
		//
		//            // Decrease the remaining text data length
		//            textDataLength -= blockLength;
		//
		//            // Get the position of the next string header byte
		//            byteContainingStringHeader += blockLength;
		//        }
		//
		//        while ( lengthOfFormattedRuns > 0 )
		//        {
		//            int blockLength = Math.Min( lengthOfFormattedRuns, this.MaximumBlockSize );
		//
		//            // Add the blocks for the formatting runs to the record
		//            // MD 7/30/08 - BR35175
		//            // Since we are manually creating the blocks, we should be setting their lengths
		//            // Also, the specified block start we were passing in was incorrect.
		//            //this.Blocks.Add( new RecordBlockInfo( this.Blocks[ this.Blocks.Count - 1 ].BlockEnd, 0, (ushort)blockLength ) );
		//            this.Blocks.Add( new RecordBlockInfo( this.Blocks[ this.Blocks.Count - 1 ].BlockEnd + 4, blockLength, (ushort)blockLength ) );
		//
		//            lengthOfFormattedRuns -= blockLength;
		//        }
		//
		//        Debug.Assert( lengthOfFormattedRuns == 0 );
		//    }
		//
		//    // Write headers in the workbook stream for each block
		//    for ( int i = 0; i < this.Blocks.Count; i++ )
		//    {
		//        RecordBlockInfo block = this.Blocks[ i ];
		//
		//        BIFF8RecordType type = i == 0 ? BIFF8RecordType.TXO : BIFF8RecordType.CONTINUE;
		//
		//        // MD 6/20/08 - BR34113
		//        // This is writing the block header at the wrong position. It is writing it to the start of the record data
		//        // not the record header. Move back to before the record header.
		//        //this.OwnerStream.Position = block.BlockStart;
		//        this.OwnerStream.Position = block.BlockStart - 4;
		//
		//        // Write the continuation record type
		//        this.WriteBlockType( this.OwnerStream, (int)type );
		//
		//        // MD 7/30/08 - BR35175
		//        // The comment below is invalid: we now have the lengths of the blocks, we so can just write out their real lengths.
		//        //// Write a length of 0 for now
		//        //this.WriteBlockLength( this.OwnerStream, 0, false );
		//        this.WriteBlockLength( this.OwnerStream, block.BlockLength, false );
		//    }
		//
		//    // Set the record length and write all data
		//    // MD 7/30/08 - BR35175
		//    // The fix for BR34113 had the right idea: to actually write out the data. However, the call to SetLength in the normal Write
		//    // method will actually add additional block on, so we have to manually set the length of the record and write out to the blocks.
		//    //// MD 6/20/08 - BR34113
		//    //// I'm not sure why I did this, but this is totally incorrect. The length will be set automatically when new data is written
		//    //// to the stream. Not only that, but it will also set the block lengths of the various blocks in the record. If we set the
		//    //// length here, the block lenghts never get set and when this stream is disposed, the stream position moves back to the beginning
		//    //// of this record and the record is overwritten later. Just make the call to Write.
		//    ////this.LengthInternal = data.Length;
		//    //this.Write( data, 0, data.Length );
		//    this.LengthInternal = data.Length;
		//
		//    // Manually write out to the blocks
		//    int bytesWritten = 0;
		//    while ( bytesWritten < data.Length )
		//        bytesWritten += this.WriteToCurrentBlock( data, bytesWritten, data.Length - bytesWritten );
		//
		//    // Don't allow writing for the TXO record anymore
		//    this.allowWriteForTXORecord = false;
		//}
		//
		//#endregion WriteEntireTXORecord  

		#endregion Not Used

		// MD 3/9/12 - TFS104455
		#region ExtractFixedValue

		private static bool ExtractFixedValue(CustomFilterCondition condition, FixedValuesFilter fixedValuesFilter)
		{
			if (condition != null)
			{
				if (condition.ComparisonOperator != ExcelComparisonOperator.Equals || condition.HasWildcards)
					return false;

				string stringValue = condition.Value as string;
				if (stringValue == null)
					return false;

				if (stringValue.Length == 0)
					fixedValuesFilter.IncludeBlanks = true;
				else
					fixedValuesFilter.DisplayValues.Add(Utilities.UnescapeWildcards(stringValue));
			}

			return true;
		}

		#endregion // ExtractFixedValue

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region GetColorInfoFromRawValues

		private WorkbookColorInfo GetColorInfoFromRawValues(XColorType colorType, uint xClrValue, double? tint)
		{
			switch (colorType)
			{
				case XColorType.Auto:
					Debug.Assert(tint == null, "nTintShade must be zero.");
					Debug.Assert(xClrValue == 0, "xClrValue must be zero.");
					return WorkbookColorInfo.Automatic;

				case XColorType.Indexed:
					return new WorkbookColorInfo(this.Manager.Workbook.Palette.GetColorAtAbsoluteIndex((int)xClrValue), null, tint, true);

				case XColorType.RGB:
					Color rgbColor = Utilities.DecodeLongRGBA(xClrValue);
					return new WorkbookColorInfo(rgbColor, null, tint, true);

				case XColorType.Themed:
					WorkbookThemeColorType themeColorType = (WorkbookThemeColorType)xClrValue;
					Debug.Assert(Enum.IsDefined(typeof(WorkbookThemeColorType), themeColorType), "This is not a valid WorkbookThemeColorType.");
					return new WorkbookColorInfo(null, themeColorType, tint, true);


				case XColorType.NotSet:
					Debug.Assert(xClrValue == 0, "xClrValue must be zero.");
					return null;

				default:
					Utilities.DebugFail("Unknown XColorType: " + colorType);
					return null;
			}
		}

		#endregion // GetColorInfoFromRawValues

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region GetRawColorInfoValues

		private void GetRawColorInfoValues(WorkbookColorInfo colorInfo, ColorableItem item, out XColorType colorType, out double tint, out uint xClrValue)
		{
			tint = 0;
			xClrValue = 0;

			if (colorInfo == null)
			{
				colorType = XColorType.NotSet;
			}
			else
			{
				if (colorInfo.Tint.HasValue)
					tint = colorInfo.Tint.Value;

				if (colorInfo.IsAutomatic)
				{
					colorType = XColorType.Auto;
				}
				else if (colorInfo.Color.HasValue)
				{
					Color color = colorInfo.Color.Value;
					if (Utilities.ColorIsSystem(color))
					{
						colorType = XColorType.Indexed;
						xClrValue = (uint)colorInfo.GetIndex(this.Manager.Workbook, item);
					}
					else
					{
						colorType = XColorType.RGB;
						xClrValue = Utilities.EncodeLongRGBA(colorInfo.Color.Value);
					}
				}
				else if (colorInfo.ThemeColorType.HasValue)
				{
					colorType = XColorType.Themed;
					xClrValue = (uint)colorInfo.ThemeColorType.Value;
				}
				else
				{
					Utilities.DebugFail("This is unexpected.");
					colorType = XColorType.NotSet;
				}
			}
		}

		#endregion // GetRawColorInfoValues

		// MD 2/22/12 - 12.1 - Table Support
		#region InterpretAFDOper

		private static CustomFilterCondition InterpretAFDOper(AFDOper doper)
		{
			switch (doper.vt)
			{
				case AFDOper.VTUndefined:
					break;

				case AFDOper.VTRk:
				case AFDOper.VTDouble:
				case AFDOper.VTString:
				case AFDOper.VTBoolErr:
					return CustomFilterCondition.CreateCustomFilterCondition(doper.grbitSign, doper.ResolvedValue);

				case AFDOper.VTBlanks:
					return CustomFilterCondition.CreateCustomFilterCondition(ST_FilterOperator.equal, string.Empty);

				case AFDOper.VTNonBlanks:
					return CustomFilterCondition.CreateCustomFilterCondition(ST_FilterOperator.notEqual, string.Empty);

				default:
					Utilities.DebugFail("Unknown AFDOper.vt value.");
					break;
			}

			return null;
		}

		#endregion // InterpretAFDOper

		// MD 9/25/08 - TFS8010
		#region IsAllTXODataAppended

		private bool IsAllTXODataAppended()
		{
			long originalOwnerPosition = this.OwnerStream.Position;
			long originalPosition = this.Position;

			try
			{
				this.Position = 10;

				// If the string length is 0, there are no CONTINUE records, all TXO data is present, so return true.
				ushort stringLength = this.ReadUInt16();
				if ( stringLength == 0 )
					return true;

				// If there is text present, there must be a TXO record with at least two CONTINUE records after it, so return false 
				// if there are less than 3 blocks.
				if ( this.Blocks.Count < 3 )
					return false;

				ushort lengthOfFormattingRuns = this.ReadUInt16();

				this.Position = 18;

				// See if the entire string can be read in. If not, we need more CONTINUE records to be appended.
				// MD 11/3/10 - TFS49093
				// The formatted string data is now stored on the FormattedStringElement.
				//FormattedString value;
				StringElement value;
				if ( this.TryReadFormattedString( stringLength, out value ) == false )
					return false;

				// If the string could be read in, make sure the string length was valid.
				Debug.Assert( value.UnformattedString.Length == stringLength, "The string in the TXO record does not have the correct length." );

				// Make sure there is enough room for the formatting runs as well. If not, more CONTINUE records must be appended.
				if ( lengthOfFormattingRuns > this.Length - this.Position )
					return false;

				Debug.Assert( lengthOfFormattingRuns == this.Length - this.Position, "There should not be any other data in the TXO record." );
				return true;
			}
			finally
			{
				this.Position = originalPosition;
				this.OwnerStream.Position = originalOwnerPosition;
			}
		} 

		#endregion IsAllTXODataAppended

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAF12CellIcon

		// http://msdn.microsoft.com/en-us/library/dd906415(v=office.12).aspx
		public void ReadAF12CellIcon(out ST_IconSetType? iconset, out uint iconIndex)
		{
			uint iIconSet = this.ReadUInt32();
			iconIndex = this.ReadUInt32();

			if (iIconSet == 0xFFFFFFFF)
				iconset = null;
			else
				iconset = (ST_IconSetType)iIconSet;
		}

		// http://msdn.microsoft.com/en-us/library/dd906415(v=office.12).aspx
		public void WriteAF12CellIcon(ST_IconSetType iconset, uint iconIndex)
		{
			this.Write((uint)iconset); // iIconSet
			this.Write(iconIndex);
		}

		#endregion // Read/WriteAF12CellIcon

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAF12Criteria

		// http://msdn.microsoft.com/en-us/library/dd924965(v=office.12).aspx
		public AFDOper ReadAF12Criteria(Worksheet worksheet)
		{
			this.ReadFrtRefHeader(worksheet, BIFF8RecordType.CONTINUEFRT12);

			AFDOper doper = this.ReadAFDOper(true);
			if (doper.vt == AFDOper.VTString)
				doper.stringValue = this.ReadXLUnicodeStringNoCch((byte)doper.vtValue);

			return doper;
		}

		// http://msdn.microsoft.com/en-us/library/dd924965(v=office.12).aspx
		public void WriteAF12Criteria(WorksheetRegion filterRegion, AFDOper doper)
		{
			this.WriteFrtRefHeader(filterRegion, BIFF8RecordType.CONTINUEFRT12);
			this.WriteAFDOper(doper, true);

			if (doper.stringValue != null)
				this.WriteXLUnicodeStringNoCch(doper.stringValue);
		}

		#endregion // Read/WriteAF12Criteria

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAF12DateInfo

		// http://msdn.microsoft.com/en-us/library/dd923785(v=office.12).aspx
		public FixedDateGroup ReadAF12DateInfo(Worksheet worksheet)
		{
			this.ReadFrtRefHeader(worksheet, BIFF8RecordType.CONTINUEFRT12);

			ushort year = this.ReadUInt16();
			ushort month = this.ReadUInt16();
			uint day = this.ReadUInt32();
			ushort hour = this.ReadUInt16();
			ushort minute = this.ReadUInt16();
			ushort second = this.ReadUInt16();
			ushort unused1 = this.ReadUInt16();
			uint reserved1 = this.ReadUInt32();
			Debug.Assert(reserved1 == 0, "The reserved1 value is incorrect.");
			uint nodeType = this.ReadUInt32();

			return FixedDateGroup.CreateFixedDateGroup((ST_DateTimeGrouping)nodeType, year, month, (ushort)day, hour, minute, second);
		}

		// http://msdn.microsoft.com/en-us/library/dd923785(v=office.12).aspx
		public void WriteAF12DateInfo(WorksheetRegion filterRegion, FixedDateGroup fixedDateGroup)
		{
			this.WriteFrtRefHeader(filterRegion, BIFF8RecordType.CONTINUEFRT12);

			this.Write((ushort)fixedDateGroup.Value.Year);
			this.Write((ushort)fixedDateGroup.Value.Month);
			this.Write((uint)fixedDateGroup.Value.Day);
			this.Write((ushort)fixedDateGroup.Value.Hour);
			this.Write((ushort)fixedDateGroup.Value.Minute);
			this.Write((ushort)fixedDateGroup.Value.Second);
			this.Write((ushort)0); // unused1
			this.Write((uint)0); // reserved1
			this.Write((uint)fixedDateGroup.DateTimeGrouping); // nodeType
		}

		#endregion // Read/WriteAF12DateInfo

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAFDOper

		// http://msdn.microsoft.com/en-us/library/dd905650(v=office.12).aspx
		private AFDOper ReadAFDOper(bool isInAutoFilter12Record)
		{
			AFDOper afdoper = new AFDOper();
			afdoper.vt = (byte)this.ReadByte();
			afdoper.grbitSign = (ST_FilterOperator)this.ReadByte();

			switch (afdoper.vt)
			{
				case AFDOper.VTUndefined:
				case AFDOper.VTBlanks:
				case AFDOper.VTNonBlanks:
					this.ReadBytes(8);
					break;

				case AFDOper.VTRk:
					Debug.Assert(isInAutoFilter12Record == false, "This value cannot appear in AutoFilter12 records.");
					afdoper.vtValue = this.ReadAFDOperRk();
					break;

				case AFDOper.VTDouble:
					afdoper.vtValue = this.ReadXnum();
					break;

				case AFDOper.VTString:
					afdoper.vtValue = this.ReadAFDOperStr(isInAutoFilter12Record);
					break;

				case AFDOper.VTBoolErr:
					afdoper.vtValue = this.ReadAFDOperBoolErr();
					break;

				default:
					Utilities.DebugFail("Unknown AFDOper.vt value.");
					break;
			}

			return afdoper;
		}

		public void WriteAFDOper(string value, bool isInAutoFilter12Record)
		{
			AFDOper afdoper = new AFDOper(value);
			this.WriteAFDOper(afdoper, isInAutoFilter12Record);
		}

		// http://msdn.microsoft.com/en-us/library/dd905650(v=office.12).aspx
		private void WriteAFDOper(AFDOper afdoper, bool isInAutoFilter12Record)
		{
			this.Write(afdoper.vt);
			this.Write((byte)afdoper.grbitSign);

			switch (afdoper.vt)
			{
				case AFDOper.VTUndefined:
				case AFDOper.VTBlanks:
				case AFDOper.VTNonBlanks:
					this.Write(new byte[8]);
					break;

				// This is not currently used.
				//case AFDOper.VTRk:
				//    Debug.Assert(isInAutoFilter12Record == false, "This value cannot appear in AutoFilter12 records.");
				//    this.WriteAFDOperRk((double)afdoper.vtValue);
				//    break;

				case AFDOper.VTDouble:
					this.WriteXnum((double)afdoper.vtValue);
					break;

				case AFDOper.VTString:
					this.WriteAFDOperStr((byte)afdoper.vtValue, afdoper.HasWildcards, isInAutoFilter12Record);
					break;

				case AFDOper.VTBoolErr:
					this.WriteAFDOperBoolErr(afdoper.vtValue);
					break;

				default:
					Utilities.DebugFail("Unknown AFDOper.vt value.");
					break;
			}
		}

		#endregion // Read/WriteAFDOper

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAFDOperBoolErr

		// http://msdn.microsoft.com/en-us/library/dd952147(v=office.12).aspx
		public object ReadAFDOperBoolErr()
		{
			object bes = this.ReadBes();
			ushort unused1 = this.ReadUInt16();
			uint unused2 = this.ReadUInt32();
			return bes;
		}

		// http://msdn.microsoft.com/en-us/library/dd952147(v=office.12).aspx
		public void WriteAFDOperBoolErr(object value)
		{
			this.WriteBes(value);
			this.Write((ushort)0);
			this.Write((uint)0);
		}

		#endregion // Read/WriteAFDOperBoolErr

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAFDOperRk

		// http://msdn.microsoft.com/en-us/library/dd923094(v=office.12).aspx
		public double ReadAFDOperRk()
		{
			uint rk = this.ReadUInt32();
			uint unused1 = this.ReadUInt32();
			return Utilities.DecodeRKValue(rk);
		}

		#endregion // Read/WriteAFDOperRk

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAFDOperStr

		// http://msdn.microsoft.com/en-us/library/dd905683(v=office.12).aspx
		private object ReadAFDOperStr(bool isInAutoFilter12Record)
		{
			if (isInAutoFilter12Record == false)
			{
				uint unused1 = this.ReadUInt32();
			}

			byte cch = (byte)this.ReadByte();
			byte fCompare = (byte)this.ReadByte();
			Debug.Assert(fCompare == 0 || fCompare == 1, "The fCompare value is incorrect.");
			byte reserved1 = (byte)this.ReadByte();
			Debug.Assert(reserved1 == 0, "The reserved1 value is incorrect.");
			byte unused2 = (byte)this.ReadByte();

			if (isInAutoFilter12Record)
			{
				uint unused3 = this.ReadUInt32();
			}

			return cch;
		}

		// http://msdn.microsoft.com/en-us/library/dd905683(v=office.12).aspx
		private void WriteAFDOperStr(byte cch, bool hasWildcards, bool isInAutoFilter12Record)
		{
			byte fCompare = (byte)(hasWildcards ? 0 : 1);

			if (isInAutoFilter12Record == false)
				this.Write((uint)0); // unused1

			this.Write(cch);
			this.Write(fCompare);
			this.Write((byte)0); // reserved1
			this.Write((byte)0); // unused2

			if (isInAutoFilter12Record)
				this.Write((uint)0); // unused3
		}

		#endregion // Read/WriteAFDOperStr

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteAutoFilter

		// http://msdn.microsoft.com/en-us/library/dd907578(v=office.12).aspx
		public Filter ReadAutoFilter(Worksheet worksheet, WorksheetTableColumn column)
		{
			ushort iEntry = this.ReadUInt16();

			ushort temp16 = this.ReadUInt16();
			ConditionalOperator wJoin = (ConditionalOperator)Utilities.GetBits(temp16, 0, 1);
			bool fSimple1 = Utilities.TestBit(temp16, 2);
			bool fSimple2 = Utilities.TestBit(temp16, 3);
			bool fTopN = Utilities.TestBit(temp16, 4);
			bool fTop = Utilities.TestBit(temp16, 5);
			bool fPercent = Utilities.TestBit(temp16, 6);
			int wTopN = Utilities.GetBits(temp16, 7, 15);

			AFDOper doper1 = this.ReadAFDOper(false);
			AFDOper doper2 = this.ReadAFDOper(false);

			if (fTopN)
			{
				TopOrBottomFilterType filterType = TopOrBottomFilter.GetFilterType(fPercent, fTop);

				double referenceValue = double.NaN;
				if (doper1.ResolvedValue is double)
					referenceValue = (double)doper1.ResolvedValue;

				TopOrBottomFilter top10Filter = new TopOrBottomFilter(column, filterType, wTopN, referenceValue);
				Debug.Assert(Double.IsNaN(referenceValue) || top10Filter.FilterOperator == doper1.grbitSign, "The doper1 structure was written out incorrectly.");
				return top10Filter;
			}

			if (doper1.vt == AFDOper.VTString)
			{
				doper1.stringValue = this.ReadXLUnicodeStringNoCch((byte)doper1.vtValue);
			}

			if (doper2.vt == AFDOper.VTString)
			{
				doper2.stringValue = this.ReadXLUnicodeStringNoCch((byte)doper2.vtValue);
			}

			CustomFilterCondition condition1 = Biff8RecordStream.InterpretAFDOper(doper1);
			CustomFilterCondition condition2 = Biff8RecordStream.InterpretAFDOper(doper2);

			if (condition1 != null || condition2 != null)
			{
				FixedValuesFilter fixedValuesFilter = new FixedValuesFilter(column);
				if (Biff8RecordStream.ExtractFixedValue(condition1, fixedValuesFilter) &&
					Biff8RecordStream.ExtractFixedValue(condition2, fixedValuesFilter))
				{
					int allowedValuesCount = fixedValuesFilter.GetAllowedItemCount();
					if (allowedValuesCount == 0)
						return null;

					// If there are multiple values with an or condition, or less than 2 values, and at least one cell matches the filter, 
					// we can return the fixed values filter. Otherwise, use a custom filter.
					if (allowedValuesCount == 1 || wJoin == ConditionalOperator.Or)
					{
						int dataAreaTopRowIndex;
						int dataAreaBottomRowIndex;
						column.Table.GetDataAreaRowIndexes(out dataAreaTopRowIndex, out dataAreaBottomRowIndex);

						bool foundMatchingCell = false;
						for (int rowIndex = dataAreaTopRowIndex; rowIndex <= dataAreaBottomRowIndex; rowIndex++)
						{
							WorksheetRow row = worksheet.Rows.GetIfCreated(rowIndex);
							if (fixedValuesFilter.MeetsCriteria(worksheet, row, column.WorksheetColumnIndex))
							{
								foundMatchingCell = true;
								break;
							}
						}

						if (foundMatchingCell)
							return fixedValuesFilter;
					}
				}
			}

			if (condition1 == null)
			{
				if (condition2 == null)
					return null;

				condition1 = condition2;
				condition2 = null;
			}

			return new CustomFilter(column, condition1, condition2, wJoin);
		}

		// http://msdn.microsoft.com/en-us/library/dd907578(v=office.12).aspx
		public void WriteAutoFilter(WorksheetTableColumn column, TableColumnFilterData filterData)
		{
			ConditionalOperator wJoin = ConditionalOperator.And;
			AFDOper doper1 = null;
			AFDOper doper2 = null;

			CustomFilter customFilter = column.Filter as CustomFilter;
			if (customFilter != null)
			{
				wJoin = customFilter.ConditionalOperator;
				doper1 = new AFDOper(this.Manager, customFilter.Condition1);
				doper2 = new AFDOper(this.Manager, customFilter.Condition2);
			}

			FixedValuesFilter fixedValuesFilter = column.Filter as FixedValuesFilter;

			if (fixedValuesFilter != null &&
				filterData.ShouldSaveIn2003Formats &&
				filterData.NeedsAUTOFILTER12Record == false)
			{
				if (fixedValuesFilter.IncludeBlanks)
				{
					AFDOper blankDoper = new AFDOper();
					blankDoper.vt = AFDOper.VTBlanks;

					if (filterData.AllowedTextValues != null && filterData.AllowedTextValues.Count != 0)
					{
						wJoin = ConditionalOperator.Or;
						doper1 = new AFDOper(Utilities.EscapeWildcards(filterData.AllowedTextValues[0]));
						doper2 = blankDoper;
					}
					else
					{
						doper1 = blankDoper;
					}
				}
				else if (filterData.AllowedTextValues != null && filterData.AllowedTextValues.Count != 0)
				{
					doper1 = new AFDOper(Utilities.EscapeWildcards(filterData.AllowedTextValues[0]));

					if (filterData.AllowedTextValues.Count != 1)
					{
						wJoin = ConditionalOperator.Or;
						doper2 = new AFDOper(Utilities.EscapeWildcards(filterData.AllowedTextValues[1]));
					}
				}
			}

			bool fTopN = false;
			bool fTop = false;
			bool fPercent = false;
			int wTopN = 0;
			TopOrBottomFilter topOrBottomFilter = column.Filter as TopOrBottomFilter;
			if (topOrBottomFilter != null)
			{
				fTopN = true;
				fTop = topOrBottomFilter.IsTop;
				fPercent = topOrBottomFilter.IsPercent;
				wTopN = topOrBottomFilter.Value;

				if (Double.IsNaN(topOrBottomFilter.ReferenceValue) == false)
					doper1 = new AFDOper(topOrBottomFilter.ReferenceValue, topOrBottomFilter.FilterOperator);
			}

			if (doper1 == null)
				doper1 = new AFDOper();

			if (doper2 == null)
				doper2 = new AFDOper();

			// The iEntry is always 0 for columns because this record is embedded in the column data.
			this.Write((ushort)0); // iEntry

			ushort temp16 = 0;
			Utilities.AddBits(ref temp16, (int)wJoin, 0, 1);
			Utilities.SetBit(ref temp16, fTopN, 4);
			Utilities.SetBit(ref temp16, fTop, 5);
			Utilities.SetBit(ref temp16, fPercent, 6);
			Utilities.AddBits(ref temp16, wTopN, 7, 15);
			this.Write(temp16);

			this.WriteAFDOper(doper1, false);
			this.WriteAFDOper(doper2, false);

			string str1 = doper1.ResolvedValue as string;
			if (str1 != null)
				this.WriteXLUnicodeStringNoCch(str1);

			string str2 = doper2.ResolvedValue as string;
			if (str2 != null)
				this.WriteXLUnicodeStringNoCch(str2);
		}

		#endregion // Read/WriteAutoFilter

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteBes

		// http://msdn.microsoft.com/en-us/library/dd906734(v=office.12).aspx
		public object ReadBes()
		{
			byte bBoolErr = (byte)this.ReadByte();
			byte fError = (byte)this.ReadByte();
			Debug.Assert(fError == 0 || fError == 1, "The fError value is incorrect.");

			if (fError == 0)
			{
				Debug.Assert(bBoolErr == 0 || bBoolErr == 1, "The bBoolErr value is incorrect.");
				return bBoolErr != 0;
			}

			return ErrorValue.FromValue(bBoolErr);
		}

		// http://msdn.microsoft.com/en-us/library/dd906734(v=office.12).aspx
		public void WriteBes(object value)
		{
			byte bBoolErr;
			byte fError;

			if (value is bool)
			{
				bBoolErr = (bool)value ? (byte)1 : (byte)0;
				fError = 0;
			}
			else
			{
				bBoolErr = ((ErrorValue)value).Value;
				fError = 1;
			}

			this.Write(bBoolErr);
			this.Write(fError);
		}

		#endregion // Read/WriteBes

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteCachedDiskHeader

		// http://msdn.microsoft.com/en-us/library/dd923428(v=office.12).aspx
		private void ReadCachedDiskHeader(WorksheetTableColumn column, bool fSaveStyleName)
		{
			uint cbdxfHdrDisk = this.ReadUInt32();

			WorksheetCellFormatData rgHdrDisk = null;
			if (cbdxfHdrDisk != 0)
				rgHdrDisk = this.ReadDXFN12List((int)cbdxfHdrDisk);

			WorkbookStyle headerStyle = null;
			if (fSaveStyleName)
			{
				string strStyleName = this.ReadXLUnicodeString();
				headerStyle = this.Manager.Workbook.Styles[strStyleName];
				Debug.Assert(headerStyle != null, "We should have a style here.");
			}

			if (rgHdrDisk != null || headerStyle != null)
			{
				this.Manager.CombineDxfInfo(column.AreaFormats, WorksheetTableColumnArea.HeaderCell, WorksheetTableColumn.CanAreaFormatValueBeSet, 
					headerStyle, rgHdrDisk);
			}
		}

		// http://msdn.microsoft.com/en-us/library/dd923428(v=office.12).aspx
		private void WriteCachedDiskHeader(WorksheetTableColumn column, WorkbookStyle dxfHeaderStyle, WorksheetCellFormatData dxfHeader)
		{
			long cbdxfHdrDiskPosition = this.Position;
			this.Write((uint)0); // cbdxfHdrDiskPosition placeholder

			this.WriteDXFN12ListAndSizeField(cbdxfHdrDiskPosition, dxfHeader);
			if (dxfHeaderStyle != null)
				this.WriteXLUnicodeString(dxfHeaderStyle.Name);
		}

		#endregion // Read/WriteCachedDiskHeader

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region Read/WriteCFColor

		// http://msdn.microsoft.com/en-us/library/dd953661(v=office.12).aspx
		public WorkbookColorInfo ReadCFColor()
		{
			XColorType xclrType = (XColorType)this.ReadUInt32();
			uint xClrValue = this.ReadUInt32();
			double numTint = this.ReadDouble();

			double? tint = null;
			if (numTint != 0)
				tint = numTint;

			return this.GetColorInfoFromRawValues(xclrType, xClrValue, tint);
		}

		// http://msdn.microsoft.com/en-us/library/dd953661(v=office.12).aspx
		public void WriteCFColor(WorkbookColorInfo colorInfo, ColorableItem item)
		{
			XColorType colorType;
			double tint;
			uint xClrValue;
			this.GetRawColorInfoValues(colorInfo, item, out colorType, out tint, out xClrValue);

			this.Write((uint)colorType);
			this.Write(xClrValue);
			this.Write(tint);
		}

		#endregion // Read/WriteCFColor

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteCFFlag

		// http://msdn.microsoft.com/en-us/library/dd906415(v=office.12).aspx
		public void ReadCFFlag(out ST_IconSetType? iconset, out uint iconIndex)
		{
			uint iIconSet = this.ReadUInt32();
			iconIndex = this.ReadUInt32();

			if (iIconSet == 0xFFFFFFFF)
				iconset = null;
			else
				iconset = (ST_IconSetType)iIconSet;
		}

		// http://msdn.microsoft.com/en-us/library/dd906415(v=office.12).aspx
		public void WriteCFFlag(ST_IconSetType iconset, uint iconIndex)
		{
			this.Write((uint)iconset); // iIconSet
			this.Write(iconIndex);
		}

		#endregion // Read/WriteAF12CellIcon

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteCondDataValue

		// http://msdn.microsoft.com/en-us/library/dd924784(v=office.12).aspx
		public SortCondition ReadCondDataValue(ST_SortBy sortOn, SortDirection sortDirection)
		{
			uint condDataValue = this.ReadUInt32();

			SortCondition sortCondition;
			switch (sortOn)
			{
				case ST_SortBy.value:
					Debug.Assert(condDataValue == 0, "The condDataValue value is incorrect.");
					sortCondition = new OrderedSortCondition(sortDirection);
					break;

				case ST_SortBy.cellColor:
					sortCondition = FillSortCondition.CreateFillSortCondition(this.Manager, condDataValue, sortDirection);
					break;

				case ST_SortBy.fontColor:
					sortCondition = FontColorSortCondition.CreateFontColorSortCondition(this.Manager, condDataValue, sortDirection);
					break;

				default:
					Utilities.DebugFail("Unknown sortOn value: " + sortOn);
					sortCondition = null;
					break;
			}

			uint reserved = this.ReadUInt32();
			Debug.Assert(reserved == 0, "The reserved value is incorrect.");

			return sortCondition;
		}

		// http://msdn.microsoft.com/en-us/library/dd924784(v=office.12).aspx
		public void WriteCondDataValue(WorksheetTableColumn column, SortCondition sortCondition)
		{
			uint condDataValue = 0;

			IColorSortCondition colorSortCondition = sortCondition as IColorSortCondition;
			if (colorSortCondition != null)
				condDataValue = (uint)column.SortConditionDxfIdDuringSave;

			this.Write(condDataValue);
			this.Write((uint)0); // reserved
		}

		#endregion // Read/WriteCondDataValue

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFALC

		// http://msdn.microsoft.com/en-us/library/dd947924(v=office.12).aspx
		public void ReadDXFALC(WorksheetCellFormatData format,
			bool alchNinch, bool alcvNinch, bool wrapNinch, bool trotNinch,
			bool kintoNinch, bool cIndentNinch, bool fShrinkNinch, bool fMergeCellNinch,
			bool iReadingOrderNinch)
		{
			uint temp32 = this.ReadUInt32();
			HorizontalCellAlignment alc = (HorizontalCellAlignment)Utilities.GetBits(temp32, 0, 2);
			bool fWrap = Utilities.TestBit(temp32, 3);
			VerticalCellAlignment alcv = (VerticalCellAlignment)Utilities.GetBits(temp32, 4, 6);
			bool fJustLast = Utilities.TestBit(temp32, 7);
			Debug.Assert(fJustLast == false || alc == HorizontalCellAlignment.Distributed, "The fJustLast value is incorrect.");
			int trot = Utilities.GetBits(temp32, 8, 15);
			int cIndent = Utilities.GetBits(temp32, 16, 19);
			bool fShrinkToFit = Utilities.TestBit(temp32, 20);
			bool fMergedCell = Utilities.TestBit(temp32, 21);
			int iReadingOrder = Utilities.GetBits(temp32, 22, 23);

			int iIndent = this.ReadInt32();
			Debug.Assert(-15 <= iIndent && iIndent <= 255, "The iIndent value is incorrect");

			if (alchNinch == false)
				format.Alignment = alc;

			if (alcvNinch == false)
				format.VerticalAlignment = alcv;

			if (wrapNinch == false)
				format.WrapText = Utilities.ToDefaultableBoolean(fWrap);

			if (trotNinch == false)
				format.Rotation = trot;

			if (kintoNinch == false && fJustLast)
				format.AddIndent = true;

			if (cIndentNinch == false)
			{
				format.Indent = cIndent;

				if (iIndent != 0xFF)
					format.Indent += iIndent;
			}

			if (fShrinkNinch == false)
				format.ShrinkToFit = Utilities.ToDefaultableBoolean(fShrinkToFit);

			if (fMergeCellNinch == false)
			{
				
			}

			if (iReadingOrderNinch == false)
			{
				
			}
		}

		// http://msdn.microsoft.com/en-us/library/dd947924(v=office.12).aspx
		public void WriteDXFALC(WorksheetCellFormatData format,
			bool alchNinch, bool alcvNinch, bool wrapNinch, bool trotNinch,
			bool kintoNinch, bool cIndentNinch, bool fShrinkNinch, bool fMergeCellNinch,
			bool iReadingOrderNinch)
		{
			HorizontalCellAlignment alc = HorizontalCellAlignment.General;
			bool fWrap = false;
			VerticalCellAlignment alcv = VerticalCellAlignment.Bottom;
			bool fJustLast = false;
			int trot = 0;
			int cIndent = 0;
			bool fShrinkToFit = false;
			bool fMergedCell = false;
			int iReadingOrder = 0;

			int iIndent = 0xFF;

			if (alchNinch == false)
				alc = format.AlignmentResolved;

			if (alcvNinch == false)
				alcv = format.VerticalAlignmentResolved;

			if (wrapNinch == false)
				fWrap = format.WrapTextResolved == ExcelDefaultableBoolean.True;

			if (trotNinch == false)
				trot = format.RotationResolved;

			if (cIndentNinch == false)
			{
				int indentResolved = format.IndentResolved;
				cIndent = Math.Min(WorksheetCellFormatData.MaxIndent2003, indentResolved);

				if (WorksheetCellFormatData.MaxIndent2003 < indentResolved)
					iIndent = indentResolved - cIndent;
			}

			if (fShrinkNinch == false)
				fShrinkToFit = format.ShrinkToFitResolved == ExcelDefaultableBoolean.True;

			uint temp32 = 0;
			Utilities.AddBits(ref temp32, (int)alc, 0, 2);
			Utilities.SetBit(ref temp32, fWrap, 3);
			Utilities.AddBits(ref temp32, (int)alcv, 4, 6);
			Utilities.SetBit(ref temp32, fJustLast, 7);
			Utilities.AddBits(ref temp32, trot, 8, 15);
			Utilities.AddBits(ref temp32, cIndent, 16, 19);
			Utilities.SetBit(ref temp32, fShrinkToFit, 20);
			Utilities.SetBit(ref temp32, fMergedCell, 21);
			Utilities.AddBits(ref temp32, iReadingOrder, 22, 23);
			this.Write(temp32);

			this.Write(iIndent);
		}

		#endregion // Read/WriteDXFALC

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFBdr

		public void ReadDXFBdr(WorksheetCellFormatData format,
			bool glLeftNinch, bool glRightNinch, bool glTopNinch, bool glBottomNinch, bool glDiagDownNinch, bool glDiagUpNinch)
		{
			uint temp32 = this.ReadUInt32();
			CellBorderLineStyle dgLeft = (CellBorderLineStyle)Utilities.GetBits(temp32, 0, 3);
			CellBorderLineStyle dgRight = (CellBorderLineStyle)Utilities.GetBits(temp32, 4, 7);
			CellBorderLineStyle dgTop = (CellBorderLineStyle)Utilities.GetBits(temp32, 8, 11);
			CellBorderLineStyle dgBottom = (CellBorderLineStyle)Utilities.GetBits(temp32, 12, 15);
			int icvLeft = Utilities.GetBits(temp32, 16, 22);
			int icvRight = Utilities.GetBits(temp32, 23, 29);
			bool bitDiagDown = Utilities.TestBit(temp32, 30);
			bool bitDiagUp = Utilities.TestBit(temp32, 31);

			temp32 = this.ReadUInt32();
			int icvTop = Utilities.GetBits(temp32, 0, 6);
			int icvBottom = Utilities.GetBits(temp32, 7, 13);
			int icvDiag = Utilities.GetBits(temp32, 14, 20);
			CellBorderLineStyle dgDiag = (CellBorderLineStyle)Utilities.GetBits(temp32, 21, 24);

			Workbook workbook = this.Manager.Workbook;

			if (glLeftNinch == false)
			{
				format.LeftBorderStyle = dgLeft;

				if (dgLeft != CellBorderLineStyle.None)
					format.LeftBorderColorInfo = new WorkbookColorInfo(workbook, icvLeft);
			}

			if (glRightNinch == false)
			{
				format.RightBorderStyle = dgRight;

				if (dgRight != CellBorderLineStyle.None)
					format.RightBorderColorInfo = new WorkbookColorInfo(workbook, icvRight);
			}

			if (glTopNinch == false)
			{
				format.TopBorderStyle = dgTop;

				if (dgTop != CellBorderLineStyle.None)
					format.TopBorderColorInfo = new WorkbookColorInfo(workbook, icvTop);
			}

			if (glBottomNinch == false)
			{
				format.BottomBorderStyle = dgBottom;

				if (dgBottom != CellBorderLineStyle.None)
					format.BottomBorderColorInfo = new WorkbookColorInfo(workbook, icvBottom);
			}

			DiagonalBorders diagonalBorders = DiagonalBorders.None;
			if (glDiagDownNinch == false && bitDiagDown)
				diagonalBorders |= DiagonalBorders.DiagonalDown;

			if (glDiagUpNinch == false && bitDiagUp)
				diagonalBorders |= DiagonalBorders.DiagonalUp;

			if (glDiagDownNinch == false || glDiagUpNinch == false)
			{
				format.DiagonalBorders = diagonalBorders;
				format.DiagonalBorderStyle = dgDiag;

				if (dgDiag != CellBorderLineStyle.None)
					format.DiagonalBorderColorInfo = new WorkbookColorInfo(workbook, icvDiag);
			}
		}

		public void WriteDXFBdr(WorksheetCellFormatData format,
			bool glLeftNinch, bool glRightNinch, bool glTopNinch, bool glBottomNinch, bool glDiagDownNinch, bool glDiagUpNinch)
		{
			CellBorderLineStyle dgLeft = CellBorderLineStyle.None;
			CellBorderLineStyle dgRight = CellBorderLineStyle.None;
			CellBorderLineStyle dgTop = CellBorderLineStyle.None;
			CellBorderLineStyle dgBottom = CellBorderLineStyle.None;
			int icvLeft = 0;
			int icvRight = 0;
			bool bitDiagDown = false;
			bool bitDiagUp = false;
			int icvTop = 0;
			int icvBottom = 0;
			int icvDiag = 0;
			CellBorderLineStyle dgDiag = CellBorderLineStyle.None;

			Workbook workbook = this.Manager.Workbook;

			if (glLeftNinch == false)
			{
				dgLeft = format.LeftBorderStyleResolved;

				if (dgLeft != CellBorderLineStyle.None)
					icvLeft = format.LeftBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}

			if (glRightNinch == false)
			{
				dgRight = format.RightBorderStyleResolved;

				if (dgRight != CellBorderLineStyle.None)
					icvRight = format.RightBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}

			if (glTopNinch == false)
			{
				dgTop = format.TopBorderStyleResolved;

				if (dgTop != CellBorderLineStyle.None)
					icvTop = format.TopBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}

			if (glBottomNinch == false)
			{
				dgBottom = format.BottomBorderStyleResolved;

				if (dgBottom != CellBorderLineStyle.None)
					icvBottom = format.BottomBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}

			if (glDiagDownNinch == false || glDiagUpNinch == false)
			{
				dgDiag = format.DiagonalBorderStyleResolved;

				if (dgDiag != CellBorderLineStyle.None)
					icvDiag = format.DiagonalBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);

				DiagonalBorders diagonalBorders = format.DiagonalBordersResolved;
				if (Utilities.TestFlag(diagonalBorders, DiagonalBorders.DiagonalDown))
					bitDiagDown = true;

				if (Utilities.TestFlag(diagonalBorders, DiagonalBorders.DiagonalUp))
					bitDiagUp = true;
			}

			uint temp32 = 0;
			Utilities.AddBits(ref temp32, (int)dgLeft, 0, 3);
			Utilities.AddBits(ref temp32, (int)dgRight, 4, 7);
			Utilities.AddBits(ref temp32, (int)dgTop, 8, 11);
			Utilities.AddBits(ref temp32, (int)dgBottom, 12, 15);
			Utilities.AddBits(ref temp32, icvLeft, 16, 22);
			Utilities.AddBits(ref temp32, icvRight, 23, 29);
			Utilities.SetBit(ref temp32, bitDiagDown, 30);
			Utilities.SetBit(ref temp32, bitDiagUp, 31);
			this.Write(temp32);

			temp32 = 0;
			Utilities.AddBits(ref temp32, icvTop, 0, 6);
			Utilities.AddBits(ref temp32, icvBottom, 7, 13);
			Utilities.AddBits(ref temp32, icvDiag, 14, 20);
			Utilities.AddBits(ref temp32, (int)dgDiag, 21, 24);
			this.Write(temp32);
		}

		#endregion // Read/WriteDXFBdr

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteDXFFntD

		// http://msdn.microsoft.com/en-us/library/dd953045(v=office.12).aspx
		public void ReadDXFFntD(IWorkbookFont font, bool fIfntNinch)
		{
			byte cchFont = (byte)this.ReadByte();
			long stFontNameStart = this.Position;
			string stFontName = null;
			if (cchFont != 0)
				stFontName = this.ReadXLUnicodeStringNoCch(cchFont);
			long stFontNameEnd = this.Position;
			byte[] unused1 = this.ReadBytes(63 - (int)(stFontNameEnd - stFontNameStart));
			Stxp stxp = this.ReadStxp();
			int icvFore = this.ReadInt32();
			uint reserved = this.ReadUInt32();
			Debug.Assert(reserved == 0, "The reserved value is incorrect.");
			Ts tsNinch = this.ReadTs();
			uint fSssNinch = this.ReadUInt32();
			uint fUlsNinch = this.ReadUInt32();
			uint fBlsNinch = this.ReadUInt32();
			uint unused = this.ReadUInt32();
			int ich = this.ReadInt32();
			Debug.Assert(ich >= -1, "The ich value is incorrect.");
			Debug.Assert(ich == -1 || ich == 0, "Figure out what to do when this is not -1 or 0.");
			int cch = this.ReadInt32();
			Debug.Assert(cch == -1 || cch == 0, "Figure out what to do when this is not -1 or 0.");
			ushort iFnt = this.ReadUInt16();

			if (fIfntNinch || iFnt > 0)
				font.Name = stFontName;

			if (stxp.twpHeight != -1)
				font.Height = stxp.twpHeight;

			if (tsNinch.ftsItalic == false)
				font.Italic = Utilities.ToDefaultableBoolean(stxp.ts.ftsItalic);

			if (tsNinch.ftsStrikeout == false)
				font.Strikeout = Utilities.ToDefaultableBoolean(stxp.ts.ftsStrikeout);

			if (fBlsNinch == 0)
			{
				switch (stxp.bls)
				{
					case FontNormalWeight:
						font.Bold = ExcelDefaultableBoolean.False;
						break;

					case FontBoldWeight:
						font.Bold = ExcelDefaultableBoolean.True;
						break;

					case 0xFFFF:
						break;

					default:
						Utilities.DebugFail("Unknown stxp.bls value.");

						if (stxp.bls < FontNormalWeight)
							goto case FontNormalWeight;

						goto case FontBoldWeight;
				}
			}

			if (fSssNinch == 0)
			{
				if (stxp.sss != 0xFFFF)
				{
					FontSuperscriptSubscriptStyle testValue = (FontSuperscriptSubscriptStyle)stxp.sss;
					if (Enum.IsDefined(typeof(FontSuperscriptSubscriptStyle), testValue))
						font.SuperscriptSubscriptStyle = testValue;
					else
						Utilities.DebugFail("Unknown stxp.sss value.");
				}
			}

			if (fUlsNinch == 0)
			{
				if (stxp.uls != 0xFF)
				{
					FontUnderlineStyle testValue = (FontUnderlineStyle)stxp.uls;
					if (Enum.IsDefined(typeof(FontUnderlineStyle), testValue))
						font.UnderlineStyle = testValue;
					else
						Utilities.DebugFail("Unknown stxp.uls value.");
				}
			}

			if (icvFore != -1)
			{
				font.ColorInfo = new WorkbookColorInfo(
					this.Manager.Workbook.Palette.GetColorAtAbsoluteIndex(icvFore));
			}
		}

		// http://msdn.microsoft.com/en-us/library/dd953045(v=office.12).aspx
		public void WriteDXFFntD(IWorkbookFont font)
		{
			long stFontNameStart = this.Position + 1;

			if (font.Name == null)
			{
				this.WriteByte((byte)0); // cchFont
			}
			else
			{
				this.WriteByte((byte)font.Name.Length); // cchFont
				this.WriteXLUnicodeStringNoCch(font.Name, true); // stFontName
			}

			int unused1Size = 63 - (int)(this.Position - stFontNameStart);
			this.Write(new byte[unused1Size]);

			Ts ts = new Ts();
			ts.ftsItalic = font.Italic == ExcelDefaultableBoolean.True;
			ts.ftsStrikeout = font.Strikeout == ExcelDefaultableBoolean.True;

			Stxp stxp = new Stxp();
			stxp.twpHeight = font.Height;
			stxp.ts = ts;
			switch (font.Bold)
			{
				case ExcelDefaultableBoolean.Default:
					stxp.bls = 0xFFFF;
					break;

				case ExcelDefaultableBoolean.False:
					stxp.bls = 0x0190;
					break;

				case ExcelDefaultableBoolean.True:
					stxp.bls = 0x02BC;
					break;
			}
			stxp.sss = (ushort)font.SuperscriptSubscriptStyle;
			stxp.uls = (byte)font.UnderlineStyle;
			this.WriteStxp(stxp);

			int icvFore = -1;
			if (font.ColorInfo != null)
				icvFore = font.ColorInfo.GetIndex(this.Manager.Workbook, ColorableItem.CellFont);

			this.Write(icvFore);
			this.Write((uint)0); // reserved 

			Ts tsNinch = new Ts();
			tsNinch.ftsItalic = (font.Italic == ExcelDefaultableBoolean.Default);
			tsNinch.ftsStrikeout = (font.Strikeout == ExcelDefaultableBoolean.Default);
			this.WriteTs(tsNinch);

			uint fSssNinch = (font.SuperscriptSubscriptStyle == FontSuperscriptSubscriptStyle.Default) ? (uint)1 : 0;
			this.Write(fSssNinch);

			uint fUlsNinch = (font.UnderlineStyle == FontUnderlineStyle.Default) ? (uint)1 : 0;
			this.Write(fUlsNinch);

			uint fBlsNinch = (font.Bold == ExcelDefaultableBoolean.Default) ? (uint)1 : 0;
			this.Write(fBlsNinch);

			this.Write((uint)0); // unused2 
			this.Write((int)0); // ich
			this.Write((int)0); // cch

			if (font.Name == null)
				this.Write((ushort)0); // iFnt 
			else
				this.Write((ushort)1); // iFnt 
		}

		#endregion // Read/WriteDXFFntD

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteDXFN

		// http://msdn.microsoft.com/en-us/library/dd772833(v=office.12).aspx
		public WorksheetCellFormatData ReadDXFN()
		{
			uint temp32 = this.ReadUInt32();
			bool alchNinch = Utilities.TestBit(temp32, 0);
			bool alcvNinch = Utilities.TestBit(temp32, 1);
			bool wrapNinch = Utilities.TestBit(temp32, 2);
			bool trotNinch = Utilities.TestBit(temp32, 3);
			bool kintoNinch = Utilities.TestBit(temp32, 4);
			bool cIndentNinch = Utilities.TestBit(temp32, 5);
			bool fShrinkNinch = Utilities.TestBit(temp32, 6);
			bool fMergeCellNinch = Utilities.TestBit(temp32, 7);
			bool lockedNinch = Utilities.TestBit(temp32, 8);
			bool hiddenNinch = Utilities.TestBit(temp32, 9);
			bool glLeftNinch = Utilities.TestBit(temp32, 10);
			bool glRightNinch = Utilities.TestBit(temp32, 11);
			bool glTopNinch = Utilities.TestBit(temp32, 12);
			bool glBottomNinch = Utilities.TestBit(temp32, 13);
			bool glDiagDownNinch = Utilities.TestBit(temp32, 14);
			bool glDiagUpNinch = Utilities.TestBit(temp32, 15);
			bool flsNinch = Utilities.TestBit(temp32, 16);
			bool icvFNinch = Utilities.TestBit(temp32, 17);
			bool icvBNinch = Utilities.TestBit(temp32, 18);
			bool ifmtNinch = Utilities.TestBit(temp32, 19);
			bool fIfntNinch = Utilities.TestBit(temp32, 20);
			bool unused1 = Utilities.TestBit(temp32, 21);
			int reserved1 = Utilities.GetBits(temp32, 22, 24);
			Debug.Assert(reserved1 == 0, "The reserved1 value is incorrect.");
			bool ibitAtrNum = Utilities.TestBit(temp32, 25);
			bool ibitAtrFnt = Utilities.TestBit(temp32, 26);
			bool ibitAtrAlc = Utilities.TestBit(temp32, 27);
			bool ibitAtrBdr = Utilities.TestBit(temp32, 28);
			bool ibitAtrPat = Utilities.TestBit(temp32, 29);
			bool ibitAtrProt = Utilities.TestBit(temp32, 30);
			bool iReadingOrderNinch = Utilities.TestBit(temp32, 31);

			ushort temp16 = this.ReadUInt16();
			bool fIfmtUser = Utilities.TestBit(temp16, 0);
			bool unused2 = Utilities.TestBit(temp16, 1);
			bool fNewBorder = Utilities.TestBit(temp16, 2);
			int reserved2 = Utilities.GetBits(temp16, 3, 14);
			Debug.Assert(reserved2 == 0, "The reserved2 value is incorrect.");
			bool fZeroInited = Utilities.TestBit(temp16, 15);

			WorksheetCellFormatData format = this.Manager.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);

			if (ibitAtrNum)
				this.ReadDXFNum(format, fIfmtUser, ifmtNinch);

			if (ibitAtrFnt)
				this.ReadDXFFntD(format.Font, fIfntNinch);

			if (ibitAtrAlc)
				this.ReadDXFALC(format, alchNinch, alcvNinch, wrapNinch, trotNinch, kintoNinch, cIndentNinch, fShrinkNinch, fMergeCellNinch, iReadingOrderNinch);

			if (ibitAtrBdr)
				this.ReadDXFBdr(format, glLeftNinch, glRightNinch, glTopNinch, glBottomNinch, glDiagDownNinch, glDiagUpNinch);

			if (ibitAtrPat)
				this.ReadDXFPat(format, flsNinch, icvFNinch, icvBNinch);

			if (ibitAtrProt)
				this.ReadDXFProt(format, lockedNinch, hiddenNinch);

			return format;
		}

		// http://msdn.microsoft.com/en-us/library/dd772833(v=office.12).aspx
		public void WriteDXFN(WorksheetCellFormatData format)
		{
			bool alchNinch = format.Alignment == HorizontalCellAlignment.Default;
			bool alcvNinch = format.VerticalAlignment == VerticalCellAlignment.Default;
			bool wrapNinch = format.WrapText == ExcelDefaultableBoolean.Default;
			bool trotNinch = format.Rotation == -1;
			bool kintoNinch = true;
			bool cIndentNinch = format.Indent == -1;
			bool fShrinkNinch = format.ShrinkToFit == ExcelDefaultableBoolean.Default;
			bool fMergeCellNinch = true;
			bool lockedNinch = format.Locked == ExcelDefaultableBoolean.Default;
			bool hiddenNinch = true;
			bool glLeftNinch = format.LeftBorderStyle == CellBorderLineStyle.Default && format.LeftBorderColorInfo == null;
			bool glRightNinch = format.RightBorderStyle == CellBorderLineStyle.Default && format.RightBorderColorInfo == null;
			bool glTopNinch = format.TopBorderStyle == CellBorderLineStyle.Default && format.TopBorderColorInfo == null;
			bool glBottomNinch = format.BottomBorderStyle == CellBorderLineStyle.Default && format.BottomBorderColorInfo == null;
			bool glDiagDownNinch = format.DiagonalBorderStyle == CellBorderLineStyle.Default && format.DiagonalBorderColorInfo == null && format.DiagonalBorders == DiagonalBorders.Default;
			bool glDiagUpNinch = glDiagDownNinch;
			bool flsNinch = format.Fill == null;
			bool icvFNinch = flsNinch;
			bool icvBNinch = flsNinch;
			bool ifmtNinch = format.FormatStringIndex == -1;
			bool iReadingOrderNinch = true;

			bool ibitAtrNum = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting);
			bool ibitAtrFnt = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyFontFormatting);
			bool ibitAtrAlc = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting);
			bool ibitAtrBdr = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting);
			bool ibitAtrPat = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting);
			bool ibitAtrProt = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting);

			uint temp32 = 0;
			Utilities.SetBit(ref temp32, alchNinch, 0);
			Utilities.SetBit(ref temp32, alcvNinch, 1);
			Utilities.SetBit(ref temp32, wrapNinch, 2);
			Utilities.SetBit(ref temp32, trotNinch, 3);
			Utilities.SetBit(ref temp32, kintoNinch, 4);
			Utilities.SetBit(ref temp32, cIndentNinch, 5);
			Utilities.SetBit(ref temp32, fShrinkNinch, 6);
			Utilities.SetBit(ref temp32, fMergeCellNinch, 7);
			Utilities.SetBit(ref temp32, lockedNinch, 8);
			Utilities.SetBit(ref temp32, hiddenNinch, 9);
			Utilities.SetBit(ref temp32, glLeftNinch, 10);
			Utilities.SetBit(ref temp32, glRightNinch, 11);
			Utilities.SetBit(ref temp32, glTopNinch, 12);
			Utilities.SetBit(ref temp32, glBottomNinch, 13);
			Utilities.SetBit(ref temp32, glDiagDownNinch, 14);
			Utilities.SetBit(ref temp32, glDiagUpNinch, 15);
			Utilities.SetBit(ref temp32, flsNinch, 16); // flsNinch
			Utilities.SetBit(ref temp32, icvFNinch, 17); // icvFNinch
			Utilities.SetBit(ref temp32, icvBNinch, 18); // icvBNinch
			Utilities.SetBit(ref temp32, ifmtNinch, 19);
			Utilities.SetBit(ref temp32, true, 20); // fIfntNinch
			Utilities.SetBit(ref temp32, true, 21); // unused1
			Utilities.SetBit(ref temp32, ibitAtrNum, 25);
			Utilities.SetBit(ref temp32, ibitAtrFnt, 26);
			Utilities.SetBit(ref temp32, ibitAtrAlc, 27);
			Utilities.SetBit(ref temp32, ibitAtrBdr, 28);
			Utilities.SetBit(ref temp32, ibitAtrPat, 29);
			Utilities.SetBit(ref temp32, ibitAtrProt, 30);
			Utilities.SetBit(ref temp32, iReadingOrderNinch, 31);
			this.Write(temp32);

			bool fIfmtUser = false;
			if (ibitAtrNum)
				fIfmtUser = (this.Manager.Workbook.Formats.IsStandardFormat(format.FormatStringIndexResolved) == false);

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, fIfmtUser, 0);
			Utilities.SetBit(ref temp16, true, 1); // unused2
			Utilities.SetBit(ref temp16, false, 2); // fNewBorder
			Utilities.SetBit(ref temp16, true, 15); // fZeroInited
			this.Write(temp16);

			if (ibitAtrNum)
				this.WriteDXFNum(format, fIfmtUser);

			if (ibitAtrFnt)
				this.WriteDXFFntD(format.Font);

			if (ibitAtrAlc)
				this.WriteDXFALC(format, alchNinch, alcvNinch, wrapNinch, trotNinch, kintoNinch, cIndentNinch, fShrinkNinch, fMergeCellNinch, iReadingOrderNinch);

			if (ibitAtrBdr)
				this.WriteDXFBdr(format, glLeftNinch, glRightNinch, glTopNinch, glBottomNinch, glDiagDownNinch, glDiagUpNinch);

			if (ibitAtrPat)
				this.WriteDXFPat(format, flsNinch, icvFNinch, icvBNinch);

			if (ibitAtrProt)
				this.WriteDXFProt(format, lockedNinch, hiddenNinch);
		}

		#endregion // Read/WriteDXFN

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteDXFN12List

		// http://msdn.microsoft.com/en-us/library/dd909387(v=office.12).aspx
		public WorksheetCellFormatData ReadDXFN12List(int size)
		{
			long endDXFN12List = this.Position + size;
			WorksheetCellFormatData dxfn = this.ReadDXFN();

			if (this.Position < endDXFN12List)
				this.ReadXFExtNoFRT(dxfn);

			return dxfn;
		}

		// http://msdn.microsoft.com/en-us/library/dd909387(v=office.12).aspx
		public void WriteDXFN12List(WorksheetCellFormatData format)
		{
			this.WriteDXFN(format);
			this.WriteXFExtNoFRT(format);
		}

		#endregion // Read/WriteDXFN12List

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFN12NoCB

		// http://msdn.microsoft.com/en-us/library/dd944973(v=office.12).aspx
		public WorksheetCellFormatData ReadDXFN12NoCB()
		{
			WorksheetCellFormatData dxfn = this.ReadDXFN();

			if (this.Position < this.Length)
				this.ReadXFExtNoFRT(dxfn);

			return dxfn;
		}

		// http://msdn.microsoft.com/en-us/library/dd944973(v=office.12).aspx
		public void WriteDXFN12NoCB(WorksheetCellFormatData format)
		{
			this.WriteDXFN(format);
			this.WriteXFExtNoFRT(format);
		}

		#endregion // Read/WriteDXFN12NoCB

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFNum

		// http://msdn.microsoft.com/en-us/library/dd947076(v=office.12).aspx
		public void ReadDXFNum(WorksheetCellFormatData format, bool fIfmtUser, bool ifmtNinch)
		{
			if (fIfmtUser)
				this.ReadDXFNumUsr(format);
			else
				this.ReadDXFNumIFmt(format, ifmtNinch);
		}

		// http://msdn.microsoft.com/en-us/library/dd947076(v=office.12).aspx
		public void WriteDXFNum(WorksheetCellFormatData format, bool fIfmtUser)
		{
			if (fIfmtUser)
				this.WriteDXFNumUsr(format);
			else
				this.WriteDXFNumIFmt(format);
		}

		#endregion // Read/WriteDXFNum

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFNumIFmt

		// http://msdn.microsoft.com/en-us/library/dd952183(v=office.12).aspx
		public void ReadDXFNumIFmt(WorksheetCellFormatData format, bool ifmtNinch)
		{
			byte unused = (byte)this.ReadByte();
			byte ifmt = (byte)this.ReadByte();

			if (ifmtNinch == false)
				format.FormatStringIndex = WorkbookFormatCollection.FromDxfIndex(ifmt);
		}

		// http://msdn.microsoft.com/en-us/library/dd952183(v=office.12).aspx
		public void WriteDXFNumIFmt(WorksheetCellFormatData format)
		{
			this.WriteByte(0); // unused

			byte ifmt = (byte)WorkbookFormatCollection.ToDxfIndex(format.FormatStringIndexResolved);
			this.WriteByte((byte)ifmt); // ifmt
		}

		#endregion // Read/WriteDXFNumIFmt

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFNumUsr

		// http://msdn.microsoft.com/en-us/library/dd952183(v=office.12).aspx
		public void ReadDXFNumUsr(WorksheetCellFormatData format)
		{
			ushort cb = this.ReadUInt16();
			string fmt = this.ReadXLUnicodeString();
			format.FormatString = fmt;
		}

		// http://msdn.microsoft.com/en-us/library/dd952183(v=office.12).aspx
		public void WriteDXFNumUsr(WorksheetCellFormatData format)
		{
			long recordStart = this.Position;
			this.Write((ushort)0); // cb placeholder
			this.WriteXLUnicodeString(format.FormatStringResolved); // fmt
			long recordEnd = this.Position;

			ushort cb = (ushort)(recordEnd - recordStart);

			this.Position = recordStart;
			this.Write(cb);
			this.Position = recordEnd;
		}

		#endregion // Read/WriteDXFNumUsr

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFPat

		// http://msdn.microsoft.com/en-us/library/dd910854(v=office.12).aspx
		public void ReadDXFPat(WorksheetCellFormatData format, bool flsNinch, bool icvFNinch, bool icvBNinch)
		{
			uint temp32 = this.ReadUInt32();
			FillPatternStyle fls = (FillPatternStyle)Utilities.GetBits(temp32, 10, 15);
			int icvForeground = Utilities.GetBits(temp32, 16, 22);
			int icvBackground = Utilities.GetBits(temp32, 23, 29);

			FillPatternStyle fillPattern = FillPatternStyle.None;
			if (flsNinch == false)
				fillPattern = fls;

			Workbook workbook = this.Manager.Workbook;

			WorkbookColorInfo foregroundColorInfo = null;
			if (icvFNinch == false)
				foregroundColorInfo = new WorkbookColorInfo(workbook, icvForeground);

			WorkbookColorInfo backgroundColorInfo = null;
			if (icvBNinch == false)
				backgroundColorInfo = new WorkbookColorInfo(workbook, icvBackground);

			if (flsNinch && icvFNinch && icvBNinch)
				return;

			format.Fill = new CellFillPattern(backgroundColorInfo, foregroundColorInfo, fillPattern, format);
		}

		// http://msdn.microsoft.com/en-us/library/dd910854(v=office.12).aspx
		public void WriteDXFPat(WorksheetCellFormatData format, bool flsNinch, bool icvFNinch, bool icvBNinch)
		{
			FillPatternStyle fls = FillPatternStyle.None;
			int icvForeground = 0;
			int icvBackground = 0;

			CellFill fillResolved = format.FillResolved;
			if (flsNinch == false)
				fls = format.GetFillPattern(fillResolved);

			Workbook workbook = this.Manager.Workbook;

			if (icvFNinch == false)
				icvForeground = format.GetFileFormatFillPatternColor(fillResolved, false, true).GetIndex(workbook, ColorableItem.CellFill);

			if (icvBNinch == false)
				icvBackground = format.GetFileFormatFillPatternColor(fillResolved, true, true).GetIndex(workbook, ColorableItem.CellFill);

			uint temp32 = 0;
			Utilities.AddBits(ref temp32, (int)fls, 10, 15);
			Utilities.AddBits(ref temp32, icvForeground, 16, 22);
			Utilities.AddBits(ref temp32, icvBackground, 23, 29);
			this.Write(temp32);
		}

		#endregion // Read/WriteDXFPat

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteDXFProt

		// http://msdn.microsoft.com/en-us/library/dd906760(v=office.12).aspx
		public void ReadDXFProt(WorksheetCellFormatData format, bool lockedNinch, bool hiddenNinch)
		{
			ushort temp16 = this.ReadUInt16();
			bool fLocked = Utilities.TestBit(temp16, 0);
			bool fHidden = Utilities.TestBit(temp16, 1);

			if (lockedNinch == false)
				format.Locked = Utilities.ToDefaultableBoolean(fLocked);
		}

		// http://msdn.microsoft.com/en-us/library/dd906760(v=office.12).aspx
		public void WriteDXFProt(WorksheetCellFormatData format, bool lockedNinch, bool hiddenNinch)
		{
			bool fLocked = true;
			bool fHidden = false;

			if (lockedNinch == false)
				fLocked = format.LockedResolved == ExcelDefaultableBoolean.True;

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, fLocked, 0);
			Utilities.SetBit(ref temp16, fHidden, 1);
			this.Write(temp16);
		}

		#endregion // Read/WriteDXFProt

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region Read/WriteExtProp

		// http://msdn.microsoft.com/en-us/library/dd906769(v=office.12).aspx
		public ExtProp ReadExtProp()
		{
			long startPosition = this.Position;

			ExtPropType extType = (ExtPropType)this.ReadUInt16();
			Debug.Assert(Enum.IsDefined(typeof(ExtPropType), extType), "This is not a valid extProp.");

			ushort cb = this.ReadUInt16();

			try
			{
				switch (extType)
				{
					case ExtPropType.BackgroundColor:
					case ExtPropType.BottomBorderColor:
					case ExtPropType.CellTextColor:
					case ExtPropType.DiagonalBorderColor:
					case ExtPropType.ForegroundColor:
					case ExtPropType.LeftBorderColor:
					case ExtPropType.RightBorderColor:
					case ExtPropType.TopBorderColor:
						return new ExtPropColor(this.ReadFullColorExt(), extType);

					case ExtPropType.FontScheme:
						FontScheme fontScheme = (FontScheme)this.ReadByte();
						Debug.Assert(Enum.IsDefined(typeof(FontScheme), fontScheme), "This is not a valid FontScheme.");
						return new ExtPropFontScheme(fontScheme);

					case ExtPropType.GradientFill:
						return new ExtPropGradientFill(this.ReadXFExtGradient());

					case ExtPropType.TextIndentationLevel:
						ushort textIndentationLevel = this.ReadUInt16();
						Debug.Assert(textIndentationLevel <= WorksheetCellFormatData.MaxIndent2007, "This is not a valid TextIndentationLevel.");
						textIndentationLevel = Math.Min((ushort)WorksheetCellFormatData.MaxIndent2007, textIndentationLevel);
						return new ExtPropTextIndentationLevel(textIndentationLevel);

					default:
						Utilities.DebugFail("Unknown ExtPropType: " + extType);
						return null;
				}
			}
			finally
			{
				long endPosition = startPosition + cb;
				Debug.Assert(this.Position == endPosition, "We did not read in the full ExtProp.");
				this.Position = endPosition;
			}
		}

		// http://msdn.microsoft.com/en-us/library/dd906769(v=office.12).aspx
		public void WriteExtProp(ExtProp extProp)
		{
			if (extProp == null)
			{
				Utilities.DebugFail("The extProp is null.");
				return;
			}

			long startPosition = this.Position;

			this.Write((ushort)extProp.ExtType);
			this.Write((ushort)0); // temporary cb
			extProp.Save(this);

			long endPostion = this.Position;
			short cb = (short)(endPostion - startPosition);
			this.Position = startPosition + 2;
			this.Write(cb);
			this.Position = endPostion;
		}

		#endregion // Read/WriteExtProp

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteFeat11FdaAutoFilter

		// http://msdn.microsoft.com/en-us/library/dd909901(v=office.12).aspx
		public Filter ReadFeat11FdaAutoFilter(Worksheet worksheet, WorksheetTableColumn column)
		{
			uint cbAutoFilter = this.ReadUInt32();
			Debug.Assert(cbAutoFilter <= 2080, "The cbAutoFilter value is incorrect");

			ushort unused = this.ReadUInt16();

			if (cbAutoFilter == 0)
				return null;

			return this.ReadAutoFilter(worksheet, column);
		}

		// http://msdn.microsoft.com/en-us/library/dd909901(v=office.12).aspx
		public void WriteFeat11FdaAutoFilter(WorksheetTableColumn column, TableColumnFilterData filterData)
		{
			long cbAutoFilterPosition = this.Position;

			this.Write((uint)0); // cbAutoFilter placeholder
			this.Write((ushort)0xFFFF); // unused

			if (filterData != null && filterData.ShouldSaveIn2003Formats)
			{
				long start = this.Position;
				this.WriteAutoFilter(column, filterData);
				long end = this.Position;
				ushort size = (ushort)(end - start);

				this.Position = cbAutoFilterPosition;
				this.Write(size);
				this.Position = end;
			}
		}

		#endregion // Read/WriteFeat11FdaAutoFilter

		// MD 2/18/12 - 12.1 - Table Support
		#region Read/WriteFeat11FieldDataItem

		// http://msdn.microsoft.com/en-us/library/dd909143(v=office.12).aspx
		private WorksheetTableColumn ReadFeat11FieldDataItem(Worksheet worksheet, WorksheetTable table, bool fSingleCell, uint crwHeader)
		{
			uint idField = this.ReadUInt32();
			uint lfdt = this.ReadUInt32();
			Debug.Assert(lfdt == 0, "The lfdt value is incorrect.");

			uint lfxidt = this.ReadUInt32();
			Debug.Assert(lfdt == 0, "The lfxidt value is incorrect.");

			ST_TotalsRowFunction ilta = (ST_TotalsRowFunction)this.ReadUInt32();
			uint cbFmtAgg = this.ReadUInt32();
			int istnAgg = this.ReadInt32();

			uint temp32 = this.ReadUInt32();
			bool fAutoFilter = Utilities.TestBit(temp32, 0);
			bool fAutoFilterHidden = Utilities.TestBit(temp32, 1);
			bool fLoadXmapi = Utilities.TestBit(temp32, 2);
			Debug.Assert(fLoadXmapi == false, "The fLoadXmapi value is incorrect.");
			bool fLoadFmla = Utilities.TestBit(temp32, 3);
			Debug.Assert(fLoadFmla == false, "The fLoadFmla value is incorrect.");
			int unused1 = Utilities.GetBits(temp32, 4, 5);
			bool reserved2 = Utilities.TestBit(temp32, 6);
			Debug.Assert(reserved2 == false, "The reserved2 value is incorrect.");
			bool fLoadTotalFmla = Utilities.TestBit(temp32, 7);
			Debug.Assert(ilta == ST_TotalsRowFunction.custom || fLoadTotalFmla == false, "The fLoadTotalFmla value is incorrect.");
			bool fLoadTotalArray = Utilities.TestBit(temp32, 8);
			Debug.Assert(fLoadTotalArray == false || fLoadTotalFmla, "The fLoadTotalArray value is incorrect.");
			bool fSaveStyleName = Utilities.TestBit(temp32, 9);
			bool fLoadTotalStr = Utilities.TestBit(temp32, 10);
			Debug.Assert(ilta == 0 || fLoadTotalStr == false, "The fLoadTotalStr value is incorrect.");
			bool fAutoCreateCalcCol = Utilities.TestBit(temp32, 11);
			int unused2 = Utilities.GetBits(temp32, 12, 31);

			uint cbFmtInsertRow = this.ReadUInt32();
			int istnInsertRow = this.ReadInt32();
			string strFieldName = this.ReadXLUnicodeString();
			Debug.Assert(strFieldName != null && 1 <= strFieldName.Length && strFieldName.Length <= 255, "The strFieldName value is incorrect.");

			string columnName = strFieldName;
			if (fSingleCell == false)
			{
				string strCaption = this.ReadXLUnicodeString();
				Debug.Assert(strCaption != null && 1 <= strCaption.Length && strCaption.Length <= 255, "The strCaption value is incorrect.");

				columnName = strCaption;
			}

			WorksheetTableColumn column = table.InsertColumn(idField);
			column.Name = columnName;
			column.TotalFormula = this.Manager.Workbook.GetTotalFormula(column, ilta);

			WorkbookStyle totalsDxfStyle = this.ManagerBiff8.GetLoadedStyle(istnAgg);
			WorksheetCellFormatData totalsDxf = null;
			if (cbFmtAgg != 0)
				totalsDxf = this.ReadDXFN12List((int)cbFmtAgg);

			WorkbookStyle dataDxfStyle = this.ManagerBiff8.GetLoadedStyle(istnInsertRow);
			WorksheetCellFormatData dataDxf = null;
			if (cbFmtInsertRow != 0)
				dataDxf = this.ReadDXFN12List((int)cbFmtInsertRow);

			if (fAutoFilter)
				column.Filter = this.ReadFeat11FdaAutoFilter(worksheet, column);

			if (fLoadXmapi)
			{
				Utilities.DebugFail("Load the rgXmap.");
			}

			if (fLoadFmla)
			{
				Utilities.DebugFail("Load the fmla.");
			}

			if (fLoadTotalFmla)
			{
				FormulaType type = fLoadTotalArray
					? FormulaType.ArrayFormula
					: FormulaType.Formula;

				
				byte[] remainingData = this.ReadBytes((int)(this.Length - this.Position));
				int dataIndex = 0;
				Formula totalFmla = Formula.Load(this, type, ref remainingData, ref dataIndex);
				this.Seek(dataIndex - remainingData.Length, SeekOrigin.Current);

				column.TotalFormula = totalFmla;
			}

			if (fLoadTotalStr)
			{
				string strTotal = this.ReadXLUnicodeString();
				column.TotalLabel = strTotal;
			}

			if (crwHeader == 0 && fSingleCell == false)
				this.ReadCachedDiskHeader(column, fSaveStyleName);

			this.Manager.CombineDxfInfo(column.AreaFormats, WorksheetTableColumnArea.DataArea, WorksheetTableColumn.CanAreaFormatValueBeSet, 
				dataDxfStyle, dataDxf);
			this.Manager.CombineDxfInfo(column.AreaFormats, WorksheetTableColumnArea.TotalCell, WorksheetTableColumn.CanAreaFormatValueBeSet, 
				totalsDxfStyle, totalsDxf);

			// When loading a file, Excel takes the formula of the last cell in the column to be the column formula, 
			// so we will do the same, even though it could be an exception cell in the column (have another value 
			// or formula applied).
			if (fAutoCreateCalcCol)
			{
				int dataAreaTopRowIndex;
				int dataAreaBottomRowIndex;
				table.GetDataAreaRowIndexes(out dataAreaTopRowIndex, out dataAreaBottomRowIndex);
				column.SetColumnFormula(worksheet.Rows[dataAreaBottomRowIndex].Cells[column.WorksheetColumnIndex].Formula.Clone(), false);
			}

			return column;
		}

		// http://msdn.microsoft.com/en-us/library/dd909143(v=office.12).aspx
		public void WriteFeat11FieldDataItem(WorksheetTableColumn column, SortedList<int, TableColumnFilterData> columnsFilterData, bool fSingleCell)
		{
			WorkbookStyle dxfDataStyle;
			WorksheetCellFormatData dxfData;
			this.Manager.ExtractDxfInfo(column.AreaFormats, WorksheetTableColumnArea.DataArea, out dxfDataStyle, out dxfData);

			WorkbookStyle dxfTotalsStyle;
			WorksheetCellFormatData dxfTotals;
			this.Manager.ExtractDxfInfo(column.AreaFormats, WorksheetTableColumnArea.TotalCell, out dxfTotalsStyle, out dxfTotals);

			int istnInsertRow = -1;
			if (dxfDataStyle != null)
				istnInsertRow = this.ManagerBiff8.Styles.IndexOf(dxfDataStyle);

			int istnAgg = -1;
			if (dxfTotalsStyle != null)
				istnAgg = this.ManagerBiff8.Styles.IndexOf(dxfTotalsStyle);

			this.Write((uint)column.Id); // idField
			this.Write((uint)0); // lfdt
			this.Write((uint)0); // lfxidt

			ST_TotalsRowFunction ilta = this.Manager.Workbook.GetTotalRowFunction(column);
			this.Write((uint)ilta);

			long cbFmtAggPosition = this.Position;
			this.Write((uint)0); // cbFmtAgg placeholder
			this.Write(istnAgg); // istnAgg

			TableColumnFilterData filterData;
			columnsFilterData.TryGetValue(column.Index, out filterData);

			bool fAutoFilter = (filterData != null && filterData.ShouldSaveIn2003Formats) || column.Table.IsFilterUIVisible;
			bool writeCustomFormula = ilta == ST_TotalsRowFunction.custom;
			bool writeTotalLabel = ilta == ST_TotalsRowFunction.none && column.Table.IsTotalsRowVisible == false && column.TotalLabel != null;

			WorkbookStyle dxfHeaderStyle = null;
			WorksheetCellFormatData dxfHeader = null;
			if (column.Table.IsHeaderRowVisible == false)
				this.Manager.ExtractDxfInfo(column.AreaFormats, WorksheetTableColumnArea.HeaderCell, out dxfHeaderStyle, out dxfHeader);

			bool fSaveStyleName = (dxfHeaderStyle != null);

			uint temp32 = 0;
			Utilities.SetBit(ref temp32, fAutoFilter, 0); // fAutoFilter
			Utilities.SetBit(ref temp32, writeCustomFormula, 7); // fLoadTotalFmla
			Utilities.SetBit(ref temp32, writeCustomFormula && column.TotalFormula is ArrayFormula, 8); // fLoadTotalArray
			Utilities.SetBit(ref temp32, fSaveStyleName, 9);
			Utilities.SetBit(ref temp32, writeTotalLabel, 10);
			Utilities.SetBit(ref temp32, column.ColumnFormula != null, 11); // fAutoCreateCalcCol
			this.Write(temp32);

			long cbFmtInsertRowPosition = this.Position;
			this.Write((uint)0); // cbFmtInsertRow placeholder
			this.Write(istnInsertRow); // istnInsertRow
			this.WriteXLUnicodeString(column.Id.ToString()); // strFieldName

			if (fSingleCell == false)
				this.WriteXLUnicodeString(column.Name); // strCaption

			this.WriteDXFN12ListAndSizeField(cbFmtAggPosition, dxfTotals);
			this.WriteDXFN12ListAndSizeField(cbFmtInsertRowPosition, dxfData);

			if (fAutoFilter)
				this.WriteFeat11FdaAutoFilter(column, filterData); // AutoFilter

			if (writeCustomFormula)
				column.TotalFormula.Save(this, true, false); // totalFmla

			if (writeTotalLabel)
				this.WriteXLUnicodeString(column.TotalLabel); // strTotal

			if (column.Table.IsHeaderRowVisible == false)
				this.WriteCachedDiskHeader(column, dxfHeaderStyle, dxfHeader); // dskHdrCache 
		}

		#endregion // Read/WriteFeat11FieldDataItem

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteFontScheme

		// http://msdn.microsoft.com/en-us/library/dd952422(v=office.12).aspx
		public FontScheme ReadFontScheme()
		{
			return (FontScheme)this.ReadByte();
		}

		public void WriteFontScheme(FontScheme fontScheme)
		{
			this.Write((byte)fontScheme);
		}

		#endregion // Read/WriteFontScheme

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteFormat

		// http://msdn.microsoft.com/en-us/library/dd944946(v=office.12).aspx
		public void ReadFormat(out ushort numFmtId, out string format)
		{
			numFmtId = this.ReadUInt16();
			format = this.ReadXLUnicodeString();
		}

		// http://msdn.microsoft.com/en-us/library/dd944946(v=office.12).aspx
		public void WriteFormat(ushort numFmtId, string format)
		{
			this.Write(numFmtId);
			this.WriteXLUnicodeString(format);
		}

		#endregion // Read/WriteFormat

		// MD 11/29/11 - TFS96205
		#region Read/WriteFrtHeader

		// http://msdn.microsoft.com/en-us/library/dd910234(v=office.12).aspx
		public void ReadFrtHeader()
		{
			ushort rt = this.ReadUInt16();
			Debug.Assert(rt == this.RecordTypeInternal, "The repeated type does not match.");

			ushort frtFlags = this.ReadUInt16();

			// 8 Unused Bytes
			this.Seek(8, SeekOrigin.Current);
		}

		// http://msdn.microsoft.com/en-us/library/dd910234(v=office.12).aspx
		public void WriteFrtHeader()
		{
			this.Write((ushort)this.RecordTypeInternal); // rt
			this.Write((ushort)0); // frtFlags
			this.Write(new byte[8]); // reserved
		}

		#endregion  // Read/WriteFrtHeader

		// MD 2/18/12 - 12.1 - Table Support
		#region Read/WriteFrtHeaderU

		// http://msdn.microsoft.com/en-us/library/dd947923(v=office.12).aspx
		public WorksheetRegion ReadFrtHeaderU(Worksheet worksheet)
		{
			ushort rt = this.ReadUInt16();
			Debug.Assert(rt == this.RecordTypeInternal, "The repeated type does not match.");

			ushort frtFlags = this.ReadUInt16();
			Debug.Assert(frtFlags == 1, "The frtFlags are incorrect.");

			return this.ReadRef8U(worksheet);
		}

		// http://msdn.microsoft.com/en-us/library/dd947923(v=office.12).aspx
		public void WriteFrtHeaderU(WorksheetRegion region)
		{
			this.Write((ushort)this.RecordTypeInternal); // rt
			this.Write((ushort)1); // frtFlags
			this.WriteRef8U(region);
		}

		#endregion // Read/WriteFrtHeaderU

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteFrtRefHeader

		// http://msdn.microsoft.com/en-us/library/dd944201(v=office.12).aspx
		public WorksheetRegion ReadFrtRefHeader(Worksheet worksheet)
		{
			return this.ReadFrtRefHeader(worksheet, this.RecordType);
		}

		
		// http://msdn.microsoft.com/en-us/library/dd944201(v=office.12).aspx
		public WorksheetRegion ReadFrtRefHeader(Worksheet worksheet, BIFF8RecordType recordType)
		{
			ushort rt = this.ReadUInt16();
			Debug.Assert(rt == (ushort)recordType, "The repeated type does not match.");

			ushort frtFlags = this.ReadUInt16();

			if (Utilities.TestBit(frtFlags, 0))
				return this.ReadRef8U(worksheet);

			this.Seek(8, SeekOrigin.Current);
			return null;
		}

		// http://msdn.microsoft.com/en-us/library/dd944201(v=office.12).aspx
		public void WriteFrtRefHeader(WorksheetRegion region)
		{
			this.WriteFrtRefHeader(region, this.RecordType);
		}

		
		// http://msdn.microsoft.com/en-us/library/dd944201(v=office.12).aspx
		public void WriteFrtRefHeader(WorksheetRegion region, BIFF8RecordType recordType)
		{
			this.Write((ushort)recordType); // rt

			if (region != null)
			{
				this.Write((ushort)1); // frtFlags
				this.WriteRef8U(region);
			}
			else
			{
				this.Write((ushort)0); // frtFlags
				this.Write(new byte[8]);
			}
		}

		#endregion // Read/WriteFrtRefHeader

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region Read/WriteFullColorExt

		// http://msdn.microsoft.com/en-us/library/dd947047(v=office.12).aspx
		public WorkbookColorInfo ReadFullColorExt()
		{
			XColorType colorType = (XColorType)this.ReadUInt16();
			short nTintShade = this.ReadInt16();
			uint xClrValue = this.ReadUInt32();
			this.Seek(8, SeekOrigin.Current);

			double? tint = null;
			if (nTintShade != 0)
				tint = (double)nTintShade / Int16.MaxValue;

			return this.GetColorInfoFromRawValues(colorType, xClrValue, tint);
		}

		// http://msdn.microsoft.com/en-us/library/dd947047(v=office.12).aspx
		public void WriteFullColorExt(WorkbookColorInfo colorInfo, ColorableItem item)
		{
			XColorType colorType;
			double tint;
			uint xClrValue;
			this.GetRawColorInfoValues(colorInfo, item, out colorType, out tint, out xClrValue);
			short nTintShade = (short)MathUtilities.MidpointRoundingAwayFromZero(tint * Int16.MaxValue);

			this.Write((ushort)colorType);
			this.Write(nTintShade);
			this.Write(xClrValue);
			this.Write(new byte[8]);
		}

		#endregion // Read/WriteFullColorExt

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region Read/WriteGradStop

		// http://msdn.microsoft.com/en-us/library/dd907878(v=office.12).aspx
		public CellFillGradientStop ReadGradStop()
		{
			XColorType xclrType = (XColorType)this.ReadUInt16();
			uint xClrValue = this.ReadUInt32();
			double numPosition = this.ReadDouble();
			double numTint = this.ReadDouble();

			double? tint = null;
			if (numTint != 0)
				tint = numTint;

			WorkbookColorInfo colorInfo = this.GetColorInfoFromRawValues(xclrType, xClrValue, tint);
			return new CellFillGradientStop(colorInfo, numPosition);
		}

		// http://msdn.microsoft.com/en-us/library/dd907878(v=office.12).aspx
		public void WriteGradStop(CellFillGradientStop gradientStop)
		{
			double numPosition = gradientStop.Offset;

			XColorType colorType;
			double tint;
			uint xClrValue;
			this.GetRawColorInfoValues(gradientStop.ColorInfo, ColorableItem.CellFill, out colorType, out tint, out xClrValue);

			this.Write((ushort)colorType);
			this.Write(xClrValue);
			this.Write(numPosition);
			this.Write(tint);
		}

		#endregion // Read/WriteGradStop

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteHorizAlign

		// http://msdn.microsoft.com/en-us/library/dd908508(v=office.12).aspx
		public HorizontalCellAlignment ReadHorizAlign()
		{
			byte value = (byte)this.ReadByte();
			if (value == 0xFF)
				return HorizontalCellAlignment.Default;

			return (HorizontalCellAlignment)value;
		}

		// http://msdn.microsoft.com/en-us/library/dd908508(v=office.12).aspx
		public void WriteHorizAlign(HorizontalCellAlignment value)
		{
			if (value == HorizontalCellAlignment.Default)
				this.Write((byte)0xFF);
			else
				this.Write((byte)value);
		}

		#endregion // Read/WriteHorizAlign

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteList12BlockLevel

		// http://msdn.microsoft.com/en-us/library/dd923200(v=office.12).aspx
		public void ReadList12BlockLevel(WorksheetTable table)
		{
			int cbdxfHeader = this.ReadInt32();
			Debug.Assert(0 <= cbdxfHeader, "The cbdxfHeader value is incorrect.");
			int istnHeader = this.ReadInt32();
			WorkbookStyle headerStyle = this.ManagerBiff8.GetLoadedStyle(istnHeader);

			int cbdxfData = this.ReadInt32();
			Debug.Assert(0 <= cbdxfData, "The cbdxfHeader value is incorrect.");
			int istnData = this.ReadInt32();
			WorkbookStyle dataStyle = this.ManagerBiff8.GetLoadedStyle(istnData);

			int cbdxfAgg = this.ReadInt32();
			Debug.Assert(0 <= cbdxfAgg, "The cbdxfAgg value is incorrect.");
			int istnAgg = this.ReadInt32();
			WorkbookStyle totalsStyle = this.ManagerBiff8.GetLoadedStyle(istnAgg);

			int cbdxfBorder = this.ReadInt32();
			Debug.Assert(0 <= cbdxfBorder, "The cbdxfBorder value is incorrect.");
			int cbdxfHeaderBorder = this.ReadInt32();
			Debug.Assert(0 <= cbdxfHeaderBorder, "The cbdxfHeaderBorder value is incorrect.");
			int cbdxfAggBorder = this.ReadInt32();
			Debug.Assert(0 <= cbdxfAggBorder, "The cbdxfAggBorder value is incorrect.");

			WorksheetCellFormatData dxfHeader = null;
			if (cbdxfHeader != 0)
				dxfHeader = this.ReadDXFN12List(cbdxfHeader);

			WorksheetCellFormatData dxfData = null;
			if (cbdxfData != 0)
				dxfData = this.ReadDXFN12List(cbdxfData);

			WorksheetCellFormatData dxfAgg = null;
			if (cbdxfAgg != 0)
				dxfAgg = this.ReadDXFN12List(cbdxfAgg);

			WorksheetCellFormatData dxfBorder = null;
			if (cbdxfBorder != 0)
				dxfBorder = this.ReadDXFN12List(cbdxfBorder);

			WorksheetCellFormatData dxfHeaderBorder = null;
			if (cbdxfHeaderBorder != 0)
				dxfHeaderBorder = this.ReadDXFN12List(cbdxfHeaderBorder);

			WorksheetCellFormatData dxfAggBorder = null;
			if (cbdxfAggBorder != 0)
				dxfAggBorder = this.ReadDXFN12List(cbdxfAggBorder);

			if (istnHeader != -1)
			{
				string stHeader = this.ReadXLUnicodeString();
				Debug.Assert(headerStyle != null && stHeader == headerStyle.Name, "The stHeader value is incorrect.");
			}

			if (istnData != -1)
			{
				string stData = this.ReadXLUnicodeString();
				Debug.Assert(dataStyle != null && stData == dataStyle.Name, "The stData value is incorrect.");
			}

			if (istnAgg != -1)
			{
				string stAgg = this.ReadXLUnicodeString();
				Debug.Assert(totalsStyle != null && stAgg == totalsStyle.Name, "The stAgg value is incorrect.");
			}

			this.Manager.CombineDxfInfo(table.AreaFormats, WorksheetTableArea.HeaderRow, WorksheetTable.CanAreaFormatValueBeSet,
				headerStyle, dxfHeader, dxfHeaderBorder,
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);

			this.Manager.CombineDxfInfo(table.AreaFormats, WorksheetTableArea.DataArea, WorksheetTable.CanAreaFormatValueBeSet,
				dataStyle, dxfData);

			this.Manager.CombineDxfInfo(table.AreaFormats, WorksheetTableArea.TotalsRow, WorksheetTable.CanAreaFormatValueBeSet,
				totalsStyle, dxfAgg, dxfAggBorder,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle);

			this.Manager.CombineDxfInfo(table.AreaFormats, WorksheetTableArea.WholeTable, WorksheetTable.CanAreaFormatValueBeSet,
				null, null, dxfBorder,
				CellFormatValue.LeftBorderColorInfo, CellFormatValue.LeftBorderStyle,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle,
				CellFormatValue.RightBorderColorInfo, CellFormatValue.RightBorderStyle,
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);
		}

		// http://msdn.microsoft.com/en-us/library/dd923200(v=office.12).aspx
		public void WriteList12BlockLevel(WorksheetTable table)
		{
			long recordStart = this.Position;

			WorkbookStyle headerStyle;
			WorksheetCellFormatData dxfHeader;
			WorksheetCellFormatData dxfHeaderBorder;
			this.Manager.ExtractDxfInfo(table.AreaFormats, WorksheetTableArea.HeaderRow, out headerStyle, out dxfHeader, out dxfHeaderBorder,
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);

			WorkbookStyle dataStyle;
			WorksheetCellFormatData dxfData;
			this.Manager.ExtractDxfInfo(table.AreaFormats, WorksheetTableArea.DataArea, out dataStyle, out dxfData);

			WorkbookStyle totalsStyle;
			WorksheetCellFormatData dxfAgg;
			WorksheetCellFormatData dxfAggBorder;
			this.Manager.ExtractDxfInfo(table.AreaFormats, WorksheetTableArea.TotalsRow, out totalsStyle, out dxfAgg, out dxfAggBorder,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle);

			WorkbookStyle notUsed1;
			WorksheetCellFormatData notUsed2;
			WorksheetCellFormatData dxfBorder;
			this.Manager.ExtractDxfInfo(table.AreaFormats, WorksheetTableArea.WholeTable, out notUsed1, out notUsed2, out dxfBorder,
				CellFormatValue.LeftBorderColorInfo, CellFormatValue.LeftBorderStyle,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle,
				CellFormatValue.RightBorderColorInfo, CellFormatValue.RightBorderStyle,
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);

			int istnHeader = -1;
			if (headerStyle != null)
				istnHeader = this.ManagerBiff8.Styles.IndexOf(headerStyle);

			int istnData = -1;
			if (dataStyle != null)
				istnData = this.ManagerBiff8.Styles.IndexOf(dataStyle);

			int istnAgg = -1;
			if (totalsStyle != null)
				istnAgg = this.ManagerBiff8.Styles.IndexOf(totalsStyle);

			this.Write((int)0); // cbdxfHeader placeholder
			this.Write(istnHeader);
			this.Write((int)0); // cbdxfData placeholder
			this.Write(istnData);
			this.Write((int)0); // cbdxfAgg placeholder
			this.Write(istnAgg);
			this.Write((int)0); // cbdxfBorder placeholder
			this.Write((int)0); // cbdxfHeaderBorder placeholder
			this.Write((int)0); // cbdxfAggBorder placeholder

			this.WriteDXFN12ListAndSizeField(recordStart, dxfHeader);
			this.WriteDXFN12ListAndSizeField(recordStart + 8, dxfData);
			this.WriteDXFN12ListAndSizeField(recordStart + 16, dxfAgg);
			this.WriteDXFN12ListAndSizeField(recordStart + 24, dxfBorder);
			this.WriteDXFN12ListAndSizeField(recordStart + 28, dxfHeaderBorder);
			this.WriteDXFN12ListAndSizeField(recordStart + 32, dxfAggBorder);

			if (headerStyle != null)
				this.WriteXLUnicodeString(headerStyle.Name); // stHeader 

			if (dataStyle != null)
				this.WriteXLUnicodeString(dataStyle.Name); // stData

			if (totalsStyle != null)
				this.WriteXLUnicodeString(totalsStyle.Name); // stAgg 
		}

		#endregion // Read/WriteList12BlockLevel

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteList12DisplayName

		// http://msdn.microsoft.com/en-us/library/dd950109(v=office.12).aspx
		public void ReadList12DisplayName(WorksheetTable table)
		{
			string stListName = this.ReadXLUnicodeString();
			Debug.Assert(stListName == table.Name, "The stListName is incorrect.");

			string stListComment = this.ReadXLUnicodeString();
			if (String.IsNullOrEmpty(stListComment) == false)
				table.Comment = stListComment;
		}

		// http://msdn.microsoft.com/en-us/library/dd950109(v=office.12).aspx
		public void WriteList12DisplayName(WorksheetTable table)
		{
			this.WriteXLUnicodeString(table.Name); // stListName
			this.WriteXLUnicodeString(table.Comment ?? string.Empty); // stListComment
		}

		#endregion // Read/WriteList12DisplayName

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteList12TableStyleClientInfo

		// http://msdn.microsoft.com/en-us/library/dd925061(v=office.12).aspx
		public void ReadList12TableStyleClientInfo(WorksheetTable table)
		{
			ushort temp16 = this.ReadUInt16();
			bool fFirstColumn = Utilities.TestBit(temp16, 0);
			bool fLastColumn = Utilities.TestBit(temp16, 1);
			bool fRowStripes = Utilities.TestBit(temp16, 2);
			bool fColumnStripes = Utilities.TestBit(temp16, 3);
			bool fDefaultStyle = Utilities.TestBit(temp16, 6);

			string stListStyleName = this.ReadXLUnicodeString();

			table.Style = this.Manager.Workbook.GetTableStyle(stListStyleName);
			table.DisplayBandedColumns = fColumnStripes;
			table.DisplayBandedRows = fRowStripes;
			table.DisplayFirstColumnFormatting = fFirstColumn;
			table.DisplayLastColumnFormatting = fLastColumn;
		}

		// http://msdn.microsoft.com/en-us/library/dd925061(v=office.12).aspx
		public void WriteList12TableStyleClientInfo(WorksheetTable table)
		{
			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, table.DisplayFirstColumnFormatting, 0); // fFirstColumn
			Utilities.SetBit(ref temp16, table.DisplayLastColumnFormatting, 1); // fLastColumn
			Utilities.SetBit(ref temp16, table.DisplayBandedRows, 2); // fRowStripes
			Utilities.SetBit(ref temp16, table.DisplayBandedColumns, 3); // fColumnStripes
			Utilities.SetBit(ref temp16, table.Style == this.Manager.Workbook.DefaultTableStyle, 6); // fDefaultStyle
			this.Write(temp16);

			this.WriteXLUnicodeString(table.Style.Name);
		}

		#endregion // Read/WriteList12TableStyleClientInfo

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteLPWideString

		// http://msdn.microsoft.com/en-us/library/dd921541(v=office.12).aspx
		public string ReadLPWideString()
		{
			ushort cchCharacters = this.ReadUInt16();
			byte[] data = this.ReadBytes(cchCharacters * 2);
			return Encoding.Unicode.GetString(data); // rgchData
		}

		// http://msdn.microsoft.com/en-us/library/dd921541(v=office.12).aspx
		public void WriteLPWideString(string value)
		{
			this.Write((ushort)value.Length); // cchCharacters
			this.Write(Encoding.Unicode.GetBytes(value)); // rgchData
		}

		#endregion // Read/WriteLPWideString

		// MD 2/18/12 - 12.1 - Table Support
		#region Read/WriteRef8U

		// http://msdn.microsoft.com/en-us/library/dd947902(v=office.12).aspx
		public WorksheetRegion ReadRef8U(Worksheet worksheet)
		{
			int firstRow = this.ReadUInt16();
			int lastRow = this.ReadUInt16();
			int firstColumn = this.ReadUInt16();
			int lastColumn = this.ReadUInt16();

			return worksheet.GetCachedRegion(firstRow, firstColumn, lastRow, lastColumn);
		}

		// http://msdn.microsoft.com/en-us/library/dd947902(v=office.12).aspx
		public void WriteRef8U(WorksheetRegion region)
		{
			this.Write((ushort)region.FirstRow);
			this.Write((ushort)region.LastRow);
			this.Write((ushort)region.FirstColumnInternal);
			this.Write((ushort)region.LastColumnInternal);
		}

		#endregion // Read/WriteRef8U

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteRFX

		// http://msdn.microsoft.com/en-us/library/dd951623(v=office.12).aspx
		public WorksheetRegion ReadRFX(Worksheet worksheet)
		{
			uint rwFirst = this.ReadUInt32();
			uint rwLast = this.ReadUInt32();
			uint colFirst = this.ReadUInt32();
			uint colLast = this.ReadUInt32();

			return worksheet.GetCachedRegion((int)rwFirst, (int)colFirst, (int)rwLast, (int)colLast);
		}

		// http://msdn.microsoft.com/en-us/library/dd951623(v=office.12).aspx
		public void WriteRFX(WorksheetRegion region)
		{
			this.Write((uint)region.FirstRow);
			this.Write((uint)region.LastRow);
			this.Write((uint)region.FirstColumnInternal);
			this.Write((uint)region.LastColumnInternal);
		}

		#endregion // Read/WriteRFX

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteScript

		// http://msdn.microsoft.com/en-us/library/dd953424(v=office.12).aspx
		public FontSuperscriptSubscriptStyle ReadScript()
		{
			return (FontSuperscriptSubscriptStyle)this.ReadUInt16();
		}

		// http://msdn.microsoft.com/en-us/library/dd953424(v=office.12).aspx
		public void WriteScript(FontSuperscriptSubscriptStyle value)
		{
			this.Write((ushort)value);
		}

		#endregion // Read/WriteScript

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteSortCond12

		// http://msdn.microsoft.com/en-us/library/dd951309(v=office.12).aspx
		public void ReadSortCond12(WorksheetTable table)
		{
			this.ReadFrtRefHeader(table.Worksheet, BIFF8RecordType.CONTINUEFRT12);

			ushort temp16 = this.ReadUInt16();
			bool fSortDes = Utilities.TestBit(temp16, 0);
			ST_SortBy sortOn = (ST_SortBy)Utilities.GetBits(temp16, 1, 4);
			int reserved = Utilities.GetBits(temp16, 5, 15);
			Debug.Assert(reserved == 0, "The reserved value is incorrect.");

			WorksheetRegion rfx = this.ReadRFX(table.Worksheet);

			int tableColumnIndex = rfx.FirstColumn - table.WholeTableAddress.FirstColumnIndex;
			if (tableColumnIndex < 0 || table.Columns.Count <= tableColumnIndex)
			{
				Utilities.DebugFail("The rfx value is out of range.");
				return;
			}

			WorksheetTableColumn column = table.Columns[tableColumnIndex];
			Debug.Assert(column.SortRegion.Equals(rfx), "The rfx value is incorrect.");

			SortDirection sortDirection = fSortDes ? SortDirection.Descending : SortDirection.Ascending;

			SortCondition sortCondition;
			switch (sortOn)
			{
				case ST_SortBy.value:
				case ST_SortBy.cellColor:
				case ST_SortBy.fontColor:
					sortCondition = this.ReadCondDataValue(sortOn, sortDirection);
					break;

				case ST_SortBy.icon:
					ST_IconSetType? iconSet;
					uint iconIndex;
					this.ReadCFFlag(out iconSet, out iconIndex);

					if (iconSet.HasValue)
						sortCondition = new IconSortCondition(iconSet.Value, iconIndex, sortDirection);
					else
						sortCondition = null;
					break;

				default:
					Utilities.DebugFail("Unknown ST_SortBy value: " + sortOn);
					return;
			}

			int cchSt = this.ReadInt32();

			if (sortCondition == null)
				return;

			if (sortOn != ST_SortBy.value)
			{
				Debug.Assert(cchSt == 0, "The cchSt value is incorrect.");
			}
			else
			{
				if (cchSt > 0)
				{
					string stSslist = this.ReadXLUnicodeStringNoCch((ushort)cchSt);
					sortCondition = new CustomListSortCondition(sortDirection, stSslist.Split(','));
				}
			}

			table.SortSettings.SortConditions.Add(column, sortCondition);
		}

		// http://msdn.microsoft.com/en-us/library/dd951309(v=office.12).aspx
		public void WriteSortCond12(WorksheetTableColumn column, SortCondition sortCondition)
		{
			this.WriteFrtRefHeader(null, BIFF8RecordType.CONTINUEFRT12);

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, sortCondition.SortDirection == SortDirection.Descending, 0); // fSortDes 
			Utilities.AddBits(ref temp16, (int)sortCondition.SortByValue, 1, 4); // sortOn
			this.Write(temp16);

			this.WriteRFX(column.SortRegion);

			switch (sortCondition.SortByValue)
			{
				case ST_SortBy.value:
				case ST_SortBy.cellColor:
				case ST_SortBy.fontColor:
					this.WriteCondDataValue(column, sortCondition);
					break;

				case ST_SortBy.icon:
					IconSortCondition iconSortCondition = sortCondition as IconSortCondition;
					if (iconSortCondition != null)
						this.WriteCFFlag(iconSortCondition.IconSet, iconSortCondition.IconIndex);
					else
						Utilities.DebugFail("We should have an IconSortCondition instance here.");
					break;

				default:
					Utilities.DebugFail("Unknown ST_SortBy value: " + sortCondition.SortByValue);
					return;
			}

			CustomListSortCondition customListSortCondition = sortCondition as CustomListSortCondition;
			if (customListSortCondition == null)
			{
				this.Write((int)0); // cchSt
			}
			else
			{
				string customList = customListSortCondition.GetListString();
				this.Write(customList.Length); // cchSt
				this.WriteXLUnicodeStringNoCch(customList);
			}
		}

		#endregion // Read/WriteSortCond12

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteStxp

		// http://msdn.microsoft.com/en-us/library/dd945272(v=office.12).aspx
		private Stxp ReadStxp()
		{
			Stxp stxp = new Stxp();
			stxp.twpHeight = this.ReadInt32();
			stxp.ts = this.ReadTs();
			stxp.bls = this.ReadUInt16();
			stxp.sss = this.ReadUInt16();
			stxp.uls = (byte)this.ReadByte();
			stxp.bFamily = (byte)this.ReadByte();
			stxp.bCharSet = (byte)this.ReadByte();
			byte unused = (byte)this.ReadByte();
			return stxp;
		}

		// http://msdn.microsoft.com/en-us/library/dd945272(v=office.12).aspx
		private void WriteStxp(Stxp stxp)
		{
			this.Write(stxp.twpHeight);
			this.WriteTs(stxp.ts);
			this.Write(stxp.bls);
			this.Write(stxp.sss);
			this.WriteByte(stxp.uls);
			this.WriteByte(stxp.bFamily);
			this.WriteByte(stxp.bCharSet);
			this.WriteByte(0); // unused
		}

		#endregion // Read/WriteStxp

		// MD 2/18/12 - 12.1 - Table Support
		#region Read/WriteTableFeatureType

		// http://msdn.microsoft.com/en-us/library/dd905609(v=office.12).aspx
		public WorksheetTable ReadTableFeatureType(Worksheet worksheet, WorksheetRegion tableRegion)
		{
			SourceType lt = (SourceType)this.ReadUInt32();

			if (lt != SourceType.Range)
			{
				Utilities.DebugFail("Not sure how to load other types of tables.");
				return null;
			}

			uint idList = this.ReadUInt32();
			uint crwHeader = this.ReadUInt32();
			Debug.Assert(crwHeader == 0 || crwHeader == 1, "The crwHeader value is incorrect.");
			uint crwTotals = this.ReadUInt32();
			Debug.Assert(crwTotals == 0 || crwTotals == 1, "The crwTotals value is incorrect.");
			uint idFieldNext = this.ReadUInt32();
			uint cbFSData = this.ReadUInt32();
			Debug.Assert(cbFSData == 0x40, "The cbFSData value is incorrect.");
			ushort rupBuilt = this.ReadUInt16();
			ushort unused1 = this.ReadUInt16();

			ushort temp16 = this.ReadUInt16();
			bool unused2 = Utilities.TestBit(temp16, 0);
			bool fAutoFilter = Utilities.TestBit(temp16, 1);
			bool fPersistAutoFilter = Utilities.TestBit(temp16, 2);
			bool fShowInsertRow = Utilities.TestBit(temp16, 3);
			bool fInsertRowInsCells = Utilities.TestBit(temp16, 4);
			bool fLoadPldwIdDeleted = Utilities.TestBit(temp16, 5);
			bool fShownTotalRow = Utilities.TestBit(temp16, 6);
			bool reserved1 = Utilities.TestBit(temp16, 7);
			Debug.Assert(reserved1 == false, "the reserved1 value is incorrect.");
			bool fNeedsCommit = Utilities.TestBit(temp16, 8);
			bool fSingleCell = Utilities.TestBit(temp16, 9);
			Debug.Assert(fSingleCell == false, "fSingleCell should only be True when lt is XML.");
			bool reserved2 = Utilities.TestBit(temp16, 10);
			Debug.Assert(reserved2 == false, "the reserved2 value is incorrect.");
			bool fApplyAutoFilter = Utilities.TestBit(temp16, 11);
			bool fForceInsertToBeVis = Utilities.TestBit(temp16, 12);
			bool fCompressedXml = Utilities.TestBit(temp16, 13);
			bool fLoadCSPName = Utilities.TestBit(temp16, 14);
			bool fLoadPldwIdChanged = Utilities.TestBit(temp16, 15);

			temp16 = this.ReadUInt16();
			int verXL = Utilities.GetBits(temp16, 0, 3);
			bool fLoadEntryId = Utilities.TestBit(temp16, 4);
			bool fLoadPllstclInvalid = Utilities.TestBit(temp16, 5);
			bool fGoodRupBld = Utilities.TestBit(temp16, 6);
			bool unused3 = Utilities.TestBit(temp16, 7);
			bool fPublished = Utilities.TestBit(temp16, 8);
			int reserved3 = Utilities.GetBits(temp16, 9, 15);

			uint lPosStmCache = this.ReadUInt32();
			uint cbStmCache = this.ReadUInt32();
			uint cchStmCache = this.ReadUInt32();
			uint lem = this.ReadUInt32();
			Debug.Assert(lem == 0, "the lem value is incorrect.");

			byte[] rgbHashParam = this.ReadBytes(16);
			string rgbName = this.ReadXLUnicodeString();
			ushort cFieldData = this.ReadUInt16();
			Debug.Assert(1 <= cFieldData && cFieldData <= 0x100, "cFieldData");

			if (fLoadCSPName)
			{
				string cSPName = this.ReadXLUnicodeString();
			}

			if (fLoadEntryId)
			{
				string entryId = this.ReadXLUnicodeString();
				Debug.Assert(entryId == idList.ToString(), "The entryId value is incorrect.");
			}

			WorksheetTable table = new WorksheetTable(rgbName, idList, tableRegion.FirstRow, tableRegion.LastRow, tableRegion.FirstColumnInternal, tableRegion.LastColumnInternal);

			table.IsHeaderRowVisible = (crwHeader != 0);

			if (table.IsHeaderRowVisible)
				table.IsFilterUIVisible = fAutoFilter;

			table.IsTotalsRowVisible = (crwTotals != 0);
			table.NextColumnId = idFieldNext;
			table.IsInsertRowVisible = fShowInsertRow && fForceInsertToBeVis;
			table.WereCellsShiftedToShowInsertRow = fInsertRowInsCells;
			table.HasTotalsRowEverBeenVisible = fShownTotalRow;
			table.Published = fPublished;
			table.Style = this.Manager.Workbook.DefaultTableStyle;

			// fieldData 
			for (int i = 0; i < cFieldData; i++)
				this.ReadFeat11FieldDataItem(worksheet, table, fSingleCell, crwHeader);

			if (fLoadPldwIdDeleted)
			{
				Utilities.DebugFail("Load idDeleted.");
			}

			if (fLoadPldwIdChanged)
			{
				Utilities.DebugFail("Load idChanged.");
			}

			if (fLoadPllstclInvalid)
			{
				Utilities.DebugFail("Load cellInvalid.");
			}

			return table;
		}

		// http://msdn.microsoft.com/en-us/library/dd905609(v=office.12).aspx
		public void WriteTableFeatureType(WorksheetTable table, SortedList<int, TableColumnFilterData> columnsFilterData)
		{
			bool fSingleCell = false;

			this.Write((uint)SourceType.Range); // lt
			this.Write((uint)table.Id); // idList
			this.Write((uint)(table.IsHeaderRowVisible ? 1 : 0)); // crwHeader
			this.Write((uint)(table.IsTotalsRowVisible ? 1 : 0)); // crwTotals
			this.Write((uint)table.NextColumnId); // idFieldNext
			this.Write((uint)0x40); // cbFSData
			this.Write((ushort)0); // rupBuild
			this.Write((ushort)0); // unused1

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, table.IsFilterUIVisible, 1); // fAutoFilter
			Utilities.SetBit(ref temp16, table.IsFilterUIVisible, 2); // fPersistAutoFilter
			Utilities.SetBit(ref temp16, table.IsInsertRowVisible, 3); // fShowInsertRow
			Utilities.SetBit(ref temp16, table.WereCellsShiftedToShowInsertRow, 4); // fInsertRowInsCells
			Utilities.SetBit(ref temp16, table.HasTotalsRowEverBeenVisible, 6); // fShownTotalRow
			Utilities.SetBit(ref temp16, fSingleCell, 9);
			Utilities.SetBit(ref temp16, true, 11); // fApplyAutoFilter
			Utilities.SetBit(ref temp16, table.IsInsertRowVisible, 12); // fForceInsertToBeVis
			this.Write(temp16);

			temp16 = 0;
			Utilities.AddBits(ref temp16, 14, 0, 3); // verXL
			Utilities.SetBit(ref temp16, true, 4); // fLoadEntryId
			Utilities.SetBit(ref temp16, false, 6); // fGoodRupBld
			Utilities.SetBit(ref temp16, table.Published, 8); // fPublished
			this.Write(temp16);

			this.Write((uint)0); // lPosStmCache
			this.Write((uint)0); // cbStmCache
			this.Write((uint)0); // cchStmCache
			this.Write((uint)0); // lem

			this.Write(new byte[16]); // rgbHashParam
			this.WriteXLUnicodeString(table.Name); // rgbName
			this.Write((ushort)table.Columns.Count); // cFieldData

			this.WriteXLUnicodeString(table.Id.ToString()); // entryId

			// fieldData 
			for (int i = 0; i < table.Columns.Count; i++)
				this.WriteFeat11FieldDataItem(table.Columns[i], columnsFilterData, fSingleCell);
		}

		#endregion // Read/WriteTableFeatureType

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteTs

		// http://msdn.microsoft.com/en-us/library/dd953356(v=office.12).aspx
		private Ts ReadTs()
		{
			Ts ts = new Ts();
			uint temp32 = this.ReadUInt32();
			ts.ftsItalic = Utilities.TestBit(temp32, 1);
			ts.ftsStrikeout = Utilities.TestBit(temp32, 7);
			return ts;
		}

		// http://msdn.microsoft.com/en-us/library/dd953356(v=office.12).aspx
		private void WriteTs(Ts ts)
		{
			uint temp32 = 0;
			Utilities.SetBit(ref temp32, ts.ftsItalic, 1);
			Utilities.SetBit(ref temp32, ts.ftsStrikeout, 7);
			this.Write(temp32);
		}

		#endregion // Read/WriteTs

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteUnderline

		// http://msdn.microsoft.com/en-us/library/dd907891(v=office.12).aspx
		private FontUnderlineStyle ReadUnderline()
		{
			return (FontUnderlineStyle)this.ReadUInt16();
		}

		// http://msdn.microsoft.com/en-us/library/dd907891(v=office.12).aspx
		private void WriteUnderline(FontUnderlineStyle underlineStyle)
		{
			this.Write((ushort)underlineStyle);
		}

		#endregion // Read/WriteUnderline

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteUnicodeString

		public string ReadUnicodeString(ushort cch)
		{
			byte[] data = this.ReadBytes(cch * 2);
			return Encoding.Unicode.GetString(data);
		}

		public void WriteUnicodeString(string value)
		{
			this.Write(Encoding.Unicode.GetBytes(value));
		}

		#endregion // Read/WriteUnicodeString

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region Read/WriteXFExtGradient

		// http://msdn.microsoft.com/en-us/library/dd926703(v=office.12).aspx
		public CellFillGradient ReadXFExtGradient()
		{
			// http://msdn.microsoft.com/en-us/library/dd905597(v=office.12).aspx
			#region Load The XFPropGradient

			int type = this.ReadInt32();
			Debug.Assert(type == 0 || type == 1, "The type is not valid.");

			double numDegree = this.ReadDouble();
			double numFillToLeft = this.ReadDouble();
			double numFillToRight = this.ReadDouble();
			double numFillToTop = this.ReadDouble();
			double numFillToBottom = this.ReadDouble();

			#endregion // Load The XFPropGradient

			uint cGradStops = this.ReadUInt32();

			CellFillGradientStop[] rgGradStops = new CellFillGradientStop[cGradStops];
			for (int i = 0; i < cGradStops; i++)
				rgGradStops[i] = this.ReadGradStop();

			if (type == 1)
				return new CellFillRectangularGradient(numFillToLeft, numFillToTop, numFillToRight, numFillToBottom, rgGradStops);

			return new CellFillLinearGradient(numDegree, rgGradStops);
		}

		// http://msdn.microsoft.com/en-us/library/dd926703(v=office.12).aspx
		public void WriteXFExtGradient(CellFillGradient gradientFill)
		{
			// http://msdn.microsoft.com/en-us/library/dd905597(v=office.12).aspx
			#region Save The XFPropGradient

			int type = 0;
			double numDegree = 0;
			double numFillToLeft = 0;
			double numFillToRight = 0;
			double numFillToTop = 0;
			double numFillToBottom = 0;

			CellFillLinearGradient linearGradient = gradientFill as CellFillLinearGradient;
			CellFillRectangularGradient rectangularGradient = gradientFill as CellFillRectangularGradient;
			if (linearGradient != null)
			{
				type = 0;
				numDegree = linearGradient.Angle;
			}
			else if (rectangularGradient != null)
			{
				type = 1;
				numFillToLeft = rectangularGradient.Left;
				numFillToRight = rectangularGradient.Right;
				numFillToTop = rectangularGradient.Top;
				numFillToBottom = rectangularGradient.Bottom;
			}
			else
			{
				Utilities.DebugFail("Unknown gradient type.");
			}

			this.Write(type);
			this.Write(numDegree);
			this.Write(numFillToLeft);
			this.Write(numFillToRight);
			this.Write(numFillToTop);
			this.Write(numFillToBottom);

			#endregion // Save The XFPropGradient

			this.Write((uint)gradientFill.Stops.Count);

			for (int i = 0; i < gradientFill.Stops.Count; i++)
				this.WriteGradStop(gradientFill.Stops[i]);
		}

		#endregion // Read/WriteXFExtGradient

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteXFExtNoFRT

		// http://msdn.microsoft.com/en-us/library/dd947091(v=office.12).aspx
		public void ReadXFExtNoFRT(WorksheetCellFormatData dxfn)
		{
			ushort reserved1 = this.ReadUInt16();
			Debug.Assert(reserved1 == 0, "The reserved1 value is incorrect.");

			ushort reserved2 = this.ReadUInt16();
			Debug.Assert(reserved2 == 0xFFFF, "The reserved2 value is incorrect.");

			ushort reserved3 = this.ReadUInt16();
			Debug.Assert(reserved3 == 0, "The reserved3 value is incorrect.");

			ushort cexts = this.ReadUInt16();
			for (int i = 0; i < cexts; i++)
			{
				ExtProp extProp = this.ReadExtProp();
				extProp.ApplyTo(this.ManagerBiff8, dxfn);
			}
		}

		// http://msdn.microsoft.com/en-us/library/dd947091(v=office.12).aspx
		public void WriteXFExtNoFRT(WorksheetCellFormatData format)
		{
			List<ExtProp> props = format.GetXFEXTProps();
			if (props.Count == 0)
				return;

			this.Write((ushort)0); // reserved1
			this.Write((ushort)0xFFFF); // reserved2
			this.Write((ushort)0); // reserved3
			this.Write((ushort)props.Count); // cexts 

			// rgExt
			for (int i = 0; i < props.Count; i++)
				this.WriteExtProp(props[i]);
		}

		#endregion // Read/WriteXFExtNoFRT

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteXFProp

		// http://msdn.microsoft.com/en-us/library/dd953723(v=office.12).aspx
		public XFProp ReadXFProp()
		{
			XFPropType xfPropType = (XFPropType)this.ReadUInt16();
			ushort cb = this.ReadUInt16();

			switch (xfPropType)
			{
				case XFPropType.BottomBorder:
				case XFPropType.DiagonalBorder:
				case XFPropType.HorizontalBorder:
				case XFPropType.LeftBorder:
				case XFPropType.RightBorder:
				case XFPropType.TopBorder:
				case XFPropType.VerticalBorder:
					WorkbookColorInfo borderColor;
					CellBorderLineStyle borderStyle;
					this.ReadXFPropBorder(out borderColor, out borderStyle);
					return new XFPropBorder(xfPropType, borderColor, borderStyle);

				case XFPropType.CellMerged:
				case XFPropType.DiagonalDownBorder:
				case XFPropType.DiagonalUpBorder:
				case XFPropType.FontCondensed:
				case XFPropType.FontExtended:
				case XFPropType.FontItalic:
				case XFPropType.FontOutline:
				case XFPropType.FontShadow:
				case XFPropType.FontStrikethrough:
				case XFPropType.Hidden:
				case XFPropType.JustifyDistributed:
				case XFPropType.Locked:
				case XFPropType.ShrinkToFit:
				case XFPropType.WrappedText:
					return new XFPropBool(xfPropType, (byte)this.ReadByte());

				case XFPropType.ForegroundColor:
				case XFPropType.BackgroundColor:
				case XFPropType.FontColor:
					return new XFPropColor(xfPropType, this.ReadXFPropColor());

				case XFPropType.FontBold:
					return new XFPropFontBold(this.ReadUInt16());

				case XFPropType.FontCharacterSet:
				case XFPropType.FontFamily:
				case XFPropType.ReadingOrder:
					return new XFPropByte(xfPropType, (byte)this.ReadByte());

				case XFPropType.FontHeight:
					return new XFPropFontHeight(this.ReadUInt32());

				case XFPropType.FontName:
					return new XFPropFontName(this.ReadLPWideString());

				case XFPropType.FontUnderline:
					return new XFPropFontUnderline(this.ReadUnderline());

				case XFPropType.FontScheme:
					return new XFPropFontScheme(this.ReadFontScheme());

				case XFPropType.FontSubscriptSuperscript:
					return new XFPropFontSubscriptSuperscript(this.ReadScript());

				case XFPropType.GradientFill:
					return this.ReadXFPropGradientFill();

				case XFPropType.GradientStop:
					return this.ReadXFPropGradientStop();

				case XFPropType.HorizontalAlignment:
					return new XFPropHorizontalAlignment(this.ReadHorizAlign());

				case XFPropType.NumberFormat:
					ushort numFmtId;
					string format;
					this.ReadFormat(out numFmtId, out format);
					return new XFPropNumberFormat(numFmtId, format);

				case XFPropType.NumberFormatId:
					return new XFPropNumberFormatId(this.ReadUInt16());

				case XFPropType.PatternFill:
					FillPatternStyle fillPatternStyle = (FillPatternStyle)this.ReadByte();
					Debug.Assert(Enum.IsDefined(typeof(FillPatternStyle), fillPatternStyle), "This is not a valid FillPatternStyle.");
					return new XFPropFillPattern(fillPatternStyle);

				case XFPropType.TextIndentationLevel:
					return new XFPropTextIndentationLevel(this.ReadUInt16());

				case XFPropType.TextIndentationLevelRelative:
					return new XFPropTextIndentationLevelRelative(this.ReadInt16());

				case XFPropType.TextRotation:
					return new XFPropTextRotation((byte)this.ReadByte());

				case XFPropType.VerticalAlignment:
					return new XFPropVerticalAlignment(this.ReadVertAlign());

				default:
					Utilities.DebugFail("Unknown XFPropType: " + xfPropType);
					return null;
			}
		}

		// http://msdn.microsoft.com/en-us/library/dd953723(v=office.12).aspx
		public void WriteXFProp(XFProp xfProp)
		{
			long recordStart = this.Position;

			this.Write((ushort)xfProp.Type); // xfPropType
			this.Write((ushort)0); // cb placeholder

			switch (xfProp.Type)
			{
				case XFPropType.BottomBorder:
				case XFPropType.DiagonalBorder:
				case XFPropType.HorizontalBorder:
				case XFPropType.LeftBorder:
				case XFPropType.RightBorder:
				case XFPropType.TopBorder:
				case XFPropType.VerticalBorder:
					XFPropBorder xfPropBorder = (XFPropBorder)xfProp;
					WorkbookColorInfo borderColor = xfPropBorder.BorderColor;
					CellBorderLineStyle borderStyle = xfPropBorder.BorderStyle;
					this.WriteXFPropBorder(borderColor, borderStyle);
					break;

				case XFPropType.CellMerged:
				case XFPropType.DiagonalDownBorder:
				case XFPropType.DiagonalUpBorder:
				case XFPropType.FontCondensed:
				case XFPropType.FontExtended:
				case XFPropType.FontItalic:
				case XFPropType.FontOutline:
				case XFPropType.FontShadow:
				case XFPropType.FontStrikethrough:
				case XFPropType.Hidden:
				case XFPropType.JustifyDistributed:
				case XFPropType.Locked:
				case XFPropType.ShrinkToFit:
				case XFPropType.WrappedText:
					this.Write(((XFPropBool)xfProp).ValueByte);
					break;

				case XFPropType.ForegroundColor:
				case XFPropType.BackgroundColor:
					this.WriteXFPropColor(((XFPropColor)xfProp).ColorInfo, ColorableItem.CellFill);
					break;

				case XFPropType.FontBold:
					this.Write(((XFPropFontBold)xfProp).FontWeight);
					break;

				case XFPropType.FontCharacterSet:
				case XFPropType.FontFamily:
				case XFPropType.ReadingOrder:
					this.Write(((XFPropByte)xfProp).Value);
					break;

				case XFPropType.FontColor:
					this.WriteXFPropColor(((XFPropColor)xfProp).ColorInfo, ColorableItem.CellFont);
					break;

				case XFPropType.FontHeight:
					this.Write(((XFPropFontHeight)xfProp).FontHeight);
					break;

				case XFPropType.FontName:
					this.WriteLPWideString(((XFPropFontName)xfProp).FontName);
					break;

				case XFPropType.FontUnderline:
					this.WriteUnderline(((XFPropFontUnderline)xfProp).UnderlineStyle);
					break;

				case XFPropType.FontScheme:
					this.WriteFontScheme(((XFPropFontScheme)xfProp).FontScheme);
					break;

				case XFPropType.FontSubscriptSuperscript:
					this.WriteScript(((XFPropFontSubscriptSuperscript)xfProp).Style);
					break;

				case XFPropType.GradientFill:
					this.WriteXFPropGradientFill((XFPropGradientFill)xfProp);
					break;

				case XFPropType.GradientStop:
					this.WriteXFPropGradientStop((XFPropGradientStop)xfProp);
					break;

				case XFPropType.HorizontalAlignment:
					this.WriteHorizAlign(((XFPropHorizontalAlignment)xfProp).Alignment);
					break;

				case XFPropType.NumberFormat:
					XFPropNumberFormat xfPropNumberFormat = xfProp as XFPropNumberFormat;
					ushort numFmtId = xfPropNumberFormat.NumFmtId;
					string format = xfPropNumberFormat.Format;
					this.ReadFormat(out numFmtId, out format);
					break;

				case XFPropType.NumberFormatId:
					this.Write(((XFPropNumberFormatId)xfProp).FmtId);
					break;

				case XFPropType.PatternFill:
					this.Write((byte)((XFPropFillPattern)xfProp).FillPattern);
					break;

				case XFPropType.TextIndentationLevel:
					this.Write(((XFPropTextIndentationLevel)xfProp).Indent);
					break;

				case XFPropType.TextIndentationLevelRelative:
					this.Write(((XFPropTextIndentationLevelRelative)xfProp).IndentOffset);
					break;

				case XFPropType.TextRotation:
					this.Write(((XFPropTextRotation)xfProp).Rotation);
					break;

				case XFPropType.VerticalAlignment:
					this.WriteVertAlign(((XFPropVerticalAlignment)xfProp).VerticalAlignment);
					break;

				default:
					Utilities.DebugFail("Unknown XFPropType: " + xfProp.Type);
					break;
			}

			long recordEnd = this.Position;
			ushort cb = (ushort)(recordEnd - recordStart);

			this.Position = recordStart + 2;
			this.Write(cb);
			this.Position = recordEnd;
		}

		#endregion // Read/WriteXFProp

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteXFPropBorder

		// http://msdn.microsoft.com/en-us/library/dd945965(v=office.12).aspx
		private void ReadXFPropBorder(out WorkbookColorInfo borderColor, out CellBorderLineStyle borderStyle)
		{
			borderColor = this.ReadXFPropColor(); // color 
			borderStyle = (CellBorderLineStyle)this.ReadUInt16(); // dgBorder 
			Debug.Assert(Enum.IsDefined(typeof(CellBorderLineStyle), borderStyle), "The dgBorder value is incorrect.");
		}

		// http://msdn.microsoft.com/en-us/library/dd945965(v=office.12).aspx
		private void WriteXFPropBorder(WorkbookColorInfo borderColor, CellBorderLineStyle borderStyle)
		{
			this.WriteXFPropColor(borderColor, ColorableItem.CellBorder); // color 
			this.Write((ushort)borderStyle); // dgBorder 
		}

		#endregion // Read/WriteXFPropBorder

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteXFPropColor

		// http://msdn.microsoft.com/en-us/library/dd906791(v=office.12).aspx
		public WorkbookColorInfo ReadXFPropColor()
		{
			byte temp8 = (byte)this.ReadByte();
			bool fValidRGBA = Utilities.TestBit(temp8, 0);
			Debug.Assert(fValidRGBA, "The fValidRGBA value is incorrect.");
			XColorType xclrType = (XColorType)Utilities.GetBits(temp8, 1, 7);

			byte icv = (byte)this.ReadByte();
			short nTintShade = this.ReadInt16();
			uint dwRgba = this.ReadUInt32();

			double? tint = null;
			if (nTintShade != 0)
				tint = (double)nTintShade / Int16.MaxValue;

			uint xClrValue;
			if (xclrType == XColorType.Indexed || xclrType == XColorType.Themed)
				xClrValue = icv;
			else if (xclrType == XColorType.RGB)
				xClrValue = dwRgba;
			else
				xClrValue = 0;

			return this.GetColorInfoFromRawValues(xclrType, xClrValue, tint);
		}

		// http://msdn.microsoft.com/en-us/library/dd906791(v=office.12).aspx
		public void WriteXFPropColor(WorkbookColorInfo colorInfo, ColorableItem item)
		{
			XColorType colorType;
			double tint;
			uint xClrValue;
			this.GetRawColorInfoValues(colorInfo, item, out colorType, out tint, out xClrValue);

			byte icv = 0xFF;
			uint dwRgba = xClrValue;

			if (colorType == XColorType.Themed || colorType == XColorType.Indexed)
				icv = (byte)xClrValue;

			short nTintShade = (short)MathUtilities.MidpointRoundingAwayFromZero(tint * Int16.MaxValue);

			byte temp8 = 0;
			Utilities.SetBit(ref temp8, true, 0); // fValidRGBA
			Utilities.AddBits(ref temp8, (int)colorType, 1, 7); // xclrType
			this.Write(temp8);

			this.Write(icv);
			this.Write(nTintShade);
			this.Write(dwRgba);
		}

		#endregion // Read/WriteXFPropColor

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteXFPropGradientFill

		// http://msdn.microsoft.com/en-us/library/dd905597(v=office.12).aspx
		public XFPropGradientFill ReadXFPropGradientFill()
		{
			uint type = this.ReadUInt32();
			Debug.Assert(type == 0 || type == 1, "The type is incorrect.");

			double numDegree = this.ReadDouble();
			double numFillToLeft = this.ReadDouble();
			double numFillToRight = this.ReadDouble();
			double numFillToTop = this.ReadDouble();
			double numFillToBottom = this.ReadDouble();
			return new XFPropGradientFill(type != 0, numDegree, numFillToLeft, numFillToRight, numFillToTop, numFillToBottom);
		}

		// http://msdn.microsoft.com/en-us/library/dd905597(v=office.12).aspx
		public void WriteXFPropGradientFill(XFPropGradientFill xfProp)
		{
			this.Write((uint)(xfProp.IsRectangular ? 1 : 0)); // type
			this.Write(xfProp.NumDegree);
			this.Write(xfProp.NumFillToLeft);
			this.Write(xfProp.NumFillToRight);
			this.Write(xfProp.NumFillToTop);
			this.Write(xfProp.NumFillToBottom);
		}

		#endregion // Read/WriteXFPropGradientFill

		// MD 2/22/12 - 12.1 - Table Support
		#region Read/WriteXFPropGradientStop

		// http://msdn.microsoft.com/en-us/library/dd910618(v=office.12).aspx
		public XFPropGradientStop ReadXFPropGradientStop()
		{
			ushort unused = this.ReadUInt16();
			double numPosition = this.ReadDouble();
			WorkbookColorInfo color = this.ReadXFPropColor();
			return new XFPropGradientStop(color, numPosition);
		}

		// http://msdn.microsoft.com/en-us/library/dd910618(v=office.12).aspx
		public void WriteXFPropGradientStop(XFPropGradientStop xfProp)
		{
			this.Write((ushort)0); // unused
			this.Write(xfProp.NumPosition);
			this.WriteXFPropColor(xfProp.Color, ColorableItem.CellFill);
		}

		#endregion // Read/WriteXFPropGradientStop

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteXFProps

		// http://msdn.microsoft.com/en-us/library/dd909797(v=office.12).aspx
		public WorksheetCellFormatData ReadXFProps(bool fUIFill)
		{
			WorksheetCellFormatData format = this.Manager.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);
			this.ReadXFProps(format, fUIFill);
			return format;
		}

		// http://msdn.microsoft.com/en-us/library/dd909797(v=office.12).aspx
		public void ReadXFProps(WorksheetCellFormatData format, bool fUIFill)
		{
			ushort reserved = this.ReadUInt16();
			Debug.Assert(reserved == 0, "We were expecting reserved to be 0.");

			ushort cprops = this.ReadUInt16();

			FillPatternStyle? fillPattern = null;
			WorkbookColorInfo backgroundColor = null;
			WorkbookColorInfo foregroundColor = null;

			XFPropGradientFill gradient = null;
			List<CellFillGradientStop> stops = new List<CellFillGradientStop>();

			for (int i = 0; i < cprops; i++)
			{
				XFProp prop = this.ReadXFProp();

				if (prop == null)
					continue;

				switch (prop.Type)
				{
					case XFPropType.PatternFill:
						fillPattern = ((XFPropFillPattern)prop).FillPattern;
						break;

					case XFPropType.BackgroundColor:
						backgroundColor = ((XFPropColor)prop).ColorInfo;
						break;

					case XFPropType.ForegroundColor:
						foregroundColor = ((XFPropColor)prop).ColorInfo;
						break;

					case XFPropType.GradientFill:
						gradient = (XFPropGradientFill)prop;
						break;

					case XFPropType.GradientStop:
						XFPropGradientStop xfStop = (XFPropGradientStop)prop;
						stops.Add(new CellFillGradientStop(xfStop.Color, xfStop.NumPosition));
						break;

					default:
						prop.ApplyTo(format);
						break;
				}
			}

			Debug.Assert(fillPattern.HasValue == false || gradient == null, "We cannot have both a fill pattern and gradient.");
			Debug.Assert(gradient == null || stops.Count >= 2, "We should have at least 2 gradient stops.");

			if (gradient != null && stops.Count >= 2)
			{
				if (gradient.IsRectangular)
				{
					format.Fill = new CellFillRectangularGradient(
						gradient.NumFillToLeft,
						gradient.NumFillToTop,
						gradient.NumFillToRight,
						gradient.NumFillToBottom,
						stops.ToArray());
				}
				else
				{
					format.Fill = new CellFillLinearGradient(
						gradient.NumDegree,
						stops.ToArray());
				}
			}
			else if (fillPattern.HasValue || backgroundColor != null || foregroundColor != null)
			{
				if (fillPattern.HasValue == false)
					fillPattern = FillPatternStyle.Solid;

				Debug.Assert(fUIFill != format.DoesReverseColorsForSolidFill, "This is unexpected. May have to swap solid colors manually here if these differ.");
				format.Fill = new CellFillPattern(backgroundColor, foregroundColor, fillPattern.Value, format);
			}
		}

		// http://msdn.microsoft.com/en-us/library/dd909797(v=office.12).aspx
		public void WriteXFProps(WorksheetCellFormatData format)
		{
			this.Write((ushort)0); // reserved

			List<XFProp> xfProps = format.GetXFProps();
			this.Write((ushort)xfProps.Count); // cprops

			for (int i = 0; i < xfProps.Count; i++)
				this.WriteXFProp(xfProps[i]);
		}

		#endregion // Read/WriteXFProps

		// MD 2/21/12 - 12.1 - Table Support
		#region Read/WriteVertAlign

		// http://msdn.microsoft.com/en-us/library/dd944020(v=office.12).aspx
		public VerticalCellAlignment ReadVertAlign()
		{
			return (VerticalCellAlignment)this.ReadByte();
		}

		// http://msdn.microsoft.com/en-us/library/dd944020(v=office.12).aspx
		public void WriteVertAlign(VerticalCellAlignment value)
		{
			Debug.Assert(value != VerticalCellAlignment.Default, "The default value cannot be used here.");
			this.Write((byte)value);
		}

		#endregion // Read/WriteVertAlign

		// MD 2/18/12 - 12.1 - Table Support
		#region Read/WriteXLUnicodeString

		// http://msdn.microsoft.com/en-us/library/dd922754(v=office.12).aspx
		public string ReadXLUnicodeString()
		{
			ushort cch = this.ReadUInt16();
			return this.ReadXLUnicodeStringNoCch(cch);
		}

		// http://msdn.microsoft.com/en-us/library/dd922754(v=office.12).aspx
		public void WriteXLUnicodeString(string value)
		{
			this.Write((ushort)value.Length);
			this.WriteXLUnicodeStringNoCch(value);
		}

		#endregion // Read/WriteXLUnicodeString

		// MD 2/19/12 - 12.1 - Table Support
		#region Read/WriteXLUnicodeStringNoCch

		// http://msdn.microsoft.com/en-us/library/dd910585(v=office.12).aspx
		public string ReadXLUnicodeStringNoCch(ushort cch)
		{
			byte temp8 = (byte)this.ReadByte();
			bool fHighByte = Utilities.TestBit(temp8, 0);

			int reserved = Utilities.GetBits(temp8, 1, 7);
			Debug.Assert(reserved == 0, "The reserved field is incorrect.");

			byte[] data = new byte[cch * 2];

			for (int bytePosition = 0; bytePosition < data.Length; bytePosition++)
			{
				data[bytePosition] = (byte)this.ReadByte();

				if (fHighByte == false)
					data[++bytePosition] = 0;
			}

			return Encoding.Unicode.GetString(data);
		}

		// http://msdn.microsoft.com/en-us/library/dd910585(v=office.12).aspx
		public void WriteXLUnicodeStringNoCch(string value)
		{
			this.WriteXLUnicodeStringNoCch(value, false);
		}

		public void WriteXLUnicodeStringNoCch(string value, bool force2BytesPerChar)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(value);

			bool fHighByte = false;

			byte[] lowBytes = new byte[bytes.Length / 2];
			if (force2BytesPerChar)
			{
				fHighByte = true;
			}
			else
			{

				for (int i = 1; i < bytes.Length; i += 2)
				{
					if (bytes[i] != 0)
					{
						fHighByte = true;
						break;
					}

					lowBytes[i / 2] = bytes[i - 1];
				}
			}

			byte[] rgb = fHighByte ? bytes : lowBytes;
			this.WriteByte(Convert.ToByte(fHighByte));
			this.Write(rgb);
		}

		#endregion // Read/WriteXLUnicodeStringNoCch

		// MD 2/20/12 - 12.1 - Table Support
		#region Read/WriteXnum

		// http://msdn.microsoft.com/en-us/library/dd953512(v=office.12).aspx
		private double ReadXnum()
		{
			return this.ReadDouble();
		}

		// http://msdn.microsoft.com/en-us/library/dd953512(v=office.12).aspx
		private void WriteXnum(double value)
		{
			this.Write(value);
		}

		#endregion // Read/WriteXnum

		// MD 2/20/12 - 12.1 - Table Support
		#region WriteDXFN12ListAndSizeField

		private void WriteDXFN12ListAndSizeField(long cbPosition, WorksheetCellFormatData dxf)
		{
			if (dxf == null)
				return;

			long dxfHeaderStart = this.Position;
			this.WriteDXFN12List(dxf);
			long dxfHeaderEnd = this.Position;
			int cbdxfHeader = (int)(dxfHeaderEnd - dxfHeaderStart);

			this.Position = cbPosition;
			this.Write(cbdxfHeader);
			this.Position = dxfHeaderEnd;
		}

		#endregion // WriteDXFN12ListAndSizeField

		#endregion Methods

		#region Properties

		#region ManagerBiff8

		public BIFF8WorkbookSerializationManager ManagerBiff8
		{
			get { return (BIFF8WorkbookSerializationManager)this.Manager; }
		}

		#endregion // ManagerBiff8

		#region NextBlockType

		public BIFF8RecordType NextBlockType
		{
			get { return (BIFF8RecordType)this.NextBlockTypeInternal; }
			set { this.NextBlockTypeInternal = (int)value; }
		}

		#endregion NextBlockType

		#region RecordType

		public BIFF8RecordType RecordType
		{
			get { return (BIFF8RecordType)this.RecordTypeInternal; }
		}

		#endregion RecordType 

		#endregion Properties


		// MD 2/20/12 - 12.1 - Table Support
		#region AFDOper

		// http://msdn.microsoft.com/en-us/library/dd905650(v=office.12).aspx
		public class AFDOper
		{
			public const byte VTUndefined = 0x00;
			public const byte VTRk = 0x02;
			public const byte VTDouble = 0x04;
			public const byte VTString = 0x06;
			public const byte VTBoolErr = 0x08;
			public const byte VTBlanks = 0x0C;
			public const byte VTNonBlanks = 0x0E;

			public byte vt;
			public ST_FilterOperator grbitSign = ST_FilterOperator.equal;
			public object vtValue;
			public string stringValue;

			public AFDOper() { }

			public AFDOper(string value)
			{
				if (value.Length > 255)
				{
					Utilities.DebugFail("Strings longer than 255 characters cannot be saved in the AFDOper structure.");
					value = value.Substring(0, 255);
				}

				this.stringValue = value;
				this.vt = VTString;
				this.vtValue = (byte)value.Length;
			}

			public AFDOper(object value, ST_FilterOperator operatorValue)
			{
				this.Initialize(value, operatorValue);
			}

			public AFDOper(WorkbookSerializationManager manager, CustomFilterCondition condition)
			{
				if (condition == null)
					return;

				ST_FilterOperator operatorValue;
				object val;
				condition.GetSaveValues(manager, out operatorValue, out val);
				this.Initialize(val, operatorValue);
			}

			private void Initialize(object value, ST_FilterOperator operatorValue)
			{
				this.grbitSign = operatorValue;

				string strValue = value as string;
				if (strValue != null)
				{
					if (strValue.Length == 0 && this.grbitSign == ST_FilterOperator.notEqual)
					{
						this.vt = AFDOper.VTNonBlanks;
					}
					else
					{
						this.vt = AFDOper.VTString;
						this.stringValue = strValue;
						this.vtValue = (byte)this.stringValue.Length;
					}
				}
				else
				{
					this.vtValue = value;

					if (value is double)
					{
						this.vt = AFDOper.VTDouble;
					}
					else if (value is bool || value is ErrorValue)
					{
						this.vt = AFDOper.VTBoolErr;
					}
					else
					{
						Utilities.DebugFail("Unknown value type.");
					}
				}
			}

			public object ResolvedValue
			{
				get
				{
					if (this.vt == VTString)
						return this.stringValue;

					return this.vtValue;
				}
			}

			public bool HasWildcards
			{
				get
				{
					if (vt != VTString)
						return false;

					// It seems like we should be skipping escaped characters here, but the AFDOperStr.fCompare field 
					// (which is saved based on this property), considers all characters.
					return
						this.stringValue.Contains(CustomFilterCondition.WildcardNChars) ||
						this.stringValue.Contains(CustomFilterCondition.WildcardSingleChar);
				}
			}
		}

		#endregion // AFDOper

		// MD 2/19/12 - 12.1 - Table Support
		#region Stxp class

		// http://msdn.microsoft.com/en-us/library/dd945272(v=office.12).aspx
		private class Stxp
		{
			public int twpHeight;
			public Ts ts;
			public ushort bls;
			public ushort sss;
			public byte uls;
			public byte bFamily;
			public byte bCharSet;
		}

		#endregion // Stxp class

		// MD 2/19/12 - 12.1 - Table Support
		#region Ts class

		// http://msdn.microsoft.com/en-us/library/dd953356(v=office.12).aspx
		private class Ts
		{
			public bool ftsItalic;
			public bool ftsStrikeout;
		}

		#endregion // Ts class
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