
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// ReSharper disable CheckNamespace
namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "not contains" recordset search option 
    /// </summary>
    public class RsOpNotContains : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {
            if (all)
                return a => values.All(x =>! a.ToString().ToLower(CultureInfo.InvariantCulture).Contains(x.ToString().ToLower(CultureInfo.InvariantCulture)));
            return a => values.Any(x => !a.ToString().ToLower(CultureInfo.InvariantCulture).Contains(x.ToString().ToLower(CultureInfo.InvariantCulture)));
        }
        public override string HandlesType()
        {
            return "Doesn't Contain";
        }
        public override int ArgumentCount
        {
            get
            {
                return 2;
            }
        }
    }
}
