#!/bin/bash

###################################################################################################
### Fetch the colour to deploy from the DNS resource group tag (deploy-colour)
###
### Outputs:
###     colour:  colour of the environment (blue or green)

rg=vpl-dns

colour=$(az group show --name $rg --query "tags.deployColour" -o tsv)

echo "Colour:  $colour"

echo "##vso[task.setvariable variable=colour;]$colour"