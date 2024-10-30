# Using Toolkits

Toolkits contain patterns which are essentially templates that can be used to automate the creation of other code/projects in your IDE. 

[A Toolkit](reference.md#toolkit) is a major top-level concept in automate, used to install coding patterns into a codebase. It contains a published and versioned [Pattern](reference.md#pattern), obtained from its creator.

You will need to have installed at least one toolkit in order to create a [Draft](reference.md#draft). The toolkit provides the patterns that you can use to create a Draft.

!!! info
    To use toolkits in your IDE, you will need to have the `automate`plugin installed into JetBrains Rider on your machine. (see [Installing plugin](installation-rider-plugin.md))

!!! warning
    The IDE plugin is currently under-development (in the [Plugin Project](https://github.com/jezzsantos/automate.plugin-rider)). Designs and plans for its development are to be found here: [Prototypes-Plugin](https://github.com/jezzsantos/automate/tree/main/discovery/prototypes/ide)


## Toolkits

### Obtaining a toolkit

Toolkits come in `*.toolkit` files, obtained from their creators who [make and publish them](authoring.md), like from a team's Tech Lead, or Tech Consultant.

!!! info
    Since they are single files they can be: emailed, dropped in Slack, downloaded from Dropbox, or shared in all the common ways that files can be transferred to you.

!!! info
    Toolkits can also be installed into a codebase (installed by other contributors or creators).

### Installing a toolkit

To install a toolkit yourself:

1. Open the "automate" tool window in the IDE (`View | Tool Windows | automate`)
2. Then, you will need to have downloaded a `*.toolkit` file to somewhere on your local machine.
    * For example: A toolkit file like `AToolkit_0.1.0.toolkit`, could be located on your desktop or downloads folder.
3. In the toolbar of the tool window, click the `Install Toolkit` button
4. Select the toolkit file on your computer. e.g. `C:\mydesktop\AToolkit_0.1.0.toolkit`

!!! info
    The toolkit will be installed, and it will become the 'active' toolkit for subsequent use.

!!! tip
    When you install a toolkit into a codebase, add its files to source control, and then be accessible to all contributors on a team.
    No need for every contributor to install the same toolkit on each of their machines.

!!! warning
    Even if the toolkit has been added to source control, to use it, every contributor will need to install `automate` to their machine.
