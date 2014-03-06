using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//internal class AttrSkipToken : AttrTokenBase
	//{
	//    private FormulaToken skipAfterToken;

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public AttrSkipToken( Formula formula )
	//    //    : base( formula ) { }
	//    public AttrSkipToken()
	//        : base() { }

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public override FormulaToken Clone( Formula newOwningFormula )
	//    //{
	//    //    return new AttrSkipToken( newOwningFormula );
	//    //}
	//    public override FormulaToken GetTokenForClonedFormula()
	//    {
	//        return new AttrSkipToken();
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
	//        //stream.Write( (ushort)(
	//        //    this.skipAfterToken.PositionInRecordStream +
	//        //    this.skipAfterToken.GetSize( stream, isForExternalNamedReference ) -
	//        //    this.PositionInRecordStream -
	//        //    this.GetSize( stream, isForExternalNamedReference ) - 1 ) );
	//        stream.Write((ushort)(
	//            tokenPositionsInRecordStream[this.skipAfterToken].PositionInRecordStream +
	//            this.skipAfterToken.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream) -
	//            tokenPositionsInRecordStream[this].PositionInRecordStream -
	//            this.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream) - 1));
	//    }

	//    public void SetSkipAfterToken( FormulaToken token )
	//    {
	//        this.skipAfterToken = token;
	//    }

	//    public override AttrType Type
	//    {
	//        get { return AttrType.Skip; }
	//    }
	//}

	#endregion // Old Code
	internal class AttrSkipToken : AttrTokenBase
	{
		#region Member Variables

		private FormulaToken skipAfterToken;

		#endregion // Member Variables

		#region Constructor

		public AttrSkipToken()
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
			return new AttrSkipToken();
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
			stream.Write((ushort)(
				tokenPositionsInRecordStream[this.skipAfterToken].PositionInRecordStream +
				this.skipAfterToken.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream) -
				tokenPositionsInRecordStream[this].PositionInRecordStream -
				this.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream) - 1));
		}

		#endregion // SaveAttr

		#region Type

		public override AttrType Type
		{
			get { return AttrType.Skip; }
		}

		#endregion // Type

		#endregion // Base Class Overrides

		#region SetSkipAfterToken

		public void SetSkipAfterToken(FormulaToken token)
		{
			this.skipAfterToken = token;
		}

		#endregion // SetSkipAfterToken
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