using System;
using System.Text;
using Dev2.Studio.Core.AppResources.Enums;

namespace Dev2.Studio.AppResources.Comparers
{
    /// <summary>
    ///     Used to uniquely identify a specific WorkSurface (I.E. something displayed in a Tab)
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceKey : IEquatable<WorkSurfaceKey>
    {
        public WorkSurfaceContext WorkSurfaceContext { get; set; }

        public Guid? ServerID { get; set; }

        public Guid? ResourceID { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Context_{0}_", WorkSurfaceContext);
            if (ServerID.HasValue)
                sb.AppendFormat("ServerID_{0}_", ServerID);
            if (ResourceID.HasValue)
                sb.AppendFormat("ResourceID_{0}_", ResourceID);
            var returnString = sb.ToString().Replace('-', '_');
            return returnString;
        }

        public bool Equals(WorkSurfaceKey other)
        {
            return WorkSurfaceKeyEqualityComparer.Current.Equals(this, other);
        }
    }
}