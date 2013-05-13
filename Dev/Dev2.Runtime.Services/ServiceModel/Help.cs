using System;
using Dev2.Common;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public class Help : ExceptionManager
    {
        #region GetDictionary

        // POST: Service/Help/GetDictionary
        public HelpDictionary GetDictionary(string dictionaryID, Guid workspaceID, Guid dataListID)
        {
            var result = new HelpDictionary();

            try
            {
                // TODO: Read from file based on dictionaryID
                switch(dictionaryID)
                {
                    case "Server":
                        result.Add("default", "<h4>New Server Details</h4><p>This creates a connection to an existing server.</p>");
                        result.Add("address", "Enter the <b>server url</b> e.g. http://192.168.0.1:77/dsf.");
                        result.Add("userName", "Enter your <b>user name</b>.");
                        result.Add("password", "Enter the <b>password</b> for the server.");
                        break;
                    //07.03.2013: Ashley Lewis - PBI 8720:
                    case "dbSource":
                        result.Add("default", "<h4>New Database Source Details</h4><p>This creates a connection to an existing database server.</p>");
                        result.Add("dbSourceServerType", "Enter the database <b>type</b> e.g. Microsoft SQL");
                        result.Add("dbSourceServer", "Enter the database <b>server url</b> e.g. IP address or computer name");
                        result.Add("dbSourceUserID", "Enter either your database <b>user name</b> or a Windows domain username e.g. 'Domainname\\Username'");
                        result.Add("dbSourcePassword", "Enter the <b>password</b> for the server.");
                        result.Add("dbSourceDatabase", "Select one of the <b>databases</b> hosted by the server");
                        break;
                    //16.04.2013: Ashley Lewis - PBI 8721:
                    case "pluginSource":
                        result.Add("default", "<h4>Plugin File</h4><p>Select a Dll file to connect to</p>");
                        result.Add("pluginAssemblyFileLocation", "Enter the plugin <b>file address.</b> e.g. 'C:\\Warewolf\\Plugins\\email.plugin.dll'");
                        result.Add("pluginAssemblyGACLocation", "Enter the plugin <b>assembly name</b> starting with '" + GlobalConstants.GACPrefix + "' and followed by the verion number e.g. '" + GlobalConstants.GACPrefix + "Microsoft.Email.Client.Library 2.0.0.0'");
                        result.Add("tab 0", "<h4>Plugin File</h4><p>Select a Dll file to connect to</p>");
                        result.Add("tab 1", "<h4>Global Cache</h4><p>Select an assembly from the <b>Global Assemblies Cache</b></p>");
                        result.Add("GACList", "<h4>Global Cache</h4><p>Select an assembly from the <b>Global Assemblies Cache</b></p>");
                        result.Add("gacSearchTerm", "<h4>Global Cache</h4><p>You are viewing all assemblies</p>");
                        break;
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

    }
}
