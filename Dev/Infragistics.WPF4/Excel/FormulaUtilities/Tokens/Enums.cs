using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	#region Token

	internal enum Token
	{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		Exp = 0x01,			// Size = 5



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		Tbl = 0x02,			// Size = 5






		Add = 0x03,			// Size = 1		// Operand Class = value







		Sub = 0x04,			// Size = 1		// Operand Class = value






		Mul = 0x05,			// Size = 1		// Operand Class = value







		Div = 0x06,			// Size = 1		// Operand Class = value







		Power = 0x07,		// Size = 1		// Operand Class = value







		Concat = 0x08,		// Size = 1		// Operand Class = value







		LT = 0x09,			// Size = 1		// Operand Class = value







		LE = 0x0A,			// Size = 1		// Operand Class = value






		EQ = 0x0B,			// Size = 1		// Operand Class = value







		GE = 0x0C,			// Size = 1		// Operand Class = value







		GT = 0x0D,			// Size = 1		// Operand Class = value






		NE = 0x0E,			// Size = 1		// Operand Class = value



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		Isect = 0x0F,		// Size = 1		// Operand Class = reference







		Union = 0x10,		// Size = 1		// Operand Class = reference



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		Range = 0x11,		// Size = 1		// Operand Class = reference






		Uplus = 0x12,		// Size = 1		// Operand Class = value






		Uminus = 0x13,		// Size = 1		// Operand Class = value






		Percent = 0x14,		// Size = 1		// Operand Class = value



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		Paren = 0x15,		// Size = 1







		MissArg = 0x16,		// Size = 1		// Operand Class = value







		Str = 0x17,			// Size = var	// Operand Class = value






		Extended = 0x18,	// Size = var	// Operand Class = reference







		Attr = 0x19,		// Size = var






		Err = 0x1C,			// Size = 2		// Operand Class = value






		Bool = 0x1D,		// Size = 2		// Operand Class = value







		Int = 0x1E,			// Size = 3		// Operand Class = value






		Number = 0x1F,		// Size = 9		// Operand Class = value

		#region Array



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		ArrayR = 0x20,		// Size = 9		// Operand Class = array



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		ArrayV = 0x40,		// Size = 9		// Operand Class = array



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		ArrayA = 0x60,		// Size = 9		// Operand Class = array

		#endregion Array

		#region Func



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		FuncR = 0x21,		// Size = 4		// Operand Class = reference



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		FuncV = 0x41,		// Size = 4		// Operand Class = value



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		FuncA = 0x61,		// Size = 4		// Operand Class = array

		#endregion Func

		#region FuncVar



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		FuncVarR = 0x22,	// Size = 5		// Operand Class = reference



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		FuncVarV = 0x42,	// Size = 5		// Operand Class = value



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		FuncVarA = 0x62,	// Size = 5		// Operand Class = array

		#endregion FuncVar

		#region Name



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		NameR = 0x23,		// Size = 5		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		NameV = 0x43,		// Size = 5		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		NameA = 0x63,		// Size = 5		// Operand Class = reference

		#endregion Name

		#region Ref






		RefR = 0x24,		// Size = 5		// Operand Class = reference






		RefV = 0x44,		// Size = 5		// Operand Class = reference






		RefA = 0x64,		// Size = 5		// Operand Class = reference

		#endregion Ref

		#region Area






		AreaR = 0x25,		// Size = 9		// Operand Class = reference






		AreaV = 0x45,		// Size = 9		// Operand Class = reference






		AreaA = 0x65,		// Size = 9		// Operand Class = reference

		#endregion Area

		#region MemArea



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		MemAreaR = 0x26,	// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		MemAreaV = 0x46,	// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		MemAreaA = 0x66,	// Size = 7		// Operand Class = reference

		#endregion MemArea

		#region MemErr



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		MemErrR = 0x27,		// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		MemErrV = 0x47,		// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		MemErrA = 0x67,		// Size = 7		// Operand Class = reference

		#endregion MemErr

		#region MemNoMem



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		MemNoMemR = 0x28,	// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		MemNoMemV = 0x48,	// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		MemNoMemA = 0x68,	// Size = 7		// Operand Class = reference

		#endregion MyRegion

		#region MemFunc



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		MemFuncR = 0x29,	// Size = 3		// Operand Class = reference



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		MemFuncV = 0x49,	// Size = 3		// Operand Class = reference



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		MemFuncA = 0x69,	// Size = 3		// Operand Class = reference

		#endregion MemFunc

		#region RefErr







		RefErrR = 0x2A,		// Size = 5		// Operand Class = reference







		RefErrV = 0x4A,		// Size = 5		// Operand Class = reference







		RefErrA = 0x6A,		// Size = 5		// Operand Class = reference

		#endregion RefErr

		#region AreaErr







		AreaErrR = 0x2B,	// Size = 9		// Operand Class = reference







		AreaErrV = 0x4B,	// Size = 9		// Operand Class = reference







		AreaErrA = 0x6B,	// Size = 9		// Operand Class = reference

		#endregion AreaErr

		#region RefN



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		RefNR = 0x2C,		// Size = 5		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		RefNV = 0x4C,		// Size = 5		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		RefNA = 0x6C,		// Size = 5		// Operand Class = reference

		#endregion RefN

		#region AreaN



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		AreaNR = 0x2D,		// Size = 9		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		AreaNV = 0x4D,		// Size = 9		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		AreaNA = 0x6D,		// Size = 9		// Operand Class = reference

		#endregion AreaN

		#region NameX



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		NameXR = 0x39,		// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		NameXV = 0x59,		// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		NameXA = 0x79,		// Size = 7		// Operand Class = reference

		#endregion NameX

		#region Ref3d



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		Ref3dR = 0x3A,		// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		Ref3dV = 0x5A,		// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		Ref3dA = 0x7A,		// Size = 7		// Operand Class = reference

		#endregion Ref3d

		#region Area3d



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		Area3DR = 0x3B,		// Size = 11	// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		Area3DV = 0x5B,		// Size = 11	// Operand Class = reference



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		Area3DA = 0x7B,		// Size = 11	// Operand Class = reference

		#endregion Area3d

		#region RefErr3d



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		RefErr3dR = 0x3C,	// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		RefErr3dV = 0x5C,	// Size = 7		// Operand Class = reference



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		RefErr3dA = 0x7C,	// Size = 7		// Operand Class = reference

		#endregion RefErr3d

		#region AreaErr3d



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		AreaErr3dR = 0x3D,	// Size = 11	// Operand Class = reference



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		AreaErr3dV = 0x5D,	// Size = 11	// Operand Class = reference



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		AreaErr3dA = 0x7D,	// Size = 11	// Operand Class = reference

		#endregion AreaErr3d

		// MD 12/7/11 - 12.1 - Table Support
		// New tokens which aren't valid in 2003 will have 0x1000 set;
		Excel2007Token = 0x1000,

		StructuredTableReferenceR = Excel2007Token | 0x20,
		StructuredTableReferenceV = Excel2007Token | 0x40,
		StructuredTableReferenceA = Excel2007Token | 0x60,
	}

	#endregion Token

	#region TokenClass

	internal enum TokenClass : byte
	{
		//None = 0x00,
		Reference = 0x20,
		Value = 0x40,
		Array = 0x60,
		Control = 0xFF,
	}

	#endregion TokenClass
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