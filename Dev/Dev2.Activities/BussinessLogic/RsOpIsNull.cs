using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "Is NULL" recordset search option 
    /// </summary>
    public class RsOpIsNull : AbstractRecsetSearchValidation
    {

        public override Func<DataASTMutable.WarewolfAtom, bool> CreateFunc(IEnumerable<DataASTMutable.WarewolfAtom> values, IEnumerable<DataASTMutable.WarewolfAtom> warewolfAtoms, IEnumerable<DataASTMutable.WarewolfAtom> to, bool all)
        {
            if(all)
            {
                return a => values.All(x => a.IsNothing);
            }
            return a => values.Any(x => a.IsNothing);
        }
        public override string HandlesType()
        {
            return "Is NULL";
        }
    }
}