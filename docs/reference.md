# Reference

## Key Concepts

Here are the key concepts in automate.

### Pattern

A "Pattern" is used to describe an existing piece of software in a codebase, in terms of its structure, its variance, and its usage.

Think of a pattern as some higher-level model of some software, or software concept(s), that includes various lower-level components or abstractions.

1. **Language**: Most software concepts can be represented conceptually as just boxes and lines, with some notation for cardinality  (i.e. OneToOne, OneToMany). Every existing coding pattern (in a codebase) is physically manifested as either just code files (or a particular programming language) or in a combination of code and configuration files - depending on the programming languages and the runtimes being used.
2. **Structure**: Any "Pattern" can be modeled structurally as a hierarchy of "Element"(s), where each element can have one or more child "Elements" of its own - describing a directed graph of related elements.
3. **Variance**: The variance in a coding pattern can be described by "Attributes" on one or more of the "Element(s)" within the pattern.
4. **Automation**: Each "Element" in the pattern can be associated with its own custom "Automation" which can read the structure and variance of the pattern, and can then perform operations with the data in these attributes, to adapt/augment/generate/configure code and configuration assets in a real codebase to realize specific use-cases.
5. **Constraints**: "Validation" Rules and constraints can be defined anywhere in the pattern and its structure, and they are enforced so that the configuration of a pattern maintains its intended integrity when applied to real use-cases.
6. **Draft**: Once a pattern is defined by its structure, automation, and constraints, it can then be used as a "Template" to create one or more individual use-cases. An individual use-case is represented in a "Draft" of the pattern. Each "Draft" can then be configured with data from a specific use-case, which can then be "applied" to another codebase. Multiple drafts of the same pattern are then maintained.

### Toolkit

A "Toolkit" is a versioned, published, packaged Pattern with its assets and automation, that is portable and sharable, installable on any platform.

It is in a form that can be easily installed and used by contributors on a codebase to get things done quicker by configuring them and having them execute automation.

Once installed, it can be source controlled, versioned, and upgraded.

It is specific to one kind of codebase, or specific to a group of contributors on a set of specific codebase types.

### Draft

A "Draft" is simply an instance of the toolkit being used, a like a use-case.

You can have more than one instance of a draft at any time. A draft notionally represents one use of a toolkit.

You configure a draft, and then instruct it to apply its patterns and templates to your code.

So for example, if your toolkit automated the use of a pattern in your codebase for a "RESTAPI", then each instance of a "REST API" (for your codebase) would be contained in a draft of its own - ready for you to configure it. If you wanted more REST APIs in your codebase, then you create multiple drafts from the "RESTAPI" toolkit.

## Pattern Expressions

A pattern expression is a reference to a specific element/collection (node) in a configured pattern.

* The expression is always wrapped in curly braces. It starts with a `{` and ends with a `}`
* It can begin with the name of the root element in the pattern, or this name can be omitted.
* Each part of the expression is separated by a period `.`
* Each part references the name of the child element

### Example pattern expressions

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

Examples pattern references:

* the root element: `{APatternName}`
* the first child element: `{APatternName.AnElementName1}` or `{AnElementName1}`
* the first grand-child element: `{APatternName.AnElementName1.AnElementName2}` or `{AnElementName1.AnElementName2}`
* the first child collection: `{APatternName.ACollectionName1}` or `{ACollectionName1}`
* the first element of the first child collection: `{APatternName.ACollectionName1.AnElementName3}` or `{ACollectionName1.AnElementName3}`

## Templating Expressions

* A templating expression is a reference to a specific "item" (node) in a configured draft.
* The expression always references the value of an attribute in the draft. References to elements/collections don't have values.
* The expression is always wrapped in double curly braces. It starts with a `{{` and ends with a `}}`
* The expression is always relative to the current "item" (node) on the configured draft.
* The expression can include the reference `.Parent` to access its' ancestry, starting from its' self.
* Each part of the expression is separated by a period `.`

### Example templating expressions

Given a pattern (cmd: `automate view pattern`):

```
- APatternName (root element)
    - AnAttributeName1 (attribute)
    - AnElementName1 (element)
        - AnAttributeName2 (attribute)
        - AnElementName2 (element)
            - AnAttributeName3 (attribute)
    - ACollectionName1 (collection)
        - AnAttributeName4 (attribute)
        - AnElementName3 (element)
            - AnAttributeName5 (attribute)
```

And, given that we have configured all instances of all elements and all attributes with some value, AND we have added 3x items to `ACollectionName1`.

If you can imagine that we were to ask for the configuration of a draft built with this toolkit (cmd: `automate view draft`), we would imagine having a draft configured something like this:

