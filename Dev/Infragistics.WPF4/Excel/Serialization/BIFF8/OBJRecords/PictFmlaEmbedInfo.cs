using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd925899(v=office.12).aspx
	internal class PictFmlaEmbedInfo
	{
		private string strClass;

		private PictFmlaEmbedInfo() { }

		public static PictFmlaEmbedInfo Load(Biff8RecordStream stream)
		{
			PictFmlaEmbedInfo result = new PictFmlaEmbedInfo();

			byte ttb = (byte)stream.ReadByte();
			Debug.Assert(ttb == 0x03, "The ttb field is incorrect.");

			byte cbClass = (byte)stream.ReadByte();

			byte reserved = (byte)stream.ReadByte();
			Debug.Assert(reserved == 0x00, "The reserved field is incorrect.");

			if (cbClass != 0)
				result.strClass = stream.ReadXLUnicodeStringNoCch(cbClass);

			return result;
		}

		public void Save(Biff8RecordStream stream)
		{

			byte cbClass = 0;
			if (this.strClass != null)
				cbClass = (byte)this.strClass.Length;

			stream.WriteByte(0x03); // ttb
			stream.WriteByte(cbClass);
			stream.WriteByte(0x00); // reserved
			stream.WriteXLUnicodeStringNoCch(this.strClass);
		}

		public string StrClass
		{
			get { return this.strClass; }
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