#include <Bootstrapper.h>

enum DotNetProfile
{
	DotNetProfileDefault,
	DotNetProfileFull,
	DotNetProfileClient
};

inline bool isInstalledDotNet(SharpSetup::PrerequisiteManager* prerequisiteManager, SharpSetup::Version version, unsigned int spLevel = 0, DotNetProfile profile = DotNetProfileDefault)
{
	using namespace SharpSetup;
	std::wstring key;
	if(version==L"2.0")
		key=L"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v2.0.50727";
	else if(version==L"3.0")
		key=L"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.0";
	else if(version==L"3.5")
		key=L"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.5";
	else if(version==L"4.0" && profile == DotNetProfileClient)
		key=L"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client";
	else if(version==L"4.0" || version==L"4.5")
		key=L"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full";
	else
		throw SharpSetup::Exception(L"Unsupported version/profile combination");
	std::wstring spValueName;
	if(version==L"2.0" || version==L"3.0" || version==L"3.5")
		spValueName = L"SP";
	else
		spValueName = L"Servicing";
	DWORD installed = SharpSetup::Registry::readDWORD(key, L"Install", 0);
    if (spLevel == 0 && version == L"3.0")
		installed = SharpSetup::Registry::readDWORD(key+L"\\Setup", L"InstallSuccess", 0);
    if (version == L"4.5")
		installed = SharpSetup::Registry::readDWORD(key, L"Release", 0) >= 378389;
	DWORD spInstalled = SharpSetup::Registry::readDWORD(key, spValueName, 0);
	if(installed == 1 && spInstalled >= spLevel)
	{
		if(prerequisiteManager)
		{
			if(version==L"4.0" && System::getDllVersion(System::getSpecialDirectory(SpecialDirectories::System)+L"\\mscoree.dll")<L"4.0")
				prerequisiteManager->setRebootRequired();
		}
		return true;
	}
	else
		return false;
}

__declspec(deprecated("Please add pointer to SharpSetup::PrerequisiteManager as first argument")) inline bool isInstalledDotNet(SharpSetup::Version version, unsigned int spLevel = 0, DotNetProfile profile = DotNetProfileDefault)
{
	return isInstalledDotNet(NULL, version, spLevel, profile);
}

