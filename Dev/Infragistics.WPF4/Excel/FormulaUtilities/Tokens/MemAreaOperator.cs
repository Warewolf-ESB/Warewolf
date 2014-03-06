using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;




using System.Drawing;


namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//internal class MemAreaOperator : MemOperatorBase
	//{
	//    private CellAddressRange[] ranges;

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public MemAreaOperator( Formula formula, TokenClass tokenClass )
	//    //    : base( formula, tokenClass ) { }
	//    public MemAreaOperator(TokenClass tokenClass)
	//        : base(tokenClass) { }

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public override FormulaToken Clone( Formula newOwningFormula )
	//    //{
	//    //    MemAreaOperator memAreaOperator = new MemAreaOperator( newOwningFormula, this.TokenClass );
	//    public override FormulaToken GetTokenForClonedFormula()
	//    {
	//        MemAreaOperator memAreaOperator = new MemAreaOperator(this.TokenClass);

	//        memAreaOperator.ranges = new CellAddressRange[ this.ranges.Length ];

	//        for ( int i = 0; i < this.ranges.Length; i++ )
	//        {
	//            // MD 10/22/10 - TFS36696
	//            // The CellAddressRange is now immutable.
	//            //memAreaOperator.ranges[ i ] = this.ranges[ i ].Clone();
	//            memAreaOperator.ranges[i] = this.ranges[i];
	//        }

	//        memAreaOperator.SizeOfRefSubExpression = this.SizeOfRefSubExpression;

	//        return memAreaOperator;
	//    }

	//    public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//    {
	//        stream.ReadUInt32FromBuffer( ref data, ref dataIndex ); // not used
	//        this.SizeOfRefSubExpression = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//    }

	//    public override void LoadAdditionalData( BiffRecordStream stream, ref byte[] data, ref int dataIndex )
	//    {
	//        ushort numberOfRanges = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );

	//        this.ranges = new CellAddressRange[ numberOfRanges ];

	//        for ( int i = 0; i < numberOfRanges; i++ )
	//            this.ranges[ i ] = stream.ReadFormulaCellAddressRangeFromBuffer( ref data, ref dataIndex );
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//    public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        return 7;
	//    }

	//    // MD 12/22/11 - 12.1 - Table Support
	//    public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//        WorksheetRow sourceRow, short sourceColumnIndex,
	//        WorksheetRow comparisonRow, short comparisonColumnIndex)
	//    {
	//        if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//            return false;

	//        MemAreaOperator comparisonMemAreaOperator = (MemAreaOperator)comparisonToken;
	//        if (this.ranges.Length != comparisonMemAreaOperator.ranges.Length)
	//            return false;

	//        for (int i = 0; i < this.ranges.Length; i++)
	//        {
	//            if (this.ranges[i].Equals(comparisonMemAreaOperator.ranges[i]) == false)
	//                return false;
	//        }

	//        return true;
	//    }

	//    public override void OffsetReferences( Point offset )
	//    {
	//        // MD 10/22/10 - TFS36696
	//        // This CellAddressRange is now immutable.
	//        //foreach ( CellAddressRange range in this.ranges )
	//        //    range.Offset( offset );
	//        for (int i = 0; i < this.ranges.Length; i++)
	//            this.ranges[i] = this.ranges[i].Offset(offset);
	//    }

	//    // MD 10/22/10 - TFS36696
	//    // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//    //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//    public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//    {
	//        stream.Write( (uint)0 );
	//        stream.Write( this.SizeOfRefSubExpression );
	//    }

	//    public override void SaveAdditionalData( BiffRecordStream stream )
	//    {
	//        stream.Write( (ushort)this.ranges.Length );

	//        for ( int i = 0; i < this.ranges.Length; i++ )
	//            stream.Write( this.ranges[ i ] );
	//    }

	//    public override Token Token
	//    {
	//        get
	//        {
	//            switch ( this.TokenClass )
	//            {
	//                case TokenClass.Array:		return Token.MemAreaA;
	//                case TokenClass.Reference:	return Token.MemAreaR;
	//                case TokenClass.Value:		return Token.MemAreaV;

	//                default:
	//                    Utilities.DebugFail( "Invalid token class" );
	//                    return Token.MemAreaV;
	//            }
	//        }
	//    }
	//}

	#endregion // Old Code
	internal class MemAreaOperator : MemOperatorBase
	{
		#region Member Variables

		private CellAddressRange[] ranges;

		#endregion // Member Variables

		#region Constructor

		public MemAreaOperator(TokenClass tokenClass)
			: base(tokenClass) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			MemAreaOperator memAreaOperator = new MemAreaOperator(this.TokenClass);

			memAreaOperator.ranges = new CellAddressRange[this.ranges.Length];

			for (int i = 0; i < this.ranges.Length; i++)
				memAreaOperator.ranges[i] = this.ranges[i];

			memAreaOperator.SizeOfRefSubExpression = this.SizeOfRefSubExpression;

			return memAreaOperator;
		}

		#endregion // GetTokenForClonedFormula

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			MemAreaOperator comparisonMemAreaOperator = (MemAreaOperator)comparisonToken;
			if (this.ranges.Length != comparisonMemAreaOperator.ranges.Length)
				return false;

			for (int i = 0; i < this.ranges.Length; i++)
			{
				if (this.ranges[i].Equals(comparisonMemAreaOperator.ranges[i]) == false)
					return false;
			}

			return true;
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			stream.ReadUInt32FromBuffer(ref data, ref dataIndex); // not used
			this.SizeOfRefSubExpression = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
		}

		#endregion // Load

		#region LoadAdditionalData

		public override void LoadAdditionalData(BiffRecordStream stream, ref byte[] data, ref int dataIndex)
		{
			ushort numberOfRanges = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);

			this.ranges = new CellAddressRange[numberOfRanges];

			for (int i = 0; i < numberOfRanges; i++)
				this.ranges[i] = stream.ReadFormulaCellAddressRangeFromBuffer(ref data, ref dataIndex);
		}

		#endregion // LoadAdditionalData

		#region OffsetReferences

		public override void OffsetReferences(FormulaContext context, Point offset)
		{
			for (int i = 0; i < this.ranges.Length; i++)
				this.ranges[i] = this.ranges[i].Offset(context, offset);
		}

		#endregion // OffsetReferences

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write((uint)0);
			stream.Write(this.SizeOfRefSubExpression);
		}

		#endregion // Save

		#region SaveAdditionalData

		public override void SaveAdditionalData(BiffRecordStream stream)
		{
			stream.Write((ushort)this.ranges.Length);

			for (int i = 0; i < this.ranges.Length; i++)
				stream.Write(this.ranges[i]);
		}

		#endregion // SaveAdditionalData

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.MemAreaA;
					case TokenClass.Reference: return Token.MemAreaR;
					case TokenClass.Value: return Token.MemAreaV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.MemAreaV;
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