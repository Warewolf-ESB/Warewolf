using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;






using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.StructuredStorage

{
	internal class StructuredStorageManager : IDisposable
	{
		#region Constants

		// This constant must be the first thing in a structured storage file
		private const ulong FileTypeID = 0xE11AB1A1E011CFD0;
		private const ushort LittleEndianFormat = 0xFFFE;

		private const int HeaderLength = 0x200;

		internal const int FreeSectorId = -1;
		internal const int EndOfStreamSectorId = -2;
		internal const int SATSectorId = -3;
		internal const int MasterSATSectorId = -4;

		internal const int HeaderMasterSATLength = 0x6D;
		internal const int HeaderMasterSATPosition = 0x01B4;

		#endregion Constants

		#region Member Variables

		private Guid id;
		private ushort revisionNumberOfFileFormat = 0x003E;
		private ushort versionNumberOfFileFormat = 0x0003;

		private int sectorSize = 512;
		private int shortSectorSize = 64;
		private int minimumSizeForNormalStreams = 4096;

		private Stream stream;
		private DirectoryEntryStream shortStreamContainerStream;

		private SATStream SATStream;
		private ShortSATStream shortSATStream;
		private DirectoryStream directoryStream;

		private bool isOpen;
		private bool isDirty;

		#endregion Member Variables

		#region Constructor

		public StructuredStorageManager( Stream stream, bool containsValidData )
		{
			Debug.Assert( stream != null );
			this.stream = new WorkbookBufferedStream( stream );

			if ( containsValidData )
			{
				#region Reader Header Sector

				BinaryReader reader = new BinaryReader( this.stream );

				ulong fileTypeID = reader.ReadUInt64();

				if ( fileTypeID != StructuredStorageManager.FileTypeID )
				{
					Utilities.DebugFail( "Invalid file type" );
					return;
				}

				this.id = new Guid( reader.ReadBytes( 0x10 ) );

				this.revisionNumberOfFileFormat = reader.ReadUInt16();
				this.versionNumberOfFileFormat = reader.ReadUInt16();

				ushort binaryFormat = reader.ReadUInt16();
				Debug.Assert( binaryFormat == StructuredStorageManager.LittleEndianFormat );

				this.sectorSize = 1 << reader.ReadUInt16();
				this.shortSectorSize = 1 << reader.ReadUInt16();
				Debug.Assert( this.sectorSize > this.shortSectorSize );

				reader.ReadBytes( 0x0A );

				int numberOfSATSectors = reader.ReadInt32();
				int firstDirectoryStreamSectorId = reader.ReadInt32();

				reader.ReadBytes( 0x04 );

				this.minimumSizeForNormalStreams = reader.ReadInt32();
				int firstMasterShortSATSectorId = reader.ReadInt32();
				int numberOfShortSATSectors = reader.ReadInt32();
				int firstMasterSATSectorId = reader.ReadInt32();
				int numberOfMasterSATSectors = reader.ReadInt32();

				int[] headerMasterSAT = new int[ StructuredStorageManager.HeaderMasterSATLength ];

				// Read the first portion of the master SAT at the end of the header block
				for ( int i = 0; i < StructuredStorageManager.HeaderMasterSATLength; i++ )
					headerMasterSAT[ i ] = reader.ReadInt32();

				MasterSATStream masterSATStream = new MasterSATStream( this, firstMasterSATSectorId, numberOfMasterSATSectors, headerMasterSAT );
				this.SATStream = new SATStream( this, headerMasterSAT[ 0 ], numberOfSATSectors, masterSATStream );
				this.shortSATStream = new ShortSATStream( this, firstMasterShortSATSectorId, numberOfShortSATSectors );

				#endregion Reader Header Sector

				this.directoryStream = new DirectoryStream( this, firstDirectoryStreamSectorId );
				this.directoryStream.ReadDirectoryEntryHeaders();
				this.shortStreamContainerStream = new DirectoryEntryStream( this, this.directoryStream.RootEntry );
			}
			else
			{
				int[] headerMasterSAT = new int[ StructuredStorageManager.HeaderMasterSATLength ];
				headerMasterSAT[ 0 ] = 0;

				for ( int i = 1; i < StructuredStorageManager.HeaderMasterSATLength; i++ )
					headerMasterSAT[ i ] = StructuredStorageManager.FreeSectorId;

				MasterSATStream masterSATStream = new MasterSATStream( this, StructuredStorageManager.EndOfStreamSectorId, 0, headerMasterSAT );
				this.SATStream = new SATStream( this, masterSATStream );
				this.shortSATStream = new ShortSATStream( this, StructuredStorageManager.EndOfStreamSectorId, 0 );
				this.directoryStream = new DirectoryStream( this );

				this.shortStreamContainerStream = new DirectoryEntryStream( this, this.directoryStream.RootEntry );
			}

			this.isOpen = true;
		}

		~StructuredStorageManager()
		{
			this.Dispose( false );
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#region AddFile

		public Stream AddFile( string fileName )
		{
			File file = this.directoryStream.AddFile( fileName );

			if ( file == null )
				return null;

			this.isDirty = true;
			return new UserFileStream( this, file );
		}

		#endregion AddFile

		#region GetFileStream

		public Stream GetFileStream( string fileName )
		{
			if ( this.isOpen == false )
			{
				Utilities.DebugFail( "The structured storage stream is closed" );
				return null;
			}

			File file = this.directoryStream.RootEntry.GetFile( string.Empty, fileName );

			if ( file == null )
				return null;

			return new UserFileStream( this, file );
		}

		#endregion GetFileStream

		// MD 6/30/08 - Excel 2007 Format
		#region IsStructuredStorageStream

		public static bool IsStructuredStorageStream( Stream stream )
		{
			if ( stream.Length < 8 )
				return false;

			stream.Position = 0;

			try
			{
				BinaryReader reader = new BinaryReader( stream );
				ulong fileTypeID = reader.ReadUInt64();

				return fileTypeID == StructuredStorageManager.FileTypeID;
			}
			finally
			{
				stream.Position = 0;
			}
		} 

		#endregion IsStructuredStorageStream

		#endregion Public Methods

		#region Protected Methods

		#region Dispose

		protected virtual void Dispose( bool disposing )
		{
			if ( this.isOpen )
			{
				if ( disposing && this.isDirty )
				{
					// MD 5/29/09 - TFS17917
					// The header contents could be modified while these things are saving out their contents, so they should be disposed before 
					// we save out the master header. These lines have been moved from below the saving of the header sector.
					this.directoryStream.Dispose();
					this.shortStreamContainerStream.Dispose();
					this.shortSATStream.Dispose();
					this.SATStream.Dispose();

					this.stream.Position = 0;

					#region Write Header Sector

					BinaryWriter writer = new BinaryWriter( this.stream );

					writer.Write( StructuredStorageManager.FileTypeID );
					writer.Write( this.id.ToByteArray() );
					writer.Write( this.revisionNumberOfFileFormat );
					writer.Write( this.versionNumberOfFileFormat );
					writer.Write( StructuredStorageManager.LittleEndianFormat );

					// MD 10/5/07 - BR26292
					// Its possible these lines of code coud cause rounding errors where an incorrect value is written out:
					// For Log of 64, it might return 5.999999 instead of 6, which when cast to a ushort, would return 5.
					// To prevent this, we should now round the resut before casting. In addition, checks have been put in 
					// place to make sure the powers recieved will correctly get back to the sector size.
					//writer.Write( (ushort)Math.Log( this.sectorSize, 2 ) );
					//writer.Write( (ushort)Math.Log( this.shortSectorSize, 2 ) );
					ushort sectorSizePower = (ushort)Math.Round( Math.Log( this.sectorSize, 2 ) );
					Debug.Assert( Math.Pow( 2, sectorSizePower ) == this.sectorSize );
					writer.Write( sectorSizePower );

					ushort shortSectorSizePower = (ushort)Math.Round( Math.Log( this.shortSectorSize, 2 ) );
					Debug.Assert( Math.Pow( 2, shortSectorSizePower ) == this.shortSectorSize );
					writer.Write( shortSectorSizePower );

					writer.Write( new byte[ 10 ] ); // Not used
					writer.Write( (int)( ( ( this.SATStream.Length - 1 ) / this.sectorSize ) + 1 ) ); // Number of SAT sectors
					writer.Write( this.directoryStream.FirstSectorId );
					writer.Write( new byte[ 4 ] ); // Not Used
					writer.Write( this.minimumSizeForNormalStreams );
					writer.Write( this.shortSATStream.FirstSectorId );

					if ( this.shortSATStream.Length == 0 )
						writer.Write( (int)0 ); // Number of short SAT sectors
					else
						writer.Write( (int)( ( ( this.shortSATStream.Length - 1 ) / this.sectorSize ) + 1 ) ); // Number of short SAT sectors

					writer.Write( this.SATStream.MasterSATStream.FirstSectorId );

					if ( this.SATStream.MasterSATStream.Length == 0 )
						writer.Write( (int)0 ); // Number of master SAT sectors
					else
						writer.Write( (int)( ( ( this.SATStream.MasterSATStream.Length - 1 ) / this.sectorSize ) + 1 ) ); // Number of master SAT sectors

					int[] headerMasterSAT = this.SATStream.MasterSATStream.HeaderMasterSAT;

					for ( int i = 0; i < headerMasterSAT.Length; i++ )
						writer.Write( headerMasterSAT[ i ] );

					#endregion Write Header Sector

					// MD 5/29/09 - TFS17917
					// The header contents could be modified while these things are saving out their contents, so they should be disposed before 
					// we save out the master header. These lines have been moved above the saving of the header sector.
					//this.directoryStream.Dispose();
					//this.shortStreamContainerStream.Dispose();
					//this.shortSATStream.Dispose();
					//this.SATStream.Dispose();

					this.stream.Dispose();
				}

				this.directoryStream = null;
				this.shortStreamContainerStream = null;
				this.shortSATStream = null;
				this.SATStream = null;
				this.stream = null;

				this.isDirty = false;
				this.isOpen = false;
			}
		}

		#endregion Dispose

		#endregion Protected Methods

		#region Internal Methods

		#region AllocateNewSector

		internal int AllocateNewSector( int lastSectorId, bool isShortStream, StructuredStorageStream.StreamType streamType )
		{
			if ( isShortStream )
			{
				Debug.Assert( streamType == StructuredStorageStream.StreamType.Normal );
				return this.shortSATStream.AllocateNewSectorForStream( lastSectorId, streamType );
			}

			return this.SATStream.AllocateNewSectorForStream( lastSectorId, streamType );
		}

		#endregion AllocateNewSector

		#region GetSectorPosition

		internal long GetSectorPosition( int sectorId, int offsetInSector, bool isShortSector )
		{
			if ( isShortSector )
				return ( sectorId * this.shortSectorSize ) + offsetInSector;

			return StructuredStorageManager.HeaderLength + ( (long)sectorId * this.sectorSize ) + offsetInSector;
		}

		#endregion GetSectorPosition

		#region GetStreamContainerStream

		internal Stream GetStreamContainerStream( bool isShortStream )
		{
			if ( isShortStream )
				return this.shortStreamContainerStream;

			return this.stream;
		}

		#endregion GetStreamContainerStream

		#region GetNextSectorId

		internal int GetNextSectorId( int lastSectorId, ref int steps, bool isShortStream )
		{
			if ( isShortStream )
				return this.shortSATStream.GetNextSectorIdInSAT( lastSectorId, ref steps );

			return this.SATStream.GetNextSectorIdInSAT( lastSectorId, ref steps );
		}

		#endregion GetNextSectorId

		#region GetSectorSize

		internal int GetSectorSize( bool isShortStream )
		{
			if ( isShortStream )
				return this.shortSectorSize;

			return this.sectorSize;
		}

		#endregion GetSectorSize

		#region IsInShortSAT

		internal bool IsShortStream( int streamLength )
		{
			return streamLength < this.minimumSizeForNormalStreams;
		}

		#endregion IsInShortSAT

		#region SeekToSector

		internal long SeekToSector( int sectorId, bool isShortSector )
		{
			return this.SeekToSector( sectorId, 0, isShortSector );
		}

		internal long SeekToSector( int sectorId, int offsetInSector, bool isShortStream )
		{
			long newPosition = this.GetSectorPosition( sectorId, offsetInSector, isShortStream );
			this.GetStreamContainerStream( isShortStream ).Position = newPosition;
			return newPosition;
		}

		#endregion SeekToSector

		#region RemoveStream

		internal void RemoveStream( UserFileStream stream )
		{
			if ( stream.IsShortStream )
				this.shortSATStream.RemoveStream( stream );
			else
				this.SATStream.RemoveStream( stream );
		}

		#endregion RemoveStream

		#endregion Internal Methods

		#endregion Methods

		#region Properties

		#region IsDirty

		public bool IsDirty
		{
			get { return this.isDirty; }
			set { this.isDirty = value; }
		}

		#endregion IsDirty

		// MD 10/1/08 - TFS8453
		#region RootDirectory

		public Directory RootDirectory
		{
			get { return this.directoryStream.RootEntry; }
		} 

		#endregion RootDirectory

		#endregion Properties

		#region IDisposable Members

		public void Dispose()
		{
			this.Dispose( true );
			GC.SuppressFinalize( this );
		}

		#endregion
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