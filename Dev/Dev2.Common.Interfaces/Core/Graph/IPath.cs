/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IPath
    {
        string ActualPath { get; set; }
        string DisplayPath { get; set; }
        string SampleData { get; set; }
        string OutputExpression { get; set; }

        IEnumerable<IPathSegment> GetSegements();
        IPathSegment CreatePathSegment(string pathSegmentString);
    }
}