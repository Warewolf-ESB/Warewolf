/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
// ReSharper disable CheckNamespace
namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "regex" recordset search option 
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RsOpRegex : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {
            if (all)
                return a => values.All(x =>
                {
                    Regex exp = new Regex(x.ToString());
                    return exp.IsMatch(a.ToString());

                });
            return a => values.Any(x =>
            {
                Regex exp = new Regex(x.ToString());
                return exp.IsMatch(a.ToString());

            });
        }

        public override string HandlesType()
        {
            return "Is Regex";
        }
        public override int ArgumentCount => 2;
    }
}
