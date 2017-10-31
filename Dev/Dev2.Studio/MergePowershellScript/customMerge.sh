#!/bin/sh
# Wrapper script for git mergetool
# [mergetool "customMerge"]
#	cmd = customMerge \"$LOCAL\" \"$REMOTE\" 
# Locate this script in your $HOME

echo "Custom Merge $1";
REMOTE=$1
LOCAL="-merge"

WDIFF=".bite"
if echo "$REMOTE" | grep -q "$WDIFF"; then
	echo "Using Warewolf to merge";
	powershell -command "Start-Process \"C:\PROGRAM FILES (x86)\Warewolf\Studio\Warewolf Studio.exe\" -ArgumentList \"$LOCAL ${REMOTE// /^}\" -Verb runas"
	
fi