#!/bin/bash

#   Should be $(System.DefaultWorkingDirectory)/_API/drop/FullVersion.txt
fullVersionPath=$1

fullVersion=$(cat $fullVersionPath)

echo "Full Version:  '$fullVersion'"

echo "##vso[task.setvariable variable=full-version;]$fullVersion"