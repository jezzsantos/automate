# Using Toolkits

[A Toolkit](reference.md#toolkit) is a major top-level concept in automate, used to install coding patterns into a codebase. It contains a published and versioned [Pattern](reference.md#pattern), obtained from its creator.

## Toolkits

### Obtaining a toolkit

Toolkits come in `*.toolkit` files, obtained from their creators who [make and publish them](authoring.md), like from a team's Tech Lead, or Tech Consultant.

!!! info
    Since they are single files they can be: emailed, dropped in Slack, downloaded from Dropbox, or shared in all the common ways that files can be transferred to you.

!!! info
    Toolkits can also be installed into a codebase (installed by other contributors or creators).

### Installing a toolkit

To install a toolkit yourself:

1. You will need to have `automate` CLI (or plugin) installed on your machine. (see [Installing automate](installation.md))
2. Then, you will need to have downloaded a `*.toolkit` file to somewhere on your local machine. 
    * For example: A toolkit file like `AToolkit_0.1.0.toolkit`, could be located on your desktop or downloads folder.
3. In a terminal, navigate to the root of your source codebase (where you want to use the toolkit). For example: `cd C:\myprojects\myproject\src`
4. Install the toolkit: `automate install toolkit C:\mydesktop\AToolkit_0.1.0.toolkit`

!!! info
    The toolkit will be installed, and it will become the 'active' toolkit for subsequent use.

!!! tip
    When you install a toolkit into a codebase, add its files to source control, and then be accessible to all contributors on a team. 
    No need for every contributor to install the same toolkit on each of their machines.
    
!!! warning
    Even if the toolkit has been added to source control, to use it, every contributor will need to install `automate` to their machine.

### Listing installed toolkits

You can view all the installed toolkits in your codebase: 
``` batch
automate list toolkits
```
or
``` batch
automate list all
```

!!! info
    This command will list all the installed toolkits and their respective names and versions.

### Upgrading a toolkit

If a toolkit is changed and upgraded by its creator, and then it can be upgraded in your codebase.

You first need to install the upgraded toolkit. This will automatically upgrade the older version of the toolkit

To upgrade a toolkit to a new version:
``` batch
automate install toolkit <INSTALLLOCATION>
```

* The `<INSTALLLOCATION>` is the full path to the upgraded toolkit file. e.g. `C:\desktop\AToolkit_0.2.0.toolkit`

!!! example
    ``` batch
    automate install toolkit "C:\desktop\AToolkit_0.2.0.toolkit"
    ```

Next, you will need to upgrade any drafts that you may have created from the previous version of the toolkit

!!! warning
    If you do not explicitly upgrade your draft to the latest toolkit version, then you will receive an error and instructions to upgrade it.

#### Upgrading drafts

To upgrade an existing draft to a newly upgraded toolkit:
``` batch
automate upgrade draft
```

* The `--force` optionally bypasses any errors that prevent automatic upgrading due to breaking changes. The default is `false`

!!! warning
    This upgrade will only upgrade the current draft, and not others you may have. You will have to [switch](#switching-drafts) to the other drafts and re-run this command on them to upgrade them individually.

#### Upgrades failing with breaking changes

When you try to upgrade an existing draft with a toolkit that contained breaking changes in it (relative to the last version of the toolkit), the upgrade of the draft will fail with an error.

!!! info
    A breaking change is determined from the new semantic version of the upgraded toolkit.

It is possible that the new toolkit may now have structure or automation that might not be compatible with the configuration of the existing draft. It is impossible to automatically tell with 100% certainty what is compatible between the last toolkit version and the current toolkit version. However, this is not all that common. Most upgrades should occur safely and automatically.

!!! tip
    For example, the existing draft may have some configuration in it that is no longer used by the new toolkit, or the new toolkit may require new configuration in it that the draft does not have yet. Then the upgrade path is not straightforward. There are some cases where these incompatibilities can be automatically resolved correctly in the upgrade process, but there are equally some cases that may not be resolved automatically, and you would need to detect and resolve them yourself or accept the possibility of data loss or misconfiguration. Sometimes, in rare cases, re-creating the draft from scratch is the only reliable option.

It is however possible to "force" the automatic upgrade process (despite the breaking changes) using the `--force` option.

!!! warning
    You should consult the creator of the toolkit if this is safe before doing this, or take the risk, and mitigate it using source control tools manually.

## Drafts

[A Draft](reference.md#draft) is a major top-level concept in automate, used to apply coding patterns to a codebase.

A Draft can only be created when a toolkit containing a pattern is installed.

### Creating a draft from a toolkit

To create a draft from an installed toolkit:
``` batch
automate run toolkit "<TOOLKITNAME>" --name "<DRAFTNAME>"
```

* The `TOOLKITNAME` is the name of the toolkit you want to use
* The `--name <DRAFTNAME>` optionally defines a friendly name for this instance of the draft.

!!! info
    This command will create a new draft and make the 'active' draft.

!!! tip
    Each Draft should be named for easy future reference. If you don't define a name (`--name`), one will be fabricated for you automatically. The name is useful for keeping track of which draft you are using right now, and which have been used in the past. Since they may have a long life in your codebase, and you may have several.

!!! example
    ``` batch
    automate run toolkit "AToolkitName" --name MyFirstUsage
    ```

### Switching drafts

Since you can have multiple drafts on the go at the same time (even from different toolkits) you will need a way to track them and which one is in use right now.

To list all the drafts you have right now:
``` batch
automate list drafts
```
or
``` batch
automate list all
```

!!! info
    This command will produce a list of drafts, and their names and IDs.

To switch to using a specific draft:
``` batch
automate run switch "<DRAFTID>"
```

* The `<DRAFTID>` is the ID of the draft you want to use.

!!! example
    ``` batch
    automate run switch 12345678
    ```

### Viewing the current draft

You can view the current draft:
``` batch
automate view draft
```

!!! info
    This command will display the name of the draft and its current configuration.

### Configuring a draft

The configuration of a specific draft is dependent on the structure and automation of the specific "Pattern" that is captured in the toolkit you are using.

!!! warning
    An existing draft (on a local machine) cannot be configured any further if its installed toolkit is either missing or has been upgraded (on the local machine).

All patterns have the same kind of meta-model behind them, that is, all patterns have these characteristics:

1. They all have a single root `Element`, and it has a name.
2. A root element can have one or more child `Element's (a hierarchy of elements)
3. An element (root or child) can either be a single `Element` or a `Collection` of elements
4. An element (root or child) can have one or more `Attribute`s
5. An attribute has a name and a value
6. Any element can have one or more `LaunchPoint`s attached to it.

All patterns may have additional automation like code templates, and other commands that can be used to automate the toolkit.

The specific combinations of these structural elements and automation are entirely dependent on how the pattern and toolkit were defined by its creator.

Following is are the common kinds of things you can do with any specific draft.

### View the current configuration of a draft

To view the current configuration of your draft:
``` batch
automate view draft
```

!!! info
    This command will only show you the data that you configured for the draft, and any defaulted values.

You can also ask the draft if it is currently in a "valid" state (i.e. if it requires further configuration), and look at the structure and any launchable automation (launch points) of the toolkit that you can use.

To view any constraint violations (validations):
``` batch
automate view draft --todo
```

!!! info
    This command will show you the current configuration data, AND the meta-model of the pattern, AND the launchable automation (launch points), AND it will also show you any validation errors with the draft that need fixing right now.

!!! info
    The validation rule violations will guide you to what must be addressed to put the draft in a "valid" state. Which is required if you want to execute any launch points on it.

### Validating a draft

To validate the draft:
``` batch
automate validate draft
```

!!! info
    Any validation rule violations are reported. These will need to be addressed before any launch points can be executed on it.

To validate specific items in the draft:
``` batch
automate validate draft --on "{<ANEXPRESSION>}"
```

* The `--on <ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to an existing item in the draft.

!!! example
    On the draft:
    ``` batch
    automate validate draft
    ```
    On an element:
    ``` batch
    automate validate draft --on "{APatternName.AnElementName}"
    ```
    On a collection item:
    ``` batch
    automate validate draft --on "{APatternName.ACollectionName.12345678}"
    ```

### Configuring an attribute

To set an attribute on any element (or any collection item) in the draft:
``` batch
automate configure on "{<ANEXPRESSION>}" --and-set "<ANATTRIBUTENAME>=<VALUE>"
```

- The `on <ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to an existing element (or collection item) in the draft
- The `--and-set <ANATTRIBUTENAME>=<VALUE>` is the name-value pair of the attribute and the value you wish to set to it.

!!! tip
    You can have many `--and-set "NAME=VALUE"` expressions as you like (one after the other) for the same element or collection item

!!! example
    On an element:
    ``` batch
    automate configure on "{APatternName.AnElementName}" --and-set "APropertyName1=avalue1"  --and-set "APropertyName2=avalue2"
    ```
    On a collection item:
    ``` batch
    automate configure on "{APatternName.ACollectionName.12345678}" --and-set "APropertyName1=avalue1"  --and-set "APropertyName2=avalue2"
    ```

### Adding an element or collection

!!! tip
    By default, all child elements and collections will be automatically created at the same time as their parent elements are created, but only if the child element is defined as `AutoCreate=true` in the toolkit.

To add an element or collection to any other element (or to any collection item) in the draft:
``` batch
automate configure add "{<ANEXPRESSION>}" --and-set "<ANATTRIBUTENAME>=<VALUE>"
```

- The `<ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to the non-existent element in the draft
- The `--and-set "<ANATTRIBUTENAME>=<VALUE>"` is the name-value pair of the attribute and the value you wish to set to it.

!!! tip
    You can also add as many `--and-set "NAME=VALUE"` expressions as you like (one after the other) on the same element or collection item

!!! example
    An element:
    ``` batch
    automate configure add "{APatternName.AnElementName}" --and-set "APropertyName1=avalue1"  --and-set "APropertyName2=avalue2"
    ```
    A collection:
    ``` batch
    automate configure add "{APatternName.ACollectionName}" --and-set "APropertyName1=avalue1"  --and-set "APropertyName2=avalue2"
    ```
    An element of a collection item:
    ``` batch
    automate configure add "{APatternName.ACollectionName.12345678.AnElementName}" --and-set "APropertyName1=avalue1"  --and-set "APropertyName2=avalue2"
    ```

### Resetting an element

To reset all the attributes of any element (or any collection item) in the draft:
``` batch
automate configure reset "{<ANEXPRESSION>}"
```

* The `<ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to an existing element (or collection item) in the draft

!!! example
    On an element:
    ``` batch
    automate configure reset "{APatternName.AnElementName}"
    ```
    On a collection item:
    ``` batch
    automate configure reset "{APatternName.ACollectionName.12345678}"
    ```

### Adding collection items

To add a new item of a collection to any collection in the draft:
``` batch
automate configure add-one-to "{<ANEXPRESSION>}" --and-set "<ANATTRIBUTENAME>=<VALUE>"
```

- The `<ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to an existing collection in the draft
- The `--and-set "<ANATTRIBUTENAME>=<VALUE>"` is the name-value pair of the attribute and the value you wish to set to it.

!!! tip
    You can also add as many `--and-set "NAME=VALUE"` expressions as you like to the collection item to make configuring its attributes easier

!!! tip
    When a collection item is added, if it has any child elements/collections, those child elements/collections are created by default if they are defined as `AutoCreate=true`. Otherwise, you need to add them separately.

!!! example
    To a collection:
    ``` batch
    automate configure add-one-to "{APatternName.ACollectionName}" --and-set "APropertyName1=avalue1"  --and-set "APropertyName2=avalue2"
    ```
    To a collection of a collection item:
    ``` batch
    automate configure add-one-to "{APatternName.ACollectionName.12345678.ACollectionName}" --and-set "APropertyName1=avalue1"  --and-set "APropertyName2=avalue2"
    ```

### Clearing collection items

To clear all items of a collection in the draft:
``` batch
automate configure clear "{<ANEXPRESSION>}"
```

- The `<ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to an existing collection in the draft

!!! example
    Of a collection:
    ``` batch
    automate configure clear "{APatternName.ACollectionName}"
    ```
    Of a collection of a collection item:
    ``` batch
    automate configure clear "{APatternName.ACollectionName.12345678.AnAlementName}"
    ```

### Deleting elements, collections or collection items

To delete an element, a collection, or an item of a collection in the draft:
``` batch
automate configure delete "{<ANEXPRESSION>}"
```

- The `<ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to an existing element, collection or collection item in the draft

!!! example
    An element:
    ``` batch
    automate configure delete "{APatternName.AnElementName}"
    ```
    A collection:
    ``` batch
    automate configure delete "{APatternName.ACollectionName}"
    ```
    A collection item:
    ``` batch
    automate configure delete "{APatternName.ACollectionName.12345678}"
    ```

### Executing automation

!!! abstract "Concept"
    A "Launch Point" is the mechanism that executes some kind of automation (or set of commands). Any element or collection item may have one or more launch points defined on it that can be executed at specific times.

To view all the launch points configured on the elements/collections within the toolkit:
``` batch
automate view toolkit --all
```

To view all the launch points available on the draft:
``` batch
automate view draft --todo
```

!!! info
    All launch points require that the entire draft is in a "valid" state before they are allowed to execute, no matter what element/collection the launch point is configured on.

To execute a launch point on any element (or collection) in the draft:
``` batch
automate execute command "<LAUNCHPOINTNAME>" --on "{<ANEXPRESSION>}"
```

- The `<LAUNCHPOINTNAME>` is the name of the launch point defined on the respective
- The  `--on <ANEXPRESSION>` is an [Expression](reference.md#draft-expressions) to an existing parent element/collection in the draft.

!!! tip
    You can omit the `--on "{<ANEXPRESSION>}"` if the command is defined on the root element.

!!! example
    On the pattern:
    ``` batch
    automate execute command "ACommandName"
    ```
    On a nested collection item:
    ``` batch
    automate execute command "ACommandName" --on "{APatternName.ACollectionName.12345678}"
    ```