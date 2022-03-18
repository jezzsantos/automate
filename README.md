[![Build and Test](https://github.com/jezzsantos/automate/actions/workflows/build.yml/badge.svg)](https://github.com/jezzsantos/automate/actions/workflows/build.yml)

      ┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐
      ├─┤│ │ │ │ ││││├─┤ │ ├┤ 
      ┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘

**Question to a Team Developer:**

What if I gave you a command line tool, that wrote most of your code for you? and it took about ~5 commands (or less)? to build an API endpoint, or all the scaffolding for a UI page?

* You would use this tool _once per_ API or once per UI page.

**Question to the Team's Tech Lead:**

What if I gave you a command line tool, that built that command line tool for your developer? and it took about ~15 commands (or less) to build it?

* You would only need to do this _once per codebase_ you work together on.

What if that command line tool was updated (by you) as your codebase evolved? and it refactored and fixed the code for the developer automatically?

What if you had a library of these tools that could be used on other codebases?

## What is Automate?

Automate is a tool for Tech Leads, Lead Developers or Tech Consultants to give their fellow codebase contributors dev tools that capture and apply their coding patterns consistently across to their codebases, as they evolve and change.

## How does it work?

It works like this:

1. You identify a *pattern* in your codebase. eg. It might be a layer pattern (an interface) or a vertical slice pattern (a domain pattern), or a way to compose certain components together that is done similarly in multiple places.
2. You pick (from your codebase) the files and folders where the code exists for this pattern right now.
3. Those files are automatically extracted for you into a *template*.
4. You name the pattern, and you define some *attributes* for it. The things that could be variable in it, when it gets used elsewhere in the codebase. (eg: the names of functions, classes, types, file, folders in the pattern etc.)
5. You modify the templates to add *substitutions* (and calculations) for the attributes that you defined, that will be executed when the template is *rendered*.
6. You can define automation *commands* that can be executed on the codebase before and after the template is applied. eg. you want to run some automated tests to verify the code has not been broken.
7. Optionally, you define *constraints* about where the pattern can be re-used in a codebase (eg. only in certain languages etc).
8. You publish your pattern. A *toolkit* is automatically built for you containing this template, and containing a custom executable tool (CLI or IDE plugin) to help apply it onto another codebase.
10. You ship this toolkit to your dev teams to use.
11. They download and run the toolkit (CLI or IDE plugin), and they construct an instance of the template and fill values for the attributes that were defined in it, configured for their specific use case and their codebase.
12. The toolkit renders the template into: code, files, folders etc, into their codebase automatically.

### Dealing with change

At some point later, you will want to update the pattern. Refactor it, modify it, or fix a defect in it, or just add new capabilities to it etc.

1. You edit the pattern, edit or add more templates, optionally change the commands and change the constraints.
2. An upgraded toolkit version is created for you automatically containing this upgrade.
2. You ship this upgraded toolkit to your team to use.
2. They download and run the upgraded toolkit (CLI or IDE plugin) and upgrade their codebase.
2. The toolkit automatically detects the previous files/folders that were written before, and detects the previous attribute configuration that was used before. The toolkit re-applies the upgraded pattern. The codebase evolves.

## What problem does it solve?

Often, for many software teams (that include individuals varying degrees of expertise from novice to expert) a Tech Lead/Lead Dev/Tech Consultant needs to make some key technical decisions about how things need to get done in that codebase. Architecturally , structurally or in the details of the implementation - all these decisions are constraints on the codebase. Keeping things small, well defined, and consistent is often the key to managing complexity over time. So that contributors to this codebase can have a single, unified and simpler understanding of the codebase -> A reasonable, shared mental model of the codebase, so that they can move freely across the codebase and not fear changing any part of it.

Being able to construct codebases in this way is earned over the years of doing similar things in other codebases over and over again. As this is done patterns emerge and they are adapted and improved for each codebase where they are applied. No two codebases are the same, but the patterns are often very similar. Often improving over time as those that define and refine them learn more each time they are applied from the context of the current codebase.

The primary challenge a Tech Lead/Lead Dev/Tech Consultant has is: communicating this knowledge in a form that can be learnt and reused by others, and also adapted for the current codebase. Learning by doing is key, but learning from experience through demonstration is an effective way we can avoid repeating the lessons learned and expensive mistakes from the past.

One of the main challenges in codebases for team is that others in the codebase will naturally re-invent unfamiliar or more novel code patterns because they lack the knowledge that other team members have, or they are unaware that the other team members already possesses this kind of knowledge. Teams are also fluid over time, with members coming and going. So the learning process is continuous and relentless.

Another major challenge is not knowing how to solve certain problems in the codebase nor where to put certain solutions in the code. This results in solutions being peppered around the codebase as workarounds are introduced, and pilled upon each other. This leads to the big-ball-of-mud which becomes un-navigable for a team as time moves forward.

A Tech Lead/Lead Dev/Tech Consultant needs to remain vigilant to ensure that established patterns and architectures are not violated because of lack of knowledge of other team members. Whilst, they also need to change as the software evolves, and as new constraints are defined.

Today's, languages and development tools (IDEs) are so *general purpose* that they cannot be used to help enforce or constrict the programmer from working around these implied constraints. Codebase specific tools, fit for purpose, must be used to enforce these constraints.

Tech Lead/Lead Dev/Tech Consultant could use a little help from their codebase specific tools.

### Does this apply to you?

If these assumptions about your software team are **all true**, then you might consider taking a look at this tool.

- [ ] You work on a codebase with others - you are working in a software team.

- [ ] You are the Tech Lead/Lead Dev/Tech Consultant of the team. (or have some other well respected, position of responsibility in the team).
- [ ] You contribute code yourself to this codebase, and you communicate often with others on your team about the code.
- [ ] You have some coding patterns, or can create some coding patterns, that are worth sharing in this specific codebase.
- [ ] Your team values: consistency, clarity, and maintainability.
- [ ] You accept that you can be wrong sometimes, and that your codebase necessarily changes over time.

If this sounds like your situation then maybe we can help you define (and enforce) some codebase specific coding patterns for your team to reuse.

# Getting Started

This project is in early prototype stage. Take a look at our [prototype scenario](discovery/prototypes/cli/Syntax.md) for a detailed example of use.

1. Install the tool:
    * Installers are here: `https://github.com/jezzsantos/automate/releases`
    * Otherwise, if .NET installed: `dotnet tool install --global automate`

2. Navigate to your codebase, and harvest your first pattern from it.

   `automate create pattern "MyPattern1"`

3. Add attributes, elements and code templates, and make it programmable.

   `automate edit add-attribute "Name" --isrequired`
   `automate edit add-codetemplate "backend/controllers/BookingController.cs"`
   etc.

5. Generate a toolkit/plugin for applying the pattern.

   `automate build toolkit`

6. Share the built toolkit with your team/community to re-use.

# Learn more

At present we are in a validation stage. Validation, viability and research work is captured in the `docs` and `discovery` folders.
