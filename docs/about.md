# Overview

## Installing automate

* Installers (all platforms) are included in the [Releases](https://github.com/jezzsantos/automate/releases) page
* If .NET is installed on your machine: `dotnet tool install automate --global`

## How It Works

There are two distinct modes of use of `automate`.

### [Creating Toolkits](authoring.md)

A Tech Lead will use `automate` to capture and configure a coding pattern from an existing codebase.

They will name the pattern, configure it structure and its attributes and its automation, version it and create toolkit out of it.

They will then deploy that toolkit to their team to use.

### [Using Toolkits](runtime.md)

Contributors on a codebase will use `automate` to install a toolkit locally, and then run the toolkit on an existing codebase to create and configure drafts that apply the templates and automation to the codebase.

![CLI](https://github.com/jezzsantos/automate/blob/main/docs/Images/CLI.jpg?raw=true)