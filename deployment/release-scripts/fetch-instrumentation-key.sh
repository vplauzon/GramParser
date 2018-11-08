#!/usr/bin/bash

$rg=$1

$key=$(az resource show -g $rg -n central-insight --resource-type "microsoft.insights/components" --query properties.InstrumentationKey -o tsv)

echo "##vso[task.setvariable variable=instrumentationKey;]$key"