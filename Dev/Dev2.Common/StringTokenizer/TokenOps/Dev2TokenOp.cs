/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using System;
using System.IO;
using System.Text;

namespace Dev2.Common
{
    class Dev2TokenOp : IDev2SplitOp
    {
        readonly string _escapeChar;
        readonly bool _include;
        readonly char[] _tokenParts;
        private readonly string _seperator;

        internal Dev2TokenOp(string token, bool includeToken)
            : this(token, includeToken, "")
        {
        }

        internal Dev2TokenOp(string token, bool includeToken, string escape)
        {
            _include = includeToken;
            _tokenParts = token.ToCharArray();
            _seperator = token;
            _escapeChar = escape;
        }

        public bool IsFinalOp() => false;
      
        public string ExecuteOperation(ref StringBuilder sourceString, int startIdx, int len, bool isReversed)
        {
            var result = isReversed ? ReverseSearch(sourceString, startIdx) : ForwardSearch(sourceString, startIdx);
            return result.ToString();
        }

        private StringBuilder ForwardSearch(StringBuilder sourceString, int startIdx)
        {            
            int pos = sourceString.IndexOf(_seperator,_escapeChar, startIdx, false);
            var result = new StringBuilder();
            if (pos > -1)
            {

                for (int i = startIdx; i < pos; i++)
                {
                    result.Append(sourceString[i]);
                }
                if (_include && result.Length < sourceString.Length)
                {
                    result.Append(_tokenParts);
                }
            }
            else
            {
                for (int i = startIdx; i < sourceString.Length; i++)
                {
                    result.Append(sourceString[i]);
                }
            }            
            return result;
        }

        private StringBuilder ReverseSearch(StringBuilder sourceString, int startIdx)
        {
            int pos = sourceString.LastIndexOf(_seperator,startIdx, false);
            var result = new StringBuilder();
            if (pos > -1)
            {

                    // fetch next value while
                    while (parts.MoveNext() &&
                           ((tmp = parts.Current) != _tokenParts[0] || SkipDueToEscapeChar(result.ToString())))
                    {
                        result.Append(tmp);
                    }
                }

                // did they want the token included?
                if (_include && startIdx + result.Length < len)
                {
                    result.Append(_tokenParts);
                }
            }
            else
            {
                throw new Exception(ErrorResource.CharEnumeratorNotSupported);
            }

            return result.ToString();
        }

        public string ExecuteOperation(StreamReader reader, int startIdx, int len, bool isReversed)
        {
            var result = new StringBuilder();

            if (!isReversed && _tokenParts.Length == 1)
            {
                if (_tokenParts.Length == 1)
                {
                    var maxRead = len - startIdx;
                    var currentChars = new char[maxRead];
                    if (reader.Read(currentChars, startIdx, maxRead-1) >= 0)
                    {
                        var pos = 0;
                        while (pos<maxRead)
                        {
                            char tmp = currentChars[pos];
                            if (tmp != _tokenParts[0] || SkipDueToEscapeChar(result.ToString()))
                            {
                                result.Append(tmp);
                            }
                            else
                            {
                                break;
                            }
                            pos++;
                        }
                    }
                }

                // did they want the token included?
                if (_include && startIdx + result.Length < len)
                {
                    result.Insert(0, _tokenParts);
                }
            }
            else
            {
                throw new Exception(ErrorResource.CharEnumeratorNotSupported);
            }

            return result.ToString();
        }


        public string ExecuteOperation(string sourceString, int startIdx, int len, bool isReversed)
        {
            var result = new StringBuilder();
            int pos = sourceString.IndexOf(_seperator, startIdx, StringComparison.InvariantCulture);
            if (pos > -1)
            {
                result.Append(sourceString.Substring(startIdx, pos-startIdx));
                if (_include && startIdx + result.Length < len)
                {
                    result.Append(_tokenParts);
                }
            }
            else
            {
                result.Append(sourceString.Substring(startIdx));
            }
            return result.ToString();
        }

        public int OpLength()
        {
            var result = _tokenParts.Length;

            if (_include)
            {
                result -= _tokenParts.Length;
            }

            return result;
        }       
    }
}