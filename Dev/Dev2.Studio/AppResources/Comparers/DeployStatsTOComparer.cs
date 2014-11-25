
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
