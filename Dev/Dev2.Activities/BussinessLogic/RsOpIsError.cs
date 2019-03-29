#pragma warning disable
using System;
using System.Collections.Generic;


namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "is date" recordset search option 
    /// </summary>

    public class RsOpIsError : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to, bool all) => a => false;

        public override string HandlesType() => "There is An Error";

        public override int ArgumentCount => 0;
    }
}