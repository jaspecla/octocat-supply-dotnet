# Deploy to Azure Container Apps

> **Warning:** This application contains intentional security vulnerabilities.
> This deployment exposes the Blazor demo over public HTTPS, without adding
> authentication. Use only disposable demo data and remove the resources when
> finished. This is not a production deployment.

## Architecture

[main.bicep](../infra/main.bicep) creates:

- A Basic Azure Container Registry (ACR), with admin credentials and anonymous
  pulls disabled.
- A user-assigned managed identity with `AcrPull` scoped only to that registry.
- A Log Analytics workspace with 30-day retention.
- An Azure Container Apps environment using the Consumption workload profile.
- One Container App with the existing API and Blazor Docker images.

Only the web container's port `8080` is exposed through HTTPS ingress. Automatic
HTTP transport supports Blazor Server WebSockets. The API listens on port `8081`
and the Blazor server calls `http://localhost:8081/api/`; it has no public ingress.
Both containers retain the Dockerfiles' non-root user and Production environment.
Startup and readiness probes cover both containers.

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
  `Microsoft.OperationalInsights`, and `Microsoft.ManagedIdentity` resource
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
3. Deploys the registry, managed identity, role assignment, logs, and environment
   with `deployApplication=false`.
4. Builds and pushes `octocat-supply/api:<tag>` and `octocat-supply/web:<tag>` to
   the new ACR using the repository root as the Docker build context.
5. Redeploys Bicep with `deployApplication=true`, after the images exist.
6. Checks the public HTTPS endpoint and returns the frontend URL, app name,
   registry name, image tag, and Azure portal resource-group URL.

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
- **Build failure:** inspect the ACR build output. Both Dockerfiles require the
  repository root as context, not their individual project directories.
- **Frontend check failure:** inspect both containers' logs and revision
  readiness in the Azure portal. The API must listen on `8081`, while web ingress
  targets `8080`. Open the frontend and verify interactive product loading and
  the Blazor connection; an HTTP check alone does not test browser interactivity.

## Cleanup and costs

The app deliberately keeps one replica running to avoid idle scale-to-zero
resets. ACR, container compute, builds, and log ingestion may incur charges.
There is no production hardening, backup, or recovery mechanism.

Delete the resource group **only if it is dedicated to this demo**:

```powershell
az group delete --name 'rg-octocat-demo' --yes --no-wait
```

## References

- [Container Apps multi-container networking](https://learn.microsoft.com/azure/container-apps/containers)
- [Managed identity image pulls](https://learn.microsoft.com/azure/container-apps/managed-identity-image-pull)
- [ACR cloud builds](https://learn.microsoft.com/azure/container-registry/container-registry-tutorial-quick-task)
