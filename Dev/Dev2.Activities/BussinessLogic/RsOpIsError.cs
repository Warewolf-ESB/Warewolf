using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "is date" recordset search option 
    /// </summary>
    public class RsOpIsError : AbstractRecsetSearchValidation
    {
        public override Func<DataASTMutable.WarewolfAtom, bool> CreateFunc(IEnumerable<DataASTMutable.WarewolfAtom> values, IEnumerable<DataASTMutable.WarewolfAtom> warewolfAtoms, IEnumerable<DataASTMutable.WarewolfAtom> to, bool all)
        {

            return a => false;

        }
        public override string HandlesType()
        {
            return "There is An Error";
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