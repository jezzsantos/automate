# Making Toolkits

[A Pattern](reference.md#pattern) is a major top-level concept in automate, used to describe coding patterns. When published, it is packaged and distributed in [A Toolkit](reference.md#toolkit) that is installed into a codebase.

!!! info
    To make toolkits, you will need to have the `automate` CLI (or plugin) installed on your machine. (see [Installing automate](installation.md))

## Capture a new pattern

In a terminal, navigate to the root of your source codebase.

Create a new pattern:
``` batch
automate create pattern "<PATTERNNAME>"
```

- The `<PATTERNNAME>` is the name of the new pattern. The name can only contain alphanumerical characters and the `.` character.

- The `--displayedas` is optional metadata that defines how the pattern might be displayed in a user interface.

- The `--describedas` is optional metadata that defines how the pattern might be displayed in a user interface.

!!! info
    This will create a new pattern, with a root element of the same name, and will set it as the "current" pattern for subsequent editing.

!!! example
    ``` batch
    automate create pattern "APatternName" --displayedas "A Pattern Name" --describedas "an example pattern"
    ```

!!! info
    This pattern automatically becomes the 'active' pattern for subsequent commands.

## Listing patterns

You can view all the patterns in your codebase:
``` batch
automate list patterns
```
or
``` batch
automate list all
```

!!! info
    Which will list all the patterns and their respective names and versions.

## Switching patterns

If you have multiple patterns going, you can switch between them using their ID:
``` batch
automate edit switch "<PATTERNID>"
```

!!! info
    Which makes this pattern the 'active' pattern for subsequent commands.

!!! example
    ``` batch
    automate edit switch 12345678
    ```

## Viewing the current pattern

You can view the current pattern:
``` batch
automate view pattern
```

!!! info
    Which will display the summarized configuration of the current pattern.

You can view the detailed configuration of the pattern:
``` batch
automate view pattern --all
```

!!! info
    Which will display detailed information about the pattern structure, configuration and its automation.

## Configuring pattern structure

The structure of a pattern describes a (conceptual) model of your code and its coding patterns.

* The simplest coding pattern, used for a single use-case, probably does not need much structure (hierarchy) or variance (attributes) to represent it. Since generating the code for it can all be hardcoded into one or more code templates (with no variance).

* If a pattern is to be used for multiple use-cases, some variance needs to be captured and configured by the person applying the actual use-case. This is where a hierarchy of elements, attributes, and automation comes to play to make a model of the software.

* Try to define the high-level concepts about your coding pattern (in a hierarchy of elements/collections), not necessarily representing detailed coding concepts like functions and variables, but more at the conceptual/component/module level. Then decorate your hierarchical elements/collections with attributes to describe what varies and to which concepts that variance would naturally be attributed.
* Construct elements (ZeroToOne) and collections (ZeroToMany) to help model your pattern and its instancing rules.

* Every pattern already has a single versioned root element, which initially has no attributes defined for it (except for a `DisplayName` and an empty `Description`).

### Update pattern

To update the name and metadata for a pattern:
``` batch
automate edit update-pattern
```

- The `--name` is an optional parameter and must be alphanumeric and can contain the following additional characters:`._`.

- The `--displayedas` is an optional parameter that defines how the pattern might be displayed in a user interface.

- The `--describedas` is an optional parameter that defines how the pattern might be displayed in a user interface.

!!! example
    ``` batch
    automate edit update-pattern --name "ANewName"
    ```

### Add attributes

!!! abstract "Concept"
    An "Attribute" is a means to represent the variability of a pattern (or the variance within a set of use-cases). When applied, they are essentially a name-value pair.

To add a new attribute to any element/collection in the pattern:
``` batch
automate edit add-attribute "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<NAME>` must be alphanumeric and can contain the following additional characters:`._`. The name must not be the same as any existing attribute or element/collection on the `--aschildof` element/collection. Cannot be one of the reserved names: `Id`, `DisplayName`, `Description`, `ConfigurePath`, `Schema` or `Items`.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the attribute to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern

- The `--isrequired` is an optional parameter that defines that the attribute must have a value when applied.

- The `--isoftype "string"` optional parameter defines the data type of the attribute. Valid values are: `string`, `bool`, `int`, `float`, `datetime`. The default is `string` when `--isoftype` is not defined.

- The `--defaultvalueis "<AVALUE>"` is optional. If defined, it must match the `--isoftype`, and must match `--isoneof` (if defined).

- The `--isoneof "<VALUE1>;<VALUE2>;<VALUE3>"` is optional, and is a `;` delimited list of specific values that represent the only values for this attribute, when applied.

!!! example
    To the pattern:
    ``` batch
    automate edit add-attribute "AnAttributeName" --isrequired --isoftype string --defaultvalueis "achoice3" --isoneof "achoice1;achoice2;achoice3"
    ```
    To an element:
    ``` batch
    automate edit add-attribute "AnAttributeName" --isrequired --isoftype string --defaultvalueis "achoice3" --isoneof "achoice1;achoice2;achoice3" --aschildof "{AnElementName}"
    ```

### Update attributes

To update an existing attribute on any element/collection in the pattern:
``` batch
automate edit update-attribute "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are updating the attribute on the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern

- The `--isrequired` optionally defines whether the attribute must have a value or not.

- The `--isoftype "string"` optionally defines the new data type of the attribute. Valid values are: `string`, `bool`, `int`, `float`, `datetime`. The default is `string` when `--isoftype` is not defined.

- The `--defaultvalueis "<AVALUE>"` optionally defines the new default value. If defined, it must match the `--isoftype`, and must match `--isoneof` (if defined).

- The `--isoneof "<VALUE1>;<VALUE2>;<VALUE3>"` optionally defines new choices, and is a `;` delimited list of specific values that represent the only values for this attribute, when applied.

- The `--name` optionally defines a new name for the attribute. It must be alphanumeric and can contain the following additional characters:`._`. The name must not be the same as any existing element/collection or attribute on the `--aschildof` element/collection. Must also not be named `Id`, or `DisplayName` or `Description`.

!!! example
    Of the pattern:
    ``` batch
    automate edit update-attribute "AnAttributeName" --name "ANewAttributeName" --isrequired false
    ```
    Of an element:
    ``` batch
    automate edit update-attribute "AnAttributeName" --name "ANewAttributeName" --isrequired false --aschildof "{AnElementName}"
    ```

### Delete attributes

To delete an existing attribute:
``` batch
automate edit delete-attribute "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

!!! tip
    The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are deleting an attribute from the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) of an existing element in the pattern.

