using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 1/26/12 - 12.1 - Cell Format Updates
	// http://msdn.microsoft.com/en-us/library/dd948156(v=office.12).aspx
	internal class STYLEEXTRecord : Biff8RecordBase
	{
		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.ReadFrtHeader();

			WorkbookStyleCollection styles = manager.Workbook.Styles;
			List<WorkbookStyle> loadedStyles = manager.LoadedStyles;

			WorkbookStyle style = loadedStyles[loadedStyles.Count - 1];
			WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;

			byte temp8 = (byte)manager.CurrentRecordStream.ReadByte();
			bool fBuiltIn = Utilities.TestBit(temp8, 0);
			bool fHidden = Utilities.TestBit(temp8, 1);
			bool fCustom = Utilities.TestBit(temp8, 2);
			Debug.Assert(fCustom == false || fBuiltIn, "If fCustom is True, so should fBuiltIn");

			if (fHidden)
				styles.Remove(style);

			byte iCategory = (byte)manager.CurrentRecordStream.ReadByte();
			ushort builtInData = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(fBuiltIn || builtInData == 0xFFFF, "The builtInData should be 0xFFFF when fBuiltIn is False.");

			if (fBuiltIn)
			{
				byte iLevel = (byte)(builtInData >> 8);
				byte istyBuiltIn = (byte)(builtInData & 0xFF);

				// MD 3/26/12
				// Found while fixing TFS106075
				// The fBuiltIn value could be written out incorrectly, so also check that the built-in type is valid.
				if (Enum.IsDefined(typeof(BuiltInStyleType), (BuiltInStyleType)istyBuiltIn))
				{
					if (builtInStyle == null)
					{
						builtInStyle = new WorkbookBuiltInStyle(manager.Workbook, style.StyleFormatInternal, (BuiltInStyleType)istyBuiltIn, iLevel);
						styles.Remove(style);
						styles.Add(builtInStyle);
						style = builtInStyle;

						loadedStyles[loadedStyles.Count - 1] = style;
					}

					Debug.Assert(istyBuiltIn == (byte)builtInStyle.Type, "The istyBuiltIn value does not match that of the built in style.");
					Debug.Assert(iLevel == builtInStyle.OutlineLevel, "The iLevel value does not match that of the built in style.");
				}
			}

			string stName = manager.CurrentRecordStream.ReadLPWideString();

			// The built in style names are localized and we do not support that right now, so don't check for matching names on built in styles.
			Debug.Assert(fBuiltIn || stName == style.Name, "The stName value does not match that of the style.");

			manager.CurrentRecordStream.ReadXFProps(style.StyleFormatInternal, false);

			if (builtInStyle != null)
				builtInStyle.IsCustomized = fCustom;
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			WorkbookStyle style = manager.ContextStack.Get<WorkbookStyle>();
			if (style == null)
			{
				Utilities.DebugFail("Cannot find the WorkbookStyle on the context stack.");
				return;
			}

			WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
			bool fCustom = builtInStyle != null && builtInStyle.IsCustomized;

			manager.CurrentRecordStream.WriteFrtHeader();

			byte temp8 = 0;
			Utilities.SetBit(ref temp8, style.IsBuiltIn, 0); // fBuiltIn
			Utilities.SetBit(ref temp8, false, 1); // fHidden
			Utilities.SetBit(ref temp8, fCustom, 2); // fCustom
			manager.CurrentRecordStream.Write(temp8);

			manager.CurrentRecordStream.Write((byte)style.Category); // iCategory
			ushort builtInData = 0xFFFF;
			if (builtInStyle != null)
				builtInData = (ushort)((builtInStyle.OutlineLevel << 8) | (byte)builtInStyle.Type);

			manager.CurrentRecordStream.Write(builtInData);
			manager.CurrentRecordStream.WriteLPWideString(style.Name);
			manager.CurrentRecordStream.WriteXFProps(style.StyleFormatInternal);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.STYLEEXT; }
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