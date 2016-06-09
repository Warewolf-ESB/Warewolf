
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Data.Decision
{
    public class Dev2DataListDecisionHandler
    {
        private static Dev2DataListDecisionHandler _inst;
        private static readonly IDictionary<Guid, IExecutionEnvironment> _environments = new ConcurrentDictionary<Guid, IExecutionEnvironment>();
        public static Dev2DataListDecisionHandler Instance
        {
            get
            {
                return _inst ?? (_inst = new Dev2DataListDecisionHandler());
            }
        }


        public void AddEnvironment(Guid id, IExecutionEnvironment env)
        {
            _environments.Add(id,env);
        }

        public void RemoveEnvironment(Guid id)
        {
            _environments.Remove(id);
        }


        // Guid dlID
    }
}
