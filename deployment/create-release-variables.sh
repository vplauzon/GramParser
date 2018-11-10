#!/bin/bash

#   Should be $(Release.EnvironmentName)
environment=$1

appinsight="central-insight-$environment"
cluster="cluster-$environment"
group="shared-infra-$environment"
repo="vplauzon/pas-api"
tag="$environment"

echo "appinsight:  '$appinsight'"
echo "##vso[task.setvariable variable=appinsight;]$appinsight"
echo "appinsight:  '$cluster'"
echo "##vso[task.setvariable variable=cluster;]$cluster"
echo "appinsight:  '$group'"
echo "##vso[task.setvariable variable=group;]$group"
echo "appinsight:  '$repo'"
echo "##vso[task.setvariable variable=repo;]$repo"
echo "appinsight:  '$tag'"
echo "##vso[task.setvariable variable=tag;]$tag"