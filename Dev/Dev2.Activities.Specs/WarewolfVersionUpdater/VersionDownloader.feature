Feature: VersionDownloader
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: New version available choose download
	Given the version of the software on the server is "1.2.3.3"
	And the current version is "1.2.2.2"
	When I start the studio 
	Then choose to download the new version "Download"
	And the file will be saved on the filesystem "true"

Scenario: New version available choose cancel
	Given the version of the software on the server is "1.2.3.3"
	And the current version is "1.2.2.2"
	When I start the studio 
	Then choose to download the new version "Cancel"
	And the file will not be saved on the filesystem "false"

Scenario: New version available choose download and install
	Given the version of the software on the server is "1.2.3.3"
	And the current version is "1.2.2.2"
	When I start the studio 
	Then choose to download the new version "Download and Install"
	And the file will be saved on the filesystem "success"
	And the studio will shutdown and install the new version "true"

Scenario: New version available choose download and download fails
	Given the version of the software on the server is "1.2.3.3"
	And the current version is "1.2.2.2"
	When I start the studio 
	Then choose to download the new version but the download "fail"
	And the file will be not saved on the filesystem "false"

