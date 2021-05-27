#!/bin/sh
#
# Queue Bamboo build.
# This script can take a branch name as a commandline parameter.
#
DefaultBranchName="develop"

function QueueBuild {
	curl -X POST -H "Content-type: application/json" "http://bamboo.opswolf.com/rest/api/latest/queue/$1.json?os_authType=basic&os_username=$BambooUsername&os_password=$BambooPassword"
}

#Parse Arguments
if [ "$1" != "" ]; then
	echo Queing build for branch $1
	branch="$1"
	branch=${branch#*refs/heads/}
else
	echo Queing build for branch develop
	branch=$DefaultBranchName
fi

#Queue Builds
if [ "$branch" == "$DefaultBranchName" ]; then
	QueueBuild "WOLF-CI"
else
	branch=${branch//\//-}
	JSONDATA=$(curl -H "Content-type: application/json" "http://bamboo.opswolf.com/rest/api/latest/plan/WOLF-CI.json?os_authType=basic&os_username=$BambooUsername&os_password=$BambooPassword&expand=branches&max-result=99")
	echo JSONDATA: $JSONDATA
	FindBranchName=$(echo $JSONDATA | grep -o "$branch.*")
	echo FindBranchName: $FindBranchName
	FindBranchKey=$(echo $FindBranchName | grep -o "\"key\":\".*")
	echo FindBranchKey: $FindBranchKey
	BranchKey=$(echo $FindBranchKey | cut -d '"' -f 4)
	echo BranchKey: $BranchKey
	ShortBranchKey=$(echo $BranchKey | cut -d '-' -f 1)-$(echo $BranchKey | cut -d '-' -f 2)
	echo ShortBranchKey: $ShortBranchKey
	QueueBuild $ShortBranchKey
fi
