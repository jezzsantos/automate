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



