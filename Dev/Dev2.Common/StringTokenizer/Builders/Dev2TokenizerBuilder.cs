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
using System.Collections.Generic;
using Warewolf.Resource.Errors;

// ReSharper disable CheckNamespace

namespace Dev2.Common
{
    public class Dev2TokenizerBuilder
    {
        private readonly IList<IDev2SplitOp> _ops = new List<IDev2SplitOp>();
        public string ToTokenize { get; set; }

        public bool ReverseOrder { get; set; }

        public void AddIndexOp(int index)
        {
            _ops.Add(new Dev2IndexOp(index));
        }

        public void AddTokenOp(string token, bool returnToken)
        {
            _ops.Add(new Dev2TokenOp(token, returnToken));
        }

        public void AddEoFOp()
        {
            _ops.Add(new Dev2EoFOp());
        }

        public IDev2Tokenizer Generate()
        {
            if (string.IsNullOrEmpty(ToTokenize))
            {
                throw new TokenizeError(ErrorResource.NullTokenzeString);
            }

            if (_ops.Count <= 0)
            {
                throw new TokenizeError(ErrorResource.NothingToSplit);
            }

            return new Dev2Tokenizer(ToTokenize, _ops, ReverseOrder);
        }

        public void AddTokenOp(string token, bool returnToken, string escape)
        {
            _ops.Add(new Dev2TokenOp(token, returnToken, escape));
        }
    }
}