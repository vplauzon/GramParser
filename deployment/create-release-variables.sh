#!/bin/bash

#   Should be $(Release.EnvironmentName)
environment=$1

repo="vplauzon/pas-api"
tag="$environment"

echo "repo:  '$repo'"
echo "##vso[task.setvariable variable=repo;]$repo"
echo "tag:  '$tag'"
echo "##vso[task.setvariable variable=tag;]$tag"