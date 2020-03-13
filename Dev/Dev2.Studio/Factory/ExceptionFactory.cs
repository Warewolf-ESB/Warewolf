#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Model;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Threading;


namespace Dev2.Studio.Factory
{
    /// <summary>
    /// Factory used to create a wrapper around exceptions
    /// </summary>
    /// <author>jurie.smit</author>
    /// <date>2013/01/15</date>
    public static class ExceptionFactory
    {
        /// <summary>
        /// Creates a UI wrapper the specified exception (recursively).
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        /// <param name="isCritical">Will append the critical error text to the message if true</param>
        public static ExceptionUiModel Create(Exception exception) => Create(exception, false);
        public static ExceptionUiModel Create(Exception exception, bool isCritical)
        {
            ExceptionUiModel uiModel;
            if (isCritical)
            {
                uiModel = new ExceptionUiModel { Message = StringResources.CriticalExceptionMessage };
                uiModel.Exception.Add(Create(exception));
            }
            else
            {
                uiModel = new ExceptionUiModel { Message = StringResources.ErrorPrefix + exception.Message };
            }

            if (exception.InnerException != null)
            {
                uiModel.Exception.Add(Create(exception.InnerException));
            }

            return uiModel;
        }
        
        public static StringBuilder CreateStringValue(Exception exception) => CreateStringValue(exception, null, false);
        public static StringBuilder CreateStringValue(Exception exception, StringBuilder builder) => CreateStringValue(exception, builder, false);
        public static StringBuilder CreateStringValue(Exception exception, StringBuilder builder, bool critical)
        {
            var appendStackTrace = false;
            if (builder == null)
            {
                builder = new StringBuilder();
                appendStackTrace = true;
            }

            if (critical)
            {
                builder.AppendLine(StringResources.CriticalExceptionMessage);
            }

            builder.AppendLine("Exception: " + exception.Message);

            if (exception.InnerException != null)
            {
                CreateStringValue(exception.InnerException, builder);
            }

            if (appendStackTrace)
            {
                builder.AppendLine("StackTrace:");
                builder.AppendLine(exception.StackTrace);
                
                var fullStackTrace = Environment.NewLine + Environment.NewLine + "Additional Trace Info" + Environment.NewLine + Environment.NewLine;
                var theStackTrace = new StackTrace();
                for (int j = theStackTrace.FrameCount - 1; j >= 0; j--)
                {
                    var module = theStackTrace.GetFrame(j).GetMethod().Module.ToString();
                    if (module != "WindowsBase.dll" && module != "CommonLanguageRuntimeLibrary")
                    {
                        fullStackTrace += "--> " + theStackTrace.GetFrame(j).GetMethod().Name + " (" + theStackTrace.GetFrame(j).GetMethod().Module + ")";
                    }
                }
                builder.Append(fullStackTrace);
            }
            return builder;
        }

        public static Func<string, string> GetUniqueOutputPath { get => getUniqueOutputPath; set => getUniqueOutputPath = value; }
        static Func<string, string> getUniqueOutputPath = extension => FileHelper.GetUniqueOutputPath(extension);

        public static async Task<IExceptionViewModel> CreateViewModel(Exception e, IServer server) => await CreateViewModel(e, server, ErrorSeverity.Default);
        public static async Task<IExceptionViewModel> CreateViewModel(Exception e, IServer server, ErrorSeverity isCritical)
        {
            var vm = new ExceptionViewModel(new AsyncWorker())
            {
                OutputText = CreateStringValue(e, null, true).ToString(),
                StackTrace = e.StackTrace,
                OutputPath = GetUniqueOutputPath?.Invoke(".txt"),
                DisplayName = isCritical == ErrorSeverity.Critical ? StringResources.CritErrorTitle : StringResources.ErrorTitle
            };

            try
            {
                vm.GetStudioLogFile();
                vm.ServerLogFile = await ExceptionViewModel.GetServerLogFile();
            } catch
            {
                // could not get log data, we should still try to return
            }
            vm.Exception.Clear();
            vm.Exception.Add(Create(e, isCritical == ErrorSeverity.Critical));
            return vm;
        }
    }
}
