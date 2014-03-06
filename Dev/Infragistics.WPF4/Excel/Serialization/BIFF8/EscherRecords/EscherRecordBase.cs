using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	
	internal abstract class EscherRecordBase
	{
		private EscherRecordContainerBase parentRecord;

		private byte version;
		private ushort instance;
		private uint recordLength;

		protected EscherRecordBase( byte version, ushort instance, uint recordLength )
		{
			this.version = version;
			this.instance = instance;
			this.recordLength = recordLength;
		}

		public EscherRecordContainerBase ParentRecord
		{
			get { return this.parentRecord; }
			// JJD 12/14/07
            // Removed protected qualifier here so that VS2008 comipler would compile it.
            // It seems that they got more strict 
            //protected set
			set
			{
				// MD 6/14/07 - BR23880
				// This assert is not valid anymore
				//Debug.Assert( this.parentRecord == null );
				this.parentRecord = value;
			}
		}

		public ushort Instance
		{
			get { return this.instance; }
			protected set { this.instance = value; }
		}

		public uint RecordLength
		{
			get { return this.recordLength; }
			protected set 
			{
				if ( this.ParentRecord != null )
				{
					if ( this.recordLength < value )
						this.ParentRecord.RecordLength += value - this.recordLength;
					else
						this.ParentRecord.RecordLength -= this.recordLength - value;
				}

				this.recordLength = value;
			}
		}

		public override string ToString()
		{
			return this.Type.ToString() + ": " + this.recordLength;
		}

		public abstract void Load( BIFF8WorkbookSerializationManager manager );
		public virtual void Save( BIFF8WorkbookSerializationManager manager )
		{
			int header = 0;

			header |= version;
			header |= ( ( instance & 0x0FFF ) << 4 );
			header |= ( (int)this.Type << 16 );

			manager.CurrentRecordStream.Write( (uint)header );
			manager.CurrentRecordStream.Write( this.recordLength );
		}

		public abstract EscherRecordType Type { get;}

		public static EscherRecordBase LoadRecord( BIFF8WorkbookSerializationManager manager )
		{
			uint header = manager.CurrentRecordStream.ReadUInt32();
			uint recordLength = manager.CurrentRecordStream.ReadUInt32();

			byte version =						(byte)( ( header & 0x0000000F ) );
			ushort instance =				  (ushort)( ( header & 0x0000FFF0 ) >> 4 );
			EscherRecordType type = (EscherRecordType)( ( header & 0xFFFF0000 ) >> 16 );

			EscherRecordBase record = EscherRecordBase.CreateEscherRecord( type, version, instance, recordLength );

			long endPosition = manager.CurrentRecordStream.Position + recordLength;

			if ( record != null )
			{
				// MD 9/15/08 - TFS7442




				Debug.Assert( record.Type == type, "The wrong record has been created." );
				record.Load( manager );
				Debug.Assert( endPosition == manager.CurrentRecordStream.Position, "The full record wasn't read in for the type " + type );
			}

			manager.CurrentRecordStream.Position = endPosition;

			return record;
		}

		private static EscherRecordBase CreateEscherRecord( EscherRecordType type, byte version, ushort instance, uint recordLength )
		{
			switch ( type )
			{
				case EscherRecordType.DrawingGroupContainer:	return new DrawingGroupContainer( version, instance, recordLength );
				case EscherRecordType.BLIPStoreContainer:		return new BLIPStoreContainer( version, instance, recordLength );
				case EscherRecordType.DrawingContainer:			return new DrawingContainer( version, instance, recordLength );
				case EscherRecordType.GroupContainer:			return new GroupContainer( version, instance, recordLength );
				case EscherRecordType.ShapeContainer:			return new ShapeContainer( version, instance, recordLength );
				case EscherRecordType.SolverContainer:			return new SolverContainer( version, instance, recordLength );	// MD 6/6/07 - BR23645
				case EscherRecordType.DrawingGroup:				return new DrawingGroup( version, instance, recordLength );
				case EscherRecordType.BLIPStoreEntry:			return new BLIPStoreEntry( version, instance, recordLength );
				case EscherRecordType.Drawing:					return new Drawing( version, instance, recordLength );
				case EscherRecordType.GroupShape:				return new GroupShape( version, instance, recordLength );
				case EscherRecordType.Shape:					return new Shape( version, instance, recordLength );
				case EscherRecordType.PropertyTable1:			return new PropertyTable1( version, instance, recordLength );
				case EscherRecordType.Textbox:					break;
				case EscherRecordType.ClientTextbox:			return new ClientTextBox( version, instance, recordLength );
				case EscherRecordType.Anchor:					break;
				case EscherRecordType.ChildAnchor:				return new ChildAnchor( version, instance, recordLength );
				case EscherRecordType.ClientAnchor:				return new ClientAnchor( version, instance, recordLength );
				case EscherRecordType.ClientData:				return new ClientData( version, instance, recordLength );
				case EscherRecordType.ConnectorRule:			return new ConnectorRule( version, instance, recordLength );	 // MD 8/1/07 - BR25039
				case EscherRecordType.AlignRule:				break;
				case EscherRecordType.ArcRule:					break;
				case EscherRecordType.ClientRule:				break;
				case EscherRecordType.ClassID:					break;
				case EscherRecordType.CalloutRule:				return new CalloutRule( version, instance, recordLength );		 // MD 6/6/07 - BR23645
				case EscherRecordType.Regroup:					return new Regroup(version, instance, recordLength);			 // MD 10/30/11 - TFS90733
				case EscherRecordType.Selections:				return new Selections( version, instance, recordLength );		 // MD 7/20/2007 - BR25039
				case EscherRecordType.ColorMRU:					break;
				case EscherRecordType.DeletedPSPL:				break;
				case EscherRecordType.SplitMenuColors:			return new SplitMenuColors( version, instance, recordLength );
				case EscherRecordType.OLEObject:				break;
				case EscherRecordType.ColorScheme:				break;
				case EscherRecordType.PropertyTable2:			return new PropertyTable2( version, instance, recordLength );
				case EscherRecordType.PropertyTable3:			return new PropertyTable3( version, instance, recordLength );

				default:
					if ( EscherRecordType.BLIPMin <= type && type <= EscherRecordType.BLIPMax )
						return new BLIP( type, version, instance, recordLength );

					Utilities.DebugFail( "Unknown escher record type: " + type );
					return null;
			}

			Utilities.DebugFail( "A known escher record type was not created: " + type );
			return null;
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