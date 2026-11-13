# GramParser

[![Continuous Build / Test](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml/badge.svg)](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml)

See the <span style="background:yellow">[following blog article](http://todo.com)</span> introducing it.

GramParser is a grammar-based parser distributed as a [.NET Standard 2.1 Nuget Package](https://www.nuget.org/packages/GramParser/).  It allows to quickly implement Domain Specific Languages (DSLs).

The workbench lets you experiment interactively. GitHub Actions builds and tests the solution and, on `main`, publishes a uniquely tagged workbench image to Azure Container Registry and deploys its immutable digest to the Container App.

## Azure deployment

Manually run **Infra Deploy** from GitHub Actions on `main`. Infrastructure deployment does not run automatically on pushes. The workflow deploys `deployment/gram-parser-infra.bicep` into the existing resource group specified by `RESOURCE_GROUP`, using its region. It discovers the existing registry's resource group in the same subscription from `ACR_NAME`. For the first deployment, or when migrating an app that uses a mutable tag, provide the `image_digest` input from the published build's summary. Later infrastructure runs preserve the app's currently configured digest when that input is blank.

Configure these variables in the GitHub `dev` environment:

| Variable | Value |
| --- | --- |
| `ACR_NAME` | Existing registry name, without `.azurecr.io` |
| `RESOURCE_GROUP` | Existing resource group for the app and environment, containing the pull identity |
| `CONTAINER_APP_NAME` | Container App name; the managed environment name is derived by appending `-env` |
| `ACR_PULL_IDENTITY_NAME` | Existing user-assigned image-pull identity name in `RESOURCE_GROUP` |

Configure two sets of identity secrets:

| Purpose | Client ID | Tenant ID | Subscription ID |
| --- | --- | --- | --- |
| Image publishing | `ACR_CLIENT_ID` | `ACR_TENANT_ID` | `ACR_SUBSCRIPTION_ID` |
| Infrastructure deployment | `INFRA_CLIENT_ID` | `INFRA_TENANT_ID` | `INFRA_SUBSCRIPTION_ID` |

Both GitHub identities need their own federated credential for this repository's `dev` environment. Give the infrastructure identity Contributor on the resource group specified by `RESOURCE_GROUP` to deploy the app and attach the existing pull identity. Grant it Reader on the existing registry for ARM metadata. It does not need permission to assign roles or deploy resources in the registry's resource group. The registry and app must be in the same subscription with the current template. Register the `Microsoft.App` resource provider in that subscription before deploying.

The publishing identity needs `AcrPush` and `AcrDelete` on the registry in RBAC Registry Permissions mode, or `Container Registry Repository Contributor` in ABAC-enabled mode (which can be scoped to the workbench repository). Cleanup deletes image manifests and their tags from that repository; ensure no other consumers need the older images.

Publish an image before the first infrastructure deployment. The build's deployment job cannot succeed until the Container App exists, but its build summary still contains the digest needed for the initial **Infra Deploy** run. The template creates a Consumption environment and one Container App with 0.25 vCPU, 0.5 GiB memory, zero minimum replicas, one maximum replica, and a 300-second cooldown. Scale-to-zero timing is approximate, not an exact last-request timer. Azure provides the app's `azurecontainerapps.io` HTTPS hostname and certificate and redirects HTTP to HTTPS; no custom domain or certificate management is required. The deployment outputs the HTTPS URL in the workflow summary.

Before deploying, grant the existing user-assigned identity named by `ACR_PULL_IDENTITY_NAME` pull access on the registry: `AcrPull` for RBAC Registry Permissions mode, or `Container Registry Repository Reader` for RBAC Registry + ABAC Repository Permissions mode. The deployment attaches the identity but does not create it or manage its role assignments. Check for an existing pull-role assignment before adding one; Azure role propagation can delay the first private-image pull.

After a new revision becomes ready, the pipeline keeps the five most recently created workbench images, including referenced manifests, and deletes older manifests and their tags. It refuses cleanup if the deployed digest is not among those five. Cleanup does not run when deployment fails or is skipped; if a deletion fails partway through, prior deletions remain in effect. Older Container App revisions remain visible but cannot be restarted or rolled back if their image has been deleted. Infrastructure deployment is incremental: **old Web Apps, the App Service plan, and monitoring resources are not deleted**. Retire those resources separately after verifying the Container App to stop their remaining charges.

## Using the workbench

A typical workflow would be to develop grammar using the workbench.

GramParser is based on the defunct project 'M' from Microsoft, circa 2008.  The workbench takes inspiration from the three columns M editor.

Follow the [tutorial](documentation/tutorial.md) to have a quick tour of capabilities.  The formal documentation is [available here](documentation).
