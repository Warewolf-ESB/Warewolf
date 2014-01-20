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