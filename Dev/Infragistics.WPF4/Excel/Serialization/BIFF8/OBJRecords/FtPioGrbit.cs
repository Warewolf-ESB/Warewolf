using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd908979(v=office.12).aspx
	internal class FtPioGrbit
	{
		private const ushort RecordSize = 0x0002;

		private const ushort FAutoPictBit		= 0x0001;
		private const ushort FDdeBit			= 0x0002;
		private const ushort FPrintCalcBit		= 0x0004;
		private const ushort FIconBit			= 0x0008;
		private const ushort FCtlBit			= 0x0010;
		private const ushort FPrstmBit			= 0x0020;
		private const ushort FCameraBit			= 0x0080;
		private const ushort FDefaultSizeBit	= 0x0100;
		private const ushort FAutoLoadBit		= 0x0200;

		private ushort data = 1;

		public FtPioGrbit() { }

		public static FtPioGrbit Load(BiffRecordStream stream)
		{
			if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.PictureOptionFlags) == false)
				return null;

			FtPioGrbit result = new FtPioGrbit();

			ushort cb = stream.ReadUInt16();
			Debug.Assert(cb == RecordSize, "The cb field is incorrect for an FtPioGrbit");

			result.data = stream.ReadUInt16();
			Debug.Assert(result.FDde == false || result.FCtl == false, "fDde and DCtrl cannot both be True.");

			return result;
		}

		public void Save(Biff8RecordStream stream)
		{
			stream.Write((ushort)OBJRecordType.PictureOptionFlags);
			stream.Write(RecordSize);
			stream.Write(this.data);
		}

		public bool FAutoLoad
		{
			get { return (this.data & FAutoLoadBit) == FAutoLoadBit; }
		}

		public bool FAutoPict
		{
			get { return (this.data & FAutoPictBit) == FAutoPictBit; }
		}

		public bool FCamera
		{
			get { return (this.data & FCameraBit) == FCameraBit; }
		}

		public bool FCtl
		{
			get { return (this.data & FCtlBit) == FCtlBit; }
		}

		public bool FDde
		{
			get { return (this.data & FDdeBit) == FDdeBit; }
		}

		public bool FDefaultSize
		{
			get { return (this.data & FDefaultSizeBit) == FDefaultSizeBit; }
		}

		public bool FIcon
		{
			get { return (this.data & FIconBit) == FIconBit; }
		}

		public bool FPrstm
		{
			get { return (this.data & FPrstmBit) == FPrstmBit; }
		}

		public bool FPrintCalc
		{
			get { return (this.data & FPrintCalcBit) == FPrintCalcBit; }
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