#!/bin/bash

###################################################################################################
### Fetch App Insights Instrumentation key
###
### Inputs:
###     ssrg:  shared-state resourge group
###
### Outputs:
###     instrumentation:  key for instrumentation

ssrg=$1

echo "Shared State Resource Group:  $ssrg"

id=$(az resource list -g $ssrg --resource-type "Microsoft.Insights/components" --query [0].id -o tsv)

echo "App Insights Resource ID:  $id"

key=$(az resource show --ids $id --query properties.InstrumentationKey -o tsv)

echo "Instrumentation Key:  $key"
echo "##vso[task.setvariable variable=instrumentationKey;]$key"