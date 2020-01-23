/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.OS;

namespace Dev2
{
    public class JobConfig : IJobConfig
    {
        private Guid guid;

        public JobConfig(Guid guid)
        {
            this.guid = guid;
        }
        public JobConfig(Guid guid,int concurrency)
            : this(guid)
        {
            this.Concurrency = concurrency;
        }
        public Guid Id => guid;

        public string Name { get; set; }
        public int Concurrency { get; set; }
    }
}
