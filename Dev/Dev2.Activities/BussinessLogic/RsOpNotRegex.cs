/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "regex" recordset search option 
    /// </summary>

    public class RsOpNotRegex : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {


            if (all)
            {
                return a => !values.All(x =>
                {
                    var exp = new Regex(x.ToString());
                    return exp.IsMatch(a.ToString());

                });
            }

            return a => !values.Any(x =>
            {
                var exp = new Regex(x.ToString());
                return exp.IsMatch(a.ToString());

            });

        }
        public override string HandlesType()
        {
            return "Not Regex";
        }
        public override int ArgumentCount => 2;
    }
}