!!! example
    Of the pattern:
    ``` batch
    automate edit delete-attribute "AnAttributeName"
    ```
    Of an element:
    ``` batch
    automate edit delete-attribute "AnAttributeName" --aschildof "{AnElementName}"
    ```

### Add elements

!!! abstract "Concept"
    An "Element" is a means to represent some relational hierarchy in a pattern, a means to relate one concept to another. Elements can be nested. An element can have a cardinality of `ZeroOrOne` or `One`, which determines whether it must exist when the pattern is applied.

!!! tip
    Use a collection instead of an element, if you want to represent other kinds of relationships (`ZeroToMany` or `OneOrMany`).

To add a new element to any element/collection in the pattern:
``` batch
automate edit add-element "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<NAME>` must be alphanumeric and can contain the following additional characters:`._`. The name must not be the same as any existing element/collection or attribute on the `--aschildof` element/collection. Cannot be one of the reserved names: `Id`, `DisplayName`, `Description`, `ConfigurePath`, `Schema` or `Items`.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the element to the root element. <ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--displayedas` is an optional parameter that defines how the element might be displayed in a user interface.

- The `--describedas` is an optional parameter that defines how the element might be described in a user interface.

- The `--isrequired` is an optional parameter that defines whether the element is required (`Cardinality=One`) or not required (`Cardinality=ZeroToOne`). By default, it is `true` (`Cardinality=One`).

- The `--autocreate` is an optional parameter that defines whether an instance of the element will be created automatically when the pattern is applied. By default, it is `true` if `--isrequired` is `true`, and `false` if `--isrequired` is `false`.

!!! example
    To the pattern:
    ``` batch
    automate edit add-element "AnElementName"
    ```
    To an element:
    ``` batch
    automate edit add-element "AnElementName" --aschildof "{AnotherElementName}"
    ```

### Update elements

To update an existing element on any element/collection in the pattern:
``` batch
automate edit update-element "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are updating the element on the root element. <ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--displayedas` optionally defines a new display name.

