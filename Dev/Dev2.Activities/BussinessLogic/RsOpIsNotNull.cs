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
using System.Linq;


namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "Is Not NULL" recordset search option 
    /// </summary>
    public class RsOpIsNotNull : AbstractRecsetSearchValidation
    {
   
        public override int ArgumentCount => 1;

        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {
            if(all)
            {
                return a => values.All(x => !a.IsNothing);
            }
            return a => values.Any(x => !a.IsNothing);
        }
        public override string HandlesType() => "Is Not NULL";
    }
}