using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Constants;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Array constant operand. Indicates an array of constant values.
	//    /// </summary> 
	//#endif
	//    // MD 8/18/08 - Excel formula solving
	//    //internal class ArrayToken : FormulaToken
	//    internal class ArrayToken : OperandToken
	//    {
	//        #region Member Variables

	//        // MD 8/18/08 - Excel formula solving
	//        private ExcelCalcValue[ , ] arrayForCalcManager;

	//        private Constant[][] values; 

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public ArrayToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        //
	//        //public ArrayToken( Formula formula, Constant[][] values )
	//        //    : this( formula, TokenClass.Array )
	//        public ArrayToken(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        public ArrayToken(Constant[][] values)
	//            : this(TokenClass.Array)
	//        {
	//            this.values = values;
	//        } 

	//        #endregion Constructor

	//        #region Base Class Overrides

	//        #region Clone

	//        // MD 10/22/10 - TFS36696
	//        // We can use the default implementation of this now.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    ArrayToken arrayToken = new ArrayToken( newOwningFormula, this.TokenClass );
	//        //    ArrayToken arrayToken = new ArrayToken(this.TokenClass);
	//        //
	//        //    int numberOfColumns = this.values.Length;
	//        //    int numberOfRows = this.values[ 0 ].Length;
	//        //
	//        //    arrayToken.values = new Constant[ numberOfColumns ][];
	//        //
	//        //    for ( int col = 0; col < numberOfColumns; col++ )
	//        //        arrayToken.values[ col ] = new Constant[ numberOfRows ];
	//        //
	//        //    for ( int row = 0; row < numberOfRows; row++ )
	//        //    {
	//        //        for ( int col = 0; col < numberOfColumns; col++ )
	//        //            arrayToken.values[ col ][ row ] = this.values[ col ][ row ].Clone();
	//        //    }
	//        //
	//        //    return arrayToken;
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
	//            if ( this.arrayForCalcManager == null )
	//            {
	//                int numberOfColumns = values.Length;
	//                int numberOfRows = values[ 0 ].Length;

	//                this.arrayForCalcManager = new ExcelCalcValue[ numberOfColumns, numberOfRows ];

	//                for ( int row = 0; row < numberOfRows; row++ )
	//                {
	//                    for ( int col = 0; col < numberOfColumns; col++ )
	//                    {
	//                        this.arrayForCalcManager[ col, row ] = new ExcelCalcValue( values[ col ][ row ].CalcValue );
	//                    }
	//                }
	//            }

	//            return this.arrayForCalcManager;
	//        } 

	//        #endregion GetCalcValue

	//        #region Load

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            stream.ReadBytesFromBuffer( 7, ref data, ref dataIndex ); // not used
	//        }

	//        #endregion Load

	//        #region LoadAdditionalData

	//        public override void LoadAdditionalData( BiffRecordStream stream, ref byte[] data, ref int dataIndex )
	//        {
	//            int numberOfColumns = stream.ReadByteFromBuffer( ref data, ref dataIndex ) + 1;
	//            int numberOfRows = stream.ReadUInt16FromBuffer( ref data, ref dataIndex ) + 1;

	//            this.values = new Constant[ numberOfColumns ][];

	//            for ( int col = 0; col < numberOfColumns; col++ )
	//                this.values[ col ] = new Constant[ numberOfRows ];

	//            for ( int row = 0; row < numberOfRows; row++ )
	//            {
	//                for ( int col = 0; col < numberOfColumns; col++ )
	//                {
	//                    Constant constant = Constant.GetNextConstant( stream, ref data, ref dataIndex );
	//                    constant.Load( stream, ref data, ref dataIndex );

	//                    this.values[ col ][ row ] = constant;
	//                }
	//            }
	//        }

	//        #endregion LoadAdditionalData

	//        #region GetSize

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 8;
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

	//            ArrayToken comparisonArrayToken = (ArrayToken)comparisonToken;

	//            if (this.values.Length != comparisonArrayToken.values.Length)
	//                return false;

	//            if (this.values[0].Length != comparisonArrayToken.values[0].Length)
	//                return false;

	//            int numberOfColumns = values.Length;
	//            int numberOfRows = values[0].Length;

	//            for (int row = 0; row < numberOfRows; row++)
	//            {
	//                for (int col = 0; col < numberOfColumns; col++)
	//                {
	//                    if (this.values[col][row].Equals(comparisonArrayToken.values[col][row]) == false)
	//                        return false;
	//                }
	//            }

	//            return true;
	//        }

	//        #endregion // IsEquivalentTo

	//        #region Save

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write( new byte[ 7 ] );
	//        }

	//        #endregion Save

	//        #region SaveAdditionalData

	//        public override void SaveAdditionalData( BiffRecordStream stream )
	//        {
	//            int numberOfColumns = this.values.Length;
	//            int numberOfRows = this.values[ 0 ].Length;

	//            stream.Write( (byte)( numberOfColumns - 1 ) );
	//            stream.Write( (ushort)( numberOfRows - 1 ) );

	//            for ( int row = 0; row < numberOfRows; row++ )
	//            {
	//                for ( int col = 0; col < numberOfColumns; col++ )
	//                {
	//                    Constant constant = this.values[ col ][ row ];
	//                    stream.Write( constant.ConstantCode );
	//                    constant.Save( stream );
	//                }
	//            }
	//        }

	//        #endregion SaveAdditionalData

	//        #region Token

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch ( this.TokenClass )
	//                {
	//                    case TokenClass.Array:
	//                        return Token.ArrayA;
	//                    case TokenClass.Reference:
	//                        return Token.ArrayR;
	//                    case TokenClass.Value:
	//                        return Token.ArrayV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.ArrayV;
	//                }
	//            }
	//        }

	//        #endregion Token 

	//        #region ToString

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            // MD 9/11/09 - TFS20376
	//            // The value separator will be different based on the culture, use the member variable instead.
	//            string decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
	//            string valueSeparator = FormulaParser.GetMatrixValueSeparatorResolved( decimalSeparator );

	//            StringBuilder sb = new StringBuilder( "{" );

	//            int numberOfColumns = values.Length;
	//            int numberOfRows = values[ 0 ].Length;

	//            for ( int row = 0; row < numberOfRows; row++ )
	//            {
	//                for ( int col = 0; col < numberOfColumns; col++ )
	//                {
	//                    //sb.Append( values[ col ][ row ].ToString() );
	//                    sb.Append(values[col][row].ToString(culture));

	//                    if ( col != numberOfColumns - 1 )
	//                    {
	//                        // MD 9/11/09 - TFS20376
	//                        // Use the culture based separator instead.
	//                        //sb.Append( FormulaParser.UnionOperator );
	//                        sb.Append( valueSeparator );
	//                    }
	//                }

	//                if ( row != numberOfRows - 1 )
	//                    sb.Append( ";" );
	//            }

	//            sb.Append( "}" );

	//            return sb.ToString();
	//        }

	//        #endregion ToString

	//        #endregion Base Class Overrides
	//    } 

	#endregion // Old Code





	internal class ArrayToken : OperandToken
	{
		#region Member Variables

		private ExcelCalcValue[,] arrayForCalcManager;
		private Constant[][] values;

		#endregion Member Variables

		#region Constructor

		public ArrayToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public ArrayToken(Constant[][] values)
			: this(TokenClass.Array)
		{
			this.values = values;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			if (this.arrayForCalcManager == null)
			{
				int numberOfColumns = values.Length;
				int numberOfRows = values[0].Length;

				this.arrayForCalcManager = new ExcelCalcValue[numberOfColumns, numberOfRows];

				for (int row = 0; row < numberOfRows; row++)
				{
					for (int col = 0; col < numberOfColumns; col++)
					{
						this.arrayForCalcManager[col, row] = new ExcelCalcValue(values[col][row].CalcValue);
					}
				}
			}

			return this.arrayForCalcManager;
		}

		#endregion GetCalcValue

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			ArrayToken comparisonArrayToken = (ArrayToken)comparisonToken;

			if (this.values.Length != comparisonArrayToken.values.Length)
				return false;

			if (this.values[0].Length != comparisonArrayToken.values[0].Length)
				return false;

			int numberOfColumns = values.Length;
			int numberOfRows = values[0].Length;

			for (int row = 0; row < numberOfRows; row++)
			{
				for (int col = 0; col < numberOfColumns; col++)
				{
					if (this.values[col][row].Equals(comparisonArrayToken.values[col][row]) == false)
						return false;
				}
			}

			return true;
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			stream.ReadBytesFromBuffer(7, ref data, ref dataIndex); // not used
		}

		#endregion Load

		#region LoadAdditionalData

		public override void LoadAdditionalData(BiffRecordStream stream, ref byte[] data, ref int dataIndex)
		{
			int numberOfColumns = stream.ReadByteFromBuffer(ref data, ref dataIndex) + 1;
			int numberOfRows = stream.ReadUInt16FromBuffer(ref data, ref dataIndex) + 1;

			this.values = new Constant[numberOfColumns][];

			for (int col = 0; col < numberOfColumns; col++)
				this.values[col] = new Constant[numberOfRows];

			for (int row = 0; row < numberOfRows; row++)
			{
				for (int col = 0; col < numberOfColumns; col++)
				{
					Constant constant = Constant.GetNextConstant(stream, ref data, ref dataIndex);
					constant.Load(stream, ref data, ref dataIndex);

					this.values[col][row] = constant;
				}
			}
		}

		#endregion LoadAdditionalData

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write(new byte[7]);
		}

		#endregion Save

		#region SaveAdditionalData

		public override void SaveAdditionalData(BiffRecordStream stream)
		{
			int numberOfColumns = this.values.Length;
			int numberOfRows = this.values[0].Length;

			stream.Write((byte)(numberOfColumns - 1));
			stream.Write((ushort)(numberOfRows - 1));

			for (int row = 0; row < numberOfRows; row++)
			{
				for (int col = 0; col < numberOfColumns; col++)
				{
					Constant constant = this.values[col][row];
					stream.Write(constant.ConstantCode);
					constant.Save(stream);
				}
			}
		}

		#endregion SaveAdditionalData

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array:
						return Token.ArrayA;
					case TokenClass.Reference:
						return Token.ArrayR;
					case TokenClass.Value:
						return Token.ArrayV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.ArrayV;
				}
			}
		}

		#endregion Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			// MD 9/11/09 - TFS20376
			// The value separator will be different based on the culture, use the member variable instead.
			string decimalSeparator = context.Culture.NumberFormat.NumberDecimalSeparator;
			string valueSeparator = FormulaParser.GetMatrixValueSeparatorResolved(decimalSeparator);

			StringBuilder sb = new StringBuilder("{");

			int numberOfColumns = values.Length;
			int numberOfRows = values[0].Length;

			for (int row = 0; row < numberOfRows; row++)
			{
				for (int col = 0; col < numberOfColumns; col++)
				{
					sb.Append(values[col][row].ToString(context.Culture));

					if (col != numberOfColumns - 1)
						sb.Append(valueSeparator);
				}

				if (row != numberOfRows - 1)
					sb.Append(";");
			}

			sb.Append("}");

			return sb.ToString();
		}

		#endregion ToString

		#endregion Base Class Overrides
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