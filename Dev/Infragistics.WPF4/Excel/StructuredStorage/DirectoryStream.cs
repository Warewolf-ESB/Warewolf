using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;




namespace Infragistics.Documents.Excel.StructuredStorage

{
	internal sealed class DirectoryStream : StructuredStorageStream
	{
		#region Constants

		private const int DirectoryEntryHeaderLength = 0x80;
		private const int DirectoryEntryTitleLength = 0x20;

		#endregion Constants

		#region Member Variables

		private Directory rootEntry;
		private List<DirectoryEntryNode> directoryEntryNodes;
		private readonly int EntriesPerSector;

		#endregion Member Variables

		#region Constructor

		public DirectoryStream( StructuredStorageManager structuredStorage )
			: this( structuredStorage, structuredStorage.AllocateNewSector( -1, false, StreamType.Normal ) )
		{
			DirectoryEntryNode node = new DirectoryEntryNode();

			this.rootEntry = new Directory( "Root Entry", node );
			this.rootEntry.FirstSectorId = StructuredStorageManager.EndOfStreamSectorId;
			this.rootEntry.TimeStampCreation = BitConverter.GetBytes( DateTime.Now.ToFileTime() );
			this.rootEntry.TimeStampModification = this.rootEntry.TimeStampCreation;

			node.childProperty = -1;
			node.leftSiblingProperty = -1;
			node.rightSiblingProperty = -1;

			node.entry = this.rootEntry;
			node.color = NodeColor.Red;

			this.directoryEntryNodes.Add( node );

			this.SetLengthInternal( this.sectorSize );
		}

