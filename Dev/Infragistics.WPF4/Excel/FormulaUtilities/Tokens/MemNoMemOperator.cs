using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//internal class MemNoMemOperator : MemOperatorBase
	//{
	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public MemNoMemOperator( Formula formula, TokenClass tokenClass )
	//    //    : base( formula, tokenClass ) { }
	//    public MemNoMemOperator(TokenClass tokenClass)
	//        : base(tokenClass) { }

	//    // MD 10/22/10 - TFS36696
	//    // We can use the default implementation of this now.
	//    //public override FormulaToken Clone(Formula newOwningFormula)
	//    //{
	//    //    MemNoMemOperator memNoMemOperator = new MemNoMemOperator(newOwningFormula, this.TokenClass);
	//    //    memNoMemOperator.SizeOfRefSubExpression = this.SizeOfRefSubExpression;
	//    //    return memNoMemOperator;
	//    //}

	//    public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//    {
	//        stream.ReadUInt32FromBuffer( ref data, ref dataIndex ); // not used
	//        this.SizeOfRefSubExpression = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//    public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        return 7;
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//    public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        stream.Write( this.SizeOfRefSubExpression );
	//    }

	//    public override Token Token
	//    {
	//        get
	//        {
	//            switch ( this.TokenClass )
	//            {
	//                case TokenClass.Array:		return Token.MemNoMemA;
	//                case TokenClass.Reference:	return Token.MemNoMemR;
	//                case TokenClass.Value:		return Token.MemNoMemV;

	//                default:
	//                    Utilities.DebugFail( "Invalid token class" );
	//                    return Token.MemNoMemV;
	//            }
	//        }
	//    }
	//}

	#endregion // Old Code
	internal class MemNoMemOperator : MemOperatorBase
	{
		#region Constructor

		public MemNoMemOperator(TokenClass tokenClass)
			: base(tokenClass) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			stream.ReadUInt32FromBuffer(ref data, ref dataIndex); // not used
			this.SizeOfRefSubExpression = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
		}

		#endregion // Load

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write(this.SizeOfRefSubExpression);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.MemNoMemA;
					case TokenClass.Reference: return Token.MemNoMemR;
					case TokenClass.Value: return Token.MemNoMemV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.MemNoMemV;
				}
			}
		}

		#endregion // Token

		#endregion // Base Class Overrides
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