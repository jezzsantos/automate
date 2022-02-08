# How Automate will work

## Design Time

look at describe the code we have now



## Run Time

automate will operate on a "toolkit" with commands that start with:

`automate toolkit "AcmeAPI"`
`automate pattern "<PATTERNID>" --options...`

the toolkit of course has to exist first, and it will only exist if it was installed with the command: 

`automate install "C:/Downloads/AcmeAPI.toolkit"`



Automate will need to be present somewhere (on disk), to work with the toolkit.

Automate will read the toolkit file to navigate the pattern model. 

Basically, it will only need to:

* install new toolkit (`install`)
* create a new instance of the pattern (`toolkit`)
* configure the instance of the pattern (`set, add `, etc)
* execute commands (`execute-command`)
* save state on disk

The question is, at this stage, shall we make automate.exe do it all? and only have a notional idea of designtime and runtime? (defined by the specfic commands being used)?
runtime commands are largely going to be prefixed with `automate pattern "<PATTERNID>"`



## Authoring versus Runtime

- You can create a new pattern definition anytime. 
- Only one can be edited at a time. There is a notion of a *current* pattern.
- You can build that pattern into a toolkit anytime, and then it is versioned, and packaged into a toolkit, which is exported.
- You can install that toolkit anywhere (on your machine).
- You can create  new instance of a pattern from any installed toolkit.
- So locally, in any location, you can have one or more patterns under-development, and you can have one or more toolkits installed.

Q. The real question is, after installing a toolkit, is that pattern just like any other pattern that you may created yourself, or are editing locally? In terms of how the pattern works?

Q. Is "toolkit" literally just a packaging format, for exporting the+se patterns?





In NuPattern, we:

1. compiled generated code that runs each toolkit into a standalone binary (vsix) that required the shared services of the runtime to work. This vsix was a plugin to the runtime.
2. deployed that vsix to machines, (and that vsix ensured that the runtime vsix was also deployed)



### places for storing stuff

1. We store the patterns under development in one place (/patterns)
2. We store the installed toolkits in another place (/toolkits)
3. We store the unfolded patterns in another place (/products?)
4. We store the local state of the tool in another place (/state?)

 

In automate, the CLI can be the runtime, and it can know how to deal with the assets in the pattern, by reading the pattern itself.





  

# Architecture

Similar to clean architecture.

- We have a CommandLine (Console App) layer that defines the command line interface. (analogue: WebAPI)
- It delegates command to Application Layer. (analogue: Application Layer)
  - Application Layer manipulates (directly ) a `PatternMetaModel` object (Transactional Script).
  - Application Layer uses `IRepositories` and other services to complete the transaction.
-  

## Improvements

Would be nice to have an aggregate analogue that has all the actions in its interface and does all the work, but then has the ability to be serialised/deserialized (by application layer) into JSON.
An aggregate can have a `Dictionary<string, object>`, and so can a ValueObject which can be used at persistence time. An aggregate can have a method called Dehydrate() and Hydrate(), as can a ValueObject.

The storage layer can call these to serialize and deserialize the aggregate and valueobjects.
Then we can move from a Transactional Script to a Domain.

