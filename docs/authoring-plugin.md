# Making Toolkits

Toolkits contain patterns which are essentially templates that are used to automate the creation of other code in your IDE.

[A Pattern](reference.md#pattern) is a major top-level concept in automate, used to describe coding patterns. When published, it is packaged and distributed in [A Toolkit](reference.md#toolkit) that is installed into a codebase.

You will need to have installed at least one toolkit in order to create a [Draft](reference.md#draft).

!!! info
    To make toolkits in your IDE, you will need to have the `automate`plugin installed into JetBrains Rider on your machine. (see [Installing plugin](installation-rider-plugin.md))

!!! warning
    The IDE plugin is currently under-development (in the [Plugin Project](https://github.com/jezzsantos/automate.plugin-rider)). Designs and plans for its development are to be found here: [Prototypes-Plugin](https://github.com/jezzsantos/automate/tree/main/discovery/prototypes/ide)

## Turn ON Authoring mode

To create patterns using the plugin in your IDE, you will need to turn ON "Authoring Mode" to show the necessary tools.

1. Open the "automate" tool window in the IDE (`View | Tool Windows | automate`)
2. Click on the `Settings` button to show the settings (you can also find them `File | Settings | Tools | automate`)
3. check "Authoring mode"

## Capture a new pattern

To create a new toolkit:

1. Open the "automate" tool window in the IDE (`View | Tool Windows | automate`)
2. In the tool bar of the tool window, click the `Patterns` button.
3. Click the `New Pattern` button

!!! info
    This pattern automatically becomes the 'active' pattern for subsequent commands.