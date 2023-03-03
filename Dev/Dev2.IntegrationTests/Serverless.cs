using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using TestBase;

namespace Dev2.Integration.Tests
{
	[TestClass]
	public class Serverless
	{
		[TestMethod]
		[TestCategory("Serverless")]
		public void Execution_Expected_CertainTimeElapsed()
		{
			//------------Setup for test--------------------------
			var waitTime = 120;
			var expectedTimeElapsed = TimeSpan.FromSeconds(waitTime).TotalMilliseconds;
			var expectedExtraTimeElapsed = TimeSpan.FromSeconds(20).TotalMilliseconds;

			//------------Execute Test---------------------------
			var result = PerformanceGadgeWithReturn.TimedExecution((inputPath) => TestHelper.PostDataToWebserver(inputPath), waitTime);

			//------------Assert Results-------------------------			
			Assert.IsTrue(result <= expectedTimeElapsed+ expectedExtraTimeElapsed, $"TimeElapsed: {result} is greater then the expected: {expectedTimeElapsed} plus the extra time expected for the request {expectedExtraTimeElapsed}");
		}
	}

	public class WaitResult
	{
		public string Result;
	}

	internal class PerformanceGadgeWithReturn
	{
		internal static long TimedExecution(Func<string, string> method, int waitTime)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			var result = method.Invoke($"http://{Environment.GetEnvironmentVariable("ComputerName")}:3142/secure/" + "Wait.json?WaitSeconds=" + waitTime.ToString());
			stopWatch.Stop();
			var json = Newtonsoft.Json.JsonConvert.DeserializeObject<WaitResult>(result);
			Assert.AreEqual("Wait Successful", json.Result);
			return stopWatch.ElapsedMilliseconds;
		}
	}
}
