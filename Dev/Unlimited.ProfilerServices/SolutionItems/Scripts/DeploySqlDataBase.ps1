Write-Host "Build Database Project"

$Workspace = $env:WORKSPACE 
$buildpath = -join($Workspace,"\Database\Warewolf.ProfilerDB\Warewolf.ProfilerDB.sqlproj")

msbuild $buildpath /P:Configuration=Release

#################################################################################################
$DatabaseName = "ProfilerDB"
$Source = $env:DBSERVER
$UserName = $env:DBUSERNAME
$Password = $env:DBPASSWORD

$DatabaseConnection = -join("Data Source=",$Source,",1433;","Initial Catalog=",$DatabaseName,";User ID=",$UserName,";Password=",$Password)

Write-Host $DatabaseConnection

$Path = -join($Workspace,"\Database\Warewolf.ProfilerDB\bin\Release\Warewolf.ProfilerDB.dacpac")

Write-Host $DatabaseConnection

#################################################################################################

Write-Host "Deploy Database Project"

Sqlpackage.exe /a:Publish /tcs:$DatabaseConnection /sf:$Path /p:BlockOnPossibleDataLoss=false /p:TreatVerificationErrorsAsWarnings=false

Write-Host $LastExitCode


if($LastExitCode -eq 0){
Write-Host "Passed"
    exit 0
}else{
    throw 'Sql Deployed failed'
    exit 1
}
