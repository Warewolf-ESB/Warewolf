using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "is date" recordset search option 
    /// </summary>

    public class RsOpIsNoError : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {

            return a => false;

        }
        public override string HandlesType()
        {
            return "There is No Error";
        }

        public override int ArgumentCount => 0;
    }
}