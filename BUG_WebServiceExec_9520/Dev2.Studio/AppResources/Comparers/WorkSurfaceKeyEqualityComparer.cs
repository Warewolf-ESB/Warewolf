using System;
using System.Collections.Generic;

namespace Dev2.Studio.AppResources.Comparers
{
    /// <summary>
    /// Used to compare two worksurface keys
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceKeyEqualityComparer : IEqualityComparer<WorkSurfaceKey>
    {
        private static readonly Lazy<WorkSurfaceKeyEqualityComparer> _current 
            = new Lazy<WorkSurfaceKeyEqualityComparer>(() => new WorkSurfaceKeyEqualityComparer());

        private WorkSurfaceKeyEqualityComparer()
        {
            
        }

        public static WorkSurfaceKeyEqualityComparer Current
        {
            get
            {
                return _current.Value;
            }
        }

        public bool Equals(WorkSurfaceKey x, WorkSurfaceKey y)
        {
            return
                ((x.WorkSurfaceContext == y.WorkSurfaceContext)
                 && x.ResourceID == y.ResourceID
                 && x.ServerID == y.ServerID);
        }

        public int GetHashCode(WorkSurfaceKey obj)
        {
            return base.GetHashCode();
        }
    }
}
