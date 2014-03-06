using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd904851(v=office.12).aspx
	internal class FtPictFmla
	{
		private ObjFmla fmla;
		private uint lPosInCtlStm;
		private uint cbBufInCtlStm;
		private PictFmlaKey key;

		private FtPictFmla() { }

		public Formula GetFormula()
		{
			if (this.fmla == null)
				return null;

			return this.fmla.GetFormula();
		}

		public void Save(Biff8RecordStream stream, Obj owner)
		{
			if (this.fmla == null)
			{
				Utilities.DebugFail("The fmla field must not be null.");
				return;
			}

			if (owner.PictFlags.FCtl && this.key == null)
			{
				Utilities.DebugFail("The key field must not be null if the pcitFlags.fCtl flag is True in the owning OBJ record.");
				return;
			}

			stream.Write((ushort)OBJRecordType.PictureFormulaStyleMacro);
			stream.Write((ushort)0); // cb

			long start = stream.Position;

			this.fmla.Save(stream);

			Formula formula = this.GetFormula();
			if (formula != null &&
				formula.PostfixTokenList.Count >= 1 &&
				formula.PostfixTokenList[0] is TblToken)
			{
				stream.Write(this.lPosInCtlStm);

				if (owner.PictFlags.FPrstm)
					stream.Write(this.cbBufInCtlStm);
			}

			if (owner.PictFlags.FCtl)
				this.key.Save(stream);

			long end = stream.Position;
			ushort cb = (ushort)(end - start);
			stream.Position = start - 2;
			stream.Write(cb);

			stream.Position = end;
		}

		public static FtPictFmla TryLoad(Biff8RecordStream stream, Obj owner)
		{
			if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.PictureFormulaStyleMacro, false) == false)
				return null;

			FtPictFmla result = new FtPictFmla();

			ushort cb = stream.ReadUInt16();

			long start = stream.Position;

			result.fmla = ObjFmla.Load(stream);

			Formula formula = result.GetFormula();
			if (formula != null &&
				formula.PostfixTokenList.Count >= 1 &&
				formula.PostfixTokenList[0] is TblToken)
			{
				result.lPosInCtlStm = stream.ReadUInt32();

				if (owner.PictFlags.FPrstm)
					result.cbBufInCtlStm = stream.ReadUInt32();
			}

			if (owner.PictFlags.FCtl)
				result.key = PictFmlaKey.Load(stream);

			Debug.Assert(cb == stream.Position - start, "The cb field was incorrect.");
			return result;
		}

		public ObjFmla Fmla
		{
			get { return this.fmla; }
		}

		public PictFmlaKey Key
		{
			get { return this.key; }
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