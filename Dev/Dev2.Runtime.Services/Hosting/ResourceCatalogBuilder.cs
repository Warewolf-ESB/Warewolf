
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{

    /// <summary>
    /// Transfer FileStream and FilePath together
    /// </summary>
// ReSharper disable InconsistentNaming
    internal class ResourceBuilderTO
// ReSharper restore InconsistentNaming
    {
        internal string FilePath;
        internal FileStream FileStream;
    }

    /// <summary>
    /// Used to build up the resource catalog ;)
    /// </summary>
    public class ResourceCatalogBuilder
    {
        private readonly List<IResource> _resources = new List<IResource>();
        private readonly HashSet<Guid> _addedResources = new HashSet<Guid>();
        private readonly IResourceUpgrader _resourceUpgrader;
        private readonly object _addLock = new object();

        public ResourceCatalogBuilder(IResourceUpgrader resourceUpgrader)
        {
            _resourceUpgrader = resourceUpgrader;
        }
        public ResourceCatalogBuilder()
        {
            _resourceUpgrader = ResourceUpgraderFactory.GetUpgrader();
        }
        public IList<IResource> ResourceList { get { return _resources; } }


        public void BuildCatalogFromWorkspace(string workspacePath, params string[] folders)
        {
            if(string.IsNullOrEmpty(workspacePath))
            {
                throw new ArgumentNullException("workspacePath");
            }
            if(folders == null)
            {
                throw new ArgumentNullException("folders");
            }

            if(folders.Length == 0 || !Directory.Exists(workspacePath))
            {
                return;
            }

            var streams = new List<ResourceBuilderTO>();

            try
            {

                foreach(var path in folders.Where(f => !string.IsNullOrEmpty(f) && !f.EndsWith("VersionControl")).Select(f => Path.Combine(workspacePath, f)))
                {
                    if(!Directory.Exists(path))
                    {
                        continue;
                    }

                    var files = Directory.GetFiles(path, "*.xml");
                    foreach(var file in files)
                    {

                        FileAttributes fa = File.GetAttributes(file);

                        if((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            Dev2Logger.Log.Info("Removed READONLY Flag from [ " + file + " ]");
                            File.SetAttributes(file, FileAttributes.Normal);
                        }

                        // Use the FileStream class, which has an option that causes asynchronous I/O to occur at the operating system level.  
                        // In many cases, this will avoid blocking a ThreadPool thread.  
                        var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true);
                        streams.Add(new ResourceBuilderTO { FilePath = file, FileStream = sourceStream });

                    }
                }

                // Use the parallel task library to process file system ;)
                Parallel.ForEach(streams.ToArray(), currentItem =>
                {

                    XElement xml = null;

                    try
                    {
                        xml = XElement.Load(currentItem.FileStream);
                    }
                    catch(Exception e)
                    {
                        Dev2Logger.Log.Error("Resource [ " + currentItem.FilePath + " ] caused " + e.Message);
                    }

                    StringBuilder result = xml.ToStringBuilder();

                    var isValid = xml != null && HostSecurityProvider.Instance.VerifyXml(result);
                    if(isValid)
                    {
                        var resource = new Resource(xml)
                        {
                            FilePath = currentItem.FilePath                     
                        };

                        //2013.08.26: Prevent duplicate unassigned folder in save dialog and studio explorer tree by interpreting 'unassigned' as blank
                        if(resource.ResourcePath.ToUpper() == "UNASSIGNED")
                        {
                            resource.ResourcePath = string.Empty;
                            // DON'T FORCE A SAVE HERE - EVER!!!!
                        }
                        xml = _resourceUpgrader.UpgradeResource(xml, Assembly.GetExecutingAssembly().GetName().Version, (a =>
                        {
                            StringBuilder updateXml = a.ToStringBuilder();
                            var signedXml = HostSecurityProvider.Instance.SignXml(updateXml);
                            signedXml.WriteToFile(currentItem.FilePath, Encoding.UTF8);
                        }));
                        if(resource.IsUpgraded)
                        {
                            // Must close the source stream first and then add a new target stream 
                            // otherwise the file will be remain locked
                            currentItem.FileStream.Close();

                            xml = resource.UpgradeXml(xml, resource);

                            StringBuilder updateXml = xml.ToStringBuilder();
                            var signedXml = HostSecurityProvider.Instance.SignXml(updateXml);
                            signedXml.WriteToFile(currentItem.FilePath, Encoding.UTF8);
                        }
                        if(resource.VersionInfo == null)
                        {
                            
                        }

                        lock(_addLock)
                        {
                            AddResource(resource, currentItem.FilePath);
                        }
                    }
                    else
                    {
                        Dev2Logger.Log.Debug(string.Format("'{0}' wasn't loaded because it isn't signed or has modified since it was signed.", currentItem.FilePath));
                    }
                });
            }
            finally
            {
                // Close all FileStream instances in a finally block after the tasks are complete. 
                // If each FileStream was instead created in a using statement, the FileStream 
                // might be disposed of before the task was complete
                foreach(var stream in streams)
                {
                    stream.FileStream.Close();
                }
            }
        }

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="res">The res.</param>
        /// <param name="filePath">The file path.</param>
        private void AddResource(IResource res, string filePath)
        {
            if(!_addedResources.Contains(res.ResourceID))
            {
                _resources.Add(res);
                _addedResources.Add(res.ResourceID);
            }
            else
            {
                var dupRes = _resources.Find(c => c.ResourceID == res.ResourceID);
                if(dupRes != null)
                {
                    Dev2Logger.Log.Debug(
                        string.Format(
                            "Resource '{0}' from file '{1}' wasn't loaded because a resource with the same name has already been loaded from file '{2}'.",
                            res.ResourceName, filePath, dupRes.ResourceName));
                }
                else
                {
                    Dev2Logger.Log.Debug(
                        string.Format(
                            "Resource '{0}' from file '{1}' wasn't loaded because a resource with the same name has already been loaded but cannot find its location.",
                            res.ResourceName, filePath));
                }
            }
        }

    }
}
