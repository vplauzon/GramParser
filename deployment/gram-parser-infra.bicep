targetScope = 'resourceGroup'

@description('Region for the Container App and its managed environment.')
param location string = resourceGroup().location

@description('Name of the existing Azure Container Registry.')
param acrName string

@description('Resource group containing the existing registry, in the deployment subscription.')
param acrResourceGroupName string = resourceGroup().name

@description('Name of the Container App.')
@minLength(1)
param appName string

@description('Name of the existing image-pull identity in the deployment resource group.')
@minLength(1)
param pullIdentityName string

resource registry 'Microsoft.ContainerRegistry/registries@2025-04-01' existing = {
  name: acrName
  scope: resourceGroup(acrResourceGroupName)
}

resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: pullIdentityName
}

resource environment 'Microsoft.App/managedEnvironments@2025-07-01' = {
  name: '${appName}-env'
  location: location
  properties: {
	appLogsConfiguration: {
	  destination: ''
	}
	workloadProfiles: [
	  {
		name: 'Consumption'
		workloadProfileType: 'Consumption'
	  }
	]
  }
}

resource app 'Microsoft.App/containerApps@2025-07-01' = {
  name: appName
  location: location
  identity: {
	type: 'UserAssigned'
	userAssignedIdentities: {
	  '${pullIdentity.id}': {}
	}
  }
  properties: {
	environmentId: environment.id
	workloadProfileName: 'Consumption'
	configuration: {
	  activeRevisionsMode: 'Single'
	  ingress: {
		external: true
		targetPort: 8080
		transport: 'auto'
		allowInsecure: false
	  }
	  registries: [
		{
		  server: registry.properties.loginServer
		  identity: pullIdentity.id
		}
	  ]
	}
	template: {
	  containers: [
		{
		  name: 'workbench'
		  image: '${registry.properties.loginServer}/gram-parser-workbench:latest'
		  resources: {
			cpu: json('0.25')
			memory: '0.5Gi'
		  }
		  env: [
			{
			  name: 'ASPNETCORE_ENVIRONMENT'
			  value: 'Production'
			}
			{
			  name: 'ASPNETCORE_HTTP_PORTS'
			  value: '8080'
			}
			{
			  name: 'WORKBENCH_ENVIRONMENT'
			  value: 'prod'
			}
		  ]
		}
	  ]
	  scale: {
		minReplicas: 0
		maxReplicas: 1
		cooldownPeriod: 300
		rules: [
		  {
			name: 'http'
			http: {
			  metadata: {
				concurrentRequests: '10'
			  }
			}
		  }
		]
	  }
	}
  }
}

output appUrl string = 'https://${app.properties.configuration.ingress.fqdn}'
output appResourceId string = app.id
output pullIdentityResourceId string = pullIdentity.id
