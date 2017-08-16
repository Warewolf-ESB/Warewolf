using System;
using System.Collections.Generic;

namespace Dev2.Studio.AppResources.Comparers
{
    /// <summary>
    /// Used to compare two worksurface keys
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceKeyEqualityComparerWithContextKey : IEqualityComparer<WorkSurfaceKey>
    {

        private static readonly Lazy<WorkSurfaceKeyEqualityComparerWithContextKey> _current
            = new Lazy<WorkSurfaceKeyEqualityComparerWithContextKey>(() => new WorkSurfaceKeyEqualityComparerWithContextKey());

        private WorkSurfaceKeyEqualityComparerWithContextKey()
        {

        }

        public static WorkSurfaceKeyEqualityComparerWithContextKey Current => _current.Value;

        public bool Equals(WorkSurfaceKey x, WorkSurfaceKey y)
        {
            bool res = false;
            if (x.EnvironmentID != null && y.EnvironmentID != null)
            {
                if (x.ResourceID == y.ResourceID
                    && x.ServerID == y.ServerID
                    && x.EnvironmentID == y.EnvironmentID
                    && x.WorkSurfaceContext == y.WorkSurfaceContext)
                {
                    res = true;
                }
            }
            else
            {
                if (x.ResourceID == y.ResourceID
                    && x.ServerID == y.ServerID
                    && x.WorkSurfaceContext ==y.WorkSurfaceContext)
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