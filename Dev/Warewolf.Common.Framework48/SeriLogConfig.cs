/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Serilog;
using Serilog.Core;

namespace Warewolf.Common.Framework48
{
    public class SeriLogConfig : ILoggerConfig
    {
        readonly LoggerConfiguration _loggerConfiguration;
        readonly ILogEventSink _logEventSink;

        public Logger Logger { get => CreateLogger(); }

        public SeriLogConfig(ILogEventSink logEventSink)
        {
            _loggerConfiguration = new LoggerConfiguration();
            _logEventSink = logEventSink;
        }

        internal Logger CreateLogger()
        {
            return _loggerConfiguration.WriteTo.Sink(logEventSink: _logEventSink).CreateLogger();
        }
    }
}
