#!/bin/sh
#
# Queue Bamboo build.
# This script can take a branch name as a commandline parameter.
#
DefaultBranchName="develop"

function QueueBuild {
	curl -I -X POST -H "Authorization: Bearer $BambooPassword" "http://bamboo.opswolf.com/rest/api/latest/queue/$1.json?stage&executeAllStages=true"
}

#Parse Arguments
if [ "$1" != "" ]; then
	if [ $(echo -n "$1" | grep -Fo "." | wc -l) != 3 ]; then
		branch="$1"
		branch=${branch#*refs/heads/}
	else
		exit 0
	fi
else
	branch=$DefaultBranchName
fi

#Queue Builds
if [ "$branch" == "$DefaultBranchName" ]; then
	QueueBuild "WOLF-CI"
else
	branch=${branch//\//-}
	JSONDATA=$(curl -H "Authorization: Bearer $BambooPassword" "http://bamboo.opswolf.com/rest/api/latest/plan/WOLF-CI.json?expand=branches&max-result=99")
	FindBranchName=$(echo $JSONDATA | grep -o "$branch.*")
	FindBranchKey=$(echo $FindBranchName | grep -o "\"key\":\".*")
	BranchKey=$(echo $FindBranchKey | cut -d '"' -f 4)
	ShortBranchKey=$(echo $BranchKey | cut -d '-' -f 1)-$(echo $BranchKey | cut -d '-' -f 2)
	QueueBuild $ShortBranchKey
fi
