/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RecordSetSearchPayload
    {
        public RecordSetSearchPayload(int index, string payload)
        {
            Index = index;
            Payload = payload;
        }

        public RecordSetSearchPayload()
        {
        }

        public int Index { get; set; }

        public string Payload { get; set; }
    }
}