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
using System.Globalization;
using System.Linq;
// ReSharper disable CheckNamespace
namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "starts with" recordset search option 
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RsOpStartsWith : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {
            if (all)
                return a => values.All(x => a.ToString().ToLower(CultureInfo.InvariantCulture).StartsWith(x.ToString().ToLower(CultureInfo.InvariantCulture)));
            return a => values.Any(x => a.ToString().ToLower(CultureInfo.InvariantCulture).StartsWith(x.ToString().ToLower(CultureInfo.InvariantCulture)));
        }
        public override string HandlesType()
        {
            return "Starts With";
        }
        public override int ArgumentCount => 2;
    }
}
