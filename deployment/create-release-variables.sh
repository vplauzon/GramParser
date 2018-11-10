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
echo "cluster:  '$cluster'"
echo "##vso[task.setvariable variable=cluster;]$cluster"
echo "group:  '$group'"
echo "##vso[task.setvariable variable=group;]$group"
echo "repo:  '$repo'"
echo "##vso[task.setvariable variable=repo;]$repo"
echo "tag:  '$tag'"
echo "##vso[task.setvariable variable=tag;]$tag"