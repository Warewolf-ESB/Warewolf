/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dev2.Common
{
    class Dev2Tokenizer : IDev2Tokenizer, IDisposable
    {
        readonly bool _isReversed;
        readonly int _masterLen;
        private readonly string _sourceString;
        private readonly MemoryStream _memoryStream;
        readonly IList<IDev2SplitOp> _ops;

        bool _disposing;
        bool _hasMoreOps;
        int _opPointer;
        int _startIdx;
        private readonly StreamReader _streamReader;

        internal Dev2Tokenizer(string sourceString, IList<IDev2SplitOp> ops, bool reversed)
        {
            _ops = ops;
            _isReversed = reversed;
            _masterLen = sourceString.Length;
            _sourceString = sourceString;
            _memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(sourceString));
            _streamReader = new StreamReader(_memoryStream);            
            _opPointer = 0;
            _hasMoreOps = true;
            _startIdx = !_isReversed ? 0 : sourceString.Length - 1;
        }

        #region Private Method
      
        void MoveOpPointer()
        {
            _opPointer++;

            if (_opPointer >= _ops.Count)
            {
                _opPointer = 0;
            }
        }

        void MoveStartIndex(int newOffSet)
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

        bool HasMoreData()
        {
            bool result;

            result = !_isReversed ? _startIdx < _masterLen : _startIdx >= 0;

            return result;
        }

        public bool HasMoreOps() => _hasMoreOps;

        public string NextToken()
        {
            _memoryStream.Position = 0;
            _streamReader.DiscardBufferedData();
            var result =_ops[_opPointer].ExecuteOperation(_sourceString, _startIdx, _masterLen, _isReversed);
            MoveStartIndex(result.Length + _ops[_opPointer].OpLength());
            MoveOpPointer();
            _hasMoreOps = !_ops[_opPointer].IsFinalOp() && HasMoreData();
            
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
                    _streamReader.BaseStream.Dispose();
                    _streamReader.Dispose();
                    _charEnumerator.Dispose();
                }
                _disposing = true;
            }
        }
    }
}