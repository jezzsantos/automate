# Installation

## Command Line Interface (CLI)

If .NET is installed on your machine: 

``` batch
    dotnet tool install automate --global
```

!!! tip
    Manual installers (for all other platforms, e.g. Linux, MacOS) are included in the [Releases](https://github.com/jezzsantos/automate/releases) page

## Jetbrains IDE Plugin

!!! note
    The plugin is only available for the Jetbrains Rider IDE, not for IntelliJ IDEA, or Visual Studio.

1. Install the CLI (above) first, and then,
2. Install the plugin from within Rider, from: `Settings | Plugins | Marketplace` and search for `automate` ![Logo](logo_plugin.svg){ align=right }

!!! tip
    This is the [homepage for the plugin](https://plugins.jetbrains.com/plugin/19421-automate) in the Jetbrains Marketplace

!!! danger "Important"
    The plugin relies on a compatible version of the automate CLI to be installed on your local machine, in a location that the plugin is configured to use.

    If you installed the CLI using `dotnet` then the plugin should be pointed at the executable by default. 

    However if you installed the CLI manually, you will need to configure the plugin to point at your installation of the CLI.

    You can configure the plugin in the IDE, from the settings (`File | Settings | Tools | automate`)

    You will also need to have the right version of the CLI installed on your machine depending on the plugin. To upgrade the installed dotnet tool: `dotnet tool install automate --global --version x.y.z`

## How automate Works

There are two distinctive modes of use of automate:

### [Making Toolkits](authoring.md)

A Tech Lead/Tech Consultant will use `automate` to capture and define a coding patterns based on code in an existing codebase.

They will:

1. Name the pattern
2. Configure its structure and its attributes, and how they vary
3. Configure any automation, to apply the pattern to another codebase
4. Publish a toolkit from it, for distribution to another codebase
5. Deploy that toolkit to a team to use, or install it into an existing codebase

### [Using Toolkits](runtime.md)

Contributors on a codebase will then use `automate` to run a toolkit on their codebase.

They will:

1. Install a toolkit locally (unless already installed into the codebase)
2. Create a "draft" use-case with the toolkit
3. Configure the draft (according the pattern)
4. Execute any automation (in the toolkit) to apply the changes to their codebase

!!! info
    The toolkit then applies the structure, templates and automation to their codebase.