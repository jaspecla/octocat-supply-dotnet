# Deploy to Azure Container Apps

> **Warning:** This application contains intentional security vulnerabilities.
> This deployment exposes the Blazor demo over public HTTPS, without adding
> authentication. Use only disposable demo data and remove the resources when
> finished. This is not a production deployment.

## Architecture

[main.bicep](../infra/main.bicep) creates:

- A Basic Azure Container Registry (ACR), with admin credentials and anonymous
  pulls disabled.
- A user-assigned managed identity with `AcrPull` scoped only to that registry
  and `Monitoring Metrics Publisher` scoped only to Application Insights.
- A Log Analytics workspace with 30-day retention.
- A workspace-based Application Insights resource for traces, metrics, and logs.
- An Azure Container Apps environment using the Consumption workload profile.
- One Container App with the existing API and Blazor Docker images.

Only the web container's port `8080` is exposed through HTTPS ingress. Automatic
HTTP transport supports Blazor Server WebSockets. The API listens on port `8081`
and the Blazor server calls `http://localhost:8081/api/`; it has no public ingress.
Both containers retain the Dockerfiles' non-root user and Production environment.
Startup and readiness probes cover both containers.

The web container also enables the intentional five-second **Explore Products**
latency defect, `DEMO-HOME-001`, for the Application Insights MCP demonstration.
It is gated by `Demo__SlowProductExploration`; preserve it unless explicitly
asked to disable it. See the [demo runbook](application-insights-demo.md).

