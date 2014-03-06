using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd926376(v=office.12).aspx
	internal class PictFmlaKey
	{
		private byte[] keyBuf;
		private ObjFmla fmlaLinkedCell;
		private ObjFmla fmlaListFillRange;

		private PictFmlaKey() { }

		public static PictFmlaKey Load(Biff8RecordStream stream)
		{
			PictFmlaKey result = new PictFmlaKey();

			uint cbKey = stream.ReadUInt32();
			result.keyBuf = stream.ReadBytes((int)cbKey);
			result.fmlaLinkedCell = ObjFmla.Load(stream);
			result.fmlaListFillRange = ObjFmla.Load(stream);

			return result;
		}

		public void Save(Biff8RecordStream stream)
		{
			if (this.fmlaLinkedCell == null)
			{
				Utilities.DebugFail("The fmlaLinkedCell field must not be null.");
				return;
			}

			if (this.fmlaListFillRange == null)
			{
				Utilities.DebugFail("The fmlaListFillRange field must not be null.");
				return;
			}

			stream.Write((uint)this.keyBuf.Length);
			stream.Write(this.keyBuf);
			this.fmlaLinkedCell.Save(stream);
			this.fmlaListFillRange.Save(stream);
		}

		public ObjFmla FmlaLinkedCell
		{
			get { return this.fmlaLinkedCell; }
		}

		public ObjFmla FmlaListFillRange
		{
			get { return this.fmlaListFillRange; }
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