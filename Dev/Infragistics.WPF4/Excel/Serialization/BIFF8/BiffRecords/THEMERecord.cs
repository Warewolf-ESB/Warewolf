using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 11/29/11 - TFS96205
	// http://msdn.microsoft.com/en-us/library/dd920392(v=office.12).aspx
	internal class THEMERecord : Biff8RecordBase
	{
		private const uint CustomTheme = 0;
		private const uint DefaultTheme = 124226;

		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.ReadFrtHeader();

			uint dwThemeVersion = manager.CurrentRecordStream.ReadUInt32();
			if (dwThemeVersion == THEMERecord.CustomTheme)
			{
				
				
				
				
				
				
				
				
				
				int remainingData = (int)(manager.CurrentRecordStream.Length - manager.CurrentRecordStream.Position);
				manager.Workbook.CustomThemeData = manager.CurrentRecordStream.ReadBytes(remainingData);
			}
			else if (dwThemeVersion == THEMERecord.DefaultTheme)
			{
				// MD 1/16/12 - 12.1 - Cell Format Updates
				// The theme colors are now populated automatically on the workbook.
				//manager.PopulateDefaultThemeColors();
			}
			else
			{
				Utilities.DebugFail("dwThemeVersion might be incorrect.");
			}

			// MD 1/17/12 - 12.1 - Cell Format Updates
			//manager.OnThemesLoaded();
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.WriteFrtHeader();

			if (manager.Workbook.CustomThemeData != null)
			{
				manager.CurrentRecordStream.Write(THEMERecord.CustomTheme); // dwThemeVersion
				manager.CurrentRecordStream.Write(manager.Workbook.CustomThemeData);
			}
			else
			{
				manager.CurrentRecordStream.Write(THEMERecord.DefaultTheme); // dwThemeVersion
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.THEME; }
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