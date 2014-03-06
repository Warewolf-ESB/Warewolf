using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace Infragistics.Documents.Excel
{
	// MD 11/28/11 - TFS96486
	internal partial class WorkbookFormatCollection
	{
		// MD 4/6/12 - TFS101506
		//private void AddCultureSpecificFormats()
		private void AddCultureSpecificFormats(CultureInfo culture)
		{
			// MD 4/6/12 - TFS101506
			//switch (CultureInfo.CurrentCulture.Name)
			switch (culture.Name)
			{
				#region CHT

				case "zh-TW": // CHT, Chinese - Taiwan
					{
						this.AddFormat(27, "[$-404]e/m/d", true);
						this.AddFormat(28, "[$-404]e\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(29, "[$-404]e\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(30, "m/d/yy", true);
						this.AddFormat(31, "yyyy\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(32, "hh\"時\"mm\"分\"", true);
						this.AddFormat(33, "hh\"時\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(34, "上午/下午hh\"時\"mm\"分\"", true);
						this.AddFormat(35, "上午/下午hh\"時\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(36, "[$-404]e/m/d", true);
						this.AddFormat(50, "[$-404]e/m/d", true);
						this.AddFormat(51, "[$-404]e\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(52, "上午/下午hh\"時\"mm\"分\"", true);
						this.AddFormat(53, "上午/下午hh\"時\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(54, "[$-404]e\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(55, "上午/下午hh\"時\"mm\"分\"", true);
						this.AddFormat(56, "上午/下午hh\"時\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(57, "[$-404]e/m/d", true);
						this.AddFormat(58, "[$-404]e\"年\"m\"月\"d\"日\"", true);
					}
					break;

				#endregion  // CHT

				#region CHS

				case "zh-CN": // CHS, Chinese - China
					{
						this.AddFormat(27, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(28, "m\"月\"d\"日\"", true);
						this.AddFormat(29, "m\"月\"d\"日\"", true);
						this.AddFormat(30, "m-d-yy", true);
						this.AddFormat(31, "yyyy\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(32, "h\"时\"mm\"分\"", true);
						this.AddFormat(33, "h\"时\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(34, "上午/下午h\"时\"mm\"分\"", true);
						this.AddFormat(35, "上午/下午h\"时\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(36, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(50, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(51, "m\"月\"d\"日\"", true);
						this.AddFormat(52, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(53, "m\"月\"d\"日\"", true);
						this.AddFormat(54, "m\"月\"d\"日\"", true);
						this.AddFormat(55, "上午/下午h\"时\"mm\"分\"", true);
						this.AddFormat(56, "上午/下午h\"时\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(57, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(58, "m\"月\"d\"日\"", true);
					}
					break;

				#endregion  // CHS

				#region JPN

				case "ja-JP": // JPN, Japanese - Japan
					{
						this.AddFormat(27, "[$-411]ge.m.d", true);
						this.AddFormat(28, "[$-411]ggge\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(29, "[$-411]ggge\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(30, "m/d/yy", true);
						this.AddFormat(31, "yyyy\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(32, "h\"時\"mm\"分\"", true);
						this.AddFormat(33, "h\"時\"mm\"分\"ss\"秒\"", true);
						this.AddFormat(34, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(35, "m\"月\"d\"日\"", true);
						this.AddFormat(36, "[$-411]ge.m.d", true);
						this.AddFormat(50, "[$-411]ge.m.d", true);
						this.AddFormat(51, "[$-411]ggge\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(52, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(53, "m\"月\"d\"日\"", true);
						this.AddFormat(54, "[$-411]ggge\"年\"m\"月\"d\"日\"", true);
						this.AddFormat(55, "yyyy\"年\"m\"月\"", true);
						this.AddFormat(56, "m\"月\"d\"日\"", true);
						this.AddFormat(57, "[$-411]ge.m.d", true);
						this.AddFormat(58, "[$-411]ggge\"年\"m\"月\"d\"日\"", true);
					}
					break;

				#endregion  // JPN

				#region KOR

				case "ko-KR": // KOR, Korean - Korea
					{
						this.AddFormat(27, "yyyy\"年\" mm\"月\" dd\"日\"", true);
						this.AddFormat(28, "mm-dd", true);
						this.AddFormat(29, "mm-dd", true);
						this.AddFormat(30, "mm-dd-yy", true);
						this.AddFormat(31, "yyyy\"년\" mm\"월\" dd\"일\"", true);
						this.AddFormat(32, "h\"시\" mm\"분\"", true);
						this.AddFormat(33, "h\"시\" mm\"분\" ss\"초\"", true);
						this.AddFormat(34, "yyyy-mm-dd", true);
						this.AddFormat(35, "yyyy-mm-dd", true);
						this.AddFormat(36, "yyyy\"年\" mm\"月\" dd\"日\"", true);
						this.AddFormat(50, "yyyy\"年\" mm\"月\" dd\"日\"", true);
						this.AddFormat(51, "mm-dd", true);
						this.AddFormat(52, "yyyy-mm-dd", true);
						this.AddFormat(53, "yyyy-mm-dd", true);
						this.AddFormat(54, "mm-dd", true);
						this.AddFormat(55, "yyyy-mm-dd", true);
						this.AddFormat(56, "yyyy-mm-dd", true);
						this.AddFormat(57, "yyyy\"年\" mm\"月\" dd\"日\"", true);
						this.AddFormat(58, "mm-dd", true);
					}
					break;

				#endregion  // KOR

				#region THA

				case "th-TH": // THA, Thai - Thailand
					{
						// MD 5/10/12 - TFS104961
						// Some of these strings are based on the culture.
						#region Old Code

						//this.AddFormat(59, "t0", true);
						//this.AddFormat(60, "t0.00", true);
						//this.AddFormat(61, "t#,##0", true);
						//this.AddFormat(62, "t#,##0.00", true);
						//this.AddFormat(67, "t0%", true);
						//this.AddFormat(68, "t0.00%", true);
						//this.AddFormat(69, "t# ?/?", true);
						//this.AddFormat(70, "t# ??/??", true);
						//this.AddFormat(71, "ว/ด/ปปปป", true);
						//this.AddFormat(72, "ว-ดดด-ปป", true);
						//this.AddFormat(73, "ว-ดดด", true);
						//this.AddFormat(74, "ดดด-ปป", true);
						//this.AddFormat(75, "ช:นน", true);
						//this.AddFormat(76, "ช:นน:ทท", true);
						//this.AddFormat(77, "ว/ด/ปปปป ช:นน", true);
						//this.AddFormat(78, "นน:ทท", true);
						//this.AddFormat(79, "[ช]:นน:ทท", true);
						//this.AddFormat(80, "นน:ทท.0", true);
						//this.AddFormat(81, "d/m/bb", true);

						#endregion // Old Code
						this.AddFormat(59, "t0", true);
						this.AddFormat(60, string.Format("t0{0}00", this.decimalSeparatorAtInitialization), true);
						this.AddFormat(61, string.Format("t#{0}##0", this.groupSeparatorAtInitialization), true);
						this.AddFormat(62, string.Format("t#{0}##0{1}00", this.groupSeparatorAtInitialization, this.decimalSeparatorAtInitialization), true);
						this.AddFormat(67, "t0%", true);
						this.AddFormat(68, string.Format("t0{0}00%", this.decimalSeparatorAtInitialization), true);
						this.AddFormat(69, "t# ?/?", true);
						this.AddFormat(70, "t# ??/??", true);
						this.AddFormat(71, "ว/ด/ปปปป", true);
						this.AddFormat(72, "ว-ดดด-ปป", true);
						this.AddFormat(73, "ว-ดดด", true);
						this.AddFormat(74, "ดดด-ปป", true);
						this.AddFormat(75, "ช:นน", true);
						this.AddFormat(76, "ช:นน:ทท", true);
						this.AddFormat(77, "ว/ด/ปปปป ช:นน", true);
						this.AddFormat(78, "นน:ทท", true);
						this.AddFormat(79, "[ช]:นน:ทท", true);
						this.AddFormat(80, string.Format("นน:ทท{0}0", this.decimalSeparatorAtInitialization), true);
						this.AddFormat(81, "d/m/bb", true);
					}
					break;

				#endregion  // THA
			}
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