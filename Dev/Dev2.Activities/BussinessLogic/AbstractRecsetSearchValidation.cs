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
using Dev2.DataList.Contract;


namespace Dev2.DataList
{
    public abstract class AbstractRecsetSearchValidation : IFindRecsetOptions
    {
        public virtual Func<DataStorage.WarewolfAtom, bool> GenerateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to, bool all) => CreateFunc(values, from, to, all);

        public abstract int ArgumentCount { get; }

        public virtual Func<DataStorage.WarewolfAtom, bool> CreateFunc(IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to, bool all) => null;
        public abstract string HandlesType();
    }
}
