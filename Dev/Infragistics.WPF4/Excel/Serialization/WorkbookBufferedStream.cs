using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization

{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class WorkbookBufferedStream : Stream
	{
		#region Constants

		// MD 9/23/09 - TFS19150
		// We now may read a block the size of the BIFF8 max record block size, so increate this length so we can still get 
		// the benefit of the buffering.
		//private const int MaxBufferLength = 8192;
		private const int MaxBufferLength = 10240;

		#endregion Constants

		#region Member Variables

		private Stream baseStream;

		private long length;
		private long position;

		private byte[] buffer;
		private int bufferLength;
		private int bufferPosition;
		private bool bytesWrittenToBuffer;
		private int baseStreamPositionInBuffer;

		#endregion Member Variables

		#region Constructor

		public WorkbookBufferedStream( Stream baseStream )
		{
			this.baseStream = baseStream;
			this.buffer = new byte[ WorkbookBufferedStream.MaxBufferLength ];

			this.length = baseStream.Length;
			this.position = baseStream.Position;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region CanRead

		public override bool CanRead
		{
			get { return this.baseStream.CanRead; }
		}

		#endregion CanRead

		#region CanSeek

		public override bool CanSeek
		{
			get { return this.baseStream.CanSeek; }
		}

		#endregion CanSeek

		#region CanWrite

		public override bool CanWrite
		{
			get { return this.baseStream.CanWrite; }
		}

		#endregion CanWrite

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			try
			{
				if ( disposing )
					this.Flush();
			}
			finally
			{
				base.Dispose( disposing );
			}
		}

		#endregion Dispose

		#region Flush

		public override void Flush()
		{
			// If bytes were written to the buffer, we need to write the buffer to the base stream
			if ( this.bytesWrittenToBuffer )
			{
				// Clear the written flag
				this.bytesWrittenToBuffer = false;

				// Put the base stream in the corrent position and write the buffer
				if ( this.baseStreamPositionInBuffer > 0 )
					this.baseStream.Seek( -this.baseStreamPositionInBuffer, SeekOrigin.Current );

				this.baseStream.Write( this.buffer, 0, this.bufferLength );
			}

			// Move the base stream into the correct position for the next read/write
			this.baseStream.Position = this.position;

			// Reset the buffer fields
			this.bufferPosition = 0;
			this.bufferLength = 0;
			this.baseStreamPositionInBuffer = 0;
		}

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
				if ( this.position != value )
					this.Seek( value - this.position, SeekOrigin.Current );
			}
		}

		#endregion Position

		#region Read

		public override int Read( byte[] array, int offset, int count )
		{
			if ( count == 0 )
				return 0;

			// Determine how many extra bytes are needed in the buffer
			int bytesNeededForBuffer = count - ( this.bufferLength - this.bufferPosition );

			if ( bytesNeededForBuffer > 0 )
			{
				// If extra bytes are needed for the buffer, and expanding the buffer will fit the extra bytes, expand it
				if ( this.bufferLength + bytesNeededForBuffer <= WorkbookBufferedStream.MaxBufferLength )
				{
					// Move the base stream so it is at the end of the current end of the buffer
					if ( this.baseStreamPositionInBuffer != this.bufferLength )
						this.baseStream.Seek( this.bufferLength - this.baseStreamPositionInBuffer, SeekOrigin.Current );

					// Read the remaining bytes that can fit in the maximum buffer length
					int bytesRead = this.baseStream.Read( this.buffer, this.bufferLength, WorkbookBufferedStream.MaxBufferLength - this.bufferLength );

					// Increase the buffer size
					this.bufferLength += bytesRead;

					// Mark the new position of the base stream in the buffer (at the end of it)
					this.baseStreamPositionInBuffer = this.bufferLength;

					// Decrement the bytes needed fo rthe buffer by the amount of bytes read, this should set
					// the neede dbytes to zero, but in case we hit the end of the stream, we want to know
					// how many more bytes are needed that couldnt be read.
					bytesNeededForBuffer -= bytesRead;
				}
				else
				{
					// If the extra bytes cannot fit in the buffer at its maximum length, flush the current buffer contents
					this.Flush();

					if ( count <= WorkbookBufferedStream.MaxBufferLength )
					{
						// If the count to read is less than the max buffer length, fill the buffer to its max
						this.bufferLength = this.baseStream.Read( this.buffer, 0, WorkbookBufferedStream.MaxBufferLength );
						
						// The base stream is now at the end of the buffer
						this.baseStreamPositionInBuffer = this.bufferLength;

						// Update the number of bytes needed
						bytesNeededForBuffer = count - this.bufferLength;
					}
					else
					{
						// Read the bytes directly from the stream
						int bytesRead = this.baseStream.Read( array, offset, count );

						// Increase the position of the buffered stream
						this.position += bytesRead;

						// The read operation is complete, return the number of bytes read
						return bytesRead;
					}
				}
			}

			if ( bytesNeededForBuffer < 0 )
				bytesNeededForBuffer = 0;

			// Read the count of bytes from the buffer, minus the bytes still needed for the buffer 
			// (which couldn't be read because the base stream reached its end)
			int bytesToRead = count - bytesNeededForBuffer;
			Buffer.BlockCopy( this.buffer, this.bufferPosition, array, offset, bytesToRead );

			// Update the position fields
			this.bufferPosition += bytesToRead;
			this.position += bytesToRead;

			return bytesToRead;
		}

		#endregion Read

		#region Seek

		public override long Seek( long offset, SeekOrigin origin )
		{
			if ( origin != SeekOrigin.Current )
			{
				switch ( origin )
				{
					case SeekOrigin.Begin:
						Debug.Assert( offset >= 0 );
						this.Position = offset;
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

			if ( offset == 0 )
				return this.position;

			this.position += offset;

			if ( Int32.MinValue <= offset && offset <= Int32.MaxValue )
			{
				this.bufferPosition += (int)offset;

				if ( this.bufferPosition >= 0 )
				{
					// If the new position is valid in the buffer, nothing needs to be done
					if ( this.bufferPosition <= this.bufferLength )
						return this.position;

					// If the new position is not valid in the buffer, but the buffer can be expanded to make 
					// the new position valid, expand the buffer
					if ( this.bufferPosition <= WorkbookBufferedStream.MaxBufferLength )
					{
						// MD 6/14/07 - BR23880
						// If the length of the base stream won't let us seek to the end of the current buffer to extend the valid buffer, 
						// there is nothing we can do, so just exit.
						if ( this.baseStream.Length - this.baseStream.Position <= this.bufferLength - this.baseStreamPositionInBuffer )
							return this.position;

						// Seek to the where the end of the valid buffer would be in the base stream
						if ( this.baseStreamPositionInBuffer != this.bufferLength )
							this.baseStream.Seek( this.bufferLength - this.baseStreamPositionInBuffer, SeekOrigin.Current );

						// Read in enough bytes from the base stream to the buffer to make the new position valid 
						// in the buffer
						int bytesRead = this.baseStream.Read( this.buffer, this.bufferLength, WorkbookBufferedStream.MaxBufferLength - this.bufferLength );

						// Set the valid buffer length to extend to the new position
						this.bufferLength += bytesRead;

						// The base stream is now positioned at the end of the buffer
						this.baseStreamPositionInBuffer = this.bufferLength;

						return this.position;
					}
				}
			}

			this.Flush();
			return this.position;
		}

		#endregion Seek

		#region SetLength

		public override void SetLength( long value )
		{
			this.Flush();
			this.baseStream.SetLength( value );

			this.length = this.baseStream.Length;
			this.position = this.baseStream.Position;
		}

		#endregion SetLength

		#region Write

		public override void Write( byte[] array, int offset, int count )
		{
			if ( count == 0 )
				return;

			// Determine how many extra bytes are needed in the buffer
			int bytesNeededForBuffer = count - ( this.bufferLength - this.bufferPosition );

			if ( bytesNeededForBuffer > 0 )
			{
				// If writing the count of bytes will make the buffer bigger than its max size, flush the buffer
				if ( WorkbookBufferedStream.MaxBufferLength < this.bufferLength + bytesNeededForBuffer )
				{
					this.Flush();

					// If after flushing the buffer, the count of bytes still cannot fit, write the data directly 
					// to the base stream
					if ( WorkbookBufferedStream.MaxBufferLength < count )
					{
						this.baseStream.Write( array, offset, count );

						// Increase the position
						this.position += count;

						// Increase the length if necessary
						if ( this.length < this.position )
							this.length = this.position;

						return;
					}
				}
			}

			// Mark the flag that bytes were written to the buffer
			this.bytesWrittenToBuffer = true;

			// Copy the bytes to the buffer
			Buffer.BlockCopy( array, offset, this.buffer, this.bufferPosition, count );

			// Increase the position fields
			this.bufferPosition += count;
			this.position += count;

			// Increase the length fields if necessary
			if ( this.bufferLength < this.bufferPosition )
				this.bufferLength = this.bufferPosition;

			if ( this.length < this.position )
				this.length = this.position;
		}

		#endregion Write

		#endregion Base Class Overrides
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