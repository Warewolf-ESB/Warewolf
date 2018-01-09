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
    
        public override int ArgumentCount => 1;

        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
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