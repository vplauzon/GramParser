#!/bin/bash

##########################################################################
##  Create resource group
##
##  Parameters:
##
##  1- Name of resource group
##  2- Azure region

rg=$1
region=$2

echo "Resource group:  $rg"
echo "Azure Region:  $region"

if [[ $(az group exists -g $rg) = 'true' ]]
then
    echo "Resource Group already exists"
else
    echo "Resource Group doesn't exist"
    echo "Creating Resource group $rg"
    az group create -n $rg --location $region
fi