
foreach ($Class in (Get-ChildItem "$PSScriptRoot\*.cs" -Recurse)) {
    $ClassContent = (Get-Content $Class)
    if ($ClassContent -ne $null) {
        $ClassContent.replace("using NUnit.Framework;", "using Microsoft.VisualStudio.TestTools.UnitTesting;").replace("[TestFixture]", "[TestClass]").replace("[SetUp]", "[TestInitialize]").replace("[TearDown]", "[TestCleanup]").replace("[Test]", "[TestMethod]").replace("driver = new FirefoxDriver();","driver = new InternetExplorerDriver();").replace("using OpenQA.Selenium.Firefox;","using OpenQA.Selenium.IE;") | Set-Content $Class
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
                if ($_ -eq "            Assert.AreEqual(`"`", verificationErrors.ToString());" -and $PreviousLine -ne "            screenRecorder.StopRecording(TestContext);") 
                {
					"            screenRecorder.StopRecording(TestContext);"
                }
                if ($_ -eq "using OpenQA.Selenium.Support.UI;" -and $PreviousLine -ne "using AutoTestSharedTools.VideoRecorder;") 
                {
					"using AutoTestSharedTools.VideoRecorder;"
                }
                $_
                $PreviousLine = $_
    } | Set-Content $Class
}