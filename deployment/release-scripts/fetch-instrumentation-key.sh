rg=$1
ai=$2

echo "Resource Group:  $rg"
echo "App Insight:  $ai"

key=$(az resource show -g $rg -n $ai --resource-type "microsoft.insights/components" --query properties.InstrumentationKey -o tsv)

echo "Instrumentation Key:  $key"

echo "##vso[task.setvariable variable=instrumentationKey;]$key"