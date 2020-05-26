#pragma warning disable
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
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Warewolf.Storage.Interfaces;



namespace Dev2

{
    public interface IEsbChannelFactory
    {
        IEsbChannel New();
    }
    public interface IEsbChannel
    {
        Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId,
            out ErrorResultTO errors);
        
        IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors, int update, bool handleErrors);

        void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors, int update);

        void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update);
    }
}