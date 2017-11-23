/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.TO;
using Dev2.Interfaces;

namespace Dev2.Services.Execution
{
    public interface IServiceExecution
    {
        IDSFDataObject DataObj { get; set; }
        string InstanceOutputDefintions { get; set; }
        string InstanceInputDefinitions { get; set; }

        void BeforeExecution(ErrorResultTO errors);
        Guid Execute(out ErrorResultTO errors, int update);
        void AfterExecution(ErrorResultTO errors);

        void GetSource(Guid sourceId);
    }
}
