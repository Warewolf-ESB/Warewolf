using System;
using System.Collections.Generic;
using System.Text;




namespace Infragistics.Documents.Excel.StructuredStorage.FileTypes

{
	internal enum DocumentSummaryPropertyType
	{
		Category =					0x00000002,
		PresentationTarget =		0x00000003,
		Bytes =						0x00000004,
		Lines =						0x00000005,
		Paragraphs =				0x00000006,
		Slides =					0x00000007,
		Notes =						0x00000008,
		HiddenSlides =				0x00000009,
		MMClips =					0x0000000A,
		ScaleCrop =					0x0000000B,
		HeadingPairs =				0x0000000C,
		TitlesOfParts =				0x0000000D,
		Manager =					0x0000000E,
		Company =					0x0000000F,
		LinksUpToData =				0x00000010,
		Status =					0x0000001B,
	}

	internal enum SummaryPropertyType
	{
		Title =						0x00000002,
		Subject =					0x00000003,
		Author =					0x00000004,
		Keywords =					0x00000005,
		Comments =					0x00000006,
		Template =					0x00000007,
		LastSavedBy =				0x00000008,
		RevisionNumber =			0x00000009,
		TotalEditingTime =			0x0000000A,
		LastPrinted =				0x0000000B,
		CreatedDateTime =			0x0000000C,
		LastSavedDateTime =			0x0000000D,
		NumberOfPages =				0x0000000E,
		NumberOfWords =				0x0000000F,
		NumberOfCharacters =		0x00000010,
		Thumbnail =					0x00000011,
		NameOfCreatingApplication = 0x00000012,
		Security =					0x00000013
	}

	internal enum UserDefinedPropertyType
	{
		Dictionary =			0x00000000,

		// MD 1/30/08 - BR30189
		// Not sure if these are the correct definitions, but I got them from here:
		// http://publib.boulder.ibm.com/infocenter/wmbhelp/v6r0m0/index.jsp?topic=/com.ibm.etools.mft.doc/as08560_.htm
		// ftp://ftp.software.ibm.com/software/integration/wbibrokers/docs/V5.0/messagebroker_User_defined_Extensions.pdf
		SelfDefStructure =		0x01000002,
		StructureInstance =		0x01000004,

		Locale =				-2147483648, // 0x80000000,
	}

	internal enum VariantType
	{
		Empty = 0,
		Null = 1,
		I2 = 2,
		I4 = 3,
		R4 = 4,
		R8 = 5,
		CY = 6,
		Date = 7,
		Bstr = 8,
		Dispatch = 9,
		Error = 10,
		Bool = 11,
		Variant = 12,
		Unknown = 13,
		Decimal = 14,
		I1 = 16,
		UI1 = 17,
		UI2 = 18,
		UI4 = 19,
		I8 = 20,
		UI8 = 21,
		INT = 22,
		UINT = 23,
		VOID = 24,
		Hresult = 25,
		PTR = 26,
		SafeArray = 27,
		CARRAY = 28,
		UserDefined = 29,
		LPSTR = 30,
		LPWSTR = 31,
		FileTime = 64,
		BLOB,
		Stream,
		Storage,
		StreamedObject,
		BLOBObject,
		CF,
		CLSID,
		Vector = 0x1000,
		Array = 0x2000,
		ByRef = 0x4000,
		Reserved = 0x8000,
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