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

using Dev2.Common.ExtMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data;


namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "not xml" recordset search option 
    /// </summary>

    public class RsOpNotXML : AbstractRecsetSearchValidation
    {
        public override Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to, bool all) => a => values.All(x => !a.ToString().IsXml());

        public override string HandlesType() => "Not XML";

        public override int ArgumentCount => 1;
    }
}
