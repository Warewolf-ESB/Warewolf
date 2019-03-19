#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Dev2.DataList;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.BussinessLogic
{

    public class RsOpIsBetween : AbstractRecsetSearchValidation
    {
        #region Overrides of AbstractRecsetSearchValidation

        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to, bool all) => a => RunBetween(from, to, a);

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        static bool RunBetween(IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> tovals, DataStorage.WarewolfAtom a)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var iterator = new WarewolfListIterator();
            var from = new WarewolfAtomIterator(warewolfAtoms);
            var to = new WarewolfAtomIterator(tovals);
            iterator.AddVariableToIterateOn(@from);
            iterator.AddVariableToIterateOn(to);
            while(iterator.HasMoreData())
            {
                var fromval = iterator.FetchNextValue(@from);
                var toVal = iterator.FetchNextValue(to);

                if (DateTime.TryParse(fromval, out DateTime fromDt))
                {
                    if (!DateTime.TryParse(toVal, out DateTime toDt))
                    {
                        throw new InvalidDataException(ErrorResource.IsBetweenDataTypeMismatch);
                    }
                    if (DateTime.TryParse(a.ToString(), out DateTime recDateTime) && recDateTime > fromDt && recDateTime < toDt)
                    {
                        return true;
                    }

                }
                if (double.TryParse(fromval, out double fromNum))
                {
                    if (!double.TryParse(toVal, out double toNum))
                    {
                        return false;
                    }
                    if (!double.TryParse(a.ToString(), out double recNum))
                    {
                        continue;
                    }
                    if (recNum > fromNum && recNum < toNum)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        public override string HandlesType() => "Is Between";

        public override int ArgumentCount => 3;
    }
}

