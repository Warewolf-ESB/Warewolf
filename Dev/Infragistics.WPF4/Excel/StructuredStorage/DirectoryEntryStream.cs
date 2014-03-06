using System;
using System.Collections.Generic;
using System.Text;




namespace Infragistics.Documents.Excel.StructuredStorage

{
	internal class DirectoryEntryStream : StructuredStorageStream
	{
		#region Member Variables

		private DirectoryEntry directoryEntry;
		private bool isDirty;

		#endregion Member Variables

		#region Constructor

		public DirectoryEntryStream( StructuredStorageManager structuredStorage, DirectoryEntry directoryEntry )
			: this( structuredStorage, directoryEntry, false ) { }

		public DirectoryEntryStream( StructuredStorageManager structuredStorage, DirectoryEntry directoryEntry, bool isShortStream )
			: base( structuredStorage, directoryEntry.FirstSectorId, directoryEntry.Size, isShortStream )
		{
			this.directoryEntry = directoryEntry;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			try
			{
				if ( disposing && this.isDirty )
				{
					this.directoryEntry.TimeStampModification = BitConverter.GetBytes( DateTime.Now.ToFileTime() );
					this.isDirty = false;
				}
			}
			finally
			{
				base.Dispose( disposing );
			}
		}

		#endregion Dispose

		#region SetFirstSectorId

		protected override void SetFirstSectorId( int firstSectorId )
		{
			base.SetFirstSectorId( firstSectorId );

			this.directoryEntry.FirstSectorId = firstSectorId;
		}

		#endregion SetFirstSectorId

		#region SetLengthInternal

		internal override void SetLengthInternal( int value )
		{
			base.SetLengthInternal( value );

			this.directoryEntry.Size = value;
		}

		#endregion SetLengthInternal

		#region Write

		public override void Write( byte[] buffer, int offset, int count )
		{
			base.Write( buffer, offset, count );

			if ( count > 0 )
				this.isDirty = true;
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