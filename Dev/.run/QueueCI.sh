#!/bin/sh
#
# Queue Bamboo build.
# This script can take a branch name as a commandline parameter.
#
DefaultCIBambooPlanKey="WOLF-CI"
DefaultBranchName="develop"

function QueueBuild {
	build=$(wget -O - "http://bamboo.opswolf.com/rest/api/latest/queue/$2.json?os_username=$BambooUsername&os_password=$BambooPassword" -q --method=GET)
	echo $(echo $build | cut -d '"' -f 20)
}

#Parse Arguments
if [ "$1" != "" ]; then
	branch="$1"
	branch=${branch#*refs/heads/}
else
	branch=$DefaultBranchName
fi 
if [ "$4" != "" ]; then
	CIBambooPlanKey="$4"
else
	CIBambooPlanKey=$DefaultCIBambooPlanKey
fi 

#Queue Builds
if [ "$branch" == "$DefaultBranchName" ]; then
	QueueBuild $CIBambooPlanKey
else
	branch=${branch//\//-}
	JSONDATA=$(wget -O - "http://bamboo.opswolf.com/rest/api/latest/plan/$CIBambooPlanKey.json?os_username=$BambooUsername&os_password=$BambooPassword&expand=branches&max-result=99" -q --method=GET)
	FindBranchName=$(echo $JSONDATA | grep -o "$branch.*")
	FindBranchKey=$(echo $FindBranchName | grep -o "\"key\":\".*")
	BranchKey=$(echo $FindBranchKey | cut -d '"' -f 4)
	ShortBranchKey=$(echo $BranchKey | cut -d '-' -f 1)-$(echo $BranchKey | cut -d '-' -f 2)
	QueueBuild $ShortBranchKey
fi