- The `--describedas` optionally defines a new description.

- The `--isrequired` optionally defines whether the element is required (`One`) or not required (`ZeroToOne`).

- The `--name` optionally defines a new name for the element. It must be alphanumeric and can contain the following additional characters:`._`. The name must not be the same as any existing attribute or element/collection on the `--aschildof` element/collection. Cannot be one of the reserved names: `Id`, `DisplayName`, `Description`, `ConfigurePath`, `Schema` or `Items`.

- The `--autocreate` is an optional parameter that defines whether an instance of the element will be created automatically when the pattern is applied.

!!! example
    Of an element:
    ``` batch
    automate edit update-element "AnElementName" --name "ANewName" --isrequired false --aschildof "{AnotherElementName}"
    ```

### Delete elements

To delete an existing element on any element/collection in the pattern:
``` batch
automate edit delete-element "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are deleting the element from the root element. <ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) of an existing element/collection in the pattern.

!!! example
    Of an element:
    ``` batch
    automate edit delete-element "AnElementName" --aschildof "{AnotherElementName}"
    ```

### Add collections

!!! abstract "Concept"
    A "Collection" is a special case of an "Element" that represents a collection of concepts. A collection is essentially an element with a choice of cardinality (`ZeroOrMany`, `OneOrMany`). When it is applied to a specific use-case, multiple instances of the element (described by the collection) are instantiated as (required `One`) elements.

To add a new collection to any element/collection in the pattern:
``` batch
automate edit add-collection "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<NAME>` must be alphanumeric and can contain the following additional characters:`._`. The name must not be the same as any existing element/collection or attribute on the `--aschildof` element/collection. Cannot be one of the reserved names: `Id`, `DisplayName`, `Description`, `ConfigurePath`, `Schema` or `Items`.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the element to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) of an existing element/collection in the pattern.

- The `--displayedas` is an optional parameter that defines how the element might be displayed in a user interface.

- The `--describedas` is an optional parameter that defines how the element might be described in a user interface.

- The `--isrequired` is an optional parameter that defines whether the collection must have at least one item within it (`Cardinality=OneToMany`) or not required (`Cardinality=ZeroToMany`). By default, it is `false` (`Cardinality=ZeroToMany`).

- The `--autocreate` is an optional parameter that defines whether an instance of the collection will be created automatically when the pattern is applied. By default, it is `true` if `--isrequired` is `true`, and `false` if `--isrequired` is `false`.

!!! example
    To the pattern:
    ``` batch
    automate edit add-collection "ACollectionName"
    ```
    To an element: 
    ``` batch
    automate edit add-collection "ACollectionName" --aschildof "{AnElementName}"
    ```

### Update collections

To update an existing collection on any element/collection in the pattern:
``` batch
automate edit update-collection "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are updating the collection on the root element. <ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--displayedas` optionally defines a new display name.

- The `--describedas` optionally defines a new description.

- The `--isrequired`optionally defines whether the collection must have at least one item within it (`OneToMany`) or not required (`ZeroToMany`).

- The `--name` optionally defines a new name for the collection. It must be alphanumeric and can contain the following additional characters:`._`. The name must not be the same as any existing attribute or element/collection on the `--aschildof` element/collection. Cannot be one of the reserved names: `Id`, `DisplayName`, `Description`, `ConfigurePath`, `Schema` or `Items`.

- The `--autocreate` is an optional parameter that defines whether an instance of the collection will be created automatically when the pattern is applied.

!!! example
    Of an element:
    ``` batch
    automate edit update-collection "ACollectionName" --name "ANewName" --isrequired false --aschildof "{AnElementName}"
    ```

### Delete collections

