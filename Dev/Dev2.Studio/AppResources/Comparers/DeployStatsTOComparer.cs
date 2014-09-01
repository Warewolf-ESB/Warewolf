using System.Collections;
using Dev2.Studio.TO;

namespace Dev2.Studio.AppResources.Comparers
{
    public class DeployStatsTOComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            DeployStatsTO a = x as DeployStatsTO;
            DeployStatsTO b = y as DeployStatsTO;

            if(a == null || b == null)
            {
                return 1;
            }

            if(a.Name == b.Name && a.Description == b.Description)
            {
                return 0;
            }
            return 1;
        }
    }
}
