#!/bin/sh
# Wrapper script for git mergetool
# [mergetool "customMerge"]
#	cmd = customMerge \"$LOCAL\" \"$REMOTE\" 
# Locate this script in your $HOME
z
echo "Custom Merge $1 $2";
LOCAL=$1
REMOTE=$2

WDIFF="\.cs"

if echo "$LOCAL" | grep -q "$WDIFF"; then
	echo "Using Warewolf to merge";		
	powershell -command "Start-Process \"C:\PROGRAM FILES (x86)\Warewolf\Studio\Warewolf Studio.exe\" -ArgumentList \"$1 $2\" -Verb runas"
	
fi