To delete an existing collection on any element/collection in the pattern:
``` batch
automate edit delete-collection "<NAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are deleting the collection from the root element. <ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) of an existing element/collection in the pattern.

!!! example
    Of an element:
    ``` batch
    automate edit delete-collection "ACollectionName" --aschildof "{AnElementName}"
    ```

## Configuring pattern automation

The structure of a pattern provides a convenient context for applying automation to it.

Each element/collection can be configured with one or more automation features, that can be used to realize or manifest the pattern when applied.

There are several concepts here. The first is "Commands" which can enact things on the pattern, make calculations, manipulations, etc. Then there are "Launch Points" which execute the commands in response to some trigger or stimulus. These launch points can be manually triggered by a human user or can be triggered in response to some event on the pattern, or some environmental event.

### Add code templates

!!! abstract "Concept"
    A "Code Template" is a way to capture any kind of code (or configuration viz: JSON, XML, etc) of a pattern so that when a use-case is realized, code artifacts can be injected/modified/augmented/inserted/generated into codebases in specific locations of the codebase. Once a piece of code has been captured by the pattern, it can be templatized by the author and marked up so that variance in a specific use-case can parameterize the actual code injected into a codebase.

To capture a piece of code:
``` batch
automate edit add-codetemplate "<FILEPATH>" --aschildof "{<ANEXPRESSION>}"
```

- The `FILEPATH` is a relative path to an existing code file locally.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the code template to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) of an existing element/collection in the pattern.

- The `--name` is an optional friendly name of the code template, which will be used to reference the code template when it is connected to automation later. If no name is specified, an automatic name is assigned to this code template.

!!! example
    To the pattern:
    ``` batch
    automate edit add-codetemplate "C:/projects/src/afilename.ext" --name "ATemplateName"
    ```
    To an element: 
    ``` batch
    automate edit add-codetemplate "C:/projects/src/afilename.ext" --name "ATemplateName" --aschildof "{AnElementName}"
    ```

### Editing code templates

!!! abstract "Concept"
    Once a code template has been added to a pattern it will then need to be annotated with [Templating Expressions](reference.md#templating-expressions). The code template exists inside the pattern (file structure), but the editing of it will need to be done in an external editor program (i.e. notepad.exe).

To edit the contents of an existing code template:
``` batch
automate edit codetemplate "<TEMPLATENAME>" --with "<APPLICATIONNAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `TEMPLATENAME` is the name of an existing code template on the `--aschildof` element/collection.

- The `--with "<APPLICATIONNAME>"` is either the name of an editor application (i.e. notepad.exe), or it is the absolute path to the editor application. This application is expected to take, as the first argument, the absolute path to the code template (on disk).

- The `--args` is an optional set of arguments to pass the application. These arguments will be added to the application before the absolute path to the code template (on disk).

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are deleting the code template from the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

!!! example
    Of the pattern (On Windows):
    ``` batch
    automate edit codetemplate "ATemplateName" --with "notepad"
    ```
    Of an element (On Windows): 
    ``` batch
    automate edit codetemplate "ATemplateName" --with "notepad" --aschildof "{AnElementName}"
    ```
    Of an element, with VS Code (On Windows): 
    ``` batch
    automate edit codetemplate "ATemplateName" --with "%localappdata%\Programs\Microsoft VS Code\code.exe" --aschildof "{AnElementName}"
    ```

### Delete code templates

To delete an existing code template:
``` batch
automate edit delete-codetemplate "<TEMPLATENAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `TEMPLATENAME` is the name of an existing code template on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are deleting the code template from the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

!!! example
    Of the pattern:
    ``` batch
    automate edit delete-codetemplate "ATemplateName"
    ```
    Of an element: 
    ``` batch
    automate edit delete-codetemplate "ATemplateName" --aschildof "{AnElementName}"
    ```

### Test code templates

!!! abstract "Concept"
    Code templates contain content that may contain [Templating Expressions](reference.md#templating-expressions). Once the template has been added to a pattern, the content can be tested with fake data to yield a test result. Fake data is arranged in the same structure of the pattern and values are populated in a sequential way. This data is then applied to the code template to give a test output. You can export this dummy data after the test, alter it manually, and then import it back to be used in a subsequent test.

To can test the contents of an existing code template:
``` batch
automate test codetemplate "<TEMPLATENAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `TEMPLATENAME` is the name of an existing code template on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are testing a code template on the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--export-data` optionally defines the relative path to a file that will be populated with the data that was used to test the template.

- The `--import-data` optionally defines the relative path to a file containing test data (in JSON) in the same structure as the pattern, relative to the current code template. Use the `--export-data` option to get a starting point to work from.

!!! example
    Of the pattern:
    ``` batch
    automate test codetemplate "ATemplateName"
    ```
    Of an element: 
    ``` batch
    automate test codetemplate "ATemplateName" --aschildof "{AnElementName}"
    ```

