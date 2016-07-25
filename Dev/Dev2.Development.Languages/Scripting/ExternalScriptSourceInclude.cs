/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Jurassic;

namespace Dev2.Development.Languages.Scripting
{
    public class ExternalScriptSourceInclude : IExternalScriptSourceInclude
    {
        private static IList<FileScriptSource> FileScriptSources = new List<FileScriptSource>();

        public IList<FileScriptSource> GetFileScriptSources()
        {
            return FileScriptSources;
        }

        public void AddPaths(string paths)
        {
            var parts = paths.Split(';');
            foreach (var path in parts)
            {
                var fileScriptSource = new FileScriptSource(path);
                FileScriptSources.Add(fileScriptSource);
            }
        }
    }

    public interface IExternalScriptSourceInclude
    {
        IList<FileScriptSource> GetFileScriptSources();
        void AddPaths(string paths);
    }
}
