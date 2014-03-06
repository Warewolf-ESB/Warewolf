using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 11/29/11 - TFS96205
	// http://msdn.microsoft.com/en-us/library/dd925873(v=office.12).aspx
	internal class XFEXTRecord : Biff8RecordBase
	{
		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.ReadFrtHeader();

			ushort reserved1 = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(reserved1 == 0, "The reserved2 value must be zero.");

			ushort ixfe = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(ixfe <= 4050, "The ixfe value is out of range.");

			ushort reserved2 = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(reserved2 == 0, "The reserved2 value must be zero.");

			ushort cexts = manager.CurrentRecordStream.ReadUInt16();

			ExtProp[] props = new ExtProp[cexts];
			for (int i = 0; i < cexts; i++)
				props[i] = manager.CurrentRecordStream.ReadExtProp();

			if (manager.IgnoreXFEXTData)
				return;

			if (manager.Formats.Count <= ixfe)
			{
				Utilities.DebugFail("Invalid ixfe value.");
				return;
			}

			// MD 1/17/12 - 12.1 - Cell Format Updates
			// We can apply these immediately, because  we don't need to wait for the theme colors to be resolved anymore.
			//Debug.Assert(manager.ExtProps.ContainsKey(ixfe) == false, "The current format data shouldn't have extProps yet.");
			//manager.ExtProps[ixfe] = props;
			WorksheetCellFormatData format = manager.Formats[ixfe];
			WorksheetCellFormatOptions originalFormatOptions = format.FormatOptions;

			for (int i = 0; i < cexts; i++)
				props[i].ApplyTo(manager, format);

			format.SetFormatOptionsInternal(originalFormatOptions);
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// There is now an XFContext on the stack instead of just an WorksheetCellFormatData
			//WorksheetCellFormatData format = (WorksheetCellFormatData)manager.ContextStack[typeof(WorksheetCellFormatData)];
			//
			//if (format == null)
			//{
			//    Utilities.DebugFail("There was no format in the context stack.");
			//    return;
			//}
			XFRecord.XFContext xfContext = (XFRecord.XFContext)manager.ContextStack[typeof(XFRecord.XFContext)];
			if (xfContext == null)
			{
				Utilities.DebugFail("There was no XFContext in the context stack.");
				return;
			}
			WorksheetCellFormatData format = xfContext.FormatData;
			List<ExtProp> extProps = xfContext.ExtProps;
			Debug.Assert(extProps.Count != 0, "We should have XFEXT props here.");

			manager.CurrentRecordStream.WriteFrtHeader();
			manager.CurrentRecordStream.Write((ushort)0); // reserved1

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// We no longer cache format indexes because we can easily get them at save time.
			//manager.CurrentRecordStream.Write((ushort)format.IndexInFormatCollection); // ixfe
			manager.CurrentRecordStream.Write(xfContext.XfId); // ixfe

			manager.CurrentRecordStream.Write((ushort)0); // reserved2
			manager.CurrentRecordStream.Write((ushort)extProps.Count); // cexts

			for (int i = 0; i < extProps.Count; i++)
				manager.CurrentRecordStream.WriteExtProp(extProps[i]);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.XFEXT; }
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