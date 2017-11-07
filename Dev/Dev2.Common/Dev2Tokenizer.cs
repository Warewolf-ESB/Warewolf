/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Dev2.Common
{
    internal class Dev2Tokenizer : IDev2Tokenizer, IDisposable
    {
        private readonly CharEnumerator _charEnumerator;
        private readonly bool _isReversed;
        private readonly int _masterLen;
        private readonly IList<IDev2SplitOp> _ops;
        private readonly char[] _tokenParts;

        private readonly bool _useEnumerator;

        private bool _disposing;
        private bool _hasMoreOps;
        private int _opPointer;
        private int _startIdx;

        internal Dev2Tokenizer(string candiateString, IList<IDev2SplitOp> ops, bool reversed)
        {
            _ops = ops;
            _isReversed = reversed;
            _useEnumerator = CanUseEnumerator();
            _masterLen = candiateString.Length;
            
            if (!_useEnumerator)
            {
                _tokenParts = candiateString.ToCharArray();
            }
            else
            {
                _charEnumerator = candiateString.GetEnumerator();
            }

            _opPointer = 0;
            _hasMoreOps = true;

            _startIdx = !_isReversed ? 0 : _tokenParts.Length - 1;
        }

        #region Private Method
        
        private bool CanUseEnumerator()
        {
            bool result = _ops != null && _ops?.Count(op => op.CanUseEnumerator(_isReversed)) == _ops.Count;
            return result;
        }
        
        private void MoveOpPointer()
        {
            _opPointer++;

            if (_opPointer >= _ops.Count)
            {
                _opPointer = 0;
            }
        }
        
        private void MoveStartIndex(int newOffSet)
        {
            if (!_isReversed)
            {
                _startIdx += newOffSet;
            }
            else
            {
                _startIdx -= newOffSet;
            }
        }
        
        private bool HasMoreData()
        {
            bool result;

            result = !_isReversed ? _startIdx < _masterLen : _startIdx >= 0;

            return result;
        }
        
        #endregion Private Method
        
        public bool HasMoreOps()
        {
            return _hasMoreOps;
        }
        
        public string NextToken()
        {
            string result;
            
            // we can be smart about the operations ;)
            result = _useEnumerator ? _ops[_opPointer].ExecuteOperation(_charEnumerator, _startIdx, _masterLen, _isReversed) : _ops[_opPointer].ExecuteOperation(_tokenParts, _startIdx, _isReversed);

            MoveStartIndex(result.Length + _ops[_opPointer].OpLength());
            MoveOpPointer();
            // check to see if there is data to fetch still?
            _hasMoreOps = !_ops[_opPointer].IsFinalOp() & HasMoreData();

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposing)
            {
                if (disposing)
                {
                    _charEnumerator.Dispose();
                }
                _disposing = true;
            }
        }
    }
}