		public DirectoryStream( StructuredStorageManager structuredStorage, int firstSectorId )
			: base( structuredStorage, firstSectorId, 0 )
		{
			this.directoryEntryNodes = new List<DirectoryEntryNode>();
			this.EntriesPerSector = this.sectorSize / DirectoryStream.DirectoryEntryHeaderLength;

			// Make sure the sectors are alligned on direct entry header boundaries
			Debug.Assert( this.sectorSize % DirectoryStream.DirectoryEntryHeaderLength == 0 );
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

					// MD 10/1/08 - TFS8453
					// We are now going to wait and assign sort numbers right before the save operation.
					this.AssignIndexNumbers();

					int index;
					for ( index = 0; index < this.directoryEntryNodes.Count; index++ )
						this.WriteDirectoryEntryHeader( this.directoryEntryNodes[ index ] );

					while ( index % this.EntriesPerSector != 0 )
					{
						this.WriteDirectoryEntryHeader( null );
						index++;
					}

					Debug.Assert( this.Position == this.Length );
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

		#region AddFile

		public File AddFile( string fileName )
		{
			// MD 10/1/08 - TFS8453
			// Added a parameter to the AddFile method on Directory.
			//File file = this.rootEntry.AddFile( fileName );
			File file = this.rootEntry.AddFile( fileName, this );

			if ( file == null )
				return null;

			// MD 10/1/08 - TFS8453
			// This sibling binary tree indexes will now be assigned just before save, by the AssignIndexNumbers method
			//this.InsertDirectoryEntryIntoChildTree( file );

			return file;
		}

		#endregion AddFile

		// MD 10/1/08 - TFS8453
		#region AssignIndexNumbers






		internal void AssignIndexNumbers()
		{
			for ( int i = 0; i < this.directoryEntryNodes.Count; i++ )
			{
				DirectoryEntryNode node = this.directoryEntryNodes[ i ];
				Directory directory = node.entry as Directory;

				// Assign the child index to the directory and sibling indexes to its direct children.
				if ( directory != null && directory.Children.Count > 0 )
				{
					// Try to pick the middle-most child when assigning choosing child sibling tree head node
					int middleIndex = directory.Children.Count / 2;
					DirectoryEntry middleEntry = directory.Children[ middleIndex ];
					node.childProperty = this.directoryEntryNodes.IndexOf( middleEntry.Node );

					// Create the sibling tree starting at the heade node
					this.AssignSiblingIndexNumbers( middleEntry, directory.Children, 0, directory.Children.Count - 1 );
				}
			}
		} 

		#endregion AssignIndexNumbers

		// MD 10/1/08 - TFS8453
		#region AssignSiblingIndexNumbers



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private void AssignSiblingIndexNumbers( DirectoryEntry currentSiblingEntry, List<DirectoryEntry> siblings, int startIndex, int endIndex )
		{
			int currentSiblingIndex = siblings.IndexOf( currentSiblingEntry );
			Debug.Assert( startIndex <= currentSiblingIndex && currentSiblingIndex <= endIndex, "The current entry is not in the sub-tree" );

			if ( startIndex < currentSiblingIndex )
			{
				int lesserSiblingIndex = ( startIndex + currentSiblingIndex ) / 2;
				DirectoryEntry lesserSiblingEntry = siblings[ lesserSiblingIndex ];
				currentSiblingEntry.Node.leftSiblingProperty = this.directoryEntryNodes.IndexOf( lesserSiblingEntry.Node );

				this.AssignSiblingIndexNumbers( lesserSiblingEntry, siblings, startIndex, currentSiblingIndex - 1 );
			}

			if ( currentSiblingIndex < endIndex )
			{
				int greaterSiblingIndex = ( endIndex + currentSiblingIndex + 1 ) / 2;
				DirectoryEntry greaterSiblingEntry = siblings[ greaterSiblingIndex ];
				currentSiblingEntry.Node.rightSiblingProperty = this.directoryEntryNodes.IndexOf( greaterSiblingEntry.Node );

				this.AssignSiblingIndexNumbers( greaterSiblingEntry, siblings, currentSiblingIndex + 1, endIndex );
			}
		} 

		#endregion AssignSiblingIndexNumbers

		// MD 10/1/08 - TFS8453
		// This code is no logner needed
		#region Old Code

		
#region Infragistics Source Cleanup (Region)









































#endregion // Infragistics Source Cleanup (Region)


		#endregion Old Code

		#region ReadDirectoryEntryHeaders

		public void ReadDirectoryEntryHeaders()
		{
			int sectorId = this.FirstSectorId;
			while ( sectorId >= 0 )
			{
				int steps = 1;
				sectorId = this.GetNextSectorId( sectorId, ref steps );
				this.SetLengthInternal( (int)( this.Length + this.sectorSize ) );
			}

			this.Position = 0;

			while ( this.Position < this.Length )
				this.ReadNextDirectoryEntryHeader();

			Debug.Assert( this.rootEntry != null );

			for ( int i = 0; i < this.directoryEntryNodes.Count; i++ )
			{
				DirectoryEntryNode node = this.directoryEntryNodes[ i ];

				if ( node.childProperty < 0 )
					continue;

				Debug.Assert( node.entry is Directory );
				this.StoreChildEntries( (Directory)node.entry, node.childProperty );
			}
		}

		#endregion ReadDirectoryEntryHeaders

		#region ReadNextDirectoryEntryHeader

		private void ReadNextDirectoryEntryHeader()
		{
			char[] propertyNameChars = new char[ DirectoryStream.DirectoryEntryTitleLength ];

			int charIndex = 0;
			while ( charIndex < DirectoryStream.DirectoryEntryTitleLength )
				propertyNameChars[ charIndex++ ] = (char)this.ReadUInt16();

			int sizeOfCharBuffer = this.ReadInt16();
			int titleLength = ( sizeOfCharBuffer / 2 ) - 1;

			if ( sizeOfCharBuffer == 0 )
				titleLength = 0;

			string propertyName = new string( propertyNameChars, 0, titleLength );

			DirectoryEntryNode node = new DirectoryEntryNode();

			PropertyType propertyType = (PropertyType)this.ReadByte();
			node.color = (NodeColor)this.ReadByte();
			node.leftSiblingProperty = this.ReadInt32();
			node.rightSiblingProperty = this.ReadInt32();
			node.childProperty = this.ReadInt32();
			byte[] uniqueIdentifier = this.ReadBytes( 0x10 );
			byte[] userFlags = this.ReadBytes( 4 );
			byte[] timeStampCreation = this.ReadBytes( 8 );
			byte[] timeStampModification = this.ReadBytes( 8 );
			int firstSectorId = this.ReadInt32();
			int size = this.ReadInt32();
			this.ReadBytes( 4 ); // Not used

			if ( propertyType == PropertyType.Empty )
				return;

			Debug.Assert( propertyType != PropertyType.RootEntry || this.directoryEntryNodes.Count == 0 );

			switch ( propertyType )
			{
				case PropertyType.RootEntry:
					{
						Debug.Assert( this.rootEntry == null, "There are two root entries in this stream." );

						this.rootEntry = new Directory( propertyName, node );
						node.entry = this.rootEntry;

						Debug.Assert( node.childProperty >= 0 );
						break;
					}

				case PropertyType.Ole2Storage:
					{
						node.entry = new Directory( propertyName, node );
						Debug.Assert( node.childProperty >= 0 );
						break;
					}

				case PropertyType.Ole2Stream:
					{
						node.entry = new File( propertyName, node );
						Debug.Assert( node.childProperty < 0 );
						break;
					}

				default:
					{
						Utilities.DebugFail( "Unknown property type." );
						return;
					}
			}

			node.entry.Size = size;
			node.entry.FirstSectorId = firstSectorId;
			node.entry.TimeStampCreation = timeStampCreation;
			node.entry.TimeStampModification = timeStampModification;
			node.entry.UniqueIdentifier = uniqueIdentifier;
			node.entry.UserFlags = userFlags;

			this.directoryEntryNodes.Add( node );
		}

		#endregion ReadNextDirectoryEntryHeader

		#region StoreChildEntries

		private void StoreChildEntries( Directory dirctory, int childNodeIndex )
		{
			DirectoryEntryNode childNode = this.directoryEntryNodes[ childNodeIndex ];

			if ( childNode.leftSiblingProperty >= 0 )
				this.StoreChildEntries( dirctory, childNode.leftSiblingProperty );

			childNode.entry.Parent = dirctory;
			dirctory.Children.Add( childNode.entry );

			if ( childNode.rightSiblingProperty >= 0 )
				this.StoreChildEntries( dirctory, childNode.rightSiblingProperty );
		}

		#endregion StoreChildEntries

		#region WriteDirectoryEntryHeader

		private void WriteDirectoryEntryHeader( DirectoryEntryNode node )
		{
			if ( node == null )
			{
				byte[] startingZeros = new byte[ ( DirectoryStream.DirectoryEntryTitleLength * 2 ) + 4 ];
				this.Write( startingZeros, 0, startingZeros.Length );

				this.WriteInt32( -1 ); // leftSiblingProperty
				this.WriteInt32( -1 ); // rightSiblingProperty
				this.WriteInt32( -1 ); // childProperty

				byte[] endingZeros = new byte[ 48 ];
				this.Write( endingZeros, 0, endingZeros.Length );
			}
			else
			{
				byte[] data = Encoding.Unicode.GetBytes( node.entry.Name );
				byte[] paddedZeros = new byte[ ( DirectoryStream.DirectoryEntryTitleLength * 2 ) - data.Length ];

				this.Write( data, 0, data.Length );
				this.Write( paddedZeros, 0, paddedZeros.Length );

				this.WriteInt16( (short)( data.Length + 2 ) );

				PropertyType type = PropertyType.Empty;

				if ( node.entry == this.rootEntry )
					type = PropertyType.RootEntry;
				else if ( node.entry is File )
					type = PropertyType.Ole2Stream;
				else if ( node.entry is Directory )
					type = PropertyType.Ole2Storage;
				else
					Utilities.DebugFail( "Unknown type" );

				this.WriteByte( (byte)type );
				this.WriteByte( (byte)node.color );
				this.WriteInt32( node.leftSiblingProperty );
				this.WriteInt32( node.rightSiblingProperty );
				this.WriteInt32( node.childProperty );
				this.Write( node.entry.UniqueIdentifier, 0, 16 );
				this.Write( node.entry.UserFlags, 0, 4 );
				this.Write( node.entry.TimeStampCreation, 0, 8 );
				this.Write( node.entry.TimeStampModification, 0, 8 );
				this.WriteInt32( node.entry.FirstSectorId );
				this.WriteInt32( node.entry.Size );
				this.WriteInt32( 0 ); // Not used
			}
		}

		#endregion ReadNextDirectoryEntryHeader

		#endregion Methods

		#region Properties

		// MD 10/1/08 - TFS8453
		#region DirectoryEntryNodes

		internal List<DirectoryEntryNode> DirectoryEntryNodes
		{
			get { return this.directoryEntryNodes; }
		}

		#endregion DirectoryEntryNodes

		#region RootEntry

		internal Directory RootEntry
		{
			get { return this.rootEntry; }
		}

		#endregion RootEntry 

		#endregion Properties


		#region DirectoryEntryNode Class

		internal class DirectoryEntryNode
		{
			public DirectoryEntryNode() { }

			public DirectoryEntry entry;
			public int leftSiblingProperty;
			public int rightSiblingProperty;
			public int childProperty;
			public NodeColor color;
		}

		#endregion DirectoryEntryNode Class

		#region NodeColor Enum

		internal enum NodeColor : byte
		{
			Red = 0,
			Black = 1
		}

		#endregion NodeColor Enum

		#region PropertyType Enum

		internal enum PropertyType : byte
		{
			Empty = 0,
			Ole2Storage = 1,
			Ole2Stream = 2,
			RootEntry = 5
		}

		#endregion PropertyType Enum
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