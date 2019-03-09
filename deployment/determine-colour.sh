#!/bin/bash

###################################################################################################
### Determine the colour to deploy
###
### Outputs:
###     colour:  colour of the environment (blue or green)
###     env:  name of the environment (e.g. dev, prod)
###     sirg:  shared-infra resource group (in proper coloured environment)
###     ssrg:  shared-state resourge group
###     cluster:  name of the AKS cluster (in sirg)

#   Bind script parameters
environment=$1
requestColour=$2

echo "Environment:  $environment"
echo "Request Colour:  $requestColour"

#   Default:  find "active colour"
if [[ $requestColour == 'default' ]]
then
    echo "Fetch DNS recort set..."

    dns=$(az network dns record-set cname show -g vpl-dns -z vplauzon.com -n main-ip.dev --query "cnameRecord.cname" -o tsv)

    echo "DNS:  $dns"

    if [[ $dns == *"blue"* ]]
    then
        colour='blue'
    else
        if [[ $dns == *"green"* ]]
        then
            colour='green'
        else
            echo "DNS entry doesn't match a colour:  $dns" 1>&2
            exit 42
        fi
    fi
else
    #   Specified colour:  force that colour
    echo "Specified colour"
    colour=$requestColour
fi

sirg="shared-stateless-$environment-$colour"
ssrg="shared-state-$environment"
cluster="shared-cluster-$environment-$colour"

echo "Colour:  $colour"
echo "Environment:  $environment"
echo "sirg:  $sirg"
echo "ssrg:  $ssrg"
echo "Cluster:  $cluster"

echo "##vso[task.setvariable variable=colour;]$colour"
echo "##vso[task.setvariable variable=env;]$environment"
echo "##vso[task.setvariable variable=sirg;]$sirg"
echo "##vso[task.setvariable variable=ssrg;]$ssrg"
echo "##vso[task.setvariable variable=cluster;]$cluster"