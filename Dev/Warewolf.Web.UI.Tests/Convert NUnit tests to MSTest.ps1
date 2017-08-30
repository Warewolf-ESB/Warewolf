
foreach ($Class in (Get-ChildItem "$PSScriptRoot\*.cs" -Recurse)) {
    $ClassContent = (Get-Content $Class)
    if ($ClassContent -ne $null) {
        $ClassContent.replace("using NUnit.Framework;", "using Microsoft.VisualStudio.TestTools.UnitTesting;").replace("[TestFixture]", "[TestClass]").replace("[SetUp]", "[TestInitialize]").replace("[TearDown]", "[TestCleanup]").replace("[Test]", "[TestMethod]`r`n        [DeploymentItem(@`"avformat-57.dll`")]`r`n        [DeploymentItem(@`"avutil-55.dll`")]`r`n        [DeploymentItem(@`"swresample-2.dll`")]`r`n        [DeploymentItem(@`"swscale-4.dll`")]`r`n        [DeploymentItem(@`"avcodec-57.dll`")]") | Set-Content $Class
    }
}
foreach ($Class in (Get-ChildItem "$PSScriptRoot\*.cs" -Recurse)) {
        $PreviousLine = ""
        (Get-Content $Class) | 
            Foreach-Object {
                if ($_ -eq "        private IWebDriver driver;" -and $PreviousLine -ne "        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();") 
                {
					"        public TestContext TestContext { get; set; }"
					"        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();"
                }
                if ($_ -eq "            verificationErrors = new StringBuilder();" -and $PreviousLine -ne "            screenRecorder.StartRecording(TestContext);") 
                {
					"            screenRecorder.StartRecording(TestContext);"
                }
                if ($_ -eq "            Assert.AreEqual(`"`", verificationErrors.ToString());" -and $PreviousLine -ne "            screenRecorder.StopRecording(TestContext.CurrentTestOutcome);") 
                {
					"            screenRecorder.StopRecording(TestContext.CurrentTestOutcome);"
                }
                if ($_ -eq "using OpenQA.Selenium.Support.UI;" -and $PreviousLine -ne "using Warewolf.Web.UI.Tests.ScreenRecording;") 
                {
					"using Warewolf.Web.UI.Tests.ScreenRecording;"
                }
                $_
                $PreviousLine = $_
    } | Set-Content $Class
}