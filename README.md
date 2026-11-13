# GramParser

[![Continuous Build / Test](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml/badge.svg)](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml)

See the <span style="background:yellow">[following blog article](http://todo.com)</span> introducing it.

GramParser is a grammar-based parser distributed as a [.NET Standard 2.1 Nuget Package](https://www.nuget.org/packages/GramParser/).  It allows to quickly implement Domain Specific Languages (DSLs).

The workbench lets you experiment interactively. GitHub Actions builds and tests the solution and, on `main`, publishes the workbench image to Azure Container Registry with commit-SHA and `latest` tags.

## Azure deployment

Manually run **Infra Deploy** from GitHub Actions on `main`. Infrastructure deployment does not run automatically on pushes. The workflow deploys `deployment/gram-parser-infra.bicep` into the existing resource group specified by `RESOURCE_GROUP`, using its region. It discovers the existing registry's resource group in the same subscription from `ACR_NAME`.

Configure these variables in the GitHub `dev` environment:

| Variable | Value |
| --- | --- |
| `ACR_NAME` | Existing registry name, without `.azurecr.io` |
| `RESOURCE_GROUP` | Existing resource group for the app, environment, and pull identity |
| `CONTAINER_APP_NAME` | Container App name; the managed environment name is derived by appending `-env` |
| `ACR_PULL_IDENTITY_NAME` | User-assigned image-pull identity name to create |

Configure two sets of identity secrets:

| Purpose | Client ID | Tenant ID | Subscription ID |
| --- | --- | --- | --- |
| Image publishing | `ACR_CLIENT_ID` | `ACR_TENANT_ID` | `ACR_SUBSCRIPTION_ID` |
| Infrastructure deployment | `INFRA_CLIENT_ID` | `INFRA_TENANT_ID` | `INFRA_SUBSCRIPTION_ID` |

Both GitHub identities need their own federated credential for this repository's `dev` environment. Give the infrastructure identity Contributor on the resource group specified by `RESOURCE_GROUP` to create the app and its pull identity. On the existing registry, grant it Reader for ARM metadata and Role Based Access Control Administrator to create the pull-role assignment. Prefer a condition on the administrator assignment limiting delegation to `AcrPull` or `Container Registry Repository Reader` and, where possible, the pull identity's principal. Contributor alone cannot assign roles. The cross-resource-group module also requires `Microsoft.Resources/deployments/*` on the registry's resource group; grant a deployment-only custom role there, or Contributor if broader resource-management access is acceptable. The registry and app must be in the same subscription with the current template.

Publish `gram-parser-workbench:latest` to the registry before the first infrastructure deployment. The template creates a Consumption environment and one Container App with 0.25 vCPU, 0.5 GiB memory, zero minimum replicas, one maximum replica, and a 300-second cooldown. Scale-to-zero timing is approximate, not an exact last-request timer. Azure provides the app's `azurecontainerapps.io` HTTPS hostname and certificate and redirects HTTP to HTTPS; no custom domain or certificate management is required. The deployment outputs the HTTPS URL in the workflow summary.

The template creates the user-assigned managed identity named by `ACR_PULL_IDENTITY_NAME` in `RESOURCE_GROUP`. The workflow reads the registry's permission mode and passes the corresponding pull-role GUID to the template before deployment. For direct Bicep deployments, provide `pullRoleId`: `7f951dda-4ed3-4680-a7ca-43fe172d538d` for `AcrPull` or `b93aa761-3e63-49ed-ac28-beffa264f7ac` for `Container Registry Repository Reader` on an ABAC-enabled registry. The `registry-pull-access.bicep` module grants that role before creating the app. Azure role propagation can delay the first private-image pull even after the assignment deployment succeeds. If the identity already has an equivalent manually created assignment, remove that assignment before deployment to avoid a `RoleAssignmentExists` conflict; let the template manage the replacement.

Updating `latest` does not replace running replicas; new container starts check the tag again. Infrastructure deployment is incremental: **old Web Apps, the App Service plan, and monitoring resources are not deleted**. Retire those resources separately after verifying the Container App to stop their remaining charges.

## Using the workbench

A typical workflow would be to develop grammar using the workbench.

GramParser is based on the defunct project 'M' from Microsoft, circa 2008.  The workbench takes inspiration from the three columns M editor.

Follow the [tutorial](documentation/tutorial.md) to have a quick tour of capabilities.  The formal documentation is [available here](documentation).
