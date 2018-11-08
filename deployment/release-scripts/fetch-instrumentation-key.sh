#!/usr/bin/bash

$rg=$1

echo "Resource Group:  $rg"

$key=$(az resource show -g $rg -n central-insight --resource-type "microsoft.insights/components" --query properties.InstrumentationKey -o tsv)

echo "Instrumentation Key:  $key"

echo "##vso[task.setvariable variable=instrumentationKey;]$key"