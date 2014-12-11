
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
using System.Text;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Model;
using Dev2.Studio.ViewModels.Diagnostics;

// ReSharper disable CheckNamespace
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
        public static ExceptionUiModel Create(Exception exception, bool isCritical = false)
        {
            ExceptionUiModel uiModel;
            if(isCritical)
            {
                uiModel = new ExceptionUiModel { Message = Resources.Languages.Core.CriticalExceptionMessage };
                uiModel.Exception.Add(Create(exception));
            }
            else
            {
                uiModel = new ExceptionUiModel { Message = Resources.Languages.Core.ErrorPrefix + exception.Message };
            }

            if(exception.InnerException != null)
            {
                uiModel.Exception.Add(Create(exception.InnerException));
            }

            return uiModel;
        }

        /// <summary>
        /// Creates the string value (recursively).
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="builder">The builder to use - null if not recursive.</param>
        /// <param name="critical"></param>
        /// <returns></returns>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public static StringBuilder CreateStringValue(Exception exception, StringBuilder builder = null, bool critical = false)
        {
            var appendStackTrace = false;
            if(builder == null)
            {
                builder = new StringBuilder();
                appendStackTrace = true;
            }

            if(critical)
            {
                builder.AppendLine(Resources.Languages.Core.CriticalExceptionMessage);
            }

            builder.AppendLine("Exception: " + exception.Message);

            if(exception.InnerException != null)
            {
                CreateStringValue(exception.InnerException, builder);
            }

            if(appendStackTrace)
            {
                builder.AppendLine("StackTrace:");
                builder.AppendLine(exception.StackTrace);

                // 14th Feb 2013
                // Added by Michael to assist with debugging
                string fullStackTrace = Environment.NewLine + Environment.NewLine + "Additional Trace Info" + Environment.NewLine + Environment.NewLine;
                StackTrace theStackTrace = new StackTrace();
                for(int j = theStackTrace.FrameCount - 1; j >= 0; j--)
                {
                    string module = theStackTrace.GetFrame(j).GetMethod().Module.ToString();
                    if(module != "WindowsBase.dll" && module != "CommonLanguageRuntimeLibrary")
                    {

                        fullStackTrace += "--> " + theStackTrace.GetFrame(j).GetMethod().Name + " (" + theStackTrace.GetFrame(j).GetMethod().Module + ")";
                    }
                }
                builder.Append(fullStackTrace);
            }


            return builder;
        }

        public static Func<string, string> GetUniqueOutputPath = extension => FileHelper.GetUniqueOutputPath(extension);
        public static Func<IEnvironmentModel, string> GetServerLogTempPath = environmentModel => environmentModel.ResourceRepository.GetServerLogTempPath(environmentModel);
        public static Func<string> GetStudioLogTempPath = () => FileHelper.GetStudioLogTempPath();

        /// <summary>
        /// Creates the exception view model.
        /// </summary>
        /// <param name="e">The exception for this viewmodel.</param>
        /// <param name="environmentModel">The environment model.</param>
        /// <param name="isCritical">The severity of the error.</param>
        /// <returns></returns>
        /// <date>2013/01/16</date>
        /// <author>
        /// Jurie.smit
        /// </author>
        public static IExceptionViewModel CreateViewModel(Exception e, IEnvironmentModel environmentModel, ErrorSeverity isCritical = ErrorSeverity.Default)
        {
            // PBI 9598 - 2013.06.10 - TWR : added environmentModel parameter
            var vm = new ExceptionViewModel
                {
                    OutputText = CreateStringValue(e, null, true).ToString(),
                    StackTrace = e.StackTrace,
                    OutputPath = GetUniqueOutputPath(".txt"),
                    ServerLogTempPath = GetServerLogTempPath(environmentModel),
                    StudioLogTempPath = GetStudioLogTempPath(),
                    DisplayName = isCritical == ErrorSeverity.Critical ? Resources.Languages.Core.CritErrorTitle : Resources.Languages.Core.ErrorTitle,
                    Critical = isCritical == ErrorSeverity.Critical
                };

            var attachedFiles = new Dictionary<string, string>();

            if(!string.IsNullOrWhiteSpace(vm.ServerLogTempPath))
            {
                attachedFiles.Add("ServerLog", vm.ServerLogTempPath);
            }

            if(!string.IsNullOrWhiteSpace(vm.StudioLogTempPath))
            {
                attachedFiles.Add("StudioLog", vm.StudioLogTempPath);
            }

            vm.FeedbackAction = FeedbackFactory.CreateEmailFeedbackAction(attachedFiles, environmentModel);
            vm.Exception.Clear();
            vm.Exception.Add(Create(e, isCritical == ErrorSeverity.Critical));
            return vm;
        }
    }
}
