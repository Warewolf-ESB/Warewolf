using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd905189(v=office.12).aspx
	internal class FtLbsData
	{
		private ushort cbFContinued;
		private ObjFmla fmla;
		private ushort cLines;
		private ushort iSel;
		private ushort flags;
		private ushort idEdit;
		private LbsDropData dropData;
		private string[] rgLines;
		private bool[] bsels;

		private FtLbsData() { }

		public static FtLbsData Load(Biff8RecordStream stream, Obj owner)
		{
			if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.ListBoxData) == false)
				return null;

			FtLbsData result = new FtLbsData();

			result.cbFContinued = stream.ReadUInt16();
			if (result.cbFContinued != 0)
			{
				result.fmla = ObjFmla.Load(stream);
				result.cLines = stream.ReadUInt16();
				Debug.Assert(result.cLines <= 0x7FFF, "The cLines field must be less than or equal to 0x7FFF.");

				result.iSel = stream.ReadUInt16();
				Debug.Assert(result.iSel <= result.cLines, "The iSel field must be less than or equal to the cLines field.");

				result.flags = stream.ReadUInt16();
				bool fValidPlex = (result.flags & 0x0002) == 0x0002;
				byte wListSelType = (byte)((result.flags & 0x0030) >> 4);

				result.idEdit = stream.ReadUInt16();

				if (owner.Cmo.Ot == ObjectType.ComboBox)
					result.dropData = LbsDropData.Load(stream);

				if (fValidPlex)
				{
					result.rgLines = new string[result.cLines];
					for (int i = 0; i < result.cLines; i++)
						result.rgLines[i] = stream.ReadXLUnicodeString();
				}

				if (wListSelType != 0)
				{
					result.bsels = new bool[result.cLines];
					for (int i = 0; i < result.cLines; i++)
					{
						byte value = (byte)stream.ReadByte();
						Debug.Assert(value == 0 || value == 1, "Each value in the bsels field must be zero or one.");
						result.bsels[i] = (value != 0);
					}
				}
			}

			return result;
		}

		public void Save(Biff8RecordStream stream, Obj owner)
		{
			stream.Write((ushort)OBJRecordType.ListBoxData);
			stream.Write(this.cbFContinued);

			if (this.cbFContinued == 0)
				return;

			this.fmla.Save(stream);
			stream.Write(this.cLines);
			stream.Write(this.iSel);
			stream.Write(this.flags);
			stream.Write(this.idEdit);

			if (owner.Cmo.Ot == ObjectType.ComboBox)
			{
				Debug.Assert(this.dropData != null, "We must have a dropData field in this case.");
				if (this.dropData != null)
					this.dropData.Save(stream);
			}

			if (this.rgLines != null)
			{
				for (int i = 0; i < this.cLines; i++)
					stream.WriteXLUnicodeString(this.rgLines[i]);
			}

			if (this.bsels != null)
			{
				for (int i = 0; i < this.cLines; i++)
					stream.Write((byte)(this.bsels[i] ? 1 : 0));
			}
		}

		public ObjFmla Fmla
		{
			get { return this.fmla; }
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