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
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "is xml" recordset search option 
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RsOpIsXML : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms, IEnumerable<DataStorage.WarewolfAtom> to, bool all)
        {

            return a => values.All(x => a.ToString().IsXml());

        }
        public override string HandlesType()
        {
            return "Is XML";
        }
        public override int ArgumentCount => 1;
    }
}
