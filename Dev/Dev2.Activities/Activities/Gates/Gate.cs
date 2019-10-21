/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Enums;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Gates
{
    public interface IGateFactory
    {
        IGate New(IExecutionEnvironment env);
    }

    public class GateFactory : IGateFactory
    {
        public IGate New(IExecutionEnvironment env) => new Gate(env);
    }

    public class Gate : IGate
    {
        public IExecutionEnvironment Environment { get; set; }
        public Gate(IExecutionEnvironment env)
        {
            Environment = env;
        }
        public Gate() {
           
        }

        public void Dispose()
        {

        }
    }
}
