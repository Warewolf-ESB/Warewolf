/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer.Executor
{
    public interface IExecutionDto
    {
        DataListFormat DataListFormat { get; set; }
        Guid DataListIdGuid { get; set; }
        IDSFDataObject DataObject { get; set; }
        ErrorResultTO ErrorResultTO { get; set; }
        string PayLoad { get; set; }
        EsbExecuteRequest Request { get; set; }
        IWarewolfResource Resource { get; set; }
        Dev2JsonSerializer Serializer { get; set; }
        string ServiceName { get; set; }
        WebRequestTO WebRequestTO { get; set; }
        Guid WorkspaceID { get; set; }
    }
}