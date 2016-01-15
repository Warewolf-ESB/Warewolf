using System;
using System.Collections.Generic;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "is date" recordset search option 
    /// </summary>
    public class RsOpIsNoError : AbstractRecsetSearchValidation
    {
        public override Func<DataASTMutable.WarewolfAtom, bool> CreateFunc(IEnumerable<DataASTMutable.WarewolfAtom> values, IEnumerable<DataASTMutable.WarewolfAtom> warewolfAtoms, IEnumerable<DataASTMutable.WarewolfAtom> to, bool all)
        {

            return a => false;

        }
        public override string HandlesType()
        {
            return "There is No Error";
        }

        public override int ArgumentCount
        {
            get
            {
                return 0;
            }
        }
    }
}