
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vestris.ResourceLib;

namespace VersionWarewolf
{
    public class VersionWarewolf
    {
        public VersionResult VersionFile(string fileName, string version)
        {
            try
            {
                if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName","Cannot have null or blank file name");
                if (String.IsNullOrEmpty(version)) throw new ArgumentNullException("version", "Cannot have null or blank version");
                
                var versionResource = new VersionResource();

                versionResource.LoadFrom(fileName);
                versionResource.FileVersion = version;
                versionResource.ProductVersion = version;

                versionResource.SaveTo(fileName);
            }
            catch(Exception e)
            {
                return new VersionResult
                {
                    ErrorMessage = fileName+": "+e.Message,
                    Result = false
                };
            }
            return new VersionResult
            {
                ErrorMessage = "",
                Result = true
            };
        }

        public VersionResult VersionFiles(string[] fileNames, string version)
        {
            foreach(var fileName in fileNames)
            {
                VersionResult versionResult = VersionFile(fileName, version);
                if(!versionResult.Result)
                {
                    return versionResult;
                }
            }
            return new VersionResult
            {
                ErrorMessage = "",
                Result = true
            };
        }

        public VersionResult VersionAllInDir(string path, string version)
        {
            IEnumerable<string> filesToVersion = Directory.EnumerateFiles(path).Where(s =>
            {
                string extension = Path.GetExtension(s);
                return extension==".dll" || extension==".exe";
            });
            return VersionFiles(filesToVersion.ToArray(), version);
        }
    }

    public class VersionResult
    {
        public string ErrorMessage { get; set; }
        public bool Result { get; set; }
    }
}
