namespace Dev2.Common.Interfaces
{
    public class IntellisenseStringResult : IIntellisenseStringResult
    {
        private readonly string _result;
        private readonly int _caretPosition;

        public IntellisenseStringResult(string result, int caretPosition)
        {
            _result = result;
            _caretPosition = caretPosition;
        }

        #region Implementation of IIntellisenseStringResult

        public string Result => _result;
        public int CaretPosition => _caretPosition;

        #endregion
    }
}