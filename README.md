[![Build and Test](https://github.com/jezzsantos/automate/actions/workflows/build.yml/badge.svg)](https://github.com/jezzsantos/automate/actions/workflows/build.yml)
__[![Nuget Tool](https://img.shields.io/nuget/v/automate?style=plastic&label=nuget)](https://www.nuget.org/packages/automate)
__[![Discord](https://img.shields.io/badge/automate-%237289DA.svg?style=plastic&logo=discord&logoColor=white)](https://discord.gg/uN8vxaGxRT)
__![Presence](https://dcbadge.vercel.app/api/shield/984584859009814608?style=plastic)

      ┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐
      ├─┤│ │ │ │ ││││├─┤ │ ├┤ 
      ┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘

![The Basic Idea](https://github.com/jezzsantos/automate/blob/main/docs/Images/BasicConcept.png)

**Question to a Developer:**

What if I gave you a command-line tool that wrote a lot of your code for you? and it took about ~5 command-line commands (or so) for you to build out an API endpoint or all the scaffolding for a UI page?

* You would use this tool _once per_ API or once per UI page.

**Question to the Tech Lead on that codebase:**

What if I gave you a command-line tool that built that other command-line tool for that developer? and it took about ~15 command-line commands (or so) for you to build it?

* You would only need to do this _once per codebase_ that you work together on.

What about, if later, that command-line tool was adapted and updated (by you) as the codebase evolved and changed? The next time it is used by the developer, it refactors and automatically fixes the old code for the developer automatically?

What if you had a selection of these kinds of tools stored alongside that code, in that codebase?

# What is Automate?

1. Why? - Consistency, and Timesaving
2. Who? - Tech Leads, Lead Developers, Tech Consultants
3. How? - Give fellow codebase contributors tools that capture and apply their own coding patterns, naming and structure consistently. And evolve as the code changes.

## How is automate different from other similar tools?

![The Process](https://github.com/jezzsantos/automate/blob/main/docs/Images/Concept.png)

Automate may look like other kinds of templating or scaffolding-type developer tools, but there are some important differences (under the covers) that make a big difference in the long run:

1. The toolkits that you build with automate are NOT defined by, nor controlled by, the big vendors of the other software tools you are using (i.e. Microsoft, IBM, JetBrains, etc). Instead, you and your team get to define them. This means they are more specific to the way you write code, and how you want to do things. (*break away from silly sample patterns*).
2. The toolkits that you build with automate will adapt and evolve as the code in your codebase changes and evolves. (*you have full control*)
3. Your toolkits are stored inside your codebase, and source controlled along with your codebase. (*everyone on the codebase gets to use them*).
4. Your toolkits are meant to be used throughout the lifecycle of your codebase, not just used once at project start-up.
5. Your toolkits can chain together ANY developer tooling to help you get your job done better. Not just code generation.
6. Your toolkits can be used with ANY programming language, on any platform, doing any kind of software development.

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
- [ ] You contribute code yourself to this codebase, and you often collaborate with others on your team about the code.
- [ ] You have some working coding patterns or can create some working coding patterns that are worth sharing in this specific codebase.
- [ ] Your team values: consistency, clarity, and maintainability.
- [ ] You accept that you can be wrong sometimes and that your codebase necessarily changes over time.

If this sounds like your situation, then maybe we can help you define (and enforce) some codebase-specific coding patterns for your team to reuse.

# Getting Started

See our [Documentation](https://github.com/jezzsantos/automate/wiki/Documentation) for more details on each of the commands below.

## Making your first toolkit

1. Install the **automate** tool:
    * Platform installers are all here: `https://github.com/jezzsantos/automate/releases`
    * If .NET is installed, install from the command line: `dotnet tool install automate --global` (note: we are in preview at the moment)

2. Navigate to the root of a codebase of yours.

3. Harvest your first pattern from it:

    * `automate create pattern "MyPattern1"`

   > For this example, we are going to assume that you have the following files and folders in the current directory (where you are running this tool). You can adjust the commands below to suit the files of any codebase.
   > ```
    > ¡
    > ├ 📂Controllers
    > │ └ 📜BookingController.cs
    > ```

4. Add attributes, code templates, and automation to make it programmable:

    * `automate edit add-attribute "Name" --isrequired`
    * `automate edit add-codetemplate "Controllers/BookingController.cs"`
    * `automate edit add-codetemplate-command "CodeTemplate1" --targetpath "~/Controllers/{{Name}}Controller.cs"`
    * `automate edit add-command-launchpoint "*" --name "Generate"`

5. Edit the contents of the code template:

    * `automate edit code-template "CodeTemplate1" --with "notepad.exe"`
      > Or use another editor like VS Code [on Windows]: `%localappdata%\Programs\Microsoft VS Code\code.exe`
    * Add the following snippet somewhere in this file:
    ```
    Here is the {{Name}}Contoller.
    ```
    * Save the file

6. Publish a toolkit for this pattern to share with others on your team:

    * `automate publish toolkit`

   > This will build a versioned self-contained toolkit file, and it will publish it to the desktop on your machine, and also provide you with the command to install it into this codebase.

7. Share the toolkit file (`MyPattern1_0.1.0.toolkit`) over email/slack/etc with someone on your team.

---

## Using your first toolkit

Now, that person (or you), can:

1. Install the **automate** tool:
    * Installers are here: `https://github.com/jezzsantos/automate/releases`
    * If .NET is installed, install from the command line: `dotnet tool install automate --global`

2. Install the toolkit, fetched from above (i.e email/slack/etc):

    * `automate install toolkit "<DOWNLOADLOCATION>/MyPattern1_0.1.0.toolkit"`

3. Create a new 'Draft' from this toolkit:

    * `automate run toolkit "MyPattern1" --name "Demo"`

4. Configure it

    * `automate configure on "{MyPattern1}" --and-set "Name=Banana"`

5. Now run the toolkit:

    * `automate execute command "Generate"`

   This command will generate the code from the code template into the destination folder (`--targetpath`), using the value of the `Name` property which you configured as `Banana`

If you now open that generated file, you will also see the value of `Banana` in the contents of the file.

---

## Upgrading your first toolkit

Now, you want to make a change to the toolkit:

1. Add a new attribute to your toolkit:

    * `automate edit add-attribute "Color" --isrequired -isoneof "Red;Green;Blue"`

2. Update the code:

    * `automate edit code-template "CodeTemplate1" --with "notepad.exe"`
    * Add color to this file
    ```
    Here is the {{Name}}Contoller it is {{Color}}.
    ```
    * Save the file

3. Rebuild the toolkit:

    * `automate publish toolkit`

4. Upgrade the toolkit:

    * `automate install toolkit "<DOWNLOADLOCATION>/MyPattern1_0.2.0.toolkit"`

5. Upgrade your draft:

    * `automate upgrade draft`

6. Configure the new color property:

    * `automate configure on "{MyPattern1}" --and-set "Color=Green"`

7. Re-run the toolkit:

    * `automate execute command "Generate"`

If you now open that generated file, you will also see the value of `Green` in the contents of the file.

# More...

Sick of silly examples, and keen to see an example on a real codebase?

* Here is a [demo toolkit](https://github.com/jezzsantos/automate/tree/main/discovery/demo) (built from scratch) with a scripted walk-through on a real codebase which anyone can follow on their machine.

Read our [Documentation](https://jezzsantos.github.io/automate/)

Join the [Discussion](https://discord.gg/vpc3gDPR) on Discord
