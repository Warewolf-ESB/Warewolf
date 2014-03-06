using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd951595(v=office.12).aspx
	internal class FtRboData
	{
		private const ushort RecordSize = 0x0004;

		private ushort idRadNext;
		private WorksheetShape idRadNextShape;
		private bool fFirstBtn;

		private FtRboData() { }

		public static FtRboData Load(BiffRecordStream stream)
		{
			if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.RadioButtonData) == false)
				return null;

			FtRboData result = new FtRboData();

			ushort cb = stream.ReadUInt16();
			Debug.Assert(cb == RecordSize, "The cb field is incorrect for an FtRboData");

			result.idRadNext = stream.ReadUInt16();
			ushort fFirstBtn = stream.ReadUInt16();

			Debug.Assert(fFirstBtn == 0 || fFirstBtn == 1, "The fFirstBtn must be zero or one.");
			result.fFirstBtn = (fFirstBtn != 0);

			return result;
		}

		public  void Save(Biff8RecordStream stream)
		{
			if (this.idRadNextShape != null && this.idRadNextShape.Worksheet != null)
				this.idRadNext = this.idRadNextShape.Obj.Cmo.Id;
			else
				this.idRadNext = 0;

			stream.Write((ushort)OBJRecordType.RadioButtonData);
			stream.Write(RecordSize);
			stream.Write(this.idRadNext);
			stream.Write((ushort)(this.fFirstBtn ? 1 : 0));
		}

		public ushort IdRadNext
		{
			get { return this.idRadNext; }
		}

		public WorksheetShape IdRadNextShape
		{
			get { return this.idRadNextShape; }
			set { this.idRadNextShape = value; }
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