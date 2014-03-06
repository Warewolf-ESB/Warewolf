using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//internal class AttrIfToken : AttrTokenBase
	//{
	//    private FunctionOperator ifFunction;
	//    private FormulaToken falseConditionJumpToToken;
	//    private List<AttrSkipToken> skipTokens;

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public AttrIfToken( Formula formula )
	//    //    : base( formula ) { }
	//    public AttrIfToken()
	//        : base() { }

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public override FormulaToken Clone( Formula newOwningFormula )
	//    //{
	//    //    return new AttrIfToken( newOwningFormula );
	//    //}
	//    public override FormulaToken GetTokenForClonedFormula()
	//    {
	//        return new AttrIfToken();
	//    }

	//    public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//    {
	//        // We don't need the distance to the next token
	//        stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//    public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        return 4;
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //protected override void SaveAttr( BiffRecordStream stream, bool isForExternalNamedReference )
	//    protected override void SaveAttr(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token
	//        //if ( this.falseConditionJumpToToken == this.ifFunction )
	//        //{
	//        //    stream.Write( (ushort)(
	//        //        this.falseConditionJumpToToken.PositionInRecordStream -
	//        //        this.PositionInRecordStream -
	//        //        this.GetSize( stream, isForExternalNamedReference ) ) );
	//        //}
	//        //else
	//        //{
	//        //    stream.Write( (ushort)(
	//        //        this.falseConditionJumpToToken.ExpressionStartInSavedStream -
	//        //        this.PositionInRecordStream -
	//        //        this.GetSize( stream, isForExternalNamedReference ) ) );
	//        //}
	//        long currentTokenPosition = tokenPositionsInRecordStream[this].PositionInRecordStream;

	//        if (this.falseConditionJumpToToken == this.ifFunction)
	//        {
	//            stream.Write((ushort)(
	//                tokenPositionsInRecordStream[this.falseConditionJumpToToken].PositionInRecordStream -
	//                currentTokenPosition -
	//                this.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream)));
	//        }
	//        else
	//        {
	//            FormulaToken falseConditionFirstTokenInExpression = tokenPositionsInRecordStream[this.falseConditionJumpToToken].FirstTokenInExpression;
	//            long falseConditionExpressionStartInSavedStream = tokenPositionsInRecordStream[falseConditionFirstTokenInExpression].PositionInRecordStream;

	//            stream.Write((ushort)(
	//                falseConditionExpressionStartInSavedStream -
	//                currentTokenPosition -
	//                this.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream)));
	//        }
	//    }

	//    public void SetFalseConditionJumpToToken( FormulaToken jumpToToken )
	//    {
	//        this.falseConditionJumpToToken = jumpToToken;
	//    }

	//    public void SetIfFunction( FunctionOperator ifFunction )
	//    {
	//        Debug.Assert( ifFunction.Function == Function.IF );
	//        this.ifFunction = ifFunction;
	//    }

	//    public List<AttrSkipToken> SkipTokens
	//    {
	//        get
	//        {
	//            if ( this.skipTokens == null )
	//                this.skipTokens = new List<AttrSkipToken>();

	//            return this.skipTokens;
	//        }
	//    }

	//    public override AttrType Type
	//    {
	//        get { return AttrType.If; }
	//    }
	//}

	#endregion // Old Code
	internal class AttrIfToken : AttrTokenBase
	{
		#region Constructor

		private FormulaToken falseConditionJumpToToken;
		private FunctionOperator ifFunction;
		private List<AttrSkipToken> skipTokens;

		#endregion // Constructor

		#region Constructor

		public AttrIfToken()
			: base() { }

		#endregion // Constructor

		#region Base Class Overrides

		#region GetSize

		public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			return 4;
		}

		#endregion // GetSize

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new AttrIfToken();
		}

		#endregion // GetTokenForClonedFormula

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			// We don't need the distance to the next token
			stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
		}

		#endregion // Load

		#region SaveAttr

		protected override void SaveAttr(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			long currentTokenPosition = tokenPositionsInRecordStream[this].PositionInRecordStream;

			if (this.falseConditionJumpToToken == this.ifFunction)
			{
				stream.Write((ushort)(
					tokenPositionsInRecordStream[this.falseConditionJumpToToken].PositionInRecordStream -
					currentTokenPosition -
					this.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream)));
			}
			else
			{
				FormulaToken falseConditionFirstTokenInExpression = tokenPositionsInRecordStream[this.falseConditionJumpToToken].FirstTokenInExpression;
				long falseConditionExpressionStartInSavedStream = tokenPositionsInRecordStream[falseConditionFirstTokenInExpression].PositionInRecordStream;

				stream.Write((ushort)(
					falseConditionExpressionStartInSavedStream -
					currentTokenPosition -
					this.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream)));
			}
		}

		#endregion // SaveAttr

		#region Type

		public override AttrType Type
		{
			get { return AttrType.If; }
		}

		#endregion // Type

		#endregion // Base Class Overrides

		#region SetFalseConditionJumpToToken

		public void SetFalseConditionJumpToToken(FormulaToken jumpToToken)
		{
			this.falseConditionJumpToToken = jumpToToken;
		}

		#endregion // SetFalseConditionJumpToToken

		#region SetIfFunction

		public void SetIfFunction(FunctionOperator ifFunction)
		{
			Debug.Assert(ifFunction.Function == Function.IF);
			this.ifFunction = ifFunction;
		}

		#endregion // SetIfFunction

		#region SkipTokens

		public List<AttrSkipToken> SkipTokens
		{
			get
			{
				if (this.skipTokens == null)
					this.skipTokens = new List<AttrSkipToken>();

				return this.skipTokens;
			}
		}

		#endregion // SkipTokens
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