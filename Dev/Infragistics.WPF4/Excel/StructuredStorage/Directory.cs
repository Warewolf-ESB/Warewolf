using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.StructuredStorage

{
	internal class Directory : DirectoryEntry
	{
		#region Member Variables

		private List<DirectoryEntry> children;

		#endregion Member Variables

		#region Constructor

		internal Directory( string name, DirectoryStream.DirectoryEntryNode node )
			: base( name, node )
		{
			this.children = new List<DirectoryEntry>();
		}

		#endregion Constructor

		#region AddFile

		// MD 10/1/08 - TFS8453
		// A reference to the owning directory stream was needed
		//public File AddFile( string fileName )
		public File AddFile( string fileName, DirectoryStream directoryStream )
		{
			// MD 10/1/08 - TFS8453
			// Refactored because now we allow nested directories to be added and the code had some redundant sections.
			#region Refactored

			
#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)


			#endregion Refactored

			DirectoryStream.DirectoryEntryNode node = new DirectoryStream.DirectoryEntryNode();
			DirectoryEntry newEntry = null;
			File finalFile;

			int separatorIndex = fileName.IndexOf( '\\' );

			if ( separatorIndex < 0 )
			{
				for ( int i = 0; i < this.Children.Count; i++ )
				{
					File file = this.Children[ i ] as File;

					if ( file != null && file.Name == fileName )
					{
						Utilities.DebugFail( "The file already exists" );
						return file;
					}
				}

				finalFile = new File( fileName, node );
				newEntry = finalFile;
			}
			else
			{
				string directoryName = fileName.Substring( 0, separatorIndex );

				for ( int i = 0; i < this.Children.Count; i++ )
				{
					Directory directory = this.Children[ i ] as Directory;

					if ( directory != null && directory.Name == directoryName )
						return directory.AddFile( fileName.Substring( separatorIndex + 1 ), directoryStream );
				}

				Directory newDirectory = new Directory( directoryName, node );
				finalFile = newDirectory.AddFile( fileName.Substring( separatorIndex + 1 ), directoryStream );
				newEntry = newDirectory;
			}

			newEntry.FirstSectorId = StructuredStorageManager.EndOfStreamSectorId;
			newEntry.TimeStampCreation = BitConverter.GetBytes( DateTime.Now.ToFileTime() );
			newEntry.TimeStampModification = newEntry.TimeStampCreation;

			node.childProperty = -1;
			node.leftSiblingProperty = -1;
			node.rightSiblingProperty = -1;

			node.color = DirectoryStream.NodeColor.Red;
			node.entry = newEntry;

			newEntry.Parent = this;

			int insertionIndex;
			for ( insertionIndex = 0; insertionIndex < this.Children.Count; insertionIndex++ )
			{
				if ( DirectoryEntry.EntryNameComparer.Compare( newEntry, this.Children[ insertionIndex ] ) < 0 )
					break;
			}

			this.Children.Insert( insertionIndex, newEntry );

			directoryStream.DirectoryEntryNodes.Add( node );

			return finalFile;
		}

		#endregion AddFile

		#region GetFile

		public File GetFile( string currentPath, string fileName )
		{
			for ( int i = 0; i < this.children.Count; i++ )
			{
				DirectoryEntry entry = this.children[ i ];

				string newPath = currentPath + entry.Name;

				Directory dir = entry as Directory;
				if ( dir != null )
				{
					// MD 7/14/10 - TFS35783
					// We should be comaparing paths case-insensitively.
					//if ( fileName.StartsWith( newPath ) )
					if (fileName.StartsWith(newPath, StringComparison.InvariantCultureIgnoreCase))
						return dir.GetFile( newPath + '\\', fileName );
				}
				// MD 7/14/10 - TFS35783
				// We should be comaparing paths case-insensitively.
				//else if ( newPath == fileName )
				else if (String.Equals(newPath, fileName, StringComparison.InvariantCultureIgnoreCase))
				{
					return (File)entry;
				}
			}

			return null;
		}

		#endregion GetFile

		#region Children

		public List<DirectoryEntry> Children
		{
			get { return this.children; }
		}

		#endregion Children
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