inline void installDotNet(SharpSetup::PrerequisiteManager* prerequisiteManager, SharpSetup::Version version, int spLevel = 0, DotNetProfile profile = DotNetProfileDefault, std::wstring language = L"")
{
	static std::wstring sourceDirectory=L"";
	std::wstring productName = L"dotnet"+SharpSetup::Utilities::toString(version.getMajor())+SharpSetup::Utilities::toString(version.getMinor());
	prerequisiteManager->queueInstall(productName, SharpSetup::Callable::bind(installDotNet, prerequisiteManager, version, spLevel, profile, language));
	SharpSetup::OsVersion winver = SharpSetup::System::getWindowsVersion();
	if(winver.getArchitecture() != SharpSetup::X86_32 && winver.getArchitecture() != SharpSetup::X86_64)
		throw SharpSetup::Exception(&prerequisiteManager->getBootstrapperInfoAdapter(), L"#GENERIC_UNSUPPORTED_ARCHITECTURE");
	if(!language.empty() && version < L"3.5")
		throw SharpSetup::Exception(L"Cannot specify language for .NET Framework prior to 3.5.");
	if(!language.empty() && language.length()!=3)
		throw SharpSetup::Exception(L"Language parameter should be three letter language code (eg. 'ENU').");

	std::wstring filename = L"dotnetfx";
	filename += SharpSetup::Utilities::toString(version.getMajor());
	filename += SharpSetup::Utilities::toString(version.getMinor());
	if(spLevel)
		filename += L"sp" + SharpSetup::Utilities::toString(spLevel);
	// NetFx 3.5 and 4.0 do not specify architecture
	if(version == L"4.0" && profile == DotNetProfileClient){
		filename = L"dotNetFx40_Client_x86_x64";
		//filename += L"_client";
	}else if(version == L"4.0"){
		filename = L"dotNetFx40_Client_x86_x64";
		//filename += L"_full";
	}else if (version == L"3.5" || version == L"4.5"){
		filename += L"";
	}else{
		filename += (winver.getArchitecture() == SharpSetup::X86_32 ? L"_x86" : L"_x64");
	}
	filename += L".exe";

	switch(prerequisiteManager->getStage())
	{
	case SharpSetup::Prepare:
		if((version == L"2.0" || version == L"3.0" || version == L"3.5") && winver >= L"6.2") //Windows 8/Windows 2012 or newer
		{
			std::wstring productName=L"win8";
			if(SharpSetup::System::getWindowsVersion().getProductType()&SharpSetup::ProductType::Server)
				productName=L"win2012";
			std::wstring mbText=prerequisiteManager->getBootstrapperInfoAdapter().getTranslation(L"#DOTNET_PREREQ_PRODUCT_DVD_REQUIRED_TEXT", L"#PRODUCT_NAME_"+productName);
			std::wstring mbCaption=prerequisiteManager->getBootstrapperInfoAdapter().getTranslation(L"#DOTNET_PREREQ_PRODUCT_DVD_REQUIRED_CAPTION", L"#PRODUCT_NAME_"+productName);
			do {
				std::vector<std::wstring> drives=SharpSetup::System::wmiExecuteColumnString(L"SELECT Drive FROM Win32_CDROMDrive WHERE MediaLoaded=true");
				for(unsigned int i=0;i<drives.size();i++)
				{
					DWORD attributes=GetFileAttributes((drives[i]+L"\\sources\\sxs").c_str());
					if((attributes != INVALID_FILE_ATTRIBUTES) && (attributes & FILE_ATTRIBUTE_DIRECTORY))
					{
						sourceDirectory=drives[i]+L"\\sources\\sxs";
						break;
					}
				}
				if(!sourceDirectory.empty())
					break;
			} while(prerequisiteManager->getBootstrapperInfoAdapter().messageBox(mbText, mbCaption, MB_YESNO|MB_DEFBUTTON2)==IDYES);
			filename=L"";
		}
		else if(version == L"2.0")
		{
			if(L"5.0" <= winver && winver < L"5.1" && winver.getSpLevel() < 3) //Windows 2000
				prerequisiteManager->prerequisiteMissing(productName, L"win2000$sp3");
			if(L"5.0" <= winver && winver < L"5.1" && spLevel > 0 && !SharpSetup::System::isSymbolDefined(L"kernel32.dll", "HeapSetInformation")) //Windows 2000, see KB816542
				prerequisiteManager->prerequisiteMissing(productName, L"win2000sp4ur1");
			if(L"5.1" <= winver && winver < L"5.2" && winver.getSpLevel() < 2) //Windows XP
				prerequisiteManager->prerequisiteMissing(productName, L"winxp$sp2");
		}
		else if(version == L"3.0" || version == L"3.5")
		{
			if(winver < L"5.1" || (L"5.1" <= winver && winver < L"5.2" && winver.getSpLevel() < 2)) //Windows 2000 or Windows XP RTM/SP1
				prerequisiteManager->prerequisiteMissing(productName, L"winxp$sp2");
			if(L"5.2" <= winver && winver < L"5.3" && winver.getSpLevel() < 1) //Windows 2003 RTM
				prerequisiteManager->prerequisiteMissing(productName, L"win2003$sp1");
		}
		else if(version == L"4.0")
		{
			if(winver < L"5.1" || (L"5.1" <= winver && winver < L"5.2" && winver.getSpLevel() < 3)) //Windows 2000 or Windows XP RTM/SP1/SP2
				prerequisiteManager->prerequisiteMissing(productName, L"winxp$sp3");
			if(L"5.2" <= winver && winver < L"5.3" && winver.getSpLevel() < 2) //Windows 2003 RTM/SP1
				prerequisiteManager->prerequisiteMissing(productName, L"win2003$sp2");
			if(L"6.0" <= winver && winver < L"6.1" && winver.getSpLevel() < 1)
				prerequisiteManager->prerequisiteMissing(productName, L"winvista$sp1");
		}
		else if(version == L"4.5")
		{
			if(winver < L"6.0" || (L"6.0" <= winver && winver < L"6.1" && winver.getSpLevel() < 2)) //Windows Vista RTM/SP1
				prerequisiteManager->prerequisiteMissing(productName, L"winvista$sp2");
		}
		if(!filename.empty())
			prerequisiteManager->requestFile(filename);
		break;
	case SharpSetup::Install:
		prerequisiteManager->getBootstrapperInfoAdapter().setProgress(-1);
		DWORD exitCode;
		std::wstring languageCommandLineOption = L"";
		if(!language.empty())
			languageCommandLineOption = L" /lang:"+language;
		if((version == L"2.0" || version == L"3.0" || version == L"3.5") && winver >= L"6.2") //Windows 8/Windows 2012 or newer
		{
			std::wstring dismCommand=L"/online /enable-feature /featurename:netfx3 /all";
			if(!sourceDirectory.empty())
				dismCommand+=L" /limitaccess /source:"+sourceDirectory;
			exitCode=SharpSetup::System::runCommand(SharpSetup::System::getSpecialDirectory(SharpSetup::SpecialDirectories::System)+L"\\dism.exe", dismCommand, SharpSetup::RunOptions::NoWindow|SharpSetup::RunOptions::NoFsRedirection);
		}
		else if(version == L"2.0" && spLevel == 0)
			exitCode = SharpSetup::System::runCommand(prerequisiteManager->getFilePath(filename), L"/q:a \"/c:install /q /l\"");
		else
			exitCode = SharpSetup::System::runCommand(prerequisiteManager->getFilePath(filename), L"/q /norestart"+languageCommandLineOption);
		if(exitCode==ERROR_SUCCESS_REBOOT_REQUIRED)
			prerequisiteManager->setRebootRequired();
		else if(exitCode)
			prerequisiteManager->installationFailed(productName, exitCode);
		break;
	}
}

