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
using Dev2.DataList.Contract;

namespace Dev2.Data.TO
{
    [Serializable]
    public class Dev2DataLanguageIntellisensePart : IDev2DataLanguageIntellisensePart
    {

        public string Name { get; private set; }

        public string Description { get; private set; }

        public IList<IDev2DataLanguageIntellisensePart> Children { get; private set; }

        public Dev2DataLanguageIntellisensePart(string name, string desc, IList<IDev2DataLanguageIntellisensePart> children)
        {
            Name = name;
            Children = children;
            Description = desc;
        }

    }
}
