using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public class ActionSteps
    {
        [Given(@"I '(.*)'")]
        [When(@"I '(.*)'")]
        [Then(@"I '(.*)'")]
        public void PerformAnyRecordedAction(string p0)
        {
            List<Type> allTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();
            List<MethodInfo> allFoundMethods = new List<MethodInfo>();
            List<Type> allFoundUIMapTypes = new List<Type>();
            foreach (Type type in allTypes)
            {
                if (type.GetMethod(p0) != null)
                {
                    allFoundMethods.Add(type.GetMethod(p0));
                    allFoundUIMapTypes.Add(type);
                }
            }

            var countAllMethods = allFoundMethods.Count();
            if (countAllMethods == 1)
            {
                var foundActionRecording = allFoundMethods.First();
                var foundUIMapType = allFoundUIMapTypes.First();
                foundActionRecording.Invoke(Activator.CreateInstance(foundUIMapType), new object[] { });
            }
            else if (countAllMethods > 1)
            {
                throw new InvalidOperationException("Cannot distinguish between duplicated action recordings, named '" + p0 + "' in different UI maps.");
            }
            else if (countAllMethods <= 0)
            {
                throw new InvalidOperationException("Cannot find action recording named '" + p0 + "' in any UI map.");
            }
        }
    }
}
