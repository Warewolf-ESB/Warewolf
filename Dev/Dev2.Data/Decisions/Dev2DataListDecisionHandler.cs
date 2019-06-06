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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.Decision
{
    // Referenced in the Resources Namespace and Variables
    public class Dev2DataListDecisionHandler
    {
        private readonly static Dev2DataListDecisionHandler _instance = new Dev2DataListDecisionHandler();
        internal static readonly IDictionary<Guid, IExecutionEnvironment> _environments = new ConcurrentDictionary<Guid, IExecutionEnvironment>();
        public static Dev2DataListDecisionHandler Instance { get => _instance; }

        public void AddEnvironment(Guid id, IExecutionEnvironment env)
        {
            _environments.Add(id, env);
        }

        public void RemoveEnvironment(Guid id)
        {
            _environments.Remove(id);
        }
    }
}
