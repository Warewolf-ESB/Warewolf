$SolutionDir = (get-item $PSScriptRoot ).parent.parent.FullName
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir | ? {$_.PSIsContainer -and (($_.Name.StartsWith("Dev2.") -or $_.Name.StartsWith("Warewolf.")) -and $_.Name.EndsWith(".Tests"))} ) {
    $TestAssembliesList = "`"$SolutionDir\" + $file.Name + "\bin\Debug\" + $file.Name + ".dll`" " + $TestAssembliesList 
}
$TestAssembliesList = $TestAssembliesList + "/logger:trx"
Start-Process -FilePath "$env:vs120comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe" -ArgumentList $TestAssembliesList -verb RunAs -WorkingDirectory $SolutionDir -Wait