```
{
    "Id": "1",
    "ConfigurationPath": "{APatternName}",
    "AnAttributeName1": "avalue1",
    "AnElementName1": {
        "Id": "2",
        "ConfigurationPath": "{APatternName.AnElementName1}",
        "AnAttributeName2": "avalue2",
        "AnElementName2": {
            "Id": "3"
            "ConfigurationPath": "{APatternName.AnElementName1.AnElementName2}",
            "AnAttributeName3": "avalue3",
        }
    },
    "ACollectionName1": {
        "Id": "4",
        "ConfigurationPath": "{APatternName.ACollectionName1}",
        "AnAttributeName4": null,
        "Items": [{
                "Id": "5",
                "ConfigurationPath": "{APatternName.ACollectionName1.5}",
                "AnAttributeName4": "avalue4",
                "AnElementName3": {
                    "Id": "6",
                    "ConfigurationPath": "{APatternName.ACollectionName1.5.AnElementName3}",
                    "AnAttributeName5": "avalue5",
                }
            }, {
                "Id": "7",
                "ConfigurationPath": "{APatternName.ACollectionName1.7}",
                "AnAttributeName4": "avalue4",
                "AnElementName3": {
                    "Id": "8",
                    "ConfigurationPath": "{APatternName.ACollectionName1.7.AnElementName3}",
                    "AnAttributeName5": "avalue5",
                }
            }, {
                "Id": "9",
                "ConfigurationPath": "{APatternName.ACollectionName1.9}",
                "AnAttributeName4": "avalue4",
                "AnElementName3": {
                    "Id": "10",
                    "ConfigurationPath": "{APatternName.ACollectionName1.9.AnElementName3}",
                    "AnAttributeName5": "avalue5",
                }
            }
        ]
    }
}
```

Examples of templating references:

!!! info
    The casing of the names of properties, elements and collections are as defined in the pattern. However, the casing of the built-in properties: `Id`, and `Items` are always pascal-cased.

(ignoring the nominal values of the "Id" properties)

Starting from the root element (`APatternName`):

* an attribute (on self): `{{AnAttributeName1}}` (equals `avalue1`)
* an attribute of the first child element (ID: 2): `{{AnElementName1.AnAttributeName2}}` (equals `avalue2`)
* an attribute of the first grand-child element (ID: 3): `{{AnElementName1.AnElementName2.AnAttributeName3}}` (equals `avalue3`)
* an attribute of the first child collection (ID: 4): `{{ACollectionName1.AnAttributeName4}}` (will always equal null, since this collection is ephemeral)
* an attribute of the first child collection item (ID: 5): `{{ACollectionName1.5.AnAttributeName4}}` (equals `avalue4`)
* an attribute of the first element of the first child collection item (ID: 6): `{{ACollectionName1.5.AnElementName3.AnAttributeName5}}` (equals `avalue5`)
* an attribute of the first element of the last child collection item (ID: 9): `{{ACollectionName1.9.AnElementName3.AnAttributeName5}}` (equals `avalue5`)

Starting from the element `AnElementName2`:

* an attribute (on self): `{{AnAttributeName3}}` (equals `avalue3`)
* an attribute of the first child element (ID: 2): `{{Parent.AnAttributeName2}}` (equals `avalue2`)
* an attribute of the root element (ID: 1): `{{Parent.Parent.AnAttributeName1}}` (equals `avalue1`)
* an attribute of the first child collection (ID: 4): `{{Parent.Parent.ACollectionName1.AnAttributeName4}}` (will always equal null, since this collection is ephemeral)
* an attribute of the first child collection item (ID: 5): `{{Parent.Parent.ACollectionName1.5.AnAttributeName4}}` (equals `avalue4`)
* an attribute of the first element of the first child collection item (ID: 6): `{{Parent.Parent.ACollectionName1.5.AnElementName3.AnAttributeName5}}` (equals `avalue5`)

Starting from the "collection item"  `AnElementName3`:

* an attribute (on self): `{{AnAttributeName5}}` (equals `avalue5`)
* an attribute of the first child element (ID: 2): `{{Parent.Parent.AnElementName1.AnAttributeName2}}` (equals `avalue2`)
* an attribute of the root element (ID: 1): `{{Parent.Parent.AnAttributeName1}}` (equals `avalue1`)
* an attribute of the first child collection (ID: 4): `{{Parent.Parent.ACollectionName1.AnAttributeName4}}` (will always equal null, since this collection is ephemeral)
* an attribute of the first child collection item (ID: 5): `{{Parent.Parent.ACollectionName1.5.AnAttributeName4}}` (equals `avalue4`)

!!! tip
    When navigating up from a "collection item", its parent is not the [ephemeral] collection itself, but the parent of that [ephemeral] collection.

## Templating Functions

