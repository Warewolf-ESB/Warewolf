using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Reflection;
using Dev2.Common.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Newtonsoft.Json;
using PluginSource = Dev2.Runtime.ServiceModel.Data.PluginSource;

namespace Dev2.Runtime.ServiceModel
{
    public class PluginSources : ExceptionManager
    {
        #region Get

        // POST: Service/PluginSources/Get
        public PluginSource Get(string resourceID, Guid workspaceID, Guid dataListID)
        {
            var result = new PluginSource { ResourceID = Guid.Empty, ResourceType = ResourceType.PluginSource };
            try
            {
                var xmlStr = Resources.ReadXml(workspaceID, ResourceType.PluginSource, resourceID);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new PluginSource(xml);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Save
        
        // POST: Service/PluginSources/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            var pluginSourceDetails = JsonConvert.DeserializeObject<PluginSource>(args);

            if(string.IsNullOrEmpty(pluginSourceDetails.AssemblyName))
            {
                //resolve AssemblyName from AssemblyLocation
                if(!pluginSourceDetails.AssemblyLocation.StartsWith("GAC:"))
                {
                    //assembly location refers to a file, read the assembly name out of the dll file
                    pluginSourceDetails.AssemblyLocation = pluginSourceDetails.AssemblyLocation.EndsWith("\\") ?
                        pluginSourceDetails.AssemblyLocation.Remove(pluginSourceDetails.AssemblyLocation.Length - 1) : //remove trailing slashes if they exist
                        pluginSourceDetails.AssemblyLocation; //else do nothing
                    try
                    {
                        pluginSourceDetails.AssemblyName = AssemblyName.GetAssemblyName(pluginSourceDetails.AssemblyLocation).Name;
                    }
                    catch(Exception)
                    {
                        if(!string.IsNullOrEmpty(pluginSourceDetails.AssemblyLocation))
                        {
                            pluginSourceDetails.AssemblyName = pluginSourceDetails.AssemblyLocation.Substring(pluginSourceDetails.AssemblyLocation.LastIndexOf("\\", StringComparison.Ordinal)+1, pluginSourceDetails.AssemblyLocation.IndexOf(".dll", StringComparison.Ordinal) - pluginSourceDetails.AssemblyLocation.LastIndexOf("\\", StringComparison.Ordinal)-1);
                        }
                    }
                }
                else
                {
                    //assembly location refers to the GAC
                    var getName = pluginSourceDetails.AssemblyLocation.Substring(pluginSourceDetails.AssemblyLocation.IndexOf(':')+1, pluginSourceDetails.AssemblyLocation.IndexOf(' ')-4);
                    pluginSourceDetails.AssemblyName = getName;
                }
            }

            pluginSourceDetails.Save(workspaceID);
            if (workspaceID != GlobalConstants.ServerWorkspaceID)
            {
                pluginSourceDetails.Save(GlobalConstants.ServerWorkspaceID);
            }

            return pluginSourceDetails.ToString();
        }

        #endregion

        #region GetServerDirectoryTree

        // POST: Service/PluginSources/GetServerDirectoryTree
        public string GetServerDirectoryTree(string args, Guid workspaceID, Guid dataListID)
        {
            //Initialize DirectoryInfo to server root directory
            var directory = new DirectoryInfo(Path.GetPathRoot(Environment.CurrentDirectory)[0].ToString(CultureInfo.InvariantCulture)+":\\");
            if(!string.IsNullOrEmpty(args))
            {
                directory = new DirectoryInfo(Path.GetPathRoot(Environment.CurrentDirectory)[0].ToString(CultureInfo.InvariantCulture) + ":\\" + args);
            }

            //Build directory tree in JSON, only dll files included
            string name = string.Empty;
            string json = "[";
            foreach (DirectoryInfo d in directory.GetDirectories())
            {
                name = Regex.Replace(d.Name, @"\\", @"\\");
                json += @"{""title"":""" + name + @""", ""isFolder"": true, ""key"":""" +
                        name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                json += ',';
            }

            foreach (FileInfo f in directory.GetFiles())
            {
                if(f.Name.EndsWith(".dll"))
                {
                    json += @"{""title"":""" + f.Name + @""", ""key"":""" +
                            f.Name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                    json += ',';
                }
            }
            json=json.Remove(json.LastIndexOf(",", System.StringComparison.Ordinal));
            json += ']';
            return json;
        }

        #endregion

        #region GetServerDirectoryTree

        // POST: Service/PluginSources/GetGacList
        public string GetGacList(string args, Guid workspaceID, Guid dataListID)
        {
            var fullGAC = GAC.CreateGACEnum();
            IAssemblyName gacIterator;
            GAC.GetNextAssembly(fullGAC, out gacIterator);
            string json = "[";
            while (GAC.GetNextAssembly(fullGAC, out gacIterator) == 0)
            {
                string getAssemblyVersion = string.Empty;
                try
                {
                    getAssemblyVersion = GAC.GetVersion(gacIterator).ToString();
                }
                catch (Exception e)
                {
                    getAssemblyVersion = "0.0.0.0";
                }
                json += @"{""Text"":""" + GAC.GetName(gacIterator) + " " + getAssemblyVersion + @"""}";
                json += ',';
            }

            json = json.Remove(json.LastIndexOf(",", System.StringComparison.Ordinal));
            json += ']';
            return json;
        }

        #endregion

        #region GetRootDriveLetter

        // POST: Service/PluginSources/GetRootDriveLetter
        public string GetRootDriveLetter(string args, Guid workspaceID, Guid dataListID)
        {
            var toJson = @"[{""driveLetter"":""" + Path.GetPathRoot(Environment.CurrentDirectory)[0].ToString(CultureInfo.InvariantCulture) + @"""}]";
            return toJson;
        }

        #endregion

        #region GetDirectoryIntellisense

        // POST: Service/PluginSources/GetDirectoryIntellisense
        public string GetDirectoryIntellisense(string args, Guid workspaceID, Guid dataListID)
        {
            var directory = new DirectoryInfo(args);

            //Build intellisense results
            IList<string> dirList = new List<string>();
            foreach (DirectoryInfo d in directory.GetDirectories())
            {
                dirList.Add(args + d.Name);
            }
            foreach (FileInfo f in directory.GetFiles())
            {
                if(f.Name.EndsWith(".dll"))
                {
                    dirList.Add(args + f.Name);
                }
            }
            return JsonConvert.SerializeObject(dirList);
        }

        #endregion

        #region ValidateAssemblyImageFormat

        // POST: Service/PluginSources/ValidateAssemblyImageFormat
        public string ValidateAssemblyImageFormat(string args, Guid workspaceID, Guid dataListID)
        {
            var toJson = @"{""validationresult"":""success""}";
            try
            {
                Assembly loadedAssembly = Assembly.LoadFile(args);
            }
            catch(Exception e)
            {
                ServerLogger.LogError(e.Message);
                toJson = @"{""validationresult"":""failure""}";
            }
            return toJson;
        }

        #endregion
    }
}