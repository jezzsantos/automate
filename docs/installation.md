# Installation

## Command Line Interface (CLI)

If .NET is installed on your machine: 

``` batch
    dotnet tool install automate --global
```

If you don't have .NET installed on your machine:

* Manual standalone executables (for all other platforms, e.g. Linux, MacOS) are included in the [Releases](https://github.com/jezzsantos/automate/releases) page

!!! info
    This CLI requires the .NET 6.0 (or more) runtime to run on your computer.

    If you are on Windows you are good. If you are on MacOS or Linux, and you have Jetbrains Rider installed, then you are also good to go.

    If you are on MacOS or Linux and .NET 6.0 (or more) is not installed on your machine (use `dotnet --list-sdks` to check), then we have some standalone installers for you to use.

### Upgrading

To upgrade the CLI to a specific version:

If .NET is installed on your machine:

``` batch
    dotnet tool uninstall automate -- global
    dotnet tool install automate --global --version x.y.z
```

Otherwise, replace your executable with one from the [Releases](https://github.com/jezzsantos/automate/releases) page

## Jetbrains IDE Plugin

Install the plugin within Rider itself, from: `File | Settings | Plugins | Marketplace` and search for `automate` ![Logo](logo_plugin.svg){ align=right }

!!! warning
    This plugin relies on the automate CLI to run. Furthermore, each version of the plugin will require a specific version of the CLI to be installed. Versions of the plugin (v1.0.4 and later) will automatically install and upgrade the version of the automate CLI that is required by the plugin. Prior versions of the plugin (less than v1.0.4) will require you to manually install the automate CLI first (see instructions above).

!!! tip
    The plugin is configured by default to point to the default install location of the automate CLI, and it is configured to automatically install and upgrade the CLI (as needed). 

    You can configure the plugin in Rider, from the settings (`File | Settings | Tools | automate`)

!!! info
    The [homepage for the plugin](https://plugins.jetbrains.com/plugin/19421-automate) on the Jetbrains Marketplace