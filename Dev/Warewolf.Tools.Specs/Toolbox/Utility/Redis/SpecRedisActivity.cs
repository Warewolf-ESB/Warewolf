/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.Redis;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using System.Collections.Generic;
using Warewolf.Driver.Redis;

namespace Warewolf.Tools.Specs.Toolbox.Utility.Redis
{
    public class SpecRedisActivity : RedisActivity
    {
        public SpecRedisActivity()
        {
        }

        public SpecRedisActivity(IResourceCatalog resourceCatalog, RedisCacheBase redisCache) : base(resourceCatalog, redisCache)
        {
        }

        public List<string> SpecPerformExecution(Dictionary<string, string> evaluatedValues)
        {
            return base.PerformExecution(evaluatedValues);
        }

        public void SpecExecuteTool(IDSFDataObject dataObject)
        {
            base.ExecuteTool(dataObject, 0);
        }

    }
}
