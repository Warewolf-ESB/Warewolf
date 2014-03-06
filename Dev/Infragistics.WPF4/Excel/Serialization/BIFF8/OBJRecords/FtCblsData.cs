using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd909618(v=office.12).aspx
	internal class FtCblsData
	{
		private const ushort RecordSize = 0x0008;

		private ushort fChecked;
		private ushort accel;
		private bool fNo3d;

		private FtCblsData() { }

		public static FtCblsData Load(BiffRecordStream stream, Obj owner)
		{
			if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.CheckBoxData) == false)
				return null;

			FtCblsData result = new FtCblsData();

			ushort cb = stream.ReadUInt16();
			Debug.Assert(cb == RecordSize, "The cb field is incorrect for an FtCblsData");

			result.fChecked = stream.ReadUInt16();

			Debug.Assert(result.fChecked != 0x0002 || owner.Cmo.Ot == ObjectType.CheckBox, 
				"The fChecked field MUST NOT have the value 0x0002 if the cmo.ot field of the Obj record that contains this FtPioGrbit is not equal to 0x0B.");

			result.accel = stream.ReadUInt16();
			ushort mustBeZero = stream.ReadUInt16();
			Debug.Assert(mustBeZero == 0, "The reserved field must be zero.");

			ushort temp = stream.ReadUInt16();
			result.fNo3d = (temp & 0x0001) == 0x0001;

			return result;
		}

		internal void Save(Biff8RecordStream stream, Obj obj)
		{
			ushort temp = 0;

			if (this.fNo3d)
				temp |= 0x0001;

			stream.Write((ushort)OBJRecordType.CheckBoxData); // ft
			stream.Write(RecordSize); // cb
			stream.Write(this.fChecked);
			stream.Write(this.accel);
			stream.Write((ushort)0x0000);
			stream.Write(temp);
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