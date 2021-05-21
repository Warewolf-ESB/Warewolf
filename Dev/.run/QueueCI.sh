#!/bin/sh
#
# Queue Bamboo build.
# This script can take a branch name as a commandline parameter.
#
DefaultBranchName="develop"

function QueueBuild {
	echo http://bamboo.opswolf.com/rest/api/latest/queue/$1.json?os_authType=basic&os_username=$BambooUsername&os_password=$BambooPassword
	curl -X POST -H "Content-type: application/json" "http://bamboo.opswolf.com/rest/api/latest/queue/$1.json?os_authType=basic&os_username=$BambooUsername&os_password=$BambooPassword"
}

#Parse Arguments
if [ "$1" != "" ]; then
	branch="$1"
	branch=${branch#*refs/heads/}
else
	branch=$DefaultBranchName
fi

#Queue Builds
if [ "$branch" == "$DefaultBranchName" ]; then
	QueueBuild "WOLF-CI"
else
	branch=${branch//\//-}
	echo http://bamboo.opswolf.com/rest/api/latest/plan/WOLF-CI.json?os_authType=basic&os_username=$BambooUsername&os_password=$BambooPassword&expand=branches&max-result=99
	JSONDATA=$(curl -H "Content-type: application/json" "http://bamboo.opswolf.com/rest/api/latest/plan/WOLF-CI.json?os_authType=basic&os_username=$BambooUsername&os_password=$BambooPassword&expand=branches&max-result=99")
	FindBranchName=$(echo $JSONDATA | grep -o "$branch.*")
	FindBranchKey=$(echo $FindBranchName | grep -o "\"key\":\".*")
	BranchKey=$(echo $FindBranchKey | cut -d '"' -f 4)
	ShortBranchKey=$(echo $BranchKey | cut -d '-' -f 1)-$(echo $BranchKey | cut -d '-' -f 2)
	QueueBuild $ShortBranchKey
fi
