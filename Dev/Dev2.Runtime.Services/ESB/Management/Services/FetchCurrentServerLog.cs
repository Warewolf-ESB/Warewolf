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
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchCurrentServerLog : DefaultEsbManagementEndpoint
    {
        readonly string _serverLogPath;

        public FetchCurrentServerLog()
            : this(EnvironmentVariables.ServerLogFile)
        {
        }

        public FetchCurrentServerLog(string serverLogPath)
        {
            _serverLogPath = serverLogPath;
        }

        public string ServerLogPath => _serverLogPath;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

                Dev2Logger.Info("Fetch Server Log Started", GlobalConstants.WarewolfInfo);
                var result = new ExecuteMessage { HasError = false };
                if (File.Exists(_serverLogPath))
                {
                    var fileStream = File.Open(_serverLogPath, FileMode.Open, FileAccess.Read,FileShare.Read);
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        while(!streamReader.EndOfStream)
                        {
                            result.Message.Append(streamReader.ReadLine());    
                        }
                    }
                }
                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(result);
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Fetch Server Log Error", err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchCurrentServerLogService";
    }
}
