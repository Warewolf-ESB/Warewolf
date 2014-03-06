using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Error value operand. Indicates an error value constant.
	//    /// </summary>
	//#endif
	//    // MD 8/18/08 - Excel formula solving
	//    //internal class ErrToken : FormulaToken
	//    internal class ErrToken : OperandToken
	//    {
	//        #region Member Variables

	//        private ErrorValue value; 

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public ErrToken( Formula formula )
	//        //    : base( formula, TokenClass.Value ) { }
	//        //
	//        //public ErrToken( Formula formula, ErrorValue value )
	//        //    : this( formula )
	//        public ErrToken()
	//            : base(TokenClass.Value) { }

	//        public ErrToken(ErrorValue value)
	//            : this()
	//        {
	//            this.value = value;

	//            if ( this.value == ErrorValue.InvalidCellReference )
	//                this.TokenClass = TokenClass.Reference;
	//        } 

	//        #endregion Constructor

	//        #region Base Class Overrides

	//        #region Clone

	//        // MD 10/22/10 - TFS36696
	//        // We can use the default implementation of this now.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new ErrToken( newOwningFormula, this.value );
	//        //}

	//        #endregion Clone

	//        // MD 8/18/08 - Excel formula solving
	//        #region GetCalcValue

	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override object GetCalcValue( Workbook workbook, IWorksheetCell formulaOwner )
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            if ( this.value == ErrorValue.Circularity )
	//                return 0;

	//            return this.value.ToCalcErrorValue();
	//        }

	//        #endregion GetCalcValue

	//        #region Load

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            this.value = ErrorValue.FromValue( stream.ReadByteFromBuffer( ref data, ref dataIndex ) );
	//        }

	//        #endregion Load

	//        #region GetSize

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 2;
	//        }

	//        #endregion GetSize

	//        // MD 12/22/11 - 12.1 - Table Support
	//        #region IsEquivalentTo

	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            ErrToken comparisonErrToken = (ErrToken)comparisonToken;
	//            return value == comparisonErrToken.value;
	//        }

	//        #endregion // IsEquivalentTo

	//        #region Save

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write( this.value.Value );
	//        }

	//        #endregion Save

	//        #region Token

	//        public override Token Token
	//        {
	//            get { return Token.Err; }
	//        }

	//        #endregion Token

	//        // MBS 9/10/08 - Excel 2007
	//        #region TokenClass

	//        public override TokenClass TokenClass
	//        {
	//            get
	//            {
	//                if (this.Value == ErrorValue.InvalidCellReference)
	//                    return TokenClass.Reference;

	//                return base.TokenClass;
	//            }
	//        }
	//        #endregion //TokenClass

	//        #region ToString

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            return this.value.ToString();
	//        }

	//        #endregion ToString 

	//        #endregion Base Class Overrides

	//        #region Value

	//        public ErrorValue Value
	//        {
	//            get { return this.value; }
	//        } 

	//        #endregion Value
	//    }

	#endregion // Old Code





	internal class ErrToken : OperandToken
	{
		#region Member Variables

		private ErrorValue value;

		#endregion Member Variables

		#region Constructor

		public ErrToken()
			: base(TokenClass.Value) { }

		public ErrToken(ErrorValue value)
			: this()
		{
			this.value = value;

			if (this.value == ErrorValue.InvalidCellReference)
				this.TokenClass = TokenClass.Reference;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			if (this.value == ErrorValue.Circularity)
				return 0;

			return this.value.ToCalcErrorValue();
		}

		#endregion GetCalcValue

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			ErrToken comparisonErrToken = (ErrToken)comparisonToken;
			return value == comparisonErrToken.value;
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			this.value = ErrorValue.FromValue(stream.ReadByteFromBuffer(ref data, ref dataIndex));
		}

		#endregion Load

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write(this.value.Value);
		}

		#endregion Save

		#region Token

		public override Token Token
		{
			get { return Token.Err; }
		}

		#endregion Token

		#region TokenClass

		public override TokenClass TokenClass
		{
			get
			{
				if (this.Value == ErrorValue.InvalidCellReference)
					return TokenClass.Reference;

				return base.TokenClass;
			}
		}
		#endregion //TokenClass

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return this.value.ToString();
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Value

		public ErrorValue Value
		{
			get { return this.value; }
		}

		#endregion Value
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