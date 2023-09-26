# GramParser

[![Continuous Build / Test](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml/badge.svg)](https://github.com/vplauzon/GramParser/actions/workflows/continuous-build.yaml)

See the <span style="background:yellow">[following blog article](http://todo.com)</span> introducing it.

GramParser is a grammar-based parser distributed as a [.NET Standard 2.1 Nuget Package](https://www.nuget.org/packages/GramParser/).  It allows to quickly implement Domain Specific Languages (DSLs).

A workbench is [available online](https://workbench-gram-parser-jlv6prl7bdhpu.azurewebsites.net/) to experiment in an interactive manner.  The workbench is continuously built by [GitHub actions](https://docs.github.com/en/free-pro-team@latest/actions) on this repo and deployed to [Docker Hub](https://hub.docker.com/repository/docker/vplauzon/gram-parser-workbench).

A typical workflow would be to develop grammar using the workbench.  We can then use that grammar to parse text in applications.

GramParser is based on the defunct project 'M' from Microsoft, circa 2008.  The workbench takes inspiration from the three columns M editor.

Follow the [tutorial](documentation/tutorial.md) to have a quick tour of capabilities.  The formal documentation is [available here](documentation).
