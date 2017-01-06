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
using Dev2.Common.ExtMethods;
using Dev2.DataList;

namespace Dev2.BussinessLogic
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RsOpIsHex : AbstractRecsetSearchValidation
    {
        public override string HandlesType()
        {
            return "Is Hex";
        }

        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {

            return a => values.All(x => a.ToString().IsHex());

        }

        public override int ArgumentCount => 1;
    }
}

