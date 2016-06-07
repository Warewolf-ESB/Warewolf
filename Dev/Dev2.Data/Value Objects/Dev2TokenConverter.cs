
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Dev2.DataList.Contract.Value_Objects
{
    public class Dev2TokenConverter
    {
        // yes only single instance per application, a token is a token after all
        private readonly static IDictionary<string, IIntellisenseResult> _partsCache = new ConcurrentDictionary<string, IIntellisenseResult>();
        // Language Parser
        private readonly IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser();
    }
}
