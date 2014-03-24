#include "stdafx.h"
#include <Bootstrapper.h>
#include <Prerequisites.h>

using namespace SharpSetup;
using namespace std;

class SetupBootstrapper : public BootstrapperBase
{
public:
	virtual DWORD OnUnpackingComplete()
	{
		PrerequisiteManager pm(*this);

		// Comment out to avoid doing .NET prereq here ;)
		if(!isInstalledMsi(L"3.1"))
			installMsi(&pm, L"3.1");
		if(!isInstalledDotNet(&pm, L"4.0"))
			installDotNet(&pm, L"4.0", 2);

		pm.getFiles();
		pm.performInstall();
		
		return pm.finalize();
	}
};
CREATEBOOTSTRAPPER(SetupBootstrapper);
