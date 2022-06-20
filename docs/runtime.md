# Using Toolkits

## Toolkits

**Concept**: A "Toolkit" is packaged Pattern with its assets and automation, that is portable and sharable, installable. It is in a form that can be easily installed and used by contributors on a codebase to get things done quicker by configuring them and having them execute automation. Once installed, it can be source controlled, versioned, and upgraded. It is specific to one kind of codebase, or specific to a group of contributors on a set of specific codebase types.

### Obtaining a toolkit

Toolkits come in `*.toolkit` files, obtained from their creators who [make and publish them](authoring.md). For example: from a tech lead, or tech consultant.

Since they are single files they can be: emailed, dropped in Slack, downloaded from Dropbox, or shared in all the common ways that files can be transferred to you.

### Installing a toolkit

You will need to have an `automate` CLI or plugin installed to install or use any toolkit. (see [Installing Automate](about.md#installing-automate))

Then, you will need to have downloaded a `*.toolkit` file to your local machine. For example: `AToolkit_0.1.0.toolkit`, possibly located on your desktop. 

1. In a terminal, navigate to the root of your source codebase (where you want to use the toolkit). For Example: `cd C:\projects\myproject\src` 
2. Install the toolkit: `automate install toolkit C:\desktop\AToolkit_0.1.0.toolkit`

The toolkit will be installed and it will become the 'active' toolkit in use.

> Once a toolkit has been installed into a codebase by anyone working on that codebase, it will be added to source control, and then be accessible to all contributors on a team. No need for every contributor to install the toolkit after this point. However, every contributor will still need to install `automate` to their machine to use the toolkit.

### Listing installed toolkits

You can view all the installed toolkits in your codebase: `automate list toolkits` which will list all the installed toolkits and their respective names and versions.

## Drafts

Once a toolkit is installed, it can be used to create a 'Draft'. 

**Concept**: A "Draft" is simply an instance of the toolkit being used, a like a use-case. You can have more than one instance of a draft at any time. A draft notionally represents one use of a toolkit. You configure a draft, and then instruct it to apply its patterns and templates to your code.

So for example, if your toolkit automated the use of a pattern in your codebase for a "RESTAPI", then each instance of a "REST API" (for your codebase) would be contained in a draft of its own - ready for you to configure it. If you wanted more REST APIs in your codebase, then you create multiple drafts from the "RESTAPI" toolkit.

### Creating a draft from a toolkit

To create a draft from an installed toolkit: `automate run toolkit "<TOOLKITNAME>" --name "<DRAFTNAME>"`

* The `TOOLKITNAME` is the name of the toolkit you want to use
* The `--name <DRAFTNAME>` optionally defines a friendly name for this instance of the draft.

> Note: Each Draft should be named for easy future reference. If you don't define a name (`--name`), one will be fabricated for you automatically. The name is useful for keeping track of which draft you are using right now, and which have been used in the past. Since they may have a long life in your codebase, and you may have several.

### Switching drafts

Since you can have multiple drafts on the go at the same time (even from different toolkits) you will need a way to track them and which one is in use right now.

To list all the drafts you have right now: `automate list drafts` 

> This command will produce a list of drafts, and their names and IDs.

To switch to using a specific draft: `automate run switch "<DRAFTID>"`

* The `<DRAFTID>` is the ID of the draft you want to use.

### Viewing the current draft

You can view the current draft: `automate view draft` 

> This command will display the name of the draft and its current configuration.

### Configuring a draft

The configuration of a specific draft is dependent on the structure and automation of the specific "Pattern" that is captured in the toolkit you are using.

> Note: An existing draft (on a local machine) cannot be configured any further if its installed toolkit is either missing or has been upgraded (on the local machine).

All patterns have the same kind of meta-model behind them, that is, all patterns have these characteristics:

1. They all have a single root `Element`, and it has a name.
1. A root element can have one or more child `Element's (a hierarchy of elements)
1. An element (root or child) can either be a single `Element` or a `Collection` of elements
1. An element (root or child) can have one or more `Attribute`s
1. An attribute has a name and a value
1. Any element can have one or more `LaunchPoint`s attached to it.

All patterns may have additional automation like code templates, and other commands that can be used to automate the toolkit.

The specific combinations of these structural elements and automation are entirely dependent on how the pattern and toolkit were defined by its creator.

Following is are the common kinds of things you can do with any specific draft.

### View the current configuration of a draft

To view the current configuration of your draft: `automate view draft` 

>  This command will only show you the data that you configured for the draft, and any defaulted values.

You can also ask the draft if it is currently in a "valid" state (i.e. if it requires further configuration), and look at the structure and any launchable automation (launch points) of the toolkit that you can use.

To view any constraint violations (validations):  `automate view draft --todo` 

> This command will show you the current configuration data, AND the meta-model of the pattern, AND the launchable automation (launch points), AND it will also show you any validation errors with the draft that need fixing right now.

> The validation rule violations will guide you to what must be addressed to put the draft in a "valid" state. Which is required if you want to execute any launch points on it.

### Validating a draft

To validate the draft: `automate validate draft`

> Any validation rule violations are reported. These will need to be addressed before any launch points can be executed on it.

To validate specific items in the draft: `automate validate draft on "{<ANEXPRESSION>}"`

*  The `on <ANEXPRESSION>` is an [Expression](#draft-expressions) to an existing item in the draft.

### Configuring an attribute

To set an attribute on any element (or any collection item) in the draft: `automate configure on "{<ANEXPRESSION>}" --and-set "<ANATTRIBUTENAME>=<VALUE>"` 

- The `on <ANEXPRESSION>` is an [Expression](#draft-expressions) to an existing element (or collection item) in the draft
- The `--and-set <ANATTRIBUTENAME>=<VALUE>` is the name-value pair of the attribute and the value you wish to set to it. 

> You can have many `--and-set "NAME=VALUE"` expressions as you like (one after the other) for the same element or collection item


### Adding an element

If an element does not yet exist in the draft, you can add it. 

>  By default, all elements will be automatically created when their parent elements are created, but only if they are defined as`AutoCreate=true`.

To add an element to any other element (or to any collection item) in the draft: `automate configure add "{<ANEXPRESSION>}" --and-set "<ANATTRIBUTENAME>=<VALUE>"`

- The `ANEXPRESSION>` is an [Expression](#draft-expressions) to the non-existent element in the draft
- The `--and-set <ANATTRIBUTENAME>=<VALUE>` is the name-value pair of the attribute and the value you wish to set to it. 

> You can also add as many `--and-set "NAME=VALUE"` expressions as you like (one after the other) on the same element or collection item

### Resetting an element

To reset all the attributes of any element (or any collection item) in the draft:  `automate configure reset "{<ANEXPRESSION>}"`

* The `<ANEXPRESSION>` is an [Expression](#draft-expressions) to an existing element (or collection item) in the draft

### Adding collection items

To add a new item of a collection to any collection in the draft: `automate configure add-one-to "{<ANEXPRESSION>}" --and-set "<ANATTRIBUTENAME>=<VALUE>"`

- The `<ANEXPRESSION>` is an [Expression](#draft-expressions) to an existing collection in the draft
- The `--and-set <ANATTRIBUTENAME>=<VALUE>` is the name-value pair of the attribute and the value you wish to set to it. 

> You can also add as many `--and-set "NAME=VALUE"` expressions as you like to the collection item to make configuring its attributes easier

> When a collection item is added, if it has any child elements/collections, those child elements/collections are created by default if they are defined as `AutoCreate=true`. Otherwise, you need to add them separately.

### Clearing collection items

To clear all items of a collection in the draft: `automate configure clear "{<ANEXPRESSION>}"`

- The `<ANEXPRESSION>` is an [Expression](#draft-expressions) to an existing collection in the draft

### Deleting elements, collections or collection items

To delete an element, a collection, or an item of a collection in the draft: `automate configure delete "{<ANEXPRESSION>}"`

- The `<ANEXPRESSION>` is an [Expression](#draft-expressions) to an existing element, collection or collection item in the draft

## Executing automation

**Concept**: A "Launch Point" is the mechanism that executes some kind of automation (or set of commands). Any element or collection item may have one or more launch points defined on it that can be executed at specific times.

To view all the launch points configured on the elements/collections within the toolkit: `automate view toolkit --all` 

To view all the launch points available on the draft: `automate view draft --todo`

> All launch points require that the entire draft is in a "valid" state before they are allowed to execute, no matter what element/collection the launch point is configured on.

To execute a launch point on any element (or collection) in the draft: `automate execute command "<LAUNCHPOINTNAME>" --on "{<ANEXPRESSION>}"` 

- The `<LAUNCHPOINTNAME>` is the name of the launch point defined on the respective
- The  `-on <ANEXPRESSION>` is an [Expression](#draft-expressions) to an existing parent element/collection in the draft.

> You can omit the `--on "{<ANEXPRESSION>}"` if the command is defined on the root element.

## Upgrading a toolkit

If a toolkit is changed and upgraded by its creator, and then it can be upgraded in your codebase.

You first need to install the upgraded toolkit, and this will upgrade the older version of the toolkit

To upgrade a toolkit to a new version: `automate install toolkit <INSTALLLOCATION>`

* The `<INSTALLLOCATION>` is the full path to the upgraded toolkit file. e.g. `C:\desktop\AToolkit_0.2.0.toolkit`

Next, you will need to upgrade any drafts that you may have created from the previous version of the toolkit

> WARNING: If you do not explicitly upgrade your draft to the latest toolkit version, then you will receive an error and instructions to upgrade it.

To upgrade an existing draft to a newly upgraded toolkit: `automate upgrade draft`

* The `--force` optionally bypasses any errors that prevent automatic upgrading due to breaking changes.  The default is `false`

> This upgrade will only upgrade the current draft, and not others you may have. You will have to switch to the other drafts and re-run this command on them to upgrade them individually.

### Upgrades failing with breaking changes

When you try to upgrade an existing draft with a toolkit that contained breaking changes in it (relative to the last version of the toolkit), the upgrade of the draft will fail with an error.

> A breaking change is determined from the new semantic version of the upgraded toolkit.

It is possible that the new toolkit may now have structure or automation that might not be compatible with the configuration of the existing draft.  It is impossible to automatically tell with 100% certainty what is compatible between the last toolkit version and the current toolkit version. However, this is not all that common. Most upgrades should occur safely and automatically.

> For example, the existing draft may have some configuration in it that is no longer used by the new toolkit, or the new toolkit may require new configuration in it that the draft does not have yet. Then the upgrade path is not straightforward. There are some cases where these incompatibilities can be automatically resolved correctly in the upgrade process, but there are equally some cases that may not be resolved automatically, and you would need to detect and resolve them yourself or accept the possibility of data loss or misconfiguration. Sometimes, in rare cases, re-creating the draft from scratch is the only reliable option.

It is however possible to "force" the automatic upgrade process (despite the breaking changes) using the `--force` option. 

> WARNING: You should consult the creator of the toolkit if this is safe before doing this, or take the risk, and mitigate it using source control tools manually.

## Reference

### Draft Expressions

* A draft expression is a reference to a specific "item" (node) in a configured draft.
* The expression is always wrapped in curly braces. It starts with a `{` and ends with a `}`
* It always begins with the name of the root element in the draft. This is the name of the pattern (and the name of the toolkit)
* Each part of the expression is separated by a period `.`
* Each part references the name of the child element, except where the child element is an instance of a collection, in which case its ID is used as its name

#### Example draft expressions

Given this pattern (in a toolkit):
```
- APatternName (root element)
    - AnAttributeName1 (attribute)
    - AnElementName1 (element)
        - AnElementName2 (element)
    - ACollectionName1 (collection)
        - AnAttributeName2 (attribute)
        - AnElementName3 (element)
```
Given that we have configured all instances of all elements and all attributes, AND we have added 3x instances of the `ACollectionName1` collection item.

If we were to ask for the configuration of this draft (`automate view draft`), we would have a draft configured with items looking something like this:

```
{
    "Id": "1",
    "AnAttributeName1": "avalue1",
    "AnElementName1": {
        "Id": "2",
        "AnElementName2": {
            "Id": "3"
        }
    },
    "aCollectionName1": {
        "Id": "4",
        "AnAttributeName2": null,
        "Items": [{
                "Id": "5",
                "AnAttributeName2": "avalue2",
                "AnElementName3": {
                    "Id": "6",
                }
            }, {
                "Id": "7",
                "AnAttributeName2": "avalue2",
                "AnElementName3": {
                    "Id": "8",
                }
            }, {
                "Id": "9",
                "AnAttributeName2": "avalue2",
                "AnElementName3": {
                    "Id": "10",
                }
            }
        ]
    }
}
```

Examples draft references:

>  The casing of elements and collection is as they are defined in the pattern. However, the casing of the built-in properties, such as: `Id`, and `Items` are always PascalCased.

(ignoring the nominal values of the "Id" properties)

* the root element (ID: 1): `{APatternName}`
* the first child element (ID: 2): `{APatternName.AnElementName1}`
* the first grand-child element (ID: 3): `{APatternName.AnElementName1.AnElementName2}`
* the first child collection (ID: 4): `{APatternName.ACollectionName1}`
* the first collection item (ID: 5): `{APatternName.ACollectionName1.5}`
* the element of the first collection item (ID: 6): `{APatternName.ACollectionName1.5.AnElementName3}`
* the element of the last collection item (ID: 10): `{APatternName.ACollectionName1.9.AnElementName3}`