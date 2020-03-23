/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Serilog;
using System.Text;
using Warewolf.Logging;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogPublisher : ILoggerPublisher
    {
        public SeriLogPublisher(ILogger logger)
        {
            Log.Logger = logger;
        }

        public void Error(string outputTemplate, params object[] args)
        {
            Log.Logger.Error(outputTemplate, args);
        }

        public void Debug(string outputTemplate, params object[] args)
        {
            Log.Logger.Debug(outputTemplate, args);
        }

        public void Fatal(string outputTemplate, params object[] args)
        {
            Log.Logger.Fatal(outputTemplate, args);
        }

        public void Info(string outputTemplate, params object[] args)
        {
            Log.Logger.Information(outputTemplate, args);
        }

        public void Publish(byte[] value) => Info(Encoding.UTF8.GetString(value), null);

        public void Warn(string outputTemplate, params object[] args)
        {
            Log.Logger.Warning(outputTemplate, args);
        }
    }
}