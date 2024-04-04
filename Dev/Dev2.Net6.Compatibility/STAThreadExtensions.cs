using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Net6.Compatibility
{
    public static class STAThreadExtensions
    {

        public static async Task RunSTAThreadAsync(Action method)
        {
            await RunSTAThread(() => method());
        }

        public static Task RunSTAThread(Action action)
        {
#if !NETFRAMEWORK
            var tcs = new TaskCompletionSource();
#else
			var tcs = new TaskCompletionSource<bool>();
#endif
			System.Threading.Thread runThread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                try
                {
                    action();
#if !NETFRAMEWORK
                    tcs.TrySetResult();
#else
					tcs.TrySetResult(true);
#endif
				}
				catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }));
            runThread.SetApartmentState(System.Threading.ApartmentState.STA);
            runThread.Start();
            return tcs.Task;
        }

        public static void RunAsSTA(Action method)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                method();
            else
            {
                //var task = Task.Run(() => await RunAsSTAInternal(method).Wait());

                //while (!task.IsCompleted) { }

                //if (task.Status == TaskStatus.Faulted)
                //{

                //    foreach (var ex in task.Exception?.InnerExceptions ?? new(Array.Empty<Exception>()))
                //    {
                //        //// Handle the custom exception.
                //        //if (ex.GetType() == expectedExceptionType)
                //        //{
                //        //    throw ex;
                //        //}
                //        //// Rethrow any other exception.
                //        //else
                //        //{
                //        //    throw ex;
                //        //}
                //        throw ex;
                //    }
                //}

                RunSTAThreadAsync(method).Wait();



                //CallRunAsSTA(method);
            }
        }
        
        public static async System.Threading.Tasks.Task RunAsSTAInternal(Action method)
        {
            // Use a semaphore to prevent the [TestMethod] from returning prematurely.
            SemaphoreSlim ss = new SemaphoreSlim(1);
            await ss.WaitAsync();

            Thread thread = new Thread(() =>
            {
                method();
                ss.Release();
            });


            // Just make sure to set the apartment state BEFORE starting the thread:
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            await ss.WaitAsync();

            Console.WriteLine("All done!");
            //return 0;
        }

    }

    /// <summary>
    /// Indicates that an exception is expected during test method execution.
    /// It also considers the AggregateException and check if the given exception is contained inside the InnerExceptions.
    /// This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ExpectedAggregateExceptionAttribute : ExpectedExceptionBaseAttribute
    {
        protected override string NoExceptionMessage
        {
            get
            {
                return string.Format("{0} - {1}, {2}, {3}",
                    "<TestClassNameMissing>",//base.TestContext.FullyQualifiedTestClassName,
                        "<TestNameMissing>", //base.TestContext.TestName,
                    this.ExceptionType.FullName,
                    base.NoExceptionMessage);
            }
        }

        /// <summary>
        /// Gets the expected exception type.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Type"/> object.
        /// </returns>
        public Type ExceptionType { get; private set; }

        public bool AllowDerivedTypes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedAggregateExceptionAttribute"/> class with and expected exception type and a message that describes the exception.
        /// </summary>
        /// <param name="exceptionType">An expected type of exception to be thrown by a method.</param>
        public ExpectedAggregateExceptionAttribute(Type exceptionType)
            : this(exceptionType, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedAggregateExceptionAttribute"/> class with and expected exception type and a message that describes the exception.
        /// </summary>
        /// <param name="exceptionType">An expected type of exception to be thrown by a method.</param>
        /// <param name="noExceptionMessage">If the test fails because an exception was not thrown, this message is included in the test result.</param>
        public ExpectedAggregateExceptionAttribute(Type exceptionType, string noExceptionMessage)
            : base(noExceptionMessage)
        {
            if (exceptionType == null)
            {
                throw new ArgumentNullException("exceptionType");
            }

            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException("Given type is not an exception.", "exceptionType");
            }

            this.ExceptionType = exceptionType;
        }

        /// <param name="exception">The exception that is thrown by the unit test.</param>
        protected override void Verify(Exception exception)
        {
            Type type = exception.GetType();

            if (this.AllowDerivedTypes)
            {
                if (!this.ExceptionType.IsAssignableFrom(type))
                {
                    base.RethrowIfAssertException(exception);

                    throw new Exception(string.Format("Test method {0}.{1} threw exception {2}, but exception {3} was expected. Exception message: {4}",
                        "<TestClassNameMissing>",//base.TestContext.FullyQualifiedTestClassName,
                        "<TestNameMissing>", //base.TestContext.TestName,
                        type.FullName,
                        this.ExceptionType.FullName,
                        GetExceptionMsg(exception)));
                }
            }
            else
            {
                if (type == typeof(AggregateException))
                {
                    //foreach (var e in ((AggregateException)exception).InnerExceptions)
                    //{
                    //    if (e.GetType() == this.ExceptionType)
                    //    {
                    //        return;
                    //    }
                    //}
                    if (IsInnerExceptionMatched((AggregateException)exception, this.ExceptionType))
                    {
                        return;
                    }
                }

                if (type != this.ExceptionType)
                {
                    base.RethrowIfAssertException(exception);

                    throw new Exception(string.Format("Test method {0}.{1} threw exception {2}, but exception {3} was expected. Exception message: {4}",
                        "<TestClassNameMissing>",//base.TestContext.FullyQualifiedTestClassName,
                        "<TestNameMissing>", //base.TestContext.TestName,
                        type.FullName,
                        this.ExceptionType.FullName,
                        GetExceptionMsg(exception)));
                }
            }
        }

        private bool IsInnerExceptionMatched(AggregateException ex, Type exceptionType)
        {
            foreach (var e in ex.InnerExceptions)
            {
                if (e.GetType() == this.ExceptionType)
                {
                    return true;
                }
                else if (e is AggregateException)
                {
                    return IsInnerExceptionMatched((AggregateException)e, exceptionType);
                }
            }

            return false;
        }

        private string GetExceptionMsg(Exception ex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;

            for (Exception exception = ex; exception != null; exception = exception.InnerException)
            {
                string str = exception.Message;

                FileNotFoundException notFoundException = exception as FileNotFoundException;
                if (notFoundException != null)
                {
                    str = str + notFoundException.FusionLog;
                }

                stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0}{1}: {2}", flag ? (object)string.Empty : (object)" ---> ", (object)exception.GetType(), (object)str));
                flag = false;
            }

            return ((object)stringBuilder).ToString();
        }
    }
}
