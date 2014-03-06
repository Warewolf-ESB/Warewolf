using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization
{
	// MD 2/1/11 - Data Validation support
	internal enum DataValidationType : byte
	{
		AnyValue = 0,
		WholeNumber = 1,
		Decimal = 2,
		List = 3,
		Date = 4,
		Time = 5,
		TextLength = 6,
		Formula = 7,
	}

	// MD 2/1/11 - Data Validation support
	internal enum DataValidationOperatorType : byte
	{
		Between = 0,
		NotBetween = 1,
		Equal = 2,
		NotEqual = 3,
		GreaterThan = 4,
		LessThan = 5,
		GreaterThanOrEqual = 6,
		LessThanOrEqual = 7,
	}

	// MD 2/1/11 - Data Validation support
	internal enum ErrorAlertStyle : byte
	{
		Stop = 0,
		Warning = 1,
		Infromation = 2,
	}

	// MD 1/18/12 - 12.1 - Cell Format Updates
	// This is no longer needed.
	//// MD 11/10/11 - TFS85193
	//internal enum FontResolverType
	//{
	//    Normal,
	//    ShapeWithText,
	//}

	internal enum SheetType : byte
	{
		Worksheet = 0,
		MacroSheet = 1,
		Chart = 2,
		VBModule = 6
	}

	internal enum SubstreamType : ushort
	{
		WorkbookGlobals = 0x0005,
		VisualBasicModule = 0x0006,
		Worksheet = 0x0010,
		Chart = 0x0020,
		MacroSheet = 0x0040,
		WorkspaceFile = 0x0100
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