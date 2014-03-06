using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal abstract class EscherRecordContainerBase : EscherRecordBase
	{
		private List<EscherRecordBase> childRecords;
		
		protected EscherRecordContainerBase( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x0F );
		}

		public void AddChildRecord( EscherRecordBase record )
		{
			if ( this.childRecords == null )
				this.childRecords = new List<EscherRecordBase>();

			if ( this.ValidateChildRecord( record ) )
			{
				record.ParentRecord = this;

				this.childRecords.Add( record );
				this.RecordLength += 8 + record.RecordLength;
			}
			else
			{
				Utilities.DebugFail( "This record cannot be stored in this container." );
			}
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 9/15/08 - TFS7442


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			this.childRecords = new List<EscherRecordBase>();

			long endPosition = manager.CurrentRecordStream.Position + this.RecordLength;

			while ( manager.CurrentRecordStream.Position < endPosition )
			{
				// MD 9/15/08 - TFS7442
				// Not sure why, but when multiple chart streams are written out in the ClientData record of a HostControl shape, the GroupContainer owning the
				// shape thinks it has extra data after the chart streams, but it doesn't seem like that is the case. Apparently, part of the chart streams must 
				// be counted in the GroupContainer's record length, but I can't figure out what it is. Since we don't support round-tripping for charts anyway,
				// just allow the container to stop loading children if this is the end of the record stream.
				if ( manager.CurrentRecordStream.Position == manager.CurrentRecordStream.Length )
				{
					Utilities.DebugFail( "The end of the stream has been reached and the remaining child records could not be read." );
					break;
				}

				EscherRecordBase childRecord = EscherRecordBase.LoadRecord( manager );

				if ( childRecord != null )
				{
					if ( this.ValidateChildRecord( childRecord ) )
					{
						childRecord.ParentRecord = this;
						this.childRecords.Add( childRecord );
					}
					else
						Utilities.DebugFail( "The loaded record cannot be stored in this container." );
				}
			}

			// MD 9/15/08 - TFS7442


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			if ( this.childRecords == null )
			{
				Utilities.DebugFail( "An empty container should not be saving." );
				return;
			}

			foreach ( EscherRecordBase childRecord in this.childRecords )
			{
				childRecord.Save( manager );
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			if ( this.childRecords != null )
			{
				foreach ( EscherRecordBase record in this.childRecords )
				{
					sb.Append( "\t" );
					sb.Append( record.ToString().Replace( "\n", "\n\t" ) );
					sb.Append( "\n" );
				}
			}

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		protected abstract bool ValidateChildRecord( EscherRecordBase childRecord );

		// MD 6/14/07 - BR23880
		// Other records may need to get the number of child records in the container
		public int ChildRecordCount
		{
			get { return this.childRecords == null ? 0 : this.childRecords.Count; }
		}
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