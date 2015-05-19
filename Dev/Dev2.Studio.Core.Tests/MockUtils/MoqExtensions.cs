
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
using System.Reflection;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Dev2.Core.Tests.MockUtils
{
    public static class MoqExtensions
    {
        public delegate void RefAction<TRef, in TParam1>(ref TRef refVal, TParam1 param1);


        // Sashen.Naidoo : 14-02-2012 : This method adds ref support to Moq
        /// <summary>
        /// This method can be used to invoke a method call containing a ref parameter in a mocked method
        /// </summary>
        /// <typeparam name="TRef"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TMock"></typeparam>
        /// <param name="mock"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IReturnsResult<TMock> RefCallback<TRef, TParam1, TMock>(this ICallback mock, RefAction<TRef, TParam1> action) where TMock : class
        {
            mock.GetType().Assembly
                .GetType("Moq.MethodCall")
                .InvokeMember("SetCallbackWithArguments", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                              null,
                              mock,
                              new object[] { action });
            return mock as IReturnsResult<TMock>;
        }

        public static ICallback IgnoreRefMatching(this ICallback mock)
        {
            try
            {


                FieldInfo matcherField = typeof(Mock).Assembly
                                                        .GetType("Moq.MethodCall")
                                                        .GetField("argumentMatchers",
                                                        BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                if(matcherField != null)
                {
                    IList<object> argumentMatchers = (IList<object>)matcherField.GetValue(mock);
                    Type refMatcherType = typeof(Mock).Assembly
                                                      .GetType("Moq.Matchers.RefMatcher");
                    FieldInfo equalField = refMatcherType.GetField("equals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                    foreach(object matcher in argumentMatchers)
                    {
                        if(matcher.GetType() == refMatcherType)
                        {
                            if(equalField != null)
                            {
                                equalField.SetValue(matcher, new Func<object, bool>(o => true));
                            }
                        }
                    }
                }

                return mock;
            }
            catch(NullReferenceException)
            {
                return mock;
            }
        }


        public delegate void OutAction<TOut, in TParam1>(out TOut outValue, TParam1 param1);

        public static IReturnsResult<TMock> OutCallback<TOut, TParam1, TMock>(this ICallback mock, OutAction<TOut, TParam1> action) where TMock : class
        {
            mock.GetType().Assembly
                .GetType("Moq.MethodCall")
                .InvokeMember("SetCallbackWithArguments",
                              BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                              null,
                              mock,
                              new object[] { action });
            return mock as IReturnsResult<TMock>;
        }

        public static ICallback IgnoreOutMatching(this ICallback mock)
        {
            try
            {
                FieldInfo matcherField = typeof(Mock).Assembly
                                            .GetType("Moq.MethodCall")
                                            .GetField("argumentMatchers",
                                                       BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                if(matcherField != null)
                {
                    IList<object> argumentMatchers = (IList<object>)matcherField.GetValue(mock);
                    Type outMatcher = typeof(Mock).Assembly
                                                  .GetType("Mock.Matchers.OutMatcher");
                    FieldInfo equalField = outMatcher.GetField("equals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);
                    foreach(object matcher in argumentMatchers)
                    {
                        if(matcher.GetType() == outMatcher)
                        {
                            if(equalField != null)
                            {
                                equalField.SetValue(matcher, new Func<object, bool>(o => true));
                            }
                        }
                    }
                }
                return mock;
            }
            catch(NullReferenceException)
            {
                return mock;
            }
        }
    }
}
