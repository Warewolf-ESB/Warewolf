using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UnittestingUtils
{
   
    public class NullArgumentConstructorHelper
    {
        public static void AssertNullConstructor(object[] parameters, Type type)
        {
            bool thrown = false;
            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    var parametersToUse = createParams(parameters, i);
                    if (parameters[i].GetType().IsPrimitive || parameters[i].GetType().IsEnum)
                    {
                        thrown = true;
                    }
                    else
                    Activator.CreateInstance(type, parametersToUse);

                }
                catch (TargetInvocationException e)
                {
                    if (null != e.InnerException as ArgumentNullException)
                        thrown = true;
                }

                Assert.IsTrue(thrown);
                thrown = false;
            }
        }

        static object[] createParams(object[] parameters, int i)
        {
            object[] val = parameters.Clone() as object[];
            Debug.Assert(val != null, "val != null");
            val[i] = null;
            return val;
        }

    }
}
