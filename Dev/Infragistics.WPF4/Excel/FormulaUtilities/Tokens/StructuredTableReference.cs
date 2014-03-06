using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//    internal class StructuredTableReference : ReferenceToken
	//    {
	//        #region Member Variables

	//        private InnerReference _innerReference;
	//        private StructuredTableReferenceKeywordType? _keyword;
	//        private string _simpleColumnName;

	//        private string _tableName;
	//        private string _workbookFileName;

	//        #endregion // Member Variables

	//        #region Constructor

	//        public StructuredTableReference(string workbookFileName, string tableName)
	//            : base(TokenClass.Reference)
	//        {
	//            _tableName = tableName;
	//            _workbookFileName = workbookFileName;
	//        }

	//        public StructuredTableReference(string workbookFileName, string tableName, StructuredTableReferenceKeywordType keyword)
	//            : this(workbookFileName, tableName)
	//        {
	//            _keyword = keyword;
	//        }

	//        public StructuredTableReference(string workbookFileName, string tableName, string simpleColumnName)
	//            : this(workbookFileName, tableName)
	//        {
	//            _simpleColumnName = simpleColumnName;
	//        }

	//        public StructuredTableReference(string workbookFileName, string tableName, InnerReference innerReference)
	//            : this(workbookFileName, tableName)
	//        {
	//            _innerReference = innerReference;
	//        }

	//        #endregion // Constructor

	//        #region Base Class Overrides

	//        #region GetCalcValue

	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            return this.GetCalcValueHelper(
	//                this.GetTable(workbook, formulaOwnerRow, formulaOwnerColumnIndex), 
	//                formulaOwnerRow);
	//        }

	//        #endregion // GetCalcValue

	//        #region GetSize

	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            Utilities.DebugFail("This token should not be written out to BIFF streams. It only applies to 2007 formats and later.");
	//            return 0;
	//        }

	//        #endregion // GetSize

	//        #region Is3DReference

	//        public override bool Is3DReference
	//        {
	//            get { return _workbookFileName != null; }
	//        }

	//        #endregion // Is3DReference

	//        #region IsEquivalentTo

	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            StructuredTableReference comparisonStructuredTableReference = (StructuredTableReference)comparisonToken;

	//            if (_keyword != comparisonStructuredTableReference._keyword)
	//                return false;

	//            if (_simpleColumnName != comparisonStructuredTableReference._simpleColumnName)
	//                return false;

	//            if (_tableName != comparisonStructuredTableReference._tableName)
	//                return false;

	//            if (_workbookFileName != comparisonStructuredTableReference._workbookFileName)
	//                return false;

	//            if (Object.Equals(_innerReference, comparisonStructuredTableReference._innerReference) == false)
	//                return false;

	//            return true;
	//        }

	//        #endregion // IsEquivalentTo

	//        #region OnNamedReferenceRenamed

	//        public override void OnNamedReferenceRenamed(Formula owningFormula, NamedReferenceBase namedReference, string oldName)
	//        {
	//            if (_workbookFileName == null &&
	//                _tableName != null &&
	//                // MD 4/9/12 - TFS101506
	//                //String.Equals(oldName, _tableName, StringComparison.CurrentCultureIgnoreCase))
	//                String.Compare(oldName, _tableName, owningFormula.Culture, CompareOptions.IgnoreCase) == 0)
	//            {
	//                Debug.Assert(namedReference is WorksheetTable, "This is unexpected.");
	//                _tableName = namedReference.Name;
	//            }
	//        }

	//        #endregion // OnNamedReferenceRenamed

	//        #region OnTableRemoved

	//        public override void ConvertTableReferencesToRanges(Workbook workbook, WorksheetTable table, WorksheetRow owningRow, short owningColumnIndex, out FormulaToken replacementToken)
	//        {
	//            replacementToken = null;

	//            if (table == null)
	//            {
	//                table = this.GetTable(workbook, owningRow, owningColumnIndex);
	//                if (table == null)
	//                    return;
	//            }

	//            // MD 4/9/12 - TFS101506
	//            CultureInfo culture = CultureInfo.CurrentCulture;
	//            if (workbook != null)
	//                culture = workbook.CultureResolved;

	//            // MD 5/4/12 - TFS107276
	//            // If the table reference is within the table, it may not specify a table name. We still need to convert these 
	//            // to valid ranges.
	//            //if (_workbookFileName == null &&
	//            //    _tableName != null &&
	//            //    // MD 4/9/12 - TFS101506
	//            //    //String.Equals(table.Name, _tableName, StringComparison.CurrentCultureIgnoreCase))
	//            //    String.Compare(table.Name, _tableName, culture, CompareOptions.IgnoreCase) == 0)
	//            bool isCurrentTable = false;
	//            if (_workbookFileName == null)
	//            {
	//                if (_tableName != null)
	//                    isCurrentTable = String.Compare(table.Name, _tableName, culture, CompareOptions.IgnoreCase) == 0;
	//                else if (owningRow != null && table.WholeTableRegion != null)
	//                    isCurrentTable = table.WholeTableRegion.Contains(owningRow, owningColumnIndex);
	//            }

	//            if (isCurrentTable)
	//            {
	//                TableCalcReferenceBase reference = this.GetCalcValueHelper(table, owningRow) as TableCalcReferenceBase;
	//                if (reference == null)
	//                    return;

	//                WorksheetRegion region = reference.Region;
	//                if (region == null)
	//                    return;

	//                replacementToken = new Area3DToken(
	//                    null,
	//                    null,
	//                    table.Worksheet.Name,
	//                    new CellAddressRange(region),
	//                    this.TokenClass);
	//            }
	//        }

	//        #endregion // OnTableRemoved

	//        #region SetDefaultWorkbookFileName

	//        public override void SetDefaultWorkbookFileName(string workbookFileName)
	//        {
	//            if (_workbookFileName == null)
	//                _workbookFileName = workbookFileName;
	//        }

	//        #endregion // SetDefaultWorkbookFileName

	//        #region ToString

	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            string tableName = _tableName;

	//            // If the table name is specified, but the formula is within the referenced table, remove the name.
	//            if (tableName != null &&
	//                owningFormula != null &&
	//                owningFormula.OwningCellRow != null)
	//            {
	//                WorksheetTable table = owningFormula.OwningCellRow.GetCellAssociatedTable(owningFormula.OwningCellColumnIndex);

	//                // MD 4/9/12 - TFS101506
	//                //if (table != null && String.Equals(table.Name, tableName, StringComparison.CurrentCultureIgnoreCase))
	//                if (table != null && String.Compare(table.Name, tableName, culture, CompareOptions.IgnoreCase) == 0)
	//                    tableName = null;
	//            }

	//            string referenceString = string.Empty;
	//            if (_workbookFileName != null)
	//                referenceString = Utilities.CreateReferenceString(_workbookFileName, null);

	//            return
	//                referenceString +
	//                _tableName +
	//                // MD 4/9/12 - TFS101506
	//                //StructuredTableReference.GetWrapInBrackets(this.GetStructuredTableReferenceString());
	//                StructuredTableReference.GetWrapInBrackets(this.GetStructuredTableReferenceString(culture));
	//        }

	//        #endregion // ToString

	//        #region Token

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch (this.TokenClass)
	//                {
	//                    case TokenClass.Array: return Token.StructuredTableReferenceA;
	//                    case TokenClass.Reference: return Token.StructuredTableReferenceR;
	//                    case TokenClass.Value: return Token.StructuredTableReferenceV;

	//                    default:
	//                        Utilities.DebugFail("Invalid token class");
	//                        return Token.StructuredTableReferenceV;
	//                }
	//            }
	//        }

	//        #endregion // Token

	//        #endregion // Base Class Overrides

	//        #region Methods

	//        #region GetCalcValueHelper

	//        private object GetCalcValueHelper(WorksheetTable table, WorksheetRow formulaOwnerRow)
	//        {
	//            if (table == null)
	//                return ErrorValue.InvalidCellReference.ToCalcErrorValue();

	//            if (_innerReference != null)
	//            {
	//                return _innerReference.GetCalcReference(table, formulaOwnerRow);
	//            }
	//            else if (_keyword.HasValue)
	//            {
	//                return new TableCalcReference(formulaOwnerRow, table, _keyword, null);
	//            }
	//            else if (_simpleColumnName != null)
	//            {
	//                WorksheetTableColumn column = table.Columns[_simpleColumnName];
	//                if (column != null)
	//                    return new TableColumnCalcReference(formulaOwnerRow, column, null, null);
	//            }

	//            return ErrorValue.InvalidCellReference.ToCalcErrorValue();
	//        }

	//        #endregion // GetCalcValueHelper

	//        #region GetTable

	//        private WorksheetTable GetTable(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            if (_workbookFileName != null)
	//                return null;

	//            if (_tableName == null)
	//            {
	//                if (formulaOwnerRow != null)
	//                    return formulaOwnerRow.GetCellAssociatedTableInternal(formulaOwnerColumnIndex);
	//                else
	//                    return null;
	//            }

	//            if (workbook != null)
	//                return workbook.GetTable(_tableName);

	//            return null;
	//        }

	//        #endregion // GetTable

	//        #region EscapeColumnName

	//        private static string EscapeColumnName(string columnName)
	//        {
	//            if (columnName == null)
	//                return null;

	//            for (int i = 0; i < columnName.Length; i++)
	//            {
	//                char currentChar = columnName[i];
	//                if (FormulaParser.IsEscapeTableColumnCharacter(currentChar))
	//                {
	//                    StringBuilder columnNameBuilder = new StringBuilder(columnName.Substring(0, i));
	//                    columnNameBuilder.Append('\'');
	//                    columnNameBuilder.Append(currentChar);

	//                    for (int j = i + 1; j < columnName.Length; j++)
	//                    {
	//                        currentChar = columnName[j];
	//                        if (FormulaParser.IsEscapeTableColumnCharacter(currentChar))
	//                            columnNameBuilder.Append('\'');

	//                        columnNameBuilder.Append(currentChar);
	//                    }

	//                    return columnNameBuilder.ToString();
	//                }
	//            }

	//            return columnName;
	//        }

	//        #endregion // EscapeColumnName

	//        #region GetStructuredTableReferenceString

	//        // MD 4/9/12 - TFS101506
	//        //private string GetStructuredTableReferenceString()
	//        private string GetStructuredTableReferenceString(CultureInfo culture)
	//        {
	//            if (_innerReference != null)
	//            {
	//                // MD 4/9/12 - TFS101506
	//                //return _innerReference.ToString();
	//                return _innerReference.ToString(culture);
	//            }

	//            if (_keyword.HasValue)
	//                return FormulaParser.GetKeywordText(_keyword.Value);

	//            if (_simpleColumnName != null)
	//                return StructuredTableReference.EscapeColumnName(_simpleColumnName);

	//            Utilities.DebugFail("This is unexpected.");
	//            return string.Empty;
	//        }

	//        #endregion // GetStructuredTableReferenceString

	//        #region GetWrapInBrackets

	//        private static string GetWrapInBrackets(string simpleColumnName)
	//        {
	//            return '[' + simpleColumnName + ']';
	//        }

	//        #endregion // GetWrapInBrackets

	//        #region OnTableColumnsRenamed

	//        internal void OnTableColumnsRenamed(WorksheetTable table, List<KeyValuePair<WorksheetTableColumn, string>> changedColumnNames)
	//        {
	//            if (_workbookFileName == null &&
	//                // MD 4/9/12 - TFS101506
	//                //String.Equals(table.Name, _tableName, StringComparison.CurrentCultureIgnoreCase))
	//                String.Compare(table.Name, _tableName, table.Culture, CompareOptions.IgnoreCase) == 0)
	//            {
	//                for (int i = 0; i < changedColumnNames.Count; i++)
	//                {
	//                    KeyValuePair<WorksheetTableColumn, string> namedChangePair = changedColumnNames[i];

	//                    if (_simpleColumnName != null &&
	//                        // MD 4/9/12 - TFS101506
	//                        //String.Equals(namedChangePair.Value, _simpleColumnName, StringComparison.CurrentCultureIgnoreCase))
	//                        String.Compare(namedChangePair.Value, _simpleColumnName, table.Culture, CompareOptions.IgnoreCase) == 0)
	//                    {
	//                        _simpleColumnName = namedChangePair.Key.Name;
	//                    }

	//                    if (_innerReference != null)
	//                    {
	//                        // MD 4/9/12 - TFS101506
	//                        //_innerReference.OnTableColumnRenamed(namedChangePair.Value, namedChangePair.Key.Name);
	//                        _innerReference.OnTableColumnRenamed(table, namedChangePair.Value, namedChangePair.Key.Name);
	//                    }
	//                }
	//            }
	//        }

	//        #endregion // OnTableColumnsRenamed

	//        #endregion // Methods

	//        #region Properties

	//        #region HasThisRowKeyword

	//        public bool HasThisRowKeyword
	//        {
	//            get
	//            {
	//                if (_keyword == StructuredTableReferenceKeywordType.ThisRow)
	//                    return true;

	//                if (_innerReference != null)
	//                    return _innerReference.HasThisRowKeyword;

	//                return false;
	//            }
	//        }

	//        #endregion // HasThisRowKeyword

	//        #region InnerStructuredReference

	//        public InnerReference InnerStructuredReference
	//        {
	//            get { return _innerReference; }
	//        }

	//        #endregion // InnerStructuredReference

	//        #region SimpleColumnName

	//        public string SimpleColumnName
	//        {
	//            get { return _simpleColumnName; }
	//        }

	//        #endregion // SimpleColumnName

	//        #region TableName

	//        public string TableName
	//        {
	//            get { return _tableName; }
	//        }

	//        #endregion // TableName

	//        #endregion // Properties


	//        #region InnerReference class

	//#if DEBUG
	//        /// <summary>
	//        /// Represents a structured reference nested inside brackets (used when more than one set of brackets is used in the
	//        /// entire structured reference).
	//        /// </summary> 
	//#endif
	//        public class InnerReference
	//        {
	//            #region Member Variables

	//            private string _firstColumnName;
	//            private StructuredTableReferenceKeywordType? _firstKeyword;
	//            private bool _hasSpaceAfter;
	//            private bool _hasSpaceAfterFirstKeyword;
	//            private bool _hasSpaceAfterKeywordList;
	//            private bool _hasSpaceBefore;
	//            private string _lastColumnName;
	//            private StructuredTableReferenceKeywordType? _lastKeyword;

	//            #endregion // Member Variables

	//            #region Constructor

	//            public InnerReference(
	//                StructuredTableReferenceKeywordType firstKeyword,
	//                bool hasSpaceAfterFirstKeyword,
	//                StructuredTableReferenceKeywordType? lastKeyword)
	//            {
	//                _firstKeyword = firstKeyword;
	//                _hasSpaceAfterFirstKeyword = hasSpaceAfterFirstKeyword;
	//                _lastKeyword = lastKeyword;
	//            }

	//            public InnerReference(
	//                StructuredTableReferenceKeywordType firstKeyword,
	//                bool hasSpaceAfterFirstKeyword,
	//                StructuredTableReferenceKeywordType? lastKeyword,
	//                bool hasSpaceAfterKeywordList,
	//                string firstColumnName,
	//                string lastColumnName)
	//                : this(firstKeyword, hasSpaceAfterFirstKeyword, lastKeyword)
	//            {
	//                _hasSpaceAfterKeywordList = hasSpaceAfterKeywordList;

	//                Debug.Assert(firstColumnName != null, "The first column must be set here.");
	//                _firstColumnName = firstColumnName;
	//                _lastColumnName = lastColumnName;
	//            }

	//            public InnerReference(
	//                string firstColumnName,
	//                string lastColumnName)
	//            {
	//                Debug.Assert(firstColumnName != null, "The first column must be set here.");
	//                _firstColumnName = firstColumnName;
	//                _lastColumnName = lastColumnName;
	//            }

	//            #endregion // Constructor

	//            #region Base Class Overrides

	//            #region Equals

	//            public override bool Equals(object obj)
	//            {
	//                InnerReference other = obj as InnerReference;
	//                return other != null &&
	//                    _firstKeyword == other._firstKeyword &&
	//                    _lastKeyword == other._lastKeyword &&
	//                    _firstColumnName == other._firstColumnName &&
	//                    _lastColumnName == other._lastColumnName &&
	//                    _hasSpaceAfterFirstKeyword == other._hasSpaceAfterFirstKeyword &&
	//                    _hasSpaceAfterKeywordList == other._hasSpaceAfterKeywordList &&
	//                    _hasSpaceAfter == other._hasSpaceAfter &&
	//                    _hasSpaceBefore == other._hasSpaceBefore;
	//            }

	//            #endregion // Equals

	//            #region GetHashCode

	//            public override int GetHashCode()
	//            {
	//                return _firstColumnName.GetHashCode() ^ _lastColumnName.GetHashCode();
	//            }

	//            #endregion // GetHashCode

	//            #region ToString

	//            public override string ToString()
	//            {
	//                // MD 4/9/12 - TFS101506
	//                Utilities.DebugFail("The overload of ToString which takes a culture should be used.");
	//                return this.ToString(CultureInfo.CurrentCulture);
	//            }

	//            // MD 4/9/12 - TFS101506
	//            public string ToString(CultureInfo culture)
	//            {
	//                string firstColumnNameEscaped = StructuredTableReference.EscapeColumnName(_firstColumnName);
	//                string lastColumnNameEscaped = StructuredTableReference.EscapeColumnName(_lastColumnName);

	//                if (firstColumnNameEscaped != null &&
	//                    lastColumnNameEscaped == null && _firstKeyword.HasValue == false && _lastKeyword.HasValue == false)
	//                {
	//                    return firstColumnNameEscaped;
	//                }

	//                // MD 4/9/12 - TFS101506
	//                //string keywordSeparator = FormulaParser.GetUnionOperatorResolved();
	//                string keywordSeparator = FormulaParser.GetUnionOperatorResolved(culture);

	//                string result = string.Empty;
	//                if (this.HasSpaceBefore)
	//                    result += " ";

	//                if (_firstKeyword.HasValue)
	//                {
	//                    result += StructuredTableReference.GetWrapInBrackets(FormulaParser.GetKeywordText(_firstKeyword.Value));

	//                    if (_lastKeyword.HasValue)
	//                    {
	//                        result += keywordSeparator;

	//                        if (_hasSpaceAfterFirstKeyword)
	//                            result += " ";

	//                        result += StructuredTableReference.GetWrapInBrackets(FormulaParser.GetKeywordText(_lastKeyword.Value));
	//                    }
	//                }

	//                if (firstColumnNameEscaped != null)
	//                {
	//                    if (_firstKeyword.HasValue)
	//                    {
	//                        result += keywordSeparator;

	//                        if (_hasSpaceAfterKeywordList)
	//                            result += " ";
	//                    }

	//                    result += StructuredTableReference.GetWrapInBrackets(firstColumnNameEscaped);

	//                    if (lastColumnNameEscaped != null)
	//                        result += ":" + StructuredTableReference.GetWrapInBrackets(lastColumnNameEscaped);
	//                }

	//                if (this.HasSpaceAfter)
	//                    result += " ";

	//                return result;
	//            }

	//            #endregion // ToString

	//            #endregion // Base Class Overrides

	//            #region Methods

	//            #region GetCalcReference

	//            public ExcelRefBase GetCalcReference(WorksheetTable table, WorksheetRow formulaOwnerRow)
	//            {
	//                if (_lastColumnName != null)
	//                {
	//                    WorksheetTableColumn firstColumn = table.Columns[_firstColumnName];
	//                    WorksheetTableColumn lastColumn = table.Columns[_lastColumnName];

	//                    if (firstColumn != null && lastColumn != null)
	//                        return new TableColumnRangeCalcReference(formulaOwnerRow, firstColumn, lastColumn, _firstKeyword, _lastKeyword);
	//                }
	//                else if (_firstColumnName != null)
	//                {
	//                    WorksheetTableColumn firstColumn = table.Columns[_firstColumnName];
	//                    if (firstColumn != null)
	//                        return new TableColumnCalcReference(formulaOwnerRow, firstColumn, _firstKeyword, _lastKeyword);
	//                }
	//                else
	//                {
	//                    return new TableCalcReference(formulaOwnerRow, table, _firstKeyword, _lastKeyword);
	//                }

	//                return ExcelReferenceError.Instance;
	//            }

	//            #endregion // GetCalcReference

	//            #region OnTableColumnRenamed

	//            // MD 4/9/12 - TFS101506
	//            //public void OnTableColumnRenamed(string oldName, string newName)
	//            public void OnTableColumnRenamed(WorksheetTable table, string oldName, string newName)
	//            {
	//                if (_firstColumnName != null &&
	//                    // MD 4/9/12 - TFS101506
	//                    //String.Equals(oldName, _firstColumnName, StringComparison.CurrentCultureIgnoreCase))
	//                    String.Compare(oldName, _firstColumnName, table.Culture, CompareOptions.IgnoreCase) == 0)
	//                {
	//                    _firstColumnName = newName;
	//                }

	//                if (_lastColumnName != null &&
	//                    // MD 4/9/12 - TFS101506
	//                    //String.Equals(oldName, _lastColumnName, StringComparison.CurrentCultureIgnoreCase))
	//                    String.Compare(oldName, _lastColumnName, table.Culture, CompareOptions.IgnoreCase) == 0)
	//                {
	//                    _lastColumnName = newName;
	//                }
	//            }

	//            #endregion // OnTableColumnRenamed

	//            #endregion // Methods

	//            #region Properties

	//            #region FirstColumnName

	//            public string FirstColumnName
	//            {
	//                get { return _firstColumnName; }
	//            }

	//            #endregion // FirstColumnName

	//            #region HasThisRowKeyword

	//            public bool HasThisRowKeyword
	//            {
	//                get
	//                {
	//                    return
	//                        _firstKeyword == StructuredTableReferenceKeywordType.ThisRow ||
	//                        _lastKeyword == StructuredTableReferenceKeywordType.ThisRow;
	//                }
	//            }

	//            #endregion // HasThisRowKeyword

	//            #region HasSpaceAfter

	//            public bool HasSpaceAfter
	//            {
	//                get { return _hasSpaceAfter; }
	//                set { _hasSpaceAfter = value; }
	//            }

	//            #endregion // HasSpaceAfter

	//            #region HasSpaceBefore

	//            public bool HasSpaceBefore
	//            {
	//                get { return _hasSpaceBefore; }
	//                set { _hasSpaceBefore = value; }
	//            }

	//            #endregion // HasSpaceBefore

	//            #region IsSimpleColumnReference

	//            public bool IsSimpleColumnReference
	//            {
	//                get
	//                {
	//                    return
	//                        _firstColumnName != null &&
	//                        _lastColumnName == null &&
	//                        _firstKeyword == null &&
	//                        _lastKeyword == null &&
	//                        _hasSpaceAfterFirstKeyword == false &&
	//                        _hasSpaceAfterKeywordList == false &&
	//                        _hasSpaceAfter == false &&
	//                        _hasSpaceBefore == false;
	//                }
	//            }

	//            #endregion // IsSimpleColumnReference

	//            #region LastColumnName

	//            public string LastColumnName
	//            {
	//                get { return _lastColumnName; }
	//            }

	//            #endregion // LastColumnName

	//            #endregion // Properties
	//        }

	//        #endregion // InnerReference class
	//    }

	#endregion // Old Code
	internal class StructuredTableReference : ReferenceToken
	{
		#region Member Variables

		private InnerReference _innerReference;
		private StructuredTableReferenceKeywordType? _keyword;
		private string _simpleColumnName;

		private NamedReferenceBase _tableReference;
		private WorkbookReferenceBase _workbookReference;

		#endregion // Member Variables

		#region Constructor

		private StructuredTableReference(string workbookFileName, string tableName, WorkbookFormat currentFormat)
			: base(TokenClass.Reference)
		{
			_workbookReference = new WorkbookReferenceUnconnected(workbookFileName);
			_tableReference = new NamedReferenceUnconnected(tableName, _workbookReference, false, currentFormat);
		}

		public StructuredTableReference(string workbookFileName, string tableName, StructuredTableReferenceKeywordType keyword, WorkbookFormat currentFormat)
			: this(workbookFileName, tableName, currentFormat)
		{
			_keyword = keyword;
		}

		public StructuredTableReference(string workbookFileName, string tableName, string simpleColumnName, WorkbookFormat currentFormat)
			: this(workbookFileName, tableName, currentFormat)
		{
			_simpleColumnName = simpleColumnName;
		}

		public StructuredTableReference(string workbookFileName, string tableName, InnerReference innerReference, WorkbookFormat currentFormat)
			: this(workbookFileName, tableName, currentFormat)
		{
			_innerReference = innerReference;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region ConnectReferences

		public override void ConnectReferences(FormulaContext context)
		{
			_workbookReference = _workbookReference.Connect(context);

			NamedReferenceBase temp = this.GetTable(context);
			if (temp != null)
				_tableReference = temp;
		}

		#endregion // ConnectReferences

		#region ConvertTableReferencesToRanges

		public override void ConvertTableReferencesToRanges(FormulaContext context, WorksheetTable table, out FormulaToken replacementToken)
		{
			replacementToken = null;

			if (context.Workbook != null && 
				_workbookReference == context.Workbook.CurrentWorkbookReference && 
				table == _tableReference)
			{
				TableCalcReferenceBase reference = this.GetCalcValueHelper(table, context.OwningRow) as TableCalcReferenceBase;
				if (reference == null)
					return;

				WorksheetRegion region = reference.Region;
				if (region == null)
					return;

				replacementToken = new Area3DToken(
					_workbookReference.GetWorksheetReference(table.Worksheet.Name, null),
					new CellAddressRange(region),
					this.TokenClass);
			}
		}

		#endregion // ConvertTableReferencesToRanges

		#region DisconnectReferences

		public override void DisconnectReferences()
		{
			_workbookReference = _workbookReference.Disconnect();
			_tableReference = new NamedReferenceUnconnected(_tableReference.Name, _workbookReference, false, _tableReference.CurrentFormat);
		}

		#endregion // DisconnectReferences

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			return this.GetCalcValueHelper(this.GetTable(context), context.OwningRow);
		}

		#endregion // GetCalcValue

		#region InitializeSerializationManager

		public override void InitializeSerializationManager(WorkbookSerializationManager manager, FormulaContext context)
		{
			manager.RetainWorkbookReference(_workbookReference);
		}

		#endregion // InitializeSerializationManager

		#region Is3DReference

		public override bool Is3DReference
		{
			get { return _workbookReference != null; }
		}

		#endregion // Is3DReference

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			StructuredTableReference comparisonStructuredTableReference = (StructuredTableReference)comparisonToken;

			if (_keyword != comparisonStructuredTableReference._keyword)
				return false;

			if (_simpleColumnName != comparisonStructuredTableReference._simpleColumnName)
				return false;

			if (Object.Equals(_tableReference, comparisonStructuredTableReference._tableReference) == false)
				return false;

			if (Object.Equals(_workbookReference, comparisonStructuredTableReference._workbookReference) == false)
				return false;

			if (Object.Equals(_innerReference, comparisonStructuredTableReference._innerReference) == false)
				return false;

			return true;
		}

		#endregion // IsEquivalentTo

		#region OnNamedReferenceRemoved

		public override bool OnNamedReferenceRemoved(NamedReferenceBase namedReference)
		{
			if (_tableReference == namedReference)
			{
				this.DisconnectReferences();
				return true;
			}

			return false;
		}

		#endregion // OnNamedReferenceRemoved

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnTableResizing

		public override void OnTableResizing(WorksheetTable table, List<WorksheetTableColumn> columnsBeingRemoved,
			out bool tableReferenced,
			out FormulaToken replacementToken)
		{
			tableReferenced = (table == _tableReference);
			replacementToken = null;

			if (tableReferenced == false)
				return;

			if (_simpleColumnName == null && _innerReference == null)
				return;

			CultureInfo culture = table.Culture;

			bool referencedColumnRemoved = false;
			for (int i = 0; i < columnsBeingRemoved.Count; i++)
			{
				WorksheetTableColumn removedColumn = columnsBeingRemoved[i];

				if (_simpleColumnName != null &&
					String.Compare(removedColumn.Name, _simpleColumnName, culture, CompareOptions.IgnoreCase) == 0)
				{
					referencedColumnRemoved = true;
					break;
				}

				if (_innerReference != null)
				{
					if (_innerReference.FirstColumnName != null &&
						String.Compare(removedColumn.Name, _innerReference.FirstColumnName, culture, CompareOptions.IgnoreCase) == 0)
					{
						referencedColumnRemoved = true;
						break;
					}

					if (_innerReference.LastColumnName != null &&
						String.Compare(removedColumn.Name, _innerReference.LastColumnName, culture, CompareOptions.IgnoreCase) == 0)
					{
						referencedColumnRemoved = true;
						break;
					}
				}
			}

			if (referencedColumnRemoved)
				replacementToken = new RefErrToken(this.TokenClass);
		}

		#endregion // OnTableResizing

		#region SetWorkbookReference

		public override void SetWorkbookReference(WorkbookReferenceBase workbookReference)
		{
			_workbookReference = workbookReference;
		}

		#endregion // SetWorkbookReference

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			string tableName = _tableReference.Name;

			// If the table name is specified, but the formula is within the referenced table, remove the name.
			if (tableName != null &&
				context.Formula != null &&
				context.Formula.OwningCellRow != null)
			{
				WorksheetTable table = context.Formula.OwningCellRow.GetCellAssociatedTable(context.Formula.OwningCellColumnIndex);

				if (table != null && String.Compare(table.Name, tableName, context.Culture, CompareOptions.IgnoreCase) == 0)
					tableName = null;
			}

			string referenceString = _workbookReference.GetWorkbookReferenceString(externalReferences);
			return
				referenceString +
				tableName +
				StructuredTableReference.GetWrapInBrackets(this.GetStructuredTableReferenceString(context.Culture));
		}

		#endregion // ToString

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.StructuredTableReferenceA;
					case TokenClass.Reference: return Token.StructuredTableReferenceR;
					case TokenClass.Value: return Token.StructuredTableReferenceV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.StructuredTableReferenceV;
				}
			}
		}

		#endregion // Token

		#endregion // Base Class Overrides

		#region Methods

		#region GetCalcValueHelper

		private object GetCalcValueHelper(WorksheetTable table, WorksheetRow formulaOwnerRow)
		{
			if (table == null)
				return ErrorValue.InvalidCellReference.ToCalcErrorValue();

			if (_innerReference != null)
			{
				return _innerReference.GetCalcReference(table, formulaOwnerRow);
			}
			else if (_keyword.HasValue)
			{
				return new TableCalcReference(formulaOwnerRow, table, _keyword, null);
			}
			else if (_simpleColumnName != null)
			{
				WorksheetTableColumn column = table.Columns[_simpleColumnName];
				if (column != null)
					return new TableColumnCalcReference(formulaOwnerRow, column, null, null);
			}

			return ErrorValue.InvalidCellReference.ToCalcErrorValue();
		}

		#endregion // GetCalcValueHelper

		#region GetTable

		private WorksheetTable GetTable(FormulaContext context)
		{
			if (_workbookReference.IsExternal)
				return null;

			if (_tableReference.Name == null)
			{
				WorksheetRow owningRow = context.OwningRow;
				if (owningRow != null)
					return owningRow.GetCellAssociatedTableInternal(context.OwningCellAddress.ColumnIndex);
				else
					return null;
			}

			if (context.Workbook != null)
				return context.Workbook.GetTable(_tableReference.Name);

			return null;
		}

		#endregion // GetTable

		#region EscapeColumnName

		private static string EscapeColumnName(string columnName)
		{
			if (columnName == null)
				return null;

			for (int i = 0; i < columnName.Length; i++)
			{
				char currentChar = columnName[i];
				if (FormulaParser.IsEscapeTableColumnCharacter(currentChar))
				{
					StringBuilder columnNameBuilder = new StringBuilder(columnName.Substring(0, i));
					columnNameBuilder.Append('\'');
					columnNameBuilder.Append(currentChar);

					for (int j = i + 1; j < columnName.Length; j++)
					{
						currentChar = columnName[j];
						if (FormulaParser.IsEscapeTableColumnCharacter(currentChar))
							columnNameBuilder.Append('\'');

						columnNameBuilder.Append(currentChar);
					}

					return columnNameBuilder.ToString();
				}
			}

			return columnName;
		}

		#endregion // EscapeColumnName

		#region GetStructuredTableReferenceString

		// MD 4/9/12 - TFS101506
		//private string GetStructuredTableReferenceString()
		private string GetStructuredTableReferenceString(CultureInfo culture)
		{
			if (_innerReference != null)
			{
				// MD 4/9/12 - TFS101506
				//return _innerReference.ToString();
				return _innerReference.ToString(culture);
			}

			if (_keyword.HasValue)
				return FormulaParser.GetKeywordText(_keyword.Value);

			if (_simpleColumnName != null)
				return StructuredTableReference.EscapeColumnName(_simpleColumnName);

			Utilities.DebugFail("This is unexpected.");
			return string.Empty;
		}

		#endregion // GetStructuredTableReferenceString

		#region GetWrapInBrackets

		private static string GetWrapInBrackets(string simpleColumnName)
		{
			return '[' + simpleColumnName + ']';
		}

		#endregion // GetWrapInBrackets

		#region OnTableColumnsRenamed

		internal void OnTableColumnsRenamed(WorksheetTable table, List<KeyValuePair<WorksheetTableColumn, string>> changedColumnNames)
		{
			if (_workbookReference.IsExternal == false &&
				String.Compare(table.Name, _tableReference.Name, table.Culture, CompareOptions.IgnoreCase) == 0)
			{
				for (int i = 0; i < changedColumnNames.Count; i++)
				{
					KeyValuePair<WorksheetTableColumn, string> namedChangePair = changedColumnNames[i];

					if (_simpleColumnName != null &&
						// MD 4/9/12 - TFS101506
						//String.Equals(namedChangePair.Value, _simpleColumnName, StringComparison.CurrentCultureIgnoreCase))
						String.Compare(namedChangePair.Value, _simpleColumnName, table.Culture, CompareOptions.IgnoreCase) == 0)
					{
						_simpleColumnName = namedChangePair.Key.Name;
					}

					if (_innerReference != null)
					{
						// MD 4/9/12 - TFS101506
						//_innerReference.OnTableColumnRenamed(namedChangePair.Value, namedChangePair.Key.Name);
						_innerReference.OnTableColumnRenamed(table, namedChangePair.Value, namedChangePair.Key.Name);
					}
				}
			}
		}

		#endregion // OnTableColumnsRenamed

		#endregion // Methods

		#region Properties

		#region HasThisRowKeyword

		public bool HasThisRowKeyword
		{
			get
			{
				if (_keyword == StructuredTableReferenceKeywordType.ThisRow)
					return true;

				if (_innerReference != null)
					return _innerReference.HasThisRowKeyword;

				return false;
			}
		}

		#endregion // HasThisRowKeyword

		#region InnerStructuredReference

		public InnerReference InnerStructuredReference
		{
			get { return _innerReference; }
		}

		#endregion // InnerStructuredReference

		#region SimpleColumnName

		public string SimpleColumnName
		{
			get { return _simpleColumnName; }
		}

		#endregion // SimpleColumnName

		#region TableName

		public string TableName
		{
			get { return _tableReference.Name; }
		}

		#endregion // TableName

		#endregion // Properties


		#region InnerReference class







		public class InnerReference
		{
			#region Member Variables

			private string _firstColumnName;
			private StructuredTableReferenceKeywordType? _firstKeyword;
			private bool _hasSpaceAfter;
			private bool _hasSpaceAfterFirstKeyword;
			private bool _hasSpaceAfterKeywordList;
			private bool _hasSpaceBefore;
			private string _lastColumnName;
			private StructuredTableReferenceKeywordType? _lastKeyword;

			#endregion // Member Variables

			#region Constructor

			public InnerReference(
				StructuredTableReferenceKeywordType firstKeyword,
				bool hasSpaceAfterFirstKeyword,
				StructuredTableReferenceKeywordType? lastKeyword)
			{
				_firstKeyword = firstKeyword;
				_hasSpaceAfterFirstKeyword = hasSpaceAfterFirstKeyword;
				_lastKeyword = lastKeyword;
			}

			public InnerReference(
				StructuredTableReferenceKeywordType firstKeyword,
				bool hasSpaceAfterFirstKeyword,
				StructuredTableReferenceKeywordType? lastKeyword,
				bool hasSpaceAfterKeywordList,
				string firstColumnName,
				string lastColumnName)
				: this(firstKeyword, hasSpaceAfterFirstKeyword, lastKeyword)
			{
				_hasSpaceAfterKeywordList = hasSpaceAfterKeywordList;

				Debug.Assert(firstColumnName != null, "The first column must be set here.");
				_firstColumnName = firstColumnName;
				_lastColumnName = lastColumnName;
			}

			public InnerReference(
				string firstColumnName,
				string lastColumnName)
			{
				Debug.Assert(firstColumnName != null, "The first column must be set here.");
				_firstColumnName = firstColumnName;
				_lastColumnName = lastColumnName;
			}

			#endregion // Constructor

			#region Base Class Overrides

			#region Equals

			public override bool Equals(object obj)
			{
				InnerReference other = obj as InnerReference;
				return other != null &&
					_firstKeyword == other._firstKeyword &&
					_lastKeyword == other._lastKeyword &&
					_firstColumnName == other._firstColumnName &&
					_lastColumnName == other._lastColumnName &&
					_hasSpaceAfterFirstKeyword == other._hasSpaceAfterFirstKeyword &&
					_hasSpaceAfterKeywordList == other._hasSpaceAfterKeywordList &&
					_hasSpaceAfter == other._hasSpaceAfter &&
					_hasSpaceBefore == other._hasSpaceBefore;
			}

			#endregion // Equals

			#region GetHashCode

			public override int GetHashCode()
			{
				return _firstColumnName.GetHashCode() ^ _lastColumnName.GetHashCode();
			}

			#endregion // GetHashCode

			#region ToString

			public override string ToString()
			{
				// MD 4/9/12 - TFS101506
				Utilities.DebugFail("The overload of ToString which takes a culture should be used.");
				return this.ToString(CultureInfo.CurrentCulture);
			}

			// MD 4/9/12 - TFS101506
			public string ToString(CultureInfo culture)
			{
				string firstColumnNameEscaped = StructuredTableReference.EscapeColumnName(_firstColumnName);
				string lastColumnNameEscaped = StructuredTableReference.EscapeColumnName(_lastColumnName);

				if (firstColumnNameEscaped != null &&
					lastColumnNameEscaped == null && _firstKeyword.HasValue == false && _lastKeyword.HasValue == false)
				{
					return firstColumnNameEscaped;
				}

				// MD 4/9/12 - TFS101506
				//string keywordSeparator = FormulaParser.GetUnionOperatorResolved();
				string keywordSeparator = FormulaParser.GetUnionOperatorResolved(culture);

				string result = string.Empty;
				if (this.HasSpaceBefore)
					result += " ";

				if (_firstKeyword.HasValue)
				{
					result += StructuredTableReference.GetWrapInBrackets(FormulaParser.GetKeywordText(_firstKeyword.Value));

					if (_lastKeyword.HasValue)
					{
						result += keywordSeparator;

						if (_hasSpaceAfterFirstKeyword)
							result += " ";

						result += StructuredTableReference.GetWrapInBrackets(FormulaParser.GetKeywordText(_lastKeyword.Value));
					}
				}

				if (firstColumnNameEscaped != null)
				{
					if (_firstKeyword.HasValue)
					{
						result += keywordSeparator;

						if (_hasSpaceAfterKeywordList)
							result += " ";
					}

					result += StructuredTableReference.GetWrapInBrackets(firstColumnNameEscaped);

					if (lastColumnNameEscaped != null)
						result += ":" + StructuredTableReference.GetWrapInBrackets(lastColumnNameEscaped);
				}

				if (this.HasSpaceAfter)
					result += " ";

				return result;
			}

			#endregion // ToString

			#endregion // Base Class Overrides

			#region Methods

			#region GetCalcReference

			public ExcelRefBase GetCalcReference(WorksheetTable table, WorksheetRow formulaOwnerRow)
			{
				if (_lastColumnName != null)
				{
					WorksheetTableColumn firstColumn = table.Columns[_firstColumnName];
					WorksheetTableColumn lastColumn = table.Columns[_lastColumnName];

					if (firstColumn != null && lastColumn != null)
						return new TableColumnRangeCalcReference(formulaOwnerRow, firstColumn, lastColumn, _firstKeyword, _lastKeyword);
				}
				else if (_firstColumnName != null)
				{
					WorksheetTableColumn firstColumn = table.Columns[_firstColumnName];
					if (firstColumn != null)
						return new TableColumnCalcReference(formulaOwnerRow, firstColumn, _firstKeyword, _lastKeyword);
				}
				else
				{
					return new TableCalcReference(formulaOwnerRow, table, _firstKeyword, _lastKeyword);
				}

				return ExcelReferenceError.Instance;
			}

			#endregion // GetCalcReference

			#region OnTableColumnRenamed

			// MD 4/9/12 - TFS101506
			//public void OnTableColumnRenamed(string oldName, string newName)
			public void OnTableColumnRenamed(WorksheetTable table, string oldName, string newName)
			{
				if (_firstColumnName != null &&
					// MD 4/9/12 - TFS101506
					//String.Equals(oldName, _firstColumnName, StringComparison.CurrentCultureIgnoreCase))
					String.Compare(oldName, _firstColumnName, table.Culture, CompareOptions.IgnoreCase) == 0)
				{
					_firstColumnName = newName;
				}

				if (_lastColumnName != null &&
					// MD 4/9/12 - TFS101506
					//String.Equals(oldName, _lastColumnName, StringComparison.CurrentCultureIgnoreCase))
					String.Compare(oldName, _lastColumnName, table.Culture, CompareOptions.IgnoreCase) == 0)
				{
					_lastColumnName = newName;
				}
			}

			#endregion // OnTableColumnRenamed

			#endregion // Methods

			#region Properties

			#region FirstColumnName

			public string FirstColumnName
			{
				get { return _firstColumnName; }
			}

			#endregion // FirstColumnName

			#region HasThisRowKeyword

			public bool HasThisRowKeyword
			{
				get
				{
					return
						_firstKeyword == StructuredTableReferenceKeywordType.ThisRow ||
						_lastKeyword == StructuredTableReferenceKeywordType.ThisRow;
				}
			}

			#endregion // HasThisRowKeyword

			#region HasSpaceAfter

			public bool HasSpaceAfter
			{
				get { return _hasSpaceAfter; }
				set { _hasSpaceAfter = value; }
			}

			#endregion // HasSpaceAfter

			#region HasSpaceBefore

			public bool HasSpaceBefore
			{
				get { return _hasSpaceBefore; }
				set { _hasSpaceBefore = value; }
			}

			#endregion // HasSpaceBefore

			#region IsSimpleColumnReference

			public bool IsSimpleColumnReference
			{
				get
				{
					return
						_firstColumnName != null &&
						_lastColumnName == null &&
						_firstKeyword == null &&
						_lastKeyword == null &&
						_hasSpaceAfterFirstKeyword == false &&
						_hasSpaceAfterKeywordList == false &&
						_hasSpaceAfter == false &&
						_hasSpaceBefore == false;
				}
			}

			#endregion // IsSimpleColumnReference

			#region LastColumnName

			public string LastColumnName
			{
				get { return _lastColumnName; }
			}

			#endregion // LastColumnName

			#endregion // Properties
		}

		#endregion // InnerReference class
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