/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Configuration;

namespace Dev2.Util
{

    public static class AppUsageStats
    {
        public static string LocalHost { get; set; }

        static string _serviceName;
        public static string ServiceName => _serviceName ?? (_serviceName = ConfigurationManager.AppSettings["ServiceName"] ?? "Warewolf Server");

#if !DEBUG
        public static bool CollectUsageStats
        {
            get
            {
                Boolean.TryParse(ConfigurationManager.AppSettings["CollectUsageStats"], out bool collectUsageStats);
                return collectUsageStats;
            }
        }
#endif
    }
}
