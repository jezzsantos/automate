# Installation

## Jetbrains Rider Plugin

<div id="plugin" style="float: right"></div>
<script src="https://plugins.jetbrains.com/assets/scripts/mp-widget.js"></script>
<script>
  MarketplaceWidget.setupMarketplaceWidget('install', 19421, "#plugin");
</script>

Install the plugin within Rider itself, from: `File | Settings | Plugins | Marketplace` and search for `automate`

!!! info
    This plugin uses the [automate CLI](installation.md) under the covers, and the CLI needs to be installed on your machine. 

    Version (v1.0.4 and later) of this plugin will install automatically and upgrade the version of the automate CLI that is required by the plugin. Prior versions (less than v1.0.4) of the plugin will require you to manually install the automate CLI first.
    
    By default, the plugin is configured to point to the default install location of the automate CLI (as a dotnet tool). 

!!! tip
    You can view and configure the installation location in Rider, from the settings (`File | Settings | Tools | automate`)
