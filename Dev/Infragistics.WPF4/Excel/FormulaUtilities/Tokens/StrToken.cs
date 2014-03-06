using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// String constant operand. Indicates a string constant.
	//    /// </summary> 
	//#endif
	//    // MD 8/18/08 - Excel formula solving
	//    //internal class StrToken : FormulaToken
	//    internal class StrToken : OperandToken
	//    {
	//        #region Member Variables

	//        private string value;

	//        #endregion // Member Variables

	//        #region Constructor

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public StrToken( Formula formula )
	//        //    : base( formula, TokenClass.Value ) { }
	//        public StrToken()
	//            : base(TokenClass.Value) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public StrToken( Formula formula, string value )
	//        //    : this( formula )
	//        public StrToken(string value)
	//            : this()
	//        {
	//            this.value = value;
	//        }

	//        #endregion // Constructor

	//        #region Base Class Overrides

	//        // MD 10/22/10 - TFS36696
	//        // We can use the default implementation of this now.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new StrToken( newOwningFormula, this.value );
	//        //}

	//        #region GetCalcValue

	//        // MD 8/18/08 - Excel formula solving
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override object GetCalcValue( Workbook workbook, IWorksheetCell formulaOwner )
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            return this.value;
	//        }

	//        #endregion // GetCalcValue

	//        #region GetSize

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            // MD 10/22/10 - TFS36696
	//            // To save space, the positionInRecordStream is no longer stored on the token
	//            //return (byte)( 1 + stream.GetDataSize( 
	//            //    this.PositionInRecordStream, 
	//            //    LengthType.EightBit, 
	//            //    this.value ) );
	//            return (byte)(1 + stream.GetDataSize(
	//                tokenPositionsInRecordStream[this].PositionInRecordStream,
	//                LengthType.EightBit,
	//                this.value));
	//        }

	//        #endregion // GetSize

	//        #region IsEquivalentTo

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            StrToken comparisonStrToken = (StrToken)comparisonToken;
	//            return this.value == comparisonStrToken.value;
	//        }

	//        #endregion // IsEquivalentTo

	//        #region Load

	//        public override void Load(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
	//        {
	//            this.value = stream.ReadFormattedStringFromBuffer(LengthType.EightBit, ref data, ref dataIndex).UnformattedString;
	//        } 

	//        #endregion // Load

	//        #region Save

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write(this.value, LengthType.EightBit);
	//        }

	//        #endregion // Save

	//        #region ToString

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            // MD 5/13/11 - Data Validations / Page Breaks
	//            // Double quotes need to be escaped in the formula string by doubling them up.
	//            //return "\"" + this.value + "\"";
	//            return "\"" + this.value.Replace("\"", "\"\"") + "\"";
	//        }

	//        #endregion // ToString

	//        #region Token

	//        public override Token Token
	//        {
	//            get { return Token.Str; }
	//        }

	//        #endregion // Token

	//        #endregion // Base Class Overrides

	//        #region Properties

	//        public string Value
	//        {
	//            get { return this.value; }
	//        }

	//        #endregion // Properties
	//    }

	#endregion // Old Code





	internal class StrToken : OperandToken
	{
		#region Member Variables

		private string value;

		#endregion // Member Variables

		#region Constructor

		public StrToken()
			: base(TokenClass.Value) { }

		public StrToken(string value)
			: this()
		{
			this.value = value;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			return this.value;
		}

		#endregion // GetCalcValue

		#region GetSize

		public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			return (byte)(1 + stream.GetDataSize(
				tokenPositionsInRecordStream[this].PositionInRecordStream,
				LengthType.EightBit,
				this.value));
		}

		#endregion // GetSize

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			StrToken comparisonStrToken = (StrToken)comparisonToken;
			return this.value == comparisonStrToken.value;
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			this.value = stream.ReadFormattedStringFromBuffer(LengthType.EightBit, ref data, ref dataIndex).UnformattedString;
		}

		#endregion // Load

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write(this.value, LengthType.EightBit);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get { return Token.Str; }
		}

		#endregion // Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			// Double quotes need to be escaped in the formula string by doubling them up.
			return "\"" + this.value.Replace("\"", "\"\"") + "\"";
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region Properties

		public string Value
		{
			get { return this.value; }
		}

		#endregion // Properties
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