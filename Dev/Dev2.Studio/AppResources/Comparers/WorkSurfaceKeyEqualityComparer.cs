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

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.AppResources.Comparers
{
    /// <summary>
    /// Used to compare two worksurface keys
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceKeyEqualityComparer : IEqualityComparer<WorkSurfaceKey>
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<WorkSurfaceKeyEqualityComparer> _current
            = new Lazy<WorkSurfaceKeyEqualityComparer>(() => new WorkSurfaceKeyEqualityComparer());

        private WorkSurfaceKeyEqualityComparer()
        {

        }

        public static WorkSurfaceKeyEqualityComparer Current => _current.Value;

        public bool Equals(WorkSurfaceKey x, WorkSurfaceKey y)
        {
            bool res = false;
            if (x.EnvironmentID != null && y.EnvironmentID != null)
            {
                if (x.ResourceID == y.ResourceID
                 && x.ServerID == y.ServerID
                    && x.EnvironmentID == y.EnvironmentID)
                {
                    res = true;
                }
            }
            else
            {
                if (x.ResourceID == y.ResourceID
                 && x.ServerID == y.ServerID)
                {
                    res = true;
                }
            }
            return res;

        }

        public int GetHashCode(WorkSurfaceKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
