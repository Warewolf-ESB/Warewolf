using System.Collections.Generic;
using System.Text;

namespace Dev2.Common {
    internal class Dev2Tokenizer : IDev2Tokenizer{

        private readonly char[] _tokenParts;
        private readonly IList<IDev2SplitOp> _ops;
        private readonly bool _isReversed;

        private int _opPointer;
        private int _startIdx;
        private bool _hasMoreOps;

        internal Dev2Tokenizer(string candiateString, IList<IDev2SplitOp> ops, bool reversed) {
            _tokenParts = candiateString.ToCharArray();
            _ops = ops;
            _opPointer = 0;
            _hasMoreOps = true;
            _isReversed = reversed;

            if (!_isReversed) {
                _startIdx = 0;
            }
            else {
                _startIdx = _tokenParts.Length - 1;
            }
        }

        #region Private Method

        private void MoveOpPointer() {

            _opPointer++;

            if (_opPointer >= _ops.Count) {
                _opPointer = 0;
            }
        }

        private void MoveStartIndex(int newOffSet) {
            if (!_isReversed) {
                _startIdx += newOffSet;
            }
            else {
                _startIdx -= newOffSet;
            }
        }

        private bool HasMoreData() {
            bool result = false;

            if (!_isReversed) {
                result = (_startIdx < _tokenParts.Length);
            }
            else {
                result = (_startIdx >= 0);
            }

            return result;
        }

        private string RemainderToString() {
            StringBuilder result = new StringBuilder();

            for (int i = _startIdx; i < _tokenParts.Length; i++) {
                result.Append(_tokenParts[i]);
            }

            MoveStartIndex(result.Length);
            _hasMoreOps = false;

            return result.ToString();
        }

        #endregion

        public bool HasMoreOps() {
            return _hasMoreOps;
        }

        public string NextToken() {
            string result = string.Empty;

            try {
                result = _ops[_opPointer].ExecuteOperation(_tokenParts, _startIdx, _isReversed);
                MoveStartIndex( (result.Length + _ops[_opPointer].OpLength()) );
                MoveOpPointer();
                // check to see if there is data to fetch still?
                _hasMoreOps = (!_ops[_opPointer].IsFinalOp() & HasMoreData());
            }
            catch {
                // error, return remaining portion of the string
                result = RemainderToString();
                throw;
            }

            return result;
        }

    }
}
