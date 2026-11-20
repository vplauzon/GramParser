# GramParser

[![Continuous Build / Test](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml/badge.svg)](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml)

See the <span style="background:yellow">[following blog article](http://todo.com)</span> introducing it.

GramParser is a grammar-based parser for building DSLs, available as a [.NET Standard 2.1 NuGet package](https://www.nuget.org/packages/GramParser/). Its workbench lets you experiment interactively.

## Publishing the NuGet package

Publishing uses [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing). Before running the **Publish NuGet package** workflow on `main`:

1. On nuget.org, create a trusted publishing policy owned by the account or organization that owns the `GramParser` package. Set repository owner to `vplauzon`, repository to `GramParser`, and workflow file to `nuget-push.yaml`. Leave the environment empty and allow publishing new versions of `GramParser` in the policy scope.
2. In GitHub repository secrets, set `NUGET_USER` to the nuget.org profile name used for the policy, not an email address.
3. Run the workflow manually, choosing whether to publish an alpha prerelease.

## Azure deployment

See the [Azure deployment guide](documentation/azure-deployment.md).

## Using the workbench

The workbench takes inspiration from Microsoft's former M editor. See the [tutorial](documentation/tutorial.md) and [documentation](documentation).

## Coding standard

See [CodingStandard](CodingStandard.md) for details