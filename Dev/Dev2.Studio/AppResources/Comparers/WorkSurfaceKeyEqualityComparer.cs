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

using Dev2.Common.Interfaces;
using System;
using System.Collections.Generic;


namespace Dev2.Studio.AppResources.Comparers
{
    public class WorkSurfaceKeyEqualityComparer : IEqualityComparer<WorkSurfaceKey>
    {
        static readonly Lazy<WorkSurfaceKeyEqualityComparer> _current
            = new Lazy<WorkSurfaceKeyEqualityComparer>(() => new WorkSurfaceKeyEqualityComparer());

        WorkSurfaceKeyEqualityComparer()
        {

        }

        public static WorkSurfaceKeyEqualityComparer Current => _current.Value;

        public bool Equals(WorkSurfaceKey x, WorkSurfaceKey y)
        {
            var res = false;
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

        public bool Equals(IWorkSurfaceKey x, IWorkSurfaceKey y)
        {
            var res = false;
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

        public int GetHashCode(WorkSurfaceKey obj) => obj.GetHashCode();
    }
}
