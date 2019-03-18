#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
            vm.GetStudioLogFile();
            vm.ServerLogFile = await ExceptionViewModel.GetServerLogFile();

            vm.Exception.Clear();
            vm.Exception.Add(Create(e, isCritical == ErrorSeverity.Critical));
            return vm;
        }
    }
}
