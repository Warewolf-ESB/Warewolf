/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Core.Tests.Environments
{
    public class TestLoadServerRespository : ServerRepository
    {
        public int LoadInternalHitCount { get; set; }

        public TestLoadServerRespository(IServer server)
            :base(server)
        {
        }

        public TestLoadServerRespository(IServer source, params IServer[] environments)
            : base(source)
        {
            if(environments != null)
            {
                foreach(var environment in environments)
                {
                    Environments.Add(environment);
                }
            }
        }

        protected override void LoadInternal(bool force = false)
        {
            // Override, so that we don't connect to the server!
            LoadInternalHitCount++;
        }

        public override ICollection<IServer> ReloadAllServers()
        {
            return Environments;
        }

        public override ICollection<IServer> All()
        {
            return Environments;
        }
    }

}
