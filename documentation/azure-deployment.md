# Azure deployment

Infrastructure is deployed manually via **Infra Deploy** on `main`; the app resource group and registry must already exist in the same subscription. Register `Microsoft.App` on that subscription.

Configure these variables in the GitHub `dev` environment:

| Variable | Value |
| --- | --- |
| `ACR_NAME` | Existing registry name, without `.azurecr.io` |
| `RESOURCE_GROUP` | App resource group, also containing the pull identity |
| `CONTAINER_APP_NAME` | Container App name; environment name is `<name>-env` |
| `ACR_PULL_IDENTITY_NAME` | Existing user-assigned pull identity in `RESOURCE_GROUP` |

Configure secrets for two identities, each with a federated credential for this repository's `dev` environment:

| Purpose | Client ID | Tenant ID | Subscription ID |
| --- | --- | --- | --- |
| Image publishing | `ACR_CLIENT_ID` | `ACR_TENANT_ID` | `ACR_SUBSCRIPTION_ID` |
| Infrastructure deployment | `INFRA_CLIENT_ID` | `INFRA_TENANT_ID` | `INFRA_SUBSCRIPTION_ID` |

Assign Azure roles:

| Identity | Scope | Roles |
| --- | --- | --- |
| Infrastructure | App resource group; registry | Contributor; Reader |
| Publishing | Registry | `AcrPush` and `AcrDelete`, or `Container Registry Repository Contributor` for ABAC mode |
| Pull | Registry | `AcrPull`, or `Container Registry Repository Reader` for ABAC mode |

ABAC repository roles can be scoped to the workbench repository. The pull identity and its role assignment must exist before deployment; allow time for role propagation.

Publish `gram-parser-workbench:latest` before the first infra deployment. Until the app exists, the build publishes the image but its deployment step fails. Infra deploys `latest`; continuous builds on `main` deploy an immutable digest. Rerunning infra switches the app back to the mutable `latest` tag, so subsequent starts may pick up a newer image without a revision change.

The Consumption app uses 0.25 vCPU, 0.5 GiB memory, 0–1 replicas and an approximately five-minute scale-down cooldown. Azure supplies HTTPS and the certificate. Logs are streamed but not retained. The app URL appears in the infra workflow summary.

After a successful deployment, cleanup retains the five newest images and deletes older manifests and tags. It refuses to delete the deployed digest; older revisions cannot roll back if their image has been removed. Cleanup may be partially completed if a deletion fails. Ensure no other consumers need the old images.

Infra deployment is incremental: **old Web Apps, App Service plans and monitoring resources remain billable** until retired separately.