The deployment passes the Application Insights connection string through a
Container App secret and configures both services to use the managed identity.
Application Insights local authentication is disabled; the identity's scoped
role permits authenticated ingestion of all three signals. The API and web
services appear as separate cloud roles. See the [README](../README.md#opentelemetry-and-application-insights)
for local configuration, sampling, and data-sensitivity considerations.
The deployment does not automatically grant developers permission to send
telemetry from their workstations.

The app stays at one replica with single-revision mode. SQLite lives at
`/app/data/app.db` in the API container's **ephemeral filesystem**. It is recreated
and seeded when that container is replaced or restarted. Deployments lose changes;
old and new revisions may briefly have separate databases during rollout.
Blazor circuits and data-protection keys are also disposable, so reconnect or sign
in again after a replacement. No persistent database or storage is provisioned.

## Prerequisites

- PowerShell 7 or later.
- Azure CLI with Bicep installed (`az bicep install` if missing).
- An Azure subscription and a region supporting Azure Container Apps Consumption.
- Permission to create the resource group and resources, run ACR builds, and
  create the scoped role assignment. For example, Contributor plus Role Based
  Access Control Administrator at the target scope. Contributor alone cannot
  create role assignments. Use narrower custom permissions where appropriate.
- The `Microsoft.App`, `Microsoft.ContainerRegistry`,
  `Microsoft.OperationalInsights`, `Microsoft.Insights`, and `Microsoft.ManagedIdentity` resource
  providers must be registered in the subscription. If needed, a subscription
  administrator can run `az provider register --namespace <provider> --wait`.

No local Docker installation is required: `az acr build` uploads the repository
build context and builds the Linux images in Azure. The existing
[.dockerignore](../.dockerignore) excludes local databases, build outputs, and
common secret files. Review the build context before uploading it.
ACR Tasks must be available to your subscription; some restricted subscriptions
cannot run cloud builds.

## Deploy

From the repository root:

```powershell
az login
az account set --subscription '<subscription-id>'

.\infra\deploy.ps1 -ResourceGroup 'rg-octocat-demo' -Location 'eastus2'
```

The [deployment script](../infra/deploy.ps1):

1. Compiles Bicep locally before creating resources.
2. Creates the resource group if absent, or preserves its existing region.
   `-Location` selects the region for application resources; it does not move
   an existing resource group.
3. Deploys the registry, managed identity, role assignments, Application Insights,
   logs, and environment with `deployApplication=false`.
4. Builds and pushes `octocat-supply/api:<tag>` and `octocat-supply/web:<tag>` to
   the new ACR using the repository root as the Docker build context.
5. Redeploys Bicep with `deployApplication=true`, after the images exist.
6. Checks the public HTTPS homepage and Blazor startup script, then returns the frontend URL, app name,
   registry and Application Insights names, image tag, and Azure portal
   resource-group URL.

By default, the image tag contains a UTC timestamp and random suffix. Redeploy
using the same resource group and app name to update the existing application:

```powershell
.\infra\deploy.ps1 -ResourceGroup 'rg-octocat-demo' -Location 'eastus2' `
    -AppName 'octocat-supply' -ImageTag 'demo-v2'
```

Always use a **new tag** for changed images, rather than overwriting an existing
tag. Keep the region unchanged when reusing existing application resources.
Names include a deterministic suffix based on resource group and app name.
Changing either creates different resources rather than renaming the existing
ones. To deploy in a different region, use a new app name, because Azure
resources cannot be moved between regions.

Infrastructure is defined in Bicep; the PowerShell script only orchestrates
provisioning, cloud image builds, and verification. It stops on failed Azure CLI
commands or failed HTTPS verification. Running the script provisions billable
resources; merely compiling the templates does not.

## Deploy with GitHub Actions

The [Deploy Azure demo workflow](../.github/workflows/deploy-azure.yml) runs only
when manually dispatched. It builds the solution and runs the tests before using
the deployment script to provision or update the infrastructure, build both
images in ACR, deploy the app, and verify its HTTPS endpoint. Deployments are
serialized; every run and retry uses a distinct image tag based on the commit,
run ID, and attempt. GitHub may cancel an older pending run when a newer one is
queued. The frontend URL and image tag appear in the run summary. The deployment
uses GitHub-hosted runners and does not need local Docker.

The workflow exposes the same intentionally vulnerable public demo and resets
disposable data when containers are replaced. Running it can incur Azure charges.
Only dispatch a trusted branch, and do not use real or production data.

### Configure Azure authentication

Set up OpenID Connect (OIDC) for the GitHub environment; no Azure client secret
is needed:

1. Create the target demo resource group in the selected subscription and
   region. Create a separate resource group and a **user-assigned managed
   identity** for GitHub Actions, so deleting the demo group does not remove
   the deployment identity. This identity is separate from the app's ACR pull
   identity created by Bicep.
2. On that identity, add a GitHub federated credential. For the original
   name-based GitHub OIDC subject format, use these values, replacing
   `<owner>/<repository>` with the actual GitHub repository:

   | Field | Value |
   |-------|-------|
   | Issuer | `https://token.actions.githubusercontent.com` |
   | Subject | `repo:<owner>/<repository>:environment:azure-demo` |
   | Audience | `api://AzureADTokenExchange` |

   The Azure portal's GitHub Actions credential option can construct this
   subject by selecting the **Environment** entity and naming it `azure-demo`.
   Repositories using GitHub's immutable subject format instead need
   `repo:<owner>@<owner-id>/<repository>@<repository-id>:environment:azure-demo`.
   Customized OIDC subjects may differ. Configure the exact subject emitted by
   the repository, and verify the environment and repository match.
3. Give the deployment identity **Contributor** and **Role Based Access
   Control Administrator** on the demo resource group, or equivalent narrowly
   scoped custom roles. The latter is needed because Bicep assigns `AcrPull`
   and `Monitoring Metrics Publisher` to the app's managed identity.
   Role-assignment permissions are powerful; restrict the scope and who can
   approve or modify the workflow. The resource providers listed above must
   already be registered.

Pre-creating the demo resource group allows these permissions to be scoped to
that group. If the workflow should create a missing group instead, the identity
also needs permission to create resource groups at subscription scope. Azure
role assignments and federated credentials can take time to propagate.

### Configure the GitHub environment

1. In the repository's **Settings > Environments**, create an environment named
   `azure-demo`. Restrict it to trusted deployment branches and configure
   required reviewers where available. An environment is not protected merely
   because the workflow names it.
2. Add these environment **secrets**:

   | Secret | Value |
   |--------|-------|
   | `AZURE_CLIENT_ID` | Client ID of the GitHub Actions managed identity |
   | `AZURE_TENANT_ID` | Microsoft Entra tenant ID |
   | `AZURE_SUBSCRIPTION_ID` | Target Azure subscription ID |

3. Add these environment **variables**:

   | Variable | Value |
   |----------|-------|
   | `AZURE_RESOURCE_GROUP` | Target demo resource group; required |
   | `AZURE_LOCATION` | Resource region; optional, defaults to `eastus2` |
   | `AZURE_APP_NAME` | App name; optional, defaults to `octocat-supply` |

   Use the same region and app name for redeployments. The app name must be
   3-16 lowercase letters, digits, or internal hyphens, starting with a letter
   and ending with a letter or digit.
4. Once the workflow is present on the default branch, open **Actions > Deploy
   Azure demo > Run workflow**, select a trusted branch, and run it. Approve the
   deployment if the environment requires approval. A failing build, test,
   provisioning step, image build, or endpoint check fails the workflow.

See [GitHub's Azure OIDC guide](https://docs.github.com/actions/security-for-github-actions/security-hardening-your-deployments/configuring-openid-connect-in-azure)
for further details. Deleting the demo resource group does not delete the
separate GitHub Actions identity; remove that identity and its federated
credential separately when they are no longer needed.

## Validate without deploying

```powershell
az bicep build --file .\infra\main.bicep --stdout
az bicep lint --file .\infra\main.bicep
```

With an existing resource group and suitable permissions, preview changes after
building the images using that registry and tag:

```powershell
az deployment group what-if --resource-group 'rg-octocat-demo' `
    --template-file .\infra\main.bicep `
    --parameters appName=octocat-supply imageTag=demo-v2 deployApplication=true
```

## Troubleshooting

- **AuthorizationFailed on role assignment:** the deploying principal needs
  role-assignment write permissions, not just Contributor.
- **Image pull failure:** check that both images and the chosen tag exist in
  ACR. Managed identity role propagation can take time; retry deployment after
  propagation. Do not enable ACR admin credentials as a workaround.
- **Missing telemetry:** verify the Application Insights connection string,
  the managed identity's scoped Monitoring Metrics Publisher assignment, and
  outbound connectivity. Role assignments can take time to propagate. Check
  each service's cloud role and allow time for batched telemetry to arrive.
- **Build failure:** inspect the ACR build output. Both Dockerfiles require the
  repository root as context, not their individual project directories.
- **Frontend check failure:** inspect both containers' logs and revision
  readiness in the Azure portal. The API must listen on `8081`, while web ingress
  targets `8080`. Open the frontend and verify interactive product loading and
  the Blazor connection; an HTTP check alone does not test browser interactivity.

## Cleanup and costs

The app deliberately keeps one replica running to avoid idle scale-to-zero
resets. ACR, container compute, builds, and Application Insights/Log Analytics
ingestion may incur charges.
There is no production hardening, backup, or recovery mechanism.

Delete the resource group **only if it is dedicated to this demo**:

```powershell
az group delete --name 'rg-octocat-demo' --yes --no-wait
```

## References

- [Container Apps multi-container networking](https://learn.microsoft.com/azure/container-apps/containers)
- [Managed identity image pulls](https://learn.microsoft.com/azure/container-apps/managed-identity-image-pull)
- [ACR cloud builds](https://learn.microsoft.com/azure/container-registry/container-registry-tutorial-quick-task)
- [Azure Monitor OpenTelemetry](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable)
- [Application Insights Microsoft Entra authentication](https://learn.microsoft.com/azure/azure-monitor/app/azure-ad-authentication)
