/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Activities.RedisCache;
using Dev2.Activities.RedisRemove;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Warewolf.Driver.Redis;
using Warewolf.UnitTestAttributes;

namespace Dev2.Integration.Tests.Redis
{
    
    class TestRedisActivity : RedisCacheActivity
    {
        public TestRedisActivity(IResourceCatalog resourceCatalog, RedisCacheImpl impl)
            : base(resourceCatalog, impl)
        {
        }

        public void TestExecuteTool(IDSFDataObject dataObject)
        {
            base.ExecuteTool(dataObject, 1);
        }

        public void TestPerformExecution(Dictionary<string, string> evaluatedValues)
        {
            base.PerformExecution(evaluatedValues);
        }
    }
    class TestRedisRemoveActivity : RedisRemoveActivity
    {
        public TestRedisRemoveActivity(IResourceCatalog resourceCatalog, RedisCacheImpl impl)
                  : base(resourceCatalog, impl)
        {
        }

        public void TestExecuteTool(IDSFDataObject dataObject)
        {
            base.ExecuteTool(dataObject, 0);
        }
    }
}
