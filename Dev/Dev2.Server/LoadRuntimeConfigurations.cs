/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Runtime.Configuration;
using System;

namespace Dev2
{
    public class LoadRuntimeConfigurations
    {
        private readonly IWriter _writer;

        public LoadRuntimeConfigurations(IWriter writer) 
            :this(writer, null)
        {
            
        }

        public LoadRuntimeConfigurations(IWriter writer, ISettingsProvider settingsProvider)
        {
            _writer = writer;
            var _settingsProvider = settingsProvider;
        }

        void LoadSettingsProvider()
        {
            _writer.Write("Loading settings provider...  ");
            if (EnvironmentVariables.WebServerUri != null)
            {
                Runtime.Configuration.SettingsProvider.WebServerUri = EnvironmentVariables.WebServerUri;
                _writer.WriteLine("done.");
            }
            else
            {
                _writer.WriteLine("fail.");
            }
        }

        public void Execute()
        {
            LoadSettingsProvider();
        }
    }
}