### Add code template commands

!!! abstract "Concept"
    A "Code Template Command" is simply a type of automation that executes a "Code Template". This automation must be wired up to a "Code Template", and a "Code Template" must have a "Code Template Command" wired to it, to be applied in any use-case. This kind of command is responsible for deciding how to render the "Code Template" into the target codebase (where in the codebase, and how its named).
    After a code template is rendered into a codebase, an "Artifact Link" is defined for the location of the rendered code. This link is then tracked and maintained on subsequent executions of this command. This is useful if rendered files are later renamed, or the `--targetpath` property of this command changes.

To add a new code template command to any element/collection in the pattern:
``` batch
automate edit add-codetemplate-command "<CODETEMPLATENAME>" --targetpath "<TARGETPATH>" --aschildof "{<ANEXPRESSION>}"
```

- The `<CODETEMPLATENAME>` is the name of an existing code template that must exist on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the command to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) of an existing element/collection in the pattern.

- The `--targetpath "<TARGETPATH>"` value describes the full path (including filename and file extension) of the code file when the command is applied. It can start with a `~` character to indicate that the path will be relative to the codebase where the toolkit will be installed. It can also be an absolute file path on the target machine (harder to predict). This expression may also contain [Templating Expressions](reference.md#templating-expressions) (relative to the element/collection of the value of `--aschildof`), that will be resolved when the command is applied.

- The `--isoneoff` optionally defines that the rendered code template will only be generated if it does not already exist on the local machine in the specified location with the specified name. Typically, this means that the code template is only rendered the first time the command is executed. The default is `false`.

- The `--name` optionally defines a name for the command. If none is given, a default name will be derived for the command.

!!! example
    To the pattern:
    ``` batch
    automate edit add-codetemplate-command "ATemplateName" --targetpath "~/apath/afilename.ext" --isoneoff
    ```
    To an element: 
    ``` batch
    automate edit add-codetemplate-command "ATemplateName" --targetpath "~/apath/afilename.ext" --isoneoff --aschildof "{AnElementName}"
    ```

### Add code template with command

!!! tip
    This command makes it possible to add a code template and add a new code template command (to render it) in one command.

To capture a piece of code and wire it up to a code template command:
``` batch
automate edit add-codetemplate-with-command "<FILEPATH>" --targetpath "<TARGETPATH>" --aschildof "{<ANEXPRESSION>}"
```

- The `FILEPATH` is a relative path to an existing code file locally.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the code template to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--targetpath "<TARGETPATH>"` value describes the full path (including filename and file extension) of the code file when the command is applied. It can start with a `~` character to indicate that the path will be relative to codebase where the toolkit will be installed. It can also be an absolute file path on the target machine (harder to predict). This expression may also contain [Templating Expressions](reference.md#templating-expressions) (relative to the element/collection of the value of `--aschildof`), that will be resolved when the command is applied.

- The `--isoneoff` optionally defines that the rendered code template will only be generated if it does not already exist on the local machine in the specified location with the specified name. Typically, this means that the code template is only rendered the first time the command is executed. The default is `false`.

- The `--name` is an optional friendly name of the code template, which will be used to reference the code template when it is connected to automation later. If no name is specified, an automatic name is assigned to this code template.

!!! example
    To the pattern:
    ``` batch
    automate edit add-codetemplate-with-command "C:/projects/src/afilename.ext" --name "ATemplateName" --targetpath "~/apath/afilename.ext" --isoneoff
    ```
    To an element: 
    ``` batch
    automate edit add-codetemplate-with-command "C:/projects/src/afilename.ext" --name "ATemplateName" --targetpath "~/apath/afilename.ext" --isoneoff --aschildof "{AnElementName}"
    ```

### Update code template commands

To update an existing code template command on any element/collection in the pattern:
``` batch
automate edit update-codetemplate-command "<COMMANDNAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<COMMANDNAME>` is the name of an existing command on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are updating a command on the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--targetpath` optionally defines a new full path (including filename and file extension) of the code file when the command is applied. It can start with a `~` character to indicate that the path will be relative to codebase where the toolkit will be installed. It can also be an absolute file path on the target machine (harder to predict). This expression may also contain [Templating Expressions](reference.md#templating-expressions) (relative to the element/collection of the value of `--aschildof`), that will be resolved when the command is applied.

- The `--isoneoff` optionally defines that the rendered code template will only be generated if it does not already exist on the local machine in the specified location with the specified name. Typically, this means that the code template is only rendered the first time the command is executed.

- The `--name` optionally defines a new name for the command.

!!! example
    Of the pattern:
    ``` batch
    automate edit update-codetemplate-command "ACommandName" --targetpath "~/anewpath/anewfilename.ext" --isoneoff false
    ```
    Of an element: 
    ``` batch
    automate edit update-codetemplate-command "ATemplateName" --targetpath "~/anewpath/anewfilename.ext" --isoneoff false --aschildof "{AnElementName}"
    ```

### Test code template commands

!!! abstract "Concept"
    Code template commands contain a "target path" that may contain [Templating Expressions](reference.md#templating-expressions). Once the command has been added to a pattern, the target path can be tested with fake data to yield a test result. Fake data is arranged in the same structure of the pattern and values are populated in a sequential way. This data is then applied to the command to give a test output. You can export this dummy data after the test, alter it manually, and then import it back to be used in a subsequent test.

To can test the contents of an existing code template command:
``` batch
automate test codetemplate-command "<COMMANDNAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `COMMANDNAME` is the name of an existing command on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are testing a command on the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--export-data` optionally defines the relative path to a file that will be populated with the data (in JSON) that was used to test the template.

- The `--import-data` optionally defines the relative path to a file containing test data (in JSON) in the same structure as the pattern, relative to the current code template. Use the `--export-data` option to get a starting point to work from.

!!! example
    Testing with default data:
    ``` batch
    automate test codetemplate-command "ACommandName"
    ```
    Testing with default data, and export that data: 
    ``` batch
    automate test codetemplate-command "ACommandName" --export-data "C:/projects/data/exportedtestdata.json"
    ```
    Testing with imported data: 
    ``` batch
    automate test codetemplate-command "ACommandName" --import-data "C:/projects/data/exportedtestdata.json"
    ```

### Add CLI commands

!!! abstract "Concept"
    A "CLI Command" is simply a type of automation that executes another command-line program (with arguments).

To add a new CLI command to any element/collection in the pattern:
``` batch
automate edit add-cli-command "<APPLICATIONNAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<APPLICATIONNAME>` can be a name of the program already in the PATH of the machine, or a fully qualified path to the program on the local machine.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the command to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--arguments <ARGUMENTS>` optionally defines the arguments to pass to the program. Double-quotes in the arguments must be escaped with double-quotes. The arguments may also contain [Templating Expressions](reference.md#templating-expressions) (relative to the element/collection of the value of `--aschildof`), which will be resolved when the command is applied.

- The `--name` optionally defines a name for the command. If none is given, a default name will be derived for the command.

!!! example
    To the pattern:
    ``` batch
    automate edit add-cli-command "C:/tools/atool.exe" --arguments "anargument1 anargument2" --name "ACommandName"
    ```
    To an element: 
    ``` batch
    automate edit add-cli-command "C:/tools/atool.exe" --arguments "anargument1 anargument2" --name "ACommandName" --aschildof "{AnElementName}"
    ```

### Update CLI commands

To update an existing CLI command on any element/collection in the pattern:
``` batch
automate edit update-cli-command "<COMMANDNAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<COMMANDNAME>` is the name of an existing command on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are updating the command to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--app <APPLICATIONNAME>` optionally defines the new name of the program already in the PATH of the machine, or a fully qualified path to the program on the local machine.

- The `--arguments <ARGUMENTS>` optionally defines the new arguments to pass to the program. Double-quotes in the arguments must be escaped with double-quotes. The arguments may also contain [Templating Expressions](reference.md#templating-expressions) (relative to the element/collection of the value of `--aschildof`), which will be resolved when the command is applied.

- The `--name` optionally defines a new name for the command.

!!! example
    Of the pattern:
    ``` batch
    automate edit update-cli-command --app "C:/tools/anothertool.exe" --arguments "anewnargument1 anewargument2" --name "ANewCommandName"
    ```
    Of an element: 
    ``` batch
    automate edit update-cli-command -app "C:/tools/anothertool.exe" --arguments "anewargument1 anewargument2" --name "ANewCommandName" --aschildof "{AnElementName}"
    ```

### Delete any commands

To delete any existing command on any element/collection in the pattern:
``` batch
automate edit delete-command "<COMMANDNAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<COMMANDNAME>` is the name of an existing command on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are deleting the command from the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

!!! tip
    Deleting a command that is referenced by a launch point (anywhere in the pattern) will remove that command identifier from the launch point configuration.

!!! example
    Of the pattern:
    ``` batch
    automate edit delete-command "ACommandName"
    ```
    Of an element: 
    ``` batch
    automate edit delete-command "ACommandName" --aschildof "{AnElementName}"
    ```

### Add launch points

!!! abstract "Concept"
    A "Launch Point" is the mechanism by which one or more command(s) are executed, and the pattern is applied to a codebase. Every command is contextualized to the element/collection upon which they are defined, but launch points can execute multiple commands from anywhere in the pattern (in an order).

!!! info
    Launch points are triggered manually by the user of the toolkit, but in future, launch points can be triggered by user-based events and other environmental triggers.

To add a new launch point to any element/collection in the pattern:
``` batch
automate edit add-command-launchpoint "<COMMANDIDENTIFIERS>" --aschildof "{<ANEXPRESSION>}"
```

- The `<COMMANDIDENTIFIERS>` is either a `;` delimited list of command identifiers (on the target element/collection) or it can be `*` to indicate that you want to add all the commands (on the `--from` element/collection). By using `*` you can update the list to add or remove any commands that have changed.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are adding the launch point to the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--from` optionally defines another element/collection in the pattern where the commands are located. If this is omitted, then it is assumed that the commands exist on the `--aschildof` element/collection.

- The `--name` optionally defines a friendly name for the launch point. If none is given, a default name will be derived for the launch point.

!!! example
    To the pattern, for all commands on the pattern:
    ``` batch
    automate edit add-command-launchpoint "*" --name "ALaunchPointName"
    ```
    To the pattern, for specific commands on the pattern: 
    ``` batch
    automate edit add-command-launchpoint "ACOMDID1;ACMDID2" --name "ALaunchPointName"
    ```
    To the pattern, for all commands on another element:
    ``` batch
    automate edit add-command-launchpoint "*" --from "{AnotherElementName}" --name "ALaunchPointName"
    ```
    To an element, for all commands on another element: 
    ``` batch
    automate edit add-command-launchpoint "*" --from "{AnotherElementName}" --name "ALaunchPointName" --aschildof "{AnElementName}"
    ```

### Update launch points

To update an existing launch point on any element/collection in the pattern:
``` batch
automate edit update-command-launchpoint "<LAUNCHPOINTNAME>" --add "<COMMANDIDENTIFIERS>" --aschildof "{<ANEXPRESSION>}"
```

- The `<LAUNCHPOINTNAME>` is the name of an existing launch point on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are updating a launch point on the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

- The `--add <COMMANDIDENTIFIERS>` optionally defines the new `;` delimited list of command identifiers (on the target element/collection) or it can be `*` to indicate that you want to add all the commands (on the `--from` element/collection). By using `*` you can update the list to add or remove any commands that have changed.

- The `--from` optionally defines another element/collection in the pattern where the commands are located. If this is omitted, then it is assumed that the commands exist on the `--aschildof` element/collection.

- The `--name` optionally defines a new name for the launch point.

!!! example
    Of the pattern, to re-add all commands on the pattern:
    ``` batch
    automate edit update-command-launchpoint "ALaunchPointName" --add "*" --name "ANewLaunchPointName"
    ```
    Of an element, to re-add all commands from another element: 
    ``` batch
    automate edit update-command-launchpoint "ALaunchPointName" --add "*" --name "ANewLaunchPointName" --from "{AnotherElementName}" --aschildof "{AnElementName}"
    ```

### Delete launch points

To delete an existing launch point on any element/collection in the pattern:
``` batch
automate edit delete-command-launchpoint "<LAUNCHPOINTNAME>" --aschildof "{<ANEXPRESSION>}"
```

- The `<LAUNCHPOINTNAME>` is the name of an existing launch point on the `--aschildof` element/collection.

- The `--aschildof "{<ANEXPRESSION>}"` is only optional if you are deleting the launch point from the root element. `<ANEXPRESSION>` is an [Expression](reference.md#pattern-expressions) to an existing element/collection in the pattern.

!!! example
    Of the pattern:
    ``` batch
    automate edit delete-command-launchpoint "ALaunchPointName"
    ```
    Of an element: 
    ``` batch
    automate edit delete-command-launchpoint "ALaunchPointName" --aschildof "{AnElementName}"
    ```

## Publishing and deploying toolkits

!!! abstract "Concept"
    A "Toolkit" is a portable package (single-file) that contains within it a versioned Pattern (with all its assets and its automation). A Toolkit is used to install a pattern into another codebase, and used to upgrade that pattern when changed later.

### Automatic versioning rules for toolkits

!!! abstract "Concept"
    Every toolkit is "Versioned" with the same version of the pattern. The Pattern is automatically versioned when changes are made to it when it is built into a toolkit. Any change to a pattern (after it has been built into a toolkit) is captured as either a "Breaking" change or a "Non-Breaking" change.

Patterns use a (2-dot) [semantic versioning](https://semver.org/) scheme (i.e. `Major.Minor.Patch`), with no pre-release information.

- A **Breaking** change to a pattern forces the pattern to auto-increment its **major** version number. e.g. deleting an existing attribute, element, or collection.
- A **Non-Breaking** change force the pattern to auto-increment its **minor** number. e.g. adding a new attribute, element, or collection, or updating the contents of a code template (or when the runtime has had a major/breaking upgrade)
- The **patch** number is not used in the automatic versioning process. But it can be used manually.

When a toolkit is built, the next version number can be automatically calculated for you (based on the previous edits to the pattern), or you can force a specific version number of your own (in cases where you want to have a specific release number).

You will need to force a version number change if the given version violates the versioning rules above, or it is a past version number than the one that is auto-calculated.

!!! warning
    Forcing a version number that violates these rules, may result in existing drafts being migrated into an invalid state.

### Installing new versions of toolkits

When a newly versioned toolkit is installed into any codebase (where a previous version of that toolkit already exists), the previous version of the toolkit is automatically replaced/upgraded by the newer version.

Given an upgraded version of the toolkit, it is assumed that there may exist other ["Drafts"](runtime.md#drafts) that would have been created from the previous versions of the toolkit. ["Drafts"](runtime.md#drafts) are bound to the version of the toolkit that created them, and a draft cannot be edited or used by future versions of a toolkit. The "Draft" will be required to be migrated to any future version of a toolkit.

Drafts can be auto-migrated safely in most cases (see: [Upgrading a Draft](runtime.md#upgrading-a-toolkit)), where there are standard Breaking and Non-Breaking changes.

!!! warning
    Forced versions of toolkits and/or forced migrations of drafts may leave the Draft in an invalid state that may require fixing manually, or recreating. Safety in these cases cannot be guaranteed.

### Publishing a toolkit

To build and publish an existing pattern into a toolkit:
``` batch
automate publish toolkit
```

- The `--asversion` optionally defines a custom 2-dot [semver](https://semver.org/) version number to use for this build of the toolkit. Or you can specify the value `auto` to have it automatically versioned, based on the latest changes made to the pattern. The default is `auto`.
- The `--force` optionally bypasses any detected violations when using a custom version number that is suspicious or that would break semantic versioning rules on the existing pattern. The default is `false`
- The `--install` optionally defines whether to install the toolkit into the current directory after it has been built. This is only useful in cases where the creator wants to install the toolkit locally, to avoid the extra installation step.

!!! info
    This command will version and publish the toolkit to a file on the current user's desktop, for distribution.

!!! example
    To create a toolkit file that can be shared: 
    ``` batch
    automate publish toolkit --asversion "2.0.0"
    ```
    To create a toolkit file, for next appropriate version, that is installed into the same codebase: 
    ``` batch
    automate publish toolkit --install
    ```

### Deploying a toolkit

When a toolkit is built, it is exported (as a single file) to the user's desktop e.g. `MyPattern1.0.1.0.toolkit`

From there it can be deployed via any file sharing mechanism e.g. in an email, Slack, Dropbox, etc.

To install a toolkit, see [installing toolkits](runtime.md#installing-a-toolkit).