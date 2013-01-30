using System;
using System.Text;
using Dev2.Composition;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.Model;
using Dev2.Studio.ViewModels.Diagnostics;

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
        public static ExceptionUIModel Create(Exception exception)
        {
            var uiModel = new ExceptionUIModel { Message = exception.Message };

            if (exception.InnerException != null)
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
        /// <returns></returns>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public static StringBuilder CreateStringValue(Exception exception, StringBuilder builder = null)
        {
            var appendStackTrace = false;
            if (builder == null)
            {
                builder = new StringBuilder();
                appendStackTrace = true;
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
            }

            return builder;
        }

        /// <summary>
        /// Creates the exception view model.
        /// </summary>
        /// <param name="e">The exception for this viewmodel.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/16</date>
        public static IExceptionViewModel CreateViewModel(Exception e)
        {
            var vm = new ExceptionViewModel
                {
                    OutputText = CreateStringValue(e).ToString(),
                    StackTrace = e.StackTrace,
                    OutputPath = FileHelper.GetUniqueOutputPath(".txt"),
                };

            vm.FeedbackAction = FeedbackFactory.CreateEmailFeedbackAction(vm.OutputPath);
            ImportService.SatisfyImports(vm);
            vm.Exception.Clear();
            vm.Exception.Add(Create(e));
            return vm;
        }
    }
}
