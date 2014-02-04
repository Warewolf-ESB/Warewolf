using System.Collections.Generic;
using Dev2.Common.StringTokenizer.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Common
{
    public class Dev2TokenizerBuilder
    {

        public string ToTokenize { get; set; }

        public bool ReverseOrder { get; set; }

        private IList<IDev2SplitOp> _ops = new List<IDev2SplitOp>();

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

            if(string.IsNullOrEmpty(ToTokenize))
            {
                throw new TokenizeError("Null or empty tokenize string!");
            }

            if(_ops.Count <= 0)
            {
                throw new TokenizeError("Cant find anything to split on!");
            }

            return new Dev2Tokenizer(ToTokenize, _ops, ReverseOrder);
        }

        public void AddTokenOp(string token, bool returnToken, string escape)
        {
            _ops.Add(new Dev2TokenOp(token, returnToken, escape));
        }
    }
}
