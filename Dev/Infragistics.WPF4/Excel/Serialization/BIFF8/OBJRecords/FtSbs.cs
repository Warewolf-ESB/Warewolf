using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd910227(v=office.12).aspx
	internal class FtSbs
	{
		private const ushort RecordSize = 0x0014;

		private uint unused1;
		private ushort iVal;
		private ushort iMin;
		private ushort iMax;
		private ushort dInc;
		private ushort dPage;
		private ushort fHoriz;
		private ushort dxScroll;
		private ushort flags;

		private FtSbs() { }

		public static FtSbs Load(BiffRecordStream stream)
		{
			if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.ScrollBar) == false)
				return null;

			FtSbs result = new FtSbs();

			ushort cb = stream.ReadUInt16();
			Debug.Assert(cb == RecordSize, "The cb field is incorrect for an FtSbs");

			result.unused1 = stream.ReadUInt32();
			result.iVal = stream.ReadUInt16();
			result.iMin = stream.ReadUInt16();
			result.iMax = stream.ReadUInt16();
			result.dInc = stream.ReadUInt16();
			Debug.Assert(result.dInc > 0, "the dInc value must be greater than zero.");

			result.dPage = stream.ReadUInt16();
			Debug.Assert(result.dPage > 0, "the dInc value must be greater than zero.");

			result.fHoriz = stream.ReadUInt16();
			Debug.Assert(result.fHoriz == 0 || result.fHoriz == 1, "The fHoriz field must be zero or one.");

			result.dxScroll = stream.ReadUInt16();

			result.flags = stream.ReadUInt16();

			return result;
		}

		public void Save(Biff8RecordStream stream)
		{
			stream.Write((ushort)OBJRecordType.ScrollBar);
			stream.Write(RecordSize);
			stream.Write(this.unused1);
			stream.Write(this.iVal);
			stream.Write(this.iMin);
			stream.Write(this.iMax);
			stream.Write(this.dInc);
			stream.Write(this.dPage);
			stream.Write(this.fHoriz);
			stream.Write(this.dxScroll);
			stream.Write(this.flags);
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