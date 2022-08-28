# DraftItem

A DraftItem is the representation of an instance of a Pattern, Element/Collection or Attribute.

It represents an abstraction of both the current state (eg. value of) plus the Schema information about the item.

## Schema

The structure of any DraftItem must comply with the schema of the pattern that underpins it. Every DraftItem has a schema underpinning it.

The underpinning schema is defined as either: `Pattern`, `Element`, or `Attribute` defined by the Toolkit that creates the Draft.

For `DraftItem`s, we further expand the set of possible types (based on some common behaviours) to include: `DraftCollection`.

### DraftPattern

A `DraftItem` that represents a `Pattern`

* There must be one and only one instance of it (essentially `Cardinality=One`)
* It behaves just like any other `Element` apart from the fact that it cannot be deleted (unmaterialised).
* It is always materialised when a draft is created (`AutoCreate =true`).
* It has no `Parent`

### DraftElement

A `DraftItem` that represents an `Element`, where `Element.Cardinality=One or ZeroOrOne`

* There can be at most one instance of it (determined by `Element.Cardinality`)
* It can have one or more properties (DraftAttributes), in its collection of `Properties`
* It can have any number of child DraftElement or DraftCollection, also in its collection of `Properties`

* It can be deleted (unmaterialised).
* It must have a `Parent`, and that parent `DraftItem` is the representation of the logical parent defined in the `PatternDefinition`

### DraftCollection

A `DraftItem` that represents an `Element`, where `Element.Cardinality=ZeroOrMany or OneOrMany`.

Also known as an "Ephemeral Collection", since it has no `Value` (or `Properties`).

* There can be at most one instance of it (essentially `Cardinality=One or ZeroOrOne`)
* It has `ZeroOrMany or OneOrMany` "Collection Items", in its collection of `Items`
    * Each "Collection Item" is itself a DraftElement or DraftCollection, which adopts the schema of the underlying `Element` of this DraftCollection.
* It cannot have any child (DraftAttributes), in its collection of `Properties`
* It cannot have any child DraftElement or DraftCollection, in its collection of `Properties`
* It can be deleted (unmaterialised).
* It must have a `Parent`, and that parent `DraftItem` is the representation of the logical parent defined in the `PatternDefinition`

#### DraftCollectionItem

A `DraftItem` that represents a `DraftElement` or `DraftCollection` that is in the `Items` collection of another `DraftCollection`.

* It must have a `Parent`, and that parent `DraftItem` is not the containing `DraftCollection`, but the next ancestor that is not a `DraftCollection`. It has an `ImmediateParent` that is the containing `DraftCollection`.

### DraftAttribute

A `DraftItem` that represents an `Attribute`.

* There can be at most one instance of it
* It cannot have any "Collection Items" in its collection on `Items`
* It cannot have any child (DraftAttributes), in its collection of `Properties`
* It cannot have any child DraftElement or DraftCollection, in its collection of `Properties`
* It cannot be deleted (unmaterialised) independently from its `Parent`.
* It must have a `Parent`, and that parent `DraftItem` is the representation of the logical parent defined in the `PatternDefinition`

## Lifecycle

When a Draft is created, all of its `DraftItem`s for all of its `Schema`, and all of its descendant schema, is instantiated.

This means that all the leaves of the initial tree (with the DraftItem as the root) are built initially.

This is done as a strategy to manage the future addition and subtraction of leaves to the tree. In other words, to know (ahead of time) what those leaves could be and where they are on the tree.

In order to keep track of whether an instance is "assigned" we use the notion of "[Materialisation](#Materialisation)".

Only DraftItems for schema that permit creation initially are instantiated. For example, the Pattern, all its Attributes, and any Elements/Collections  (and all their attributes) where `AutoCreate=true`, are created initially.  Even though Collections may be created, no items are created.

### Materialisation

A DraftItem maintains a state of its own materialisation.

* Materialised, or
* UnMaterialised

Materialised DraftItem:

* A DraftItem has a `Value` of some sort.
    * That would be a value that the user could "see" and interact with.
        * For example, if you were to view the "Configuration" of the DraftItem, then only materialised Values would be present in the output.
* A DraftItem has a `Structure` of some sort.
    * For an Attribute, and a ephemeral Collection this is nothing
    * For Pattern/Element/CollectionItem, they will always have materialised Attributes, and descendant Element/Collections.
* The user can view, and interact with anything that is Materialised

UnMaterialised DraftItem:

* The `Value` will always null
* All structure of the current Pattern/Attribute/Element/Collection/CollectionItem is also UnMaterialised.
* For Collections, their Items are always cleared.
* The user cannot view or interact with anything that is UnMaterialised 