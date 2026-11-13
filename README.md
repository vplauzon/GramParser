# GramParser

[![Continuous Build / Test](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml/badge.svg)](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml)

See the <span style="background:yellow">[following blog article](http://todo.com)</span> introducing it.

GramParser is a grammar-based parser distributed as a [.NET Standard 2.1 Nuget Package](https://www.nuget.org/packages/GramParser/).  It allows to quickly implement Domain Specific Languages (DSLs).

The workbench lets you experiment interactively. GitHub Actions builds and tests the solution and, on `main`, publishes the workbench image to Azure Container Registry with commit-SHA and `latest` tags.

## Azure deployment

Run **Infra Deploy** from GitHub Actions on `main`, or push a change to `deployment/*.bicep`. The workflow deploys `deployment/gram-parser-infra.bicep` into the existing `gram-parser` resource group, using its region. It discovers the existing registry's resource group in the same subscription from the `ACR_NAME` variable.

Configure the GitHub `dev` environment with the `ACR_NAME` variable (registry name only) and two sets of identity secrets:

| Purpose | Client ID | Tenant ID | Subscription ID |
| --- | --- | --- | --- |
| Image publishing | `ACR_CLIENT_ID` | `ACR_TENANT_ID` | `ACR_SUBSCRIPTION_ID` |
| Infrastructure deployment | `INFRA_CLIENT_ID` | `INFRA_TENANT_ID` | `INFRA_SUBSCRIPTION_ID` |

Both identities need their own federated credential for this repository's `dev` environment. The publishing identity needs registry read and image push permissions. The infrastructure identity needs permission to deploy resources in `gram-parser`, read the registry, and deploy the registry-access module and assign roles in the registry's resource group. Image push permission alone is not sufficient. The current template expects the registry and Container App to be in the same subscription, so set both subscription secrets accordingly; the client IDs should identify the separate identities. Register the `Microsoft.App`, `Microsoft.ManagedIdentity`, and `Microsoft.ContainerRegistry` resource providers in the subscription before deploying.

Publish `gram-parser-workbench:latest` to the registry before the first infrastructure deployment. The template creates a Consumption environment and one Container App with 0.25 vCPU, 0.5 GiB memory, zero minimum replicas, one maximum replica, and a 300-second cooldown. Scale-to-zero timing is approximate, not an exact last-request timer. Azure provides the app's `azurecontainerapps.io` HTTPS hostname and certificate and redirects HTTP to HTTPS; no custom domain or certificate management is required. The deployment outputs the HTTPS URL in the workflow summary.

A dedicated user-assigned managed identity pulls the image without registry passwords. The template grants it `AcrPull`, or `Container Registry Repository Reader` for an ABAC-enabled registry. The registry must be reachable by Container Apps and allow ARM-audience authentication for managed-identity image pulls. Log Analytics and Application Insights are not provisioned, and persistent application log collection is disabled to minimize cost.

Updating `latest` does not replace running replicas; new container starts check the tag again. Infrastructure deployment is incremental: **old Web Apps, the App Service plan, and monitoring resources are not deleted**. Retire those resources separately after verifying the Container App to stop their remaining charges.

## Using the workbench

A typical workflow would be to develop grammar using the workbench.

GramParser is based on the defunct project 'M' from Microsoft, circa 2008.  The workbench takes inspiration from the three columns M editor.

Follow the [tutorial](documentation/tutorial.md) to have a quick tour of capabilities.  The formal documentation is [available here](documentation).
