using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.TO;

namespace Dev2.Studio.AppResources.Comparers
{
    public class DeployStatsTOComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            DeployStatsTO a = x as DeployStatsTO;
            DeployStatsTO b = y as DeployStatsTO;

            if (a == null || b == null)
            {
                return 1;
            }

            if (a.Name == b.Name && a.Description == b.Description)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
