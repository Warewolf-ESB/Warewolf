using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;




namespace Infragistics.Documents.Excel.StructuredStorage

{



	internal abstract class DirectoryEntry
	{
		#region Member Variables

		private int firstSectorId;
		private string name;
		private int size;
		private byte[] timeStampCreation;
		private byte[] timeStampModification;
		private byte[] uniqueIdentifier;
		private byte[] userFlags;

		private Directory parent;
		private DirectoryStream.DirectoryEntryNode node;

		#endregion Member Variables

		#region Constructor

		internal DirectoryEntry( string name, DirectoryStream.DirectoryEntryNode node )
		{
			this.name = name;
			this.node = node;
		} 

		#endregion Constructor

		#region Methods

		#region VerifySize

		private static void VerifySize( byte[] newValue, int requiredLength )
		{
			if ( newValue.Length != requiredLength )
				Utilities.DebugFail( "The data has an invalid size." );
		}

		#endregion VerifySize

		#endregion Methods

		#region Properties

		#region FirstSectorId

		public int FirstSectorId
		{
			get { return this.firstSectorId; }
			set { this.firstSectorId = value; }
		}

		#endregion FirstSectorId

		#region Name

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		#endregion Name

		#region Node

		internal DirectoryStream.DirectoryEntryNode Node
		{
			get { return this.node; }
		}

		#endregion Node

		#region Parent

		public Directory Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		#endregion Parent

		#region Size

		public int Size
		{
			get { return this.size; }
			set { this.size = value; }
		}

		#endregion Size

		#region TimeStampCreation

		public byte[] TimeStampCreation
		{
			get
			{
				if ( this.timeStampCreation == null )
					this.timeStampCreation = new byte[ 8 ];

				return this.timeStampCreation;
			}
			set
			{
				VerifySize( value, 8 );
				this.timeStampCreation = value;
			}
		}

		#endregion TimeStampCreation

		#region TimeStampModification

		public byte[] TimeStampModification
		{
			get
			{
				if ( this.timeStampModification == null )
					this.timeStampModification = new byte[ 8 ];

				return this.timeStampModification;
			}
			set
			{
				VerifySize( value, 8 );
				this.timeStampModification = value;
			}
		}

		#endregion TimeStampModification

		#region UniqueIdentifier

		public byte[] UniqueIdentifier
		{
			get
			{
				if ( this.uniqueIdentifier == null )
					this.uniqueIdentifier = new byte[ 16 ];

				return this.uniqueIdentifier;
			}
			set
			{
				VerifySize( value, 16 );
				this.uniqueIdentifier = value;
			}
		}

		#endregion UniqueIdentifier

		#region UserFlags

		public byte[] UserFlags
		{
			get
			{
				if ( this.userFlags == null )
					this.userFlags = new byte[ 4 ];

				return this.userFlags;
			}
			set
			{
				VerifySize( value, 4 );
				this.userFlags = value;
			}
		}

		#endregion UserFlags

		#endregion Properties

		#region EntryNameComparer

		// MD 4/18/08 - BR32154
		// Made the static variable thread static so we do not get into a race condition when creating it.
		[ThreadStatic]
		private static IComparer<DirectoryEntry> entryNameComparer;

		public static IComparer<DirectoryEntry> EntryNameComparer
		{
			get
			{
				if ( entryNameComparer == null )
					entryNameComparer = new NameComparer();

				return entryNameComparer;
			}
		}

		#endregion EntryNameComparer


		#region NameComparer Class

		private class NameComparer : IComparer<DirectoryEntry>
		{
			#region IComparer<DirectoryEntry> Members

			public int Compare( DirectoryEntry x, DirectoryEntry y )
			{
				if ( x.Name.Length < y.Name.Length )
					return -1;

				if ( x.Name.Length == y.Name.Length )
				{
					// MD 10/1/08 - TFS8453
					// Excel is very sensitive about the files being sorted correctly. We were using the wrong sorting alrogithm.
					//return String.Compare( x.Name, y.Name, false, CultureInfo.CurrentCulture );
					return String.Compare( x.Name, y.Name, StringComparison.OrdinalIgnoreCase );
				}

				return 1;
			}

			#endregion
		}

		#endregion NameComparer Class
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