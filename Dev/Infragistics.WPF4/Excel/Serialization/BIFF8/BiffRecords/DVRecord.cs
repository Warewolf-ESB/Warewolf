using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 9/12/08 - TFS6887
	internal class DVRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// MD 2/1/11 - Data Validation support
			//if ( worksheet.DataValidationInfo2003 == null )
			//{
			//    Utilities.DebugFail("The worksheet had no data validation info.");
			//    return;
			//}

			byte[] data = new byte[ 0 ];
			int dataIndex = 0;
			uint dwDvFlags = manager.CurrentRecordStream.ReadUInt32FromBuffer( ref data, ref dataIndex );

			// MD 2/1/11 - Data Validation support
			DataValidationType valType = (DataValidationType)
									( dwDvFlags & 0x0000000F ); // Validation type
			ErrorAlertStyle errStyle = (ErrorAlertStyle)
								  ( ( dwDvFlags & 0x00000070 ) >> 4 ); // Error alert style
			bool fStrLookup =		( dwDvFlags & 0x00000080 ) != 0; // =1 if this is list-type validation with an explicitly expressed list of valid inputs.
			bool fAllowBlank =		( dwDvFlags & 0x00000100 ) != 0; // =1 suppress an error when any cell referenced by the validation formula is blank.
			bool fSuppressCombo =	( dwDvFlags & 0x00000200 ) != 0; // =1 if this is list-type validation, which no drop-down to be displayed in the cell when selected.
			uint mdImeMode =		( dwDvFlags & 0x0003FC00 ) >> 10; // The IME mode to be used for this cell (East Asian versions only).
			bool fShowInputMsg =	( dwDvFlags & 0x00040000 ) != 0; // =1 show input message box
			bool fShowErrorMsg =	( dwDvFlags & 0x00080000 ) != 0; // =1 show error message box
			DataValidationOperatorType typOperator = (DataValidationOperatorType)
								  ( ( dwDvFlags & 0x00F00000 ) >> 20 ); // Operator type

			// MD 2/1/11 - Data Validation support
			DataValidationRule rule;
			switch (valType)
			{
				case DataValidationType.AnyValue:
					rule = new AnyValueDataValidationRule();
					break;

				case DataValidationType.List:
					ListDataValidationRule listRule = new ListDataValidationRule();
					listRule.ShowDropdown = (fSuppressCombo == false);
					rule = listRule;
					break;

				case DataValidationType.Formula:
					rule = new CustomDataValidationRule();
					break;

				case DataValidationType.Date:
				case DataValidationType.Decimal:
				case DataValidationType.TextLength:
				case DataValidationType.Time:
				case DataValidationType.WholeNumber:
					switch (typOperator)
					{
						case DataValidationOperatorType.Between:
						case DataValidationOperatorType.NotBetween:
							rule = new TwoConstraintDataValidationRule((TwoConstraintDataValidationOperator)typOperator, (DataValidationCriteria)valType);
							break;

						case DataValidationOperatorType.Equal:
						case DataValidationOperatorType.NotEqual:
						case DataValidationOperatorType.GreaterThan:
						case DataValidationOperatorType.LessThan:
						case DataValidationOperatorType.GreaterThanOrEqual:
						case DataValidationOperatorType.LessThanOrEqual:
							rule = new OneConstraintDataValidationRule((OneConstraintDataValidationOperator)typOperator, (DataValidationCriteria)valType);
							break;

						default:
							Utilities.DebugFail("Unknown DataValidationOperatorType: " + typOperator);
							return;
					}
					break;

				default:
					Utilities.DebugFail("Unknown DataValidationType: " + valType);
					return;
			}

			// MD 6/11/12 - TFS113884
			// Temporarily store the loading path so we can ignore 3D refs in the formulas which use this path.
			rule.LoadingWorkbookPath = manager.Workbook.LoadingPath;

			// MD 2/1/11 - Data Validation support
			rule.AllowNullInternal = fAllowBlank;
			rule.ErrorStyle = (DataValidationErrorStyle)errStyle;
			rule.ShowInputMessage = fShowInputMsg;
			rule.ShowErrorMessageForInvalidValue = fShowErrorMsg;

			string promptBoxTitle = manager.CurrentRecordStream.ReadFormattedStringFromBuffer( LengthType.SixteenBit, ref data, ref dataIndex ).UnformattedString;
			DVRecord.DecodeDVString( ref promptBoxTitle );
			// MD 2/1/11 - Data Validation support
			rule.InputMessageTitle = promptBoxTitle;

			string errorTitle = manager.CurrentRecordStream.ReadFormattedStringFromBuffer( LengthType.SixteenBit, ref data, ref dataIndex ).UnformattedString;
			DVRecord.DecodeDVString( ref errorTitle );
			// MD 2/1/11 - Data Validation support
			rule.ErrorMessageTitle = errorTitle;

			string promptBoxMessage = manager.CurrentRecordStream.ReadFormattedStringFromBuffer( LengthType.SixteenBit, ref data, ref dataIndex ).UnformattedString;
			DVRecord.DecodeDVString( ref promptBoxMessage );
			// MD 2/1/11 - Data Validation support
			rule.InputMessageDescription = promptBoxMessage;

			string errorMessage = manager.CurrentRecordStream.ReadFormattedStringFromBuffer( LengthType.SixteenBit, ref data, ref dataIndex ).UnformattedString;
			DVRecord.DecodeDVString( ref errorMessage );
			// MD 2/1/11 - Data Validation support
			rule.ErrorMessageDescription = errorMessage;

			ushort formulaForFirstConditionTokenArraySize = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			dataIndex += 2;

			// MD 12/21/11 - TFS97840
			// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
			//Formula formulaForFirstCondition = Formula.Load( manager, manager.CurrentRecordStream, formulaForFirstConditionTokenArraySize, FormulaType.Formula, ref data, ref dataIndex );
			Formula formulaForFirstCondition = Formula.Load(manager.CurrentRecordStream, formulaForFirstConditionTokenArraySize, rule.FormulaType, ref data, ref dataIndex);

			// MD 2/1/11 - Data Validation support
			rule.SetFormula1(formulaForFirstCondition, null);

			// MD 2/1/11 - Data Validation support
			// The commas in the list of literals are converted to 0's in the 2003 format. Convert them back to commas.
			if (fStrLookup &&
				formulaForFirstCondition != null &&
				formulaForFirstCondition.PostfixTokenList.Count == 1)
			{
				StrToken token = formulaForFirstCondition.PostfixTokenList[0] as StrToken;

				if (token != null)
					formulaForFirstCondition.PostfixTokenList[0] = new StrToken(token.Value.Replace('\0', ','));
			}

			ushort formulaForSecondConditionTokenArraySize = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			dataIndex += 2;

			// MD 12/21/11 - TFS97840
			// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
			//Formula formulaForSecondCondition = Formula.Load(manager, manager.CurrentRecordStream, formulaForSecondConditionTokenArraySize, FormulaType.Formula, ref data, ref dataIndex);
			Formula formulaForSecondCondition = Formula.Load(manager.CurrentRecordStream, formulaForSecondConditionTokenArraySize, rule.FormulaType, ref data, ref dataIndex);

			// MD 2/1/11 - Data Validation support
			rule.SetFormula2(formulaForSecondCondition, null);

			List<CellAddressRange> ranges = manager.CurrentRecordStream.ReadFormulaCellAddressRangeListFromBuffer( ref data, ref dataIndex );

			// MD 2/1/11 - Data Validation support
			//DataValidationCriteria criteria = new DataValidationCriteria();
			//worksheet.DataValidationInfo2003.DataValidations.Add(criteria);
			//
			//criteria.DwDvFlags = dwDvFlags;
			//criteria.ErrorMessage = errorMessage;
			//criteria.ErrorTitle = errorTitle;
			//criteria.FormulaForFirstCondition = formulaForFirstCondition;
			//criteria.FormulaForSecondCondition = formulaForSecondCondition;
			//criteria.PromptBoxMessage = promptBoxMessage;
			//criteria.PromptBoxTitle = promptBoxTitle;
			//criteria.Ranges = ranges;
			WorksheetReferenceCollection references = new WorksheetReferenceCollection(worksheet);
			foreach (CellAddressRange range in ranges)
			{
				// MD 2/20/12 - 12.1 - Table Support
				//references.Add(range.GetTargetRegion(worksheet, null, -1, false));
				WorksheetRegion region = range.GetTargetRegion(worksheet, -1, -1, false);
				if (region != null)
					references.Add(region);
				else
					Utilities.DebugFail("This is unexpected.");
			}

			worksheet.DataValidationRules.AddInternal(rule, references);

			// MD 6/11/12 - TFS113884
			// Clear the temporary loading path.
			rule.LoadingWorkbookPath = null;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 2/1/11 - Data Validation support
			//DataValidationCriteria criteria = (DataValidationCriteria)manager.ContextStack[typeof(DataValidationCriteria)];
			//
			//if (criteria == null)
			//{
			//    Utilities.DebugFail("There is no data validation criteria in the context stack.");
			//    return;
			//}
			//
			//manager.CurrentRecordStream.Write(criteria.DwDvFlags);
			object ruleReferencesPairBoxed = manager.ContextStack[typeof(KeyValuePair<DataValidationRule, WorksheetReferenceCollection>)];

			if (ruleReferencesPairBoxed == null)
			{
				Utilities.DebugFail("There is no data validation rule in the context stack.");
				return;
			}

			KeyValuePair<DataValidationRule, WorksheetReferenceCollection> ruleReferencesPair = 
				(KeyValuePair<DataValidationRule, WorksheetReferenceCollection>)ruleReferencesPairBoxed;

			DataValidationRule rule = ruleReferencesPair.Key;
			ListDataValidationRule listRule = rule as ListDataValidationRule;
			Formula formula1 = rule.GetFormula1(null);

			uint dwDvFlags = 0;
			dwDvFlags |= (uint)rule.ValidationType;
			dwDvFlags |= (uint)rule.ErrorStyle << 4;

			if (listRule != null &&
				formula1 != null &&
				formula1.PostfixTokenList.Count == 1 &&
				formula1.PostfixTokenList[0] is StrToken)
			{
				formula1 = formula1.Clone();
				StrToken strToken = (StrToken)formula1.PostfixTokenList[0];
				formula1.PostfixTokenList[0] = new StrToken(strToken.Value.Replace(",", "\0"));

				dwDvFlags |= 0x00000080;
			}

			if (rule.AllowNullInternal)
				dwDvFlags |= 0x00000100;

			if (listRule != null && listRule.ShowDropdown == false)
				dwDvFlags |= 0x00000200;

			if (rule.ShowInputMessage)
				dwDvFlags |= 0x00040000;

			if (rule.ShowErrorMessageForInvalidValue)
				dwDvFlags |= 0x00080000;

			dwDvFlags |= (uint)rule.OperatorType << 20;

			manager.CurrentRecordStream.Write(dwDvFlags);

			// MD 2/1/11 - Data Validation support
			//string promptBoxTitle = criteria.PromptBoxTitle;
			string promptBoxTitle = rule.InputMessageTitle;

			DVRecord.EncodeDVString( ref promptBoxTitle );
			manager.CurrentRecordStream.Write( promptBoxTitle, LengthType.SixteenBit );

			// MD 2/1/11 - Data Validation support
			//string errorTitle = criteria.ErrorTitle;
			string errorTitle = rule.ErrorMessageTitle;

			DVRecord.EncodeDVString( ref errorTitle );
			manager.CurrentRecordStream.Write( errorTitle, LengthType.SixteenBit );

			// MD 2/1/11 - Data Validation support
			//string promptBoxMessage = criteria.PromptBoxMessage;
			string promptBoxMessage = rule.InputMessageDescription;

			// MD 1/5/12 - TFS98535
			// We should remove carriage returns from the descriptions.
			promptBoxMessage = Utilities.RemoveCarriageReturns(promptBoxMessage);

			DVRecord.EncodeDVString( ref promptBoxMessage );
			manager.CurrentRecordStream.Write( promptBoxMessage, LengthType.SixteenBit );

			// MD 2/1/11 - Data Validation support
			//string errorMessage = criteria.ErrorMessage;
			string errorMessage = rule.ErrorMessageDescription;

			// MD 1/5/12 - TFS98535
			// We should remove carriage returns from the descriptions.
			errorMessage = Utilities.RemoveCarriageReturns(errorMessage);

			DVRecord.EncodeDVString( ref errorMessage );
			manager.CurrentRecordStream.Write( errorMessage, LengthType.SixteenBit );

			long formula1LengthPos = manager.CurrentRecordStream.Position;
			manager.CurrentRecordStream.Write( (ushort)0 ); // formula1 length, leave at zero for now, we will come back later to update it
			manager.CurrentRecordStream.Write( (ushort)0 );

			// MD 2/1/11 - Data Validation support
			//if ( criteria.FormulaForFirstCondition != null )
			//{
			//    int formula1Length = criteria.FormulaForFirstCondition.Save( manager, manager.CurrentRecordStream, false, false );
			if (formula1 != null)
			{
				// MD 9/19/11 - TFS86108
				// We need to keep shared tokens when saving the formulas, so pass False for the resolveSharedReferences parameter.
				//int formula1Length = formula1.Save(manager, manager.CurrentRecordStream, false, false);
				int formula1Length = formula1.Save(manager.CurrentRecordStream, false, false, false);

				long currentPos = manager.CurrentRecordStream.Position;
				manager.CurrentRecordStream.Position = formula1LengthPos;
				manager.CurrentRecordStream.Write( (int)formula1Length );
				manager.CurrentRecordStream.Position = currentPos;
			}

			long formula2LengthPos = manager.CurrentRecordStream.Position;
			manager.CurrentRecordStream.Write( (ushort)0 ); // formula2 length, leave at zero for now, we will come back later to update it
			manager.CurrentRecordStream.Write( (ushort)0 );

			// MD 2/1/11 - Data Validation support
			//if ( criteria.FormulaForSecondCondition != null )
			//{
			//    int formula2Length = criteria.FormulaForSecondCondition.Save( manager, manager.CurrentRecordStream, false, false );
			Formula formula2 = rule.GetFormula2(null);
			if (formula2 != null)
			{
				// MD 9/19/11 - TFS86108
				// We need to keep shared tokens when saving the formulas, so pass False for the resolveSharedReferences parameter.
				//int formula2Length = formula2.Save(manager, manager.CurrentRecordStream, false, false);
				int formula2Length = formula2.Save(manager.CurrentRecordStream, false, false, false);

				long currentPos = manager.CurrentRecordStream.Position;
				manager.CurrentRecordStream.Position = formula2LengthPos;
				manager.CurrentRecordStream.Write( (int)formula2Length );
				manager.CurrentRecordStream.Position = currentPos;
			}

			// MD 2/1/11 - Data Validation support
			//manager.CurrentRecordStream.Write( criteria.Ranges );
			List<CellAddressRange> ranges = new List<CellAddressRange>();
			foreach (WorksheetRegion region in (IEnumerable<WorksheetRegion>)ruleReferencesPair.Value)
				ranges.Add(new CellAddressRange(region));

			manager.CurrentRecordStream.Write( ranges );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.DV; }
		}

		private static void DecodeDVString( ref string promptBoxTitle )
		{
			if ( promptBoxTitle.Length == 1 && promptBoxTitle[ 0 ] == (char)0 )
				promptBoxTitle = string.Empty;
		}

		private static void EncodeDVString( ref string promptBoxTitle )
		{
			// MD 2/1/11 - Data Validation support
			//if ( promptBoxTitle.Length == 0 )
			if (String.IsNullOrEmpty(promptBoxTitle))
				promptBoxTitle = "\0";
		}
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