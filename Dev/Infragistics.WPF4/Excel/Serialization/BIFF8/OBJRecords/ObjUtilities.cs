using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	internal static class ObjUtilities
	{
		// MD 2/19/12 - 12.1 - Table Support
		// Moved these methods to the Biff8RecordStream
		#region Moved

		//// http://msdn.microsoft.com/en-us/library/dd922754(v=office.12).aspx
		//public static string LoadXLUnicodeString(BiffRecordStream stream)
		//{
		//    ushort cch = stream.ReadUInt16();
		//    return ObjUtilities.LoadXLUnicodeStringNoCch(stream, cch);
		//}

		//// http://msdn.microsoft.com/en-us/library/dd910585(v=office.12).aspx
		//public static string LoadXLUnicodeStringNoCch(BiffRecordStream stream, ushort cch)
		//{
		//    byte temp = (byte)stream.ReadByte();
		//    byte fHighByte = (byte)(temp & 0x01);

		//    byte reserved = (byte)(temp >> 1);
		//    Debug.Assert(reserved == 0, "The reserved field is incorrect.");

		//    byte[] data;
		//    if (fHighByte == 0)
		//        data = new byte[cch * 2];
		//    else
		//        data = new byte[cch];

		//    int bytePosition = 0;
		//    for (int i = 0; i < cch; i++)
		//    {
		//        data[bytePosition++] = (byte)stream.ReadByte();

		//        if (fHighByte == 0)
		//            data[bytePosition++] = 0;
		//    }

		//    return Encoding.Unicode.GetString(data);
		//}

		#endregion // Moved

		public static bool ReadAndVerifyFt(BiffRecordStream stream, OBJRecordType expectedFt)
		{
			return ObjUtilities.ReadAndVerifyFt(stream, expectedFt, true);
		}

		public static bool ReadAndVerifyFt(BiffRecordStream stream, OBJRecordType expectedFt, bool isRequired)
		{
			OBJRecordType ft = (OBJRecordType)stream.ReadUInt16();
			if (ft != expectedFt)
			{
				Debug.Assert(isRequired == false, "The ft field is incorrect.");
				stream.Seek(-2, SeekOrigin.Current);
				return false;
			}

			return true;
		}

		// MD 2/19/12 - 12.1 - Table Support
		// Moved these methods to the Biff8RecordStream
		#region Moved

		//// http://msdn.microsoft.com/en-us/library/dd922754(v=office.12).aspx
		//public static void SaveXLUnicodeString(Biff8RecordStream stream, string value)
		//{
		//    stream.Write((ushort)value.Length);
		//    ObjUtilities.SaveXLUnicodeStringNoCch(stream, value);
		//}

		//// http://msdn.microsoft.com/en-us/library/dd910585(v=office.12).aspx
		//public static void SaveXLUnicodeStringNoCch(Biff8RecordStream stream, string value)
		//{
		//    byte[] bytes = Encoding.Unicode.GetBytes(value);

		//    bool fHighByte = false;

		//    byte[] lowBytes = new byte[bytes.Length / 2];
		//    for (int i = 1; i < bytes.Length; i += 2)
		//    {
		//        if (bytes[i] != 0)
		//        {
		//            fHighByte = true;
		//            break;
		//        }

		//        lowBytes[i / 2] = bytes[i - 1];
		//    }

		//    byte[] rgb = fHighByte ? bytes : lowBytes;

		//    stream.WriteByte(Convert.ToByte(fHighByte));
		//    stream.Write(rgb);
		//}

		#endregion // Moved
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