As part of any expression, you can also use any language feature of Scriban, found in [this reference](https://github.com/scriban/scriban/blob/master/doc/language.md)

For example, conditional `if` statements, `for` statements, and other common operations and functions.

Scriban also supports numerous [built-in functions](https://github.com/scriban/scriban/blob/master/doc/builtins.md) for strings and other common data types.

For example, to convert an expression to lowercase, you would write something like: `{{model.AnAttributeName | string.downcase}}`

automate adds the following additional custom functions:

* **CamelCase**: `{{model.AnAttributeName | string.camelcase}}` to convert any string value to a camel-cased string, where the first letter of each word in the string is lowercased, and all spaces are removed.

* **PascalCase**: `{{model.AnAttributeName | string.pascalcase}}` to convert any string value to a pascal-cased string, where the first letter of each word in the string is upper-cased, and all spaces are removed.

* **Pluralize**: `{{model.AnAttributeName | string.pluralize}}` to convert any string value to its pluralized form.  (e.g. duck -> ducks)

* **Singularize**: `{{model.AnAttributeName | string.singularize}}` to convert any string to its singular form. (e.g. ducks -> duck)

* **CamelPlural**: `{{model.AnAttributeName | string.camelplural}}` to convert any string value to its camel-cased and pluralized form.  (e.g. Duck -> ducks)

* **PascalPlural**: `{{model.AnAttributeName | string.pascalplural}}` to convert any string value to its pascal-cased pluralized form.  (e.g. duck -> Ducks)

* **CamelSingular**: `{{model.AnAttributeName | string.camelsingular}}` to convert any string to its camel-cased singularized form. (e.g. Ducks -> duck)

* **PascalSingular**: `{{model.AnAttributeName | string.pascalsingular}}` to convert any string to its pascal-cased singularized form. (e.g. ducks -> Duck)

## Draft Expressions

* A draft expression is a reference to a specific "item" (node) in a configured draft.
* The expression is always wrapped in curly braces. It starts with a `{` and ends with a `}`
* It always begins with the name of the root element in the draft. This is the name of the pattern (and the name of the toolkit)
* Each part of the expression is separated by a period `.`
* Each part references the name of the child element, except where the child element is an instance of a collection, in which case its ID is used as its name

### Example draft expressions

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
    "ConfigurationPath": "{APatternName}",
    "AnAttributeName1": "avalue1",
    "AnElementName1": {
        "Id": "2",
        "ConfigurationPath": "{APatternName.AnElementName1}",
        "AnElementName2": {
            "Id": "3"
            "ConfigurationPath": "{APatternName.AnElementName1.AnElementName2}",
        }
    },
    "ACollectionName1": {
        "Id": "4",
        "ConfigurationPath": "{APatternName.ACollectionName1}",
        "AnAttributeName2": null,
        "Items": [{
                "Id": "5",
                "ConfigurationPath": "{APatternName.ACollectionName1.5}",
                "AnAttributeName2": "avalue2",
                "AnElementName3": {
                    "Id": "6",
                    "ConfigurationPath": "{APatternName.ACollectionName1.5.AnElementName3}",
                }
            }, {
                "Id": "7",
                "ConfigurationPath": "{APatternName.ACollectionName1.7}",
                "AnAttributeName2": "avalue2",
                "AnElementName3": {
                    "Id": "8",
                    "ConfigurationPath": "{APatternName.ACollectionName1.7.AnElementName3}",
                }
            }, {
                "Id": "9",
                "AnAttributeName2": "avalue2",
                "ConfigurationPath": "{APatternName.ACollectionName1.9}",
                "AnElementName3": {
                    "Id": "10",
                    "ConfigurationPath": "{APatternName.ACollectionName1.9.AnElementName3}",
                }
            }
        ]
    }
}
```

Examples draft references:

!!! info
    The casing of the names of properties, elements and collections are as defined in the pattern. However, the casing of the built-in properties: `Id`, and `Items` are always pascal-cased.

(ignoring the nominal values of the "Id" properties)

* the root element (ID: 1): `{APatternName}`
* the first child element (ID: 2): `{APatternName.AnElementName1}`
* the first grand-child element (ID: 3): `{APatternName.AnElementName1.AnElementName2}`
* the first child collection (ID: 4): `{APatternName.ACollectionName1}`
* the first collection item (ID: 5): `{APatternName.ACollectionName1.5}`
* the element of the first collection item (ID: 6): `{APatternName.ACollectionName1.5.AnElementName3}`
* the element of the last collection item (ID: 10): `{APatternName.ACollectionName1.9.AnElementName3}`

## The CLI Interface

These are the CLI commands currently available in the Command Line Interface.

<img alt="CLI commands" src="https://github.com/jezzsantos/automate/blob/main/docs/Images/CLI.jpg?raw=true" width="800" />
