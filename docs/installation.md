# Installation

## Command Line Interface (CLI)

If .NET is installed on your machine: 

``` batch
    dotnet tool install automate --global
```

!!! tip
    Manual installers (for all other platforms, e.g. Linux, MacOS) are included in the [Releases](https://github.com/jezzsantos/automate/releases) page

To upgrade the CLI to a specific version:

``` batch
    dotnet tool uninstall automate -- global
    dotnet tool install automate --global --version x.y.z
```

## Jetbrains IDE Plugin

!!! danger "Important"
    This plugin relies on having the CLI installed on your machine as well. Furthermore, each version of the plugin will require a specific version of the CLI, their versions must be compatible (not necessarily the same).

1. Make sure that you install the CLI (above) first
2. Install the plugin within Rider itself, from: `Settings | Plugins | Marketplace` and search for `automate` ![Logo](logo_plugin.svg){ align=right }

!!! tip
    The plugin is configured to point to the installed executable of the CLI.

    If you installed the CLI using `dotnet` (as above) then the plugin should be pointed at the executable by default. However if you installed the CLI manually, you will need to configure the plugin to point at the executable where you installed the CLI.

    You can configure the plugin in Rider, from the settings (`File | Settings | Tools | automate`)

!!! info
    The [homepage for the plugin](https://plugins.jetbrains.com/plugin/19421-automate) on the Jetbrains Marketplace