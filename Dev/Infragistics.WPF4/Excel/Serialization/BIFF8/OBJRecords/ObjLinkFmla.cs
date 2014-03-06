using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd953621(v=office.12).aspx
	internal class ObjLinkFmla
	{
		private ObjFmla fmla;

		private ObjLinkFmla() { }

		public void Save(Biff8RecordStream stream, Obj owner)
		{
			if (fmla == null)
			{
				Utilities.DebugFail("This record should only exist if it has a fmla value.");
				return;
			}

			ushort ft;
			switch (owner.Cmo.Ot)
			{
				case ObjectType.CheckBox:
				case ObjectType.OptionButton:
					ft = (ushort)OBJRecordType.CheckBoxLinkFormulaStyleMacro;
					break;

				case ObjectType.Spinner:
				case ObjectType.ScrollBar:
				case ObjectType.ListBox:
				case ObjectType.ComboBox:
					ft = (ushort)OBJRecordType.ScrollBarFormulaStyleMacro;
					break;

				default:
					Utilities.DebugFail("The ObjLinkFmla should not exist.");
					return;
			}

			stream.Write(ft);
			this.fmla.Save(stream);
		}

		public static ObjLinkFmla TryLoad(Biff8RecordStream stream, Obj owner)
		{
			switch (owner.Cmo.Ot)
			{
				case ObjectType.CheckBox:
				case ObjectType.OptionButton:
					if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.CheckBoxLinkFormulaStyleMacro, false) == false)
						return null;

					break;

				case ObjectType.Spinner:
				case ObjectType.ScrollBar:
				case ObjectType.ListBox:
				case ObjectType.ComboBox:
					if (ObjUtilities.ReadAndVerifyFt(stream, OBJRecordType.ScrollBarFormulaStyleMacro, false) == false)
						return null;

					break;

				default:
					Utilities.DebugFail("The ObjLinkFmla should not exist.");
					return null;
			}

			ObjLinkFmla result = new ObjLinkFmla();
			result.fmla = ObjFmla.Load(stream);
			return result;
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