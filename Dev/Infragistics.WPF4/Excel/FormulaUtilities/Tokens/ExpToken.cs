using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;




using System.Drawing;


namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Array formula or shared formula token. Indicates an array formula or shared formula cell. 
	//    /// When this token is present, it is the only token in the formula. 
	//    /// </summary>  
	//#endif
	//    internal class ExpToken : FormulaToken
	//    {
	//        private CellAddress cellAddress;

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public ExpToken( Formula formula )
	//        //    : base( formula, TokenClass.Control ) { }
	//        public ExpToken()
	//            : base(TokenClass.Control) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public ExpToken( Formula formula, int firstColumn, int firstRow )
	//        //    : this( formula )
	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //public ExpToken(int firstColumn, int firstRow)
	//        public ExpToken(short firstColumn, int firstRow)
	//            : this()
	//        {
	//            this.cellAddress = new CellAddress( firstRow, false, firstColumn, false );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public ExpToken( Formula formula, CellAddress cellAddress )
	//        //    : this( formula )
	//        public ExpToken(CellAddress cellAddress)
	//            : this()
	//        {
	//            this.cellAddress = cellAddress;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new ExpToken( newOwningFormula, this.cellAddress.Clone() );
	//        //}
	//        public override FormulaToken GetTokenForClonedFormula()
	//        {
	//            return new ExpToken(this.cellAddress);
	//        }

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            int firstRow = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );

	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //int firstColumn = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//            short firstColumn = stream.ReadInt16FromBuffer(ref data, ref dataIndex);

	//            this.cellAddress = new CellAddress( firstRow, false, firstColumn, false );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 5;
	//        }

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            ExpToken comparisonExpToken = (ExpToken)comparisonToken;
	//            return this.cellAddress.Equals(comparisonExpToken.cellAddress);
	//        }

	//        public override void OffsetReferences( Point offset )
	//        {
	//            // MD 10/22/10 - TFS36696
	//            // This CellAddress is now immutable.
	//            //this.cellAddress.Offset( offset );
	//            this.cellAddress = this.cellAddress.Offset(offset);
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write( (ushort)this.cellAddress.Row );
	//            stream.Write( (ushort)this.cellAddress.Column );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            // MD 10/22/10 - TFS36696
	//            // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell and format from the formula.
	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //IWorksheetCell sourceCell = null;
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //WorksheetRow sourceCellRow = null;
	//            int sourceCellRowIndex = -1;
	//            short sourceCellColumnIndex = -1;

	//            // MD 2/24/12
	//            // Found while implementing 12.1 - Table Support
	//            // We should use the least restrictive format version when there is no workbook, not the most.
	//            //WorkbookFormat currentFormat = WorkbookFormat.Excel97To2003;
	//            WorkbookFormat currentFormat = Workbook.LatestFormat;

	//            if (owningFormula != null)
	//            {
	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //sourceCell = owningFormula.OwningCell;
	//                // MD 2/20/12 - 12.1 - Table Support
	//                //sourceCellRow = owningFormula.OwningCellRow;
	//                if (owningFormula.OwningCellRow != null)
	//                    sourceCellRowIndex = owningFormula.OwningCellRow.Index;

	//                sourceCellColumnIndex = owningFormula.OwningCellColumnIndex;

	//                currentFormat = owningFormula.CurrentFormat;
	//            }

	//            // MD 6/31/08 - Excel 2007 Format
	//            //return "<Array Interior Or Shared Formula, Source: " + this.cellAddress.ToString( sourceCell, cellReferenceMode ) + ">";
	//            return 
	//                "<Array Interior Or Shared Formula, Source: " +
	//                // MD 10/22/10 - TFS36696
	//                // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell and format from the formula.
	//                //this.cellAddress.ToString( sourceCell, cellReferenceMode, this.Formula.CurrentFormat ) + 
	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //this.cellAddress.ToString(sourceCell, cellReferenceMode, currentFormat) + 
	//                // MD 2/20/12 - 12.1 - Table Support
	//                //this.cellAddress.ToString(sourceCellRow, sourceCellColumnIndex, cellReferenceMode, currentFormat) + 
	//                this.cellAddress.ToString(sourceCellRowIndex, sourceCellColumnIndex, cellReferenceMode, currentFormat) + 
	//                ">";
	//        }

	//        public override Token Token
	//        {
	//            get { return Token.Exp; }
	//        }

	//        // MD 3/30/10 - TFS30253
	//        // This needs to be accessed by other classes.
	//        public CellAddress CellAddress
	//        {
	//            get { return this.cellAddress; }
	//        }
	//    }

	#endregion // Old Code






	internal class ExpToken : FormulaToken
	{
		#region Member Variables

		private CellAddress cellAddress;

		#endregion // Member Variables

		#region Constructor

		public ExpToken()
			: base(TokenClass.Control) { }

		public ExpToken(short firstColumn, int firstRow)
			: this(new CellAddress(firstRow, false, firstColumn, false)) { }

		public ExpToken(CellAddress cellAddress)
			: this()
		{
			this.cellAddress = cellAddress;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new ExpToken(this.cellAddress);
		}

		#endregion // GetTokenForClonedFormula

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			if (sourceContext.Worksheet != comparisonContext.Worksheet)
				return false;

			ExpToken comparisonExpToken = (ExpToken)comparisonToken;
			return this.cellAddress.Equals(comparisonExpToken.cellAddress);
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			int firstRow = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);

			short firstColumn = stream.ReadInt16FromBuffer(ref data, ref dataIndex);
			this.cellAddress = new CellAddress(firstRow, false, firstColumn, false);
		}

		#endregion // Load

		#region OffsetReferences

		public override void OffsetReferences(FormulaContext context, Point offset)
		{
			this.cellAddress = this.cellAddress.Offset(context, offset);
		}

		#endregion // OffsetReferences

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write((ushort)this.cellAddress.Row);
			stream.Write((ushort)this.cellAddress.Column);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get { return Token.Exp; }
		}

		#endregion // Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return
				"<Array Interior Or Shared Formula, Source: " +
				this.cellAddress.ToString(context.OwningCellAddress.RowIndex, context.OwningCellAddress.ColumnIndex, context.CellReferenceMode, context.Format) +
				">";
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region CellAddress

		public CellAddress CellAddress
		{
			get { return this.cellAddress; }
		} 

		#endregion // CellAddress
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