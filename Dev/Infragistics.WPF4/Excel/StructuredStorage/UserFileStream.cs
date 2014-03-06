using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.StructuredStorage

{
	internal class UserFileStream : DirectoryEntryStream
	{
		#region Member Variables

		private File file;

		#endregion Member Variables

		#region Constructor

		public UserFileStream( StructuredStorageManager structuredStorage, File file )
			: base( structuredStorage, file, structuredStorage.IsShortStream( file.Size ) )
		{
			this.file = file;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region SetLength

		public override void SetLength( long value )
		{
			// If the stream exists in the short SAT and the bytes written will cause its
			// length to go above the minimim length for the main SAT, move it to the 
			// main SAT
			if ( this.IsShortStream &&
				this.StorageManager.IsShortStream( (int)value ) == false )
			{
				long oldPosition = this.Position;
				Debug.Assert( oldPosition >= 0 );

				byte[] data = new byte[ this.Length ];

				if ( data.Length > 0 )
				{
					this.Position = 0;
					this.Read( data, 0, data.Length );
					this.SetLengthInternal( 0 );
				}

				this.StorageManager.RemoveStream( this );
				this.isShortStream = false;
				this.ResetCache();
				this.SetFirstSectorId( this.StorageManager.AllocateNewSector( -1, this.IsShortStream, this.Type ) );
				this.file.FirstSectorId = this.FirstSectorId;

				if ( data.Length > 0 )
				{
					this.Position = 0;
					this.Write( data, 0, data.Length );
					this.Position = oldPosition;
				}
			}

			base.SetLength( value );
		}

		#endregion SetLength

		#endregion Base Class Overrides

		#region Properties

		#region IsShortStream

		public bool IsShortStream
		{
			get { return this.isShortStream; }
		}

		#endregion IsShortStream

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