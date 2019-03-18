#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

    public class RsOpNotBetween : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to, bool all) => a => !RunBetween(from, to, a);

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
            while (iterator.HasMoreData())
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
        public override string HandlesType() => "Not Between";

        public override int ArgumentCount => 3;
    }
}

