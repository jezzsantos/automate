[![Build and Test](https://github.com/jezzsantos/automate/actions/workflows/build.yml/badge.svg)](https://github.com/jezzsantos/automate/actions/workflows/build.yml)
__[![Nuget Tool](https://img.shields.io/nuget/v/automate?style=plastic&label=nuget)](https://www.nuget.org/packages/automate)
__[![Discord](https://img.shields.io/badge/automate-%237289DA.svg?style=plastic&logo=discord&logoColor=white)](https://discord.gg/uN8vxaGxRT)

      ┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐
      ├─┤│ │ │ │ ││││├─┤ │ ├┤ 
      ┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘

![The Basic Idea](https://raw.githubusercontent.com/jezzsantos/automate/main/docs/Images/BasicConcept.png)

**Question to a Developer:**

What if I gave you a command-line tool that wrote a lot of your code for you? and it took about ~5 command-line commands (or so) for you to build out an API endpoint or all the scaffolding for a UI page?

* You would use this tool _once per_ API or once per UI page.

**Question to the Tech Lead on that codebase:**

What if I gave you a command-line tool that built that other command-line tool for that developer? and it took about ~15 command-line commands (or so) for you to build it?

* You would only need to do this _once per codebase_ that you work together on.

What about, if later, that command-line tool was adapted and updated (by you) as the codebase evolved and changed? The next time it is used by the developer, it refactors and automatically fixes the old code for the developer automatically?

What if you had a selection of these kinds of tools stored alongside that code, in that codebase?

# What is Automate?

1. Why? - Consistency, and  tim saving
2. Who? - Tech Leads, Lead Developers, Tech Consultants
3. How? - Give fellow codebase contributors tools that capture and apply their own coding patterns, naming and structure consistently. And evolve as the code changes.

# How does it work?

It works like this:

1. You identify a *pattern* in your codebase. eg. It might be a layering pattern (an interface) or a vertical slice pattern (a domain pattern) or a certain kind of component, or a way to compose certain components together that is done similarly in multiple places, etc.
2. You pick (from your codebase) the files and folders where the code exists for this pattern right now.
3. Those files are then automatically extracted for you into *code templates*.
4. You name the pattern, and you define some *attributes* for it. Attributes are the things that represent what could be variable in the pattern when it gets applied in the codebase. (e.g. the names of functions, classes, types, files, folders in the pattern, etc.)
5. You modify the code templates to add *substitutions* for the attributes that you defined, which will be calculated when the code templates are *rendered*.
6. Optionally, you can define other *commands* that can be executed on the codebase before and after the code templates are applied. e.g. you want to execute a package manager to install something, or maybe run some automated tests to verify the code has not been broken, etc.
7. Optionally, you define *constraints* about where the pattern can be reused in a codebase (e.g. only in certain languages or locations, etc.)
8. You "build" your pattern, and a portable *toolkit* is automatically created for you containing these templates and automation.
9. You ship this new toolkit to your dev teams to install into their codebases, or you install it into an existing codebase they are using for them.
10. They then use the toolkit to construct a usage of the pattern (called a "draft") and configure it for their specific use case in their workflow.
11. The toolkit renders the code templates into files and folders in the codebase with information contained within the draft, and executes the relevant automation etc.

## Dealing with change

At some point later (inevitably), you will want to update the pattern. Refactor it, modify it, fix a defect in it, or just add new capabilities to it, etc.

1. You simply edit the original pattern. You edit or add more code templates, optionally add or change the automation and update the constraints.
2. An upgraded toolkit version is then built for you automatically containing these upgrades.
2. You ship this upgraded toolkit to your team (or codebase) to use.
2. They run the upgraded toolkit and upgrade the code in their codebase.
2. The toolkit automatically detects the previous files/folders that were written before and detects the previous draft configuration that was used before. The toolkit re-applies the upgraded pieces. The codebase evolves.

## Does automate apply to you?

If these assumptions about your software team are **all true**, then you might consider taking a look at this tool.

- [ ] You work on a codebase with others - you are working in a software team.

- [ ] You are the Tech Lead/Lead Dev/Tech Consultant of the team. (or have some other well-respected position of authority in the team).
- [ ] You contribute code yourself to this codebase, and you often collaborate with others on your team about how the code is structured or written.
- [ ] You already have some defined coding patterns or can create some coding patterns that are worth repeating in this specific codebase.
- [ ] Your team values: consistency, clarity, and maintainability.
- [ ] You accept that code changes over time and keeping things up to date is important.

If this sounds like your situation, then maybe we can help you define (and enforce) some codebase-specific coding patterns for your team to reuse.

# Getting Started

See our [Getting Started](https://jezzsantos.github.io/automate) documentation to see how to install automate.

## Making your first toolkit

Follow this tutorial to [make your first toolkit](https://jezzsantos.github.io/automate/tutorial)

# More...

Sick of silly examples, and keen to see an example on a real codebase?

* Here is a [demo toolkit](https://github.com/jezzsantos/automate/tree/main/discovery/demo) (built from scratch) with a scripted walk-through on a real codebase which anyone can follow on their machine.

Read our [Documentation](https://jezzsantos.github.io/automate/)

What to contribute? We sure welcome you!

See our [Contributing Guidelines](CONTRIBUTING.md).

Join the [Discussion](https://discord.gg/vpc3gDPR) on Discord