inline bool isInstalledMsi(SharpSetup::Version version)
{
	if(version != L"3.1")
		throw SharpSetup::Exception(L"Installation of Windows Installer other than version 3.1 not supported.");
	return SharpSetup::System::getDllVersion(L"msi.dll") >= L"3.1";
}

inline void installMsi(SharpSetup::PrerequisiteManager* prerequisiteManager, SharpSetup::Version version)
{
	prerequisiteManager->queueInstall(L"msi31", SharpSetup::Callable::bind(installMsi, prerequisiteManager, version));
	if(version != L"3.1")
		throw SharpSetup::Exception(L"Installation of Windows Installer other than version 3.1 not supported.");
	SharpSetup::OsVersion winver = SharpSetup::System::getWindowsVersion();
	if(winver.getArchitecture() != SharpSetup::X86_32 && winver.getArchitecture() != SharpSetup::X86_64)
		throw SharpSetup::Exception(&prerequisiteManager->getBootstrapperInfoAdapter(), L"#GENERIC_UNSUPPORTED_ARCHITECTURE");

	std::wstring filename = L"WindowsInstaller-KB893803-v2-x86.exe";
	switch(prerequisiteManager->getStage())
	{
	case SharpSetup::Prepare:
		if(L"5.0" <= winver && winver < L"5.1" && winver.getSpLevel() < 3) //Windows 2000
			prerequisiteManager->prerequisiteMissing(L"msi31", L"win2000$sp3");
		else if(winver.getArchitecture() == SharpSetup::X86_32 && winver < L"5.3") //32-bit Windows 2000 & Windows XP & Windows 2003
			prerequisiteManager->requestFile(filename);
		else
			throw SharpSetup::Exception(&prerequisiteManager->getBootstrapperInfoAdapter(), L"#INSTALLMSI_SYSTEM_UNSUPPORTED");
		break;
	case SharpSetup::Install:
		prerequisiteManager->getBootstrapperInfoAdapter().setProgress(-1);
		DWORD exitCode = SharpSetup::System::runCommand(prerequisiteManager->getFilePath(filename), L"/quiet /norestart \"/log:" + SharpSetup::System::getCurrentDirectory() + L"\\msi31.log\"");
		if(exitCode != 0 && exitCode != 3010) //ok or reboot required
			prerequisiteManager->installationFailed(L"msi31", exitCode);
		break;
	}
}
