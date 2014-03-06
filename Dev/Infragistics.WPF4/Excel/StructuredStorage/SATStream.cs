using System;
using System.Collections.Generic;
using System.Text;




namespace Infragistics.Documents.Excel.StructuredStorage

{
	internal sealed class SATStream : SATStreamBase
	{
		#region Member Variables

		private MasterSATStream masterSATStream;

		#endregion Member Variables

		#region Constructor

		public SATStream( StructuredStorageManager structuredStorage, MasterSATStream masterSATStream )
			: base( structuredStorage, 0, 1 )
		{
			this.masterSATStream = masterSATStream;

			List<SATEntry> SAT = new List<SATEntry>();

			// Mark the first sector as an SAT sector and the rest as free space
			SAT.Add( new SATEntry( StructuredStorageManager.EndOfStreamSectorId, StructuredStorageManager.SATSectorId ) );

			for ( int i = 1; i < this.SATEntriesPerSector; i++ )
				SAT.Add( SATEntry.FreeEntry );

			this.SetSATContents( SAT );
		}

		public SATStream( StructuredStorageManager structuredStorage, int firstSATSectorId, int numberOfSATSectors, MasterSATStream masterSATStream )
			: base( structuredStorage, firstSATSectorId, numberOfSATSectors )
		{
			this.masterSATStream = masterSATStream;
			this.ReadSATContents( firstSATSectorId, numberOfSATSectors );
		}

		#endregion Constructor

		#region Base Class Overrides

		#region AllocateNewSector

		protected override int AllocateNewSector( int lastSectorId )
		{
			int newSectorId = base.AllocateNewSector( lastSectorId );
			this.masterSATStream.AddNewSATEntry( newSectorId );
			return newSectorId;
		}

		#endregion AllocateNewSector

		#region Dispose

		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );

			if ( disposing )
				this.masterSATStream.Dispose();
		}

		#endregion Dispose

		#region GetNextSectorId

		internal override int GetNextSectorId( int lastSectorId, ref int steps )
		{
			while ( steps < 0 )
			{
				lastSectorId = this.masterSATStream.GetPreviousSATSector( lastSectorId );

				if ( lastSectorId < 0 )
					return -1;

				steps++;
			}

			while ( steps > 0 )
			{
				lastSectorId = this.masterSATStream.GetNextSATSector( lastSectorId );

				if ( lastSectorId < 0 )
					return -1;

				steps--;
			}

			return lastSectorId;
		}

		#endregion GetNextSectorId

		#region Type

		protected override StreamType Type
		{
			get { return StreamType.SAT; }
		}

		#endregion Type

		#endregion Base Class Overrides

		#region MasterSATStream

		public MasterSATStream MasterSATStream
		{
			get { return this.masterSATStream; }
		}

		#endregion MasterSATStream
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