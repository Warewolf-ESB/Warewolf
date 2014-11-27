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
using System.Reflection;
using Vestris.ResourceLib;

// ReSharper disable CheckNamespace

namespace Dev2.Studio.Utils
{
    public static class VersionInfo
    {
        public static string FetchVersionInfo()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            string fileName = asm.Location;
            versionResource.LoadFrom(fileName);

            return versionResource.FileVersion;
        }

        public static Version FetchVersionInfoAsVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            string fileName = asm.Location;
            versionResource.LoadFrom(fileName);

            var v = new Version(versionResource.FileVersion);

            return v;
        }
    }
}