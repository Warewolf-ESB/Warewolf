
foreach ($Class in (Get-ChildItem "$PSScriptRoot\*.cs" -Recurse)) {
    $ClassContent = (Get-Content $Class)
    if ($ClassContent -ne $null) {
        $ClassContent.replace("using NUnit.Framework;", "using Microsoft.VisualStudio.TestTools.UnitTesting;`r`nusing Microsoft.VisualStudio.TestTools.UITesting;").replace("[TestFixture]", "[CodedUITest]").replace("[SetUp]", "[TestInitialize]").replace("[TearDown]", "[TestCleanup]").replace("[Test]", "[TestMethod]") | Set-Content $Class
    }
}