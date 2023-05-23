1) Copy all required resources (all directories mentioned below) to ProgramData/Warewolf Directory
   a) ServerSettings
   b) Resources
   c) VersionControl  
   d) Execute en-ZA_Culture.ps1 (found at the same location of this Readme.txt) file in powershell command prompt

2) Project "Dev2.Activities.Specs"
	Copy below ones to Dev\Dev2.Activities.Specs\bin\Debug\net6.0-windows\win
	a) Copy "Dev2.Activities.Specs.dll.config" and rename it to "testhost.dll.config" 
	b) Execute en-ZA_Culture.ps1 (found at the same location of this Readme.txt) file in powershell command prompt

3) Project "Dev2.Runtime.WebServer.Tests".
	Copy below ones to Dev\Dev2.Runtime.WebServer\bin\Debug\net6.0-windows\win
	a) Copy "Dev2.Runtime.WebServer.Tests.dll.config" and rename it to "testhost.dll.config"
	b) Add "Warewolf Server.exe.secureconfig"

4) Project "Dev2.Runtime.Tests"
	Copy below ones to Dev\Dev2.Runtime.Tests\bin\Debug\net6.0-windows\win
	a) Copy "Dev2.Runtime.Tests.dll.config" and rename it to "testhost.dll.config" 
	b) Add sni.dll


5) Project "Warewolf.Tools.Specs"
	Copy below ones to Dev\Warewolf.Tools.Specs\bin\Debug\net6.0-windows\win
	a) Copy "Warewolf.Tools.Specs.dll.config" and rename it to "testhost.dll.config" 
	b) Add "Warewolf Server.exe.config"
	c) Add sni.dll

6) Copy SQLite.Interop.dll (found at the same location of this Readme.txt) to the following locations: 

a) Dev\Dev2.Server\bin\Debug\net6.0-windows
b) Dev\Dev2.Activities.Specs\bin\Debug\net6.0-windows\win
c) Dev\Warewolf.Tools.Specs\bin\Debug\net6.0-windows\win
d) Dev\Warewolf.COMIPC.Tests\bin\Debug\net6.0-windows\win-x86
e) Dev\Dev2.Activities.Tests\bin\Debug\net6.0-windows\win
f) Dev\Dev2.Runtime.Tests\bin\Debug\net6.0-windows\win
g) Dev\Dev2.Sql.Tests\bin\Debug\net6.0-windows\win

Note: Make sure to run Warewolf Server.exe (server) before tests dependant on it are executed.


7) Before running tests of Warewolf.Security.Specs project, setup as below

  a) Create new user "SecuritySpecsUser" with pwd "ASfas123@!fda" and make it member of "Public" and "Users" group
  
  b) Copy secure.config (found at the "Warewolf.Security.Specs_Setup" directory) file in the Directory "%Programdata%\Warewolf\Server Settings"

8) Before running tests of Dev2.TaskScheduler.Wrappers.Tests project, setup as below

  a) Create new user "LocalSchedulerAdmin" with pwd "987Sched#@!"
