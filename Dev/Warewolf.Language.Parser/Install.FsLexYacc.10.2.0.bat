CD /d "%~dp0."
IF NOT EXIST ..\packages mkdir ..\packages
IF NOT EXIST ..\packages\FsLexYacc.10.2.0 (
    dotnet add package FsLexYacc --version 10.2.0 --package-directory ..\packages
	move /Y ..\packages\fslexyacc\10.2.0 ..\packages\FsLexYacc.10.2.0
	move /Y ..\packages\fslexyacc.runtime\10.2.0 ..\packages\FsLexYacc.runtime.10.2.0
	move /Y ..\packages\fsharp.core\4.5.2 ..\packages\FSharp.Core.4.5.2
	rmdir ..\packages\fslexyacc
	rmdir ..\packages\fslexyacc.runtime
	rmdir ..\packages\fsharp.core
)