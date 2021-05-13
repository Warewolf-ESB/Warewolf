/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using System.Collections.Generic;

namespace Dev2.Activities.Factories
{
    public interface IResponseManagerFactory
    {
        IResponseManager New(IOutputDescription outputDescription);
    }

    public class ResponseManagerFactory : IResponseManagerFactory
    {
        public IResponseManager New(IOutputDescription outputDescription)
        {
            return new ResponseManager
            {
                OutputDescription = outputDescription
            };
        }
    };
}
