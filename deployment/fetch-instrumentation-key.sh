#!/bin/bash

###################################################################################################
### Fetch Instrumentation key
###
### Outputs:
###     instrumentation:  key for instrumentation

ssrg=$1

ai="central-insight-$ssrg"

echo "Shared State Resource Group:  $ssrg"
echo "App Insight:  $ai"

key=$(az resource show -g $ssrg -n $ai --resource-type "microsoft.insights/components" --query properties.InstrumentationKey -o tsv)

echo "Instrumentation Key:  $key"

echo "##vso[task.setvariable variable=instrumentationKey;]$instrumentation"