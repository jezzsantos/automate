# Installation

## Command Line Interface (CLI)

If `dotnet.exe` is already installed on your machine, simply run: 

``` batch
    dotnet tool install automate --global
```

!!! info
    The automate CLI above is dependent on having the `dotnet.exe` development tools (.NET 6.0 or higher) already installed on your machine. [Installing dotnet](https://learn.microsoft.com/en-us/dotnet/core/install/). All versions of the automate CLI can be found on [nuget.org](https://www.nuget.org/packages/automate)

---

If you don't have `dotnet.exe` installed on your machine, or you don't want to install `dotnet.exe`, you can just install the automate CLI manually:

On Windows:

1. Download the file `automate-Win-x64.exe` to your machine, from: [Releases](https://github.com/jezzsantos/automate/releases)
2. Remove "protection" for the downloaded file, by viewing the 'Properties' of the downloaded file in File Explorer and uncheck the 'Unblock' setting.
3. Copy and rename (to: `automate.exe`) the file to somewhere on your local machine. Recommend: `%USERPROFILE%\AppData\Local\Programs\automate.exe`
4. Edit your environment variables on Windows and add the folder `%USERPROFILE%\AppData\Local\Programs` to the `Path` variable.

On MacOS:

1. Download the file `automate-OSX-x64` to your machine, from: [Releases](https://github.com/jezzsantos/automate/releases)
2. Make the downloaded file executable: `chmod a+x automate.OSX-x64`
3. Remove "protection" for the downloaded file: `xattr -d com.apple.quarantine automate.OSX-x64`
4. Copy and rename (to: `automate`) the file somewhere that is in your PATH. For example: `sudo cp automate.OSX-x64 /usr/local/bin/automate`

On Linux (i.e Ubuntu):

1. Download the file `automate-Linux-x64` to your machine, from: [Releases](https://github.com/jezzsantos/automate/releases)
2. Make the downloaded file executable: `chmod a+x automate.Linux-x64`
3. Copy and rename (to: `automate`) the file somewhere that is in your PATH. For example: `sudo cp automate.Linux-x64 /usr/local/bin/automate`

!!! info
    On some distros of Linux you may need to install the following dependency: `apt-get install libicu-dev`. Other than that, you should be good to go.

### Upgrading

To upgrade the CLI to a specific version:

If `dotnet.exe` is already installed on your machine, simply run:

``` batch
    dotnet tool uninstall automate -- global
    dotnet tool install automate --global --version x.y.z
```

---

If you don't have `dotnet.exe` installed on your machine, update the CLI manually:

1. Download the executable to  your machine with a later one from the [Releases](https://github.com/jezzsantos/automate/releases) page
2. Replace the previous version on your machine. Follow instructions above.

## Jetbrains IDE Plugin

<div id="plugin" style="float: right"></div>
<script src="https://plugins.jetbrains.com/assets/scripts/mp-widget.js"></script>
<script>
  MarketplaceWidget.setupMarketplaceWidget('install', 19421, "#plugin");
</script>

Install the plugin within Rider itself, from: `File | Settings | Plugins | Marketplace` and search for `automate`

!!! info
    This plugin relies on the automate CLI to be installed to run. Furthermore, each version of the plugin will require a specific version of the automate CLI to be installed. 

    Version (v1.0.4 and later) of the plugin will automatically install and upgrade the version of the automate CLI that is required by the plugin. Prior versions (less than v1.0.4) of the plugin will require you to manually install the automate CLI first.
    
    By default, the plugin is configured to point to the default install location of the automate CLI (as a dotnet tool). 

!!! tip
    You can view and configure the installation location in Rider, from the settings (`File | Settings | Tools | automate`)
