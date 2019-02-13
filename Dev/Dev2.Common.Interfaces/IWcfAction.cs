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
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IWcfAction
    {
        string FullName { get; set; }
        string Method { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        Type ReturnType { get; set; }
        IList<INameValue> Variables { get; set; }
        string GetHashCodeBySource();
    }

    public class WcfAction : IWcfAction
    {
        public string FullName { get; set; }
        public string Method { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public Type ReturnType { get; set; }
        public IList<INameValue> Variables { get; set; }
        public string GetHashCodeBySource() => FullName + Method;
    }
}
