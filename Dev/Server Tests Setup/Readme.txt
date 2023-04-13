1) Project "Dev2.Activities.Designers" should have testhost.config file.
	Add "testhost.config" with contents of "Dev2.Activities.Designers.dll.config"


2) Project "Dev2.Runtime.WebServer.Tests" should have testhost.config file.
	Add "testhost.config" with contents of "Dev2.Runtime.WebServer.Tests.dll.config"


3) Project "Dev2.Runtime.Tests" should have testhost.config file.
	Add "testhost.config" with contents of "Dev2.Runtime.Tests.dll.config"


4) Project "Warewolf.Tools.Specs" should have testhost.config file.
	Add "testhost.config" with contents of "Warewolf.Tools.Specs.dll.config"


5) Copy ServerSettings and Resources Directory from these directory to ProgramData/Warewolf Directory

6) Copy SQLite.Interop.dll (found at the same location of this Readme.txt) to the following locations: 

a) Dev\Dev2.Server\bin\Debug\net6.0-windows
b) Dev\Dev2.Activities.Specs\bin\Debug\net6.0-windows\win
c) Dev\Warewolf.Tools.Specs\bin\Debug\net6.0-windows\win
d) Dev\Warewolf.COMIPC.Tests\bin\Debug\net6.0-windows\win-x86

Note: Make sure to run Warewolf Server.exe (server) before tests dependant on it are executed.