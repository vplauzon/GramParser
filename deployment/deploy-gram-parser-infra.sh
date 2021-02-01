#!/bin/bash

##########################################################################
##  Deploys GramParser Azure infrastructure solution
##
##  Parameters:
##
##  1- Name of resource group
##  2- Value for customDomainVerificationId

rg=$1
verificationId=$2

echo "Resource group:  $rg"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file gram-parser-infra.json \
    --parameters verificationId=$verificationId
