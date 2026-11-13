targetScope = 'resourceGroup'

@description('Region for the Container App and its managed environment.')
param location string = resourceGroup().location

@description('Name of the existing Azure Container Registry.')
param acrName string

@description('Resource group containing the existing registry, in the deployment subscription.')
param acrResourceGroupName string = resourceGroup().name

@description('Pull role GUID appropriate for the registry permission mode.')
@allowed([
  'b93aa761-3e63-49ed-ac28-beffa264f7ac'
  '7f951dda-4ed3-4680-a7ca-43fe172d538d'
])
param pullRoleId string

@description('Name of the Container App.')
@minLength(1)
param appName string

@description('Name of the image-pull identity to create in the deployment resource group.')
@minLength(1)
param pullIdentityName string

@description('Image repository and tag within the registry.')
param imageName string = 'gram-parser-workbench:latest'

resource registry 'Microsoft.ContainerRegistry/registries@2025-04-01' existing = {
  name: acrName
  scope: resourceGroup(acrResourceGroupName)
}

resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: pullIdentityName
  location: location
}

module registryAccess 'registry-pull-access.bicep' = {
  name: '${appName}-registry-access'
  scope: resourceGroup(acrResourceGroupName)
  params: {
    acrName: acrName
    principalId: pullIdentity.properties.principalId
    pullRoleId: pullRoleId
  }
}

resource environment 'Microsoft.App/managedEnvironments@2025-07-01' = {
  name: '${appName}-env'
  location: location
  properties: {
	appLogsConfiguration: {
	  destination: 'none'
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
		  image: '${registry.properties.loginServer}/${imageName}'
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
  dependsOn: [
    registryAccess
  ]
}

output appUrl string = 'https://${app.properties.configuration.ingress.fqdn}'
output appResourceId string = app.id
output pullIdentityResourceId string = pullIdentity.id
