using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd946410(v=office.12).aspx
	internal class LbsDropData
	{
		private byte wStyle;
		private ushort cLine;
		private ushort dxMin;
		private string str;

		private LbsDropData() { }

		public static LbsDropData Load(Biff8RecordStream stream)
		{
			LbsDropData result = new LbsDropData();

			ushort temp = stream.ReadUInt16();
			result.wStyle = (byte)(temp & 0x0003);
			Debug.Assert(result.wStyle >= 0 && result.wStyle <= 2, "The value of the wStyle field must be between 0 and 2.");

			result.cLine = stream.ReadUInt16();
			Debug.Assert(result.cLine <= 0x7FFF, "The value of the cLines field must be less than or equal to 0x7FFF");

			result.dxMin = stream.ReadUInt16();
			Debug.Assert(result.dxMin <= 0x7FFF, "The value of the dxMin field must be less than or equal to 0x7FFF");

			long start = stream.Position;
			result.str = stream.ReadXLUnicodeString();

			long strLength = stream.Position - start;
			if (strLength % 2 == 1)
				stream.ReadByte();

			return result;
		}

		public void Save(Biff8RecordStream stream)
		{
			ushort temp = 0;
			temp |= this.wStyle;

			stream.Write(temp);
			stream.Write(this.cLine);
			stream.Write(this.dxMin);

			long start = stream.Position;
			stream.WriteXLUnicodeString(this.str);

			long strLength = stream.Position - start;
			if (strLength % 2 == 1)
				stream.WriteByte(0);
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