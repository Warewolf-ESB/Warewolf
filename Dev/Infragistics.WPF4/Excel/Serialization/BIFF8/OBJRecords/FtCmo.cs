using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd906115(v=office.12).aspx
	internal class FtCmo
	{
		#region Constants

		private const ushort RecordSize = 0x0012;

		private const ushort FLockedBit = 0x0001;
		private const ushort FDefaultSizeBit = 0x0004;
		private const ushort FPublishedBit = 0x0004;
		private const ushort FPrintBit = 0x0008;
		private const ushort FDisabledBit = 0x0080;
		private const ushort FUIObjBit = 0x0100;
		private const ushort FRecalcObjBit = 0x0200;
		private const ushort FRecalcObjAlwaysBit = 0x1000;

		#endregion // Constants

		#region Member Variables

		private ObjectType ot;
		private ushort id;
		private ushort optionFlags;
		private uint unused1;
		private uint unused2;
		private uint unused3;

		#endregion // Member Variables

		#region Constructor

		private FtCmo() { }

		public FtCmo(ObjectType objectType)
		{
			this.ot = objectType;

			if (objectType == ObjectType.Comment)
				this.optionFlags = 0x4011;
			else
				this.optionFlags = 0x6011;
		}

		#endregion // Constructor

		#region Base Class Overrides

		public static FtCmo Load(BiffRecordStream stream)
		{
			if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.CommonObjectData) == false)
				return null;

			FtCmo result = new FtCmo();

			ushort cb = stream.ReadUInt16();
			Debug.Assert(cb == RecordSize, "The cb field is incorrect for an FtCmo");

			result.ot = (ObjectType)stream.ReadUInt16();
			result.id = stream.ReadUInt16();
			result.optionFlags = stream.ReadUInt16();

			Debug.Assert(Enum.IsDefined(typeof(ObjectType), result.ot));

			result.unused1 = stream.ReadUInt32();
			result.unused2 = stream.ReadUInt32();
			result.unused3 = stream.ReadUInt32();

			return result;
		}

		public void Save(Biff8RecordStream stream)
		{
			stream.Write((ushort)OBJRecordType.CommonObjectData); // ft
			stream.Write(RecordSize); // cb

			stream.Write((ushort)this.ot);
			stream.Write(this.id);
			stream.Write(this.optionFlags);

			stream.Write(this.unused1);
			stream.Write(this.unused2);
			stream.Write(this.unused3);
		}

		#endregion // Base Class Overrides

		#region Properties

		public bool FDefaultSize
		{
			get { return (this.optionFlags & FDefaultSizeBit) == FDefaultSizeBit; }
		}

		public bool FDisabled
		{
			get { return (this.optionFlags & FDisabledBit) == FDisabledBit; }
		}

		public bool FLocked
		{
			get { return (this.optionFlags & FLockedBit) == FLockedBit; }
		}

		public bool FPrint
		{
			get { return (this.optionFlags & FPrintBit) == FPrintBit; }
		}

		public bool FPublished
		{
			get { return (this.optionFlags & FPublishedBit) == FPublishedBit; }
		}

		public bool FRecalcObj
		{
			get { return (this.optionFlags & FRecalcObjBit) == FRecalcObjBit; }
		}

		public bool FRecalcObjAlways
		{
			get { return (this.optionFlags & FRecalcObjAlwaysBit) == FRecalcObjAlwaysBit; }
		}

		public bool FUIObj
		{
			get { return (this.optionFlags & FUIObjBit) == FUIObjBit; }
		}

		public ushort Id
		{
			get { return this.id; }
			set { this.id = value; }
		}

		public ObjectType Ot
		{
			get { return this.ot; }
		}

		#endregion // Properties
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