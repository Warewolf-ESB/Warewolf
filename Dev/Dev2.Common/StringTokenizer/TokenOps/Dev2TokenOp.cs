using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common {
    class Dev2TokenOp :  IDev2SplitOp {

        private readonly char[] _tokenParts;
        private readonly bool _include;

        internal Dev2TokenOp(string token, bool includeToken) {
            _include = includeToken;
            _tokenParts = token.ToCharArray();
        }

        public bool IsFinalOp() {
            return false;
        }

        public bool CanUseEnumerator(bool isReversed)
        {
            return (isReversed != true && _tokenParts.Length == 1);
        }

        public string ExecuteOperation(char[] canidate, int startIdx, bool isReversed) {

            StringBuilder result = new StringBuilder();


            if (!isReversed) {
                if (_tokenParts.Length == 1) {
                    int pos = startIdx;
                    while (pos < canidate.Length && canidate[pos] != _tokenParts[0]) {
                        result.Append(canidate[pos]);
                        pos++;
                    }
                }
                else {
                    int pos = startIdx;
                    while (pos < canidate.Length && !IsMultiTokenMatch(canidate, pos, isReversed)) {
                        result.Append(canidate[pos]);
                        pos++;
                    }
                }

                // did the user want the token included?
                if (_include && (result.Length + startIdx) < canidate.Length) {
                    result.Append(_tokenParts);
                }
            }
            else { // reverse order
                if (_tokenParts.Length == 1) {
                    int pos = startIdx;
                    if (pos > canidate.Length) {
                        pos = canidate.Length - 1;
                    }
                    while (pos >= 0 && canidate[pos] != _tokenParts[0]) {
                        result.Insert(0,canidate[pos]);
                        pos--;
                    }
                }
                else {
                    int pos = startIdx;
                    while (pos >= 0 && !IsMultiTokenMatch(canidate, pos, isReversed)) {
                        result.Insert(0,canidate[pos]);
                        pos--;
                    }
                }

                if (_include && (startIdx - result.Length) >= 0) {
                    result.Insert(0, _tokenParts);
                }
            }

            return result.ToString();

        }

        public string ExecuteOperation(CharEnumerator canidate, int startIdx, bool isReversed)
        {

            StringBuilder result = new StringBuilder();
            char[] multTokenMatch = null;

            if (!isReversed)
            {
                if (_tokenParts.Length == 1)
                {
                    char tmp;

                    // fetch next value while 
                    while (canidate.MoveNext() && (tmp = canidate.Current) != _tokenParts[0])
                    {
                        result.Append(tmp);
                    }
                }
                else
                {
                    int pos = startIdx;
                    CharEnumerator clonedCandiate = canidate.Clone() as CharEnumerator;
                    multTokenMatch = new char[_tokenParts.Length];
                    // now move forward the required number of chars ;)
                    for(int i = 0; i < multTokenMatch.Length; i++)
                    {
                        multTokenMatch[i] = clonedCandiate.Current;
                        clonedCandiate.MoveNext();
                    }
                    
                    while (canidate.MoveNext() && !IsMultiTokenMatch(multTokenMatch, pos, isReversed))
                    {
                        result.Append(multTokenMatch[pos]);
                        pos++;
                    }
                }

                // did the user want the token included?
                multTokenMatch = canidate.ToString().ToCharArray();

                if (_include && (result.Length + startIdx) < multTokenMatch.Length)
                {
                    result.Append(_tokenParts);
                }
            }
            else
            { // reverse order
                multTokenMatch = canidate.ToString().ToCharArray();
                if (_tokenParts.Length == 1)
                {
                    int pos = startIdx;
                    if (pos > multTokenMatch.Length)
                    {
                        pos = multTokenMatch.Length - 1;
                    }
                    while (pos >= 0 && multTokenMatch[pos] != _tokenParts[0])
                    {
                        result.Insert(0, multTokenMatch[pos]);
                        pos--;
                    }
                }
                else
                {
                    int pos = startIdx;
                    multTokenMatch = canidate.ToString().ToCharArray();
                    while (pos >= 0 && !IsMultiTokenMatch(multTokenMatch, pos, isReversed))
                    {
                        result.Insert(0, multTokenMatch[pos]);
                        pos--;
                    }
                }

                if (_include && (startIdx - result.Length) >= 0)
                {
                    result.Insert(0, _tokenParts);
                }
            }

            return result.ToString();

        }

        public int OpLength() {
            int result = _tokenParts.Length;
            
            if (_include) {
                result -= 1;
            }

            return result;
        }

        #region Private Method
        private bool IsMultiTokenMatch(char[] canidate, int fromIndex, bool isReversed)
        {
            bool result = true;

            int cnt = 0;
            int canidateIdx = fromIndex;

            if (isReversed)
            {
                cnt = _tokenParts.Length - 1;
            }

            if (((canidateIdx - (_tokenParts.Length - 1)) >= 0 && isReversed) || !isReversed)
            {
                while (((cnt < _tokenParts.Length) && (cnt >= 0)) && (canidateIdx >= 0 && canidateIdx < canidate.Length) && result)
                {
                    if (canidate[canidateIdx] != _tokenParts[cnt])
                    {
                        result = false;
                    }
                    if (!isReversed)
                    {
                        canidateIdx++;
                        cnt++;
                    }
                    else
                    {
                        canidateIdx--;
                        cnt--;
                    }

                }
            }
            else
            {
                result = false; // no way we can match, not enough chars ;)
            }

            return result;
        }
        #endregion
    }
}
