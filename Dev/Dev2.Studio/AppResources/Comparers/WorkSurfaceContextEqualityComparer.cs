
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Studio.ViewModels.WorkSurface;

namespace Dev2.Studio.AppResources.Comparers
{
    public class WorkSurfaceContextEqualityComparer : IEqualityComparer<WorkSurfaceContextViewModel>
    {
        private static readonly Lazy<WorkSurfaceContextEqualityComparer> _current
            = new Lazy<WorkSurfaceContextEqualityComparer>(() => new WorkSurfaceContextEqualityComparer());

        private WorkSurfaceContextEqualityComparer()
        {
        }

        public static WorkSurfaceContextEqualityComparer Current
        {
            get { return _current.Value; }
        }

        public bool Equals(WorkSurfaceContextViewModel x, WorkSurfaceContextViewModel y)
        {
            return WorkSurfaceKeyEqualityComparer.Current.Equals(x.WorkSurfaceKey, y.WorkSurfaceKey);
        }

        public int GetHashCode(WorkSurfaceContextViewModel obj)
        {
            return base.GetHashCode();
        }
    }
}
