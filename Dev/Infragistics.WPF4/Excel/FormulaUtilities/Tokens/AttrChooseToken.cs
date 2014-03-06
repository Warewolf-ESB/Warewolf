using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//internal class AttrChooseToken : AttrTokenBase
	//{
	//    private FunctionOperator chooseFunction;
	//    private List<AttrSkipToken> skipTokens;
	//    private List<FormulaToken> chooseOptions;

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public AttrChooseToken( Formula formula )
	//    //    : base( formula ) { }
	//    public AttrChooseToken()
	//        : base() { }

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public override FormulaToken Clone( Formula newOwningFormula )
	//    //{
	//    //    return new AttrChooseToken( newOwningFormula );
	//    //}
	//    public override FormulaToken GetTokenForClonedFormula()
	//    {
	//        return new AttrChooseToken();
	//    }

	//    public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//    {
	//        int numberOfChooseOptions = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );

	//        for ( int i = 0; i < numberOfChooseOptions; i++ )
	//            stream.ReadUInt16FromBuffer( ref data, ref dataIndex ); // distance from jump table to param i

	//        stream.ReadUInt16FromBuffer( ref data, ref dataIndex ); // distance from jump table to choose function
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//    public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        return (byte)( 6 + ( 2 * this.ChooseOptions.Count ) );
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //protected override void SaveAttr( BiffRecordStream stream, bool isForExternalNamedReference )
	//    protected override void SaveAttr(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        stream.Write( (ushort)this.ChooseOptions.Count );

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token
	//        //foreach ( FormulaToken chooseOption in this.ChooseOptions )
	//        //{
	//        //    stream.Write( (ushort)( chooseOption.ExpressionStartInSavedStream - this.PositionInRecordStream - 4 ) );
	//        //}
	//        //
	//        //stream.Write( (ushort)( this.chooseFunction.PositionInRecordStream - this.PositionInRecordStream - 4 ) );
	//        long currentTokenRecord = tokenPositionsInRecordStream[this].PositionInRecordStream;

	//        foreach (FormulaToken chooseOption in this.ChooseOptions)
	//        {
	//            FormulaToken chooseOptionFirstTokenInExpression = tokenPositionsInRecordStream[chooseOption].FirstTokenInExpression;
	//            long chooseOptionExpressionStartInSavedStream = tokenPositionsInRecordStream[chooseOptionFirstTokenInExpression].PositionInRecordStream;
	//            stream.Write((ushort)(chooseOptionExpressionStartInSavedStream - currentTokenRecord - 4));
	//        }

	//        stream.Write((ushort)(tokenPositionsInRecordStream[this.chooseFunction].PositionInRecordStream - currentTokenRecord - 4));
	//    }

	//    public void SetChooseFunction( FunctionOperator chooseFunction )
	//    {
	//        Debug.Assert( chooseFunction.Function == Function.CHOOSE );
	//        this.chooseFunction = chooseFunction;
	//    }

	//    public List<FormulaToken> ChooseOptions
	//    {
	//        get
	//        {
	//            if ( this.chooseOptions == null )
	//                this.chooseOptions = new List<FormulaToken>();

	//            return this.chooseOptions;
	//        }
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
	//        get { return AttrType.Choose; }
	//    }
	//}

	#endregion // Old Code
	internal class AttrChooseToken : AttrTokenBase
	{
		#region Member Variables

		private FunctionOperator chooseFunction;
		private List<FormulaToken> chooseOptions;
		private List<AttrSkipToken> skipTokens;

		#endregion // Member Variables

		#region Constructor

		public AttrChooseToken()
			: base() { }

		#endregion // Constructor

		#region Base Class Overrides

		#region GetSize

		public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			return (byte)(6 + (2 * this.ChooseOptions.Count));
		}

		#endregion // GetSize

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new AttrChooseToken();
		}

		#endregion // GetTokenForClonedFormula

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			int numberOfChooseOptions = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);

			for (int i = 0; i < numberOfChooseOptions; i++)
				stream.ReadUInt16FromBuffer(ref data, ref dataIndex); // distance from jump table to param i

			stream.ReadUInt16FromBuffer(ref data, ref dataIndex); // distance from jump table to choose function
		}

		#endregion // Load

		#region SaveAttr

		protected override void SaveAttr(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write((ushort)this.ChooseOptions.Count);

			long currentTokenRecord = tokenPositionsInRecordStream[this].PositionInRecordStream;

			foreach (FormulaToken chooseOption in this.ChooseOptions)
			{
				FormulaToken chooseOptionFirstTokenInExpression = tokenPositionsInRecordStream[chooseOption].FirstTokenInExpression;
				long chooseOptionExpressionStartInSavedStream = tokenPositionsInRecordStream[chooseOptionFirstTokenInExpression].PositionInRecordStream;
				stream.Write((ushort)(chooseOptionExpressionStartInSavedStream - currentTokenRecord - 4));
			}

			stream.Write((ushort)(tokenPositionsInRecordStream[this.chooseFunction].PositionInRecordStream - currentTokenRecord - 4));
		}

		#endregion // SaveAttr

		#region Type

		public override AttrType Type
		{
			get { return AttrType.Choose; }
		}

		#endregion // Type

		#endregion // Base Class Overrides

		#region SetChooseFunction

		public void SetChooseFunction(FunctionOperator chooseFunction)
		{
			Debug.Assert(chooseFunction.Function == Function.CHOOSE);
			this.chooseFunction = chooseFunction;
		}

		#endregion // SetChooseFunction

		#region ChooseOptions

		public List<FormulaToken> ChooseOptions
		{
			get
			{
				if (this.chooseOptions == null)
					this.chooseOptions = new List<FormulaToken>();

				return this.chooseOptions;
			}
		}

		#endregion // ChooseOptions

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