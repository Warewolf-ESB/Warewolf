using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 2/19/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd906119(v=office.12).aspx
	internal class TABLESTYLESRecord : Biff8RecordBase
	{
		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.ReadFrtHeader();
			uint cts = manager.CurrentRecordStream.ReadUInt32();
			Debug.Assert(cts >= 144, "The cts value is incorrect.");
			ushort cchDefTableStyle = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(cchDefTableStyle <= 255, "The cchDefTableStyle value is incorrect.");
			ushort cchDefPivotStyle = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(cchDefTableStyle <= 255, "The cchDefPivotStyle  value is incorrect.");

			string rgchDefTableStyle = Encoding.Unicode.GetString(manager.CurrentRecordStream.ReadBytes(cchDefTableStyle * 2));
			string rgchDefPivotStyle = Encoding.Unicode.GetString(manager.CurrentRecordStream.ReadBytes(cchDefPivotStyle * 2));

			manager.Workbook.DefaultTableStyle = manager.Workbook.GetTableStyle(rgchDefTableStyle);
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.WriteFrtHeader();

			uint cts = (uint)(144 + manager.Workbook.CustomTableStyles.Count);
			manager.CurrentRecordStream.Write(cts);

			string rgchDefTableStyle = manager.Workbook.DefaultTableStyle.Name;
			string rgchDefPivotStyle = "PivotStyleMedium9";
			manager.CurrentRecordStream.Write((ushort)rgchDefTableStyle.Length);
			manager.CurrentRecordStream.Write((ushort)rgchDefPivotStyle.Length);

			manager.CurrentRecordStream.Write(Encoding.Unicode.GetBytes(rgchDefTableStyle));
			manager.CurrentRecordStream.Write(Encoding.Unicode.GetBytes(rgchDefPivotStyle));
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.TABLESTYLES; }
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