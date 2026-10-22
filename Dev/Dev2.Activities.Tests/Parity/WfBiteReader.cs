/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Tests.Activities.WfCorpus;
using Warewolf.Bite;

namespace Dev2.Tests.Activities.Parity
{
    /// <summary>Today's WF path (ActivityXamlServices.Load + ActivityParser): the reference side of parity.</summary>
    public sealed class WfBiteReader : IBiteReader
    {
        public IDev2Activity Read(string bite) => WfCorpusGenerator.LoadAndParse(bite);
    }
}
