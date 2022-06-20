# Overview

## Installing automate

* Installers (all platforms) are included in the [Releases](https://github.com/jezzsantos/automate/releases) page
* If .NET is installed on your machine: `dotnet tool install automate --global`

## How It Works

There are two distinct modes of use:

### [Making Toolkits](authoring.md)

A Tech Lead will use `automate` to capture and define a coding pattern based on code in an existing codebase.

They will:

1. Name the pattern
2. Configure it structure and its attributes
3. Configure any automation 
4. Publish a toolkit from it
5. Deploy that toolkit to their team to use

### [Using Toolkits](runtime.md)

Contributors on a codebase will then use `automate` to run a toolkit on their codebase.

They will:

1. Install a toolkit locally (unless already installed into the codebase)
2. Create a "draft" use-case with the toolkit
3. Configure the draft (according the pattern)
4. Execute any automation (in the toolkit)

The toolkit applies the templates and automation to their codebase.

## The CLI Interface

<img alt="CLI commands" src="https://github.com/jezzsantos/automate/blob/main/docs/Images/CLI.jpg?raw=true" width="600" />
